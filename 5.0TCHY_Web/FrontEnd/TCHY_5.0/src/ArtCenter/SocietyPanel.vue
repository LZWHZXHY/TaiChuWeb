<template>
  <div class="society">
    <header class="head">
      <h2 class="title">CHAI_SOCIETY // 柴圈社团</h2>
      <p class="sub">左侧为已收录社团，右侧可提交你的社团信息（提交后进入审核）</p>
    </header>

    <div class="layout">
      <section class="catalog" aria-labelledby="catalog-title">
        <div class="catalog-head">
          <h3 id="catalog-title" class="section-title">已收录社团</h3>
          <div class="tools">
            <input
              v-model.trim="query"
              type="search"
              class="input"
              placeholder="搜索：社团名称 / 团长…"
              aria-label="搜索社团"
            />
            <span class="count" v-if="!loading">共 {{ societies.length }} 个</span>
            <button class="btn ghost" type="button" @click="loadSocietyList()">刷新</button>
          </div>
        </div>

        <div v-if="loading" class="rows">
          <article v-for="n in 6" :key="'skel-'+n" class="row skeleton">
            <div class="line lg"></div>
            <div class="line md"></div>
            <div class="line sm"></div>
          </article>
        </div>

        <div v-else-if="societies.length" class="rows">
          <article
            v-for="s in societies"
            :key="s.id || s.name"
            class="row"
          >
            <div class="head-line">
              <h4 class="name">{{ s.name }}</h4>
            </div>

            <div class="meta-line">
              <span class="meta-item"><b>团长</b>{{ s.leader }}</span>
              <span class="dot">·</span>
              <span class="meta-item"><b>类型</b>{{ typeLabel(s.type) }}</span>
              <span class="dot">·</span>
              <span class="meta-item"><b>考核</b>{{ testLabel(s.test, s.testlevel) }}</span>
            </div>

            <p class="desc" :class="{ 'clamp-2': !isDescOpen(s.id) }">{{ s.desc }}</p>

            <div class="actions">
              <button
                type="button"
                class="btn primary small"
                :disabled="!s.url"
                @click="applyJoin(s)"
                title="申请加入该社团"
              >
                申请加群
              </button>

              <button
                type="button"
                class="link small"
                :aria-expanded="isDescOpen(s.id)"
                @click="toggleDesc(s.id)"
              >
                {{ isDescOpen(s.id) ? '收起描述' : '展开描述' }}
              </button>
            </div>
          </article>
        </div>

        <div v-else class="empty">
          <strong>暂无收录社团</strong>
          <span>试试更换筛选条件，或在右侧提交你的社团</span>
        </div>
      </section>

      <aside class="submit" aria-labelledby="submit-title">
        <h3 id="submit-title" class="section-title">提交你的社团</h3>

        <form class="form" @submit.prevent="handleSubmit">
          <div class="field">
            <label for="name">社团名称<span class="req">*</span></label>
            <input id="name" v-model.trim="form.name" class="input" type="text" placeholder="例如：太初工作室" required />
          </div>

          <div class="field">
            <label for="leader">团长<span class="req">*</span></label>
            <input id="leader" v-model.trim="form.leader" class="input" type="text" placeholder="联系人/负责人" required />
          </div>

          <div class="field split">
            <div>
              <label for="type">社团类型<span class="req">*</span></label>
              <select id="type" v-model.number="form.type" class="select" required>
                <option :value="null" disabled>选择类型</option>
                <option v-for="t in TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
              </select>
            </div>
            <div>
              <label for="size">成员规模<span class="req">*</span></label>
              <input id="size" v-model.number="form.size" class="input" type="number" min="1" placeholder="至少 1" required />
            </div>
          </div>

          <div class="field split">
            <div>
              <label>是否需要考核<span class="req">*</span></label>
              <div class="radios">
                <label class="radio"><input type="radio" name="test" :value="1" v-model.number="form.test" /><span>需要</span></label>
                <label class="radio"><input type="radio" name="test" :value="0" v-model.number="form.test" /><span>不需要</span></label>
              </div>
            </div>
            <div>
              <label for="testlevel">考核等级</label>
              <select id="testlevel" v-model.number="form.testlevel" class="select" :disabled="form.test !== 1">
                <option :value="0">无</option>
                <option v-for="lvl in [1,2,3,4,5]" :key="lvl" :value="lvl">Lv.{{ lvl }}</option>
              </select>
            </div>
          </div>

          <div class="field">
            <label for="url">主页/加群链接<span class="req">*</span></label>
            <input id="url" v-model.trim="form.url" class="input" type="url" placeholder="https://…" required />
          </div>

          <div class="field">
            <label for="desc">社团简介<span class="req">*</span></label>
            <textarea id="desc" v-model.trim="form.desc" class="textarea" rows="4" placeholder="用 50~200 字介绍社团定位、方向与风格" required />
          </div>

          <div class="form-actions">
            <button class="btn primary" type="submit" :disabled="submitting || !valid">
              {{ submitting ? '提交中…' : '提交社团' }}
            </button>
            <button class="btn ghost" type="button" @click="resetForm">清空</button>
          </div>

          <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
          <p v-if="successMsg" class="success">{{ successMsg }}</p>
        </form>
        <p class="tip">提交后将进入审核，审核通过后展示在列表中。</p>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'

