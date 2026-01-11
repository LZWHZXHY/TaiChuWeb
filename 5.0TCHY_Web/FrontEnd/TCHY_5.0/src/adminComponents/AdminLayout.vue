<template>
  <div class="admin" :class="{ 'sidebar-collapsed': collapsed }">
    <!-- 顶部栏 -->
    <header class="admin-header" role="banner">
      <div class="header-left">
        <h1 class="title">管理员后台</h1>
        <p class="sub">用于审核与管理站内内容</p>
      </div>

      <div class="header-right">
        <!-- 刷新图标（现在有加载态、全局事件和防抖） -->
        <button
          class="icon-btn"
          type="button"
          @click="handleRefresh"
          :aria-label="loading ? '正在刷新' : '刷新数据'"
          :title="loading ? '正在刷新…' : '刷新页面数据'"
          :aria-busy="loading"
          :disabled="loading"
        >
          <template v-if="!loading">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M23 4v6h-6"></path>
              <path d="M1 20v-6h6"></path>
              <path d="M3.51 9a9 9 0 0114.13-3.36L23 4"></path>
              <path d="M20.49 15a9 9 0 01-14.13 3.36L1 20"></path>
            </svg>
          </template>

          <template v-else>
            <!-- 简单 spinner -->
            <svg class="spinner" viewBox="0 0 50 50" width="18" height="18" aria-hidden="true">
              <circle cx="25" cy="25" r="20" fill="none" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-dasharray="31.4 31.4"/>
            </svg>
          </template>
        </button>

        <!-- 折叠侧边栏按钮 -->
        <button
          class="icon-btn"
          type="button"
          @click="toggleCollapsed"
          :aria-pressed="collapsed"
          :title="collapsed ? '展开侧栏' : '折叠侧栏'"
        >
          <svg v-if="!collapsed" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M6 6h14M6 18h14M6 12h14"></path>
          </svg>
          <svg v-else viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M4 6h16M4 12h16M4 18h16"></path>
          </svg>
        </button>
      </div>
    </header>

    <div class="admin-layout">
      <!-- 侧边导航 -->
      <aside class="sidebar" aria-label="管理员菜单">
        <div class="sidebar-top">
          <nav class="menu" role="navigation" aria-label="主菜单">
            <button
              v-for="item in items"
              :key="item.id"
              class="menu-item"
              :class="{ active: active === item.id }"
              type="button"
              @click="handleChange(item.id)"
              :title="!collapsed ? '' : item.label"
              :aria-current="active === item.id ? 'page' : false"
            >
              <span class="icon" aria-hidden="true">{{ item.icon }}</span>
              <span v-if="!collapsed" class="label">{{ item.label }}</span>
            </button>
          </nav>
        </div>
      </aside>

      <!-- 主内容插槽 -->
      <main class="content" role="main">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  active: { type: String, required: true }
})
const emit = defineEmits(['change', 'refresh'])

// 可收起侧栏，改善窄屏或管理操作专注度
const collapsed = ref(false)
const loading = ref(false) // 刷新加载态
let refreshTimer = null

const items = [
  { id: 'review', label: '审核中心', icon: '🗂️'},
  { id: 'users', label: '用户管理', icon: '👤'},
  { id: 'notifications', label: '通知系统', icon: '🔔' }, // 建议换个图标
  { id: 'reports', label: '举报处理', icon: '🚩', },
  { id: 'settings', label: '系统设置', icon: '⚙️'},
  { id: 'updates', label: '更新日志', icon: '📝'}, // 建议换个图标
  { id: 'rules', label: '社区规则', icon: '📜' },    // 建议换个图标
  { id: 'feedback', label: '意见箱', icon: '📫' } // 建议换个图标
]

function handleChange(id) {
  emit('change', id)
}

function toggleCollapsed() {
  collapsed.value = !collapsed.value
}

/**
 * 刷新处理：
 * - 防抖：避免重复点击短时间内触发多次
 * - 发射组件事件 'refresh' 供父组件处理
 * - 也 dispatch 全局 window 事件 'admin-refresh'，确保没有绑定父监听时也能触发其他监听器
 * - 显示短暂 loading 反馈
 */
