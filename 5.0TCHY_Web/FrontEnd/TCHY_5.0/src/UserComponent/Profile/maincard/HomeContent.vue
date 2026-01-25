<template>
  <div class="home-layout">
    <div class="main-content-area">
      <FeaturedSection />

      <GallerySection />

      <BlogSection />

      <PostSection />
    </div>

    <div class="side-container">
      <div class="side-wrapper">
        <StatsCard />
        
        <NoticeBoard />
      </div>

      <div><AchievementPanel /></div>
    </div>
  </div>
</template>

<script setup>
import FeaturedSection from './HomeContent/FeaturedSection.vue'
import StatsCard from './HomeContent/StatsCard.vue'
import NoticeBoard from './HomeContent/NoticeBoard.vue'
import AchievementPanel from './HomeContent/AchievementPanel.vue'
import GallerySection from './HomeContent/GallerySection.vue'
import BlogSection from './HomeContent/BlogSection.vue'
import PostSection from './HomeContent/PostSection.vue'
</script>

<style scoped>
.home-layout {
  display: flex;
  gap: 15px;
  width: 100%;
  height: auto;
  font-family: 'Courier New', Courier, monospace;
}

/* --- 左侧布局 --- */
.main-content-area {
  flex: 0 0 75%;
  min-width: 0;
}

/* --- 右侧布局 --- */
.side-container {
  flex: 0 0 25%;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.side-wrapper {
  display: flex;
  flex-direction: column;
  /* 这里不需要 gap，因为 NoticeBoard 组件自带了 margin-top，或者你可以由父级控制 */
}

.item-label { font-size: 10px; color: #FF2A2A; margin-bottom: 8px; letter-spacing: 1px; }
.mini-list { list-style: none; padding: 0; margin: 0; font-size: 11px; line-height: 1.6; }
</style>