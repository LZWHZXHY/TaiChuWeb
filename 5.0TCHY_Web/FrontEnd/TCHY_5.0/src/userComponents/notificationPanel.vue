<template>
  <section class="notifications-wrap" aria-labelledby="notifications-title">
    <header class="notifications-header">
      <h1 id="notifications-title">通知</h1>

      <div class="controls">
        <input
          type="search"
          v-model="q"
          placeholder="搜索通知标题或内容"
          @input="onSearchInput"
          aria-label="搜索通知"
        />
        <button @click="refresh" :disabled="loading" class="btn">
          刷新
        </button>
        <button 
          v-if="unreadCount > 0" 
          @click="markAllAsRead" 
          :disabled="loading || markingAll"
          class="btn primary"
        >
          {{ markingAll ? '标记中...' : '全部已读' }}
        </button>
      </div>
    </header>

    <main class="notifications-main">
      <aside class="notifications-list" aria-label="通知列表">
        <div class="list-stats">
          <div>共 {{ filtered.length }} 条</div>
          <div>未读 {{ unreadCount }}</div>
        </div>

        <ul>
          <li
            v-for="item in paged"
            :key="item.Id ?? item.id"
            :class="['notification-row', { unread: !item.IsRead }]"
            @click="openDetail(item)"
            :aria-disabled="isBusy(item)"
          >
            <div class="left">
              <div class="nid">{{ item.Id ?? item.id }}</div>
              <div class="meta">
                <div class="meta-top">
                  <span
                    class="type-badge"
                    :style="{ backgroundColor: getTypeMeta(item.Type ?? item.type).bg }"
                  >
                    <span class="type-icon">{{ getTypeMeta(item.Type ?? item.type).icon }}</span>
                    <span class="type-name">{{ getTypeMeta(item.Type ?? item.type).name }}</span>
                  </span>

                  <div class="title">{{ item.title }}</div>
                </div>

                <div class="snippet">{{ item.content }}</div>
              </div>
            </div>

            <div class="right">
              <div class="time">{{ formatShortTime(item.CreateTime ?? item.createTime) }}</div>
              <div class="actions">
                <button
                  @click.stop="toggleRead(item)"
                  class="icon-btn"
                  :disabled="isBusy(item)"
                >
                  {{ item.IsRead ? '标为未读' : '标为已读' }}
                </button>
                <button
                  @click.stop="remove(item)"
                  class="icon-btn danger"
                  :disabled="isBusy(item)"
                >删除</button>
              </div>
            </div>
          </li>

          <li v-if="!loading && filtered.length === 0" class="empty">暂无通知</li>
          <li v-if="loading" class="empty">加载中…</li>
        </ul>

        <div class="pager" v-if="totalPages > 1">
          <button @click="prev" :disabled="page === 1">上一页</button>
          <span>{{ page }} / {{ totalPages }}</span>
          <button @click="next" :disabled="page === totalPages">下一页</button>
        </div>
      </aside>

      <section class="notifications-detail" aria-live="polite">
        <div v-if="active" class="detail-card">
          <header class="detail-head">
            <div class="detail-left">
              <span
                class="detail-type-badge"
                :style="{ backgroundColor: getTypeMeta(active.Type ?? active.type).bg }"
              >
                <span class="type-icon">{{ getTypeMeta(active.Type ?? active.type).icon }}</span>
              </span>
              <div>
                <h2 class="detail-title">{{ active.title }}</h2>
                <div class="detail-meta">
                  <span class="detail-type-name">{{ getTypeMeta(active.Type ?? active.type).name }}</span>
                  <span class="time">{{ formatFullTime(active.CreateTime ?? active.createTime) }}</span>
                </div>
              </div>
            </div>

            <div class="detail-actions">
              <button class="btn" @click="toggleRead(active)" :disabled="isBusy(active)">
                {{ active.IsRead ? '标为未读' : '标为已读' }}
              </button>
              <button class="btn ghost danger" @click="remove(active)" :disabled="isBusy(active)">删除</button>
            </div>
          </header>

          <article class="detail-body">
            <p v-if="active.title" class="detail-heading">{{ active.title }}</p>
            <p class="detail-content">{{ active.content }}</p>

            <div v-if="active.ExpireTime || active.expireTime" class="expire">
              过期时间：{{ formatFullTime(active.ExpireTime ?? active.expireTime) }}
            </div>
          </article>
        </div>

        <div v-else class="detail-empty">请选择一条通知查看详情</div>
      </section>
    </main>
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, reactive } from 'vue'
import apiClient from '../utils/api'

