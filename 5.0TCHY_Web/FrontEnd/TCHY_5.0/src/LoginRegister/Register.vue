<!-- src/views/Register.vue -->
<template>
  <div class="register-container">
    <div class="register-card">
      <div class="register-header">
        <h1>{{$t('Register.registerAccount')}}</h1>
        <p>{{$t('Register.createAccount')}}</p>
      </div>

      <form @submit.prevent="handleRegister" class="register-form">
        <div class="form-group">
          <label for="username">{{$t('Register.username')}}</label>
          <input
            id="username"
            v-model="registerForm.username"
            type="text"
            name="username"
            required
            placeholder="请输入用户名（3-20个字符）"
            autocomplete="username"
            :class="{ 'error': errors.username }"
            @input="clearError('username')"
          />
          <span v-if="errors.username" class="error-text">{{ errors.username }}</span>
        </div>

        <div class="form-group">
          <label for="email">邮箱地址</label>
          <div class="email-input-group">
            <input
              id="email"
              v-model="registerForm.email"
              type="email"
              name="email"
              required
              placeholder="请输入邮箱地址"
              autocomplete="email"
              :class="{ 'error': errors.email }"
              @input="onEmailInput"
            />
            <button
              type="button"
              class="send-code-btn"
              :disabled="!canSendCode || isSendingCode"
              @click="sendVerificationCode"
            >
              <span v-if="isSendingCode" class="loading-spinner"></span>
              <span v-else>{{ countdown > 0 ? `${countdown}秒后重发` : '发送验证码' }}</span>
            </button>
          </div>
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label for="verificationCode">邮箱验证码</label>
          <input
            id="verificationCode"
            v-model="registerForm.verificationCode"
            type="text"
            maxlength="6"
            required
            placeholder="请输入6位验证码"
            autocomplete="off"
            :class="{ 'error': errors.verificationCode }"
            @input="clearError('verificationCode')"
          />
          <span v-if="errors.verificationCode" class="error-text">{{ errors.verificationCode }}</span>
        </div>

        <div class="form-group">
          <label for="password">密码</label>
          <input
            id="password"
            v-model="registerForm.password"
            type="password"
            name="new-password"
            required
            placeholder="请输入密码（至少6位）"
            autocomplete="new-password"
            :class="{ 'error': errors.password }"
            @input="clearError('password')"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <div class="form-group">
          <label for="confirmPassword">确认密码</label>
          <input
            id="confirmPassword"
            v-model="registerForm.confirmPassword"
            type="password"
            name="new-password"
            required
            placeholder="请再次输入密码"
            autocomplete="new-password"
            :class="{ 'error': errors.confirmPassword }"
            @input="clearError('confirmPassword')"
          />
          <span v-if="errors.confirmPassword" class="error-text">{{ errors.confirmPassword }}</span>
        </div>

        <div class="form-options">
          <label class="agree-terms">
            <input
              type="checkbox"
              v-model="registerForm.agreeTerms"
              required
              autocomplete="off"
            />
            <span>我已阅读并同意 <a href="#" @click.prevent="showTerms">服务条款</a> 和 <a href="#" @click.prevent="showPrivacy">隐私政策</a></span>
          </label>
        </div>

        <button type="submit" class="register-btn" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>注册账户</span>
        </button>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>

      <div class="register-footer">
        <p>已有账户？ <a href="#" @click.prevent="switchToLogin">立即登录</a></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'
import { useI18n } from 'vue-i18n'



const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

const registerForm = reactive({
  username: '',
  email: '',
  verificationCode: '',
  password: '',
  confirmPassword: '',
  agreeTerms: false
})

const errors = reactive({
  username: '',
  email: '',
  verificationCode: '',
  password: '',
  confirmPassword: ''
})

const isLoading = ref(false)
const isSendingCode = ref(false)
const countdown = ref(0)
const error = ref('')
let countdownTimer = null

// 计算属性：是否可以发送验证码
const canSendCode = computed(() => {
  return registerForm.email && isValidEmail(registerForm.email) && countdown.value === 0
})

const onEmailInput = () => {
  clearError('email')
  error.value = ''
}

// 发送验证码 - 修改API路径
// 在 Register.vue 的 script setup 中修改发送验证码方法
const sendVerificationCode = async () => {
  if (!isValidEmail(registerForm.email)) {
    errors.email = '请输入有效的邮箱地址'
    return
  }

  isSendingCode.value = true
  error.value = ''

  try {
    // 使用 authStore 的 sendVerificationCode 方法
    const result = await authStore.sendVerificationCode(registerForm.email)
    
    if (result.success) {
      startCountdown()
      error.value = result.message || '验证码已发送到您的邮箱，请查收'
    } else {
      error.value = result.error
    }
  } catch (err) {
    console.error('验证码发送错误:', err)
    error.value = '发送验证码失败，请稍后重试'
  } finally {
    isSendingCode.value = false
  }
}

