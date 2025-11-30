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
            :alt="oc.name || '角色立绘'"
            @load="logImageLoad(oc, 'success')"
            @error="logImageLoad(oc, 'error')"
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

          <!-- 按钮组：约战、详情 -->
          <div class="card-actions">
            <button class="btn-secondary" @click.stop="challengeOC(oc)">约战</button>
            <button class="btn-primary" @click.stop="viewOCDetail(oc.id)">详情</button>
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
            <div class="spinner"></div>
            <span>加载中...</span>
          </div>

          <div v-else-if="modalError" class="modal-error">
            {{ modalError }}
          </div>

          <template v-else>
            <div class="modal-image">
              <img
                v-if="getImageUrl(selectedOC)"
                :src="getImageUrl(selectedOC)"
                :alt="selectedOC?.name || '角色立绘'"
                @load="logImageLoad(selectedOC, 'success')"
                @error="logImageLoad(selectedOC, 'error')"
              />
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

              <!-- 武器图片 -->
              <div v-if="selectedOC.weaponImages && selectedOC.weaponImages.length">
                <h4>武器立绘</h4>
                <div class="weapon-list">
                  <div v-for="(w, idx) in selectedOC.weaponImages" :key="idx" class="weapon-item">
                    <img :src="w" :alt="`weapon-${idx}`" @click="previewImage(w)" />
                  </div>
                </div>
              </div>

              <!-- 版本信息 -->
              <div class="version-info">
                <h4>版本信息</h4>
                <p><strong>当前版本：</strong> v{{ selectedOC.versionNumber }}</p>
                <p><strong>版本描述：</strong> {{ selectedOC.versionDescription || '无描述' }}</p>
                <p><strong>创建时间：</strong> {{ formatDate(selectedOC.createTime) }}</p>
              </div>

              <!-- 版本历史 -->
              <div v-if="selectedOC.versionHistory && selectedOC.versionHistory.length > 0" class="version-history">
                <h4>版本历史</h4>
                <div class="version-list">
                  <div 
                    v-for="version in selectedOC.versionHistory" 
                    :key="version.versionNumber"
                    class="version-item"
                    :class="{ current: version.isCurrent }"
                  >
                    <span class="version-number">v{{ version.versionNumber }}</span>
                    <span class="version-desc">{{ version.versionDescription }}</span>
                    <span class="version-time">{{ formatDate(version.createTime) }}</span>
                    <span class="version-age">{{ version.age }}岁</span>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </div>

        <footer class="modal-footer" v-if="!modalLoading && !modalError">
          <button class="btn-secondary" @click="closeModal">关闭</button>
          <button class="btn-primary" @click="emitViewDetailFromModal">在新页查看</button>
          
          <!-- 编辑按钮：只有所有者才能看到（并且仅在详情模态中可见） -->
          <button
            v-if="selectedOC && isOwner(selectedOC)"
            class="btn-edit"
            @click="editOCFromModal"
          >
            ✏️ 编辑角色
          </button>
        </footer>
      </div>
    </div>

    <!-- 编辑模态框 -->
    <div v-if="showEditModal" class="modal-overlay" @click.self="closeEditModal">
      <div class="modal edit-modal" role="dialog" aria-modal="true">
        <header class="modal-header">
          <h2>编辑角色 - {{ editingOC?.name }}</h2>
          <button class="close-btn" @click="closeEditModal">✕</button>
        </header>

        <div class="modal-body">
          <div v-if="editLoading" class="modal-loading">
            <div class="spinner"></div>
            <span>保存中...</span>
          </div>

          <div v-else-if="editError" class="modal-error">
            {{ editError }}
          </div>

          <form v-else @submit.prevent="onSaveClick()" class="edit-form" novalidate>
            <!-- 编辑表单字段 -->
            <div class="form-group">
              <label class="form-label required">角色名称</label>
              <input
                v-model="editForm.name"
                type="text"
                class="form-input"
                :maxlength="50"
                placeholder="输入角色名称"
                required
              />
              <div class="form-hint">{{ editForm.name.length }}/50</div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label required">性别</label>
                <select v-model="editForm.gender" class="form-select" required>
                  <option value="0">男</option>
                  <option value="1">女</option>
                  <option value="2">未知</option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label required">年龄</label>
                <input
                  v-model.number="editForm.age"
                  type="number"
                  class="form-input"
                  min="0"
                  max="1000"
                  placeholder="年龄"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label required">种族</label>
              <input
                v-model="editForm.species"
                type="text"
                class="form-input"
                :maxlength="100"
                placeholder="输入种族"
                required
              />
            </div>

            <div class="form-group">
              <label class="form-label required">能力描述</label>
              <textarea
                v-model="editForm.ability"
                class="form-textarea"
                rows="4"
                :maxlength="500"
                placeholder="描述角色的能力"
                required
              ></textarea>
              <div class="form-hint">{{ editForm.ability.length }}/500</div>
            </div>

            <div class="form-group">
              <label class="form-label">更新立绘（可选）</label>
              <div class="image-upload-area">
                <label v-if="!editImagePreview" class="upload-btn">
                  <input 
                    type="file" 
                    accept="image/*" 
                    @change="handleEditImageSelect"
                    class="file-input"
                  />
                  <span class="upload-icon">+</span>
                  <span>选择新图片</span>
                </label>
                
                <div v-else class="image-preview">
                  <img
                    :src="editImagePreview"
                    alt="编辑图片预览"
                    @error="logImageLoad(editingOC, 'error')"
                  />
                  <button type="button" class="remove-btn" @click="removeEditImage">×</button>
                </div>
              </div>
              <p class="form-hint">支持 JPG、PNG、WebP，最大 5MB</p>
            </div>

            <!-- 新增：编辑时追加武器图片（多张） -->
            <div class="form-group">
              <label class="form-label">新增武器图片（可选，多张）</label>
              <input type="file" accept="image/*" @change="handleEditWeaponSelect" multiple />
              <div class="weapon-previews" v-if="editWeaponPreviews.length">
                <div v-for="(p, idx) in editWeaponPreviews" :key="idx" class="weapon-preview">
                  <img :src="p" alt="weapon preview" />
                  <button type="button" @click="removeEditWeaponAt(idx)">×</button>
                </div>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">编辑说明</label>
              <input
                v-model="editForm.editDescription"
                type="text"
                class="form-input"
                :maxlength="200"
                placeholder="简要说明此次编辑的内容（可选）"
              />
              <div class="form-hint">例如：更新了年龄和能力描述</div>
            </div>

            <div class="form-actions">
              <button type="button" class="btn-secondary" @click="closeEditModal" :disabled="editLoading">
                取消
              </button>
              <button type="button" class="btn-primary btn-save" @click="onSaveClick()" :disabled="editLoading">
                {{ editLoading ? '保存中...' : '保存更改' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 图片预览模态框 -->
    <div v-if="previewImageUrl" class="image-preview-modal" @click="previewImageUrl = null">
      <div class="preview-content" @click.stop>
        <img :src="previewImageUrl" alt="图片预览" />
        <button class="preview-close" @click="previewImageUrl = null" title="关闭">×</button>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, reactive, onMounted, onUnmounted, watch } from 'vue'
import apiClient from '../../utils/api'

// props & emits
const props = defineProps({
  currentUserId: {
    type: [String, Number],
    default: null
  }
})
const emit = defineEmits(['createOC', 'viewDetail', 'editOC'])

// ============ 响应式数据 ============
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

// 编辑模态相关
const showEditModal = ref(false)
const editingOC = ref(null)
const editLoading = ref(false)
const editError = ref('')
const editImagePreview = ref(null)

// 编辑表单（支持追加武器图片）
const editForm = reactive({
  name: '',
  gender: 0,
  age: 0,
  species: '',
  ability: '',
  CharacterImage: null,
  WeaponImages: [],      // File[]
  editDescription: '内容更新'
})
const editWeaponPreviews = ref([]) // array of data URLs (includes existing URLs first, then new file previews)

// 图片预览
const previewImageUrl = ref('')

// 当前用户信息
const currentUserIdLocal = ref(props.currentUserId ?? null)

// ============ 计算属性 ============
const speciesOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.species).filter(Boolean))].sort()
})

