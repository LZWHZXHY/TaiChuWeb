<template>
  <div class="video-recommendations-container">
    <!-- 页面头部 -->
    <header class="page-header">
      <h1>🎬 火柴人视频推荐</h1>
      <p>发现精彩的火柴人动画作品</p>
      <div class="header-stats">
        <div class="stat-item">
          <span class="stat-number">{{ videos.length }}</span>
          <span class="stat-label">推荐作品</span>
        </div>
        <div class="stat-item">
          <span class="stat-number">{{ featuredCount }}</span>
          <span class="stat-label">特别推荐</span>
        </div>
      </div>
    </header>

    <!-- 控制栏 -->
    <div class="controls-bar">
      <div class="search-box">
        <input 
          v-model="query" 
          type="text" 
          placeholder="搜索视频标题、作者或描述..." 
          class="search-input"
          @input="handleSearch"
        />
        <span class="search-icon">🔍</span>
      </div>
      
      <div class="filter-tabs">
        <button 
          v-for="tab in tabs" 
          :key="tab.id"
          :class="['tab-button', { active: activeTab === tab.id }]"
          @click="switchTab(tab.id)"
        >
          <span class="tab-icon">{{ tab.icon }}</span>
          <span class="tab-label">{{ tab.label }}</span>
        </button>
      </div>
      
      <div class="action-buttons">
        <button @click="refreshData" class="btn-refresh" :disabled="loading">
          <span class="refresh-icon" :class="{ spinning: loading }">🔄</span>
          刷新
        </button>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>加载中...</p>
    </div>

    <!-- 错误提示 -->
    <div v-else-if="error" class="error-message">
      <div class="error-icon">⚠️</div>
      <h3>API连接失败</h3>
      <p>{{ error }}</p>
      <div class="error-actions">
        <button @click="refreshData" class="btn-retry">🔄 重试</button>
      </div>
    </div>

    <!-- 视频内容 -->
    <div v-else class="videos-content">
      <!-- 空状态 -->
      <div v-if="videos.length === 0" class="empty-state">
        <div class="empty-icon">📹</div>
        <h3>暂无视频内容</h3>
        <p>当前没有可显示的视频</p>
      </div>

      <!-- 视频网格 -->
      <div v-else class="videos-grid">
        <div 
          v-for="video in filteredVideos" 
          :key="video.ID"
          :ref="el => observeVideoCard(el, video.ID)"
          class="video-card"
          :class="{ 
            'featured': isFeatured(video),
            'bilibili': isBilibiliVideo(video.video_url)
          }"
        >
          <!-- 视频封面 -->
          <div class="video-thumbnail" @click="openVideoInNewTab(video)">
            <div class="thumbnail-image">
              <!-- 优先使用后端提供的封面或已缓存的 _thumbnail -->
              <img
                v-if="getThumbnailUrl(video) && !video._thumbnailFailed"
                :src="getThumbnailUrl(video)"
                :alt="video.title || '视频封面'"
                class="thumbnail-img"
                loading="lazy"
                referrerpolicy="no-referrer"
                @load="onThumbnailLoad($event, video)"
                @error="onThumbnailError($event, video)"
              />

              <!-- 当没有可用缩略图时显示平台图标作为占位 -->
              <div v-else class="platform-icon">
                <span v-if="isBilibiliVideo(video.video_url)" class="bilibili-icon">📺</span>
                <span v-else class="video-icon">🎬</span>
              </div>

              <div class="play-overlay">
                <div class="play-button">
                  <span class="play-icon">▶</span>
                  <span class="play-text">点击观看</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 视频信息 -->
          <div class="video-info">
            <div class="video-header">
              <h3 class="video-title">{{ video.title }}</h3>
              <div class="video-badges">
                <span v-if="video.Bestyear" class="badge badge-yearly">🏆 {{ video.Bestyear }}年度最佳</span>
                <span v-if="video.monthRecommend" class="badge badge-monthly">⭐ {{ formatMonth(video.monthRecommend) }}</span>
                <span class="badge badge-category">{{ getCategoryName(video.category_id) }}</span>
                <span v-if="isBilibiliVideo(video.video_url)" class="badge badge-bilibili">B站</span>
              </div>
            </div>

            <div class="video-description">
              <p>{{ truncateDescription(video.description) }}</p>
            </div>
            
            <div class="video-meta">
              <span class="meta-item">
                <span class="meta-icon">👤</span>
                {{ video.author }}
              </span>
              <span class="meta-item">
                <span class="meta-icon">📁</span>
                {{ getCategoryName(video.category_id) }}
              </span>
              <span class="meta-item">
                <span class="meta-icon">🔗</span>
                {{ getVideoType(video.video_url) }}
              </span>
            </div>

            <div class="video-url">
              <span class="url-label">视频链接:</span>
              <a 
                :href="video.video_url" 
                target="_blank" 
                class="video-link"
                :title="video.video_url"
                @click.stop
              >
                {{ truncateUrl(video.video_url) }}
              </a>
            </div>
          </div>

          <!-- 视频操作 -->
          <div class="video-actions">
            <button class="btn-watch" @click="openVideoInNewTab(video)">
              ▶️ 观看视频
            </button>
            <button class="btn-copy" @click="copyVideoUrl(video)">
              📋 复制链接
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import apiClient from '@/utils/api'

