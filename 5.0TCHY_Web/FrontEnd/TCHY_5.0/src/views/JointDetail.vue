<template>
  <div class="dossier-page">
    <div class="paper-texture"></div>
    
    <div class="dossier-wrapper" v-if="data">
      
      <header class="dossier-header">
        <div class="header-left">
          <button class="back-to-archives" @click="$router.back()">
            <span class="arrow">←</span> [ 返回档案库 // RETURN_TO_ARCHIVES ]
          </button>
          
          <div class="serial-no">
            <span class="lbl">SERIAL_NO:</span>
            <span class="val">THCY-OP-{{ padZero(data.id) }}</span>
          </div>
        </div>

        <div class="header-right">
          <div class="clearance-level">
            <span class="stamp" :class="data.verify === 0 ? 'pending' : 'active'">
              {{ data.verify === 0 ? 'PENDING_APPROVAL' : 'ACTIVE_OPERATION' }}
            </span>
          </div>
          <div class="timestamp-small">记录时间: {{ currentTime }}</div>
        </div>
      </header>

      <hr class="heavy-line" />

      <section class="title-section">
        <div class="subject-meta">
          <span class="type">行动性质: {{ typeLabelCn(data.type) }}</span>
          <span class="date">建档日期: {{ shortDate(data.startdate) }}</span>
        </div>
        <h1 class="mission-title">{{ data.name }}</h1>
      </section>

      <div class="main-content-layout">
        <main class="report-body">
          <div class="content-block">
            <h3 class="block-title">01_任务简报 // MISSION_SUMMARY</h3>
            <div class="text-area briefing">
              {{ data.desc || '等待情报录入...' }}
            </div>
          </div>

          <div class="content-block" v-if="data.rules">
            <h3 class="block-title">02_执行协议 // PROTOCOLS</h3>
            <div class="text-area code-font">
              {{ data.rules }}
            </div>
          </div>

          <div class="content-block">
            <h3 class="block-title">03_通讯交互记录 // COMMS_LOG</h3>
            <div class="comms-container">
              <UniversalComments targetType="JOINT" :targetId="data.id" theme="minimal-light" />
            </div>
          </div>
        </main>

        <aside class="metadata-sidebar">
          
          <div class="action-box">
            <button 
              class="formal-btn primary" 
              @click="handleOneClickApply" 
              :disabled="hasApplied || isSubmitting"
            >
              {{ applyBtnText }}
            </button>
            
            <button class="formal-btn" @click="openUploadModal" :disabled="!isApproved">
              提交数据包 // SUBMIT_FILE
            </button>
          </div>

          <div class="stats-box">
            <div class="stat-row">
              <span class="l">剩余期限 // DEADLINE</span>
              <span class="r highlight">{{ daysRemaining }} DAYS</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" :style="{ width: progressPercentage + '%' }"></div>
            </div>
            <div class="stat-grid">
              <div class="s-cell">
                <span class="v">{{ participants.length }}</span>
                <span class="k">部署总数</span>
              </div>
              <div class="s-cell">
                <span class="v success">{{ completedCount }}</span>
                <span class="k">已达成</span>
              </div>
              <div class="s-cell">
                <span class="v warning">{{ guguCount }}</span>
                <span class="k">失联/鸽</span>
              </div>
            </div>
          </div>

          <div class="status-manager" v-if="currentUserParticipant && isApproved">
            <div class="box-title">状态变更申报 // UPDATE_STATUS</div>
            <div class="radio-group">
              <button 
                v-for="(label, status) in statusOptions" 
                :key="status"
                class="radio-btn"
                :class="[{ active: currentUserParticipant.prodStatus == status }, `s-${status}`]"
                @click="updateProdStatus(status)"
              >
                {{ label }}
              </button>
            </div>
          </div>

          <div class="roster-box">
            <div class="box-title">参战单位名册 // PERSONNEL_ROSTER</div>
            <div class="unit-list custom-scroll">
              <div v-for="user in participants" :key="user.id" class="unit-entry" :class="`state-${user.prodStatus}`">
                <div class="u-avatar">
                  <CommonAvatar :userId="user.id" :passedAvatar="user.avatar" :showLevel="false" />
                </div>
                <div class="u-info">
                  <div class="u-name">{{ user.username }}</div>
                  <div class="u-tag">{{ statusOptions[user.prodStatus] }}</div>
                </div>
                <div class="u-icon">{{ statusIcons[user.prodStatus] }}</div>
              </div>
            </div>
          </div>
        </aside>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showUploadModal" class="modal-overlay" @click.self="closeUploadModal">
          <div class="formal-modal">
            <div class="modal-title">数据包传输协议 // FILE_TRANSFER</div>
            <div class="modal-body">
              <div class="drop-zone" @click="triggerFileInput" :class="{ 'has-file': selectedFile }">
                <input type="file" ref="fileInput" @change="handleFileChange" style="display: none" />
                <span class="file-ico">{{ selectedFile ? '☑' : '☐' }}</span>
                <p>{{ selectedFile ? selectedFile.name : '点击选择附件资料 (Max 500MB)' }}</p>
              </div>
              <textarea v-model="uploadDescription" placeholder="附言: 请输入本次提交的备注信息..."></textarea>
              <div v-if="isUploading" class="upload-status">
                正在同步: {{ uploadProgress }}%
                <div class="p-track"><div class="p-fill" :style="{width: uploadProgress + '%'}"></div></div>
              </div>
            </div>
            <div class="modal-actions">
              <button @click="closeUploadModal">取消</button>
              <button class="submit" @click="executeUpload" :disabled="!selectedFile || isUploading">执行传输</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import CommonAvatar from '@/GeneralComponents/UserAvatar.vue'
import UniversalComments from '@/GeneralComponents/UniversalComments.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const data = ref(null)
const participants = ref([])
const loadingParticipants = ref(false)
const isSubmitting = ref(false)
const currentTime = ref('')

// 状态选项
const statusOptions = { 0: '行动中', 1: '已完成', 2: '咕咕咕' }
const statusIcons = { 0: '⏳', 1: '✅', 2: '🕊️' }

// 上传控制
const showUploadModal = ref(false)
const isUploading = ref(false)
const uploadProgress = ref(0)
const selectedFile = ref(null)
const uploadDescription = ref('')
const fileInput = ref(null)

const padZero = (n) => (n < 10 ? `0${n}` : n)
const typeLabelCn = (t) => ({ 1: '联合行动', 2: '接力计划', 3: '创作竞技' }[t] || '常规')
const shortDate = (d) => d ? d.substring(0, 10) : ''

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString('zh-CN', { hour12: false })
}
let timer = setInterval(updateTime, 1000)

