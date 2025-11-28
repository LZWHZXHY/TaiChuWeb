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
            <label for="type" class="fb-label required">分类</label>
            <select
              id="type"
              v-model="form.type"
              class="fb-select"
              :class="{ invalid: touched.type && !valid.type }"
              @blur="touched.type = true"
            >
              <option disabled value="">请选择分类</option>
              <option v-for="category in categories" :key="category.value" :value="category.value">
                {{ category.label }}
              </option>
            </select>
            <p v-if="touched.type && !valid.type" class="fb-error">请选择分类</p>
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
                  :disabled="form.images.length >= rules.images.maxCount"
                  @change="onFiles"
                />
                <span class="fb-upload-plus">+</span>
                <span class="fb-upload-text">上传</span>
              </label>
              <div v-for="(img, i) in form.images" :key="i" class="fb-thumb">
                
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

          <!-- 联系方式 -->
          <div class="fb-field">
            <label for="ContactQQ" class="fb-label">联系QQ（可选）</label>
            <input
              id="ContactQQ"
              type="text"
              v-model.trim="form.ContactQQ"
              class="fb-input"
              placeholder="请输入QQ号码"
              @blur="touched.ContactQQ = true"
              :class="{ invalid: touched.ContactQQ && !valid.ContactQQ }"
            />
            <p v-if="touched.ContactQQ && !valid.ContactQQ" class="fb-error">
              QQ号码格式不正确（5-15位数字）
            </p>
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
                v-for="status in statuses"
                :key="status.value"
                class="fb-tab"
                :class="{ active: filter.status === status.value }"
                @click="filter.status = status.value; filter.page = 1"
                role="tab"
              >
                {{ status.label }}
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

        <div v-if="loadingList" class="fb-empty">加载中...</div>
        <div v-else-if="pagedItems.length === 0" class="fb-empty">暂无反馈</div>

        <ul v-else class="fb-items">
          <li v-for="item in pagedItems" :key="item.id" class="fb-item">
            <div class="fb-item-row1">
              <h3 class="fb-item-title">{{ item.title }}</h3>
              <span class="fb-status">
                <span class="fb-dot" :data-status="getStatusText(item.status)"></span>
                {{ getStatusText(item.status) }}
              </span>
            </div>
            <div class="fb-item-meta">
              <span>{{ getTypeText(item.type) }}</span>
              <span class="sep">·</span>
              <time>{{ formatDate(item.createTime) }}</time>
              <template v-if="item.imagesURL">
                <span class="sep">·</span>
                <span>有图片</span>
              </template>
            </div>
            <p class="fb-item-content">{{ snippet(item.content) }}</p>
            <div v-if="item.imagesURL" class="fb-mini-row">
              
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
      
      <button class="fb-close" @click="previewUrl = ''" aria-label="关闭预览">×</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed, watch, onMounted } from 'vue'
import apiClient from '../utils/api'

interface FeedbackForm {
  type: number
  title: string
  content: string
  ContactQQ?: string
  images: Array<{ file: File; preview: string }>
}

interface FeedbackItem {
  id: number
  type: number
  title: string
  content: string
  status: number
  createTime: string
  imagesURL?: string
}

interface Category {
  value: number
  label: string
  description: string
}

interface Status {
  value: number
  label: string
}

const rules = {
  title: { min: 2, max: 50 },
  content: { min: 10, max: 1000 },
  images: { maxCount: 1, maxSize: 10 * 1024 * 1024 } // 单张图片限制
}

const categories: Category[] = [
  { value: 1, label: '网站BUG反馈', description: '报告网站功能异常、错误等问题' },
  { value: 2, label: '社区意见', description: '对社区功能、体验的建议' },
  { value: 3, label: '内容举报', description: '举报违规、不良内容' },
  { value: 4, label: '其他', description: '其他类型的反馈' }
]

const statuses: Status[] = [
  { value: 0, label: '待处理' },
  { value: 1, label: '处理中' },
  { value: 2, label: '已解决' },
  { value: 3, label: '已关闭' }
]

const form = reactive<FeedbackForm>({
  type: 0,
  title: '',
  content: '',
  ContactQQ: '',
  images: []
})

const touched = reactive({
  type: false,
  title: false,
  content: false,
  ContactQQ: false
})

const errors = reactive({ images: '' })
const loading = ref(false)
const loadingList = ref(false)
const message = reactive({ text: '', type: '' as 'success' | 'error' | '' })
const previewUrl = ref('')

const items = ref<FeedbackItem[]>([])
const filter = reactive({
  status: -1, // -1 表示全部
  q: '',
  page: 1,
  pageSize: 6
})

// 验证规则
const valid = reactive({
  type: computed(() => form.type > 0 && form.type <= 4),
  title: computed(() => form.title.length >= rules.title.min && form.title.length <= rules.title.max),
  content: computed(() => form.content.length >= rules.content.min && form.content.length <= rules.content.max),
  ContactQQ: computed(() => {
    if (!form.ContactQQ) return true
    return /^[1-9][0-9]{4,14}$/.test(form.ContactQQ)
  })
})

const formValid = computed(() => valid.type && valid.title && valid.content && valid.ContactQQ)

