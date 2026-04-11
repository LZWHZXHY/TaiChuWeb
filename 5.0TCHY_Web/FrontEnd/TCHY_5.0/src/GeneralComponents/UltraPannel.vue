<template>
  <div class="ultra-wrapper" :class="{ 'open': isOpen }">
    <div class="ultra-panel">
      
      <div class="panel-inner">
        <header class="panel-header">
          <span class="header-title">{{ $t('UltraPannel.title') || '系统菜单' }}</span>
        </header>
        
        <main class="panel-body">
          <nav class="nav-grid">
            
            <span class="group-label">核心 / CORE</span>
            <router-link to="/profile/MEE" class="nav-item" @click="isOpen = false">
              {{ $t('UltraPannel.profile') }}
            </router-link>
            <router-link to="/creative-center" class="nav-item" @click="isOpen = false">
              {{ $t('UltraPannel.creativeCenter') }}
            </router-link>
            

            <span class="group-label group-margin">社区 / COMMUNITY</span>
            <router-link to="/" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.home') }}</router-link>
            <router-link to="/Intro" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.intro') }}</router-link>
            <router-link to="/WorkCenter" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.workCenter') }}</router-link>
            <router-link to="/BlogCenter" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.blogCenter') }}</router-link>
            <router-link to="/PostCenter" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.postCenter') }}</router-link>
            <router-link to="/ChatCenter" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.chatCenter') }}</router-link>
            <router-link to="/media" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.mediaCenter') }}</router-link>
            <router-link to="/recommend" class="nav-item" @click="isOpen = false">推荐</router-link>
            
            <router-link to="/suggest" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.feedback') }}</router-link>

            <span class="group-label group-margin">资源 / RESOURCE</span>
            <router-link to="/wiki" class="nav-item" @click="isOpen = false">{{ $t('UltraPannel.wiki') }}</router-link>
            
          </nav>
        </main>

        <footer class="panel-footer">
          V1.0.42
        </footer>
      </div>

      <button class="trigger-handle" @click="togglePanel">
        <span class="handle-text">
          {{ isOpen ? '《' : '》' }}
        </span>
      </button>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
// import { useI18n } from 'vue-i18n' // 如果没用到 useI18n 的变量可以注释掉

const isOpen = ref(false)
const togglePanel = () => {
  isOpen.value = !isOpen.value
}
</script>

<style scoped>
/* --- 核心修复部分 --- */
.ultra-wrapper {
  position: fixed;
  left: 0;
  top: 120px;
  z-index: 2000;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  
  /* 1. 关键修复：让整个包裹层不响应点击（穿透） */
  pointer-events: none; 
}

.ultra-panel {
  display: flex;
  width: 220px; 
  min-height: 240px; 
  max-height: 80vh; 
  background: #ffffff;
  border: 1px solid #000;
  border-left: none;
  /* 2. 稍微加大位移，确保连边框都彻底藏好 */
  transform: translateX(calc(-100% - 2px)); 
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;

  /* 3. 关键修复：面板显示时，内部需要恢复点击响应 */
  pointer-events: auto; 
}

.open .ultra-panel {
  transform: translateX(0);
}

.trigger-handle {
  position: absolute;
  right: -24px;
  top: 0;
  width: 24px;
  height: 60px;
  background: #0a6e28;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;

  /* 4. 关键修复：按钮必须恢复点击，否则点不到它 */
  pointer-events: auto; 
}

/* --- 其余样式保持不变 --- */
.panel-inner {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 15px;
  overflow-y: auto;
  scrollbar-width: none; /* 火狐隐藏滚动条 */
}

.panel-inner::-webkit-scrollbar {
  display: none; /* Webkit 隐藏滚动条 */
}

.panel-header {
  margin-bottom: 12px;
  border-bottom: 1px solid #000;
  padding-bottom: 5px;
}

.header-title {
  font-size: 13px;
  font-weight: 900;
  letter-spacing: 2px;
}

.nav-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 8px;
}

.group-label {
  grid-column: span 2;
  font-size: 9px;
  color: #ccc;
  letter-spacing: 1px;
  margin-bottom: 2px;
  border-bottom: 1px solid #f5f5f5;
  padding-bottom: 2px;
  pointer-events: none;
}

.group-margin {
  margin-top: 10px;
}

.nav-item {
  text-decoration: none;
  color: #000;
  font-size: 13px;
  font-weight: 500;
  padding: 2px 0;
}

.nav-item:hover {
  text-decoration: underline;
  opacity: 0.7;
}

.router-link-active {
  font-weight: 900;
  text-decoration: underline;
}

.panel-footer {
  margin-top: 15px;
  font-size: 9px;
  color: #eee;
}

.handle-text {
  writing-mode: vertical-lr;
  font-size: 10px;
  letter-spacing: 2px;
}
</style>