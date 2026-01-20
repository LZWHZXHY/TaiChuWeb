<template>
  <div class="archive-deck">
    
    <div class="deck-column main-column">
      <div class="panel-section full-height">
        <div class="panel-header">
          <span class="header-tag">:: PROJECT_ZONES</span>
          <span class="header-decor-line"></span>
          <span class="header-id">SYS.OP.4</span>
        </div>
        <div class="works-grid-container">
          <div 
            class="work-card" 
            v-for="(work, index) in data.works.slice(0, 4)" 
            :key="work.id"
          >
            <div class="visual-layer">
              <img v-if="work.cover" :src="work.cover" alt="cover" />
              <div v-else class="visual-placeholder" :style="{ background: work.color || '#ccc' }"></div>
              
              <div class="hover-overlay">
                <button class="launch-btn">LAUNCH_SEQ</button>
              </div>
            </div>
            <div class="info-strip">
              <span class="is-idx">0{{ index + 1 }}</span>
              <span class="is-title">{{ work.title }}</span>
              <span class="is-cat">// {{ work.category || 'DEV' }}</span>
            </div>
            <div class="corner-L"></div>
          </div>
        </div>
      </div>
    </div>
    <div class="deck-column aux-column">
      
      <div class="panel-section log-section">
        <div class="panel-header">
          <span class="header-tag">:: TRANSMISSION</span>
          <span class="header-decor-line"></span>
        </div>
        <div class="logs-scroll-area custom-scroll">
          <div class="log-group">
            <div 
              class="log-item" 
              v-for="blog in data.blogs.slice(0, 2)" 
              :key="'b'+blog.id"
            >
              <div class="li-marker"></div>
              <div class="li-content">
                <div class="li-title">{{ blog.title }}</div>
                <div class="li-meta">LOG // {{ blog.date }}</div>
              </div>
            </div>
          </div>
          <div class="divider-text">--- RECENT_SIGNALS ---</div>
          <div class="post-group">
            <div 
              class="post-item" 
              v-for="post in data.posts.slice(0, 2)" 
              :key="'p'+post.id"
            >
              <div class="pi-img">
                <img v-if="post.image" :src="post.image" />
                <div v-else class="pi-ph"></div>
              </div>
              <div class="pi-text">
                <div class="pi-t">{{ post.title }}</div>
                <div class="pi-d">{{ post.date }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="panel-section setting-section">
        <div class="world-access-card">
          <div class="wac-bg">
            <img v-if="data.setting.image" :src="data.setting.image" />
            <div class="wac-overlay"></div>
            <div class="wac-grid"></div>
          </div>
          
          <div class="wac-content">
            <div class="wac-top">
              <span class="icon-hazard">☢</span>
              <span class="label-restricted">RESTRICTED_AREA</span>
            </div>
            
            <div class="wac-main">
              <h2 class="world-title">{{ data.setting.name || '彼岸宇宙' }}</h2>
              <div class="world-desc">
                DATABASE_ARCHIVE // LEVEL_5 <br>
                ACCESS_GRANTED
              </div>
            </div>
            <button class="access-trigger">
              <span class="blink-cursor">></span> ENTER_SYSTEM
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup>
defineProps({
  data: {
    type: Object,
    required: true,
    default: () => ({ works: [], blogs: [], posts: [], setting: {} })
  }
})
</script>
<style scoped>
/* 布局策略: Flexbox + Percentage 
  配色: 米色 #F4F1EA, 黑色 #111, 红色 #D92323
*/
.archive-deck {
  width: 100%;
  height: 670px; /* 依赖父容器高度 */
  display: flex;
  gap: 12px;
  background: #F4F1EA;
  color: #111;
  font-family: 'Helvetica Neue', Arial, sans-serif;
  box-sizing: border-box;
  padding: 2%;
  background-color: rgba(0, 0, 0, 0.1);
}
/* 列布局 */
.deck-column {
  display: flex;
  flex-direction: column;
  gap: 12px;
  height: 100%;
}
.main-column { flex: 2; /* 约 66% */ }
.aux-column { flex: 1; /* 约 33% */ }
/* 通用面板容器 */
.panel-section {
  border: 2px solid #111;
  background: rgba(255, 255, 255, 0.55);
  display: flex;
  flex-direction: column;
  box-shadow: #7c7c7c 4px 4px 0;
}
.full-height { height: 100%; }
/* 头部样式 */
.panel-header {
  height: 36px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  padding: 0 10px;
  border-bottom: 2px solid #111;
  background: #000000;
  font-family: monospace;
  font-weight: 700;
  font-size: 0.75rem;
}
.header-tag { color: #ffffff; margin-right: 8px; }
.header-decor-line {
  flex: 1; height: 1px; background: #999; margin: 0 10px;
}
.header-id { color: #D92323; }
/* ============================
   左侧：Works Grid (自适应高度)
   ============================ */
.works-grid-container {
  flex: 1; /* 撑满剩余高度 */
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr; /* 两行平分高度 */
  gap: 8px;
  padding: 8px;
  overflow: hidden; /* 关键：防止卡片超出容器 */
  background-color: rgba(0,0,0,0.4);
}
.work-card {
  position: relative;
  display: flex;
  flex-direction: column;
  border: 1px solid #ddd;
  transition: border-color 0.3s;
  cursor: pointer;
  overflow: hidden;
  height: 100%; /* 关键：强制卡片高度=Grid单元格高度 */
  box-sizing: border-box; /* 关键：边框不占用额外宽度/高度 */
}
.work-card:hover { border-color: #D92323; }
/* 图片层 (占据大部分高度) */
.visual-layer {
  flex: 1;
  position: relative;
  overflow: hidden;
  background: rgba(238,238,238,0.0);
  min-height: 0; /* 修复 Flex 子元素最小高度问题 */
}
.visual-layer img {
  width: 100%; height: 100%; object-fit: cover; /* 核心：保持比例+填充裁剪 */
  filter: grayscale(100%); transition: filter 0.4s;
}
.work-card:hover .visual-layer img { filter: grayscale(0%); }
.hover-overlay {
  position: absolute; inset: 0;
  background: rgba(17,17,17,0.4);
  display: flex; align-items: center; justify-content: center;
  opacity: 0; transition: opacity 0.3s;
}
.work-card:hover .hover-overlay { opacity: 1; }
.launch-btn {
  background: #D92323; color: #fff; border: none;
  padding: 6px 12px; font-family: monospace; font-weight: bold;
  cursor: pointer; letter-spacing: 1px;
}
/* 底部信息条 */
.info-strip {
  height: 32px;
  flex-shrink: 0;
  background: #fff;
  border-top: 1px solid #111;
  display: flex;
  align-items: center;
  padding: 0 8px;
  font-family: monospace;
  font-size: 0.7rem;
}
.is-idx { font-weight: 900; margin-right: 8px; color: #ccc; }
.work-card:hover .is-idx { color: #D92323; }
.is-title { font-weight: bold; flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.is-cat { color: #888; }
.corner-L {
  position: absolute; bottom: 0; left: 0;
  width: 0; height: 0;
  border-bottom: 8px solid #111; border-right: 8px solid transparent;
}
/* ============================
   右侧上部：Logs (60%)
   ============================ */
.log-section {
  height: 60%; /* 高度分配 */
}
.logs-scroll-area {
  flex: 1; overflow-y: auto; padding: 10px;
}
.log-item {
  display: flex; align-items: flex-start; margin-bottom: 15%;margin-top: 10px;
  
}
.li-marker {
  width: 6px; height: 6px; background: #ccc;
  margin-top: 5px; margin-right: 8px;
}
.log-item:hover .li-marker { background: #D92323; box-shadow: 0 0 5px #D92323; }
.li-title { font-size: 0.8rem; font-weight: bold; line-height: 1.3; }
.li-meta { font-size: 0.6rem; color: #999; font-family: monospace; margin-top: 2px; }
.divider-text {
  text-align: center; font-size: 0.6rem; color: #000000;
  margin: 15px 0; font-family: monospace;
}
.post-item {
  display: flex; gap: 8px; margin-bottom: 8px;
  padding: 4px; border: 1px solid transparent;
  transition: all 0.8s ease;
  background: #ffffff;
  border: 1px solid #252424;
}
.post-item:hover { border-color: #000000; background: #000000; color: #ffffff;}
.pi-img { width: 48px; height: 48px; background: #ddd; flex-shrink: 0; }
.pi-img img { width: 100%; height: 100%; object-fit: cover; }
.pi-text { display: flex; flex-direction: column; justify-content: center; }
.pi-t { font-size: 0.75rem; font-weight: bold; margin-bottom: 2px; }
.pi-d { font-size: 0.6rem; color: #aaa; }
/* ============================
   右侧下部：World View (40%) - NEW DESIGN
   ============================ */
.setting-section {
  height: 40%; /* 高度分配 */
  border: none; /* 移除外边框，让黑卡自己做主 */
  background: transparent;
  overflow: hidden;
}
.world-access-card {
  width: 100%; height: 100%;
  position: relative;
  background: #0a0a0a;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  border: 2px solid #111;
  overflow: hidden;
}
/* 背景特效 */
.wac-bg {
  position: absolute; inset: 0; z-index: 0;
}
.wac-bg img {
  width: 100%; height: 100%; object-fit: cover;
  opacity: 0.6; filter: grayscale(100%) contrast(1.2);
  transition: all 0.5s ease;
}
.world-access-card:hover .wac-bg img {
  opacity: 0.8; filter: grayscale(0%) contrast(1.1); transform: scale(1.05);
}
.wac-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(to top, #000 10%, rgba(0,0,0,0.4) 100%);
}
.wac-grid {
  position: absolute; inset: 0;
  background-image: linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px);
  background-size: 100% 20px;
  pointer-events: none;
}
/* 内容层 */
.wac-content {
  position: relative; z-index: 1;
  padding: 16px;
  height: 100%;
  display: flex; flex-direction: column;
  color: #fff;
  box-sizing: border-box;
}
.wac-top {
  display: flex; align-items: center; justify-content: space-between;
  font-family: monospace; font-size: 0.65rem; color: #D92323;
}
.icon-hazard { font-size: 0.9rem; animation: pulse 2s infinite; }
.wac-main {
  flex: 1;
  display: flex; flex-direction: column; justify-content: center;
}
.world-title {
  font-size: 1.6rem; font-weight: 900; margin: 0 0 6px 0;
  text-transform: uppercase; letter-spacing: -1px;
  color: #fff;
  text-shadow: 0 0 10px rgba(255,255,255,0.3);
}
.world-desc {
  font-family: monospace; font-size: 0.65rem; color: #888;
  line-height: 1.4;
}
.access-trigger {
  align-self: flex-start;
  background: rgba(217, 35, 35, 0.1);
  border: 1px solid #D92323;
  color: #D92323;
  padding: 8px 16px;
  font-family: monospace; font-weight: bold; font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.2s;
  text-transform: uppercase;
}
.access-trigger:hover {
  background: #D92323; color: #fff; box-shadow: 0 0 15px rgba(217,35,35,0.6);
}
@keyframes pulse { 0% { opacity: 0.5; } 50% { opacity: 1; } 100% { opacity: 0.5; } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.blink-cursor { animation: blink 1s step-end infinite; margin-right: 4px;}
/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 3px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
</style>