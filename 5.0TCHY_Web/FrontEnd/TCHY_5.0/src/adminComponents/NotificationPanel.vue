<template>
  <section class="notify-panel">
    <header class="panel-head">
      <h2 class="title">📡 通知指挥系统 // NOTIFICATION_TERMINAL</h2>
      <button class="refresh-btn" @click="refresh" type="button" :disabled="isLoadingUsers">
        <span class="refresh-icon" :class="{ rotating: isLoadingUsers }">🔄</span>
        {{ isLoadingUsers ? '加载中...' : '刷新用户列表' }}
      </button>
    </header>

    <!-- 全局反馈信息 -->
    <div v-if="globalMessage" class="global-message" :class="globalMessageType">
      {{ globalMessage }}
      <button class="close-msg" @click="globalMessage = ''">✕</button>
    </div>

    <div class="content">
      <!-- 左侧：用户选择面板 -->
      <div class="user-panel">
        <h3>
          <span class="icon">👥</span>
          选择接收用户 // SELECT_RECIPIENTS
        </h3>

        <div class="user-controls">
          <label class="select-all">
            <input
              type="checkbox"
              v-model="selectAll"
              @change="toggleSelectAll"
            />
            <span>全选 / 全不选</span>
          </label>

          <div class="controls-right">
            <div class="search-box">
              <input
                type="text"
                v-model="searchQuery"
                placeholder="搜索 ID/用户名/邮箱..."
                @input="onSearchInput"
              />
              <span class="search-icon">🔍</span>
            </div>

            <!-- ID 排序控制 -->
            <button
              class="sort-btn"
              type="button"
              @click="cycleSortOrder"
              :title="`按 ID 排序：${sortOrderLabel}`"
            >
              ID 排序
              <span class="sort-icon">
                <span v-if="sortOrder === 'none'">—</span>
                <span v-else-if="sortOrder === 'asc'">▲</span>
                <span v-else>▼</span>
              </span>
            </button>
          </div>
        </div>

        <div class="user-list" role="list" aria-label="用户列表">
          <div
            v-for="user in filteredUsers"
            :key="user.id"
            class="user-item"
            :class="{ selected: !!user.selected }"
            @click="toggleUser(user)"
            role="listitem"
            :aria-checked="!!user.selected"
            tabindex="0"
            @keydown.enter.prevent="toggleUser(user)"
          >
            <input
              type="checkbox"
              :checked="user.selected"
              @change.stop="toggleUser(user)"
              @click.stop
            />

            <!-- 用户 ID（横向一列显示） -->
            <div class="user-id" title="用户 ID">{{ user.id }}</div>

            <div class="user-info">
              <div class="user-row-top">
                <div class="user-name">{{ user.name }}</div>
                <div class="user-account" v-if="user.account">@{{ user.account }}</div>
              </div>
              <div class="user-row-bottom">
                <div class="user-email">{{ user.email || '' }}</div>
              </div>
            </div>
          </div>

          <div v-if="users.length === 0 && !isLoadingUsers" class="empty">
            暂无用户数据。请确保后端提供用户列表接口。
          </div>
          <div v-if="isLoadingUsers" class="loading-more">加载用户列表中...</div>
        </div>

        <div class="user-count">
          已选择 <strong>{{ selectedCount }}</strong> 个用户，共 {{ users.length }} 个用户
        </div>
      </div>

      <!-- 右侧：通知编辑面板 -->
      <div class="form-panel">
        <h3>
          <span class="icon">✉️</span>
          指令编辑 // MESSAGE_COMPOSER
        </h3>

        <!-- 通知类型（后端可能强制系统类型，但保留选择用于前端预览） -->
        <div class="field">
          <label for="noticeTypeSelect">信号类型 // TYPE</label>
          <select
            id="noticeTypeSelect"
            v-model="selectedType"
            aria-label="选择通知类型"
          >
            <option value="" disabled>请选择类型</option>
            <option
              v-for="t in notificationTypes"
              :key="t.id"
              :value="t.id"
            >
              {{ t.name }}
            </option>
          </select>
          <p class="hint">注：后端可能强制为系统通知，此选择仅影响预览。</p>
        </div>

        <div class="field">
          <label for="noticeTitle">指令标题 // TITLE（可选）</label>
          <input
            id="noticeTitle"
            type="text"
            v-model="title"
            placeholder="例如：系统维护公告"
            maxlength="200"
          />
        </div>

        <div class="field">
          <label for="noticeMessage">指令内容 // MESSAGE <span class="required">*</span></label>
          <textarea
            id="noticeMessage"
            v-model="message"
            placeholder="请输入详细通知内容……"
            @input="updateCharCount"
          ></textarea>
        </div>

        <div class="message-info">
          <span>📝 字数: {{ charCount }}</span>
          <span>👥 收件人: {{ selectedCount }} 人</span>
        </div>

        <!-- 发送进度与结果 -->
        <div v-if="sendProgress" class="send-progress">{{ sendProgress }}</div>
        <div v-if="sendResult" class="send-result" :class="{ success: sendResult.fail === 0 }">
          ✅ 发送完成：成功 {{ sendResult.success }}，失败 {{ sendResult.fail }}
        </div>

        <div class="send-options">
          <button class="btn btn-secondary" @click="clearForm" type="button" :disabled="isSubmitting">
            <span class="icon">🗑️</span>
            清空
          </button>
          <button class="btn btn-primary" @click="sendNotification" type="button" :disabled="isSubmitting">
            <span class="icon">📤</span>
            {{ isSubmitting ? '发送中...' : '发送指令' }}
          </button>
        </div>

        <!-- 预览区域 -->
        <div class="preview-panel" v-if="message || title">
          <h4>
            <span class="icon">👁️</span>
            预览 // PREVIEW
          </h4>
          <div class="preview-content">
            <div class="preview-type" v-if="selectedTypeObj">
              <span class="type-icon" :style="{ color: selectedTypeObj.color }">
                {{ selectedTypeObj.icon }}
              </span>
              <strong>{{ selectedTypeObj.name }}</strong>
            </div>
            <div class="preview-title" v-if="title">
              <strong>{{ title }}</strong>
            </div>
            <div class="preview-body">{{ message }}</div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import apiClient from '../utils/api'
