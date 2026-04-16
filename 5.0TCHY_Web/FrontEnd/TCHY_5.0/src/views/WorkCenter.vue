<template>
  <div class="doc-system-container">
    <header class="doc-header">
      <div class="header-inner">
        <div class="brand">
          <div class="brand-logo"></div>
          <h1 class="brand-title">太初 <span class="thin">艺术档案系统</span></h1>
        </div>
        
        <div class="header-meta">
          <div class="status-badge">
            <span class="pulse-cyan"></span> 系统运行稳定
          </div>
          <div class="clock">{{ currentTime }}</div>
        </div>
      </div>
    </header>

    <div class="doc-layout">
      <aside class="doc-sidebar">
        <div class="sidebar-scroll md-scrollbar">
          <section class="nav-group">
            <h3 class="group-title">索引 / INDEX</h3>
            <nav class="doc-nav">
              <div 
                v-for="item in navItems" 
                :key="item.id"
                class="doc-nav-item" 
                :class="{ active: currentChannel === item.id }"
                @click="switchChannel(item.id)"
              >
                <span class="item-index">{{ item.index }}</span>
                <span class="item-text">{{ item.name }}</span>
              </div>
            </nav>
          </section>

          <section class="nav-group stats-section">
            <h3 class="group-title">概览 / OVERVIEW</h3>
            <div class="doc-stat-grid">
              <div class="stat-row">
                <span class="label">作品档案</span>
                <span class="value">{{ artAmount }}</span>
              </div>
              <div class="stat-row">
                <span class="label">OC 录入</span>
                <span class="value">{{ OCAmount }}</span>
              </div>
              <div class="stat-progress">
                <div class="progress-bar">
                  <div class="fill" :style="{ width: '65%' }"></div>
                </div>
              </div>
            </div>
          </section>
        </div>
        
        <div class="sidebar-footer">
          <p>终端 ID: 0X-ART-505</p>
          <p>机密数据来源 · 未经授权禁止访问</p>
        </div>
      </aside>

      <main class="doc-viewer">
        <div class="canvas-header">
          <div class="breadcrumb">数据库 > {{ currentChannel.toUpperCase() }}</div>
          <div class="deco-line"></div>
        </div>
        
        <div class="canvas-body">
          <Transition name="doc-fade" mode="out-in">
            <div :key="currentChannel" class="component-mount">
              <ArtworkShowcase v-if="currentChannel === 'showcase'" />
              <ArtGallery v-else-if="currentChannel === 'gallery'" @refresh-stats="fetchTotalCount" />
              <JointBoard v-else-if="currentChannel === 'joint'" />
              <Battlefield v-else-if="currentChannel === 'battlefield'" />
              <CertificationPanel v-else-if="currentChannel === 'certification'" />
              <WorldIpList v-else-if="currentChannel === 'world'" />
              <OcList v-else-if="currentChannel === 'ocList'" />
              <OcGallery v-else-if="currentChannel === 'OcGallery'" />
            </div>
          </Transition>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'; 
import { useRoute, useRouter } from 'vue-router'; 
import apiClient from '@/utils/api'; 

// 组件异步导入或直接导入
import ArtGallery from '@/ArtCenter/ArtGallery.vue'; 
import JointBoard from '@/ArtCenter/UnionPanel.vue';
import Battlefield from '@/ArtCenter/Battlefield.vue';
import CertificationPanel from '@/ArtCenter/CertificationPanel.vue';
import WorldIpList from '@/ArtCenter/WorldIpList.vue';
import OcList from '@/ArtCenter/OcList.vue';
import ArtworkShowcase from '@/ArtCenter/ArtworkShowcase.vue';
import OcGallery from '@/ArtCenter/OcGallery.vue';

const route = useRoute();
const router = useRouter();

const navItems = [
  { id: 'showcase', index: '00', name: '精品收藏' },
  { id: 'gallery', index: '01', name: '寰宇画廊' },
  { id: 'joint', index: '02', name: '联合企划' },
  { id: 'certification', index: '03', name: '创作认证' },
  { id: 'battlefield', index: '04', name: '太初约战' },
  { id: 'world', index: '05', name: '世界观收录' },
  { id: 'ocList', index: '06', name: 'OC 数据库' },
  { id: 'OcGallery', index: '07', name: 'OC文档库'}
];

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;
const currentChannel = ref(route.query.tab || 'showcase');

const artAmount = ref(0);
const OCAmount = ref(0);

const switchChannel = (name) => {
  currentChannel.value = name;
  router.replace({ query: { ...route.query, tab: name } });
};

watch(() => route.query.tab, (newTab) => {
  if (newTab && newTab !== currentChannel.value) currentChannel.value = newTab;
});

// 统计数据拉取逻辑
const fetchTotalCount = async () => {
  try {
    const [galleryRes, ocRes] = await Promise.all([
      apiClient.get('/Drawing/AllArtWork'),
      apiClient.get('/Chai/stats/OCcount')
    ]);
    if (galleryRes.status === 200) artAmount.value = galleryRes.data;
    if (ocRes.status === 200) OCAmount.value = ocRes.data;
  } catch (e) { 
    console.error("数据加载失败", e); 
  }
};

onMounted(() => { 
  fetchTotalCount(); 
  clockTimer = setInterval(() => { 
    currentTime.value = new Date().toLocaleTimeString(); 
  }, 1000); 
});

onUnmounted(() => {
  if (clockTimer) clearInterval(clockTimer);
});
</script>

