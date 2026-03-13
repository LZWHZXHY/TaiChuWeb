<template>
  <div class="security-setting-wrapper">

    <div class="scroll-container">
      <div class="main-content">
        
        <div class="layout-column">
          <div class="security-card" :class="{ 'is-active': expanded.password }">
            <div class="card-header" @click="toggleCard('password')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>密码修改</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.password }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.password }">
              <div class="options-inner">
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
                    <button class="save-button" @click="changePassword">
                      <span class="btn-text">修改密码</span>
                      <span class="btn-deco">➜</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="security-card" :class="{ 'is-active': expanded.email }">
            <div class="card-header" @click="toggleCard('email')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>邮箱验证</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.email }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.email }">
              <div class="options-inner">
                <div class="email-section">
                  <template v-if="!emailVerified">
                    <div class="form-group">
                      <label>邮箱地址</label>
                      <input 
                        v-model="emailInput" 
                        type="email" 
                        class="standard-input"
                        placeholder="请输入您的邮箱"
                      />
                    </div>
                    <div class="form-group">
                      <label>验证码</label>
                      <div class="input-with-btn">
                        <input 
                          v-model="emailCode" 
                          type="text" 
                          class="standard-input"
                          placeholder="6位验证码"
                          maxlength="6"
                        />
                        <button class="send-code-btn" @click="sendEmailCode">发送</button>
                      </div>
                    </div>
                    <div class="action-row">
                      <button class="save-button" @click="verifyEmailCode">
                        <span class="btn-text">验证并绑定</span>
                      </button>
                    </div>
                  </template>

                  <template v-else>
                    <div class="verified-status-box">
                      <div class="status-left">
                        <span class="status-dot safe"></span>
                        <span class="account-number">{{ maskedEmailAddress }}</span>
                      </div>
                      <button class="text-btn" @click="changeEmail">更换</button>
                    </div>
                  </template>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="layout-column">
          <div class="security-card" :class="{ 'is-active': expanded.phone }">
            <div class="card-header" @click="toggleCard('phone')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>手机验证</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.phone }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.phone }">
              <div class="options-inner">
                <div class="phone-section">
                  <template v-if="!phoneVerified">
                    <div class="form-group">
                      <label>手机号码</label>
                      <input 
                        v-model="phoneInput" 
                        type="text" 
                        class="standard-input"
                        placeholder="请输入手机号码"
                      />
                    </div>
                    <div class="form-group">
                      <label>验证码</label>
                      <div class="input-with-btn">
                        <input 
                          v-model="phoneCode" 
                          type="text" 
                          class="standard-input"
                          placeholder="6位验证码"
                          maxlength="6"
                        />
                        <button class="send-code-btn" @click="sendPhoneCode">发送</button>
                      </div>
                    </div>
                    <div class="action-row">
                      <button class="save-button" @click="verifyPhoneCode">
                        <span class="btn-text">验证并绑定</span>
                      </button>
                    </div>
                  </template>

                  <template v-else>
                    <div class="verified-status-box">
                      <div class="status-left">
                        <span class="status-dot safe"></span>
                        <span class="account-number">{{ maskedPhoneNumber }}</span>
                      </div>
                      <button class="text-btn" @click="changePhone">更换</button>
                    </div>
                  </template>
                </div>
              </div>
            </div>
          </div>

          <div class="security-card" :class="{ 'is-active': expanded.history }">
            <div class="card-header" @click="toggleCard('history')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>登录历史</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.history }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.history }">
              <div class="options-inner">
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
                          {{ session.location }} · {{ session.loginTime }}
                        </div>
                      </div>
                      <div class="session-status" :class="{ current: session.isCurrent }">
                        {{ session.isCurrent ? '当前会话' : '已过期' }}
                      </div>
                    </div>
                  </div>
                  
                  <div class="action-row" v-if="loginHistory.length > 1">
                    <button class="terminate-btn" @click="terminateOtherSessions">
                      <span class="btn-text">终止其他会话</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';

// 展开状态管理 (默认全部展开)
const expanded = reactive({
  password: true,
  phone: true,
  email: true,
  history: true
});

const toggleCard = (key) => {
  expanded[key] = !expanded[key];
};

// ================= 数据状态 =================
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
const maskedPhoneNumber = ref('');

// 邮箱验证状态
const emailVerified = ref(true); 
const emailInput = ref('user@example.com');
const emailCode = ref('');
const maskedEmailAddress = ref('us**@example.com');

// 静态的登录历史Mock数据
const loginHistory = ref([
  { deviceType: 'Windows PC', os: 'Windows 11', location: '中国 · 广东', loginTime: '2023-10-24 14:30', isCurrent: true },
  { deviceType: 'iPhone 14 Pro', os: 'iOS 17', location: '中国 · 广东', loginTime: '2023-10-23 09:15', isCurrent: false }
]);


// ================= 纯前端交互逻辑 =================
const changePassword = () => {
  if (!passwordForm.currentPassword || !passwordForm.newPassword) {
    alert('请填写完整密码信息');
    return;
  }
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    alert('两次输入的新密码不一致');
    return;
  }
  alert('密码修改成功 (模拟)');
  passwordForm.currentPassword = '';
  passwordForm.newPassword = '';
  passwordForm.confirmPassword = '';
};

// 掩码函数
const maskPhone = (phone) => phone.substring(0, 3) + '****' + phone.substring(7);
const maskEmailFunc = (email) => {
  const [local, domain] = email.split('@');
  return local.substring(0, 2) + '**@' + domain;
};

