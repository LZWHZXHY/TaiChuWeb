<template>
  <div class="container"></div>

  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    
    <div class="header-img-box"><Headerimg /></div>

    <div class="header-box container">
      <div class="idcard-box container"><Idcard :status="userStatus" /></div>
    </div>
    
    <div class="main-card-wrapper">
      <Maincard :status="userStatus" />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, nextTick } from 'vue'
import { onMounted } from 'vue'
import apiClient from '@/utils/api';
import { useRouter } from 'vue-router' 
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'

import Idcard from './Idcard/Idcard.vue'
import Headerimg from './Headerimg.vue'
import backgroundcard from './backgroundcard.vue'
import Maincard from './maincard/Maincard.vue'

// 1. 定义标准格式的数据
const userStatus = ref({
  level: 1,
  currentExp: 0,
  nextLevelExp: 100,
  gold: 0, 
  reputation: 100,
  title: 'Loading...',
  expPercent: 0
});

// 2. 获取数据的函数
const fetchUserStatus = async () => {
  try {
    const res = await apiClient.get('/Profile/me'); 
    if(res.data.success) {
      const serverData = res.data.data;
      userStatus.value = {
        level: serverData.Level,
        currentExp: serverData.CurrentExp,
        nextLevelExp: serverData.NextLevelExp,
        gold: serverData.Points, // 注意：后端是 Points，DataCard 内部用的是 gold
        reputation: serverData.Reputation,
        title: serverData.Title,
        expPercent: serverData.ExpPercent
      };
    }
  } catch(e) { console.error("数据加载失败", e); }
};

onMounted(() => {
  fetchUserStatus();
});
</script>

<style scoped>
.container {
  border: 2px solid #000000;
  background: transparent;
}

.New-Profile.container {
  display: flex;
  flex-direction: column;
  justify-content: start;
  align-items: center;
  width: 100%;
  min-height: 100vh; 
  height: auto;      
  overflow: visible; 
  position: relative;
  z-index: 2;
  background: rgba(0,0,0,0);
  border: 0px solid #000;
  padding-bottom: 50px; 
}

.background {
  position: fixed;
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  z-index: 1;
  opacity: 0.8;
  pointer-events: none; 
}

.header-img-box {
  position: absolute;
  top: 0; left: 0; 
  width: 100%;
  height: 28vh; 
  min-height: 140px; 
  z-index: 2; 
  overflow: hidden; 
  /* 新增动画：从上至下淡入 */
  opacity: 0; /* 初始隐藏 */
  animation: slideDownFade 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
}

.header-box.container {
  width: 100%;
  height: 28vh; 
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
}

.idcard-box.container {
  width: 70%; height: 70%; max-height: 200px; 
  background: rgba(255, 255, 255, 0);
  box-shadow: 0px 4px 12px rgba(0,0,0,0.2); 
  border: #000 0px solid;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: transform 0.3s ease;
  /* 新增动画：带有弹性的弹出效果，延迟0.2s */
  opacity: 0; /* 初始隐藏 */
  animation: popIn 0.8s cubic-bezier(0.34, 1.56, 0.64, 1) 0.2s forwards;
}
.idcard-box.container:hover {transform: translateY(-2px);}

/* 新增 .main-card-wrapper 的动画样式 */
.main-card-wrapper {
  width: 70%;
  display: flex;
  justify-content: center;
  /* 新增动画：从下至上淡入，延迟0.4s */
  opacity: 0; /* 初始隐藏 */
  animation: slideUpFade 0.8s cubic-bezier(0.25, 0.46, 0.45, 0.94) 0.4s forwards;
}


/* --- 新增动画关键帧定义 --- */

/* 顶部图片动画：下划淡入 */
@keyframes slideDownFade {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* ID卡动画：弹性弹出 */
@keyframes popIn {
  0% {
    opacity: 0;
    transform: scale(0.9) translateY(30px);
  }
  70% {
    opacity: 1;
    transform: scale(1.02) translateY(-5px);
  }
  100% {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

/* 主卡片动画：上划淡入 */
@keyframes slideUpFade {
  from {
    opacity: 0;
    transform: translateY(40px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>