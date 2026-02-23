<template>
  <div class="dossier-page">
    <div class="paper-texture"></div>
    <div class="drafting-grid"></div>
    <div class="scanline-effect"></div>

    <div class="dossier-wrapper" v-if="data">
      <div class="paper-clip"></div>
      <div class="top-binding"></div>

      <header class="dossier-header">
        <div class="header-left">
          <button class="back-to-archives" @click="$router.back()">
            <span class="arrow">←</span> [ 返回档案库 // RETURN_TO_ARCHIVES ]
          </button>
          
          <div class="serial-no">
            <span class="lbl">REF_NO.</span>
            <span class="val">THCY-OP-{{ padZero(data.id) }}</span>
          </div>
        </div>

        <div class="header-right">
          <div class="clearance-level">
            <span class="stamp" :class="getStatusStampClass(data.verify)">
              {{ getStatusStampText(data.verify) }}
            </span>
          </div>
          <div class="timestamp-small">记录时间: {{ currentTime }}</div>
        </div>
      </header>

      <hr class="heavy-line" />

      <section class="title-section">
        <div class="subject-meta">
          <span class="meta-item">行动性质: {{ typeLabelCn(data.type) }}</span>
          <span class="meta-item">发起单位: {{ data.host }}</span>
          <span class="meta-item">建档日期: {{ shortDate(data.startdate) }}</span>
        </div>
        <h1 class="mission-title">{{ data.name }}</h1>
        
        <div class="tag-list" v-if="tags && tags.length">
          <span v-for="tag in tags" :key="tag" class="tag-item"># {{ tag }}</span>
        </div>
      </section>

      <div class="main-content-layout">
        <main class="report-body">
          <div class="content-block">
            <h3 class="block-title">01_任务简报 // MISSION_SUMMARY</h3>
            <div class="text-area briefing">
              {{ data.desc || '等待情报录入...' }}
            </div>
          </div>

          <div class="content-block" v-if="data.require">
            <h3 class="block-title">02_执行协议 // PROTOCOLS</h3>
            <div class="text-area code-font">
              <div v-for="(line, index) in data.require.split('\n')" :key="index" class="code-line">
                <span class="line-index">{{ padZero(index + 1) }}</span>
                <span class="line-content">{{ line }}</span>
              </div>
            </div>
          </div>

          <div class="content-block">
            <h3 class="block-title">03_通讯交互记录 // COMMS_LOG</h3>
            <div class="comms-container" :class="{ 'comms-locked': data.verify !== 1 }">
              <UniversalComments 
                targetType="JOINT" 
                :targetId="data.id"
                theme="minimal-light" 
                :readOnly="data.verify !== 1"
              />
              <div v-if="data.verify !== 1" class="lock-overlay">
                <div class="lock-box">
                  <div class="lock-icon">🔒</div>
                  <span class="lock-text">
                    {{ data.verify === 3 ? '[ 行动已结案：通讯频道永久关闭 ]' : '[ 档案待核准：通讯频道锁定 ]' }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </main>

        <aside class="metadata-sidebar">
          <div class="host-profile-box">
            <div class="box-title">发起单位 // INITIATOR</div>
            <div class="h-card">
              <div class="h-avatar">
                <CommonAvatar :userId="data.userId" :showLevel="true" />
              </div>
              <div class="h-info">
                <div class="h-name">{{ data.host }}</div>
                <div class="h-id">ID_REF: {{ padZero(data.userId) }}</div>
              </div>
            </div>
          </div>

          <div class="action-box">
            <button 
              class="formal-btn primary" 
              @click="handleOneClickApply" 
              :disabled="hasApplied || isSubmitting || data.verify !== 1"
            >
              <span class="btn-text">{{ applyBtnText }}</span>
            </button>
            
            <button 
              class="formal-btn secondary" 
              @click="triggerUpload" 
              :disabled="!isApproved || data.verify !== 1"
            >
              <span class="btn-text">提交数据包 // SUBMIT_FILE</span>
            </button>
            <input 
              type="file" 
              ref="fileInput" 
              style="display: none" 
              @change="onFileChange"
              accept=".zip,.rar,.7z,.pdf,.png,.jpg"
            />
          </div>

          <div class="stats-box">
            <div class="stat-row">
              <span class="l">剩余期限 // DEADLINE</span>
              <span class="r highlight">{{ daysRemaining }} DAYS</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" :style="{ width: progressPercentage + '%' }"></div>
              <div class="progress-glow"></div>
            </div>
            <div class="stat-grid">
              <div class="s-cell">
                <span class="v">{{ participants.length }}</span>
                <span class="k">部署单位</span>
              </div>
              <div class="s-cell">
                <span class="v success">{{ completedCount }}</span>
                <span class="k">已达成</span>
              </div>
              <div class="s-cell">
                <span class="v warning">{{ guguCount }}</span>
                <span class="k">失联</span>
              </div>
            </div>
          </div>

          <div class="status-manager" v-if="currentUserParticipant && isApproved && data.verify === 1">
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
            <div class="box-title">人员名册 // PERSONNEL_ROSTER</div>
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
              <div v-if="!participants.length" class="empty-roster">暂无单位部署</div>
            </div>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import CommonAvatar from '@/GeneralComponents/UserAvatar.vue'
import UniversalComments from '@/GeneralComponents/UniversalComments.vue'
// 注意：此处假设你安装了 element-plus 以提供消息反馈，如果没有，可以替换为原生 alert
import { ElMessage, ElNotification } from 'element-plus'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// 状态变量
const data = ref(null)
const tags = ref([]) 
const participants = ref([])
const loadingParticipants = ref(false)
const isSubmitting = ref(false)
const currentTime = ref('')
const fileInput = ref(null) // 文件上传引用

const statusOptions = { 0: '行动中', 1: '已完成', 2: '咕咕咕' }
const statusIcons = { 0: '⏳', 1: '✅', 2: '🕊️' }

// 辅助函数
const padZero = (n) => (n < 10 ? `0${n}` : n)
const typeLabelCn = (t) => ({ 1: '联合行动', 2: '接力计划', 3: '创作竞技', 4: '同屏同步', 5: '测试项目' }[t] || '常规行动')
const shortDate = (d) => d ? d.substring(0, 10) : '202X-XX-XX'

// 时钟逻辑
const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString('zh-CN', { hour12: false })
}
let timer = setInterval(updateTime, 1000)

// 计算属性
const currentUserParticipant = computed(() => participants.value.find(p => p.id === authStore.user?.id))
const hasApplied = computed(() => !!currentUserParticipant.value)
const isApproved = computed(() => currentUserParticipant.value?.status === 1)
const completedCount = computed(() => participants.value.filter(p => p.prodStatus == 1).length)
const guguCount = computed(() => participants.value.filter(p => p.prodStatus == 2).length)

const applyBtnText = computed(() => {
  if (data.value?.verify === 0) return '档案审核中...'
  if (data.value?.verify === 3) return '行动已结案'
  if (isSubmitting.value) return '信号同步中...'
  return hasApplied.value ? '已接入指挥链路' : '申请接入行动'
})

const getStatusStampText = (v) => ({ 0: 'PENDING', 1: 'ACTIVE', 2: 'REJECTED', 3: 'ARCHIVED' }[v])
const getStatusStampClass = (v) => ({ 0: 'pending', 1: 'active', 2: 'rejected', 3: 'closed' }[v])

const daysRemaining = computed(() => {
  if (!data.value?.enddate) return 'INF'
  const diff = new Date(data.value.enddate).getTime() - Date.now()
  return diff < 0 ? 0 : Math.ceil(diff / 86400000)
})

const progressPercentage = computed(() => {
  if (!data.value) return 0
  const start = new Date(data.value.startdate).getTime()
  const end = data.value.enddate ? new Date(data.value.enddate).getTime() : start + 2592000000
  const now = Date.now()
  return Math.min(100, Math.max(0, ((now - start) / (end - start)) * 100))
})

// --- 核心业务逻辑 ---

const fetchData = async () => {
  try {
    const res = await apiClient.get(`ChaiLianHe/${route.params.id}`)
    data.value = res.data.data 
    tags.value = res.data.tags || []
    fetchParticipants()
  } catch (err) {
    console.error('档案调取失败:', err)
  }
}

const fetchParticipants = async () => {
  loadingParticipants.value = true
  try {
    const res = await apiClient.get(`ChaiLianHe/${route.params.id}/participants`)
    participants.value = res.data.data || []
  } catch (err) {
    console.error('名册读取失败:', err)
  } finally {
    loadingParticipants.value = false
  }
}

// 申请接入
const handleOneClickApply = async () => {
  if (!authStore.checkAuth()) return router.push({ name: 'Login' })
  if (data.value.verify !== 1) return
  
  isSubmitting.value = true
  try {
    await apiClient.post('ChaiLianHe/apply', { eventId: data.value.id })
    ElNotification({ title: '系统提示', message: '已接入行动指挥链路', type: 'success' })
    fetchParticipants()
  } catch (err) {
    ElMessage.error('接入失败: 信号被防火墙拦截')
  } finally {
    isSubmitting.value = false
  }
}

// 状态更新
const updateProdStatus = async (status) => {
  try {
    await apiClient.post('ChaiLianHe/update-prod-status', { eventId: data.value.id, prodStatus: status })
    fetchParticipants()
    ElMessage.success('状态更新成功')
  } catch (err) {
    ElMessage.error('更新失败')
  }
}

// --- 文件上传逻辑实现 ---
const triggerUpload = () => {
  fileInput.value.click()
}
// 修改后的前端上传逻辑
const onFileChange = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file) // 对应后端的 IFormFile file
  formData.append('eventId', data.value.id) // 对应后端的 int eventId
  // 如果需要，也可以 append('description', '...')

  try {
    ElMessage.info('数据包同步中...')
    // 重点：URL 必须与后端 [HttpPost("submit-work-file")] 一致
    const res = await apiClient.post('ChaiLianHe/submit-work-file', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    ElMessage.success('数据包已存入档案库')
    fetchParticipants() // 刷新人员名册以显示新状态
  } catch (err) {
    // 处理 405 或其他错误
    if (err.response?.status === 405) {
      ElMessage.error('请求方法错误 (405): 请确认后端路由地址')
    } else {
      ElMessage.error('同步失败：链路中断')
    }
  } finally {
    event.target.value = '' 
  }
}

