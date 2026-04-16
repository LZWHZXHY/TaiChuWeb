<template>
  <div class="md-oc-app-container">
    <nav class="md-app-bar">
      <div class="bar-content">
        <div class="nav-brand">
          <span class="brand-icon">◈</span>
          <span class="brand-title">Dimension_Archive</span>
        </div>
        
        <div class="md-segmented-control">
          <button :class="{ active: viewType === 'public' }" @click="viewType = 'public'">画廊</button>
          <button :class="{ active: viewType === 'mine' }" @click="viewType = 'mine'">档案库</button>
          <div class="glider" :style="gliderStyle"></div>
        </div>

        <button v-if="viewType === 'mine'" class="md-fab-mini" @click="triggerCreate">
          录入角色
        </button>
      </div>
    </nav>

    <main class="md-main-scroller md-scrollbar">
      <Transition name="md-fade-slide" mode="out-in">
        <div v-if="!showDetail" key="list-view" class="content-wrapper">
          <header class="section-header">
            <h1 class="page-title">{{ viewType === 'public' ? '公共画廊' : '个人档案馆' }}</h1>
            <p class="page-subtitle">CURRENT_RECORDS: {{ filteredList.length }}</p>
          </header>

          <div v-if="loading" class="md-loading-grid">加载中...</div>

          <div v-else-if="filteredList.length === 0" class="md-empty-state">
            <div class="empty-icon">🛰️</div>
            <h3>已准备就绪</h3>
            <p>尚未检测到生命信号，请点击右上角录入</p>
          </div>

          <div v-else class="md-document-grid">
            <article 
              v-for="oc in filteredList" 
              :key="oc.id" 
              class="md-archive-card" 
              @click="openOcDetail(oc)"
            >
              <div class="card-image-wrap">
                <img :src="getImageUrl(oc.currentVersion?.mainAvatarUrl)" @error="handleImgError" />
                <div class="card-badges">
                  <span class="type-tag">{{ oc.currentVersion?.sheetType || 'STD' }}</span>
                </div>
              </div>
              <div class="card-body">
                <span class="serial-no">UID.{{ oc.id }}</span>
                <h3 class="name">{{ oc.currentVersion?.name || '未知档案' }}</h3>
                <p class="tagline">{{ oc.currentVersion?.tagline || '暂无描述。' }}</p>
                <div class="card-footer">
                  <time class="update-time">{{ formatDate(oc.updatedAt) }}</time>
                  <div class="actions" v-if="viewType === 'mine'">
                    <button class="mini-btn edit" @click.stop="openEditModal(oc)">✏️</button>
                    <button class="mini-btn danger" @click.stop="deleteOc(oc.id)">🗑</button>
                  </div>
                </div>
              </div>
            </article>
          </div>
        </div>

        <div v-else key="detail-view" class="oc-detail-container">
          <nav class="detail-nav-bar" style="display: flex; justify-content: space-between; align-items: center;">
            <button class="back-btn" @click="showDetail = false">← BACK_TO_DATABASE</button>
            <button v-if="viewType === 'mine'" class="md-fab-mini" @click="openEditModal(activeOc)">
               ✏️ 编辑档案
            </button>
          </nav>

          <div class="detail-content-layout">
            <aside class="detail-visual">
              <div class="avatar-frame">
                <img :src="getImageUrl(activeOc.currentVersion?.mainAvatarUrl)" class="main-img" />
              </div>
            </aside>

            <section class="detail-info">
              <h1 class="oc-name">{{ activeOc.currentVersion?.name }}</h1>
              <p class="oc-tagline">“ {{ activeOc.currentVersion?.tagline }} ”</p>

              <div class="data-grid-section" v-if="getParsedJson(activeOc.currentVersion?.contentJson).props">
                <div v-for="(p, i) in getParsedJson(activeOc.currentVersion?.contentJson).props" :key="i" class="data-cell">
                  <span class="cell-label">{{ p.label }}</span>
                  <span class="cell-value">{{ p.value }}</span>
                </div>
              </div>

              <div class="story-section">
                <div class="story-block">
                  <h3>背景设定 // BIOGRAPHY</h3>
                  <p class="story-text">{{ getParsedJson(activeOc.currentVersion?.contentJson).story?.biography || '档案缺失' }}</p>
                </div>
              </div>
            </section>
          </div>
        </div>
      </Transition>
    </main>

    <Teleport to="body">
      <Transition name="md-modal">
        <div v-if="showCreateModal" class="md-scrim" @click.self="closeCreateModal">
          <div class="md-dialog deep-archive-modal">
            <header class="dialog-header">
              <div class="header-left">
                <span class="status-dot"></span>
                <h2>{{ editingId ? '档案更新 // ARCHIVE_UPDATE' : '档案初始化 // ARCHIVE_INIT' }}</h2>
              </div>
              <button class="close-btn" @click="closeCreateModal">×</button>
            </header>
            
            <nav class="modal-tabs">
              <button :class="{ active: activeTab === 'base' }" @click="activeTab = 'base'">基础特征</button>
              <button :class="{ active: activeTab === 'story' }" @click="activeTab = 'story'">设定与故事</button>
              <button :class="{ active: activeTab === 'assets' }" @click="activeTab = 'assets'">资产相册</button>
            </nav>

            <div class="dialog-content md-scrollbar">
              <div class="md-form-wrapper">
                <div v-show="activeTab === 'base'" class="tab-pane">
                  <div class="form-row">
                    <div class="md-field flex-2">
                      <label>角色全称 *</label>
                      <input v-model="createForm.Name" placeholder="角色名" />
                    </div>
                  </div>
                  <div class="md-field">
                    <label>识别寄语</label>
                    <input v-model="createForm.Tagline" placeholder="一句话简介" />
                  </div>
                  <div class="matrix-section">
                    <div class="properties-grid">
                      <div v-for="(item, key) in customProps" :key="key" class="prop-item">
                        <input class="prop-key" v-model="item.label" placeholder="属性" />
                        <input class="prop-val" v-model="item.value" placeholder="数值" />
                        <button class="mini-btn danger" @click="removeProp(key)" style="width: 44px; height: 44px;">-</button>
                      </div>
                      <button @click="addProp" class="add-prop-btn">+ 新增自定义字段</button>
                    </div>
                  </div>
                </div>

                <div v-show="activeTab === 'story'" class="tab-pane">
                  <div class="md-field">
                    <label>背景故事</label>
                    <textarea v-model="archiveContent.biography" rows="8" class="md-textarea"></textarea>
                  </div>
                </div>

                <div v-show="activeTab === 'assets'" class="tab-pane">
                  <div class="asset-manager">
                    <div class="main-avatar-side" @click="triggerUpload('main')">
                      <div class="hero-drop">
                        <img v-if="createForm.MainAvatarUrl" :src="getImageUrl(createForm.MainAvatarUrl)" />
                        <span v-else>点击上传主立绘</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <footer class="dialog-actions">
              <button class="btn-secondary" @click="closeCreateModal">取消</button>
              <button class="btn-primary" :disabled="submitting || !createForm.Name" @click="handleDeepSubmit">
                {{ submitting ? '执行中...' : (editingId ? '确认更新' : '确认录入') }}
              </button>
            </footer>
          </div>
        </div>
      </Transition>
    </Teleport>

    <input type="file" ref="fileInput" hidden @change="handleFileChange" accept="image/*" />
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'

