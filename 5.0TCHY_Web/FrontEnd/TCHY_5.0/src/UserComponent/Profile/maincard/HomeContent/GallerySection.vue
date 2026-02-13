<template>
  <div class="gallery-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">个人画廊</span>
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

      <div class="gallery-grid">
        <div v-if="!userId" class="status-msg">正在链接身份识别系统...</div>
        <div v-else-if="isLoading" class="status-msg">正在接入神经网络视图...</div>
        
        <div v-else-if="galleryList.length === 0" class="status-msg">
          此分区未检测到有效数据记录
        </div>

        <div 
          v-for="item in galleryList" 
          :key="item.id" 
          class="gallery-card"
          @click="goToDetail(item.id)"
        >
          <div class="thumb-container">
            <img 
              v-if="item.url" 
              :src="item.url" 
              class="thumb-img" 
              loading="lazy"
            />
            <div v-else class="thumb-placeholder">
              <span>IMAGE_ERR_{{ item.id }}</span>
            </div>
          </div>
          <div class="info-container">
            <div class="item-title" :title="item.title">
              {{ item.title }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">记录数</span>
        <span class="value">{{ galleryList.length }}</span>
      </div>
      <div class="watermark-text">DATA VIEW</div>
      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router' // 1. 引入 useRouter
import apiClient from '@/utils/api'

const props = defineProps({
  userId: {
    type: [Number, String],
    default: null
  }
})

const router = useRouter() // 2. 初始化 router
const galleryList = ref([])
const isLoading = ref(false)
const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'views', label: '最多点击' },
  { key: 'likes', label: '最多收藏' }
]

/**
 * 核心请求方法
 */
const fetchGallery = async () => {
  if (!props.userId || props.userId === '') {
    return
  }

  // 身份识别转换 (处理 MEE 模式)
  let finalId = props.userId;
  if (finalId === 'MEE') {
    const userData = JSON.parse(localStorage.getItem('user_info') || '{}');
    finalId = userData.id;
    if (!finalId) return;
  }

  isLoading.value = true
  console.log("Gallery: 开始同步数据，当前 ID ->", finalId)

  try {
    const response = await apiClient.get(`/Drawing/user/${finalId}`, {
      params: { 
        sortBy: currentFilter.value,
        limit: 12 
      }
    })
    
    if (response.data.success) {
      galleryList.value = response.data.data
    }
  } catch (error) {
    console.error("画廊同步失败:", error)
  } finally {
    isLoading.value = false
  }
}

watch(
  () => [props.userId, currentFilter.value],
  () => {
    fetchGallery()
  },
  { immediate: true }
)

/**
 * 3. 实现真实的跳转逻辑
 */
const goToDetail = (id) => {
  router.push({
    name: 'ArtWorkDetail', // 对应路由表中的 name
    params: { id: id }
  })
}

/**
 * 4. 实现“查看全部”跳转至作品大厅
 */
const handleViewAll = () => {
  router.push('/WorkCenter') 
}
</script>

<style scoped>
/* 样式保持不变 */
.gallery-section-wrapper {
  --primary-blue: #2c3e50;
  --accent-orange: #e67e22;
  --text-main: #333333;
  --text-sub: #666666;
  --bg-light: transparent;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
}

.gallery-section-wrapper {
  display: flex;
  width: 100%;
  position: relative;
  background-image: radial-gradient(#ddd 1px, transparent 1px);
  background-size: 20px 20px;
  background-color: var(--bg-light);
  padding: 10px 0 20px 0;
  overflow: hidden;
  color: var(--text-main);
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
  margin-bottom: 15px;
  padding-left: 5px;
  height: 30px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
  margin-right: 25px;
}

.deco-block {
  width: 6px; 
  height: 22px; 
  background: var(--accent-orange);
}

.header-main {
  font-size: 20px; 
  font-weight: 900; 
  color: var(--primary-blue);
}

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
  flex: 1;
  height: 1px;
  margin: 0 20px;
  opacity: 0.3;
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

.gallery-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 15px 12px;
}

.gallery-card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
}

.thumb-container {
  position: relative;
  width: 100%;
  aspect-ratio: 1 / 1;
  background: rgba(238, 238, 238, 0.4);
  border: 1px solid #ddd;
  overflow: hidden;
  transition: all 0.3s ease;
  border-radius: 4px;
}

.thumb-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.6s ease;
}

.gallery-card:hover .thumb-img {
  transform: scale(1.1);
}

.gallery-card:hover .thumb-container {
  border-color: var(--accent-orange);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.item-title {
  padding-top: 8px;
  font-size: 13px;
  font-weight: bold;
  color: var(--text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.gallery-card:hover .item-title { color: var(--accent-orange); }

.status-msg {
  grid-column: 1 / -1;
  text-align: center;
  padding: 40px;
  color: var(--text-sub);
  font-size: 13px;
  border: 1px dashed #ccc;
}

.decoration-sidebar {
  width: 40px; 
  border-left: 1px solid rgba(0,0,0,0.1); 
  display: flex; 
  flex-direction: column; 
  align-items: center; 
  justify-content: space-between; 
  position: relative;
}

.deco-status-box {
  width: 100%; 
  text-align: center; 
  padding: 10px 0;
  border-right: 3px solid var(--accent-orange);
}

.deco-status-box .label { font-size: 10px; color: var(--text-sub); }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }

.watermark-text {
  writing-mode: vertical-rl;
  font-size: 20px;
  font-weight: 900;
  color: rgba(0, 0, 0, 0.05);
  letter-spacing: 4px;
}

.stripe-bar {
  width: 6px; 
  height: 80px;
  background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px);
}
</style>