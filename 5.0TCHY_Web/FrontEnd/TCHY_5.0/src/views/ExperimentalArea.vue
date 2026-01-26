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
import { ref, computed, markRaw, onMounted, watch } from 'vue';
import apiClient from '@/utils/api';
import { useRoute } from 'vue-router'; // 引入 useRoute 获取路由参数

// 1. 引入你的实验组件
import Terminal from '@/ExperimentalComponents/Terminal.vue';
import LingMaiModule from '@/LingMaiComponents/LingMaiModule.vue';

const route = useRoute(); // 初始化路由钩子


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

const currentTab = ref(route.query.tab || 'terminal');




// 计算当前应该渲染哪个组件
const activeComponent = computed(() => {
  const found = experiments.find(e => e.id === currentTab.value);
  return found ? found.component : null;
});


onMounted(() => {
  if (route.query.tab) {
    const found = experiments.find(e => e.id === route.query.tab);
    if (found) {
      currentTab.value = found.id;
    }
  }
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');

.experiment-lab {
  height: 100%; /* 🔥 强制占满全屏 */
  padding-top: 72px; /* 用 padding 代替 margin，防止高度计算溢出 */
  box-sizing: border-box; /* 关键 */
  background-color: #F0F0F0;
  font-family: 'JetBrains Mono', monospace;
  color: #111;
  display: flex;
  flex-direction: column;
}

.lab-warning-strip {
  background: #FFCC00;
  color: #000;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 0;
  text-align: center;
  border-bottom: 2px solid #111;
  user-select: none;
  flex-shrink: 0; /* 防止被挤压 */
}

.lab-container {
  flex: 1;
  display: flex;
  overflow: hidden; /* 防止最外层滚动 */
  height: 100%; /* 确保继承高度 */
}

/* --- 左侧边栏 --- */
.lab-sidebar {
  width: 240px;
  background: #E5E5E5;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  padding: 20px;
  flex-shrink: 0;
}

/* (Sidebar 内部样式保持不变...) */
.sidebar-header { font-weight: 700; font-size: 14px; margin-bottom: 30px; border-bottom: 2px dashed #999; padding-bottom: 10px; }
.module-item { cursor: pointer; padding: 10px 0; font-size: 13px; color: #666; display: flex; align-items: center; transition: all 0.2s; }
.module-item .bracket { opacity: 0; margin: 0 4px; color: #D92323; }
.module-item .indicator { margin-left: auto; font-weight: bold; color: #D92323; }
.module-item:hover, .module-item.active { color: #111; font-weight: 700; background: rgba(0,0,0,0.05); padding-left: 10px; }
.module-item:hover .bracket, .module-item.active .bracket { opacity: 1; }
.lab-status { margin-top: auto; font-size: 10px; color: #999; border-top: 2px solid #ccc; padding-top: 10px; }

/* --- 🔥🔥🔥 右侧视口核心修复 🔥🔥🔥 --- */
.lab-viewport {
  flex: 1;
  position: relative;
  background: #FAFAFA;
  
  /* 1. 变为 Flex 布局 */
  display: flex; 
  flex-direction: column;
  
  /* 2. 封印滚动：自己绝对不滚，让子元素滚 */
  overflow: hidden; 
  
  padding: 20px;
  background-image: radial-gradient(#ccc 1px, transparent 1px);
  background-size: 20px 20px;
  
  /* 3. 关键：Flex子项的最小高度设为0，防止被内容撑爆 */
  min-height: 0; 
}

/* 🔥 新增：给动态组件加的样式 */
/* 请确保在 template 里：<component class="experiment-content" ... /> */
.experiment-content {
  flex: 1;      /* 占满剩余空间 */
  height: 100%; /* 强制高度 */
  overflow: hidden; /* 内部再管理滚动 */
  min-height: 0; /* 🔥 极其重要：防止 Flex 子项高度溢出 */
  display: flex; /* 让内部的 LingMaiModule 也能用 Flex */
  flex-direction: column;
}

/* 占位空状态 */
.empty-state { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #999; }
.empty-icon { font-size: 40px; margin-bottom: 10px; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>