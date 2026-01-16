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
          <h2 class="cn-title">娱乐频道 // ENTERTAINMENT</h2>
          <div class="live-indicator"><span class="dot"></span> NODE_CONNECTED</div>
        </div>
        <div class="tech-lines">
          <div class="sys-time-display">{{ currentTime }}</div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // GAME_SERVER_SYNC: ACTIVE // DOWNLOAD_RESOURCES_BELOW // JOIN_COMMUNITY_QQ: 784251645 // 
          // GAME_SERVER_SYNC: ACTIVE // DOWNLOAD_RESOURCES_BELOW // JOIN_COMMUNITY_QQ: 784251645 // 
        </div>
      </div>

      <div class="main-bridge">
        
        <aside class="left-column">
          <div class="cyber-card">
            <div class="card-label-strip"><span>// CATEGORY_SELECT</span></div>
            <div class="nav-stack">
              <button class="nav-btn active">
                <span class="n-icon">MC</span>
                <span class="n-text">MINECRAFT_NODES</span>
              </button>
              <button class="nav-btn" disabled>
                <span class="n-icon">ST</span>
                <span class="n-text">STEAM_PROJECTS [LOCKED]</span>
              </button>
              <button class="nav-btn" disabled>
                <span class="n-icon">WB</span>
                <span class="n-text">WEB_GAMES [LOCKED]</span>
              </button>
            </div>
          </div>

          <div class="cyber-card announcement-panel">
            <div class="card-label-strip"><span>// LOG_FILES</span></div>
            <div class="card-inner-pad log-text">
              <p>> 地图存档已启动自动备份机制...</p>
              <p>> MOD列表不定期同步，请保持版本一致。</p>
            </div>
          </div>
        </aside>

        <main class="mid-column">
          <section class="activity-banner">
            <div class="banner-content">
              <div class="banner-badge">FEATURED_SERVER</div>
              <h2 class="banner-title">太初寰宇 · 生存服</h2>
              <p class="banner-sub">STATUS: TEST_RUNNING // VERSION: 1.20.1</p>
            </div>
            <div class="banner-pattern"></div>
          </section>

          <div class="server-grid">
            <McServerModule 
              title="太初寰宇 · Mod生存"
              ip="play.bianyuzhou.com:25565"
              version="1.20.1 (Fabric)"
              :online="mcStats.online"
              :max="mcStats.max"
              @download="handleDownload"
            />
            
            </div>
        </main>

        <aside class="right-column">
          <div class="cyber-terminal">
             <div class="terminal-header">
               <span>> GLOBAL_METRICS.exe</span>
               <span class="blink">_</span>
             </div>
             <div class="metrics-content">
                <div class="metric-row">
                  <span class="label">ACTIVE_PLAYERS:</span>
                  <span class="val">{{ mcStats.online }}</span>
                </div>
                <div class="metric-row">
                  <span class="label">WORLD_SIZE:</span>
                  <span class="val">1.2 GB</span>
                </div>
             </div>
          </div>

          <div class="cyber-card">
            <div class="card-label-strip"><span>// JAVA_RUNTIME</span></div>
            <div class="card-inner-pad">
              <button class="cyber-btn" @click="handleDownload('java')">
                <span class="btn-icon"><i class="fab fa-java"></i></span>
                <div class="btn-text">
                  <span class="main">环境下载</span>
                  <span class="sub">JAVA_17_RUNTIME</span>
                </div>
              </button>
            </div>
          </div>
        </aside>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import McServerModule from '@/EnterainmentComponents/MCServerModule.vue';

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

const mcStats = reactive({
  online: 0,
  max: 10
});

const handleDownload = (type) => {
  const urls = {
    modpack: 'https://bianyuzhou.com/game/MC/mod/mods.7z',
    java: 'https://bianyuzhou.com/game/MC/java/jdk-17.0.17_windows-x64_bin.exe',
    launcher: 'https://bianyuzhou.com/game/MC/pcl2/Plain_Craft_Launcher2.exe'
  };
  if (urls[type]) window.open(urls[type]);
};

