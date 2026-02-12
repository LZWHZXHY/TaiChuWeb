<template>
  <div class="data-card" v-if="hasData || !loading">
    <div class="bg-deco">LV.{{ displayStats.level }}</div>

    <div class="header-group">
      <div class="title-row">
        <span class="deco-label">TITLE //</span>
        <span class="main-title">{{ displayStats.title || '无名之辈' }}</span>
      </div>
      <div class="level-row">
        <span class="cn-label">等级</span>
        <div class="level-badge">
          <span class="level-deco">LV.</span>
          <span class="level-val">{{ displayStats.level }}</span>
        </div>
      </div>
    </div>

    <div class="stats-group">
      <div class="stat-row">
        <div class="stat-label-box">
          <span class="cn-label">声望</span>
          <span class="en-deco">REP</span>
        </div>
        <div class="stat-val rep-color">{{ displayStats.reputation }}</div>
      </div>
      
      <div class="stat-row">
        <div class="stat-label-box">
          <span class="cn-label">金币</span>
          <span class="en-deco">COINS</span>
        </div>
        <div class="stat-val gold-color">{{ formatNumberWithComma(displayStats.coins) }}</div>
      </div>
    </div>

    <div class="exp-group">
      <div class="exp-header">
        <span class="cn-label">同步率</span>
        <span class="exp-nums">{{ displayStats.currentExp }} / {{ displayStats.nextLevelExp }}</span>
      </div>
      
      <div class="progress-container">
        <div class="progress-fill" :style="{ width: expPercentage + '%' }"></div>
        <div class="progress-texture"></div>
      </div>
      
      <div class="exp-percent">{{ expPercentage }}%</div>
    </div>
    
    <div class="right-border"></div>
  </div>
  
  <div v-else class="data-card loading-state">
    <div class="loading-text">LOADING STATS...</div>
  </div>
</template>

<script setup>
import { ref, defineProps, computed, onMounted, watch } from 'vue'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'

const props = defineProps({
  userId: {
    type: [String, Number],
    default: null
  }
})

const authStore = useAuthStore()
const loading = ref(true)
const remoteData = ref({}) // 用于存别人的数据

// 1. 判断是否是自己
const isMe = computed(() => {
  if (!props.userId || props.userId === 'MEE') return true
  return String(props.userId) === String(authStore.userID)
})

// 2. 双数据源选择
const rawStats = computed(() => {
  if (isMe.value) return authStore.user || {}
  return remoteData.value
})

// 3. 数据映射 (兼容大小写)
const displayStats = computed(() => {
  const s = rawStats.value
  return {
    level: s.Level || s.level || 1,
    title: s.Title || s.title || 'Loading...',
    reputation: s.Reputation || s.reputation || 0,
    coins: s.Coins || s.coins || 0, 
    currentExp: s.CurrentExp || s.currentExp || 0,
    nextLevelExp: s.NextLevelExp || s.nextLevelExp || 100
  }
})

// 4. 判断是否有数据
const hasData = computed(() => !!displayStats.value.title)

// 5. 计算经验百分比
const expPercentage = computed(() => {
  const { currentExp, nextLevelExp } = displayStats.value
  if (!nextLevelExp || nextLevelExp <= 0) return 100 
  let percent = (currentExp / nextLevelExp) * 100
  return Math.min(Math.max(percent, 0), 100).toFixed(1)
})

// 数字格式化
const formatNumberWithComma = (num) => {
  if (num === undefined || num === null) return '0'
  return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
}

// 6. 获取数据逻辑
const fetchUserStats = async () => {
  // 如果是看自己且 Store 有数据，直接结束
  if (isMe.value && authStore.user?.level) {
    loading.value = false
    return
  }

  loading.value = true
  try {
    const url = isMe.value ? '/profile/detail' : `/profile/get-id/${props.userId}`
    const res = await apiClient.get(url)
    
    if (res.data.success) {
      const d = res.data.data
      const statsMap = {
        level: d.Level,
        title: d.Title,
        coins: d.Coins || d.Points,
        reputation: d.Reputation,
        currentExp: d.CurrentExp,
        nextLevelExp: d.NextLevelExp
      }
      
      if (isMe.value) {
        authStore.user = { ...authStore.user, ...statsMap }
      } else {
        remoteData.value = statsMap
      }
    }
  } catch (e) { console.error(e) } 
  finally { loading.value = false }
}

onMounted(fetchUserStats)

watch(() => props.userId, () => {
  remoteData.value = {}
  fetchUserStats()
})
</script>

