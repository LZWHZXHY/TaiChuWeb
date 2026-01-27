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
              draggable="true"
              @dragstart="onDragStart($event, card)"
              @click="openCard(card.Id)"
            >
              {{ card.Title || '无标题' }}
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

const getCards = (s) => cards.value.filter(c => {
  const p = c.Properties?.find(prop => (prop.PropKey || prop.propKey) === 'Status')
  return (p?.PropValue || p?.propValue || 'To Do') === s
})

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
  await apiClient.post('/Notes/property/save', { noteId: cardId, propKey: 'Status', propValue: status, propType: 'select' })
  refresh()
}

const addCard = async (status) => {
  const title = prompt("任务名称:")
  if (!title) return
  const res = await apiClient.post('/Notes/create-sub', { parentId: props.node.attrs.parentId, title })
  await apiClient.post('/Notes/property/save', { noteId: res.data.id, propKey: 'Status', propValue: status, propType: 'select' })
  refresh()
}

const openCard = (id) => {
  // 通过 TipTap 的编辑逻辑触发导航，或者直接调用全局 window 事件
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
    .k-card { 
      background: #fff; padding: 10px; border-radius: 4px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); margin-bottom: 8px; font-size: 14px; cursor: pointer; border: 1px solid transparent;
      &:hover { border-color: #ddd; }
    }
    .k-add { padding: 6px; font-size: 12px; color: #999; cursor: pointer; text-align: center; border-radius: 4px; &:hover { background: #eee; color: #333; } }
  }
}
</style>