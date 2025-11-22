<template>
  <div class="fb-root">
    <header class="fb-header">
      <h1 class="fb-h1">意见箱</h1>
      <p class="fb-sub">欢迎留下你的想法或问题，我们重视每一条反馈。</p>
    </header>

    <main class="fb-layout">
      <!-- 提交表单 -->
      <section class="fb-panel fb-form">
        <h2 class="fb-h2">提交反馈</h2>

        <form @submit.prevent="onSubmit" novalidate>
          <!-- 分类 -->
          <div class="fb-field">
            <label for="category" class="fb-label required">分类</label>
            <select
              id="category"
              v-model="form.category"
              class="fb-select"
              :class="{ invalid: touched.category && !valid.category }"
              @blur="touched.category = true"
            >
              <option disabled value="">请选择分类</option>
              <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
            </select>
            <p v-if="touched.category && !valid.category" class="fb-error">请选择分类</p>
          </div>

          <!-- 标题 -->
          <div class="fb-field">
            <label for="title" class="fb-label required">标题</label>
            <input
              id="title"
              type="text"
              v-model.trim="form.title"
              :maxlength="rules.title.max"
              class="fb-input"
              placeholder="简要概括你的反馈"
              @blur="touched.title = true"
              :class="{ invalid: touched.title && !valid.title }"
            />
            <div class="fb-hint-row">
              <span class="fb-hint">建议 {{ rules.title.min }}-{{ rules.title.max }} 字</span>
              <span class="fb-count">{{ form.title.length }}/{{ rules.title.max }}</span>
            </div>
            <p v-if="touched.title && !valid.title" class="fb-error">
              标题需 {{ rules.title.min }}-{{ rules.title.max }} 字
            </p>
          </div>

            <!-- 内容 -->
          <div class="fb-field">
            <label for="content" class="fb-label required">详细描述</label>
            <textarea
              id="content"
              v-model.trim="form.content"
              :maxlength="rules.content.max"
              rows="6"
              class="fb-textarea"
              placeholder="描述问题现象、复现步骤、期望结果等"
              @blur="touched.content = true"
              @paste="onPaste"
              :class="{ invalid: touched.content && !valid.content }"
            ></textarea>
            <div class="fb-hint-row">
              <span class="fb-hint">至少 {{ rules.content.min }} 字</span>
              <span class="fb-count">{{ form.content.length }}/{{ rules.content.max }}</span>
            </div>
            <p v-if="touched.content && !valid.content" class="fb-error">
              内容需 {{ rules.content.min }}-{{ rules.content.max }} 字
            </p>
          </div>

          <!-- 图片上传 -->
          <div class="fb-field">
            <label class="fb-label">图片 / 截图（可选）</label>
            <div class="fb-upload-row">
              <label
                class="fb-upload-tile"
                :class="{ disabled: form.images.length >= rules.images.maxCount }"
              >
                <input
                  type="file"
                  accept="image/*"
                  multiple
                  :disabled="form.images.length >= rules.images.maxCount"
                  @change="onFiles"
                />
                <span class="fb-upload-plus">+</span>
                <span class="fb-upload-text">上传</span>
              </label>
              <div v-for="(img, i) in form.images" :key="img" class="fb-thumb">
                <img :src="img" alt="预览图片" />
                <button
                  type="button"
                  class="fb-thumb-remove"
                  @click="removeImage(i)"
                  aria-label="删除"
                >×</button>
              </div>
            </div>
            <p class="fb-hint">
              最多 {{ rules.images.maxCount }} 张，单张 ≤ {{ (rules.images.maxSize/1024/1024).toFixed(0) }}MB，支持粘贴
            </p>
            <p v-if="errors.images" class="fb-error">{{ errors.images }}</p>
          </div>

          <!-- 联系方式 / 匿名 -->
          <div class="fb-field fb-grid-2">
            <div>
              <label for="contact" class="fb-label">联系方式（可选）</label>
              <input
                id="contact"
                type="text"
                v-model.trim="form.contact"
                class="fb-input"
                placeholder="邮箱 / 手机（便于跟进）"
                :disabled="form.anonymous"
                @blur="touched.contact = true"
                :class="{ invalid: touched.contact && !valid.contact }"
              />
              <p v-if="touched.contact && !valid.contact" class="fb-error">
                格式不正确（示例：name@domain.com 或 11 位手机号）
              </p>
            </div>
            <label class="fb-check">
              <input type="checkbox" v-model="form.anonymous" />
              <span>匿名提交</span>
            </label>
          </div>

          <!-- 隐私同意 -->
          <div class="fb-field">
            <label class="fb-check">
              <input
                type="checkbox"
                v-model="form.consent"
                @blur="touched.consent = true"
              />
              <span>同意用于改进服务</span>
            </label>
            <p v-if="touched.consent && !valid.consent" class="fb-error">请勾选同意</p>
          </div>

          <!-- 操作 -->
          <div class="fb-actions">
            <button type="submit" class="fb-btn primary" :disabled="!formValid || loading">
              <span v-if="loading" class="fb-spinner" aria-hidden="true"></span>
              提交
            </button>
            <button type="button" class="fb-btn" @click="onReset" :disabled="loading">清空</button>
          </div>

          <!-- 消息 -->
          <p v-if="message.text" :class="['fb-msg', message.type]">{{ message.text }}</p>
        </form>
      </section>

      <!-- 列表 -->
      <section class="fb-panel fb-list">
        <div class="fb-list-head">
          <h2 class="fb-h2">反馈列表</h2>
          <div class="fb-filters">
            <div class="fb-tabs" role="tablist" aria-label="状态筛选">
              <button
                v-for="s in statusesForTabs"
                :key="s.value"
                class="fb-tab"
                :class="{ active: filter.status === s.value }"
                @click="filter.status = s.value; filter.page = 1"
                role="tab"
              >
                {{ s.label }}
              </button>
            </div>
            <input
              type="search"
              v-model.trim="filter.q"
              class="fb-input fb-search"
              placeholder="搜索"
              @input="filter.page = 1"
              aria-label="搜索反馈"
            />
          </div>
        </div>

        <div v-if="pagedItems.length === 0" class="fb-empty">暂无反馈</div>

        <ul v-else class="fb-items">
          <li v-for="item in pagedItems" :key="item.id" class="fb-item">
            <div class="fb-item-row1">
              <h3 class="fb-item-title">{{ item.title }}</h3>
              <span class="fb-status">
                <span class="fb-dot" :data-status="item.status"></span>{{ item.status }}
              </span>
            </div>
            <div class="fb-item-meta">
              <span>{{ item.category }}</span>
              <span class="sep">·</span>
              <time>{{ formatDate(item.createdAt) }}</time>
              <template v-if="item.images?.length">
                <span class="sep">·</span>
                <span>{{ item.images.length }} 图</span>
              </template>
              <template v-if="item.anonymous">
                <span class="sep">·</span>
                <span>匿名</span>
              </template>
            </div>
            <p class="fb-item-content">{{ snippet(item.content) }}</p>
            <div v-if="item.images?.length" class="fb-mini-row">
              <img
                v-for="(img, i) in item.images"
                :key="i"
                :src="img"
                alt="缩略图"
                class="fb-mini"
                @click="preview(img)"
              />
            </div>
          </li>
        </ul>

        <div v-if="totalPages > 1" class="fb-pager">
          <button class="fb-btn small" :disabled="filter.page === 1" @click="filter.page--">上一页</button>
          <span class="fb-hint">第 {{ filter.page }} / {{ totalPages }} 页</span>
          <button class="fb-btn small" :disabled="filter.page === totalPages" @click="filter.page++">下一页</button>
        </div>
      </section>
    </main>

    <!-- 图片预览 -->
    <div
      v-if="previewUrl"
      class="fb-preview"
      @click="previewUrl = ''"
      role="dialog"
      aria-modal="true"
    >
      <img :src="previewUrl" alt="图片预览" @click.stop />
      <button class="fb-close" @click="previewUrl = ''" aria-label="关闭预览">×</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed, watch } from 'vue'

