<template>
  <div class="blog-section-wrapper">
    
    <div class="content-area">
      <div class="section-header">
        <div class="header-left">
          <div class="deco-block"></div>
          <span class="header-main">技术博客</span>
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

      <div class="blog-list">
        <div v-for="n in 3" :key="n" class="blog-card">
          <div class="date-box">
            <span class="day">{{ (20 + n) }}</span>
            <span class="month">01月</span>
            <span class="year">2026</span>
          </div>
          <div class="blog-content">
            <div class="blog-title">
              <span class="category-tag">前端架构</span>
              Vue 3.5 响应式系统深层原理解析与性能优化实践
            </div>
            <div class="blog-summary">
              本文深入探讨了 Vue 3.5 版本中响应式系统的重构细节，分析了依赖收集与触发机制的改进点。通过实际案例演示了如何利用新的 effectScope API 进行更细粒度的副作用管理...
            </div>
            <div class="blog-meta">
              <span class="meta-item">阅读 2,341</span>
              <span class="meta-divider">|</span>
              <span class="meta-item">评论 18</span>
            </div>
          </div>
          <div class="blog-cover">
            <div class="cover-placeholder">没有数据</div>
          </div>
        </div>
      </div>
    </div>

    <div class="decoration-sidebar">
      <div class="deco-status-box">
        <span class="label">分区</span>
        <span class="value">00</span>
      </div>
      <div class="watermark-text">深度写作</div>
      <div class="stripe-bar"></div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'

const currentFilter = ref('latest')

const filters = [
  { key: 'latest', label: '最新发布' },
  { key: 'clicks', label: '最多点击' }, // 已改为最多点击
  { key: 'likes', label: '最多收藏' }
]
</script>

<style scoped>
.blog-section-wrapper {
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

/* --- 筛选按钮组 (参考 GallerySection) --- */
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

/* 选中状态：橙色高亮 */
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

/* --- 博客卡片样式 (保持不变) --- */
.blog-list { display: flex; flex-direction: column; gap: 15px; }
.blog-card { display: flex; background: rgba(255, 255, 255, 0.4); border: 1px solid #ddd; border-radius: 4px; padding: 15px; transition: all 0.3s ease; cursor: pointer; align-items: stretch; }
.blog-card:hover { border-color: var(--accent-orange); box-shadow: 0 4px 12px rgba(0,0,0,0.08); transform: translateX(4px); }
.date-box { display: flex; flex-direction: column; align-items: center; justify-content: center; padding-right: 20px; border-right: 1px dashed #eee; min-width: 60px; color: var(--primary-blue); }
.day { font-size: 24px; font-weight: 900; line-height: 1; }
.month { font-size: 12px; color: var(--text-sub); margin-top: 4px; }
.year { font-size: 10px; color: #999; transform: scale(0.9); }
.blog-content { flex: 1; padding: 0 20px; display: flex; flex-direction: column; justify-content: space-between; }
.blog-title { font-size: 16px; font-weight: bold; color: var(--text-main); margin-bottom: 8px; line-height: 1.4; display: flex; align-items: center; gap: 8px; }
.category-tag { background: rgba(230, 126, 34, 0.1); color: var(--accent-orange); font-size: 11px; padding: 2px 6px; border-radius: 2px; font-weight: normal; }
.blog-summary { font-size: 13px; color: var(--text-sub); line-height: 1.6; margin-bottom: 10px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.blog-meta { font-size: 12px; color: #999; }
.meta-divider { margin: 0 8px; color: #ddd; }
.blog-cover { width: 120px; background: rgba(249, 249, 249, 0.0); border-radius: 2px; display: flex; align-items: center; justify-content: center; margin-left: auto; }
.cover-placeholder { font-size: 10px; color: #ccc; font-weight: bold; }

/* --- 右侧装饰条 (保持不变) --- */
.decoration-sidebar { width: 40px; flex: 0 0 40px; border-left: 1px solid rgba(0,0,0,0.1); display: flex; flex-direction: column; align-items: center; justify-content: space-between; padding-top: 5px; position: relative; }
.deco-status-box { background: transparent; width: 100%; padding: 6px 0; text-align: center; border-right: 3px solid var(--accent-orange); }
.deco-status-box .label { font-size: 10px; color: var(--text-sub); font-weight: bold; }
.deco-status-box .value { font-size: 14px; font-weight: bold; color: var(--primary-blue); }
.watermark-text { writing-mode: vertical-rl; text-orientation: mixed; font-size: 24px; font-weight: 900; color: rgba(0, 0, 0, 0.05); letter-spacing: 4px; white-space: nowrap; user-select: none; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); }
.stripe-bar { width: 6px; height: 100px; opacity: 0.6; margin-bottom: 10px; background: repeating-linear-gradient(45deg, var(--primary-blue), var(--primary-blue) 2px, transparent 2px, transparent 4px); }
</style>