<template>
  <div class="post-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">动态发布</span>
        </div>

        <div class="filter-group">
          <button 
            v-for="filter in filters" 
            :key="filter.key"
            class="filter-btn"
            :class="{ active: currentFilter === filter.key }"
            @click="currentFilter = filter.key"
          >
            {{ filter.label }}
          </button>
        </div>

        <div class="header-right-line"></div>

        <div class="view-all-btn" @click="handleViewAll">
          <span>查看全部</span>
          <span class="arrow">>></span>
        </div>
      </div>

      <div class="post-grid">
        <div v-if="isLoading" class="status-msg">正在同步实时数据流...</div>
        <div v-else-if="postList.length === 0" class="status-msg">中枢数据库未发现相关动态记录</div>

        <div v-for="post in postList" :key="post.id" class="post-card">
          <div class="post-header">
            <img v-if="post.authorAvatar" :src="post.authorAvatar" class="avatar-img" alt="avatar">
            <div v-else class="avatar-circle">{{ post.authorName ? post.authorName.charAt(0).toUpperCase() : '?' }}</div>
            
            <div class="user-info">
              <div class="username">{{ post.authorName }}</div>
              <div class="time-ago">{{ formatRelativeTime(post.createTime) }} · 终端发布</div>
            </div>
            <div class="more-options">...</div>
          </div>

          <div class="post-content">
            <span v-if="post.post_title" class="post-title-tag">【{{ post.post_title }}】</span>
            {{ post.content }}
            <div class="post-tags" v-if="post.tags && post.tags.length">
              <span v-for="tag in post.tags" :key="tag" class="tag-item">#{{ tag }}</span>
            </div>
          </div>
          
          <div class="post-images" v-if="post.images && post.images.length">
            <div 
              v-for="(img, idx) in post.images.slice(0, 3)" 
              :key="idx" 
              class="post-image-item"
            >
              <img :src="img" alt="动态图片" loading="lazy">
              <div v-if="idx === 2 && post.images.length > 3" class="image-count-overlay">
                +{{ post.images.length - 3 }}
              </div>
            </div>
          </div>
          
          <div class="post-actions">
            <div class="action-btn">
              <span class="icon">➦</span> 分享
            </div>
            <div class="action-btn">
              <span class="icon">💬</span> {{ post.comment_count }}
            </div>
            <div class="action-btn">
              <span class="icon">♥</span> {{ post.like_count }}
            </div>
            <div class="post-source">
              <span class="icon">👁</span> {{ post.view_count }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">记录数</span>
        <span class="value">{{ postList.length }}</span>
      </div>
      <div class="watermark-text">实时动态</div>
      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  userId: {
    type: [Number, String],
    default: null
  }
})

const postList = ref([])
const isLoading = ref(false)
const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'clicks', label: '最多点击' },
  { key: 'likes', label: '最多收藏' }
]

/**
 * 核心请求方法
 */
const fetchPosts = async () => {
  if (!props.userId || props.userId === '') return

  let finalId = props.userId;

  // 身份识别转换 (处理 MEE 模式)
  if (finalId === 'MEE') {
    const userData = JSON.parse(localStorage.getItem('user_info') || '{}');
    finalId = userData.id;
    if (!finalId) return;
  }

  isLoading.value = true
  try {
    const response = await apiClient.get(`/ThePost/user/${finalId}`, {
      params: { 
        sortBy: currentFilter.value,
        limit: 5 // 首页展示 5 条动态预览
      }
    })
    
    if (response.data.success) {
      // 这里的字段名需与后端 C# Controller 返回的对象属性严格一致
      postList.value = response.data.data
    }
  } catch (error) {
    console.error("动态同步失败:", error)
  } finally {
    isLoading.value = false
  }
}

/**
 * 相对时间转换助手
 */
const formatRelativeTime = (dateStr) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diff = (now - date) / 1000 // 秒

  if (diff < 60) return '刚刚'
  if (diff < 3600) return Math.floor(diff / 60) + '分钟前'
  if (diff < 86400) return Math.floor(diff / 3600) + '小时前'
  if (diff < 2592000) return Math.floor(diff / 86400) + '天前'
  return date.toLocaleDateString()
}

// 监听 ID 和 筛选器
watch(
  () => [props.userId, currentFilter.value],
  ([newId]) => {
    if (newId) fetchPosts()
  },
  { immediate: true }
)

const handleViewAll = () => {
  console.log('跳转至动态广场页面')
}
</script>

