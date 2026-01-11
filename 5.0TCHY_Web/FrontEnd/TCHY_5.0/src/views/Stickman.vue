<template>
  <div class="huochair-board">
    <header class="board-header">
      <h1>🔥 火柴人板块</h1>
    </header>
    <div class="board-container">
      <!-- 左侧导航栏 -->
      <nav class="sidebar-nav">
        <ul class="nav-list">
          <li
            class="nav-item"
            v-for="tab in tabs"
            :key="tab.id"
            :class="{ active: currentTab === tab.id }"
            @click="currentTab = tab.id"
          >
            <span class="nav-icon">{{ tab.icon }}</span>
            <span class="nav-text">{{ tab.name }}</span>
          </li>
        </ul>
      </nav>

      <!-- 右侧内容区 -->
      <section class="content-area">
        <transition name="fade-slide" mode="out-in">
          <component
            :is="getCurrentTabComponent()"
            :key="currentTab"
            :class="['content-panel', { 'content-panel--full': currentTab === 'society' }]"
          />
        </transition>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

/* 组件保留你提供的路径地址 */
import ArtistAssessment from '@/火柴人板块/ArtistAssessment.vue'
import MatchstickSociety from '@/火柴人板块/MatchstickSociety.vue'
import TaichuBattlefield from '@/火柴人板块/TaichuBattlefield.vue'
import UnionZone from '@/火柴人板块/UnionZone.vue'
import WorldviewProject from '@/火柴人板块/WorldviewProject.vue'
import IntroArea from '@/火柴人板块/IntroArea.vue'
import 作品推荐 from '@/火柴人板块/作品推荐.vue'

const tabs = [
  { id: 'intro', name: '板块介绍', icon: '📖' },
  { id: 'artist', name: '画师考核', icon: '🎨' },
  { id: 'recommendations', name: '作品推荐', icon: '⭐' },
  { id: 'society', name: '柴圈社团', icon: '🍃' },
  { id: 'battle', name: '太初约战场', icon: '⚡' },
  { id: 'union', name: '联合区域', icon: '🤝' },
  { id: 'worldview', name: '世界观企划', icon: '🌏' }
]

const currentTab = ref(tabs[0].id)

function getCurrentTabComponent() {
  switch (currentTab.value) {
    case 'artist':
      return ArtistAssessment
    case 'society':
      return MatchstickSociety
    case 'battle':
      return TaichuBattlefield
    case 'union':
      return UnionZone
    case 'worldview':
      return WorldviewProject
    case 'intro':
      return IntroArea
    case 'recommendations':
      return 作品推荐
    default:
      return IntroArea
  }
}
</script>

<style scoped>
/* Design tokens */
:root {
  --main-bg: #f4f7fb;
  --header-bg: #111827;
  --header-text: #ffffff;

  --sidebar-bg: #ffffff;
  --sidebar-text: #4b5563;
  --sidebar-active-text: #111827;
  --sidebar-border: #e5e7eb;
  --sidebar-hover-bg: #f3f6fc;
  --sidebar-active-bg: #edf2ff;
  --accent: #2563eb;
  --accent-weak: #e8f1ff;

  --content-bg: #ffffff;
  --content-text: #111827;

  --radius-lg: 16px;
  --radius-md: 12px;

  --shadow-sm: 0 2px 10px rgba(17, 24, 39, 0.06);
  --shadow-md: 0 10px 30px rgba(17, 24, 39, 0.08);

  --focus-ring: 0 0 0 3px rgba(37, 99, 235, .15);
  --trans: 180ms cubic-bezier(.22,.61,.36,1);
}

.huochair-board {
  min-height: 100vh;
  background: var(--main-bg);
  display: flex;
  flex-direction: column;
  font-family: ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, "Helvetica Neue", Arial;
}

