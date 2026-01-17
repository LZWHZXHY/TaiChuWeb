<template>
  <div class="viewport custom-scroll" ref="viewportRef">
    <Transition name="fade-slide" mode="out-in">
      <!-- 作品 Tab -->
      <div v-if="currentTab === 'works'" class="view-section view-works" key="works">
        <div class="section-toolbar">
          <span class="label">筛选类型:</span>
          <div class="filter-chips">
            <span 
              v-for="filter in workFilters" 
              :key="filter.key"
              class="chip" 
              :class="{ active: activeWorkFilter === filter.key }"
              @click="handleWorkFilterChange(filter.key)"
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
            @click="handleGoToWorkDetail(work.id)"
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

        <!-- 留言板组件容器 -->
        <div class="guestbook-container">
          <GuestbookInput />
        </div>
      </div>

      <!-- 博客 Tab -->
      <div v-else-if="currentTab === 'blogs'" class="view-section view-blogs" key="blogs">
        <div class="blog-timeline">
          <div v-for="blog in blogs" :key="blog.id" class="blog-entry">
            <div class="date-col">
              <span class="day">{{ blog.day }}</span>
              <span class="month">{{ blog.month }}</span>
            </div>
            <div class="content-col">
              <div class="blog-card" @click="handleGoToBlogDetail(blog.id)">
                <h3 class="b-title">{{ blog.title }}</h3>
                <p class="b-excerpt">{{ blog.excerpt }}</p>
                <div class="b-footer">
                  <div class="tags">
                    <span v-for="t in blog.tags" :key="t">#{{ t }}</span>
                  </div>
                  <button class="read-btn" @click.stop="handleGoToBlogDetail(blog.id)">阅读档案 >></button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 活动 Tab -->
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
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import GuestbookInput from './GuestbookInput.vue'

// 定义 Props（接收父组件传递的数据源和状态）
const props = defineProps({
  currentTab: {
    type: String,
    required: true,
    default: 'works'
  },
  workFilters: {
    type: Array,
    required: true
  },
  works: {
    type: Array,
    required: true
  },
  blogs: {
    type: Array,
    required: true
  },
  activities: {
    type: Array,
    required: true
  },
  activeWorkFilter: {
    type: String,
    required: true,
    default: 'all'
  }
})

// 定义 Emit（向父组件传递事件）
const emit = defineEmits([
  'switch-tab',       // 切换Tab
  'go-to-work-detail',// 打开作品详情
  'go-to-blog-detail',// 打开博客详情
  'change-work-filter'// 切换作品筛选
])

// 容器Ref（用于滚动置顶）
const viewportRef = ref(null)

// 计算属性：筛选后的作品列表
const filteredWorks = computed(() => {
  if (props.activeWorkFilter === 'all') return props.works
  return props.works.filter(w => w.category === props.activeWorkFilter)
})

// 方法：切换作品筛选
const handleWorkFilterChange = (filterKey) => {
  emit('change-work-filter', filterKey)
}

// 方法：打开作品详情
const handleGoToWorkDetail = (id) => {
  emit('go-to-work-detail', id)
}

// 方法：打开博客详情
const handleGoToBlogDetail = (id) => {
  emit('go-to-blog-detail', id)
}

// 暴露方法给父组件（比如滚动置顶）
defineExpose({
  scrollToTop: () => {
    if (viewportRef.value) viewportRef.value.scrollTop = 0
  }
})
</script>

