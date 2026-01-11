import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(null)
  const isAuthenticated = ref(false)

  // 清理认证状态的方法
  const clearAuthState = () => {
    user.value = null
    token.value = null
    isAuthenticated.value = false
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
    console.log('✅ 认证状态已清除')
  }

  // 简单的认证状态检查
  const checkAuth = () => {
    console.log('🔐 检查认证状态...')
    
    const storedToken = localStorage.getItem('auth_token')
    const storedUser = localStorage.getItem('user')
    
    console.log('  - Token 存在:', !!storedToken)
    console.log('  - User 存在:', !!storedUser)
    
    if (!storedToken || !storedUser) {
      console.log('❌ Token 或 User 不存在')
      clearAuthState() // 直接调用 clearAuthState 而不是 this.logout
      return false
    }

    try {
      // 解析用户信息
      const userData = JSON.parse(storedUser)
      user.value = userData
      token.value = storedToken
      isAuthenticated.value = true
      
      console.log('✅ 认证状态检查成功')
      console.log('👤 用户:', userData.username)
      
      return true
    } catch (error) {
      console.error('❌ 认证状态检查失败:', error)
      clearAuthState() // 直接调用 clearAuthState
      return false
    }
  }

  // 登录方法
  const login = async (credentials) => {
    try {
      console.log('🔐 开始登录:', { username: credentials.username })
      
      const response = await apiClient.post('/loginregister/login', {
        usernameOrEmail: credentials.username,
        password: credentials.password
      })

      console.log('📨 登录响应:', response.data)

      if (response.data.success) {
        const authToken = response.data.token
        const userData = {
          id: response.data.userId,
          username: response.data.username,
          creater: response.data.creater
        }

        // 更新状态
        user.value = userData
        token.value = authToken
        isAuthenticated.value = true
        
        // 存储到 localStorage
        localStorage.setItem('user', JSON.stringify(userData))
        localStorage.setItem('auth_token', authToken)
        
        console.log('✅ 登录成功，状态已更新')
        console.log('👤 用户:', userData.username)
        
        return { success: true, user: userData }
      } else {
        console.error('❌ 登录失败:', response.data.error)
        return { success: false, error: response.data.error }
      }
    } catch (error) {
      console.error('❌ 登录请求失败:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '登录失败' 
      }
    }
  }

  // 登出方法
  const logout = () => {
    clearAuthState()
    console.log('✅ 用户已登出')
  }

  // 发送验证码方法
  const sendVerificationCode = async (email) => {
    try {
      const response = await apiClient.post('/loginregister/send-verification-code', {
        email: email
      })
      
      if (response.data.success) {
        return { success: true, message: response.data.message || '验证码发送成功' }
      } else {
        return { success: false, error: response.data.error }
      }
    } catch (error) {
      console.error('❌ 发送验证码错误:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '发送验证码失败' 
      }
    }
  }

  // 注册方法
  const register = async (userData) => {
    try {
      const response = await apiClient.post('/loginregister/register', {
        username: userData.username,
        email: userData.email,
        password: userData.password,
        verificationCode: userData.verificationCode
      })

      if (response.data.success) {
        return { 
          success: true, 
          message: '注册成功',
          username: response.data.username
        }
      } else {
        return { success: false, error: response.data.error }
      }
    } catch (error) {
      console.error('❌ 注册错误:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '注册失败' 
      }
    }
  }

  return {
    user,
    token,
    isAuthenticated,
    login,
    logout,
    checkAuth,
    sendVerificationCode,
    register
  }
})