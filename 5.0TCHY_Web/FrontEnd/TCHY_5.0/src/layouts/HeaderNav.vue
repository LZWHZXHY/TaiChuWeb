<template>
  <header class="cyber-header">
    <div class="nav-container">
      
      <div class="logo-block" @click="navigateToHome">  
        <div class="logo-icon-box"> 
          <img src="/favicon.ico" class="logo-img" alt="LOGO">
        </div>
        <div class="logo-text-group">                          
          <span class="logo-main">太初寰宇</span>
          <span class="logo-sub">社区</span>
        </div>
      </div>

      <nav class="cyber-nav">
        <template v-for="(item, index) in filteredNavItems" :key="item.path || item.name || index">
          
          <div
            v-if="item.type === 'link'"
            class="nav-link-item"
            :class="{ 'active': isActive(item.path) }"
            @click="handleNavClick(item)"
          >
            <span class="bracket">[</span>
            <span class="link-text">{{ item.name }}</span>
            <span class="bracket">]</span>
          </div>
          
          <div 
            v-else-if="item.type === 'dropdown'" 
            class="dropdown-container" 
            @mouseleave="startCloseTimer"
          >
            <div 
              class="dropdown-trigger"
              :class="{ 'dropdown-trigger--active': activeDropdown === index }"
              @mouseenter="startOpenTimer(index)"
              @click="toggleDropdown(index)"
            >
              <span class="trigger-deco">[</span>
              <span class="trigger-text">{{ item.name }}</span>
              <span class="trigger-deco">]</span>
              
              <div class="dropdown-arrow-box">
                <span class="dropdown-arrow" :class="{ 'dropdown-arrow--open': activeDropdown === index }">▼</span>
              </div>
            </div>

            <transition name="cyber-clip">
              <div 
                v-if="activeDropdown === index"
                class="dropdown-menu"
                @mouseenter="cancelCloseTimer"
                @mouseleave="startCloseTimer"
              >
                <div class="menu-header-line"></div>
                <div 
                  v-for="subItem in item.items"
                  :key="subItem.path"
                  class="dropdown-item"
                  :class="{ 'dropdown-item--active': isActive(subItem.path) }"
                  @click="handleNavClick(subItem); activeDropdown = null"
                >
                  <span class="item-marker">></span>
                  <span class="dropdown-item-text">{{ subItem.name }}</span>
                </div>
              </div>
            </transition>
          </div>

        </template>
      </nav>

      <div class="cyber-actions">
       

        <div v-if="authStore.isAuthenticated" class="user-control-panel">
          
          <div class="tactical-btn-wrapper" @click="router.push('/creative-center')">
            <button class="tactical-btn gold">
              <span class="btn-label">创作中心</span>
            </button>
            <div class="btn-deco-line"></div>
          </div>

          <div class="tactical-btn-wrapper" @click="router.push('/lingmai')">
            <button class="tactical-btn amethyst">
              <span class="btn-label">灵脉空间</span>
            </button>
            <div class="btn-deco-line"></div>
          </div>

          <div class="tactical-btn-wrapper" @click="router.push('/workspace')">
            <button class="tactical-btn blue">
              <span class="btn-icon">☖</span>
              <span class="btn-label">多人协作</span>
            </button>
            <div class="btn-deco-line"></div>
          </div>

          <div class="user-trigger" @click.stop="toggleUserMenu" :class="{ 'menu-open': showUserMenu }">
            <div class="avatar-frame">
              <img 
                v-if="realAvatarUrl"
                :src="realAvatarUrl" 
                class="avatar-img" 
                alt="USER"
                @error="handleImageError"
              />
              <div v-else class="avatar-fallback">{{ userNameText.charAt(0) }}</div>
            </div>
            <div class="user-id-block">
              <span class="id-label">操作员</span>
              <span class="id-name">{{ userNameText }}</span>
            </div>
            <div v-if="localUnreadCount > 0" class="alert-tag">!</div>
          </div>

          <div class="music-wrapper">
            <button 
              class="cyber-music-widget" 
              :class="{ 'is-active': showMusicPanel }"
              @click.stop="toggleMusicPanel"
              title="音频系统"
            >
              <div class="mini-visualizer">
                <div v-for="n in 4" :key="n" class="vis-bar" :class="{ 'animating': musicStore.isPlaying }"></div>
              </div>
              <span class="btn-label">音乐播放器</span>
              
              <div class="mode-tag" :class="musicStore.isGlobalMode ? 'tag-global' : 'tag-local'">
                {{ musicStore.isGlobalMode ? '同步' : '私人' }}
              </div>
            </button>

            <transition name="panel-slide">
              <div v-show="showMusicPanel" class="global-music-panel" @click.stop>
                <div class="panel-header">
                  <span class="title">>> 播 放 模 块</span>
                  <button class="close-btn" @click="showMusicPanel = false">[X]</button>
                </div>
                
                <div class="panel-content">
                  <div class="cyber-audio-engine">
                    <audio 
                      ref="audioRef" 
                      :src="musicStore.currentTrack?.url"
                      crossorigin="anonymous"
                      @ended="handleEnded"
                    ></audio>

                    <div class="visualizer-container">
                      <canvas ref="canvasRef" width="280" height="60"></canvas>
                      <div class="track-info-overlay">
                        <span class="indicator" :class="{ 'active': musicStore.isPlaying }">▶</span>
                        <span class="track-name">{{ musicStore.currentTrack?.title || '未选择音轨' }}</span>
                      </div>
                    </div>

                    <div class="tactical-controls">
                      <button class="cyber-btn" @click="handleTogglePlay">
                        {{ musicStore.isPlaying ? '[ 暂停 ]' : '[ 播放 ]' }}
                      </button>
                      
                      <button 
                        class="cyber-btn" 
                        :class="{ 'sync-active': musicStore.isGlobalMode }"
                        @click="musicStore.toggleMode"
                      >
                        {{ musicStore.isGlobalMode ? '< 同步 >' : '< 私人 >' }}
                      </button>

                      <input 
                        type="range" 
                        min="0" max="1" step="0.01" 
                        v-model="musicStore.volume" 
                        @input="updateVolume"
                        class="cyber-slider"
                      >
                    </div>
                  </div>
                </div>

                <div class="panel-footer">
                  <span class="status-code">状态: {{ musicStore.isGlobalMode ? '加密同步中' : '仅本地模式' }}</span>
                </div>
              </div>
            </transition>
          </div>

          <transition name="scale-fade">
            <div v-if="showUserMenu" class="cyber-dropdown-menu">
              <div class="menu-header">// 用户菜单</div>
              <div class="menu-list">
                <div class="menu-row" @click="goToNewProfile">
                  <span class="row-label">> 我的主页</span>
                  <span class="row-icon">-></span>
                </div>
                
                <div class="menu-row" @click="router.push('/workspace'); showUserMenu = false;">
                  <span class="row-label">> 多人协作</span>
                  <span class="row-icon">☖</span>
                </div>

                <div class="menu-row" @click="goToCreativeCenter">
                  <span class="row-label">> 创作中心</span>
                  <span class="row-icon">◈</span>
                </div>

                <div class="menu-row" @click="router.push('/notifications'); showUserMenu = false;">
                  <span class="row-label">> 通知中心</span>
                  <span class="row-icon">📬</span>
                </div>

                <div class="menu-row" @click="goToNewSettings">
                  <span class="row-label">> 全局设置</span>
                  <span class="row-icon">-></span>
                </div>
                <div class="menu-divider">----------------</div>
                <div class="menu-row logout" @click="handleLogout">
                  <span class="row-label">>> 登出</span>
                </div>
              </div>
            </div>
          </transition>

          <div class="notification-wrapper">
            <button 
              class="cyber-icon-btn" 
              @click="router.push('/notifications')"
              title="通知中心"
            >
              <span class="btn-inner">📬</span>
              <div v-if="localUnreadCount > 0" class="mini-badge"></div>
            </button>
          </div>

          <div class="tactical-btn-wrapper" @click="router.push('/suggest')">
            <button class="tactical-btn red">
              <span class="btn-icon">✎</span>
              <span class="btn-label">反馈区</span>
            </button>
            <div class="btn-deco-line"></div>
          </div>

        </div>

        <div v-else class="auth-group">
          <button @click="handleLogin" class="cyber-btn-outline">登录</button>
          <button @click="handleRegister" class="cyber-btn-solid">注册</button>
        </div>

      </div>
    </div>
    <div class="header-bottom-border"></div>
  </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'

