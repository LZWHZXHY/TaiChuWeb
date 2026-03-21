<template>
  <div class="gallery-root">
    <header class="gallery-app-bar">
      <div class="app-bar-inner">
        <div class="brand">
         
          <div class="texts">
            <h1>艺术画廊</h1>
            <span class="meta">ARCHIVE_SYSTEM // 2.1.2_OPTIMIZED</span>
          </div>
        </div>

        <nav class="segment-pills">
          <button 
            v-for="seg in segments" :key="seg.id"
            class="pill-btn"
            :class="{ active: selectedSegment === seg.id }"
            @click="handleSegmentClick(seg.id)"
          >
            <span class="p-icon">{{ seg.icon }}</span>
            <span class="p-label">{{ seg.label }}</span>
          </button>
        </nav>
      </div>
    </header>

    <div class="main-layout">
      <main class="content-stream custom-scroll" ref="mainScroll" @scroll="handleScroll">
        <div v-if="loading && totalCount === 0" class="init-loader">
          <div class="md-linear-loader"></div>
          <p>正在读取神经数据流...</p>
        </div>

        <div v-else class="waterfall-grid">
          <div v-for="(col, colIdx) in waterfallColumns" :key="colIdx" class="waterfall-col">
            <div 
              v-for="img in col" 
              :key="img.id || img.Id" 
              class="art-card-node"
              :class="{ 'is-folder': isAlbum(img) }"
              @click="goToDetail(img)"
            >
              <div class="media-container">
                <img 
                  :src="upgradeUrlToHttps(img.coverImageUrl || img.imageUrl || img.url)" 
                  loading="lazy" 
                  decoding="async"
                />
                
                <div v-if="isAlbum(img)" class="folder-hint">
                  <span>📁 {{ img.itemCount || 0 }}</span>
                </div>

                <div class="overlay-info">
                  <div class="stats">
                    <span class="stat-pill">♥ {{ img.likes || 0 }}</span>
                    <span v-if="!isAlbum(img)" class="stat-pill">💬 {{ img.commentCount || 0 }}</span>
                  </div>
                </div>
              </div>

              <div class="text-content">
                <h3 class="title" :title="img.title || img.Title">{{ img.title || img.Title || 'Untitled' }}</h3>
                <span class="author">@{{ img.AuthorName || img.authorName || 'Anonymous' }}</span>
              </div>
            </div>
          </div>
        </div>

        <div class="stream-footer">
          <div v-if="loading && totalCount > 0" class="loading-inline">STREAMING...</div>
          <div v-if="!hasMore && totalCount > 0" class="end-marker">END_OF_DATA_STREAM</div>
        </div>
      </main>

      <aside class="sidebar-info">
        <div class="side-widget rank-widget">
          <div class="widget-header">🏆 贡献排行 / TOP_15</div>
          <div class="rank-list custom-scroll">
            <div v-for="(user, idx) in leaderboard" :key="user.UploaderId" class="rank-item-compact">
              <div class="num" :class="{ 'gold': idx < 3 }">{{ idx + 1 }}</div>
              
              <div class="avatar-holder">
                <UserAvatar 
                  :user-id="user.UploaderId" 
                  :passed-avatar="user.Avatar"
                  :size="32"
                  :show-level="false"
                />
              </div>

              <div class="info">
                <div class="u-name" :title="user.name">{{ user.name }}</div>
                <div class="u-meta">{{ user.TotalLikes }} LIKES</div>
              </div>
            </div>
          </div>
        </div>

        <div class="side-widget tags-widget">
          <div class="widget-header"># 热门矩阵</div>
          <div class="tag-chips">
            <span v-for="t in ['赛博朋克','插画','二创','UI','概念']" :key="t" class="m-chip">#{{ t }}</span>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, nextTick } from 'vue'
import { useRouter, onBeforeRouteLeave } from 'vue-router'
import apiClient from '@/utils/api'
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'

const router = useRouter()

