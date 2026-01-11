<template>
  <div class="profile-page">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>加载中...</p>
    </div>
    <!-- 错误状态 -->
    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <button v-if="!error.includes('登录')" @click="fetchUser" class="retry-btn">重试</button>
    </div>
    <!-- 用户信息 -->
    <div v-else-if="user" class="profile-content">
      <div class="user-header">
        
        <div class="user-info">
          <h2>
            {{ isMe ? "我的资料" : user.name + " 的资料" }}
            <span v-if="isMe" class="self-tag">(你自己)</span>
          </h2>
          <div class="user-badges">
            <span class="badge level">Lv.{{ user.level }}</span>
            <span v-if="user.title" class="badge title">{{ user.title }}</span>
            <span v-if="user.isVerified" class="badge verified">已验证</span>
            <span class="badge" :class="getStateClass(user.state)">{{ getStateText(user.state) }}</span>
          </div>
          <div class="user-meta">
            <span>用户ID: {{ user.id }}</span>
            <span>注册: {{ formatDate(user.registerDate) }}</span>
            <span>最后活跃: {{ formatDate(user.lastActiveTime) }}</span>
          </div>
        </div>
      </div>
      <!-- 基本信息 -->
      <div class="info-grid">
        <div class="info-item"><span>用户名</span><span>{{ user.name }}</span></div>
        <div v-if="isMe" class="info-item"><span>邮箱</span><span>{{ user.email || '未设置' }}</span></div>
        <div class="info-item"><span>等级</span><span>Lv.{{ user.level }}</span></div>
        <div class="info-item"><span>经验</span><span>{{ user.exp }}</span></div>
        <div class="info-item"><span>积分</span><span>{{ user.points || 0 }}</span></div>
        <div class="info-item"><span>获赞</span><span>{{ user.likes || 0 }}</span></div>
        <div class="info-item"><span>上次登录</span><span>{{ formatDate(user.lastLogin) }}</span></div>
      </div>
      <!-- 选项卡 -->
      <div class="function-panel">
        <div class="panel-tabs">
          <button v-for="tab in visibleTabs" :key="tab.id"
                  @click="switchTab(tab.id)"
                  :class="['tab-btn', { active: activeTab === tab.id }]">
            <span class="tab-icon">{{ tab.icon }}</span>
            <span>{{ tab.name }}</span>
          </button>
        </div>
        <div class="panel-content">
          <SettingsPanel      v-if="activeTab === 'settings' && isMe"      :user="user" :isMe="isMe"/>
          <UserBlog           v-if="activeTab === 'blogs'       && isMe"  :user="user" :isMe="isMe"/>
          <UserPosts          v-if="activeTab === 'posts'       && isMe"  :user="user" :isMe="isMe"/>
          <RepositoryPanel    v-if="activeTab === 'repository'"    :user="user" :isMe="isMe"/>
          <CheckinPanel       v-if="activeTab === 'checkin'"       :user="user" :isMe="isMe"/>
          <FriendsPanel       v-if="activeTab === 'friends' && isMe"       :user="user" :isMe="isMe"/>
          <AchievementsPanel  v-if="activeTab === 'achievements'"  :user="user" :isMe="isMe"/>
          <MessagesPanel      v-if="activeTab === 'messages' && isMe"      :user="user" :isMe="isMe"/>
          <NotificationPanel  v-if="activeTab === 'notification' && isMe"  :user="user" :isMe="isMe"/>

          
          <!-- 查看他人页面时的提示 -->
          <div v-if="!isMe && isPrivateTab(activeTab)" class="private-tab-notice">
            <div class="notice-content">
              <span class="notice-icon">🔒</span>
              <h3>隐私保护</h3>
              <p>此页面内容属于用户隐私，仅限本人查看</p>
              <button @click="switchToPublicTab" class="switch-tab-btn">
                查看公开信息
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-else class="no-data">无用户数据</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'
import SettingsPanel      from './SettingsPanel.vue'
import RepositoryPanel    from './RepositoryPanel.vue'
import CheckinPanel       from './CheckinPanel.vue'
import FriendsPanel       from './FriendsPanel.vue'
import AchievementsPanel  from './AchievementsPanel.vue'
import MessagesPanel      from './MessagesPanel.vue'
import NotificationPanel  from './notificationPanel.vue'
import UserBlog from './UserBlog.vue'
import UserPosts from './UserPosts.vue'

