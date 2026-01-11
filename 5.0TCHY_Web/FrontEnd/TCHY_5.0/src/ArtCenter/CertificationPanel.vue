<template>
  <div class="certification-panel tc-card md-elevation-1">
    <div class="grid-bg moving-grid"></div>

    <div class="panel-scroll-container custom-scroll">
      <header class="main-header">
        <div class="header-split left">
          <h1 class="giant-text glitch-hover" data-text="FRAME">
            <div class="text-row">TCHY</div>
            <div class="text-row outline">画师</div>
            <div class="text-row red-fill">认证</div>
          </h1>
        </div>
        <div class="header-split right">
          <div class="info-block">
            <h2 class="cn-title">创作认证核心 // CERTIFICATION_CORE</h2>
            <div class="live-indicator"><span class="dot"></span> SYSTEM ONLINE</div>
            <p class="desc">
              为顶尖创作者打造的评级体系。<br>
              <span class="highlight">STATIC</span> 凝固瞬间 / <span class="highlight red">MOTION</span> 注入灵魂
            </p>
            
            <button class="apply-btn md-ripple" @click="openApplyModal">
              <span class="btn-inner"><span class="icon">⚡</span> ACCESS SYSTEM / 申请考核</span>
              <div class="btn-glitch"></div>
            </button>
          </div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">// SYSTEM_READY // DATA_STREAM_OPEN // AUDIT_QUEUE_ACTIVE // UPLOAD_LINK_ESTABLISHED //</div>
      </div>

      <main class="core-container">
        
        <nav class="category-tabs md-elevation-2">
          <div class="sliding-bg" :style="sliderStyle"></div>
          <div class="tab-item" :class="{ active: currentCategory === 'static' }" @click="switchCategory('static')"><span class="tab-label">STATIC / 静态</span></div>
          <div class="tab-item" :class="{ active: currentCategory === 'animation' }" @click="switchCategory('animation')"><span class="tab-label">ANIMATION / 动画</span></div>
        </nav>

        <div class="rules-wrapper">
          <Transition name="glitch-fade" mode="out-in">
            <section class="rules-panel md-elevation-2" :key="currentCategory">
              <div class="panel-label-strip"><span>// PROTOCOL_{{ currentCategory.toUpperCase() }} // <span class="blink">_</span></span></div>
              <div class="rules-grid">
                <div class="rule-box requirements">
                  <h3 class="rule-header"><span class="icon">TARGET</span> 考核要求</h3>
                  <ul class="spec-list">
                    <li v-for="(req, index) in currentRules.requirements" :key="index" :style="{ '--i': index }"><span class="index">0{{index + 1}}</span>{{ req }}</li>
                  </ul>
                </div>
                <div class="rule-box methods">
                  <h3 class="rule-header"><span class="icon">METHOD</span> 考核方式</h3>
                  <div class="method-content">
                    <p v-for="(method, index) in currentRules.methods" :key="index"><span class="bullet">■</span> {{ method }}</p>
                  </div>
                </div>
              </div>
            </section>
          </Transition>
        </div>

        <section class="roster-section">
          <div class="section-head">
            <div class="title-group">
              <div class="title-block"><span class="panel-icon">■</span> 名录 / ROSTER</div>
              <div class="year-stepper md-elevation-1">
                <button class="step-btn" @click="changeYear(-1)" :disabled="currentYear <= minYear">&lt;</button>
                <span class="year-display"><span class="prefix">SEASON_</span>{{ currentYear }}</span>
                <button class="step-btn" @click="changeYear(1)" :disabled="currentYear >= maxYear">&gt;</button>
              </div>
            </div>
            <div class="mini-filters custom-scroll">
              <span v-for="lv in levels" :key="lv.id" class="mini-btn" :class="{ active: currentLevel === lv.id }" @click="currentLevel = lv.id">{{ lv.name }}</span>
            </div>
          </div>

          <div class="artist-grid">
            <div v-if="isLoading" class="status-box loading">[ SYSTEM_SYNCING... ]</div>
            <div v-else-if="filteredArtists.length === 0" class="status-box empty">// NO_DATA_FOUND // 暂无数据</div>
            <TransitionGroup v-else name="card-pop">
              <div class="artist-card md-elevation-2" v-for="artist in filteredArtists" :key="artist.id" :class="artist.type">
                <div v-if="artist.type === 'animation'" class="film-strip-holes"></div>
                <div class="level-strip" :class="`lv-${artist.level}`">LV.{{ artist.level }}</div>
                <div class="card-content">
                  <div class="avatar-frame">
                    <img v-if="artist.avatar" :src="fixAvatarUrl(artist.avatar)" class="avatar-img-real" alt="avatar" @error="handleImgError"/>
                    <div v-else class="avatar-img" :style="{ background: artist.color }">{{ artist.name.charAt(0) }}</div>
                    <div v-if="artist.type === 'animation'" class="play-overlay">▶</div>
                  </div>
                  <div class="info-group">
                    <div class="name">{{ artist.name }}</div>
                    <div class="sub-type">{{ artist.type === 'static' ? 'ILLUSTRATOR' : 'KEY ANIMATOR' }}</div>
                    <div class="tags"><span class="tag" v-for="tag in artist.tags" :key="tag">#{{ tag }}</span></div>
                  </div>
                </div>
                <div class="scan-line"></div>
              </div>
            </TransitionGroup>
          </div>
        </section>

        <section class="audit-section">
          <div class="audit-header">
            <span class="blinking-cursor">█</span> INCOMING_DATA_STREAM // 实时审核队列
          </div>
          <div class="terminal-window custom-scroll">
            <div class="terminal-row header">
              <span class="col id">ID</span>
              <span class="col time">TIME</span>
              <span class="col type">TYPE</span>
              <span class="col link">SOURCE_LINK</span>
              <span class="col status">STATUS</span>
            </div>
            
            <div v-if="pendingList.length === 0" class="terminal-row empty">
              >> NO PENDING REQUESTS... SYSTEM IDLE
            </div>
            <div v-else v-for="item in pendingList" :key="item.id" class="terminal-row data-row">
              <span class="col id">#{{ item.id }}</span>
              <span class="col time">{{ formatDate(item.submitTime) }}</span>
              <span class="col type" :class="item.category.toLowerCase()">{{ item.category }}</span>
              <span class="col link">
                <a :href="item.portfolioUrl" target="_blank" class="link-text">
                  {{ item.portfolioUrl }}
                </a>
              </span>
              <span class="col status blinking">AWAITING_ASSESSMENT</span>
            </div>
          </div>
        </section>

      </main>
    </div>

    <Teleport to="body">
      <Transition name="modal-scale">
        <div v-if="isModalOpen" class="modal-overlay" @click.self="isModalOpen = false">
          <div class="modal-window md-elevation-5">
            <div class="modal-header">
              <h3 class="modal-title">// INITIATE_PROTOCOL</h3>
              <button class="close-btn" @click="isModalOpen = false">×</button>
            </div>
            <div class="modal-body">
              <div class="info-row">
                <span class="label">SEASON:</span> <span class="val">{{ currentYear }}</span>
                <span class="sep">|</span>
                <span class="label">TYPE:</span> <span class="val highlight">{{ currentCategory.toUpperCase() }}</span>
              </div>
              <form @submit.prevent="submitApplication" class="apply-form">
                <div class="form-info-text">
                  * 评级将由审核员根据作品质量在 <strong>Lv.1 - Lv.7</strong> 之间评定。
                </div>

                <div class="form-group">
                  <label>PORTFOLIO URL / 作品集链接</label>
                  <input type="url" v-model="applyForm.portfolioUrl" placeholder="https://..." required class="cyber-input">
                </div>
                <div class="form-group">
                  <label>DESCRIPTION / 附言</label>
                  <textarea v-model="applyForm.description" placeholder="说明..." class="cyber-input textarea"></textarea>
                </div>
                <div class="form-actions">
                  <div v-if="submitStatus.msg" class="status-msg" :class="submitStatus.type">{{ submitStatus.msg }}</div>
                  <button type="submit" class="confirm-btn md-ripple" :disabled="isSubmitting">{{ isSubmitting ? 'TRANSMITTING...' : 'CONFIRM UPLOAD' }}</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
