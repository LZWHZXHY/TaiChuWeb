<template>
  <div class="society">
    <header class="head">
      <h2 class="title">柴圈社团</h2>
      <p class="sub">左侧为已收录社团，右侧可提交你的社团信息（提交后进入审核）</p>
    </header>

    <div class="layout">
      <!-- 已收录社团 -->
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

        <!-- 骨架态（极简） -->
        <div v-if="loading" class="rows">
          <article v-for="n in 6" :key="'skel-'+n" class="row skeleton">
            <div class="line lg"></div>
            <div class="line md"></div>
            <div class="line sm"></div>
          </article>
        </div>

        <!-- 列表：仅显示 名称 / 描述 / 团长 / 类型 / 是否考核；隐藏链接，用“申请加群”打开新窗口 -->
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

        <!-- 空态 -->
        <div v-else class="empty">
          <strong>暂无收录社团</strong>
          <span>试试更换筛选条件，或在右侧提交你的社团</span>
        </div>
      </section>

      <!-- 提交社团 -->
      <aside class="submit" aria-labelledby="submit-title">
        <h3 id="submit-title" class="section-title">提交你的社团</h3>

        <form class="form" @submit.prevent="handleSubmit">
          <div class="field">
            <label for="name">社团名称<span class="req">*</span></label>
            <input
              id="name"
              v-model.trim="form.name"
              class="input"
              type="text"
              placeholder="例如：太初工作室"
              required
            />
          </div>

          <div class="field">
            <label for="leader">团长<span class="req">*</span></label>
            <input
              id="leader"
              v-model.trim="form.leader"
              class="input"
              type="text"
              placeholder="联系人/负责人"
              required
            />
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
              <input
                id="size"
                v-model.number="form.size"
                class="input"
                type="number"
                min="1"
                placeholder="至少 1"
                required
              />
            </div>
          </div>

          <div class="field split">
            <div>
              <label>是否需要考核<span class="req">*</span></label>
              <div class="radios">
                <label class="radio">
                  <input type="radio" name="test" :value="1" v-model.number="form.test" />
                  <span>需要</span>
                </label>
                <label class="radio">
                  <input type="radio" name="test" :value="0" v-model.number="form.test" />
                  <span>不需要</span>
                </label>
              </div>
            </div>
            <div>
              <label for="testlevel">考核等级</label>
              <select
                id="testlevel"
                v-model.number="form.testlevel"
                class="select"
                :disabled="form.test !== 1"
              >
                <option :value="0">无</option>
                <option v-for="lvl in [1,2,3,4,5]" :key="lvl" :value="lvl">Lv.{{ lvl }}</option>
              </select>
            </div>
          </div>

          <div class="field">
            <label for="url">主页/加群链接<span class="req">*</span></label>
            <input
              id="url"
              v-model.trim="form.url"
              class="input"
              type="url"
              placeholder="https://…"
              required
            />
          </div>

          <div class="field">
            <label for="desc">社团简介<span class="req">*</span></label>
            <textarea
              id="desc"
              v-model.trim="form.desc"
              class="textarea"
              rows="4"
              placeholder="用 50~200 字介绍社团定位、方向与风格"
              required
            />
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

        <p class="tip">提交后将进入审核，审核通过后展示在列表中（verify: 0→待审；1→通过；2→拒绝）。</p>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'

/* 类型映射（可按后端字典调整） */
const TYPES = [
  { value: 0, label: '综合' },
  { value: 1, label: '动画' },
  { value: 2, label: '分镜' },
  { value: 3, label: '特效' },
  { value: 4, label: '音乐/配音' }
]

/* 列表状态（内部请求 API） */
const societies = ref([])
const loading = ref(false)

/* 仅保留“搜索”功能 */
const query = ref('')

/* 描述展开/收起（按 id 记录） */
const expandedDesc = ref(new Set())
const isDescOpen = (id) => expandedDesc.value.has(id)
const toggleDesc = (id) => {
  const set = new Set(expandedDesc.value)
  if (set.has(id)) set.delete(id); else set.add(id)
  expandedDesc.value = set
}

/* 拉取列表（仅带 verify 与搜索关键字） */
const loadSocietyList = async () => {
  loading.value = true
  try {
    const params = { verify: 1 }
    if (query.value && query.value.trim()) params.q = query.value.trim()

    // 假设 apiClient.baseURL 已含 /api
    const response = await apiClient.get('/ChaiSheTuan/list', { params })
    if (Array.isArray(response.data)) {
      societies.value = response.data
      expandedDesc.value = new Set()
      console.log('✅ 社团列表获取成功:', societies.value.length, '条')
    } else {
      societies.value = []
      console.warn('⚠️ 社团列表返回非数组，已置空')
    }
  } catch (err) {
    console.error('获取社团列表失败，已置空', err)
    if (err?.response?.status === 401 || err?.response?.status === 403) {
      window.dispatchEvent(new Event('unauthorized'))
    }
    societies.value = []
  } finally {
    loading.value = false
  }
}