// ... (这里完全粘贴你原来的 script 内容) ...
const TYPES = [
  { value: 0, label: '综合' }, { value: 1, label: '动画' }, { value: 2, label: '分镜' },
  { value: 3, label: '特效' }, { value: 4, label: '音乐/配音' }
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
const loadSocietyList = async () => {
  loading.value = true
  try {
    // 模拟数据 (如果你后端还没写好，用这个测试)
    // await new Promise(r => setTimeout(r, 500));
    // societies.value = [{id:1, name:'太虚绘院', leader:'太初', type:0, test:1, testlevel:3, desc:'官方社团', url:'https://qq.com', size:100}];
    // return;

    const params = { verify: 1 }
    if (query.value && query.value.trim()) params.q = query.value.trim()
    const response = await apiClient.get('/ChaiSheTuan/list', { params })
    if (Array.isArray(response.data)) societies.value = response.data
    else if (response.data && Array.isArray(response.data.data)) societies.value = response.data.data // 兼容分页格式
    else societies.value = []
  } catch (err) {
    console.error(err); societies.value = []
  } finally { loading.value = false }
}
let queryTimer
watch(query, () => { clearTimeout(queryTimer); queryTimer = setTimeout(() => loadSocietyList(), 300) })
onMounted(() => { loadSocietyList() })
function typeLabel(v) { const f = TYPES.find(x => x.value === Number(v)); return f ? f.label : `类型 ${v}` }
function testLabel(test, level) { return Number(test) === 1 ? `需要 / Lv.${Number(level) || 0}` : '不需要' }
const submitting = ref(false)
const form = reactive({ name: '', leader: '', type: null, test: 0, testlevel: 0, url: '', size: null, desc: '' })
const errorMsg = ref(''); const successMsg = ref('')
const valid = computed(() => !!form.name.trim() && !!form.leader.trim() && Number.isInteger(form.type) && Number.isInteger(form.size) && form.size >= 1 && !!form.url.trim() && !!form.desc.trim())
function resetForm() { form.name = ''; form.leader = ''; form.type = null; form.test = 0; form.testlevel = 0; form.url = ''; form.size = null; form.desc = ''; errorMsg.value = ''; successMsg.value = '' }
async function handleSubmit() {
  errorMsg.value = ''; successMsg.value = ''
  if (!valid.value) return
  submitting.value = true
  try {
    const payload = { ...form, type: Number(form.type), size: Number(form.size), verify: 0 }
    await apiClient.post('/ChaiSheTuan/submit', payload)
    successMsg.value = '已提交，等待审核…'; resetForm(); await loadSocietyList()
  } catch (err) { errorMsg.value = '提交失败' } finally { submitting.value = false }
}
function applyJoin(s) { if (s?.url) window.open(s.url, '_blank') }
</script>

<style scoped>
/* 修改点：将变量移动到 .society 内部，避免污染全局，同时适配 ArtCenter 的紫色风格 */
.society { 
  --bg: #f8fafc;
  --ink: #1e293b;
  --muted: #64748b;
  --brand: #8b5cf6; /* 改为 ArtCenter 的紫色 */
  --card: #ffffff;
  --ring: #e2e8f0;
  --ring-strong: #cbd5e1;
  --r-lg: 12px; --r-md: 8px;
  --shadow-sm: 0 1px 3px rgba(0,0,0,0.05);
  --shadow-md: 0 4px 6px -1px rgba(0,0,0,0.1);
  
  display: grid; gap: 20px; padding: 20px; background: var(--bg); color: var(--ink); 
  height: 100%; overflow-y: auto; /* 确保自身可以滚动 */
}

/* 滚动条美化 */
.society::-webkit-scrollbar { width: 6px; }
.society::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }

