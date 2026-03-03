<template>
  <div class="wiki-editor-wrapper">
    <header class="editor-header">
      <div class="header-left">
        <span class="status-badge" :class="{ 'status-saved': saveStatus === '草稿已暂存' }">
          {{ saveStatus }}
        </span>
      </div>
      <div class="header-actions">
        <button class="btn btn-ghost" @click="$emit('cancel')">放弃修改</button>
        <button class="btn btn-primary" @click="saveChanges">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
          保存并提交
        </button>
      </div>
    </header>

    <div class="editor-document">
      <input type="text" v-model="localTitle" class="doc-title-input" placeholder="无标题词条..." />

      <div class="editor-meta-bar">
        <label class="meta-label">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"></path></svg>
          归属目录：
        </label>
        <select v-model="localCategoryId" class="category-select">
          <option disabled value="">请选择一个归属目录</option>
          <option v-for="cat in flattenedCategories" :key="cat.id" :value="cat.id">
            {{ cat.name }}
          </option>
        </select>
      </div>

      <div class="editor-toolbar">
        <div class="format-tools">
          <button title="粗体" @click="insertFormat('**', '**')"><b>B</b></button>
          <button title="斜体" @click="insertFormat('*', '*')"><i>I</i></button>
          <div class="divider"></div>
          <button title="二级标题" @click="insertFormat('## ', '')">H2</button>
          <button title="三级标题" @click="insertFormat('### ', '')">H3</button>
          <div class="divider"></div>
          <button title="无序列表" @click="insertFormat('- ', '')">•</button>
          <button title="代码块" @click="insertFormat('```\n', '\n```')">&lt;/&gt;</button>
          <div class="divider"></div>
          <button title="上传图片" @click="triggerImageUpload">🖼️</button>
          <button title="关联内部词条" @click="insertFormat('[[', ']]')" style="color: #2563eb; font-weight: 800;">[[ ]]</button>
        </div>

        <div class="mode-switch">
          <button :class="{ active: activeTab === 'write' }" @click="activeTab = 'write'">撰写</button>
          <button :class="{ active: activeTab === 'preview' }" @click="activeTab = 'preview'">预览</button>
        </div>

        <input 
          type="file" 
          ref="imageInputRef" 
          style="display: none" 
          accept="image/*" 
          @change="handleImageUpload" 
        />
      </div>

      <div class="editor-workspace">
        <textarea 
          v-show="activeTab === 'write'"
          ref="textareaRef"
          v-model="localContent" 
          class="markdown-textarea"
          placeholder="从这里开始编写宇宙的真理... (支持 Markdown 语法)"
        ></textarea>

        <div v-show="activeTab === 'preview'" class="markdown-preview markdown-body" v-html="previewHtml"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import { marked } from 'marked' 
import apiClient from '@/utils/api'

const props = defineProps({ 
  article: Object,
  treeData: {
    type: Array,
    default: () => []
  }
})
const emit = defineEmits(['save', 'cancel'])

// 基础响应式数据
const localTitle = ref(props.article.title || '')
const localContent = ref(props.article.rawMarkdown || props.article.contentHtml || '')
const localCategoryId = ref(props.article.categoryId || 1)
const activeTab = ref('write') 
const textareaRef = ref(null)
const imageInputRef = ref(null)

// 保存状态文字
const saveStatus = ref('正在编辑草稿')

// 本地草稿的 Key，带上文章 ID 确保唯一性
const DRAFT_KEY = computed(() => `wiki_draft_${props.article.id || 'new'}`)

// --- 核心逻辑：草稿处理 ---

// 1. 初始化：挂载时加载本地草稿
onMounted(() => {
  const savedDraft = localStorage.getItem(DRAFT_KEY.value)
  if (savedDraft) {
    try {
      const { title, content, categoryId } = JSON.parse(savedDraft)
      // 如果草稿存在，询问用户或直接恢复（这里采用直接恢复）
      if (content || title) {
        localTitle.value = title
        localContent.value = content
        localCategoryId.value = categoryId
        saveStatus.value = '已恢复本地草稿'
      }
    } catch (e) {
      console.error("解析草稿失败", e)
    }
  }
})

// 2. 实时保存：监听所有变动并存入 LocalStorage
watch([localTitle, localContent, localCategoryId], () => {
  saveStatus.value = '正在同步到本地...'
  
  const draftData = {
    title: localTitle.value,
    content: localContent.value,
    categoryId: localCategoryId.value,
    timestamp: Date.now()
  }
  
  localStorage.setItem(DRAFT_KEY.value, JSON.stringify(draftData))
  
  // 模拟一个保存成功的反馈感
  setTimeout(() => {
    saveStatus.value = '草稿已暂存'
  }, 500)
}, { deep: true })

// --- 原有 Wiki 逻辑 ---

const flattenedCategories = computed(() => {
  const flatten = (nodes, level = 0) => {
    let list = []
    if (!nodes || !Array.isArray(nodes)) return list;
    nodes.forEach(node => {
      const prefix = '　'.repeat(level) + (level > 0 ? '├─ ' : '')
      const catName = node.Name || node.name || node.Title || node.title || '未命名'
      const catId = node.Id || node.id
      list.push({ id: catId, name: prefix + catName })
      if (node.Children && node.Children.length > 0) {
        list = list.concat(flatten(node.Children, level + 1))
      }
    })
    return list
  }
  return flatten(props.treeData)
})

const previewHtml = computed(() => {
  return localContent.value ? marked.parse(localContent.value) : '<p class="empty-tip">没有可以预览的内容...</p>'
})

