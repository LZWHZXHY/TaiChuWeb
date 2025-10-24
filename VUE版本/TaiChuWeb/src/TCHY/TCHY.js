import { createApp } from 'vue'
import TCHY from './TCHY.vue'
import { createPinia } from 'pinia'
import { createRouter, createWebHashHistory } from 'vue-router'
import { useUserStore } from '../store/user'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
// 创建社区专属路由
const routes = [
  {
    path: '/',
    name: 'CommunityHome',
    component: TCHY,
    meta: { requiresAuth: true },
    children: [
      // 用户页面
      {
        path: 'user',
        name: '用户页面',
        component: () => import('./user_components/user.vue'),
        meta: { requiresAuth: true },
        children: [
          {
            path: '/daka',
            name: '打卡页面',
            component: () => import('./user_components/daka.vue'),
            meta: { requiresAuth: true }
          },
          {
            path:'/inventory',
            name:'仓库',
            component: () => import('./user_components/inventory.vue'),
            meta: { requiresAuth: true }  
          },{
            path:'/myPost',
            name:'我的帖子',
             component: () => import('./user_components/myPost.vue'),
            meta: { requiresAuth: true }
          },{
            path:'/myFriends',
            name:'我的帖子',
             component: () => import('./user_components/friends.vue'),
            meta: { requiresAuth: true }
          }
        ]
      },
      {
        path:'BYDworldSetting',
        name:'世界观',
        component: () => import('../BYDworld/BYDworldSetting.vue'),
        meta: { requiresAuth: true },
      },
      {
        path:'shop',
        name:'太初交易行',
        component: () => import('./components/Shop.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: '/BYDstory',
        name: '彼岸故事',
        component: () => import('../BYDworld/BYDstory.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: '/Review',
        name: '审核页面',
        component: () => import('./components/Review.vue'),
        meta: { requiresAuth: true, requiredRank: 1 }
      },
      {
        path: '/BYD-join',
        name: '彼岸企划制作',
        component: () => import('../BYDworld/BYD_join.vue'),
        meta: { requiresAuth: true}
      },
      // 太初部分
      {
        path: 'taichu',
        name: '太初',
        component: () => import('./components/Taichu.vue'),
        meta: { requiresAuth: true },
        children: [
          {
            path: '/BYDseries',
            name: '彼岸宇宙系列',
            component: () => import('./tc-components/BYDseries.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/BYDanimation',
            name: '彼岸动画',
            component: () => import('./tc-components/BYDanimation.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/BYDgame',
            name: '彼岸游戏',
            component: () => import('./tc-components/BYDgame.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/intro',
            name: '介绍',
            component: () => import('./tc-components/intro.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/links',
            name: '链接',
            component: () => import('./tc-components/links.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/news',
            name: '消息',
            component: () => import('./tc-components/news.vue'),
            meta: { requiresAuth: true }
          },{
            path: '/updates',
            name: '更新',
            component: () => import('./tc-components/Updates.vue'),
            meta: { requiresAuth: true }
          },{
            path: '/rules',
            name: '规则',
            component: () => import('./tc-components/rules.vue'),
            meta: { requiresAuth: true }
          }
        ]
      },
      {
        path:'/financial',
        name:'财政',
        component:()=> import('./tc-components/financial.vue'),
        meta: { requiresAuth: true }
        
      },
      // 游戏
      {
        path: 'game',
        name: '游戏',
        component: () => import('./components/Game.vue'),
        meta: { requiresAuth: true }
      },
      // 排行
      {
        path: 'rank',
        name: '排位',
        component: () => import('./components/Rank.vue'),
        meta: { requiresAuth: true }
      },
      {
        path:"resource",
        name:'资源板块',
        component: () => import('./components/Resource.vue'),
        meta: { requiresAuth: true, requireLevel: 8}
      },
      
      // 意见箱
      {
        path: 'feedback',
        name: '意见箱',
        component: () => import('./components/FeedBack.vue'),
        meta: { requiresAuth: true }
      },
      
      // 帖子区
      {
        path: 'posts',
        name: '帖子区',
        component: () => import('./components/Posts.vue'),
        meta: { requiresAuth: true }
      },
      
      // 柴圈部分
      {
        path: 'chai',
        name: 'Chai',
        component: () => import('./components/Chai.vue'),
        meta: { requiresAuth: true },
        children: [
          {
            path: '/lianhe',
            name: '联合',
            component: () => import('./Chai_components/lianhe.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/shetuan',
            name: '社团',
            component: () => import('./Chai_components/SheTuan.vue'),
            meta: { requiresAuth: true }
          },
          {
            path: '/activity',
            name: "活动",
            component: () => import('./Chai_components/activity.vue'),
            meta: { requiresAuth: true }
          }
        ]
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

// 创建路由
const router = createRouter({
  history: createWebHashHistory(),
  routes
})



router.beforeEach(async (to, from, next) => {
  const userStore = useUserStore()
  
  // 确保用户状态已初始化
  if (!userStore.isInitialized) {
    await userStore.initialize()
  }
  
  // 检查是否需要认证
  if (to.meta.requiresAuth && !userStore.isAuthenticated) {
    next({
      path: '/login.html',
      query: { redirect: to.fullPath }
    })
    return
  }


  // 特殊处理：对于 BYD-join 路由，检查 byd 属性
  if (to.path === '/BYD-join') {
    const userByd = userStore.user?.byd ?? 0;
    if (userByd !== 1) {
      alert('只有参加了BYD企划的成员可以进入该页面');
      next('/'); // 跳转到首页
      return;
    }
  }
  // 检查进入某些页面是否需要特定 rank
  if (to.meta.requiredRank !== undefined) {
    const userRank = userStore.user?.rank ?? 0;
    if (userRank !== to.meta.requiredRank) {
      alert('只有管理账号才有资格进入该页面');
      next('/'); // 跳首页，你也可以改成其他页面
      return;
    }
  }


  // 检查页面是否有 requireLevel 限制
if (to.meta.requireLevel !== undefined) {
  const userLevel = userStore.user?.level ?? 0;
  if (userLevel < to.meta.requireLevel) {
    alert(`该资源板块需要等级 Lv.${to.meta.requireLevel} 以上才能访问`);
    next('/'); // 跳转到首页或其他页面
    return;
  }
}


  next()
})

// 创建Vue应用
const app = createApp(TCHY)
const pinia = createPinia()
app.use(pinia)
app.use(router)
app.use(ElementPlus)
// 初始化用户状态
const userStore = useUserStore()
await userStore.initialize()

// 检查用户是否已登录
if (!userStore.isAuthenticated) {
  router.push('/login.html')
} else {
  // 检查令牌有效性
  const isValid = await userStore.checkTokenValidity()
  if (!isValid) {
    alert('会话已过期，请重新登录')
    router.push('/login.html')
  } else {
    // 挂载应用
    app.mount('#tchy-app')
    
    // 检查是否有重定向路径
    const redirect = router.currentRoute.value.query.redirect
    if (redirect) {
      router.push(redirect)
    }
  }
}