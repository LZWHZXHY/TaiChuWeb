<template>
  <div class="achievement-panel">
    <div class="panel-header">
      <div class="header-left">
        <span class="icon">◈</span>
        <span class="title">成就履历</span>
      </div>
      <span class="count">4/8</span>
    </div>

    <div class="scroll-container">
      <div class="achievement-grid">
        <div 
          v-for="(item, index) in achievements" 
          :key="index" 
          class="achieve-card"
          :class="{ 'locked': !item.unlocked }"
        >
          <div class="card-icon">
            <span v-if="item.unlocked" class="status-dot online"></span>
            <span v-else class="status-dot offline"></span>
            <span class="icon-text">{{ item.code }}</span>
          </div>

          <div class="card-content">
            <div class="achieve-name">{{ item.name }}</div>
            <div class="achieve-desc">{{ item.desc }}</div>
          </div>
          
          <div class="corner-deco"></div>
        </div>
      </div>
    </div>

    <div class="panel-bottom-bar"></div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const achievements = ref([
  { code: 'A-01', name: '初入太初', desc: '首次登录系统', unlocked: true },
  { code: 'A-02', name: '神经漫游', desc: '浏览时长超过10小时', unlocked: true },
  { code: 'A-03', name: '代码猎手', desc: '发现并修复漏洞', unlocked: true },
  { code: 'A-04', name: '超频响应', desc: '操作频次达到峰值', unlocked: true },
  { code: 'B-01', name: '幽灵协议', desc: '??? (未解锁)', unlocked: false },
  { code: 'B-02', name: '深层潜入', desc: '??? (未解锁)', unlocked: false },
  { code: 'C-01', name: '核心超载', desc: '??? (未解锁)', unlocked: false },
  { code: 'C-02', name: '零日漏洞', desc: '??? (未解锁)', unlocked: false },
])
</script>

<style scoped>
.achievement-panel {
  width: 100%;
  
  /* 背景色保持米色系 */
  background-color: #EFECE3; 
  
  /* 圆角与溢出隐藏：保证顶底条贴边 */
  border-radius: 8px;
  overflow: hidden;
  
  /* 移除容器内边距，改由子元素控制 */
  padding: 0;
  
  box-sizing: border-box;
  
  /* 字体设置：全局统一 */
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  
  box-shadow: 0 4px 10px rgba(0,0,0,0.05);
  border: 1px solid #dcd9ce;
  
  /* Flex 布局确保底栏沉底 */
  display: flex;
  flex-direction: column;
  min-height: 200px;
}

/* --- 头部样式 (与 NoticeBoard 一致) --- */
.panel-header {
  width: 100%;
  background-color: #2C3E50; /* 深蓝背景 */
  color: #fff;               /* 白字 */
  padding: 10px 15px;
  box-sizing: border-box;
  
  display: flex;
  justify-content: space-between;
  align-items: center;
  
  /* 防止头部被压缩 */
  flex-shrink: 0; 
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.icon { 
  color: #e67e22; /* 橙色图标 */
  font-size: 12px; 
}

.title { 
  font-weight: 900; 
  font-size: 13px; 
  letter-spacing: 1px; 
  color: #fff;
  /* 移除 Courier New，继承全局字体 */
}

.count { 
  font-size: 11px; 
  font-weight: bold; 
  color: #2C3E50; 
  background: rgba(255, 255, 255, 0.9); 
  padding: 2px 8px;
  border-radius: 4px;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
}

/* --- 滚动容器 --- */
.scroll-container {
  overflow-y: auto;
  flex: 1; /* 填满剩余空间 */
  
  /* 补充内边距 */
  padding: 12px; 
  
  /* 确保内容不被底部条遮挡 */
  min-height: 0; 
}

/* 美化滚动条 (Webkit) */
.scroll-container::-webkit-scrollbar {
  width: 4px;
}
.scroll-container::-webkit-scrollbar-track {
  background: rgba(0,0,0,0.05);
  border-radius: 2px;
}
.scroll-container::-webkit-scrollbar-thumb {
  background: #ccc;
  border-radius: 2px;
}
.scroll-container::-webkit-scrollbar-thumb:hover {
  background: #FF9D00;
}

/* --- 网格布局 --- */
.achievement-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

/* --- 卡片样式 --- */
.achieve-card {
  background: #F5F1E7;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  position: relative;
  transition: all 0.2s;
  overflow: hidden;
}

.achieve-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 2px 6px rgba(0,0,0,0.08);
  border-color: #FF9D00;
}

/* 锁定状态 */
.achieve-card.locked {
  background: #f4f4f4;
  opacity: 0.7;
  border-style: dashed;
}
.achieve-card.locked .achieve-name,
.achieve-card.locked .icon-text {
  color: #999;
}

/* 图标行 */
.card-icon {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.status-dot.online { background: #FF9D00; box-shadow: 0 0 4px #FF9D00; }
.status-dot.offline { background: #ccc; }

.icon-text {
  font-size: 10px;
  /* 移除 Courier New */
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  color: #ccc;
  font-weight: bold;
}

/* 内容行 */
.card-content {
  display: flex;
  flex-direction: column;
}

.achieve-name {
  font-size: 12px; 
  font-weight: bold;
  color: #000000;
  margin-bottom: 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.achieve-desc {
  font-size: 10px;
  color: #888;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 装饰细节 */
.corner-deco {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 0;
  height: 0;
  border-bottom: 6px solid #e0e0e0;
  border-left: 6px solid transparent;
}
.achieve-card:hover .corner-deco {
  border-bottom-color: #FF9D00;
}

/* --- 底部装饰条 (与 NoticeBoard 一致) --- */
.panel-bottom-bar {
  width: 100%;
  height: 5px; 
  background-color: #2C3E50;
  
  /* 关键：防止 Flex 压缩 */
  flex-shrink: 0; 
  display: block;
  z-index: 1;
}
</style>