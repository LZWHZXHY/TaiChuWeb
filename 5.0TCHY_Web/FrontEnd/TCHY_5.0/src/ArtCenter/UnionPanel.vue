<template>
  <div class="joint-board-container">
    <header class="board-header">
      <div class="header-title">
        <span class="pulse-dot"></span>
        <h2>CHAI_JOINT_BOARD // 柴圈联合</h2>
      </div>
      
      <div class="header-tools">
        <div class="search-group">
          <i class="fas fa-search"></i>
          <input 
            v-model="q" 
            type="text" 
            placeholder="SEARCH_EVENT..." 
            @input="onSearchInput" 
          />
        </div>

        <div class="status-tabs">
          <button 
            :class="{ active: tab === 'ongoing' }" 
            @click="switchTab('ongoing')"
          >进行中</button>
          <button 
            :class="{ active: tab === 'finished' }" 
            @click="switchTab('finished')"
          >已归档</button>
        </div>

        <button class="create-btn" @click="openCreate">
          <i class="fas fa-plus"></i> 发起新联合
        </button>
      </div>

      <div class="filter-bar">
        <div class="filter-group">
          <label><i class="fas fa-filter"></i> 类型:</label>
          <select v-model="typeFilter" @change="onFilterChange">
            <option value="">ALL_TYPES</option>
            <option :value="1">联合 (Collab)</option>
            <option :value="2">接力 (Relay)</option>
            <option :value="3">比赛 (Match)</option>
          </select>
        </div>

        <div class="filter-group">
          <label><i class="fas fa-sort"></i> 排序:</label>
          <select v-model="sortBy" @change="onSortChange">
            <option value="">默认排序</option>
            <option value="startdate_desc">最新发起</option>
            <option value="startdate_asc">最早发起</option>
            <option value="enddate_asc">即将结束</option>
          </select>
        </div>
      </div>
    </header>

    <div class="board-content custom-scroll">
      
      <div v-if="loading" class="state-box">
        <i class="fas fa-circle-notch fa-spin"></i>
        <span>正在读取联合数据...</span>
      </div>

      <div v-else-if="!items.length" class="state-box">
        <i class="fas fa-folder-open"></i>
        <span>暂无相关活动数据</span>
      </div>

      <div v-else class="event-grid">
        <article 
          v-for="it in items" 
          :key="it.id" 
          class="event-card" 
          :class="`type-accent-${it.type}`"
        >
          <div class="card-status-strip"></div>
          
          <div class="card-main">
            <div class="card-header">
              <div class="header-row">
                <span class="type-badge">{{ typeLabel(it.type) }}</span>
                <span class="status-text" :class="`verify-${it.verify}`">{{ verifyLabel(it.verify) }}</span>
              </div>
              <h3 class="event-title" :title="it.name">{{ it.name }}</h3>
              <div class="author-info">
                <i class="fas fa-user-astronaut"></i> {{ it.host }}
              </div>
            </div>

            <div class="card-body">
              <p class="desc-text">{{ it.desc || '暂无简介...' }}</p>
              
              <div class="meta-grid">
                <div class="meta-item">
                  <span class="label">START</span>
                  <span class="val">{{ shortDate(it.startdate) }}</span>
                </div>
                <div class="meta-item">
                  <span class="label">END</span>
                  <span class="val">{{ it.enddate ? shortDate(it.enddate) : '无限期' }}</span>
                </div>
                <div class="meta-item full-width" v-if="it.QQgroup">
                  <span class="label">QQ GROUP</span>
                  <span class="val highlight">{{ it.QQgroup }}</span>
                </div>
              </div>
            </div>

            <div class="card-actions">
              <button class="action-btn outline" @click="viewDetail(it)">
                详情
              </button>
              
              <button 
                v-if="tab === 'ongoing' && it.QQgroup" 
                class="action-btn primary" 
                @click="applyJoin(it)"
              >
                加入群聊
              </button>
              
              <button 
                class="action-btn secondary" 
                @click="applyForEvent(it)"
              >
                申请联合
              </button>

              <div v-if="isMine(it) || isAdmin" class="admin-tools">
                <button v-if="isMine(it)" class="icon-btn" title="编辑" @click="editItem(it)"><i class="fas fa-pen"></i></button>
                <template v-if="isAdmin">
                  <button v-if="it.verify !== 1" class="icon-btn success" title="通过" @click="approveItem(it)"><i class="fas fa-check"></i></button>
                  <button v-if="it.verify !== 2" class="icon-btn danger" title="驳回" @click="rejectItem(it)"><i class="fas fa-times"></i></button>
                  <button class="icon-btn danger" title="删除" @click="deleteItem(it.id)"><i class="fas fa-trash"></i></button>
                </template>
              </div>
            </div>
          </div>
        </article>
      </div>

      <footer class="board-footer" v-if="items.length > 0">
        <button class="page-btn" :disabled="page <= 1" @click="prevPage"><i class="fas fa-chevron-left"></i></button>
        <span class="page-info">PAGE {{ page }} / {{ totalPages }}</span>
        <button class="page-btn" :disabled="page >= totalPages" @click="nextPage"><i class="fas fa-chevron-right"></i></button>
      </footer>
    </div>

    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="detail" class="tc-modal-overlay" @click.self="detail = null">
          <div class="tc-modal-card">
            <header class="modal-head">
              <h3>EVENT_DETAIL // 活动详情</h3>
              <button class="close-icon" @click="detail = null"><i class="fas fa-times"></i></button>
            </header>
            <div class="modal-content custom-scroll">
              <h2 class="detail-title">{{ detail.name }}</h2>
              <div class="detail-meta-bar">
                <span class="tag">{{ typeLabel(detail.type) }}</span>
                <span class="host">Host: {{ detail.host }}</span>
              </div>
              
              <div class="info-section">
                <label>DESCRIPTION</label>
                <p>{{ detail.desc }}</p>
              </div>

              <div class="info-section" v-if="detail.rules">
                <label>RULES & REQUIREMENTS</label>
                <div class="rules-box">{{ detail.rules }}</div>
              </div>

              <div class="info-grid">
                <div><label>Time</label> {{ formatDate(detail.startdate) }} - {{ detail.enddate ? formatDate(detail.enddate) : 'TBD' }}</div>
                <div><label>QQ Group</label> {{ detail.QQgroup || 'Not Set' }}</div>
              </div>
            </div>
            <footer class="modal-foot">
              <button class="tc-btn" @click="applyForEvent(detail)">填写申请表</button>
            </footer>
          </div>
        </div>
      </Transition>

      <Transition name="modal-fade">
        <div v-if="showCreate" class="tc-modal-overlay" @click.self="closeCreate">
          <div class="tc-modal-card form-mode">
            <header class="modal-head">
              <h3>{{ editMode ? 'EDIT_PROTOCOL' : 'INITIATE_PROTOCOL' }} // {{ editMode ? '编辑' : '发起' }}</h3>
              <button class="close-icon" @click="closeCreate"><i class="fas fa-times"></i></button>
            </header>
            <div class="modal-content custom-scroll">
              <form @submit.prevent="submitForm" id="createForm">
                <div class="form-group">
                  <label>Type / 类型</label>
                  <select v-model.number="form.type" class="tc-input">
                    <option :value="1">联合 (Collaboration)</option>
                    <option :value="2">接力 (Relay)</option>
                    <option :value="3">竞标赛 (Tournament)</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Title / 名称</label>
                  <input v-model="form.name" class="tc-input" required />
                </div>
                <div class="row-2">
                  <div class="form-group">
                    <label>Host / 发起人</label>
                    <input v-model="form.host" class="tc-input" required />
                  </div>
                  <div class="form-group">
                    <label>QQ Group / 群号</label>
                    <input v-model="form.QQgroup" class="tc-input" />
                  </div>
                </div>
                <div class="row-2">
                  <div class="form-group">
                    <label>Start / 开始时间</label>
                    <input v-model="form.startdate" type="datetime-local" class="tc-input" required />
                  </div>
                  <div class="form-group">
                    <label>End / 结束时间</label>
                    <input v-model="form.enddate" type="datetime-local" class="tc-input" />
                  </div>
                </div>
                <div class="form-group">
                  <label>Description / 简介</label>
                  <textarea v-model="form.desc" class="tc-input" rows="4"></textarea>
                </div>
                <div class="form-group">
                  <label>Rules / 规则</label>
                  <textarea v-model="form.rules" class="tc-input" rows="4"></textarea>
                </div>
              </form>
            </div>
            <footer class="modal-foot">
              <button class="tc-btn ghost" @click="closeCreate">CANCEL</button>
              <button class="tc-btn primary" type="submit" form="createForm" :disabled="submitting">
                {{ submitting ? 'PROCESSING...' : 'CONFIRM' }}
              </button>
            </footer>
          </div>
        </div>
      </Transition>

      <Transition name="modal-fade">
        <div v-if="showApplyModal" class="tc-modal-overlay" @click.self="closeApplyModal">
          <div class="tc-modal-card form-mode">
            <header class="modal-head">
              <h3>APPLY_LINK // 申请联合</h3>
              <button class="close-icon" @click="closeApplyModal"><i class="fas fa-times"></i></button>
            </header>
            <div class="modal-content custom-scroll">
              <div class="target-event-summary">
                Applying for: <strong>{{ selectedEvent?.name }}</strong>
              </div>
              <form @submit.prevent="submitApplication" id="applyForm">
                <div class="form-group">
                  <label>Applicant / 申请人ID</label>
                  <input v-model="applicationForm.applicantName" class="tc-input" required />
                </div>
                <div class="form-group">
                  <label>Contact / 联系方式 (QQ/Email)</label>
                  <input v-model="applicationForm.contact" class="tc-input" required />
                </div>
                <div class="form-group">
                  <label>Reason / 申请说明</label>
                  <textarea v-model="applicationForm.description" class="tc-input" rows="4" required></textarea>
                </div>
                <div class="form-group">
                  <label>Portfolio / 作品链接 (可选)</label>
                  <input v-model="applicationForm.portfolioUrl" class="tc-input" placeholder="https://..." />
                </div>
              </form>
            </div>
            <footer class="modal-foot">
              <button class="tc-btn ghost" @click="closeApplyModal">CANCEL</button>
              <button class="tc-btn primary" type="submit" form="applyForm" :disabled="submittingApplication">
                {{ submittingApplication ? 'SENDING...' : 'SUBMIT APPLICATION' }}
              </button>
            </footer>
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