const segments = [
  { id: 1, label: '动态', icon: '⚡' },
  { id: 2, label: '全部', icon: '∞' },
  { id: 4, label: '画册', icon: '📁' },
  { id: 3, label: '单图', icon: '■' }
]

const selectedSegment = ref(2)
const waterfallColumns = ref([[], [], [], [], []]) // 标准 5 列
const loading = ref(false)
const nextCursor = ref(null)
const hasMore = ref(true)
const mainScroll = ref(null)
const leaderboard = ref([])

const totalCount = computed(() => waterfallColumns.value.reduce((acc, col) => acc + col.length, 0))
const isAlbum = (img) => selectedSegment.value === 4 || img.isFolder || img.FolderType === 0

// --- 逻辑方法 ---

const handleSegmentClick = (id) => {
  if (selectedSegment.value === id) return
  selectedSegment.value = id
  refreshGallery()
}

const refreshGallery = () => {
  waterfallColumns.value = [[], [], [], [], []]
  nextCursor.value = null
  hasMore.value = true
  if (mainScroll.value) mainScroll.value.scrollTop = 0
  fetchArtworks(true)
}

const fetchArtworks = async (isRefresh = false) => {
  if (loading.value || (!isRefresh && !hasMore.value)) return
  loading.value = true
  
  try {
    let items = []
    let cursor = null
    let more = false

    const endpoint = selectedSegment.value === 4 ? '/Folder/all-list' : '/Drawing/list'
    const params = selectedSegment.value === 4 
      ? { category: 0, type: 0 } 
      : { pageSize: 25, cursor: isRefresh ? null : nextCursor.value, type: selectedSegment.value }

    const res = await apiClient.get(endpoint, { params })
    
    if (res.data.success) {
      if (selectedSegment.value === 4) {
        items = res.data.data.map(a => ({ ...a, isFolder: true }))
      } else {
        items = res.data.data.items || res.data.data
        cursor = res.data.data.nextCursor
        more = res.data.data.hasMore ?? false
      }

      // 混合逻辑
      if (selectedSegment.value === 2 && isRefresh) {
        const albRes = await apiClient.get('/Folder/all-list', { params: { category: 0, type: 0 } })
        if (albRes.data.success) {
          const albums = albRes.data.data.map(a => ({ ...a, isFolder: true, uploadAt: a.createdAt }))
          items = [...albums, ...items].sort((a, b) => new Date(b.uploadAt || b.createdAt) - new Date(a.uploadAt || a.createdAt))
        }
      }

      if (isRefresh) waterfallColumns.value = [[], [], [], [], []]

      // 🚀 核心优化：分帧注入 (Staggered Injection)
      // 解决 GIF 模式下的瞬间掉帧问题
      let i = 0
      const inject = () => {
        if (i >= items.length) {
          loading.value = false
          return
        }
        // 每次注入 2 个，留出主线程解码 GIF
        for (let j = 0; j < 2 && i < items.length; j++) {
          const item = items[i]
          const lens = waterfallColumns.value.map(c => c.length)
          const minIdx = lens.indexOf(Math.min(...lens))
          waterfallColumns.value[minIdx].push(item)
          i++
        }
        requestAnimationFrame(inject)
      }
      inject()
      
      nextCursor.value = cursor
      hasMore.value = more
    }
  } catch (e) { 
    console.error(e) 
    loading.value = false 
  }
}

const handleScroll = (e) => {
  const { scrollTop, scrollHeight, clientHeight } = e.target
  if (scrollTop + clientHeight >= scrollHeight - 350) fetchArtworks(false)
}

const fetchLeaderboard = async () => {
  const res = await apiClient.get('/Drawing/leaderboard?limit=15')
  if (res.data.success) leaderboard.value = res.data.data
}

const goToDetail = (img) => {
  const path = isAlbum(img) ? `/album/${img.id || img.Id}` : `/gallery/${img.id || img.Id}`
  router.push(path)
}

