import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  // --- 1. 状态初始化 ---
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

      const [detailRes, statsRes] = await Promise.all([
        apiClient.get('/profile/detail'),
        apiClient.get('/profile/me')
      ])

      if (detailRes.data.success && statsRes.data.success) {
        const d = detailRes.data.data
        const s = statsRes.data.data

        const fullUserData = {
          ...user.value,
          username: d.Username,
          avatar: d.Avatar, 
          gender: d.Gender,
          bio: d.Bio,
          interests: d.Interests,
          region: d.Region,
          signature: d.Signature,
          birthDate: d.BirthDate,
          email: d.Email,
          level: s.Level,
          title: s.Title,
          coins: s.Points,
          reputation: s.Reputation,
          currentExp: s.CurrentExp,
          nextLevelExp: s.NextLevelExp,
          expPercent: s.ExpPercent
        }

        user.value = fullUserData
        if (d.Id) userID.value = d.Id

        localStorage.setItem('user_cache', JSON.stringify(fullUserData))
        localStorage.setItem('user', JSON.stringify(fullUserData))
        
        console.log('✅ [SWR] 用户全量资料已更新:', fullUserData.username)
      }
    } catch (error) {
      console.warn('⚠️ [SWR] 同步失败，部分接口未响应:', error)
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
        localStorage.setItem('user', JSON.stringify(basicUser))

        await fetchLatestUser()
        return { success: true, user: user.value }
      } else {
        return { success: false, error: response.data.error || '登录失败' }
      }
    } catch (error) {
      return { success: false, error: error.response?.data?.error || '网络连接失败' }
    }
  }

  // --- 6. 登出 ---
  const logout = () => {
    clearAuthState()
  }

  // --- 7. 发送验证码 (✨ 核心修复部分 ✨) ---
  const sendVerificationCode = async (email) => {
    try {
      // 请确保后端的 URL 路径正确，这里假设是 /loginregister/send-code
      const response = await apiClient.post('/loginregister/send-verification-code', { email })
      
      // 这里的逻辑必须保证在 success 为 false 时也返回对象
      if (response.data && response.data.success) {
        return { 
          success: true, 
          message: response.data.message || '验证码已发送到邮箱' 
        }
      } else {
        return { 
          success: false, 
          error: response.data?.error || '服务器拒绝发送验证码' 
        }
      }
    } catch (error) {
      console.error('发送验证码 API 错误:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '无法连接到服务器，请检查网络' 
      }
    }
  }

  // --- 8. 注册 (✨ 核心修复部分 ✨) ---
  const register = async (userData) => {
    try {
      const response = await apiClient.post('/loginregister/register', userData)
      
      if (response.data && response.data.success) {
        return { 
          success: true, 
          username: response.data.username || userData.username 
        }
      } else {
        return { 
          success: false, 
          error: response.data?.error || '注册失败' 
        }
      }
    } catch (error) {
      console.error('注册 API 错误:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '注册请求超时或服务器错误' 
      }
    }
  }

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