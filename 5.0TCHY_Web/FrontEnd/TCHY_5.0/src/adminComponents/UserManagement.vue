<template>
  <section class="panel">
    <header class="panel-head">
      <div class="panel-title">
        <h2>用户管理</h2>
        <span class="tag">用户数: {{ userCount }}</span>
      </div>
      <div class="panel-controls">
        <div class="search-box">
          <input 
            v-model="searchKeyword" 
            placeholder="搜索用户名、邮箱、头衔..." 
            class="search-input"
            @input="handleSearch"
          />
          <i class="search-icon">🔍</i>
        </div>
        <button class="refresh-btn" @click="refresh" :disabled="loading">
          {{ loading ? '刷新中...' : '刷新' }}
        </button>
      </div>
    </header>
    
    <!-- 搜索和排序控制栏 -->
    <div v-if="userDataList.length > 0" class="controls-bar">
      <div class="sort-controls">
        <span class="sort-label">排序:</span>
        <select v-model="sortField" @change="handleSort" class="sort-select">
          <option value="id">ID</option>
          <option value="username">用户名</option>
          <option value="email_address">邮箱</option>
          <option value="title">头衔</option>
          <option value="rank">Rank</option>
          <option value="state">状态</option>
          <option value="level">等级</option>
          <option value="points">积分</option>
          <option value="date">注册时间</option>
          <option value="lastlogin">最后登录</option>
        </select>
        <button class="sort-direction" @click="toggleSortDirection">
          {{ sortDirection === 'asc' ? '↑' : '↓' }}
        </button>
      </div>
      <div class="results-info">
        显示 {{ startIndex + 1 }}-{{ endIndex }} 条，共 {{ filteredUsers.length }} 条
        <span v-if="searchKeyword" class="search-info">(搜索: "{{ searchKeyword }}")</span>
      </div>
    </div>
    
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      正在加载用户数据...
    </div>
    
    <!-- 错误状态 -->
    <div v-else-if="error" class="error-state">
      <p>加载失败: {{ error }}</p>
      <button class="retry-btn" @click="refresh">重试</button>
    </div>
    
    <!-- 权限错误 -->
    <div v-else-if="permissionError" class="permission-error">
      <span>⚠️</span>
      <span>{{ permissionError }}</span>
    </div>
    
    <!-- 空状态 -->
    <div v-else-if="filteredUsers.length === 0" class="empty-state">
      <span v-if="searchKeyword">未找到匹配的用户</span>
      <span v-else>暂无用户数据</span>
    </div>
    
    <!-- 用户数据表格 -->
    <div v-else class="user-table-container">
      <table class="user-table">
        <thead>
          <tr>
            <th @click="sortBy('id')" class="sortable-header">
              ID
              <span v-if="sortField === 'id'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('username')" class="sortable-header">
              用户信息
              <span v-if="sortField === 'username'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('email_address')" class="sortable-header">
              邮箱
              <span v-if="sortField === 'email_address'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('title')" class="sortable-header">
              头衔
              <span v-if="sortField === 'title'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('rank')" class="sortable-header">
              Rank
              <span v-if="sortField === 'rank'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('state')" class="sortable-header">
              账号状态
              <span v-if="sortField === 'state'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('level')" class="sortable-header">
              等级
              <span v-if="sortField === 'level'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('points')" class="sortable-header">
              积分
              <span v-if="sortField === 'points'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('date')" class="sortable-header">
              注册时间
              <span v-if="sortField === 'date'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('lastlogin')" class="sortable-header">
              最后登录
              <span v-if="sortField === 'lastlogin'" class="sort-indicator">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in paginatedUsers" :key="user.id" class="user-row">
            <td>{{ user.id }}</td>
            <td>
              <div class="user-info">
                <div class="user-avatar">
                  <span class="avatar-placeholder">
                    {{ user.username.charAt(0).toUpperCase() }}
                  </span>
                </div>
                <div class="user-details">
                  <div class="username-row">
                    <span class="username">{{ user.username }}</span>
                    <span v-if="user.creater === 1" class="creator-badge">创作者</span>
                  </div>
                  <div v-if="user.title" class="user-title">
                    {{ user.title }}
                  </div>
                </div>
              </div>
            </td>
            <td>{{ user.email_address }}</td>
            <td>
              <span v-if="user.title" class="title-badge">
                {{ user.title }}
              </span>
              <span v-else class="no-title">-</span>
            </td>
            <td>
              <span class="rank-badge" :class="getRankClass(user.rank)">
                {{ getRankText(user.rank) }}
              </span>
            </td>
            <td>
              <span class="status-badge" :class="getStatusClass(user.state)">
                {{ getStatusText(user.state) }}
              </span>
            </td>
            <td>
              <span class="level-badge">Lv.{{ user.level }}</span>
            </td>
            <td>{{ user.points }}</td>
            <td>{{ formatDate(user.date) }}</td>
            <td>{{ formatDate(user.lastlogin) }}</td>
            <td>
              <div class="actions">
                <button class="btn btn-sm btn-primary" @click="viewUser(user.id)">查看</button>
                <button class="btn btn-sm btn-warning" @click="editUser(user)">编辑</button>
                <button 
                  v-if="user.state === 1" 
                  class="btn btn-sm btn-danger" 
                  @click="banUser(user.id)"
                  :disabled="user.rank >= currentUserRank"
                >
                  封禁
                </button>
                <button 
                  v-else 
                  class="btn btn-sm btn-success" 
                  @click="unbanUser(user.id)"
                  :disabled="user.rank >= currentUserRank"
                >
                  解封
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    
    <!-- 分页控件 -->
    <div v-if="filteredUsers.length > 0" class="pagination">
      <div class="pagination-info">
        显示 {{ startIndex + 1 }}-{{ endIndex }} 条，共 {{ filteredUsers.length }} 条
      </div>
      <div class="pagination-controls">
        <button class="btn btn-prev" :disabled="currentPage === 1" @click="prevPage">上一页</button>
        <span class="page-info">第 {{ currentPage }} 页 / 共 {{ totalPages }} 页</span>
        <button class="btn btn-next" :disabled="currentPage >= totalPages" @click="nextPage">下一页</button>
      </div>
    </div>
    
    <!-- 编辑用户模态框 -->
    <div v-if="showEditModal" class="modal-overlay" @click.self="closeEditModal">
      <div class="edit-modal">
        <div class="modal-header">
          <h3>编辑用户信息</h3>
          <button class="close-btn" @click="closeEditModal">×</button>
        </div>
        
        <div class="modal-body">
          <div v-if="editingUser" class="user-edit-form">
            <!-- 用户基本信息 -->
            <div class="form-section">
              <h4>基本信息</h4>
              <div class="form-group">
                <label>用户ID</label>
                <input type="text" :value="editingUser.id" disabled class="form-input" />
              </div>
              <div class="form-group">
                <label>用户名</label>
                <input type="text" :value="editingUser.username" disabled class="form-input" />
              </div>
              <div class="form-group">
                <label>邮箱</label>
                <input type="text" :value="editingUser.email_address" disabled class="form-input" />
              </div>
            </div>
            
            <!-- 权限和状态 -->
            <div class="form-section">
              <h4>权限和状态</h4>
              <div class="form-group">
                <label>Rank等级</label>
                <select v-model="editForm.rank" class="form-select">
                  <option value="0">用户</option>
                  <option value="1">管理员</option>
                  <option value="2">超级管理员</option>
                </select>
              </div>
              <div class="form-group">
                <label>账号状态</label>
                <select v-model="editForm.state" class="form-select">
                  <option value="1">正常</option>
                  <option value="0">封禁</option>
                  <option value="2">离线</option>
                </select>
              </div>
            </div>
            
            <!-- 用户数据 -->
            <div class="form-section">
              <h4>用户数据</h4>
              <div class="form-group">
                <label>等级</label>
                <input 
                  type="number" 
                  v-model.number="editForm.level" 
                  min="0" 
                  max="999"
                  class="form-input" 
                />
              </div>
              <div class="form-group">
                <label>积分</label>
                <input 
                  type="number" 
                  v-model.number="editForm.points" 
                  min="0"
                  class="form-input" 
                />
              </div>
              <div class="form-group">
                <label>经验值</label>
                <input 
                  type="number" 
                  v-model.number="editForm.exp" 
                  min="0"
                  class="form-input" 
                />
              </div>
              <div class="form-group">
                <label>头衔</label>
                <input 
                  type="text" 
                  v-model="editForm.title" 
                  placeholder="输入用户头衔"
                  maxlength="50"
                  class="form-input" 
                />
              </div>
              <div class="form-group">
                <label>创作者标识</label>
                <select v-model.number="editForm.creater" class="form-select">
                  <option :value="0">否</option>
                  <option :value="1">是</option>
                </select>
              </div>
            </div>
            
            <!-- 统计信息 -->
            <div class="form-section">
              <h4>统计信息</h4>
              <div class="stats-grid">
                <div class="stat-item">
                  <label>点赞数</label>
                  <input 
                    type="number" 
                    v-model.number="editForm.likes" 
                    min="0"
                    class="form-input" 
                  />
                </div>
                <div class="stat-item">
                  <label>帖子数</label>
                  <input 
                    type="number" 
                    :value="editingUser.post_count" 
                    disabled
                    class="form-input" 
                  />
                </div>
                <div class="stat-item">
                  <label>评论数</label>
                  <input 
                    type="number" 
                    :value="editingUser.comment_count" 
                    disabled
                    class="form-input" 
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="modal-footer">
          <div class="modal-actions">
            <button class="btn btn-secondary" @click="closeEditModal" :disabled="saving">
              取消
            </button>
            <button class="btn btn-primary" @click="saveUser" :disabled="saving">
              {{ saving ? '保存中...' : '保存更改' }}
            </button>
          </div>
        </div>
        
        <!-- 保存状态 -->
        <div v-if="saveError" class="save-error">
          {{ saveError }}
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted, computed, reactive } from 'vue'
import apiClient from '../utils/api'

