<template>
  <div class="wiki-editor-wrapper">
    <div class="blog-drawer" :class="{ 'drawer-open': isDrawerOpen }">
      <div class="drawer-header">
        <div class="drawer-title">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
          博客正文预览
        </div>
        <button class="close-drawer-btn" @click="isDrawerOpen = false">✕</button>
      </div>
      <div class="drawer-body markdown-body" v-if="drawerLoading">
        <div class="loading-placeholder">正在加载宇宙中的文字...</div>
      </div>
      <div class="drawer-body markdown-body" v-else v-html="drawerContentHtml"></div>
    </div>

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
          
          <button title="引用我的博客文章" @click="openBlogSelector" style="color: #059669;">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"></path><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"></path></svg>
          </button>
        </div>

        <div class="mode-switch">
          <button :class="{ active: activeTab === 'write' }" @click="activeTab = 'write'">撰写</button>
          <button :class="{ active: activeTab === 'preview' }" @click="activeTab = 'preview'">预览</button>
        </div>
      </div>

      <div class="editor-workspace">
        <textarea 
          v-show="activeTab === 'write'"
          ref="textareaRef"
          v-model="localContent" 
          class="markdown-textarea"
          placeholder="从这里开始编写宇宙的真理... (支持 Markdown 语法)"
        ></textarea>

        <div 
          v-show="activeTab === 'preview'" 
          class="markdown-preview markdown-body" 
          @click="handlePreviewClick"
          v-html="previewHtml"
        ></div>
      </div>
    </div>

    <div v-if="showBlogSelector" class="modal-overlay" @click.self="showBlogSelector = false">
      <div class="blog-selector-modal">
        <div class="modal-header">
          <span>选择要关联的文章</span>
          <button @click="showBlogSelector = false">✕</button>
        </div>
        <div class="modal-search">
          <input type="text" v-model="blogSearchQuery" placeholder="搜索标题或内容摘要..." />
        </div>
        <div class="blog-list" v-if="!loadingBlogs">
          <div 
            v-for="blog in filteredBlogs" 
            :key="blog.Id" 
            class="blog-item" 
            @click="insertBlogEmbed(blog)"
          >
            <div class="blog-item-title">{{ blog.Title }}</div>
            <div class="blog-item-meta">
              <span>📅 {{ blog.publishTime }}</span>
              <span>👁️ {{ blog.ViewCount }}</span>
            </div>
          </div>
          <div v-if="filteredBlogs.length === 0" class="empty-list">没有找到相关文章</div>
        </div>
        <div v-else class="modal-loading">正在拉取列表...</div>
      </div>
    </div>

    <input type="file" ref="imageInputRef" style="display: none" accept="image/*" @change="handleImageUpload" />
  </div>
</template>

<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import { marked } from 'marked' 
import apiClient from '@/utils/api'

const props = defineProps({ 
  article: { type: Object, default: () => ({ id: 0 }) }, 
  treeData: { type: Array, default: () => [] }
})
const emit = defineEmits(['save', 'cancel'])

const localTitle = ref(props.article.title || '')
const localContent = ref(props.article.rawMarkdown || props.article.contentHtml || '')
const localCategoryId = ref(props.article.categoryId || 1)
const activeTab = ref('write') 
const textareaRef = ref(null)
const imageInputRef = ref(null)
const saveStatus = ref('正在编辑草稿')

const showBlogSelector = ref(false)
const blogSearchQuery = ref('')
const allBlogs = ref([]) 
const loadingBlogs = ref(false)
const isDrawerOpen = ref(false)
const drawerLoading = ref(false)
const drawerContentHtml = ref('')

const DRAFT_KEY = computed(() => `wiki_draft_${props.article.id || 'new'}`)

onMounted(async () => {
  const savedDraft = localStorage.getItem(DRAFT_KEY.value)
  if (savedDraft) {
    try {
      const { title, content, categoryId } = JSON.parse(savedDraft)
      localTitle.value = title
      localContent.value = content
      localCategoryId.value = categoryId
      saveStatus.value = '已恢复本地草稿'
    } catch (e) { console.error("Draft error", e) }
  }
  fetchBlogList()
})