const upgradeUrlToHttps = (url) => url ? url.replace('http://', 'https://') : '/placeholder.jpg'
const padZero = (n) => n < 10 ? `0${n}` : n

// --- 🌟 核心：定位恢复与快照逻辑 🌟 ---

onMounted(() => {
  const cached = sessionStorage.getItem('cyberGalleryCache')
  if (cached) {
    try {
      const p = JSON.parse(cached)
      waterfallColumns.value = p.columns
      nextCursor.value = p.cursor
      hasMore.value = p.hasMore
      selectedSegment.value = p.segment
      nextTick(() => {
        if (mainScroll.value) mainScroll.value.scrollTop = p.scroll
      })
    } catch (e) { fetchArtworks(true) }
    sessionStorage.removeItem('cyberGalleryCache')
  } else {
    fetchArtworks(true)
  }
  fetchLeaderboard()
})

onBeforeRouteLeave((to, from, next) => {
  if (to.path.includes('/gallery/') || to.path.includes('/album/')) {
    const data = {
      columns: waterfallColumns.value,
      cursor: nextCursor.value,
      hasMore: hasMore.value,
      segment: selectedSegment.value,
      scroll: mainScroll.value ? mainScroll.value.scrollTop : 0
    }
    sessionStorage.setItem('cyberGalleryCache', JSON.stringify(data))
  } else {
    sessionStorage.removeItem('cyberGalleryCache')
  }
  next()
})
</script>

<style scoped>
/* --- MD3 DENSE DESIGN SYSTEM --- */
.gallery-root {
  --md-surface: #f7f7f9;
  --md-on-surface: #1a1c1e;
  --md-primary: #005fb0;
  --md-outline: #c4c7cf;
  --md-card: #ffffff;

  display: flex;
  flex-direction: column;
  height: 100vh;
  width: 100%;
  background: var(--md-surface);
  overflow: hidden;
  box-sizing: border-box;
}

