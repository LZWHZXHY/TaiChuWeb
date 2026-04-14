<template>
  <node-view-wrapper class="kanban-block-wrapper">
    <div class="kanban-block-header">
      <span class="icon">📊</span>
      <span class="label">任务看板 (动态视图)</span>
    </div>
    
    <div class="kanban-scroll-area">
      <div class="kanban-flex-container">
        <div 
          v-for="(status, index) in columns" 
          :key="status" 
          class="kanban-col"
          :class="{ 'is-over': activeColumn === status }"
          draggable="true"
          @dragstart="onColDragStart($event, index)"
          @dragover.prevent="activeColumn = status"
          @dragleave="activeColumn = null"
          @drop="onDrop($event, status, index)"
        >
          <div class="col-head" @dblclick="renameColumn(status)">
            <span class="col-title" title="双击重命名列">
              <span class="dot"></span>
              {{ status }}
            </span>
            <div class="col-actions">
              <span class="count">{{ getCards(status).length }}</span>
              <span class="delete-btn" @click.stop="removeColumn(status)" title="删除该列">×</span>
            </div>
          </div>

          <div class="col-body">
            <div 
              v-for="card in getCards(status)" 
              :key="card.id || card.Id" 
              class="k-card"
              :class="{ 'is-done': status === '已完成' || status === 'Done' || isCardDone(card) }"
              draggable="true"
              @dragstart="onCardDragStart($event, card)"
              @click="openCard(card.id || card.Id)"
            >
              <div class="card-header">
                <div class="checkbox-wrapper" @click.stop>
                  <input 
                    type="checkbox" 
                    :checked="isCardDone(card)" 
                    @change="toggleTaskDone(card)"
                  />
                </div>
                <div class="card-title">{{ card.title || card.Title || '无标题' }}</div>
              </div>
              
              <div class="card-meta">
                <div 
                  class="type-tag" 
                  :style="{ background: getTypeColor(getCardType(card)) }"
                  @click.stop="cycleCardType(card)"
                  @dblclick.stop="customCardType(card)"
                  title="单击切换预设，双击自定义"
                >
                  {{ getCardType(card) || 'Default' }}
                </div>
              </div>
            </div>
            
            <div class="k-add" @click="addCard(status)">+ 新建</div>
          </div>
        </div>

        <div class="kanban-col add-col-btn" @click="addColumn">
          <span>+ 添加新列</span>
        </div>

      </div>
    </div>
  </node-view-wrapper>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { nodeViewProps, NodeViewWrapper } from '@tiptap/vue-3'
import apiClient from '@/utils/api'

const props = defineProps(nodeViewProps)
const cards = ref([])
const activeColumn = ref(null)

const columns = ref([...(props.node.attrs.columns || ['待办', '进行中', '已完成'])])

const updateColumnsAttribute = () => {
  props.updateAttributes({ columns: columns.value })
}

const availableTypes = ['Default', 'Coding', 'Writing', 'Design', 'BugFix', 'Life', 'Study']

// ==========================================
// 🔥 核心修复区：全面兼容后端返回的 properties (小写)
// ==========================================

const getCardStatus = (card) => {
  const props = card.properties || card.Properties || []
  const p = props.find(prop => (prop.propKey || prop.PropKey) === 'Status')
  return (p?.propValue || p?.PropValue) || columns.value[0]
}

const getCardType = (card) => {
  const props = card.properties || card.Properties || []
  const prop = props.find(p => (p.propKey || p.PropKey) === 'Type')
  return (prop?.propValue || prop?.PropValue) || 'Default'
}

const isCardDone = (card) => {
  const props = card.properties || card.Properties || []
  const prop = props.find(p => (p.propKey || p.PropKey) === 'IsDone')
  const val = prop?.propValue || prop?.PropValue
  return val === 'true' || val === true
}

