<template>
  <div class="container"></div>

  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    
    <div class="header-img-box">
      <Headerimg />
    </div>

    <div class="header-box container">
      <div class="idcard-box container">
        <Idcard />
      </div>
    </div>
    
    <div class="mian-card-box container"></div>

  </div>
</template>

<script setup>
import { ref, reactive, nextTick } from 'vue'
import { useRouter } from 'vue-router' 
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'
// 引入组件
import Idcard from './Idcard.vue'
import Headerimg from './Headerimg.vue' // 请确保路径正确
import backgroundcard from './backgroundcard.vue' // 请确保路径正确
</script>

<style scoped>
/* 基础容器样式 */
.container {
  border: 2px solid #000;
  background: #dfdfdf;
}

/* New-Profile 容器 */
.New-Profile.container {
  display: flex;
  flex-direction: column;
  justify-content: start;
  align-items: center;
  height: 100vw; /* 注意：全屏通常建议用 100vh */
  overflow-y: auto;
  position: relative;
  z-index: 2;
  /* 隐藏滚动条但允许滚动 (Chrome/Safari) */
  scrollbar-width: none; 
}
.New-Profile.container::-webkit-scrollbar {
  display: none;
}

/* 通用背景层 */
.background {
  position: fixed;
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  z-index: 1;
  opacity: 0.8;
}

/* --- 核心修改区域 Start --- */

/* header-img-box: 背景层容器 */
.header-img-box {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  
  /* 关键修复：设置最小高度，防止在窄屏下挤压背景内容 */
  height: 15vw;
  min-height: 140px; 
  
  z-index: 2;
  border: 0px solid #000;
  overflow: hidden; /* 防止子元素溢出 */
}

/* header-box: 内容层容器 */
.header-box.container {
  width: 100%;
  
  /* 关键修复：高度必须与 header-img-box 保持一致 */
  height: 15vw;
  min-height: 140px;
  
  /* 调整背景透明度，让底下的赛博背景透出来 */
  background: rgba(255, 255, 255, 0.05); 
  
  padding: 10px;
  position: relative;
  z-index: 3;
  box-sizing: border-box;
  
  /* 布局调整：让卡片沉底，不挡住右上角的 UNIT-01 字样 */
  display: flex;
  flex-direction: column;
  justify-content: flex-end; 
  align-items: center;
  border: 0px solid #000;
}

/* --- 核心修改区域 End --- */

/* idcard-box */
.idcard-box.container {
  width: 70%;
  height: 70%;
  /* 限制卡片最大高度，防止在大屏拉伸太严重 */
  max-height: 200px; 
  
  background: rgba(255, 255, 255, 0);
  /* 稍微增强阴影，使其在复杂背景上更清晰 */
  box-shadow: 0px 4px 12px rgba(0,0,0,0.2); 
  
  display: flex;
  justify-content: center;
  align-items: center;
  border: #000 solid 0px;
  
  /* 加上一点动效 */
  transition: transform 0.3s ease;
}

.idcard-box.container:hover {
  transform: translateY(-2px);
}

.mian-card-box.container {
  width: 70%;
  height: 100px;
  background: rgba(255, 255, 255, 0.1);
  box-shadow: 0px 4px 12px rgba(0,0,0,0.2);
  border: #000 solid 0px;
  flex: 1;
  margin-top: 10px;
}
</style>