<template>
  <div class="friend-request-container">
    <div class="main-layout">
      <!-- 左侧面板 - 好友列表和未处理请求 -->
      <div class="left-panel">
        <!-- 未处理的好友请求 -->
        <div class="pending-requests-section">
          <div class="section-header">
            <h3>待处理的好友请求 ({{ pendingRequests.length }})</h3>
            <button class="refresh-btn" @click="loadPendingRequests" :disabled="loadingPending">
              <span v-if="loadingPending" class="spinner-tiny"></span>
              {{ loadingPending ? '刷新中...' : '刷新' }}
            </button>
          </div>
          
          <div class="requests-container">
            <div v-if="pendingRequests.length > 0" class="requests-grid">
              <div
                v-for="request in pendingRequests"
                :key="request.RequestId"
                class="request-card"
              >
                <div class="request-header">
                  <div class="user-avatar">
                    {{ getInitials(request.FromUserName) }}
                  </div>
                  <div class="request-info">
                    <h4 class="username">{{ request.FromUserName }}</h4>
                    <p class="user-id">ID: {{ request.FromUserId }}</p>
                    <div class="user-meta">
                      <span class="level">Lv.{{ request.FromUserLevel }}</span>
                      <span class="time">申请时间: {{ formatDate(request.CreateTime) }}</span>
                    </div>
                    <p v-if="request.Remark" class="remark">备注: {{ request.Remark }}</p>
                  </div>
                </div>
                
                <div class="request-actions">
                  <button 
                    class="btn btn-accept" 
                    @click="handleRequest(request.RequestId, 'accept')"
                    :disabled="handlingRequest"
                  >
                    <span v-if="handlingRequest === request.RequestId" class="spinner-tiny"></span>
                    {{ handlingRequest === request.RequestId ? '处理中...' : '✅ 同意' }}
                  </button>
                  <button 
                    class="btn btn-reject" 
                    @click="handleRequest(request.RequestId, 'reject')"
                    :disabled="handlingRequest"
                  >
                    <span v-if="handlingRequest === request.RequestId" class="spinner-tiny"></span>
                    {{ handlingRequest === request.RequestId ? '处理中...' : '❌ 拒绝' }}
                  </button>
                </div>
              </div>
            </div>
            
            <div v-else class="empty-requests">
              <div class="empty-icon">📥</div>
              <h3>暂无待处理请求</h3>
              <p>当有人向您发送好友请求时，会显示在这里</p>
            </div>
          </div>
        </div>

        <!-- 好友列表区域 -->
        <div class="friends-section">
          <div class="section-header">
            <div class="section-title-row">
              <h3>我的好友 ({{ friendsList.length }})</h3>
              <div class="friends-stats">
                <span class="stat-item">在线: {{ onlineFriendsCount }}</span>
                <span class="stat-item">离线: {{ offlineFriendsCount }}</span>
                <button class="refresh-btn" @click="loadFriendsList" :disabled="loadingFriends">
                  <span v-if="loadingFriends" class="spinner-tiny"></span>
                  {{ loadingFriends ? '刷新中...' : '刷新' }}
                </button>
              </div>
            </div>
          </div>
          
          <div class="friends-container">
            <div v-if="friendsList.length > 0" class="friends-grid">
              <div
                v-for="friend in filteredFriends"
                :key="friend.FriendId"
                class="friend-card"
                :class="{ 'online': friend.IsOnline }"
              >
                <div class="friend-card-header">
                  <div class="friend-avatar">
                    {{ getInitials(friend.FriendName) }}
                    <span class="online-indicator" :class="{ 'online': friend.IsOnline }"></span>
                  </div>
                  <div class="friend-info">
                    <h4 class="friend-name">{{ friend.FriendName }}</h4>
                    <p class="friend-id">ID: {{ friend.FriendId }}</p>
                    <div class="friend-meta">
                      <span class="level">Lv.{{ friend.FriendLevel }}</span>
                      <span class="status">{{ friend.IsOnline ? '在线' : '离线' }}</span>
                      <span class="add-time">添加于: {{ formatDate(friend.CreateTime) }}</span>
                    </div>
                  </div>
                </div>
                
                <div class="friend-card-actions">
                  <button class="btn btn-chat" @click="startChat(friend)">
                    💬 私信
                  </button>
                  <button class="btn btn-profile" @click="viewProfile(friend.FriendId)">
                    👤 资料
                  </button>
                  <button class="btn btn-delete" @click="confirmDeleteFriend(friend)">
                    🗑️ 删除
                  </button>
                </div>
              </div>
            </div>
            
            <div v-else class="empty-friends">
              <div class="empty-icon">👥</div>
              <h3>暂无好友</h3>
              <p>搜索用户并添加好友开始聊天吧！</p>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧面板 - 搜索和添加好友 -->
      <div class="right-panel">
        <div class="search-section">
          <div class="search-header">
            <h2 class="section-title">添加好友</h2>
            <p class="section-subtitle">搜索用户ID或用户名添加好友</p>
          </div>
          
          <div class="search-box">
            <div class="search-input-group">
              <input
                v-model="searchKeyword"
                type="text"
                placeholder="输入用户ID或用户名进行搜索..."
                class="search-input"
                @keyup.enter="searchUsers"
                @input="handleSearchInput"
              />
              <button 
                class="search-btn" 
                @click="searchUsers" 
                :disabled="searching || !searchKeyword.trim()"
              >
                <span v-if="searching" class="spinner-small"></span>
                <span v-else>🔍</span>
                {{ searching ? '搜索中...' : '搜索' }}
              </button>
            </div>
          </div>
        </div>

        <div class="results-section">
          <div class="results-header">
            <h3>搜索结果</h3>
            <div class="results-controls">
              <span class="results-count" v-if="searchResults.length > 0">
                找到 {{ searchResults.length }} 个用户
              </span>
              <button v-if="searchResults.length > 0" class="clear-results" @click="clearResults">
                清空结果
              </button>
            </div>
          </div>
          
          <div class="results-container">
            <div v-if="searchResults.length > 0" class="users-grid">
              <div
                v-for="user in searchResults"
                :key="user.id"
                class="user-card"
              >
                <div class="user-card-header">
                  <div class="user-avatar">
                    {{ getInitials(user.username) }}
                  </div>
                  <div class="user-info">
                    <h4 class="username">{{ user.username }}</h4>
                    <p class="user-id">ID: {{ user.id }}</p>
                    <div class="user-tags">
                      <span v-if="user.rank > 0" class="tag admin-tag">管理员</span>
                      <span v-if="user.creater > 0" class="tag creator-tag">创作者</span>
                      <span v-if="user.isOnline" class="tag online-tag">在线</span>
                      <span v-else class="tag offline-tag">离线</span>
                    </div>
                  </div>
                </div>
                
                <div class="user-card-body">
                  <div class="user-stats">
                    <div class="stat-item">
                      <span class="stat-label">等级</span>
                      <span class="stat-value">Lv.{{ user.level || 1 }}</span>
                    </div>
                    <div class="stat-item">
                      <span class="stat-label">积分</span>
                      <span class="stat-value">{{ user.points || 0 }}</span>
                    </div>
                    <div class="stat-item">
                      <span class="stat-label">经验</span>
                      <span class="stat-value">{{ user.exp || 0 }}</span>
                    </div>
                    <div class="stat-item">
                      <span class="stat-label">点赞</span>
                      <span class="stat-value">{{ user.likes || 0 }}</span>
                    </div>
                  </div>
                  <div v-if="user.title" class="user-title">
                    <span class="title-text">{{ user.title }}</span>
                  </div>
                </div>

                <div class="user-card-actions">
                  <button
                    v-if="user.id === currentUserId"
                    class="btn btn-disabled"
                    disabled
                  >
                    这是您自己
                  </button>
                  <button
                    v-else-if="user.isFriend"
                    class="btn btn-friend"
                    disabled
                  >
                    ✅ 已是好友
                  </button>
                  <button
                    v-else-if="user.hasPendingRequest"
                    class="btn btn-pending"
                    disabled
                  >
                    ⏳ 请求中
                  </button>
                  <button
                    v-else-if="user.hasReceivedRequest"
                    class="btn btn-received"
                    @click="handleReceivedRequest(user)"
                  >
                    📥 处理请求
                  </button>
                  <button
                    v-else
                    class="btn btn-primary"
                    @click="sendFriendRequest(user)"
                    :disabled="sendingRequest === user.id"
                  >
                    <span v-if="sendingRequest === user.id" class="spinner-tiny"></span>
                    {{ sendingRequest === user.id ? '发送中...' : '➕ 添加好友' }}
                  </button>
                </div>
              </div>
            </div>

            <!-- 搜索结果空状态 -->
            <div v-else-if="searchKeyword && !searching" class="empty-state">
              <div class="empty-icon">🔍</div>
              <h3>未找到相关用户</h3>
              <p>请检查搜索关键词是否正确</p>
            </div>

            <!-- 初始状态提示 -->
            <div v-else-if="!searchKeyword" class="initial-state">
              <div class="initial-icon">🔍</div>
              <h3>搜索用户</h3>
              <p>在左侧输入用户ID或用户名搜索用户</p>
            </div>

            <!-- 加载状态 -->
            <div v-if="searching" class="loading-state">
              <div class="spinner-large"></div>
              <p>搜索用户中...</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 操作结果提示 -->
    <div v-if="showToast" class="toast" :class="toastType">
      <span class="toast-icon">{{ toastIcon }}</span>
      <span class="toast-message">{{ toastMessage }}</span>
    </div>

    <!-- 删除确认对话框 -->
    <div v-if="showDeleteConfirm" class="modal-overlay">
      <div class="confirm-modal">
        <div class="modal-header">
          <h3>确认删除好友</h3>
        </div>
        <div class="modal-body">
          <p>确定要删除好友 <strong>{{ friendToDelete?.FriendName }}</strong> 吗？此操作不可撤销。</p>
        </div>
        <div class="modal-actions">
          <button class="btn btn-cancel" @click="cancelDelete">取消</button>
          <button class="btn btn-danger" @click="deleteFriend">确认删除</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '../utils/api'