const saveProperty = async (card, key, value) => {
  const targetId = card.id || card.Id
  if (!targetId) return false

  // 统一初始化小写的 properties 数组
  if (!card.properties) {
    card.properties = card.Properties || []
  }

  let prop = card.properties.find(p => (p.propKey || p.PropKey) === key)
  
  if (!prop) {
    prop = { propKey: key, propValue: value, propType: 'select' }
    card.properties.push(prop)
  } else {
    // 兼容更新，确保视图响应
    prop.propValue = value
    prop.PropValue = value
  }
  
  // 保持引用同步，防止其他地方报错
  card.Properties = card.properties

  try {
    await apiClient.post('/Notes/property/save', {
      noteId: targetId,
      propKey: key,
      propValue: value,
      propType: 'select'
    })
    return true
  } catch (e) {
    console.error("保存失败", e)
    return false
  }
}

// ==========================================
// 其他业务逻辑维持不变
// ==========================================

const getCards = (status) => cards.value.filter(c => getCardStatus(c) === status)

const getTypeColor = (str) => {
  if (!str || str === 'Default') return '#eee'
  let hash = 0;
  for (let i = 0; i < str.length; i++) { hash = str.charCodeAt(i) + ((hash << 5) - hash); }
  const h = Math.abs(hash % 360);
  return `hsl(${h}, 70%, 85%)`; 
}

const addColumn = () => {
  const name = prompt("请输入新列名称:")
  if (name && !columns.value.includes(name)) {
    columns.value.push(name)
    updateColumnsAttribute()
  }
}

const renameColumn = async (oldName) => {
  const newName = prompt(`将 "${oldName}" 重命名为:`, oldName)
  if (newName && newName !== oldName && !columns.value.includes(newName)) {
    const index = columns.value.indexOf(oldName)
    columns.value[index] = newName
    updateColumnsAttribute()
    const cardsToUpdate = cards.value.filter(c => getCardStatus(c) === oldName)
    for (const card of cardsToUpdate) {
      await saveProperty(card, 'Status', newName)
    }
  }
}

const removeColumn = (statusName) => {
  if(!confirm(`确定要删除列 "${statusName}" 吗？`)) return
  columns.value = columns.value.filter(c => c !== statusName)
  updateColumnsAttribute()
}

const onCardDragStart = (e, card) => {
  e.stopPropagation()
  e.dataTransfer.effectAllowed = 'move'
  e.dataTransfer.setData('cardId', (card.id || card.Id).toLowerCase()) // 统一转小写存储
}

const onColDragStart = (e, index) => {
  e.dataTransfer.effectAllowed = 'move'
  e.dataTransfer.setData('colIndex', index)
}

const onDrop = async (e, targetStatus, dropColIndex) => {
  activeColumn.value = null
  const cardId = e.dataTransfer.getData('cardId')
  const sourceColIndex = e.dataTransfer.getData('colIndex')

  if (cardId) {
    // 关键：在匹配时也统一转小写对比
    const card = cards.value.find(c => (c.id || c.Id).toLowerCase() === cardId)
    if (card && getCardStatus(card) !== targetStatus) {
       await saveProperty(card, 'Status', targetStatus)
    }
  } 
  else if (sourceColIndex !== "") {
    const fromIndex = parseInt(sourceColIndex, 10)
    if (fromIndex !== dropColIndex) {
      const newCols = [...columns.value]
      const [movedCol] = newCols.splice(fromIndex, 1)
      newCols.splice(dropColIndex, 0, movedCol)
      columns.value = newCols
      updateColumnsAttribute()
    }
  }
}

const cycleCardType = async (card) => {
  const currentType = getCardType(card)
  let currentIndex = availableTypes.indexOf(currentType)
  const nextType = availableTypes[(currentIndex + 1) % availableTypes.length]
  await saveProperty(card, 'Type', nextType)
}

const customCardType = async (card) => {
  const current = getCardType(card)
  const input = prompt("请输入自定义标签名称:", current)
  if (input?.trim()) await saveProperty(card, 'Type', input.trim())
}

const toggleTaskDone = async (card) => {
  const current = isCardDone(card)
  const newValue = current ? 'false' : 'true'
  await saveProperty(card, 'IsDone', newValue)
}

