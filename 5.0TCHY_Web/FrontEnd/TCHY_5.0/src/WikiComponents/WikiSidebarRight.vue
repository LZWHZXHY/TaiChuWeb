<template>
  <aside class="sidebar-right">
    <div class="toc-container">
      <h4 class="toc-title">本页目录</h4>
      <ul class="toc-list">
        <li 
          v-for="heading in tocList" :key="heading.id"
          :class="['toc-item', `level-${heading.level}`, { active: activeId === heading.id }]"
          @click="$emit('scroll-to', heading.id)"
        >
          {{ heading.text }}
        </li>
      </ul>
    </div>
  </aside>
</template>

<script setup>
defineProps({ tocList: Array, activeId: String })
defineEmits(['scroll-to'])
</script>

<style scoped>
.sidebar-right { width: 240px; overflow-y: auto; border-left: 1px solid #e2e8f0; background: #fff; }
.toc-container { padding: 24px 16px; }
.toc-title { font-size: 12px; color: #64748b; margin-bottom: 12px; }
ul { list-style: none; padding: 0; margin: 0; }
.toc-item { padding: 6px 12px; border-radius: 6px; cursor: pointer; color: #64748b; font-size: 14px; }
.toc-item:hover { background: #f1f5f9; color: #0f172a; }
.toc-item.active { background: #eff6ff; color: #2563eb; font-weight: 500; }
.level-2 { padding-left: 24px; font-size: 13px; }
</style>