// 响应式数据
const searchKeyword = ref('')
const searching = ref(false)
const searchResults = ref([])
const currentUserId = ref(null)
const sendingRequest = ref(null)
const handlingRequest = ref(null)
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const toastIcon = ref('')

// 好友列表相关
const friendsList = ref([])
const pendingRequests = ref([])
const loadingFriends = ref(false)
const loadingPending = ref(false)
const sortBy = ref('online')
const filterBy = ref('all')
const showDeleteConfirm = ref(false)
const friendToDelete = ref(null)

// 添加缺失的工具函数
const getInitials = (username) => {
  if (!username) return '?'
  return username.charAt(0).toUpperCase()
}

const formatDate = (dateString) => {
  if (!dateString) return '未知时间'
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN')
}

// 计算属性
const onlineFriendsCount = computed(() => {
  return friendsList.value.filter(friend => friend.IsOnline).length
})

const offlineFriendsCount = computed(() => {
  return friendsList.value.filter(friend => !friend.IsOnline).length
})

const filteredFriends = computed(() => {
  let filtered = [...friendsList.value]
  
  if (filterBy.value === 'online') {
    filtered = filtered.filter(friend => friend.IsOnline)
  } else if (filterBy.value === 'offline') {
    filtered = filtered.filter(friend => !friend.IsOnline)
  }
  
  switch (sortBy.value) {
    case 'online':
      filtered.sort((a, b) => (b.IsOnline - a.IsOnline) || a.FriendName.localeCompare(b.FriendName))
      break
    case 'name':
      filtered.sort((a, b) => a.FriendName.localeCompare(b.FriendName))
      break
    case 'level':
      filtered.sort((a, b) => b.FriendLevel - a.FriendLevel)
      break
    case 'recent':
      filtered.sort((a, b) => new Date(b.CreateTime) - new Date(a.CreateTime))
      break
  }
  
  return filtered
})