// 手机验证逻辑
const sendPhoneCode = () => {
  if (!phoneInput.value || phoneInput.value.length < 11) {
    alert('请输入正确的手机号');
    return;
  }
  alert('验证码已发送至 ' + phoneInput.value);
};

const verifyPhoneCode = () => {
  if (phoneCode.value.length === 6) {
    maskedPhoneNumber.value = maskPhone(phoneInput.value);
    phoneVerified.value = true;
  } else {
    alert('请输入6位验证码');
  }
};

const changePhone = () => {
  phoneVerified.value = false;
  phoneCode.value = '';
};

// 邮箱验证逻辑
const sendEmailCode = () => {
  if (!emailInput.value || !emailInput.value.includes('@')) {
    alert('请输入正确的邮箱');
    return;
  }
  alert('验证码已发送至 ' + emailInput.value);
};

const verifyEmailCode = () => {
  if (emailCode.value.length === 6) {
    maskedEmailAddress.value = maskEmailFunc(emailInput.value);
    emailVerified.value = true;
  } else {
    alert('请输入6位验证码');
  }
};

const changeEmail = () => {
  emailVerified.value = false;
  emailCode.value = '';
};

// 会话管理
const terminateOtherSessions = () => {
  if (confirm('确定要终止所有其他设备的会话吗？')) {
    loginHistory.value = loginHistory.value.filter(session => session.isCurrent);
    alert('其他会话已终止 (模拟)');
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;500;700;900&display=swap');

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
  overflow-x: hidden;
  scrollbar-gutter: stable; /* 核心修复：永远为滚动条预留空间，防止宽度突然变化挤压布局 */
  padding: 30px;
  padding-right: 24px; /* 补偿滚动条预留的 6px 宽度，使得两边看起来绝对居中 (30 - 6 = 24) */
}

.scroll-container::-webkit-scrollbar {
  width: 6px;
  background: transparent; /* 确保无滚动内容时轨道隐形 */
}

.scroll-container::-webkit-scrollbar-thumb {
  background: #ddd;
  border-radius: 3px;
}

/* ======== 核心布局：双列 Flex ======== */
.main-content {
  display: flex;
  align-items: flex-start;
  gap: 24px;
  padding-bottom: 30px;
}

.layout-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 24px;
  min-width: 0;
}

/* ======== 卡片样式 ======== */
.security-card {
  background: #F4F1EA;
  border-radius: 20px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.security-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
}

.security-card.is-active {
  background: #f7f5ef;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #333;
  cursor: pointer;
  user-select: none;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.card-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.deco-icon {
  font-size: 16px;
  color: #d35400;
  font-weight: bold;
}

.expand-icon {
  font-size: 18px;
  color: #888;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: inline-block;
}

.expand-icon.rotated {
  transform: rotate(180deg);
  color: #d35400;
}

/* ======== 折叠面板顺滑动画 ======== */
.options-wrapper {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open {
  grid-template-rows: 1fr;
}

.options-inner {
  min-height: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  opacity: 0;
  transform: translateY(-8px);
  transition: opacity 0.35s ease, transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open .options-inner {
  opacity: 1;
  transform: translateY(0);
}

.password-form, .email-section, .phone-section, .history-section {
  padding-top: 16px;
}

/* ======== 内部表单组件 ======== */
.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 13px;
  font-weight: 700;
  color: #555;
  margin-bottom: 8px;
}

.standard-input {
  width: 100%;
  padding: 12px 16px;
  background: #FFFDF8;
  border: 1px solid #e5e5e5;
  border-radius: 12px;
  font-size: 14px;
  color: #333;
  transition: all 0.2s;
  box-sizing: border-box;
}

.standard-input:focus {
  border-color: #d35400;
  background: #fff;
  box-shadow: 0 4px 12px rgba(211, 84, 0, 0.05);
  outline: none;
}

/* 输入框与按钮合并 */
.input-with-btn {
  display: flex;
  gap: 10px;
}

.input-with-btn .standard-input {
  flex: 1;
}

.send-code-btn {
  background: #fff;
  color: #333;
  border: 1px solid #e5e5e5;
  padding: 0 20px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.send-code-btn:hover {
  background: #f0f0f0;
  border-color: #ccc;
}

.action-row {
  display: flex;
  justify-content: flex-end;
  margin-top: 24px;
}

/* 按钮样式 */
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

.save-button:hover,
.terminate-btn:hover {
  background: #333;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
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

.terminate-btn {
  background: #fff;
  color: #e74c3c;
  border: 1px solid #ffcccc;
}

.terminate-btn:hover {
  background: #fff5f5;
  color: #c0392b;
}

/* 已绑定/验证状态框 */
.verified-status-box {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e5e5e5;
}

.status-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-dot.safe {
  background: #27ae60;
  box-shadow: 0 0 0 2px rgba(39, 174, 96, 0.2);
}

.account-number {
  font-family: 'Roboto Mono', monospace;
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.text-btn {
  background: transparent;
  border: none;
  color: #666;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: all 0.2s;
}

.text-btn:hover {
  background: #f0f0f0;
  color: #000;
}

/* 登录历史列表 */
.empty-history {
  text-align: center;
  color: #999;
  font-size: 13px;
  padding: 30px 0;
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
  border: 1px solid #e5e5e5;
}

.session-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.device-info {
  display: flex;
  gap: 8px;
}

.device-type, .os {
  font-size: 13px;
  color: #333;
  font-weight: 700;
}

.location-info {
  font-size: 12px;
  color: #888;
}

.session-status {
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 20px;
  background: #f5f5f5;
  color: #666;
  font-weight: 700;
}

.session-status.current {
  background: #e8f5e9;
  color: #27ae60;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .main-content {
    flex-direction: column;
  }
}
</style>