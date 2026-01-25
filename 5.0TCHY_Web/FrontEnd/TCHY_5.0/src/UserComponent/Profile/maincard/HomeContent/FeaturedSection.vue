<template>
  <div class="featured-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">精选发布</span>
        </div>
        <div class="header-right-line"></div>
      </div>

      <div class="featured-scroll-wrapper" ref="scrollContainer">
        <div v-for="n in 6" :key="n" class="featured-card">
          <div class="card-image">
            <div class="inner-border"></div>
            <div class="image-placeholder">视觉预览_0{{ n }}</div>
            <span class="img-tag">推荐-0{{ n }}</span>
          </div>
          
          <div class="card-info">
            <div class="info-header">
              <span class="card-title">精选内容标题 #{{ n }}</span>
              <span class="card-date">2026.01.25</span>
            </div>
            <div class="card-desc">
              > 检测到部分数据残留，正在尝试读取详细信息...
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">分区</span>
        <span class="value">00</span>
      </div>
      
      <div class="watermark-text">
        精选视图
      </div>

      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const scrollContainer = ref(null)

const handleScroll = (e) => {
  if (scrollContainer.value) {
    e.preventDefault()
    scrollContainer.value.scrollLeft += e.deltaY * 1.5
  }
}

onMounted(() => {
  const el = scrollContainer.value
  if (el) {
    el.addEventListener('wheel', handleScroll, { passive: false })
  }
})

onUnmounted(() => {
  const el = scrollContainer.value
  if (el) {
    el.removeEventListener('wheel', handleScroll)
  }
})
</script>

<style scoped>
/* 定义一致的 CSS 变量 */
.featured-section-wrapper {
  --primary-blue: #2c3e50;  /* 深蓝 */
  --accent-orange: #e67e22; /* 橙色 */
  --text-main: #333333;     /* 主要文字 */
  --text-sub: #666666;      /* 次要文字 */
  
  /* 字体设置 */
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
}

/* --- 全局容器与背景 --- */
.featured-section-wrapper {
  display: flex;
  width: 100%;
  position: relative;
  /* 背景改为浅色点阵 */
  background-image: radial-gradient(#ddd 1px, transparent 1px);
  background-size: 20px 20px;
  background-color: transparent;
  padding: 10px 0 0px 0;
  overflow: hidden;
  color: var(--text-main);
}

/* --- 左侧内容区 --- */
.content-area {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-right: 20px;
}

/* --- 标题栏 --- */
.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding-left: 5px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
}

.deco-block {
  width: 6px;
  height: 22px;
  background: var(--accent-orange); /* 橙色装饰块 */
  margin-right: 4px;
}

.header-main {
  font-size: 20px;
  font-weight: 900;
  color: var(--primary-blue); /* 深蓝标题 */
}

.header-right-line {
  flex: 1;
  height: 1px;
  margin-left: 15px;
  /* 深蓝虚线 */
  background: repeating-linear-gradient(
    90deg,
    var(--primary-blue),
    var(--primary-blue) 4px,
    transparent 4px,
    transparent 8px
  );
  opacity: 0.3;
}

/* --- 右侧装饰条 --- */
.decoration-sidebar {
  width: 40px; 
  flex: 0 0 40px;
  border-left: 1px solid rgba(0,0,0,0.1);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
  padding-top: 5px;
  position: relative;
}

.deco-status-box {
  background: transparent;
  width: 100%;
  padding: 6px 0;
  text-align: center;
  display: flex;
  flex-direction: column;
  border-right: 3px solid var(--accent-orange); /* 橙色右边框 */
}

.deco-status-box .label {
  font-size: 10px;
  color: var(--text-sub);
  font-weight: bold;
}

.deco-status-box .value {
  font-size: 14px;
  font-weight: bold;
  color: var(--primary-blue);
}

.watermark-text {
  writing-mode: vertical-rl;
  text-orientation: mixed;
  font-size: 24px;
  font-weight: 900;
  color: rgba(0, 0, 0, 0.05); /* 极淡水印 */
  letter-spacing: 4px;
  white-space: nowrap;
  user-select: none;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.stripe-bar {
  width: 6px;
  height: 100px;
  background: repeating-linear-gradient(
    45deg,
    var(--primary-blue),
    var(--primary-blue) 2px,
    transparent 2px,
    transparent 4px
  );
  opacity: 0.6;
  margin-bottom: 10px;
}

/* --- 滚动容器 --- */
.featured-scroll-wrapper {
  display: flex;
  flex-wrap: nowrap;
  gap: 15px;
  overflow-x: auto;
  padding-bottom: 15px;
  padding-top: 4px;
}

/* 滚动条样式优化 */
.featured-scroll-wrapper::-webkit-scrollbar {
  height: 6px;
}
.featured-scroll-wrapper::-webkit-scrollbar-track {
  background: rgba(0,0,0,0.05);
  border-radius: 3px;
}
.featured-scroll-wrapper::-webkit-scrollbar-thumb {
  background: #ccc;
  border-radius: 3px;
}
.featured-scroll-wrapper::-webkit-scrollbar-thumb:hover {
  background: var(--accent-orange);
}

/* --- 卡片样式 --- */
.featured-card {
  flex: 0 0 240px;
  height: 300px;
  display: flex;
  flex-direction: column;
  border: 1px solid #ddd;
  box-sizing: border-box;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  border-radius: 4px;
}

.featured-card:hover {
  transform: translateY(-2px);
  border-color: var(--accent-orange); /* 悬停变橙色 */
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.card-image {
  flex: 1;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.inner-border {
  position: absolute;
  top: 8px; left: 8px; right: 8px; bottom: 8px;
  border: 1px dashed #ddd;
  pointer-events: none;
}

.image-placeholder {
  color: #999;
  font-weight: bold;
  font-size: 13px;
}

.img-tag {
  position: absolute;
  top: 0;
  left: 0;
  background: var(--accent-orange);
  color: #fff;
  font-size: 11px;
  padding: 3px 8px;
  font-weight: bold;
  border-bottom-right-radius: 4px;
}

.card-info {
  height: 60px;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.5);
  border-top: 1px solid #eee;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.info-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.card-title {
  font-size: 13px;
  font-weight: bold;
  color: var(--text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 70%;
}

.featured-card:hover .card-title {
  color: var(--accent-orange);
}

.card-date {
  font-size: 11px;
  color: #999;
}

.card-desc {
  font-size: 11px;
  color: var(--text-sub);
  line-height: 1.2;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>