import { useAuthStore } from '../utils/auth'

// -------------------- 类型定义 --------------------
type User = {
  id: number | string
  name: string
  email?: string
  account?: string
  selected?: boolean
}

// 通知类型（用于前端预览，实际发送时后端可能忽略）
const notificationTypes = [
  { id: 1, name: '系统紧急通知', icon: '🔔', color: '#ff4d4f' },
  { id: 2, name: '更新通知', icon: '🔄', color: '#2563eb' }
]

// -------------------- 状态 --------------------
const authStore = useAuthStore()
const users = ref<User[]>([])
const title = ref<string>('')
const message = ref<string>('')
const searchQuery = ref<string>('')
const selectAll = ref<boolean>(false)
const charCount = ref<number>(0)
const selectedType = ref<number>(1) // 默认系统紧急通知
const sortOrder = ref<'none' | 'asc' | 'desc'>('none')
const isSubmitting = ref<boolean>(false)
const isLoadingUsers = ref<boolean>(false)

// 发送进度与结果
const sendProgress = ref<string>('')
const sendResult = ref<{ success: number; fail: number } | null>(null)

// 全局提示消息
const globalMessage = ref<string>('')
const globalMessageType = ref<'info' | 'error' | 'success'>('info')

// -------------------- 计算属性 --------------------
const selectedCount = computed(() => users.value.filter(u => u.selected).length)

const filteredUsers = computed(() => {
  const q = (searchQuery.value || '').toLowerCase().trim()
  let list = users.value

  if (q) {
    list = list.filter(user =>
      (user.name || '').toLowerCase().includes(q) ||
      (user.email || '').toLowerCase().includes(q) ||
      (user.account || '').toString().toLowerCase().includes(q) ||
      String(user.id).toLowerCase().includes(q)
    )
  }

  if (sortOrder.value !== 'none') {
    list = [...list].sort((a, b) => {
      const ai = Number(a.id)
      const bi = Number(b.id)
      if (Number.isNaN(ai) || Number.isNaN(bi)) {
        return sortOrder.value === 'asc'
          ? String(a.id).localeCompare(String(b.id))
          : String(b.id).localeCompare(String(a.id))
      }
      return sortOrder.value === 'asc' ? ai - bi : bi - ai
    })
  }

  return list
})

