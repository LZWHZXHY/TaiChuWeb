<template>
  <section class="video-section">
    <div class="section-title">
      <span class="icon">▶</span>
      <h2>VISUAL_STREAM // 视频流</h2>
    </div>

    <div class="scroll-container">
      <div 
        v-for="(video, index) in videos" 
        :key="index" 
        class="video-card"
      >
        <div class="cover-box">
          <img :src="video.cover" loading="lazy" />
          <div class="play-btn"></div>
          <div class="duration">{{ video.duration }}</div>
        </div>
        <div class="info-box">
          <h3 class="v-title">{{ video.title }}</h3>
          <div class="v-meta">
            <span>UP: {{ video.author }}</span>
            <span>{{ video.views }} views</span>
          </div>
        </div>
      </div>
      
      <div class="more-card">
        <span>VIEW<br>ALL</span>
      </div>
    </div>
  </section>
</template>

<script setup>
const videos = [

];
</script>

<style scoped>
/* 容器 */
.video-section {
  width: 100%;
  margin: 60px 0;
  overflow: hidden; /* 防止撑破页面 */
}

.section-title {
  max-width: 1400px; margin: 0 auto 20px; padding: 0 20px;
  display: flex; align-items: center; gap: 10px;
  font-family: 'JetBrains Mono'; color: #00F0FF;
}

/* 滚动区域核心样式 */
.scroll-container {
  display: flex;
  gap: 20px;
  overflow-x: auto; /* 允许横向滚动 */
  padding: 20px calc((100% - 1400px) / 2 + 20px); /* 智能计算 Padding，让内容居中但能滑到边缘 */
  scrollbar-width: none; /* Firefox 隐藏滚动条 */
}
.scroll-container::-webkit-scrollbar { display: none; /* Chrome 隐藏滚动条 */ }

/* 卡片样式 */
.video-card {
  flex: 0 0 300px; /* 固定宽度，不伸缩 */
  background: rgba(20,20,20,0.8);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 12px;
  overflow: hidden;
  transition: 0.3s;
  cursor: pointer;
}
.video-card:hover {
  transform: translateY(-5px);
  border-color: #00F0FF;
  box-shadow: 0 5px 20px rgba(0, 240, 255, 0.2);
}

.cover-box {
  position: relative;
  height: 168px; /* 16:9 */
  overflow: hidden;
}
.cover-box img { width: 100%; height: 100%; object-fit: cover; transition: 0.5s; }
.video-card:hover .cover-box img { transform: scale(1.1); }

.play-btn {
  position: absolute; inset: 0; margin: auto;
  width: 40px; height: 40px; background: rgba(0,0,0,0.6);
  border-radius: 50%; border: 2px solid #fff;
  opacity: 0; transition: 0.3s;
  display: flex; align-items: center; justify-content: center;
}
.play-btn::after { content: '▶'; color: #fff; margin-left: 2px; }
.video-card:hover .play-btn { opacity: 1; transform: scale(1.1); }

.duration {
  position: absolute; bottom: 5px; right: 5px;
  background: #000; color: #fff; font-size: 10px; padding: 2px 4px;
  font-family: 'JetBrains Mono';
}

.info-box { padding: 15px; }
.v-title { font-size: 14px; color: #fff; margin-bottom: 8px; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.v-meta { display: flex; justify-content: space-between; font-size: 12px; color: #666; font-family: 'JetBrains Mono'; }

/* 查看更多卡片 */
.more-card {
  flex: 0 0 100px;
  display: flex; align-items: center; justify-content: center;
  border: 1px dashed rgba(255,255,255,0.2);
  color: #666; font-family: 'JetBrains Mono';
  cursor: pointer; transition: 0.3s;
}
.more-card:hover { border-color: #00F0FF; color: #00F0FF; background: rgba(0, 240, 255, 0.1); }
</style>