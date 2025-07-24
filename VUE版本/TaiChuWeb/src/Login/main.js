import { createApp } from 'vue'
import App from './LoginMain.vue'
import { createRouter, createWebHashHistory } from 'vue-router'
//import router from '../router'

import Mainbox from './login/Mainbox.vue'
import Login from './login/login.vue'



const routes = [
  {
    path:"/login",
    name:"login",
    component:Login
  },
  {
    path:"/mainbox",
    name:"mainbox",
    component:Mainbox
  }
]


const router = createRouter({
  history:createWebHashHistory(),
  routes:routes
})

//路由拦截
router.beforeEach((to,from,next)=>{
  if(to.name=="login"){
    next()
  }else{
    if(!localStorage.getItem("token")){
    next({path:'/login'})
  }else{
    next()
  }
  }
  

})

export default router




const app = createApp(App)
app.use(router)
app.mount('#app')