onMounted(() => {
  fetchData()
  updateTime()
})

onUnmounted(() => {
  clearInterval(timer)
})
</script>

<style scoped>
/* 视觉核心系统 
   采用“绝密档案”+“重工业控制台”风格
*/

.dossier-page {
  --ink: #111111; 
  --paper: #ffffff; 
  --dim: #666; 
  --accent: #D92323; 
  --border-heavy: #111; 
  --bg-site: #e0e0e0;
  --approved: #1b8a4a;
  
  width: 100%; 
  min-height: 100vh; 
  background-color: var(--bg-site); 
  color: var(--ink);
  font-family: "PingFang SC", "Hiragino Sans GB", "Microsoft YaHei", monospace; 
  display: flex; 
  justify-content: center;
  padding: 60px 20px; 
  position: relative;
}

/* 背景纹理 */
.paper-texture { position: fixed; inset: 0; pointer-events: none; opacity: 0.05; background-image: url("https://www.transparenttextures.com/patterns/paper-fibers.png"); }
.drafting-grid { position: fixed; inset: 0; pointer-events: none; opacity: 0.1; background-image: linear-gradient(#999 1px, transparent 1px), linear-gradient(90deg, #999 1px, transparent 1px); background-size: 32px 32px; }
.scanline-effect { position: fixed; inset: 0; pointer-events: none; background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.05) 50%), linear-gradient(90deg, rgba(255, 0, 0, 0.02), rgba(0, 255, 0, 0.01), rgba(0, 0, 255, 0.02)); background-size: 100% 4px, 3px 100%; z-index: 100; }