// --- State ---
const tab = ref('ongoing')
const q = ref('')
const typeFilter = ref('')
const sortBy = ref('')
const page = ref(1)
const pageSize = ref(9) 
const loading = ref(false)
const items = ref([])
const total = ref(0)

// --- Modal States ---
const detail = ref(null)
const showCreate = ref(false)
const editMode = ref(false)
const submitting = ref(false)
const showApplyModal = ref(false)
const selectedEvent = ref(null)
const submittingApplication = ref(false)

// --- Forms ---
// 修正6: form 初始化使用 QQgroup
const form = reactive({
  id: null, name: '', host: '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0, creatorId: null
})
const applicationForm = reactive({
  applicantName: '', contact: '', description: '', portfolioUrl: ''
})

// --- Helpers ---
const TYPE_MAP = { 1: '联合', 2: '接力', 3: '比赛' }
const typeLabel = (t) => TYPE_MAP[t] || '其他'
const verifyLabel = (v) => v === 0 ? '审核中' : v === 1 ? '进行中' : '已驳回'
const isMine = (it) => auth.user?.id && it.creatorId === auth.user.id
const isAdmin = computed(() => auth.user?.roles?.includes?.('Admin'))
const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))
const parseSortBy = (val) => {
  if (!val) return { field: '', order: '' }
  const [field, order] = val.split('_')
  return { field, order }
}