// 响应式数据
const videos = ref([])
const loading = ref(false)
const error = ref('')
const query = ref('')
const activeTab = ref('all')
const observer = ref(null)
const observedVideos = ref(new Set())

// 分类映射
const CATEGORIES = {
  1: { name: '接力类', icon: '🏃' },
  2: { name: '联合类', icon: '👥' },
  3: { name: '世界观企划类', icon: '🌍' }
}

// 选项卡配置
const tabs = [
  { id: 'all', label: '全部视频', icon: '📺' },
  { id: 'featured', label: '特别推荐', icon: '⭐' },
  { id: 'yearly', label: '年度最佳', icon: '🏆' },
  { id: 'monthly', label: '月度推荐', icon: '📅' }
]

// 计算属性
const featuredCount = computed(() => 
  videos.value.filter(v => v.Bestyear || v.monthRecommend).length
)

const filteredVideos = computed(() => {
  let filtered = videos.value
  
  // 搜索过滤
  if (query.value.trim()) {
    const searchTerm = query.value.trim().toLowerCase()
    filtered = filtered.filter(video => 
      (video.title || '').toLowerCase().includes(searchTerm) ||
      (video.description || '').toLowerCase().includes(searchTerm) ||
      (video.author || '').toLowerCase().includes(searchTerm)
    )
  }
  
  // 选项卡过滤
  if (activeTab.value !== 'all') {
    switch (activeTab.value) {
      case 'featured':
        filtered = filtered.filter(v => v.Bestyear || v.monthRecommend)
        break
      case 'yearly':
        filtered = filtered.filter(v => v.Bestyear)
        break
      case 'monthly':
        filtered = filtered.filter(v => v.monthRecommend)
        break
    }
  }
  
  return filtered
})

// 工具函数
const getCategoryName = (categoryId) => {
  const category = CATEGORIES[categoryId]
  return category ? `${category.icon} ${category.name}` : '未知分类'
}

const isFeatured = (video) => video.Bestyear || video.monthRecommend

const formatMonth = (monthCode) => {
  if (!monthCode) return ''
  const month = monthCode % 100
  const monthNames = {
    1: '一月', 2: '二月', 3: '三月', 4: '四月',
    5: '五月', 6: '六月', 7: '七月', 8: '八月',
    9: '九月', 10: '十月', 11: '十一月', 12: '十二月'
  }
  return monthNames[month] || `${month}月`
}

const truncateDescription = (description, maxLength = 100) => {
  if (!description) return ''
  if (description.length <= maxLength) return description
  return description.substring(0, maxLength) + '...'
}

const truncateUrl = (url, maxLength = 30) => {
  if (!url) return ''
  if (url.length <= maxLength) return url
  return url.substring(0, maxLength) + '...'
}

// 视频类型检测
const isBilibiliVideo = (url) => {
  if (!url) return false
  return url.includes('bilibili.com') || url.includes('b23.tv')
}

const getVideoType = (url) => {
  if (isBilibiliVideo(url)) return 'B站视频'
  if (url.match(/\.(mp4|webm|ogg|mov|avi|m3u8)$/i)) return '直链视频'
  if (/youtube\.com|youtu\.be/.test(url)) return 'YouTube 视频'
  return '外部链接'
}

// 视频操作
const openVideoInNewTab = (video) => {
  window.open(video.video_url, '_blank', 'noopener,noreferrer')
}

const copyVideoUrl = async (video) => {
  try {
    await navigator.clipboard.writeText(video.video_url)
    console.log('✅ 视频链接已复制到剪贴板')
  } catch (err) {
    console.error('复制失败:', err)
    // 降级方案
    const textArea = document.createElement('textarea')
    textArea.value = video.video_url
    document.body.appendChild(textArea)
    textArea.select()
    document.execCommand('copy')
    document.body.removeChild(textArea)
    console.log('✅ 视频链接已复制（降级方案）')
  }
}

