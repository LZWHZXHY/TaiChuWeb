<template>
  <div class="update-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="log-wrapper">
      
      <header class="log-header">
        <div class="header-left">
          <h1 class="giant-text">
            <span class="deco-box">■</span> PATCH_NOTES
          </h1>
          <div class="sub-text">系统更新记录 // VERSION_HISTORY</div>
        </div>
        
        <div class="header-right">
          <div class="status-row">
            <span>SERVER_STATUS:</span>
            <span class="status-ok">ONLINE</span>
          </div>
          <div class="status-row">
            <span>LAST_SYNC:</span>
            <span class="blink">{{ new Date().toLocaleTimeString() }}</span>
          </div>
        </div>
      </header>

      <div class="stream-container">
        <div class="timeline-bar"></div>

        <div class="update-tree">
          <LogNode 
            v-for="(node, index) in updateTree" 
            :key="node.version || index" 
            :node="node" 
            class="cyber-node-item"
          />
        </div>

        <div class="status-footer">
          <div v-if="loading" class="loading-state">
            <span class="spinner"></span>
            [ DOWNLOADING_PACKETS... ]
          </div>
          
          <div v-if="error" class="error-state">
            [ ! ] ERROR: {{ error }} // CONNECTION_RESET
          </div>
          
          <div v-if="!hasMore && updateTree.length > 0" class="end-state">
            <div class="divider-line"></div>
            <span>// END_OF_STREAM // ALL_SYSTEMS_UPDATED</span>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import apiClient from '@/utils/api'
import LogNode from './LogNode.vue'

const updateTree = ref([])
const loading = ref(false)
const error = ref('')
const page = ref(1)
const hasMore = ref(true)
const pageSize = 20

async function loadUpdates() {
  if (!hasMore.value || loading.value) return
  loading.value = true
  error.value = ''
  try {
    const res = await apiClient.get('/Update/all', { params: { page: page.value, pageSize } })
    const arr = res.data?.data || []
    if (arr.length < pageSize) hasMore.value = false
    updateTree.value = [...updateTree.value, ...arr]
    page.value += 1
  } catch (e) {
    error.value = '加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

function handleScroll() {
  if (!hasMore.value || loading.value) return
  if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 120) {
    loadUpdates()
  }
}

onMounted(() => {
  loadUpdates()
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.update-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  min-height: 100vh;
  position: relative;
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono);
  overflow: hidden;
  padding: 40px 0;
}

/* --- 背景网格 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* --- 主容器 --- */
.log-wrapper {
  position: relative;
  z-index: 1;
  max-width: 900px;
  margin: 0 auto;
  padding: 0 20px;
}

/* --- 头部 --- */
.log-header {
  border: 2px solid var(--black);
  background: #fff;
  padding: 20px 30px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 40px;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}
.header-left { display: flex; flex-direction: column; }
.giant-text {
  font-family: var(--heading);
  font-size: 3rem;
  margin: 0;
  line-height: 1;
  display: flex;
  align-items: center;
  gap: 15px;
}
.deco-box { color: var(--red); font-size: 0.8em; }
.sub-text { font-weight: bold; color: #666; margin-top: 5px; font-size: 0.9rem; }

.header-right { 
  text-align: right; font-size: 0.8rem; border-left: 2px solid #eee; padding-left: 20px; 
}
.status-row { margin-bottom: 5px; }
.status-ok { background: var(--black); color: var(--white); padding: 0 6px; font-weight: bold; margin-left: 5px; }
.blink { animation: blink 1s infinite; color: var(--red); font-weight: bold; }

/* --- 数据流区域 --- */
.stream-container {
  position: relative;
  padding-left: 30px; /* 给左侧线条留空 */
}

/* 左侧时间轴线条 */
.timeline-bar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: repeating-linear-gradient(to bottom, var(--black), var(--black) 10px, transparent 10px, transparent 20px);
}

/* --- 列表与节点 --- */
.update-tree {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 关键点：这里使用 :deep() 来强制覆盖 LogNode 组件内部的样式 
   哪怕我不知道 LogNode 的具体代码，我假设它是一块 div
*/
:deep(.cyber-node-item) {
  position: relative;
  border: 2px solid var(--black) !important;
  background: #fff !important;
  padding: 20px !important;
  border-radius: 0 !important; /* 强制去圆角 */
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1) !important;
  transition: transform 0.2s !important;
  margin-bottom: 10px;
}

:deep(.cyber-node-item:hover) {
  transform: translateX(5px) !important;
  box-shadow: 6px 6px 0 var(--red) !important;
  border-color: var(--black) !important;
}

/* 装饰性连接线：连接左侧时间轴和卡片 */
:deep(.cyber-node-item::before) {
  content: '';
  position: absolute;
  left: -32px; /* 伸到父容器 padding 区域 */
  top: 30px;
  width: 30px;
  height: 2px;
  background: var(--black);
}
:deep(.cyber-node-item::after) {
  content: '';
  position: absolute;
  left: -36px;
  top: 26px;
  width: 10px;
  height: 10px;
  background: var(--red);
  border: 2px solid var(--black);
}

/* 假设 LogNode 内部有标题或日期，尝试用通用选择器调整字体 */
:deep(.cyber-node-item h3), 
:deep(.cyber-node-item .title) {
  font-family: var(--heading) !important;
  font-size: 1.5rem !important;
  margin-bottom: 10px !important;
  text-transform: uppercase !important;
}

/* --- 底部状态 --- */
.status-footer {
  margin-top: 40px;
  text-align: center;
  font-family: var(--mono);
  font-weight: bold;
}

.loading-state {
  color: var(--black);
  background: #fff;
  display: inline-block;
  padding: 10px 20px;
  border: 1px dashed var(--black);
}
.spinner {
  display: inline-block;
  width: 12px; height: 12px;
  border: 2px solid var(--black);
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-right: 10px;
}

.error-state { color: var(--red); border: 2px solid var(--red); padding: 10px; background: rgba(217, 35, 35, 0.1); }

.end-state {
  color: #888;
  position: relative;
  padding-top: 20px;
}
.divider-line {
  height: 2px;
  background: var(--black);
  margin-bottom: 10px;
  opacity: 0.2;
}

/* --- 响应式 --- */
@media (max-width: 768px) {
  .log-header { flex-direction: column; align-items: flex-start; gap: 20px; }
  .header-right { border-left: none; padding-left: 0; border-top: 2px solid #eee; padding-top: 10px; width: 100%; }
  .stream-container { padding-left: 15px; }
  .timeline-bar { width: 2px; left: 0; }
  
  /* 移动端调整连接线 */
  :deep(.cyber-node-item::before) { width: 15px; left: -17px; }
  :deep(.cyber-node-item::after) { left: -21px; width: 8px; height: 8px; }
}

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@keyframes spin { to { transform: rotate(360deg); } }
</style>