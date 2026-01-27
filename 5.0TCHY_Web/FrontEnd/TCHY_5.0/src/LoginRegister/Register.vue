<!-- src/views/Register.vue -->
<template>
  <div class="register-container">
    <div class="register-card">
      <div class="register-header">
        <h1>{{ $t('Register.registerAccount') }}</h1>
        <p>{{ $t('Register.createAccount') }}</p>
      </div>

      <form @submit.prevent="handleRegister" class="register-form">
        <div class="form-group">
          <label for="username">{{ $t('Register.username') }}</label>
          <input
            id="username"
            v-model="registerForm.username"
            type="text"
            name="username"
            required
            :placeholder="$t('Register.placeholder_username')"
            autocomplete="username"
            :class="{ 'error': errors.username }"
            @input="clearError('username')"
          />
          <span v-if="errors.username" class="error-text">{{ errors.username }}</span>
        </div>

        <div class="form-group">
          <label for="email">{{ $t('Register.emailAddress') }}</label>
          <div class="email-input-group">
            <input
              id="email"
              v-model="registerForm.email"
              type="email"
              name="email"
              required
              :placeholder="$t('Register.placeholder_email')"
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
              <span v-else>
                {{ countdown > 0 ? `${countdown}${$t('Register.resend_code')}` : $t('Register.send_code') }}
              </span>
            </button>
          </div>
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label for="verificationCode">{{ $t('Register.emailVerifyCode') }}</label>
          <input
            id="verificationCode"
            v-model="registerForm.verificationCode"
            type="text"
            maxlength="6"
            required
            :placeholder="$t('Register.placeholder_code')"
            autocomplete="off"
            :class="{ 'error': errors.verificationCode }"
            @input="clearError('verificationCode')"
          />
          <span v-if="errors.verificationCode" class="error-text">{{ errors.verificationCode }}</span>
        </div>

        <div class="form-group">
          <label for="password">{{ $t('Register.password') }}</label>
          <input
            id="password"
            v-model="registerForm.password"
            type="password"
            name="new-password"
            required
            :placeholder="$t('Register.placeholder_password')"
            autocomplete="new-password"
            :class="{ 'error': errors.password }"
            @input="clearError('password')"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <div class="form-group">
          <label for="confirmPassword">{{ $t('Register.confirmPassword') }}</label>
          <input
            id="confirmPassword"
            v-model="registerForm.confirmPassword"
            type="password"
            name="new-password"
            required
            :placeholder="$t('Register.placeholder_confirm')"
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
            <span>
              {{ $t('Register.agree_prefix') }} 
              <a href="#" @click.prevent="showTerms">{{ $t('Register.terms') }}</a> 
              & 
              <a href="#" @click.prevent="showPrivacy">{{ $t('Register.privacy') }}</a>
            </span>
          </label>
        </div>

        <button type="submit" class="register-btn" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>{{ isLoading ? $t('Register.submit_loading') : $t('Register.submit_btn') }}</span>
        </button>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>

      <div class="register-footer">
        <p>
          {{ $t('Register.have_account') }} 
          <a href="#" @click.prevent="switchToLogin">{{ $t('Register.login_now') }}</a>
        </p>
      </div>
    </div>

    <Transition name="modal">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal-content">
          <div class="modal-header">
            <h3>{{ modalTitle }}</h3>
            <button class="close-btn" @click="closeModal">&times;</button>
          </div>
          <div class="modal-body custom-scroll">
            <p>{{ modalText }}</p>
          </div>
          <div class="modal-footer">
            <button class="primary-btn" @click="closeModal">{{ $t('Register.modal_close') || '关闭' }}</button>
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

// --- ✅ 新增：弹窗状态控制 ---
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
    errors.email = t('Register.err_email_invalid')
    return
  }

  isSendingCode.value = true
  error.value = ''

  try {
    const result = await authStore.sendVerificationCode(registerForm.email)
    
    if (result.success) {
      startCountdown()
      error.value = result.message || t('Register.msg_code_sent')
    } else {
      error.value = result.error
    }
  } catch (err) {
    console.error('验证码发送错误:', err)
    error.value = t('Register.msg_code_fail')
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

const validateForm = () => {
  let isValid = true
  Object.keys(errors).forEach(key => errors[key] = '')
  error.value = ''
  
  if (!registerForm.username.trim()) {
    errors.username = t('Register.err_username_empty')
    isValid = false
  } else if (registerForm.username.length < 3 || registerForm.username.length > 20) {
    errors.username = t('Register.err_username_len')
    isValid = false
  }
  
  if (!registerForm.email.trim()) {
    errors.email = t('Register.err_email_empty')
    isValid = false
  } else if (!isValidEmail(registerForm.email)) {
    errors.email = t('Register.err_email_invalid')
    isValid = false
  }
  
  if (!registerForm.verificationCode) {
    errors.verificationCode = t('Register.err_code_empty')
    isValid = false
  } else if (registerForm.verificationCode.length !== 6) {
    errors.verificationCode = t('Register.err_code_len')
    isValid = false
  }
  
  if (!registerForm.password) {
    errors.password = t('Register.err_password_empty')
    isValid = false
  } else if (registerForm.password.length < 6) {
    errors.password = t('Register.err_password_len')
    isValid = false
  }
  
  if (!registerForm.confirmPassword) {
    errors.confirmPassword = t('Register.err_confirm_empty')
    isValid = false
  } else if (registerForm.password !== registerForm.confirmPassword) {
    errors.confirmPassword = t('Register.err_confirm_match')
    isValid = false
  }
  
  if (!registerForm.agreeTerms) {
    error.value = t('Register.err_terms')
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
      router.push({ 
        path: '/login', 
        query: { 
          message: `${t('Register.msg_reg_success')} ${result.username}`,
          username: result.username 
        }
      })
    } else {
      error.value = result.error
    }
  } catch (err) {
    error.value = t('Register.msg_reg_fail')
    console.error('注册错误:', err)
  } finally {
    isLoading.value = false
  }
}

