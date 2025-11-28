<template>
  <section class="notify-panel">
    <header class="panel-head">
      <h2 class="title">通知系统</h2>
      <button class="refresh-btn" @click="refresh" type="button">
        <span class="refresh-icon">🔄</span>
        刷新用户列表
      </button>
    </header>

    <div class="content">
      <!-- 用户列表 -->
      <div class="user-panel">
        <h3>
          <span class="icon">👥</span>
          选择接收用户
        </h3>

        <div class="user-controls">
          <label class="select-all">
            <input
              type="checkbox"
              v-model="selectAll"
              @change="toggleSelectAll"
            />
            <span>全选/取消全选</span>
          </label>

          <div class="controls-right">
            <div class="search-box">
              <input
                type="text"
                v-model="searchQuery"
                placeholder="搜索用户..."
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
              aria-pressed="false"
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

          <div v-if="users.length === 0" class="empty">
            暂无用户数据。请通过父组件或 API 注入 users 列表，或点击刷新加载。
          </div>
        </div>

        <div class="user-count">
          已选择 {{ selectedCount }} 个用户，共 {{ users.length }} 个用户
        </div>
      </div>

      <!-- 通知编辑（包含标题输入与下拉选择通知类型） -->
      <div class="form-panel">
        <h3>
          <span class="icon">✉️</span>
          通知内容
        </h3>

        <div class="field">
          <label for="noticeTypeSelect">通知类型</label>
          <select
            id="noticeTypeSelect"
            v-model="selectedType"
            aria-label="选择通知类型"
          >
            <option value="" disabled>请选择通知类型</option>
            <option
              v-for="t in notificationTypes"
              :key="t.id"
              :value="t.id"
            >
              {{ t.name }}
            </option>
          </select>
          <p class="hint">后端说明：type=1 为系统紧急通知，type=2 为更新通知。</p>
        </div>

        <div class="field">
          <label for="noticeTitle">通知标题</label>
          <input
            id="noticeTitle"
            type="text"
            v-model="title"
            placeholder="请输入通知标题（可选）"
            maxlength="200"
          />
          <p class="hint">标题长度上限 200 字符（非必填）。</p>
        </div>

        <div class="field">
          <label for="noticeMessage">通知文本</label>
          <textarea
            id="noticeMessage"
            v-model="message"
            placeholder="请输入系统通知内容……"
            @input="updateCharCount"
          ></textarea>
        </div>

        <div class="message-info">
          <span>字数: {{ charCount }}</span>
          <span>收件人: {{ selectedCount }} 人</span>
        </div>

        <div class="send-options">
          <button class="btn btn-secondary" @click="clearForm" type="button">
            <span class="icon">🗑️</span>
            清空
          </button>
          <button class="btn btn-primary" @click="sendNotification" type="button" :disabled="isSubmitting">
            <span class="icon">📤</span>
            {{ isSubmitting ? '发送中...' : '发送通知' }}
          </button>
        </div>

        <div class="preview-panel" v-if="message || title">
          <h4>
            <span class="icon">👁️</span>
            预览
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

type User = {
  id: number | string
  name: string
  email?: string
  account?: string
  selected?: boolean
}

/* 通知类型（后端约定） */
const notificationTypes = [
  { id: 1, name: '系统紧急通知', icon: '🔔', color: '#ff4d4f' },
  { id: 2, name: '更新通知', icon: '🔄', color: '#2563eb' }
]

/* 状态 */
const users = ref<User[]>([])
const title = ref<string>('')           // 新增：通知标题
const message = ref<string>('')
const searchQuery = ref<string>('')
const selectAll = ref<boolean>(false)
const charCount = ref<number>(0)
const selectedType = ref<number>(1) // 默认选 1（系统紧急通知）
const sortOrder = ref<'none' | 'asc' | 'desc'>('none')
const isSubmitting = ref<boolean>(false)

