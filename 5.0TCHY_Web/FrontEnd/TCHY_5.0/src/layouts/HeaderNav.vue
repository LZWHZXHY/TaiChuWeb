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
        <!-- 用户数量显示 -->
        <div class="user-count" v-if="userCount > 0">
          <div class="count-icon">👥</div>
          <div class="count-text">
            <span class="count-number">{{ formattedCount }}</span>
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
            <div class="dropdown-arrow">▼</div>
          </div>
          
          <!-- 用户下拉菜单 -->
          <div v-if="showUserMenu" class="user-dropdown">
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
import { useNavigation, useRouteActive } from '@/composables/useNavigation'
import DropdownMenu from './DropdownMenu.vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()
const router = useRouter()

const userCount = ref(0)
const isLoading = ref(false)
const showUserMenu = ref(false)

const props = defineProps({
  navItems: {
    type: Array,
    required: true
  }
})

const emit = defineEmits(['nav-change', 'user-action'])

const { isScrolled } = useNavigation()
const { isActive: checkActive } = useRouteActive()

const currentPath = computed(() => router.currentRoute.value.path)

const isActive = (path) => checkActive(currentPath.value, path)

// 格式化数字显示
const formattedCount = computed(() => {
  if (userCount.value >= 10000) {
    return `${(userCount.value / 10000).toFixed(1)}万`
  } else if (userCount.value >= 1000) {
    return `${(userCount.value / 1000).toFixed(1)}千`
  }
  return userCount.value.toString()
})

// 直接获取用户数量
const loadUserCount = async () => {
  try {
    isLoading.value = true
    const response = await apiClient.get('/default/users/count')
    userCount.value = response.data.count
  } catch (error) {
    console.error('获取用户数量失败:', error)
    userCount.value = 0
  } finally {
    isLoading.value = false
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
  router.push('/profile')
}

const goToSettings = () => {
  showUserMenu.value = false
  router.push('/settings')
}

const handleLogout = async () => {
  try {
    showUserMenu.value = false
    await authStore.logout()
    router.push('/')
  } catch (error) {
    console.error('退出登录失败:', error)
  }
}

const closeUserMenu = (event) => {
  if (!event.target.closest('.user-menu')) {
    showUserMenu.value = false
  }
}

// 监听路由变化关闭用户菜单
watch(() => router.currentRoute.value.path, () => {
  showUserMenu.value = false
})

// 组件挂载时加载数据
onMounted(() => {
  try {
    loadUserCount()
    document.addEventListener('click', closeUserMenu)
  } catch (error) {
    console.error('组件初始化失败:', error)
  }
})

// 组件卸载时移除事件监听
onUnmounted(() => {
  document.removeEventListener('click', closeUserMenu)
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

.logo:hover {
  color: #3498db;
}

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

.nav-item--active .nav-item-indicator {
  width: 100%;
}

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

.nav-item:hover .nav-item-indicator {
  width: 60%;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 20px;
}

/* 用户数量样式 */
.user-count {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(52, 152, 219, 0.1);
  border-radius: 8px;
  color: #2c3e50;
}

.count-icon {
  font-size: 16px;
}

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

/* 登录注册按钮样式 */
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
}

.register-btn {
  background: #3498db;
  color: white;
}

.register-btn:hover {
  background: #2980b9;
}

/* 用户菜单样式 */
.user-menu {
  position: relative;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.3s ease;
}

.user-info:hover {
  background: rgba(0, 0, 0, 0.05);
}

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
}

.dropdown-arrow {
  font-size: 10px;
  color: #666;
  transition: transform 0.3s ease;
}

.user-info:hover .dropdown-arrow {
  transform: rotate(180deg);
}

/* 用户下拉菜单 */
.user-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  min-width: 150px;
  z-index: 1001;
}

.dropdown-item {
  padding: 12px 16px;
  cursor: pointer;
  transition: background 0.3s ease;
  color: #2c3e50;
}

.dropdown-item:hover {
  background: #f8f9fa;
}

.dropdown-divider {
  height: 1px;
  background: #e0e0e0;
  margin: 4px 0;
}

.logout-item {
  color: #e74c3c;
}

.logout-item:hover {
  background: #fee;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .nav-container {
    padding: 0 16px;
  }
  
  .desktop-nav {
    display: none;
  }
  
  .user-count {
    display: none;
  }
  
  .auth-buttons {
    gap: 8px;
  }
  
  .login-btn, .register-btn {
    padding: 6px 16px;
    font-size: 13px;
  }
}
</style>