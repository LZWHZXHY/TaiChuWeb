<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h1>登录太初寰宇</h1>
        <p>欢迎回来！请登录您的账户</p>
      </div>

      <!-- 测试连接按钮 -->
      <button @click="testConnection" class="test-btn">
        <span v-if="testingConnection" class="loading-spinner"></span>
        <span v-else>测试API连接</span>
      </button>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="username">用户名或邮箱</label>
          <input
            id="username"
            v-model="loginForm.username"
            type="text"
            required
            placeholder="请输入用户名或邮箱"
            autocomplete="username"
            :class="{ 'error': errors.username }"
            @input="clearError('username')"
          />
          <span v-if="errors.username" class="error-text">{{ errors.username }}</span>
        </div>

        <div class="form-group">
          <label for="password">密码</label>
          <input
            id="password"
            v-model="loginForm.password"
            type="password"
            required
            placeholder="请输入密码"
            autocomplete="current-password"
            :class="{ 'error': errors.password }"
            @input="clearError('password')"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <div class="form-options">
          <label class="remember-me">
            <input
              type="checkbox"
              v-model="loginForm.rememberMe"
              autocomplete="off"
            />
            <span>记住我</span>
          </label>
          <a href="#" class="forgot-password" @click.prevent="handleForgotPassword">忘记密码？</a>
        </div>

        <button type="submit" class="login-btn" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>登录</span>
        </button>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>

      <div class="login-footer">
        <p>还没有账号？ <a href="#" @click.prevent="switchToRegister">立即注册</a></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const loginForm = reactive({
  username: '',
  password: '',
  rememberMe: false
})

const errors = reactive({
  username: '',
  password: ''
})

const isLoading = ref(false)
const testingConnection = ref(false)
const error = ref('')

// 检查是否已登录
const isLoggedIn = computed(() => authStore.isAuthenticated)

// 组件挂载时检查登录状态
onMounted(() => {
  // 如果用户已登录，重定向到首页或目标页面
  if (isLoggedIn.value) {
    const redirect = route.query.redirect || '/'
    console.log('用户已登录，重定向到:', redirect)
    router.push(redirect)
    return
  }

  // 检查是否有注册成功消息
  if (route.query.registered === 'true' || route.query.message) {
    error.value = route.query.message || '注册成功！请登录您的账户'
  }
})

// 清除错误信息
const clearError = (field) => {
  if (errors[field]) {
    errors[field] = ''
  }
  if (error.value) {
    error.value = ''
  }
}

// 表单验证
const validateForm = () => {
  let isValid = true
  
  // 清空错误信息
  errors.username = ''
  errors.password = ''
  error.value = ''
  
  if (!loginForm.username.trim()) {
    errors.username = '请输入用户名或邮箱'
    isValid = false
  }
  
  if (!loginForm.password) {
    errors.password = '请输入密码'
    isValid = false
  } else if (loginForm.password.length < 6) {
    errors.password = '密码长度至少6位'
    isValid = false
  }
  
  return isValid
}

// 测试API连接
const testConnection = async () => {
  testingConnection.value = true
  error.value = ''
  
  try {
    console.log('🧪 开始测试API连接...')
    
    // 尝试多个可能的测试端点
    const testEndpoints = [
      '/default/health',
      '/default/users/count',
      '/loginregister/health',
      '/api/health'
    ]
    
    let success = false
    let result = null
    let workingEndpoint = ''
    
    for (const endpoint of testEndpoints) {
      try {
        console.log(`尝试连接: ${endpoint}`)
        const response = await apiClient.get(endpoint)
        success = true
        result = response.data
        workingEndpoint = endpoint
        console.log(`✅ ${endpoint} 连接成功:`, result)
        break
      } catch (err) {
        console.log(`❌ ${endpoint} 连接失败:`, err.message)
        continue
      }
    }
    
    if (success) {
      error.value = `✅ API连接正常！\n端点: ${workingEndpoint}\n响应: ${JSON.stringify(result, null, 2)}`
    } else {
      error.value = '❌ 所有API端点连接失败，请检查后端服务状态'
    }
  } catch (err) {
    console.error('❌ 连接测试失败:', err)
    error.value = '连接测试失败: ' + err.message
  } finally {
    testingConnection.value = false
  }
}

