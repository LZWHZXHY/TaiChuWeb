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
          </div>
        </div>
      </header>

      <div 
        class="mission-grid-wrapper custom-scroll" 
        ref="scrollContainer"
        @scroll="handleScroll"
      >
        <div v-if="loading && items.length === 0" class="system-msg">
          <div class="loader-spinner"></div>
          <span>INITIALIZING_TACTICAL_DATA_STREAM...</span>
        </div>

        <div v-else-if="!items.length && !loading" class="system-msg error">
          [ NO_OPERATIONS_FOUND_IN_SECTOR ]
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
              {{ it.desc || 'NO_INTEL_AVAILABLE_FOR_THIS_UNIT...' }}
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
                <button class="icon-action" title="Detail" @click="viewDetail(it)">👁</button>
                <button v-if="it.QQgroup" class="icon-action primary" @click="applyJoin(it)">QQ</button>
                <button class="icon-action primary" @click="applyForEvent(it)">APPLY</button>
                
                <div v-if="isMine(it) || isAdmin" class="more-actions">
                  <button v-if="isMine(it)" class="icon-mini" @click="editItem(it)">✎</button>
                  <template v-if="isAdmin">
                    <button class="icon-mini del" @click="deleteItem(it.id)">🗑</button>
                  </template>
                </div>
              </div>
            </div>
            <div class="side-strip"></div>
          </article>
        </div>

        <footer class="stream-footer">
          <div v-if="loading && items.length > 0" class="loading-more">
            <span class="blink">>> SYNCING_ADDITIONAL_DATA...</span>
          </div>
          <div v-if="!hasMore && items.length > 0" class="end-marker">
            <div class="line"></div>
            <span class="text">END_OF_ARCHIVE // 数据同步完成</span>
            <div class="line"></div>
          </div>
        </footer>
      </div>

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
            </div>
            <div class="term-actions">
              <button class="cyber-btn primary full" @click="applyForEvent(detail)">>>> SIGN_UP_FOR_MISSION</button>
            </div>
          </div>
        </div>
      </Transition>

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
                  <select v-model.number="form.type" class="cyber-input" required>
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

      <Transition name="glitch-fade">
        <div v-if="showApplyModal" class="cyber-modal-overlay" @click.self="closeApplyModal">
          <div class="cyber-terminal form-mode">
            <div class="term-header">
              <span class="term-title">>> APPLICATION_FORM</span>
              <button class="term-close" @click="closeApplyModal">[ ABORT ]</button>
            </div>
            <div class="term-content custom-scroll">
              <div class="target-info">APPLYING_TO: <span class="highlight">{{ selectedEvent?.name }}</span></div>
              <form @submit.prevent="submitApplication" id="applyForm" class="cyber-form">
                <div class="form-row"><label>APPLICANT_ID:</label><input v-model="applicationForm.applicantName" class="cyber-input" required /></div>
                <div class="form-row"><label>CONTACT (QQ/Email):</label><input v-model="applicationForm.contact" class="cyber-input" required /></div>
                <div class="form-row"><label>STATEMENT:</label><textarea v-model="applicationForm.description" class="cyber-input" rows="4" required></textarea></div>
              </form>
            </div>
            <div class="term-actions">
              <button class="cyber-btn ghost" @click="closeApplyModal">CANCEL</button>
              <button class="cyber-btn primary" type="submit" form="applyForm" :disabled="submittingApplication">SEND</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const router = useRouter()
const auth = useAuthStore()

// --- 1. 状态定义 ---
const page = ref(1)
const pageSize = ref(12)
const items = ref([])
const total = ref(0)
const loading = ref(false)
const hasMore = ref(true)
const scrollContainer = ref(null)

const tab = ref('ongoing')
const q = ref('')
const typeFilter = ref('')

const detail = ref(null)
const showCreate = ref(false)
const editMode = ref(false)
const submitting = ref(false)
const showApplyModal = ref(false)
const selectedEvent = ref(null)
const submittingApplication = ref(false)

