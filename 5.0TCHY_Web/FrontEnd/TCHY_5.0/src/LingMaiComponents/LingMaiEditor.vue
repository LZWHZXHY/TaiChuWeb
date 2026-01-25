<template>
  <div class="editor-scroll-container" @click="handleEditorClick">
    <div class="editor-header">
      
      <div class="header-toolbar">
        
        <div class="view-switcher">
          <button 
            :class="{ active: viewMode === 'doc' }" 
            @click="viewMode = 'doc'"
          >
            <span class="icon">📄</span> 文档
          </button>
          <button 
            :class="{ active: viewMode === 'kanban' }" 
            @click="viewMode = 'kanban'"
          >
            <span class="icon">📊</span> 看板
          </button>
        </div>
        
        <div class="spacer"></div>
        
        <button 
          v-if="note.parentNoteId" 
          class="action-btn" 
          @click="handleMoveToRoot" 
          title="移出父节点 (变为根页面)"
        >
          ⬆️ 移出
        </button>
        
        <button class="delete-btn" @click="handleDelete" title="删除此页面">
          <span class="trash-icon">🗑️</span> 
          <span class="btn-text">删除页面</span>
        </button>
      </div>

      <input v-model="note.title" class="title-field" placeholder="无标题" @blur="syncTitle" />
      <div class="meta-info">
        <span class="time-label">上次更新</span> 
        {{ formatDate(note.updatedAt) }}
      </div>
    </div>

    <div v-if="viewMode === 'doc'" class="fade-in">
      <div class="editor-body">
        <editor-content :editor="editor" />
      </div>

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
// 🔥 1. 引入 Image 扩展
import Image from '@tiptap/extension-image'
import tippy from 'tippy.js'
import apiClient from '@/utils/api'
import SuggestionList from '@/LingMaiComponents/SuggestionList.vue'
import SlashCommand from './slashCommand' 
import KanbanBoard from './KanbanBoard.vue' 

const props = defineProps(['noteId'])
const emit = defineEmits(['navigate', 'deleted']) 

const viewMode = ref('doc')

const handleNavigate = (id) => {
   emit('navigate', id)
   viewMode.value = 'doc' 
}

const note = reactive({ 
  id: props.noteId, 
  title: '', 
  updatedAt: new Date(),
  parentNoteId: null 
})
const syncing = ref(false)
const backlinks = ref([])

