import { createRouter, createWebHistory } from 'vue-router'

// ==========================================
// 1. 路由定义 (按功能模块分组，保持清晰)
// ==========================================
const routes = [
  // --- 核心/公共页面 ---
  { path: '/', name: 'Home', component: () => import('@/views/MainPush.vue'), meta: { title: '推送大厅', public: true } },
  { path: '/Intro', name: '介绍', component: () => import('@/views/Home.vue'), meta: { title: '太初寰宇 - 介绍', public: true } },
  { path: '/404', name: '404', component: () => import('@/views/Forbidden.vue'), meta: { title: '404', public: true } },
  
  // --- 登录/注册/找回密码 ---
  { path: '/login', name: '登录', component: () => import('@/LoginRegister/Login.vue'), meta: { requiresGuest: true, title: '用户登录', public: true } },
  { path: '/register', name: '注册', component: () => import('@/LoginRegister/Register.vue'), meta: { requiresGuest: true, title: '用户注册', public: true } },
  { path: '/forgetPassword', name: '密码找回', component: () => import('@/LoginRegister/ForgetPassword.vue'), meta: { requiresGuest: true, title: '密码找回', public: true } },

  // --- 大厅与内容展示 (无需强登录) ---
  { path: '/recommend', name: '推荐', component: () => import('@/views/RecommendationNode.vue'), meta: { title: '推荐' } },
  { path: '/media', name: 'MediaMatrix', component: () => import('@/views/VideoHub.vue'), meta: { title: '影音频段 - 太初寰宇', public: true } },
  { path: '/media/:id', name: 'MediaDetail', component: () => import('@/VideoHubComponents/VideoDetailPage.vue'), meta: { title: '影像解密中...' }, props: true },
  { path: '/Resource', name: '资源大厅', component: () => import('@/views/Resource.vue'), meta: { title: '作品大厅' } },
  { path: '/MainPush', name: '推送大厅', component: () => import('@/views/MainPush.vue'), meta: { title: '推送大厅' } },
  { path: '/BlogCenter', name: '博客中心', component: () => import('@/views/BlogCenter.vue'), meta: { title: '博客中心' } },
  { path: '/PostCenter', name: '帖子中心', component: () => import('@/views/PostCenter.vue'), meta: { title: '帖子中心' } },
  { path: '/wiki/:id?', name: 'Wiki', component: () => import('@/views/WikiPage.vue'), meta: { title: '寰宇百科', public: true } },
  { path: '/ClubCenter', name: 'Club', component: () => import('@/Club/ClubCenter.vue'), meta:{ title: '社团收录', public: true}},
  
  // --- 详情页 (通常公开) ---
  { path: '/blog/:id', name: 'BlogDetail', component: () => import('@/DetailPage/BlogDetailPage.vue'), meta: { title: '太初寰宇 // 博客详情' }, props: true },
  { path: '/post/:id', name: 'PostDetail', component: () => import('@/DetailPage/PostDetailPage.vue'), meta: { title: '太初网络 // 数据节点详情' }, props: true },
  { path: '/gallery/:id', name: 'ArtWorkDetail', component: () => import('@/ArtCenter/Components/ArtWorkDetailPage.vue'), meta: { title: '太初寰宇画廊' }, props: true },
  { path: '/album/:id', name: 'AlbumDetail', component: () => import('@/ArtCenter/Components/AlbumDetailPage.vue'), meta: { title: '画册档案 - 太初寰宇' }, props: true },
  { path: '/activity/:id', name: 'ActivityDetail', component: () => import('@/DetailPage/ActivityDetail.vue'), props: true },
  { path: '/club/:id', name: 'ClubDetail', component: () => import('@/Club/ClubDetail.vue'), props: true },
   { path: '/Market', name:'交易行', component:()=>import('@/Community-market/CommunityMarket.vue'), props:true},

  // --- 需要登录的工作区与交互 ---
  { path: '/lingmai', name: '灵脉空间', component: () => import('@/LingMaiComponents/LingMaiModule.vue'), meta: { requiresAuth: true, title: '灵脉工作空间' } },
  { path: '/notifications', name: 'NotificationCenter', component: () => import('@/views/NotificationCenter.vue'), meta: { requiresAuth: true } },
  { path: '/WorkCenter', name: '艺术大厅', component: () => import('@/views/WorkCenter.vue'), meta: { requiresAuth: true, title: '作品大厅' } },
  { path: '/MissionCenter', name: '任务中心', component: () => import('@/views/MissionCenter.vue'), meta: { requiresAuth: true, title: '任务中心' } },
  { path: '/ChatCenter', name: '太虚坛', component: () => import('@/views/ChatCenter.vue'), meta: { requiresAuth: true, title: '太虚坛' } },
  { path: '/EntertainmentArea', name: '娱乐区', component: () => import('@/views/EntertainmentArea.vue'), meta: { requiresAuth: true, title: '娱乐区' } },
  { path: '/suggest', name: '意见箱', component: () => import('@/feedbackComponents/FeedbackBox.vue'), meta: { requiresAuth: true, title: '意见箱' } },
  { path: '/workspace', name: '项目管理', component: () => import('@/views/ProjectManager.vue'), meta: { requiresAuth: true, title: '协作工作台 // WORKSPACE' } },
  { path: '/projects/:id', name: 'ProjectDetail', component: () => import('@/views/ProjectDetail.vue'), meta: { requiresAuth: true } },
  { path: '/creative-center', name: 'CreativeCenter', component: () => import('@/views/CreativeCenter.vue'), meta: { requiresAuth: true } },
  { path: '/activity-play/:id', name: 'ActivityPlay', component: () => import('@/EnterainmentComponents/TRPG/TRPGPlayPage.vue'), props: true, meta: { requiresAuth: true, hideHeader: true } },
 

  // --- 用户个人资料 ---
  { path: "/profile/MEE", name: "my-new-profile", component: () => import("@/UserComponent/Profile/NewProfile.vue"), meta: { requiresAuth: true, title: '我的资料 · 新' } },
  { path: "/profile/NewSettings", name: "my-new-settings", component: () => import("@/UserComponent/UserSettings/UserSettings.vue"), meta: { requiresAuth: true, title: '用户资料设置 · 新' } },
  { path: "/profile/:userId", name: "profile", component: () => import("@/UserComponent/Profile/NewProfile.vue"), meta: { requiresAuth: true, title: '用户资料' } },

  // --- 管理员页面 ---
  { path: '/admin', name: '管理员页面', component: () => import('@/views/Admin.vue'), meta: { requiresAuth: true, title: '管理员页面', allowedRoles: ['SuperAdmin', 'Admin'] } },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// ==========================================
// 2. 依赖懒加载与状态管理助手函数
// ==========================================
let authStore = null
let apiClient = null

const loadDependencies = async () => {
  if (!authStore) authStore = (await import('@/utils/auth')).useAuthStore()
  if (!apiClient) {
    const mod = await import('@/utils/api')
    apiClient = mod.default || mod.apiClient || mod
  }
  return { authStore, apiClient }
}

/**
 * 修复与同步本地缓存与 Pinia 状态
 */
const syncAuthState = (store) => {
  const token = localStorage.getItem('auth_token')
  const userString = localStorage.getItem('user')
  const isLocalStorageReady = !!(token && userString)
  const isPiniaReady = store.isAuthenticated

  if (isPiniaReady && !isLocalStorageReady) {
    // Pinia 有数据但缓存没有 (可能刚登录但缓存没写进去)
    localStorage.setItem('auth_token', store.token)
    localStorage.setItem('user', JSON.stringify(store.user))
  } else if (!isPiniaReady && isLocalStorageReady) {
    // 缓存有数据但 Pinia 没有 (通常是刷新了页面)
    try {
      store.user = JSON.parse(userString)
      store.token = token
      store.isAuthenticated = true
    } catch (e) {
      store.logout?.()
    }
  }
  return store.isAuthenticated || isLocalStorageReady
}

// ==========================================
// 3. 后端权限校验辅助函数
// ==========================================
const checkBackendPermission = async (url, params = {}) => {
  try {
    const resp = await apiClient.get(url, { params })
    return { allowed: resp?.data?.allowed || resp?.data >= params.minLevel, status: 200 }
  } catch (err) {
    return { allowed: false, status: err?.response?.status || 500 }
  }
}

// ==========================================
// 4. 全局路由守卫 (逻辑展平，早期返回)
// ==========================================
router.beforeEach(async (to, from, next) => {
  // 1. 更新标题
  if (to.meta.title) document.title = `${to.meta.title} - 太初寰宇`

  try {
    // 2. 初始化依赖并同步状态
    const { authStore, apiClient } = await loadDependencies()
    const isLoggedIn = syncAuthState(authStore)

    // 3. 基础页面访问控制
    if (to.meta.requiresAuth && !isLoggedIn) {
      return next({ path: '/login', query: { redirect: to.fullPath } })
    }
    if (to.meta.requiresGuest && isLoggedIn) {
      return next('/')
    }

    // 4. 角色鉴权 (你新增的 allowedRoles 逻辑)
    if (to.meta.allowedRoles && to.meta.allowedRoles.length > 0) {
      // 1. 获取用户的角色代码数组 (确保 HeaderNav 里同步了 roleCodes)
      const userRoleCodes = authStore.user?.roleCodes || []
      
      // 2. 检查用户的角色数组中，是否有任何一个值匹配 allowedRoles 要求的角色
      const hasPermission = to.meta.allowedRoles.some(role => userRoleCodes.includes(role))

      if (!hasPermission) {
        // 额外的逻辑保险：如果 rank 为 1，强制视为拥有 SuperAdmin 权限
        if (authStore.user?.rank === 1) {
          return next() 
        }

        console.warn(`⛔ 权限拒绝。需要: ${to.meta.allowedRoles}, 用户的角色列表:`, userRoleCodes)
        return next({ path: '/404', query: { noAccess: 1 } })
      }
    }

    // 5. 动态 Rank 校验
    if (to.meta.minRank != null) {
      const { allowed, status } = await checkBackendPermission('/Userinfo/authorize', { minRank: Number(to.meta.minRank) })
      if (!allowed) {
        if (status === 401) return next({ path: '/login', query: { redirect: to.fullPath } })
        return next({ path: '/404', query: { noAccess: 1 } })
      }
    }

    // 6. 动态 Level 校验
    if (to.meta.minLevel != null) {
      const { allowed, status } = await checkBackendPermission('/Userinfo/level', { minLevel: Number(to.meta.minLevel) })
      if (!allowed) {
        if (status === 401) return next({ path: '/login', query: { redirect: to.fullPath } })
        return next({ 
          path: '/404', 
          query: { noAccess: 1, message: `需要等级 ${to.meta.minLevel} 以上才能使用该功能` } 
        })
      }
    }

    // 7. 全部通过，放行
    next()

  } catch (error) {
    console.error('❌ 路由守卫出错:', error)
    if (to.meta.requiresAuth) {
      next({ path: '/login', query: { redirect: to.fullPath } })
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