<template>
  <div class="post-review-container">
    <!-- 顶部标题和搜索 -->
    <div class="review-header">
      <div class="header-left">
        <h1 class="page-title">帖子审核</h1>
        <p class="page-subtitle">管理用户发布的帖子内容</p>
      </div>
      <div class="header-right">
        <div class="search-box">
          <el-input
            v-model="searchQuery"
            placeholder="搜索帖子标题、内容、作者..."
            clearable
            @input="handleSearch"
            @clear="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>
      </div>
    </div>

    <!-- 审核统计卡片 -->
    <div class="stats-cards">
      <el-row :gutter="20">
        <el-col :xs="12" :sm="6" :md="6" :lg="6">
          <div class="stat-card">
            <div class="stat-icon pending">
              <el-icon><Clock /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ stats.pending }}</div>
              <div class="stat-label">待审核</div>
            </div>
          </div>
        </el-col>
        <el-col :xs="12" :sm="6" :md="6" :lg="6">
          <div class="stat-card">
            <div class="stat-icon total">
              <el-icon><Document /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ stats.total }}</div>
              <div class="stat-label">总帖子数</div>
            </div>
          </div>
        </el-col>
        <el-col :xs="12" :sm="6" :md="6" :lg="6">
          <div class="stat-card">
            <div class="stat-icon hidden">
              <el-icon><Hide /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ stats.hidden }}</div>
              <div class="stat-label">已隐藏</div>
            </div>
          </div>
        </el-col>
        <el-col :xs="12" :sm="6" :md="6" :lg="6">
          <div class="stat-card">
            <div class="stat-icon banned">
              <el-icon><Warning /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ stats.banned }}</div>
              <div class="stat-label">已封禁</div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 审核列表 -->
    <div class="review-content">
      <el-card class="review-card">
        <template #header>
          <div class="card-header">
            <span class="card-title">帖子列表</span>
            <div class="card-actions">
              <el-select
                v-model="postType"
                placeholder="全部类型"
                clearable
                style="width: 120px; margin-right: 10px;"
                @change="handleFilterChange"
              >
                <el-option label="全部类型" :value="null" />
                <el-option label="普通帖子" :value="0" />
                <el-option label="特殊帖子" :value="1" />
                <el-option label="公告" :value="2" />
              </el-select>
              <el-select
                v-model="filterByReport"
                placeholder="按举报数筛选"
                clearable
                style="width: 140px; margin-right: 10px;"
                @change="handleFilterChange"
              >
                <el-option label="全部" :value="null" />
                <el-option label="有举报(≥1)" :value="1" />
                <el-option label="举报较多(≥5)" :value="5" />
                <el-option label="举报很多(≥10)" :value="10" />
                <el-option label="举报很多(≥20)" :value="20" />
                <el-option label="举报很多(≥50)" :value="50" />
              </el-select>
              <el-button
                type="primary"
                :icon="Refresh"
                @click="refreshList"
              >
                刷新
              </el-button>
              <el-tooltip
                content="前端筛选模式：获取所有数据后在本地筛选"
                placement="top"
              >
                <el-tag type="info" style="margin-left: 10px;">前端筛选</el-tag>
              </el-tooltip>
            </div>
          </div>
        </template>

        <!-- 帖子列表表格 -->
        <el-table
          v-loading="loading"
          :data="displayedPosts"
          style="width: 100%"
          @row-click="handleRowClick"
        >
          <el-table-column prop="id" label="ID" width="80" />
          <el-table-column label="标题" min-width="200">
            <template #default="{ row }">
              <div class="post-title-cell">
                <div class="title-text">{{ row.post_title || row.title || '无标题' }}</div>
                <div class="content-preview">
                  {{ row.excerpt || row.content?.substring(0, 50) + '...' || '无内容' }}
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="作者" width="150">
            <template #default="{ row }">
              <div class="author-cell">
                <el-avatar
                  v-if="row.author?.avatar"
                  :src="getAvatarUrl(row.author.avatar)"
                  size="small"
                />
                <el-avatar v-else size="small">{{ row.author?.username?.charAt(0) }}</el-avatar>
                <span style="margin-left: 8px">{{ row.author?.username || '未知用户' }}</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column prop="post_type" label="类型" width="100">
            <template #default="{ row }">
              <el-tag :type="getPostTypeTag(row.post_type)" size="small">
                {{ getPostTypeText(row.post_type) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="report" label="举报数" width="100" sortable>
            <template #default="{ row }">
              <el-tag
                :type="getReportCountTag(row.report)"
                size="small"
                effect="dark"
              >
                {{ row.report || 0 }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="getStatusType(row.status)" size="small">
                {{ getStatusText(row.status) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="发布时间" width="180">
            <template #default="{ row }">
              {{ formatDateTime(row.createTime) }}
            </template>
          </el-table-column>
          <el-table-column prop="view_count" label="浏览" width="80" sortable />
          <el-table-column prop="like_count" label="点赞" width="80" sortable />
          <el-table-column prop="comment_count" label="评论" width="80" sortable />
          <el-table-column label="操作" width="220" fixed="right">
            <template #default="{ row }">
              <el-button
                type="primary"
                size="small"
                @click.stop="showPostDetail(row)"
              >
                查看
              </el-button>
              <el-dropdown
                @command="(command) => handleAction(command, row)"
                trigger="click"
              >
                <el-button type="info" size="small">
                  操作<el-icon class="el-icon--right"><ArrowDown /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="approve" v-if="row.status !== 0">
                      <el-icon><Check /></el-icon>通过审核
                    </el-dropdown-item>
                    <el-dropdown-item command="hide" v-if="row.status !== 1">
                      <el-icon><Hide /></el-icon>隐藏帖子
                    </el-dropdown-item>
                    <el-dropdown-item command="ban" v-if="row.status !== 2">
                      <el-icon><Warning /></el-icon>封禁帖子
                    </el-dropdown-item>
                    <el-dropdown-item command="restore" v-if="row.status !== 0">
                      <el-icon><RefreshRight /></el-icon>恢复帖子
                    </el-dropdown-item>
                    <el-dropdown-item command="delete" divided>
                      <el-icon><Delete /></el-icon>删除帖子
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </template>
          </el-table-column>
        </el-table>

        <!-- 加载更多 -->
        <div v-if="hasMore" class="load-more-wrapper">
          <el-button
            type="text"
            :loading="loadingMore"
            @click="loadMore"
            style="width: 100%;"
          >
            {{ loadingMore ? '加载中...' : '加载更多' }}
          </el-button>
        </div>
        
        <div v-else class="no-more-data">
          <el-empty
            v-if="displayedPosts.length > 0"
            description="没有更多数据了"
            :image-size="100"
          />
          <el-empty
            v-else-if="loading"
            description="加载中..."
            :image-size="100"
          />
          <el-empty
            v-else
            description="暂无数据"
            :image-size="100"
          />
        </div>

        <!-- 筛选结果统计 -->
        <div v-if="allPosts.length > 0" class="filter-stats">
          <el-text type="info" size="small">
            共 {{ allPosts.length }} 条数据，筛选后显示 {{ filteredPosts.length }} 条，
            当前显示 {{ displayedPosts.length }} 条
            <span v-if="filteredPosts.length > displayedPosts.length">(还有 {{ filteredPosts.length - displayedPosts.length }} 条未显示)</span>
          </el-text>
        </div>
      </el-card>
    </div>

    <!-- 帖子详情对话框 -->
    <el-dialog
      v-model="detailDialog.visible"
      title="帖子详情"
      width="80%"
      destroy-on-close
    >
      <div v-if="currentPostDetail" class="post-detail-content">
        <div class="post-header">
          <h2>{{ currentPostDetail.post_title }}</h2>
          <div class="post-meta">
            <div class="author-info">
              <el-avatar
                v-if="currentPostDetail.author?.avatar"
                :src="getAvatarUrl(currentPostDetail.author.avatar)"
                size="small"
              />
              <el-avatar v-else size="small">
                {{ currentPostDetail.author?.username?.charAt(0) }}
              </el-avatar>
              <span style="margin-left: 8px">{{ currentPostDetail.author?.username }}</span>
            </div>
            <div class="post-time">
              发布于 {{ formatDateTime(currentPostDetail.createTime) }}
            </div>
            <div class="post-stats">
              <span>浏览 {{ currentPostDetail.view_count }}</span>
              <span>点赞 {{ currentPostDetail.like_count }}</span>
              <span>评论 {{ currentPostDetail.comment_count }}</span>
              <el-tag
                :type="getReportCountTag(currentPostDetail.report)"
                style="margin-left: 8px;"
              >
                举报: {{ currentPostDetail.report || 0 }}
              </el-tag>
              <el-tag
                :type="getStatusType(currentPostDetail.status)"
                style="margin-left: 8px;"
              >
                {{ getStatusText(currentPostDetail.status) }}
              </el-tag>
            </div>
          </div>
        </div>

        <div class="post-content">
          <div class="content-text" v-html="formatContent(currentPostDetail.content)"></div>
          
          <!-- 图片预览 -->
          <div v-if="currentPostDetail.images && currentPostDetail.images.length" class="post-images">
            <h4>图片({{ currentPostDetail.images.length }})</h4>
            <el-scrollbar>
              <div class="image-list">
                <div
                  v-for="(img, index) in currentPostDetail.images"
                  :key="index"
                  class="image-item"
                >
                  <el-image
                    :src="getImageUrl(img)"
                    :preview-src-list="currentPostDetail.images.map(img => getImageUrl(img))"
                    :initial-index="index"
                    style="width: 120px; height: 120px; border-radius: 4px; cursor: pointer;"
                    fit="cover"
                  />
                </div>
              </div>
            </el-scrollbar>
          </div>
        </div>

        <div class="post-actions" style="margin-top: 20px; padding-top: 20px; border-top: 1px solid #ebeef5;">
          <el-button
            type="primary"
            @click="handleDialogAction('approve')"
            v-if="currentPostDetail.status !== 0"
          >
            通过审核
          </el-button>
          <el-button
            type="warning"
            @click="handleDialogAction('hide')"
            v-if="currentPostDetail.status !== 1"
          >
            隐藏帖子
          </el-button>
          <el-button
            type="danger"
            @click="handleDialogAction('ban')"
            v-if="currentPostDetail.status !== 2"
          >
            封禁帖子
          </el-button>
          <el-button
            type="success"
            @click="handleDialogAction('restore')"
            v-if="currentPostDetail.status !== 0"
          >
            恢复帖子
          </el-button>
          <el-button
            type="danger"
            plain
            @click="handleDialogAction('delete')"
          >
            删除帖子
          </el-button>
        </div>
      </div>
    </el-dialog>

    <!-- 操作确认对话框 -->
    <el-dialog
      v-model="confirmDialog.visible"
      :title="confirmDialog.title"
      width="500px"
      @close="closeConfirmDialog"
    >
      <div v-if="['hide', 'ban'].includes(confirmDialog.type)">
        <el-form :model="confirmForm" label-width="80px">
          <el-form-item label="操作原因">
            <el-input
              v-model="confirmForm.reason"
              type="textarea"
              :rows="3"
              placeholder="请输入操作原因（可选）"
            />
          </el-form-item>
        </el-form>
      </div>
      <div v-else>
        <p>{{ confirmDialog.message }}</p>
      </div>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="closeConfirmDialog">取消</el-button>
          <el-button
            type="primary"
            :loading="confirmDialog.loading"
            @click="confirmAction"
          >
            确认
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>
<script setup>
import { ref, reactive, onMounted, computed, watch, nextTick, onUnmounted} from 'vue'
import { 
  Search, 
  Refresh, 
  ArrowDown, 
  Check, 
  Hide, 
  Warning, 
  RefreshRight, 
  Delete,
  Clock,
  Document
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import apiClient from '@/utils/api'

// 所有原始数据
const allPosts = ref([])
// 当前显示的数据
const displayedPosts = ref([])
const loading = ref(false)
const loadingMore = ref(false)
const searchQuery = ref('')
const postType = ref(null)
const filterByReport = ref(10) // 默认显示举报≥10的帖子
const filterByStatus = ref(null) // 状态筛选：null=全部, 0=正常, 1=隐藏, 2=封禁

// 统计信息
const stats = reactive({
  pending: 0,
  total: 0,
  hidden: 0,
  banned: 0,
  reported: 0
})

// 分页
const pagination = reactive({
  lastId: null,
  pageSize: 20,
  hasMore: false,
  currentDisplayCount: 0
})

// 详情对话框
const detailDialog = reactive({
  visible: false
})

// 确认对话框
const confirmDialog = reactive({
  visible: false,
  title: '',
  message: '',
  type: '',
  loading: false,
  data: null
})

// 确认表单
const confirmForm = reactive({
  reason: ''
})

// 当前查看的帖子详情
const currentPostDetail = ref(null)

// 帖子类型映射
const postTypeMap = {
  0: { text: '普通', type: '' },
  1: { text: '特殊', type: 'info' },
  2: { text: '公告', type: 'warning' }
}

// 状态映射
const statusMap = {
  0: { text: '正常', type: 'success' },
  1: { text: '隐藏', type: 'info' },
  2: { text: '封禁', type: 'danger' }
}

// 举报数标签类型
const getReportCountTag = (report) => {
  if (!report) return 'info'
  if (report >= 20) return 'danger'
  if (report >= 10) return 'warning'
  if (report >= 5) return ''
  return 'info'
}

// 获取帖子类型文本
const getPostTypeText = (type) => {
  return postTypeMap[type]?.text || '未知'
}

// 获取帖子类型标签
const getPostTypeTag = (type) => {
  return postTypeMap[type]?.type || 'info'
}

// 获取状态文本
const getStatusText = (status) => {
  return statusMap[status]?.text || '未知'
}

// 获取状态类型
const getStatusType = (status) => {
  return statusMap[status]?.type || 'info'
}

// 获取头像URL
const getAvatarUrl = (avatarPath) => {
  if (!avatarPath) return ''
  if (avatarPath.startsWith('http')) return avatarPath
  return `/uploads/${avatarPath}`
}

// 获取图片URL
const getImageUrl = (imgPath) => {
  if (!imgPath) return ''
  if (imgPath.startsWith('http')) return imgPath
  return imgPath
}

// 格式化时间
const formatDateTime = (dateTime) => {
  if (!dateTime) return ''
  const date = new Date(dateTime)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 格式化内容
const formatContent = (content) => {
  if (!content) return ''
  return content.replace(/\n/g, '<br>')
}

// 获取状态筛选文本
const getStatusFilterText = () => {
  if (filterByStatus.value === null) return '全部状态'
  if (filterByStatus.value === 0) return '正常'
  if (filterByStatus.value === 1) return '隐藏'
  if (filterByStatus.value === 2) return '封禁'
  return '全部状态'
}

// 获取举报筛选文本
const getReportFilterText = () => {
  if (!filterByReport.value) return '全部'
  if (filterByReport.value === 1) return '有举报(≥1)'
  if (filterByReport.value === 5) return '举报较多(≥5)'
  if (filterByReport.value === 10) return '举报很多(≥10)'
  if (filterByReport.value === 20) return '举报很多(≥20)'
  if (filterByReport.value === 50) return '举报很多(≥50)'
  return '全部'
}

// 获取帖子类型筛选文本
const getPostTypeFilterText = () => {
  if (postType.value === null) return '全部类型'
  if (postType.value === 0) return '普通帖子'
  if (postType.value === 1) return '特殊帖子'
  if (postType.value === 2) return '公告'
  return '全部类型'
}

// 计算属性：筛选后的帖子
const filteredPosts = computed(() => {
  if (allPosts.value.length === 0) return []
  
  let filtered = [...allPosts.value]
  
  // 1. 按帖子类型筛选
  if (postType.value !== null && postType.value !== undefined) {
    filtered = filtered.filter(post => post.post_type === postType.value)
  }
  
  // 2. 按举报数筛选
  if (filterByReport.value !== null && filterByReport.value !== undefined && filterByReport.value > 0) {
    const minReport = filterByReport.value
    filtered = filtered.filter(post => (post.report || 0) >= minReport)
  }
  
  // 3. 按状态筛选
  if (filterByStatus.value !== null && filterByStatus.value !== undefined) {
    filtered = filtered.filter(post => post.status === filterByStatus.value)
  }
  
  // 4. 按搜索词筛选
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase().trim()
    filtered = filtered.filter(post => {
      return (
        (post.post_title?.toLowerCase().includes(query) || false) ||
        (post.content?.toLowerCase().includes(query) || false) ||
        (post.author?.username?.toLowerCase().includes(query) || false)
      )
    })
  }
  
  console.log('🔍 筛选结果:', {
    原数据: allPosts.value.length,
    类型筛选: postType.value,
    举报筛选: filterByReport.value,
    状态筛选: filterByStatus.value,
    搜索词: searchQuery.value,
    筛选后: filtered.length
  })
  
  return filtered
})

// 计算是否还有更多数据
const hasMore = computed(() => {
  return displayedPosts.value.length < filteredPosts.value.length
})

// 更新显示的数据
const updateDisplayedPosts = () => {
  console.log('🔄 更新显示数据')
  const endIndex = Math.min(
    pagination.currentDisplayCount + pagination.pageSize,
    filteredPosts.value.length
  )
  displayedPosts.value = filteredPosts.value.slice(0, endIndex)
  pagination.currentDisplayCount = displayedPosts.value.length
  console.log(`📄 显示 ${displayedPosts.value.length}/${filteredPosts.value.length} 条数据`)
}

// 搜索帖子
const handleSearch = () => {
  console.log('🔍 搜索关键词：', searchQuery.value)
  // 重置显示的数据
  displayedPosts.value = []
  pagination.currentDisplayCount = 0
  // 重新计算显示的数据
  updateDisplayedPosts()
  // 计算统计数据
  calculateStats()
}

// 处理筛选条件变化
const handleFilterChange = () => {
  console.log('🔍 筛选条件变化：', {
    postType: getPostTypeFilterText(),
    filterByReport: getReportFilterText(),
    filterByStatus: getStatusFilterText(),
    searchQuery: searchQuery.value
  })
  // 重置显示的数据
  displayedPosts.value = []
  pagination.currentDisplayCount = 0
  // 重新计算显示的数据
  updateDisplayedPosts()
  // 计算统计数据
  calculateStats()
}

// 获取所有帖子（包括所有状态）
const fetchAllPosts = async () => {
  loading.value = true
  allPosts.value = []  // 清空现有数据
  displayedPosts.value = []  // 清空显示数据
  pagination.lastId = null
  pagination.hasMore = false
  
  try {
    const allData = []
    let hasMoreData = true
    let lastId = null
    let page = 1
    
    while (hasMoreData) {
      console.log(`📥 正在获取第 ${page} 页数据...`)
      
      const params = {
        pageSize: 50
      }
      
      if (lastId) {
        params.lastId = lastId
      }
      
      console.log('API请求参数：', params)
      
      // 使用审核接口获取所有帖子
      const response = await apiClient.get('/posts/review', { params })
      console.log('API响应：', response.data)
      
      if (response.data?.success) {
        const data = response.data.data || []
        console.log(`✅ 第 ${page} 页获取 ${data.length} 条数据`)
        
        if (data.length > 0) {
          allData.push(...data)
          lastId = data[data.length - 1].id
          console.log(`最后ID：${lastId}`)
          
          // 检查是否还有更多数据
          hasMoreData = response.data.pagination?.hasMore || false
          page++
        } else {
          hasMoreData = false
        }
      } else {
        console.error('获取数据失败：', response.data?.message)
        hasMoreData = false
      }
      
      // 防止无限循环
      if (page > 20) {
        console.warn('⚠️ 达到最大获取页数限制，停止获取')
        break
      }
    }
    
    console.log(`🎉 总共获取 ${allData.length} 条帖子`)
    allPosts.value = allData
    
    // 获取统计信息
    await fetchStats()
    
    // 更新显示的数据
    updateDisplayedPosts()
    
  } catch (error) {
    console.error('获取帖子失败:', error)
    ElMessage.error('获取帖子列表失败：' + error.message)
  } finally {
    loading.value = false
  }
}

// 获取统计信息
const fetchStats = async () => {
  try {
    const response = await apiClient.get('/posts/review/stats')
    if (response.data?.success) {
      const data = response.data.data
      stats.total = data.total || 0
      stats.pending = data.pending || 0
      stats.hidden = data.hidden || 0
      stats.banned = data.banned || 0
      stats.reported = data.reported || 0
      console.log('📊 统计数据：', { ...stats })
    } else {
      console.error('获取统计信息失败：', response.data?.message)
      // 如果获取统计信息失败，从当前数据计算
      calculateStatsFromLocalData()
    }
  } catch (error) {
    console.error('获取统计信息失败:', error)
    // 如果获取统计信息失败，从当前数据计算
    calculateStatsFromLocalData()
  }
}

// 从本地数据计算统计信息
const calculateStatsFromLocalData = () => {
  console.log('📊 从本地数据计算统计信息')
  if (allPosts.value.length === 0) {
    stats.total = 0
    stats.pending = 0
    stats.hidden = 0
    stats.banned = 0
    stats.reported = 0
    return
  }
  
  stats.total = allPosts.value.length
  stats.pending = allPosts.value.filter(post => post.status === 0).length
  stats.hidden = allPosts.value.filter(post => post.status === 1).length
  stats.banned = allPosts.value.filter(post => post.status === 2).length
  stats.reported = allPosts.value.filter(post => (post.report || 0) >= 10).length
  
  console.log('📊 本地统计：', { ...stats })
}

// 计算统计数据
const calculateStats = () => {
  calculateStatsFromLocalData()
}

// 加载更多
const loadMore = () => {
  console.log('📥 加载更多数据')
  if (loadingMore.value || !hasMore.value) return
  
  loadingMore.value = true
  
  // 模拟异步加载
  setTimeout(() => {
    updateDisplayedPosts()
    loadingMore.value = false
  }, 300)
}

// 刷新列表
const refreshList = async () => {
  console.log('🔄 刷新列表')
  displayedPosts.value = []  // 清空显示数据
  allPosts.value = []       // 清空所有数据
  await fetchAllPosts()
}

// 显示帖子详情
const showPostDetail = (post) => {
  console.log('👀 查看帖子详情：', post.id)
  currentPostDetail.value = { ...post }
  detailDialog.visible = true
}

// 处理行点击
const handleRowClick = (row) => {
  showPostDetail(row)
}

// 处理对话框操作
const handleDialogAction = (action) => {
  if (!currentPostDetail.value) return
  handleAction(action, currentPostDetail.value)
}

// 处理操作
const handleAction = (command, row) => {
  confirmDialog.data = row
  
  switch (command) {
    case 'approve':
      confirmDialog.title = '通过审核'
      confirmDialog.message = `确定要通过帖子"${row.post_title || row.title || '无标题'}"的审核吗？`
      confirmDialog.type = 'approve'
      break
    case 'hide':
      confirmDialog.title = '隐藏帖子'
      confirmDialog.message = '确定要隐藏此帖子吗？'
      confirmDialog.type = 'hide'
      break
    case 'ban':
      confirmDialog.title = '封禁帖子'
      confirmDialog.message = '确定要封禁此帖子吗？'
      confirmDialog.type = 'ban'
      break
    case 'restore':
      confirmDialog.title = '恢复帖子'
      confirmDialog.message = `确定要恢复帖子"${row.post_title || row.title || '无标题'}"吗？`
      confirmDialog.type = 'restore'
      break
    case 'delete':
      confirmDialog.title = '删除帖子'
      confirmDialog.message = `确定要删除帖子"${row.post_title || row.title || '无标题'}"吗？此操作不可恢复！`
      confirmDialog.type = 'delete'
      break
  }
  
  confirmDialog.visible = true
  confirmForm.reason = ''
  console.log('操作对话框：', confirmDialog)
}

// 确认操作
const confirmAction = async () => {
  confirmDialog.loading = true
  try {
    const { id: postId, status: currentStatus } = confirmDialog.data
    
    let success = false
    switch (confirmDialog.type) {
      case 'approve':
        success = await updatePostStatus(postId, 0, confirmForm.reason)
        break
      case 'hide':
        success = await updatePostStatus(postId, 1, confirmForm.reason)
        break
      case 'ban':
        success = await updatePostStatus(postId, 2, confirmForm.reason)
        break
      case 'restore':
        success = await updatePostStatus(postId, 0, confirmForm.reason)
        break
      case 'delete':
        success = await deletePost(postId)
        break
    }
    
    if (success) {
      ElMessage.success('操作成功')
      closeConfirmDialog()
      // 重新获取数据
      await refreshList()
    }
  } catch (error) {
    console.error('操作失败:', error)
    ElMessage.error('操作失败：' + error.message)
  } finally {
    confirmDialog.loading = false
  }
}

// 更新帖子状态
const updatePostStatus = async (postId, status, reason) => {
  try {
    const params = { status }
    if (reason) params.reason = reason
    console.log('更新帖子状态：', { postId, params })
    
    const response = await apiClient.put(`/posts/${postId}/status`, null, { params })
    
    if (response.data?.success) {
      console.log('✅ 更新帖子状态成功：', response.data.message)
      
      // 更新本地数据
      const postIndex = allPosts.value.findIndex(post => post.id === postId)
      if (postIndex !== -1) {
        allPosts.value[postIndex].status = status
        console.log('✅ 更新本地数据成功')
      }
      
      const displayedPostIndex = displayedPosts.value.findIndex(post => post.id === postId)
      if (displayedPostIndex !== -1) {
        displayedPosts.value[displayedPostIndex].status = status
      }
      
      // 重新计算统计
      calculateStats()
      
      // 重新计算显示的数据
      updateDisplayedPosts()
      
      return true
    } else {
      console.error('❌ 更新帖子状态失败：', response.data?.message)
      throw new Error(response.data?.message || '更新状态失败')
    }
  } catch (error) {
    console.error('更新帖子状态失败:', error)
    throw error
  }
}

// 删除帖子
const deletePost = async (postId) => {
  try {
    console.log('删除帖子：', postId)
    const response = await apiClient.delete(`/posts/${postId}`)
    
    if (response.data?.success) {
      console.log('✅ 删除帖子成功')
      
      // 从本地数据中删除
      allPosts.value = allPosts.value.filter(post => post.id !== postId)
      displayedPosts.value = displayedPosts.value.filter(post => post.id !== postId)
      
      // 重新计算统计
      calculateStats()
      
      return true
    } else {
      console.error('❌ 删除帖子失败：', response.data?.message)
      throw new Error(response.data?.message || '删除失败')
    }
  } catch (error) {
    console.error('删除帖子失败:', error)
    throw error
  }
}

// 关闭确认对话框
const closeConfirmDialog = () => {
  confirmDialog.visible = false
  confirmDialog.loading = false
  confirmDialog.data = null
  confirmDialog.type = ''
  confirmForm.reason = ''
  console.log('关闭确认对话框')
}

// 处理状态变化
const handleStatusChanged = () => {
  console.log('帖子状态发生变化，刷新列表')
  refreshList()
  detailDialog.visible = false
}

// 重置筛选条件
const resetFilters = () => {
  console.log('🔄 重置筛选条件')
  postType.value = null
  filterByReport.value = 10
  filterByStatus.value = null
  searchQuery.value = ''
  // 重置显示的数据
  displayedPosts.value = []
  pagination.currentDisplayCount = 0
  // 重新计算显示的数据
  updateDisplayedPosts()
  calculateStats()
}

// 处理状态筛选
const handleStatusFilter = (status) => {
  filterByStatus.value = status
  handleFilterChange()
}

// 处理举报筛选
const handleReportFilter = (minReport) => {
  filterByReport.value = minReport
  handleFilterChange()
}

// 处理类型筛选
const handleTypeFilter = (type) => {
  postType.value = type
  handleFilterChange()
}

// 监听筛选条件变化
watch([postType, filterByReport, filterByStatus, searchQuery], () => {
  // 防抖处理
  clearTimeout(window.filterTimeout)
  window.filterTimeout = setTimeout(() => {
    handleFilterChange()
  }, 300)
})

// 监听筛选后的数据变化
watch(filteredPosts, () => {
  console.log('🔍 筛选后的数据变化，数量：', filteredPosts.value.length)
  // 重置显示的数据
  displayedPosts.value = []
  pagination.currentDisplayCount = 0
  updateDisplayedPosts()  // 重新计算显示的数据
  calculateStats()       // 重新计算统计
})

// 获取当前筛选统计
const getFilterStats = computed(() => {
  return {
    total: allPosts.value.length,
    filtered: filteredPosts.value.length,
    displayed: displayedPosts.value.length,
    remaining: Math.max(0, filteredPosts.value.length - displayedPosts.value.length)
  }
})

// 初始化
onMounted(async () => {
  console.log('🚀 组件初始化')
  await fetchAllPosts()
  
  // 添加键盘快捷键
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && detailDialog.visible) {
      detailDialog.visible = false
    }
  })
})

// 组件卸载时清理
onUnmounted(() => {
  clearTimeout(window.filterTimeout)
  document.removeEventListener('keydown', () => {})
})

// 调试功能
const showDebugInfo = () => {
  console.log('=== 调试信息 ===')
  console.log('📊 数据统计：', {
    所有数据: allPosts.value.length,
    筛选后: filteredPosts.value.length,
    显示中: displayedPosts.value.length,
    状态筛选: filterByStatus.value,
    举报筛选: filterByReport.value,
    类型筛选: postType.value,
    搜索词: searchQuery.value
  })
  console.log('🔧 筛选条件：', {
    postType: getPostTypeFilterText(),
    filterByReport: getReportFilterText(),
    filterByStatus: getStatusFilterText(),
    searchQuery: searchQuery.value
  })
  console.log('📁 当前显示数据：', displayedPosts.value)
  console.log('================')
}
</script>

<style scoped>
.post-review-container {
  padding: 20px;
  background: #f5f7fa;
  min-height: calc(100vh - 60px);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header-left .page-title {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
  margin: 0 0 8px 0;
}

.header-left .page-subtitle {
  font-size: 14px;
  color: #909399;
  margin: 0;
}

.search-box {
  width: 300px;
}

.stats-cards {
  margin-bottom: 24px;
}

.stat-card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  display: flex;
  align-items: center;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  transition: transform 0.3s;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16px;
  font-size: 24px;
  color: white;
}

.stat-icon.pending {
  background: linear-gradient(135deg, #ffd666 0%, #faad14 100%);
}

.stat-icon.total {
  background: linear-gradient(135deg, #b37feb 0%, #722ed1 100%);
}

.stat-icon.hidden {
  background: linear-gradient(135deg, #87e8de 0%, #13c2c2 100%);
}

.stat-icon.banned {
  background: linear-gradient(135deg, #ffa39e 0%, #f5222d 100%);
}

.stat-content .stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
  line-height: 1;
  margin-bottom: 4px;
}

.stat-content .stat-label {
  font-size: 13px;
  color: #909399;
}

.review-content {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
}

.card-actions {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.post-title-cell {
  padding: 4px 0;
}

.post-title-cell .title-text {
  font-weight: 500;
  color: #303133;
  margin-bottom: 4px;
  font-size: 14px;
}

.post-title-cell .content-preview {
  font-size: 12px;
  color: #909399;
  line-height: 1.4;
  word-break: break-all;
}

.author-cell {
  display: flex;
  align-items: center;
}

.load-more-wrapper {
  padding: 20px 0;
  text-align: center;
  border-top: 1px solid #ebeef5;
  margin-top: 20px;
}

.no-more-data {
  padding: 40px 0;
  text-align: center;
  color: #909399;
  font-size: 14px;
}

.filter-stats {
  padding: 10px 0;
  border-top: 1px solid #ebeef5;
  margin-top: 20px;
  display: flex;
  justify-content: center;
  background-color: #f8f9fa;
  border-radius: 4px;
}

/* 帖子详情对话框样式 */
.post-detail-content {
  max-height: 70vh;
  overflow-y: auto;
  padding-right: 10px;
}

.post-detail-content .post-header {
  margin-bottom: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid #ebeef5;
}

.post-detail-content .post-header h2 {
  margin: 0 0 16px 0;
  color: #303133;
  font-size: 24px;
  line-height: 1.5;
}

.post-detail-content .post-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 20px;
  color: #606266;
  font-size: 14px;
}

.post-detail-content .author-info {
  display: flex;
  align-items: center;
}

.post-detail-content .post-time {
  color: #909399;
}

.post-detail-content .post-stats {
  display: flex;
  align-items: center;
  gap: 20px;
  color: #606266;
  font-size: 14px;
}

.post-detail-content .post-stats span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.post-detail-content .post-content {
  margin-bottom: 20px;
}

.post-detail-content .content-text {
  line-height: 1.8;
  color: #303133;
  font-size: 16px;
  white-space: pre-wrap;
  word-break: break-word;
  margin-bottom: 20px;
}

.post-detail-content .post-images {
  margin-top: 20px;
  border-top: 1px solid #ebeef5;
  padding-top: 20px;
}

.post-detail-content .post-images h4 {
  margin: 0 0 12px 0;
  color: #303133;
  font-size: 16px;
}

.post-detail-content .image-list {
  display: flex;
  gap: 12px;
  padding: 8px 0;
  flex-wrap: wrap;
}

.post-detail-content .image-item {
  cursor: pointer;
  transition: transform 0.3s;
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #ebeef5;
}

.post-detail-content .image-item:hover {
  transform: scale(1.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .review-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .header-left {
    margin-bottom: 16px;
  }
  
  .search-box {
    width: 100%;
  }
  
  .card-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .card-actions {
    margin-top: 12px;
    justify-content: space-between;
  }
  
  .card-actions .el-select {
    width: 100%;
    margin-bottom: 10px;
  }
  
  .post-detail-content .post-meta {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
}
</style>