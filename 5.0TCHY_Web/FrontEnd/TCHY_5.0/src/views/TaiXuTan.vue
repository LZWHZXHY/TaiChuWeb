<template>
  <div class="container">
    <!-- 头部 -->
    <header class="forum-header">
      <h1 class="forum-title">太虚坛</h1>
      <p class="forum-subtitle">太初寰宇 - 自由交流地</p>
    </header>
    
    <!-- 主内容区 -->
    <main>
      <div class="posts-header">
        <h2 class="posts-title"><i class="fas fa-stream"></i> 最新帖子</h2>
        <div class="filter-controls">
          <button 
            v-for="filter in filters" 
            :key="filter.id"
            :class="['filter-btn', { active: activeFilter === filter.id }]"
            @click="changeFilter(filter.id)"
          >
            {{ filter.name }}
          </button>
        </div>
      </div>
      
      <div class="posts-list">
        <!-- 帖子列表 -->
        <div 
          v-for="post in posts" 
          :key="post.id"
          class="post-item"
          @click="viewPostDetail(post.id)"
        >
          <!-- 帖子图片预览 -->
          <div v-if="post.images && post.images.length > 0" class="post-images-preview">
            <img 
              v-for="(img, index) in getDisplayImages(post.images)" 
              :key="index"
              :src="getImageUrl(img)" 
              :alt="'帖子图片' + (index + 1)"
              class="post-preview-image"
              @error="handleImageError"
              @load="handleImageLoad"
              loading="lazy"
            />
            <div v-if="post.images.length > 3" class="more-images">
              +{{ post.images.length - 3 }}
            </div>
          </div>
          
          <h3 class="post-title">{{ post.post_title }}</h3>
          <p class="post-excerpt">{{ post.excerpt || truncateText(post.content, 100) }}</p>
          <div class="post-meta">
            <div class="post-author">
              <span class="author-avatar">{{ getAuthorInitial(post.author) }}</span>
              <span>{{ post.author.username }}</span>
            </div>
            <div class="post-tags">
              <span class="post-tag">{{ getPostTypeName(post.post_type) }}</span>
            </div>
          </div>
          <div class="post-stats">
            <span><i class="far fa-eye"></i> {{ post.view_count || 0 }}</span>
            <span><i class="far fa-comment"></i> {{ post.comment_count || 0 }}</span>
            <span><i class="far fa-clock"></i> {{ formatTime(post.createTime) }}</span>
          </div>
        </div>
        
        <!-- 加载状态 -->
        <div v-if="loading" class="loading-state">
          <i class="fas fa-spinner fa-spin"></i> 加载中...
        </div>
        
        <!-- 加载更多按钮 -->
        <button 
          v-if="!loading && hasMore"
          class="load-more"
          @click="loadMore"
          :disabled="loading"
        >
          <i class="fas fa-chevron-down"></i> 加载更多
        </button>
        
        <!-- 没有更多数据提示 -->
        <div v-if="!hasMore && posts.length > 0" class="no-more">
          没有更多帖子了
        </div>
        
        <!-- 空状态 -->
        <div v-if="!loading && posts.length === 0" class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>暂无帖子</p>
        </div>
      </div>
    </main>
    
    <!-- 右侧边栏 -->
    <aside class="sidebar">
      <!-- 发帖表单 -->
      <div class="post-form">
        <h3 class="form-title"><i class="fas fa-edit"></i> 发布新帖</h3>
        
        <div class="form-group">
          <label for="post-title">帖子标题 *</label>
          <input 
            type="text" 
            id="post-title" 
            v-model="newPost.title"
            placeholder="请输入标题（必填）"
            maxlength="50"
            class="form-input"
          >
          <div class="char-counter">{{ newPost.title.length }}/50</div>
        </div>
        
        <div class="form-group">
          <label for="post-content">帖子内容 *</label>
          <textarea 
            id="post-content" 
            v-model="newPost.content"
            placeholder="请输入内容（必填）"
            maxlength="2000"
            class="form-textarea"
            rows="4"
          ></textarea>
          <div class="char-counter">{{ newPost.content.length }}/2000</div>
        </div>
        
        <!-- 图片上传区域 -->
        <div class="form-group">
          <label for="post-images">上传图片（最多10张）</label>
          <input 
            type="file" 
            id="post-images"
            ref="fileInput"
            multiple 
            accept="image/*,.gif"
            @change="handleImageSelect"
            class="form-input"
          >
          
          <!-- 图片预览区域 -->
          <div v-if="selectedImages.length > 0" class="image-preview-container">
            <div class="image-preview-list">
              <div 
                v-for="(image, index) in selectedImages" 
                :key="index"
                class="image-preview-item"
              >
                <img 
                  :src="getImagePreviewUrl(image)" 
                  :alt="'预览图' + (index + 1)"
                  class="preview-image"
                />
                <button 
                  type="button" 
                  class="remove-image-btn"
                  @click="removeImage(index)"
                >
                  <i class="fas fa-times"></i>
                </button>
                <div class="image-info">
                  <span class="image-name">{{ truncateText(image.name, 15) }}</span>
                  <span class="image-size">{{ (image.size / 1024 / 1024).toFixed(2) }}MB</span>
                </div>
              </div>
            </div>
            <div class="image-count">
              已选择 {{ selectedImages.length }}/10 张图片
            </div>
          </div>
        </div>
        
        <div class="form-group">
          <label for="post-type">帖子类型</label>
          <select id="post-type" v-model="newPost.post_type" class="form-select">
            <option value="0">柴圈帖子</option>
            <option value="1">游戏帖子</option>
            <option value="2">其他帖子</option>
          </select>
        </div>
        
        <div class="form-actions">
          <button 
            class="btn btn-primary"
            @click="submitPost"
            :disabled="posting || !newPost.title.trim() || !newPost.content.trim()"
          >
            <i class="fas fa-paper-plane"></i> 
            {{ posting ? '发布中...' : '发布' }}
          </button>
          <button 
            class="btn btn-secondary"
            @click="resetForm"
            :disabled="posting"
          >
            <i class="fas fa-times"></i> 重置
          </button>
        </div>
      </div>
    </aside>
    
    <!-- 帖子详情弹窗 -->
    <div v-if="showPostDetail" class="modal-overlay" @click="closePostDetail">
      <div class="post-detail-modal" @click.stop>
        <button class="close-modal" @click="closePostDetail">
          <i class="fas fa-times"></i>
        </button>
        
        <div v-if="currentPost" class="post-detail-content">
          <h2>{{ currentPost.post_title }}</h2>
          <div class="post-author-info">
            <span class="author-avatar">{{ getAuthorInitial(currentPost.author) }}</span>
            <div>
              <span class="author-name">{{ currentPost.author.username }}</span>
              <span class="post-time">{{ formatTime(currentPost.createTime) }}</span>
            </div>
          </div>
          
          <!-- 帖子图片展示 -->
          <div v-if="currentPost.images && currentPost.images.length > 0" class="post-detail-images">
            <img 
              v-for="(img, index) in currentPost.images" 
              :key="index"
              :src="getImageUrl(img)" 
              :alt="'帖子图片' + (index + 1)"
              class="detail-image"
              @error="handleImageError"
              @load="handleImageLoad"
              loading="lazy"
            />
          </div>
          
          <div class="post-body">
            <p>{{ currentPost.content }}</p>
          </div>
          
          <div class="post-stats">
            <span><i class="far fa-eye"></i> {{ currentPost.view_count || 0 }}</span>
            <span><i class="far fa-comment"></i> {{ currentPost.comment_count || 0 }}</span>
            <span><i class="far fa-heart"></i> {{ currentPost.like_count || 0 }}</span>
          </div>
        </div>
        
        <div v-if="modalLoading" class="modal-loading">
          <i class="fas fa-spinner fa-spin"></i> 加载中...
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

