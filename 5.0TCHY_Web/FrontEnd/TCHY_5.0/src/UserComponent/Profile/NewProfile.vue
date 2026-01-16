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
            
            <div class="id-face id-front">
              <div class="cyber-card id-card-content">
                
                
                 <button class="settings-trigger-btn" title="用户资料设置" @click="GoToUserSettings">
                  <span class="icon">⚙</span>
                 </button>
                              <div class="menu-row" @click="goToSettings">
                <span class="row-icon"></span>
                <span class="row-label">User Avatar</span>
                
              </div>

                <button class="flip-trigger-btn" @click="toggleIdArchive" title="查看详细资料">
                  <span class="icon">➜</span>
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

            <div class="id-face id-back">
              <div class="cyber-card archive-card-content">
                <div class="archive-header">
                  <span class="blink">> PERSONAL_DATA</span>
                  <button class="close-btn" @click="toggleIdArchive">×</button>
                </div>
                
                <div class="archive-body custom-scroll">
                  <div class="data-group">
                    <div class="group-title">01. 基础识别 // BASIC_ID</div>
                    <div class="info-grid">
                      <div class="info-cell">
                        <span class="label">AGE</span>
                        <span class="value">{{ user.age }}</span>
                      </div>
                      <div class="info-cell">
                        <span class="label">SEX</span>
                        <span class="value">{{ user.gender }}</span>
                      </div>
                      <div class="info-cell full-width">
                        <span class="label">LOC</span>
                        <span class="value">{{ user.location }}</span>
                      </div>
                      <div class="info-cell full-width">
                        <span class="label">EXP</span>
                        <span class="value">{{ user.creationAge }} (since 2019)</span>
                      </div>
                    </div>
                  </div>

                  <div class="data-group">
                    <div class="group-title">02. 通讯终端 // CONNECT</div>
                    <div class="contact-list">
                      <a :href="`mailto:${user.email}`" class="contact-item">
                        <span class="c-icon">✉</span>
                        <div class="c-detail">
                          <span class="c-label">EMAIL_LINK</span>
                          <span class="c-val">{{ user.email }}</span>
                        </div>
                        <span class="c-arrow">>></span>
                      </a>
                      <a :href="`tencent://message/?uin=${user.qq}&Site=Sambow&Menu=yes`" class="contact-item">
                        <span class="c-icon">🐧</span>
                        <div class="c-detail">
                          <span class="c-label">TENCENT_QQ</span>
                          <span class="c-val">{{ user.qq }}</span>
                        </div>
                        <span class="c-arrow">>></span>
                      </a>
                    </div>
                  </div>

                  <div class="data-group">
                    <div class="group-title">03. 外部链路 // LINKS</div>
                    <div class="link-buttons">
                      <a v-for="link in user.externalLinks" :key="link.name" :href="link.url" target="_blank" class="cyber-link-btn">
                        [{{ link.name }}]
                      </a>
                    </div>
                  </div>

                  <div class="barcode-area">
                    <div class="barcode"></div>
                    <span class="code-num">UID: {{ user.uid }}</span>
                  </div>

                </div>
              </div>
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

      <div class="content-area">
        <ProfileMain />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router' 
import ProfileMain from '@/UserComponent/Profile/ProfileMain.vue';

// --- Data & State ---
const router = useRouter() 
const isFollowing = ref(false)

// 跳转用户设置
const GoToUserSettings = () => {
  router.push('/profile/Usersettings')
}

// 兼容模板中的 goToSettings 调用
const goToSettings = () => {
  GoToUserSettings()
}

// ID Card Flip State
const showIdArchive = ref(false)

