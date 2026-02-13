<template>
  <div class="container"></div>
  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    <div class="header-img-box"><Headerimg /></div>

    <div class="header-box container">
      <div class="idcard-box container">
        <Idcard :user-id="targetUserId" />
      </div>
    </div>
    
    <div class="main-card-wrapper">
      <Maincard :user-id="targetUserId" />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/utils/auth' 

import Idcard from './Idcard/Idcard.vue'
import Headerimg from './Headerimg.vue'
import backgroundcard from './backgroundcard.vue'
import Maincard from './maincard/Maincard.vue'

const route = useRoute()
const authStore = useAuthStore()

// 计算目标用户 ID
// NewProfile.vue

const targetUserId = computed(() => {
  const paramId = route.params.userId
  
  // 逻辑判断：是否是在看“我”自己
  const isMe = !paramId || paramId === 'MEE' || String(paramId) === String(authStore.userID)

  if (isMe) {
    // 【核心修复】不要返回字符串 'MEE'，直接返回存储在 authStore 里的数字 ID
    // 这样传给 Maincard 的就是 1, 2, 10 等真实数字了
    return authStore.userID 
  }
  
  // 如果是看别人，则返回路由里的 ID
  return paramId
})


</script>

<style scoped>
/* 保持你的样式不变 */
.container { border: 2px solid #000000; background: transparent; }
.New-Profile.container { display: flex; flex-direction: column; justify-content: start; align-items: center; width: 100%; min-height: 100vh; height: auto; overflow: visible; position: relative; z-index: 2; background: rgba(0,0,0,0); border: 0px solid #000; padding-bottom: 50px; }
.background { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 1; opacity: 0.8; pointer-events: none; }
.header-img-box { position: absolute; top: 0; left: 0; width: 100%; height: 28vh; min-height: 140px; z-index: 2; overflow: hidden; opacity: 0; animation: slideDownFade 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards; }
.header-box.container { width: 100%; height: 28vh; min-height: 140px; background: transparent; padding: 10px; position: relative; z-index: 3; box-sizing: border-box; display: flex; flex-direction: column; justify-content: flex-end; align-items: center; }
.idcard-box.container { width: 70%; height: 70%; max-height: 200px; background: rgba(255, 255, 255, 0); box-shadow: 0px 4px 12px rgba(0,0,0,0.2); border: #000 0px solid; display: flex; justify-content: center; align-items: center; transition: transform 0.3s ease; opacity: 0; animation: popIn 0.8s cubic-bezier(0.34, 1.56, 0.64, 1) 0.2s forwards; }
.idcard-box.container:hover {transform: translateY(-2px);}
.main-card-wrapper { width: 70%; display: flex; justify-content: center; opacity: 0; animation: slideUpFade 0.8s cubic-bezier(0.25, 0.46, 0.45, 0.94) 0.4s forwards; }
@keyframes slideDownFade { from { opacity: 0; transform: translateY(-30px); } to { opacity: 1; transform: translateY(0); } }
@keyframes popIn { 0% { opacity: 0; transform: scale(0.9) translateY(30px); } 70% { opacity: 1; transform: scale(1.02) translateY(-5px); } 100% { opacity: 1; transform: scale(1) translateY(0); } }
@keyframes slideUpFade { from { opacity: 0; transform: translateY(40px); } to { opacity: 1; transform: translateY(0); } }
</style>