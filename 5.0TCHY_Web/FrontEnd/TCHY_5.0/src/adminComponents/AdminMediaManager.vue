<template>
  <div class="admin-panel">
    <header class="admin-header">
      <div class="header-left">
        <h2>影音矩阵控制台 // ADMIN_OVERRIDE</h2>
        <span class="system-status">SYSTEM_READY</span>
      </div>
      <button class="cyber-btn primary" @click="openAddModal">+ 录入新信号</button>
    </header>

    <div class="table-container custom-scroll">
      <table class="cyber-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>封面</th>
            <th>标题与链接</th>
            <th>类型</th>
            <th>标签</th>
            <th>评分</th>
            <th>权重</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in mediaList" :key="item.id">
            <td>{{ item.id }}</td>
            <td><img :src="item.coverUrl" class="table-cover" alt="cover"/></td>
            <td>
              <div class="title-text">{{ item.title }}</div>
              <a :href="item.targetUrl" target="_blank" class="link-preview">▶ SOURCE_LINK</a>
            </td>
            <td><span class="type-tag">{{ item.mediaType }}</span></td>
            <td>
              <UniversalTags :tags="parseTags(item.tags)" class="admin-tags-display" />
            </td>
            <td>{{ item.rating }}</td>
            <td>{{ item.weight }}</td>
            <td>
              <span :class="['status-dot', item.status === 0 ? 'online' : 'offline']"></span>
              {{ item.status === 0 ? '展示中' : '已下架' }}
            </td>
            <td class="action-cell">
              <button class="cyber-btn small" @click="openEditModal(item)">覆写</button>
              <button class="cyber-btn small danger" @click="deleteItem(item.id)">抹除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content custom-scroll">
        <div class="modal-header">
          <h3>{{ isEdit ? '覆写档案信息 // EDIT_MODE' : '录入新信号 // NEW_ENTRY' }}</h3>
          <span class="close-x" @click="closeModal">×</span>
        </div>
        
        <form @submit.prevent="submitForm" class="cyber-form">
          <div class="form-row-2">
            <div class="form-group">
              <label>标题 (Title)</label>
              <input v-model="formData.title" required placeholder="输入作品名..." />
            </div>
            <div class="form-group">
              <label>类型 (Media Type)</label>
              <select v-model="formData.mediaType" class="cyber-select">
                <option value="ANIME">ANIME (动画)</option>
                <option value="MOVIE">MOVIE (电影)</option>
                <option value="VIDEO">VIDEO (短片)</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label>▶ 外部跳转链接 (Target URL)</label>
            <input v-model="formData.targetUrl" placeholder="https://www.bilibili.com/video/..." required />
          </div>

          <div class="form-group">
            <label>封面海报 (Cover Image)</label>
            <div class="cover-upload-box">
              <div class="preview-wrapper">
                <img v-if="formData.coverUrl" :src="formData.coverUrl" class="cover-preview" alt="Preview"/>
                <div v-else class="preview-placeholder">NO_IMAGE</div>
              </div>
              <div class="upload-controls">
                <input type="file" ref="fileInput" @change="handleFileUpload" accept="image/*" style="display: none;" />
                
                <button v-if="!isUploading" type="button" class="cyber-btn" @click="triggerFileInput">
                  选择本地文件直传 COS
                </button>
                
                <div v-else class="cyber-progress-wrapper">
                  <div class="progress-info">
                    <span>UPLOADING_SIGNAL...</span>
                    <span class="pct">{{ uploadProgress }}%</span>
                  </div>
                  <div class="progress-track">
                    <div class="progress-fill" :style="{ width: uploadProgress + '%' }"></div>
                  </div>
                </div>

                <div class="or-text">或手动修正直链:</div>
                <input v-model="formData.coverUrl" placeholder="https://img.bianyuzhou.com/..." required :disabled="isUploading" />
              </div>
            </div>
          </div>

          <div class="form-row-3">
            <div class="form-group">
              <label>评分 (Rating)</label>
              <input type="number" step="0.1" min="0" max="10" v-model="formData.rating" required />
            </div>
            <div class="form-group">
              <label>权重 (Weight)</label>
              <input type="number" v-model="formData.weight" />
            </div>
            <div class="form-group">
              <label>状态 (Status)</label>
              <select v-model="formData.status" class="cyber-select">
                <option :value="0">0 - 正常发布</option>
                <option :value="1">1 - 信号拦截(下架)</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label>特征标签 (System Tags)</label>
            <CyberTagInput 
              v-model="formData.tags" 
              :max-tags="8" 
            />
          </div>

          <div class="form-group">
            <label>描述引言 (Description)</label>
            <textarea v-model="formData.description" rows="3" placeholder="输入简短的推荐语..."></textarea>
          </div>

          <div class="modal-actions">
            <button type="button" class="cyber-btn" @click="closeModal">放弃更改</button>
            <button type="submit" class="cyber-btn primary" :disabled="isUploading">执行写入指令</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import apiClient from '@/utils/api'
