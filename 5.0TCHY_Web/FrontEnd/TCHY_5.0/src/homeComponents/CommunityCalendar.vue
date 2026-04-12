<template>
  <div class="calendar-container">
    
    <header class="calendar-header">
      <div class="header-left">
        <h2 class="current-month">
          {{ currentYear }}年 {{ currentMonth }}月
        </h2>
        <button @click="goToToday" class="btn-today">回到今天</button>
      </div>

      <div class="header-right">
        <div class="nav-group">
          <button @click="prevMonth" class="btn-icon" title="上个月">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 18l-6-6 6-6"/></svg>
          </button>
          <button @click="nextMonth" class="btn-icon" title="下个月">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18l6-6-6-6"/></svg>
          </button>
        </div>
      </div>
    </header>

    <div class="calendar-grid-wrapper">
      <div class="week-header">
        <div v-for="day in weekDays" :key="day" class="week-cell">{{ day }}</div>
      </div>

      <div class="days-grid">
        <div 
          v-for="(date, index) in calendarDates" 
          :key="index"
          class="day-cell"
          :class="{ 
            'is-other-month': !isCurrentMonth(date),
            'is-today': isToday(date),
            'is-selected': isSelected(date)
          }"
          @click="selectDate(date)"
        >
          <div class="cell-top">
            <span class="day-number">
              {{ date.getDate() }}
              <span v-if="date.getDate() === 1" class="month-suffix">/ {{ date.getMonth() + 1 }}月</span>
            </span>
          </div>

          <div class="cell-events">
            <div 
              v-for="ev in getEventsForDate(date).slice(0, 3)" 
              :key="ev.id" 
              class="event-pill"
              :style="{ backgroundColor: hexToRgba(ev.color, 0.15), color: ev.color, borderLeft: `3px solid ${ev.color}` }"
            >
              <span class="pill-text">{{ ev.title }}</span>
            </div>
            <div v-if="getEventsForDate(date).length > 3" class="more-events">
              +{{ getEventsForDate(date).length - 3 }} 更多
            </div>
          </div>
          
        </div>
      </div>
    </div>

    <Transition name="slide">
      <div v-if="isPanelOpen" class="side-panel-mask" @click.self="closePanel">
        <div class="side-panel">
          <div class="panel-header">
            <div class="ph-date">
              <span class="ph-day">{{ selectedDate.getDate() }}</span>
              <div class="ph-ym">{{ currentYear }}年 {{ selectedDate.getMonth() + 1 }}月</div>
            </div>
            <button class="btn-close" @click="closePanel">×</button>
          </div>

          <div class="panel-body custom-scroll">
            <div v-if="dayEvents.length === 0" class="empty-state">
              <div class="empty-img">📅</div>
              <p>今天暂无安排</p>
            </div>

            <div v-else class="event-timeline">
              <div v-for="ev in dayEvents" :key="ev.id" class="timeline-item">
                <div class="tl-time">{{ ev.time }}</div>
                <div class="tl-card" :style="{ borderLeftColor: ev.color }">
                  <div class="tl-title">{{ ev.title }}</div>
                  <div class="tl-desc" v-if="ev.description">{{ ev.description }}</div>
                  <div class="tl-tag" :style="{ color: ev.color, backgroundColor: hexToRgba(ev.color, 0.1) }">
                    {{ ev.type }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api' 

// --- 状态 ---
const currentYear = ref(new Date().getFullYear())
const currentMonth = ref(new Date().getMonth() + 1)
const selectedDate = ref(new Date())
const isPanelOpen = ref(false)
const weekDays = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']

const monthEvents = ref([]) 
const dayEvents = ref([])   

// --- 逻辑 ---
const calendarDates = computed(() => {
  const dates = []
  const firstDay = new Date(currentYear.value, currentMonth.value - 1, 1)
  const lastDay = new Date(currentYear.value, currentMonth.value, 0)
  const firstDayOfWeek = firstDay.getDay()
  
  for (let i = 0; i < firstDayOfWeek; i++) {
    const d = new Date(firstDay)
    d.setDate(d.getDate() - (firstDayOfWeek - i))
    dates.push(d)
  }
  for (let i = 1; i <= lastDay.getDate(); i++) {
    dates.push(new Date(currentYear.value, currentMonth.value - 1, i))
  }
  const totalCells = 42
  const remaining = totalCells - dates.length
  for (let i = 1; i <= remaining; i++) {
    const d = new Date(lastDay)
    d.setDate(d.getDate() + i)
    dates.push(d)
  }
  return dates
})

const padZero = (n) => n < 10 ? `0${n}` : n
const getDateStr = (date) => `${date.getFullYear()}-${padZero(date.getMonth() + 1)}-${padZero(date.getDate())}`

const hexToRgba = (hex, alpha) => {
  if (!hex) return `rgba(0,0,0,${alpha})`
  let c;
  if(/^#([A-Fa-f0-9]{3}){1,2}$/.test(hex)){
      c= hex.substring(1).split('');
      if(c.length== 3){
          c= [c[0], c[0], c[1], c[1], c[2], c[2]];
      }
      c= '0x'+c.join('');
      return 'rgba('+[(c>>16)&255, (c>>8)&255, c&255].join(',')+','+alpha+')';
  }
  return hex;
}

const isToday = (date) => getDateStr(date) === getDateStr(new Date())
const isCurrentMonth = (date) => date.getMonth() + 1 === currentMonth.value
const isSelected = (date) => getDateStr(date) === getDateStr(selectedDate.value)

// ⚡ 修改点：ev.Date -> ev.date
const updateSelectedDayEvents = () => {
  const key = getDateStr(selectedDate.value)
  dayEvents.value = monthEvents.value.filter(ev => ev.date === key)
}

const getEventsForDate = (date) => {
  const key = getDateStr(date)
  return monthEvents.value.filter(ev => ev.date === key)
}

// API
const fetchMonthlyEvents = async () => {
  try {
    const res = await apiClient.get('/events', {
      params: { year: currentYear.value, month: currentMonth.value }
    })
    // 假设后端现在返回的是驼峰数组 [ { id: 1, title: '...', date: '2026-04-12', color: '#ff0000', ... }, ... ]
    monthEvents.value = res.data
    updateSelectedDayEvents()
  } catch (error) {
    console.error("API Error", error)
  }
}

// 交互
const selectDate = (date) => {
  selectedDate.value = date
  updateSelectedDayEvents()
  isPanelOpen.value = true
}

const closePanel = () => isPanelOpen.value = false
const prevMonth = () => { currentMonth.value === 1 ? (currentMonth.value = 12, currentYear.value--) : currentMonth.value-- }
const nextMonth = () => { currentMonth.value === 12 ? (currentMonth.value = 1, currentYear.value++) : currentMonth.value++ }

const goToToday = () => {
  const today = new Date()
  currentYear.value = today.getFullYear()
  currentMonth.value = today.getMonth() + 1
  selectedDate.value = today
  updateSelectedDayEvents()
}

watch([currentYear, currentMonth], () => fetchMonthlyEvents())
onMounted(() => fetchMonthlyEvents())
</script>

<style scoped>
/* 引入更现代的字体 */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap');

.calendar-container {
  /* 现代配色：柔和的白与灰 */
  --bg-color: #ffffff;
  --text-main: #1f2937;
  --text-light: #6b7280;
  --border-color: #e5e7eb; /* Tailwind Gray-200 */
  --primary-color: #3b82f6; /* Tailwind Blue-500 */
  --highlight-bg: #eff6ff;
  
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: var(--text-main);
  background: #f9fafb; /* 整个页面极浅的灰底 */
  min-height: 100vh;
  padding: 20px;
  box-sizing: border-box;
}

/* 1. 头部 */
.calendar-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 20px; padding: 0 10px;
}
.current-month { font-size: 1.5rem; font-weight: 600; margin: 0; }
.header-left { display: flex; align-items: center; gap: 20px; }

