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

        <form @submit.prevent="onSubmit" novalidate @paste="handlePaste">
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
              <option disabled value="0">请选择分类</option>
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
            <div class="fb-upload-area">
              <!-- 上传按钮 -->
              <label v-if="!imagePreview" class="fb-upload-tile">
                <input 
                  type="file" 
                  accept="image/*" 
                  @change="handleImageSelect"
                  class="fb-file-input"
                />
                <span class="fb-upload-plus">+</span>
                <span class="fb-upload-text">点击上传或粘贴图片</span>
                <span class="fb-upload-hint">支持 JPG、PNG、GIF、WebP，最大 5MB</span>
              </label>
              
              <!-- 图片预览 -->
              <div v-else class="fb-image-preview">
                <img :src="imagePreview.previewUrl" :alt="imagePreview.file.name" class="fb-preview-image" />
                <div class="fb-preview-info">
                  <span class="fb-file-name">{{ imagePreview.file.name }}</span>
                  <span class="fb-file-size">{{ (imagePreview.file.size / 1024).toFixed(1) }} KB</span>
                </div>
                <button type="button" class="fb-remove-btn" @click="removeImage" title="移除图片">
                  ×
                </button>
              </div>
            </div>
            <p v-if="imageError" class="fb-error">{{ imageError }}</p>
            <p v-else class="fb-hint">支持粘贴截图，单张图片最大 5MB</p>
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
              提交反馈
            </button>
            <button type="button" class="fb-btn" @click="onReset" :disabled="loading">清空</button>
          </div>

          <!-- 消息 -->
          <p v-if="message.text" :class="['fb-msg', message.type]">{{ message.text }}</p>
        </form>
      </section>

      <!-- 已提交意见列表 -->
      <section class="fb-panel fb-list">
        <div class="fb-list-header">
          <h2 class="fb-h2">已提交的意见</h2>
          <div class="fb-list-controls">
            <div class="fb-tabs">
              <button 
                v-for="tab in tabs" 
                :key="tab.value"
                class="fb-tab"
                :class="{ active: activeTab === tab.value }"
                @click="changeTab(tab.value)"
              >
                {{ tab.label }}
              </button>
            </div>
            <button class="fb-btn small" @click="loadFeedbacks" :disabled="loadingList">
              <span v-if="loadingList" class="fb-spinner" aria-hidden="true"></span>
              刷新
            </button>
          </div>
        </div>

        <!-- 加载状态 -->
        <div v-if="loadingList" class="fb-loading">
          <div class="fb-spinner large"></div>
          <span>加载中...</span>
        </div>

        <!-- 空状态 -->
        <div v-else-if="feedbacks.length === 0" class="fb-empty">
          <div class="fb-empty-icon">📝</div>
          <p class="fb-empty-text">暂无提交的意见</p>
          <p class="fb-empty-hint">提交第一条反馈后，将在这里显示</p>
        </div>

        <!-- 意见列表 -->
        <div v-else class="fb-feedbacks">
          <div 
            v-for="feedback in feedbacks" 
            :key="feedback.id" 
            class="fb-feedback-item"
            :class="getStatusClass(feedback.status)"
          >
            <div class="fb-feedback-header">
              <h3 class="fb-feedback-title">{{ feedback.title }}</h3>
              <span class="fb-feedback-badge" :class="getTypeClass(feedback.type)">
                {{ getTypeLabel(feedback.type) }}
              </span>
            </div>
            
            <div class="fb-feedback-meta">
              <span class="fb-feedback-time">{{ formatDate(feedback.createTime) }}</span>
              <span class="fb-feedback-status" :class="getStatusClass(feedback.status)">
                {{ getStatusLabel(feedback.status) }}
              </span>
            </div>

            <p class="fb-feedback-content">{{ truncateContent(feedback.content) }}</p>

            <div v-if="feedback.imageFullUrl" class="fb-feedback-image">
              <img 
                :src="feedback.imageFullUrl" 
                :alt="feedback.title" 
                class="fb-feedback-thumb"
                @click="previewImage(feedback.imageFullUrl)"
              />
              <span class="fb-image-hint">点击查看图片</span>
            </div>

            <div class="fb-feedback-footer">
              <span class="fb-feedback-id">ID: {{ feedback.id }}</span>
              <button 
                v-if="feedback.contactQQ" 
                class="fb-qq-btn"
                @click="copyQQ(feedback.contactQQ)"
                title="复制QQ号"
              >
                QQ: {{ feedback.contactQQ }}
              </button>
            </div>
          </div>
        </div>

        <!-- 分页 -->
        <div v-if="totalPages > 1" class="fb-pagination">
          <button 
            class="fb-btn small" 
            :disabled="currentPage === 1" 
            @click="changePage(currentPage - 1)"
          >
            上一页
          </button>
          <span class="fb-page-info">第 {{ currentPage }} 页 / 共 {{ totalPages }} 页</span>
          <button 
            class="fb-btn small" 
            :disabled="currentPage === totalPages" 
            @click="changePage(currentPage + 1)"
          >
            下一页
          </button>
        </div>
      </section>
    </main>

    <!-- 图片预览模态框 -->
    <div v-if="previewImageUrl" class="fb-image-modal" @click="previewImageUrl = null">
      <div class="fb-modal-content" @click.stop>
        <img :src="previewImageUrl" alt="预览图片" class="fb-modal-image" />
        <button class="fb-modal-close" @click="previewImageUrl = null" title="关闭">×</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed, onMounted } from 'vue'
