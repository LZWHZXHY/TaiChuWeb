<template>
  <div class="relations-panel">
    <div class="panel-title">
      <h4>🔗 人际与实体关系</h4>
      <small>当前节点ID: {{ currentNode.id }}</small>
    </div>

    <div class="relations-list">
      <div v-for="rel in currentRelations" :key="rel.id" class="relation-item">
        <span class="rel-tag">{{ rel.relation_type }}</span>
        <span class="rel-arrow">➡️</span>
        <span class="rel-target" @click="$emit('select-node', rel.targetId)">
          {{ rel.targetName }}
        </span>
        <button class="delete-btn" @click="deleteRelation(rel.id)">×</button>
      </div>
      <div v-if="currentRelations.length === 0" class="empty-text">
        暂无特殊关系连接
      </div>
    </div>

    <div class="add-relation-box">
      <input v-model="newRel.type" placeholder="关系 (如:好友)" class="small-input" :disabled="isSubmitting" />
      <span class="rel-arrow">➡️</span>
      
      <select v-model="newRel.targetId" class="small-select" :disabled="isSubmitting">
        <option :value="null" disabled>选择目标...</option>
        <option v-for="node in otherNodes" :key="node.id" :value="node.id">
          {{ node.name }} ({{ node.type }})
        </option>
      </select>

      <button class="add-btn-small" @click="addRelation" :disabled="!canAddRelation || isSubmitting">
        {{ isSubmitting ? '添加中...' : '添加' }}
      </button>
    </div>
  </div>
</template>

<script>
import apiClient from '@/utils/api';

export default {
  name: 'NodeRelationPanel',
  props: {
    currentNode: { type: Object, required: true },
    allNodes: { type: Array, required: true }
  },
  data() {
    return {
      currentRelations: [],
      isSubmitting: false,
      newRel: { type: '', targetId: null }
    };
  },
  computed: {
    otherNodes() { return this.allNodes.filter(n => n.id !== this.currentNode.id); },
    canAddRelation() { return this.newRel.type && this.newRel.targetId; }
  },
  watch: {
    'currentNode.id': {
      immediate: true,
      handler(newId) {
        if (newId) { this.fetchRelations(); } else { this.currentRelations = []; }
      }
    }
  },
  methods: {
    async fetchRelations() {
      try {
        // 🔴 修改点：请求 graph-relations 接口！
        const res = await apiClient.get(`/Setting/${this.currentNode.id}/graph-relations`);
        this.currentRelations = res.data.map(rel => ({ ...rel, targetId: Number(rel.targetId) }));
      } catch (e) { console.error("获取关系失败", e); }
    },
    async addRelation() {
      this.isSubmitting = true;
      try {
        const targetIdNum = Number(this.newRel.targetId);
        await apiClient.post('/Setting/relation', {
          sourceNodeId: this.currentNode.id,
          targetNodeId: targetIdNum,
          relationType: this.newRel.type,
          description: ''
        });
        await this.fetchRelations();
        this.newRel.type = ''; this.newRel.targetId = null;
        alert("关系添加成功！");
      } catch (e) { alert(e.response?.data?.message || "添加失败"); } finally { this.isSubmitting = false; }
    },
    async deleteRelation(relId) {
      if(!confirm("确定断开此关系吗？")) return;
      try {
        await apiClient.delete(`/Setting/relation/${relId}`);
        await this.fetchRelations();
      } catch (e) { alert("删除失败"); }
    }
  }
};
</script>

<style scoped>
.relations-panel { background: #fff; padding: 15px; border-radius: 4px; border: 1px solid #ccc; margin-top: 20px; }
.panel-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
.panel-title h4 { margin: 0; color: #333; font-size: 0.9rem; font-weight: bold; }
.relations-list { margin-bottom: 15px; }
.relation-item { display: flex; align-items: center; padding: 8px 0; border-bottom: 1px dashed #eee; font-size: 0.85rem; }
.rel-tag { background: #eee; color: #333; padding: 2px 8px; border-radius: 4px; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.rel-arrow { margin: 0 10px; color: #999; font-size: 0.8rem; }
.rel-target { font-weight: bold; color: #333; cursor: pointer; text-decoration: underline; }
.rel-target:hover { color: #D92323; }
.delete-btn { border: none; background: none; color: #999; font-size: 1.2rem; cursor: pointer; margin-left: auto; }
.delete-btn:hover { color: #D92323; }
.empty-text { color: #999; font-size: 0.8rem; font-style: italic; padding: 10px 0; text-align: center; }
.add-relation-box { display: flex; align-items: center; gap: 8px; background: #f9f9f9; padding: 10px; border-radius: 4px; }
.small-input { width: 100px; padding: 5px; border: 1px solid #ccc; font-family: inherit; font-size: 0.8rem; }
.small-select { flex: 1; padding: 5px; border: 1px solid #ccc; font-family: inherit; font-size: 0.8rem; }
.add-btn-small { background: #333; color: white; border: none; padding: 5px 12px; cursor: pointer; font-size: 0.8rem; font-weight: bold; }
.add-btn-small:hover { background: #D92323; }
.add-btn-small:disabled { background: #ccc; cursor: not-allowed; }
</style>