const route = useRoute(); 
const router = useRouter(); 
const auth = useAuthStore();
const defaultAvatar = '/default-avatar.png'

const userId = computed(() => {
  let id = route.params.userId
  return (!id || id === 'undefined' || id === undefined) ? 'me' : id
})

const isMe = computed(() => userId.value === 'me' || (auth.user && String(auth.user.id) === String(userId.value)))
const loading = ref(true)
const error = ref('')
const user = ref(null)
const activeTab = ref('settings')

// 完整的选项卡定义
const allTabs = [
  { id: 'settings', name: '设置', icon: '⚙️', isPrivate: true },
  { id: 'blogs', name: '博客', icon: '⚙️', isPrivate: false },
  { id: 'posts', name: '帖子', icon: '⚙️', isPrivate: true },
  { id: 'repository', name: '仓库', icon: '📦', isPrivate: false },
  { id: 'checkin', name: '签到', icon: '📅', isPrivate: false },
  { id: 'friends', name: '好友', icon: '👥', isPrivate: true },
  { id: 'achievements', name: '成就', icon: '🏆', isPrivate: false },
  { id: 'messages', name: '信息', icon: '💬', isPrivate: true },
  { id: 'notification', name: '通知', icon: '🔔', isPrivate: true }
]

// 根据是否查看自己来显示不同的选项卡
const visibleTabs = computed(() => {
  if (isMe.value) {
    // 查看自己：显示所有选项卡
    return allTabs
  } else {
    // 查看他人：只显示公开选项卡
    return allTabs.filter(tab => !tab.isPrivate)
  }
})

// 检查当前选项卡是否是隐私选项卡
const isPrivateTab = (tabId) => {
  const tab = allTabs.find(t => t.id === tabId)
  return tab ? tab.isPrivate : false
}

// 切换到公开选项卡
const switchToPublicTab = () => {
  // 找到第一个公开的选项卡
  const publicTab = visibleTabs.value[0]
  if (publicTab) {
    switchTab(publicTab.id)
  }
}

// 切换标签页并更新URL
const switchTab = (tabId) => {
  // 检查权限：如果是查看他人且尝试访问隐私选项卡，则阻止
  if (!isMe.value && isPrivateTab(tabId)) {
    return
  }
  
  activeTab.value = tabId
  // 更新URL参数但不触发页面刷新
  router.push({ 
    path: route.path, 
    query: { ...route.query, tab: tabId }
  })
}

// 根据URL参数设置活动标签页
const setActiveTabFromQuery = () => {
  const tabFromQuery = route.query.tab
  
  if (tabFromQuery) {
    // 检查权限：如果是查看他人且URL参数是隐私选项卡，则重定向到公开选项卡
    if (!isMe.value && isPrivateTab(tabFromQuery)) {
      switchToPublicTab()
      return
    }
    
    if (allTabs.some(tab => tab.id === tabFromQuery)) {
      activeTab.value = tabFromQuery
      return
    }
  }
  
  // 默认标签页：如果是查看自己则显示设置，查看他人则显示仓库
  activeTab.value = isMe.value ? 'settings' : 'repository'
}

function formatDate(dt) {
  if (!dt) return '暂无'
  try { return new Date(dt).toLocaleDateString('zh-CN') } catch { return dt }
}
function getStateText(s) {
  switch (s) {
    case 1: return '在线'
    case 0: return '离线'
    case 2: return '封禁'
    default: return '未知'
  }
}
function getStateClass(s) {
  return s === 1 ? 'online' : s === 0 ? 'offline' : s === 2 ? 'banned' : ''
}

async function fetchUser() {
  loading.value = true
  error.value = ''
  user.value = null
  let url = userId.value === 'me' 
    ? '/default/user/me' 
    : isNaN(Number(userId.value)) 
      ? (() => { error.value = '无效用户ID'; loading.value = false; return null })() 
      : `/default/user/${userId.value}`;
  if (!url) return
  try {
    const resp = await apiClient.get(url)
    if (resp.data && resp.data.success) user.value = resp.data.data
    else error.value = resp.data.error || '用户不存在'
  } catch (err) {
    error.value = err?.response?.data?.error || err.message || "请求失败"
  }
  loading.value = false
}