const tierOptions = computed(() => {
  return [...new Set(ocList.value.map(oc => oc.worldTime).filter(Boolean))].sort()
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
  if (selectedTier.value) filtered = filtered.filter(oc => oc.worldTime === selectedTier.value)
  return filtered
})

const paginatedOCs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredOCs.value.slice(start, start + pageSize.value)
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredOCs.value.length / pageSize.value)))
const hasFilters = computed(() => !!(searchQuery.value || selectedSpecies.value || selectedTier.value))

const editFormValid = computed(() => {
  return editForm.name.trim().length >= 2 &&
         editForm.name.trim().length <= 50 &&
         editForm.species.trim().length >= 1 &&
         editForm.ability.trim().length >= 10 &&
         editForm.age >= 0 &&
         editForm.age <= 1000
})

// ============ 主要业务方法 ============

const loadOCList = async () => {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const response = await apiClient.get('/ocbattlefield/list')
    if (response.data && response.data.success) {
      const items = response.data.data?.items || response.data.data || []
      ocList.value = items.map(item => ({
        ...item,
        authorID: item.authorID ?? item.authorId ?? item.author?.id
      }))
    } else {
      throw new Error(response.data?.message || 'API返回失败')
    }
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    isLoading.value = false
  }
}

// 获取OC详情
const fetchOCDetail = async (id) => {
  modalLoading.value = true
  modalError.value = ''
  selectedOC.value = null
  try {
    const response = await apiClient.get(`/ocbattlefield/${encodeURIComponent(String(id))}`)
    if (response.data && response.data.success && response.data.data) {
      const raw = response.data.data
      selectedOC.value = {
        ...raw,
        ability: raw.ability ?? '',
        authorID: raw.authorID ?? raw.authorId ?? raw.author?.id,
        weaponImages: raw.weaponImages ?? [] // full URLs
      }
      const index = ocList.value.findIndex(x => String(x.id) === String(id) || String(x.ocId) === String(id))
      if (index !== -1) {
        ocList.value[index] = { ...ocList.value[index], ...selectedOC.value }
      }
      emit('viewDetail', id)
    } else {
      modalError.value = response.data?.message || '获取详情失败'
    }
  } catch (error) {
    modalError.value = getErrorMessage(error)
  } finally {
    modalLoading.value = false
  }
}

