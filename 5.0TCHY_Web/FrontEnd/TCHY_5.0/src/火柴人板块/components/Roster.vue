<template>
  <section class="oc-container">
    <!-- 搜索和筛选 -->
    <div class="search-filters">
      <input 
        v-model="searchQuery" 
        placeholder="搜索角色名称、作者、种族、能力..."
        @input="currentPage = 1"
      />
      
      <select v-model="selectedSpecies" @change="currentPage = 1">
        <option value="">全部种族</option>
        <option v-for="species in speciesOptions" :key="species" :value="species">
          {{ species }}
        </option>
      </select>
      
      <select v-model="selectedTier" @change="currentPage = 1">
        <option value="">全部强度</option>
        <option v-for="tier in tierOptions" :key="tier" :value="tier">
          {{ tier }}
        </option>
      </select>
      
      <button @click="refreshList" :disabled="isLoading">
        {{ isLoading ? '加载中...' : '刷新' }}
      </button>
      
      <!-- 调试按钮 -->
      <button @click="debugImageUrls" class="debug-btn">
        🐛 调试图片
      </button>

      <!-- 用户信息显示 -->
      <div class="user-info">
        <span v-if="currentUserIdLocal">用户ID: {{ currentUserIdLocal }}</span>
        <span v-else>未登录</span>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="isLoading" class="loading-state">
      正在加载OC列表...
    </div>

    <!-- 错误状态 -->
    <div v-else-if="errorMessage" class="error-state">
      {{ errorMessage }}
      <button @click="refreshList">重试</button>
    </div>

    <!-- 角色卡片列表 -->
    <div v-else-if="paginatedOCs.length > 0" class="oc-list">
      <div 
        v-for="oc in paginatedOCs" 
        :key="oc.id" 
        class="oc-card"
        @click="viewOCDetail(oc.id)"
      >
        <div class="card-image">
          <img
            v-if="getImageUrl(oc)"
            :src="getImageUrl(oc)"
            :alt="oc.name || 'OC图片'"
            loading="lazy"
            @load="() => logImageLoad(oc, 'success')"
            @error="() => logImageLoad(oc, 'error')"
          />
          <div v-else class="no-image-placeholder">
            <span>暂无图片</span>
          </div>
        </div>
        
        <div class="card-content">
          <h3>{{ oc.name || '未命名角色' }}</h3>
          <p class="author">by {{ oc.authorName || '未知作者' }}</p>
          
          <div class="meta">
            <span>{{ getGenderText(oc.gender) }}</span>
            <span>{{ oc.age || 0 }}岁</span>
            <span>{{ oc.species || '未知种族' }}</span>
          </div>
          
          <p class="ability">{{ truncateText(oc.ability, 60) }}</p>

          <!-- 按钮组：约战、详情（打开完整内容） -->
          <!-- 注意：移除了列表页的编辑按钮 -->
          <div class="card-actions">
            <button class="btn-secondary" @click.stop="challengeOC(oc)">约战</button>
            <button class="btn-primary" @click.stop="viewOCDetail(oc.id)">详情</button>
            <!-- 编辑按钮只在详情模态框中显示 -->
          </div>

          <!-- 所有者标识 -->
          <div v-if="isOwner(oc)" class="owner-badge">
            👑 我的角色
          </div>
          
          <!-- 调试信息（可选显示） -->
          <div v-if="showDebugInfo" class="debug-info">
            <small>ID: {{ oc.id }} | URL: {{ getImageUrl(oc) || '无' }} | authorId: {{ oc.authorID || oc.authorId || '无' }}</small>
            <br>
            <small>当前用户: {{ currentUserIdLocal }} | 是否所有者: {{ isOwner(oc) }}</small>
          </div>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-else class="empty-state">
      {{ hasFilters ? '没有匹配的OC' : '暂无人设' }}
      <button v-if="hasFilters" @click="clearFilters">清除筛选</button>
    </div>

    <!-- 分页控件 -->
    <div v-if="totalPages > 1" class="pagination">
      <button @click="prevPage" :disabled="currentPage <= 1">上一页</button>
      <span>第 {{ currentPage }} 页 / 共 {{ totalPages }} 页</span>
      <button @click="nextPage" :disabled="currentPage >= totalPages">下一页</button>
    </div>

    <!-- 详情模态框 -->
    <div v-if="modalLoading || selectedOC" class="modal-overlay" @click.self="closeModal">
      <div class="modal" role="dialog" aria-modal="true">
        <header class="modal-header">
          <div class="header-content">
            <h2 v-if="!modalLoading">{{ selectedOC?.name || '未命名角色' }}</h2>
            <h2 v-else>正在加载...</h2>
            
            <!-- 所有者标识 -->
            <div v-if="selectedOC && isOwner(selectedOC)" class="owner-badge-modal">
              👑 我的角色
            </div>
          </div>
          <button class="close-btn" @click="closeModal">✕</button>
        </header>

        <div class="modal-body">
          <div v-if="modalLoading" class="modal-loading">
            加载中...
          </div>

          <div v-else-if="modalError" class="modal-error">
            {{ modalError }}
          </div>

          <template v-else>
            <div class="modal-image">
              <img v-if="getImageUrl(selectedOC)" :src="getImageUrl(selectedOC)" :alt="selectedOC.name || 'OC图片'" />
              <div v-else class="no-image-placeholder">暂无图片</div>
            </div>

            <div class="modal-info">
              <p><strong>作者：</strong> {{ selectedOC.authorName || '未知作者' }}</p>
              <p><strong>作者ID：</strong> {{ selectedOC.authorID || '未知' }}</p>
              
              <p><strong>性别：</strong> {{ getGenderText(selectedOC.gender) }}</p>
              <p><strong>年龄：</strong> {{ selectedOC.age || '未知' }}</p>
              <p><strong>种族：</strong> {{ selectedOC.species || '未知' }}</p>
              <p><strong>强度：</strong> {{ selectedOC.tier || '未知' }}</p>

              <div class="ability-full">
                <h4>能力 / 简介</h4>
                <pre class="ability-text">{{ selectedOC.ability || '暂无描述' }}</pre>
              </div>

              <!-- 可扩展字段展示 -->
              <div v-if="selectedOC.background">
                <h4>背景</h4>
                <p>{{ selectedOC.background }}</p>
              </div>

              <div v-if="selectedOC.POO">
                <h4>POO</h4>
                <p>{{ selectedOC.POO }}</p>
              </div>
            </div>
          </template>
        </div>

        <footer class="modal-footer" v-if="!modalLoading && !modalError">
          <button class="btn-secondary" @click="closeModal">关闭</button>
          <button class="btn-primary" @click="emitViewDetailFromModal">在新页查看</button>
          
          <!-- 编辑按钮：只有所有者才能看到，并且只在详情模态框中显示 -->
          <button
            v-if="selectedOC && isOwner(selectedOC)"
            class="btn-edit"
            @click="editOCInModal(selectedOC)"
          >
            ✏️ 编辑角色
          </button>
        </footer>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import apiClient from '../../utils/api'

