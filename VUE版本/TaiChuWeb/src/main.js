import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'



import{createRouter, createWebHistory} from 'vue-router'

import { useUserStore } from '/src/store/user'




import Login from "/src/components/login.vue"

import Register from './components/Register.vue'





//1.配置路由规则

const routes = [
  
  {path:"/", component: () => import('./components/login.vue')},
  {path:"/login", component:Login},
  
  {path:"/Register", component:Register}
  
]



//2.创建路由器
const router = createRouter({
  history:createWebHistory(),
  routes
})





//3.加载路由器
const app = createApp(App)
const pinia = createPinia()



app.use(pinia)
app.use(router)



const userStore = useUserStore()
userStore.initialize()

app.mount('#app')
