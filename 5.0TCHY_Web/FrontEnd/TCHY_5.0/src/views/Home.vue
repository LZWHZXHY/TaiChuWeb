<template>
  <div class="cyber-archive-system">
    <div class="grid-bg moving-grid"></div>

    <div class="archive-container">
      
      <header class="sys-header">
        <div class="sys-title">
          <span class="icon">■</span>
          <span class="text">COMMUNITY_ARCHIVES // 社区档案库</span>
        </div>
        <div class="sys-meta">
          <span>OP_MODE: READ_ONLY</span>
          <span class="blink">_</span>
        </div>
      </header>

      <div class="archive-body">
        <nav class="cyber-sidebar custom-scroll">
          <div class="sidebar-deco">
            // NAV_PROTOCOL_V2 <br>
            // SELECT_MODULE
          </div>
          
          <ul class="nav-list">
            <li
              v-for="item in navItems"
              :key="item.id"
              class="cyber-tab"
              :class="{ 'active': activeTab === item.id }"
              @click="setActiveTab(item.id)"
            >
              <div class="tab-marker">>></div>
              <div class="tab-content">
                <span class="tab-en">{{ item.id.toUpperCase() }}</span>
                <span class="tab-cn">{{ item.name }}</span>
              </div>
              <div class="tab-status-light"></div>
            </li>
          </ul>

          <div class="sidebar-footer">
            <div class="barcode">||| || ||| |</div>
            <div>TAICHU_OS</div>
          </div>
        </nav>

        <main class="cyber-viewport">
          <div class="viewport-header">
            <span class="current-path">ROOT / ARCHIVES / {{ activeTab.toUpperCase() }}</span>
            <span class="deco-line">----------------------------------------</span>
          </div>

          <div class="viewport-content custom-scroll">
            <transition name="glitch-fade" mode="out-in">
              
              <section v-if="activeTab === 'intro'" key="intro" class="content-wrapper">
                <CommunityIntro />
              </section>

              <section v-else-if="activeTab === 'rules'" key="rules" class="content-wrapper">
                <CommunityRules />
              </section>

              <section v-else-if="activeTab === 'calendar'" key="calendar" class="content-wrapper">
                <CommunityCalendar />
              </section>

              <section v-else-if="activeTab === 'update'" key="update" class="content-wrapper">
                <CommnuityUpdate />
              </section>

              <section v-else-if="activeTab === 'about'" key="about" class="content-wrapper full-width-mode">
                 <div class="section-header-block">
                  <h3 class="giant-text">CORE_KERNEL</h3>
                  <div class="sub-text">// 开发组 // 核心架构 // 愿景协议</div>
                </div>
                <CommunityAbout />
              </section>

              <section v-else-if="activeTab === 'finance'" key="finance" class="content-wrapper">
                <CommunityFinancial />
              </section>

            </transition>
          </div>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
// 引入所有优化后的子组件
import CommunityIntro from '@/homeComponents/CommunityIntro.vue'
import CommunityRules from '@/homeComponents/CommunityRules.vue'
import CommunityCalendar from '@/homeComponents/CommunityCalendar.vue'
import CommunityFinancial from '@/homeComponents/CommunityFinancial.vue'
import CommnuityUpdate from '@/homeComponents/CommnuityUpdate.vue'
import CommunityAbout from '@/homeComponents/CommunityAbout.vue'

// 导航配置
const navItems = [
  { id: 'intro', name: '社区介绍' },
  { id: 'rules', name: '社区规则' },
  { id: 'calendar', name:'社区日历'},
  { id: 'update', name: '更新日志' },
  { id: 'about', name: '关于我们' },
  { id: 'finance', name: '财政记录' }
]

const activeTab = ref('intro')

