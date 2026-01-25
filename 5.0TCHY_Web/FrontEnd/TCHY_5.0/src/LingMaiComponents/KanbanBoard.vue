<template>
  <div class="kanban-board">
    <div 
      v-for="status in columns" 
      :key="status" 
      class="kanban-column"
      @dragover.prevent
      @drop="onDrop($event, status)"
    >
      <div class="column-header">
        <div class="header-left">
          <span class="status-dot" :class="statusClass(status)"></span>
          <span class="status-name">{{ statusMap[status] }}</span>
          <span class="count">{{ getCardsByStatus(status).length }}</span>
        </div>
      </div>

      <div class="column-body">
        <div 
          v-for="card in getCardsByStatus(status)" 
          :key="card.Id" 
          class="kanban-card"
          draggable="true"
          @dragstart="onDragStart($event, card)"
          @click="$emit('open-card', card.Id)"
        >
          <div class="card-title">{{ card.Title || '未命名页面' }}</div>
          
          <div class="card-footer">
             <span v-if="card.Properties.length > 1" class="prop-badge">
               {{ card.Properties.length - 1 }} 个其他属性
             </span>
          </div>
        </div>

        <div class="add-card-btn" @click="addCard(status)">
          + 新建
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps(['parentId'])
const emit = defineEmits(['open-card'])

const rawCards = ref([])

// 定义看板的列 (对应数据库里 NoteProperty 的 Value)
const columns = ['To Do', 'In Progress', 'Done']
// 显示在界面上的中文名
const statusMap = { 'To Do': '待办', 'In Progress': '进行中', 'Done': '已完成' }

const statusClass = (s) => {
  if (s === 'To Do') return 'bg-gray'
  if (s === 'In Progress') return 'bg-blue'
  if (s === 'Done') return 'bg-green'
  return 'bg-gray'
}

// 1. 获取子页面数据
const fetchChildren = async () => {
  if (!props.parentId) return
  try {
    const res = await apiClient.get(`/Notes/${props.parentId}/children`)
    rawCards.value = res.data
  } catch (e) {
    console.error("加载看板失败", e)
  }
}

// 2. 筛选卡片逻辑
const getCardsByStatus = (status) => {
  return rawCards.value.filter(card => {
    // 找到该卡片的 Status 属性
    // 注意：后端返回的 Properties 里的 Key 大小写要确认，通常是 PropKey
    const statusProp = card.Properties.find(p => p.PropKey === 'Status')
    // 如果没有状态属性，默认归为 To Do
    const cardStatus = statusProp ? statusProp.PropValue : 'To Do'
    return cardStatus === status
  })
}

// 3. 拖拽开始
const onDragStart = (evt, card) => {
  evt.dataTransfer.dropEffect = 'move'
  evt.dataTransfer.effectAllowed = 'move'
  evt.dataTransfer.setData('cardId', card.Id)
}

// 4. 拖拽放下 (核心: 修改属性)
const onDrop = async (evt, newStatus) => {
  const cardId = evt.dataTransfer.getData('cardId')
  
  // 乐观更新 (不等后端，先改界面)
  const card = rawCards.value.find(c => c.Id === cardId)
  if (card) {
    let prop = card.Properties.find(p => p.PropKey === 'Status')
    if (!prop) {
        // 如果原本没属性，就造一个
        prop = { PropKey: 'Status', PropValue: newStatus, PropType: 'select' }
        card.Properties.push(prop)
    } else {
        prop.PropValue = newStatus
    }
    
    // 发送请求给后端保存
    await apiClient.post('/Notes/property/save', {
        noteId: cardId,
        propKey: 'Status',
        propValue: newStatus,
        propType: 'select'
    })
  }
}

// 5. 快速新建任务
const addCard = async (status) => {
    const title = prompt("输入任务名称:")
    if (!title) return
    
    // A. 创建子页面
    const res = await apiClient.post('/Notes/create-sub', { 
        parentId: props.parentId, 
        title: title 
    })
    const newId = res.data.id

    // B. 赋予它状态属性
    await apiClient.post('/Notes/property/save', {
        noteId: newId,
        propKey: 'Status',
        propValue: status,
        propType: 'select'
    })

    // C. 刷新
    fetchChildren() 
}

// 监听父ID变化，自动刷新
watch(() => props.parentId, fetchChildren, { immediate: true })
</script>

<style lang="scss" scoped>
.kanban-board {
  display: flex;
  height: 600px; /* 固定高度或者 100% */
  overflow-x: auto;
  gap: 15px;
  padding: 20px 0;
  align-items: flex-start;
  background: #fff;
}

.kanban-column {
  width: 260px;
  flex-shrink: 0;
  background: #f7f7f5;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  max-height: 100%;
}

.column-header {
  padding: 12px;
  font-size: 14px;
  font-weight: 600;
  color: #37352f;
  
  .header-left { display: flex; align-items: center; gap: 6px; }
  
  .status-dot { width: 8px; height: 8px; border-radius: 50%; }
  .bg-gray { background: #9b9a97; }
  .bg-blue { background: #2383e2; }
  .bg-green { background: #008953; }
  
  .count { color: #999; font-weight: normal; margin-left: auto; font-size: 12px; }
}

.column-body {
  padding: 0 8px 8px 8px;
  overflow-y: auto;
  flex: 1;
}

.kanban-card {
  background: #fff;
  padding: 10px;
  border-radius: 4px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
  margin-bottom: 8px;
  cursor: grab;
  transition: transform 0.1s, box-shadow 0.1s;
  border: 1px solid transparent;
  
  &:hover { 
    background: #fafafa; 
    border-color: #eee;
  }
  &:active { cursor: grabbing; transform: rotate(1deg); }

  .card-title { font-size: 14px; color: #37352f; margin-bottom: 4px; font-weight: 500; }
  .card-footer { font-size: 10px; color: #aaa; }
}

.add-card-btn {
  padding: 6px;
  color: #999;
  cursor: pointer;
  border-radius: 4px;
  font-size: 13px;
  text-align: center;
  &:hover { background: #e0e0e0; color: #333; }
}
</style>