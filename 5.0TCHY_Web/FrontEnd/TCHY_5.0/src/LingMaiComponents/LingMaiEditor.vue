<template>
  <div class="editor-scroll-container" @click="handleEditorClick">
    <div class="editor-header">
      <div v-if="note.isShortcut" class="shortcut-banner">
        🔗 这是一个跨空间分身。原始页面位于「{{ note.realSpaceName }}」。你在这里的修改将实时同步至真身。
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
        
        <span class="divider">|</span>
        <div class="action-dropdown" @click.stop="toggleSpaceMenu">
          <span class="action-btn">🚀 页面流转</span>

          <span class="divider">|</span>
<span class="action-btn" @click="toggleHistoryPanel">🕒 版本历史</span>
          
          <div class="dropdown-menu" v-if="showSpaceMenu" @click.stop>
            <div class="menu-title">将此页面发送至...</div>
            <select v-model="targetSpaceId" class="space-select">
              <option disabled value="">请选择目标空间</option>
              <option v-for="s in availableSpaces" :key="s.id" :value="s.id">
                🌌 {{ s.name }}
              </option>
            </select>
            
            <div class="menu-actions" v-if="targetSpaceId">
              <button class="btn-move" @click="moveToSpace" title="将此页面连根拔起，彻底移入目标空间">
                🚚 彻底移动
              </button>
              <button class="btn-shortcut" @click="createShortcut" title="在目标空间创建一个指向这里的快捷方式" v-if="!note.isShortcut">
                🔗 创建分身
              </button>
            </div>
          </div>
        </div>

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
          <div v-for="bl in backlinks" :key="bl.id" class="backlink-card" @click="handleBacklinkClick(bl.id)">
            <div class="bl-title">📄 {{ bl.title }}</div>
            <div style="font-size: 12px; color: #888; margin-top: 4px;" v-if="bl.spaceName">🌌 {{ bl.spaceName }}</div>
          </div>
        </div>
      </div>


      <div class="history-sidebar" :class="{ 'is-open': showHistoryPanel }">
  <div class="sidebar-header">
    <h3>🕒 历史版本</h3>
    <button class="close-btn" @click="showHistoryPanel = false">×</button>
  </div>
  
  <div class="sidebar-content">
    <div v-if="isLoadingHistory" class="loading-state">加载中...</div>
    <div v-else-if="historyList.length === 0" class="empty-state">暂无历史记录</div>
    
    <div v-else class="history-timeline">
      <div v-for="h in historyList" :key="h.id" class="history-card">
        <div class="h-time">{{ formatHistoryTime(h.savedAt) }}</div>
        <div class="h-title">{{ h.title || '无标题' }}</div>
        <div class="h-meta">规模: {{ h.wordCount }} 字符</div>
        <button class="restore-btn" @click="applyHistory(h.id)">恢复此版本</button>
      </div>
    </div>
  </div>
</div>


    </div>

    <div class="status-monitor">
      <Transition name="status-fade">
        <div v-if="isUploading" class="upload-progress-card">
          <div class="progress-text">
            <span class="spinner">⏳</span>
            <span class="label">图片处理中...</span>
            <span class="percent">{{ uploadProgress }}%</span>
          </div>
          <div class="progress-track">
            <div class="progress-bar" :style="{ width: uploadProgress + '%' }"></div>
          </div>
        </div>
      </Transition>

      <div class="sync-indicator" :class="{ 'is-syncing': syncing || isUploading }">
        <template v-if="isUploading">
          <span class="status-icon">📡</span> 正在传输资源...
        </template>
        <template v-else-if="syncing">
          <span class="status-icon">☁️</span> 同步至云端...
        </template>
        <template v-else>
          <span class="status-icon">✅</span> 灵脉已就绪
        </template>
      </div>
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
import ResizeImage from 'tiptap-extension-resize-image'
import { CustomTaskItem } from './CustomTaskItem.js'
import { useNoteHistory } from './LingMaiNoteHistory.js'

import CalendarBlock from './CalendarNode.js'


import { BlockId } from './BlockId'
import BlockEmbed from './BlockEmbedNode.js'

const isDataLoaded = ref(false) 
const uploadProgress = ref(0) 
const isUploading = ref(false) 

const props = defineProps(['noteId', 'spaceId'])
const emit = defineEmits(['navigate', 'deleted']) 

const tocItems = ref([])
const activeHeadingIndex = ref(-1)

