<template>
  <div class="sidebar-node">
    <div 
      class="node-row" 
      :style="{ paddingLeft: depth * 12 + 12 + 'px' }"
      :class="{ 
        'is-active': activeId === item.Id,
        'is-drag-over': isDragOver 
      }"
      draggable="true"
      @click.stop="handleNodeClick"
      @dragstart.stop="onDragStart"
      @drop.stop="onDrop"
      @dragover.prevent="isDragOver = true"
      @dragleave="isDragOver = false"
    >
      <span class="ctrl-icon" @click.stop="toggleExpand">
         <template v-if="item.Children && item.Children.length">
           {{ isExpanded ? '▼' : '▶' }}
         </template>
         <template v-else>•</template>
      </span>
      
      <span class="node-text" :title="item.Title">
        {{ item.Title || '无标题页面' }}
      </span>
      
      <div class="actions">
        <button class="action-btn add" title="新建子页面" @click.stop="createSub(item.Id)">+</button>
        <button class="action-btn del" title="删除页面" @click.stop="$emit('delete', item.Id)">🗑️</button>
      </div>
    </div>

    <div v-if="isExpanded && item.Children" class="node-children">
      <SidebarItem
        v-for="child in item.Children"
        :key="child.Id" 
        :item="child"
        :depth="depth + 1"
        :active-id="activeId"
        @select="onChildSelect" 
        @delete="$emit('delete', $event)"
        @refresh="$emit('refresh')"
        @drag-start="$emit('drag-start', $event)" 
        @drop-on="$emit('drop-on', $event)"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/utils/api'
import SidebarItem from './SidebarItem.vue' // 递归引用自己

const props = defineProps(['item', 'depth', 'activeId'])
const emit = defineEmits(['select', 'refresh', 'delete', 'drag-start', 'drop-on'])

const isExpanded = ref(false)
const isDragOver = ref(false)

const handleNodeClick = () => { emit('select', props.item.Id) }
const onChildSelect = (id) => { emit('select', id) }

const onDragStart = (event) => {
  event.dataTransfer.effectAllowed = 'move'
  event.dataTransfer.setData('text/plain', props.item.Id)
  emit('drag-start', props.item)
}

const onDrop = () => {
  isDragOver.value = false
  emit('drop-on', props.item)
}

const toggleExpand = () => {
  if (props.item.Children && props.item.Children.length) {
    isExpanded.value = !isExpanded.value
  }
}

const createSub = async (parentId) => {
  try {
    await apiClient.post('/Notes/create-sub', { parentId, title: '未命名子页面' })
    isExpanded.value = true
    emit('refresh')
  } catch (e) { console.error(e) }
}
</script>

<style lang="scss" scoped>
.node-row { 
  display: flex; align-items: center; padding: 6px 10px; cursor: pointer; font-size: 14px; color: #37352f; transition: background 0.1s; user-select: none; height: 32px; border-radius: 4px; margin: 1px 0;
  &:hover { background: #efefef; .actions { opacity: 1; } } 
  &.is-active { background: #e0e0e0; font-weight: 600; color: #000; } 
  &.is-drag-over { background: #d0e8ff; outline: 2px solid #2383e2; z-index: 10; }
  .ctrl-icon { width: 20px; font-size: 10px; color: #999; text-align: center; margin-right: 2px; &:hover { color: #333; } } 
  .node-text { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; } 
  .actions { display: flex; align-items: center; gap: 2px; opacity: 0; transition: opacity 0.2s; }
  .action-btn { border: none; background: transparent; color: #999; cursor: pointer; font-size: 14px; padding: 0; width: 20px; height: 20px; border-radius: 3px; &:hover { color: #000; background: rgba(0,0,0,0.05); } &.del:hover { color: #d93025; background: #fee2e2; } } 
}
</style>