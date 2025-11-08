<template>
    <div class="friend-manager">
        <div class="header">
            <h1>好友管理</h1>
            <p>添加新朋友，管理您的好友和黑名单</p>
        </div>
        
        <div class="tabs">
            <div class="tab" :class="{active: activeTab === 'search'}" @click="activeTab = 'search'">
                <i class="fas fa-user-plus"></i> 添加好友
            </div>
            <div class="tab" :class="{active: activeTab === 'friends'}" @click="activeTab = 'friends'">
                <i class="fas fa-users"></i> 好友列表 ({{ friends.length }})
            </div>
            <div class="tab" :class="{active: activeTab === 'requests'}" @click="activeTab = 'requests'">
                <i class="fas fa-user-clock"></i> 好友请求 ({{ pendingRequests.length }})
            </div>
            <div class="tab" :class="{active: activeTab === 'blocked'}" @click="activeTab = 'blocked'">
                <i class="fas fa-ban"></i> 黑名单 ({{ blockedUsers.length }})
            </div>
        </div>
        
        <!-- 搜索添加好友 -->
        <div class="tab-content" v-if="activeTab === 'search'">
            <div class="search-container">
                <h2>添加好友</h2>
                <div class="search-box">
                    <input 
                        v-model="searchKeyword" 
                        type="text" 
                        placeholder="输入用户ID、用户名或邮箱进行搜索" 
                        @keyup.enter="searchUser"
                    />
                    <button @click="searchUser">
                        <i class="fas fa-search"></i> 搜索
                    </button>
                </div>
                
                <div class="search-history" v-if="searchHistory.length > 0">
                    <div class="history-tag" v-for="item in searchHistory" :key="item" @click="searchKeyword = item; searchUser()">
                        {{ item }}
                    </div>
                </div>
                
                <div v-if="searchResult" class="search-result">
                    <div class="user-card">
                        <div class="avatar">{{ getInitials(searchResult.username) }}</div>
                        <div class="user-info">
                            <h3>{{ searchResult.username }}</h3>
                            <p>ID: {{ searchResult.id }} | 最后活跃: {{ formatLastActive(searchResult.lastActive) }}</p>
                        </div>
                        <button 
                            v-if="!searchResult.isFriend && !searchResult.hasPendingRequest"
                            class="action-btn add-btn" 
                            @click="sendFriendRequest"
                            :disabled="searchResult.isBlocked"
                        >
                            发送好友请求
                        </button>
                        <button 
                            v-else-if="searchResult.hasPendingRequest"
                            class="action-btn pending-btn" 
                            disabled
                        >
                            请求已发送
                        </button>
                        <button 
                            v-else
                            class="action-btn friend-btn" 
                            disabled
                        >
                            已是好友
                        </button>
                        <button 
                            class="action-btn block-btn" 
                            @click="blockUser"
                            v-if="!searchResult.isBlocked"
                        >
                            加入黑名单
                        </button>
                        <button 
                            class="action-btn unblock-btn" 
                            @click="unblockUser"
                            v-else
                        >
                            移出黑名单
                        </button>
                    </div>
                </div>
                
                <div v-if="errorMessage" class="error-message message">
                    {{ errorMessage }}
                </div>
                
                <div v-if="successMessage" class="success-message message">
                    {{ successMessage }}
                </div>
            </div>
        </div>
        
        <!-- 好友列表 -->
        <div class="tab-content" v-if="activeTab === 'friends'">
            <h2>好友列表</h2>
            <div class="list-controls">
                <div class="list-search">
                    <input v-model="friendSearch" type="text" placeholder="搜索好友..." />
                </div>
                <div class="sort-options">
                    <select v-model="friendSortBy">
                        <option value="name">按姓名排序</option>
                        <option value="recent">最近活跃</option>
                        <option value="added">添加时间</option>
                    </select>
                </div>
            </div>
            
            <div class="friends-list">
                <div v-if="filteredFriends.length === 0" class="empty-state">
                    <i class="fas fa-user-friends"></i>
                    <p>暂无好友</p>
                </div>
                
                <div v-else>
                    <div class="friend-item" v-for="friend in filteredFriends" :key="friend.id">
                        <div class="friend-avatar">{{ getInitials(friend.username) }}</div>
                        <div class="friend-details">
                            <h4>{{ friend.remark || friend.username }}</h4>
                            <p>ID: {{ friend.id }} | 添加时间: {{ formatDate(friend.addedTime) }}</p>
                        </div>
                        <div class="friend-actions">
                            <button class="action-btn msg-btn" @click="sendMessage(friend)">
                                <i class="fas fa-comment"></i> 发消息
                            </button>
                            <button class="action-btn remove-btn" @click="removeFriend(friend)">
                                <i class="fas fa-user-minus"></i> 删除
                            </button>
                            <button class="action-btn block-btn" @click="blockFriend(friend)">
                                <i class="fas fa-ban"></i> 拉黑
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 好友请求 -->
        <div class="tab-content" v-if="activeTab === 'requests'">
            <h2>好友请求</h2>
            
            <!-- 收到的请求 -->
            <div class="requests-section">
                <h3>收到的请求 ({{ receivedRequests.length }})</h3>
                <div class="requests-list">
                    <div v-if="receivedRequests.length === 0" class="empty-state">
                        <i class="fas fa-inbox"></i>
                        <p>暂无收到的好友请求</p>
                    </div>
                    
                    <div v-else>
                        <div class="request-item" v-for="request in receivedRequests" :key="request.id">
                            <div class="request-avatar">{{ getInitials(request.fromUserName) }}</div>
                            <div class="request-details">
                                <h4>{{ request.fromUserName }}</h4>
                                <p>ID: {{ request.fromUserId }} | 请求时间: {{ formatDate(request.createTime) }}</p>
                                <p v-if="request.remark" class="request-remark">备注: {{ request.remark }}</p>
                            </div>
                            <div class="request-actions">
                                <button class="action-btn accept-btn" @click="acceptFriendRequest(request.id)">
                                    <i class="fas fa-check"></i> 同意
                                </button>
                                <button class="action-btn reject-btn" @click="rejectFriendRequest(request.id)">
                                    <i class="fas fa-times"></i> 拒绝
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- 发送的请求 -->
            <div class="requests-section">
                <h3>发送的请求 ({{ sentRequests.length }})</h3>
                <div class="requests-list">
                    <div v-if="sentRequests.length === 0" class="empty-state">
                        <i class="fas fa-paper-plane"></i>
                        <p>暂无发送的好友请求</p>
                    </div>
                    
                    <div v-else>
                        <div class="request-item" v-for="request in sentRequests" :key="request.id">
                            <div class="request-avatar">{{ getInitials(request.toUserName) }}</div>
                            <div class="request-details">
                                <h4>{{ request.toUserName }}</h4>
                                <p>ID: {{ request.toUserId }} | 发送时间: {{ formatDate(request.createTime) }}</p>
                                <p class="request-status" :class="getStatusClass(request.status)">
                                    状态: {{ getStatusText(request.status) }}
                                </p>
                            </div>
                            <div class="request-actions" v-if="request.status === 0">
                                <button class="action-btn cancel-btn" @click="cancelFriendRequest(request.id)">
                                    <i class="fas fa-trash"></i> 取消
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 黑名单 -->
        <div class="tab-content" v-if="activeTab === 'blocked'">
            <h2>黑名单</h2>
            <div class="list-controls">
                <div class="list-search">
                    <input v-model="blockedSearch" type="text" placeholder="搜索黑名单..." />
                </div>
            </div>
            
            <div class="blocked-list">
                <div v-if="filteredBlockedUsers.length === 0" class="empty-state">
                    <i class="fas fa-ban"></i>
                    <p>黑名单为空</p>
                </div>
                
                <div v-else>
                    <div class="blocked-item" v-for="user in filteredBlockedUsers" :key="user.id">
                        <div class="blocked-avatar">{{ getInitials(user.username) }}</div>
                        <div class="blocked-details">
                            <h4>{{ user.username }}</h4>
                            <p>ID: {{ user.id }} | 拉黑时间: {{ formatDate(user.blockedTime) }}</p>
                        </div>
                        <div class="blocked-actions">
                            <button class="action-btn unblock-btn" @click="unblockUserFromList(user)">
                                <i class="fas fa-user-check"></i> 移出黑名单
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import request from '../../../server/UserInfoApi';