const note = reactive({ 
  id: props.noteId, 
  title: '', 
  updatedAt: new Date(), 
  parentNoteId: null,
  isShortcut: false, 
  realSpaceName: '' 
})

// 🔥 跨空间流转状态
const showSpaceMenu = ref(false)
const availableSpaces = ref([])
const targetSpaceId = ref('')

const syncing = ref(false)
const backlinks = ref([])
const editorAreaRef = ref(null)
let tippyInstance = null

// 🔥 加载用户的其他空间列表
const loadSpaces = async () => {
  try {
    const res = await apiClient.get('/Spaces/list')
    // 过滤掉当前所在的空间
    availableSpaces.value = res.data.filter(s => s.id !== props.spaceId)
  } catch (e) { console.error("无法加载空间列表", e) }
}

const toggleSpaceMenu = () => {
  showSpaceMenu.value = !showSpaceMenu.value
  if (showSpaceMenu.value && availableSpaces.value.length === 0) {
    loadSpaces()
  }
}

// 🚀 彻底移动到其他空间
const moveToSpace = async () => {
  if (!targetSpaceId.value) return
  if (confirm("确定要将此页面及其所有子页面彻底移动到新空间吗？")) {
    try {
      await apiClient.post('/Notes/move', {
        id: note.id,
        targetSpaceId: targetSpaceId.value,
        parentId: null // 默认移动到目标空间的根目录
      })
      alert("移动成功！页面已流转至目标空间。")
      showSpaceMenu.value = false
      emit('deleted') // 通知父组件当前页面已移走，刷新左侧目录树
    } catch (e) { alert("移动失败: " + (e.response?.data?.message || e.message)) }
  }
}

// 🔗 在其他空间创建分身
const createShortcut = async () => {
  if (!targetSpaceId.value) return
  try {
    await apiClient.post('/Notes/create-shortcut', {
      targetNoteId: note.id,
      targetSpaceId: targetSpaceId.value,
      parentId: null
    })
    alert("分身创建成功！你可以在目标空间看到此页面的引用。")
    showSpaceMenu.value = false
  } catch (e) { alert("创建分身失败: " + (e.response?.data?.message || e.message)) }
}