// --- 基础状态 ---
const viewType = ref('public')
const showDetail = ref(false)
const showCreateModal = ref(false)
const loading = ref(false)
const submitting = ref(false)
const activeTab = ref('base')
const activeOc = ref(null)
const ocList = ref([])
const searchQuery = ref('')
const fileInput = ref(null)
const activeUploadType = ref('main')
const editingId = ref(null)

// --- 表单与内容 ---
const createForm = reactive({ Name: '', Tagline: '', MainAvatarUrl: '', SheetType: 'STANDARD', ContentJson: '{}' })
const customProps = ref([{ label: '性别', value: '' }, { label: '身高', value: '' }])
const archiveContent = reactive({ biography: '', personality: '' })
const album = ref([])

// --- 计算属性 ---
const gliderStyle = computed(() => ({
  left: viewType.value === 'public' ? '4px' : '82px'
}))

const filteredList = computed(() => {
  if (!searchQuery.value) return ocList.value
  const q = searchQuery.value.toLowerCase()
  return ocList.value.filter(oc => oc.currentVersion?.name?.toLowerCase().includes(q))
})

// --- 逻辑方法 ---
const fetchOcs = async () => {
  loading.value = true
  try {
    const endpoint = viewType.value === 'public' ? '/OC_Gallery/public' : '/OC_Gallery/mine'
    const res = await apiClient.get(endpoint)
    const payload = res.data || res
    ocList.value = viewType.value === 'public' ? (payload.data || []) : (payload || [])
  } catch (e) { console.error(e) } finally { loading.value = false }
}

