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

            <li :class="{ active: activeSection === 'pinned' }" @click="scrollTo('pinned', 'right')">
              <span class="idx">A1</span> 置顶内容
            </li>
            <li :class="{ active: activeSection === 'achieve' }" @click="scrollTo('achieve', 'right')">
              <span class="idx">A2</span> 成就矩阵
            </li>
            <li :class="{ active: activeSection === 'widgets' }" @click="scrollTo('widgets', 'right')">
              <span class="idx">A3</span> 挂件参数
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
            
            <div class="monitor-wrapper">
              <input type="file" ref="fileInput" accept="image/*" style="display: none" @change="handleFileChange" />
              
              <div class="monitor-frame" @click="triggerUpload">
                <div class="monitor-screen">
                  <img :src="form.avatar" alt="Avatar" class="screen-content" />
                  <div class="screen-overlay">
                    <div class="scan-line"></div>
                    <div class="screen-glare"></div>
                    <div class="click-hint">[ CLICK_TO_UPLOAD ]</div>
                  </div>
                </div>
                <div class="monitor-base">
                   <div class="led-light"></div>
                   <span class="model-text">SYS-VISUAL-01</span>
                </div>
              </div>

              <div class="upload-controls">
                <div class="system-hint warning-text">
                  <span class="icon">⚠</span> 
                  LIMIT: 5MB_MAX // 只支持高压缩比格式
                </div>
                <button class="cyber-btn primary full-width" @click="triggerUpload">
                  [ ↑ ] UPLOAD_IMAGE_DATA
                </button>
              </div>
            </div>

            <div class="form-group" style="margin-top: 20px;">
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
     </div>

  <div id="pinned" class="config-module">
    <div class="module-header">
      <span class="mod-id">MOD_A1</span>
      <span class="mod-name">PINNED_CONTENT // 置顶管理</span>
      <div class="mod-decor"></div>
    </div>
    <div class="module-body">
      
      <div class="sub-group">
        <label>FEATURED_WORK // 代表作 [4_SLOTS]</label>
        <div class="grid-row-4">
          <div 
            v-for="(item, index) in form.works" 
            :key="'w'+index" 
            class="square-slot"
            :class="{ empty: !item }"
            @click="openSelector('works', index)"
          >
            <template v-if="item">
              <span class="slot-type">{{ item.type }}</span>
              <span class="slot-name">{{ item.title }}</span>
            </template>
            <span v-else class="add-sign">+</span>
          </div>
        </div>
      </div>

      <div class="sub-group">
        <label>TOP_ARTICLES // 置顶文章 [4_SLOTS]</label>
        <div class="grid-row-4">
           <div 
            v-for="(item, index) in form.articles" 
            :key="'a'+index" 
            class="square-slot article-slot"
            :class="{ empty: !item }"
            @click="openSelector('articles', index)"
          >
            <template v-if="item">
              <div class="doc-icon">📄</div>
              <span class="slot-name tiny">{{ item.title }}</span>
            </template>
            <span v-else class="add-sign">+</span>
          </div>
        </div>
      </div>

    </div>
  </div>

  <div id="achieve" class="config-module">
      <div class="module-header">
      <span class="mod-id">MOD_A2</span>
      <span class="mod-name">ACHIEVEMENTS // 成就矩阵</span>
      <div class="mod-decor"></div>
    </div>
    <div class="module-body">
      <div class="grid-row-4 wrap-grid">
        <div 
          v-for="(badge, index) in form.achievements" 
          :key="'ach'+index"
          class="hex-slot" 
          :class="{ active: badge, empty: !badge }"
          @click="openSelector('achievements', index)"
        >
          <span v-if="badge">{{ badge }}</span>
          <span v-else class="tiny-idx">{{ index + 1 }}</span>
        </div>
      </div>
    </div>
  </div>

  <div id="inventory" class="config-module">
     <div class="module-header">
      <span class="mod-id">MOD_A3</span>
      <span class="mod-name">INVENTORY // 展示柜</span>
      <div class="mod-decor"></div>
    </div>
    <div class="module-body inventory-body">
      <div class="inventory-grid">
         <div 
          v-for="(item, index) in form.inventory" 
          :key="'inv'+index"
          class="inv-slot"
          :class="[item ? item.rarity : 'empty']"
          @click="openSelector('inventory', index)"
        >
          <div class="slot-inner">
             <template v-if="item">
                <span class="rarity-dot"></span>
                <span class="inv-name">{{ item.name }}</span>
             </template>
             <span v-else class="pattern-bg"></span>
          </div>
          <div class="slot-corner"></div>
        </div>
      </div>
      <div class="inventory-footer">
        <span>CAPACITY: 8/64</span>
        <span>SORT: DATE</span>
      </div>
    </div>
  </div>

  <div class="end-marker">/// END OF CUSTOM_CONFIG ///</div>
