<template>
  <div class="ticker-wrapper">
    <div class="ticker-content">
      <span v-for="n in 10" :key="n">
        官网开发5.0 ing..... /// 欢迎来到太初寰宇！ /// 请给我们你的反馈和需求！ ///
      </span>
    </div>
  </div>
</template>

<style scoped>
.ticker-wrapper {
  width: 100%;
  background: #00F0FF;
  color: #000;
  font-family: 'JetBrains Mono';
  font-weight: bold;
  font-size: 14px;
  padding: 8px 0;
  overflow: hidden;
  margin: 40px 0;
  transform: rotate(-1deg) scale(1.05); /* 稍微倾斜，更有动感 */
}

.ticker-content {
  display: flex;
  white-space: nowrap;
  animation: scroll 20s linear infinite;
}

.ticker-content span { margin-right: 50px; }

@keyframes scroll {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}
</style>