const selectedTypeObj = computed(() => notificationTypes.find(t => t.id === selectedType.value) ?? null)

const sortOrderLabel = computed(() => 
  sortOrder.value === 'none' ? '无' : sortOrder.value === 'asc' ? '升序' : '降序'
)

// -------------------- 方法 --------------------
// 加载用户列表（使用 Notification 控制器的 users 接口）
const loadUsers = async () => {
  isLoadingUsers.value = true
  try {
    const res = await apiClient.get('/Notification/users')
    if (res?.data?.success && Array.isArray(res.data.data)) {
      users.value = res.data.data.map((u: any) => ({
        id: u.id,
        name: u.username ?? u.name ?? `用户-${u.id}`,
        email: u.email,
        account: u.username ?? u.account,
        selected: false
      }))
    } else {
      users.value = []
      showGlobalMessage('用户列表为空或接口返回格式错误', 'error')
    }
  } catch (err) {
    console.error('获取用户失败：', err)
    users.value = []
    showGlobalMessage('获取用户列表失败，请检查网络或后端接口', 'error')
  } finally {
    isLoadingUsers.value = false
  }
  updateSelectAllState()
}

const refresh = () => {
  loadUsers()
}

const toggleUser = (user: User) => {
  user.selected = !user.selected
  updateSelectAllState()
}

const toggleSelectAll = () => {
  const newState = selectAll.value
  users.value.forEach(user => { user.selected = newState })
}

const updateSelectAllState = () => {
  const cnt = users.value.filter(u => u.selected).length
  selectAll.value = cnt > 0 && cnt === users.value.length
}

const updateCharCount = () => { charCount.value = message.value.length }

const clearForm = () => {
  title.value = ''
  message.value = ''
  users.value.forEach(user => (user.selected = false))
  selectAll.value = false
  charCount.value = 0
  sendResult.value = null
  sendProgress.value = ''
}

// 显示全局提示
const showGlobalMessage = (msg: string, type: 'info' | 'error' | 'success' = 'info') => {
  globalMessage.value = msg
  globalMessageType.value = type
  setTimeout(() => { globalMessage.value = '' }, 5000)
}

// 发送通知（适配后端单发接口，循环发送）
const sendNotification = async () => {
  const selectedUsers = users.value.filter(u => u.selected)
  if (selectedUsers.length === 0) {
    showGlobalMessage('请至少选择一个接收用户！', 'error')
    return
  }
  if (!message.value.trim()) {
    showGlobalMessage('通知内容不能为空！', 'error')
    return
  }

  // 确认发送
  if (!confirm(`确定向 ${selectedUsers.length} 位用户发送通知吗？`)) return

  isSubmitting.value = true
  sendProgress.value = `📡 发送中... 0/${selectedUsers.length}`
  sendResult.value = null

  let successCount = 0
  let failCount = 0

  // 注意：不需要传递 SenderId，后端会从当前登录用户获取
  for (let i = 0; i < selectedUsers.length; i++) {
    const user = selectedUsers[i]
    const payload = {
      ReceiverId: Number(user.id),      // 与后端 DTO 字段名一致
      Content: message.value,
      Title: title.value?.trim() || null
    }

    try {
      const res = await apiClient.post('/Notification/send-system', payload)
      if (res?.data?.success) {
        successCount++
      } else {
        failCount++
        console.warn(`发送给用户 ${user.id} 失败：`, res?.data?.message)
      }
    } catch (err) {
      console.error(`发送给用户 ${user.id} 出错：`, err)
      failCount++
    } finally {
      sendProgress.value = `📡 发送中... ${i + 1}/${selectedUsers.length}`
    }
  }

  sendResult.value = { success: successCount, fail: failCount }
  sendProgress.value = ''

  if (failCount === 0) {
    showGlobalMessage(`✅ 全部发送成功 (${successCount} 条)`, 'success')
  } else {
    showGlobalMessage(`⚠️ 部分失败：成功 ${successCount}，失败 ${failCount}`, 'error')
  }

  isSubmitting.value = false
}

