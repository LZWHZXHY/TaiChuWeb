<template>
  <div class="society-review-panel">
    <!-- 面板头部 -->
    <div class="panel-header">
      <h3>社团审核</h3>
      <div class="panel-filters">
        <select v-model="filterStatus" @change="fetchData">
          <option value="0">待审核</option>
          <option value="1">已通过</option>
          <option value="2">已拒绝</option>
          <option value="">全部</option>
        </select>
        
        <select v-model="societyType" @change="fetchData">
          <option value="">所有类型</option>
          <option :value="1">类型1</option>
          <option :value="2">类型2</option>
          <option :value="3">类型3</option>
        </select>

        <select v-model="sortBy" @change="fetchData">
          <option value="id_desc">最新优先</option>
          <option value="name_asc">名称A-Z</option>
          <option value="size_desc">规模从大到小</option>
          <option value="verify_asc">状态排序</option>
        </select>

        
        <button @click="showDebugInfo" class="btn outline small">调试</button>
      </div>
    </div>


    
    
    <!-- 调试信息 -->
     <!--  
    <div v-if="debugInfo" class="debug-panel">
      <h4>调试信息</h4>
      <div class="debug-content">
        <div><strong>API端点:</strong> {{ debugInfo.endpoint }}</div>
        <div><strong>请求参数:</strong> {{ JSON.stringify(debugInfo.params) }}</div>
        <div><strong>响应状态:</strong> {{ debugInfo.status }}</div>
        <div><strong>数据条数:</strong> {{ debugInfo.count }}</div>
        <div><strong>第一条数据:</strong> {{ JSON.stringify(debugInfo.sample) }}</div>
      </div>
    </div>
    -->
    <!-- 表格内容 -->
    <div class="table-container">
      <table class="review-table">
        <thead>
          <tr>
            <th class="th checkbox">
              <input 
                type="checkbox" 
                @change="toggleSelectAll" 
                :checked="allSelected" 
                :disabled="loading || !items.length" 
              />
            </th>
            <th class="th">社团名称</th>
            <th class="th">团长</th>
            <th class="th">类型</th>
            <th class="th">规模</th>
            <th class="th">状态</th>
            <th class="th">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="7" class="td empty">
              <div class="loading-spinner">
                <div class="spinner"></div>
                <span>加载中…</span>
              </div>
            </td>
          </tr>

          <tr v-else-if="!items.length">
            <td colspan="7" class="td empty">
              <div class="empty-state">
                <div class="empty-icon">🏢</div>
                <div class="empty-text">
                  <strong>暂无社团数据</strong>
                  <span>当前筛选条件下无社团记录</span>
                  <div v-if="debugInfo" class="debug-hint">
                    <small>提示: {{ debugInfo.hint }}</small>
                  </div>
                </div>
              </div>
            </td>
          </tr>

          <tr v-else v-for="item in items" :key="item.id" class="table-row">
            <td class="td checkbox">
              <input 
                type="checkbox" 
                :value="item.id" 
                v-model="selectedItems" 
              />
            </td>
            <td class="td name-cell">
              <div class="name-wrapper">
                <strong class="society-name" :title="item.name">{{ item.name }}</strong>
                <div v-if="item.desc" class="society-desc" :title="item.desc">
                  {{ truncateText(item.desc, 50) }}
                </div>
              </div>
            </td>
            <td class="td leader-cell">{{ item.leader }}</td>
            <td class="td type-cell">
              <span class="type-badge" :class="`type-${item.type}`">
                类型{{ item.type }}
              </span>
            </td>
            <td class="td size-cell">{{ item.size }}人</td>
            <td class="td status-cell">
              <span class="status-badge" :class="`status-${item.verify}`">
                {{ getStatusText(item.verify) }}
              </span>
            </td>
            <td class="td actions">
              <div class="action-buttons">
                <button 
                  class="btn success small" 
                  @click="approveItem(item)"
                  :disabled="item.verify === 1"
                >
                  <span class="btn-icon">✓</span>
                  通过
                </button>
                <button 
                  class="btn danger small" 
                  @click="rejectItem(item)"
                  :disabled="item.verify === 2"
                >
                  <span class="btn-icon">✕</span>
                  拒绝
                </button>
                <button 
                  class="btn outline small" 
                  @click="viewDetails(item)"
                >
                  <span class="btn-icon">🔍</span>
                  详情
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 分页控件 -->
    <div class="pagination" v-if="items.length > 0">
      <div class="pagination-info">
        显示 {{ items.length }} 条记录
      </div>
      <div class="pagination-controls">
        <button 
          class="btn ghost small" 
          @click="prevPage" 
          :disabled="page <= 1"
        >
          上一页
        </button>
        <span class="page-number">第 {{ page }} 页</span>
        <button 
          class="btn ghost small" 
          @click="nextPage" 
          :disabled="items.length < pageSize"
        >
          下一页
        </button>
      </div>
    </div>

    <!-- 批量操作栏 -->
    <div class="bulk-actions" v-if="selectedItems.length > 0">
      <div class="bulk-info">
        已选择 {{ selectedItems.length }} 个社团
      </div>
      <div class="bulk-buttons">
        <button class="btn success small" @click="bulkApprove">
          批量通过
        </button>
        <button class="btn danger small" @click="bulkReject">
          批量拒绝
        </button>
        <button class="btn outline small" @click="clearSelection">
          取消选择
        </button>
      </div>
    </div>

    <!-- 详情模态框 -->
    <div v-if="selectedItem" class="modal-backdrop" @click.self="selectedItem = null">
      <div class="modal">
        <div class="modal-header">
          <h3>{{ selectedItem.name }} - 详情</h3>
          <button class="modal-close" @click="selectedItem = null">×</button>
        </div>
        <div class="modal-body">
          <div class="detail-grid">
            <div class="detail-item">
              <label>社团名称：</label>
              <span>{{ selectedItem.name }}</span>
            </div>
            <div class="detail-item">
              <label>团长：</label>
              <span>{{ selectedItem.leader }}</span>
            </div>
            <div class="detail-item">
              <label>类型：</label>
              <span>类型{{ selectedItem.type }}</span>
            </div>
            <div class="detail-item">
              <label>规模：</label>
              <span>{{ selectedItem.size }}人</span>
            </div>
            <div class="detail-item">
              <label>状态：</label>
              <span class="status-badge" :class="`status-${selectedItem.verify}`">
                {{ getStatusText(selectedItem.verify) }}
              </span>
            </div>
            <div class="detail-item full-width">
              <label>主页链接：</label>
              <a :href="selectedItem.url" target="_blank" class="url-link">{{ selectedItem.url }}</a>
            </div>
            <div class="detail-item full-width">
              <label>社团描述：</label>
              <div class="description-text">{{ selectedItem.desc }}</div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn outline" @click="selectedItem = null">关闭</button>
          <button 
            v-if="selectedItem.verify !== 1"
            class="btn success" 
            @click="approveItem(selectedItem)"
          >
            通过审核
          </button>
          <button 
            v-if="selectedItem.verify !== 2"
            class="btn danger" 
            @click="rejectItem(selectedItem)"
          >
            拒绝审核
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch,  defineEmits, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  search: String
})

