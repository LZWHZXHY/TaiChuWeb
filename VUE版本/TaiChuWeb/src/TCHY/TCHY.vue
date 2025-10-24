<template>
  <div class="Big-ContainerS">
    <!-- 折叠按钮 -->
    <div class="collapse-button" @click="toggleSidebar">
      <i class="fas fa-bars"></i>
    </div>
    
    <!-- 侧边栏 -->
    <div 
      class="leftBar" 
      :class="{ 'expanded': isExpanded, 'collapsing': isCollapsing }"
      @mouseenter="pauseCollapse"
      @mouseleave="startCollapse"
    >
      <div class="sidebar-header">
        <div class="logo">TC</div>
        <div class="sidebar-title">太初寰宇</div>
      </div>
      
      <div class="menu-container">
        <ul class="menu">
          <router-link to="/taichu" class="menu-item">
            <li>
              <i class="fas fa-home"></i>
              <span class="menu-text">太初首页</span>
            </li>
          </router-link>
          <!-- 侧边栏 
          <router-link to="/taichu" class="menu-item">
            <li>
              <i class="fas fa-home"></i>
              <span class="menu-text">太初·璇玑玉衡·原型</span>
            </li>
          </router-link>
          -->

          
          <router-link to="/shop" class="menu-item">
            <li>
              <i class="fas fa-home"></i>
              <span class="menu-text">太初交易行</span>
            </li>
          </router-link>
          


          <router-link to="/posts" class="menu-item">
            <li>
              <i class="fas fa-comments"></i>
              <span class="menu-text">太虚坛</span>
            </li>
          </router-link>
          <router-link to="/chai" class="menu-item">
            <li>
              <i class="fas fa-users"></i>
              <span class="menu-text">柴圈板块</span>
            </li>
          </router-link>
          <router-link to="/game" class="menu-item">
            <li>
              <i class="fas fa-gamepad"></i>
              <span class="menu-text">游戏板块</span>
            </li>
          </router-link>
          <router-link to="/Resource" class="menu-item">
            <li>
              <i class="fas fa-gamepad"></i>
              <span class="menu-text">资源板块</span>
            </li>
          </router-link>
          <router-link to="/rank" class="menu-item">
            <li>
              <i class="fas fa-gamepad"></i>
              <span class="menu-text">排行榜</span>
            </li>
          </router-link>
          <router-link to="/feedback" class="menu-item">
            <li>
              <i class="fas fa-envelope"></i>
              <span class="menu-text">意见箱</span>
            </li>
          </router-link>

          <router-link to="/Review" class="menu-item">
            <li>
              <i class="fas fa-envelope"></i>
              <span class="menu-text">管理审核</span>
            </li>
          </router-link>
        </ul>
      </div>
      
      
    </div>
    
    <!-- 遮罩层 -->
    <div 
      class="sidebar-overlay" 
      :class="{ 'visible': isExpanded }"
      @click="closeSidebar"
    ></div>
    
    <!-- 主内容区 -->
    <main class="main-content">
      <div class="content-wrapper">
        <Transition name="glass-fade" mode="out-in">
          <router-view></router-view>
        </Transition>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../store/user'
import axios from 'axios'

// API 路径按需修改，开发环境和生产环境自动切换
const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com"

// 你的用户 pinia 仓库
const userStore = useUserStore()
// 侧边栏等UI逻辑

console.log("userStore:", userStore);


const isExpanded = ref(false)
const isCollapsing = ref(false)
const collapseTimer = ref(null)
const collapseDelay = ref(500)
const isMouseOver = ref(false)

const user = computed(() => userStore.user || {})
const userRank = computed(() => userStore.userRank || 0)

console.group("用户状态调试");
console.log("认证状态:", userStore.isAuthenticated);
console.log("用户对象:", userStore.user);
console.log("用户权限等级:", userStore.userRank);
console.log("用户BYD值:", userStore.user?.byd);
console.log("令牌:", userStore.token ? "存在" : "不存在");
console.groupEnd();


