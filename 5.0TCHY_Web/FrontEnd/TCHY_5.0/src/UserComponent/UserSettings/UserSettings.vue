<template>
  <div class="settings-container container">
    
    <div class="settings-layout">
      
      <div class="content-panel">
        <div class="panel-header">
          <span class="panel-title">// {{ currentTabName }}</span>
          <div class="deco-line"></div>
        </div>
        
        <div class="empty-placeholder">
          <span class="placeholder-text">WAITING_FOR_DATA_INPUT...</span>
        </div>
      </div>

      <div class="nav-panel">
        <div 
          v-for="item in navItems" 
          :key="item.key"
          class="nav-btn"
          :class="{ active: currentTab === item.key }"
          @click="switchTab(item.key)"
        >
          <span class="corner top-left"></span>
          <span class="corner bottom-right"></span>
          
          <span class="btn-text">{{ item.label }}</span>
          
          <div class="active-bar"></div>
        </div>
      </div>
      
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 当前选中的 Tab，默认 'profile'
const currentTab = ref('profile')

// 导航配置
const navItems = [
  { key: 'profile', label: '资料设置' },
  { key: 'home',    label: '主页设置' },
  { key: 'privacy', label: '隐私设置' },
  { key: 'feature', label: '功能设置' }
]

// 计算当前显示的标题（用于左侧头部）
const currentTabName = computed(() => {
  const item = navItems.find(i => i.key === currentTab.value)
  return item ? item.label : 'SETTINGS'
})

// 切换 Tab 方法
const switchTab = (key) => {
  currentTab.value = key
}
</script>

<style scoped>
/* 引入字体 (复用你项目中的字体) */
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 1. 外部大容器 --- */
.settings-container {
  width: 70%;
  margin: 0 auto; /* 居中 */
  /* 设置一个最小高度，保证页面撑开，你可以根据需要调整 */
  min-height: 600px; 
  background-color: #F4F1EA; /* 米色背景 */
  border: 2px solid #111;    /* 黑色硬朗边框 */
  box-shadow: 0 4px 10px rgba(0,0,0,0.1);
  font-family: 'JetBrains Mono', "PingFang SC", sans-serif;
  color: #111;
  box-sizing: border-box;
}

/* --- 2. 内部布局 (Flex) --- */
.settings-layout {
  display: flex;
  width: 100%;
  height: 100%;
  min-height: inherit; /* 继承高度 */
}

/* --- 3. 左侧面板 (80%) --- */
.content-panel {
  flex: 0 0 80%; /* 固定 80% */
  width: 80%;
  padding: 30px;
  border-right: 2px solid #111; /* 右侧分割线 */
  display: flex;
  flex-direction: column;
  background-color: #fff; /* 内容区稍微白一点，突出层级 */
  position: relative;
}

/* 左侧头部样式 */
.panel-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
}
.panel-title {
  font-size: 24px;
  font-weight: 800;
  text-transform: uppercase;
  margin-right: 15px;
  color: #111;
}
.deco-line {
  flex: 1;
  height: 2px;
  background: repeating-linear-gradient(
    90deg,
    #111,
    #111 4px,
    transparent 4px,
    transparent 8px
  );
  opacity: 0.3;
}

/* 占位符样式 (暂时不做内容) */
.empty-placeholder {
  flex: 1;
  border: 1px dashed #ccc;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.02);
}
.placeholder-text {
  color: #999;
  font-size: 14px;
  letter-spacing: 1px;
}

/* --- 4. 右侧导航 (20%) --- */
.nav-panel {
  flex: 0 0 20%; /* 固定 20% */
  width: 20%;
  display: flex;
  flex-direction: column; /* 上下布局 */
  background-color: #F4F1EA;
  padding-top: 20px; /* 顶部留白 */
}

/* 导航按钮样式 */
.nav-btn {
  width: 100%;
  height: 40px; /* 限制最大高度 40px */
  max-height: 40px;
  display: flex;
  align-items: center;
  justify-content: center; /* 文字居中，也可改为 flex-start + padding-left */
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  font-weight: bold;
  font-size: 14px;
  color: #666;
  border-bottom: 1px solid rgba(0,0,0,0.1); /* 按钮分割线 */
}

/* 悬浮效果 */
.nav-btn:hover {
  background-color: rgba(0,0,0,0.05);
  color: #000;
  padding-left: 10px; /* 悬浮时轻微右移 */
}

/* 选中状态 (Active) */
.nav-btn.active {
  background-color: #111;
  color: #F4F1EA; /* 反色 */
  border-bottom: 1px solid #111;
}
.nav-btn.active:hover {
  padding-left: 0; /* 选中后取消位移 */
}

/* 装饰性元素 (模仿你的设计风格) */
.active-bar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background-color: #e67e22; /* 橙色高亮 */
  display: none;
}
.nav-btn.active .active-bar {
  display: block;
}

/* 角标装饰 */
.corner {
  position: absolute;
  width: 4px;
  height: 4px;
  border: 1px solid #fff;
  opacity: 0;
  transition: opacity 0.2s;
}
.nav-btn.active .corner {
  opacity: 1;
}
.corner.top-left { top: 2px; left: 6px; border-top: 1px solid #fff; border-left: 1px solid #fff; }
.corner.bottom-right { bottom: 2px; right: 2px; border-bottom: 1px solid #fff; border-right: 1px solid #fff; }

</style>