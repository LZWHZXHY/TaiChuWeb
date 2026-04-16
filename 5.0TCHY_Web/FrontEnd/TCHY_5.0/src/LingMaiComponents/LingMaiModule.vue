<template>
  <div class="lingmai-container">
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="space-group">
          <div class="space-selector-wrapper">
            <select v-model="currentSpaceId" class="space-selector" @change="handleSpaceChange">
              <option v-for="space in spaces" :key="space.id" :value="space.id">
                {{ space.name }}
              </option>
            </select>
            <button class="icon-btn tiny" title="新建空间" @click="createNewSpace">
              <span class="icon">＋</span>
            </button>
          </div>
        </div>
        <button class="icon-btn primary-btn" title="新建顶级页面" @click="createRootNote">
          <span class="icon">＋</span>
        </button>
      </div>

      <nav class="nav-menu">
        <div class="nav-item graph-link" :class="{ active: viewMode === 'graph' }" @click="switchToGraph">
          <span class="item-icon">🕸️</span> 全局关系图谱
        </div>
        <div class="nav-item" :class="{ active: viewMode === 'guide' }" @click="switchToGuide">
          <span class="item-icon">📖</span> 使用指南
        </div>
      </nav>

      <div class="divider"></div>

      <div 
        class="tree-wrapper"
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
        
        <div class="drop-indicator" v-if="draggingItem">
          释放以设为顶级页面
        </div>
      </div>

      <footer class="usage-stats">
        <div class="stats-label">
          <span>节点资产</span>
          <span class="count">{{ usage.used }} / {{ usage.total }}</span>
        </div>
        <div class="progress-bar">
          <div 
            class="progress-fill" 
            :style="{ width: usage.percent + '%' }"
            :class="{ 'warning': usage.percent > 80, 'danger': usage.percent > 95 }"
          ></div>
        </div>
      </footer>
    </aside>

    <main class="editor-main">
      <GraphView v-if="viewMode === 'graph'" :space-id="currentSpaceId" @node-click="handleSelect" />
      
      <LingMaiGuide v-else-if="viewMode === 'guide'" @start="createRootNote" />
      
      <div v-else-if="viewMode === 'editor' && currentId" class="content-viewport">
        <LingMaiEditor 
          :key="currentId" 
          :note-id="currentId" 
          :space-id="currentSpaceId" 
          @navigate="handleSelect" 
          @deleted="handleNoteDeleted" 
          @save-success="fetchUsage" 
        />
      </div>

      <div v-else class="empty-state">
        <div class="empty-hero">🌌</div>
        <h3>灵脉 · 漫游于思维深处</h3>
        <p>选择一个页面或创建一个新灵感开始探索</p>
        <div class="action-row">
          <button class="btn btn-outline" @click="switchToGuide">阅读手册</button>
          <button class="btn btn-primary" @click="createRootNote">开启新篇章</button>
        </div>
      </div>
    </main>

    <ConfirmModal 
      v-model:visible="showDeleteModal"
      title="确认删除"
      message="此操作将永久抹除该页面及其所有子页面的存在，是否继续？"
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

// --- 状态管理 ---
const spaces = ref([])
const currentSpaceId = ref(null)
const currentId = ref(null)
const treeData = ref([])
const viewMode = ref('editor') 
const showDeleteModal = ref(false)
const deleteTargetId = ref(null)
const draggingItem = ref(null)

// 节点统计数据：初始为空，通过 fetchUsage 获取
const usage = ref({ used: 0, total: 500, percent: 0 })

const activeNoteId = computed(() => viewMode.value === 'editor' ? currentId.value : null)

// --- 数据接口调用 ---

// 1. 获取漫游者的笔记节点使用统计
const fetchUsage = async () => {
  try {
    const res = await apiClient.get('/Spaces/usage-stats')
    usage.value = res.data
  } catch (e) { 
    console.error("统计获取失败", e) 
  }
}

