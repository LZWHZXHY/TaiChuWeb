<template>
  <div class="calendar-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="calendar-wrapper">
      
      <header class="control-console">
        <div class="console-left">
          <h1 class="giant-text">
            <span class="deco-box">■</span> EVENT_LOG
          </h1>
          <div class="sub-text">社区时间线 // CHRONO_MATRIX</div>
        </div>

        <div class="console-center">
          <div class="time-display">
            <button @click="prevYear" class="cyber-nav-btn" title="PREV_YEAR">&lt;&lt;</button>
            <button @click="prevMonth" class="cyber-nav-btn" title="PREV_MONTH">&lt;</button>
            
            <div class="current-date-box">
              <span class="year-val">{{ currentYear }}</span>
              <span class="divider">/</span>
              <span class="month-val">{{ padZero(currentMonth) }}</span>
            </div>

            <button @click="nextMonth" class="cyber-nav-btn" title="NEXT_MONTH">&gt;</button>
            <button @click="nextYear" class="cyber-nav-btn" title="NEXT_YEAR">&gt;&gt;</button>
          </div>
          <button @click="goToToday" class="today-reset-btn">[ RESET_TO_TODAY ]</button>
        </div>

        <div class="console-right" v-if="isAdmin">
          <button @click="showCreateEvent" class="action-btn-rect">
             <span>+ CREATE_EVENT</span>
          </button>
          <button @click="showEventManager" class="action-btn-rect secondary">
             <span># MANAGE_DB</span>
          </button>
        </div>
      </header>

      <div class="matrix-container">
        <div class="week-header-row">
          <div v-for="day in weekDaysEn" :key="day" class="week-cell">
            {{ day }}
          </div>
        </div>

        <div class="days-grid">
          <div 
            v-for="(date, index) in calendarDates" 
            :key="index"
            class="day-cell"
            :class="getCellClass(date)"
            @click="selectDate(date)"
          >
            <div class="corner-lt"></div>
            <div class="corner-rb"></div>

            <div class="day-header">
              <span class="day-num">{{ padZero(date.getDate()) }}</span>
              <span v-if="date.getDate() === 1" class="month-label">
                {{ date.getMonth() + 1 }}月
              </span>
            </div>
            
            <div v-if="isToday(date)" class="today-marker">TODAY</div>
            
            <div class="event-dots" v-if="hasEvents(date)">
               <span class="dot"></span>
            </div>

            <div class="target-frame" v-if="isSelected(date)"></div>
          </div>
        </div>
      </div>

      <div class="event-detail-panel">
        <div class="panel-header">
           <span class="icon">►</span>
           <span class="panel-title">
             ACTIVE_EVENTS // {{ formatSelectedDate(selectedDate) }}
           </span>
           <div class="view-switch">
             <span :class="{ active: viewMode === 'month' }" @click="viewMode = 'month'">GRID</span>
             <span class="sep">|</span>
             <span :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">LIST</span>
           </div>
        </div>

        <div class="panel-body custom-scroll">
          <div v-if="dayEvents.length === 0" class="empty-signal">
            <span class="blink">_</span> NO_SIGNAL_DETECTED
            <div class="sub">该日期暂无活动安排</div>
          </div>
          
          <div v-else class="event-list">
             <div v-for="ev in dayEvents" :key="ev.id" class="event-item">
               <div class="ev-time">{{ ev.time }}</div>
               <div class="ev-content">
                 <div class="ev-title">{{ ev.title }}</div>
                 <div class="ev-type" :style="{ color: ev.color }">[{{ ev.type }}]</div>
               </div>
             </div>
          </div>
        </div>
      </div>

      <div class="legend-strip">
        <div class="legend-label">SIGNAL_TYPES:</div>
        <div class="legend-items">
          <div v-for="type in eventTypes" :key="type.id" class="l-item">
            <span class="l-box" :style="{ background: type.color }"></span>
            <span class="l-text">{{ type.name }}</span>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const currentYear = ref(new Date().getFullYear())
const currentMonth = ref(new Date().getMonth() + 1)
const selectedDate = ref(new Date())
const viewMode = ref('month')
const weekDaysEn = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT']

const isAdmin = ref(true) // 假设管理员权限开启以展示按钮

