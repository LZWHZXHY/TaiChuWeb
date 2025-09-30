<template>
  <div class="partical-card">
    <div class="partical-header">
      <div class="left">
        <div class="partical-icon">⚛️</div>
        <div>
          <h2 class="partical-title">{{ detail.title }}</h2>
          <p class="partical-subtitle">—— 宇宙中的特殊粒子设定</p>
        </div>
      </div>
      <div class="update-time" v-if="detail.time">
        <span>更新时间：</span>
        <span class="time-text">{{ detail.time }}</span>
      </div>
    </div>
    <div class="partical-content">
      <div v-for="(section, idx) in detail.sections" :key="idx" class="partical-section">
        <p v-html="section.replace(/\\n/g,'<br>')"></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import particalData from '../../assets/partical.json'
const detail = ref({})
onMounted(() => {
  detail.value = particalData
})
</script>

<style scoped>
.partical-card {
  background: linear-gradient(120deg, #343a60 60%, #3d485f 100%);
  border-radius: 18px;
  box-shadow: 0 4px 24px rgba(56, 67, 104, 0.32);
  padding: 36px 28px 24px 28px;
  margin: 0 auto;
  max-width: 720px;
  color: #f5f6fa;
  font-size: 1.14em;
  line-height: 2;
  border: 1.5px solid #49528550;
}
.partical-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}
.left {
  display: flex;
  align-items: center;
}
.partical-icon {
  font-size: 2.5rem;
  margin-right: 18px;
  filter: drop-shadow(0 2px 6px #ffd70080);
}
.partical-title {
  margin: 0;
  font-size: 2.1rem;
  font-weight: 700;
  letter-spacing: 2px;
  background: linear-gradient(90deg, #ffe66d, #fbc531 70%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.partical-subtitle {
  margin: 5px 0 0 0;
  font-size: 1rem;
  color: #a2a8c8;
  letter-spacing: 1px;
}
.update-time {
  font-size: 0.98em;
  color: #ffe66dbb;
  text-align: right;
  min-width: 120px;
}
.time-text {
  font-weight: bold;
  color: #ffd700;
  margin-left: 4px;
}
.partical-content {
  padding-top: 8px;
}
.partical-section {
  margin-bottom: 1.2em;
  padding-left: 8px;
  border-left: 3px solid #ffe66d22;
  transition: border-color 0.3s;
}
.partical-section:hover {
  border-left: 3px solid #ffd700;
  background: #2d3250aa;
}
@media (max-width: 600px) {
  .partical-card { padding: 18px 4px; }
  .partical-title { font-size: 1.3rem; }
  .update-time { font-size: 0.87em; }
  .partical-header { flex-direction: column; align-items: flex-start; }
}
</style>