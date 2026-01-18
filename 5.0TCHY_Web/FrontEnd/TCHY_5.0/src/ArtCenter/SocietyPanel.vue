<template>
  <div class="society-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="society-container">
      
      <header class="console-header">
        <div class="header-left">
          <div class="title-block">
            <span class="icon-box">SOC</span>
            <div class="text-group">
              <h2>SOCIETY_ARCHIVE</h2>
              <span class="sub">社团档案库 // REGISTERED_UNITS</span>
            </div>
          </div>
        </div>

        <div class="header-right">
          <div class="search-module">
            <span class="prompt">>> QUERY:</span>
            <input
              v-model.trim="query"
              type="text"
              class="cyber-input-search"
              placeholder="NAME / LEADER..."
            />
            <button class="cyber-btn-sm" @click="loadSocietyList">
              <span v-if="loading" class="blink">SCANNING...</span>
              <span v-else>EXECUTE_SCAN</span>
            </button>
          </div>
          <div class="total-counter">
            INDEX_COUNT: <span class="val">{{ societies.length }}</span>
          </div>
        </div>
      </header>

      <div class="main-terminal-layout">
        
        <section class="archive-panel custom-scroll">
          <div class="panel-label">
            <span class="deco">■</span> ARCHIVED_UNITS // 已收录社团
          </div>

          <div v-if="loading" class="loading-state">
            <div class="loader-spinner"></div>
            <span>DOWNLOADING_ARCHIVES...</span>
          </div>

          <div v-else-if="societies.length" class="society-list">
            <article
              v-for="(s, index) in societies"
              :key="s.id || index"
              class="society-card"
            >
              <div class="card-header">
                <div class="header-top">
                  <span class="unit-id">UNIT_{{ padZero(index + 1) }}</span>
                  <span class="unit-type">{{ typeLabel(s.type) }}</span>
                </div>
                <h3 class="unit-name">{{ s.name }}</h3>
              </div>

              <div class="card-data-grid">
                <div class="data-cell">
                  <span class="label">LEADER</span>
                  <span class="val">{{ s.leader }}</span>
                </div>
                <div class="data-cell">
                  <span class="label">SIZE</span>
                  <span class="val">{{ s.size || '?' }} MEMS</span>
                </div>
                <div class="data-cell full">
                  <span class="label">ACCESS_REQ // 考核</span>
                  <div class="access-badge" :class="s.test === 1 ? 'restricted' : 'public'">
                    {{ s.test === 1 ? `LV.${s.testlevel || 1}_CLEARANCE` : 'OPEN_ACCESS' }}
                  </div>
                </div>
              </div>

              <div class="card-desc">
                <div class="desc-content" :class="{ 'expanded': isDescOpen(s.id) }">
                  {{ s.desc || 'NO_DATA_AVAILABLE.' }}
                </div>
                <button 
                  class="expand-btn" 
                  @click="toggleDesc(s.id)"
                  v-if="s.desc && s.desc.length > 50"
                >
                  [{{ isDescOpen(s.id) ? '-' : '+' }}] {{ isDescOpen(s.id) ? 'COLLAPSE' : 'EXPAND' }}
                </button>
              </div>

              <div class="card-footer">
                <button
                  class="action-btn"
                  :disabled="!s.url"
                  @click="applyJoin(s)"
                >
                  <span class="btn-deco">>>></span>
                  REQUEST_ENTRY // 申请加群
                </button>
              </div>
              
              <div class="side-accent"></div>
            </article>
          </div>

          <div v-else class="empty-state">
            [ NO_MATCHING_RECORDS_FOUND ]
          </div>
        </section>

        <aside class="registry-panel custom-scroll">
          <div class="panel-label">
            <span class="deco">▼</span> NEW_UNIT_REGISTRATION // 提交申请
          </div>

          <form class="cyber-form" @submit.prevent="handleSubmit">
            <div class="form-group">
              <label>UNIT_NAME / 社团名称 <span class="req">*</span></label>
              <input v-model.trim="form.name" class="cyber-input" placeholder="ENTER_NAME..." required />
            </div>

            <div class="form-group">
              <label>COMMANDER / 团长 <span class="req">*</span></label>
              <input v-model.trim="form.leader" class="cyber-input" placeholder="ENTER_ID..." required />
            </div>

            <div class="form-row">
              <div class="form-group half">
                <label>TYPE / 类型 <span class="req">*</span></label>
                <select v-model.number="form.type" class="cyber-select" required>
                  <option :value="null" disabled>SELECT...</option>
                  <option v-for="t in TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
                </select>
              </div>
              <div class="form-group half">
                <label>SCALE / 规模 <span class="req">*</span></label>
                <input v-model.number="form.size" class="cyber-input" type="number" min="1" required />
              </div>
            </div>

            <div class="form-group">
              <label>ACCESS_CONTROL / 考核设置 <span class="req">*</span></label>
              <div class="cyber-radios">
                <div 
                  class="radio-opt" 
                  :class="{ active: form.test === 1 }"
                  @click="form.test = 1"
                >
                  RESTRICTED
                </div>
                <div 
                  class="radio-opt" 
                  :class="{ active: form.test === 0 }"
                  @click="form.test = 0"
                >
                  OPEN
                </div>
              </div>
            </div>

            <div class="form-group" v-if="form.test === 1">
              <label>CLEARANCE_LEVEL / 考核等级</label>
              <select v-model.number="form.testlevel" class="cyber-select">
                <option :value="0">NONE</option>
                <option v-for="lvl in [1,2,3,4,5]" :key="lvl" :value="lvl">LEVEL {{ lvl }}</option>
              </select>
            </div>

            <div class="form-group">
              <label>LINK / 加群链接 <span class="req">*</span></label>
              <input v-model.trim="form.url" class="cyber-input" type="url" placeholder="HTTPS://..." required />
            </div>

            <div class="form-group">
              <label>MANIFESTO / 简介 <span class="req">*</span></label>
              <textarea v-model.trim="form.desc" class="cyber-textarea" rows="4" placeholder="DESCRIPTION..." required></textarea>
            </div>

            <div v-if="errorMsg" class="msg-box error">>> ERROR: {{ errorMsg }}</div>
            <div v-if="successMsg" class="msg-box success">>> SUCCESS: {{ successMsg }}</div>

            <div class="form-actions">
              <button class="cyber-btn primary" type="submit" :disabled="submitting || !valid">
                {{ submitting ? 'TRANSMITTING...' : 'SUBMIT_REGISTRY' }}
              </button>
              <button class="cyber-btn ghost" type="button" @click="resetForm">
                RESET_FORM
              </button>
            </div>
            
            <div class="tip-text">
              * NOTE: 提交后将进入人工审核队列。
            </div>
          </form>
        </aside>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'

