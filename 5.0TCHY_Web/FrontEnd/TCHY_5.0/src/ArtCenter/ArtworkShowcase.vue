<template>
  <div class="showcase-container">
    <div class="inner-container">
      
      <header class="hero-header">
        <div class="hero-content">
          <h1 class="main-title">殿堂级视觉大赏</h1>
          <p class="sub-title">汇集全站顶尖艺术结晶。见证笔尖流转的光影，感受直击灵魂的色彩。</p>
        </div>
        
        <div class="time-nav">
          <button 
            v-for="tab in tabs" 
            :key="tab.value"
            :class="['nav-btn', { active: activeTab === tab.value }]"
            @click="activeTab = tab.value"
          >
            {{ tab.label }}
          </button>
        </div>
      </header>

      <main class="gallery-main">
        
        <div v-if="isLoading" class="loading-state">
          <div class="loader"></div>
          <p>正在为您展开画卷...</p>
        </div>

        <div v-else>
          <section class="podium-section" v-if="topThree.length">
            <h2 class="section-title">
              <svg class="icon-crown" fill="currentColor" viewBox="0 0 24 24"><path d="M5 16L3 5L8.5 10L12 4L15.5 10L21 5L19 16H5ZM19 19C19 19.6 18.6 20 18 20H6C5.4 20 5 19.6 5 19V18H19V19Z"></path></svg>
              巅峰杰作
            </h2>
            
            <div class="podium-grid">
              <article 
                v-for="(artwork, index) in topThree" 
                :key="artwork.id" 
                :class="['top-card', `rank-${index + 1}`]"
              >
                <div class="image-wrapper">
                  <div class="rank-badge">{{ index + 1 }}</div>
                  <img :src="artwork.cover" :alt="artwork.title" class="art-img">
                  <div class="hover-overlay">
                    <button class="btn-view">查看详情</button>
                  </div>
                </div>
                
                <div class="art-meta">
                  <h3 class="art-title text-truncate">{{ artwork.title }}</h3>
                  <div class="author-info">
                    <img :src="artwork.author.avatar" alt="avatar" class="author-avatar">
                    <span class="author-name text-truncate">{{ artwork.author.name }}</span>
                  </div>
                  <div class="stats">
                    <div class="stat"><span class="icon">♥</span> {{ artwork.likes }}</div>
                    <div class="stat"><span class="icon">👁</span> {{ artwork.views }}</div>
                  </div>
                </div>
              </article>
            </div>
          </section>

          <section class="excellence-section" v-if="restArtworks.length">
            <h2 class="section-title">卓越之选</h2>
            
            <div class="art-grid">
              <article v-for="(artwork, index) in restArtworks" :key="artwork.id" class="standard-card">
                <div class="image-wrapper">
                  <div class="rank-tag">{{ index + 4 }}</div>
                  <img :src="artwork.cover" :alt="artwork.title" class="art-img">
                </div>
                <div class="art-meta-sm">
                  <h4 class="art-title-sm text-truncate">{{ artwork.title }}</h4>
                  <div class="author-row">
                    <img :src="artwork.author.avatar" alt="avatar" class="author-avatar-sm">
                    <span class="author-name-sm text-truncate">{{ artwork.author.name }}</span>
                    <span class="likes-sm">♥ {{ artwork.likes }}</span>
                  </div>
                </div>
              </article>
            </div>
          </section>
        </div>

      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'

// 筛选标签
const tabs = [
  { label: '年度最佳', value: 'yearly' },
  { label: '本月精选', value: 'monthly' },
  { label: '周榜排行', value: 'weekly' }
]
const activeTab = ref('yearly')
const isLoading = ref(false)

// 模拟数据源
const artworks = ref([])

// 计算属性：前三名
const topThree = computed(() => artworks.value.slice(0, 3))
// 计算属性：第四名及以后
const restArtworks = computed(() => artworks.value.slice(3))

