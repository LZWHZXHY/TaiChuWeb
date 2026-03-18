<template>
  <div class="gantt-wrapper">
    <div class="gantt-header">
      <div class="header-info">
        <h2 class="title">SYNC_TRACK // 战术全景路线图</h2>
        <p class="subtitle">拖拽平移画布 | 滚轮无极缩放自动切换 日/周/月/年 视图</p>
      </div>
      <div class="header-actions">
        <button class="btn-outline focus-today" @click="scrollToToday">
          <span class="pulse-dot"></span> 定位至今日
        </button>
        <button class="btn-primary" @click="showAddModal = true">
          <span class="icon">+</span> 下达指令
        </button>
      </div>
    </div>

    <div class="gantt-container">
      
      <div class="gantt-sidebar">
        <div class="sidebar-header">
          <span class="col-title">任务流 (TASK_STREAM)</span>
        </div>
        <div class="task-list">
          <div class="task-list-item" v-for="task in tasks" :key="task.id">
            <div class="status-dot" :class="task.status"></div>
            <div class="task-info">
              <span class="task-name" :title="task.title">{{ task.title }}</span>
              <div class="task-meta-row">
                <span class="t-assignee">{{ task.assignee }}</span>
                <span class="t-date-range">
                  {{ formatShortDate(task.startDate) }} → {{ formatShortDate(task.endDate) }}
                  <span class="t-duration">{{ getDurationDays(task.startDate, task.endDate) }}天</span>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div 
        class="gantt-timeline" 
        ref="timelineContainer"
        :class="[zoomMode, { 'is-dragging': isDragging }]"
        :style="{ '--cell-width': cellWidth + 'px' }"
        @mousedown="startDrag"
        @mousemove="onDrag"
        @mouseup="stopDrag"
        @mouseleave="stopDrag"
        @wheel.prevent="handleZoom"
      >
        
        <div class="timeline-dates">
          <div 
            class="date-cell" 
            v-for="day in timelineDays" 
            :key="day.getTime()"
            :class="{ 'is-today-cell': isToday(day) }"
          >
            <div class="date-top">
              <span 
                class="sticky-label" 
                v-if="(zoomMode === 'zoom-month' || zoomMode === 'zoom-year') && (isFirstOfYear(day) || day === timelineDays[0])"
              >
                {{ day.getFullYear() }}年
              </span>
              <span 
                class="sticky-label" 
                v-else-if="(zoomMode === 'zoom-day' || zoomMode === 'zoom-week') && (isFirstOfMonth(day) || day === timelineDays[0])"
              >
                {{ getYearMonthStr(day) }}
              </span>
            </div>

            <div class="date-bottom">
              <span class="day-text center" v-if="zoomMode === 'zoom-day'">{{ day.getDate() }}</span>
              <span class="day-text left-anchor" v-else-if="zoomMode === 'zoom-week' && isMonday(day)">{{ day.getDate() }}日</span>
              <span class="month-text left-anchor" v-else-if="zoomMode === 'zoom-month' && isFirstOfMonth(day)">{{ day.getMonth() + 1 }}月</span>
            </div>
          </div>
        </div>

        <div class="timeline-tracks">
          <div class="grid-bg">
            <div 
              class="grid-col" 
              v-for="day in timelineDays" 
              :key="'grid-'+day.getTime()" 
              :class="{ 
                'is-today-col': isToday(day),
                'is-monday-col': isMonday(day),
                'is-first-day-col': isFirstOfMonth(day),
                'is-first-of-year-col': isFirstOfYear(day)
              }"
            ></div>
          </div>

          <div class="track-row" v-for="task in tasks" :key="'track-'+task.id">
            <div 
              v-if="isTaskVisible(task)"
              class="task-bar"
              :class="task.status"
              :style="getTaskStyle(task)"
              :title="`${task.title}\n周期: ${formatShortDate(task.startDate)} ~ ${formatShortDate(task.endDate)}`"
            >
              <div class="bar-progress" :style="{ width: task.progress + '%' }"></div>
              <span class="bar-label" v-show="cellWidth * getDurationDays(task.startDate, task.endDate) > 60">
                {{ task.title }}
              </span>
            </div>
          </div>
        </div>
        
      </div>
    </div>

    <div v-if="showAddModal" class="modal-overlay" @click.self="showAddModal = false">
      <div class="modal-content cyber-modal">
        <h3>// 下达新任务指令</h3>
        <form @submit.prevent="submitNewTask">
          <div class="form-group">
            <label>任务代号</label>
            <input type="text" v-model="newTask.title" required />
          </div>
          <div class="form-row">
            <div class="form-group half">
              <label>开始时间</label>
              <input type="date" v-model="newTask.startDate" required />
            </div>
            <div class="form-group half">
              <label>结束时间</label>
              <input type="date" v-model="newTask.endDate" required />
            </div>
          </div>
          <div class="form-actions">
            <button type="button" class="modal-btn cancel" @click="showAddModal = false">取消</button>
            <button type="submit" class="modal-btn submit">前往看板写入</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import apiClient from '@/utils/api' 

