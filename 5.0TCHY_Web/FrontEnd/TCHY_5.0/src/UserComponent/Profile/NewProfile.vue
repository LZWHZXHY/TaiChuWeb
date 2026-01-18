<template>
  <div class="user-profile-terminal">
    <div class="crt-lines"></div>

    <header class="terminal-header">
      <!-- 替换后的 header-left 区域 -->
      <div class="header-left">
        <div class="brand-block">
          <div class="logo-box">
            <span class="glitch-text" data-text="T">T</span>
          </div>
          <div class="brand-info">
            <span class="brand-text">TERMINAL_CORE // USER_CENTER</span>
            <div class="load-bar"></div>
          </div>
        </div>
        <div class="path-bread">
          <span class="root">SYS</span>
          <span class="sep">>></span>
          <span class="root">ARCHIVE</span>
          <span class="sep">>></span>
          <span class="current blink">{{ user.username }}</span>
        </div>
      </div>
      <!-- 原有 header-right 保留不变 -->
      <div class="header-right">
        <div class="status-indicator">
          <span class="dot"></span> NET_STATUS: ONLINE
        </div>
        <button class="sys-btn" @click="goBack">
          <span class="btn-content">[ ESC ] BACK</span>
        </button>
      </div>
    </header>

    <div class="main-layout">
      <aside class="sidebar-left custom-scroll">
        <div class="id-flip-wrapper" :class="{ 'is-flipped': showIdArchive }">
          <div class="id-flipper">
            <div class="id-face id-front">
              <div class="cyber-card id-card-content">
                <button class="settings-trigger-btn" title="用户设置" @click="GoToUserSettings">
                  <span class="icon">⚙</span>
                </button>
                <div class="menu-row" @click="goToSettings">
                  <span class="row-icon"></span>
                  <span class="row-label">[ID:{{ userID }}]</span>
                </div>

                <button class="flip-trigger-btn" @click="toggleIdArchive" title="详细资料">
                  <span class="icon">▶</span>
                  <span class="corner-deco"></span>
                </button>

                <div class="card-deco-top"></div>
                <div class="avatar-frame">
                  <img :src="user.avatar" alt="avatar" />
                  <div class="scan-overlay"></div>
                  <div class="corner-brackets"></div>
                  <div class="level-badge">LV.{{ user.level }}</div>
                </div>
                <div class="id-info">
                  <h1 class="user-name">{{ user.nickname }}</h1>
                  <div class="user-role">
                    <span class="hash">#</span> {{ user.role }}
                  </div>
                  <p class="bio-text">
                    {{ user.bio || 'DATA NOT FOUND...' }}
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
                    {{ isFollowing ? '✓ SYNCED' : 'FOLLOW +' }}
                  </button>
                  <button class="action-btn" @click="handleMessage">
                    MSG // 私信
                  </button>
                </div>
              </div>
            </div>

            <div class="id-face id-back">
              <IdArchiveCard :user="user" @close="toggleIdArchive" />
            </div>
          </div>
        </div>

        <div class="cyber-card stats-card">
          <div class="panel-header">
            <span class="deco">📊</span> METRICS // 数据
          </div>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="label">LIKES</span>
              <span class="val">{{ formatNumber(user.stats.likes) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">VIEWS</span>
              <span class="val">{{ formatNumber(user.stats.views) }}</span>
            </div>
            <div class="stat-item">
              <span class="label">WORKS</span>
              <span class="val">{{ user.stats.works }}</span>
            </div>
            <div class="stat-item">
              <span class="label">FANS</span>
              <span class="val">{{ formatNumber(user.stats.followers) }}</span>
            </div>
          </div>
        </div>
        
        <div class="cyber-card achievement-card">
          <div class="panel-header">
            <span class="deco">🏆</span> MEDALS // 成就
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

      <div class="content-area-right dock-station">
        <div class="dock-top-bar">
          <div class="caution-stripes"></div>
          <span class="dock-id">/// MODULE_BAY_01 ///</span>
          <div class="indicator-leds">
            <span class="led red"></span>
            <span class="led green"></span>
          </div>
        </div>

        <div class="dock-inner-slot">
          <RightConfigSection :form="rightForm" @update:form="updateRightForm" />
          
          <div class="dock-overlay-grid"></div>
        </div>

        <div class="dock-latch left"></div>
        <div class="dock-latch right"></div>

        <div class="dock-bottom-vent">
          <div class="vent-lines"></div>
          <span class="temp-readout">TEMP: 34°C // STABLE</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router' 
import ProfileMain from '@/UserComponent/Profile/ProfileMainLeft/ProfileMainLeft.vue';
import IdArchiveCard from '@/UserComponent/Profile/IdArchiveCard.vue';
import { useAuthStore } from '@/utils/auth' 
import { storeToRefs } from 'pinia'
import RightConfigSection from '@/UserComponent/Profile/ProfileMainRight/RightConfigSection.vue';

// --- 原有逻辑完全保留 ---
const authStore = useAuthStore()
const { userID } = storeToRefs(authStore)
const router = useRouter() 
const isFollowing = ref(false)
const showIdArchive = ref(false)

const GoToUserSettings = () => { router.push('/profile/Usersettings') }
const goToSettings = () => GoToUserSettings()

const user = reactive({
  username: 'USER_114514',
  uid: '89757-X',
  nickname: 'No Name', 
  role: '视觉前端 // VISUAL_ENG',
  level: 42,
  avatar: authStore.user?.logo || 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
  bio: '原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼。',
  tags: ['界面设计', 'Vue开发', '三维艺术'],
  age: 24, gender: 'M/F', location: 'Guangzhou, CN', creationAge: '4年',
  email: 'fengfengzi@cyber.com', qq: '1145141919',
  externalLinks: [
    { name: 'GitHub', url: 'https://github.com' },
    { name: 'Bilibili', url: 'https://bilibili.com' },
    { name: 'Dribbble', url: 'https://dribbble.com' }
  ],
  stats: { likes: 12450, views: 89000, works: 142, followers: 3500 }
})

const rightForm = reactive({
  works: [null, null, null, null],
  articles: [null, null, null, null],
  achievements: [null, null, null, null, null, null, null, null],
  inventory: [null, null, null, null, null, null, null, null]
})

const updateRightForm = (newForm) => { Object.assign(rightForm, newForm) }

const achievements = ref([
  { id: 1, name: '早期开拓者', desc: '2023年前注册', icon: '⚡', unlocked: true },
  { id: 2, name: '高产机器', desc: '发布 > 100 作品', icon: '📦', unlocked: true },
  { id: 3, name: '万人瞩目', desc: '粉丝 > 10k', icon: '👑', unlocked: false },
])

const formatNumber = (num) => num > 1000 ? (num / 1000).toFixed(1) + 'k' : num
const goBack = () => {
  if (window.history.length > 1) window.history.back()
  else alert('[System]: Redirecting to Root...')
}
const toggleFollow = () => isFollowing.value = !isFollowing.value
const handleMessage = () => alert(`[System]: Encrypted channel to ${user.username} opened.`)
const toggleIdArchive = () => showIdArchive.value = !showIdArchive.value
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Caveat:wght@700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Rajdhani:wght@500;700&display=swap');

* { box-sizing: border-box; }

.user-profile-terminal {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --sans: 'Noto Sans SC', sans-serif;
  --cy-dark: #080808;
  --cy-trim: #333;
  --cy-accent: #ffae00; /* 工业黄 */

  width: 100%; height: 100%;
  overflow: hidden; 
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex; flex-direction: column;
  position: relative;
}

/* 全局 CRT 纹理 */
.crt-lines {
  position: fixed; inset: 0; pointer-events: none; z-index: 9999;
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.05) 50%);
  background-size: 100% 4px;
  height: 100%;
}