// 搜索防抖
let searchTimer: number | undefined
const onSearchInput = () => {
  if (searchTimer) window.clearTimeout(searchTimer)
  searchTimer = window.setTimeout(() => { searchTimer = undefined }, 200)
}

// 排序控制
const cycleSortOrder = () => {
  sortOrder.value = sortOrder.value === 'none' ? 'asc' : sortOrder.value === 'asc' ? 'desc' : 'none'
}

// 生命周期
onMounted(() => {
  loadUsers()
})
</script>

<style scoped>
/* ============================================
   通知指挥系统 · 专业面板样式
   继承档案视觉风格，优化可读性与操作反馈
   ============================================ */
:root {
  --bg: #ffffff;
  --muted: #6b7280;
  --text: #0f172a;
  --primary: #2563eb;
  --surface: #fbfdff;
  --border: #e6e9ee;
  --radius: 12px;
  --gap: 16px;
  --success: #2e7d32;
  --error: #d32f2f;
}

.notify-panel {
  width: 93% !important;
  margin: 0 auto;
  padding: 24px 36px 24px 24px;
  background: var(--bg);
  border-radius: 14px;
  box-shadow: 0 8px 26px rgba(15, 23, 42, 0.06);
  color: var(--text);
  font-family: 'Inter', 'Segoe UI', Roboto, Arial, sans-serif;
  display: flex;
  flex-direction: column;
  gap: var(--gap);
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  letter-spacing: -0.01em;
  border-left: 4px solid var(--primary);
  padding-left: 12px;
}
.refresh-btn {
  background: transparent;
  border: 1px solid var(--border);
  border-radius: 30px;
  padding: 8px 16px;
  font-size: 0.9rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
}
.refresh-btn:hover:not(:disabled) {
  background: var(--surface);
  border-color: var(--primary);
}
.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.refresh-icon {
  display: inline-block;
}
.rotating {
  animation: rotate 1s infinite linear;
}
@keyframes rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 全局消息 */
.global-message {
  padding: 12px 16px;
  border-radius: 8px;
  background: #e6f4ff;
  color: #0052cc;
  border: 1px solid #91caff;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.global-message.error {
  background: #fff2f0;
  color: var(--error);
  border-color: #ffccc7;
}
.global-message.success {
  background: #f6ffed;
  color: var(--success);
  border-color: #b7eb8f;
}
.close-msg {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: inherit;
  opacity: 0.7;
}
.close-msg:hover {
  opacity: 1;
}

.content {
  display: grid;
  grid-template-columns: 1fr 420px;
  gap: 24px;
  align-items: start;
}
@media (max-width: 920px) {
  .content {
    grid-template-columns: 1fr;
  }
}

/* 左侧用户面板 */
.user-panel,
.form-panel {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  justify-content: space-between;
  flex-wrap: wrap;
}
.select-all {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--muted);
  cursor: pointer;
}
.controls-right {
  display: flex;
  gap: 8px;
  align-items: center;
}
.search-box {
  position: relative;
  flex: 1;
  max-width: 200px;
}
.search-box input {
  width: 100%;
  padding: 9px 12px 9px 36px;
  border-radius: 8px;
  border: 1px solid var(--border);
  background: #fff;
}
.search-box input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
.search-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  opacity: 0.6;
  pointer-events: none;
}
.sort-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  border-radius: 8px;
  background: #fff;
  border: 1px solid var(--border);
  cursor: pointer;
  font-size: 0.9rem;
}
.sort-btn:hover {
  background: var(--surface);
  border-color: var(--primary);
}
.sort-icon {
  font-size: 0.8rem;
}