</div>

<div v-if="selectorState.visible" class="modal-overlay" @click.self="selectorState.visible = false">
  <div class="selector-window">
    <div class="sel-header">
      <span>{{ selectorState.title }}</span>
      <button class="close-x" @click="selectorState.visible = false">×</button>
    </div>
    <div class="sel-body custom-scroll">
      
      <div 
        v-for="(opt, idx) in currentOptions" 
        :key="idx" 
        class="sel-option"
        @click="selectItem(opt)"
      >
        <span class="opt-bullet">>></span>
        <span class="opt-text">{{ typeof opt === 'object' ? (opt.title || opt.name) : opt }}</span>
        <span v-if="typeof opt === 'object'" class="opt-tag">{{ opt.type || opt.rarity }}</span>
      </div>

      <div class="sel-option destructive" @click="clearSlot">
        <span class="opt-bullet">X</span>
        <span class="opt-text">CLEAR_SLOT // 清空该槽位</span>
      </div>

    </div>
    <div class="sel-footer">
      DATABASE_CONNECTED // RESULTS: {{ currentOptions.length }}
    </div>
  </div>
</div>

      </main>

      <transition name="fade">
        <div v-if="uploadModal.visible" class="upload-modal-overlay">
          <div class="upload-terminal-window">
            <div class="term-header">
              <span>>> UPLOAD_PROTOCOL</span>
              <span class="close-btn" @click="cancelUpload">X</span>
            </div>
            <div class="term-body">
              <div class="process-text">
                <p>> INITIATING HANDSHAKE...</p>
                <p>> ANALYZING BIOMETRIC DATA...</p>
                <p v-if="uploadModal.progress > 30" class="success">> FORMAT CHECK: PASSED</p>
                <p v-if="uploadModal.progress > 60">> COMPRESSING STREAM...</p>
              </div>
              
              <div class="preview-stage">
                <img :src="uploadModal.tempImg" class="temp-preview" />
                <div class="scan-grid"></div>
              </div>

              <div class="progress-bar-container">
                <div class="progress-fill" :style="{ width: uploadModal.progress + '%' }"></div>
              </div>
              <div class="progress-text">{{ uploadModal.progress }}% // TRANSFERRING</div>
              
              <button v-if="uploadModal.progress === 100" class="sys-btn primary full" @click="confirmUpload">
                [ EXECUTE ] APPLY CHANGES
              </button>
            </div>
          </div>
        </div>
      </transition>

    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
  

onMounted(() => {
  document.body.style.overflow = 'hidden'
})

onUnmounted(() => {
  document.body.style.overflow = ''
})

// --- State ---
const activeSection = ref('identity')
const newTagInput = ref('')