// 模拟事件数据
const eventTypes = [
  { id: 'tech', name: 'TECH_SHARE', color: '#4CAF50' },
  { id: 'social', name: 'SOCIAL', color: '#2196F3' },
  { id: 'update', name: 'UPDATE', color: '#FF9800' },
  { id: 'qa', name: 'Q&A', color: '#9C27B0' },
  { id: 'meeting', name: 'MEETING', color: '#F44336' },
  { id: 'announcement', name: 'NOTICE', color: '#607D8B' }
]

// 假数据：仅供展示
const dayEvents = ref([]) 

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

const isToday = (date) => {
  const today = new Date()
  return date.getDate() === today.getDate() && 
         date.getMonth() === today.getMonth() &&
         date.getFullYear() === today.getFullYear()
}

const isCurrentMonth = (date) => {
  return date.getMonth() + 1 === currentMonth.value &&
         date.getFullYear() === currentYear.value
}

const isSelected = (date) => {
  return selectedDate.value && 
         date.getDate() === selectedDate.value.getDate() && 
         date.getMonth() === selectedDate.value.getMonth() &&
         date.getFullYear() === selectedDate.value.getFullYear()
}

const getCellClass = (date) => {
  return isCurrentMonth(date) ? 'in-month' : 'out-month'
}

const hasEvents = (date) => false // 暂时没接真实数据

const formatSelectedDate = (date) => {
  return `${date.getFullYear()}.${padZero(date.getMonth() + 1)}.${padZero(date.getDate())}`
}

const prevMonth = () => {
  if (currentMonth.value === 1) { currentMonth.value = 12; currentYear.value -= 1 } 
  else { currentMonth.value -= 1 }
}
const nextMonth = () => {
  if (currentMonth.value === 12) { currentMonth.value = 1; currentYear.value += 1 } 
  else { currentMonth.value += 1 }
}
const prevYear = () => currentYear.value -= 1
const nextYear = () => currentYear.value += 1

const goToToday = () => {
  const today = new Date()
  currentYear.value = today.getFullYear()
  currentMonth.value = today.getMonth() + 1
  selectedDate.value = today
}

const selectDate = (date) => {
  selectedDate.value = date
  // 这里可以去 fetch 这一天的 events
}

const showCreateEvent = () => alert('INIT_CREATE_PROTOCOL...')
const showEventManager = () => alert('ACCESS_DB_MANAGER...')

onMounted(() => {
  // fetchEvents()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.calendar-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  min-height: 100vh;
  position: relative;
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono);
  overflow: hidden;
  padding: 40px 0;
}

/* 背景网格 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* 主容器 */
.calendar-wrapper {
  position: relative;
  z-index: 1;
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* --- 1. 头部控制台 --- */
.control-console {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  border-bottom: 4px solid var(--black);
  padding-bottom: 20px;
}
.console-left .giant-text {
  font-family: var(--heading);
  font-size: 3rem; margin: 0; line-height: 1;
  display: flex; align-items: center; gap: 10px;
}
.deco-box { color: var(--red); font-size: 0.8em; }
.sub-text { font-weight: bold; color: #666; margin-top: 5px; font-size: 0.9rem; }

.console-center {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}
.time-display {
  display: flex;
  align-items: center;
  gap: 10px;
}
.cyber-nav-btn {
  background: var(--white); border: 2px solid var(--black);
  font-family: var(--mono); font-weight: bold; cursor: pointer;
  width: 40px; height: 30px;
  transition: all 0.2s;
}
.cyber-nav-btn:hover { background: var(--black); color: var(--white); }

.current-date-box {
  font-family: var(--heading); font-size: 2rem;
  background: var(--black); color: var(--white);
  padding: 0 15px; display: flex; align-items: baseline; gap: 5px;
}
.divider { color: var(--red); }

.today-reset-btn {
  font-family: var(--mono); font-size: 0.8rem;
  background: none; border: none; cursor: pointer;
  text-decoration: underline; color: #666;
}
.today-reset-btn:hover { color: var(--red); }

.console-right { display: flex; gap: 10px; }
.action-btn-rect {
  background: var(--red); color: var(--white);
  border: 2px solid var(--black); padding: 10px 15px;
  font-family: var(--mono); font-weight: bold; cursor: pointer;
  box-shadow: 4px 4px 0 var(--black);
  transition: all 0.2s;
}
.action-btn-rect.secondary { background: var(--white); color: var(--black); }
.action-btn-rect:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }

/* --- 2. 日历矩阵 --- */
.matrix-container {
  border: 2px solid var(--black);
  background: #fff;
  padding: 5px;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}

.week-header-row {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  border-bottom: 2px solid var(--black);
  margin-bottom: 5px;
}
.week-cell {
  text-align: center;
  font-weight: 800;
  padding: 10px 0;
  background: #eee;
  border-right: 1px solid var(--black);
}
.week-cell:last-child { border-right: none; }

.days-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
  background: var(--black); /* 缝隙颜色 */
}

