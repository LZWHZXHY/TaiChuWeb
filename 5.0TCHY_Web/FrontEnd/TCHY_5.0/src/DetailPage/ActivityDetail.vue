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
          <button class="back-to-archives" @click="$router.push('/activity-list')">
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
          <span class="meta-item">行动领域: {{ categoryLabel(data.category) }}</span>
          <span class="meta-item">运行模式: {{ typeLabelCn(data.type) }}</span>
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
              <div v-if="data.category === 'TRPG'" class="trpg-quick-specs">
                <div class="spec-row">
                  <span><strong>模组难度:</strong> {{ parsedMeta.difficulty || '未知' }}</span>
                  <span><strong>预计时长:</strong> {{ parsedMeta.duration || '未知' }}</span>
                </div>
                <div class="spec-row">
                  <span><strong>设定年代:</strong> {{ parsedMeta.era || '现代' }}</span>
                  <span><strong>推荐PL数:</strong> {{ parsedMeta.playerLimit || '不限' }}</span>
                </div>
                <div class="spec-full"><strong>车卡规则:</strong> {{ parsedMeta.buildRules || '见执行协议' }}</div>
                <div class="spec-full warn" v-if="parsedMeta.warnings"><strong>⚠ 避雷预警:</strong> {{ parsedMeta.warnings }}</div>
                <hr class="inner-divider">
              </div>
              <p class="desc-text">{{ data.desc || '等待情报录入...' }}</p>
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

          <div class="content-block workspace-block" v-if="data.verify === 1">
            <h3 class="block-title">03_战术沙盘 // ACTIVE_WORKSPACE</h3>
            
            <div v-if="data.category === 'TRPG'" class="trpg-workspace">
              <div class="sys-info">
                <span>规则引擎: {{ parsedMeta.system || '自定义规则' }}</span>
                <a v-if="parsedMeta.voice" :href="parsedMeta.voice" target="_blank" class="voice-btn">🔊 接入语音频道</a>
              </div>
              <div class="dice-roller-box">
                <div class="dice-display">{{ currentDiceResult || '--' }}</div>
                <div class="dice-controls">
                  <button @click="rollDice(100)" class="dice-btn">D100</button>
                  <button @click="rollDice(20)" class="dice-btn">D20</button>
                  <button @click="rollDice(10)" class="dice-btn">D10</button>
                  <button @click="rollDice(6)" class="dice-btn">D6</button>
                </div>
              </div>
            </div>

            <div v-else-if="data.category === 'ART' || data.category === 'ANIMATION'" class="art-workspace">
              <div class="sys-info">
                <span v-if="parsedMeta.canvas">画布: {{ parsedMeta.canvas }}</span>
                <span v-if="parsedMeta.fps">帧率: {{ parsedMeta.fps }} FPS</span>
              </div>
              <div class="gallery-preview">
                <div class="gallery-empty">视觉档案库部署中，等待节点上传数据...</div>
              </div>
            </div>

            <div v-else class="generic-workspace">
              <div class="gallery-empty">[ 当前模式无特殊可视化组件 ]</div>
            </div>
          </div>

          <div class="content-block">
            <h3 class="block-title">04_通讯交互记录 // COMMS_LOG</h3>
            <div class="comms-container" :class="{ 'comms-locked': data.verify !== 1 }">
              <UniversalComments 
                targetType="PROJECT" 
                :targetId="data.id"
                theme="minimal-light" 
                :readOnly="data.verify !== 1"
              />
              <div v-if="data.verify !== 1" class="lock-overlay">
                <div class="lock-box">
                  <div class="lock-icon">🔒</div>
                  <span class="lock-text">
                    {{ data.verify === 3 ? '[ 行动已结案：频道关闭 ]' : '[ 档案待核准：频道锁定 ]' }}
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
              v-if="isApproved && data.category === 'TRPG'"
              class="formal-btn primary warn-glow" 
              @click="$router.push(`/activity-play/${data.id}`)"
            >
              <span class="btn-text">进入战术区域</span>
            </button>


            <button class="formal-btn primary" @click="handleApplyEntry" :disabled="hasApplied || isSubmitting || data.verify !== 1">
              <span class="btn-text">{{ applyBtnText }}</span>
            </button>
            
            <button class="formal-btn secondary" @click="triggerUpload" :disabled="!isApproved || data.verify !== 1">
              <span class="btn-text">提交数据包 // SUBMIT_DATA</span>
            </button>
            <input type="file" ref="fileInput" style="display: none" @change="onFileChange" />
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

          <div class="roster-box">
            <div class="box-title">人员名册 // PERSONNEL_ROSTER</div>
            <div class="unit-list custom-scroll">
              <div v-for="user in participants" :key="user.id" class="unit-entry">
                <div class="u-avatar">
                  <CommonAvatar :userId="user.id" :passedAvatar="user.avatar" :showLevel="false" />
                </div>
                <div class="u-info">
                  <div class="u-name">{{ user.username }}</div>
                  <div class="u-tag">{{ statusOptions[user.prodStatus] }}</div>
                </div>
                <div class="u-icon" v-if="user.ocMasterId" title="已携角色卡出战">🎴</div>
                <div class="u-icon" v-else>{{ statusIcons[user.prodStatus] }}</div>
              </div>
            </div>
          </div>
        </aside>
      </div>
    </div>

    <OcSelectorModal v-model="showOcSelector" @selected="submitApplyRequest" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import CommonAvatar from '@/GeneralComponents/UserAvatar.vue'
