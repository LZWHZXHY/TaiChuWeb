<template>
  <div class="doc-viewer-wrapper">
    <div class="doc-paper">
      
      <div class="doc-toolbar">
        <div class="doc-breadcrumbs">
          ID: {{ node.id }} // {{ node.type }}
        </div>
        <div class="tool-actions">
          <template v-if="!isEditing">
            <button class="cyber-btn primary" @click="startEdit">EDIT_MODE</button>
            <button class="cyber-btn ghost del" @click="deleteNode">DEL</button>
          </template>
          <template v-else>
            <button class="cyber-btn ghost" @click="cancelEdit">CANCEL</button>
            <button class="cyber-btn primary" @click="saveNode" :disabled="submitting">
              {{ submitting ? 'SAVING...' : 'SAVE_CHANGES' }}
            </button>
          </template>
        </div>
      </div>

      <div v-if="!isEditing" class="read-mode">
        
        <div class="doc-header-section">
          <div class="header-content">
            <h1 class="doc-title">{{ node.name }}</h1>
            <div class="meta-row">
              <span class="tag">TYPE: {{ node.type }}</span>
              <span class="tag">AUTHOR: {{ node.author || 'SYSTEM' }}</span>
              <span class="tag">UPDATED: {{ formatDate(node.updateTime) }}</span>
            </div>
          </div>
          <div class="header-visual" v-if="parsedImages.length > 0">
            <img :src="parsedImages[0]" @error="handleImgError" />
          </div>
        </div>

        <div class="doc-divider"></div>

        <div class="doc-section">
          <h3 class="sec-title">DESCRIPTION // 描述</h3>
          <div class="text-content">
            {{ node.description || node.summary || 'NO_DATA_LOGGED...' }}
          </div>
        </div>

        <div class="doc-section" v-if="hasMetaData">
          <h3 class="sec-title">ATTRIBUTES // 属性设定</h3>
          <div class="attr-panel">
            <JsonTreeViewer :data="parsedMeta" />
          </div>
        </div>

        <div class="doc-section" v-if="parsedImages.length > 0">
          <h3 class="sec-title">GALLERY // 图像档案</h3>
          <div class="gallery-grid">
            <div v-for="(img, idx) in parsedImages" :key="idx" class="gallery-item">
              <img :src="img" loading="lazy" />
            </div>
          </div>
        </div>

      </div>

      <div v-else class="edit-mode custom-scroll">
        
        <div class="edit-section">
          <div class="sec-label">>> BASIC_INFO // 基础信息</div>
          <div class="form-row">
            <div class="form-group grow">
              <label>NAME</label>
              <input v-model="editForm.name" class="cyber-input" />
            </div>
            <div class="form-group">
              <label>TYPE</label>
              <input v-model="editForm.type" class="cyber-input" />
            </div>
            <div class="form-group">
              <label>AUTHOR</label>
              <input v-model="editForm.author" class="cyber-input" />
            </div>
          </div>
        </div>

        <div class="edit-section">
          <div class="sec-label">>> RELATIONS // 关系链接</div>
          <NodeRelationPanel 
            :currentNode="node" 
            :allNodes="allNodes"
            @select-node="$emit('select-node', $event)" 
          />
        </div>

        <div class="edit-section">
          <div class="sec-header">
            <div class="sec-label">>> PROPERTIES // 属性参数</div>
            <button class="cyber-btn-sm" @click="addRootProperty">+ ADD PROP</button>
          </div>
          
          <div class="props-editor-container">
            <PropertyItem 
              v-for="(item, index) in editForm.propsList" 
              :key="index"
              v-model="editForm.propsList[index]"
              @delete="removeRootProperty(index)"
            />
            <div v-if="editForm.propsList.length === 0" class="empty-tip">
              暂无属性，点击右上角添加
            </div>
          </div>
        </div>

        <div class="edit-section">
          <div class="sec-header">
            <div class="sec-label">>> IMAGES // 图像资料</div>
            <div class="upload-box">
              <button class="cyber-btn-sm" @click="$refs.fileInput.click()" :disabled="uploading">
                {{ uploading ? 'UPLOADING...' : '+ UPLOAD' }}
              </button>
              <input type="file" ref="fileInput" style="display:none" accept="image/*" @change="handleFileUpload" />
            </div>
          </div>
          
          <div class="edit-gallery">
            <div v-for="(url, idx) in parsedImages" :key="idx" class="edit-img-card">
              <img :src="url" />
            </div>
            <div v-if="parsedImages.length === 0" class="empty-tip">暂无图片</div>
          </div>
        </div>

        <div class="edit-section">
          <div class="sec-label">>> DESCRIPTION // 详细档案</div>
          <textarea v-model="editForm.description" class="cyber-textarea" rows="12"></textarea>
        </div>

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