// 响应式数据
const userDataList = ref([])
const loading = ref(false)
const error = ref('')
const permissionError = ref('')
const searchKeyword = ref('')
const sortField = ref('id')
const sortDirection = ref('desc')
const currentPage = ref(1)
const pageSize = 10
const currentUserRank = ref(0)

// 编辑相关数据
const showEditModal = ref(false)
const editingUser = ref(null)
const saving = ref(false)
const saveError = ref('')

// 编辑表单
const editForm = reactive({
  rank: 0,
  state: 1,
  level: 0,
  points: 0,
  exp: 0,
  title: '',
  creater: 0,
  likes: 0
})

// 计算属性
const userCount = computed(() => userDataList.value.length)

// 过滤和排序用户数据
const filteredUsers = computed(() => {
  let users = userDataList.value
  
  // 搜索过滤
  if (searchKeyword.value.trim()) {
    const keyword = searchKeyword.value.toLowerCase()
    users = users.filter(user => 
      user.username.toLowerCase().includes(keyword) ||
      user.email_address.toLowerCase().includes(keyword) ||
      (user.title && user.title.toLowerCase().includes(keyword))
    )
  }
  
  // 排序
  users.sort((a, b) => {
    let aValue = a[sortField.value]
    let bValue = b[sortField.value]
    
    if (aValue === null || aValue === undefined) aValue = ''
    if (bValue === null || bValue === undefined) bValue = ''
    
    if (sortField.value === 'date' || sortField.value === 'lastlogin') {
      aValue = new Date(aValue).getTime()
      bValue = new Date(bValue).getTime()
    }
    
    if (sortDirection.value === 'asc') {
      return aValue < bValue ? -1 : aValue > bValue ? 1 : 0
    } else {
      return aValue > bValue ? -1 : aValue < bValue ? 1 : 0
    }
  })
  
  return users
})

