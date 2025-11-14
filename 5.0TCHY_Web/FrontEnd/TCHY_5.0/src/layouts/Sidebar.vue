<template>
  <div class="sidebar" :class="{ collapsed: isCollapsed }">
    <div class="sidebar-header">
      <div class="logo">
        <i class="fas fa-globe-asia"></i>
        <span class="logo-text">太初寰宇</span>
      </div>
      <button class="toggle-sidebar" @click="toggleSidebar">
        <i class="fas fa-bars"></i>
      </button>
    </div>
    
    <div class="sidebar-menu">
      <div class="menu-section">
        <div class="menu-title">主要内容</div>
        <router-link to="/admin" class="menu-item" exact-active-class="active">
          <i class="fas fa-home"></i>
          <span class="menu-text">首页</span>
        </router-link>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-info-circle"></i>
          <span class="menu-text">社区介绍</span>
        </router-link>
      </div>
      
      <div class="menu-section">
        <div class="menu-title">社区板块</div>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-paw"></i>
          <span class="menu-text">柴圈板块</span>
        </router-link>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-palette"></i>
          <span class="menu-text">绘画板块</span>
        </router-link>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-gamepad"></i>
          <span class="menu-text">游戏板块</span>
        </router-link>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-code"></i>
          <span class="menu-text">代码板块</span>
        </router-link>
      </div>
      
      <div class="menu-section">
        <div class="menu-title">用户中心</div>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-user"></i>
          <span class="menu-text">个人页面</span>
        </router-link>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-cog"></i>
          <span class="menu-text">设置</span>
        </router-link>
      </div>
      
      <div class="menu-section">
        <div class="menu-title">管理</div>
        <router-link to="/admin" class="menu-item" active-class="active">
          <i class="fas fa-shield-alt"></i>
          <span class="menu-text">管理员板块</span>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Sidebar',
  data() {
    return {
      isCollapsed: false
    }
  },
  methods: {
    toggleSidebar() {
      this.isCollapsed = !this.isCollapsed
    }
  }
}
</script>

<style scoped>
.sidebar {
  width: 260px;
  background: var(--dark);
  transition: all 0.3s ease;
  position: fixed;
  height: 100vh;
  overflow-y: auto;
  z-index: 100;
}

.sidebar.collapsed {
  width: 70px;
}

.sidebar-header {
  padding: 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--secondary);
}

.logo i {
  font-size: 1.8rem;
}

.toggle-sidebar {
  background: none;
  border: none;
  color: var(--light);
  font-size: 1.2rem;
  cursor: pointer;
}

.sidebar-menu {
  padding: 15px 0;
}

.menu-section {
  margin-bottom: 20px;
}

.menu-title {
  padding: 10px 20px;
  font-size: 0.85rem;
  color: var(--gray);
  text-transform: uppercase;
  letter-spacing: 1px;
}

.menu-item {
  display: flex;
  align-items: center;
  padding: 12px 20px;
  color: var(--light);
  text-decoration: none;
  transition: all 0.2s;
  border-left: 3px solid transparent;
}

.menu-item:hover, .menu-item.active {
  background: rgba(255, 255, 255, 0.05);
  border-left-color: var(--primary);
  color: var(--secondary);
}

.menu-item i {
  width: 24px;
  margin-right: 12px;
  font-size: 1.1rem;
}

.menu-text {
  white-space: nowrap;
  overflow: hidden;
}

.sidebar.collapsed .menu-text,
.sidebar.collapsed .menu-title {
  display: none;
}

@media (max-width: 768px) {
  .sidebar {
    width: 0;
  }
  
  .sidebar.collapsed {
    width: 70px;
  }
}
</style>
