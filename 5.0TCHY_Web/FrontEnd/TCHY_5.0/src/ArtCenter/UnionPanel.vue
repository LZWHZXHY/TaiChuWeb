<template>
  <div class="joint-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="joint-container">
      
      <header class="command-console">
        <div class="console-row top">
          <div class="title-block">
            <span class="icon-box">JOINT</span>
            <div class="text-group">
              <h2>JOINT_OPERATIONS</h2>
              <span class="sub">联合作战看板 // 柴圈联合</span>
            </div>
          </div>
          
          <div class="action-block">
            <button class="cyber-btn primary" @click="openCreate">
              <span class="btn-icon">+</span> INITIATE_NEW_OP
            </button>
          </div>
        </div>

        <div class="console-row bottom">
          <div class="search-terminal">
            <span class="prompt">> SEARCH:</span>
            <input 
              v-model="q" 
              type="text" 
              class="terminal-input"
              placeholder="INPUT_KEYWORDS..." 
              @input="onSearchInput" 
            />
            <div class="scan-line"></div>
          </div>

          <div class="filter-deck">
            <div class="toggle-switch">
              <div class="switch-opt" :class="{ active: tab === 'ongoing' }" @click="switchTab('ongoing')">ACTIVE</div>
              <div class="switch-opt" :class="{ active: tab === 'finished' }" @click="switchTab('finished')">ARCHIVED</div>
            </div>
            <div class="cyber-select">
              <span class="label">TYPE:</span>
              <select v-model="typeFilter" @change="onFilterChange">
                <option value="">ALL_CLASS</option>
                <option :value="1">COLLAB // 联合</option>
                <option :value="2">RELAY // 接力</option>
                <option :value="3">MATCH // 比赛</option>
              </select>
            </div>
            <div class="cyber-select">
              <span class="label">SORT:</span>
              <select v-model="sortBy" @change="onSortChange">
                <option value="">DEFAULT</option>
                <option value="startdate_desc">NEWEST</option>
                <option value="startdate_asc">OLDEST</option>
                <option value="enddate_asc">ENDING_SOON</option>
              </select>
            </div>
          </div>
        </div>
      </header>

      <div class="mission-grid-wrapper custom-scroll">
        
        <div v-if="loading" class="system-msg">
          <div class="loader-spinner"></div>
          <span>LOADING_TACTICAL_DATA...</span>
        </div>

        <div v-else-if="!items.length" class="system-msg error">
          [ NO_OPERATIONS_FOUND ]
        </div>

        <div v-else class="plate-grid">
          <article 
            v-for="it in items" 
            :key="it.id" 
            class="data-plate" 
            :class="`status-${it.verify}`"
          >
            <div class="plate-header">
              <div class="header-top">
                <span class="op-code">OP-{{ padZero(it.id) }}</span>
                <span class="verify-tag">{{ verifyLabelEn(it.verify) }}</span>
              </div>
              <h3 class="plate-title" :title="it.name">{{ it.name }}</h3>
            </div>

            <div class="data-cells">
              <div class="cell">
                <span class="c-label">TYPE</span>
                <span class="c-val type-hl">{{ typeLabelEn(it.type) }}</span>
              </div>
              <div class="cell">
                <span class="c-label">COMMANDER</span>
                <span class="c-val">{{ it.host }}</span>
              </div>
              <div class="cell" v-if="it.QQgroup">
                <span class="c-label">CHANNEL</span>
                <span class="c-val group-hl">{{ it.QQgroup }}</span>
              </div>
            </div>

            <div class="plate-desc">
              {{ it.desc || 'NO_INTEL_AVAILABLE...' }}
            </div>

            <div class="progress-section">
              <div class="prog-info">
                <span class="prog-label">MISSION_PROGRESS: {{ calculateProgress(it) }}%</span>
              </div>
              <div class="prog-track">
                <div class="prog-fill" :style="{ width: calculateProgress(it) + '%' }"></div>
                <div class="prog-grid-mask"></div>
              </div>
            </div>

            <div class="plate-footer">
              <div class="footer-time">
                <div class="time-row">S: {{ shortDate(it.startdate) }}</div>
                <div class="time-row">E: {{ it.enddate ? shortDate(it.enddate) : '∞' }}</div>
              </div>
              
              <div class="footer-actions">
                <button class="icon-action" title="Detail" @click="viewDetail(it)">
                  👁
                </button>
                
                <button 
                  v-if="tab === 'ongoing' && it.QQgroup" 
                  class="icon-action primary" 
                  title="Join Group"
                  @click="applyJoin(it)"
                >
                  QQ
                </button>
                
                <button 
                  class="icon-action primary" 
                  title="Apply"
                  @click="applyForEvent(it)"
                >
                  APPLY
                </button>

                <div v-if="isMine(it) || isAdmin" class="more-actions">
                  <button v-if="isMine(it)" class="icon-mini" @click="editItem(it)">✎</button>
                  <template v-if="isAdmin">
                    <button v-if="it.verify !== 1" class="icon-mini ok" @click="approveItem(it)">✓</button>
                    <button v-if="it.verify !== 2" class="icon-mini no" @click="rejectItem(it)">✕</button>
                    <button class="icon-mini del" @click="deleteItem(it.id)">🗑</button>
                  </template>
                </div>
              </div>
            </div>
            
            <div class="side-strip"></div>
          </article>
        </div>

        <footer class="pagination-bar" v-if="items.length > 0">
          <button class="nav-arrow" :disabled="page <= 1" @click="prevPage">&lt; PREV</button>
          <div class="page-indicator">PAGE: {{ padZero(page) }} / {{ padZero(totalPages) }}</div>
          <button class="nav-arrow" :disabled="page >= totalPages" @click="nextPage">NEXT &gt;</button>
        </footer>
      </div>

      <Teleport to="body">
        <Transition name="glitch-fade">
          <div v-if="detail" class="cyber-modal-overlay" @click.self="detail = null">
            <div class="cyber-terminal detail-mode">
              <div class="term-header">
                <span class="term-title">>> MISSION_INTEL // 详情</span>
                <button class="term-close" @click="detail = null">[ CLOSE ]</button>
              </div>
              <div class="term-content custom-scroll">
                <h1 class="detail-h1">{{ detail.name }}</h1>
                <div class="detail-meta-row">
                  <span class="meta-tag">TYPE: {{ typeLabelEn(detail.type) }}</span>
                  <span class="meta-tag">HOST: {{ detail.host }}</span>
                  <span class="meta-tag">STATUS: {{ verifyLabelEn(detail.verify) }}</span>
                </div>

                <div class="term-section">
                  <div class="sec-title"># BRIEFING // 简介</div>
                  <p class="sec-body">{{ detail.desc }}</p>
                </div>

                <div class="term-section" v-if="detail.rules">
                  <div class="sec-title warning"># PROTOCOLS // 规则</div>
                  <div class="code-block">{{ detail.rules }}</div>
                </div>

                <div class="term-section">
                  <div class="sec-title"># LOGISTICS // 后勤信息</div>
                  <div class="kv-grid">
                    <div class="kv"><span>START:</span> {{ formatDate(detail.startdate) }}</div>
                    <div class="kv"><span>END:</span> {{ detail.enddate ? formatDate(detail.enddate) : 'INDEFINITE' }}</div>
                    <div class="kv"><span>QQ_GROUP:</span> {{ detail.QQgroup || 'N/A' }}</div>
                  </div>
                </div>
              </div>
              <div class="term-actions">
                <button class="cyber-btn full" @click="applyForEvent(detail)">
                  >>> SIGN_UP_FOR_MISSION
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>

      <Teleport to="body">
        <Transition name="glitch-fade">
          <div v-if="showCreate" class="cyber-modal-overlay" @click.self="closeCreate">
            <div class="cyber-terminal form-mode">
              <div class="term-header">
                <span class="term-title">{{ editMode ? '>> UPDATE_LOG' : '>> NEW_OPERATION' }}</span>
                <button class="term-close" @click="closeCreate">[ ABORT ]</button>
              </div>
              <div class="term-content custom-scroll">
                <form @submit.prevent="submitForm" id="createForm" class="cyber-form">
                  <div class="form-row">
                    <label>OPERATION_TYPE:</label>
                    <select v-model.number="form.type" class="cyber-input">
                      <option :value="1">COLLAB (联合)</option>
                      <option :value="2">RELAY (接力)</option>
                      <option :value="3">MATCH (比赛)</option>
                    </select>
                  </div>
                  <div class="form-row">
                    <label>CODENAME / 标题:</label>
                    <input v-model="form.name" class="cyber-input" required placeholder="Enter Title..." />
                  </div>
                  <div class="form-row split">
                    <div>
                      <label>COMMANDER / 发起人:</label>
                      <input v-model="form.host" class="cyber-input" required />
                    </div>
                    <div>
                      <label>COMMS_CHANNEL / 群号:</label>
                      <input v-model="form.QQgroup" class="cyber-input" />
                    </div>
                  </div>
                  <div class="form-row split">
                    <div>
                      <label>START_TIME:</label>
                      <input v-model="form.startdate" type="datetime-local" class="cyber-input" required />
                    </div>
                    <div>
                      <label>END_TIME:</label>
                      <input v-model="form.enddate" type="datetime-local" class="cyber-input" />
                    </div>
                  </div>
                  <div class="form-row">
                    <label>BRIEFING / 简介:</label>
                    <textarea v-model="form.desc" class="cyber-input" rows="3"></textarea>
                  </div>
                  <div class="form-row">
                    <label>PROTOCOLS / 规则:</label>
                    <textarea v-model="form.rules" class="cyber-input" rows="3"></textarea>
                  </div>
                </form>
              </div>
              <div class="term-actions">
                <button class="cyber-btn ghost" @click="closeCreate">CANCEL</button>
                <button class="cyber-btn primary" type="submit" form="createForm" :disabled="submitting">
                  {{ submitting ? 'PROCESSING...' : 'EXECUTE' }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>

      <Teleport to="body">
        <Transition name="glitch-fade">
          <div v-if="showApplyModal" class="cyber-modal-overlay" @click.self="closeApplyModal">
            <div class="cyber-terminal form-mode">
              <div class="term-header">
                <span class="term-title">>> APPLICATION_FORM</span>
                <button class="term-close" @click="closeApplyModal">[ ABORT ]</button>
              </div>
              <div class="term-content custom-scroll">
                <div class="target-info">
                  APPLYING_TO: <span class="highlight">{{ selectedEvent?.name }}</span>
                </div>
                <form @submit.prevent="submitApplication" id="applyForm" class="cyber-form">
                  <div class="form-row">
                    <label>APPLICANT_ID:</label>
                    <input v-model="applicationForm.applicantName" class="cyber-input" required />
                  </div>
                  <div class="form-row">
                    <label>CONTACT_FREQ (QQ/Email):</label>
                    <input v-model="applicationForm.contact" class="cyber-input" required />
                  </div>
                  <div class="form-row">
                    <label>STATEMENT / 申请说明:</label>
                    <textarea v-model="applicationForm.description" class="cyber-input" rows="4" required></textarea>
                  </div>
                  <div class="form-row">
                    <label>PORTFOLIO_LINK (Optional):</label>
                    <input v-model="applicationForm.portfolioUrl" class="cyber-input" placeholder="https://..." />
                  </div>
                </form>
              </div>
              <div class="term-actions">
                <button class="cyber-btn ghost" @click="closeApplyModal">CANCEL</button>
                <button class="cyber-btn primary" type="submit" form="applyForm" :disabled="submittingApplication">
                  {{ submittingApplication ? 'TRANSMITTING...' : 'SEND_APPLICATION' }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>

    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const router = useRouter()
const auth = useAuthStore()

// State
const tab = ref('ongoing')
const q = ref('')
const typeFilter = ref('')
const sortBy = ref('')
const page = ref(1)
const pageSize = ref(12) 
const loading = ref(false)
const items = ref([])
const total = ref(0)

// Modal States
const detail = ref(null)
const showCreate = ref(false)
const editMode = ref(false)
const submitting = ref(false)
const showApplyModal = ref(false)
const selectedEvent = ref(null)
const submittingApplication = ref(false)

// Forms
const form = reactive({
  id: null, name: '', host: '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0, creatorId: null
})
const applicationForm = reactive({
  applicantName: '', contact: '', description: '', portfolioUrl: ''
})

// Helpers
const typeLabelEn = (t) => {
  const map = { 1: 'COLLAB', 2: 'RELAY', 3: 'MATCH' }
  return map[t] || 'UNKNOWN'
}
const verifyLabelEn = (v) => v === 0 ? 'PENDING' : v === 1 ? 'ACTIVE' : 'REJECTED'
const padZero = (n) => n < 10 ? `0${n}` : n

const isMine = (it) => auth.user?.id && it.creatorId === auth.user.id
const isAdmin = computed(() => auth.user?.roles?.includes?.('Admin'))
const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

const parseSortBy = (val) => {
  if (!val) return { field: '', order: '' }
  const [field, order] = val.split('_')
  return { field, order }
}

const formatDate = (d) => {
  if (!d) return 'N/A'
  const date = new Date(d)
  return `${date.getFullYear()}.${padZero(date.getMonth()+1)}.${padZero(date.getDate())} ${padZero(date.getHours())}:${padZero(date.getMinutes())}`
}
const shortDate = (d) => {
  if (!d) return 'N/A'
  const date = new Date(d)
  return `${date.getFullYear()}.${padZero(date.getMonth()+1)}.${padZero(date.getDate())}`
}

// 计算进度 (0-100)
const calculateProgress = (it) => {
  const start = new Date(it.startdate).getTime()
  // 如果没有结束时间，进度条默认显示 10% 表示进行中，或者根据开始时间显示一个伪进度
  if (!it.enddate) return 10 
  
  const end = new Date(it.enddate).getTime()
  const now = new Date().getTime()
  
  if (now < start) return 0
  const total = end - start
  const current = now - start
  
  if (total <= 0) return 100 // 数据异常防错
  if (current >= total) return 100
  
  return Math.floor((current / total) * 100)
}

// API Actions
const basePath = 'ChaiLianHe'

async function ensureAuth() {
  if (!auth.checkAuth()) {
    auth.clearAuthState()
    router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } })
    return false
  }
  return true
}

