<template>
  <div class="gallery-industrial">
    <div class="gallery-control-bar">
      <div class="header-title-block">
        <h2 class="giant-text">ARTWORKS</h2>
        <span class="sub-text">VISUAL_DB // 视觉库</span>
      </div>

      <div class="segment-button-group cyber-group">
        <div
          class="button-segment"
          :class="{ active: selectedSegment === 1, hoverable: true }"
          @click="handleSegmentClick(1)"
        >
          <span class="seg-icon">⚡</span> DYNAMIC
        </div>
        <div
          class="button-segment"
          :class="{ active: selectedSegment === 2, hoverable: true }"
          @click="handleSegmentClick(2)"
        >
          <span class="seg-icon">∞</span> ALL_TYPE
        </div>
        <div
          class="button-segment"
          :class="{ active: selectedSegment === 3, hoverable: true }"
          @click="handleSegmentClick(3)"
        >
          <span class="seg-icon">■</span> STATIC
        </div>
      </div>

      <button class="cyber-upload-btn" @click="showUploadModal = true">
        <span class="icon">+</span> UPLOAD_DATA
      </button>
    </div>

    <div class="gallery-body">
      
      <main class="gallery-main">
        <div 
          id="gallery-scroller"
          class="gallery-scroll-container custom-scroll" 
          ref="mainScroll" 
          @scroll="handleScroll"
        >
          <div class="gallery-content-wrapper">
            
            <div v-if="loading && totalCount === 0" class="loading-state">
              <div class="spinner"></div>
              <span>[ DOWNLOADING_NEURAL_DATA... ]</span>
            </div>

            <div v-else class="gallery-waterfall">
              <div 
                v-for="(col, colIndex) in waterfallColumns" 
                :key="colIndex" 
                class="waterfall-column"
              >
                <div 
                  v-for="img in col" 
                  :key="img.id" 
                  class="cyber-art-card" 
                  @click="openLightbox(img)"
                >
                  <div class="card-frame">
                    <div class="img-box">
                      <img :src="upgradeUrlToHttps(img.imageUrlFull || img.url)" @error="handleImgError" loading="lazy" />
                      <div class="scan-line"></div>
                    </div>
                    <div class="card-info">
                      <h3 class="art-title">{{ img.title }}</h3>
                      <div class="art-meta-row">
                        <span class="author">BY: {{ img.authorName || img.author }}</span>
                        <span class="likes">♥ {{ img.likes }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="corner-dec"></div>
                </div>
              </div>
            </div>

            <div class="list-footer">
              <div v-if="loading && totalCount > 0" class="loading-text">
                <span class="blink">>> SYNCING_DATA_STREAM...</span>
              </div>
              <div v-if="!hasMore && totalCount > 0" class="end-marker">
                // END_OF_ARCHIVE //
              </div>
              <div v-if="!loading && totalCount === 0" class="empty-state">
                [ NO_DATA_FOUND ]
              </div>
            </div>
            
          </div>
        </div>
      </main>

      <aside class="gallery-sidebar custom-scroll">
        <div class="cyber-panel">
          <div class="panel-header">
            <span class="deco">#</span> HASH_TAGS
          </div>
          <div class="tag-matrix">
            <span class="cyber-tag">#太初设定集</span>
            <span class="cyber-tag">#二创插画</span>
            <span class="cyber-tag">#火柴人</span>
            <span class="cyber-tag">#概念设计</span>
            <span class="cyber-tag red-mode">#赛博朋克</span>
            <span class="cyber-tag">#壁纸</span>
          </div>
        </div>

        <div class="cyber-panel leaderboard-panel">
          <div class="panel-header">
            <span class="deco">🏆</span> HIGH_SCORES
          </div>
          <div class="rank-list">
            <div v-for="(user, idx) in leaderboard" :key="user.UploaderId" class="rank-item">
              <div class="rank-idx" :class="'top-' + (idx + 1)">{{ padZero(idx + 1) }}</div>
              <div class="rank-info">
                <div class="r-name">{{ user.Name }}</div>
                <div class="r-score">LIKES: <span class="val">{{ user.TotalLikes }}</span></div>
              </div>
            </div>
            <div v-if="leaderboard.length === 0" class="empty-rank">
              [ WAITING_FOR_PLAYERS... ]
            </div>
          </div>
        </div>
      </aside>
    </div>

    <Teleport to="body">
      <Transition name="glitch-fade">
        <div v-if="showUploadModal" class="cyber-modal-overlay" @click.self="showUploadModal = false">
          <div class="cyber-modal-window upload-mode">
            <div class="modal-header">
              <div class="header-left">
                <span class="status-light blink"></span>
                <span class="title">DATA_INJECTION // 投稿终端</span>
              </div>
              <button class="close-btn" @click="showUploadModal = false">
                ABORT [ESC]
              </button>
            </div>

            <div class="modal-body custom-scroll">
              <div class="upload-section">
                <div 
                  class="upload-zone" 
                  @click="triggerFileSelect" 
                  :class="{ 'has-file': uploadForm.previewUrl, 'is-dragover': isDragOver }"
                  @dragover.prevent="isDragOver = true"
                  @dragleave.prevent="isDragOver = false"
                  @drop.prevent="handleDrop"
                >
                  <input type="file" ref="fileInput" accept="image/*" @change="handleFileChange" style="display:none" />
                  
                  <div v-if="uploadForm.previewUrl" class="preview-wrap">
                    <img :src="uploadForm.previewUrl" />
                    <div class="reupload-overlay">
                      <span class="btn-replace">REPLACE_SOURCE</span>
                    </div>
                    <div class="file-meta">
                      FILE: {{ uploadForm.file?.name || 'UNKNOWN' }}
                    </div>
                  </div>

                  <div v-else class="placeholder">
                    <div class="scan-grid"></div>
                    <div class="center-content">
                      <div class="icon-frame"><span class="plus">+</span></div>
                      <div class="text-group">
                        <p class="main-tip">DRAG & DROP or CLICK</p>
                        <span class="sub-tip">SUPPORT: JPG / PNG / WEBP (MAX 20MB)</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-section">
                <div class="cyber-input-group">
                  <div class="label-chip">TITLE / 标题</div>
                  <input type="text" v-model="uploadForm.title" placeholder="请输入作品标题..." class="cyber-input" maxlength="50" />
                  <div class="input-line"></div>
                </div>
                <div class="cyber-input-group">
                  <div class="label-chip">DESCRIPTION / 描述</div>
                  <textarea v-model="uploadForm.desc" placeholder="输入作品背后的故事或设定..." rows="3" class="cyber-input textarea"></textarea>
                  <div class="input-line"></div>
                </div>
                <div class="cyber-input-group">
                  <div class="label-chip">AUTHOR / 署名</div>
                  <input type="text" v-model="uploadForm.authorName" placeholder="默认使用当前用户名" class="cyber-input" />
                  <div class="input-line"></div>
                </div>
              </div>

              <div class="modal-footer">
                <div class="status-display">
                  STATUS: <span :class="isUploading ? 'busy' : 'ready'">{{ isUploading ? 'UPLOADING...' : 'READY' }}</span>
                </div>
                <button class="execute-btn" @click="submitArtwork" :disabled="isUploading">
                  <span class="btn-deco">>>></span>
                  <span class="btn-text">{{ isUploading ? 'TRANSMITTING...' : 'INITIATE_UPLOAD' }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <ArtWorkDetail 
      :visible="!!lightboxImg" 
      :artwork="lightboxImg || {}"
      @close="lightboxImg = null"
      @like="handleLike"
    />

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, nextTick } from 'vue'
import ArtWorkDetail from './Components/ArtWorkDetail.vue'
import apiClient from '@/utils/api'

const emit = defineEmits(['refresh-stats'])

// --- 状态定义 ---
const selectedSegment = ref(2)
const waterfallColumns = ref([[], [], [], []]) 
const loading = ref(false)
const nextCursor = ref(null)
const hasMore = ref(true)
const mainScroll = ref(null) // 绑定到新的滚动容器
const leaderboard = ref([])

// 弹窗相关
const showUploadModal = ref(false)
const isUploading = ref(false)
const isDragOver = ref(false) 
const fileInput = ref(null)
const uploadForm = reactive({ title: '', desc: '', authorName: '', file: null, previewUrl: '' })
const lightboxImg = ref(null)

const totalCount = computed(() => waterfallColumns.value.reduce((acc, col) => acc + col.length, 0))

// --- 方法 ---

const handleSegmentClick = (segment) => {
  if (selectedSegment.value === segment) return
  selectedSegment.value = segment
  
  // 重置数据
  waterfallColumns.value = [[], [], [], []]
  nextCursor.value = null
  hasMore.value = true
  
  // 滚回顶部
  if (mainScroll.value) {
    mainScroll.value.scrollTop = 0
  }
  
  fetchArtworks(true)
}

const upgradeUrlToHttps = (url) => {
  if (!url) return ''
  if (url.startsWith('/')) return url
  return url.replace('http://', 'https://')
}

// 核心：加载列表
// 修改 fetchArtworks 里的分发逻辑
const fetchArtworks = async (isRefresh = false) => {
  if (loading.value) return;
  if (!isRefresh && !hasMore.value) return;

  loading.value = true;
  try {
    const params = { 
      pageSize: 20, 
      sort: 'new', 
      cursor: isRefresh ? null : nextCursor.value, 
      type: selectedSegment.value 
    };
    
    const res = await apiClient.get('/Drawing/list', { params });
    
    if (res.data.success) {
      const { items, nextCursor: newCursor, hasMore: more } = res.data.data;
      if (isRefresh) waterfallColumns.value = [[], [], [], []];

      // 修复 BUG 的关键：不要简单的 (i % 4)
      // 在“综合”模式下，按照每列现有的高度平衡插入
      items.forEach((item) => {
        // 找到当前四列中，内容最少（最短）的那一列
        let shortestIdx = 0;
        let minLen = waterfallColumns.value[0].length;
        for (let i = 1; i < 4; i++) {
          if (waterfallColumns.value[i].length < minLen) {
            minLen = waterfallColumns.value[i].length;
            shortestIdx = i;
          }
        }
        waterfallColumns.value[shortestIdx].push(item);
      });

      nextCursor.value = newCursor;
      hasMore.value = more;
    }
  } finally {
    loading.value = false;
  }
};

// 核心：滚动监听 (无限加载)
const handleScroll = (e) => {
  const { scrollTop, scrollHeight, clientHeight } = e.target
  
  // 调试日志：如果发现不触发，打开控制台查看数值
  // console.log(`Top: ${scrollTop}, H: ${clientHeight}, Total: ${scrollHeight}`)
  
  // 距离底部 150px 时加载
  if (scrollTop + clientHeight >= scrollHeight - 150) {
    fetchArtworks(false)
  }
}

const fetchLeaderboard = async () => {
  try {
    const res = await apiClient.get('/Drawing/leaderboard?limit=15')
    if (res.data.success) leaderboard.value = res.data.data
  } catch (error) {}
}

const handleLike = async (img) => {
  if (!img) return
  const originalLiked = img.isLiked
  const originalLikes = img.likes || 0
  img.isLiked = !originalLiked
  img.likes = originalLiked ? originalLikes - 1 : originalLikes + 1
  try {
    const res = await apiClient.post(`/Drawing/like/${img.id}`)
    if (res.data.success) {
      img.likes = res.data.likes; img.isLiked = res.data.isLiked
    } else {
      img.isLiked = originalLiked; img.likes = originalLikes
    }
  } catch (error) {
    img.isLiked = originalLiked; img.likes = originalLikes
  }
}

// 上传相关
const triggerFileSelect = () => { fileInput.value.click() }
const handleFileChange = (e) => { processFile(e.target.files[0]) }
const handleDrop = (e) => { isDragOver.value = false; processFile(e.dataTransfer.files[0]) }
const processFile = (file) => {
  if (!file) return
  if (!file.type.startsWith('image/')) return alert("INVALID_FILE_TYPE")
  if (file.size > 20 * 1024 * 1024) return alert("FILE_TOO_LARGE (MAX 20MB)")
  uploadForm.file = file
  uploadForm.previewUrl = URL.createObjectURL(file)
}

const submitArtwork = async () => {
  if (!uploadForm.file) return alert("No File Selected")
  if (!uploadForm.title.trim()) return alert("Title Required")
  isUploading.value = true
  try {
    const formData = new FormData()
    formData.append('Image', uploadForm.file)
    formData.append('Title', uploadForm.title)
    if (uploadForm.desc) formData.append('Desc', uploadForm.desc)
    if (uploadForm.authorName) formData.append('AuthorName', uploadForm.authorName)
    
    const res = await apiClient.post('/Drawing/upload', formData, { 
      headers: { 'Content-Type': 'multipart/form-data' } 
    })
    
    if (res.data.success) {
      showUploadModal.value = false
      uploadForm.title = ''; uploadForm.desc = ''; uploadForm.file = null; uploadForm.previewUrl = ''; uploadForm.authorName = ''
      fetchArtworks(true)
      fetchLeaderboard() 
      emit('refresh-stats')
    } else { 
      alert(res.data.message) 
    }
  } catch (e) { alert("Upload Error") } finally { isUploading.value = false }
}

const handleImgError = (e) => { e.target.src = '/土豆.jpg' }
const openLightbox = (img) => { lightboxImg.value = img }
const padZero = (n) => n < 10 ? `0${n}` : n

onMounted(() => {
  fetchArtworks(true)
  fetchLeaderboard()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.gallery-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%; 
  height: 100%; /* 确保填满父容器 */
  display: flex; 
  flex-direction: column; /* 垂直排列 Header 和 Body */
  font-family: var(--mono); 
  color: var(--black);
  overflow: hidden; /* 防止最外层出现滚动条 */
  position: relative;
}

/* 1. 控制条 (固定不滚动) */
.gallery-control-bar {
  flex-shrink: 0; /* 禁止压缩 */
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px 20px;
  background: var(--white);
  border-bottom: 2px solid var(--black);
  gap: 20px; 
  z-index: 10;
}

/* 2. Body：主体区域 */
.gallery-body {
  flex: 1; /* 占据剩余高度 */
  display: flex; 
  overflow: hidden; /* 关键：切断外部滚动 */
  padding: 20px; 
  gap: 20px;
  position: relative;
  min-height: 0; /* 关键：允许 Flex 子项缩小 */
}

/* 2.1 Main：左侧列表容器 */
.gallery-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden; /* 自身不滚动 */
  min-width: 0;
  min-height: 0; /* 关键 */
}

/* 🔥 真正的滚动容器 🔥 */
.gallery-scroll-container {
  flex: 1;
  overflow-y: auto; /* 只有这里允许纵向滚动 */
  overflow-x: hidden;
  padding-right: 5px; /* 给滚动条留位置 */
  min-height: 0; /* 关键：防止被内容撑开 */
}

/* 内容包装器 */
.gallery-content-wrapper {
  padding-bottom: 40px; /* 底部留白，方便触发加载 */
}

.gallery-waterfall {
  display: flex; gap: 15px; align-items: flex-start;
}
.waterfall-column {
  flex: 1; display: flex; flex-direction: column; gap: 15px; min-width: 0;
}

/* 2.2 Sidebar：右侧侧边栏 */
.gallery-sidebar {
  width: 260px; 
  flex-shrink: 0; 
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
  overflow-y: auto; /* 侧边栏独立滚动 */
  max-height: 100%;
}

/* --- 以下样式保持原样 --- */
.header-title-block .giant-text { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.header-title-block .sub-text { font-size: 0.7rem; color: #666; font-weight: bold; }
.cyber-upload-btn { background: var(--black); color: var(--white); border: none; padding: 10px 20px; font-family: var(--heading); font-size: 1.1rem; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: all 0.2s; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.cyber-upload-btn:hover { background: var(--red); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.cyber-upload-btn .icon { color: var(--red); font-size: 1.2rem; }
.cyber-upload-btn:hover .icon { color: var(--white); }
.cyber-group { display: flex; align-items: center; gap: 0; height: 45px; flex: 1; max-width: 600px; border: 2px solid var(--black); background-color: var(--white); padding: 2px; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.button-segment { display: flex; align-items: center; justify-content: center; height: 100%; font-family: var(--heading); font-size: 1.1rem; color: var(--black); cursor: pointer; flex-grow: 1; width: 10%; white-space: nowrap; overflow: hidden; border-right: 1px solid var(--black); transition: width 0.5s cubic-bezier(0.25, 1, 0.5, 1), flex-grow 0.5s cubic-bezier(0.25, 1, 0.5, 1), background-color 0.2s, color 0.2s; }
.button-segment:last-child { border-right: none; }
.seg-icon { margin-right: 8px; font-size: 0.9em; color: var(--red); }
.button-segment.hoverable:hover { background-color: var(--gray); flex-grow: 4; width: 30%; }
.button-segment.active { background-color: var(--black); color: var(--white); flex-grow: 3; width: 25%; }
.button-segment.active .seg-icon { color: var(--white); }
.segment-button-group:has(.button-segment.hoverable:hover:not(.active)) .button-segment.active { flex-grow: 1; width: 10%; background-color: #333; color: #ccc; }
.button-segment.active.hoverable:hover { flex-grow: 6; width: 40%; background-color: var(--red); color: var(--white); }
.cyber-art-card { width: 100%; position: relative; cursor: pointer; transition: 0.2s; }
.card-frame { border: 2px solid var(--black); background: var(--white); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); padding: 8px; }
.cyber-art-card:hover .card-frame { transform: translate(-3px, -3px); box-shadow: 6px 6px 0 var(--red); border-color: var(--black); }
.corner-dec { position: absolute; bottom: 0; right: 0; width: 0; height: 0; border-style: solid; border-width: 0 0 15px 15px; border-color: transparent transparent var(--black) transparent; pointer-events: none; }
.img-box { width: 100%; background: #000; position: relative; overflow: hidden; border-bottom: 2px solid var(--black); margin-bottom: 8px; }
.img-box img { width: 100%; display: block; height: auto; transition: 0.3s; }
.cyber-art-card:hover img { filter: contrast(1.1); }
.scan-line { position: absolute; top:0; left:0; width:100%; height:2px; background:rgba(255,255,255,0.3); animation: scan 3s linear infinite; display: none; }
.cyber-art-card:hover .scan-line { display: block; }
.card-info { padding: 0 4px; }
.art-title { font-family: var(--heading); font-size: 1.1rem; margin: 0 0 5px 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-transform: uppercase; }
.art-meta-row { display: flex; justify-content: space-between; font-size: 0.7rem; font-weight: bold; color: #666; }
.likes { color: var(--red); }
.cyber-panel { border: 2px solid var(--black); background: var(--white); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.panel-header { background: var(--black); color: var(--white); padding: 8px 10px; font-weight: bold; font-size: 0.9rem; border-bottom: 2px solid var(--black); }
.deco { color: var(--red); margin-right: 5px; }
.tag-matrix { padding: 15px; display: flex; flex-wrap: wrap; gap: 8px; }
.cyber-tag { background: var(--white); border: 1px solid var(--black); padding: 4px 8px; font-size: 0.75rem; font-weight: bold; cursor: pointer; transition: 0.2s; }
.cyber-tag:hover { background: var(--black); color: var(--white); }
.cyber-tag.red-mode { border-color: var(--red); color: var(--red); }
.cyber-tag.red-mode:hover { background: var(--red); color: var(--white); }
.rank-list { padding: 10px; }
.rank-item { display: flex; align-items: center; gap: 10px; padding: 8px 0; border-bottom: 1px dashed #ccc; }
.rank-item:last-child { border-bottom: none; }
.rank-idx { font-family: var(--heading); font-size: 1.2rem; width: 30px; text-align: center; color: #ccc; }
.top-1 { color: var(--red); font-size: 1.5rem; text-shadow: 2px 2px 0 #000; }
.top-2 { color: var(--black); }
.top-3 { color: #666; }
.rank-info { flex: 1; }
.r-name { font-weight: bold; font-size: 0.9rem; }
.r-score { font-size: 0.7rem; color: #666; }
.r-score .val { font-weight: bold; color: var(--black); }
.list-footer { text-align: center; padding: 20px; font-size: 0.8rem; color: #888; font-weight: bold; }
.blink { animation: blink 1s infinite; }

/* Upload Modal */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
.cyber-modal-window.upload-mode { width: 550px; max-width: 95vw; background: #f4f4f4; border: 4px solid var(--black); box-shadow: 15px 15px 0 rgba(0,0,0,0.5); display: flex; flex-direction: column; max-height: 90vh; font-family: var(--mono); }
.modal-header { background: var(--black); color: var(--white); padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--red); flex-shrink: 0; }
.header-left { display: flex; align-items: center; gap: 10px; }
.status-light { width: 10px; height: 10px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.title { font-family: var(--heading); font-size: 1.2rem; letter-spacing: 1px; }
.close-btn { background: transparent; border: 1px solid #666; color: #aaa; padding: 4px 10px; cursor: pointer; font-size: 0.75rem; font-family: var(--mono); transition: 0.2s; }
.close-btn:hover { border-color: var(--red); color: var(--red); }
.modal-body { padding: 30px; overflow-y: auto; flex: 1; }
.upload-section { margin-bottom: 30px; }
.upload-zone { height: 220px; border: 2px dashed #999; background: #e0e0e0; position: relative; cursor: pointer; transition: all 0.3s; overflow: hidden; display: flex; justify-content: center; align-items: center; }
.upload-zone:hover, .upload-zone.is-dragover { border-color: var(--black); background: #fff; box-shadow: inset 0 0 20px rgba(0,0,0,0.05); }
.upload-zone.has-file { border-style: solid; border-color: var(--black); padding: 0; background: #000; }
.scan-grid { position: absolute; inset: 0; background-image: linear-gradient(45deg, #ccc 25%, transparent 25%, transparent 75%, #ccc 75%, #ccc), linear-gradient(45deg, #ccc 25%, transparent 25%, transparent 75%, #ccc 75%, #ccc); background-size: 20px 20px; opacity: 0.1; pointer-events: none; }
.center-content { text-align: center; position: relative; z-index: 2; }
.icon-frame { width: 60px; height: 60px; border: 2px solid var(--black); display: flex; align-items: center; justify-content: center; margin: 0 auto 15px; background: #fff; }
.plus { font-size: 40px; font-weight: 300; line-height: 1; color: var(--black); }
.main-tip { font-weight: bold; font-size: 1rem; color: var(--black); margin: 0 0 5px; }
.sub-tip { font-size: 0.7rem; color: #666; background: #fff; padding: 2px 5px; border: 1px solid #ccc; }
.preview-wrap { width: 100%; height: 100%; position: relative; }
.preview-wrap img { width: 100%; height: 100%; object-fit: contain; }
.reupload-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.6); display: flex; justify-content: center; align-items: center; opacity: 0; transition: 0.2s; }
.preview-wrap:hover .reupload-overlay { opacity: 1; }
.btn-replace { color: var(--white); border: 2px solid var(--white); padding: 8px 16px; font-weight: bold; background: rgba(0,0,0,0.5); }
.file-meta { position: absolute; bottom: 0; left: 0; right: 0; background: var(--black); color: var(--white); font-size: 0.7rem; padding: 4px 10px; text-align: right; }
.form-section { display: flex; flex-direction: column; gap: 25px; }
.cyber-input-group { position: relative; }
.label-chip { position: absolute; top: -10px; left: 10px; background: var(--black); color: var(--white); font-size: 0.75rem; font-weight: bold; padding: 2px 8px; z-index: 2; letter-spacing: 0.5px; }
.cyber-input { width: 100%; background: #fff; border: 2px solid #ccc; border-bottom: 4px solid var(--black); padding: 15px 15px 10px; font-family: var(--mono); font-size: 1rem; color: var(--black); outline: none; transition: all 0.3s; }
.cyber-input.textarea { resize: vertical; min-height: 80px; }
.cyber-input:focus { border-color: var(--black); background: #fffef0; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.input-line { position: absolute; bottom: -4px; left: 0; height: 4px; width: 0; background: var(--red); transition: width 0.3s; }
.cyber-input:focus + .input-line { width: 100%; }
.modal-footer { margin-top: 30px; display: flex; justify-content: space-between; align-items: center; border-top: 2px dashed #ccc; padding-top: 20px; }
.status-display { font-size: 0.8rem; font-weight: bold; color: #666; }
.status-display .busy { color: var(--red); animation: blink 1s infinite; }
.status-display .ready { color: #27c93f; }
.execute-btn { background: var(--black); color: var(--white); border: none; padding: 12px 30px; font-family: var(--heading); font-size: 1.2rem; cursor: pointer; transition: all 0.2s; display: flex; align-items: center; gap: 10px; box-shadow: 4px 4px 0 #999; }
.execute-btn:hover { background: var(--red); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.execute-btn:disabled { background: #ccc; color: #888; cursor: not-allowed; box-shadow: none; transform: none; }

/* 动画 */
@keyframes scan { 0% { top: -10%; } 100% { top: 110%; } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.95); }
.glitch-fade-leave-to { opacity: 0; transform: scale(1.05); }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); border-radius: 0; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }

/* 响应式 */
@media (max-width: 1200px) {
  .gallery-sidebar { display: none; }
}
</style>