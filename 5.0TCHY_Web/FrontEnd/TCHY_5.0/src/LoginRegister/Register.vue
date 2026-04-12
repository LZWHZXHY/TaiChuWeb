<template>
  <div class="md-register-wrapper">
    <div class="md-register-container">
      
      <header class="md-header">
        <h1>申请凭证</h1>
        <p class="md-subtitle">请填写以下信息以注册您的系统操作员身份。</p>
      </header>

      <form @submit.prevent="handleRegister" class="md-form">
        
        <div class="md-input-group">
          <label for="username">操作员标识 (用户名)</label>
          <input
            id="username"
            v-model="registerForm.username"
            type="text"
            name="username"
            required
            placeholder="请输入 1-20 位字符"
            autocomplete="username"
            :class="{ 'error': errors.username }"
            @input="clearError('username')"
          />
          <span v-if="errors.username" class="md-error-text">{{ errors.username }}</span>
        </div>

        <div class="md-input-group">
          <label for="email">通讯地址 (邮箱)</label>
          <div class="md-flex-input">
            <input
              id="email"
              v-model="registerForm.email"
              type="email"
              name="email"
              required
              placeholder="请输入有效的邮箱地址"
              autocomplete="email"
              :class="{ 'error': errors.email }"
              @input="onEmailInput"
            />
            <button
              type="button"
              class="md-btn-outline"
              :disabled="!canSendCode || isSendingCode"
              @click="sendVerificationCode"
            >
              <span v-if="isSendingCode" class="loading-spinner-small"></span>
              <span v-else>
                {{ countdown > 0 ? `${countdown}s 后重发` : '发送验证码' }}
              </span>
            </button>
          </div>
          <span v-if="errors.email" class="md-error-text">{{ errors.email }}</span>
        </div>

        <div class="md-input-group">
          <label for="verificationCode">安全校验码</label>
          <input
            id="verificationCode"
            v-model="registerForm.verificationCode"
            type="text"
            maxlength="6"
            required
            placeholder="请输入 6 位邮箱验证码"
            autocomplete="off"
            :class="{ 'error': errors.verificationCode }"
            @input="clearError('verificationCode')"
          />
          <span v-if="errors.verificationCode" class="md-error-text">{{ errors.verificationCode }}</span>
        </div>

        <div class="md-input-group">
          <label for="password">安全密钥 (密码)</label>
          <input
            id="password"
            v-model="registerForm.password"
            type="password"
            name="new-password"
            required
            placeholder="请输入至少 6 位密码"
            autocomplete="new-password"
            :class="{ 'error': errors.password }"
            @input="clearError('password')"
          />
          <span v-if="errors.password" class="md-error-text">{{ errors.password }}</span>
        </div>

        <div class="md-input-group">
          <label for="confirmPassword">确认密钥</label>
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
          <span v-if="errors.confirmPassword" class="md-error-text">{{ errors.confirmPassword }}</span>
        </div>

        <div class="md-form-options">
          <label class="md-checkbox">
            <input
              type="checkbox"
              v-model="registerForm.agreeTerms"
              required
              autocomplete="off"
            />
            <span>
              我已阅读并同意 
              <a href="#" class="md-link" @click.prevent="showTerms">服务条款</a> 
              与 
              <a href="#" class="md-link" @click.prevent="showPrivacy">隐私政策</a>
            </span>
          </label>
        </div>

        <button type="submit" class="md-btn-primary" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>{{ isLoading ? '数据提交中...' : '注册并接入' }}</span>
        </button>

        <blockquote v-if="error" class="md-blockquote">
          <strong>System Message:</strong>
          <br />
          {{ error }}
        </blockquote>
      </form>

      <footer class="md-footer">
        <hr />
        <p>已有身份凭证? <a href="#" class="md-link" @click.prevent="switchToLogin">立即接入</a></p>
      </footer>
    </div>

    <Transition name="md-fade">
      <div v-if="showModal" class="md-modal-overlay" @click.self="closeModal">
        <div class="md-modal-content">
          <div class="md-modal-header">
            <h3>{{ modalTitle }}</h3>
            <button class="md-close-btn" @click="closeModal">&times;</button>
          </div>
          <div class="md-modal-body custom-scroll">
            <p>{{ modalText }}</p>
          </div>
          <div class="md-modal-footer">
            <button class="md-btn-outline" @click="closeModal">关闭</button>
          </div>
        </div>
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
// 导入外部协议文件
import termsText from '@/locales/legal/terms.zh.js'
import privacyText from '@/locales/legal/privacy.zh.js'

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