onMounted(() => {
  setActiveTabFromQuery()
  fetchUser()
})

// 监听路由参数变化
watch(() => route.query.tab, (newTab) => {
  if (newTab) {
    // 检查权限
    if (!isMe.value && isPrivateTab(newTab)) {
      switchToPublicTab()
      return
    }
    
    if (allTabs.some(tab => tab.id === newTab)) {
      activeTab.value = newTab
    }
  }
})

// 监听用户ID变化
watch(userId, () => { 
  setActiveTabFromQuery()
  fetchUser() 
})

// 监听isMe变化，自动调整选项卡
watch(isMe, (newIsMe) => {
  setActiveTabFromQuery()
})
</script>

<style scoped>
.profile-page {
  max-width: 100%;
  margin: 20px auto;
  padding: 20px;
}

.loading-state {
  text-align: center;
  padding: 60px 20px;
  color: #666;
}
.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}
@keyframes spin { 0% { transform: rotate(0deg);} 100% { transform: rotate(360deg);} }
.error-state {
  text-align: center;
  padding: 40px 20px;
  background: #ffeaea;
  border-radius: 8px;
  color: #d63031;
  margin: 20px 0;
}
.retry-btn {
  padding: 8px 16px;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  margin-top: 10px;
}
.retry-btn:hover { background: #2980b9; }
.profile-content {
  background: rgb(255, 255, 255);
  border-radius: 12px;
  padding: 30px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  width: 100%;
}
.user-header { display: flex; align-items: center; gap: 20px; margin-bottom: 30px; }
.avatar { width: 100px; height: 100px; border-radius: 50%; border: 4px solid #e9ecef; object-fit: cover; }
.user-info h2 { margin: 0 0 10px 0; color: #2c3e50; font-size: 24px; }
.self-tag { font-size: 14px; color: #666; font-weight: normal; }
.user-badges { display: flex; gap: 8px; flex-wrap: wrap;}
.badge { padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;}
.badge.level { background: linear-gradient(135deg, #ffd700 0%, #ffa500 100%); color: white;}
.badge.title { background: #3498db; color: white;}
.badge.verified { background: #27ae60; color: white;}
.basic-info-card { background: #f8f9fa; border-radius: 8px; padding: 20px; margin-bottom: 30px; border: 1px solid #e9ecef;}
.info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px;}
.info-item { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e9ecef;}
.info-item:last-child { border-bottom: none; }
.info-item .label { color: #666; font-weight: 500; }
.info-item .value { color: #2c3e50; font-weight: 600; }
.online { color: #27ae60; }
.offline { color: #95a5a6; }
.banned { color: #e74c3c; }
.function-panel { border: 1px solid #e9ecef; border-radius: 8px; overflow: hidden;}
.panel-tabs { display: flex; background: #f8f9fa; border-bottom: 1px solid #e9ecef;}
.tab-btn { flex: 1; padding: 15px; border: none; background: none; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; transition: all 0.3s ease; border-bottom: 3px solid transparent;}
.tab-btn:hover { background: #e9ecef; }
.tab-btn.active { background: white; border-bottom-color: #3498db; color: #3498db;}
.tab-icon { font-size: 18px; }
.panel-content { padding: 20px; min-height: 300px;}

/* 隐私选项卡提示样式 */
.private-tab-notice {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  text-align: center;
}

.notice-content {
  max-width: 300px;
}

.notice-icon {
  font-size: 48px;
  display: block;
  margin-bottom: 15px;
}

.notice-content h3 {
  margin: 0 0 10px 0;
  color: #2c3e50;
}

.notice-content p {
  margin: 0 0 20px 0;
  color: #666;
}

.switch-tab-btn {
  padding: 10px 20px;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.switch-tab-btn:hover {
  background: #2980b9;
}

.no-data { text-align: center; padding: 60px 20px; color: #666; background: white; border-radius: 8px; border: 1px solid #e9ecef;}
@media (max-width: 768px) {
  .profile-page { padding: 10px;}
  .profile-content { padding: 20px;}
  .user-header { flex-direction: column; text-align: center;}
  .info-grid { grid-template-columns: 1fr;}
  .panel-tabs { flex-direction: column;}
  .tab-btn { justify-content: flex-start; padding: 12px 20px;}
}
</style>