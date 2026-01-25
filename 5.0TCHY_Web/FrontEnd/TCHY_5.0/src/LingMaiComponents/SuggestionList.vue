<template>
  <div class="lingmai-suggestion">
    <div v-if="items.length" class="list">
      <button
        v-for="(item, index) in items"
        :key="item.id"
        class="item"
        :class="{ 'is-active': index === selectedIndex }"
        @click="selectItem(index)"
      >
        <span class="icon">🌌</span>
        <span class="label">{{ item.label }}</span>
      </button>
    </div>
    <div v-else class="no-result">
      未在太初寰宇中找到相关灵脉...
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  items: { type: Array, required: true },
  command: { type: Function, required: true },
})

const selectedIndex = ref(0)

// 重置选中项
watch(() => props.items, () => {
  selectedIndex.value = 0
})

const selectItem = (index) => {
  const item = props.items[index]
  if (item) {
    props.command(item)
  }
}

// 供父组件 Tiptap 调用，处理键盘上下键和回车
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

// 必须暴露给父组件
defineExpose({ onKeyDown })
</script>

<style lang="scss" scoped>
.lingmai-suggestion {
  background: #ffffff;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1), 0 2px 5px rgba(0, 0, 0, 0.05);
  border: 1px solid #e2e8f0;
  overflow: hidden;
  padding: 6px;
  min-width: 240px;

  .item {
    display: flex;
    align-items: center;
    width: 100%;
    padding: 8px 12px;
    gap: 10px;
    border: none;
    background: transparent;
    border-radius: 6px;
    cursor: pointer;
    text-align: left;
    transition: all 0.2s;

    .icon { font-size: 1.1rem; }
    .label { color: #334155; font-size: 0.95rem; }

    &.is-active {
      background: #f1f5f9;
      .label { color: #2563eb; font-weight: 500; }
    }
    &:hover { background: #f8fafc; }
  }

  .no-result {
    padding: 12px;
    color: #94a3b8;
    font-size: 0.9rem;
    text-align: center;
  }
}
</style>