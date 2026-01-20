<template>
  <aside class="sidebar-left custom-scroll">
    <div class="id-flip-wrapper" :class="{ 'is-flipped': showIdArchive }">
      <div class="id-flipper">
        <!-- 卡片正面 -->
        <div class="id-face id-front">
          <div class="cyber-card id-card-content">
            <button class="settings-trigger-btn" title="用户资料设置" @click="emit('goToUserSettings')">
              <span class="icon">⚙</span>
            </button>
            <div class="menu-row" @click="emit('goToUserSettings')">
              <span class="row-icon"></span>
              <span class="row-label">[ID:{{ userID }}]</span>
            </div>

            <button class="flip-trigger-btn" @click="emit('toggleIdArchive')" title="查看详细资料">
              <span class="icon">▶</span>
              <span class="corner-deco"></span>
            </button>

            <div class="card-deco-top"></div>
            <div class="avatar-frame">
              <img :src="user.avatar" alt="avatar" />
              <div class="corner-brackets"></div>
              <div class="level-badge">LV.{{ user.level }}</div>
            </div>
            <div class="id-info">
              <h1 class="user-name">{{ user.nickname }}</h1>
              <div class="user-role">
                <span class="hash">#</span> {{ user.role }}
              </div>
              <p class="bio-text">
                {{ user.bio || '暂无个人简介数据...' }}
              </p>
              <div class="meta-tags">
                <span class="tag" v-for="tag in user.tags" :key="tag">{{ tag }}</span>
              </div>
            </div>
            <div class="action-row">
              <button 
                class="action-btn" 
                :class="isFollowing ? 'active-state' : 'primary'" 
                @click="emit('toggleFollow')"
              >
                {{ isFollowing ? '✓ 已关注' : '关注 + FOLLOW' }}
              </button>
              <button class="action-btn" @click="emit('handleMessage')">
                私信 // MSG
              </button>
            </div>
          </div>
        </div>

        <!-- 卡片背面 -->
        <div class="id-face id-back">
          <IdArchiveCard :user="user" @close="emit('toggleIdArchive')" />
        </div>
      </div>
    </div>

    <div class="cyber-card stats-card">
      <div class="panel-header">
        <span class="deco">·</span> 数据概览 // METRICS
      </div>
      <div class="stats-grid">
        <div class="stat-item">
          <span class="label">获赞数</span>
          <span class="val">{{ formatNumber(user.stats.likes) }}</span>
        </div>
        <div class="stat-item">
          <span class="label">浏览量</span>
          <span class="val">{{ formatNumber(user.stats.views) }}</span>
        </div>
        <div class="stat-item">
          <span class="label">作品数</span>
          <span class="val">{{ user.stats.works }}</span>
        </div>
        <div class="stat-item">
          <span class="label">粉丝数</span>
          <span class="val">{{ formatNumber(user.stats.followers) }}</span>
        </div>
      </div>
    </div>
    
    
  </aside>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'
import IdArchiveCard from '@/UserComponent/Profile/IdArchiveCard.vue'

// 定义接收的属性
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

// 定义触发的事件
const emit = defineEmits([
  'toggleIdArchive',
  'toggleFollow',
  'handleMessage',
  'goToUserSettings'
])

// 数字格式化工具方法
const formatNumber = (num) => {
  return num > 1000 ? (num / 1000).toFixed(1) + 'k' : num
}
</script>

<style scoped>
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://gs.jurieo.com/gemini/fonts-googleapis/css2?family=Caveat:wght@700&display=swap');

/* Sidebar 基础样式 */
.sidebar-left {
  width: 320px;
  display: flex; flex-direction: column; gap: 20px;
  overflow-y: auto;
  padding-right: 5px; 
  flex-shrink: 0;
  padding-top: 1%;
  padding-bottom: 1%;
  border-top: #000000 0px solid;
  height: 100%;
}

/* ID卡片翻转逻辑 */
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
  transition: transform 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.id-flip-wrapper.is-flipped .id-flipper {
  transform: rotateY(180deg);
}
.id-face {
  backface-visibility: hidden;
  width: 100%;
}
.id-front {
  position: relative;
  z-index: 2;
}
.id-back {
  position: absolute;
  top: 0; left: 0;
  height: 100%;
  transform: rotateY(180deg);
  z-index: 1;
  display: flex;
}

/* 卡片基础样式 */
.cyber-card {
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1);
  padding: 0;
  position: relative;
  transition: transform 0.2s;
}
.id-card-content { padding: 25px; text-align: center; height: 100%; }
.card-deco-top { height: 10px; background: repeating-linear-gradient(45deg, var(--black), var(--black) 5px, transparent 5px, transparent 10px); position: absolute; top:0; left:0; width:100%; opacity: 0.1; }