/* Header - 替换后的 header-left 样式 + 原有 header 基础样式保留 */
.terminal-header {
  height: 64px; /* 适配新 header-left 高度 */
  background: #080808;
  color: var(--white);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 24px; /* 适配新布局内边距 */
  border-bottom: 2px solid var(--red); /* 强化边框 */
  flex-shrink: 0;
  z-index: 100;
  box-shadow: 0 5px 20px rgba(0,0,0,0.5); /* 新增阴影增强层级 */
}

/* 替换后的 header-left 样式 */
.header-left { 
  display: flex; 
  align-items: center; 
  gap: 20px; 
}
.brand-block { 
  display: flex; 
  align-items: center; 
  gap: 12px; 
}
.logo-box {
  width: 40px; height: 40px; 
  background: var(--red); color: #000;
  font-weight: 800; font-size: 1.5rem; 
  display: flex; align-items: center; justify-content: center;
  clip-path: polygon(0 0, 100% 0, 100% 70%, 70% 100%, 0 100%); /* 切角样式 */
}
.brand-info { 
  display: flex; 
  flex-direction: column; 
  gap: 2px; 
}
.brand-text { 
  font-family: 'Rajdhani', sans-serif; 
  font-weight: 700; 
  font-size: 1.1rem; 
  letter-spacing: 1px; 
  color: #fff; 
}
.load-bar { 
  height: 2px; width: 100%; 
  background: #333; 
  position: relative; 
  overflow: hidden; 
}
.load-bar::after { 
  content: ''; 
  position: absolute; 
  left: 0; top: 0; 
  height: 100%; width: 30%; 
  background: var(--red); 
  animation: loading 2s infinite linear; 
}