import { useAuthStore } from '@/utils/auth'
import { usePublisherStore } from '@/stores/publisher' 
import apiClient from '@/utils/api'
import { useMusicStore } from '@/stores/music'

const props = defineProps({
  navItems: { 
    type: Array, 
    default: () => [] 
  }
})
const emit = defineEmits(['nav-change', 'user-action'])

const router = useRouter()
const authStore = useAuthStore()
const publisherStore = usePublisherStore() 
const musicStore = useMusicStore()

const BASE_URL = 'https://bianyuzhou.com'

// --- 基础状态 ---
const userCount = ref(0)
const localUnreadCount = ref(0) 
const showUserMenu = ref(false)
const showMusicPanel = ref(false)
const avatarLoadError = ref(false)
let unreadTimer = null 

// --- Dropdown Menu 状态管理 ---
const activeDropdown = ref(null)
let dropdownOpenTimer = null
let dropdownCloseTimer = null

const startOpenTimer = (index) => {
  cancelCloseTimer()
  dropdownOpenTimer = setTimeout(() => {
    activeDropdown.value = index
  }, 150)
}

const startCloseTimer = () => {
  cancelOpenTimer()
  dropdownCloseTimer = setTimeout(() => {
    activeDropdown.value = null
  }, 200)
}

const cancelOpenTimer = () => {
  if (dropdownOpenTimer) { clearTimeout(dropdownOpenTimer); dropdownOpenTimer = null; }
}