// 🔥 2. 通用上传图片函数
const uploadImage = async (file) => {
  const formData = new FormData()
  // 注意：这里的 'file' 必须和你后端 Controller 的参数名一致 (IFormFile file)
  formData.append('file', file) 

  try {
    // 调用你的 COS 后端接口
    const res = await apiClient.post('/Upload/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    // 后端返回格式: { url: "https://..." }
    return res.data.url
  } catch (e) {
    console.error("上传图片失败", e)
    alert("图片上传失败，请检查网络或配置")
    return null
  }
}

const editor = useEditor({
  extensions: [
    StarterKit,
    Placeholder.configure({ placeholder: '输入 / 唤起命令，输入 [[ 建立关联，或直接粘贴图片...' }),
    SlashCommand,
    // 🔥 3. 启用 Image 扩展
    Image.configure({ inline: true }),
    Mention.configure({
      HTMLAttributes: { class: 'internal-link' },
      renderHTML({ options, node }) {
        return [
          'span',
          {
            class: 'internal-link',
            'data-id': node.attrs.id,
            style: 'color: #0078d4; cursor: pointer; text-decoration: underline; font-weight: 500;' 
          },
          `@${node.attrs.label ?? node.attrs.id}`,
        ]
      },
      suggestion: {
        char: '[[',
        items: async ({ query }) => {
          try {
            const res = await apiClient.get(`/Notes/search?query=${encodeURIComponent(query || '')}`)
            return res.data.map(n => ({ id: n.Id, label: n.Title }))
          } catch (e) {
            console.error("搜索失败", e)
            return []
          }
        },
        render: () => {
          let component, popup
          return {
            onStart: props => {
              component = new VueRenderer(SuggestionList, { props, editor: props.editor })
              popup = tippy('body', { getReferenceClientRect: props.clientRect, appendTo: () => document.body, content: component.element, showOnCreate: true, interactive: true, trigger: 'manual', placement: 'bottom-start' })
            },
            onUpdate(props) { component.updateProps(props); popup[0].setProps({ getReferenceClientRect: props.clientRect }) },
            onKeyDown(props) { 
              if (props.event.key === 'Escape') { popup[0].hide(); return true }
              return component.ref?.onKeyDown(props) 
            },
            onExit() { popup[0].destroy(); component.destroy() }
          }
        }
      }
    })
  ],
  // 🔥 4. 拦截粘贴和拖拽事件
  editorProps: {
    // 处理粘贴 (Ctrl+V)
    handlePaste: (view, event) => {
      const items = (event.clipboardData || event.originalEvent.clipboardData).items
      for (const item of items) {
        if (item.type.indexOf('image') === 0) {
          event.preventDefault() // 阻止默认行为
          const file = item.getAsFile()
          
          // 执行上传
          uploadImage(file).then(url => {
            if (url) {
              const { schema } = view.state
              const node = schema.nodes.image.create({ src: url }) // 创建图片节点
              const transaction = view.state.tr.replaceSelectionWith(node)
              view.dispatch(transaction)
            }
          })
          return true 
        }
      }
      return false
    },
    // 处理拖拽 (Drag & Drop)
    handleDrop: (view, event, slice, moved) => {
      if (!moved && event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files.length > 0) {
        const file = event.dataTransfer.files[0]
        if (file.type.indexOf('image') === 0) {
          event.preventDefault()
          
          // 获取鼠标放置的坐标
          const coordinates = view.posAtCoords({ left: event.clientX, top: event.clientY })
          
          uploadImage(file).then(url => {
            if (url) {
               const { schema } = view.state
               const node = schema.nodes.image.create({ src: url })
               // 在鼠标位置插入图片
               const transaction = view.state.tr.insert(coordinates.pos, node)
               view.dispatch(transaction)
            }
          })
          return true
        }
      }
      return false
    }
  },
  onUpdate: ({ editor }) => { debouncedSave(editor.getJSON()) }
})

const loadData = async (id) => {
  if (!editor.value) return
  
  try {
    const res = await apiClient.get(`/Notes/detail/${id}`)
    
    note.title = res.data.Title || ''
    note.updatedAt = res.data.UpdatedAt
    note.parentNoteId = res.data.ParentNoteId 
    
    const content = res.data.ContentJson
    if (content && content !== '""') {
      try {
        editor.value.commands.setContent(JSON.parse(content))
      } catch (e) {
        editor.value.commands.setContent('')
      }
    } else {
      editor.value.commands.setContent('')
    }
    
    loadBacklinks(id)
  } catch (e) {
    console.error("加载失败", e)
    note.title = "加载失败"
    editor.value?.commands.setContent(`<p>无法加载笔记内容: ${e.message}</p>`)
  }
}

const loadBacklinks = async (id) => {
  try {
    const blRes = await apiClient.get(`/Notes/${id}/backlinks`)
    backlinks.value = blRes.data 
  } catch (e) {
    backlinks.value = []
  }
}

let timer = null
const debouncedSave = (json) => {
  clearTimeout(timer)
  syncing.value = true
  timer = setTimeout(async () => {
    try {
      await apiClient.post('/Notes/save', { 
        id: note.id, 
        title: note.title, 
        contentJson: JSON.stringify(json),
        parentNoteId: note.parentNoteId 
      })
      note.updatedAt = new Date()
    } catch (e) { console.error("保存失败", e) } 
    finally { syncing.value = false }
  }, 2000)
}

const syncTitle = () => { if (editor.value) debouncedSave(editor.value.getJSON()) }

const handleEditorClick = (event) => {
  const target = event.target.closest('.internal-link')
  if (target) {
    const targetId = target.getAttribute('data-id')
    if (targetId) emit('navigate', targetId)
  }
}

// 🔥 5. “移出父节点”逻辑
const handleMoveToRoot = async () => {
  try {
    // 调用 save 接口，将 parentNoteId 显式设为 null
    await apiClient.post('/Notes/save', { 
        id: note.id, 
        title: note.title, 
        contentJson: JSON.stringify(editor.value.getJSON()),
        parentNoteId: null // 设为 null，即变为根节点
    })
    note.parentNoteId = null
    // 触发 deleted 事件通知父组件刷新侧边栏结构
    emit('deleted', note.id) 
    alert("已成功移至根目录")
  } catch (e) {
    console.error(e)
  }
}

const handleDelete = async () => {
  // 🔥 核心修复：按下删除键的一瞬间，必须立即终止所有待处理的自动保存！
  if (timer) clearTimeout(timer)
  syncing.value = false 
  
  if (!confirm(`⚠️ 确定要删除 "${note.title || '当前页面'}" 吗？\n删除后无法恢复！`)) {
    return
  }
  
  try {
    await apiClient.delete(`/Notes/delete/${note.id}`)
    emit('deleted', note.id)
  } catch (e) {
    alert("删除失败")
  }
}

const handleBacklinkClick = (targetId) => emit('navigate', targetId)
const formatDate = (d) => d ? new Date(d).toLocaleString('zh-CN', { hour12: false }) : ''

watch(
  [() => props.noteId, editor], 
  ([newId, newEditor]) => {
    if (newId && newEditor) {
      note.id = newId
      loadData(newId)
    }
  }, 
  { immediate: true }
)

onBeforeUnmount(() => { 
  // 🔥 核心修复：组件都要销毁了，就别保存了
  if (timer) clearTimeout(timer)
  
  editor.value?.destroy() 
})
</script>

<style lang="scss">
/* --- 全局布局调整 --- */
.editor-scroll-container { 
  max-width: 900px; 
  margin: 0 auto; 
  padding: 40px 60px; 
  min-height: 100%; 
  background: #fff;
}

/* --- ✨ 顶部工具栏 --- */
.editor-header { 
  margin-bottom: 40px; 
  border-bottom: 1px solid rgba(0,0,0,0.06); 
  padding-bottom: 20px;
}

.header-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.view-switcher {
  display: inline-flex;
  background-color: #f3f3f3; 
  padding: 3px;
  border-radius: 8px;
  position: relative;
  border: 1px solid rgba(0,0,0,0.04);
  
  button {
    position: relative;
    z-index: 2; 
    border: none;
    background: transparent;
    padding: 6px 16px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 13px;
    font-weight: 500;
    color: #666;
    display: flex;
    align-items: center;
    gap: 6px;
    transition: color 0.2s ease;
    
    .icon { font-size: 14px; }

    &:hover { color: #333; }
    
    &.active {
      color: #000;
      background: #fff;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06);
    }
  }
}

/* 🔥 新增按钮样式 */
.action-btn {
  background: transparent;
  border: 1px solid #eee;
  color: #666;
  font-size: 13px;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
  margin-right: 8px;
  transition: all 0.2s;
  
  &:hover { background: #f5f5f5; color: #333; border-color: #ddd; }
}

.delete-btn {
  background: transparent;
  border: none;
  color: #9ca3af; 
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  border-radius: 6px;
  transition: all 0.2s ease;

  .trash-icon { font-size: 14px; }
  .btn-text { display: none; }

  &:hover {
    background-color: #fee2e2; 
    color: #dc2626; 
    
    .btn-text { 
      display: inline; 
    }
  }
}

.title-field { 
  width: 100%; 
  font-size: 40px; 
  font-weight: 700; 
  border: none; 
  outline: none; 
  margin-bottom: 8px; 
  color: #111; 
  background: transparent; 
  line-height: 1.2;
  
  &::placeholder { color: #e5e5e5; } 
}

.meta-info { 
  font-size: 12px; 
  color: #999; 
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  
  .time-label { margin-right: 6px; opacity: 0.7; }
}

/* --- Tiptap 内容区美化 --- */
.ProseMirror { 
  outline: none; 
  min-height: 400px; 
  font-size: 16px; 
  line-height: 1.75; 
  color: #37352f; 
  margin-top: 20px;
  
  blockquote { border-left: 3px solid #333; padding-left: 14px; margin: 1.5em 0; font-style: italic; color: #555; background: transparent; }
  pre { background: #f7f6f3; border-radius: 4px; padding: 16px; font-family: monospace; code { background: none; color: inherit; } }
  code { background-color: rgba(97, 97, 97, 0.1); color: #eb5757; padding: 0.25rem; border-radius: 4px; font-size: 0.85rem; }
  
  /* 🔥 图片样式 */
  img {
    max-width: 100%;
    height: auto;
    border-radius: 4px;
    margin: 10px 0;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    
    /* 选中时的边框 */
    &.ProseMirror-selectednode { 
      outline: 2px solid #0078d4; 
    }
  }
}

.backlinks-section { margin-top: 80px; border-top: 1px solid #eaeaea; padding-top: 24px; .section-title { font-size: 14px; font-weight: 600; color: #37352f; margin-bottom: 16px; text-transform: uppercase; } .backlink-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 12px; } .backlink-card { padding: 12px 16px; border: 1px solid #eaeaea; border-radius: 6px; cursor: pointer; transition: all 0.2s; background: #fff; &:hover { border-color: #d4d4d4; box-shadow: 0 4px 12px rgba(0,0,0,0.05); transform: translateY(-1px); } .bl-title { font-size: 14px; color: #333; font-weight: 500; } } }
.sync-indicator { position: fixed; bottom: 20px; right: 20px; font-size: 12px; color: #999; background: #fff; padding: 6px 12px; border-radius: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); pointer-events: none; transition: all 0.3s; border: 1px solid #eee; &.is-syncing { color: #2383e2; border-color: #2383e2; } }
.fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
.kanban-wrapper { margin-top: 20px; }
</style>