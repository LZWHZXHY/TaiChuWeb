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
      <div 
        v-for="(item, index) in filteredNotifications" 
        :key="item.id || index" 
        class="notify-item"
      >
        <div class="item-avatar-box">
          <img :src="item.avatar" class="item-avatar" alt="User" loading="lazy">
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

        <div class="item-ref-box" v-if="item.refImage">
          <img :src="item.refImage" class="ref-img" alt="Ref" loading="lazy">
        </div>
      </div>
      
      <div v-if="filteredNotifications.length === 0" class="empty-state">
        <div class="empty-icon">/</div>
        <span>NO_DATA_MATCHED</span>
        <span class="empty-sub">没有找到相关信号</span>
      </div>
    </div>

    <div class="panel-footer">
      <div class="footer-info">
        <span class="footer-label">接收状态 // RECEPTION</span>
        <span class="footer-val">24ms / Synced</span>
      </div>
      
      <div class="progress-track">
        <div class="progress-bar" style="width: 92%">
          <div class="progress-glare"></div>
        </div>
      </div>
      
      <div class="bg-watermark bottom">DATA</div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const showFilterMenu = ref(false)
const currentFilter = ref('all')

const filterOptions = [
  { label: '全部消息', value: 'all' },
  { label: '粉丝', value: 'follow' },
  { label: '@我的', value: 'mention' },
  { label: '评论', value: 'comment' },
  { label: '喜欢', value: 'like' },
  { label: '推荐', value: 'recommend' },
  { label: '收藏', value: 'collect' }
]

const currentFilterLabel = computed(() => {
  const opt = filterOptions.find(o => o.value === currentFilter.value)
  return opt ? opt.label : '全部消息'
})

const toggleFilter = () => { showFilterMenu.value = !showFilterMenu.value }
const selectFilter = (opt) => { currentFilter.value = opt.value; showFilterMenu.value = false }
const closeDropdown = (e) => { if (!e.target.closest('.filter-wrapper')) showFilterMenu.value = false }

onMounted(() => document.addEventListener('click', closeDropdown))
onUnmounted(() => document.removeEventListener('click', closeDropdown))