// 🔥 注册事件
const emit = defineEmits(['update-node', 'delete-node', 'select-node'])

// --- State ---
const isEditing = ref(false)
const submitting = ref(false)
const uploading = ref(false)
const fileInput = ref(null)

// 编辑态表单数据
const editForm = reactive({
  id: null,
  name: '',
  type: '',
  author: '',
  description: '',
  propsList: [] 
})

// --- Helpers ---
const handleImgError = (e) => e.target.src = '/土豆.jpg'
const formatDate = (t) => t ? new Date(t).toLocaleDateString() : '-'
const getImageUrl = (url) => {
  if (!url) return ''
  if (url.startsWith('http')) return url
  return `https://bianyuzhou.com/${url}`
}

// --- Data Computeds ---

// 解析 meta_data_json 用于阅读模式
const parsedMeta = computed(() => {
  if (!props.node.meta_data_json) return {}
  try { 
    return (typeof props.node.meta_data_json === 'string') 
      ? JSON.parse(props.node.meta_data_json) 
      : props.node.meta_data_json
  } catch { return {} }
})

const hasMetaData = computed(() => Object.keys(parsedMeta.value).length > 0)

// 解析 image_url
const parsedImages = computed(() => {
  if (!props.node.image_url) return []
  try {
    const res = JSON.parse(props.node.image_url)
    const arr = Array.isArray(res) ? res : [props.node.image_url]
    return arr.map(getImageUrl)
  } catch { return [getImageUrl(props.node.image_url)] }
})

// --- JSON <-> Tree Conversion ---

// JSON 对象转 Tree 数组
const jsonToTree = (jsonObj) => {
  if (!jsonObj || typeof jsonObj !== 'object') return []
  return Object.keys(jsonObj).map(key => {
    const val = jsonObj[key]
    if (val && typeof val === 'object' && !Array.isArray(val)) {
      return { key: key, children: jsonToTree(val) } 
    } else {
      return { key: key, value: val } 
    }
  })
}

// Tree 数组转 JSON 对象
const treeToJson = (treeArr) => {
  const result = {}
  treeArr.forEach(item => {
    if (!item.key) return
    if (item.children && Array.isArray(item.children)) {
      result[item.key] = treeToJson(item.children)
    } else {
      result[item.key] = item.value
    }
  })
  return result
}

// --- Actions ---

const startEdit = () => {
  const n = props.node
  editForm.id = n.id
  editForm.name = n.name
  editForm.type = n.type
  editForm.author = n.author
  editForm.description = n.description || n.summary || ''
  
  // 转换 MetaData
  const meta = parsedMeta.value
  editForm.propsList = jsonToTree(meta)
  
  isEditing.value = true
}

const cancelEdit = () => isEditing.value = false

// 添加根属性
const addRootProperty = () => {
  editForm.propsList.push({ key: '', value: '' })
}
const removeRootProperty = (index) => {
  editForm.propsList.splice(index, 1)
}

const handleFileUpload = async (e) => {
  const file = e.target.files[0]
  if (!file) return
  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('file', file)
    await apiClient.post(`/Setting/${props.node.id}/image`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
    alert("上传成功，保存节点后刷新列表可见")
  } catch (e) { alert("上传失败") } finally { uploading.value = false }
}

const saveNode = () => {
  submitting.value = true
  
  // 1. 将 Tree 数组转回 JSON 字符串
  const metaJsonObj = treeToJson(editForm.propsList)
  const metaStr = JSON.stringify(metaJsonObj)

  const payload = {
    id: editForm.id,
    name: editForm.name,
    type: editForm.type, 
    description: editForm.description,
    author: editForm.author,
    metaStr: metaStr,
    parentId: props.node.parentId 
  }

  // Emit 给父组件去调 API
  emit('update-node', payload, () => {
    submitting.value = false
    isEditing.value = false
  })
}

const deleteNode = () => {
  if(confirm("Confirm Delete?")) emit('delete-node', props.node.id)
}

// 切换节点时自动退出编辑
watch(() => props.node.id, () => { isEditing.value = false })

</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.doc-viewer-wrapper {
  --bg-panel: #F4F1EA;
  --border-color: #111111;
  --text-main: #111111;
  --text-sub: #666666;
  --accent: #D92323;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --body-font: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;

  width: 100%; max-width: 900px; margin: 0 auto; height: 100%;
  display: flex; flex-direction: column;
}

