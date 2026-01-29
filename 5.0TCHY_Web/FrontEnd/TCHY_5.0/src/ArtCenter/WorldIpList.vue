<template>
  <div class="world-industrial">
    
    <div class="world-control-bar">
      <div class="header-left">
        <div class="icon-box">SECTOR</div>
        <div class="text-info">
          <h2 class="title">WORLD_ARCHIVES</h2>
          <span class="sub">世界观数据库 // SECTOR_DATABASE</span>
        </div>
      </div>

      <div class="header-right">
        <div class="search-unit">
          <span class="prompt">>> FILTER:</span>
          <input v-model="searchQuery" class="cyber-input-search" placeholder="KEYWORD..." />
        </div>
        <button class="cyber-btn primary" @click="openCreateModal">
          <span class="icon">+</span> INITIATE_GENESIS
        </button>
      </div>
    </div>

    <div class="world-body">
      <div class="world-scroll-container custom-scroll">
        <div class="world-content-wrapper">
          
          <div v-if="loading" class="loading-state"><div class="spinner"></div><span>SCANNING_SECTORS...</span></div>
          <div v-else-if="filteredList.length === 0" class="empty-state">[ NO_WORLDS_DISCOVERED ]</div>

          <div v-else class="ip-grid">
            <article v-for="ip in filteredList" :key="ip.id" class="ip-card">
              <div class="card-header">
                <span class="sector-id">SEC-{{ padZero(ip.id) }}</span>
                <div class="status-light online"></div>
              </div>

              <div class="card-viewport" @click="openGraphEditor(ip)">
                <img :src="getImageUrl(ip.cover_url)" loading="lazy" @error="handleImgError" />
                <div class="viewport-overlay"><span class="enter-text">>>> ACCESS_NEURAL_LINK</span></div>
              </div>

              <div class="card-data">
                <div class="data-row-title">
                  <h3 class="ip-name" :title="ip.name">{{ ip.name }}</h3>
                  <div class="action-icons">
                    <button class="icon-btn edit" @click.stop="openEditModal(ip)" title="EDIT">✎</button>
                    <button class="icon-btn del" @click.stop="deleteIp(ip.id)" title="DELETE">🗑</button>
                  </div>
                </div>
                <div class="ip-tagline" v-if="ip.tagline"><span class="quote">"</span>{{ ip.tagline }}<span class="quote">"</span></div>
                <p class="ip-summary">{{ ip.summary || 'DATA_MISSING...' }}</p>
                <div class="card-footer"><span class="update-time">UPDATED: {{ formatDate(ip.updateTime) }}</span></div>
              </div>
              <div class="corner-bl"></div>
            </article>
          </div>

        </div>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="glitch-fade">
        <div v-if="showModal" class="cyber-modal-overlay" @click.self="closeModal">
          <div class="cyber-terminal form-mode">
            <div class="term-header">
              <span class="term-title">>> {{ isEditing ? 'UPDATE_PROTOCOL' : 'GENESIS_PROTOCOL' }}</span>
              <button class="term-close" @click="closeModal">[ 关闭 ]</button>
            </div>
            
            <div class="term-content custom-scroll">
              <form @submit.prevent="submitForm" class="cyber-form">
                
                <div class="form-group">
                  <label>WORLD_NAME <span class="req">*</span></label>
                  <input v-model="form.Name" class="cyber-input" required />
                </div>
                
                <div class="form-group">
                  <label>TAGLINE</label>
                  <input v-model="form.Tagline" class="cyber-input" placeholder="一句简短的描述..." />
                </div>
                
                <div class="form-group">
                  <label>SUMMARY</label>
                  <textarea v-model="form.Summary" class="cyber-textarea" rows="4"></textarea>
                </div>

                <div class="form-group">
                  <label>COVER_IMAGE</label>
                  
                  <div v-if="isEditing" class="cover-upload-box">
                    <div class="preview-frame">
                      <img :src="getImageUrl(form.CoverUrl)" @error="handleImgError" />
                    </div>
                    <div class="upload-actions">
                       <button type="button" class="cyber-btn ghost small" @click="triggerFileSelect" :disabled="uploading">
                         {{ uploading ? 'TRANSMITTING...' : 'UPLOAD_NEW_COVER' }}
                       </button>
                       <input type="file" ref="coverInput" style="display:none" accept="image/*" @change="handleCoverUpload" />
                       <span class="tip-text" v-if="!uploading">>> RECOMMENDED: 16:9 RATIO</span>
                    </div>
                  </div>

                  <div v-else class="create-tip-box">
                    <span class="icon">ℹ</span>
                    <span>INITIALIZE_WORLD_FIRST_TO_UPLOAD_COVER</span>
                    <br>
                    <small>(请先创建世界观，创建成功后再次编辑即可上传封面)</small>
                  </div>
                </div>

                <div class="form-actions">
                  <button class="cyber-btn ghost" type="button" @click="closeModal">CANCEL</button>
                  <button class="cyber-btn primary" type="submit" :disabled="submitting">
                    {{ submitting ? 'PROCESSING...' : 'EXECUTE' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Transition name="zoom-in">
      <WorldGraph 
        v-if="showGraph" 
        :id="currentGraphId" 
        @close="closeGraphEditor" 
      />
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'
import WorldGraph from '@/ArtCenter/Components/WorldDashBoard.vue'

// --- State ---
const loading = ref(false)
const ipList = ref([])
const searchQuery = ref('')

// Graph & Modal State
const showGraph = ref(false)
const currentGraphId = ref(null)
const showModal = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const currentEditId = ref(null)
const form = reactive({ Name: '', Tagline: '', Summary: '', CoverUrl: '' })

// Upload State
const uploading = ref(false)
const coverInput = ref(null) // 绑定 input ref

// --- Helpers ---
const padZero = (n) => n < 10 ? `0${n}` : n
const formatDate = (t) => t ? new Date(t).toLocaleDateString() : 'UNKNOWN'
const getImageUrl = (url) => { 
  if (!url) return '/土豆.jpg'; 
  if (url.startsWith('http')) return url; 
  return `https://bianyuzhou.com/${url}` 
}
const handleImgError = (e) => e.target.src = '/土豆.jpg'

// --- Actions ---
const fetchList = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/ip')
    ipList.value = Array.isArray(res.data) ? res.data : []
  } catch (e) { ipList.value = [] } 
  finally { loading.value = false }
}

