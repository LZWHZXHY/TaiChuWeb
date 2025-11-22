<template>
  <section class="card" aria-labelledby="roster-title">
    <header class="card-head">
      <h3 id="roster-title">3. 人设列表</h3>
      <div class="tools">
        <div class="search-box">
          <input 
            v-model="searchQuery" 
            class="input search-input" 
            type="search" 
            placeholder="搜索角色名称、作者、种族、能力..." 
            @input="handleSearch"
          />
          <span class="search-icon">🔍</span>
        </div>
        <div class="filter-group">
          <select v-model="selectedSpecies" class="select" @change="handleFilter">
            <option value="">全部种族</option>
            <option v-for="species in speciesOptions" :key="species" :value="species">
              {{ species }}
            </option>
          </select>
          <select v-model="selectedTier" class="select" @change="handleFilter">
            <option value="">全部强度</option>
            <option v-for="tier in tierOptions" :key="tier" :value="tier">
              {{ tier }}
            </option>
          </select>
        </div>
        <button class="btn ghost" @click="refreshList" :disabled="isLoading">
          <span v-if="isLoading" class="loading-spinner"></span>
          {{ isLoading ? '加载中...' : '刷新' }}
        </button>
        <button class="btn ghost" @click="debugData" title="调试数据">
          🐛 调试
        </button>
      </div>
    </header>

    <!-- 统计信息 -->
    <div class="stats-bar">
      <div class="stat-item">
        <span class="stat-label">总计</span>
        <span class="stat-value">{{ totalCount }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">显示</span>
        <span class="stat-value">{{ displayedCount }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">页数</span>
        <span class="stat-value">{{ currentPage }}/{{ totalPages }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">有图片</span>
        <span class="stat-value">{{ imageCount }}</span>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="isLoading" class="loading-state">
      <div class="loading-content">
        <div class="loading-spinner large"></div>
        <p>正在加载OC列表...</p>
      </div>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="errorMessage" class="error-state">
      <div class="error-content">
        <div class="error-icon">❌</div>
        <div class="error-text">
          <h4>加载失败</h4>
          <p>{{ errorMessage }}</p>
        </div>
        <button class="btn primary" @click="refreshList">重试</button>
      </div>
    </div>

    <!-- 角色卡片网格 -->
    <div v-else-if="paginatedOCs.length > 0" class="cards-grid">
      <div 
        v-for="oc in paginatedOCs" 
        :key="oc.id" 
        class="oc-card"
        @click="viewOCDetail(oc.id)"
      >
        <div class="card-image">
          <!-- 新增：实际渲染图片 -->
          <img
            class="avatar"
            :src="resolveImageSrc(oc)"
            :alt="oc.name || 'OC 图片'"
            loading="lazy"
            @load="() => handleImageLoad(oc.id)"
            @error="(e) => handleImageError(e, oc)"
          />

          <div class="card-badge" :class="getTierClass(oc.tier)">
            {{ oc.tier || '标准' }}
          </div>
        </div>
        
        <div class="card-content">
          <h4 class="oc-name">{{ oc.name || '未命名角色' }}</h4>
          <p class="oc-author">by {{ oc.authorName || '未知作者' }}</p>
          
          <div class="oc-meta">
            <span class="meta-item">
              <span class="meta-icon">👤</span>
              {{ getGenderText(oc.gender) }}
            </span>
            <span class="meta-item">
              <span class="meta-icon">🎂</span>
              {{ oc.age || 0 }}岁
            </span>
            <span class="meta-item">
              <span class="meta-icon">🏷️</span>
              {{ oc.species || '未知种族' }}
            </span>
          </div>
          
          <p class="oc-ability" :title="oc.ability">
            {{ truncateText(oc.ability || '暂无能力描述', 60) }}
          </p>
          
          <div class="oc-tags">
            <span class="tag time-tag">{{ formatTime(oc.updateTime) }}</span>
            <span v-if="oc.winCount !== undefined && oc.winCount !== null" class="tag win-tag">
              🏆 {{ oc.winCount }}胜
            </span>
            <span v-if="oc.loseCount !== undefined && oc.loseCount !== null" class="tag lose-tag">
              💔 {{ oc.loseCount }}负
            </span>
          </div>
        </div>
        
        <div class="card-actions">
          <button class="btn-action view" @click.stop="viewOCDetail(oc.id)">
            👁 查看详情
          </button>
          <button class="btn-action edit" @click.stop="editOC(oc.id)">
            ✏️ 编辑
          </button>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-else class="empty-state">
      <div class="empty-content">
        <div class="empty-emoji">🎭</div>
        <div class="empty-text">
          <h4>{{ hasFilters ? '没有匹配的OC' : '暂无人设' }}</h4>
          <p>{{ hasFilters ? '尝试调整搜索条件' : '快来创建第一个OC角色吧！' }}</p>
        </div>
        <button v-if="!hasFilters" class="btn primary" @click="$emit('createOC')">
          ➕ 创建人设
        </button>
        <button v-else class="btn ghost" @click="clearFilters">
          🔄 清除筛选
        </button>
      </div>
    </div>

    <!-- 分页控件 -->
    <footer v-if="totalPages > 1 && !isLoading && !errorMessage" class="panel-foot">
      <div class="pager">
        <button 
          class="btn ghost small" 
          @click="prevPage" 
          :disabled="currentPage === 1"
        >
          ← 上一页
        </button>
        
        <div class="page-numbers">
          <button
            v-for="page in visiblePages"
            :key="page"
            class="page-btn"
            :class="{ active: page === currentPage }"
            @click="goToPage(page)"
          >
            {{ page }}
          </button>
          <span v-if="showEllipsis" class="page-ellipsis">...</span>
        </div>
        
        <button 
          class="btn ghost small" 
          @click="nextPage" 
          :disabled="currentPage === totalPages"
        >
          下一页 →
        </button>
        
        <div class="page-size-selector">
          <span>每页</span>
          <select v-model="pageSize" class="page-size-select" @change="handlePageSizeChange">
            <option value="6">6</option>
            <option value="12">12</option>
            <option value="24">24</option>
          </select>
          <span>条</span>
        </div>
      </div>
    </footer>

    <!-- 调试信息面板 -->
    <div v-if="showDebugPanel" class="debug-panel">
      <div class="debug-header">
        <h4>调试信息</h4>
        <button class="btn ghost small" @click="showDebugPanel = false">关闭</button>
      </div>
      <div class="debug-content">
        <div class="debug-section">
          <h5>数据统计</h5>
          <pre>{{ debugStats }}</pre>
        </div>
        <div class="debug-section">
          <h5>图片URL示例</h5>
          <div v-for="oc in debugImageExamples" :key="oc.id" class="debug-item">
            <strong>{{ oc.name }}:</strong> 原始: {{ oc.OC_image_url || oc.imageUrl }}<br>
            <small>→ 渲染: {{ resolveImageSrc(oc) }}</small>
          </div>
        </div>
        <div class="debug-section">
          <h5>图片测试</h5>
          <button class="btn small" @click="testImageLoading">
            🖼️ 测试图片加载
          </button>
          <div v-if="testResults" class="test-results">
            <pre>{{ testResults }}</pre>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '../../utils/api'

// 定义事件
const emit = defineEmits(['createOC', 'viewDetail', 'editOC'])

// 响应式数据
const searchQuery = ref('')
const selectedSpecies = ref('')
const selectedTier = ref('')
const currentPage = ref(1)
const pageSize = ref(12)
const isLoading = ref(false)
const errorMessage = ref('')
const ocList = ref([])
const showDebugPanel = ref(false)
const testResults = ref(null)

// API端点
const API_ENDPOINTS = {
  LIST: '/OCBattleField/list',
  DETAIL: '/OCBattleField/'
}

// 计算属性
const speciesOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.species).filter(Boolean))].sort()
})

const tierOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.tier).filter(Boolean))].sort()
})

const filteredOCs = computed(() => {
  let filtered = ocList.value
  
  // 搜索过滤
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(oc => 
      (oc.name || '').toLowerCase().includes(query) ||
      (oc.authorName || '').toLowerCase().includes(query) ||
      (oc.species || '').toLowerCase().includes(query) ||
      (oc.ability || '').toLowerCase().includes(query)
    )
  }
  
  // 种族过滤
  if (selectedSpecies.value) {
    filtered = filtered.filter(oc => oc.species === selectedSpecies.value)
  }
  
  // 强度过滤
  if (selectedTier.value) {
    filtered = filtered.filter(oc => oc.tier === selectedTier.value)
  }
  
  return filtered
})

const totalCount = computed(() => ocList.value.length)
const displayedCount = computed(() => filteredOCs.value.length)
const totalPages = computed(() => Math.max(1, Math.ceil(filteredOCs.value.length / pageSize.value)))
const hasFilters = computed(() => searchQuery.value || selectedSpecies.value || selectedTier.value)
// 支持后端返回 imageUrl（完整URL）或旧字段 OC_image_url（相对路径）
const imageCount = computed(() => ocList.value.filter(oc => oc.imageUrl || oc.OC_image_url).length)