<style scoped>
/* 极简文档色彩系统 - MD 风格 */
.doc-system-container {
  --doc-bg: #F8FAFC;        /* 更清爽的背景色 */
  --doc-white: #FFFFFF;
  --doc-border: #E2E8F0;
  --doc-text-main: #0F172A; /* Slate 900 */
  --doc-text-sub: #64748B;  /* Slate 500 */
  --doc-accent: #0284C7;    /* 亮蓝色 */
  --doc-accent-soft: rgba(2, 132, 199, 0.08);

  width: 100%;
  height: 100%;
  background-color: var(--doc-bg);
  color: var(--doc-text-main);
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  display: flex;
  flex-direction: column;
  overflow: hidden; /* 禁止外层滚动 */
}

/* 顶部栏 */
.doc-header {
  height: 64px;
  background: var(--doc-white);
  border-bottom: 1px solid var(--doc-border);
  padding: 0 40px;
  display: flex;
  align-items: center;
  z-index: 100;
  flex-shrink: 0;
}

.header-inner {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand { display: flex; align-items: center; gap: 12px; }
.brand-logo { width: 20px; height: 20px; background: var(--doc-accent); border-radius: 4px; }
.brand-title { font-size: 1.1rem; font-weight: 700; letter-spacing: 0.5px; }
.brand-title .thin { font-weight: 300; color: var(--doc-text-sub); margin-left: 4px; }

.header-meta { display: flex; gap: 32px; align-items: center; }
.status-badge { font-size: 0.75rem; font-weight: 600; color: var(--doc-accent); display: flex; align-items: center; gap: 8px; }
.pulse-cyan { width: 6px; height: 6px; background: var(--doc-accent); border-radius: 50%; box-shadow: 0 0 6px var(--doc-accent); }
.clock { font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; color: var(--doc-text-sub); }

/* 布局结构 */
.doc-layout { 
  flex: 1; 
  display: flex; 
  overflow: hidden; /* 核心：防止容器被内部撑开导致滚动条混乱 */
}

/* 侧边栏 */
.doc-sidebar {
  width: 280px;
  background: var(--doc-white);
  border-right: 1px solid var(--doc-border);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}

.sidebar-scroll { 
  flex: 1;
  overflow-y: auto;
  padding: 32px 0;
}

.nav-group { margin-bottom: 40px; padding: 0 20px; }
.group-title { font-size: 0.7rem; font-weight: 800; color: var(--doc-text-sub); margin-bottom: 16px; padding-left: 12px; letter-spacing: 1.5px; }

.doc-nav-item {
  height: 44px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  border-radius: 8px;
  cursor: pointer;
  margin-bottom: 4px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  color: var(--doc-text-sub);
}

.doc-nav-item:hover { background: #F1F5F9; color: var(--doc-text-main); }
.doc-nav-item.active { background: var(--doc-accent-soft); color: var(--doc-accent); font-weight: 600; }

.item-index { font-family: 'JetBrains Mono', monospace; font-size: 0.7rem; margin-right: 14px; opacity: 0.6; }
.item-text { font-size: 0.9rem; }

/* 统计概览区域 */
.doc-stat-grid { background: #F8FAFC; padding: 20px; border-radius: 12px; border: 1px solid #F1F5F9; }
.stat-row { display: flex; justify-content: space-between; margin-bottom: 12px; }
.stat-row .label { font-size: 0.75rem; color: var(--doc-text-sub); font-weight: 500; }
.stat-row .value { font-family: 'JetBrains Mono', monospace; font-weight: 700; color: var(--doc-text-main); }
.progress-bar { height: 6px; background: #E2E8F0; border-radius: 3px; overflow: hidden; }
.fill { height: 100%; background: var(--doc-accent); border-radius: 3px; transition: width 1.2s cubic-bezier(0.4, 0, 0.2, 1); }

/* 主体容器 (MD Canvas) */
.doc-viewer {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 32px 40px;
  overflow: hidden; /* 禁止主区域直接出现整体滚动条 */
}

.canvas-header { margin-bottom: 24px; flex-shrink: 0; }
.breadcrumb { font-size: 0.75rem; color: var(--doc-accent); font-weight: 800; margin-bottom: 8px; letter-spacing: 1px; }
.deco-line { width: 40px; height: 3px; background: var(--doc-accent); border-radius: 2px; }

/* 核心：确保 body 撑满垂直空间 */
.canvas-body {
  background: var(--doc-white);
  flex: 1; 
  display: flex;
  flex-direction: column;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05), 0 10px 15px -3px rgba(0,0,0,0.02);
  position: relative;
  overflow: hidden; /* 让内部子组件自己处理内部滚动 */
  border: 1px solid var(--doc-border);
}

/* 挂载点容器也需要 Flex 撑满 */
.component-mount {
  flex: 1;
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
}

.sidebar-footer { 
  margin-top: auto; 
  padding: 24px 32px; 
  font-size: 0.65rem; 
  color: #94A3B8; 
  line-height: 1.8; 
  border-top: 1px solid #F1F5F9;
}

/* 滚动条美化 */
.md-scrollbar::-webkit-scrollbar { width: 5px; }
.md-scrollbar::-webkit-scrollbar-thumb { background: #CBD5E1; border-radius: 10px; }

/* 细腻过渡动画 */
.doc-fade-enter-active, .doc-fade-leave-active { transition: all 0.2s ease; }
.doc-fade-enter-from { opacity: 0; transform: translateY(8px); }
.doc-fade-leave-to { opacity: 0; transform: translateY(-8px); }

@media (max-width: 1200px) {
  .doc-sidebar { width: 240px; }
  .doc-viewer { padding: 24px; }
}
</style>