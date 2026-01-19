<template>
  <div class="steam-simple-layout">
    <div class="hero-bg" :style="{ backgroundImage: `url(${game.cover})` }"></div>
    <div class="scanline-overlay"></div>

    <div class="content-wrapper">
      <div class="poster-section">
        <div class="glitch-img">
          <img :src="game.cover" alt="Cover" />
        </div>
      </div>

      <div class="info-section">
        <div class="brand-tag">STEAM_DATABASE // {{ game.appId }}</div>
        <h1 class="game-name">{{ game.title }}</h1>
        
        <div class="meta-grid">
          <div class="meta-item">
            <span class="label">DEVELOPER</span>
            <span class="val">Valve</span>
          </div>
          <div class="meta-item">
            <span class="label">GENRE</span>
            <span class="val">解密 / 第一人称 / 射击</span>
          </div>
          <div class="meta-item">
            <span class="label">STATUS</span>
            <span class="val warning">尚未发布</span>
          </div>
        </div>

        <p class="description">
          这是一部由太初寰宇构建的第一人称关卡制冒险游戏。你将深入一系列独立且危机四伏的地图，利用背包管理与物资存储系统规划你的求生之路。搜集资源、拿起武器、完成目标，唯有步步为营，才能通往下一个未知的领域。
        </p>

        <div class="action-bar">
          <a :href="game.steamUrl" target="_blank" class="steam-link-btn">
            <span class="btn-icon"><i class="fab fa-steam"></i></span>
            <div class="btn-content">
              <span class="top">OPEN_IN_STEAM</span>
              <span class="bottom">前往商店页面查看详情</span>
            </div>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const game = {
  title: '不喝水就无法离开的房间',
  appId: '3654500',
  cover: 'https://image2url.com/r2/default/images/1768790181750-f6563b02-fbe4-4a51-af5b-237de4e65676.png',
  steamUrl: 'https://store.steampowered.com/app/3654500/_/'
};
</script>

<style scoped>
.steam-simple-layout {
  position: relative;
  height: 100%;
  background: #0a0a0a;
  color: #fff;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid #111;
}

/* 背景模糊效果 */
.hero-bg {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
  filter: blur(40px) brightness(0.3);
  opacity: 0.6;
  z-index: 0;
}

.scanline-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,0,0,0.1) 50%);
  background-size: 100% 4px;
  z-index: 1;
  pointer-events: none;
}

.content-wrapper {
  position: relative;
  z-index: 2;
  display: flex;
  gap: 50px;
  max-width: 1000px;
  padding: 40px;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255,255,255,0.1);
  box-shadow: 20px 20px 0 rgba(0,0,0,0.2);
}

/* 海报部分 */
.poster-section {
  flex: 0 0 280px;
}
.glitch-img {
  border: 4px solid #fff;
  box-shadow: 8px 8px 0 #D92323;
  overflow: hidden;
}
.glitch-img img { width: 100%; display: block; }

/* 信息部分 */
.info-section { flex: 1; display: flex; flex-direction: column; }
.brand-tag { font-family: 'JetBrains Mono'; font-size: 12px; color: #D92323; margin-bottom: 10px; }
.game-name { font-family: 'Anton'; font-size: 4rem; line-height: 1; margin: 0 0 20px 0; text-transform: uppercase; }

.meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 25px; }
.meta-item { display: flex; flex-direction: column; font-family: 'JetBrains Mono'; }
.meta-item .label { font-size: 10px; color: #666; }
.meta-item .val { font-size: 14px; font-weight: bold; }
.meta-item .val.warning { color: #D92323; }

.description { font-size: 15px; color: #aaa; line-height: 1.6; margin-bottom: 30px; }

/* 核心跳转按钮 */
.steam-link-btn {
  display: inline-flex;
  align-items: center;
  background: #fff;
  color: #000;
  text-decoration: none;
  padding: 12px 25px;
  transition: 0.2s;
  border: 2px solid #fff;
}
.steam-link-btn:hover {
  background: transparent;
  color: #fff;
  transform: translate(-4px, -4px);
  box-shadow: 8px 8px 0 #D92323;
}
.btn-icon { font-size: 24px; margin-right: 15px; }
.btn-content { display: flex; flex-direction: column; }
.btn-content .top { font-weight: 800; font-family: 'JetBrains Mono'; font-size: 16px; }
.btn-content .bottom { font-size: 11px; opacity: 0.7; }

@media (max-width: 800px) {
  .content-wrapper { flex-direction: column; gap: 20px; align-items: center; text-align: center; }
  .game-name { font-size: 2.5rem; }
  .meta-grid { grid-template-columns: 1fr; }
}
</style>