<template>
  <div class="forum-container">
    <header class="forum-header">
      <h1 class="forum-title">太虚坛</h1>
      <p class="forum-subtitle">太初寰宇 - 自由交流地</p>
    </header>

    <div class="forum-layout">
      <!-- 左侧：帖子列表 -->
      <main class="forum-main">
        <div class="posts-header">
          <h2 class="posts-title">📝 最新帖子</h2>
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

        <PostList
          :posts="posts"
          :loading="loading"
          :has-more="hasMore"
          @view-post="onViewPost"
          @load-more="loadMore"
        />
      </main>

      <!-- 右侧：发帖区域和热帖 -->
      <aside class="sidebar">
        <PostForm @posted="onPostPosted" />
        
        <!-- 热帖区域 -->
        <div class="hot-posts-section">
          <div class="hot-posts-header">
            <div class="hot-posts-title">
              <span class="fire-icon">🔥</span>
              <span>热门帖子</span>
            </div>
            <button 
              class="refresh-btn" 
              @click="refreshHotPosts" 
              :disabled="refreshingHot"
              title="刷新热帖"
            >
              <span class="refresh-icon" :class="{ refreshing: refreshingHot }">🔄</span>
            </button>
          </div>
          
          <div class="hot-posts-content">
            <div v-if="hotPostsLoading" class="loading-state">
              <div class="loading-spinner"></div>
              <span>加载中...</span>
            </div>
            
            <div v-else-if="hotPosts.length === 0" class="empty-state">
              <span class="empty-icon">📊</span>
              <span>暂无热帖数据</span>
            </div>
            
            <div v-else class="hot-posts-list">
              <div 
                v-for="(post, index) in hotPosts" 
                :key="post.id" 
                class="hot-post-item"
                :class="getRankClass(index)"
                @click="viewHotPost(post.id)"
              >
                <div class="post-rank">
                  <span class="rank-number">{{ index + 1 }}</span>
                </div>
                
                <div class="post-content">
                  <h4 class="post-title">{{ post.post_title || post.title || '无标题' }}</h4>
                  <div class="post-stats">
                    <span class="stat">
                      <span class="stat-icon">👁</span>
                      <span class="stat-value">{{ post.view_count || 0 }}</span>
                    </span>
                    <span class="stat">
                      <span class="stat-icon">💬</span>
                      <span class="stat-value">{{ post.comment_count || 0 }}</span>
                    </span>
                    <span class="post-time">{{ formatTime(post.createTime) }}</span>
                  </div>
                </div>
                
                <div class="post-hotness" :class="getHotnessClass(post)">
                  <span class="hotness-badge">热</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </aside>
    </div>

    <PostDetailModal
      v-if="showPostDetail"
      :post-id="currentPostId"
      :visible="showPostDetail"
      @close="closePostDetail"
      @reply-posted="onReplyPosted"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'
import PostList from '@/TXTComponents/PostList.vue'
import PostDetailModal from '@/TXTComponents/PostDetailModal.vue'
import PostForm from '@/TXTComponents/PostForm.vue'

const posts = ref([])
const loading = ref(false)
const hasMore = ref(true)
const lastId = ref(null)
const activeFilter = ref('all')
const showPostDetail = ref(false)
const currentPostId = ref(null)

// 热帖相关状态
const hotPosts = ref([])
const hotPostsLoading = ref(false)
const refreshingHot = ref(false)

const filters = ref([
  { id: 'all', name: '全部' },
  { id: '0', name: '柴圈帖子' },
  { id: '1', name: '游戏帖子' },
  { id: '2', name: '其他帖子' }
])

// 获取热帖
const fetchHotPosts = async () => {
  hotPostsLoading.value = true
  try {
    const response = await apiClient.get('/posts', {
      params: {
        pageSize: 5,
        sortBy: 'view_count',
        order: 'desc'
      }
    })
    
    const data = response.data
    let items = []
    
    if (Array.isArray(data)) {
      items = data.slice(0, 5)
    } else if (data?.success && Array.isArray(data.data)) {
      items = data.data.slice(0, 5)
    } else if (data?.data?.items) {
      items = data.data.items.slice(0, 5)
    }
    
    // 计算热度并排序
    items.forEach(post => {
      post.hotness = (post.view_count || 0) + (post.comment_count || 0) * 2
    })
    
    items.sort((a, b) => b.hotness - a.hotness)
    hotPosts.value = items.slice(0, 5)
  } catch (error) {
    console.error('获取热帖失败:', error)
    hotPosts.value = []
  } finally {
    hotPostsLoading.value = false
  }
}