const props = defineProps<{
  projectId: string | number
}>()

interface Task {
  id: number; title: string; assignee: string;
  startDate: string; endDate: string; 
  status: 'todo' | 'in-progress' | 'done'; progress: number;
}

const tasks = ref<Task[]>([]) 
const isLoading = ref(true)

const fetchTasks = async () => {
  isLoading.value = true
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/tasks`)
    tasks.value = res.data.map((t: any) => {
      
      // 🔥 修复：适配后端真实返回的 AssigneeNames 字符串数组
      let assigneeName = '未分配'
      const names = t.AssigneeNames || t.assigneeNames
      if (names && names.length > 0) {
        // 如果有多个负责人，用逗号拼接，例如: @腾蛇, @照烧鸡排
        assigneeName = '@' + names.join(', @')
      }

      // 解析状态 (匹配前端的 CSS 类名)
      let s: 'todo' | 'in-progress' | 'done' = 'todo'
      const statusStr = (t.status || t.Status || '').toLowerCase()
      if (statusStr.includes('progress') || statusStr.includes('进行')) s = 'in-progress'
      else if (statusStr === 'done' || statusStr.includes('归档') || statusStr.includes('完成')) s = 'done'

      // 解析时间 
      const start = t.createdAt || t.CreatedAt || new Date().toISOString()
      const end = t.dueDate || t.DueDate || new Date().toISOString()

      return {
        id: t.id || t.Id, 
        title: t.title || t.Title, 
        assignee: assigneeName, // ✅ 赋值正确的负责人
        startDate: start, 
        endDate: end, 
        status: s,
        progress: s === 'in-progress' ? 50 : (s === 'done' ? 100 : 0)
      }
    })
  } catch (error) {
    console.error('获取同步轨道任务失败', error)
  } finally {
    isLoading.value = false
    nextTick(() => scrollToToday())
  }
}

// ================= 时间线底层引擎 =================
const timelineContainer = ref<HTMLElement | null>(null)
const cellWidth = ref(30) // 默认宽度

// 生成轨道 (过去半年 + 未来两年)
const PAST_DAYS = 180
const TOTAL_DAYS = 900
const today = new Date()
today.setHours(0,0,0,0)

const timelineDays = computed(() => {
  const days = []
  const baseDate = new Date(today)
  baseDate.setDate(baseDate.getDate() - PAST_DAYS)

  for (let i = 0; i < TOTAL_DAYS; i++) {
    const d = new Date(baseDate)
    d.setDate(baseDate.getDate() + i)
    days.push(d)
  }
  return days
})

const timelineStartDate = computed(() => timelineDays.value[0])
const timelineEndDate = computed(() => timelineDays.value[timelineDays.value.length - 1])

// 🔥 优化：更细腻的视图切换阈值 🔥
const zoomMode = computed(() => {
  if (cellWidth.value >= 20) return 'zoom-day'     // > 20px 显示天
  if (cellWidth.value >= 4) return 'zoom-week'     // 4px ~ 20px 显示周
  if (cellWidth.value >= 1.5) return 'zoom-month'  // 1.5px ~ 4px 显示月
  return 'zoom-year'                               // < 1.5px 仅显示年
})

// 无极缩放计算
const handleZoom = (e: WheelEvent) => {
  const zoomFactor = 0.15 
  const delta = e.deltaY < 0 ? 1 : -1 
  
  let newWidth = cellWidth.value * (1 + delta * zoomFactor)
  newWidth = Math.max(0.5, Math.min(newWidth, 120)) // 极限范围
  
  if (timelineContainer.value) {
    const container = timelineContainer.value
    const mouseX = e.clientX - container.getBoundingClientRect().left
    const scrollX = container.scrollLeft
    const targetIndex = (scrollX + mouseX) / cellWidth.value
    
    cellWidth.value = newWidth
    
    nextTick(() => {
      container.scrollLeft = targetIndex * newWidth - mouseX
    })
  } else {
    cellWidth.value = newWidth
  }
}

// 拖拽平移
const isDragging = ref(false)
const startX = ref(0)
const startScrollLeft = ref(0)

const startDrag = (e: MouseEvent) => {
  if (!timelineContainer.value) return
  isDragging.value = true
  startX.value = e.pageX - timelineContainer.value.offsetLeft
  startScrollLeft.value = timelineContainer.value.scrollLeft
}
const onDrag = (e: MouseEvent) => {
  if (!isDragging.value || !timelineContainer.value) return
  e.preventDefault()
  const x = e.pageX - timelineContainer.value.offsetLeft
  const walk = (x - startX.value) * 1.5 
  timelineContainer.value.scrollLeft = startScrollLeft.value - walk
}
const stopDrag = () => { isDragging.value = false }

const scrollToToday = () => {
  if (!timelineContainer.value) return
  const targetX = PAST_DAYS * cellWidth.value
  const offset = timelineContainer.value.clientWidth / 2
  timelineContainer.value.scrollTo({
    left: targetX - offset,
    behavior: 'smooth'
  })
}

// ================= 日期与运算工具 =================
const isToday = (d: Date) => d.getTime() === today.getTime()
const isMonday = (d: Date) => d.getDay() === 1
const isFirstOfMonth = (d: Date) => d.getDate() === 1
const isFirstOfYear = (d: Date) => d.getMonth() === 0 && d.getDate() === 1

const getYearMonthStr = (d: Date) => `${d.getFullYear()}年${d.getMonth() + 1}月`

const formatShortDate = (dateStr: string) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getMonth() + 1}月${d.getDate()}日`
}

