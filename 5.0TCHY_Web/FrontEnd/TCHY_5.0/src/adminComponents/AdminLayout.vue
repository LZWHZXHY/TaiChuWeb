<template>
  <div class="admin">
    <!-- 顶部栏 -->
    <header class="admin-header">
      <div class="header-left">
        <h1 class="title">管理员后台</h1>
        <p class="sub">用于审核与管理站内内容</p>
      </div>
      <div class="header-right">
        <button class="btn ghost" type="button" @click="$emit('refresh')">刷新</button>
      </div>
    </header>

    <div class="admin-layout">
      <!-- 侧边导航 -->
      <aside class="sidebar" aria-label="管理员菜单">
        <nav class="menu">
          <button
            v-for="item in items"
            :key="item.id"
            class="menu-item"
            :class="{ active: active === item.id }"
            type="button"
            @click="$emit('change', item.id)"
          >
            <span class="icon">{{ item.icon }}</span>
            <span>{{ item.label }}</span>
          </button>
        </nav>
      </aside>

      <!-- 主内容插槽 -->
      <main class="content">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  active: { type: String, required: true }
})

const items = [
  { id: 'review', label: '审核中心', icon: '🗂️' },
  { id: 'users', label: '用户管理', icon: '👤' },
  { id: 'reports', label: '举报处理', icon: '🚩' },
  { id: 'settings', label: '系统设置', icon: '⚙️' }
]
</script>

<style scoped>
:root {
  --bg: #f6f8fb;
  --ink: #0f172a;
  --muted: #64748b;
  --card: #ffffff;
  --ring: #e6ebf3;
  --ring-2: #d6deea;
  --primary: #2563eb;
  --danger: #ef4444;
  --r-lg: 16px;
  --shadow-sm: 0 2px 10px rgba(2,6,23,.06);
}

.admin-header {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  gap: 12px;
  padding: 14px 18px;
  background: var(--card);
  border-bottom: 1px solid var(--ring);
  box-shadow: var(--shadow-sm);
}
.header-left .title { margin: 0; font-size: 20px; font-weight: 900; }
.header-left .sub { margin: 4px 0 0; color: var(--muted); font-size: 13px; }
.header-right { display: flex; gap: 8px; }

.admin-layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 16px;
  padding: 16px;
  align-items: start;
  min-height: calc(100vh - 70px);
}

.sidebar {
  position: sticky; top: 12px;
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm);
  padding: 10px;
  height: fit-content;
}

.menu { display: grid; gap: 6px; }
.menu-item {
  display: grid; grid-template-columns: 22px 1fr; gap: 10px; align-items: center;
  padding: 10px 12px; border-radius: 10px; border: 1px solid transparent;
  background: #fff; color: #0f172a; cursor: pointer; font-weight: 700;
  transition: background .15s, border-color .15s;
}
.menu-item:hover { background: #f7faff; border-color: var(--ring); }
.menu-item.active { background: #eef4ff; border-color: #d9e6ff; }

.content { display: flex; flex-direction: column; gap: 16px; }

@media (max-width: 1024px) {
  .admin-layout { grid-template-columns: 1fr; }
  .sidebar { position: static; }
}
</style>