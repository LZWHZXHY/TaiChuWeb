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
      <input 
        v-model="newRel.type" 
        placeholder="关系 (如:好友)" 
        class="small-input"
        :disabled="isSubmitting"
      />
      <span class="rel-arrow">➡️</span>
      
      <select v-model="newRel.targetId" class="small-select" :disabled="isSubmitting">
        <option :value="null" disabled>选择目标...</option>
        <option 
          v-for="node in otherNodes" 
          :key="node.id" 
          :value="node.id"
        >
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
    // 必需：当前正在编辑的节点对象 (包含 id)
    currentNode: {
      type: Object,
      required: true
    },
    // 必需：所有节点列表 (用于目标下拉菜单)
    allNodes: {
      type: Array,
      required: true
    }
  },
  data() {
    return {
      currentRelations: [], // 存储当前节点的所有关系
      isSubmitting: false,
      newRel: {
        type: '',
        targetId: null
      }
    };
  },
  computed: {
    // 过滤掉自己，用于下拉菜单选择
    otherNodes() {
      // 这里的 node.id 是数字，但 allNodes 里的 id 可能是数字或字符串，需要确保类型匹配
      return this.allNodes.filter(n => n.id !== this.currentNode.id);
    },
    // 简单的校验
    canAddRelation() {
      return this.newRel.type && this.newRel.targetId;
    }
  },
  watch: {
    // 监听当前节点变化，自动刷新关系列表
    'currentNode.id': {
      immediate: true,
      handler(newId) {
        if (newId) {
          this.fetchRelations();
        } else {
          this.currentRelations = [];
        }
      }
    }
  },
  methods: {
    // 获取当前节点的所有关系 (对应后端 GET api/Setting/{id}/relations)
    async fetchRelations() {
      try {
        const res = await apiClient.get(`/Setting/${this.currentNode.id}/relations`);
        this.currentRelations = res.data.map(rel => ({
          ...rel,
          // 确保 targetId 是数字类型，以便与下拉菜单的 :value 匹配
          targetId: Number(rel.targetId) 
        }));
      } catch (e) {
        console.error("获取关系失败", e);
      }
    },

    // 添加新关系 (对应后端 POST api/Setting/relation)
    async addRelation() {
      this.isSubmitting = true;
      try {
        const targetIdNum = Number(this.newRel.targetId); // 确保是数字
        
        await apiClient.post('/Setting/relation', {
          sourceNodeId: this.currentNode.id,
          targetNodeId: targetIdNum,
          relationType: this.newRel.type,
          description: ''
        });

        // 刷新列表 & 重置表单
        await this.fetchRelations();
        this.newRel.type = '';
        this.newRel.targetId = null;
        alert("关系添加成功！图谱数据已更新。");
      } catch (e) {
        alert(e.response?.data || "添加失败，请检查是否重复或网络问题。");
      } finally {
        this.isSubmitting = false;
      }
    },

    // 删除关系 (对应后端 DELETE api/Setting/relation/{id})
    async deleteRelation(relId) {
      if(!confirm("确定断开此关系吗？")) return;
      try {
        await apiClient.delete(`/Setting/relation/${relId}`);
        await this.fetchRelations(); // 刷新列表
        alert("关系已删除。");
      } catch (e) {
        alert("删除失败");
      }
    }
  }
};
</script>

<style scoped>
.relations-panel {
  background: #fff;
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #eee;
  margin-top: 20px;
}

.panel-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.panel-title h4 { 
  margin: 0; 
  color: #555; 
  font-size: 1rem; 
}

.relations-list {
  margin-bottom: 15px;
}

.relation-item {
  display: flex;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px dashed #eee;
}

.rel-tag {
  background: #e6f7ff;
  color: #1890ff;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: bold;
}

.rel-arrow {
  margin: 0 10px;
  color: #999;
  font-size: 0.8rem;
}

.rel-target {
  font-weight: bold;
  color: #333;
  cursor: pointer;
  text-decoration: underline;
}

.rel-target:hover {
  color: #40a9ff;
}

.delete-btn {
  border: none;
  background: none;
  color: #ff4d4f;
  font-size: 1.2rem;
  cursor: pointer;
  margin-left: auto; /* 靠右对齐 */
}

.empty-text {
  color: #999;
  font-size: 0.9rem;
  font-style: italic;
  padding: 10px 0;
  text-align: center;
}

.add-relation-box {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #f9f9f9;
  padding: 10px;
  border-radius: 4px;
}

.small-input {
  width: 100px;
  padding: 5px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.small-select {
  flex: 1;
  padding: 5px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.add-btn-small {
  background: #1890ff;
  color: white;
  border: none;
  padding: 5px 12px;
  border-radius: 4px;
  cursor: pointer;
}
.add-btn-small:disabled {
  background: #ccc;
  cursor: not-allowed;
}
</style>