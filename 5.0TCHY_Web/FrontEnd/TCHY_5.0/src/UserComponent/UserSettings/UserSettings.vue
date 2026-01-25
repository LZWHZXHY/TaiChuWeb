<template>
  <div class="container"></div>

  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    <div class="main-box container">
      
      <div class="left-box container">
        内容显示区域
      </div>

      <div class="right-box container">
        <button 
          v-for="(item, index) in menuItems" 
          :key="index" 
          class="menu-btn"
        >
          {{ item }}
        </button>
      </div>

      <div class="xuanxiang"></div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const menuItems = ref([
  '资料设置', 
  '个性设置', 
  '隐私设置', 
  '安全设置', 
  '功能设置', 
  '其它设置', 
  '测试功能'
])
</script>

<style scoped>
/* 全局容器设置 */
.container {
  border: 1px solid #000000;
  background: transparent;
  box-sizing: border-box; /* 防止 padding 撑大盒子 */
}

.background {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 0;
  background-color: #f5f5f5;
}

.New-Profile.container {
  display: flex;
  flex-direction: column;
  justify-content: start;
  align-items: center;
  width: 100vw;
  height: 89vh;      
  position: relative;
  padding: 1% 0px 2% 0px;
  z-index: 2;
}

.main-box.container {
  width: 60%;
  height: 100%;
  margin-top: 1%;
  display: flex;
}

/* 左侧盒子 (内容区) */
.left-box.container {
  width: 80%;
  height: 100%;
  /* 左侧样式可以根据需要添加，比如背景色区分 */
  background: #ffffff; 
}

/* 右侧盒子 (菜单区) - 修改点 */
.right-box.container {
  width: 20%;
  height: 100%;
  display: flex;
  flex-direction: column; /* 竖向排列 */
  gap: 10px;              /* 按钮间距 */
  padding: 10px;          /* 内边距 */
  align-items: center;    /* 子元素居中 */
  background: #f9f9f9;    /* 可选：给菜单栏一个浅色背景 */
}

/* 按钮样式 */
.menu-btn {
  width: 100%;            /* 宽度 100% */
  height: 50px;           /* 高度固定 50px */
  max-height: 50px;       /* 最大高度 50px */
  
  display: flex;          /* 启用 Flex 居中文字 */
  justify-content: center;
  align-items: center;
  
  font-size: 14px;        /* 字体大小 */
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 5px;
  cursor: pointer;
  transition: all 0.2s;
}

.menu-btn:hover {
  background-color: #e6f7ff;
  border-color: #1890ff;
  color: #1890ff;
}
</style>