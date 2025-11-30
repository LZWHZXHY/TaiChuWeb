<template>
  <div v-if="show" class="mw-overlay" role="dialog" aria-modal="true" aria-label="移动端提示">
    <div class="mw-card">
      <h3 class="mw-title">建议使用桌面端浏览</h3>
      <p class="mw-message">
        检测到您正在使用手机访问本页面。为了获得更完整的页面布局和功能，建议切换到浏览器的“桌面模式（Desktop site）”，或在电脑上打开本页面。
      </p>

      <div class="mw-actions">
        <button class="btn primary" @click="copyLink">
          复制链接并在电脑打开
        </button>
        <button class="btn" @click="showInstructions = !showInstructions">
          如何切换到桌面模式
        </button>
      </div>

      <transition name="fade">
        <div v-if="showInstructions" class="mw-instructions">
          <strong>Chrome（Android）</strong>: 页面右上角菜单 → 选择“桌面网站”或“Desktop site”。<br />
          <strong>Safari（iOS）</strong>: 长按刷新按钮或地址栏左侧的“aA”图标 → 选择“请求桌面网站”。<br />
          <strong>其它浏览器</strong>: 浏览器菜单中一般会有“桌面网站 / Desktop site”的选项。
        </div>
      </transition>

      <div class="mw-footer">
        <button class="btn" @click="dismiss">我知道了</button>
        <button class="btn secondary" @click="neverShow">不再提示</button>
      </div>

      <div v-if="copied" class="mw-copied">链接已复制到剪贴板</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const show = ref(false)
const showInstructions = ref(false)
const copied = ref(false)
const SESSION_DISMISS_KEY = 'mobileWarning.dismissed'
const LOCAL_NEVER_KEY = 'mobileWarning.neverShow'

// 是否强制显示（方便在开发者工具测试）：url 参数 ?showMobileWarning=1
function forcedByQuery() {
  try {
    return new URL(location.href).searchParams.get('showMobileWarning') === '1'
  } catch (e) {
    return false
  }
}

function isMobileDevice() {
  if (typeof navigator === 'undefined' || typeof window === 'undefined') return false
  const ua = navigator.userAgent || ''
  const mobileUA = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i
  const byUA = mobileUA.test(ua)
  const byWidth = window.matchMedia && window.matchMedia('(max-width: 768px)').matches
  return byUA || byWidth
}

function shouldShow() {
  if (forcedByQuery()) {
    console.debug('MobileWarning: forced to show via URL param')
    return true
  }
  try {
    const never = localStorage.getItem(LOCAL_NEVER_KEY)
    const dismissed = sessionStorage.getItem(SESSION_DISMISS_KEY)
    if (never === '1' || dismissed === '1') return false
  } catch (e) {
    console.debug('MobileWarning: storage access error', e)
  }
  return isMobileDevice()
}

let mql = null
function handleViewportChange(e) {
  const willShow = shouldShow()
  console.debug('MobileWarning: viewport change => willShow=', willShow, 'matchMedia=', mql ? mql.matches : 'n/a')
  show.value = willShow
}

onMounted(() => {
  console.debug('MobileWarning: mounted, userAgent=', navigator?.userAgent)
  // 初始检测（短延迟以避免阻塞渲染）
  setTimeout(() => {
    const result = shouldShow()
    console.debug('MobileWarning: initial shouldShow=', result)
    if (result) show.value = true
  }, 100)

  if (window.matchMedia) {
    mql = window.matchMedia('(max-width: 768px)')
    if (mql.addEventListener) {
      mql.addEventListener('change', handleViewportChange)
    } else if (mql.addListener) {
      mql.addListener(handleViewportChange)
    }
  }
  window.addEventListener('resize', handleViewportChange)
  window.addEventListener('orientationchange', handleViewportChange)
})

onBeforeUnmount(() => {
  if (mql) {
    if (mql.removeEventListener) {
      mql.removeEventListener('change', handleViewportChange)
    } else if (mql.removeListener) {
      mql.removeListener(handleViewportChange)
    }
  }
  window.removeEventListener('resize', handleViewportChange)
  window.removeEventListener('orientationchange', handleViewportChange)
})

function dismiss() {
  try {
    sessionStorage.setItem(SESSION_DISMISS_KEY, '1')
  } catch (e) {}
  show.value = false
}

function neverShow() {
  try {
    localStorage.setItem(LOCAL_NEVER_KEY, '1')
  } catch (e) {}
  show.value = false
}

async function copyLink() {
  try {
    await navigator.clipboard.writeText(location.href)
    copied.value = true
    setTimeout(() => (copied.value = false), 1600)
  } catch (e) {
    alert('复制失败，请手动复制地址栏链接：' + location.href)
  }
}
</script>

<style scoped>
.mw-overlay {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(10, 15, 25, 0.45);
  backdrop-filter: blur(3px);
  z-index: 12000;
  padding: 1rem;
}

.mw-card {
  width: 100%;
  max-width: 520px;
  background: #ffffff;
  border-radius: 10px;
  padding: 1.25rem;
  box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12);
  color: #0f172a;
  position: relative;
}

.mw-title { margin: 0 0 0.5rem; font-size: 1.125rem; }
.mw-message { margin: 0 0 1rem; color: #475569; line-height: 1.5; }

.mw-actions { display:flex; gap:0.5rem; flex-wrap:wrap; margin-bottom:0.75rem; }
.btn { padding:0.5rem 0.75rem; border-radius:6px; border:1px solid #cbd5e1; background:#fff; color:#0f172a; cursor:pointer; font-size:0.95rem; }
.btn.primary { background:#0ea5a3; color:#fff; border-color:rgba(0,0,0,0.04); }
.btn.secondary { background:#f1f5f9; }

.mw-instructions { margin:0.5rem 0; padding:0.6rem; background:#f8fafc; border-radius:6px; font-size:0.9rem; color:#344054; }
.mw-footer { display:flex; justify-content:flex-end; gap:0.5rem; margin-top:0.25rem; }

.mw-copied { position:absolute; left:50%; transform:translateX(-50%); bottom:-1.6rem; background:#0ea5a3; color:white; padding:0.25rem 0.6rem; border-radius:6px; font-size:0.85rem; }

/* 过渡 */
.fade-enter-active, .fade-leave-active { transition: all .18s ease; }
.fade-enter-from, .fade-leave-to { opacity:0; transform:translateY(-6px); }
</style>