// --- 计算属性 ---
const currentUserParticipant = computed(() => participants.value.find(p => p.id === authStore.user?.id))
const hasApplied = computed(() => !!currentUserParticipant.value)
const isApproved = computed(() => currentUserParticipant.value?.status === 1)
const completedCount = computed(() => participants.value.filter(p => p.prodStatus == 1).length)
const guguCount = computed(() => participants.value.filter(p => p.prodStatus == 2).length)
const applyBtnText = computed(() => isSubmitting.value ? '同步中...' : (hasApplied.value ? '已接入' : '申请接入行动'))

const daysRemaining = computed(() => {
  if (!data.value?.enddate) return 'INF'
  const diff = new Date(data.value.enddate).getTime() - Date.now()
  return diff < 0 ? 0 : Math.ceil(diff / 86400000)
})

const progressPercentage = computed(() => {
  if (!data.value) return 0
  const start = new Date(data.value.startdate).getTime()
  const end = data.value.enddate ? new Date(data.value.enddate).getTime() : start + 2592000000
  return Math.min(100, Math.max(0, ((Date.now() - start) / (end - start)) * 100))
})

// --- 接口 ---
const fetchData = async () => {
  try {
    const res = await apiClient.get(`ChaiLianHe/${route.params.id}`)
    data.value = res.data
    fetchParticipants()
  } catch (err) { console.error(err) }
}

const fetchParticipants = async () => {
  loadingParticipants.value = true
  try {
    const res = await apiClient.get(`ChaiLianHe/${route.params.id}/participants`)
    participants.value = res.data.data || []
  } finally { loadingParticipants.value = false }
}

const updateProdStatus = async (status) => {
  try {
    await apiClient.post('ChaiLianHe/update-prod-status', { eventId: data.value.id, prodStatus: status })
    fetchParticipants()
  } catch (err) { console.error(err) }
}

