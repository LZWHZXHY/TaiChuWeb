<template>
  <header class="header-nav" :class="{ 'header-scrolled': isScrolled }">
    <div class="nav-container">
      <div class="logo" @click="navigateToHome">
        <span class="logo-text">太初寰宇</span>
      </div>

      <nav class="desktop-nav">
        <template v-for="item in navItems" :key="item.path || item.name">
          <div
            v-if="item.type === 'link'"
            class="nav-item"
            :class="{ 'nav-item--active': isActive(item.path) }"
            @click="handleNavClick(item)"
          >
            <span class="nav-item-text">{{ item.name }}</span>
            <div class="nav-item-indicator"></div>
          </div>
          <DropdownMenu
            v-else-if="item.type === 'dropdown'"
            :items="item.children"
            :trigger-text="item.name"
            @item-click="handleNavClick"
            class="nav-dropdown"
          />
        </template>
      </nav>

      <div class="nav-actions">
        <div class="user-count" v-if="userCount > 0">
          <div class="count-icon">👥</div>
          <div class="count-text">
            <span class="count-number">{{ userCount }}</span>
            <span class="count-label">位成员</span>
          </div>
        </div>

        <div v-if="authStore.isAuthenticated" class="user-menu">
          <div class="user-info" @click="toggleUserMenu">
            
            <img 
              v-if="realAvatarUrl"
              :src="realAvatarUrl" 
              class="user-avatar" 
              alt="Avatar"
              @error="handleImageError"
            />
            
            <div v-else class="avatar-placeholder">
              {{ userNameText.charAt(0)?.toUpperCase() || 'U' }}
            </div>

            <span class="username">{{ userNameText }}</span>
            
            <div v-if="unreadCount > 0" class="notification-badge">
              <span class="badge-count">{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
            </div>
            
            <div class="dropdown-arrow">▼</div>
          </div>
          
          <div v-if="showUserMenu" class="user-dropdown">
            <div class="dropdown-item" @click="goToNotifications">
              <span>通知中心</span>
              <span v-if="unreadCount > 0" class="dropdown-badge">{{ unreadCount }}</span>
            </div>
            <div class="dropdown-item" @click="goToProfile">
              <span>个人资料</span>
            </div>
            <div class="dropdown-item" @click="goToSettings">
              <span>账户设置</span>
            </div>
            <div class="dropdown-divider"></div>
            <div class="dropdown-item logout-item" @click="handleLogout">
              <span>退出登录</span>
            </div>
          </div>
        </div>

        <div v-else class="auth-buttons">
          <button @click="handleLogin" class="login-btn">登录</button>
          <button @click="handleRegister" class="register-btn">注册</button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import DropdownMenu from './DropdownMenu.vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()
const router = useRouter()

// ====== 1. 配置基础域名 ======
// 生产环境用 bianyuzhou.com，开发环境用 localhost
const BASE_URL = 'https://bianyuzhou.com'

// 状态变量
const userCount = ref(0)
const unreadCount = ref(0)
const showUserMenu = ref(false)
const isScrolled = ref(false)
const avatarLoadError = ref(false) // 图片是否加载失败

// ====== 2. 核心逻辑：获取最新用户信息 ======
// 页面加载时，直接请求接口，拿到最新的头像字段
const fetchLatestUserInfo = async () => {
  if (!authStore.isAuthenticated) return

  try {
    const res = await apiClient.get('/default/user/me')
    if (res.data && res.data.success) {
      const userData = res.data.data
      
      // 重要：把拿到的最新数据（含 avatar）更新到 Pinia Store 中
      // 这样整个应用都能用上最新的头像
      authStore.user = {
        ...authStore.user,
        ...userData // 这会覆盖旧数据，把 avatar 字段补进去
      }
      
      console.log('✅ 用户信息已同步，头像路径:', userData.avatar)
    }
  } catch (error) {
    console.warn('获取用户信息失败:', error)
  }
}

// ====== 3. 核心逻辑：拼接图片地址 ======
// 这里不需要复杂的正则，只要做最简单的拼接
const realAvatarUrl = computed(() => {
  // 从 Store 里拿头像路径
  let path = authStore.user?.avatar || authStore.user?.logo
  
  // 1. 如果没有路径，或者是图片加载报错了，返回 null (显示文字头像)
  if (!path || avatarLoadError.value) return null
  
  // 2. 如果已经是完整链接 (比如 http 开头)，直接用
  if (path.startsWith('http')) return path

  // 3. 规范化路径：把 Windows 的反斜杠 \ 换成正斜杠 /
  path = path.replace(/\\/g, '/')
  
  // 4. 去掉开头的 /
  if (path.startsWith('/')) path = path.substring(1)

  // 5. 补全 uploads 目录 (如果你数据库只存了 "头像/82/xxx")
  if (!path.startsWith('uploads/')) {
    path = `uploads/${path}`
  }

  // 6. 最终拼接：域名 + 路径
  return `${BASE_URL}/${path}`
})

const userNameText = computed(() => {
  return authStore.user?.name || authStore.user?.username || '用户'
})

const handleImageError = () => {
  avatarLoadError.value = true
}

// 监听用户变化，重置错误状态
watch(() => authStore.user, () => {
  avatarLoadError.value = false
})

// ... 以下是通用的路由和菜单逻辑 (保持不变) ...
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
const goToSettings = () => { showUserMenu.value = false; router.push('/settings') }
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
  if (!event.target.closest('.user-menu')) showUserMenu.value = false
}
const handleScroll = () => isScrolled.value = window.scrollY > 10

