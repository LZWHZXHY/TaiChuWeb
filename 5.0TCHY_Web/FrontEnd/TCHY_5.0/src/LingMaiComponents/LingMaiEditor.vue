<template>
  <div class="editor-scroll-container" @click="handleEditorClick">
    <div class="editor-header">
      
      <div class="header-toolbar">
        <div class="view-switcher">
          <button :class="{ active: viewMode === 'doc' }" @click="viewMode = 'doc'">
            <span class="icon">📄</span> 文档
          </button>
          <button :class="{ active: viewMode === 'kanban' }" @click="viewMode = 'kanban'">
            <span class="icon">📊</span> 看板
          </button>
        </div>
        <div class="spacer"></div>
        </div>

      <input 
        v-model="note.title" 
        class="title-field" 
        placeholder="无标题" 
        @blur="syncTitle" 
        @click.stop
      />
      
      <div class="meta-info">
        <span class="time-label">上次更新</span> 
        {{ formatDate(note.updatedAt) }}
      </div>
    </div>

    <div v-if="viewMode === 'doc'" class="fade-in doc-layout">
      <div class="editor-body">
        <LingMaiBubbleMenu v-if="editor" :editor="editor" />
        <editor-content :editor="editor" />
      </div>

      <aside class="toc-sidebar" v-if="tocItems.length > 0">
        <div class="toc-header">大纲</div>
        <div class="toc-list">
          <div 
            v-for="(item, index) in tocItems" 
            :key="index"
            class="toc-item"
            :class="[`level-${item.level}`, { active: activeHeadingIndex === index }]"
            @click.stop="scrollToHeading(item.pos)"
          >
            {{ item.text }}
          </div>
        </div>
      </aside>

      <div class="backlinks-section" v-if="backlinks.length">
        <div class="section-title">🔗 引用了此页面的文档 ({{ backlinks.length }})</div>
        <div class="backlink-list">
          <div v-for="bl in backlinks" :key="bl.Id" class="backlink-card" @click="handleBacklinkClick(bl.Id)">
            <div class="bl-title">📄 {{ bl.Title }}</div>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="kanban-wrapper fade-in">
       <KanbanBoard :parent-id="note.id" @open-card="handleNavigate" />
    </div>

    <div class="sync-indicator" :class="{ 'is-syncing': syncing }">
      <span v-if="syncing">☁️ 保存中...</span>
      <span v-else>✅ 已保存</span>
    </div>
  </div>
</template>

<script setup>
import { onBeforeUnmount, ref, reactive, watch } from 'vue'
import { useEditor, EditorContent, VueRenderer } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Placeholder from '@tiptap/extension-placeholder'
import Mention from '@tiptap/extension-mention'
import Image from '@tiptap/extension-image'
import tippy from 'tippy.js'
import apiClient from '@/utils/api'
import SuggestionList from '@/LingMaiComponents/SuggestionList.vue'
import SlashCommand from './slashCommand' 
import KanbanBoard from './KanbanBoard.vue' 
import { Details, Summary } from '@/LingMaiComponents/ToggleNodes.js' 
import { TextStyle } from '@tiptap/extension-text-style'
import { Color } from '@tiptap/extension-color'
import { Highlight } from '@tiptap/extension-highlight'
import { BubbleMenu as BubbleMenuExtension } from '@tiptap/extension-bubble-menu'
import LingMaiBubbleMenu from './LingMaiBubbleMenu.vue'
import Underline from '@tiptap/extension-underline'
import { FontSize } from './FontSize.js'

const props = defineProps(['noteId'])
const emit = defineEmits(['navigate', 'deleted']) 

const viewMode = ref('doc')
const tocItems = ref([])
const activeHeadingIndex = ref(-1)

const handleNavigate = (id) => { emit('navigate', id); viewMode.value = 'doc' }

const note = reactive({ id: props.noteId, title: '', updatedAt: new Date(), parentNoteId: null })
const syncing = ref(false)
const backlinks = ref([])