const paginatedOCs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredOCs.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages = []
  const start = Math.max(1, currentPage.value - 2)
  const end = Math.min(totalPages.value, start + 4)
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})

const showEllipsis = computed(() => totalPages.value > 5 && currentPage.value < totalPages.value - 2)

// 调试信息
const debugStats = computed(() => {
  return {
    总记录数: totalCount.value,
    有图片的记录: imageCount.value,
    过滤后记录: displayedCount.value,
    当前页记录: paginatedOCs.value.length,
    图片字段示例: ocList.value.slice(0, 3).map(oc => ({
      id: oc.id,
      name: oc.name,
      imageUrlReturned: oc.imageUrl,
      rawPath: oc.OC_image_url,
      resolved: resolveImageSrc(oc)
    }))
  }
})

const debugImageExamples = computed(() => {
  return ocList.value
    .filter(oc => oc.imageUrl || oc.OC_image_url)
    .slice(0, 5)
    .map(oc => ({
      id: oc.id,
      name: oc.name,
      OC_image_url: oc.OC_image_url,
      imageUrl: oc.imageUrl
    }))
})

// 主要方法
const loadOCList = async () => {
  isLoading.value = true
  errorMessage.value = ''
  
  try {
    const response = await apiClient.get(API_ENDPOINTS.LIST)
    
    if (response.data.success) {
      // 兼容后端两种结构：{ data: { items: [...] } } 或 直接 data: [...]
      ocList.value = Array.isArray(response.data.data) 
        ? response.data.data 
        : response.data.data?.items || []
    } else {
      throw new Error(response.data.message || 'API返回失败')
    }
    
  } catch (error) {
    console.error('加载OC列表失败:', error)
    errorMessage.value = getErrorMessage(error)
    ocList.value = []
  } finally {
    isLoading.value = false
  }
}

const getErrorMessage = (error) => {
  if (error.response?.status === 401) {
    return '未授权，请重新登录'
  } else if (error.response?.status === 403) {
    return '权限不足，无法访问OC列表'
  } else if (error.response?.status === 404) {
    return 'API接口不存在，请检查后端服务'
  } else if (error.response?.data?.message) {
    return error.response.data.message
  } else if (error.message) {
    return error.message
  } else {
    return '网络连接失败，请检查API服务'
  }
}

const refreshList = () => {
  currentPage.value = 1
  loadOCList()
}

const handleSearch = () => {
  currentPage.value = 1
}

const handleFilter = () => {
  currentPage.value = 1
}

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

const goToPage = (page) => {
  currentPage.value = page
}

const handlePageSizeChange = () => {
  currentPage.value = 1
}

// 图片处理方法 - 优先使用后端返回的 imageUrl（完整URL）
// 如果只有 OC_image_url（相对路径），则构造到 ImageController 的请求
const resolveImageSrc = (oc) => {
  // 后端优先返回 imageUrl （例如: https://host/api/Image?path=...）
  if (oc.imageUrl && typeof oc.imageUrl === 'string' && oc.imageUrl.length > 0) {
    return oc.imageUrl
  }
  // 否则尝试使用旧字段 OC_image_url（可能是相对路径 like "OC_Battle_Picture/..."）
  if (oc.OC_image_url && typeof oc.OC_image_url === 'string' && oc.OC_image_url.length > 0) {
    return getImageUrl(oc.OC_image_url)
  }
  // 最后回退占位图
  return '/placeholder-avatar.jpg'
}

