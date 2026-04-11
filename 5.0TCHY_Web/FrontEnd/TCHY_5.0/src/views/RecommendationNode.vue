<template>
  <div class="media-matrix-container custom-scroll">
    <div class="inner-container">
      
      <header class="matrix-header">
        <h1 class="sys-title">精选作品矩阵</h1>
        <div class="accent-bar"></div>
        <p class="sys-desc">汇聚高品质视听档案。作品展示面积随综合得分精确缩放。</p>
      </header>

      <div class="matrix-bento-grid">
        <article 
          v-for="item in rawData" 
          :key="item.id" 
          class="bento-card"
          :style="calculateAdaptiveBentoStyle(item)"
          @click="openSignal(item)"
        >
          <div class="card-bg-wrapper">
            <img 
              :src="item.coverUrl" 
              class="bg-img" 
              alt="封面" 
              loading="lazy"
              referrerpolicy="no-referrer"
            >
            <div class="overlay-gradient"></div>
          </div>

          <div class="card-content">
            <div class="top-bar">
              <span class="media-type-tag">{{ translateType(item.mediaType) }}</span>
              <span class="recommend-index">推荐指数 {{ getFinalIndex(item) }}</span>
            </div>

            <div class="bottom-info">
              <h3 class="m-title" :style="{ fontSize: getAdaptiveFontSize(item) }">
                {{ item.title }}
              </h3>
              
              <UniversalTags :tags="parseTags(item.tags)" class="matrix-tags-area" />

              <p class="m-desc text-truncate-2">
                {{ item.description || '暂无作品简介' }}
              </p>
            </div>
          </div>
        </article>
      </div>

      <div ref="loadMoreRef" class="load-more-sentinel">
        <div v-if="isLoading" class="loading-status">
          <div class="md-spinner"></div>
          <span>正在同步档案信号...</span>
        </div>
        <div v-else-if="isNoMore" class="end-text">—— 已显示全部推荐作品 ——</div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import apiClient from '@/utils/api'
import UniversalTags from '@/GeneralComponents/UniversalTags.vue' 

const isLoading = ref(false)
const isNoMore = ref(false)
const rawData = ref([])
const currentOffset = ref(0)
const LIMIT_SIZE = 15
const loadMoreRef = ref(null)
let observer = null

/**
 * 🌟 完全动态的线性缩放算法 (已调整参数以缩小整体卡片)
 */

// 1. 获取基础分 (权重1.5倍影响 + 评分1倍影响)
const getFinalIndex = (item) => {
  return parseFloat(((item.weight * 1.5) + item.rating).toFixed(1));
};

// 2. 🌟 完全动态排盘逻辑：线性映射
const calculateAdaptiveBentoStyle = (item) => {
  const index = getFinalIndex(item);
  
  /**
   * 宽度计算：映射到 100 列系统
   * 🌟 调整：将宽度系数从之前的例如 3.8 减小到 3.2，让卡片在视觉上整体“窄”一截。
   * 公式：span = index * 系数
   * 宇宙涅槃 (16.5分) -> span 约 53 (占 53% 宽度)
   * 时间之战 (14.5分) -> span 约 46 (占 46% 宽度)
   * 仍然满足“76分比75.5分大一点”和“像素级差异”的要求。
   */
  let colSpan = Math.round(index * 3.2); 
  colSpan = Math.max(25, Math.min(100, colSpan)); // 限制最小占 25%，最大 100%

  /**
   * 高度计算：
   * 🌟 调整：由于行高整体缩小了，这里的逻辑自然也会跟随。
   * 维持高度跃迁判定门槛：指数 > 14.5 占双倍高。
   */
  let rowSpan = 1;
  if (index > 14.5) rowSpan = 2; // 中高分作品自动变大块

  return {
    gridColumn: `span ${colSpan}`,
    gridRow: `span ${rowSpan}`
  };
};

// 3. 字体自适应：稍微减小字体基准值
const getAdaptiveFontSize = (item) => {
  const index = getFinalIndex(item);
  const size = 1.0 + (index / 18);
  return `${Math.min(2.2, size)}rem`;
};

const translateType = (type) => {
  const map = { 'ANIME': '动画', 'MOVIE': '电影', 'VIDEO': '视频' };
  return map[type] || type;
};

const parseTags = (tagStr) => tagStr ? tagStr.split(',').map(t => t.trim()).filter(t => t) : [];

const openSignal = (item) => {
  if (item.targetUrl) window.open(item.targetUrl, '_blank')
}

const fetchMediaData = async () => {
  if (isLoading.value || isNoMore.value) return
  isLoading.value = true
  try {
    const res = await apiClient.get('/MediaRecommendation/board', {
      params: { limit: LIMIT_SIZE, offset: currentOffset.value }
    })
    if (res.data?.success) {
      const newData = res.data.data
      if (newData.length < LIMIT_SIZE) isNoMore.value = true
      rawData.value = [...rawData.value, ...newData]
      currentOffset.value += newData.length
    }
  } catch (e) {
    console.error(e)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchMediaData()
  observer = new IntersectionObserver((entries) => {
    if (entries[0].isIntersecting && !isLoading.value && !isNoMore.value) {
      fetchMediaData()
    }
  }, { threshold: 0.1 })
  if (loadMoreRef.value) observer.observe(loadMoreRef.value)
})

