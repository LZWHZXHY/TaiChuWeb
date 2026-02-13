<template>
  <button 
    class="cyber-like-btn" 
    :class="{ 'is-active': isLiked, 'is-animating': isAnimating }" 
    @click="toggleLike"
    :disabled="isLoading"
  >
    <span class="icon-box">
      <svg v-if="isLiked" viewBox="0 0 24 24" class="heart-icon filled"><path fill="currentColor" d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>
      <svg v-else viewBox="0 0 24 24" class="heart-icon outline"><path fill="currentColor" d="M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3zm-4.4 15.55l-.1.1-.1-.1C7.14 14.24 4 11.39 4 8.5 4 6.5 5.5 5 7.5 5c1.54 0 3.04.99 3.57 2.36h1.87C13.46 5.99 14.96 5 16.5 5c2 0 3.5 1.5 3.5 3.5 0 2.89-3.14 5.74-7.9 10.05z"/></svg>
    </span>
    
    <span class="count-text">{{ displayCount }}</span>
    
    <div class="particles" v-if="isAnimating">
      <span v-for="n in 6" :key="n" class="particle"></span>
    </div>
  </button>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import apiClient from '@/utils/api'; // 确保你有这个 axios 实例

const props = defineProps({
  targetType: { type: String, required: true }, // 'Post', 'Blog', 'Drawing'
  targetId: { type: [String, Number], required: true },
  initialCount: { type: Number, default: 0 }
});

const isLiked = ref(false);
const currentCount = ref(props.initialCount);
const isLoading = ref(false);
const isAnimating = ref(false);

// 用于显示：如果大于 999 显示 999+
const displayCount = ref(props.initialCount);

// 1. 初始化检查状态
const checkStatus = async () => {
  if (!props.targetId) return;
  try {
    const res = await apiClient.get(`/Like/status?type=${props.targetType}&id=${props.targetId}`);
    if (res.data) {
      isLiked.value = res.data.isLiked;
    }
  } catch (e) {
    console.error("点赞状态同步失败", e);
  }
};

// 2. 核心：切换点赞
const toggleLike = async () => {
  if (isLoading.value) return;
  
  // 乐观更新 (Optimistic UI)：先变界面，再发请求，体验极快
  const previousState = isLiked.value;
  const previousCount = currentCount.value;
  
  isLiked.value = !isLiked.value;
  currentCount.value += isLiked.value ? 1 : -1;
  displayCount.value = currentCount.value; // 更新显示
  
  if (isLiked.value) triggerAnimation(); // 播放动画

  isLoading.value = true;
  try {
    const res = await apiClient.post('/Like/toggle', {
      targetType: props.targetType,
      targetId: props.targetId.toString()
    });
    
    // 如果后端返回了最新准确计数，同步一下
    if (res.data && res.data.success) {
      currentCount.value = res.data.count;
      displayCount.value = currentCount.value;
    }
  } catch (e) {
    // 失败回滚
    console.error("操作失败", e);
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

// 监听 ID 变化（防止组件复用时数据不更新）
watch(() => props.targetId, () => {
  currentCount.value = props.initialCount;
  displayCount.value = props.initialCount;
  checkStatus();
});

onMounted(() => {
  checkStatus();
});
</script>

<style scoped>
.cyber-like-btn {
  --main-color: #D92323; /* 赛博红 */
  --text-color: #333;
  
  background: transparent;
  border: 2px solid #ddd;
  padding: 6px 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  position: relative;
  overflow: visible; /* 让粒子飞出去 */
  font-family: 'JetBrains Mono', monospace;
}

.cyber-like-btn:hover {
  border-color: var(--main-color);
  transform: translateY(-2px);
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.cyber-like-btn:active {
  transform: translateY(0);
}

/* 激活状态（已点赞） */
.cyber-like-btn.is-active {
  border-color: var(--main-color);
  background: rgba(217, 35, 35, 0.05);
}

.cyber-like-btn.is-active .heart-icon {
  color: var(--main-color);
  filter: drop-shadow(0 0 5px var(--main-color));
  animation: heartbeat 0.4s ease-in-out;
}

.icon-box {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.heart-icon {
  width: 100%;
  height: 100%;
  color: #999;
  transition: color 0.3s;
}

.count-text {
  font-weight: bold;
  font-size: 1rem;
  color: var(--text-color);
}

/* --- 粒子动画特效 --- */
@keyframes heartbeat {
  0% { transform: scale(1); }
  50% { transform: scale(1.4); }
  100% { transform: scale(1); }
}

.particles {
  position: absolute;
  top: 50%; left: 20px; /* 定位在心形附近 */
  width: 0; height: 0;
  pointer-events: none;
}

.particle {
  position: absolute;
  width: 4px; height: 4px;
  background: var(--main-color);
  border-radius: 50%;
}

.is-animating .particle:nth-child(1) { animation: pop1 0.6s ease-out forwards; }
.is-animating .particle:nth-child(2) { animation: pop2 0.6s ease-out forwards; }
.is-animating .particle:nth-child(3) { animation: pop3 0.6s ease-out forwards; }
.is-animating .particle:nth-child(4) { animation: pop4 0.6s ease-out forwards; }
.is-animating .particle:nth-child(5) { animation: pop5 0.6s ease-out forwards; }
.is-animating .particle:nth-child(6) { animation: pop6 0.6s ease-out forwards; }

@keyframes pop1 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(-15px, -15px); opacity: 0; } }
@keyframes pop2 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(0px, -20px); opacity: 0; } }
@keyframes pop3 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(15px, -15px); opacity: 0; } }
@keyframes pop4 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(-10px, 10px); opacity: 0; } }
@keyframes pop5 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(10px, 10px); opacity: 0; } }
@keyframes pop6 { 0% { transform: translate(0,0); opacity: 1; } 100% { transform: translate(0px, 20px); opacity: 0; } }
</style>