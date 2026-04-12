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
          v-for="item in menuItems" 
          :key="item.id"
          class="nav-item"
          :class="{ active: activeViewId === item.id, 'publish-btn': item.id === 'publish' }"
          @click="switchView(item.id)"
        >
          <span class="icon">{{ item.icon }}</span> {{ item.label }}
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
        <component 
          :is="currentViewComponent" 
          :key="activeViewId" 
        />
      </transition>

      <div v-if="!currentViewComponent" class="wip-placeholder">
        <p>[ SYSTEM_RESTRICTED: 模块丢失或开发中 ]</p>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, shallowRef, markRaw } from 'vue'

// 1. 导入所有子组件
import MyJoints from '@/CreateCenterComponents/MyActivity.vue'
import SubmissionsManager from '@/CreateCenterComponents/SubmissionsManager.vue'
import PublishCenter from '@/CreateCenterComponents/PublishCenter.vue'
import MyCollections from '@/CreateCenterComponents/MyCollections.vue'
import VideoUploader from '@/CreateCenterComponents/VideoUploader.vue'
// ⚠️ 重点检查此路径！如果文件不在 components 子文件夹下，请修改为正确路径
import RadioUploader from '@/CreateCenterComponents/components/RadioUploader.vue'

/**
 * 2. 建立组件映射表 (使用 markRaw 避免 Vue 对组件定义进行深度代理，提升性能)
 */
const viewMap = {
  'publish': markRaw(PublishCenter),
  'my-joints': markRaw(MyJoints),
  'submissions': markRaw(SubmissionsManager),
  'my-collections': markRaw(MyCollections),
  'video-upload': markRaw(VideoUploader),
  'radio-upload': markRaw(RadioUploader)
}

// 3. 菜单配置
const menuItems = [
  { id: 'publish', label: '创造矩阵 // PUBLISH', icon: '✚' },
  { id: 'my-joints', label: '我的联合 // MY_OPS', icon: '▤' },
  { id: 'submissions', label: '稿件管理 // ARCHIVE', icon: '◈' },
  { id: 'my-collections', label: '星标档案 // COLLECTIONS', icon: '★' },
  { id: 'radio-upload', label: '深空电台 // RADIO', icon: '📡' },
  { id: 'video-upload', label: '影像归档 // VIDEO', icon: '🎬' }
]

// 4. 响应式状态
const activeViewId = ref('publish')

// 5. 计算属性：返回当前需要渲染的组件对象
const currentViewComponent = computed(() => {
  return viewMap[activeViewId.value] || null
})

// 6. 切换方法
const switchView = (id) => {
  activeViewId.value = id
}
</script>

<style scoped>
/* 保持你原来的样式不变 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.creative-terminal {
  display: flex; height: 100vh; background: #f4f4f4; color: #111; font-family: 'JetBrains Mono', monospace; overflow: hidden;
}
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
.publish-btn { border-left: 4px solid transparent; }
.publish-btn:hover { border-left-color: #D92323; background: #1a1a1a; }
.publish-btn.active { border-left-color: #fff; }
.tower-footer { padding: 20px; border-top: 1px solid #333; }
.exit-btn { background: transparent; border: 1px solid #444; color: #666; padding: 5px 15px; cursor: pointer; width: 100%; text-align: left; transition: 0.2s; }
.exit-btn:hover { border-color: #D92323; color: #D92323; }
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