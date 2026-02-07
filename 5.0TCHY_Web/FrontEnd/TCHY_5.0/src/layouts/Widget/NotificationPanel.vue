<template>
  <div class="cyber-notify-panel">
    <div class="bg-watermark">SIGNAL</div>

    <div class="panel-header">
      <div class="header-content">
        <div class="title-row">
          <span class="title-main">互动信号</span>
          <div class="count-pill">
            <span class="pill-label">LOGS</span>
            <span class="pill-val">{{ filteredNotifications.length }}</span>
          </div>
        </div>
        <div class="title-sub">SIGNAL LOG // SYNCHRONIZED</div>
      </div>

      <div class="header-actions">
        <div class="filter-wrapper">
          <button 
            class="filter-btn" 
            @click.stop="toggleFilter"
            :class="{ 'active': showFilterMenu }"
          >
            <span class="btn-text">{{ currentFilterLabel }}</span>
            <span class="btn-arrow" :class="{ 'rotated': showFilterMenu }">▼</span>
          </button>

          <transition name="scale-fade">
            <div v-if="showFilterMenu" class="filter-dropdown">
              <div class="dropdown-list">
                <div 
                  v-for="opt in filterOptions" 
                  :key="opt.value"
                  class="dropdown-item"
                  :class="{ 'selected': currentFilter === opt.value }"
                  @click="selectFilter(opt)"
                >
                  <span class="opt-label">{{ opt.label }}</span>
                  <span v-if="currentFilter === opt.value" class="opt-check">●</span>
                </div>
              </div>
            </div>
          </transition>
        </div>
      </div>
    </div>

    <div class="notify-scroll-area">
      <div v-if="loading" class="empty-state">
        <div class="loading-spinner"></div>
        <span>SYNCING_DATA...</span>
      </div>

      <div 
        v-else
        v-for="(item, index) in filteredNotifications" 
        :key="item.id || index" 
        class="notify-item"
        :class="{ 'is-read': item.isRead }"
        @click="handleItemClick(item)"
      >
        <div class="item-avatar-box">
          <img :src="item.avatar" class="item-avatar" alt="User" loading="lazy">
          <div v-if="!item.isRead" class="unread-signal"></div>
          <div class="type-badge" :class="item.type">
            {{ getTypeIcon(item.type) }}
          </div>
        </div>

        <div class="item-content">
          <div class="top-row">
            <span class="user-name">{{ item.user }}</span>
            <span class="time-stamp">{{ item.time }}</span>
          </div>
          <div class="action-row">
            <span class="action-desc">{{ item.action }}</span>
          </div>
          <div v-if="item.content" class="content-text">
            "{{ item.content }}"
          </div>
        </div>
      </div>
      
      <div v-if="!loading && filteredNotifications.length === 0" class="empty-state">
        <div class="empty-icon">/</div>
        <span>NO_DATA_MATCHED</span>
        <span class="empty-sub">没有找到相关信号</span>
      </div>
    </div>

    <div class="panel-footer">
      <div class="footer-info">
        <span class="footer-label">接收状态 // RECEPTION</span>
        <span class="footer-val">{{ loading ? 'Syncing...' : '24ms / Synced' }}</span>
      </div>
      <div class="progress-track">
        <div class="progress-bar" :style="{ width: loading ? '40%' : '100%' }"></div>
      </div>
      <div class="bg-watermark bottom">DATA</div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router' // 引入路由
import apiClient from '@/utils/api' 

const router = useRouter()
// 定义向父组件发送的事件：
// update-count: 通知 HeaderNav 更新红点
// item-click: 通知 HeaderNav 关闭面板
const emit = defineEmits(['update-count', 'item-click'])

// --- 状态定义 ---
const showFilterMenu = ref(false)
const currentFilter = ref('all')
const allNotifications = ref([])
const loading = ref(false)

const filterOptions = [
  { label: '全部消息', value: 'all' },
  { label: '评论', value: 'comment' },
  { label: '点赞', value: 'like' },
  { label: '系统', value: 'system' }
]

