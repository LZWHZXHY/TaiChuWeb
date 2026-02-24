<template>
  <header class="global-header">
    <div class="logo">🌌 寰宇百科</div>
    <button class="search-trigger" @click="$emit('open-search')">
      <span class="icon">🔍</span> 搜索全站... <kbd>Ctrl K</kbd>
    </button>
    <div class="header-right">
      <button class="action-btn" @click="$emit('create')">写新词条</button>
    </div>
  </header>
</template>

<script setup>
defineEmits(['open-search', 'create'])
</script>

<style scoped>
.global-header { height: 60px; display: flex; align-items: center; justify-content: space-between; padding: 0 24px; border-bottom: 1px solid #e2e8f0; background: #fff; z-index: 10; }
.logo { font-size: 1.2rem; font-weight: 700; }
.search-trigger { display: flex; align-items: center; gap: 12px; background: #f1f5f9; border: 1px solid transparent; padding: 6px 12px; border-radius: 8px; width: 300px; color: #64748b; cursor: pointer; }
.header-right { display: flex; gap: 16px; align-items: center; }
.action-btn { background: #2563eb; color: white; border: none; padding: 8px 16px; border-radius: 6px; cursor: pointer; }
.avatar { width: 32px; height: 32px; border-radius: 50%; background: #94a3b8; color: white; display: flex; align-items: center; justify-content: center; }
</style>