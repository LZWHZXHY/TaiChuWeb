<template>
  <header class="header-nav" :class="{ 'header-scrolled': isScrolled }">
    <div class="nav-container">
      <!-- Logo区域 -->
      <div class="logo" @click="navigateToHome">
        <span class="logo-text">太初寰宇</span>
      </div>

      <!-- 导航菜单 -->
      <nav class="desktop-nav">
        <template v-for="item in navItems" :key="item.path || item.name">
          <!-- 普通链接 -->
          <div
            v-if="item.type === 'link'"
            class="nav-item"
            :class="{ 'nav-item--active': isActive(item.path) }"
            @click="handleNavClick(item)"
          >
            <span class="nav-item-text">{{ item.name }}</span>
            <div class="nav-item-indicator"></div>
          </div>
          <!-- 下拉菜单 -->
          <DropdownMenu
            v-else-if="item.type === 'dropdown'"
            :items="item.children"
            :trigger-text="item.name"
            @item-click="handleNavClick"
            class="nav-dropdown"
          />
        </template>
      </nav>

      <!-- 右侧功能区 -->
      <div class="nav-actions">
        <!-- 用户数量显示（精确人数，无单位换算） -->
        <div class="user-count" v-if="userCount > 0">
          <div class="count-icon">👥</div>
          <div class="count-text">
            <span class="count-number">{{ userCount }}</span>
            <span class="count-label">位成员</span>
          </div>
        </div>

        <!-- 登录状态显示 -->
        <div v-if="authStore.isAuthenticated" class="user-menu">
          <div class="user-info" @click="toggleUserMenu">
            <div class="avatar-placeholder">
              {{ authStore.user?.username?.charAt(0) || 'U' }}
            </div>
            <span class="username">{{ authStore.user?.username }}</span>
            
            <!-- 未读通知徽标 -->
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

        <!-- 未登录状态 -->
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

const userCount = ref(0) // 默认为 0
const unreadCount = ref(0) // 未读通知数量
const showUserMenu = ref(false)
const isScrolled = ref(false)
let notificationInterval = null

