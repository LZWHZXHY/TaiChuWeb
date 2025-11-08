<template>
  <div class="page">
    <header class="top">
      <div class="title">审核控制台</div>
      <div class="seg">
        <button class="seg-btn" :class="{ active: section === 'review' }" @click="section = 'review'">内容审核</button>
        <button class="seg-btn" :class="{ active: section === 'activity' }" @click="section = 'activity'">活动配置</button>
        <button class="seg-btn" :class="{ active: section === 'site' }" @click="section = 'site'">站点设置</button>
        <button class="seg-btn" :class="{ active: section === 'notification' }" @click="section = 'notification'">系统通知</button>
      </div>
    </header>

    <main class="main">
      <ContentReview v-if="section === 'review'" />
      <ActivityConfig v-else-if="section === 'activity'" />
      <SiteSettings v-else-if="section === 'site'" />
      <NotificationManager v-else />
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ContentReview from '../Review_components/ContentReview.vue'
import ActivityConfig from '../Review_components/ActivityConfig.vue'
import SiteSettings from '../Review_components/SiteSettings.vue'
import NotificationManager from '../Review_components/NotificationManager.vue'

const section = ref('review') // review | activity | site
</script>

<style scoped>
.page {
  min-height: 100vh;
  background: #f6f7fb;
  color: #0f172a;
  display: flex;
  flex-direction: column;
}
.top {
  position: sticky; top: 0; z-index: 10;
  backdrop-filter: blur(6px);
  background: rgba(255,255,255,.7);
  border-bottom: 1px solid rgba(17,24,39,.08);
  padding: 12px 16px;
  display: flex; align-items: center; justify-content: space-between;
}
.title { font-weight: 700; }
.seg { display: inline-flex; gap: 6px; padding: 4px; border: 1px solid rgba(17,24,39,.12); border-radius: 10px; background: #fff; }
.seg-btn { padding: 6px 12px; border-radius: 8px; border: 1px solid transparent; background: transparent; cursor: pointer; }
.seg-btn.active { background: #111827; color: #fff; border-color: #111827; }
.main { padding: 16px; }
</style>