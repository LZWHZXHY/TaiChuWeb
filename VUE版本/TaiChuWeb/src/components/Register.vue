<template>
  <div class="BigContainer">
    <div class="register-container">
      <h2>注册太初寰宇</h2>
      <p>(｡･∀･)ﾉﾞ嗨！在这里进行注册！</p>
      <p>注意！那你需要同时输入用户名和邮箱才能收获验证码！</p>
      <form @submit.prevent="handleRegister">
        <!-- 用户名输入 -->
        <div class="form-group">
          <label for="username">用户名:</label>
          <input 
            id="username"
            v-model="formData.username" 
            autocomplete="username" 
            required 
          />
        </div>
        
        <!-- 邮箱输入 -->
        <div class="form-group">
          <label for="email">邮箱:</label>
          <input 
            id="email"
            v-model="formData.email" 
            type="email" 
            autocomplete="email" 
            required 
          />
          <button 
            type="button" 
            @click="sendVerificationCode" 
            :disabled="uiState.countdown > 0 || uiState.sendingCode"
            class="code-btn"
            aria-label="获取验证码"
          >
            {{ uiState.sendingCode ? '发送中...' : (uiState.countdown > 0 ? `${uiState.countdown}秒后重发` : '获取验证码') }}
          </button>
        </div>
        
        <!-- 验证码输入 -->
        <div class="form-group" v-if="uiState.codeSent">
          <label for="verificationCode">验证码:</label>
          <input 
            id="verificationCode"
            v-model="formData.verificationCode" 
            placeholder="请输入邮件中的验证码" 
            required 
          />
        </div>
        
        <!-- 密码输入 -->
        <div class="form-group">
          <div class="password-container">
            <label for="password">密码:</label>
            <input 
              id="password"
              :type="uiState.showPassword ? 'text' : 'password'" 
              v-model="formData.password" 
              autocomplete="new-password" 
              required 
            />
            <span 
              class="toggle-password" 
              @click="uiState.showPassword = !uiState.showPassword"
              :aria-label="uiState.showPassword ? '隐藏密码' : '显示密码'"
            >
              {{ uiState.showPassword ? '隐藏' : '显示' }}
            </span>
          </div>
        </div>
        
        <!-- 确认密码 -->
        <div class="form-group">
          <div class="password-container">
            <label for="confirmPassword">确认密码:</label>
            <input 
              id="confirmPassword"
              :type="uiState.showConfirmPassword ? 'text' : 'password'" 
              v-model="formData.confirmPassword" 
              autocomplete="new-password" 
              required 
            />
            <span 
              class="toggle-password" 
              @click="uiState.showConfirmPassword = !uiState.showConfirmPassword"
              :aria-label="uiState.showConfirmPassword ? '隐藏确认密码' : '显示确认密码'"
            >
              {{ uiState.showConfirmPassword ? '隐藏' : '显示' }}
            </span>
          </div>
        </div>
        
        <!-- 注册按钮 -->
        <button 
          type="submit" 
          :disabled="uiState.loading" 
          id="Register"
          aria-busy="loading"
        >
          <span v-if="uiState.loading">
            <span class="loader"></span> 注册中...
          </span>
          <span v-else>注册</span>
        </button>
        
        <!-- 状态消息 -->
        <div class="error" v-if="uiState.apiError">{{ uiState.apiError }}</div>
        <div class="error" v-if="uiState.fieldError">{{ uiState.fieldError }}</div>
        <div class="success" v-if="uiState.success">{{ uiState.success }}</div>
        
        <!-- 开发提示 -->
        <div v-if="isDev" class="note">
          开发提示：验证码会打印到后端控制台
        </div>
        
        <!-- 返回登录 -->
        <div style="margin-top:1em;" class="backToLogin">
          已有账号？<router-link to="/login">点这里返回登录~</router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted } from 'vue';
import axios from 'axios';
import { useRouter } from 'vue-router';

// 使用 Vue Router
const router = useRouter();

// 表单数据对象
const formData = ref({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  verificationCode: ''
});