const filteredList = computed(() => {
  if (!searchQuery.value) return ipList.value
  const q = searchQuery.value.toLowerCase()
  return ipList.value.filter(ip => (ip.name && ip.name.toLowerCase().includes(q)) || (ip.tagline && ip.tagline.toLowerCase().includes(q)))
})

const openGraphEditor = (ip) => { currentGraphId.value = ip.id; showGraph.value = true }
const closeGraphEditor = () => { showGraph.value = false; currentGraphId.value = null; fetchList() }

const openCreateModal = () => { 
  isEditing.value = false; 
  currentEditId.value = null; 
  Object.assign(form, { Name: '', Tagline: '', Summary: '', CoverUrl: '' }); 
  showModal.value = true 
}

const openEditModal = (ip) => { 
  isEditing.value = true; 
  currentEditId.value = ip.id; 
  Object.assign(form, { Name: ip.name, Tagline: ip.tagline, Summary: ip.summary, CoverUrl: ip.cover_url }); 
  showModal.value = true 
}

const closeModal = () => showModal.value = false

// 🔥 触发文件选择的辅助函数
const triggerFileSelect = () => {
  if(coverInput.value) {
    coverInput.value.click()
  } else {
    console.error("Input ref not found")
  }
}

// 🔥 处理封面上传
const handleCoverUpload = async (e) => {
  const file = e.target.files[0]
  if (!file) return
  if (!currentEditId.value) return 

  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('file', file)
    
    // 调用新的后端接口
    const res = await apiClient.post(`/ip/${currentEditId.value}/image`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    if (res.data.success) {
      form.CoverUrl = res.data.url
      alert("COVER_UPLOAD_COMPLETE")
      // 刷新列表以更新背景图
      fetchList()
    }
  } catch (err) {
    console.error(err)
    alert("UPLOAD_FAILED")
  } finally {
    uploading.value = false
    if(e.target) e.target.value = ''
  }
}

const submitForm = async () => {
  if (!form.Name.trim()) return alert("NAME_REQUIRED")
  submitting.value = true
  try {
    const method = isEditing.value ? 'put' : 'post'
    const url = isEditing.value ? `/ip/${currentEditId.value}` : '/ip'
    await apiClient[method](url, form)
    alert("SUCCESS")
    closeModal(); fetchList()
  } catch (e) { alert("ERROR") } finally { submitting.value = false }
}

const deleteIp = async (id) => { if (confirm("DELETE?")) { await apiClient.delete(`/ip/${id}`); fetchList() } }