const triggerUpload = (type) => { activeUploadType.value = type; fileInput.value.click() }

const handleFileChange = async (e) => {
  const file = e.target.files[0]
  if (!file) return
  const fd = new FormData(); fd.append('file', file)
  try {
    const res = await apiClient.post('/Drawing/upload', fd)
    if (activeUploadType.value === 'main') createForm.MainAvatarUrl = res.data
    else album.value.push({ url: res.data, desc: '' })
  } catch (e) { alert("上传失败") }
}

const resetForm = () => {
  editingId.value = null
  createForm.Name = ''
  createForm.Tagline = ''
  createForm.MainAvatarUrl = ''
  createForm.SheetType = 'STANDARD'
  customProps.value = [{ label: '性别', value: '' }, { label: '身高', value: '' }]
  archiveContent.biography = ''
  archiveContent.personality = ''
  album.value = []
  activeTab.value = 'base'
}

const triggerCreate = () => {
  resetForm()
  showCreateModal.value = true
}

const openEditModal = (oc) => {
  editingId.value = oc.id
  const cv = oc.currentVersion || {}
  
  createForm.Name = cv.name || ''
  createForm.Tagline = cv.tagline || ''
  createForm.MainAvatarUrl = cv.mainAvatarUrl || ''
  createForm.SheetType = cv.sheetType || 'STANDARD'
  
  const parsed = getParsedJson(cv.contentJson)
  customProps.value = parsed.props?.length ? parsed.props : [{ label: '性别', value: '' }, { label: '身高', value: '' }]
  archiveContent.biography = parsed.story?.biography || ''
  archiveContent.personality = parsed.story?.personality || ''
  album.value = parsed.album || []
  
  activeTab.value = 'base'
  showCreateModal.value = true
}

const handleDeepSubmit = async () => {
  submitting.value = true
  try {
    createForm.ContentJson = JSON.stringify({ props: customProps.value, story: archiveContent, album: album.value })
    
    if (editingId.value) {
      await apiClient.put(`/OC_Gallery/update/${editingId.value}`, createForm)
    } else {
      await apiClient.post('/OC_Gallery/submit', createForm)
    }
    
    closeCreateModal()
    
    if (showDetail.value) {
      showDetail.value = false
    }
    
    fetchOcs()
  } catch (e) { 
    console.error(e)
    alert(editingId.value ? "更新失败" : "录入失败") 
  } finally { 
    submitting.value = false 
  }
}

const deleteOc = async (id) => {
  if (!confirm("确定要永久抹除这个档案吗？此操作无法恢复！")) return
  try {
    await apiClient.delete(`/OC_Gallery/${id}`)
    if (showDetail.value && activeOc.value?.id === id) showDetail.value = false
    fetchOcs()
  } catch(e) { 
    alert("删除失败") 
  }
}