const cancelCloseTimer = () => {
  if (dropdownCloseTimer) { clearTimeout(dropdownCloseTimer); dropdownCloseTimer = null; }
}

const toggleDropdown = (index) => {
  cancelOpenTimer()
  cancelCloseTimer()
  activeDropdown.value = activeDropdown.value === index ? null : index
}

// --- Music Player 逻辑 ---
const audioRef = ref(null)
const canvasRef = ref(null)
let audioCtx = null
let analyser = null
let source = null
let animationId = null

const initAudioEngine = () => {
  if (audioCtx) return 
  audioCtx = new (window.AudioContext || window.webkitAudioContext)()
  analyser = audioCtx.createAnalyser()
  analyser.fftSize = 128 

  source = audioCtx.createMediaElementSource(audioRef.value)
  source.connect(analyser)
  analyser.connect(audioCtx.destination)

  drawVisualizer()
}

const drawVisualizer = () => {
  if (!canvasRef.value) return
  
  const canvas = canvasRef.value
  const ctx = canvas.getContext('2d')
  const bufferLength = analyser.frequencyBinCount
  const dataArray = new Uint8Array(bufferLength)

  const draw = () => {
    animationId = requestAnimationFrame(draw)
    analyser.getByteFrequencyData(dataArray)

    ctx.fillStyle = 'rgba(244, 241, 234, 0.3)' 
    ctx.fillRect(0, 0, canvas.width, canvas.height)

    const barWidth = (canvas.width / bufferLength) * 2.5
    let x = 0

    for (let i = 0; i < bufferLength; i++) {
      const barHeight = (dataArray[i] / 255) * canvas.height
      const r = barHeight + 50
      const g = 50
      const b = 250 - (i * 2)

      ctx.fillStyle = musicStore.isGlobalMode ? '#f1c40f' : `rgb(${r},${g},${b})`
      ctx.fillRect(x, canvas.height - barHeight, barWidth, barHeight)
      x += barWidth + 1
    }
  }
  draw()
}

const handleTogglePlay = () => {
  if (!audioCtx) initAudioEngine()
  if (audioCtx.state === 'suspended') audioCtx.resume()
  musicStore.togglePlay()
}

const updateVolume = () => {
  if (audioRef.value) audioRef.value.volume = musicStore.volume
}

const handleEnded = () => {
  if (!musicStore.isGlobalMode) {
    musicStore.isPlaying = false;
  }
}

