<template>
  <div class="md-world-list-wrapper">
    
    <template v-if="!showGraph">
      <header class="md-control-bar">
        <div class="header-left">
          <div class="icon-box">🌌</div>
          <div class="text-info">
            <h2 class="title">世界观档案</h2>
            <span class="sub">知识库目录管理系统</span>
          </div>
        </div>

        <div class="header-right">
          <div class="search-unit">
            <span class="search-icon">🔍</span>
            <input v-model="searchQuery" class="md-input-search" placeholder="通过名称或简介筛选..." />
          </div>
          <button class="md-btn primary" @click="openCreateModal">
            <span class="icon">＋</span> 新建档案
          </button>
        </div>
      </header>

      <main class="md-world-body">
        <div class="scroll-container md-scrollbar">
          <div class="content-wrapper">
            
            <div v-if="loading" class="state-container">
              <div class="md-spinner"></div>
              <span>正在扫描档案库...</span>
            </div>
            <div v-else-if="filteredList.length === 0" class="state-container empty">
              <div class="empty-icon">📭</div>
              <span>暂无符合条件的档案节点</span>
            </div>

            <div v-else class="md-grid">
              <article v-for="ip in filteredList" :key="ip.id" class="md-card">
                
                <div class="card-cover" @click="openGraphEditor(ip)">
                  <img :src="getImageUrl(ip.cover_url)" loading="lazy" @error="handleImgError" />
                  <div class="cover-overlay">
                    <span class="enter-btn">进入知识库</span>
                  </div>
                  <span class="sector-badge">编号-{{ padZero(ip.id) }}</span>
                </div>

                <div class="card-content">
                  <div class="content-header">
                    <h3 class="ip-name" :title="ip.name">{{ ip.name }}</h3>
                    <div class="action-menu">
                      <button class="md-icon-btn" @click.stop="openEditModal(ip)" title="编辑档案">✎</button>
                      <button class="md-icon-btn danger" @click.stop="deleteIp(ip.id)" title="删除档案">🗑</button>
                    </div>
                  </div>
                  
                  <div class="ip-tagline" v-if="ip.tagline">{{ ip.tagline }}</div>
                  <p class="ip-summary">{{ ip.summary || '该节点尚无详细描述...' }}</p>
                  
                  <div class="card-footer">
                    <span class="status-dot"></span>
                    <span class="update-time">档案同步时间: {{ formatDate(ip.updateTime) }}</span>
                  </div>
                </div>
              </article>
            </div>

          </div>
        </div>
      </main>
    </template>

    <template v-else>
      <WorldGraph 
        :id="currentGraphId" 
        @close="closeGraphEditor" 
        class="detail-view-mount"
      />
    </template>

    <Teleport to="body">
      <Transition name="md-fade">
        <div v-if="showModal" class="md-modal-overlay" @click.self="closeModal">
          <div class="md-modal-card">
            <div class="modal-header">
              <h3 class="modal-title">{{ isEditing ? '配置世界观元数据' : '初始化新档案节点' }}</h3>
              <button class="modal-close" @click="closeModal">×</button>
            </div>
            
            <div class="modal-content md-scrollbar">
              <form @submit.prevent="submitForm" class="md-form">
                
                <div class="form-group">
                  <label>档案名称 <span class="required">*</span></label>
                  <input v-model="form.Name" class="md-input" placeholder="输入核心项目或世界观名称" required />
                </div>
                
                <div class="form-group">
                  <label>副标题 / 简介</label>
                  <input v-model="form.Tagline" class="md-input" placeholder="一句简洁的描述..." />
                </div>
                
                <div class="form-group">
                  <label>档案概述</label>
                  <textarea v-model="form.Summary" class="md-textarea" rows="4" placeholder="关于该世界的宏观背景信息..."></textarea>
                </div>

                <div class="form-group">
                  <label>视觉封面</label>
                  
                  <div v-if="isEditing" class="cover-upload-section">
                    <div class="preview-frame">
                      <img :src="getImageUrl(form.CoverUrl)" @error="handleImgError" />
                    </div>
                    <div class="upload-actions">
                       <button type="button" class="md-btn outlined" @click="triggerFileSelect" :disabled="uploading">
                         {{ uploading ? '数据传输中...' : '更新视觉资产' }}
                       </button>
                       <input type="file" ref="coverInput" style="display:none" accept="image/*" @change="handleCoverUpload" />
                       <span class="tip-text" v-if="!uploading">建议 16:9 比例，支持 5MB 以内图片</span>
                    </div>
                  </div>

                  <div v-else class="info-alert">
                    <span class="icon">ℹ️</span>
                    <span>请先完成初始化创建，随后在编辑模式中上传封面图像。</span>
                  </div>
                </div>

                <div class="form-actions">
                  <button class="md-btn text" type="button" @click="closeModal">取消</button>
                  <button class="md-btn primary" type="submit" :disabled="submitting">
                    {{ submitting ? '执行中...' : '提交存档' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'
import WorldGraph from '@/ArtCenter/Components/WorldDashBoard.vue'

// --- 核心状态 ---
const loading = ref(false)
const ipList = ref([])
const searchQuery = ref('')

// 切换状态
const showGraph = ref(false)
const currentGraphId = ref(null)

// 弹窗表单状态
const showModal = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const currentEditId = ref(null)
const form = reactive({ Name: '', Tagline: '', Summary: '', CoverUrl: '' })

// 文件上传状态
const uploading = ref(false)
const coverInput = ref(null)

// --- 工具函数 ---
const padZero = (n) => n < 10 ? `0${n}` : n
const formatDate = (t) => t ? new Date(t).toLocaleDateString() : '未知'
const getImageUrl = (url) => { 
  if (!url) return 'https://img.bianyuzhou.com/uploads/ip_assets/default.png'; 
  if (url.startsWith('http')) return url; 
  return `https://bianyuzhou.com/${url}` 
}
const handleImgError = (e) => e.target.src = 'https://img.bianyuzhou.com/uploads/ip_assets/default.png'

// --- 业务逻辑 ---
const fetchList = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/ip')
    ipList.value = Array.isArray(res.data) ? res.data : []
  } catch (e) { 
    ipList.value = [] 
  } finally { 
    loading.value = false 
  }
}