const openOcDetail = (oc) => { activeOc.value = oc; showDetail.value = true }
const closeCreateModal = () => { showCreateModal.value = false }
const addProp = () => customProps.value.push({ label: '', value: '' })
const removeProp = (index) => customProps.value.splice(index, 1)
const getParsedJson = (str) => { try { return JSON.parse(str || '{}') } catch { return {} } }
const getImageUrl = (url) => url ? (url.startsWith('http') ? url : `https://bianyuzhou.com/${url}`) : ''
const formatDate = (t) => t ? new Date(t).toLocaleDateString() : '-'
const handleImgError = (e) => e.target.src = 'https://img.bianyuzhou.com/uploads/ip_assets/default.png'

watch(viewType, fetchOcs)
onMounted(fetchOcs)
</script>

<style scoped>
/* ========== 全局基础 ========== */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.md-oc-app-container {
  min-height: 100vh;
  background: #F7F8FA;
  color: #1C2137;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  line-height: 1.5;
}

/* ========== 顶部导航栏 ========== */
.md-app-bar {
  height: 64px;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px) saturate(180%);
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
  position: sticky;
  top: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
}

.bar-content {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
}

.nav-brand {
  display: flex;
  align-items: center;
  gap: 10px;
}

.brand-icon {
  font-size: 18px;
  color: #0071E3;
}

.brand-title {
  font-size: 16px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

/* ========== 分段切换器 ========== */
.md-segmented-control {
  position: relative;
  display: flex;
  background: #E9EBF0;
  border-radius: 14px;
  padding: 4px;
  width: 160px;
}

.md-segmented-control button {
  flex: 1;
  border: none;
  background: transparent;
  padding: 8px 0;
  font-size: 14px;
  font-weight: 600;
  color: #8A8F98;
  cursor: pointer;
  z-index: 2;
  transition: color 0.2s ease;
}

.md-segmented-control button.active {
  color: #1C2137;
}

.glider {
  position: absolute;
  top: 4px;
  width: 74px;
  height: calc(100% - 8px);
  background: #FFF;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1;
}

/* ========== 按钮 ========== */
.md-fab-mini {
  background: #0071E3;
  color: #FFF;
  border: none;
  padding: 10px 18px;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
}

.md-fab-mini:hover {
  background: #0077ED;
  transform: translateY(-1px);
}

/* ========== 内容区域 ========== */
.content-wrapper {
  max-width: 1200px;
  margin: 0 auto;
  padding: 48px 24px;
}

.section-header {
  margin-bottom: 36px;
}

.page-title {
  font-size: 32px;
  font-weight: 700;
  margin-bottom: 8px;
}

.page-subtitle {
  font-size: 13px;
  color: #8A8F98;
  font-weight: 500;
  letter-spacing: 1px;
}

/* ========== 网格卡片 ========== */
.md-document-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 28px;
}

.md-archive-card {
  background: #FFF;
  border-radius: 20px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.04);
  cursor: pointer;
  transition: all 0.3s ease;
}

.md-archive-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.08);
  border-color: rgba(0, 113, 227, 0.15);
}

.card-image-wrap {
  height: 300px;
  background: #F1F3F6;
  position: relative;
  overflow: hidden;
}

.card-image-wrap img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.md-archive-card:hover img {
  transform: scale(1.04);
}

.card-badges {
  position: absolute;
  top: 12px;
  left: 12px;
}

.type-tag {
  background: rgba(0, 113, 227, 0.9);
  color: #FFF;
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 700;
}

.card-body {
  padding: 20px;
}

.serial-no {
  font-size: 11px;
  color: #0071E3;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.name {
  font-size: 20px;
  font-weight: 600;
  margin: 6px 0 4px;
}

.tagline {
  font-size: 13px;
  color: #6E7380;
  height: 36px;
  overflow: hidden;
  line-height: 1.4;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
}

.update-time {
  font-size: 11px;
  color: #8A8F98;
}

