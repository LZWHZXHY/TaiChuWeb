import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home', // 路由名称保持Home或改为MainPush都可以，这里由MainPush接管首页
    component: () => import('@/views/MainPush.vue'), // 【修改】这里现在指向推送大厅
    meta: { 
      title: '推送大厅', // 【修改】标题改为推送大厅
      public: true
    }
  },
  {
    path: '/Intro', // 【可选】如果你还想保留原来的首页，可以通过这个链接访问
    name: '介绍',
    component: () => import('@/views/Home.vue'),
    meta: { 
      title: '太初寰宇 - 介绍',
      public: true
    }
  },
  // 🔥 【新增】灵脉工作空间 (Notion + Obsidian 生产力工具)
  {
    path: '/lingmai',
    name: '灵脉空间',
    // 🔥 核心修改：直接指向集成模块，不再需要 NoteLayout
    component: () => import('@/LingMaiComponents/LingMaiModule.vue'),
    meta: { 
      requiresAuth: true, 
      title: '灵脉工作空间' 
    }
    // 🔥 删除 children：因为模块内部现在是通过变量 (currentId) 切换，而不是通过 URL 路由切换
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
    path: '/operators',
    name: 'Operators',
    // 假设你把新页面放在 views/Operators 目录下
    component: () => import('@/views/OperatorList.vue'), 
    meta: { 
      requiresAuth: false, // 如果你希望游客也能看到社区人数和名录，设为 false
      title: '操作员名录' 
    }
  },
  {
    path:'/Resource',
    name:'资源大厅',
    component:()=>import('@/views/Resource.vue'),
    meta:{
      requiresAuth:false,
      title:'作品大厅'
    }
  },
  {
    path:'/MainPush',
    name:'推送大厅',
    component:()=>import('@/views/MainPush.vue'),
    meta:{
      requiresAuth:false,
      title:'推送大厅'
    }
  },
  {
    path:'/MissionCenter',
    name:'任务中心',
    component:()=>import('@/views/MissionCenter.vue'),
    meta:{
      requiresAuth:true,
      title:'任务中心'
    }
  },
  {
    path:'/RankCenter',
    name:'排行榜大厅',
    component:()=>import('@/views/RankCenter.vue'),
    meta:{
      requiresAuth:true,
      title:'排行榜大厅'
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
    path:'/ExperimentalArea',
    name:'实验区',
    component: () => import('@/views/ExperimentalArea.vue'),
    meta: { 
      requiresAuth: true,
      title: '实验区'
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
  // 必须放在 /workspace 后面
  path: '/projects/:id', 
  name: 'ProjectDetail',
  component: () => import('@/views/ProjectDetail.vue'),
  meta: { requiresAuth: true }
},
{
  path: '/workspace',
  name: '项目管理',
  component: () => import('@/views/ProjectManager.vue'), 
  meta: { 
    requiresAuth: true, 
    title: '协作工作台 // WORKSPACE' 
  }
},
  {
  path: '/creative-center',
  name: 'CreativeCenter',
  component: () => import('@/views/CreativeCenter.vue'),
  meta: { requiresAuth: true } // 记得路由守卫
},
  {
    path: '/blog/:id',  // :id 是动态参数
    name: 'BlogDetail',
    component: () => import('@/BlogComponents/BlogDetailPage.vue'), 
    meta: { 
      requiresAuth: false, // 通常博客是公开可见的
      title: '太初寰宇 // 博客详情'
    },
    props: true // 开启 props 传参，组件内可以直接通过 props.id 获取路由参数
  },
  {
  path: '/post/:id',
  name: 'PostDetail',
  component: () => import('@/PostComponents/PostDetailPage.vue'), // 新建一个独立的页面组件
  meta: { 
      requiresAuth: false,
      title: '太初网络 // 数据节点详情'
    },
    props: true
  },
  {
    path: '/gallery/:id',
    name: 'ArtWorkDetail',
    component: () => import('@/ArtCenter/Components/ArtWorkDetailPage.vue'), // 路径根据你的项目调整
    meta: { 
      requiresAuth: false,
      title: '太初寰宇画廊'
    },
    props: true
  },
  {
  path: '/joint/:id',
  name: 'JointDetail',
  component: () => import('@/views/JointDetail.vue'), // 你的新组件路径
  props: true
},
  {
    path: "/profile/MEE",
    name: "my-new-profile",
    component: () => import("@/UserComponent/Profile/NewProfile.vue"),
    meta: { requiresAuth: true, title: '我的资料 · 新' }
  },
    {
    path: "/profile/NewSettings",
    name: "my-new-settings",
    component: () => import("@/UserComponent/UserSettings/UserSettings.vue"),
    meta: { requiresAuth: true, title: '用户资料设置 · 新' }
  },
  {
    path: "/profile/:userId",
    name: "profile",
    component: () => import("@/UserComponent/Profile/NewProfile.vue"),
    meta: { requiresAuth: true, title: '用户资料' }
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
// src/router/app.js (或 index.js)

router.beforeEach(async (to, from, next) => {
  console.log('🛣️ 路由导航:', from.path, '->', to.path)
  console.log('📋 路由元信息:', to.meta)
  
  // 设置页面标题
  if (to.meta.title) {
    document.title = `${to.meta.title} - 太初寰宇`
  }

  try {
    await Promise.all([initAuthStore(), initApiClient()])

    // 1. 获取两边的状态
    const isLocalStorageReady = checkAuthStatus() // 本地缓存是否有 Token + User
    const isPiniaReady = authStore && authStore.isAuthenticated // 内存是否已认证

    console.log(`🔐 状态检查 | LocalStorage: ${isLocalStorageReady} | Pinia: ${isPiniaReady}`)

    // 🔥🔥🔥【核心修复开始】🔥🔥🔥
    // 逻辑：不再单纯因为两边不一致就登出，而是尝试互补
    
    if (isPiniaReady && !isLocalStorageReady) {
      // 场景：刚登录成功 (Pinia 有数据)，但 LocalStorage 还没来得及写入或丢失
      // 动作：信任 Pinia，补写到 LocalStorage，而不是踢出用户
      console.warn('⚠️ 检测到 Pinia 在线但 LocalStorage 缺失，正在修复缓存...')
      if (authStore.user && authStore.token) {
        localStorage.setItem('auth_token', authStore.token)
        localStorage.setItem('user', JSON.stringify(authStore.user))
        console.log('✅ [缓存修复] 已将 Pinia 状态同步至 LocalStorage')
      }
    } 
    else if (!isPiniaReady && isLocalStorageReady) {
      // 场景：用户刷新页面 (Pinia 被清空)，但 LocalStorage 还在
      // 动作：从 LocalStorage 恢复到 Pinia
      try {
        const userData = JSON.parse(localStorage.getItem('user'))
        const token = localStorage.getItem('auth_token')
        if (userData && token) {
          authStore.user = userData
          authStore.token = token
          authStore.isAuthenticated = true
          console.log('🔄 [刷新恢复] 已从 LocalStorage 恢复用户状态')
        }
      } catch (error) {
        console.error('❌ 恢复状态失败，清除异常缓存', error)
        authStore.logout?.()
      }
    }
    // 🔥🔥🔥【核心修复结束】🔥🔥🔥

    // 重新计算最终认证状态 (只要有一边是通的，就算已登录)
    const finalIsLoggedIn = checkAuthStatus() || (authStore && authStore.isAuthenticated)

    // 需要登录的页面
    if (to.meta.requiresAuth && !finalIsLoggedIn) {
      console.log('🔐 需要登录，重定向到登录页')
      next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
      return
    }

    // 游客专用页面 (如登录页)
    if (to.meta.requiresGuest && finalIsLoggedIn) {
      next('/')
      return
    }

    // --- 下面是 Rank 和 Level 的校验逻辑，保持不变 ---
    
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
          next({ path: '/login', query: { redirect: to.fullPath } })
          return
        }
        next({ path: '/', query: { noAccess: 1 } })
        return
      }
    }

    // 如果目标路由声明了最小 Level，实时向后端校验
    if (to.meta.minLevel != null) {
      try {
        const ok = await authorizeByLevel(Number(to.meta.minLevel))
        if (!ok) {
          console.warn('⛔ 等级不足，禁止访问')
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
          next({ path: '/login', query: { redirect: to.fullPath } })
          return
        }
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