<template>
  <main class="content-area-inside">
    <nav class="tab-controller">
      <button 
        v-for="tab in tabs" 
        :key="tab.id"
        class="tab-btn"
        :class="{ active: currentTab === tab.id }"
        @click="switchTab(tab.id)"
      >
        <span class="tab-idx">0{{ tab.index }}</span>
        <div class="label-group">
          <span class="tab-label">{{ tab.label }}</span>
          <span class="tab-sub">{{ tab.sub }}</span>
        </div>
        <div class="active-line" v-if="currentTab === tab.id"></div>
      </button>
      <div class="tab-filler">
        <div class="deco-stripes"></div>
      </div>
    </nav>

    <div class="viewport custom-scroll" ref="viewportRef">
      
      <Transition name="cyber-slide" mode="out-in">
        
        <div v-if="currentTab === 'works'" class="view-section view-works" key="works">
          <div class="section-toolbar">
            <span class="label">FILTER_MODE //</span>
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
                <img :src="work.cover" loading="lazy" alt="work cover" />
                <div class="crt-overlay"></div>
                <div class="overlay-text">
                  <span class="blink">ACCESSING...</span>
                </div>
              </div>
              <div class="work-meta">
                <div class="w-title">{{ work.title }}</div>
                <div class="w-stats">
                  <span>♥ {{ work.likes }}</span>
                  <span class="tag-box">{{ work.category === 'ui' ? 'UI' : 'ART' }}</span>
                </div>
              </div>
              <div class="corner-deco"></div>
            </div>
          </div>

          <div class="guestbook-container">
            <div class="divider-text">/// PUBLIC_GUEST_CHANNEL ///</div>
            <GuestbookInput />
          </div>
        </div>

        <PostSection v-else-if="currentTab === 'posts'" key="posts" />

        <div v-else-if="currentTab === 'blogs'" class="view-section view-blogs" key="blogs">
          <div class="blog-timeline">
            <div class="timeline-line"></div>
            <div v-for="blog in blogs" :key="blog.id" class="blog-entry">
              <div class="date-col">
                <span class="day">{{ blog.day }}</span>
                <span class="month">{{ blog.month }}</span>
              </div>
              <div class="content-col">
                <div class="blog-card" @click="goToBlogDetail(blog.id)">
                  <h3 class="b-title">
                    <span class="prefix">>></span> {{ blog.title }}
                  </h3>
                  <p class="b-excerpt">{{ blog.excerpt }}</p>
                  <div class="b-footer">
                    <div class="tags">
                      <span v-for="t in blog.tags" :key="t" class="cyber-tag">#{{ t }}</span>
                    </div>
                    <button class="read-btn">READ_LOG [Enter]</button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="currentTab === 'activity'" class="view-section view-activity" key="activity">
          <div class="terminal-log">
            <div class="scan-bar"></div>
            <div class="log-header">
              <span>TIMESTAMP</span>
              <span>OP_CODE</span>
              <span>PAYLOAD_DETAIL</span>
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

      </Transition>

    </div>
  </main>
</template>

<script setup>
import { ref, computed } from 'vue'
import GuestbookInput from './GuestbookInput.vue'
import PostSection from './PostSection.vue' // 新增引入

// --- State Management ---
const currentTab = ref('works')
const viewportRef = ref(null)
const activeWorkFilter = ref('all')

// --- Data Constants ---
const tabs = [
  { id: 'works', index: 1, label: '作品库', sub: 'ARTWORKS_DB' },
  { id: 'posts', index: 2, label: '动态', sub: 'BROADCAST' }, // 新增 Tab
  { id: 'blogs', index: 3, label: '思维库', sub: 'THOUGHT_LOGS' },
  { id: 'activity', index: 4, label: '系统志', sub: 'SYSTEM_ACT' }
]

const workFilters = [
  { key: 'all', label: 'ALL // 全部' },
  { key: 'GIF', label: 'MOV // 动态' },
  { key: '图片', label: 'IMG // 静态' }
]

