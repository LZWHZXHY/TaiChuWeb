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
          .replaceRangeWith(range, {
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
   1. 侧边栏大纲样式
   ========================================= */
.toc-sidebar {
  position: fixed; top: 200px; right: 30px; z-index: 1000; display: flex; flex-direction: column; align-items: flex-end;
  .toc-trigger {
    background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08); padding: 10px 16px; border-radius: 50px; cursor: pointer; display: flex; align-items: center; gap: 8px; color: #475569; transition: all 0.3s ease;
    .trigger-icon { font-size: 16px; }
    .trigger-text { font-size: 14px; font-weight: 500; }
    &:hover { border-color: #cbd5e1; background: #f8fafc; }
  }
  .toc-content-wrapper {
    position: absolute; top: 0; right: 0; width: 260px; max-height: 75vh; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15); padding: 16px; opacity: 0; visibility: hidden; transform: translateY(10px) scale(0.95); transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1); display: flex; flex-direction: column; pointer-events: none;
  }
  &:hover {
    .toc-trigger { opacity: 0; transform: scale(0.8); }
    .toc-content-wrapper { opacity: 1; visibility: visible; transform: translateY(0) scale(1); pointer-events: auto; }
  }
  .toc-header { font-size: 14px; font-weight: 700; color: #1e293b; margin-bottom: 12px; padding-bottom: 8px; border-bottom: 1px solid #f1f5f9; }
  .toc-list { overflow-y: auto; flex: 1; display: flex; flex-direction: column; gap: 2px; &::-webkit-scrollbar { width: 4px; } &::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 4px; } }
  .toc-item {
    font-size: 13px; color: #64748b; cursor: pointer; padding: 6px 8px; border-radius: 6px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; transition: all 0.2s;
    &:hover { background-color: #f1f5f9; color: #2563eb; }
    &.active { background-color: #eff6ff; color: #2563eb; font-weight: 600; }
    &.level-1 { font-weight: 600; color: #334155; }
    &.level-2 { padding-left: 18px; }
    &.level-3 { padding-left: 30px; font-size: 12px; }
  }
}

/* =========================================
   2. Tippy & 容器样式
   ========================================= */
.tippy-box[data-theme~='light-border'] {
  background-color: #fff; color: #333; border: 1px solid #eee; box-shadow: 0 4px 14px rgba(0,0,0,0.1); border-radius: 8px;
  .tippy-content { padding: 0; }
  .tippy-arrow { color: #fff; }
}

.editor-scroll-container { 
  max-width: 100%; margin: 0 auto; padding: 40px 60px; min-height: 100%; background: #ffffff; position: relative; cursor: text; 
}
.doc-layout { position: relative; }
.editor-header { margin-bottom: 40px; border-bottom: 1px solid rgba(0,0,0,0.06); padding-bottom: 20px; }
.title-field { width: 100%; font-size: 40px; font-weight: 700; border: none; outline: none; margin-bottom: 8px; color: #111; background: transparent; line-height: 1.2; &::placeholder { color: #e5e5e5; } }
.meta-info { font-size: 12px; color: #999; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; .time-label { margin-right: 6px; opacity: 0.7; } }

/* =========================================
   终极修复版：保留标题的折叠样式
   ========================================= */
.ProseMirror { 
  outline: none; 
  min-height: 400px; 
  font-size: 16px; 
  line-height: 1.75; 
  color: #37352f; 
  margin-top: 20px; 
  padding-bottom: 30vh; 


  /* =======================================================
     🟢 优化：有序列表嵌套层级样式 (1. -> a. -> i.)
     ======================================================= */
  ol {
    list-style-type: decimal; /* 第一级: 1, 2, 3 */
    padding-left: 1.5rem;     /* 保证适当的缩进距离 */
    
    ol {
      list-style-type: lower-alpha; /* 第二级: a, b, c */
      margin-top: 4px;              /* 稍微紧凑一点 */
      
      ol {
        list-style-type: lower-roman; /* 第三级: i, ii, iii */
        
        ol {
          list-style-type: decimal; /* 第四级以上循环回数字 */
        }
      }
    }
  }

  /* 顺手优化一下无序列表的嵌套展示 (实心圆 -> 空心圆 -> 方块) */
  ul:not([data-type="taskList"]) {
    list-style-type: disc;
    padding-left: 1.5rem;

    ul {
      list-style-type: circle;
      
      ul {
        list-style-type: square;
      }
    }
  }






  // 1. 标题层级
  h1, h2, h3, h4, h5, h6 { scroll-margin-top: 140px; } 

  // 2. 基础块元素交互 (保留你原本的悬停效果)
  > * { 
    position: relative; 
    transition: background-color 0.2s ease; 
    border-radius: 4px; 
    margin-left: -12px; 
    padding-left: 12px; 
    border-left: 3px solid transparent; 
  } 
  > *:hover { 
    border-left-color: rgba(0, 0, 0, 0.08); 
    background-color: rgba(0, 0, 0, 0.01); 
  } 

  // 3. 选区与占位符
  .ProseMirror-selectednode { outline: 2px solid #b4d5fe; background-color: transparent !important; } 
  p.is-editor-empty:first-child::before { color: #9ca3af; content: attr(data-placeholder); float: left; height: 0; pointer-events: none; font-style: italic; opacity: 0.7; } 

  // 4. 表格系统
  table {
    border-collapse: collapse; table-layout: fixed; width: 100%; margin: 0; overflow: hidden;
    td, th { 
      min-width: 1em; border: 2px solid #ced4da; padding: 3px 5px; vertical-align: top; box-sizing: border-box; position: relative; 
      > * { margin-bottom: 0; } 
    }
    th { font-weight: bold; text-align: left; background-color: #f1f3f5; }
    .selectedCell:after { z-index: 2; position: absolute; content: ""; left: 0; right: 0; top: 0; bottom: 0; background: rgba(200, 200, 255, 0.4); pointer-events: none; }
  }

  /* =======================================================
     🟢 核心修复：Details 节点 (折叠后保留标题)
     ======================================================= */
  .details-node { 
    border: 1px solid #e2e8f0; 
    border-radius: 6px; 
    padding: 8px 12px; 
    margin: 10px 0; 
    background-color: #fbfbfa; 
    transition: all 0.2s;
    display: block !important;
    user-select: text !important;

    // 状态：折叠
    &.is-closed {
      background-color: #f8fafc;
      
      // 🔴 核心修复：只隐藏 summary 之后的兄弟节点
      .details-content {
        > *:not([data-type="summary"]) {
          display: none !important;
        }
      }
    }

    // 状态：打开
    &.is-open { 
      background-color: #fff; 
      border-color: #d1d5db; 
      [data-type="summary"]::before { transform: rotate(90deg); }
    } 

    // 标题行容器
    [data-type="summary"] { 
      font-weight: 600; 
      cursor: pointer; 
      color: #37352f; 
      padding: 4px 0; 
      display: flex !important;
      align-items: center;
      user-select: text !important;

      &::before {
        content: "▶";
        font-size: 10px;
        margin-right: 10px;
        color: #9ca3af;
        transition: transform 0.2s;
        display: inline-block;
      }
    }

    // 内容布局缩进
    .details-content { 
      margin-top: 4px; 
      > *:not([data-type="summary"]) {
        margin-left: 14px;
        padding-left: 14px;
        border-left: 2px solid #f3f4f6;
        color: #4b5563;
      }
    } 
  }

    /* Summary 标题行渲染 */
    .summary-node { 
      font-weight: 600; 
      cursor: pointer; 
      color: #37352f; 
      padding: 4px 0; 
      display: flex !important;
      align-items: center;
      position: relative;
      user-select: text !important;

      &::before {
        content: "▶";
        font-size: 10px;
        margin-right: 10px;
        color: #9ca3af;
        transition: transform 0.2s;
        display: inline-block;
        flex-shrink: 0;
      }

      &:hover { color: #2383e2; &::before { color: #2383e2; } } 
    }

    /* 旋转图标 */
    &.is-open .summary-node::before {
      transform: rotate(90deg);
    }

    /* 内容区域包装层 */
    .details-content { 
      margin-top: 8px; 
      padding-left: 14px; 
      border-left: 2px solid #f3f4f6; 
      color: #4b5563; 
      user-select: text !important;
    } 
  } 



:deep(ul[data-type="taskList"]) {
  list-style: none !important;
  padding: 0 !important;
  margin: 0 !important;

  li[data-type="taskItem"] {
    /* 🔥 CSS Grid 布局：左侧固定 24px，右侧自适应 */
    /* 这是最稳的布局，文字绝对不可能掉下来 */
    display: grid !important;
    grid-template-columns: 24px 1fr !important; 
    gap: 0 !important;
    align-items: start !important;
    
    margin-bottom: 0.5rem !important;
    
    /* 左侧：框 */
    & > label {
      display: flex !important;
      align-items: center;
      justify-content: center;
      margin: 0 !important;
      padding: 0 !important;
      width: 100% !important;
      height: 1.6em !important; /* 与文字行高对齐 */
      user-select: none;
    }

    /* 右侧：字 */
    & > div {
      grid-column: 2 !important; 
      min-width: 0 !important;
      margin: 0 !important;
      padding: 0 !important;
      
      & > p {
        margin: 0 !important;
        padding: 0 !important;
        line-height: 1.6 !important;
        display: block !important;
      }
    }

    /* 复选框 UI */
    input[type="checkbox"] {
      appearance: none !important;
      -webkit-appearance: none !important;
      background-color: #fff;
      margin: 0 !important;
      color: currentColor;
      width: 1.15em !important;
      height: 1.15em !important;
      border: 2px solid #cbd5e1 !important;
      border-radius: 4px;
      display: grid;
      place-content: center;
      cursor: pointer;
      transition: all 0.2s ease-in-out;

      &::before {
        content: ""; width: 0.65em; height: 0.65em;
        transform: scale(0);
        transition: 0.1s transform ease-in-out;
        box-shadow: inset 1em 1em white;
        transform-origin: center;
        clip-path: polygon(14% 44%, 0 65%, 50% 100%, 100% 16%, 80% 0%, 43% 62%);
      }

      &:hover { border-color: #2383e2; background-color: #f0f9ff; }
      &:checked { background-color: #2383e2; border-color: #2383e2; }
      &:checked::before { transform: scale(1); }
    }
    
    /* 完成状态 */
    &[data-checked="true"] > div {
      text-decoration: line-through; color: #94a3b8; transition: color 0.2s;
    }
    &[data-checked="true"] input[type="checkbox"] {
       opacity: 0.8; background-color: #94a3b8; border-color: #94a3b8;
    }
  }
}

/* =========================================
   5. 其他页面元素
   ========================================= */
.backlinks-section { margin-top: 80px; border-top: 1px solid #eaeaea; padding-top: 24px; .section-title { font-size: 14px; font-weight: 600; color: #37352f; margin-bottom: 16px; text-transform: uppercase; } .backlink-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 12px; } .backlink-card { padding: 12px 16px; border: 1px solid #eaeaea; border-radius: 6px; cursor: pointer; transition: all 0.2s; background: #fff; &:hover { border-color: #d4d4d4; box-shadow: 0 4px 12px rgba(0,0,0,0.05); transform: translateY(-1px); } .bl-title { font-size: 14px; color: #333; font-weight: 500; } } }
.sync-indicator { position: fixed; bottom: 20px; right: 20px; font-size: 12px; color: #999; background: #fff; padding: 6px 12px; border-radius: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); pointer-events: none; transition: all 0.3s; border: 1px solid #eee; &.is-syncing { color: #2383e2; border-color: #2383e2; } }
.fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>

<style>
  /* 1. 选中容器 */
  .ProseMirror ul[data-type="taskList"] {
    list-style: none !important;
    padding: 0 !important;
    margin: 0 !important;
  }

  /* 2. 选中 LI (不再依赖 data-type属性，直接选中子元素) */
  .ProseMirror ul[data-type="taskList"] > li {
    display: flex !important;       /* 核心：强制横向排列 */
    flex-direction: row !important;
    align-items: flex-start !important; /* 顶部对齐 */
    margin-bottom: 8px !important;
    position: relative !important;
  }

  /* 3. 左侧 Label (复选框包裹层) */
  .ProseMirror ul[data-type="taskList"] > li > label {
    flex: 0 0 auto !important;      /* 固定宽度，不许缩放 */
    margin-right: 8px !important;   /* 与文字的间距 */
    user-select: none !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    width: 20px !important;
    height: 24px !important;        /* 高度与文字行高一致，确保垂直居中 */
    margin-top: 2px !important;     /* 微调垂直位置 */
  }

  /* 4. 右侧文字容器 (截图里是 div > p) */
  .ProseMirror ul[data-type="taskList"] > li > div {
    flex: 1 1 auto !important;      /* 占满剩余空间 */
    min-width: 0 !important;        /* 防止被撑爆 */
    margin: 0 !important;
    padding: 0 !important;
  }

  /* 5. 💀 关键：杀掉 P 标签的 margin，防止错位 */
  .ProseMirror ul[data-type="taskList"] > li > div > p {
    margin: 0 !important;
    padding: 0 !important;
    line-height: 1.6 !important;
    display: block !important;
  }

  /* 6. 复选框样式美化 */
  .ProseMirror ul[data-type="taskList"] > li input[type="checkbox"] {
    appearance: none !important;
    -webkit-appearance: none !important;
    background-color: #fff !important;
    margin: 0 !important;
    width: 18px !important;
    height: 18px !important;
    border: 2px solid #cbd5e1 !important;
    border-radius: 4px !important;
    cursor: pointer !important;
    display: grid !important;
    place-content: center !important;
  }

  /* 勾选后的对勾 */
  .ProseMirror ul[data-type="taskList"] > li input[type="checkbox"]::before {
    content: "" !important;
    width: 10px !important;
    height: 10px !important;
    transform: scale(0) !important;
    transition: 0.1s transform ease-in-out !important;
    box-shadow: inset 1em 1em white !important;
    transform-origin: center !important;
    clip-path: polygon(14% 44%, 0 65%, 50% 100%, 100% 16%, 80% 0%, 43% 62%) !important;
  }

  /* 选中状态 */
  .ProseMirror ul[data-type="taskList"] > li input[type="checkbox"]:checked {
    background-color: #2383e2 !important;
    border-color: #2383e2 !important;
  }
  .ProseMirror ul[data-type="taskList"] > li input[type="checkbox"]:checked::before {
    transform: scale(1) !important;
  }

  /* 7. 完成状态 (根据截图里的 data-checked="true/false") */
  .ProseMirror ul[data-type="taskList"] > li[data-checked="true"] > div {
    text-decoration: line-through !important;
    color: #999 !important;
  }
</style>