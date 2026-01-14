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
                <span class="row-label">SETTINGS</span>
                <span class="row-icon">-></span>
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

      <main class="content-area">
        <nav class="tab-controller">
          <button 
            v-for="tab in tabs" 
            :key="tab.id"
            class="tab-btn"
            :class="{ active: currentTab === tab.id }"
            @click="switchTab(tab.id)"
          >
            <span class="tab-idx">0{{ tab.index }}</span>
            <span class="tab-label">{{ tab.label }}</span>
            <span class="tab-sub">{{ tab.sub }}</span>
          </button>
          <div class="tab-filler"></div>
        </nav>

        <div class="viewport custom-scroll" ref="viewportRef">
          
          <Transition name="fade-slide" mode="out-in">
            <div v-if="currentTab === 'works'" class="view-section view-works" key="works">
              <div class="section-toolbar">
                <span class="label">筛选类型:</span>
                <div class="filter-chips">
                  <span 
                    v-for="filter in workFilters" 
                    :key="filter.key"
                    class="chip" 
                    :class="{ active: activeWorkFilter === filter.key }"
                    @click="activeWorkFilter = filter.key"
                  >
                    {{ filter.label }}
                  </span>
                </div>
              </div>
              <div class="works-grid">
                <div 
                  v-for="work in filteredWorks" 
                  :key="work.id" 
                  class="work-item"
                  @click="goToWorkDetail(work.id)"
                >
                  <div class="img-wrapper">
                    <img :src="work.cover" loading="lazy" />
                    <div class="overlay">
                      <span>查看详情 ></span>
                    </div>
                  </div>
                  <div class="work-meta">
                    <div class="w-title">{{ work.title }}</div>
                    <div class="w-stats">
                      <span>♥ {{ work.likes }}</span>
                      <span style="font-size: 0.6rem; border:1px solid #ccc; padding:0 2px;">{{ work.category === 'ui' ? 'UI' : 'ART' }}</span>
                    </div>
                  </div>
                </div>
                <div v-if="filteredWorks.length === 0" class="empty-state">
                  [NULL] 该分类下暂无数据...
                </div>
              </div>
            </div>

            <div v-else-if="currentTab === 'blogs'" class="view-section view-blogs" key="blogs">
              <div class="blog-timeline">
                <div v-for="blog in blogs" :key="blog.id" class="blog-entry">
                  <div class="date-col">
                    <span class="day">{{ blog.day }}</span>
                    <span class="month">{{ blog.month }}</span>
                  </div>
                  <div class="content-col">
                    <div class="blog-card" @click="goToBlogDetail(blog.id)">
                      <h3 class="b-title">{{ blog.title }}</h3>
                      <p class="b-excerpt">{{ blog.excerpt }}</p>
                      <div class="b-footer">
                        <div class="tags">
                          <span v-for="t in blog.tags" :key="t">#{{ t }}</span>
                        </div>
                        <button class="read-btn" @click.stop="goToBlogDetail(blog.id)">阅读档案 >></button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="currentTab === 'activity'" class="view-section view-activity" key="activity">
              <div class="terminal-log">
                <div class="log-header">
                  <span>时间戳 (TIME)</span>
                  <span>操作 (ACT)</span>
                  <span>详情 (DETAIL)</span>
                </div>
                <div class="log-body">
                  <div v-for="(log, idx) in activities" :key="idx" class="log-row">
                    <span class="col-time">{{ log.time }}</span>
                    <span class="col-action" :class="log.type">[{{ log.action }}]</span>
                    <span class="col-detail">{{ log.detail }}</span>
                  </div>
                  <div class="log-cursor">_</div>
                </div>
              </div>
            </div>

            <div v-else-if="currentTab === 'guestbook'" class="view-section view-guestbook" key="guestbook">
              
              <div class="guestbook-input-area">
                <div class="gb-input-header">
                  <span class="blink">PULSE_INPUT //</span> 请输入加密留言
                </div>
                <div class="gb-form">
                  <textarea 
                    v-model="newGuestMsg" 
                    placeholder="输入内容..." 
                    rows="3"
                    class="gb-textarea"
                  ></textarea>
                  <div class="gb-actions">
                    <span class="deco-code">CODE: 0x88</span>
                    <button class="sign-btn" @click="submitMessage">
                      [ 签署并归档 / SIGN ]
                    </button>
                  </div>
                </div>
              </div>

              <div class="guestbook-list">
                <div class="list-label">历史归档 // ARCHIVE_HISTORY ({{ guestMessages.length }})</div>
                
                <div v-for="msg in guestMessages" :key="msg.id" class="archive-note">
                  <div class="pin"></div>
                  <div class="note-header">
                    <span class="note-id">NO.{{ msg.id.toString().padStart(4, '0') }}</span>
                    <span class="note-date">{{ msg.date }}</span>
                  </div>
                  <div class="note-body">{{ msg.content }}</div>
                  <div class="note-footer">
                    <div class="signature-block">
                      <span class="sig-label">Signed by:</span>
                      <span class="sig-name">{{ msg.user }}</span>
                    </div>
                    <div class="stamp-seal" :class="msg.stampType">{{ msg.stampText }}</div>
                  </div>
                </div>
                <div class="end-of-file">--- END OF FILE ---</div>
              </div>

            </div>

          </Transition>

        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