const formatDate = (d) => d ? new Date(d).toLocaleString() : '-'
const shortDate = (d) => d ? new Date(d).toLocaleDateString() : '-'

// --- API Actions ---
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
    // ----------------------------------------------------
    // MOCK DATA START (你的真实数据)
    // ----------------------------------------------------
    await new Promise(r => setTimeout(r, 400)); // 模拟一点点延迟，更有实感
    const mockData = {
        "data": [
            {
                "id": 19,
                "name": "五天联合",
                "host": "zetak",
                "type": 1,
                "startdate": "2026-01-10T00:00:00",
                "enddate": "2026-02-01T00:00:00",
                "desc": "计时五天，自律绘画，场景左边出开始，进右边结束。表现形式不限，屠杀室，11或者22都行。五天计时到请自觉结束，之后导出视频或者源文件@zetak即可。oc任意，发挥想象！没有限制水平",
                "QQgroup": "",
                "rules": "",
                "verify": 1,
                "report": 0
            },
            {
                "id": 17,
                "name": "不咕联合",
                "host": "Applecat",
                "type": 1,
                "startdate": "2020-07-02T00:00:00",
                "enddate": "2028-10-24T00:00:00",
                "desc": "分为平面屠杀、平面对打、分镜对打三个部分；\n平面屠杀可以接力，也可以自己制作单独的片段，可以使用flash或fc软件制作\n另外两个部分仅限接力的方式，使用任意版本的flash(包括Adobe Animate)制作",
                "QQgroup": "1108112956",
                "rules": "进群，选择你想画的部分，让群主把文件发给你\n画布尺寸1920x1080像素，帧率24fps\n画完后提交fla文件；fc软件用户可以提交fc文件，或提交背景透明的序列帧文件",
                "verify": 1,
                "report": 0
            },
            {
                "id": 15,
                "name": "鹅鸭杀联合",
                "host": "太初寰宇-太虚绘院",
                "type": 1,
                "startdate": "2025-11-05T00:00:00",
                "enddate": "2025-11-28T00:00:00",
                "desc": "只要内容有鹅鸭杀就行，要求不限制，很自由的一个联合。",
                "QQgroup": "973380634",
                "rules": "24fps，软件绘画，1080p，自行配音！\n要求上传到太虚绘院的五联群文件中！\n另外要求改名，将视频改为 用户圈名+B站UID！\n\n",
                "verify": 1,
                "report": 0
            },
            {
                "id": 14,
                "name": "火柴人文字联合2水水水",
                "host": "紫刃-游风",
                "type": 1,
                "startdate": "2025-07-19T00:00:00",
                "enddate": "2025-10-26T00:00:00",
                "desc": "本次联合主题为：联合主角是一位脸上带有三点水类汉字的黑色火柴人...",
                "QQgroup": "722466754",
                "rules": "软件和技术没有限制，分辨率统一比例为:高清1920x1080或1280x720",
                "verify": 1,
                "report": 0
            },
            {
                "id": 11,
                "name": "自由接力练习",
                "host": "援受",
                "type": 2,
                "startdate": "2025-07-01T00:00:00",
                "desc": "画！",
                "QQgroup": "973380634",
                "rules": "",
                "verify": 1,
                "report": 0
            },
            {
                "id": 6,
                "name": "【永无止境的火柴人电影战斗】",
                "host": "太初寰宇 - 太虚绘院",
                "type": 3,
                "startdate": "2025-07-01T00:00:00",
                "enddate": "2025-10-01T00:00:00",
                "desc": "选择一个带有动作的电影，比如说枪战，或者打戏，超能力都行...",
                "QQgroup": "973380634",
                "rules": "24fps,软件，1080p，至少5s以上的内容。",
                "verify": 1,
                "report": 0
            },
            {
                "id": 1,
                "name": "踢毽子联合",
                "host": "太初寰宇-太虚绘院",
                "type": 1,
                "startdate": "2025-10-12T00:00:00",
                "enddate": "2025-10-28T00:00:00",
                "desc": "在画面中踢毽子，毽子要求从画面外飞进画面内开始...",
                "QQgroup": "973380634",
                "rules": "24fps。\n软件作画！\n提交视频！",
                "verify": 1,
                "report": 0
            }
        ],
        "total": 7
    };
    items.value = mockData.data;
    total.value = mockData.total;
    // ----------------------------------------------------
    // MOCK DATA END
    // ----------------------------------------------------

    /* // 原有的 API 逻辑（当你后端准备好后取消注释即可）
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
    if (resp.data?.success || Array.isArray(resp.data)) {
        const data = resp.data.data || resp.data
        items.value = data
        total.value = resp.data.total || data.length
    }
    */
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