let heartbeatTimer = null





// 全局心跳定时器（1分钟一次）
onMounted(() => {
  // 立即发送一次
  if (userStore.token) {
    axios.post(`${baseApiUrl}/api/UserInfo/heartbeat`, null, {
      headers: { Authorization: `Bearer ${userStore.token}` }
    })
  }
  // 然后每分钟发一次
  heartbeatTimer = setInterval(() => {
    if (userStore.token) {
      axios.post(`${baseApiUrl}/api/UserInfo/heartbeat`, null, {
        headers: { Authorization: `Bearer ${userStore.token}` }
      })
    }
  }, 60000)

  


})

onUnmounted(() => {
  if (heartbeatTimer) clearInterval(heartbeatTimer)
})

// 侧边栏控制函数（如需）
function toggleSidebar() {
  if (!isExpanded.value) {
    openSidebar();
  } else {
    closeSidebar();
  }
}

function openSidebar() {
  isCollapsing.value = false;
  isExpanded.value = true;
  clearTimeout(collapseTimer.value);
}

function closeSidebar() {
  isCollapsing.value = true;
  setTimeout(() => {
    isExpanded.value = false;
    isCollapsing.value = false;
  }, 400);
  clearTimeout(collapseTimer.value);
}

function pauseCollapse() {
  isMouseOver.value = true;
  clearTimeout(collapseTimer.value);
}

function startCollapse() {
  isMouseOver.value = false;
  if (isExpanded.value) {
    collapseTimer.value = setTimeout(() => {
      closeSidebar();
    }, collapseDelay.value);
  }
}

// 用户登出逻辑
function logout() {
  userStore.logout();
  window.location.href = '/login.html';
}
</script>

<style scoped>
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');
@import url('https://fonts.googleapis.com/css2?family=Orbitron:wght@400;500;600;700&family=Exo+2:wght@300;400;500;600&display=swap');

:root {
  --sidebar-width: 220px;
  --transition-duration: 0.4s;
  --ease-out-quint: cubic-bezier(0.23, 1, 0.32, 1);
  --ease-in-out-quint: cubic-bezier(0.86, 0, 0.07, 1);
}

.Big-ContainerS {
  background-image: url('/static/image/伊渡Yee素材1.webp');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  position: absolute;
  width: 100%;
  height: 100%;
  overflow: hidden;
  font-family: 'Exo 2', 'Microsoft YaHei', sans-serif;
}