const getDurationDays = (startStr: string, endStr: string) => {
  if (!startStr || !endStr) return 0
  const start = new Date(startStr).getTime()
  const end = new Date(endStr).getTime()
  const days = Math.floor((end - start) / 86400000) + 1
  return days > 0 ? days : 1 
}

const isTaskVisible = (task: Task) => {
  if (!task.startDate || !task.endDate) return false
  const tEnd = new Date(task.endDate).getTime()
  const tStart = new Date(task.startDate).getTime()
  return tEnd >= timelineStartDate.value.getTime() && tStart <= timelineEndDate.value.getTime()
}

// 计算任务条宽度和位置
const getTaskStyle = (task: Task) => {
  if (!task.startDate || !task.endDate) return { display: 'none' }

  const tStart = new Date(task.startDate)
  tStart.setHours(0,0,0,0)
  const tEnd = new Date(task.endDate)
  tEnd.setHours(0,0,0,0)
  
  let offsetDays = Math.floor((tStart.getTime() - timelineStartDate.value.getTime()) / 86400000)
  let duration = Math.floor((tEnd.getTime() - tStart.getTime()) / 86400000) + 1
  
  if (offsetDays < 0) {
    duration += offsetDays
    offsetDays = 0
  }
  return {
    left: `calc(var(--cell-width) * ${offsetDays})`,
    width: `calc(var(--cell-width) * ${duration})`
  }
}

const showAddModal = ref(false)
const newTask = ref({ title: '', startDate: '', endDate: '' })
const submitNewTask = () => {
  alert('功能联调中：建议前往 Kanban Board 视图创建任务')
  showAddModal.value = false
}

onMounted(() => { fetchTasks() })
</script>

<style scoped>
.gantt-wrapper {
  padding: 24px 32px; height: 100%; display: flex; flex-direction: column;
  box-sizing: border-box; background: #F4F5F7; font-family: 'Inter', 'JetBrains Mono', sans-serif;
  user-select: none; 
}