.actions {
  display: flex;
  gap: 8px;
}

.mini-btn {
  width: 30px;
  height: 30px;
  border-radius: 8px;
  border: none;
  background: #F1F3F6;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.mini-btn.edit {
  background: #EAF2FF;
  color: #0071E3;
}

.mini-btn.edit:hover {
  background: #0071E3;
  color: #FFF;
}

.mini-btn.danger {
  background: #FFF1F0;
  color: #FF3B30;
}

.mini-btn.danger:hover {
  background: #FF3B30;
  color: #FFF;
}

/* ========== 空状态 / 加载 ========== */
.md-loading-grid {
  text-align: center;
  padding: 60px 0;
  color: #8A8F98;
  font-size: 14px;
}

.md-empty-state {
  text-align: center;
  padding: 80px 20px;
  color: #6E7380;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 20px;
  opacity: 0.5;
}

.md-empty-state h3 {
  font-size: 18px;
  margin-bottom: 8px;
  color: #1C2137;
}

.md-empty-state p {
  font-size: 14px;
  max-width: 320px;
  margin: 0 auto;
}

/* ========== 详情页 ========== */
.oc-detail-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 48px 24px;
}

.detail-nav-bar {
  margin-bottom: 40px;
}

.back-btn {
  background: #E9EBF0;
  border: none;
  padding: 10px 20px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  color: #1C2137;
  cursor: pointer;
  transition: all 0.2s ease;
}

.back-btn:hover {
  background: #DCE1E8;
}

.detail-content-layout {
  display: flex;
  gap: 48px;
  align-items: flex-start;
}

.detail-visual {
  flex: 0 0 350px;
}

.avatar-frame img {
  width: 100%;
  border-radius: 24px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1);
}

.detail-info {
  flex: 1;
}

.oc-name {
  font-size: 36px;
  font-weight: 700;
  margin-bottom: 8px;
}

.oc-tagline {
  font-size: 16px;
  color: #6E7380;
  font-style: italic;
  margin-bottom: 32px;
}

.data-grid-section {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin: 32px 0;
}