import apiClient from '../utils/api'

interface FeedbackForm {
  type: number
  title: string
  content: string
  ContactQQ?: string
  ErrorImage?: File | null
}

interface ImagePreview {
  file: File
  previewUrl: string
}

interface FeedbackItem {
  id: number
  title: string
  content: string
  type: number
  status: number
  createTime: string
  contactQQ?: number
  imagesUrl?: string
  imageFullUrl?: string
}

const rules = {
  title: { min: 2, max: 50 },
  content: { min: 10, max: 1000 },
  images: { maxCount: 1, maxSize: 5 * 1024 * 1024 }
}

const categories = [
  { value: 1, label: '网站BUG反馈', description: '报告网站功能异常、错误等问题' },
  { value: 2, label: '社区意见', description: '对社区功能、体验的建议' },
  { value: 3, label: '内容举报', description: '举报违规、不良内容' },
  { value: 4, label: '其他', description: '其他类型的反馈' }
]

const tabs = [
  { value: -1, label: '全部' },
  { value: 0, label: '待处理' },
  { value: 1, label: '处理中' },
  { value: 2, label: '已解决' }
]

const statusLabels = {
  0: '待处理',
  1: '处理中', 
  2: '已解决',
  3: '已关闭'
}

const form = reactive<FeedbackForm>({
  type: 0,
  title: '',
  content: '',
  ContactQQ: '',
  ErrorImage: null
})

