<template>
  <div class="media-matrix-wrapper">
   

    <main class="matrix-content">
      <section class="hero-broadcast cyber-box" v-if="featuredVideo && !searchQuery">
        <div class="hero-backdrop" :style="{ backgroundImage: `url(${featuredVideo.coverUrl})` }"></div>
        <div class="hero-overlay">
          <div class="hero-badge">BROADCAST_OVERRIDE // 全频段广播</div>
          <h1 class="hero-title">{{ featuredVideo.title }}</h1>
          <p class="hero-desc">{{ featuredVideo.description }}</p>
          <div class="hero-actions">
            <button class="btn-play" @click="goToVideo(featuredVideo.id)">
              <span class="play-triangle">▶</span> 立即播放
            </button>
            <button class="btn-outline" @click="goToVideo(featuredVideo.id)">查阅详情</button>
          </div>
        </div>
      </section>

      <div v-if="loading" class="matrix-loading">
        <div class="loader"></div>
        <span>正在接入数据链路...</span>
      </div>

      <section class="video-grid" v-else-if="videos.length > 0">
        <div 
          class="video-card" 
          v-for="video in videos" 
          :key="video.id"
          @click="goToVideo(video.id)"
        >
          <div class="cover-wrapper">
            <img :src="video.coverUrl" :alt="video.title" class="video-cover" />
            <span class="video-duration">{{ video.duration }}</span>
            <div class="hover-play-indicator">▶</div>
          </div>
          
          <div class="video-info">
            <div class="author-avatar">
              <span>{{ video.authorName?.charAt(0) || '?' }}</span> 
            </div>
            <div class="info-text">
              <h3 class="v-title" :title="video.title">{{ video.title }}</h3>
              <div class="v-meta">
                <span class="v-author">@{{ video.authorName }}</span>
                <span class="v-stats">
                  👁 {{ video.views }} · 🗓 {{ formatDate(video.createdAt) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <div v-else class="empty-state">
        <p>[ 信号中断 : 未找到相关影像档案 ]</p>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'

const router = useRouter()
const loading = ref(false)
const searchQuery = ref('')
const videos = ref<any[]>([])

// 自动提取第一个视频作为精选
const featuredVideo = computed(() => videos.value[0] || null)

// --- 数据抓取 ---
// --- 核心数据抓取 ---
const fetchVideos = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/Video/list', {
      params: { page: 1, pageSize: 30, search: searchQuery.value }
    })
    
    if (res.data.success) {
      // ⚡ 注意：这里要对应你 JSON 里的首字母大写字段
      videos.value = res.data.data.map((v) => ({
        id: v.Id,                   // 对应 Id
        title: v.Title,             // 对应 Title
        description: v.Description, // 对应 Description
        authorName: v.Author?.Username || '未知干员', // 对应 Author.Username
        views: v.Views,             // 对应 Views
        duration: v.Duration,       // 对应 Duration
        createdAt: v.CreatedAt,     // 对应 CreatedAt
        coverUrl: v.CoverUrl        // 对应 CoverUrl
      }))
    }
  } catch (error) {
    console.error("信号接入失败:", error)
  } finally {
    loading.value = false
  }
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}

const goToVideo = (id: number | string) => {
  router.push(`/media/${id}`)
}

const triggerUpload = () => {
  router.push('/create/video-upload')
}

onMounted(() => {
  fetchVideos()
})
</script>

<style scoped>
/* 还原原本简洁、高对比度的工业风格 */
.media-matrix-wrapper {
  padding: 24px 32px;
  height: 100%;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  background: #F4F5F7;
  font-family: 'Inter', 'JetBrains Mono', sans-serif;
  overflow-y: auto;
}

