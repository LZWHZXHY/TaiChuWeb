<template>
  <div class="video-detail-wrapper">
    <header class="detail-header">
      <button class="btn-back" @click="router.push('/media')">
        <span class="icon">←</span> 返回影音频段
      </button>
      <div class="header-status">
        <span class="pulse"></span> 
        <span class="status-text">档案实时解密 [ID: {{ id }}] // NODE_READY</span>
      </div>
    </header>

    <main class="detail-content" v-if="videoData">
      <div class="main-vision">
        
        <div class="player-wrapper">
          <div class="player-container-16-9 cyber-box">
            <video 
              ref="videoPlayer"
              class="real-video-player"
              controls
              autoplay
              playsinline
              :poster="videoData.CoverUrl"
              :src="videoData.VideoUrl"
              @timeupdate="handleTimeUpdate"
            >
              您的终端协议版本过低，无法解析此影像频段。
            </video>
            
            <div class="danmaku-layer" v-show="showDanmaku">
              <div 
                class="danmaku-item" 
                v-for="dm in activeDanmakus" 
                :key="dm.Id"
                :style="{ top: dm.TopPosition, color: dm.Color, fontSize: dm.FontSize + 'px' }"
              >
                {{ dm.Text }}
              </div>
            </div>

            <div class="player-hud hud-top-left">STREAM_SYNC ●</div>
            <div class="player-hud hud-top-right">REC_MODE: AUTO</div>
            <div class="player-hud hud-bottom-left">FRM_RATE: 60FPS</div>
            <div class="player-hud hud-bottom-right">CODEC: H.264 // {{ videoData.Duration }}</div>
          </div>
          
          <div class="danmaku-controls cyber-box-light">
            <div class="dm-switch-box" @click="showDanmaku = !showDanmaku" :class="{ off: !showDanmaku }">
              <div class="dm-icon">弹</div>
              <div class="dm-status">{{ showDanmaku ? '开启' : '关闭' }}</div>
            </div>
            <input 
              type="text" 
              v-model="newDanmaku" 
              @keyup.enter="sendDanmaku" 
              placeholder="在此时刻发射共振信号..." 
              class="dm-input" 
              :disabled="!showDanmaku"
            />
            <button class="dm-send-btn" @click="sendDanmaku" :disabled="!showDanmaku || !newDanmaku.trim()">
              发射
            </button>
          </div>
        </div>

        <div class="video-meta-section">
          <h1 class="v-title">{{ videoData.Title }}</h1>
          <div class="v-stats-row">
            <div class="v-stats-left">
              <span class="stat-item">👁 {{ videoData.Views }} 次访问</span>
              <span class="stat-item">🗓 归档于 {{ formatDate(videoData.CreatedAt) }}</span>
              <span class="tag-channel">{{ videoData.VideoType }}</span>
            </div>
            <div class="v-actions">
              <button class="action-btn"><span class="icon">👍</span> 赞同</button>
              <button class="action-btn"><span class="icon">⭐</span> 收藏</button>
              <button class="action-btn share" @click="copyLink"><span class="icon">🔗</span> 分享</button>
            </div>
          </div>
        </div>

        <div class="author-and-desc cyber-box-light">
          <div class="author-info">
            <div class="author-avatar-wrapper">
              <UserAvatar :userId="videoData.AuthorId" :showLevel="true" />
            </div>
            <div class="author-text">
              <div class="a-name">{{ videoData.Author?.Username || '未知干员' }}</div>
              <div class="a-role">太初寰宇 // 认证创作者</div>
            </div>
            <button class="btn-subscribe">+ 关注指令</button>
          </div>
          <div class="desc-text">
            {{ videoData.Description || '该档案未记录详细描述。' }}
          </div>
        </div>

        <div class="comments-section-wrapper">
          <UniversalComments targetType="Video" :targetId="id" />
        </div>
      </div>

      <aside class="side-vision">
        <h3 class="section-title">// 关联档案推荐</h3>
        <div class="related-list">
          <div 
            class="related-card" 
            v-for="rel in relatedVideos" 
            :key="rel.Id"
            @click="router.push(`/media/${rel.Id}`)"
          >
            <div class="r-cover">
              <img :src="rel.CoverUrl" alt="cover" />
              <span class="r-duration">{{ rel.Duration }}</span>
            </div>
            <div class="r-info">
              <h4 class="r-title">{{ rel.Title }}</h4>
              <span class="r-author">@{{ rel.Author?.Username || '匿名' }}</span>
            </div>
          </div>
        </div>
      </aside>
    </main>

    <main class="loading-state" v-else>
      <div class="loader"></div>
      <p class="loading-text">正在从太初节点提取数据...</p>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'
