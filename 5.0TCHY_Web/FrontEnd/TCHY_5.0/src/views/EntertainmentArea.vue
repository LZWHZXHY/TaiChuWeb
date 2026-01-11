<template>
  <div class="tc-dashboard">
    <header class="sys-info-bar">
      <span class="pulse">●</span>
      <span>TAICHU_HUB // ENTERTAINMENT_MODULE // {{ currentTime }}</span>
    </header>

    <div class="main-bridge">
      
      <aside class="left-column">
        <div class="section-label">SECTORS // 分区</div>
        <div class="nav-menu">
          <div class="nav-item active">
            <i class="fas fa-cube"></i> Minecraft 服务器
            <span class="status-dot online"></span>
          </div>
        </div>

        <div class="notice-box">
          <div class="notice-title"><i class="fas fa-bullhorn"></i> 公告</div>
          <p>Minecraft服务器的地图存档会不定期保存备份</p>
          <p>Minecraft服务器的mod也会不定期更新,如果你发现你突然进不去服务器了,可以尝试重新下载最新的整合包。</p>
          <p>有任何想法和问题可以进入QQ群进行交流:784251645</p>
        </div>
      </aside>

      <main class="mid-column custom-scroll">
        
        <div class="server-zone">
          
          <section class="server-monitor-card">
            <div class="server-bg"></div>
            <div class="server-content">
              <div class="server-header">
                <h2 class="server-name">太初寰宇 · Mod生存服</h2>
                <span class="version-tag">Ver: {{ mcServer.version }}</span>
              </div>
              
              <div class="server-metrics">
                <div class="metric">
                  <span class="label">STATUS</span>
                  <span class="value online">试运行中ing....</span>
                </div>
                <div class="metric">
                  <span class="label">PLAYERS</span>
                  <span class="value">{{ mcServer.online }}/{{ mcServer.max }}</span>
                </div>
                <div class="metric">
                  <span class="label">PING</span>
                  <span class="value green">unknown</span>
                </div>
              </div>

              <div class="ip-box">
                <span class="ip-address">{{ mcServer.ip }}</span>
                <button class="copy-btn" @click="copyIP">
                  <i class="far fa-copy"></i> 复制 IP
                </button>
              </div>
            </div>
          </section>

          <div class="section-label">RESOURCES // 资源下载</div>
          <div class="resource-grid">
            <div class="res-card primary">
              <div class="icon-box"><i class="fas fa-box-open"></i></div>
              <div class="res-info">
                <h4>太初整合包 (Modpack)</h4>
                <p>文件名: mods.7z | 大小: 61.25 MB</p>
              </div>
              <button class="download-btn" @click="downloadFile('modpack')">
                <i class="fas fa-download"></i> 点击下载
              </button>
            </div>

            <div class="res-card">
              <div class="icon-box"><i class="fab fa-java"></i></div>
              <div class="res-info">
                <h4>Java 运行环境</h4>
                <p>推荐 Java 17 (64位)</p>
              </div>
              <button class="download-btn secondary" @click="downloadFile('java')">下载</button>
            </div>

            <div class="res-card">
              <div class="icon-box"><i class="fas fa-rocket"></i></div>
              <div class="res-info">
                <h4>PCL2 启动器</h4>
                <p>推荐使用的第三方启动器</p>
              </div>
              <button class="download-btn secondary" @click="downloadFile('launcher')">下载</button>
            </div>
          </div>

          <div class="section-label">SERVER_INFO // 服务器简介</div>
          <div class="info-block">
            <p>欢迎来到太初寰宇官方服务器。请务必阅读以下规则：</p>
            <ul>
              <li>禁止任何形式的作弊 (X-ray, 飞行等)。</li>
              <li>禁止恶意破坏他人建筑。</li>
              <li>加入QQ群以了解更多信息：784251645 </li>
            </ul>
          </div>
        </div>

      </main>

      <aside class="right-column">
        </aside>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

// MC 服务器数据
const mcServer = ref({
  ip: 'play.bianyuzhou.com:25565',
  version: '1.20.1 (Fabric)',
  online: 0, // 暂时写死或等待接口对接
  max: 10
});

const copyIP = () => {
  navigator.clipboard.writeText(mcServer.value.ip).then(() => {
    alert('服务器 IP 已复制到剪贴板！快去游戏里连接吧！'); 
  });
};

const downloadFile = (type) => {
  if (type === 'modpack') {
    // 指向你服务器的真实文件路径
    const fileUrl = 'https://bianyuzhou.com/game/MC/mod/mods.7z';

    const link = document.createElement('a');
    link.href = fileUrl;
    link.setAttribute('download', '太初整合包_v2.1.7z'); 
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
  } else if (type === 'java') {
    window.open('https://bianyuzhou.com/game/MC/java/jdk-17.0.17_windows-x64_bin.exe');
  } else {
    // PCL2 下载地址
    window.open('https://bianyuzhou.com/game/MC/pcl2/Plain_Craft_Launcher2.exe');
  }
};

onMounted(() => {
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});
onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 全局容器 */
.tc-dashboard {
  --bg-main: #f8fafc;
  --panel-white: #ffffff;
  --accent-blue: #2563eb;
  --accent-green: #10b981;
  --accent-dark: #0f172a;
  --text-primary: #1e293b;
  --text-sub: #64748b;
  --border: #e2e8f0;
  
  width: 100%; height: 100vh; background-color: var(--bg-main);
  display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box;
}
* { box-sizing: border-box; }