const fetchBlogList = async () => {
  loadingBlogs.value = true
  try {
    const res = await apiClient.get('/Blog/articles?page=1&pageSize=50')
    allBlogs.value = res.data.list || []
  } catch (e) {
    console.error("Failed to fetch blogs", e)
  } finally {
    loadingBlogs.value = false
  }
}

watch([localTitle, localContent, localCategoryId], () => {
  saveStatus.value = '正在同步到本地...'
  const draftData = {
    title: localTitle.value,
    content: localContent.value,
    categoryId: localCategoryId.value,
    timestamp: Date.now()
  }
  localStorage.setItem(DRAFT_KEY.value, JSON.stringify(draftData))
  setTimeout(() => { saveStatus.value = '草稿已暂存' }, 500)
}, { deep: true })

const previewHtml = computed(() => {
  if (!localContent.value) return '<p class="empty-tip">暂无内容预览...</p>'
  
  let raw = localContent.value
  
  // 🚀 核心修复：将多行模板字符串改为单行，彻底消除 Markdown 把缩进识别为代码块的 BUG
  raw = raw.replace(/:::blog (\d+)\n([\s\S]*?):::/g, (match, id, title) => {
    return `<div class="blog-embed-card" data-blog-id="${id}"><div class="card-left">📖</div><div class="card-center"><div class="card-title">${title.trim()}</div><div class="card-subtitle">点击展开关联博客</div></div><div class="card-right">查看全文</div></div>`;
  })
  
  return marked.parse(raw)
})

const handlePreviewClick = async (e) => {
  const card = e.target.closest('.blog-embed-card')
  if (card) {
    const blogId = card.dataset.blogId
    isDrawerOpen.value = true
    drawerLoading.value = true
    try {
      const res = await apiClient.get(`/Blog/articles/${blogId}`)
      drawerContentHtml.value = marked.parse(res.data.content || '该文章暂无内容')
    } catch (err) {
      drawerContentHtml.value = '<p class="error-msg">内容获取失败，请确认文章是否已发布。</p>'
    } finally {
      drawerLoading.value = false
    }
  }
}

const openBlogSelector = () => showBlogSelector.value = true

const filteredBlogs = computed(() => {
  return allBlogs.value.filter(b => 
    b.Title.toLowerCase().includes(blogSearchQuery.value.toLowerCase()) ||
    b.Summary?.toLowerCase().includes(blogSearchQuery.value.toLowerCase())
  )
})

const insertBlogEmbed = (blog) => {
  const embedTag = `\n:::blog ${blog.Id}\n${blog.Title}\n:::\n`
  insertFormat(embedTag, '')
  showBlogSelector.value = false
}

const triggerImageUpload = () => imageInputRef.value.click()





const handleImageUpload = async (event) => {
  const file = event.target.files[0]
  if (!file) return
  
  // 不需要 articleId 了
  const formData = new FormData()
  formData.append('file', file) // 这里的 'file' 对应后端 IFormFile file
  
  try {
    // 1. 改为请求 /Wiki/upload-image 
    // 2. 移除尾部的 /${articleId}
    const res = await apiClient.post(`/Wiki/upload-image`, formData)
    insertFormat(`\n![图片](${res.data.url})\n`, '')
  } catch (e) { 
    console.error(e)
    alert("图片上传失败，请检查是否已登录或后端配置") 
  } finally {
    event.target.value = ''
  }
}





const insertFormat = (prefix, suffix) => {
  const textarea = textareaRef.value
  if (!textarea) return
  const start = textarea.selectionStart
  const end = textarea.selectionEnd
  const selectedText = localContent.value.substring(start, end)
  localContent.value = localContent.value.substring(0, start) + prefix + selectedText + suffix + localContent.value.substring(end)
  setTimeout(() => {
    textarea.focus()
    textarea.setSelectionRange(start + prefix.length, start + prefix.length + selectedText.length)
  }, 0)
}

