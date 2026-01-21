<template>
  <aside class="sidebar-left custom-scroll">
    <div class="id-flip-wrapper" :class="{ 'is-flipped': showIdArchive }">
      <div class="id-flipper">
        <div class="id-face id-front">
          <div class="cyber-card id-card-content">
            <div class="grid-bg"></div>
            
            <button class="settings-trigger-btn" title="用户资料设置" @click="emit('goToUserSettings')">
              <span class="icon">⚙</span>
            </button>
            
            <div class="tech-header" @click="emit('goToUserSettings')">
              <span class="tech-id">ID:{{ userID }}</span>
              <div class="status-dot"></div>
            </div>

            <button class="flip-trigger-btn" @click="emit('toggleIdArchive')" title="查看详细资料">
              <span class="icon">▶</span>
              <span class="corner-deco-sm"></span>
            </button>

            <div class="caution-stripe"></div>

            <div class="avatar-container">
              <div class="avatar-frame">
                <img :src="user.avatar" alt="avatar" />
                <div class="bracket-tl"></div>
                <div class="bracket-br"></div>
                <div class="scan-line"></div>
              </div>
            </div>

            <div class="id-info">
              <h1 class="user-name">{{ user.nickname }}</h1>
              
              <div class="user-meta-row">
                <div class="meta-badge role-badge">
                  <span class="hash">#</span> {{ user.role }}
                </div>
                <div class="meta-badge gender-badge" v-if="user.gender">
                  <span class="icon-gender">{{ user.gender === '男' || user.gender === 'Male' ? '♂' : (user.gender === '女' || user.gender === 'Female' ? '♀' : '⚥') }}</span>
                  <span class="text-gender">{{ user.gender }}</span>
                </div>
              </div>

              <div class="bio-container">
                <p class="bio-text">
                  {{ user.bio || 'NO DATA // 暂无个人简介数据...' }}
                </p>
                <div class="bio-deco"></div>
              </div>

              <div class="meta-tags">
                <span class="tag" v-for="tag in user.tags" :key="tag">
                  {{ tag }}
                </span>
              </div>
            </div>

            <div class="action-row">
              <button 
                class="action-btn" 
                :class="isFollowing ? 'active-state' : 'primary'" 
                @click="emit('toggleFollow')"
              >
                {{ isFollowing ? '✓ FOLLOWING' : '+ FOLLOW' }}
              </button>
              <button class="action-btn secondary" @click="emit('handleMessage')">
                MESSAGE_
              </button>
            </div>
            
            <div class="card-footer-deco">
              <span>DESIGNED BY JURIEO</span>
              <span>VER 2.0</span>
            </div>
          </div>
        </div>

        <div class="id-face id-back">
          <IdArchiveCard :user="user" @close="emit('toggleIdArchive')" />
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'
import IdArchiveCard from '@/UserComponent/Profile/IdArchiveCard.vue'

const props = defineProps({
  user: {
    type: Object,
    required: true
  },
  userID: {
    type: [String, Number],
    required: true
  },
  achievements: {
    type: Array,
    required: true
  },
  isFollowing: {
    type: Boolean,
    required: true
  },
  showIdArchive: {
    type: Boolean,
    required: true
  }
})

const emit = defineEmits([
  'toggleIdArchive',
  'toggleFollow',
  'handleMessage',
  'goToUserSettings'
])
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* 定义赛博工业风变量 */
:root {
  --main-bg: #F4F1EA; /* 用户偏好的米色 */
  --cyber-black: #1a1a1a;
  --cyber-red: #ff3333;
  --cyber-gray: #888888;
  --border-width: 2px;
}

/* 基础布局 */
.sidebar-left {
  width: 320px;
  display: flex; flex-direction: column; gap: 20px;
  overflow-y: hidden;
  padding: 5px 5px 0px 0; /* 调整padding防止阴影被切 */
  flex-shrink: 0;
  height: 97%;
}

/* 翻转容器 */
.id-flip-wrapper {
  perspective: 1200px;
  width: 100%;
  position: relative;
  z-index: 10;
}
.id-flipper {
  width: 100%;
  position: relative;
  transform-style: preserve-3d;
  transition: transform 0.6s cubic-bezier(0.23, 1, 0.32, 1);
}
.id-flip-wrapper.is-flipped .id-flipper {
  transform: rotateY(180deg);
}
.id-face {
  backface-visibility: hidden;
  width: 100%;
}
.id-front { z-index: 2; }
.id-back {
  position: absolute;
  top: 0; left: 0;
  height: 100%;
  transform: rotateY(180deg);
  z-index: 1;
}

