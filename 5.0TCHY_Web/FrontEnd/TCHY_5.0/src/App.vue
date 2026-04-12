<!--已整理-->
<template>
  <div class="BigContainer">
    <div id="app">
      <HeaderNav
        :show-cta="true"
        
      />

      <MobileWarning />

      <main class="main-content">
        <router-view />
      </main>
      
      <footer class="app-footer fixed-footer">
        <div class="footer-content">
          <span>太初寰宇  © 2025</span>
          <a href="http://beian.miit.gov.cn" target="_blank" rel="noopener noreferrer">
            京ICP备2022020268号
          </a>
        </div>
      </footer>

      <UltraPannel />   <!--新版超级终端 还在开发阶段-->



    </div>
  </div> 
</template>

<script setup>
import { ref } from 'vue'
import HeaderNav from './layouts/HeaderNav.vue'
import MobileWarning from './views/MobileWarning.vue'
import UltraPannel from './GeneralComponents/UltraPannel.vue'


</script>

<style scoped>
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


/* 假设手机端的阈值是 768px    在这个模式下，直接隐藏整个头部导航栏*/
@media (max-width: 768px) {
  /* 1. 直接选定 HeaderNav 组件对应的标签或类名进行隐藏 */
  header, 
  .header-nav-container { 
    display: none !important; 
  }

  /* 2. 关键：把内容区的顶部间距清零，否则顶部会有一块 72px 的空白 */
  .main-content {
    margin-top: 0 !important;
  }
}


</style>