const uploadImage = async (file) => {
  const formData = new FormData(); formData.append('file', file)
  try { const res = await apiClient.post('/Upload/image', formData, { headers: { 'Content-Type': 'multipart/form-data' } }); return res.data.url } catch (e) { return null }
}

const updateToc = (editorInstance) => {
  const items = []; editorInstance.state.doc.descendants((node, pos) => { if (node.type.name === 'heading') items.push({ level: node.attrs.level, text: node.textContent, pos }) }); tocItems.value = items
}

const scrollToHeading = (pos) => {
  if (!editor.value) return
  let dom = editor.value.view.nodeDOM(pos); if (dom && dom.nodeType === 3) dom = dom.parentElement; if (!dom) return
  const scrollContainer = document.getElementById('main-scroll-container')
  if (!scrollContainer) { dom.scrollIntoView({ behavior: 'smooth', block: 'center' }); return }
  const offset = 140; const targetScrollTop = scrollContainer.scrollTop + (dom.getBoundingClientRect().top - scrollContainer.getBoundingClientRect().top) - offset
  scrollContainer.scrollTo({ top: targetScrollTop, behavior: 'auto' })
}

const editor = useEditor({
  extensions: [
    StarterKit, Placeholder.configure({ placeholder: '输入 / 唤起命令，输入 [[ 建立关联...' }), SlashCommand, Image.configure({ inline: true }), Details, Summary, TextStyle, Color, Underline, FontSize, Highlight.configure({ multicolor: true }), BubbleMenuExtension.configure({ element: null }),
    Mention.configure({
      HTMLAttributes: { class: 'internal-link' },
      renderHTML({ options, node }) { return ['span', { class: 'internal-link', 'data-id': node.attrs.id, style: 'color: #0078d4; cursor: pointer; text-decoration: underline; font-weight: 500;' }, `@${node.attrs.label ?? node.attrs.id}`] },
      suggestion: {
        char: '[[', items: async ({ query }) => { try { const res = await apiClient.get(`/Notes/search?query=${encodeURIComponent(query || '')}`); return res.data.map(n => ({ id: n.Id, label: n.Title })) } catch (e) { return [] } },
        render: () => {
          let component, popup; return {
            onStart: props => { component = new VueRenderer(SuggestionList, { props, editor: props.editor }); popup = tippy('body', { getReferenceClientRect: props.clientRect, appendTo: () => document.body, content: component.element, showOnCreate: true, interactive: true, trigger: 'manual', placement: 'bottom-start' }) },
            onUpdate(props) { component.updateProps(props); popup[0].setProps({ getReferenceClientRect: props.clientRect }) },
            onKeyDown(props) { if (props.event.key === 'Escape') { popup[0].hide(); return true } return component.ref?.onKeyDown(props) },
            onExit() { popup[0].destroy(); component.destroy() }
          }
        }
      }
    })
  ],
  editorProps: {
    handlePaste: (view, event) => {
      const items = (event.clipboardData || event.originalEvent.clipboardData).items
      for (const item of items) { if (item.type.indexOf('image') === 0) { event.preventDefault(); const file = item.getAsFile(); uploadImage(file).then(url => { if (url) { const { schema } = view.state; const node = schema.nodes.image.create({ src: url }); view.dispatch(view.state.tr.replaceSelectionWith(node)) } }); return true } } return false
    },
    handleDrop: (view, event, slice, moved) => {
      if (!moved && event.dataTransfer && event.dataTransfer.files.length > 0) { const file = event.dataTransfer.files[0]; if (file.type.indexOf('image') === 0) { event.preventDefault(); const coordinates = view.posAtCoords({ left: event.clientX, top: event.clientY }); uploadImage(file).then(url => { if (url) { const { schema } = view.state; const node = schema.nodes.image.create({ src: url }); view.dispatch(view.state.tr.insert(coordinates.pos, node)) } }); return true } } return false
    }
  },
  onUpdate: ({ editor }) => { debouncedSave(editor.getJSON()); updateToc(editor) },
  onCreate: ({ editor }) => { updateToc(editor) }
})