.sys-info-bar { flex-shrink: 0; height: 36px; background: #fff; border-bottom: 1px solid var(--border); display: flex; align-items: center; padding: 0 25px; font-size: 11px; font-weight: bold; color: var(--text-sub); }
.main-bridge { flex: 1; display: flex; padding: 20px; gap: 20px; min-height: 0; }

/* 左侧导航 */
.left-column { width: 18%; display: flex; flex-direction: column; gap: 20px; }
.nav-menu { display: flex; flex-direction: column; gap: 5px; }
.nav-item {
  padding: 12px 15px; background: #fff; border-radius: 8px; border: 1px solid var(--border);
  cursor: pointer; display: flex; align-items: center; gap: 10px; font-size: 14px; color: var(--text-primary);
  transition: 0.2s; position: relative;
}
.nav-item:hover, .nav-item.active { background: var(--accent-dark); color: #fff; border-color: var(--accent-dark); }
.status-dot { width: 8px; height: 8px; border-radius: 50%; margin-left: auto; }
.status-dot.online { background: var(--accent-green); box-shadow: 0 0 5px var(--accent-green); }

.notice-box { background: #fff8c5; padding: 15px; border-radius: 8px; border: 1px solid #fde047; font-size: 12px; line-height: 1.5; color: #854d0e; }
.notice-title { font-weight: bold; margin-bottom: 5px; display: flex; align-items: center; gap: 5px; }

/* 中间内容区 */
.mid-column { width: 60%; display: flex; flex-direction: column; gap: 20px; overflow-y: auto; padding-right: 5px; }

/* --- MC 服务器卡片 --- */
.server-monitor-card {
  background: #1e293b; color: #fff; border-radius: 12px; overflow: hidden; position: relative;
  min-height: 200px; display: flex; flex-direction: column; justify-content: center;
}
.server-bg {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: url('https://www.minecraft.net/content/dam/games/minecraft/key-art/Games_Subnav_Minecraft-300x465.jpg') center/cover;
  opacity: 0.3; filter: blur(2px);
}
.server-content { position: relative; z-index: 2; padding: 30px; }
.server-header { display: flex; align-items: center; gap: 15px; margin-bottom: 20px; }
.server-name { margin: 0; font-size: 24px; font-weight: 800; text-shadow: 0 2px 4px rgba(0,0,0,0.5); }
.version-tag { background: rgba(255,255,255,0.2); padding: 2px 8px; border-radius: 4px; font-size: 12px; font-weight: bold; backdrop-filter: blur(4px); }

.server-metrics { display: flex; gap: 30px; margin-bottom: 25px; }
.metric { display: flex; flex-direction: column; }
.metric .label { font-size: 10px; opacity: 0.7; letter-spacing: 1px; }
.metric .value { font-size: 18px; font-weight: bold; font-family: monospace; }
.metric .value.online { color: var(--accent-green); }
.metric .value.green { color: var(--accent-green); }

.ip-box {
  display: flex; background: rgba(0,0,0,0.6); backdrop-filter: blur(5px);
  border-radius: 8px; overflow: hidden; max-width: 400px; border: 1px solid rgba(255,255,255,0.1);
}
.ip-address { flex: 1; padding: 12px 15px; font-family: monospace; font-size: 14px; display: flex; align-items: center; }
.copy-btn {
  border: none; background: var(--accent-blue); color: #fff; padding: 0 20px; cursor: pointer; font-weight: bold;
  transition: 0.2s;
}
.copy-btn:hover { background: #3b82f6; }

/* 资源下载网格 */
.resource-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; }
.res-card {
  background: #fff; border: 1px solid var(--border); border-radius: 10px; padding: 20px;
  display: flex; flex-direction: column; gap: 15px; text-align: center; align-items: center;
  transition: 0.2s;
}
.res-card:hover { transform: translateY(-3px); box-shadow: 0 10px 20px rgba(0,0,0,0.05); }
.res-card.primary { border-color: var(--accent-blue); background: #eff6ff; }
.icon-box { font-size: 32px; color: var(--text-primary); }
.res-card.primary .icon-box { color: var(--accent-blue); }
.res-info h4 { margin: 0 0 5px 0; font-size: 14px; }
.res-info p { margin: 0; font-size: 11px; color: var(--text-sub); }
.download-btn {
  width: 100%; padding: 8px; border-radius: 6px; border: none; cursor: pointer; font-weight: bold; font-size: 12px;
}
.res-card.primary .download-btn { background: var(--accent-blue); color: #fff; }
.res-card .download-btn.secondary { background: #e2e8f0; color: var(--text-primary); }
.res-card .download-btn:hover { filter: brightness(0.95); }

/* 服务器简介 */
.info-block { background: #fff; padding: 20px; border-radius: 10px; border: 1px solid var(--border); font-size: 13px; color: var(--text-primary); line-height: 1.6; }
.info-block ul { margin: 10px 0 0 20px; padding: 0; color: var(--text-sub); }

/* --- 右侧栏 --- */
.right-column { width: 22%; display: flex; flex-direction: column; gap: 20px; }

/* 通用 */
.section-label { font-size: 10px; font-weight: 800; color: #94a3b8; letter-spacing: 1.5px; margin-bottom: 10px; margin-top: 10px; }
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>