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
          <div v-for="bl in backlinks" :key="bl.id" class="backlink-card" @click="handleBacklinkClick(bl.id)">
            <div class="bl-title">📄 {{ bl.title }}</div>
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

import { BlockId } from './BlockId'
import BlockEmbed from './BlockEmbedNode.js'


const isDataLoaded = ref(false) // 标记数据是否真正同步完成

const uploadProgress = ref(0) // 上传进度 (0-100)
const isUploading = ref(false) // 是否正在上传

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
  const formData = new FormData()
  formData.append('file', file)
  
  isUploading.value = true
  uploadProgress.value = 0
  
  try {
    const res = await apiClient.post('/Upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      // 关键：Axios 自带的进度回调
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        uploadProgress.value = percentCompleted
      }
    })
    return res.data.url
  } catch (e) {
    console.error('图片上传失败', e)
    return null
  } finally {
    // 延迟消失，让用户看清“100%”
    setTimeout(() => {
      isUploading.value = false
      uploadProgress.value = 0
    }, 1000)
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
    KanbanNode,
    StarterKit.configure({
      // 关键：禁用掉 StarterKit 自带的图片功能，防止它干扰 ResizeImage
      image: false, 
    }),
    BlockId, 
    BlockEmbed, 
    HeatmapNode,
    Placeholder.configure({ placeholder: '输入 / 唤起命令，输入 [[ 建立关联...' }), 
    SlashCommand, 
    TaskList,
    TaskItem.extend({
      content: 'block+', 
    }).configure({
      nested: true,
    }),
    

    Image.configure({ inline: true }),
    // 后放缩放（后定义的会覆盖前面的渲染逻辑）
    ResizeImage.extend({
      name: 'image', // 核心操作：把插件名从 'imageResize' 改为 'image'
    }).configure({
      inline: true,
      // 其他你需要的配置
    }),
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
        items: async ({ query }) => {
          console.log('当前 Mention Query:', query);

          if (query && query.startsWith('#')) {
            const blockQuery = query.substring(1); 
            
            try {
              console.log('检测到块搜索标识，正在发起请求...', blockQuery);
              const res = await apiClient.get(`/Blocks/search?query=${encodeURIComponent(blockQuery)}`);
              
              return res.data.map(b => ({
                id: b.blockId,
                label: b.previewText,
                type: 'block', 
                noteTitle: b.noteTitle
              }));
            } catch (e) {
              console.error('块搜索请求失败:', e);
              return [];
            }
          }

          try {
            const res = await apiClient.get(`/Notes/search?query=${encodeURIComponent(query || '')}`);
            return res.data.map(n => ({
              // 🔥 修复：n.Id -> n.id，n.Title -> n.title
              id: n.id,
              label: n.title,
              type: 'note'
            }));
          } catch (e) {
            console.error('文档搜索请求失败:', e);
            return [];
          }
        },

        command: ({ editor, range, props }) => {
          if (props.type === 'block') {
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
  const items = (event.clipboardData || event.originalEvent.clipboardData).items;
  
  for (const item of items) {
    if (item.type.indexOf('image') === 0) {
      event.preventDefault();
      const file = item.getAsFile();
      
      uploadImage(file).then(url => {
        if (url) {
          const { schema } = view.state;
          
          // --- 关键修改点 ---
          // 确保使用的 node 名是 'image'（因为你已经把 Resize 插件改名为 image 了）
          // 并且增加默认的 width 属性，否则有些缩放插件在没有宽度值时不会显示手柄
          const node = schema.nodes.image.create({ 
            src: url,
            width: '100%', // 初始给个 100% 宽度，方便后续缩放
          });
          
          const transaction = view.state.tr.replaceSelectionWith(node);
          view.dispatch(transaction);
        }
      });
      return true; // 表示已处理该粘贴事件
    }
  }
  return false;
},
    handleDrop: (view, event, slice, moved) => {
      if (!moved && event.dataTransfer && event.dataTransfer.files.length > 0) { const file = event.dataTransfer.files[0]; if (file.type.indexOf('image') === 0) { event.preventDefault(); const coordinates = view.posAtCoords({ left: event.clientX, top: event.clientY }); uploadImage(file).then(url => { if (url) { const { schema } = view.state; const node = schema.nodes.image.create({ src: url }); view.dispatch(view.state.tr.insert(coordinates.pos, node)) } }); return true } } return false
    }
  },
  onUpdate: ({ editor }) => { 
  // 如果还没加载完，直接跳过，不触发保存
  if (!isDataLoaded.value) return; 

  debouncedSave(editor.getJSON()); 
  updateToc(editor); 
},
  onCreate: ({ editor }) => { updateToc(editor) }
})