const feedbacks = ref<FeedbackItem[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const activeTab = ref(-1)
const loadingList = ref(false)
const previewImageUrl = ref('')

const imagePreview = ref<ImagePreview | null>(null)
const imageError = ref('')

const touched = reactive({
  type: false,
  title: false,
  content: false,
  ContactQQ: false
})

const loading = ref(false)
const message = reactive({ text: '', type: '' as 'success' | 'error' | '' })

// 验证规则
const valid = reactive({
  type: computed(() => form.type > 0 && form.type <= 4),
  title: computed(() => form.title.length >= rules.title.min && form.title.length <= rules.title.max),
  content: computed(() => form.content.length >= rules.content.min && form.content.length <= rules.content.max),
  ContactQQ: computed(() => {
    if (!form.ContactQQ) return true
    return /^[1-9][0-9]{4,14}$/.test(form.ContactQQ)
  }),
  image: computed(() => {
    if (!form.ErrorImage) return true
    return form.ErrorImage.size <= rules.images.maxSize
  })
})

const formValid = computed(() => valid.type && valid.title && valid.content && valid.ContactQQ && valid.image)

// 工具函数
function getTypeLabel(type: number): string {
  const category = categories.find(c => c.value === type)
  return category ? category.label : '未知类型'
}

function getStatusLabel(status: number): string {
  return statusLabels[status as keyof typeof statusLabels] || '未知状态'
}

function getTypeClass(type: number): string {
  return `type-${type}`
}

function getStatusClass(status: number): string {
  return `status-${status}`
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function truncateContent(content: string, maxLength: number = 100): string {
  if (content.length <= maxLength) return content
  return content.substring(0, maxLength) + '...'
}

function changeTab(tab: number) {
  activeTab.value = tab
  currentPage.value = 1
  loadFeedbacks()
}

function changePage(page: number) {
  currentPage.value = page
  loadFeedbacks()
}

function previewImage(url: string) {
  previewImageUrl.value = url
}

async function copyQQ(qq: number) {
  try {
    await navigator.clipboard.writeText(qq.toString())
    setMessage('QQ号已复制到剪贴板', 'success', 1500)
  } catch (err) {
    console.error('复制失败:', err)
  }
}

// 加载反馈列表
async function loadFeedbacks() {
  loadingList.value = true
  try {
    const params = new URLSearchParams({
      page: currentPage.value.toString(),
      pageSize: pageSize.value.toString()
    })
    
    if (activeTab.value !== -1) {
      params.append('status', activeTab.value.toString())
    }

    const response = await apiClient.get(`/FeedBack/list?${params}`)
    
    if (response.data && response.data.success) {
      feedbacks.value = response.data.data.items || []
      totalPages.value = response.data.data.totalPages || 1
    } else {
      setMessage('加载反馈列表失败', 'error')
      feedbacks.value = []
    }
  } catch (error: any) {
    console.error('加载反馈列表失败:', error)
    setMessage('加载失败，请重试', 'error')
    feedbacks.value = []
  } finally {
    loadingList.value = false
  }
}

// 表单相关函数
function onReset() {
  form.type = 0
  form.title = ''
  form.content = ''
  form.ContactQQ = ''
  form.ErrorImage = null
  imagePreview.value = null
  imageError.value = ''
  Object.keys(touched).forEach(key => (touched as any)[key] = false)
  message.text = ''
}

function setMessage(text: string, type: 'success' | 'error', timeout = 2500) {
  message.text = text
  message.type = type
  if (timeout) {
    setTimeout(() => {
      if (message.text === text) message.text = ''
    }, timeout)
  }
}

function handleImageSelect(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return
  
  const file = input.files[0]
  imageError.value = ''
  
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    imageError.value = '只支持 JPG、PNG、GIF、WebP 格式的图片'
    input.value = ''
    return
  }
  
  if (file.size > rules.images.maxSize) {
    imageError.value = `图片大小不能超过 ${(rules.images.maxSize / 1024 / 1024).toFixed(0)}MB`
    input.value = ''
    return
  }
  
  form.ErrorImage = file
  
  const reader = new FileReader()
  reader.onload = (e) => {
    imagePreview.value = {
      file: file,
      previewUrl: e.target?.result as string
    }
  }
  reader.readAsDataURL(file)
  
  input.value = ''
}

function removeImage() {
  form.ErrorImage = null
  imagePreview.value = null
  imageError.value = ''
}

function handlePaste(event: ClipboardEvent) {
  const items = event.clipboardData?.items
  if (!items) return
  
  const itemsArray = Array.from(items)
  
  for (const item of itemsArray) {
    if (item.kind === 'file' && item.type.startsWith('image/')) {
      const file = item.getAsFile()
      if (file) {
        const dataTransfer = new DataTransfer()
        dataTransfer.items.add(file)
        const input = document.createElement('input')
        input.type = 'file'
        input.files = dataTransfer.files
        
        const changeEvent = new Event('change', { bubbles: true })
        Object.defineProperty(changeEvent, 'target', { value: input })
        
        handleImageSelect(changeEvent as unknown as Event)
        break
      }
    }
  }
}

async function onSubmit() {
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
    
    if (form.ErrorImage) {
      formData.append('ErrorImage', form.ErrorImage)
    }
    
    const response = await apiClient.post('/FeedBack/create', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    
    if (response.data && response.data.success) {
      setMessage(response.data.message || '提交成功', 'success')
      onReset()
      // 提交成功后重新加载列表
      await loadFeedbacks()
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

// 初始化加载
onMounted(() => {
  loadFeedbacks()
})
</script>

<style scoped>
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
  --warning: #d97706;
  --info: #0369a1;
  --radius-sm: 4px;
  --radius: 8px;
  --radius-lg: 12px;
  --focus: 2px solid var(--accent);
  --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
  --shadow: 0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1);
  --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
  font-family: var(--font-stack);
  color: var(--fg);
  background: transparent;
  padding: 25px;
  min-height: 100vh;
}

/* 头部样式 */
.fb-header { 
  margin-bottom: 28px; 
  text-align: center;
}
.fb-h1 {
  margin: 0 0 6px;
  font-size: 26px;
  letter-spacing: .5px;
  font-weight: 600;
  color: var(--fg);
  line-height: 1.2;
}
.fb-sub {
  margin: 0;
  font-size: 14px;
  color: var(--mute);
  line-height: 1.5;
}

/* 布局样式 */
.fb-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: 28px;
  align-items: start;
  max-width: 1200px;
  margin: 0 auto;
}

/* 面板样式 */
.fb-panel {
  background: var(--panel-bg);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 24px 24px 28px;
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.3s ease;
}
.fb-panel:hover {
  box-shadow: var(--shadow);
}

/* 表单样式 */
.fb-form { 
  display: flex; 
  flex-direction: column; 
  gap: 18px; 
}
.fb-h2 {
  margin: 0 0 8px;
  font-size: 18px;
  font-weight: 600;
  letter-spacing: .3px;
  color: var(--fg);
  line-height: 1.3;
}

/* 表单字段样式 */
.fb-field { 
  display: flex; 
  flex-direction: column; 
  gap: 6px; 
}
.fb-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--fg-soft);
  display: inline-flex;
  gap: 4px;
  line-height: 1.4;
}
.fb-label.required::after {
  content:"*";
  color: var(--danger);
  font-weight: 400;
}