import UniversalTags from '@/GeneralComponents/UniversalTags.vue'
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue' 

const mediaList = ref([])
const showModal = ref(false)
const isEdit = ref(false)
const fileInput = ref(null)

const isUploading = ref(false)
const uploadProgress = ref(0) 

const formData = ref({
  id: null,
  title: '',
  mediaType: 'ANIME',
  coverUrl: '',
  targetUrl: '',
  rating: 0,
  weight: 0,
  status: 0,
  tags: '',
  description: ''
})

const parseTags = (tagStr) => {
  if (!tagStr) return [];
  return tagStr.split(',').map(t => t.trim()).filter(t => t);
}

const fetchList = async () => {
  try {
    const res = await apiClient.get('/MediaRecommendation/admin/list')
    if (res.data.success) {
      // ⚡ 这里拿到的已经是小写驼峰的对象数组
      mediaList.value = res.data.data; 
    }
  } catch (error) { console.error('Signal Error:', error) }
}

const openAddModal = () => {
  isEdit.value = false
  formData.value = { id: null, title: '', mediaType: 'ANIME', coverUrl: '', targetUrl: '', rating: 0, weight: 0, status: 0, tags: '', description: '' }
  showModal.value = true
}

const openEditModal = (item) => {
  isEdit.value = true
  // ⚡ 修正映射逻辑：item 本身已经是小写驼峰，直接对应赋值
  formData.value = {
    id: item.id,
    title: item.title,
    mediaType: item.mediaType,
    coverUrl: item.coverUrl,
    targetUrl: item.targetUrl,
    rating: item.rating,
    weight: item.weight,
    status: item.status,
    tags: item.tags || '',
    description: item.description
  };
  showModal.value = true
}

const closeModal = () => { showModal.value = false }
const triggerFileInput = () => { if (fileInput.value) fileInput.value.click() }

const handleFileUpload = async (event) => {
  const file = event.target.files[0]
  if (!file) return
  isUploading.value = true
  uploadProgress.value = 0

  try {
    // 1. 获取预签名 URL
    const signRes = await apiClient.get(`/MediaRecommendation/admin/get-upload-url`, {
      params: { fileName: file.name }
    })
    if (!signRes.data.success) throw new Error('AUTH_FAILED')
    const { uploadUrl, finalUrl } = signRes.data.data

    // 2. 创建一个没有任何拦截器的干净实例
    const cleanAxios = axios.create(); 

    // 3. 直接上传，不再需要 transformRequest 里的 delete 操作
    await cleanAxios.put(uploadUrl, file, {
      headers: { 
        'Content-Type': file.type 
        // 这里千万不要写 Authorization
      },
      onUploadProgress: (p) => {
        if (p.lengthComputable) {
          uploadProgress.value = Math.round((p.loaded * 100) / p.total)
        }
      }
    })

    formData.value.coverUrl = finalUrl
  } catch (error) {
    console.error('Upload Error:', error)
    alert('上传失败')
  } finally {
    setTimeout(() => { isUploading.value = false }, 800)
    event.target.value = ''
  }
}

const submitForm = async () => {
  try {
    if (isEdit.value) {
      await apiClient.put(`/MediaRecommendation/admin/edit/${formData.value.id}`, formData.value)
    } else {
      await apiClient.post('/MediaRecommendation/admin/add', formData.value)
    }
    closeModal()
    fetchList() 
  } catch (error) { alert('执行失败') }
}

const deleteItem = async (id) => {
  if (!confirm('警告：确定要从矩阵中永久抹除该档案吗？')) return
  try {
    await apiClient.delete(`/MediaRecommendation/admin/delete/${id}`)
    fetchList()
  } catch (error) { alert('抹除指令执行失败') }
}

onMounted(() => fetchList())
</script>