.btn-today {
  background: white; border: 1px solid var(--border-color);
  padding: 6px 12px; border-radius: 6px; font-size: 0.9rem; color: var(--text-main);
  cursor: pointer; transition: all 0.2s;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
.btn-today:hover { background: #f3f4f6; }

.nav-group {
  display: flex; gap: 8px; background: white; padding: 4px; border-radius: 8px;
  border: 1px solid var(--border-color);
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
.btn-icon {
  border: none; background: transparent; cursor: pointer;
  padding: 4px 8px; border-radius: 4px; color: var(--text-light);
  display: flex; align-items: center;
}
.btn-icon:hover { background: #f3f4f6; color: var(--text-main); }

/* 2. 网格主体 */
.calendar-grid-wrapper {
  background: white;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.week-header {
  display: grid; grid-template-columns: repeat(7, 1fr);
  border-bottom: 1px solid var(--border-color);
  background: #f9fafb;
}
.week-cell {
  text-align: center; padding: 12px 0;
  font-size: 0.85rem; font-weight: 600; color: var(--text-light);
}

.days-grid {
  display: grid; grid-template-columns: repeat(7, 1fr);
  /* 使用背景色做边框，制造细腻的分割线效果 */
  background: var(--border-color); 
  gap: 1px; 
}

.day-cell {
  background: white;
  min-height: 120px; /* 保证格子高度 */
  padding: 8px;
  cursor: pointer;
  transition: background 0.2s;
  display: flex; flex-direction: column; gap: 6px;
}

.day-cell:hover { background: #fafafa; }
.day-cell.is-other-month { background: #fcfcfc; color: #d1d5db; }
.day-cell.is-other-month .day-number { color: #d1d5db; }

/* 日期数字 */
.cell-top { display: flex; justify-content: flex-start; }
.day-number { 
  font-weight: 500; font-size: 0.9rem; width: 28px; height: 28px; 
  display: flex; align-items: center; justify-content: center; border-radius: 50%;
}
.month-suffix { font-size: 0.7em; margin-left: 4px; color: var(--text-light); font-weight: normal; }

/* "今天"样式 */
.is-today .day-number { background: var(--primary-color); color: white; }

/* 选中样式 */
.is-selected { background: var(--highlight-bg); }

/* 活动胶囊条 */
.cell-events { display: flex; flex-direction: column; gap: 4px; }
.event-pill {
  font-size: 0.75rem; padding: 2px 6px; border-radius: 4px;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  font-weight: 500; line-height: 1.4;
  transition: transform 0.1s;
}
.event-pill:hover { transform: scale(1.02); }

.more-events { font-size: 0.7rem; color: var(--text-light); padding-left: 6px; }


/* 3. 侧边栏 (Side Panel) */
.side-panel-mask {
  position: fixed; inset: 0; z-index: 50;
  background: rgba(0,0,0,0.2); backdrop-filter: blur(2px);
  display: flex; justify-content: flex-end;
}

.side-panel {
  width: 380px; max-width: 100%;
  background: white;
  height: 100%;
  box-shadow: -4px 0 15px rgba(0,0,0,0.1);
  display: flex; flex-direction: column;
}

.panel-header {
  padding: 20px; border-bottom: 1px solid var(--border-color);
  display: flex; justify-content: space-between; align-items: flex-start;
}
.ph-date { display: flex; align-items: baseline; gap: 10px; }
.ph-day { font-size: 2.5rem; font-weight: 700; color: var(--primary-color); line-height: 1; }
.ph-ym { font-size: 1rem; color: var(--text-light); }

.btn-close {
  background: none; border: none; font-size: 1.5rem; color: var(--text-light);
  cursor: pointer; padding: 0 5px; line-height: 1;
}
.btn-close:hover { color: var(--text-main); }

.panel-body { padding: 20px; flex: 1; overflow-y: auto; }

/* 空状态 */
.empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  height: 200px; color: var(--text-light);
}
.empty-img { font-size: 3rem; margin-bottom: 10px; opacity: 0.5; }

/* 时间轴列表 */
.event-timeline { display: flex; flex-direction: column; gap: 20px; }
.timeline-item { display: flex; gap: 15px; }
.tl-time { 
  font-size: 0.85rem; color: var(--text-light); min-width: 45px; text-align: right; 
  padding-top: 2px; font-variant-numeric: tabular-nums;
}
.tl-card {
  flex: 1; border-left: 3px solid #ccc; padding-left: 12px;
}
.tl-title { font-weight: 600; font-size: 1rem; margin-bottom: 4px; }
.tl-desc { font-size: 0.85rem; color: #666; margin-bottom: 8px; line-height: 1.5; }
.tl-tag {
  display: inline-block; font-size: 0.7rem; padding: 2px 8px; border-radius: 12px; font-weight: 500;
}

/* 动画 */
.slide-enter-active, .slide-leave-active { transition: transform 0.3s ease; }
.slide-enter-from, .slide-leave-to { transform: translateX(100%); }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ddd; border-radius: 3px; }

@media (max-width: 640px) {
  .calendar-container { padding: 10px; }
  .day-cell { min-height: 80px; }
  .event-pill { font-size: 0.65rem; }
}
</style>