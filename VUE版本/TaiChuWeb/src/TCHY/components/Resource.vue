<template>
  <div class="resource-container">
    <!-- 用户信息区域 -->
    <div class="user-section">
      <el-card class="user-card">
        <div class="user-info">
          <div v-if="loading" class="loading-state">
            <el-skeleton :rows="2" animated />
          </div>
          <div v-else-if="error" class="error-state">
            <el-alert type="error" :title="error" show-icon />
            <el-button type="primary" size="small" @click="fetchCurrentUserData">重新加载</el-button>
          </div>
          <div v-else>
            <div class="username">你好，{{ userData.username || "用户" }}</div>
            <div class="user-stats">
              <span>等级：Lv.{{ userData.level || 1 }}</span>
              <span>积分：{{ userData.points || 0 }}</span>
              <span v-if="userData.exp">经验：{{ userData.exp }}</span>
            </div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 主内容区域 -->
    <div class="main-content">
      <el-card class="content-card">
        <template #header>
          <div class="card-header">
            <div class="header-left">
              <span class="content-title">资源中心</span>
              <div class="category-tabs">
                <el-radio-group v-model="activeCategory" @change="handleCategoryChange" size="small">
                  <el-radio-button label="all">全部</el-radio-button>
                  <el-radio-button
                    v-for="category in categories"
                    :key="category.id"
                    :label="category.id"
                  >
                    {{ category.name }}
                  </el-radio-button>
                </el-radio-group>
              </div>
            </div>
            <div class="header-right">
              <el-button
                type="primary"
                :loading="resourceLoading"
                @click="refreshResources"
                size="small"
              >
                刷新资源
              </el-button>
              <el-button
                size="small"
                @click="refreshCategories"
              >
                刷新分类
              </el-button>
            </div>
          </div>
        </template>

        <!-- 卡片内容：资源列表 -->
        <div class="card-content">
          <div v-if="categories.length === 0 && !resourceLoading" class="empty-state">
            <el-empty description="正在加载分类数据..." />
          </div>

          <div v-else-if="resourceLoading" class="loading-state">
            <el-skeleton :rows="6" animated />
          </div>

          <div v-else-if="resources.length === 0" class="empty-state">
            <el-empty :description="`暂无${getCategoryName(activeCategory)}资源`" />
          </div>

          <div v-else class="resource-grid">
            <div
              class="resource-item"
              v-for="resource in resources"
              :key="resource.id"
            >
              <div class="resource-info">
                <h4 class="resource-title">
                  <i class="el-icon-document"></i>
                  {{ resource.title || '未命名资源' }}
                </h4>

                <div class="resource-desc-container" v-if="resource.description">
                  <p
                    class="resource-desc"
                    :class="{ 'desc-collapsed': !resource.expanded && isDescriptionLong(resource.description) }"
                  >
                    {{ resource.description }}
                  </p>
                  <el-button
                    v-if="isDescriptionLong(resource.description)"
                    type="text"
                    size="small"
                    @click.stop="toggleDescription(resource)"
                    class="desc-toggle-btn"
                  >
                    {{ resource.expanded ? '收起' : '展开' }}
                    <i :class="resource.expanded ? 'el-icon-arrow-up' : 'el-icon-arrow-down'"></i>
                  </el-button>
                </div>

                <div class="resource-category">
                  <el-tag size="small" type="info">
                    <i class="el-icon-collection-tag"></i>
                    {{ resource.categoryName }}
                  </el-tag>
                </div>

                <div class="resource-requirements">
                  <el-tag v-if="resource.levelRequired > 0" size="small" type="warning">
                    <i class="el-icon-arrow-up"></i>
                    需要等级: {{ resource.levelRequired }}
                  </el-tag>
                  <el-tag v-if="resource.pointsRequired > 0" size="small" type="danger">
                    <i class="el-icon-coin"></i>
                    需要积分: {{ resource.pointsRequired }}
                  </el-tag>
                </div>

                <div class="resource-meta">
                  <div class="meta-row">
                    <span class="meta-item">
                      <i class="el-icon-time"></i>
                      {{ formatDate(resource.createTime) }}
                    </span>
                    <span class="meta-item">
                      <i class="el-icon-download"></i>
                      下载次数: {{ resource.downloadCount || 0 }}
                    </span>
                  </div>
                </div>

                <div class="resource-actions">
                  <el-button
                    :type="getDownloadButtonType(resource)"
                    :disabled="!canDownload(resource)"
                    size="small"
                    @click.stop="openDeclareDialog(resource)"
                    :loading="resource.downloading"
                  >
                    <i class="el-icon-download"></i>
                    {{ getDownloadButtonText(resource) }}
                  </el-button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <template #footer>
          <div class="card-footer">
            <div class="footer-info">
              <span>当前分类: <strong>{{ getCategoryName(activeCategory) }}</strong></span>
              <span>共 <strong class="count">{{ categories.length }}</strong> 个分类</span>
              <span>共 <strong class="count">{{ resources.length }}</strong> 个资源</span>
            </div>
          </div>
        </template>
      </el-card>
    </div>

    <!-- 免责声明弹窗 -->
    <el-dialog
      v-model="showAgreeDialog"
      title="下载声明"
      width="600px"
      :close-on-click-modal="false"
      :show-close="true"
    >
      <div class="download-declare-content">
        <p><strong>请在下载前仔细阅读以下声明：</strong></p>
        <p>1. 本网站提供的软件或资源仅供学习、研究与交流使用，禁止用于商业用途或其他违法用途。</p>
        <p>2. 对于资源内容的准确性、完整性或合法性，我们不作任何保证。资源可能包含第三方软件、资料或版权内容，如涉及侵权，我们将在接到通知后及时处理并下架相关资源。</p>
        <p>3. 下载并使用资源可能带来安全风险（例如病毒、木马、恶意代码等），请在可信环境中进行扫描与测试。对于因下载或使用资源导致的任何损失或法律责任，本网站及作者不承担责任。</p>
        <p>4. 请确保您具备相应的使用权限与授权，如因使用资源导致的任何纠纷或法律责任，您需自行承担。</p>
      </div>

      <div style="margin-top:12px; display:flex; align-items:center; gap:8px;">
        <el-checkbox v-model="agreeChecked">我已阅读并同意上述声明</el-checkbox>
      </div>

      <template #footer>
        <div style="text-align:right">
          <el-button @click="cancelDeclare">取消</el-button>
          <el-button
            type="primary"
            :disabled="!agreeChecked || downloadInProgress"
            :loading="downloadInProgress"
            @click="confirmAgreeAndDownload"
          >
            同意并下载
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox, ElNotification } from 'element-plus'
import { getCurrentUserData, deductPoints } from "../../../server/UserInfoApi"
import request from "../../../server/UserInfoApi"

