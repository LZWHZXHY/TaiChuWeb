// src/utils/api.js
import axios from 'axios'

// 根据环境获取API基础URL
const getBaseURL = () => {
  const env = import.meta.env.VITE_APP_ENV
  const baseURL = import.meta.env.VITE_API_BASE_URL
  
  console.log(`当前环境: ${env}, API地址: ${baseURL}`)
  
  return baseURL
}

// 创建 axios 实例
const apiClient = axios.create({
  baseURL: getBaseURL(),
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器 - 只做基础日志
apiClient.interceptors.request.use(
  (config) => {
    // 开发环境日志
    if (import.meta.env.VITE_APP_ENV === 'development') {
      console.log(`🚀 发送请求: ${config.method?.toUpperCase()} ${config.url}`, config)
    }
    
    return config
  },
  (error) => {
    console.error('请求配置错误:', error)
    return Promise.reject(error)
  }
)

// 响应拦截器 - 只做基础错误处理
apiClient.interceptors.response.use(
  (response) => {
    // 开发环境日志
    if (import.meta.env.VITE_APP_ENV === 'development') {
      console.log(`✅ 收到响应: ${response.status} ${response.config.url}`, response.data)
    }
    
    return response
  },
  (error) => {
    console.error('API请求错误:', {
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status,
      message: error.message
    })
    
    // 基础错误处理
    if (error.response?.status === 404) {
      console.error('接口不存在，请检查后端服务')
    } else if (error.response?.status >= 500) {
      console.error('服务器错误，请检查后端服务状态')
    } else if (!error.response) {
      console.error('网络错误，请检查网络连接和后端服务是否启动')
    }
    
    return Promise.reject(error)
  }
)

export default apiClient