// 生命周期钩子
onMounted(async () => {
  await getCurrentUser()
  await loadPendingRequests()
  await loadFriendsList()
})

// 获取当前用户ID
const getCurrentUser = async () => {
  try {
    const response = await apiClient.get('/Userinfo/me')
    if (response.data && response.data.success) {
      currentUserId.value = response.data.id
    }
  } catch (error) {
    console.error('获取用户信息失败:', error)
    showToastMessage('获取用户信息失败', 'error', '❌')
  }
}

// 修改加载待处理请求的方法，确保 RequestId 正确赋值
const loadPendingRequests = async () => {
  try {
    const response = await apiClient.get('/Friends/pending-requests')
    
    if (response.data && response.data.success) {
      console.log('后端返回的待处理请求数据:', response.data.data) // 调试
      
      pendingRequests.value = response.data.data.map(request => {
        // 确保 RequestId 正确赋值
        const requestId = request.RequestId || request.id || request.requestId || 0
        console.log('单个请求数据:', request, '提取的RequestId:', requestId) // 调试
        
        return {
          RequestId: requestId,
          FromUserId: request.FromUserId,
          FromUserName: request.FromUserName,
          FromUserLevel: request.FromUserLevel || 1,
          FromUserAvatar: request.FromUserAvatar || '',
          FromUserTitle: request.FromUserTitle || '',
          Remark: request.Remark || '',
          CreateTime: request.CreateTime
        }
      })
      
      console.log('处理后的pendingRequests:', pendingRequests.value) // 调试
    } else {
      showToastMessage(response.data?.message || '获取待处理请求失败', 'error', '❌')
      pendingRequests.value = []
    }
  } catch (error) {
    console.error('获取待处理请求失败:', error)
    showToastMessage('获取待处理请求失败，请重试', 'error', '❌')
    pendingRequests.value = []
  }
}