.gantt-header {
  display: flex; justify-content: space-between; align-items: flex-end;
  margin-bottom: 20px; padding-bottom: 16px; border-bottom: 2px solid #111; flex-shrink: 0;
}
.title { font-size: 20px; font-weight: 900; color: #111; margin: 0 0 8px 0; }
.subtitle { font-size: 12px; color: #666; font-weight: 700; margin: 0; }
.header-actions { display: flex; gap: 12px; }

.btn-outline { display: flex; align-items: center; gap: 6px; background: transparent; border: 2px solid #111; padding: 6px 12px; font-weight: 800; font-size: 12px; cursor: pointer; transition: 0.2s; }
.btn-outline:hover { background: #eee; }
.btn-primary { background: #111; color: #fff; border: 2px solid #111; padding: 6px 16px; font-weight: 800; font-size: 12px; cursor: pointer; transition: 0.2s; }
.btn-primary:hover { background: #00A3FF; border-color: #00A3FF; }
.pulse-dot { width: 8px; height: 8px; background: #FF8B00; border-radius: 50%; box-shadow: 0 0 8px #FF8B00; animation: pulseGlow 1.5s infinite; }

.gantt-container {
  flex: 1; display: flex; background: #fff; border: 2px solid #111;
  box-shadow: 6px 6px 0 rgba(0,0,0,0.05); overflow: hidden; 
}

/* ================= 左侧看板 ================= */
.gantt-sidebar {
  width: 320px; border-right: 2px solid #111; display: flex; flex-direction: column;
  flex-shrink: 0; background: #FAFBFC; z-index: 20; position: relative;
  box-shadow: 4px 0 16px rgba(0,0,0,0.03);
}
.sidebar-header { height: 50px; display: flex; align-items: center; padding: 0 16px; border-bottom: 2px solid #111; background: #fff; flex-shrink: 0; }
.col-title { font-size: 12px; font-weight: 900; color: #111; }

.task-list { flex: 1; overflow-y: auto; overflow-x: hidden; }
.task-list::-webkit-scrollbar { display: none; }

.task-list-item {
  height: 60px; display: flex; align-items: center; padding: 0 16px;
  border-bottom: 1px solid #eee; box-sizing: border-box; background: #fff;
}
.status-dot { width: 8px; height: 8px; border-radius: 50%; margin-right: 12px; flex-shrink: 0; }
.status-dot.todo { background: #DFE1E6; border: 2px solid #999; }
.status-dot.in-progress { background: #00A3FF; box-shadow: 0 0 6px #00A3FF; }
.status-dot.done { background: #36B37E; }

.task-info { flex: 1; display: flex; flex-direction: column; overflow: hidden; justify-content: center; gap: 4px; }
.task-name { font-size: 13px; font-weight: 700; color: #111; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; line-height: 1.2; }
.task-meta-row { display: flex; justify-content: space-between; align-items: center; font-size: 10px; color: #666; font-family: 'JetBrains Mono', monospace; }
.t-assignee { font-weight: 700; color: #00A3FF; }
.t-date-range { display: flex; align-items: center; gap: 4px; color: #888; }
.t-duration { background: #DFE1E6; color: #172B4D; padding: 1px 4px; border-radius: 2px; font-weight: 800; margin-left: 2px; }

/* ================= 右侧时间线 (核心修复) ================= */
.gantt-timeline {
  flex: 1; overflow-x: hidden; overflow-y: auto; 
  display: flex; flex-direction: column; position: relative; cursor: grab; 
}
.gantt-timeline.is-dragging { cursor: grabbing; } 
.gantt-timeline::-webkit-scrollbar { width: 8px; height: 8px; }
.gantt-timeline::-webkit-scrollbar-track { background: #f0f0f0; }
.gantt-timeline::-webkit-scrollbar-thumb { background: #ccc; border-radius: 4px; }

/* 🔥 修复：双层表头结构防止挤压 🔥 */
.timeline-dates {
  display: flex; height: 50px; min-height: 50px; flex-shrink: 0; /* 必须加 shrink: 0 防止被压扁 */
  border-bottom: 2px solid #111; background: #fff;
  position: sticky; top: 0; z-index: 10; box-sizing: border-box;
}
.date-cell {
  width: var(--cell-width); flex-shrink: 0; box-sizing: border-box; 
  display: flex; flex-direction: column; border-right: 1px solid transparent; 
}

/* 上半部分：年月标签 */
.date-top {
  height: 24px; position: relative; border-bottom: 1px solid #f0f0f0;
}
.sticky-label {
  position: absolute; left: 4px; top: 0; line-height: 24px;
  font-size: 11px; font-weight: 800; color: #111;
  white-space: nowrap; z-index: 5; /* 不换行，永远完整展示 */
}

/* 下半部分：日期数字 */
.date-bottom {
  height: 25px; position: relative;
}
.day-text, .month-text {
  position: absolute; top: 0; line-height: 25px;
  font-size: 11px; font-weight: 700; color: #555;
}
.center { width: 100%; text-align: center; left: 0; }
.left-anchor { left: 4px; white-space: nowrap; }

.date-cell.is-today-cell .date-bottom { background: #FFF0B3; border-bottom: 3px solid #FF8B00; }
.date-cell.is-today-cell .day-text { color: #BF2600; font-weight: 900; }

/* 网格与降级线 */
.timeline-tracks { position: relative; min-height: 100%; }
.grid-bg { position: absolute; top: 0; left: 0; right: 0; bottom: 0; display: flex; z-index: 0; pointer-events: none; }
.grid-col { width: var(--cell-width); flex-shrink: 0; box-sizing: border-box; border-right: 1px solid transparent; }

.zoom-day .date-cell { border-right-color: #eee; }
.zoom-day .grid-col { border-right: 1px dashed #f5f5f5; }

.zoom-week .grid-col.is-monday-col { border-left: 1px dashed #ddd; }
.zoom-week .date-cell.is-monday { border-left: 1px dashed #ddd; }

.zoom-month .grid-col.is-first-day-col { border-left: 1px solid #ccc; }
.zoom-month .date-cell.is-first-day { border-left: 1px solid #ccc; }

.zoom-year .grid-col.is-first-of-year-col { border-left: 2px solid #111; }
.zoom-year .date-cell.is-first-of-year { border-left: 2px solid #111; }

.grid-col.is-today-col { background: rgba(255, 139, 0, 0.05); border-left: 2px solid #FF8B00; position: relative; z-index: 5;}

/* 轨道进度条 */
.track-row { height: 60px; border-bottom: 1px solid #f5f5f5; position: relative; box-sizing: border-box; }
.track-row:hover { background: rgba(0, 163, 255, 0.02); }

.task-bar {
  position: absolute; top: 16px; height: 28px; border-radius: 4px;
  background: #DFE1E6; z-index: 6; cursor: pointer; overflow: hidden;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: transform 0.1s, box-shadow 0.1s;
}
.task-bar:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(0,0,0,0.15); z-index: 7; }

.task-bar.todo { background: #EBECF0; border: 1px solid #C1C7D0; }
.task-bar.in-progress { background: #DEEBFF; border: 1px solid #00A3FF; }
.task-bar.done { background: #E3FCEF; border: 1px solid #36B37E; }

.bar-progress { position: absolute; top: 0; left: 0; bottom: 0; background: rgba(0,0,0,0.1); z-index: 1; }
.task-bar.in-progress .bar-progress { background: rgba(0, 163, 255, 0.2); }
.task-bar.done .bar-progress { background: rgba(54, 179, 126, 0.3); }

.bar-label { position: relative; z-index: 2; font-size: 11px; font-weight: 700; color: #111; padding: 0 8px; line-height: 28px; white-space: nowrap; text-shadow: 0 0 2px rgba(255,255,255,0.8); }

@keyframes pulseGlow { 0% { transform: scale(0.8); opacity: 0.5; } 50% { transform: scale(1.4); opacity: 1; } 100% { transform: scale(0.8); opacity: 0.5; } }

/* 弹窗样式 */
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(2px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content.cyber-modal { background: #fff; padding: 32px; border: 2px solid #111; width: 100%; max-width: 460px; box-shadow: 12px 12px 0 #00A3FF; }
.modal-content h3 { margin-top: 0; font-size: 18px; font-weight: 800; border-left: 4px solid #111; padding-left: 12px; margin-bottom: 24px; }
.form-row { display: flex; gap: 16px; }
.form-group { margin-bottom: 20px; }
.form-group.half { flex: 1; }
.form-group label { display: block; font-size: 12px; font-weight: 700; margin-bottom: 6px; }
.form-group input { width: 100%; padding: 10px; border: 2px solid #111; font-family: inherit; font-size: 14px; outline: none; box-sizing: border-box; }
.form-group input:focus { border-color: #00A3FF; }
.form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 24px; padding-top: 16px; border-top: 1px solid #eee; }
.modal-btn { padding: 10px 20px; font-weight: 700; cursor: pointer; border: 2px solid #111; background: transparent; font-size: 12px; }
.modal-btn.submit { background: #111; color: #fff; }
.modal-btn.submit:hover { background: #00A3FF; border-color: #00A3FF; }
</style>