// Script 部分保持完全一致，无需修改
import { ref, computed, onMounted, reactive } from 'vue'
import apiClient from '@/utils/api'

// --- 工具函数: 头像URL修复 ---
const BASE_URL = 'https://bianyuzhou.com';
const fixAvatarUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  if (!path.startsWith('uploads/')) path = `uploads/${path}`;
  return `${BASE_URL}/${path}`;
};
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const currentCategory = ref('static')
const currentLevel = ref(0)
const isLoading = ref(false)
const artists = ref([])
const pendingList = ref([])
const maxYear = new Date().getFullYear()
const minYear = 2024
const currentYear = ref(maxYear)

const isModalOpen = ref(false)
const isSubmitting = ref(false)
const submitStatus = reactive({ type: '', msg: '' })
const applyForm = reactive({ portfolioUrl: '', description: '' })

// Lv.1 - Lv.7
const levels = [
  { id: 0, name: 'ALL' }, 
  { id: 1, name: 'LV.1' }, { id: 2, name: 'LV.2' }, { id: 3, name: 'LV.3' }, 
  { id: 4, name: 'LV.4' }, { id: 5, name: 'LV.5' }, { id: 6, name: 'LV.6' }, { id: 7, name: 'LV.7' }
]

const rulesData = {
  static: { requirements: ['需要提供作画过程', '提交视频链接【包含绘画过程】', '内容不限'], methods: ['提交视频链接，什么平台都可以只要能正常观看'] },
  animation: { requirements: ['时常不低于10s', '分镜或者平面或者特效不限制,3D背景或者人物都可以', '不接受剪辑视频'], methods: ['提交视频时常不低于10s的视频链接'] }
}