<style scoped>
/* 引入等宽字体用于数字显示 */
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@500;700&display=swap');

.data-card {
  width: 100%;
  height: 100%;
  padding: 10px 14px 10px 20px; 
  box-sizing: border-box;
  background: transparent;
  display: flex;
  flex-direction: column;
  justify-content: space-between; 
  align-items: flex-end; 
  position: relative;
  overflow: hidden;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  color: #333;
}

.loading-state {
  align-items: center;
  justify-content: center;
}
.loading-text {
  font-family: 'Roboto Mono';
  font-size: 12px;
  color: #999;
  animation: blink 1s infinite;
}
@keyframes blink { 50% { opacity: 0.5; } }

/* --- 背景装饰 --- */
.bg-deco {
  position: absolute;
  top: 40px;
  right: -10px;
  font-family: 'Roboto Mono', monospace;
  font-size: 3rem;
  font-weight: 900;
  color: rgba(0,0,0,0.05); 
  transform: rotate(-90deg);
  pointer-events: none;
  z-index: 0;
  white-space: nowrap;
}

/* 右侧装饰条 */
.right-border {
  position: absolute;
  top: 0;
  right: 0;
  width: 4px;
  height: 100%;
  background-color: #2c3e50;
}
.right-border::after {
  content: '';
  position: absolute;
  top: 0;
  right: 0;
  width: 100%;
  height: 20px;
  background-color: #e67e22; 
}

/* --- 通用文字样式 --- */
.cn-label {
  font-size: 12px;
  color: #666;
  font-weight: bold;
  margin-right: 4px;
}
.en-deco {
  font-family: 'Roboto Mono', monospace;
  font-size: 9px;
  color: #999;
  text-transform: uppercase;
  opacity: 0.6;
}
.deco-label {
  font-family: 'Roboto Mono', monospace;
  font-size: 10px;
  color: #e67e22; 
  margin-right: 6px;
  opacity: 0.8;
}

/* --- 顶部区域 (头衔 & 等级) --- */
.header-group {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
  z-index: 1;
  margin-top: 10px;
}

.title-row {
  display: flex;
  align-items: baseline;
}
.main-title {
  font-size: 20px; 
  font-weight: 900;
  color: #1a1a1a;
  line-height: 1.2;
}

.level-row {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
}
.level-badge {
  background: #2c3e50;
  color: #fff;
  padding: 0 6px;
  border-radius: 2px;
  display: flex;
  align-items: center;
  height: 20px;
}
.level-deco {
  font-size: 10px;
  opacity: 0.7;
  margin-right: 2px;
  font-family: 'Roboto Mono', monospace;
}
.level-val {
  font-size: 14px;
  font-weight: bold;
  font-family: 'Roboto Mono', monospace;
  color: #e67e22; 
}

/* --- 中间数值区 (声望 & 金币) --- */
.stats-group {
  display: flex;
  flex-direction: column;
  gap: 8px; 
  width: 100%;
  z-index: 1;
  margin-top: 15px;
}

.stat-row {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px; 
  border-bottom: 1px dashed rgba(0,0,0,0.1); 
  padding-bottom: 4px;
}

.stat-label-box {
  display: flex;
  align-items: baseline;
}

.stat-val {
  font-family: 'Roboto Mono', monospace;
  font-size: 18px; 
  font-weight: bold;
}

.rep-color { color: #2c3e50; }
.gold-color { color: #d35400; } 

/* --- 底部经验区 --- */
.exp-group {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
  z-index: 1;
  margin-top: auto; 
  padding-bottom: 5px;
}

.exp-header {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.exp-nums {
  font-family: 'Roboto Mono', monospace;
  font-size: 10px;
  color: #555;
}

/* 进度条容器 */
.progress-container {
  width: 100%;
  height: 8px; 
  background: rgba(0,0,0,0.05);
  border: 1px solid #999;
  position: relative;
  border-radius: 1px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #2c3e50;
  position: absolute;
  top: 0;
  left: 0;
  z-index: 2;
  transition: width 0.5s ease-out; /* 增加动画效果 */
}

/* 装饰纹理层 */
.progress-texture {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-image: repeating-linear-gradient(
    45deg,
    transparent,
    transparent 4px,
    rgba(255,255,255,0.2) 4px,
    rgba(255,255,255,0.2) 6px
  );
  z-index: 3;
  pointer-events: none;
}

.exp-percent {
  font-family: 'Roboto Mono', monospace;
  font-size: 10px;
  color: #e67e22;
  font-weight: bold;
}
</style>