<template>
  <header class="cyber-header" :class="{ 'header-compact': isScrolled }">
    <div class="header-decoration-line"></div>

    <div class="nav-container">
      
      <div class="logo-block" @click="navigateToHome">
        <div class="logo-icon-box">
          <img src="/favicon.ico" class="logo-img" alt="LOGO">
        </div>
        <div class="logo-text-group">
          <span class="logo-main">太初寰宇</span>
          <span class="logo-sub">社区 // {{ $t('HeaderNav.site_name') }}</span>
        </div>
      </div>

      <nav class="cyber-nav">
        <template v-for="item in navItems" :key="item.path || item.name">
          <div
            v-if="item.type === 'link'"
            class="nav-link-item"
            :class="{ 'active': isActive(item.path) }"
            @click="handleNavClick(item)"
          >
            <span class="bracket">[</span>
            <span class="link-text">{{ $t(item.name) }}</span>
            <span class="bracket">]</span>
          </div>
          
          <DropdownMenu
            v-else-if="item.type === 'dropdown'"
            :items="item.children"
            :trigger-text="$t(item.name)" 
            @item-click="handleNavClick"
            class="nav-dropdown-cyber"
          />
        </template>
      </nav>

      <div class="cyber-actions">
        
        <button class="cyber-icon-btn" @click="toggleLang" :title="locale === 'zh' ? 'EN' : 'CN'">
          <span class="btn-inner">{{ locale === 'zh' ? 'CN' : 'EN' }}</span>
        </button>

        <div class="status-terminal" v-if="userCount > 0">
          <span class="status-indicator">●</span>
          <span class="status-text">ONLINE: {{ userCount }}</span>
        </div>

        <div v-if="authStore.isAuthenticated" class="user-control-panel">
          
          <div class="user-trigger" @click.stop="toggleUserMenu" :class="{ 'menu-open': showUserMenu }">
            <div class="avatar-frame">
              <img 
                v-if="realAvatarUrl"
                :src="realAvatarUrl" 
                class="avatar-img" 
                alt="USER"
                @error="handleImageError"
              />
              <div v-else class="avatar-fallback">{{ userNameText.charAt(0) }}</div>
            </div>
            <div class="user-id-block">
              <span class="id-label">OPERATOR</span>
              <span class="id-name">{{ userNameText }}</span>
            </div>
            <div v-if="localUnreadCount > 0" class="alert-tag">!</div>
          </div>

          <transition name="scale-fade">
            <div v-if="showUserMenu" class="cyber-dropdown-menu">
              <div class="menu-header">// USER_ACTIONS</div>
              <div class="menu-list">
                <div class="menu-row" @click="goToNotifications">
                  <span class="row-label">NOTIFICATIONS</span>
                  <span v-if="localUnreadCount > 0" class="row-val alert">{{ localUnreadCount }}</span>
                </div>
                <div class="menu-row" @click="goToProfile">
                  <span class="row-label">PROFILE</span>
                  <span class="row-icon">-></span>
                </div>
                <div class="menu-row" @click="goToNewProfile">
                  <span class="row-label">NEW PROFILE</span>
                  <span class="row-icon">-></span>
                </div>
                <div class="menu-row" @click="goToNewSettings">
                  <span class="row-label">PROFILE SETTINGS</span>
                  <span class="row-icon">-></span>
                </div>
                <div class="menu-divider">----------------</div>
                <div class="menu-row logout" @click="handleLogout">
                  <span class="row-label">>> LOGOUT</span>
                </div>
              </div>
            </div>
          </transition>

          <div class="notification-wrapper">
            <button 
              class="cyber-icon-btn notify-trigger" 
              :class="{ 'active': showNotifications }"
              @click.stop="toggleNotifications"
              title="NOTIFICATIONS"
            >
              <span class="btn-inner">信</span>
              <div v-if="localUnreadCount > 0" class="mini-badge"></div>
            </button>

            <transition name="pop-down">
              <div v-if="showNotifications" class="notification-popover" @click.stop>
                <NotificationPanel @update-count="fetchUnreadCount" />
              </div>
            </transition>
          </div>

          <div class="tactical-btn-wrapper" @click="router.push('/suggest')">
            <button class="tactical-btn red">
              <span class="btn-icon">✎</span>
              <span class="btn-label">反馈区</span>
            </button>
            <div class="btn-deco-line"></div>
          </div>

        </div>

        <div v-else class="auth-group">
          <button @click="handleLogin" class="cyber-btn-outline">LOGIN</button>
          <button @click="handleRegister" class="cyber-btn-solid">REGISTER</button>
        </div>

      </div>
    </div>
    
    <div class="header-bottom-border"></div>
  </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n' 
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'
import DropdownMenu from './DropdownMenu.vue' 
import NotificationPanel from './Widget/NotificationPanel.vue' // 引入通知组件