// 响应式数据
const activeTab = ref('search');
const searchKeyword = ref('');
const searchResult = ref(null);
const errorMessage = ref('');
const successMessage = ref('');
const searchHistory = ref([]);
const friendSearch = ref('');
const friendSortBy = ref('name');
const blockedSearch = ref('');

// 实际数据 - 从API获取
const friends = ref([]);
const blockedUsers = ref([]);
const receivedRequests = ref([]);
const sentRequests = ref([]);

// 计算属性
const filteredFriends = computed(() => {
  let result = friends.value.filter(friend => 
    (friend.username && friend.username.toLowerCase().includes(friendSearch.value.toLowerCase())) ||
    (friend.remark && friend.remark.toLowerCase().includes(friendSearch.value.toLowerCase())) ||
    friend.id.toString().includes(friendSearch.value)
  );
  
  // 排序
  if (friendSortBy.value === 'name') {
    result.sort((a, b) => (a.remark || a.username).localeCompare(b.remark || b.username));
  } else if (friendSortBy.value === 'added') {
    result.sort((a, b) => new Date(b.addedTime) - new Date(a.addedTime));
  }
  
  return result;
});

const filteredBlockedUsers = computed(() => {
  return blockedUsers.value.filter(user => 
    user.username.toLowerCase().includes(blockedSearch.value.toLowerCase()) ||
    user.id.toString().includes(blockedSearch.value)
  );
});