import UniversalComments from '@/GeneralComponents/UniversalComments.vue'
import OcSelectorModal from '@/GeneralComponents/OcSelectorModal.vue'
import { ElMessage, ElNotification } from 'element-plus'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const data = ref(null)
const tags = ref([]) 
const participants = ref([])
const loadingParticipants = ref(false)
const isSubmitting = ref(false)
const currentTime = ref('')
const fileInput = ref(null) 
const currentDiceResult = ref(null)
const showOcSelector = ref(false)

const statusOptions = { 0: '行动中', 1: '已完成', 2: '咕咕咕' }
const statusIcons = { 0: '⏳', 1: '✅', 2: '🕊️' }

const padZero = (n) => (n < 10 && n !== null) ? `0${n}` : n
const categoryLabel = (c) => ({ ANIMATION: '动画/视频联合', ART: '视觉/绘圈企划', TRPG: '跑团/桌游', WORLD: '世界观共创' }[c] || '通用领域')
const typeLabelCn = (t) => {
  const map = { 1: '并行协作', 2: '串行接力', 6: '中长团', 7: '短团单次', 8: '文字团', 10: '沙盒共创' }
  return map[t] || '常规模式'
}
const shortDate = (d) => d ? d.substring(0, 10) : '202X-XX-XX'

const updateTime = () => { currentTime.value = new Date().toLocaleTimeString('zh-CN', { hour12: false }) }
let timer = setInterval(updateTime, 1000)

const currentUserParticipant = computed(() => participants.value.find(p => p.id === authStore.user?.id))
const hasApplied = computed(() => !!currentUserParticipant.value)
const isApproved = computed(() => currentUserParticipant.value?.status === 1)
const completedCount = computed(() => participants.value.filter(p => p.prodStatus == 1).length)
const guguCount = computed(() => participants.value.filter(p => p.prodStatus == 2).length)

const parsedMeta = computed(() => {
  if (!data.value?.metaJson) return {}
  try { return JSON.parse(data.value.metaJson) } catch (e) { return {} }
})

