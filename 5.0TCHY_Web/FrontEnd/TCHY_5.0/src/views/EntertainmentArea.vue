<template>
  <div class="doc-dashboard">
    <header class="doc-header">
      <div class="header-inner">
        <div class="brand">
          <div class="brand-glyph">太初</div>
          <h1 class="brand-title">娱乐枢纽 <span class="light">// 交互终端</span></h1>
        </div>
        
        <div class="header-status">
          <div class="node-badge">
            <span class="pulse-cyan"></span> 节点运行稳定
          </div>
          <div class="clock">{{ currentTime }}</div>
        </div>
      </div>
    </header>

    <div class="doc-ticker">
      <div class="ticker-content">
        【 系统提示：欢迎接入太初娱乐终端 // 核心资源已完成全域同步 // 当前频道：{{ currentChannel.label }} 】
      </div>
    </div>

    <div class="doc-main">
      <aside class="doc-sidebar">
        
        <section class="nav-section">
          <h3 class="section-label">频道索引</h3>
          <nav class="nav-list">
            <button 
              v-for="(item, index) in channelList" 
              :key="item.id"
              class="nav-item" 
              :class="{ active: currentId === item.id }"
              @click="switchChannel(item.id)"
            >
              <span class="item-no">0{{ index + 1 }}</span>
              <span class="item-name">{{ item.name }}</span>
              <div class="active-indicator"></div>
            </button>
          </nav>
        </section>

        <section class="nav-section">
          <h3 class="section-label">外部接入</h3>
          <div class="uplink-list">
            <a href="https://oopz.cn/i/iZDd74" target="_blank" class="uplink-card">
              <div class="up-content">
                <span class="up-title">群聊社区</span>
                <span class="up-desc">加入即时讨论</span>
              </div>
              <span class="up-icon">跳转</span>
            </a>
            <a href="https://kook.vip/oAj8SQ" target="_blank" class="uplink-card">
              <div class="up-content">
                <span class="up-title">语音频道</span>
                <span class="up-desc">高音质语音开黑</span>
              </div>
              <span class="up-icon">跳转</span>
            </a>
          </div>
        </section>

        <div class="sidebar-log">
          <div class="log-line">>> 引擎版本：V3.4.2</div>
          <div class="log-line">>> 权限级别：完全控制</div>
        </div>
      </aside>

      <main class="doc-content">
        <div class="canvas-area">
          <div class="canvas-header">
            <div class="page-info">
              <span class="folder-icon">📂</span>
              <span class="breadcrumb">档案库 / {{ currentChannel.label }}</span>
            </div>
            <div class="page-meta">页码：0{{ channelList.findIndex(c => c.id === currentId) + 1 }}</div>
          </div>

          <div class="component-mount">
            <Transition name="doc-slide" mode="out-in">
              <KeepAlive>
                <component :is="currentChannel.component" :key="currentId" />
              </KeepAlive>
            </Transition>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, markRaw } from 'vue';

// 引入子组件
import ChannelMinecraft from '@/EnterainmentComponents/MCServerModule.vue';
import Steam from '@/EnterainmentComponents/Steam.vue';
import trpg from '@/EnterainmentComponents/trpg.vue';

const currentTime = ref(new Date().toLocaleTimeString('zh-CN', { hour12: false }));
let clockTimer = null;

const channelList = [
  { id: 'mc', name: '我的世界服务器', label: '我的世界', component: markRaw(ChannelMinecraft) },
  { id: 'steam', name: '综合游戏资源', label: '综合游戏', component: markRaw(Steam) },
  { id: 'TRPG', name: '跑团大厅', label: '跑团', component: markRaw(trpg) }
];

const currentId = ref('mc');
const currentChannel = computed(() => {
  return channelList.find(c => c.id === currentId.value) || channelList[0];
});

const switchChannel = (id) => { currentId.value = id; };

onMounted(() => {
  clockTimer = setInterval(() => { 
    currentTime.value = new Date().toLocaleTimeString('zh-CN', { hour12: false }); 
  }, 1000);
});
onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
.doc-dashboard {
  --doc-bg: #F0F4F7;
  --doc-white: #FFFFFF;
  --doc-accent: #0891B2;
  --doc-accent-light: rgba(8, 145, 178, 0.05);
  --doc-text: #1F2937;
  --doc-border: #E5E7EB;
  --doc-sub: #6B7280;
  
  width: 100%;
  height: 100%;
  background-color: var(--doc-bg);
  color: var(--doc-text);
  font-family: 'PingFang SC', 'Microsoft YaHei', sans-serif;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  letter-spacing: 0.01em;
}

/* Header */
.doc-header {
  height: 60px;
  background: var(--doc-white);
  border-bottom: 1px solid var(--doc-border);
  padding: 0 30px;
  display: flex;
  align-items: center;
  z-index: 10;
}