/* 路径面包屑样式优化 */
.path-bread { 
  font-size: 0.75rem; 
  color: #555; 
  display: flex; gap: 8px; 
  margin-left: 20px; 
  text-transform: uppercase; 
}
.path-bread .current { 
  color: var(--red); 
  text-shadow: 0 0 5px var(--red);
  animation: blink 1s infinite; /* 新增闪烁动效 */
}
.path-bread .sep { color: #555; }

/* 原有 header-right 样式保留（微调适配新 header 高度） */
.header-right { 
  display: flex; 
  align-items: center; 
  gap: 20px; 
}
.status-indicator { 
  font-size: 0.7rem; 
  color: #00ff00; 
  display: flex; align-items: center; gap: 6px; 
}
.dot { 
  width: 6px; height: 6px; 
  background: #00ff00; 
  border-radius: 50%; 
  box-shadow: 0 0 5px #00ff00; 
  animation: blink 1s infinite; /* 统一闪烁动效 */
}

.sys-btn {
  background: transparent; 
  border: 1px solid #666; 
  color: #ccc;
  padding: 6px 16px; /* 微调内边距适配新 header 高度 */
  font-family: var(--mono); 
  cursor: pointer; 
  transition: 0.2s;
  clip-path: polygon(10px 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%, 0 10px);
}
.sys-btn:hover { 
  background: var(--red); 
  color: white; 
  border-color: var(--red); 
}

/* Main Layout */
.main-layout {
  flex: 1; display: flex; overflow: hidden; 
  padding: 20px; padding-top:0%; gap:  20px;
  background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px);
  background-size: 40px 40px;
  height: 100%;
}

/* Sidebar */
.sidebar-left {
  width: 320px; display: flex; flex-direction: column; gap: 20px;
  overflow-y: auto; padding-right: 5px; flex-shrink: 0;
}

/* ID Card Styles */
.id-flip-wrapper { perspective: 1200px; width: 100%; position: relative; z-index: 10; }
.id-flipper { width: 100%; position: relative; transform-style: preserve-3d; transition: transform 0.8s cubic-bezier(0.68, -0.55, 0.265, 1.55); } 
.id-flip-wrapper.is-flipped .id-flipper { transform: rotateY(180deg); }
.id-face { backface-visibility: hidden; width: 100%; }
.id-front { position: relative; z-index: 2; }
.id-back { position: absolute; top: 0; left: 0; height: 100%; transform: rotateY(180deg); z-index: 1; display: flex; }

