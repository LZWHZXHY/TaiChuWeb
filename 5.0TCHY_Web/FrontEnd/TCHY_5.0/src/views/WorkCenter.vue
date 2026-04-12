<template>
  <div class="doc-system-container">
    <header class="doc-header">
      <div class="header-inner">
        <div class="brand">
          <div class="brand-logo"></div>
          <h1 class="brand-title">TAICHU <span class="thin">ART DATA</span></h1>
        </div>
        
        <div class="header-meta">
          <div class="status-badge">
            <span class="pulse-cyan"></span> SYSTEM_STABLE
          </div>
          <div class="clock">{{ currentTime }}</div>
        </div>
      </div>
    </header>

    <div class="doc-layout">
      <aside class="doc-sidebar">
        <div class="sidebar-scroll">
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
                <div class="progress-bar"><div class="fill" :style="{ width: '65%' }"></div></div>
              </div>
            </div>
          </section>
        </div>
        
        <div class="sidebar-footer">
          <p>TERM_ID: 0X-ART-505</p>
          <p>CONFIDENTIAL DATA SOURCE</p>
        </div>
      </aside>

      <main class="doc-viewer">
        <div class="canvas-header">
          <div class="breadcrumb">DATABASE > {{ currentChannel.toUpperCase() }}</div>
          <div class="deco-line"></div>
        </div>
        
        <div class="canvas-body">
          <Transition name="doc-fade" mode="out-in">
            <div :key="currentChannel" class="component-mount">
              <ArtworkShowcase v-if="currentChannel === 'showcase'" />
              <ArtGallery v-else-if="currentChannel === 'gallery'" @refresh-stats="refreshGlobalStats" />
              <JointBoard v-else-if="currentChannel === 'joint'" />
              <Battlefield v-else-if="currentChannel === 'battlefield'" />
              <CertificationPanel v-else-if="currentChannel === 'certification'" />
              <WorldIpList v-else-if="currentChannel === 'world'" />
              <OcList v-else-if="currentChannel === 'ocList'" />
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
// 组件导入
import ArtGallery from '@/ArtCenter/ArtGallery.vue'; 
import JointBoard from '@/ArtCenter/UnionPanel.vue';
import Battlefield from '@/ArtCenter/Battlefield.vue';
import CertificationPanel from '@/ArtCenter/CertificationPanel.vue';
import WorldIpList from '@/ArtCenter/WorldIpList.vue';
import OcList from '@/ArtCenter/OcList.vue';
import ArtworkShowcase from '@/ArtCenter/ArtworkShowcase.vue';

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

const fetchTotalCount = async () => {
  try {
    const [galleryRes, ocRes] = await Promise.all([
      apiClient.get('/Drawing/AllArtWork'),
      apiClient.get('/Chai/stats/OCcount')
    ]);
    if (galleryRes.status === 200) artAmount.value = galleryRes.data;
    if (ocRes.status === 200) OCAmount.value = ocRes.data;
  } catch (e) { console.error(e); }
};

onMounted(() => { 
  fetchTotalCount(); 
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000); 
});
onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 极简文档色彩系统 */
.doc-system-container {
  --doc-bg: #F5F7F9;        /* 极浅灰蓝色背景 */
  --doc-white: #FFFFFF;     /* 纯白 */
  --doc-border: #E2E8F0;    /* 细腻边框色 */
  --doc-text-main: #1E293B; /* 深青灰文字 */
  --doc-text-sub: #64748B;  /* 次要文字 */
  --doc-accent: #06B6D4;    /* 亮青蓝色 (点缀) */
  --doc-accent-soft: rgba(6, 182, 212, 0.08);

  width: 100%;
  height: 100%;
  background-color: var(--doc-bg);
  color: var(--doc-text-main);
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* 顶部栏 */
.doc-header {
  height: 60px;
  background: var(--doc-white);
  border-bottom: 1px solid var(--doc-border);
  padding: 0 40px;
  display: flex;
  align-items: center;
  z-index: 100;
}

.header-inner {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand { display: flex; align-items: center; gap: 12px; }
.brand-logo { width: 24px; height: 24px; background: var(--doc-accent); border-radius: 4px; }
.brand-title { font-size: 1.1rem; font-weight: 800; letter-spacing: 1px; }
.brand-title .thin { font-weight: 300; color: var(--doc-text-sub); }

.header-meta { display: flex; gap: 30px; align-items: center; }
.status-badge { font-size: 0.75rem; font-weight: 700; color: var(--doc-accent); display: flex; align-items: center; gap: 8px; }
.pulse-cyan { width: 6px; height: 6px; background: var(--doc-accent); border-radius: 50%; box-shadow: 0 0 8px var(--doc-accent); }
.clock { font-family: monospace; font-size: 0.85rem; color: var(--doc-text-sub); }

/* 布局 */
.doc-layout { flex: 1; display: flex; overflow: hidden; }

/* 侧边栏 */
.doc-sidebar {
  width: 280px;
  background: var(--doc-white);
  border-right: 1px solid var(--doc-border);
  display: flex;
  flex-direction: column;
  padding: 30px 0;
}

.nav-group { margin-bottom: 40px; padding: 0 20px; }
.group-title { font-size: 0.7rem; font-weight: 700; color: var(--doc-text-sub); margin-bottom: 15px; padding-left: 10px; letter-spacing: 1px; }

.doc-nav-item {
  height: 40px;
  display: flex;
  align-items: center;
  padding: 0 15px;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 4px;
  transition: all 0.2s;
  color: var(--doc-text-sub);
}

.doc-nav-item:hover { background: var(--doc-bg); color: var(--doc-text-main); }
.doc-nav-item.active { background: var(--doc-accent-soft); color: var(--doc-accent); font-weight: 600; }

.item-index { font-family: monospace; font-size: 0.75rem; margin-right: 12px; opacity: 0.5; }
.item-text { font-size: 0.9rem; }

/* 统计区域 */
.doc-stat-grid { background: var(--doc-bg); padding: 15px; border-radius: 8px; }
.stat-row { display: flex; justify-content: space-between; margin-bottom: 10px; }
.stat-row .label { font-size: 0.75rem; color: var(--doc-text-sub); }
.stat-row .value { font-family: monospace; font-weight: 700; color: var(--doc-text-main); }
.progress-bar { height: 4px; background: var(--doc-border); border-radius: 2px; overflow: hidden; }
.fill { height: 100%; background: var(--doc-accent); transition: width 1s ease-out; }

/* 主画布区 */
.doc-viewer {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 40px;
  overflow-y: auto;
}

.canvas-header { margin-bottom: 30px; }
.breadcrumb { font-size: 0.75rem; color: var(--doc-accent); font-weight: 700; margin-bottom: 10px; letter-spacing: 1px; }
.deco-line { width: 60px; height: 2px; background: var(--doc-accent); }

.canvas-body {
  background: var(--doc-white);
  min-height: 100%;
  border-radius: 2px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  padding: 30px;
}

.sidebar-footer { margin-top: auto; padding: 0 30px; font-size: 0.65rem; color: var(--doc-text-sub); line-height: 1.6; opacity: 0.6; }

/* 动画 */
.doc-fade-enter-active, .doc-fade-leave-active { transition: all 0.25s ease; }
.doc-fade-enter-from { opacity: 0; transform: translateY(5px); }
.doc-fade-leave-to { opacity: 0; transform: translateY(-5px); }

@media (max-width: 1024px) {
  .doc-sidebar { display: none; }
  .doc-header { padding: 0 20px; }
  .doc-viewer { padding: 20px; }
}
</style>