/* 输入框样式 */
.fb-input, .fb-select, .fb-textarea {
  width: 100%;
  font: inherit;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: #fff;
  color: var(--fg);
  line-height: 1.4;
  transition: all 0.3s ease;
  font-size: 14px;
}
.fb-input:focus, .fb-select:focus, .fb-textarea:focus {
  outline: var(--focus);
  outline-offset: 0;
  background: var(--accent-bg);
  border-color: var(--accent);
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}
.invalid { 
  border-color: var(--danger) !important; 
  background: #fef2f2 !important;
}
.fb-textarea { 
  resize: vertical; 
  min-height: 148px; 
  line-height: 1.6;
}

/* 提示信息样式 */
.fb-hint-row {
  display: flex; 
  justify-content: space-between; 
  align-items: center;
}
.fb-hint, .fb-count {
  font-size: 12px;
  color: var(--mute);
  line-height: 1.4;
}
.fb-error {
  font-size: 12px;
  color: var(--danger);
  margin: 0;
  line-height: 1.4;
}

/* 图片上传区域样式 */
.fb-upload-area {
  border: 2px dashed var(--border);
  border-radius: var(--radius-sm);
  padding: 20px;
  background: var(--accent-bg);
  transition: all 0.3s ease;
  cursor: pointer;
  position: relative;
}
.fb-upload-area:hover {
  border-color: var(--accent);
  background: #dbeafe;
}

.fb-upload-tile {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  padding: 20px;
  text-align: center;
  color: var(--mute);
  transition: all 0.3s ease;
}
.fb-upload-tile:hover {
  color: var(--accent);
  transform: scale(1.02);
}

.fb-file-input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.fb-upload-plus {
  font-size: 32px;
  line-height: 1;
  font-weight: 300;
  transition: transform 0.3s ease;
}
.fb-upload-tile:hover .fb-upload-plus {
  transform: scale(1.1);
}
.fb-upload-text {
  font-size: 14px;
  font-weight: 500;
}
.fb-upload-hint {
  font-size: 12px;
  opacity: 0.7;
}

/* 图片预览样式 */
.fb-image-preview {
  position: relative;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: white;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  transition: all 0.3s ease;
}
.fb-image-preview:hover {
  border-color: var(--accent);
  box-shadow: var(--shadow-sm);
}
.fb-preview-image {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
  border: 1px solid var(--border);
  transition: transform 0.3s ease;
}
.fb-preview-image:hover {
  transform: scale(1.05);
}
.fb-preview-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.fb-file-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--fg);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.fb-file-size {
  font-size: 12px;
  color: var(--mute);
}
.fb-remove-btn {
  width: 24px;
  height: 24px;
  border: none;
  background: var(--danger);
  color: white;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  line-height: 1;
  transition: all 0.3s ease;
  position: absolute;
  top: -8px;
  right: -8px;
}
.fb-remove-btn:hover {
  background: #b91c1c;
  transform: scale(1.1);
}