// 模拟 API 请求获取排行榜数据
const fetchLeaderboard = async (period) => {
  isLoading.value = true
  // 模拟网络延迟
  await new Promise(resolve => setTimeout(resolve, 800))
  
  // 生成随机假数据以展示效果
  const mockData = Array.from({ length: 15 }).map((_, i) => ({
    id: `${period}-${i}`,
    title: `虚构幻境与光影交响曲 Vol.${i + 1}`,
    // 使用 Unsplash 占位图模拟高质量插画
    cover: `https://images.unsplash.com/photo-${1550000000000 + (i * 100000)}?auto=format&fit=crop&w=600&q=80`,
    author: {
      name: `画师_0${i + 1}`,
      avatar: `https://i.pravatar.cc/150?u=${period}${i}`
    },
    likes: Math.floor(Math.random() * 50000) + 1000,
    views: Math.floor(Math.random() * 200000) + 5000
  })).sort((a, b) => b.likes - a.likes) // 按点赞数排序

  artworks.value = mockData
  isLoading.value = false
}

// 监听 Tab 切换，重新获取数据
watch(activeTab, (newTab) => {
  fetchLeaderboard(newTab)
})

onMounted(() => {
  fetchLeaderboard(activeTab.value)
})
</script>

<style scoped>
/* ==================== 基础设定 ==================== */
.showcase-container {
  background-color: #f8fafc; /* 延续你的 slate-50 背景 */
  min-height: 100vh;
  padding: 0 0 4rem 0;
  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: #0f172a;
}