/* 搜索关键字变更时防抖刷新 */
let queryTimer
watch(query, () => {
  clearTimeout(queryTimer)
  queryTimer = setTimeout(() => loadSocietyList(), 300)
})

onMounted(() => {
  loadSocietyList()
})

/* 展示辅助 */
function typeLabel(v) {
  const f = TYPES.find(x => x.value === Number(v))
  return f ? f.label : `类型 ${v}`
}
function testLabel(test, level) {
  return Number(test) === 1 ? `需要 / Lv.${Number(level) || 0}` : '不需要'
}

/* 表单（直接在组件内调用后端 API 提交） */
const submitting = ref(false)
const form = reactive({
  name: '',
  leader: '',
  type: null,
  test: 0,
  testlevel: 0,
  url: '',
  size: null,
  desc: ''
})

const errorMsg = ref('')
const successMsg = ref('')
const valid = computed(() =>
  !!form.name.trim() &&
  !!form.leader.trim() &&
  Number.isInteger(form.type) &&
  Number.isInteger(form.size) && form.size >= 1 &&
  !!form.url.trim() &&
  !!form.desc.trim() &&
  (form.test === 0 || (form.test === 1 && Number.isInteger(form.testlevel) && form.testlevel >= 1))
)

function resetForm() {
  form.name = ''
  form.leader = ''
  form.type = null
  form.test = 0
  form.testlevel = 0
  form.url = ''
  form.size = null
  form.desc = ''
  errorMsg.value = ''
  successMsg.value = ''
}

async function handleSubmit() {
  errorMsg.value = ''
  successMsg.value = ''
  if (!valid.value) {
    errorMsg.value = '请检查必填项与考核等级（需要考核时需选择 >= Lv.1）'
    return
  }
  submitting.value = true
  try {
    const payload = {
      name: form.name.trim(),
      leader: form.leader.trim(),
      type: Number(form.type),
      test: Number(form.test),
      testlevel: Number(form.test === 1 ? form.testlevel : 0),
      url: form.url.trim(),
      size: Number(form.size),
      desc: form.desc.trim(),
      verify: 0
    }
    const res = await apiClient.post('/ChaiSheTuan/submit', payload)
    console.log('✅ 提交社团成功:', res.data)
    successMsg.value = '已提交，等待审核…'
    resetForm()
    await loadSocietyList()
  } catch (err) {
    console.error('提交社团失败', err)
    if (err?.response?.status === 401 || err?.response?.status === 403) {
      window.dispatchEvent(new Event('unauthorized'))
    }
    errorMsg.value = err?.response?.data?.message || '提交失败，请稍后重试'
  } finally {
    submitting.value = false
  }
}

/* 申请加群：不展示链接本身，点击按钮以新窗口打开 */
function applyJoin(s) {
  if (!s?.url) {
    console.warn('该社团暂无链接')
    return
  }
  window.open(s.url, '_blank', 'noopener,noreferrer')
}
</script>

<style scoped>
:root {
  --bg: #f7f9fc;
  --ink: #0f172a;
  --muted: #5a6478;
  --brand: #2563eb;
  --card: #ffffff;
  --ring: #e6ebf3;
  --ring-strong: #d6deea;
  --r-lg: 16px; --r-md: 12px;
  --shadow-sm: 0 1px 2px rgba(2,6,23,.03), 0 8px 24px rgba(2,6,23,.04);
  --shadow-md: 0 10px 30px rgba(2,6,23,.08);
}

.society { display: grid; gap: 20px; padding: clamp(16px, 3.2vw, 24px); background: var(--bg); color: var(--ink); }

.head { text-align: center; }
.title { margin: 0; font-weight: 900; font-size: clamp(20px, 2.6vw, 26px); letter-spacing: .2px; }
.sub { margin: 6px 0 0; color: var(--muted); }

.layout { display: grid; grid-template-columns: 1.6fr .9fr; gap: 16px; align-items: start; }
@media (max-width: 1000px) { .layout { grid-template-columns: 1fr; } }

