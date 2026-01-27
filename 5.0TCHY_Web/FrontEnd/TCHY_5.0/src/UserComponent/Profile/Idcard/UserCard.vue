<template>
  <div class="user-card">
    <div class="uc-left">
      <div class="top-row">
        <div class="identity-info">
          <div class="name-wrapper">
            <h1 class="user-name">{{ name }}</h1>
            <span class="gender-badge" :class="gender === 'Male' ? 'male' : 'female'">
              {{ gender === 'Male' ? '男' : '女' }}
            </span>
          </div>
          <div class="job-title">资深神经架构师 // ARCHITECT</div>
        </div>
      </div>

      <div class="bio-container">
        <div class="tags-list">
          <span v-for="(tag, index) in tags" :key="index" class="tag-item">
            #{{ tag }}
          </span>
        </div>
        <p class="bio-text">{{ bio }}</p>
      </div>

      <div class="footer-row">
        <span class="ft-item">📍 {{ location }}</span>
        <span class="ft-divider">|</span>
        <span class="ft-item">✉ {{ contact }}</span>
      </div>
      
      <div class="grid-bg"></div>
    </div>

    <div class="uc-center">
      <UserAvatar :avatar="avatar" :level="level" />
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'
import UserAvatar from './UserAvatar.vue' // 引入新组件

const props = defineProps({
  name: { type: String, default: 'K_Runner' },
  gender: { type: String, default: 'Male' }, 
  bio: { type: String, default: '义体维修专家，专注于老式神经网络接口的调试。寻找电子幽灵的踪迹。' },
  tags: { type: Array, default: () => ['赛博义体', '全栈开发', '黑客'] },
  location: { type: String, default: '夜之城·第七区' },
  contact: { type: String, default: 'k_dev@net.connect' },
  avatar: { type: String, default: 'https://image2url.com/r2/default/images/1769279786393-674d59cf-5620-4bde-9d31-c66661aa72f2.jpg' },
  level: { type: Number, default: 42 }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

.user-card {
  width: 100%;
  height: 100%;
  max-height: 178px;
  display: flex;
  background: transparent;
  font-family: 'Noto Sans SC', sans-serif;
  color: #2c3e50;
  overflow: hidden;
  position: relative;
}

/* === 左侧 uc-left (Flex 2) === */
.uc-left {
  flex: 2;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  position: relative;
  z-index: 1;
}

/* 顶部布局 */
.top-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.identity-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.name-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-name {
  margin: 0;
  font-size: 26px;
  font-weight: 900;
  line-height: 1.1;
  color: #1a1a1a;
}

.gender-badge {
  font-size: 11px;
  padding: 1px 4px;
  border-radius: 2px;
  color: #fff;
  font-weight: bold;
}
.male { background-color: #3498db; }
.female { background-color: #e91e63; }

.job-title {
  font-size: 10px;
  color: #e67e22;
  font-weight: bold;
  letter-spacing: 0.5px;
  font-family: 'Share Tech Mono', monospace;
}

/* --- 中间简介区 --- */
.bio-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 4px;
  overflow: hidden;
}

.tags-list {
  display: flex;
  gap: 6px;
}
.tag-item {
  font-size: 12px;
  background: rgba(0,0,0,0.05);
  color: #555;
  padding: 1px 6px;
  border-radius: 2px;
}

.bio-text {
  margin: 0;
  font-size: 13px;
  line-height: 1.5;
  color: #444;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* --- 底部信息 --- */
.footer-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #777;
  font-family: 'Share Tech Mono', 'Noto Sans SC', sans-serif;
  border-top: 1px dashed rgba(0,0,0,0.1);
  padding-top: 6px;
}
.ft-divider { opacity: 0.3; font-size: 10px; }

/* 背景装饰 */
.grid-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: radial-gradient(#ccc 1px, transparent 1px);
  background-size: 20px 20px;
  opacity: 0.2;
  z-index: -1;
  pointer-events: none;
}

/* === 右侧 uc-center (Flex 1) === */
/* 这里保留布局属性，负责将 Avatar 组件居中 */
.uc-center {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  border-left: 0px solid rgba(0,0,0,0.05);
  border-right: 0px solid rgba(0,0,0,0.05);
}
</style>