// 加载好友列表
// 加载好友列表
const loadFriendsList = async () => {
  try {
    loadingFriends.value = true
    const response = await apiClient.get('/Friends/list')
    
    if (response.data && response.data.success) {
      // 🐛 修复点：后端返回的是大写开头 (FriendId)，不要用小写 (friend.friendId)
      friendsList.value = response.data.data.map(friend => ({
        FriendId: friend.FriendId,       // 修改为大写
        FriendName: friend.FriendName,   // 修改为大写
        FriendLevel: friend.FriendLevel || 1, // 修改为大写
        IsOnline: friend.IsOnline || false,   // 修改为大写
        CreateTime: friend.CreateTime,   // 修改为大写
        Avatar: friend.FriendAvatar || '' // 修改为大写
      }))
    }
  } catch (error) {
    console.error('加载好友列表失败:', error)
    const message = error.response?.data?.message || '加载失败'
    showToastMessage(message, 'error', '❌')
  } finally {
    loadingFriends.value = false
  }
}

const searchUsers = async () => {
  try {
    if (!searchKeyword.value.trim()) {
      showToastMessage('请输入搜索关键词', 'warning', '⚠️')
      return
    }

    searching.value = true
    searchResults.value = []

    // 使用正确的API路径：/Friends/search
    const response = await apiClient.get('/Friends/search', {
      params: {
        keyword: searchKeyword.value,
        page: 1,
        pageSize: 20
      }
    })

    if (response.data && response.data.success) {
      searchResults.value = response.data.data.users.map(user => ({
        id: user.id,
        username: user.username,
        level: user.level || 1,
        points: user.points || 0,
        exp: user.exp || 0,
        likes: user.likes || 0,
        title: user.title || '',
        logo: user.logo || '',
        isOnline: user.isOnline || false,
        isFriend: user.isFriend || false,
        hasPendingRequestFromMe: user.hasPendingRequestFromMe || false,
        hasPendingRequestToMe: user.hasPendingRequestToMe || false
      }))
      
      if (searchResults.value.length === 0) {
        showToastMessage('未找到相关用户', 'info', 'ℹ️')
      }
    } else {
      showToastMessage(response.data?.message || '搜索失败', 'error', '❌')
    }
  } catch (error) {
    console.error('搜索用户失败:', error)
    const message = error.response?.data?.message || '搜索失败，请重试'
    showToastMessage(message, 'error', '❌')
    searchResults.value = []
  } finally {
    searching.value = false
  }
}

