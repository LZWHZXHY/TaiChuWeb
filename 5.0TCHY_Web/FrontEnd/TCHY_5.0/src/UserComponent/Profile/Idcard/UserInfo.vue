<template>
  <div class="uc-left">
    <div class="top-row">
      <div class="identity-info">
        <div class="name-wrapper">
          <h1 class="user-name">{{ name || '未命名' }}</h1>
          <span v-if="gender" class="gender-badge">
            {{ gender }}
          </span>
        </div>
        <div class="signature-text">
          {{ signature || 'NO_SIGNATURE // 暂无签名' }}
        </div>
      </div>
    </div>

    <div class="bio-container">
      <div class="tags-list" v-if="processedInterests.length > 0">
        <span v-for="(tag, index) in processedInterests" :key="index" class="tag-item">
          #{{ tag }}
        </span>
      </div>
      <p class="bio-text">{{ bio || '用户很懒，什么都没写...' }}</p>
    </div>

    <div class="footer-row">
      <span class="ft-item" v-if="location">
        📍 {{ location }}
      </span>
      <span class="ft-divider" v-if="location && birthday">|</span>
      
      <span class="ft-item" v-if="birthday">
        🎂 {{ birthday }}
      </span>
      <span class="ft-divider" v-if="(location || birthday) && contact">|</span>
      
      <span class="ft-item" v-if="contact">
        ✉ {{ contact }}
      </span>
    </div>
    
    <div class="grid-bg"></div>
  </div>
</template>

<script setup>
import { defineProps, computed } from 'vue'

const props = defineProps({
  name: { type: String, default: '' },
  gender: { type: String, default: '' },
  bio: { type: String, default: '' },
  // 对应 ProfileSetting 中的 interests (String)
  interests: { type: String, default: '' }, 
  location: { type: String, default: '' },
  contact: { type: String, default: '' },
  // 新增字段
  signature: { type: String, default: '' },
  birthday: { type: String, default: '' }
})

// 处理兴趣字段：将中文或英文逗号分隔的字符串转为数组
const processedInterests = computed(() => {
  if (!props.interests) return [];
  // 替换中文逗号为英文逗号，然后分割，并过滤空项
  return props.interests.replace(/，/g, ',').split(',').map(i => i.trim()).filter(i => i);
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* === 左侧 uc-left (Flex 2) === */
.uc-left {
  flex: 2;
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  position: relative;
  z-index: 1;
  font-family: 'Noto Sans SC', sans-serif;
  overflow: hidden; /* 防止内容溢出 */
}

/* 顶部布局 */
.top-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.identity-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
}

.name-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.user-name {
  margin: 0;
  font-size: 28px;
  font-weight: 900;
  line-height: 1.1;
  color: #1a1a1a;
  letter-spacing: -0.5px;
}

/* 性别徽章：改为通用的工业黑风格，适配自定义输入 */
.gender-badge {
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
  background-color: #111;
  color: #F4F1EA; /* 米色文字 */
  font-weight: bold;
  font-family: 'Share Tech Mono', monospace;
  text-transform: uppercase;
  display: inline-block;
}

/* 个性签名样式 (原 Job Title 位置) */
.signature-text {
  font-size: 12px;
  color: #d35400; /* 工业橙/红 */
  font-weight: bold;
  letter-spacing: 0.5px;
  font-family: 'Share Tech Mono', 'Noto Sans SC', monospace;
  opacity: 0.9;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

/* --- 中间简介区 --- */
.bio-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 12px;
  overflow: hidden;
}

.tags-list {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.tag-item {
  font-size: 11px;
  background: rgba(0,0,0,0.06);
  color: #444;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 500;
  border: 1px solid rgba(0,0,0,0.05);
}

.bio-text {
  margin: 0;
  font-size: 13px;
  line-height: 1.6;
  color: #444;
  display: -webkit-box;
  -webkit-line-clamp: 4; /* 限制行数 */
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: pre-wrap; /* 保留换行符 */
}

/* --- 底部信息 --- */
.footer-row {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 11px;
  color: #888;
  font-family: 'Share Tech Mono', 'Noto Sans SC', sans-serif;
  border-top: 2px solid #eee;
  padding-top: 10px;
  margin-top: auto; /* 推到底部 */
}

.ft-item {
    display: flex;
    align-items: center;
    gap: 4px;
}

.ft-divider { opacity: 0.3; font-size: 10px; color: #000; }

/* 背景装饰 */
.grid-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: 
    linear-gradient(rgba(0,0,0,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.03) 1px, transparent 1px);
  background-size: 20px 20px;
  z-index: -1;
  pointer-events: none;
}
</style>