// 导入 UserSettings 组件（根据你的组件实际路径调整）
import UserSettings from '@/UserComponent/UserSettings.vue' 

// --- Data & State ---
const currentTab = ref('works')
const viewportRef = ref(null)
const isFollowing = ref(false)
const activeWorkFilter = ref('all')
const goToSettings = () => { showUserMenu.value = false; router.push('/UserComponent/UserSettings') }

// ID Card Flip State
const showIdArchive = ref(false)

const tabs = [
  { id: 'works', index: 1, label: '视觉作品', sub: 'ARTWORKS_DB' },
  { id: 'blogs', index: 2, label: '思维日志', sub: 'THOUGHT_LOGS' },
  { id: 'activity', index: 3, label: '活动轨迹', sub: 'SYSTEM_ACT' },
  { id: 'guestbook', index: 4, label: '访客留言', sub: 'SIGNATURE_ARCHIVE' } 
]

const workFilters = [
  { key: 'all', label: '全部 ALL' },
  { key: 'ui', label: '作品' },
  { key: 'art', label: '帖子' }
]

const user = reactive({
  username: 'USER_114514',
  uid: '89757-X',
  nickname: '峰峰子', 
  role: '视觉前端 // VISUAL_ENG',
  level: 42,
  avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
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

const works = ref(Array.from({ length: 8 }).map((_, i) => ({
  id: i,
  title: `霓虹计划_第${i+1}期`,
  likes: 100 + i * 15,
  date: '2023.10.12',
  cover: `https://picsum.photos/400/300?random=${i}`,
  category: i % 2 === 0 ? 'ui' : 'art'
})))

const blogs = ref([
  { id: 1, day: '15', month: '10月', title: '低保真界面的美学复兴', excerpt: '为什么在人工智能生成平滑图像的时代，我们开始回归粗野主义设计风格...', tags: ['设计杂谈', '思考'] },
  { id: 2, day: '02', month: '09月', title: 'Vue 3 渲染性能深度优化', excerpt: '关于如何减少大型工业级仪表盘页面渲染开销的技术报告...', tags: ['代码', '开发'] },
  { id: 3, day: '28', month: '08月', title: '赛博朋克色彩理论研究', excerpt: '高对比度色彩方案在黑暗模式下的视觉引导作用。', tags: ['艺术', '色彩'] }
])

const activities = ref([
  { time: '2023-10-15 14:30', action: '上传', type: 'info', detail: '发布了新作品 "霓虹创世纪"。' },
  { time: '2023-10-14 09:15', action: '评论', type: 'success', detail: '在用户 @Ghost 的帖子下留言。' },
  { time: '2023-10-12 18:45', action: '登录', type: 'warn', detail: '检测到来自 HK_02 节点的系统访问。' },
  { time: '2023-10-10 11:20', action: '点赞', type: 'success', detail: '点赞了作品 "机械设计_v2"。' },
  { time: '2023-10-05 22:00', action: '系统', type: 'error', detail: '连接超时，已尝试自动重连。' }
])

const guestMessages = ref([
  { id: 1024, user: 'NetRunner_01', date: '2023-10-14 22:00', content: '非常喜欢你的配色方案，特别是红色警戒色的运用。期待更多作品！', stampType: 'approved', stampText: 'APPROVED' },
  { id: 1023, user: 'Unknown_V', date: '2023-10-12 10:15', content: 'Ping... 这里的交互逻辑很流畅。使用的是哪个版本的框架？', stampType: 'reviewed', stampText: 'REVIEWED' },
  { id: 1022, user: 'Design_Bot', date: '2023-10-10 09:30', content: '[自动留言] 系统检测到高能级设计作品。', stampType: 'system', stampText: 'LOGGED' },
])
const newGuestMsg = ref('')

const filteredWorks = computed(() => {
  if (activeWorkFilter.value === 'all') return works.value
  return works.value.filter(w => w.category === activeWorkFilter.value)
})

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

const switchTab = (tabId) => {
  currentTab.value = tabId
  if (viewportRef.value) viewportRef.value.scrollTop = 0
}

const toggleFollow = () => {
  isFollowing.value = !isFollowing.value
}

const handleMessage = () => {
  alert(`正在建立与 ${user.username} 的加密通道...\n[System]: Encryption handshake initiated.`)
}

const goToWorkDetail = (id) => {
  alert(`打开作品详情档案 [ID:${id}] ...`)
}

const goToBlogDetail = (id) => {
  alert(`加载思维日志 [ID:${id}] 内容...`)
}

const submitMessage = () => {
  if (!newGuestMsg.value.trim()) return
  const now = new Date()
  const timeStr = `${now.getFullYear()}-${(now.getMonth()+1).toString().padStart(2,'0')}-${now.getDate().toString().padStart(2,'0')} ${now.getHours()}:${now.getMinutes()}`
  guestMessages.value.unshift({
    id: Math.floor(Math.random() * 10000),
    user: 'Visitor_Guest',
    date: timeStr,
    content: newGuestMsg.value,
    stampType: 'pending', 
    stampText: 'RECEIVED' 
  })
  newGuestMsg.value = ''
  setTimeout(() => {
    if (viewportRef.value) viewportRef.value.scrollTop = 150 
  }, 100)
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
  height: 100vh;
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

/* 4. Content Area */
.content-area {
  flex: 1;
  display: flex; flex-direction: column;
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: hidden;
}

/* Tab Controller */
.tab-controller { display: flex; background: #eee; border-bottom: 2px solid var(--black); }
.tab-btn {
  flex: 1; max-width: 200px;
  background: #ddd; border: none; border-right: 1px solid var(--black);
  padding: 12px 15px; text-align: left; cursor: pointer; position: relative;
  transition: 0.2s; display: flex; flex-direction: column;
}
.tab-btn.active { background: var(--white); color: var(--black); padding-bottom: 14px; margin-bottom: -2px; z-index: 5; border-bottom: none; }
.tab-btn:hover:not(.active) { background: #ccc; }
.tab-btn::before { content:''; position: absolute; top:0; left:0; width: 100%; height: 4px; background: transparent; }
.tab-btn.active::before { background: var(--red); }
.tab-idx { font-size: 0.7rem; color: #888; font-weight: bold; font-family: var(--mono); }
.tab-label { font-family: var(--sans); font-weight: 800; font-size: 1.1rem; margin-top: 2px; }
.tab-sub { font-size: 0.6rem; color: #666; margin-top: 2px; opacity: 0.8; font-family: var(--mono); text-transform: uppercase; }
.tab-filler { flex: 1; background: #ddd; border-bottom: 2px solid var(--black); }

/* Viewport */
.viewport { flex: 1; overflow-y: auto; padding: 30px; position: relative; scroll-behavior: smooth; }
.section-toolbar { display: flex; align-items: center; gap: 15px; margin-bottom: 20px; font-size: 0.85rem; border-bottom: 2px solid #eee; padding-bottom: 10px; font-family: var(--sans); font-weight: bold; }
.filter-chips { display: flex; gap: 10px; }
.chip { border: 1px solid var(--black); padding: 4px 12px; cursor: pointer; transition: 0.2s; font-size: 0.8rem; user-select: none; }
.chip.active, .chip:hover { background: var(--black); color: var(--white); }
.chip:active { transform: scale(0.95); }

.works-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 20px; }
.work-item { border: 2px solid var(--black); transition: transform 0.2s; cursor: pointer; }
.work-item:hover { transform: translateY(-5px); box-shadow: 6px 6px 0 var(--red); }
.img-wrapper { height: 160px; overflow: hidden; position: relative; border-bottom: 2px solid var(--black); }
.img-wrapper img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(30%); transition: 0.3s; }
.work-item:hover img { filter: grayscale(0) contrast(1.1); transform: scale(1.05); }
.overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.7); display: flex; align-items: center; justify-content: center; color: var(--white); opacity: 0; transition: 0.2s; font-weight: bold; font-family: var(--sans); }
.work-item:hover .overlay { opacity: 1; }
.work-meta { padding: 10px; background: #fff; }
.w-title { font-weight: bold; font-size: 0.95rem; margin-bottom: 5px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; font-family: var(--sans); }
.w-stats { display: flex; justify-content: space-between; font-size: 0.75rem; color: #666; font-family: var(--mono); }
.empty-state { width: 100%; grid-column: 1 / -1; padding: 40px; text-align: center; color: #999; border: 2px dashed #ddd; font-family: var(--mono); }

/* Blogs */
.blog-timeline { display: flex; flex-direction: column; gap: 20px; max-width: 800px; }
.blog-entry { display: flex; gap: 20px; }
.date-col { width: 60px; text-align: center; border: 2px solid var(--black); height: fit-content; background: #fff; }
.date-col .day { display: block; font-size: 1.5rem; font-family: var(--heading); background: var(--black); color: var(--white); }
.date-col .month { display: block; font-size: 0.85rem; font-weight: bold; padding: 5px 0; font-family: var(--sans); }
.content-col { flex: 1; }
.blog-card { border: 2px solid var(--black); background: #fff; padding: 20px; position: relative; transition: 0.2s; cursor: pointer; }
.blog-card:hover { box-shadow: 6px 6px 0 var(--gray-dark); transform: translateX(5px); }
.b-title { margin: 0 0 10px 0; font-family: var(--sans); font-size: 1.2rem; font-weight: bold; }
.b-excerpt { color: #555; font-size: 0.9rem; margin-bottom: 15px; font-family: var(--sans); line-height: 1.6; }
.b-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px dashed #ccc; padding-top: 10px; }
.read-btn { border: none; background: transparent; color: var(--red); font-weight: bold; cursor: pointer; font-family: var(--sans); font-size: 0.85rem; transition: 0.2s; }
.read-btn:hover { text-decoration: underline; background: #eee; }

/* Activity */
.terminal-log { background: #000; color: #0f0; padding: 20px; font-family: 'Courier New', Courier, monospace; height: 100%; overflow-y: auto; border: 2px solid #333; }
.log-header { display: grid; grid-template-columns: 160px 100px 1fr; border-bottom: 1px dashed #0f0; padding-bottom: 10px; margin-bottom: 10px; opacity: 0.7; font-size: 0.8rem; }
.log-row { display: grid; grid-template-columns: 160px 100px 1fr; margin-bottom: 8px; font-size: 0.85rem; }
.col-time { opacity: 0.6; font-family: var(--mono); }
.col-action { font-weight: bold; }
.col-action.info { color: #00bfff; }
.col-action.warn { color: #ffff00; }
.col-action.error { color: #ff0000; }
.col-action.success { color: #00ff00; }
.log-cursor { animation: blink 1s infinite; font-weight: bold; margin-top: 10px; }

/* Guestbook */
.view-guestbook { display: flex; flex-direction: column; gap: 30px; max-width: 800px; }
.guestbook-input-area { border: 2px solid var(--black); background: #eee; padding: 15px; }
.gb-input-header { font-family: var(--mono); font-weight: bold; margin-bottom: 10px; font-size: 0.8rem; color: #555; }
.gb-form { display: flex; flex-direction: column; gap: 10px; }
.gb-textarea { width: 100%; border: 1px solid var(--black); padding: 10px; font-family: var(--sans); background: var(--white); resize: vertical; min-height: 80px; }
.gb-textarea:focus { outline: 2px solid var(--red); }
.gb-actions { display: flex; justify-content: space-between; align-items: center; }
.deco-code { font-family: var(--mono); font-size: 0.7rem; color: #888; }
.sign-btn { background: var(--black); color: var(--white); border: none; padding: 8px 20px; font-family: var(--mono); font-weight: bold; cursor: pointer; transition: 0.2s; }
.sign-btn:hover { background: var(--red); }
.guestbook-list { display: flex; flex-direction: column; gap: 20px; margin-top: 30px; }
.list-label { font-family: var(--heading); font-size: 1.2rem; border-bottom: 2px solid var(--black); padding-bottom: 5px; margin-bottom: 10px; }
.end-of-file { text-align: center; font-family: var(--mono); color: #ccc; margin-top: 20px; font-size: 0.8rem; }
.archive-note { background: var(--white); border: 1px solid #999; padding: 20px; position: relative; box-shadow: 3px 3px 0 rgba(0,0,0,0.1); }
.pin { width: 12px; height: 12px; background: #aaa; border-radius: 50%; position: absolute; top: 10px; left: 50%; transform: translateX(-50%); box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3); }
.note-header { display: flex; justify-content: space-between; font-family: var(--mono); font-size: 0.75rem; color: #666; border-bottom: 1px solid #ddd; padding-bottom: 8px; margin-bottom: 10px; }
.note-body { font-family: var(--sans); line-height: 1.6; font-size: 0.95rem; margin-bottom: 20px; min-height: 40px; }
.note-footer { display: flex; justify-content: space-between; align-items: flex-end; position: relative; }
.signature-block { display: flex; flex-direction: column; }
.sig-label { font-size: 0.6rem; color: #888; font-family: var(--sans); }
.sig-name { font-family: var(--hand), cursive; font-size: 1.5rem; color: var(--black); transform: rotate(-2deg); margin-top: -5px; }
.stamp-seal { position: absolute; right: 0; bottom: -5px; border: 3px double; padding: 5px 10px; font-family: var(--heading); font-size: 1rem; letter-spacing: 2px; transform: rotate(-15deg); opacity: 0.7; mix-blend-mode: multiply; text-transform: uppercase; }
.stamp-seal.approved { color: var(--red); border-color: var(--red); }
.stamp-seal.reviewed { color: #0099ff; border-color: #0099ff; }
.stamp-seal.system { color: #333; border-color: #333; transform: rotate(0deg); border-style: solid; background: #eee; }
.stamp-seal.pending { color: #ffaa00; border-color: #ffaa00; border-style: dashed; }

/* Utilities */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.blink { animation: blink 1s infinite; }
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.fade-slide-enter-active, .fade-slide-leave-active { transition: opacity 0.3s, transform 0.3s; }
.fade-slide-enter-from { opacity: 0; transform: translateY(10px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }
@media (max-width: 1000px) {
  .main-layout { flex-direction: column; overflow-y: auto; }
  .sidebar-left { width: 100%; flex: none; overflow: visible; }
  .content-area { height: 600px; flex: none; }
}
</style>