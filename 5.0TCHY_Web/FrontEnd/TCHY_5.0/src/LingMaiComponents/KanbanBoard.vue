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
              :key="card.Id" 
              class="k-card"
              :class="{ 'is-done': status === '已完成' || status === 'Done' }"
              draggable="true"
              @dragstart="onCardDragStart($event, card)"
              @click="openCard(card.Id)"
            >
              <div class="card-header">
                <div class="checkbox-wrapper" @click.stop>
                  <input 
                    type="checkbox" 
                    :checked="isCardDone(card)" 
                    @change="toggleTaskDone(card)"
                  />
                </div>
                <div class="card-title">{{ card.Title || '无标题' }}</div>
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

// 从 Tiptap 节点属性中初始化列数据
const columns = ref([...(props.node.attrs.columns || ['待办', '进行中', '已完成'])])

// 同步列数据到 Tiptap 编辑器中（为了保存）
const updateColumnsAttribute = () => {
  props.updateAttributes({ columns: columns.value })
}

const availableTypes = ['Default', 'Coding', 'Writing', 'Design', 'BugFix', 'Life', 'Study']

// 获取卡片状态
const getCardStatus = (card) => {
  const p = card.Properties?.find(prop => (prop.PropKey || prop.propKey) === 'Status')
  return (p?.PropValue || p?.propValue) || columns.value[0] // 如果没有状态，默认归入第一列
}

const getCards = (status) => cards.value.filter(c => getCardStatus(c) === status)

const getCardType = (card) => {
  const prop = card.Properties?.find(p => (p.PropKey === 'Type' || p.propKey === 'Type'))
  return (prop?.PropValue || prop?.propValue) || 'Default'
}

const getTypeColor = (str) => {
  if (!str || str === 'Default') return '#eee'
  let hash = 0;
  for (let i = 0; i < str.length; i++) { hash = str.charCodeAt(i) + ((hash << 5) - hash); }
  const h = Math.abs(hash % 360);
  return `hsl(${h}, 70%, 85%)`; 
}

