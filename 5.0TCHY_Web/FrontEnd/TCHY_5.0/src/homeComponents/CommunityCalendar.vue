<template>
  <div class="community-calendar">
    <!-- 日历头部 -->
    <div class="calendar-header">
      <div class="header-main">
        <h1>📅 社区日历</h1>
        <div class="header-info">
          <span class="stats">暂无活动</span>
          <span v-if="selectedDate" class="selected-date">{{ formatSelectedDate(selectedDate) }}</span>
        </div>
      </div>
      
      <!-- 操作按钮 -->
      <div class="action-buttons" v-if="isAdmin">
        <button @click="showCreateEvent" class="btn-primary">
          <span>+</span>
          <span>创建活动</span>
        </button>
        <button @click="showEventManager" class="btn-secondary">
          <span>📊</span>
          <span>管理活动</span>
        </button>
      </div>
    </div>

    <!-- 日历主区域 -->
    <div class="calendar-main">
      <!-- 月份导航 -->
      <div class="month-navigation">
        <button @click="prevYear" class="nav-btn" title="上一年">«</button>
        <button @click="prevMonth" class="nav-btn" title="上一月">‹</button>
        
        <div class="current-month">
          <span class="month-year">{{ currentYear }}年 {{ currentMonth }}月</span>
          <button @click="goToToday" class="today-btn">今天</button>
        </div>
        
        <button @click="nextMonth" class="nav-btn" title="下一月">›</button>
        <button @click="nextYear" class="nav-btn" title="下一年">»</button>
      </div>

      <!-- 日历容器 -->
      <div class="calendar-container">
        <!-- 星期标题 -->
        <div class="week-header">
          <div v-for="day in weekDays" :key="day" class="week-day">{{ day }}</div>
        </div>

        <!-- 日历网格 -->
        <div class="calendar-grid">
          <div 
            v-for="(date, index) in calendarDates" 
            :key="index"
            class="calendar-cell"
            :class="getCellClass(date)"
            @click="selectDate(date)"
          >
            <div class="date-header">
              <span class="date-number">{{ date.getDate() }}</span>
              <span v-if="isToday(date)" class="today-indicator">今</span>
              <span v-if="date.getDate() === 1" class="month-indicator">
                {{ date.getMonth() + 1 }}月
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 事件列表 -->
    <div class="events-section">
      <div class="section-header">
        <h3>
          <span v-if="selectedDate">{{ formatSelectedDate(selectedDate) }} 的活动</span>
          <span v-else>本月活动</span>
        </h3>
        <div class="view-toggle">
          <button 
            @click="viewMode = 'month'"
            :class="['view-btn', { active: viewMode === 'month' }]"
          >
            月视图
          </button>
          <button 
            @click="viewMode = 'list'"
            :class="['view-btn', { active: viewMode === 'list' }]"
          >
            列表视图
          </button>
        </div>
      </div>

      <!-- 无活动提示 -->
      <div class="no-events">
        <p>暂无活动安排</p>
        <p v-if="isAdmin" class="prompt-text">管理员可以点击上方按钮创建活动</p>
      </div>
    </div>

    <!-- 活动类型图例 -->
    <div class="event-legend">
      <h4>活动类型</h4>
      <div class="legend-items">
        <div v-for="type in eventTypes" :key="type.id" class="legend-item">
          <span class="legend-color" :style="{ backgroundColor: type.color }"></span>
          <span class="legend-text">{{ type.name }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

// 基础数据
const currentYear = ref(new Date().getFullYear())
const currentMonth = ref(new Date().getMonth() + 1)
const selectedDate = ref(new Date())
const viewMode = ref('month')
const weekDays = ['日', '一', '二', '三', '四', '五', '六']

// 空活动数据
const events = ref([])

// 事件类型定义
const eventTypes = ref([
  { id: 'tech', name: '技术分享', color: '#4CAF50' },
  { id: 'social', name: '社交活动', color: '#2196F3' },
  { id: 'update', name: '版本更新', color: '#FF9800' },
  { id: 'qa', name: '问答活动', color: '#9C27B0' },
  { id: 'meeting', name: '会议', color: '#F44336' },
  { id: 'announcement', name: '公告', color: '#607D8B' }
])

// 权限（这里应该是从store获取）
const isAdmin = ref(false) // 可以根据实际情况设置

// 计算属性
const calendarDates = computed(() => {
  const dates = []
  const firstDay = new Date(currentYear.value, currentMonth.value - 1, 1)
  const lastDay = new Date(currentYear.value, currentMonth.value, 0)
  
  // 添加上个月末尾的几天
  const firstDayOfWeek = firstDay.getDay()
  for (let i = 0; i < firstDayOfWeek; i++) {
    const date = new Date(firstDay)
    date.setDate(date.getDate() - (firstDayOfWeek - i))
    dates.push(date)
  }
  
  // 添加本月的所有天
  for (let i = 1; i <= lastDay.getDate(); i++) {
    dates.push(new Date(currentYear.value, currentMonth.value - 1, i))
  }
  
  // 添加下个月开始的几天，补全6行（42个格子）
  const totalCells = 42
  const remaining = totalCells - dates.length
  for (let i = 1; i <= remaining; i++) {
    const date = new Date(lastDay)
    date.setDate(date.getDate() + i)
    dates.push(date)
  }
  
  return dates
})

// 方法
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

const hasEvents = (date) => {
  return false // 无数据，始终返回false
}

const getCellClass = (date) => {
  const classes = []
  
  if (isCurrentMonth(date)) {
    classes.push('current-month')
  } else {
    classes.push('other-month')
  }
  
  if (isToday(date)) {
    classes.push('today')
  }
  
  if (selectedDate.value && 
      date.getDate() === selectedDate.value.getDate() && 
      date.getMonth() === selectedDate.value.getMonth() &&
      date.getFullYear() === selectedDate.value.getFullYear()) {
    classes.push('selected')
  }
  
  return classes
}

const formatSelectedDate = (date) => {
  return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`
}

// 导航
const prevMonth = () => {
  if (currentMonth.value === 1) {
    currentMonth.value = 12
    currentYear.value -= 1
  } else {
    currentMonth.value -= 1
  }
  selectedDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const nextMonth = () => {
  if (currentMonth.value === 12) {
    currentMonth.value = 1
    currentYear.value += 1
  } else {
    currentMonth.value += 1
  }
  selectedDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const prevYear = () => {
  currentYear.value -= 1
  selectedDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const nextYear = () => {
  currentYear.value += 1
  selectedDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
}

const goToToday = () => {
  const today = new Date()
  currentYear.value = today.getFullYear()
  currentMonth.value = today.getMonth() + 1
  selectedDate.value = today
  viewMode.value = 'month'
}

const selectDate = (date) => {
  selectedDate.value = date
  viewMode.value = 'month'
}

// 管理员功能
const showCreateEvent = () => {
  alert('创建活动功能开发中...')
}

const showEventManager = () => {
  alert('活动管理功能开发中...')
}

// 初始化
onMounted(() => {
  // 这里可以加载活动数据
})
</script>

<style scoped>
.community-calendar {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  color: #333;
  line-height: 1.5;
}

/* 日历头部 */
.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 2px solid #e9ecef;
}

.header-main h1 {
  margin: 0 0 0.5rem 0;
  font-size: 1.8rem;
  color: #2c3e50;
  font-weight: 700;
}

.header-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.9rem;
  color: #6c757d;
}

.stats, .selected-date {
  background: #e9ecef;
  padding: 0.4rem 0.8rem;
  border-radius: 20px;
  font-weight: 500;
}

.action-buttons {
  display: flex;
  gap: 0.75rem;
}

.btn-primary, .btn-secondary {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.6rem 1.2rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.9rem;
}

.btn-primary {
  background: #4CAF50;
  color: white;
}

.btn-primary:hover {
  background: #388e3c;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
}

.btn-secondary {
  background: #2196F3;
  color: white;
}

.btn-secondary:hover {
  background: #1976d2;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(33, 150, 243, 0.3);
}

/* 月份导航 */
.month-navigation {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.nav-btn {
  background: none;
  border: 2px solid #e9ecef;
  width: 40px;
  height: 40px;
  border-radius: 8px;
  font-size: 1.2rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #495057;
  transition: all 0.2s ease;
}

.nav-btn:hover {
  border-color: #4CAF50;
  color: #4CAF50;
  transform: scale(1.1);
}

.current-month {
  display: flex;
  align-items: center;
  gap: 1rem;
  min-width: 200px;
  justify-content: center;
}

.month-year {
  font-size: 1.4rem;
  font-weight: 600;
  color: #2c3e50;
  min-width: 150px;
  text-align: center;
}

.today-btn {
  background: #6c757d;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.2s ease;
}

.today-btn:hover {
  background: #545b62;
  transform: translateY(-1px);
}

/* 日历容器 - 关键修复：确保不超出容器 */
.calendar-container {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  margin-bottom: 2rem;
  border: 1px solid #e9ecef;
  width: 100%;
  box-sizing: border-box;
  overflow: hidden;
}

/* 星期标题 - 修复：确保不超出 */
.week-header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 0.5rem;
  text-align: center;
  font-weight: 600;
  color: #495057;
  padding-bottom: 0.5rem;
  border-bottom: 2px solid #f8f9fa;
  gap: 0;
  width: 100%;
}

.week-day {
  padding: 0.5rem 0;
  font-size: 0.9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 日历网格 - 关键修复：缩小格子尺寸 */
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
  width: 100%;
  box-sizing: border-box;
}

/* 日历单元格 - 关键修复：缩小尺寸 */
.calendar-cell {
  aspect-ratio: 1;
  background: white;
  border: 1px solid #f8f9fa;
  border-radius: 8px;
  padding: 0.5rem;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 0;
  box-sizing: border-box;
  min-width: 0;
}

.calendar-cell:hover {
  border-color: #4CAF50;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.calendar-cell.current-month {
  background: white;
}

.calendar-cell.other-month {
  background: #f8f9fa;
  color: #adb5bd;
}

.calendar-cell.today {
  background: #e8f5e9;
  border-color: #4CAF50;
  font-weight: 600;
}

.calendar-cell.selected {
  background: #e3f2fd;
  border-color: #2196F3;
  box-shadow: 0 0 0 2px #2196F3;
}

.date-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.25rem;
  min-height: 1.5rem;
}

.date-number {
  font-size: 0.9rem;
  font-weight: 600;
  color: inherit;
}

.today-indicator {
  background: #f44336;
  color: white;
  font-size: 0.6rem;
  padding: 0.1rem 0.3rem;
  border-radius: 3px;
  font-weight: 600;
  line-height: 1;
}

.month-indicator {
  font-size: 0.6rem;
  color: #6c757d;
  background: #e9ecef;
  padding: 0.1rem 0.3rem;
  border-radius: 3px;
  line-height: 1;
}

/* 活动区域 */
.events-section {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border: 1px solid #e9ecef;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 2px solid #f8f9fa;
}

.section-header h3 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.4rem;
  font-weight: 600;
}

.view-toggle {
  display: flex;
  gap: 0.5rem;
  background: #f8f9fa;
  padding: 0.25rem;
  border-radius: 8px;
}

.view-btn {
  padding: 0.5rem 1rem;
  background: none;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  color: #6c757d;
  transition: all 0.2s ease;
}

.view-btn.active {
  background: white;
  color: #2196F3;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.view-btn:hover:not(.active) {
  background: #e9ecef;
  color: #495057;
}

/* 无活动提示 */
.no-events {
  text-align: center;
  padding: 3rem 2rem;
  color: #6c757d;
  background: #f8f9fa;
  border-radius: 8px;
  border: 2px dashed #dee2e6;
}

.no-events p {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
}

.prompt-text {
  font-size: 0.9rem;
  color: #adb5bd;
}

/* 图例 */
.event-legend {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border: 1px solid #e9ecef;
}

.event-legend h4 {
  margin: 0 0 1rem 0;
  color: #2c3e50;
  font-size: 1.2rem;
  font-weight: 600;
}

.legend-items {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 3px;
  flex-shrink: 0;
}

.legend-text {
  font-size: 0.85rem;
  color: #495057;
}

/* 响应式设计 - 针对小屏幕优化 */
@media (max-width: 768px) {
  .community-calendar {
    padding: 1rem;
  }
  
  .calendar-header {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .header-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .action-buttons {
    align-self: stretch;
  }
  
  .btn-primary, .btn-secondary {
    flex: 1;
    justify-content: center;
    padding: 0.5rem 1rem;
    font-size: 0.85rem;
  }
  
  .month-navigation {
    flex-wrap: wrap;
    gap: 0.5rem;
    padding: 0.75rem;
  }
  
  .month-year {
    min-width: 120px;
    font-size: 1.2rem;
  }
  
  .calendar-container {
    padding: 1rem;
  }
  
  .week-header {
    margin-bottom: 0.25rem;
  }
  
  .week-day {
    padding: 0.4rem 0;
    font-size: 0.8rem;
  }
  
  .calendar-grid {
    gap: 2px;
  }
  
  .calendar-cell {
    padding: 0.4rem;
    border-radius: 6px;
  }
  
  .date-header {
    margin-bottom: 0.2rem;
  }
  
  .date-number {
    font-size: 0.8rem;
  }
  
  .today-indicator, .month-indicator {
    font-size: 0.5rem;
    padding: 0.1rem 0.2rem;
  }
  
  .section-header {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .view-toggle {
    align-self: center;
  }
  
  .view-btn {
    padding: 0.4rem 0.8rem;
    font-size: 0.85rem;
  }
}

/* 超小屏幕优化 */
@media (max-width: 480px) {
  .community-calendar {
    padding: 0.5rem;
  }
  
  .calendar-container {
    padding: 0.75rem;
  }
  
  .month-navigation {
    gap: 0.25rem;
  }
  
  .nav-btn {
    width: 35px;
    height: 35px;
    font-size: 1rem;
  }
  
  .month-year {
    min-width: 100px;
    font-size: 1rem;
  }
  
  .today-btn {
    padding: 0.4rem 0.8rem;
    font-size: 0.8rem;
  }
  
  .week-day {
    font-size: 0.75rem;
    padding: 0.3rem 0;
  }
  
  .calendar-cell {
    padding: 0.3rem;
    border-radius: 4px;
  }
  
  .date-number {
    font-size: 0.75rem;
  }
}
</style>