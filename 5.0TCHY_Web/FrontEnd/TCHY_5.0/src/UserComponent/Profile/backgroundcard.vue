<template>
  <div class="background-card">
    <div class="side-panel left">
      <div class="decoration-column">
        <div class="hazard-stripe"></div>
        <div class="tech-block">
          <span class="label">SYS.L</span>
          <div class="status-light"></div>
        </div>
        <div class="vertical-line"></div>
        <div class="bottom-deco">
          <span class="tiny-text">CAUTION // 01</span>
        </div>
      </div>
    </div>

    <div class="center-texture">
      <div class="grid-line horizontal"></div>
      <div class="grid-line vertical"></div>
    </div>

    <div class="side-panel right">
      <div class="decoration-column">
        <div class="hazard-stripe"></div>
        <div class="tech-block">
          <div class="status-light red"></div>
          <span class="label">SYS.R</span>
        </div>
        <div class="vertical-line"></div>
        <div class="bottom-deco">
          <span class="tiny-text">NO.998-B</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 配色方案:
  主色调 (调暗的 #F3F1E9): #E2E0D6
  黑色 (文字/线条): #1A1A1A
  红色 (警示/强调): #CF2A2A
  白色 (高光): #FFFFFF
*/

.background-card {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 1; /* 确保在内容之下 */
  background-color: #E2E0D6; /* 基础工业米色 */
  overflow: hidden;
  pointer-events: none; /* 关键：让鼠标点击能穿透背景层，操作上层内容 */
  
  /* 添加一点微噪点质感，增加工业真实感 */
  background-image: 
    radial-gradient(circle at 50% 50%, transparent 90%, rgba(0,0,0,0.05) 100%),
    linear-gradient(0deg, rgba(0,0,0,0.02) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.02) 1px, transparent 1px);
  background-size: 100% 100%, 40px 40px, 40px 40px;
}

/* --- 通用侧边栏结构 --- */
.side-panel {
  position: absolute;
  top: 0;
  height: 100%;
  width: 15%; /* 严格控制宽度 */
  background: rgba(0, 0, 0, 0.02); /* 稍微加深侧边 */
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

.side-panel.left {
  left: 0;
  border-right: 2px solid #1A1A1A; /* 黑色分割线 */
}

.side-panel.right {
  right: 0;
  border-left: 2px solid #1A1A1A; /* 黑色分割线 */
}

/* 内部装饰容器 */
.decoration-column {
  width: 100%;
  height: 100%;
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 20px 0;
}

/* --- 顶部警示条 (黑黄/黑透 工业条纹) --- */
.hazard-stripe {
  height: 100px;
  width: 100%;
  opacity: 0.6;
  background: repeating-linear-gradient(
    45deg,
    #1A1A1A,
    #1A1A1A 2px,
    transparent 2px,
    transparent 8px
  );
  border-bottom: 1px solid #CF2A2A; /* 红色底边 */
}

/* --- 科技块 --- */
.tech-block {
  background: #1A1A1A;
  color: #E2E0D6;
  margin: 20px 10px;
  padding: 4px 8px;
  font-family: 'Courier New', Courier, monospace;
  font-weight: bold;
  font-size: 12px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  clip-path: polygon(0 0, 100% 0, 100% 70%, 90% 100%, 0 100%); /* 切角造型 */
}

.status-light {
  width: 8px;
  height: 8px;
  background-color: #E2E0D6;
  border-radius: 50%;
  box-shadow: 0 0 5px #E2E0D6;
}

.status-light.red {
  background-color: #CF2A2A;
  box-shadow: 0 0 5px #CF2A2A;
}

/* --- 垂直装饰线 --- */
.vertical-line {
  flex: 1;
  width: 1px;
  background: #1A1A1A;
  margin: 0 auto;
  opacity: 0.3;
}

/* --- 底部装饰 --- */
.bottom-deco {
  height: 15vw; /* 呼应头部的高度 */
  background: rgba(207, 42, 42, 0.05); /* 极淡的红色背景 */
  border-top: 1px solid #CF2A2A;
  display: flex;
  align-items: end;
  justify-content: center;
  padding-bottom: 20px;
}

.tiny-text {
  font-size: 10px;
  color: #1A1A1A;
  transform: rotate(-90deg); /* 竖排文字 */
  letter-spacing: 2px;
  opacity: 0.7;
  font-family: sans-serif;
  font-weight: 900;
}

/* --- 中间纹理微调 --- */
.center-texture {
  position: absolute;
  left: 15%;
  width: 70%;
  height: 100%;
}

/* 装饰性十字线 */
.grid-line {
  position: absolute;
  background-color: #CF2A2A; /* 红色细线 */
  opacity: 0.2;
}

.grid-line.horizontal {
  top: 30%;
  left: 0;
  width: 100%;
  height: 1px;
}

.grid-line.vertical {
  left: 50%;
  top: 0;
  height: 100%;
  width: 1px;
  border-left: 1px dashed #1A1A1A; /* 虚线 */
  background: transparent;
  opacity: 0.1;
}

/* 响应式调整：手机端稍微减少装饰复杂度 */
@media (max-width: 768px) {
  .side-panel {
    width: 40px; /* 手机端固定宽度，防止占用过多内容区 */
  }
  .center-texture {
    left: 40px;
    width: calc(100% - 80px);
  }
  .tech-block {
    display: none; /* 小屏隐藏部分细节 */
  }
}
</style>