.day-cell {
  background: var(--white);
  aspect-ratio: 1;
  position: relative;
  cursor: pointer;
  padding: 5px;
  display: flex;
  flex-direction: column;
  transition: all 0.2s;
}
.day-cell:hover { background: #fff; z-index: 2; }

/* 非本月样式 */
.day-cell.out-month {
  background: repeating-linear-gradient(45deg, #f0f0f0, #f0f0f0 5px, #e0e0e0 5px, #e0e0e0 10px);
  color: #aaa;
}
.day-cell.out-month .day-num { opacity: 0.5; }

/* 日期数字 */
.day-header { display: flex; justify-content: space-between; font-size: 0.9rem; font-weight: bold; }
.month-label { font-size: 0.7rem; background: var(--black); color: var(--white); padding: 0 4px; }

/* 今天标记 */
.today-marker {
  position: absolute; bottom: 5px; right: 5px;
  font-size: 0.6rem; font-weight: 900; color: var(--red);
  border: 1px solid var(--red); padding: 0 2px;
}

/* 选中状态瞄准框 */
.target-frame {
  position: absolute; inset: 4px;
  border: 2px solid var(--black);
  pointer-events: none;
  animation: pulseBorder 2s infinite;
}
.target-frame::before {
  content: ''; position: absolute; top: -4px; left: -4px; width: 8px; height: 8px; border-top: 2px solid var(--red); border-left: 2px solid var(--red);
}
.target-frame::after {
  content: ''; position: absolute; bottom: -4px; right: -4px; width: 8px; height: 8px; border-bottom: 2px solid var(--red); border-right: 2px solid var(--red);
}

/* --- 3. 详情面板 --- */
.event-detail-panel {
  border: 2px solid var(--black);
  background: #fff;
  min-height: 200px;
}
.panel-header {
  background: var(--black); color: var(--white);
  padding: 10px 15px;
  display: flex; align-items: center; gap: 10px;
}
.icon { color: var(--red); }
.panel-title { font-weight: bold; flex: 1; }
.view-switch { font-size: 0.8rem; }
.view-switch span { cursor: pointer; opacity: 0.5; }
.view-switch span.active { opacity: 1; font-weight: bold; color: var(--red); }
.view-switch .sep { margin: 0 5px; opacity: 0.3; }

.panel-body { padding: 20px; }
.empty-signal {
  text-align: center; color: #999; font-weight: bold; padding: 30px;
}
.blink { animation: blink 1s infinite; }
.sub { font-size: 0.8rem; font-weight: normal; margin-top: 5px; }

/* --- 4. 图例 --- */
.legend-strip {
  display: flex; align-items: center; gap: 20px;
  border-top: 2px dashed #ccc; padding-top: 20px;
}
.legend-label { font-weight: bold; font-size: 0.9rem; }
.legend-items { display: flex; flex-wrap: wrap; gap: 15px; }
.l-item { display: flex; align-items: center; gap: 5px; font-size: 0.8rem; }
.l-box { width: 12px; height: 12px; border: 1px solid var(--black); }

/* 动画 */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@keyframes pulseBorder { 0% { border-color: var(--black); } 50% { border-color: var(--red); } 100% { border-color: var(--black); } }

/* 响应式 */
@media (max-width: 768px) {
  .control-console { flex-direction: column; align-items: flex-start; gap: 20px; }
  .console-center { width: 100%; }
  .matrix-container { padding: 2px; }
  .day-cell { aspect-ratio: auto; min-height: 50px; }
  .console-right { width: 100%; justify-content: space-between; }
  .action-btn-rect { flex: 1; text-align: center; }
}
</style>