async function fetchList() {
  loading.value = true
  try {
    const sort = parseSortBy(sortBy.value)
    
    const params = {
      q: q.value || undefined,
      type: typeFilter.value !== '' ? typeFilter.value : undefined,
      verify: 1, 
      page: page.value,
      pageSize: pageSize.value,
      sortField: sort.field || undefined,
      sortOrder: sort.order || undefined
    }

    const resp = await apiClient.get(`${basePath}/list`, { params })
    
    if (resp.data?.data) {
        items.value = resp.data.data
        total.value = resp.data.total
    } else {
        items.value = []
        total.value = 0
    }
  } catch (err) {
    console.error("Fetch Error:", err)
    items.value = []
  } finally {
    loading.value = false
  }
}

const onSearchInput = () => { page.value = 1; fetchList() }
const onFilterChange = () => { page.value = 1; fetchList() }
const onSortChange = () => { page.value = 1; fetchList() }
const switchTab = (t) => { tab.value = t; page.value = 1; fetchList() } 
const prevPage = () => { if (page.value > 1) { page.value--; fetchList() } }
const nextPage = () => { if (page.value < totalPages.value) { page.value++; fetchList() } }

const viewDetail = (it) => { detail.value = it }

const applyJoin = (it) => {
  if (!it.QQgroup) return alert('NO_COMMS_CHANNEL')
  window.open(`https://jq.qq.com/?_=${encodeURIComponent(it.QQgroup)}`, '_blank')
}

