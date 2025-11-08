// server/UserInfoApi.js
import axios from 'axios'
import { defineStore } from 'pinia'
import { ElMessage, ElLoading } from 'element-plus'

export const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";

const request = axios.create({
  baseURL: baseApiUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 获取token的辅助函数
export const getAuthToken = () => {
  return localStorage.getItem('authToken')
}

// 从token中解析用户ID
export const getUserIdFromToken = () => {
  const token = getAuthToken()
  if (!token) return null

  try {
    if (!token.includes('.') || token.split('.').length !== 3) {
      return null
    }

    const payloadPart = token.split('.')[1]
    const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=')
    const decodedPayload = atob(padded)
    const payload = JSON.parse(decodedPayload)
    
    // 关键：使用正确的字段名
    const userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    
    console.log('从token解析的用户ID:', userId) // 应该是 "82"
    return userId
    
  } catch (error) {
    console.error('解析token失败:', error)
    return null
  }
}


// 请求拦截器 - 自动添加token
request.interceptors.request.use(
  (config) => {
    const token = getAuthToken()   //使用拿到的Token
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器 - 处理token过期
request.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      ElMessage.error('登录已过期，请重新登录')
      localStorage.removeItem('authToken')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// ==================== 用户数据API封装 ====================
// 把原来的 getCurrentUserData 替换为：
export const getCurrentUserData = async () => {
  try {
    const response = await request.get('/api/User/me')  // 不再传 userId
    return response.data
  } catch (error) {
    if (error.response?.status === 401) {
      throw new Error('未登录或登录已过期')
    }
    if (error.response?.status === 404) {
      throw new Error('用户数据不存在')
    }
    throw error
  }
}


// 兼容的 id 生成器：优先使用浏览器的 crypto.randomUUID，回退到简单实现
const generateId = () => {
  try {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID()
    }
  } catch (e) {
    // ignore
  }
  // 回退：时间戳 + 随机字符串，低冲突概率，可作为幂等 key
  return Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 9)
}

export const deductPoints = async (amount, opts = {}) => {
  if (!Number.isInteger(amount) || amount <= 0) {
    throw new Error('amount 必须为大于 0 的整数')
  }

  const idempotencyKey = opts.idempotencyKey || generateId()

  const url = `/api/User/deduct?amount=${encodeURIComponent(amount)}`

  const headers = {
    'Idempotency-Key': idempotencyKey
  }

  try {
    const resp = await request.post(url, null, { headers })
    return resp.data
  } catch (error) {
    // 统一处理常见的业务状态码并抛出更友好的错误
    if (error && error.response) {
      const status = error.response.status
      const data = error.response.data

      if (status === 401) {
        throw new Error('未登录或登录已过期')
      }
      if (status === 400) {
        // 业务校验失败（例如积分不足）
        const msg = data?.message || '请求参数错误'
        throw new Error(msg)
      }
      if (status === 409) {
        throw new Error(data?.message || '并发冲突，请重试')
      }
      // 其他后端错误，抛出原始 message 或 statusText
      throw new Error(data?.message || error.response.statusText || '扣分失败')
    }
    // 网络或其他错误
    throw error
  }
}



// 导出request实例
export default request