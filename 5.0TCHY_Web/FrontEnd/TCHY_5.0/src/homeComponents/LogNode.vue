<template>
  <div class="log-node-industrial">
    
    <div class="version-indicator" v-if="node.version">
      <span class="v-line"></span>
      <span class="v-text">## REVISION_{{ node.version }}</span>
    </div>

    <div v-if="node.logs && node.logs.length > 0" class="logs-container">
      <div 
        v-for="log in node.logs" 
        :key="log.id" 
        class="cyber-log-card"
        :class="getTypeClass(log.type)"
      >
        <div class="connector-line"></div>
        <div class="connector-dot"></div>

        <div class="card-inner">
          <div class="log-meta-row">
            <div class="meta-left">
              <span class="version-chip">v{{ log.version }}</span>
              <span class="type-tag">[{{ typeMap[log.type] || 'UNKNOWN' }}]</span>
            </div>
            <div class="meta-right">
              <span class="date-stamp">{{ formatDate(log.created_at) }}</span>
            </div>
          </div>

          <div class="log-title">
            <span class="arrow">></span> {{ log.title }}
          </div>

          <div class="log-body">
            {{ log.content }}
          </div>
        </div>
      </div>
    </div>

    <div v-if="node.children && node.children.length > 0" class="nested-children">
      <LogNode v-for="child in node.children" :key="child.version" :node="child" />
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

const props = defineProps({ node: Object })

const typeMap = {
  1: 'FIX',         // Bug修复
  2: 'UPDATE',      // 小内容更新
  3: 'OPTIMIZE',    // 网站优化
  4: 'MAJOR',       // 重大更新
  5: 'MISC'         // 其他
}

// 返回对应的样式类名
function getTypeClass(type) {
  switch (type) {
    case 1: return 'mode-fix'     // 红色
    case 2: return 'mode-update'  // 蓝色/黑色
    case 3: return 'mode-opt'     // 绿色/橙色
    case 4: return 'mode-major'   // 反色(黑底白字)
    default: return 'mode-misc'   // 灰色
  }
}

function formatDate(ts) {
  if (!ts) return 'N/A'
  const d = new Date(ts)
  // 格式化为工业风格: 2026.01.13 14:30
  const pad = (n) => n.toString().padStart(2, '0')
  return `${d.getFullYear()}.${pad(d.getMonth() + 1)}.${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 基础变量 --- */
.log-node-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #ffffff;
  --bg-gray: #f4f4f4;
  --mono: 'JetBrains Mono', monospace;
  
  font-family: var(--mono);
  position: relative;
  margin-bottom: 20px;
}

/* --- 版本号标头 --- */
.version-indicator {
  display: flex;
  align-items: center;
  margin-bottom: 15px;
  margin-left: -5px; /* 微调对齐 */
}
.v-line {
  width: 15px;
  height: 4px;
  background: var(--black);
  margin-right: 8px;
}
.v-text {
  font-weight: 800;
  font-size: 1.1rem;
  color: var(--black);
  background: #eee;
  padding: 2px 8px;
}

/* --- 卡片容器 --- */
.logs-container {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

/* --- 核心卡片样式 --- */
.cyber-log-card {
  position: relative;
  margin-left: 20px; /* 给连接线留位置 */
}

/* 装饰：水平连接线 (连接到父级的时间轴) */
.connector-line {
  position: absolute;
  left: -24px; /* 向左延伸，连接到父组件的竖线 */
  top: 24px;
  width: 24px;
  height: 2px;
  background: var(--black);
  z-index: 1;
}

/* 装饰：节点方块 */
.connector-dot {
  position: absolute;
  left: -28px;
  top: 20px;
  width: 8px;
  height: 8px;
  background: var(--black);
  border: 1px solid var(--white);
  z-index: 2;
}

/* 卡片本体 */
.card-inner {
  background: var(--white);
  border: 2px solid var(--black);
  padding: 16px;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1); /* 硬阴影 */
  transition: all 0.2s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  position: relative;
  overflow: hidden;
}

/* 悬停效果 */
.card-inner:hover {
  transform: translateX(4px) translateY(-2px);
  box-shadow: 6px 6px 0 var(--red);
  border-color: var(--black);
}

/* 激活连接点变红 */
.cyber-log-card:hover .connector-dot {
  background: var(--red);
}
.cyber-log-card:hover .connector-line {
  background: var(--black); /* 也可以变红，看喜好 */
}

/* --- 卡片内部布局 --- */

/* 顶部 Meta 信息 */
.log-meta-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px dashed #ccc;
  padding-bottom: 8px;
  margin-bottom: 10px;
  font-size: 0.8rem;
}

.meta-left { display: flex; gap: 8px; align-items: center; }

.version-chip {
  background: var(--black);
  color: var(--white);
  padding: 1px 6px;
  font-weight: bold;
}

.type-tag {
  font-weight: 800;
  text-transform: uppercase;
}

.date-stamp {
  color: #888;
  font-size: 0.75rem;
}

/* 标题 */
.log-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--black);
  margin-bottom: 8px;
  line-height: 1.4;
}
.arrow { color: var(--red); margin-right: 5px; }

/* 内容 */
.log-body {
  font-size: 0.9rem;
  color: #444;
  line-height: 1.6;
  white-space: pre-wrap;
}

/* --- 不同类型的变体样式 --- */

/* 1. BUG FIX (警告色/红色) */
.mode-fix .type-tag { color: var(--red); }
.mode-fix .card-inner { border-left: 4px solid var(--red); }

/* 2. UPDATE (常规/蓝色变体 -> 工业风里用黑色或深蓝) */
.mode-update .type-tag { color: #000; }
.mode-update .card-inner { border-left: 4px solid #000; }

/* 3. OPTIMIZE (优化/绿色 -> 工业风用灰色或条纹) */
.mode-opt .type-tag { color: #555; text-decoration: underline; }
.mode-opt .card-inner { border-left: 4px solid #888; }

/* 4. MAJOR (重大更新 -> 反色) */
.mode-major .card-inner {
  background: var(--black);
  color: var(--white);
  border: 2px solid var(--black);
}
.mode-major .version-chip { background: var(--red); }
.mode-major .type-tag { color: var(--red); }
.mode-major .log-title { color: var(--white); font-size: 1.2rem; text-transform: uppercase; }
.mode-major .log-body { color: #ccc; }
.mode-major .arrow { color: var(--red); }
.mode-major .log-meta-row { border-bottom: 1px dashed #555; }
.mode-major:hover .card-inner {
  box-shadow: 6px 6px 0 var(--red); /* 保持红色阴影 */
  border-color: var(--black);
}

/* --- 递归子节点 --- */
.nested-children {
  margin-left: 40px; /* 缩进 */
  padding-left: 20px;
  border-left: 1px dashed #ccc; /* 虚线表示层级关系 */
  margin-top: 20px;
}

/* 响应式微调 */
@media (max-width: 768px) {
  .cyber-log-card { margin-left: 10px; }
  .connector-line { width: 14px; left: -14px; }
  .connector-dot { left: -18px; }
  .nested-children { margin-left: 15px; padding-left: 10px; }
}
</style>