<template>
  <div class="blog-section-wrapper">
    <div class="content-area">
      <div class="section-header">
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">技术博客</span>
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

      <div class="blog-list">
        <div v-if="isLoading" class="status-msg">正在检索数据索引...</div>
        
        <div v-else-if="blogList.length === 0" class="status-msg">
          该分区尚未发布任何技术文档
        </div>

        <div 
          v-for="blog in blogList" 
          :key="blog.id" 
          class="blog-card"
          @click="goToBlog(blog.id)"
        >
          <div class="date-box">
            <span class="day">{{ blog.day }}</span>
            <span class="month">{{ blog.month }}</span>
            <span class="year">{{ blog.year }}</span>
          </div>

          <div class="blog-content">
            <div class="blog-title">
              <span v-if="blog.tags && blog.tags.length" class="category-tag">
                {{ blog.tags[0] }}
              </span>
              {{ blog.title }}
            </div>
            <div class="blog-summary">
              {{ blog.summary }}
            </div>
            <div class="blog-meta">
              <span class="meta-item">阅读 {{ blog.viewCount }}</span>
              <span class="meta-divider">|</span>
              <span class="meta-item">评论 {{ blog.commentCount }}</span>
            </div>
          </div>

          <div class="blog-cover">
            <img v-if="blog.coverImage" :src="blog.coverImage" class="cover-img" alt="cover" />
            <div v-else class="cover-placeholder">NO DATA</div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">文章数</span>
        <span class="value">{{ blogList.length }}</span>
      </div>
      <div class="watermark-text">深度写作</div>
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

const blogList = ref([])
const isLoading = ref(false)
const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'clicks', label: '最多点击' },
  { key: 'likes', label: '最多收藏' }
]

const fetchBlogs = async () => {
  if (!props.userId || props.userId === '') return

  let finalId = props.userId;

  if (finalId === 'MEE') {
    const userData = JSON.parse(localStorage.getItem('user_info') || '{}');
    finalId = userData.id;
    if (!finalId) return;
  }

  isLoading.value = true
  try {
    const response = await apiClient.get(`/Blog/user/${finalId}`, {
      params: { 
        sortBy: currentFilter.value,
        limit: 5 
      }
    })
    
    if (response.data.success) {
      // 映射 PascalCase 字段到 camelCase
      blogList.value = response.data.data.map(item => ({
        id: item.Id,
        title: item.Title,
        summary: item.Summary,
        viewCount: item.ViewCount,
        commentCount: item.CommentCount,
        coverImage: item.coverImage,
        day: item.day,
        month: item.month,
        year: item.year,
        tags: item.tags || []
      }))
    }
  } catch (error) {
    console.error("博客数据同步失败:", error)
  } finally {
    isLoading.value = false
  }
}

watch(
  () => [props.userId, currentFilter.value],
  ([newId]) => {
    if (newId) {
      fetchBlogs()
    }
  },
  { immediate: true }
)

const goToBlog = (id) => {
  console.log('阅读文章 ID:', id)
}

const handleViewAll = () => {
  console.log('查看全部文章')
}
</script>

<style scoped>
.blog-section-wrapper {
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
}

.content-area {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-right: 20px;
}

/* --- 标题栏 --- */
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

.view-all-btn:hover { color: var(--accent-orange); }
.view-all-btn .arrow { transition: transform 0.3s; }
.view-all-btn:hover .arrow { transform: translateX(4px); }

/* --- 列表与卡片 --- */
.blog-list { display: flex; flex-direction: column; gap: 15px; }

.blog-card { 
  display: flex; 
  background: rgba(255, 255, 255, 0.4); 
  border: 1px solid #ddd; 
  border-radius: 4px; 
  padding: 15px; 
  transition: all 0.3s ease; 
  cursor: pointer; 
}

.blog-card:hover { 
  border-color: var(--accent-orange); 
  box-shadow: 0 4px 12px rgba(0,0,0,0.08); 
  transform: translateX(4px); 
}

.date-box { 
  display: flex; 
  flex-direction: column; 
  align-items: center; 
  justify-content: center; 
  padding-right: 20px; 
  border-right: 1px dashed #eee; 
  min-width: 60px; 
  color: var(--primary-blue); 
}

.day { font-size: 24px; font-weight: 900; line-height: 1; }
.month { font-size: 12px; color: var(--text-sub); margin-top: 4px; }
.year { font-size: 10px; color: #999; transform: scale(0.9); }

.blog-content { flex: 1; padding: 0 20px; display: flex; flex-direction: column; justify-content: space-between; }

.blog-title { 
  font-size: 16px; 
  font-weight: bold; 
  color: var(--text-main); 
  margin-bottom: 8px; 
  line-height: 1.4; 
  display: flex; 
  align-items: center; 
  gap: 8px; 
}

.category-tag { 
  background: rgba(230, 126, 34, 0.1); 
  color: var(--accent-orange); 
  font-size: 11px; 
  padding: 2px 6px; 
  border-radius: 2px; 
  font-weight: normal; 
}

.blog-summary { 
  font-size: 13px; 
  color: var(--text-sub); 
  line-height: 1.6; 
  margin-bottom: 10px; 
  display: -webkit-box; 
  -webkit-line-clamp: 2; 
  -webkit-box-orient: vertical; 
  overflow: hidden; 
}

.blog-meta { font-size: 12px; color: #999; }
.meta-divider { margin: 0 8px; color: #ddd; }

.blog-cover { 
  width: 120px; 
  height: 80px;
  background: rgba(249, 249, 249, 0.4); 
  border-radius: 2px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  margin-left: auto; 
  overflow: hidden;
}

.cover-img { width: 100%; height: 100%; object-fit: cover; }
.cover-placeholder { font-size: 10px; color: #ccc; font-weight: bold; }

.status-msg {
  text-align: center;
  padding: 30px;
  color: var(--text-sub);
  font-size: 13px;
  border: 1px dashed #ddd;
}

/* --- 装饰条 --- */
.decoration-sidebar { width: 40px; flex: 0 0 40px; border-left: 1px solid rgba(0,0,0,0.1); display: flex; flex-direction: column; align-items: center; justify-content: space-between; padding-top: 5px; position: relative; }
.deco-status-box { width: 100%; padding: 6px 0; text-align: center; border-right: 3px solid var(--accent-orange); }
.deco-status-box .label { font-size: 10px; color: var(--text-sub); font-weight: bold; }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }
.watermark-text { writing-mode: vertical-rl; font-size: 24px; font-weight: 900; color: rgba(0, 0, 0, 0.05); letter-spacing: 4px; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); }
.stripe-bar { width: 6px; height: 100px; opacity: 0.6; margin-bottom: 10px; background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px); }
</style>