// --- 文本生成 ---
const generateActionText = (cat, act, title) => {
  if (cat === 4) return '发布了全员系统通告'
  
  const actions = { 1: '赞了', 2: '评论了', 3: '收藏了' }
  const categories = { 1: '帖子', 2: '画廊作品', 3: '博客文章' }
  
  const actionStr = actions[act] || '交互了'
  const catStr = categories[cat] || '内容'
  
  // 如果有标题，显示标题；否则显示泛指
  if (title) {
      const safeTitle = title.length > 10 ? title.substring(0, 10) + '...' : title
      return `${actionStr}：${safeTitle}`
  } else {
      return `${actionStr}你的${catStr}`
  }
}

const mapNotificationType = (cat, act) => {
  if (cat === 4) return 'system'
  if (act === 1) return 'like'
  if (act === 2) return 'comment'
  return 'collect' // 默认归类
}

const formatRelativeTime = (dateStr) => {
  const date = new Date(dateStr)
  const diff = (new Date() - date) / 1000
  if (diff < 60) return `${Math.floor(diff)}s ago`
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`
  return date.toLocaleDateString()
}

// --- 核心：获取数据 ---
const fetchNotifications = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/Notification/mine')
    if (res.data.success) {
      allNotifications.value = res.data.data.items.map(i => ({
        id: i.Id, 
        category: i.Category, // 1:帖子, 2:画廊, 3:博客, 4:系统
        targetId: i.TargetId, // 这里的 TargetId 就是 PostId (例如 82)
        
        type: mapNotificationType(i.Category, i.ActionType),
        user: i.SenderName || '系统用户', 
        avatar: i.SenderLogo || '/favicon.ico', 
        action: generateActionText(i.Category, i.ActionType, i.TargetTitle), // 包含标题的文案
        content: i.Content,
        time: formatRelativeTime(i.CreatedAt),
        isRead: i.IsRead
      }))
    }
  } catch (error) {
    console.error("信号同步失败 //", error)
  } finally {
    loading.value = false
  }
}

// --- 核心：点击跳转逻辑 ---
const handleItemClick = async (item) => {
  // 1. 标记已读
  if (!item.isRead) {
    try {
      await apiClient.patch(`/Notification/${item.id}/read`)
      item.isRead = true
      emit('update-count') // 刷新导航栏红点
    } catch (err) { console.error(err) }
  }

  // 2. 路由跳转
  if (item.targetId) {
    switch (item.category) {
      case 1: // 帖子 -> /post/82
        await router.push(`/post/${item.targetId}`)
        break
      case 2: // 画廊 -> /gallery/82
        await router.push(`/gallery/${item.targetId}`)
        break
      case 3: // 博客 -> 假设跳到 post
        await router.push(`/blog/${item.targetId}`)
        break
      case 4: // 系统通知 -> 推送大厅
        await router.push('/MainPush')
        break
    }
  }

  // 3. 关闭面板
  emit('item-click')
}

// --- UI 逻辑 ---
const toggleFilter = () => { showFilterMenu.value = !showFilterMenu.value }
const selectFilter = (opt) => { currentFilter.value = opt.value; showFilterMenu.value = false }
const closeDropdown = (e) => { if (!e.target.closest('.filter-wrapper')) showFilterMenu.value = false }

const filteredNotifications = computed(() => {
  if (currentFilter.value === 'all') return allNotifications.value
  return allNotifications.value.filter(n => n.type === currentFilter.value)
})

const currentFilterLabel = computed(() => {
  return filterOptions.find(o => o.value === currentFilter.value)?.label || '全部消息'
})

const getTypeIcon = (t) => ({ like: '❤', comment: '✎', system: '⚡', collect: '★' }[t] || '●')

onMounted(() => {
  fetchNotifications()
  document.addEventListener('click', closeDropdown)
})

onUnmounted(() => {
  document.removeEventListener('click', closeDropdown)
})
</script>

<style scoped>
/* 机能风 CSS 样式 (保持不变) */
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;900&display=swap');

.cyber-notify-panel {
  width: 420px; height: 620px; max-width: 90vw;
  background-color: #F4F1EA; border-radius: 24px;
  display: flex; flex-direction: column; overflow: hidden;
  font-family: 'Noto Sans SC', sans-serif; position: relative;
  box-shadow: 0 16px 48px rgba(0,0,0,0.15); border: 1px solid rgba(0,0,0,0.05);
}

.bg-watermark {
  position: absolute; font-family: 'Roboto Mono'; font-weight: 900;
  font-size: 80px; color: rgba(0,0,0,0.04); pointer-events: none;
}
.bg-watermark:not(.bottom) { top: -15px; right: 20px; }
.bg-watermark.bottom { bottom: -15px; left: 10px; font-size: 60px; }

.panel-header {
  padding: 32px 32px 20px; border-bottom: 1px solid rgba(0,0,0,0.06);
  display: flex; justify-content: space-between; align-items: center; z-index: 2;
}

.title-main { font-size: 24px; font-weight: 900; color: #1a1a1a; }
.count-pill {
  background: #000; color: #fff; padding: 4px 10px; border-radius: 14px;
  display: flex; gap: 6px; font-size: 11px; font-family: 'Roboto Mono'; margin-top: 4px;
}
.pill-val { background: #D35400; padding: 0 6px; border-radius: 10px; }

.filter-btn {
  height: 40px; padding: 0 16px; background: #fff; border: 1px solid #ddd;
  border-radius: 20px; cursor: pointer; font-weight: 700; display: flex; align-items: center; gap: 8px;
}
.filter-btn.active { background: #000; color: #fff; border-color: #000; }

.filter-dropdown {
  position: absolute; top: 50px; right: 0; width: 150px; background: #fff;
  border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,0.1); padding: 8px; z-index: 10;
}
.dropdown-item {
  padding: 8px 12px; font-size: 13px; cursor: pointer; border-radius: 6px;
}
.dropdown-item:hover { background: #f0f0f0; }

.notify-scroll-area { flex: 1; overflow-y: auto; padding: 10px 24px; }
.notify-item {
  display: flex; padding: 16px; margin-bottom: 8px; border-radius: 12px;
  background: rgba(255,255,255,0.4); cursor: pointer; transition: 0.2s; gap: 12px;
}
.notify-item:hover { background: #fff; transform: translateX(5px); }
.notify-item.is-read { opacity: 0.5; filter: grayscale(0.5); }

.item-avatar-box { position: relative; width: 44px; height: 44px; flex-shrink: 0; }
.item-avatar { width: 100%; height: 100%; border-radius: 50%; border: 2px solid #fff; }

.unread-signal {
  position: absolute; top: -2px; left: -2px; width: 10px; height: 10px;
  background: #D35400; border-radius: 50%; box-shadow: 0 0 8px #D35400;
  animation: pulse 1.5s infinite;
}
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

.type-badge {
  position: absolute; bottom: -4px; right: -4px; width: 18px; height: 18px;
  background: #1a1a1a; color: #fff; border-radius: 50%; font-size: 10px;
  display: flex; align-items: center; justify-content: center;
}
.type-badge.like { background: #D35400; }
.type-badge.system { background: #2980b9; }

.item-content { flex: 1; min-width: 0; }
.user-name { font-size: 14px; font-weight: 700; }
.time-stamp { font-size: 10px; color: #999; float: right; }
.action-desc { font-size: 12px; color: #666; margin: 4px 0; }
.content-text {
  font-size: 11px; color: #888; background: rgba(0,0,0,0.03);
  padding: 4px 8px; border-radius: 4px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}

.panel-footer {
  height: 70px; background: rgba(0,0,0,0.02); padding: 0 32px;
  display: flex; flex-direction: column; justify-content: center; gap: 6px;
}
.footer-info { display: flex; justify-content: space-between; font-size: 10px; color: #666; font-family: 'Roboto Mono'; }
.progress-track { height: 6px; background: #eee; border-radius: 3px; overflow: hidden; }
.progress-bar { height: 100%; background: #1a1a1a; transition: 0.3s; }

.empty-state {
  height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #bbb;
}
.loading-spinner {
  width: 20px; height: 20px; border: 2px solid #ddd; border-top-color: #D35400; border-radius: 50%; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>