/* --- 核心卡片样式 --- */
.cyber-card {
  background-color: rgba(244, 241, 234, 0.4); /* 核心米色 */
  border: 2px solid #000;
  /* 硬朗的黑色投影 */
  box-shadow: 8px 8px 0px  rgba(0,0,0,0.15); 
  padding: 0;
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.id-card-content {
  padding: 24px;
  text-align: center;
  position: relative;
}

/* 背景网格纹理 */
.grid-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: linear-gradient(#00000008 1px, transparent 1px), linear-gradient(90deg, #00000008 1px, transparent 1px);
  background-size: 20px 20px;
  pointer-events: none;
  z-index: 0;
}

/* 顶部装饰条 (警示条纹) */
.caution-stripe {
  height: 12px;
  width: 100%;
  position: absolute;
  top: 0; left: 0;
  background: repeating-linear-gradient(
    45deg,
    #000,
    #000 4px,
    #F4F1EA 4px,
    #F4F1EA 8px
  );
  opacity: 0.8;
  z-index: 1;
}

/* 顶部按钮组 */
.settings-trigger-btn, .flip-trigger-btn {
  position: absolute;
  top: 20px; 
  width: 36px; height: 36px;
  background: #fff;
  border: 2px solid #000;
  color: #000;
  cursor: pointer;
  z-index: 10;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
  box-shadow: 2px 2px 0 #000;
}
.settings-trigger-btn { left: 15px; }
.flip-trigger-btn { right: 15px; }

.settings-trigger-btn:hover, .flip-trigger-btn:hover {
  background: #000;
  color: #fff;
  transform: translate(-1px, -1px);
  box-shadow: 4px 4px 0 var(--cyber-red);
}

.tech-header {
  margin-top: 40px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  color: #666;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  opacity: 0.7;
}
.status-dot { width: 6px; height: 6px; background: #2ecc71; border-radius: 50%; box-shadow: 0 0 4px #2ecc71; }

/* 头像区域优化 */
.avatar-container {
  margin: 20px auto;
  position: relative;
  width: 110px; height: 110px;
}
.avatar-frame {
  width: 100%; height: 100%;
  border: 2px solid #000;
  padding: 4px;
  background: #fff;
  position: relative;
}
.avatar-frame img {
  width: 100%; height: 100%;
  object-fit: cover;
  filter: grayscale(20%) contrast(1.1);
  display: block;
}
/* 头像框装饰 Bracket */
.bracket-tl, .bracket-br {
  position: absolute;
  width: 10px; height: 10px;
  border-color: #000;
  border-style: solid;
  z-index: 5;
  transition: all 0.3s;
}
.bracket-tl { top: -2px; left: -2px; border-width: 2px 0 0 2px; }
.bracket-br { bottom: -2px; right: -2px; border-width: 0 2px 2px 0; }
.avatar-container:hover .bracket-tl { transform: translate(-3px, -3px); border-color: var(--cyber-red); }
.avatar-container:hover .bracket-br { transform: translate(3px, 3px); border-color: var(--cyber-red); }

/* 用户信息 */
.user-name {
  font-family: 'Noto Sans SC', sans-serif;
  font-weight: 900;
  font-size: 2rem;
  margin: 0 0 10px 0;
  color: #cc0f0f;
  line-height: 1.1;
  letter-spacing: -1px;
  text-transform: uppercase;
}

/* 角色与性别行 */
.user-meta-row {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
}
.meta-badge {
  display: inline-flex;
  align-items: center;
  border: 1px solid #000;
  padding: 2px 8px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.8rem;
  font-weight: 700;
  background: #fff;
}
.role-badge { color: var(--cyber-red); background: #000; color: #fff; }
.gender-badge { background: transparent; color: #000; gap: 4px; }
.icon-gender { font-size: 1rem; line-height: 1; }

/* 简介区域修复 */
.bio-container {
  position: relative;
  margin-bottom: 20px;
}
.bio-text {
  background: rgba(0,0,0,0.05); /* 浅灰底 */
  padding: 15px; /* 修复了原先巨大的80px padding */
  font-size: 0.85rem;
  color: #333;
  line-height: 1.6;
  border-left: 2px solid #000;
  text-align: left;
  font-family: 'Noto Sans SC', sans-serif;
  min-height: 60px;
}
.bio-deco {
  position: absolute;
  bottom: 0; right: 0;
  width: 10px; height: 10px;
  background: linear-gradient(135deg, transparent 50%, #000 50%);
}

/* 标签云 */
.meta-tags {
  display: flex;
  justify-content: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 25px;
}
.tag {
  background: transparent;
  font-size: 0.75rem;
  padding: 2px 6px;
  border: 1px dashed #666;
  font-family: 'JetBrains Mono', monospace;
  color: #555;
  transition: all 0.2s;
}
.tag:hover {
  border-style: solid;
  border-color: #000;
  color: #000;
  background: #fff;
}

/* 底部按钮 */
.action-row {
  display: flex;
  gap: 12px;
  margin-top: auto;
}
.action-btn {
  flex: 1;
  padding: 10px 0;
  border: 2px solid #000;
  font-family: 'JetBrains Mono', monospace;
  font-weight: bold;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
}
/* 主按钮 - 黑色 */
.action-btn.primary {
  background: #000;
  color: #fff;
}
.action-btn.primary:hover {
  background: var(--cyber-red);
  border-color: var(--cyber-red);
  box-shadow: 0 4px 0 rgba(0,0,0,0.2);
  color: #D92323;
  
}
/* 次级按钮 - 透明 */
.action-btn.secondary {
  background: transparent;
  color: #000;
}
.action-btn.secondary:hover {
  background: #D92323;
  color: #fff;
}
.action-btn.active-state {
  background: #D92323;
  color: #ffffff;
}
.action-btn:active {
  transform: translateY(2px);
}

/* 底部文字 */
.card-footer-deco {
  margin-top: 15px;
  display: flex;
  justify-content: space-between;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.5rem;
  color: #aaa;
  border-top: 1px solid #ddd;
  padding-top: 5px;
}

/* 滚动条美化 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #000; }
</style>