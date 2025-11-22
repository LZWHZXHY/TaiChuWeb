<template>
  <div class="fb-page">
    <div class="fb-container">
      <FeedbackBox />
    </div>
  </div>
</template>

<script setup lang="ts">
import FeedbackBox from '@/feedbackComponents/FeedbackBox.vue'
</script>

<style scoped>
/* 页面背景保持，与内容容器分离 */
.fb-page {
  background: var(--bg, #ffffff);
  min-height: 100vh;
}

/* 内容容器：居中 + 限宽 + 自适应左右间距 */
.fb-container {
  max-width: 1180px;
  margin: 0 auto;
  /* 上下固定，下左右使用 clamp 做响应式：最少 1rem，随视口增长到最多 3.5rem */
  padding: 40px clamp(1rem, 4vw, 3.5rem) 80px;
  box-sizing: border-box;
  background-color: rgb(255, 255, 255);
}

/* 小屏进一步减小下边距避免过多留白 */
@media (max-width: 640px) {
  .fb-container {
    padding: 28px 1.1rem 60px;
  }
}
</style>