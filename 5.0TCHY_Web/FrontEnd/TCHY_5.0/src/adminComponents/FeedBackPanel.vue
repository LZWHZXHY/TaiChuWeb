<template>
  <div class="feedback-admin-panel">
    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" :model="filters" class="filter-form">
        <el-form-item label="状态">
          <el-select v-model="filters.status" placeholder="全部状态" clearable style="width: 140px" @change="fetchFeedbacks">
            <el-option label="全部" :value="-1" />
            <el-option label="待处理" :value="0" />
            <el-option label="处理中" :value="1" />
            <el-option label="已解决" :value="2" />
            <el-option label="已驳回" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="filters.type" placeholder="全部分类" clearable style="width: 140px" @change="fetchFeedbacks">
            <el-option label="全部" :value="0" />
            <el-option v-for="cat in categories" :key="cat.value" :label="cat.label" :value="cat.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="搜索">
          <el-input v-model="filters.keyword" placeholder="搜索标题/ID/内容" clearable @keyup.enter="fetchFeedbacks" style="width: 200px" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchFeedbacks" :loading="loading">查询</el-button>
          <el-button @click="resetFilters">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" class="table-card">
      <el-table :data="feedbacks" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="80" sortable />
        
        <el-table-column label="分类" width="120">
          <template #default="{ row }">
            <el-tag :type="getTypeTag(row.type)">{{ getTypeLabel(row.type) }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column label="标题" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="table-title">{{ row.title }}</span>
          </template>
        </el-table-column>

        <el-table-column label="图片" width="100" align="center">
          <template #default="{ row }">
            <el-image 
              v-if="row.imageFullUrl" 
              style="width: 40px; height: 40px; border-radius: 4px;"
              :src="row.imageFullUrl" 
              :preview-src-list="[row.imageFullUrl]"
              preview-teleported
              fit="cover"
            />
            <span v-else class="text-gray">-</span>
          </template>
        </el-table-column>

        <el-table-column prop="contactQQ" label="联系QQ" width="140">
           <template #default="{ row }">
             {{ row.contactQQ || '-' }}
           </template>
        </el-table-column>

        <el-table-column label="提交时间" width="170" sortable prop="createTime">
          <template #default="{ row }">
            {{ formatDate(row.createTime) }}
          </template>
        </el-table-column>

        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="getStatusTag(row.status)" effect="dark">
              {{ getStatusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openProcessDialog(row)">
              {{ row.status === 2 || row.status === 3 ? '查看/修改' : '处理' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="pagination.page"
          v-model:page-size="pagination.pageSize"
          :total="pagination.total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      title="反馈处理"
      width="600px"
      destroy-on-close
    >
      <div v-if="currentRow" class="detail-container">
        <div class="user-content-section">
          <div class="detail-item">
            <span class="label">反馈分类：</span>
            <el-tag size="small" :type="getTypeTag(currentRow.type)">{{ getTypeLabel(currentRow.type) }}</el-tag>
          </div>
          <div class="detail-item">
            <span class="label">反馈标题：</span>
            <span class="value title">{{ currentRow.title }}</span>
          </div>
          <div class="detail-item content-box">
            <div class="label">详细描述：</div>
            <div class="value content">{{ currentRow.content }}</div>
          </div>
          <div class="detail-item" v-if="currentRow.imageFullUrl">
            <div class="label">附件图片：</div>
            <div class="image-box">
              <el-image 
                :src="currentRow.imageFullUrl" 
                :preview-src-list="[currentRow.imageFullUrl]"
                fit="contain"
                class="detail-image"
              />
            </div>
          </div>
          <div class="detail-item">
             <span class="label">联系 QQ：</span>
             <span class="value">{{ currentRow.contactQQ || '未提供' }}</span>
          </div>
          <div class="detail-item">
             <span class="label">提交时间：</span>
             <span class="value">{{ formatDate(currentRow.createTime) }}</span>
          </div>
        </div>

        <el-divider content-position="left">管理员处理</el-divider>

        <el-form ref="processFormRef" :model="processForm" :rules="processRules" label-position="top">
          <el-form-item label="更新状态" prop="status">
            <el-radio-group v-model="processForm.status">
              <el-radio-button :label="0">待处理</el-radio-button>
              <el-radio-button :label="1">处理中</el-radio-button>
              <el-radio-button :label="2">已解决</el-radio-button>
              <el-radio-button :label="3">驳回/关闭</el-radio-button>
            </el-radio-group>
          </el-form-item>

          <el-form-item label="回复内容 (用户可见)" prop="reply">
            <el-input 
              v-model="processForm.reply" 
              type="textarea" 
              :rows="4" 
              placeholder="请输入处理结果或回复内容，用户将在反馈详情中看到..."
            />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" @click="submitProcess" :loading="submitting">
            提交处理
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import apiClient from '@/utils/api' // 确保引入你的 api 工具

// === 常量定义 ===
const categories = [
  { value: 1, label: '网站BUG' },
  { value: 2, label: '社区意见' },
  { value: 3, label: '内容举报' },
  { value: 4, label: '其他' }
]

// === 状态定义 ===
const loading = ref(false)
const submitting = ref(false)
const feedbacks = ref([])
const dialogVisible = ref(false)
const currentRow = ref(null)

// 筛选条件
const filters = reactive({
  status: -1,
  type: 0,
  keyword: ''
})

// 分页
const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
})

// 处理表单
const processForm = reactive({
  id: null,
  status: 0,
  reply: ''
})

const processRules = {
  status: [{ required: true, message: '请选择状态', trigger: 'change' }],
  reply: [{ required: false, message: '请输入回复内容', trigger: 'blur' }] // 根据需求，可以设为必填
}

const processFormRef = ref(null)

// === 数据获取 ===
const fetchFeedbacks = async () => {
  loading.value = true
  try {
    // 构造查询参数
    const params = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      status: filters.status === -1 ? null : filters.status, // 后端若支持 null 查全部
      type: filters.type === 0 ? null : filters.type,
      keyword: filters.keyword
    }

    // 假设后端有一个 admin 列表接口，或者复用 list 接口但带权限
    const res = await apiClient.get('/FeedBack/list', { params })
    
    if (res.data && res.data.success) {
      feedbacks.value = res.data.data.items
      pagination.total = res.data.data.totalCount || res.data.data.items.length // 根据后端返回结构调整
    } else {
      ElMessage.error(res.data?.message || '获取列表失败')
    }
  } catch (error) {
    console.error(error)
    ElMessage.error('网络错误，获取失败')
  } finally {
    loading.value = false
  }
}

// === 操作逻辑 ===
const resetFilters = () => {
  filters.status = -1
  filters.type = 0
  filters.keyword = ''
  pagination.page = 1
  fetchFeedbacks()
}

const handleSizeChange = (val) => {
  pagination.pageSize = val
  fetchFeedbacks()
}

const handlePageChange = (val) => {
  pagination.page = val
  fetchFeedbacks()
}

// 打开弹窗
const openProcessDialog = (row) => {
  currentRow.value = row
  // 初始化表单
  processForm.id = row.id
  processForm.status = row.status
  processForm.reply = row.adminReply || '' // 假设后端返回字段叫 adminReply
  dialogVisible.value = true
}

// 提交处理
const submitProcess = async () => {
  if (!processFormRef.value) return
  
  await processFormRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true
      try {
        // 调用后端处理接口
        const res = await apiClient.post('/FeedBack/process', {
          id: processForm.id,
          status: processForm.status,
          adminReply: processForm.reply
        })

        if (res.data && res.data.success) {
          ElMessage.success('处理成功')
          dialogVisible.value = false
          fetchFeedbacks() // 刷新列表
        } else {
          ElMessage.error(res.data?.message || '提交失败')
        }
      } catch (error) {
        ElMessage.error('网络异常，提交失败')
      } finally {
        submitting.value = false
      }
    }
  })
}