const getImageUrl = (relativePath) => {
  if (!relativePath) {
    return '/placeholder-avatar.jpg'
  }

  // 如果已经是完整 URL，直接返回
  if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
    return relativePath
  }

  // 保持后端 ImageController 的路径（后端示例： /api/Image?path=... ）
  // 这里构建相对路径（不带 host），会自动跟当前域名一致
  let encoded = relativePath
  if (encoded.startsWith('/')) encoded = encoded.substring(1)
  // 只编码必要的部分：encodeURIComponent 全部编码后把 / 恢复
  encoded = encodeURIComponent(encoded).replace(/%2F/g, '/')

  // 可选：在这里添加缩略尺寸参数（例如 &width=300&height=300）
  const width = 320 // 如果你想默认返回缩略图可改这里或移除
  const height = 160

  return `/api/Image?path=${encoded}&width=${width}&height=${height}`
}

const handleImageLoad = (ocId) => {
  // 简单记录
  // console.log(`✅ 图片加载成功 (OC ID: ${ocId})`)
}

const handleImageError = (event, oc) => {
  console.warn('图片加载失败，使用占位图:', oc.id, oc.name, event.target.src)
  event.target.src = '/placeholder-avatar.jpg'
  event.target.onerror = null
}

const editOC = (ocId) => {
  emit('editOC', ocId)
}

const viewOCDetail = async (ocId) => {
  try {
    emit('viewDetail', ocId)
    // 可选：也可以在这里请求详情
  } catch (error) {
    console.error('获取OC详情失败:', error)
  }
}

const clearFilters = () => {
  searchQuery.value = ''
  selectedSpecies.value = ''
  selectedTier.value = ''
  currentPage.value = 1
}

const getGenderText = (gender) => {
  if (typeof gender === 'number') {
    const genders = { 0: '男', 1: '女', 2: '未知' }
    return genders[gender] || '未知'
  } else if (typeof gender === 'string') {
    return gender
  }
  return '未知'
}

const getTierClass = (tier) => {
  if (!tier) return 'tier-standard'
  const tierMap = {
    '入门': 'tier-beginner',
    'beginner': 'tier-beginner',
    '标准': 'tier-standard', 
    'standard': 'tier-standard',
    '高强度': 'tier-advanced',
    'advanced': 'tier-advanced',
    '超规格': 'tier-ultimate',
    'ultimate': 'tier-ultimate'
  }
  return tierMap[tier] || 'tier-standard'
}

const truncateText = (text, length) => {
  if (!text) return '暂无描述'
  return text.length > length ? text.substring(0, length) + '...' : text
}

const formatTime = (timeString) => {
  if (!timeString) return '未知时间'
  try {
    const date = new Date(timeString)
    if (isNaN(date.getTime())) {
      return timeString
    }
    return date.toLocaleDateString('zh-CN')
  } catch {
    return timeString
  }
}

// 调试方法
const debugData = () => {
  showDebugPanel.value = !showDebugPanel.value
  console.log('OC 列表（前 5）', ocList.value.slice(0,5))
}

const testImageLoading = async () => {
  const tests = []
  const testOC = ocList.value.find(oc => oc.imageUrl || oc.OC_image_url) || ocList.value[0]
  if (!testOC) {
    testResults.value = '没有可用的OC数据进行测试'
    return
  }
  tests.push({
    type: 'resolved',
    url: resolveImageSrc(testOC),
    status: '待测'
  })
  for (const t of tests) {
    try {
      await new Promise((resolve, reject) => {
        const img = new Image()
        img.onload = () => resolve()
        img.onerror = () => reject()
        img.src = t.url
      })
      t.status = '✅ 成功'
    } catch {
      t.status = '❌ 失败'
    }
  }
  testResults.value = { oc: { id: testOC.id, name: testOC.name }, tests }
}

// 生命周期
onMounted(() => {
  loadOCList()
})

// 监听搜索词变化
watch(searchQuery, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    handleSearch()
  }
})

// 暴露方法给父组件
defineExpose({
  refreshList,
  loadOCList,
  debugData
})
</script>

