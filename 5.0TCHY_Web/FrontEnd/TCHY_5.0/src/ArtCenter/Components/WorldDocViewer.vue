<template>
  <div class="cyber-doc-viewer">
    <div class="grid-bg moving-grid"></div>

    <div class="doc-paper-heavy">
      <div class="cyber-toolbar-heavy">
        <div class="tech-info-group">
          <span class="deco-box">■</span>
          <span class="node-id-label">NODE_ID: {{ node.id }}</span>
          <span class="status-badge" :class="{ 'pending': node.status === 2 }">
            // {{ node.status === 2 ? 'PENDING_REVIEW' : 'VERIFIED_STABLE' }}
          </span>
        </div>
        <div class="tool-actions">
          <template v-if="!isEditing">
            <button class="cyber-btn-rect primary-red" @click="startEdit">
              <span class="btn-text">EDIT_MODE</span>
            </button>
            <button class="cyber-btn-rect ghost-dark" @click="deleteNode">
              <span class="btn-text">删除节点</span>
            </button>
          </template>
          <template v-else>
            <button class="cyber-btn-rect ghost-dark" @click="cancelEdit">
              <span class="btn-text">退出</span>
            </button>
            <button class="cyber-btn-rect primary-black" @click="saveNode" :disabled="submitting">
              <span class="btn-text">{{ submitting ? 'SYNCING...' : '提交变更' }}</span>
            </button>
          </template>
        </div>
      </div>

      <div v-if="!isEditing" class="read-mode-scroll custom-scroll">
        <div class="doc-hero-section">
          <div class="hero-main">
            <h1 class="giant-doc-title glitch-hover" :data-text="node.name">{{ node.name }}</h1>
            <div class="meta-strip">
              <span class="meta-item"><span class="label">TYPE:</span> {{ node.type }}</span>
              <span class="meta-item"><span class="label">AUTH:</span> {{ node.author || 'SYSTEM' }}</span>
              <span class="meta-item"><span class="label">SYNC:</span> {{ formatDate(node.updateTime) }}</span>
              <span class="meta-item" v-if="node.parentId">
                <span class="label">PARENT_ID:</span> {{ node.parentId }}
              </span>
            </div>
          </div>
          <div class="hero-portrait" v-if="displayImages.length > 0">
            <div class="portrait-frame" @click="openLightbox(displayImages[0])">
               <img :src="displayImages[0]" @error="handleImgError" />
               <div class="scanline"></div>
            </div>
          </div>
        </div>

        <div class="divider-tech"><span>// INTEL_DATA_STREAM //</span></div>

        <section class="info-block-heavy">
          <div class="block-label">// DESCRIPTION_ARCHIVE</div>
          <div class="text-content-industrial">{{ node.description || '>> NO_DATA_TRANSMITTED_IN_THIS_NODE.' }}</div>
        </section>

        <section class="info-block-heavy" v-if="hasMetaData">
          <div class="block-label">// ATTRIBUTE_MATRIX</div>
          <div class="attr-matrix-inner">
            <JsonTreeViewer :data="parsedMeta" />
          </div>
        </section>

        <section class="info-block-heavy">
          <div class="block-label">// NEURAL_CONNECTIONS</div>
          <NodeRelationPanel :currentNode="node" :allNodes="allNodes" @select-node="$emit('select-node', $event)" />
        </section>

        <section class="info-block-heavy" v-if="displayImages.length > 0">
          <div class="block-label">// VISUAL_DATABASE (CLICK_TO_EXPAND)</div>
          <div class="cyber-gallery-grid">
            <div v-for="(img, idx) in displayImages" :key="idx" class="gallery-cell" @click="openLightbox(img)">
              <img :src="img" loading="lazy" />
              <div class="cell-deco">IMG_{{ idx + 1 }}</div>
            </div>
          </div>
        </section>
      </div>

      <div v-else class="edit-mode-scroll custom-scroll">
        
        <div class="edit-card-heavy">
          <div class="card-tag-black">BASIC_INPUT_BUFFER</div>
          <div class="form-grid-industrial">
            <div class="form-group-tech">
              <label class="input-label-tech">> IDENTIFIER_NAME</label>
              <input v-model="editForm.name" class="cyber-input-heavy" />
            </div>
            
            <div class="form-group-tech">
              <label class="input-label-tech">> NODE_CLASSIFICATION</label>
              <input v-model="editForm.type" class="cyber-input-heavy" list="type-options" />
              <datalist id="type-options">
                <option v-for="t in existingTypes" :key="t" :value="t" />
              </datalist>
            </div>

            <div class="form-group-tech">
              <label class="input-label-tech">> PARENT_NODE_LINK</label>
              <select v-model="editForm.parentId" class="cyber-select-heavy">
                <option :value="null">>> [ ROOT_NODE / NO_PARENT ]</option>
                <option v-for="p in availableParents" :key="p.id" :value="p.id">
                  ID:{{ p.id }} | {{ p.name }}
                </option>
              </select>
            </div>

            <div class="form-group-tech">
              <label class="input-label-tech">> ORIGIN_AUTHOR</label>
              <input v-model="editForm.author" class="cyber-input-heavy" />
            </div>
          </div>
        </div>

        <div class="edit-card-heavy">
          <div class="card-tag-black">META_DATA_CONFIGURATION</div>
          <div class="property-header-tech">
            <button class="cyber-btn-sm-heavy" @click="addRootProperty">+ ADD_ENTRY</button>
          </div>
          <div class="props-editor-industrial">
            <PropertyItem 
              v-for="(item, index) in editForm.propsList" 
              :key="index" 
              v-model="editForm.propsList[index]" 
              @delete="removeRootProperty(index)" 
            />
          </div>
        </div>

        <div class="edit-card-heavy">
          <div class="card-tag-black">VISUAL_ASSET_UPLOAD</div>
          <div class="upload-area-tech">
            <button class="cyber-btn-rect primary-red small" @click="$refs.fileInput.click()" :disabled="uploading">
              {{ uploading ? 'TRANSMITTING...' : 'UPLOAD_NEW_IMAGE' }}
            </button>
            <input type="file" ref="fileInput" style="display:none" accept="image/*" @change="handleFileUpload" />
          </div>
          
          <div class="edit-gallery-previews">
            <div v-for="(url, idx) in displayImages" :key="idx" class="preview-box">
              <img :src="url" class="img-contain" />
              <div class="img-delete-overlay" @click="handleRemoveImage(url)">
                <span class="delete-icon">×</span>
                <span class="delete-text">PURGE</span>
              </div>
            </div>
          </div>
        </div>

        <div class="edit-card-heavy">
          <div class="card-tag-black">DETAILED_LOG_CONTENT</div>
          <textarea v-model="editForm.description" class="cyber-textarea-heavy" rows="12"></textarea>
        </div>

      </div>
    </div>

    <div v-if="lightboxImage" class="cyber-lightbox" @click="closeLightbox">
      <div class="lightbox-content">
        <img :src="lightboxImage" />
        <div class="lightbox-close">CLICK_ANYWHERE_TO_CLOSE</div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'
