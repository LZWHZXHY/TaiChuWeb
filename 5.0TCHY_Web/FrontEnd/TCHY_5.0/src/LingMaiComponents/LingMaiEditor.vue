<template>
  <div class="editor-scroll-container" @click="handleEditorClick">
    <div class="editor-header">
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

    <div class="fade-in doc-layout" ref="editorAreaRef">
      <div class="editor-body preview-target-area">
        <LingMaiBubbleMenu v-if="editor" :editor="editor" />
        <editor-content :editor="editor" />
      </div>

      <aside class="toc-sidebar" v-if="tocItems.length > 0">
        <div class="toc-trigger">
          <span class="trigger-icon">📑</span>
          <span class="trigger-text">目录</span>
        </div>
        
        <div class="toc-content-wrapper">
          <div class="toc-header">文档大纲</div>
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

    <div class="sync-indicator" :class="{ 'is-syncing': syncing }">
      <span v-if="syncing">☁️ 保存中...</span>
      <span v-else>✅ 已保存</span>
    </div>
  </div>
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref, reactive, watch, createVNode, render } from 'vue'
import { useEditor, EditorContent, VueRenderer } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Placeholder from '@tiptap/extension-placeholder'
import Mention from '@tiptap/extension-mention'
import Image from '@tiptap/extension-image'
import tippy from 'tippy.js'
import apiClient from '@/utils/api'
import SuggestionList from '@/LingMaiComponents/SuggestionList.vue'
import SlashCommand from './slashCommand' 
import { Details, Summary } from '@/LingMaiComponents/ToggleNodes.js' 
import { TextStyle } from '@tiptap/extension-text-style'
import { Color } from '@tiptap/extension-color'
import { Highlight } from '@tiptap/extension-highlight'
import { BubbleMenu as BubbleMenuExtension } from '@tiptap/extension-bubble-menu'
import LingMaiBubbleMenu from './LingMaiBubbleMenu.vue'
import Underline from '@tiptap/extension-underline'
import { FontSize } from './FontSize.js'
import KanbanNode from './KanbanNode.js'
import { delegate } from 'tippy.js'
import LinkPreviewCard from '@/LingMaiComponents/LinkPreviewCard.vue'
import { Table } from '@tiptap/extension-table'
import { TableRow } from '@tiptap/extension-table-row'
import { TableCell } from '@tiptap/extension-table-cell'
import { TableHeader } from '@tiptap/extension-table-header'
import HeatmapNode from './HeatmapNode.js'
import TaskList from '@tiptap/extension-task-list'
import TaskItem from '@tiptap/extension-task-item'
import { BlockId } from './BlockId'
// 找到第 91 行，修改为：
import BlockEmbed from './BlockEmbedNode.js' // 将 BlockEmbedNode 改为 BlockEmbed



const props = defineProps(['noteId'])
const emit = defineEmits(['navigate', 'deleted']) 

const tocItems = ref([])
const activeHeadingIndex = ref(-1)