const loadData = async (id) => {
  if (!editor.value) return
  
  // 1. 明确关锁，禁止任何自动保存
  isDataLoaded.value = false 
  
  try {
    const res = await apiClient.get(`/Notes/detail/${id}`); 
    note.title = res.data.title || ''; 
    note.updatedAt = res.data.updatedAt; 
    note.parentNoteId = res.data.parentNoteId; 
    const content = res.data.contentJson;

    if (content && content !== '""') { 
      const jsonContent = JSON.parse(content);

      // --- 【新增：防自杀校验逻辑】 ---
      // 检查 JSON 中所有的节点类型，编辑器 Schema 是否全部支持
      // 如果不支持（如缺少 Image 插件），Tiptap 会静默丢弃节点，导致数据丢失
      const schemaNodes = editor.value.schema.nodes;
      
      console.log('编辑器当前支持的节点:', Object.keys(editor.value.schema.nodes));


      // 递归检查 JSON 树中是否存在编辑器无法识别的 type
      const checkSchemaMatch = (node) => {
        if (node.type && !schemaNodes[node.type]) return node.type;
        if (node.content) {
          for (const child of node.content) {
            const missing = checkSchemaMatch(child);
            if (missing) return missing;
          }
        }
        return null;
      };

      const missingType = checkSchemaMatch(jsonContent);
      if (missingType) {
        throw new Error(`编辑器配置错误：缺少 [${missingType}] 节点扩展。为防止覆盖原始数据，已拦截渲染。`);
      }
      // --- 【校验结束】 ---

      try { 
        // 这一步会触发 onUpdate，但因为 isDataLoaded 是 false，会被拦截
        editor.value.commands.setContent(jsonContent); 
        updateToc(editor.value);
        
        // 记录一个初始的“纯净快照”，用于后续比对内容是否真的发生了变化
        // (需要在 script setup 顶层定义 let lastContentHash = '')
        // lastContentHash = JSON.stringify(jsonContent); 

      } catch (e) { 
        throw new Error(`内容解析失败：${e.message}`);
      } 
    } else { 
      editor.value.commands.setContent('');
    }
    
    loadBacklinks(id);
    
    // 3. 关键：确保 Tiptap 处理完所有事务后再开锁
    // 使用 300ms 延迟比 100ms 更稳妥，避开所有宏任务/微任务的波动
    setTimeout(() => {
      isDataLoaded.value = true;
      console.log('✅ 灵脉加载完成，自动保存已就绪');
    }, 300);

  } catch (e) { 
    note.title = "加载受阻"; 
    // 加载失败的情况下，绝对不开启 isDataLoaded 锁！
    isDataLoaded.value = false; 
    
    // 给用户显眼的报错提示，而不是空白内容
    editor.value?.commands.setContent(`
      <div style="border: 2px dashed #ff4d4f; padding: 20px; border-radius: 8px; color: #ff4d4f;">
        <h3>⚠️ 数据加载异常</h3>
        <p>${e.message}</p>
        <p>请检查插件配置或刷新页面。当前自动保存已禁用，以保护数据库数据。</p>
      </div>
    `);
  }
}

const loadBacklinks = async (id) => { 
  try { 
    const blRes = await apiClient.get(`/Notes/${id}/backlinks`); 
    backlinks.value = blRes.data 
  } catch (e) { 
    backlinks.value = [] 
  } 
}

let timer = null
let isSaving = false 

const debouncedSave = (json) => {
  clearTimeout(timer);
  syncing.value = true;

  timer = setTimeout(async () => {
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
  }, 2000); 
}

const syncTitle = () => { if (editor.value) debouncedSave(editor.value.getJSON()) }

const handleEditorClick = (event) => {
  const target = event.target.closest('.internal-link'); if (target) { emit('navigate', target.getAttribute('data-id')); return }

  if (event.altKey) {
    const blockNode = event.target.closest('[data-block-id]');
    if (blockNode) {
      const bId = blockNode.getAttribute('data-block-id');
      navigator.clipboard.writeText(bId);
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

  .tiptap-image-resizer {
    display: inline-block !important;
    position: relative !important;
    line-height: 0;
    vertical-align: middle;
    user-select: none;

    /* 关键：给整个缩放容器一个高层级，确保手柄在图片上方 */
    z-index: 10; 

    /* 强行绘制四个角的小方块 */
    /* 不同的插件类名可能叫 .handler, .resizer-handler, 或 .resize-trigger */
    [class*="handler"], 
    [class*="resize-trigger"],
    .resizer {
      display: block !important;
      position: absolute !important;
      width: 10px !important;  /* 宽度 */
      height: 10px !important; /* 高度 */
      background-color: #2383e2 !important; /* 方块颜色：蓝色 */
      border: 1.5px solid #ffffff !important; /* 白色边框，让它更清晰 */
      border-radius: 2px !important;
      z-index: 100 !important;
      cursor: nwse-resize; /* 鼠标悬停显示缩放图标 */
      
      /* 强制显示 */
      visibility: visible !important;
      opacity: 1 !important;
    }

    /* 精确控制四个角的位置（如果插件没对齐，这里可以微调） */
    .handler-top-left { top: -5px; left: -5px; cursor: nwse-resize; }
    .handler-top-right { top: -5px; right: -5px; cursor: nesw-resize; }
    .handler-bottom-left { bottom: -5px; left: -5px; cursor: nesw-resize; }
    .handler-bottom-right { bottom: -5px; right: -5px; cursor: nwse-resize; }
  }

  /* 解决蓝色边框和方块重叠导致看不清的问题 */
  .ProseMirror-selectednode {
    outline: 2px solid #2383e2 !important;
    outline-offset: 2px; /* 让边框往外扩一点，露出方块 */
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