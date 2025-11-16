import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router/app.js'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

const initApp = async () => {
  try {
    console.log('🚀 开始初始化应用...')
    
    // 等待 Vue 应用和插件完全初始化
    await new Promise(resolve => setTimeout(resolve, 100))
    
    // 初始化认证状态
    const { useAuthStore } = await import('@/utils/auth')
    const authStore = useAuthStore()
    
    // 检查认证状态
    const isAuthenticated = authStore.checkAuth()
    
    console.log('✅ 应用初始化完成')
    console.log('🔐 认证状态:', isAuthenticated)
    console.log('👤 用户信息:', authStore.user)
    
  } catch (error) {
    console.error('❌ 应用初始化失败:', error)
  } finally {
    // 挂载应用
    app.mount('#app')
    console.log('📍 应用已挂载')
  }
}

initApp()