const uploadImage = async (file) => {
  const formData = new FormData()
  formData.append('file', file)
  
  isUploading.value = true
  uploadProgress.value = 0
  
  try {
    const res = await apiClient.post('/Upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        uploadProgress.value = percentCompleted
      }
    })
    return res.data.url
  } catch (e) {
    return null
  } finally {
    setTimeout(() => { isUploading.value = false; uploadProgress.value = 0 }, 1000)
  }
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
    CalendarBlock,
    KanbanNode, StarterKit.configure({ image: false }), BlockId, BlockEmbed, HeatmapNode,
    Placeholder.configure({ placeholder: '输入 / 唤起命令，输入 [[ 建立关联...' }), SlashCommand, 
    TaskList.configure({ nested: true }), TaskItem.extend({ content: 'block+' }).configure({ nested: true }),CustomTaskItem.configure({ nested: true }),
    Image.configure({ inline: true }), ResizeImage.extend({ name: 'image' }).configure({ inline: true }),
    Details, Summary, Table.configure({ resizable: true }), TableRow, TableHeader, TableCell,
    TextStyle, Color, Underline, FontSize, Highlight.configure({ multicolor: true }), BubbleMenuExtension.configure({ element: null }),
    Mention.configure({
      HTMLAttributes: { class: 'internal-link' },
      suggestion: {
        char: '[[',
        items: async ({ query }) => {
          if (!props.spaceId) return [];

          // 🔥 块级别全局搜索逻辑
          if (query && query.startsWith('#')) {
            const blockQuery = query.substring(1); 
            try {
              const res = await apiClient.get(`/Notes/search/blocks/global?query=${encodeURIComponent(blockQuery)}`);
              
              // 排序：当前空间的排在前面
              const sortedData = res.data.sort((a, b) => {
                const aIsLocal = a.spaceId === props.spaceId ? -1 : 1;
                const bIsLocal = b.spaceId === props.spaceId ? -1 : 1;
                return aIsLocal - bIsLocal;
              });

              return sortedData.map(b => {
                const isLocal = b.spaceId === props.spaceId;
                const prefix = isLocal ? '📍当前' : `🌌${b.spaceName}`;
                return { 
                  id: b.blockId, 
                  label: `[${prefix}] ${b.noteTitle} - ${b.previewText}`, 
                  type: 'block' 
                }
              });
            } catch (e) { return []; }
          }

          // 🔥 笔记级别全局搜索逻辑
          try {
            const res = await apiClient.get(`/Notes/search/global?query=${encodeURIComponent(query || '')}`);
            
            // 排序：当前空间的排在前面
            const sortedData = res.data.sort((a, b) => {
              const aIsLocal = a.spaceId === props.spaceId ? -1 : 1;
              const bIsLocal = b.spaceId === props.spaceId ? -1 : 1;
              return aIsLocal - bIsLocal;
            });

            return sortedData.map(n => {
              const isLocal = n.spaceId === props.spaceId;
              const prefix = isLocal ? '📍当前' : `🌌${n.spaceName}`;
              return { 
                id: n.id, 
                label: `[${prefix}] ${n.title}`, 
                type: 'note' 
              }
            });
          } catch (e) { return []; }
        },
        command: ({ editor, range, props }) => {
          if (props.type === 'block') {
            editor.chain().focus().insertContentAt(range, [ { type: 'blockEmbed', attrs: { targetId: props.id } }, { type: 'paragraph' } ]).run();
          } else {
            editor.chain().focus().insertContentAt(range, { type: 'mention', attrs: { id: props.id, label: props.label } }).insertContent(' ').run();
          }
        },
        render: () => {
          let component; let popup;
          return {
            onStart: suggestionProps => {
              component = new VueRenderer(SuggestionList, { props: suggestionProps, editor: suggestionProps.editor });
              if (!suggestionProps.clientRect) return;
              popup = tippy('body', { getReferenceClientRect: suggestionProps.clientRect, appendTo: () => document.body, content: component.element, showOnCreate: true, interactive: true, trigger: 'manual', placement: 'bottom-start' });
            },
            onUpdate(suggestionProps) {
              component.updateProps(suggestionProps);
              if (!suggestionProps.clientRect) return;
              popup[0].setProps({ getReferenceClientRect: suggestionProps.clientRect });
            },
            onKeyDown(suggestionProps) {
              if (suggestionProps.event.key === 'Escape') { popup[0].hide(); return true; }
              return component.ref?.onKeyDown(suggestionProps);
            },
            onExit() { if (popup && popup[0]) popup[0].destroy(); if (component) component.destroy(); },
          };
        },
      },
    })
  ],
  editorProps: {
    noteId: props.noteId,
    handlePaste: (view, event) => {
      const items = (event.clipboardData || event.originalEvent.clipboardData).items;
      for (const item of items) {
        if (item.type.indexOf('image') === 0) {
          event.preventDefault(); const file = item.getAsFile();
          uploadImage(file).then(url => {
            if (url) { const { schema } = view.state; const node = schema.nodes.image.create({ src: url, width: '100%' }); const transaction = view.state.tr.replaceSelectionWith(node); view.dispatch(transaction); }
          });
          return true; 
        }
      }
      return false;
    },
    handleDrop: (view, event, slice, moved) => {
      if (!moved && event.dataTransfer && event.dataTransfer.files.length > 0) { 
        const file = event.dataTransfer.files[0]; 
        if (file.type.indexOf('image') === 0) { 
          event.preventDefault(); const coordinates = view.posAtCoords({ left: event.clientX, top: event.clientY }); 
          uploadImage(file).then(url => { 
            if (url) { const { schema } = view.state; const node = schema.nodes.image.create({ src: url }); view.dispatch(view.state.tr.insert(coordinates.pos, node)) } 
          }); 
          return true 
        } 
      } 
      return false
    }
  },
  onUpdate: ({ editor }) => { if (!isDataLoaded.value) return; debouncedSave(editor.getJSON()); updateToc(editor); },
  onCreate: ({ editor }) => { updateToc(editor) }
})