/* 计算属性 */
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
const sortOrderLabel = computed(() => (sortOrder.value === 'none' ? '无' : sortOrder.value === 'asc' ? '升序' : '降序'))

/* 方法 */
const refresh = () => { loadUsers() }

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
}

const sendNotification = async () => {
  const selectedUsers = users.value.filter(u => u.selected)
  if (selectedUsers.length === 0) {
    alert('请至少选择一个用户！')
    return
  }
  if (!message.value.trim()) {
    alert('请输入通知内容！')
    return
  }

  // payload 与后端 DTO 字段对齐（注意首字母大小写）
  const payload = {
    Type: Number(selectedType.value),
    Title: title.value?.trim() ?? '',
    Content: message.value,
    Recipients: selectedUsers.map(u => Number(u.id)),
    SenderId: 1,
    ExpireTime: null as string | null
  }

  isSubmitting.value = true
  try {
    const res = await apiClient.post('/Notification/send', payload)
    if (res?.data?.success) {
      const inserted = res.data.inserted ?? selectedUsers.length
      alert(`通知已写入数据库，已插入 ${inserted} 条记录（类型: ${selectedTypeObj.value?.name ?? selectedType.value}）`)
      clearForm()
      await loadUsers()
    } else {
      const msg = res?.data?.message ?? '后端返回失败'
      alert(`发送失败：${msg}`)
    }
  } catch (err) {
    console.error('发送通知错误：', err)
    alert('发送失败，请查看控制台或后端日志。')
  } finally {
    isSubmitting.value = false
  }
}

/* 搜索防抖 */
let searchTimer: number | undefined
const onSearchInput = () => {
  if (searchTimer) window.clearTimeout(searchTimer)
  searchTimer = window.setTimeout(() => { searchTimer = undefined }, 200)
}

/* 排序控制 */
const cycleSortOrder = () => {
  sortOrder.value = sortOrder.value === 'none' ? 'asc' : sortOrder.value === 'asc' ? 'desc' : 'none'
}

/* 加载用户（与后端 NotificationController 的 GetUserDataList 相对应） */
const loadUsers = async () => {
  try {
    const res = await apiClient.get('/Notification/list')
    if (res?.data?.success && Array.isArray(res.data.data?.items)) {
      users.value = res.data.data.items.map((u: any) => ({
        id: u.id,
        name: u.username ?? (u.name ?? `用户-${u.id}`),
        email: u.email,
        account: u.username ?? u.account,
        selected: false
      }))
    } else {
      users.value = []
    }
    selectAll.value = false
    updateSelectAllState()
  } catch (err) {
    console.error('获取用户失败：', err)
    users.value = []
    selectAll.value = false
  }
}

onMounted(() => { loadUsers() })
</script>

<style scoped>
/* 采用之前优化过的样式（只保留必要样式以便文件自包含） */

:root{
  --bg:#fff; --muted:#6b7280; --text:#0f172a; --primary:#2563eb;
  --surface:#fbfdff; --border:#e6e9ee; --radius:12px; --gap:16px;
  --max-width:1100px; --transition-fast:120ms;
}

/* 这里把容器右侧内边距加大，让右列不紧贴右边缘 */
.notify-panel {
  width:93% !important;
  margin:0;
  padding:20px 36px 20px 20px; /* top/right/bottom/left -> 增加右侧内边距为 36px */
  background:var(--bg);
  border-radius:14px;
  box-shadow:0 8px 26px rgba(15,23,42,0.06);
  color:var(--text);
  font-family:Inter, "Segoe UI", Roboto, Arial, sans-serif;
  display:flex;
  flex-direction:column;
  gap:var(--gap);
}

/* 如果需要在窄屏时恢复较小的内边距 */
@media (max-width: 920px){
  .notify-panel {
    padding: 16px; /* 移动端使用统一小内边距 */
  }
}

.panel-head{ display:flex; justify-content:space-between; align-items:center; gap:12px; }
.title{ margin:0; font-size:20px; font-weight:600; }