const emit = defineEmits(['update-count'])

// 响应式数据
const items = ref([])
const loading = ref(false)
const selectedItems = ref([])
const selectedItem = ref(null)
const debugInfo = ref(null)

// 筛选条件 - 修正为与后端API匹配
const filterStatus = ref('0') // 默认待审核，使用数字字符串
const societyType = ref('')
const sortBy = ref('id_desc')
const page = ref(1)
const pageSize = ref(20)

// 计算属性
const allSelected = computed(() => {
  return items.value.length > 0 && selectedItems.value.length === items.value.length
})

// 状态映射
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

function showDebugInfo() {
  console.group('🔍 调试信息')
  console.log('📊 当前数据:', items.value)
  console.log('🔧 筛选条件:', {
    verify: filterStatus.value,
    type: societyType.value,
    sort: sortBy.value,
    q: props.search
  })
  console.log('🔄 加载状态:', loading.value)
  console.groupEnd()
  
  alert('调试信息已输出到控制台，请按F12查看')
}

// API调用 - 修正为调用正确的端点
async function fetchData() {
  loading.value = true
  debugInfo.value = null
  
  try {
    // 构建查询参数 - 与后端API参数匹配
    const params = {
      verify: filterStatus.value ? parseInt(filterStatus.value) : undefined,
      type: societyType.value ? parseInt(societyType.value) : undefined,
      q: props.search || undefined,
      sort: sortBy.value
      // 注意：基础列表接口不支持分页参数 page/pageSize
    }

    // 移除空值参数
    Object.keys(params).forEach(key => {
      if (params[key] === undefined || params[key] === '' || params[key] === null) {
        delete params[key]
      }
    })

    console.log('🔍 调用基础列表接口: /ChaiSheTuan/list')
    console.log('🔍 请求参数:', params)

    // 调用正确的API端点：基础列表接口
    const response = await apiClient.get('/ChaiSheTuan/list', { params })
    
    console.log('✅ API响应:', response)
    console.log('📊 响应数据:', response.data)

    // 记录调试信息
    debugInfo.value = {
      endpoint: '/ChaiSheTuan/list',
      params: params,
      status: response.status,
      count: Array.isArray(response.data) ? response.data.length : 0,
      sample: Array.isArray(response.data) && response.data.length > 0 ? response.data[0] : null,
      hint: Array.isArray(response.data) && response.data.length === 0 ? 
            '数据库中没有符合条件的数据' : '数据加载成功'
    }

    if (response.data && Array.isArray(response.data)) {
      items.value = response.data
      emit('update-count', items.value.length)
      console.log(`✅ 成功获取 ${items.value.length} 条数据`)
    } else {
      items.value = []
      emit('update-count', 0)
      console.warn('⚠️ API返回数据格式异常')
    }
  } catch (error) {
    console.error('❌ 获取社团数据失败', error)
    console.error('错误详情:', error.response?.data)
    
    debugInfo.value = {
      endpoint: '/ChaiSheTuan/list',
      params: params || {},
      status: error.response?.status || '网络错误',
      count: 0,
      sample: null,
      hint: `请求失败: ${error.response?.status} ${error.message}`
    }
    
    items.value = []
    emit('update-count', 0)
    
    if (error.response?.status === 403) {
      alert('权限不足：请使用管理员账号登录')
    } else if (error.response?.status === 401) {
      alert('请先登录')
    } else if (error.response?.status === 404) {
      alert('API接口不存在，请检查后端服务')
    } else {
      alert('请求失败：' + (error.message || '网络错误'))
    }
  } finally {
    loading.value = false
  }
}