const user = reactive({
  username: 'USER_114514',
  uid: '89757-X',
  nickname: '峰峰子', 
  role: '视觉前端 // VISUAL_ENG',
  level: 42,
  avatar: 'https://image2url.com/r2/default/images/1768440579620-518d987a-37b7-4f81-8406-5fa77e6d79c1.jpg',
  bio: '原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼。',
  tags: ['界面设计', 'Vue开发', '三维艺术'],
  // 实用新增字段
  age: 24,
  gender: 'Male / M',
  location: 'Guangzhou, CN',
  creationAge: '4年', // 创作年龄
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

const toggleFollow = () => {
  isFollowing.value = !isFollowing.value
}

const handleMessage = () => {
  alert(`正在建立与 ${user.username} 的加密通道...\n[System]: Encryption handshake initiated.`)
}

// --- ID Flip Methods ---
const toggleIdArchive = () => {
  showIdArchive.value = !showIdArchive.value
}
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Caveat:wght@700&display=swap');

/* --- 核心修改：全局 Box Sizing 重置，防止 Padding 撑开宽度 --- */
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

  /* --- 核心修改：使用 100% 替代 100vw，并隐藏横向溢出 --- */
  width: 100%; 
  max-width: 100vw;
  height: 100%;
  overflow-x: hidden; /* 强制隐藏横向滚动条 */
  overflow-y: hidden; /* 纵向也不滚动，内容在内部滚动 */
  
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
}

/* 1. Header */
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

/* 2. Main Layout */
.main-layout {
  flex: 1;
  display: flex;
  overflow: hidden; 
  padding: 20px;
  gap: 20px;
  background-image: 
    linear-gradient(#ccc 1px, transparent 1px),
    linear-gradient(90deg, #ccc 1px, transparent 1px);
  background-size: 40px 40px;
  width: 100%; /* 确保布局不超出容器 */
  height: 100%;
}

/* 3. Sidebar */
.sidebar-left {
  width: 320px;
  display: flex; flex-direction: column; gap: 20px;
  overflow-y: auto;
  padding-right: 5px; 
  flex-shrink: 0;
}

/* --- ID Card Flip Logic --- */
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
  display: flex; /* 让内部卡片充满高度 */
}

/* ID Card Content (Front) */
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

/* Flip Trigger Button (Top Right) */
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
.flip-trigger-btn .icon { font-size: 1.2rem; line-height: 1; }
.corner-deco { position: absolute; bottom: -4px; left: -4px; width: 8px; height: 8px; border-bottom: 2px solid var(--black); border-left: 2px solid var(--black); }

/* Settings Trigger Button (Top Left) */
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
  transition: 0.2s;
}
.settings-trigger-btn:hover {
  background: var(--red);
  transform: rotate(90deg);
}
.settings-trigger-btn .icon {
  font-size: 1.2rem;
  line-height: 1;
}

.avatar-frame { width: 120px; height: 120px; margin: 0 auto 20px; position: relative; border: 2px solid var(--black); padding: 4px; }
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(20%); }
.level-badge { position: absolute; bottom: -10px; left: 50%; transform: translateX(-50%); background: var(--red); color: white; padding: 2px 8px; font-size: 0.9rem; font-weight: bold; border: 2px solid var(--black); font-family: var(--heading); letter-spacing: 1px; }

