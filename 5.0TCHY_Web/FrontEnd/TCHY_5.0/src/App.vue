<template>
  <div id="app">
    <HeaderNav 
      :nav-items="navItems"
      :show-cta="true"
      @nav-change="handleNavChange"
      @user-action="handleUserAction"
    />
    <main class="main-content">
      <router-view />
    </main>
    
    <!-- 固定在底部的备案信息 -->
    <footer class="app-footer fixed-footer">
      <div class="footer-content">
        <span>太初寰宇 © 2025</span>
        <a href="http://beian.miit.gov.cn" target="_blank" rel="noopener noreferrer">
          京ICP备2022020268号
        </a>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import HeaderNav from './layouts/HeaderNav.vue'
import { NAV_ITEMS } from './constants/navigation.js'

const navItems = ref(NAV_ITEMS)

const handleNavChange = (item) => {
  console.log('导航切换:', item)
}

const handleUserAction = () => {
  console.log('用户操作')
}
</script>

<style>
.main-content {
  margin-top: 70px;
  padding-bottom: 50px; /* 为固定footer留出空间 */
}

body {
  margin: 0;
  font-family: 'Segoe UI', system-ui, sans-serif;
  background-color: #f8f9fa;
}

#app {
  min-height: 100vh;
  position: relative;
}

/* 固定在底部的备案信息样式 */
.fixed-footer {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: rgba(255, 255, 255, 0.95);
  border-top: 1px solid #e0e0e0;
  padding: 0.75rem 0;
  backdrop-filter: blur(10px);
  z-index: 100;
}

.fixed-footer .footer-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.8rem;
  color: #666;
}

.fixed-footer .footer-content a {
  color: #666;
  text-decoration: none;
  transition: color 0.3s ease;
}

.fixed-footer .footer-content a:hover {
  color: #007bff;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .fixed-footer .footer-content {
    padding: 0 1rem;
    flex-direction: column;
    gap: 0.25rem;
    text-align: center;
  }
  
  .main-content {
    padding-bottom: 70px; /* 移动端需要更多空间 */
  }
}
</style>