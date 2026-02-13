<template>
  <div class="home-layout">
    <div class="main-content-area">
      <FeaturedSection :userId="userId" />

      <GallerySection :userId="userId" />

      <BlogSection :userId="userId" />

      <PostSection :userId="userId" />
    </div>

    <div class="side-container">
      <div class="side-wrapper">
        <UserActionButtons />
        
        <StatsCard :status="status"/>
        
        <NoticeBoard />
      </div>

      <div><AchievementPanel /></div>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue' // 显式引入（可选）
import FeaturedSection from './HomeContent/FeaturedSection.vue'
import StatsCard from './HomeContent/StatsCard.vue'
import NoticeBoard from './HomeContent/NoticeBoard.vue'
import AchievementPanel from './HomeContent/AchievementPanel.vue'
import GallerySection from './HomeContent/GallerySection.vue'
import BlogSection from './HomeContent/BlogSection.vue'
import PostSection from './HomeContent/PostSection.vue'
import UserActionButtons from './HomeContent/UserActionButtons.vue'

// 【修复】定义 Props 接收来自父组件 (Maincard) 的数据
const props = defineProps({
  status: {
    type: Object,
    default: () => ({})
  },
  // 接收传下来的 userId，允许数字或字符串（应对 MEE 这种情况）
  userId: {
    type: [Number, String],
    default: null
  }
})
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
}

.item-label { 
  font-size: 10px; 
  color: #FF2A2A; 
  margin-bottom: 8px; 
  letter-spacing: 1px; 
}

.mini-list { 
  list-style: none; 
  padding: 0; 
  margin: 0; 
  font-size: 11px; 
  line-height: 1.6; 
}
</style>