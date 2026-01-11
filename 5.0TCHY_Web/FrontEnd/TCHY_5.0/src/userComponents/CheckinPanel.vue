<template>
  <div class="checkin-container">
    <!-- 顶部信息栏 -->
    <div class="header">
      <h2>📅 签到中心</h2>
      <div class="user-info">
        <span class="username">{{ displayUser?.username || '加载中...' }}</span>
        <span v-if="isCurrentUser" class="tag-me">我的签到</span>
        <span v-else class="tag-other">查看用户签到</span>
        <span v-if="!isCurrentUser && isAdmin" class="admin-badge">👑 管理员视图</span>
      </div>
    </div>

    <!-- 用户基本信息 -->
    <div v-if="displayUser" class="user-basic-info">
      <div class="info-card">
        <div class="info-item">
          <span class="info-label">用户ID:</span>
          <span class="info-value">{{ displayUser.id }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">权限等级:</span>
          <span class="info-value rank-badge" :class="`rank-${displayUser.rank}`">
            {{ getRankText(displayUser.rank) }}
          </span>
        </div>
        <div class="info-item">
          <span class="info-label">总积分:</span>
          <span class="info-value points">{{ displayUser.points || 0 }}</span>
        </div>
      </div>
    </div>

    <!-- 统计概览 -->
    <div class="stats-section">
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalCount || 0 }}</div>
        <div class="stat-label">累计签到</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.continuousDays || 0 }} 天</div>
        <div class="stat-label">连续签到</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ currentMonthCheckins }}/{{ currentMonthDays }}</div>
        <div class="stat-label">本月签到</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ displayUser?.points || 0 }}</div>
        <div class="stat-label">签到积分</div>
      </div>
    </div>

    <!-- 签到按钮（仅当前用户可见） -->
    <div class="checkin-action" v-if="isCurrentUser && !todayChecked">
      <button 
        class="checkin-btn" 
        :disabled="isChecking"
        @click="handleCheckin"
      >
        <span v-if="!isChecking">🎯 立即签到</span>
        <span v-else>签到中...</span>
      </button>
      <p class="reward-hint">签到奖励: +10 积分</p>
    </div>

    <!-- 今日已签到提示 -->
    <div class="checked-today" v-if="isCurrentUser && todayChecked">
      <div class="checked-badge">
        <span>✅</span>
        <span>今日已签到</span>
      </div>
      <p class="checkin-time">签到时间: {{ formatTime(todayCheckinTime) }}</p>
    </div>

    <!-- 如果不是当前用户，显示提示信息 -->
    <div class="view-only-notice" v-if="!isCurrentUser">
      <div class="notice-content">
        <span>👀 查看模式</span>
        <p>您正在查看其他用户的签到记录，无法进行签到操作</p>
      </div>
    </div>

    <!-- 日期选择器 -->
    <div class="date-selector">
      <button class="nav-btn" @click="prevMonth">‹</button>
      <span class="current-date">{{ currentYear }}年 {{ currentMonth }}月</span>
      <button class="nav-btn" @click="nextMonth">›</button>
    </div>

    <!-- 日历视图 -->
    <div class="calendar">
      <div class="week-header">
        <div v-for="day in weekDays" :key="day" class="week-day">{{ day }}</div>
      </div>
      <div class="calendar-grid">
        <div 
          v-for="(date, index) in calendarDates" 
          :key="index"
          class="calendar-cell"
          :class="getCellClass(date)"
        >
          <div class="date-number">{{ date.getDate() }}</div>
          <div v-if="isCheckinDay(date)" class="checkin-indicator">
            <span v-if="isToday(date)">🎯</span>
            <span v-else>✅</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 签到记录 -->
    <div class="records-section">
      <h3>📊 签到记录 - {{ displayUser?.username || '用户' }}</h3>
      <div v-if="loading" class="loading">加载中...</div>
      <div v-else-if="checkinRecords.length === 0" class="no-data">
        暂无签到记录
      </div>
      <div v-else class="records-list">
        <div 
          v-for="record in paginatedRecords" 
          :key="record.id"
          class="record-item"
        >
          <div class="record-date">{{ formatDate(record.checkinDate) }}</div>
          <div class="record-time">{{ formatTime(record.checkinDate) }}</div>
          <div class="record-type">{{ getCheckinType(record.type) }}</div>
        </div>
      </div>
      
      <!-- 分页控件 -->
      <div v-if="checkinRecords.length > pageSize" class="pagination">
        <button @click="prevPage" :disabled="currentPage === 1" class="page-btn">
          上一页
        </button>
        <span class="page-info">第 {{ currentPage }} 页 / 共 {{ totalPages }} 页</span>
        <button @click="nextPage" :disabled="currentPage === totalPages" class="page-btn">
          下一页
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'