// --- Interaction Handlers ---
const onSearchInput = () => { page.value = 1; fetchList() }
const onFilterChange = () => { page.value = 1; fetchList() }
const onSortChange = () => { page.value = 1; fetchList() }
const switchTab = (t) => { tab.value = t; page.value = 1; fetchList() } 
const prevPage = () => { if (page.value > 1) { page.value--; fetchList() } }
const nextPage = () => { if (page.value < totalPages.value) { page.value++; fetchList() } }

const viewDetail = (it) => { detail.value = it }

const applyJoin = (it) => {
  // 修正7: 跳转链接使用 QQgroup
  if (!it.QQgroup) return alert('未设置群号')
  window.open(`https://jq.qq.com/?_=${encodeURIComponent(it.QQgroup)}`, '_blank')
}

// --- Create / Edit Logic ---
const openCreate = async () => {
  if(!(await ensureAuth())) return
  editMode.value = false
  // 修正8: 初始化字段 QQgroup
  Object.assign(form, { 
    id: null, name: '', host: auth.user?.username || '', type: 1, startdate: '', enddate: '', desc: '', rules: '', QQgroup: '', verify: 0 
  })
  showCreate.value = true
}

const editItem = async (it) => {
    if(!(await ensureAuth())) return
    try {
        // 如果后端返回的也是 QQgroup 大写，这里 form 赋值就没问题
        // 这里如果是 Mock 模式，我们可以直接用 it 填充
        Object.assign(form, it)
        
        // Fix date format
        if(form.startdate) form.startdate = form.startdate.substring(0, 16)
        if(form.enddate) form.enddate = form.enddate.substring(0, 16)
        
        editMode.value = true
        showCreate.value = true
    } catch(e) { alert('加载详情失败') }
}