watch(() => musicStore.isPlaying, (newVal) => {
  if (!audioRef.value) return
  if (newVal) {
    audioRef.value.play().catch(e => {
      console.warn('播放被拦截:', e)
      musicStore.isPlaying = false
    })
  } else {
    audioRef.value.pause()
  }
})

// --- UI 交互逻辑 ---
const toggleMusicPanel = () => {
  showMusicPanel.value = !showMusicPanel.value
  if (showMusicPanel.value) showUserMenu.value = false 
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
  if (showUserMenu.value) showMusicPanel.value = false 
}

const closeAllMenus = (event) => {
  const target = event.target
  if (!target.closest('.user-control-panel') && !target.closest('.music-wrapper')) {
    showUserMenu.value = false
    showMusicPanel.value = false
  }
}

// --- ⚡ 核心数据处理修正 (全量驼峰) ---

// 在 HeaderNav.vue 中找到 fetchLatestUserInfo 函数，修改为：
const fetchLatestUserInfo = async () => {
  if (!authStore.isAuthenticated) return
  try {
    const res = await apiClient.get('/profile/detail') 
    if (res.data && res.data.success) {
      const userData = res.data.data
      
      // 🚀 核心修复：必须显式映射 rank 和 roleCodes
      authStore.user = { 
        ...authStore.user, 
        rank: userData.rank,          // 👈 必须加这一行
        roleCodes: userData.roleCodes, // 👈 建议也加上
        username: userData.username, 
        avatar: userData.avatar,    
        level: userData.level,
        title: userData.title,
        role: userData.role
      }
      console.log(">> [权限同步成功] Rank:", authStore.user.rank);
    }
  } catch (error) { 
    console.warn('User Info Sync Failed', error) 
  }
}

const filteredNavItems = computed(() => {
  const currentRole = authStore.user?.role || 0; 
  return props.navItems.filter(item => {
    if (item.minRole !== undefined) return currentRole >= item.minRole;
    return true;
  });
})

const fetchUnreadCount = async () => {
  if (!authStore.isAuthenticated) return
  try {
    const res = await apiClient.get('/Notification/unread-count')
    if (res.data.success) localUnreadCount.value = res.data.count
  } catch (err) {
    console.warn('Sync Signal Error:', err)
  }
}

const realAvatarUrl = computed(() => {
  // 🚀 修正：移除 Avatar 大写回退
  let path = authStore.user?.avatar || authStore.user?.logo
  if (!path || avatarLoadError.value) return null
  if (path.startsWith('http')) return path
  path = path.replace(/\\/g, '/')
  if (path.startsWith('/')) path = path.substring(1)
  if (!path.startsWith('uploads/')) path = `uploads/${path}`
  return `${BASE_URL}/${path}`
})

const userNameText = computed(() => {
  // 🚀 修正：移除 Username 大写回退
  return authStore.user?.username || authStore.user?.name || '游客'
})

const handleImageError = () => { avatarLoadError.value = true }

watch(() => authStore.user, () => { avatarLoadError.value = false }, { deep: true })

watch(() => authStore.isAuthenticated, (val) => {
  if (val) {
    fetchLatestUserInfo()
    fetchUnreadCount()
  }
}, { immediate: true })

const isActive = (path) => {
  if (!path) return false
  const currentPath = router.currentRoute.value.path
  if (path === '/') return currentPath === '/'
  return currentPath === path || currentPath.startsWith(path + '/')
}

const loadUserCount = async () => {
  try {
    const response = await apiClient.get('/Tool/user-count')
    if (response.data && typeof response.data.count === 'number') {
      userCount.value = response.data.count
    }
  } catch (err) {
    console.warn('Failed to sync user count:', err)
    userCount.value = 0 
  }
}

const handleNavClick = (item) => {
  emit('nav-change', item)
  if (item.path) router.push(item.path)
}

const navigateToHome = () => router.push('/')
const handleLogin = () => router.push('/login')
const handleRegister = () => router.push('/register')

const goToNewProfile = () => { showUserMenu.value = false; router.push('/profile/MEE') }
const goToNewSettings = () => { showUserMenu.value = false; router.push('/profile/NewSettings') }

