<template>
  <section class="roadmap-section">
    <div class="section-header">
      <div class="header-content">
        <h2>SYSTEM_ROADMAP // 发展规划</h2>
        <p class="subtitle">TAICHU_NETWORK 迭代日志与未来蓝图</p>
      </div>
      <div class="system-status">
        <span class="blink">●</span> SYSTEM_STATUS: <span class="highlight">EVOLVING</span>
      </div>
    </div>

    <div class="roadmap-grid">
      <div 
        v-for="(plan, index) in plans" 
        :key="index" 
        class="plan-card"
        :class="plan.status"
      >
        <div class="bg-decoration">{{ plan.phase }}</div>

        <div class="card-top">
          <span class="phase-tag">PHASE_{{ plan.phase }}</span>
          <span class="status-badge">{{ plan.statusText }}</span>
        </div>

        <h3 class="plan-title">{{ plan.title }}</h3>
        <p class="plan-desc">{{ plan.desc }}</p>

        <div class="progress-container">
          <div class="progress-label">
            <span>COMPLETION</span>
            <span>{{ plan.progress }}%</span>
          </div>
          <div class="progress-track">
            <div class="progress-bar" :style="{ width: plan.progress + '%' }"></div>
          </div>
        </div>

        <ul class="feature-list">
          <li v-for="(item, idx) in plan.features" :key="idx">
            <span class="bullet">></span> {{ item }}
          </li>
        </ul>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue';

const plans = ref([
  {
    phase: '01',
    title: '基建重构 & 核心',
    status: 'active', // 样式类名
    statusText: 'IN_PROGRESS', // 显示文本
    progress: 60,
    desc: '优化底层架构，提升 API 响应速度，完善个人页面功能，修复和优化局部小细节',
    features: [
      '个人页面全功能实现',
      '全站视觉优化',
      '全面使用对象储存技术'
    ]
  },
  {
    phase: '02',
    title: '网站生产化',
    status: 'pending',
    statusText: 'NEXT_STEP',
    progress: 10,
    desc: '让网站成为实际有效生产助力，可以实际的对生活产生真实助力',
    features: [
      'Notion功能 + obsidian功能 融合实现',
      '评论区富文本支持'
    ]
  },
  {
    phase: '03',
    title: '社区文化萌芽',
    status: 'locked',
    statusText: 'LOCKED',
    progress: 0,
    desc: '构建属于太初寰宇团体的社区文化',
    features: [
        '社区认同感',
        '社区文化',
        '固定节日活动'
    ]
  },
  {
    phase: '04',
    title: '商业化警告',
    status: 'locked',
    statusText: 'LOCKED',
    progress: 0,
    desc: '未来2-3年不及与考虑',
    features: [
        '要吃饭的嘛'
    ]
  },
]);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.roadmap-section {
  width: 100%;
  max-width: 1400px;
  margin: auto;
  padding: 0 20px;
  font-family: 'JetBrains Mono', monospace;
}

/* --- 头部样式 --- */
.section-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  border-bottom: 2px solid #333;
  padding-bottom: 20px;
  margin-bottom: 40px;
  flex-wrap: wrap;
  gap: 20px;
}



.section-header h2 {
  font-family: 'Anton', sans-serif;
  font-size: 2.5rem;
  color: #000000;
  margin: 0;
  letter-spacing: 1px;
}

.subtitle {
  color: #666;
  margin: 5px 0 0 0;
  font-size: 0.9rem;
}

.system-status {
  color: #00F0FF;
  font-weight: bold;
  border: 1px solid #00F0FF;
  padding: 5px 10px;
  font-size: 12px;
  background: rgba(0, 240, 255, 0.05);
}

.blink { animation: blink 1s infinite; }
@keyframes blink { 50% { opacity: 0; } }

/* --- 网格布局 --- */
.roadmap-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 30px;
}

/* --- 卡片核心样式 --- */
.plan-card {
  background: rgba(17, 17, 17, 0.8);
  border: 1px solid #333;
  padding: 30px;
  position: relative;
  overflow: hidden;
  transition: all 0.3s ease;
  /* 切角设计 */
  clip-path: polygon(
    0 0, 
    100% 0, 
    100% calc(100% - 20px), 
    calc(100% - 20px) 100%, 
    0 100%
  );
}

/* 背景大数字装饰 */
.bg-decoration {
  position: absolute;
  top: -20px;
  right: -10px;
  font-size: 8rem;
  font-family: 'Anton';
  color: rgba(255, 255, 255, 0.03);
  z-index: 0;
  pointer-events: none;
}

/* 状态颜色区分 */
.plan-card.active { border-color: #00F0FF; box-shadow: 0 0 15px rgba(0, 240, 255, 0.1); }
.plan-card.pending { border-color: #FFB800; }
.plan-card.locked { border-color: #444; opacity: 0.7; }

/* 卡片顶部 */
.card-top {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
  position: relative;
  z-index: 1;
}

.phase-tag { color: #666; font-size: 12px; font-weight: bold; }

.status-badge {
  font-size: 10px;
  padding: 2px 6px;
  background: #333;
  color: #fff;
  border-radius: 2px;
}

.active .status-badge { background: #00F0FF; color: #000; }
.pending .status-badge { background: #FFB800; color: #000; }
.locked .status-badge { background: #333; color: #888; }

/* 标题和描述 */
.plan-title {
  color: #fff;
  font-size: 1.5rem;
  margin-bottom: 10px;
  position: relative;
  z-index: 1;
}

.plan-desc {
  color: #999;
  font-size: 0.9rem;
  line-height: 1.6;
  margin-bottom: 25px;
  height: 45px; /* 限制高度保持对齐 */
  overflow: hidden;
  position: relative;
  z-index: 1;
}

/* 进度条 */
.progress-container { margin-bottom: 25px; position: relative; z-index: 1; }
.progress-label {
  display: flex;
  justify-content: space-between;
  font-size: 10px;
  color: #666;
  margin-bottom: 5px;
}
.progress-track {
  width: 100%;
  height: 4px;
  background: #333;
}
.progress-bar {
  height: 100%;
  background: #fff;
  transition: width 1s ease;
}

.active .progress-bar { background: #00F0FF; box-shadow: 0 0 10px #00F0FF; }
.pending .progress-bar { background: #FFB800; }

/* 功能列表 */
.feature-list {
  list-style: none;
  padding: 0;
  margin: 0;
  position: relative;
  z-index: 1;
}
.feature-list li {
  color: #bbb;
  font-size: 12px;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.bullet { color: #555; }
.active .bullet { color: #00F0FF; }
.pending .bullet { color: #FFB800; }

/* 悬停效果 */
.plan-card:hover {
  transform: translateY(-5px);
  background: rgba(30, 30, 30, 0.9);
}

/* 响应式 */
@media (max-width: 1024px) {
  .roadmap-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 768px) {
  .roadmap-grid { grid-template-columns: 1fr; }
  .section-header h2 { font-size: 1.8rem; }
}
</style>