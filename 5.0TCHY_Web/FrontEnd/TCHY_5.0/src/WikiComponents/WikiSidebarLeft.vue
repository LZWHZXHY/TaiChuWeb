<template>
  <aside class="sidebar-left">
    <div class="sidebar-content">
      <h4 class="sidebar-title">知识库目录</h4>
      <ul class="tree-root">
        <TreeItem 
          v-for="node in treeData" 
          :key="node.Type + node.Id"
          :node="node"
          :activeId="activeId"
          :isAdmin="isAdmin"
          @select-article="$emit('select-article', $event)"
          @edit-category="$emit('edit-category', $event)"
        />
      </ul>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/utils/auth'
import TreeItem from './TreeItem.vue' // 引入刚才建的组件

const props = defineProps({ treeData: Array, activeId: String })
defineEmits(['select-article', 'edit-category'])

const authStore = useAuthStore()
const isAdmin = computed(() => {
  const userData = authStore.user?.data || authStore.user;
  const roles = userData?.RoleCodes || userData?.roleCodes || []; 
  return roles.includes('WikiAdmin') || roles.includes('SuperAdmin');
});
</script>

<style scoped>
.sidebar-left { width: 280px; height: 100%; background: #fff; border-right: 1px solid #f0f0f0; overflow-y: auto; }
.sidebar-content { padding: 20px 12px; }
.sidebar-title { margin: 0 0 16px 12px; font-size: 12px; font-weight: 600; color: #94a3b8; text-transform: uppercase; }
.tree-root { padding: 0; margin: 0; }
</style>