const triggerImageUpload = () => {
  imageInputRef.value.click()
}

const handleImageUpload = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const res = await apiClient.post('/Wiki/upload-image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    const imageUrl = res.data.url
    const imageMarkdown = `\n![${file.name}](${imageUrl})\n`
    
    const textarea = textareaRef.value
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    
    localContent.value = 
      localContent.value.substring(0, start) + 
      imageMarkdown + 
      localContent.value.substring(end)
  } catch (error) {
    console.error("图片上传失败:", error)
    alert("图片上传失败")
  } finally {
    event.target.value = ''
  }
}

const insertFormat = (prefix, suffix) => {
  const textarea = textareaRef.value;
  if (!textarea) return;
  const start = textarea.selectionStart;
  const end = textarea.selectionEnd;
  const selectedText = localContent.value.substring(start, end);
  const newText = prefix + selectedText + suffix;
  localContent.value = localContent.value.substring(0, start) + newText + localContent.value.substring(end);
  setTimeout(() => {
    textarea.focus();
    textarea.setSelectionRange(start + prefix.length, start + prefix.length + selectedText.length);
  }, 0);
}

const saveChanges = () => {
  if (!localTitle.value.trim()) { alert("标题不能为空！"); return; }
  if (!localCategoryId.value) { alert("请选择归属目录！"); return; }
  
  // 提交给父组件
  emit('save', { 
    title: localTitle.value, 
    content: localContent.value,
    categoryId: localCategoryId.value 
  })

  // 🚀 重点：提交成功后清理本地草稿，防止下次进入加载旧数据
  localStorage.removeItem(DRAFT_KEY.value)
}
</script>

<style scoped>
.wiki-editor-wrapper { flex: 1; display: flex; flex-direction: column; background-color: #f8fafc; overflow-y: auto; padding: 0 24px 40px 24px; }
.editor-header { position: sticky; top: 0; z-index: 10; display: flex; justify-content: space-between; align-items: center; padding: 16px 0; background-color: #f8fafc; }

/* 状态标签样式 */
.status-badge { font-size: 12px; color: #fbbf24; background: #fffbeb; padding: 4px 10px; border-radius: 12px; font-weight: 600; border: 1px solid #fde68a; transition: all 0.3s; }
.status-saved { color: #10b981; background: #ecfdf5; border-color: #a7f3d0; }

.header-actions { display: flex; gap: 12px; }
.btn { display: flex; align-items: center; gap: 6px; padding: 8px 16px; border-radius: 6px; font-size: 14px; font-weight: 500; cursor: pointer; transition: all 0.2s; }
.btn-ghost { background: transparent; border: 1px solid #cbd5e1; color: #475569; }
.btn-ghost:hover { background: #e2e8f0; color: #0f172a; }
.btn-primary { background: #2563eb; border: 1px solid #2563eb; color: #ffffff; box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2); }
.btn-primary:hover { background: #1d4ed8; }

.editor-document { max-width: 860px; margin: 0 auto; width: 100%; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.04), 0 1px 3px rgba(0, 0, 0, 0.02); display: flex; flex-direction: column; min-height: 700px; }
.doc-title-input { width: 100%; font-size: 36px; font-weight: 800; color: #0f172a; border: none; padding: 40px 48px 16px 48px; background: transparent; outline: none; border-radius: 12px 12px 0 0; }
.doc-title-input::placeholder { color: #cbd5e1; font-weight: 600; }

.editor-meta-bar { padding: 0 48px 16px 48px; display: flex; align-items: center; gap: 8px; }
.meta-label { display: flex; align-items: center; gap: 4px; font-size: 13px; color: #64748b; font-weight: 600; }
.category-select { padding: 6px 12px; border-radius: 6px; border: 1px solid #e2e8f0; background: #f8fafc; font-size: 13px; color: #334155; outline: none; cursor: pointer; transition: border-color 0.2s; font-family: monospace; }
.category-select:focus { border-color: #3b82f6; background: #ffffff; box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1); }

.editor-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 8px 48px; border-bottom: 1px solid #f1f5f9; background: #ffffff; position: sticky; top: 60px; z-index: 9; }
.format-tools { display: flex; align-items: center; gap: 4px; }
.format-tools button { background: transparent; border: none; width: 32px; height: 32px; border-radius: 4px; color: #64748b; font-size: 14px; cursor: pointer; transition: background 0.2s; }
.format-tools button:hover { background: #f1f5f9; color: #0f172a; }
.divider { width: 1px; height: 16px; background: #e2e8f0; margin: 0 8px; }

.mode-switch { display: flex; background: #f1f5f9; padding: 4px; border-radius: 6px; }
.mode-switch button { border: none; background: transparent; padding: 4px 16px; font-size: 13px; font-weight: 500; color: #64748b; border-radius: 4px; cursor: pointer; transition: all 0.2s; }
.mode-switch button.active { background: #ffffff; color: #2563eb; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }

.editor-workspace { flex: 1; display: flex; flex-direction: column; }
.markdown-textarea { flex: 1; width: 100%; padding: 24px 48px 60px 48px; border: none; resize: none; font-family: 'JetBrains Mono', monospace; font-size: 15px; line-height: 1.8; color: #334155; outline: none; background: transparent; }
.markdown-textarea::placeholder { color: #94a3b8; }
.markdown-preview { flex: 1; padding: 24px 48px 60px 48px; color: #334155; line-height: 1.8; }
.empty-tip { color: #94a3b8; font-style: italic; text-align: center; margin-top: 40px; }
</style>