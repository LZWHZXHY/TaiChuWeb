<template>
  <div class="json-tree">
    <div v-for="(val, key) in data" :key="key" class="prop-row">
      
      <template v-if="isObject(val)">
        <div class="prop-group">
          <div class="group-header" @click="toggle(key)">
            <span class="toggle-icon">{{ isOpen(key) ? '▼' : '▶' }}</span>
            <span class="key-label">{{ key }}</span>
          </div>
          <div v-if="isOpen(key)" class="group-body">
            <JsonTreeViewer :data="val" />
          </div>
        </div>
      </template>

      <template v-else>
        <div class="prop-item">
          <span class="key">{{ key }}:</span>
          <span class="val">{{ val }}</span>
        </div>
      </template>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  data: { type: Object, required: true }
})

// 默认展开所有一级
const openKeys = ref(Object.keys(props.data).reduce((acc, k) => {
  acc[k] = true
  return acc
}, {}))

const isObject = (val) => val !== null && typeof val === 'object' && !Array.isArray(val)

const isOpen = (key) => !!openKeys.value[key]
const toggle = (key) => { openKeys.value[key] = !openKeys.value[key] }
</script>

<style scoped>
.json-tree {
  font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
  font-size: 0.95rem;
}

/* 嵌套组 */
.prop-group {
  margin-bottom: 8px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.group-header {
  background: #f5f5f5;
  padding: 6px 10px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: bold;
  color: #333;
  user-select: none;
  transition: background 0.2s;
}
.group-header:hover { background: #e8e8e8; }

.toggle-icon { font-size: 0.7rem; color: #888; }

.group-body {
  padding: 10px 15px;
  background: #fff;
  border-top: 1px solid #e0e0e0;
}

/* 普通键值对 */
.prop-item {
  display: flex;
  padding: 6px 0;
  border-bottom: 1px dashed #eee;
}
.prop-item:last-child { border-bottom: none; }

.key {
  color: #666;
  font-weight: 600;
  margin-right: 10px;
  min-width: 80px; /* 对齐 */
}

.val {
  color: #111;
  word-break: break-all;
  line-height: 1.5;
}
</style>