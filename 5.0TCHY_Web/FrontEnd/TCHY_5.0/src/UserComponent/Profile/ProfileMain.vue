<template>
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
                <img :src="work.cover" loading="lazy" alt="work cover" />
                <div class="overlay">
                  <span>查看详情 ></span>
                </div>
              </div>
              <div class="work-meta">
                <div class="w-title">{{ work.title }}</div>
                <div class="w-stats">
                  <span>♥ {{ work.likes }}</span>
                  <span style="font-size: 0.6rem; border:1px solid #ccc; padding:0 2px;">
                    {{ work.category === 'ui' ? 'UI' : 'ART' }}
                  </span>
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
</template>

<script setup>
import { ref, computed } from 'vue'

// --- State Management ---
const currentTab = ref('works')
const viewportRef = ref(null)
const activeWorkFilter = ref('all')
const newGuestMsg = ref('')

// --- Data Constants ---
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

// Mock Data: Works
const works = ref(Array.from({ length: 8 }).map((_, i) => ({
  id: i,
  title: `霓虹计划_第${i+1}期`,
  likes: 100 + i * 15,
  date: '2023.10.12',
  cover: `https://picsum.photos/400/300?random=${i}`,
  category: i % 2 === 0 ? 'ui' : 'art'
})))

// Mock Data: Blogs
const blogs = ref([
  { id: 1, day: '15', month: '10月', title: '低保真界面的美学复兴', excerpt: '为什么在人工智能生成平滑图像的时代，我们开始回归粗野主义设计风格...', tags: ['设计杂谈', '思考'] },
  { id: 2, day: '02', month: '09月', title: 'Vue 3 渲染性能深度优化', excerpt: '关于如何减少大型工业级仪表盘页面渲染开销的技术报告...', tags: ['代码', '开发'] },
  { id: 3, day: '28', month: '08月', title: '赛博朋克色彩理论研究', excerpt: '高对比度色彩方案在黑暗模式下的视觉引导作用。', tags: ['艺术', '色彩'] }
])

// Mock Data: Activities
const activities = ref([
  { time: '2023-10-15 14:30', action: '上传', type: 'info', detail: '发布了新作品 "霓虹创世纪"。' },
  { time: '2023-10-14 09:15', action: '评论', type: 'success', detail: '在用户 @Ghost 的帖子下留言。' },
  { time: '2023-10-12 18:45', action: '登录', type: 'warn', detail: '检测到来自 HK_02 节点的系统访问。' },
  { time: '2023-10-10 11:20', action: '点赞', type: 'success', detail: '点赞了作品 "机械设计_v2"。' },
  { time: '2023-10-05 22:00', action: '系统', type: 'error', detail: '连接超时，已尝试自动重连。' }
])