.user-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 440px;
  overflow-y: auto;
  padding: 8px;
  background: #fff;
  border-radius: 8px;
  border: 1px solid var(--border);
}
.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.15s;
  min-height: 48px;
}
.user-item:hover {
  background: rgba(59, 130, 246, 0.03);
}
.user-item.selected {
  background: rgba(59, 130, 246, 0.06);
  border-left: 3px solid var(--primary);
}
.user-item input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
  flex-shrink: 0;
}
.user-id {
  min-width: 84px;
  max-width: 120px;
  padding: 6px 8px;
  border-radius: 8px;
  background: #f3f4f6;
  border: 1px solid var(--border);
  font-size: 13px;
  color: var(--muted);
  text-align: center;
  flex-shrink: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.user-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
  width: 100%;
}
.user-row-top {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: space-between;
}
.user-name {
  font-size: 14px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.user-account {
  font-size: 12px;
  color: var(--muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.user-row-bottom {
  display: flex;
  gap: 8px;
  margin-top: 4px;
  align-items: center;
}
.user-email {
  font-size: 12px;
  color: var(--muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.empty,
.loading-more {
  padding: 18px;
  text-align: center;
  color: var(--muted);
  border-radius: 8px;
  background: #fff;
}
.user-count {
  margin-top: 8px;
  text-align: center;
  color: var(--muted);
  font-size: 13px;
  padding: 8px;
  border-radius: 8px;
  background: #fff;
}

/* 右侧表单 */
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.field label {
  font-weight: 600;
  font-size: 13px;
  color: var(--text);
  display: flex;
  align-items: center;
  gap: 4px;
}
.required {
  color: var(--error);
  font-size: 1.2rem;
  line-height: 1;
}
.field input,
.field select,
textarea {
  padding: 10px 12px;
  border-radius: 8px;
  border: 1px solid var(--border);
  background: #fff;
  font-size: 14px;
  transition: border 0.2s, box-shadow 0.2s;
}
.field input:focus,
.field select:focus,
textarea:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
textarea {
  min-height: 160px;
  resize: vertical;
  line-height: 1.5;
}
.hint {
  font-size: 12px;
  color: var(--muted);
  margin-top: 2px;
}
.message-info {
  display: flex;
  gap: 20px;
  font-size: 13px;
  color: var(--muted);
  background: #fff;
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid var(--border);
}
.send-progress {
  font-size: 13px;
  color: var(--primary);
  background: #e6f7ff;
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid #91caff;
}
.send-result {
  font-size: 13px;
  background: #fff3cd;
  color: #856404;
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid #ffeeba;
}
.send-result.success {
  background: #d4edda;
  color: var(--success);
  border-color: #c3e6cb;
}
.send-options {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  align-items: center;
  margin-top: 8px;
}
.btn {
  padding: 10px 18px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: background 0.2s, transform 0.1s;
}
.btn-primary {
  background: var(--primary);
  color: #ffffff;
}
.btn-primary:hover:not(:disabled) {
  background: #1d4ed8;
  transform: translateY(-1px);
}
.btn-secondary {
  background: #f3f4f6;
  color: var(--text);
}
.btn-secondary:hover:not(:disabled) {
  background: #e5e7eb;
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.preview-panel {
  margin-top: 16px;
  padding: 16px;
  border-radius: 8px;
  background: #fff;
  border: 1px solid var(--border);
}
.preview-panel h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: var(--muted);
  display: flex;
  align-items: center;
  gap: 6px;
}
.preview-type {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
  font-size: 14px;
}
.preview-title {
  margin-bottom: 8px;
  font-size: 16px;
  color: var(--text);
}
.preview-body {
  font-size: 14px;
  line-height: 1.6;
  color: #1f2937;
  white-space: pre-wrap;
  word-break: break-word;
  background: #f9fafb;
  padding: 12px;
  border-radius: 6px;
}

/* 响应式调整 */
@media (max-width: 600px) {
  .notify-panel {
    padding: 16px;
  }
  .content {
    grid-template-columns: 1fr;
  }
  .send-options {
    flex-direction: column;
  }
  .btn {
    width: 100%;
    justify-content: center;
  }
  .controls-right {
    flex-wrap: wrap;
  }
  .search-box {
    max-width: 100%;
  }
}
</style>