/* 折叠按钮 */
.collapse-button {
  position: absolute;
  top: 15px;
  left: 15px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(40, 40, 60, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 20;
  color: #28d7ff;
  border: 1px solid rgba(40, 215, 255, 0.3);
  box-shadow: 
    0 0 10px rgba(0, 0, 0, 0.5),
    0 0 15px rgba(40, 215, 255, 0.3);
  transition: 
    all 0.3s ease,
    transform 0.3s var(--ease-out-quint);
}

.collapse-button:hover {
  background: rgba(40, 215, 255, 0.2);
  transform: scale(1.1) rotate(90deg);
  box-shadow: 
    0 0 15px rgba(0, 0, 0, 0.6),
    0 0 20px rgba(40, 215, 255, 0.4);
}

.collapse-button i {
  font-size: 1.2rem;
  transition: transform 0.3s ease;
}

/* 可折叠侧边栏 */
.leftBar {
  background: linear-gradient(to bottom, rgba(18, 20, 33, 0.95), rgba(8, 10, 28, 0.98));
  width: 15%;
  height: 100vh;
  position: fixed;
  top: 0;
  left: 0;
  z-index: 25;
  overflow: hidden;
  transform: translateX(-100%);
  transition: 
    transform var(--transition-duration) var(--ease-out-quint),
    box-shadow var(--transition-duration) ease;
  border-right: 1px solid rgba(40, 215, 255, 0.3);
  box-shadow: 0 0 0 rgba(0, 0, 0, 0);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  flex-direction: column;
}

.leftBar.expanded {
  transform: translateX(0);
  box-shadow: 
    0 0 30px rgba(0, 0, 0, 0.7),
    15px 0 30px -15px rgba(40, 215, 255, 0.3);
}

.leftBar.collapsing {
  transform: translateX(-100%);
  transition: 
    transform var(--transition-duration) var(--ease-in-out-quint),
    box-shadow var(--transition-duration) ease;
}

.sidebar-header {
  width: 100%;
  padding: 20px 15px;
  text-align: center;
  position: relative;
  border-bottom: 1px solid rgba(40, 215, 255, 0.2);
  opacity: 0;
  transform: translateX(-20px);
  transition: 
    opacity 0.3s ease,
    transform 0.4s ease;
}

.expanded .sidebar-header {
  opacity: 1;
  transform: translateX(0);
  transition: 
    opacity 0.3s ease 0.1s,
    transform 0.4s ease 0.1s;
}

.collapsing .sidebar-header {
  opacity: 0;
  transform: translateX(-20px);
  transition: 
    opacity 0.2s ease,
    transform 0.3s ease;
}

.logo {
  font-size: 28px;
  font-weight: 700;
  color: #28d7ff;
  font-family: 'Orbitron', sans-serif;
  text-shadow: 0 0 10px rgba(40, 215, 255, 0.7);
  margin-bottom: 10px;
  transition: all 0.3s ease;
}

.sidebar-title {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1rem;
  font-weight: 500;
  letter-spacing: 2px;
  opacity: 0;
  transform: translateY(10px);
  transition: 
    opacity 0.3s ease,
    transform 0.4s ease;
}

.expanded .sidebar-title {
  opacity: 1;
  transform: translateY(0);
  transition: 
    opacity 0.3s ease 0.2s,
    transform 0.4s ease 0.2s;
}

.collapsing .sidebar-title {
  opacity: 0;
  transform: translateY(10px);
  transition: 
    opacity 0.2s ease,
    transform 0.3s ease;
}

.menu-container {
  flex-grow: 1;
  width: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 15px 0;
  opacity: 0;
  transform: translateX(-20px);
  transition: 
    opacity 0.3s ease,
    transform 0.4s ease;
}

.expanded .menu-container {
  opacity: 1;
  transform: translateX(0);
  transition: 
    opacity 0.3s ease 0.1s,
    transform 0.4s ease 0.1s;
}

.collapsing .menu-container {
  opacity: 0;
  transform: translateX(-20px);
  transition: 
    opacity 0.2s ease,
    transform 0.3s ease;
}

.menu {
  list-style: none;
  padding: 0;
  margin: 0;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.menu-item {
  color: #eee;
  font-size: 0.9rem;
  font-family: 'Exo 2', sans-serif;
  letter-spacing: 0.05em;
  background: rgba(36, 35, 36, 0.6);
  width: calc(100% - 20px);
  margin: 0 auto;
  padding: 12px 15px;
  border-radius: 8px;
  transition: 
    all 0.3s var(--ease-out-quint),
    background 0.3s ease;
  box-shadow: 
    0 2px 10px rgba(0, 0, 0, 0.3),
    inset 0 0 0 1px rgba(40, 215, 255, 0.1);
  cursor: pointer;
  display: flex;
  align-items: center;
  opacity: 0;
  transform: translateX(-20px);
}

.expanded .menu-item {
  opacity: 1;
  transform: translateX(0);
  transition: 
    opacity 0.3s var(--ease-out-quint),
    transform 0.4s var(--ease-out-quint);
}

.collapsing .menu-item {
  opacity: 0;
  transform: translateX(-20px);
  transition: 
    opacity 0.2s var(--ease-in-out-quint),
    transform 0.3s var(--ease-in-out-quint);
}

.menu-item:nth-child(1) { transition-delay: 0.1s; }
.menu-item:nth-child(2) { transition-delay: 0.15s; }
.menu-item:nth-child(3) { transition-delay: 0.2s; }
.menu-item:nth-child(4) { transition-delay: 0.25s; }
.menu-item:nth-child(5) { transition-delay: 0.3s; }
.menu-item:nth-child(6) { transition-delay: 0.35s; }

.menu-item:hover {
  background: rgba(36, 35, 36, 0.8);
  box-shadow: 
    0 5px 15px rgba(0, 0, 0, 0.4),
    inset 0 0 0 1px rgba(40, 215, 255, 0.3);
  transform: translateX(5px) !important;
}

.menu-item i {
  font-size: 1.3rem;
  min-width: 30px;
  text-align: center;
  color: #28d7ff;
  text-shadow: 0 0 8px rgba(40, 215, 255, 0.5);
  margin-right: 10px;
  transition: all 0.3s ease;
}

.menu-item:hover i {
  transform: scale(1.1);
  text-shadow: 0 0 12px rgba(40, 215, 255, 0.8);
}

.menu-text {
  white-space: nowrap;
}

.sidebar-footer {
  color: rgba(200, 220, 255, 0.7);
  font-size: 0.75rem;
  padding: 10px;
  text-align: center;
  border-top: 1px solid rgba(40, 215, 255, 0.1);
  background: rgba(0, 5, 15, 0.4);
  opacity: 0;
  transform: translateY(10px);
  transition: 
    opacity 0.3s ease,
    transform 0.4s ease;
}

.expanded .sidebar-footer {
  opacity: 1;
  transform: translateY(0);
  transition: 
    opacity 0.3s ease 0.3s,
    transform 0.4s ease 0.3s;
}

.collapsing .sidebar-footer {
  opacity: 0;
  transform: translateY(10px);
  transition: 
    opacity 0.2s ease,
    transform 0.3s ease;
}

.current-user {
  margin-top: 3px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.9);
}

/* 遮罩层 */
.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(2px);
  z-index: 22;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.4s ease;
}

