<template>
  <div class="action-card-wrapper">
    <div class="action-content">
      
      <div class="action-row">
        
        <button 
          class="u-btn btn-focus" 
          :class="{ 'is-active': isFollowed }"
          @click="toggleFollow"
        >
          <span v-if="!isFollowed" class="btn-text main-txt">+ 关注</span>
          <div v-else class="fans-info">
            <span class="label">FANS</span>
            <span class="num">{{ formatFans(fansCount) }}</span>
          </div>
        </button>
        
        <button class="u-btn btn-msg">
          <span class="btn-text">私信</span>
        </button>

        <div class="more-container" ref="moreMenuRef">
          <button 
            class="more-btn" 
            @click="toggleMenu" 
            :class="{ 'menu-open': showMenu }"
          >
            <span class="dots">•••</span>
          </button>

          <transition name="fade-slide">
            <div v-if="showMenu" class="dropdown-menu">
              <div class="menu-item" @click="handleAction('block')">
                <span class="icon">⊘</span>
                <span class="txt">拉黑</span>
              </div>
              <div class="menu-item warning" @click="handleAction('report')">
                <span class="icon">⚠</span>
                <span class="txt">举报</span>
              </div>
            </div>
          </transition>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  fansCount: { type: Number, default: 12580 }
})

// === 关注逻辑 ===
const isFollowed = ref(false)
const toggleFollow = () => {
  isFollowed.value = !isFollowed.value
}
const formatFans = (num) => {
  return num > 9999 ? (num / 10000).toFixed(1) + 'w' : num
}

// === 菜单逻辑 ===
const showMenu = ref(false)
const moreMenuRef = ref(null)

const toggleMenu = () => {
  showMenu.value = !showMenu.value
}

const handleAction = (type) => {
  console.log(`Action triggered: ${type}`)
  showMenu.value = false // 点击后关闭菜单
}

// 点击外部关闭菜单
const handleClickOutside = (event) => {
  if (moreMenuRef.value && !moreMenuRef.value.contains(event.target)) {
    showMenu.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@500;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* === 容器 === */
.action-card-wrapper {
  width: 100%;
  box-sizing: border-box;
  background: transparent;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 10px;
}

.action-content {
  width: 100%;
}

/* === 主行布局 === */
.action-row {
  display: flex;
  align-items: stretch; /* 让按钮高度一致 */
  gap: 8px;
  width: 100%;
  height: 38px; /* 固定行高 */
  position: relative; /* 为下拉菜单定位 */
}

/* === 通用按钮样式 === */
.u-btn {
  border: 2px solid #2c3e50;
  background: rgba(255,255,255,0);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  /* 赛博切角 */
  clip-path: polygon(8px 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%, 0 8px);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  font-family: 'Noto Sans SC', sans-serif;
  padding: 0 12px;
}
.btn-text {
  font-size: 14px;
  font-weight: 800;
  color: #2c3e50;
  letter-spacing: 1px;
  white-space: nowrap;
}

/* 关注按钮 (Flex 2) */
.btn-focus {
  flex: 2; /* 占据较大比例 */
}
.btn-focus:hover {
  background: #2c3e50;
  transform: translateY(-2px);
}
.btn-focus:hover .main-txt { color: #e67e22; }

.btn-focus.is-active {
  background: #2c3e50;
}
.fans-info {
  display: flex;
  align-items: center;
  gap: 6px;
}
.fans-info .label { font-size: 10px; color: #aaa; font-family: 'Roboto Mono'; }
.fans-info .num { font-size: 14px; color: #e67e22; font-family: 'Roboto Mono'; font-weight: bold; }

/* 私信按钮 (Flex 1) */
.btn-msg {
  flex: 1;
}
.btn-msg:hover {
  background: #d35400;
  border-color: #d35400;
}
.btn-msg:hover .btn-text { color: #fff; }


/* === 更多菜单容器 === */
.more-container {
  position: relative;
  display: flex;
  align-items: center;
}

/* 更多按钮 (...) */
.more-btn {
  height: 100%;
  width: 32px;
  border: none;
  background: transparent;
  cursor: pointer;
  color: #999;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: all 0.2s;
}
.more-btn:hover, .more-btn.menu-open {
  background: rgba(44, 62, 80, 0.1);
  color: #2c3e50;
}
.dots {
  font-weight: bold;
  letter-spacing: 1px;
  transform: rotate(90deg); /* 竖向排列的点，或者去掉这行变成横向 */
  font-size: 14px;
}

/* === 下拉菜单 === */
.dropdown-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  width: 100px;
  background: #2c3e50;
  border: 1px solid #fff; /* 反色边框 */
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  z-index: 100;
  display: flex;
  flex-direction: column;
  padding: 4px;
  
  /* 赛博切角装饰 */
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%);
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  color: #ccc;
  font-size: 12px;
  font-family: 'Noto Sans SC', sans-serif;
  transition: all 0.2s;
  border-radius: 2px;
}

.menu-item:hover {
  background: rgba(255,255,255,0.1);
  color: #fff;
}

.menu-item.warning:hover {
  color: #e74c3c;
  background: rgba(231, 76, 60, 0.1);
}

.menu-item .icon {
  font-size: 14px;
  width: 16px;
  text-align: center;
}

/* === 动画效果 === */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.2s ease;
}

.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}
</style>