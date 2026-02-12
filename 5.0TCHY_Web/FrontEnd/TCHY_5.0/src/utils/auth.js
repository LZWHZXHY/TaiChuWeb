import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  // --- 1. 状态初始化 ---
  // 尝试读取缓存，避免刷新空白
  const cachedUser = localStorage.getItem('user_cache')
  const user = ref(cachedUser ? JSON.parse(cachedUser) : {})
  
  const token = ref(localStorage.getItem('auth_token') || null)
  const isAuthenticated = ref(!!token.value)
  const userID = ref(user.value?.id || null)

  // --- 2. 核心：同时获取 [资料] 和 [数值] 并合并 ---
  const fetchLatestUser = async () => {
    if (!isAuthenticated.value) return

    try {
      console.log('🔄 [SWR] 正在全量同步用户状态 (资料 + 数值)...')

      // ✨ 关键修改：并行请求两个接口 ✨
      // /detail -> 获取头像、简介、社交链接
      // /me     -> 获取金币、经验、声望
      const [detailRes, statsRes] = await Promise.all([
        apiClient.get('/profile/detail'),
        apiClient.get('/profile/me')
      ])

      if (detailRes.data.success && statsRes.data.success) {
        const d = detailRes.data.data // 资料数据
        const s = statsRes.data.data  // 数值数据 (Stats)

        // 💡 数据合并逻辑
        // 将两个接口的数据拼成一个完整的对象
        const fullUserData = {
          ...user.value, // 保留旧字段防止覆盖
          
          // --- 来自 /profile/detail 的基础信息 ---
          // 注意：后端返回的是 PascalCase (首字母大写)
          username: d.Username,
          avatar: d.Avatar, 
          gender: d.Gender,
          bio: d.Bio,
          interests: d.Interests,
          region: d.Region,
          signature: d.Signature,
          birthDate: d.BirthDate,
          email: d.Email,

          // --- 来自 /profile/me 的 RPG 数值 ---
          level: s.Level,
          title: s.Title,
          coins: s.Points,       // 映射：后端的 Points -> 前端的 coins/gold
          reputation: s.Reputation,
          currentExp: s.CurrentExp,
          nextLevelExp: s.NextLevelExp,
          expPercent: s.ExpPercent
        }

        // 1. 更新 Pinia
        user.value = fullUserData
        // 确保 ID 存在 (detail 接口可能没返回 ID，尽量保留原有的或从 token 解析的)
        if (d.Id) userID.value = d.Id

        // 2. 更新 LocalStorage (持久化)
        localStorage.setItem('user_cache', JSON.stringify(fullUserData))
        localStorage.setItem('user', JSON.stringify(fullUserData)) // ✅ 新增这一行
        
        console.log('✅ [SWR] 用户全量资料已更新:', fullUserData.username)
      }
    } catch (error) {
      console.warn('⚠️ [SWR] 同步失败，部分接口未响应:', error)
      // 即使失败也不清除数据，保证离线体验
    }
  }

  // --- 3. 清理状态 ---
  const clearAuthState = () => {
    user.value = {}
    token.value = null
    isAuthenticated.value = false
    userID.value = null
    
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user') 
    localStorage.removeItem('user_cache')
    console.log('✅ 认证状态已清除')
  }

  // --- 4. 检查认证 (App 启动时) ---
  const checkAuth = () => {
    const storedToken = localStorage.getItem('auth_token')
    if (!storedToken) {
      clearAuthState()
      return false
    }
    
    token.value = storedToken
    isAuthenticated.value = true
    
    // 启动时触发一次静默更新
    fetchLatestUser()
    
    return true
  }

  // --- 5. 登录 ---
  const login = async (credentials) => {
    try {
      const response = await apiClient.post('/loginregister/login', {
        usernameOrEmail: credentials.username,
        password: credentials.password
      })

      if (response.data.success) {
        const authToken = response.data.token
        const userId = response.data.userId

        // 先存一个最基础的壳子，让路由能跳过去
        const basicUser = {
          id: userId,
          username: response.data.username,
          creater: response.data.creater
        }

        token.value = authToken
        user.value = basicUser
        userID.value = userId
        isAuthenticated.value = true
        
        localStorage.setItem('auth_token', authToken)
        localStorage.setItem('user_cache', JSON.stringify(basicUser))
        localStorage.setItem('user', JSON.stringify(basicUser)) // ✅ 新增这一行

        // 🚀 登录成功后，立刻去拉取详细数值！
        await fetchLatestUser()

        return { success: true, user: user.value }
      } else {
        return { success: false, error: response.data.error }
      }
    } catch (error) {
      return { success: false, error: error.response?.data?.error || '登录失败' }
    }
  }

  // --- 6. 登出 ---
  const logout = () => {
    clearAuthState()
  }

  // 注册与验证码逻辑保持不变...
  const sendVerificationCode = async (email) => { /* ... */ }
  const register = async (userData) => { /* ... */ }

  return {
    user,
    token,
    isAuthenticated,
    userID,
    login,
    logout,
    checkAuth,
    sendVerificationCode,
    register,
    fetchLatestUser
  }
})