// --- 逻辑部分保持不变，仅适配新结构 ---
const TYPES = [
  { value: 0, label: '综合 (GENERAL)' }, 
  { value: 1, label: '动画 (ANIMATION)' }, 
  { value: 2, label: '分镜 (STORYBOARD)' },
  { value: 3, label: '特效 (VFX)' }, 
  { value: 4, label: '音乐/配音 (AUDIO)' }
]
const societies = ref([])
const loading = ref(false)
const query = ref('')
const expandedDesc = ref(new Set())

const isDescOpen = (id) => expandedDesc.value.has(id)
const toggleDesc = (id) => {
  const set = new Set(expandedDesc.value)
  if (set.has(id)) set.delete(id); else set.add(id)
  expandedDesc.value = set
}

const padZero = (n) => n < 10 ? `0${n}` : n

const loadSocietyList = async () => {
  loading.value = true
  try {
    const params = { verify: 1 }
    if (query.value && query.value.trim()) params.q = query.value.trim()
    
    // 使用真实 API
    const response = await apiClient.get('/ChaiSheTuan/list', { params })
    
    if (Array.isArray(response.data)) societies.value = response.data
    else if (response.data && Array.isArray(response.data.data)) societies.value = response.data.data
    else societies.value = []
  } catch (err) {
    console.error(err); societies.value = []
  } finally { loading.value = false }
}

let queryTimer
watch(query, () => { clearTimeout(queryTimer); queryTimer = setTimeout(() => loadSocietyList(), 300) })

onMounted(() => { loadSocietyList() })

function typeLabel(v) { 
  const f = TYPES.find(x => x.value === Number(v)); 
  return f ? f.label.split(' ')[0] : `TYPE_${v}` 
}

const submitting = ref(false)
const form = reactive({ name: '', leader: '', type: null, test: 0, testlevel: 0, url: '', size: null, desc: '' })
const errorMsg = ref(''); const successMsg = ref('')

const valid = computed(() => !!form.name.trim() && !!form.leader.trim() && Number.isInteger(form.type) && Number.isInteger(form.size) && form.size >= 1 && !!form.url.trim() && !!form.desc.trim())

function resetForm() { 
  Object.assign(form, { name: '', leader: '', type: null, test: 0, testlevel: 0, url: '', size: null, desc: '' })
  errorMsg.value = ''; successMsg.value = '' 
}

