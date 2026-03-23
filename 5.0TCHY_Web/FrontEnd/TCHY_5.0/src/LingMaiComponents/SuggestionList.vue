<template>
  <div class="lingmai-suggestion">
    <div v-if="items.length" class="list">
      <button
        v-for="(item, index) in items"
        :key="item.id"
        class="item"
        :class="{ 
          'is-active': index === selectedIndex,
          'type-block': item.type === 'block',
          'type-note': item.type === 'note'
        }"
        @click="selectItem(index)"
      >
        <span class="icon">
          {{ item.type === 'block' ? '📑' : '🌌' }}
        </span>

        <div class="item-info">
          <span class="label">{{ item.label }}</span>
          
          <span v-if="item.type === 'block' && item.noteTitle" class="source-hint">
            来自: {{ item.noteTitle }}
          </span>
        </div>
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

// 当搜索结果变化时，重置选中项到第一个
watch(() => props.items, () => {
  selectedIndex.value = 0
})

const selectItem = (index) => {
  const item = props.items[index]
  if (item) {
    props.command(item)
  }
}

// 处理 Tiptap 传来的键盘事件
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
.lingmai-suggestion {
  background: #ffffff;
  border-radius: 10px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15), 0 2px 10px rgba(0, 0, 0, 0.05);
  border: 1px solid #e2e8f0;
  padding: 6px;
  min-width: 280px;
  max-width: 420px;
  
  /* --- 核心修复：开启弹性布局并限制整体高度 --- */
  display: flex;
  flex-direction: column;
  max-height: 380px; // 🔥 必须设置最大高度，超过此高度将触发滚动
  outline: none;

  .list {
    display: flex;
    flex-direction: column;
    gap: 2px;
    /* --- 核心修复：允许内部列表垂直滚动 --- */
    flex: 1;
    overflow-y: auto; 
    overflow-x: hidden;
    padding-right: 2px; // 为滚动条留出微小间距

    /* 💡 美化滚动条，使其符合灵脉空间的精致风格 */
    &::-webkit-scrollbar {
      width: 5px;
    }
    &::-webkit-scrollbar-track {
      background: transparent;
    }
    &::-webkit-scrollbar-thumb {
      background: #e2e8f0;
      border-radius: 10px;
      &:hover {
        background: #cbd5e1;
      }
    }
  }

  .item {
    display: flex;
    align-items: flex-start;
    width: 100%;
    padding: 10px 12px;
    gap: 12px;
    border: none;
    background: transparent;
    border-radius: 8px;
    cursor: pointer;
    text-align: left;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    flex-shrink: 0; // 防止按钮在 flex 容器中被压缩

    .icon { 
      font-size: 1.2rem; 
      margin-top: 2px;
      flex-shrink: 0;
    }

    .item-info {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0; // 关键：允许子元素截断文本

      .label { 
        color: #334155; 
        font-size: 0.95rem; 
        line-height: 1.5;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis; // 超长内容显示省略号
      }

      .source-hint {
        font-size: 0.75rem;
        color: #94a3b8;
        margin-top: 4px;
      }
    }

    /* 选中状态交互 */
    &.is-active {
      background: #f1f5f9;
      transform: translateX(2px); // 选中的位移反馈

      .label { 
        color: #2563eb; 
        font-weight: 600; 
      }
      
      &.type-block {
        background: #fffbeb; 
        border-left: 3px solid #f59e0b;
        padding-left: 9px; // 补偿边框宽度
        .label { color: #d97706; }
      }
    }

    &:hover:not(.is-active) {
      background: #f8fafc;
    }
  }

  .no-result {
    padding: 20px;
    color: #94a3b8;
    font-size: 0.9rem;
    text-align: center;
    font-style: italic;
  }
}
</style>