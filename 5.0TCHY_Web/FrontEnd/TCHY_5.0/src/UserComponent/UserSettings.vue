<template>
  <div class="settings-terminal">
    <header class="terminal-header">
      <div class="header-left">
        <div class="brand-block">
          <span class="logo-box">S</span>
          <span class="brand-text">配置中心 // CONFIG_SYS</span>
        </div>
      </div>
      <div class="header-right">
        <div class="status-indicator">
          <span class="dot blink"></span> 写入模式: 开启
        </div>
        <button class="sys-btn warning" @click="goBack">
          [ ESC ] 取消并返回
        </button>
        <button class="sys-btn primary" @click="saveAll">
          [ ENTER ] 保存变更
        </button>
      </div>
    </header>

    <div class="main-layout">
      <aside class="sidebar-nav">
        <div class="nav-title">导航 // NAV</div>
        <ul class="nav-list">
          <li :class="{ active: activeSection === 'identity' }" @click="scrollTo('identity')">01. 身份识别</li>
          <li :class="{ active: activeSection === 'basic' }" @click="scrollTo('basic')">02. 基础档案</li>
          <li :class="{ active: activeSection === 'tags' }" @click="scrollTo('tags')">03. 标签管理</li>
          <li :class="{ active: activeSection === 'contact' }" @click="scrollTo('contact')">04. 通讯终端</li>
          <li :class="{ active: activeSection === 'links' }" @click="scrollTo('links')">05. 外部链路</li>
        </ul>
        
        <div class="console-box">
          <div class="console-header">SYSTEM_LOG >></div>
          <div class="console-content custom-scroll">
            <div v-for="(log, i) in logs" :key="i" :class="log.type">
              > {{ log.text }}
            </div>
          </div>
        </div>
      </aside>

      <main class="form-area custom-scroll" @scroll="handleScroll">
        
        <section id="identity" class="form-section">
          <div class="section-header">
            <span class="sec-num">01</span>
            <span class="sec-title">身份识别 // IDENTITY_MATRIX</span>
          </div>
          
          <div class="form-row avatar-row">
            <div class="avatar-preview">
              <img :src="form.avatar" alt="Avatar Preview" />
              <div class="scan-line"></div>
            </div>
            <div class="avatar-controls">
              <label class="field-label">头像源 // AVATAR_SRC</label>
              <input type="text" v-model="form.avatar" class="cyber-input" placeholder="输入图片URL..." />
              <div class="btn-group">
                <button class="cyber-btn sm" @click="randomAvatar">🎲 随机生成</button>
                <button class="cyber-btn sm">↥ 上传文件</button>
              </div>
            </div>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label>昵称 // NICKNAME</label>
              <input type="text" v-model="form.nickname" class="cyber-input bold" />
            </div>
            <div class="form-group">
              <label>职业 // ROLE</label>
              <input type="text" v-model="form.role" class="cyber-input" />
            </div>
          </div>
        </section>

        <section id="basic" class="form-section">
          <div class="section-header">
            <span class="sec-num">02</span>
            <span class="sec-title">基础档案 // BASIC_DATA</span>
          </div>

          <div class="form-group">
            <label>个人简介 // BIO_TEXT</label>
            <textarea v-model="form.bio" class="cyber-textarea" rows="4" placeholder="输入你的个人简介..."></textarea>
            <div class="char-count">{{ form.bio.length }} / 200 chars</div>
          </div>

          <div class="form-grid three-col">
            <div class="form-group">
              <label>年龄 // AGE</label>
              <input type="number" v-model="form.age" class="cyber-input" />
            </div>
            <div class="form-group">
              <label>性别 // SEX</label>
              <select v-model="form.gender" class="cyber-select">
                <option value="Male / M">Male / M</option>
                <option value="Female / F">Female / F</option>
                <option value="Bot / B">Bot / B</option>
                <option value="Unknown">Unknown</option>
              </select>
            </div>
            <div class="form-group">
              <label>坐标 // LOC</label>
              <input type="text" v-model="form.location" class="cyber-input" />
            </div>
          </div>
        </section>

        <section id="tags" class="form-section">
          <div class="section-header">
            <span class="sec-num">03</span>
            <span class="sec-title">标签管理 // TAG_SYSTEM</span>
          </div>
          
          <div class="tag-manager">
            <div class="current-tags">
              <span v-for="(tag, index) in form.tags" :key="index" class="tag-chip">
                {{ tag }}
                <span class="remove-x" @click="removeTag(index)">×</span>
              </span>
            </div>
            <div class="add-tag-row">
              <input 
                type="text" 
                v-model="newTagInput" 
                @keyup.enter="addTag"
                class="cyber-input" 
                placeholder="输入标签后回车添加..." 
              />
              <button class="cyber-btn" @click="addTag">ADD +</button>
            </div>
          </div>
        </section>

        <section id="contact" class="form-section">
          <div class="section-header">
            <span class="sec-num">04</span>
            <span class="sec-title">通讯终端 // CONNECT_HUB</span>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label>电子邮箱 // EMAIL</label>
              <input type="email" v-model="form.email" class="cyber-input" />
            </div>
            <div class="form-group">
              <label>腾讯QQ // TENCENT_ID</label>
              <input type="text" v-model="form.qq" class="cyber-input" />
            </div>
          </div>
        </section>

        <section id="links" class="form-section">
          <div class="section-header">
            <span class="sec-num">05</span>
            <span class="sec-title">外部链路 // HYPERLINKS</span>
          </div>

          <div class="links-editor">
            <div v-for="(link, index) in form.externalLinks" :key="index" class="link-row">
              <span class="row-idx">{{ (index + 1).toString().padStart(2,'0') }}</span>
              <input type="text" v-model="link.name" class="cyber-input sm" placeholder="平台名称 (e.g. GitHub)" />
              <input type="text" v-model="link.url" class="cyber-input lg" placeholder="URL..." />
              <button class="del-btn" @click="removeLink(index)">[ DEL ]</button>
            </div>
            
            <button class="add-link-btn" @click="addLink">
              + 添加新链路节点 // ADD_NODE
            </button>
          </div>
        </section>
        
        <div class="form-footer">
          <div class="footer-deco">--- END OF CONFIG ---</div>
        </div>

      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'