export default {
  name: 'TaixuForum',
  setup() {
    // 状态管理
    const posts = ref([])
    const loading = ref(false)
    const posting = ref(false)
    const hasMore = ref(true)
    const lastId = ref(null)
    const activeFilter = ref('all')
    const showPostDetail = ref(false)
    const currentPost = ref(null)
    const modalLoading = ref(false)
    
    // 新帖子表单
    const newPost = ref({
      title: '',
      content: '',
      post_type: 0
    })
    
    // 图片选择相关
    const selectedImages = ref([])
    const fileInput = ref(null)
    
    // 筛选器选项
    const filters = ref([
      { id: 'all', name: '全部' },
      { id: '0', name: '柴圈帖子' },
      { id: '1', name: '游戏帖子' },
      { id: '2', name: '其他帖子' }
    ])

    // ==================== 计算属性 ====================
    const filteredPosts = computed(() => {
      let filtered = posts.value
      if (activeFilter.value !== 'all') {
        filtered = filtered.filter(post => post.post_type === parseInt(activeFilter.value))
      }
      return filtered
    })

    const hasFilters = computed(() => {
      return activeFilter.value !== 'all'
    })

    // ==================== 核心方法 ====================
    
    // 获取帖子列表
    const fetchPosts = async () => {
      if (loading.value) return
      
      loading.value = true
      try {
        const params = {
          pageSize: 10
        }
        
        if (lastId.value) {
          params.lastId = lastId.value
        }
        
        if (activeFilter.value !== 'all') {
          params.postType = parseInt(activeFilter.value)
        }
        
        console.log('📡 请求参数:', params)
        console.log('🌍 当前环境:', import.meta.env.VITE_APP_ENV)
        console.log('🔗 API基础URL:', import.meta.env.VITE_API_BASE_URL)
        
        const response = await apiClient.get('/posts', { params })
        const data = response.data
        
        console.log('📸 API响应数据:', data)
        
        if (data.success) {
          if (params.lastId) {
            posts.value = [...posts.value, ...data.data]
          } else {
            posts.value = data.data
          }
          
          hasMore.value = data.pagination?.hasMore || false
          lastId.value = data.pagination?.lastId || null
          
          console.log('✅ 加载成功，当前帖子数:', posts.value.length)
        } else {
          console.error('❌ API返回失败:', data.message)
        }
      } catch (error) {
        console.error('❌ 获取帖子失败:', error)
        console.error('❌ 错误详情:', error.response?.data || error.message)
      } finally {
        loading.value = false
      }
    }

    // 加载更多帖子
    const loadMore = async () => {
      if (hasMore.value && !loading.value) {
        await fetchPosts()
      }
    }

    // 切换筛选器
    const changeFilter = async (filterId) => {
      activeFilter.value = filterId
      lastId.value = null
      posts.value = []
      await fetchPosts()
    }

    // 清除筛选
    const clearFilters = () => {
      activeFilter.value = 'all'
      lastId.value = null
      posts.value = []
      fetchPosts()
    }

    // ==================== 图片处理方法 ====================
    const getImageUrl = (img) => {
      if (!img) return '/土豆.jpg'
      
      // 如果已经是完整URL，直接返回
      if (typeof img === 'string' && img.startsWith('http')) {
        return img
      }
      
      // 如果是对象，尝试获取url属性
      if (typeof img === 'object' && img.url) {
        return img.url
      }
      
      // 如果是相对路径，构建完整URL
      if (typeof img === 'string') {
        const baseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/api.*$/, '') || 
                       (import.meta.env.DEV ? 'http://localhost:44359' : 'https://bianyuzhou.com')
        
        const cleanPath = img.trim().replace(/^\//, '')
        return `${baseUrl}/${cleanPath}`
      }
      
      return '/土豆.jpg'
    }

    // 获取显示的图片（最多3张）
    const getDisplayImages = (images) => {
      if (!images || !Array.isArray(images)) return []
      return images.slice(0, 3)
    }

    // 图片选择处理
    const handleImageSelect = (event) => {
      const files = Array.from(event.target.files)
      
      // 验证文件数量
      if (selectedImages.value.length + files.length > 10) {
        alert('最多只能上传10张图片')
        return
      }
      
      // 验证文件类型和大小
      const validFiles = files.filter(file => {
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif']
        const maxSize = 10 * 1024 * 1024 // 10MB
        
        if (!allowedTypes.includes(file.type)) {
          alert(`文件 ${file.name} 格式不支持，请选择图片文件`)
          return false
        }
        
        if (file.size > maxSize) {
          alert(`文件 ${file.name} 太大，请选择小于10MB的图片`)
          return false
        }
        
        return true
      })
      
      selectedImages.value.push(...validFiles)
    }

    // 移除图片
    const removeImage = (index) => {
      selectedImages.value.splice(index, 1)
    }

    // 获取图片预览URL
    const getImagePreviewUrl = (file) => {
      return URL.createObjectURL(file)
    }

    // 图片错误处理
    const handleImageError = (event) => {
      console.error('❌ 图片加载失败:', event.target.src)
      event.target.src = '/土豆.jpg'
      event.target.alt = '图片加载失败'
    }

    const handleImageLoad = (event) => {
      console.log('✅ 图片加载成功:', event.target.src)
    }

    // ==================== 发帖功能 ====================
    const submitPost = async () => {
      if (!newPost.value.title.trim()) {
        alert('请输入帖子标题')
        return
      }
      
      if (!newPost.value.content.trim()) {
        alert('请输入帖子内容')
        return
      }
      
      posting.value = true
      try {
        const formData = new FormData()
        formData.append('Title', newPost.value.title)
        formData.append('Content', newPost.value.content)
        formData.append('PostType', newPost.value.post_type.toString())
        
        // 添加图片文件
        if (selectedImages.value && selectedImages.value.length > 0) {
          selectedImages.value.forEach((file, index) => {
            formData.append('Images', file)
          })
        }
        
        console.log('📤 提交表单数据:')
        console.log('标题:', newPost.value.title)
        console.log('内容:', newPost.value.content)
        console.log('类型:', newPost.value.post_type)
        console.log('图片数量:', selectedImages.value.length)
        
        const response = await apiClient.post('/posts/create', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        })
        
        const result = response.data
        console.log('✅ 发布结果:', result)
        
        if (result.success) {
          alert('帖子发布成功！')
          resetForm()
          lastId.value = null
          await fetchPosts() // 刷新列表
        } else {
          alert('发布失败: ' + result.message)
        }
      } catch (error) {
        console.error('❌ 发布帖子失败:', error)
        console.error('❌ 错误详情:', error.response?.data || error.message)
        alert('发布失败，请重试')
      } finally {
        posting.value = false
      }
    }

    // 重置表单
    const resetForm = () => {
      newPost.value = {
        title: '',
        content: '',
        post_type: 0
      }
      selectedImages.value = []
      if (fileInput.value) {
        fileInput.value.value = ''
      }
    }

    // ==================== 帖子详情功能 ====================
    const viewPostDetail = async (postId) => {
      modalLoading.value = true
      showPostDetail.value = true
      
      try {
        console.log('🔍 查看帖子详情，ID:', postId)
        const response = await apiClient.get(`/posts/${postId}`)
        console.log('📄 帖子详情响应:', response.data)
        
        if (response.data.success) {
          currentPost.value = response.data.data
          console.log('✅ 帖子详情数据:', currentPost.value)
        }
      } catch (error) {
        console.error('❌ 获取帖子详情失败:', error)
        alert('获取帖子详情失败')
      } finally {
        modalLoading.value = false
      }
    }

    const closePostDetail = () => {
      showPostDetail.value = false
      currentPost.value = null
      modalLoading.value = false
    }

    // ==================== 工具函数 ====================
    const formatTime = (time) => {
      if (!time) return ''
      const date = new Date(time)
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      const hours = String(date.getHours()).padStart(2, '0')
      const minutes = String(date.getMinutes()).padStart(2, '0')
      return `${year}-${month}-${day} ${hours}:${minutes}`
    }

    const getAuthorInitial = (author) => {
      if (!author || !author.username) return '?'
      return author.username.charAt(0).toUpperCase()
    }

    const getPostTypeName = (type) => {
      const types = {
        0: '柴圈帖子',
        1: '游戏帖子',
        2: '其他帖子'
      }
      return types[type] || '未知类型'
    }

    const truncateText = (text, length) => {
      if (!text) return ''
      return text.length > length ? text.substring(0, length) + '...' : text
    }

    // ==================== 生命周期 ====================
    onMounted(() => {
      console.log('🚀 组件挂载，开始加载帖子...')
      fetchPosts()
    })

    return {
      // 状态
      posts,
      loading,
      posting,
      hasMore,
      activeFilter,
      filters,
      newPost,
      selectedImages,
      fileInput,
      showPostDetail,
      currentPost,
      modalLoading,
      
      // 计算属性
      hasFilters,
      
      // 方法
      fetchPosts,
      loadMore,
      changeFilter,
      clearFilters,
      handleImageSelect,
      removeImage,
      getImagePreviewUrl,
      getImageUrl,
      getDisplayImages,
      handleImageError,
      handleImageLoad,
      submitPost,
      resetForm,
      viewPostDetail,
      closePostDetail,
      formatTime,
      getAuthorInitial,
      getPostTypeName,
      truncateText
    }
  }
}
</script>

