<template>
  <div class="meta-grid">
    <div class="md-input-box">
      <label>构图规格 // FORMAT</label>
      <input v-model="meta.canvas" type="text" placeholder="例: A4竖版, 1:1头像..." />
    </div>
    <div class="md-input-box">
      <label>指定色系 // PALETTE (HEX)</label>
      <input v-model="meta.color" type="text" placeholder="例: #FF0055, 黑白..." />
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