const handleLogout = async () => {
  showUserMenu.value = false
  authStore.logout()
  localStorage.removeItem('auth_token')
  localStorage.removeItem('user')
  localUnreadCount.value = 0
  router.push('/')
}

onMounted(() => {
  loadUserCount()
  if (authStore.isAuthenticated) {
    fetchLatestUserInfo()
    fetchUnreadCount()
  }
  unreadTimer = setInterval(fetchUnreadCount, 60000)
  document.addEventListener('click', closeAllMenus)
  
  if (audioRef.value) {
    musicStore.initRadio(audioRef.value)
  }
})

onUnmounted(() => {
  if (unreadTimer) clearInterval(unreadTimer)
  document.removeEventListener('click', closeAllMenus)
  if (animationId) cancelAnimationFrame(animationId)
  if (audioCtx) audioCtx.close()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- Global Variables & Layout --- */
.cyber-header {
  --bg-color: #F4F1EA;
  --ink-black: #111111;
  --cyber-red: #D92323;
  --cyber-blue: #00A3FF;
  --font-mono: 'JetBrains Mono', monospace;
  --font-title: 'Anton', sans-serif;
  --bg-hover: #e0e0e0;
  
  position: fixed;
  top: 0;
  width: 100%;
  height: 72px;
  background-color: var(--bg-color);
  z-index: 1000;
  font-family: var(--font-mono);
  color: var(--ink-black);
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.header-bottom-border {
  position: absolute; bottom: 0; left: 0; width: 100%; height: 2px; background: var(--ink-black);
}

.nav-container {
  max-width: 100%; padding: 0 30px; height: 100%;
  display: flex; align-items: center; justify-content: space-between;
}

/* --- Logo --- */
.logo-block { display: flex; align-items: center; gap: 12px; cursor: pointer; user-select: none; }
.logo-icon-box {
  width: 36px; height: 36px; background: var(--ink-black); color: #fff;
  font-family: var(--font-title); font-size: 24px;
  display: flex; align-items: center; justify-content: center; transition: 0.2s;
}
.logo-icon-box .logo-img { width: 100%; height: 100%; object-fit: contain; display: block; }
.logo-block:hover .logo-icon-box { background: var(--cyber-red); transform: rotate(10deg); }
.logo-text-group { display: flex; flex-direction: column; line-height: 1; }
.logo-main { font-family: var(--font-title); font-size: 22px; letter-spacing: 1px; }
.logo-sub { font-size: 10px; color: #666; font-weight: 700; }

/* --- Navigation --- */
.cyber-nav { display: flex; align-items: center; gap: 20px; }
.nav-link-item { cursor: pointer; font-size: 14px; font-weight: 700; padding: 5px 0; color: #555; transition: 0.2s; }
.nav-link-item .bracket { opacity: 0; color: var(--cyber-red); transition: 0.2s; margin: 0 2px; }
.nav-link-item:hover, .nav-link-item.active { color: var(--ink-black); }
.nav-link-item:hover .bracket, .nav-link-item.active .bracket { opacity: 1; }

/* --- Dropdown Menu --- */
.dropdown-container { position: relative; display: inline-block; font-family: var(--font-mono); }
.dropdown-trigger {
  display: flex; align-items: center; gap: 8px; padding: 8px 12px;
  cursor: pointer; color: var(--ink-black); font-size: 14px; font-weight: 700;
  transition: all 0.2s cubic-bezier(0.25, 1, 0.5, 1);
  border: 2px solid transparent; background: transparent; user-select: none;
}
.dropdown-trigger:hover, .dropdown-trigger--active {
  background: var(--bg-color); border-color: var(--ink-black); color: var(--ink-black);
  box-shadow: 4px 4px 0 var(--cyber-red); transform: translate(-2px, -2px);
}
.dropdown-trigger--active { color: var(--cyber-red); border-color: var(--cyber-red); }
.trigger-deco { opacity: 0.3; font-weight: 400; transition: opacity 0.2s; }
.dropdown-trigger:hover .trigger-deco, .dropdown-trigger--active .trigger-deco { opacity: 1; color: var(--ink-black); }
.dropdown-arrow-box { display: flex; align-items: center; justify-content: center; width: 16px; height: 16px; }
.dropdown-arrow { font-size: 10px; transition: transform 0.2s ease; }
.dropdown-arrow--open { transform: rotate(180deg); color: var(--cyber-red); }

.dropdown-menu {
  position: absolute; top: calc(100% + 10px); left: 0; min-width: 180px;
  background: var(--bg-color); border: 2px solid var(--ink-black); box-shadow: 6px 6px 0 rgba(0, 0, 0, 0.2);
  padding: 0; z-index: 1000; display: flex; flex-direction: column;
}
.menu-header-line { height: 4px; background: var(--ink-black); width: 100%; }
.dropdown-item {
  padding: 12px 16px; cursor: pointer; color: var(--ink-black); font-size: 14px; font-weight: 500;
  transition: all 0.1s ease; display: flex; align-items: center; gap: 8px;
  border-bottom: 1px solid #ccc; position: relative; overflow: hidden;
}
.dropdown-item:last-child { border-bottom: none; }
.item-marker { opacity: 0; transform: translateX(-5px); transition: all 0.2s; color: var(--cyber-red); font-weight: bold; }
.dropdown-item:hover { background: var(--ink-black); color: var(--bg-color); padding-left: 20px; }
.dropdown-item:hover .item-marker { opacity: 1; transform: translateX(0); color: var(--cyber-red); }
.dropdown-item--active { background: var(--cyber-red); color: var(--bg-color); font-weight: 700; }
.dropdown-item--active .item-marker { opacity: 1; transform: translateX(0); color: var(--bg-color); }

.cyber-clip-enter-active, .cyber-clip-leave-active { transition: all 0.2s cubic-bezier(0.19, 1, 0.22, 1); }
.cyber-clip-enter-from, .cyber-clip-leave-to { opacity: 0; transform: translateY(-10px) scaleY(0.9); clip-path: inset(0 0 100% 0); }
.cyber-clip-enter-to, .cyber-clip-leave-from { opacity: 1; transform: translateY(0) scaleY(1); clip-path: inset(0 0 0 0); }

/* --- Global Actions --- */
.cyber-actions { display: flex; align-items: center; gap: 16px; }
.cyber-icon-btn {
  width: 32px; height: 32px; border: 2px solid var(--ink-black); background: transparent;
  cursor: pointer; font-family: var(--font-mono); font-weight: 700; font-size: 12px;
  display: flex; align-items: center; justify-content: center; transition: 0.2s; position: relative; 
}
.cyber-icon-btn:hover { background: var(--ink-black); color: #fff; }

.status-terminal { display: flex; align-items: center; gap: 6px; font-size: 12px; border: 1px dashed #999; padding: 4px 8px; background: rgba(0,0,0,0.03); }
.status-indicator { color: #00AA00; animation: blink 2s infinite; }
.status-terminal.clickable { cursor: pointer; transition: all 0.2s ease; }
.status-terminal.clickable:hover { background: var(--ink-black); color: #fff; border-style: solid; }

/* --- User Panel --- */
.user-control-panel { display: flex; align-items: center; gap: 12px; position: relative; }
.user-trigger {
  display: flex; align-items: center; gap: 10px; cursor: pointer; padding: 4px;
  border: 1px solid transparent; transition: 0.2s; position: relative;
}
.user-trigger:hover, .user-trigger.menu-open { background: #fff; border-color: var(--ink-black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }

.avatar-frame { width: 32px; height: 32px; border: 2px solid var(--ink-black); overflow: hidden; }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-fallback { width: 100%; height: 100%; background: var(--ink-black); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; }
.user-id-block { display: flex; flex-direction: column; line-height: 1.1; }
.id-label { font-size: 8px; color: #888; }
.id-name { font-size: 12px; font-weight: 700; max-width: 80px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.alert-tag {
  background: var(--cyber-red); color: #fff; font-size: 10px; width: 16px; height: 16px;
  display: flex; align-items: center; justify-content: center; font-weight: 700;
  position: absolute; top: -5px; right: -5px;
}

.notification-wrapper { position: relative; }
.mini-badge {
  position: absolute; top: -2px; right: -2px; width: 8px; height: 8px;
  background-color: var(--cyber-red); border-radius: 50%; border: 2px solid #F4F1EA;
  z-index: 5; box-shadow: 0 0 6px var(--cyber-red); animation: badge-pulse 2s infinite ease-in-out;
}
@keyframes badge-pulse { 0% { transform: scale(1); opacity: 1; } 50% { transform: scale(1.3); opacity: 0.7; } 100% { transform: scale(1); opacity: 1; } }

.cyber-dropdown-menu {
  position: absolute; top: 60px; right: 40%; width: 200px;
  background: #fff; border: 2px solid var(--ink-black); box-shadow: 6px 6px 0 rgba(0,0,0,0.2); z-index: 1001;
}
.menu-header { background: var(--ink-black); color: #fff; font-size: 10px; padding: 4px 8px; }
.menu-list { padding: 8px 0; }
.menu-row { display: flex; justify-content: space-between; padding: 8px 16px; cursor: pointer; font-size: 12px; font-weight: 700; color: #555; }
.menu-row:hover { background: #eee; color: var(--ink-black); }
.menu-row.logout:hover { background: var(--cyber-red); color: #fff; }
.menu-divider { text-align: center; color: #ddd; font-size: 10px; margin: 4px 0; overflow: hidden; }

.scale-fade-enter-active, .scale-fade-leave-active { transition: all 0.2s; }
.scale-fade-enter-from, .scale-fade-leave-to { opacity: 0; transform: translateY(-10px) scale(0.95); }

/* --- Tactical Buttons --- */
.tactical-btn-wrapper { position: relative; }
.tactical-btn {
  height: 36px; padding: 0 16px; background: var(--ink-black); color: #fff; border: none;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer;
  display: flex; align-items: center; gap: 6px; position: relative; transition: 0.1s;
  clip-path: polygon(10% 0, 100% 0, 100% 70%, 90% 100%, 0 100%, 0 30%);
}
.tactical-btn.red { background: var(--cyber-red); }
.tactical-btn.gold { background: #f1c40f; color: var(--ink-black); }
.tactical-btn.gold:hover { background: #d4ac0d; box-shadow: 3px 3px 0 var(--ink-black); }
.tactical-btn.blue { background: var(--cyber-blue); }
.tactical-btn.blue:hover { background: #0087D1; box-shadow: 3px 3px 0 var(--ink-black); }
.tactical-btn.amethyst { background: #9b59b6; } /* 补充遗失的紫水晶配色 */
.tactical-btn.amethyst:hover { background: #8e44ad; box-shadow: 3px 3px 0 var(--ink-black); }
.tactical-btn:hover { transform: translate(-2px, -2px); box-shadow: 3px 3px 0 var(--ink-black); }
.tactical-btn:active { transform: translate(0, 0); box-shadow: none; }
.btn-deco-line {
  position: absolute; bottom: -4px; right: -4px; width: 10px; height: 10px;
  border-right: 2px solid var(--ink-black); border-bottom: 2px solid var(--ink-black);
}

/* --- Authentication Buttons --- */
.auth-group { display: flex; gap: 10px; }
.cyber-btn-outline {
  background: transparent; border: 2px solid var(--ink-black); padding: 6px 16px;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer; transition: 0.2s;
}
.cyber-btn-outline:hover { background: var(--ink-black); color: #fff; }
.cyber-btn-solid {
  background: var(--ink-black); border: 2px solid var(--ink-black); color: #fff; padding: 6px 16px;
  font-family: var(--font-mono); font-weight: 700; font-size: 12px; cursor: pointer; transition: 0.2s;
}
.cyber-btn-solid:hover { background: var(--cyber-red); border-color: var(--cyber-red); }

@keyframes blink { 0% { opacity: 1; } 50% { opacity: 0; } 100% { opacity: 1; } }

/* ====== AUDIO SYSTEM WIDGET ====== */
.music-wrapper { position: relative; display: flex; align-items: center; }
.cyber-music-widget {
  display: flex; align-items: center; gap: 8px; height: 32px; padding: 0 10px;
  border: 2px solid var(--ink-black); background: transparent; cursor: pointer;
  font-family: var(--font-mono); font-weight: 700; color: var(--ink-black); transition: all 0.2s;
}
.cyber-music-widget:hover, .cyber-music-widget.is-active { background: var(--ink-black); color: #fff; }
.mini-visualizer { display: flex; align-items: flex-end; gap: 2px; height: 12px; }
.vis-bar { width: 3px; background: var(--cyber-blue); height: 3px; transition: height 0.1s; }
.vis-bar.animating { animation: bar-jump 0.5s infinite alternate ease-in-out; }
.vis-bar:nth-child(1) { animation-delay: 0.0s; }
.vis-bar:nth-child(2) { animation-delay: 0.2s; }
.vis-bar:nth-child(3) { animation-delay: 0.1s; }
.vis-bar:nth-child(4) { animation-delay: 0.3s; }
@keyframes bar-jump { 0% { height: 3px; } 100% { height: 12px; } }
.btn-label { font-size: 12px; }
.mode-tag { font-size: 8px; padding: 1px 4px; border: 1px solid currentColor; border-radius: 2px; line-height: 1; }
.tag-global { color: #f1c40f; border-color: #f1c40f; }
.tag-local { color: var(--cyber-blue); border-color: var(--cyber-blue); }

.global-music-panel {
  position: absolute; top: 48px; right: 0; width: 300px;
  background: var(--bg-color); border: 2px solid var(--ink-black); box-shadow: 6px 6px 0 rgba(0,0,0,0.2);
  z-index: 1002; padding: 12px; color: var(--ink-black);
}
.panel-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--ink-black); padding-bottom: 8px; margin-bottom: 12px; }
.panel-header .title { font-size: 12px; font-weight: 700; color: var(--cyber-red); }
.close-btn { background: transparent; border: none; font-family: var(--font-mono); font-weight: 700; cursor: pointer; color: var(--ink-black); }
.close-btn:hover { color: var(--cyber-red); }
.panel-content { min-height: 80px; display: flex; flex-direction: column; gap: 10px; }
.panel-footer { margin-top: 12px; border-top: 1px dotted var(--ink-black); padding-top: 8px; font-size: 9px; font-weight: 700; text-align: right; color: #666; }

.panel-slide-enter-active, .panel-slide-leave-active { transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1); }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateY(10px) scale(0.98); }

.cyber-audio-engine { display: flex; flex-direction: column; gap: 12px; }
.visualizer-container { position: relative; width: 100%; height: 60px; background: var(--ink-black); overflow: hidden; }
canvas { width: 100%; height: 100%; display: block; }
.track-info-overlay { position: absolute; top: 4px; left: 6px; font-size: 15px; color: #00eeff; font-family: var(--font-mono); text-shadow: 1px 1px 0 #ff0000; pointer-events: none; }
.indicator { color: #000; margin-right: 4px; }
.indicator.active { color: #ff0000; animation: blink 1s infinite; }
.tactical-controls { display: flex; align-items: center; gap: 8px; }
.cyber-btn {
  background: transparent; border: 1px solid var(--ink-black); color: var(--ink-black);
  font-family: var(--font-mono); font-size: 10px; font-weight: bold; border-radius: 4px;
  padding: 4px 8px; cursor: pointer; transition: 0.2s; width: 30%;
}
.cyber-btn:hover { background: var(--ink-black); color: #fff; }
.cyber-btn.sync-active { background: #f1c40f; border-color: #f1c40f; color: #111; }
.cyber-slider { flex-grow: 1; height: 2px; background: #ccc; appearance: none; outline: none; }
.cyber-slider::-webkit-slider-thumb { appearance: none; width: 8px; height: 12px; background: var(--ink-black); cursor: pointer; }

/* --- Responsive Rules --- */
@media (max-width: 1200px) {
  .tactical-btn.blue .btn-label, .tactical-btn.gold .btn-label { display: none; }
}
@media (max-width: 992px) {
  .tactical-btn .btn-label { display: none; }
  .tactical-btn { padding: 0 12px; }
}
@media (max-width: 768px) {
  .nav-container { padding: 0 15px; }
  .cyber-nav, .status-terminal, .id-label, .id-name, .logo-sub { display: none; }
}
</style>