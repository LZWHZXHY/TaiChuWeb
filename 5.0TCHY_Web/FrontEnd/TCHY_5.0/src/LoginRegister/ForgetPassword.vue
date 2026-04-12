<template>
  <div class="md-fp-wrapper">
    <div class="md-fp-container">
      
      <header class="md-header">
        <h1>重置密钥</h1>
        <p class="md-subtitle" v-if="step === 1">请输入您注册时使用的通讯地址，我们将向其发送验证指令。</p>
        <p class="md-subtitle" v-else-if="step === 2">指令已发送至您的邮箱，请输入接收到的 6 位安全校验码。</p>
        <p class="md-subtitle" v-else>身份验证通过，请为您的操作员凭证设置新的安全密钥。</p>
      </header>

      <form class="md-form" @submit.prevent="onSubmit">
        
        <div v-if="step === 1" class="md-input-group">
          <label for="email">通讯地址 (邮箱)</label>
          <input
            id="email"
            type="email"
            v-model.trim="form.email"
            required
            placeholder="请输入注册时使用的邮箱"
            :class="{ error: errors.email }"
            @input="clearError('email')"
            autocomplete="email"
          />
          <span v-if="errors.email" class="md-error-text">{{ errors.email }}</span>
        </div>

        <div v-else-if="step === 2" class="md-input-group">
          <label for="code">安全校验码</label>
          <input
            id="code"
            type="text"
            v-model.trim="form.code"
            required
            maxlength="6"
            placeholder="请输入 6 位校验码"
            :class="{ error: errors.code }"
            @input="clearError('code')"
            autocomplete="one-time-code"
          />
          <span v-if="errors.code" class="md-error-text">{{ errors.code }}</span>

          <div class="md-action-row">
            <button type="button" class="md-btn-outline" :disabled="countdown > 0 || isLoading" @click="sendCode">
              <span v-if="isLoading" class="loading-spinner-small"></span>
              <span v-else>{{ countdown > 0 ? `重新发送 (${countdown}s)` : '重新发送' }}</span>
            </button>
            <a href="#" class="md-link" @click.prevent="step = 1">修改邮箱</a>
          </div>
        </div>

        <div v-else class="md-step-group">
          <div class="md-input-group">
            <label for="password">新安全密钥</label>
            <input
              id="password"
              type="password"
              v-model="form.newPassword"
              required
              placeholder="请输入新密码，至少 6 位"
              :class="{ error: errors.newPassword }"
              @input="clearError('newPassword')"
              autocomplete="new-password"
            />
            <span v-if="errors.newPassword" class="md-error-text">{{ errors.newPassword }}</span>
          </div>

          <div class="md-input-group">
            <label for="confirm">确认新密钥</label>
            <input
              id="confirm"
              type="password"
              v-model="form.confirmPassword"
              required
              placeholder="请再次输入新密码"
              :class="{ error: errors.confirmPassword }"
              @input="clearError('confirmPassword')"
              autocomplete="new-password"
            />
            <span v-if="errors.confirmPassword" class="md-error-text">{{ errors.confirmPassword }}</span>
          </div>
        </div>

        <button type="submit" class="md-btn-primary" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>
            {{ step === 1 ? '发送验证指令' : step === 2 ? '验证校验码' : '提交新密钥' }}
          </span>
        </button>

        <blockquote 
          v-if="message" 
          class="md-blockquote" 
          :class="{
            'is-success': messageType === 'success',
            'is-error': messageType === 'error',
            'is-info': messageType === 'info'
          }"
        >
          <strong>System Message:</strong>
          <br />
          {{ message }}
        </blockquote>
      </form>

      <footer class="md-footer">
        <hr />
        <p>回想起密钥了? <a href="#" class="md-link" @click.prevent="goLogin">返回接入节点</a></p>
      </footer>
      
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'

const router = useRouter()

const step = ref(1) // 1: 输入邮箱, 2: 输入验证码, 3: 设置新密码
const isLoading = ref(false)
const countdown = ref(0)
let timer = null

const form = reactive({
  email: '',
  code: '',
  newPassword: '',
  confirmPassword: ''
})

const errors = reactive({
  email: '',
  code: '',
  newPassword: '',
  confirmPassword: ''
})

const message = ref('')
const messageType = ref('') // 'success' | 'error' | 'info'

const clearError = (field) => {
  if (field && errors[field]) errors[field] = ''
  message.value = ''
  messageType.value = ''
}