// 分页相关计算
const totalPages = computed(() => Math.ceil(filteredUsers.value.length / pageSize))
const startIndex = computed(() => (currentPage.value - 1) * pageSize)
const endIndex = computed(() => Math.min(startIndex.value + pageSize, filteredUsers.value.length))
const paginatedUsers = computed(() => 
  filteredUsers.value.slice(startIndex.value, endIndex.value)
)

// 获取用户数据
const fetchUserData = async () => {
  loading.value = true
  error.value = ''
  permissionError.value = ''
  
  try {
    const currentUserResponse = await apiClient.get('/userinfo/me')
    currentUserRank.value = currentUserResponse.data.rank
    
    const response = await apiClient.get('/userinfo/all')
    
    if (response.data && Array.isArray(response.data)) {
      userDataList.value = response.data
    } else {
      userDataList.value = []
      error.value = '未获取到用户数据'
    }
  } catch (err) {
    console.error('获取用户数据失败:', err)
    if (err.response?.status === 403) {
      permissionError.value = '需要管理员权限才能查看所有用户数据'
    } else {
      error.value = err.response?.data?.message || err.message || '网络请求失败'
    }
    userDataList.value = []
  } finally {
    loading.value = false
  }
}

// 搜索处理
const handleSearch = () => {
  currentPage.value = 1
}

