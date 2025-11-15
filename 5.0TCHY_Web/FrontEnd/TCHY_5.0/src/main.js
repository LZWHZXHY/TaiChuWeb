import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router/app.js'  


const pinia = createPinia()
const app = createApp(App);
app.use(pinia)
app.use(router);

// 确保在挂载前初始化认证状态
import { useAuthStore } from '@/utils/auth'
const authStore = useAuthStore()
authStore.checkAuth() // 检查现有认证状态

app.mount('#app');