const loadData = async (id) => {
  if (!editor.value) return
  isDataLoaded.value = false 
  try {
    const res = await apiClient.get(`/Notes/detail/${id}`); 
    note.title = res.data.title || ''; 
    note.updatedAt = res.data.updatedAt; 
    note.parentNoteId = res.data.parentNoteId; 
    note.isShortcut = res.data.isShortcut;
    note.realSpaceName = res.data.realSpaceName;
    const content = res.data.contentJson;

    if (content && content !== '""') { 
      const jsonContent = JSON.parse(content);
      const schemaNodes = editor.value.schema.nodes;
      const checkSchemaMatch = (node) => {
        if (node.type && !schemaNodes[node.type]) return node.type;
        if (node.content) { for (const child of node.content) { const missing = checkSchemaMatch(child); if (missing) return missing; } }
        return null;
      };

      const missingType = checkSchemaMatch(jsonContent);
      if (missingType) throw new Error(`编辑器配置错误：缺少 [${missingType}] 节点扩展。`);

      try { editor.value.commands.setContent(jsonContent); updateToc(editor.value); } catch (e) { throw new Error(`内容解析失败：${e.message}`); } 
    } else { 
      editor.value.commands.setContent('');
    }
    
    loadBacklinks(id);
    setTimeout(() => { isDataLoaded.value = true; }, 300);
  } catch (e) { 
    note.title = "加载受阻"; isDataLoaded.value = false; 
    editor.value?.commands.setContent(`
      <div style="border: 2px dashed #ff4d4f; padding: 20px; border-radius: 8px; color: #ff4d4f;">
        <h3>⚠️ 数据加载异常</h3><p>${e.message}</p>
        <p>请检查插件配置或刷新页面。当前自动保存已禁用，以保护数据库数据。</p>
      </div>
    `);
  }
}

const loadBacklinks = async (id) => { try { const blRes = await apiClient.get(`/Notes/${id}/backlinks`); backlinks.value = blRes.data } catch (e) { backlinks.value = [] } }

let timer = null
let isSaving = false 

const debouncedSave = (json) => {
  clearTimeout(timer); syncing.value = true;
  timer = setTimeout(async () => {
    if (isSaving || !props.spaceId) return; 
    try {
      isSaving = true;
      const payload = { id: note.id, title: note.title, contentJson: JSON.stringify(json), parentNoteId: note.parentNoteId, spaceId: props.spaceId };
      await apiClient.post('/Notes/save', payload);
      note.updatedAt = new Date();
    } catch (e) { console.error('❌ 自动保存失败:', e); } finally { isSaving = false; syncing.value = false; }
  }, 2000); 
}

const { 
  showHistoryPanel, 
  historyList, 
  isLoadingHistory, 
  toggleHistoryPanel, 
  applyHistory, 
  formatHistoryTime 
} = useNoteHistory(note, editor, debouncedSave)



const syncTitle = () => { if (editor.value) debouncedSave(editor.value.getJSON()) }

const handleEditorClick = (event) => {
  const target = event.target.closest('.internal-link'); if (target) { emit('navigate', target.getAttribute('data-id')); return }
  if (event.altKey) { const blockNode = event.target.closest('[data-block-id]'); if (blockNode) { navigator.clipboard.writeText(blockNode.getAttribute('data-block-id')); return; } }
  if (event.target.closest('input') || event.target.closest('button')) return
  if (editor.value && !editor.value.isFocused) editor.value.chain().focus().run()
}

const handleBacklinkClick = (targetId) => emit('navigate', targetId)
const formatDate = (d) => d ? new Date(d).toLocaleString('zh-CN', { hour12: false }) : ''

onMounted(() => {
  document.addEventListener('click', () => { showSpaceMenu.value = false }) // 点击外部关闭菜单
  window.addEventListener('navigate-note', (e) => { emit('navigate', e.detail) })
  if (editorAreaRef.value) {
    tippyInstance = delegate(editorAreaRef.value, {
      target: '.internal-link', content: '加载中...', animation: 'shift-away', interactive: true, theme: 'light-border', placement: 'bottom-start', delay: [500, 0], allowHTML: true, appendTo: () => document.body,
      onShow(instance) {
        const targetId = instance.reference.getAttribute('data-id'); if (!targetId) return false;
        const container = document.createElement('div'); const vnode = createVNode(LinkPreviewCard, { noteId: targetId }); render(vnode, container); instance.setContent(container)
      }
    })
  }
})

watch([() => props.noteId, editor], ([newId, newEditor]) => { if (newId && newEditor) { note.id = newId; loadData(newId) } }, { immediate: true })
onBeforeUnmount(() => { if (timer) clearTimeout(timer); editor.value?.destroy(); if (tippyInstance) tippyInstance.destroy() })
</script>

<style lang="scss">

/* 🔥 分身横幅样式 */
.shortcut-banner {
  background: #f0f7ff; border: 1px solid #cce3fd; border-radius: 6px;
  padding: 10px 14px; margin-bottom: 16px; font-size: 13px; color: #005cc5;
  display: flex; align-items: center; font-weight: 500;
}