const flattenedCategories = computed(() => {
  const flatten = (nodes, level = 0) => {
    let list = []
    if (!nodes || !Array.isArray(nodes)) return list
    nodes.forEach(node => {
      const prefix = '　'.repeat(level) + (level > 0 ? '├─ ' : '')
      const catName = node.Name || node.name || node.Title || node.title || '未命名'
      list.push({ id: node.Id || node.id, name: prefix + catName })
      if (node.Children?.length > 0) list = list.concat(flatten(node.Children, level + 1))
    })
    return list
  }
  return flatten(props.treeData)
})

const saveChanges = () => {
  if (!localTitle.value.trim()) { alert("词条标题不能为空"); return }
  emit('save', {
    title: localTitle.value,
    content: localContent.value,
    categoryId: localCategoryId.value
  })
  localStorage.removeItem(DRAFT_KEY.value)
}
</script>

<style scoped>
/* 0. 全局盒模型与重置 */
.wiki-editor-wrapper {
  box-sizing: border-box;
  width: 100%;
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: #f8fafc;
  color: #334155;
  overflow-y: auto; /* 关键：让最外层可以滚动，避免高度塌陷 */
  position: relative;
}

.wiki-editor-wrapper *, 
.wiki-editor-wrapper *::before, 
.wiki-editor-wrapper *::after {
  box-sizing: inherit;
}

/* 1. 顶部 Header (固定吸顶) */
.editor-header {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 24px;
  background-color: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid #e2e8f0;
}

.status-badge {
  font-size: 13px;
  padding: 6px 14px;
  border-radius: 20px;
  font-weight: 600;
  background: #fffbeb;
  color: #d97706;
  border: 1px solid #fde68a;
  transition: all 0.3s ease;
}
.status-saved {
  background: #ecfdf5;
  color: #059669;
  border-color: #a7f3d0;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.2s;
}
.btn-ghost {
  background: #f8fafc;
  color: #475569;
  border-color: #cbd5e1;
}
.btn-ghost:hover {
  background: #f1f5f9;
  color: #0f172a;
}
.btn-primary {
  background: #2563eb;
  color: #ffffff;
  box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2);
}
.btn-primary:hover {
  background: #1d4ed8;
}

/* 2. 编辑器主体文档 (居中白板) */
.editor-document {
  max-width: 900px;
  margin: 24px auto 60px auto;
  width: 100%;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  min-height: calc(100vh - 140px);
}

.doc-title-input {
  width: 100%;
  font-size: 32px;
  font-weight: 800;
  color: #0f172a;
  border: none;
  padding: 40px 48px 16px 48px;
  outline: none;
  background: transparent;
  border-radius: 12px 12px 0 0;
}
.doc-title-input::placeholder {
  color: #cbd5e1;
}

.editor-meta-bar {
  padding: 0 48px 16px 48px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.meta-label {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  color: #64748b;
  font-weight: 600;
}
.category-select {
  padding: 6px 12px;
  border-radius: 6px;
  border: 1px solid #e2e8f0;
  background: #f8fafc;
  font-size: 14px;
  color: #334155;
  outline: none;
  cursor: pointer;
  min-width: 200px;
  transition: border-color 0.2s;
}
.category-select:focus {
  border-color: #3b82f6;
}

/* 3. 工具栏 (内部吸顶) */
.editor-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 48px;
  border-bottom: 1px solid #f1f5f9;
  border-top: 1px solid #f1f5f9;
  background: #fafafa;
  position: sticky;
  top: 61px; /* 紧贴在最顶部 Header 的下方 */
  z-index: 90;
}

.format-tools {
  display: flex;
  align-items: center;
  gap: 4px;
}
.format-tools button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  border-radius: 6px;
  color: #475569;
  font-size: 15px;
  cursor: pointer;
  transition: background 0.2s;
}
.format-tools button:hover {
  background: #e2e8f0;
  color: #0f172a;
}
/* 修复：丢失的分割线样式 */
.divider {
  width: 1px;
  height: 18px;
  background: #cbd5e1;
  margin: 0 8px;
}

