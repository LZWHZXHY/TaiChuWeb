import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css' 
import router from './router/app.js'
import Antd from 'ant-design-vue';
import 'ant-design-vue/dist/reset.css';
import i18n from './i18n'


const app = createApp(App)
const pinia = createPinia()


app.use(ElementPlus)
app.use(pinia)
app.use(router)
app.use(Antd);
app.use(i18n)


const initApp = async () => {
  try {
    console.log('🚀 开始初始化应用...')
    await new Promise(resolve => setTimeout(resolve, 100))
    
    // 1. 初始化认证状态
    const { useAuthStore } = await import('@/utils/auth')
    const authStore = useAuthStore()
    const isAuthenticated = authStore.checkAuth()
    
    // 2. 【新增】初始化全局在线统计
    // 只有在认证通过或需要统计全站时调用
    const { useOnlineStore } = await import('@/stores/online')
    const onlineStore = useOnlineStore()
    
    // 定义后端地址
    const BASE_URL = window.location.hostname === 'localhost' 
      ? 'https://localhost:44359' 
      : 'https://bianyuzhou.com'

    // 启动全局连接（不随组件销毁而断开）
    await onlineStore.startSignalR(BASE_URL)
    
    console.log('✅ 应用初始化完成')
    // ... 原有 log 保持不变 ...
    
  } catch (error) {
    console.error('❌ 应用初始化失败:', error)
  } finally {
    app.mount('#app')
    console.log('📍 应用已挂载')
  }
}

initApp()