// 状态
const loading = ref(false)
const error = ref('')
const userData = ref({})
const categories = ref([])
const resources = ref([])
const activeCategory = ref('all')
const resourceLoading = ref(false)

// 声明弹窗相关
const showAgreeDialog = ref(false)
const agreeChecked = ref(false)
const pendingDownloadResource = ref(null)
const downloadInProgress = ref(false)

// 简化的辅助函数
const isDescriptionLong = (description) => description && description.length > 100
const toggleDescription = (resource) => { resource.expanded = !resource.expanded }
const formatDate = (dateString) => new Date(dateString).toLocaleDateString()
const getCategoryName = (categoryId) => {
  if (categoryId === 'all') return '全部'
  const category = categories.value.find(cat => cat.id === categoryId)
  return category ? category.name : '未知'
}

// 下载权限判断与按钮文本
const canDownload = (resource) => {
  const userLevel = userData.value.level || 0
  const userPoints = userData.value.points || 0
  return userLevel >= resource.levelRequired && userPoints >= resource.pointsRequired
}

const getDownloadButtonText = (resource) => {
  const userLevel = userData.value.level || 0
  const userPoints = userData.value.points || 0
  if (userLevel < resource.levelRequired) return `需要等级 ${resource.levelRequired}`
  if (userPoints < resource.pointsRequired) return `需要积分 ${resource.pointsRequired}`
  return '下载资源'
}