// 弹窗状态控制
const showModal = ref(false)
const modalTitle = ref('')
const modalText = ref('')

// 计算属性：是否可以发送验证码
const canSendCode = computed(() => {
  return registerForm.email && isValidEmail(registerForm.email) && countdown.value === 0
})

const onEmailInput = () => {
  clearError('email')
  error.value = ''
}

// 发送验证码
const sendVerificationCode = async () => {
  if (!isValidEmail(registerForm.email)) {
    errors.email = '邮箱格式不正确'
    return
  }

  isSendingCode.value = true
  error.value = ''

  try {
    const result = await authStore.sendVerificationCode(registerForm.email)
    
    if (result.success) {
      startCountdown()
      error.value = result.message || '✅ 验证码已发送，请查收邮箱'
    } else {
      error.value = result.error
    }
  } catch (err) {
    console.error('验证码发送错误:', err)
    error.value = '验证码发送失败，请稍后重试'
  } finally {
    isSendingCode.value = false
  }
}

// 开始倒计时
const startCountdown = () => {
  countdown.value = 60
  countdownTimer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(countdownTimer)
    }
  }, 1000)
}

// 表单验证
const validateForm = () => {
  let isValid = true
  Object.keys(errors).forEach(key => errors[key] = '')
  error.value = ''
  
  if (!registerForm.username.trim()) {
    errors.username = '请输入用户名'
    isValid = false
  } else if (registerForm.username.length < 1 || registerForm.username.length > 20) {
    errors.username = '用户名长度必须在 1-20 个字符之间'
    isValid = false
  }
  
  if (!registerForm.email.trim()) {
    errors.email = '请输入邮箱'
    isValid = false
  } else if (!isValidEmail(registerForm.email)) {
    errors.email = '邮箱格式不正确'
    isValid = false
  }
  
  if (!registerForm.verificationCode) {
    errors.verificationCode = '请输入验证码'
    isValid = false
  } else if (registerForm.verificationCode.length !== 6) {
    errors.verificationCode = '验证码必须是 6 位'
    isValid = false
  }
  
  if (!registerForm.password) {
    errors.password = '请输入密码'
    isValid = false
  } else if (registerForm.password.length < 6) {
    errors.password = '密码长度至少为 6 位'
    isValid = false
  }
  
  if (!registerForm.confirmPassword) {
    errors.confirmPassword = '请确认密码'
    isValid = false
  } else if (registerForm.password !== registerForm.confirmPassword) {
    errors.confirmPassword = '两次输入的密码不一致'
    isValid = false
  }
  
  if (!registerForm.agreeTerms) {
    error.value = '请阅读并同意服务条款与隐私政策'
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

// 注册处理
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
      router.push({ 
        path: '/login', 
        query: { 
          message: `✅ 注册成功！欢迎，${result.username}`,
          username: result.username 
        }
      })
    } else {
      error.value = result.error
    }
  } catch (err) {
    error.value = '注册过程中发生未知错误，请重试'
    console.error('注册错误:', err)
  } finally {
    isLoading.value = false
  }
}

const switchToLogin = () => {
  router.push('/login')
}

// 显示条款弹窗（加载 terms.zh.js 内容）
const showTerms = () => {
  modalTitle.value = '服务条款'
  modalText.value = termsText
  showModal.value = true
}

// 显示隐私弹窗（加载 privacy.zh.js 内容）
const showPrivacy = () => {
  modalTitle.value = '隐私保护协议'
  modalText.value = privacyText
  showModal.value = true
}