.doc-paper { 
  background: var(--bg-panel); 
  border: 2px solid var(--border-color); 
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1); 
  flex: 1; display: flex; flex-direction: column; overflow: hidden; /* 内部滚动 */
}

.doc-toolbar { 
  display: flex; justify-content: space-between; align-items: center; 
  padding: 10px 30px; border-bottom: 1px dashed #ccc; background: #fff; flex-shrink: 0;
}
.doc-breadcrumbs { color: var(--text-sub); font-size: 0.75rem; font-weight: bold; font-family: var(--mono); }
.tool-actions { display: flex; gap: 10px; }

/* === Read Mode === */
.read-mode { padding: 40px; overflow-y: auto; }

.doc-header-section { display: flex; justify-content: space-between; align-items: flex-start; gap: 20px; margin-bottom: 20px; }
.header-content { flex: 1; }
.header-visual { width: 120px; height: 120px; border: 2px solid var(--border-color); flex-shrink: 0; background: #eee; }
.header-visual img { width: 100%; height: 100%; object-fit: cover; }

.doc-title { 
  font-family: var(--heading); font-size: 3rem; margin: 0 0 10px 0; 
  line-height: 1; letter-spacing: 1px; color: var(--text-main);
}
.meta-row { display: flex; gap: 15px; flex-wrap: wrap; }
.tag { background: #e0e0e0; padding: 2px 8px; font-size: 0.7rem; color: #333; border: 1px solid #ccc; font-family: var(--mono); }

.doc-divider { border-bottom: 2px solid var(--border-color); margin: 20px 0 30px 0; }

.doc-section { margin-bottom: 40px; }
.sec-title { font-size: 1rem; color: var(--accent); margin-bottom: 15px; font-family: var(--mono); font-weight: bold; border-left: 4px solid var(--accent); padding-left: 10px; }

.text-content { font-family: var(--body-font); font-size: 1.05rem; line-height: 1.8; color: #333; white-space: pre-wrap; }
.attr-panel { background: #fff; border: 1px solid #ccc; padding: 20px; border-radius: 4px; }

.gallery-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 10px; }
.gallery-item img { width: 100%; height: 150px; object-fit: cover; border: 1px solid #ccc; }

/* === Edit Mode (Industrial Form) === */
.edit-mode { padding: 30px; background: #f9f9f9; overflow-y: auto; }

.edit-section { 
  background: #fff; border: 1px solid #ccc; padding: 20px; margin-bottom: 20px; 
  box-shadow: 2px 2px 0 rgba(0,0,0,0.05);
}
.sec-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.sec-label { font-family: var(--heading); font-size: 1.1rem; color: var(--text-main); letter-spacing: 1px; border-bottom: 2px solid var(--accent); display: inline-block; padding-bottom: 2px; }

.form-row { display: flex; gap: 15px; }
.form-group { display: flex; flex-direction: column; gap: 5px; }
.form-group.grow { flex: 1; }
.form-group label { font-size: 0.7rem; font-weight: bold; color: var(--text-sub); font-family: var(--mono); }

.cyber-input, .cyber-textarea { 
  background: #fff; border: 2px solid #ccc; padding: 8px; 
  font-family: var(--mono); outline: none; width: 100%; box-sizing: border-box; color: var(--text-main);
  transition: 0.2s; font-size: 0.9rem;
}
.cyber-input:focus, .cyber-textarea:focus { border-color: var(--text-main); background: #fffef0; }

.props-editor-container { display: flex; flex-direction: column; gap: 5px; }
.empty-tip { text-align: center; color: #999; font-size: 0.8rem; padding: 10px; }

.edit-gallery { display: flex; gap: 10px; flex-wrap: wrap; margin-top: 10px; }
.edit-img-card { width: 100px; height: 100px; border: 1px solid #ddd; background: #eee; }
.edit-img-card img { width: 100%; height: 100%; object-fit: cover; }

/* Buttons */
.cyber-btn { border: 2px solid var(--border-color); background: transparent; padding: 6px 15px; cursor: pointer; font-family: var(--heading); font-size: 0.9rem; transition: 0.2s; }
.cyber-btn.primary { background: var(--text-main); color: #fff; }
.cyber-btn.primary:hover { background: var(--accent); border-color: var(--accent); }
.cyber-btn.ghost:hover { background: #eee; }
.cyber-btn.del:hover { border-color: var(--accent); color: var(--accent); }
.cyber-btn-sm { background: var(--text-main); color: #fff; border: none; padding: 4px 10px; cursor: pointer; font-family: var(--mono); font-size: 0.75rem; }
.cyber-btn-sm:hover { background: var(--accent); }

/* Scrollbar */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ccc; }
</style>