const refresh = async () => {
  const id = props.node.attrs.parentId
  if (!id) return
  try {
    const res = await apiClient.get(`/Notes/${id}/children`)
    cards.value = res.data
  } catch (e) { console.error(e) }
}

const addCard = async (status) => {
  const title = prompt("任务名称:")
  if (!title) return
  try {
    const res = await apiClient.post('/Notes/create-sub', { parentId: props.node.attrs.parentId, title })
    const newId = res.data.id
    await apiClient.post('/Notes/property/save', { noteId: newId, propKey: 'Status', propValue: status, propType: 'select' })
    await apiClient.post('/Notes/property/save', { noteId: newId, propKey: 'Type', propValue: 'Default', propType: 'select' })
    refresh()
  } catch (e) { console.error(e) }
}

const openCard = (id) => {
  if (id) window.dispatchEvent(new CustomEvent('navigate-note', { detail: id }))
}

onMounted(refresh)
watch(() => props.node.attrs.parentId, refresh)
</script>

<style lang="scss" scoped>
.kanban-block-wrapper {
  margin: 2rem 0; border: 1px solid #efefef; border-radius: 8px; background: #fff; overflow: hidden;
  .kanban-block-header { padding: 8px 12px; background: #f9f9f9; border-bottom: 1px solid #efefef; font-size: 12px; color: #888; display: flex; gap: 8px; }
  .kanban-scroll-area { overflow-x: auto; padding: 16px; scroll-behavior: smooth; &::-webkit-scrollbar { height: 8px; } &::-webkit-scrollbar-thumb { background: #d0d0d0; border-radius: 4px; } }
  .kanban-flex-container { display: flex; gap: 16px; width: max-content; padding-bottom: 8px; }
  .kanban-col {
    width: 280px; flex-shrink: 0; background: #f7f7f5; border-radius: 6px; padding: 8px; border: 2px solid transparent; transition: border-color 0.2s;
    &.is-over { border-color: #2383e2; background: #f0f7ff; }
    &.add-col-btn { display: flex; align-items: center; justify-content: center; background: rgba(0,0,0,0.02); border: 2px dashed #ddd; color: #888; cursor: pointer; min-height: 100px; &:hover { background: #f0f0f0; border-color: #bbb; color: #333; } }
    .col-head { padding: 4px 8px 12px; font-size: 14px; font-weight: 600; color: #444; display: flex; align-items: center; justify-content: space-between; gap: 6px; cursor: grab; .col-title { display: flex; align-items: center; gap: 6px; flex: 1; cursor: text; } .dot { width: 8px; height: 8px; border-radius: 50%; background: #2383e2; opacity: 0.6; } .col-actions { display: flex; align-items: center; gap: 8px; .count { font-weight: normal; font-size: 12px; opacity: 0.6; } .delete-btn { cursor: pointer; color: #999; font-size: 16px; opacity: 0; &:hover { color: #e74c3c; } } } &:hover .delete-btn { opacity: 1; } }
    .col-body { min-height: 50px; }
    .k-card { 
      background: #fff; padding: 10px; border-radius: 4px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); margin-bottom: 8px; font-size: 14px; cursor: pointer; border: 1px solid transparent; transition: all 0.2s;
      &:hover { border-color: #ddd; } &.is-done { opacity: 0.6; .card-title { text-decoration: line-through; color: #999; } }
      .card-header { display: flex; align-items: flex-start; gap: 8px; margin-bottom: 6px; }
      .checkbox-wrapper { padding-top: 2px; input[type="checkbox"] { cursor: pointer; accent-color: #2383e2; } }
      .card-title { flex: 1; line-height: 1.4; word-break: break-all; }
      .card-meta { display: flex; align-items: center; justify-content: flex-end; margin-top: 4px; .type-tag { font-size: 10px; padding: 2px 6px; border-radius: 4px; color: #555; font-weight: 500; cursor: pointer; } }
    }
    .k-add { padding: 6px; font-size: 12px; color: #999; cursor: pointer; text-align: center; &:hover { background: #eee; color: #333; } }
  }
}
</style>