<style scoped>
/* 基础样式 */
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 25px;
  min-height: 100vh;
}

.forum-header {
  grid-column: 1 / -1;
  text-align: center;
  margin-bottom: 30px;
  padding: 25px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  color: white;
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.3);
}

.forum-title {
  font-size: 2.8rem;
  font-weight: 700;
  margin-bottom: 8px;
}

.forum-subtitle {
  font-size: 1.1rem;
  opacity: 0.9;
}

/* 帖子列表区域 */
.posts-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #eaeaea;
}

.posts-title {
  font-size: 1.5rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 10px;
}

.filter-controls {
  display: flex;
  gap: 8px;
}

.filter-btn {
  padding: 8px 16px;
  border: 1px solid #e0e0e0;
  border-radius: 20px;
  background: white;
  color: #666;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.9rem;
}

.filter-btn.active {
  background: #667eea;
  color: white;
  border-color: #667eea;
}

.filter-btn:hover {
  transform: translateY(-1px);
}

/* 帖子列表 */
.posts-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.post-item {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid #f0f0f0;
}

.post-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

/* 帖子图片预览 */
.post-images-preview {
  display: flex;
  gap: 8px;
  margin-bottom: 12px;
}

.post-preview-image {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.more-images {
  width: 80px;
  height: 80px;
  background: #f8f9fa;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #666;
  font-weight: 600;
}

.post-title {
  font-size: 1.2rem;
  font-weight: 600;
  margin-bottom: 12px;
  color: #2c3e50;
}

.post-excerpt {
  color: #666;
  line-height: 1.6;
  margin-bottom: 16px;
}

.post-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.85rem;
  color: #888;
  margin-bottom: 8px;
}

