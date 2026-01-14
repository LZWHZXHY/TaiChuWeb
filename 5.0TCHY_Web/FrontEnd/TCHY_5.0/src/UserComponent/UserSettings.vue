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
      
      <aside class="sidebar-nav">
        
        <div class="nav-section custom-scroll">
          <div class="nav-header">>> 基础索引 // BASICS</div>
          <ul class="nav-list">
            <li :class="{ active: activeSection === 'identity' }" @click="scrollTo('identity', 'left')">
              <span class="idx">01</span> 身份识别
            </li>
            <li :class="{ active: activeSection === 'basic' }" @click="scrollTo('basic', 'left')">
              <span class="idx">02</span> 基础档案
            </li>
            <li :class="{ active: activeSection === 'tags' }" @click="scrollTo('tags', 'left')">
              <span class="idx">03</span> 标签管理
            </li>
            <li :class="{ active: activeSection === 'contact' }" @click="scrollTo('contact', 'left')">
              <span class="idx">04</span> 通讯终端
            </li>
            <li :class="{ active: activeSection === 'links' }" @click="scrollTo('links', 'left')">
              <span class="idx">05</span> 外部链路
            </li>
          </ul>
        </div>

        <div class="nav-divider"></div>

        <div class="nav-section custom-scroll">
          <div class="nav-header">>> 个性化配置 // CUSTOM</div>
          <ul class="nav-list">
             <li :class="{ active: activeSection === 'theme' }" @click="scrollTo('theme', 'right')">
              <span class="idx">A1</span> 视觉主题
            </li>
            <li :class="{ active: activeSection === 'pinned' }" @click="scrollTo('pinned', 'right')">
              <span class="idx">A2</span> 置顶内容
            </li>
            <li :class="{ active: activeSection === 'achieve' }" @click="scrollTo('achieve', 'right')">
              <span class="idx">A3</span> 成就矩阵
            </li>
            <li :class="{ active: activeSection === 'widgets' }" @click="scrollTo('widgets', 'right')">
              <span class="idx">A4</span> 挂件参数
            </li>
          </ul>
        </div>

        <div class="sidebar-footer">
          <span>MEM: 42%</span>
          <span>NET: SECURE</span>
        </div>
      </aside>

      <main class="content-split">
        
        <div id="col-left" class="content-col left-col custom-scroll">
          
          <section id="identity" class="form-section">
            <div class="section-header">
              <span class="sec-num">01</span>
              <span class="sec-title">身份识别 // IDENTITY</span>
            </div>
            <div class="form-row avatar-row">
              <div class="avatar-preview">
                <img :src="form.avatar" alt="Avatar" />
                <div class="scan-line"></div>
              </div>
              <div class="avatar-controls">
                <input type="text" v-model="form.avatar" class="cyber-input" placeholder="IMG_SOURCE_URL..." />
                <button class="cyber-btn sm full-width" @click="randomAvatar">🎲 REROLL_HASH</button>
              </div>
            </div>
            <div class="form-group">
              <label>昵称 // UID</label>
              <input type="text" v-model="form.nickname" class="cyber-input bold" />
            </div>
          </section>

          <section id="basic" class="form-section">
            <div class="section-header">
              <span class="sec-num">02</span>
              <span class="sec-title">基础档案 // DATA</span>
            </div>
            <div class="form-group">
              <label>BIO // 简介</label>
              <textarea v-model="form.bio" class="cyber-textarea" rows="3"></textarea>
            </div>
            <div class="form-grid">
               <div class="form-group">
                <label>LOC // 坐标</label>
                <input type="text" v-model="form.location" class="cyber-input" />
              </div>
              <div class="form-group">
                <label>ROLE // 职业</label>
                <input type="text" v-model="form.role" class="cyber-input" />
              </div>
            </div>
          </section>

          <section id="tags" class="form-section">
            <div class="section-header">
              <span class="sec-num">03</span>
              <span class="sec-title">标签 // TAGS</span>
            </div>
            <div class="tag-manager">
              <div class="current-tags">
                <span v-for="(tag, index) in form.tags" :key="index" class="tag-chip">
                  {{ tag }} <span class="remove-x" @click="removeTag(index)">×</span>
                </span>
              </div>
              <input type="text" v-model="newTagInput" @keyup.enter="addTag" class="cyber-input" placeholder="Add Tag + Enter" />
            </div>
          </section>

          <section id="contact" class="form-section">
            <div class="section-header">
              <span class="sec-num">04</span>
              <span class="sec-title">通讯 // COMMS</span>
            </div>
            <div class="form-group">
              <label>EMAIL</label>
              <input type="email" v-model="form.email" class="cyber-input" />
            </div>
          </section>

           <section id="links" class="form-section">
            <div class="section-header">
              <span class="sec-num">05</span>
              <span class="sec-title">链路 // LINKS</span>
            </div>
            <div class="links-editor">
              <div v-for="(link, index) in form.externalLinks" :key="index" class="link-row">
                <input type="text" v-model="link.name" class="cyber-input sm" placeholder="Name" />
                <input type="text" v-model="link.url" class="cyber-input lg" placeholder="URL" />
                <button class="del-btn" @click="removeLink(index)">×</button>
              </div>
              <button class="add-link-btn" @click="addLink">+ ADD NODE</button>
            </div>
          </section>

          <div class="end-marker">/// END OF BASIC_CONFIG ///</div>
        </div>

        <div id="col-right" class="content-col right-col custom-scroll">
          
          <div id="theme" class="config-module">
            <div class="module-header">
              <span class="mod-id">MOD_A1</span>
              <span class="mod-name">VISUAL_THEME // 视觉主题</span>
              <div class="mod-decor"></div>
            </div>
            <div class="module-body">
              <div class="form-group">
                <label>ACCENT_COLOR // 强调色</label>
                <div class="color-picker-grid">
                  <div class="color-opt" style="background: #D92323" @click="setTheme('#D92323')"></div>
                  <div class="color-opt" style="background: #23D9D9" @click="setTheme('#23D9D9')"></div>
                  <div class="color-opt" style="background: #D9D923" @click="setTheme('#D9D923')"></div>
                  <div class="color-opt" style="background: #8823D9" @click="setTheme('#8823D9')"></div>
                  <input type="color" class="color-input" />
                </div>
              </div>
              <div class="form-group">
                <label>INTERFACE_MODE // 界面模式</label>
                <select class="cyber-select inverted">
                  <option>DARK_MATTER (Default)</option>
                  <option>LIGHT_ECHO</option>
                  <option>HIGH_CONTRAST</option>
                </select>
              </div>
            </div>
          </div>

          <div id="pinned" class="config-module">
            <div class="module-header">
              <span class="mod-id">MOD_A2</span>
              <span class="mod-name">PINNED_CONTENT // 置顶管理</span>
              <div class="mod-decor"></div>
            </div>
            <div class="module-body">
              <div class="sub-group">
                <label>FEATURED_WORK // 代表作</label>
                <div class="pinned-item empty">
                  <span>[ + ] Select Project to Pin</span>
                </div>
              </div>
               <div class="sub-group">
                <label>TOP_ARTICLE // 置顶文章</label>
                <div class="pinned-item">
                  <span class="item-title">Vue3 Reactivity Deep Dive</span>
                  <span class="item-status">ACTIVE</span>
                </div>
              </div>
            </div>
          </div>

          <div id="achieve" class="config-module">
             <div class="module-header">
              <span class="mod-id">MOD_A3</span>
              <span class="mod-name">ACHIEVEMENTS // 成就展示</span>
              <div class="mod-decor"></div>
            </div>
            <div class="module-body">
              <div class="achievement-grid">
                <div class="ach-badge active">100 Days</div>
                <div class="ach-badge active">Contributor</div>
                <div class="ach-badge">Bug Hunter</div>
                <div class="ach-badge">Sponsor</div>
              </div>
              <div class="switch-row">
                <label>Show Badges Publicly</label>
                <input type="checkbox" checked />
              </div>
            </div>
          </div>

          <div class="end-marker">/// END OF CUSTOM_CONFIG ///</div>
        </div>

      </main>
    </div>
  </div>