// 单个操作 - 使用审核接口
async function approveItem(item) {
  if (!confirm(`确定要通过社团 "${item.name}" 吗？`)) return
  
  try {
    // 调用审核接口
    await apiClient.post('/ChaiSheTuan/moderation/approve', {
      Entity: 'ChaiSheTuan',
      Id: item.id
    })
    
    await fetchData() // 重新加载数据
    alert('操作成功')
  } catch (error) {
    console.error('审核通过失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

async function rejectItem(item) {
  const reason = prompt('请输入拒绝原因（可选）')
  if (reason === null) return
  
  if (!confirm(`确定要拒绝社团 "${item.name}" 吗？`)) return
  
  try {
    // 调用审核接口
    await apiClient.post('/ChaiSheTuan/moderation/reject', {
      Entity: 'ChaiSheTuan',
      Id: item.id,
      Note: reason
    })
    
    await fetchData() // 重新加载数据
    alert('操作成功')
  } catch (error) {
    console.error('审核拒绝失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

// 批量操作
async function bulkApprove() {
  if (selectedItems.value.length === 0) return
  
  if (!confirm(`确定要通过选中的 ${selectedItems.value.length} 个社团吗？`)) return
  
  try {
    for (const itemId of selectedItems.value) {
      await apiClient.post('/ChaiSheTuan/moderation/approve', {
        Entity: 'ChaiSheTuan',
        Id: itemId
      })
    }
    
    await fetchData()
    selectedItems.value = []
    alert('批量通过成功')
  } catch (error) {
    console.error('批量通过失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

async function bulkReject() {
  if (selectedItems.value.length === 0) return
  
  const reason = prompt('请输入拒绝原因（将应用于所有选中的社团）')
  if (reason === null) return
  
  if (!confirm(`确定要拒绝选中的 ${selectedItems.value.length} 个社团吗？`)) return
  
  try {
    for (const itemId of selectedItems.value) {
      await apiClient.post('/ChaiSheTuan/moderation/reject', {
        Entity: 'ChaiSheTuan',
        Id: itemId,
        Note: reason
      })
    }
    
    await fetchData()
    selectedItems.value = []
    alert('批量拒绝成功')
  } catch (error) {
    console.error('批量拒绝失败', error)
    alert('操作失败：' + (error.response?.data?.message || '服务器错误'))
  }
}

// 选择操作
function toggleSelectAll(event) {
  if (event.target.checked) {
    selectedItems.value = items.value.map(item => item.id)
  } else {
    selectedItems.value = []
  }
}

function clearSelection() {
  selectedItems.value = []
}

function viewDetails(item) {
  selectedItem.value = item
}

// 分页 - 基础列表接口不支持分页，暂时禁用
function prevPage() {
  if (page.value > 1) {
    page.value--
    fetchData()
  }
}

function nextPage() {
  // 基础列表接口不支持分页，暂时简单处理
  if (items.value.length === pageSize.value) {
    page.value++
    fetchData()
  }
}

// 搜索处理
function onSearch(searchTerm) {
  page.value = 1
  fetchData()
}

function refresh() {
  page.value = 1
  selectedItems.value = []
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
  console.log('🚀 初始化社团审核面板...')
  console.log('📝 使用基础列表接口: /ChaiSheTuan/list')
  console.log('📝 审核操作接口: /ChaiSheTuan/moderation/approve|reject')
  fetchData()
})
</script>

<style scoped>
.society-review-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  height: 100%;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 8px;
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.panel-filters {
  display: flex;
  gap: 12px;
  align-items: center;
}

.panel-filters select {
  padding: 8px 12px;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: var(--card-bg);
  font-size: 14px;
  min-width: 120px;
}

/* 调试面板样式 */
.debug-panel {
  background: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.debug-panel h4 {
  margin: 0 0 12px 0;
  color: #333;
  border-bottom: 1px solid #ccc;
  padding-bottom: 8px;
}

.debug-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.debug-content div {
  padding: 4px 0;
}

.debug-content strong {
  color: #666;
  min-width: 100px;
  display: inline-block;
}

.debug-hint {
  margin-top: 8px;
  padding: 8px;
  background: #fff3cd;
  border-radius: 4px;
  border-left: 4px solid #ffc107;
}

.debug-hint small {
  color: #856404;
}

.table-container {
  flex: 1;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  overflow: hidden;
  background: var(--card-bg);
}

.review-table {
  width: 100%;
  border-collapse: collapse;
}

.th, .td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.th {
  background: var(--light-bg);
  font-weight: 600;
  font-size: 13px;
  color: var(--text-secondary);
}

.th.checkbox {
  width: 40px;
  text-align: center;
}

.th.actions {
  width: 200px;
}

.name-cell {
  max-width: 250px;
}

.name-wrapper {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.society-name {
  font-weight: 600;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.society-desc {
  font-size: 12px;
  color: var(--text-secondary);
  line-height: 1.4;
  display: -webkit-box;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.type-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  background: var(--light-bg);
  color: var(--text-secondary);
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.status-badge.status-0 { background: #fef3c7; color: #d97706; }
.status-badge.status-1 { background: #d1fae5; color: #065f46; }
.status-badge.status-2 { background: #fee2e2; color: #dc2626; }

.action-buttons {
  display: flex;
  gap: 6px;
}

.btn {
  padding: 6px 10px;
  border: 1px solid;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn.small {
  padding: 4px 8px;
  font-size: 11px;
}

.btn.success {
  background: #10b981;
  border-color: #10b981;
  color: white;
}

.btn.danger {
  background: #ef4444;
  border-color: #ef4444;
  color: white;
}

.btn.outline {
  background: transparent;
  border-color: var(--border-color);
  color: var(--text-secondary);
}

.btn.ghost {
  background: transparent;
  border-color: transparent;
  color: var(--text-secondary);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  border-top: 1px solid var(--border-color);
}

.bulk-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: var(--light-bg);
  border-radius: 6px;
  border: 1px solid var(--border-color);
}

/* 模态框样式 */
.modal-backdrop {
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
}

.modal {
  background: var(--card-bg);
  border-radius: 8px;
  padding: 20px;
  max-width: 600px;
  width: 90%;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.modal-close {
  background: none;
  border: none;
  font-size: 20px;
  cursor: pointer;
  padding: 4px;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-item.full-width {
  grid-column: 1 / -1;
}

.detail-item label {
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 13px;
}

.url-link {
  color: var(--primary-color);
  text-decoration: none;
}

.url-link:hover {
  text-decoration: underline;
}

.description-text {
  line-height: 1.5;
  padding: 8px;
  background: var(--light-bg);
  border-radius: 4px;
  white-space: pre-wrap;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--border-color);
}

/* 加载和空状态 */
.loading-spinner, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  gap: 12px;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid var(--border-color);
  border-top: 2px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-icon {
  font-size: 32px;
  opacity: 0.7;
}

.empty-text {
  text-align: center;
}

.empty-text strong {
  display: block;
  margin-bottom: 4px;
  color: var(--text-primary);
}
</style>