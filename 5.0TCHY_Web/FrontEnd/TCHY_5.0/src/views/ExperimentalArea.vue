<template>
  <div class="experiment-lab">
    <div class="lab-warning-strip">
      <span class="strip-text">/// WARNING: EXPERIMENTAL FEATURES // UNSTABLE ENVIRONMENT ///</span>
    </div>

    <div class="lab-container">
      <aside class="lab-sidebar">
        <div class="sidebar-header">
          <span class="header-icon">⚗️</span>
          <span class="header-title">LAB_MODULES</span>
        </div>

        <nav class="module-list">
          <div 
            v-for="item in experiments" 
            :key="item.id"
            class="module-item"
            :class="{ active: currentTab === item.id }"
            @click="currentTab = item.id"
          >
            <span class="bracket">[</span>
            <span class="module-name">{{ item.name }}</span>
            <span class="bracket">]</span>
            <span v-if="currentTab === item.id" class="indicator"><<</span>
          </div>
        </nav>

        <div class="lab-status">
          <div>ENV: DEV</div>
          <div>PING: 12ms</div>
        </div>
      </aside>

      <main class="lab-viewport">
        <Transition name="fade" mode="out-in">
          <component 
            :is="activeComponent" 
            v-if="activeComponent"
            class="experiment-content"
          />
          <div v-else class="empty-state">
            <div class="empty-icon">⚠️</div>
            <div class="empty-text">MODULE NOT FOUND OR PENDING...</div>
          </div>
        </Transition>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, markRaw } from 'vue';
import apiClient from '@/utils/api';

// 1. 引入你的实验组件
import Terminal from '@/ExperimentalComponents/Terminal.vue';
import LingMaiModule from '@/LingMaiComponents/LingMaiModule.vue';

// 2. 这里定义你的实验频道列表
// 使用 markRaw 避免 Vue 对组件本身进行深度响应式代理（性能优化）
const experiments = [
  { 
    id: 'terminal', 
    name: 'SYS_TERMINAL', 
    component: markRaw(Terminal) 
  },
  { 
    // 现在 LING_MAI_CORE 对应的是一个完整的系统
    id: 'lingmai', 
    name: 'LING_MAI_CORE', 
    component: markRaw(LingMaiModule) 
  }
];

// 当前选中的频道 ID
const currentTab = ref('terminal');

// 计算当前应该渲染哪个组件
const activeComponent = computed(() => {
  const found = experiments.find(e => e.id === currentTab.value);
  return found ? found.component : null;
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');

.experiment-lab {
  /* 填满视口，减去头部导航的高度(假设72px) */
  min-height: calc(100vh - 72px);
  margin-top: 72px; /* 给固定头部留位置 */
  background-color: #F0F0F0;
  font-family: 'JetBrains Mono', monospace;
  color: #111;
  display: flex;
  flex-direction: column;
}

/* 警告条 */
.lab-warning-strip {
  background: #FFCC00;
  color: #000;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 0;
  text-align: center;
  border-bottom: 2px solid #111;
  user-select: none;
}

.lab-container {
  flex: 1;
  display: flex;
  overflow: hidden; /* 防止内部滚动条溢出 */
}

/* --- 左侧边栏 --- */
.lab-sidebar {
  width: 240px;
  background: #E5E5E5;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  padding: 20px;
}

.sidebar-header {
  font-weight: 700;
  font-size: 14px;
  margin-bottom: 30px;
  border-bottom: 2px dashed #999;
  padding-bottom: 10px;
}

.module-item {
  cursor: pointer;
  padding: 10px 0;
  font-size: 13px;
  color: #666;
  display: flex;
  align-items: center;
  transition: all 0.2s;
}

.module-item .bracket { opacity: 0; margin: 0 4px; color: #D92323; }
.module-item .indicator { margin-left: auto; font-weight: bold; color: #D92323; }

.module-item:hover, .module-item.active {
  color: #111;
  font-weight: 700;
  background: rgba(0,0,0,0.05);
  padding-left: 10px;
}

.module-item:hover .bracket, .module-item.active .bracket {
  opacity: 1;
}

.lab-status {
  margin-top: auto;
  font-size: 10px;
  color: #999;
  border-top: 2px solid #ccc;
  padding-top: 10px;
}

/* --- 右侧视口 --- */
.lab-viewport {
  flex: 1;
  position: relative;
  background: #FAFAFA;
  overflow: auto; /* 让组件自己决定是否滚动 */
  padding: 20px;
  background-image: radial-gradient(#ccc 1px, transparent 1px);
  background-size: 20px 20px;
}

/* 占位空状态 */
.empty-state {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #999;
}
.empty-icon { font-size: 40px; margin-bottom: 10px; }

/* 简单的过渡动画 */
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>