import UniversalComments from '@/GeneralComponents/UniversalComments.vue'

const props = defineProps<{ id: string }>()
const router = useRouter()

const videoPlayer = ref<HTMLVideoElement | null>(null)
const videoData = ref<any>(null)
const relatedVideos = ref<any[]>([])

// --- 弹幕状态核心 ---
const showDanmaku = ref(true)
const newDanmaku = ref('')
const allDanmakus = ref<any[]>([])    
const activeDanmakus = ref<any[]>([]) 
const consumedIds = new Set<number>() // ⚡ 使用原生 Set，不参与 Vue 响应式，极致性能

// --- 1. 获取数据 (加锁防止重复请求) ---
let loadingData = false
const fetchVideoData = async (videoId: string) => {
  if (loadingData) return
  loadingData = true
  try {
    const res = await apiClient.get(`/Video/${videoId}`)
    if (res.data.success) {
      videoData.value = res.data.data
      const dmRes = await apiClient.get(`/Video/${videoId}/danmaku`)
      if (dmRes.data.success) {
        // ⚡ 排序是关键
        allDanmakus.value = dmRes.data.data.sort((a, b) => a.TimeOffset - b.TimeOffset)
        resetSystem()
      }
      fetchRelatedVideos()
    }
  } catch (err) {
    console.error("档案解密失败")
  } finally {
    loadingData = false
  }
}

// --- 2. ⚡ 物理重置 ---
const resetSystem = () => {
  consumedIds.clear()
  activeDanmakus.value = []
  console.log("⚡ 物理重置完成：清空记录");
}

// --- 3. ⚡ 弹幕判断 (暴力判定补丁) ---
const handleTimeUpdate = () => {
  const v = videoPlayer.value
  if (!v || !showDanmaku.value || v.seeking) return
  
  const currentTime = v.currentTime
  
  // ⚡ 寻找当前时间 ±0.5s 内的弹幕
  const hits = allDanmakus.value.filter(dm => {
    return Math.abs(dm.TimeOffset - currentTime) < 0.5 && !consumedIds.has(dm.Id)
  })

  if (hits.length > 0) {
    hits.forEach(h => {
      consumedIds.add(h.Id)
      activeDanmakus.value.push(h)
      
      // 7秒后物理移除渲染
      setTimeout(() => {
        activeDanmakus.value = activeDanmakus.value.filter(a => a.Id !== h.Id)
      }, 7000)
    })
  }
}

// --- 4. ⚡ 解决拖动重播的核心：原生事件监听 ---
const initVideoEvents = () => {
  const v = videoPlayer.value
  if (!v) return

  // 只要进度条动了，不管往前还是往后，瞬间重置
  v.addEventListener('seeking', resetSystem)
  
  // 拖动结束瞬间强制检查一次
  v.addEventListener('seeked', () => {
    resetSystem()
    handleTimeUpdate()
  })

  // 如果从 0 开始播，强制重置
  v.addEventListener('play', () => {
    if (v.currentTime < 1) resetSystem()
  })
}

// --- 5. 发送弹幕 ---
const sendDanmaku = async () => {
  if (!newDanmaku.value.trim() || !videoPlayer.value) return
  const v = videoPlayer.value
  const payload = {
    VideoId: Number(props.id),
    Text: newDanmaku.value,
    TimeOffset: v.currentTime,
    Color: '#00A3FF',
    FontSize: 20,
    TopPosition: Math.floor(Math.random() * 75) + 5 + '%'
  }

  try {
    const res = await apiClient.post('/Video/danmaku/send', payload)
    if (res.data.success) {
      const serverDm = res.data.data
      consumedIds.add(serverDm.Id) 
      activeDanmakus.value.push(serverDm)
      allDanmakus.value.push(serverDm)
      newDanmaku.value = ''
    }
  } catch (err) {}
}