const sendFriendRequest = async (user) => {
  // 发送好友请求逻辑
  try {
    sendingRequest.value = user.id
    const response = await apiClient.post('/Friends/send-request', {
      toUserId: user.id,
      remark: `你好，我是用户${currentUserId.value}，想添加你为好友`
    })
    
    if (response.data && response.data.success) {
      showToastMessage('好友请求发送成功', 'success', '✅')
    }
  } catch (error) {
    console.error('发送请求失败:', error)
    showToastMessage('发送失败', 'error', '❌')
  } finally {
    sendingRequest.value = null
  }
}

// 修改处理好友请求方法，添加详细调试
const handleRequest = async (requestId, action) => {
  try {
    console.log('=== 处理好友请求调试信息 ===')
    console.log('收到的参数 - requestId:', requestId, '类型:', typeof requestId)
    console.log('收到的参数 - action:', action, '类型:', typeof action)
    
    // 检查requestId是否有效
    if (!requestId) {
      console.log('❌ requestId 为空')
      showToastMessage('请求ID不能为空', 'error', '❌')
      return
    }
    
    if (requestId <= 0) {
      console.log('❌ requestId 小于等于0:', requestId)
      showToastMessage('请求ID无效', 'error', '❌')
      return
    }
    
    if (isNaN(requestId)) {
      console.log('❌ requestId 不是数字:', requestId)
      showToastMessage('请求ID格式错误', 'error', '❌')
      return
    }
    
    if (!action) {
      console.log('❌ action 为空')
      showToastMessage('操作类型不能为空', 'error', '❌')
      return
    }

    // 确保action是小写
    const normalizedAction = action.toLowerCase()
    console.log('标准化后的action:', normalizedAction)
    
    if (normalizedAction !== 'accept' && normalizedAction !== 'reject') {
      console.log('❌ action 值无效:', normalizedAction)
      showToastMessage('操作类型无效', 'error', '❌')
      return
    }

    // 准备请求数据
    const requestData = {
      RequestId: Number(requestId), // 强制转换为数字
      Action: normalizedAction
    }
    
    console.log('准备发送的请求数据:', requestData)

    // 发送请求
    const response = await apiClient.post('/Friends/handle-request', requestData, {
      headers: {
        'Content-Type': 'application/json'
      }
    })

    console.log('后端响应:', response.data)

    if (response.data && response.data.success) {
      const actionText = normalizedAction === 'accept' ? '同意' : '拒绝'
      showToastMessage(`已${actionText}好友请求`, 'success', '✅')
      
      // 从待处理列表中移除
      pendingRequests.value = pendingRequests.value.filter(
        req => req.RequestId !== requestId
      )
      
      // 如果是同意，刷新好友列表
      if (normalizedAction === 'accept') {
        await loadFriendsList()
      }
    } else {
      showToastMessage(response.data?.message || '处理失败', 'error', '❌')
    }
  } catch (error) {
    console.error('处理好友请求失败:', error)
    console.error('错误响应数据:', error.response?.data)
    
    let message = '处理失败，请重试'
    if (error.response?.data) {
      // 显示后端返回的具体错误信息
      message = error.response.data.message || 
               error.response.data.title || 
               JSON.stringify(error.response.data)
    }
    
    showToastMessage(message, 'error', '❌')
  }
}

// 显示提示消息
const showToastMessage = (message, type = 'success', icon = '✅') => {
  toastMessage.value = message
  toastType.value = type
  toastIcon.value = icon
  showToast.value = true
  
  setTimeout(() => {
    showToast.value = false
  }, 3000)
}
</script>

<style scoped>
.friend-request-container {
  width: 100%;
  max-width: none;
  margin: 0;
  padding: 20px;
  background: white;
  min-height: 100vh;
  box-sizing: border-box;
}

/* 主布局容器 */
.main-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

/* 左侧区域 */
.left-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 右侧区域 */
.right-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 搜索区域 */
.search-section {
  background: #f8fafc;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
}

.search-header {
  text-align: center;
  margin-bottom: 20px;
}

