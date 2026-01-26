<template>
  <div class="avatar-wrapper">
    <img :src="avatar" class="big-avatar" />
    <div class="level-pill">
      <span class="lv-txt">LV.</span>
      <span class="lv-num">{{ level }}</span>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

const props = defineProps({
  avatar: { 
    type: String, 
    default: 'https://image2url.com/r2/default/images/1769279786393-674d59cf-5620-4bde-9d31-c66661aa72f2.jpg' 
  },
  level: { type: Number, default: 0 }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');

.avatar-wrapper {
  position: relative;
  width: 130px; /* 保持大尺寸，外部可通过 transform 缩放复用 */
  height: 130px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.big-avatar {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  object-fit: cover;
  background: #f0f0f0;
}

/* 等级胶囊 */
.level-pill {
  position: absolute;
  bottom: 0px;
  background: #2c3e50;
  color: #fff;
  padding: 2px 10px;
  border-radius: 12px;
  border: 2px solid #fff;
  display: flex;
  align-items: baseline;
  gap: 2px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15);
  z-index: 2;
}

.lv-txt { 
  font-size: 10px; 
  opacity: 0.8; 
  font-family: 'Share Tech Mono', monospace; 
}

.lv-num { 
  font-size: 14px; 
  font-weight: bold; 
  font-family: 'Share Tech Mono', monospace; 
  color: #e67e22; 
}
</style>