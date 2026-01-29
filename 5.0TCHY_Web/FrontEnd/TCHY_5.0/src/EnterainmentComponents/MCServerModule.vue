<template>
  <div class="mc-channel-layout">
    <div class="main-area custom-scroll">
      
      <div class="featured-banner">
        <div class="banner-bg"></div>
        <div class="banner-content">
          <span class="tag">FEATURED_SERVER // 推荐</span>
          <h1 class="title">太初寰宇 · MC频道区</h1>
          <p class="desc">
            <span class="ver">VER: 1.20.1</span>
            <span class="status">STATUS: STABLE</span>
          </p>
        </div>
      </div>

      <div class="notice-bar">
        <div class="notice-label">
          <span class="blink">●</span> LIVE_NOTICES
        </div>
        <div class="notice-scroller">
          <div class="scrolling-content">
            <span v-for="(notice, i) in notices" :key="i" class="notice-item">
              [{{ notice.type }}] {{ notice.content }}
            </span>
            <span v-for="(notice, i) in notices" :key="'dup-'+i" class="notice-item">
              [{{ notice.type }}] {{ notice.content }}
            </span>
          </div>
        </div>
      </div>

      <div class="broadcast-card">
        <div class="broadcast-header">
          <div class="title-group">
            <span class="icon">⚠</span>
            <span class="title">SYSTEM_BROADCAST // 全域广播</span>
          </div>
          <div class="meta-group">
            <span class="priority">PRIORITY: HIGH</span>
            <span class="date">2023-11-25</span>
          </div>
        </div>
        
        <div class="broadcast-content">
          <h3 class="sub-title">>> 关于“机械时代”赛季结算的通知</h3>
          <p class="text-body">
            各位干员请注意，当前赛季将于本月底进行结算。请尽快备份您的私人数据终端（整理背包）。
            新赛季将引入 <span class="highlight">Create: Above and Beyond</span> 核心玩法。
          </p>
          <ul class="bullet-list">
            <li>[+] 新增：由 ytonidc 提供的 CN2 优化线路</li>
            <li>[!] 警告：末地维度将于 24 小时后重置</li>
            <li>[-] 移除：旧版经济系统插件</li>
          </ul>
          <div class="signature">-- SERVER_ADMIN // 001</div>
        </div>
      </div>

      <div class="server-grid">
        <ServerCard 
          v-for="(server, idx) in serverList"
          :key="server.title"
          :index="idx + 1"
          v-bind="server"
          @download="(type) => handleDownload(type, server)"
        />
      </div>
    </div>

    <aside class="side-tools">
      <div class="tool-card terminal-card">
        <div class="card-header"><span class="icon">_</span> SYSTEM_INFO</div>
        <div class="terminal-body">
          <div class="row"><span class="label">NODE_LOC:</span><span class="val">CN_SHANGHAI</span></div>
          <div class="row"><span class="label">SEC_PROTOCOL:</span><span class="val">WAF_ACTIVE</span></div>
          <div class="row"><span class="label">ENCRYPTION:</span><span class="val">AES_256</span></div>
        </div>
      </div>

      <div class="tool-card java-card">
        <div class="card-header"><span class="icon">☕</span> JAVA_RUNTIME</div>
        <div class="java-body">
          <p class="hint">游戏启动需要 Java 17 环境</p>
          <button class="cyber-btn" @click="handleDownload('java')">
            <span class="btn-text">下载 JDK 17</span>
            <span class="btn-arrow">→</span>
          </button>
        </div>
      </div>

      <div class="tool-card log-card">
        <div class="card-header"><span class="icon">!</span> UPDATES</div>
        <div class="log-list">
          <div class="log-item">版本 1.20.1 核心模组已更新</div>
          <div class="log-item">客户端优化补丁已上线</div>
        </div>
      </div>
    </aside>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import ServerCard from './ServerCard.vue';

// 滚动公告数据
const notices = ref([
  { type: '公告', content: '服务器将于每周五凌晨 4:00 进行例行维护，请留意时间。' },
  { type: '活动', content: '新服“齿轮盛宴”开荒活动火热进行中，进群领取新手礼包！' },
  { type: '警告', content: '严禁在游戏内使用任何作弊客户端，违者封号处理。' }
]);

