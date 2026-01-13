<template>
  <div class="dropdown-container" @mouseleave="startCloseTimer">
    <div 
      class="dropdown-trigger"
      :class="{ 'dropdown-trigger--active': isOpen }"
      @mouseenter="startOpenTimer"
      @click="toggleDropdown"
    >
      <span class="trigger-deco">[</span>
      <span class="trigger-text">{{ triggerText }}</span>
      <span class="trigger-deco">]</span>
      
      <div class="dropdown-arrow-box">
        <span class="dropdown-arrow" :class="{ 'dropdown-arrow--open': isOpen }">▼</span>
      </div>
    </div>

    <transition name="cyber-clip">
      <div 
        v-if="isOpen"
        class="dropdown-menu"
        @mouseenter="cancelCloseTimer"
        @mouseleave="startCloseTimer"
      >
        <div class="menu-header-line"></div>
        
        <div 
          v-for="item in items"
          :key="item.path"
          class="dropdown-item"
          :class="{ 'dropdown-item--active': isActive(item.path) }"
          @click="handleItemClick(item)"
        >
          <span class="item-marker">></span>
          <span class="dropdown-item-text">{{ $t(item.name) }}</span>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  items: {
    type: Array,
    default: () => []
  },
  triggerText: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['item-click'])

const router = useRouter()
const isOpen = ref(false)
let openTimer = null
let closeTimer = null

const currentPath = computed(() => router.currentRoute.value.path)

const isActive = (path) => {
  if (path === '/') {
    return currentPath.value === '/'
  }
  return currentPath.value === path || currentPath.value.startsWith(path + '/')
}

const startOpenTimer = () => {
  cancelCloseTimer()
  openTimer = setTimeout(() => {
    isOpen.value = true
  }, 150) // 稍微加快响应速度
}

const startCloseTimer = () => {
  cancelOpenTimer()
  closeTimer = setTimeout(() => {
    isOpen.value = false
  }, 200)
}

const cancelOpenTimer = () => {
  if (openTimer) {
    clearTimeout(openTimer)
    openTimer = null
  }
}

const cancelCloseTimer = () => {
  if (closeTimer) {
    clearTimeout(closeTimer)
    closeTimer = null
  }
}

const toggleDropdown = () => {
  cancelOpenTimer()
  cancelCloseTimer()
  isOpen.value = !isOpen.value
}

const handleItemClick = (item) => {
  if (item.path) {
    router.push(item.path)
    emit('item-click', item)
  }
  isOpen.value = false
}
</script>

<style scoped>
/* 引入字体，确保样式统一 */
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&display=swap');

.dropdown-container {
  position: relative;
  display: inline-block;
  font-family: 'JetBrains Mono', monospace; /* 工业等宽字体 */
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --bg-hover: #e0e0e0;
}

/* --- 触发器样式 --- */
.dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  color: var(--black);
  font-size: 1rem;
  font-weight: 700;
  transition: all 0.2s cubic-bezier(0.25, 1, 0.5, 1);
  border: 2px solid transparent; /* 预留边框位置防止抖动 */
  background: transparent;
  user-select: none;
}

/* 悬停/激活状态：变为实体按钮 */
.dropdown-trigger:hover,
.dropdown-trigger--active {
  background: var(--white);
  border-color: var(--black);
  color: var(--black);
  box-shadow: 4px 4px 0 var(--red); /* 标志性硬阴影 */
  transform: translate(-2px, -2px);
}

/* 激活时文字变红 */
.dropdown-trigger--active {
  color: var(--red);
  border-color: var(--red);
}

.trigger-deco {
  opacity: 0.3;
  font-weight: 400;
  transition: opacity 0.2s;
}
.dropdown-trigger:hover .trigger-deco,
.dropdown-trigger--active .trigger-deco {
  opacity: 1;
  color: var(--black);
}

.dropdown-arrow-box {
  display: flex; align-items: center; justify-content: center;
  width: 16px; height: 16px;
}

.dropdown-arrow {
  font-size: 0.6rem;
  transition: transform 0.2s ease;
}

.dropdown-arrow--open {
  transform: rotate(180deg);
  color: var(--red);
}

/* --- 下拉菜单样式 --- */
.dropdown-menu {
  position: absolute;
  top: calc(100% + 10px); /* 留出一点空隙 */
  left: 0;
  min-width: 180px;
  background: var(--white);
  border: 2px solid var(--black);
  box-shadow: 6px 6px 0 rgba(0, 0, 0, 0.2); /* 更深的阴影 */
  padding: 0;
  z-index: 1000;
  display: flex;
  flex-direction: column;
}

/* 顶部红色装饰线 */
.menu-header-line {
  height: 4px;
  background: var(--black);
  width: 100%;
}

/* --- 菜单项样式 --- */
.dropdown-item {
  padding: 12px 16px;
  cursor: pointer;
  color: var(--black);
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.1s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid #ccc; /* 内部细分割线 */
  position: relative;
  overflow: hidden;
}
.dropdown-item:last-child {
  border-bottom: none;
}

.item-marker {
  opacity: 0;
  transform: translateX(-5px);
  transition: all 0.2s;
  color: var(--red);
  font-weight: bold;
}

/* 悬停效果：黑底白字 */
.dropdown-item:hover {
  background: var(--black);
  color: var(--white);
  padding-left: 20px; /* 轻微右移 */
}

.dropdown-item:hover .item-marker {
  opacity: 1;
  transform: translateX(0);
  color: var(--red); /* 箭头保持红色 */
}

/* 激活状态：红底白字 */
.dropdown-item--active {
  background: var(--red);
  color: var(--white);
  font-weight: 700;
}
.dropdown-item--active .item-marker {
  opacity: 1;
  transform: translateX(0);
  color: var(--white);
}

/* --- 动画：赛博裁切效果 --- */
.cyber-clip-enter-active,
.cyber-clip-leave-active {
  transition: all 0.2s cubic-bezier(0.19, 1, 0.22, 1);
}

.cyber-clip-enter-from,
.cyber-clip-leave-to {
  opacity: 0;
  transform: translateY(-10px) scaleY(0.9);
  clip-path: inset(0 0 100% 0); /* 像卷帘门一样展开 */
}
.cyber-clip-enter-to,
.cyber-clip-leave-from {
  opacity: 1;
  transform: translateY(0) scaleY(1);
  clip-path: inset(0 0 0 0);
}
</style>