// 打开详情模态框
const viewOCDetail = (ocOrId) => {
  const id = typeof ocOrId === 'number' || typeof ocOrId === 'string' ? ocOrId : (ocOrId && (ocOrId.id ?? ocOrId.ocId ?? ocOrId.ocID))
  if (!id) return
  fetchOCDetail(id)
}

// 关闭详情模态框
const closeModal = () => {
  selectedOC.value = null
  modalError.value = ''
  modalLoading.value = false
}

// 在新页查看
const emitViewDetailFromModal = () => {
  if (selectedOC.value) {
    emit('viewDetail', selectedOC.value.id ?? selectedOC.value.ocId)
    closeModal()
  }
}

// ============ 编辑流程：从详情进入编辑 ============

const editOCFromModal = () => {
  if (!selectedOC.value) { setMessage('无选中角色，无法编辑', 'error'); return }
  const idFromSelected = selectedOC.value.id ?? selectedOC.value.ocId ?? selectedOC.value.ocID ?? null
  if (!idFromSelected) { setMessage('无法确定角色 ID，编辑失败', 'error'); return }
  closeModal()
  editOC(idFromSelected)
}

const editOC = async (ocOrId) => {
  try {
    if (!ocOrId) { setMessage('未提供要编辑的角色', 'error'); return }
    let id = ocOrId
    if (typeof ocOrId === 'object') {
      id = ocOrId.id ?? ocOrId.ocId ?? ocOrId.ocID ?? ocOrId.ocid ?? null
    }
    if (!id) { setMessage('角色 ID 无效', 'error'); return }
    id = String(id)

    editingOC.value = null
    editError.value = ''
    editImagePreview.value = null
    editForm.WeaponImages = []
    editWeaponPreviews.value = []

    const response = await apiClient.get(`/ocbattlefield/${encodeURIComponent(id)}`)

    if (response.data && response.data.success && response.data.data) {
      const ocData = response.data.data
      editForm.name = ocData.name ?? ''
      editForm.gender = ocData.gender ?? 0
      editForm.age = ocData.age ?? 0
      editForm.species = ocData.species ?? ''
      editForm.ability = ocData.ability ?? ''
      editForm.editDescription = '内容更新'
      editForm.CharacterImage = null
      editForm.WeaponImages = []

      // existing weapon URLs -> show as previews first
      editWeaponPreviews.value = (ocData.weaponImages && Array.isArray(ocData.weaponImages)) ? [...ocData.weaponImages] : []

      const resolvedId = ocData.ocId ?? ocData.id ?? id
      editingOC.value = { ...(ocData || {}), id: resolvedId }

      // set edit image preview if exists
      if (ocData.imageUrl || ocData.OC_image_url || ocData.image || ocData.currentVersion?.OC_image_url) {
        editImagePreview.value = getImageUrl(ocData) || getImageUrl({ imageUrl: ocData.imageUrl || ocData.currentVersion?.OC_image_url })
      }

      showEditModal.value = true
    } else {
      setMessage(response.data?.message || '加载角色详情失败', 'error')
    }
  } catch (error) {
    setMessage('加载角色详情失败', 'error')
  }
}