// File Upload State
const fileInput = ref(null)
const uploadModal = reactive({
  visible: false,
  progress: 0,
  tempImg: null,
  file: null
})

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
  ],
  // 新增/修改的数据结构
  works: [
    { id: 1, title: 'Project A', type: 'PRO' },
    null, null, null // 占位符
  ],
  articles: [
    { id: 1, title: 'Vue3 Guide', type: 'DOC' },
    { id: 2, title: 'CSS Houdini', type: 'CSS' },
    null, null
  ],
  achievements: [
    '100 Days', 'Contributor', 'Bug Hunter', 'Sponsor',
    null, null, null, null // 8个槽位
  ],
  inventory: [
    { name: 'Mech Key', rarity: 'legendary' },
    { name: 'Coffee', rarity: 'common' },
    null, null, null, null, null, null
  ]
})


// --- 通用选择器逻辑 (Selector Logic) ---
const selectorState = reactive({
  visible: false,
  type: '', // 'works' | 'articles' | 'achievements' | 'inventory'
  targetIndex: -1,
  title: ''
})

// 模拟数据库中的备选数据
const database = {
  works: [
    { title: 'SaaS Dashboard', type: 'PRO' },
    { title: 'E-Commerce UI', type: 'UI' },
    { title: 'Three.js Earth', type: '3D' },
    { title: 'Cyber Blog', type: 'WEB' }
  ],
  articles: [
    { title: 'React vs Vue', type: 'DOC' },
    { title: 'JS Async/Await', type: 'JS' },
    { title: 'Grid Layout', type: 'CSS' },
    { title: 'Web Assembly', type: 'WASM' }
  ],
  achievements: [
    'Top Writer', 'Mentor', 'Early Bird', 'Night Owl', 'Full Stack', 'Designer'
  ],
  inventory: [
    { name: 'RTX 4090', rarity: 'legendary' },
    { name: 'Switch', rarity: 'epic' },
    { name: 'Book', rarity: 'common' },
    { name: 'Headset', rarity: 'rare' }
  ]
}

// 计算当前弹窗应该显示的数据列表
const currentOptions = computed(() => {
  return database[selectorState.type] || []
})

// 打开选择器
const openSelector = (type, index) => {
  selectorState.type = type
  selectorState.targetIndex = index
  selectorState.title = `SELECT_${type.toUpperCase()} // SLOT_${index < 9 ? '0'+(index+1) : index+1}`
  selectorState.visible = true
}

// 选中数据
const selectItem = (item) => {
  form[selectorState.type][selectorState.targetIndex] = item
  selectorState.visible = false
}

// 清空该槽位
const clearSlot = () => {
   form[selectorState.type][selectorState.targetIndex] = null
   selectorState.visible = false
}

// --- Methods ---

const goBack = () => {
  if (confirm('ABORT CHANGES?')) window.history.back()
}

const saveAll = () => {
  alert('SYSTEM_MSG: Configuration packets transmitted.')
}

// --- New Avatar Logic ---

const triggerUpload = () => {
  fileInput.value.click()
}

const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (!file) return

  // Size Limit Check (5MB)
  if (file.size > 5 * 1024 * 1024) {
    alert('SYSTEM_ERROR: FILE_SIZE_EXCEEDED (MAX 5MB)')
    return
  }

  // Create preview URL and Open Modal
  const reader = new FileReader()
  reader.onload = (e) => {
    uploadModal.tempImg = e.target.result
    uploadModal.file = file
    uploadModal.visible = true
    uploadModal.progress = 0
    simulateUpload()
  }
  reader.readAsDataURL(file)
  
  // Reset input so same file can be selected again if cancelled
  event.target.value = ''
}

const simulateUpload = () => {
  const interval = setInterval(() => {
    if (uploadModal.progress < 100) {
      uploadModal.progress += Math.floor(Math.random() * 15)
      if (uploadModal.progress > 100) uploadModal.progress = 100
    } else {
      clearInterval(interval)
    }
  }, 200)
}

const confirmUpload = () => {
  form.avatar = uploadModal.tempImg
  uploadModal.visible = false
  // 这里实际项目中会发送 API 请求
}

