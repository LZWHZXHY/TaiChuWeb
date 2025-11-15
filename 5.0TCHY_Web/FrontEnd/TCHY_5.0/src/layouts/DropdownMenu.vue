<template>
  <div class="dropdown-container" @mouseleave="startCloseTimer">
    <!-- 下拉菜单触发器 -->
    <div 
      class="dropdown-trigger"
      :class="{ 'dropdown-trigger--active': isOpen }"
      @mouseenter="startOpenTimer"
      @click="toggleDropdown"
    >
      <span class="trigger-text">{{ triggerText }}</span>
      <span class="dropdown-arrow" :class="{ 'dropdown-arrow--open': isOpen }">▼</span>
    </div>

    <!-- 下拉菜单内容 -->
    <transition name="dropdown">
      <div 
        v-if="isOpen"
        class="dropdown-menu"
        @mouseenter="cancelCloseTimer"
        @mouseleave="startCloseTimer"
      >
        <div 
          v-for="item in items"
          :key="item.path"
          class="dropdown-item"
          :class="{ 'dropdown-item--active': isActive(item.path) }"
          @click="handleItemClick(item)"
        >
          <span class="dropdown-item-text">{{ item.name }}</span>
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
  }, 200)
}

const startCloseTimer = () => {
  cancelOpenTimer()
  closeTimer = setTimeout(() => {
    isOpen.value = false
  }, 300)
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
.dropdown-container {
  position: relative;
  display: inline-block;
}

.dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  cursor: pointer;
  color: #666;
  font-size: 16px;
  font-weight: 500;
  transition: all 0.3s ease;
  border-radius: 6px;
}

.dropdown-trigger:hover {
  color: #2c3e50;
  background: rgba(0, 0, 0, 0.03);
}

.dropdown-trigger--active {
  color: #3498db;
  font-weight: 600;
}

.dropdown-arrow {
  font-size: 12px;
  transition: transform 0.3s ease;
}

.dropdown-arrow--open {
  transform: rotate(180deg);
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  min-width: 160px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  padding: 8px;
  z-index: 1000;
  margin-top: 8px;
}

.dropdown-item {
  padding: 12px 16px;
  cursor: pointer;
  color: #666;
  font-size: 14px;
  border-radius: 6px;
  transition: all 0.3s ease;
}

.dropdown-item:hover {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.dropdown-item--active {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
  font-weight: 500;
}

.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 0.2s ease;
}

.dropdown-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>