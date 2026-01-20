<template>
  <div class="stat-card">
    <div class="header-row">
      <span class="id-code">[ID:{{userID}}]</span>
      <div class="role-badge">{{ data.title || 'UNKNOWN' }}</div>
    </div>

    <div class="main-content">
      <div class="asset-block">
        <div class="label">CREDITS</div>
        <div class="value">
          <span class="currency">点数</span>
          {{ formattedGold }}
        </div>
      </div>

      <div class="level-block">
        <span class="lv-label">LV.</span>
        <span class="lv-val">{{ data.level }}</span>
      </div>
    </div>

    <div class="footer-row">
      <div class="xp-info">
        <span>EXP</span>
        <span>{{ expPercentage }}%</span>
      </div>
      <div class="xp-bar-bg">
        <div class="xp-bar-fill" :style="{ width: expPercentage + '%' }"></div>
      </div>
    </div>
    
    <div class="notch-tr"></div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { defineProps, defineEmits } from 'vue'

const props = defineProps({
  data: {
    type: Object,
    required: true,
    default: () => ({
      level: 42,
      title: 'ARCHITECT', // 职位/头衔
      gold: 58240,
      currentExp: 7500,
      maxExp: 10000
    })
  }
    ,user: {
    type: Object,
    required: true
  }, 
  userID: {
    type: [String, Number],
    required: true
  },
  achievements: {
    type: Array,
    required: true
  },
  isFollowing: {
    type: Boolean,
    required: true
  },
  showIdArchive: {
    type: Boolean,
    required: true
  }
})




const formattedGold = computed(() => {
  return (props.data.gold || 0).toLocaleString('en-US')
})

const expPercentage = computed(() => {
  if (!props.data.maxExp) return 0
  return Math.round(Math.min(100, Math.max(0, (props.data.currentExp / props.data.maxExp) * 100)))
})
</script>

<style scoped>
/* 风格策略: High Contrast Typographic (高对比排版)
  色板: 黑(#111), 米(#F4F1EA), 红(#D92323)
*/
.stat-card {
  position: relative;
  background-color: #111;
  color: #F4F1EA;
  padding: 20px;
  font-family: 'Helvetica Neue', Arial, sans-serif;
  overflow: hidden; /* 裁剪溢出的装饰 */
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 140px; /* 给一个最小高度保证气势 */
  box-shadow: 0 10px 20px rgba(0,0,0,0.3);
  transition: transform 0.3s;
}

.stat-card:hover {
  transform: translateY(-2px);
}

/* === Header === */
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  z-index: 2;
}
.id-code {
  font-family: monospace;
  font-size: 0.7rem;
  color: #666;
  letter-spacing: 1px;
}
.role-badge {
  font-size: 0.75rem;
  font-weight: 900;
  background: #F4F1EA;
  color: #111;
  padding: 2px 6px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* === Main Content === */
.main-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-end; /* 底部对齐 */
  position: relative;
  z-index: 2;
  margin-bottom: 20px;
}

/* 金币：清晰易读 */
.asset-block .label {
  font-size: 0.6rem;
  color: #888;
  font-weight: bold;
  margin-bottom: 4px;
}
.asset-block .value {
  font-size: 1.8rem;
  font-weight: 900;
  letter-spacing: -1px;
  line-height: 1;
}
.currency {
  font-size: 1rem;
  font-weight: 300;
  color: #D92323; /* 红色点缀 */
  margin-right: 2px;
}

/* 等级：巨大的视觉冲击 */
.level-block {
  text-align: right;
  line-height: 0.8;
}
.lv-label {
  display: block;
  font-size: 0.7rem;
  font-weight: bold;
  color: #D92323;
  margin-bottom: -5px;
}
.lv-val {
  font-size: 4rem; /* 极大字号 */
  font-weight: 900;
  color: #F4F1EA;
  /* 文字描边风格，让它更有层次 */
  -webkit-text-stroke: 0px; 
}

/* === Footer / XP === */
.footer-row {
  z-index: 2;
}
.xp-info {
  display: flex;
  justify-content: space-between;
  font-family: monospace;
  font-size: 0.6rem;
  color: #666;
  margin-bottom: 4px;
}
.xp-bar-bg {
  width: 100%;
  height: 4px;
  background: #333;
}
.xp-bar-fill {
  height: 100%;
  background: #D92323; /* 红色进度条 */
  transition: width 0.4s ease;
}

/* === 装饰元素 === */
/* 右上角切角 (纯CSS实现) */
.notch-tr {
  position: absolute;
  top: 0; right: 0;
  width: 0; height: 0;
  border-style: solid;
  border-width: 0 30px 30px 0;
  border-color: transparent #D92323 transparent transparent; /* 红色三角角标 */
  opacity: 0.8;
}

/* 背景隐形大字装饰 */
.stat-card::before {
  content: "USER";
  position: absolute;
  bottom: -10px; right: 60px;
  font-size: 6rem;
  font-weight: 900;
  color: rgba(255,255,255,0.03); /* 极淡的水印 */
  z-index: 1;
  pointer-events: none;
}
</style>