function handleRefresh() {
  if (loading.value) return

  // 防抖：短时间内只允许一次刷新
  loading.value = true
  emit('refresh') // 父组件处理刷新逻辑（如果有）
  try {
    window.dispatchEvent(new CustomEvent('admin-refresh')) // 全局备用触发
  } catch {
    // ignore
  }

  // 保持 loading 状态至少 700ms，提升反馈感
  clearTimeout(refreshTimer)
  refreshTimer = setTimeout(() => {
    loading.value = false
  }, 700)
}
</script>

<style scoped>
:root {
  --bg: #f6f8fb;
  --ink: #0f172a;
  --muted: #64748b;
  --card: #ffffff;
  --ring: #e6ebf3;
  --primary: #2563eb;
  --r-lg: 12px;
  --shadow-sm: 0 6px 24px rgba(2,6,23,.06);
}

/* 整体容器 */
.admin {
  background: var(--bg);
  min-height: 100vh;
  color: var(--ink);
}

/* 头部 */
.admin-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 14px 20px;
  background: linear-gradient(90deg, #ffffff, #f7fbff);
  border-bottom: 1px solid var(--ring);
  box-shadow: var(--shadow-sm);
  position: sticky;
  top: 0;
  z-index: 40;
  width: 85%;
}
.header-left { display:flex; flex-direction:column; gap:4px; }
.title { margin: 0; font-size: 20px; font-weight: 900; letter-spacing: .2px; }
.sub { margin: 0; color: var(--muted); font-size: 13px; }

/* 头部右侧小图标按钮 */
.header-right { display:flex; gap:8px; align-items:center; }
.icon-btn {
  appearance: none;
  display: inline-grid;
  place-items: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  border: 1px solid transparent;
  background: white;
  cursor: pointer;
  color: var(--primary);
  transition: background .12s ease, transform .08s ease, box-shadow .12s;
}
.icon-btn:hover { background: #f0f6ff; transform: translateY(-1px); box-shadow: 0 6px 18px rgba(37,99,235,.06); }

/* spinner 动画 */
.spinner {
  animation: spin 1s linear infinite;
}
@keyframes spin { 100% { transform: rotate(360deg); } }

/* 布局：侧边栏 + 内容 */
.admin-layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 18px;
  padding: 18px;
}
.sidebar {
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: 12px;
  box-shadow: var(--shadow-sm);
  padding: 10px;
  height: fit-content;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

/* 当折叠时把侧边栏变窄，仅显示图标 */
.sidebar-collapsed .admin-layout {
  grid-template-columns: 76px 1fr;
}
.sidebar-collapsed .sidebar {
  padding: 8px 6px;
  width: 76px;
  overflow: visible;
}
.sidebar-collapsed .menu-item { justify-content: center; padding: 10px 6px; }
.sidebar-collapsed .menu-item .label { display: none; }

/* 菜单 */
.menu { display: grid; gap: 8px; }
.menu-item {
  display: flex;
  gap: 10px;
  align-items: center;
  padding: 10px 12px;
  border-radius: 10px;
  background: #fff;
  border: 1px solid transparent;
  font-weight: 800;
  color: var(--ink);
  cursor: pointer;
  transition: background .12s, border-color .12s, transform .06s;
}
.menu-item .icon { font-size: 18px; line-height: 1; width: 22px; text-align: center; }
.menu-item .label { font-size: 14px; }

.menu-item:hover { background: #f7faff; border-color: var(--ring); transform: translateY(-1px); }
.menu-item.active { background: linear-gradient(90deg,#eef4ff,#f7fbff); border-color: #dbe9ff; box-shadow: 0 6px 20px rgba(37,99,235,.04); color: var(--primary); }

/* 主内容 */
.content {
  min-height: 60vh;
  background: transparent;
}

/* 小屏优化 */
@media (max-width: 960px) {
  .admin-layout { grid-template-columns: 1fr; padding: 12px; }
  .sidebar { position: static; width: 100%; }
  .sidebar-collapsed .admin-layout { grid-template-columns: 1fr; }
  .header-right .icon-btn { width: 40px; height: 40px; }
}
</style>