// Mock Data: Guest Messages
const guestMessages = ref([
  { id: 1024, user: 'NetRunner_01', date: '2023-10-14 22:00', content: '非常喜欢你的配色方案，特别是红色警戒色的运用。期待更多作品！', stampType: 'approved', stampText: 'APPROVED' },
  { id: 1023, user: 'Unknown_V', date: '2023-10-12 10:15', content: 'Ping... 这里的交互逻辑很流畅。使用的是哪个版本的框架？', stampType: 'reviewed', stampText: 'REVIEWED' },
  { id: 1022, user: 'Design_Bot', date: '2023-10-10 09:30', content: '[自动留言] 系统检测到高能级设计作品。', stampType: 'system', stampText: 'LOGGED' },
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
  // 简单滚动动画模拟
  setTimeout(() => {
    if (viewportRef.value) viewportRef.value.scrollTop = 150 
  }, 100)
}
</script>

<style scoped>
/* 引入字体，保证独立组件也能渲染正确字体 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Caveat:wght@700&display=swap');

/* --- 局部变量定义 (模拟父级环境) --- */
.content-area {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #333;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --sans: 'Noto Sans SC', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  --hand: 'Caveat', cursive;
}

/* --- Core Layout --- */
.content-area {
  flex: 1;
  display: flex; flex-direction: column;
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  overflow: hidden;
  height: 100%; /* 确保在父容器中撑满 */
  box-sizing: border-box;
  font-family: var(--mono), var(--sans);
  color: var(--black);
}

/* --- Tab Controller --- */
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

/* --- Viewport & Common --- */
.viewport { flex: 1; overflow-y: auto; padding: 30px; position: relative; scroll-behavior: smooth; }

/* 筛选条 */
.section-toolbar { display: flex; align-items: center; gap: 15px; margin-bottom: 20px; font-size: 0.85rem; border-bottom: 2px solid #eee; padding-bottom: 10px; font-family: var(--sans); font-weight: bold; }
.filter-chips { display: flex; gap: 10px; }
.chip { border: 1px solid var(--black); padding: 4px 12px; cursor: pointer; transition: 0.2s; font-size: 0.8rem; user-select: none; }
.chip.active, .chip:hover { background: var(--black); color: var(--white); }
.chip:active { transform: scale(0.95); }

/* --- Tab: Works --- */
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

/* --- Tab: Blogs --- */
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

/* --- Tab: Activity --- */
.terminal-log { background: #000; color: #0f0; padding: 20px; font-family: 'Courier New', Courier, monospace; height: 100%; overflow-y: auto; border: 2px solid #333; box-sizing: border-box; }
.log-header { display: grid; grid-template-columns: 160px 100px 1fr; border-bottom: 1px dashed #0f0; padding-bottom: 10px; margin-bottom: 10px; opacity: 0.7; font-size: 0.8rem; }
.log-row { display: grid; grid-template-columns: 160px 100px 1fr; margin-bottom: 8px; font-size: 0.85rem; }
.col-time { opacity: 0.6; font-family: var(--mono); }
.col-action { font-weight: bold; }
.col-action.info { color: #00bfff; }
.col-action.warn { color: #ffff00; }
.col-action.error { color: #ff0000; }
.col-action.success { color: #00ff00; }
.log-cursor { animation: blink 1s infinite; font-weight: bold; margin-top: 10px; }

/* --- Tab: Guestbook --- */
.view-guestbook { display: flex; flex-direction: column; gap: 30px; max-width: 800px; }
.guestbook-input-area { border: 2px solid var(--black); background: #eee; padding: 15px; }
.gb-input-header { font-family: var(--mono); font-weight: bold; margin-bottom: 10px; font-size: 0.8rem; color: #555; }
.gb-form { display: flex; flex-direction: column; gap: 10px; }
.gb-textarea { width: 100%; border: 1px solid var(--black); padding: 10px; font-family: var(--sans); background: var(--white); resize: vertical; min-height: 80px; box-sizing: border-box; }
.gb-textarea:focus { outline: 2px solid var(--red); }
.gb-actions { display: flex; justify-content: space-between; align-items: center; }
.deco-code { font-family: var(--mono); font-size: 0.7rem; color: #888; }
.sign-btn { background: var(--black); color: var(--white); border: none; padding: 8px 20px; font-family: var(--mono); font-weight: bold; cursor: pointer; transition: 0.2s; }
.sign-btn:hover { background: var(--red); }
.guestbook-list { display: flex; flex-direction: column; gap: 20px; margin-top: 30px; }
.list-label { font-family: var(--heading); font-size: 1.2rem; border-bottom: 2px solid var(--black); padding-bottom: 5px; margin-bottom: 10px; }
.end-of-file { text-align: center; font-family: var(--mono); color: #ccc; margin-top: 20px; font-size: 0.8rem; }

/* Guestbook Notes */
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

/* --- Utilities & Animations --- */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.blink { animation: blink 1s infinite; }

.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }

.fade-slide-enter-active, .fade-slide-leave-active { transition: opacity 0.3s, transform 0.3s; }
.fade-slide-enter-from { opacity: 0; transform: translateY(10px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }

/* --- Responsive --- */
@media (max-width: 1000px) {
  .content-area { height: 600px; flex: none; width: 100%; }
}
</style>