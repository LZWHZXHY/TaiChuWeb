<template>
  <div class="common-container">
    <div class="flashing-text">太初寰宇牛逼</div>
  </div>
</template>

<script setup>
// 这里目前不需要逻辑，留空即可
</script>

<style scoped>
/* 容器样式：占满父盒子，并强制居中 */
.common-container {
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center; /* 水平居中 */
  align-items: center;     /* 垂直居中 */
  background-color: #ffffff; /* 保持背景干净 */
}

/* 文字样式与动画 */
.flashing-text {
  font-size: 48px;         /* 特大字号 */
  font-weight: 900;        /* 特粗字体 */
  letter-spacing: 4px;     /* 字间距，显得更有气势 */
  color: #333;             /* 初始颜色 */
  
  /* 动画设置：
     name: pulse
     duration: 1.5s (一次循环1.5秒)
     iteration-count: infinite (无限循环)
     timing-function: ease-in-out (缓动，更自然)
     direction: alternate (一来一回，避免突变)
  */
  animation: pulse 1.5s infinite ease-in-out alternate;
}

/* 定义动画关键帧 */
@keyframes pulse {
  0% {
    opacity: 1;            /* 完全不透明 */
    transform: scale(1);   /* 原始大小 */
    text-shadow: 0 0 10px rgba(0,0,0,0.1);
  }
  100% {
    opacity: 0.3;          /* 变淡 */
    transform: scale(0.95);/* 稍微缩小一点，增加呼吸感 */
    color: #1890ff;        /* 变色 (可选，增加科技感) */
  }
}
</style>