// ---- 图片优化相关 ----

// 验证是否为图片 URL
const isImageUrl = (url) => {
  if (!url || typeof url !== 'string') return false
  url = url.trim()
  if (!url) return false
  if (url.startsWith('data:') || url.startsWith('blob:')) return true
  if (url.startsWith('/api/BiliBili/cover')) return true
  if (/\.(jpe?g|png|gif|webp|avif|bmp|svg)(?:\?.*)?$/i.test(url)) return true
  if (/i\d?\.hdslb\.com/i.test(url) || /bilivideo\.com/i.test(url)) return true
  return false
}

// 返回缩略图 URL
const getThumbnailUrl = (video) => {
  if (!video) return ''
  
  const candidate = video._thumbnail || video.cover || video.thumbnail || ''
  if (!candidate) return ''
  
  if (!isImageUrl(candidate)) return ''
  
  // 对于B站图片，使用优化的代理策略
  if (isBilibiliCdnUrl(candidate)) {
    return getOptimizedBilibiliThumbnailUrl(candidate, video)
  }
  
  return candidate.trim()
}

// 判断是否是B站CDN图片
const isBilibiliCdnUrl = (url) => {
  return /(hdslb\.com|bilivideo\.com)/i.test(url)
}

// 优化的B站缩略图URL生成
const getOptimizedBilibiliThumbnailUrl = (originalUrl, video) => {
  const optimizedUrl = optimizeImageSize(originalUrl)
  
  // 对于已经成功加载过的图片，直接返回原URL（避免重复代理）
  if (video._thumbnailLoaded) {
    return optimizedUrl
  }
  
  // 首次加载使用代理（避免热链问题）
  const BACKEND_BASE = import.meta.env.VITE_API_BASE || 'https://localhost:44359'
  return `${BACKEND_BASE.replace(/\/$/, '')}/api/BiliBili/cover?url=${encodeURIComponent(optimizedUrl)}&w=400&h=250&q=80`
}

// 图片尺寸优化
const optimizeImageSize = (url) => {
  if (!url) return url
  
  // B站图片尺寸优化
  if (url.includes('hdslb.com')) {
    return url.replace(/@[^/]*\.(jpg|png|webp)$/, '@400w_250h_1c.jpg')
  }
  
  // YouTube 图片使用中等质量
  if (url.includes('ytimg.com')) {
    return url.replace('/hqdefault.jpg', '/mqdefault.jpg')
  }
  
  return url
}

// 图片加载成功处理
const onThumbnailLoad = (event, video) => {
  if (video) {
    video._thumbnailLoaded = true
    video._thumbnailFailed = false
    video._thumbnailRetryCount = 0
  }
}

// 图片加载错误处理
const onThumbnailError = async (event, video) => {
  if (!video) return
  
  const maxRetries = 2
  const currentRetry = video._thumbnailRetryCount || 0
  
  if (currentRetry < maxRetries) {
    // 重试逻辑
    video._thumbnailRetryCount = currentRetry + 1
    console.log(`缩略图加载失败，重试 ${video._thumbnailRetryCount}/${maxRetries}`, video.video_url)
    
    // 延迟重试
    setTimeout(() => {
      fetchThumbnailForVideo(video)
    }, 1000 * currentRetry)
  } else {
    // 最终失败处理
    video._thumbnailFailed = true
    video._thumbnail = ''
    console.warn('缩略图最终加载失败', video.video_url)
  }
}

// 获取视频缩略图
const fetchThumbnailForVideo = async (video) => {
  if (!video || video._thumbnailLoading || getThumbnailUrl(video) || video._thumbnailFailed) return
  video._thumbnailLoading = true

  try {
    const url = (video.video_url || '').trim()
    if (!url) {
      video._thumbnailFailed = true
      return
    }

    // 1) YouTube 直接构造缩略图
    const ytId = extractYouTubeId(url)
    if (ytId) {
      const pic = `https://img.youtube.com/vi/${ytId}/mqdefault.jpg`
      Object.assign(video, { _thumbnail: pic, _thumbnailFailed: false })
      return
    }

    // 2) 请求后端 info（返回 data.pic）
    const resp = await apiClient.get('/BiliBili', { params: { url } })
    const data = resp?.data

    if (data?.success && data.pic) {
      let originalPic = (data.pic || '').trim()
      if (!originalPic) {
        Object.assign(video, { _thumbnail: '', _thumbnailFailed: true })
        return
      }

      const BACKEND_BASE = import.meta.env.VITE_API_BASE || 'https://localhost:44359'
      const proxied = `${BACKEND_BASE.replace(/\/$/, '')}/api/BiliBili/cover?url=${encodeURIComponent(originalPic)}&w=400&h=250&q=80`

      Object.assign(video, { _thumbnail: proxied, _thumbnailFailed: false })
    } else {
      Object.assign(video, { _thumbnail: '', _thumbnailFailed: true })
    }
  } catch (err) {
    console.warn('获取缩略图失败:', err)
    Object.assign(video, { _thumbnail: '', _thumbnailFailed: true })
  } finally {
    video._thumbnailLoading = false
  }
}