// UI 状态 - 确保所有属性都已定义
const uiState = ref({
  showPassword: false,
  showConfirmPassword: false, // 修复的属性
  loading: false,
  codeSent: false,
  sendingCode: false,
  apiError: '', // 修复的属性
  fieldError: '', // 修复的属性
  success: '', // 修复的属性
  countdown: 0
});

// 计时器引用
let countdownTimer = null;

// API 基础 URL（使用环境变量）
const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321' 
  : 'https://bianyuzhou.com';

// 是否为开发环境
const isDev = computed(() => {
  return import.meta.env.MODE === 'development';
});

// 组件卸载时清理定时器
onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
});

// 倒计时功能
const startCountdown = () => {
  uiState.value.countdown = 60;
  
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
  
  countdownTimer = setInterval(() => {
    if (uiState.value.countdown > 0) {
      uiState.value.countdown--;
    } else {
      clearInterval(countdownTimer);
    }
  }, 1000);
};

// 发送验证码
const sendVerificationCode = async () => {
  // 验证邮箱格式
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailPattern.test(formData.value.email)) {
    uiState.value.fieldError = '请输入有效的邮箱地址';
    return;
  }

  // 重置错误状态
  uiState.value.apiError = '';
  uiState.value.fieldError = '';
  uiState.value.sendingCode = true;
  
  try {
    const requestUrl = `${baseApiUrl}/api/Auth/send-verification-code`;
    
    const response = await axios.post(requestUrl, {
      email_address: formData.value.email,
      username: formData.value.username
    });

    // 开发环境自动填充验证码
    if (isDev.value && response.data?.code) {
      formData.value.verificationCode = response.data.code;
      uiState.value.success = `验证码已发送: ${response.data.code} (仅在开发环境显示)`;
    } else {
      uiState.value.success = '验证码已发送到您的邮箱，请注意查收';
    }
    
    uiState.value.codeSent = true;
    startCountdown();
    
    // 10秒后清除成功消息
    setTimeout(() => {
      if (uiState.value.success.includes('验证码已发送:')) {
        uiState.value.success = '';
      }
    }, 10000);
  } catch (error) {
    console.error('发送验证码出错:', error);
    
    if (error.response) {
      uiState.value.apiError = error.response.data?.message || '发送验证码失败';
    } else {
      uiState.value.apiError = '网络错误，请检查您的网络连接';
    }
  } finally {
    uiState.value.sendingCode = false;
  }
};

// 处理注册
const handleRegister = async () => {

  console.log('发送注册请求，数据:', {
    username: formData.value.username,
    email: formData.value.email,
    password: formData.value.password,
    verificationCode: formData.value.verificationCode
  });


  // 重置错误状态
  uiState.value.apiError = '';
  uiState.value.fieldError = '';
  uiState.value.loading = true;
  
  try {
    // 创建请求数据对象
    const requestData = {
      username: formData.value.username,
      email_address: formData.value.email,
      password_hash: formData.value.password,
      verification_code: formData.value.verificationCode
    };
    
    const response = await axios.post(
      `${baseApiUrl}/api/Auth/register-with-code`, 
      requestData
    );
    
    uiState.value.success = '注册成功！即将跳转到登录页面...';
    
    // 3秒后跳转到登录页面
    setTimeout(() => {
      router.push('/login');
    }, 3000);
  } catch (error) {
    console.error('注册失败:', error);
    
    if (error.response?.data) {
      const responseData = error.response.data;
      
      // 处理验证错误
      if (responseData.errors) {
        const errorMessages = Object.values(responseData.errors).flat();
        uiState.value.apiError = '验证错误: ' + errorMessages.join('; ');
      } else {
        uiState.value.apiError = responseData.message || responseData.title || '注册失败，请稍后重试';
      }
    } else {
      uiState.value.apiError = '网络错误，请检查您的网络连接';
    }
  } finally {
    uiState.value.loading = false;
  }
};
</script>

<style scoped>
  @import url('/static/css/general.css');
  @import url('/static/css/Register.css');
</style>