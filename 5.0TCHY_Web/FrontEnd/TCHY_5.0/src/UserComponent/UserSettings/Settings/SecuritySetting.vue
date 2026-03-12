<template>
  <div class="security-setting-wrapper">
    
    <div v-if="loading" class="loading-mask">
      <div class="spinner"></div>
      <span>加载中...</span>
    </div>

    <div class="scroll-container">
      <div class="main-content">
      <!-- 密码修改区域 -->
      <div class="security-card">
        <div class="card-header">
          <span class="deco-icon">⌬</span>
          <h3>密码修改</h3>
        </div>
        
        <div class="password-form">
          <div class="form-group">
            <label>当前密码</label>
            <input 
              v-model="passwordForm.currentPassword" 
              type="password" 
              class="standard-input"
              placeholder="请输入当前密码"
            />
          </div>
          
          <div class="form-group">
            <label>新密码</label>
            <input 
              v-model="passwordForm.newPassword" 
              type="password" 
              class="standard-input"
              placeholder="请输入新密码（至少6位）"
            />
          </div>
          
          <div class="form-group">
            <label>确认新密码</label>
            <input 
              v-model="passwordForm.confirmPassword" 
              type="password" 
              class="standard-input"
              placeholder="请再次输入新密码"
            />
          </div>
          
          <div class="action-row">
            <button 
              class="save-button" 
              @click="changePassword"
              :disabled="isChangingPassword"
            >
              <span class="btn-text">{{ isChangingPassword ? '修改中...' : '修改密码' }}</span>
              <span class="btn-deco" v-if="!isChangingPassword">➜</span>
            </button>
          </div>
        </div>
      </div>

      <!-- 手机验证区域 -->
      <div class="security-card">
        <div class="card-header">
          <span class="deco-icon">⌬</span>
          <h3>手机验证</h3>
        </div>
        
        <div class="phone-section">
          <div class="phone-status">
            <div class="status-indicator" :class="{ verified: phoneVerified }">
              {{ phoneVerified ? '已验证' : '未验证' }}
            </div>
            <div v-if="!phoneVerified" class="phone-input-section">
              <input 
                v-model="phoneInput" 
                type="text" 
                class="standard-input"
                placeholder="请输入手机号码"
              />
              <button 
                class="send-code-btn" 
                @click="sendPhoneCode"
                :disabled="isSendingPhoneCode || !isValidPhone(phoneInput)"
              >
                {{ isSendingPhoneCode ? '发送中...' : '发送验证码' }}
              </button>
            </div>
            <div v-else class="verified-info">
              <span class="phone-number">{{ maskedPhoneNumber }}</span>
              <button 
                class="change-phone-btn" 
                @click="changePhone"
              >
                更换手机号
              </button>
            </div>
          </div>
          
          <div v-if="showPhoneVerification" class="verification-section">
            <label>验证码</label>
            <input 
              v-model="phoneCode" 
              type="text" 
              class="standard-input"
              placeholder="请输入6位验证码"
              maxlength="6"
            />
            <div class="verification-actions">
              <button 
                class="verify-btn" 
                @click="verifyPhoneCode"
                :disabled="!phoneCode || phoneCode.length !== 6"
              >
                验证
              </button>
              <button 
                class="cancel-btn" 
                @click="cancelPhoneVerification"
              >
                取消
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 邮箱验证区域 -->
      <div class="security-card">
        <div class="card-header">
          <span class="deco-icon">⌬</span>
          <h3>邮箱验证</h3>
        </div>
        
        <div class="email-section">
          <div class="email-status">
            <div class="status-indicator" :class="{ verified: emailVerified }">
              {{ emailVerified ? '已验证' : '未验证' }}
            </div>
            <div v-if="!emailVerified" class="email-input-section">
              <input 
                v-model="emailInput" 
                type="email" 
                class="standard-input"
                placeholder="请输入邮箱地址"
              />
              <button 
                class="send-code-btn" 
                @click="sendEmailCode"
                :disabled="isSendingEmailCode || !isValidEmail(emailInput)"
              >
                {{ isSendingEmailCode ? '发送中...' : '发送验证码' }}
              </button>
            </div>
            <div v-else class="verified-info">
              <span class="email-address">{{ maskedEmailAddress }}</span>
              <button 
                class="change-email-btn" 
                @click="changeEmail"
              >
                更换邮箱
              </button>
            </div>
          </div>
          
          <div v-if="showEmailVerification" class="verification-section">
            <label>验证码</label>
            <input 
              v-model="emailCode" 
              type="text" 
              class="standard-input"
              placeholder="请输入6位验证码"
              maxlength="6"
            />
            <div class="verification-actions">
              <button 
                class="verify-btn" 
                @click="verifyEmailCode"
                :disabled="!emailCode || emailCode.length !== 6"
              >
                验证
              </button>
              <button 
                class="cancel-btn" 
                @click="cancelEmailVerification"
              >
                取消
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 登录历史区域 -->
      <div class="security-card">
        <div class="card-header">
          <span class="deco-icon">⌬</span>
          <h3>登录历史</h3>
        </div>
        
        <div class="history-section">
          <div v-if="loginHistory.length === 0" class="empty-history">
            暂无登录记录
          </div>
          
          <div v-else class="history-list">
            <div 
              v-for="(session, index) in loginHistory" 
              :key="index" 
              class="history-item"
            >
              <div class="session-info">
                <div class="device-info">
                  <span class="device-type">{{ session.deviceType }}</span>
                  <span class="os">{{ session.os }}</span>
                </div>
                <div class="location-info">
                  {{ session.location }} · {{ formatDate(session.loginTime) }}
                </div>
              </div>
              <div class="session-status" :class="{ current: session.isCurrent }">
                {{ session.isCurrent ? '当前会话' : '已过期' }}
              </div>
            </div>
          </div>
          
          <div class="action-row">
            <button 
              class="terminate-btn" 
              @click="terminateOtherSessions"
              :disabled="isTerminatingSessions"
            >
              <span class="btn-text">{{ isTerminatingSessions ? '处理中...' : '终止其他会话' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import apiClient from '@/utils/api';

// 加载状态
const loading = ref(true);
const isChangingPassword = ref(false);
const isSendingPhoneCode = ref(false);
const isSendingEmailCode = ref(false);
const isTerminatingSessions = ref(false);

// 密码表单
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
});

