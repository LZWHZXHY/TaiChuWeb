<template>
  <div class="showcase-container custom-scroll">
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
                @click="goToDetail(artwork.id)" 
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
              <article 
                v-for="(artwork, index) in restArtworks" 
                :key="artwork.id" 
                class="standard-card"
                @click="goToDetail(artwork.id)"
              >
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
import { useRouter } from 'vue-router' // 🌟 引入路由
import apiClient from '@/utils/api'

const router = useRouter() // 🌟 初始化路由实例

const tabs = [
  { label: '今日热点', value: 'daily' },
  { label: '周榜排行', value: 'weekly' },
  { label: '本月精选', value: 'monthly' },
  { label: '年度最佳', value: 'yearly' }
]
const activeTab = ref('monthly')
const isLoading = ref(false)
const artworks = ref([])

const topThree = computed(() => artworks.value.slice(0, 3))
const restArtworks = computed(() => artworks.value.slice(3))

/**
 * 🌟 跳转至详情页
 * @param {string|number} id 作品ID
 */
const goToDetail = (id) => {
  // 对应你配置的路由名称 'ArtWorkDetail'
  router.push({
    name: 'ArtWorkDetail',
    params: { id: id }
  })
}

const fetchLeaderboard = async (period) => {
  isLoading.value = true
  try {
    const res = await apiClient.get(`/Drawing/showcase?period=${period}&limit=15`)
    if (res.data && res.data.success) {
      artworks.value = res.data.data
    }
  } catch (error) {
    console.error("殿堂数据拉取失败:", error)
  } finally {
    isLoading.value = false
  }
}

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
  background-color: #f8fafc; 
  height: 100%; 
  overflow-y: auto; 
  padding: 0 0 4rem 0;
  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: #0f172a;
}

/* 🌟 增加点击手型 */
.top-card, .standard-card {
  cursor: pointer;
}

.showcase-container::-webkit-scrollbar { width: 8px; }
.showcase-container::-webkit-scrollbar-track { background: transparent; }
.showcase-container::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }

.inner-container {
  max-width: 88rem;
  margin: 0 auto;
  padding: 0 1.5rem;
}

.text-truncate { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* 头部视觉 */
.hero-header { padding: 4rem 0 3rem; text-align: center; display: flex; flex-direction: column; align-items: center; }
.main-title { font-size: 2.5rem; font-weight: 900; margin-bottom: 1rem; }
.sub-title { font-size: 1rem; color: #64748b; max-width: 40rem; line-height: 1.6; margin-bottom: 2.5rem; }

/* 导航控制 */
.time-nav { display: flex; gap: 0.5rem; background-color: #e2e8f0; padding: 0.375rem; border-radius: 999px; }
.nav-btn { 
  padding: 0.625rem 1.5rem; border-radius: 999px; transition: 0.3s; color: #475569; font-weight: 600;
  cursor: pointer; border: none; background: none; 
}
.nav-btn.active { background-color: #fff; color: #4f46e5; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); }

/* 区块样式 */
.section-title { font-size: 1.5rem; font-weight: 800; margin-bottom: 1.5rem; display: flex; align-items: center; gap: 0.5rem; }
.icon-crown { width: 1.75rem; height: 1.75rem; color: #eab308; }

/* 前三名布局 */
.podium-grid { display: grid; grid-template-columns: 1fr; gap: 2rem; }
@media (min-width: 1024px) {
  .podium-grid { grid-template-columns: 1fr 1.2fr 1fr; align-items: flex-end; }
  .top-card.rank-1 { order: 2; transform: translateY(-20px); }
  .top-card.rank-2 { order: 1; }
  .top-card.rank-3 { order: 3; }
}

.top-card { background: #fff; border-radius: 1.25rem; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); border: 2px solid transparent; transition: 0.3s; }
.top-card:hover { transform: translateY(-5px); }
.top-card.rank-1 { border-color: #fde047; }
.top-card.rank-2 { border-color: #cbd5e1; }
.top-card.rank-3 { border-color: #fed7aa; }

.image-wrapper { position: relative; aspect-ratio: 4/3; overflow: hidden; }
.top-card.rank-1 .image-wrapper { aspect-ratio: 1/1; }
.art-img { width: 100%; height: 100%; object-fit: cover; transition: 0.5s; }
.top-card:hover .art-img { transform: scale(1.05); }

.rank-badge { 
  position: absolute; top: 1rem; left: 1rem; width: 2.5rem; height: 2.5rem; 
  display: flex; align-items: center; justify-content: center; font-weight: 900; 
  border-radius: 50%; color: #fff; z-index: 10;
}
.rank-1 .rank-badge { background: linear-gradient(135deg, #fde047, #ca8a04); }
.rank-2 .rank-badge { background: linear-gradient(135deg, #e2e8f0, #94a3b8); }
.rank-3 .rank-badge { background: linear-gradient(135deg, #fed7aa, #b45309); }

.hover-overlay { position: absolute; inset: 0; background: rgba(15, 23, 42, 0.4); opacity: 0; display: flex; align-items: center; justify-content: center; transition: 0.3s; }
.top-card:hover .hover-overlay { opacity: 1; }
.btn-view { background: #fff; padding: 0.75rem 1.5rem; border-radius: 999px; font-weight: 600; cursor: pointer; border: none; }

.art-meta { padding: 1.5rem; }
.author-info { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; }
.author-avatar { width: 1.75rem; height: 1.75rem; border-radius: 50%; }

/* 普通网格 */
.art-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1.5rem; }
.standard-card { background: #fff; border-radius: 0.75rem; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.05); transition: 0.2s; }
.standard-card:hover { transform: translateY(-2px); box-shadow: 0 10px 15px rgba(0,0,0,0.1); }
.standard-card .image-wrapper { position: relative; aspect-ratio: 1/1; }
.rank-tag { position: absolute; top: 0.5rem; left: 0.5rem; background: rgba(15, 23, 42, 0.7); color: #fff; padding: 0.125rem 0.5rem; border-radius: 0.25rem; font-size: 0.75rem; }

.art-meta-sm { padding: 1rem; }
.author-row { display: flex; align-items: center; gap: 0.375rem; }
.author-avatar-sm { width: 1.25rem; height: 1.25rem; border-radius: 50%; }
.author-name-sm { font-size: 0.75rem; color: #64748b; flex: 1; }

/* 加载动画 */
.loader { border: 3px solid #f1f5f9; border-top: 3px solid #4f46e5; border-radius: 50%; width: 2rem; height: 2rem; animation: spin 1s linear infinite; margin-bottom: 1rem; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>