<template>
  <node-view-wrapper class="lingmai-calendar-wrapper" contenteditable="false">
    <div class="calendar-header">
      <button class="nav-btn" @click="changeMonth(-1)">◀ 上个月</button>
      <span class="month-title">📅 {{ currentYear }} 年 {{ currentMonth + 1 }} 月</span>
      <button class="nav-btn" @click="changeMonth(1)">下个月 ▶</button>
    </div>

    <div class="calendar-grid">
      <div class="weekday" v-for="day in ['日', '一', '二', '三', '四', '五', '六']" :key="day">{{ day }}</div>
      
      <div 
        v-for="(dateObj, index) in calendarDays" 
        :key="index"
        class="day-cell"
        :class="{ 
          'is-other-month': !isCurrentMonth(dateObj), 
          'is-today': isToday(dateObj),
          'is-dragover': dragOverDateKey === formatDateKey(dateObj) 
        }"
        @dragover.prevent="handleDragOver($event, dateObj)"
        @dragleave.prevent="handleDragLeave($event, dateObj)"
        @drop.prevent="handleDrop($event, dateObj)"
      >
        <div class="day-content">
          <div class="day-header">
            <span class="day-number">
              {{ dateObj.getDate() === 1 ? (dateObj.getMonth() + 1) + '月' : '' }}{{ dateObj.getDate() }}
            </span>
            <button class="add-btn" @click="promptAddEvent(dateObj)" title="添加事项">+</button>
          </div>
          
          <div class="day-events">
            <div 
              v-for="evt in getEvents(dateObj)" 
              :key="evt.id" 
              class="event-item"
              :class="{ 
                'is-link': evt.linkId, 
                'is-todo': evt.isTodo,
                'is-completed': evt.completed,
                'is-dragging': draggedItemInfo?.event.id === evt.id 
              }"
              draggable="true"
              @dragstart="handleDragStart($event, dateObj, evt)"
              @dragend="handleDragEnd"
              @click="handleEventClick(evt)"
            >
              <input 
                v-if="evt.isTodo" 
                type="checkbox" 
                class="todo-checkbox"
                :checked="evt.completed"
                @click.stop="toggleTodo(dateObj, evt)"
              />
              
              <span v-if="evt.linkId && !evt.isTodo" class="icon">🔗</span>
              <span class="text">{{ evt.text }}</span>
              <button class="del-btn" @click.stop="removeEvent(dateObj, evt.id)">×</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showEventEditor" class="modal-overlay" @click="closeEditor">
      <div class="modal-content" @click.stop>
        <h3>➕ 添加日程或关联笔记</h3>
        <p class="modal-date">{{ formatDateKey(selectedDateObj) }}</p>
        
        <input 
          v-model="newEventText" 
          class="event-input"
          placeholder="输入普通文本..." 
          @keyup.enter="saveEvent" 
        />

        <label class="todo-toggle-label">
          <input type="checkbox" v-model="isNewEventTodo" />
          ☑️ 将此项设为待办任务
        </label>
        
        <div class="divider">或者关联已有笔记</div>
        
        <input 
          v-model="searchQuery" 
          class="search-input"
          placeholder="🔍 搜索笔记进行双链..." 
          @input="handleSearch" 
        />
        <div class="search-results" v-if="searchResults.length">
          <div 
            v-for="n in searchResults" 
            :key="n.id" 
            class="search-item"
            @click="selectSearchItem(n)"
          >
            📄 {{ n.title }} <span class="space-tag" v-if="n.spaceName">🌌 {{ n.spaceName }}</span>
          </div>
        </div>

        <div class="selected-note" v-if="selectedNote">
          已选择链接: <strong>{{ selectedNote.title }}</strong>
          <button @click="selectedNote = null">清除</button>
        </div>

        <div class="modal-actions">
          <button class="btn-cancel" @click="closeEditor">取消</button>
          <button class="btn-save" @click="saveEvent">保存</button>
        </div>
      </div>
    </div>
  </node-view-wrapper>