// 刷新热帖
const refreshHotPosts = async () => {
  refreshingHot.value = true
  await fetchHotPosts()
  refreshingHot.value = false
}

// 获取排名样式
const getRankClass = (index) => {
  return `rank-${index + 1}`
}

// 获取热度等级
const getHotnessClass = (post) => {
  const hotness = post.hotness || 0
  if (hotness > 100) return 'hotness-high'
  if (hotness > 50) return 'hotness-medium'
  return 'hotness-low'
}

// 格式化时间
const formatTime = (time) => {
  if (!time) return ''
  const date = new Date(time)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const hours = Math.floor(diff / (1000 * 60 * 60))
  
  if (hours < 1) return '刚刚'
  if (hours < 24) return `${hours}小时前`
  return `${Math.floor(hours / 24)}天前`
}

// 查看热帖
const viewHotPost = (postId) => {
  currentPostId.value = postId
  showPostDetail.value = true
}

const fetchPosts = async () => {
  if (loading.value) return
  loading.value = true
  
  try {
    const params = { pageSize: 12 }
    if (lastId.value) params.lastId = lastId.value
    if (activeFilter.value !== 'all') params.postType = parseInt(activeFilter.value)

    const response = await apiClient.get('/posts', { params })
    const data = response.data

    let items = []
    let pagination = null

    if (Array.isArray(data)) {
      items = data
    } else if (data?.success && Array.isArray(data.data)) {
      items = data.data
      pagination = data.pagination
    } else if (data?.data?.items) {
      items = data.data.items
      pagination = data.data.pagination
    }

    if (params.lastId) {
      posts.value.push(...items)
    } else {
      posts.value = items
    }

    hasMore.value = !!(pagination?.hasMore || items.length >= params.pageSize)
    lastId.value = pagination?.lastId || (items.length ? items[items.length - 1]?.id : null)
  } catch (error) {
    console.error('获取帖子失败:', error)
  } finally {
    loading.value = false
  }
}

const loadMore = () => {
  if (!hasMore.value || loading.value) return
  fetchPosts()
}

const changeFilter = async (filterId) => {
  activeFilter.value = filterId
  lastId.value = null
  posts.value = []
  await fetchPosts()
}

const onPostPosted = async () => {
  lastId.value = null
  await Promise.all([fetchPosts(), fetchHotPosts()])
}

const onViewPost = (postId) => {
  currentPostId.value = postId
  showPostDetail.value = true
}

const closePostDetail = () => {
  showPostDetail.value = false
  currentPostId.value = null
}

const onReplyPosted = async () => {
  await Promise.all([fetchPosts(), fetchHotPosts()])
}

onMounted(() => {
  Promise.all([fetchPosts(), fetchHotPosts()])
})
</script>

<style scoped>
.forum-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: var(--space-lg);
  min-height: 100vh;
}

.forum-header {
  text-align: center;
  margin-bottom: 40px;
  padding: 40px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: var(--radius);
  color: white;
}

.forum-title {
  font-size: 2.5rem;
  font-weight: 700;
  margin: 0 0 8px 0;
}

.forum-subtitle {
  font-size: 1.1rem;
  opacity: 0.9;
  margin: 0;
}

.forum-layout {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: var(--space-lg);
  align-items: start;
}

/* 左侧主内容区 */
.forum-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.posts-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space);
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow);
}

.posts-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
}

.filter-controls {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 8px 16px;
  border-radius: 20px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.2s ease;
}

.filter-btn:hover {
  background: var(--bg-primary);
  border-color: var(--accent-primary);
}

.filter-btn.active {
  background: var(--accent-primary);
  color: white;
  border-color: var(--accent-primary);
}

/* 右侧边栏 */
.sidebar {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  position: sticky;
  top: var(--space-lg);
  height: fit-content;
}

/* 热帖区域 */
.hot-posts-section {
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.hot-posts-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space);
  background: linear-gradient(135deg, #fff5f5, #fed7d7);
  border-bottom: 1px solid var(--border-light);
}

