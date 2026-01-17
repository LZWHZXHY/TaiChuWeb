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
            <span>CORE_STATUS:</span>
            <span class="status-ok">OPERATIONAL</span>
          </div>
          <div class="status-row">
            <span>NODE_SYNC:</span>
            <span class="blink">{{ currentTime }}</span>
          </div>
        </div>
      </header>

      <div class="main-bridge">
        
        <main class="primary-stream">
          <div class="section-label">// FORMAL_ARCHIVE // 正式日志</div>
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
                <span class="spinner"></span> [ DOWNLOADING_PACKETS... ]
              </div>
              <div v-if="!hasMore && updateTree.length > 0" class="end-state">
                <div class="divider-line"></div>
                <span>// END_OF_STREAM // ALL_SYSTEMS_UPDATED</span>
              </div>
            </div>
          </div>
        </main>

        <aside class="technical-stream">
          <div class="section-label">// GIT_PUSH_STREAM // 技术流</div>
          <GitLogModule repo="LZWHZXHY/TaiChuWeb" :limit="12" />
          
          <div class="sidebar-decoration">
            <div class="terminal-text">> SYNC_STATUS: ACTIVE</div>
            <div class="terminal-text">> BRANCH: MAIN</div>
            <div class="terminal-text">> REGION: SHANGHAI_CN</div>
          </div>
        </aside>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import apiClient from '@/utils/api'
import LogNode from './LogNode.vue'
import GitLogModule from './GitLogModule.vue'

const updateTree = ref([])
const loading = ref(false)
const hasMore = ref(true)
const page = ref(1)
const pageSize = 20
const currentTime = ref(new Date().toLocaleTimeString())

async function loadUpdates() {
  if (!hasMore.value || loading.value) return
  loading.value = true
  try {
    const res = await apiClient.get('/Update/all', { params: { page: page.value, pageSize } })
    const arr = res.data?.data || []
    if (arr.length < pageSize) hasMore.value = false
    updateTree.value = [...updateTree.value, ...arr]
    page.value += 1
  } catch (e) {
    console.error("Fetch Error");
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

let timer;
onMounted(() => {
  loadUpdates()
  window.addEventListener('scroll', handleScroll)
  timer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString() }, 1000)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
  clearInterval(timer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.update-industrial {
  --red: #D92323; --black: #111111; --white: #F4F1EA; --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif;
  width: 100%; min-height: 100vh; background-color: var(--white); color: var(--black); font-family: var(--mono);
  position: relative; padding: 40px 0; overflow-x: hidden;
}

/* 布局控制 */
.log-wrapper { max-width: 1240px; margin: 0 auto; padding: 0 20px; z-index: 1; position: relative; }
.main-bridge { display: flex; gap: 40px; align-items: flex-start; }

.primary-stream { flex: 1; min-width: 0; }
.technical-stream { width: 340px; flex-shrink: 0; position: sticky; top: 40px; }

/* 装饰性标签 */
.section-label {
  font-size: 0.7rem; font-weight: bold; color: #999; margin-bottom: 20px;
  letter-spacing: 2px; border-bottom: 1px solid #ddd; padding-bottom: 5px;
}

/* 侧边栏装饰 */
.sidebar-decoration { margin-top: 20px; padding: 15px; background: var(--black); color: #444; font-size: 0.65rem; font-family: var(--mono); }
.terminal-text { margin-bottom: 4px; }

/* 正式日志流样式修复 */
.stream-container { position: relative; padding-left: 35px; }
.timeline-bar { position: absolute; left: 0; top: 0; bottom: 0; width: 4px; background: repeating-linear-gradient(to bottom, var(--black), var(--black) 10px, transparent 10px, transparent 20px); }

:deep(.cyber-node-item) {
  position: relative; border: 2px solid var(--black) !important; background: #fff !important;
  padding: 25px !important; margin-bottom: 30px; box-shadow: 6px 6px 0 rgba(0,0,0,0.1) !important;
}
:deep(.cyber-node-item::before) { content: ''; position: absolute; left: -37px; top: 35px; width: 35px; height: 2px; background: var(--black); }
:deep(.cyber-node-item::after) { content: ''; position: absolute; left: -42px; top: 30px; width: 12px; height: 12px; background: var(--red); border: 2px solid var(--black); z-index: 2; }

/* 头部样式保持原样... */
.log-header { border: 2px solid var(--black); background: #fff; padding: 25px 35px; display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; box-shadow: 10px 10px 0 rgba(0,0,0,0.15); }
.giant-text { font-family: var(--heading); font-size: 3.2rem; margin: 0; display: flex; align-items: center; gap: 15px; }
.deco-box { color: var(--red); }
.status-ok { background: var(--black); color: var(--white); padding: 0 8px; margin-left: 10px; font-weight: 800; }
.blink { animation: blink 1s infinite; color: var(--red); }

/* 背景网格 */
.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); background-size: 40px 40px; z-index: 0; }
.moving-grid { animation: gridScroll 60s linear infinite; }

@keyframes gridScroll { from { transform: translateY(0); } to { transform: translateY(-40px); } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

@media (max-width: 1100px) {
  .main-bridge { flex-direction: column; }
  .technical-stream { width: 100%; position: static; }
}
</style>