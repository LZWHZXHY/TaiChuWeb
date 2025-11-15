// src/utils/auth.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('auth_token'))
  const isAuthenticated = ref(!!token.value)

  // 初始化时从localStorage恢复用户信息
  const initUserFromStorage = () => {
    const storedUser = localStorage.getItem('user')
    if (storedUser) {
      try {
        user.value = JSON.parse(storedUser)
        isAuthenticated.value = true
      } catch (error) {
        console.error('解析存储的用户信息失败:', error)
        clearAuthState()
      }
    }
  }

  // 立即初始化
  initUserFromStorage()

  // 清理认证状态
  const clearAuthState = () => {
    user.value = null
    token.value = null
    isAuthenticated.value = false
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
  }

  // 登录方法
  const login = async (credentials) => {
    try {
      const response = await apiClient.post('/loginregister/login', {
        usernameOrEmail: credentials.username,
        password: credentials.password
      })

      if (response.data.success) {
        const userData = {
          id: response.data.userId,
          username: response.data.username
        }
        
        // 更新状态
        user.value = userData
        token.value = response.data.token
        isAuthenticated.value = true
        
        // 存储到localStorage
        localStorage.setItem('user', JSON.stringify(userData))
        localStorage.setItem('auth_token', response.data.token)
        
        return { success: true, user: userData }
      } else {
        return { success: false, error: response.data.error }
      }
    } catch (error) {
      console.error('登录错误:', error)
      return { 
        success: false, 
        error: error.response?.data?.error || '登录失败' 
      }
    }
  }

  // 检查认证状态 - 仅检查本地状态
  const checkAuth = () => {
    // 如果本地有token和用户信息，则认为已认证
    if (token.value && user.value) {
      isAuthenticated.value = true
      return true
    }
    return false
  }

  // 登出方法 - 仅清理本地状态
  const logout = () => {
    clearAuthState()
    console.log('✅ 用户已登出，本地状态已清除')
  }

  return {
    user,
    token,
    isAuthenticated,
    login,
    logout,
    checkAuth
  }
})