// 关闭编辑模态框
const closeEditModal = () => {
  showEditModal.value = false
  editingOC.value = null
  editError.value = ''
  editLoading.value = false
  resetEditForm()
}

// 重置编辑表单
const resetEditForm = () => {
  editForm.name = ''
  editForm.gender = 0
  editForm.age = 0
  editForm.species = ''
  editForm.ability = ''
  editForm.CharacterImage = null
  editForm.WeaponImages = []
  editForm.editDescription = '内容更新'
  editImagePreview.value = null
  editWeaponPreviews.value = []
}

// 处理编辑图片选择（人物）
const handleEditImageSelect = (event) => {
  const file = event.target.files[0]
  if (!file) return
  if (!validateImageFile(file)) { setMessage('不支持的图片格式或超过大小限制', 'error'); return }
  editForm.CharacterImage = file
  const reader = new FileReader()
  reader.onload = (e) => editImagePreview.value = e.target.result
  reader.readAsDataURL(file)
  event.target.value = ''
}

// 编辑时选择武器图片（追加）
const handleEditWeaponSelect = (event) => {
  const files = Array.from(event.target.files || [])
  files.forEach(file => {
    if (!validateImageFile(file)) { setMessage('不支持的图片或超过5MB', 'error'); return }
    editForm.WeaponImages.push(file)
    const reader = new FileReader()
    reader.onload = (e) => editWeaponPreviews.value.push(e.target.result)
    reader.readAsDataURL(file)
  })
  event.target.value = ''
}

const removeEditWeaponAt = (index) => {
  const existingUrlsCount = (selectedOC.value?.weaponImages?.length) || 0
  if (index < existingUrlsCount) {
    // remove existing URL
    selectedOC.value.weaponImages.splice(index, 1)
    editWeaponPreviews.value.splice(index, 1)
  } else {
    // remove newly added file
    const fileIndex = index - existingUrlsCount
    if (fileIndex >= 0 && fileIndex < editForm.WeaponImages.length) {
      editForm.WeaponImages.splice(fileIndex, 1)
      editWeaponPreviews.value.splice(index, 1)
    }
  }
}

// 移除编辑人物图片
const removeEditImage = () => {
  editForm.CharacterImage = null
  editImagePreview.value = null
}

// 保存编辑（提交）
const onSaveClick = (force = false) => {
  if (editLoading.value) return
  if (!force && !editFormValid.value) {
    setMessage('表单验证未通过，请检查必填项（名称2-50、种族、能力≥10字符等）', 'error')
    return
  }
  submitEdit(force)
}

