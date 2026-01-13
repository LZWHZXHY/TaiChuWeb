<template>
  <div class="cyber-feedback-root">
    <div class="grid-bg moving-grid"></div>

    <div class="panel-scroll-container custom-scroll">
      
      <header class="main-header">
        <div class="header-split left">
          <h1 class="giant-text glitch-hover">
            <div class="text-row">USER</div>
            <div class="text-row outline">FEEDBACK</div>
            <div class="text-row red-fill">意见箱</div>
          </h1>
        </div>
        <div class="header-split right">
          <div class="info-block">
            <h2 class="cn-title">反馈传输协议 // UPLOAD_PROTOCOL</h2>
            <div class="live-indicator"><span class="dot"></span> SYSTEM LISTENING</div>
            <p class="desc">
              你的声音将被编码并传输至核心数据库。<br>
              <span class="highlight">BUG_REPORT</span> / <span class="highlight red">SUGGESTION</span>
            </p>
          </div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // AWAITING_INPUT // FEEDBACK_LOOP_OPEN // WE_VALUE_YOUR_DATA // TRANSMIT_NOW // 
          // AWAITING_INPUT // FEEDBACK_LOOP_OPEN // WE_VALUE_YOUR_DATA // TRANSMIT_NOW //
        </div>
      </div>

      <main class="fb-layout">
        
        <section class="cyber-card fb-form-panel">
          <div class="card-label-strip">
            <span>// INPUT_TERMINAL</span>
          </div>
          
          <div class="card-inner-pad">
            <form @submit.prevent="onSubmit" novalidate @paste="handlePaste">
              
              <div class="fb-field">
                <label for="type" class="cyber-label required">> SELECT_CATEGORY / 分类</label>
                <div class="cyber-select-wrapper">
                  <select
                    id="type"
                    v-model="form.type"
                    class="cyber-select"
                    :class="{ invalid: touched.type && !valid.type }"
                    @blur="touched.type = true"
                  >
                    <option disabled value="0">-- CHOOSE_OPTION --</option>
                    <option v-for="category in categories" :key="category.value" :value="category.value">
                      [ {{ category.label }} ]
                    </option>
                  </select>
                  <div class="select-arrow">▼</div>
                </div>
                <div v-if="touched.type && !valid.type" class="error-msg">>> ERROR: CATEGORY_REQUIRED</div>
              </div>

              <div class="fb-field">
                <label for="title" class="cyber-label required">> HEADLINE / 标题</label>
                <input
                  id="title"
                  type="text"
                  v-model.trim="form.title"
                  :maxlength="rules.title.max"
                  class="cyber-input"
                  placeholder="INPUT_SUMMARY..."
                  @blur="touched.title = true"
                  :class="{ invalid: touched.title && !valid.title }"
                />
                <div class="fb-hint-row">
                  <span class="fb-hint">REQ: {{ rules.title.min }}-{{ rules.title.max }} CHARS</span>
                  <span class="fb-count">{{ form.title.length }}/{{ rules.title.max }}</span>
                </div>
              </div>

              <div class="fb-field">
                <label for="content" class="cyber-label required">> DETAILS / 详细描述</label>
                <textarea
                  id="content"
                  v-model.trim="form.content"
                  :maxlength="rules.content.max"
                  rows="6"
                  class="cyber-textarea custom-scroll"
                  placeholder="DESCRIBE_ISSUE_OR_SUGGESTION..."
                  @blur="touched.content = true"
                  :class="{ invalid: touched.content && !valid.content }"
                ></textarea>
                <div class="fb-hint-row">
                  <span class="fb-hint">MIN: {{ rules.content.min }} CHARS</span>
                  <span class="fb-count">{{ form.content.length }}/{{ rules.content.max }}</span>
                </div>
              </div>

              <div class="fb-field">
                <label class="cyber-label">> ATTACHMENT / 附件 (OPTIONAL)</label>
                <div class="cyber-upload-zone" :class="{ 'has-file': imagePreview }">
                  
                  <label v-if="!imagePreview" class="upload-trigger">
                    <input 
                      type="file" 
                      accept="image/*" 
                      @change="handleImageSelect"
                      class="hidden-input"
                    />
                    <div class="upload-icon">[ + ]</div>
                    <div class="upload-text">CLICK_OR_PASTE_IMAGE</div>
                    <div class="upload-sub">SUPPORT: JPG/PNG/GIF | MAX: 5MB</div>
                  </label>
                  
                  <div v-else class="upload-preview">
                    <div class="preview-frame">
                      <img :src="imagePreview.previewUrl" :alt="imagePreview.file.name" />
                    </div>
                    <div class="preview-info">
                      <span class="file-name">{{ imagePreview.file.name }}</span>
                      <span class="file-size">{{ (imagePreview.file.size / 1024).toFixed(1) }} KB</span>
                    </div>
                    <button type="button" class="remove-btn" @click="removeImage">REMOVE [X]</button>
                  </div>
                </div>
                <div v-if="imageError" class="error-msg">>> ERROR: {{ imageError }}</div>
              </div>

              <div class="fb-field">
                <label for="ContactQQ" class="cyber-label">> CONTACT_QQ / 联系方式 (OPTIONAL)</label>
                <input
                  id="ContactQQ"
                  type="text"
                  v-model.trim="form.ContactQQ"
                  class="cyber-input"
                  placeholder="DIGITAL_ID..."
                  @blur="touched.ContactQQ = true"
                  :class="{ invalid: touched.ContactQQ && !valid.ContactQQ }"
                />
                <div v-if="touched.ContactQQ && !valid.ContactQQ" class="error-msg">>> ERROR: INVALID_FORMAT</div>
              </div>

              <div class="fb-actions">
                <button type="submit" class="cyber-btn primary" :disabled="!formValid || loading">
                  <span v-if="loading" class="btn-spinner">[/]</span>
                  <span class="btn-text">{{ loading ? 'TRANSMITTING...' : 'INITIATE_UPLOAD / 提交' }}</span>
                </button>
                <button type="button" class="cyber-btn secondary" @click="onReset" :disabled="loading">
                  RESET / 重置
                </button>
              </div>

              <div v-if="message.text" :class="['status-readout', message.type]">
                <span class="blink">></span> {{ message.text }}
              </div>
            </form>
          </div>
        </section>

        <section class="cyber-card fb-list-panel">
          <div class="card-label-strip">
            <span>// ARCHIVE_LOGS</span>
          </div>
          
          <div class="list-header-row">
            <h2 class="list-title">已提交反馈 / SUBMISSIONS</h2>
            <div class="list-controls">
              <div class="cyber-tabs">
                <button 
                  v-for="tab in tabs" 
                  :key="tab.value"
                  class="cyber-tab"
                  :class="{ active: activeTab === tab.value }"
                  @click="changeTab(tab.value)"
                >
                  {{ tab.label }}
                </button>
              </div>
              <button class="cyber-mini-btn" @click="loadFeedbacks" :disabled="loadingList">
                <span v-if="loadingList" class="btn-spinner-sm">~</span> REFRESH
              </button>
            </div>
          </div>

          <div class="list-content-area custom-scroll">
            <div v-if="loadingList" class="loading-state">
              <div class="cyber-spinner"></div>
              <span>SYNCING_DATABASE...</span>
            </div>

            <div v-else-if="feedbacks.length === 0" class="empty-state">
              <div class="empty-icon">NULL</div>
              <p>NO DATA FOUND IN SECTOR.</p>
            </div>

            <div v-else class="feedback-feed">
              <div 
                v-for="feedback in feedbacks" 
                :key="feedback.id" 
                class="feedback-entry"
                :class="`status-${feedback.status}`"
              >
                <div class="entry-scanline"></div>

                <div class="entry-header">
                  <div class="title-row">
                    <span class="entry-id">#{{ feedback.id }}</span>
                    <h3 class="entry-title">{{ feedback.title }}</h3>
                  </div>
                  <div class="badge-row">
                    <span class="cyber-badge type">{{ getTypeLabel(feedback.type) }}</span>
                    <span class="cyber-badge status" :class="`st-${feedback.status}`">
                      {{ getStatusLabel(feedback.status) }}
                    </span>
                  </div>
                </div>

                <div class="entry-meta">
                  <span class="meta-item">TIME: {{ formatDate(feedback.createTime) }}</span>
                  <button 
                    v-if="feedback.contactQQ" 
                    class="meta-action"
                    @click="copyQQ(feedback.contactQQ)"
                  >
                    QQ: {{ feedback.contactQQ }} [COPY]
                  </button>
                </div>

                <div class="entry-content">
                  {{ truncateContent(feedback.content) }}
                </div>

                <div v-if="feedback.imageFullUrl" class="entry-attachment">
                  <div class="att-label">> IMAGE_DETECTED:</div>
                  <img 
                    :src="feedback.imageFullUrl" 
                    :alt="feedback.title" 
                    class="att-thumb"
                    @click="previewImage(feedback.imageFullUrl)"
                  />
                </div>

                <div v-if="feedback.adminReply" class="admin-reply-terminal">
                  <div class="reply-header">
                    <span class="icon">★</span> SYS_ADMIN_RESPONSE:
                  </div>
                  <div class="reply-body">
                    {{ feedback.adminReply }}
                  </div>
                </div>

              </div>
            </div>
            
            <div v-if="totalPages > 1" class="cyber-pagination">
              <button 
                class="page-btn" 
                :disabled="currentPage === 1" 
                @click="changePage(currentPage - 1)"
              >
                &lt; PREV
              </button>
              <span class="page-info">PAGE {{ currentPage }} / {{ totalPages }}</span>
              <button 
                class="page-btn" 
                :disabled="currentPage === totalPages" 
                @click="changePage(currentPage + 1)"
              >
                NEXT &gt;
              </button>
            </div>

          </div>
        </section>
      </main>

      <div v-if="previewImageUrl" class="cyber-modal-overlay" @click="previewImageUrl = null">
        <div class="cyber-modal-window" @click.stop>
          <div class="modal-header">
            <span>> IMAGE_VIEWER.exe</span>
            <button class="modal-close" @click="previewImageUrl = null">[ X ]</button>
          </div>
          <div class="modal-body">
             <img :src="previewImageUrl" alt="预览图片" />
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
// --- 逻辑部分保持完全一致，仅 TypeScript 接口和逻辑复用 ---
import { reactive, ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

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
  adminReply?: string
}

