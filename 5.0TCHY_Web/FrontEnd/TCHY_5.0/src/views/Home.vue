<template>
  <div class="settings-panel">
    <!-- 面板内容 -->
    <div class="panel-content">
      <!-- 侧边导航 -->
      <nav class="settings-nav">
        <div class="nav-section">
          <h3 class="section-title">信息分类</h3>
          <ul class="nav-list">
            <li
              v-for="item in navItems"
              :key="item.id"
              class="nav-item"
              :class="{ 'nav-item--active': activeTab === item.id }"
              @click="setActiveTab(item.id)"
            >
              <span class="nav-text">{{ item.name }}</span>
            </li>
          </ul>
        </div>
      </nav>

      <!-- 内容区域 -->
      <main class="settings-main">
        <!-- 社区介绍 -->
        <section v-if="activeTab === 'intro'" class="content-section">
          
          <div class="content-body">
            <CommunityIntro />
          </div>
        </section>

        <!-- 社区规则 -->
        <section v-if="activeTab === 'rules'" class="content-section">
          <div class="content-header">
            <h3>社区规则</h3>
            <p>请遵守以下规则，共同维护良好社区环境</p>
          </div>
          <div class="content-body">
             <div class="content-body">
            <CommunityRules />
          </div>
          </div>
        </section>

        <!-- 关于我们 -->
        <section v-if="activeTab === 'about'" class="content-section">
          <div class="content-header">
            <h3>关于我们</h3>
            <p>了解太初寰宇团队的使命和愿景</p>
          </div>
          <div class="content-body">
            <!-- 内容区域 - 等待填充 -->
            <div class="content-placeholder">
              <p>关于我们内容将在这里显示</p>
            </div>
          </div>
        </section>

        <!-- 财政记录 -->
        <section v-if="activeTab === 'finance'" class="content-section">
          <div class="content-header">
            <h3>财政记录</h3>
            <p>平台运营收支透明公开</p>
          </div>
          <div class="content-body">
            <!-- 内容区域 - 等待填充 -->
            <div class="content-placeholder">
              <p>财政记录内容将在这里显示</p>
            </div>
          </div>
        </section>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import CommunityIntro from '@/homeComponents/CommunityIntro.vue'
import CommunityRules from '@/homeComponents/CommunityRules.vue'

// 导航项配置
const navItems = [
  { id: 'intro', name: '社区介绍' },
  { id: 'rules', name: '社区规则' },
  { id: 'about', name: '关于我们' },
  { id: 'finance', name: '财政记录' }
]

const activeTab = ref('intro')

const setActiveTab = (tabId) => {
  activeTab.value = tabId
}
</script>

<style scoped>
.settings-panel {
  width: 100%;
  margin: 0;
  background: white;
  border-radius: 0;
  box-shadow: none;
  overflow: hidden;
  min-height: 100vh;
}

/* 面板内容布局 */
.panel-content {
  display: grid;
  grid-template-columns: minmax(250px, 300px) 1fr; /* 修复：使用minmax限制侧边栏 */
  min-height: calc(100vh - 200px);

  width: 100%; 
  margin-left: -50vw; /* 新增：居中修正 */
  left: 50%; /* 新增：居中修正 */
  position: relative;
}

/* 导航侧边栏 */
.settings-nav {
  background: #f8f9fa;
  border-right: 1px solid #e0e0e0;
  padding: 2rem;
  height: 100%;
  width: 100%;
  box-sizing: border-box;
}

.section-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 1.5rem;
  padding: 0;
}

.nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 1.5rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  color: #555;
  margin-bottom: 0.75rem;
  font-size: 1.2rem;
  font-weight: 500;
  border: 1px solid transparent;
  width: 100%;
  box-sizing: border-box;
}

.nav-item:hover {
  background: #e9ecef;
  color: #333;
  border-color: #d0d7de;
}

.nav-item--active {
  background: #007bff;
  color: white;
  border-color: #007bff;
  box-shadow: 0 2px 12px rgba(0, 123, 255, 0.3);
}

.nav-text {
  flex: 1;
  font-weight: 500;
}

/* 主内容区域 - 关键修复 */
.settings-main {
  padding: 3rem;
  background: rgb(255, 255, 255);
  min-height: 100%;
  width: 100%;
  overflow-x: hidden;
  box-sizing: border-box;
}

.content-section {
  animation: fadeIn 0.3s ease;
  height: 100%;
  display: flex;
  flex-direction: column;
  width: 100%;
  max-width: 100%;
}

.content-header {
  margin-bottom: 3rem;
  padding-bottom: 2rem;
  border-bottom: 2px solid #f0f0f0;
  width: 100%;
  max-width: 100%;
}

.content-header h3 {
  font-size: 2.2rem;
  font-weight: 600;
  color: #333;
  margin: 0 0 1rem 0;
  word-wrap: break-word;
}

.content-header p {
  color: #666;
  margin: 0;
  font-size: 1.3rem;
  font-weight: 400;
  word-wrap: break-word;
}

.content-body {
  flex: 1;
  min-height: 500px;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow: hidden; /* 新增：防止内部内容溢出 */
}

.content-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 500px;
  background: #f8f9fa;
  border-radius: 8px;
  color: #999;
  font-size: 1.5rem;
  border: 2px dashed #ddd;
  font-weight: 500;
  width: 100%;
  box-sizing: border-box;
}

/* 确保社区介绍组件正确适应 */
.content-body > * {
  width: 100% !important;
  max-width: 100% !important;
  box-sizing: border-box !important;
}

/* 动画 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 1k分辨率特别优化 (1024px) */
@media (max-width: 1200px) {
  .panel-content {
    grid-template-columns: minmax(220px, 250px) 1fr;
    width: 100vw;
  }
  
  .settings-nav {
    padding: 1.5rem;
  }
  
  .nav-item {
    padding: 1.25rem 1.5rem;
    font-size: 1.1rem;
  }
  
  .settings-main {
    padding: 2rem;
    max-width: calc(100vw - 250px);
  }
  
  .content-header h3 {
    font-size: 2rem;
  }
}



</style>