// 2. 初始化空间列表及第一个空间的数据
const initSpaces = async () => {
  try {
    const res = await apiClient.get('/Spaces/list')
    if (res.data && res.data.length > 0) {
      spaces.value = res.data
      if (!currentSpaceId.value) currentSpaceId.value = res.data[0].id 
      await fetchTree()
    } else {
      // 若无空间，自动创建一个默认空间
      const createRes = await apiClient.post('/Spaces/create', { name: '漫游空间' })
      spaces.value = [{ id: createRes.data.id, name: createRes.data.name }]
      currentSpaceId.value = createRes.data.id
      await fetchTree()
    }
    fetchUsage() // 加载统计
  } catch (e) { 
    console.error("初始化失败", e) 
  }
}

// 3. 加载当前空间下的笔记树状结构
const fetchTree = async () => {
  if (!currentSpaceId.value) return
  try {
    const res = await apiClient.get(`/Notes/tree/${currentSpaceId.value}`)
    treeData.value = res.data
    // 自动选中逻辑
    if (!currentId.value && viewMode.value === 'editor' && treeData.value.length > 0) {
       currentId.value = treeData.value[0].id
    } else if (treeData.value.length === 0) {
      viewMode.value = 'guide'
    }
  } catch (e) { 
    console.error("树结构加载失败", e) 
  }
}

// --- 交互逻辑 ---

const handleSpaceChange = () => {
  currentId.value = null
  viewMode.value = 'guide' 
  fetchTree()
}

const createNewSpace = async () => {
  const name = prompt("请输入新空间名称")
  if (!name?.trim()) return
  try {
    const res = await apiClient.post('/Spaces/create', { name: name.trim() })
    currentSpaceId.value = res.data.id
    await initSpaces()
  } catch (e) { 
    alert("创建空间失败") 
  }
}

const handleSelect = (id) => { 
  currentId.value = id
  viewMode.value = 'editor' 
}

// 创建顶级笔记节点
const createRootNote = async () => {
  if (!currentSpaceId.value) return
  const newId = crypto.randomUUID()
  try { 
    await apiClient.post('/Notes/save', { 
      id: newId, 
      title: '未命名笔记', 
      contentJson: '',
      spaceId: currentSpaceId.value
    })
    await fetchTree()
    handleSelect(newId)
    fetchUsage() // 新增后刷新统计
  } catch (e) { 
    console.error("创建笔记失败", e) 
  }
}

const handleSidebarDelete = (id) => { 
  deleteTargetId.value = id
  showDeleteModal.value = true 
}

const executeDelete = async () => {
  try {
    await apiClient.delete(`/Notes/delete/${deleteTargetId.value}`)
    if (currentId.value === deleteTargetId.value) {
      currentId.value = null
      viewMode.value = 'guide'
    }
    await fetchTree()
    fetchUsage() // 删除后刷新统计
  } catch (e) { 
    alert("删除失败") 
  }
}

const handleNoteDeleted = () => {
  fetchTree()
  currentId.value = null
  viewMode.value = 'guide'
  fetchUsage()
}

// --- 拖拽移动逻辑 ---
const handleDragStart = (item) => { draggingItem.value = item }

const handleDropOn = async (targetItem) => {
  if (!draggingItem.value || draggingItem.value.id === targetItem.id) return
  try {
    await apiClient.post('/Notes/move', { id: draggingItem.value.id, parentId: targetItem.id })
    await fetchTree()
  } finally { 
    draggingItem.value = null 
  }
}

const handleDropToRoot = async () => {
  if (!draggingItem.value) return
  try {
    await apiClient.post('/Notes/move', { id: draggingItem.value.id, parentId: null })
    await fetchTree()
  } finally { 
    draggingItem.value = null 
  }
}

const switchToGraph = () => { viewMode.value = 'graph' }
const switchToGuide = () => { viewMode.value = 'guide' }

onMounted(initSpaces)
</script>

<style lang="scss" scoped>
@use "sass:color"; // 使用现代 Sass 模块处理颜色函数

// --- 设计变量 ---
$sidebar-bg: #f9f9f8;
$border-color: #edeceb;
$text-main: #37352f;
$text-secondary: #7a7a78;
$accent-blue: #2383e2;
$accent-purple: #8e44ad;
$hover-bg: rgba(55, 53, 47, 0.06);
$font-mono: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, monospace;

