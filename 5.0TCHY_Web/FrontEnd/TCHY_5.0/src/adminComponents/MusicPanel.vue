<template>
  <div class="music-audit-terminal">
    <div class="terminal-header">
      <span class="status-light"></span>
      <h2 class="title">>> 轨道电台 // 弹药审核终端</h2>
      <div class="stats">待处理: {{ pendingList.length }}</div>
    </div>

    <div v-if="previewUrl" class="preview-player">
      <div class="player-info">正在试听: {{ previewTitle }}</div>
      <audio ref="previewAudio" :src="previewUrl" controls autoplay class="cyber-audio"></audio>
      <button class="close-preview" @click="stopPreview">关闭预览</button>
    </div>

    <div class="audit-list" v-loading="loading">
      <div v-if="pendingList.length === 0" class="empty-hint">
        [ SYSTEM ] 当前无待处理的音频请求。
      </div>

      <div v-for="track in pendingList" :key="track.Id" class="audit-card">
        <div class="track-meta">
          <div class="main-info">
            <span class="track-id">#{{ track.Id }}</span>
            <span class="track-title">{{ track.Title }}</span>
          </div>
          <div class="sub-info">
            <span>ARTIST: {{ track.Artist }}</span>
            <span>DURATION: {{ track.DurationSeconds }}s</span>
            <span>UPLOADER: {{ track.UploaderName }}</span>
          </div>
        </div>

        <div class="action-zone">
          <button class="btn btn-preview" @click="startPreview(track)">
            [ PREVIEW ]
          </button>
          
          <button class="btn btn-approve" @click="handleApprove(track.Id)">
            [ APPROVE // 准许发射 ]
          </button>

          <button class="btn btn-reject" @click="handleDelete(track.Id)">
            [ PURGE // 清除 ]
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'

const pendingList = ref([])
const loading = ref(false)
const previewUrl = ref('')
const previewTitle = ref('')
const previewAudio = ref(null)

// --- 1. 获取待审核列表 ---
const fetchPendingTracks = async () => {
  loading.value = true
  try {
    // ⚡ 对应我们在 SongController 中新增的接口
    const res = await apiClient.get('/Song/pending')
    if (res.data.success) {
      pendingList.value = res.data.data
    }
  } catch (err) {
    console.error('>> [SYS_ERR] 无法获取审核列表:', err)
  } finally {
    loading.value = false
  }
}

// --- 2. 试听控制 ---
const startPreview = (track) => {
  previewUrl.value = track.Url
  previewTitle.value = track.Title
}

const stopPreview = () => {
  previewUrl.value = ''
  previewTitle.value = ''
}

// --- 3. 批准发射 ---
const handleApprove = async (id) => {
  if (!confirm(`确认将该曲目并入全网电台序列？`)) return

  try {
    // ⚡ 调用批准接口
    const res = await apiClient.post(`/Song/approve/${id}`)
    if (res.data.success) {
      alert('>> [SYS_OK] 曲目已成功并入轨道电台。')
      // 刷新列表
      fetchPendingTracks()
      if (previewUrl.value) stopPreview()
    }
  } catch (err) {
    alert('>> [ERROR] 授权失败：' + (err.response?.data?.message || '身份验证未通过'))
  }
}

// --- 4. 彻底删除 ---
const handleDelete = async (id) => {
  if (!confirm(`警告：此操作将物理销毁该音频记录，是否继续？`)) return

  try {
    const res = await apiClient.delete(`/Song/${id}`)
    if (res.data.success) {
      fetchPendingTracks()
    }
  } catch (err) {
    console.error('删除失败:', err)
  }
}

onMounted(() => {
  fetchPendingTracks()
})
</script>

<style scoped>
.music-audit-terminal {
  background: #f4f1ea;
  border: 2px solid #111;
  padding: 20px;
  font-family: 'JetBrains Mono', monospace;
}

.terminal-header {
  display: flex;
  align-items: center;
  gap: 15px;
  border-bottom: 2px solid #d92323;
  padding-bottom: 10px;
  margin-bottom: 20px;
}

.status-light {
  width: 12px;
  height: 12px;
  background: #d92323;
  border-radius: 50%;
  box-shadow: 0 0 8px #d92323;
}

.title {
  margin: 0;
  font-size: 16px;
  color: #111;
  flex-grow: 1;
}

.stats {
  font-size: 12px;
  background: #111;
  color: #fff;
  padding: 2px 8px;
}

/* 预览播放器 */
.preview-player {
  background: #111;
  color: #00ff00;
  padding: 15px;
  margin-bottom: 20px;
  border-left: 4px solid #00ff00;
}

.player-info {
  font-size: 11px;
  margin-bottom: 8px;
}

.cyber-audio {
  width: 100%;
  height: 30px;
}

/* 审核卡片 */
.audit-card {
  background: #fff;
  border: 1px solid #111;
  padding: 15px;
  margin-bottom: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: all 0.2s;
}

.audit-card:hover {
  transform: translateX(5px);
  box-shadow: -5px 0 0 #d92323;
}

.track-id {
  color: #999;
  margin-right: 10px;
}

.track-title {
  font-weight: bold;
  font-size: 15px;
}

.sub-info {
  display: flex;
  gap: 15px;
  font-size: 10px;
  color: #666;
  margin-top: 5px;
}

.action-zone {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 8px 12px;
  border: 1px solid #111;
  background: transparent;
  cursor: pointer;
  font-family: inherit;
  font-size: 11px;
  font-weight: bold;
  transition: all 0.2s;
}

.btn-preview:hover { background: #111; color: #fff; }
.btn-approve { background: #d92323; color: #fff; border-color: #d92323; }
.btn-approve:hover { background: #111; border-color: #111; }
.btn-reject:hover { background: #000; color: #fff; }

.empty-hint {
  text-align: center;
  padding: 40px;
  color: #999;
  border: 1px dashed #ccc;
}
</style>