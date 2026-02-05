<template>
  <div class="fp-container">
    <div class="fp-card">
      <div class="fp-header">
        <h1>找回密码</h1>
        <p v-if="step === 1">请输入您注册时使用的邮箱，我们将发送验证码到该邮箱</p>
        <p v-else-if="step === 2">我们已经向您的邮箱发送了验证码，请输入6位验证码</p>
        <p v-else>请设置一个新的密码</p>
      </div>

      <form class="fp-form" @submit.prevent="onSubmit">
        <!-- Step 1: 输入邮箱 -->
        <div v-if="step === 1" class="form-group">
          <label for="email">邮箱地址</label>
          <input
            id="email"
            type="email"
            v-model.trim="form.email"
            required
            placeholder="请输入邮箱地址"
            :class="{ error: errors.email }"
            @input="clearError('email')"
            autocomplete="email"
          />
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
        </div>

        <!-- Step 2: 输入验证码 -->
        <div v-else-if="step === 2" class="form-group">
          <label for="code">验证码</label>
          <input
            id="code"
            type="text"
            v-model.trim="form.code"
            required
            maxlength="6"
            placeholder="请输入6位验证码"
            :class="{ error: errors.code }"
            @input="clearError('code')"
            autocomplete="one-time-code"
          />
          <span v-if="errors.code" class="error-text">{{ errors.code }}</span>

          <div class="code-actions">
            <button type="button" class="link-btn" :disabled="countdown > 0 || isLoading" @click="sendCode">
              {{ countdown > 0 ? `重新发送 (${countdown}s)` : '重新发送验证码' }}
            </button>
            <button type="button" class="link-btn" @click="step = 1">修改邮箱</button>
          </div>
        </div>

        <!-- Step 3: 重置密码 -->
        <div v-else class="form-group">
          <label for="password">新密码</label>
          <input
            id="password"
            type="password"
            v-model="form.newPassword"
            required
            placeholder="请输入新密码，至少6位"
            :class="{ error: errors.newPassword }"
            @input="clearError('newPassword')"
            autocomplete="new-password"
          />
          <span v-if="errors.newPassword" class="error-text">{{ errors.newPassword }}</span>

          <label for="confirm">确认新密码</label>
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
          <span v-if="errors.confirmPassword" class="error-text">{{ errors.confirmPassword }}</span>

          <div class="hint">提示：密码长度至少6位。可根据需要在后端增加复杂度校验。</div>
        </div>

        <button type="submit" class="primary-btn" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          <span v-else>
            {{ step === 1 ? '发送验证码' : step === 2 ? '验证' : '提交新密码' }}
          </span>
        </button>

        <div v-if="message" :class="['msg', messageType]">{{ message }}</div>
      </form>

      <div class="fp-footer">
        <a href="#" @click.prevent="goLogin">返回登录</a>
      </div>
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
    message.value = '如果该邮箱存在，将会收到验证码，请查收'
    messageType.value = 'info'
    startCountdown(60)
    // 自动切换到输入验证码步骤（如果当前为第一步）
    if (step.value === 1) step.value = 2
  } catch (e) {
    message.value = e?.response?.data?.error || '发送失败，请稍后重试'
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
        message.value = '验证码验证成功'
        messageType.value = 'success'
        step.value = 3
      } else {
        message.value = data?.error || '验证码无效'
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
        message.value = '密码重置成功，正在跳转登录...'
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
.fp-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
}
.fp-card {
  background: white;
  padding: 2rem 2rem 1.5rem;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0,0,0,.1);
  width: 100%;
  max-width: 420px;
}
.fp-header { text-align: center; margin-bottom: 1.5rem; }
.fp-header h1 { font-size: 1.75rem; font-weight: 700; color: #2c3e50; margin-bottom: .5rem; }
.fp-header p { color: #666; }
.fp-form { display: flex; flex-direction: column; gap: 1rem; }

.form-group { display: flex; flex-direction: column; }
label { margin: .5rem 0; color: #4a5568; font-weight: 500; }
input {
  padding: 0.75rem 1rem; border: 2px solid #e2e8f0; border-radius: 8px;
  font-size: 1rem; transition: all .2s ease; font-family: inherit;
}
input:focus { outline: none; border-color: #667eea; box-shadow: 0 0 0 3px rgba(102,126,234,.1); color:#2c3e50}
input.error { border-color: #e53e3e; color:black}

.error-text { color: #e53e3e; font-size: .85rem; margin-top: .25rem; min-height: 1rem; }

.primary-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #fff; border: none;
  padding: .75rem; border-radius: 8px; font-weight: 600; cursor: pointer;
  display: flex; align-items: center; justify-content: center; height: 44px;
}
.primary-btn:disabled { opacity: .6; cursor: not-allowed; }

.link-btn {
  margin-top: .5rem; background: transparent; border: none; color: #667eea;
  cursor: pointer; padding: 0; text-align: left;
}
.link-btn:disabled { color: #999; cursor: not-allowed; }

.code-actions {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-top: .5rem;
}

.hint {
  margin-top: .5rem;
  color: #718096;
  font-size: .85rem;
}

.msg { text-align: center; padding: .75rem; border-radius: 6px; font-size: .95rem; margin-top: .5rem; }
.msg.success { background: #c6f6d5; color: #2f855a; }
.msg.error { background: #fed7d7; color: #c53030; }
.msg.info { background: #bee3f8; color: #2b6cb0; }

.loading-spinner {
  width: 20px; height: 20px; border: 2px solid transparent; border-top: 2px solid currentColor;
  border-radius: 50%; animation: spin 1s linear infinite; margin-right: .5rem;
}
@keyframes spin { 0% { transform: rotate(0deg) } 100% { transform: rotate(360deg) } }

.fp-footer { text-align: center; margin-top: 1rem; color: #718096; }
.fp-footer a { color: #667eea; text-decoration: none; font-weight: 500; }
.fp-footer a:hover { text-decoration: underline; }

@media (max-width: 480px) {
  .fp-card { padding: 1.25rem; }
}
</style>