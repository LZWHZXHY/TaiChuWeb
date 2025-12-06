<template>
  <div class="modal-overlay" @click.self="closeModal" v-show="visible" role="dialog" aria-modal="true">
    <div class="post-detail-modal" @click.stop ref="modalRoot" tabindex="-1">
      <button class="close-modal" @click="closeModal" aria-label="关闭">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
          <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
      </button>

      <div v-if="loading" class="modal-loading">
        <div class="loading-spinner"></div>
        <span>加载中...</span>
      </div>

      <div v-else-if="post" class="post-detail-content">
        <!-- 帖子头部 -->
        <div class="post-header">
          <div class="post-meta">
            <div class="author-info">
              <div class="author-avatar">
                {{ getAuthorInitial(post.author) }}
              </div>
              <div class="author-details">
                <div class="author-name">{{ post.author?.username || '匿名用户' }}</div>
                <div class="post-time">{{ formatTime(post.createTime) }}</div>
              </div>
            </div>
            <button class="btn-reply" @click="openDrawer(null, '')">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" stroke="currentColor" stroke-width="2"/>
              </svg>
              回复
            </button>
          </div>
          
          <h1 class="post-title">{{ post.post_title || post.title || '无标题' }}</h1>
          
          <div class="post-tags">
            <span class="post-tag" :class="getPostTypeClass(post.post_type)">
              {{ getPostTypeText(post.post_type) }}
            </span>
          </div>
        </div>

        <!-- 帖子内容 -->
        <div class="post-content-section">
          <div class="post-content">
            <p>{{ post.content }}</p>
          </div>
          
          <!-- 图片展示 -->
          <div v-if="post.images && post.images.length" class="post-images">
            <div class="images-grid" :class="`grid-${Math.min(post.images.length, 4)}`">
              <div 
                v-for="(img, index) in post.images" 
                :key="index" 
                class="image-item"
                @click="openImageGallery(index)"
              >
                <!-- 实际图片元素：之前模板里缺失导致无图显示 -->
                <img
                  :src="getImageUrl(img)"
                  :alt="`图片 ${index + 1}`"
                  class="post-image"
                  loading="lazy"
                  @error="onImageError($event, index)"
                />

                <!-- 当超过 4 张图，第四张显示 “+n” 覆盖层 -->
                <div v-if="post.images.length > 4 && index === 3" class="image-more" aria-hidden="true">
                  +{{ post.images.length - 4 }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 帖子统计 -->
        <div class="post-stats">
          <div class="stat-item">
            <span class="stat-icon">👁</span>
            <span class="stat-value">{{ post.view_count || 0 }}</span>
            <span class="stat-label">浏览</span>
          </div>
          <div class="stat-item">
            <span class="stat-icon">💬</span>
            <span class="stat-value">{{ post.comment_count || 0 }}</span>
            <span class="stat-label">回复</span>
          </div>
          <div class="stat-item">
            <span class="stat-icon">❤️</span>
            <span class="stat-value">{{ post.like_count || 0 }}</span>
            <span class="stat-label">点赞</span>
          </div>
        </div>

        <!-- 回复列表 -->
        <div class="replies-section">
          <div class="section-header">
            <h3 class="section-title">回复</h3>
            <span class="section-count">{{ post.comment_count || 0 }} 条回复</span>
          </div>
          
          <ReplyList
            v-if="postId"
            :post-id="postId"
            @reply-open="openDrawer"
            @replies-loaded="onRepliesLoaded"
          />
        </div>
      </div>

      <div v-else-if="!loading" class="modal-error">
        <div class="error-icon">😵</div>
        <h3>帖子加载失败</h3>
        <p>无法加载帖子内容，请稍后重试</p>
        <button class="btn-retry" @click="fetchPost(props.postId)">重新加载</button>
      </div>

      <!-- 回复抽屉 -->
      <aside
        class="reply-drawer"
        :class="{ open: drawerOpen }"
        :aria-hidden="!drawerOpen"
      >
        <div class="drawer-overlay" @click="closeDrawer"></div>
        <div class="drawer-content">
          <div class="drawer-header">
            <div class="drawer-title">
              <span v-if="drawerParentName">回复：{{ drawerParentName }}</span>
              <span v-else>回复帖子</span>
            </div>
            <button class="drawer-close" @click="closeDrawer" aria-label="关闭回复面板">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
                <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
              </svg>
            </button>
          </div>

          <div class="drawer-body">
            <ReplyForm
              ref="replyFormRef"
              :post-id="postId"
              :parent-comment-id="drawerParentId"
              @posted="onDrawerPosted"
              @cancel="closeDrawer"
            />
          </div>
        </div>
      </aside>

      <!-- 图片查看器 -->
      <div v-if="imageViewer.open" class="image-viewer" @click="closeImageViewer">
        <button class="viewer-close" @click="closeImageViewer">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>
        <div class="viewer-content" @click.stop>
          <!-- 显示当前图片 -->
          <img
            v-if="post?.images?.length"
            :src="getImageUrl(post.images[imageViewer.currentIndex])"
            class="viewer-image"
            :alt="`大图 ${imageViewer.currentIndex + 1}`"
            loading="eager"
          />

          <div class="viewer-nav">
            <button class="nav-btn prev" @click="prevImage" :disabled="imageViewer.currentIndex === 0">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
                <path d="M15 18l-6-6 6-6" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
              </svg>
            </button>
            <span class="image-counter">{{ imageViewer.currentIndex + 1 }} / {{ post.images.length }}</span>
            <button class="nav-btn next" @click="nextImage" :disabled="imageViewer.currentIndex === post.images.length - 1">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
                <path d="M9 18l6-6-6-6" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup>
import { ref, watch, onMounted, nextTick } from 'vue'
import apiClient from '@/utils/api'
import ReplyList from './ReplyList.vue'
import ReplyForm from './ReplyForm.vue'

const props = defineProps({
  postId: { type: [String, Number], default: null },
  visible: { type: Boolean, default: false }
})
const emit = defineEmits(['close','reply-posted'])

const post = ref(null)
const loading = ref(false)
const modalRoot = ref(null)
const replyFormRef = ref(null)

// Drawer state
const drawerOpen = ref(false)
const drawerParentId = ref(null)
const drawerParentName = ref('')

// 图片查看器状态
const imageViewer = ref({
  open: false,
  currentIndex: 0
})

const fetchPost = async (id) => {
  if (!id) return
  loading.value = true
  try {
    const resp = await apiClient.get(`/posts/${id}`)
    if (resp.data && resp.data.success) {
      post.value = resp.data.data
      console.log('帖子数据:', post.value)
      console.log('帖子图片:', post.value.images)
    } else {
      post.value = null
    }
  } catch (e) {
    console.error('fetch post error', e)
    post.value = null
  } finally {
    loading.value = false
  }
}

// 监听帖子ID变化
watch(() => props.postId, (newId) => {
  if (!newId) {
    post.value = null
    drawerOpen.value = false
    drawerParentId.value = null
    drawerParentName.value = ''
    imageViewer.value.open = false
    imageViewer.value.currentIndex = 0
    return
  }
  fetchPost(newId)
})

// 监听可见性变化
watch(() => props.visible, (newVisible) => {
  if (newVisible && props.postId) {
    fetchPost(props.postId)
  }
})

onMounted(() => { 
  if (props.visible && props.postId) fetchPost(props.postId) 
})

const openDrawer = async (parentId = null, parentName = '') => {
  if (!props.postId) {
    console.warn('openDrawer called but postId is missing')
    return
  }
  drawerParentId.value = parentId
  drawerParentName.value = parentName || ''
  drawerOpen.value = true

  await nextTick()
  try {
    const ta = replyFormRef.value?.$el?.querySelector?.('textarea') || document.querySelector('.reply-drawer textarea')
    if (ta) ta.focus()
  } catch (e) {}
}

const closeDrawer = () => {
  try {
    const active = document.activeElement
    const drawerEl = document.querySelector('.reply-drawer')
    if (drawerEl && active && drawerEl.contains(active)) {
      modalRoot.value?.focus?.()
    }
  } catch (e) {}

  drawerOpen.value = false
  drawerParentId.value = null
  drawerParentName.value = ''
}

// 当回复发布后
const onDrawerPosted = async (newComment) => {
  if (post.value) {
    post.value.comment_count = (post.value.comment_count || 0) + 1
  }
  
  emit('reply-posted', newComment)
  closeDrawer()
}

// 当回复列表加载完成后更新计数
const onRepliesLoaded = (total) => {
  if (post.value) {
    post.value.comment_count = total
  }
}

const closeModal = () => {
  if (drawerOpen.value) {
    try { modalRoot.value?.focus?.() } catch {}
    drawerOpen.value = false
  }
  imageViewer.value.open = false
  emit('close')
}

// 图片查看器相关函数
const openImageGallery = (index) => {
  if (!post.value?.images?.length) {
    console.warn('没有图片可显示')
    return
  }
  
  // 确保 index 在合法范围内
  const idx = Math.max(0, Math.min(index || 0, post.value.images.length - 1))
  imageViewer.value.currentIndex = idx
  imageViewer.value.open = true
}

const closeImageViewer = () => {
  imageViewer.value.open = false
}

const prevImage = () => {
  if (imageViewer.value.currentIndex > 0) {
    imageViewer.value.currentIndex--
  }
}

const nextImage = () => {
  if (imageViewer.value.currentIndex < post.value.images.length - 1) {
    imageViewer.value.currentIndex++
  }
}

// 图片请求错误回退（可以替换为占位图）
const onImageError = (ev, index) => {
  // 隐藏出错图片，或替换为占位图
  ev.target.style.opacity = '0.4'
  ev.target.style.filter = 'grayscale(40%)'
  ev.target.alt = '图片加载失败'
  console.warn('图片加载失败，index=', index)
}

// 辅助函数
const getAuthorInitial = (author) => (author && author.username) ? author.username.charAt(0).toUpperCase() : '?'

const formatTime = (t) => { 
  if (!t) return ''
  const d = new Date(t)
  const now = new Date()
  const diff = now.getTime() - d.getTime()
  const hours = Math.floor(diff / (1000 * 60 * 60))
  
  if (hours < 1) return '刚刚'
  if (hours < 24) return `${hours}小时前`
  if (hours < 168) return `${Math.floor(hours / 24)}天前`
  return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}`
}

const getPostTypeText = (type) => {
  const types = { 0: '柴圈帖子', 1: '游戏帖子', 2: '其他帖子' }
  return types[type] || '其他帖子'
}

const getPostTypeClass = (type) => {
  const classes = { 0: 'type-chai', 1: 'type-game', 2: 'type-other' }
  return classes[type] || 'type-other'
}

const getImageUrl = (img) => {
  if (!img) {
    console.warn('图片数据为空')
    return ''
  }
  
  // 如果是完整URL
  if (typeof img === 'string' && img.startsWith('http')) {
    return img
  }
  
  // 如果是对象包含url属性
  if (typeof img === 'object' && img.url) {
    return img.url.startsWith('http') ? img.url : formatImageUrl(img.url)
  }
  
  // 如果是相对路径
  if (typeof img === 'string') {
    return formatImageUrl(img)
  }
  
  console.warn('无法处理的图片格式:', img)
  return ''
}

const formatImageUrl = (path) => {
  if (!path) return ''
  
  // 移除开头的斜杠
  const cleanPath = path.replace(/^\/+/, '')
  
  // 根据环境构建完整URL
  const baseUrl = import.meta.env.VITE_API_BASE_URL || ''
  if (baseUrl) {
    return `${baseUrl.replace(/\/api.*$/, '')}/${cleanPath}`
  } else {
    const isDev = import.meta.env.DEV
    const defaultBase = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com'
    return `${defaultBase}/${cleanPath}`
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1400;
  padding: 20px;
  backdrop-filter: blur(8px);
}

.post-detail-modal {
  width: 100%;
  max-width: 800px;
  max-height: 90vh;
  background: #ffffff;
  border-radius: 20px;
  overflow: hidden;
  position: relative;
  box-shadow: 0 32px 64px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
}

.close-modal {
  position: absolute;
  right: 20px;
  top: 20px;
  width: 44px;
  height: 44px;
  border-radius: 12px;
  border: none;
  background: rgba(0, 0, 0, 0.08);
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  transition: all 0.2s ease;
}

.close-modal:hover {
  background: rgba(0, 0, 0, 0.12);
  transform: scale(1.05);
}

.modal-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 40px;
  gap: 16px;
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #e2e8f0;
  border-top: 3px solid #0fa3a3;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.post-detail-content {
  flex: 1;
  overflow-y: auto;
  padding: 40px;
}

/* 帖子头部 */
.post-header {
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 24px;
  margin-bottom: 24px;
}

.post-meta {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.author-avatar {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: linear-gradient(135deg, #667eea, #764ba2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: white;
  font-size: 1.2rem;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.author-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.author-name {
  font-weight: 600;
  color: #1a202c;
  font-size: 1rem;
}

.post-time {
  color: #64748b;
  font-size: 0.9rem;
}

.btn-reply {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  background: #ffffff;
  color: #64748b;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-reply:hover {
  background: #f8fafc;
  border-color: #0fa3a3;
  color: #0fa3a3;
}

.post-title {
  font-size: 1.8rem;
  font-weight: 700;
  color: #1a202c;
  line-height: 1.3;
  margin: 0 0 16px 0;
}

.post-tags {
  display: flex;
  gap: 8px;
}

.post-tag {
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.type-chai {
  background: linear-gradient(135deg, #fed7d7, #feb2b2);
  color: #c53030;
}

.type-game {
  background: linear-gradient(135deg, #c3dafe, #a3bffa);
  color: #2c5282;
}

.type-other {
  background: linear-gradient(135deg, #d1fae5, #a7f3d0);
  color: #065f46;
}

/* 帖子内容 */
.post-content-section {
  margin-bottom: 32px;
}

.post-content {
  font-size: 1.1rem;
  line-height: 1.7;
  color: #374151;
  margin-bottom: 24px;
}

.post-images {
  margin-top: 20px;
}

/* 图片网格和项 */
.images-grid {
  display: grid;
  gap: 12px;
}

.grid-1 {
  grid-template-columns: 1fr;
}

.grid-2 {
  grid-template-columns: 1fr 1fr;
}

.grid-3 {
  grid-template-columns: 1fr 1fr;
  grid-auto-rows: 1fr;
}

.grid-4 {
  grid-template-columns: 1fr 1fr;
  grid-auto-rows: 1fr;
}

.image-item {
  position: relative;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: transform 0.2s ease;
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-item:hover {
  transform: scale(1.02);
}

.post-image {
  width: 100%;
  height: 200px;
  object-fit: cover;
  display: block;
}

/* 小屏幕时使图片高度自适应 */
@media (max-width: 480px) {
  .post-image { height: 160px; }
  .viewer-image { max-height: 70vh; }
}

.image-more {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
}

/* 帖子统计 */
.post-stats {
  display: flex;
  gap: 24px;
  padding: 20px;
  background: #f8fafc;
  border-radius: 12px;
  margin-bottom: 32px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #64748b;
}

.stat-icon {
  font-size: 1.2rem;
}

.stat-value {
  font-weight: 600;
  color: #1a202c;
}

.stat-label {
  font-size: 0.9rem;
}

/* 回复区域 */
.replies-section {
  border-top: 1px solid #f1f5f9;
  padding-top: 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-title {
  font-size: 1.3rem;
  font-weight: 600;
  color: #1a202c;
  margin: 0;
}

.section-count {
  color: #64748b;
  font-size: 0.9rem;
}

/* 错误状态 */
.modal-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 40px;
  text-align: center;
  gap: 16px;
}

.error-icon {
  font-size: 3rem;
}

.modal-error h3 {
  margin: 0;
  color: #1a202c;
  font-size: 1.3rem;
}

.modal-error p {
  margin: 0;
  color: #64748b;
}

.btn-retry {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  background: #0fa3a3;
  color: white;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s ease;
}

.btn-retry:hover {
  background: #0d9191;
}

/* 回复抽屉 */
.reply-drawer {
  position: fixed;
  inset: 0;
  z-index: 1500;
  pointer-events: none;
}

.reply-drawer.open {
  pointer-events: all;
}

.drawer-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.reply-drawer.open .drawer-overlay {
  opacity: 1;
}

.drawer-content {
  position: absolute;
  right: 0;
  top: 0;
  bottom: 0;
  width: 100%;
  max-width: 500px;
  background: white;
  transform: translateX(100%);
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
}

.reply-drawer.open .drawer-content {
  transform: translateX(0);
}

.drawer-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #f1f5f9;
}

.drawer-title {
  font-weight: 600;
  color: #1a202c;
}

.drawer-close {
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: background 0.2s ease;
}

.drawer-close:hover {
  background: #f1f5f9;
}

.drawer-body {
  flex: 1;
  overflow: auto;
}

/* 图片查看器 */
.image-viewer {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.95);
  z-index: 1600;
  display: flex;
  align-items: center;
  justify-content: center;
}

.viewer-close {
  position: absolute;
  top: 20px;
  right: 20px;
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: white;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  backdrop-filter: blur(10px);
}

.viewer-content {
  position: relative;
  max-width: 90vw;
  max-height: 90vh;
}

.viewer-image {
  max-width: 100%;
  max-height: 80vh;
  object-fit: contain;
  border-radius: 12px;
}

.viewer-nav {
  position: absolute;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 20px;
  background: rgba(0, 0, 0, 0.7);
  padding: 12px 20px;
  border-radius: 25px;
  backdrop-filter: blur(10px);
}

.nav-btn {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: background 0.2s ease;
}

.nav-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.1);
}

.nav-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.image-counter {
  color: white;
  font-weight: 500;
  min-width: 60px;
  text-align: center;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .modal-overlay {
    padding: 10px;
  }
  
  .post-detail-modal {
    max-height: 95vh;
    border-radius: 16px;
  }
  
  .post-detail-content {
    padding: 24px;
  }
  
  .post-title {
    font-size: 1.5rem;
  }
  
  .post-meta {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }
  
  .btn-reply {
    align-self: stretch;
    justify-content: center;
  }
  
  .images-grid {
    grid-template-columns: 1fr !important;
  }
  
  .drawer-content {
    max-width: 100%;
  }
}

@media (max-width: 480px) {
  .post-detail-content {
    padding: 20px;
  }
  
  .post-stats {
    flex-direction: column;
    gap: 12px;
  }
}
</style>