const handleOneClickApply = async () => {
  if (!authStore.checkAuth()) return router.push({ name: 'Login' })
  isSubmitting.value = true
  try {
    await apiClient.post('ChaiLianHe/apply', { eventId: data.value.id })
    fetchParticipants()
  } catch (err) { alert('申请失败') }
  finally { isSubmitting.value = false }
}

const openUploadModal = () => showUploadModal.value = true
const closeUploadModal = () => { if(!isUploading.value) showUploadModal.value = false }
const triggerFileInput = () => fileInput.value?.click()
const handleFileChange = (e) => {
  const file = e.target.files[0]
  if (file && file.size < 500 * 1024 * 1024) selectedFile.value = file
}
const executeUpload = async () => {
  if (!selectedFile.value) return
  isUploading.value = true
  const formData = new FormData()
  formData.append('eventId', data.value.id)
  formData.append('file', selectedFile.value)
  formData.append('description', uploadDescription.value)
  try {
    await apiClient.post('ChaiLianHe/submit-work-file', formData, {
      onUploadProgress: (p) => { uploadProgress.value = Math.round((p.loaded * 100) / p.total) }
    })
    updateProdStatus(1)
    closeUploadModal()
  } catch (err) { alert('传输中断') }
  finally { isUploading.value = false }
}

onMounted(() => { fetchData(); updateTime(); })
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
/* --- 📄 机密档案样式表 --- */
.dossier-page {
  --ink: #1a1a1a;
  --paper: #ffffff;
  --dim: #777;
  --accent: #b71c1c; 
  --border: #ccc;
  --bg-soft: #f9f9f9;

  width: 100%; min-height: 100vh;
  background-color: #e5e5e5;
  color: var(--ink);
  font-family: "Georgia", "PingFang SC", serif;
  display: flex; justify-content: center;
  padding: 50px 20px; position: relative;
}

.paper-texture {
  position: fixed; inset: 0; pointer-events: none; opacity: 0.05;
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.8'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
}

.dossier-wrapper {
  width: 100%; max-width: 1100px;
  background: var(--paper);
  box-shadow: 0 15px 50px rgba(0,0,0,0.15);
  padding: 80px 100px;
  position: relative; border: 1px solid #bbb;
}

/* --- 顶部页眉修复 --- */
.dossier-header {
  display: flex; justify-content: space-between; align-items: flex-start;
}
.header-left { display: flex; flex-direction: column; gap: 15px; }

/* ✅ 找回的返回按钮样式：极简链接感 */
.back-to-archives {
  background: none; border: none; padding: 0;
  color: var(--dim); font-size: 0.75rem; font-weight: bold;
  cursor: pointer; text-align: left;
  transition: color 0.2s;
}
.back-to-archives:hover { color: var(--ink); }

.serial-no { font-family: 'Courier New', monospace; }
.serial-no .lbl { font-size: 0.7rem; color: var(--dim); font-weight: 800; }
.serial-no .val { font-size: 1.2rem; font-weight: 900; margin-left: 8px; text-decoration: underline; }

.header-right { text-align: right; }
.timestamp-small { font-size: 0.65rem; color: var(--dim); margin-top: 10px; font-family: monospace; }