/* 按钮样式 */
.fb-actions { 
  display: flex; 
  gap: 10px; 
  margin-top: 4px; 
}
.fb-btn {
  font: inherit;
  padding: 10px 18px;
  border: 1px solid var(--border);
  background: #fff;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  color: var(--fg-soft);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}
.fb-btn:hover:not(:disabled) {
  background: var(--accent-bg);
  border-color: var(--accent);
  color: var(--accent);
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
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
  transform: translateY(-1px);
  box-shadow: var(--shadow);
}
.fb-btn:disabled { 
  opacity:.55; 
  cursor: not-allowed; 
  transform: none !important;
}
.fb-btn.small {
  padding: 6px 12px;
  font-size: 13px;
}

/* 加载动画 */
.fb-spinner {
  width: 16px; 
  height:16px;
  border: 2px solid rgba(255,255,255,.6);
  border-right-color: transparent;
  border-radius: 50%;
  display:inline-block;
  margin-right:6px;
  animation: fb-spin .8s linear infinite;
}
.fb-spinner.large {
  width: 24px;
  height: 24px;
  border-width: 3px;
  margin-right: 0;
  margin-bottom: 8px;
}
@keyframes fb-spin { 
  to { 
    transform: rotate(360deg); 
  } 
}

/* 消息提示样式 */
.fb-msg {
  margin: 10px 0 0;
  padding: 10px 14px;
  font-size: 13px;
  border-radius: var(--radius-sm);
  border: 1px solid;
  line-height: 1.4;
  animation: slideDown 0.3s ease;
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
@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 列表区域样式 */
.fb-list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 15px;
}
.fb-list-controls {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-wrap: wrap;
}

/* 标签页样式 */
.fb-tabs {
  display: flex;
  background: var(--accent-bg);
  border-radius: var(--radius-sm);
  padding: 4px;
  gap: 4px;
}
.fb-tab {
  padding: 6px 16px;
  border: none;
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  color: var(--mute);
  transition: all 0.3s ease;
  white-space: nowrap;
}
.fb-tab:hover {
  color: var(--accent);
}
.fb-tab.active {
  background: white;
  color: var(--accent);
  box-shadow: var(--shadow-sm);
  font-weight: 500;
}

/* 加载状态 */
.fb-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: var(--mute);
  gap: 10px;
  text-align: center;
}

/* 空状态 */
.fb-empty {
  text-align: center;
  padding: 40px 20px;
  color: var(--mute);
}
.fb-empty-icon {
  font-size: 48px;
  margin-bottom: 10px;
  opacity: 0.5;
}
.fb-empty-text {
  font-size: 16px;
  font-weight: 500;
  margin-bottom: 5px;
  color: var(--fg-soft);
}
.fb-empty-hint {
  font-size: 14px;
  opacity: 0.7;
}

/* 反馈列表样式 */
.fb-feedbacks {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.fb-feedback-item {
  background: white;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 20px;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}
.fb-feedback-item::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: var(--border);
  transition: background-color 0.3s ease;
}
.fb-feedback-item:hover {
  border-color: var(--accent);
  box-shadow: var(--shadow);
  transform: translateY(-2px);
}
.fb-feedback-item:hover::before {
  background: var(--accent);
}