onUnmounted(() => { if (observer) observer.disconnect() })
</script>

<style scoped>
.media-matrix-container {
  height: 100%; overflow-y: auto; background-color: #F8F7F4; 
  padding: 4rem 2rem; font-family: 'PingFang SC', sans-serif; color: #111;
}
.inner-container {
  max-width: 1400px; /* 根据你的缩放需求，可以微调这个宽度 */
  margin: 0 auto;    /* 核心：确保容器本身在页面水平居中 */
  display: flex;
  flex-direction: column;
  /* 确保 header 和 grid 的左右边距对齐 */
  align-items: stretch; 
}

/* 头部样式：干净高级 */
.matrix-header {
  margin-bottom: 4rem;
  text-align: left;
  /* 移除不必要的 margin 或 padding，确保和下方卡片左对齐线一致 */
  padding-left: 0; 
}
.sys-title { font-size: 3.5rem; font-weight: 900; color: #111; margin: 0; letter-spacing: -2px; }
.accent-bar { width: 50px; height: 5px; background: #D92323; margin: 20px 0; }
.sys-desc { color: #888; font-size: 1.1rem; max-width: 600px; line-height: 1.6; }

/* 🌟 高精度网格排盘：100列，通过 grid-auto-rows 缩小全局行高 */
.matrix-bento-grid {
  display: grid; 
  grid-template-columns: repeat(100, 1fr); 
  grid-auto-rows: 190px; 
  grid-auto-flow: dense; 
  gap: 20px;
  
  /* 🌟 关键新增：如果一行没占满 100 份，让内容整体居中 */
  justify-content: center; 
  /* 确保网格容器占满 inner-container 的宽度 */
  width: 100%; 
}

/* 响应式降级 */
@media (max-width: 1000px) { .matrix-bento-grid { grid-template-columns: repeat(50, 1fr); } }
@media (max-width: 600px) { .matrix-bento-grid { display: flex; flex-direction: column; } }

.bento-card {
  position: relative; background: #fff; overflow: hidden; cursor: pointer;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05); transition: all 0.5s cubic-bezier(0.165, 0.84, 0.44, 1);
  border-radius: 4px;
}
.bento-card:hover { transform: translateY(-8px); box-shadow: 0 20px 40px rgba(0,0,0,0.08); z-index: 10; }

.card-bg-wrapper { position: absolute; inset: 0; z-index: 1; }
.bg-img { width: 100%; height: 100%; object-fit: cover; transition: 1.2s; opacity: 0.95; }
.bento-card:hover .bg-img { transform: scale(1.04); }

.overlay-gradient {
  position: absolute; inset: 0;
  background: linear-gradient(to top, rgba(0,0,0,0.92) 0%, rgba(0,0,0,0.2) 60%, transparent 100%);
}

.card-content {
  position: absolute; inset: 0; 
  padding: 1.6rem; /* 🌟 核心调整：减少 Padding，从 2.2rem 减少到 1.6rem */
  display: flex; flex-direction: column; justify-content: space-between; z-index: 2;
}

.top-bar { display: flex; justify-content: space-between; align-items: center; }
.media-type-tag { background: #111; color: #fff; font-size: 0.7rem; padding: 3px 10px; font-weight: 800; }
/* 🌟 徽章字号缩小，从 0.75rem 缩小到 0.7rem */
.score-badge, .score-label, .recommend-index { 
  background: #D92323; color: #fff; font-size: 0.7rem; padding: 3px 10px; 
  font-weight: 900; box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.2); 
}

.m-title { color: #fff; font-weight: 800; text-shadow: 0 2px 15px rgba(0,0,0,0.4); line-height: 1.1; margin-bottom: 10px; }

/* 🌟 描述文字修复逻辑：悬停动态撑开容器高度，解决看不到问题 */
.m-desc { 
  color: rgba(255,255,255,0.8); font-size: 0.9rem; line-height: 1.6; 
  margin-top: 10px; height: 0; opacity: 0; overflow: hidden; transition: 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}
/* 大尺寸卡片默认显示描述的一部分 */
.size-large .m-desc, .size-wide .m-desc { height: auto; opacity: 0.7; }
/* 悬停时所有卡片都优雅浮现完整描述 */
.bento-card:hover .m-desc { height: auto; opacity: 1; }

.text-truncate-2 { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

/* 加载监测点 */
.load-more-sentinel { padding: 6rem 0; text-align: center; color: #999; }
.md-spinner {
  width: 32px; height: 32px; border: 3px solid rgba(217, 35, 35, 0.1);
  border-top-color: #D92323; border-radius: 50%; animation: spin 0.8s infinite linear; margin: 0 auto 15px;
}
@keyframes spin { 100% { transform: rotate(360deg); } }

/* 标签组件深度微调 */
:deep(.matrix-tags-area .universal-tags-wrapper) { margin-top: 0 !important; padding: 0 !important; border-top: none !important; }
:deep(.matrix-tags-area .cyber-tag-chip) { background: rgba(255,255,255,0.15) !important; color: #fff !important; border: none !important; backdrop-filter: blur(8px); }
</style>