// 排序处理
const sortBy = (field) => {
  if (sortField.value === field) {
    toggleSortDirection()
  } else {
    sortField.value = field
    sortDirection.value = 'desc'
  }
  currentPage.value = 1
}

const toggleSortDirection = () => {
  sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  currentPage.value = 1
}

const handleSort = () => {
  currentPage.value = 1
}

// 刷新数据
const refresh = () => {
  currentPage.value = 1
  searchKeyword.value = ''
  sortField.value = 'id'
  sortDirection.value = 'desc'
  fetchUserData()
}

// 分页方法
const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

// 编辑用户功能
const editUser = (user) => {
  if (user.rank >= currentUserRank.value) {
    alert('您不能编辑权限等级高于或等于您的用户')
    return
  }
  
  editingUser.value = user
  // 填充表单数据
  editForm.rank = user.rank
  editForm.state = user.state
  editForm.level = user.level || 0
  editForm.points = user.points || 0
  editForm.exp = user.exp || 0
  editForm.title = user.title || ''
  editForm.creater = user.creater || 0
  editForm.likes = user.likes || 0
  
  showEditModal.value = true
  saveError.value = ''
}

const closeEditModal = () => {
  if (!saving.value) {
    showEditModal.value = false
    editingUser.value = null
    saveError.value = ''
  }
}

const saveUser = async () => {
  if (!editingUser.value) return
  
  saving.value = true
  saveError.value = ''
  
  try {
    // 构建更新数据
    const updateData = {
      rank: parseInt(editForm.rank),
      state: parseInt(editForm.state),
      level: parseInt(editForm.level),
      points: parseInt(editForm.points),
      exp: parseInt(editForm.exp),
      title: editForm.title.trim(),
      creater: parseInt(editForm.creater),
      likes: parseInt(editForm.likes)
    }
    
    // 调用API更新用户信息
    const response = await apiClient.put(`/userinfo/${editingUser.value.id}`, updateData)
    
    if (response.data.success) {
      // 更新本地数据
      const userIndex = userDataList.value.findIndex(u => u.id === editingUser.value.id)
      if (userIndex !== -1) {
        // 更新用户账户信息
        userDataList.value[userIndex] = {
          ...userDataList.value[userIndex],
          rank: updateData.rank,
          state: updateData.state
        }
        
        // 更新用户数据信息
        if (userDataList.value[userIndex].userdata) {
          userDataList.value[userIndex].userdata = {
            ...userDataList.value[userIndex].userdata,
            level: updateData.level,
            points: updateData.points,
            exp: updateData.exp,
            title: updateData.title,
            creater: updateData.creater,
            likes: updateData.likes
          }
        }
      }
      
      alert('用户信息更新成功！')
      closeEditModal()
    } else {
      saveError.value = response.data.message || '更新失败'
    }
  } catch (err) {
    console.error('更新用户信息失败:', err)
    saveError.value = err.response?.data?.message || err.message || '网络请求失败'
  } finally {
    saving.value = false
  }
}