.header-inner { width: 100%; display: flex; justify-content: space-between; align-items: center; }
.brand { display: flex; align-items: center; gap: 12px; }
.brand-glyph { 
  background: var(--doc-accent); color: white; 
  padding: 4px 8px; font-weight: 900; border-radius: 4px; font-size: 0.8rem;
}
.brand-title { font-size: 1.1rem; font-weight: 800; }
.brand-title .light { font-weight: 300; color: var(--doc-sub); font-size: 0.85rem; }

.header-status { display: flex; gap: 24px; align-items: center; }
.node-badge { font-size: 0.75rem; font-weight: 600; color: var(--doc-accent); display: flex; align-items: center; gap: 6px; }
.pulse-cyan { width: 6px; height: 6px; background: var(--doc-accent); border-radius: 50%; box-shadow: 0 0 8px var(--doc-accent); }
.clock { font-size: 0.95rem; color: var(--doc-sub); font-family: monospace; }

/* 通告栏 */
.doc-ticker {
  height: 30px; background: #F9FAFB; border-bottom: 1px solid var(--doc-border);
  display: flex; align-items: center; overflow: hidden;
}
.ticker-content { font-size: 0.75rem; color: var(--doc-sub); padding-left: 30px; }

/* 主布局 */
.doc-main { flex: 1; display: flex; overflow: hidden; padding: 20px 30px; gap: 20px; }

/* 侧边栏优化：变窄至 220px */
.doc-sidebar { width: 220px; display: flex; flex-direction: column; gap: 30px; flex-shrink: 0; }
.section-label { font-size: 0.7rem; font-weight: 800; color: var(--doc-sub); letter-spacing: 1.5px; margin-bottom: 12px; padding-left: 10px; }

.nav-list { display: flex; flex-direction: column; gap: 4px; }
.nav-item {
  position: relative; height: 44px; border: none; background: transparent;
  display: flex; align-items: center; padding: 0 15px; border-radius: 6px;
  cursor: pointer; transition: all 0.2s; color: var(--doc-sub);
}
.nav-item:hover { background: #E5E7EB; color: var(--doc-text); }
.nav-item.active { background: var(--doc-white); color: var(--doc-accent); box-shadow: 0 2px 8px rgba(0,0,0,0.06); }

.item-no { font-size: 0.65rem; margin-right: 12px; opacity: 0.5; font-family: monospace; }
.item-name { font-size: 0.85rem; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.active-indicator {
  position: absolute; left: 0; width: 3px; height: 16px; 
  background: var(--doc-accent); border-radius: 0 2px 2px 0;
  display: none;
}
.nav-item.active .active-indicator { display: block; }

/* 外部列表优化：单列垂直排列 */
.uplink-list { display: flex; flex-direction: column; gap: 8px; }
.uplink-card {
  background: var(--doc-white); border: 1px solid var(--doc-border); padding: 10px 12px;
  border-radius: 6px; text-decoration: none; display: flex; justify-content: space-between;
  align-items: center; transition: all 0.3s;
}
.uplink-card:hover { border-color: var(--doc-accent); background: var(--doc-accent-light); transform: translateX(3px); }
.up-content { display: flex; flex-direction: column; }
.up-title { font-size: 0.8rem; font-weight: 700; color: var(--doc-text); }
.up-desc { font-size: 0.65rem; color: var(--doc-sub); }
.up-icon { font-size: 0.7rem; color: var(--doc-accent); font-weight: 800; border: 1px solid var(--doc-accent); padding: 2px 6px; border-radius: 4px; }

.sidebar-log { 
  margin-top: auto; border-top: 1px dashed var(--doc-border); padding-top: 20px;
  font-size: 0.7rem; color: #9CA3AF; line-height: 1.6; font-family: monospace;
}

/* 内容视口 */
.doc-content { flex: 1; min-width: 0; display: flex; flex-direction: column; }
.canvas-area {
  flex: 1; background: var(--doc-white); border-radius: 8px;
  border: 1px solid var(--doc-border); box-shadow: 0 8px 24px rgba(0,0,0,0.02);
  position: relative; overflow: hidden; display: flex; flex-direction: column;
}

.canvas-header {
  height: 44px; border-bottom: 1px solid #F3F4F6; padding: 0 20px;
  display: flex; justify-content: space-between; align-items: center;
  background: #FAFAFA; flex-shrink: 0;
}
.page-info { display: flex; align-items: center; gap: 8px; }
.breadcrumb { font-size: 0.75rem; font-weight: 600; color: var(--doc-sub); }
.page-meta { font-size: 0.7rem; color: #BDC3C7; font-weight: 500; }

.component-mount { flex: 1; overflow-y: auto; padding: 0; }

/* 动画效果 */
.doc-slide-enter-active, .doc-slide-leave-active { transition: all 0.3s cubic-bezier(0.25, 1, 0.5, 1); }
.doc-slide-enter-from { opacity: 0; transform: translateY(10px); }
.doc-slide-leave-to { opacity: 0; transform: translateY(-10px); }

@media (max-width: 1024px) {
  .doc-main { padding: 15px; flex-direction: column; }
  .doc-sidebar { width: 100%; flex-direction: row; flex-wrap: wrap; }
  .nav-section { flex: 1; min-width: 200px; }
}
</style>