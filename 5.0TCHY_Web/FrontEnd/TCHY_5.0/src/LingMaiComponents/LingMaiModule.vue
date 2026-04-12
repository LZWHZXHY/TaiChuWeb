<template>
  <div class="lingmai-integrated-module">
    <aside class="module-sidebar">
      <div class="sidebar-header">
        <span class="title">灵脉空间 v 1.2</span>
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
      <GraphView v-if="viewMode === 'graph'" @node-click="handleSelect" />
      <LingMaiGuide v-else-if="viewMode === 'guide'" @start="createRootNote" />
      <div v-else-if="viewMode === 'editor' && currentId" class="editor-wrapper" id="main-scroll-container">
        <LingMaiEditor :key="currentId" :note-id="currentId" @navigate="handleSelect" @deleted="handleNoteDeleted" />
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

const currentId = ref(null)
const treeData = ref([])
const viewMode = ref('editor') 
const showDeleteModal = ref(false)
const deleteTargetId = ref(null)

const draggingItem = ref(null)

const activeNoteId = computed(() => viewMode.value === 'editor' ? currentId.value : null)

const fetchTree = async () => {
  try {
    const res = await apiClient.get('/Notes/tree')
    treeData.value = res.data
    if (!currentId.value && viewMode.value === 'editor') {
       // 修复了此处的 .id
       if (treeData.value.length > 0) currentId.value = treeData.value[0].id
       else viewMode.value = 'guide'
    }
  } catch (e) { console.error("加载灵脉树失败", e) }
}

const handleSelect = (id) => { currentId.value = id; viewMode.value = 'editor' }

// 1. 开始拖拽
const handleDragStart = (item) => {
  draggingItem.value = item
}

// 2. 拖到某个节点上 (变成子节点)
const handleDropOn = async (targetItem) => {
  const dragged = draggingItem.value
  
  if (!dragged || !targetItem) return
  // 修复了此处的 .id
  if (dragged.id === targetItem.id) return 
  
  // 修复了此处的 .id
  if (isChildOf(targetItem, dragged.id)) {
    alert("❌ 无法将父节点移动到其子节点内部")
    draggingItem.value = null
    return
  }

  try {
    await apiClient.post('/Notes/move', { 
      // 修复了此处的 .id
      id: dragged.id, 
      parentId: targetItem.id 
    })
    
    await fetchTree()
  } catch (e) {
    console.error("移动失败", e)
    try {
       // 修复了此处的 .id
       await apiClient.post('/Notes/save', { id: dragged.id, parentNoteId: targetItem.id })
       await fetchTree()
    } catch(err) { alert("移动失败，请稍后重试") }
  } finally {
    draggingItem.value = null
  }
}

// 3. 拖到空白处 (变成根节点) 
const handleDropToRoot = async (event) => {
  const dragged = draggingItem.value
  if (!dragged) return

  try {
    await apiClient.post('/Notes/move', { 
      // 修复了此处的 .id
      id: dragged.id, 
      parentId: null
    })
    await fetchTree()
  } catch (e) {
    try {
       // 修复了此处的 .id
       await apiClient.post('/Notes/save', { id: dragged.id, parentNoteId: null })
       await fetchTree()
    } catch(err) { alert("移至根目录失败") }
  } finally {
    draggingItem.value = null
  }
}

const isChildOf = (node, parentId) => {
  return false 
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
  const newId = crypto.randomUUID()
  try { await apiClient.post('/Notes/save', { id: newId, title: '未命名页面', contentJson: '' }); await fetchTree(); handleSelect(newId) } catch (e) { console.error(e) }
}
onMounted(fetchTree)
</script>

<style lang="scss" scoped>
.lingmai-integrated-module {
  display: flex; flex-direction: row !important; height: 100%; width: 100%; background: #fff; border: 1px solid #ddd; border-radius: 4px; overflow: hidden;

  .module-sidebar {
    width: 240px; background: #f7f7f5; border-right: 1px solid #e8e8e7; display: flex; flex-direction: column; flex-shrink: 0;
    .sidebar-header { padding: 15px; display: flex; justify-content: space-between; align-items: center; .title { font-weight: 700; font-size: 14px; color: #37352f; } .add-btn { border: none; background: none; cursor: pointer; color: #999; font-size: 18px; transition: color 0.2s; &:hover { color: #000; } } }
    .sidebar-menu { padding: 0 8px; margin-bottom: 5px; .menu-item { padding: 6px 10px; font-size: 14px; color: #555; border-radius: 4px; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: background 0.1s; &:hover { background: rgba(0,0,0,0.04); } &.active { background: #e0e0e0; color: #000; font-weight: 500; } &.special-item { color: #8e44ad; } } }
    .sidebar-divider { height: 1px; background: #e8e8e7; margin: 5px 15px; }
    
    .tree-container { 
      flex: 1; overflow-y: auto; padding: 5px 0; 
      position: relative; /* 为 root-drop-zone 定位 */
    }
    
    /* 拖拽时的空白区域提示 */
    .root-drop-zone {
      margin: 10px;
      padding: 20px;
      border: 2px dashed #ddd;
      border-radius: 6px;
      color: #999;
      font-size: 12px;
      text-align: center;
      pointer-events: none; /* 让事件穿透到底层 div */
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