/* 合并一次性定义 props（使用 withDefaults 提供默认值） */
const props = withDefaults(
  defineProps<{
    user: any
    isMe?: boolean
  }>(),
  { isMe: false }
)

/* API wrapper */
const GetAllNotification = () => apiClient.get('/Notification/all')
const PatchMarkRead = (id: number | string, isRead: boolean) =>
  apiClient.patch(`/Notification/${id}/read`, { isRead })
const DeleteNotification = (id: number | string) => apiClient.delete(`/Notification/${id}`)
const GetUnreadCount = () => apiClient.get('/Notification/unread/count')

/* 本地状态 */
const items = ref<any[]>([])
const loading = ref(false)
const markingAll = ref(false)
const q = ref('')
const page = ref(1)
const pageSize = ref(20)
const activeId = ref<number | string | null>(null)
const searchTimer = ref<number | undefined>(undefined)

/* per-item busy map to prevent duplicate requests */
const busyMap = reactive<Record<string, boolean>>({})

function setBusy(id: number | string | null, val: boolean) {
  if (id == null) return
  busyMap[String(id)] = val
}
function isBusy(item: any) {
  if (!item) return false
  return !!busyMap[String(item.Id ?? item.id)]
}

/* 类型元信息（按你的要求：1 = 紧急通知，2 = 更新通知） */
const typeMetaMap: Record<number, { name: string; icon: string; bg: string }> = {
  1: { name: '紧急通知', icon: '🔔', bg: '#ffeded' }, // 红系背景
  2: { name: '更新通知', icon: '🔄', bg: '#eef6ff' }  // 蓝系背景
}
function getTypeMeta(type: number | undefined) {
  if (!type) return { name: '未知类型', icon: '📄', bg: '#f3f4f6' }
  return typeMetaMap[type] ?? { name: '未知类型', icon: '📄', bg: '#f3f4f6' }
}