// YouTube ID 解析
const extractYouTubeId = (url) => {
  if (!url) return null
  const ytMatch = url.match(/(?:v=|\/)([0-9A-Za-z_-]{11})(?:[&?]|$)/)
  if (ytMatch) return ytMatch[1]
  const shortMatch = url.match(/youtu\.be\/([0-9A-Za-z_-]{11})/)
  if (shortMatch) return shortMatch[1]
  return null
}

// ---- 懒加载优化 ----

// 初始化 Intersection Observer
const initObserver = () => {
  if (typeof window !== 'undefined' && 'IntersectionObserver' in window) {
    observer.value = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          const videoId = entry.target.dataset.videoId
          if (videoId && !observedVideos.value.has(videoId)) {
            observedVideos.value.add(videoId)
            loadThumbnailForVisibleVideo(videoId)
          }
        }
      })
    }, {
      rootMargin: '50px 0px',
      threshold: 0.1
    })
  }
}

// 为可见的视频加载缩略图
const loadThumbnailForVisibleVideo = (videoId) => {
  const video = videos.value.find(v => v.ID === videoId)
  if (video && !getThumbnailUrl(video) && !video._thumbnailFailed) {
    fetchThumbnailForVideo(video)
  }
}

// 观察视频卡片
const observeVideoCard = (element, videoId) => {
  if (observer.value && element) {
    element.dataset.videoId = videoId
    observer.value.observe(element)
  }
}

// 清理观察器
const cleanupObserver = () => {
  if (observer.value) {
    observer.value.disconnect()
    observedVideos.value.clear()
  }
}

// 页面操作
const switchTab = (tabId) => {
  activeTab.value = tabId
}

const refreshData = () => {
  loadVideos()
}

const handleSearch = () => {
  // 搜索逻辑在计算属性中自动处理
}

// 从API加载数据
const loadVideos = async () => {
  loading.value = true
  error.value = ''
  
  try {
    console.log('🌐 从API加载数据...')
    
    const response = await apiClient.get('/ChaiVideoRecommend')
    
    if (response.data && response.data.success) {
      videos.value = response.data.data || []
      console.log('videos loaded:', videos.value)

      // 初始只加载前3个可见项目的缩略图
      setTimeout(() => {
        videos.value.slice(0, 3).forEach(video => {
          if (!getThumbnailUrl(video)) {
            fetchThumbnailForVideo(video)
          }
        })
      }, 100)

      console.log('✅ API数据加载成功:', videos.value.length, '条')
    } else {
      throw new Error('API返回数据格式不正确')
    }
    
  } catch (err) {
    console.error('❌ API加载失败:', err)
    error.value = err.response?.data?.message || err.message || '数据加载失败'
  } finally {
    loading.value = false
  }
}

// 初始化
onMounted(() => {
  initObserver()
  loadVideos()
})

onUnmounted(() => {
  cleanupObserver()
})
</script>

<style scoped>
/* 样式保持不变，与之前相同 */
.video-recommendations-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
  padding: 30px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 16px;
}

.page-header h1 {
  margin: 0 0 10px 0;
  font-size: 2.5rem;
  font-weight: 700;
}

.page-header p {
  font-size: 1.1rem;
  opacity: 0.9;
  margin-bottom: 20px;
}