// props：接收当前用户 id（可选），如果未传入会尝试从本地 token 解码提取
const props = defineProps({
  currentUserId: {
    type: [String, Number],
    default: null
  }
})

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
const showDebugInfo = ref(false)

// 详情模态相关
const selectedOC = ref(null)
const modalLoading = ref(false)
const modalError = ref('')

// 当前用户信息（优先使用 props.currentUserId，若未传入则尝试从 token 解码）
const currentUserIdLocal = ref(props.currentUserId ?? null)
const currentUserNameLocal = ref(null)

// ==================== 获取用户ID的API ====================
const GetUserId = () => {
  return apiClient.get('/default/user/id')
}

const fetchUserId = async () => {
  try {
    const response = await GetUserId()
    // 将API返回的用户ID赋值给 currentUserIdLocal，这样 isOwner 函数就能使用了
    currentUserIdLocal.value = response.data.id
    console.log("Current User ID:", currentUserIdLocal.value)
  } catch (error) {
    console.error("获取用户ID失败:", error)
  }
}

/* ============== Helpers to extract current user from JWT token (if available) ============== */
const tryDecodeToken = () => {
  try {
    const possibleKeys = ['token', 'access_token', 'auth_token', 'jwt']
    let raw = null
    for (const k of possibleKeys) {
      raw = localStorage.getItem(k)
      if (raw) break
    }
    if (!raw && typeof document !== 'undefined') {
      const cookies = document.cookie?.split(';').map(c => c.trim())
      for (const c of cookies || []) {
        if (c.startsWith('token=') || c.startsWith('access_token=')) {
          raw = decodeURIComponent(c.split('=')[1])
          break
        }
      }
    }
    if (!raw) return false

    if (raw.startsWith('Bearer ')) raw = raw.slice(7)

    const parts = raw.split('.')
    if (parts.length < 2) return false
    const payload = parts[1]
    const json = JSON.parse(decodeBase64Url(payload))
    if (json.nameid || json.sub || json.id) {
      currentUserIdLocal.value = json.nameid ?? json.sub ?? json.id
    }
    if (json.unique_name || json.name || json.email) {
      currentUserNameLocal.value = json.unique_name ?? json.name ?? json.email
    }
    return true
  } catch (e) {
    return false
  }
}