.section-title {
  font-size: 1.8rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 8px 0;
}

.section-subtitle {
  color: #64748b;
  margin: 0;
  font-size: 1rem;
}

.search-input-group {
  display: flex;
  gap: 12px;
  max-width: 100%;
}

.search-input {
  flex: 1;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 16px;
  transition: all 0.3s ease;
  background: white;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.search-btn {
  padding: 12px 24px;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  white-space: nowrap;
}

.search-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

.search-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

/* 待处理请求样式 */
.pending-requests-section {
  background: white;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.section-header {
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-header h3 {
  margin: 0;
  color: #1e293b;
  font-size: 1.3rem;
}

.refresh-btn {
  padding: 6px 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.8rem;
  display: flex;
  align-items: center;
  gap: 4px;
  transition: all 0.3s ease;
}

.refresh-btn:hover:not(:disabled) {
  background: #f1f5f9;
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.requests-container {
  max-height: 300px;
  overflow-y: auto;
}

.requests-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.request-card {
  background: white;
  border: 2px solid #f1f5f9;
  border-radius: 8px;
  padding: 16px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.request-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #cbd5e1;
}

.request-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 12px;
}

.user-avatar {
  position: relative;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  font-weight: 600;
  flex-shrink: 0;
}

.request-info {
  flex: 1;
  min-width: 0;
}

.username {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-id {
  color: #64748b;
  font-size: 0.8rem;
  margin: 0 0 6px 0;
}

.user-meta {
  display: flex;
  gap: 8px;
  font-size: 0.8rem;
  color: #64748b;
  flex-wrap: wrap;
  margin-bottom: 8px;
}

.level {
  color: #059669;
  font-weight: 500;
}

.time {
  font-size: 0.7rem;
}

.remark {
  font-size: 0.8rem;
  color: #94a3b8;
  margin: 8px 0 0 0;
  font-style: italic;
  padding: 8px;
  background: #f8fafc;
  border-radius: 4px;
  border-left: 3px solid #3b82f6;
}

.request-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  min-width: 80px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-accept {
  background: #10b981;
  color: white;
}

.btn-accept:hover:not(:disabled) {
  background: #059669;
  transform: translateY(-1px);
}

.btn-reject {
  background: #ef4444;
  color: white;
}

.btn-reject:hover:not(:disabled) {
  background: #dc2626;
  transform: translateY(-1px);
}

/* 好友列表区域 */
.friends-section {
  background: white;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  flex: 1;
  display: flex;
  flex-direction: column;
}

.section-title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.section-title-row h3 {
  margin: 0;
  color: #1e293b;
  font-size: 1.3rem;
}

.friends-stats {
  display: flex;
  align-items: center;
  gap: 15px;
  font-size: 0.9rem;
  color: #64748b;
}

.stat-item {
  padding: 4px 8px;
  background: #f1f5f9;
  border-radius: 4px;
}

.friends-filter {
  display: flex;
  gap: 20px;
  margin-bottom: 15px;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.filter-group label {
  font-size: 0.9rem;
  color: #64748b;
}

.filter-select {
  padding: 6px 10px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  background: white;
  font-size: 0.9rem;
}

/* 好友列表容器 */
.friends-container {
  flex: 1;
  overflow-y: auto;
  max-height: 600px;
}

.friends-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.friend-card {
  background: white;
  border: 2px solid #f1f5f9;
  border-radius: 12px;
  padding: 16px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.friend-card.online {
  border-left: 4px solid #10b981;
}

.friend-card:not(.online) {
  border-left: 4px solid #94a3b8;
}

.friend-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #cbd5e1;
}

.friend-card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.friend-avatar {
  position: relative;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  font-weight: 600;
  flex-shrink: 0;
}

.online-indicator {
  position: absolute;
  bottom: 2px;
  right: 2px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #d1d5db;
  border: 2px solid white;
}

.online-indicator.online {
  background: #10b981;
}

.friend-info {
  flex: 1;
  min-width: 0;
}

.friend-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.friend-id {
  color: #64748b;
  font-size: 0.8rem;
  margin: 0 0 6px 0;
}

.friend-meta {
  display: flex;
  gap: 8px;
  font-size: 0.8rem;
  color: #64748b;
  flex-wrap: wrap;
}

.level {
  color: #059669;
  font-weight: 500;
}

.status {
  padding: 2px 6px;
  border-radius: 10px;
  font-size: 0.7rem;
  font-weight: 500;
}

.friend-card.online .status {
  background: #d1fae5;
  color: #065f46;
}

.friend-card:not(.online) .status {
  background: #f3f4f6;
  color: #6b7280;
}

.add-time {
  font-size: 0.7rem;
}

.friend-card-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn-chat {
  background: #3b82f6;
  color: white;
}

.btn-chat:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
}

.btn-profile {
  background: #f59e0b;
  color: white;
}

.btn-profile:hover:not(:disabled) {
  background: #d97706;
  transform: translateY(-1px);
}

.btn-delete {
  background: #ef4444;
  color: white;
}

.btn-delete:hover:not(:disabled) {
  background: #dc2626;
  transform: translateY(-1px);
}

/* 右侧搜索结果区域 */
.results-section {
  background: white;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 500px;
}

.results-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e2e8f0;
}

.results-header h3 {
  margin: 0;
  color: #374151;
  font-size: 1.3rem;
}

.results-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.results-count {
  font-size: 0.9rem;
  color: #64748b;
}

.clear-results {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.8rem;
  transition: all 0.3s ease;
}

.clear-results:hover {
  background: #e2e8f0;
}

/* 搜索结果容器 */
.results-container {
  flex: 1;
  overflow-y: auto;
  max-height: 600px;
  position: relative;
}

.users-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.user-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #cbd5e1;
}

.user-card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.user-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  font-weight: 600;
  flex-shrink: 0;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.username {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-id {
  color: #64748b;
  font-size: 0.8rem;
  margin: 0 0 6px 0;
}

.user-tags {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.tag {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: 600;
}

.admin-tag {
  background: #fef3c7;
  color: #92400e;
}

.creator-tag {
  background: #f3e8ff;
  color: #7c3aed;
}

.online-tag {
  background: #d1fae5;
  color: #065f46;
}

.offline-tag {
  background: #f3f4f6;
  color: #6b7280;
}

.user-card-body {
  margin-bottom: 12px;
}

.user-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
  margin-bottom: 8px;
}

.stat-item {
  text-align: center;
  padding: 6px;
  background: #f8fafc;
  border-radius: 6px;
}

.stat-label {
  display: block;
  font-size: 0.7rem;
  color: #64748b;
  margin-bottom: 2px;
}

.stat-value {
  display: block;
  font-weight: 600;
  color: #1e293b;
  font-size: 0.8rem;
}

.user-title {
  text-align: center;
  padding: 4px 8px;
  background: #f1f5f9;
  border-radius: 6px;
  font-size: 0.8rem;
  color: #475569;
  font-style: italic;
}

.user-card-actions {
  text-align: center;
}

/* 按钮样式 */
.btn-disabled {
  background: #f1f5f9;
  color: #94a3b8;
  border: 1px solid #e2e8f0;
}

.btn-friend {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.btn-pending {
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fde68a;
}

.btn-received {
  background: #dbeafe;
  color: #1e40af;
  border: 1px solid #93c5fd;
}

.btn-received:hover:not(:disabled) {
  background: #93c5fd;
  transform: translateY(-1px);
}

.btn-primary {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

/* 空状态和加载状态 */
.empty-state, .loading-state, .empty-friends, .empty-requests, .initial-state {
  text-align: center;
  padding: 40px 20px;
  color: #64748b;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
}

.empty-state, .empty-friends, .empty-requests, .initial-state {
  background: #f8fafc;
  border-radius: 12px;
  border: 2px dashed #e2e8f0;
}

.loading-state {
  background: transparent;
  border: none;
}

.empty-icon, .initial-icon {
  font-size: 2.5rem;
  margin-bottom: 16px;
}

.loading-state h3, .empty-state h3, .empty-friends h3, .empty-requests h3, .initial-state h3 {
  margin: 0 0 8px 0;
  color: #475569;
}

.loading-state p, .empty-state p, .empty-friends p, .empty-requests p, .initial-state p {
  margin: 0;
  font-size: 0.9rem;
}

/* 旋转动画 */
.spinner-large {
  width: 40px;
  height: 40px;
  border: 3px solid #e2e8f0;
  border-top: 3px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  display: inline-block;
}

.spinner-tiny {
  width: 14px;
  height: 14px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  display: inline-block;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 提示消息 */
.toast {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 12px 20px;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  z-index: 1000;
  animation: slideIn 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.toast.success {
  background: #10b981;
}

.toast.error {
  background: #ef4444;
}

.toast.warning {
  background: #f59e0b;
}

.toast.info {
  background: #3b82f6;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

/* 模态框 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease;
}

.confirm-modal {
  background: white;
  border-radius: 12px;
  padding: 0;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: scaleIn 0.3s ease;
}

.modal-header {
  padding: 20px 20px 0;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h3 {
  margin: 0;
  color: #1e293b;
}

.modal-body {
  padding: 20px;
}

.modal-body p {
  margin: 0;
  color: #64748b;
  line-height: 1.5;
}

.modal-actions {
  padding: 0 20px 20px;
  display: flex;
  gap: 10px;
  justify-content: flex-end;
}

.btn-cancel {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-cancel:hover {
  background: #e2e8f0;
}

.btn-danger {
  background: #ef4444;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-danger:hover {
  background: #dc2626;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes scaleIn {
  from {
    transform: scale(0.9);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

/* 滚动条样式 */
.requests-container::-webkit-scrollbar,
.friends-container::-webkit-scrollbar,
.results-container::-webkit-scrollbar {
  width: 6px;
}

.requests-container::-webkit-scrollbar-track,
.friends-container::-webkit-scrollbar-track,
.results-container::-webkit-scrollbar-track {
  background: #f1f5f9;
  border-radius: 3px;
}

.requests-container::-webkit-scrollbar-thumb,
.friends-container::-webkit-scrollbar-thumb,
.results-container::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.requests-container::-webkit-scrollbar-thumb:hover,
.friends-container::-webkit-scrollbar-thumb:hover,
.results-container::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .main-layout {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .left-panel, .right-panel {
    min-height: auto;
  }
}

@media (max-width: 768px) {
  .friend-request-container {
    padding: 15px;
  }
  
  .search-input-group {
    flex-direction: column;
  }
  
  .section-header {
    flex-direction: column;
    gap: 10px;
    align-items: stretch;
  }
  
  .section-title-row {
    flex-direction: column;
    gap: 10px;
    align-items: stretch;
  }
  
  .friends-stats {
    justify-content: center;
  }
  
  .friends-filter {
    flex-direction: column;
    gap: 10px;
  }
  
  .friend-card-actions,
  .request-actions {
    flex-direction: column;
  }
  
  .friend-card-header,
  .request-header {
    flex-direction: column;
    text-align: center;
  }
  
  .user-stats {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .friends-container, .results-container, .requests-container {
    max-height: 400px;
  }
  
  .results-header {
    flex-direction: column;
    gap: 10px;
    align-items: stretch;
  }
  
  .results-controls {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .section-title {
    font-size: 1.5rem;
  }
  
  .user-card, .friend-card, .request-card {
    padding: 12px;
  }
  
  .user-stats {
    grid-template-columns: 1fr;
  }
  
  .toast {
    left: 10px;
    right: 10px;
    top: 10px;
  }
  
  .friend-meta, .user-meta {
    flex-direction: column;
    align-items: center;
    gap: 4px;
  }
  
  .btn {
    padding: 10px 12px;
    font-size: 0.7rem;
  }
  
  .search-input {
    font-size: 14px;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .friend-card, .user-card, .request-card {
    border-width: 2px;
  }
  
  .btn {
    border-width: 2px;
  }
}

/* 减少动画模式支持 */
@media (prefers-reduced-motion: reduce) {
  .friend-card, .user-card, .request-card,
  .btn, .toast, .modal-overlay, .confirm-modal {
    transition: none;
    animation: none;
  }
}
</style>