.sidebar-overlay.visible {
  opacity: 1;
  pointer-events: all;
}

/* 主内容区 */
.main-content {
  background-color: rgba(28, 53, 35, 0);
  width: 100%;
  height: 100%;
  z-index: 10;
  position: relative;
  transition: filter 0.4s ease;
}

.expanded ~ .main-content {
  filter: blur(2px);
}

.content-wrapper {
  width: 100%;
  height: 100%;
  position: absolute;
  
}

/* 过渡效果 */
.glass-fade-enter-active {
  transition: all 0.5s var(--ease-out-quint);
}

.glass-fade-leave-active {
  transition: all 0.3s var(--ease-in-out-quint);
}

.glass-fade-enter-from,
.glass-fade-leave-to {
  opacity: 0;
  transform: translateY(20px);
  backdrop-filter: blur(0px);
  -webkit-backdrop-filter: blur(0px);
}

/* 路由视图样式 */
:deep(.router-container) {
  background: rgba(15, 20, 35, 0.45);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-radius: 16px;
  padding: 25px;
  border: 1px solid rgba(40, 215, 255, 0.15);
  box-shadow: 
    0 12px 32px rgba(0, 0, 0, 0.3),
    inset 0 0 20px rgba(40, 215, 255, 0.1);
  
  min-height: 50px;
  height: auto;
  max-height: 80vh;
  overflow-y: auto;
  color: #eef;
  font-family: 'Exo 2', sans-serif;
}

:deep(.router-container:empty) {
  min-height: 0;
  padding: 0;
  opacity: 0;
  transform: translateY(10px);
  background: rgba(235, 245, 255, 0);
  backdrop-filter: blur(0px);
  -webkit-backdrop-filter: blur(0px);
  box-shadow: none;
  border: 1px solid rgba(255, 255, 255, 0);
}

a {
  text-decoration: none;
  color: inherit;
}

/* 响应式设计 */

</style>