// 服务器数据
const serverList = ref([
  {
    title: '太初寰宇 · 自整合服',
    ip1: 'www.bianyuzhou.com:25565',
    ip2: '120.53.224.225:25565',
    version: 'Fabric 1.20.1',
    isOnline: true,
    coverImage: 'https://image2url.com/r2/default/images/1768800240160-f79ab1ee-64bc-43da-9a51-7977301da6de.blob',
    downloadLinks: {
      modpack: { url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=f98ee8ef3350405db3fd6fcb36a3a646', pwd: 'W1ST' },
      launcher: { url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=5868d1b8cbde4907bc29d750fbbd3c25', pwd: 'mBT8' }
    }
  },
  {
    title: '齿轮盛宴',
    ip1: 'p24.ytonidc.com:42134',
    ip2: '', // 单线演示
    version: 'Forge 1.20.1',
    isOnline: true,
    coverImage: 'https://image2url.com/r2/default/images/1769209536510-b6e46210-d3cd-4693-8adb-1d62a9450854.blob',
    downloadLinks: {
      modpack: { url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=c7dac1dd2eb84a178416c47a2b4f7fe7', pwd: '3JY6' },
      launcher: { url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=5868d1b8cbde4907bc29d750fbbd3c25', pwd: 'mBT8' }
    }
  }
]);

const handleDownload = async (type, server = null) => {
  if (type === 'java') {
    window.open('https://www.oracle.com/java/technologies/downloads/');
    return;
  }
  if (server && server.downloadLinks && server.downloadLinks[type]) {
    const { url, pwd } = server.downloadLinks[type];
    if (pwd) {
      try {
        await navigator.clipboard.writeText(pwd);
        alert(`>> 访问密码 [ ${pwd} ] 已复制`);
      } catch (err) { console.error(err); }
    }
    window.open(url);
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Anton&display=swap');

/* --- 基础布局 --- */
.mc-channel-layout { display: flex; gap: 20px; height: 100vh; overflow: hidden; padding: 20px; background: #eef; box-sizing: border-box; font-family: 'JetBrains Mono', monospace; }
.main-area { flex: 1; overflow-y: auto; padding-right: 10px; display: flex; flex-direction: column; gap: 20px; }

/* --- 顶部 Banner --- */
.featured-banner { position: relative; height: 180px; background: #111; border-radius: 12px; overflow: hidden; display: flex; align-items: center; padding: 0 40px; border: 1px solid #333; flex-shrink: 0; }
.banner-bg { position: absolute; inset: 0; background: linear-gradient(90deg, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.4) 100%), url('https://images.unsplash.com/photo-1598550476439-6847785fcea6?q=80&w=1000'); background-size: cover; background-position: center; z-index: 0; }
.banner-content { position: relative; z-index: 1; color: #fff; }
.tag { background: #D92323; padding: 2px 8px; font-size: 12px; font-weight: bold; letter-spacing: 1px; }
.title { font-family: 'Anton', sans-serif; font-size: 42px; margin: 10px 0; text-shadow: 0 4px 0 rgba(0,0,0,0.5); letter-spacing: 1px; }
.desc { color: #ccc; font-size: 14px; }
.desc span { margin-right: 15px; }

/* --- 滚动公告条 (Ticker) --- */
.notice-bar {
  display: flex; background: #111; border-radius: 6px; overflow: hidden;
  height: 40px; align-items: center; border: 1px solid #333;
  box-shadow: 0 4px 6px rgba(0,0,0,0.05); flex-shrink: 0;
}
.notice-label {
  background: #D92323; color: #fff; font-size: 12px; font-weight: bold;
  padding: 0 15px; height: 100%; display: flex; align-items: center; gap: 8px; z-index: 2;
}
.notice-scroller { flex: 1; overflow: hidden; position: relative; height: 100%; display: flex; align-items: center; background: #1a1a1a; }
.scrolling-content { display: flex; white-space: nowrap; animation: scroll-left 25s linear infinite; }
.notice-scroller:hover .scrolling-content { animation-play-state: paused; }
.notice-item { margin-right: 50px; font-size: 13px; color: #facc15; display: flex; align-items: center; }
@keyframes scroll-left { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }
.blink { animation: blinker 1.5s linear infinite; color: #fff; }
@keyframes blinker { 50% { opacity: 0; } }

/* --- 静态公告板 (Broadcast) --- */
.broadcast-card {
  background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;
  margin-bottom: 0; box-shadow: 0 4px 10px rgba(0,0,0,0.03); flex-shrink: 0;
}
.broadcast-header {
  background: #1a1a1a; color: #fff; padding: 10px 20px;
  display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #D92323;
}
.title-group { display: flex; align-items: center; gap: 10px; font-weight: bold; font-size: 14px; }
.title-group .icon { color: #D92323; font-size: 16px; }
.meta-group { display: flex; gap: 15px; font-size: 12px; }
.priority { color: #facc15; font-weight: bold; animation: pulse 2s infinite; }
.date { color: #666; }
.broadcast-content {
  padding: 20px; background: #fafafa;
  background-image: radial-gradient(#ddd 1px, transparent 1px); background-size: 20px 20px;
}
.sub-title { margin: 0 0 10px 0; font-size: 16px; color: #111; font-weight: bold; }
.text-body { font-size: 13px; color: #444; line-height: 1.6; margin-bottom: 12px; }
.highlight { background: #ffe4e6; color: #D92323; padding: 2px 4px; border-radius: 4px; font-weight: bold; }
.bullet-list { list-style: none; padding: 0; margin: 0; font-size: 13px; color: #555; }
.bullet-list li { margin-bottom: 6px; padding-left: 10px; border-left: 2px solid #ccc; }
.bullet-list li:nth-child(1) { border-color: #0f0; }
.bullet-list li:nth-child(2) { border-color: #facc15; }
.bullet-list li:nth-child(3) { border-color: #D92323; }
.signature { margin-top: 15px; text-align: right; font-size: 12px; color: #999; font-style: italic; }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

/* --- 列表与侧边栏 --- */
.server-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; padding-bottom: 40px; }
.side-tools { width: 280px; display: flex; flex-direction: column; gap: 20px; flex-shrink: 0; }
.tool-card { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
.card-header { background: #111; color: #fff; padding: 10px 15px; font-size: 13px; font-weight: bold; display: flex; align-items: center; gap: 8px; }
.card-header .icon { color: #D92323; }
.terminal-body { background: #1a1a1a; padding: 15px; color: #0f0; font-size: 13px; }
.terminal-body .row { display: flex; justify-content: space-between; margin-bottom: 8px; border-bottom: 1px dashed #333; padding-bottom: 4px; }
.terminal-body .label { color: #888; }
.java-body { padding: 15px; text-align: center; }
.java-body .hint { font-size: 12px; color: #666; margin-bottom: 12px; }
.cyber-btn { width: 100%; background: #D92323; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; display: flex; justify-content: space-between; transition: background 0.2s; }
.cyber-btn:hover { background: #b91c1c; }
.log-list { padding: 0; background: #fafafa; }
.log-item { padding: 10px 15px; border-bottom: 1px solid #eee; font-size: 12px; color: #555; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ccc; border-radius: 3px; }
</style>