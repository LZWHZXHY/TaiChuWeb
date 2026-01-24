<template>
  <div class="ticket-card">
    <div class="noise-overlay"></div>
    
    <div class="wing left-wing">
      <div class="bio-text">
        <span class="quote-mark">“</span>
        {{ userInfo.bio }}
      </div>
      
      <div class="meta-row">
        <div class="meta-item">
          <span class="label">LOC</span>
          <span class="val">{{ userInfo.location }}</span>
        </div>
        <div class="meta-item">
          <span class="label">COMM</span>
          <span class="val">{{ userInfo.contact }}</span>
        </div>
      </div>

      <div class="tags-row">
        <span v-for="tag in displayTags" :key="tag" class="mini-tag">#{{ tag }}</span>
      </div>
    </div>

    <div class="center-core">
      <div class="avatar-ring">
        <img :src="userInfo.avatar" alt="avatar" />
        <div class="gender-badge" :class="userInfo.gender === '女' ? 'female' : 'male'">
           {{ userInfo.gender === '女' ? '♀' : '♂' }}
        </div>
      </div>
    </div>

    <div class="wing right-wing">
      <div class="identity-block">
        <h1 class="name">{{ userInfo.nickname }}</h1>
        <span class="title-badge">{{ userInfo.title }}</span>
      </div>

      <div class="action-group">
        <div class="fans-display" v-if="isFollowing">
          <span class="pulse-dot"></span>
          LINKED: {{ formattedFansCount }}
        </div>

        <div class="btn-row">
          <button 
            class="action-btn follow-btn" 
            :class="{ 'active': isFollowing }"
            @click="handleFollowClick"
          >
            {{ isFollowing ? '取消关注' : '+ 关注' }}
          </button>
          
          <button class="action-btn msg-btn" @click="emit('handleMessage')">
            ✉
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { defineProps, defineEmits, computed } from 'vue'

const MOCK_USER_DATA = {
  nickname: 'K-VAI',
  avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?ixlib=rb-4.0.3&auto=format&fit=crop&w=200&h=200&q=80',
  gender: '男', 
  title: 'ARCHITECT',
  location: 'CN / SH',
  contact: 'K@V.DEV',
  bio: '探索虚拟与现实的边界。', // 简介建议短一点
  tags: ['VUE', 'DESIGN', 'ART'],
}

