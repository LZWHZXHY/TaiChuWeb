import { createRouter, createWebHistory } from 'vue-router'

// 路由配置
const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/Home.vue')
  },
  {
    path:'/login',
    name:'登录',
    component: () => import('@/LoginRegister/Login.vue'),
    meta: { requiresGuest: true }

  },
  {
    path:'/register',
    name:'注册',
    component: () => import('@/LoginRegister/Register.vue'),
    meta: { requiresGuest: true }
  },
  {
    path:'/art',
    name:'绘画板块',
    component: () => import('@/views/Art.vue')
  }

]

const router = createRouter({
  history: createWebHistory(),
  routes,
})




export default router