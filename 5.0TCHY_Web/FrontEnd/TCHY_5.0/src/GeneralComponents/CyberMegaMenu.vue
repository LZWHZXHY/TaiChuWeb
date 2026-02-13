<template>
  <div class="mega-menu-trigger" @mouseenter="isOpen = true" @mouseleave="isOpen = false">
    <div class="trigger-label" :class="{ 'active': isOpen }">
      <span class="icon-prefix">◈</span>
      <span class="text">COMMUNITY // 社区</span>
      <div class="active-indicator"></div>
    </div>

    <Transition name="hologram-fade">
      <div v-show="isOpen" class="mega-panel-wrapper">
        <div class="panel-decoration-top"></div>
        
        <div class="mega-grid">
          <router-link 
            v-for="(item, index) in menuItems" 
            :key="index"
            :to="item.path"
            class="tactical-card"
            @click="isOpen = false"
          >
            <div class="card-bg-scanline"></div>
            <div class="card-inner">
              <div class="card-icon">{{ item.icon }}</div>
              <div class="card-text-group">
                <div class="card-code">{{ item.code }}</div>
                <div class="card-title">{{ item.title }}</div>
                <div class="card-desc">{{ item.desc }}</div>
              </div>
              <div class="card-arrow">>>></div>
            </div>
            <div class="corner-top-left"></div>
            <div class="corner-bottom-right"></div>
          </router-link>
        </div>

        <div class="panel-decoration-bottom">
          <span class="deco-text">SYSTEM_READY // WAITING_FOR_INPUT</span>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const isOpen = ref(false);

// 🛠️ 在这里配置你的菜单项，改这里就像改配置表一样简单
// 🛠️ 在这里配置你的菜单项
const menuItems = [
  {
    path: '/DataCenter', // 暂时指向原来的交流中枢
    icon: '⚡',
    code: 'SEC-01',
    title: 'NEXUS / 广场',
    desc: '实时信号流与碎片化信息'
  },
  {
    path: '/DataCenter', // 暂时也指向原来的交流中枢 (以后改成 /columns)
    icon: '📝',
    code: 'SEC-02',
    title: 'ARCHIVES / 专栏',
    desc: '深度解析与长文归档库'
  },
  {
    path: '/WorkCenter', // 指向原来的作品大厅
    icon: '🎨',
    code: 'SEC-03',
    title: 'VISUALS / 视界',
    desc: '光学传感器与艺术数据库'
  }
];
</script>

<style scoped>
/* --- 变量定义 --- */
.mega-menu-trigger {
  --red: #D92323;
  --black: #050505;
  --dark-gray: #151515;
  --light-gray: #888;
  --white: #F4F1EA;
  --font-mono: 'JetBrains Mono', monospace;
  
  position: relative;
  height: 100%;
  display: flex;
  align-items: center;
}

/* --- 1. 触发器样式 --- */
.trigger-label {
  padding: 0 20px;
  height: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-family: var(--font-mono);
  font-weight: bold;
  color: var(--light-gray);
  position: relative;
  transition: 0.3s;
}

.trigger-label:hover, .trigger-label.active {
  color: var(--white);
  background: rgba(255, 255, 255, 0.05);
}

.icon-prefix { font-size: 0.8rem; color: var(--red); }

/* 底部激活条 */
.active-indicator {
  position: absolute; bottom: 0; left: 0; width: 100%; height: 3px;
  background: var(--red);
  transform: scaleX(0); transition: transform 0.3s;
}
.trigger-label.active .active-indicator { transform: scaleX(1); }

/* --- 2. 面板容器样式 --- */
.mega-panel-wrapper {
  position: absolute;
  top: 100%; /* 紧贴导航栏底部 */
  left: 50%;
  transform: translateX(-50%); /* 水平居中 */
  width: 650px; /* 面板宽度 */
  background: rgba(10, 10, 10, 0.95);
  border: 1px solid #333;
  border-top: 3px solid var(--red); /* 顶部红线 */
  backdrop-filter: blur(15px); /* 磨砂玻璃 */
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.8);
  z-index: 2000;
  padding: 25px;
}

/* --- 3. 网格与卡片样式 --- */
.mega-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
}

.tactical-card {
  position: relative;
  background: var(--dark-gray);
  border: 1px solid #333;
  padding: 20px 15px;
  text-decoration: none;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

/* 卡片悬停特效 */
.tactical-card:hover {
  background: var(--white);
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(217, 35, 35, 0.15);
  border-color: var(--white);
}

/* 内部文字颜色反转 */
.tactical-card:hover .card-title,
.tactical-card:hover .card-code,
.tactical-card:hover .card-arrow { color: var(--black); }
.tactical-card:hover .card-desc { color: #333; }

/* 内容布局 */
.card-icon { font-size: 2.2rem; margin-bottom: 12px; }
.card-code { font-family: var(--font-mono); font-size: 0.6rem; color: var(--red); margin-bottom: 4px; letter-spacing: 1px; }
.card-title { font-family: 'Anton', sans-serif; font-size: 1.1rem; color: var(--white); letter-spacing: 0.5px; margin-bottom: 6px; }
.card-desc { font-family: var(--font-mono); font-size: 0.7rem; color: #666; line-height: 1.4; }
.card-arrow { margin-top: 15px; font-family: var(--font-mono); font-size: 0.8rem; color: #444; font-weight: bold; opacity: 0; transform: translateX(-10px); transition: 0.3s; }
.tactical-card:hover .card-arrow { opacity: 1; transform: translateX(0); }

/* 扫描线背景 */
.card-bg-scanline {
  position: absolute; inset: 0;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,0,0,0.2) 50%);
  background-size: 100% 4px;
  pointer-events: none; opacity: 0.3;
}

/* 装饰角标 */
.corner-top-left { position: absolute; top: 0; left: 0; width: 10px; height: 10px; border-top: 2px solid var(--red); border-left: 2px solid var(--red); transition: 0.3s; }
.corner-bottom-right { position: absolute; bottom: 0; right: 0; width: 10px; height: 10px; border-bottom: 2px solid var(--red); border-right: 2px solid var(--red); transition: 0.3s; }
.tactical-card:hover .corner-top-left, .tactical-card:hover .corner-bottom-right { width: 100%; height: 100%; opacity: 0.1; }

/* --- 底部装饰 --- */
.panel-decoration-bottom {
  margin-top: 20px;
  border-top: 1px dashed #333;
  padding-top: 5px;
  text-align: right;
}
.deco-text { font-family: var(--font-mono); font-size: 0.6rem; color: #444; }

/* --- 动画 --- */
.hologram-fade-enter-active, .hologram-fade-leave-active { transition: all 0.25s ease-out; }
.hologram-fade-enter-from, .hologram-fade-leave-to { opacity: 0; transform: translateX(-50%) translateY(-10px) scale(0.98); }
</style>