const pendingRequests = computed(() => {
  return receivedRequests.value.length;
});

// 工具方法
const getInitials = (name) => {
  if (!name) return '?';
  return name.charAt(0).toUpperCase();
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('zh-CN') + ' ' + date.toLocaleTimeString('zh-CN', {hour: '2-digit', minute:'2-digit'});
};

const formatLastActive = (lastActive) => {
  if (!lastActive) return '未知';
  return lastActive;
};

const getStatusText = (status) => {
  switch (status) {
    case 0: return '待处理';
    case 1: return '已同意';
    case 2: return '已拒绝';
    default: return '未知';
  }
};

const getStatusClass = (status) => {
  switch (status) {
    case 0: return 'status-pending';
    case 1: return 'status-accepted';
    case 2: return 'status-rejected';
    default: return '';
  }
};

const showMessage = (message, type = 'success') => {
  if (type === 'success') {
    ElMessage.success(message);
  } else {
    ElMessage.error(message);
  }
};

const clearMessages = () => {
  errorMessage.value = '';
  successMessage.value = '';
};

// API方法 - 搜索用户
const searchUser = async () => {
  if (!searchKeyword.value.trim()) {
    errorMessage.value = '请输入搜索内容';
    return;
  }
  
  clearMessages();
  
  // 添加到搜索历史
  if (!searchHistory.value.includes(searchKeyword.value)) {
    searchHistory.value.unshift(searchKeyword.value);
    if (searchHistory.value.length > 5) {
      searchHistory.value.pop();
    }
  }
  
  try {
    const response = await request.get(`/api/Friends/search/${encodeURIComponent(searchKeyword.value)}`);
    if (response.data && response.data.length > 0) {
      searchResult.value = response.data[0]; // 取第一个结果
      
      // 检查是否有待处理的好友请求
      const requestCheck = await request.get(`/api/Friends/requests/sent`);
      if (requestCheck.data) {
        const hasPending = requestCheck.data.some(req => 
          req.toUserId === searchResult.value.id && req.status === 0
        );
        searchResult.value.hasPendingRequest = hasPending;
      }
    } else {
      searchResult.value = null;
      errorMessage.value = '未找到该用户';
    }
  } catch (error) {
    searchResult.value = null;
    const message = error.response?.data?.message || '搜索失败，请稍后再试';
    errorMessage.value = message;
    showMessage(message, 'error');
  }
};