/* 档案容器 */
.dossier-wrapper { 
  width: 100%; 
  max-width: 1100px; 
  background: var(--paper); 
  box-shadow: 20px 20px 0 rgba(0,0,0,0.1); 
  padding: 60px 80px; 
  position: relative; 
  border: 1px solid #aaa; 
  z-index: 10;
}

.paper-clip {
  position: absolute; top: -15px; left: 15%; width: 40px; height: 100px;
  border: 4px solid #999; border-radius: 20px; opacity: 0.4; pointer-events: none;
}

/* 页眉 */
.dossier-header { display: flex; justify-content: space-between; align-items: flex-start; }
.back-to-archives { 
  background: var(--ink); color: #fff; border: none; padding: 5px 12px; 
  font-size: 0.7rem; font-weight: bold; cursor: pointer; transition: 0.2s;
}
.back-to-archives:hover { background: var(--accent); letter-spacing: 1px; }

.serial-no { font-family: 'Courier New', monospace; margin-top: 15px; }
.serial-no .lbl { font-size: 0.8rem; color: var(--dim); margin-right: 5px; }
.serial-no .val { font-size: 1.2rem; font-weight: 900; border-bottom: 3px solid var(--ink); }

.stamp { 
  border: 4px double #ccc; color: #ccc; padding: 8px 15px; font-weight: 900; font-size: 0.9rem; 
  transform: rotate(-10deg); display: inline-block; text-transform: uppercase;
}
.stamp.active { border-color: var(--approved); color: var(--approved); }
.stamp.rejected { border-color: var(--accent); color: var(--accent); }
.timestamp-small { font-size: 10px; color: var(--dim); margin-top: 10px; font-family: monospace; }

