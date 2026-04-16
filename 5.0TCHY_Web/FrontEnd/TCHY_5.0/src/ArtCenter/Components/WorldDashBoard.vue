<template>
  <div class="md-dashboard-container">
    
    <header class="md-header">
      <div class="header-left">
        <button class="md-btn md-btn-text" @click="$emit('close')">
          <span class="icon">←</span> 返回系统
        </button>
        <div class="world-info">
          <h2 class="title">{{ worldInfo.name || '加载中...' }}</h2>
          <span class="subtitle">编号: {{ id }} · 知识库</span>
        </div>
      </div>
      
      <div class="md-tabs">
        <div class="tab-item" :class="{ active: viewMode === 'docs' }" @click="viewMode = 'docs'">文档</div>
        <div class="tab-item" :class="{ active: viewMode === 'graph' }" @click="viewMode = 'graph'">图谱</div>
        <div class="tab-item disabled" title="开发中">时间轴</div>
      </div>

      <div class="header-right">
        <button class="md-btn md-btn-text" @click="showAuditModal = true">📢 审核</button>
        <button class="md-btn md-btn-text" @click="showTeamModal = true">👥 团队</button>
        <span class="status-chip">已同步</span>
      </div>
    </header>

    <div class="md-body">
      <div v-if="viewMode === 'docs'" class="doc-layout">
        <aside class="md-sidebar">
          <div class="sidebar-tools">
            <input v-model="searchQuery" class="md-input-sm" placeholder="筛选节点..." />
            <button class="md-btn-icon primary" title="新建节点" @click="openNodeModal('create')">+</button>
          </div>
          
          <div class="doc-tree md-scroll">
            <div v-if="loading" class="empty-text">正在加载目录...</div>
            
            <div 
              v-for="item in flattenedTree" 
              :key="item.id"
              :id="'tree-node-' + item.id"
              class="tree-node"
              :class="{ 
                active: selectedNodeId === item.id,
                'is-parent': item.children && item.children.length > 0 
              }"
              :style="{ paddingLeft: (item.level * 16 + 16) + 'px' }"
              @click="selectNode(item)"
            >
              <span class="tree-guide" v-if="item.level > 0"></span>
              <span class="node-icon">{{ getNodeIcon(item.type) }}</span>
              <span class="node-name">{{ item.name }}</span>
              
              <button 
                v-if="!searchQuery" 
                class="node-action-btn" 
                title="重新分配父节点"
                @click.stop="openNodeModal('move', item)"
              >⇄</button>

              <span class="node-type-tag" v-if="item.level === 0">{{ item.type }}</span>
            </div>
          </div>
        </aside>

        <main class="md-main">
          <WorldDocViewer 
            v-if="currentNode" 
            :node="currentNode" 
            :all-nodes="rawNodes" 
            @update-node="handleUpdateNode"
            @delete-node="handleDeleteNode"
            @select-node="handleSelectNode"
          />
          <div v-else class="empty-state">
            <span class="big-icon">📂</span>
            <p>请在左侧选择要阅读的文档</p>
          </div>
        </main>
      </div>

      <div v-else-if="viewMode === 'graph'" class="graph-layout">
        <WorldGraph 
          :ip-id="id"
          @select-node="handleGraphSelect" 
        />
      </div>
    </div>

    <CollaboratorModal :show="showTeamModal" :ip-id="id" @close="showTeamModal = false" />
    <AuditModal :show="showAuditModal" :ip-id="id" @close="showAuditModal = false" />
    <NodeManagerModal 
      :show="showNodeModal"
      :mode="nodeModalMode"
      :node-list="flattenedTree"
      :initial-data="nodeModalData"
      @close="showNodeModal = false"
      @submit="handleNodeSubmit"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import apiClient from '@/utils/api'
import WorldDocViewer from './WorldDocViewer.vue'
import WorldGraph from './WorldGraph.vue'
import CollaboratorModal from './CollaboratorModal.vue'
import AuditModal from './AuditModal.vue'
import NodeManagerModal from './NodeManagerModal.vue'

const props = defineProps({ id: { type: [String, Number], required: true } })
const emit = defineEmits(['close'])

const viewMode = ref('docs')
const loading = ref(false)
const rawNodes = ref([]) 
const worldInfo = ref({})
const selectedNodeId = ref(null)
const searchQuery = ref('')
const showTeamModal = ref(false)
const showAuditModal = ref(false)