.matrix-header {
  display: flex; justify-content: space-between; align-items: flex-end;
  margin-bottom: 32px; padding-bottom: 16px; border-bottom: 2px solid #111;
  flex-shrink: 0;
}
.title { font-size: 24px; font-weight: 900; color: #111; margin: 0 0 8px 0; }
.subtitle { font-size: 13px; color: #666; font-weight: 700; margin: 0; }

.header-actions { display: flex; gap: 16px; align-items: center; }
.cyber-input-group { display: flex; align-items: center; border: 2px solid #111; background: #fff; padding: 4px 12px; border-radius: 4px; }
.cyber-input-group input { border: none; outline: none; background: transparent; font-family: inherit; font-size: 13px; font-weight: 600; width: 200px; margin-left: 8px;}

.btn-primary { background: #111; color: #fff; border: 2px solid #111; padding: 8px 20px; font-weight: 800; font-size: 13px; cursor: pointer; transition: 0.2s; border-radius: 4px; }
.btn-primary:hover { background: #00A3FF; border-color: #00A3FF; }

.matrix-content { flex: 1; display: flex; flex-direction: column; gap: 32px; }

.hero-broadcast {
  position: relative; height: 360px; border-radius: 8px; overflow: hidden;
  border: 2px solid #111; box-shadow: 8px 8px 0 rgba(0,0,0,0.05);
}
.hero-backdrop { position: absolute; inset: 0; background-size: cover; background-position: center; transition: transform 10s linear; }
.hero-overlay {
  position: absolute; inset: 0; background: linear-gradient(to right, rgba(17,17,17,0.9) 0%, rgba(17,17,17,0.4) 50%, transparent 100%);
  padding: 48px; display: flex; flex-direction: column; justify-content: center;
}
.hero-badge { background: #00A3FF; color: #fff; font-size: 11px; font-weight: 900; padding: 4px 8px; margin-bottom: 16px; width: fit-content;}
.hero-title { color: #fff; font-size: 32px; font-weight: 900; margin: 0 0 16px 0; max-width: 600px; line-height: 1.2; }
.hero-desc { color: #ccc; font-size: 14px; line-height: 1.6; max-width: 500px; margin: 0 0 32px 0; }

.btn-play { background: #fff; color: #111; border: none; padding: 10px 24px; font-weight: 900; font-size: 14px; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 8px;}
.btn-play:hover { background: #00A3FF; color: #fff; }
.btn-outline { background: transparent; color: #fff; border: 2px solid #fff; padding: 10px 24px; font-weight: 800; font-size: 14px; cursor: pointer; transition: 0.2s; }

.video-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 32px 24px;
}
.video-card { display: flex; flex-direction: column; gap: 12px; cursor: pointer; }
.cover-wrapper {
  position: relative; width: 100%; aspect-ratio: 16 / 9; border-radius: 6px; overflow: hidden;
  border: 2px solid #111; box-sizing: border-box; background: #000;
}
.video-cover { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s ease; }
.video-duration {
  position: absolute; bottom: 8px; right: 8px; background: rgba(17,17,17,0.8); color: #fff;
  font-size: 11px; font-weight: 800; padding: 2px 6px; border-radius: 4px; font-family: 'JetBrains Mono';
}
.hover-play-indicator {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%) scale(0.8);
  width: 48px; height: 48px; background: rgba(0, 163, 255, 0.9); color: #fff; border-radius: 50%;
  display: flex; justify-content: center; align-items: center; font-size: 20px;
  opacity: 0; transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1); backdrop-filter: blur(4px);
}
.video-card:hover .hover-play-indicator { opacity: 1; transform: translate(-50%, -50%) scale(1); }

.video-info { display: flex; gap: 12px; align-items: flex-start; }
.author-avatar {
  width: 36px; height: 36px; border-radius: 50%; background: #111; color: #fff;
  display: flex; justify-content: center; align-items: center; font-weight: 800; flex-shrink: 0;
  border: 2px solid #DFE1E6;
}
.v-title { margin: 0; font-size: 14px; font-weight: 800; color: #111; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.v-meta { font-size: 11px; color: #666; font-weight: 600; display: flex; flex-direction: column; gap: 2px; }

.matrix-loading { display: flex; flex-direction: column; align-items: center; padding: 100px; gap: 20px; color: #666; }
.loader { width: 40px; height: 40px; border: 4px solid #DFE1E6; border-top-color: #00A3FF; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.empty-state { text-align: center; padding: 100px; color: #999; border: 2px dashed #DFE1E6; border-radius: 8px; }
</style>