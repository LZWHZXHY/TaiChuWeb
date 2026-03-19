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
          <a href="http://beian.miit.gov.cn" target="_blank" rel="noopener noreferrer">
            京ICP备2022020268号
          </a>
        </div>
      </footer>

      <UniversalPublisherModal 
        v-if="publisherStore.isOpen" 
        @close="publisherStore.close" 
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import HeaderNav from './layouts/HeaderNav.vue'
import { NAV_ITEMS } from './constants/navigation.js'
import MobileWarning from './views/MobileWarning.vue'

// ✅ 引入发布器组件与全局状态
import UniversalPublisherModal from '@/Publisher/UniversalPublisherModal.vue'
import { usePublisherStore } from '@/stores/publisher'

// 初始化发布状态仓库
const publisherStore = usePublisherStore()

// 导航配置映射
const navItems = ref(NAV_ITEMS)

/**
 * 导航切换回调
 */
const handleNavChange = (item) => { 
  console.log('📡 系统信号：切换至路由 ->', item.path) 
}

/**
 * 用户交互动作回调
 */
const handleUserAction = () => { 
  console.log('🛡️ 拦截到用户操作指令') 
}
</script>

<style scoped>
/* 保持原有布局结构 */
.BigContainer {
  width: 100%;
  height: 100vh;
  /* 建议：正式发布时可将此绿色改为更符合深色模式的背景 */
  background-color: rgb(54, 192, 107);
  display: flex;
  flex-direction: column;
}

#app {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  width: 100%;
}

.main-content {
  flex: 1;
  overflow-y: auto; /* 确保内容区可独立滚动 */
  width: 100%;
  padding: 0 0;
  background-color: #ffffff;
  margin-top: 72px; /* 对应 HeaderNav 的标准高度 */
}

.app-footer {
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  padding: 0.5rem 0;
  width: 100%;
  margin-top: auto; /* 将页脚固定在底部 */
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