.mode-switch {
  display: flex;
  background: #e2e8f0;
  padding: 4px;
  border-radius: 8px;
}
.mode-switch button {
  border: none;
  background: transparent;
  padding: 6px 16px;
  font-size: 13px;
  font-weight: 600;
  color: #64748b;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}
.mode-switch button.active {
  background: #ffffff;
  color: #2563eb;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

/* 4. 工作区 (编辑 & 预览) */
.editor-workspace {
  flex: 1;
  display: flex;
  flex-direction: column;
}
.markdown-textarea {
  flex: 1;
  min-height: 400px;
  width: 100%;
  padding: 24px 48px;
  border: none;
  resize: vertical;
  font-family: 'Consolas', monospace;
  font-size: 15px;
  line-height: 1.8;
  color: #334155;
  outline: none;
  background: transparent;
}
.markdown-preview {
  flex: 1;
  padding: 24px 48px;
  min-height: 400px;
}
.empty-tip {
  color: #94a3b8;
  font-style: italic;
  text-align: center;
  margin-top: 40px;
}

/* 5. 侧边预览抽屉 (Drawer) */
.blog-drawer {
  position: fixed;
  top: 0;
  right: -600px;
  width: 550px;
  height: 100vh;
  background: #ffffff;
  box-shadow: -10px 0 30px rgba(0,0,0,0.1);
  z-index: 2000;
  transition: right 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
}
.blog-drawer.drawer-open {
  right: 0;
}
.drawer-header {
  padding: 20px 24px;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fafafa;
}
.drawer-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 700;
  color: #1e293b;
  font-size: 16px;
}
.close-drawer-btn {
  border: none;
  background: transparent;
  font-size: 20px;
  cursor: pointer;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 50%;
}
.close-drawer-btn:hover {
  background: #e2e8f0;
  color: #0f172a;
}
.drawer-body {
  flex: 1;
  overflow-y: auto;
  padding: 32px;
  line-height: 1.8;
}
.loading-placeholder {
  text-align: center;
  color: #94a3b8;
  margin-top: 100px;
  font-size: 14px;
}

/* 6. 博客预览卡片 (注入到 Markdown 中的样式) */
:deep(.blog-embed-card) {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-left: 4px solid #10b981;
  border-radius: 8px;
  margin: 20px 0;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
:deep(.blog-embed-card:hover) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.1);
  border-color: #10b981;
}
:deep(.card-left) {
  font-size: 24px;
}
:deep(.card-center) {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
:deep(.card-title) {
  font-weight: 700;
  color: #0f172a;
  font-size: 15px;
}
:deep(.card-subtitle) {
  font-size: 12px;
  color: #64748b;
}
:deep(.card-right) {
  font-size: 13px;
  font-weight: 600;
  color: #059669;
  background: #f0fdf4;
  padding: 6px 12px;
  border-radius: 20px;
}

/* 7. 博客选择弹窗 Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(2px);
  z-index: 3000;
  display: flex;
  align-items: center;
  justify-content: center;
}
.blog-selector-modal {
  background: #ffffff;
  width: 500px;
  border-radius: 12px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.modal-header {
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 700;
  font-size: 16px;
  color: #0f172a;
  border-bottom: 1px solid #e2e8f0;
  background: #f8fafc;
}
.modal-header button {
  background: transparent;
  border: none;
  font-size: 18px;
  color: #64748b;
  cursor: pointer;
}
.modal-search {
  padding: 16px 24px;
  border-bottom: 1px solid #f1f5f9;
}
.modal-search input {
  width: 100%;
  padding: 10px 14px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  outline: none;
  font-size: 14px;
  transition: border-color 0.2s;
}
.modal-search input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}
.blog-list {
  max-height: 400px;
  overflow-y: auto;
  padding: 8px 16px 16px 16px;
}
.blog-item {
  padding: 12px 16px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  border-bottom: 1px solid #f8fafc;
}
.blog-item:last-child {
  border-bottom: none;
}
.blog-item:hover {
  background: #f0fdf4;
}
.blog-item-title {
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 6px;
  font-size: 14px;
}
.blog-item-meta {
  font-size: 12px;
  color: #64748b;
  display: flex;
  gap: 16px;
}
.empty-list, .modal-loading {
  padding: 32px;
  text-align: center;
  color: #94a3b8;
  font-size: 14px;
}
</style>