// === 辅助函数 ===
const getTypeLabel = (val) => {
  const t = categories.find(c => c.value === val)
  return t ? t.label : '未知'
}

const getTypeTag = (val) => {
  const map = { 1: 'danger', 2: 'primary', 3: 'warning', 4: 'info' }
  return map[val] || 'info'
}

const getStatusLabel = (val) => {
  const map = { 0: '待处理', 1: '处理中', 2: '已解决', 3: '已驳回' }
  return map[val] || '未知'
}

const getStatusTag = (val) => {
  const map = { 0: 'info', 1: 'primary', 2: 'success', 3: 'danger' }
  return map[val] || 'info'
}

const formatDate = (str) => {
  if (!str) return '-'
  return new Date(str).toLocaleString()
}

// === 生命周期 ===
onMounted(() => {
  fetchFeedbacks()
})
</script>

<style scoped>
.feedback-admin-panel {
  padding: 24px;
  background-color: #f5f7fa;
  min-height: 100vh;
}

.filter-card {
  margin-bottom: 20px;
}

.filter-form .el-form-item {
  margin-bottom: 0;
  margin-right: 24px;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.text-gray {
  color: #909399;
}

/* 弹窗详情样式 */
.detail-container {
  padding: 0 10px;
}

.user-content-section {
  background-color: #f8f9fa;
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 20px;
  border: 1px solid #ebeef5;
}

.detail-item {
  margin-bottom: 12px;
  display: flex;
  line-height: 1.5;
}

.detail-item .label {
  font-weight: bold;
  color: #606266;
  width: 80px;
  flex-shrink: 0;
}

.detail-item .value {
  color: #303133;
  word-break: break-all;
}

.detail-item .value.title {
  font-weight: bold;
}

.detail-item .value.content {
  white-space: pre-wrap;
}

.image-box {
  margin-top: 8px;
  border: 1px solid #e4e7ed;
  padding: 4px;
  border-radius: 4px;
  background: white;
  display: inline-block;
}

.detail-image {
  max-width: 100%;
  max-height: 200px;
  display: block;
}
</style>