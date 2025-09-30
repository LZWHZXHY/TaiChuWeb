<template>
  <div class="big-container" ref="bigContainer">
    <div class="login-container">
      <div class="login-header">
        <h2>太初寰宇</h2>
        <p>欢迎回来，请在这里登录你的账户~</p>
      </div>
      
      <div class="alert-container">
        <transition name="fade">
          <div v-if="alert.show" :class="`alert ${alert.type}`">
            <i :class="`fa ${alert.type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}`"></i>
            <div class="alert-message">{{ alert.message }}</div>
            <span class="alert-close" @click="closeAlert">&times;</span>
          </div>
        </transition>
      </div>
      
      <form @submit.prevent="handleLogin">
        <div :class="['form-group', usernameError ? 'error' : '']" id="UserName">
          <label for="username" class="UserName">用户名/邮箱 ↓</label>
          <div class="input-wrapper">
            <i class="fa fa-user"></i>
            <input 
              type="text" 
              id="username" 
              v-model="username" 
              placeholder="输入用户名或邮箱地址"
              autocomplete="username" 
              required
            />
          </div>
          <div class="error-message" v-if="usernameError">{{ usernameError }}</div>
        </div>
        
        <div :class="['form-group', passwordError ? 'error' : '']" id="PassWord">
          <label for="password">密码</label>
          <div class="input-wrapper">
            <i class="fa fa-lock"></i>
            <input 
              :type="showPassword ? 'text' : 'password'" 
              id="password" 
              v-model="password" 
              placeholder="输入您的密码" 
              autocomplete="current-password" 
              required
            />
            
            <button 
              type="button" 
              class="password-toggle-btn" 
              @click.stop.prevent="togglePassword"
              tabindex="-1"
              aria-label="切换密码显示"
            >
              <i :class="['fa', showPassword ? 'fa-eye-slash' : 'fa-eye']"></i>
            </button>
          </div>
          <div class="error-message" v-if="passwordError">{{ passwordError }}</div>
        </div>
        
        <div class="remember-forgot">
          <!-- 添加记住我功能 -->
          <div class="remember-me">
            <input type="checkbox" id="remember-me" v-model="rememberMe">
            <label for="remember-me">记住我</label>
          </div>
          
          <div class="forgot-password">
            <router-link to="/forgot-password">你忘记了你的密码？欸！那我们也没办法帮你找回，因为我们还没做密码找回（都说没做啦，别点）</router-link>
          </div>
        </div>
        
        <button 
          type="submit" 
          class="login-button" 
          :class="{ 'loading': loading }"
          :disabled="loading"
        >
          <span>{{ loading ? '正在登录...' : '登录' }}</span>
          <div class="spinner"></div>
        </button>
      </form>
      
      <div class="register-link">
        还没有账户? &nbsp;&nbsp;&nbsp;&nbsp;<router-link to="/Register">立即注册</router-link>
      </div>

      <div class="cannot-login">
        有Bug？那必不可能！你真登陆不了就进反馈群！ 
        <a href="https://qm.qq.com/q/FdynxJj7iM" target="_blank"> 反馈群</a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from './../store/user' // 导入Pinia用户存储
import { authApi } from '../../server/api'

// 登录表单相关响应式数据
const username = ref('')
const password = ref('')
const loading = ref(false)
const rememberMe = ref(false) // 记住我功能
const showPassword = ref(false)
const usernameError = ref('')
const passwordError = ref('')
const router = useRouter()
const userStore = useUserStore()

const alert = reactive({
  show: false,
  type: 'error',
  message: ''
})

// 令牌刷新定时器
let tokenRefreshTimer = null

// 从本地存储加载记住我的凭据
onMounted(() => {
  // Font Awesome检测
  const isFontAwesomeLoaded = document.querySelector('link[href*="font-awesome"]') !== null || 
                             document.querySelector('link[href*="fontawesome"]') !== null;
  if (!isFontAwesomeLoaded) {
    console.warn('警告: 未检测到Font Awesome，图标可能无法正常显示。请确保在index.html中引入Font Awesome库。');
  }
  
  // 加载保存的凭据
  const savedCredentials = localStorage.getItem('tchy_credentials');
  if (savedCredentials) {
    try {
      const { username: savedUsername, password: savedPassword } = JSON.parse(savedCredentials);
      username.value = savedUsername;
      password.value = savedPassword;
      rememberMe.value = true;
    } catch (e) {
      console.error('无法加载保存的凭据:', e);
    }
  }
  
  // 启动令牌刷新机制
  startTokenRefreshCheck();
})

