<template>
  <div class="BigContainer">
    <div id="app">

      <HeaderNav
        :nav-items="navItems"
        :show-cta="true"
        @nav-change="handleNavChange"
        @user-action="handleUserAction"
      />

      <MobileWarning />

      <main class="main-content">
        <router-view />
      </main>

      <footer class="app-footer fixed-footer">
        <div class="footer-content">
          <span>{{ $t('HeaderNav.site_name') }} © 2025</span>
          
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
import MobileWarning from './views/MobileWarning.vue'

// 只要 navigation.js 里的 name 改成了 'nav.home' 这种 Key，
// 这里直接传给子组件就行，不需要额外处理
const navItems = ref(NAV_ITEMS)

const handleNavChange = (item) => { console.log('导航切换:', item) }
const handleUserAction = () => { console.log('用户操作') }
</script>

<style scoped>
/* 样式保持完全不变 */
.BigContainer {
  width: 100%;
  height: 100vh;
  background-color: rgb(54, 192, 107);
  display: flex;
  flex-direction: column;
}

#app {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0; /* 防止内容溢出 */
  width: 100%;
}

.main-content {
  flex: 1;
  overflow-y: auto; /* 主内容可滚动 */
  width: 100%;
  padding: 20px 0;
  background-color: #ffffff;
  margin-top: 70px; /* Header高度 */
}

.app-footer {
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  padding: 1.5rem 0;
  width: 100%;
  margin-top: auto; /* 关键：将footer推到最下面 */
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
</style>