.post-author {
  display: flex;
  align-items: center;
  gap: 8px;
}

.author-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  font-weight: 600;
}

.post-tags {
  display: flex;
  gap: 6px;
}

.post-tag {
  background: #f8f9fa;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75rem;
  color: #666;
  border: 1px solid #e9ecef;
}

.post-stats {
  display: flex;
  gap: 15px;
  font-size: 0.8rem;
  color: #888;
}

/* 加载状态 */
.loading-state, .no-more, .empty-state {
  text-align: center;
  padding: 40px;
  color: #666;
}

.load-more {
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  background: white;
  color: #666;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  margin: 20px auto;
  display: block;
}

.load-more:hover:not(:disabled) {
  background: #f8f9fa;
}

.load-more:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 右侧边栏 */
.sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 发帖表单 */
.post-form {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  border: 1px solid #f0f0f0;
}

.form-title {
  font-size: 1.3rem;
  margin-bottom: 20px;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 10px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #2c3e50;
}

.form-input, .form-textarea, .form-select {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e9ecef;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.3s ease;
}

.form-input:focus, .form-textarea:focus, .form-select:focus {
  outline: none;
  border-color: #667eea;
}

.form-textarea {
  min-height: 120px;
  resize: vertical;
}

.char-counter {
  text-align: right;
  font-size: 0.8rem;
  color: #888;
  margin-top: 4px;
}

