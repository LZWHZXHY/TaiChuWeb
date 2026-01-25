<template>
  <div class="sidebar-node">
    <div 
      class="node-row" 
      :style="{ paddingLeft: depth * 12 + 12 + 'px' }"
      :class="{ 'is-active': activeId === item.Id }"
      @click.stop="handleNodeClick"
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
      
      <button class="action-btn" title="新建子页面" @click.stop="createSub(item.Id)">
        +
      </button>
    </div>

    <div v-if="isExpanded && item.Children" class="node-children">
      <SidebarItem
        v-for="child in item.Children"
        :key="child.Id" 
        :item="child"
        :depth="depth + 1"
        :active-id="activeId"
        @select="onChildSelect" 
        @refresh="$emit('refresh')"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/utils/api'

// 🔥 核心修复：显式导入自己，确保递归正常工作
import SidebarItem from './SidebarItem.vue'

const props = defineProps(['item', 'depth', 'activeId'])
const emit = defineEmits(['select', 'refresh'])

const isExpanded = ref(false)

const handleNodeClick = () => {
  console.log("点击侧边栏节点:", props.item.Id)
  emit('select', props.item.Id)
}

const onChildSelect = (id) => {
  emit('select', id)
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
  } catch (e) { 
    console.error("创建子页面失败", e) 
  }
}
</script>

<style lang="scss" scoped>
/* 样式不变，直接复用 */
.node-row { display: flex; align-items: center; padding: 6px 10px; cursor: pointer; font-size: 14px; color: #37352f; transition: background 0.1s; user-select: none; &:hover { background: #efefef; .action-btn { opacity: 1; } } &.is-active { background: #e0e0e0; font-weight: 600; color: #000; } .ctrl-icon { width: 20px; font-size: 10px; color: #999; text-align: center; display: flex; align-items: center; justify-content: center; margin-right: 2px; &:hover { color: #333; } } .node-text { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; line-height: 1.5; } .action-btn { opacity: 0; border: none; background: transparent; color: #999; cursor: pointer; font-size: 16px; padding: 0 4px; border-radius: 3px; &:hover { color: #000; background: rgba(0,0,0,0.05); } } }
</style>