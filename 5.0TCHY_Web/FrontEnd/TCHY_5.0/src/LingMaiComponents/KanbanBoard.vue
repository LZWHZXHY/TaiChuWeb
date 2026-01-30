<template>
  <node-view-wrapper class="kanban-block-wrapper">
    <div class="kanban-block-header">
      <span class="icon">📊</span>
      <span class="label">任务看板 (子页面视图)</span>
    </div>
    
    <div class="kanban-scroll-area">
      <div class="kanban-flex-container">
        <div 
          v-for="status in columns" 
          :key="status" 
          class="kanban-col"
          :class="{ 'is-over': activeColumn === status }"
          @dragover.prevent="activeColumn = status"
          @dragleave="activeColumn = null"
          @drop="onDrop($event, status)"
        >
          <div class="col-head">
            <span class="dot" :class="status.replace(' ', '-').toLowerCase()"></span>
            {{ statusMap[status] }}
            <span class="count">{{ getCards(status).length }}</span>
          </div>

          <div class="col-body">
            <div 
              v-for="card in getCards(status)" 
              :key="card.Id" 
              class="k-card"
              :class="{ 'is-done': status === 'Done' }"
              draggable="true"
              @dragstart="onDragStart($event, card)"
              @click="openCard(card.Id)"
            >
              <div class="card-header">
                <div class="checkbox-wrapper" @click.stop>
                  <input 
                    type="checkbox" 
                    :checked="status === 'Done'" 
                    @change="toggleTaskStatus(card, status)"
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
                  title="单击切换预设，双击自定义输入"
                >
                  {{ getCardType(card) || 'Default' }}
                </div>
              </div>

            </div>
            <div class="k-add" @click="addCard(status)">+ 新建</div>
          </div>
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

const columns = ['To Do', 'In Progress', 'Done']
const statusMap = { 'To Do': '待办', 'In Progress': '进行中', 'Done': '已完成' }

// 预设类型 (你仍然可以用这些快速切换)
const availableTypes = ['Default', 'Coding', 'Writing', 'Design', 'BugFix', 'Life', 'Study']

const getCards = (s) => cards.value.filter(c => {
  const p = c.Properties?.find(prop => (prop.PropKey || prop.propKey) === 'Status')
  return (p?.PropValue || p?.propValue || 'To Do') === s
})

const getCardType = (card) => {
  const prop = card.Properties?.find(p => (p.PropKey === 'Type' || p.propKey === 'Type'))
  return (prop?.PropValue || prop?.propValue) || 'Default'
}

// 颜色算法：你的自定义文字会自动生成一个固定的颜色
const getTypeColor = (str) => {
  if (!str || str === 'Default') return '#eee'
  let hash = 0;
  for (let i = 0; i < str.length; i++) { hash = str.charCodeAt(i) + ((hash << 5) - hash); }
  const h = Math.abs(hash % 360);
  return `hsl(${h}, 70%, 85%)`; 
}

// 💾 通用保存属性方法
const saveProperty = async (card, key, value) => {
  // 1. 乐观更新
  let prop = card.Properties?.find(p => (p.PropKey === key || p.propKey === key))
  if (!prop) {
    if (!card.Properties) card.Properties = []
    prop = { PropKey: key, PropValue: value, PropType: 'select' }
    card.Properties.push(prop)
  } else {
    prop.PropValue = value
    prop.propValue = value
  }

  // 2. 发送请求
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

// 🔄 单击：循环切换预设
const cycleCardType = async (card) => {
  const currentType = getCardType(card)
  // 如果当前类型不在预设列表里（是自定义的），重置回 Default
  let currentIndex = availableTypes.indexOf(currentType)
  if (currentIndex === -1) currentIndex = -1
  
  const nextType = availableTypes[(currentIndex + 1) % availableTypes.length]
  await saveProperty(card, 'Type', nextType)
}

// ✏️ 双击：自定义输入
const customCardType = async (card) => {
  const current = getCardType(card)
  const input = prompt("请输入自定义标签名称 (例如: 洗衣服, 撸猫):", current)
  if (input && input.trim() !== "") {
    await saveProperty(card, 'Type', input.trim())
  }
}

// ✅ 打勾：在 ToDo/Done 之间切换
const toggleTaskStatus = async (card, currentStatus) => {
  const newStatus = currentStatus === 'Done' ? 'To Do' : 'Done'
  await saveProperty(card, 'Status', newStatus)
  // 状态改变会导致卡片移动列，需要刷新一下视图
  // 由于我们是 filter 过滤 cards.value，修改属性后 Vue 会自动重新渲染列表
  // 但为了保险起见，或者如果需要排序更新，可以调用 refresh()
  // 这里因为是响应式的，直接修改 card.Properties 就会触发移动动画
}

const refresh = async () => {
  const id = props.node.attrs.parentId
  if (!id) return
  const res = await apiClient.get(`/Notes/${id}/children`)
  cards.value = res.data
}

const onDragStart = (e, card) => e.dataTransfer.setData('cardId', card.Id)

const onDrop = async (e, status) => {
  activeColumn.value = null
  const cardId = e.dataTransfer.getData('cardId')
  // 乐观更新：找到卡片并修改状态，让它立即跳过去
  const card = cards.value.find(c => c.Id === cardId)
  if (card) {
     await saveProperty(card, 'Status', status)
  }
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
  .kanban-block-header { padding: 8px 12px; background: #f9f9f9; border-bottom: 1px solid #efefef; font-size: 12px; color: #888; display: flex; gap: 8px; }
  .kanban-scroll-area { overflow-x: auto; padding: 16px; }
  .kanban-flex-container { display: flex; gap: 16px; min-width: 700px; }
  .kanban-col {
    flex: 1; background: #f7f7f5; border-radius: 6px; padding: 8px; border: 2px solid transparent;
    &.is-over { border-color: #2383e2; background: #f0f7ff; }
    .col-head { padding: 4px 8px 12px; font-size: 13px; font-weight: 600; color: #666; display: flex; align-items: center; gap: 6px; 
      .count { margin-left: auto; font-weight: normal; opacity: 0.5; }
      .dot { width: 8px; height: 8px; border-radius: 50%; 
        &.to-do { background: #d3d3d3; } &.in-progress { background: #2383e2; } &.done { background: #008953; }
      }
    }
    .col-body { min-height: 50px; }
    
    .k-card { 
      background: #fff; padding: 10px; border-radius: 4px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); margin-bottom: 8px; font-size: 14px; cursor: pointer; border: 1px solid transparent; transition: all 0.2s;
      &:hover { border-color: #ddd; }
      
      /* 完成状态的卡片样式 */
      &.is-done {
        opacity: 0.6;
        .card-title { text-decoration: line-through; color: #999; }
      }
      
      .card-header {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        margin-bottom: 6px;
      }
      
      /* Checkbox 样式 */
      .checkbox-wrapper {
        padding-top: 2px;
        input[type="checkbox"] {
          cursor: pointer;
          accent-color: #2383e2;
          width: 1.1em;
          height: 1.1em;
        }
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