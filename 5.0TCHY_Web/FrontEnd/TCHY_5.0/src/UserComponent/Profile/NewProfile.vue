<template>
  <div class="container"></div>

  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    
    <div class="header-img-box"><Headerimg /></div>

    <div class="header-box container">
      <div class="idcard-box container"><Idcard /></div>
    </div><Maincard /></div>

</template>

<script setup>
import { ref, reactive, nextTick } from 'vue'
import { useRouter } from 'vue-router' 
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'

// 引入组件
import Idcard from './Idcard/Idcard.vue'
import Headerimg from './Headerimg.vue'
import backgroundcard from './backgroundcard.vue'
// 新增引入 Maincard 组件
import Maincard from './maincard/Maincard.vue'

// maincardbackground 已经移入 Maincard.vue，这里不需要了
// 导航逻辑也已经移入 Maincard.vue
</script>

<style scoped>
/* 基础容器样式 */
.container {
  border: 2px solid #000000;
  background: transparent;
}

/* --- 核心修改 --- */
.New-Profile.container {
  display: flex;
  flex-direction: column;
  justify-content: start;
  align-items: center;
  
  /* 1. 不再固定高度，而是由内容撑开 */
  width: 100%;
  min-height: 100vh; /* 保证内容少时也能占满一屏 */
  height: auto;      /* 关键：允许高度无限增长 */
  
  /* 2. 移除内部滚动逻辑，交由浏览器 body 滚动 */
  overflow: visible; 
  
  position: relative;
  z-index: 2;
  background: rgba(0,0,0,0);
  border: 0px solid #000;
  
  /* 3. 底部留白，防止内容贴底 */
  padding-bottom: 50px; 
}
.New-Profile.container::-webkit-scrollbar {display: none;}

/* 移除之前隐藏滚动条的 Hack 代码，因为我们现在用的是原生滚动条 */

.background {
  position: fixed;
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  z-index: 1;
  opacity: 0.8;
  /* 背景固定不动，内容在上面滚动 */
  pointer-events: none; 
  border: #000 0px solid;
}

.header-img-box {
  position: absolute;
  top: 0; left: 0; 
  width: 100%;
  height: 28vh; /* 匹配您提到的高度 */
  min-height: 140px; 
  z-index: 2; 
  overflow: hidden; 
  border: #000 0px solid;
}

.header-box.container {
  width: 100%;
  height: 28vh; /* 匹配您提到的高度，作为占位 */
  min-height: 140px;
  background: transparent;
  padding: 10px;
  position: relative;
  z-index: 3;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: flex-end; 
  align-items: center;
  border: #000 0px solid;
}
.idcard-box.container {
  width: 70%; height: 70%; max-height: 200px; 
  background: rgba(255, 255, 255, 0);
  box-shadow: 0px 4px 12px rgba(0,0,0,0.2); 
  display: flex;
  justify-content: center;
  align-items: center;
  border: #000 solid 0px;
  transition: transform 0.3s ease;
}
.idcard-box.container:hover {transform: translateY(-2px);}

/* 注意：原有的 Maincard 相关样式已全部移至 Maincard.vue
   此处不需要保留，保持代码整洁
*/
</style>