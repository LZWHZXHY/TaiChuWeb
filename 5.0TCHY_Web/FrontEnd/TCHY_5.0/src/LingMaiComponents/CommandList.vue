<template>
  <div class="command-list">
    <div 
      v-for="(item, index) in items"
      :key="index"
      class="item"
      :class="{ 'is-selected': index === selectedIndex }"
      @click="selectItem(index)"
      @mouseenter="selectedIndex = index"
    >
      <div class="icon">{{ item.icon }}</div>
      <div class="info">
        <div class="label">{{ item.title }}</div>
        </div>
    </div>
    <div v-if="items.length === 0" class="empty">没有找到命令</div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  items: { type: Array, required: true },
  command: { type: Function, required: true }
})

const selectedIndex = ref(0)

// 当列表变化时，重置选中项
watch(() => props.items, () => {
  selectedIndex.value = 0
})

const selectItem = (index) => {
  const item = props.items[index]
  if (item) {
    props.command(item)
  }
}

// 暴露给父组件调用的键盘事件
const onKeyDown = ({ event }) => {
  if (event.key === 'ArrowUp') {
    selectedIndex.value = (selectedIndex.value + props.items.length - 1) % props.items.length
    return true
  }
  if (event.key === 'ArrowDown') {
    selectedIndex.value = (selectedIndex.value + 1) % props.items.length
    return true
  }
  if (event.key === 'Enter') {
    selectItem(selectedIndex.value)
    return true
  }
  return false
}

defineExpose({ onKeyDown })
</script>

<style lang="scss" scoped>
.command-list {
  background: #fff;
  border-radius: 6px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid #e2e8f0;
  overflow: hidden;
  padding: 6px;
  min-width: 200px;
  max-height: 300px;
  overflow-y: auto;

  .item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 8px 10px;
    cursor: pointer;
    border-radius: 4px;
    transition: background 0.1s;
    color: #37352f;

    &.is-selected {
      background: #eff6ff; /* Notion 蓝 */
    }

    .icon {
      font-size: 18px;
      width: 24px;
      text-align: center;
      flex-shrink: 0;
      color: #666;
    }

    .info {
      display: flex;
      flex-direction: column;
      
      .label { font-size: 14px; font-weight: 500; }
      // .desc { font-size: 12px; color: #999; }
    }
  }
  
  .empty {
    padding: 10px;
    font-size: 13px;
    color: #999;
    text-align: center;
  }
}
</style>