const validateStep = () => {
  let ok = true
  // 清空逐步错误
  errors.email = ''
  errors.code = ''
  errors.newPassword = ''
  errors.confirmPassword = ''
  message.value = ''
  messageType.value = ''

  if (step.value === 1) {
    if (!form.email) {
      errors.email = '请输入邮箱地址'
      ok = false
    } else if (!/^\S+@\S+\.\S+$/.test(form.email)) {
      errors.email = '邮箱格式不正确'
      ok = false
    }
  } else if (step.value === 2) {
    if (!form.code) {
      errors.code = '请输入验证码'
      ok = false
    } else if (!/^\d{6}$/.test(form.code)) {
      errors.code = '验证码必须为6位数字'
      ok = false
    }
  } else if (step.value === 3) {
    if (!form.newPassword || form.newPassword.length < 6) {
      errors.newPassword = '新密码长度至少6位'
      ok = false
    }
    if (form.confirmPassword !== form.newPassword) {
      errors.confirmPassword = '两次输入的密码不一致'
      ok = false
    }
  }
  return ok
}

const startCountdown = (sec = 60) => {
  countdown.value = sec
  clearInterval(timer)
  timer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(timer)
    }
  }, 1000)
}

const sendCode = async () => {
  if (!form.email || !/^\S+@\S+\.\S+$/.test(form.email)) {
    errors.email = '请先填写正确的邮箱地址'
    return
  }
  isLoading.value = true
  message.value = ''
  try {
    await apiClient.post('/loginregister/password/forgot', { email: form.email })
    message.value = '若凭证有效，验证指令将送达您的邮箱。'
    messageType.value = 'info'
    startCountdown(60)
    // 自动切换到输入验证码步骤（如果当前为第一步）
    if (step.value === 1) step.value = 2
  } catch (e) {
    message.value = e?.response?.data?.error || '指令发送失败，请稍后重试'
    messageType.value = 'error'
  } finally {
    isLoading.value = false
  }
}

const onSubmit = async () => {
  if (!validateStep()) return
  isLoading.value = true
  message.value = ''
  try {
    if (step.value === 1) {
      // 发送验证码并切换到 step 2
      await sendCode()
    } else if (step.value === 2) {
      const resp = await apiClient.post('/loginregister/password/verify-code', {
        email: form.email,
        code: form.code
      })
      const data = resp?.data
      if (data?.success) {
        message.value = '校验码验证成功，准许重置密钥。'
        messageType.value = 'success'
        step.value = 3
      } else {
        message.value = data?.error || '校验码无效或已过期'
        messageType.value = 'error'
      }
    } else if (step.value === 3) {
      const resp = await apiClient.post('/loginregister/password/reset', {
        email: form.email,
        code: form.code,
        newPassword: form.newPassword
      })
      const data = resp?.data
      if (data?.success) {
        message.value = '密钥重置成功！正在引导至接入节点...'
        messageType.value = 'success'
        setTimeout(() => router.push('/login'), 1200)
      } else {
        message.value = data?.error || '重置失败，请稍后重试'
        messageType.value = 'error'
      }
    }
  } catch (e) {
    message.value = e?.response?.data?.error || e?.message || '操作失败，请稍后重试'
    messageType.value = 'error'
  } finally {
    isLoading.value = false
  }
}

const goLogin = () => {
  router.push('/login')
}

onBeforeUnmount(() => {
  clearInterval(timer)
})
</script>

<style scoped>
/* --- Markdown 极简风格变量 --- */
.md-fp-wrapper {
  --text-primary: #24292f;
  --text-secondary: #57606a;
  --border-color: #d0d7de;
  --bg-color: #ffffff;
  --accent-color: #0969da;
  --error-color: #cf222e;
  --success-color: #1a7f37;
  --info-color: #0969da;
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

.md-fp-container {
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

.md-step-group {
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

/* 验证码附属操作区 */
.md-action-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-top: 0.25rem;
}

.md-btn-outline {
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  padding: 4px 12px;
  font-size: 0.85rem;
  border-radius: 6px;
  cursor: pointer;
  transition: 0.2s;
  font-family: inherit;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: 32px;
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

.md-link {
  color: var(--accent-color);
  text-decoration: none;
  font-size: 0.9rem;
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

/* --- Markdown 引用块风格提示 --- */
.md-blockquote {
  margin: 1rem 0 0 0;
  padding: 0.5rem 1rem;
  border-left: 4px solid var(--border-color);
  color: var(--text-secondary);
  background: #f6f8fa;
  font-size: 0.9rem;
  border-radius: 0 6px 6px 0;
  white-space: pre-line;
  word-break: break-all;
}

.md-blockquote.is-error {
  border-left-color: var(--error-color);
  background: #fff8f8;
  color: var(--error-color);
}

.md-blockquote.is-success {
  border-left-color: var(--success-color);
  background: #f0fff4;
  color: var(--success-color);
}

.md-blockquote.is-info {
  border-left-color: var(--info-color);
  background: #f0f6ff;
  color: var(--info-color);
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

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* --- 响应式 --- */
@media (max-width: 480px) {
  .md-fp-wrapper {
    padding: 1rem;
    align-items: flex-start;
    padding-top: 3rem;
  }
}
</style>