<template>
  <div class="battlefield-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="bf-container">
      <header class="tactical-header">
        <div class="header-left">
          <div class="logo-group">
            <span class="icon-box">太初战场</span>
            <div class="text-info">
              <h2 class="title">柴圈OC的大混乱企划</h2>
              <span class="sub">混合成长型企划平台 // 包含OC数据库</span>
            </div>
          </div>
        </div>
        
        <nav class="tactical-tabs">
          <button 
            v-for="t in tabs" 
            :key="t.key"
            class="cyber-tab-btn"
            :class="{ active: activeTab === t.key }"
            @click="activeTab = t.key"
          >
            <span class="tab-deco"></span>
            <span class="tab-text">{{ t.label }}</span>
          </button>
        </nav>
      </header>

      <div class="bf-main-viewport custom-scroll">
        <Transition name="glitch-fade" mode="out-in">
          
          <Battle_Intro 
            v-if="activeTab === 'intro'" 
            @switch-tab="handleSwitchTab"
          />

          <OC_Upload 
            v-else-if="activeTab === 'upload'" 
            @upload-success="onUploadSuccess"
          />

          <OC_Roster 
            v-else-if="activeTab === 'roster'" 
            :currentUserId="currentUserId"
            ref="rosterRef"
          />

          <OC_Battle_Logs 
            v-else-if="activeTab === 'records'" 
          />

        </Transition>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import Battle_Intro from '@/ArtCenter/BattleComponents/Battle_Intro.vue'
import OC_Upload from '@/ArtCenter/BattleComponents/OC_Upload.vue'
import OC_Roster from '@/ArtCenter/BattleComponents/OC_Roster.vue'
import OC_Battle_Logs from '@/ArtCenter/BattleComponents/OC_Battle_Logs.vue'

const tabs = [
  { key: 'intro', label: 'BRIEFING' },
  { key: 'upload', label: 'UPLOAD' },
  { key: 'roster', label: 'ROSTER' },
  { key: 'records', label: 'LOGS' }
]
const activeTab = ref('intro')
const currentUserId = ref(null)

// 简单的 JWT 解析获取 ID
const getUserFromToken = () => {
  const token = localStorage.getItem('auth_token')
  if (!token) return null
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(window.atob(base64))
    return parseInt(payload['nameid'] || payload['sub'] || payload['id'] || payload['UserId'] || 0)
  } catch (e) { return null }
}

const handleSwitchTab = (tab) => {
  activeTab.value = tab
}

const onUploadSuccess = () => {
  activeTab.value = 'roster'
  // 切换到 roster 后，组件挂载会自动刷新列表，无需额外操作
}

onMounted(() => {
  currentUserId.value = getUserFromToken()
})
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

:root {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --green: #2ecc71;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
}

/* 基础布局 */
.battlefield-industrial {
  width: 100%; height: 100%;
  font-family: var(--mono); color: var(--black);
  position: relative; overflow: hidden;
  display: flex; flex-direction: column;
}

.bf-container { position: relative; z-index: 1; flex: 1; display: flex; flex-direction: column; padding: 20px; gap: 20px; overflow: hidden; }
.bf-main-viewport { flex: 1; overflow-y: auto; padding-right: 5px; display: flex; flex-direction: column; }

/* 背景动画 */
.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); background-size: 40px 40px; z-index: 0; pointer-events: none; }
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* 通用组件：按钮 */
.cyber-btn { border: 2px solid var(--black); padding: 10px 20px; font-family: var(--heading); font-size: 1.1rem; cursor: pointer; background: var(--white); transition: 0.2s; display: inline-flex; align-items: center; justify-content: center; }
.cyber-btn:hover:not(:disabled) { box-shadow: 4px 4px 0 var(--black); transform: translate(-2px, -2px); }
.cyber-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover:not(:disabled) { background: var(--red); }
.cyber-btn.ghost { background: transparent; }
.cyber-btn.secondary { background: #fff; color: #000; }
.cyber-btn.large { font-size: 1.5rem; padding: 15px 30px; }
.cyber-btn.full { width: 100%; }

/* 通用组件：表单元素 */
.cyber-input, .cyber-select, .cyber-textarea { width: 100%; border: 2px solid var(--black); background: #fff; padding: 10px; font-family: var(--mono); outline: none; transition: 0.2s; box-sizing: border-box; }
.cyber-input:focus, .cyber-select:focus, .cyber-textarea:focus { background: #000; color: #fff; border-color: var(--red); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }

/* 通用组件：Checkbox */
.cyber-checkbox { display: flex; align-items: center; gap: 10px; cursor: pointer; font-family: var(--mono); }
.cyber-checkbox input { display: none; }
.checkmark { width: 20px; height: 20px; border: 2px solid var(--black); display: inline-block; position: relative; }
.cyber-checkbox input:checked + .checkmark { background: var(--red); }
.cyber-checkbox input:checked + .checkmark::after { content: '✓'; color: white; position: absolute; left: 3px; top: -2px; font-weight: bold; }

/* 通用组件：加载与空状态 */
.loading-state, .loading-modal { text-align: center; padding: 50px; font-weight: bold; color: #888; display: flex; flex-direction: column; align-items: center; justify-content: center; width: 100%; }
.spinner { width: 30px; height: 30px; border: 4px solid #ccc; border-top-color: var(--black); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 10px; }
@keyframes spin { to { transform: rotate(360deg); } }

/* 通用动画 */
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.98); }
.glitch-fade-leave-to { opacity: 0; transform: scale(1.02); }

/* 头部样式 (Specific to Layout) */
.tactical-header { background: var(--white); border: 4px solid var(--black); padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; box-shadow: 8px 8px 0 rgba(0,0,0,0.1); flex-shrink: 0; }
.logo-group { display: flex; align-items: center; gap: 15px; }
.icon-box { background: var(--black); color: var(--white); font-family: var(--heading); font-size: 1.5rem; padding: 5px 10px; }
.text-info h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-info .sub { font-size: 0.7rem; font-weight: bold; color: #666; }
.tactical-tabs { display: flex; gap: 10px; }
.cyber-tab-btn { background: transparent; border: 2px solid var(--black); padding: 8px 16px; cursor: pointer; display: flex; align-items: center; gap: 5px; font-family: var(--mono); font-weight: bold; transition: 0.2s; }
.cyber-tab-btn:hover { background: #eee; }
.cyber-tab-btn.active { background: var(--black); color: var(--white); box-shadow: 4px 4px 0 var(--red); }
.tab-deco { width: 6px; height: 6px; background: var(--red); display: none; }
.cyber-tab-btn.active .tab-deco { display: block; }

@media (max-height: 1080px) {
  .bf-container{ margin: 2% 0 10%; }
}
</style>