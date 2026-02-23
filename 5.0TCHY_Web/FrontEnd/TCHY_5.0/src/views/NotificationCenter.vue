<template>
  <div class="notification-page">
    <div class="paper-texture"></div>
    <div class="drafting-grid"></div>

    <div class="notification-wrapper">
      <header class="page-header">
        <div class="header-left">
          <button class="back-to-archives" @click="$router.back()">
            <span class="arrow">←</span> [ 返回 // RETURN ]
          </button>
          <div class="serial-no">
            <span class="lbl">NOTIFICATION_CENTER</span>
            <span class="val">v2.0</span>
          </div>
        </div>
        <div class="header-right">
          <div class="stamp" :class="{ active: unreadCount > 0 }">
            {{ unreadCount > 0 ? `未读 ${unreadCount}` : 'ALL_READ' }}
          </div>
        </div>
      </header>

      <hr class="heavy-line" />

      <div class="action-bar">
        <button class="formal-btn small" @click="markAllAsRead" :disabled="unreadCount === 0">
          <span class="btn-text">全部标为已读 // MARK_ALL_READ</span>
        </button>
        <button class="formal-btn small" @click="refreshList">
          <span class="btn-text">刷新 // REFRESH</span>
        </button>
      </div>

      <div class="split-layout">
        <div class="list-column">
          <div class="notification-list custom-scroll">
            <div v-if="loading && notifications.length === 0" class="loading-indicator">加载中...</div>
            <div v-else-if="notifications.length === 0" class="empty-state">
              <span class="empty-icon">📭</span>
              <span class="empty-text">暂无通知 // NO_MESSAGES</span>
            </div>
            
            <div
              v-for="item in notifications"
              :key="item.id"
              class="notification-item"
              :class="{
                unread: !item.isRead,
                selected: selectedNotification?.id === item.id
              }"
              @click="selectNotification(item)"
            >
              <div class="item-left">
                <div class="avatar-wrapper">
                  <img v-if="item.senderLogo" :src="item.senderLogo" class="sender-avatar" />
                  <div v-else class="item-icon">{{ getTypeIcon(item.category) }}</div>
                </div>
              </div>
              <div class="item-content">
                <div class="item-title">
                  <span class="sender-name">{{ item.senderName }}</span>
                  <span v-if="item.title"> · {{ item.title }}</span>
                </div>
                <div class="item-desc">{{ truncate(item.content, 50) }}</div>
                <div class="item-meta">
                  <span class="item-time">{{ formatTime(item.createdAt) }}</span>
                  <span class="item-type">[{{ getTypeName(item.category) }}]</span>
                </div>
              </div>
              <div class="item-right">
                <div v-if="!item.isRead" class="unread-dot"></div>
              </div>
            </div>

            <div v-if="hasMore" class="load-more">
              <button class="formal-btn tiny" @click="loadMore" :disabled="loading">
                {{ loading ? '加载中...' : '加载更多 // LOAD_MORE' }}
              </button>
            </div>
          </div>
        </div>

        <div class="detail-column">
          <div v-if="selectedNotification" class="detail-card">
            <div class="detail-header">
              <div class="detail-origin">
                <img :src="selectedNotification.senderLogo" class="detail-avatar" v-if="selectedNotification.senderLogo">
                <span class="origin-name">FROM: {{ selectedNotification.senderName }}</span>
              </div>
              
              <h3 class="detail-title">{{ selectedNotification.title || '无标题消息' }}</h3>
              
              <div class="detail-meta">
                <span class="meta-type">分类：{{ getTypeName(selectedNotification.category) }}</span>
                <span class="meta-time">录入时间：{{ formatFullTime(selectedNotification.createdAt) }}</span>
              </div>
            </div>
            <div class="detail-content">
              {{ selectedNotification.content }}
            </div>
            <div class="detail-actions">
              <button
                v-if="canJump(selectedNotification)"
                class="formal-btn small jump-btn"
                @click="goToSource(selectedNotification)"
              >
                调取原宗 // VIEW_SOURCE
              </button>

              <button
                v-if="!selectedNotification.isRead"
                class="formal-btn small"
                @click="markSelectedAsRead"
              >
                标记已读 // MARK_READ
              </button>
              <span v-else class="read-tag">✅ 已通过阅览</span>
            </div>
          </div>
          <div v-else class="detail-placeholder">
            <span class="placeholder-icon">📋</span>
            <span class="placeholder-text">请选择一条通知 // SELECT_A_MESSAGE</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const router = useRouter()
const authStore = useAuthStore()

// 状态
const notifications = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = 20
const hasMore = ref(true)
const unreadCount = ref(0)
const selectedNotification = ref(null)