const route = useRoute()
const router = useRouter()

// 响应式数据
const currentYear = ref(new Date().getFullYear())
const currentMonth = ref(new Date().getMonth() + 1)
const isChecking = ref(false)
const todayChecked = ref(false)
const todayCheckinTime = ref('')
const loading = ref(false)
const currentPage = ref(1)
const pageSize = 10
const isAdmin = ref(false)

// 初始化 displayUser 对象
const displayUser = ref({
  id: 0,
  username: '加载中...',
  points: 0,
  rank: 0
})

// 初始化 stats 对象
const stats = ref({
  totalCount: 0,
  continuousDays: 0
})

const checkinRecords = ref([])
// 修复：初始值设为 false，等待API返回真实值
const isCurrentUser = ref(false)

// 星期显示
const weekDays = ['日', '一', '二', '三', '四', '五', '六']

// 计算属性
const calendarDates = computed(() => {
  const dates = []
  const firstDay = new Date(currentYear.value, currentMonth.value - 1, 1)
  const lastDay = new Date(currentYear.value, currentMonth.value, 0)
  
  // 添加上个月末尾的几天
  const firstDayOfWeek = firstDay.getDay()
  for (let i = 0; i < firstDayOfWeek; i++) {
    const date = new Date(firstDay)
    date.setDate(date.getDate() - (firstDayOfWeek - i))
    dates.push(date)
  }
  
  // 添加本月的所有天
  for (let i = 1; i <= lastDay.getDate(); i++) {
    dates.push(new Date(currentYear.value, currentMonth.value - 1, i))
  }
  
  // 添加下个月开始的几天，补全6行（42个格子）
  const totalCells = 42
  const remaining = totalCells - dates.length
  for (let i = 1; i <= remaining; i++) {
    const date = new Date(lastDay)
    date.setDate(date.getDate() + i)
    dates.push(date)
  }
  
  return dates
})

const currentMonthDays = computed(() => {
  return new Date(currentYear.value, currentMonth.value, 0).getDate()
})

const currentMonthCheckins = computed(() => {
  return checkinRecords.value.filter(record => {
    const recordDate = new Date(record.checkinDate)
    return recordDate.getFullYear() === currentYear.value && 
           recordDate.getMonth() + 1 === currentMonth.value
  }).length
})

const totalPages = computed(() => {
  return Math.ceil(checkinRecords.value.length / pageSize)
})

const paginatedRecords = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return checkinRecords.value.slice(start, end)
})

// 从路由参数获取用户ID
const getTargetUserId = () => {
  const userId = route.params.userId
  if (userId === 'me') {
    return null // 表示当前用户
  }
  return parseInt(userId)
}

// API调用函数
const fetchCheckinData = async () => {
 loading.value = true
 try {
 const targetUserId = getTargetUserId()
  const url = targetUserId ? `/UserDetail/CheckInList/${targetUserId}` : '/UserDetail/CheckInList'
  
  const response = await apiClient.get(url)

 if (response.data.success) {
 const data = response.data.data
 stats.value = {
 totalCount: data.totalCount || 0,
  continuousDays: data.continuousDays || 0
 }
      
      // ★★★ 核心修复：字段映射 ★★★
 checkinRecords.value = (data.checkinList || []).map(item => ({
        id: item.Id,
        checkinDate: item.CheckinDate,
        type: item.Type,
        formattedDate: item.FormattedDate
      }))

      displayUser.value = data.userInfo || { 
username: '未知用户', 
points: 0, 
rank: 0,
id: targetUserId || 0
}


isCurrentUser.value = data.isCurrentUser || false

// 检查今天是否已签到（仅对当前用户有效）
if (isCurrentUser.value) {
const today = new Date().toDateString()
        // 注意：这里使用 checkinRecords.value 而不是 data.checkinList，因为我们已经转换过了
const todayRecord = checkinRecords.value.find(record => 
new Date(record.checkinDate).toDateString() === today
)

todayChecked.value = !!todayRecord
if (todayRecord) {
todayCheckinTime.value = todayRecord.checkinDate
}
} else {
todayChecked.value = false // 查看其他用户时不显示签到状态
}
}
} catch (error) {
    console.error('获取签到数据失败:', error)
    // 设置默认值避免渲染错误
    displayUser.value = { 
      username: '加载失败', 
      points: 0, 
      rank: 0,
      id: getTargetUserId() || 0
    }
    isCurrentUser.value = false // 出错时也设为false
  } finally {
    loading.value = false
  }
}

