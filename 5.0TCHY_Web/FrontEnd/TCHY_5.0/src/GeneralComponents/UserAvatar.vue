<template>
  <div 
    class="avatar-wrapper" 
    :class="{ 'is-clickable': allowLink }"
    @click.stop="handleToProfile"
  >
    <div 
      class="title-badge" 
      v-if="currentTitle && showTitle" 
      :class="`bg-rarity-${currentRarity}`"
    >
      {{ currentTitle }}
    </div>

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
  passedAvatar: { type: String, default: null },
  passedLevel: { type: [Number, String], default: null },
  passedTitle: { type: String, default: null },
  passedTitleRarity: { type: [Number, String], default: null },
  allowLink: { type: Boolean, default: true },
  showLevel: { type: Boolean, default: true },
  showTitle: {type:Boolean, default: false}
})

const router = useRouter()
const authStore = useAuthStore()
const remoteData = ref({}) 
const loading = ref(true) 
const BASE_URL = 'https://bianyuzhou.com' 

// UserAvatar.vue

const isMe = computed(() => {
  // 只有明确传入 'MEE' 或者 ID 确实等于当前登录 ID 时才返回 true
  if (props.userId === 'MEE') return true
  if (!props.userId || !authStore.userID) return false // ID 为空时，绝对不是“我”
  return String(props.userId) === String(authStore.userID)
})

// UserAvatar.vue 里的 userData 建议改为
const userData = computed(() => {
  if (isMe.value) return authStore.user || {}
  
  // 这里的映射要非常小心
  if (props.passedAvatar) {
    return {
      avatar: props.passedAvatar, // 统一存入小写，方便 finalAvatarUrl 读取
      level: props.passedLevel,
      title: props.passedTitle,
      titleRarity: props.passedTitleRarity
    }
  }

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
  return props.passedLevel ?? (userData.value.level || userData.value.Level || 0)
})

const currentTitle = computed(() => {
  return props.passedTitle ?? (userData.value.title || userData.value.Title || '')
})

const currentRarity = computed(() => {
  return props.passedTitleRarity ?? (userData.value.titleRarity || userData.value.TitleRarity || 1)
})

const handleImgError = (e) => { e.target.src = '/土豆.jpg' }

const handleToProfile = () => {
  if (!props.allowLink || !props.userId) return
  router.push(`/profile/${props.userId}`)
}

const fetchAvatarData = async () => {
  // 即使有缓存也建议后台静默刷新，确保头衔变更能实时看到
  if (props.passedAvatar || (isMe.value && authStore.user?.avatar)) {
    loading.value = false
  }

  try {
    const url = isMe.value ? '/profile/detail' : `/profile/get-id/${props.userId}`
    const res = await apiClient.get(url)
    if (res.data && res.data.success) {
      const d = res.data.data
      const dataMap = {
        avatar: d.Avatar || d.avatar,
        level: d.Level || d.level,
        title: d.Title || d.title,
        titleRarity: d.TitleRarity || d.titleRarity || 1
      }
      if (isMe.value) {
        authStore.user = { ...authStore.user, ...dataMap }
      } else {
        remoteData.value = dataMap
      }
    }
  } catch (e) {
    console.warn("Avatar load failed:", props.userId)
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
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@500;700;900&display=swap');

.avatar-wrapper { 
  position: relative; 
  width: 100%; 
  height: 100%; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  flex-shrink: 0; 
  transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); 


}

.avatar-wrapper.is-clickable { cursor: pointer; }
.avatar-wrapper.is-clickable:hover { transform: scale(1.05); z-index: 10; }

.avatar-frame { box-sizing: border-box; width: 100%; height: 100%; border-radius: 4px; border: 3px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.15); overflow: hidden; background: #f0f0f0; transform: translateZ(0); }
.big-avatar { width: 100%; height: 100%; object-fit: cover; display: block; }

.level-pill { position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); background: #2c3e50; color: #fff; padding: 1px 8px; border-radius: 4px; border: 2px solid #fff; display: flex; align-items: baseline; gap: 2px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); z-index: 2; white-space: nowrap; }
.lv-txt { font-size: 10px; opacity: 0.8; font-family: 'Share Tech Mono', monospace; }
.lv-num { font-size: 12px; font-weight: bold; font-family: 'Share Tech Mono', monospace; color: #e67e22; }

/* --- 🌟 头衔悬浮铭牌样式 🌟 --- */
.title-badge {
  position: absolute;
  bottom: 100%;
  margin-bottom: 4px;
  left: 50%;
  transform: translateX(-50%);
  padding: 2px 10px;
  border-radius: 4px;
  border: 1px solid rgba(255, 255, 255, 0.8);
  color: #fff;
  font-size: 11px;
  font-weight: 900;
  font-family: 'Noto Sans SC', sans-serif;
  white-space: nowrap;
  z-index: 3;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
  letter-spacing: 0.5px;
}

/* 稀有度颜色方案 */
.bg-rarity-1 { background-color: #7f8c8d; }
.bg-rarity-2 { background-color: #2ecc71; border-color: #a8e6cf; }
.bg-rarity-3 { background-color: #3498db; border-color: #bbdefb; box-shadow: 0 0 8px rgba(52,152,219,0.5); }
.bg-rarity-4 { background-color: #9b59b6; border-color: #e1bee7; box-shadow: 0 0 10px rgba(155,89,182,0.6); }
.bg-rarity-5 { background-color: #f39c12; border-color: #ffe0b2; box-shadow: 0 0 12px rgba(243,156,18,0.8); }

.bg-rarity-6 {
  background: linear-gradient(90deg, #d4af37, #ff8c00, #d4af37);
  background-size: 200% auto;
  border-color: #fff;
  animation: badge-shine 2s linear infinite;
  box-shadow: 0 0 12px rgba(255, 215, 0, 0.6);
}

.bg-rarity-7 {
  background: linear-gradient(90deg, #8b0000, #ff1a1a, #8b0000);
  background-size: 200% auto;
  border-color: #ffcccc;
  animation: badge-shine 2s linear infinite;
  box-shadow: 0 0 15px rgba(255, 0, 0, 0.8);
}

@keyframes badge-shine {
  to { background-position: 200% center; }
}
</style>