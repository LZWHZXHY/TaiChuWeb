<template>
  <header class="cyber-header" :class="{ 'header-compact': isScrolled }">
    <div class="header-decoration-line"></div>

    <div class="nav-container">
      
      <div class="logo-block" @click="navigateToHome">
        <div class="logo-icon-box">T</div>
        <div class="logo-text-group">
          <span class="logo-main">TAICHU</span>
          <span class="logo-sub">NETWORK // {{ $t('HeaderNav.site_name') }}</span>
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
          
          <div class="user-trigger" @click="toggleUserMenu" :class="{ 'menu-open': showUserMenu }">
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
            <div v-if="unreadCount > 0" class="alert-tag">!</div>
          </div>

          <div v-if="showUserMenu" class="cyber-dropdown-menu">
            <div class="menu-header">// USER_ACTIONS</div>
            <div class="menu-list">
              <div class="menu-row" @click="goToNotifications">
                <span class="row-label">NOTIFICATIONS</span>
                <span v-if="unreadCount > 0" class="row-val alert">{{ unreadCount }}</span>
              </div>
              <div class="menu-row" @click="goToProfile">
                <span class="row-label">PROFILE</span>
                <span class="row-icon">-></span>
              </div>
              <div class="menu-row" @click="goToSettings">
                <span class="row-label">SETTINGS</span>
                <span class="row-icon">-></span>
              </div>
              <div class="menu-divider">----------------</div>
              <div class="menu-row logout" @click="handleLogout">
                <span class="row-label">>> LOGOUT</span>
              </div>
            </div>
          </div>

          <div class="tactical-btn-wrapper" @click="router.push('/suggest')">
            <button class="tactical-btn red">
              <span class="btn-icon">✎</span>
              <span class="btn-label">FEEDBACK</span>
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
import DropdownMenu from './DropdownMenu.vue' 
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import { useI18n } from 'vue-i18n' 

const authStore = useAuthStore()
const router = useRouter()
const { t, locale } = useI18n() 

const BASE_URL = 'https://bianyuzhou.com'

const userCount = ref(0)
const unreadCount = ref(0)
const showUserMenu = ref(false)
const isScrolled = ref(false)
const avatarLoadError = ref(false)

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
  } catch (error) { console.warn('获取用户信息失败:', error) }
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

const props = defineProps({
  navItems: { type: Array, required: true },
  unreadCount: { type: Number, default: 0 }
})
const emit = defineEmits(['nav-change', 'user-action'])

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
const toggleUserMenu = () => showUserMenu.value = !showUserMenu.value
const goToProfile = () => { showUserMenu.value = false; router.push('/profile/me') }
const goToSettings = () => { showUserMenu.value = false; router.push('/profile/MEE') }
const goToNotifications = () => { showUserMenu.value = false; router.push('/profile/me?tab=notification') }

const handleLogout = async () => {
  showUserMenu.value = false
  authStore.logout()
  localStorage.removeItem('auth_token')
  localStorage.removeItem('user')
  unreadCount.value = 0
  router.push('/')
}

const closeUserMenu = (event) => {
  if (!event.target.closest('.user-control-panel')) showUserMenu.value = false
}

const handleScroll = () => isScrolled.value = window.scrollY > 10