// 启动令牌刷新检查
function startTokenRefreshCheck() {
  // 清除现有的定时器
  if (tokenRefreshTimer) clearInterval(tokenRefreshTimer);
  
  // 每5分钟检查一次令牌是否需要刷新
  tokenRefreshTimer = setInterval(async () => {
    if (userStore.isAuthenticated) {
      try {
        // 检查令牌是否即将过期
        const token = localStorage.getItem('authToken');
        if (token) {
          const payload = JSON.parse(atob(token.split('.')[1]));
          const expiryTime = payload.exp * 1000; // 转换为毫秒
          const currentTime = Date.now();
          
          // 如果令牌在15分钟内过期，则刷新它
          if (expiryTime - currentTime < 15 * 60 * 1000) {
            console.log('检测到令牌即将过期，开始刷新...');
            await refreshToken();
          }
        }
      } catch (e) {
        console.error('令牌检查失败:', e);
      }
    }
  }, 5 * 60 * 1000); // 每5分钟检查一次
}

// 刷新令牌函数
async function refreshToken() {
  try {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      console.log('没有可用的刷新令牌');
      return;
    }
    
    // 调用后端API刷新令牌
    const response = await authApi.refreshToken({
      accessToken: localStorage.getItem('authToken'),
      refreshToken: refreshToken
    });
    
    if (response.success) {
      console.log('令牌刷新成功');
      
      // 更新访问令牌
      localStorage.setItem('authToken', response.accessToken);
      
      // 如果有新的刷新令牌，更新它
      if (response.refreshToken) {
        localStorage.setItem('refreshToken', response.refreshToken);
      }
      
      // 更新用户存储中的令牌
      userStore.token = response.accessToken;
    } else {
      console.log('令牌刷新失败:', response.message);
      // 刷新失败时执行登出
      userStore.logout();
      router.push('/login.html');
    }
  } catch (error) {
    console.error('刷新令牌时出错:', error);
    // 发生错误时执行登出
    userStore.logout();
    router.push('/login.html');
  }
}

function showAlert(type, message) {
  alert.show = true
  alert.type = type
  alert.message = message

  if (type === 'success') {
    setTimeout(() => {
      alert.show = false
    }, 3000)
  }
}

function closeAlert() {
  alert.show = false
}

function togglePassword() {
  showPassword.value = !showPassword.value
  setTimeout(() => {
    document.getElementById('password').focus()
  }, 0)
}

function validateForm() {
  let isValid = true

  usernameError.value = ''
  passwordError.value = ''

  if (!username.value.trim()) {
    usernameError.value = '请输入用户名或邮箱'
    isValid = false
  }

  if (!password.value) {
    passwordError.value = '请输入密码'
    isValid = false
  } else if (password.value.length < 6) {
    passwordError.value = '密码长度不能小于6位'
    isValid = false
  }

  return isValid
}

