<template>
  <div class="warning-container">
    <div class="content-wrapper">
      <img 
        src="https://image2url.com/r2/default/images/1769474422869-bb762f83-9cdb-41f8-b3ea-ce2bb8add47a.gif" 
        class="construction-gif" 
        draggable="false"
      />
      
      <div class="text-box">
        <h1 class="rainbow-text">没做完！</h1>
        <h1 class="rainbow-text delayed">滚出切！！</h1>
      </div>
    </div>
  </div>
</template>

<script setup>
// 没有任何逻辑，纯粹的情绪输出
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@900&display=swap');

.warning-container {
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #fff; /* 或者 transparent，看你喜好 */
  overflow: hidden;
}

.content-wrapper {
  display: flex;
  align-items: center;
  gap: 20px; /* 图片和文字的间距 */
  padding: 40px;
}

.construction-gif {
  height: 200px; /* 限制高度，防止图片过大撑爆布局 */
  object-fit: contain;
}

.text-box {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  justify-content: center;
  line-height: 1.2;
}

.rainbow-text {
  margin: 0;
  font-family: 'Noto Sans SC', sans-serif;
  font-size: 64px; /* 超大字号 */
  font-weight: 900;
  white-space: nowrap;
  
  /* 五彩流光 + 疯狂闪烁动画 */
  background: linear-gradient(90deg, #ff0000, #ff7f00, #ffff00, #00ff00, #00ffff, #0000ff, #8b00ff);
  background-size: 200% auto;
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  
  animation: 
    rainbow-flow 2s linear infinite,
    angry-shake 0.5s ease-in-out infinite alternate;
  
  text-shadow: 4px 4px 0px rgba(0,0,0,0.1);
}

.rainbow-text.delayed {
  font-size: 80px; /* 第二行更大更凶 */
  animation-delay: 0.1s; /* 稍微错开一点节奏 */
}

/* 背景流光动画 */
@keyframes rainbow-flow {
  to {
    background-position: 200% center;
  }
}

/* 愤怒颤抖/缩放动画 */
@keyframes angry-shake {
  0% { transform: scale(1) rotate(0deg); }
  25% { transform: scale(1.1) rotate(-2deg); }
  50% { transform: scale(1) rotate(0deg); }
  75% { transform: scale(1.1) rotate(2deg); }
  100% { transform: scale(1) rotate(0deg); }
}
</style>