// 手机验证状态
const phoneVerified = ref(false);
const phoneInput = ref('');
const phoneCode = ref('');
const showPhoneVerification = ref(false);
const maskedPhoneNumber = ref('');

// 邮箱验证状态
const emailVerified = ref(false);
const emailInput = ref('');
const emailCode = ref('');
const showEmailVerification = ref(false);
const maskedEmailAddress = ref('');

// 登录历史
const loginHistory = ref([]);

// 初始化数据
const initData = async () => {
  try {
    loading.value = true;
    
    // 获取安全设置信息
    const [securityRes, historyRes] = await Promise.all([
      apiClient.get('/security/settings'),
      apiClient.get('/security/login-history')
    ]);
    
    if (securityRes.data && securityRes.data.success) {
      const securityData = securityRes.data.data;
      phoneVerified.value = securityData.phoneVerified || false;
      emailVerified.value = securityData.emailVerified || false;
      
      if (phoneVerified.value && securityData.phoneNumber) {
        maskedPhoneNumber.value = maskPhoneNumber(securityData.phoneNumber);
        phoneInput.value = securityData.phoneNumber;
      }
      
      if (emailVerified.value && securityData.email) {
        maskedEmailAddress.value = maskEmail(securityData.email);
        emailInput.value = securityData.email;
      }
    }
    
    if (historyRes.data && historyRes.data.success) {
      loginHistory.value = historyRes.data.data.sessions || [];
    }
  } catch (error) {
    console.error('获取安全设置失败:', error);
  } finally {
    loading.value = false;
  }
};

// 密码修改
const changePassword = async () => {
  if (!validatePasswordForm()) return;
  
  isChangingPassword.value = true;
  
  try {
    const response = await apiClient.post('/security/change-password', {
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword
    });
    
    if (response.data && response.data.success) {
      alert('密码修改成功');
      // 清空表单
      passwordForm.currentPassword = '';
      passwordForm.newPassword = '';
      passwordForm.confirmPassword = '';
    } else {
      alert(response.data.message || '密码修改失败');
    }
  } catch (error) {
    console.error('密码修改失败:', error);
    alert('网络错误，密码修改失败');
  } finally {
    isChangingPassword.value = false;
  }
};

// 密码表单验证
const validatePasswordForm = () => {
  if (!passwordForm.currentPassword) {
    alert('请输入当前密码');
    return false;
  }
  
  if (!passwordForm.newPassword) {
    alert('请输入新密码');
    return false;
  }
  
  if (passwordForm.newPassword.length < 6) {
    alert('新密码长度至少6位');
    return false;
  }
  
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    alert('两次输入的新密码不一致');
    return false;
  }
  
  return true;
};