const loadData = async (id) => {
  if (!editor.value) return
  try {
    const res = await apiClient.get(`/Notes/detail/${id}`); note.title = res.data.Title || ''; note.updatedAt = res.data.UpdatedAt; note.parentNoteId = res.data.ParentNoteId; const content = res.data.ContentJson
    if (content && content !== '""') { try { editor.value.commands.setContent(JSON.parse(content)); updateToc(editor.value) } catch (e) { editor.value.commands.setContent('') } } else { editor.value.commands.setContent('') }
    loadBacklinks(id)
  } catch (e) { note.title = "加载失败"; editor.value?.commands.setContent(`<p>无法加载笔记内容: ${e.message}</p>`) }
}

const loadBacklinks = async (id) => { try { const blRes = await apiClient.get(`/Notes/${id}/backlinks`); backlinks.value = blRes.data } catch (e) { backlinks.value = [] } }

let timer = null
const debouncedSave = (json) => {
  clearTimeout(timer); syncing.value = true
  timer = setTimeout(async () => { try { await apiClient.post('/Notes/save', { id: note.id, title: note.title, contentJson: JSON.stringify(json), parentNoteId: note.parentNoteId }); note.updatedAt = new Date() } catch (e) { console.error(e) } finally { syncing.value = false } }, 2000)
}
const syncTitle = () => { if (editor.value) debouncedSave(editor.value.getJSON()) }

// 只处理空白处的点击
const handleEditorClick = (event) => {
  const target = event.target.closest('.internal-link'); if (target) { emit('navigate', target.getAttribute('data-id')); return }
  // input 上已经加了 @click.stop，这里其实是一个双重保险
  if (event.target.closest('input') || event.target.closest('button')) return
  if (editor.value && !editor.value.isFocused) editor.value.chain().focus().run()
}

const handleBacklinkClick = (targetId) => emit('navigate', targetId)
const formatDate = (d) => d ? new Date(d).toLocaleString('zh-CN', { hour12: false }) : ''

watch([() => props.noteId, editor], ([newId, newEditor]) => { if (newId && newEditor) { note.id = newId; loadData(newId) } }, { immediate: true })
onBeforeUnmount(() => { if (timer) clearTimeout(timer); editor.value?.destroy() })
</script>