.lingmai-container {
  display: flex;
  height: 100%;
  width: 100%;
  background: #ffffff;
  color: $text-main;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;

  // 侧边栏样式优化
  .sidebar {
    width: 260px;
    background: $sidebar-bg;
    border-right: 1px solid $border-color;
    display: flex;
    flex-direction: column;
    user-select: none;

    .sidebar-header {
      padding: 16px 12px 8px;
      display: flex;
      justify-content: space-between;
      align-items: center;

      .space-group {
        display: flex;
        align-items: center;
        background: rgba(0,0,0,0.04);
        padding: 4px 8px;
        border-radius: 6px;
        max-width: 180px;

        .space-selector {
          background: transparent;
          border: none;
          font-weight: 600;
          font-size: 13px;
          color: $text-main;
          outline: none;
          cursor: pointer;
          overflow: hidden;
          text-overflow: ellipsis;
        }
      }
    }

    .nav-menu {
      padding: 8px;
      .nav-item {
        padding: 6px 12px;
        font-size: 14px;
        border-radius: 5px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 8px;
        color: $text-secondary;
        transition: 0.1s ease;

        &:hover { background: $hover-bg; color: $text-main; }
        &.active { background: #eee; color: $text-main; font-weight: 500; }
        &.graph-link { 
          color: $accent-purple; 
          &:hover { color: color.adjust($accent-purple, $lightness: -10%); } 
        }
      }
    }

    .divider { height: 1px; background: $border-color; margin: 8px 16px; }

    .tree-wrapper {
      flex: 1;
      overflow-y: auto;
      padding: 0 4px;
      position: relative;

      .drop-indicator {
        margin: 12px;
        padding: 16px;
        border: 2px dashed #ddd;
        border-radius: 8px;
        font-size: 12px;
        color: #aaa;
        text-align: center;
      }
    }

    // 统计底部：已用节点数/总节点数显示
    .usage-stats {
      padding: 16px;
      border-top: 1px solid $border-color;
      background: rgba(0,0,0,0.01);

      .stats-label {
        display: flex;
        justify-content: space-between;
        font-size: 11px;
        color: #999;
        margin-bottom: 6px;
        font-weight: 500;
        .count { font-family: $font-mono; }
      }

      .progress-bar {
        height: 5px;
        background: #eee;
        border-radius: 10px;
        overflow: hidden;
        .progress-fill {
          height: 100%;
          background: $accent-blue;
          transition: width 0.5s cubic-bezier(0.4, 0, 0.2, 1);
          &.warning { background: #f2994a; } // > 80%
          &.danger { background: #eb5757; }  // > 95%
        }
      }
    }
  }

  // 主编辑区视图
  .editor-main {
    flex: 1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    background: #fff;

    .content-viewport {
      flex: 1;
      height: 100%;
      overflow-y: auto;
      scroll-behavior: smooth;
    }

    .empty-state {
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 40px;

      .empty-hero { font-size: 64px; margin-bottom: 24px; filter: grayscale(0.5); }
      h3 { font-size: 20px; font-weight: 500; margin-bottom: 8px; color: #111; }
      p { color: $text-secondary; font-size: 14px; margin-bottom: 24px; }
      
      .action-row {
        display: flex;
        gap: 12px;
      }
    }
  }
}

// 通用组件风格优化
.icon-btn {
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #888;
  transition: 0.2s;

  &:hover { background: $hover-bg; color: #333; }
  &.primary-btn { color: $text-secondary; &:hover { color: $text-main; } }
}

.btn {
  padding: 8px 18px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: 0.2s;

  &.btn-primary {
    background: $accent-blue;
    color: white;
    border: none;
    &:hover { background: color.adjust($accent-blue, $lightness: -5%); }
  }

  &.btn-outline {
    background: white;
    border: 1px solid $border-color;
    color: $text-main;
    &:hover { background: $sidebar-bg; }
  }
}
</style>