<template>
  <aside class="sidebar-left custom-scroll">
    <div class="cyber-card">
      <div class="card-header-deco">
        <div class="hazard-stripes"></div>
        <div class="header-status">SYS_READY</div>
        <button class="icon-btn settings-btn" @click="emit('goToUserSettings')" title="设置">
          <span class="icon">⚙</span>
        </button>
      </div>

      <div class="card-scroll-content custom-scroll">
        
        <div class="identity-section">
          <div class="avatar-box">
            <img :src="userInfo.avatar" alt="avatar" />
          </div>
          
          <div class="identity-text">
            <h1 class="user-name">{{ userInfo.nickname }}</h1>
            <div class="badges-row">
              <span class="badge gender" :class="userInfo.gender">
                {{ userInfo.gender === '男' ? 'MALE-01' : (userInfo.gender === '女' ? 'FEMALE-02' : 'UNIT-X') }}
              </span>
              <span class="badge title">{{ userInfo.title }}</span>
            </div>
          </div>
        </div>

        <div class="data-grid">
          <div class="data-item">
            <span class="label">LOCATION_</span>
            <span class="value">{{ userInfo.location }}</span>
          </div>
          <div class="data-item">
            <span class="label">COMM_LINK_</span>
            <span class="value">{{ userInfo.contact }}</span>
          </div>
        </div>

        <div class="bio-section">
          <div class="bio-inner">
             <p class="bio-text">
              <span class="quote-mark">>></span>
              {{ userInfo.bio }}
            </p>
          </div>
        </div>

        <div class="tags-container">
          <span class="tag" v-for="tag in userInfo.tags" :key="tag">{{ tag }}</span>
        </div>

        <div class="divider-line"></div>

        <div class="links-module">
          <div class="links-header">
            <span class="deco-block"></span> EXTERNAL_PROTOCOLS
          </div>
          
          <div class="fixed-links-grid">
            <a 
              v-for="(link, index) in fixedLinks" 
              :key="index"
              :href="link.url" 
              target="_blank"
              class="link-box"
              :title="link.label"
            >
              <i class="link-icon">{{ link.icon }}</i>
              <div class="link-glow"></div>
            </a>
          </div>

          <button 
            v-if="extraLinks.length > 0"
            class="expand-links-btn" 
            :class="{ 'is-expanded': showAllLinks }"
            @click="showAllLinks = !showAllLinks"
          >
            <span class="btn-deco-l">///</span>
            <span>{{ showAllLinks ? 'RETRACT_LIST' : `ACCESS_ALL [${extraLinks.length}+]` }}</span>
            <span class="btn-deco-r">///</span>
          </button>

          <div class="expand-wrapper" :class="{ 'is-open': showAllLinks }">
            <div class="expanded-links-list">
              <a 
                v-for="(link, index) in extraLinks" 
                :key="'ex-'+index"
                :href="link.url"
                target="_blank"
                class="extra-link-row"
              >
                <span class="link-label">{{ link.label }}</span>
                <span class="link-arrow">➜</span>
              </a>
            </div>
          </div>
        </div>

      </div>

      <div class="card-footer-actions">
        <button 
          class="action-btn main-btn" 
          :class="{ 'active': isFollowing }"
          @click="handleFollowClick"
        >
          <div class="btn-grid-bg"></div>
          <div class="btn-content">
            <span v-if="!isFollowing" class="txt-normal">INITIATE_FOLLOW</span>
            <span v-else class="txt-active">
              <span class="status-dot"></span> LINKED: {{ formattedFansCount }}
            </span>
          </div>
        </button>

        <button class="action-btn sub-btn" @click="emit('handleMessage')">
          <span class="icon">✉</span>
        </button>
      </div>
      
    </div>
  </aside>
</template>

<script>
const MOCK_USER_DATA = {
  nickname: 'K-VAI',
  avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?ixlib=rb-4.0.3&auto=format&fit=crop&w=200&h=200&q=80',
  gender: 'Male', 
  title: 'NET_RUNNER',
  location: 'NIGHT_CITY // SEC-9',
  contact: 'K.VAI@NET.IO',
  bio: '神经漫游者。专注于虚拟架构与遗落数据的挖掘。如果不在线，请尝试连接 127.0.0.1。',
  tags: ['CYBERPUNK', 'VUE3', 'FULLSTACK', 'HACKER', 'DESIGN'],
  links: [
    { label: 'GitHub', url: 'https://github.com', icon: '©' },
    { label: 'Twitter', url: '#', icon: '𝕏' },
    { label: 'Discord', url: '#', icon: '⚡' },
    { label: 'Dribbble', url: '#', icon: '🏀' },
    { label: 'STEAM PROFILE', url: '#', icon: '' },
    { label: 'PIXIV', url: '#', icon: '' },
    { label: 'PERSONAL SITE', url: '#', icon: '' },
    { label: 'ENCRYPTED KEY', url: '#', icon: '' },
  ]
}
</script>

