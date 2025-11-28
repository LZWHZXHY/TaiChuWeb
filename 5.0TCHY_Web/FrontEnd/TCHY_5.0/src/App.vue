<template>
  <div class="BigContainer">
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

      <footer class="app-footer fixed-footer">
        <div class="footer-content">
          <span>太初寰宇 © 2025</span>
          <a href="http://beian.miit.gov.cn" target="_blank" rel="noopener noreferrer">京ICP备2022020268号</a>
        </div>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import HeaderNav from './layouts/HeaderNav.vue'
import { NAV_ITEMS } from './constants/navigation.js'

const navItems = ref(NAV_ITEMS)

const handleNavChange = (item) => { console.log('导航切换:', item) }
const handleUserAction = () => { console.log('用户操作') }
</script>

<style scoped>
/* Footer 样式 */
.app-footer {
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  padding: 1.5rem 0;
  margin-top: auto;
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  color: #64748b;
}

.footer-content a {
  color: #64748b;
  text-decoration: none;
  transition: color 0.2s ease;
}

.footer-content a:hover {
  color: #334155;
}

/* 固定底部样式 */
.fixed-footer {
  position: relative;
  width: 100%;
  bottom: 0;
}

.main-content {
  width: 100%;
  margin-top: 70px;
  padding-bottom: 50px;
}

#app {
  min-height: 100%;
  position: relative;
  width: 100%;
}

/* 小屏适配 */
@media (max-width: 768px) {
  .footer-content {
    flex-direction: column;
    gap: 0.5rem;
    text-align: center;
  }
}

.main-content {
  min-height: calc(100vh - 200px);
  padding-bottom: 2rem;
}
</style>