<template>
  <div class="gallery-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">视觉档案库</span>
        </div>

        <div class="filter-group">
          <button 
            v-for="filter in filters" 
            :key="filter.key"
            class="filter-btn"
            :class="{ active: currentFilter === filter.key }"
            @click="currentFilter = filter.key"
          >
            {{ filter.label }}
          </button>
        </div>

        <div class="header-right-line"></div>

        <div class="view-all-btn">
          <span>查看全部</span>
          <span class="arrow">>></span>
        </div>

      </div>

      <div class="gallery-grid">
        <div v-for="n in 8" :key="n" class="gallery-card">
          <div class="thumb-container">
            <div class="thumb-placeholder">
              <span>视频记录_00{{ n }}</span>
              </div>
            <div class="overlay-stats">
              <span class="stat-item">
                <i class="icon">▶</i> {{ (Math.random() * 500).toFixed(1) }}万
              </span>
              <span class="stat-item">
                <i class="icon">▤</i> {{ Math.floor(Math.random() * 9000) }}
              </span>
            </div>
            <div class="overlay-duration">
              {{ Math.floor(Math.random() * 20) + 10 }}:{{ (Math.floor(Math.random() * 60) + '').padStart(2, '0') }}
            </div>
          </div>
          <div class="info-container">
            <div class="item-title">
              【绝密记录】第{{ n }}次神经网络潜入行动纪实，发现未知数据幽灵
            </div>
            <div class="item-date">
              2026年01月{{ (25 - n).toString().padStart(2, '0') }}日
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">分区</span>
        <span class="value">02</span>
      </div>
      <div class="watermark-text">数据视图</div>
      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'

const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'views', label: '最多点击' },
  { key: 'likes', label: '最多收藏' }
]
</script>

<style scoped>
/* 定义新配色的 CSS 变量 (参考 DataCard) */
.gallery-section-wrapper {
  --primary-blue: #2c3e50;  /* 深蓝 */
  --accent-orange: #e67e22; /* 橙色 */
  --text-main: #333333;     /* 主要文字 */
  --text-sub: #666666;      /* 次要文字 */
  --bg-light: transparent;  /* 背景 */
  
  /* 字体设置：强制统一为 DataCard 里的字体 */
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
}

/* --- 基础结构 --- */
.gallery-section-wrapper {
  display: flex;
  width: 100%;
  position: relative;
  /* 背景纹理改为浅灰色点阵 */
  background-image: radial-gradient(#ddd 1px, transparent 1px);
  background-size: 20px 20px;
  background-color: var(--bg-light);
  margin-top: 0px;
  padding: 10px 0 20px 0;
  overflow: hidden;
  color: var(--text-main);
}

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
  height: 30px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
  margin-right: 25px;
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
  color: var(--primary-blue); /* 深蓝色标题 */
  letter-spacing: 0px;
}

/* --- 筛选按钮组 --- */
.filter-group {
  display: flex;
  gap: 12px;
  align-items: center;
}

.filter-btn {
  background: transparent;
  border: none;
  font-family: inherit; /* 继承字体 */
  font-size: 13px;
  color: var(--text-sub);
  cursor: pointer;
  padding: 4px 8px;
  transition: all 0.3s;
  font-weight: bold;
  border-radius: 2px;
}

.filter-btn:hover {
  color: var(--primary-blue);
}

/* 选中状态：橙色高亮 */
.filter-btn.active {
  color: var(--accent-orange);
  font-weight: 900;
  border-bottom: 2px solid var(--accent-orange);
}

/* --- 装饰虚线 (深蓝色) --- */
.header-right-line {
  flex: 1;
  height: 1px;
  margin: 0 20px;
  opacity: 0.3;
  background: repeating-linear-gradient(90deg, var(--primary-blue), var(--primary-blue) 4px, transparent 4px, transparent 8px);
}

/* --- 查看全部按钮 --- */
.view-all-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--text-sub);
  cursor: pointer;
  transition: all 0.3s;
  font-family: inherit;
  padding: 4px 8px;
}

.view-all-btn:hover {
  color: var(--accent-orange);
}

.view-all-btn .arrow {
  font-weight: bold;
  transition: transform 0.3s;
}

.view-all-btn:hover .arrow {
  transform: translateX(4px);
}

/* --- 右侧装饰条 (DataCard 风格) --- */
.decoration-sidebar {
  width: 40px; flex: 0 0 40px; 
  border-left: 1px solid rgba(0,0,0,0.1); 
  display: flex; flex-direction: column; align-items: center; justify-content: space-between; padding-top: 5px; position: relative;
}
.deco-status-box {
  background: transparent; width: 100%; padding: 6px 0; text-align: center; display: flex; flex-direction: column; 
  border-right: 3px solid var(--accent-orange); /* 橙色右边框 */
}
.deco-status-box .label { font-size: 10px; color: var(--text-sub); font-weight: bold; }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }

.watermark-text {
  writing-mode: vertical-rl; text-orientation: mixed; font-size: 24px; font-weight: 900; 
  color: rgba(0, 0, 0, 0.05); /* 极淡的水印 */
  letter-spacing: 4px; white-space: nowrap; user-select: none; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
}
/* 装饰条 */
.stripe-bar {
  width: 6px; height: 100px; opacity: 0.6; margin-bottom: 10px;
  background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px);
}

/* --- 网格布局 --- */
.gallery-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-auto-rows: auto;
  gap: 20px 15px; 
}

.gallery-card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
  group: hover;
}

.thumb-container {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  background: rgba(238, 238, 238, 0.1);
  border: 1px solid #ddd;
  overflow: hidden;
  transition: all 0.3s ease;
  border-radius: 4px;
}

/* 悬停效果：橙色边框 */
.gallery-card:hover .thumb-container {
  border-color: var(--accent-orange);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  transform: translateY(-2px);
}

/* 图片占位符：简洁风格 */
.thumb-placeholder {
  width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; 
  color: #999; font-weight: bold; font-size: 13px;
  background: rgba(255, 255, 255, 0.3);
}

/* 覆盖层数据 */
.overlay-stats, .overlay-duration {
  position: absolute; bottom: 6px; color: #fff; font-size: 11px; padding: 2px 6px;
  background: rgba(44, 62, 80, 0.8); /* 深蓝半透明底 */
  border-radius: 2px;
  display: flex; align-items: center;
}

.overlay-stats { left: 6px; gap: 8px; }
.stat-item .icon { font-style: normal; margin-right: 2px; font-size: 10px; opacity: 0.8; }
.overlay-duration { right: 6px; font-weight: bold; }

.info-container { padding-top: 8px; }

.item-title {
  font-size: 14px;
  font-weight: bold;
  color: var(--text-main);
  line-height: 1.4;
  margin-bottom: 4px;
  
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color 0.2s;
}

/* 悬停标题变橙色 */
.gallery-card:hover .item-title { color: var(--accent-orange); }

.item-date {
  font-size: 12px; color: #999; 
}
</style>