async function handleSubmit() {
  errorMsg.value = ''; successMsg.value = ''
  if (!valid.value) return
  submitting.value = true
  try {
    const payload = { ...form, type: Number(form.type), size: Number(form.size), verify: 0 }
    await apiClient.post('/ChaiSheTuan/submit', payload)
    successMsg.value = 'REGISTRY_RECEIVED. PENDING_APPROVAL.'; 
    resetForm(); 
    await loadSocietyList()
  } catch (err) { errorMsg.value = 'TRANSMISSION_FAILED' } 
  finally { submitting.value = false }
}

function applyJoin(s) { if (s?.url) window.open(s.url, '_blank') }
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.society-industrial {
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
.society-container {
  position: relative; z-index: 1;
  flex: 1; display: flex; flex-direction: column;
  padding: 20px; gap: 20px;min-height: 0;      /* 关键：允许子元素溢出处理 */
  overflow: hidden;
}

/* --- 1. 头部控制台 --- */
.console-header {
  background: var(--white);
  border: 4px solid var(--black);
  padding: 15px 20px;
  display: flex; justify-content: space-between; align-items: center;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
  flex-wrap: wrap; gap: 15px;
  flex-shrink: 0;
}

.title-block { display: flex; align-items: center; gap: 15px; }
.icon-box {
  background: var(--black); color: var(--white);
  font-family: var(--heading); font-size: 1.5rem;
  padding: 5px 10px; transform: skew(-10deg);
}
.text-group h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-group .sub { font-size: 0.7rem; font-weight: bold; color: #666; }

.header-right { display: flex; align-items: center; gap: 20px; }
.search-module { display: flex; align-items: center; gap: 10px; }
.prompt { font-weight: bold; color: var(--red); }
.cyber-input-search {
  border: none; border-bottom: 2px solid var(--black);
  background: transparent; padding: 5px; width: 200px;
  font-family: var(--mono); outline: none; font-weight: bold;
}
.cyber-input-search:focus { border-color: var(--red); }

.cyber-btn-sm {
  background: var(--black); color: var(--white); border: none;
  padding: 5px 10px; cursor: pointer; font-family: var(--mono); font-weight: bold;
}
.cyber-btn-sm:hover { background: var(--red); }

.total-counter { font-size: 0.8rem; font-weight: bold; border-left: 2px solid #ccc; padding-left: 15px; }
.total-counter .val { color: var(--red); font-size: 1.2rem; margin-left: 5px; }

/* --- 主布局 (双列) --- */
.main-terminal-layout {
  flex: 1;            /* 占据剩余全部高度 */
  display: flex;
  gap: 20px;
  min-height: 0;      /* 极其关键：这是让子面板 overflow 生效的前提 */
  overflow: hidden;   /* 容器本身不滚动 */
}

/* --- 2. 左侧：档案列表 --- */
.archive-panel {
  flex: 2;
  background: var(--white);
  border: 2px solid var(--black);
  padding: 20px;
  
  /* 确保这里滚动生效 */
  display: flex;
  flex-direction: column;
  overflow-y: auto;   /* 开启独立滚动 */
  min-height: 0;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
  
  /* 增加底部垫片，防止最后一个卡片滚不到底 */
  padding-bottom: 60px; 
}

.panel-label {
  font-family: var(--heading); font-size: 1.2rem;
  border-bottom: 2px solid var(--black); padding-bottom: 10px; margin-bottom: 20px;
}
.deco { color: var(--red); margin-right: 10px; }

.society-list {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

/* 社团卡片 */
.society-card {
  background: #fff; border: 2px solid var(--black);
  position: relative; padding: 15px;
  display: flex; flex-direction: column; gap: 10px;
  transition: all 0.2s;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.05);
}
.society-card:hover {
  transform: translate(-3px, -3px);
  box-shadow: 6px 6px 0 var(--red);
  border-color: var(--black);
}

.side-accent {
  position: absolute; top: 0; bottom: 0; left: 0; width: 4px; background: var(--black);
}
.society-card:hover .side-accent { background: var(--red); }

.card-header { border-bottom: 1px dashed #ccc; padding-bottom: 8px; margin-left: 10px; }
.header-top { display: flex; justify-content: space-between; font-size: 0.65rem; color: #888; margin-bottom: 4px; }
.unit-id { font-family: monospace; }
.unit-type { font-weight: bold; color: var(--black); }
.unit-name { font-family: var(--heading); font-size: 1.4rem; margin: 0; line-height: 1.1; }

.card-data-grid {
  display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-left: 10px;
  background: #f9f9f9; padding: 8px; border: 1px solid #eee;
}
.data-cell { display: flex; flex-direction: column; }
.data-cell.full { grid-column: span 2; border-top: 1px dashed #ddd; padding-top: 5px; margin-top: 2px; }
.data-cell .label { font-size: 0.6rem; color: #888; font-weight: bold; }
.data-cell .val { font-size: 0.8rem; font-weight: bold; }

.access-badge {
  font-size: 0.7rem; font-weight: bold; padding: 2px 6px; display: inline-block; width: fit-content;
  margin-top: 2px;
}
.access-badge.restricted { background: var(--black); color: var(--red); }
.access-badge.public { background: #eee; color: #2ecc71; border: 1px solid #2ecc71; }

.card-desc { margin-left: 10px; flex: 1; display: flex; flex-direction: column; }
.desc-content {
  font-size: 0.8rem; color: #555; line-height: 1.5;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}
.desc-content.expanded { -webkit-line-clamp: unset; display: block; }
.expand-btn {
  background: none; border: none; color: var(--black);
  font-size: 0.65rem; font-weight: bold; cursor: pointer;
  margin-top: 5px; align-self: flex-start;
}
.expand-btn:hover { color: var(--red); }

.card-footer { margin-top: auto; padding-top: 15px; margin-left: 10px; }
.action-btn {
  width: 100%; background: var(--black); color: var(--white);
  border: none; padding: 8px; font-family: var(--heading);
  cursor: pointer; transition: 0.2s;
  display: flex; align-items: center; justify-content: center; gap: 10px;
}
.action-btn:hover { background: var(--red); }
.action-btn:disabled { background: #ccc; cursor: not-allowed; }
.btn-deco { font-size: 0.8rem; letter-spacing: -2px; }

.registry-panel {
  flex: 1;
  min-width: 350px;
  max-width: 450px;
  background: #f4f4f4;
  border: 2px solid var(--black);
  padding: 20px;
  
  /* 确保这里滚动生效 */
  overflow-y: auto;   /* 开启独立滚动 */
  min-height: 0;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
  
  /* 底部垫片 */
  padding-bottom: 60px;
}

.cyber-form { display: flex; flex-direction: column; gap: 15px; }
.form-group { display: flex; flex-direction: column; gap: 5px; }
.form-row { display: flex; gap: 15px; }
.form-group.half { flex: 1; }

label { font-size: 0.75rem; font-weight: bold; color: var(--black); }
.req { color: var(--red); }

.cyber-input, .cyber-select, .cyber-textarea {
  width: 100%; padding: 10px; border: 2px solid #ccc;
  background: var(--white); font-family: var(--mono); font-size: 0.9rem;
  outline: none; transition: 0.2s;
}
.cyber-input:focus, .cyber-select:focus, .cyber-textarea:focus {
  border-color: var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.cyber-radios { display: flex; border: 2px solid var(--black); background: #ccc; }
.radio-opt {
  flex: 1; text-align: center; padding: 8px; font-size: 0.8rem; font-weight: bold;
  cursor: pointer; transition: 0.2s;
}
.radio-opt.active { background: var(--black); color: var(--white); }
.radio-opt:hover:not(.active) { background: #fff; }

.form-actions { display: flex; gap: 10px; margin-top: 20px; }
.cyber-btn {
  flex: 1; padding: 12px; font-family: var(--heading); font-size: 1.1rem;
  border: none; cursor: pointer; transition: 0.2s;
}
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover { background: var(--red); box-shadow: 4px 4px 0 var(--black); transform: translate(-2px, -2px); }
.cyber-btn.ghost { background: transparent; border: 2px solid var(--black); color: var(--black); }
.cyber-btn.ghost:hover { background: #fff; }
.cyber-btn:disabled { background: #ccc; cursor: wait; transform: none; box-shadow: none; }

.msg-box { padding: 10px; font-size: 0.8rem; font-weight: bold; border: 1px solid; }
.msg-box.error { color: var(--red); border-color: var(--red); background: rgba(217, 35, 35, 0.1); }
.msg-box.success { color: #2ecc71; border-color: #2ecc71; background: rgba(46, 204, 113, 0.1); }

.tip-text { font-size: 0.7rem; color: #666; margin-top: 10px; text-align: center; }

/* Loaders & Empty */
.loading-state, .empty-state {
  text-align: center; padding: 40px; color: #888; font-weight: bold;
  border: 2px dashed #ccc; margin: 20px 0;
}
.loader-spinner {
  width: 24px; height: 24px; border: 3px solid #ccc; border-top-color: var(--black);
  border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 10px;
}
.blink { animation: blink 1s infinite; }

@keyframes spin { to { transform: rotate(360deg); } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* 响应式 */
@media (max-width: 1024px) {
  .main-terminal-layout { flex-direction: column; }
  .registry-panel { width: 100%; max-width: none; border-left: 2px solid var(--black); }
  .archive-panel { box-shadow: none; }
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>