<style scoped>
/* 样式保持不变，与之前相同 */
:root { 
  --card:#fff; 
  --ring:#e6ebf3; 
  --r-lg:16px; 
  --shadow-sm:0 2px 10px rgba(2,6,23,.06); 
  --muted:#5a6478; 
  --primary:#2563eb;
  --success:#10b981;
  --warning:#f59e0b;
  --danger:#ef4444;
}

.card { 
  background:var(--card); 
  border:1px solid var(--ring); 
  border-radius:var(--r-lg); 
  box-shadow:var(--shadow-sm); 
  padding:20px;
  margin-bottom:20px;
}

.card-head { 
  display:grid; 
  grid-template-columns:1fr auto; 
  gap:16px; 
  align-items:start; 
  border-bottom:1px solid var(--ring); 
  padding-bottom:16px; 
  margin-bottom:16px; 
}

.card-head h3 { 
  margin:0; 
  font-size:20px; 
  font-weight:900; 
  color:#1e293b;
}

.tools { 
  display:flex; 
  gap:12px; 
  align-items:center; 
  flex-wrap:wrap;
}

.search-box {
  position:relative;
  min-width:280px;
}

.search-input {
  width:100%;
  height:40px;
  padding-left:40px;
  padding-right:12px;
  border:1px solid var(--ring);
  border-radius:8px;
  outline:none;
  transition:border-color 0.2s;
}

.search-input:focus {
  border-color:var(--primary);
  box-shadow:0 0 0 3px rgba(37,99,235,0.1);
}

.search-icon {
  position:absolute;
  left:12px;
  top:50%;
  transform:translateY(-50%);
  color:var(--muted);
}

.filter-group {
  display:flex;
  gap:8px;
}

.select {
  height:40px;
  padding:0 12px;
  border:1px solid var(--ring);
  border-radius:8px;
  background:white;
  outline:none;
  min-width:120px;
}

.btn { 
  appearance:none; 
  border:1px solid var(--ring); 
  border-radius:8px; 
  padding:8px 16px; 
  font-weight:600; 
  background:#fff; 
  color:#374151;
  cursor:pointer;
  transition:all 0.2s;
  height:40px;
  display:inline-flex;
  align-items:center;
  gap:6px;
}

.btn:disabled {
  opacity:0.6;
  cursor:not-allowed;
}

.btn.ghost:hover:not(:disabled) {
  background:#f8fafc;
  border-color:#d1d5db;
}

.btn.primary {
  background:var(--primary);
  color:white;
  border-color:var(--primary);
}

.btn.primary:hover:not(:disabled) {
  background:#1d4ed8;
}

.loading-spinner {
  width:16px;
  height:16px;
  border:2px solid transparent;
  border-top:2px solid currentColor;
  border-radius:50%;
  animation:spin 1s linear infinite;
}

.loading-spinner.large {
  width:32px;
  height:32px;
  border-width:3px;
}

@keyframes spin {
  to { transform:rotate(360deg); }
}

.stats-bar {
  display:flex;
  gap:20px;
  margin-bottom:16px;
  padding:12px;
  background:#f8fafc;
  border-radius:8px;
  flex-wrap:wrap;
}

.stat-item {
  display:flex;
  align-items:center;
  gap:6px;
}

.stat-label {
  color:var(--muted);
  font-size:14px;
}

.stat-value {
  font-weight:600;
  color:#1e293b;
}

.loading-state, .error-state, .empty-state {
  padding:60px 20px;
  text-align:center;
}

.loading-content, .error-content, .empty-content {
  max-width:300px;
  margin:0 auto;
}

.error-icon, .empty-emoji {
  font-size:48px;
  margin-bottom:16px;
}

.error-text h4, .empty-text h4 {
  margin:0 0 8px 0;
  color:#1e293b;
  font-weight:600;
}

.error-text p, .empty-text p {
  margin:0 0 20px 0;
  color:var(--muted);
  line-height:1.5;
}

.cards-grid {
  display:grid;
  grid-template-columns:repeat(auto-fill, minmax(320px, 1fr));
  gap:16px;
  margin-bottom:20px;
}

.oc-card {
  background:white;
  border:1px solid var(--ring);
  border-radius:12px;
  overflow:hidden;
  transition:all 0.3s ease;
  cursor:pointer;
}