const currentRules = computed(() => rulesData[currentCategory.value])
const sliderStyle = computed(() => currentCategory.value === 'static' ? { transform: 'translateX(0%)', background: '#111' } : { transform: 'translateX(100%)', background: '#D92323' })
const filteredArtists = computed(() => {
  let list = artists.value || []
  if (currentLevel.value !== 0) list = list.filter(a => a.level === currentLevel.value)
  return list
})

// --- API Calls ---
const fetchRoster = async () => {
  isLoading.value = true
  artists.value = []
  try {
    const categoryInt = currentCategory.value === 'static' ? 0 : 1
    const res = await apiClient.get('/ChaiArtist/roster', { 
      params: { 
        year: currentYear.value, 
        category: categoryInt 
      } 
    })

    if (res.data) {
      artists.value = res.data.map(item => ({
        id: item.UserId,
        name: item.UserName,
        level: item.Level,
        color: item.HexColor,
        avatar: item.Avatar,
        tags: (item.Tags || []).filter(t => t !== '[]' && t !== '' && t !== '""'),
        type: currentCategory.value
      }))
    }
  } catch (error) { 
    console.error("Fetch Roster Error:", error) 
  } finally { 
    isLoading.value = false 
  }
}

const fetchPending = async () => {
  try {
    const res = await apiClient.get('/ChaiArtist/pending')
    if (res.data) {
      pendingList.value = res.data.map(item => ({
        id: item.Id,
        submitTime: item.SubmitTime,
        category: item.Category,
        portfolioUrl: item.PortfolioUrl
      }))
    }
  } catch (error) { console.error("Fetch Pending Error:", error) }
}

const openApplyModal = () => {
  applyForm.portfolioUrl = ''; applyForm.description = ''; submitStatus.msg = '';
  isModalOpen.value = true
}