import JsonTreeViewer from './JsonTreeViewer.vue' 
import PropertyItem from './PropertyItem.vue'     
import NodeRelationPanel from './NodeRelationPanel.vue' 
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

// 灯箱状态
const lightboxImage = ref(null)

// 🔥 修复1: 本地图片缓存，用于上传后即时预览
const localImages = ref([])

const editForm = reactive({
  id: null, name: '', type: '', author: '', description: '', parentId: null, propsList: [] 
})

// --- Computed ---
const availableParents = computed(() => props.allNodes.filter(n => n.id !== props.node.id))
const existingTypes = computed(() => [...new Set(props.allNodes.map(n => n.type))].filter(t => t && t !== '待定'))
const parsedMeta = computed(() => {
  if (!props.node.meta_data_json) return {}
  try { return (typeof props.node.meta_data_json === 'string') ? JSON.parse(props.node.meta_data_json) : props.node.meta_data_json } catch { return {} }
})
const hasMetaData = computed(() => Object.keys(parsedMeta.value).length > 0)

// 🔥 修复1 & 2: 统一的图片解析逻辑
// 优先使用 localImages (上传后)，如果为空则使用 props.node.image_url (初始加载)
const displayImages = computed(() => {
  let sourceList = []
  
  if (localImages.value.length > 0) {
    sourceList = localImages.value
  } else if (props.node.image_url) {
    try {
      const res = JSON.parse(props.node.image_url)
      sourceList = Array.isArray(res) ? res : [props.node.image_url]
    } catch { 
      sourceList = [] 
    }
  }

  return sourceList.map(url => url.startsWith('http') ? url : `https://bianyuzhou.com/${url}`)
})

