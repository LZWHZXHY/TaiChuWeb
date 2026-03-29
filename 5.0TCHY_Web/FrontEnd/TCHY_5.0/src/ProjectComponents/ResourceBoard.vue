<template>
  <div class="resource-board-layout">
    
    <aside class="doc-sidebar">
      <div class="sidebar-header">
        <h3 class="sidebar-title">RESOURCE_DOCS //</h3>
        <button class="btn-new-doc" @click="createNewDoc" :disabled="isCreating">
          <span class="icon">+</span> {{ isCreating ? '创建中' : '新建文档' }}
        </button>
      </div>

      <div class="doc-list-container">
        <div v-if="loadingDocs" class="loading-state">加载文档列表中...</div>
        <div v-else-if="docList.length === 0" class="empty-list">
          暂无文档，点击上方按钮创建。
        </div>
        <ul v-else class="doc-list">
          <li 
            v-for="doc in docList" 
            :key="doc.Id || doc.id" 
            class="doc-item"
            :class="{ active: selectedDoc && (selectedDoc.Id || selectedDoc.id) === (doc.Id || doc.id) }"
            @click="selectDoc(doc.Id || doc.id)"
          >
            <div class="doc-item-title">{{ doc.Title || doc.title }}</div>
            <div class="doc-item-meta">
              {{ formatDate(doc.UpdatedAt || doc.updatedAt) }} · {{ doc.EditorName || doc.editorName }}
            </div>
          </li>
        </ul>
      </div>
    </aside>

    <main class="doc-main-content">
      
      <div v-if="!selectedDoc && !loadingDetail" class="empty-main-state">
        <div class="empty-icon-wrapper">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
        </div>
        <p class="empty-title">请在左侧选择或新建一篇文档</p>
      </div>

      <div v-else-if="loadingDetail" class="empty-main-state">
        <p class="loading-text">正在读取文档数据...</p>
      </div>

      <div v-else class="doc-detail-wrapper">
        
        <div class="detail-header">
          <div class="detail-title-area">
            <h2 v-if="!isEditing" class="detail-title">{{ selectedDoc.Title || selectedDoc.title }}</h2>
            <input 
              v-else 
              v-model="editForm.title" 
              class="edit-title-input" 
              placeholder="请输入文档标题..." 
            />
            <div class="detail-meta" v-if="!isEditing">
              最后由 <span class="highlight">{{ selectedDoc.EditorName || selectedDoc.editorName }}</span> 更新于 {{ formatDate(selectedDoc.UpdatedAt || selectedDoc.updatedAt) }}
            </div>
          </div>
          
          <div class="detail-actions">
            <template v-if="!isEditing">
              <button class="btn-danger-ghost" @click="deleteDoc">删除</button>
              <button class="btn-primary" @click="startEdit">
                编辑文档
              </button>
            </template>
            <template v-else>
              <button class="btn-ghost" @click="cancelEdit">取消</button>
              <button class="btn-success" @click="saveDocument" :disabled="isSaving">
                {{ isSaving ? '保存中...' : '保存更改' }}
              </button>
            </template>
          </div>
        </div>

        <div class="detail-body">
          <div v-if="!isEditing" class="markdown-body" v-html="parsedContent"></div>
          
          <div v-else class="editor-container">
            <textarea 
              v-model="editForm.content" 
              class="modern-textarea" 
              placeholder="在此输入文本...&#10;直接粘贴网址（如 https://docs.google.com/xxx），保存后将自动变为可跳转链接。"
            ></textarea>
          </div>
        </div>

      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps<{
  projectId: string | number
}>()

const docList = ref<any[]>([])            
const selectedDoc = ref<any>(null)        
const loadingDocs = ref(true)             
const loadingDetail = ref(false)          
const isCreating = ref(false)             

const isEditing = ref(false)
const isSaving = ref(false)
const editForm = ref({ title: '', content: '' })

const formatDate = (dateString: string) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')} ${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

const parsedContent = computed(() => {
  // 💡 兼容大写 Content 和小写 content
  const content = selectedDoc.value?.Content || selectedDoc.value?.content
  if (!content) return '暂无内容...';
  
  const urlRegex = /(https?:\/\/[^\s]+)/g;
  return content
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(urlRegex, function(url: string) {
      return `<a href="${url}" target="_blank" rel="noopener noreferrer" class="doc-link">${url}</a>`;
    })
    .replace(/\n/g, '<br>');
});