// 开始倒计时
const startCountdown = () => {
  countdown.value = 60 // 60秒倒计时
  countdownTimer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(countdownTimer)
    }
  }, 1000)
}

const validateForm = () => {
  let isValid = true
  
  // 清空错误信息
  Object.keys(errors).forEach(key => errors[key] = '')
  error.value = ''
  
  // 验证用户名
  if (!registerForm.username.trim()) {
    errors.username = '请输入用户名'
    isValid = false
  } else if (registerForm.username.length < 3) {
    errors.username = '用户名至少3个字符'
    isValid = false
  } else if (registerForm.username.length > 20) {
    errors.username = '用户名不能超过20个字符'
    isValid = false
  }
  
  // 验证邮箱
  if (!registerForm.email.trim()) {
    errors.email = '请输入邮箱地址'
    isValid = false
  } else if (!isValidEmail(registerForm.email)) {
    errors.email = '请输入有效的邮箱地址'
    isValid = false
  }
  
  // 验证验证码
  if (!registerForm.verificationCode) {
    errors.verificationCode = '请输入验证码'
    isValid = false
  } else if (registerForm.verificationCode.length !== 6) {
    errors.verificationCode = '验证码必须是6位数字'
    isValid = false
  }
  
  // 验证密码
  if (!registerForm.password) {
    errors.password = '请输入密码'
    isValid = false
  } else if (registerForm.password.length < 6) {
    errors.password = '密码长度至少6位'
    isValid = false
  }
  
  // 验证确认密码
  if (!registerForm.confirmPassword) {
    errors.confirmPassword = '请确认密码'
    isValid = false
  } else if (registerForm.password !== registerForm.confirmPassword) {
    errors.confirmPassword = '两次输入的密码不一致'
    isValid = false
  }
  
  // 验证条款
  if (!registerForm.agreeTerms) {
    error.value = '请同意服务条款和隐私政策'
    isValid = false
  }
  
  return isValid
}

const isValidEmail = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

const clearError = (field) => {
  errors[field] = ''
  error.value = ''
}

const handleRegister = async () => {
  if (!validateForm()) return
  
  isLoading.value = true
  error.value = ''
  
  try {
    const result = await authStore.register({
      username: registerForm.username,
      email: registerForm.email,
      password: registerForm.password,
      verificationCode: registerForm.verificationCode
    })
    
    if (result.success) {
      // 注册成功，跳转到登录页
      router.push({ 
        path: '/login', 
        query: { 
          message: `注册成功！欢迎 ${result.username} 加入太初寰宇`,
          username: result.username 
        }
      })
    } else {
      error.value = result.error
    }
  } catch (err) {
    error.value = '注册失败，请稍后重试'
    console.error('注册错误:', err)
  } finally {
    isLoading.value = false
  }
}

const switchToLogin = () => {
  router.push('/login')
}

const showTerms = () => {
  alert('服务条款功能开发中...')
}

const showPrivacy = () => {
  alert('隐私政策功能开发中...')
}

// 组件卸载时清除定时器
onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer)
  }
})
</script>

<style scoped>
.register-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}

.register-card {
  background: white;
  padding: 3rem;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 450px;
}

.register-header {
  text-align: center;
  margin-bottom: 2rem;
}

.register-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.register-header p {
  color: #666;
  font-size: 1rem;
}

.register-form {
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

/* 邮箱输入组样式 */
.email-input-group {
  display: flex;
  gap: 10px;
}

.email-input-group input {
  flex: 1;
}

.send-code-btn {
  background: #667eea;
  color: white;
  border: none;
  padding: 0 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  min-width: 120px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  white-space: nowrap;
}

.send-code-btn:hover:not(:disabled) {
  background: #5a6fd8;
  transform: translateY(-1px);
}

.send-code-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
}

.form-options {
  margin: 1rem 0;
}

.agree-terms {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  font-size: 0.9rem;
  line-height: 1.4;
}

.agree-terms a {
  color: #667eea;
  text-decoration: none;
}

.agree-terms a:hover {
  text-decoration: underline;
}

.register-btn {
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

.register-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.register-btn:disabled {
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

.register-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
  color: #718096;
}

.register-footer a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.register-footer a:hover {
  text-decoration: underline;
}

@media (max-width: 480px) {
  .register-container {
    padding: 1rem;
  }
  
  .register-card {
    padding: 2rem;
  }
  
  .register-header h1 {
    font-size: 1.5rem;
  }
  
  .email-input-group {
    flex-direction: column;
  }
  
  .send-code-btn {
    min-width: auto;
    height: 40px;
  }
}
</style>