/* 翻转按钮 */
.flip-trigger-btn {
  position: absolute;
  top: 10px; right: 10px;
  width: 32px; height: 32px;
  background: var(--black);
  color: var(--white);
  border: none;
  cursor: pointer;
  z-index: 5;
  display: flex; align-items: center; justify-content: center;
  transition: 0.2s;
}
.flip-trigger-btn:hover { background: var(--red); transform: rotate(180deg); }
.flip-trigger-btn .icon { font-size: 1rem; line-height: 1; }
.corner-deco { position: absolute; bottom: -4px; left: -4px; width: 8px; height: 8px; border-bottom: 2px solid var(--black); border-left: 2px solid var(--black); }

/* 设置按钮动画 */
@keyframes move-rotate-loop {
  0% {
    transform: translateY(0) rotate(0deg);
  }
  50% {
    transform: translateY(100px) rotate(180deg);
  }
  75% {
    transform: translateY(100px) rotate(180deg);
  }
  100% {
    transform: translateY(0) rotate(360deg);
  }
}
.settings-trigger-btn {
  position: absolute;
  top: 10px; left: 10px;
  width: 32px; height: 32px;
  background: var(--black);
  color: var(--white);
  border: none;
  cursor: pointer;
  z-index: 5;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.2s ease;
  animation: move-rotate-loop 2s ease-in-out 8s infinite;
  transform: translateZ(0);
}
.settings-trigger-btn:hover {
  background: var(--red);
  animation-play-state: paused;
  transform: rotate(90deg);
  transition: background 0.2s ease, transform 0.2s ease;
}
.settings-trigger-btn .icon {
  font-size: 1.2rem;
  line-height: 1;
}

/* 头像区域 */
.avatar-frame { width: 120px; height: 120px; margin: 0 auto 20px; position: relative; border: 2px solid var(--black); padding: 4px; }
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(20%); }
.level-badge { position: absolute; bottom: -10px; left: 50%; transform: translateX(-50%); background: var(--red); color: white; padding: 2px 8px; font-size: 0.9rem; font-weight: bold; border: 2px solid var(--black); font-family: var(--heading); letter-spacing: 1px; }

/* 用户信息 */
.user-name { font-family: var(--sans); font-weight: 800; font-size: 1.8rem; margin: 0; line-height: 1.2; color: var(--black); }
.user-role { color: var(--red); font-weight: bold; font-size: 0.85rem; margin-bottom: 15px; font-family: var(--sans); height: auto;}
.bio-text { font-size: 0.85rem; color: #555; margin-bottom: 20px; line-height: 1.6; border-top: 1px dashed #ccc; border-bottom: 1px dashed #ccc; padding: 10px 0; font-family: var(--sans); text-align: left;}
.meta-tags { display: flex; justify-content: center; gap: 5px; flex-wrap: wrap; margin-bottom: 20px; }
.tag { background: #eee; font-size: 0.7rem; padding: 2px 8px; border: 1px solid #ccc; font-family: var(--sans); }

/* 操作按钮 */
.action-row { display: flex; gap: 10px; }
.action-btn { flex: 1; border: 2px solid var(--black); background: transparent; padding: 8px; font-family: var(--sans); font-weight: bold; cursor: pointer; transition: 0.2s; font-size: 0.8rem; }
.action-btn.primary { background: var(--black); color: var(--white); }
.action-btn.active-state { background: var(--white); color: var(--red); border-color: var(--red); }
.action-btn:hover { background: var(--red); color: var(--white); border-color: var(--black); }
.action-btn:active{ transform: translateY(2px); }

/* 数据概览卡片 */
.panel-header { background: var(--black); color: var(--white); padding: 8px 12px; font-weight: bold; font-size: 0.9rem; display: flex; align-items: center; gap: 8px; font-family: var(--sans); }
.stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1px; background: var(--black); border: 2px solid var(--black); margin: 15px; }
.stat-item { background: var(--white); padding: 15px 10px; display: flex; flex-direction: column; align-items: center; }
.stat-item .label { font-size: 0.75rem; color: #888; font-weight: bold; margin-bottom: 5px; font-family: var(--sans); }
.stat-item .val { font-family: var(--heading); font-size: 1.4rem; line-height: 1; color: var(--black); }

/* 成就卡片 */
.achieve-list { display: flex; flex-direction: column; }
.ach-item { display: flex; align-items: center; padding: 12px; border-bottom: 1px dashed #ccc; gap: 10px; }
.ach-item:last-child { border-bottom: none; }
.ach-icon { font-size: 1.5rem; }
.ach-info { flex: 1; }
.ach-name { font-weight: bold; font-size: 0.9rem; font-family: var(--sans); }
.ach-desc { font-size: 0.75rem; color: #888; font-family: var(--sans); margin-top: 2px; }
.ach-item.locked { opacity: 0.5; filter: grayscale(1); }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #f1f1f1; }
.custom-scroll::-webkit-scrollbar-thumb { background: #888; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }
</style>