const fetchDocList = async () => {
  loadingDocs.value = true
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/docs`)
    if (res.data && res.data.success) {
      docList.value = res.data.data
    }
  } catch (error) {
    console.error('获取文档列表失败', error)
  } finally {
    loadingDocs.value = false
  }
}

const selectDoc = async (docId: number) => {
  isEditing.value = false 
  loadingDetail.value = true
  try {
    const res = await apiClient.get(`/docs/${docId}`)
    if (res.data && res.data.success) {
      selectedDoc.value = res.data.data
    }
  } catch (error) {
    console.error('获取文档详情失败', error)
  } finally {
    loadingDetail.value = false
  }
}

const createNewDoc = async () => {
  isCreating.value = true
  try {
    const res = await apiClient.post(`/projects/${props.projectId}/docs`, {
      Title: '新建未命名文档'
    })
    if (res.data && res.data.success) {
      await fetchDocList()
      await selectDoc(res.data.data) 
      startEdit()
    }
  } catch (error) {
    console.error('创建文档失败', error)
    alert('创建失败，请稍后重试')
  } finally {
    isCreating.value = false
  }
}

const saveDocument = async () => {
  if (!selectedDoc.value) return
  isSaving.value = true
  
  // 💡 获取正确的 ID（兼容大小写）
  const docId = selectedDoc.value.Id || selectedDoc.value.id
  
  try {
    await apiClient.put(`/docs/${docId}`, {
      Title: editForm.value.title,
      Content: editForm.value.content
    })
    
    // 💡 保存成功后更新本地状态（统一更新大写属性，防备后端）
    selectedDoc.value.Title = editForm.value.title
    selectedDoc.value.Content = editForm.value.content
    
    // 更新左侧列表里的标题
    const listItem = docList.value.find(d => (d.Id || d.id) === docId)
    if (listItem) listItem.Title = editForm.value.title
    
    isEditing.value = false
  } catch (error) {
    console.error('保存失败', error)
    alert('保存失败，请检查网络')
  } finally {
    isSaving.value = false
  }
}

const deleteDoc = async () => {
  if (!selectedDoc.value) return
  if (!confirm('确定要删除这篇文档吗？此操作不可逆！')) return

  // 💡 获取正确的 ID
  const docId = selectedDoc.value.Id || selectedDoc.value.id

  try {
    await apiClient.delete(`/docs/${docId}`)
    docList.value = docList.value.filter(d => (d.Id || d.id) !== docId)
    selectedDoc.value = null 
  } catch (error) {
    console.error('删除失败', error)
    alert('删除失败')
  }
}

const startEdit = () => {
  // 💡 兼容读取标题和正文
  editForm.value.title = selectedDoc.value.Title || selectedDoc.value.title || ''
  editForm.value.content = selectedDoc.value.Content || selectedDoc.value.content || ''
  isEditing.value = true
}

const cancelEdit = () => {
  isEditing.value = false
}

onMounted(() => {
  fetchDocList()
})
</script>

<style scoped>
/* ================= 核心变量 ================= */
.resource-board-layout {
  --primary: #0052CC;
  --primary-hover: #0043A6;
  --danger: #DE350B;
  --success: #36B37E;
  --bg-main: #F4F5F7;
  --bg-surface: #FFFFFF;
  --text-main: #172B4D;
  --text-muted: #5E6C84;
  --border-light: #DFE1E6;
  --border-focus: #4C9AFF;
  
  flex: 1;
  display: flex;
  background: var(--bg-main);
  height: 100%;
  box-sizing: border-box;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  overflow: hidden;
}

/* ================= 左侧栏 (侧边栏) ================= */
.doc-sidebar {
  width: 280px;
  background: var(--bg-surface);
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  z-index: 2;
}
.sidebar-header {
  padding: 20px 16px;
  border-bottom: 1px solid var(--border-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.sidebar-title {
  margin: 0; font-size: 14px; font-weight: 800; color: #111; font-family: 'JetBrains Mono', monospace;
}
.btn-new-doc {
  background: transparent; border: 1px solid #111; border-radius: 4px; padding: 4px 8px; font-size: 12px; font-weight: bold; cursor: pointer; transition: 0.2s;
}
.btn-new-doc:hover { background: #111; color: #fff; }

.doc-list-container { flex: 1; overflow-y: auto; padding: 12px 0; }
.loading-state, .empty-list { padding: 20px; text-align: center; color: var(--text-muted); font-size: 13px; }
.doc-list { list-style: none; margin: 0; padding: 0; }
.doc-item {
  padding: 12px 16px; cursor: pointer; border-left: 3px solid transparent; transition: background 0.2s;
}
.doc-item:hover { background: rgba(9, 30, 66, 0.04); }
.doc-item.active { background: #DEEBFF; border-left-color: var(--primary); }
.doc-item-title { font-size: 14px; font-weight: 600; color: var(--text-main); margin-bottom: 4px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.doc-item-meta { font-size: 11px; color: var(--text-muted); }

/* ================= 右侧栏 (主内容区) ================= */
.doc-main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #FAFBFC;
  overflow: hidden;
}

.empty-main-state {
  flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: center; text-align: center;
}
.empty-icon-wrapper { color: var(--border-light); margin-bottom: 16px; }
.empty-title { font-size: 16px; font-weight: 600; color: var(--text-muted); }
.loading-text { font-size: 14px; font-weight: bold; color: var(--primary); animation: pulse 1.5s infinite; }

.doc-detail-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* 详情页头部 */
.detail-header {
  padding: 24px 40px;
  background: var(--bg-surface);
  border-bottom: 1px solid var(--border-light);
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-shrink: 0;
}
.detail-title-area { flex: 1; margin-right: 20px; }
.detail-title { margin: 0 0 8px 0; font-size: 24px; font-weight: 800; color: var(--text-main); }
.edit-title-input { width: 100%; font-size: 20px; font-weight: bold; padding: 8px 12px; border: 2px solid var(--border-focus); border-radius: 4px; outline: none; margin-bottom: 8px; }
.detail-meta { font-size: 12px; color: var(--text-muted); }
.detail-meta .highlight { font-weight: bold; color: var(--text-main); }

.detail-actions { display: flex; gap: 12px; }

/* 详情页主体 */
.detail-body {
  flex: 1;
  padding: 0;
  overflow-y: auto;
  position: relative;
}

/* 阅读模式样式 */
.markdown-body {
  padding: 32px 40px;
  font-size: 15px;
  line-height: 1.8;
  color: var(--text-main);
  word-wrap: break-word;
}
:deep(.doc-link) { color: var(--primary); text-decoration: none; font-weight: 600; background: rgba(0, 82, 204, 0.08); padding: 2px 6px; border-radius: 4px; transition: all 0.2s; word-break: break-all; }
:deep(.doc-link:hover) { background: var(--primary); color: #fff; text-decoration: underline; }

/* 编辑模式样式 */
.editor-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  padding: 20px 40px;
}
.modern-textarea {
  flex: 1; width: 100%; padding: 20px; border: 2px solid var(--border-light); border-radius: 6px; font-size: 15px; line-height: 1.8; color: var(--text-main); background: var(--bg-surface); resize: none; outline: none; font-family: inherit; box-sizing: border-box;
}
.modern-textarea:focus { border-color: var(--border-focus); box-shadow: 0 0 0 3px rgba(76, 154, 255, 0.2); }

/* ================= 按钮样式 ================= */
.btn-primary { background: #111; color: #fff; border: 2px solid #111; border-radius: 4px; padding: 8px 16px; font-weight: 800; font-size: 13px; cursor: pointer; }
.btn-primary:hover { background: var(--primary); border-color: var(--primary); }
.btn-success { background: var(--success); color: #fff; border: 2px solid var(--success); border-radius: 4px; padding: 8px 16px; font-weight: 800; font-size: 13px; cursor: pointer; }
.btn-success:not(:disabled):hover { opacity: 0.9; }
.btn-ghost { background: transparent; border: 2px solid transparent; color: var(--text-muted); cursor: pointer; padding: 8px 16px; font-size: 13px; font-weight: 700; border-radius: 4px; }
.btn-ghost:hover { background: rgba(9,30,66,0.08); color: var(--text-main); }
.btn-danger-ghost { background: transparent; border: 1px solid var(--danger); color: var(--danger); border-radius: 4px; padding: 8px 16px; font-weight: bold; font-size: 13px; cursor: pointer; transition: 0.2s;}
.btn-danger-ghost:hover { background: var(--danger); color: #fff; }
button:disabled { opacity: 0.6; cursor: not-allowed; }

@keyframes pulse { 0% { opacity: 0.6; } 50% { opacity: 1; } 100% { opacity: 0.6; } }

/* 滚动条美化 */
::-webkit-scrollbar { width: 8px; height: 8px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: #C1C7D0; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #A5ADBA; }
</style>