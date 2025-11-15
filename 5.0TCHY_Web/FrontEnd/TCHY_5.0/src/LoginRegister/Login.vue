<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h1>登录太初寰宇</h1>
        <p>欢迎回来！请登录您的账户</p>
      </div>

      <!-- 测试按钮 -->
      <button @click="testConnection" class="test-btn">
        测试API连接
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
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'

const router = useRouter()
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
const error = ref('')

// 清除错误信息
const clearError = (field) => {
  errors[field] = ''
  error.value = ''
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
    
    const result = await authStore.login({
      username: loginForm.username,
      password: loginForm.password
    })
    
    console.log('登录结果:', result)
    
    if (result.success) {
      // 登录成功，跳转到首页或原页面
      const redirect = router.currentRoute.value.query.redirect || '/'
      console.log('✅ 登录成功，跳转到:', redirect)
      router.push(redirect)
    } else {
      error.value = result.error
      console.error('❌ 登录失败:', result.error)
    }
  } catch (err) {
    error.value = '登录失败，请稍后重试'
    console.error('❌ 登录异常:', err)
  } finally {
    isLoading.value = false
  }
}

// 测试连接
const testConnection = async () => {
  try {
    console.log('🧪 开始测试API连接...')
    const response = await apiClient.get('/loginregister/check-email?email=test@example.com')
    console.log('✅ 连接测试成功:', response.data)
    alert('API连接正常！服务器响应: ' + JSON.stringify(response.data))
  } catch (err) {
    console.error('❌ 连接测试失败:', err)
    alert('API连接失败: ' + err.message)
  }
}

// 跳转到注册页面
const switchToRegister = () => {
  router.push('/register')
}

// 跳转到忘记密码页面
const handleForgotPassword = () => {
  router.push('/forgot-password')
}
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
  transition: border-color 0.3s ease;
}

input:focus {
  outline: none;
  border-color: #667eea;
}

input.error {
  border-color: #e53e3e;
}

.error-text {
  color: #e53e3e;
  font-size: 0.8rem;
  margin-top: 0.25rem;
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
}

.remember-me input {
  margin: 0;
}

.forgot-password {
  color: #667eea;
  text-decoration: none;
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
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.loading-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid transparent;
  border-top: 2px solid white;
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

.test-btn {
  background: #48bb78;
  color: white;
  border: none;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 500;
  margin-bottom: 1rem;
  width: 100%;
  transition: background-color 0.3s ease;
}

.test-btn:hover {
  background: #38a169;
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
}
</style>