.heavy-line { height: 6px; background: var(--ink); border: none; margin: 25px 0; }

/* 标题区 */
.subject-meta { margin-bottom: 15px; display: flex; gap: 20px; }
.meta-item { font-size: 0.8rem; font-weight: bold; color: var(--dim); border-left: 3px solid var(--accent); padding-left: 8px; }
.mission-title { font-size: 3.5rem; font-weight: 900; line-height: 1; margin: 0; letter-spacing: -2px; }
.tag-list { margin-top: 20px; display: flex; gap: 10px; }
.tag-item { font-size: 11px; font-weight: 900; color: var(--dim); background: #eee; padding: 3px 10px; border-radius: 2px; }

/* 布局布局 */
.main-content-layout { display: grid; grid-template-columns: 1fr 340px; gap: 60px; margin-top: 40px; }

/* 正文 */
.block-title { 
  font-size: 1rem; font-weight: 900; border-bottom: 2px solid var(--ink); 
  padding-bottom: 8px; margin-bottom: 25px; display: flex; align-items: center;
}
.block-title::before { content: ""; width: 8px; height: 1.2em; background: var(--accent); margin-right: 10px; }

.text-area { line-height: 1.8; color: #222; font-size: 1.05rem; text-align: justify; }
.code-font { 
  background: #f7f7f7; padding: 25px; border: 1px solid #ddd; 
  font-family: "Consolas", "Monaco", monospace;
}
.code-line { display: flex; margin-bottom: 4px; border-bottom: 1px dashed #eee; }
.line-index { color: #bbb; width: 30px; font-size: 12px; user-select: none; }

/* 锁屏 */
.comms-container { position: relative; min-height: 200px; border: 1px solid #eee; padding: 10px; }
.comms-locked { filter: blur(2px) grayscale(1); pointer-events: none; }
.lock-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.7); display: flex; justify-content: center; align-items: center; z-index: 50; }
.lock-box { text-align: center; background: #fff; border: 2px solid var(--ink); padding: 20px; box-shadow: 10px 10px 0 var(--ink); }
.lock-icon { font-size: 2rem; margin-bottom: 10px; }
.lock-text { font-weight: 900; font-size: 0.8rem; color: var(--accent); text-transform: uppercase; }

/* 侧边栏按钮 */
.action-box { margin-bottom: 30px; display: flex; flex-direction: column; gap: 15px; }
.formal-btn {
  width: 100%; position: relative; padding: 18px; 
  font-weight: 900; font-size: 0.95rem; cursor: pointer;
  border: 2px solid var(--ink); text-transform: uppercase;
  transition: 0.1s; display: flex; align-items: center; justify-content: center;
  box-shadow: 6px 6px 0 var(--ink);
}
.formal-btn:hover:not(:disabled) { transform: translate(-2px, -2px); box-shadow: 8px 8px 0 var(--ink); }
.formal-btn:active:not(:disabled) { transform: translate(4px, 4px); box-shadow: 2px 2px 0 var(--ink); }
.formal-btn:disabled { background: #ddd; color: #999; border-color: #ccc; box-shadow: none; cursor: not-allowed; }

.formal-btn.primary { background: var(--ink); color: #fff; }
.formal-btn.primary:hover:not(:disabled) { background: var(--accent); border-color: var(--accent); }
.formal-btn.secondary { background: #fff; color: var(--ink); }

/* 侧边栏组件 */
.host-profile-box { margin-bottom: 30px; border: 2px solid var(--ink); padding: 15px; background: #fafafa; }
.h-card { display: flex; align-items: center; gap: 12px; }
.h-avatar { width: 50px; height: 50px; border: 1px solid var(--ink); }
.h-info .h-name { font-weight: 900; }
.h-info .h-id { font-size: 11px; color: var(--dim); font-family: monospace; }

.stats-box { border: 1px solid #ddd; padding: 20px; margin-bottom: 25px; position: relative; overflow: hidden; }
.stat-row { display: flex; justify-content: space-between; font-size: 0.75rem; font-weight: 900; margin-bottom: 10px; }
.progress-container { height: 10px; background: #eee; margin-bottom: 20px; position: relative; }
.progress-bar { height: 100%; background: var(--ink); transition: width 0.8s cubic-bezier(0.22, 1, 0.36, 1); }
.progress-glow { position: absolute; top: 0; left: 0; height: 100%; width: 30px; background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent); animation: sweep 2s infinite; }

@keyframes sweep { 0% { left: -30px; } 100% { left: 100%; } }

.stat-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; border-top: 1px solid #eee; padding-top: 15px; }
.s-cell { text-align: center; }
.s-cell .v { display: block; font-size: 1.5rem; font-weight: 900; }
.s-cell .k { font-size: 0.6rem; color: var(--dim); }
.success { color: var(--approved); }
.warning { color: var(--accent); }

/* 状态切换按钮 */
.status-manager { margin-bottom: 25px; }
.radio-group { display: flex; border: 2px solid var(--ink); }
.radio-btn { 
  flex: 1; border: none; background: #fff; padding: 10px 5px; 
  cursor: pointer; font-size: 12px; font-weight: 900; 
  border-right: 2px solid var(--ink); transition: 0.2s;
}
.radio-btn:last-child { border-right: none; }
.radio-btn.active { background: var(--ink); color: #fff; }
.radio-btn:hover:not(.active) { background: #eee; }

/* 人员名册 */
.roster-box { border: 1px solid #ddd; padding: 15px; background: #fff; }
.unit-list { max-height: 250px; overflow-y: auto; padding-right: 5px; }
.unit-entry { display: flex; align-items: center; padding: 10px 0; border-bottom: 1px dashed #eee; }
.u-avatar { width: 35px; height: 35px; margin-right: 12px; border: 1px solid #eee; }
.u-info { flex: 1; }
.u-name { font-weight: 900; font-size: 0.85rem; }
.u-tag { font-size: 10px; color: var(--dim); }
.empty-roster { font-size: 12px; color: #bbb; text-align: center; padding: 20px; }

/* 响应式调整 */
@media (max-width: 900px) {
  .dossier-wrapper { padding: 30px; }
  .main-content-layout { grid-template-columns: 1fr; }
  .mission-title { font-size: 2.2rem; }
}

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--ink); }
</style>