const submitApplication = async () => {
  if (isSubmitting.value) return
  isSubmitting.value = true; submitStatus.msg = ''
  try {
    const payload = {
      applyYear: currentYear.value,
      category: currentCategory.value === 'static' ? 0 : 1,
      portfolioUrl: applyForm.portfolioUrl,
      description: applyForm.description
    }
    await apiClient.post('/ChaiArtist/apply', payload)
    submitStatus.type = 'success'; submitStatus.msg = 'UPLOAD COMPLETE. AWAITING ASSESSMENT.'
    fetchPending() 
    setTimeout(() => { isModalOpen.value = false }, 1500)
  } catch (error) {
    submitStatus.type = 'error'; submitStatus.msg = error.response?.data || 'UPLOAD FAILED.'
  } finally { isSubmitting.value = false }
}

const switchCategory = (cat) => { if (currentCategory.value === cat) return; currentCategory.value = cat; currentLevel.value = 0; fetchRoster() }
const changeYear = (step) => { const newYear = currentYear.value + step; if (newYear < minYear || newYear > maxYear) return; currentYear.value = newYear; fetchRoster() }
const formatDate = (dateStr) => {
  if (!dateStr) return '--/--';
  const d = new Date(dateStr)
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours()}:${String(d.getMinutes()).padStart(2, '0')}`
}

onMounted(() => { fetchRoster(); fetchPending() })
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;900&family=JetBrains+Mono:wght@400;700&display=swap');

.certification-panel {
  --red: #D92323; 
  --black: #111111; 
  --off-white: #F4F1EA; 
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  font-family: 'Inter', sans-serif;
  
  /* MD Elevation Colors */
  --shadow-color: rgba(0, 0, 0, 0.2);
  
  position: relative;
  background-color: var(--off-white); 
  color: var(--black); 
  border-radius: 12px; 
  overflow: hidden; 
  
  /* [关键修改] 100% 高度 + flex 布局 */
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* [新增] 滚动容器 */
.panel-scroll-container {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  position: relative;
  z-index: 1;
}

/* MD Elevations */
.md-elevation-1 { box-shadow: 0 2px 1px -1px rgba(0,0,0,0.2), 0 1px 1px 0 rgba(0,0,0,0.14), 0 1px 3px 0 rgba(0,0,0,0.12); }
.md-elevation-2 { box-shadow: 0 3px 1px -2px rgba(0,0,0,0.2), 0 2px 2px 0 rgba(0,0,0,0.14), 0 1px 5px 0 rgba(0,0,0,0.12); }
.md-elevation-5 { box-shadow: 0 24px 38px 3px rgba(0,0,0,0.14), 0 9px 46px 8px rgba(0,0,0,0.12), 0 11px 15px -7px rgba(0,0,0,0.2); }

/* --- 核心布局 --- */
.moving-grid { position: absolute; inset: 0; background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); background-size: 50px 50px; opacity: 0.4; z-index: 0; animation: gridScroll 20s linear infinite; pointer-events: none; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

.main-header { display: flex; flex-wrap: wrap; border-bottom: 4px solid var(--black); position: relative; z-index: 1; background: var(--off-white); }
.header-split { padding: 40px; flex: 1; min-width: 300px; }
.header-split.left { background: var(--black); color: var(--off-white); display: flex; justify-content: center; overflow: hidden; align-items: center; }
.header-split.right { display: flex; align-items: center; }

/* 文本样式适配 */
.giant-text { font-family: 'Anton'; font-size: clamp(3rem, 6vw, 6rem); line-height: 0.85; text-transform: uppercase; transform: rotate(-2deg); }
.text-row.outline { color: rgb(24, 23, 23); -webkit-text-stroke: 2px var(--off-white); }
.text-row.red-fill { color: var(--red); margin-left: 20px; }
.info-block { width: 100%; max-width: 500px; }
.cn-title { font-weight: 900; letter-spacing: -1px; margin-bottom: 5px; font-size: 1.5rem; }

.live-indicator { display: inline-flex; align-items: center; gap: 8px; font-family: var(--mono); font-size: 0.8rem; color: var(--red); border: 1px solid var(--red); padding: 4px 8px; margin-bottom: 15px; background: rgba(217, 35, 35, 0.05); }
.dot { width: 8px; height: 8px; background: var(--red); border-radius: 50%; animation: pulse 1s infinite; }
.desc { margin-bottom: 25px; font-weight: 600; line-height: 1.6; font-size: 0.95rem; color: #444; }
.highlight { background: var(--black); color: #fff; padding: 0 4px; border-radius: 2px; }
.highlight.red { background: var(--red); }

/* Apply Button */
.apply-btn { position: relative; background: transparent; border: none; cursor: pointer; padding: 0; }
.btn-inner { display: flex; align-items: center; gap: 10px; background: var(--red); color: #fff; padding: 12px 24px; font-weight: 700; border-radius: 6px; position: relative; z-index: 2; transition: transform 0.1s, box-shadow 0.2s; box-shadow: 0 4px 6px rgba(0,0,0,0.2); }
.btn-glitch { position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: var(--black); z-index: 1; transition: 0.2s; border-radius: 6px; opacity: 0; transform: translate(0,0); }
.apply-btn:hover .btn-inner { transform: translate(-2px, -2px); box-shadow: 2px 6px 10px rgba(0,0,0,0.3); }
.apply-btn:hover .btn-glitch { opacity: 1; transform: translate(4px, 4px); }

/* Tech Strip */
.tech-strip { background: var(--black); color: var(--off-white); padding: 8px 0; border-bottom: 4px solid var(--black); overflow: hidden; white-space: nowrap; font-family: var(--mono); font-size: 0.8rem; }
.strip-content { display: inline-block; animation: marquee 20s linear infinite; }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }

/* Main Content */
.core-container { max-width: 1100px; margin: 0 auto; padding: 40px 20px; position: relative; z-index: 2; }

/* Tabs */
.category-tabs { display: flex; position: relative; background: #fff; margin-bottom: 30px; height: 60px; border-radius: 8px; overflow: hidden; border: 2px solid var(--black); }
.sliding-bg { position: absolute; top: 0; left: 0; width: 50%; height: 100%; transition: all 0.4s cubic-bezier(0.25, 1, 0.5, 1); z-index: 0; }
.tab-item { flex: 1; display: flex; align-items: center; justify-content: center; cursor: pointer; z-index: 1; font-family: 'Anton'; font-size: 1.4rem; letter-spacing: 1px; transition: color 0.3s; }
.tab-item.active { color: #fff; }

/* Rules */
.rules-wrapper { min-height: 350px; margin-bottom: 50px; }
.rules-panel { background: #fff; border-radius: 8px; overflow: hidden; border: 1px solid #ddd; }
.panel-label-strip { background: var(--black); color: var(--off-white); padding: 8px 20px; font-family: var(--mono); font-size: 0.8rem; }
.blink { animation: pulse 0.5s step-end infinite; }
.rules-grid { display: grid; grid-template-columns: 1.2fr 1fr; }
@media (max-width: 768px) { .rules-grid { grid-template-columns: 1fr; } }
.rule-box { padding: 30px; }
.rule-box.requirements { border-right: 1px solid #eee; }
@media (max-width: 768px) { .rule-box.requirements { border-right: none; border-bottom: 1px solid #eee; } }
.rule-header { font-family: 'Anton'; font-size: 1.2rem; margin-bottom: 20px; display: flex; align-items: center; gap: 8px; }
.icon { background: var(--black); color: #fff; padding: 2px 6px; font-size: 0.7rem; font-family: var(--mono); border-radius: 2px; }
.spec-list li { margin-bottom: 15px; font-weight: 600; display: flex; gap: 10px; animation: slideInRight 0.5s ease forwards; animation-delay: calc(var(--i) * 0.1s); opacity: 0; transform: translateX(-10px); color: #333; }
@keyframes slideInRight { to { opacity: 1; transform: translateX(0); } }
.spec-list .index { font-family: 'Anton'; color: var(--red); }
.method-content p { margin-bottom: 10px; font-weight: 600; color: #444; }
.bullet { color: var(--red); }

/* Roster */
.section-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 15px; }
.title-group { display: flex; align-items: center; gap: 15px; }
.title-block { font-weight: 900; font-family: var(--mono); font-size: 1.2rem; }
.year-stepper { display: flex; align-items: center; background: var(--black); color: var(--off-white); padding: 4px; border-radius: 4px; font-family: var(--mono); }
.year-display { margin: 0 15px; font-weight: 700; font-size: 1rem; }
.prefix { color: #888; font-size: 0.8rem; margin-right: 4px; }
.step-btn { background: #333; color: var(--off-white); border: none; width: 24px; height: 24px; cursor: pointer; font-weight: 900; display: flex; align-items: center; justify-content: center; transition: all 0.1s; border-radius: 2px; }
.step-btn:hover:not(:disabled) { background: var(--red); }
.step-btn:disabled { opacity: 0.3; cursor: not-allowed; }

.mini-filters { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 5px; }
.mini-btn { font-size: 0.8rem; font-weight: 700; cursor: pointer; padding: 4px 10px; border-radius: 20px; background: #eee; color: #555; transition: 0.2s; white-space: nowrap; }
.mini-btn:hover { background: #ddd; }
.mini-btn.active { background: var(--black); color: #fff; box-shadow: 0 2px 4px rgba(0,0,0,0.2); }

.artist-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 15px; min-height: 200px; }
.status-box { grid-column: 1 / -1; display: flex; align-items: center; justify-content: center; padding: 60px; font-family: var(--mono); color: #888; border: 2px dashed #ccc; background: rgba(255,255,255,0.5); border-radius: 8px; }

/* Artist Card */
.artist-card { position: relative; display: flex; height: 90px; background: #fff; border-radius: 8px; transition: transform 0.2s; overflow: hidden; border: 1px solid #e0e0e0; }
.artist-card:hover { transform: translateY(-3px); }
.artist-card.animation { border-left: 4px solid var(--red); } 
.artist-card.static { border-left: 4px solid var(--black); }

.film-strip-holes { position: absolute; top: 0; bottom: 0; left: 0; width: 12px; background-image: radial-gradient(circle, var(--black) 2px, transparent 2.5px); background-size: 100% 10px; background-color: var(--red); z-index: 2; opacity: 0.8; }

.level-strip { width: 24px; writing-mode: vertical-rl; transform: rotate(180deg); display: flex; align-items: center; justify-content: center; font-weight: 900; font-size: 0.7rem; margin-left: 0; color: #fff; }
.artist-card.animation .level-strip { margin-left: 12px; }
.lv-7 { background: linear-gradient(to top, #FFD700, #DAA520); color: #000; }
.lv-6 { background: #800080; }
.lv-5 { background: #D92323; }
.lv-4 { background: var(--black); }
.lv-3 { background: #555; }
.lv-1, .lv-2 { background: #999; }

.card-content { flex: 1; padding: 10px; display: flex; align-items: center; gap: 12px; position: relative; z-index: 1; }
.avatar-frame { width: 45px; height: 45px; border-radius: 50%; overflow: hidden; border: 2px solid #eee; position: relative; }
.avatar-img { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #fff; font-family: 'Anton'; }
.avatar-img-real { width: 100%; height: 100%; object-fit: cover; }
.play-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.5); color: #fff; display: flex; align-items: center; justify-content: center; opacity: 0; transition: opacity 0.2s; font-size: 0.8rem; }
.artist-card:hover .play-overlay { opacity: 1; }
.name { font-weight: 800; font-size: 0.95rem; text-transform: uppercase; margin-bottom: 2px; color: #333; }
.sub-type { font-size: 0.6rem; font-family: var(--mono); color: #888; margin-bottom: 4px; }
.tags { display: flex; gap: 4px; flex-wrap: wrap; }
.tag { font-size: 0.6rem; background: #f0f0f0; padding: 2px 5px; border-radius: 3px; color: #555; }
.scan-line { position: absolute; top: 0; left: 0; width: 2px; height: 100%; background: rgba(255, 255, 255, 0.8); box-shadow: 0 0 5px rgba(255,255,255,1); opacity: 0; pointer-events: none; z-index: 10; }
.artist-card:hover .scan-line { opacity: 1; animation: scan 1s linear infinite; }

/* Audit Terminal */
.audit-section { margin-top: 50px; border-top: 2px solid #eee; padding-top: 30px; margin-bottom: 20px; }
.audit-header { font-family: var(--mono); font-weight: 700; font-size: 1rem; margin-bottom: 15px; color: #555; display: flex; align-items: center; gap: 10px; }
.blinking-cursor { color: var(--red); animation: blink 1s step-end infinite; }
.terminal-window { background: var(--black); color: var(--off-white); font-family: var(--mono); padding: 15px; border-radius: 8px; box-shadow: inset 0 0 20px rgba(0,0,0,0.5); overflow-x: auto; max-height: 300px; }
.terminal-row { display: grid; grid-template-columns: 0.5fr 1fr 0.8fr 2fr 0.8fr; padding: 8px 0; border-bottom: 1px dashed #333; font-size: 0.8rem; align-items: center; }
.terminal-row.header { color: #888; font-weight: 700; border-bottom: 1px solid #555; padding-bottom: 10px; margin-bottom: 5px; }
.terminal-row.data-row:hover { background: rgba(255,255,255,0.05); }
.col.status { color: var(--red); font-weight: 700; }
.col.link { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.link-text { color: #888; text-decoration: none; border-bottom: 1px solid #555; }
.link-text:hover { color: var(--off-white); border-color: var(--red); }

/* --- Modal (MD Style) --- */
.modal-overlay { 
  position: fixed; inset: 0; z-index: 9999; 
  background: rgba(0, 0, 0, 0.5); 
  backdrop-filter: blur(4px); 
  display: flex; align-items: center; justify-content: center; 
}
.modal-window { 
  background: #fff; 
  width: 90%; max-width: 500px; 
  border-radius: 12px; 
  overflow: hidden; 
  display: flex; flex-direction: column;
}
.modal-header { background: var(--black); color: var(--off-white); padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; }
.modal-title { margin: 0; font-family: var(--mono); letter-spacing: 1px; font-size: 1rem; }
.close-btn { background: none; border: none; color: var(--off-white); font-size: 1.5rem; cursor: pointer; transition: 0.2s; }
.close-btn:hover { color: var(--red); }
.modal-body { padding: 30px; }
.info-row { margin-bottom: 20px; font-family: var(--mono); font-size: 0.9rem; padding-bottom: 10px; border-bottom: 1px dashed #ccc; }
.info-row .highlight { color: var(--red); font-weight: bold; }

.form-group { margin-bottom: 20px; }
.form-group label { display: block; font-weight: 700; margin-bottom: 6px; font-size: 0.85rem; color: #333; }
.cyber-input { width: 100%; padding: 12px; border: 2px solid #ddd; border-radius: 6px; background: #f9f9f9; font-family: var(--mono); font-size: 0.9rem; outline: none; transition: 0.2s; }
.cyber-input:focus { border-color: var(--black); background: #fff; }
.textarea { height: 100px; resize: vertical; }

.confirm-btn { width: 100%; padding: 14px; background: var(--red); color: white; border: none; font-weight: 700; font-size: 1rem; cursor: pointer; border-radius: 6px; letter-spacing: 1px; transition: 0.2s; }
.confirm-btn:hover { background: #b91d1d; transform: translateY(-1px); }
.confirm-btn:disabled { background: #ccc; cursor: not-allowed; }

.status-msg { margin-bottom: 10px; font-family: var(--mono); font-size: 0.8rem; text-align: center; }
.status-msg.error { color: var(--red); }
.status-msg.success { color: green; }

/* Transitions */
.modal-scale-enter-active, .modal-scale-leave-active { transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1); }
.modal-scale-enter-from, .modal-scale-leave-to { opacity: 0; transform: scale(0.95); }
.card-pop-enter-active { animation: popIn 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
@keyframes popIn { 0% { opacity: 0; transform: scale(0.9); } 100% { opacity: 1; transform: scale(1); } }

/* Scrollbar */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>