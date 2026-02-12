<template>
  <div 
    class="avatar-wrapper" 
    :class="{ 'is-clickable': allowLink }"
    @click.stop="handleToProfile"
  >
    <div class="avatar-frame">
      <img 
        :src="finalAvatarUrl" 
        class="big-avatar" 
        alt="User Avatar"
        @error="handleImgError"
      />
    </div>
    
    <div class="level-pill" v-if="showLevel">
      <span class="lv-txt">LV.</span>
      <span class="lv-num">{{ currentLevel }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'

const props = defineProps({
  userId: { type: [String, Number], default: null },
  // ✨ 新增：允许父组件直接传入已经拿到的头像 URL
  passedAvatar: { type: String, default: null },
  // ✨ 新增：允许父组件直接传入等级
  passedLevel: { type: [Number, String], default: null },
  allowLink: { type: Boolean, default: true },
  showLevel: { type: Boolean, default: true }
})

const router = useRouter()
const authStore = useAuthStore()
const remoteData = ref({}) 
const loading = ref(true) // 用于控制加载状态
const BASE_URL = 'https://bianyuzhou.com' 

const isMe = computed(() => {
  if (!props.userId || props.userId === 'MEE') return true
  return String(props.userId) === String(authStore.userID)
})

// ✨ 优化：数据源选择逻辑
const userData = computed(() => {
  // 1. 如果是自己，优先用 Store
  if (isMe.value) return authStore.user || {}
  
  // 2. 如果父组件传了 passedAvatar，直接封装成对象使用，不再读 remoteData
  if (props.passedAvatar) {
    return {
      avatar: props.passedAvatar,
      level: props.passedLevel
    }
  }

  // 3. 否则使用异步获取的远程数据
  return remoteData.value
})

const finalAvatarUrl = computed(() => {
  let url = userData.value.avatar || userData.value.Avatar
  if (!url) return 'https://via.placeholder.com/150' 
  if (url.startsWith('http')) return url
  url = url.replace(/\\/g, '/')
  if (url.startsWith('/')) url = url.substring(1)
  if (!url.startsWith('uploads/')) url = `uploads/${url}`
  return `${BASE_URL}/${url}`
})

const currentLevel = computed(() => {
  // 优先显示传入的等级，否则显示数据源里的等级
  return props.passedLevel ?? (userData.value.level || userData.value.Level || 0)
})

const handleImgError = (e) => { e.target.src = '/土豆.jpg' }

const handleToProfile = () => {
  if (!props.allowLink || !props.userId) return
  router.push(`/profile/${props.userId}`)
}

const fetchAvatarData = async () => {
  // ✨ 核心优化：如果有预传数据，或者是带缓存的自己，直接退出，不发 API
  if (props.passedAvatar || (isMe.value && authStore.user?.avatar)) {
    loading.value = false
    return 
  }

  try {
    const url = isMe.value ? '/profile/detail' : `/profile/get-id/${props.userId}`
    const res = await apiClient.get(url)
    if (res.data && res.data.success) {
      const d = res.data.data
      const dataMap = {
        avatar: d.Avatar || d.avatar,
        level: d.Level || d.level
      }
      if (isMe.value) {
        authStore.user = { ...authStore.user, ...dataMap }
      } else {
        remoteData.value = dataMap
      }
    }
  } catch (e) {
    console.warn("Avatar load failed for:", props.userId)
  } finally {
    loading.value = false
  }
}

onMounted(fetchAvatarData)

watch(() => props.userId, () => {
  if (!props.passedAvatar) {
    remoteData.value = {}
    fetchAvatarData()
  }
})
</script>

<style scoped>
/* 样式部分保持不变... */
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
.avatar-wrapper { position: relative; width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; flex-shrink: 0; transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); }
.avatar-wrapper.is-clickable { cursor: pointer; }
.avatar-wrapper.is-clickable:hover { transform: scale(1.05); z-index: 10; }
.avatar-frame { width: 100%; height: 100%; border-radius: 4px; border: 3px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.15); overflow: hidden; background: #f0f0f0; transform: translateZ(0); }
.big-avatar { width: 100%; height: 100%; object-fit: cover; display: block; }
.level-pill { position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); background: #2c3e50; color: #fff; padding: 1px 8px; border-radius: 4px; border: 2px solid #fff; display: flex; align-items: baseline; gap: 2px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); z-index: 2; white-space: nowrap; }
.lv-txt { font-size: 10px; opacity: 0.8; font-family: 'Share Tech Mono', monospace; }
.lv-num { font-size: 12px; font-weight: bold; font-family: 'Share Tech Mono', monospace; color: #e67e22; }
</style>