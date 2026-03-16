<template>
  <div class="meta-grid">
    <div class="md-input-box">
      <label>帧率要求 // FPS</label>
      <input v-model="meta.fps" type="text" placeholder="例: 24, 30, 60..." />
    </div>
    <div class="md-input-box">
      <label>画布尺寸 // CANVAS_SIZE</label>
      <input v-model="meta.canvas" type="text" placeholder="例: 1920x1080 (16:9)" />
    </div>
    <div class="md-input-box" style="grid-column: span 2;">
      <label>动画主体/内容限定 // SUBJECT</label>
      <input v-model="meta.subject" type="text" placeholder="例: 必须包含火柴人使用冷兵器战斗的镜头..." />
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