const submitForm = async () => {
  submitting.value = true
  try {
    const payload = { ...form }
    payload.startdate = new Date(form.startdate).toISOString()
    if(form.enddate) payload.enddate = new Date(form.enddate).toISOString()
    
    // 注意：这里发送给后端时，确保后端接收的是 QQgroup 还是 qQgroup
    // 如果后端是用 C# 且属性名是 QQgroup，那么 JSON 序列化通常也是 QQgroup
    if (editMode.value) await apiClient.put(`${basePath}/${form.id}`, payload)
    else await apiClient.post(`${basePath}/create`, payload)
    
    alert(editMode.value ? '已更新' : '已提交申请')
    showCreate.value = false
    fetchList()
  } catch (e) { alert('提交失败') } 
  finally { submitting.value = false }
}

// --- Application Logic ---
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
    alert('申请已发送')
    closeApplyModal()
  } catch(e) { alert('发送失败') }
  finally { submittingApplication.value = false }
}

const closeCreate = () => showCreate.value = false
const closeApplyModal = () => showApplyModal.value = false

// --- Admin Logic ---
const deleteItem = async (id) => { if(confirm('确认删除?')) { await apiClient.delete(`${basePath}/${id}`); fetchList() } }
const approveItem = async (it) => { if(confirm('确认通过?')) { await apiClient.post(`${basePath}/moderation/approve`, { Id: it.id }); fetchList() } }
const rejectItem = async (it) => { 
    const reason = prompt('驳回理由:')
    if(reason) { await apiClient.post(`${basePath}/moderation/reject`, { Id: it.id, Note: reason }); fetchList() } 
}

onMounted(() => {
  auth.checkAuth()
  fetchList()
})
</script>

<style scoped>
/* ArtCenter Design System Variables */
.joint-board-container {
  --accent-purple: #8b5cf6;
  --accent-light: #ddd6fe;
  --text-main: #1e293b;
  --text-sub: #64748b;
  --bg-card: #ffffff;
  --border: #e2e8f0;
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-hover: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #f8fafc;
  padding: 20px;
  box-sizing: border-box;
  font-family: 'Segoe UI', sans-serif;
}