const form = reactive({ id: null, name: '', host: '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0, creatorId: null })
const applicationForm = reactive({ applicantName: '', contact: '', description: '', portfolioUrl: '' })

// --- 2. 核心数据请求 ---
const fetchList = async (isRefresh = true) => {
  if (loading.value) return
  if (!isRefresh && !hasMore.value) return

  loading.value = true
  if (isRefresh) { page.value = 1; items.value = []; hasMore.value = true }

  try {
    const params = {
      q: q.value || undefined,
      type: typeFilter.value !== '' ? typeFilter.value : undefined,
      verify: 1,
      page: page.value,
      pageSize: pageSize.value
    }
    const resp = await apiClient.get('ChaiLianHe/list', { params })
    if (resp.data?.data) {
      const newItems = resp.data.data
      items.value = isRefresh ? newItems : [...items.value, ...newItems]
      total.value = resp.data.total
      hasMore.value = items.value.length < total.value
    }
  } catch (err) { console.error(err) } finally { loading.value = false }
}

// --- 3. 交互逻辑 ---
const handleScroll = (e) => {
  if (loading.value || !hasMore.value) return
  const { scrollTop, scrollHeight, clientHeight } = e.target
  if (scrollTop + clientHeight >= scrollHeight - 120) {
    page.value++
    fetchList(false)
  }
}

const openCreate = async () => {
  if (!auth.checkAuth()) { router.push({ name: 'Login' }); return }
  editMode.value = false
  Object.assign(form, { id: null, name: '', host: auth.user?.username || '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0 })
  showCreate.value = true
}

const closeCreate = () => { showCreate.value = false }

const submitForm = async () => {
  submitting.value = true
  try {
    const payload = { ...form }
    payload.startdate = new Date(form.startdate).toISOString()
    if(form.enddate) payload.enddate = new Date(form.enddate).toISOString()
    if (editMode.value) await apiClient.put(`ChaiLianHe/${form.id}`, payload)
    else await apiClient.post(`ChaiLianHe/create`, payload)
    showCreate.value = false; fetchList(true)
  } catch (e) { alert('FAILED') } finally { submitting.value = false }
}

const applyForEvent = (it) => {
  selectedEvent.value = it
  Object.assign(applicationForm, { applicantName: auth.user?.username || '', contact: '', description: '' })
  showApplyModal.value = true
}

const submitApplication = async () => {
  submittingApplication.value = true
  try {
    await apiClient.post(`ChaiLianHe/apply`, { eventId: selectedEvent.value.id, ...applicationForm })
    alert('SUCCESS'); showApplyModal.value = false
  } catch(e) { alert('FAILED') } finally { submittingApplication.value = false }
}

const closeApplyModal = () => { showApplyModal.value = false }
const viewDetail = (it) => { detail.value = it }
const applyJoin = (it) => { window.open(`https://jq.qq.com/?_=${it.QQgroup}`, '_blank') }
const onSearchInput = () => fetchList(true)
const onFilterChange = () => fetchList(true)
const switchTab = (t) => { tab.value = t; fetchList(true) }

const editItem = (it) => { Object.assign(form, it); editMode.value = true; showCreate.value = true }
const deleteItem = async (id) => { if(confirm('DELETE?')) { await apiClient.delete(`ChaiLianHe/${id}`); fetchList(true) } }

const padZero = (n) => n < 10 ? `0${n}` : n
const typeLabelEn = (t) => ({ 1: 'COLLAB', 2: 'RELAY', 3: 'MATCH' }[t] || 'UNKNOWN')
const verifyLabelEn = (v) => v === 0 ? 'PENDING' : v === 1 ? 'ACTIVE' : 'REJECTED'
const shortDate = (d) => d ? d.substring(0, 10).replace(/-/g, '.') : 'N/A'
const calculateProgress = (it) => {
  const start = new Date(it.startdate).getTime(); if (!it.enddate) return 10
  const end = new Date(it.enddate).getTime(); const now = Date.now()
  if (now < start) return 0; const p = Math.floor(((now - start) / (end - start)) * 100)
  return Math.min(100, Math.max(0, p))
}
const isMine = (it) => auth.user?.id && it.creatorId === auth.user.id
const isAdmin = computed(() => auth.user?.roles?.includes?.('Admin'))

onMounted(() => fetchList(true))
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.joint-industrial {
  --red: #D92323; --black: #111111; --white: #F4F1EA;
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif;
  width: 100%; height: 100%; font-family: var(--mono); color: var(--black);
  position: relative; overflow: hidden; display: flex; flex-direction: column;
}

.joint-container {
  flex: 1; display: flex; flex-direction: column;
  padding: 20px; gap: 20px; min-height: 0; overflow: hidden; position: relative; z-index: 1;
}

/* --- 控制台 --- */
.command-console {
  flex-shrink: 0; background: var(--white); border: 4px solid var(--black);
  padding: 15px 20px; display: flex; flex-direction: column; gap: 15px;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}
.console-row { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 15px; }
.title-block { display: flex; align-items: center; gap: 15px; }
.icon-box { background: var(--black); color: var(--white); font-family: var(--heading); font-size: 1.5rem; padding: 5px 10px; transform: skew(-10deg); }
.text-group h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-group .sub { font-size: 0.7rem; font-weight: bold; color: #666; }

.search-terminal { flex: 1; max-width: 400px; display: flex; align-items: center; border-bottom: 2px solid var(--black); position: relative; }
.prompt { font-weight: bold; margin-right: 10px; color: var(--red); }
.terminal-input { border: none; background: transparent; outline: none; width: 100%; font-family: var(--mono); font-size: 1rem; padding: 5px; }
.scan-line { position: absolute; bottom: -2px; left: 0; height: 2px; width: 0; background: var(--red); transition: width 0.3s; }
.terminal-input:focus + .scan-line { width: 100%; }

.filter-deck { display: flex; gap: 20px; align-items: center; }
.toggle-switch { display: flex; border: 2px solid var(--black); background: #ccc; }
.switch-opt { padding: 5px 15px; font-weight: bold; font-size: 0.8rem; cursor: pointer; transition: 0.2s; }
.switch-opt.active { background: var(--black); color: var(--white); }

.cyber-btn { border: none; padding: 10px 20px; font-family: var(--heading); font-size: 1.1rem; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.2s; box-shadow: 4px 4px 0 #999; }
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover { background: var(--red); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.cyber-btn.ghost { background: transparent; border: 2px solid var(--black); color: var(--black); }

/* --- 列表区域 --- */
.mission-grid-wrapper { flex: 1; overflow-y: auto; min-height: 0; padding-right: 5px; }
.plate-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 25px; }

.data-plate { background: #fff; border: 2px solid var(--black); display: flex; flex-direction: column; box-shadow: 6px 6px 0 rgba(0,0,0,0.1); transition: 0.25s; position: relative; overflow: hidden; }
.data-plate:hover { transform: translateY(-5px); box-shadow: 10px 10px 0 var(--red); }
.side-strip { position: absolute; left: 0; top: 0; bottom: 0; width: 6px; background: var(--black); }
.status-1 .side-strip { background: #27ae60; }
.status-2 .side-strip { background: var(--red); }

.plate-header { padding: 15px 15px 10px 25px; border-bottom: 1px dashed #ccc; }
.plate-title { font-family: var(--heading); font-size: 1.5rem; margin: 0; text-transform: uppercase; }

.data-cells { display: flex; gap: 15px; padding: 10px 15px 10px 25px; background: #f9f9f9; border-bottom: 1px solid var(--black); }
.cell { display: flex; flex-direction: column; flex: 1; }
.c-label { font-size: 0.6rem; font-weight: bold; color: #888; }
.c-val { font-size: 0.85rem; font-weight: bold; }

.plate-desc { padding: 15px 15px 15px 25px; font-size: 0.85rem; line-height: 1.5; color: #555; flex: 1; }

.progress-section { padding: 0 15px 15px 25px; }
.prog-track { height: 8px; background: #eee; border: 1px solid var(--black); position: relative; overflow: hidden; }
.prog-fill { height: 100%; background: var(--black); transition: width 0.6s ease; }
.prog-grid-mask { position: absolute; inset: 0; background-image: repeating-linear-gradient(90deg, transparent, transparent 19px, rgba(255,255,255,0.5) 19px, rgba(255,255,255,0.5) 20px); }

.plate-footer { margin-top: auto; background: var(--black); color: var(--white); padding: 10px 15px 10px 25px; display: flex; justify-content: space-between; align-items: center; }
.footer-actions { display: flex; gap: 8px; align-items: center; }
.icon-action { background: #333; border: 1px solid #555; color: #fff; padding: 5px 10px; cursor: pointer; font-size: 0.8rem; }
.icon-action.primary { background: var(--red); border: none; font-weight: bold; }

/* --- 瀑布流反馈 --- */
.stream-footer { padding: 40px 0 80px; text-align: center; }
.loading-more { color: var(--red); font-weight: bold; }
.end-marker { display: flex; align-items: center; justify-content: center; gap: 15px; opacity: 0.3; }
.end-marker .line { height: 1px; flex: 0 1 100px; background: var(--black); }
.end-marker .text { font-size: 0.7rem; font-weight: bold; }

/* --- 弹窗样式 --- */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
.cyber-terminal { width: 500px; max-width: 95vw; background: #f4f4f4; border: 4px solid var(--black); box-shadow: 15px 15px 0 rgba(0,0,0,0.5); display: flex; flex-direction: column; max-height: 85vh; }
.cyber-terminal.form-mode { width: 600px; }
.term-header { background: var(--black); color: var(--white); padding: 10px 15px; display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--red); }
.term-title { font-family: var(--heading); letter-spacing: 1px; }
.term-close { background: none; border: none; color: #aaa; cursor: pointer; font-family: var(--mono); }
.term-content { padding: 25px; overflow-y: auto; flex: 1; }
.detail-h1 { font-family: var(--heading); font-size: 2rem; margin: 0 0 10px 0; }
.term-section { margin-bottom: 20px; }
.sec-title { font-weight: bold; margin-bottom: 5px; border-left: 3px solid var(--black); padding-left: 8px; }
.code-block { background: #e0e0e0; padding: 10px; font-size: 0.85rem; border: 1px solid #ccc; color: #8b0000; }
.term-actions { padding: 15px 25px; border-top: 2px solid #ccc; background: #eee; display: flex; justify-content: flex-end; gap: 10px; }
.cyber-form { display: flex; flex-direction: column; gap: 15px; }
.form-row label { display: block; font-size: 0.75rem; font-weight: bold; margin-bottom: 5px; }
.form-row.split { display: flex; gap: 15px; }
.cyber-input { width: 100%; border: 2px solid #999; background: #fff; padding: 8px; font-family: var(--mono); outline: none; }

/* 背景与动画 */
.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(0,0,0,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(0,0,0,0.05) 1px, transparent 1px); background-size: 40px 40px; }
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { from { transform: translateY(0); } to { transform: translateY(-40px); } }
.loader-spinner { width: 30px; height: 30px; border: 3px solid #ddd; border-top-color: var(--red); border-radius: 50%; animation: spin 0.8s linear infinite; margin: 0 auto 10px; }
@keyframes spin { to { transform: rotate(360deg); } }
.blink { animation: blink 1s infinite; }
@keyframes blink { 50% { opacity: 0.3; } }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }

/* Glitch Transition */
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.95); }
</style>