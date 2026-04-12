<template>
  <aside class="sidebar-left">
    <div class="sidebar-header">
      <h4 class="sidebar-title">
        <span class="icon">🌌</span> 寰宇知识库
      </h4>
      <div class="sidebar-stat" v-if="treeData.length > 0">
        共 {{ totalArticles }} 个条目
      </div>
    </div>

    <div class="sidebar-scroll-area">
      <nav class="tree-nav">
        <TreeItem 
          v-for="node in treeData" 
          :key="node.uKey || (node.Type + node.Id)"
          :node="node"
          :activeId="activeId"
          :isAdmin="isAdmin"
          @select-article="$emit('select-article', $event)"
          @edit-category="$emit('edit-category', $event)"
        />
      </nav>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/utils/auth'
import TreeItem from './TreeItem.vue'

const props = defineProps({ 
  treeData: { type: Array, default: () => [] }, 
  activeId: String 
})
defineEmits(['select-article', 'edit-category'])

const authStore = useAuthStore()

// 🛡️ 同步最强权限判断逻辑，确保按钮不消失
const isAdmin = computed(() => {
  const user = authStore.user?.data || authStore.user || {};
  const roles = (user.RoleCodes?.length ? user.RoleCodes : null) || 
                (user.roleCodes?.length ? user.roleCodes : null) || 
                [];
  return roles.includes('WikiAdmin') || roles.includes('SuperAdmin');
});

// 计算总文章数（简单递归统计）
const totalArticles = computed(() => {
  let count = 0;
  const traverse = (nodes) => {
    nodes.forEach(n => {
      if (n.Type === 'article' || n.type === 'article') count++;
      const children = n.Children || n.children;
      if (children) traverse(children);
    });
  }
  traverse(props.treeData);
  return count;
});
</script>

<style scoped>
.sidebar-left {
  width: 280px;
  height: 100%;
  background: #fcfcfd; /* 极淡的灰蓝色背景 */
  border-right: 1px solid #eef2f6;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
}

/* 顶部标题区 */
.sidebar-header {
  padding: 24px 20px 12px 20px;
}

.sidebar-title {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: #1e293b;
  display: flex;
  align-items: center;
  gap: 8px;
  letter-spacing: 0.5px;
}

.sidebar-stat {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 4px;
  margin-left: 26px;
}

/* 滚动区域优化 */
.sidebar-scroll-area {
  flex: 1;
  overflow-y: auto;
  padding: 0 12px 20px 12px;
}

/* 自定义滚动条，让它看起来更高级 */
.sidebar-scroll-area::-webkit-scrollbar {
  width: 4px;
}

.sidebar-scroll-area::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}

.sidebar-scroll-area:hover::-webkit-scrollbar-thumb {
  background: #cbd5e1;
}

.tree-nav {
  display: flex;
  flex-direction: column;
  gap: 2px; /* 节点之间的微小间距 */
}

/* 装饰：背景微光效果 */
.sidebar-left::before {
  content: "";
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100px;
  background: linear-gradient(180deg, rgba(37, 99, 235, 0.03) 0%, rgba(255, 255, 255, 0) 100%);
  pointer-events: none;
}
</style>