type Category = '功能建议' | 'BUG反馈' | '内容建议' | '其他'
type Status = '未读' | '处理中' | '已完成'
interface FeedbackItem {
  id: string
  category: Category
  title: string
  content: string
  images: string[]
  contact?: string
  anonymous: boolean
  createdAt: string
  status: Status
}

const STORAGE_KEY = 'feedback_box_items'
const categories: Category[] = ['功能建议', 'BUG反馈', '内容建议', '其他']
const statusTabs: Array<{ label: string; value: Status | '全部' }> = [
  { label: '全部', value: '全部' },
  { label: '未读', value: '未读' },
  { label: '处理中', value: '处理中' },
  { label: '已完成', value: '已完成' }
]

const rules = {
  title: { min: 6, max: 40 },
  content: { min: 10, max: 1000 },
  images: { maxCount: 5, maxSize: 2 * 1024 * 1024 }
}

const form = reactive({
  category: '' as '' | Category,
  title: '',
  content: '',
  images: [] as string[],
  contact: '',
  anonymous: false,
  consent: true
})

const touched = reactive({
  category: false,
  title: false,
  content: false,
  contact: false,
  consent: false
})
const errors = reactive({ images: '' })
const loading = ref(false)
const message = reactive({ text: '', type: '' as 'success' | 'error' | '' })
const previewUrl = ref('')