<style scoped>
/* 样式保持原有的工业风格，并补充新增逻辑的样式 */
.post-section-wrapper {
  --primary-blue: #2c3e50;
  --accent-orange: #e67e22;
  --text-main: #333333;
  --text-sub: #666666;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  display: flex;
  width: 100%;
  position: relative;
  background-image: radial-gradient(#ddd 1px, transparent 1px);
  background-size: 20px 20px;
  padding: 20px 0;
  color: var(--text-main);
  border-top: 1px dashed rgba(0,0,0,0.1); 
}

.content-area {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-right: 20px;
}

.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding-left: 5px;
  height: 30px;
}
.header-left { display: flex; align-items: center; gap: 8px; margin-right: 25px; }
.deco-block { width: 6px; height: 22px; background: var(--accent-orange); margin-right: 4px; }
.header-main { font-size: 20px; font-weight: 900; color: var(--primary-blue); }

.filter-group {
  display: flex;
  gap: 12px;
  align-items: center;
}

.filter-btn {
  background: transparent;
  border: none;
  font-size: 13px;
  color: var(--text-sub);
  cursor: pointer;
  padding: 4px 8px;
  transition: all 0.3s;
  font-weight: bold;
}

.filter-btn.active {
  color: var(--accent-orange);
  border-bottom: 2px solid var(--accent-orange);
}

.header-right-line {
  flex: 1; height: 1px; margin: 0 20px; opacity: 0.3;
  background: repeating-linear-gradient(90deg, var(--primary-blue), var(--primary-blue) 4px, transparent 4px, transparent 8px);
}

.view-all-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--text-sub);
  cursor: pointer;
}

.post-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 15px;
}

.post-card {
  background: rgba(255, 255, 255, 0.4);
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 15px;
  display: flex;
  flex-direction: column;
  transition: all 0.2s;
}

.post-card:hover {
  border-color: var(--accent-orange);
  transform: translateY(-2px);
}

.post-header { display: flex; align-items: center; margin-bottom: 10px; }

.avatar-img {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  margin-right: 10px;
  object-fit: cover;
  border: 1px solid #eee;
}

.avatar-circle { 
  width: 32px; height: 32px; background: var(--primary-blue); color: #fff; 
  border-radius: 50%; display: flex; align-items: center; justify-content: center; 
  font-weight: bold; margin-right: 10px; font-size: 14px; 
}

.user-info { flex: 1; display: flex; flex-direction: column; }
.username { font-size: 13px; font-weight: bold; color: var(--text-main); }
.time-ago { font-size: 10px; color: #999; }

.post-content { font-size: 13px; line-height: 1.6; color: var(--text-sub); margin-bottom: 12px; text-align: justify; }
.post-title-tag { color: var(--primary-blue); font-weight: bold; }

.post-tags { margin-top: 8px; display: flex; gap: 8px; flex-wrap: wrap; }
.tag-item { color: var(--accent-orange); font-size: 12px; font-weight: bold; }

.post-images {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
  margin-bottom: 12px;
}

.post-image-item {
  position: relative;
  aspect-ratio: 1 / 1;
  border-radius: 4px;
  overflow: hidden;
  background-color: #f0f0f0;
}

.post-image-item img { width: 100%; height: 100%; object-fit: cover; }

.image-count-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0, 0, 0, 0.5); color: #fff; display: flex;
  align-items: center; justify-content: center; font-size: 18px; font-weight: bold;
}

.post-actions {
  display: flex;
  align-items: center;
  border-top: 1px solid #f5f5f5;
  padding-top: 10px;
  margin-top: auto;
}

.action-btn {
  font-size: 12px; color: #999; cursor: pointer;
  display: flex; align-items: center; gap: 4px; margin-right: 20px;
}

.post-source { margin-left: auto; font-size: 12px; color: #999; }

.status-msg {
  text-align: center; padding: 40px; color: var(--text-sub);
  font-size: 13px; border: 1px dashed #ccc;
}

.decoration-sidebar { width: 40px; flex: 0 0 40px; border-left: 1px solid rgba(0,0,0,0.1); display: flex; flex-direction: column; align-items: center; justify-content: space-between; padding-top: 5px; position: relative; }
.deco-status-box { width: 100%; text-align: center; border-right: 3px solid var(--accent-orange); }
.deco-status-box .label { font-size: 10px; color: var(--text-sub); }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }
.watermark-text { writing-mode: vertical-rl; font-size: 24px; font-weight: 900; color: rgba(0, 0, 0, 0.05); letter-spacing: 4px; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); }
.stripe-bar { width: 6px; height: 100px; opacity: 0.6; margin-bottom: 10px; background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px); }
</style>