onMounted(() => { fetchList() })
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 基础布局与变量 --- */
.world-industrial {
  /* 🔥 修复点：变量分行写，避免特殊字符报错 */
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;

  width: 100%; 
  height: 100%; 
  display: flex; 
  flex-direction: column;
  
  font-family: var(--mono); 
  color: var(--black); 
  
  position: relative;
  overflow: hidden;
}

/* --- 1. Header --- */
.world-control-bar {
  flex-shrink: 0; display: flex; justify-content: space-between; align-items: center;
  padding: 15px 20px; background: var(--white); border-bottom: 4px solid var(--black); gap: 20px; z-index: 10;
}
.header-left { display: flex; align-items: center; gap: 15px; }
.icon-box { 
  background: var(--black); color: var(--white); font-family: var(--heading); 
  font-size: 1.5rem; padding: 5px 10px; transform: skew(-10deg); 
}
.text-info h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-info .sub { font-size: 0.7rem; font-weight: bold; color: #666; }

.header-right { display: flex; align-items: center; gap: 20px; }
.search-unit { 
  display: flex; align-items: center; gap: 10px; background: #eee; 
  padding: 5px 10px; border: 2px solid var(--black); 
}
.prompt { font-weight: bold; color: var(--red); }
.cyber-input-search { 
  border: none; background: transparent; outline: none; 
  font-family: var(--mono); width: 200px; font-weight: bold; 
}

.cyber-btn { 
  border: 2px solid var(--black); padding: 8px 20px; font-family: var(--heading); 
  font-size: 1.1rem; cursor: pointer; display: flex; align-items: center; gap: 8px; 
  transition: 0.2s; background: var(--white); 
}
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover { 
  background: var(--red); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); 
}
.cyber-btn.ghost { background: transparent; }
.cyber-btn.ghost:hover { background: #e0e0e0; }
.cyber-btn.small { padding: 5px 10px; font-size: 0.8rem; }

/* --- 2. Body --- */
.world-body { 
  flex: 1; display: flex; overflow: hidden; min-height: 0; 
  position: relative; background-color: var(--gray); 
}
.world-scroll-container { flex: 1; overflow-y: auto; padding: 20px; }
.world-content-wrapper { padding-bottom: 40px; }

/* Grid */
.ip-grid { 
  display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 25px; 
}
.ip-card { 
  background: var(--white); border: 2px solid var(--black); display: flex; flex-direction: column; 
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1); transition: all 0.2s; 
  height: 100%; position: relative; overflow: hidden; 
}
.ip-card:hover { 
  transform: translateY(-4px); box-shadow: 10px 10px 0 var(--red); border-color: var(--black); 
}

