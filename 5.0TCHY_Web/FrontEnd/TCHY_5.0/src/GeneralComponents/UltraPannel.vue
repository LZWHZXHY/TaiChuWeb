<template>
  <div class="ultra-wrapper" :class="{ 'open': isOpen }">
    <div class="ultra-panel">
      
      <div class="panel-inner">
        <header class="panel-header">
          <span class="header-title">系统菜单</span>
        </header>
        
        <main class="panel-body">
          <nav class="nav-grid">
            
            <template v-if="isAdmin">
              <span class="group-label">系统</span>
              <router-link to="/admin" class="nav-item admin-link" @click="isOpen = false">管理后台</router-link>
              <div class="admin-divider"></div>
            </template>

            <span class="group-label" :class="{ 'group-margin': !isAdmin }">核心</span>
            <router-link to="/profile/MEE" class="nav-item" @click="isOpen = false">我的主页</router-link>
            <router-link to="/creative-center" class="nav-item" @click="isOpen = false">创作中心</router-link>
            <router-link to="/lingmai" class="nav-item" @click="isOpen = false">灵脉空间</router-link>
            <router-link to="/workspace" class="nav-item" @click="isOpen = false">多人协作</router-link>
            
            <span class="group-label group-margin">社区</span>
            <router-link to="/" class="nav-item" @click="isOpen = false">首页</router-link>
            <router-link to="/Intro" class="nav-item" @click="isOpen = false">社区介绍</router-link>
            <router-link to="/WorkCenter" class="nav-item" @click="isOpen = false">作品中心</router-link>
            <router-link to="/BlogCenter" class="nav-item" @click="isOpen = false">博客中心</router-link>
            <router-link to="/PostCenter" class="nav-item" @click="isOpen = false">动态中心</router-link>
            <router-link to="/ChatCenter" class="nav-item" @click="isOpen = false">聊天中心</router-link>
            <router-link to="/EntertainmentArea" class="nav-item" @click="isOpen = false">娱乐区</router-link>
            <router-link to="/media" class="nav-item" @click="isOpen = false">媒体中心</router-link>
            <router-link to="/recommend" class="nav-item" @click="isOpen = false">推荐</router-link>
            <router-link to="/suggest" class="nav-item" @click="isOpen = false">反馈区</router-link>
            <router-link to="/ClubCenter" class="nav-item" @click="isOpen = false">社团收录</router-link>

            <span class="group-label group-margin">资源</span>
            <router-link to="/wiki" class="nav-item" @click="isOpen = false">知识库</router-link>
            <router-link to="/Market" class="nav-item" @click="isOpen = false">交易行</router-link>
            
          </nav>
        </main>

        <footer class="panel-footer">
          V1.0.44
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
import { ref, computed } from 'vue'
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()
const isOpen = ref(false)

const togglePanel = () => {
  isOpen.value = !isOpen.value
}

// 🚀 核心修复：使用数字 rank 进行硬判断
// 在 UltraPannel.vue 的 <script setup> 中修改
const isAdmin = computed(() => {
  const u = authStore.user;
  if (!u || !Array.isArray(u.roleCodes)) return false;

  // 🚀 核心修改：检查 roleCodes 数组中是否包含指定的管理员标识
  return u.roleCodes.includes('Admin') || u.roleCodes.includes('SuperAdmin');
})
</script>

<style scoped>
/* 保持之前定义的样式 */
.ultra-wrapper {
  position: fixed;
  left: 0;
  top: 120px;
  z-index: 2000;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
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
  transform: translateX(calc(-100% - 2px)); 
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  pointer-events: auto; 
}

.open .ultra-panel {
  transform: translateX(0);
}

.admin-link {
  color: #D92323 !important; 
  font-weight: 900 !important;
}

.admin-divider {
  grid-column: span 2;
  height: 1px;
  background: #eee;
  margin: 4px 0;
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
  pointer-events: auto; 
}

.panel-inner {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 15px;
  overflow-y: auto;
  scrollbar-width: none;
}

.panel-inner::-webkit-scrollbar {
  display: none;
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