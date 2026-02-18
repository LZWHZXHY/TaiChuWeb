<template>
  <div class="archive-terminal">
    <div class="paper-texture"></div>
    <div class="drafting-grid"></div>

    <div class="archive-container">
      <header class="archive-header">
        <div class="header-main">
          <div class="brand-box">
            <span class="dept">BUREAU_ARCHIVE // 档案局</span>
            <h1 class="main-title">柴圈联合</h1>
            <div class="sub-info">
              <span class="status-indicator"><i class="dot"></i> 系统同步中</span>
              <span class="divider">|</span>
              <span class="ver">V 2.2.0</span>
            </div>
          </div>
          
          <div class="header-actions">
            <button class="md-btn primary-ink" @click="showCreateModal = true">
              <span class="icon">＋</span> 发起新提案 // NEW_PROPOSAL
            </button>
          </div>
        </div>

        <div class="header-controls">
          <div class="search-field">
            <input v-model="q" type="text" class="ink-search" placeholder="检索案卷名、指挥官或编号..." @input="onSearchInput" />
            <div class="focus-line"></div>
          </div>
          
          <div class="tab-selector">
            <button :class="{ active: tab === 'all' }" @click="switchTab('all')">ALL_全部</button>
            <button :class="{ active: tab === 'ongoing' }" @click="switchTab('ongoing')">ACTIVE_进行中</button>
            <button :class="{ active: tab === 'finished' }" @click="switchTab('finished')">ARCHIVED_已结束</button>
          </div>
        </div>
      </header>

      <main class="archive-scroll custom-scroll" ref="scrollContainer" @scroll="handleScroll">
        <div v-if="loading && items.length === 0" class="loading-state">
          <div class="spinner"></div> 正在解密历史档案...
        </div>

        <div v-else class="archive-grid">
          <article 
            v-for="it in items" 
            :key="it.id" 
            class="archive-card" 
            :class="{ 'is-expired': checkExpired(it.enddate) }"
            @click="viewDetail(it)"
          >
            <div class="card-sidebar">
              <span class="index-no">{{ padZero(it.id) }}</span>
              <div class="type-tag">{{ typeLabelEn(it.type) }}</div>
            </div>

            <div class="card-content">
              <div v-if="checkExpired(it.enddate)" class="closed-stamp">ARCHIVED_结案</div>

              <header class="card-head">
                <h3 class="card-title">{{ it.name }}</h3>
                <div class="stamp-wrapper">
                   <div class="stamp-ink" :class="it.verify === 1 ? 'approved' : 'pending'">
                     {{ it.verify === 1 ? 'FILE_OK' : 'UNDER_REVIEW' }}
                   </div>
                </div>
              </header>

              <div class="card-meta">
                <div class="m-item"><span class="k">HOST:</span><span class="v">{{ it.host }}</span></div>
                <div class="m-item"><span class="k">LIMIT:</span><span class="v">{{ shortDate(it.enddate) || '永久' }}</span></div>
              </div>

              <p class="card-desc">{{ it.desc }}</p>

              <footer class="card-footer">
                <span class="access-btn">{{ checkExpired(it.enddate) ? '查阅存档内容' : 'ACCESS_ARCHIVE' }} →</span>
                <span class="file-ref">REF.THCY_{{ it.id }}</span>
              </footer>
            </div>
          </article>
        </div>

        <div v-if="!hasMore && items.length > 0" class="end-seal">
          <span class="line"></span>
          <span class="text">数据同步完毕 // EOF</span>
          <span class="line"></span>
        </div>
      </main>
    </div>

    <Teleport to="body">
      <Transition name="md-fade">
        <div v-if="showCreateModal" class="modal-overlay" @click.self="showCreateModal = false">
          <div class="md-modal proposal-form">
            <div class="modal-header">
              <div class="m-header-left">
                <h2 class="m-title">行动提案申请表 // FORM_1024_A</h2>
                <span class="draft-tag" v-if="isDraftSaved">● 草稿已自动保存</span>
              </div>
              <button class="m-close" @click="showCreateModal = false">×</button>
            </div>

            <div class="modal-body custom-scroll">
              <div class="f-section">
                <div class="f-section-title">01 核心定义 / DEFINITION</div>
                <div class="md-input-box">
                  <label>行动代号 // MISSION_NAME</label>
                  <input v-model="form.name" type="text" placeholder="正式行动名称..." />
                </div>
                <div class="f-row">
                  <div class="md-input-box">
                    <label>发起人 // HOST</label>
                    <input v-model="form.host" type="text" readonly />
                  </div>
                  <div class="md-input-box">
                    <label>协议类别 // TYPE</label>
                    <select v-model="form.type">
                      <option :value="1">协作 (COLLAB)</option>
                      <option :value="2">接力 (RELAY)</option>
                      <option :value="3">竞技 (MATCH)</option>
                    </select>
                  </div>
                </div>
              </div>

              <div class="f-section">
                <div class="f-section-title">02 时间尺度 / TEMPORAL</div>
                <div class="f-row">
                  <div class="md-input-box">
                    <label>启动日期 // START</label>
                    <input v-model="form.startdate" type="date" />
                  </div>
                  <div class="md-input-box">
                    <label>截止日期 // LIMIT</label>
                    <input v-model="form.enddate" type="date" />
                  </div>
                </div>
              </div>

              <div class="f-section">
                <div class="f-section-title">03 详细情报 / INTEL</div>
                <div class="md-input-box">
                  <label>任务目标简报 // BRIEFING</label>
                  <textarea v-model="form.desc" placeholder="详细说明行动内容..."></textarea>
                </div>
                <div class="md-input-box">
                  <label>指令频道 // COMMS (QQ群号)</label>
                  <input v-model="form.QQgroup" type="text" placeholder="群号..." />
                </div>
                <div class="md-input-box">
                  <label>执行协议 // PROTOCOLS</label>
                  <textarea v-model="form.rules" placeholder="24fps, 1080P等条件..."></textarea>
                </div>
              </div>
            </div>

            <div class="modal-footer">
              <button class="md-btn ghost" @click="showCreateModal = false">暂存并关闭 // SUSPEND</button>
              <button class="md-btn primary-ink" @click="submitProposal" :disabled="submitting">
                {{ submitting ? '归档中...' : '提交正式提案 // EXECUTE' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, reactive, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const router = useRouter()
const auth = useAuthStore()

// --- 自动保存配置 ---
const STORAGE_KEY = 'THCY_JOINT_PROPOSAL_DRAFT'
const isDraftSaved = ref(false)

const items = ref([])
const loading = ref(false)
const hasMore = ref(true)
const page = ref(1)
const pageSize = ref(12)
const q = ref('')
const tab = ref('all') // 默认为全部

const showCreateModal = ref(false)
const submitting = ref(false)

// 表单结构
const form = reactive({
  name: '',
  host: auth.user?.username || '',
  type: 1,
  startdate: new Date().toISOString().substr(0, 10),
  enddate: '',
  desc: '',
  QQgroup: '',
  rules: ''
})

const checkExpired = (endDate) => {
  if (!endDate) return false
  return new Date(endDate).getTime() < Date.now()
}

const loadDraft = () => {
  const saved = localStorage.getItem(STORAGE_KEY)
  if (saved) {
    try { Object.assign(form, JSON.parse(saved)); form.host = auth.user?.username || ''; } catch (e) {}
  }
}

watch(form, (newVal) => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(newVal))
  isDraftSaved.value = true
  setTimeout(() => { isDraftSaved.value = false }, 1500)
}, { deep: true })

