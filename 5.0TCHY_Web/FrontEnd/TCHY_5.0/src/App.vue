<template>
  <div class="app-container">
    <!-- 侧边栏组件 -->
    <Sidebar />
    
    <!-- 主内容区域 -->
    <div class="main-content">
      <!-- 顶部导航组件 -->
      <TopNav @toggle-auth-modal="toggleAuthModal" />
      
      <!-- 路由视图 -->
      <router-view />
    </div>
    
    <!-- 登录/注册模态框组件 -->
    <AuthModal  :isActive="isAuthModalActive"  @update:isActive="isAuthModalActive = $event" />
  </div>
</template>



<script>
import Sidebar from './layouts/Sidebar.vue';
import TopNav from './layouts/TopNav.vue';
import AuthModal from './layouts/AuthModal.vue';

export default {
  name: 'App',
  components: {
    Sidebar,
    TopNav,
    AuthModal
  },
  data() {
    return {
      isSidebarActive: false,
      isAuthModalActive: false,
    };
  },
  methods: {
    toggleSidebar() {
      this.isSidebarActive = !this.isSidebarActive;
    },
    toggleAuthModal() {
      this.isAuthModalActive = !this.isAuthModalActive;
    },
  },
};


</script>

<style>
/* 全局样式 */
:root {
  --primary: #6c5ce7;
  --primary-dark: #5649c0;
  --secondary: #00cec9;
  --dark: #2d3436;
  --darker: #1e2224;
  --light: #f5f6fa;
  --gray: #636e72;
  --success: #00b894;
  --warning: #fdcb6e;
  --danger: #e17055;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif;
}

body {
  background-color: var(--darker);
  color: var(--light);
  line-height: 1.6;
}

/* 布局容器 */
.app-container {
  display: flex;
  min-height: 100vh;
}

/* 主内容区域 */
.main-content {
  flex: 1;
  margin-left: 260px;
  transition: margin-left 0.3s ease;
}

.sidebar.collapsed ~ .main-content {
  margin-left: 70px;
}

/* 响应式设计 */
@media (max-width: 992px) {
  .main-content {
    margin-left: 70px;
  }
}

@media (max-width: 768px) {
  .main-content {
    margin-left: 0;
  }
}
</style>
