<template>
  <div class="stat-card">
    <div class="card-bg-grid"></div>
    <div class="corner-tl"></div>
    <div class="corner-br"></div>
    <div class="scan-line"></div>

    <div class="header-section">
      <div class="identity-col">
        <div class="role-badge">
          <span class="icon">◈</span>
          {{ user.role || 'OPERATOR' }}
        </div>
        <div class="user-name">{{ displayName }}</div>
      </div>
      <div class="serial-col">
        <span class="label">UID //</span>
        <span class="value">{{ user.uid || 'ERR' }}</span>
      </div>
    </div>

    <div class="hero-section">
      <div class="hero-block level-block">
        <div class="hero-label">CURRENT LEVEL</div>
        <div class="hero-value level-value">
          <span class="prefix">LV.</span>{{ user.level || 0 }}
        </div>
      </div>
      
      <div class="hero-divider"></div>

      <div class="hero-block points-block">
        <div class="hero-label">CONTRIBUTION POINTS</div>
        <div class="hero-value points-value">
          {{ formatNumber(user.stats?.likes || 0) }}
          <span class="unit">PTS</span>
        </div>
      </div>
    </div>

    <div class="exp-section">
      <div class="exp-meta">
        <span class="exp-label">EXP_PROGRESS</span>
        <span class="exp-val">75%</span>
      </div>
      <div class="progress-track">
        <div class="progress-fill" style="width: 75%">
          <div class="progress-glare"></div>
        </div>
      </div>
    </div>

    <div class="stats-grid-container">
      <div class="grid-item" v-for="(value, key) in displayStats" :key="key">
        <div class="stat-label">{{ key }}</div>
        <div class="stat-num">{{ formatNumber(value) }}</div>
        <div class="deco-corner"></div>
      </div>
    </div>

    <div class="footer-status">
      <span class="sys-msg">SYSTEM: OPTIMAL</span>
      <span class="ver">v2.0.4</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  user: {
    type: Object,
    required: true,
    default: () => ({})
  }
})

const displayName = computed(() => {
  return (props.user.nickname || props.user.username || 'UNKNOWN').toUpperCase()
})

// 排除 likes (因为已在上方作为 Points 显示)，只显示其他次要数据
const displayStats = computed(() => {
  const s = props.user.stats || {}
  return {
    FOLLOWERS: s.followers || 0,
    VIEWS: s.views || 0,
    WORKS: s.works || 0,
    // 这里可以加更多，比如 'CREDITS', 'RANK' 等
    ACHIEVEMENTS: 12 // 示例固定值
  }
})

const formatNumber = (num) => {
  if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M'
  if (num >= 1000) return (num / 1000).toFixed(1) + 'K'
  return num.toString()
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700;800&display=swap');

.stat-card {
  /* 变量定义 */
  --c-bg: #0a0a0a;
  --c-panel: rgba(20, 20, 20, 0.8);
  --c-red: #D92323;
  --c-red-dim: rgba(217, 35, 35, 0.2);
  --c-white: #F4F1EA;
  --c-gray: #555;
  --c-accent: #D92323; /* 主色调 */
  
  width: 100%;
  height: 100%;
  background: var(--c-bg);
  color: var(--c-white);
  font-family: 'JetBrains Mono', monospace;
  position: relative;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #333;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
}

/* === 背景特效 === */
.card-bg-grid {
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(rgba(255,255,255,0.02) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.02) 1px, transparent 1px);
  background-size: 30px 30px;
  pointer-events: none;
  z-index: 0;
}
.scan-line {
  position: absolute;
  top: 0; left: 0; right: 0; height: 100%;
  background: linear-gradient(to bottom, transparent, rgba(217, 35, 35, 0.05) 50%, transparent);
  animation: scan 4s linear infinite;
  pointer-events: none;
  z-index: 1;
}
@keyframes scan { 0% { transform: translateY(-100%); } 100% { transform: translateY(100%); } }

/* 角落装饰 */
.corner-tl { position: absolute; top: 0; left: 0; width: 12px; height: 12px; border-top: 3px solid var(--c-red); border-left: 3px solid var(--c-red); z-index: 2; }
.corner-br { position: absolute; bottom: 0; right: 0; width: 12px; height: 12px; border-bottom: 3px solid var(--c-red); border-right: 3px solid var(--c-red); z-index: 2; }

