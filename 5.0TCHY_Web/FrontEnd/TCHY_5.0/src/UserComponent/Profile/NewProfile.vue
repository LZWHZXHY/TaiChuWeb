<template>
  <div class="user-profile-terminal">
    <div class="bg-decoration">SYSTEM_READY // 2026</div>

    <header class="terminal-header">
      <div class="header-left">
        <div class="brand-block">
          <span class="logo-box">T</span>
          <span class="brand-text">用户终端 // USER_CENTER</span>
        </div>
        <div class="path-bread">
          <span class="root">系统</span>
          <span class="sep">></span>
          <span class="root">档案库</span>
          <span class="sep">></span>
          <span class="current blink">{{ user.username }}</span>
        </div>
      </div>
      <div class="header-right">
        <div class="status-indicator">
          <span class="dot"></span> 
          <span class="status-text">联机状态: 在线</span>
        </div>
        <button class="sys-btn" @click="goBack">
          [ ESC ] 返回上级
        </button>
      </div>
    </header>

    <div class="main-layout">
      <div class="center-container">
        
        <div class="top-section slide-in-item" style="--delay: 0.1s">
          <div class="id-wrapper">
            <Idcard 
              :user="user" 
              :userID="userID" 
              :achievements="achievements" 
              :isFollowing="isFollowing" 
              :showIdArchive="showIdArchive"
              @toggleIdArchive="toggleIdArchive"
              @toggleFollow="toggleFollow"
              @handleMessage="handleMessage"
              @goToUserSettings="GoToUserSettings"
            />
            
            <div class="datacard-container">
               <DataCard :user="user" />
            </div>
          </div>
          
          <div class="feature-wrapper cyber-card">
            <div class="featured-wrapper-inner">
             <FeaturedCard :data="mockData" />
            </div>

            <div class="terminal-link-runner">
               <div class="runner-header">
                <div class="runner-title">
                  <span class="icon">⚡</span> NET_JUMP // 外部链路
                </div>
                <div class="runner-status" :class="{ 'active': isRunning }">
                  {{ isRunning ? 'EXECUTING...' : 'IDLE' }}
                </div>
              </div>

              <div class="runner-log" ref="logContainer">
                <div v-for="(log, index) in logs" :key="index" class="log-line" :style="{ color: log.color }">
                  <span class="log-prefix">{{ log.prefix }}</span>
                  <span class="log-content">{{ log.text }}</span>
                </div>
              </div>

              <div class="runner-input-box">
                <span class="prompt">root@link:~$</span>
                <input 
                  type="text" 
                  v-model="terminalInput" 
                  @keyup.enter="executeLinkCommand"
                  :disabled="isRunning"
                  placeholder="输入目标代号 (QQ / Bili / Douyin / XHS)..."
                  autocomplete="off"
                />
                <div class="blinking-cursor">_</div>
              </div>
            </div>
          </div>
        </div>

        <div class="bottom-section slide-in-item cyber-card" style="--delay: 0.3s">
          <ProfileMain />
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
// ... (imports 保持不变) ...
import { ref, reactive, nextTick } from 'vue'
import { useRouter } from 'vue-router' 
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'
import ProfileMain from '@/UserComponent/Profile/ProfileMainLeft/ProfileMainLeft.vue';
import Idcard from '@/UserComponent/Profile/Idcard.vue';
import DataCard from '@/UserComponent/Profile/DataCard.vue'; 
import FeaturedCard from '@/UserComponent/Profile/FeaturedCard.vue';

const authStore = useAuthStore()
const { userID } = storeToRefs(authStore)

// ... (其余逻辑保持不变) ...
const router = useRouter() 
const isFollowing = ref(false)
const showIdArchive = ref(false)

const GoToUserSettings = () => {
  router.push('/profile/Usersettings')
}

