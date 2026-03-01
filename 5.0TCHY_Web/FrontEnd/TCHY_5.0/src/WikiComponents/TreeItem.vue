<template>
  <li class="tree-node">
    <div 
      class="tree-row" 
      :class="{ 
        'is-article': node.Type === 'article', 
        'is-active': activeId === String(node.Id) 
      }"
      @click="handleClick"
    >
      <span v-if="node.Type === 'folder'" class="arrow" :class="{ 'is-open': node.IsOpen }">
        ▸
      </span>
      <span class="icon">{{ node.Type === 'folder' ? '📁' : '📄' }}</span>
      <span class="title">{{ node.Title }}</span>

      <button 
        v-if="isAdmin && node.Type === 'folder'" 
        class="btn-edit-mini" 
        @click.stop="$emit('edit-category', node)"
      >
        ✏️
      </button>
    </div>

    <ul v-if="node.Type === 'folder' && node.IsOpen" class="tree-children">
      <TreeItem 
        v-for="child in node.Children" 
        :key="child.Type + child.Id"
        :node="child"
        :activeId="activeId"
        :isAdmin="isAdmin"
        @select-article="$emit('select-article', $event)"
        @edit-category="$emit('edit-category', $event)"
      />
    </ul>
  </li>
</template>

<script setup>
const props = defineProps({
  node: Object,
  activeId: String,
  isAdmin: Boolean
})
const emit = defineEmits(['select-article', 'edit-category'])

const handleClick = () => {
  if (props.node.Type === 'article') {
    emit('select-article', props.node.Id)
  } else {
    // 切换文件夹打开状态
    props.node.IsOpen = !props.node.IsOpen
  }
}
</script>

<style scoped>
.tree-node { list-style: none; }
.tree-row { 
  display: flex; align-items: center; padding: 6px 12px; border-radius: 6px;
  cursor: pointer; color: #475569; font-size: 14px; transition: 0.2s; position: relative;
}
.tree-row:hover { background: #f1f5f9; color: #0f172a; }
.tree-row.is-active { background: #eff6ff; color: #2563eb; font-weight: 600; }

.arrow { width: 16px; font-size: 10px; transition: 0.2s; color: #94a3b8; }
.arrow.is-open { transform: rotate(90deg); }
.icon { margin-right: 6px; font-size: 16px; }
.title { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.tree-children { padding-left: 20px; border-left: 1px solid #e2e8f0; margin-left: 18px; }

.btn-edit-mini { 
  opacity: 0; background: none; border: none; font-size: 12px; cursor: pointer; padding: 2px 4px;
}
.tree-row:hover .btn-edit-mini { opacity: 1; }
</style>