const saveProperty = async (card, key, value) => {
  let prop = card.Properties?.find(p => (p.PropKey === key || p.propKey === key))
  if (!prop) {
    if (!card.Properties) card.Properties = []
    prop = { PropKey: key, PropValue: value, PropType: 'select' }
    card.Properties.push(prop)
  } else {
    prop.PropValue = value
    prop.propValue = value
  }

  try {
    await apiClient.post('/Notes/property/save', {
      noteId: card.Id,
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

/* ================= 动态列管理 ================= */

// 添加列
const addColumn = () => {
  const name = prompt("请输入新列名称:")
  if (name && !columns.value.includes(name)) {
    columns.value.push(name)
    updateColumnsAttribute()
  } else if (columns.value.includes(name)) {
    alert("该列名已存在！")
  }
}

// 重命名列 (双击列头触发)
const renameColumn = async (oldName) => {
  const newName = prompt(`将 "${oldName}" 重命名为:`, oldName)
  if (newName && newName !== oldName && !columns.value.includes(newName)) {
    // 1. 更新列数组
    const index = columns.value.indexOf(oldName)
    columns.value[index] = newName
    updateColumnsAttribute()

    // 2. 批量更新该列下所有卡片的属性
    const cardsToUpdate = cards.value.filter(c => getCardStatus(c) === oldName)
    for (const card of cardsToUpdate) {
      await saveProperty(card, 'Status', newName)
    }
  }
}

// 删除列
const removeColumn = (statusName) => {
  const cardCount = getCards(statusName).length
  if (cardCount > 0) {
    if(!confirm(`列 "${statusName}" 中还有 ${cardCount} 个任务。确定要删除该列吗？（卡片不会丢失，但会失去当前状态）`)) return
  } else {
    if(!confirm(`确定要删除列 "${statusName}" 吗？`)) return
  }
  
  columns.value = columns.value.filter(c => c !== statusName)
  updateColumnsAttribute()
}

/* ================= 拖拽逻辑 (双层拖拽) ================= */

// 卡片拖拽起点
const onCardDragStart = (e, card) => {
  e.stopPropagation() // 关键：阻止事件冒泡到列的拖拽上
  e.dataTransfer.effectAllowed = 'move'
  e.dataTransfer.setData('cardId', card.Id)
}

// 列拖拽起点
const onColDragStart = (e, index) => {
  e.dataTransfer.effectAllowed = 'move'
  e.dataTransfer.setData('colIndex', index)
}

// 统一的 Drop 处理中心
const onDrop = async (e, targetStatus, dropColIndex) => {
  activeColumn.value = null
  const cardId = e.dataTransfer.getData('cardId')
  const sourceColIndex = e.dataTransfer.getData('colIndex')

  // 情况 A: 拖拽的是卡片
  if (cardId) {
    const card = cards.value.find(c => c.Id === cardId)
    if (card && getCardStatus(card) !== targetStatus) {
       await saveProperty(card, 'Status', targetStatus)
    }
  } 
  // 情况 B: 拖拽的是列
  else if (sourceColIndex !== "") {
    const fromIndex = parseInt(sourceColIndex, 10)
    if (fromIndex !== dropColIndex) {
      const newCols = [...columns.value]
      const [movedCol] = newCols.splice(fromIndex, 1) // 拔出原来的列
      newCols.splice(dropColIndex, 0, movedCol)       // 插入新位置
      columns.value = newCols
      updateColumnsAttribute()
    }
  }
}

/* ================= 其他原有逻辑 ================= */

const cycleCardType = async (card) => {
  const currentType = getCardType(card)
  let currentIndex = availableTypes.indexOf(currentType)
  if (currentIndex === -1) currentIndex = -1
  const nextType = availableTypes[(currentIndex + 1) % availableTypes.length]
  await saveProperty(card, 'Type', nextType)
}

const customCardType = async (card) => {
  const current = getCardType(card)
  const input = prompt("请输入自定义标签名称:", current)
  if (input && input.trim() !== "") {
    await saveProperty(card, 'Type', input.trim())
  }
}

/* ================= 卡片完成状态 (独立于列) ================= */

// 1. 读取卡片是否已打勾
const isCardDone = (card) => {
  const prop = card.Properties?.find(p => (p.PropKey === 'IsDone' || p.propKey === 'IsDone'))
  return (prop?.PropValue || prop?.propValue) === 'true'
}

// 2. 切换打勾状态（原地划线，不再移动列）
const toggleTaskDone = async (card) => {
  const current = isCardDone(card)
  const newValue = current ? 'false' : 'true'
  
  // 乐观更新：立刻在前端响应变化，不等后端接口返回
  let prop = card.Properties?.find(p => (p.PropKey === 'IsDone' || p.propKey === 'IsDone'))
  if (!prop) {
    if (!card.Properties) card.Properties = []
    card.Properties.push({ PropKey: 'IsDone', PropValue: newValue, PropType: 'select' })
  } else {
    prop.PropValue = newValue
    prop.propValue = newValue
  }

  // 发送接口保存这个新属性
  await saveProperty(card, 'IsDone', newValue)
}

const refresh = async () => {
  const id = props.node.attrs.parentId
  if (!id) return
  const res = await apiClient.get(`/Notes/${id}/children`)
  cards.value = res.data
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
  } catch (e) {
    console.error(e)
  }
}

const openCard = (id) => {
  window.dispatchEvent(new CustomEvent('navigate-note', { detail: id }))
}

onMounted(refresh)
watch(() => props.node.attrs.parentId, refresh)
</script>

<style lang="scss" scoped>
.kanban-block-wrapper {
  margin: 2rem 0; border: 1px solid #efefef; border-radius: 8px; background: #fff; overflow: hidden;
  
  .kanban-block-header { 
    padding: 8px 12px; background: #f9f9f9; border-bottom: 1px solid #efefef; font-size: 12px; color: #888; display: flex; gap: 8px; 
  }
  
  /* 核心滚动区域 */
  .kanban-scroll-area { 
    overflow-x: auto; /* 允许横向滚动 */
    padding: 16px; 
    scroll-behavior: smooth;
    
    /* 美化滚动条 */
    &::-webkit-scrollbar { height: 8px; }
    &::-webkit-scrollbar-thumb { background: #d0d0d0; border-radius: 4px; }
    &::-webkit-scrollbar-thumb:hover { background: #a0a0a0; }
  }
  
  /* 容器必须是 max-content，这样子元素才不会被压缩 */
  .kanban-flex-container { 
    display: flex; 
    gap: 16px; 
    width: max-content; /* 关键代码：无限延伸不换行 */
    padding-bottom: 8px; 
  }
  
  .kanban-col {
    width: 280px; /* 固定每一列的宽度 */
    flex-shrink: 0; /* 绝对不压缩变形 */
    background: #f7f7f5; border-radius: 6px; padding: 8px; border: 2px solid transparent; transition: border-color 0.2s;
    
    &.is-over { border-color: #2383e2; background: #f0f7ff; }
    
    /* 新建列按钮样式 */
    &.add-col-btn {
      display: flex; align-items: center; justify-content: center;
      background: rgba(0,0,0,0.02); border: 2px dashed #ddd; color: #888; cursor: pointer;
      min-height: 100px; transition: all 0.2s;
      &:hover { background: #f0f0f0; border-color: #bbb; color: #333; }
    }

    .col-head { 
      padding: 4px 8px 12px; font-size: 14px; font-weight: 600; color: #444; 
      display: flex; align-items: center; justify-content: space-between; gap: 6px; 
      cursor: grab; /* 提示可拖拽 */
      
      &:active { cursor: grabbing; }
      
      .col-title {
        display: flex; align-items: center; gap: 6px; flex: 1;
        cursor: text; /* 提示双击编辑 */
      }

      .dot { width: 8px; height: 8px; border-radius: 50%; background: #2383e2; opacity: 0.6; }
      
      .col-actions {
        display: flex; align-items: center; gap: 8px;
        .count { font-weight: normal; font-size: 12px; opacity: 0.6; }
        .delete-btn {
          cursor: pointer; color: #999; font-size: 16px; opacity: 0; transition: opacity 0.2s;
          &:hover { color: #e74c3c; }
        }
      }

      &:hover .delete-btn { opacity: 1; }
    }
    
    .col-body { min-height: 50px; }
    
    .k-card { 
      background: #fff; padding: 10px; border-radius: 4px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); 
      margin-bottom: 8px; font-size: 14px; cursor: pointer; border: 1px solid transparent; transition: all 0.2s;
      
      /* 当鼠标放在卡片上时，改变鼠标指针为抓取状态 */
      &:hover { border-color: #ddd; }
      &:active { cursor: grabbing; }
      
      &.is-done { opacity: 0.6; .card-title { text-decoration: line-through; color: #999; } }
      
      .card-header { display: flex; align-items: flex-start; gap: 8px; margin-bottom: 6px; }
      
      .checkbox-wrapper {
        padding-top: 2px;
        input[type="checkbox"] { cursor: pointer; accent-color: #2383e2; width: 1.1em; height: 1.1em; }
      }

      .card-title { flex: 1; line-height: 1.4; word-break: break-all; }
      
      .card-meta {
        display: flex; align-items: center; justify-content: flex-end; margin-top: 4px;
        .type-tag {
           font-size: 10px; padding: 2px 6px; border-radius: 4px; color: #555; font-weight: 500;
           user-select: none; transition: filter 0.2s; cursor: pointer;
           &:hover { filter: brightness(0.9); }
           &:active { transform: scale(0.95); }
        }
      }
    }
    
    .k-add { padding: 6px; font-size: 12px; color: #999; cursor: pointer; text-align: center; border-radius: 4px; &:hover { background: #eee; color: #333; } }
  }
}
</style>