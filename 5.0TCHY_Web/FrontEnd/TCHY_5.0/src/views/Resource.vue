<template>
  <div class="page-root">
    
    <TerrainBackground :color="0x00ff41" :speed="0.0004" />

    <div class="content-scroll custom-scrollbar">
      <div class="layout-container">
        
        <header class="hud-header glass-panel">
          <div class="brand-section">
            <div class="brand-info">
              <h1 class="title">太初资源库 <span class="version">V.4.3_LITE</span></h1>
              <div class="status-row">
                <span class="status-dot"></span>
                <span class="status-text">SYSTEM_ONLINE // 纯文本模式加载完毕</span>
              </div>
            </div>
          </div>

          <div class="stats-matrix">
            <div class="stat-item">
              <span class="label">INDEX_COUNT</span>
              <span class="value">{{ resources.length }}</span>
            </div>
            <div class="stat-divider"></div>
            <div class="stat-item">
              <span class="label">MATCHED</span>
              <span class="value active">{{ filteredResources.length }}</span>
            </div>
          </div>
        </header>

        <div class="control-bar-wrapper">
          <div class="control-bar glass-panel">
            <nav class="filter-tabs">
              <button 
                v-for="tab in tabs" 
                :key="tab.id"
                class="tab-item"
                :class="{ active: currentTab === tab.id }"
                @click="currentTab = tab.id"
              >
                {{ tab.name }}
              </button>
            </nav>

            <div class="search-module">
              <span class="prefix">QUERY://</span>
              <input 
                v-model="searchQuery" 
                type="text" 
                placeholder="输入协议关键词..." 
              />
              <div class="search-indicator" :class="{ 'blink': searchQuery.length > 0 }">_</div>
            </div>
          </div>
        </div>

        <div class="resource-matrix">
          <transition-group name="grid-anim">
            <div 
              v-for="(item, index) in filteredResources" 
              :key="item.id" 
              class="matrix-card glass-panel"
              :style="{ '--delay': `${index * 0.03}s` }"
            >
              <div class="holo-border"></div>
              <div class="left-accent"></div>

              <div class="card-inner">
                
                <div class="card-header-row">
                  <div class="title-group">
                    <span class="id-tag">REF_{{ item.id.toString().padStart(3, '0') }}</span>
                    <h3 class="res-name">{{ item.name }}</h3>
                  </div>
                  
                  <button class="launch-btn" @click.stop="handleLink(item)">
                    <span v-if="item.linkType === 'web'">OPEN ↗</span>
                    <span v-else>GET ⬇</span>
                  </button>
                </div>

                <div class="card-body-row">
                  <span class="type-badge">[{{ getTabName(item.type) }}]</span>
                  <p class="res-desc" :title="item.desc">{{ item.desc }}</p>
                </div>

                <div class="card-footer-row">
                  <div class="tags-scroller">
                    <span v-for="tag in item.tags" :key="tag" class="tech-tag">#{{ tag }}</span>
                  </div>
                  <div class="heat-display" title="访问热度">
                    <div class="heat-bars">
                      <span v-for="n in 5" :key="n" class="h-bar" :class="{ active: (item.heatValue / 20) >= n }"></span>
                    </div>
                  </div>
                </div>
              
              </div>
            </div>
          </transition-group>
        </div>

        <div v-if="filteredResources.length === 0" class="empty-zone">
          <p>NULL_POINTER_EXCEPTION // 无匹配数据</p>
          <button @click="resetFilters" class="reset-cmd">重置过滤指令</button>
        </div>

      </div>

      <footer class="page-footer">
        <div class="footer-line"></div>
        <p>© 2026 太初寰宇 TACTICAL ARCHIVE // TEXT_MODE_ENABLED</p>
      </footer>

    </div>

    <transition name="toast-slide">
      <div v-if="showToast" class="tactical-toast">
        <span class="icon">✅</span>
        <span class="msg">CLIPBOARD_WRITE_SUCCESS</span>
      </div>
    </transition>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import TerrainBackground from '@/BackGroundComponents/TerrainBackground.vue';

