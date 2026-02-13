import { ref, onMounted, onUnmounted } from 'vue'

// 1. 滚动检测逻辑
export function useNavigation() {
  const isScrolled = ref(false)

  const checkScroll = () => {
    // 建议：如果你觉得 20px 太灵敏，可以改成 10 或 50
    isScrolled.value = window.scrollY > 20
  }

  const handleScroll = () => {
    checkScroll()
  }

  onMounted(() => {
    window.addEventListener('scroll', handleScroll, { passive: true })
    // ✅ 新增：初始化时立即检查一次（防止刷新页面时已经在底部，但导航栏还是大的）
    checkScroll() 
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
  })

  return {
    isScrolled,
    checkScroll
  }
}

// 2. 路由激活状态检查 (核心修改)
export function useRouteActive() {
  /**
   * @param {string} currentPath 当前浏览器路径
   * @param {string} targetPath  菜单配置的 path
   */
  const isActive = (currentPath, targetPath) => {
    // ✅ 核心修复：如果 targetPath 为空 (比如 Mega 菜单触发器)，直接返回 false
    // 否则 undefined.startsWith() 会报错
    if (!targetPath) return false

    // 首页特判：只有完全匹配 '/' 才算激活
    if (targetPath === '/') {
      return currentPath === '/'
    }

    // 其他页面：完全匹配 OR 是其子路径 (例如 /gallery/123 也会激活 /gallery)
    return currentPath === targetPath || currentPath.startsWith(targetPath + '/')
  }

  return {
    isActive
  }
}