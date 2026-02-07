<template>
  <div class="cyber-admin-container" :class="{ 'sidebar-collapsed': collapsed }">
    <div class="grid-bg moving-grid"></div>

    <header class="admin-bridge-header" role="banner">
      <div class="header-left">
        <div class="tech-tag">ADMIN_PROTOCOL_v5.0</div>
        <h1 class="giant-title">
          <span class="red-box"></span>
          COMMAND_CENTER <small>// 管理员后台</small>
        </h1>
      </div>

      <div class="header-right">
        <button
          class="cyber-tool-btn"
          :class="{ 'is-loading': loading }"
          @click="handleRefresh"
          :disabled="loading"
          title="SYNC_DATA"
        >
          <span class="btn-icon">
            <svg v-if="!loading" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
              <path d="M23 4v6h-6M1 20v-6h6M3.51 9a9 9 0 0114.13-3.36L23 4M20.49 15a9 9 0 01-14.13 3.36L1 20"></path>
            </svg>
            <div v-else class="tech-spinner"></div>
          </span>
          <span class="btn-text" v-if="!collapsed">REFRESH</span>
        </button>

        <button class="cyber-tool-btn toggle-btn" @click="toggleCollapsed" :title="collapsed ? 'EXPAND' : 'COLLAPSE'">
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="3">
            <path v-if="!collapsed" d="M4 6h16M4 12h10M4 18h16"></path>
            <path v-else d="M4 6h16M4 12h16M4 18h16"></path>
          </svg>
        </button>
      </div>
    </header>

    <div class="admin-main-bridge">
      <aside class="sidebar-deck">
        <div class="deck-label">// NAVIGATION_SYSTEM</div>
        <nav class="cyber-menu">
          <button
            v-for="item in items"
            :key="item.id"
            class="cyber-menu-item"
            :class="{ active: active === item.id }"
            @click="handleChange(item.id)"
            :aria-current="active === item.id ? 'page' : false"
          >
            <span class="item-indicator"></span>
            <span class="item-icon">{{ item.icon }}</span>
            <span v-if="!collapsed" class="item-label">{{ item.label }}</span>
            <span class="item-id-tag" v-if="!collapsed">{{ item.id.toUpperCase() }}</span>
          </button>
        </nav>
        
        <div class="sidebar-footer" v-if="!collapsed">
          <div class="footer-line">CORE_STATUS: STABLE</div>
          <div class="footer-line">ENCRYPTION: AES_256</div>
        </div>
      </aside>

      <main class="content-viewport custom-scroll" role="main">
        <div class="viewport-header" v-if="activeItem">
          <span class="path-root">TAICHU_HUB</span>
          <span class="path-sep">/</span>
          <span class="path-current">{{ activeItem.label.toUpperCase() }}</span>
        </div>
        <div class="slot-container">
          <slot />
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  active: { type: String, required: true }
})
const emit = defineEmits(['change', 'refresh'])

const collapsed = ref(false)
const loading = ref(false)
let refreshTimer = null

const items = [
  { id: 'review', label: '审核中心', icon: '🗂️'},
  { id: 'users', label: '用户管理', icon: '👤'},
  { id: 'notifications', label: '通知系统', icon: '🔔' },
  { id: 'reports', label: '举报处理', icon: '🚩', },
  { id: 'settings', label: '系统设置', icon: '⚙️'},
  { id: 'updates', label: '更新日志', icon: '📝'},
  { id: 'rules', label: '社区规则', icon: '📜' },
  { id: 'feedback', label: '意见箱', icon: '📫' },
  { id: 'calendar', label:'日历',icon:''}
]

const activeItem = computed(() => items.find(i => i.id === props.active))

function handleChange(id) {
  emit('change', id)
}

function toggleCollapsed() {
  collapsed.value = !collapsed.value
}