// 获取通知列表
const fetchNotifications = async (reset = true) => {
  if (reset) {
    page.value = 1
    notifications.value = []
    hasMore.value = true
    selectedNotification.value = null
  }
  if (!hasMore.value || loading.value) return

  loading.value = true
  try {
    const res = await apiClient.get('/Notification/list', {
      params: { page: page.value, limit: pageSize }
    })
    if (res.data.success) {
      const items = res.data.data.items || []
      
      const mapped = items.map(item => ({
        id: item.Id,
        title: item.Title || '',
        content: item.Content,
        createdAt: item.CreatedAt,
        isRead: item.IsRead,
        category: item.Category,
        actionType: item.ActionType,
        senderName: item.SenderName || '系统/未知',
        senderLogo: item.SenderLogo,
        targetId: item.TargetId
      }))
      notifications.value.push(...mapped)
      hasMore.value = items.length === pageSize

      if (reset) {
        const unreadRes = await apiClient.get('/Notification/unread-count')
        if (unreadRes.data.success) {
          unreadCount.value = unreadRes.data.count
        }
      }
    }
  } catch (err) {
    console.error('获取通知失败', err)
  } finally {
    loading.value = false
  }
}

// 刷新列表
const refreshList = () => fetchNotifications(true)

// 加载更多
const loadMore = () => {
  if (!hasMore.value || loading.value) return
  page.value++
  fetchNotifications(false)
}

// 标记单条已读
const markAsRead = async (id) => {
  try {
    // 1. 请求方法改为 patch
    // 2. 将 id 拼接到 URL 路径中，匹配后端的 [HttpPatch("{id}/read")]
    const res = await apiClient.patch(`/Notification/${id}/read`)
    
    if (res.data.success) {
      const item = notifications.value.find(n => n.id === id)
      if (item && !item.isRead) {
        item.isRead = true
        unreadCount.value = Math.max(0, unreadCount.value - 1)
        if (selectedNotification.value?.id === id) {
          selectedNotification.value.isRead = true
        }
      }
    }
  } catch (err) {
    console.error('标记失败', err)
  }
}

// 全部标记已读
const markAllAsRead = async () => {
  try {
    // 1. 请求方法保持 post
    // 2. 路径改为匹配后端的 [HttpPost("read-all")]
    const res = await apiClient.post('/Notification/read-all')
    
    if (res.data.success) {
      notifications.value.forEach(n => (n.isRead = true))
      if (selectedNotification.value) {
        selectedNotification.value.isRead = true
      }
      unreadCount.value = 0
    }
  } catch (err) {
    console.error('全部标记失败', err)
  }
}

// 标记当前选中为已读
const markSelectedAsRead = () => {
  if (selectedNotification.value && !selectedNotification.value.isRead) {
    markAsRead(selectedNotification.value.id)
  }
}

// 选择通知
const selectNotification = (item) => {
  selectedNotification.value = item
}

// 检查是否具备跳转条件
const canJump = (item) => {
  if (!item || !item.targetId || item.targetId === "0") return false;
  return [1, 2, 3].includes(item.category);
}

// 执行跳转到原地址
const goToSource = (item) => {
  if (!canJump(item)) return;

  // 跳转时自动标记为已读
  if (!item.isRead) {
    markSelectedAsRead();
  }

  let routePath = '';

  // 根据通知分类，拼装目标路由。这里需要与你的 Vue Router 实际配置对应
  switch (item.category) {
    case 1: // 帖子
      routePath = `/post/${String(item.targetId).replace('post-', '')}`; 
      break;
    case 2: // 画廊
      routePath = `/gallery/${item.targetId}`;
      break;
    case 3: // 博客
      routePath = `/blog/${item.targetId}`;
      break;
    default:
      console.warn('未知的 Category，无法跳转');
      return;
  }

  if (routePath) {
    router.push(routePath);
  }
}

// 工具函数：根据 category 返回图标和类型名称
const getTypeIcon = (category) => {
  const map = {
    1: '💬',
    2: '🖼️',
    3: '📝',
    4: '🔔',
  }
  return map[category] || '📄'
}
const getTypeName = (category) => {
  const map = {
    1: '评论',
    2: '画廊',
    3: '博客',
    4: '系统',
  }
  return map[category] || '其他'
}