const items = ref<FeedbackItem[]>(loadItems())
const filter = reactive({
  status: '全部' as Status | '全部',
  q: '',
  page: 1,
  pageSize: 6
})
const statusesForTabs = statusTabs

function loadItems(): FeedbackItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}
function saveItems(arr: FeedbackItem[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(arr))
}

const filtered = computed(() => {
  const q = filter.q.trim().toLowerCase()
  return items.value
    .filter(it => (filter.status === '全部' ? true : it.status === filter.status))
    .filter(it => (q ? (it.title + it.content).toLowerCase().includes(q) : true))
    .sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt))
})
const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / filter.pageSize)))
const pagedItems = computed(() => {
  const start = (filter.page - 1) * filter.pageSize
  return filtered.value.slice(start, start + filter.pageSize)
})
watch([() => filter.status, () => filter.q], () => {
  filter.page = 1
})

const valid = reactive({
  category: computed(() => !!form.category),
  title: computed(
    () => form.title.length >= rules.title.min && form.title.length <= rules.title.max
  ),
  content: computed(
    () => form.content.length >= rules.content.min && form.content.length <= rules.content.max
  ),
  contact: computed(() => {
    if (form.anonymous || !form.contact) return true
    const email = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    const mobile = /^1[3-9]\d{9}$/
    return email.test(form.contact) || mobile.test(form.contact)
  }),
  consent: computed(() => !!form.consent),
  images: computed(() => !errors.images)
})
const formValid = computed(
  () => valid.category && valid.title && valid.content && valid.contact && valid.consent && valid.images
)

watch(
  () => form.anonymous,
  v => {
    if (v) form.contact = ''
  }
)