const filteredList = computed(() => {
  if (!searchQuery.value) return ipList.value
  const q = searchQuery.value.toLowerCase()
  return ipList.value.filter(ip => 
    (ip.name && ip.name.toLowerCase().includes(q)) || 
    (ip.tagline && ip.tagline.toLowerCase().includes(q))
  )
})

// 进入详情视图
const openGraphEditor = (ip) => { 
  currentGraphId.value = ip.id; 
  showGraph.value = true 
}

// 退出详情并刷新列表
const closeGraphEditor = () => { 
  showGraph.value = false; 
  currentGraphId.value = null; 
  fetchList() 
}

// 打开弹窗逻辑
const openCreateModal = () => { 
  isEditing.value = false; 
  currentEditId.value = null; 
  Object.assign(form, { Name: '', Tagline: '', Summary: '', CoverUrl: '' }); 
  showModal.value = true 
}

const openEditModal = (ip) => { 
  isEditing.value = true; 
  currentEditId.value = ip.id; 
  Object.assign(form, { Name: ip.name, Tagline: ip.tagline, Summary: ip.summary, CoverUrl: ip.cover_url }); 
  showModal.value = true 
}

const closeModal = () => showModal.value = false

// 文件选择触发
const triggerFileSelect = () => {
  if(coverInput.value) coverInput.value.click()
}

// 封面图上传
const handleCoverUpload = async (e) => {
  const file = e.target.files[0]
  if (!file || !currentEditId.value) return 

  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('file', file)
    
    const res = await apiClient.post(`/ip/${currentEditId.value}/image`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    if (res.data.success) {
      form.CoverUrl = res.data.url
      fetchList() // 列表静默同步
    }
  } catch (err) {
    console.error(err)
    alert("档案视觉同步失败，请检查网络链接")
  } finally {
    uploading.value = false
    if(e.target) e.target.value = ''
  }
}

// 提交表单
const submitForm = async () => {
  if (!form.Name.trim()) return alert("档案名称为必填项")
  submitting.value = true
  try {
    const method = isEditing.value ? 'put' : 'post'
    const url = isEditing.value ? `/ip/${currentEditId.value}` : '/ip'
    await apiClient[method](url, form)
    closeModal(); 
    fetchList()
  } catch (e) { 
    alert("档案操作失败，请重试") 
  } finally { 
    submitting.value = false 
  }
}

// 删除档案
const deleteIp = async (id) => { 
  if (confirm("确认移除该档案节点吗？其内部所有知识数据将被冻结。")) { 
    try {
      await apiClient.delete(`/ip/${id}`); 
      fetchList() 
    } catch (e) { alert("删除失败") }
  } 
}

