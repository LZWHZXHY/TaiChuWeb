<template>
  <div class="video-review-panel">
    <div class="filter-bar cyber-box-light">
      <div class="filter-group">
        <button 
          v-for="s in statusOptions" 
          :key="s.value"
          class="filter-btn"
          :class="{ active: currentStatus === s.value }"
          @click="currentStatus = s.value"
        >
          {{ s.label }}
        </button>
      </div>
      <div class="search-box">
        <input type="text" placeholder="搜索影像标题/干员ID..." v-model="searchQuery" />
      </div>
    </div>

    <div class="review-grid custom-scroll">
      <div v-if="loading" class="loading-overlay">正在同步待审档案...</div>
      
      <div 
        v-for="video in filteredVideos" 
        :key="video.Id" 
        class="video-card cyber-box"
        :class="{ 'is-selected': selectedVideo?.Id === video.Id, 'is-banned': video.Status === 1 }"
      >
        <div class="card-thumb" @click="openPreview(video)">
          <img :src="video.CoverUrl" alt="cover" />
          <div class="thumb-overlay">
            <span class="play-icon">▶</span>
          </div>
          <div class="status-badge" v-if="video.Status === 1">已封禁</div>
          <div class="status-badge pending" v-if="video.Status === 2">待审核</div>
        </div>

        <div class="card-info">
          <h4 class="v-title">{{ video.Title }}</h4>
          <div class="v-meta">
            <span>👤 {{ video.AuthorName }}</span>
            <span>📅 {{ formatDate(video.CreatedAt) }}</span>
          </div>
        </div>

        <div class="card-actions">
          <button v-if="video.Status !== 0" class="op-btn approve" @click="handleAction(video.Id, 'approve')">通过 ARCHIVE</button>
          <button v-if="video.Status !== 1" class="op-btn reject" @click="handleAction(video.Id, 'ban')">封禁/驳回 BAN</button>
          <button class="op-btn delete" @click="handleAction(video.Id, 'delete')">物理抹除 DEL</button>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <div class="preview-modal" v-if="selectedVideo" @click.self="selectedVideo = null">
        <div class="modal-content cyber-box">
          <header class="modal-header">
            <h3>FILE_PREVIEW // {{ selectedVideo.Title }}</h3>
            <button class="close-btn" @click="selectedVideo = null">×</button>
          </header>
          <div class="video-player-box">
            <video :src="selectedVideo.VideoUrl" controls autoplay class="preview-video"></video>
          </div>
          <footer class="modal-footer">
             <button class="op-btn approve" @click="handleAction(selectedVideo.Id, 'approve')">确认归档 (通过)</button>
             <button class="op-btn reject" @click="handleAction(selectedVideo.Id, 'ban')">信号切断 (封禁)</button>
          </footer>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'

const loading = ref(false)
const videos = ref([])
const currentStatus = ref(2) 
const currentPage = ref(1)
const searchQuery = ref('')
const selectedVideo = ref(null)

const statusOptions = [
  { label: '全部 ALL', value: null },
  { label: '待审核 PENDING', value: 2 }, 
  { label: '已公开 PUBLIC', value: 0 },
  { label: '已封禁 BANNED', value: 1 }
]

const fetchVideos = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/admin/VideoManager/all-videos', {
      params: {
        page: currentPage.value,
        pageSize: 50,
        status: currentStatus.value 
      }
    })
    if (res.data.success) {
      videos.value = res.data.data
    }
  } catch (e) {
    console.error("档案同步失败", e)
  } finally {
    loading.value = false
  }
}

watch(currentStatus, () => {
  currentPage.value = 1
  fetchVideos()
})

