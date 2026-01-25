<template>
  <div class="lingmai-integrated-module">
    <aside class="module-sidebar">
      <div class="sidebar-header">
        <span class="title">灵脉空间</span>
        <button class="add-btn" title="新建顶级页面" @click="createRootNote">＋</button>
      </div>
      
      <div class="sidebar-menu">
        <div 
          class="menu-item special-item" 
          :class="{ active: viewMode === 'graph' }"
          @click="switchToGraph"
        >
          <span class="icon">🕸️</span> 全局关系图谱
        </div>
        
        <div 
          class="menu-item" 
          :class="{ active: viewMode === 'guide' }"
          @click="switchToGuide"
        >
          <span class="icon">📖</span> 使用指南
        </div>
      </div>

      <div class="sidebar-divider"></div>

      <div class="tree-container">
        <SidebarItem
          v-for="node in treeData"
          :key="node.Id"
          :item="node"
          :depth="0"
          :active-id="activeNoteId" 
          @select="handleSelect" 
          @refresh="fetchTree"
        />
      </div>

      <div class="usage-footer">
        <div class="label">存储: 12%</div>
        <div class="bar"><div class="fill" style="width: 12%"></div></div>
      </div>
    </aside>

    <main class="module-editor">
      
      <GraphView 
        v-if="viewMode === 'graph'" 
        @node-click="handleSelect" 
      />

      <LingMaiGuide 
        v-else-if="viewMode === 'guide'"
        @start="createRootNote"
      />

      <div v-else-if="viewMode === 'editor' && currentId" class="editor-wrapper">
        <LingMaiEditor 
          :key="currentId" 
          :note-id="currentId" 
          @navigate="handleSelect" 
          @deleted="handleNoteDeleted"
        />
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
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api'

// 引入子组件
import SidebarItem from './SidebarItem.vue'
import LingMaiEditor from './LingMaiEditor.vue' 
import GraphView from './GraphView.vue' 
import LingMaiGuide from './LingMaiGuide.vue' // 🔥 修改 3：引入指南组件

// 状态管理
const currentId = ref(null)
const treeData = ref([])
const viewMode = ref('editor') // 'editor' | 'graph' | 'guide'

// 只有在编辑器模式下才高亮左侧树节点
const activeNoteId = computed(() => viewMode.value === 'editor' ? currentId.value : null)

const fetchTree = async () => {
  try {
    const res = await apiClient.get('/Notes/tree')
    treeData.value = res.data
    
    // 默认进入逻辑：如果有笔记，选第一个；如果没有，显示指南
    if (!currentId.value && viewMode.value === 'editor') {
       if (treeData.value.length > 0) {
         currentId.value = treeData.value[0].Id
       } else {
         viewMode.value = 'guide' // 如果没有任何笔记，直接进指南
       }
    }
  } catch (e) {
    console.error("加载灵脉树失败", e)
  }
}

const handleSelect = (id) => {
  console.log("切换到笔记:", id)
  currentId.value = id
  viewMode.value = 'editor' 
}

const switchToGraph = () => {
  viewMode.value = 'graph'
}

// 🔥 修改 4：切换到指南的方法
const switchToGuide = () => {
  viewMode.value = 'guide'
}

const handleNoteDeleted = () => {
  fetchTree() 
  currentId.value = null 
  viewMode.value = 'guide' // 删除后如果不自动选，可以跳回指南
}

const createRootNote = async () => {
  const newId = crypto.randomUUID()
  try {
    await apiClient.post('/Notes/save', { id: newId, title: '未命名页面', contentJson: '' })
    await fetchTree()
    handleSelect(newId) 
  } catch (e) {
    console.error("创建失败", e)
  }
}

onMounted(fetchTree)
</script>

<style lang="scss" scoped>
.lingmai-integrated-module {
  display: flex;
  height: 100%;
  width: 100%;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 4px;
  overflow: hidden;

  .module-sidebar {
    width: 240px;
    background: #f7f7f5;
    border-right: 1px solid #e8e8e7;
    display: flex;
    flex-direction: column;
    flex-shrink: 0;

    .sidebar-header {
      padding: 15px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .title { font-weight: 700; font-size: 14px; color: #37352f; }
      .add-btn { 
        border: none; background: none; cursor: pointer; color: #999; font-size: 18px; transition: color 0.2s;
        &:hover { color: #000; } 
      }
    }

    .sidebar-menu {
      padding: 0 8px;
      margin-bottom: 5px;

      .menu-item {
        padding: 6px 10px;
        font-size: 14px;
        color: #555;
        border-radius: 4px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 8px;
        transition: background 0.1s;

        &:hover { background: rgba(0,0,0,0.04); }
        &.active { background: #e0e0e0; color: #000; font-weight: 500; }
        
        &.special-item { color: #8e44ad; } 
      }
    }
    
    .sidebar-divider { height: 1px; background: #e8e8e7; margin: 5px 15px; }

    .tree-container { 
      flex: 1; 
      overflow-y: auto; 
      padding: 5px 0; 
    }

    .usage-footer {
      padding: 12px;
      border-top: 1px solid #e8e8e7;
      background: #fbfbfa;
      .label { font-size: 10px; color: #999; margin-bottom: 4px; }
      .bar { height: 3px; background: #eee; border-radius: 1px; .fill { height: 100%; background: #0078d4; border-radius: 1px; } }
    }
  }

  .module-editor {
    flex: 1;
    overflow: hidden; 
    background: #fff;
    position: relative;
    
    .editor-wrapper { height: 100%; overflow-y: auto; }
    
    .empty-prompt {
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      color: #ccc;
      
      .icon { font-size: 60px; margin-bottom: 20px; opacity: 0.8; }
      p { margin-bottom: 20px; font-size: 16px; }
      
      .btn-group {
        display: flex;
        gap: 12px;
      }
      
      .link-btn {
        padding: 8px 20px;
        background: #fff;
        border: 1px solid #eee;
        border-radius: 4px;
        color: #666;
        cursor: pointer;
        transition: all 0.2s;
        &:hover { background: #f5f5f5; color: #333; }
        
        &.primary {
           background: #2383e2;
           color: #fff;
           border: none;
           &:hover { background: #1a6fb0; }
        }
      }
    }
  }
}
</style>