const cancelUpload = () => {
  uploadModal.visible = false
  uploadModal.tempImg = null
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
const scrollTo = (id, colType) => {
  activeSection.value = id
  const containerId = colType === 'left' ? 'col-left' : 'col-right'
  const container = document.getElementById(containerId)
  const target = document.getElementById(id)
  
  if (container && target) {
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

/* --- Main Layout --- */
.main-layout { flex: 1; display: flex; overflow: hidden; position: relative; }

/* --- Sidebar --- */
.sidebar-nav {
  width: 260px;
  max-width: 98vw;
  background: var(--black);
  color: var(--green);
  display: flex; flex-direction: column;
  border-right: 1px solid #333;
  flex-shrink: 0;
  font-family: var(--mono);
  min-height: 500px;
}
.nav-section { flex: 1; overflow-y: auto; padding: 20px; display: flex; flex-direction: column; }
.nav-header { font-size: 0.75rem; color: #666; margin-bottom: 15px; border-bottom: 1px dashed #333; padding-bottom: 5px; }

.nav-list li { padding: 10px; cursor: pointer; margin-bottom: 4px; font-size: 0.85rem; transition: 0.2s; border-left: 2px solid transparent; opacity: 0.7; }
.nav-list li:hover { opacity: 1; background: rgba(0,255,65,0.1); }
.nav-list li.active { color: var(--black); background: var(--green); border-left-color: var(--red); opacity: 1; font-weight: bold; }
.idx { opacity: 0.5; margin-right: 8px; font-size: 0.7rem; }
.nav-divider { height: 2px; background: #222; border-top: 1px solid #333; border-bottom: 1px solid #000; }
.sidebar-footer { height: 40px; background: #000; border-top: 1px solid var(--red); display: flex; justify-content: space-between; align-items: center; padding: 0 15px; font-size: 0.7rem; color: #555; }

/* --- Content Split --- */
.content-split { flex: 1; display: flex; background: #ddd; gap: 2px; }
.content-col { flex: 1; background: var(--light-grey); overflow-y: auto; padding: 40px; position: relative; }
.left-col { background: #F4F1EA; background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px); background-size: 20px 20px; }
.right-col { background: #F4F1EA; padding: 40px 30px; background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px); background-size: 20px 20px; }

/* --- Forms --- */
.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid var(--black); padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: var(--sans); }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: var(--mono); }
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; padding-top: 10px; }
.form-group label { font-size: 0.7rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: var(--mono); }
.cyber-input, .cyber-textarea { border: 1px solid #999; background: #fff; padding: 10px; font-family: var(--sans); font-size: 0.9rem; outline: none; transition: 0.2s; width: 100%; display: block; }
.cyber-input:focus, .cyber-textarea:focus { border-color: var(--red); box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

/* --- New Monitor/Avatar Styles --- */
.monitor-wrapper {
  display: flex;
  gap: 25px;
  align-items: center;
}

.monitor-frame {
  width: 140px;
  background: #2a2a2a;
  padding: 2px 2px 16px 2px; /* Bottom padding for base text */
  border-radius: 0px 0px 0px 0px;
  box-shadow: 
    0 0 0 2px #111,
    4px 4px 10px rgba(0,0,0,0.3);
  cursor: pointer;
  transition: transform 0.2s;
}

.monitor-frame:hover {
  transform: scale(1.02);
}

.monitor-screen {
  background: #000;
  width: 100%;
  aspect-ratio: 1;
  border-radius: 4px;
  position: relative;
  overflow: hidden;
  border: 2px solid #000;
  box-shadow: inset 0 0 10px rgba(255,255,255,0.1);
}

.screen-content {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: contrast(1.1) brightness(0.9);
}

.screen-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.25) 50%), linear-gradient(90deg, rgba(255, 0, 0, 0.06), rgba(0, 255, 0, 0.02), rgba(0, 0, 255, 0.06));
  background-size: 100% 2px, 3px 100%;
  pointer-events: none;
  display: flex;
  align-items: center;
  justify-content: center;
}

.click-hint {
  color: var(--green);
  font-family: var(--mono);
  font-size: 0.6rem;
  opacity: 0;
  background: rgba(0,0,0,0.8);
  padding: 2px 4px;
  transition: opacity 0.2s;
}
.monitor-frame:hover .click-hint { opacity: 1; }

.monitor-base {
  height: 0;
  position: relative;
}
.led-light {
  width: 4px; height: 4px; background: #0f0;
  position: absolute; bottom: -8px; right: 5px;
  border-radius: 50%;
  box-shadow: 0 0 4px #0f0;
}
.model-text {
  position: absolute; bottom: -10px; left: 2px;
  font-size: 0.5rem; color: #666; font-family: var(--mono);
}

.upload-controls {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.system-hint {
  font-family: var(--mono);
  font-size: 0.7rem;
  color: #666;
  border-left: 2px solid #ff9900;
  padding-left: 8px;
  line-height: 1.4;
}
.cyber-btn.primary {
  background: var(--black);
  color: var(--red);
  border: 1vh black;
  padding: 10px;
  font-weight: bold;
  font-family: var(--mono);
}
.cyber-btn.primary:hover {
  background: #000000;
  color: #ffffff;
}

/* --- Upload Modal (Secondary Page) --- */
.upload-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.85);
  backdrop-filter: blur(4px);
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
}

.upload-terminal-window {
  width: 500px;
  max-width: 90%;
  background: #0d0d0d;
  border: 1px solid #444;
  box-shadow: 0 20px 50px rgba(0,0,0,0.8);
  font-family: var(--mono);
  color: #ddd;
}

.term-header {
  background: #333;
  padding: 8px 12px;
  font-size: 0.8rem;
  display: flex; justify-content: space-between;
  border-bottom: 1px solid #555;
  color: #fff;
}
.close-btn { cursor: pointer; }
.close-btn:hover { color: var(--red); }

.term-body { padding: 20px; display: flex; flex-direction: column; gap: 15px; }

.process-text {
  min-height: 80px;
  font-size: 0.75rem;
  color: #888;
}
.process-text p { margin: 4px 0; }
.process-text .success { color: var(--green); }

.preview-stage {
  height: 150px;
  border: 1px dashed #444;
  background: #111;
  position: relative;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden;
}
.temp-preview { height: 100%; opacity: 0.8; }
.scan-grid {
  position: absolute; top:0; left:0; width:100%; height:100%;
  background: 
    linear-gradient(90deg, rgba(0,255,0,0.1) 1px, transparent 1px),
    linear-gradient(rgba(0,255,0,0.1) 1px, transparent 1px);
  background-size: 20px 20px;
}

.progress-bar-container {
  height: 6px;
  background: #222;
  border: 1px solid #444;
  width: 100%;
}
.progress-fill {
  height: 100%;
  background: var(--green);
  transition: width 0.2s;
  box-shadow: 0 0 10px var(--green);
}
.progress-text { text-align: right; font-size: 0.7rem; color: var(--green); }

.sys-btn.full { width: 100%; padding: 12px; margin-top: 10px; }

/* Transitions */
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

/* --- Other Elements (Tags, Modules) --- */
.tag-chip { background: var(--black); color: #fff; display: inline-flex; align-items: center; gap: 5px; padding: 2px 8px; font-size: 0.8rem; margin: 0 5px 5px 0; }
.remove-x { color: var(--red); cursor: pointer; }
.link-row { display: flex; gap: 5px; margin-bottom: 5px; }
.cyber-input.sm { width: 30%; }
.cyber-input.lg { flex: 1; }
.del-btn { background: #ccc; border: none; cursor: pointer; width: 30px; }
.del-btn:hover { background: var(--red); color: white; }
.add-link-btn { width: 100%; border: 1px dashed #999; background: transparent; padding: 8px; cursor: pointer; font-size: 0.8rem; }
.add-link-btn:hover { border-color: var(--black); background: #eee; }

/* Module Generic */
.config-module { background: #fff; border: 1px solid var(--black); margin-bottom: 30px; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.module-header { background: var(--dark-grey); color: #fff; padding: 8px 15px; display: flex; align-items: center; gap: 10px; font-family: var(--mono); font-size: 0.8rem; border-bottom: 1px solid var(--black); }
.mod-id { color: var(--red); font-weight: bold; }
.mod-decor { flex: 1; height: 2px; background: repeating-linear-gradient(90deg, #444, #444 2px, transparent 2px, transparent 4px); }
.module-body { padding: 20px; }
.color-picker-grid { display: flex; gap: 10px; margin-top: 5px; }
.color-opt { width: 24px; height: 24px; border: 1px solid #000; cursor: pointer; }
.color-opt:hover { transform: scale(1.1); }
.pinned-item { border: 1px dashed #999; padding: 10px; text-align: center; font-size: 0.8rem; color: #666; margin-top: 5px; display: flex; justify-content: space-between; }
.pinned-item.empty { background: #f9f9f9; cursor: pointer; }
.pinned-item.empty:hover { background: #eee; color: #000; border-color: #000; }
.item-status { font-size: 0.6rem; background: var(--green); color: #000; padding: 1px 4px; }
.achievement-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 15px; }
.ach-badge { aspect-ratio: 1; background: #333; color: #555; display: flex; align-items: center; justify-content: center; font-size: 0.6rem; text-align: center; border: 1px solid #000;width: 50%; }
.ach-badge.active { background: var(--black); color: var(--red); border-color: var(--red); font-weight: bold; }
.end-marker { text-align: center; font-family: var(--mono); color: #bbb; margin-top: 40px; font-size: 0.7rem; letter-spacing: 2px; }
.sub-group {padding-top: 10px; padding-bottom: 10px;}



/* --- 通用 Grid 辅助类 --- */
.grid-row-4 {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
  width: 100%;
}
.wrap-grid {
  grid-template-rows: repeat(2, 1fr); /* 两行 */
}

/* --- Pin/Article Square Slots (正方形按钮) --- */
.square-slot {
  aspect-ratio: 1;
  background: #fff;
  border: 1px solid #333;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
  padding: 5px;
  text-align: center;
}

.square-slot:hover {
  background: #eee;
  border-color: var(--red);
  transform: translateY(-2px);
  box-shadow: 2px 2px 0 rgba(0,0,0,0.2);
}

.square-slot.empty {
  border: 1px dashed #999;
  background: #f9f9f9;
  color: #ccc;
}
.square-slot.empty:hover {
  border-color: #666;
  color: #666;
}

.add-sign { font-size: 1.5rem; font-weight: bold; }
.slot-type { 
  font-size: 0.6rem; background: var(--black); color: #fff; 
  padding: 1px 4px; position: absolute; top: 5px; right: 5px; 
}
.slot-name { 
  font-size: 0.75rem; font-weight: bold; line-height: 1.2; word-break: break-all;
}
.slot-name.tiny { font-size: 0.65rem; color: #555; }
.doc-icon { font-size: 1.2rem; margin-bottom: 5px; }

/* --- Achievements Hex Slots (成就) --- */
.hex-slot {
  aspect-ratio: 1;
  border: 1px solid #444;
  background: #2a2a2a;
  color: #666;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.65rem;
  cursor: pointer;
  text-align: center;
  clip-path: polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%); /* 切角矩形 */
  transition: 0.2s;
}
.hex-slot:hover { border-color: #fff; color: #fff; }
.hex-slot.active {
  background: var(--black);
  border: 1px solid var(--red);
  color: var(--red);
  font-weight: bold;
  box-shadow: inset 0 0 10px rgba(217, 35, 35, 0.2);
}
.tiny-idx { font-family: var(--mono); font-size: 0.6rem; opacity: 0.3; }

/* --- Inventory Module (仓库) --- */
.inventory-body {
  background: #1a1a1a; /* 深色底 */
  padding: 15px;
}

.inventory-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-auto-rows: 1fr;
  gap: 8px;
}

.inv-slot {
  aspect-ratio: 1;
  background: #252525;
  border: 1px solid #444;
  position: relative;
  cursor: pointer;
  transition: 0.2s;
}

.inv-slot:hover { z-index: 2; transform: scale(1.05); border-color: #888; background: #333; }

.slot-inner {
  width: 100%; height: 100%;
  display: flex; flex-direction: column;
  justify-content: center; align-items: center;
  padding: 4px;
}

/* 稀有度颜色条 */
.inv-slot::before {
  content: ''; position: absolute; left: 0; bottom: 0; 
  width: 3px; height: 100%; background: #333;
}
.inv-slot.legendary::before { background: #ffaa00; box-shadow: 0 0 5px #ffaa00; }
.inv-slot.epic::before { background: #aa00ff; }
.inv-slot.rare::before { background: #0088ff; }
.inv-slot.common::before { background: #888; }
.inv-slot.empty::before { display: none; }

/* 空槽位纹理 */
.pattern-bg {
  width: 100%; height: 100%; opacity: 0.1;
  background-image: repeating-linear-gradient(45deg, #ccc 0, #ccc 1px, transparent 0, transparent 50%);
  background-size: 10px 10px;
}

.inv-name { color: #eee; font-size: 0.6rem; text-align: center; font-family: var(--mono); }
.rarity-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; margin-bottom: 5px; opacity: 0.8; }
.slot-corner {
  position: absolute; top: 0; right: 0; 
  width: 0; height: 0; 
  border-style: solid; border-width: 0 10px 10px 0; 
  border-color: transparent #333 transparent transparent;
}

.inventory-footer {
  margin-top: 10px;
  border-top: 1px dashed #444;
  padding-top: 5px;
  display: flex; justify-content: space-between;
  color: #666; font-family: var(--mono); font-size: 0.6rem;
}

/* --- Selector Modal (通用弹窗) --- */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.6); backdrop-filter: blur(2px);
  z-index: 200; display: flex; align-items: center; justify-content: center;
}
.selector-window {
  width: 300px; max-height: 400px;
  background: #f4f4f4; border: 2px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.5);
  display: flex; flex-direction: column;
}
.sel-header {
  background: var(--black); color: #fff; padding: 8px 10px;
  font-family: var(--mono); font-size: 0.75rem;
  display: flex; justify-content: space-between; align-items: center;
}
.close-x { background: none; border: none; color: #fff; font-size: 1.2rem; cursor: pointer; }
.sel-body { padding: 10px; overflow-y: auto; flex: 1; }
.sel-option {
  padding: 8px; border-bottom: 1px solid #ddd; cursor: pointer;
  display: flex; align-items: center; font-family: var(--mono); font-size: 0.8rem;
  transition: 0.2s;
}
.sel-option:hover { background: #ddd; padding-left: 12px; }
.opt-bullet { color: var(--red); margin-right: 8px; font-weight: bold; }
.opt-text { flex: 1; color: #333; }
.opt-tag { font-size: 0.6rem; background: #333; color: #fff; padding: 2px 4px; border-radius: 2px; }
.destructive .opt-text { color: var(--red); }
.sel-footer {
  background: #e5e5e5; padding: 5px 10px; font-size: 0.6rem; color: #666;
  border-top: 1px solid #ccc; font-family: var(--mono);
}
/* Scrollbars */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #413e3a; }
.sidebar-nav .custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.sidebar-nav .custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--green); }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

@media (max-width: 900px) {
  .main-layout { flex-direction: column; overflow-y: auto; }
  .sidebar-nav { width: 100%; height: auto; border-right: none; max-height: 200px; }
  .content-split { flex-direction: column; }
  .content-col { overflow: visible; height: auto; }
  .settings-terminal { overflow-y: auto; height: auto; }
  .upload-terminal-window { width: 95%; }
}
</style>