<style scoped>
/* 引入字体 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Caveat:wght@700&display=swap');

/* 局部变量定义 */
.viewport {
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

/* --- Viewport 核心样式 --- */
.viewport { 
  flex: 1; 
  overflow-y: auto; 
  padding: 30px; 
  position: relative; 
  scroll-behavior: smooth; 
}

/* 筛选条样式 */
.section-toolbar { 
  display: flex; 
  align-items: center; 
  gap: 15px; 
  margin-bottom: 20px; 
  font-size: 0.85rem; 
  border-bottom: 2px solid #eee; 
  padding-bottom: 10px; 
  font-family: var(--sans); 
  font-weight: bold; 
}
.filter-chips { display: flex; gap: 10px; }
.chip { 
  border: 1px solid var(--black); 
  padding: 4px 12px; 
  cursor: pointer; 
  transition: 0.2s; 
  font-size: 0.8rem; 
  user-select: none; 
}
.chip.active, .chip:hover { 
  background: var(--black); 
  color: var(--white); 
}
.chip:active { transform: scale(0.95); }

/* --- 作品 Tab 样式 --- */
.works-grid { 
  display: grid; 
  grid-template-columns: repeat(auto-fill, minmax(130px, 3fr)); 
  gap: 20px; 
  margin-bottom: 30px; 
}
.work-item { 
  border: 2px solid var(--black); 
  transition: transform 0.2s; 
  cursor: pointer; 
  background: #4b4a4a;
  height:100% ;
}
.work-item:hover { 
  transform: translateY(-5px); 
  box-shadow: 6px 6px 0 var(--red); 
}
.img-wrapper { 
  height: 70%; 
  overflow: hidden; 
  position: relative; 
  border-bottom: 2px solid var(--black); 
}
.img-wrapper img { 
  width: 100%; 
  height: 100%; 
  object-fit: cover; 
  filter: grayscale(30%); 
  transition: 0.3s; 
}
.work-item:hover img { 
  filter: grayscale(0) contrast(1.1); 
  transform: scale(1.05); 
}
.overlay { 
  position: absolute; 
  inset: 0; 
  background: rgba(0,0,0,0.7); 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  color: var(--white); 
  opacity: 0; 
  transition: 0.2s; 
  font-weight: bold; 
  font-family: var(--sans); 
}
.work-item:hover .overlay { opacity: 1; }
.work-meta { padding: 5px; background: #fff; }
.w-title { 
  font-weight: bold; 
  font-size: 0.95rem; 
  margin-bottom: 5px; 
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis; 
  font-family: var(--sans); 
}
.w-stats { 
  display: flex; 
  justify-content: space-between; 
  font-size: 0.75rem; 
  color: #666; 
  font-family: var(--mono); 
}
.empty-state { 
  width: 100%; 
  grid-column: 1 / -1; 
  padding: 40px; 
  text-align: center; 
  color: #999; 
  border: 2px dashed #ddd; 
  font-family: var(--mono); 
}

/* 留言板容器样式 */
.guestbook-container {
  width: 100%;
  margin-top: 20px;
  padding: 0px;
}

/* --- 博客 Tab 样式 --- */
.blog-timeline { 
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
  max-width: 800px; 
}
.blog-entry { display: flex; gap: 20px; }
.date-col { 
  width: 60px; 
  text-align: center; 
  border: 2px solid var(--black); 
  height: fit-content; 
  background: #fff; 
}
.date-col .day { 
  display: block; 
  font-size: 1.5rem; 
  font-family: var(--heading); 
  background: var(--black); 
  color: var(--white); 
}
.date-col .month { 
  display: block; 
  font-size: 0.85rem; 
  font-weight: bold; 
  padding: 5px 0; 
  font-family: var(--sans); 
}
.content-col { flex: 1; }
.blog-card { 
  border: 2px solid var(--black); 
  background: #fff; 
  padding: 20px; 
  position: relative; 
  transition: 0.2s; 
  cursor: pointer; 
}
.blog-card:hover { 
  box-shadow: 6px 6px 0 var(--gray-dark); 
  transform: translateX(5px); 
}
.b-title { 
  margin: 0 0 10px 0; 
  font-family: var(--sans); 
  font-size: 1.2rem; 
  font-weight: bold; 
}
.b-excerpt { 
  color: #555; 
  font-size: 0.9rem; 
  margin-bottom: 15px; 
  font-family: var(--sans); 
  line-height: 1.6; 
}
.b-footer { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  border-top: 1px dashed #ccc; 
  padding-top: 10px; 
}
.read-btn { 
  border: none; 
  background: transparent; 
  color: var(--red); 
  font-weight: bold; 
  cursor: pointer; 
  font-family: var(--sans); 
  font-size: 0.85rem; 
  transition: 0.2s; 
}
.read-btn:hover { 
  text-decoration: underline; 
  background: #eee; 
}

/* --- 活动 Tab 样式 --- */
.terminal-log { 
  background: #000; 
  color: #0f0; 
  padding: 20px; 
  font-family: 'Courier New', Courier, monospace; 
  height: 100%; 
  overflow-y: auto; 
  border: 2px solid #333; 
  box-sizing: border-box; 
}
.log-header { 
  display: grid; 
  grid-template-columns: 160px 100px 1fr; 
  border-bottom: 1px dashed #0f0; 
  padding-bottom: 10px; 
  margin-bottom: 10px; 
  opacity: 0.7; 
  font-size: 0.8rem; 
}
.log-row { 
  display: grid; 
  grid-template-columns: 160px 100px 1fr; 
  margin-bottom: 8px; 
  font-size: 0.85rem; 
}
.col-time { 
  opacity: 0.6; 
  font-family: var(--mono); 
}
.col-action { font-weight: bold; }
.col-action.info { color: #00bfff; }
.col-action.warn { color: #ffff00; }
.col-action.error { color: #ff0000; }
.col-action.success { color: #00ff00; }
.log-cursor { animation: blink 1s infinite; font-weight: bold; margin-top: 10px; }

/* --- 通用样式 & 动画 --- */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.blink { animation: blink 1s infinite; }

.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
</style>