// 工具函数
const getRankClass = (rank) => {
  const classes = {
    0: 'rank-user',
    1: 'rank-admin',
    2: 'rank-superadmin'
  }
  return classes[rank] || 'rank-user'
}

const getRankText = (rank) => {
  const texts = {
    0: '用户',
    1: '管理员',
    2: '超级管理员'
  }
  return texts[rank] || '未知'
}

const getStatusClass = (state) => {
  const classes = {
    0: 'status-banned',
    1: 'status-normal',
    2: 'status-offline'
  }
  return classes[state] || 'status-normal'
}

const getStatusText = (state) => {
  const texts = {
    0: '已封禁',
    1: '正常',
    2: '离线'
  }
  return texts[state] || '正常'
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN') + ' ' + date.toLocaleTimeString('zh-CN', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

// 操作函数
const viewUser = (userId) => {
  console.log('查看用户:', userId)
}

const banUser = (userId) => {
  console.log('封禁用户:', userId)
  if (confirm('确定要封禁此用户吗？')) {
    // TODO: 调用封禁API
  }
}

const unbanUser = (userId) => {
  console.log('解封用户:', userId)
  if (confirm('确定要解封此用户吗？')) {
    // TODO: 调用解封API
  }
}

// 生命周期
onMounted(() => {
  fetchUserData()
})
</script>

<style scoped>
.panel { 
  background: #fff; 
  border: 1px solid #e6ebf3; 
  border-radius: 16px; 
  padding: 20px; 
  box-shadow: 0 2px 10px rgba(2, 6, 23, 0.06); 
}

.panel-head { 
  display: grid; 
  grid-template-columns: 1fr auto; 
  align-items: center; 
  padding-bottom: 15px; 
  border-bottom: 1px solid #e6ebf3; 
  margin-bottom: 15px; 
}

.panel-title { 
  display: flex; 
  gap: 10px; 
  align-items: center; 
}

.panel-title h2 { 
  margin: 0; 
  font-size: 18px; 
  font-weight: 900; 
  color: #1e293b;
}

.tag { 
  padding: 4px 10px; 
  font-size: 12px; 
  font-weight: 600; 
  background: #eef2ff; 
  color: #4f46e5;
  border: 1px solid #dbe5ff; 
  border-radius: 999px; 
}

.panel-controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input {
  padding: 6px 12px 6px 32px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 14px;
  width: 200px;
  transition: all 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
}

.search-icon {
  position: absolute;
  left: 10px;
  color: #64748b;
  font-size: 14px;
}

.refresh-btn {
  padding: 6px 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  color: #475569;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
}

.refresh-btn:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 控制栏 */
.controls-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding: 10px 0;
  border-bottom: 1px solid #f1f5f9;
}

.sort-controls {
  display: flex;
  align-items: center;
  gap: 8px;
}

.sort-label {
  font-size: 14px;
  color: #64748b;
}

.sort-select {
  padding: 4px 8px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  background: white;
  font-size: 14px;
}

.sort-direction {
  padding: 4px 8px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  background: #f8fafc;
  cursor: pointer;
  font-size: 14px;
  min-width: 30px;
}

.sort-direction:hover {
  background: #f1f5f9;
}

.results-info {
  font-size: 14px;
  color: #64748b;
}

.search-info {
  color: #3b82f6;
  font-weight: 500;
}

/* 表格排序头部 */
.sortable-header {
  cursor: pointer;
  user-select: none;
  position: relative;
  padding-right: 20px !important;
}

.sortable-header:hover {
  background: #f1f5f9;
}

.sort-indicator {
  position: absolute;
  right: 8px;
  color: #3b82f6;
  font-weight: bold;
}

/* 加载状态 */
.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #64748b;
  gap: 10px;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #e2e8f0;
  border-left: 2px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 错误状态 */
.error-state, .permission-error {
  text-align: center;
  padding: 40px;
  color: #dc2626;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.permission-error {
  color: #d97706;
}

.retry-btn {
  margin-top: 10px;
  padding: 8px 16px;
  background: #dc2626;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 40px;
  color: #64748b;
}

/* 用户表格 */
.user-table-container {
  overflow-x: auto;
}