const setActiveTab = (tabId) => {
  activeTab.value = tabId
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.cyber-archive-system {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  min-height: 100vh;
  background-color: var(--gray);
  color: var(--black);
  font-family: var(--mono);
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
  box-sizing: border-box;
}

/* 动态背景 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: 
    linear-gradient(rgba(17, 17, 17, 0.1) 1px, transparent 1px), 
    linear-gradient(90deg, rgba(17, 17, 17, 0.1) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; 
  pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* 主容器 */
.archive-container {
  width: 100%;
  max-width: 1600px;
  height: 90vh;
  background: var(--white);
  border: 4px solid var(--black);
  box-shadow: 15px 15px 0 rgba(0,0,0,0.2);
  z-index: 1;
  display: flex;
  flex-direction: column;
}

/* --- 系统 Header --- */
.sys-header {
  height: 60px;
  background: var(--black);
  color: var(--white);
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px;
  font-family: var(--heading);
  font-size: 1.5rem;
  letter-spacing: 1px;
}
.sys-title { display: flex; align-items: center; gap: 10px; }
.sys-title .icon { color: var(--red); }
.sys-meta { font-family: var(--mono); font-size: 0.9rem; color: #888; }
.blink { animation: blink 1s infinite; }

/* --- 核心布局 --- */
.archive-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

/* 侧边栏 */
.cyber-sidebar {
  width: 280px;
  background: #f0f0f0;
  border-right: 4px solid var(--black);
  display: flex;
  flex-direction: column;
  padding: 20px 0;
  user-select: none;
  flex-shrink: 0;
}

.sidebar-deco {
  padding: 0 20px 20px;
  font-size: 0.7rem;
  color: #888;
  border-bottom: 2px dashed #ccc;
  margin-bottom: 10px;
  line-height: 1.5;
}

.nav-list { list-style: none; padding: 0; margin: 0; }

.cyber-tab {
  padding: 15px 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.2s;
  border-bottom: 1px solid #ddd;
  position: relative;
  background: #f0f0f0;
}
.cyber-tab:hover { background: #fff; padding-left: 25px; }
.cyber-tab.active { background: var(--black); color: var(--white); border-bottom: 1px solid var(--black); }

.tab-marker { font-weight: bold; opacity: 0; color: var(--red); font-size: 0.8rem; }
.cyber-tab.active .tab-marker { opacity: 1; }

.tab-content { flex: 1; display: flex; flex-direction: column; }
.tab-en { font-size: 0.7rem; opacity: 0.6; font-weight: bold; }
.tab-cn { font-size: 1.1rem; font-weight: bold; }

.tab-status-light { width: 8px; height: 8px; background: #ccc; border: 1px solid #999; }
.cyber-tab.active .tab-status-light { background: var(--red); border-color: var(--red); box-shadow: 0 0 5px var(--red); }

.sidebar-footer { margin-top: auto; padding: 20px; text-align: center; opacity: 0.3; font-size: 0.8rem; }
.barcode { font-family: 'Libre Barcode 39', cursive; font-size: 2rem; transform: scaleY(0.5); }

/* --- 主视口 --- */
.cyber-viewport {
  flex: 1;
  background: #fff;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden; /* 防止双滚动条 */
}

.viewport-header {
  padding: 15px 30px;
  border-bottom: 2px solid var(--black);
  font-size: 0.8rem;
  color: #666;
  display: flex;
  justify-content: space-between;
  background: #fafafa;
  flex-shrink: 0;
}

.viewport-content {
  flex: 1;
  padding: 40px;
  overflow-y: auto;
  position: relative;
  /* 内部细微扫描线背景，增加质感 */
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.02) 50%);
  background-size: 100% 4px;
}

/* 内容容器调整 */
.content-wrapper {
  /* 不再限制最大宽度，让子组件自己决定布局，适应工业风的全屏感 */
  width: 100%;
  animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.section-header-block {
  margin-bottom: 40px;
  border-bottom: 4px solid var(--black);
  padding-bottom: 10px;
}
.giant-text {
  font-family: var(--heading); font-size: 3rem; margin: 0; line-height: 1; text-transform: uppercase;
}
.sub-text {
  font-family: var(--mono); color: var(--red); margin-top: 5px; font-weight: bold;
}

/* 动画 */
@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* 响应式适配 */
@media (max-width: 1024px) {
  .archive-container { height: auto; min-height: 90vh; }
  .archive-body { flex-direction: column; }
  .cyber-sidebar { width: 100%; height: auto; border-right: none; border-bottom: 4px solid var(--black); }
  .nav-list { display: flex; overflow-x: auto; }
  .cyber-tab { flex: 0 0 auto; padding: 10px; min-width: 140px; border-right: 1px solid #ddd; }
  .viewport-content { padding: 20px; }
}
</style>