/* 🔥 页面流转菜单样式 */
.divider { margin: 0 10px; color: #e0e0e0; }
.action-dropdown {
  display: inline-block; position: relative;
  .action-btn { 
    cursor: pointer; color: #666; font-weight: 500; transition: color 0.2s;
    &:hover { color: #2383e2; }
  }
  
  .dropdown-menu {
    position: absolute; top: 25px; right: 0; width: 220px;
    background: #fff; border: 1px solid #e2e8f0; border-radius: 8px;
    box-shadow: 0 10px 25px rgba(0,0,0,0.1); padding: 12px; z-index: 1000;
    
    .menu-title { font-size: 12px; color: #888; margin-bottom: 8px; font-weight: bold; }
    
    .space-select {
      width: 100%; padding: 6px; border: 1px solid #ddd; border-radius: 4px;
      margin-bottom: 12px; font-size: 13px; outline: none;
    }
    
    .menu-actions {
      display: flex; flex-direction: column; gap: 8px;
      button {
        width: 100%; padding: 8px; border: none; border-radius: 4px;
        font-size: 13px; font-weight: 600; cursor: pointer; transition: all 0.2s;
        &.btn-move { background: #fff0f0; color: #d32f2f; border: 1px solid #ffccc7; }
        &.btn-move:hover { background: #ffccc7; }
        &.btn-shortcut { background: #f0f7ff; color: #005cc5; border: 1px solid #cce3fd; }
        &.btn-shortcut:hover { background: #cce3fd; }
      }
    }
  }
}

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

  .tiptap-image-resizer {
    display: inline-block !important;
    position: relative !important;
    line-height: 0;
    vertical-align: middle;
    user-select: none;
    z-index: 10; 

    [class*="handler"], 
    [class*="resize-trigger"],
    .resizer {
      display: block !important;
      position: absolute !important;
      width: 10px !important; 
      height: 10px !important;
      background-color: #2383e2 !important; 
      border: 1.5px solid #ffffff !important;
      border-radius: 2px !important;
      z-index: 100 !important;
      cursor: nwse-resize; 
      visibility: visible !important;
      opacity: 1 !important;
    }

    .handler-top-left { top: -5px; left: -5px; cursor: nwse-resize; }
    .handler-top-right { top: -5px; right: -5px; cursor: nesw-resize; }
    .handler-bottom-left { bottom: -5px; left: -5px; cursor: nesw-resize; }
    .handler-bottom-right { bottom: -5px; right: -5px; cursor: nwse-resize; }
  }

  .ProseMirror-selectednode {
    outline: 2px solid #2383e2 !important;
    outline-offset: 2px;
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
  /* =========================================
   极致打磨版：多级任务列表 (Tiptap TaskList)
   ========================================= */
ul[data-type="taskList"] {
  list-style: none !important;
  padding: 0 !important;
  margin: 0.5rem 0 !important;

  /* ------------------------------
     1. 基础任务行样式 (所有层级通用)
     ------------------------------ */
  li {
    display: flex !important;
    align-items: flex-start !important;
    margin-bottom: 2px !important;
    padding: 4px 6px !important;
    border-radius: 6px;
    border-left: none !important;
    background-color: transparent;
    transition: background-color 0.2s ease, opacity 0.2s ease;

    /* 整行 Hover 时的微妙反馈 */
    &:hover {
      background-color: rgba(0, 0, 0, 0.03) !important;
    }

    /* ------------------------------
       2. 复选框核心样式
       ------------------------------ */
    & > label {
      flex: 0 0 auto !important;
      margin-right: 10px !important;
      margin-top: 3px !important; /* 让复选框与第一行文字基线对齐 */
      cursor: pointer;
      display: flex;
      align-items: center;

      input[type="checkbox"] {
        appearance: none !important;
        -webkit-appearance: none !important;
        width: 1.15em !important;
        height: 1.15em !important;
        margin: 0 !important;
        border: 1.5px solid #a1a1aa !important;
        border-radius: 5px !important; /* 第一级：平滑的圆角矩形 */
        background-color: transparent !important;
        cursor: pointer !important;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        position: relative;
        display: grid;
        place-content: center;

        /* 复选框 Hover 状态 */
        &:hover {
          border-color: #2383e2 !important;
          background-color: rgba(35, 131, 226, 0.08) !important;
        }

        /* 勾选后的状态 & 弹出动画 */
        &:checked {
          background-color: #2383e2 !important;
          border-color: #2383e2 !important;

          /* 使用内联 SVG 绘制完美的对号 */
          background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='white' stroke-width='3.5' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='20 6 9 17 4 12'%3E%3C/polyline%3E%3C/svg%3E") !important;
          background-size: 75% !important;
          background-position: center !important;
          background-repeat: no-repeat !important;
          animation: checkmark-pop 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
        }
      }
    }

    /* ------------------------------
       3. 文本容器样式
       ------------------------------ */
    & > div {
      flex: 1 1 auto !important;
      min-width: 0 !important;
      
      p {
        margin: 0 !important;
        line-height: 1.6 !important;
        color: #37352f;
        transition: color 0.3s ease;
      }
    }

    /* ------------------------------
       4. 任务完成后的整体暗化状态
       ------------------------------ */
    &[data-checked="true"] {
      & > div p {
        color: #9ca3af !important;
        text-decoration: line-through;
        text-decoration-color: #cbd5e1;
      }
    }
  }

  /* ------------------------------
     5. 子任务嵌套层级 (第二级)
     ------------------------------ */
  ul[data-type="taskList"] {
    margin-top: 2px !important;
    /* 核心计算：刚好对齐父级复选框的中心，并留出内边距 */
    margin-left: 0.55em !important; 
    padding-left: 1.4em !important; 
    /* 干净的实线引导线，比虚线更有现代感 */
    border-left: 1.5px solid #e2e8f0; 

    li {
      & > label input[type="checkbox"] {
        border-radius: 50% !important; /* 第二级：变为正圆形 */
        width: 1.05em !important;
        height: 1.05em !important;
      }
      & > div p {
        font-size: 0.95em; /* 字体微微缩小，凸显层级 */
        color: #475569;
      }
    }

    /* ------------------------------
       6. 孙任务嵌套层级 (第三级)
       ------------------------------ */
    ul[data-type="taskList"] {
      border-left-color: #f1f5f9; /* 第三级引导线颜色更淡 */
      
      li > label input[type="checkbox"] {
        border-radius: 3px !important; /* 第三级：硬朗的小方块 */
        width: 0.95em !important;
        height: 0.95em !important;
      }
      & > div p {
        font-size: 0.9em;
      }
    }
  }
}

/* =========================================
   复选框弹出动画关键帧
   ========================================= */
@keyframes checkmark-pop {
  0% { transform: scale(0.8); }
  50% { transform: scale(1.15); }
  100% { transform: scale(1); }
}

  /* =========================================
     4. 高级自定义节点样式
     ========================================= */
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

.sync-indicator {
  position: fixed; bottom: 25px; right: 25px; font-size: 12px; 
  padding: 8px 16px; background: #fff; border: 1px solid #eee; border-radius: 30px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05); z-index: 100;
  &.is-syncing { color: #2383e2; border-color: #2383e2; font-weight: 600; }
}

.tippy-box[data-theme~='light-border'] {
  background-color: #fff; border: 1px solid #e2e8f0; border-radius: 12px;
  box-shadow: 0 12px 24px rgba(0,0,0,0.1);
  .tippy-content { padding: 0; }
}

.fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

.history-sidebar {
  position: fixed;
  top: 0;
  right: -320px;
  width: 300px;
  height: 100vh;
  background: #fff;
  box-shadow: -5px 0 25px rgba(0,0,0,0.1);
  z-index: 2000;
  transition: right 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;

  &.is-open {
    right: 0;
  }

  .sidebar-header {
    padding: 20px;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
    
    h3 { margin: 0; font-size: 16px; color: #333; }
    .close-btn { background: none; border: none; font-size: 24px; cursor: pointer; color: #999; }
  }

  .sidebar-content {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    background: #f9f9f9;
  }

  .history-card {
    background: #fff;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 12px;
    transition: transform 0.2s;

    &:hover { border-color: #2383e2; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.05); }

    .h-time { font-size: 12px; color: #2383e2; font-weight: 600; margin-bottom: 4px; }
    .h-title { font-size: 14px; font-weight: 500; color: #333; margin-bottom: 8px; }
    .h-meta { font-size: 12px; color: #888; margin-bottom: 12px; }
    
    .restore-btn {
      width: 100%; padding: 6px; background: #f0f7ff; color: #005cc5; border: 1px solid #cce3fd; border-radius: 4px; cursor: pointer;
      &:hover { background: #cce3fd; }
    }
  }
}


</style>