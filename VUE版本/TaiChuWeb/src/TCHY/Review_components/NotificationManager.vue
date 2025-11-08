<template>
  <div class="notification-manager">
    <div class="header">
      <h2>系统通知管理</h2>
      <p>向用户发送系统通知和管理固定通知</p>
    </div>

    <div class="tabs">
      <div class="tab" :class="{ active: activeTab === 'send' }" @click="activeTab = 'send'">
        <i class="fas fa-paper-plane"></i> 发送通知
      </div>
      <div class="tab" :class="{ active: activeTab === 'fixed' }" @click="activeTab = 'fixed'">
        <i class="fas fa-bullhorn"></i> 固定通知
      </div>
      <div class="tab" :class="{ active: activeTab === 'history' }" @click="activeTab = 'history'">
        <i class="fas fa-history"></i> 发送记录 ({{ notificationHistory.length }})
      </div>
    </div>

    <!-- 发送通知 -->
    <div v-if="activeTab === 'send'" class="tab-content">
      <div class="send-notification">
        <!-- 用户统计信息 -->
        <div class="stats-section" v-if="sendStats">
          <h3>用户统计</h3>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-label">总用户数:</span>
              <span class="stat-value">{{ sendStats.totalUsers }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">活跃用户:</span>
              <span class="stat-value">{{ sendStats.activeUsers }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">新用户:</span>
              <span class="stat-value">{{ sendStats.newUsers }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">VIP用户:</span>
              <span class="stat-value">{{ sendStats.vipUsers }}</span>
            </div>
          </div>
        </div>

        <div class="form-section">
          <h3>选择接收用户</h3>
          <div class="user-selection">
            <div class="selection-type">
              <label>
                <input type="radio" v-model="sendType" value="all" /> 
                <span class="radio-label">全部用户 ({{ sendStats?.totalUsers || 0 }}人)</span>
              </label>
              <label>
                <input type="radio" v-model="sendType" value="selected" /> 
                <span class="radio-label">选定用户</span>
              </label>
              <label>
                <input type="radio" v-model="sendType" value="group" /> 
                <span class="radio-label">用户组</span>
              </label>
            </div>

            <div v-if="sendType === 'selected'" class="selected-users">
              <div class="user-search">
                <input v-model="userSearch" type="text" placeholder="搜索用户ID或用户名" @keyup.enter="searchUser" />
                <button @click="searchUser" class="search-btn">
                  <i class="fas fa-search"></i> 搜索
                </button>
              </div>
              
              <div v-if="searchResults.length" class="search-results">
                <div class="search-header">搜索结果 ({{ searchResults.length }})</div>
                <div v-for="user in searchResults" :key="user.id" class="user-item">
                  <span class="user-info">{{ user.username }} (ID: {{ user.id }})</span>
                  <button @click="addUser(user)" class="add-user-btn">添加</button>
                </div>
              </div>
              
              <div class="selected-list" v-if="selectedUsers.length > 0">
                <div class="selected-header">
                  <h4>已选用户 ({{ selectedUsers.length }})</h4>
                  <button @click="selectedUsers = []" class="clear-all-btn">清空</button>
                </div>
                <div v-for="user in selectedUsers" :key="user.id" class="selected-user">
                  <span class="user-info">{{ user.username }} (ID: {{ user.id }})</span>
                  <button @click="removeUser(user.id)" class="remove-btn">×</button>
                </div>
              </div>
              
              <div v-if="selectedUsers.length === 0 && searchResults.length === 0" class="empty-hint">
                请搜索并选择要发送的用户
              </div>
            </div>

            <div v-if="sendType === 'group'" class="group-selection">
              <div class="group-options">
                <label v-for="group in ['all', 'vip', 'new', 'active', 'inactive']" :key="group" class="group-option">
                  <input type="radio" v-model="selectedGroup" :value="group" />
                  <span class="group-label">{{ getGroupDescription(group) }}</span>
                </label>
              </div>
            </div>
          </div>
        </div>

        <div class="form-section">
          <h3>通知内容</h3>
          <div class="notification-content">
            <div class="input-group">
              <label>通知标题:</label>
              <input v-model="notification.title" type="text" placeholder="请输入通知标题" class="title-input" />
            </div>
            
            <div class="input-group">
              <label>通知内容:</label>
              <textarea v-model="notification.content" placeholder="请输入通知内容" rows="6" class="content-textarea"></textarea>
            </div>
            
            <div class="notification-options">
              <label class="option-item">
                <input type="checkbox" v-model="notification.important" />
                <span class="option-label">重要通知</span>
              </label>
              <label class="option-item">
                <input type="checkbox" v-model="notification.requiresAck" />
                <span class="option-label">需要用户确认</span>
              </label>
            </div>

            <div class="expiry-section">
              <label class="expiry-label">
                <span>设置过期时间:</span>
                <input v-model="notification.expiresAt" type="datetime-local" class="expiry-input" />
              </label>
              <span v-if="notification.expiresAt" class="expiry-hint">
                将于 {{ formatDate(notification.expiresAt) }} 过期
              </span>
            </div>
          </div>
        </div>

        <div class="actions">
          <button @click="sendNotification" :disabled="sending || !notification.title || !notification.content" 
                  class="send-btn" :class="{ disabled: sending || !notification.title || !notification.content }">
            <i class="fas fa-paper-plane"></i>
            {{ sending ? '发送中...' : '发送通知' }}
          </button>
          <button @click="resetForm" class="reset-btn">
            <i class="fas fa-redo"></i> 重置
          </button>
        </div>
      </div>
    </div>

    <!-- 固定通知管理 -->
    <div v-if="activeTab === 'fixed'" class="tab-content">
      <div class="fixed-notifications">
        <div class="fixed-header">
          <h3>固定通知管理</h3>
          <button @click="showCreateFixed = true" class="add-btn">添加固定通知</button>
        </div>
        <div class="empty-state">
          功能开发中...
        </div>
      </div>
    </div>

    <!-- 发送记录 -->
    <div v-if="activeTab === 'history'" class="tab-content">
      <div class="history">
        <div class="history-filters">
          <input v-model="historyFilter" type="text" placeholder="搜索通知标题或内容" class="filter-input" />
          <select v-model="historyType" class="filter-select">
            <option value="all">全部类型</option>
            <option value="immediate">即时通知</option>
            <option value="fixed">固定通知</option>
          </select>
        </div>

        <div class="history-list">
          <div v-if="filteredHistory.length === 0" class="empty-state">
            <i class="fas fa-inbox"></i>
            <p>暂无发送记录</p>
          </div>
          
          <div v-else>
            <div v-for="record in filteredHistory" :key="record.id" class="history-item">
              <div class="history-content">
                <h4>{{ record.title }}</h4>
                <p class="history-content-text">{{ record.content }}</p>
                <div class="history-meta">
                  <span class="recipient">发送给: {{ getRecipientText(record) }}</span>
                  <span class="time">时间: {{ formatDate(record.createdAt) }}</span>
                  <span class="status" :class="record.status === '发送成功' ? 'success' : 'error'">
                    状态: {{ record.status }}
                  </span>
                  <span v-if="record.sentCount" class="count">成功: {{ record.sentCount }}</span>
                  <span v-if="record.errorCount" class="count error">失败: {{ record.errorCount }}</span>
                </div>
              </div>
              <div class="history-actions">
                <button @click="resendHistory(record)" class="resend-btn" title="重新发送">
                  <i class="fas fa-redo"></i>
                </button>
                <button @click="deleteHistory(record.id)" class="delete-btn" title="删除记录">
                  <i class="fas fa-trash"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../../server/UserInfoApi'

const activeTab = ref('send')
const sendType = ref('all')
const userSearch = ref('')
const searchResults = ref([])
const selectedUsers = ref([])
const selectedGroup = ref('all')
const sending = ref(false)
const fixedNotifications = ref([])
const showCreateFixed = ref(false)
const historyFilter = ref('')
const historyType = ref('all')
const sendStats = ref(null)

const notification = ref({
  title: '',
  content: '',
  important: false,
  requiresAck: false,
  fixed: false,
  position: 'top',
  expiresAt: ''
})

// 模拟历史记录数据
const notificationHistory = ref([])

// 计算属性
const filteredHistory = computed(() => {
  return notificationHistory.value.filter(record => {
    const matchesFilter = record.title.includes(historyFilter.value) || 
                          record.content.includes(historyFilter.value)
    const matchesType = historyType.value === 'all' || record.type === historyType.value
    return matchesFilter && matchesType
  })
})

// 获取发送统计
const loadSendStats = async () => {
  try {
    const response = await request.get('/api/Notification/send-stats')
    sendStats.value = response.data
  } catch (error) {
    console.error('获取发送统计失败:', error)
    ElMessage.error('获取发送统计失败')
  }
}

// 搜索用户
const searchUser = async () => {
  if (!userSearch.value.trim()) {
    ElMessage.warning('请输入搜索关键词')
    return
  }

  try {
    // 调用用户搜索API
    const response = await request.get(`/api/Friends/search/${encodeURIComponent(userSearch.value)}`)
    searchResults.value = response.data || []
    
    if (searchResults.value.length === 0) {
      ElMessage.info('未找到相关用户')
    }
  } catch (error) {
    console.error('搜索用户失败:', error)
    ElMessage.error('搜索用户失败')
    searchResults.value = []
  }
}

const addUser = (user) => {
  if (!selectedUsers.value.find(u => u.id === user.id)) {
    selectedUsers.value.push(user)
    ElMessage.success(`已添加用户: ${user.username}`)
  }
}

const removeUser = (userId) => {
  const user = selectedUsers.value.find(u => u.id === userId)
  selectedUsers.value = selectedUsers.value.filter(u => u.id !== userId)
  if (user) {
    ElMessage.info(`已移除用户: ${user.username}`)
  }
}

// 发送通知的主要方法
const sendNotification = async () => {
  if (!notification.value.title || !notification.value.content) {
    ElMessage.error('请填写通知标题和内容')
    return
  }

  // 确认发送
  try {
    await ElMessageBox.confirm(
      `确定要发送这条通知吗？\n标题: ${notification.value.title}\n内容: ${notification.value.content}`,
      '确认发送',
      {
        confirmButtonText: '确定发送',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
  } catch (error) {
    if (error === 'cancel') return
  }

  try {
    sending.value = true

    let response
    const sendData = {
      title: notification.value.title,
      content: notification.value.content,
      important: notification.value.important,
      requiresAck: notification.value.requiresAck,
      expireTime: notification.value.expiresAt ? new Date(notification.value.expiresAt).toISOString() : null
    }

    switch (sendType.value) {
      case 'all':
        response = await request.post('/api/Notification/send-to-all', sendData)
        break
        
      case 'selected':
        if (selectedUsers.value.length === 0) {
          ElMessage.error('请至少选择一个用户')
          sending.value = false
          return
        }
        response = await request.post('/api/Notification/send-to-users', {
          ...sendData,
          userIds: selectedUsers.value.map(u => u.id),
          type: 1 // 系统通知
        })
        break
        
      case 'group':
        response = await request.post('/api/Notification/send-to-group', {
          ...sendData,
          groupType: selectedGroup.value,
          type: 1 // 系统通知
        })
        break
    }

    if (response.data) {
      const result = response.data
      let message = `通知发送成功！`
      
      if (result.sentCount > 0) {
        message += ` 成功发送: ${result.sentCount} 条`
      }
      if (result.errorCount > 0) {
        message += `，失败: ${result.errorCount} 条`
      }
      
      ElMessage.success(message)
      
      // 记录到发送历史
      addToHistory(result, sendType.value)
    }

    resetForm()
  } catch (error) {
    console.error('发送通知失败:', error)
    const errorMessage = error.response?.data?.message || '发送通知失败'
    ElMessage.error(errorMessage)
  } finally {
    sending.value = false
  }
}

// 添加到发送历史
const addToHistory = (result, sendType) => {
  const historyItem = {
    id: Date.now(),
    title: notification.value.title,
    content: notification.value.content,
    type: 'immediate',
    recipients: sendType,
    status: '发送成功',
    sentCount: result.sentCount,
    errorCount: result.errorCount || 0,
    createdAt: new Date(),
    groupType: sendType === 'group' ? selectedGroup.value : null
  }
  
  notificationHistory.value.unshift(historyItem)
}

const resetForm = () => {
  notification.value = {
    title: '',
    content: '',
    important: false,
    requiresAck: false,
    fixed: false,
    position: 'top',
    expiresAt: ''
  }
  selectedUsers.value = []
  searchResults.value = []
  sendType.value = 'all'
  selectedGroup.value = 'all'
}

const getPositionText = (position) => {
  const positions = {
    top: '页面顶部',
    bottom: '页面底部',
    popup: '弹窗提示'
  }
  return positions[position] || position
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('zh-CN')
}

const getRecipientText = (record) => {
  const recipientMap = {
    all: '全部用户',
    selected: `选定用户 (${record.sentCount || 0}人)`,
    vip: 'VIP用户',
    new: '新用户',
    active: '活跃用户',
    inactive: '非活跃用户'
  }
  
  if (record.groupType) {
    return recipientMap[record.groupType] || record.groupType
  }
  
  return recipientMap[record.recipients] || record.recipients
}

const getGroupDescription = (groupType) => {
  const descriptions = {
    all: `所有用户 (${sendStats.value?.totalUsers || 0}人)`,
    vip: `VIP用户 (等级≥10，${sendStats.value?.vipUsers || 0}人)`,
    new: `新用户 (7天内注册，${sendStats.value?.newUsers || 0}人)`,
    active: `活跃用户 (30天内有活动，${sendStats.value?.activeUsers || 0}人)`,
    inactive: `非活跃用户 (30天内无活动，${sendStats.value?.inactiveUsers || 0}人)`
  }
  return descriptions[groupType] || groupType
}

// 删除历史记录
const deleteHistory = (id) => {
  notificationHistory.value = notificationHistory.value.filter(item => item.id !== id)
  ElMessage.success('删除记录成功')
}

// 重新发送历史记录
const resendHistory = async (record) => {
  try {
    await ElMessageBox.confirm(
      '确定要重新发送这条通知吗？',
      '确认重新发送',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )

    // 根据记录类型重新发送
    if (record.recipients === 'all') {
      await request.post('/api/Notification/send-to-all', {
        title: record.title,
        content: record.content,
        important: false,
        requiresAck: false,
        expireTime: null
      })
    }
    
    ElMessage.success('重新发送成功')
  } catch (error) {
    if (error === 'cancel') return
    ElMessage.error('重新发送失败')
  }
}

onMounted(() => {
  loadSendStats()
})
</script>

<style scoped>
.notification-manager {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  margin-bottom: 30px;
  text-align: center;
}

.header h2 {
  margin: 0 0 8px 0;
  color: #333;
  font-size: 28px;
  font-weight: 600;
}

.header p {
  margin: 0;
  color: #666;
  font-size: 16px;
}

.tabs {
  display: flex;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 4px;
  margin-bottom: 30px;
  border: 1px solid #e9ecef;
}

.tab {
  flex: 1;
  padding: 12px 20px;
  text-align: center;
  cursor: pointer;
  border-radius: 8px;
  transition: all 0.3s ease;
  font-weight: 500;
  color: #6c757d;
  border: none;
  background: transparent;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.tab.active {
  background: #fff;
  color: #1890ff;
  box-shadow: 0 2px 8px rgba(24, 144, 255, 0.15);
}

.tab:hover:not(.active) {
  color: #1890ff;
  background: rgba(24, 144, 255, 0.05);
}

.tab-content {
  background: white;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border: 1px solid #f0f0f0;
}

/* 统计信息样式 */
.stats-section {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 24px;
  border-radius: 12px;
  margin-bottom: 30px;
}

.stats-section h3 {
  margin: 0 0 20px 0;
  font-size: 20px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 10px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 8px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.stat-label {
  font-size: 14px;
  opacity: 0.9;
  font-weight: 500;
}

.stat-value {
  font-size: 20px;
  font-weight: 700;
}

/* 表单区域样式 */
.form-section {
  margin-bottom: 32px;
}

.form-section h3 {
  margin: 0 0 16px 0;
  color: #333;
  font-size: 18px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-selection, .notification-content {
  border: 1px solid #e8e8e8;
  padding: 20px;
  border-radius: 8px;
  background: #fafafa;
}

/* 选择类型样式 */
.selection-type {
  display: flex;
  gap: 30px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.selection-type label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-weight: 500;
  color: #333;
  padding: 8px 16px;
  border-radius: 6px;
  transition: all 0.3s;
}

.selection-type label:hover {
  background: #f0f7ff;
}

.selection-type input[type="radio"] {
  margin-right: 8px;
  width: 16px;
  height: 16px;
}

.radio-label {
  font-weight: 500;
}

/* 用户搜索样式 */
.user-search {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
}

.user-search input {
  flex: 1;
  padding: 12px 16px;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s;
}

.user-search input:focus {
  outline: none;
  border-color: #1890ff;
  box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
}

.search-btn {
  padding: 12px 24px;
  background: #1890ff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.search-btn:hover {
  background: #40a9ff;
  transform: translateY(-1px);
}

/* 搜索结果样式 */
.search-results {
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  max-height: 200px;
  overflow-y: auto;
  margin-bottom: 20px;
  background: white;
}

.search-header {
  padding: 12px 16px;
  background: #f5f5f5;
  font-weight: 600;
  border-bottom: 1px solid #e8e8e8;
  color: #333;
}

.user-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.2s;
}

.user-item:hover {
  background: #f8f9fa;
}

.user-item:last-child {
  border-bottom: none;
}

.user-info {
  font-weight: 500;
  color: #333;
}

.add-user-btn {
  padding: 6px 12px;
  background: #52c41a;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.2s;
}

.add-user-btn:hover {
  background: #73d13d;
  transform: scale(1.05);
}

/* 已选用户样式 */
.selected-list {
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  background: white;
}

.selected-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #f8f9fa;
  border-bottom: 1px solid #e8e8e8;
}

.selected-header h4 {
  margin: 0;
  color: #333;
  font-size: 16px;
}

.clear-all-btn {
  padding: 6px 12px;
  background: #ff4d4f;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.2s;
}

.clear-all-btn:hover {
  background: #ff7875;
}

.selected-user {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f0f0f0;
}

.selected-user:last-child {
  border-bottom: none;
}

.remove-btn {
  padding: 4px 8px;
  background: #ff4d4f;
  color: white;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  transition: all 0.2s;
}

.remove-btn:hover {
  background: #ff7875;
  transform: scale(1.1);
}

.empty-hint {
  text-align: center;
  padding: 20px;
  color: #999;
  font-style: italic;
}

/* 用户组选择样式 */
.group-selection {
  margin-top: 16px;
}

.group-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.group-option {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  border: 1px solid #e8e8e8;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  transition: all 0.3s;
}

.group-option:hover {
  border-color: #1890ff;
  background: #f0f7ff;
}

.group-option input[type="radio"] {
  margin-right: 12px;
  width: 16px;
  height: 16px;
}

.group-label {
  font-weight: 500;
  color: #333;
}

/* 通知内容样式 */
.input-group {
  margin-bottom: 20px;
}

.input-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #333;
}

.title-input, .content-textarea {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s;
  font-family: inherit;
}

.title-input:focus, .content-textarea:focus {
  outline: none;
  border-color: #1890ff;
  box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
}

.content-textarea {
  resize: vertical;
  min-height: 120px;
  line-height: 1.5;
}

.notification-options {
  display: flex;
  gap: 24px;
  margin-bottom: 20px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 6px;
}

.option-item {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-weight: 500;
  color: #333;
}

.option-item input[type="checkbox"] {
  margin-right: 8px;
  width: 16px;
  height: 16px;
}

.expiry-section {
  padding: 16px;
  background: #fff9e6;
  border-radius: 6px;
  border: 1px solid #ffe58f;
}

.expiry-label {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
  font-weight: 500;
}

.expiry-input {
  padding: 8px 12px;
  border: 1px solid #d9d9d9;
  border-radius: 4px;
  font-size: 14px;
}

.expiry-hint {
  font-size: 12px;
  color: #faad14;
  font-style: italic;
}

/* 操作按钮样式 */
.actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #f0f0f0;
}

.send-btn {
  padding: 12px 32px;
  background: #1890ff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  font-size: 16px;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.send-btn:not(.disabled):hover {
  background: #40a9ff;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.3);
}

.send-btn.disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.reset-btn {
  padding: 12px 24px;
  background: #f5f5f5;
  color: #666;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.reset-btn:hover {
  background: #e8e8e8;
  border-color: #ccc;
}

/* 固定通知样式 */
.fixed-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.fixed-header h3 {
  margin: 0;
  color: #333;
}

.add-btn {
  padding: 10px 20px;
  background: #52c41a;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.add-btn:hover {
  background: #73d13d;
  transform: translateY(-1px);
}

/* 历史记录样式 */
.history-filters {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
}

.filter-input {
  flex: 1;
  padding: 10px 16px;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  font-size: 14px;
}

.filter-select {
  padding: 10px 16px;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  background: white;
  font-size: 14px;
}

.history-list {
  max-height: 500px;
  overflow-y: auto;
}

.history-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 20px;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  margin-bottom: 12px;
  background: white;
  transition: all 0.3s;
}

.history-item:hover {
  border-color: #1890ff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.history-content {
  flex: 1;
}

.history-content h4 {
  margin: 0 0 8px 0;
  color: #333;
  font-size: 16px;
  font-weight: 600;
}

.history-content-text {
  margin: 0 0 12px 0;
  color: #666;
  line-height: 1.5;
}

.history-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 12px;
  color: #999;
}

.history-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.status.success {
  color: #52c41a;
  font-weight: 500;
}

.status.error {
  color: #ff4d4f;
  font-weight: 500;
}

.count {
  padding: 2px 6px;
  background: #f0f0f0;
  border-radius: 3px;
}

.count.error {
  background: #fff2f0;
  color: #ff4d4f;
}

.history-actions {
  display: flex;
  gap: 8px;
  margin-left: 16px;
}

.resend-btn, .delete-btn {
  padding: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.resend-btn {
  background: #1890ff;
  color: white;
}

.resend-btn:hover {
  background: #40a9ff;
  transform: scale(1.1);
}

.delete-btn {
  background: #ff4d4f;
  color: white;
}

.delete-btn:hover {
  background: #ff7875;
  transform: scale(1.1);
}

/* 空状态样式 */
.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: #999;
}

.empty-state i {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty-state p {
  margin: 0;
  font-size: 16px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notification-manager {
    padding: 16px;
  }
  
  .tabs {
    flex-direction: column;
    gap: 4px;
  }
  
  .tab {
    padding: 16px;
  }
  
  .tab-content {
    padding: 20px;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .selection-type {
    flex-direction: column;
    gap: 12px;
  }
  
  .user-search {
    flex-direction: column;
  }
  
  .history-filters {
    flex-direction: column;
  }
  
  .history-item {
    flex-direction: column;
    gap: 16px;
  }
  
  .history-actions {
    margin-left: 0;
    justify-content: flex-end;
    width: 100%;
  }
  
  .actions {
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .header h2 {
    font-size: 24px;
  }
  
  .header p {
    font-size: 14px;
  }
  
  .stats-section {
    padding: 20px;
  }
  
  .stat-item {
    padding: 12px;
  }
  
  .stat-value {
    font-size: 18px;
  }
  
  .user-selection, .notification-content {
    padding: 16px;
  }
  
  .history-meta {
    flex-direction: column;
    gap: 8px;
  }
}

/* 滚动条样式 */
.search-results::-webkit-scrollbar,
.history-list::-webkit-scrollbar {
  width: 6px;
}

.search-results::-webkit-scrollbar-track,
.history-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.search-results::-webkit-scrollbar-thumb,
.history-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.search-results::-webkit-scrollbar-thumb:hover,
.history-list::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.tab-content {
  animation: fadeIn 0.3s ease;
}

/* 加载状态 */
.sending {
  opacity: 0.7;
  pointer-events: none;
}

.sending::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>