onMounted(() => {
  loadUserCount()
  fetchLatestUserInfo()
  document.addEventListener('click', closeUserMenu)
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  document.removeEventListener('click', closeUserMenu)
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 全局变量 --- */
.cyber-header {
  --bg-color: #F4F1EA; /* 米白色底 */
  --ink-black: #111111;
  --cyber-red: #D92323;
  --border-style: 2px solid var(--ink-black);
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

/* 装饰线条 */
.header-decoration-line {
  height: 4px;
  background: var(--ink-black);
  width: 100%;
}

.header-bottom-border {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background: var(--ink-black);
}

.nav-container {
  max-width: 100%;
  padding: 0 30px;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* --- Logo: 工业方块风格 --- */
.logo-block {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  user-select: none;
}

.logo-icon-box {
  width: 36px;
  height: 36px;
  background: var(--ink-black);
  color: #fff;
  font-family: var(--font-title);
  font-size: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.logo-block:hover .logo-icon-box {
  background: var(--cyber-red);
  transform: rotate(10deg);
}

.logo-text-group {
  display: flex;
  flex-direction: column;
  line-height: 1;
}

.logo-main {
  font-family: var(--font-title);
  font-size: 22px;
  letter-spacing: 1px;
}

.logo-sub {
  font-size: 10px;
  color: #666;
  font-weight: 700;
}

/* --- 导航: 字符界定风格 --- */
.cyber-nav {
  display: flex;
  align-items: center;
  gap: 20px;
}

.nav-link-item {
  cursor: pointer;
  font-size: 14px;
  font-weight: 700;
  padding: 5px 0;
  color: #555;
  transition: 0.2s;
}

.nav-link-item .bracket {
  opacity: 0;
  color: var(--cyber-red);
  transition: 0.2s;
  margin: 0 2px;
}

.nav-link-item:hover, .nav-link-item.active {
  color: var(--ink-black);
}

.nav-link-item:hover .bracket, .nav-link-item.active .bracket {
  opacity: 1;
}

/* --- 右侧功能区 --- */
.cyber-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

/* 语言切换方块 */
.cyber-icon-btn {
  width: 32px;
  height: 32px;
  border: 2px solid var(--ink-black);
  background: transparent;
  cursor: pointer;
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.cyber-icon-btn:hover {
  background: var(--ink-black);
  color: #fff;
}

/* 状态终端条 */
.status-terminal {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  border: 1px dashed #999;
  padding: 4px 8px;
  background: rgba(0,0,0,0.03);
}

.status-indicator {
  color: #00AA00;
  animation: blink 2s infinite;
}

/* 用户控制面板 */
.user-control-panel {
  display: flex;
  align-items: center;
  gap: 15px;
  position: relative;
}

/* 用户触发器 */
.user-trigger {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  padding: 4px;
  border: 1px solid transparent;
  transition: 0.2s;
}

.user-trigger:hover, .user-trigger.menu-open {
  background: #fff;
  border-color: var(--ink-black);
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.avatar-frame {
  width: 32px;
  height: 32px;
  border: 2px solid var(--ink-black);
  overflow: hidden;
}

.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-fallback { 
  width: 100%; height: 100%; background: var(--ink-black); 
  color: #fff; display: flex; align-items: center; justify-content: center; 
  font-weight: 700; 
}

.user-id-block {
  display: flex;
  flex-direction: column;
  line-height: 1.1;
}

.id-label { font-size: 8px; color: #888; }
.id-name { font-size: 12px; font-weight: 700; max-width: 80px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.alert-tag {
  background: var(--cyber-red);
  color: #fff;
  font-size: 10px;
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

/* 战术按钮 (Suggest) */
.tactical-btn-wrapper {
  position: relative;
  margin-left: 5px;
}

.tactical-btn {
  height: 36px;
  padding: 0 16px;
  background: var(--ink-black);
  color: #fff;
  border: none;
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  position: relative;
  transition: 0.1s;
  clip-path: polygon(10% 0, 100% 0, 100% 70%, 90% 100%, 0 100%, 0 30%); /* 切角造型 */
}

.tactical-btn.red {
  background: var(--cyber-red);
}

.tactical-btn:hover {
  transform: translate(-2px, -2px);
  box-shadow: 3px 3px 0 var(--ink-black);
}

.tactical-btn:active {
  transform: translate(0, 0);
  box-shadow: none;
}

.btn-deco-line {
  position: absolute;
  bottom: -4px;
  right: -4px;
  width: 10px;
  height: 10px;
  border-right: 2px solid var(--ink-black);
  border-bottom: 2px solid var(--ink-black);
}

/* 下拉菜单: 赛博列表 */
.cyber-dropdown-menu {
  position: absolute;
  top: 60px;
  right: 0;
  width: 200px;
  background: #fff;
  border: 2px solid var(--ink-black);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.2);
  z-index: 1001;
}

.menu-header {
  background: var(--ink-black);
  color: #fff;
  font-size: 10px;
  padding: 4px 8px;
}

.menu-list {
  padding: 8px 0;
}

.menu-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 700;
  color: #555;
}

.menu-row:hover {
  background: #eee;
  color: var(--ink-black);
}

.menu-row.logout:hover {
  background: var(--cyber-red);
  color: #fff;
}

.row-val.alert {
  background: var(--cyber-red);
  color: #fff;
  padding: 0 4px;
}

.menu-divider {
  text-align: center;
  color: #ddd;
  font-size: 10px;
  margin: 4px 0;
  overflow: hidden;
}

/* 登录/注册按钮组 */
.auth-group {
  display: flex;
  gap: 10px;
}

.cyber-btn-outline {
  background: transparent;
  border: 2px solid var(--ink-black);
  padding: 6px 16px;
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 12px;
  cursor: pointer;
  transition: 0.2s;
}

.cyber-btn-outline:hover {
  background: var(--ink-black);
  color: #fff;
}

.cyber-btn-solid {
  background: var(--ink-black);
  border: 2px solid var(--ink-black);
  color: #fff;
  padding: 6px 16px;
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 12px;
  cursor: pointer;
  transition: 0.2s;
}

.cyber-btn-solid:hover {
  background: var(--cyber-red);
  border-color: var(--cyber-red);
}

/* 动画 */
@keyframes blink { 0% { opacity: 1; } 50% { opacity: 0; } 100% { opacity: 1; } }

/* 响应式 */
@media (max-width: 768px) {
  .nav-container { padding: 0 15px; }
  .cyber-nav, .status-terminal, .id-label, .id-name, .logo-sub { display: none; }
  .tactical-btn .btn-label { display: none; }
  .tactical-btn { padding: 0 10px; }
  .header-decoration-line { height: 2px; }
}
</style>