import { defineStore } from 'pinia'
import axios from 'axios'
import api from '../../server/api'



export const useUserStore = defineStore('user', {
  state: () => ({
  user: {
    id: null,
    level: 0,
    exp: 0,
    rank: 0,
    byd:0,
    creater:0,
  },
  token: null,
  isAuthenticated: false,
  isInitialized: false
}),

  actions: {
    async initialize() {
      if (this.isInitialized) return
      
      const token = localStorage.getItem('authToken')
      const user = localStorage.getItem('user')
      
      if (token && user) {
        try {
          // 验证令牌格式
          if (!this.validateToken(token)) {
            console.error('初始化时检测到无效令牌格式')
            this.clearUserData()
            return
          }
          
          this.user = JSON.parse(user)
          this.token = token
          this.isAuthenticated = true
        } catch (e) {
          console.error('解析用户数据失败:', e)
          this.clearUserData()
        }
      }
      
      this.isInitialized = true
    },
    
    // 在 actions 里补充如下方法
    updateUser(userData) {
      this.user = { ...userData }
      localStorage.setItem('user', JSON.stringify(this.user))
    },


    login(userData, authToken) {
      // 验证令牌格式
      if (!this.validateToken(authToken)) {
        console.error('登录时检测到无效令牌格式:', authToken)
        setTimeout(() => {
            console.error('登录时检测到无效令牌格式:', authToken)
          }, 10000)
        return
      }
      
      this.user = userData
      this.token = authToken
      this.isAuthenticated = true
      
      localStorage.setItem('user', JSON.stringify(userData))
      localStorage.setItem('authToken', authToken)
    },
    
    logout() {
      this.clearUserData()
    },
    
    clearUserData() {
      this.user = null
      this.token = null
      this.isAuthenticated = false
      
      localStorage.removeItem('user')
      localStorage.removeItem('authToken')
      localStorage.removeItem('refreshToken')
      sessionStorage.removeItem('refreshToken')
    },
    
    async checkTokenValidity() {
      if (!this.token) return false;
      
      try {
        // 检查令牌格式
        if (!this.token.includes('.') || this.token.split('.').length !== 3) {
          throw new Error('令牌格式无效');
        }
        
        const payloadPart = this.token.split('.')[1];
        // base64url转base64
        const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
        const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
        const decodedPayload = atob(padded);
        const payload = JSON.parse(decodedPayload);
        
        const expiryTime = payload.exp * 1000; // 转换为毫秒
        const currentTime = Date.now();
        
        // 如果令牌过期，尝试刷新
        if (expiryTime < currentTime) {
          return await this.refreshToken();
        }
        
        return true;
      } catch (e) {
        console.error('令牌检查失败:', e);
        return false;
      }
    },
    
    async refreshToken() {
      // 获取刷新令牌
      const refreshToken = localStorage.getItem('refreshToken') || 
                          sessionStorage.getItem('refreshToken');
      
      if (!refreshToken) return false;
      
      try {
        const response = await api.post('/api/auth/refresh-token', {
          refreshToken
        })
        
        // 验证新令牌格式
        if (!response.data.accessToken || !response.data.accessToken.includes('.')) {
          throw new Error('无效的新令牌格式');
        }
        
        // 更新令牌
        this.token = response.data.accessToken;
        localStorage.setItem('authToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        
        return true;
      } catch (error) {
        console.error('刷新令牌失败:', error);
        this.logout();
        return false;
      }
    },
    
    validateToken(token) {
  try {
    if (!token || !token.includes('.') || token.split('.').length !== 3) {
      console.error('token分段异常', token)
      return false;
    }
    const payloadPart = token.split('.')[1];
    const paddedPayload = payloadPart.padEnd(payloadPart.length + (4 - payloadPart.length % 4) % 4, '=');
    const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, '=');
    const decodedPayload = atob(padded);
    JSON.parse(decodedPayload);
    return true;
  } catch (e) {
    return false;
  }
}
    
    // ...其他方法保持不变...
  },
  getters: {
    userRank: (state) => state.user?.rank || 0,
    nextLevelExp: (state) => {
      if (!state.user) return 0
      const level = state.user.level
      
      if (level <= 0) return 50
      if (level <= 10) return 50 + level * 10
      if (level <= 30) return 140 + (level - 10) * 10
      return 340 + (level - 30) * 30
    }
  }
})