<script setup>
import { defineProps, defineEmits, ref, computed } from 'vue'

const props = defineProps({
  user: {
    type: Object,
    default: () => null 
  },
  followerCount: {
    type: Number,
    default: 89757
  },
  isFollowing: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits([
  'toggleFollow',
  'handleMessage',
  'goToUserSettings'
])

const showAllLinks = ref(false)

const userInfo = computed(() => {
  if (!props.user) return MOCK_USER_DATA
  if (Object.keys(props.user).length === 0) return MOCK_USER_DATA
  return { ...MOCK_USER_DATA, ...props.user }
})

const fixedLinks = computed(() => {
  const links = userInfo.value.links || []
  return links.slice(0, 4)
})

const extraLinks = computed(() => {
  const links = userInfo.value.links || []
  return links.slice(4)
})

const formattedFansCount = computed(() => {
  const num = props.followerCount
  if (num >= 10000) return (num / 10000).toFixed(1) + 'w'
  if (num >= 1000) return (num / 1000).toFixed(1) + 'k'
  return num
})

const handleFollowClick = () => {
  emit('toggleFollow')
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700;800&family=Chakra+Petch:wght@400;600;700&display=swap');

:root {
  /* 重工业调色板 */
  --c-bg: #141414;
  --c-panel: #1e1e1e;
  --c-border: #000000;
  
  /* 文字颜色：全面改为灰白色系 */
  --c-text-main: #f0f0f0;  /* 亮白 */
  --c-text-dim: #bbbbbb;   /* 灰白 */
  
  /* 引导重色 */
  --c-heavy-orange: #FF5500; 
  --c-hazard-yellow: #FFB300; 
  --c-alert-red: #D92323;
  --c-terminal-green: #0f0;
}

* { box-sizing: border-box; }

.sidebar-left {
  width: 320px;
  height: 97%;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  padding-right: 5px;
  background-color: transparent;
}

.cyber-card {
  width: 100%;
  height: 100%;
  background-color: var(--c-bg);
  background-image: linear-gradient(rgb(255, 255, 255, 0.05), rgba(20,20,20,0.95)), url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)' opacity='0.05'/%3E%3C/svg%3E");
  border: 3px solid var(--c-border);
  box-shadow: 8px 8px 0px rgba(0,0,0,0.5);
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
  font-family: 'JetBrains Mono', monospace;
  color: var(--c-text-main); /* 全局默认颜色设为亮色 */
}

/* --- 顶部装饰 (警示条风格) --- */
.card-header-deco {
  height: 48px;
  border-bottom: 3px solid var(--c-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #000;
  padding: 0;
  flex-shrink: 0;
  z-index: 2;
  position: relative;
}

.hazard-stripes {
  flex: 1;
  height: 100%;
  background: repeating-linear-gradient(
    45deg,
    #000,
    #000 10px,
    var(--c-hazard-yellow) 10px,
    var(--c-hazard-yellow) 20px
  );
  opacity: 0.8;
  filter: brightness(0.6) contrast(1.2);
}

.header-status {
  position: absolute;
  left: 10px;
  font-size: 0.7rem;
  font-weight: 800;
  background: #000;
  color: white;
  padding: 2px 6px;
  border: 1px solid var(--c-heavy-orange);
  z-index: 2;
}

.icon-btn {
  width: 48px;
  height: 100%;
  background: rgba(26, 26, 26, 0.8); 
  border: none; 
  border-left: 3px solid var(--c-border);
  color: #ccc; /* 图标颜色改为浅灰 */
  cursor: pointer; 
  font-size: 1.4rem; 
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  z-index: 2;
}
.icon-btn:hover { 
  background: white; 
  color: #000; /* 悬停时为了对比度，保持黑色 */
}

/* --- 内容区域 --- */
.card-scroll-content {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  padding-bottom: 0px;
  padding-top: 10px;
  position: relative;
  z-index: 1;
  background: rgba(0, 0, 0, 0.1);
}

/* --- 身份信息 --- */
.identity-section { display: flex; gap: 15px; margin-bottom: 5px; align-items: flex-start; }
.avatar-box {
  width: 80px; height: 80px; 
  border: 2px solid rgba(0, 0, 0, 0.589);
  position: relative; 
  background: #000; 
  flex-shrink: 0;
  overflow: hidden;
  transition: all 0.3s ease-in-out;
}
.identity-section { display: flex; gap: 15px; margin-bottom: 5px; align-items: flex-start; }
.avatar-box:hover{
  width: 100px; height: 100px; 
  border: 2px solid rgba(0, 0, 0, 0.589);
  position: relative; 
  background: #000; 
  flex-shrink: 0;
  overflow: hidden;
}
.avatar-box img { 
  width: 100%; height: 100%; object-fit: cover; 
  filter: grayscale(50%) contrast(1.2); 
  transition: 0.3s; 
}
.avatar-box:hover img { filter: grayscale(0%); }


@keyframes scan { 0% {top:0} 100% {top:100%} }


.identity-text { flex: 1; display: flex; flex-direction: column; gap: 8px; overflow: hidden; justify-content: center; height: 80px;}
.user-name {
  font-family: 'Chakra Petch', sans-serif; 
  font-weight: 700; font-size: 1.5rem;
  margin: 0; line-height: 1; color: #fff; /* 纯白 */
  text-transform: uppercase;
  letter-spacing: 1px;
  text-shadow: 2px 2px 0px #000;
}
.badges-row { display: flex; gap: 6px; flex-wrap: wrap; }
.badge { 
  font-size: 0.6rem; padding: 2px 6px; font-weight: bold; 
  border: 1px solid #555; background: #000; color: #ccc; /* 徽章文字改浅 */
}
.badge.title { 
  background: var(--c-panel); 
  border-color: var(--c-heavy-orange); 
  color: var(--c-heavy-orange); 
}

/* --- 数据终端格 --- */
.data-grid {
  display: grid; grid-template-columns: 1fr; gap: 2px; margin-bottom: 12px;
  background: rgba(0, 0, 0, 0.05); 
  border: 1px solid #333;
  padding: 4px;
}
.data-item { 
  display: flex; justify-content: space-between; align-items: center;
  background: #111; padding: 8px 10px;
}
.data-item .label { 
  font-size: 0.6rem; color: #aaa; /* 标签文字改浅灰 */
  font-weight: bold; letter-spacing: 1px;
}
.data-item .value { 
  font-size: 0.7rem; color: #fff; /* 值文字改纯白 */
  font-weight: normal; 
  font-family: 'JetBrains Mono', monospace;
}

/* --- 简介 --- */
.bio-section { margin-bottom: 20px; }
.bio-inner {
  background: rgba(255,255,255,0.03);
  border-left: 2px solid #555;
  padding: 10px;
}
.bio-text {
  font-family: 'Chakra Petch', sans-serif; font-size: 0.8rem; line-height: 1.6; 
  color: #d4d4d4; /* 简介文字由黑改灰白 */
  position: relative; margin: 0;
}
.quote-mark { color: var(--c-heavy-orange); margin-right: 5px; font-weight: bold;}

/* --- 标签 (金属铭牌风) --- */
.tags-container { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 24px; }
.tag { 
  font-size: 0.6rem; 
  color: #fff; /* 标签文字改白 */
  background: #444; /* 背景加深以保证对比度 */
  border: 1px solid #000; 
  padding: 3px 6px; 
  font-weight: 800;
  clip-path: polygon(10px 0, 100% 0, 100% 100%, 0 100%, 0 10px);
}

.divider-line {
  height: 4px; 
  background: repeating-linear-gradient(90deg, #333, #333 2px, transparent 2px, transparent 8px);
  margin: 10px 0 24px 0;
}

/* --- 链接模块 --- */
.links-module { margin-bottom: 5px; }
.links-header { 
  font-size: 0.7rem; font-weight: bold; color: #ccc; /* 标题改浅 */
  margin-bottom: 12px; display: flex; align-items: center; gap: 8px;
}
.deco-block { width: 8px; height: 8px; background: var(--c-heavy-orange); display: inline-block; }

.fixed-links-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 12px; }
.link-box {
  aspect-ratio: 1; 
  border: 2px solid #444; 
  display: flex; align-items: center; justify-content: center;
  background: #222; 
  color: #eee; /* 图标改白 */
  text-decoration: none; 
  position: relative;
  transition: all 0.2s cubic-bezier(0.25, 1, 0.5, 1);
  overflow: hidden;
}
/* 悬停 */
.link-box:hover { 
  background: rgba(255, 255, 255, 1);
  color: var(--c-heavy-orange); 
  transform: translateY(-2px); 
  box-shadow: 0 4px 12px rgba(255, 255, 255, 0.2);
}
.link-icon { font-style: normal; font-size: 1.4rem; z-index: 2; }

/* 展开按钮 */
.expand-links-btn {
  width: 100%; 
  border: 2px solid #949494; 
  background: #000000; 
  padding: 10px;
  color: #bbb; /* 按钮文字改浅 */
  font-family: 'JetBrains Mono', monospace; font-size: 0.7rem; font-weight: bold;
  cursor: pointer; text-align: center;
  display: flex; justify-content: space-between; align-items: center;
  transition: all 0.2s;
}
.expand-links-btn:hover { 
  border-color: #ffffffdc;
  color: #fff;
  background: #929292;
}
.expand-links-btn.is-expanded { 
  background: rgba(255, 255, 255, 0.767); 
  color: #000; 
  border-color: var(--c-hazard-yellow);
}
.btn-deco-l, .btn-deco-r { color: #666; font-size: 0.6rem; }
.is-expanded .btn-deco-l, .is-expanded .btn-deco-r { color: #000; }

.expand-wrapper {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 0.3s ease-out;
  border-left: 2px solid #333;
  margin-left: 10px;
}
.expand-wrapper.is-open { grid-template-rows: 1fr; }

.expanded-links-list {
  min-height: 0;
  overflow: hidden; 
  background: #111;
  border: 1px solid #333; border-top: none;
}
.extra-link-row {
  display: flex; justify-content: space-between; padding: 10px 15px; border-bottom: 1px solid #222;
  text-decoration: none; 
  color: #bbb; /* 链接文字改浅 */
  font-size: 0.75rem;
  transition: all 0.2s;
}
.extra-link-row:hover { 
  background: #e2e2e2; 
  color: var(--c-heavy-orange); 
  padding-left: 20px; 
}
.extra-link-row .link-arrow { opacity: 0; transition: 0.2s; }
.extra-link-row:hover .link-arrow { opacity: 1; }

/* --- 底部操作栏 --- */
.card-footer-actions {
  flex-shrink: 0; padding: 5px; 
  background: #111;
  border-top: 3px solid #000; 
  display: flex; gap: 12px; 
  z-index: 5;
}

/* 主按钮 - 保持橙底黑字 (工业标准) */
.action-btn {
  height: 50px; 
  border: none;
  cursor: pointer; 
  font-family: 'Chakra Petch', sans-serif; font-weight: 800;
  position: relative; 
  display: flex; align-items: center; justify-content: center;
  text-transform: uppercase;
  clip-path: polygon(10px 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%, 0 10px);
  transition: all 0.1s;
  color: #ffffff;
}

.main-btn { 
  flex: 1; 
  background: var(--c-heavy-orange); 
  color: #D92323; /* 橙色背景必须搭配黑字 */
  box-shadow: 4px 4px 0px #000; 
  border: #d9232367 2px solid;
  text-shadow: 
    0 0 5px #d9232356,
    0 0 10px #D92323,
    0 0 20px #D92323,
    0 0 40px rgba(217, 35, 35, 0.137),
    0 0 80px rgba(217, 35, 35, 0.6);
}
.main-btn:active { transform: translate(2px, 2px); box-shadow: 0 0 0;background: #ee0404cc; color: #ffffff;border: #ffffff 2px solid;}
.main-btn:hover { brightness: 1.1; background: #D92323; color: #ffffff;border: #D92323 2px solid;
    text-shadow: 
    0 0 5px #fff,
    0 0 10px #fff,
    0 0 20px #ffffff,
    0 0 40px rgba(255, 255, 255, 0.8),
    0 0 80px rgba(255, 255, 255, 0.6);
}

.main-btn.active { 
  font-size: 1.1rem;
  /* 渐变文字 */
  background: linear-gradient(90deg, #ffffff, #ffffffde);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent; /* 隐藏原文本色，显示渐变 */
  /* 搭配粉色系发光 */
  text-shadow: 0 0 10px rgba(255, 255, 255, 0.7), 0 0 20px rgba(255, 255, 255, 0.5);
  border: #ffffff 2px solid;
}
.main-btn.active .count { color: var(--c-heavy-orange); }
.status-dot { 
  display: inline-block; width: 8px; height: 8px; background: #0f0; border-radius: 50%; margin-right: 5px; box-shadow: 0 0 5px #0f0; 
}

/* 私信按钮 */
.sub-btn { 
  width: 50px; 
  background: #222; 
  color: #ccc; /* 图标改白 */
  border: 2px solid #333;
  font-size: 1.4rem;
}
.sub-btn:hover { 
  background: #fff; 
  color: #000; 
  border-color: #fff;
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-track { background: #111; }
.custom-scroll::-webkit-scrollbar-thumb { background: #444; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--c-heavy-orange); }
</style>