// --- 初始化 ---
const authStore = useAuthStore()
const router = useRouter()
const { t, locale } = useI18n() 

const BASE_URL = 'https://bianyuzhou.com' // 你的图片服务器地址

// --- 响应式状态 ---
const userCount = ref(0)
const localUnreadCount = ref(0) // 未读消息数
const showUserMenu = ref(false)
const showNotifications = ref(false) // 通知面板显示状态
const isScrolled = ref(false)
const avatarLoadError = ref(false)
let unreadTimer = null // 轮询定时器

// --- Props ---
const props = defineProps({
  navItems: { type: Array, required: true }
})
const emit = defineEmits(['nav-change', 'user-action'])

// --- 核心业务逻辑 ---

// 1. 获取未读数 (调用后端接口)
const fetchUnreadCount = async () => {
  if (!authStore.isAuthenticated) return
  try {
    const res = await apiClient.get('/Notification/unread-count')
    if (res.data.success) {
      localUnreadCount.value = res.data.count
    }
  } catch (err) {
    // 静默失败，不打扰用户
    console.warn('Sync Signal Error:', err)
  }
}

// 2. 切换通知面板
const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
  
  if (showNotifications.value) {
    // 打开通知时，关闭用户菜单
    showUserMenu.value = false
    // 顺便刷新一下未读数
    fetchUnreadCount()
  } else {
    // 关闭时也刷新一下，确保红点同步
    fetchUnreadCount()
  }
}

// 3. 切换用户菜单
const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
  if (showUserMenu.value) {
    // 打开用户菜单时，关闭通知面板
    showNotifications.value = false
  }
}

// 4. 点击外部关闭面板 (Global Click Handler)
const closeAllMenus = (event) => {
  // 如果点击的目标不在 .user-control-panel 内部，则关闭所有菜单
  const target = event.target
  
  // 检查是否点击了用户菜单触发器或内部
  if (!target.closest('.user-control-panel')) {
    showUserMenu.value = false
    showNotifications.value = false
    return
  }

  // 特殊处理：如果已经打开了通知面板，但点击了用户头像，逻辑在 toggleUserMenu 处理
  // 这里主要处理点击空白处关闭
  // 由于使用了 @click.stop 在按钮上，这里主要兜底
}

// --- 辅助逻辑 ---

const toggleLang = () => {
  const newLang = locale.value === 'zh' ? 'en' : 'zh'
  locale.value = newLang
  localStorage.setItem('app_language', newLang) 
}

const fetchLatestUserInfo = async () => {
  if (!authStore.isAuthenticated) return
  try {
    const res = await apiClient.get('/default/user/me')
    if (res.data && res.data.success) {
      const userData = res.data.data
      authStore.user = { ...authStore.user, ...userData }
    }
  } catch (error) { console.warn('User Info Sync Failed', error) }
}

const realAvatarUrl = computed(() => {
  let path = authStore.user?.avatar || authStore.user?.logo
  if (!path || avatarLoadError.value) return null
  if (path.startsWith('http')) return path
  path = path.replace(/\\/g, '/')
  if (path.startsWith('/')) path = path.substring(1)
  if (!path.startsWith('uploads/')) path = `uploads/${path}`
  return `${BASE_URL}/${path}`
})

const userNameText = computed(() => {
  return authStore.user?.name || authStore.user?.username || 'GUEST'
})

const handleImageError = () => { avatarLoadError.value = true }
watch(() => authStore.user, () => { avatarLoadError.value = false })

const isActive = (path) => {
  const currentPath = router.currentRoute.value.path
  if (path === '/') return currentPath === '/'
  return currentPath === path || currentPath.startsWith(path + '/')
}

const loadUserCount = async () => {
  try {
    const response = await apiClient.get('/default/users/count')
    if (typeof response.data.count === 'number') userCount.value = response.data.count
  } catch (err) { userCount.value = 0 }
}

const handleNavClick = (item) => {
  emit('nav-change', item)
  if (item.path) router.push(item.path)
}

const navigateToHome = () => router.push('/')
const handleLogin = () => router.push('/login')
const handleRegister = () => router.push('/register')

const goToProfile = () => { showUserMenu.value = false; router.push('/profile/me') }
const goToNewProfile = () => { showUserMenu.value = false; router.push('/profile/MEE') }
const goToNewSettings = () => { showUserMenu.value = false; router.push('/profile/NewSettings') }
const goToNotifications = () => { showUserMenu.value = false; router.push('/profile/me?tab=notification') }