const applyBtnText = computed(() => {
  if (data.value?.verify === 0) return '档案解密中...'
  if (data.value?.verify === 3) return '行动已封档'
  if (isSubmitting.value) return '同步信号中...'
  return hasApplied.value ? '已接入指挥链路' : '请求接入行动'
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

const fetchData = async () => {
  try {
    const res = await apiClient.get(`Activity/${route.params.id}`)
    data.value = res.data.data 
    tags.value = res.data.tags || []
    fetchParticipants()
  } catch (err) { ElMessage.error('档案调取失败') }
}

const fetchParticipants = async () => {
  loadingParticipants.value = true
  try {
    const res = await apiClient.get(`Activity/${route.params.id}/participants`)
    participants.value = res.data.data || []
  } finally { loadingParticipants.value = false }
}

const rollDice = (max) => {
  const result = Math.floor(Math.random() * max) + 1
  currentDiceResult.value = `[D${max}] 结果: ${result}`
  ElMessage({ message: `🎲 掷骰判定: ${result}`, type: 'success', plain: true })
}

const handleApplyEntry = () => {
  if (!authStore.checkAuth()) return router.push({ name: 'Login' })
  if (data.value.verify !== 1) return
  if (parsedMeta.value.requireOC) {
    showOcSelector.value = true
  } else {
    submitApplyRequest(null)
  }
}

const submitApplyRequest = async (ocId) => {
  isSubmitting.value = true
  try {
    const payload = { eventId: data.value.id, description: "Link Request", ocMasterId: ocId }
    await apiClient.post('Activity/apply', payload)
    ElNotification({ title: '系统提示', message: '已发出接入申请', type: 'success' })
    fetchParticipants()
  } catch (err) { ElMessage.error('申请失败') }
  finally { isSubmitting.value = false }
}

const triggerUpload = () => fileInput.value.click()
const onFileChange = async (event) => {
  const file = event.target.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  formData.append('eventId', data.value.id)
  try {
    await apiClient.post('Activity/submit-work-file', formData)
    ElMessage.success('归档成功')
    fetchParticipants()
  } catch (err) { ElMessage.error('归档失败') }
}

onMounted(() => { fetchData(); updateTime() })
onUnmounted(() => { clearInterval(timer) })
</script>

<style scoped>
.dossier-page {
  --ink: #111; --paper: #fff; --accent: #D92323; --approved: #1b8a4a;
  width: 100%; min-height: 100vh; background-color: #e0e0e0; display: flex; justify-content: center; padding: 60px 20px; position: relative;
}

/* 纹理背景 */
.paper-texture { position: fixed; inset: 0; pointer-events: none; opacity: 0.05; background-image: url("https://www.transparenttextures.com/patterns/paper-fibers.png"); }
.drafting-grid { position: fixed; inset: 0; pointer-events: none; opacity: 0.1; background-image: linear-gradient(#999 1px, transparent 1px), linear-gradient(90deg, #999 1px, transparent 1px); background-size: 32px 32px; }
.scanline-effect { position: fixed; inset: 0; pointer-events: none; background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.05) 50%); background-size: 100% 4px; z-index: 100; }

.dossier-wrapper { 
  width: 100%; max-width: 1100px; background: var(--paper); box-shadow: 20px 20px 0 rgba(0,0,0,0.1); 
  padding: 60px 80px; position: relative; border: 1px solid #aaa; z-index: 10;
}

/* 装饰 */
.paper-clip { position: absolute; top: -15px; left: 15%; width: 40px; height: 100px; border: 4px solid #999; border-radius: 20px; opacity: 0.4; }
.top-binding { position: absolute; top: 0; left: 0; right: 0; height: 15px; background: repeating-linear-gradient(90deg, #333, #333 10px, #555 10px, #555 20px); }

/* 页眉 */
.dossier-header { display: flex; justify-content: space-between; align-items: flex-start; }
.back-to-archives { background: var(--ink); color: #fff; border: none; padding: 8px 15px; font-weight: bold; cursor: pointer; font-size: 12px; }
.serial-no .lbl { font-size: 12px; color: #999; }
.serial-no .val { font-size: 1.4rem; font-weight: 900; border-bottom: 3px solid #111; }

.stamp { 
  border: 4px double #ccc; color: #ccc; padding: 10px 20px; font-weight: 900; font-size: 1.1rem; 
  transform: rotate(-10deg); display: inline-block;
}
.stamp.active { border-color: var(--approved); color: var(--approved); }
.stamp.rejected { border-color: var(--accent); color: var(--accent); }

.heavy-line { height: 8px; background: #111; border: none; margin: 30px 0; }

/* 标题 */
.subject-meta { display: flex; gap: 20px; margin-bottom: 10px; }
.meta-item { font-size: 12px; font-weight: bold; color: #666; border-left: 3px solid var(--accent); padding-left: 8px; }
.mission-title { font-size: 3.8rem; font-weight: 900; line-height: 1; margin: 0; letter-spacing: -2px; }
.tag-list { margin-top: 15px; display: flex; gap: 8px; }
.tag-item { font-size: 11px; background: #eee; padding: 4px 10px; font-weight: 900; }

/* 布局 */
.main-content-layout { display: grid; grid-template-columns: 1fr 320px; gap: 50px; margin-top: 40px; }

/* 正文块 */
.content-block { margin-bottom: 40px; }
.block-title { font-size: 14px; font-weight: 900; border-bottom: 2px solid #111; padding-bottom: 5px; margin-bottom: 20px; }
.text-area { line-height: 1.8; font-size: 15px; text-align: justify; }

.trpg-quick-specs { background: #f5f5f5; padding: 20px; border-radius: 4px; margin-bottom: 20px; border-left: 6px solid #111; }
.spec-row { display: flex; justify-content: space-between; margin-bottom: 8px; }
.spec-full { margin-top: 8px; padding-top: 8px; border-top: 1px dashed #ccc; }

.code-font { background: #f9f9f9; padding: 20px; font-family: monospace; border: 1px solid #ddd; }
.code-line { display: flex; gap: 15px; border-bottom: 1px dashed #eee; padding: 2px 0; }
.line-index { color: #ccc; }

/* 战术沙盘 */
.workspace-block { border: 2px dashed #111; padding: 25px; background: #fff; }
.sys-info { display: flex; justify-content: space-between; font-size: 12px; font-weight: bold; margin-bottom: 15px; background: #eee; padding: 8px; }
.dice-roller-box { background: #111; color: #00F0FF; padding: 30px; text-align: center; }
.dice-display { font-size: 3rem; font-weight: 900; margin-bottom: 20px; text-shadow: 0 0 10px #00F0FF; }
.dice-btn { background: transparent; border: 1px solid #00F0FF; color: #00F0FF; padding: 8px 15px; cursor: pointer; margin: 0 5px; }

/* 评论锁定 */
.comms-container { position: relative; min-height: 200px; }
.comms-locked { filter: grayscale(1) blur(2px); pointer-events: none; }
.lock-overlay { position: absolute; inset: 0; background: rgba(255,255,255,0.7); display: flex; justify-content: center; align-items: center; }
.lock-box { background: #fff; border: 3px solid #111; padding: 20px; text-align: center; box-shadow: 10px 10px 0 #111; }

/* 侧边栏 */
.host-profile-box { border: 2px solid #111; padding: 15px; margin-bottom: 25px; }
.h-card { display: flex; gap: 15px; align-items: center; }
.h-avatar { width: 50px; height: 50px; border: 2px solid #111; }
.h-name { font-weight: 900; }

.formal-btn { width: 100%; padding: 15px; font-weight: 900; border: 2px solid #111; cursor: pointer; margin-bottom: 15px; box-shadow: 6px 6px 0 #111; }
.formal-btn.primary { background: #111; color: #fff; }
.formal-btn:disabled { background: #eee; color: #999; box-shadow: none; cursor: not-allowed; }

.stats-box { border: 1px solid #ddd; padding: 15px; margin-bottom: 25px; }
.stat-row { display: flex; justify-content: space-between; font-size: 11px; font-weight: 900; }
.progress-container { height: 8px; background: #eee; margin: 10px 0; }
.progress-bar { height: 100%; background: #111; }
.stat-grid { display: grid; grid-template-columns: repeat(3, 1fr); text-align: center; padding-top: 10px; }
.s-cell .v { font-size: 1.4rem; font-weight: 900; display: block; }
.s-cell .k { font-size: 10px; color: #999; }

.roster-box { border: 1px solid #ddd; padding: 15px; }
.unit-entry { display: flex; align-items: center; gap: 10px; padding: 8px 0; border-bottom: 1px dashed #eee; }
.u-avatar { width: 35px; height: 35px; border: 1px solid #eee; }
.u-name { font-weight: bold; font-size: 13px; }
.u-tag { font-size: 10px; color: #999; }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>