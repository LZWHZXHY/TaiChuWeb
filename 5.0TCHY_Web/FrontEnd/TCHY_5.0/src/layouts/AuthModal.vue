<!-- src/components/AuthModal.vue -->
<template>
  <div class="auth-modal" :class="{ active: isActive }">
    <div class="auth-container">
      <div class="auth-tabs">
        <button
          class="auth-tab"
          :class="{ active: activeTab === 'login' }"
          @click="switchTab('login')"
        >
          登录
        </button>
        <button
          class="auth-tab"
          :class="{ active: activeTab === 'register' }"
          @click="switchTab('register')"
        >
          注册
        </button>
      </div>

      <div class="auth-form">
        <!-- 登录表单 -->
        <form v-if="activeTab === 'login'" @submit.prevent="handleLogin">
          <div class="form-group">
            <label for="login-username">用户名或邮箱</label>
            <input
              type="text"
              id="login-username"
              class="form-control"
              v-model="loginUsername"
              required
              autocomplete="current-username"
            />
          </div>
          <div class="form-group">
            <label for="login-password">密码</label>
            <input
              type="password"
              id="login-password"
              class="form-control"
              v-model="loginPassword"
              required
              autocomplete="current-password"
            />
          </div>
          <div class="form-group">
            <button type="submit" class="btn">登录</button>
          </div>
          <div class="auth-footer">
            <span class="auth-link" @click="switchTab('register')">忘记密码？</span>
          </div>
        </form>

        <!-- 注册表单 -->
        <form v-else-if="activeTab === 'register'" @submit.prevent="handleRegister">
          <div class="form-group">
            <label for="register-username">用户名</label>
            <input
              type="text"
              id="register-username"
              class="form-control"
              v-model="registerUsername"
              required
            />
          </div>
          <div class="form-group">
            <label for="register-email">邮箱</label>
            <input
              type="email"
              id="register-email"
              class="form-control"
              v-model="registerEmail"
              required
              autocomplete="new-email"
            />
          </div>
          <div class="form-group">
            <label for="register-password">密码</label>
            <input
              type="password"
              id="register-password"
              class="form-control"
              v-model="registerPassword"
              required
              autocomplete="new-password"
            />
          </div>
          <div class="form-group">
            <label for="register-confirm-password">确认密码</label>
            <input
              type="password"
              id="register-confirm-password"
              class="form-control"
              v-model="registerConfirmPassword"
              required
              autocomplete="new-password"
            />
          </div>
          <div class="form-group">
            <button type="submit" class="btn">注册</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AuthModal',
  props: {
    isActive: {
      type: Boolean,
      default: false,
    },
  },
  emits: ['update:isActive'],
  data() {
    return {
      activeTab: 'login',
      // 登录表单数据
      loginUsername: '',
      loginPassword: '',
      // 注册表单数据
      registerUsername: '',
      registerEmail: '',
      registerPassword: '',
      registerConfirmPassword: '',
    };
  },
  methods: {
    switchTab(tab) {
      this.activeTab = tab;
    },
    handleLogin() {
      // 这里可以添加实际的登录逻辑，比如发送请求到后端
      console.log('登录:', {
        username: this.loginUsername,
        password: this.loginPassword,
      });
      // 登录成功后，可以关闭模态框
      this.$emit('update:isActive', false);
    },
    handleRegister() {
      // 简单的表单验证
      if (this.registerPassword !== this.registerConfirmPassword) {
        alert('密码和确认密码不一致！');
        return;
      }
      // 这里可以添加实际的注册逻辑，比如发送请求到后端
      console.log('注册:', {
        username: this.registerUsername,
        email: this.registerEmail,
        password: this.registerPassword,
      });
      // 注册成功后，可以关闭模态框
      this.$emit('update:isActive', false);
    },
  },
};
</script>

<style scoped>
.auth-modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.7);
  display: none; /* 默认隐藏 */
  align-items: center;
  justify-content: center;
  z-index: 1000; /* 确保在最上层 */
}

.auth-modal.active {
  display: flex;
}

.auth-container {
  background: var(--dark);
  padding: 30px;
  border-radius: 10px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
  color: var(--light);
}

.auth-tabs {
  display: flex;
  border-bottom: 2px solid var(--gray);
  margin-bottom: 20px;
}

.auth-tab {
  flex: 1;
  padding: 10px 0;
  text-align: center;
  background: none;
  border: none;
  color: var(--light);
  cursor: pointer;
  font-size: 1rem;
  transition: background 0.3s, color 0.3s;
}

.auth-tab:not(:last-child) {
  border-right: 1px solid var(--gray);
}

.auth-tab.active,
.auth-tab:hover {
  background: var(--primary);
  color: var(--light);
}

.auth-form .form-group {
  margin-bottom: 15px;
}

.auth-form label {
  display: block;
  margin-bottom: 5px;
  font-size: 0.9rem;
  color: var(--gray);
}

.auth-form .form-control {
  width: 100%;
  padding: 10px 12px;
  background: var(--darker);
  border: 1px solid var(--gray);
  border-radius: 5px;
  color: var(--light);
  font-size: 1rem;
}

.auth-form .form-control:focus {
  border-color: var(--primary);
  outline: none;
}

.auth-form button[type="submit"] {
  width: 100%;
  padding: 12px;
  background: var(--primary);
  border: none;
  border-radius: 5px;
  color: var(--light);
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s;
}

.auth-form button[type="submit"]:hover {
  background: var(--primary-dark);
}

.auth-footer {
  margin-top: 15px;
  text-align: center;
  font-size: 0.9rem;
}

.auth-link {
  color: var(--secondary);
  cursor: pointer;
  text-decoration: underline;
}

.auth-link:hover {
  color: var(--light);
}
</style>