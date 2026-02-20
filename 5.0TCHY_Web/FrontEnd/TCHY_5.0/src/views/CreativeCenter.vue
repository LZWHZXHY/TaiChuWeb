<template>
  <div class="creative-terminal">
    <div class="bg-grid"></div>
    
    <aside class="nav-tower">
      <div class="tower-header">
        <span class="logo-box">CREATOR</span>
        <span class="sub">创造中心 // V.1.0</span>
      </div>

      <nav class="nav-menu">
        <div 
          class="nav-item publish-btn"
          :class="{ active: currentView === 'publish' }"
          @click="currentView = 'publish'"
        >
          <span class="icon">✚</span> 创造矩阵 // PUBLISH
        </div>

        <div 
          class="nav-item" 
          :class="{ active: currentView === 'my-joints' }"
          @click="currentView = 'my-joints'"
        >
          <span class="icon">▤</span> 我的联合 // MY_OPS
        </div>

        <div 
          class="nav-item"
          :class="{ active: currentView === 'submissions' }"
          @click="currentView = 'submissions'"
        >
          <span class="icon">◈</span> 稿件管理 // ARCHIVE
        </div>

        <div class="nav-item disabled">
          <span class="icon">📊</span> 数据分析 (W.I.P)
        </div>
      </nav>
      
      <div class="tower-footer">
        <button class="exit-btn" @click="$router.push('/')"><< EXIT_SYSTEM</button>
      </div>
    </aside>

    <main class="main-deck custom-scroll">
      <transition name="view-fade" mode="out-in">
        <PublishCenter v-if="currentView === 'publish'" />
        <MyJoints v-else-if="currentView === 'my-joints'" />
        <SubmissionsManager v-else-if="currentView === 'submissions'" />
        
        <div v-else class="wip-placeholder">
          <p>[ SYSTEM_RESTRICTED: 模块开发中 ]</p>
        </div>
      </transition>
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import MyJoints from '@/CreateCenterComponents/MyJoints.vue'
import SubmissionsManager from '@/CreateCenterComponents/SubmissionsManager.vue'
import PublishCenter from '@/CreateCenterComponents/PublishCenter.vue' // ✅ 引入发布中心

// 可以默认打开发布中心，或者稿件管理
const currentView = ref('publish')
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* 基础框架布局 */
.creative-terminal {
  display: flex; height: 100vh; background: #f4f4f4; color: #111; font-family: 'JetBrains Mono', monospace; overflow: hidden;
}

/* 侧边导航样式 */
.nav-tower {
  width: 260px; background: #111; color: #fff; display: flex; flex-direction: column; padding: 30px 0; border-right: 4px solid #D92323; z-index: 2; flex-shrink: 0;
}
.tower-header { padding: 0 25px 30px; border-bottom: 1px solid #333; }
.logo-box { font-family: 'Anton'; font-size: 2rem; display: block; letter-spacing: 2px; }
.sub { color: #666; font-size: 0.7rem; font-weight: bold; }

.nav-menu { flex: 1; padding-top: 20px; display: flex; flex-direction: column; gap: 5px; }
.nav-item { padding: 15px 25px; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 10px; font-weight: bold; }
.nav-item:hover { background: #222; color: #D92323; }
.nav-item.active { background: #D92323; color: #fff; }
.nav-item.disabled { opacity: 0.3; cursor: not-allowed; }

/* ✅ 给发布按钮一个特殊样式，凸显其重要性 */
.publish-btn { border-left: 4px solid transparent; }
.publish-btn:hover { border-left-color: #D92323; background: #1a1a1a; }
.publish-btn.active { border-left-color: #fff; }

.tower-footer { padding: 20px; border-top: 1px solid #333; }
.exit-btn { background: transparent; border: 1px solid #444; color: #666; padding: 5px 15px; cursor: pointer; width: 100%; text-align: left; transition: 0.2s; }
.exit-btn:hover { border-color: #D92323; color: #D92323; }

/* 主展示区 */
.main-deck { flex: 1; padding: 40px; position: relative; overflow-y: auto; z-index: 1; }
.bg-grid { position: absolute; inset: 0; background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px); background-size: 40px 40px; z-index: 0; pointer-events: none; }
.wip-placeholder { display: flex; height: 60vh; align-items: center; justify-content: center; color: #999; font-weight: bold; border: 2px dashed #ccc; }

.view-fade-enter-active, .view-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.view-fade-enter-from { opacity: 0; transform: translateY(10px); }
.view-fade-leave-to { opacity: 0; transform: translateY(-10px); }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>