const handleLogout = async () => {
  showUserMenu.value = false
  authStore.logout()
  localStorage.removeItem('auth_token')
  localStorage.removeItem('user')
  localUnreadCount.value = 0
  router.push('/')
}

const handleScroll = () => isScrolled.value = window.scrollY > 10

// --- 生命周期 ---
onMounted(() => {
  loadUserCount()
  fetchLatestUserInfo()
  
  // 启动未读数同步
  fetchUnreadCount()
  // 每 60 秒轮询一次，作为 SignalR 断连的兜底方案
  unreadTimer = setInterval(fetchUnreadCount, 60000)

  // 全局点击监听，用于关闭下拉菜单
  document.addEventListener('click', closeAllMenus)
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  if (unreadTimer) clearInterval(unreadTimer)
  document.removeEventListener('click', closeAllMenus)
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 全局变量与基础布局 --- */
.cyber-header {
  --bg-color: #F4F1EA;
  --ink-black: #111111;
  --cyber-red: #D92323;
  --font-mono: 'JetBrains Mono', monospace;
  --font-title: 'Anton', sans-serif;
  
  position: fixed;
  top: 0;
  width: 100%;
  height: 72px;
  background-color: var(--bg-color);
  z-index: 1000;
  font-family: var(--font-mono);
  color: var(--ink-black);
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.header-compact {
  height: 60px;
  background-color: rgba(244, 241, 234, 0.95);
  backdrop-filter: blur(5px);
}

.header-decoration-line {
  height: 4px;
  background: var(--ink-black);
  width: 100%;
}

.header-bottom-border {
  position: absolute; bottom: 0; left: 0; width: 100%; height: 2px; background: var(--ink-black);
}

.nav-container {
  max-width: 100%; padding: 0 30px; height: 100%;
  display: flex; align-items: center; justify-content: space-between;
}

/* --- Logo --- */
.logo-block {
  display: flex; align-items: center; gap: 12px; cursor: pointer; user-select: none;
}
.logo-icon-box {
  width: 36px; height: 36px; background: var(--ink-black); color: #fff;
  font-family: var(--font-title); font-size: 24px;
  display: flex; align-items: center; justify-content: center; transition: 0.2s;
}
.logo-icon-box .logo-img {
  width: 100%; height: 100%; object-fit: contain; display: block;
}
.logo-block:hover .logo-icon-box {
  background: var(--cyber-red); transform: rotate(10deg);
}
.logo-text-group { display: flex; flex-direction: column; line-height: 1; }
.logo-main { font-family: var(--font-title); font-size: 22px; letter-spacing: 1px; }
.logo-sub { font-size: 10px; color: #666; font-weight: 700; }

/* --- 导航 --- */
.cyber-nav { display: flex; align-items: center; gap: 20px; }
.nav-link-item {
  cursor: pointer; font-size: 14px; font-weight: 700; padding: 5px 0; color: #555; transition: 0.2s;
}
.nav-link-item .bracket { opacity: 0; color: var(--cyber-red); transition: 0.2s; margin: 0 2px; }
.nav-link-item:hover, .nav-link-item.active { color: var(--ink-black); }
.nav-link-item:hover .bracket, .nav-link-item.active .bracket { opacity: 1; }

/* --- 右侧功能区 --- */
.cyber-actions { display: flex; align-items: center; gap: 16px; }

.cyber-icon-btn {
  width: 32px; height: 32px; border: 2px solid var(--ink-black); background: transparent;
  cursor: pointer; font-family: var(--font-mono); font-weight: 700; font-size: 12px;
  display: flex; align-items: center; justify-content: center; transition: 0.2s;
  position: relative; /* 为红点定位 */
}
.cyber-icon-btn:hover, .cyber-icon-btn.active {
  background: var(--ink-black); color: #fff;
}

.status-terminal {
  display: flex; align-items: center; gap: 6px; font-size: 12px;
  border: 1px dashed #999; padding: 4px 8px; background: rgba(0,0,0,0.03);
}
.status-indicator { color: #00AA00; animation: blink 2s infinite; }

/* --- 用户面板 --- */
.user-control-panel { display: flex; align-items: center; gap: 15px; position: relative; }

.user-trigger {
  display: flex; align-items: center; gap: 10px; cursor: pointer; padding: 4px;
  border: 1px solid transparent; transition: 0.2s; position: relative;
}
.user-trigger:hover, .user-trigger.menu-open {
  background: #fff; border-color: var(--ink-black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.avatar-frame {
  width: 32px; height: 32px; border: 2px solid var(--ink-black); overflow: hidden;
}
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-fallback { 
  width: 100%; height: 100%; background: var(--ink-black); color: #fff;
  display: flex; align-items: center; justify-content: center; font-weight: 700; 
}

.user-id-block { display: flex; flex-direction: column; line-height: 1.1; }
.id-label { font-size: 8px; color: #888; }
.id-name { font-size: 12px; font-weight: 700; max-width: 80px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.alert-tag {
  background: var(--cyber-red); color: #fff; font-size: 10px; width: 16px; height: 16px;
  display: flex; align-items: center; justify-content: center; font-weight: 700;
  position: absolute; top: -5px; right: -5px;
}

/* --- 通知面板样式 (关键新增) --- */
.notification-wrapper {
  position: relative;
}

.mini-badge {
  position: absolute;
  top: -2px; right: -2px;
  width: 8px; height: 8px;
  background-color: var(--cyber-red);
  border-radius: 50%;
  border: 2px solid #F4F1EA;
  z-index: 5;
  box-shadow: 0 0 6px var(--cyber-red);
  animation: badge-pulse 2s infinite ease-in-out;
}

@keyframes badge-pulse {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.3); opacity: 0.7; }
  100% { transform: scale(1); opacity: 1; }
}

.notification-popover {
  position: absolute;
  top: 52px; /* 贴合按钮下方 */
  right: -50px; /* 向左微调，确保不贴边 */
  z-index: 2000;
  filter: drop-shadow(0 10px 30px rgba(0,0,0,0.2));
}

/* --- 用户下拉菜单 --- */
.cyber-dropdown-menu {
  position: absolute; top: 60px; right: 0; width: 200px;
  background: #fff; border: 2px solid var(--ink-black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.2); z-index: 1001;
}
.menu-header { background: var(--ink-black); color: #fff; font-size: 10px; padding: 4px 8px; }
.menu-list { padding: 8px 0; }
.menu-row { display: flex; justify-content: space-between; padding: 8px 16px; cursor: pointer; font-size: 12px; font-weight: 700; color: #555; }
.menu-row:hover { background: #eee; color: var(--ink-black); }
.menu-row.logout:hover { background: var(--cyber-red); color: #fff; }
.row-val.alert { background: var(--cyber-red); color: #fff; padding: 0 4px; }
.menu-divider { text-align: center; color: #ddd; font-size: 10px; margin: 4px 0; overflow: hidden; }

/* --- 按钮动画与适配 --- */
.scale-fade-enter-active, .scale-fade-leave-active { transition: all 0.2s; }
.scale-fade-enter-from, .scale-fade-leave-to { opacity: 0; transform: translateY(-10px) scale(0.95); }
.pop-down-enter-active, .pop-down-leave-active { transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); }
.pop-down-enter-from, .pop-down-leave-to { opacity: 0; transform: translateY(-20px) scale(0.9); }

/* --- 其他 --- */
.tactical-btn-wrapper { position: relative; margin-left: 5px; }
.tactical-btn {
  height: 36px; padding: 0 16px; background: var(--ink-black); color: #fff; border: none;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer;
  display: flex; align-items: center; gap: 6px; position: relative; transition: 0.1s;
  clip-path: polygon(10% 0, 100% 0, 100% 70%, 90% 100%, 0 100%, 0 30%);
}
.tactical-btn.red { background: var(--cyber-red); }
.tactical-btn:hover { transform: translate(-2px, -2px); box-shadow: 3px 3px 0 var(--ink-black); }
.tactical-btn:active { transform: translate(0, 0); box-shadow: none; }
.btn-deco-line {
  position: absolute; bottom: -4px; right: -4px; width: 10px; height: 10px;
  border-right: 2px solid var(--ink-black); border-bottom: 2px solid var(--ink-black);
}

.auth-group { display: flex; gap: 10px; }
.cyber-btn-outline {
  background: transparent; border: 2px solid var(--ink-black); padding: 6px 16px;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer; transition: 0.2s;
}
.cyber-btn-outline:hover { background: var(--ink-black); color: #fff; }
.cyber-btn-solid {
  background: var(--ink-black); border: 2px solid var(--ink-black); color: #fff; padding: 6px 16px;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer; transition: 0.2s;
}
.cyber-btn-solid:hover { background: var(--cyber-red); border-color: var(--cyber-red); }

@keyframes blink { 0% { opacity: 1; } 50% { opacity: 0; } 100% { opacity: 1; } }

/* 移动端适配 */
@media (max-width: 480px) {
  .notification-popover {
    position: fixed; top: 72px; right: 0; left: 0; width: 100vw;
    display: flex; justify-content: center;
  }
}
@media (max-width: 768px) {
  .nav-container { padding: 0 15px; }
  .cyber-nav, .status-terminal, .id-label, .id-name, .logo-sub { display: none; }
  .tactical-btn .btn-label { display: none; }
  .tactical-btn { padding: 0 10px; }
  .header-decoration-line { height: 2px; }
}
</style>