const decodeBase64Url = (str) => {
  str = str.replace(/-/g, '+').replace(/_/g, '/')
  while (str.length % 4) str += '='
  try {
    return decodeURIComponent(atob(str).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    }).join(''))
  } catch {
    try {
      return Buffer.from(str, 'base64').toString('utf-8')
    } catch {
      return ''
    }
  }
}

// ==================== 图片处理函数 ====================
const getImageUrl = (oc) => {
  if (!oc) return null
  const url = oc.imageUrl || oc.OC_image_url || oc.img || (oc.image && oc.image.url)
  if (typeof url === 'string' && (url.startsWith('http://') || url.startsWith('https://'))) {
    return url
  }
  return null
}

// ==================== 调试方法 ====================
const debugImageUrls = () => {
  showDebugInfo.value = !showDebugInfo.value
  if (ocList.value.length === 0) {
    console.log('❌ 没有OC数据')
    return
  }
  const total = ocList.value.length
  const withImg = ocList.value.filter(oc => getImageUrl(oc)).length
  console.log({ 总数: total, 有图片: withImg, 无图: total - withImg })
  ocList.value.forEach((oc, idx) => {
    console.log(idx + 1, oc.id, oc.name, 'authorId:', oc.authorID ?? oc.authorId ?? oc.author?.id)
  })
}

const logImageLoad = (oc, status) => {
  console.log(`图片 ${status}:`, oc.id, getImageUrl(oc))
}

// ==================== 主要业务方法 ====================
const loadOCList = async () => {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const response = await apiClient.get('/OCBattleField/list')
    if (response.data.success) {
      const items = response.data.data?.items || response.data.data || []
      ocList.value = items.map(it => ({
        ...it,
        authorID: it.authorID ?? it.authorId ?? it.author?.id ?? it.authorID
      }))
    } else {
      throw new Error(response.data.message || 'API返回失败')
    }
  } catch (error) {
    console.error('加载OC列表失败:', error)
    errorMessage.value = getErrorMessage(error)
  } finally {
    isLoading.value = false
  }
}

// 调用详情接口按需获取完整数据（包含 ability）
const fetchOCDetail = async (id) => {
  modalLoading.value = true
  modalError.value = ''
  selectedOC.value = null
  try {
    const res = await apiClient.get(`/OCBattleField/${id}`)
    if (res.data && res.data.success && res.data.data) {
      const raw = res.data.data
      raw.ability = raw.ability ?? raw.Ability ?? raw.abilityDetail ?? raw.description ?? raw.intro ?? ''
      selectedOC.value = raw
      const idx = ocList.value.findIndex(x => String(x.id) === String(id))
      if (idx !== -1) {
        ocList.value[idx] = { ...ocList.value[idx], ...raw }
      }
      emit('viewDetail', id)
    } else {
      modalError.value = res.data?.message || '获取详情失败'
    }
  } catch (err) {
    console.error('获取OC详情失败：', err)
    modalError.value = err?.response?.data?.message || err.message || '请求失败'
  } finally {
    modalLoading.value = false
  }
}

