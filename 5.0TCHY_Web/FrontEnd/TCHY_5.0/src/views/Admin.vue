<template>
  <div class="cyber-admin-root">
    <div class="grid-bg moving-grid"></div>

    <div class="panel-terminal custom-scroll">
      <header class="admin-header">
        <div class="header-split left">
          <h1 class="giant-text glitch-hover">
            <div class="text-row">COMMAND</div>
            <div class="text-row outline">CENTER</div>
            <div class="text-row red-fill">管理端</div>
          </h1>
        </div>
        <div class="header-split right">
          <div class="status-block">
            <h2 class="sys-title">// 系统权限：最高指挥 //</h2>
            <div class="live-indicator">
              <span class="dot"></span> NODE_ACTIVE
            </div>
            <div class="sys-time">{{ currentTime }}</div>
            <div class="auth-lines">
               <span>>> AUTH_LEVEL: Level_9</span>
               <span>>> ACTIVE_TAB: {{ activeTab.toUpperCase() }}</span>
            </div>
          </div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // ADMIN_ACCESS_GRANTED // SECURE_LINE_V5.0 // MONITORING_DATABASE // SYSTEM_STABLE // 
          // ADMIN_ACCESS_GRANTED // SECURE_LINE_V5.0 // MONITORING_DATABASE // SYSTEM_STABLE //
        </div>
      </div>

      <div class="main-bridge">
        <AdminLayout
          :active="activeTab"
          @change="activeTab = $event"
          @refresh="handleGlobalRefresh"
          class="heavy-layout"
        >
          <div class="admin-panel-container cyber-card">
            <div class="card-label-strip">
              <span>// OPERATIONAL_INTERFACE // {{ activeTab.toUpperCase() }}</span>
            </div>
            
            <Transition name="fade-slide" mode="out-in">
              <component
                :is="currentComponent"
                class="admin-panel"
                :key="activeTab"
              />
            </Transition>
          </div>
        </AdminLayout>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import AdminLayout from '@/adminComponents/AdminLayout.vue'
import ReviewSocieties from '@/adminComponents/ReviewContent.vue'
import UsersManagement from '@/adminComponents/UserManagement.vue'
import ReportsPanel from '@/adminComponents/ReportsPanel.vue'
import SettingsPanel from '@/adminComponents/SettingsPanel.vue'
import NotificationPanel from '@/adminComponents/NotificationPanel.vue'
import UpdatePanel from '@/adminComponents/UpdatePanel.vue'
import RulesPanel from '@/adminComponents/RulesPanel.vue'
import FeedBackPanel from '@/adminComponents/FeedBackPanel.vue'
import CalendarPanel from '@/adminComponents/CalendarPanel.vue'





const activeTab = ref('review')
const currentTime = ref(new Date().toLocaleTimeString())
let timer = null

const map = {
  review: ReviewSocieties,
  users: UsersManagement,
  reports: ReportsPanel,
  settings: SettingsPanel,
  notifications: NotificationPanel,
  updates: UpdatePanel,
  rules: RulesPanel,
  feedback: FeedBackPanel,
  calendar: CalendarPanel
}

const currentComponent = computed(() => map[activeTab.value] || ReviewSocieties)

function handleGlobalRefresh() {
  console.log(">> SYSTEM REFRESH INITIATED")
}

onMounted(() => {
  timer = setInterval(() => {
    currentTime.value = new Date().toLocaleTimeString()
  }, 1000)
})

onUnmounted(() => {
  clearInterval(timer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量：同步 ComCenter 工业赛博风格 --- */
.cyber-admin-root {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  height: 100vh;
  background-color: var(--off-white);
  color: var(--black);
  position: relative;
  overflow: hidden;
  font-family: 'Inter', sans-serif;
}

/* --- 背景与动画 --- */
.grid-bg {
  position: absolute; inset: 0;
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), 
                    linear-gradient(90deg, var(--gray) 1px, transparent 1px);
  background-size: 50px 50px;
  opacity: 0.4;
  z-index: 0;
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 
  0% { transform: translateY(0); } 
  100% { transform: translateY(-50px); } 
}

.panel-terminal {
  position: relative;
  z-index: 1;
  height: 100%;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

/* --- 头部设计 --- */
.admin-header {
  display: flex;
  border-bottom: 4px solid var(--black);
  background: var(--off-white);
}

.header-split.left {
  background: var(--black);
  color: var(--off-white);
  padding: 30px 50px;
  flex: 0 0 400px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.giant-text {
  font-family: var(--heading);
  font-size: 3.5rem;
  line-height: 0.9;
  text-transform: uppercase;
  transform: rotate(-2deg);
}

.text-row.outline {
  color: var(--black);
  -webkit-text-stroke: 1px var(--off-white);
}

.text-row.red-fill {
  color: var(--red);
  margin-left: 20px;
}

.header-split.right {
  flex: 1;
  padding: 30px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
}

.status-block { text-align: right; }
.sys-title { font-weight: 900; font-size: 1.2rem; margin-bottom: 5px; }
.live-indicator {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-family: var(--mono);
  font-size: 0.8rem;
  color: var(--red);
  border: 1px solid var(--red);
  padding: 4px 8px;
  background: rgba(217, 35, 35, 0.05);
}

.dot {
  width: 8px; height: 8px;
  background: var(--red);
  border-radius: 50%;
  animation: pulse 1s infinite;
}

.sys-time {
  font-family: var(--mono);
  font-weight: 700;
  font-size: 1.5rem;
  margin: 5px 0;
}

.auth-lines {
  font-family: var(--mono);
  font-size: 0.75rem;
  color: #666;
  display: flex;
  flex-direction: column;
}

/* --- 技术条 --- */
.tech-strip {
  background: var(--black);
  color: var(--off-white);
  padding: 8px 0;
  overflow: hidden;
  white-space: nowrap;
  font-family: var(--mono);
  font-size: 0.8rem;
  border-bottom: 4px solid var(--black);
}

.strip-content {
  display: inline-block;
  animation: marquee 25s linear infinite;
}

@keyframes marquee {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

/* --- 主布局区 --- */
.main-bridge {
  flex: 1;
  padding: 30px;
  max-width: 1800px;
  margin: 0 auto;
  width: 100%;
  box-sizing: border-box;
}

/* 强制穿透修改 AdminLayout 的外观（如果内部允许） */
:deep(.heavy-layout) {
  background: transparent !important;
}

.admin-panel-container {
  min-height: 70vh;
  margin-top: 20px;
}

.cyber-card {
  background: #fff;
  border: 2px solid var(--black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.15);
  display: flex;
  flex-direction: column;
}

.card-label-strip {
  background: var(--black);
  color: var(--off-white);
  padding: 6px 15px;
  font-family: var(--mono);
  font-size: 0.75rem;
  font-weight: bold;
}

.admin-panel {
  width: 100% !important;
  padding: 30px;
  box-sizing: border-box;
}

/* --- 动画 --- */
.fade-slide-enter-active, .fade-slide-leave-active {
  transition: all 0.3s ease;
}
.fade-slide-enter-from { opacity: 0; transform: translateY(10px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: var(--gray); }
</style>