const fetchList = async (isRefresh = true) => {
  if (loading.value) return
  if (isRefresh) { page.value = 1; items.value = [] }
  loading.value = true
  
  try {
    const params = { q: q.value || undefined, verify: 1, page: page.value, pageSize: pageSize.value }
    const resp = await apiClient.get('ChaiLianHe/list', { params })
    
    if (resp.data?.data) {
      const now = Date.now()
      const rawData = resp.data.data

      // ✅ 核心过滤逻辑
      let filtered = rawData.filter(it => {
        const isExpired = it.enddate ? new Date(it.enddate).getTime() < now : false
        if (tab.value === 'all') return true
        if (tab.value === 'ongoing') return !isExpired
        return isExpired
      })

      items.value = isRefresh ? filtered : [...items.value, ...filtered]
      
      // 自动填补：如果过滤后这页太少，就抓下一页
      if (items.value.length < 6 && resp.data.total > (page.value * pageSize.value)) {
         page.value++
         fetchList(false)
      }
      hasMore.value = items.value.length < resp.data.total
    }
  } catch (err) { console.error(err) } finally { loading.value = false }
}

const submitProposal = async () => {
  if (!form.name || !form.desc) return alert('请确保代号和简报已填写。')
  submitting.value = true
  try {
    await apiClient.post('ChaiLianHe/create', form)
    localStorage.removeItem(STORAGE_KEY)
    alert('提案已提交，正在等待审核。')
    showCreateModal.value = false
    Object.assign(form, { name: '', desc: '', QQgroup: '', rules: '' })
    fetchList(true)
  } catch (err) { alert('提交失败') } finally { submitting.value = false }
}

