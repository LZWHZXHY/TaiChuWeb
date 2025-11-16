// src/utils/api.js
import axios from 'axios'

// 根据环境获取API基础URL
const getBaseURL = () => {
  const env = import.meta.env.VITE_APP_ENV
  let baseURL = import.meta.env.VITE_API_BASE_URL
  
  // 确保baseURL以/结尾
  if (baseURL && !baseURL.endsWith('/')) {
    baseURL += '/'
  }
  
  console.log(`当前环境: ${env}, API地址: ${baseURL}`)
  
  return baseURL
}

// 创建 axios 实例
const apiClient = axios.create({
  baseURL: getBaseURL(),
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  },
  withCredentials: true
})

// 获取认证token
const getAuthToken = () => {
  return localStorage.getItem('auth_token')
}

// 检查token是否有效
const isTokenValid = (token) => {
  if (!token) return false
  
  try {
    // 解析JWT token
    const payload = JSON.parse(atob(token.split('.')[1]))
    const currentTime = Math.floor(Date.now() / 1000)
    return payload.exp > currentTime
  } catch (error) {
    console.error('Token解析失败:', error)
    return false
  }
}

// 清除无效的token
const clearInvalidToken = () => {
  localStorage.removeItem('auth_token')
  // 触发全局事件，通知其他组件用户已登出
  window.dispatchEvent(new CustomEvent('unauthorized'))
}

// 请求拦截器 - 自动添加token和错误处理
apiClient.interceptors.request.use(
  (config) => {
    // 开发环境日志
    if (import.meta.env.VITE_APP_ENV === 'development') {
      console.log(`🚀 发送请求: ${config.method?.toUpperCase()} ${config.url}`)
    }
    
    // 自动添加认证token
    const token = getAuthToken()
    if (token) {
      // 简化token检查，让后端验证
      config.headers.Authorization = `Bearer ${token}`
      console.log('✅ Token已添加到请求头')
    }
    
    return config
  },
  (error) => {
    console.error('请求配置错误:', error)
    return Promise.reject(error)
  }
)

// 响应拦截器 - 统一错误处理
apiClient.interceptors.response.use(
  (response) => {
    // 开发环境日志
    if (import.meta.env.VITE_APP_ENV === 'development') {
      console.log(`✅ 收到响应: ${response.status} ${response.config.url}`)
    }
    
    return response
  },
  (error) => {
    const { config, response, message, code } = error
    
    console.error('API请求错误详情:', {
      url: config?.url,
      method: config?.method,
      status: response?.status,
      statusText: response?.statusText,
      message: message,
      code: code
    })
    
    // 统一错误处理
    if (response) {
      switch (response.status) {
        case 401:
          console.error('❌ 认证失败 (401): Token无效或已过期')
          clearInvalidToken()
          break
          
        case 403:
          console.error('❌ 权限不足 (403): 没有访问该资源的权限')
          break
          
        case 404:
          console.error('❌ 接口不存在 (404): 请检查API路径是否正确')
          break
          
        case 500:
          console.error('❌ 服务器内部错误 (500): 请检查后端服务状态')
          break
          
        default:
          console.error(`❌ HTTP错误 (${response.status}): ${message}`)
      }
    } else if (code === 'ECONNABORTED' || message.includes('timeout')) {
      console.error('❌ 请求超时: 请检查网络连接或服务器状态')
    } else if (message.includes('Network Error') || message.includes('CORS')) {
      console.error('❌ 网络/CORS错误: 请检查网络连接和后端CORS配置')
    } else {
      console.error('❌ 未知错误:', message)
    }
    
    // 创建更友好的错误消息
    const friendlyMessage = getFriendlyErrorMessage(error)
    const enhancedError = new Error(friendlyMessage)
    enhancedError.originalError = error
    enhancedError.config = config
    enhancedError.response = response
    enhancedError.isNetworkError = !response
    enhancedError.isCorsError = message.includes('CORS')
    
    return Promise.reject(enhancedError)
  }
)

// 获取友好的错误消息
const getFriendlyErrorMessage = (error) => {
  const { response, message, code } = error
  
  if (response) {
    switch (response.status) {
      case 401:
        return '登录已过期，请重新登录'
      case 403:
        return '权限不足，无法访问该资源'
      case 404:
        if (response.config?.url?.includes('/api/')) {
          return '请求的接口不存在，请检查API路径'
        }
        return '请求的资源不存在'
      case 500:
        return '服务器内部错误，请稍后重试'
      default:
        return `请求失败 (${response.status})`
    }
  } else if (code === 'ECONNABORTED' || message.includes('timeout')) {
    return '请求超时，请检查网络连接'
  } else if (message.includes('Network Error')) {
    return '网络连接失败，请检查API服务是否运行'
  } else if (message.includes('CORS')) {
    return '跨域请求被阻止，请检查CORS配置'
  } else {
    return '请求失败，请稍后重试'
  }
}

// 添加全局事件监听器
if (typeof window !== 'undefined') {
  window.addEventListener('unauthorized', () => {
    console.log('🔔 接收到未授权事件，执行全局登出逻辑')
    
    // 清除本地存储
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user')
    
    // 重定向到登录页面
    if (window.location.pathname !== '/login') {
      const currentPath = window.location.pathname + window.location.search
      window.location.href = `/login?redirect=${encodeURIComponent(currentPath)}`
    }
  })
}

// 添加网络状态检查函数
const checkNetworkStatus = async () => {
  try {
    const response = await fetch('/favicon.ico', { method: 'HEAD' })
    return response.ok
  } catch {
    return false
  }
}

// 添加API健康检查函数
const checkApiHealth = async () => {
  try {
    const response = await apiClient.get('/health')
    return {
      healthy: true,
      data: response.data
    }
  } catch (error) {
    return {
      healthy: false,
      error: error.message
    }
  }
}

// 添加重试机制
const createRetryableRequest = (requestFn, maxRetries = 3, delay = 1000) => {
  return async (...args) => {
    let lastError
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        return await requestFn(...args)
      } catch (error) {
        lastError = error
        console.warn(`请求失败，第${attempt}次重试...`)
        
        if (attempt < maxRetries) {
          await new Promise(resolve => setTimeout(resolve, delay * attempt))
        }
      }
    }
    throw lastError
  }
}

// 统一导出所有工具函数
export { 
  getAuthToken, 
  isTokenValid, 
  clearInvalidToken,
  checkNetworkStatus,
  checkApiHealth,
  createRetryableRequest
}

export default apiClient