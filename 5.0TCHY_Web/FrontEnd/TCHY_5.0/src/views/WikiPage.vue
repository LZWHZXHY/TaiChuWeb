<template>
  <div class="pro-wiki-app">
    <GlobalHeader 
      @create="handleCreateArticle" 
      @open-search="handleSearch" 
    />

    <div class="wiki-layout">
      <SidebarLeft 
        :treeData="directoryTree" 

        :activeId="String(currentArticle?.id || '')"
        @select-article="fetchArticle" 
      />

      <main class="main-content-area">
        <div v-if="isLoading" class="loading-state">
          <div class="spinner"></div>
          <p>正在连接寰宇数据库...</p>
        </div>

        <WikiEditor 
          v-else-if="isEditing"
          :article="currentArticle"
          @save="handleSave"
          @cancel="isEditing = false"
        />

        <ArticleViewer 
          v-else-if="currentArticle"
          :article="currentArticle" 
          @edit="isEditing = true" 
        />

        <div v-else class="loading-state">
          <p>请从左侧选择一个词条开始阅读</p>
        </div>
      </main>

      <SidebarRight 
        v-show="!isEditing && !isLoading && currentToc.length"
        :tocList="currentToc" 
        :activeId="activeHeadingId"
        @scroll-to="handleScroll" 
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api' 
import { marked } from 'marked'     

// 组件引入
import GlobalHeader from '@/WikiComponents/WikiHeader.vue'
import SidebarLeft from '@/WikiComponents/WikiSidebarLeft.vue'
import ArticleViewer from '@/WikiComponents/WikiViewer.vue'
import SidebarRight from '@/WikiComponents/WikiSidebarRight.vue'
import WikiEditor from '@/WikiComponents/WikiEditor.vue'

// 💡 确保路径正确：引入你的 Pinia Auth Store
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()

// --- 核心状态 ---
const isLoading = ref(true)
const isEditing = ref(false)
const currentArticle = ref(null)
const directoryTree = ref([]) 
const currentToc = ref([])
const activeHeadingId = ref('')

// --- 生命周期：初始化加载 ---
onMounted(async () => {
  await loadCategoryTree() // 加载左侧目录
  fetchArticle('1')        // 默认加载 1 号词条
})

// --- 业务逻辑 ---

// 1. 获取左侧目录树
const loadCategoryTree = async () => {
  try {
    const res = await apiClient.get('/Wiki/category-tree')
    directoryTree.value = res.data ?? res 
  } catch (error) {
    console.error('加载分类目录失败:', error)
  }
}

// 2. 新建文章逻辑
const handleCreateArticle = () => {
  // 权限检查
  if (!authStore.isAuthenticated) {
    alert("请先登录后再创建新词条！")
    return
  }

  // 准备空白数据
  currentArticle.value = {
    id: 0, // 后端据此识别为“新建”
    title: "",
    rawMarkdown: "",
    contentHtml: "",
    metadata: { author: authStore.user?.username || '当前用户', updatedAt: '刚刚' },
    breadcrumbs: ['首页', '新建词条']
  }

  // 切换到编辑模式
  isEditing.value = true 
}

// 3. 提交保存逻辑
const handleSave = async (editData) => {
  if (!authStore.isAuthenticated || !authStore.userID) {
    alert("登录状态异常，请重新登录！")
    return
  }

  const payload = {
    ArticleId: currentArticle.value.id, 
    UserId: authStore.userID, 
    Title: editData.title,
    ContentMarkdown: editData.content,
    EditSummary: currentArticle.value.id === 0 ? "创建新词条" : "修改词条内容"
  }
  
  try {
    const res = await apiClient.post('/Wiki/submit-edit', payload)
    alert("提交成功，请等待管理员审核！")
    
    // 💡 修复：提交成功后自动退出编辑模式
    isEditing.value = false
    
    // 如果是修改旧文章，可以考虑重新刷新一下当前内容
    if (payload.ArticleId !== 0) {
        // fetchArticle(payload.ArticleId) 
    }
  } catch (error) {
    console.error("提交失败:", error)
    alert("提交失败，请检查网络或后端接口")
  }
}

// 4. 获取文章详情并解析
const fetchArticle = async (id) => {
  if (!id) return
  isLoading.value = true
  isEditing.value = false 
  
  try {
    const res = await apiClient.get(`/Wiki/article/${id}`)
    const data = res.data ?? res

    // TOC 大纲解析逻辑
    const toc = []
    if (data.rawMarkdown) {
      const lines = data.rawMarkdown.split('\n')
      lines.forEach(line => {
        const match = line.match(/^(#{1,3})\s+(.*)/)
        if (match) {
          const level = match[1].length
          const text = match[2].trim()
          const anchorId = text.toLowerCase().replace(/[^a-zA-Z0-9\u4e00-\u9fa5]+/g, '-')
          toc.push({ id: anchorId, text, level })
        }
      })
    }
    currentToc.value = toc

    // 配置 Markdown 渲染器
    const renderer = new marked.Renderer()
    renderer.heading = function({ text, depth }) {
      const anchorId = text.toLowerCase().replace(/[^a-zA-Z0-9\u4e00-\u9fa5]+/g, '-')
      return `<h${depth} id="${anchorId}">${text}</h${depth}>`
    }

    currentArticle.value = {
      id: data.id,
      title: data.title,
      breadcrumbs: data.breadcrumbs,
      metadata: data.metadata,
      rawMarkdown: data.rawMarkdown,
      contentHtml: marked.parse(data.rawMarkdown || '', { renderer })
    }

  } catch (error) {
    console.error('获取文章详情失败:', error)
    currentArticle.value = null
  } finally {
    isLoading.value = false
  }
}

// --- 交互逻辑 ---
const handleScroll = (headingId) => {
    activeHeadingId.value = headingId
    const el = document.getElementById(headingId)
    if (el) el.scrollIntoView({ behavior: 'smooth' })
}

const handleSearch = () => {
    console.log('搜索功能待实现...')
    // 这里可以触发搜索弹窗的显示
}
</script>

<style scoped>
.pro-wiki-app { height: 100vh; display: flex; flex-direction: column; overflow: hidden; font-family: sans-serif; background: #fff;}
.wiki-layout { flex: 1; display: flex; overflow: hidden; }
.main-content-area { flex: 1; display: flex; flex-direction: column; overflow: hidden; background: #fff; }
.loading-state { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #64748b; }
.spinner { width: 40px; height: 40px; margin-bottom: 16px; border: 4px solid #f1f5f9; border-top: 4px solid #2563eb; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>