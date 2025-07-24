import { createApp } from 'vue'
import App from './App.vue'
import{createRouter, createWebHistory} from 'vue-router'
import AboutUS from "/src/components/AboutUS.vue"
import transfer from "/src/components/Transfer.vue"
import HW from "/src/components/HelloWorld.vue"

//1.配置路由规则

const routes = [
  {path:"/about", component: AboutUS},
  {path:"/transfer", component:transfer},
  {path:"/HelloWorld", component:HW},
]



//2.创建路由器
const router = createRouter({
  history:createWebHistory(),
  routes
})



//3.加载路由器
const app = createApp(App)
app.use(router)

app.mount('#app')
