<template>
  <div class="notice-board-container">
    <div class="notice-header">
      <span class="header-icon">◈</span>
      <span class="header-text">公告 // {{ currentNotice.date }}</span>
    </div>

    <div class="content-wrapper">
      <div class="notice-content">
        <span class="prefix">>></span>
        {{ currentNotice.text }}
      </div>
    </div>

    <div class="notice-bottom-bar"></div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const notices = ref([
  { date: '2026.01.24', text: '个人主页已升级到4.0版本，新增多项功能。' },
  { date: '2026.01.22', text: '检测到外部数据流接入，防火墙规则已更新。' }
])

const currentNotice = computed(() => notices.value[0])
</script>

<style scoped>
.notice-board-container {
  width: 100%;
  /* 米色背景 */
  background-color: #F4F1EA; 
  /* 圆角设置 */
  border-radius: 8px;
  /* 隐藏溢出 */
  overflow: hidden;
  /* 移除内边距 */
  padding: 0;
  font-family: "Microsoft YaHei", "Heiti SC", sans-serif;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  margin-top: 15px;
  
  /* Flex 布局 */
  display: flex;
  flex-direction: column;
}

.notice-header {
  width: 100%;
  background-color: #2C3E50;
  color: #fff;
  padding: 10px 15px;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  font-weight: 900;
  letter-spacing: 1px;
  /* 防止头部也被压缩 */
  flex-shrink: 0; 
}

.header-icon {
  color: #e67e22;
  font-size: 10px;
}

.content-wrapper {
  padding: 15px;
  flex: 1; /* 撑开中间 */
  /* 确保内容不会溢出覆盖到底部条 */
  min-height: 0; 
}

.notice-content {
  font-size: 14px;
  font-weight: bold;
  line-height: 1.5;
  color: #1a1a1a;
  text-align: justify;
  word-wrap: break-word;
  margin-bottom: 15px;
}

.prefix {
  color: #FF2A2A;
  margin-right: 4px;
  font-weight: 900;
  font-family: 'Courier New', Courier, monospace; 
}

.notice-bottom-bar {
  width: 100%;
  height: 3px; /* 这里设置为 5px */
  background-color: #2C3E50;
  
  /* --- 关键修复代码 --- */
  flex-shrink: 0; /* 禁止 Flex 布局压缩这个元素 */
  display: block; /* 确保它是块级元素 */
  z-index: 1;     /* 确保层级正确 */
}
</style>