/* 加载函数 */
const load = async () => {
  loading.value = true
  try {
    const res = await GetAllNotification()
    const list = res?.data?.data?.items ?? res?.data ?? []
    items.value = list.map((i: any) => normalize(i))

    // 如果只显示当前用户的通知（props.isMe），过滤
    if (props.isMe && props.user?.id != null) {
      items.value = items.value.filter(it => Number(it.UserId ?? it.userId ?? it.user?.id) === Number(props.user.id))
    }
  } catch (err) {
    console.error('获取通知失败', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  load()
})

/* 获取未读数量并更新HeaderNav */
const loadUnreadCount = async () => {
  try {
    const response = await GetUnreadCount()
    const count = response?.data?.unread ?? 0
    // 触发全局事件更新HeaderNav
    window.dispatchEvent(new CustomEvent('unread-count-updated', { 
      detail: { count } 
    }))
    return count
  } catch (err) {
    console.warn('获取未读数量失败:', err)
    // 降级方案：使用本地计算的未读数量
    const localUnreadCount = items.value.filter(item => !item.IsRead && !item.IsDeleted).length
    window.dispatchEvent(new CustomEvent('unread-count-updated', { 
      detail: { count: localUnreadCount } 
    }))
    return localUnreadCount
  }
}

/* normalize：统一后端字段命名差异并强制布尔转换 IsRead */
function normalize(raw: any) {
  const rawIsRead = raw.IsRead ?? raw.isRead ?? raw.read ?? false
  return {
    Id: raw.Id ?? raw.id ?? raw.ID ?? raw.Id,
    UserId: raw.UserId ?? raw.userId ?? raw.user?.id ?? raw.UserId,
    Type: raw.Type ?? raw.type,
    title: raw.title ?? raw.Title ?? '',
    content: raw.content ?? raw.Content ?? '',
    senderID: raw.senderID ?? raw.senderId ?? raw.SenderId,
    IsRead: !!rawIsRead, // 强制为 boolean
    IsDeleted: !!(raw.IsDeleted ?? raw.isDeleted ?? false),
    ExpireTime: raw.ExpireTime ?? raw.expireTime ?? null,
    CreateTime: raw.CreateTime ?? raw.createTime ?? raw.create_time ?? null,
    ReadTime: raw.ReadTime ?? raw.readTime ?? null,
    __raw: raw
  }
}

/* 计算项与分页 */
const filtered = computed(() => {
  const term = (q.value || '').trim().toLowerCase()
  let list = items.value.filter(i => !i.IsDeleted)
  if (term) {
    list = list.filter(i =>
      (i.title || '').toString().toLowerCase().includes(term) ||
      (i.content || '').toString().toLowerCase().includes(term) ||
      (String(i.Id) || '').toLowerCase().includes(term)
    )
  }
  return list
})

const unreadCount = computed(() => filtered.value.filter(i => !i.IsRead).length)
const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize.value)))
const paged = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filtered.value.slice(start, start + pageSize.value)
})
const active = computed(() => items.value.find(it => it.Id === activeId.value) ?? null)

/* 交互函数 */
function onSearchInput() {
  if (searchTimer.value) window.clearTimeout(searchTimer.value)
  searchTimer.value = window.setTimeout(() => {
    page.value = 1
    searchTimer.value = undefined
  }, 200)
}

function refresh() { 
  load() 
}

/* 标记全部已读 */
async function markAllAsRead() {
  if (unreadCount.value === 0) return
  
  markingAll.value = true
  try {
    // 乐观更新：先将所有通知标记为已读
    items.value.forEach(item => {
      if (!item.IsRead) {
        item.IsRead = true
        item.ReadTime = new Date().toISOString()
      }
    })
    
    await apiClient.patch('/Notification/read/all', { isRead: true })
    // 操作成功后重新获取未读数量
    await loadUnreadCount()
  } catch (err) {
    console.error('标记全部已读失败:', err)
    // 失败时重新加载数据
    await load()
  } finally {
    markingAll.value = false
  }
}

async function openDetail(item: any) {
  activeId.value = item.Id
  // 如果未读 -> 标记为已读（调用后端）
  if (!item.IsRead) {
    // optimistic update
    const prev = item.IsRead
    item.IsRead = true
    item.ReadTime = new Date().toISOString()
    try {
      setBusy(item.Id, true)
      await PatchMarkRead(item.Id, true)
      // 操作成功后重新获取未读数量
      await loadUnreadCount()
    } catch (err) {
      // revert on failure
      console.error('标记已读失败', err)
      item.IsRead = prev
      item.ReadTime = prev ? item.ReadTime : null
    } finally {
      setBusy(item.Id, false)
    }
  }
}

async function toggleRead(item: any) {
  const newState = !item.IsRead
  const prevReadTime = item.ReadTime
  // optimistic
  item.IsRead = newState
  item.ReadTime = newState ? new Date().toISOString() : null

  try {
    setBusy(item.Id, true)
    await PatchMarkRead(item.Id, newState)
    // 操作成功后重新获取未读数量
    await loadUnreadCount()
  } catch (err) {
    console.error('切换已读状态失败', err)
    // revert
    item.IsRead = !newState
    item.ReadTime = prevReadTime
  } finally {
    setBusy(item.Id, false)
  }
}