const switchToLogin = () => {
  router.push('/login')
}

// --- ✅ 修改：显示条款弹窗 ---
const showTerms = () => {
  modalTitle.value = t('Register.terms_title')
  // 这里会自动获取你之前在 zh.js/en.js 里配置的长文本
  modalText.value = t('Register.terms_content') 
  showModal.value = true
}

// --- ✅ 修改：显示隐私弹窗 ---
const showPrivacy = () => {
  modalTitle.value = t('Register.privacy_title')
  modalText.value = t('Register.privacy_content')
  showModal.value = true
}

// --- ✅ 修改：关闭弹窗 ---
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
/* 原有样式保持不变 */
.register-container { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 2rem; }
.register-card { background: white; padding: 3rem; border-radius: 16px; box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1); width: 100%; max-width: 450px; }
.register-header { text-align: center; margin-bottom: 2rem; }
.register-header h1 { font-size: 2rem; font-weight: 700; color: #2c3e50; margin-bottom: 0.5rem; }
.register-header p { color: #666; font-size: 1rem; }
.register-form { display: flex; flex-direction: column; gap: 1.5rem; }
.form-group { display: flex; flex-direction: column; }
label { margin-bottom: 0.5rem; color: #4a5568; font-weight: 500; font-size: 0.9rem; }
input { padding: 0.75rem 1rem; border: 2px solid #757575; border-radius: 8px; font-size: 1rem; transition: border-color 0.3s ease; color:#2d3748}
input:focus { outline: none; border-color: #667eea; }
input.error { border-color: #e53e3e; }
.error-text { color: #e53e3e; font-size: 0.8rem; margin-top: 0.25rem; }
.email-input-group { display: flex; gap: 10px; }
.email-input-group input { flex: 1; }
.send-code-btn { background: #667eea; color: white; border: none; padding: 0 16px; border-radius: 6px; cursor: pointer; font-size: 14px; min-width: 120px; transition: all 0.3s ease; display: flex; align-items: center; justify-content: center; white-space: nowrap; }
.send-code-btn:hover:not(:disabled) { background: #5a6fd8; transform: translateY(-1px); }
.send-code-btn:disabled { background: #ccc; cursor: not-allowed; transform: none; }
.form-options { margin: 1rem 0; }
.agree-terms { display: flex; align-items: flex-start; gap: 0.5rem; font-size: 0.9rem; line-height: 1.4; }
.agree-terms a { color: #667eea; text-decoration: none; }
.agree-terms a:hover { text-decoration: underline; }
.register-btn { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border: none; padding: 0.75rem; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; transition: all 0.3s ease; height: 48px; display: flex; align-items: center; justify-content: center; }
.register-btn:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3); }
.register-btn:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }
.loading-spinner { width: 20px; height: 20px; border: 2px solid transparent; border-top: 2px solid white; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
.error-message { background: #fed7d7; color: #c53030; padding: 0.75rem; border-radius: 6px; text-align: center; font-size: 0.9rem; }
.register-footer { text-align: center; margin-top: 2rem; padding-top: 1rem; border-top: 1px solid #e2e8f0; color: #718096; }
.register-footer a { color: #667eea; text-decoration: none; font-weight: 500; }
.register-footer a:hover { text-decoration: underline; }

/* ✅ 新增：弹窗组件的样式 */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0, 0, 0, 0.5); z-index: 2000;
  display: flex; justify-content: center; align-items: center;
  backdrop-filter: blur(4px);
}
.modal-content {
  background: white; width: 90%; max-width: 500px;
  border-radius: 12px; overflow: hidden;
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.2);
  display: flex; flex-direction: column;
  max-height: 80vh; /* 限制高度，防止超出屏幕 */
}
.modal-header {
  padding: 16px 20px; border-bottom: 1px solid #e2e8f0;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-header h3 { margin: 0; font-size: 18px; color: #2d3748; }
.close-btn { background: none; border: none; font-size: 24px; color: #a0aec0; cursor: pointer; }
.close-btn:hover { color: #4a5568; }

.modal-body { 
  padding: 20px; 
  overflow-y: auto; /* 内容过长时滚动 */
  color: #4a5568; 
  font-size: 14px; 
  line-height: 1.6; 
}
.modal-body p { 
  white-space: pre-wrap; /* 关键：保留你文本文件里的换行和格式 */
}

.modal-footer {
  padding: 16px 20px; border-top: 1px solid #e2e8f0; text-align: right; background: #f8fafc;
}
.primary-btn {
  background: #667eea; color: white; border: none;
  padding: 8px 20px; border-radius: 6px; cursor: pointer; font-size: 14px;
}
.primary-btn:hover { background: #5a6fd8; }

/* 简单的弹窗动画 */
.modal-enter-active, .modal-leave-active { transition: opacity 0.3s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-active .modal-content { animation: modalIn 0.3s ease; }
@keyframes modalIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }

@media (max-width: 480px) {
  .register-container { padding: 1rem; }
  .register-card { padding: 2rem; }
  .register-header h1 { font-size: 1.5rem; }
  .email-input-group { flex-direction: column; }
  .send-code-btn { min-width: auto; height: 40px; }
}
</style>