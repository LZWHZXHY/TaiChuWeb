<template>
  <div class="cyber-panel" ref="panelRef" @mousemove="handleMouseMove" @mouseleave="resetState">
    
    <div class="bg-grid" :style="parallaxStyle"></div>

    <div class="warning-zone">
      <div class="strip-pattern"></div>
      <div class="warning-content">
        <span class="icon">⚠</span>
        <span class="text">CAUTION // HOT</span>
      </div>
    </div>

    <div class="mech-info">
      <div class="screw-row">
        <div class="screw"></div>
        <div class="screw"></div>
      </div>
      <span class="unit-id">UNIT-01</span>
      <span class="status">OPERATIONAL</span>
    </div>

    <div class="data-block">
      <div class="bar"></div>
      <div class="bar medium"></div>
      <div class="bar long"></div>
    </div>

    <div class="crosshair-system" :style="crosshairStyle">
      <div class="line x"></div>
      <div class="line y"></div>
      <div class="coords">X:{{ mouseX.toFixed(0) }} <br> Y:{{ mouseY.toFixed(0) }}</div>
    </div>
    
    <div class="top-decor"></div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const panelRef = ref(null)
const mouseX = ref(0)
const mouseY = ref(0)
const tiltX = ref(0)
const tiltY = ref(0)

// 鼠标移动逻辑
const handleMouseMove = (e) => {
  if (!panelRef.value) return
  const rect = panelRef.value.getBoundingClientRect()
  mouseX.value = e.clientX - rect.left
  mouseY.value = e.clientY - rect.top
  
  // 计算轻微的视差偏移量
  tiltX.value = (e.clientX - rect.left - rect.width / 2) / 25
  tiltY.value = (e.clientY - rect.top - rect.height / 2) / 25
}

// 鼠标离开复位
const resetState = () => {
  tiltX.value = 0
  tiltY.value = 0
}

// 背景层的视差样式
const parallaxStyle = computed(() => ({
  transform: `translate(${tiltX.value * -1}px, ${tiltY.value * -1}px)`
}))

// 准星样式
const crosshairStyle = computed(() => ({
  left: `${mouseX.value}px`,
  top: `${mouseY.value}px`,
  opacity: mouseX.value === 0 ? 0 : 1
}))
</script>

<style scoped>
:root {
  --c-bg: #F4F1EA;    /* 工业米白 */
  --c-ink: #2B2B2B;   /* 碳素黑 */
  --c-red: #C0392B;   /* 警示红 */
}

.cyber-panel {
  width: 100%;
  height: 100%;
  background-color: #F4F1EA;
  /* 叠加噪点图层增加质感 */
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.8' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)' opacity='0.04'/%3E%3C/svg%3E");
  position: relative;
  overflow: hidden;
  user-select: none;
  font-family: 'Arial Narrow', sans-serif;
  border-bottom: 2px solid #2B2B2B;
}

/* 1. 网格背景 */
.bg-grid {
  position: absolute;
  top: -10%; left: -10%;
  width: 120%; height: 120%;
  background-image: 
    linear-gradient(rgba(0,0,0,0.06) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.06) 1px, transparent 1px);
  background-size: 30px 30px;
  transition: transform 0.1s linear;
  z-index: 0;
}

/* 2. 警示模块 (百分比高度 + 切角) */
.warning-zone {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 40%; /* 相对高度，防止溢出 */
  width: 180px;
  max-width: 45%;
  background: #2B2B2B;
  display: flex;
  align-items: center;
  clip-path: polygon(0 0, 100% 0, 85% 100%, 0% 100%);
  z-index: 2;
}

.strip-pattern {
  position: absolute;
  width: 100%; height: 100%;
  background: repeating-linear-gradient(
    45deg,
    transparent,
    transparent 8px,
    #C0392B 8px,
    #C0392B 16px
  );
  opacity: 0.2;
}

.warning-content {
  position: relative;
  z-index: 3;
  padding-left: 15px;
  color: #fff;
  display: flex;
  align-items: center;
  gap: 6px;
}
.warning-content .icon { color: #C0392B; }
.warning-content .text { 
  font-weight: bold; 
  /* 智能字体：最小10px，最大14px */
  font-size: clamp(10px, 1.5vw, 14px); 
  letter-spacing: 1px;
}

/* 3. 机体信息 (右上角) */
.mech-info {
  position: absolute;
  top: 10px;
  right: 15px;
  text-align: right;
  z-index: 2;
}
.screw-row {
  display: flex; justify-content: flex-end; gap: 6px; margin-bottom: 2px;
}
.screw {
  width: 6px; height: 6px; background: #bbb; border-radius: 50%;
  box-shadow: inset 1px 1px 1px rgba(0,0,0,0.4);
}
.unit-id {
  display: block;
  /* 智能大字：最小24px，最大48px */
  font-size: clamp(24px, 6vw, 48px);
  font-weight: 900;
  color: rgba(0,0,0,0.12);
  line-height: 1;
}
.status {
  display: block;
  font-size: 10px;
  color: #C0392B;
  letter-spacing: 2px;
}

/* 4. 数据条 */
.data-block {
  position: absolute;
  right: 0; bottom: 0;
  width: 80px; height: 40%;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  gap: 3px;
  padding: 0 10px 8px 0;
  opacity: 0.5;
}
.bar { height: 3px; background: #2B2B2B; align-self: flex-end; width: 40%; }
.bar.medium { width: 70%; }
.bar.long { width: 100%; background: #C0392B; }

/* 5. 准星系统 */
.crosshair-system {
  position: absolute;
  pointer-events: none;
  z-index: 10;
  will-change: top, left;
}
.line { position: absolute; background: rgba(192, 57, 43, 0.4); }
.line.x { width: 100vw; height: 1px; left: -50vw; top: 0; }
.line.y { width: 1px; height: 100vh; top: -50vh; left: 0; }
.coords {
  position: absolute; top: 4px; left: 4px;
  font-size: 9px; font-family: monospace; color: #C0392B;
  background: rgba(244, 241, 234, 0.8);
  padding: 2px;
}

/* 顶部装饰 */
.top-decor {
  position: absolute; top: 0; left: 30%;
  width: 10%; height: 4px; background: #C0392B;
}
</style>