<template>
  <div class="union-review-panel">
    <div class="panel-header">
      <h3>联合内容审核</h3>
      <div class="panel-filters">
        <!-- 修改选项值为数字字符串 -->
        <select v-model="filterStatus" @change="fetchData">
          <option value="0">待审核</option>
          <option value="1">已通过</option>
          <option value="2">已拒绝</option>
          <option value="">全部</option>
        </select>
        
        <select v-model="contentType" @change="fetchData">
          <option value="">所有类型</option>
          <option value="1">类型1</option>
          <option value="2">类型2</option>
          <option value="3">类型3</option>
        </select>
      </div>
    </div>

    <!-- 表格内容 -->
    <div class="table-container">
      <table class="review-table">
        <thead>
          <tr>
            <th>活动名称</th>
            <th>主办方</th>
            <th>活动类型</th>
            <th>开始时间</th>
            <th>结束时间</th>
            <th>QQ群</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="8" class="loading-cell">加载中...</td>
          </tr>
          
          <tr v-else-if="!items.length">
            <td colspan="8" class="empty-cell">暂无数据</td>
          </tr>
          
          <tr v-else v-for="item in items" :key="item.id">
            <td>{{ item.name }}</td>
            <td>{{ item.host }}</td>
            <td>类型{{ item.type }}</td>
            <td>{{ formatDate(item.startdate) }}</td>
            <td>{{ formatDate(item.enddate) }}</td>
            <td>{{ item.QQgroup || '-' }}</td>
            <td>
              <span :class="`status-${item.verify}`">
                {{ getStatusText(item.verify) }}
              </span>
            </td>
            <td>
              <button 
                @click="approveItem(item)" 
                :disabled="item.verify === 1"
                class="btn-success"
              >
                通过
              </button>
              <button 
                @click="rejectItem(item)" 
                :disabled="item.verify === 2"
                class="btn-danger"
              >
                拒绝
              </button>
              <button @click="viewDetails(item)" class="btn-info">
                详情
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination" v-if="total > 0">
      <div class="pagination-info">
        显示 {{ items.length }} 条记录，共 {{ total }} 条
      </div>
      <div class="pagination-controls">
        <button @click="prevPage" :disabled="page <= 1">上一页</button>
        <span>第 {{ page }} 页</span>
        <button @click="nextPage" :disabled="page * pageSize >= total">下一页</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch,  defineEmits, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  search: String
})

const emit = defineEmits(['update-count'])

// 响应式数据 - 使用数字字符串
const filterStatus = ref('0') // 使用 '0', '1', '2' 而不是 'pending', 'approved', 'rejected'
const contentType = ref('')
const items = ref([])
const total = ref(0)
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)

// 状态映射 - 用于显示
const statusMap = {
  0: { text: '待审核', class: 'pending' },
  1: { text: '已通过', class: 'approved' },
  2: { text: '已拒绝', class: 'rejected' }
}

// 方法
function getStatusText(status) {
  return statusMap[status]?.text || '未知状态'
}

function truncateText(text, length) {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

function formatDate(date) {
  if (!date) return '-'
  try {
    return new Date(date).toLocaleDateString('zh-CN')
  } catch {
    return date
  }
}

// 修复API调用 - 正确的参数格式
async function fetchData() {
  loading.value = true
  try {
    // 构建正确的查询参数
    const params = {
      page: page.value,
      pageSize: pageSize.value
    }

    // 添加筛选参数 - 确保是数字类型
    if (filterStatus.value !== '') {
      // 将字符串状态映射为数字
      const statusMap = {
        'pending': 0,
        'approved': 1,
        'rejected': 2,
        '0': 0,
        '1': 1,
        '2': 2
      }
      
      const statusValue = statusMap[filterStatus.value]
      if (statusValue !== undefined) {
        params.verify = statusValue
      }
    }

    if (contentType.value !== '') {
      params.type = parseInt(contentType.value)
    }

    if (props.search) {
      params.q = props.search
    }

    console.log('🔍 API请求参数:', params)

    const response = await apiClient.get('/ChaiLianHe/list', { params })
    
    console.log('✅ API响应:', response.data)

    if (response.data && response.data.data) {
      items.value = response.data.data
      total.value = response.data.total || 0
      emit('update-count', items.value.length)
    } else {
      items.value = []
      total.value = 0
      emit('update-count', 0)
    }
  } catch (error) {
    console.error('❌ 获取联合内容数据失败', error)
    
    if (error.response?.data?.errors) {
      console.error('❌ 验证错误详情:', error.response.data.errors)
      alert('参数错误: ' + JSON.stringify(error.response.data.errors))
    }
    
    items.value = []
    total.value = 0
    emit('update-count', 0)
  } finally {
    loading.value = false
  }
}

// 审核操作
async function approveItem(item) {
  if (!confirm(`确定要通过活动 "${item.name}" 吗？`)) return
  
  try {
    await apiClient.post('/ChaiLianHe/moderation/approve', {
      Id: item.id
    })
    
    await fetchData()
    alert('操作成功')
  } catch (error) {
    console.error('审核通过失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

async function rejectItem(item) {
  const reason = prompt('请输入拒绝原因（可选）')
  if (reason === null) return
  
  if (!confirm(`确定要拒绝活动 "${item.name}" 吗？`)) return
  
  try {
    await apiClient.post('/ChaiLianHe/moderation/reject', {
      Id: item.id,
      Note: reason
    })
    
    await fetchData()
    alert('操作成功')
  } catch (error) {
    console.error('审核拒绝失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

function viewDetails(item) {
  console.log('查看详情:', item)
  alert(`活动详情：
名称：${item.name}
主办方：${item.host}
类型：类型${item.type}
时间：${formatDate(item.startdate)} - ${formatDate(item.enddate)}
QQ群：${item.QQgroup || '无'}
状态：${getStatusText(item.verify)}
描述：${item.desc || '无描述'}
规则：${item.rules || '无规则'}`)
}

// 分页
function prevPage() {
  if (page.value > 1) {
    page.value--
    fetchData()
  }
}

function nextPage() {
  if (page.value * pageSize.value < total.value) {
    page.value++
    fetchData()
  }
}

function onSearch(searchTerm) {
  page.value = 1
  fetchData()
}

function refresh() {
  page.value = 1
  fetchData()
}

// 监听和暴露
watch(() => props.search, (newSearch) => {
  onSearch(newSearch)
})

defineExpose({
  onSearch,
  refresh
})

// 初始化
onMounted(() => {
  console.log('🚀 初始化联合内容审核面板...')
  fetchData()
})
</script>

<style scoped>
.union-review-panel {
  padding: 20px;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.panel-filters {
  display: flex;
  gap: 10px;
}

.panel-filters select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.table-container {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.review-table {
  width: 100%;
  border-collapse: collapse;
}

.review-table th,
.review-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.review-table th {
  background: #f5f5f5;
  font-weight: 600;
}

.loading-cell,
.empty-cell {
  text-align: center;
  padding: 40px;
  color: #666;
}

.status-0 { color: #f59e0b; font-weight: 600; }
.status-1 { color: #10b981; font-weight: 600; }
.status-2 { color: #ef4444; font-weight: 600; }

.btn-success, .btn-danger, .btn-info {
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 5px;
  font-size: 12px;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.btn-info {
  background: #3b82f6;
  color: white;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  padding: 10px;
}

.pagination button {
  padding: 8px 16px;
  border: 1px solid #ddd;
  background: white;
  border-radius: 4px;
  cursor: pointer;
}

.pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>