.showcase-container * {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

.inner-container {
  max-width: 88rem; /* 艺术展厅需要更宽的画幅 (1408px) */
  margin: 0 auto;
  padding: 0 1.5rem;
}

button { cursor: pointer; border: none; background: none; font-family: inherit; }
.text-truncate { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* ==================== 头部与导航 ==================== */
.hero-header {
  padding: 4rem 0 3rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.main-title {
  font-size: 2.5rem;
  font-weight: 900;
  color: #0f172a;
  letter-spacing: 0.05em;
  margin-bottom: 1rem;
}

.sub-title {
  font-size: 1rem;
  color: #64748b;
  max-width: 40rem;
  line-height: 1.6;
  margin-bottom: 2.5rem;
}

/* 时间导航 Pills */
.time-nav {
  display: flex;
  gap: 0.5rem;
  background-color: #e2e8f0;
  padding: 0.375rem;
  border-radius: 999px;
}

.nav-btn {
  padding: 0.625rem 1.5rem;
  font-weight: 600;
  font-size: 0.9375rem;
  color: #475569;
  border-radius: 999px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.nav-btn:hover { color: #0f172a; }

.nav-btn.active {
  background-color: #fff;
  color: #4f46e5; /* Indigo 强调色 */
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

/* ==================== 公共区块标题 ==================== */
.section-title {
  font-size: 1.5rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #1e293b;
}
.icon-crown { width: 1.75rem; height: 1.75rem; color: #eab308; }

/* ==================== 冠军/前三名特殊展示区 ==================== */
.podium-section { margin-bottom: 4rem; }

.podium-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}

@media (min-width: 1024px) {
  .podium-grid {
    grid-template-columns: 1fr 1.2fr 1fr; /* 中间的 Rank 1 更宽 */
    align-items: flex-end; /* 底部对齐，让中间显得更高大 */
  }
}

.top-card {
  background: #fff;
  border-radius: 1.25rem;
  overflow: hidden;
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05);
  transition: transform 0.3s ease;
  border: 2px solid transparent;
}
.top-card:hover { transform: translateY(-5px); }

/* 为前三名赋予专属色彩边框 */
.top-card.rank-1 { border-color: #fde047; box-shadow: 0 20px 25px -5px rgba(250, 204, 21, 0.15); }
.top-card.rank-2 { border-color: #cbd5e1; }
.top-card.rank-3 { border-color: #fed7aa; }

/* Rank 1 在大屏下稍微上浮，显得最高 */
@media (min-width: 1024px) {
  .top-card.rank-1 { transform: translateY(-20px); }
  .top-card.rank-1:hover { transform: translateY(-25px); }
  /* 调整DOM显示顺序使得页面视觉为 2 - 1 - 3 */
  .top-card.rank-1 { order: 2; }
  .top-card.rank-2 { order: 1; }
  .top-card.rank-3 { order: 3; }
}

.top-card .image-wrapper {
  position: relative;
  aspect-ratio: 4/3; /* 经典画作比例 */
  overflow: hidden;
}
.top-card.rank-1 .image-wrapper { aspect-ratio: 1/1; /* 冠军给予更大的方形展示面 */ }

.art-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}
.top-card:hover .art-img { transform: scale(1.05); }

/* 皇冠角标 */
.rank-badge {
  position: absolute;
  top: 1rem;
  left: 1rem;
  width: 2.5rem;
  height: 2.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  font-weight: 900;
  border-radius: 50%;
  color: #fff;
  z-index: 10;
  box-shadow: 0 4px 6px rgba(0,0,0,0.2);
}
.rank-1 .rank-badge { background: linear-gradient(135deg, #fde047, #ca8a04); }
.rank-2 .rank-badge { background: linear-gradient(135deg, #e2e8f0, #94a3b8); }
.rank-3 .rank-badge { background: linear-gradient(135deg, #fed7aa, #b45309); }

/* 悬停遮罩 */
.hover-overlay {
  position: absolute;
  inset: 0;
  background: rgba(15, 23, 42, 0.4);
  opacity: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.3s ease;
}
.top-card:hover .hover-overlay { opacity: 1; }
.btn-view {
  background: #fff;
  color: #0f172a;
  padding: 0.75rem 1.5rem;
  border-radius: 999px;
  font-weight: 600;
  transform: translateY(10px);
  transition: transform 0.3s ease;
}
.top-card:hover .btn-view { transform: translateY(0); }

/* 卡片信息区 */
.art-meta { padding: 1.5rem; }
.art-title { font-size: 1.125rem; font-weight: 700; margin-bottom: 0.75rem; color: #0f172a; }
.author-info { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; }
.author-avatar { width: 1.75rem; height: 1.75rem; border-radius: 50%; object-fit: cover; }
.author-name { font-size: 0.875rem; color: #475569; font-weight: 500; }
.stats { display: flex; gap: 1rem; color: #64748b; font-size: 0.875rem; font-weight: 600; }
.icon { color: #f43f5e; margin-right: 0.25rem; } /* 爱心颜色 */

/* ==================== 常规作品网格 (Rank 4+) ==================== */
.art-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

.standard-card {
  background: #fff;
  border-radius: 0.75rem;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  transition: all 0.2s ease;
}
.standard-card:hover {
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
  transform: translateY(-2px);
}

.standard-card .image-wrapper {
  position: relative;
  aspect-ratio: 1/1;
  overflow: hidden;
}
.standard-card:hover .art-img { transform: scale(1.05); }

.rank-tag {
  position: absolute;
  top: 0.5rem;
  left: 0.5rem;
  background: rgba(15, 23, 42, 0.7);
  color: #fff;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
  backdrop-filter: blur(4px);
  z-index: 10;
}

.art-meta-sm { padding: 1rem; }
.art-title-sm { font-size: 0.9375rem; font-weight: 600; color: #1e293b; margin-bottom: 0.5rem; }
.author-row { display: flex; align-items: center; gap: 0.375rem; }
.author-avatar-sm { width: 1.25rem; height: 1.25rem; border-radius: 50%; }
.author-name-sm { font-size: 0.75rem; color: #64748b; flex: 1; }
.likes-sm { font-size: 0.75rem; color: #94a3b8; font-weight: 600; }

/* ==================== 加载状态 ==================== */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 6rem 0;
  color: #94a3b8;
}
.loader {
  border: 3px solid #f1f5f9;
  border-top: 3px solid #4f46e5;
  border-radius: 50%;
  width: 2rem;
  height: 2rem;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>