// --- 基础数据 ---
const searchQuery = ref('');
const currentTab = ref('all');
const showToast = ref(false);

const tabs = [
  { id: 'all', name: '全域资源' },
  { id: 'dev', name: '开发构建' },
  { id: 'design', name: '设计资产' },
  { id: 'ai', name: 'AI 引擎' },
  { id: 'media', name: '数字媒体' },
];

// 去掉了 icon 字段
const resources = ref([
  {
    id: 1,
    name: 'Excalidraw',
    desc: '虚拟手绘风格白板工具，适合快速绘制架构图、流程图，支持端到端加密。',
    type: 'dev',
    linkType: 'web',
    url: 'https://excalidraw.com',
    tags: ['绘图', '开源', '白板'],
    heatValue: 92
  },
  {
    id: 2,
    name: 'Cyber_UI_Kit_V2',
    desc: '包含50+高精度赛博朋克风格界面的 Figma 组件库源文件。',
    type: 'design',
    linkType: 'file',
    url: '#',
    tags: ['Figma', 'UI组件', '未来派'],
    heatValue: 65
  },
  {
    id: 3,
    name: 'Raycast',
    desc: 'MacOS 效率启动器，可完全替代 Spotlight，支持脚本扩展与工作流自动化。',
    type: 'dev',
    linkType: 'web',
    url: 'https://raycast.com',
    tags: ['Mac', '效率', '工具'],
    heatValue: 98
  },
  {
    id: 4,
    name: 'Midjourney 咒语库',
    desc: '精选 AI 绘画提示词大全，包含风格修正与参数调优指南。',
    type: 'ai',
    linkType: 'file',
    url: '#',
    tags: ['AI', 'Prompt', '艺术'],
    heatValue: 80
  },
  {
    id: 5,
    name: 'Three.js 文档',
    desc: 'Web 3D 图形库官方文档中文版与精选案例解析。',
    type: 'dev',
    linkType: 'web',
    url: 'https://threejs.org',
    tags: ['3D', 'WebGL', '前端'],
    heatValue: 85
  },
  {
    id: 6,
    name: 'Pexels 4K',
    desc: '无版权高清视频素材库，适合作为网站背景或演示素材。',
    type: 'media',
    linkType: 'web',
    url: 'https://pexels.com',
    tags: ['素材', '视频', '免费'],
    heatValue: 70
  },{
    id: 7,
    name: 'image2url',
    desc: '将图片转换成URL形式，极简图床工具。',
    type: 'dev',
    linkType: 'web',
    url: 'https://www.image2url.com/zh',
    tags: ['素材', '转换', '工具'],
    heatValue: 70
  },{
    id: 8,
    name: 'JS Bin',
    desc: '可以预览html页面效果的网站',
    type: 'dev',
    linkType: 'web',
    url: 'https://jsbin.com/?html,output',
    tags: ['html', '预览', '工具'],
    heatValue: 70
  }
]);

// --- 逻辑处理 ---
const filteredResources = computed(() => {
  return resources.value.filter(item => {
    const matchTab = currentTab.value === 'all' || item.type === currentTab.value;
    const q = searchQuery.value.toLowerCase();
    const matchSearch = item.name.toLowerCase().includes(q) || 
                        item.desc.toLowerCase().includes(q) ||
                        item.tags.some(t => t.toLowerCase().includes(q));
    return matchTab && matchSearch;
  });
});

const getTabName = (type) => {
  const t = tabs.find(t => t.id === type);
  return t ? t.name : '未知';
};

const handleLink = (item) => {
  if (item.linkType === 'web') {
    window.open(item.url, '_blank');
  } else {
    navigator.clipboard.writeText(item.url);
    showToast.value = true;
    setTimeout(() => showToast.value = false, 2000);
  }
};

