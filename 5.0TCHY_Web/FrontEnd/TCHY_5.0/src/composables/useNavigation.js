import { ref, onMounted, onUnmounted } from 'vue'

// 可复用的导航逻辑
export function useNavigation() {
  const isScrolled = ref(false)

  const checkScroll = () => {
    isScrolled.value = window.scrollY > 20
  }

  const handleScroll = () => {
    checkScroll()
  }

  onMounted(() => {
    window.addEventListener('scroll', handleScroll, { passive: true })
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
  })

  return {
    isScrolled,
    checkScroll
  }
}

// 路由激活状态检查
export function useRouteActive() {
  const isActive = (currentPath, targetPath) => {
    if (targetPath === '/') {
      return currentPath === '/'
    }
    return currentPath === targetPath || currentPath.startsWith(targetPath + '/')
  }

  return {
    isActive
  }
}