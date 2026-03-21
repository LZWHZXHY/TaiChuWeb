<template>
  <div class="uc-left" v-if="!loading || hasData">
    <div class="top-row">
      <div class="identity-info">
       <div class="name-wrapper">
  <h1 class="user-name">{{ displayData.username || '未命名' }}</h1>
  
  <div class="uid-tag">
    <span class="uid-label">UID</span>
    <span class="uid-number">{{ userId || authStore.userID || '000000' }}</span>
  </div>

  <span v-if="displayData.gender" class="gender-badge">
    {{ displayData.gender }}
  </span>
</div>
        <div class="signature-text">
          {{ displayData.signature || 'NO_SIGNATURE // 暂无签名' }}
        </div>
      </div>
    </div>

    <div class="bio-container">
      <div class="tags-list" v-if="processedInterests.length > 0">
        <span v-for="(tag, index) in processedInterests" :key="index" class="tag-item">
          #{{ tag }}
        </span>
      </div>
      <p class="bio-text">{{ displayData.bio || '用户很懒，什么都没写...' }}</p>
    </div>

    <div class="footer-row">
      <span class="ft-item" v-if="displayData.region">
        📍 {{ displayData.region }}
      </span>
      <span class="ft-divider" v-if="displayData.region && formattedBirthday">|</span>
      
      <span class="ft-item" v-if="formattedBirthday">
        🎂 {{ formattedBirthday }}
      </span>
      
      <span class="ft-divider" v-if="(displayData.region || formattedBirthday) && displayData.email">|</span>
      
      <span class="ft-item" v-if="displayData.email">
        ✉ {{ displayData.email }}
      </span>
    </div>
    
    <div class="grid-bg"></div>
  </div>
  
  <div v-else class="uc-left loading-state">
    LOADING USER DATA...
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth' // 👈 引入 Store

const props = defineProps({
  userId: {
    type: [String, Number],
    default: null
  }
})

const authStore = useAuthStore()
const loading = ref(true)
const remoteData = ref({}) // 用于存储“别人”的数据

// 1. 判断是否是当前用户
const isMe = computed(() => {
  // 如果没传 ID，或者 ID 是 'MEE'，或者是当前登录用户的 ID
  if (!props.userId || props.userId === 'MEE') return true
  return String(props.userId) === String(authStore.userID)
})

// 2. 核心：双源数据选择器
// 如果是自己 -> 取 Store (带缓存)
// 如果是别人 -> 取 remoteData (API获取)
const displayData = computed(() => {
  if (isMe.value) {
    return authStore.user || {}
  }
  return remoteData.value
})

// 3. 辅助判断：是否有足够的数据显示 (防止空对象导致界面塌陷)
const hasData = computed(() => {
  return !!displayData.value.username
})

const processedInterests = computed(() => {
  const raw = displayData.value.interests || ''
  return raw.replace(/，/g, ',').split(',').map(i => i.trim()).filter(i => i)
})

const formattedBirthday = computed(() => {
  const d = displayData.value.birthDate
  if (!d) return ''
  try { return new Date(d).toLocaleDateString() } catch { return d }
})

// 4. 数据获取逻辑
const fetchUserData = async () => {
  // ✅ 优化点：如果是看自己，且 Store 里已经有缓存数据（比如 username 存在）
  // 直接结束 Loading，不再请求 API (因为 AuthStore 会在后台静默更新)
  if (isMe.value && authStore.user?.username) {
    loading.value = false
    return
  }

  loading.value = true
  try {
    // 只有看别人，或者 Store 里完全没数据时，才发起请求
    let url = ''
    if (isMe.value) {
      url = '/profile/detail'
    } else {
      url = `/profile/get-id/${props.userId}`
    }

    const res = await apiClient.get(url)
    
    if (res.data && res.data.success) {
      const data = res.data.data
      
      const mappedData = {
        username: data.Username || data.username,
        gender: data.Gender,
        bio: data.Bio,
        interests: data.Interests,
        region: data.Region,
        birthDate: data.BirthDate,
        signature: data.Signature,
        email: data.Email || ''
      }

      if (isMe.value) {
        // 如果这里触发了请求（比如强制刷新），顺便更新 Store
        // 但通常 Store 的 fetchLatestUser 已经处理了
        authStore.user = { ...authStore.user, ...mappedData }
      } else {
        remoteData.value = mappedData
      }
    }
  } catch (error) {
    console.error("Fetch Profile Error:", error)
    if (!isMe.value) remoteData.value = { username: "ERR", bio: "连接断开..." }
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchUserData()
})