async function remove(item: any) {
  // optimistic: hide locally while request in-flight
  const prevDeleted = item.IsDeleted
  item.IsDeleted = true
  if (activeId.value === item.Id) activeId.value = null

  try {
    setBusy(item.Id, true)
    await DeleteNotification(item.Id)
    // 操作成功后重新获取未读数量
    await loadUnreadCount()
  } catch (err) {
    console.error('删除通知失败', err)
    // revert
    item.IsDeleted = prevDeleted
  } finally {
    setBusy(item.Id, false)
  }
}

function prev() { if (page.value > 1) page.value-- }
function next() { if (page.value < totalPages.value) page.value++ }

/* 格式化 */
function formatShortTime(iso?: string) { return iso ? new Date(iso).toLocaleString() : '' }
function formatFullTime(iso?: string) { return iso ? new Date(iso).toLocaleString() : '' }

/* 重置页码：搜索时 */
watch(q, () => (page.value = 1))
</script>

<style scoped>
/* （保留你已有的 CSS - 未做变更） */

:root {
  --bg: #ffffff;
  --panel-bg: #fbfdff;
  --muted: #6b7280;
  --text: #0f172a;
  --primary: #2563eb;
  --danger: #ef4444;
  --border: #e6e9ee;
  --radius: 12px;
  --shadow: 0 8px 30px rgba(2,6,23,0.06);
  --gap: 16px;
  --max-width: 1100px;
  --control-height: 40px;
}

/* 页面容器 */
.notifications-wrap {
  max-width: var(--max-width);
  margin: 20px auto;
  padding: 20px;
  background: var(--bg);
  border-radius: 12px;
  box-shadow: var(--shadow);
  font-family: Inter, "Segoe UI", Roboto, Arial, sans-serif;
  color: var(--text);
}

/* 头部 */
.notifications-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  margin-bottom: 14px;
}
.controls {
  display: flex;
  gap: 8px;
  align-items: center;
}
.controls input[type="search"] {
  height: var(--control-height);
  padding: 8px 12px;
  border-radius: 10px;
  border: 1px solid var(--border);
  min-width: 220px;
  transition: box-shadow 120ms ease, border-color 120ms ease;
}
.controls input[type="search"]:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 6px 18px rgba(37,99,235,0.06);
}
.btn {
  height: var(--control-height);
  padding: 0 14px;
  border-radius: 10px;
  border: none;
  background: var(--primary);
  color: #fff;
  cursor: pointer;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.btn.primary {
  background: var(--primary);
}
.btn.ghost {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text);
}
.btn.danger {
  background: var(--danger);
}
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 主体布局 */
.notifications-main {
  display: grid;
  grid-template-columns: 420px 1fr;
  gap: 18px;
  align-items: start;
}
@media (max-width: 920px) {
  .notifications-main { grid-template-columns: 1fr; }
}

