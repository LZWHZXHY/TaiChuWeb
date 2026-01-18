<template>
  <div class="user-profile-terminal">
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
          <span class="dot"></span> 联机状态: 在线
        </div>
        <button class="sys-btn" @click="goBack">
          [ ESC ] 返回上级
        </button>
      </div>
    </header>

    <div class="main-layout">
      <aside class="sidebar-left custom-scroll">
        <div class="id-flip-wrapper" :class="{ 'is-flipped': showIdArchive }">
          <div class="id-flipper">
            <!-- 卡片正面（保留原有内容） -->
            <div class="id-face id-front">
              <div class="cyber-card id-card-content">
                <button class="settings-trigger-btn" title="用户资料设置" @click="GoToUserSettings">
                  <span class="icon">⚙</span>
                </button>
                <div class="menu-row" @click="goToSettings">
                  <span class="row-icon"></span>
                  <span class="row-label">[ID:{{ userID }}]</span>
                </div>

                <button class="flip-trigger-btn" @click="toggleIdArchive" title="查看详细资料">
                  <span class="icon">▶</span>
                  <span class="corner-deco"></span>
                </button>

                <div class="card-deco-top"></div>
                <div class="avatar-frame">
                  <img :src="user.avatar" alt="avatar" />
                  <div class="corner-brackets"></div>
                  <div class="level-badge">LV.{{ user.level }}</div>
                </div>
                <div class="id-info">
                  <h1 class="user-name">{{ user.nickname }}</h1>
                  <div class="user-role">
                    <span class="hash">#</span> {{ user.role }}
                  </div>
                  <p class="bio-text">
                    {{ user.bio || '暂无个人简介数据...' }}
                  </p>
                  <div class="meta-tags">
                    <span class="tag" v-for="tag in user.tags" :key="tag">{{ tag }}</span>
                  </div>
                </div>
                <div class="action-row">
                  <button 
                    class="action-btn" 
                    :class="isFollowing ? 'active-state' : 'primary'" 
                    @click="toggleFollow"
                  >
                    {{ isFollowing ? '✓ 已关注' : '关注 + FOLLOW' }}
                  </button>
                  <button class="action-btn" @click="handleMessage">
                    私信 // MSG
                  </button>
                </div>
              </div>
            </div>

            <!-- 卡片背面：替换为拆分后的组件 -->
            <div class="id-face id-back">
              <IdArchiveCard :user="user" @close="toggleIdArchive" />
            </div>
          </div>
        </div>

        <div class="cyber-card stats-card">
          <div class="panel-header">
            <span class="deco">📊</span> 数据概览 // METRICS
          </div>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="label">获赞数</span>
              <span class="val">{{ formatNumber(user.stats.likes) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">浏览量</span>
              <span class="val">{{ formatNumber(user.stats.views) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">作品数</span>
              <span class="val">{{ user.stats.works }}</span>
            </div>
            <div class="stat-item">
              <span class="label">粉丝数</span>
              <span class="val">{{ formatNumber(user.stats.followers) }}</span>
            </div>
          </div>
        </div>
        
        <div class="cyber-card achievement-card">
          <div class="panel-header">
            <span class="deco">🏆</span> 成就系统 // MEDALS
          </div>
          <div class="achieve-list">
            <div 
              v-for="ach in achievements" 
              :key="ach.id" 
              class="ach-item" 
              :class="{ locked: !ach.unlocked }"
            >
              <div class="ach-icon">{{ ach.icon }}</div>
              <div class="ach-info">
                <div class="ach-name">{{ ach.name }}</div>
                <div class="ach-desc">{{ ach.desc }}</div>
              </div>
              <div class="lock-status" v-if="!ach.unlocked">🔒</div>
            </div>
          </div>
        </div>
      </aside>

      <div class="content-area-left">
        <ProfileMain />
      </div>
      <div class="content-area-right">
        <!-- 修复1：传递正确的form属性 + 绑定update事件 -->
        <RightConfigSection :form="rightForm" @update:form="updateRightForm" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed} from 'vue'
import { useRouter } from 'vue-router' 
import ProfileMain from '@/UserComponent/Profile/ProfileMainLeft/ProfileMainLeft.vue';
import apiClient from '@/utils/api';
import IdArchiveCard from '@/UserComponent/Profile/IdArchiveCard.vue';
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'
import RightConfigSection from '@/UserComponent/Profile/ProfileMainRight/RightConfigSection.vue';
const authStore = useAuthStore()

const { userID } = storeToRefs(authStore)

console.log(userID.value)
// --- Data & State ---
const router = useRouter() 
const isFollowing = ref(false)
const showIdArchive = ref(false)

// 跳转用户设置
const GoToUserSettings = () => {
  router.push('/profile/Usersettings')
}
const goToSettings = () => GoToUserSettings()

// 用户数据（保留原有内容）
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

// 修复2：初始化RightConfigSection需要的form数据结构（匹配子组件需求）
const rightForm = reactive({
  works: [null, null, null, null], // 4个作品槽位
  articles: [null, null, null, null], // 4个文章槽位
  achievements: [null, null, null, null, null, null, null, null], // 8个成就槽位
  inventory: [null, null, null, null, null, null, null, null] // 8个展示柜槽位
})

// 修复3：定义form更新方法
const updateRightForm = (newForm) => {
  Object.assign(rightForm, newForm)
}

const achievements = ref([
  { id: 1, name: '早期开拓者', desc: '在2023年前注册加入网络', icon: '⚡', unlocked: true },
  { id: 2, name: '高产机器', desc: '累计发布超过100个作品', icon: '📦', unlocked: true },
  { id: 3, name: '万人瞩目', desc: '拥有超过10,000名关注者', icon: '👑', unlocked: false },
])

// 工具方法（保留原有内容）
const formatNumber = (num) => {
  return num > 1000 ? (num / 1000).toFixed(1) + 'k' : num
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

/* --- 全局 Box Sizing 重置 --- */
* {
  box-sizing: border-box;
}

/* --- 全局变量 --- */
.user-profile-terminal {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #333;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --sans: 'Noto Sans SC', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  --hand: 'Caveat', cursive;

  width: 100%; 
  max-width: 100vw;
  height: 100%; /* 修复4：设置为100vh确保容器高度铺满视口 */
  overflow-x: hidden;
  overflow-y: auto; /* 修复5：改为auto避免内容被完全隐藏 */
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
}

/* 1. Header 样式（保留原有） */
.terminal-header {
  height: 60px;
  background: var(--black);
  color: var(--white);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 20px;
  border-bottom: 4px solid var(--red);
  flex-shrink: 0;
}
.header-left { display: flex; align-items: center; gap: 20px; }
.brand-block { display: flex; align-items: center; gap: 10px; font-weight: bold; font-family: var(--sans); }
.logo-box { background: var(--white); color: var(--black); width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-family: var(--heading); font-size: 1.2rem; }
.path-bread { font-size: 0.8rem; color: #aaa; display: flex; gap: 5px; font-family: var(--sans); }
.path-bread .current { color: var(--red); font-weight: bold; }
.header-right { display: flex; align-items: center; gap: 20px; font-family: var(--sans); }
.status-indicator { font-size: 0.75rem; display: flex; align-items: center; gap: 6px; color: #00ff00; }
.dot { width: 8px; height: 8px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.sys-btn { background: transparent; border: 1px solid #666; color: #ccc; padding: 5px 15px; font-family: var(--sans); cursor: pointer; transition: 0.2s; font-size: 0.75rem; font-weight: bold; }
.sys-btn:hover { border-color: var(--red); color: var(--red); background: rgba(217, 35, 35, 0.1); }
.sys-btn:active { transform: scale(0.95); }

/* 2. Main Layout 样式（保留原有） */
.main-layout {
  flex: 1;
  display: flex;
  overflow: hidden; 
  padding: 20px;
  padding-top: 0%;
  padding-bottom: 0%;
  gap: 20px;
  background-image: 
    linear-gradient(#ccc 1px, transparent 1px),
    linear-gradient(90deg, #ccc 1px, transparent 1px);
  background-size: 40px 40px;
  width: 100%;
  height: 100%; /* 修复6：计算高度（总高度 - header高度） */
}

/* 3. Sidebar 样式（保留原有） */
.sidebar-left {
  width: 320px;
  display: flex; flex-direction: column; gap: 20px;
  overflow-y: auto;
  padding-right: 5px; 
  flex-shrink: 0;
  padding-top: 1%;
  padding-bottom: 1%;
  border-top: #000000 0px solid;
  height: 100%;
}

/* ID Card Flip 逻辑（保留原有） */
.id-flip-wrapper {
  perspective: 1200px;
  width: 100%;
  position: relative;
  z-index: 10;
}
.id-flipper {
  width: 100%;
  position: relative;
  transform-style: preserve-3d;
  transition: transform 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.id-flip-wrapper.is-flipped .id-flipper {
  transform: rotateY(180deg);
}
.id-face {
  backface-visibility: hidden;
  width: 100%;
}
.id-front {
  position: relative;
  z-index: 2;
}
.id-back {
  position: absolute;
  top: 0; left: 0;
  height: 100%;
  transform: rotateY(180deg);
  z-index: 1;
  display: flex;
}

/* ID Card 正面样式（保留原有） */
.cyber-card {
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1);
  padding: 0;
  position: relative;
  transition: transform 0.2s;
}
.id-card-content { padding: 25px; text-align: center; height: 100%; }
.card-deco-top { height: 10px; background: repeating-linear-gradient(45deg, var(--black), var(--black) 5px, transparent 5px, transparent 10px); position: absolute; top:0; left:0; width:100%; opacity: 0.1; }

/* 翻转按钮样式（保留原有） */
.flip-trigger-btn {
  position: absolute;
  top: 10px; right: 10px;
  width: 32px; height: 32px;
  background: var(--black);
  color: var(--white);
  border: none;
  cursor: pointer;
  z-index: 5;
  display: flex; align-items: center; justify-content: center;
  transition: 0.2s;
}
.flip-trigger-btn:hover { background: var(--red); transform: rotate(180deg); }
.flip-trigger-btn .icon { font-size: 1rem; line-height: 1; }
.corner-deco { position: absolute; bottom: -4px; left: -4px; width: 8px; height: 8px; border-bottom: 2px solid var(--black); border-left: 2px solid var(--black); }

/* 1. 优化后的关键帧动画（移除空闲时间，仅保留实际动画部分） */
@keyframes move-rotate-loop {
  0% {
    transform: translateY(0) rotate(0deg); /* 初始状态：原地、无旋转 */
  }
  50% {
    transform: translateY(100px) rotate(180deg); /* 中途：下移100px、旋转180° */
  }
  75% {
    transform: translateY(100px) rotate(180deg); /* 停顿：保持下移旋转状态 */
  }
  100% {
    transform: translateY(0) rotate(360deg); /* 复位：回到原地、旋转360° */
  }
}

/* 2. 设置按钮样式（核心修改：hover时暂停动画，优化过渡） */
.settings-trigger-btn {
  position: absolute;
  top: 10px; left: 10px;
  width: 32px; height: 32px;
  background: var(--black);
  color: var(--white);
  border: none;
  cursor: pointer;
  z-index: 5;
  display: flex; align-items: center; justify-content: center;
  /* 优化transition：仅针对背景色过渡，避免干扰transform（动画/hover） */
  transition: background 0.2s ease;
  /* 优化后的动画配置：减少动画时长，使用delay创建循环间隔 */
  animation: move-rotate-loop 2s ease-in-out 8s infinite;
  transform: translateZ(0);
}

/* 3. hover状态：暂停动画 + 保留原有悬浮效果，无冲突 */
.settings-trigger-btn:hover {
  background: var(--red);
  /* 核心：暂停动画，让动画不再修改transform，hover样式正常生效 */
  animation-play-state: paused;
  /* 悬浮旋转效果：覆盖动画当前的transform，优先级正常 */
  transform: rotate(90deg);
  /* 可选：添加悬浮旋转的过渡，让切换更流畅 */
  transition: background 0.2s ease, transform 0.2s ease;
}

.settings-trigger-btn .icon {
  font-size: 1.2rem;
  line-height: 1;
}
/* 头像区域样式（保留原有） */
.avatar-frame { width: 120px; height: 120px; margin: 0 auto 20px; position: relative; border: 2px solid var(--black); padding: 4px; }
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(20%); }
.level-badge { position: absolute; bottom: -10px; left: 50%; transform: translateX(-50%); background: var(--red); color: white; padding: 2px 8px; font-size: 0.9rem; font-weight: bold; border: 2px solid var(--black); font-family: var(--heading); letter-spacing: 1px; }

/* 用户信息样式（保留原有） */
.user-name { font-family: var(--sans); font-weight: 800; font-size: 1.8rem; margin: 0; line-height: 1.2; color: var(--black); }
.user-role { color: var(--red); font-weight: bold; font-size: 0.85rem; margin-bottom: 15px; font-family: var(--sans); height: auto;}
.bio-text { font-size: 0.85rem; color: #555; margin-bottom: 20px; line-height: 1.6; border-top: 1px dashed #ccc; border-bottom: 1px dashed #ccc; padding: 10px 0; font-family: var(--sans); text-align: left;}
.meta-tags { display: flex; justify-content: center; gap: 5px; flex-wrap: wrap; margin-bottom: 20px; }
.tag { background: #eee; font-size: 0.7rem; padding: 2px 8px; border: 1px solid #ccc; font-family: var(--sans); }

/* 操作按钮样式（保留原有） */
.action-row { display: flex; gap: 10px; }
.action-btn { flex: 1; border: 2px solid var(--black); background: transparent; padding: 8px; font-family: var(--sans); font-weight: bold; cursor: pointer; transition: 0.2s; font-size: 0.8rem; }
.action-btn.primary { background: var(--black); color: var(--white); }
.action-btn.active-state { background: var(--white); color: var(--red); border-color: var(--red); }
.action-btn:hover { background: var(--red); color: var(--white); border-color: var(--black); }
.action-btn:active{ transform: translateY(2px); }

/* 数据概览卡片样式（保留原有） */
.panel-header { background: var(--black); color: var(--white); padding: 8px 12px; font-weight: bold; font-size: 0.9rem; display: flex; align-items: center; gap: 8px; font-family: var(--sans); }
.stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1px; background: var(--black); border: 2px solid var(--black); margin: 15px; }
.stat-item { background: var(--white); padding: 15px 10px; display: flex; flex-direction: column; align-items: center; }
.stat-item .label { font-size: 0.75rem; color: #888; font-weight: bold; margin-bottom: 5px; font-family: var(--sans); }
.stat-item .val { font-family: var(--heading); font-size: 1.4rem; line-height: 1; color: var(--black); }

/* 成就卡片样式（保留原有） */
.achieve-list { display: flex; flex-direction: column; }
.ach-item { display: flex; align-items: center; padding: 12px; border-bottom: 1px dashed #ccc; gap: 10px; }
.ach-item:last-child { border-bottom: none; }
.ach-icon { font-size: 1.5rem; }
.ach-info { flex: 1; }
.ach-name { font-weight: bold; font-size: 0.9rem; font-family: var(--sans); }
.ach-desc { font-size: 0.75rem; color: #888; font-family: var(--sans); margin-top: 2px; }
.ach-item.locked { opacity: 0.5; filter: grayscale(1); }

/* 内容区域样式（保留原有 + 修复溢出） */
.content-area-left {
  display: flex; flex-direction: column;
  background: var(--white);
  border: 0.1px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: auto; /* 修复7：改为auto允许内部滚动 */
  height: 97%;
  margin-top: 1%;
  width: 40%;
}
.content-area-right {
  display: flex; flex-direction: column;
  background: var(--white);
  border: 0.1px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: auto; /* 修复8：改为auto允许内部滚动 */
  height: 97%;
  margin-top: 1%;
  width: 40%;
}

/* 通用滚动条样式（保留原有） */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #f1f1f1; }
.custom-scroll::-webkit-scrollbar-thumb { background: #888; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }
</style>