// ... (user reactive 对象保持不变) ...
const user = reactive({
  username: 'USER_114514',
  uid: '89757-X',
  nickname: 'No Name', 
  role: '视觉前端 // VISUAL_ENG',
  level: 42,
  avatar: authStore.user?.logo || 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
  bio: '原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼。',
  tags: ['界面设计', 'Vue开发', '三维艺术'],
  age: 24,
  gender: 'M/F',
  location: 'Guangzhou, CN',
  creationAge: '4年',
  email: 'fengfengzi@cyber.com',
  qq: '1145141919',
  externalLinks: [
    { name: 'GitHub', url: 'https://github.com' },
    { name: 'Bilibili', url: 'https://bilibili.com' },
    { name: 'Dribbble', url: 'https://dribbble.com' }
  ],
  stats: { likes: 12450, views: 89000, works: 142, followers: 3500 }
})

// ... (mockData, terminal logic 保持不变) ...
const mockData = {
    // ...
    works: [
    { id: 1, title: 'NEURAL_LINK', category: 'INTERFACE', color: '#ff0055', cover: 'https://images.unsplash.com/photo-1550751827-4bd374c3f58b?q=80&w=2070&auto=format&fit=crop' },
    { id: 2, title: 'GHOST_SHELL', category: '3D_MODEL', color: '#00ccff', cover: 'https://images.unsplash.com/photo-1535295972055-1c762f4483e5?q=80&w=1887&auto=format&fit=crop' },
    { id: 3, title: 'HYPER_DRIVE', category: 'MOTION', color: '#ffcc00', cover: 'https://images.unsplash.com/photo-1614728263952-84ea256f9679?q=80&w=1908&auto=format&fit=crop' },
    { id: 4, title: 'SYS_CORE', category: 'BACKEND', color: '#00ff99' }
  ],
  blogs: [
    { id: 1, title: 'Vue3 响应式系统深度解析与重构', date: '2026.01.15' },
    { id: 2, title: '赛博朋克UI设计指南：从理论到实践', date: '2026.01.12' }
  ],
  posts: [
    { id: 1, title: '新装备入手，性能提升200%', date: '2h ago', image: 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?q=80&w=2070&auto=format&fit=crop' },
    { id: 2, title: '凌晨四点的代码构建...', date: '5h ago' }
  ],
  setting: {
    name: '彼岸宇宙',
    image: 'https://images.unsplash.com/photo-1462331940025-496dfbfc7564?q=80&w=2111&auto=format&fit=crop'
  }
}

const terminalInput = ref('')
const isRunning = ref(false)
const logs = ref([
  { prefix: '[SYS]', text: 'Terminal ready. Waiting for input...', color: '#666' }
])
const logContainer = ref(null)

const linkMap = {
  qq: { url: 'tencent://message/?uin=1145141919', color: '#0099FF' }, 
  bilibili: { url: 'https://space.bilibili.com/3493105678420282', color: '#FF6699' },
  b站: { url: 'https://space.bilibili.com/393582103', color: '#FF6699' },
  哔哩哔哩: { url: 'https://space.bilibili.com/393582103', color: '#FF6699' },
  douyin: { url: 'https://www.douyin.com/', color: '#FFFFFF' },
  抖音: { url: 'https://www.douyin.com/', color: '#FFFFFF' },
  xiaohongshu: { url: 'https://www.xiaohongshu.com/explore', color: '#FF2442' },
  小红书: { url: 'https://www.xiaohongshu.com/explore', color: '#FF2442' },
  weibo: { url: 'https://weibo.com/', color: '#E6162D' },
  微博: { url: 'https://weibo.com/', color: '#E6162D' }
}

const generateTechJargon = () => {
  const hex = Math.random().toString(16).substring(2, 10).toUpperCase();
  const verbs = ['ALLOC', 'PARSING', 'BYPASS', 'INJECT', 'DECODE', 'PING', 'TRACE', 'MOUNT'];
  const nouns = ['MEMORY', 'TCP_PACKET', 'FIREWALL', 'PROXY', 'BUFFER', 'KERNEL', 'NODE', 'STACK'];
  const verb = verbs[Math.floor(Math.random() * verbs.length)];
  const noun = nouns[Math.floor(Math.random() * nouns.length)];
  return `0x${hex} // ${verb} > ${noun}... OK`;
}

const scrollToBottom = () => {
  nextTick(() => {
    if (logContainer.value) {
      logContainer.value.scrollTop = logContainer.value.scrollHeight;
    }
  })
}

const executeLinkCommand = async () => {
  const key = terminalInput.value.trim().toLowerCase();
  if (!key) return;
  
  let target = linkMap[key];
  
  isRunning.value = true;
  logs.value = []; 
  
  logs.value.push({ prefix: '[CMD]', text: `Run: "${key}"`, color: '#fff' });
  scrollToBottom();
  await new Promise(r => setTimeout(r, 200));

  if (!target) {
    logs.value.push({ prefix: '[ERR]', text: `TARGET_NOT_FOUND: ${key}`, color: 'red' });
    logs.value.push({ prefix: '[SYS]', text: 'Available: QQ, Bili, Douyin, XHS...', color: '#666' });
    isRunning.value = false;
    terminalInput.value = '';
    scrollToBottom();
    return;
  }

  const steps = 12; 
  for (let i = 0; i < steps; i++) {
    logs.value.push({ 
      prefix: `[${Math.floor(Math.random()*99)}ms]`, 
      text: generateTechJargon(), 
      color: '#444' 
    });
    scrollToBottom();
    await new Promise(r => setTimeout(r, Math.random() * 40 + 20));
  }

  logs.value.push({ prefix: '[SUCC]', text: `Protocol Handshake: ${target.url.substring(0, 20)}...`, color: '#00ff00' });
  scrollToBottom();
  await new Promise(r => setTimeout(r, 600));

  logs.value.push({ prefix: '[SYS]', text: 'REDIRECTING NOW...', color: target.color });
  scrollToBottom();
  
  setTimeout(() => {
    window.open(target.url, '_blank');
    terminalInput.value = '';
    isRunning.value = false;
    logs.value.push({ prefix: '[SYS]', text: 'Session Closed. Ready.', color: '#666' });
    scrollToBottom();
  }, 800);
}

const goBack = () => {
  if (window.history.length > 1) {
    window.history.back()
  } else {
    alert('正在返回根系统...\n[System]: Redirecting to Root...')
  }
}
const toggleFollow = () => isFollowing.value = !isFollowing.value
const handleMessage = () => {
  alert(`正在建立与 ${user.username} 的加密通道...\n[System]: Encryption handshake initiated.`)
}
const toggleIdArchive = () => showIdArchive.value = !showIdArchive.value
</script>

<style scoped>
/* ... (原有的 CSS 保持不变，仅修改 datacard-container) ... */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Caveat:wght@700&display=swap');

:root {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #ccc;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --sans: 'Noto Sans SC', sans-serif;
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

.user-profile-terminal {
    /* ... 保持不变 ... */
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #CFCBC0;
  
  width: 100%; 
  min-height: 100vh;
  background-color: transparent; 
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
  position: relative;
  overflow-x: hidden;
}

.bg-decoration {
  position: fixed;
  bottom: 20px;
  right: 20px;
  font-family: var(--heading);
  font-size: 5rem;
  color: rgba(0,0,0,0.03);
  z-index: 0;
  pointer-events: none;
  user-select: none;
}

.terminal-header {
  height: 64px;
  background: var(--black);
  color: var(--white);
  display: flex; 
  justify-content: space-between; 
  align-items: center;
  padding: 0 24px;
  border-bottom: 4px solid var(--red);
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}

.header-left, .header-right { 
  display: flex; align-items: center; gap: 20px; 
}

.brand-block { 
  display: flex; align-items: center; gap: 12px; 
  font-weight: 700; font-family: var(--sans); letter-spacing: 0.5px;
}

.logo-box { 
  background: var(--white); color: var(--black); 
  width: 32px; height: 32px; 
  display: flex; align-items: center; justify-content: center; 
  font-family: var(--heading); font-size: 1.4rem; 
  transition: all 0.3s ease;
}

.brand-block:hover .logo-box {
  background: var(--red);
  color: var(--white);
  transform: rotate(180deg);
}

.path-bread { 
  font-size: 0.85rem; color: #888; display: flex; gap: 8px; font-family: var(--mono); 
}
.path-bread .current { 
  color: var(--red); font-weight: bold; 
}

.status-indicator { 
  font-size: 0.75rem; display: flex; align-items: center; gap: 8px; color: #00ff00; font-family: var(--mono);
}
.dot { 
  width: 8px; height: 8px; background: #00ff00; border-radius: 50%; 
  box-shadow: 0 0 8px #00ff00;
  animation: signal-pulse 2s infinite;
}

.sys-btn { 
  position: relative;
  background: transparent; 
  border: 1px solid #666; 
  color: #ccc; 
  padding: 6px 18px; 
  font-family: var(--mono); 
  cursor: pointer; 
  transition: 0.2s; 
  font-size: 0.75rem; 
  font-weight: bold; 
  overflow: hidden;
  text-transform: uppercase;
}
.sys-btn:hover { 
  border-color: var(--red); 
  color: var(--red); 
  background: rgba(217, 35, 35, 0.05); 
  box-shadow: 2px 2px 0 var(--red);
  transform: translate(-2px, -2px);
}
.sys-btn:active { 
  transform: translate(0, 0); 
  box-shadow: none;
}

.main-layout {
  flex: 1; 
  display: flex;
  justify-content: center;
  width: 100%;
  background-color: rgb(0, 0, 0);
  background-image: 
    radial-gradient(circle at 50% 50%, rgba(70, 70, 70, 0.8) 0%, rgba(240,237,230,1) 100%),
    linear-gradient(var(--gray-dark) 1px, transparent 1px),
    linear-gradient(90deg, var(--gray-dark) 1px, transparent 1px),
    linear-gradient(rgba(0,0,0,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.03) 1px, transparent 1px);
  background-size: 100% 100%, 50px 50px, 50px 50px, 10px 10px, 10px 10px;
  animation: grid-scroll 3s linear infinite;
}

.center-container {
  width: 65%;
  max-width: 1400px;
  min-width: 900px;
  height: auto;
  padding: 1% 0 80px 0;
  display: flex;
  flex-direction: column;
  gap: 0%;
}

.top-section {
  display: flex;
  gap: 24px;
  width: 100%;
  height: auto;
  padding-bottom: 20px;
  min-width: 50%;
}

.id-wrapper {
  flex-shrink: 0;
  width: 320px;
  height: auto;
  flex-direction: column;
  gap: 20px; 
  display: flex;
  align-self: flex-start;
  max-height: 600px;
}

/* --- 修改点: 去除边距，允许 flex 填充 --- */
.datacard-container {
  width: 100%;
  padding-top: 0; /* Removed padding */
  display: flex;
  flex-direction: column;
  flex: 1;             /* 自动填充剩余高度 */
  max-height: 46%;       /* 防止内容过多时撑破 Flex 容器 */
  display: flex;
  flex-wrap: wrap;
  
}

.feature-wrapper {
  display: flex;
  flex-direction: column;
  height: auto;
  min-width: 50%;
}
.featured-wrapper-inner {
  display: flex;
  flex-direction: column;
  height: auto;
  
  /* --- 新增：占据剩余空间，但保持底线宽度 --- */
  flex: 1;
  min-width: 350px; /* 当宽度小于 350px 时，强制换行到下面 */
  flex-wrap: wrap;
  min-height: 680px;
}


.bottom-section {
  width: 100%;
  height: auto; 
}

.cyber-card {
  background: transparent;
  border: 0px solid var(--black);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  position: relative;
  display: flex;
  flex-direction: column;
}

.cyber-card:hover {
  transform: translateY(-2px);
  border-color: var(--red);
}

.cyber-card::after {
  content: '';
  position: absolute;
  bottom: -1px; right: -1px;
  width: 12px; height: 12px;
  background: var(--black);
  clip-path: polygon(100% 0, 0 100%, 100% 100%);
  pointer-events: none;
}

.terminal-link-runner {
  height: 200px;
  width: 100%;
  background: rgba(10, 10, 10, 0.7);
  border-top: 1px solid #333;
  display: flex;
  flex-direction: column;
  padding: 12px;
  font-family: 'JetBrains Mono', monospace;
  position: relative;
  flex-shrink: 0;
  margin-top: 16px;
  box-shadow: 0 -4px 10px rgba(0,0,0,0.1);
}

.terminal-link-runner::before {
  content: '';
  position: absolute;
  top: -1px; left: 0;
  width: 40px; height: 1px;
  background: var(--red);
  box-shadow: 0 0 10px var(--red);
}

.runner-header {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  color: #666;
  margin-bottom: 8px;
  border-bottom: 1px dashed #333;
  padding-bottom: 4px;
}

.runner-status {
  font-weight: bold;
}
.runner-status.active {
  color: var(--red);
  animation: blink-anim 0.5s infinite;
}

.runner-log {
  flex: 1;
  overflow-y: auto;
  font-size: 0.8rem;
  line-height: 1.4;
  margin-bottom: 8px;
  scrollbar-width: none;
}
.runner-log::-webkit-scrollbar { display: none; }

.log-line {
  display: flex;
  gap: 8px;
}
.log-prefix { opacity: 0.7; min-width: 50px; }

.runner-input-box {
  display: flex;
  align-items: center;
  background: #111;
  padding: 4px 8px;
  border: 1px solid #333;
}
.runner-input-box:focus-within {
  border-color: #666;
}

.prompt {
  color: var(--red);
  font-weight: bold;
  margin-right: 8px;
  font-size: 0.9rem;
  user-select: none;
}

.runner-input-box input {
  background: transparent;
  border: none;
  color: var(--white);
  font-family: inherit;
  font-size: 0.9rem;
  flex: 1;
  outline: none;
}

.runner-input-box input::placeholder {
  color: #333;
  font-style: italic;
}

.blinking-cursor {
  color: var(--red);
  font-weight: bold;
  animation: blink-anim 1s step-end infinite;
}

@keyframes grid-scroll {
  0% { background-position: 0 0, 0 0, 0 0, 0 0, 0 0; }
  100% { background-position: 0 0, 50px 50px, 50px 50px, 0 0, 0 0; }
}

@keyframes signal-pulse {
  0% { opacity: 0.5; box-shadow: 0 0 2px #00ff00; }
  50% { opacity: 1; box-shadow: 0 0 10px #00ff00; }
  100% { opacity: 0.5; box-shadow: 0 0 2px #00ff00; }
}

.blink { animation: blink-anim 1s step-end infinite; }
@keyframes blink-anim {
  0%, 100% { opacity: 1; }
  50% { opacity: 0; }
}

.slide-in-item {
  opacity: 0;
  transform: translateY(30px);
  animation: slide-up-fade 2s cubic-bezier(0.2, 1, 0.3, 1) forwards;
  animation-delay: var(--delay, 0s);
}

@keyframes slide-up-fade {
  from { 
    opacity: 0; 
    transform: translateY(30px); 
  }
  to { 
    opacity: 1; 
    transform: translateY(0); 
  }
}

@media (max-width: 1024px) {
  .center-container { 
    width: 95%; 
    min-width: auto;
  }
  .top-section { 
    flex-direction: column; 
  }
  .id-wrapper { 
    width: 100%; 
  }
}
</style>