// 手机号码验证
const isValidPhone = (phone) => {
  const phoneRegex = /^1[3-9]\d{9}$/;
  return phoneRegex.test(phone);
};

// 邮箱验证
const isValidEmail = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

// 掩码手机号
const maskPhoneNumber = (phone) => {
  if (!phone || phone.length < 11) return phone;
  return phone.substring(0, 3) + '****' + phone.substring(7);
};

// 掩码邮箱
const maskEmail = (email) => {
  if (!email || !email.includes('@')) return email;
  const [localPart, domain] = email.split('@');
  if (localPart.length <= 2) return email;
  const maskedLocal = localPart.substring(0, 2) + '*'.repeat(localPart.length - 2);
  return maskedLocal + '@' + domain;
};

// 发送手机验证码
const sendPhoneCode = async () => {
  if (!isValidPhone(phoneInput.value)) {
    alert('请输入有效的手机号码');
    return;
  }
  
  isSendingPhoneCode.value = true;
  
  try {
    const response = await apiClient.post('/security/phone/send-code', {
      phoneNumber: phoneInput.value
    });
    
    if (response.data && response.data.success) {
      showPhoneVerification.value = true;
      alert('验证码已发送到您的手机');
    } else {
      alert(response.data.message || '发送验证码失败');
    }
  } catch (error) {
    console.error('发送手机验证码失败:', error);
    alert('网络错误，发送验证码失败');
  } finally {
    isSendingPhoneCode.value = false;
  }
};

// 验证手机验证码
const verifyPhoneCode = async () => {
  if (!phoneCode.value || phoneCode.value.length !== 6) {
    alert('请输入6位验证码');
    return;
  }
  
  try {
    const response = await apiClient.post('/security/phone/verify', {
      phoneNumber: phoneInput.value,
      code: phoneCode.value
    });
    
    if (response.data && response.data.success) {
      phoneVerified.value = true;
      showPhoneVerification.value = false;
      phoneCode.value = '';
      maskedPhoneNumber.value = maskPhoneNumber(phoneInput.value);
      alert('手机验证成功');
    } else {
      alert(response.data.message || '验证码错误');
    }
  } catch (error) {
    console.error('验证手机验证码失败:', error);
    alert('网络错误，验证失败');
  }
};

// 取消手机验证
const cancelPhoneVerification = () => {
  showPhoneVerification.value = false;
  phoneCode.value = '';
};

// 更换手机号
const changePhone = () => {
  phoneVerified.value = false;
  maskedPhoneNumber.value = '';
};

// 发送邮箱验证码
const sendEmailCode = async () => {
  if (!isValidEmail(emailInput.value)) {
    alert('请输入有效的邮箱地址');
    return;
  }
  
  isSendingEmailCode.value = true;
  
  try {
    const response = await apiClient.post('/security/email/send-code', {
      email: emailInput.value
    });
    
    if (response.data && response.data.success) {
      showEmailVerification.value = true;
      alert('验证码已发送到您的邮箱');
    } else {
      alert(response.data.message || '发送验证码失败');
    }
  } catch (error) {
    console.error('发送邮箱验证码失败:', error);
    alert('网络错误，发送验证码失败');
  } finally {
    isSendingEmailCode.value = false;
  }
};

// 验证邮箱验证码
const verifyEmailCode = async () => {
  if (!emailCode.value || emailCode.value.length !== 6) {
    alert('请输入6位验证码');
    return;
  }
  
  try {
    const response = await apiClient.post('/security/email/verify', {
      email: emailInput.value,
      code: emailCode.value
    });
    
    if (response.data && response.data.success) {
      emailVerified.value = true;
      showEmailVerification.value = false;
      emailCode.value = '';
      maskedEmailAddress.value = maskEmail(emailInput.value);
      alert('邮箱验证成功');
    } else {
      alert(response.data.message || '验证码错误');
    }
  } catch (error) {
    console.error('验证邮箱验证码失败:', error);
    alert('网络错误，验证失败');
  }
};

// 取消邮箱验证
const cancelEmailVerification = () => {
  showEmailVerification.value = false;
  emailCode.value = '';
};

// 更换邮箱
const changeEmail = () => {
  emailVerified.value = false;
  maskedEmailAddress.value = '';
};