// --- 辅助方法 ---
const handleImgError = (e) => e.target.src = '/土豆.jpg'
const formatDate = (t) => t ? new Date(t).toLocaleString() : 'N/A'
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

// 灯箱控制
const openLightbox = (url) => { lightboxImage.value = url }
const closeLightbox = () => { lightboxImage.value = null }

// --- 操作逻辑 ---
const startEdit = () => {
  const n = props.node
  // 初始化本地图片缓存，防止编辑模式下图片闪烁
  localImages.value = [] 
  
  Object.assign(editForm, { 
    id: n.id, 
    name: n.name, 
    type: n.type, 
    author: n.author, 
    description: n.description || '', 
    // 🔥 修复3: 确保 parentId 被正确读取，如果不存在则默认为 null
    parentId: (n.parentId !== undefined && n.parentId !== 0) ? n.parentId : null, 
    propsList: jsonToTree(parsedMeta.value) 
  })
  isEditing.value = true
}
const cancelEdit = () => {
  isEditing.value = false
  localImages.value = [] // 退出编辑清除缓存
}
const addRootProperty = () => editForm.propsList.push({ key: '', value: '' })
const removeRootProperty = (index) => editForm.propsList.splice(index, 1)

const handleFileUpload = async (e) => {
  const file = e.target.files[0]
  if (!file) return
  uploading.value = true
  try {
    const fd = new FormData(); fd.append('file', file)
    
    // 发送请求
    const res = await apiClient.post(`/Setting/${props.node.id}/image`, fd, { 
      headers: { 'Content-Type': 'multipart/form-data' } 
    })
    
    // 🔥 修复点：正确获取返回的图片列表并赋值给本地变量
    // 兼容可能存在的不同 axios 响应结构
    const responseData = res.data || res; 
    
    if (responseData && responseData.allImages) {
      localImages.value = responseData.allImages
    }

    alert("TRANSMISSION_COMPLETE: IMAGE_UPLOADED")
    
    // 通知父组件同步数据
    emit('select-node', props.node) 
  } catch (e) { 
    console.error(e)
    alert("UPLOAD_FAILED") 
  } finally { 
    uploading.value = false 
  }
}

// 删除图片
const handleRemoveImage = async (url) => {
  if(!confirm("WARNING: CONFIRM PERMANENT DELETION OF ASSET? \n确定要永久移除此图像资产吗？")) return
  
  try {
    const res = await apiClient.delete(`/Setting/${props.node.id}/image`, { params: { imageUrl: url } })
    
    // 🔥 修复1: 删除后同样更新本地列表
    if (res.data && res.data.allImages) {
      localImages.value = res.data.allImages
    } else {
       // 如果后端没返回 list，我们手动在本地剔除
       localImages.value = localImages.value.filter(u => u !== url)
    }

    alert("ASSET_TERMINATED: 图片已移除")
    emit('select-node', props.node)
  } catch (e) {
    console.error(e)
    alert("TERMINATION_FAILED: 删除失败")
  }
}

const saveNode = () => {
  submitting.value = true
  emit('update-node', { ...editForm, metaStr: JSON.stringify(treeToJson(editForm.propsList)) }, () => {
    submitting.value = false; isEditing.value = false; localImages.value = []
  })
}