// 过滤和分页
const filtered = computed(() => {
  const q = filter.q.trim().toLowerCase()
  return items.value
    .filter(it => filter.status === -1 ? true : it.status === filter.status)
    .filter(it => q ? (it.title + it.content).toLowerCase().includes(q) : true)
    .sort((a, b) => new Date(b.createTime).getTime() - new Date(a.createTime).getTime())
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / filter.pageSize)))
const pagedItems = computed(() => {
  const start = (filter.page - 1) * filter.pageSize
  return filtered.value.slice(start, start + filter.pageSize)
})

watch([() => filter.status, () => filter.q], () => {
  filter.page = 1
})

// 工具函数
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
  form.type = 0
  form.title = ''
  form.content = ''
  form.ContactQQ = ''
  form.images = []
  Object.keys(touched).forEach(key => (touched as any)[key] = false)
  errors.images = ''
}

function snippet(text: string, max = 140) {
  return text.length > max ? text.slice(0, max) + '…' : text
}

function formatDate(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function preview(url: string) {
  previewUrl.value = url
}

function getTypeText(type: number): string {
  const category = categories.find(c => c.value === type)
  return category ? category.label : '未知类型'
}

function getStatusText(status: number): string {
  const statusObj = statuses.find(s => s.value === status)
  return statusObj ? statusObj.label : '未知状态'
}

function getFullImageUrl(localUrl: string): string {
  if (!localUrl) return ''
  return `/uploads/${localUrl}`
}

// 图片处理
async function dataURLFromFile(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result))
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

async function handleFiles(files: FileList | File[]) {
  errors.images = ''
  const arr = Array.from(files)
  if (!arr.length) return
  
  const available = rules.images.maxCount - form.images.length
  for (const file of arr.slice(0, available)) {
    if (!file.type.startsWith('image/')) {
      errors.images = '仅支持图片文件'
      continue
    }
    if (file.size > rules.images.maxSize) {
      errors.images = `图片大小不能超过${(rules.images.maxSize / 1024 / 1024).toFixed(0)}MB`
      continue
    }
    
    const preview = await dataURLFromFile(file)
    form.images.push({ file, preview })
  }
  
  if (arr.length > available) {
    setMessage(`最多上传${rules.images.maxCount}张图片，多余图片已忽略`, 'error')
  }
}

function onFiles(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files) return
  handleFiles(input.files)
  input.value = ''
}

function removeImage(index: number) {
  form.images.splice(index, 1)
}

async function onPaste(e: ClipboardEvent) {
  const items = e.clipboardData?.items
  if (!items) return
  
  const files: File[] = []
  for (const item of items as any) {
    if (item.kind === 'file') {
      const file = item.getAsFile()
      if (file) files.push(file)
    }
  }
  
  if (files.length) {
    e.preventDefault()
    await handleFiles(files)
  }
}

// API 调用
async function loadFeedbacks() {
  try {
    loadingList.value = true
    console.log('开始加载反馈列表...')
    
    const response = await apiClient.get('/feedback/list')
    console.log('API响应:', response.data)
    
    if (response.data && response.data.success) {
      // 修复：从 response.data.data.items 获取数据
      const apiData = response.data.data
      if (apiData && apiData.items) {
        items.value = apiData.items
        console.log('成功加载数据:', items.value.length, '条')
      } else {
        console.log('数据结构异常:', apiData)
        items.value = []
      }
    } else {
      console.log('API返回失败:', response.data?.message)
      setMessage('加载反馈列表失败: ' + (response.data?.message || '未知错误'), 'error')
      items.value = [] // 确保清空数据
    }
  } catch (error: any) {
    console.error('加载反馈列表失败:', error)
    const errorMessage = error.response?.data?.message || error.message
    setMessage('加载反馈列表失败: ' + errorMessage, 'error')
    items.value = [] // 出错时清空数据
  } finally {
    loadingList.value = false
  }
}

async function onSubmit() {
  // 标记所有字段为已触摸
  Object.keys(touched).forEach(key => (touched as any)[key] = true)
  
  if (!formValid.value) {
    setMessage('请检查表单填写', 'error')
    return
  }
  
  loading.value = true
  
  try {
    const formData = new FormData()
    formData.append('title', form.title)
    formData.append('content', form.content)
    formData.append('type', form.type.toString())
    
    if (form.ContactQQ) {
      formData.append('ContactQQ', form.ContactQQ)
    }
    
    // 添加图片文件（如果有）
    if (form.images.length > 0) {
      formData.append('ErrorImage', form.images[0].file)
    }
    
    const response = await apiClient.post('/FeedBack/create', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    
    if (response.data && response.data.success) {
      setMessage('反馈提交成功！感谢您的反馈', 'success')
      onReset()
      await loadFeedbacks() // 重新加载列表
    } else {
      setMessage(response.data?.message || '提交失败', 'error')
    }
  } catch (error: any) {
    console.error('提交反馈失败:', error)
    const errorMessage = error.response?.data?.message || '提交失败，请重试'
    setMessage(errorMessage, 'error')
  } finally {
    loading.value = false
  }
}

// 初始化
onMounted(() => {
  loadFeedbacks()
})
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