// --- State ---
const activeSection = ref('identity')
const newTagInput = ref('')
const logs = ref([
  { text: 'System Initialized...', type: 'info' },
  { text: 'Loading User Config...', type: 'info' },
  { text: 'Ready for Input.', type: 'success' }
])

// --- Form Data (初始值模拟) ---
const form = reactive({
  avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
  nickname: '峰峰子',
  role: '视觉前端 // VISUAL_ENG',
  bio: '原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼原神牛逼。',
  age: 24,
  gender: 'Male / M',
  location: 'Guangzhou, CN',
  creationAge: '4年', 
  email: 'fengfengzi@cyber.com',
  qq: '1145141919',
  tags: ['界面设计', 'Vue开发', '三维艺术', 'Cyberpunk'],
  externalLinks: [
    { name: 'GitHub', url: 'https://github.com' },
    { name: 'Bilibili', url: 'https://bilibili.com' },
    { name: 'Dribbble', url: 'https://dribbble.com' }
  ]
})

// --- Methods ---

const goBack = () => {
  // 实际场景用 router.back()
  if (confirm('检测到未保存的更改。确定要中止并返回吗？')) {
    addLog('Process Aborted.', 'error')
    setTimeout(() => {
      if(window.history.length > 1) window.history.back()
    }, 500)
  }
}

const saveAll = () => {
  addLog('Validating Data Packets...', 'info')
  setTimeout(() => {
    addLog('Writing to Database...', 'warning')
    setTimeout(() => {
      addLog('Config Saved Successfully.', 'success')
      alert('保存成功！\n[System]: Configuration Updated.')
      // 此处调用API保存
    }, 800)
  }, 600)
}

// 头像随机
const randomAvatar = () => {
  const seeds = ['Felix', 'Aneka', 'Zoe', 'Midnight', 'Bear']
  const randomSeed = seeds[Math.floor(Math.random() * seeds.length)]
  form.avatar = `https://api.dicebear.com/7.x/notionists/svg?seed=${randomSeed + Math.random()}`
  addLog('Avatar Hash Regenerated.', 'info')
}

