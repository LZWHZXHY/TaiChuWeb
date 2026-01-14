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
        <div class="mode-btn active"><span class="icon">📄</span> DOCS</div>
        <div class="mode-btn disabled"><span class="icon">🕸️</span> GRAPH</div>
        <div class="mode-btn disabled"><span class="icon">⏳</span> TIME</div>
      </div>

      <div class="header-right">
        <button class="cyber-btn-header" @click="showTeamModal = true">
          👥 TEAM
        </button>
        <div class="status-indicator online">SYNCED</div>
      </div>
    </header>

    <div class="dashboard-body">
      
      <div class="doc-layout">
        
        <aside class="doc-sidebar">
          <div class="sidebar-tools">
            <input v-model="searchQuery" class="cyber-input-sm" placeholder="FILTER_NODES..." />
            <button class="cyber-btn-sm" @click="createNewNode">+ NODE</button>
          </div>
          
          <div class="doc-tree custom-scroll">
            <div v-if="loading" class="loading-txt">LOADING_TREE...</div>
            
            <div 
              v-for="item in flattenedTree" 
              :key="item.id"
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
    </div>

    <CollaboratorModal 
      :show="showTeamModal" 
      :ip-id="id" 
      @close="showTeamModal = false" 
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'
import WorldDocViewer from './WorldDocViewer.vue'
import CollaboratorModal from './CollaboratorModal.vue'

const props = defineProps({ id: { type: [String, Number], required: true } })
const emit = defineEmits(['close'])

// --- State ---
const loading = ref(false)
const rawNodes = ref([]) 
const worldInfo = ref({})
const selectedNodeId = ref(null)
const searchQuery = ref('')
const showTeamModal = ref(false)