const submitEdit = async (force = false) => {
  if (!editingOC.value || !editingOC.value.id) { setMessage('编辑失败：无效的角色信息', 'error'); return }
  editLoading.value = true
  editError.value = ''
  try {
    const formData = new FormData()
    formData.append('name', editForm.name.trim())
    formData.append('gender', String(editForm.gender))
    formData.append('age', String(editForm.age))
    formData.append('species', editForm.species.trim())
    formData.append('ability', editForm.ability.trim())
    formData.append('updateDescription', editForm.editDescription || '内容更新')

    if (editForm.CharacterImage) formData.append('CharacterImage', editForm.CharacterImage)

    // append new weapon files
    if (editForm.WeaponImages && editForm.WeaponImages.length) {
      editForm.WeaponImages.forEach(f => formData.append('WeaponImages', f))
    }

    const url = `/ocbattlefield/${encodeURIComponent(String(editingOC.value.id))}/update`
    const response = await apiClient.post(url, formData)
    if (response.data && response.data.success) {
      setMessage('✅ 角色编辑成功！', 'success')
      closeEditModal()
      await loadOCList()
    } else {
      throw new Error(response.data?.message || '编辑失败')
    }
  } catch (error) {
    editError.value = error.message || '编辑失败'
    setMessage(editError.value, 'error')
  } finally {
    editLoading.value = false
  }
}

// ============ 辅助函数 ============

const fetchUserId = async () => {
  try {
    const response = await apiClient.get('/default/user/id')
    if (response.data && response.data.id) currentUserIdLocal.value = response.data.id
  } catch {
    tryDecodeToken()
  }
}

const tryDecodeToken = () => {
  try {
    const token = localStorage.getItem('token') || localStorage.getItem('access_token')
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]))
      currentUserIdLocal.value = payload.nameid || payload.sub || payload.id
    }
  } catch {}
}

const isOwner = (oc) => {
  if (!oc || !currentUserIdLocal.value) return false
  const ocAuthorID = oc.authorID ?? oc.authorId ?? oc.author?.id
  return ocAuthorID && String(ocAuthorID) === String(currentUserIdLocal.value)
}

const getImageUrl = (oc) => {
  if (!oc) return null
  const url = oc.imageUrl || oc.OC_image_url || oc.img || (oc.image && oc.image.url)
  if (typeof url === 'string') {
    if (url.startsWith('/')) return `${window.location.origin}${url}`
    if (url.startsWith('http://') || url.startsWith('https://')) return url
  }
  return null
}

const logImageLoad = (oc, status) => {
  if (showDebugInfo.value) {
    console.log(`🖼️ 图片${status}:`, { id: oc?.id, name: oc?.name, url: getImageUrl(oc), authorID: oc?.authorID })
  }
}

const getGenderText = (gender) => {
  if (typeof gender === 'number') return ['男', '女', '未知'][gender] || '未知'
  return gender || '未知'
}

const truncateText = (text, length) => {
  if (!text) return '暂无描述'
  return text.length > length ? text.substring(0, length) + '...' : text
}

const formatDate = (dateString) => {
  if (!dateString) return '未知'
  try {
    const date = new Date(dateString)
    return date.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
  } catch {
    return dateString
  }
}

const getErrorMessage = (error) => {
  if (!error) return '请求失败'
  if (error.response) {
    switch (error.response.status) {
      case 401: return '未授权，请登录'
      case 403: return '权限不足'
      case 404: return '接口不存在'
      case 400: return error.response.data?.message || '请求参数错误'
      case 500: return '服务器内部错误'
      default: return error.response.data?.message || `请求失败 (${error.response.status})`
    }
  }
  return error.message || '网络错误'
}

const setMessage = (text, type, timeout = 2500) => {
  // simple snackbar-ish message
  const el = document.createElement('div')
  el.textContent = text
  el.style.position = 'fixed'
  el.style.right = '20px'
  el.style.bottom = '20px'
  el.style.padding = '10px 14px'
  el.style.borderRadius = '6px'
  el.style.color = '#fff'
  el.style.zIndex = 9999
  el.style.background = type === 'error' ? '#dc3545' : '#16a34a'
  document.body.appendChild(el)
  setTimeout(() => { document.body.removeChild(el) }, timeout)
}

const validateImageFile = (file) => {
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif']
  const maxSize = 5 * 1024 * 1024 // 5MB
  if (!file) return false
  if (!allowedTypes.includes(file.type)) return false
  if (file.size > maxSize) return false
  return true
}

const previewImage = (url) => {
  previewImageUrl.value = url
}

// debug
const debugImageUrls = () => {
  showDebugInfo.value = !showDebugInfo.value
  console.log('当前用户ID:', currentUserIdLocal.value)
  console.log('OC列表数量:', ocList.value.length)
  ocList.value.forEach((oc, i) => console.log(i+1, oc.id, oc.name, getImageUrl(oc), oc.weaponImages))
}