// Mock Data: Works
const works = ref(Array.from({ length: 8 }).map((_, i) => ({
  id: i,
  title: `PROJECT_NEON_0${i+1}`,
  likes: 100 + i * 15,
  date: '2023.10.12',
  cover: `https://picsum.photos/400/300?random=${i}`,
  category: i % 2 === 0 ? 'ui' : 'art'
})))

// Mock Data: Blogs
const blogs = ref([
  { id: 1, day: '15', month: 'OCT', title: '低保真界面的美学复兴', excerpt: '在AI生成平滑图像的时代，回归粗野主义设计风格的必要性...', tags: ['DESIGN', 'RANT'] },
  { id: 2, day: '02', month: 'SEP', title: 'Vue 3 渲染性能深度优化', excerpt: '大型工业级仪表盘页面渲染开销的技术报告...', tags: ['CODE', 'VUE'] },
  { id: 3, day: '28', month: 'AUG', title: '赛博朋克色彩理论研究', excerpt: '高对比度色彩方案在黑暗模式下的视觉引导作用。', tags: ['ART', 'COLOR'] }
])

// Mock Data: Activities
const activities = ref([
  { time: '14:30:05', action: 'UPLOAD', type: 'info', detail: 'New Asset "Neon_Gen" uploaded.' },
  { time: '09:15:22', action: 'COMMENT', type: 'success', detail: 'Reply sent to user @Ghost.' },
  { time: '18:45:00', action: 'AUTH', type: 'warn', detail: 'System access from node HK_02.' },
  { time: '11:20:10', action: 'LIKE', type: 'success', detail: 'Liked "Mech_Design_v2".' },
  { time: '22:00:55', action: 'ERROR', type: 'error', detail: 'Connection timeout. Retrying...' }
])

// --- Computed Properties ---
const filteredWorks = computed(() => {
  if (activeWorkFilter.value === 'all') return works.value
  return works.value.filter(w => w.category === activeWorkFilter.value)
})

// --- Methods ---
const switchTab = (tabId) => {
  currentTab.value = tabId
  if (viewportRef.value) viewportRef.value.scrollTop = 0
}

const goToWorkDetail = (id) => {
  // console.log(`Opening Work ID: ${id}`)
}

const goToBlogDetail = (id) => {
  // console.log(`Opening Blog ID: ${id}`)
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

.content-area-inside {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray-ui: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --sans: 'Noto Sans SC', sans-serif;
  
  flex: 1;
  display: flex; flex-direction: column;
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.15); /* 加深阴影 */
  overflow: hidden;
  height: 100%;
  font-family: var(--mono), var(--sans);
  position: relative;
}