.hot-posts-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 1.1rem;
  font-weight: 600;
  color: #e53e3e;
  margin: 0;
}

.fire-icon {
  font-size: 1.2rem;
}

.refresh-btn {
  background: rgba(229, 62, 62, 0.1);
  border: 1px solid rgba(229, 62, 62, 0.2);
  border-radius: 8px;
  padding: 6px 8px;
  color: #e53e3e;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.refresh-btn:hover:not(:disabled) {
  background: rgba(229, 62, 62, 0.15);
  transform: scale(1.05);
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.refresh-icon.refreshing {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.hot-posts-content {
  padding: var(--space);
}

.loading-state, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: var(--text-muted);
  gap: 8px;
}

.loading-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid var(--border-color);
  border-top: 2px solid var(--accent-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.empty-icon {
  font-size: 2rem;
  opacity: 0.5;
}

/* 热帖列表 */
.hot-posts-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.hot-post-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px;
  background: var(--bg-secondary);
  border-radius: 12px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

.hot-post-item::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: var(--border-color);
  transition: all 0.3s ease;
}

.hot-post-item:hover {
  background: var(--bg-primary);
  border-color: var(--border-color);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.hot-post-item:hover::before {
  width: 4px;
}

/* 排名样式 */
.post-rank {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: white;
}

.rank-1 .post-rank {
  background: linear-gradient(135deg, #ffd700, #ffb347);
  box-shadow: 0 4px 12px rgba(255, 193, 7, 0.3);
}

.rank-2 .post-rank {
  background: linear-gradient(135deg, #c0c0c0, #a8a8a8);
  box-shadow: 0 4px 12px rgba(156, 163, 175, 0.3);
}

.rank-3 .post-rank {
  background: linear-gradient(135deg, #cd7f32, #b5651d);
  box-shadow: 0 4px 12px rgba(180, 83, 9, 0.3);
}

.rank-4 .post-rank,
.rank-5 .post-rank {
  background: linear-gradient(135deg, #9ca3af, #6b7280);
  box-shadow: 0 2px 8px rgba(107, 114, 128, 0.2);
}

.rank-number {
  font-size: 0.9rem;
  font-weight: 700;
}

/* 帖子内容 */
.post-content {
  flex: 1;
  min-width: 0;
}

.post-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text-primary);
  line-height: 1.4;
  margin: 0 0 8px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.post-stats {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.8rem;
  color: var(--text-muted);
}

.stat {
  display: flex;
  align-items: center;
  gap: 4px;
}

.stat-icon {
  font-size: 0.7rem;
}

.stat-value {
  font-weight: 500;
}

.post-time {
  margin-left: auto;
  font-size: 0.75rem;
  opacity: 0.7;
}

/* 热度标签 */
.post-hotness {
  flex-shrink: 0;
}

.hotness-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: 600;
  color: white;
}

.hotness-high .hotness-badge {
  background: linear-gradient(135deg, #ff6b6b, #ee5a52);
}

.hotness-medium .hotness-badge {
  background: linear-gradient(135deg, #ffa726, #fb8c00);
}

.hotness-low .hotness-badge {
  background: linear-gradient(135deg, #66bb6a, #4caf50);
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .forum-layout {
    grid-template-columns: 1fr;
    gap: var(--space);
  }
  
  .sidebar {
    position: static;
    order: -1;
  }
}

@media (max-width: 768px) {
  .forum-container {
    padding: var(--space);
  }
  
  .forum-header {
    padding: 30px 16px;
  }
  
  .forum-title {
    font-size: 2rem;
  }
  
  .posts-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .filter-controls {
    justify-content: center;
  }
  
  .hot-post-item {
    padding: 12px;
    gap: 10px;
  }
  
  .post-title {
    font-size: 0.9rem;
  }
  
  .post-stats {
    gap: 8px;
  }
}

@media (max-width: 480px) {
  .forum-layout {
    gap: var(--space);
  }
  
  .hot-posts-header {
    padding: 12px;
  }
  
  .hot-posts-content {
    padding: 12px;
  }
  
  .hot-post-item {
    flex-direction: column;
    gap: 8px;
  }
  
  .post-rank {
    align-self: flex-start;
  }
  
  .post-stats {
    justify-content: space-between;
  }
  
  .post-time {
    margin-left: 0;
  }
}
</style>