/* --- Header --- */
.board-header {
  margin-bottom: 25px;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title h2 {
  font-size: 20px;
  font-weight: 800;
  color: var(--text-main);
  margin: 0;
  letter-spacing: 1px;
}

.pulse-dot {
  width: 8px; height: 8px;
  background: var(--accent-purple);
  border-radius: 50%;
  box-shadow: 0 0 0 4px rgba(139, 92, 246, 0.2);
}

.header-tools {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 15px;
  flex-wrap: wrap;
}

/* Search */
.search-group {
  position: relative;
  flex: 1;
  max-width: 300px;
}
.search-group i { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: var(--text-sub); }
.search-group input {
  width: 100%;
  padding: 10px 10px 10px 35px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  font-size: 13px;
  transition: 0.2s;
}
.search-group input:focus { border-color: var(--accent-purple); box-shadow: 0 0 0 3px rgba(139,92,246,0.1); }

/* Tabs */
.status-tabs {
  background: #fff;
  padding: 4px;
  border-radius: 8px;
  border: 1px solid var(--border);
  display: flex;
}
.status-tabs button {
  border: none;
  background: transparent;
  padding: 6px 15px;
  font-size: 12px;
  font-weight: bold;
  color: var(--text-sub);
  cursor: pointer;
  border-radius: 6px;
  transition: 0.2s;
}
.status-tabs button.active {
  background: var(--accent-purple);
  color: #fff;
}

/* Create Btn */
.create-btn {
  background: var(--text-main);
  color: #fff;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: bold;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: 0.2s;
  box-shadow: var(--shadow-md);
}
.create-btn:hover { background: #000; transform: translateY(-2px); }

/* Filters */
.filter-bar {
  display: flex;
  gap: 20px;
  padding: 10px 15px;
  background: #fff;
  border-radius: 8px;
  border: 1px solid var(--border);
  font-size: 12px;
}
.filter-group { display: flex; align-items: center; gap: 8px; color: var(--text-sub); }
.filter-group select {
  border: none;
  background: transparent;
  font-weight: bold;
  color: var(--text-main);
  cursor: pointer;
  outline: none;
}

/* --- Content & Grid --- */
.board-content {
  flex: 1;
  overflow-y: auto;
  padding-right: 5px;
}
.event-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  padding-bottom: 20px;
}

/* --- Card Design (MD + Tech) --- */
.event-card {
  background: #fff;
  border-radius: 12px;
  border: 1px solid var(--border);
  overflow: hidden;
  position: relative;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  display: flex;
  flex-direction: column;
}
.event-card:hover {
  transform: translateY(-5px);
  box-shadow: var(--shadow-hover);
  border-color: var(--accent-light);
}

/* Type Accents */
.type-accent-1 .card-status-strip { background: #8b5cf6; } /* Joint */
.type-accent-1 .type-badge { background: #f3e8ff; color: #7e22ce; }

.type-accent-2 .card-status-strip { background: #10b981; } /* Relay */
.type-accent-2 .type-badge { background: #d1fae5; color: #047857; }

.type-accent-3 .card-status-strip { background: #f59e0b; } /* Match */
.type-accent-3 .type-badge { background: #fef3c7; color: #b45309; }

.card-status-strip { height: 4px; width: 100%; }

.card-main { padding: 20px; display: flex; flex-direction: column; flex: 1; }

.card-header { margin-bottom: 15px; }
.header-row { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 10px; font-weight: bold; }
.status-text.verify-0 { color: #f59e0b; }
.status-text.verify-1 { color: #10b981; }

.event-title { margin: 0 0 5px 0; font-size: 16px; color: var(--text-main); line-height: 1.4; }
.author-info { font-size: 11px; color: var(--text-sub); display: flex; align-items: center; gap: 5px; }

.card-body { flex: 1; margin-bottom: 15px; }
.desc-text { font-size: 13px; color: #475569; line-height: 1.6; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; margin-bottom: 15px; }

.meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; background: #f8fafc; padding: 10px; border-radius: 8px; }
.meta-item { display: flex; flex-direction: column; }
.meta-item.full-width { grid-column: span 2; border-top: 1px dashed #cbd5e1; padding-top: 5px; margin-top: 5px; }
.meta-item .label { font-size: 9px; color: #94a3b8; font-weight: bold; letter-spacing: 0.5px; }
.meta-item .val { font-size: 12px; font-weight: 600; color: var(--text-main); }
.meta-item .val.highlight { color: var(--accent-purple); font-family: monospace; }

.card-actions { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }
.action-btn { flex: 1; border: none; padding: 8px 0; border-radius: 6px; font-size: 12px; font-weight: bold; cursor: pointer; transition: 0.2s; }
.action-btn.outline { background: transparent; border: 1px solid var(--border); color: var(--text-sub); }
.action-btn.outline:hover { border-color: var(--text-main); color: var(--text-main); }
.action-btn.primary { background: var(--accent-purple); color: #fff; }
.action-btn.primary:hover { background: #7c3aed; }
.action-btn.secondary { background: #f1f5f9; color: var(--text-main); }
.action-btn.secondary:hover { background: #e2e8f0; }

.admin-tools { display: flex; gap: 5px; margin-left: auto; }
.icon-btn { border: none; background: transparent; color: var(--text-sub); cursor: pointer; padding: 5px; transition: 0.2s; }
.icon-btn:hover { color: var(--accent-purple); transform: scale(1.1); }
.icon-btn.danger:hover { color: #ef4444; }

/* --- Footer --- */
.board-footer { display: flex; justify-content: center; align-items: center; gap: 15px; margin-top: 20px; }
.page-btn { background: #fff; border: 1px solid var(--border); width: 32px; height: 32px; border-radius: 50%; cursor: pointer; color: var(--text-sub); transition: 0.2s; }
.page-btn:hover:not(:disabled) { border-color: var(--accent-purple); color: var(--accent-purple); }
.page-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.page-info { font-size: 11px; font-weight: bold; color: var(--text-sub); letter-spacing: 1px; }

/* --- States --- */
.state-box { text-align: center; padding: 50px; color: var(--text-sub); font-size: 14px; display: flex; flex-direction: column; align-items: center; gap: 10px; }
.state-box i { font-size: 24px; color: #cbd5e1; }

/* --- Modal Styles (Global Reuse Potential) --- */
.tc-modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.6); backdrop-filter: blur(4px); z-index: 2000;
  display: flex; justify-content: center; align-items: center;
}
.tc-modal-card {
  background: #fff; width: 500px; max-width: 90vw; max-height: 85vh; border-radius: 12px;
  display: flex; flex-direction: column; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  animation: modalUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.tc-modal-card.form-mode { width: 600px; }

.modal-head {
  padding: 15px 20px; border-bottom: 1px solid var(--border); display: flex; justify-content: space-between; align-items: center;
  background: #f8fafc; border-radius: 12px 12px 0 0;
}
.modal-head h3 { margin: 0; font-size: 13px; font-weight: 800; color: #64748b; letter-spacing: 1px; }
.close-icon { border: none; background: none; font-size: 16px; color: #94a3b8; cursor: pointer; }

.modal-content { padding: 25px; overflow-y: auto; }
.detail-title { margin: 0 0 10px 0; font-size: 20px; color: var(--text-main); }
.detail-meta-bar { display: flex; gap: 10px; font-size: 12px; color: var(--text-sub); margin-bottom: 20px; padding-bottom: 15px; border-bottom: 1px solid var(--border); }
.tag { background: #f1f5f9; padding: 2px 8px; border-radius: 4px; font-weight: bold; }

.info-section { margin-bottom: 20px; }
.info-section label { display: block; font-size: 10px; font-weight: bold; color: #94a3b8; margin-bottom: 5px; letter-spacing: 0.5px; }
.info-section p { font-size: 14px; line-height: 1.6; color: var(--text-main); margin: 0; }
.rules-box { background: #fffbeb; border: 1px solid #fcd34d; padding: 15px; border-radius: 6px; font-size: 13px; color: #92400e; white-space: pre-wrap; }

.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; background: #f8fafc; padding: 15px; border-radius: 8px; font-size: 13px; }
.info-grid label { display: block; font-size: 10px; color: #94a3b8; font-weight: bold; }

.modal-foot { padding: 15px 20px; border-top: 1px solid var(--border); display: flex; justify-content: flex-end; gap: 10px; background: #f8fafc; border-radius: 0 0 12px 12px; }

/* Form Elements */
.tc-input { width: 100%; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; outline: none; font-size: 14px; transition: 0.2s; background: #fff; color: var(--text-main); }
.tc-input:focus { border-color: var(--accent-purple); box-shadow: 0 0 0 3px rgba(139,92,246,0.1); }
.form-group { margin-bottom: 15px; }
.form-group label { display: block; font-size: 12px; font-weight: bold; margin-bottom: 6px; color: var(--text-sub); }
.row-2 { display: flex; gap: 15px; } .row-2 > * { flex: 1; }

.tc-btn { padding: 10px 20px; border-radius: 6px; border: none; font-weight: bold; cursor: pointer; font-size: 13px; }
.tc-btn.primary { background: var(--accent-purple); color: #fff; }
.tc-btn.ghost { background: transparent; color: var(--text-sub); }
.tc-btn:disabled { opacity: 0.6; cursor: wait; }

@keyframes modalUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
</style>