// 处理签到
const handleCheckin = async () => {
  if (isChecking.value || !isCurrentUser.value) return
  
  isChecking.value = true
  try {
    const response = await apiClient.post('/UserDetail/CheckIn')
    if (response.data.success) {
      await fetchCheckinData() // 重新加载数据
    }
  } catch (error) {
    console.error('签到失败:', error)
  } finally {
    isChecking.value = false
  }
}

// 检查是否是签到日
const isCheckinDay = (date) => {
  const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
  return checkinRecords.value.some(record => {
    const recordDate = new Date(record.checkinDate)
    const recordDateStr = `${recordDate.getFullYear()}-${String(recordDate.getMonth() + 1).padStart(2, '0')}-${String(recordDate.getDate()).padStart(2, '0')}`
    return recordDateStr === dateStr
  })
}

// 检查是否是今天
const isToday = (date) => {
  const today = new Date()
  return date.getDate() === today.getDate() && 
         date.getMonth() === today.getMonth() &&
         date.getFullYear() === today.getFullYear()
}

// 获取单元格类名
const getCellClass = (date) => {
  const classes = []
  
  if (date.getMonth() + 1 === currentMonth.value) {
    classes.push('current-month')
  } else {
    classes.push('other-month')
  }
  
  if (isToday(date)) {
    classes.push('today')
  }
  
  if (isCheckinDay(date)) {
    classes.push('checked')
  }
  
  return classes
}

// 获取权限等级文本
const getRankText = (rank) => {
  const rankMap = {
    0: '普通用户',
    1: '创作者',
    2: '管理员'
  }
  return rankMap[rank] || '未知'
}

// 时间格式化函数
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