const fetchRelatedVideos = async () => {
  const res = await apiClient.get('/Video/list', { params: { pageSize: 6 } })
  if (res.data.success) relatedVideos.value = res.data.data.filter((v: any) => v.Id != props.id)
}

const formatDate = (d: string) => d ? new Date(d).toLocaleDateString() : '--'
const copyLink = () => { navigator.clipboard.writeText(window.location.href).then(() => alert('档案链接已复制')) }

// --- 6. 监控与生命周期 ---
watch(() => props.id, (newId) => {
  if (newId) fetchVideoData(newId)
})

onMounted(() => {
  fetchVideoData(props.id)
  nextTick(() => {
    initVideoEvents()
  })
})

onUnmounted(() => {
  const v = videoPlayer.value
  if (v) v.removeEventListener('seeking', resetSystem)
  resetSystem()
})
</script>

<style scoped>
/* 核心包装器 */
.video-detail-wrapper {
  padding: 0; height: 100%; display: flex; flex-direction: column;
  background: #F4F5F7; font-family: 'Inter', 'JetBrains Mono', sans-serif; overflow-y: auto;
}

/* 顶部 Header */
.detail-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 40px; background: #fff; border-bottom: 2px solid #111;
  position: sticky; top: 0; z-index: 100;
}
.btn-back { background: transparent; border: none; font-weight: 800; font-size: 14px; color: #111; cursor: pointer; display: flex; align-items: center; gap: 8px; }
.btn-back:hover { color: #00A3FF; }
.header-status { font-family: 'JetBrains Mono'; font-size: 12px; color: #666; display: flex; align-items: center; gap: 8px; }
.pulse { width: 8px; height: 8px; background: #36B37E; border-radius: 50%; box-shadow: 0 0 8px #36B37E; animation: blink 1.5s infinite; }

/* 布局控制 */
.detail-content { display: flex; gap: 32px; padding: 32px 40px; max-width: 1600px; margin: 0 auto; width: 100%; box-sizing: border-box; }
.main-vision { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 24px; }
.side-vision { width: 380px; flex-shrink: 0; }

/* ⚡ 强制 16:9 播放器样式 */
.player-wrapper { display: flex; flex-direction: column; border: 2px solid #111; border-radius: 8px; overflow: hidden; box-shadow: 8px 8px 0 rgba(0,0,0,0.05); }
.player-container-16-9 {
  width: 100%; aspect-ratio: 16 / 9; background: #000; position: relative; overflow: hidden;
  display: flex; align-items: center; justify-content: center;
}
.real-video-player { width: 100%; height: 100%; object-fit: contain; outline: none; }

/* HUD 装饰样式 */
.player-hud { position: absolute; font-family: 'JetBrains Mono'; font-size: 11px; font-weight: 800; color: rgba(255,255,255,0.4); z-index: 5; pointer-events: none; }
.hud-top-left { top: 16px; left: 16px; color: #ff4757; }
.hud-top-right { top: 16px; right: 16px; }
.hud-bottom-left { bottom: 16px; left: 16px; }
.hud-bottom-right { bottom: 16px; right: 16px; background: rgba(0,0,0,0.6); padding: 2px 6px; border-radius: 2px; }

/* 弹幕层与动画 */
.danmaku-layer { position: absolute; inset: 0; z-index: 3; pointer-events: none; overflow: hidden; }
.danmaku-item {
  position: absolute; right: -100%; white-space: nowrap; font-weight: 900;
  text-shadow: 1px 1px 2px rgba(0,0,0,0.8);
  animation: scroll-left 8s linear forwards; 
}
@keyframes scroll-left {
  0% { left: 100%; transform: translateX(0); }
  100% { left: 0; transform: translateX(-100%); }
}

/* 弹幕发射台样式 */
.danmaku-controls { display: flex; height: 52px; background: #fff; align-items: center; padding: 0 20px; gap: 15px; border-top: 2px solid #111; }
.dm-switch-box { display: flex; align-items: center; gap: 8px; cursor: pointer; user-select: none; }
.dm-icon { width: 26px; height: 26px; background: #00A3FF; color: #fff; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 900; }
.dm-switch-box.off .dm-icon { background: #DFE1E6; color: #999; }
.dm-status { font-size: 12px; font-weight: 800; color: #111; }
.dm-input { flex: 1; border: none; background: #F4F5F7; height: 36px; border-radius: 18px; padding: 0 18px; font-size: 14px; outline: none; font-family: inherit; }
.dm-send-btn { background: #111; color: #fff; border: none; height: 36px; padding: 0 24px; border-radius: 18px; font-weight: 800; font-size: 13px; cursor: pointer; transition: 0.2s; }
.dm-send-btn:hover:not(:disabled) { background: #00A3FF; }
.dm-send-btn:disabled { background: #DFE1E6; color: #999; cursor: not-allowed; }

/* 视频标题区 */
.v-title { font-size: 26px; font-weight: 900; color: #111; margin: 0 0 16px 0; line-height: 1.3; }
.v-stats-row { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #DFE1E6; padding-bottom: 18px; }
.v-stats-left { display: flex; align-items: center; gap: 16px; font-size: 13px; color: #666; font-weight: 600; }
.tag-channel { background: #eee; padding: 2px 10px; border-radius: 4px; font-family: 'JetBrains Mono'; font-size: 11px; }

/* 交互按钮 */
.v-actions { display: flex; gap: 12px; }
.action-btn { background: #fff; border: 2px solid #DFE1E6; padding: 8px 16px; font-weight: 800; font-size: 13px; cursor: pointer; border-radius: 4px; display: flex; align-items: center; gap: 8px; transition: 0.2s; }
.action-btn:hover { background: #f0f0f0; border-color: #111; }
.action-btn.share { background: #111; color: #fff; border-color: #111; }
.action-btn.share:hover { background: #00A3FF; border-color: #00A3FF; }

/* 作者与简介 */
.author-and-desc { background: #fff; border: 2px solid #DFE1E6; border-radius: 12px; padding: 24px; }
.author-info { display: flex; align-items: center; gap: 16px; margin-bottom: 20px; }
.author-avatar-wrapper { width: 52px; height: 52px; flex-shrink: 0; }
.a-name { font-size: 17px; font-weight: 900; color: #111; }
.a-role { font-size: 12px; color: #888; font-weight: 600; margin-top: 2px; }
.btn-subscribe { background: transparent; border: 2px solid #00A3FF; color: #00A3FF; padding: 6px 16px; border-radius: 20px; font-weight: 800; font-size: 13px; cursor: pointer; transition: 0.2s; }
.btn-subscribe:hover { background: #00A3FF; color: #fff; }
.desc-text { font-size: 15px; line-height: 1.7; color: #333; white-space: pre-wrap; padding-top: 20px; border-top: 1px dashed #DFE1E6; }

/* 右侧推荐列表 */
.side-vision .section-title { font-size: 15px; font-weight: 900; margin-bottom: 20px; border-left: 4px solid #00A3FF; padding-left: 12px; }
.related-list { display: flex; flex-direction: column; gap: 16px; }
.related-card { display: flex; gap: 14px; cursor: pointer; transition: 0.2s; }
.related-card:hover .r-title { color: #00A3FF; }
.r-cover { position: relative; width: 160px; aspect-ratio: 16 / 9; border-radius: 6px; overflow: hidden; border: 1px solid #111; flex-shrink: 0; }
.r-cover img { width: 100%; height: 100%; object-fit: cover; }
.r-duration { position: absolute; bottom: 6px; right: 6px; background: rgba(0,0,0,0.8); color: #fff; font-size: 10px; font-family: 'JetBrains Mono'; padding: 2px 4px; border-radius: 2px; }
.r-info { display: flex; flex-direction: column; overflow: hidden; }
.r-title { margin: 0 0 8px 0; font-size: 13px; font-weight: 800; color: #111; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.r-author { font-size: 11px; color: #666; font-weight: 600; }

/* 加载动画 */
.loading-state { flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: center; gap: 16px; color: #666; }
.loader { width: 36px; height: 36px; border: 4px solid #DFE1E6; border-top-color: #00A3FF; border-radius: 50%; animation: spin 1s linear infinite; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }
@keyframes spin { to { transform: rotate(360deg); } }
</style>