<style scoped>
.admin-panel { padding: 2rem; background: #E0DDD5; min-height: 100vh; font-family: 'JetBrains Mono', 'PingFang SC', monospace; color: #111; }
.admin-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem; border-bottom: 4px solid #111; padding-bottom: 1rem; }
.header-left h2 { margin: 0; font-size: 1.8rem; letter-spacing: -1px; }
.system-status { font-size: 0.7rem; color: #22c55e; font-weight: bold; background: #111; padding: 2px 6px; margin-top: 5px; display: inline-block; }

.table-container { width: 100%; overflow-x: auto; background: #fff; border: 2px solid #111; box-shadow: 8px 8px 0 rgba(0,0,0,0.1); }
.cyber-table { width: 100%; border-collapse: collapse; white-space: nowrap; }
.cyber-table th { background: #111; color: #fff; text-align: left; padding: 1rem; font-size: 0.85rem; text-transform: uppercase; }
.cyber-table td { padding: 0.8rem 1rem; border-bottom: 1px solid #eee; vertical-align: middle; }
.cyber-table tr:hover { background: #f9f9f9; }

.table-cover { width: 45px; height: 62px; object-fit: cover; border: 1px solid #111; }
.title-text { font-weight: 800; font-size: 0.95rem; }
.link-preview { font-size: 0.65rem; color: #D92323; font-weight: bold; text-decoration: none; border-bottom: 1px dashed; }
.type-tag { background: #eee; border: 1px solid #999; padding: 2px 6px; font-size: 0.75rem; font-weight: bold; }

.status-dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 5px; }
.status-dot.online { background: #22c55e; box-shadow: 0 0 8px #22c55e; }
.status-dot.offline { background: #ef4444; }

.cyber-btn { background: transparent; border: 2px solid #111; padding: 0.6rem 1.2rem; font-weight: bold; cursor: pointer; transition: all 0.2s; font-family: inherit; }
.cyber-btn:hover { background: #111; color: #fff; transform: translate(-2px, -2px); box-shadow: 2px 2px 0 #D92323; }
.cyber-btn.primary { background: #111; color: #fff; }
.cyber-btn.danger { color: #D92323; border-color: #D92323; }
.cyber-btn.danger:hover { background: #D92323; color: #fff; box-shadow: 2px 2px 0 #111; }
.cyber-btn.small { padding: 0.3rem 0.6rem; font-size: 0.75rem; margin-right: 0.5rem; }
.cyber-btn:disabled { opacity: 0.5; cursor: not-allowed; filter: grayscale(1); }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.8); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal-content { background: #E0DDD5; padding: 2rem; width: 850px; max-width: 95vw; max-height: 90vh; border: 4px solid #111; box-shadow: 15px 15px 0 rgba(0,0,0,0.3); overflow-y: auto; }
.modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; border-bottom: 2px solid #111; padding-bottom: 0.5rem; }
.close-x { font-size: 1.5rem; cursor: pointer; font-weight: bold; }

.cyber-form .form-group { margin-bottom: 1.2rem; display: flex; flex-direction: column; }
.cyber-form .form-row-2 { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; }
.cyber-form .form-row-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1.5rem; }
.cyber-form label { font-size: 0.75rem; font-weight: 900; margin-bottom: 0.4rem; text-transform: uppercase; color: #555; }
.cyber-form input, .cyber-form textarea, .cyber-select { padding: 0.7rem; border: 2px solid #111; background: #fff; font-family: inherit; outline: none; width: 100%; box-sizing: border-box; }
.cyber-form input:focus, .cyber-select:focus { border-color: #D92323; background: #fffcfc; }

.cover-upload-box { display: flex; gap: 1.5rem; background: rgba(0,0,0,0.03); padding: 1.2rem; border: 2px dashed #999; align-items: flex-start; }
.preview-wrapper { width: 100px; height: 140px; background: #ccc; border: 2px solid #111; flex-shrink: 0; display: flex; align-items: center; justify-content: center; overflow: hidden; }
.cover-preview { width: 100%; height: 100%; object-fit: cover; }
.preview-placeholder { font-size: 0.6rem; font-weight: bold; color: #888; }
.upload-controls { flex: 1; display: flex; flex-direction: column; gap: 0.6rem; }
.or-text { font-size: 0.7rem; color: #777; margin: 0.2rem 0; }

.cyber-progress-wrapper { background: #111; padding: 0.6rem; border: 1px solid #444; }
.progress-info { display: flex; justify-content: space-between; color: #fff; font-size: 0.7rem; margin-bottom: 0.4rem; font-weight: bold; }
.progress-info .pct { color: #D92323; }
.progress-track { width: 100%; height: 6px; background: #333; overflow: hidden; }
.progress-fill { height: 100%; background: #D92323; transition: width 0.3s; position: relative; }

.custom-scroll::-webkit-scrollbar { width: 8px; height: 8px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; border: 2px solid #eee; }

:deep(.admin-tags-display .universal-tags-wrapper) { margin-top: 0 !important; padding: 0 !important; border-top: none !important; gap: 4px !important; }
:deep(.admin-tags-display .cyber-tag-chip) { padding: 1px 6px !important; font-size: 0.7rem !important; }
</style>