async function handleLogin() {
  if (!validateForm()) return

  loading.value = true
  
  try {
    // 使用正确的 API 调用方式
    const response = await authApi.login({
      username: username.value,
      password: password.value,
      rememberMe: rememberMe.value // 添加记住我标志
    })
    
    console.log('登录响应数据:', response)


    



 

    
    
    if (response.success) {


      

      showAlert('success', '登录成功，正在跳转...')
      
      console.log('将进入if response')


      // 检查令牌是否存在
      if (!response.token && !response.accessToken) {
        throw new Error('服务器未返回令牌')
      }
      
      // 获取令牌（支持多种可能的字段名）
      const token = response.token || response.accessToken
      const refreshToken = response.refreshToken // 获取刷新令牌
      




      





      // 保存用户信息
      userStore.login({
        id: response.user.id,
        username: response.user.username,
        points: response.user.points,
        level: response.user.level,
        exp: response.user.exp,
        title: response.user.title,
        logo: response.user.logo,
        background: response.user.background,
        email: response.user.email,
        rank: response.user.rank || 0,
        avatar: response.user.avatar || '',
        lastLogin: new Date(),
        byd: response.user.byd,        // <--- 新增
        creater: response.user.creater // <--- 新增，如果有的话
        
      }, token)



      console.log('断点 ====================================')
      //await new Promise(resolve => setTimeout(resolve, 10000))

      
      
      // 保存令牌到 localStorage
      console.log('login获得token:', token)
      localStorage.setItem('authToken', token)
      console.log('localStorage已更新:', localStorage.getItem('authToken'))
      
      // 保存刷新令牌
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
      }
      
      // 检查记住我选项并保存凭据
      if (rememberMe.value) {
        localStorage.setItem('tchy_credentials', JSON.stringify({
          username: username.value,
          password: password.value
        }));
      } else {
        localStorage.removeItem('tchy_credentials');
      }
      
      console.log('断点2 ====================================')
      //await new Promise(resolve => setTimeout(resolve, 10000))

      // 启动令牌刷新机制
      startTokenRefreshCheck();
      
      // 修复跳转问题 - 保留重定向参数
      const redirect = new URLSearchParams(window.location.search).get('redirect');
      if (redirect) {
        window.location.href = redirect;
      } else {
        window.location.href = '/TCHY.html';
      }
    } else {
      console.log('response.message！！！！！！！！！！！！！！！！！！！！！！！！');
      await new Promise(resolve => setTimeout(resolve, 100000))
      throw new Error(response.message || '登录失败')
      
    
    }
  } catch (error) {
    console.log('登录失败!!!!!!!!!!!!');
    console.error('登录错误详情:', error);
    
    showAlert('error', `登录失败: ${error.message}`)
  } finally {
    loading.value = false
  }
}

// 清除定时器
onBeforeUnmount(() => {
  if (tokenRefreshTimer) clearInterval(tokenRefreshTimer);
});
// ------------------- 背景动画特效（方法二）-------------------
const bigContainer = ref(null);

const MAX_SHIFT = 80;
const BASE_X = 40;
const BASE_Y = 50;

let targetX = BASE_X, targetY = BASE_Y;
let currentX = BASE_X, currentY = BASE_Y;
let animationFrameId = null;

const handleMouseMove = (e) => {
  const container = bigContainer.value;
  if (!container) return;
  const rect = container.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const y = e.clientY - rect.top;
  const percentX = x / rect.width;
  const percentY = y / rect.height;
  targetX = BASE_X + (percentX - 0.5) * MAX_SHIFT;
  targetY = BASE_Y + (percentY - 0.5) * MAX_SHIFT;
  startAnimation(container);
};

const startAnimation = (container) => {
  if (animationFrameId) return;
  const animate = () => {
    currentX += (targetX - currentX) * 0.18;
    currentY += (targetY - currentY) * 0.18;
    container.style.backgroundPosition = `${currentX}% ${currentY}%`;
    if (Math.abs(targetX - currentX) > 0.1 || Math.abs(targetY - currentY) > 0.1) {
      animationFrameId = requestAnimationFrame(animate);
    } else {
      animationFrameId = null;
    }
  };
  animate();
};

// 方法二：只在鼠标离开整个窗口时回正
const handleWindowMouseOut = (e) => {
  if (!e.relatedTarget || e.relatedTarget.nodeName === "HTML") {
    const container = bigContainer.value;
    if (!container) return;
    targetX = BASE_X;
    targetY = BASE_Y;
    startAnimation(container);
  }
};

onMounted(() => {
  const container = bigContainer.value;
  if (!container) return;
  container.addEventListener('mousemove', handleMouseMove);
  window.addEventListener('mouseout', handleWindowMouseOut);
});

onBeforeUnmount(() => {
  const container = bigContainer.value;
  if (container) {
    container.removeEventListener('mousemove', handleMouseMove);
  }
  window.removeEventListener('mouseout', handleWindowMouseOut);
  if (animationFrameId) cancelAnimationFrame(animationFrameId);
});
</script>

<style scoped>
  @import url('/static/css/general.css');
  @import url('/static/css/login.css');
  @import url('/static/css/mobileCSS/loginMobile.css');
  
  /* 添加记住我样式 */
  .remember-me {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .remember-me input[type="checkbox"] {
    width: 16px;
    height: 16px;
    cursor: pointer;
  }
  
  .remember-me label {
    cursor: pointer;
    font-size: 0.95rem;
  }
</style>