// 时间格式化 (YYYY-MM-DD 档案风格)
const formatTime = (timestamp) => {
  if (!timestamp) return ''
  const date = new Date(timestamp)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// 完整时间格式化 (包含时分秒)
const formatFullTime = (timestamp) => {
  if (!timestamp) return ''
  return timestamp.replace('T', ' ').substring(0, 19)
}

// 文本截断
const truncate = (text, length) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

onMounted(() => {
  fetchNotifications()
})
</script>

<style scoped>
/* =========================================
   通知中心 · 档案风格优化版
   ========================================= */

@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Playfair+Display:wght@700;900&display=swap');

.notification-page {
  --ink: #111111;
  --paper: #ffffff;
  --dim: #5e5e5e;
  --accent: #D92323;
  --border: #aaa;
  --bg-soft: #f5f3f0;
  --approved: #2e7d32;
  --font-title: 'Playfair Display', 'Georgia', serif;
  --font-mono: 'JetBrains Mono', 'Courier New', monospace;
  --font-body: 'Georgia', 'PingFang SC', serif;

  width: 100%;
  min-height: 100vh;
  background-color: #e5e2da;
  color: var(--ink);
  font-family: var(--font-body);
  display: flex;
  justify-content: center;
  padding: 40px 20px;
  position: relative;
  line-height: 1.6;
}

.paper-texture {
  position: fixed;
  inset: 0;
  pointer-events: none;
  opacity: 0.03;
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.8' numOctaves='1' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)' opacity='0.2'/%3E%3C/svg%3E");
}
.drafting-grid {
  position: fixed;
  inset: 0;
  pointer-events: none;
  opacity: 0.05;
  background-image: 
    linear-gradient(#888 0.8px, transparent 0.8px),
    linear-gradient(90deg, #888 0.8px, transparent 0.8px);
  background-size: 32px 32px;
}

.notification-wrapper {
  width: 90%;
  max-width: 1400px;
  background: var(--paper);
  box-shadow: 16px 16px 0 rgba(0, 0, 0, 0.15);
  padding: 48px 64px;
  position: relative;
  border: 2px solid var(--ink);
  z-index: 10;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 20px;
}
.back-to-archives {
  background: none;
  border: none;
  padding: 0;
  color: var(--dim);
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.5px;
  cursor: pointer;
  text-transform: uppercase;
  font-family: var(--font-mono);
  transition: color 0.2s;
}
.back-to-archives:hover {
  color: var(--accent);
}
.serial-no {
  font-family: var(--font-mono);
  margin-top: 12px;
}
.serial-no .lbl {
  font-size: 0.7rem;
  color: var(--dim);
  margin-right: 6px;
}
.serial-no .val {
  font-size: 1.2rem;
  font-weight: 700;
  border-bottom: 2px solid var(--ink);
  padding-bottom: 2px;
}
.stamp {
  border: 4px double #bbb;
  color: #999;
  padding: 10px 20px;
  font-weight: 900;
  font-size: 0.9rem;
  transform: rotate(-2deg);
  display: inline-block;
  font-family: var(--font-mono);
  letter-spacing: 1px;
  background: rgba(255,255,255,0.3);
}
.stamp.active {
  border-color: var(--approved);
  color: var(--approved);
}
.heavy-line {
  height: 4px;
  background: var(--ink);
  border: none;
  margin: 24px 0 32px 0;
}

.action-bar {
  display: flex;
  gap: 16px;
  margin-bottom: 32px;
}
.formal-btn.small {
  padding: 10px 20px;
  font-size: 0.8rem;
  box-shadow: 5px 5px 0 var(--ink);
}
.formal-btn.tiny {
  padding: 8px 16px;
  font-size: 0.75rem;
  box-shadow: 4px 4px 0 var(--ink);
}
.formal-btn {
  border: 2px solid var(--ink);
  background: #fff;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.1s ease;
  text-transform: uppercase;
  font-family: var(--font-mono);
  letter-spacing: 0.5px;
  position: relative;
}
.formal-btn:hover:not(:disabled) {
  background: var(--ink);
  color: #fff;
  transform: translate(3px, 3px);
  box-shadow: 2px 2px 0 var(--ink);
}
.formal-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
  border-color: #ccc;
}

/* 跳转按钮特殊样式 */
.formal-btn.jump-btn {
  background: var(--ink);
  color: #fff;
}
.formal-btn.jump-btn:hover {
  background: var(--accent);
  box-shadow: 2px 2px 0 var(--ink);
}

.split-layout {
  display: grid;
  grid-template-columns: 1fr 1.3fr;
  gap: 40px;
  min-height: 550px;
}

.list-column {
  border-right: 2px dashed #ccc;
  padding-right: 24px;
}
.notification-list {
  max-height: 70vh;
  overflow-y: auto;
  border: 1px solid var(--border);
  background: #fefcf9;
  scrollbar-width: thin;
}
.notification-list::-webkit-scrollbar {
  width: 5px;
}
.notification-list::-webkit-scrollbar-thumb {
  background: var(--ink);
}
.loading-indicator,
.empty-state {
  padding: 80px 20px;
  text-align: center;
  color: var(--dim);
  font-weight: 400;
  font-size: 1rem;
  font-style: italic;
}
.empty-icon {
  display: block;
  font-size: 3.5rem;
  margin-bottom: 16px;
  opacity: 0.6;
}

.notification-item {
  display: flex;
  padding: 20px 20px;
  border-bottom: 1px solid #e0ddd8;
  transition: background 0.15s;
  cursor: pointer;
}
.notification-item:hover {
  background: #f3f1ed;
}
.notification-item.unread {
  background: #f0f5fb;
  border-left: 5px solid var(--accent);
}
.notification-item.selected {
  background: #e3e1dd;
  border-left: 5px solid var(--ink);
}
.item-left {
  margin-right: 16px;
}

.avatar-wrapper {
  width: 48px;
  height: 48px;
  border: 1px solid var(--ink);
  overflow: hidden;
  background: #eee;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 3px 3px 0 rgba(0,0,0,0.05);
}
.sender-avatar {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(0.3);
}
.item-icon {
  font-size: 1.2rem;
}

.item-content {
  flex: 1;
  overflow: hidden;
}
.item-title {
  font-size: 1.05rem;
  margin-bottom: 6px;
  font-family: var(--font-title);
  letter-spacing: -0.2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.sender-name {
  font-weight: 900;
  color: var(--ink);
}
.item-desc {
  font-size: 0.85rem;
  color: #444;
  margin-bottom: 8px;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.item-meta {
  display: flex;
  gap: 16px;
  font-size: 0.7rem;
  color: #777;
  font-family: var(--font-mono);
  align-items: center;
}
.item-time {
  background: var(--ink);
  color: #fff;
  padding: 1px 5px;
}
.item-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: space-between;
  min-width: 30px;
}
.unread-dot {
  width: 10px;
  height: 10px;
  background: var(--accent);
  border-radius: 0;
  transform: rotate(45deg);
}
.load-more {
  padding: 24px;
  text-align: center;
}

.detail-column {
  padding-left: 5px;
}
.detail-card {
  background: #fefcf9;
  border: 2px solid var(--ink);
  padding: 36px;
  min-height: 450px;
  display: flex;
  flex-direction: column;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.08);
}
.detail-header {
  border-bottom: 2px solid var(--ink);
  padding-bottom: 20px;
  margin-bottom: 24px;
}
.detail-origin {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  font-family: var(--font-mono);
  font-size: 0.9rem;
}
.detail-avatar {
  width: 32px;
  height: 32px;
  border: 1px solid var(--ink);
  object-fit: cover;
}
.origin-name {
  border-bottom: 1px solid var(--ink);
  padding-bottom: 2px;
  font-weight: bold;
}
.detail-title {
  font-size: 2.2rem;
  font-weight: 900;
  margin: 0 0 12px 0;
  line-height: 1.2;
  font-family: var(--font-title);
  letter-spacing: -0.5px;
  word-break: break-word;
}
.detail-meta {
  display: flex;
  gap: 24px;
  font-size: 0.85rem;
  color: #666;
  font-family: var(--font-mono);
}
.detail-content {
  flex: 1;
  font-size: 1.05rem;
  line-height: 1.8;
  white-space: pre-wrap;
  word-break: break-word;
  margin-bottom: 32px;
  color: #1e1e1e;
}
.detail-actions {
  border-top: 2px dashed #ccc;
  padding-top: 24px;
  display: flex;
  gap: 20px;
}
.read-tag {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--approved);
  font-family: var(--font-mono);
  display: flex;
  align-items: center;
  gap: 5px;
}
.detail-placeholder {
  background: #faf8f5;
  border: 2px dashed #bbb;
  padding: 80px 20px;
  min-height: 450px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #999;
  font-weight: 400;
  font-family: var(--font-mono);
}
.placeholder-icon {
  font-size: 5rem;
  margin-bottom: 20px;
  opacity: 0.4;
}
.placeholder-text {
  font-size: 1.1rem;
  text-transform: uppercase;
  letter-spacing: 2px;
}

.custom-scroll::-webkit-scrollbar {
  width: 4px;
}
.custom-scroll::-webkit-scrollbar-thumb {
  background: var(--ink);
}

@media (max-width: 1000px) {
  .split-layout {
    grid-template-columns: 1fr;
    gap: 30px;
  }
  .list-column {
    border-right: none;
    padding-right: 0;
  }
  .detail-column {
    padding-left: 0;
  }
  .notification-wrapper {
    width: 95%;
    padding: 24px;
  }
}
@media (max-width: 768px) {
  .notification-wrapper {
    padding: 20px;
  }
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  .header-right {
    text-align: left;
  }
  .notification-item {
    padding: 14px 12px;
  }
  .avatar-wrapper {
    width: 40px;
    height: 40px;
  }
  .item-icon {
    font-size: 1.1rem;
  }
  .detail-title {
    font-size: 1.8rem;
  }
  .detail-card {
    padding: 24px;
  }
}
@media (max-width: 480px) {
  .action-bar {
    flex-direction: column;
    gap: 10px;
  }
  .formal-btn.small {
    width: 100%;
  }
}
</style>