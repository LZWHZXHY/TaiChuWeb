import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/Home.vue'),
    meta: { 
      title: '首页',
      public: true
    }
  },
  {
    path: '/404',
    name: '404',
    component: () => import('@/views/Forbidden.vue'),
    meta: { 
      title: '404',
      public: true
    }
  },
  {
    path: '/login',
    name: '登录',
    component: () => import('@/LoginRegister/Login.vue'),
    meta: { 
      requiresGuest: true,
      title: '用户登录',
      public: true
    }
  },{
    path:'/trade',
    name:'交易站',
    component:()=>import('@/views/TradeStation.vue'),
    meta: { 
      requiresAuth: true,
      title: '交易站'
    }
  },
  {
    path:'/TCHYproduct',
    name:'太初寰宇作品',
    component:()=>import('@/views/TCHYproduct.vue'),
    meta: { 
      requiresAuth: true,
      title: '太初寰宇'
    }
  },
  {
    path: '/register',
    name: '注册',
    component: () => import('@/LoginRegister/Register.vue'),
    meta: { 
      requiresGuest: true,
      title: '用户注册',
      public: true
    }
  },
  {
    path:'/forgetPassword',
    name:'密码找回',
    component: () => import('@/LoginRegister/ForgetPassword.vue'),
    meta: { 
      requiresGuest: true,
      title: '密码找回',
      public: true
    }
  },
  {
    path:'/admin',
    name:'管理员页面',
    component: () => import('@/views/Admin.vue'),
    meta: { 
      requiresAuth: true,
      title: '管理员页面',
      minRank: 1 // 需要后端校验 rank >= 1
    }
  },
  {
    path:'/DataCenter',
    name:'交流中枢',
    component:()=>import('@/views/ComCenter.vue'),
    meta:{
      requiresAuth:true,
      title:'交流中枢'
    }
  },
  {
    path:'/WorkCenter',
    name:'艺术大厅',
    component:()=>import('@/views/WorkCenter.vue'),
    meta:{
      requiresAuth:true,
      title:'作品大厅'
    }
  },
  {
    path:'/blogCreater',
    name:'博客创作',
    component:()=>import('@/BlogComponents/BlogCreater.vue'),
    meta:{
      requiresAuth:true,
      title:'博客创作页面'
    }
  },
  {
    path:'/EntertainmentArea',
    name:'娱乐区',
    component: () => import('@/views/EntertainmentArea.vue'),
    meta: { 
      requiresAuth: true,
      title: '娱乐区'
    }
  },
  {
    path:'/suggest',
    name:'意见箱',
    component: () => import('@/feedbackComponents/FeedbackBox.vue'),
    meta: { 
      requiresAuth: true,
      title: '意见箱'
    }
  },
  {
    path: "/profile/me",
    name: "my-profile",
    component: () => import("@/userComponents/profile.vue"),
    meta: { requiresAuth: true, title: '我的资料' }
  },
  {
    path: "/profile/MEE",
    name: "my-new-profile",
    component: () => import("@/UserComponent/Profile/NewProfile.vue"),
    meta: { requiresAuth: true, title: '我的资料 · 新' }
  },
  {
    path: "/profile/:userId",
    name: "profile",
    component: () => import("@/userComponents/profile.vue"),
    meta: { requiresAuth: true, title: '用户资料' }
  },
  // 可选：覆盖 /profile 自动跳转到/profile/me
  {
    path: "/profile",
    redirect: "/profile/me"
  },
  {
  path: "/profile/Usersettings", // 建议使用固定路径
  name: "UserSettings",      // 设置路由名称，方便跳转
  component: () => import("@/UserComponent/UserSettings/UserSettings.vue"),
  meta: { requiresAuth: true, title: '用户资料设置' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

let authStore = null
let apiClient = null

const initAuthStore = async () => {
  if (!authStore) {
    try {
      const { useAuthStore } = await import('@/utils/auth')
      authStore = useAuthStore()
      console.log('✅ AuthStore 初始化完成')
    } catch (error) {
      console.error('❌ AuthStore 初始化失败:', error)
    }
  }
  return authStore
}

const initApiClient = async () => {
  if (!apiClient) {
    try {
      const mod = await import('@/utils/api')
      apiClient = mod.default || mod.apiClient || mod
      console.log('✅ apiClient 初始化完成')
    } catch (error) {
      console.error('❌ apiClient 初始化失败:', error)
    }
  }
  return apiClient
}

const checkAuthStatus = () => {
  const token = localStorage.getItem('auth_token')
  const user = localStorage.getItem('user')
  console.log('🔐 快速认证检查:')
  console.log('  - Token 存在:', !!token)
  console.log('  - User 存在:', !!user)
  return !!(token && user)
}

// 每次进入需要 rank 的页面，实时向后端校验
const authorizeByRank = async (minRank) => {
  if (!apiClient) throw new Error('apiClient未就绪')
  try {
    const resp = await apiClient.get('/Userinfo/authorize', { params: { minRank } })
    const allowed = resp?.data?.allowed === true
    console.log('🔎 Rank 实时校验结果:', { required: minRank, allowed, data: resp?.data })
    return allowed
  } catch (err) {
    const status = err?.response?.status
    console.warn('⚠️ Rank 校验失败:', status, err?.response?.data)
    if (status === 401) {
      // 未授权（token 失效/未登录）
      throw new Error('unauthorized')
    }
    if (status === 403) {
      // 没权限
      return false
    }
    // 其他错误按无权限处理
    return false
  }
}

// 🔥 新增：每次进入需要 level 的页面，实时向后端校验
const authorizeByLevel = async (minLevel) => {
  if (!apiClient) throw new Error('apiClient未就绪')
  try {
    const resp = await apiClient.get('/Userinfo/level')
    const userLevel = resp?.data || 0
    const allowed = userLevel >= minLevel
    console.log('🔎 Level 实时校验结果:', { 
      required: minLevel, 
      currentLevel: userLevel, 
      allowed 
    })
    return allowed
  } catch (err) {
    const status = err?.response?.status
    console.warn('⚠️ Level 校验失败:', status, err?.response?.data)
    if (status === 401) {
      // 未授权（token 失效/未登录）
      throw new Error('unauthorized')
    }
    // 其他错误按无权限处理
    return false
  }
}

// 路由守卫
router.beforeEach(async (to, from, next) => {
  console.log('🛣️ 路由导航:', from.path, '->', to.path)
  console.log('📋 路由元信息:', to.meta)
  
  // 设置页面标题
  if (to.meta.title) {
    document.title = `${to.meta.title} - 太初寰宇`
  }

  try {
    await Promise.all([initAuthStore(), initApiClient()])

    const isLoggedIn = checkAuthStatus()
    console.log('🔐 认证状态:', isLoggedIn)

    // 同步 Pinia 与本地缓存
    if (authStore && isLoggedIn !== authStore.isAuthenticated) {
      if (isLoggedIn) {
        try {
          const userData = JSON.parse(localStorage.getItem('user'))
          const token = localStorage.getItem('auth_token')
          authStore.user = userData
          authStore.token = token
          authStore.isAuthenticated = true
          console.log('🔄 已同步认证状态到 Pinia')
        } catch (error) {
          console.error('❌ 同步认证状态失败:', error)
        }
      } else {
        authStore.logout?.()
        console.log('🔄 已清除 Pinia 认证状态')
      }
    }

    // 需要登录
    if (to.meta.requiresAuth && !isLoggedIn) {
      console.log('🔐 需要登录，重定向到登录页')
      next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
      return
    }

    // 游客专用
    if (to.meta.requiresGuest && isLoggedIn) {
      next('/')
      return
    }

    // 如果目标路由声明了最小 Rank，实时向后端校验
    if (to.meta.minRank != null) {
      try {
        const ok = await authorizeByRank(Number(to.meta.minRank))
        if (!ok) {
          console.warn('⛔ Rank 不足，禁止访问')
          next({ path: '/404', query: { noAccess: 1 } })
          return
        }
      } catch (e) {
        if (e?.message === 'unauthorized') {
          // Token 失效或未登录
          next({ path: '/login', query: { redirect: to.fullPath } })
          return
        }
        // 其他错误：退回首页
        next({ path: '/', query: { noAccess: 1 } })
        return
      }
    }

    // 🔥 新增：如果目标路由声明了最小 Level，实时向后端校验
    if (to.meta.minLevel != null) {
      try {
        const ok = await authorizeByLevel(Number(to.meta.minLevel))
        if (!ok) {
          console.warn('⛔ 等级不足，禁止访问意见箱')
          next({ 
            path: '/404', 
            query: { 
              noAccess: 1,
              message: `需要等级 ${to.meta.minLevel} 以上才能使用意见箱` 
            } 
          })
          return
        }
      } catch (e) {
        if (e?.message === 'unauthorized') {
          // Token 失效或未登录
          next({ path: '/login', query: { redirect: to.fullPath } })
          return
        }
        // 其他错误：退回首页
        next({ path: '/', query: { noAccess: 1 } })
        return
      }
    }

    next()
  } catch (error) {
    console.error('❌ 路由守卫出错:', error)
    if (to.meta.requiresAuth) {
      next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
    } else {
      next()
    }
  }
})

router.afterEach(() => {
  window.scrollTo(0, 0)
})

router.onError((error) => {
  console.error('❌ 路由错误:', error)
})



export default router