.data-cell {
  background: #FFF;
  padding: 16px;
  border-radius: 14px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.cell-label {
  font-size: 11px;
  color: #0071E3;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  display: block;
  margin-bottom: 6px;
}

.cell-value {
  font-size: 16px;
  font-weight: 600;
  color: #1C2137;
}

.story-section {
  margin-top: 40px;
}

.story-block {
  background: #FFF;
  padding: 24px;
  border-radius: 16px;
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.story-block h3 {
  font-size: 14px;
  color: #0071E3;
  margin-bottom: 16px;
  font-weight: 700;
}

.story-text {
  font-size: 15px;
  line-height: 1.7;
  color: #3A3F52;
  white-space: pre-wrap;
}

/* ========== 弹窗 ========== */
.md-scrim {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.deep-archive-modal {
  background: #FFF;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  border-radius: 24px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 40px 120px rgba(0, 0, 0, 0.2);
}

.dialog-header {
  padding: 24px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #F1F3F6;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.status-dot {
  width: 10px;
  height: 10px;
  background: #34C759;
  border-radius: 50%;
}

.dialog-header h2 {
  font-size: 18px;
  font-weight: 700;
}

.close-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: none;
  background: #F1F3F6;
  font-size: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background: #DCE1E8;
}

/* 弹窗标签页 */
.modal-tabs {
  display: flex;
  gap: 32px;
  padding: 0 32px;
  border-bottom: 1px solid #F1F3F6;
}

.modal-tabs button {
  padding: 18px 0;
  border: none;
  background: transparent;
  color: #8A8F98;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  position: relative;
  transition: color 0.2s ease;
}

.modal-tabs button.active {
  color: #0071E3;
}

.modal-tabs button.active::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background: #0071E3;
  border-radius: 2px;
}

/* 弹窗内容 */
.dialog-content {
  flex: 1;
  overflow-y: auto;
  padding: 32px;
}

.md-form-wrapper {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.tab-pane {
  animation: fade 0.2s ease;
}

@keyframes fade {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.md-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 20px;
}

.md-field label {
  font-size: 13px;
  font-weight: 600;
  color: #3A3F52;
}

.md-field input,
.md-textarea {
  padding: 14px 16px;
  border-radius: 14px;
  border: 1px solid #DCE1E8;
  outline: none;
  font-size: 15px;
  transition: all 0.2s ease;
  background: #F9FAFB;
}

.md-field input:focus,
.md-textarea:focus {
  border-color: #0071E3;
  background: #FFF;
  box-shadow: 0 0 0 4px rgba(0, 113, 227, 0.1);
}

.md-textarea {
  resize: vertical;
  min-height: 120px;
  line-height: 1.6;
}

/* 属性表单 */
.properties-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 16px;
}

.prop-item {
  display: flex;
  gap: 12px;
}

.prop-key,
.prop-val {
  flex: 1;
  padding: 12px 14px;
  border-radius: 12px;
  border: 1px solid #DCE1E8;
  outline: none;
  font-size: 14px;
  background: #F9FAFB;
}

.add-prop-btn {
  padding: 12px;
  border-radius: 12px;
  border: 1px dashed #0071E3;
  background: transparent;
  color: #0071E3;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.add-prop-btn:hover {
  background: rgba(0, 113, 227, 0.05);
}

/* 上传区域 */
.asset-manager {
  display: flex;
  justify-content: center;
  padding: 20px 0;
}

.hero-drop {
  width: 220px;
  height: 300px;
  border: 2px dashed #DCE1E8;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s ease;
  color: #8A8F98;
  font-size: 14px;
  font-weight: 500;
}

.hero-drop:hover {
  border-color: #0071E3;
  background: rgba(0, 113, 227, 0.03);
}

.hero-drop img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* 弹窗按钮 */
.dialog-actions {
  padding: 24px 32px;
  border-top: 1px solid #F1F3F6;
  display: flex;
  justify-content: flex-end;
  gap: 16px;
}

.btn-secondary,
.btn-primary {
  padding: 12px 24px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
}

.btn-secondary {
  background: #E9EBF0;
  color: #3A3F52;
}

.btn-secondary:hover {
  background: #DCE1E8;
}

.btn-primary {
  background: #0071E3;
  color: #FFF;
}

.btn-primary:hover:not(:disabled) {
  background: #0077ED;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* ========== 滚动条 ========== */
.md-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.md-scrollbar::-webkit-scrollbar-track {
  background: #F1F3F6;
  border-radius: 10px;
}

.md-scrollbar::-webkit-scrollbar-thumb {
  background: #C9CDD4;
  border-radius: 10px;
}

.md-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #8A8F98;
}

/* ========== 过渡动画 ========== */
.md-fade-slide-enter-active,
.md-fade-slide-leave-active {
  transition: all 0.3s ease;
}

.md-fade-slide-enter-from {
  opacity: 0;
  transform: translateY(12px);
}

.md-fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-12px);
}

.md-modal-enter-active {
  transition: all 0.3s ease;
}

.md-modal-leave-active {
  transition: all 0.2s ease;
}

.md-modal-enter-from {
  opacity: 0;
  transform: scale(0.96);
}

.md-modal-leave-to {
  opacity: 0;
  transform: scale(1.04);
}

/* ========== 响应式适配 ========== */
@media (max-width: 768px) {
  .detail-content-layout {
    flex-direction: column;
    gap: 32px;
  }
  
  .detail-visual {
    flex: none;
    width: 100%;
    max-width: 350px;
    margin: 0 auto;
  }
  
  .data-grid-section {
    grid-template-columns: 1fr;
  }
  
  .modal-tabs {
    gap: 20px;
  }
  
  .bar-content {
    padding: 0 16px;
  }
  
  .content-wrapper,
  .oc-detail-container {
    padding: 32px 16px;
  }
}
</style>