.section-title { margin: 0; font-weight: 800; color: #0f1e3a; }

/* Catalog */
.catalog {
  background: var(--card); border: 1px solid var(--ring); border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm); padding: 14px;
}
.catalog-head { display: grid; grid-template-columns: 1fr auto; gap: 10px; align-items: center; margin-bottom: 10px; }
.tools { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.count { color: #6b7280; font-size: 13px; margin-left: 6px; }

.input, .select, .textarea {
  width: 100%; background: #fff; border: 1px solid var(--ring); border-radius: 10px;
  padding: 10px 12px; color: var(--ink); outline: none;
  transition: border-color .16s ease, box-shadow .16s ease, background .16s ease;
}
.input:focus, .select:focus, .textarea:focus { border-color: var(--ring-strong); box-shadow: 0 0 0 3px rgba(37,99,235,.10); }

/* 两列卡片布局 + 响应式回退 */
.rows {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}
@media (max-width: 900px) {
  .rows { grid-template-columns: 1fr; }
}

/* 行卡片：极简、纯文字、清爽留白 */
.row {
  background: #fff; border: 1px solid var(--ring); border-radius: 12px;
  padding: 12px 14px; box-shadow: var(--shadow-sm);
  display: grid; gap: 8px;
  transition: border-color .16s ease, box-shadow .16s ease, transform .12s ease;
}
.row:hover { border-color: var(--ring-strong); box-shadow: var(--shadow-md); transform: translateY(-1px); }

.head-line { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.name { margin: 0; font-weight: 900; font-size: 16px; color: #0f1e3a; letter-spacing: .2px; }

/* 信息行（单行/多行自适应，简洁点状分隔） */
.meta-line {
  display: flex; flex-wrap: wrap; gap: 6px 10px; align-items: center;
  color: #475569; font-size: 14px;
}
.meta-line .meta-item b { color: #0f172a; font-weight: 800; margin-right: 6px; }
.meta-line .dot { color: #cbd5e1; }

/* 描述（两行收起 + 可展开） */
.desc { margin: 0; color: #475569; line-height: 1.6; font-size: 14px; }
.clamp-2 {
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
  overflow: hidden; text-overflow: ellipsis;
}

/* 行内动作：按钮极简 + 文本链接 */
.actions { display: flex; gap: 12px; align-items: center; margin-top: 2px; }
.btn {
  appearance: none; border: 1px solid var(--ring); border-radius: 10px;
  padding: 8px 12px; font-weight: 800; cursor: pointer; background: #fff; color: #0f172a;
}
.btn.small { padding: 8px 12px; font-weight: 700; font-size: 13px; }
.btn.primary { background: var(--brand); color: #000000; border-color: transparent; }
.btn.primary:disabled { opacity: .55; cursor: not-allowed; }
.link.small {
  appearance: none; background: transparent; border: none; padding: 0;
  color: #1d4ed8; font-weight: 700; font-size: 13px; cursor: pointer;
}
.link.small:hover { text-decoration: underline; }

/* 提交面板（保持简约） */
.submit {
  position: sticky; top: 10px;
  background: var(--card); border: 1px solid var(--ring); border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm); padding: 14px;
}
.form { display: grid; gap: 12px; }
.field { display: grid; gap: 6px; }
.field.split { grid-template-columns: 1fr 1fr; gap: 8px; }
@media (max-width: 520px) { .field.split { grid-template-columns: 1fr; } }

label { font-weight: 700; color: #0f1e3a; }
.req { color: #ef4444; margin-left: 2px; }
.radios { display: flex; gap: 12px; align-items: center; }
.radio { display: inline-flex; align-items: center; gap: 6px; color: #0f172a; }

.btn.ghost { background: #fff; color: #0f172a; border-color: var(--ring); }
.btn.ghost:hover { background: #f8fafc; }

.error { color: #b91c1c; background: #fde8e8; border: 1px solid #f9caca; padding: 8px 10px; border-radius: 10px; }
.success { color: #065f46; background: #e7f8f1; border: 1px solid #c6f0df; padding: 8px 10px; border-radius: 10px; }
.tip { margin-top: 8px; color: var(--muted); }

.empty {
  display: grid; justify-items: center; gap: 6px; padding: 18px;
  color: #3f4a5f; background: #fbfdff; border: 1px dashed var(--ring); border-radius: 12px;
}

/* Skeleton（极简） */
.skeleton { position: relative; overflow: hidden; }
.skeleton::after { content: ""; position: absolute; inset: 0; background: linear-gradient(90deg, transparent, rgba(0,0,0,.04), transparent); animation: shimmer 1.2s infinite; }
.line { height: 12px; background: #e8eef7; border-radius: 6px; }
.line.lg { width: 40%; }
.line.md { width: 70%; }
.line.sm { width: 55%; }
@keyframes shimmer { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }
</style>