/**
 * 点击卡片或详情按钮：请求详情并打开模态
 */
const viewOCDetail = (ocOrId) => {
  const id = typeof ocOrId === 'number' || typeof ocOrId === 'string' ? ocOrId : (ocOrId && ocOrId.id)
  if (!id) return
  fetchOCDetail(id)
}

const closeModal = () => {
  selectedOC.value = null
  modalError.value = ''
  modalLoading.value = false
}

const emitViewDetailFromModal = () => {
  if (selectedOC.value) emit('viewDetail', selectedOC.value.id)
}

const refreshList = () => {
  currentPage.value = 1
  loadOCList()
}

// ==================== 模态框编辑功能 ====================
/**
 * 详情模态框中的编辑按钮点击事件
 * 只有进入详情模态框后才会显示编辑按钮
 */
const editOCInModal = (oc) => {
  console.log('在详情模态框中编辑OC:', oc.id, oc.name)
  closeModal()
  emit('editOC', oc.id)
}

/**
 * 判断当前用户是否为上传者
 * 这个函数在列表页用于显示"我的角色"标识，在详情页用于显示编辑按钮
 */
const isOwner = (oc) => {
  if (!oc || !currentUserIdLocal.value) {
    return false
  }
  
  const ocAuthorID = oc.authorID ?? oc.authorId ?? oc.author?.id
  if (ocAuthorID && currentUserIdLocal.value) {
    const isMatch = String(ocAuthorID) === String(currentUserIdLocal.value)
    console.log('所有权检查:', {
      ocID: oc.id,
      ocAuthorID: ocAuthorID,
      currentUserID: currentUserIdLocal.value,
      isOwner: isMatch
    })
    return isMatch
  }
  
  return false
}

// ==================== 其他方法 ====================
const challengeOC = (oc) => {
  console.log('约战（占位）：', oc.id, oc.name)
}

const clearFilters = () => {
  searchQuery.value = ''
  selectedSpecies.value = ''
  selectedTier.value = ''
  currentPage.value = 1
}

// ==================== 计算属性 ====================
const speciesOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.species).filter(Boolean))].sort()
})

const tierOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.tier).filter(Boolean))].sort()
})

const filteredOCs = computed(() => {
  let filtered = ocList.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    filtered = filtered.filter(oc =>
      (oc.name || '').toLowerCase().includes(q) ||
      (oc.authorName || '').toLowerCase().includes(q) ||
      (oc.species || '').toLowerCase().includes(q) ||
      ((oc.ability || '') + '').toLowerCase().includes(q)
    )
  }
  if (selectedSpecies.value) filtered = filtered.filter(oc => oc.species === selectedSpecies.value)
  if (selectedTier.value) filtered = filtered.filter(oc => oc.tier === selectedTier.value)
  return filtered
})

const paginatedOCs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredOCs.value.slice(start, start + pageSize.value)
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredOCs.value.length / pageSize.value)))
const hasFilters = computed(() => !!(searchQuery.value || selectedSpecies.value || selectedTier.value))

// ==================== 分页方法 ====================
const prevPage = () => { if (currentPage.value > 1) currentPage.value-- }
const nextPage = () => { if (currentPage.value < totalPages.value) currentPage.value++ }

// ==================== 工具函数 ====================
const getGenderText = (gender) => {
  if (typeof gender === 'number') {
    return ['男', '女', '未知'][gender] || '未知'
  }
  return gender || '未知'
}

const truncateText = (text, length) => {
  if (!text) return '暂无描述'
  return text.length > length ? text.substring(0, length) + '...' : text
}