/* 顶部标题 */
.board-header {
  background: var(--header-bg);
  color: var(--header-text);
  padding: 18px 24px;
  text-align: center;
  font-size: clamp(20px, 2.6vw, 28px);
  font-weight: 800;
  letter-spacing: .2px;
  box-shadow: var(--shadow-sm);
}

/* 主体布局 */
.board-container {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 0;
  flex: 1;
  align-items: start;
  min-height: calc(100vh - 80px);
}

/* 左侧导航栏 */
.sidebar-nav {
  position: sticky;
  top: 0;
  align-self: start;
  background: var(--sidebar-bg);
  padding: 12px 0;
  border-right: 1px solid var(--sidebar-border);
}

.nav-list {
  list-style: none;
  padding: 6px 6px 6px 0;
  margin: 0;
}

.nav-item {
  display: grid;
  grid-template-columns: 28px 1fr;
  align-items: center;
  gap: 10px;
  padding: 12px 16px 12px 18px;
  margin-right: 8px;
  color: var(--sidebar-text);
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  border-left: 3px solid transparent;
  transition: background var(--trans), color var(--trans), border-color var(--trans), transform var(--trans);
}

.nav-item:hover {
  background: var(--sidebar-hover-bg);
  color: var(--sidebar-active-text);
}

.nav-item.active {
  background: var(--sidebar-active-bg);
  color: var(--sidebar-active-text);
  border-left-color: var(--accent);
  box-shadow: inset -6px 0 14px rgba(17,24,39, .04);
}

.nav-item:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.nav-icon {
  font-size: 18px;
  line-height: 1;
  opacity: .9;
}

.nav-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 右侧内容区：允许面板横铺 */
.content-area {
  padding: clamp(16px, 2.4vw, 28px);
  background: var(--main-bg);
  display: block;           /* 关键：让子元素可 100% 宽度铺满 */
}

/* 默认内容卡片（其他 Tab 保持卡片宽度） */
.content-panel {
  width: min(1080px, 92vw);
  background: var(--content-bg);
  color: var(--content-text);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
  padding: clamp(18px, 2.4vw, 28px);
  border: 1px solid #eef2f7;
  animation: fadein .25s ease-out;
  margin: 0 auto;          /* 居中显示 */
}

/* 柴圈社团：横铺整个页面（占满右侧工作区宽度） */
.content-panel--full {
  width: 100% !important;
  max-width: none !important;
  margin: 0;                /* 取消居中外边距 */
  border-radius: 0;         /* 可选：去除圆角，彻底铺满 */
  border: none;             /* 可选：去边框，更简洁 */
  box-shadow: none;         /* 可选：去阴影，更贴近“横铺页面” */
  padding: 0;               /* 可选：交由内层页面自己控制留白 */
}

/* 过渡动画 */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity .25s ease, transform .25s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

@keyframes fadein {
  from { opacity: 0; transform: translateY(4px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* 响应式：窄屏顶部横向标签 */
@media (max-width: 900px) {
  .board-container {
    display: flex;
    flex-direction: column;
    min-height: unset;
  }

  .sidebar-nav {
    position: static;
    border-right: none;
    border-bottom: 1px solid var(--sidebar-border);
    padding: 10px;
    overflow-x: auto;
    white-space: nowrap;
  }

  .nav-list {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: max-content;
    gap: 8px;
    padding: 0 4px;
  }

  .nav-item {
    border-radius: var(--radius-md);
    margin: 0;
    border-left-width: 0;
    padding: 10px 14px;
    background: transparent;
  }
  .nav-item.active {
    background: var(--accent-weak);
    color: var(--sidebar-active-text);
    border-left-color: transparent;
  }

  .content-panel {
    width: 100%;
    padding: 16px;
    border-radius: var(--radius-md);
  }

  .content-panel--full {
    padding: 0;
    border-radius: 0;
  }
}

/* 触控优化 */
@media (hover: none) {
  .nav-item { padding: 14px 18px; }
}
</style>