// 标签管理
const addTag = () => {
  const val = newTagInput.value.trim()
  if (val && !form.tags.includes(val)) {
    form.tags.push(val)
    newTagInput.value = ''
    addLog(`Tag [${val}] appended.`, 'success')
  }
}
const removeTag = (index) => {
  const removed = form.tags.splice(index, 1)
  addLog(`Tag [${removed}] detached.`, 'warning')
}

// 链接管理
const addLink = () => {
  form.externalLinks.push({ name: '', url: '' })
}
const removeLink = (index) => {
  form.externalLinks.splice(index, 1)
}

// 日志工具
const addLog = (text, type = 'info') => {
  logs.value.push({ text, type })
  // 保持日志在最新
  setTimeout(() => {
    const el = document.querySelector('.console-content')
    if(el) el.scrollTop = el.scrollHeight
  }, 50)
}

// 滚动监听 (简单的)
const scrollTo = (id) => {
  const el = document.getElementById(id)
  if (el) el.scrollIntoView({ behavior: 'smooth' })
  activeSection.value = id
}
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

.settings-terminal {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #ccc;
  --mono: 'JetBrains Mono', monospace;
  --sans: 'Noto Sans SC', sans-serif;

  width: 100vw;
  height: 100vh;
  background: var(--white);
  color: var(--black);
  font-family: var(--mono), var(--sans);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* --- Header --- */
.terminal-header {
  height: 60px;
  background: var(--black);
  color: var(--white);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 20px;
  border-bottom: 4px solid var(--red);
  flex-shrink: 0;
}
.brand-block { display: flex; align-items: center; gap: 10px; font-weight: bold; }
.logo-box { background: var(--white); color: var(--black); width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-weight: bold; }
.header-right { display: flex; align-items: center; gap: 15px; }
.status-indicator { font-size: 0.75rem; color: #00ff00; margin-right: 15px; }
.dot { display: inline-block; width: 8px; height: 8px; background: #00ff00; border-radius: 50%; margin-right: 5px; }
.sys-btn {
  border: 1px solid #666; background: transparent; color: #ccc;
  padding: 5px 15px; font-family: var(--mono); cursor: pointer; transition: 0.2s; font-size: 0.75rem;
}
.sys-btn:hover { background: #333; color: #fff; }
.sys-btn.primary { border-color: var(--red); color: var(--red); font-weight: bold; }
.sys-btn.primary:hover { background: var(--red); color: var(--white); }
.sys-btn.warning:hover { color: #ffff00; border-color: #ffff00; }

/* --- Layout --- */
.main-layout { flex: 1; display: flex; overflow: hidden; }

/* --- Sidebar --- */
.sidebar-nav {
  width: 260px;
  background: #eee;
  border-right: 2px solid var(--black);
  display: flex; flex-direction: column;
  padding: 20px;
}
.nav-title { font-weight: bold; margin-bottom: 15px; border-bottom: 2px solid var(--black); padding-bottom: 5px; }
.nav-list { list-style: none; padding: 0; margin-bottom: auto; }
.nav-list li {
  padding: 10px; cursor: pointer; border: 1px solid transparent; margin-bottom: 5px; font-size: 0.85rem; transition: 0.2s;
}
.nav-list li:hover { background: #ddd; }
.nav-list li.active { background: var(--black); color: var(--white); border-left: 5px solid var(--red); }

/* Console */
.console-box {
  background: #000; color: #00ff00; border: 2px solid #333; font-size: 0.7rem; display: flex; flex-direction: column; height: 200px;
}
.console-header { background: #333; color: #fff; padding: 2px 5px; font-weight: bold; }
.console-content { padding: 10px; overflow-y: auto; flex: 1; font-family: 'Courier New', monospace; }
.console-content .error { color: #ff0000; }
.console-content .warning { color: #ffff00; }
.console-content .success { color: #00ff00; }

/* --- Form Area --- */
.form-area { flex: 1; padding: 40px; overflow-y: auto; background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px); background-size: 40px 40px; position: relative; scroll-behavior: smooth; }

.form-section {
  background: var(--white);
  border: 2px solid var(--black);
  padding: 30px;
  margin-bottom: 40px;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}

.section-header { display: flex; align-items: center; border-bottom: 2px solid var(--black); padding-bottom: 15px; margin-bottom: 25px; }
.sec-num { font-size: 2rem; font-weight: bold; color: #ddd; margin-right: 15px; line-height: 1; }
.sec-title { font-size: 1.2rem; font-weight: bold; font-family: var(--mono); }

/* Inputs Common */
.form-group { margin-bottom: 20px; display: flex; flex-direction: column; }
.form-group label { font-size: 0.75rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: var(--mono); }
.cyber-input, .cyber-textarea, .cyber-select {
  border: 2px solid var(--black);
  background: #f9f9f9;
  padding: 10px;
  font-family: var(--sans);
  font-size: 0.9rem;
  outline: none;
  transition: 0.2s;
  width: 100%;
}
.cyber-input:focus, .cyber-textarea:focus, .cyber-select:focus {
  border-color: var(--red);
  box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.1);
  background: #fff;
}
.cyber-input.bold { font-weight: bold; font-size: 1.1rem; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.form-grid.three-col { grid-template-columns: 1fr 1fr 1fr; }

/* Specific: Avatar */
.avatar-row { display: flex; gap: 30px; align-items: center; margin-bottom: 20px; }
.avatar-preview { width: 100px; height: 100px; border: 2px solid var(--black); position: relative; overflow: hidden; background: #eee; flex-shrink: 0; }
.avatar-preview img { width: 100%; height: 100%; object-fit: cover; }
.scan-line { position: absolute; top: 0; left: 0; width: 100%; height: 2px; background: rgba(0,255,0,0.5); animation: scan 2s infinite linear; }
.avatar-controls { flex: 1; display: flex; flex-direction: column; gap: 10px; }
.btn-group { display: flex; gap: 10px; margin-top: 5px; }

/* Specific: Tags */
.tag-manager { border: 2px solid #ddd; padding: 15px; background: #fafafa; }
.current-tags { display: flex; flex-wrap: wrap; gap: 10px; margin-bottom: 15px; }
.tag-chip { background: var(--black); color: var(--white); padding: 5px 12px; font-size: 0.8rem; display: flex; align-items: center; gap: 8px; }
.remove-x { cursor: pointer; color: var(--red); font-weight: bold; }
.remove-x:hover { color: #fff; }
.add-tag-row { display: flex; gap: 10px; }

/* Specific: Links */
.link-row { display: flex; gap: 10px; align-items: center; margin-bottom: 10px; }
.row-idx { font-family: var(--mono); color: #aaa; width: 30px; }
.cyber-input.sm { width: 150px; }
.cyber-input.lg { flex: 1; }
.del-btn { background: transparent; border: none; color: #999; cursor: pointer; font-family: var(--mono); font-size: 0.8rem; }
.del-btn:hover { color: var(--red); text-decoration: line-through; }
.add-link-btn { width: 100%; padding: 10px; border: 2px dashed #999; background: transparent; color: #666; cursor: pointer; font-family: var(--mono); transition: 0.2s; }
.add-link-btn:hover { border-color: var(--black); color: var(--black); background: #eee; }

/* Buttons */
.cyber-btn { background: var(--black); color: var(--white); border: none; padding: 10px 20px; font-weight: bold; cursor: pointer; transition: 0.2s; font-family: var(--mono); }
.cyber-btn:hover { background: var(--red); }
.cyber-btn.sm { padding: 5px 10px; font-size: 0.75rem; }

/* Footer */
.form-footer { text-align: center; margin-top: 50px; padding-bottom: 50px; color: #999; font-family: var(--mono); }

/* Animation */
@keyframes scan { 0% { top: 0; } 100% { top: 100%; } }
.blink { animation: blink 1s infinite; }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* Scrollbar */
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }

/* Mobile */
@media (max-width: 900px) {
  .main-layout { flex-direction: column; overflow-y: auto; }
  .sidebar-nav { width: 100%; height: auto; border-right: none; border-bottom: 2px solid var(--black); flex-shrink: 0; }
  .console-box { display: none; }
  .form-grid, .form-grid.three-col { grid-template-columns: 1fr; }
  .avatar-row { flex-direction: column; align-items: flex-start; }
}
</style>