// 模拟数据 (保持不变，为了节省篇幅省略部分重复内容，逻辑一致)
const allNotifications = ref([
  { type: 'like', user: '曹小样儿', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Felix', action: '赞了你的评论', content: '确实，这种风格很难得...', time: '10m ago', refImage: 'https://picsum.photos/id/12/80/80' },
  { type: 'mention', user: '机能风bot', avatar: 'https://api.dicebear.com/7.x/bottts/svg?seed=cyber', action: '在动态中@了你', content: '来看看这套新的UI设计方案 @峰峰子', time: '1h ago', refImage: 'https://picsum.photos/id/48/80/80' },
  { type: 'comment', user: '一棵小栗', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Aneka', action: '回复了你', content: '我也觉得那个组件很有意思！', time: '2h ago', refImage: 'https://picsum.photos/id/24/80/80' },
  { type: 'collect', user: '设计收藏夹', avatar: 'https://api.dicebear.com/7.x/identicon/svg?seed=collect', action: '收藏了你的作品', content: '', time: '3h ago', refImage: 'https://picsum.photos/id/55/80/80' },
  { type: 'system', user: '太初核心', avatar: '/favicon.ico', action: '权限变更通知', content: '您的账户权限已提升至 LV.2', time: '1d ago', refImage: '' },
  ...Array.from({ length: 5 }).map((_, i) => ({ type: 'like', user: `User_${8000 + i}`, avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${i}`, action: '收藏了你的作品', content: '', time: '2d ago', refImage: `https://picsum.photos/id/${60+i}/80/80` }))
])

const filteredNotifications = computed(() => {
  if (currentFilter.value === 'all') return allNotifications.value
  if (currentFilter.value === 'recommend') return allNotifications.value.filter(n => n.type === 'recommend' || n.type === 'system')
  return allNotifications.value.filter(n => n.type === currentFilter.value)
})

const getTypeIcon = (type) => {
  const map = { like: '❤', comment: '✎', system: '⚡', mention: '@', follow: '+', collect: '★', recommend: '☀' }
  return map[type] || '●'
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.cyber-notify-panel {
  /* [核心修改] 强制固定宽度，不再随内容伸缩 */
  width: 420px;
  height: 620px;
  
  /* 移动端兜底，防止在小屏幕上撑破 */
  max-width: 90vw; 

  background-color: #F4F1EA;
  border-radius: 24px;
  box-shadow: 0 16px 48px rgba(0,0,0,0.18), 0 4px 16px rgba(0,0,0,0.08);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  font-family: 'Noto Sans SC', sans-serif;
  position: relative;
  border: 1px solid rgba(0,0,0,0.05);
  /* 确保内边距不会撑大总宽度 */
  box-sizing: border-box; 
}

/* 水印 */
.bg-watermark {
  position: absolute;
  font-family: 'Roboto Mono', monospace;
  font-weight: 900;
  font-size: 80px;
  color: rgba(0,0,0,0.04);
  pointer-events: none;
  z-index: 0;
}
.bg-watermark:not(.bottom) { top: -15px; right: 20px; }
.bg-watermark.bottom { bottom: -15px; left: 10px; font-size: 60px; }

/* --- 头部 --- */
.panel-header {
  padding: 32px 32px 20px;
  position: relative;
  z-index: 2;
  border-bottom: 1px solid rgba(0,0,0,0.06);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0; /* 防止头部被压缩 */
}

.title-main {
  font-size: 24px;
  font-weight: 900;
  color: #1a1a1a;
  letter-spacing: -0.5px;
}

.count-pill {
  background: #000;
  color: #fff;
  padding: 4px 10px;
  border-radius: 14px;
  width: 50%;
  display: flex;
  gap: 6px;
  align-items: center;
  font-size: 11px;
  font-family: 'Roboto Mono';
  
}

.pill-val {
  background: #D35400;  
  padding: 5px;
  border-radius: 14px;
  color: #ffffff;
}
.title-sub {
  margin-top: 6px;
  font-size: 11px;
  font-family: 'Roboto Mono';
  color: #999;
  letter-spacing: 0.5px;
}

/* --- 筛选区 --- */
.header-actions { position: relative; }

.filter-btn {
  height: 42px;
  padding: 0 20px;
  background: #fff;
  border: 1px solid rgba(0,0,0,0.1);
  border-radius: 21px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  font-weight: 700;
  color: #333;
  transition: all 0.2s cubic-bezier(0.25, 0.8, 0.25, 1);
  box-shadow: 0 2px 8px rgba(0,0,0,0.03);
}

.filter-btn:hover {
  background: #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  transform: translateY(-1px);
}

.filter-btn.active {
  background: #000;
  color: #fff;
  border-color: #000;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}

.btn-arrow { font-size: 10px; opacity: 0.6; transition: transform 0.3s; }
.btn-arrow.rotated { transform: rotate(180deg); }

.filter-dropdown {
  position: absolute;
  top: calc(100% + 12px);
  right: 0;
  width: 160px;
  background: #fff;
  border: 1px solid rgba(0,0,0,0.08);
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.12);
  overflow: hidden;
  z-index: 100;
  padding: 8px;
}

.dropdown-item {
  padding: 10px 14px;
  font-size: 13px;
  color: #555;
  cursor: pointer;
  border-radius: 8px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: background 0.1s;
  margin-bottom: 2px;
}

.dropdown-item:hover { background: #F4F1EA; color: #000; font-weight: 600; }
.dropdown-item.selected { background: rgba(0,0,0,0.04); color: #000; font-weight: 700; }
.opt-check { font-size: 8px; color: #d35400; }

.scale-fade-enter-active, .scale-fade-leave-active { transition: all 0.25s cubic-bezier(0.2, 0.8, 0.2, 1); }
.scale-fade-enter-from, .scale-fade-leave-to { opacity: 0; transform: translateY(-8px) scale(0.96); }

/* --- 列表区 --- */
.notify-scroll-area {
  flex: 1;
  overflow-y: auto;
  padding: 12px 24px;
  /* 确保宽度包含 padding */
  width: 100%; 
  box-sizing: border-box;
}

/* 滚动条美化 */
.notify-scroll-area::-webkit-scrollbar { width: 4px; }
.notify-scroll-area::-webkit-scrollbar-thumb { background: #ddd; border-radius: 2px; }

.notify-item {
  /* 让卡片占满容器宽度 */
  width: 100%;
  box-sizing: border-box; 
  
  display: flex;
  padding: 18px;
  margin-bottom: 10px;
  border-radius: 12px;
  transition: all 0.2s ease;
  background: transparent;
  cursor: pointer;
  position: relative;
  gap: 16px; /* 控制子元素间距 */
}

.notify-item:hover {
  background: rgba(255,255,255,0.6);
  transform: translateX(4px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
}

.notify-item::before {
  content: ''; position: absolute; left: 4px; top: 50%; transform: translateY(-50%);
  width: 3px; height: 0; background: #d35400; border-radius: 2px; transition: 0.2s;
}
.notify-item:hover::before { height: 20px; }

.item-avatar-box {
  position: relative; width: 44px; height: 44px; flex-shrink: 0;
}
.item-avatar {
  width: 100%; height: 100%; border-radius: 50%; object-fit: cover; border: 2px solid #fff;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}
.type-badge {
  position: absolute; bottom: -5px; right: -5px; width: 20px; height: 20px;
  background: #1a1a1a; color: #fff; border-radius: 50%; font-size: 11px;
  display: flex; align-items: center; justify-content: center; border: 2px solid #F4F1EA;
}
.type-badge.like { background: #d35400; }
.type-badge.system { background: #2980b9; }
.type-badge.mention { background: #8e44ad; }
.type-badge.follow { background: #27ae60; }
.type-badge.collect { background: #f39c12; }

/* --- 消息内容自适应 --- */
.item-content {
  flex: 1; /* 关键：占满剩余空间 */
  min-width: 0; /* 关键：防止 Flex 子项被长文本撑大，允许截断 */
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.top-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2px; }
.user-name { font-size: 14px; font-weight: 700; color: #2c3e50; }
.time-stamp { font-size: 10px; font-family: 'Roboto Mono'; color: #aaa; flex-shrink: 0; margin-left: 8px; }

.action-row { font-size: 13px; color: #666; margin-bottom: 4px; }

.content-text {
  font-size: 12px; color: #888; background: rgba(0,0,0,0.03); padding: 6px 10px;
  border-radius: 6px; 
  font-family: 'Roboto Mono';
  
  /* 文本截断处理 */
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis;
  max-width: 100%;
}

.item-ref-box {
  width: 52px; height: 52px; flex-shrink: 0; 
  border-radius: 8px; overflow: hidden; border: 1px solid rgba(0,0,0,0.1);
}
.ref-img { width: 100%; height: 100%; object-fit: cover; }

.empty-state {
  height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center;
  color: #ccc; font-family: 'Roboto Mono'; font-size: 12px; gap: 4px;
}
.empty-icon { font-size: 24px; margin-bottom: 4px; opacity: 0.5; }
.empty-sub { font-size: 10px; opacity: 0.6; font-family: 'Noto Sans SC'; }

/* --- 底部 --- */
.panel-footer {
  flex-shrink: 0; height: 80px; background: rgba(0,0,0,0.03); padding: 0 32px;
  display: flex; flex-direction: column; justify-content: center; gap: 8px;
  position: relative; z-index: 1; border-top: 1px solid rgba(0,0,0,0.05);
}
.footer-info { display: flex; justify-content: space-between; font-family: 'Roboto Mono'; font-size: 11px; color: #666; }
.footer-val { font-weight: 700; color: #1a1a1a; }
.progress-track { width: 100%; height: 8px; background: rgba(0,0,0,0.06); border-radius: 4px; overflow: hidden; position: relative; }
.progress-bar { height: 100%; background: #1a1a1a; border-radius: 4px; position: relative; }
.progress-glare {
  position: absolute; top: 0; left: 0; bottom: 0; right: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent);
  animation: glare 2s infinite linear;
}
@keyframes glare { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }
</style>