</template>

<script setup>
import { ref, computed } from 'vue'
import { nodeViewProps, NodeViewWrapper } from '@tiptap/vue-3'
import apiClient from '@/utils/api'

const props = defineProps(nodeViewProps)

const currentYear = ref(new Date().getFullYear())
const currentMonth = ref(new Date().getMonth())

const changeMonth = (delta) => {
  let newMonth = currentMonth.value + delta
  let newYear = currentYear.value
  if (newMonth > 11) { newMonth = 0; newYear++ }
  if (newMonth < 0) { newMonth = 11; newYear-- }
  currentMonth.value = newMonth
  currentYear.value = newYear
}

// ==========================================
// 🔥 核心魔法：固定 42 天网格算法
// ==========================================
const calendarDays = computed(() => {
  const days = []
  // 1. 获取当前月的第一天
  const firstDayOfMonth = new Date(currentYear.value, currentMonth.value, 1)
  // 2. 看看这一天是星期几 (0是周日, 1是周一)
  const startDayOfWeek = firstDayOfMonth.getDay()
  
  // 3. 往前倒推，找到日历面板第一格的真实日期（包含上个月的尾巴）
  const startDate = new Date(currentYear.value, currentMonth.value, 1 - startDayOfWeek)

  // 4. 永远生成 42 天 (6周)，保证日历高度和跨月无缝衔接
  for (let i = 0; i < 42; i++) {
    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i)
    days.push(currentDate)
  }
  
  return days
})

// 判断是否是当前月的日子（用来给非本月的格子做半透明处理）
const isCurrentMonth = (dateObj) => {
  return dateObj.getMonth() === currentMonth.value && dateObj.getFullYear() === currentYear.value
}
// ==========================================

const isToday = (dateObj) => {
  if (!dateObj) return false
  const today = new Date()
  return dateObj.getDate() === today.getDate() && dateObj.getMonth() === today.getMonth() && dateObj.getFullYear() === today.getFullYear()
}