</template>

<script setup>
  import { onMounted, onUnmounted } from 'vue'

onMounted(() => {
  // 进入页面时，禁止全局滚动
  document.body.style.overflow = 'hidden'
})

onUnmounted(() => {
  // 离开页面时，恢复全局滚动
  document.body.style.overflow = ''
})
import { ref, reactive } from 'vue'

// --- State ---
const activeSection = ref('identity')
const newTagInput = ref('')

// --- Form Data ---
const form = reactive({
  avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
  nickname: '峰峰子',
  role: 'Visual Eng.',
  bio: 'System initialization complete. Waiting for input...',
  location: 'Guangzhou, CN',
  email: 'fengfengzi@cyber.com',
  tags: ['UI/UX', 'Vue3', 'Cyberpunk'],
  externalLinks: [
    { name: 'GitHub', url: 'https://github.com' },
    { name: 'Bilibili', url: 'https://bilibili.com' }
  ]
})

// --- Methods ---

const goBack = () => {
  if (confirm('ABORT CHANGES?')) window.history.back()
}

const saveAll = () => {
  alert('SYSTEM_MSG: Configuration packets transmitted.')
}

// Avatar
const randomAvatar = () => {
  const seed = Math.random().toString(36).substring(7)
  form.avatar = `https://api.dicebear.com/7.x/notionists/svg?seed=${seed}`
}