/* ... (以下样式保持你原来的，或者根据 --brand 自动适配) ... */
.head { text-align: left; padding-bottom: 20px; border-bottom: 1px solid var(--ring); margin-bottom: 20px; }
.title { margin: 0; font-weight: 800; font-size: 20px; color: var(--ink); }
.sub { margin: 5px 0 0; color: var(--muted); font-size: 13px; }

.layout { display: grid; grid-template-columns: 1.8fr 1fr; gap: 20px; align-items: start; }
@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }

/* 你的原有样式复用... */
.section-title { margin: 0; font-weight: 700; color: var(--ink); font-size: 16px; margin-bottom: 15px; border-left: 4px solid var(--brand); padding-left: 10px; }

.catalog, .submit { background: var(--card); border: 1px solid var(--ring); border-radius: var(--r-lg); box-shadow: var(--shadow-sm); padding: 20px; }
.catalog-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; flex-wrap: wrap; gap: 10px; }
.tools { display: flex; gap: 10px; align-items: center; flex: 1; justify-content: flex-end; }
.input, .select, .textarea { width: 100%; padding: 10px; border: 1px solid var(--ring); border-radius: 6px; outline: none; transition: 0.2s; font-size: 13px; }
.input:focus, .select:focus, .textarea:focus { border-color: var(--brand); box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.1); }

.rows { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 15px; }
.row { background: #fff; border: 1px solid var(--ring); border-radius: 8px; padding: 15px; transition: 0.2s; display: flex; flex-direction: column; gap: 10px; }
.row:hover { transform: translateY(-3px); box-shadow: var(--shadow-md); border-color: var(--brand); }
.head-line { display: flex; justify-content: space-between; }
.name { margin: 0; font-weight: bold; font-size: 15px; }
.meta-line { font-size: 12px; color: var(--muted); display: flex; gap: 8px; flex-wrap: wrap; }
.desc { font-size: 13px; color: var(--muted); line-height: 1.5; margin: 0; flex: 1; }
.clamp-2 { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.actions { display: flex; gap: 10px; margin-top: auto; padding-top: 10px; border-top: 1px dashed var(--ring); }

.btn { padding: 8px 16px; border-radius: 6px; font-weight: bold; cursor: pointer; border: none; font-size: 12px; transition: 0.2s; }
.btn.primary { background: var(--brand); color: #fff; }
.btn.primary:hover { background: #7c3aed; }
.btn.ghost { background: transparent; color: var(--muted); border: 1px solid var(--ring); }
.btn.ghost:hover { color: var(--brand); border-color: var(--brand); }
.link.small { background: none; border: none; color: var(--brand); cursor: pointer; font-size: 12px; padding: 0; }

.form { display: grid; gap: 15px; }
.field { display: grid; gap: 5px; }
.field.split { grid-template-columns: 1fr 1fr; gap: 15px; }
label { font-size: 12px; font-weight: bold; color: var(--muted); }
.req { color: #ef4444; }
.radios { display: flex; gap: 15px; font-size: 13px; }
.radio { display: flex; align-items: center; gap: 5px; cursor: pointer; }
.tip { font-size: 12px; color: var(--muted); margin-top: 10px; }
.error { color: #ef4444; font-size: 12px; } 
.success { color: #10b981; font-size: 12px; }

/* Skeleton */
.skeleton .line { height: 14px; background: #f1f5f9; border-radius: 4px; margin-bottom: 8px; }
</style>