const formatDateKey = (dateObj) => {
  if (!dateObj) return ''
  const y = dateObj.getFullYear()
  const m = String(dateObj.getMonth() + 1).padStart(2, '0')
  const d = String(dateObj.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

const getEvents = (dateObj) => {
  if (!dateObj) return []
  const key = formatDateKey(dateObj)
  return props.node.attrs.dayData[key] || []
}

// 拖拽核心逻辑 (保持不变)
const draggedItemInfo = ref(null)
const dragOverDateKey = ref(null)

const handleDragStart = (e, dateObj, evt) => {
  draggedItemInfo.value = { sourceDateKey: formatDateKey(dateObj), event: evt }
  e.dataTransfer.effectAllowed = 'move'
  e.dataTransfer.setData('text/plain', evt.id)
}

const handleDragOver = (e, dateObj) => {
  if (!dateObj || !draggedItemInfo.value) return
  e.dataTransfer.dropEffect = 'move'
  dragOverDateKey.value = formatDateKey(dateObj)
}

const handleDragLeave = (e, dateObj) => {
  if (!dateObj) return
  if (dragOverDateKey.value === formatDateKey(dateObj)) {
    dragOverDateKey.value = null
  }
}

const handleDrop = (e, targetDateObj) => {
  if (!targetDateObj || !draggedItemInfo.value) {
    dragOverDateKey.value = null; draggedItemInfo.value = null; return
  }

  const targetDateKey = formatDateKey(targetDateObj)
  const sourceDateKey = draggedItemInfo.value.sourceDateKey
  const droppedEvent = draggedItemInfo.value.event

  dragOverDateKey.value = null
  if (sourceDateKey === targetDateKey) { draggedItemInfo.value = null; return }

  const currentData = JSON.parse(JSON.stringify(props.node.attrs.dayData))
  if (currentData[sourceDateKey]) {
    currentData[sourceDateKey] = currentData[sourceDateKey].filter(item => item.id !== droppedEvent.id)
  }
  if (!currentData[targetDateKey]) currentData[targetDateKey] = []
  
  currentData[targetDateKey].push(droppedEvent)
  props.updateAttributes({ dayData: currentData })
  draggedItemInfo.value = null
}

const handleDragEnd = () => { draggedItemInfo.value = null; dragOverDateKey.value = null }

// 添加事项逻辑 (保持不变)
const showEventEditor = ref(false)
const selectedDateObj = ref(null)
const newEventText = ref('')
const searchQuery = ref('')
const searchResults = ref([])
const selectedNote = ref(null)
const isNewEventTodo = ref(false)

const promptAddEvent = (dateObj) => {
  selectedDateObj.value = dateObj; newEventText.value = ''; searchQuery.value = ''
  selectedNote.value = null; searchResults.value = []; isNewEventTodo.value = false 
  showEventEditor.value = true
}

const closeEditor = () => { showEventEditor.value = false }

let searchTimer = null
const handleSearch = (e) => {
  const query = e.target.value
  if (!query) { searchResults.value = []; return }
  clearTimeout(searchTimer)
  searchTimer = setTimeout(async () => {
    try {
      const res = await apiClient.get(`/Notes/search/global?query=${encodeURIComponent(query)}`)
      searchResults.value = res.data.slice(0, 5) 
    } catch (e) { console.error(e) }
  }, 300)
}

const selectSearchItem = (note) => { selectedNote.value = note; searchResults.value = []; searchQuery.value = '' }

const saveEvent = () => {
  if (!newEventText.value.trim() && !selectedNote.value) return
  const key = formatDateKey(selectedDateObj.value)
  const currentData = JSON.parse(JSON.stringify(props.node.attrs.dayData))
  if (!currentData[key]) currentData[key] = []
  
  currentData[key].push({
    id: Math.random().toString(36).substring(2, 9),
    text: newEventText.value || (selectedNote.value ? selectedNote.value.title : '未命名'),
    linkId: selectedNote.value ? selectedNote.value.id : null,
    isTodo: isNewEventTodo.value, completed: false
  })
  props.updateAttributes({ dayData: currentData }); closeEditor()
}

const removeEvent = (dateObj, eventId) => {
  const key = formatDateKey(dateObj)
  const currentData = JSON.parse(JSON.stringify(props.node.attrs.dayData))
  if (currentData[key]) {
    currentData[key] = currentData[key].filter(e => e.id !== eventId)
    props.updateAttributes({ dayData: currentData })
  }
}

const toggleTodo = (dateObj, evt) => {
  const key = formatDateKey(dateObj)
  const currentData = JSON.parse(JSON.stringify(props.node.attrs.dayData))
  const targetEvent = currentData[key].find(e => e.id === evt.id)
  if (targetEvent) {
    targetEvent.completed = !targetEvent.completed; props.updateAttributes({ dayData: currentData })
  }
}

const handleEventClick = (evt) => {
  if (evt.linkId) window.dispatchEvent(new CustomEvent('navigate-note', { detail: evt.linkId }))
}
</script>

<style scoped>
.lingmai-calendar-wrapper { margin: 24px 0; border: 1px solid #e2e8f0; border-radius: 12px; background: #ffffff; box-shadow: 0 4px 12px rgba(0,0,0,0.02); overflow: hidden; }
.calendar-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 24px; background: #f8fafc; border-bottom: 1px solid #e2e8f0; }
.month-title { font-size: 18px; font-weight: bold; color: #1e293b; }
.nav-btn { background: none; border: none; color: #64748b; cursor: pointer; font-weight: 500; }
.nav-btn:hover { color: #2383e2; }

.calendar-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 1px; background: #e2e8f0; }
.weekday { background: #ffffff; text-align: center; padding: 10px 0; font-size: 13px; font-weight: 600; color: #94a3b8; }

.day-cell { 
  background: #ffffff; min-height: 100px; padding: 8px; 
  transition: background-color 0.2s ease;
}

/* 🔥 新增：如果是非本月的格子，让它变得柔和且半透明，突出主次 */
.day-cell.is-other-month {
  background: #f8fafc;
}
.day-cell.is-other-month .day-number {
  color: #cbd5e1;
}
.day-cell.is-other-month .event-item {
  opacity: 0.7; /* 让上/下个月的事项稍微微弱一点 */
}

.day-cell.is-today .day-number { background: #2383e2; color: white; border-radius: 50%; width: 24px; height: 24px; display: inline-flex; justify-content: center; align-items: center; }
.day-cell.is-dragover { background-color: #eff6ff !important; box-shadow: inset 0 0 0 2px #3b82f6; }

.day-header { display: flex; justify-content: space-between; margin-bottom: 8px; align-items: center; }
/* 优化了字体大小，容纳 "5月1日" 这种字样 */
.day-number { font-size: 13px; color: #475569; font-weight: 600; }
.add-btn { opacity: 0; background: none; border: none; color: #94a3b8; cursor: pointer; }
.day-cell:hover .add-btn { opacity: 1; }
.add-btn:hover { color: #2383e2; }

.day-events { display: flex; flex-direction: column; gap: 4px; }
.event-item { font-size: 12px; padding: 4px 6px; border-radius: 4px; background: #f1f5f9; color: #334155; position: relative; cursor: grab; display: flex; align-items: center; transition: opacity 0.2s; }
.event-item:active { cursor: grabbing; }
.event-item.is-dragging { opacity: 0.4; background: #cbd5e1; }
.todo-checkbox { margin-right: 6px; cursor: pointer; accent-color: #2383e2; }
.event-item.is-completed { background: transparent; opacity: 0.6; }
.event-item.is-completed .text { text-decoration: line-through; color: #94a3b8; }
.event-item.is-link { background: rgba(35, 131, 226, 0.08); color: #2383e2; cursor: pointer; border: 1px solid rgba(35, 131, 226, 0.2); }
.event-item:hover .del-btn { display: block; }
.icon { margin-right: 4px; font-size: 10px; }
.text { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.del-btn { display: none; position: absolute; right: 4px; background: none; border: none; color: #ef4444; cursor: pointer; }

/* 弹窗样式 */
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.3); z-index: 9999; display: flex; justify-content: center; align-items: center; }
.modal-content { background: white; padding: 24px; border-radius: 12px; width: 400px; box-shadow: 0 20px 40px rgba(0,0,0,0.2); }
.modal-content h3 { margin-top: 0; margin-bottom: 8px; color: #1e293b; }
.modal-date { margin-top: 0; color: #64748b; font-size: 14px; margin-bottom: 20px; }
.event-input, .search-input { width: 100%; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; margin-bottom: 12px; box-sizing: border-box; }
.event-input:focus, .search-input:focus { border-color: #2383e2; outline: none; }
.todo-toggle-label { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #475569; margin-bottom: 16px; cursor: pointer; user-select: none; }
.todo-toggle-label input { cursor: pointer; }
.divider { text-align: center; color: #94a3b8; font-size: 12px; margin: 12px 0; }
.search-results { border: 1px solid #e2e8f0; border-radius: 6px; max-height: 150px; overflow-y: auto; margin-bottom: 12px; }
.search-item { padding: 8px 12px; cursor: pointer; font-size: 14px; border-bottom: 1px solid #f1f5f9; }
.search-item:hover { background: #f8fafc; color: #2383e2; }
.space-tag { font-size: 12px; color: #94a3b8; margin-left: 8px; }
.selected-note { padding: 10px; background: #eff6ff; color: #1e3a8a; border-radius: 6px; display: flex; justify-content: space-between; margin-bottom: 16px; }
.selected-note button { background: none; border: none; color: #3b82f6; cursor: pointer; }
.modal-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 20px; }
.btn-cancel { padding: 8px 16px; border: 1px solid #cbd5e1; background: white; border-radius: 6px; cursor: pointer; }
.btn-save { padding: 8px 16px; border: none; background: #2383e2; color: white; border-radius: 6px; cursor: pointer; }
</style>