const note = reactive({ id: props.noteId, title: '', updatedAt: new Date(), parentNoteId: null })
const syncing = ref(false)
const backlinks = ref([])
const editorAreaRef = ref(null)
let tippyInstance = null

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
    KanbanNode,
    StarterKit, 
    BlockId, // 🔥 必须加上，否则新块没有 ID
    BlockEmbed, // 🔥 联动块渲染器
    HeatmapNode,
    Placeholder.configure({ placeholder: '输入 / 唤起命令，输入 [[ 建立关联...' }), 
    SlashCommand, 
    TaskList,
    TaskItem.extend({
      // 🔥 核心修复：覆盖默认的 'paragraph block*'，允许任意块级元素打头
      content: 'block+', 
    }).configure({
      nested: true,
    }),
    Image.configure({ inline: true }), 
    Details, 
    Summary, 
    Table.configure({ resizable: true }),
    TableRow,
    TableHeader,
    TableCell,
    TextStyle, 
    Color, 
    Underline, 
    FontSize, 
    Highlight.configure({ multicolor: true }), 
    BubbleMenuExtension.configure({ element: null }),


    Mention.configure({
  HTMLAttributes: {
    class: 'internal-link',
  },
  suggestion: {
    char: '[[',
    // 1. 数据获取逻辑
    items: async ({ query }) => {
      // 调试：在控制台查看 Tiptap 传过来的 query 到底是什么
      console.log('当前 Mention Query:', query);

      // 核心修复：Tiptap 的 query 不带 [[。输入 [[# 时，query 是 "#"
      if (query && query.startsWith('#')) {
        const blockQuery = query.substring(1); // 截取 # 之后的关键字
        
        try {
          console.log('检测到块搜索标识，正在发起请求...', blockQuery);
          // 发起请求到后端 LingMaiBlockController
          const res = await apiClient.get(`/Blocks/search?query=${encodeURIComponent(blockQuery)}`);
          
          return res.data.map(b => ({
            id: b.blockId,
            label: b.previewText,
            type: 'block', // 关键标识：用于 command 判断插入类型
            noteTitle: b.noteTitle
          }));
        } catch (e) {
          console.error('块搜索请求失败:', e);
          return [];
        }
      }

      // 默认逻辑：普通文档搜索
      try {
        const res = await apiClient.get(`/Notes/search?query=${encodeURIComponent(query || '')}`);
        return res.data.map(n => ({
          id: n.Id,
          label: n.Title,
          type: 'note'
        }));
      } catch (e) {
        console.error('文档搜索请求失败:', e);
        return [];
      }
    },

    // 2. 插入逻辑：根据 type 决定是插入 Mention 还是 BlockEmbed
    command: ({ editor, range, props }) => {
      if (props.type === 'block') {
        // 选中块：插入联动节点并自动换行
        editor
          .chain()
          .focus()
          .insertContentAt(range, [
            {
              type: 'blockEmbed',
              attrs: { targetId: props.id }
            },
            {
              type: 'paragraph' 
            }
          ])
          .run();
      } else {
        // 选中页面：插入普通双链
        editor
          .chain()
          .focus()
          .insertContentAt(range, {
            type: 'mention',
            attrs: {
              id: props.id,
              label: props.label,
            },
          })
          .insertContent(' ') 
          .run();
      }
    },

    // 3. 渲染逻辑：调用 SuggestionList.vue
    render: () => {
      let component;
      let popup;

      return {
        onStart: props => {
          component = new VueRenderer(SuggestionList, {
            props,
            editor: props.editor,
          });

          if (!props.clientRect) return;

          popup = tippy('body', {
            getReferenceClientRect: props.clientRect,
            appendTo: () => document.body,
            content: component.element,
            showOnCreate: true,
            interactive: true,
            trigger: 'manual',
            placement: 'bottom-start',
          });
        },

        onUpdate(props) {
          component.updateProps(props);
          if (!props.clientRect) return;
          popup[0].setProps({
            getReferenceClientRect: props.clientRect,
          });
        },

        onKeyDown(props) {
          if (props.event.key === 'Escape') {
            popup[0].hide();
            return true;
          }
          return component.ref?.onKeyDown(props);
        },

        onExit() {
          if (popup && popup[0]) popup[0].destroy();
          if (component) component.destroy();
        },
      };
    },
  },
})











  ],
  editorProps: {
    noteId: props.noteId,
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
let isSaving = false // 增加一个内部锁，防止请求堆积

const debouncedSave = (json) => {
  clearTimeout(timer);
  syncing.value = true;

  timer = setTimeout(async () => {
    // 如果当前正有一个请求在飞，我们等它结束或直接跳过本次，交由下一次防抖处理
    if (isSaving) return; 

    try {
      isSaving = true;
      const payload = { 
        id: note.id, 
        title: note.title, 
        contentJson: JSON.stringify(json), 
        parentNoteId: note.parentNoteId 
      };

      await apiClient.post('/Notes/save', payload);
      
      note.updatedAt = new Date();
      console.log('✨ 灵脉已同步至太初数据库');
    } catch (e) {
      console.error('❌ 自动保存失败:', e);
    } finally {
      isSaving = false;
      syncing.value = false;
    }
  }, 2000); // 恢复为 2000ms，兼顾性能与用户体验
}



const syncTitle = () => { if (editor.value) debouncedSave(editor.value.getJSON()) }

const handleEditorClick = (event) => {
  const target = event.target.closest('.internal-link'); if (target) { emit('navigate', target.getAttribute('data-id')); return }

// 2. 🔥 新增：按住 Alt 点击块，直接复制块 ID
  if (event.altKey) {
    const blockNode = event.target.closest('[data-block-id]');
    if (blockNode) {
      const bId = blockNode.getAttribute('data-block-id');
      navigator.clipboard.writeText(bId);
      // 这里可以加一个轻提示
      console.log('块 ID 已复制:', bId);
      return;
    }
  }

  if (event.target.closest('input') || event.target.closest('button')) return
  if (editor.value && !editor.value.isFocused) editor.value.chain().focus().run()
}

const handleBacklinkClick = (targetId) => emit('navigate', targetId)
const formatDate = (d) => d ? new Date(d).toLocaleString('zh-CN', { hour12: false }) : ''

onMounted(() => {
  window.addEventListener('navigate-note', (e) => { emit('navigate', e.detail) })
  if (editorAreaRef.value) {
    tippyInstance = delegate(editorAreaRef.value, {
      target: '.internal-link', 
      content: '加载中...',
      animation: 'shift-away',
      interactive: true,
      theme: 'light-border',
      placement: 'bottom-start',
      delay: [500, 0], 
      allowHTML: true,
      appendTo: () => document.body,
      onShow(instance) {
        const targetId = instance.reference.getAttribute('data-id')
        if (!targetId) return false;
        const container = document.createElement('div')
        const vnode = createVNode(LinkPreviewCard, { noteId: targetId })
        render(vnode, container)
        instance.setContent(container)
      }
    })
  }
})

watch([() => props.noteId, editor], ([newId, newEditor]) => { if (newId && newEditor) { note.id = newId; loadData(newId) } }, { immediate: true })
onBeforeUnmount(() => { 
  if (timer) clearTimeout(timer); 
  editor.value?.destroy();
  if (tippyInstance) tippyInstance.destroy()
})
</script>

<style lang="scss">
/* =========================================
   1. 编辑器基础容器布局
   ========================================= */
.editor-scroll-container { 
  max-width: 100%; margin: 0 auto; padding: 40px 60px; min-height: 100%; 
  background: #ffffff; position: relative; cursor: text; 
}

.doc-layout { position: relative; }

.editor-header { 
  margin-bottom: 40px; border-bottom: 1px solid rgba(0,0,0,0.06); padding-bottom: 20px; 
}

.title-field { 
  width: 100%; font-size: 40px; font-weight: 700; border: none; outline: none; 
  margin-bottom: 8px; color: #111; background: transparent; line-height: 1.2; 
  &::placeholder { color: #e5e5e5; } 
}

.meta-info { 
  font-size: 12px; color: #999; font-family: sans-serif;
  .time-label { margin-right: 6px; opacity: 0.7; } 
}

/* =========================================
   2. Tiptap 编辑器核心渲染区 (ProseMirror)
   ========================================= */
.ProseMirror { 
  outline: none; 
  min-height: 400px; 
  font-size: 16px; 
  line-height: 1.75; 
  color: #37352f; 
  margin-top: 20px; 
  padding-bottom: 30vh; 

  /* --- 块元素通用样式与悬停效果 --- */
  > * { 
    position: relative; 
    transition: all 0.2s ease; 
    border-radius: 4px; 
    margin-left: -12px; 
    padding-left: 12px; 
    border-left: 3px solid transparent; 
  } 
  
  > *:hover { 
    border-left-color: rgba(0, 0, 0, 0.08); 
    background-color: rgba(0, 0, 0, 0.01); 
  }

  /* --- 选区与占位符 --- */
  .ProseMirror-selectednode { 
    outline: 2px solid #b4d5fe; 
    background-color: rgba(180, 213, 254, 0.2) !important; 
  } 

  p.is-editor-empty:first-child::before { 
    color: #9ca3af; content: attr(data-placeholder); float: left; 
    height: 0; pointer-events: none; font-style: italic; opacity: 0.7; 
  }

  /* --- 标题层级 --- */
  h1, h2, h3, h4, h5, h6 { 
    scroll-margin-top: 140px; 
    margin-top: 1.5em;
    margin-bottom: 0.5em;
    font-weight: 700;
  }

  /* --- 列表嵌套布局 (1. -> a. -> i.) --- */
  ol {
    list-style-type: decimal;
    padding-left: 1.5rem;
    ol { list-style-type: lower-alpha; 
      ol { list-style-type: lower-roman; }
    }
  }

  ul:not([data-type="taskList"]) {
    list-style-type: disc;
    padding-left: 1.5rem;
    ul { list-style-type: circle; 
      ul { list-style-type: square; }
    }
  }

  /* =========================================
     3. 任务列表 (TaskList) 专用样式
     ========================================= */
  ul[data-type="taskList"] {
    list-style: none !important;
    padding: 0 !important;
    margin: 1rem 0 !important;

    li {
      display: flex !important;
      align-items: flex-start !important;
      margin-bottom: 0.5rem !important;
      padding: 0 !important;
      border-left: none !important;
      background-color: transparent !important;

      & > label {
        flex: 0 0 auto !important;
        width: 24px !important;
        height: 28px !important;
        display: flex !important;
        align-items: center !important;
        justify-content: center !important;
        margin-right: 8px !important;
        cursor: pointer;

        input[type="checkbox"] {
          appearance: none !important;
          -webkit-appearance: none !important;
          width: 18px !important;
          height: 18px !important;
          border: 2px solid #cbd5e1 !important;
          border-radius: 4px !important;
          background-color: #fff !important;
          cursor: pointer !important;
          display: grid !important;
          place-content: center !important;
          transition: all 0.2s;

          &:checked {
            background-color: #2383e2 !important;
            border-color: #2383e2 !important;
            &::before {
              content: "" !important; width: 10px; height: 10px; background-color: white;
              clip-path: polygon(14% 44%, 0 65%, 50% 100%, 100% 16%, 80% 0%, 43% 62%);
            }
          }
        }
      }

      & > div {
        flex: 1 1 auto !important;
        min-width: 0 !important;
        p { margin: 0 !important; line-height: 28px !important; display: block !important; }
      }

      &[data-checked="true"] {
        & > div { text-decoration: line-through !important; color: #94a3b8 !important; }
        input[type="checkbox"] { background-color: #94a3b8 !important; border-color: #94a3b8 !important; }
      }
    }
  }

  /* =========================================
     4. 高级自定义节点样式 (Details, Kanban, etc.)
     ========================================= */
  
  /* 折叠详情节点 */
  .details-node { 
    border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; 
    margin: 16px 0; background-color: #fbfbfa;

    &.is-closed .details-content > *:not([data-type="summary"]) {
      display: none !important;
    }

    [data-type="summary"], .summary-node { 
      font-weight: 600; cursor: pointer; display: flex !important;
      align-items: center; color: #37352f;
      &::before {
        content: "▶"; font-size: 10px; margin-right: 12px; color: #9ca3af;
        transition: transform 0.2s; flex-shrink: 0;
      }
      &:hover { color: #2383e2; &::before { color: #2383e2; } }
    }

    &.is-open {
      background-color: #fff;
      [data-type="summary"]::before, .summary-node::before { transform: rotate(90deg); }
    }

    .details-content {
      margin-top: 8px; padding-left: 14px; border-left: 2px solid #f3f4f6;
    }
  }

  /* 表格系统 */
  table {
    border-collapse: collapse; table-layout: fixed; width: 100%; margin: 16px 0; overflow: hidden;
    td, th { 
      min-width: 1em; border: 2px solid #ced4da; padding: 6px 10px; 
      vertical-align: top; box-sizing: border-box; position: relative;
    }
    th { background-color: #f8f9fa; font-weight: bold; text-align: left; }
    .selectedCell:after { 
      z-index: 2; position: absolute; content: ""; left: 0; right: 0; top: 0; bottom: 0; 
      background: rgba(200, 200, 255, 0.4); pointer-events: none; 
    }
  }

  /* 双链/提及样式 */
  .internal-link {
    background-color: rgba(35, 131, 226, 0.08);
    color: #2383e2;
    padding: 0 4px;
    border-radius: 4px;
    font-weight: 500;
    text-decoration: none;
    cursor: pointer;
    border-bottom: 1px solid rgba(35, 131, 226, 0.2);
    &:hover { background-color: rgba(35, 131, 226, 0.15); }
  }

  /* 图片样式 */
  img {
    max-width: 100%; height: auto; border-radius: 8px;
    &.ProseMirror-selectednode { outline: 3px solid #2383e2; }
  }
}

/* =========================================
   5. 侧边栏大纲与 UI 组件
   ========================================= */
.toc-sidebar {
  position: fixed; top: 200px; right: 30px; z-index: 1000;
  display: flex; flex-direction: column; align-items: flex-end;

  .toc-trigger {
    background: #fff; border: 1px solid #e2e8f0; padding: 10px 18px;
    border-radius: 50px; cursor: pointer; box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    display: flex; align-items: center; gap: 8px;
    .trigger-text { font-size: 14px; font-weight: 600; color: #475569; }
  }

  .toc-content-wrapper {
    position: absolute; top: 0; right: 0; width: 260px; max-height: 70vh;
    background: #fff; border: 1px solid #e2e8f0; border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.15); padding: 20px;
    opacity: 0; visibility: hidden; transform: translateY(10px) scale(0.95);
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    display: flex; flex-direction: column;

    .toc-header { font-weight: 800; margin-bottom: 15px; border-bottom: 1px solid #f1f5f9; padding-bottom: 8px; }
    .toc-list { overflow-y: auto; flex: 1; }
    .toc-item {
      padding: 8px; font-size: 13px; color: #64748b; cursor: pointer; border-radius: 6px;
      &:hover { background: #f1f5f9; color: #2383e2; }
      &.level-2 { padding-left: 20px; }
      &.level-3 { padding-left: 35px; font-size: 12px; }
      &.active { background: #eff6ff; color: #2383e2; font-weight: 700; }
    }
  }

  &:hover {
    .toc-trigger { opacity: 0; pointer-events: none; }
    .toc-content-wrapper { opacity: 1; visibility: visible; transform: translateY(0) scale(1); }
  }
}

/* 反向链接区域 */
.backlinks-section {
  margin-top: 80px; border-top: 1px solid #eaeaea; padding-top: 30px;
  .section-title { font-size: 14px; font-weight: 700; color: #888; margin-bottom: 20px; }
  .backlink-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
  .backlink-card { 
    padding: 16px; border: 1px solid #eee; border-radius: 8px; 
    cursor: pointer; background: #fff; transition: all 0.2s;
    &:hover { border-color: #2383e2; box-shadow: 0 4px 12px rgba(0,0,0,0.05); transform: translateY(-2px); }
    .bl-title { font-size: 14px; color: #333; font-weight: 600; }
  }
}

/* 同步状态指示器 */
.sync-indicator {
  position: fixed; bottom: 25px; right: 25px; font-size: 12px; 
  padding: 8px 16px; background: #fff; border: 1px solid #eee; border-radius: 30px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05); z-index: 100;
  &.is-syncing { color: #2383e2; border-color: #2383e2; font-weight: 600; }
}

/* Tippy 预览框 */
.tippy-box[data-theme~='light-border'] {
  background-color: #fff; border: 1px solid #e2e8f0; border-radius: 12px;
  box-shadow: 0 12px 24px rgba(0,0,0,0.1);
  .tippy-content { padding: 0; }
}

/* 动画 */
.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
</style>