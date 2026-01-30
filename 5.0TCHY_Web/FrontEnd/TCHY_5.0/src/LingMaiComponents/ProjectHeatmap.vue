<template>
  <node-view-wrapper class="heatmap-block">
    <div class="heatmap-header">
      <div class="title">
        <span class="icon">🎨</span> 项目足迹 (自动分类)
      </div>
      
      <div class="legend-scroll">
        <div class="legend">
          <div 
            v-for="type in availableTypes" 
            :key="type" 
            class="legend-item"
            @click="toggleFilter(type)"
            :class="{ 'is-dimmed': activeFilter && activeFilter !== type }"
          >
            <span class="dot" :style="{ background: getTypeColor(type) }"></span>
            <span class="name">{{ type }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="heatmap-container" ref="containerRef">
      <div class="week-days">
        <span>Mon</span><span>Wed</span><span>Fri</span>
      </div>

      <div class="heatmap-grid">
        <div v-for="(week, wIndex) in calendarData" :key="wIndex" class="week-col">
          <div 
            v-for="(day, dIndex) in week" 
            :key="dIndex"
            class="day-cell"
            :style="dayStyle(day.date)"
            :class="{ 'is-empty': !day.date, 'is-dimmed': shouldDim(day.date) }"
            :data-date="day.dateStr"
            :data-info="getDayTooltip(day.date)"
          ></div>
        </div>
      </div>
    </div>
  </node-view-wrapper>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { nodeViewProps, NodeViewWrapper } from '@tiptap/vue-3'
import apiClient from '@/utils/api'
import tippy from 'tippy.js'

const props = defineProps(nodeViewProps)
const containerRef = ref(null)
const activityData = ref({}) 
const activeFilter = ref(null)

// 🔥 核心修复：强制使用本地日期格式 (YYYY-MM-DD)
// 解决 toISOString() 导致的 UTC 时区偏差问题
const toLocalDateStr = (date) => {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// --- 🎨 核心算法：字符串 -> 颜色 ---
const stringToColor = (str) => {
  if (!str || str === 'Default') return '#ebedf0' // 默认灰色
  
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  
  // HSL 算法：固定饱和度和亮度，只变色相
  const h = Math.abs(hash % 360);
  return `hsl(${h}, 70%, 60%)`; 
}

const getTypeColor = (type) => {
  return stringToColor(type)
}

// --- 📅 数据处理 ---

// 动态计算所有存在的类型 (用于渲染图例)
const availableTypes = computed(() => {
  const typesSet = new Set()
  Object.values(activityData.value).forEach(typeList => {
    typeList.forEach(t => typesSet.add(t))
  })
  return Array.from(typesSet).sort()
})

const calendarData = computed(() => {
  const weeks = []
  const today = new Date()
  const startDate = new Date(today)
  startDate.setDate(today.getDate() - 365)
  const dayOfWeek = startDate.getDay()
  startDate.setDate(startDate.getDate() - dayOfWeek) // 对齐到周日

  let current = new Date(startDate)
  
  for (let w = 0; w < 53; w++) {
    const week = []
    for (let d = 0; d < 7; d++) {
      if (current > today) {
        week.push({ date: null })
      } else {
        week.push({ 
          date: new Date(current),
          // 🔥 修复：使用本地日期格式
          dateStr: toLocalDateStr(current) 
        })
      }
      current.setDate(current.getDate() + 1)
    }
    weeks.push(week)
  }
  return weeks
})

// 获取某天的主导类型
const getDayDominantType = (dateStr) => {
  const types = activityData.value[dateStr]
  if (!types || types.length === 0) return null

  const counts = {}
  let maxType = types[0], maxCount = 0
  
  types.forEach(t => {
    counts[t] = (counts[t] || 0) + 1
    if (counts[t] > maxCount) {
      maxCount = counts[t]
      maxType = t
    }
  })
  return maxType
}

// 单元格样式
const dayStyle = (date) => {
  if (!date) return {}
  // 🔥 修复：使用本地日期格式
  const dateStr = toLocalDateStr(date)
  const type = getDayDominantType(dateStr)
  
  if (!type) return { background: '#ebedf0' }
  return { background: getTypeColor(type) }
}

// 过滤变暗逻辑
const shouldDim = (date) => {
  if (!activeFilter.value || !date) return false
  // 🔥 修复：使用本地日期格式
  const dateStr = toLocalDateStr(date)
  const type = getDayDominantType(dateStr)
  return type !== activeFilter.value
}

const toggleFilter = (type) => {
  activeFilter.value = activeFilter.value === type ? null : type
}

const getDayTooltip = (date) => {
  if (!date) return ''
  // 🔥 修复：使用本地日期格式
  const dateStr = toLocalDateStr(date)
  const types = activityData.value[dateStr] || []
  if (types.length === 0) return `${dateStr}: 无贡献`
  
  const summary = types.reduce((acc, t) => {
    acc[t] = (acc[t] || 0) + 1; return acc
  }, {})
  
  let text = `<div style="font-size:12px; font-weight:bold; margin-bottom:4px">${dateStr}</div>`
  for (let [t, c] of Object.entries(summary)) {
    const color = getTypeColor(t)
    text += `<div style="display:flex;align-items:center;gap:4px;font-size:11px">
              <span style="display:inline-block;width:6px;height:6px;border-radius:50%;background:${color}"></span>
              ${t}: ${c} 项
             </div>`
  }
  return text
}

const fetchData = async () => {
  const parentId = props.node.attrs.parentId
  if (!parentId) return
  try {
    const res = await apiClient.get(`/Notes/${parentId}/activity`)
    activityData.value = res.data
  } catch (e) { console.error(e) }
}

onMounted(() => {
  fetchData()
  setTimeout(() => {
    if (containerRef.value) {
      tippy('.day-cell', {
        content: (el) => el.getAttribute('data-info'),
        allowHTML: true,
        theme: 'light-border',
        animation: 'shift-away',
        delay: [200, 0]
      })
    }
  }, 500)
})
</script>

<style lang="scss" scoped>
.heatmap-block {
  background: #fff; border: 1px solid #eee; border-radius: 8px; padding: 16px; margin: 20px 0;
}
.heatmap-header {
  display: flex; flex-direction: column; gap: 8px; margin-bottom: 12px;
  .title { font-weight: 600; color: #333; font-size: 14px; display: flex; align-items: center; gap: 6px; }
}

.legend-scroll {
  overflow-x: auto; 
  padding-bottom: 4px;
  &::-webkit-scrollbar { height: 4px; }
  &::-webkit-scrollbar-thumb { background: #eee; border-radius: 4px; }
}

.legend { 
  display: flex; gap: 12px; width: max-content;
  .legend-item { 
    display: flex; align-items: center; gap: 5px; cursor: pointer; font-size: 12px; color: #555; transition: opacity 0.2s;
    background: #f9f9f9; padding: 2px 8px; border-radius: 12px; border: 1px solid transparent;
    
    .dot { width: 8px; height: 8px; border-radius: 50%; border: 1px solid rgba(0,0,0,0.1); }
    &:hover { background: #f0f0f0; }
    &.is-dimmed { opacity: 0.3; }
  }
}

.heatmap-container { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 4px; }
.week-days { display: flex; flex-direction: column; justify-content: space-between; padding-top: 13px; height: 86px; font-size: 10px; color: #ccc; flex-shrink: 0;}
.heatmap-grid { display: flex; gap: 3px; }
.week-col { display: flex; flex-direction: column; gap: 3px; }
.day-cell {
  width: 10px; height: 10px; border-radius: 2px; border: 1px solid rgba(0,0,0,0.05); transition: all 0.2s;
  &.is-empty { visibility: hidden; }
  &.is-dimmed { opacity: 0.1; filter: grayscale(100%); }
  &:hover { transform: scale(1.4); border-color: rgba(0,0,0,0.5); z-index: 10; box-shadow: 0 2px 4px rgba(0,0,0,0.2); }
}
</style>