function setMessage(text: string, type: 'success' | 'error', timeout = 2500) {
  message.text = text
  message.type = type
  if (timeout) {
    setTimeout(() => {
      if (message.text === text) message.text = ''
    }, timeout)
  }
}
function onReset() {
  form.category = ''
  form.title = ''
  form.content = ''
  form.images = []
  form.contact = ''
  form.anonymous = false
  form.consent = true
  Object.keys(touched).forEach(k => ((touched as any)[k] = false))
  errors.images = ''
}
function uid() {
  return Math.random().toString(36).slice(2, 8) + Date.now().toString(36).slice(-6)
}
function snippet(s: string, max = 140) {
  return s.length > max ? s.slice(0, max) + '…' : s
}
function formatDate(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(
    d.getHours()
  )}:${pad(d.getMinutes())}`
}
function preview(url: string) {
  previewUrl.value = url
}

async function dataURLFromFile(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const r = new FileReader()
    r.onload = () => resolve(String(r.result))
    r.onerror = reject
    r.readAsDataURL(file)
  })
}
async function handleFiles(files: FileList | File[]) {
  errors.images = ''
  const arr = Array.from(files)
  if (!arr.length) return
  const available = rules.images.maxCount - form.images.length
  for (const f of arr.slice(0, available)) {
    if (!f.type.startsWith('image/')) {
      errors.images = '仅支持图片'
      continue
    }
    if (f.size > rules.images.maxSize) {
      errors.images = `单张需 ≤ ${(rules.images.maxSize / 1024 / 1024).toFixed(0)}MB`
      continue
    }
    form.images.push(await dataURLFromFile(f))
  }
  if (arr.length > available) setMessage(`最多 ${rules.images.maxCount} 张，多余已忽略`, 'error')
}
function onFiles(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files) return
  handleFiles(input.files)
  input.value = ''
}
function removeImage(i: number) {
  form.images.splice(i, 1)
}
async function onPaste(e: ClipboardEvent) {
  const items = e.clipboardData?.items
  if (!items) return
  const files: File[] = []
  for (const it of items as any) {
    if (it.kind === 'file') {
      const f = it.getAsFile()
      if (f) files.push(f)
    }
  }
  if (files.length) {
    e.preventDefault()
    await handleFiles(files)
  }
}

async function onSubmit() {
  touched.category = touched.title = touched.content = touched.consent = touched.contact = true
  if (!formValid.value) {
    setMessage('请检查表单填写', 'error')
    return
  }
  loading.value = true
  try {
    const payload: FeedbackItem = {
      id: uid(),
      category: form.category as Category,
      title: form.title.trim(),
      content: form.content.trim(),
      images: [...form.images],
      contact: form.anonymous ? '' : form.contact.trim(),
      anonymous: form.anonymous,
      createdAt: new Date().toISOString(),
      status: '未读'
    }
    await new Promise(r => setTimeout(r, 500))
    const next = [payload, ...items.value]
    items.value = next
    saveItems(next)
    setMessage('提交成功，感谢你的反馈', 'success')
    onReset()
  } catch (e) {
    console.error(e)
    setMessage('提交失败，请稍后再试', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* 基础变量 */
.fb-root {
  --font-stack: system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif;
  --bg: #f9fafb;
  --panel-bg: #ffffff;
  --border: #e5e7eb;
  --border-strong: #d1d5db;
  --fg: #111827;
  --fg-soft: #374151;
  --mute: #6b7280;
  --accent: #2563eb;
  --accent-bg: #eff6ff;
  --danger: #dc2626;
  --success: #059669;
  --radius-sm: 4px;
  --radius: 8px;
  --focus: 2px solid var(--accent);
  font-family: var(--font-stack);
  color: var(--fg);
  background: transparent;
  /* 为独立使用时防止紧贴屏幕边缘，可按需删除或调整 */
  padding: 25px;
}

/* 页头 */
.fb-header { margin-bottom: 28px; }
.fb-h1 {
  margin: 0 0 6px;
  font-size: 26px;
  letter-spacing: .5px;
  font-weight: 600;
}
.fb-sub {
  margin: 0;
  font-size: 14px;
  color: var(--mute);
}

/* 布局 */
.fb-layout {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  gap: 28px;
  align-items: start;
}
@media (max-width: 980px) {
  .fb-layout { grid-template-columns: 1fr; }
}

/* Panel */
.fb-panel {
  background: var(--panel-bg);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 24px 24px 28px;
}
.fb-form, .fb-list { display: flex; flex-direction: column; gap: 18px; }
.fb-h2 {
  margin: 0 0 8px;
  font-size: 18px;
  font-weight: 600;
  letter-spacing: .3px;
}

/* 表单元素 */
.fb-field { display: flex; flex-direction: column; gap: 6px; }
.fb-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--fg-soft);
  display: inline-flex;
  gap: 4px;
}
.fb-label.required::after {
  content:"*";
  color: var(--danger);
  font-weight: 400;
}
.fb-input, .fb-select, .fb-textarea {
  width: 100%;
  font: inherit;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--fg);
  line-height: 1.4;
  transition: border-color .15s, background-color .15s;
}
.fb-input:focus, .fb-select:focus, .fb-textarea:focus {
  outline: var(--focus);
  outline-offset: 0;
  background: var(--accent-bg);
  border-color: var(--accent);
}
.invalid { border-color: var(--danger) !important; }
.fb-textarea { resize: vertical; min-height: 148px; }

.fb-hint-row {
  display: flex; justify-content: space-between; align-items: center;
}
.fb-hint, .fb-count {
  font-size: 12px;
  color: var(--mute);
}
.fb-error {
  font-size: 12px;
  color: var(--danger);
  margin: 0;
}

.fb-grid-2 {
  display: grid;
  gap: 18px;
  grid-template-columns: 1fr 160px;
}
@media (max-width: 640px) {
  .fb-grid-2 { grid-template-columns: 1fr; }
}

.fb-check {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding-left: 20px;
  font-size: 13px;
  color: var(--fg-soft);
  cursor: pointer;
  user-select: none;
}

.fb-check input {
  width: 16px; height: 16px;
  border: 1px solid var(--border);
  border-radius: 4px;
  accent-color: var(--accent);
}

/* 上传区 */
.fb-upload-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.fb-upload-tile {
  width: 96px; height: 72px;
  border: 1px dashed var(--border);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--mute);
  font-size: 12px;
  display: flex; flex-direction: column;
  justify-content: center; align-items: center;
  gap: 4px;
  cursor: pointer;
  position: relative;
  transition: background .15s, border-color .15s;
}
.fb-upload-tile:hover { background: var(--accent-bg); border-color: var(--accent); }
.fb-upload-tile.disabled { opacity:.5; cursor: not-allowed; }
.fb-upload-tile input {
  position: absolute; inset: 0; opacity: 0; cursor: pointer;
}
.fb-upload-plus { font-size: 22px; line-height: 1; }
.fb-thumb {
  width: 96px; height: 72px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  overflow: hidden;
  position: relative;
  background: #fff;
}
.fb-thumb img {
  width: 100%; height: 100%; object-fit: cover; display: block;
}
.fb-thumb-remove {
  position: absolute; top: 3px; right: 3px;
  border: none; background: rgba(0,0,0,.45);
  color: #fff; width: 20px; height: 20px;
  border-radius: 50%; font-size: 14px; cursor: pointer;
  line-height: 1;
}
.fb-thumb-remove:hover { background: rgba(0,0,0,.65); }

/* 按钮 */
.fb-actions { display: flex; gap: 10px; margin-top: 4px; }
.fb-btn {
  font: inherit;
  padding: 10px 18px;
  border: 1px solid var(--border);
  background: #fff;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  color: var(--fg-soft);
  transition: background .15s, border-color .15s, color .15s;
}
.fb-btn:hover:not(:disabled) {
  background: var(--accent-bg);
  border-color: var(--accent);
  color: var(--accent);
}
.fb-btn.primary {
  background: var(--accent);
  color: #fff;
  border-color: var(--accent);
  font-weight: 500;
}
.fb-btn.primary:hover:not(:disabled) {
  background: #1d4ed8;
  border-color: #1d4ed8;
}
.fb-btn:disabled { opacity:.55; cursor: not-allowed; }
.fb-btn.small { padding: 6px 12px; font-size: 13px; }

.fb-spinner {
  width: 16px; height:16px;
  border: 2px solid rgba(255,255,255,.6);
  border-right-color: transparent;
  border-radius: 50%;
  display:inline-block;
  margin-right:6px;
  animation: fb-spin .8s linear infinite;
}
@keyframes fb-spin { to { transform: rotate(360deg); } }

.fb-msg {
  margin: 10px 0 0;
  padding: 10px 14px;
  font-size: 13px;
  border-radius: var(--radius-sm);
  border: 1px solid;
}
.fb-msg.success {
  color: var(--success);
  background: #ecfdf5;
  border-color: #d1fae5;
}
.fb-msg.error {
  color: var(--danger);
  background: #fef2f2;
  border-color: #fecaca;
}

/* 列表 */
.fb-list-head {
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.fb-filters {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}
.fb-tabs {
  display: flex; gap: 6px; flex-wrap: wrap;
}
.fb-tab {
  background: #fff;
  border: 1px solid var(--border);
  padding: 6px 14px;
  font-size: 13px;
  border-radius: 20px;
  cursor: pointer;
  color: var(--fg-soft);
  transition: background .15s, color .15s, border-color .15s;
}
.fb-tab.active {
  background: var(--accent);
  color: #fff;
  border-color: var(--accent);
}
.fb-tab:not(.active):hover {
  background: var(--accent-bg);
  color: var(--accent);
  border-color: var(--accent);
}
.fb-search { width: 200px; }
@media (max-width: 640px) { .fb-search { width: 100%; } }

.fb-empty {
  text-align: center;
  padding: 40px 0;
  color: var(--mute);
  font-size: 14px;
  border-top: 1px solid var(--border);
}

.fb-items {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 18px;
  border-top: 1px solid var(--border);
  padding-top: 6px;
}
.fb-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 4px 0 2px;
}
.fb-item-row1 {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12px;
}
.fb-item-title {
  margin: 0;
  font-size: 15px;
  font-weight: 500;
  line-height: 1.3;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.fb-status {
  font-size: 12px;
  color: var(--mute);
  display: inline-flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}
.fb-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: var(--border-strong);
  display: inline-block;
}
.fb-dot[data-status="未读"] { background: var(--accent); }
.fb-dot[data-status="处理中"] { background: #d97706; }
.fb-dot[data-status="已完成"] { background: var(--success); }

.fb-item-meta {
  font-size: 12px;
  color: var(--mute);
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.sep { color: var(--border-strong); }

.fb-item-content {
  margin: 0;
  font-size: 13px;
  color: var(--fg-soft);
  line-height: 1.55;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.fb-mini-row {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 2px;
}
.fb-mini {
  width: 60px; height: 44px;
  object-fit: cover;
  border: 1px solid var(--border);
  border-radius: 6px;
  cursor: zoom-in;
  background: #fff;
}
.fb-mini:hover { border-color: var(--accent); }

.fb-pager {
  margin-top: 8px;
  display: flex;
  gap: 14px;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
  font-size: 13px;
}

/* 预览 */
.fb-preview {
  position: fixed;
  inset: 0;
  background: rgba(17,24,39,.8);
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 40px 20px;
  z-index: 90;
}
.fb-preview img {
  max-width: 92vw;
  max-height: 80vh;
  border-radius: var(--radius);
  background: #fff;
  box-shadow: 0 0 0 1px var(--border), 0 8px 30px rgba(0,0,0,.35);
}
.fb-close {
  position: fixed;
  top: 18px; right: 18px;
  width: 40px; height: 40px;
  border: none;
  background: rgba(0,0,0,.5);
  color: #fff;
  font-size: 22px;
  border-radius: 50%;
  cursor: pointer;
}
.fb-close:hover { background: rgba(0,0,0,.65); }

/* 可访问性辅助 */
.fb-input:disabled { background: #f3f4f6; color: #9ca3af; }
.fb-upload-tile:focus-visible,
.fb-tab:focus-visible,
.fb-btn:focus-visible,
.fb-thumb-remove:focus-visible {
  outline: var(--focus);
  outline-offset: 2px;
}

/* 动效/微调 */
.fb-tab, .fb-btn, .fb-upload-tile { will-change: background, color, border-color; }
</style>