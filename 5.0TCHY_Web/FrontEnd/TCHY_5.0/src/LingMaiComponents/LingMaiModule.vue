<template>
  <div class="lingmai-integrated-module">
    <aside class="module-sidebar">
      <div class="sidebar-header">
        <div class="space-selector-wrapper">
          <select v-model="currentSpaceId" class="space-selector" @change="handleSpaceChange">
            <option v-for="space in spaces" :key="space.id" :value="space.id">
              🌌 {{ space.name }}
            </option>
          </select>
          <button class="add-space-btn" title="新建空间" @click="createNewSpace">➕</button>
        </div>
        
        <button class="add-btn" title="新建顶级页面" @click="createRootNote">＋</button>
      </div>
      
      <div class="sidebar-menu">
        <div class="menu-item special-item" :class="{ active: viewMode === 'graph' }" @click="switchToGraph">
          <span class="icon">🕸️</span> 全局关系图谱
        </div>
        <div class="menu-item" :class="{ active: viewMode === 'guide' }" @click="switchToGuide">
          <span class="icon">📖</span> 使用指南
        </div>
      </div>

      <div class="sidebar-divider"></div>

      <div 
        class="tree-container"
        @dragover.prevent 
        @drop="handleDropToRoot"
      >
        <SidebarItem
          v-for="node in treeData"
          :key="node.id"
          :item="node"
          :depth="0"
          :active-id="activeNoteId" 
          @select="handleSelect" 
          @delete="handleSidebarDelete" 
          @refresh="fetchTree"
          @drag-start="handleDragStart" 
          @drop-on="handleDropOn"
        />
        
        <div class="root-drop-zone" v-if="draggingItem">
          拖放到此处设为顶级页面
        </div>
      </div>

      <div class="usage-footer">
        <div class="label">存储: 12%</div>
        <div class="bar"><div class="fill" style="width: 12%"></div></div>
      </div>
    </aside>

    <main class="module-editor">
      <GraphView v-if="viewMode === 'graph'" :space-id="currentSpaceId" @node-click="handleSelect" />
      <LingMaiGuide v-else-if="viewMode === 'guide'" @start="createRootNote" />
      <div v-else-if="viewMode === 'editor' && currentId" class="editor-wrapper" id="main-scroll-container">
        <LingMaiEditor :key="currentId" :note-id="currentId" :space-id="currentSpaceId" @navigate="handleSelect" @deleted="handleNoteDeleted" />
      </div>
      <div v-else class="empty-prompt">
        <div class="icon">🌌</div>
        <p>灵脉笔记 · 连接你的思维</p>
        <div class="btn-group">
           <button class="link-btn" @click="switchToGuide">📖 查看指南</button>
           <button class="link-btn primary" @click="createRootNote">＋ 新建笔记</button>
        </div>
      </div>
    </main>

    <ConfirmModal 
      v-model:visible="showDeleteModal"
      title="确定要删除页面吗？"
      message="该页面及其所有子页面将被永久删除，此操作无法恢复。"
      @confirm="executeDelete"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api'
import SidebarItem from './SidebarItem.vue'
import LingMaiEditor from './LingMaiEditor.vue' 
import GraphView from './GraphView.vue' 
import LingMaiGuide from './LingMaiGuide.vue'
import ConfirmModal from './ConfirmModal.vue'

// 空间状态管理
const spaces = ref([])
const currentSpaceId = ref(null)

const currentId = ref(null)
const treeData = ref([])
const viewMode = ref('editor') 
const showDeleteModal = ref(false)
const deleteTargetId = ref(null)

const draggingItem = ref(null)

const activeNoteId = computed(() => viewMode.value === 'editor' ? currentId.value : null)

// 🔥 1. 初始化获取空间列表
const initSpaces = async () => {
  try {
    const res = await apiClient.get('/Spaces/list')
    if (res.data && res.data.length > 0) {
      spaces.value = res.data
      if (!currentSpaceId.value || !spaces.value.find(s => s.id === currentSpaceId.value)) {
        currentSpaceId.value = res.data[0].id 
      }
      await fetchTree()
    } else {
      // 自动创建默认空间
      const createRes = await apiClient.post('/Spaces/create', { name: '默认空间' })
      spaces.value = [{ id: createRes.data.id, name: createRes.data.name }]
      currentSpaceId.value = createRes.data.id
      await fetchTree()
    }
  } catch (e) {
    console.error("加载空间失败", e)
  }
}

// 🔥 2. 新建空间方法
const createNewSpace = async () => {
  const spaceName = prompt("请输入新空间的名称：", "新空间")
  if (!spaceName || !spaceName.trim()) return

  try {
    const res = await apiClient.post('/Spaces/create', { name: spaceName.trim() })
    currentSpaceId.value = res.data.id
    await initSpaces()
    await handleSpaceChange() 
  } catch (e) {
    alert("创建空间失败")
    console.error(e)
  }
}

// 切换空间时触发
const handleSpaceChange = async () => {
  currentId.value = null
  viewMode.value = 'guide' 
  await fetchTree()
}

const fetchTree = async () => {
  if (!currentSpaceId.value) return
  try {
    const res = await apiClient.get(`/Notes/tree/${currentSpaceId.value}`)
    treeData.value = res.data
    if (!currentId.value && viewMode.value === 'editor') {
       if (treeData.value.length > 0) currentId.value = treeData.value[0].id
       else viewMode.value = 'guide'
    }
  } catch (e) { console.error("加载灵脉树失败", e) }
}