const deleteNode = () => { if(confirm("EXECUTE TERMINATION PROTOCOL?")) emit('delete-node', props.node.id) }

// 监听 node 变化，清空本地缓存，确保切换节点时显示正确
watch(() => props.node.id, () => { 
  isEditing.value = false
  localImages.value = []
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.cyber-doc-viewer {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --body: 'Inter', sans-serif;
  
  width: 100%; height: 100%;
  position: relative; overflow: hidden;
  background: var(--off-white); color: var(--black); font-family: var(--body);
}

/* 网格背景动画 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

/* 主容器 */
.doc-paper-heavy {
  position: relative; z-index: 1;
  width: 100%; max-width: 1000px; height: 95%; margin: 20px auto;
  background: #fff; border: 3px solid var(--black);
  box-shadow: 12px 12px 0 rgba(0,0,0,0.15); display: flex; flex-direction: column;
}

/* 顶部技术工具条 */
.cyber-toolbar-heavy {
  height: 60px; background: var(--black); color: #fff;
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 20px; border-bottom: 4px solid var(--red); flex-shrink: 0;
}
.tech-info-group { display: flex; align-items: center; gap: 15px; font-family: var(--mono); font-size: 0.85rem; }
.deco-box { color: var(--red); }
.status-badge { background: #222; padding: 2px 8px; color: #00ff00; }
.status-badge.pending { color: #ffae00; }

/* 按钮通用样式 */
.cyber-btn-rect {
  border: none; padding: 8px 16px; cursor: pointer;
  font-family: var(--heading); font-size: 0.9rem; transition: 0.2s; position: relative;
}
.primary-red { background: var(--red); color: #fff; }
.primary-red:hover { background: #b91d1d; transform: translate(-2px, -2px); box-shadow: 4px 4px 0 rgba(0,0,0,0.2); }
.primary-black { background: #fff; color: var(--black); border: 1px solid var(--black); }
.primary-black:hover { background: var(--black); color: #fff; }
.ghost-dark { background: transparent; color: #fff; border: 1px solid #555; }
.ghost-dark:hover { border-color: var(--red); color: var(--red); }

/* --- 阅读模式内容 --- */
.read-mode-scroll { flex: 1; overflow-y: auto; padding: 40px; }

.doc-hero-section { display: flex; gap: 30px; margin-bottom: 40px; }
.hero-main { flex: 1; }
.giant-doc-title {
  font-family: var(--heading); font-size: 4rem; line-height: 0.9;
  text-transform: uppercase; margin: 0 0 20px 0; color: var(--black);
}
.meta-strip { 
  display: flex; gap: 20px; flex-wrap: wrap;
  font-family: var(--mono); font-size: 0.8rem; color: #666;
}
.meta-item .label { font-weight: 800; color: var(--black); }

/* 修复图片显示不全: 改用 contain + 黑色背景 */
.hero-portrait { width: 180px; height: 180px; flex-shrink: 0; }
.portrait-frame {
  width: 100%; height: 100%; border: 3px solid var(--black);
  position: relative; overflow: hidden; background: #111; /* 黑色底 */
  cursor: pointer;
}
.portrait-frame img { width: 100%; height: 100%; object-fit: contain; }
.scanline {
  position: absolute; top: 0; left: 0; width: 100%; height: 4px;
  background: rgba(255, 255, 255, 0.5); animation: scan 3s linear infinite; pointer-events: none;
}
@keyframes scan { 0% { top: -5%; } 100% { top: 105%; } }

.divider-tech {
  background: var(--black); color: #fff; text-align: center;
  font-family: var(--mono); font-size: 0.75rem; padding: 5px; margin: 30px 0; letter-spacing: 2px;
}

.info-block-heavy { margin-bottom: 40px; }
.block-label {
  background: var(--black); color: #fff; display: inline-block;
  padding: 4px 12px; font-family: var(--mono); font-size: 0.8rem;
  margin-bottom: 15px; font-weight: 700;
}
.text-content-industrial {
  font-size: 1.15rem; line-height: 1.8; color: #333;
  white-space: pre-wrap; padding-left: 15px; border-left: 4px solid var(--gray);
}

.cyber-gallery-grid {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 15px;
}
.gallery-cell {
  border: 1px solid var(--black); position: relative; overflow: hidden;
  background: #111; cursor: pointer;
}
/* 修复图片显示不全 */
.gallery-cell img { width: 100%; height: 200px; object-fit: contain; transition: 0.3s; }
.gallery-cell:hover img { transform: scale(1.02); opacity: 0.9; }
.cell-deco {
  position: absolute; bottom: 0; right: 0; background: var(--black);
  color: #fff; font-family: var(--mono); font-size: 0.6rem; padding: 2px 6px;
}

/* --- 编辑模式内容 --- */
.edit-mode-scroll { flex: 1; overflow-y: auto; padding: 30px; background: var(--off-white); }

.edit-card-heavy {
  background: #fff; border: 2px solid var(--black); margin-bottom: 25px; padding: 25px;
  box-shadow: 6px 6px 0 rgba(0,0,0,0.1); position: relative;
}
.card-tag-black {
  position: absolute; top: -12px; left: 15px; background: var(--black); color: #fff;
  padding: 2px 10px; font-family: var(--mono); font-size: 0.7rem;
}

.input-label-tech {
  display: block; font-family: var(--mono); font-weight: 800;
  font-size: 0.75rem; margin-bottom: 10px; color: #555;
}

.cyber-input-heavy, .cyber-select-heavy, .cyber-textarea-heavy {
  width: 100%; border: 2px solid var(--black); padding: 12px;
  font-family: var(--mono); font-weight: 700; outline: none;
  background: #fff; box-sizing: border-box;
}
.cyber-input-heavy:focus, .cyber-textarea-heavy:focus, .cyber-select-heavy:focus {
  background: #fdfdfd; border-color: var(--red); box-shadow: 4px 4px 0 rgba(0,0,0,0.05);
}

.form-grid-industrial { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; }

.property-header-tech { display: flex; justify-content: flex-end; margin-bottom: 15px; }
.cyber-btn-sm-heavy {
  background: var(--black); color: #fff; border: none; font-family: var(--mono); padding: 5px 15px; cursor: pointer;
}
.cyber-btn-sm-heavy:hover { background: var(--red); }

.edit-gallery-previews { display: flex; gap: 10px; flex-wrap: wrap; margin-top: 20px; }
.preview-box {
  width: 100px; height: 100px; border: 1px solid var(--black); background: #111;
  position: relative; 
}
.preview-box img { width: 100%; height: 100%; object-fit: contain; }

.img-delete-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(217, 35, 35, 0.9); display: flex; flex-direction: column; 
  align-items: center; justify-content: center; opacity: 0; transition: all 0.2s ease;
  cursor: pointer; z-index: 10;
}
.preview-box:hover .img-delete-overlay { opacity: 1; }
.delete-icon { color: #fff; font-size: 2rem; line-height: 1; font-family: var(--heading); }
.delete-text { color: #fff; font-size: 0.6rem; font-family: var(--mono); letter-spacing: 1px; margin-top: 5px; }

/* 滚动条美化 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: var(--off-white); }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); border-radius: 3px; }

/* 🔥 灯箱样式 */
.cyber-lightbox {
  position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(0,0,0,0.9); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
  backdrop-filter: blur(5px);
}
.lightbox-content {
  position: relative; max-width: 90vw; max-height: 90vh;
  border: 2px solid var(--red); box-shadow: 0 0 20px rgba(217,35,35,0.3);
}
.lightbox-content img {
  max-width: 100%; max-height: 90vh; display: block; object-fit: contain;
}
.lightbox-close {
  position: absolute; bottom: -30px; left: 50%; transform: translateX(-50%);
  color: #fff; font-family: var(--mono); font-size: 0.8rem; letter-spacing: 2px;
}
</style>