// Tags
const addTag = () => {
  const val = newTagInput.value.trim()
  if (val && !form.tags.includes(val)) {
    form.tags.push(val)
    newTagInput.value = ''
  }
}
const removeTag = (index) => form.tags.splice(index, 1)

// Links
const addLink = () => form.externalLinks.push({ name: '', url: '' })
const removeLink = (index) => form.externalLinks.splice(index, 1)

// Theme Mock
const setTheme = (color) => {
  console.log(`System Accent Color set to: ${color}`)
}

// Scroll Logic
// colType: 'left' | 'right'
const scrollTo = (id, colType) => {
  activeSection.value = id
  const containerId = colType === 'left' ? 'col-left' : 'col-right'
  const container = document.getElementById(containerId)
  const target = document.getElementById(id)
  
  if (container && target) {
    // 计算相对位置进行滚动
    const top = target.offsetTop - container.offsetTop
    container.scrollTo({ top, behavior: 'smooth' })
  }
}
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

/* --- Global Reset & Variables --- */
.settings-terminal {
  --red: #D92323;
  --black: #111111; /* 主要背景 */
  --dark-grey: #1a1a1a; /* 模块背景 */
  --light-grey: #f4f4f4; /* 表单背景 */
  --green: #00ff41; /* 终端绿 */
  --text-main: #111;
  --text-inv: #eee;
  
  --mono: 'JetBrains Mono', monospace;
  --sans: 'Noto Sans SC', sans-serif;

  width: 100vw;
  height: 100vh;
  background: var(--light-grey);
  color: var(--text-main);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
  overflow: hidden; /* 禁止全局滚动 */
}

/* --- Header (Fixed) --- */
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
.logo-box { background: var(--text-inv); color: var(--black); width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-weight: bold; }
.header-right { display: flex; align-items: center; gap: 15px; font-size: 0.8rem; }
.status-indicator { font-size: 0.75rem; display: flex; align-items: center; gap: 6px; color: #00ff00; }
.dot { width: 8px; height: 8px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.sys-btn {
  background: transparent; border: 1px solid #444; color: #888;
  padding: 6px 12px; font-family: var(--mono); cursor: pointer; transition: 0.2s; font-size: 0.75rem;
}
.sys-btn:hover { background: #333; color: #fff; }
.sys-btn.primary { border-color: var(--red); color: var(--red); font-weight: bold; }
.sys-btn.primary:hover { background: var(--red); color: white; }
.blink { animation: blink 1s infinite; }

/* --- Main Layout (Flex) --- */
.main-layout {
  flex: 1;
  display: flex;
  overflow: hidden; /* 内部区域独立滚动 */
}

/* --- Sidebar (Left, 260px) --- */
.sidebar-nav {
  width: 260px;
  background: var(--black); /* 借鉴原 Console 风格 */
  color: var(--green);
  display: flex; flex-direction: column;
  border-right: 1px solid #333;
  flex-shrink: 0;
  font-family: var(--mono);
}

.nav-section {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex; flex-direction: column;
}

.nav-header {
  font-size: 0.75rem;
  color: #666;
  margin-bottom: 15px;
  border-bottom: 1px dashed #333;
  padding-bottom: 5px;
}

.nav-list { list-style: none; padding: 0; margin: 0; }
.nav-list li {
  padding: 10px; cursor: pointer; margin-bottom: 4px; font-size: 0.85rem;
  transition: 0.2s; border-left: 2px solid transparent; opacity: 0.7;
}
.nav-list li:hover { opacity: 1; background: rgba(0,255,65,0.1); }
.nav-list li.active {
  color: var(--black); background: var(--green);
  border-left-color: var(--red); opacity: 1; font-weight: bold;
}
.idx { opacity: 0.5; margin-right: 8px; font-size: 0.7rem; }

.nav-divider {
  height: 2px;
  background: #222;
  border-top: 1px solid #333;
  border-bottom: 1px solid #000;
}

.sidebar-footer {
  height: 40px;
  background: #000;
  border-top: 1px solid var(--red);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 15px; font-size: 0.7rem; color: #555;
}

/* --- Content Split (Right Area) --- */
.content-split {
  flex: 1;
  display: flex;
  background: #ddd; /* 缝隙颜色 */
  gap: 2px; /* 产生分割线效果 */
}

.content-col {
  flex: 1;
  background: var(--light-grey);
  overflow-y: auto;
  padding: 40px;
  position: relative;
}

/* --- Left Col Styles (Profile Forms) --- */
.left-col {
  background: #F4F1EA;
  background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px);
  background-size: 20px 20px;
  
}

.form-section {
  margin-bottom: 50px;
}
.section-header {
  display: flex; align-items: baseline; border-bottom: 2px solid var(--black);
  padding-bottom: 10px; margin-bottom: 20px;
}
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: var(--sans); }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: var(--mono); }