.content{ display:grid; grid-template-columns:1fr 420px; gap:24px; align-items:start; }
@media (max-width:920px){ .content{ grid-template-columns:1fr; } }

.user-panel, .form-panel{
  background:var(--surface); border:1px solid var(--border); border-radius:10px;
  padding:12px; display:flex; flex-direction:column; gap:12px;
}

.user-controls{ display:flex; align-items:center; gap:12px; justify-content:space-between; flex-wrap:wrap; }
.select-all{ display:flex; align-items:center; gap:8px; color:var(--muted); cursor:pointer; }
.controls-right{ display:flex; gap:8px; align-items:center; }

.search-box{ position:relative; flex:1; max-width:200px; right:0%;}
.search-box input{ width:80%; padding:9px 12px 9px 36px; border-radius:8px; border:1px solid var(--border); }
.search-icon{ position:absolute; left:10px; top:50%; transform:translateY(-50%); opacity:0.6; }

.sort-btn{ display:inline-flex; align-items:center; gap:8px; padding:8px 10px; border-radius:8px; background:#fff; border:1px solid var(--border); cursor:pointer; }

.user-list{ display:flex; flex-direction:column; gap:8px; max-height:440px; overflow:auto; padding:8px; }
.user-item{ display:flex; align-items:center; gap:12px; padding:10px; border-radius:10px; cursor:pointer; transition:background var(--transition-fast); min-height:48px; }
.user-item:hover{ background:rgba(59,130,246,0.03); }
.user-item.selected{ background:rgba(59,130,246,0.06); }

.user-item input[type="checkbox"]{ width:18px; height:18px; accent-color:var(--primary); flex-shrink:0; }

.user-id{ min-width:84px; max-width:120px; padding:6px 8px; border-radius:8px; background:#fff; border:1px solid var(--border); font-size:13px; color:var(--muted); text-align:center; flex-shrink:0; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }

.user-info{ display:flex; flex-direction:column; min-width:0; width:100%; }
.user-row-top{ display:flex; align-items:center; gap:8px; justify-content:space-between; }
.user-name{ font-size:14px; font-weight:500; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.user-account{ font-size:12px; color:var(--muted); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.user-row-bottom{ display:flex; gap:8px; margin-top:4px; align-items:center; }
.user-email{ font-size:12px; color:var(--muted); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }

.empty{ padding:18px; text-align:center; color:var(--muted); border-radius:8px; background:linear-gradient(180deg,#fff,#fbfdff); }

.user-count{ margin-top:8px; text-align:center; color:var(--muted); font-size:13px; padding:8px; border-radius:8px; background:#fff; }

.field{ display:flex; flex-direction:column; gap:8px; }
.field label{ font-weight:600; font-size:13px; color:var(--text); }
.field input, .field select, textarea{ padding:10px 12px; border-radius:8px; border:1px solid var(--border); background:#fff; font-size:14px; }
.field input:focus, .field select:focus, textarea:focus{ outline:none; box-shadow:0 6px 18px rgba(37,99,235,0.06); border-color:var(--primary); }

textarea{ min-height:160px; resize:vertical; line-height:1.5; }

.hint{ font-size:12px; color:var(--muted); }

.send-options{ display:flex; gap:12px; justify-content:flex-end; align-items:center; }
.btn{ padding:9px 14px; border-radius:10px; font-weight:600; cursor:pointer; border:none; }
.btn-primary{ background:var(--primary); color:#000000; }
.btn-secondary{ background:#f3f4f6; color:var(--text); }

.preview-panel{ margin-top:8px; padding:12px; border-radius:8px; background:#fff; border:1px solid var(--border); }
.preview-type{ display:flex; align-items:center; gap:10px; margin-bottom:8px; }
.preview-title{ margin-bottom:8px; }

@media (max-width:600px){
  .content{ grid-template-columns:1fr; }
  .send-options{ flex-direction:column; }
  .btn{ width:100%; justify-content:center; }
}
</style>