// 登录处理
// 登录处理
const handleLogin = async () => {
  if (!validateForm()) return
  
  isLoading.value = true
  error.value = ''
  
  try {
    console.log('🔐 开始登录:', { 
      username: loginForm.username,
      password: '***' 
    })
    
    // 使用authStore的login方法
    const result = await authStore.login({
      username: loginForm.username,
      password: loginForm.password
    })
    
    console.log('登录结果:', result)
    
    if (result.success) {
      // 登录成功，显示成功消息
      error.value = '✅ 登录成功！正在跳转...'
      
      // 确保状态完全更新后再跳转
      await new Promise(resolve => setTimeout(resolve, 100))
      
      const redirect = route.query.redirect || '/'
      console.log('✅ 登录成功，跳转到:', redirect)
      
      // 直接跳转，不使用 setTimeout
      router.push(redirect)
    } else {
      error.value = result.error || '登录失败'
      console.error('❌ 登录失败:', result.error)
    }
  } catch (err) {
    console.error('❌ 登录异常:', err)
    error.value = '登录失败: ' + (err.message || '未知错误')
  } finally {
    isLoading.value = false
  }
}

// 跳转到注册页面
const switchToRegister = () => {
  router.push({
    path: '/register',
    query: { 
      redirect: route.query.redirect 
    }
  })
}

// 跳转到忘记密码页面
const handleForgotPassword = () => {
  router.push('/forgot-password')
}

// 监听路由变化，处理注册成功后的消息
const unwatch = router.afterEach((to, from) => {
  if (to.name === 'Login' && from.name === 'Register') {
    if (to.query.registered === 'true' || to.query.message) {
      error.value = to.query.message || '注册成功！请登录您的账户'
    }
  }
})
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

.login-card {
  background: white;
  padding: 3rem;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.login-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.login-header p {
  color: #666;
  font-size: 1rem;
  line-height: 1.5;
}

.test-btn {
  width: 100%;
  background: #48bb78;
  color: white;
  border: none;
  padding: 0.75rem;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.test-btn:hover:not(:disabled) {
  background: #38a169;
  transform: translateY(-1px);
}

.test-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

label {
  margin-bottom: 0.5rem;
  color: #4a5568;
  font-weight: 500;
  font-size: 0.9rem;
}

input {
  padding: 0.75rem 1rem;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
  font-family: inherit;
}

input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

input.error {
  border-color: #e53e3e;
}

.error-text {
  color: #e53e3e;
  font-size: 0.8rem;
  margin-top: 0.25rem;
  min-height: 1rem;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9rem;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: #4a5568;
}

.remember-me input {
  margin: 0;
  width: auto;
}

.forgot-password {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.forgot-password:hover {
  text-decoration: underline;
}

.login-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 0.75rem;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.loading-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  background: #fed7d7;
  color: #c53030;
  padding: 0.75rem;
  border-radius: 6px;
  text-align: center;
  font-size: 0.9rem;
  line-height: 1.4;
  white-space: pre-line;
}

.error-message:empty {
  display: none;
}

.login-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
  color: #718096;
}

.login-footer a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.login-footer a:hover {
  text-decoration: underline;
}

.success-message {
  background: #c6f6d5;
  color: #2f855a;
  padding: 0.75rem;
  border-radius: 6px;
  text-align: center;
  font-size: 0.9rem;
  margin-bottom: 1rem;
}

@media (max-width: 480px) {
  .login-container {
    padding: 1rem;
  }
  
  .login-card {
    padding: 2rem;
  }
  
  .login-header h1 {
    font-size: 1.5rem;
  }
  
  .form-options {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .forgot-password {
    align-self: flex-end;
  }
}

@media (max-width: 360px) {
  .login-card {
    padding: 1.5rem;
  }
  
  .login-header h1 {
    font-size: 1.25rem;
  }
}
</style>