const getErrorMessage = (error) => {
  if (!error) return '请求失败'
  if (error.response) {
    switch (error.response.status) {
      case 401: return '未授权，请登录'
      case 403: return '权限不足'
      case 404: return '接口不存在'
      default: return error.response.data?.message || '请求失败'
    }
  }
  return error.message || '网络错误'
}

// ==================== 生命周期 ====================
onMounted(() => {
  if (!currentUserIdLocal.value) tryDecodeToken()
  fetchUserId()
  loadOCList()
  window.addEventListener('keydown', onKeyDown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeyDown)
})

const onKeyDown = (e) => {
  if (e.key === 'Escape' && selectedOC.value) {
    closeModal()
  }
}

// 监听搜索变化
watch(searchQuery, () => { currentPage.value = 1 })

// 暴露方法
defineExpose({
  refreshList,
  debugImageUrls
})
</script>

<style scoped>
.oc-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.search-filters {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
  align-items: center;
}

.search-filters input,
.search-filters select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.search-filters button {
  padding: 8px 16px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.search-filters button:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.user-info {
  margin-left: auto;
  padding: 8px 12px;
  background: #f0f0f0;
  border-radius: 4px;
  font-size: 14px;
}

.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 40px;
  color: #666;
}

.error-state {
  color: #dc3545;
}

.oc-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.oc-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  background: white;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  position: relative;
}

.oc-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.card-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
}

.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-image-placeholder {
  width: 100%;
  height: 100%;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999;
}

.card-content {
  padding: 15px;
}

.card-content h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  color: #333;
}

.author {
  color: #666;
  font-size: 14px;
  margin: 0 0 10px 0;
}

.meta {
  display: flex;
  gap: 10px;
  margin: 10px 0;
  font-size: 12px;
  color: #888;
}

.ability {
  color: #666;
  font-size: 14px;
  line-height: 1.4;
  margin: 10px 0;
}

.card-actions {
  display: flex;
  gap: 8px;
  margin-top: 15px;
}

.btn-primary,
.btn-secondary,
.btn-edit {
  padding: 6px 12px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  flex: 1;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-edit {
  background: #28a745;
  color: white;
}

.owner-badge {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(255, 215, 0, 0.9);
  color: #8b6914;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 10px;
  font-weight: bold;
}

.debug-info {
  background: #f5f5f5;
  padding: 8px;
  border-radius: 4px;
  font-size: 10px;
  margin-top: 10px;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 15px;
  margin-top: 20px;
}

.pagination button {
  padding: 8px 16px;
  border: 1px solid #ddd;
  background: white;
  border-radius: 4px;
  cursor: pointer;
}

.pagination button:disabled {
  background: #f8f9fa;
  color: #6c757d;
  cursor: not-allowed;
}

/* 模态框样式 */
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
}

.modal {
  background: white;
  border-radius: 8px;
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e0e0e0;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 10px;
}

.owner-badge-modal {
  background: #ffd700;
  color: #8b6914;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: bold;
}

.close-btn {
  background: none;
  border: none;
  font-size: 20px;
  cursor: pointer;
  padding: 5px;
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.modal-loading,
.modal-error {
  text-align: center;
  padding: 40px;
}

.modal-error {
  color: #dc3545;
}

.modal-image {
  width: 100%;
  height: 300px;
  margin-bottom: 20px;
  border-radius: 4px;
  overflow: hidden;
}

.modal-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.modal-info p {
  margin: 8px 0;
}

.ability-full {
  margin: 20px 0;
}

.ability-text {
  white-space: pre-wrap;
  background: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
  font-family: inherit;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 20px;
  border-top: 1px solid #e0e0e0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .search-filters {
    flex-direction: column;
    align-items: stretch;
  }
  
  .user-info {
    margin-left: 0;
    text-align: center;
  }
  
  .oc-list {
    grid-template-columns: 1fr;
  }
  
  .modal-footer {
    flex-direction: column;
  }
  
  .modal {
    width: 95%;
    margin: 10px;
  }
}

.debug-btn {
  background: #ffc107;
  color: #212529;
}
</style>