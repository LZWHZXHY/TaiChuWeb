<template>
  <div class="settings-terminal">
    <header class="terminal-header">
      <div class="header-left">
        <div class="brand-block">
          <span class="logo-box">S</span>
          <span class="brand-text">配置中心 // CONFIG_SYS_V2</span>
        </div>
      </div>
      <div class="header-right">
        <div class="status-indicator">
          <span class="dot blink"></span> 联机状态: 在线
        </div>
        <button class="sys-btn warning" @click="goBack">
          [ ESC ] ABORT
        </button>
        <button class="sys-btn primary" @click="saveAll">
          [ ENTER ] COMMIT
        </button>
      </div>
    </header>

    <div class="main-layout">
      <main class="content-split">
        <div class="content-col left-col custom-scroll">
          <!-- 引入拆分后的子组件 -->
          <IdentitySection 
            v-model:avatar="form.avatar"
            v-model:nickname="form.nickname"
          />

          <BaseProfileSection
            v-model:bio="form.bio"
            v-model:location="form.location"
            v-model:role="form.role"
          />

          <TagsSection
            v-model:tags="form.tags"
            v-model:newTagInput="newTagInput"
          />

          <CommsSection
            v-model:email="form.email"
          />

          <LinksSection
            v-model:externalLinks="form.externalLinks"
          />

          <div class="end-marker">/// END OF BASIC_CONFIG ///</div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue'
// 引入拆分后的子组件
import IdentitySection from './IdentitySection.vue'
import BaseProfileSection from './BaseProfileSection.vue'
import TagsSection from './TagsSection.vue'
import CommsSection from './CommsSection.vue'
import LinksSection from './LinksSection.vue'
import { useAuthStore } from '@/utils/auth' // 确保路径正确
const authStore = useAuthStore()

onMounted(() => {
  document.body.style.overflow = 'hidden'
})

onUnmounted(() => {
  document.body.style.overflow = ''
})

// --- State ---
const newTagInput = ref('')

// --- Form Data ---
const form = reactive({
  avatar: authStore.user?.logo || 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
  nickname: '峰峰子',
  role: 'Visual Eng.',
  bio: 'System initialization complete. Waiting for input...',
  location: 'Guangzhou, CN',
  email: 'fengfengzi@cyber.com',
  tags: ['UI/UX', 'Vue3', 'Cyberpunk'],
  externalLinks: [
    { name: 'GitHub', url: 'https://github.com' },
    { name: 'Bilibili', url: 'https://bilibili.com' }
  ],
  works: [
    { id: 1, title: 'Project A', type: 'PRO' },
    null, null, null
  ],
  articles: [
    { id: 1, title: 'Vue3 Guide', type: 'DOC' },
    { id: 2, title: 'CSS Houdini', type: 'CSS' },
    null, null
  ],
  achievements: [
    '100 Days', 'Contributor', 'Bug Hunter', 'Sponsor',
    null, null, null, null
  ],
  inventory: [
    { name: 'Mech Key', rarity: 'legendary' },
    { name: 'Coffee', rarity: 'common' },
    null, null, null, null, null, null
  ]
})

// --- Methods ---
const goBack = () => {
  if (confirm('ABORT CHANGES?')) window.history.back()
}

const saveAll = () => {
  alert('SYSTEM_MSG: Configuration packets transmitted.')
}

// Theme Mock
const setTheme = (color) => {
  console.log(`System Accent Color set to: ${color}`)
}
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

/* --- 全局变量 & 重置 --- */
.settings-terminal {
  --red: #D92323;
  --black: #111111;
  --dark-grey: #1a1a1a;
  --light-grey: #f4f4f4;
  --green: #00ff41;
  --text-main: #111;
  --text-inv: #eee;

  --mono: 'JetBrains Mono', monospace;
  --sans: 'Noto Sans SC', sans-serif;
  --title: 'Anton', sans-serif;

  width: 98vw;
  height: 100vh;
  background: var(--light-grey);
  color: var(--text-main);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* --- 头部样式 --- */
.terminal-header {
  height: 60px;
  background: var(--black);
  color: var(--text-inv);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 20px;
  border-bottom: 4px solid var(--red);
  flex-shrink: 0;
  z-index: 10;
}
.brand-block { display: flex; align-items: center; gap: 10px; font-weight: bold; font-family: var(--mono); }
.logo-box { background: var(--text-inv); color: var(--black); width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-weight: bold; font-family: var(--title);font-size: 1.2rem;}
.header-right { display: flex; align-items: center; gap: 15px; font-size: 0.8rem; }
.status-indicator { font-size: 0.75rem; display: flex; align-items: center; gap: 6px; color: #00ff00; text-shadow: 0 0 2px #00ff00, 0 0 2px #00ff00, 0 0 2px #00ff00;}
.dot { width: 8px; height: 8px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.sys-btn {
  background: transparent; border: 1px solid #444; color: #888;
  padding: 6px 12px; font-family: var(--mono); cursor: pointer; transition: 0.2s; font-size: 0.75rem;
}
.sys-btn:hover { background: #333; color: #fff; }
.sys-btn.primary { border-color: var(--red); color: var(--red); font-weight: bold; }
.sys-btn.primary:hover { background: var(--red); color: white; }
.blink { animation: blink 1s infinite; }

/* --- 主布局 --- */
.main-layout { flex: 1; display: flex; overflow: hidden; position: relative; }

/* --- 内容区域 --- */
.content-split { flex: 1; display: flex; background: #ddd; gap: 2px; }
.content-col { flex: 1; background: var(--light-grey); overflow-y: auto; padding: 40px; position: relative; }
.left-col { background: #F4F1EA; background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px); background-size: 20px 20px; }

/* 结束标记 */
.end-marker { text-align: center; font-family: var(--mono); color: #bbb; margin-top: 40px; font-size: 0.7rem; letter-spacing: 2px; }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #413e3a; }

/* 闪烁动画 */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* 响应式 */
@media (max-width: 900px) {
  .main-layout { flex-direction: column; overflow-y: auto; }
  .content-split { flex-direction: column; }
  .content-col { overflow: visible; height: auto; }
  .settings-terminal { overflow-y: auto; height: auto; }
}
</style>