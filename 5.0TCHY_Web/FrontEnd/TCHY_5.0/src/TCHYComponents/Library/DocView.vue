<template>
  <div class="doc-view-container">
    <div v-if="loading" class="doc-loading">正在读取档案...</div>
    
    <div v-else class="doc-content">
      <div class="doc-header">
        <div class="stat">共收录 {{ nodes.length }} 条设定</div>
        <div class="filter-tip">已按层级分类整理</div>
      </div>

      <div 
        v-for="(group, typeName) in groupedRoots" 
        :key="typeName" 
        class="category-section"
      >
        <h2 class="category-title">
          <span class="icon">📂</span> {{ typeName }}
        </h2>

        <div class="category-list">
          <DocItem 
            v-for="rootNode in group" 
            :key="rootNode.id" 
            :node="rootNode"
            @preview="openLightbox"
          />
        </div>
      </div>

      <div v-if="Object.keys(groupedRoots).length === 0" class="empty-hint">
        暂无层级数据
      </div>
    </div>

    <transition name="fade">
      <div v-if="previewUrl" class="lightbox-overlay" @click="previewUrl = null">
        <div class="lightbox-content">
          <img :src="previewUrl" @click.stop />
          <button class="lightbox-close-btn" @click="previewUrl = null">×</button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
import apiClient from '@/utils/api';
import DocItem from './DocItem.vue'; // 引入刚才新建的组件

export default {
  name: 'DocView',
  components: { DocItem },
  props: { ipId: { type: Number, required: true } },
  data() {
    return {
      nodes: [],
      loading: false,
      previewUrl: null
    };
  },
  computed: {
    // 1. 先构建完整的树
    treeRoots() {
      if (!this.nodes.length) return [];
      
      // 深拷贝避免修改原数据
      const data = JSON.parse(JSON.stringify(this.nodes));
      const map = {};
      const roots = [];

      // 初始化 map
      data.forEach(node => {
        node.children = [];
        map[node.id] = node;
      });

      // 组装树
      data.forEach(node => {
        if (node.parentId && map[node.parentId]) {
          map[node.parentId].children.push(node);
        } else {
          // 没有父节点的是根节点
          roots.push(node);
        }
      });

      return roots;
    },

    // 2. 将根节点按 type 分组
    groupedRoots() {
      const groups = {};
      this.treeRoots.forEach(root => {
        const type = root.type || '未分类';
        if (!groups[type]) {
          groups[type] = [];
        }
        groups[type].push(root);
      });
      return groups;
    }
  },
  mounted() {
    this.fetchData();
  },
  methods: {
    async fetchData() {
      this.loading = true;
      try {
        const res = await apiClient.get(`/Setting/ip/${this.ipId}`);
        this.nodes = res.data;
      } catch (e) {
        console.error(e);
      } finally {
        this.loading = false;
      }
    },
    openLightbox(url) {
      this.previewUrl = url;
    }
  }
};
</script>

<style scoped>
.doc-view-container {
  padding: 30px;
  background: #f4f6f9;
  height: 100%;
  overflow-y: auto;
  box-sizing: border-box;
}

.doc-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 30px;
  color: #666;
  border-bottom: 1px solid #ddd;
  padding-bottom: 10px;
}

/* 分类区块 */
.category-section {
  margin-bottom: 40px;
}

.category-title {
  font-size: 1.4rem;
  color: #2c3e50;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  border-left: 5px solid #1890ff;
  padding-left: 15px;
}
.category-title .icon { margin-right: 10px; font-size: 1.2rem; }

.category-list {
  /* 给根节点列表稍微加点间距 */
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.doc-loading { text-align: center; margin-top: 50px; color: #999; }
.empty-hint { text-align: center; margin-top: 50px; color: #ccc; }

/* Lightbox 样式 (复制之前的即可) */
.lightbox-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.85); z-index: 9999; display: flex; align-items: center; justify-content: center; }
.lightbox-content { position: relative; max-width: 90vw; max-height: 90vh; }
.lightbox-content img { max-width: 100%; max-height: 90vh; border-radius: 4px; box-shadow: 0 5px 20px rgba(0,0,0,0.5); }
.lightbox-close-btn { position: absolute; top: -40px; right: -40px; font-size: 2rem; color: white; background: none; border: none; cursor: pointer; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>