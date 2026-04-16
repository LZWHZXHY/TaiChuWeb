<template>
  <div class="md-doc-viewer">
    
    <div class="doc-toolbar">
      <div class="toolbar-left">
        <span class="node-id">节点编号: {{ node.id }}</span>
        <span class="status-chip" :class="{ 'pending': node.status === 2 }">
          {{ node.status === 2 ? '待审核' : '已发布' }}
        </span>
      </div>
      <div class="toolbar-right">
        <template v-if="!isEditing">
          <button class="md-btn primary" @click="startEdit">编辑文档</button>
          <button class="md-btn danger-text" @click="deleteNode">删除</button>
        </template>
        <template v-else>
          <button class="md-btn text" @click="cancelEdit">取消</button>
          <button class="md-btn primary" @click="saveNode" :disabled="submitting">
            {{ submitting ? '保存中...' : '保存变更' }}
          </button>
        </template>
      </div>
    </div>

    <div class="doc-content-scroll md-scroll">
      
      <div v-if="!isEditing" class="md-card read-mode">
        
        <div class="doc-header">
          <div class="header-main">
            <h1 class="doc-title">{{ node.name }}</h1>
            <div class="meta-row">
              <span class="meta-item"><span class="label">类型:</span> {{ node.type }}</span>
              <span class="meta-item author-box">
                <span class="label">作者:</span>
                <GenericAvatar v-if="node.author_id" :userId="node.author_id" :allowLink="true" :showLevel="false" class="tiny-avatar" />
                <span class="author-name">{{ node.author || '系统' }}</span>
              </span>
              <span class="meta-item"><span class="label">更新于:</span> {{ formatDate(node.updateTime) }}</span>
            </div>
          </div>
          
          <div class="header-image" v-if="displayImages.length > 0">
            <img :src="displayImages[0]" @click="openLightbox(displayImages[0])" @error="handleImgError" />
          </div>
        </div>

        <div class="md-divider"></div>

        <section class="doc-section">
          <h3 class="section-title">文档描述</h3>
          <p class="doc-desc">{{ node.description || '暂无详细描述...' }}</p>
        </section>

        <section class="doc-section" v-if="hasMetaData">
          <h3 class="section-title">属性档案</h3>
          <div class="json-wrapper">
            <JsonTreeViewer :data="parsedMeta" />
          </div>
        </section>

        <section class="doc-section" v-if="displayImages.length > 0">
          <h3 class="section-title">影像资料</h3>
          <div class="image-grid">
            <div v-for="(img, idx) in displayImages" :key="idx" class="img-card" @click="openLightbox(img)">
              <img :src="img" loading="lazy" @error="handleImgError" />
            </div>
          </div>
        </section>

      </div>

      <div v-else class="edit-mode">
        
        <div class="md-card">
          <h3 class="card-title">基础信息</h3>
          <div class="form-grid">
            <div class="form-group">
              <label>文档名称</label>
              <input v-model="editForm.name" class="md-input" placeholder="输入名称" />
            </div>
            
            <div class="form-group">
              <label>类型标签</label>
              <input v-model="editForm.type" class="md-input" list="type-options" placeholder="选择或输入" />
              <datalist id="type-options">
                <option v-for="t in existingTypes" :key="t" :value="t" />
              </datalist>
            </div>

            <div class="form-group">
              <label>隶属父节点</label>
              <select v-model="editForm.parentId" class="md-select">
                <option :value="null">-- 作为根节点 --</option>
                <option v-for="p in availableParents" :key="p.id" :value="p.id">
                  {{ p.name }} (ID:{{ p.id }})
                </option>
              </select>
            </div>

            <div class="form-group search-group">
              <label>关联作者</label>
              <div class="search-box" v-if="!editForm.author">
                <input v-model="authorSearchQuery" @input="handleAuthorSearch" class="md-input" placeholder="搜索用户名或ID..." />
                
                <ul v-if="authorSearchResults.length > 0" class="search-dropdown md-scroll">
                  <li v-for="user in authorSearchResults" :key="user.id" @click="selectAuthor(user)">
                    <GenericAvatar :userId="user.id" :passedAvatar="user.avatar" :showLevel="false" class="tiny-avatar" />
                    <div class="user-info">
                      <span class="name">{{ user.nickname || user.name }}</span>
                      <span class="id">ID: {{ user.id }}</span>
                    </div>
                  </li>
                </ul>
                <div v-else-if="isSearchingAuthor" class="search-loading">检索中...</div>
              </div>
              <div v-else class="selected-author">
                <GenericAvatar :userId="editForm.author_id" :showLevel="false" class="tiny-avatar" />
                <span>{{ editForm.author }}</span>
                <button @click="clearAuthor" class="clear-btn">×</button>
              </div>
            </div>
          </div>
        </div>

        <div class="md-card">
          <div class="card-header-flex">
            <h3 class="card-title">属性拓展</h3>
            <button class="md-btn text small" @click="addRootProperty">+ 添加属性</button>
          </div>
          <div class="props-editor">
            <PropertyItem v-for="(item, index) in editForm.propsList" :key="index" v-model="editForm.propsList[index]" @delete="removeRootProperty(index)" />
          </div>
        </div>

        <div class="md-card">
          <h3 class="card-title">影像管理</h3>
          <div class="upload-area">
            <button class="md-btn outlined" @click="$refs.fileInput.click()" :disabled="uploading">
              {{ uploading ? '上传中...' : '点击上传图片' }}
            </button>
            <input type="file" ref="fileInput" style="display:none" accept="image/*" @change="handleFileUpload" />
          </div>
          <div class="preview-grid">
            <div v-for="(url, idx) in displayImages" :key="idx" class="preview-item">
              <img :src="url" @error="handleImgError" />
              <button class="delete-img-btn" @click="handleRemoveImage(url)">×</button>
            </div>
          </div>
        </div>

        <div class="md-card">
          <NodeRelationPanel :currentNode="node" :allNodes="allNodes" @select-node="$emit('select-node', $event)" />
        </div>

        <div class="md-card">
          <h3 class="card-title">详细描述</h3>
          <textarea v-model="editForm.description" class="md-textarea" rows="8" placeholder="在此输入文档正文..."></textarea>
        </div>

      </div>
    </div>

    <div v-if="lightboxImage" class="md-lightbox" @click="closeLightbox">
      <div class="lightbox-wrapper">
        <img :src="lightboxImage" />
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'
import JsonTreeViewer from './JsonTreeViewer.vue' 
import PropertyItem from './PropertyItem.vue'     
import NodeRelationPanel from './NodeRelationPanel.vue' 
import GenericAvatar from '@/GeneralComponents/UserAvatar.vue' 
import apiClient from '@/utils/api'