<style lang="scss">
/* 保持原有 CSS 不变，因为样式是没问题的 */
.editor-scroll-container { max-width: 900px; margin: 0 auto; padding: 40px 60px; min-height: 100%; background: #fff; position: relative; cursor: text; }
.doc-layout { position: relative; }
.toc-sidebar { position: fixed; top: 50%; transform: translateY(-50%); right: 40px; width: 200px; max-height: 70vh; overflow-y: auto; border-left: 2px solid #f0f0f0; padding-left: 15px; z-index: 10; @media (max-width: 1400px) { display: none; } }
.toc-header { font-size: 13px; font-weight: 700; color: #37352f; margin-bottom: 12px; opacity: 0.6; }
.toc-list { display: flex; flex-direction: column; gap: 2px; }
.toc-item { font-size: 13px; color: #666; cursor: pointer; padding: 4px 8px; border-radius: 4px; transition: all 0.2s; line-height: 1.5; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; &:hover { background-color: #f3f3f3; color: #000; } &.active { background-color: #e6f7ff; color: #1890ff; } &.level-1 { font-weight: 600; color: #333; } &.level-2 { padding-left: 16px; } &.level-3 { padding-left: 28px; font-size: 12px; } &.level-4 { padding-left: 36px; font-size: 12px; color: #999; } }
.toc-sidebar::-webkit-scrollbar { width: 4px; } .toc-sidebar::-webkit-scrollbar-thumb { background: #eee; border-radius: 4px; }
.editor-header { margin-bottom: 40px; border-bottom: 1px solid rgba(0,0,0,0.06); padding-bottom: 20px; }
.header-toolbar { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; }
.view-switcher { display: inline-flex; background-color: #f3f3f3; padding: 3px; border-radius: 8px; position: relative; border: 1px solid rgba(0,0,0,0.04); button { position: relative; z-index: 2; border: none; background: transparent; padding: 6px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500; color: #666; display: flex; align-items: center; gap: 6px; transition: color 0.2s ease; .icon { font-size: 14px; } &:hover { color: #333; } &.active { color: #000; background: #fff; box-shadow: 0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06); } } }
.title-field { width: 100%; font-size: 40px; font-weight: 700; border: none; outline: none; margin-bottom: 8px; color: #111; background: transparent; line-height: 1.2; &::placeholder { color: #e5e5e5; } }
.meta-info { font-size: 12px; color: #999; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; .time-label { margin-right: 6px; opacity: 0.7; } }
.ProseMirror { outline: none; min-height: 400px; font-size: 16px; line-height: 1.75; color: #37352f; margin-top: 20px; padding-bottom: 30vh; h1, h2, h3, h4, h5, h6 { scroll-margin-top: 140px; } > * { position: relative; transition: background-color 0.2s ease; border-radius: 4px; margin-left: -12px; padding-left: 12px; border-left: 3px solid transparent; } > *:hover { border-left-color: rgba(0, 0, 0, 0.08); background-color: rgba(0, 0, 0, 0.01); } .ProseMirror-selectednode { outline: 2px solid #b4d5fe; background-color: transparent !important; } p.is-editor-empty:first-child::before { color: #9ca3af; content: attr(data-placeholder); float: left; height: 0; pointer-events: none; font-style: italic; opacity: 0.7; } blockquote { border-left: 3px solid #333; padding-left: 14px; margin: 1.5em 0; font-style: italic; color: #555; background: transparent; } pre { background: #f7f6f3; border-radius: 4px; padding: 16px; font-family: monospace; code { background: none; color: inherit; } } code { background-color: rgba(97, 97, 97, 0.1); color: #eb5757; padding: 0.25rem; border-radius: 4px; font-size: 0.85rem; } img { max-width: 100%; height: auto; border-radius: 4px; margin: 10px 0; box-shadow: 0 2px 8px rgba(0,0,0,0.1); &.ProseMirror-selectednode { outline: 2px solid #0078d4; } } details { border: 1px solid #e2e8f0; border-radius: 6px; padding: 8px 12px; margin: 10px 0; background-color: #fbfbfa; transition: all 0.2s ease; &[open] { background-color: #fff; border-color: #d1d5db; box-shadow: 0 2px 5px rgba(0,0,0,0.02); } summary { font-weight: 600; cursor: pointer; outline: none; color: #37352f; padding: 4px 0; &::marker { color: #9ca3af; font-size: 0.8em; transition: color 0.2s; } &:hover { color: #2383e2; &::marker { color: #2383e2; } } } div { margin-top: 8px; padding-left: 14px; border-left: 2px solid #f3f4f6; color: #4b5563; } } }
.backlinks-section { margin-top: 80px; border-top: 1px solid #eaeaea; padding-top: 24px; .section-title { font-size: 14px; font-weight: 600; color: #37352f; margin-bottom: 16px; text-transform: uppercase; } .backlink-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 12px; } .backlink-card { padding: 12px 16px; border: 1px solid #eaeaea; border-radius: 6px; cursor: pointer; transition: all 0.2s; background: #fff; &:hover { border-color: #d4d4d4; box-shadow: 0 4px 12px rgba(0,0,0,0.05); transform: translateY(-1px); } .bl-title { font-size: 14px; color: #333; font-weight: 500; } } }
.sync-indicator { position: fixed; bottom: 20px; right: 20px; font-size: 12px; color: #999; background: #fff; padding: 6px 12px; border-radius: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); pointer-events: none; transition: all 0.3s; border: 1px solid #eee; &.is-syncing { color: #2383e2; border-color: #2383e2; } }
.fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
.kanban-wrapper { margin-top: 20px; }
</style>