// 终止其他会话
const terminateOtherSessions = async () => {
  if (!confirm('确定要终止所有其他设备的会话吗？')) return;
  
  isTerminatingSessions.value = true;
  
  try {
    const response = await apiClient.post('/security/sessions/terminate-others');
    
    if (response.data && response.data.success) {
      // 重新加载登录历史
      const historyRes = await apiClient.get('/security/login-history');
      if (historyRes.data && historyRes.data.success) {
        loginHistory.value = historyRes.data.data.sessions || [];
      }
      alert('其他会话已终止');
    } else {
      alert(response.data.message || '终止会话失败');
    }
  } catch (error) {
    console.error('终止会话失败:', error);
    alert('网络错误，终止失败');
  } finally {
    isTerminatingSessions.value = false;
  }
};

// 格式化日期
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

onMounted(() => {
  initData();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.security-setting-wrapper {
  width: 100%;
  height: 100%;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Noto Sans SC', sans-serif;
  position: relative;
  overflow: hidden;
}

.scroll-container {
  height: 100%;
  overflow-y: auto;
  padding: 30px;
  padding-right: 10px;
}

/* 隐藏滚动条但保留功能 */
.scroll-container::-webkit-scrollbar {
  width: 6px;
}

.scroll-container::-webkit-scrollbar-thumb {
  background: #ddd;
  border-radius: 3px;
}

.loading-mask {
  position: absolute;
  inset: 0;
  background: rgba(255, 255, 255, 0.9);
  display: flex;
  flex-direction: column;
  gap: 10px;
  justify-content: center;
  align-items: center;
  z-index: 100;
}

.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid #eee;
  border-top-color: #000;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.main-content {
  display: flex;
  flex-direction: column;
  gap: 30px;
  min-height: 100%;
}

.security-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.2s ease;
}

.security-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #333;
  margin-bottom: 20px;
}

.card-header h3 {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.deco-icon {
  font-size: 16px;
  color: #d35400;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 700;
  color: #666;
  margin-bottom: 8px;
}

.standard-input {
  width: 100%;
  padding: 12px 16px;
  background: #FFFDF8;
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  font-size: 14px;
  color: #333;
  transition: all 0.2s;
  box-sizing: border-box;
}

.standard-input:focus {
  border-color: #000;
  background: #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  outline: none;
}

.action-row {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.save-button,
.terminate-btn {
  background: #000;
  color: #fff;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.save-button:hover:not(:disabled),
.terminate-btn:hover:not(:disabled) {
  background: #333;
  transform: translateY(-2px);
}

.save-button:disabled,
.terminate-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
}

.btn-text {
  font-family: 'Noto Sans SC', sans-serif;
}

.btn-deco {
  font-size: 16px;
  transition: transform 0.2s;
}

.save-button:hover .btn-deco {
  transform: translateX(4px);
}

/* 手机和邮箱验证样式 */
.phone-section,
.email-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.phone-status,
.email-status {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.status-indicator {
  font-size: 14px;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 20px;
  background: #f0f0f0;
  color: #666;
}

.status-indicator.verified {
  background: #e8f5e9;
  color: #2e7d32;
}

.phone-input-section,
.email-input-section {
  display: flex;
  gap: 10px;
  align-items: center;
}

.phone-input-section input,
.email-input-section input {
  flex: 1;
}

.phone-input-section input:focus,
.email-input-section input:focus {
  border-color: #000;
  background: #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  outline: none;
}

.send-code-btn {
  background: #000;
  color: #fff;
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.send-code-btn:hover:not(:disabled) {
  transform: scale(1.05);
}

.verified-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}

.phone-number,
.email-address {
  font-family: 'Roboto Mono', monospace;
  font-size: 12px;
  color: #333;
}

.change-phone-btn,
.change-email-btn {
  background: #000;
  color: #fff;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.change-phone-btn:hover,
.change-email-btn:hover {
  transform: scale(1.05);
}

.verification-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 20px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.verification-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-start;
}

.verify-btn,
.cancel-btn {
  background: #000;
  color: #fff;
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.cancel-btn {
  background: #666;
}

.verify-btn:hover:not(:disabled),
.cancel-btn:hover:not(:disabled) {
  transform: scale(1.05);
}

/* 登录历史样式 */
.history-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.empty-history {
  text-align: center;
  color: #ccc;
  font-size: 14px;
  padding: 40px;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.history-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.session-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.device-info {
  display: flex;
  gap: 8px;
}

.device-type,
.os {
  font-size: 12px;
  color: #666;
  font-weight: 700;
}

.location-info {
  font-size: 12px;
  color: #999;
}

.session-status {
  font-size: 12px;
  padding: 4px 12px;
  border-radius: 20px;
  background: #f0f0f0;
  color: #666;
}

.session-status.current {
  background: #e8f5e9;
  color: #2e7d32;
}
</style>