.cyber-card {
  background: var(--white); border: 2px solid var(--black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.15); padding: 0; position: relative;
  transition: transform 0.2s;
}
.id-card-content { padding: 25px; text-align: center; margin-top: 5%;}
.card-deco-top { height: 10px; background: repeating-linear-gradient(45deg, var(--black), var(--black) 5px, transparent 5px, transparent 10px); position: absolute; top:0; left:0; width:100%; opacity: 0.2; }

/* Avatar */
.avatar-frame { 
  width: 120px; height: 120px; margin: 0 auto 20px; position: relative; 
  border: 2px solid var(--black); padding: 4px; 
}
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(20%); }
.scan-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(to bottom, transparent 40%, rgba(217,35,35,0.2) 50%, transparent 60%);
  background-size: 100% 200%;
  animation: radar-scan 3s infinite linear; pointer-events: none;
}
@keyframes radar-scan { 0% { background-position: 0% 0%; } 100% { background-position: 0% 200%; } }
.level-badge { 
  position: absolute; bottom: -10px; left: 50%; transform: translateX(-50%); 
  background: var(--red); color: white; padding: 2px 8px; font-size: 0.9rem; 
  font-weight: bold; border: 2px solid var(--black); font-family: var(--heading);
  box-shadow: 2px 2px 0 var(--black);
}

