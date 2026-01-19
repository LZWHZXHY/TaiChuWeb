<template>
  <div class="cyber-dashboard">
    <div class="grid-bg moving-grid"></div>

    <div class="panel-scroll-container custom-scroll">
      
      <header class="main-header">
        <div class="header-split left">
          <h1 class="giant-text glitch-hover">
            <div class="text-row">TC_HUB</div>
            <div class="text-row red-fill">娱乐中心</div>
          </h1>
        </div>
        <div class="info-block">
          <h2 class="cn-title">太初终端 // {{ currentChannel.label }}</h2>
          <div class="live-indicator"><span class="dot"></span> NODE_CONNECTED</div>
        </div>
        <div class="tech-lines">
          <div class="sys-time-display">{{ currentTime }}</div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // SYSTEM_NOTICE: 欢迎接入太初寰宇娱乐终端 // 全局资源同步正常 // 如遇连接中断请联系管理员 // 
        </div>
      </div>

      <div class="main-bridge">
        
        <aside class="left-column">
          <div class="cyber-card">
            <div class="card-label-strip"><span>// CHANNEL_SELECT</span></div>
            <div class="nav-stack">
              <button 
                v-for="item in channelList" 
                :key="item.id"
                class="nav-btn"
                :class="{ active: currentId === item.id }"
                @click="switchChannel(item.id)"
              >
                <span class="n-icon">{{ item.icon }}</span>
                <span class="n-text">{{ item.name }}</span>
              </button>
            </div>
          </div>

          <div class="cyber-card announcement-panel">
            <div class="card-label-strip"><span>// SYS_LOG</span></div>
            <div class="card-inner-pad log-text">
              <p>> 核心模块已加载...</p>
              <p>> 渲染引擎: VUE_3_CORE</p>
            </div>
          </div>
        </aside>

        <div class="content-viewport">
          <Transition name="cyber-fade" mode="out-in">
            <KeepAlive>
              <component :is="currentChannel.component" />
            </KeepAlive>
          </Transition>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, markRaw } from 'vue';

// 引入子组件
import ChannelMinecraft from '@/EnterainmentComponents/MCServerModule.vue';
import Steam from '@/EnterainmentComponents/Steam.vue';
// 如果其他频道还没写好，暂时都指向 MC 组件以防止报错
// import ChannelSteam from './channels/ChannelSteam.vue'; 

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

// --- ⚠️ 修复关键：去掉 shallowRef，改用 markRaw 或直接赋值 ---
const channelList = [
  { 
    id: 'mc', 
    name: 'MINECRAFT_NODE', 
    icon: 'MC', 
    label: '我的世界', 
    // 错误写法: component: shallowRef(ChannelMinecraft)
    // 正确写法: 直接赋值，或者用 markRaw 标记为非响应式
    component: markRaw(ChannelMinecraft) 
  },
  { 
    id: 'steam', 
    name: 'STEAM_GAMES', 
    icon: 'ST', 
    label: '综合游戏', 
    // 暂时用 MC 占位
    component: markRaw(Steam) 
  }
  
];

const currentId = ref('mc');

const currentChannel = computed(() => {
  return channelList.find(c => c.id === currentId.value) || channelList[0];
});

const switchChannel = (id) => {
  currentId.value = id;
};

onMounted(() => {
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});

onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 样式保持不变，直接复用之前的 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

.cyber-dashboard { 
  --red: #D92323; --black: #111111; --off-white: #F4F1EA; --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif; --body: 'Inter', sans-serif;
  --gap: 20px;
  width: 100%; height: 100vh; background-color: var(--off-white); 
  display: flex; flex-direction: column; overflow: hidden; font-family: var(--body); color: var(--black);
}

.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

.panel-scroll-container { flex: 1; overflow-y: hidden; position: relative; z-index: 1; display: flex; flex-direction: column; }

.main-header { 
  display: flex; align-items: stretch; height: 100px; flex-shrink: 0;
  border-bottom: 4px solid var(--black); background: var(--off-white); position: relative; z-index: 10;
}
.header-split.left { 
  background: var(--black); color: var(--off-white); 
  width: 320px; display: flex; align-items: center; justify-content: center;
}
.giant-text { 
  font-family: var(--heading); font-size: 2.8rem; line-height: 0.9; 
  text-transform: uppercase; transform: rotate(-2deg); 
}
.text-row.red-fill { color: var(--red); -webkit-text-stroke: 1px var(--red); }

.info-block { padding: 0 20px; flex: 1; display: flex; flex-direction: column; justify-content: center; }
.cn-title { font-weight: 900; margin: 0 0 5px 0; font-size: 1.4rem; }
.live-indicator { 
  display: inline-flex; align-items: center; gap: 8px; font-family: var(--mono); 
  font-size: 0.8rem; color: var(--red); border: 1px solid var(--red); padding: 2px 8px; width: fit-content;
}
.dot { width: 8px; height: 8px; background: var(--red); border-radius: 50%; animation: pulse 1s infinite; }
.sys-time-display { font-family: var(--mono); font-size: 1.2rem; font-weight: bold; padding: 0 20px; line-height: 100px; }

.tech-strip { 
  background: var(--black); color: var(--off-white); 
  height: 30px; border-bottom: 4px solid var(--black); flex-shrink: 0;
  overflow: hidden; white-space: nowrap; font-family: var(--mono); font-size: 0.8rem; display: flex; align-items: center;
}
.strip-content { animation: marquee 30s linear infinite; padding-left: 100%; }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-100%); } }

.main-bridge { display: flex; gap: var(--gap); padding: var(--gap); flex: 1; min-height: 0; }
.left-column { flex: 0 0 260px; display: flex; flex-direction: column; gap: var(--gap); }
.content-viewport { flex: 1; min-width: 0; position: relative; display: flex; flex-direction: column; }

.cyber-card { 
  background: #fff; border: 2px solid var(--black); 
  box-shadow: 4px 4px 0 rgba(0,0,0,0.15); display: flex; flex-direction: column; 
}
.card-label-strip { background: var(--black); color: var(--off-white); padding: 4px 10px; font-family: var(--mono); font-size: 0.7rem; }
.nav-stack { display: flex; flex-direction: column; }
.nav-btn {
  width: 100%; display: flex; align-items: center; border: none; background: transparent;
  padding: 18px 15px; cursor: pointer; border-bottom: 1px solid var(--gray); font-family: var(--mono);
  font-size: 0.9rem; transition: all 0.2s;
}
.nav-btn:hover { background: rgba(0,0,0,0.05); padding-left: 20px; }
.nav-btn.active { background: #fff; border-left: 6px solid var(--red); font-weight: 800; padding-left: 20px; }
.n-icon { width: 24px; display: inline-block; font-weight: bold; margin-right: 10px; color: var(--red); }
.log-text { padding: 15px; font-family: var(--mono); font-size: 0.75rem; line-height: 1.6; color: #666; }

.cyber-fade-enter-active, .cyber-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.cyber-fade-enter-from { opacity: 0; transform: translateX(10px); }
.cyber-fade-leave-to { opacity: 0; transform: translateX(-10px); }

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }
</style>