// 关闭弹窗
const closeModal = () => {
  showModal.value = false
}

onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer)
  }
})
</script>

<style scoped>
/* --- Markdown 极简风格变量 --- */
.md-register-wrapper {
  --text-primary: #24292f;
  --text-secondary: #57606a;
  --border-color: #d0d7de;
  --bg-color: #ffffff;
  --accent-color: #0969da;
  --error-color: #cf222e;
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

.md-register-container {
  width: 100%;
  max-width: 460px;
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
  width: 100%;
  box-sizing: border-box;
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

/* 邮箱与发送按钮行内布局 */
.md-flex-input {
  display: flex;
  gap: 8px;
  align-items: stretch;
}
.md-flex-input input {
  flex: 1;
}

.md-btn-outline {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  padding: 0 12px;
  font-size: 0.85rem;
  border-radius: 6px;
  cursor: pointer;
  transition: 0.2s;
  font-family: inherit;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  white-space: nowrap;
  min-width: 100px;
}

.md-btn-outline:hover:not(:disabled) {
  background: #f3f4f6;
  color: var(--text-primary);
  border-color: #1f2328;
}

.md-btn-outline:disabled {
  background: #f6f8fa;
  color: #8c959f;
  cursor: not-allowed;
}

/* --- 表单选项 (同意条款) --- */
.md-form-options {
  font-size: 0.9rem;
  margin-top: 0.5rem;
}

.md-checkbox {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  cursor: pointer;
  color: var(--text-primary);
  line-height: 1.4;
}

.md-checkbox input {
  cursor: pointer;
  margin-top: 2px;
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
  height: 40px;
}

.md-btn-primary:hover:not(:disabled) {
  background-color: #2c974b;
}

.md-btn-primary:disabled {
  background-color: #94d3a2;
  cursor: not-allowed;
  color: rgba(255, 255, 255, 0.8);
}

/* --- Markdown 引用块风格报错 --- */
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

/* --- Loading Spinners --- */
.loading-spinner {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
.loading-spinner-small {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid #d0d7de;
  border-top: 2px solid var(--text-secondary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

/* ====== MD 风格的极简弹窗 ====== */
.md-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(27, 31, 36, 0.5);
  z-index: 2000;
  display: flex;
  justify-content: center;
  align-items: center;
}

.md-modal-content {
  background: var(--bg-color);
  width: 90%;
  max-width: 540px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  box-shadow: 0 12px 28px rgba(0,0,0,0.15);
  display: flex;
  flex-direction: column;
  max-height: 80vh;
}

.md-modal-header {
  padding: 16px;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f6f8fa;
  border-radius: 8px 8px 0 0;
}

.md-modal-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--text-primary);
}

.md-close-btn {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  color: var(--text-secondary);
  cursor: pointer;
  line-height: 1;
  padding: 0 4px;
}

.md-close-btn:hover {
  color: var(--text-primary);
}

.md-modal-body {
  padding: 20px 16px;
  overflow-y: auto;
  color: var(--text-primary);
  font-size: 0.95rem;
  line-height: 1.6;
}

.md-modal-body p {
  margin: 0;
  white-space: pre-wrap; /* 保证换行生效 */
}

.md-modal-footer {
  padding: 16px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: flex-end;
  background: #f6f8fa;
  border-radius: 0 0 8px 8px;
}

/* 弹窗动画 */
.md-fade-enter-active, .md-fade-leave-active { transition: opacity 0.2s ease; }
.md-fade-enter-from, .md-fade-leave-to { opacity: 0; }
.md-fade-enter-active .md-modal-content { animation: mdModalIn 0.2s ease; }
@keyframes mdModalIn {
  from { transform: translateY(-20px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* --- 响应式 --- */
@media (max-width: 480px) {
  .md-register-wrapper {
    padding: 1rem;
    align-items: flex-start;
    padding-top: 2rem;
  }
  .md-flex-input {
    flex-direction: column;
  }
  .md-btn-outline {
    height: 38px;
  }
}
</style>