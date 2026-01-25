<template>
  <div class="data-card">
    <div class="bg-deco">DATA_LAYER</div>

    <div class="header-group">
      <div class="title-row">
        <span class="deco-label">TITLE //</span>
        <span class="main-title">{{ title }}</span>
      </div>
      <div class="level-row">
        <span class="cn-label">等级</span>
        <div class="level-badge">
          <span class="level-deco">LV.</span>
          <span class="level-val">{{ level }}</span>
        </div>
      </div>
    </div>

    <div class="stats-group">
      <div class="stat-row">
        <div class="stat-label-box">
          <span class="cn-label">声望</span>
          <span class="en-deco">REP</span>
        </div>
        <div class="stat-val rep-color">{{ reputation }}</div>
      </div>
      
      <div class="stat-row">
        <div class="stat-label-box">
          <span class="cn-label">金币</span>
          <span class="en-deco">GOLD</span>
        </div>
        <div class="stat-val gold-color">{{ formatNumberWithComma(gold) }}</div>
      </div>
    </div>

    <div class="exp-group">
      <div class="exp-header">
        <span class="cn-label">同步率</span>
        <span class="exp-nums">{{ currentExp }} / {{ nextLevelExp }}</span>
      </div>
      <div class="progress-container">
        <div class="progress-fill" :style="{ width: expPercent + '%' }"></div>
        <div class="progress-texture"></div>
      </div>
      <div class="exp-percent">{{ expPercent }}%</div>
    </div>
    
    <div class="right-border"></div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

const props = defineProps({
  level: { type: Number, default: 12 },
  currentExp: { type: Number, default: 2450 },
  nextLevelExp: { type: Number, default: 5000 },
  gold: { type: Number, default: 9527 },
  reputation: { type: Number, default: 880 },
  title: { type: String, default: '资深干员' }, // 默认改成中文，测试效果
  expPercent: { type: Number, default: 49 }
})

// 金币等大数字增加千分位逗号，提升可读性
const formatNumberWithComma = (num) => {
  return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")
}
</script>

<style scoped>
/* 引入等宽字体用于数字显示 */
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@500;700&display=swap');

.data-card {
  width: 100%;
  height: 100%;
  max-height: 178px; /* 严格限制高度 */
  padding: 10px 14px 10px 10px; /* 右侧留白给装饰条 */
  box-sizing: border-box;
  background-color: rgba(245, 241, 231, 0.5); 
  
  display: flex;
  flex-direction: column;
  justify-content: space-between; /* 上中下分布 */
  align-items: flex-end; /* 右对齐 */
  
  position: relative;
  overflow: hidden;
  border: 1px solid #2C3E50;
  
  /* 基础字体设置 */
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  color: #333;
  border-radius: 10px;
}

/* --- 背景装饰 --- */
.bg-deco {
  position: absolute;
  top: 40px;
  right: -10px;
  font-family: 'Roboto Mono', monospace;
  font-size: 3rem;
  font-weight: 900;
  color: rgba(0,0,0,0.03); /* 极淡的水印 */
  transform: rotate(-90deg);
  pointer-events: none;
  z-index: 0;
}

/* 右侧装饰条 */
.right-border {
  position: absolute;
  top: 0;
  right: 0;
  width: 4px;
  height: 100%;
  background-color: #2c3e50;
  border-left: 1px solid rgba(255,255,255,0.5);
}
.right-border::after {
  content: '';
  position: absolute;
  top: 0;
  right: 0;
  width: 100%;
  height: 20px;
  background-color: #e67e22; /* 顶部橙色点缀 */
}

/* --- 通用文字样式 --- */
.cn-label {
  font-size: 12px;
  color: #666;
  font-weight: bold;
}
.en-deco {
  font-family: 'Roboto Mono', monospace;
  font-size: 9px;
  color: #999;
  margin-left: 4px;
  text-transform: uppercase;
  opacity: 0.6;
}
.deco-label {
  font-family: 'Roboto Mono', monospace;
  font-size: 10px;
  color: #e67e22; /* 橙色装饰字 */
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
}

.title-row {
  display: flex;
  align-items: baseline;
}
.main-title {
  font-size: 20px; /* 大号字 */
  font-weight: 900;
  color: #1a1a1a;
  line-height: 1.2;
}

.level-row {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
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
  color: #e67e22; /* 等级数字橙色高亮 */
}

/* --- 中间数值区 (声望 & 金币) --- */
.stats-group {
  display: flex;
  flex-direction: column;
  gap: 8px; /* 数据间距 */
  width: 100%;
  z-index: 1;
}

.stat-row {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px; /* 标签和数值的距离 */
  border-bottom: 1px dashed rgba(0,0,0,0.1); /* 辅助线增强阅读引导 */
  padding-bottom: 2px;
}

.stat-label-box {
  display: flex;
  align-items: baseline;
}

.stat-val {
  font-family: 'Roboto Mono', monospace;
  font-size: 18px; /* 数值加大 */
  font-weight: bold;
}

.rep-color { color: #2c3e50; }
.gold-color { color: #d35400; } /* 金币用深橙色/暗金色 */

/* --- 底部经验区 --- */
.exp-group {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
  z-index: 1;
}

.exp-header {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.exp-nums {
  font-family: 'Roboto Mono', monospace;
  font-size: 11px;
  color: #555;
}

/* 进度条容器 */
.progress-container {
  width: 100%;
  height: 8px; /* 高度适中 */
  background: rgba(0,0,0,0.05);
  border: 1px solid #999;
  position: relative;
  border-radius: 1px;
}

.progress-fill {
  height: 100%;
  background: #2c3e50;
  position: absolute;
  top: 0;
  left: 0;
  z-index: 2;
}

/* 装饰纹理层，覆盖在整个条上 */
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
}

.exp-percent {
  font-family: 'Roboto Mono', monospace;
  font-size: 10px;
  color: #e67e22;
  font-weight: bold;
}
</style>