.card-header { 
  display: flex; justify-content: space-between; align-items: center; 
  padding: 5px 10px; background: #eee; border-bottom: 2px solid var(--black); 
}
.sector-id { font-size: 0.7rem; font-weight: bold; color: #666; }
.status-light { width: 10px; height: 10px; border-radius: 50%; background: #ccc; border: 1px solid #999; }
.status-light.online { background: #2ecc71; box-shadow: 0 0 5px #2ecc71; }

.card-viewport { 
  height: 180px; background: #000; position: relative; overflow: hidden; 
  border-bottom: 2px solid var(--black); cursor: pointer; 
}
.card-viewport img { 
  width: 100%; height: 100%; object-fit: cover; transition: 0.4s; opacity: 0.9; 
}
.ip-card:hover .card-viewport img { 
  transform: scale(1.05); opacity: 0.4; filter: grayscale(100%); 
}
.viewport-overlay { 
  position: absolute; inset: 0; display: flex; justify-content: center; 
  align-items: center; opacity: 0; transition: 0.3s; 
}
.ip-card:hover .viewport-overlay { opacity: 1; }
.enter-text { 
  color: var(--white); font-weight: bold; font-family: var(--heading); 
  font-size: 1.2rem; border: 2px solid var(--white); 
  padding: 5px 15px; background: rgba(0,0,0,0.5); 
}

.card-data { padding: 15px; flex: 1; display: flex; flex-direction: column; }
.data-row-title { 
  display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 10px; 
}
.ip-name { 
  font-family: var(--heading); font-size: 1.6rem; margin: 0; line-height: 1; 
  width: 70%; word-break: break-all; 
}
.action-icons { display: flex; gap: 5px; opacity: 0; transition: 0.2s; }
.ip-card:hover .action-icons { opacity: 1; }
.icon-btn { 
  background: transparent; border: 1px solid #ccc; cursor: pointer; 
  padding: 2px 6px; font-size: 0.8rem; 
}
.icon-btn:hover { background: var(--black); color: var(--white); border-color: var(--black); }
.icon-btn.del:hover { background: var(--red); border-color: var(--red); }

.ip-tagline { font-size: 0.8rem; font-weight: bold; color: var(--red); margin-bottom: 10px; }
.quote { opacity: 0.5; margin: 0 2px; }
.ip-summary { 
  font-size: 0.85rem; color: #555; line-height: 1.6; margin-bottom: 15px; 
  flex: 1; display: -webkit-box; -webkit-line-clamp: 3; 
  -webkit-box-orient: vertical; overflow: hidden; 
}
.card-footer { 
  border-top: 1px dashed #ccc; padding-top: 10px; margin-top: auto; 
  font-size: 0.7rem; color: #999; font-weight: bold; text-align: right; 
}
.corner-bl { 
  position: absolute; bottom: 0; left: 0; width: 0; height: 0; border-style: solid; 
  border-width: 15px 0 0 15px; border-color: transparent transparent transparent var(--black); 
  pointer-events: none; 
}

/* --- 3. Modal --- */
.cyber-modal-overlay { 
  position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; 
  display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); 
}
.cyber-terminal { 
  width: 600px; max-width: 95vw; background: #f4f4f4; border: 4px solid var(--black); 
  display: flex; flex-direction: column; max-height: 85vh; 
  box-shadow: 20px 20px 0 rgba(0,0,0,0.5); 
}
.term-header { 
  background: var(--black); color: var(--white); padding: 12px 20px; 
  display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--red); 
}
.term-title { font-family: var(--heading); letter-spacing: 1px; font-size: 1.2rem; }
.term-close { 
  background: transparent; border: 1px solid #555; color: #aaa; cursor: pointer; 
  font-family: var(--mono); font-weight: bold; padding: 4px 10px; 
}
.term-close:hover { border-color: var(--red); color: var(--red); }

.term-content { padding: 30px; overflow-y: auto; flex: 1; }
.cyber-form { display: flex; flex-direction: column; gap: 20px; }
.form-group label { 
  display: block; font-weight: bold; font-size: 0.8rem; margin-bottom: 5px; color: var(--black); 
}
.req { color: var(--red); }
.cyber-input, .cyber-textarea { 
  width: 100%; border: 2px solid #999; padding: 10px; 
  font-family: var(--mono); outline: none; background: #fff; box-sizing: border-box; 
}
.cyber-input:focus, .cyber-textarea:focus { 
  border-color: var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); color:#000 
}

/* 🔥 新增样式：封面上传区 */
.cover-upload-box {
  display: flex; gap: 15px; align-items: flex-start;
  border: 1px dashed #999; padding: 10px; background: #eee;
}
.preview-frame {
  width: 120px; height: 68px; /* 16:9 ratio */
  border: 2px solid var(--black); background: #333;
  flex-shrink: 0;
}
.preview-frame img {
  width: 100%; height: 100%; object-fit: cover;
}
.upload-actions {
  display: flex; flex-direction: column; gap: 5px; flex: 1;
}
.tip-text {
  font-size: 0.65rem; color: #666; font-family: var(--mono);
}
.create-tip-box {
  background: #fff; border: 1px solid #ccc; padding: 10px; 
  display: flex; gap: 10px; align-items: center;
  color: #666; font-size: 0.8rem;
}
.create-tip-box .icon { font-weight: bold; color: var(--red); }

.form-actions { 
  display: flex; gap: 15px; margin-top: 10px; border-top: 1px dashed #ccc; padding-top: 20px; 
}

/* 状态与加载 */
.loading-state, .empty-state { 
  text-align: center; padding: 100px 0; font-weight: bold; color: #888; 
  display: flex; flex-direction: column; align-items: center; gap: 15px; 
}
.spinner { 
  width: 30px; height: 30px; border: 4px solid #ccc; 
  border-top-color: var(--black); border-radius: 50%; animation: spin 1s linear infinite; 
}

@keyframes spin { to { transform: rotate(360deg); } }

/* 动画效果 */
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.95); }

.zoom-in-enter-active, .zoom-in-leave-active { transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1); }
.zoom-in-enter-from, .zoom-in-leave-to { opacity: 0; transform: scale(0.9); }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>