.user-name { font-family: var(--sans); font-weight: 800; font-size: 1.8rem; margin: 0; line-height: 1.2; color: var(--black); }
.user-role { color: var(--red); font-weight: bold; font-size: 0.85rem; margin-bottom: 15px; font-family: var(--sans); }
.bio-text { font-size: 0.85rem; color: #555; margin-bottom: 20px; line-height: 1.6; border-top: 1px dashed #ccc; border-bottom: 1px dashed #ccc; padding: 10px 0; font-family: var(--sans); text-align: left;}
.meta-tags { display: flex; justify-content: center; gap: 5px; flex-wrap: wrap; margin-bottom: 20px; }
.tag { background: #eee; font-size: 0.7rem; padding: 2px 8px; border: 1px solid #ccc; font-family: var(--sans); }

.action-row { display: flex; gap: 10px; }
.action-btn { flex: 1; border: 2px solid var(--black); background: transparent; padding: 8px; font-family: var(--sans); font-weight: bold; cursor: pointer; transition: 0.2s; font-size: 0.8rem; }
.action-btn.primary { background: var(--black); color: var(--white); }
.action-btn.active-state { background: var(--white); color: var(--red); border-color: var(--red); }
.action-btn:hover { background: var(--red); color: var(--white); border-color: var(--black); }
.action-btn:active{ transform: translateY(2px); }

/* --- ID Card Back (Archive - Practical Version) --- */
.archive-card-content {
  width: 100%; height: 100%;
  background: #222; color: #ddd;
  display: flex; flex-direction: column;
  text-align: left;
  border: 2px solid var(--black);
}
.archive-header {
  background: #000; padding: 10px 15px;
  display: flex; justify-content: space-between; align-items: center;
  border-bottom: 1px dashed #444; font-family: var(--mono); font-size: 0.8rem; font-weight: bold; color: #0f0;
}
.close-btn { background: none; border: none; color: #fff; font-size: 1.2rem; cursor: pointer; line-height: 1; }
.close-btn:hover { color: var(--red); }

.archive-body { padding: 15px; display: flex; flex-direction: column; gap: 20px; flex: 1; overflow-y: auto; }

/* Archive Group Styles */
.data-group { display: flex; flex-direction: column; gap: 8px; }
.group-title { font-size: 0.7rem; color: #666; font-weight: bold; border-left: 3px solid var(--red); padding-left: 8px; font-family: var(--mono); text-transform: uppercase; }

/* 1. Basic ID Grid */
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.info-cell { background: #111; border: 1px solid #333; padding: 8px; display: flex; flex-direction: column; }
.info-cell.full-width { grid-column: span 2; }
.info-cell .label { font-size: 0.6rem; color: #888; font-family: var(--mono); margin-bottom: 2px; }
.info-cell .value { font-size: 0.85rem; font-weight: bold; color: #fff; font-family: var(--sans); }

/* 2. Contact List */
.contact-list { display: flex; flex-direction: column; gap: 8px; }
.contact-item { 
  display: flex; align-items: center; gap: 10px; 
  background: #111; border: 1px solid #333; padding: 10px; 
  text-decoration: none; color: #ccc; transition: all 0.2s; 
}
.contact-item:hover { border-color: #0f0; background: #000; color: #fff; box-shadow: 0 0 8px rgba(0, 255, 0, 0.2); }
.c-icon { font-size: 1.2rem; width: 30px; text-align: center; }
.c-detail { flex: 1; display: flex; flex-direction: column; }
.c-label { font-size: 0.6rem; color: #666; font-family: var(--mono); }
.c-val { font-size: 0.8rem; font-weight: bold; }
.c-arrow { font-family: var(--mono); font-size: 0.8rem; color: #0f0; opacity: 0; transform: translateX(-5px); transition: 0.2s; }
.contact-item:hover .c-arrow { opacity: 1; transform: translateX(0); }

/* 3. Link Buttons */
.link-buttons { display: flex; flex-wrap: wrap; gap: 8px; }
.cyber-link-btn {
  background: transparent; border: 1px solid #555; color: #aaa;
  padding: 6px 12px; font-family: var(--mono); font-size: 0.75rem;
  text-decoration: none; transition: 0.2s;
}
.cyber-link-btn:hover { border-color: var(--red); color: var(--red); background: rgba(217, 35, 35, 0.1); }

/* Bottom Barcode */
.barcode-area { margin-top: auto; padding-top: 15px; text-align: center; opacity: 0.5; }
.barcode { height: 25px; background: repeating-linear-gradient(90deg, #fff, #fff 2px, transparent 2px, transparent 4px); margin-bottom: 5px; }
.code-num { font-size: 0.6rem; letter-spacing: 2px; font-family: var(--mono); }

/* Other Sidebar Cards */
.panel-header { background: var(--black); color: var(--white); padding: 8px 12px; font-weight: bold; font-size: 0.9rem; display: flex; align-items: center; gap: 8px; font-family: var(--sans); }
.stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1px; background: var(--black); border: 2px solid var(--black); margin: 15px; }
.stat-item { background: var(--white); padding: 15px 10px; display: flex; flex-direction: column; align-items: center; }
.stat-item .label { font-size: 0.75rem; color: #888; font-weight: bold; margin-bottom: 5px; font-family: var(--sans); }
.stat-item .val { font-family: var(--heading); font-size: 1.4rem; line-height: 1; color: var(--black); }

.achieve-list { display: flex; flex-direction: column; }
.ach-item { display: flex; align-items: center; padding: 12px; border-bottom: 1px dashed #ccc; gap: 10px; }
.ach-item:last-child { border-bottom: none; }
.ach-icon { font-size: 1.5rem; }
.ach-info { flex: 1; }
.ach-name { font-weight: bold; font-size: 0.9rem; font-family: var(--sans); }
.ach-desc { font-size: 0.75rem; color: #888; font-family: var(--sans); margin-top: 2px; }
.ach-item.locked { opacity: 0.5; filter: grayscale(1); }

/* 保留 content-area 基础样式（仅容器结构，无内容样式） */
.content-area {
  flex: 1;
  display: flex; flex-direction: column;
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: hidden;
  height: inherit;
}
</style>