.user-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.user-table th {
  background: #f8fafc;
  padding: 12px;
  text-align: left;
  font-weight: 600;
  color: #475569;
  border-bottom: 1px solid #e2e8f0;
}

.user-table td {
  padding: 12px;
  border-bottom: 1px solid #f1f5f9;
}

.user-row:hover {
  background: #f8fafc;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-weight: 600;
  font-size: 14px;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.username-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.username {
  font-weight: 500;
  color: #1e293b;
}

.user-title {
  font-size: 12px;
  color: #64748b;
  font-style: italic;
}

.creator-badge {
  padding: 1px 4px;
  font-size: 10px;
  background: #f0f9ff;
  color: #0c4a6e;
  border: 1px solid #bae6fd;
  border-radius: 3px;
}

/* 头衔徽章样式 */
.title-badge {
  padding: 4px 8px;
  background: #f8fafc;
  color: #475569;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: inline-block;
}

.no-title {
  color: #94a3b8;
  font-style: italic;
  font-size: 12px;
}

/* 徽章样式 */
.rank-badge, .status-badge, .level-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.rank-user { background: #dbeafe; color: #1e40af; }
.rank-admin { background: #fef2f2; color: #dc2626; }
.rank-superadmin { background: #7c2d12; color: #fed7aa; }

.status-normal { background: #f0fdf4; color: #166534; }
.status-banned { background: #fef2f2; color: #dc2626; }
.status-offline { background: #f3f4f6; color: #374151; }

.level-badge {
  background: #f8fafc;
  color: #475569;
  border: 1px solid #e2e8f0;
}

/* 操作按钮 */
.actions {
  display: flex;
  gap: 6px;
}

.btn {
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
  min-width: 50px;
}

.btn-sm {
  padding: 4px 8px;
  font-size: 11px;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-warning {
  background: #f59e0b;
  color: white;
}

.btn-warning:hover:not(:disabled) {
  background: #d97706;
}

.btn-danger {
  background: #dc2626;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
}

.btn-success {
  background: #16a34a;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #15803d;
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  padding-top: 15px;
  border-top: 1px solid #e2e8f0;
}

.pagination-info {
  font-size: 14px;
  color: #64748b;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 15px;
}

.btn-prev, .btn-next {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  color: #475569;
  padding: 6px 12px;
}

.btn-prev:hover:not(:disabled), .btn-next:hover:not(:disabled) {
  background: #f1f5f9;
}

.page-info {
  font-size: 14px;
  color: #64748b;
}

/* 编辑模态框样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
}

.edit-modal {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 80vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
  color: #1e293b;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #64748b;
  padding: 5px;
}

.close-btn:hover {
  color: #374151;
}

.modal-body {
  padding: 20px;
}

.modal-footer {
  padding: 20px;
  border-top: 1px solid #e2e8f0;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

/* 表单样式 */
.form-section {
  margin-bottom: 24px;
}

.form-section h4 {
  margin: 0 0 16px 0;
  font-size: 16px;
  color: #374151;
  padding-bottom: 8px;
  border-bottom: 1px solid #f1f5f9;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  color: #374151;
  font-size: 14px;
}

.form-input, .form-select {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.2s;
}

.form-input:focus, .form-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input:disabled {
  background: #f9fafb;
  color: #6b7280;
  cursor: not-allowed;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
}

.stat-item {
  margin-bottom: 0;
}

.save-error {
  margin-top: 12px;
  padding: 8px 12px;
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  border-radius: 4px;
  font-size: 14px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .panel-controls {
    flex-direction: column;
    gap: 10px;
    align-items: stretch;
  }
  
  .search-input {
    width: 100%;
  }
  
  .controls-bar {
    flex-direction: column;
    gap: 10px;
  }
  
  .user-table-container {
    font-size: 12px;
  }
  
  .user-table th, .user-table td {
    padding: 8px;
  }
  
  .actions {
    flex-direction: column;
  }
  
  .pagination {
    flex-direction: column;
    gap: 10px;
  }
  
  .edit-modal {
    width: 95%;
    margin: 10px;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .modal-actions {
    flex-direction: column;
  }
}
</style>