.user-name { font-family: var(--heading); letter-spacing: 1px; font-size: 2rem; margin: 10px 0 5px; }
.user-role { color: var(--red); font-family: var(--mono); font-size: 0.8rem; font-weight: bold; margin-bottom: 15px; }
.bio-text { font-size: 0.85rem; color: #555; margin-bottom: 20px; line-height: 1.6; border-top: 1px dashed #ccc; border-bottom: 1px dashed #ccc; padding: 10px 0; font-family: var(--sans); text-align: left;}
.meta-tags { display: flex; justify-content: center; gap: 5px; flex-wrap: wrap; margin-bottom: 20px; }
.tag { background: #eee; font-size: 0.7rem; padding: 2px 8px; border: 1px solid #ccc; font-family: var(--sans); }

.action-row { display: flex; gap: 10px; }
.action-btn { 
  flex: 1; border: none; background: transparent; padding: 10px; 
  font-size: 0.9rem; font-weight: bold; cursor: pointer; transition: 0.2s; 
  position: relative; border: 2px solid var(--black);
}
.action-btn.primary { background: var(--black); color: var(--white); }
.action-btn:hover { transform: translate(-2px, -2px); box-shadow: 4px 4px 0 var(--red); }
.action-btn.active-state { background: var(--white); color: var(--red); border-color: var(--red); }

/* Stats & Achievements */
.panel-header { background: var(--black); color: var(--white); padding: 8px 12px; font-weight: bold; font-family: var(--mono); font-size: 0.85rem; }
.stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1px; background: var(--black); border: 2px solid var(--black); margin: 15px; }
.stat-item { background: var(--white); padding: 10px; text-align: center; }
.stat-item .label { display: block; font-size: 0.65rem; color: #888; font-family: var(--mono); }
.stat-item .val { font-family: var(--heading); font-size: 1.4rem; }

.ach-item { display: flex; align-items: center; padding: 12px; border-bottom: 1px dashed #ccc; gap: 10px; }
.ach-item.locked { opacity: 0.5; filter: grayscale(1); }
.ach-name { font-weight: bold; font-size: 0.9rem; }
.ach-desc { font-size: 0.7rem; color: #888; }

/* Settings Button */
@keyframes settings-spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(180deg); } }
.settings-trigger-btn {
  position: absolute; top: 10px; left: 10px; width: 32px; height: 32px;
  background: var(--black); color: var(--white); border: none; cursor: pointer;
  z-index: 5; display: flex; align-items: center; justify-content: center; transition: 0.3s;
}
.settings-trigger-btn:hover { background: var(--red); }
.settings-trigger-btn:hover .icon { animation: settings-spin 2s infinite linear; }
.flip-trigger-btn {
  position: absolute; top: 10px; right: 10px; width: 32px; height: 32px;
  background: var(--black); color: var(--white); border: none; cursor: pointer;
  z-index: 5; display: flex; align-items: center; justify-content: center; transition: 0.2s;
}
.flip-trigger-btn:hover { background: var(--red); transform: rotate(180deg); }

/* Left Content Area */
.content-area-left {
  display: flex; flex-direction: column;
  background: var(--white);
  border: 0.1px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: hidden;
  height: 97%; margin-top: 1%; width: 40%;
}

/* --- Right Dock Station (新设计的右侧容器) --- */
.dock-station {
  display: flex; flex-direction: column;
  /* 沥青黑背景，模拟重金属 */
  background: #151515;
  border: 4px solid #2a2a2a;
  box-shadow: inset 0 0 50px rgba(0,0,0,0.8), 15px 15px 0 rgba(0,0,0,0.2);
  overflow: hidden;
  height: 97%; margin-top: 1%; width: 40%;
  position: relative;
  /* 顶部左上角和右下角的切角效果 */
  clip-path: polygon(
    20px 0, 100% 0, 
    100% calc(100% - 20px), calc(100% - 20px) 100%, 
    0 100%, 0 20px
  );
}

/* 扩展坞顶部条 */
.dock-top-bar {
  height: 40px; background: #080808;
  border-bottom: 2px solid #333;
  display: flex; align-items: center; justify-content: space-between;
  padding: 0 15px; box-shadow: 0 5px 10px rgba(0,0,0,0.5);
  flex-shrink: 0;
}
.caution-stripes {
  width: 60px; height: 10px;
  background: repeating-linear-gradient(
    45deg,
    var(--cy-accent),
    var(--cy-accent) 5px,
    #000 5px,
    #000 10px
  );
  opacity: 0.8;
}
.dock-id {
  color: #666; font-family: 'Rajdhani', sans-serif; font-weight: bold; letter-spacing: 2px;
  font-size: 0.9rem;
}
.indicator-leds { display: flex; gap: 5px; }
.led { width: 8px; height: 4px; background: #333; }
.led.red { background: #500; animation: blink 3s infinite; }
.led.green { background: #0f0; box-shadow: 0 0 5px #0f0; }

/* 内部嵌入槽 */
.dock-inner-slot {
  flex: 1; position: relative;
  margin: 10px;
  background: #000; /* 内槽全黑 */
  border: 2px solid #333; /* 内边框 */
  box-shadow: inset 0 0 20px #000;
  overflow: hidden;
  display: flex;
}
.dock-overlay-grid {
  position: absolute; inset: 0; pointer-events: none; z-index: 50;
  background-image: linear-gradient(transparent 95%, rgba(0, 255, 255, 0.02) 50%);
  background-size: 100% 10px;
}

/* 机械卡扣装饰 */
.dock-latch {
  position: absolute; top: 50%; width: 8px; height: 60px;
  background: #444; border: 1px solid #000;
  transform: translateY(-50%); z-index: 20;
  box-shadow: 2px 2px 5px rgba(0,0,0,0.5);
}
.dock-latch::after {
  content: ''; position: absolute; top: 10px; left: 1px; width: 4px; height: 40px;
  background: repeating-linear-gradient(to bottom, #222, #222 2px, #555 2px, #555 4px);
}
.dock-latch.left { left: 0; border-radius: 0 4px 4px 0; }
.dock-latch.right { right: 0; border-radius: 4px 0 0 4px; }

/* 底部通风口 */
.dock-bottom-vent {
  height: 30px; background: #1a1a1a;
  border-top: 1px solid #333;
  display: flex; align-items: center; justify-content: space-between;
  padding: 0 20px;
  font-family: var(--mono); font-size: 0.6rem; color: #555;
}
.vent-lines {
  width: 100px; height: 6px;
  background: repeating-linear-gradient(90deg, #333, #333 2px, transparent 2px, transparent 4px);
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #f1f1f1; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }

/* 新增动效（适配新 header-left） */
@keyframes loading { 0% { left: -30%; } 100% { left: 100%; } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
</style>