onMounted(() => {
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});

onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 强制导入字体，确保工业感 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心：工业赛博全局变量 (这是视觉的基础) --- */
.cyber-dashboard { 
  --red: #D92323; 
  --black: #111111; 
  --off-white: #F4F1EA; 
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  --body: 'Inter', sans-serif;
  --gap: 20px;
  
  width: 100%; height: 100vh; 
  background-color: var(--off-white); 
  display: flex; flex-direction: column; overflow: hidden; 
  font-family: var(--body); color: var(--black);
}

/* --- 背景：移动网格 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

.panel-scroll-container { flex: 1; overflow-y: auto; position: relative; z-index: 1; }

/* --- 修复截图中的头部 (Main Header) --- */
.main-header { 
  display: flex; align-items: stretch; height: 120px;
  border-bottom: 4px solid var(--black); background: var(--off-white); position: relative; z-index: 10;
}
.header-split.left { 
  background: var(--black); color: var(--off-white); 
  width: 320px; display: flex; align-items: center; justify-content: center;
}
.giant-text { 
  font-family: var(--heading); font-size: 3rem; line-height: 0.9; 
  text-transform: uppercase; transform: rotate(-2deg); 
}
.text-row.red-fill { color: var(--red); -webkit-text-stroke: 1px var(--red); }

.info-block { padding: 20px; flex: 1; display: flex; flex-direction: column; justify-content: center; }
.cn-title { font-weight: 900; margin: 0 0 5px 0; font-size: 1.2rem; }
.live-indicator { 
  display: inline-flex; align-items: center; gap: 8px; font-family: var(--mono); 
  font-size: 0.8rem; color: var(--red); border: 1px solid var(--red); padding: 4px 8px; width: fit-content;
}
.dot { width: 8px; height: 8px; background: var(--red); border-radius: 50%; animation: pulse 1s infinite; }

/* --- 跑马灯 (Tech Strip) --- */
.tech-strip { 
  background: var(--black); color: var(--off-white); 
  height: 30px; border-bottom: 4px solid var(--black); 
  overflow: hidden; white-space: nowrap; font-family: var(--mono); font-size: 0.8rem; display: flex; align-items: center;
}
.strip-content { animation: marquee 30s linear infinite; padding-left: 100%; }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-100%); } }

/* --- 布局容器 --- */
.main-bridge { display: flex; gap: var(--gap); padding: var(--gap); align-items: flex-start; }
.left-column { flex: 0 0 280px; display: flex; flex-direction: column; gap: var(--gap); }
.mid-column { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--gap); }
.right-column { flex: 0 0 300px; }

/* --- 核心卡片 & 按钮 UI (修复截图中的简陋边框) --- */
:deep(.cyber-card) { 
  background: #fff; border: 2px solid var(--black); 
  box-shadow: 4px 4px 0 rgba(0,0,0,0.15); display: flex; flex-direction: column; overflow: hidden;
}
:deep(.card-label-strip) { background: var(--black); color: var(--off-white); padding: 4px 10px; font-family: var(--mono); font-size: 0.7rem; }

/* 导航按钮 */
.nav-btn {
  width: 100%; display: flex; align-items: center; border: none; background: transparent;
  padding: 15px; cursor: pointer; border-bottom: 1px solid var(--gray); font-family: var(--mono);
}
.nav-btn.active { background: #fff; border-left: 6px solid var(--red); font-weight: 800; }
.n-icon { width: 30px; height: 30px; background: var(--black); color: #fff; display: flex; align-items: center; justify-content: center; margin-right: 10px; }

/* Banner 修复 */
.activity-banner { 
  height: 160px; background: var(--black); color: #fff; 
  padding: 40px; position: relative; overflow: hidden; border: 2px solid var(--black);
}
.banner-badge { background: var(--red); padding: 2px 8px; font-family: var(--mono); font-size: 0.8rem; margin-bottom: 10px; width: fit-content; }
.banner-title { font-family: var(--heading); font-size: 2.5rem; margin: 0; }

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }
</style>