.header-stats {
  display: flex;
  justify-content: center;
  gap: 40px;
  margin-top: 20px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-number {
  font-size: 2.2rem;
  font-weight: 700;
  line-height: 1;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.9;
  margin-top: 5px;
}

.controls-bar {
  display: flex;
  gap: 20px;
  margin-bottom: 30px;
  padding: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  align-items: center;
  flex-wrap: wrap;
}

.search-box {
  position: relative;
  flex: 1;
  min-width: 250px;
}

.search-input {
  width: 100%;
  padding: 12px 45px 12px 15px;
  border: 2px solid #e9ecef;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: #007bff;
}

.search-icon {
  position: absolute;
  right: 15px;
  top: 50%;
  transform: translateY(-50%);
  color: #6c757d;
  font-size: 1.1rem;
}

.filter-tabs {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.tab-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: 2px solid #e9ecef;
  background: white;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s;
  font-weight: 500;
}

.tab-button.active {
  background: #007bff;
  color: white;
  border-color: #007bff;
}

.tab-button:hover:not(.active) {
  background: #f8f9fa;
  border-color: #007bff;
}

.action-buttons {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-refresh {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: 2px solid #e9ecef;
  background: white;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s;
  font-weight: 500;
}

.btn-refresh:hover {
  background: #f8f9fa;
  border-color: #007bff;
}

.btn-refresh:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.refresh-icon.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-state {
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  margin: 20px 0;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

.loading-state p {
  color: #666;
  font-size: 1.1rem;
}

.error-message {
  text-align: center;
  padding: 40px;
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  border-radius: 12px;
  margin: 20px 0;
}

.error-icon {
  font-size: 3rem;
  margin-bottom: 20px;
}

.error-message h3 {
  color: #856404;
  margin-bottom: 10px;
}

.error-message p {
  color: #856404;
  margin-bottom: 20px;
}

.error-actions {
  display: flex;
  gap: 15px;
  justify-content: center;
  margin-top: 20px;
}

.btn-retry {
  padding: 10px 20px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s;
}

.btn-retry:hover {
  background: #0056b3;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  margin: 20px 0;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.empty-state h3 {
  color: #666;
  margin-bottom: 10px;
}

.empty-state p {
  color: #888;
  margin-bottom: 20px;
}

.videos-content {
  margin-top: 20px;
}

.videos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.video-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.video-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15);
}

.video-card.featured {
  border-color: #ffd700;
  box-shadow: 0 4px 20px rgba(255,215,0,0.3);
}

.video-card.bilibili {
  border-color: #00a1d6;
  box-shadow: 0 4px 20px rgba(0,161,214,0.3);
}

.video-thumbnail {
  position: relative;
  height: 180px;
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  overflow: hidden;
  cursor: pointer;
}

.thumbnail-image {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}

.thumbnail-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.platform-icon {
  font-size: 3rem;
  opacity: 0.7;
}

.bilibili-icon {
  color: #00a1d6;
}

.video-icon {
  color: #fff;
}

.play-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.video-thumbnail:hover .play-overlay {
  opacity: 1;
}

.play-button {
  text-align: center;
  color: white;
}

.play-icon {
  font-size: 2rem;
  display: block;
  margin-bottom: 5px;
}

.play-text {
  font-size: 0.9rem;
  font-weight: 600;
}

.video-info {
  padding: 20px;
}

.video-header {
  margin-bottom: 15px;
}

.video-title {
  margin: 0 0 12px 0;
  font-size: 1.2rem;
  font-weight: 600;
  color: #333;
  line-height: 1.4;
}

.video-badges {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
}

.badge-yearly {
  background: #ffd700;
  color: #000;
}

.badge-monthly {
  background: #4CAF50;
  color: white;
}

.badge-bilibili {
  background: #00a1d6;
  color: white;
}

.badge-category {
  background: #6c757d;
  color: white;
}

.video-description {
  margin-bottom: 15px;
}

.video-description p {
  margin: 0;
  line-height: 1.5;
  color: #666;
  font-size: 0.9rem;
}

.video-meta {
  display: flex;
  gap: 15px;
  margin-bottom: 15px;
  flex-wrap: wrap;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 5px;
  color: #666;
  font-size: 0.9rem;
}

.meta-icon {
  font-size: 1rem;
}

.video-url {
  margin-bottom: 15px;
}

.url-label {
  font-weight: 600;
  color: #333;
  margin-right: 8px;
}

.video-link {
  color: #007bff;
  text-decoration: none;
  font-size: 0.9rem;
  word-break: break-all;
}

.video-link:hover {
  text-decoration: underline;
}

.video-actions {
  padding: 15px 20px;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
  display: flex;
  gap: 10px;
}

.video-actions button {
  padding: 8px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.3s;
  flex: 1;
}

.btn-watch {
  background: #007bff;
  color: white;
}

.btn-watch:hover {
  background: #0056b3;
}

.btn-copy {
  background: #6c757d;
  color: white;
}

.btn-copy:hover {
  background: #545b62;
}

@media (max-width: 768px) {
  .videos-grid {
    grid-template-columns: 1fr;
  }
  
  .controls-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filter-tabs {
    justify-content: center;
  }
  
  .action-buttons {
    justify-content: center;
  }
  
  .video-actions {
    flex-direction: column;
  }
  
  .header-stats {
    flex-direction: column;
    gap: 20px;
  }
}
</style>