const resetFilters = () => {
  searchQuery.value = '';
  currentTab.value = 'all';
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;500;700&family=JetBrains+Mono:wght@400;700&display=swap');

.page-root {
  --neon: #00ff41;
  --neon-dim: rgba(0, 255, 65, 0.2);
  --glass-bg: rgba(10, 15, 12, 0.8);
  --glass-border: rgba(255, 255, 255, 0.1);
  --card-bg: rgba(255, 255, 255, 0.02);
  --text-main: #f0f0f0;
  --text-muted: #888;
  
  width: 100vw; height: 100vh;
  position: relative;
  font-family: 'Noto Sans SC', sans-serif;
  color: var(--text-main);
  background: #000;
  overflow: hidden;
}

.content-scroll {
  position: relative; z-index: 10;
  width: 100%; height: 100%;
  overflow-y: auto; overflow-x: hidden;
}

.layout-container {
  width: 100%; max-width: 1600px;
  margin: 0 auto; padding: 40px 20px;
  min-height: 90vh;
  display: flex; flex-direction: column; gap: 30px;
}

.glass-panel {
  background: var(--glass-bg);
  backdrop-filter: blur(12px);
  border: 1px solid var(--glass-border);
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
}

/* === Header === */
.hud-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 20px 30px; border-radius: 4px;
  border-left: 4px solid var(--neon);
}
.title { margin: 0; font-size: 1.5rem; font-weight: 900; letter-spacing: 1px; }
.version { font-size: 0.8rem; color: var(--neon); font-family: 'JetBrains Mono'; margin-left: 5px; }
.status-row { display: flex; align-items: center; gap: 8px; font-size: 0.75rem; color: var(--text-muted); margin-top: 4px; font-family: 'JetBrains Mono'; }
.status-dot { width: 6px; height: 6px; background: var(--neon); border-radius: 50%; box-shadow: 0 0 5px var(--neon); }

.stats-matrix { display: flex; gap: 20px; align-items: center; font-family: 'JetBrains Mono'; }
.stat-item { display: flex; flex-direction: column; align-items: flex-end; }
.stat-item .label { font-size: 0.7rem; color: var(--text-muted); }
.stat-item .value { font-size: 1.2rem; font-weight: bold; }
.stat-item .value.active { color: var(--neon); }
.stat-divider { width: 1px; height: 24px; background: var(--glass-border); }