const openCreate = async () => {
  if(!(await ensureAuth())) return
  editMode.value = false
  Object.assign(form, { 
    id: null, name: '', host: auth.user?.username || '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0 
  })
  showCreate.value = true
}

const editItem = async (it) => {
    if(!(await ensureAuth())) return
    try {
        Object.assign(form, it)
        if(form.startdate) form.startdate = form.startdate.substring(0, 16)
        if(form.enddate) form.enddate = form.enddate.substring(0, 16)
        editMode.value = true
        showCreate.value = true
    } catch(e) { alert('ERROR_LOADING_DATA') }
}

const submitForm = async () => {
  submitting.value = true
  try {
    const payload = { ...form }
    payload.startdate = new Date(form.startdate).toISOString()
    if(form.enddate) payload.enddate = new Date(form.enddate).toISOString()
    
    if (editMode.value) await apiClient.put(`${basePath}/${form.id}`, payload)
    else await apiClient.post(`${basePath}/create`, payload)
    
    showCreate.value = false
    fetchList()
  } catch (e) { alert('TRANSMISSION_FAILED') } 
  finally { submitting.value = false }
}

const applyForEvent = async (it) => {
  if(!(await ensureAuth())) return
  selectedEvent.value = it
  Object.assign(applicationForm, { applicantName: auth.user.username, contact: '', description: '', portfolioUrl: '' })
  showApplyModal.value = true
}