.stamp {
  border: 4px double var(--accent); color: var(--accent);
  padding: 8px 20px; font-weight: 900; font-size: 1rem;
  transform: rotate(-5deg); display: inline-block;
  opacity: 0.8;
}
.stamp.active { border-color: #2e7d32; color: #2e7d32; }

.heavy-line { height: 4px; background: var(--ink); border: none; margin: 25px 0; }

/* --- 内容排版 --- */
.title-section { margin-bottom: 50px; }
.subject-meta { font-size: 0.85rem; color: var(--dim); margin-bottom: 8px; font-weight: bold; }
.subject-meta span { margin-right: 25px; }
.mission-title {
  font-size: 3.2rem; font-weight: 900; line-height: 1;
  text-transform: uppercase; margin: 0; letter-spacing: -1px;
}

.main-content-layout {
  display: grid; grid-template-columns: 1fr 340px; gap: 70px;
}

/* --- 正文区块 --- */
.content-block { margin-bottom: 50px; }
.block-title {
  font-size: 1rem; font-weight: 900; border-bottom: 2px solid var(--ink);
  padding-bottom: 5px; margin-bottom: 20px;
}
.text-area { line-height: 1.8; color: #222; font-size: 1.05rem; }
.briefing { white-space: pre-wrap; text-align: justify; }
.code-font {
  background: var(--bg-soft); padding: 25px; font-family: 'Courier New', monospace;
  font-size: 0.9rem; border: 1px solid #eee; border-left: 5px solid var(--ink);
}

/* --- 侧边栏卡片 --- */
.action-box { margin-bottom: 30px; }
.formal-btn {
  width: 100%; padding: 18px; background: #fff; border: 2px solid var(--ink);
  font-weight: 900; font-size: 0.9rem; cursor: pointer; margin-bottom: 12px;
  transition: all 0.2s; font-family: inherit;
}
.formal-btn.primary { background: var(--ink); color: #fff; }
.formal-btn:hover:not(:disabled) { background: #eee; transform: translate(-2px, -2px); box-shadow: 2px 2px 0 var(--ink); }
.formal-btn.primary:hover:not(:disabled) { background: #333; }

.stats-box { border: 1px solid var(--border); padding: 25px; margin-bottom: 30px; background: var(--bg-soft); }
.stat-row { display: flex; justify-content: space-between; font-size: 0.8rem; font-weight: 900; margin-bottom: 12px; }
.progress-container { height: 6px; background: #ddd; margin-bottom: 25px; }
.progress-bar { height: 100%; background: var(--ink); }

.stat-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; border-top: 1px solid var(--border); padding-top: 20px; gap: 10px; }
.s-cell { text-align: center; }
.s-cell .v { display: block; font-size: 1.6rem; font-weight: 900; line-height: 1; }
.s-cell .k { font-size: 0.6rem; color: var(--dim); font-weight: bold; margin-top: 5px; display: block; }
.s-cell .success { color: #2e7d32; }
.s-cell .warning { color: var(--accent); }

/* 状态申报区 */
.status-manager { margin-bottom: 30px; }
.box-title { font-size: 0.75rem; font-weight: 900; margin-bottom: 12px; color: var(--dim); text-transform: uppercase; }
.radio-group { border: 2px solid var(--ink); display: flex; }
.radio-btn {
  flex: 1; border: none; background: #fff; padding: 12px 5px; cursor: pointer;
  font-size: 0.75rem; font-weight: 900; border-right: 2px solid var(--ink);
}
.radio-btn:last-child { border-right: none; }
.radio-btn.active { background: var(--ink); color: #fff; }

.roster-box { border: 1px solid var(--border); padding: 20px; }
.unit-entry { display: flex; align-items: center; padding: 12px 0; border-bottom: 1px dashed var(--border); }
.u-avatar { width: 35px; height: 35px; margin-right: 15px; border: 1px solid #eee; }
.u-name { font-weight: 900; font-size: 0.9rem; }
.u-tag { font-size: 0.7rem; color: var(--dim); font-weight: bold; }
.u-icon { margin-left: auto; font-size: 1rem; }

.state-2 { opacity: 0.4; filter: grayscale(1); }

/* 其他 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--border); }

/* 弹窗样式 */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); display: flex; justify-content: center; align-items: center; z-index: 2000; backdrop-filter: blur(2px); }
.formal-modal { background: #fff; width: 550px; border: 4px solid var(--ink); padding: 50px; position: relative; }
.modal-title { font-size: 1.3rem; font-weight: 900; border-bottom: 3px solid var(--ink); padding-bottom: 15px; margin-bottom: 25px; }
.drop-zone { border: 2px dashed var(--dim); padding: 40px; text-align: center; cursor: pointer; transition: 0.2s; }
.drop-zone:hover { background: var(--bg-soft); border-color: var(--ink); }
.drop-zone.has-file { background: #f1f8e9; border-color: #2e7d32; border-style: solid; }
.dark-input { width: 100%; border: 2px solid var(--border); padding: 15px; margin-top: 20px; height: 100px; font-family: inherit; font-size: 0.9rem; }
.modal-actions { margin-top: 30px; display: flex; justify-content: flex-end; gap: 20px; }
.modal-actions button { border: none; background: none; font-weight: 900; font-size: 1rem; cursor: pointer; padding: 10px 20px; }
.modal-actions button.submit { background: var(--ink); color: #fff; }
</style>