const showNodeModal = ref(false)
const nodeModalMode = ref('create')
const nodeModalData = ref({ id: null, name: '', parentId: null })

const flattenedTree = computed(() => {
  const map = {}
  const roots = []
  const nodes = JSON.parse(JSON.stringify(rawNodes.value))
  
  nodes.forEach(node => { map[node.id] = { ...node, children: [], level: 0 } })
  nodes.forEach(node => {
    const mappedNode = map[node.id]
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase()
      if (node.name.toLowerCase().includes(q) || node.type.toLowerCase().includes(q)) roots.push(mappedNode)
    } else {
      if (node.parentId && map[node.parentId]) map[node.parentId].children.push(mappedNode)
      else roots.push(mappedNode)
    }
  })
  
  const result = []
  const traverse = (nodes, level) => {
    nodes.forEach(node => {
      node.level = level
      result.push(node)
      if (node.children && node.children.length > 0) traverse(node.children, level + 1)
    })
  }
  
  if (searchQuery.value) {
    roots.forEach(n => n.level = 0)
    return roots
  }
  traverse(roots, 0)
  return result
})

const currentNode = computed(() => rawNodes.value.find(n => n.id === selectedNodeId.value))

const getNodeIcon = (type) => {
  const t = (type || '').toLowerCase()
  if (t.includes('角色') || t.includes('character')) return '👤'
  if (t.includes('星') || t.includes('planet')) return '🪐'
  return '📄'
}

const initData = async () => {
  if (!props.id) return
  loading.value = true
  try {
    const [ipRes, nodesRes] = await Promise.all([
      apiClient.get(`/ip/${props.id}`),
      apiClient.get(`/Setting/ip/${props.id}`)
    ])
    worldInfo.value = ipRes.data
    rawNodes.value = nodesRes.data || []
  } catch (e) { console.error(e) } finally { loading.value = false }
}

const selectNode = (item) => { selectedNodeId.value = item.id }

const handleGraphSelect = async (nodeId) => {
  const targetId = parseInt(nodeId);
  selectedNodeId.value = targetId;
  viewMode.value = 'docs'; 
  
  await nextTick();
  
  const element = document.getElementById(`tree-node-${targetId}`);
  if (element) {
    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    element.style.transition = 'background 0.3s ease';
    element.style.background = 'rgba(25, 118, 210, 0.12)'; 
    setTimeout(() => {
      element.style.background = ''; 
    }, 800);
  }
}

const handleSelectNode = (targetId) => {
  const target = rawNodes.value.find(n => n.id === targetId)
  if (target) selectNode(target)
}

const openNodeModal = (mode, node = null) => {
  nodeModalMode.value = mode
  if (mode === 'create') {
    nodeModalData.value = { id: null, name: '', parentId: selectedNodeId.value }
  } else {
    nodeModalData.value = { id: node.id, name: node.name, parentId: node.parentId }
  }
  showNodeModal.value = true
}

const handleNodeSubmit = async (data, done) => {
  try {
    if (nodeModalMode.value === 'create') {
      await apiClient.post('/Setting', { 
        IpId: parseInt(props.id), 
        Name: data.name, 
        Type: data.type, 
        ParentId: data.parentId 
      })
    } else {
      await apiClient.post('/Setting/move', { 
        NodeId: data.id, 
        TargetParentId: data.parentId 
      })
    }
    await initData()
    showNodeModal.value = false
  } catch (e) {
    alert("操作失败")
  } finally { if (done) done() }
}

const handleUpdateNode = async (formData, done) => {
  try {
    const res = await apiClient.put(`/Setting/${formData.id}`, {
      Name: formData.name, 
      Description: formData.description,
      Type: formData.type,
      Author: formData.author, 
      author_id: formData.author_id, 
      ParentId: formData.parentId, 
      MetaData: formData.metaStr ? JSON.parse(formData.metaStr) : {}
    })
    
    if (res.data && res.data.isPending) {
      alert("系统提示：变更已进入审核队列，请等待通过。");
    } else {
      await initData();
    }
    if(done) done(); 
  } catch (e) { 
    console.error("保存报错详情:", e);
    alert("保存失败: " + (e.response?.data?.message || e.message || "未知错误"));
    if(done) done(); 
  }
}

