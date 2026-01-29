<template>
  <div class="dashboard-industrial full-screen-overlay">
    
    <header class="dashboard-header">
      <div class="header-left">
        <button class="back-btn" @click="$emit('close')">&lt; EXIT_SYSTEM</button>
        <div class="world-info">
          <span class="sys-icon">DOCS</span>
          <div class="info-text">
            <h2 class="w-name">{{ worldInfo.name || 'LOADING...' }}</h2>
            <span class="w-sub">SEC-{{ id }} // KNOWLEDGE_BASE</span>
          </div>
        </div>
      </div>
      
      <div class="mode-switcher">
        <div class="mode-btn" :class="{ active: viewMode === 'docs' }" @click="viewMode = 'docs'">
          <span class="icon">📄</span> DOCS
        </div>
        <div class="mode-btn" :class="{ active: viewMode === 'graph' }" @click="viewMode = 'graph'">
          <span class="icon">🕸️</span> GRAPH
        </div>
        <div class="mode-btn disabled"><span class="icon">⏳</span> TIME</div>
      </div>

      <div class="header-right">
        <button class="cyber-btn-header alert" @click="showAuditModal = true">📢 AUDIT</button>
        <button class="cyber-btn-header" @click="showTeamModal = true">👥 TEAM</button>
        <div class="status-indicator online">SYNCED</div>
      </div>
    </header>

    <div class="dashboard-body">
      <div v-if="viewMode === 'docs'" class="doc-layout">
        <aside class="doc-sidebar">
          <div class="sidebar-tools">
            <input v-model="searchQuery" class="cyber-input-sm" placeholder="FILTER..." />
            <button class="cyber-btn-sm" @click="openNodeModal('create')">+ NODE</button>
          </div>
          
          <div class="doc-tree custom-scroll">
            <div v-if="loading" class="loading-txt">LOADING_TREE...</div>
            
            <div 
              v-for="item in flattenedTree" 
              :key="item.id"
              :id="'tree-node-' + item.id"
              class="tree-node"
              :class="{ 
                active: selectedNodeId === item.id,
                'is-parent': item.children && item.children.length > 0 
              }"
              :style="{ paddingLeft: (item.level * 15 + 15) + 'px' }"
              @click="selectNode(item)"
            >
              <span class="tree-guide" v-if="item.level > 0"></span>
              <span class="node-icon">{{ getNodeIcon(item.type) }}</span>
              <span class="node-name">{{ item.name }}</span>
              
              <button 
                v-if="!searchQuery" 
                class="node-move-btn" 
                title="Reassign Parent"
                @click.stop="openNodeModal('move', item)"
              >⇄</button>

              <span class="node-type-tag" v-if="item.level === 0">{{ item.type }}</span>
            </div>
          </div>
        </aside>

        <main class="doc-main custom-scroll">
          <WorldDocViewer 
            v-if="currentNode" 
            :node="currentNode" 
            :all-nodes="rawNodes" 
            @update-node="handleUpdateNode"
            @delete-node="handleDeleteNode"
            @select-node="handleSelectNode"
          />

          <div v-else class="empty-state">
            <div class="empty-box">
              <span class="big-icon">📂</span>
              <p>SELECT_A_FILE_TO_READ</p>
            </div>
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
// 🔥 补全之前遗漏的组件导入
import CollaboratorModal from './CollaboratorModal.vue'
import AuditModal from './AuditModal.vue'
import NodeManagerModal from './NodeManagerModal.vue'

const props = defineProps({ id: { type: [String, Number], required: true } })
const emit = defineEmits(['close'])

// --- State ---
const viewMode = ref('docs')
const loading = ref(false)
const rawNodes = ref([]) 
const worldInfo = ref({})
const selectedNodeId = ref(null)
const searchQuery = ref('')
const showTeamModal = ref(false)
const showAuditModal = ref(false)

// --- Node Manager State ---
const showNodeModal = ref(false)
const nodeModalMode = ref('create')
const nodeModalData = ref({ id: null, name: '', parentId: null })

// --- Computed: Tree Logic ---
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

// 🔥 补全之前遗漏的 Helper 函数
const getNodeIcon = (type) => {
  const t = (type || '').toLowerCase()
  if (t.includes('角色') || t.includes('character')) return '👤'
  if (t.includes('星') || t.includes('planet')) return '🪐'
  return '📄'
}

// --- Actions ---

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

