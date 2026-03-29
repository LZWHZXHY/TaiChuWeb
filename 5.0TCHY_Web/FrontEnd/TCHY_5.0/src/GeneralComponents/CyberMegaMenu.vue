<template>
  <div class="mega-menu-trigger" @mouseenter="isOpen = true" @mouseleave="mouseleaveHandler">
    <div class="trigger-label" :class="{ 'active': isOpen }">
      <span class="bracket">[</span>
      <span class="icon-prefix">◈</span>
      <span class="text">社区枢纽</span>
      <span class="bracket">]</span>
    </div>

    <Transition name="hologram-fade">
      <div v-show="isOpen" class="mega-panel-wrapper">
        <div class="panel-header">
          <div class="header-tag">NODE_01</div>
          <div class="header-status">LIVE_SIGNAL</div>
          <div class="header-line"></div>
        </div>

        <div class="section-container">
          <div class="section-label">// 核心交互</div>
          <div class="compact-grid">
            <router-link 
              v-for="item in primaryItems" 
              :key="item.path"
              :to="item.path" 
              class="tactical-item"
              @click="isOpen = false"
            >
              <div class="item-icon">{{ item.icon }}</div>
              <div class="item-body">
                <div class="item-title">{{ $t(item.name) }}</div>
                <div class="item-desc">{{ item.desc }}</div>
              </div>
              <div class="corner-mark"></div>
            </router-link>
          </div>
        </div>

        <div class="secondary-container">
          <div class="section-label">// 职能终端</div>
          <div class="horizontal-links">
            <router-link 
              v-for="item in secondaryItems" 
              :key="item.path"
              :to="item.path" 
              class="mini-link"
              @click="isOpen = false"
            >
              <span class="marker">></span>
              <span class="label">{{ $t(item.name) }}</span>
              <span class="tag" v-if="item.tag">{{ item.tag }}</span>
            </router-link>
          </div>
        </div>

        <div class="panel-footer">
          <span class="footer-code">REF:HUB_v2.0.4</span>
          <div class="footer-dots"><span></span><span></span><span></span></div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>

import { ref } from 'vue';

const isOpen = ref(false);

const mouseleaveHandler = () => {
  isOpen.value = false;
};

// 替换你的 <script setup> 中的 primaryItems 数组：
const primaryItems = [
    { name: 'nav.push_center', path: '/MainPush', icon: '📡', desc: '情报与动态' },
    { name: 'nav.post_center', path: '/PostCenter', icon: '📡', desc: '帖子中心' },
    { name: 'nav.blog_center', path: '/BlogCenter', icon: '📡', desc: '博客中心' },
    { name: 'nav.chat_center', path: '/ChatCenter', icon: '📡', desc: '实时聊天室' },
  { name: 'nav.work_center', path: '/WorkCenter', icon: '🎨', desc: '艺术数据库' },

  // 🔥 新增：影音频段
  { name: 'nav.media', path: '/media', icon: '🎬', desc: '视觉档案库' }, 
  { name: 'nav.missionCenter', path: '/MissionCenter', icon: '🎯', desc: '系统任务' },
  { name: 'nav.entertainment', path: '/EntertainmentArea', icon: '🎮', desc: '娱乐协议' }
];

const secondaryItems = [
  { name: 'nav.RankCenter', path: '/RankCenter', tag: 'TOP' },
  { name: 'nav.resourse', path: '/Resource', tag: 'DATA' }
];
</script>

<style scoped>
.mega-menu-trigger {
  --red: #D92323;
  --ink: #111;
  --bg: #F4F1EA;
  --font-mono: 'JetBrains Mono', monospace;
  position: relative;
  height: 100%;
  display: flex;
  align-items: center;
}

/* --- 触发器 --- */
.trigger-label {
  padding: 0 12px; height: 100%; display: flex; align-items: center; gap: 6px;
  cursor: pointer; font-family: var(--font-mono); font-weight: 700; color: #555;
}
.trigger-label.active { color: var(--ink); }
.bracket { color: var(--red); opacity: 0; transition: 0.2s; }
.trigger-label.active .bracket { opacity: 1; }

/* --- 紧凑型面板 --- */
.mega-panel-wrapper {
  position: absolute; 
  top: calc(100% + 4px); 
  left: 0; 
  width: 420px; /* 大幅度压缩宽度，消除空白 */
  background: #fff;
  border: 1px solid var(--ink);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1);
  padding: 16px;
  z-index: 2000;
}

/* 头部装饰简化 */
.panel-header { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; font-family: var(--font-mono); font-size: 9px; }
.header-tag { background: var(--ink); color: #fff; padding: 1px 4px; }
.header-status { color: var(--red); font-weight: bold; }
.header-line { flex: 1; height: 1px; background: #eee; }

.section-label { font-family: var(--font-mono); font-size: 9px; color: #ccc; margin-bottom: 8px; letter-spacing: 1px; }

/* --- 核心交互：紧凑网格 --- */
.compact-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
  margin-bottom: 16px;
}

.tactical-item {
  text-decoration: none;
  background: var(--bg);
  padding: 10px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid transparent;
  transition: 0.2s;
  position: relative;
}

/* 最后一个元素（娱乐区）占满，保持对称 */
.tactical-item:last-child {
  grid-column: span 2;
}

.tactical-item:hover {
  background: #fff;
  border-color: var(--ink);
  transform: translateX(2px);
}

.item-icon { font-size: 1.1rem; }
.item-title { font-family: var(--font-mono); font-weight: 700; color: var(--ink); font-size: 12px; }
.item-desc { font-size: 10px; color: #999; margin-top: 1px; }

/* 右上角装饰点 */
.corner-mark { position: absolute; top: 4px; right: 4px; width: 3px; height: 3px; background: var(--ink); opacity: 0.2; }
.tactical-item:hover .corner-mark { background: var(--red); opacity: 1; }

/* --- 职能终端：横向排列 --- */
.horizontal-links {
  display: flex;
  gap: 12px;
  background: #fafafa;
  padding: 8px;
}

.mini-link {
  text-decoration: none;
  font-family: var(--font-mono);
  font-size: 11px;
  color: #666;
  display: flex;
  align-items: center;
  gap: 4px;
}

.mini-link:hover { color: var(--red); }
.mini-link .marker { color: var(--red); font-weight: bold; }
.mini-link .tag { font-size: 8px; background: #eee; padding: 0 3px; border-radius: 2px; }

/* --- 底部装饰 --- */
.panel-footer {
  margin-top: 16px;
  padding-top: 8px;
  border-top: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer-code { font-family: var(--font-mono); font-size: 8px; color: #ddd; }
.footer-dots { display: flex; gap: 3px; }
.footer-dots span { width: 3px; height: 3px; background: #eee; border-radius: 50%; }

/* --- 动画 --- */
.hologram-fade-enter-active, .hologram-fade-leave-active { transition: 0.2s ease-out; }
.hologram-fade-enter-from, .hologram-fade-leave-to { opacity: 0; transform: translateY(-4px); }
</style>