/* === 1. 顶部 Header (紧凑) === */
.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: rgba(255,255,255,0.03);
  border-bottom: 1px solid #333;
  z-index: 2;
  flex-shrink: 0;
}
.role-badge {
  font-size: 0.65rem;
  color: var(--c-red);
  background: rgba(217, 35, 35, 0.1);
  padding: 2px 6px;
  border-radius: 2px;
  display: inline-block;
  margin-bottom: 2px;
  font-weight: bold;
}
.user-name {
  font-family: 'Anton', sans-serif;
  font-size: 1.2rem;
  letter-spacing: 1px;
  line-height: 1;
}
.serial-col { text-align: right; font-size: 0.6rem; color: var(--c-gray); }
.serial-col .value { display: block; color: var(--c-white); font-family: 'Anton'; letter-spacing: 1px; }

/* === 2. Hero Section (Level & Points) - 核心重点 === */
.hero-section {
  display: flex;
  padding: 16px;
  align-items: center;
  justify-content: space-between;
  background: linear-gradient(180deg, rgba(0,0,0,0) 0%, rgba(217, 35, 35, 0.05) 100%);
  border-bottom: 1px solid #333;
  z-index: 2;
  flex-shrink: 0; /* 防止被压缩 */
}

.hero-block { flex: 1; display: flex; flex-direction: column; justify-content: center; }
.level-block { align-items: flex-start; }
.points-block { align-items: flex-end; text-align: right; }

.hero-label { font-size: 0.6rem; color: #888; margin-bottom: 4px; letter-spacing: 1px; }
.hero-value { font-family: 'Anton', sans-serif; line-height: 0.9; color: var(--c-white); text-shadow: 0 0 10px rgba(217, 35, 35, 0.3); }

/* 等级样式 */
.level-value { font-size: 3rem; color: var(--c-red); }
.level-value .prefix { font-size: 1rem; color: #666; margin-right: 2px; font-weight: normal; font-family: 'JetBrains Mono'; }

/* 点数样式 */
.points-value { font-size: 2.2rem; }
.points-value .unit { font-size: 0.8rem; color: var(--c-red); margin-left: 2px; vertical-align: super; font-family: 'JetBrains Mono'; }

.hero-divider {
  width: 1px; height: 40px; background: #333; margin: 0 20px;
  transform: skewX(-15deg);
}

/* === 3. Experience Bar === */
.exp-section {
  padding: 8px 16px 16px 16px; /* 上边距稍小，紧贴 Hero */
  z-index: 2;
  flex-shrink: 0;
}
.exp-meta { display: flex; justify-content: space-between; font-size: 0.6rem; color: #666; margin-bottom: 4px; }
.exp-val { color: var(--c-red); font-weight: bold; }

.progress-track {
  width: 100%; height: 6px; background: #222; position: relative; overflow: hidden;
}
.progress-fill {
  height: 100%; background: var(--c-red); position: relative;
}
/* 条纹动画效果 */
.progress-glare {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  background: repeating-linear-gradient(
    45deg,
    transparent,
    transparent 5px,
    rgba(0,0,0,0.3) 5px,
    rgba(0,0,0,0.3) 10px
  );
}

/* === 4. Stats Grid (自适应滚动区域) === */
.stats-grid-container {
  flex: 1; /* 关键：占据剩余空间 */
  min-height: 0; /* 关键：允许压缩 */
  overflow-y: auto;
  
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-auto-rows: max-content;
  gap: 8px;
  padding: 0 16px 16px 16px;
  z-index: 2;
}

/* 隐藏滚动条但保留功能 */
.stats-grid-container::-webkit-scrollbar { width: 0px; }

.grid-item {
  background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.05);
  padding: 10px;
  position: relative;
  transition: all 0.2s;
}
.grid-item:hover { background: rgba(255,255,255,0.06); border-color: var(--c-red); }

.stat-label { font-size: 0.6rem; color: #666; margin-bottom: 2px; }
.stat-num { font-family: 'Anton'; font-size: 1.2rem; color: #ddd; letter-spacing: 0.5px; }
.deco-corner {
  position: absolute; top: -1px; right: -1px; width: 6px; height: 6px;
  background: var(--c-gray); clip-path: polygon(100% 0, 0 0, 100% 100%);
}
.grid-item:hover .deco-corner { background: var(--c-red); }

/* === 5. Footer === */
.footer-status {
  padding: 8px 16px;
  border-top: 1px dashed #333;
  background: #080808;
  display: flex; justify-content: space-between;
  font-size: 0.55rem; color: #444;
  flex-shrink: 0;
  z-index: 3;
}
</style>