const submitApplication = async () => {
  submittingApplication.value = true
  try {
    await apiClient.post(`${basePath}/apply`, {
      eventId: selectedEvent.value.id,
      ...applicationForm,
      applyTime: new Date().toISOString()
    })
    alert('APPLICATION_SENT')
    closeApplyModal()
  } catch(e) { alert('SEND_FAILED') }
  finally { submittingApplication.value = false }
}

const closeCreate = () => showCreate.value = false
const closeApplyModal = () => showApplyModal.value = false

const deleteItem = async (id) => { if(confirm('CONFIRM_DELETION?')) { await apiClient.delete(`${basePath}/${id}`); fetchList() } }
const approveItem = async (it) => { if(confirm('CONFIRM_APPROVAL?')) { await apiClient.post(`${basePath}/moderation/approve`, { Id: it.id }); fetchList() } }
const rejectItem = async (it) => { 
    const reason = prompt('REJECTION_REASON:')
    if(reason) { await apiClient.post(`${basePath}/moderation/reject`, { Id: it.id, Note: reason }); fetchList() } 
}

onMounted(() => {
  auth.checkAuth()
  fetchList()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.joint-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%; height: 100%;
  font-family: var(--mono);
  color: var(--black);
  position: relative;
  overflow: hidden;
  display: flex; flex-direction: column;
}

/* 背景 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* 容器 */
.joint-container {
  position: relative; z-index: 1;
  flex: 1; display: flex; flex-direction: column;
  padding: 20px; gap: 20px;
}

/* --- 1. 顶部样式 --- */
.command-console {
  background: var(--white);
  border: 4px solid var(--black);
  padding: 15px 20px;
  display: flex; flex-direction: column; gap: 15px;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}
.console-row { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 15px; }
.console-row.bottom { align-items: flex-end; }
.title-block { display: flex; align-items: center; gap: 15px; }
.icon-box {
  background: var(--black); color: var(--white);
  font-family: var(--heading); font-size: 1.5rem;
  padding: 5px 10px; transform: skew(-10deg);
}
.text-group h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-group .sub { font-size: 0.7rem; font-weight: bold; color: #666; }
.search-terminal {
  flex: 1; max-width: 400px;
  display: flex; align-items: center;
  border-bottom: 2px solid var(--black);
  position: relative;
}
.prompt { font-weight: bold; margin-right: 10px; color: var(--red); }
.terminal-input {
  border: none; background: transparent; outline: none;
  font-family: var(--mono); font-size: 1rem; width: 100%;
  padding: 5px; color: var(--black);
}
.scan-line {
  position: absolute; bottom: -2px; left: 0; height: 2px; width: 0;
  background: var(--red); transition: width 0.3s;
}
.terminal-input:focus + .scan-line { width: 100%; }
.filter-deck { display: flex; gap: 20px; align-items: center; }
.toggle-switch {
  display: flex; border: 2px solid var(--black);
  background: #ccc;
}
.switch-opt {
  padding: 5px 15px; font-weight: bold; font-size: 0.8rem; cursor: pointer;
  transition: 0.2s;
}
.switch-opt.active { background: var(--black); color: var(--white); }
.switch-opt:hover:not(.active) { background: #fff; }
.cyber-select { display: flex; flex-direction: column; }
.cyber-select .label { font-size: 0.6rem; font-weight: bold; color: #666; margin-bottom: 2px; }
.cyber-select select {
  border: 2px solid var(--black); background: var(--white);
  font-family: var(--mono); font-weight: bold; padding: 2px 5px;
  outline: none; cursor: pointer;
}
.cyber-btn {
  border: none; padding: 10px 20px; font-family: var(--heading); font-size: 1.1rem;
  cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.2s;
  box-shadow: 4px 4px 0 #999;
}
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover { background: var(--red); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.cyber-btn.ghost { background: transparent; border: 2px solid var(--black); color: var(--black); }
.cyber-btn.ghost:hover { background: #eee; }
.cyber-btn.full { width: 100%; justify-content: center; }

/* --- 2. 任务列表样式 (自适应 + 进度条) --- */
.mission-grid-wrapper { flex: 1; overflow-y: auto; padding-right: 5px; }

.plate-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  padding-bottom: 30px;
}

/* 卡片 */
.data-plate {
  background: #fff;
  border: 2px solid var(--black);
  display: flex; flex-direction: column;
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1);
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
  height: auto; 
  min-height: 100%;
}
.data-plate:hover {
  transform: translateY(-4px);
  box-shadow: 8px 8px 0 var(--red);
  border-color: var(--black);
}

.side-strip {
  position: absolute; left: 0; top: 0; bottom: 0; width: 6px;
  background: var(--black);
}
.status-1 .side-strip { background: #27ae60; }
.status-2 .side-strip { background: var(--red); }
.status-0 .side-strip { background: #f39c12; }

/* 内容区域 */
.plate-header, .data-cells, .plate-desc, .progress-section {
  padding: 10px 15px 10px 20px; /* 左边留出 side-strip 空间 */
}

/* Header */
.plate-header { border-bottom: 1px dashed #ccc; }
.header-top { display: flex; justify-content: space-between; margin-bottom: 5px; font-size: 0.7rem; color: #888; }
.verify-tag { font-weight: bold; color: var(--black); text-transform: uppercase; }
.plate-title { 
  font-family: var(--heading); font-size: 1.5rem; margin: 0; 
  line-height: 1.1; 
  white-space: normal; overflow: visible; 
}

/* Data Cells */
.data-cells {
  display: flex; flex-wrap: wrap; gap: 15px; 
  background: #f9f9f9; border-bottom: 1px solid var(--black);
}
.cell { display: flex; flex-direction: column; min-width: 80px; flex: 1; }
.c-label { font-size: 0.6rem; font-weight: bold; color: #888; margin-bottom: 2px; }
.c-val { font-size: 0.8rem; font-weight: bold; color: var(--black); word-break: break-all; }
.type-hl { color: var(--red); }
.group-hl { font-family: monospace; }

/* Desc */
.plate-desc {
  font-size: 0.85rem; color: #555; line-height: 1.5;
  height: auto; overflow: visible; margin-bottom: 0; /* 下方有进度条，取消 margin */
}

/* 🔥 Progress Section */
.progress-section {
  padding-top: 0; /* 紧接描述 */
  padding-bottom: 15px;
}
.prog-info {
  font-size: 0.6rem; font-weight: bold; color: #888; margin-bottom: 4px;
}
.prog-track {
  height: 8px; width: 100%; background: #e0e0e0;
  border: 1px solid var(--black);
  position: relative;
  overflow: hidden;
}
.prog-fill {
  height: 100%; background: var(--black);
  transition: width 0.5s ease;
  position: relative; z-index: 1;
}
/* 状态色覆盖 */
.status-1 .prog-fill { background: #27ae60; }
.status-2 .prog-fill { background: var(--red); }

/* 刻度纹理 */
.prog-grid-mask {
  position: absolute; inset: 0; z-index: 2;
  background-image: repeating-linear-gradient(90deg, transparent, transparent 19px, #fff 19px, #fff 20px);
  opacity: 0.3; pointer-events: none;
}

/* Footer */
.plate-footer {
  margin-top: auto;
  background: var(--black);
  color: var(--white);
  padding: 10px 15px 10px 20px;
  display: flex; justify-content: space-between; align-items: center;
}
.footer-time { font-size: 0.7rem; font-family: monospace; color: #aaa; }
.time-row { line-height: 1.2; }
.footer-actions { display: flex; gap: 8px; align-items: center; }
.icon-action {
  background: #333; border: 1px solid #555; color: #fff;
  width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: 0.2s; font-size: 1rem;
}
.icon-action:hover { background: #fff; color: var(--black); border-color: #fff; }
.icon-action.primary { background: var(--red); border-color: var(--red); font-weight: bold; font-size: 0.7rem; width: auto; padding: 0 10px; }
.icon-action.primary:hover { background: #fff; color: var(--red); }
.more-actions { display: flex; gap: 4px; margin-left: 5px; padding-left: 5px; border-left: 1px solid #555; }
.icon-mini { background: transparent; border: none; color: #888; cursor: pointer; font-size: 0.8rem; padding: 2px; }
.icon-mini:hover { color: #fff; }
.icon-mini.del:hover { color: var(--red); }

/* Pagination & Msg */
.pagination-bar { display: flex; justify-content: center; align-items: center; gap: 20px; margin-top: 20px; }
.nav-arrow { background: none; border: none; font-weight: bold; cursor: pointer; font-family: var(--mono); }
.nav-arrow:hover:not(:disabled) { color: var(--red); }
.page-indicator { font-weight: bold; border: 2px solid var(--black); padding: 2px 10px; background: #fff; }
.system-msg { text-align: center; padding: 50px; color: #999; font-weight: bold; display: flex; flex-direction: column; align-items: center; gap: 10px; }
.loader-spinner { width: 20px; height: 20px; border: 3px solid #ccc; border-top-color: var(--black); border-radius: 50%; animation: spin 1s linear infinite; }

/* Modals (保持不变) */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
.cyber-terminal { width: 500px; max-width: 95vw; background: #f4f4f4; border: 4px solid var(--black); box-shadow: 15px 15px 0 rgba(0,0,0,0.5); display: flex; flex-direction: column; max-height: 85vh; }
.cyber-terminal.form-mode { width: 600px; }
.term-header { background: var(--black); color: var(--white); padding: 10px 15px; display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--red); }
.term-title { font-family: var(--heading); letter-spacing: 1px; }
.term-close { background: none; border: none; color: #aaa; cursor: pointer; font-family: var(--mono); font-weight: bold; }
.term-close:hover { color: var(--red); }
.term-content { padding: 25px; overflow-y: auto; flex: 1; }
.detail-h1 { font-family: var(--heading); font-size: 2rem; margin: 0 0 10px 0; line-height: 1; }
.detail-meta-row { display: flex; gap: 10px; margin-bottom: 20px; border-bottom: 1px dashed #ccc; padding-bottom: 10px; }
.meta-tag { background: #ddd; font-size: 0.7rem; padding: 2px 6px; font-weight: bold; }
.term-section { margin-bottom: 20px; }
.sec-title { font-weight: bold; margin-bottom: 5px; color: var(--black); border-left: 3px solid var(--black); padding-left: 8px; }
.sec-title.warning { border-color: var(--red); color: var(--red); }
.sec-body { font-size: 0.9rem; line-height: 1.6; color: #333; }
.code-block { background: #e0e0e0; padding: 10px; font-size: 0.85rem; border: 1px solid #ccc; color: #8b0000; }
.kv-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; background: #fff; padding: 10px; border: 1px solid #ccc; }
.kv span { font-weight: bold; color: #888; font-size: 0.7rem; display: block; }
.term-actions { padding: 15px 25px; border-top: 2px solid #ccc; background: #eee; display: flex; justify-content: flex-end; gap: 10px; }
.cyber-form { display: flex; flex-direction: column; gap: 15px; }
.form-row label { display: block; font-size: 0.75rem; font-weight: bold; margin-bottom: 5px; }
.form-row.split { display: flex; gap: 15px; } .form-row.split > * { flex: 1; }
.cyber-input { width: 100%; border: 2px solid #999; background: #fff; padding: 8px; font-family: var(--mono); font-size: 0.9rem; outline: none; }
.cyber-input:focus { border-color: var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.target-info { background: var(--black); color: var(--white); padding: 10px; margin-bottom: 20px; font-size: 0.9rem; }
.target-info .highlight { color: var(--red); font-weight: bold; }
@keyframes spin { to { transform: rotate(360deg); } }
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.95); }
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>