const viewDetail = (it) => router.push(`/joint/${it.id}`)
const padZero = (n) => n < 10 ? `0${n}` : n
const typeLabelEn = (t) => ({ 1: 'COLLAB', 2: 'RELAY', 3: 'MATCH' }[t] || 'FILE')
const shortDate = (d) => d ? d.substring(0, 10).replace(/-/g, '.') : ''
const onSearchInput = () => fetchList(true)
const switchTab = (t) => { tab.value = t; fetchList(true) }

const handleScroll = (e) => {
  if (!hasMore.value || loading.value) return
  if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight - 50) {
    page.value++; fetchList(false)
  }
}

onMounted(() => { loadDraft(); fetchList(true); })
</script>

<style scoped>
/* --- 📄 完整档案库 CSS 系统 --- */
.archive-terminal {
  --ink: #121212;
  --ink-light: #666;
  --paper: #ffffff;
  --paper-old: #f9f7f0;
  --divider: #e0e0e0;
  --approved: #2e7d32;
  --danger: #b71c1c;
  
  width: 100%; height: 100vh;
  background-color: #f0f0f0;
  color: var(--ink);
  font-family: "PingFang SC", "Segoe UI", sans-serif;
  display: flex; justify-content: center; overflow: hidden;
}

.paper-texture { position: absolute; inset: 0; pointer-events: none; opacity: 0.04; background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200'/%3E"); }
.drafting-grid { position: absolute; inset: 0; pointer-events: none; opacity: 0.1; background-image: linear-gradient(var(--divider) 1px, transparent 1px), linear-gradient(90deg, var(--divider) 1px, transparent 1px); background-size: 30px 30px; }

.archive-container {
  width: 100%; max-width: 1400px; background: var(--paper);
  margin: 20px; box-shadow: 0 10px 40px rgba(0,0,0,0.1);
  display: flex; flex-direction: column; z-index: 2; border: 1px solid var(--divider);
}

/* --- 1. Header --- */
.archive-header { padding: 40px 60px 25px; border-bottom: 5px solid var(--ink); }
.header-main { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }
.main-title { font-size: 3.2rem; font-weight: 900; letter-spacing: -2px; margin: 10px 0; line-height: 1; }
.dept { font-size: 0.75rem; font-weight: bold; color: var(--ink-light); letter-spacing: 3px; font-family: monospace; }
.sub-info { font-size: 0.8rem; font-weight: bold; color: #999; display: flex; align-items: center; gap: 10px; }
.dot { width: 6px; height: 6px; background: var(--approved); border-radius: 50%; display: inline-block; }

.header-controls { display: grid; grid-template-columns: 1fr 480px; gap: 60px; align-items: flex-end; }
.search-field { position: relative; }
.ink-search {
  width: 100%; border: none; background: transparent; font-size: 1.4rem;
  padding: 8px 0; outline: none; border-bottom: 2px solid var(--divider);
  transition: 0.3s;
}
.ink-search:focus { border-bottom-color: var(--ink); }

.tab-selector { display: flex; border: 2px solid var(--ink); border-radius: 4px; overflow: hidden; background: var(--ink); gap: 2px; }
.tab-selector button { flex: 1; border: none; background: #fff; padding: 10px 15px; font-size: 0.7rem; font-weight: 900; cursor: pointer; transition: 0.2s; white-space: nowrap; }
.tab-selector button.active { background: var(--ink); color: #fff; }

/* --- 2. 列表 & 卡片 --- */
.archive-scroll { flex: 1; overflow-y: auto; padding: 40px 60px; }
.archive-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 35px; }

.archive-card {
  display: flex; background: #fff; border: 1px solid var(--divider);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  cursor: pointer; height: 260px; position: relative; overflow: hidden;
}
.archive-card:hover { transform: translateY(-5px); border-color: var(--ink); box-shadow: 0 10px 20px rgba(0,0,0,0.05); }

/* 已结束状态 */
.archive-card.is-expired { background: var(--paper-old); filter: grayscale(0.8); opacity: 0.8; border-style: dashed; }
.archive-card.is-expired .card-sidebar { background: #999; }
.closed-stamp { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%) rotate(-15deg); border: 4px solid var(--danger); color: var(--danger); padding: 5px 15px; font-size: 1.8rem; font-weight: 900; opacity: 0.3; pointer-events: none; letter-spacing: 5px; z-index: 10; text-transform: uppercase; }

.card-sidebar { width: 50px; background: var(--ink); color: #fff; display: flex; flex-direction: column; align-items: center; padding: 20px 0; }
.index-no { font-weight: 900; font-family: monospace; font-size: 0.9rem; }
.type-tag { writing-mode: vertical-rl; transform: rotate(180deg); margin-top: auto; font-size: 0.6rem; font-weight: bold; letter-spacing: 3px; opacity: 0.6; }

.card-content { flex: 1; padding: 25px; display: flex; flex-direction: column; position: relative; }
.card-head { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 12px; }
.card-title { font-size: 1.4rem; font-weight: 900; margin: 0; line-height: 1.1; flex: 1; }

.stamp-ink { border: 3px double #ccc; color: #ccc; padding: 4px 10px; font-size: 0.7rem; font-weight: 900; transform: rotate(-10deg); opacity: 0.5; }
.stamp-ink.approved { border-color: var(--approved); color: var(--approved); opacity: 1; }

.card-meta { display: grid; grid-template-columns: 1fr 1fr; border-top: 1px solid #eee; padding-top: 12px; margin-bottom: 15px; }
.m-item .k { font-size: 0.6rem; color: #999; font-weight: bold; display: block; }
.m-item .v { font-size: 0.85rem; font-weight: bold; }

.card-desc { font-size: 0.85rem; color: var(--ink-light); line-height: 1.6; height: 3.2em; overflow: hidden; }

.card-footer { display: flex; justify-content: space-between; border-top: 1px dashed #eee; padding-top: 12px; margin-top: auto; }
.access-btn { font-size: 0.75rem; font-weight: 900; color: var(--ink); text-decoration: underline; }
.file-ref { font-family: monospace; font-size: 0.65rem; color: #bbb; }

/* --- 3. 弹窗 --- */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); }
.md-modal { background: #fff; width: 680px; max-height: 90vh; border: 6px solid var(--ink); display: flex; flex-direction: column; box-shadow: 0 30px 100px rgba(0,0,0,0.3); }

.modal-header { padding: 30px 40px; border-bottom: 4px solid var(--ink); display: flex; justify-content: space-between; align-items: center; }
.m-title { font-size: 1.4rem; font-weight: 900; }
.draft-tag { font-size: 0.7rem; color: var(--approved); font-weight: bold; margin-left: 15px; animation: blink 2s infinite; }
.m-close { background: none; border: none; font-size: 2rem; cursor: pointer; }

.modal-body { padding: 40px; flex: 1; overflow-y: auto; }
.f-section { margin-bottom: 40px; }
.f-section-title { font-size: 0.8rem; font-weight: 900; background: var(--ink); color: #fff; padding: 5px 15px; display: inline-block; margin-bottom: 20px; }

.md-input-box { margin-bottom: 25px; }
.md-input-box label { font-size: 0.7rem; font-weight: 900; color: #666; margin-bottom: 8px; display: block; }
.md-input-box input, .md-input-box select, .md-input-box textarea { width: 100%; border: 2px solid var(--ink); background: #fff; padding: 12px; font-family: inherit; font-size: 1rem; }
.md-input-box textarea { height: 100px; resize: none; }
.f-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }

.modal-footer { padding: 30px 40px; border-top: 1px solid var(--divider); display: flex; justify-content: flex-end; gap: 20px; }

.md-btn { padding: 12px 30px; font-weight: 900; font-size: 0.85rem; cursor: pointer; border: 2px solid var(--ink); background: #fff; transition: 0.2s; }
.md-btn.primary-ink { background: var(--ink); color: #fff; }
.md-btn.ghost { border: none; background: none; color: #999; text-decoration: underline; }
.md-btn:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 5px 15px rgba(0,0,0,0.1); }

/* --- 4. 辅助 --- */
.end-seal { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 60px 0; opacity: 0.4; }
.end-seal .line { height: 1px; flex: 1; background: var(--ink); }
.end-seal .text { font-size: 0.75rem; font-weight: 900; letter-spacing: 2px; }

@keyframes blink { 50% { opacity: 0.3; } }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--ink); }
</style>