const refreshList = () => { currentPage.value = 1; loadOCList() }
const clearFilters = () => { searchQuery.value = ''; selectedSpecies.value = ''; selectedTier.value = ''; currentPage.value = 1 }
const prevPage = () => { if (currentPage.value > 1) currentPage.value-- }
const nextPage = () => { if (currentPage.value < totalPages.value) currentPage.value++ }
const editLoadingGuard = () => {}

onMounted(() => {
  if (!currentUserIdLocal.value) fetchUserId()
  loadOCList()
  try { window.__submitEdit = submitEdit } catch {}
})

onUnmounted(() => {
  try { delete window.__submitEdit } catch {}
})

watch(searchQuery, () => { currentPage.value = 1 })
watch([selectedSpecies, selectedTier], () => { currentPage.value = 1 })

// expose methods to parent
defineExpose({ refreshList, loadOCList, debugImageUrls })
</script>

<style scoped>
/* 完整：Roster.vue 组件样式（包括编辑/详情/武器/版本/表单 等相关类） */

/* 容器与布局 */
.oc-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  box-sizing: border-box;
}

/* 搜索与筛选 */
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
  background: #fff;
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

/* 状态显示 */
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

/* 列表网格 */
.oc-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

/* 卡片 */
.oc-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  background: white;
  cursor: pointer;
  transition: transform 0.18s ease, box-shadow 0.18s ease;
  position: relative;
}
.oc-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 18px rgba(0,0,0,0.08);
}

/* 卡片图片 */
.card-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
  background: #fafafa;
}
.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* 无图占位 */
.no-image-placeholder {
  width: 100%;
  height: 100%;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999;
  font-size: 14px;
}

/* 卡片内容 */
.card-content {
  padding: 15px;
}

.card-content h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  color: #222;
}