// 🔥 核心跳转逻辑 (包含类型转换 + 自动滚动)
const handleGraphSelect = async (nodeId) => {
  // 1. 强制转为数字，解决 id 类型不匹配导致内容不显示的问题
  const targetId = parseInt(nodeId);
  selectedNodeId.value = targetId;
  
  // 2. 切换视图
  viewMode.value = 'docs'; 
  
  // 3. 等待 DOM 渲染
  await nextTick();
  
  // 4. 找到并滚动到列表项
  const element = document.getElementById(`tree-node-${targetId}`);
  if (element) {
    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    
    // 5. 闪烁高亮提示
    element.style.transition = 'background 0.3s';
    element.style.background = '#D92323'; 
    element.style.color = '#fff';
    setTimeout(() => {
      element.style.background = ''; 
      element.style.color = '';
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
    await apiClient.put(`/Setting/${formData.id}`, {
      Name: formData.name, 
      Description: formData.description,
      Type: formData.type,
      Author: formData.author, 
      
      // 🔥 核心修复：必须在这里要把 parentId 传给后端
      // 这里的 formData.parentId 来自子组件的 emit
      ParentId: formData.parentId, 
      
      MetaData: JSON.parse(formData.metaStr)
    })
    
    await initData() // 刷新列表，这样左侧树状图也会更新位置
    if(done) done() 
  } catch (e) { 
    console.error(e)
    alert("Save failed")
    if(done) done() 
  }
}

const handleDeleteNode = async (id) => {
  if(!confirm("Confirm deletion?")) return
  try {
    await apiClient.delete(`/Setting/${id}`)
    selectedNodeId.value = null
    await initData()
  } catch (e) { alert("Delete failed") }
}

watch(() => props.id, initData)
onMounted(initData)
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.dashboard-industrial {
  --bg-app: #E0DDD5; --bg-panel: #F4F1EA; --border-color: #111111; 
  --text-main: #111111; --text-sub: #666666; --accent: #D92323; 
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif;
  color: var(--text-main); font-family: var(--mono);
}

.full-screen-overlay { position: absolute; inset: 0; background: var(--bg-app); z-index: 50; display: flex; flex-direction: column; }

.dashboard-header { height: 55px; background: var(--bg-panel); border-bottom: 2px solid var(--border-color); display: flex; justify-content: space-between; align-items: center; padding: 0 15px; flex-shrink: 0; }
.header-left { display: flex; gap: 15px; align-items: center; }
.back-btn { background: transparent; border: 2px solid var(--border-color); padding: 5px 15px; cursor: pointer; font-family: var(--mono); font-weight: bold; }
.world-info { display: flex; align-items: center; gap: 8px; }
.sys-icon { background: var(--border-color); color: var(--bg-panel); padding: 2px 5px; font-weight: bold; font-size: 0.8rem; }
.w-name { margin: 0; font-size: 1.1rem; letter-spacing: 1px; font-family: var(--heading); }

.mode-switcher { display: flex; gap: -1px; }
.mode-btn { padding: 6px 15px; font-size: 0.8rem; font-weight: bold; cursor: pointer; border: 1px solid var(--border-color); background: #eee; color: #999; display: flex; align-items: center; gap: 5px; margin-right: -1px; transition: all 0.2s ease; }
.mode-btn.active { background: var(--text-main); color: var(--bg-panel); border-color: var(--text-main); z-index: 2; }

.header-right { display: flex; align-items: center; gap: 15px; }
.cyber-btn-header { background: var(--text-main); color: #fff; border: none; padding: 5px 15px; font-family: var(--heading); letter-spacing: 1px; cursor: pointer; }
.cyber-btn-header.alert { background: var(--accent); }
.status-indicator { font-size: 0.7rem; color: var(--accent); font-weight: bold; border: 1px solid var(--accent); padding: 2px 6px; }

.dashboard-body { flex: 1; overflow: hidden; display: flex; }
.doc-layout { display: flex; width: 100%; height: 100%; }
.graph-layout { flex: 1; width: 100%; height: 100%; position: relative; overflow: hidden; }

.doc-sidebar { width: 280px; background: #EEECE6; border-right: 2px solid var(--border-color); display: flex; flex-direction: column; flex-shrink: 0; padding-bottom: 5%;}

.sidebar-tools { padding: 10px; border-bottom: 1px solid #ccc; display: flex; gap: 5px; background: #e8e8e8; }
.cyber-input-sm { width: 120px; flex: 1; background: #fff; border: 1px solid #999; padding: 5px; outline: none; }
.cyber-btn-sm { background: var(--text-main); color: #fff; border: none; padding: 0 10px; font-weight: bold; cursor: pointer; }

.doc-tree { flex: 1; overflow-y: auto; padding: 10px 0; }
.tree-node { padding: 8px 10px; cursor: pointer; display: flex; align-items: center; gap: 8px; border-bottom: 1px dashed #ccc; transition: 0.1s; position: relative; }
.tree-node:hover { background: #fff; }
.tree-node.active { background: var(--text-main); color: #fff; }

.node-move-btn {
  margin-left: auto; background: transparent; border: 1px solid #ccc; color: #999;
  font-size: 10px; cursor: pointer; padding: 0 4px; opacity: 0; transition: 0.2s;
  flex-shrink: 0;
}
.tree-node:hover .node-move-btn { opacity: 1; }
.node-move-btn:hover { background: #111; color: #fff; border-color: #111; }

.node-name { font-size: 0.9rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-weight: bold; flex: 1; }
.node-type-tag { font-size: 0.6rem; background: #ccc; color: #333; padding: 1px 4px; border-radius: 2px; }

.doc-main { flex: 1; background: var(--bg-app); overflow-y: auto; display: flex; flex-direction: column; align-items: center; padding: 20px; }
.empty-state { height: 100%; display: flex; align-items: center; justify-content: center; }
.empty-box { text-align: center; color: #999; opacity: 0.5; }
.loading-txt { text-align: center; padding: 20px; color: #999; font-size: 0.8rem; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ccc; }
</style>