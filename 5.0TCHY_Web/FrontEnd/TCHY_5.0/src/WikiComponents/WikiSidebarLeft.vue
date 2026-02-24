<template>
  <aside class="sidebar-left">
    <div class="sidebar-content">
      <h4 class="sidebar-title">知识库目录</h4>
      <ul class="tree-menu">
        <li v-for="folder in treeData" :key="folder.id" class="tree-group">
          <div class="tree-item" @click="folder.isOpen = !folder.isOpen">
            <span class="arrow">{{ folder.isOpen ? '▾' : '▸' }}</span> {{ folder.title }}
          </div>
          <ul v-show="folder.isOpen" class="sub-tree">
            <li 
              v-for="doc in folder.children" :key="doc.id"
              :class="['tree-item', { current: activeId === doc.id }]"
              @click="$emit('select-article', doc.id)"
            >
              {{ doc.title }}
            </li>
          </ul>
        </li>
      </ul>
    </div>
  </aside>
</template>

<script setup>
defineProps({
  treeData: Array,
  activeId: String
})
defineEmits(['select-article'])
</script>

<style scoped>
.sidebar-left { width: 280px; overflow-y: auto; border-right: 1px solid #e2e8f0; background: #fff; }
.sidebar-content { padding: 24px 16px; }
.sidebar-title { font-size: 12px; color: #64748b; margin-bottom: 12px; }
ul { list-style: none; padding: 0; margin: 0; }
.tree-item { padding: 6px 12px; border-radius: 6px; cursor: pointer; color: #64748b; font-size: 14px; }
.tree-item:hover { background: #f1f5f9; color: #0f172a; }
.tree-item.current { background: #eff6ff; color: #2563eb; font-weight: 500; }
.sub-tree { padding-left: 20px; margin-top: 4px; }
</style>