// --- Computed: Tree Logic ---
const flattenedTree = computed(() => {
  const map = {}
  const roots = []
  // 深拷贝，防止污染原数据
  const nodes = JSON.parse(JSON.stringify(rawNodes.value))
  
  // 1. 初始化 Map
  nodes.forEach(node => { 
    map[node.id] = { ...node, children: [], level: 0 } 
  })
  
  // 2. 构建树形关系
  nodes.forEach(node => {
    const mappedNode = map[node.id]
    // 搜索模式下忽略层级，只显示匹配项
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase()
      if (node.name.toLowerCase().includes(q) || node.type.toLowerCase().includes(q)) {
        roots.push(mappedNode)
      }
    } else {
      // 正常模式：挂载到父节点或作为根节点
      if (node.parentId && map[node.parentId]) {
        map[node.parentId].children.push(mappedNode)
      } else {
        roots.push(mappedNode)
      }
    }
  })
  
  // 3. 递归展平为列表
  const result = []
  const traverse = (nodes, level) => {
    nodes.forEach(node => {
      node.level = level
      result.push(node)
      if (node.children && node.children.length > 0) {
        traverse(node.children, level + 1)
      }
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

// --- Helpers ---
const getNodeIcon = (type) => {
  const t = (type || '').toLowerCase()
  if (t.includes('角色') || t.includes('character')) return '👤'
  if (t.includes('星') || t.includes('planet')) return '🪐'
  if (t.includes('城') || t.includes('city') || t.includes('地')) return '📍'
  if (t.includes('列表') || t.includes('list')) return '📁'
  if (t.includes('装备') || t.includes('item')) return '⚔️'
  if (t.includes('律') || t.includes('规则')) return '⚖️'
  return '📄'
}

// --- Actions ---

const initData = async () => {
  if (!props.id) return
  loading.value = true
  try {
    // 并行请求提高速度
    const [ipRes, nodesRes] = await Promise.all([
      apiClient.get(`/ip/${props.id}`),
      apiClient.get(`/Setting/ip/${props.id}`)
    ])
    
    worldInfo.value = ipRes.data
    rawNodes.value = nodesRes.data || []
  } catch (e) {
    console.error("Failed to load world data:", e)
  } finally {
    loading.value = false
  }
}

// 选中节点
const selectNode = (item) => {
  selectedNodeId.value = item.id
}

// 处理从子组件传来的“跳转到关联节点”请求
const handleSelectNode = (targetId) => {
  const target = rawNodes.value.find(n => n.id === targetId)
  if (target) {
    selectNode(target)
  } else {
    alert("Target node not found in current sector.")
  }
}

// 创建新节点
const createNewNode = async () => {
  const name = prompt("ENTER_NODE_NAME:")
  if (!name) return
  try {
    // 默认创建在当前选中节点之下
    const parentId = selectedNodeId.value || null
    await apiClient.post('/Setting', { 
      IpId: parseInt(props.id), 
      Name: name, 
      Type: '待定', // 默认类型，可在编辑中修改
      ParentId: parentId 
    })
    await initData()
  } catch(e) {
    alert("CREATE_FAILED: " + (e.response?.data || e.message))
  }
}

// 处理更新 (来自 WorldDocViewer)
const handleUpdateNode = async (formData, done) => {
  try {
    await apiClient.put(`/Setting/${formData.id}`, {
      Name: formData.name,
      Description: formData.description,
      Author: formData.author, // ✅ 确保使用表单中的新作者名
      MetaData: JSON.parse(formData.metaStr)
    })
    
    // 如果父节点变了，处理移动逻辑
    if (formData.parentId !== currentNode.value.parentId) {
       await apiClient.post('/Setting/move', { 
         NodeId: formData.id, 
         TargetParentId: formData.parentId 
       })
    }
    
    await initData()
    selectedNodeId.value = formData.id // 保持选中状态
    if(done) done() 
  } catch (e) { 
    alert("SAVE_FAILED: " + (e.response?.data || e.message))
    if(done) done()
  }
}

// 处理删除
const handleDeleteNode = async (id) => {
  if(!confirm("CONFIRM_DELETION? This action cannot be undone.")) return
  try {
    await apiClient.delete(`/Setting/${id}`)
    selectedNodeId.value = null
    await initData()
  } catch (e) {
    alert("DELETE_FAILED: " + (e.response?.data || e.message))
  }
}

// 监听 ID 变化（路由参数变化时重载）
watch(() => props.id, (newVal) => {
  if(newVal) initData()
})

onMounted(() => {
  initData()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心主题变量 (Light Industrial) --- */
.dashboard-industrial {
  --bg-app: #E0DDD5; 
  --bg-panel: #F4F1EA; 
  --border-color: #111111; 
  --text-main: #111111; 
  --text-sub: #666666; 
  --accent: #D92323; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  color: var(--text-main); 
  font-family: var(--mono);
}

/* 全屏覆盖层 */
.full-screen-overlay {
  position: absolute; 
  top: 0; left: 0; width: 100%; height: 100%;
  background: var(--bg-app); 
  z-index: 50; 
  display: flex; flex-direction: column;
}

/* 1. Header Styles */
.dashboard-header {
  height: 55px; 
  background: var(--bg-panel); 
  border-bottom: 2px solid var(--border-color);
  display: flex; justify-content: space-between; align-items: center; 
  padding: 0 15px; 
  flex-shrink: 0;
}

.header-left { display: flex; gap: 15px; align-items: center; }
.back-btn { 
  background: transparent; border: 2px solid var(--border-color); 
  padding: 5px 15px; cursor: pointer; 
  font-family: var(--mono); font-weight: bold; 
  transition: 0.2s; 
}
.back-btn:hover { background: var(--border-color); color: var(--bg-panel); }

.world-info { display: flex; align-items: center; gap: 8px; }
.sys-icon { 
  background: var(--border-color); color: var(--bg-panel); 
  padding: 2px 5px; font-weight: bold; font-size: 0.8rem; 
}
.w-name { margin: 0; font-size: 1.1rem; letter-spacing: 1px; }
.w-sub { font-size: 0.6rem; color: var(--text-sub); margin-left: 5px; font-weight: bold; }

/* 模式切换 */
.mode-switcher { display: flex; gap: -1px; }
.mode-btn { 
  padding: 6px 15px; font-size: 0.8rem; font-weight: bold; cursor: pointer; 
  border: 1px solid var(--border-color); background: #eee; color: #999; 
  display: flex; align-items: center; gap: 5px; margin-right: -1px;
}
.mode-btn.active { 
  background: var(--text-main); color: var(--bg-panel); 
  border-color: var(--text-main); z-index: 2; 
}
.mode-btn.disabled { opacity: 0.5; cursor: not-allowed; }

/* Header Right */
.header-right { display: flex; align-items: center; gap: 15px; }
.cyber-btn-header { 
  background: var(--text-main); color: #fff; border: none; 
  padding: 5px 15px; font-family: var(--heading); letter-spacing: 1px; 
  cursor: pointer; transition: 0.2s;
}
.cyber-btn-header:hover { background: var(--accent); transform: scale(1.05); }

.status-indicator { 
  font-size: 0.7rem; color: var(--accent); font-weight: bold; 
  border: 1px solid var(--accent); padding: 2px 6px; 
}

/* 2. Body Layout */
.dashboard-body { flex: 1; overflow: hidden; display: flex; position: relative; }
.doc-layout { display: flex; width: 100%; height: 100%; }

/* Sidebar */
.doc-sidebar { 
  width: 280px; background: #EEECE6; 
  border-right: 2px solid var(--border-color); 
  display: flex; flex-direction: column; flex-shrink: 0; 
}
.sidebar-tools { 
  padding: 10px; border-bottom: 1px solid #ccc; 
  display: flex; gap: 5px; background: #e8e8e8; 
}
.cyber-input-sm { 
  flex: 1; background: #fff; border: 1px solid #999; 
  padding: 5px; outline: none; font-family: var(--mono); 
}
.cyber-btn-sm { 
  background: var(--text-main); color: #fff; border: none; 
  padding: 0 10px; font-weight: bold; cursor: pointer; 
}
.cyber-btn-sm:hover { background: var(--accent); }

.doc-tree { flex: 1; overflow-y: auto; padding: 10px 0; }
.tree-node { 
  padding: 8px 10px; cursor: pointer; 
  display: flex; align-items: center; gap: 8px; 
  border-bottom: 1px dashed #ccc; transition: 0.1s;
}
.tree-node:hover { background: #fff; }
.tree-node.active { background: var(--text-main); color: #fff; }
.node-icon { font-size: 1rem; width: 20px; text-align: center; }
.node-name { 
  font-size: 0.9rem; overflow: hidden; text-overflow: ellipsis; 
  white-space: nowrap; font-weight: bold; 
}
.node-type-tag { 
  margin-left: auto; font-size: 0.6rem; background: #ccc; 
  color: #333; padding: 1px 4px; border-radius: 2px; 
}
.tree-node.active .node-type-tag { background: #555; color: #eee; }

/* Main Viewer */
.doc-main { 
  flex: 1; background: var(--bg-app); overflow-y: auto; 
  display: flex; flex-direction: column; align-items: center; padding: 20px; 
}

.empty-state { height: 100%; display: flex; align-items: center; justify-content: center; }
.empty-box { text-align: center; color: #999; opacity: 0.5; }
.big-icon { font-size: 4rem; display: block; margin-bottom: 10px; }
.loading-txt { text-align: center; padding: 20px; color: #999; font-size: 0.8rem; }

/* Scrollbar */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #ccc; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
</style>