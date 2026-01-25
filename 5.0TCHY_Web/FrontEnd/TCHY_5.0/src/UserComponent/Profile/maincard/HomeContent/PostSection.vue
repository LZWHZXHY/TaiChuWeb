<template>
  <div class="post-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">动态发布</span>
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

      <div class="post-grid">
        <div v-for="n in 4" :key="n" class="post-card">
          <div class="post-header">
            <div class="avatar-circle">K</div>
            <div class="user-info">
              <div class="username">K_Runner</div>
              <div class="time-ago">{{ n * 2 }}小时前 · 发布于夜之城</div>
            </div>
            <div class="more-options">...</div>
          </div>
          <div class="post-content">
            刚才重构了数据中心的散热控制逻辑，能耗降低了 15%。代码就像精密的机械，每一个齿轮的咬合都令人着迷。 #CodingLife #工业美学
          </div>
          
          <div class="post-images">
            <div class="post-image-item">
              <img :src="`https://picsum.photos/300/300?random=${n*10+1}`" alt="预览图片1">
            </div>
            <div class="post-image-item">
              <img :src="`https://picsum.photos/300/300?random=${n*10+2}`" alt="预览图片2">
            </div>
            <div class="post-image-item">
              <img :src="`https://picsum.photos/300/300?random=${n*10+3}`" alt="预览图片3">
              <div class="image-count-overlay">+2</div>
            </div>
          </div>
          
          <div class="post-actions">
            <div class="action-btn">
              <span class="icon">➦</span> 分享
            </div>
            <div class="action-btn">
              <span class="icon">💬</span> {{ 5 + n }}
            </div>
            <div class="action-btn">
              <span class="icon">♥</span> {{ 120 + n }}
            </div>
            <div class="post-source">
              发表于: 夜之城开发者社区
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
      <div class="watermark-text">实时动态</div>
      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'

const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'clicks', label: '最多点击' },
  { key: 'likes', label: '最多收藏' }
]
</script>

<style scoped>
/* 保持原有的CSS变量和基础样式不变 */
.post-section-wrapper {
  --primary-blue: #2c3e50;
  --accent-orange: #e67e22;
  --text-main: #333333;
  --text-sub: #666666;
  
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  display: flex;
  width: 100%;
  position: relative;
  background-image: radial-gradient(#ddd 1px, transparent 1px);
  background-size: 20px 20px;
  padding: 20px 0;
  color: var(--text-main);
  border-top: 1px dashed rgba(0,0,0,0.1); 
}

.content-area {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-right: 20px;
}

/* --- 标题栏样式 --- */
.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding-left: 5px;
  height: 30px;
}
.header-left { display: flex; align-items: center; gap: 8px; margin-right: 25px; }
.deco-block { width: 6px; height: 22px; background: var(--accent-orange); margin-right: 4px; }
.header-main { font-size: 20px; font-weight: 900; color: var(--primary-blue); }

/* --- 筛选按钮组 --- */
.filter-group {
  display: flex;
  gap: 12px;
  align-items: center;
}

.filter-btn {
  background: transparent;
  border: none;
  font-family: inherit;
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

.filter-btn.active {
  color: var(--accent-orange);
  font-weight: 900;
  border-bottom: 2px solid var(--accent-orange);
}

/* --- 装饰线 --- */
.header-right-line {
  flex: 1; height: 1px; margin: 0 20px; opacity: 0.3;
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

/* --- 帖子卡片样式 (修改为单列布局) --- */
.post-grid {
  display: grid;
  grid-template-columns: 1fr; /* 改为单列 */
  gap: 15px;
}

.post-card {
  background: rgba(255, 255, 255, 0.4);
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 15px;
  display: flex;
  flex-direction: column;
  transition: all 0.2s;
}

.post-card:hover {
  border-color: var(--accent-orange);
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.05);
}

.post-header { display: flex; align-items: center; margin-bottom: 10px; }
.avatar-circle { width: 32px; height: 32px; background: var(--primary-blue); color: #fff; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; margin-right: 10px; font-size: 14px; }
.user-info { flex: 1; display: flex; flex-direction: column; }
.username { font-size: 13px; font-weight: bold; color: var(--text-main); }
.time-ago { font-size: 10px; color: #999; }
.more-options { font-weight: bold; color: #ccc; cursor: pointer; letter-spacing: 2px; }
.post-content { font-size: 13px; line-height: 1.6; color: var(--text-sub); margin-bottom: 12px; text-align: justify; }

/* --- 新增图片画廊样式 --- */
.post-images {
  display: grid;
  grid-template-columns: repeat(3, 1fr); /* 3列布局 */
  gap: 8px;
  margin-bottom: 12px;
}

.post-image-item {
  position: relative;
  aspect-ratio: 1 / 1; /* 保持正方形 */
  border-radius: 4px;
  overflow: hidden;
  background-color: #f0f0f0;
  cursor: pointer;
}

.post-image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.post-image-item:hover img {
  transform: scale(1.05);
}

.image-count-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: bold;
}

/* --- 底部操作栏样式 (调整布局) --- */
.post-actions {
  display: flex;
  align-items: center;
  border-top: 1px solid #f5f5f5;
  padding-top: 10px;
  margin-top: auto;
}

.action-btn {
  font-size: 12px;
  color: #999;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  transition: color 0.2s;
  margin-right: 20px; /* 按钮间距 */
}

.action-btn:hover {
  color: var(--accent-orange);
}

.icon { font-size: 14px; }

.post-source {
  margin-left: auto; /* 靠右显示 */
  font-size: 12px;
  color: #999;
}

/* --- 右侧装饰条 (保持不变) --- */
.decoration-sidebar { width: 40px; flex: 0 0 40px; border-left: 1px solid rgba(0,0,0,0.1); display: flex; flex-direction: column; align-items: center; justify-content: space-between; padding-top: 5px; position: relative; }
.deco-status-box { background: transparent; width: 100%; padding: 6px 0; text-align: center; border-right: 3px solid var(--accent-orange); }
.deco-status-box .label { font-size: 10px; color: var(--text-sub); font-weight: bold; }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }
.watermark-text { writing-mode: vertical-rl; text-orientation: mixed; font-size: 24px; font-weight: 900; color: rgba(0, 0, 0, 0.05); letter-spacing: 4px; white-space: nowrap; user-select: none; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); }
.stripe-bar { width: 6px; height: 100px; opacity: 0.6; margin-bottom: 10px; background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px); }
</style>