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

// --- 1. 数据配置：增加密码字段 ---
const serverList = ref([
  {
    title: '太初寰宇 · 自整合服',
    ip: 'frp-fly.com:24147',
    version: '1.20.1 Fabric',
    isOnline: true,
    coverImage: 'https://image2url.com/r2/default/images/1768800240160-f79ab1ee-64bc-43da-9a51-7977301da6de.blob',
    downloadLinks: {
      modpack: {
        url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=f98ee8ef3350405db3fd6fcb36a3a646',
        pwd: 'W1ST' // 专属密码
      },
      launcher: {
        url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=5868d1b8cbde4907bc29d750fbbd3c25',
        pwd: 'mBT8'
      }
    }
  },
  {
    title: '齿轮盛宴',
    ip: 'p24.ytonidc.com:42134',
    version: '1.20.1 Forge',
    isOnline: true,
    coverImage: 'https://images.unsplash.com/photo-1599933591462-8411eb11359c?q=80&w=600&auto=format&fit=crop',
    downloadLinks: {
      modpack: {
        url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=c7dac1dd2eb84a178416c47a2b4f7fe7',
        pwd: '3JY6' // 另一个服务器的专属密码
      },
      launcher: {
        url: 'https://ug.link/TaiChuHuanYu/filemgr/share-download/?id=5868d1b8cbde4907bc29d750fbbd3c25',
        pwd: 'mBT8'
      }
    }
  }
]);

// --- 2. 处理函数：智能识别链接与密码 ---
const handleDownload = async (type, server = null) => {
  // 处理全局 Java 下载
  if (type === 'java') {
    window.open('https://www.oracle.com/java/technologies/downloads/');
    return;
  }

  // 处理服务器特定下载
  if (server && server.downloadLinks && server.downloadLinks[type]) {
    const { url, pwd } = server.downloadLinks[type];

    if (pwd) {
      try {
        await navigator.clipboard.writeText(pwd);
        // 这里你可以换成更漂亮的全局通知组件
        alert(`>> 正在前往下载中心\n>> 访问密码 [ ${pwd} ] 已复制到剪贴板`);
      } catch (err) {
        console.error('无法复制密码:', err);
      }
    }
    
    // 打开网盘地址
    window.open(url);
  } else {
    console.error('未找到有效的下载链接配置');
  }
};
</script>

<style scoped>
/* 此处保留您之前的全部 CSS，不作改动 */
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Anton&display=swap');

.mc-channel-layout { display: flex; gap: 20px; height: 100%; overflow: hidden; padding-right: 5px; }
.main-area { flex: 1; overflow-y: auto; padding-right: 10px; display: flex; flex-direction: column; gap: 24px; }
.featured-banner { position: relative; height: 180px; background: #111; border-radius: 12px; overflow: hidden; display: flex; align-items: center; padding: 0 40px; border: 1px solid #333; flex-shrink: 0; }
.banner-bg { position: absolute; inset: 0; background: linear-gradient(90deg, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.4) 100%), url('https://images.unsplash.com/photo-1598550476439-6847785fcea6?q=80&w=1000'); background-size: cover; background-position: center; z-index: 0; }
.banner-content { position: relative; z-index: 1; color: #fff; }
.featured-banner .tag { background: #D92323; padding: 2px 8px; font-size: 12px; font-weight: bold; font-family: 'JetBrains Mono'; letter-spacing: 1px; }
.featured-banner .title { font-family: 'Anton', sans-serif; font-size: 42px; margin: 10px 0; text-shadow: 0 4px 0 rgba(0,0,0,0.5); letter-spacing: 1px; }
.featured-banner .desc { font-family: 'JetBrains Mono'; color: #ccc; font-size: 14px; }
.featured-banner .desc span { margin-right: 15px; }
.server-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; padding-bottom: 40px; }
.side-tools { width: 280px; display: flex; flex-direction: column; gap: 20px; flex-shrink: 0; }
.tool-card { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
.card-header { background: #111; color: #fff; padding: 10px 15px; font-family: 'JetBrains Mono'; font-size: 13px; font-weight: bold; display: flex; align-items: center; gap: 8px; }
.card-header .icon { color: #D92323; }
.terminal-card { border-color: #111; }
.terminal-body { background: #1a1a1a; padding: 15px; font-family: 'JetBrains Mono'; color: #0f0; font-size: 13px; }
.terminal-body .row { display: flex; justify-content: space-between; margin-bottom: 8px; border-bottom: 1px dashed #333; padding-bottom: 4px; }
.terminal-body .row:last-child { border: none; margin: 0; }
.terminal-body .label { color: #888; }
.text-green { color: #0f0; text-shadow: 0 0 5px rgba(0,255,0,0.5); }
.java-body { padding: 15px; text-align: center; }
.java-body .hint { font-size: 12px; color: #666; margin-bottom: 12px; }
.cyber-btn { width: 100%; background: #D92323; color: #fff; border: none; padding: 10px; border-radius: 4px; cursor: pointer; font-weight: bold; display: flex; justify-content: space-between; padding: 10px 20px; transition: background 0.2s; }
.cyber-btn:hover { background: #b91c1c; }
.log-list { padding: 0; background: #fafafa; }
.log-item { padding: 10px 15px; border-bottom: 1px solid #eee; font-size: 12px; color: #555; font-family: 'JetBrains Mono'; }
.log-item .time { color: #999; margin-right: 5px; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ccc; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #999; }
</style>