// 发送好友请求
const sendFriendRequest = async () => {
  if (!searchResult.value) return;
  
  try {
    await ElMessageBox.confirm(
      `确定要向 ${searchResult.value.username} 发送好友请求吗？`,
      '发送好友请求',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.post('/api/Friends/request', {
      toUserId: searchResult.value.id,
      remark: ''
    });
    
    showMessage('好友请求发送成功');
    searchResult.value.hasPendingRequest = true;
    await loadSentRequests(); // 重新加载发送的请求
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '发送好友请求失败';
    showMessage(message, 'error');
  }
};

// 处理好友请求 - 同意
const acceptFriendRequest = async (requestId) => {
  try {
    await ElMessageBox.confirm(
      '确定要同意这个好友请求吗？',
      '同意好友请求',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.post(`/api/Friends/request/${requestId}/process`, {
      action: 1 // 同意
    });
    
    showMessage('好友请求已同意');
    await loadReceivedRequests(); // 重新加载收到的请求
    await loadFriends(); // 重新加载好友列表
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '处理好友请求失败';
    showMessage(message, 'error');
  }
};

// 处理好友请求 - 拒绝
const rejectFriendRequest = async (requestId) => {
  try {
    await ElMessageBox.confirm(
      '确定要拒绝这个好友请求吗？',
      '拒绝好友请求',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.post(`/api/Friends/request/${requestId}/process`, {
      action: 2 // 拒绝
    });
    
    showMessage('好友请求已拒绝');
    await loadReceivedRequests(); // 重新加载收到的请求
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '处理好友请求失败';
    showMessage(message, 'error');
  }
};

// 取消好友请求
const cancelFriendRequest = async (requestId) => {
  try {
    await ElMessageBox.confirm(
      '确定要取消这个好友请求吗？',
      '取消好友请求',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.delete(`/api/Friends/request/${requestId}`);
    
    showMessage('好友请求已取消');
    await loadSentRequests(); // 重新加载发送的请求
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '取消好友请求失败';
    showMessage(message, 'error');
  }
};

// 拉黑用户
const blockUser = async () => {
  if (!searchResult.value) return;
  
  try {
    await ElMessageBox.confirm(
      `确定要将 ${searchResult.value.username} 加入黑名单吗？`,
      '加入黑名单',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.post('/api/Friends/block', {
      blockID: searchResult.value.id
    });
    
    showMessage('用户已加入黑名单');
    await loadBlockedUsers(); // 重新加载黑名单
    searchResult.value.isBlocked = true;
    
    // 如果该用户是好友，从好友列表中移除
    if (searchResult.value.isFriend) {
      await loadFriends();
    }
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '拉黑用户失败';
    showMessage(message, 'error');
  }
};

// 取消拉黑用户
const unblockUser = async () => {
  if (!searchResult.value) return;
  
  try {
    await ElMessageBox.confirm(
      `确定要将 ${searchResult.value.username} 移出黑名单吗？`,
      '移出黑名单',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.delete(`/api/Friends/blocked/${searchResult.value.id}`);
    
    showMessage('已取消拉黑');
    await loadBlockedUsers(); // 重新加载黑名单
    searchResult.value.isBlocked = false;
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '取消拉黑失败';
    showMessage(message, 'error');
  }
};

// 删除好友
const removeFriend = async (friend) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除好友 ${friend.remark || friend.username} 吗？`,
      '删除好友',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.delete(`/api/Friends/${friend.id}`);
    
    showMessage('好友删除成功');
    await loadFriends(); // 重新加载好友列表
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '删除好友失败';
    showMessage(message, 'error');
  }
};

// 拉黑好友
const blockFriend = async (friend) => {
  try {
    await ElMessageBox.confirm(
      `确定要将好友 ${friend.remark || friend.username} 加入黑名单吗？`,
      '拉黑好友',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.post('/api/Friends/block', {
      blockID: friend.id
    });
    
    showMessage('好友已加入黑名单');
    await loadFriends(); // 重新加载好友列表
    await loadBlockedUsers(); // 重新加载黑名单
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '拉黑好友失败';
    showMessage(message, 'error');
  }
};

// 从黑名单中移出用户
const unblockUserFromList = async (user) => {
  try {
    await ElMessageBox.confirm(
      `确定要将 ${user.username} 移出黑名单吗？`,
      '移出黑名单',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    );

    const response = await request.delete(`/api/Friends/blocked/${user.id}`);
    
    showMessage('已取消拉黑');
    await loadBlockedUsers(); // 重新加载黑名单
  } catch (error) {
    if (error === 'cancel') return;
    const message = error.response?.data?.message || '取消拉黑失败';
    showMessage(message, 'error');
  }
};

// 发送消息
const sendMessage = (friend) => {
  // 这里可以跳转到聊天页面或打开聊天窗口
  ElMessage.info(`开始与 ${friend.remark || friend.username} 聊天`);
};

// 加载数据方法
const loadFriends = async () => {
  try {
    const response = await request.get('/api/Friends');
    friends.value = response.data || [];
  } catch (error) {
    console.error('加载好友列表失败:', error);
    friends.value = [];
    showMessage('加载好友列表失败', 'error');
  }
};

const loadBlockedUsers = async () => {
  try {
    const response = await request.get('/api/Friends/blocked');
    blockedUsers.value = response.data || [];
  } catch (error) {
    console.error('加载黑名单失败:', error);
    blockedUsers.value = [];
    showMessage('加载黑名单失败', 'error');
  }
};

const loadReceivedRequests = async () => {
  try {
    const response = await request.get('/api/Friends/requests/received');
    receivedRequests.value = response.data || [];
  } catch (error) {
    console.error('加载收到的请求失败:', error);
    receivedRequests.value = [];
    showMessage('加载收到的请求失败', 'error');
  }
};

const loadSentRequests = async () => {
  try {
    const response = await request.get('/api/Friends/requests/sent');
    sentRequests.value = response.data || [];
  } catch (error) {
    console.error('加载发送的请求失败:', error);
    sentRequests.value = [];
    showMessage('加载发送的请求失败', 'error');
  }
};

// 加载所有数据
const loadAllData = async () => {
  try {
    await Promise.all([
      loadFriends(),
      loadBlockedUsers(),
      loadReceivedRequests(),
      loadSentRequests()
    ]);
  } catch (error) {
    console.error('加载数据失败:', error);
  }
};

// 监听标签切换
const handleTabChange = () => {
  clearMessages();
};

// 生命周期
onMounted(async () => {
  await loadAllData();
});
</script>

<style scoped>
.friend-manager {
    max-width: 1000px;
    margin: 0 auto;
    background-color: #fff;
    border-radius: 12px;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.08);
    overflow: hidden;
}

.header {
    background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
    color: white;
    padding: 25px 30px;
    text-align: center;
}

.header h1 {
    font-size: 28px;
    margin-bottom: 5px;
}

.header p {
    opacity: 0.9;
    font-size: 16px;
}

.tabs {
    display: flex;
    background-color: #f0f2f5;
    border-bottom: 1px solid #e0e5eb;
}

.tab {
    flex: 1;
    text-align: center;
    padding: 15px;
    cursor: pointer;
    font-weight: 600;
    transition: all 0.3s;
    border-bottom: 3px solid transparent;
}

.tab.active {
    border-bottom: 3px solid #2575fc;
    color: #2575fc;
    background-color: white;
}

.tab:hover {
    background-color: #e8f0fe;
}

.tab-content {
    padding: 25px;
    min-height: 400px;
}

.search-container {
    margin-bottom: 25px;
}

.search-box {
    display: flex;
    margin-bottom: 15px;
}

.search-box input {
    flex: 1;
    padding: 12px 15px;
    border: 1px solid #ddd;
    border-radius: 8px 0 0 8px;
    font-size: 16px;
    outline: none;
    transition: border 0.3s;
}

.search-box input:focus {
    border-color: #2575fc;
}

.search-box button {
    padding: 0 20px;
    background-color: #2575fc;
    color: white;
    border: none;
    border-radius: 0 8px 8px 0;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.3s;
}

.search-box button:hover {
    background-color: #1a68e8;
}

.search-history {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 15px;
}

.history-tag {
    background-color: #f0f2f5;
    padding: 5px 12px;
    border-radius: 20px;
    font-size: 14px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.history-tag:hover {
    background-color: #e0e5eb;
}

.search-result {
    margin-top: 20px;
    animation: fadeIn 0.5s;
}

.user-card {
    display: flex;
    align-items: center;
    padding: 15px;
    background-color: #f9fafb;
    border-radius: 10px;
    margin-bottom: 15px;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.avatar {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 24px;
    font-weight: bold;
    margin-right: 15px;
}

.user-info {
    flex: 1;
}

.user-info h3 {
    margin-bottom: 5px;
    color: #333;
}

.user-info p {
    color: #666;
    font-size: 14px;
}

.action-btn {
    padding: 8px 15px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: all 0.3s;
    font-size: 14px;
    margin-left: 10px;
    white-space: nowrap;
}

.add-btn {
    background-color: #4CAF50;
    color: white;
}

.add-btn:hover:not(:disabled) {
    background-color: #3d8b40;
}

.add-btn:disabled {
    background-color: #a5d6a7;
    cursor: not-allowed;
}

.pending-btn {
    background-color: #ff9800;
    color: white;
    cursor: not-allowed;
}

.friend-btn {
    background-color: #2196F3;
    color: white;
    cursor: not-allowed;
}

.block-btn {
    background-color: #f44336;
    color: white;
}

.block-btn:hover {
    background-color: #d32f2f;
}

.unblock-btn {
    background-color: #4CAF50;
    color: white;
}

.unblock-btn:hover {
    background-color: #3d8b40;
}

.message {
    padding: 12px;
    border-radius: 6px;
    margin: 15px 0;
    text-align: center;
    font-weight: 500;
}

.error-message {
    background-color: #ffebee;
    color: #c62828;
    border-left: 4px solid #f44336;
}

.success-message {
    background-color: #e8f5e9;
    color: #2e7d32;
    border-left: 4px solid #4caf50;
}

.list-controls {
    display: flex;
    justify-content: space-between;
    margin-bottom: 20px;
    gap: 15px;
}

.list-search {
    flex: 1;
    max-width: 300px;
}

.list-search input {
    width: 100%;
    padding: 10px 15px;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 14px;
}

.sort-options select {
    padding: 10px 15px;
    border: 1px solid #ddd;
    border-radius: 8px;
    background-color: white;
    font-size: 14px;
}

.friends-list, .blocked-list, .requests-list {
    max-height: 400px;
    overflow-y: auto;
}

.friend-item, .blocked-item, .request-item {
    display: flex;
    align-items: center;
    padding: 12px 15px;
    border-bottom: 1px solid #f0f0f0;
    transition: background-color 0.3s;
}

.friend-item:hover, .blocked-item:hover, .request-item:hover {
    background-color: #f9fafb;
}

.friend-avatar, .blocked-avatar, .request-avatar {
    width: 45px;
    height: 45px;
    border-radius: 50%;
    background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 18px;
    font-weight: bold;
    margin-right: 15px;
}

.friend-details, .blocked-details, .request-details {
    flex: 1;
}

.friend-details h4, .blocked-details h4, .request-details h4 {
    margin-bottom: 3px;
    color: #333;
}

.friend-details p, .blocked-details p, .request-details p {
    color: #666;
    font-size: 13px;
}

.request-remark {
    font-style: italic;
    color: #888;
    margin-top: 5px;
}

.request-status {
    font-weight: 600;
    margin-top: 5px;
}

.status-pending {
    color: #ff9800;
}

.status-accepted {
    color: #4CAF50;
}

.status-rejected {
    color: #f44336;
}

.friend-actions, .blocked-actions, .request-actions {
    display: flex;
    gap: 10px;
}

.msg-btn {
    background-color: #2196F3;
    color: white;
}

.msg-btn:hover {
    background-color: #0b7dda;
}

.remove-btn {
    background-color: #ff9800;
    color: white;
}

.remove-btn:hover {
    background-color: #e68900;
}

.accept-btn {
    background-color: #4CAF50;
    color: white;
}

.accept-btn:hover {
    background-color: #3d8b40;
}

.reject-btn {
    background-color: #f44336;
    color: white;
}

.reject-btn:hover {
    background-color: #d32f2f;
}

.cancel-btn {
    background-color: #ff9800;
    color: white;
}

.cancel-btn:hover {
    background-color: #e68900;
}

.empty-state {
    text-align: center;
    padding: 40px 20px;
    color: #666;
}

.empty-state i {
    font-size: 50px;
    margin-bottom: 15px;
    color: #ccc;
}

.requests-section {
    margin-bottom: 30px;
}

.requests-section h3 {
    margin-bottom: 15px;
    color: #333;
    border-bottom: 2px solid #f0f0f0;
    padding-bottom: 10px;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

/* 响应式设计 */
@media (max-width: 768px) {
    .tabs {
        flex-direction: column;
    }
    
    .list-controls {
        flex-direction: column;
        gap: 15px;
    }
    
    .list-search {
        max-width: 100%;
    }
    
    .friend-item, .blocked-item, .request-item {
        flex-direction: column;
        align-items: flex-start;
        padding: 15px;
    }
    
    .friend-avatar, .blocked-avatar, .request-avatar {
        margin-bottom: 10px;
    }
    
    .friend-actions, .blocked-actions, .request-actions {
        margin-top: 10px;
        width: 100%;
        justify-content: flex-end;
    }
    
    .action-btn {
        margin-left: 0;
        margin-top: 5px;
        flex: 1;
    }
    
    .user-card {
        flex-direction: column;
        text-align: center;
    }
    
    .avatar {
        margin-right: 0;
        margin-bottom: 10px;
    }
    
    .user-info {
        margin-bottom: 15px;
    }
}

@media (max-width: 480px) {
    .tab-content {
        padding: 15px;
    }
    
    .header {
        padding: 20px 15px;
    }
    
    .header h1 {
        font-size: 24px;
    }
    
    .search-box {
        flex-direction: column;
    }
    
    .search-box input {
        border-radius: 8px;
        margin-bottom: 10px;
    }
    
    .search-box button {
        border-radius: 8px;
        padding: 12px;
    }
    
    .friend-actions, .blocked-actions, .request-actions {
        flex-direction: column;
    }
}
</style>