const handleDeleteNode = async (id) => {
  if(!confirm("确定要删除此文档吗？此操作不可逆。")) return
  try {
    await apiClient.delete(`/Setting/${id}`)
    selectedNodeId.value = null
    await initData()
  } catch (e) { alert("删除失败") }
}

watch(() => props.id, initData)
onMounted(initData)
</script>

<style scoped>
/* 核心布局：彻底解决高度塌陷和全屏溢出问题 */
.md-dashboard-container {
  flex: 1;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #F5F7FA;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #2C3E50;
  overflow: hidden; /* 防止溢出 */
}

/* 顶部栏 */
.md-header {
  height: 56px;
  background: #FFFFFF;
  border-bottom: 1px solid #E0E0E0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px;
  flex-shrink: 0;
  z-index: 10;
}
.header-left { display: flex; align-items: center; gap: 20px; }
.world-info { display: flex; align-items: baseline; gap: 8px; }
.world-info .title { margin: 0; font-size: 18px; font-weight: 600; }
.world-info .subtitle { font-size: 13px; color: #7F8C8D; }

/* Tabs 标签页 */
.md-tabs { display: flex; height: 100%; }
.tab-item {
  padding: 0 20px;
  display: flex;
  align-items: center;
  font-size: 14px;
  font-weight: 500;
  color: #7F8C8D;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  transition: all 0.2s ease;
}
.tab-item:hover { color: #1976D2; background: rgba(25, 118, 210, 0.04); }
.tab-item.active { color: #1976D2; border-bottom-color: #1976D2; }
.tab-item.disabled { cursor: not-allowed; opacity: 0.5; }

.header-right { display: flex; align-items: center; gap: 12px; }
.status-chip { font-size: 12px; background: #E8F5E9; color: #2E7D32; padding: 4px 10px; border-radius: 12px; }

/* 基础按钮 */
.md-btn {
  border: none; background: transparent; cursor: pointer; font-size: 14px;
  font-weight: 500; border-radius: 4px; padding: 6px 12px; transition: 0.2s;
}
.md-btn-text { color: #555; }
.md-btn-text:hover { background: rgba(0, 0, 0, 0.04); color: #000; }

/* 主体布局 */
.md-body { flex: 1; height: 0; display: flex; }
.doc-layout, .graph-layout { flex: 1; display: flex; width: 100%; height: 100%; }

/* 左侧栏 */
.md-sidebar {
  width: 280px;
  background: #FAFAFA;
  border-right: 1px solid #E0E0E0;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}
.sidebar-tools { padding: 12px; border-bottom: 1px solid #E0E0E0; display: flex; gap: 8px; }
.md-input-sm { flex: 1; border: 1px solid #DCDFE6; border-radius: 4px; padding: 6px 10px; outline: none; transition: 0.2s; }
.md-input-sm:focus { border-color: #1976D2; }
.md-btn-icon { background: #E0E0E0; border: none; border-radius: 4px; width: 32px; cursor: pointer; color: #333; font-weight: bold; }
.md-btn-icon.primary { background: #1976D2; color: #fff; }
.md-btn-icon.primary:hover { background: #1565C0; }

.doc-tree { flex: 1; overflow-y: auto; padding: 8px 0; }
.tree-node {
  padding: 8px 12px; cursor: pointer; display: flex; align-items: center; gap: 8px;
  font-size: 14px; transition: 0.1s; position: relative;
}
.tree-node:hover { background: rgba(0, 0, 0, 0.04); }
.tree-node.active { background: #E3F2FD; color: #1976D2; font-weight: 500; }
.node-name { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.node-type-tag { font-size: 11px; background: #E0E0E0; color: #555; padding: 2px 6px; border-radius: 4px; }

.node-action-btn { background: transparent; border: none; color: #999; cursor: pointer; opacity: 0; }
.tree-node:hover .node-action-btn { opacity: 1; }
.node-action-btn:hover { color: #1976D2; }

/* 右侧内容区 */
.md-main { flex: 1; display: flex; flex-direction: column; position: relative; }
.empty-state { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #999; }
.big-icon { font-size: 48px; margin-bottom: 10px; opacity: 0.5; }
.empty-text { padding: 20px; text-align: center; color: #999; font-size: 14px; }

/* 滚动条 */
.md-scroll::-webkit-scrollbar { width: 6px; }
.md-scroll::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.15); border-radius: 3px; }
.md-scroll::-webkit-scrollbar-thumb:hover { background: rgba(0,0,0,0.3); }
</style>