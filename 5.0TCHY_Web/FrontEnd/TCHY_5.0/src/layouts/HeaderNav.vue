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
        <div class="user-info">
          <img :src="authStore.user?.avatar" :alt="authStore.user?.username" class="user-avatar" />
          <span class="username">{{ authStore.user?.username }}</span>
        </div>
        <button @click="handleLogout" class="logout-btn">退出</button>
      </div>
      
      <!-- 未登录状态 -->
      <div v-else class="auth-buttons">
        <AppButton variant="outline" @click="handleLogin">登录</AppButton>
        <AppButton @click="handleRegister">注册</AppButton>
      </div>
    </div>
    </div>
  </header>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useNavigation, useRouteActive } from '@/composables/useNavigation'
import DropdownMenu from './DropdownMenu.vue'
import AppButton from './AppButton.vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()


const userCount = ref(0)
const isLoading = ref(false)


const props = defineProps({
  navItems: {
    type: Array,
    required: true
  }
})
const emit = defineEmits(['nav-change', 'user-action'])

const router = useRouter()
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
  isLoading.value = true
  try {
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
  // 添加路由跳转
  if (item.path) {
    router.push(item.path)
  }
}

const handleUserAction = () => emit('user-action')
const navigateToHome = () => router.push('/')


const handleLogin = () => {
  router.push('/login')
}

const handleRegister = () => {
  router.push('/register')
}

const handleLogout = async () => {
  await authStore.logout()
  router.push('/')
}


// 组件挂载时加载数据
onMounted(() => {
  loadUserCount()
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

.nav-dropdown {
  position: relative;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}
</style>