.oc-card:hover {
  transform:translateY(-2px);
  box-shadow:0 8px 25px rgba(0,0,0,0.1);
  border-color:var(--primary);
}

.card-image {
  position:relative;
  height:160px;
  overflow:hidden;
  background:#f1f5f9;
}

.avatar {
  width:100%;
  height:100%;
  object-fit:cover;
  transition:transform 0.3s ease;
}

.oc-card:hover .avatar {
  transform:scale(1.05);
}

.card-badge {
  position:absolute;
  top:8px;
  right:8px;
  padding:4px 8px;
  border-radius:6px;
  font-size:12px;
  font-weight:600;
  color:white;
}

.tier-beginner { background:var(--success); }
.tier-standard { background:var(--primary); }
.tier-advanced { background:var(--warning); }
.tier-ultimate { background:var(--danger); }

.card-content {
  padding:16px;
}

.oc-name {
  margin:0 0 4px 0;
  font-size:18px;
  font-weight:700;
  color:#1e293b;
  line-height:1.2;
}

.oc-author {
  margin:0 0 12px 0;
  color:var(--muted);
  font-size:14px;
}

.oc-meta {
  display:flex;
  gap:12px;
  margin-bottom:12px;
  flex-wrap:wrap;
}

.meta-item {
  display:flex;
  align-items:center;
  gap:4px;
  font-size:13px;
  color:#64748b;
}

.meta-icon {
  font-size:14px;
}

.oc-ability {
  margin:0 0 12px 0;
  color:#475569;
  font-size:14px;
  line-height:1.4;
  min-height:40px;
}

.oc-tags {
  display:flex;
  gap:6px;
  flex-wrap:wrap;
}

.tag {
  padding:2px 8px;
  border-radius:4px;
  font-size:12px;
  font-weight:500;
}

.time-tag {
  background:#f1f5f9;
  color:#475569;
}

.card-actions {
  padding:12px 16px;
  border-top:1px solid #f1f5f9;
  display:flex;
  gap:8px;
}

.btn-action {
  flex:1;
  padding:8px 12px;
  border:1px solid var(--ring);
  border-radius:6px;
  background:white;
  cursor:pointer;
  font-size:13px;
  transition:all 0.2s;
  text-align:center;
}

.btn-action.view:hover {
  background:#eff6ff;
  border-color:var(--primary);
  color:var(--primary);
}

.panel-foot {
  border-top:1px solid var(--ring);
  padding-top:16px;
}

.pager {
  display:flex;
  align-items:center;
  justify-content:center;
  gap:16px;
  flex-wrap:wrap;
}

.page-numbers {
  display:flex;
  gap:4px;
}

.page-btn {
  padding:6px 12px;
  border:1px solid var(--ring);
  background:white;
  border-radius:6px;
  cursor:pointer;
  font-size:14px;
  min-width:36px;
  transition:all 0.2s;
}

.page-btn.active {
  background:var(--primary);
  color:white;
  border-color:var(--primary);
}

.page-btn:hover:not(.active) {
  background:#f8fafc;
}

.page-ellipsis {
  padding:6px 4px;
  color:var(--muted);
}

.page-size-selector {
  display:flex;
  align-items:center;
  gap:6px;
  color:var(--muted);
  font-size:14px;
}

.page-size-select {
  padding:4px 8px;
  border:1px solid var(--ring);
  border-radius:4px;
  background:white;
  outline:none;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .card-head {
    grid-template-columns: 1fr;
    gap: 12px;
  }
  
  .tools {
    justify-content: space-between;
  }
  
  .search-box {
    min-width: 200px;
  }
  
  .filter-group {
    order: -1;
    width: 100%;
  }
  
  .cards-grid {
    grid-template-columns: 1fr;
  }
  
  .pager {
    flex-direction: column;
    gap: 12px;
  }
}

@media (max-width: 480px) {
  .card {
    padding: 16px;
  }
  
  .oc-meta {
    flex-direction: column;
    gap: 6px;
  }
  
  .card-actions {
    flex-direction: column;
  }
  
  .stats-bar {
    flex-direction: column;
    gap: 8px;
    align-items: flex-start;
  }
}
</style>