const props = defineProps({
  user: {
    type: Object,
    default: () => null 
  },
  followerCount: {
    type: Number,
    default: 89757
  },
  isFollowing: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['toggleFollow', 'handleMessage'])

const userInfo = computed(() => {
  if (!props.user || Object.keys(props.user).length === 0) return MOCK_USER_DATA
  return { ...MOCK_USER_DATA, ...props.user }
})

const displayTags = computed(() => (userInfo.value.tags || []).slice(0, 3))

const formattedFansCount = computed(() => {
  const num = props.followerCount
  if (num >= 10000) return (num / 10000).toFixed(1) + 'w'
  return num
})

const handleFollowClick = () => {
  emit('toggleFollow')
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;500;700&family=Jura:wght@500;700&display=swap');

/* --- 核心容器：填满 idcard-box --- */
.ticket-card {
  width: 100%;
  height: 100%;
  background-color: #F4F1EA; /* 米白 */
  color: #333;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 2%;
  position: relative;
  overflow: hidden;
  font-family: 'Noto Sans SC', sans-serif;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  border-radius: 8px; /* 轻微圆角 */
  user-select: none;
}

/* 纸张噪点纹理 */
.noise-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  opacity: 0.03;
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.8' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)'/%3E%3C/svg%3E");
  pointer-events: none;
  z-index: 0;
}

/* --- 左右翼布局 (Wings) --- */
.wing {
  flex: 1;
  height: 80%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  z-index: 1;
}

/* 左翼：简介与元数据 */
.left-wing {
  align-items: flex-start;
  padding-right: 20px;
  border-right: 1px dashed #ccc; /* 虚线分割，像票据 */
  text-align: left;
}

/* 右翼：身份与按钮 */
.right-wing {
  align-items: flex-end;
  padding-left: 20px;
  border-left: 1px dashed #ccc;
  text-align: right;
}

/* --- 中间核心：头像 --- */
.center-core {
  flex-shrink: 0;
  width: 13vw; /* 稍微宽一点以容纳头像 */
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
  z-index: 2;
}

.avatar-ring {
  /* 动态计算大小：利用视口单位确保适配扁长条 */
  width: 9vw; 
  height: 9vw;
  max-width: 140px; max-height: 140px;
  min-width: 80px; min-height: 80px;
  
  border-radius: 50%;
  background: #F4F1EA;
  border: 4px solid #fff;
  box-shadow: 
    0 4px 10px rgba(0,0,0,0.1),
    inset 0 0 0 1px rgba(0,0,0,0.1);
  position: relative;
}

.avatar-ring img {
  width: 100%; height: 100%;
  object-fit: cover;
  border-radius: 50%;
}

.gender-badge {
  position: absolute;
  bottom: 0; right: 0;
  width: 1.8vw; height: 1.8vw;
  min-width: 20px; min-height: 20px;
  background: #333;
  color: #fff;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.8rem;
  border: 2px solid #F4F1EA;
}
.gender-badge.female { background: #e85d75; }
.gender-badge.male { background: #2d6a4f; }

/* --- 样式细节：左侧 --- */
.bio-text {
  font-size: 0.8rem;
  color: #555;
  margin-bottom: 8px;
  font-style: italic;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.quote-mark { color: #aaa; font-size: 1.2rem; margin-right: 2px; }

.meta-row { display: flex; gap: 12px; margin-bottom: 6px; }
.meta-item { display: flex; flex-direction: column; }
.label { font-size: 0.5rem; color: #999; font-weight: 700; font-family: 'Jura', sans-serif; }
.val { font-size: 0.7rem; font-weight: 600; font-family: 'Jura', sans-serif;}

.tags-row { display: flex; gap: 5px; }
.mini-tag {
  font-size: 0.6rem;
  padding: 2px 6px;
  background: #e8e6df;
  border-radius: 4px;
  color: #666;
}

/* --- 样式细节：右侧 --- */
.identity-block { margin-bottom: 8px; }
.name {
  margin: 0;
  font-size: 1.4rem;
  font-weight: 700;
  line-height: 1;
  color: #1a1a1a;
}
.title-badge {
  font-size: 0.7rem;
  background: #1a1a1a;
  color: #F4F1EA;
  padding: 1px 6px;
  border-radius: 2px;
  font-family: 'Jura', sans-serif;
  letter-spacing: 1px;
}

.action-group {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 5px;
}

.fans-display {
  font-size: 0.6rem;
  color: #666;
  font-family: 'Jura', sans-serif;
  display: flex; align-items: center; gap: 4px;
}
.pulse-dot {
  width: 6px; height: 6px; background: #10B981; border-radius: 50%;
  box-shadow: 0 0 4px #10B981;
}

.btn-row { display: flex; gap: 8px; }

.action-btn {
  border: none;
  cursor: pointer;
  transition: all 0.2s;
  font-family: 'Noto Sans SC', sans-serif;
  height: 32px;
  display: flex; align-items: center; justify-content: center;
}

.follow-btn {
  background: #1a1a1a;
  color: #fff;
  padding: 0 16px;
  font-size: 0.8rem;
  font-weight: 500;
  border-radius: 4px;
}
.follow-btn:hover { background: #333; transform: translateY(-1px); }
.follow-btn.active { 
  background: #ddd; 
  color: #555; 
}

.msg-btn {
  width: 32px;
  background: #fff;
  border: 1px solid #ccc;
  border-radius: 4px;
  color: #333;
}
.msg-btn:hover { border-color: #333; }

/* 响应式微调：如果是非常宽的屏幕 */
@media (min-width: 1600px) {
  .name { font-size: 1.8rem; }
  .bio-text { font-size: 0.9rem; }
}
</style>