/* --- Tab Controller Enhanced --- */
.tab-controller { 
  display: flex; 
  background: #dcdcdc; 
  border-bottom: 2px solid var(--black); 
  height: 60px;
}
.tab-btn {
  flex: 1; max-width: 180px;
  background: transparent; border: none; border-right: 1px solid #999;
  padding: 0 15px; text-align: left; cursor: pointer; position: relative;
  transition: all 0.2s cubic-bezier(0.25, 1, 0.5, 1);
  display: flex; align-items: center; gap: 10px;
  overflow: hidden;
}
.tab-btn:hover { background: #cfcfcf; }
.tab-btn.active { 
  background: var(--white); 
  color: var(--black); 
  border-bottom: none;
  box-shadow: 0 4px 0 var(--white); /* 遮挡下边框 */
  z-index: 2;
}
.tab-idx { font-size: 1.2rem; color: #aaa; font-weight: bold; font-family: var(--heading); opacity: 0.3; }
.tab-btn.active .tab-idx { color: var(--red); opacity: 1; }
.label-group { display: flex; flex-direction: column; }
.tab-label { font-weight: 800; font-size: 0.9rem; letter-spacing: 1px; }
.tab-sub { font-size: 0.55rem; color: #666; font-family: var(--mono); }

.active-line { 
  position: absolute; top: 0; left: 0; width: 100%; height: 3px; 
  background: var(--red); 
  box-shadow: 0 0 10px var(--red);
}

.tab-filler { 
  flex: 1; background: #dcdcdc; 
  display: flex; align-items: center; padding-left: 10px; opacity: 0.2;
}
.deco-stripes {
  height: 10px; width: 100%;
  background: repeating-linear-gradient(45deg, #000, #000 2px, transparent 2px, transparent 6px);
}

/* --- Viewport & Common --- */
.viewport { 
  flex: 1; overflow-y: auto; padding: 25px; 
  position: relative; 
  background-image: radial-gradient(#ccc 1px, transparent 1px);
  background-size: 20px 20px;
}

/* Toolbar */
.section-toolbar { 
  display: flex; align-items: center; gap: 15px; 
  margin-bottom: 20px; font-size: 0.8rem; 
  border-bottom: 2px solid var(--black); padding-bottom: 10px; 
  font-family: var(--mono); 
}
.chip { 
  border: 1px solid var(--black); padding: 4px 12px; cursor: pointer; 
  transition: 0.2s; font-size: 0.7rem; background: #fff; 
}
.chip:hover, .chip.active { 
  background: var(--black); color: var(--white); 
  box-shadow: 3px 3px 0 var(--red);
  transform: translate(-1px, -1px);
}

/* --- Works Grid Enhanced --- */
.works-grid { 
  display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); 
  gap: 20px; margin-bottom: 30px; 
}
.work-item { 
  border: 2px solid var(--black); 
  background: #000; 
  position: relative; 
  cursor: pointer;
  transition: transform 0.2s;
  height: 180px; display: flex; flex-direction: column;
}
.work-item:hover { 
  transform: translateY(-5px); 
  box-shadow: 8px 8px 0 var(--black);
  border-color: var(--red);
}
.img-wrapper { 
  flex: 1; overflow: hidden; position: relative; border-bottom: 2px solid var(--black); 
}
.img-wrapper img { 
  width: 100%; height: 100%; object-fit: cover; 
  filter: grayscale(80%) contrast(1.2); 
  transition: 0.3s; 
}
.work-item:hover img { 
  filter: grayscale(0) contrast(1.1); 
  transform: scale(1.1); 
}
/* CRT 扫描线叠加 */
.crt-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.25) 50%), linear-gradient(90deg, rgba(255, 0, 0, 0.06), rgba(0, 255, 0, 0.02), rgba(0, 0, 255, 0.06));
  background-size: 100% 2px, 3px 100%;
  pointer-events: none; z-index: 1;
}
.overlay-text { 
  position: absolute; inset: 0; background: rgba(0,0,0,0.8); 
  display: flex; align-items: center; justify-content: center; 
  color: var(--red); font-family: var(--mono); font-weight: bold; font-size: 0.8rem;
  opacity: 0; transition: 0.2s; z-index: 2;
}
.work-item:hover .overlay-text { opacity: 1; }

.work-meta { 
  padding: 8px; background: #fff; height: 50px; 
  display: flex; flex-direction: column; justify-content: space-between;
}
.w-title { 
  font-weight: 800; font-size: 0.85rem; 
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis; 
}
.w-stats { display: flex; justify-content: space-between; align-items: center; font-size: 0.7rem; color: #666; font-family: var(--mono); }
.tag-box { border: 1px solid #ccc; padding: 0 4px; }

/* Corner Deco */
.corner-deco {
  position: absolute; bottom: -2px; right: -2px; width: 10px; height: 10px;
  background: var(--red); clip-path: polygon(100% 0, 0 100%, 100% 100%);
  opacity: 0; transition: 0.2s;
}
.work-item:hover .corner-deco { opacity: 1; }

/* --- Guestbook Area --- */
.guestbook-container { margin-top: 20px; }
.divider-text { 
  text-align: center; color: #aaa; font-family: var(--mono); font-size: 0.7rem; 
  margin-bottom: 15px; letter-spacing: 2px; 
}

/* --- Blogs Enhanced --- */
.blog-timeline { position: relative; padding-left: 20px; }
.timeline-line { 
  position: absolute; left: 49px; top: 0; bottom: 0; width: 2px; 
  background: repeating-linear-gradient(to bottom, var(--black) 0, var(--black) 5px, transparent 5px, transparent 10px); 
}
.blog-entry { display: flex; gap: 25px; margin-bottom: 25px; align-items: flex-start; }
.date-col { 
  width: 60px; text-align: center; border: 2px solid var(--black); 
  background: #fff; z-index: 1; 
}
.day { display: block; font-size: 1.4rem; font-family: var(--heading); background: var(--black); color: var(--white); }
.month { display: block; font-size: 0.7rem; font-weight: bold; padding: 3px 0; }
.content-col { flex: 1; }

.blog-card { 
  border: 2px solid var(--black); background: #fff; padding: 15px; 
  position: relative; transition: 0.2s; cursor: pointer;
  box-shadow: 4px 4px 0 #ddd;
}
.blog-card:hover { 
  box-shadow: 6px 6px 0 var(--red); 
  transform: translateX(5px); 
}
.b-title { margin: 0 0 8px 0; font-size: 1.1rem; font-weight: 800; display: flex; align-items: center; gap: 5px; }
.prefix { color: var(--red); font-family: var(--mono); font-size: 0.8rem; }
.b-excerpt { color: #555; font-size: 0.85rem; margin-bottom: 15px; font-family: var(--sans); }
.b-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px dashed #ccc; padding-top: 10px; }
.cyber-tag { background: #eee; font-size: 0.65rem; padding: 2px 6px; font-family: var(--mono); margin-right: 5px; color: #555; }
.read-btn { 
  background: transparent; border: none; color: var(--red); font-weight: bold; 
  font-family: var(--mono); font-size: 0.75rem; cursor: pointer;
}
.read-btn:hover { text-decoration: underline; }

/* --- Activity Log Enhanced --- */
.terminal-log { 
  background: #0a0a0a; color: #33ff00; padding: 20px; 
  font-family: 'JetBrains Mono', monospace; height: 100%; overflow-y: auto; 
  border: 2px solid #333; position: relative; 
}
.scan-bar {
  position: absolute; top: 0; left: 0; width: 100%; height: 20px;
  background: rgba(51, 255, 0, 0.1);
  animation: scan 3s linear infinite; pointer-events: none;
}
@keyframes scan { 0% { top: -20px; } 100% { top: 100%; } }

.log-header { 
  display: grid; grid-template-columns: 120px 100px 1fr; 
  border-bottom: 1px solid #33ff00; padding-bottom: 10px; margin-bottom: 10px; 
  opacity: 0.8; font-size: 0.75rem; 
}
.log-row { display: grid; grid-template-columns: 120px 100px 1fr; margin-bottom: 8px; font-size: 0.8rem; opacity: 0.9; }
.log-row:hover { background: rgba(51, 255, 0, 0.1); }

.col-action.info { color: #00ccff; }
.col-action.warn { color: #ffcc00; }
.col-action.error { color: #ff3333; }
.log-cursor { animation: blink 1s infinite; margin-top: 10px; }

/* --- Animations --- */
.blink { animation: blink 1s infinite; }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* Cyber Slide Transition */
.cyber-slide-enter-active,
.cyber-slide-leave-active {
  transition: all 0.3s ease;
}
.cyber-slide-enter-from {
  opacity: 0;
  transform: translateX(-10px);
  filter: blur(2px);
}
.cyber-slide-leave-to {
  opacity: 0;
  transform: translateX(10px);
  filter: blur(2px);
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
</style>