watch(() => props.userId, () => {
  remoteData.value = {} // 切换用户时清空旧数据
  fetchUserData()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* === 左侧 uc-left (Flex 2) === */
.uc-left {
  flex: 2;
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  position: relative;
  z-index: 1;
  font-family: 'Noto Sans SC', sans-serif;
  overflow: hidden;
  background-color: #F4F1EA; 
  min-height: 200px; 
  transition: opacity 0.2s; /* 增加一点淡入淡出 */
}

.loading-state {
  align-items: center;
  justify-content: center;
  font-family: 'Share Tech Mono', monospace;
  color: #888;
  border: 1px dashed #ccc;
  background: repeating-linear-gradient(45deg, #f9f9f9, #f9f9f9 10px, #eee 10px, #eee 20px);
}

/* 顶部布局 */
.top-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.identity-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 100%;
}

.name-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.user-name {
  margin: 0;
  font-size: 28px;
  font-weight: 900;
  line-height: 1.1;
  color: #1a1a1a;
  letter-spacing: -0.5px;
}

/* 性别徽章 */
.gender-badge {
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
  background-color: #111;
  color: #F4F1EA; 
  font-weight: bold;
  font-family: 'Share Tech Mono', monospace;
  text-transform: uppercase;
  display: inline-block;
  transform: translateY(2px);
}

/* 个性签名样式 */
.signature-text {
  font-size: 12px;
  color: #d35400; 
  font-weight: bold;
  letter-spacing: 0.5px;
  font-family: 'Share Tech Mono', 'Noto Sans SC', monospace;
  opacity: 0.9;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

/* --- 中间简介区 --- */
.bio-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 12px;
  overflow: hidden;
  padding-top: 8px;
  border-top: 1px dashed rgba(0,0,0,0.1);
}

.tags-list {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.tag-item {
  font-size: 11px;
  background: #fff;
  color: #444;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 500;
  border: 1px solid rgba(0,0,0,0.1);
  box-shadow: 1px 1px 0 rgba(0,0,0,0.05);
}

.bio-text {
  margin: 0;
  font-size: 13px;
  line-height: 1.6;
  color: #444;
  display: -webkit-box;
  -webkit-line-clamp: 4; 
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: pre-wrap; 
}

/* --- 底部信息 --- */
.footer-row {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 11px;
  color: #888;
  font-family: 'Share Tech Mono', 'Noto Sans SC', sans-serif;
  border-top: 2px solid #eee;
  padding-top: 10px;
  margin-top: auto; 
}

.ft-item {
    display: flex;
    align-items: center;
    gap: 4px;
}

.ft-divider { opacity: 0.3; font-size: 10px; color: #000; }

/* 背景装饰 */
.grid-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: 
    linear-gradient(rgba(0,0,0,0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.03) 1px, transparent 1px);
  background-size: 20px 20px;
  z-index: -1;
  pointer-events: none;
}

/* UID 标签整体样式 */
.uid-tag {
  display: flex;
  align-items: center;
  background-color: rgba(0, 0, 0, 0.05); /* 浅灰色背景 */
  border: 1px solid rgba(0, 0, 0, 0.1);  /* 细边框 */
  border-radius: 4px;
  height: 18px;
  overflow: hidden;
  transform: translateY(2px); /* 稍微下移，与名字对齐 */
}

/* 左侧 "UID" 字样 */
.uid-label {
  background-color: #2c3e50; /* 深色背景 */
  color: #fff;
  font-family: 'Share Tech Mono', monospace;
  font-size: 9px;
  padding: 0 4px;
  height: 100%;
  display: flex;
  align-items: center;
  font-weight: bold;
  letter-spacing: 0.5px;
}

/* 右侧数字部分 */
.uid-number {
  color: #666;
  font-family: 'Share Tech Mono', monospace;
  font-size: 11px;
  padding: 0 6px;
  font-weight: 500;
}

/* 鼠标悬停效果 (可选) */
.uid-tag:hover {
  border-color: #e67e22; /* 悬停变色，呼应 DataCard 的主色 */
  transition: border-color 0.2s;
}
.uid-tag:hover .uid-label {
  background-color: #e67e22;
  transition: background-color 0.2s;
}

</style>