onMounted(() => { fetchList() })
</script>

<style scoped>
/* --- MD 极简风格布局 --- */
.md-world-list-wrapper {
  flex: 1; 
  width: 100%; 
  height: 100%; 
  display: flex; 
  flex-direction: column;
  background-color: #F8FAFC;
  overflow: hidden;
  position: relative;
}

/* 控制栏：采用与 WorkCenter 统一的高度和阴影 */
.md-control-bar {
  flex-shrink: 0; 
  display: flex; 
  justify-content: space-between; 
  align-items: center;
  padding: 16px 32px; 
  background: #FFF; 
  border-bottom: 1px solid #E2E8F0;
  z-index: 10;
}

.header-left { display: flex; align-items: center; gap: 16px; }
.icon-box { font-size: 24px; }
.text-info .title { font-size: 1.1rem; font-weight: 600; margin: 0; color: #0F172A; }
.text-info .sub { font-size: 0.75rem; color: #64748B; }

.header-right { display: flex; align-items: center; gap: 16px; }
.search-unit { 
  display: flex; align-items: center; gap: 8px; 
  background: #F1F5F9; 
  padding: 6px 12px; 
  border-radius: 8px; 
  border: 1px solid #E2E8F0;
  transition: all 0.2s;
}
.search-unit:focus-within { border-color: #0284C7; background: #FFF; box-shadow: 0 0 0 2px rgba(2, 132, 199, 0.1); }
.search-icon { font-size: 14px; opacity: 0.5; }
.md-input-search { border: none; background: transparent; outline: none; width: 200px; font-size: 0.85rem; color: #0F172A; }

/* 基础按钮规范 */
.md-btn { 
  border: none; padding: 8px 18px; border-radius: 6px; 
  font-size: 0.85rem; font-weight: 500; cursor: pointer; 
  display: flex; align-items: center; gap: 8px; transition: 0.2s; 
}
.md-btn.primary { background: #0284C7; color: #FFF; }
.md-btn.primary:hover { background: #0369A1; box-shadow: 0 4px 12px rgba(2, 132, 199, 0.2); }
.md-btn.outlined { background: transparent; border: 1px solid #0284C7; color: #0284C7; }
.md-btn.outlined:hover { background: rgba(2, 132, 199, 0.05); }
.md-btn.text { background: transparent; color: #64748B; }
.md-btn.text:hover { background: #F1F5F9; color: #0F172A; }

/* 主体网格区 */
.md-world-body { flex: 1; display: flex; overflow: hidden; }
.scroll-container { flex: 1; overflow-y: auto; padding: 32px; }
.content-wrapper { padding-bottom: 40px; max-width: 1600px; margin: 0 auto; }

.md-grid { 
  display: grid; 
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); 
  gap: 24px; 
}

/* 详情挂载层：强制铺满 */
.detail-view-mount {
  flex: 1;
  width: 100%;
  height: 100%;
}

/* 卡片 MD 风格：细腻投影与过渡 */
.md-card { 
  background: #FFF; 
  border-radius: 12px;
  display: flex; 
  flex-direction: column; 
  border: 1px solid #E2E8F0;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); 
  height: 100%; 
  overflow: hidden; 
}
.md-card:hover { 
  transform: translateY(-4px); 
  box-shadow: 0 10px 25px -5px rgba(0,0,0,0.08); 
  border-color: #CBD5E1;
}

.card-cover { 
  height: 160px; position: relative; overflow: hidden; background: #F1F5F9; cursor: pointer; 
}
.card-cover img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.6s; }
.md-card:hover .card-cover img { transform: scale(1.05); }

.cover-overlay { 
  position: absolute; inset: 0; background: rgba(15, 23, 42, 0.3); 
  display: flex; justify-content: center; align-items: center; 
  opacity: 0; transition: opacity 0.3s; 
}
.md-card:hover .cover-overlay { opacity: 1; }
.enter-btn { 
  background: #FFF; color: #0F172A; 
  padding: 6px 16px; border-radius: 20px; font-weight: 600; font-size: 0.8rem;
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}

.sector-badge {
  position: absolute; top: 12px; left: 12px; 
  background: rgba(15, 23, 42, 0.7); color: #FFF; 
  font-size: 0.65rem; padding: 3px 8px; border-radius: 4px; backdrop-filter: blur(4px);
  font-family: 'JetBrains Mono', monospace;
}

.card-content { padding: 20px; flex: 1; display: flex; flex-direction: column; }
.content-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px; }
.ip-name { font-size: 1rem; font-weight: 600; margin: 0; line-height: 1.4; color: #0F172A; }

.action-menu { display: flex; gap: 4px; opacity: 0; transition: opacity 0.2s; }
.md-card:hover .action-menu { opacity: 1; }
.md-icon-btn { 
  background: transparent; border: none; cursor: pointer; 
  width: 28px; height: 28px; border-radius: 4px; font-size: 0.8rem; color: #64748B; 
  display: flex; align-items: center; justify-content: center; transition: 0.2s;
}
.md-icon-btn:hover { background: #F1F5F9; color: #0284C7; }
.md-icon-btn.danger:hover { color: #DC2626; background: #FEF2F2; }

.ip-tagline { font-size: 0.8rem; font-weight: 500; color: #0284C7; margin-bottom: 12px; }
.ip-summary { 
  font-size: 0.8rem; color: #475569; line-height: 1.6; margin-bottom: 16px; 
  flex: 1; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; 
}

.card-footer { 
  border-top: 1px solid #F1F5F9; padding-top: 12px; margin-top: auto; 
  display: flex; align-items: center; gap: 8px;
}
.status-dot { width: 6px; height: 6px; background: #10B981; border-radius: 50%; }
.update-time { font-size: 0.7rem; color: #94A3B8; }

/* 弹窗设计 */
.md-modal-overlay { 
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.4); z-index: 2000; 
  display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); 
}
.md-modal-card { 
  width: 540px; max-width: 90vw; background: #FFF; 
  border-radius: 12px; display: flex; flex-direction: column; max-height: 85vh; 
  box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04); 
}
.modal-header { 
  padding: 20px 24px; display: flex; justify-content: space-between; align-items: center; 
  border-bottom: 1px solid #F1F5F9; 
}
.modal-title { font-size: 1rem; font-weight: 600; margin: 0; color: #0F172A; }
.modal-close { background: transparent; border: none; font-size: 1.5rem; color: #94A3B8; cursor: pointer; transition: 0.2s; }
.modal-close:hover { color: #DC2626; }

.modal-content { padding: 24px; overflow-y: auto; flex: 1; }
.md-form { display: flex; flex-direction: column; gap: 20px; }
.form-group label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 8px; color: #334155; }
.required { color: #DC2626; }

.md-input, .md-textarea { 
  width: 100%; border: 1px solid #E2E8F0; padding: 10px 12px; border-radius: 6px;
  font-size: 0.9rem; outline: none; background: #FFF; box-sizing: border-box; transition: 0.2s;
}
.md-input:focus, .md-textarea:focus { border-color: #0284C7; box-shadow: 0 0 0 2px rgba(2, 132, 199, 0.1); }

/* 预览图上传区 */
.cover-upload-section {
  display: flex; gap: 16px; align-items: flex-start;
  border: 1px dashed #E2E8F0; padding: 16px; border-radius: 8px; background: #F8FAFC;
}
.preview-frame {
  width: 140px; height: 78px; border-radius: 4px; overflow: hidden; background: #E2E8F0; flex-shrink: 0; border: 1px solid #F1F5F9;
}
.preview-frame img { width: 100%; height: 100%; object-fit: cover; }
.upload-actions { display: flex; flex-direction: column; gap: 8px; flex: 1; justify-content: center; }
.tip-text { font-size: 0.7rem; color: #94A3B8; }

.info-alert {
  background: #F0F9FF; border: 1px solid #E0F2FE; padding: 12px 16px; 
  border-radius: 8px; display: flex; gap: 10px; color: #0369A1; font-size: 0.8rem; line-height: 1.5;
}

.form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 12px; padding-top: 20px; border-top: 1px solid #F1F5F9; }

/* 状态组件 */
.state-container { text-align: center; padding: 100px 0; color: #94A3B8; display: flex; flex-direction: column; align-items: center; gap: 16px; }
.md-spinner { width: 28px; height: 28px; border: 3px solid #F1F5F9; border-top-color: #0284C7; border-radius: 50%; animation: spin 0.8s linear infinite; }
.empty-icon { font-size: 40px; opacity: 0.5; }

@keyframes spin { to { transform: rotate(360deg); } }

/* 滚动条 */
.md-scrollbar::-webkit-scrollbar { width: 5px; }
.md-scrollbar::-webkit-scrollbar-thumb { background: #CBD5E1; border-radius: 10px; }
</style>