/* 列表面板 */
.notifications-list {
  background: var(--panel-bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-height: 320px;
}
.list-stats {
  display:flex;
  justify-content:space-between;
  font-size:13px;
  color:var(--muted);
  padding: 6px 4px;
}

/* 通知列表 */
ul {
  list-style: none;
  margin: 0;
  padding: 0;
  display:flex;
  flex-direction:column;
  gap:8px;
  max-height: 62vh;
  overflow: auto;
}
ul::-webkit-scrollbar { width: 10px; }
ul::-webkit-scrollbar-thumb { background: rgba(2,6,23,0.06); border-radius: 8px; }

/* 每行项 */
.notification-row {
  display:flex;
  justify-content:space-between;
  gap: 12px;
  padding: 10px;
  border-radius: 10px;
  cursor: pointer;
  align-items: flex-start;
  transition: background 120ms ease, transform 60ms ease;
}
.notification-row:hover { background: rgba(2,6,23,0.02); transform: translateY(-1px); }
.notification-row.unread { background: linear-gradient(90deg, rgba(37,99,235,0.03), transparent); }

/* 左侧（id + 文本） */
.left { display:flex; gap:12px; align-items:flex-start; min-width: 0; }
.nid {
  min-width: 64px;
  text-align:center;
  font-size:12px;
  color:var(--muted);
  background: #fff;
  border: 1px solid var(--border);
  padding: 6px 8px;
  border-radius: 8px;
  flex-shrink: 0;
}
.meta { min-width: 0; max-width: calc(100% - 160px); }
.meta-top {
  display:flex;
  gap:8px;
  align-items:center;
}
/* type badge in list */
.type-badge {
  display:inline-flex;
  align-items:center;
  gap:8px;
  padding:6px 8px;
  border-radius:999px;
  font-size:12px;
  color:#111827;
  min-width:92px;
  justify-content:center;
  box-shadow: inset 0 0 0 1px rgba(0,0,0,0.02);
}
.type-badge .type-icon { font-size:14px; }
.meta .title {
  font-weight:700;
  font-size:14px;
  white-space:nowrap;
  overflow:hidden;
  text-overflow:ellipsis;
}
.meta .snippet {
  font-size:13px;
  color:var(--muted);
  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;
  overflow:hidden;
  margin-top:6px;
}

/* 右侧（时间 + 操作） */
.right {
  display:flex;
  flex-direction:column;
  align-items:flex-end;
  gap:8px;
  min-width:120px;
}
.time { font-size:12px; color:var(--muted); white-space:nowrap; }
.actions { display:flex; gap:6px; align-items:center; }
.icon-btn { background: transparent; border: none; cursor: pointer; color: var(--primary); padding: 6px; border-radius: 6px; font-weight:600; }
.icon-btn:hover { background: rgba(2,6,23,0.03); }
.icon-btn.danger { color: var(--danger); }

/* 分页 */
.pager { display:flex; gap:8px; align-items:center; justify-content:center; padding:8px 0; }

/* 详情面板 */
.notifications-detail {
  background: #fff;
  border:1px solid var(--border);
  border-radius:10px;
  padding:14px;
  min-height:320px;
  display:flex;
  flex-direction:column;
}
.detail-head { display:flex; justify-content:space-between; align-items:flex-start; gap:12px; }
.detail-left { display:flex; gap:12px; align-items:center; }
.detail-type-badge {
  width:48px;
  height:48px;
  border-radius:10px;
  display:flex;
  align-items:center;
  justify-content:center;
  font-size:20px;
  color:#111827;
}
.detail-title { margin:0 0 6px 0; font-size:18px; }
.detail-meta { color:var(--muted); font-size:13px; display:flex; gap:12px; align-items:center; }

/* 详情正文 */
.detail-body { margin-top:12px; white-space:pre-wrap; line-height:1.7; color:var(--text); background: var(--panel-bg); padding:12px; border-radius:8px; border:1px solid #f0f0f0; }

/* 按钮组 */
.detail-actions { display:flex; gap:8px; margin-top:14px; justify-content:flex-end; }
.btn.ghost { background: transparent; border: 1px solid rgba(0,0,0,0.06); color: var(--text); }
.btn.danger { background: var(--danger); color: #fff; }

/* empty / loading */
.detail-empty, .empty { color:var(--muted); text-align:center; padding:14px 0; }

/* 可访问性焦点态 */
.notification-row:focus-visible, .btn:focus-visible, .icon-btn:focus-visible, .controls input[type="search"]:focus-visible {
  outline: 3px solid rgba(37,99,235,0.12);
  outline-offset: 2px;
  border-radius: 8px;
}

/* 减少动画对偏好减少动画用户的影响 */
@media (prefers-reduced-motion: reduce) {
  * { transition: none !important; animation: none !important; }
}

/* 小屏优化 */
@media (max-width: 600px) {
  .notifications-wrap { padding: 14px; }
  .controls input[type="search"] { min-width: 140px; }
  .nid { display:none; } /* 隐藏 ID 以节省空间 */
  .right { min-width: 90px; }
  .meta .title { white-space: normal; -webkit-line-clamp: 1; display: block; }
}
</style>