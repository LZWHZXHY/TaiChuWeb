// src/stores/auth.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/utils/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('auth_token'))
  const isAuthenticated = ref(!!token.value)

  // 登录
  const login = async (credentials) => {
    try {
      const response = await apiClient.post('/auth/login', credentials)
      const { token: authToken, user: userData } = response.data
      
      // 保存token和用户信息
      token.value = authToken
      user.value = userData
      isAuthenticated.value = true
      
      // 存储到localStorage
      localStorage.setItem('auth_token', authToken)
      localStorage.setItem('user', JSON.stringify(userData))
      
      return { success: true, user: userData }
    } catch (error) {
      const errorMessage = error.response?.data?.error || '登录失败'
      return { success: false, error: errorMessage }
    }
  }

  // 注册
  const register = async (userData) => {
    try {
      const response = await apiClient.post('/auth/register', userData)
      const { token: authToken, user: newUser } = response.data
      
      // 保存token和用户信息
      token.value = authToken
      user.value = newUser
      isAuthenticated.value = true
      
      localStorage.setItem('auth_token', authToken)
      localStorage.setItem('user', JSON.stringify(newUser))
      
      return { success: true, user: newUser }
    } catch (error) {
      const errorMessage = error.response?.data?.error || '注册失败'
      return { success: false, error: errorMessage }
    }
  }

  // 登出
  const logout = async () => {
    try {
      await apiClient.post('/auth/logout')
    } catch (error) {
      console.error('登出失败:', error)
    } finally {
      // 清除本地存储
      token.value = null
      user.value = null
      isAuthenticated.value = false
      localStorage.removeItem('auth_token')
      localStorage.removeItem('user')
    }
  }

  // 检查认证状态
  const checkAuth = async () => {
    if (!token.value) return false
    
    try {
      const response = await apiClient.get('/auth/me')
      user.value = response.data.user
      return true
    } catch (error) {
      // token无效，清除本地存储
      logout()
      return false
    }
  }

  return {
    user,
    token,
    isAuthenticated,
    login,
    register,
    logout,
    checkAuth
  }
})