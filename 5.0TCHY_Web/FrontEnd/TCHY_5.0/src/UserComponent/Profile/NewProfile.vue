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
          </div>
          
          <div class="feature-wrapper cyber-card">
            <FeaturedCard :data="mockData" />
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
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router' 
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'
// 组件导入
import ProfileMain from '@/UserComponent/Profile/ProfileMainLeft/ProfileMainLeft.vue';
import Idcard from '@/UserComponent/Profile/Idcard.vue';
import FeaturedCard from '@/UserComponent/Profile/FeaturedCard.vue';

const authStore = useAuthStore()
const { userID } = storeToRefs(authStore)

const router = useRouter() 
const isFollowing = ref(false)
const showIdArchive = ref(false)

const GoToUserSettings = () => {
  router.push('/profile/Usersettings')
}

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

const achievements = ref([
  { id: 1, name: '早期开拓者', desc: '在2023年前注册加入网络', icon: '⚡', unlocked: true },
  { id: 2, name: '高产机器', desc: '累计发布超过100个作品', icon: '📦', unlocked: true },
  { id: 3, name: '万人瞩目', desc: '拥有超过10,000名关注者', icon: '👑', unlocked: false },
])

const mockData = {
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
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Caveat:wght@700&display=swap');

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
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #CFCBC0;
  
  width: 100%; 
  min-height: 100vh; /* 取消固定高度，允许自然延伸 */
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
  position: relative;
  overflow-x: hidden; /* 防止横向溢出 */

}

/* 背景装饰文字 */
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

/* --- 头部样式 --- */
.terminal-header {
  height: 64px;
  background: var(--black);
  color: var(--white);
  display: flex; 
  justify-content: space-between; 
  align-items: center;
  padding: 0 24px;
  border-bottom: 4px solid var(--red);
  /* 头部固定在顶部，随页面滚动分离 */
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

/* --- 主内容区域 --- */
.main-layout {
  flex: 1; /* 撑满剩余空间 */
  display: flex;
  justify-content: center;
  width: 100%;
  /* --- 背景设计升级 --- */
  background-color: rgb(0, 0, 0);
  background-image: 
    /* 1. 径向渐变 (Vignette) - 四周略暗，突出中间 */
    radial-gradient(circle at 50% 50%, rgba(70, 70, 70, 0.8) 0%, rgba(240,237,230,1) 100%),
    /* 2. 主网格 (大格子) */
    linear-gradient(var(--gray-dark) 1px, transparent 1px),
    linear-gradient(90deg, var(--gray-dark) 1px, transparent 1px),
    /* 3. 次网格 (小格子 - 增加精细度) */
    linear-gradient(rgba(0,0,0,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.03) 1px, transparent 1px);
    
  /* 双层背景尺寸：大格子50px，小格子10px */
  background-size: 100% 100%, 50px 50px, 50px 50px, 10px 10px, 10px 10px;
  
  /* 仅让大格子层移动 */
  animation: grid-scroll 3s linear infinite;
}

.center-container {
  width: 65%;
  max-width: 1400px;
  min-width: 900px;
  /* 移除高度限制，允许自然滚动 */
  height: auto;
  padding: 40px 0 80px 0; /* 底部增加padding，防止内容贴底 */
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* 顶部双栏结构 */
.top-section {
  display: flex;
  gap: 24px;
  width: 100%;
  align-items: stretch;
}

.id-wrapper {
  flex-shrink: 0;
  width: 320px;
}

.feature-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
}

/* 底部区域 */
.bottom-section {
  width: 100%;
  /* 取消最小高度限制，随内容撑开 */
  height: auto; 
}

/* --- 赛博卡片通用样式 --- */
.cyber-card {
  background: var(--white);
  border: 1px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.08);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  position: relative;
  display: flex;
  flex-direction: column;
}

.cyber-card:hover {
  transform: translateY(-2px);
  box-shadow: 12px 12px 0 rgba(0,0,0,0.12); /* 阴影加深 */
  border-color: var(--red);
}

/* 右下角工业标记装饰 */
.cyber-card::after {
  content: '';
  position: absolute;
  bottom: -1px; right: -1px;
  width: 12px; height: 12px;
  background: var(--black);
  clip-path: polygon(100% 0, 0 100%, 100% 100%);
  pointer-events: none;
}

/* --- 动画关键帧 --- */

/* 网格滚动：只移动背景层的第2/3层 (大网格) */
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

/* 入场动画 */
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

/* --- 响应式适配 --- */
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