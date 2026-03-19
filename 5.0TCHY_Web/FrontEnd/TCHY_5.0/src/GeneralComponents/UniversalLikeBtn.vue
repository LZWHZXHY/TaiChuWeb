<template>
  <button 
    class="modern-like-btn" 
    :class="{ 'is-active': isLiked, 'is-animating': isAnimating }" 
    @click="toggleLike"
    :disabled="isLoading"
  >
    <div class="icon-fixed-box">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <path 
          class="path-outline"
          d="M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z"
        />
        <path 
          class="path-filled"
          d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
        />
      </svg>
    </div>
    
    <span class="count-text">{{ displayCount }}</span>
    
    <div class="particles" v-if="isAnimating">
      <span v-for="n in 6" :key="n" class="particle"></span>
    </div>
  </button>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import apiClient from '@/utils/api'; 

const props = defineProps({
  targetType: { type: String, required: true }, 
  targetId: { type: [String, Number], required: true },
  initialCount: { type: Number, default: 0 }
});

const isLiked = ref(false);
const currentCount = ref(props.initialCount);
const isLoading = ref(false);
const isAnimating = ref(false);
const displayCount = ref(props.initialCount);

const checkStatus = async () => {
  if (!props.targetId) return;
  try {
    const res = await apiClient.get(`/Like/status?type=${props.targetType}&id=${props.targetId}`);
    // 强制转换为布尔值，防止后端返回 0/1 导致的判断失效
    isLiked.value = !!(res.data?.isLiked);
  } catch (e) {
    console.error("Like status error", e);
  }
};

const toggleLike = async () => {
  if (isLoading.value) return;
  const previousState = isLiked.value;
  const previousCount = currentCount.value;
  
  isLiked.value = !isLiked.value;
  currentCount.value += isLiked.value ? 1 : -1;
  displayCount.value = currentCount.value; 
  
  if (isLiked.value) triggerAnimation(); 

  isLoading.value = true;
  try {
    const res = await apiClient.post('/Like/toggle', {
      targetType: props.targetType,
      targetId: props.targetId.toString()
    });
    if (res.data?.success) {
      currentCount.value = res.data.count;
      displayCount.value = currentCount.value;
    }
  } catch (e) {
    isLiked.value = previousState;
    currentCount.value = previousCount;
    displayCount.value = previousCount;
  } finally {
    isLoading.value = false;
  }
};

const triggerAnimation = () => {
  isAnimating.value = true;
  setTimeout(() => isAnimating.value = false, 600);
};

watch(() => props.targetId, () => {
  currentCount.value = props.initialCount;
  displayCount.value = props.initialCount;
  checkStatus();
});

onMounted(checkStatus);
</script>

<style scoped>
/* 按钮主体：强制背景与边框 */
.modern-like-btn {
  background-color: #ffffff !important;
  border: 1px solid #e5e7eb !important;
  color: #374151 !important;
  padding: 8px 16px !important;
  border-radius: 8px !important;
  display: flex !important;
  align-items: center !important;
  gap: 8px !important;
  cursor: pointer;
  height: 38px !important;
  min-width: 60px !important;
  transition: all 0.2s;
  position: relative;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05) !important;
}

/* 2. 核心修复：强制容器宽高，防止塌陷 */
.icon-fixed-box {
  width: 20px !important;
  height: 20px !important;
  display: block !important;
  position: relative !important;
  flex-shrink: 0;
}

.icon-fixed-box svg {
  width: 100% !important;
  height: 100% !important;
  display: block !important;
}

/* 3. 颜色与显隐逻辑 */
.path-outline {
  fill: #9ca3af !important;
  opacity: 1;
  transition: opacity 0.2s;
}

.path-filled {
  fill: #ef4444 !important;
  opacity: 0;
  transition: opacity 0.2s;
}

/* 激活状态下的变化 */
.modern-like-btn.is-active {
  background-color: #fef2f2 !important;
  border-color: #fca5a5 !important;
}

.modern-like-btn.is-active .path-outline {
  opacity: 0;
}

.modern-like-btn.is-active .path-filled {
  opacity: 1;
}

/* 数字文本 */
.count-text {
  font-family: 'Inter', sans-serif !important;
  font-weight: 700 !important;
  font-size: 0.95rem !important;
}

/* 粒子颜色 */
.particle {
  position: absolute;
  width: 4px; height: 4px;
  background: #ef4444;
  border-radius: 50%;
}
/* ... 粒子动画与之前保持一致 ... */
.particles { position: absolute; top: 50%; left: 24px; pointer-events: none; }
.is-animating .particle:nth-child(1) { animation: pop1 0.5s forwards; }
.is-animating .particle:nth-child(2) { animation: pop2 0.5s forwards; }
.is-animating .particle:nth-child(3) { animation: pop3 0.5s forwards; }
.is-animating .particle:nth-child(4) { animation: pop4 0.5s forwards; }
.is-animating .particle:nth-child(5) { animation: pop5 0.5s forwards; }
.is-animating .particle:nth-child(6) { animation: pop6 0.5s forwards; }
@keyframes pop1 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(-12px, -12px); opacity: 0; } }
@keyframes pop2 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(0px, -18px); opacity: 0; } }
@keyframes pop3 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(12px, -12px); opacity: 0; } }
@keyframes pop4 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(-10px, 10px); opacity: 0; } }
@keyframes pop5 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(10px, 10px); opacity: 0; } }
@keyframes pop6 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(0px, 18px); opacity: 0; } }
</style>