const rules = {
  title: { min: 2, max: 50 },
  content: { min: 10, max: 1000 },
  images: { maxCount: 1, maxSize: 5 * 1024 * 1024 }
}

const categories = [
  { value: 1, label: 'BUG / 漏洞' },
  { value: 2, label: 'OPINION / 建议' },
  { value: 3, label: 'REPORT / 举报' },
  { value: 4, label: 'OTHER / 其他' }
]

const tabs = [
  { value: -1, label: 'ALL' },
  { value: 0, label: 'PENDING' },
  { value: 1, label: 'PROCESS' },
  { value: 2, label: 'SOLVED' }
]

const statusLabels = {
  0: 'PENDING',
  1: 'PROCESSING', 
  2: 'SOLVED',
  3: 'CLOSED'
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
  // 简化显示，去掉英文前缀用于列表
  return category ? category.label.split(' / ')[0] : 'UNKNOWN'
}

function getStatusLabel(status: number): string {
  return statusLabels[status as keyof typeof statusLabels] || 'UNKNOWN'
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

function truncateContent(content: string, maxLength: number = 80): string {
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
    setMessage('QQ COPIED TO CLIPBOARD', 'success', 1500)
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
      setMessage('LOAD FAILED', 'error')
      feedbacks.value = []
    }
  } catch (error: any) {
    console.error('加载反馈列表失败:', error)
    setMessage('NETWORK ERROR', 'error')
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
    imageError.value = 'INVALID FORMAT. USE JPG/PNG/WEBP.'
    input.value = ''
    return
  }
  
  if (file.size > rules.images.maxSize) {
    imageError.value = `FILE TOO LARGE. MAX ${(rules.images.maxSize / 1024 / 1024).toFixed(0)}MB.`
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
    setMessage('FORM INVALID. CHECK INPUTS.', 'error')
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
      setMessage(response.data.message || 'UPLOAD COMPLETE', 'success')
      onReset()
      await loadFeedbacks()
    } else {
      setMessage(response.data?.message || 'UPLOAD FAILED', 'error')
    }
  } catch (error: any) {
    console.error('提交反馈失败:', error)
    const errorMessage = error.response?.data?.message || 'SYSTEM ERROR. RETRY.'
    setMessage(errorMessage, 'error')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadFeedbacks()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.cyber-feedback-root {
  --red: #D92323; 
  --black: #111111; 
  --off-white: #F4F1EA; 
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  --body: 'Inter', sans-serif;
  --shadow-hard: 4px 4px 0 rgba(0,0,0,0.15);
  --border-thick: 2px solid var(--black);
  
  width: 100%; height: 100vh; 
  background-color: var(--off-white); 
  display: flex; flex-direction: column; overflow: hidden; 
  font-family: var(--body);
  color: var(--black);
}

/* --- 背景动画 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

.panel-scroll-container { 
  flex: 1; overflow-y: auto; position: relative; z-index: 1; display: flex; flex-direction: column; 
}

/* --- 头部设计 --- */
.main-header { 
  display: flex; flex-wrap: wrap; 
  border-bottom: 4px solid var(--black); 
  background: var(--off-white); position: relative; z-index: 10;
}
.header-split { padding: 30px; }
.header-split.left { 
  background: var(--black); color: var(--off-white); 
  display: flex; justify-content: center; align-items: center; 
  flex: 0 0 320px;
}
.giant-text { 
  font-family: var(--heading); font-size: 3.5rem; line-height: 0.9; 
  text-transform: uppercase; transform: rotate(-2deg); 
}
.text-row.outline { color: var(--black); -webkit-text-stroke: 1px var(--off-white); }
.text-row.red-fill { color: var(--red); margin-left: 20px; }

.header-split.right { flex: 1; display: flex; align-items: center; }
.cn-title { font-weight: 900; margin: 0 0 5px 0; font-size: 1.5rem; letter-spacing: -1px; font-family: var(--mono); }
.live-indicator { display: inline-flex; align-items: center; gap: 8px; font-family: var(--mono); font-size: 0.8rem; color: var(--red); border: 1px solid var(--red); padding: 4px 8px; margin-bottom: 5px; background: rgba(217, 35, 35, 0.05); }
.dot { width: 8px; height: 8px; background: var(--red); border-radius: 50%; animation: pulse 1s infinite; }
.desc { margin: 10px 0 0; font-family: var(--mono); font-size: 0.8rem; color: #555; }
.highlight { background: var(--black); color: #fff; padding: 0 4px; }
.highlight.red { background: var(--red); }

/* --- 跑马灯 --- */
.tech-strip { 
  background: var(--black); color: var(--off-white); 
  padding: 6px 0; border-bottom: 4px solid var(--black); 
  overflow: hidden; white-space: nowrap; font-family: var(--mono); font-size: 0.8rem; 
}
.strip-content { display: inline-block; animation: marquee 20s linear infinite; }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }

/* --- 主布局 --- */
.fb-layout {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 20px;
  max-width: 1600px;
  width: 100%;
  margin: 20px auto;
  padding: 0 20px 40px;
  box-sizing: border-box;
}

/* --- 卡片通用 --- */
.cyber-card {
  background: #fff;
  border: 2px solid var(--black);
  box-shadow: var(--shadow-hard);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.card-label-strip {
  background: var(--black); color: var(--off-white);
  padding: 5px 15px; font-family: var(--mono); font-size: 0.8rem;
  border-bottom: 2px solid var(--black);
}
.card-inner-pad { padding: 25px; }

/* --- 表单样式 --- */
.cyber-label {
  display: block; font-family: var(--mono); font-weight: 700;
  font-size: 0.8rem; margin-bottom: 6px; color: #444;
}
.cyber-label.required::after { content: " *"; color: var(--red); }

.fb-field { margin-bottom: 20px; }

.cyber-input, .cyber-textarea, .cyber-select {
  width: 100%;
  padding: 10px;
  background: #fff;
  border: 2px solid var(--black);
  font-family: var(--mono);
  font-size: 0.9rem;
  color: var(--black);
  box-sizing: border-box;
  transition: 0.2s;
  border-radius: 0;
}
.cyber-input:focus, .cyber-textarea:focus, .cyber-select:focus {
  outline: none;
  border-color: var(--red);
  background: #fffbfb;
  box-shadow: 2px 2px 0 var(--red);
}
.invalid { border-color: var(--red) !important; background: #ffe6e6 !important; }

.cyber-select-wrapper { position: relative; }
.cyber-select { appearance: none; cursor: pointer; }
.select-arrow { position: absolute; right: 10px; top: 12px; pointer-events: none; font-size: 0.7rem; }

.fb-hint-row { display: flex; justify-content: space-between; font-family: var(--mono); font-size: 0.7rem; color: #888; margin-top: 4px; }
.error-msg { font-family: var(--mono); color: var(--red); font-size: 0.75rem; margin-top: 4px; font-weight: 700; }

/* --- 上传区域 --- */
.cyber-upload-zone {
  border: 2px dashed #999;
  background: repeating-linear-gradient(45deg, #f0f0f0, #f0f0f0 10px, #e9e9e9 10px, #e9e9e9 20px);
  padding: 15px;
  text-align: center;
  position: relative;
  transition: 0.3s;
}
.cyber-upload-zone:hover { border-color: var(--black); background: #e0e0e0; }
.cyber-upload-zone.has-file { border-style: solid; border-color: var(--black); background: #fff; padding: 10px; }

.upload-trigger { cursor: pointer; display: block; }
.hidden-input { display: none; }
.upload-icon { font-size: 1.5rem; font-weight: 700; margin-bottom: 5px; }
.upload-text { font-family: var(--heading); font-size: 1.1rem; }
.upload-sub { font-family: var(--mono); font-size: 0.7rem; color: #666; margin-top: 5px; }

.upload-preview { display: flex; align-items: center; gap: 10px; }
.preview-frame { width: 60px; height: 60px; border: 1px solid var(--black); padding: 2px; }
.preview-frame img { width: 100%; height: 100%; object-fit: cover; }
.preview-info { flex: 1; text-align: left; display: flex; flex-direction: column; font-family: var(--mono); font-size: 0.8rem; }
.file-name { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 150px; }
.remove-btn { background: var(--red); color: #fff; border: none; font-family: var(--mono); font-size: 0.7rem; cursor: pointer; padding: 4px 8px; }

/* --- 按钮 --- */
.fb-actions { display: flex; gap: 10px; margin-top: 10px; }
.cyber-btn {
  flex: 1; height: 44px; border: 2px solid var(--black);
  font-family: var(--heading); font-size: 1rem; letter-spacing: 1px;
  cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: 0.2s; box-shadow: 3px 3px 0 var(--black);
}
.cyber-btn.primary { background: var(--black); color: #fff; }
.cyber-btn.primary:hover:not(:disabled) { background: var(--red); border-color: var(--black); transform: translate(-2px, -2px); box-shadow: 5px 5px 0 var(--black); }
.cyber-btn.secondary { background: #fff; color: var(--black); }
.cyber-btn.secondary:hover { background: #f0f0f0; transform: translate(-2px, -2px); box-shadow: 5px 5px 0 var(--black); }
.cyber-btn:disabled { opacity: 0.6; cursor: not-allowed; box-shadow: none; transform: none; }

.status-readout { margin-top: 15px; font-family: var(--mono); font-size: 0.8rem; padding: 8px; border: 1px solid currentColor; }
.status-readout.success { color: green; background: #e6fffa; }
.status-readout.error { color: var(--red); background: #ffe6e6; }

/* --- 列表区域 --- */
.list-header-row { padding: 20px; border-bottom: 2px solid var(--black); display: flex; justify-content: space-between; align-items: center; background: #f9f9f9; }
.list-title { font-family: var(--heading); font-size: 1.4rem; margin: 0; }
.list-controls { display: flex; gap: 15px; align-items: center; }

.cyber-tabs { display: flex; gap: -2px; }
.cyber-tab {
  background: transparent; border: 1px solid var(--black);
  font-family: var(--mono); font-size: 0.8rem; padding: 6px 12px;
  cursor: pointer; font-weight: 700; color: #888;
}
.cyber-tab.active { background: var(--black); color: #fff; z-index: 1; }
.cyber-mini-btn { background: #fff; border: 1px solid var(--black); font-family: var(--mono); cursor: pointer; padding: 6px 10px; font-size: 0.8rem; }
.cyber-mini-btn:hover { background: var(--black); color: #fff; }

.list-content-area { flex: 1; overflow-y: auto; background: #f0f0f0; padding: 20px; max-height: 800px; }

.loading-state, .empty-state { text-align: center; padding: 40px; font-family: var(--mono); color: #888; }
.cyber-spinner { width: 30px; height: 30px; border: 4px solid #ccc; border-top-color: var(--black); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 10px; }
@keyframes spin { to { transform: rotate(360deg); } }
.empty-icon { font-size: 3rem; font-weight: 900; color: #ddd; margin-bottom: 10px; }

/* --- 反馈条目 --- */
.feedback-entry {
  background: #fff; border: 1px solid var(--black); padding: 20px; margin-bottom: 15px;
  position: relative; overflow: hidden; transition: transform 0.2s;
}
.feedback-entry:hover { transform: translateX(5px); box-shadow: -4px 4px 0 rgba(0,0,0,0.1); }
.entry-scanline { position: absolute; left: 0; top: 0; width: 4px; height: 100%; background: #ccc; }
.status-0 .entry-scanline { background: #EAB308; } /* Pending - Yellow */
.status-1 .entry-scanline { background: #3B82F6; } /* Process - Blue */
.status-2 .entry-scanline { background: #10B981; } /* Solved - Green */
.status-3 .entry-scanline { background: #6B7280; } /* Closed - Grey */

.entry-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 10px; }
.title-row { display: flex; align-items: baseline; gap: 10px; }
.entry-id { font-family: var(--mono); color: #888; font-size: 0.9rem; }
.entry-title { font-family: var(--body); font-weight: 800; font-size: 1.1rem; margin: 0; }

.badge-row { display: flex; gap: 5px; }
.cyber-badge { font-family: var(--mono); font-size: 0.7rem; padding: 2px 6px; border: 1px solid var(--black); font-weight: 700; }
.cyber-badge.type { background: #eee; }
.st-0 { background: #FEF3C7; color: #92400E; }
.st-1 { background: #DBEAFE; color: #1E40AF; }
.st-2 { background: #D1FAE5; color: #065F46; }
.st-3 { background: #F3F4F6; color: #374151; }

.entry-meta { display: flex; justify-content: space-between; font-family: var(--mono); font-size: 0.75rem; color: #666; margin-bottom: 10px; border-bottom: 1px dashed #ddd; padding-bottom: 8px; }
.meta-action { background: none; border: none; color: var(--red); cursor: pointer; text-decoration: underline; font-family: inherit; }

.entry-content { font-size: 0.95rem; line-height: 1.6; color: #333; margin-bottom: 15px; }

.entry-attachment { background: #f9f9f9; padding: 10px; border: 1px solid #ddd; display: inline-block; }
.att-label { font-family: var(--mono); font-size: 0.7rem; color: #888; margin-bottom: 5px; }
.att-thumb { height: 60px; border: 1px solid var(--black); cursor: pointer; transition: 0.2s; }
.att-thumb:hover { transform: scale(1.1); }

/* --- 管理员回复 (终端风格) --- */
.admin-reply-terminal {
  background: #1e1e1e; color: #0f0; font-family: var(--mono); 
  padding: 12px; margin-top: 15px; border-left: 4px solid var(--red);
  font-size: 0.85rem;
}
.reply-header { font-weight: 700; margin-bottom: 6px; color: #fff; border-bottom: 1px dashed #444; padding-bottom: 4px; }
.reply-body { line-height: 1.5; color: #ccc; white-space: pre-wrap; }

/* --- 分页 --- */
.cyber-pagination { display: flex; justify-content: center; align-items: center; gap: 15px; margin-top: 20px; font-family: var(--mono); }
.page-btn { background: #fff; border: 1px solid var(--black); cursor: pointer; padding: 5px 10px; font-weight: 700; }
.page-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* --- 模态框 --- */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 9999; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(2px); }
.cyber-modal-window { background: #fff; border: 4px solid var(--black); padding: 0; max-width: 90vw; max-height: 90vh; display: flex; flex-direction: column; }
.modal-header { background: var(--black); color: #fff; padding: 10px 15px; display: flex; justify-content: space-between; font-family: var(--mono); }
.modal-close { background: transparent; border: none; color: #fff; cursor: pointer; font-family: var(--mono); }
.modal-close:hover { color: var(--red); }
.modal-body { padding: 10px; background: #333; overflow: auto; display: flex; justify-content: center; }
.modal-body img { max-width: 100%; max-height: 80vh; border: 1px solid #fff; }

/* --- 响应式 --- */
@media (max-width: 1024px) {
  .fb-layout { grid-template-columns: 1fr; }
  .header-split.left { flex: 0 0 100%; justify-content: center; text-align: center; }
  .header-split.right { display: none; }
  .giant-text { font-size: 2.5rem; }
}

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }
</style>