/* 图片预览 */
.image-preview-container {
  margin-top: 10px;
}

.image-preview-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 10px;
  margin-bottom: 10px;
}

.image-preview-item {
  position: relative;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 8px;
  background: #f8f9fa;
}

.preview-image {
  width: 100%;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
}

.remove-image-btn {
  position: absolute;
  top: 5px;
  right: 5px;
  width: 20px;
  height: 20px;
  border: none;
  border-radius: 50%;
  background: rgba(255, 0, 0, 0.7);
  color: white;
  cursor: pointer;
  font-size: 0.7rem;
}

.image-info {
  margin-top: 5px;
  font-size: 0.7rem;
}

.image-name, .image-size {
  display: block;
  color: #666;
}

.image-count {
  text-align: center;
  font-size: 0.8rem;
  color: #666;
  padding: 5px;
  background: #f0f0f0;
  border-radius: 4px;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.btn {
  padding: 12px 20px;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  flex: 1;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-1px);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f8f9fa;
  color: #666;
  border: 1px solid #e9ecef;
}

.btn-secondary:hover {
  background: #e9ecef;
}

/* 模态框 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
}

.post-detail-modal {
  background: white;
  border-radius: 12px;
  padding: 30px;
  width: 90%;
  max-width: 600px;
  max-height: 80vh;
  overflow-y: auto;
  position: relative;
}

.close-modal {
  position: absolute;
  top: 15px;
  right: 15px;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
}

.post-detail-content h2 {
  font-size: 1.5rem;
  margin-bottom: 16px;
  color: #2c3e50;
}

.post-author-info {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #f0f0f0;
}

.post-author-info .author-avatar {
  width: 40px;
  height: 40px;
  font-size: 1rem;
}

.author-name {
  font-weight: 600;
  color: #2c3e50;
}

.post-time {
  color: #888;
  font-size: 0.9rem;
}

/* 帖子详情图片 */
.post-detail-images {
  margin: 20px 0;
  display: grid;
  gap: 10px;
}

.detail-image {
  width: 100%;
  max-height: 400px;
  object-fit: contain;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.post-body {
  margin: 20px 0;
  line-height: 1.6;
  color: #2c3e50;
}

.post-stats {
  display: flex;
  gap: 20px;
  padding-top: 16px;
  border-top: 1px solid #f0f0f0;
  color: #888;
  font-size: 0.9rem;
}

.modal-loading {
  text-align: center;
  padding: 40px;
  color: #666;
}

/* 响应式设计 */
@media (max-width: 900px) {
  .container {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .posts-header {
    flex-direction: column;
    gap: 15px;
  }
  
  .filter-controls {
    justify-content: center;
    flex-wrap: wrap;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .modal-overlay {
    padding: 10px;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 15px;
  }
  
  .forum-header {
    padding: 20px 15px;
  }
  
  .forum-title {
    font-size: 2.2rem;
  }
  
  .post-form {
    padding: 20px;
  }
  
  .image-preview-list {
    grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  }
}
</style>