onMounted(() => {
  loadUserCount()
  
  // 🔥🔥🔥 页面加载时，立即去获取最新的用户信息！ 🔥🔥🔥
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
/* 样式保持不变，直接复制之前的即可 */
.header-nav { position: fixed; top: 0; width: 100%; background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px); border-bottom: 1px solid rgba(0, 0, 0, 0.1); transition: all 0.3s ease; z-index: 1000; }
.header-scrolled { background: rgba(255, 255, 255, 0.98); box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1); }
.nav-container { max-width: 1200px; margin: 0 auto; padding: 0 32px; display: flex; align-items: center; justify-content: space-between; height: 70px; }
.logo { cursor: pointer; font-size: 24px; font-weight: bold; color: #2c3e50; transition: color 0.3s ease; }
.logo:hover { color: #3498db; }
.desktop-nav { display: flex; align-items: center; gap: 40px; }
.nav-item { position: relative; padding: 8px 16px; cursor: pointer; color: #666; font-size: 16px; font-weight: 500; transition: all 0.3s ease; border-radius: 6px; }
.nav-item:hover { color: #2c3e50; background: rgba(0, 0, 0, 0.03); }
.nav-item--active { color: #3498db; font-weight: 600; }
.nav-item--active .nav-item-indicator { width: 100%; }
.nav-item-indicator { position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); width: 0; height: 2px; background: #3498db; transition: width 0.3s ease; }
.nav-item:hover .nav-item-indicator { width: 60%; }
.nav-actions { display: flex; align-items: center; gap: 20px; }
.user-count { display: flex; align-items: center; gap: 8px; padding: 8px 12px; background: rgba(52, 152, 219, 0.1); border-radius: 8px; color: #2c3e50; transition: all 0.3s ease; }
.user-count:hover { background: rgba(52, 152, 219, 0.15); transform: translateY(-1px); }
.count-icon { font-size: 16px; }
.count-text { display: flex; align-items: baseline; gap: 4px; }
.count-number { font-weight: 600; font-size: 14px; }
.count-label { font-size: 12px; color: #666; }
.auth-buttons { display: flex; gap: 12px; }
.login-btn, .register-btn { padding: 8px 20px; border: none; border-radius: 6px; font-size: 14px; font-weight: 500; cursor: pointer; transition: all 0.3s ease; }
.login-btn { background: transparent; border: 1px solid #ddd; color: #666; }
.login-btn:hover { border-color: #3498db; color: #3498db; transform: translateY(-1px); }
.register-btn { background: #3498db; color: white; }
.register-btn:hover { background: #2980b9; transform: translateY(-1px); }
.user-menu { position: relative; }
.user-info { display: flex; align-items: center; gap: 8px; padding: 6px 12px; border-radius: 8px; cursor: pointer; transition: background 0.3s ease; position: relative; }
.user-info:hover { background: rgba(0, 0, 0, 0.05); }
.user-avatar { width: 32px; height: 32px; border-radius: 50%; object-fit: cover; border: 1px solid rgba(0,0,0,0.1); background-color: #f0f2f5; }
.avatar-placeholder { width: 32px; height: 32px; border-radius: 50%; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; color: white; font-weight: 600; font-size: 14px; }
.username { font-weight: 500; color: #2c3e50; max-width: 100px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dropdown-arrow { font-size: 10px; color: #666; transition: transform 0.3s ease; }
.user-info:hover .dropdown-arrow { transform: rotate(180deg); }
.notification-badge { position: relative; display: flex; align-items: center; justify-content: center; min-width: 20px; height: 20px; background: #ff4444; border-radius: 10px; border: 2px solid white; box-shadow: 0 2px 8px rgba(255, 68, 68, 0.3); animation: pulse 2s infinite; }
.badge-count { color: white; font-size: 11px; font-weight: 600; line-height: 1; padding: 0 4px; }
@keyframes pulse { 0% { transform: scale(1); } 50% { transform: scale(1.05); } 100% { transform: scale(1); } }
.user-dropdown { position: absolute; top: 100%; right: 0; margin-top: 8px; background: white; border: 1px solid #e0e0e0; border-radius: 8px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); min-width: 180px; z-index: 1001; animation: dropdownFadeIn 0.2s ease; }
@keyframes dropdownFadeIn { from { opacity: 0; transform: translateY(-10px);} to { opacity: 1; transform: translateY(0);} }
.dropdown-item { padding: 12px 16px; cursor: pointer; transition: background 0.3s ease; color: #2c3e50; font-size: 14px; display: flex; justify-content: space-between; align-items: center; }
.dropdown-item:hover { background: #f8f9fa; }
.dropdown-badge { background: #ff4444; color: white; border-radius: 10px; padding: 2px 6px; font-size: 11px; font-weight: 600; min-width: 18px; text-align: center; }
.dropdown-divider { height: 1px; background: #e0e0e0; margin: 4px 0;}
.logout-item { color: #e74c3c;}
.logout-item:hover { background: #fee; }
@media (max-width: 768px) {
  .nav-container { padding: 0 16px; }
  .desktop-nav { display: none; }
  .user-count { display: none; }
  .auth-buttons { gap: 8px;}
  .login-btn, .register-btn { padding: 6px 16px; font-size: 13px; }
  .username { max-width: 80px;}
  .notification-badge { min-width: 18px; height: 18px; border-width: 1.5px; }
  .badge-count { font-size: 10px; padding: 0 3px; }
}
</style>