<template>
  <div class="meta-grid">
    <div class="md-input-box">
      <label>世界观纪元 // ERA</label>
      <input v-model="meta.era" type="text" placeholder="例: 赛博朋克2077, 维多利亚时代" />
    </div>
    <div class="md-input-box">
      <label>字数下限 // MIN_WORDS</label>
      <input v-model="meta.wordCount" type="text" placeholder="例: 单次提交不少于1000字" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  meta: { type: Object, required: true }
})
</script>

<style scoped>
/* 同上 */
.meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0 20px; }
.md-input-box { margin-bottom: 20px; flex: 1; }
.md-input-box label { font-size: 10px; font-weight: 900; color: #888; margin-bottom: 8px; display: block; text-transform: uppercase; }
.md-input-box input { width: 100%; box-sizing: border-box; border: 2px solid #121212; background: #fff; padding: 12px; font-family: inherit; font-size: 14px; transition: 0.2s; }
</style>