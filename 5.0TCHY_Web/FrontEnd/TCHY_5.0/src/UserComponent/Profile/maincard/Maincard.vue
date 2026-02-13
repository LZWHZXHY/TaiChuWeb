<template>
  <div class="main-card-box container">
    <div class="main-card-content">
      <div class="navigation-bar">  
        <div 
          v-for="item in navItems" 
          :key="item.key"
          class="nav-item"
          :class="{ active: currentTab === item.key }"
          @click="handleNavClick(item.key)"
        >
          <span class="corner-mark top-left"></span>
          <span class="corner-mark bottom-right"></span>
          
          <span class="nav-text">{{ item.name }}</span>
          
          <div class="status-bar"></div>
        </div>
      </div>

      <div class="display-area">
        <router-view v-if="false"></router-view> <Transition name="fade" mode="out-in">
          <KeepAlive>
      <component 
        :is="currentComponent" 
        :status="status" 
        :userId="userId"  />
    </KeepAlive>
        </Transition>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// --- 引入子组件 ---
// 确保这些文件在同级目录或正确的路径下
import HomeContent from './HomeContent.vue'
import PostContent from './PostContent.vue'
import HonorContent from './HonorContent.vue'
import MessageContent from './MessageContent.vue'

// --- 导航逻辑 ---
const currentTab = ref('home') // 默认选中主页

const navItems = [
  { name: '主页', key: 'home', icon: 'H' },
  { name: '发布', key: 'post', icon: 'P' },
  { name: '荣誉', key: 'honor', icon: 'O' },
  { name: '留言', key: 'message', icon: 'M' }
]

const props = defineProps({
  status: Object,
  userId: [Number, String] // 添加这一行，允许接收 ID
})
// 组件映射表：key 对应 navItems 中的 key
const tabs = {
  home: HomeContent,
  post: PostContent,
  honor: HonorContent,
  message: MessageContent
}

// 计算当前应该显示的组件
const currentComponent = computed(() => {
  return tabs[currentTab.value]
})

const handleNavClick = (key) => {
  currentTab.value = key
}
</script>

<style scoped>
/* 1. 父容器 */
.main-card-box.container {
  width: 70%;
  /* 移除固定最小高度限制，或者保留用于初始状态 */
  min-height: 400px; 
  /* 关键：height: auto 让其随内容撑开 */
  height: auto; 
  flex: 1; 
  margin-top: 5px;
  position: relative; 
  border: #000 0px solid;
  z-index: 3;
  display: flex;
  flex-direction: column;
  margin-bottom: 50px; /* 底部留白，防止滚动到底部太贴边 */
}

/* 3. 内容层 */
/* 3. 内容层 */
.main-card-content {
  position: relative;
  z-index: 1; 
  width: 100%;
  /* 修改：高度设为 auto */
  height: auto; 
  display: flex;
  flex-direction: column;
}

/* --- 导航栏样式 --- */
.navigation-bar {
  width: 100%;
  height: 50px;
  /* flex-shrink: 0 防止导航栏被压缩 */
  background: #f5f1e7;
  flex-shrink: 0; 
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 0px solid #000;
  padding: 0 0px;
  box-sizing: border-box;
}

.nav-item {
  flex: 1;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
  cursor: pointer;
  color: #888;
  font-weight: bold;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  letter-spacing: 2px;
  transition: all 0.2s ease-out;
  border-right: 1px dotted #aaa;
}
/* ... nav-item 的其他原有样式保持不变 ... */
.nav-item:last-child { border-right: none; }
.nav-item:hover { background: rgba(0, 0, 0, 0.2); color: #000; }
.nav-item.active { background: #1A1A1A; color: #F4F1EA; border-right: none; }
.nav-item.active .nav-text { transform: scale(1.1); text-shadow: 0 0 5px rgba(244, 241, 234, 0.5); }
.nav-item.active .status-bar { position: absolute; bottom: 0; left: 0; width: 100%; height: 4px; background: #ffffff; box-shadow: 0 -2px 10px rgba(255, 42, 42, 0.5); }
.corner-mark { position: absolute; width: 4px; height: 4px; background: transparent; transition: all 0.3s; }
.nav-item.active .corner-mark.top-left { top: 2px; left: 2px; border-top: 2px solid #FF2A2A; border-left: 2px solid #FF2A2A; }
.nav-item.active .corner-mark.bottom-right { bottom: 2px; right: 2px; border-bottom: 2px solid #FF2A2A; border-right: 2px solid #FF2A2A; }

/* --- 显示区域样式 --- */
.display-area {
  flex: 1; 
  width: 100%;
  padding: 20px;
  box-sizing: border-box;
  background: rgba(244, 241, 234, 0.9);
  /* 关键修改：改为 visible，允许内容溢出撑开父容器 */
  overflow-y: visible; 
  height: auto;
}

/* 简单的过渡动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@media (max-width: 400px) {
  .nav-item {
    font-size: 12px;
    letter-spacing: 0px;
  }
}
</style>