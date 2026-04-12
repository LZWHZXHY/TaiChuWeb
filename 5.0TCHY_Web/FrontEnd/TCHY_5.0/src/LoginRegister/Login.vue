<template>
  <div class="md-login-wrapper">
    <div class="md-login-container">
      
      <header class="md-header">
        <h1>终端接入</h1>
        <p class="md-subtitle">请输入您的操作员凭证进行身份验证。</p>
      </header>

      <form @submit.prevent="handleLogin" class="md-form">
        
        <div class="md-input-group">
          <label for="username">操作员标识</label>
          <input
            id="username"
            v-model="loginForm.username"
            type="text"
            required
            placeholder="用户名或邮箱"
            autocomplete="username"
            :class="{ 'error': errors.username }"
            @input="clearError('username')"
          />
          <span v-if="errors.username" class="md-error-text">{{ errors.username }}</span>
        </div>

        <div class="md-input-group">
          <label for="password">安全密钥</label>
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
          <span v-if="errors.password" class="md-error-text">{{ errors.password }}</span>
        </div>

        <div class="md-form-options">
          <label class="md-checkbox">
            <input
              type="checkbox"
              v-model="loginForm.rememberMe"
              autocomplete="off"
            />
            <span>保持连接</span>
          </label>
          <a href="#" class="md-link" @click.prevent="handleForgotPassword">忘记密钥?</a>
        </div>

        <button type="submit" class="md-btn-primary" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>接入系统</span>
        </button>

        <blockquote v-if="error" class="md-blockquote" :class="{ 'is-success': error.includes('✅') }">
          <strong>System Message:</strong>
          <br />
          {{ error }}
        </blockquote>
      </form>

      <footer class="md-footer">
        <hr />
        <p>未注册身份? <a href="#" class="md-link" @click.prevent="switchToRegister">申请凭证</a></p>
      </footer>
      
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/utils/auth'

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
const error = ref('')

// 检查是否已登录
const isLoggedIn = computed(() => authStore.isAuthenticated)

// 组件挂载时检查登录状态
onMounted(() => {
  if (isLoggedIn.value) {
    const redirect = route.query.redirect || '/'
    console.log('用户已登录，重定向到:', redirect)
    router.push(redirect)
    return
  }

  // 检查是否有注册成功消息
  if (route.query.registered === 'true' || route.query.message) {
    error.value = route.query.message || '✅ 注册成功！请登录您的账户'
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
    const result = await authStore.login({
      username: loginForm.username,
      password: loginForm.password
    })
    
    if (result.success) {
      error.value = '✅ 登录成功！正在跳转...'
      await new Promise(resolve => setTimeout(resolve, 100))
      const redirect = route.query.redirect || '/'
      router.push(redirect)
    } else {
      error.value = result.error || '登录失败'
    }
  } catch (err) {
    error.value = '登录失败: ' + (err.message || '未知错误')
  } finally {
    isLoading.value = false
  }
}

// 跳转到注册页面
const switchToRegister = () => {
  router.push({
    path: '/register',
    query: { redirect: route.query.redirect }
  })
}

// 跳转到忘记密码页面
const handleForgotPassword = () => {
  router.push('/forgetPassword')
}

// 监听路由变化，处理注册成功后的消息
const unwatch = router.afterEach((to, from) => {
  if (to.name === 'Login' && from.name === 'Register') {
    if (to.query.registered === 'true' || to.query.message) {
      error.value = to.query.message || '✅ 注册成功！请登录您的账户'
    }
  }
})
</script>

<style scoped>
/* --- Markdown 极简风格变量 --- */
.md-login-wrapper {
  --text-primary: #24292f;
  --text-secondary: #57606a;
  --border-color: #d0d7de;
  --bg-color: #ffffff;
  --accent-color: #0969da;
  --error-color: #cf222e;
  --success-color: #1a7f37;
  --font-system: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";

  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: var(--bg-color);
  color: var(--text-primary);
  font-family: var(--font-system);
  padding: 2rem;
  line-height: 1.5;
}

.md-login-container {
  width: 100%;
  max-width: 420px;
}

/* --- 标题区域 (H1) --- */
.md-header {
  margin-bottom: 2rem;
}

.md-header h1 {
  font-size: 2rem;
  font-weight: 600;
  margin: 0 0 1rem 0;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
}

.md-header h1::before {
  content: '#';
  color: var(--border-color);
  margin-right: 0.5rem;
  font-weight: 400;
}

.md-subtitle {
  color: var(--text-secondary);
  font-size: 1rem;
  margin: 0;
}

/* --- 表单区域 --- */
.md-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.md-input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.md-input-group label {
  font-weight: 600;
  font-size: 0.9rem;
}

.md-input-group input {
  padding: 8px 12px;
  font-size: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background-color: var(--bg-color);
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  font-family: inherit;
}

.md-input-group input:focus {
  border-color: var(--accent-color);
  box-shadow: 0 0 0 3px rgba(9, 105, 218, 0.3);
}

.md-input-group input.error {
  border-color: var(--error-color);
}
.md-input-group input.error:focus {
  box-shadow: 0 0 0 3px rgba(207, 34, 46, 0.3);
}

.md-error-text {
  color: var(--error-color);
  font-size: 0.8rem;
  font-weight: 500;
}

/* --- 表单选项 --- */
.md-form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9rem;
}

.md-checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: var(--text-primary);
}

.md-checkbox input {
  cursor: pointer;
}

.md-link {
  color: var(--accent-color);
  text-decoration: none;
}

.md-link:hover {
  text-decoration: underline;
}

/* --- 提交按钮 --- */
.md-btn-primary {
  background-color: #2da44e;
  color: #ffffff;
  border: 1px solid rgba(27, 31, 36, 0.15);
  padding: 8px 16px;
  font-size: 1rem;
  font-weight: 600;
  border-radius: 6px;
  cursor: pointer;
  transition: 0.2s;
  text-align: center;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 0.5rem;
}

.md-btn-primary:hover:not(:disabled) {
  background-color: #2c974b;
}

.md-btn-primary:disabled {
  background-color: #94d3a2;
  cursor: not-allowed;
  color: rgba(255, 255, 255, 0.8);
}

/* --- Markdown 引用块风格提示 --- */
.md-blockquote {
  margin: 1rem 0 0 0;
  padding: 0.5rem 1rem;
  border-left: 4px solid var(--error-color);
  color: var(--text-secondary);
  background: #fff8f8;
  font-size: 0.9rem;
  border-radius: 0 6px 6px 0;
  white-space: pre-line;
  word-break: break-all;
}

.md-blockquote.is-success {
  border-left-color: var(--success-color);
  background: #f0fff4;
}

/* --- 页脚 --- */
.md-footer {
  margin-top: 2rem;
  text-align: center;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.md-footer hr {
  border: 0;
  border-top: 1px solid var(--border-color);
  margin-bottom: 1.5rem;
}

/* --- Loading Spinner --- */
.loading-spinner {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* --- 响应式 --- */
@media (max-width: 480px) {
  .md-login-wrapper {
    padding: 1rem;
    align-items: flex-start;
    padding-top: 3rem;
  }
}
</style>