const handleSelect = (id) => { currentId.value = id; viewMode.value = 'editor' }

const handleDragStart = (item) => { draggingItem.value = item }

const handleDropOn = async (targetItem) => {
  const dragged = draggingItem.value
  if (!dragged || !targetItem) return
  if (dragged.id === targetItem.id) return 
  
  try {
    await apiClient.post('/Notes/move', { id: dragged.id, parentId: targetItem.id })
    await fetchTree()
  } catch (e) {
    console.error("移动失败", e)
  } finally {
    draggingItem.value = null
  }
}

const handleDropToRoot = async (event) => {
  const dragged = draggingItem.value
  if (!dragged) return

  try {
    await apiClient.post('/Notes/move', { id: dragged.id, parentId: null })
    await fetchTree()
  } catch (e) {
    console.error("移至根目录失败", e)
  } finally {
    draggingItem.value = null
  }
}

const handleSidebarDelete = (id) => { deleteTargetId.value = id; showDeleteModal.value = true }
const executeDelete = async () => {
  if (!deleteTargetId.value) return
  try {
    await apiClient.delete(`/Notes/delete/${deleteTargetId.value}`)
    if (currentId.value === deleteTargetId.value) { currentId.value = null; viewMode.value = 'guide' }
    await fetchTree()
  } catch (e) { alert("删除失败"); console.error(e) } finally { deleteTargetId.value = null }
}

const switchToGraph = () => { viewMode.value = 'graph' }
const switchToGuide = () => { viewMode.value = 'guide' }
const handleNoteDeleted = () => { fetchTree(); currentId.value = null; viewMode.value = 'guide' }

const createRootNote = async () => {
  if (!currentSpaceId.value) return
  const newId = crypto.randomUUID()
  try { 
    await apiClient.post('/Notes/save', { 
      id: newId, 
      title: '未命名页面', 
      contentJson: '',
      spaceId: currentSpaceId.value
    }); 
    await fetchTree(); 
    handleSelect(newId) 
  } catch (e) { console.error(e) }
}

onMounted(() => {
  initSpaces()
})
</script>

<style lang="scss" scoped>
.lingmai-integrated-module {
  display: flex; flex-direction: row !important; height: 100%; width: 100%; background: #fff; border: 1px solid #ddd; border-radius: 4px; overflow: hidden;

  .module-sidebar {
    width: 240px; background: #f7f7f5; border-right: 1px solid #e8e8e7; display: flex; flex-direction: column; flex-shrink: 0;
    
    .sidebar-header { 
      padding: 15px; display: flex; justify-content: space-between; align-items: center; 
      
      /* 🔥 新增空间组的样式 */
      .space-selector-wrapper {
        display: flex; align-items: center; gap: 4px; background: rgba(0,0,0,0.03); padding: 4px; border-radius: 6px;
      }
      .space-selector {
        background: transparent; border: none; font-weight: 700; font-size: 14px; color: #37352f; outline: none; cursor: pointer; max-width: 120px;
      }
      .add-space-btn {
        background: transparent; border: none; cursor: pointer; font-size: 12px; color: #888; padding: 2px 6px; border-radius: 4px;
        &:hover { background: rgba(0,0,0,0.1); color: #000; }
      }

      .add-btn { border: none; background: none; cursor: pointer; color: #999; font-size: 18px; transition: color 0.2s; &:hover { color: #000; } } 
    }
    
    .sidebar-menu { padding: 0 8px; margin-bottom: 5px; .menu-item { padding: 6px 10px; font-size: 14px; color: #555; border-radius: 4px; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: background 0.1s; &:hover { background: rgba(0,0,0,0.04); } &.active { background: #e0e0e0; color: #000; font-weight: 500; } &.special-item { color: #8e44ad; } } }
    .sidebar-divider { height: 1px; background: #e8e8e7; margin: 5px 15px; }
    
    .tree-container { 
      flex: 1; overflow-y: auto; padding: 5px 0; 
      position: relative; 
    }
    
    .root-drop-zone {
      margin: 10px; padding: 20px; border: 2px dashed #ddd; border-radius: 6px; color: #999; font-size: 12px; text-align: center; pointer-events: none; 
    }

    .usage-footer { padding: 12px; border-top: 1px solid #e8e8e7; background: #fbfbfa; .label { font-size: 10px; color: #999; margin-bottom: 4px; } .bar { height: 3px; background: #eee; border-radius: 1px; .fill { height: 100%; background: #0078d4; border-radius: 1px; } } }
  }

  .module-editor {
    flex: 1; overflow: hidden; background: #fff; position: relative; display: flex; flex-direction: column; 
    .editor-wrapper { flex: 1; height: 100%; overflow-y: auto; position: relative; scroll-behavior: smooth; overflow-x: auto; }
    .empty-prompt { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #ccc; .icon { font-size: 60px; margin-bottom: 20px; opacity: 0.8; } p { margin-bottom: 20px; font-size: 16px; } .btn-group { display: flex; gap: 12px; } .link-btn { padding: 8px 20px; background: #fff; border: 1px solid #eee; border-radius: 4px; color: #666; cursor: pointer; transition: all 0.2s; &:hover { background: #f5f5f5; color: #333; } &.primary { background: #2383e2; color: #fff; border: none; &:hover { background: #1a6fb0; } } } }
  }
}
</style>