/* === Control Bar === */
.control-bar-wrapper { position: sticky; top: 20px; z-index: 100; }
.control-bar {
  display: flex; flex-wrap: wrap; gap: 20px; align-items: center;
  padding: 10px 20px; border-radius: 4px;
}
.filter-tabs { display: flex; gap: 5px; overflow-x: auto; flex: 1; }
.tab-item {
  background: transparent; border: 1px solid transparent;
  color: var(--text-muted); padding: 6px 16px; border-radius: 2px;
  cursor: pointer; font-size: 0.9rem; font-weight: 500; transition: all 0.2s;
}
.tab-item:hover { color: #fff; background: rgba(255,255,255,0.05); }
.tab-item.active {
  background: var(--neon-dim); color: var(--neon); border: 1px solid var(--neon);
}

.search-module {
  display: flex; align-items: center;
  background: rgba(0,0,0,0.4); padding: 8px 15px; border-radius: 2px;
  border: 1px solid var(--glass-border); min-width: 260px;
}
.search-module:focus-within { border-color: var(--neon); }
.prefix { font-family: 'JetBrains Mono'; color: var(--neon); font-size: 0.8rem; margin-right: 10px; }
.search-module input {
  background: none; border: none; outline: none; color: #fff; flex: 1; font-size: 0.9rem;
}
.search-indicator { color: var(--neon); font-weight: bold; }
.blink { animation: blink 1s step-end infinite; }

/* === No-Icon Matrix Card (关键修改) === */
.resource-matrix {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.matrix-card {
  position: relative; border-radius: 2px;
  transition: all 0.3s ease; background: var(--card-bg);
  animation: fade-in 0.5s both; animation-delay: var(--delay);
}
.matrix-card:hover {
  transform: translateY(-2px); background: rgba(255, 255, 255, 0.04);
  border-color: var(--neon);
}

/* 左侧亮条装饰 */
.left-accent {
  position: absolute; left: 0; top: 0; bottom: 0; width: 2px;
  background: var(--neon); opacity: 0; transition: opacity 0.3s;
}
.matrix-card:hover .left-accent { opacity: 1; }

.card-inner {
  position: relative; z-index: 10; padding: 24px;
  display: flex; flex-direction: column; height: 100%; gap: 16px;
}

/* 头部重构：纯文字对齐 */
.card-header-row { display: flex; justify-content: space-between; align-items: flex-start; }
.title-group { display: flex; flex-direction: column; gap: 4px; }
.id-tag { font-family: 'JetBrains Mono'; font-size: 0.65rem; color: #555; letter-spacing: 1px; }
.res-name { font-size: 1.2rem; font-weight: bold; margin: 0; color: #fff; letter-spacing: 0.5px; }

.launch-btn {
  background: transparent; border: 1px solid var(--glass-border);
  color: var(--neon); font-family: 'JetBrains Mono'; font-size: 0.75rem; font-weight: bold;
  padding: 6px 12px; cursor: pointer; transition: all 0.2s;
}
.launch-btn:hover { background: var(--neon); color: #000; border-color: var(--neon); }

/* 中部 */
.card-body-row { flex: 1; display: flex; flex-direction: column; gap: 8px; }
.type-badge { 
  align-self: flex-start;
  font-family: 'JetBrains Mono'; font-size: 0.7rem; color: var(--text-muted); 
}
.res-desc { 
  font-size: 0.9rem; color: #aaa; line-height: 1.6;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}

/* 底部：标签 + 热度条 */
.card-footer-row {
  display: flex; justify-content: space-between; align-items: center;
  border-top: 1px solid rgba(255,255,255,0.05); padding-top: 16px; margin-top: auto;
}
.tags-scroller { display: flex; gap: 8px; max-width: 60%; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
.tech-tag { font-size: 0.75rem; color: #666; font-family: 'JetBrains Mono'; }
.tech-tag::before { content: '#'; color: #444; }

/* 信号格风格热度条 */
.heat-bars { display: flex; gap: 2px; align-items: flex-end; height: 12px; }
.h-bar { width: 4px; background: #333; height: 60%; }
.h-bar:nth-child(2) { height: 70%; }
.h-bar:nth-child(3) { height: 80%; }
.h-bar:nth-child(4) { height: 90%; }
.h-bar:nth-child(5) { height: 100%; }
.h-bar.active { background: var(--neon); box-shadow: 0 0 4px var(--neon); }

/* 杂项 */
.empty-zone { text-align: center; grid-column: 1 / -1; padding: 60px; border: 1px dashed #333; color: #666; font-family: 'JetBrains Mono'; }
.reset-cmd { margin-top: 15px; background: none; border: 1px solid #444; color: var(--neon); padding: 8px 20px; cursor: pointer; }
.page-footer { margin-top: auto; padding: 40px 0; text-align: center; color: #444; font-size: 0.8rem; font-family: 'JetBrains Mono'; }
.footer-line { width: 40px; height: 2px; background: var(--neon); margin: 0 auto 10px; opacity: 0.5; }

/* Toast & Anim */
.tactical-toast {
  position: fixed; bottom: 30px; right: 30px; background: var(--neon); color: #000;
  padding: 12px 24px; font-weight: bold; display: flex; align-items: center; gap: 10px;
  box-shadow: 0 0 20px var(--neon-dim); z-index: 999;
}
@keyframes blink { 0%,100%{opacity:1} 50%{opacity:0} }
@keyframes fade-in { from{opacity:0; transform:translateY(10px)} to{opacity:1; transform:translateY(0)} }
.toast-slide-enter-active, .toast-slide-leave-active { transition: all 0.3s ease; }
.toast-slide-enter-from, .toast-slide-leave-to { transform: translateY(20px); opacity: 0; }

.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #050505; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #333; border-radius: 3px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: var(--neon); }
</style>