/* Inputs */
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; }
.form-group label { font-size: 0.7rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: var(--mono); }
.cyber-input, .cyber-textarea {
  border: 1px solid #999; background: #fff; padding: 10px;
  font-family: var(--sans); font-size: 0.9rem; outline: none; transition: 0.2s;
  width: 100%; display: block;
}
.cyber-input:focus, .cyber-textarea:focus { border-color: var(--red); box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

/* Avatar */
.avatar-row { display: flex; gap: 20px; align-items: center; margin-bottom: 20px; }
.avatar-preview { width: 80px; height: 80px; border: 1px solid var(--black); position: relative; overflow: hidden; background: #000; flex-shrink: 0; }
.avatar-preview img { width: 100%; height: 100%; object-fit: cover; }
.avatar-controls { flex: 1; display: flex; flex-direction: column; gap: 5px; }
.cyber-btn.sm { padding: 5px; font-size: 0.75rem; background: var(--black); color: #fff; border: none; cursor: pointer; width: 120px;}
.cyber-btn.sm:hover { background: var(--red); }

/* Tags & Links */
.tag-chip { background: var(--black); color: #fff; display: inline-flex; align-items: center; gap: 5px; padding: 2px 8px; font-size: 0.8rem; margin: 0 5px 5px 0; }
.remove-x { color: var(--red); cursor: pointer; }
.link-row { display: flex; gap: 5px; margin-bottom: 5px; }
.cyber-input.sm { width: 30%; }
.cyber-input.lg { flex: 1; }
.del-btn { background: #ccc; border: none; cursor: pointer; width: 30px; }
.del-btn:hover { background: var(--red); color: white; }
.add-link-btn { width: 100%; border: 1px dashed #999; background: transparent; padding: 8px; cursor: pointer; font-size: 0.8rem; }
.add-link-btn:hover { border-color: var(--black); background: #eee; }

/* --- Right Col Styles (Personalization Modules) --- */
.right-col {
  background: #F4F1EA;
  padding: 40px 30px;
  background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px);
  background-size: 20px 20px;
}

.config-module {
  background: #fff;
  border: 1px solid var(--black);
  margin-bottom: 30px;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

/* Module Header: Industrial Style */
.module-header {
  background: var(--dark-grey);
  color: #fff;
  padding: 8px 15px;
  display: flex; align-items: center; gap: 10px;
  font-family: var(--mono);
  font-size: 0.8rem;
  border-bottom: 1px solid var(--black);
}
.mod-id { color: var(--red); font-weight: bold; }
.mod-decor { flex: 1; height: 2px; background: repeating-linear-gradient(90deg, #444, #444 2px, transparent 2px, transparent 4px); }

.module-body {
  padding: 20px;
}

/* Module Contents */
.color-picker-grid { display: flex; gap: 10px; margin-top: 5px; }
.color-opt { width: 24px; height: 24px; border: 1px solid #000; cursor: pointer; }
.color-opt:hover { transform: scale(1.1); }
.pinned-item {
  border: 1px dashed #999; padding: 10px; text-align: center; font-size: 0.8rem; color: #666; margin-top: 5px;
  display: flex; justify-content: space-between;
}
.pinned-item.empty { background: #f9f9f9; cursor: pointer; }
.pinned-item.empty:hover { background: #eee; color: #000; border-color: #000; }
.item-status { font-size: 0.6rem; background: var(--green); color: #000; padding: 1px 4px; }

.achievement-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 15px; }
.ach-badge {
  aspect-ratio: 1; background: #333; color: #555; display: flex; align-items: center; justify-content: center;
  font-size: 0.6rem; text-align: center; border: 1px solid #000;
}
.ach-badge.active { background: var(--black); color: var(--red); border-color: var(--red); font-weight: bold; }

.end-marker {
  text-align: center; font-family: var(--mono); color: #bbb; margin-top: 40px; font-size: 0.7rem; letter-spacing: 2px;
}

/* Scrollbars */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #413e3a; }
.sidebar-nav .custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.sidebar-nav .custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--green); }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* Mobile Response (简易适配) */
@media (max-width: 900px) {
  .main-layout { flex-direction: column; overflow-y: auto; }
  .sidebar-nav { width: 100%; height: auto; border-right: none; max-height: 200px; }
  .content-split { flex-direction: column; }
  .content-col { overflow: visible; height: auto; }
  .settings-terminal { overflow-y: auto; height: auto; }
}
</style>