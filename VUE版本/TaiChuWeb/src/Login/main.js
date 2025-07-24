import { createApp } from 'vue'
import App from './LoginMain.vue'
import { createRouter, createWebHashHistory } from 'vue-router'
//import router from '../router'
import RoutesConfig from './config'
import Mainbox from './login/Mainbox.vue'
import Login from './login/login.vue'
import NotFound from './notfound/NotFound.vue'
import{useRouterStore} from '../store/useRouterStrore'
import { createPinia } from 'pinia'

const pinia = createPinia()

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

  const {isGetterRouter, } = useRouterStore()

  if(to.name=="login"){
    next()
  }else{
    if(!localStorage.getItem("token")){
    next({path:'/login'})
  }else{
    //动态配置路由
    
    if(!isGetterRouter){
      ConfigRouter()
      next({
        path:to.fullPath
      })
    }else{
      next()
    }
    
  }
  }
  

})



const ConfigRouter = ()=>{

  const {changeRouter } = useRouterStore()

    RoutesConfig.forEach(item=>{
      router.addRoute("mainbox",item)
  })

  //重定向
    router.addRoute("mainbox",{
    path:"/",
    redirect:"/index"
  })
  //404匹配
  router.addRoute("mainbox",{
    path:"/:pathMatch(.*)*",
    name:"not found 404",
    component:NotFound
  })



  changeRouter(true)
}







export default router




const app = createApp(App)
app.use(pinia)
app.use(router)
app.mount('#app')