function handleRefresh() {
  if (loading.value) return
  loading.value = true
  emit('refresh')
  try {
    window.dispatchEvent(new CustomEvent('admin-refresh'))
  } catch (e) {}

  clearTimeout(refreshTimer)
  refreshTimer = setTimeout(() => {
    loading.value = false
  }, 700)
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Inter:wght@400;800&display=swap');

.cyber-admin-container {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  
  background: var(--off-white);
  min-height: 100vh;
  color: var(--black);
  font-family: 'Inter', sans-serif;
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* --- 背景网格 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

/* --- 顶部指令栏 --- */
.admin-bridge-header {
  height: 70px;
  background: var(--black);
  color: #fff;
  border-bottom: 4px solid var(--red);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 30px;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 10px 30px rgba(0,0,0,0.3);
}

.tech-tag {
  font-family: var(--mono);
  font-size: 0.65rem;
  color: var(--red);
  letter-spacing: 2px;
  margin-bottom: 2px;
}

.giant-title {
  margin: 0;
  font-family: var(--heading);
  font-size: 1.8rem;
  display: flex;
  align-items: center;
  gap: 12px;
  letter-spacing: 1px;
}
.giant-title small { font-family: 'Inter'; font-size: 0.8rem; opacity: 0.6; font-weight: 400; }
.red-box { width: 14px; height: 14px; background: var(--red); }

.header-right { display: flex; gap: 15px; }

/* --- 工业风格按钮 --- */
.cyber-tool-btn {
  background: transparent;
  border: 1px solid #444;
  color: #fff;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 15px;
  cursor: pointer;
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.8rem;
  transition: all 0.2s;
}
.cyber-tool-btn:hover { background: #222; border-color: var(--red); color: var(--red); }
.cyber-tool-btn.is-loading { color: var(--red); border-color: var(--red); }

.tech-spinner {
  width: 16px; height: 16px;
  border: 2px solid var(--red);
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { 100% { transform: rotate(360deg); } }

/* --- 侧栏面板 --- */
.admin-main-bridge {
  flex: 1;
  display: flex;
  padding: 20px;
  gap: 20px;
  z-index: 1;
  height: calc(100vh - 70px);
}

.sidebar-deck {
  width: 260px;
  background: #fff;
  border: 3px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
  display: flex;
  flex-direction: column;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  flex-shrink: 0;
}

.sidebar-collapsed .sidebar-deck { width: 80px; }

.deck-label {
  background: var(--black);
  color: #fff;
  font-family: var(--mono);
  font-size: 0.7rem;
  padding: 5px 12px;
  font-weight: 800;
}

.cyber-menu {
  flex: 1;
  padding: 10px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.cyber-menu-item {
  height: 50px;
  border: 1px solid #eee;
  background: #fff;
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 0 15px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  text-align: left;
}

.item-indicator {
  position: absolute; left: -3px; top: 20%; bottom: 20%;
  width: 6px; background: var(--black); transform: scaleY(0);
  transition: transform 0.2s;
}

.item-icon { font-size: 1.2rem; filter: grayscale(1); transition: 0.2s; }
.item-label { font-weight: 800; font-size: 0.9rem; flex: 1; color: #444; }
.item-id-tag { font-family: var(--mono); font-size: 0.6rem; opacity: 0.4; }

.cyber-menu-item:hover {
  background: var(--off-white);
  transform: translateX(5px);
  border-color: var(--black);
}

.cyber-menu-item.active {
  background: var(--black);
  border-color: var(--black);
  color: #fff;
  box-shadow: 4px 4px 0 var(--red);
}
.cyber-menu-item.active .item-label { color: #fff; }
.cyber-menu-item.active .item-icon { filter: grayscale(0); }
.cyber-menu-item.active .item-indicator { transform: scaleY(1); background: var(--red); }
.cyber-menu-item.active .item-id-tag { opacity: 0.8; color: var(--red); }

.sidebar-collapsed .cyber-menu-item { justify-content: center; padding: 0; }
.sidebar-collapsed .item-id-tag { display: none; }

.sidebar-footer {
  padding: 15px;
  border-top: 2px dashed var(--gray);
  font-family: var(--mono);
  font-size: 0.65rem;
  color: #888;
}

/* --- 内容视口 --- */
.content-viewport {
  flex: 1;
  background: #fff;
  border: 3px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  padding: 30px;
}

.viewport-header {
  margin-bottom: 25px;
  padding-bottom: 15px;
  border-bottom: 2px solid var(--black);
  font-family: var(--mono);
  font-weight: 700;
  display: flex;
  gap: 10px;
  font-size: 0.85rem;
}
.path-root { color: #888; }
.path-sep { color: var(--red); }
.path-current { color: var(--black); background: #eee; padding: 0 8px; }

.slot-container {
  flex: 1;
  animation: fadeIn 0.4s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 滚动条美化 */
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-track { background: var(--gray); }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); border: 2px solid var(--gray); }

@media (max-width: 1024px) {
  .admin-main-bridge { padding: 10px; gap: 10px; }
  .sidebar-deck { position: fixed; left: -300px; height: 80vh; z-index: 200; }
  /* 这里可以添加更复杂的移动端逻辑，或保持简单的样式调整 */
}
</style>