.author {
  color: #666;
  font-size: 13px;
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
  color: #444;
  font-size: 14px;
  line-height: 1.4;
  margin: 10px 0;
  max-height: 3.6em;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 操作按钮组 */
.card-actions {
  display: flex;
  gap: 8px;
  margin-top: 12px;
}

.btn-primary,
.btn-secondary,
.btn-edit {
  padding: 6px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
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

/* 所有者徽章 */
.owner-badge {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(255, 215, 0, 0.95);
  color: #8b6914;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
}

/* 调试信息 */
.debug-info {
  background: #f5f5f5;
  padding: 8px;
  border-radius: 4px;
  font-size: 11px;
  margin-top: 10px;
  color: #333;
}

/* 分页 */
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

/* 模态框 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.52);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}
.modal {
  background: #fff;
  border-radius: 8px;
  max-width: 900px;
  width: 94%;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: 0 12px 30px rgba(0,0,0,0.12);
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 20px;
  border-bottom: 1px solid #eee;
}
.header-content { display:flex; align-items:center; gap:10px; }
.close-btn { background:none; border:none; font-size:20px; cursor:pointer; padding:6px; }
.modal-body { padding: 18px; overflow:auto; }
.modal-footer { display:flex; justify-content:flex-end; gap:10px; padding: 14px 18px; border-top:1px solid #eee; }

/* spinner */
.spinner {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 4px solid rgba(0,0,0,0.08);
  border-top-color: #007bff;
  animation: spin 1s linear infinite;
  margin: 0 auto 12px;
}
@keyframes spin { to { transform: rotate(360deg); } }

.modal-loading,
.modal-error { text-align:center; padding: 40px; }

/* 详情图片 */
.modal-image {
  width: 100%;
  height: 300px;
  margin-bottom: 16px;
  border-radius: 6px;
  overflow: hidden;
  background: #fafafa;
}
.modal-image img { width:100%; height:100%; object-fit:cover; }

/* 详情文本 */
.modal-info p { margin: 8px 0; color: #333; }

/* 能力文本 */
.ability-full { margin: 18px 0; }
.ability-text {
  white-space: pre-wrap;
  background: #fbfcfe;
  padding: 14px;
  border-radius: 6px;
  font-family: inherit;
  color: #333;
}

/* 版本信息 / 历史 */
.version-info { margin-top: 12px; }
.version-history { margin-top: 12px; }
.version-list { display:flex; flex-direction:column; gap:8px; margin-top:8px; }
.version-item {
  display:flex; gap:12px; align-items:center;
  padding:8px; border-radius:6px; background:#fff; border:1px solid #f0f3f7;
}
.version-item.current { background: linear-gradient(90deg, rgba(37,99,235,0.06), rgba(37,99,235,0.02)); border-color: rgba(37,99,235,0.12); }
.version-number { font-weight:700; color:#1f2937; margin-right:6px; }
.version-desc { color:#4b5563; flex:1; }
.version-time, .version-age { font-size:12px; color:#9ca3af; }

/* 编辑表单基础 */
.edit-modal .modal-body { max-height: 70vh; overflow-y:auto; }
.edit-form .form-group { margin-bottom: 12px; }
.form-row { display:flex; gap:12px; }
.form-row .form-group { flex:1; }
.form-label { display:block; margin-bottom:6px; font-weight:600; color:#374151; }
.form-input, .form-select, .form-textarea {
  width:100%; padding:8px 10px; border:1px solid #e6ecf5; border-radius:6px; font-size:14px; background:#fff;
}
.form-textarea { min-height:100px; resize:vertical; }
.form-hint { font-size:12px; color:#6b7280; margin-top:6px; }

/* 上传 / 预览 UI（编辑内使用） */
.image-upload-area { display:flex; gap:10px; align-items:center; }
.upload-btn {
  display:inline-flex; align-items:center; gap:8px; padding:8px 12px; border-radius:8px;
  background:#eef2ff; color:#1e3a8a; border:1px dashed rgba(30,58,138,0.12); cursor:pointer;
}
.upload-btn .upload-icon { font-size:18px; }
.file-input { display:none; }
.image-preview img { max-width:120px; max-height:120px; border-radius:6px; object-fit:cover; }
.remove-btn { background:rgba(0,0,0,0.6); color:#fff; border:none; border-radius:50%; width:28px; height:28px; cursor:pointer; }

/* 武器图片展示与编辑预览 */
.weapon-list { display:flex; gap:8px; flex-wrap:wrap; margin-top:8px; }
.weapon-item { width:80px; height:80px; border:1px solid #e6ebf3; border-radius:6px; overflow:hidden; cursor:pointer; background:#fff; display:flex; align-items:center; justify-content:center; }
.weapon-item img { width:100%; height:100%; object-fit:cover; }

.weapon-previews { display:flex; gap:8px; margin-top:8px; flex-wrap:wrap; }
.weapon-preview { position:relative; width:80px; height:80px; border:1px solid #e6ebf3; border-radius:6px; overflow:hidden; background:#fff; }
.weapon-preview img { width:100%; height:100%; object-fit:cover; }
.weapon-preview button {
  position:absolute; top:4px; right:4px; background:#ef4444; color:#fff; border:none; border-radius:50%; width:22px; height:22px; cursor:pointer;
}

/* 预览图 */
.preview-image { max-width:200px; max-height:200px; border-radius:8px; margin-top:8px }

/* 简化按钮样式 */
.btn-primary { background:#007bff; color:#fff; padding:8px 12px; border-radius:6px; border:none; cursor:pointer; }
.btn-secondary { background:#6c757d; color:#fff; padding:8px 12px; border-radius:6px; border:none; cursor:pointer; }
.btn-edit { background:#28a745; color:#fff; padding:8px 12px; border-radius:6px; border:none; cursor:pointer; }

/* 所有者徽章（详情） */
.owner-badge-modal { background:#ffd700; color:#8b6914; padding:6px 10px; border-radius:8px; font-weight:700; }

/* 调试按钮 */
.debug-btn { background:#ffc107; color:#212529; padding:6px 10px; border-radius:6px; border:none; cursor:pointer; }

/* 响应式 */
@media (max-width: 768px) {
  .search-filters { flex-direction:column; align-items:stretch; gap:8px; }
  .oc-list { grid-template-columns: 1fr; }
  .modal { width: 96%; }
  .form-row { flex-direction:column; }
}

/* 小交互 */
.oc-card:active { transform:none; }
</style>