const getDownloadButtonType = (resource) => canDownload(resource) ? 'primary' : 'info'

// 点击下载：先打开声明弹窗
const openDeclareDialog = (resource) => {
  if (!canDownload(resource)) {
    ElMessage.warning('不满足下载条件')
    return
  }
  pendingDownloadResource.value = resource
  agreeChecked.value = false
  showAgreeDialog.value = true
}

// 取消声明
const cancelDeclare = () => {
  showAgreeDialog.value = false
  agreeChecked.value = false
  pendingDownloadResource.value = null
}

// 确认声明并下载
// 替换 confirmAgreeAndDownload（签名直链 + 原生下载）
const confirmAgreeAndDownload = async () => {
  const resource = pendingDownloadResource.value
  if (!resource) {
    ElMessage.error('未找到要下载的资源')
    showAgreeDialog.value = false
    return
  }

  const amount = Number(resource.pointsRequired || 0)
  const userPoints = Number(userData.value.points || 0)
  if (amount > 0 && userPoints < amount) {
    ElMessage.warning('积分不足，无法下载')
    showAgreeDialog.value = false
    return
  }

  downloadInProgress.value = true
  resource.downloading = true

  try {
    // 保留你的扣分逻辑
    if (amount > 0) {
      const deductResp = await deductPoints(amount)
      const remaining = deductResp?.remaining ?? deductResp?.remainingPoints ?? deductResp?.remainingBalance ?? null
      if (remaining !== null && !Number.isNaN(Number(remaining))) {
        userData.value.points = Number(remaining)
      } else {
        try { userData.value = await getCurrentUserData() } catch {}
      }
    }

    // 1) 获取短期签名直链（接口会带 JWT）
    const { data } = await request.get('/api/Resource/signed-url', { params: { resourceId: resource.id } })
    const signedUrl = data?.url
    if (!signedUrl) throw new Error('获取直链失败')

    console.log('[下载] 签名直链:', signedUrl)

    // 2) 关闭弹窗与 loading
    showAgreeDialog.value = false
    downloadInProgress.value = false
    resource.downloading = false

    // 3) 让浏览器原生下载（并行 range，速度更快）
    window.open(signedUrl, '_blank')

    // 本地计数（可选）
    resource.downloadCount = (resource.downloadCount || 0) + 1
    ElNotification.success({ title: '开始下载', message: `资源 "${resource.title}" 开始下载`, duration: 2000 })
  } catch (err) {
    const msg = err?.response?.data?.message || err?.message || String(err)
    ElMessage.error('下载失败: ' + msg)
  } finally {
    agreeChecked.value = false
    pendingDownloadResource.value = null
    downloadInProgress.value = false
    resource.downloading = false
  }
}

// 数据获取
const handleCategoryChange = (categoryId) => fetchResources(categoryId)

const fetchCurrentUserData = async () => {
  loading.value = true
  error.value = ''
  try {
    const data = await getCurrentUserData()
    userData.value = data
  } catch (err) {
    error.value = err.message || '获取用户信息失败'
  } finally {
    loading.value = false
  }
}

const fetchCategories = async () => {
  try {
    const response = await request.get('/api/Resource/categories')
    categories.value = response.data || []
    if (categories.value.length > 0) {
      activeCategory.value = categories.value[0].id
      fetchResources(activeCategory.value)
    } else {
      fetchResources('all')
    }
  } catch (err) {
    console.error('获取分类失败:', err)
    ElMessage.error('获取分类失败: ' + (err.response?.data?.message || err.message))
  }
}