/* --- 1. 应用栏 --- */
.gallery-app-bar {
  height: 56px;
  background: #fff;
  border-bottom: 1px solid var(--md-outline);
  z-index: 100;
  flex-shrink: 0;
}
.app-bar-inner {
  max-width: 1800px;
  margin: 0 auto;
  height: 100%;
  padding: 0 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.brand h1 { font-size: 1rem; font-weight: 800; margin: 0; letter-spacing: -0.3px; }
.logo { background: #000; color: #fff; width: 28px; height: 28px; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-weight: 900; font-size: 14px; margin-right: 12px; }
.texts { display: flex; flex-direction: column; }
.meta { font-size: 0.6rem; color: #888; font-family: monospace; font-weight: bold; }

.segment-pills { display: flex; gap: 4px; background: #eff1f4; padding: 4px; border-radius: 10px; }
.pill-btn { border: none; background: transparent; padding: 4px 16px; border-radius: 8px; cursor: pointer; font-weight: 700; font-size: 0.8rem; color: #444; transition: 0.2s; display: flex; align-items: center; gap: 6px; }
.pill-btn.active { background: #fff; color: var(--md-primary); box-shadow: 0 2px 6px rgba(0,0,0,0.06); }

/* --- 2. 布局：Grid 严格控制主区不溢出 --- */
.main-layout {
  flex: 1;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 260px;
  max-width: 1800px;
  width: 100%;
  margin: 0 auto;
  overflow: hidden;
}

/* --- 瀑布流主区 (性能核心) --- */
.content-stream {
  padding: 16px;
  overflow-y: auto;
  overflow-x: hidden; /* 彻底禁止横滚 */
}
.waterfall-grid { display: flex; gap: 12px; }
.waterfall-col { flex: 1; display: flex; flex-direction: column; gap: 12px; min-width: 0; }

.art-card-node {
  background: var(--md-card);
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid var(--md-outline);
  transition: transform 0.2s;
  cursor: pointer;
  
  /* 🚀 性能黑科技：隔离卡片，防止 GIF 渲染影响全局 */
  contain: layout paint;
  content-visibility: auto;
  contain-intrinsic-size: 100px 300px;
}
.art-card-node:hover { border-color: var(--md-primary); transform: translateY(-2px); }

.media-container { position: relative; width: 100%; background: #f0f0f0; min-height: 80px; }
.media-container img { width: 100%; display: block; object-fit: cover; }

.overlay-info {
  position: absolute; inset: 0; background: linear-gradient(to top, rgba(0,0,0,0.3), transparent);
  display: flex; align-items: flex-end; padding: 8px; opacity: 0; transition: 0.2s;
}
.art-card-node:hover .overlay-info { opacity: 1; }
.stat-pill { background: rgba(255,255,255,0.9); color: #000; padding: 1px 8px; border-radius: 4px; font-size: 0.65rem; font-weight: 800; margin-right: 6px; }

.text-content { padding: 8px 10px; }
.text-content .title { font-size: 0.8rem; font-weight: 700; margin: 0 0 2px 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.text-content .author { font-size: 0.65rem; color: #666; font-weight: 600; }

/* --- 3. 侧边栏：彻底修复遮挡问题 --- */
.sidebar-info {
  padding: 16px 16px 16px 0;
  display: flex; flex-direction: column; gap: 16px;
  overflow: hidden;
}
.side-widget { background: #fff; border: 1px solid var(--md-outline); border-radius: 16px; padding: 12px; }
.widget-header { font-size: 0.7rem; font-weight: 900; color: #888; margin-bottom: 12px; text-transform: uppercase; }

.rank-list { max-height: 520px; overflow-x: hidden; padding-right: 4px; }

.rank-item-compact {
  display: grid;
  /* 🚀 关键：数字列 40px，头像列 48px，确保铭牌向左溢出也不会盖住数字 */
  grid-template-columns: 20px 30px 1fr;
  align-items: center;
  gap: 8px;
  padding: 6px 6px ; /* 增加上下 Padding 保护铭牌垂直空间 */
  border-radius: 8px;
  margin-bottom: 2px;
  transition: 0.2s;
}
.rank-item-compact:hover { background: #f0f3f8; }

.num { font-weight: 900; font-family: 'JetBrains Mono', monospace; color: #000000; font-size: 0.9rem; text-align: center; position: relative; z-index: 10; }
.num.gold { color: var(--md-primary); font-size: 1.1rem; }

.avatar-holder {
  position: relative;
  /* 🚀 关键：顶部留白 18px，让 UserAvatar 的 Badge 不会撞车 */
  margin-top: 18px; 
  display: flex;
  justify-content: center;
}

/* 🚀 缩放排行榜内的铭牌，更精致 */
.avatar-holder :deep(.title-badge) {
  font-size: 9px;
  padding: 1px 6px;
  top: -11px;
  max-width: 75px; 
}

.u-name { font-weight: 800; font-size: 0.8rem; color: #111; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.u-meta { font-size: 0.6rem; font-weight: 700; color: var(--md-primary); opacity: 0.7; }

.tag-chips { display: flex; flex-wrap: wrap; gap: 6px; }
.m-chip { background: #eff1f4; padding: 4px 10px; border-radius: 6px; font-size: 0.65rem; font-weight: 700; color: #555; cursor: pointer; }

/* 辅助样式 */
.custom-scroll::-webkit-scrollbar { width: 3px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #d1d4d9; border-radius: 10px; }
.md-linear-loader { width: 100%; height: 2px; background: #eee; overflow: hidden; position: relative; }
.md-linear-loader::after { content: ""; position: absolute; left: 0; top: 0; height: 100%; width: 50%; background: var(--md-primary); animation: move 1s infinite linear; }
@keyframes move { 0% { left: -50%; } 100% { left: 100%; } }

@media (max-width: 1300px) {
  .main-layout { grid-template-columns: 1fr; }
  .sidebar-info { display: none; }
}
</style>