const filteredVideos = computed(() => {
  return videos.value.filter(v => 
    v.Title.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

const openPreview = (video) => {
  selectedVideo.value = video
}

const handleAction = async (id, actionType) => {
  let url = '';
  let method = 'post';
  let confirmMsg = '';

  // ⚡ 严格对应后端 Controller 路由
  switch (actionType) {
    case 'approve':
      url = `/admin/VideoManager/${id}/approve`;
      confirmMsg = "确认解密该档案并向全站广播？";
      break;
    case 'ban':
      url = `/admin/VideoManager/${id}/ban`;
      confirmMsg = "确认切断该频段信号？(封禁/驳回)";
      break;
    case 'delete':
      url = `/admin/VideoManager/${id}/delete`;
      method = 'delete';
      confirmMsg = "警告：此操作将从数据库物理抹除档案，不可逆！";
      break;
  }

  if (!confirm(confirmMsg)) return;

  try {
    // 封禁操作需要 body，后端默认值是"违反社区准则"
    const res = actionType === 'ban' 
      ? await apiClient.post(url, "违规内容审核未通过", { headers: {'Content-Type': 'application/json'} })
      : await apiClient[method](url);

    if (res.data.success) {
      alert(res.data.message);
      selectedVideo.value = null;
      fetchVideos(); // 刷新列表
    }
  } catch (e) {
    alert("指令执行失败：权限不足或档案已被锁定");
  }
};

const formatDate = (d) => d ? new Date(d).toLocaleString() : '未知时间'
onMounted(fetchVideos)
</script>

<style scoped>
/* 保持原有赛博风格，新增部分状态样式 */
.video-review-panel { display: flex; flex-direction: column; gap: 20px; height: 100%; }
.filter-bar { display: flex; justify-content: space-between; align-items: center; padding: 15px; border: 2px solid #111; background: #fff; }
.filter-group { display: flex; gap: 10px; }
.filter-btn { background: #eee; border: 1px solid #ccc; padding: 6px 12px; font-family: 'JetBrains Mono'; font-size: 0.8rem; cursor: pointer; }
.filter-btn.active { background: #111; color: #fff; border-color: #111; }

.review-grid { flex: 1; display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; overflow-y: auto; }
.video-card { background: #fff; border: 2px solid #111; display: flex; flex-direction: column; position: relative; }
.video-card.is-banned { opacity: 0.7; filter: grayscale(0.5); }

.card-thumb { position: relative; aspect-ratio: 16/9; background: #000; cursor: pointer; }
.card-thumb img { width: 100%; height: 100%; object-fit: cover; }

.status-badge { position: absolute; top: 5px; right: 5px; background: #ff4757; color: #fff; padding: 2px 6px; font-size: 0.7rem; font-weight: bold; }
.status-badge.pending { background: #f1c40f; color: #000; }

.card-info { padding: 12px; flex: 1; }
.v-title { font-weight: 800; font-size: 0.9rem; margin-bottom: 8px; }
.v-meta { font-size: 0.75rem; color: #666; display: flex; flex-direction: column; }

.card-actions { display: grid; grid-template-columns: 1fr; border-top: 1px solid #eee; }
.op-btn { border: none; padding: 8px; font-family: 'JetBrains Mono'; font-size: 0.7rem; font-weight: 800; cursor: pointer; }
.op-btn.approve { color: #2ecc71; background: #f9fffb; border-bottom: 1px solid #eee; }
.op-btn.reject { color: #e67e22; background: #fffaf5; border-bottom: 1px solid #eee; }
.op-btn.delete { color: #ff4757; background: #fff5f5; }

.preview-modal { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 9999; display: flex; align-items: center; justify-content: center; }
.modal-content { background: #fff; width: 90%; max-width: 800px; border: 4px solid #111; }
.modal-header { background: #111; color: #fff; padding: 10px 20px; display: flex; justify-content: space-between; }
.video-player-box { background: #000; width: 100%; aspect-ratio: 16/9; }
.preview-video { width: 100%; height: 100%; }
.modal-footer { display: flex; border-top: 4px solid #111; }
.modal-footer .op-btn { flex: 1; padding: 15px; font-size: 0.9rem; }
</style>