.fb-feedback-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 15px;
  margin-bottom: 10px;
}
.fb-feedback-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--fg);
  flex: 1;
  line-height: 1.4;
}
.fb-feedback-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  white-space: nowrap;
}
.fb-feedback-badge.type-1 { background: #fef2f2; color: #dc2626; }
.fb-feedback-badge.type-2 { background: #f0f9ff; color: #0369a1; }
.fb-feedback-badge.type-3 { background: #fefce8; color: #ca8a04; }
.fb-feedback-badge.type-4 { background: #f3f4f6; color: #374151; }

.fb-feedback-meta {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 12px;
  font-size: 13px;
  color: var(--mute);
  flex-wrap: wrap;
}
.fb-feedback-time {
  display: flex;
  align-items: center;
  gap: 4px;
}
.fb-feedback-time::before {
  content: '🕒';
  font-size: 12px;
}
.fb-feedback-status {
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 500;
}
.fb-feedback-status.status-0 { background: #fef3c7; color: #d97706; }
.fb-feedback-status.status-1 { background: #dbeafe; color: #1d4ed8; }
.fb-feedback-status.status-2 { background: #dcfce7; color: #16a34a; }
.fb-feedback-status.status-3 { background: #f3f4f6; color: #6b7280; }

.fb-feedback-content {
  margin: 0 0 15px 0;
  font-size: 14px;
  line-height: 1.6;
  color: var(--fg-soft);
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.fb-feedback-image {
  margin-bottom: 15px;
}
.fb-feedback-thumb {
  width: 80px;
  height: 60px;
  object-fit: cover;
  border-radius: 6px;
  border: 1px solid var(--border);
  cursor: pointer;
  transition: all 0.3s ease;
}
.fb-feedback-thumb:hover {
  transform: scale(1.05);
  box-shadow: var(--shadow);
}
.fb-image-hint {
  display: block;
  font-size: 12px;
  color: var(--mute);
  margin-top: 4px;
}

.fb-feedback-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: var(--mute);
  padding-top: 10px;
  border-top: 1px solid var(--border);
}
.fb-feedback-id {
  opacity: 0.7;
}
.fb-qq-btn {
  background: #ffe6e6;
  border: 1px solid #ffcccc;
  border-radius: 4px;
  padding: 2px 8px;
  font-size: 11px;
  color: #d63384;
  cursor: pointer;
  transition: all 0.3s ease;
}
.fb-qq-btn:hover {
  background: #ffcccc;
  transform: translateY(-1px);
}

/* 分页样式 */
.fb-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid var(--border);
}
.fb-page-info {
  font-size: 14px;
  color: var(--mute);
  font-weight: 500;
}

/* 图片预览模态框 */
.fb-image-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  animation: fadeIn 0.3s ease;
}
.fb-modal-content {
  position: relative;
  max-width: 90vw;
  max-height: 90vh;
  animation: scaleIn 0.3s ease;
}
.fb-modal-image {
  max-width: 100%;
  max-height: 100%;
  border-radius: 8px;
  box-shadow: var(--shadow-md);
}
.fb-modal-close {
  position: absolute;
  top: -40px;
  right: 0;
  background: rgba(255,255,255,0.9);
  border: none;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  font-size: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #333;
  transition: all 0.3s ease;
}
.fb-modal-close:hover {
  background: white;
  transform: scale(1.1);
}

/* 动画效果 */
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

/* 响应式设计 */
@media (min-width: 1024px) {
  .fb-layout {
    grid-template-columns: 1fr 1fr;
    gap: 32px;
  }
}

@media (max-width: 768px) {
  .fb-root {
    padding: 15px;
  }
  
  .fb-layout {
    gap: 20px;
  }
  
  .fb-panel {
    padding: 20px;
  }
  
  .fb-list-header {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }
  
  .fb-list-controls {
    justify-content: space-between;
  }
  
  .fb-tabs {
    flex-wrap: wrap;
    justify-content: center;
  }
  
  .fb-feedback-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
  
  .fb-feedback-meta {
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
  }
  
  .fb-feedback-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
  }
  
  .fb-pagination {
    flex-direction: column;
    gap: 10px;
  }
  
  .fb-actions {
    flex-direction: column;
  }
  
  .fb-image-preview {
    flex-direction: column;
    text-align: center;
    gap: 8px;
  }
  
  .fb-preview-info {
    align-items: center;
  }
}

@media (max-width: 480px) {
  .fb-root {
    padding: 10px;
  }
  
  .fb-h1 {
    font-size: 22px;
  }
  
  .fb-h2 {
    font-size: 16px;
  }
  
  .fb-panel {
    padding: 16px;
  }
  
  .fb-feedback-item {
    padding: 16px;
  }
}

/* 打印样式 */
@media print {
  .fb-root {
    padding: 0;
  }
  
  .fb-panel {
    border: none;
    box-shadow: none;
    page-break-inside: avoid;
  }
  
  .fb-actions,
  .fb-list-controls,
  .fb-remove-btn,
  .fb-modal-close {
    display: none;
  }
}
</style>