const formatTime = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}:${String(date.getSeconds()).padStart(2, '0')}`
}

// 获取签到类型文本
const getCheckinType = (type) => {
  const types = {
    0: '每日签到',
    1: '每日签到',
    2: '活动签到'
  }
  return types[type] || '未知类型'
}

// 月份切换
const prevMonth = () => {
  if (currentMonth.value === 1) {
    currentMonth.value = 12
    currentYear.value -= 1
  } else {
    currentMonth.value -= 1
  }
}

const nextMonth = () => {
  if (currentMonth.value === 12) {
    currentMonth.value = 1
    currentYear.value += 1
  } else {
    currentMonth.value += 1
  }
}

// 分页控制
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

// 切换到我的签到
const switchToMyCheckin = () => {
  router.push('/profile/me')
}

// 检查管理员权限
const checkAdminPermission = async () => {
  try {
    const response = await apiClient.get('/Userinfo/me')
    if (response.data.rank >= 1) {
      isAdmin.value = true
    }
  } catch (error) {
    console.error('检查管理员权限失败:', error)
  }
}

// 监听路由变化
watch(
  () => route.params.userId,
  (newUserId) => {
    if (newUserId) {
      currentPage.value = 1
      currentYear.value = new Date().getFullYear()
      currentMonth.value = new Date().getMonth() + 1
      fetchCheckinData()
    }
  },
  { immediate: true }
)

// 监听月份年份变化，可以用于预加载数据
watch(
  [currentYear, currentMonth],
  () => {
    // 可以在这里实现月份切换时的数据预加载
    console.log(`切换到 ${currentYear.value}年${currentMonth.value}月`)
  }
)

// 初始化
onMounted(async () => {
  await fetchCheckinData()
  await checkAdminPermission()
})

// 组件卸载前的清理
import { onUnmounted } from 'vue'

onUnmounted(() => {
  // 清理工作
  console.log('签到组件卸载')
})
</script>

<style scoped>
.view-only-notice {
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  border-radius: 10px;
  padding: 20px;
  margin-bottom: 20px;
  text-align: center;
}

.notice-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.notice-content span {
  font-weight: 600;
  color: #856404;
  font-size: 16px;
}

.notice-content p {
  margin: 0;
  color: #856404;
  font-size: 14px;
}
.checkin-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* 头部样式 */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.header h2 {
  margin: 0;
  color: #333;
  font-size: 24px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.username {
  font-weight: 600;
  color: #333;
}

.tag-me, .tag-other {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.tag-me {
  background: #e3f2fd;
  color: #1976d2;
}

.tag-other {
  background: #f3e5f5;
  color: #7b1fa2;
}

.admin-badge {
  background: #fff3cd;
  color: #856404;
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 11px;
}

.user-switcher {
  margin-left: auto;
}

.switch-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  text-decoration: none;
}

.switch-btn:hover {
  background: #0056b3;
}

/* 统计卡片 */
.stats-section {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 15px;
  margin-bottom: 20px;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 10px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #333;
  margin-bottom: 5px;
}

.stat-label {
  color: #666;
  font-size: 14px;
}

/* 签到按钮 */
.checkin-action {
  background: white;
  padding: 30px;
  border-radius: 10px;
  text-align: center;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.checkin-btn {
  background: #28a745;
  color: white;
  border: none;
  padding: 15px 40px;
  font-size: 16px;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 10px;
}

.checkin-btn:hover:not(:disabled) {
  background: #218838;
}

.checkin-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.reward-hint {
  color: #666;
  margin: 0;
}

/* 已签到提示 */
.checked-today {
  background: #d4edda;
  padding: 20px;
  border-radius: 10px;
  text-align: center;
  margin-bottom: 20px;
  color: #155724;
}

.checked-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  font-weight: bold;
  margin-bottom: 5px;
}

.checkin-time {
  margin: 0;
  font-size: 14px;
}

/* 日期选择 */
.date-selector {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-bottom: 20px;
}

.nav-btn {
  background: #6c757d;
  color: white;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 18px;
}

.nav-btn:hover {
  background: #545b62;
}

.current-date {
  font-size: 18px;
  font-weight: 600;
  min-width: 120px;
  text-align: center;
}

/* 日历 */
.calendar {
  background: white;
  border-radius: 10px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  margin-bottom: 20px;
}

.week-header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 15px;
  text-align: center;
  font-weight: 600;
  color: #666;
}

.week-day {
  padding: 10px 0;
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 5px;
}

.calendar-cell {
  aspect-ratio: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  padding: 8px;
  position: relative;
  border: 1px solid #eee;
}

.calendar-cell.current-month {
  background: white;
}

.calendar-cell.other-month {
  background: #f8f9fa;
  color: #999;
}

.calendar-cell.today {
  background: #e3f2fd;
  border-color: #2196f3;
  font-weight: bold;
}

.calendar-cell.checked {
  background: #d4edda;
  border-color: #28a745;
  font-weight: bold;
}

.date-number {
  font-size: 14px;
  margin-bottom: 2px;
}

.checkin-indicator {
  font-size: 12px;
}

/* 签到记录 */
.records-section {
  background: white;
  border-radius: 10px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.records-section h3 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #333;
}

.loading, .no-data {
  text-align: center;
  padding: 40px;
  color: #666;
}

.records-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 300px;
  overflow-y: auto;
}

.record-item {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  align-items: center;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 6px;
  font-size: 14px;
}

.record-date {
  font-weight: 500;
}

.record-time {
  text-align: center;
  color: #666;
}

.record-type {
  text-align: right;
  color: #007bff;
  font-weight: 500;
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 15px;
  margin-top: 15px;
}

.page-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.page-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.page-info {
  color: #666;
  font-size: 14px;
}

/* 响应式 */
@media (max-width: 768px) {
  .checkin-container {
    padding: 10px;
  }
  
  .header {
    flex-direction: column;
    gap: 10px;
    text-align: center;
  }
  
  .user-switcher {
    margin-left: 0;
  }
  
  .stats-section {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .calendar-grid {
    gap: 2px;
  }
  
  .date-number {
    font-size: 12px;
  }
  
  .record-item {
    grid-template-columns: 1fr;
    gap: 5px;
    text-align: center;
  }
}
</style>