const fetchResources = async (category = 'all') => {
  resourceLoading.value = true
  resources.value = []
  try {
    const params = {}
    if (category && category !== 'all') params.categoryId = category
    const response = await request.get('/api/Resource', { params })
    resources.value = (response.data || []).map(r => ({ ...r, expanded: false, downloading: false }))
  } catch (err) {
    console.error('获取资源失败:', err)
    ElMessage.error('获取资源失败: ' + (err.response?.data?.message || err.message))
  } finally {
    resourceLoading.value = false
  }
}

const refreshCategories = async () => {
  await fetchCategories()
  ElMessage.success('分类数据已刷新')
}

const refreshResources = async () => {
  await fetchResources(activeCategory.value)
  ElMessage.success('资源数据已刷新')
}

// 生命周期
onMounted(() => {
  fetchCurrentUserData()
  fetchCategories()
})
</script>

<style scoped>
.resource-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  min-height: 100vh;
}

/* 保留原有样式（略）——只包含必要样式以确保页面正常显示 */
.user-section { margin-bottom: 24px; }
.main-content { margin-bottom: 24px; }
.content-card { border-radius: 12px; box-shadow: 0 4px 16px rgba(0,0,0,0.1); }
.card-header { display:flex; justify-content:space-between; align-items:center; gap:16px; }
.header-left { display:flex; align-items:center; gap:24px; }
.content-title { font-size:20px; font-weight:600; color:#303133; }
.category-tabs { display:flex; align-items:center; }
.header-right { display:flex; gap:8px; }
.card-content { min-height:400px; }
.resource-grid { display:grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap:16px; padding:8px 0; }
.resource-item { border:1px solid #e8e8e8; border-radius:8px; padding:16px; background:#fff; }
.resource-item:hover { border-color:#409eff; box-shadow:0 2px 8px rgba(64,158,255,0.15); background:#f5faff; }
.resource-info { display:flex; flex-direction:column; gap:12px; }
.resource-title { margin:0; color:#303133; font-size:16px; font-weight:600; display:flex; gap:8px; align-items:center; }
.resource-desc-container { margin:4px 0; }
.resource-desc { margin:0; color:#606266; font-size:14px; line-height:1.5; }
.desc-collapsed { display:-webkit-box; -webkit-line-clamp:2; -webkit-box-orient:vertical; overflow:hidden; }
.desc-toggle-btn { margin-top:4px; padding:0; height:auto; font-size:12px; }
.resource-category { margin:4px 0; }
.resource-requirements { display:flex; gap:8px; margin:4px 0; flex-wrap:wrap; }
.resource-meta { margin:4px 0; }
.meta-row { display:flex; justify-content:space-between; align-items:center; }
.meta-item { display:flex; align-items:center; gap:4px; font-size:12px; color:#909399; }
.resource-actions { margin-top:12px; text-align:center; }
.resource-actions .el-button { width:100%; }
.card-footer { display:flex; justify-content:space-between; align-items:center; gap:16px; }
.footer-info { display:flex; gap:16px; align-items:center; }
.count { color:#409eff; font-weight:600; }
.loading-state { padding:40px 0; }
.empty-state { padding:80px 0; }
.user-info { display:flex; align-items:center; gap:16px; }
.username { font-size:18px; font-weight:600; color:#1976d2; }
.user-stats { display:flex; gap:12px; }
.user-stats span { padding:6px 12px; background:#f0f7ff; border-radius:6px; color:#409eff; font-weight:500; }
.download-declare-content { max-height:320px; overflow:auto; padding-right:8px; }
.download-declare-content p { margin:8px 0; line-height:1.6; color:#333; font-size:13px; }
@media (max-width:768px) {
  .resource-grid { grid-template-columns: 1fr; gap:12px; }
  .card-header { flex-direction:column; align-items:stretch; }
}
</style>