const props = defineProps({
  node: { type: Object, required: true },
  allNodes: { type: Array, default: () => [] } 
})
const emit = defineEmits(['update-node', 'delete-node', 'select-node'])

const isEditing = ref(false)
const submitting = ref(false)
const uploading = ref(false)
const fileInput = ref(null)
const lightboxImage = ref(null)
const localImages = ref([])

const authorSearchQuery = ref('')
const authorSearchResults = ref([])
const isSearchingAuthor = ref(false)
let searchTimeout = null

const editForm = reactive({
  id: null, name: '', type: '', author: '', author_id: null, description: '', parentId: null, propsList: [] 
})

const parseGalleryData = (raw) => {
  if (!raw || typeof raw !== 'string') return []
  let finalUrls = []
  if (raw.includes('[') && raw.includes(']')) {
    try {
      const jsonPart = raw.match(/\[.*\]/)
      if (jsonPart) {
        const parsed = JSON.parse(jsonPart[0])
        finalUrls = Array.isArray(parsed) ? parsed : [parsed]
      }
    } catch (e) {
      const regex = /https?:\/\/[^\s"\\\]]+/g
      finalUrls = raw.match(regex) || []
    }
  } else {
    finalUrls = [raw]
  }
  return finalUrls.map(url => {
    if (!url || typeof url !== 'string') return ''
    let clean = url.replace(/\\/g, '')
    if (clean.startsWith('http')) return clean
    return `https://img.bianyuzhou.com/${clean.replace(/^\/+/, '')}`
  }).filter(url => url.length > 5)
}

const displayImages = computed(() => {
  if (isEditing.value && localImages.value.length > 0) return localImages.value
  return parseGalleryData(props.node.image_url)
})

const availableParents = computed(() => props.allNodes.filter(n => n.id !== props.node.id))
const existingTypes = computed(() => [...new Set(props.allNodes.map(n => n.type))].filter(t => t && t !== '待定'))
const parsedMeta = computed(() => {
  if (!props.node.meta_data_json) return {}
  try { return (typeof props.node.meta_data_json === 'string') ? JSON.parse(props.node.meta_data_json) : props.node.meta_data_json } catch { return {} }
})
const hasMetaData = computed(() => Object.keys(parsedMeta.value).length > 0)

const handleImgError = (e) => e.target.src = 'https://img.bianyuzhou.com/uploads/ip_assets/default.png'
const formatDate = (t) => t ? new Date(t).toLocaleString() : '暂无'

const jsonToTree = (jsonObj) => {
  if (!jsonObj || typeof jsonObj !== 'object') return []
  return Object.keys(jsonObj).map(key => {
    const val = jsonObj[key]
    return (val && typeof val === 'object' && !Array.isArray(val)) ? { key, children: jsonToTree(val) } : { key, value: val }
  })
}
const treeToJson = (treeArr) => {
  const result = {}
  treeArr.forEach(item => { if (item.key) result[item.key] = item.children ? treeToJson(item.children) : item.value })
  return result
}

const openLightbox = (url) => { lightboxImage.value = url }
const closeLightbox = () => { lightboxImage.value = null }

const handleAuthorSearch = () => {
  clearTimeout(searchTimeout)
  if (!authorSearchQuery.value.trim()) {
    authorSearchResults.value = []; isSearchingAuthor.value = false; return
  }
  isSearchingAuthor.value = true
  searchTimeout = setTimeout(async () => {
    try {
      const res = await apiClient.get('/userinfo/search', { params: { keyword: authorSearchQuery.value } })
      authorSearchResults.value = (res.data && res.data.success) ? (res.data.data || []) : []
    } catch (e) { console.error(e) } finally { isSearchingAuthor.value = false }
  }, 400)
}

const selectAuthor = (user) => {
  editForm.author = user.nickname || user.name
  editForm.author_id = user.id
  authorSearchQuery.value = ''; authorSearchResults.value = []
}
const clearAuthor = () => { editForm.author = ''; editForm.author_id = null }

const startEdit = () => {
  const n = props.node
  localImages.value = parseGalleryData(n.image_url)
  Object.assign(editForm, { 
    id: n.id, name: n.name, type: n.type, author: n.author, author_id: n.author_id || null, 
    description: n.description || '', parentId: (n.parentId !== undefined && n.parentId !== 0) ? n.parentId : null, 
    propsList: jsonToTree(parsedMeta.value) 
  })
  isEditing.value = true
}

const cancelEdit = () => { isEditing.value = false; localImages.value = [] }

const addRootProperty = () => editForm.propsList.push({ key: '', value: '' })
const removeRootProperty = (index) => editForm.propsList.splice(index, 1)

const handleFileUpload = async (e) => {
  const file = e.target.files[0]; if (!file) return;
  uploading.value = true
  try {
    const fd = new FormData(); fd.append('file', file)
    const res = await apiClient.post(`/Setting/${props.node.id}/image`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
    const responseData = res.data || res; 
    if (responseData && responseData.allImages) localImages.value = parseGalleryData(JSON.stringify(responseData.allImages))
    emit('select-node', props.node) 
  } catch (e) { alert("上传失败") } finally { uploading.value = false }
}

const handleRemoveImage = async (url) => {
  if(!confirm("确定要删除此图片吗？")) return
  try {
    const res = await apiClient.delete(`/Setting/${props.node.id}/image`, { params: { imageUrl: url } })
    const responseData = res.data || res; 
    if (responseData && responseData.allImages) {
      localImages.value = parseGalleryData(JSON.stringify(responseData.allImages))
    } else {
       localImages.value = localImages.value.filter(u => u !== url)
    }
    emit('select-node', props.node)
  } catch (e) { alert("删除失败") }
}

const saveNode = () => {
  submitting.value = true
  const metaStr = JSON.stringify(treeToJson(editForm.propsList))
  const payload = { 
    id: editForm.id, name: editForm.name, type: editForm.type, author: editForm.author, 
    author_id: editForm.author_id, description: editForm.description, parentId: editForm.parentId, 
    meta_data_json: metaStr, metaStr: metaStr 
  }
  emit('update-node', payload, () => {
    submitting.value = false; isEditing.value = false; localImages.value = []
  })
}

const deleteNode = () => { emit('delete-node', props.node.id) }

watch(() => props.node.id, () => { isEditing.value = false; localImages.value = [] })
</script>

<style scoped>
/* Material Design 风格 */
.md-doc-viewer {
  width: 100%; height: 100%;
  display: flex; flex-direction: column;
  background: #F5F7FA;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #2C3E50;
}

/* 顶部工具栏 */
.doc-toolbar {
  height: 56px; background: #FFF; border-bottom: 1px solid #E0E0E0;
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 24px; flex-shrink: 0;
}
.toolbar-left { display: flex; align-items: center; gap: 12px; }
.node-id { font-size: 13px; color: #7F8C8D; font-family: monospace; }
.status-chip { font-size: 12px; background: #E3F2FD; color: #1976D2; padding: 4px 8px; border-radius: 4px; }
.status-chip.pending { background: #FFF3E0; color: #E65100; }

.toolbar-right { display: flex; gap: 12px; }

/* 按钮规范 */
.md-btn {
  border: none; border-radius: 4px; padding: 6px 16px; font-size: 14px; font-weight: 500; cursor: pointer; transition: 0.2s;
}
.md-btn.primary { background: #1976D2; color: #FFF; }
.md-btn.primary:hover:not(:disabled) { background: #1565C0; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.md-btn.primary:disabled { background: #90CAF9; cursor: not-allowed; }
.md-btn.text { background: transparent; color: #555; }
.md-btn.text:hover { background: rgba(0,0,0,0.04); }
.md-btn.danger-text { background: transparent; color: #D32F2F; }
.md-btn.danger-text:hover { background: #FFEBEE; }
.md-btn.outlined { background: transparent; border: 1px solid #1976D2; color: #1976D2; }
.md-btn.outlined:hover { background: #E3F2FD; }
.md-btn.small { padding: 4px 8px; font-size: 12px; }

/* 内容滚动区 */
.doc-content-scroll { flex: 1; overflow-y: auto; padding: 24px; }
.md-card { background: #FFF; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.08); padding: 32px; margin-bottom: 24px; max-width: 900px; margin-left: auto; margin-right: auto; }

/* 阅读模式 */
.doc-header { display: flex; justify-content: space-between; gap: 32px; }
.header-main { flex: 1; }
.doc-title { font-size: 32px; font-weight: bold; margin: 0 0 16px 0; color: #111; line-height: 1.2; }
.meta-row { display: flex; gap: 24px; flex-wrap: wrap; font-size: 13px; color: #666; }
.meta-item { display: flex; align-items: center; gap: 6px; }
.meta-item .label { color: #999; }
.tiny-avatar { width: 24px; height: 24px; }
.author-name { font-weight: 500; color: #333; }
.header-image { width: 140px; height: 140px; border-radius: 8px; overflow: hidden; border: 1px solid #E0E0E0; cursor: pointer; }
.header-image img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; }
.header-image:hover img { transform: scale(1.05); }

.md-divider { height: 1px; background: #E0E0E0; margin: 32px 0; }

.doc-section { margin-bottom: 32px; }
.section-title { font-size: 16px; font-weight: 600; margin: 0 0 16px 0; color: #333; border-left: 4px solid #1976D2; padding-left: 12px; }
.doc-desc { font-size: 15px; line-height: 1.8; color: #444; white-space: pre-wrap; }
.json-wrapper { background: #FAFAFA; padding: 16px; border-radius: 6px; border: 1px solid #E0E0E0; }

.image-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 16px; }
.img-card { border-radius: 6px; overflow: hidden; border: 1px solid #E0E0E0; cursor: pointer; aspect-ratio: 1; }
.img-card img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; }
.img-card:hover img { transform: scale(1.05); opacity: 0.9; }

/* 编辑模式 */
.card-title { font-size: 16px; font-weight: 600; margin: 0 0 20px 0; color: #111; }
.card-header-flex { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.card-header-flex .card-title { margin: 0; }

.form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 20px; }
.form-group label { display: block; font-size: 13px; color: #666; margin-bottom: 8px; font-weight: 500; }
.md-input, .md-select, .md-textarea {
  width: 100%; box-sizing: border-box; border: 1px solid #DCDFE6; border-radius: 4px; padding: 10px 12px; font-size: 14px; outline: none; transition: 0.2s; background: #FFF;
}
.md-input:focus, .md-select:focus, .md-textarea:focus { border-color: #1976D2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }

/* 下拉检索 */
.search-group { position: relative; }
.search-dropdown { position: absolute; top: 100%; left: 0; width: 100%; background: #FFF; border: 1px solid #E0E0E0; border-radius: 4px; max-height: 200px; z-index: 10; box-shadow: 0 4px 12px rgba(0,0,0,0.1); list-style: none; padding: 0; margin: 4px 0 0 0; }
.search-dropdown li { display: flex; align-items: center; padding: 10px 12px; gap: 12px; cursor: pointer; border-bottom: 1px solid #F0F0F0; }
.search-dropdown li:hover { background: #F5F7FA; }
.search-dropdown .user-info { display: flex; flex-direction: column; }
.search-dropdown .name { font-size: 14px; color: #333; }
.search-dropdown .id { font-size: 12px; color: #999; }
.search-loading { padding: 10px; font-size: 13px; color: #999; background: #FFF; border: 1px solid #E0E0E0; border-radius: 4px; margin-top: 4px; }

.selected-author { display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: #F5F7FA; border-radius: 4px; border: 1px solid #E0E0E0; }
.clear-btn { margin-left: auto; background: none; border: none; font-size: 16px; color: #999; cursor: pointer; }
.clear-btn:hover { color: #D32F2F; }

/* 图像上传预览 */
.upload-area { margin-bottom: 16px; }
.preview-grid { display: flex; flex-wrap: wrap; gap: 12px; }
.preview-item { width: 100px; height: 100px; border-radius: 6px; border: 1px solid #E0E0E0; position: relative; overflow: hidden; }
.preview-item img { width: 100%; height: 100%; object-fit: cover; }
.delete-img-btn { position: absolute; top: 4px; right: 4px; width: 24px; height: 24px; background: rgba(0,0,0,0.6); color: #FFF; border: none; border-radius: 50%; cursor: pointer; font-size: 14px; display: flex; align-items: center; justify-content: center; opacity: 0; transition: 0.2s; }
.preview-item:hover .delete-img-btn { opacity: 1; }
.delete-img-btn:hover { background: #D32F2F; }

/* 灯箱与滚动条 */
.md-lightbox { position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 1000; display: flex; justify-content: center; align-items: center; }
.lightbox-wrapper { max-width: 90vw; max-height: 90vh; }
.lightbox-wrapper img { max-width: 100%; max-height: 100%; border-radius: 4px; box-shadow: 0 4px 20px rgba(0,0,0,0.5); }

.md-scroll::-webkit-scrollbar { width: 6px; }
.md-scroll::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.15); border-radius: 3px; }
.md-scroll::-webkit-scrollbar-thumb:hover { background: rgba(0,0,0,0.3); }
</style>