const props = defineProps({
  navItems: {
    type: Array,
    required: true
  },
  unreadCount: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits(['nav-change', 'user-action'])

// 检查路由是否激活
const isActive = (path) => {
  const currentPath = router.currentRoute.value.path
  if (path === '/') {
    return currentPath === '/'
  }
  return currentPath === path || currentPath.startsWith(path + '/')
}

// 精确显示人数，无格式单位换算
const loadUserCount = async () => {
  try {
    const response = await apiClient.get('/default/users/count')
    if (typeof response.data.count === 'number' && !isNaN(response.data.count)) {
      userCount.value = response.data.count
      console.log('✅ 用户数量获取成功:', userCount.value)
    } else {
      userCount.value = 0
      console.warn('⚠️ 未获取到用户数量，显示为0')
    }
  } catch (err) {
    console.error('获取用户数量失败，显示为0')
    userCount.value = 0
  }
}

// 加载未读通知数量
const loadUnreadCount = async () => {
  if (!authStore.isAuthenticated) {
    unreadCount.value = 0
    return
  }
  
  try {
    const response = await apiClient.get('/Notification/unread/count')
    unreadCount.value = response?.data?.unread ?? 0
  } catch (err) {
    console.warn('未读通知数量获取失败，使用降级方案:', err)
    // 降级方案：获取通知列表计算未读数量
    try {
      const response = await apiClient.get('/Notification/mine', {
        params: { isRead: false, page: 1, pageSize: 1 }
      })
      const data = response?.data?.data
      if (data && typeof data.total === 'number') {
        unreadCount.value = data.total
      } else {
        const items = data?.items ?? []
        unreadCount.value = Array.isArray(items) ? items.length : 0
      }
    } catch (err2) {
      console.error('降级方案也失败:', err2)
      unreadCount.value = 0
    }
  }
}

const handleNavClick = (item) => {
  emit('nav-change', item)
  if (item.path) {
    router.push(item.path)
  }
}

const navigateToHome = () => router.push('/')

const handleLogin = () => {
  router.push('/login')
}

const handleRegister = () => {
  router.push('/register')
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

const goToProfile = () => {
  showUserMenu.value = false
  router.push('/profile/me')
}

const goToSettings = () => {
  showUserMenu.value = false
  router.push('/settings')
}

const goToNotifications = () => {
  showUserMenu.value = false
  // 跳转到个人资料页面的通知标签页
  router.push('/profile/me?tab=notification')
}

const handleLogout = async () => {
  try {
    showUserMenu.value = false
    authStore.logout()
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
    unreadCount.value = 0 // 登出后清空未读计数
    console.log('✅ 用户已登出')
    router.push('/')
  } catch (error) {
    console.error('退出登录失败:', error)
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
    unreadCount.value = 0
    router.push('/')
  }
}

const closeUserMenu = (event) => {
  if (!event.target.closest('.user-menu')) {
    showUserMenu.value = false
  }
}

const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

// 监听认证状态变化
watch(() => authStore.isAuthenticated, (isAuthenticated) => {
  if (isAuthenticated) {
    // 登录后立即加载未读数量
    loadUnreadCount()
    // 启动定时刷新
    notificationInterval = setInterval(loadUnreadCount, 60000) // 每分钟刷新一次
  } else {
    // 登出后清空未读数量并清除定时器
    unreadCount.value = 0
    if (notificationInterval) {
      clearInterval(notificationInterval)
      notificationInterval = null
    }
  }
  showUserMenu.value = false
})

watch(() => router.currentRoute.value.path, () => {
  showUserMenu.value = false
})

onMounted(() => {
  loadUserCount()
  
  // 如果已登录，启动未读通知定时刷新
  if (authStore.isAuthenticated) {
    loadUnreadCount()
    notificationInterval = setInterval(loadUnreadCount, 60000)
  }
  
  document.addEventListener('click', closeUserMenu)
  window.addEventListener('scroll', handleScroll)
  window.addEventListener('unauthorized', () => {
    showUserMenu.value = false
    unreadCount.value = 0
  })
})

// 在onMounted中添加全局事件监听
onMounted(() => {
  loadUserCount()
  
  // 监听未读数量更新事件
  window.addEventListener('unread-count-updated', (event) => {
    if (event.detail?.count !== undefined) {
      unreadCount.value = event.detail.count
    }
  })
  
  if (authStore.isAuthenticated) {
    loadUnreadCount()
    notificationInterval = setInterval(loadUnreadCount, 60000)
  }
  
  document.addEventListener('click', closeUserMenu)
  window.addEventListener('scroll', handleScroll)
  window.addEventListener('unauthorized', () => {
    showUserMenu.value = false
    unreadCount.value = 0
  })
})

// 在onUnmounted中移除事件监听
onUnmounted(() => {
  if (notificationInterval) {
    clearInterval(notificationInterval)
  }
  // 移除未读数量更新事件监听
  window.removeEventListener('unread-count-updated', () => {})
  document.removeEventListener('click', closeUserMenu)
  window.removeEventListener('scroll', handleScroll)
  window.removeEventListener('unauthorized', () => {})
})
</script>

<style scoped>
.header-nav {
  position: fixed;
  top: 0;
  width: 100%;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  z-index: 1000;
}
.header-scrolled {
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
}
.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 70px;
}
.logo {
  cursor: pointer;
  font-size: 24px;
  font-weight: bold;
  color: #2c3e50;
  transition: color 0.3s ease;
}
.logo:hover { color: #3498db; }
.desktop-nav {
  display: flex;
  align-items: center;
  gap: 40px;
}
.nav-item {
  position: relative;
  padding: 8px 16px;
  cursor: pointer;
  color: #666;
  font-size: 16px;
  font-weight: 500;
  transition: all 0.3s ease;
  border-radius: 6px;
}
.nav-item:hover {
  color: #2c3e50;
  background: rgba(0, 0, 0, 0.03);
}
.nav-item--active {
  color: #3498db;
  font-weight: 600;
}
.nav-item--active .nav-item-indicator { width: 100%; }
.nav-item-indicator {
  position: absolute;
  bottom: -8px;
  left: 50%;
  transform: translateX(-50%);
  width: 0;
  height: 2px;
  background: #3498db;
  transition: width 0.3s ease;
}
.nav-item:hover .nav-item-indicator { width: 60%; }
.nav-actions {
  display: flex;
  align-items: center;
  gap: 20px;
}
.user-count {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(52, 152, 219, 0.1);
  border-radius: 8px;
  color: #2c3e50;
  transition: all 0.3s ease;
}
.user-count:hover {
  background: rgba(52, 152, 219, 0.15);
  transform: translateY(-1px);
}
.count-icon { font-size: 16px; }
.count-text {
  display: flex;
  align-items: baseline;
  gap: 4px;
}
.count-number {
  font-weight: 600;
  font-size: 14px;
}
.count-label {
  font-size: 12px;
  color: #666;
}
.auth-buttons {
  display: flex;
  gap: 12px;
}
.login-btn, .register-btn {
  padding: 8px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}
.login-btn {
  background: transparent;
  border: 1px solid #ddd;
  color: #666;
}
.login-btn:hover {
  border-color: #3498db;
  color: #3498db;
  transform: translateY(-1px);
}
.register-btn {
  background: #3498db;
  color: white;
}
.register-btn:hover {
  background: #2980b9;
  transform: translateY(-1px);
}
.user-menu { position: relative; }
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.3s ease;
  position: relative;
}
.user-info:hover { background: rgba(0, 0, 0, 0.05); }
.avatar-placeholder {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 600;
  font-size: 14px;
}
.username {
  font-weight: 500;
  color: #2c3e50;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.dropdown-arrow { font-size: 10px; color: #666; transition: transform 0.3s ease; }
.user-info:hover .dropdown-arrow { transform: rotate(180deg); }

/* 未读通知徽标样式 */
.notification-badge {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 20px;
  height: 20px;
  background: #ff4444;
  border-radius: 10px;
  border: 2px solid white;
  box-shadow: 0 2px 8px rgba(255, 68, 68, 0.3);
  animation: pulse 2s infinite;
}

.badge-count {
  color: white;
  font-size: 11px;
  font-weight: 600;
  line-height: 1;
  padding: 0 4px;
}

/* 呼吸动画效果 */
@keyframes pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.05); }
  100% { transform: scale(1); }
}

.user-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  min-width: 180px;
  z-index: 1001;
  animation: dropdownFadeIn 0.2s ease;
}
@keyframes dropdownFadeIn {
  from { opacity: 0; transform: translateY(-10px);}
  to   { opacity: 1; transform: translateY(0);}
}
.dropdown-item {
  padding: 12px 16px;
  cursor: pointer;
  transition: background 0.3s ease;
  color: #2c3e50;
  font-size: 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.dropdown-item:hover { background: #f8f9fa; }

/* 下拉菜单中的徽标 */
.dropdown-badge {
  background: #ff4444;
  color: white;
  border-radius: 10px;
  padding: 2px 6px;
  font-size: 11px;
  font-weight: 600;
  min-width: 18px;
  text-align: center;
}

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
  
  /* 移动端徽标调整 */
  .notification-badge {
    min-width: 18px;
    height: 18px;
    border-width: 1.5px;
  }
  .badge-count {
    font-size: 10px;
    padding: 0 3px;
  }
}
</style>