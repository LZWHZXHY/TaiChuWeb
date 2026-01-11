<template>
  <div class="chronicle-editor">
    
    <div class="sidebar">
      <div class="sidebar-header">
        <h3>⏳ 编年史时间轴</h3>
        <button class="add-btn" @click="createNewEvent">+ 新事件</button>
      </div>
      <div v-if="loading" class="loading-text">加载中...</div>
      <div v-else class="event-list">
        <div 
          v-for="ev in sortedEvents" 
          :key="ev.id" 
          class="event-item"
          :class="{ active: currentEvent && currentEvent.id === ev.id }"
          @click="selectEvent(ev)"
        >
          <div class="time-badge">{{ ev.time_label }}</div>
          <div class="event-title">{{ ev.title }}</div>
        </div>
        <div v-if="events.length === 0" class="empty-tip">暂无事件，点击上方新建</div>
      </div>
    </div>

    <div class="editor-main">
      <div v-if="currentEvent" class="edit-form">
        
        <div class="form-row">
          <div class="form-group flex-2">
            <label>事件标题</label>
            <input v-model="currentEvent.title" type="text" placeholder="例如：第一次深渊远征" />
          </div>
          
          <div class="form-group flex-1">
            <label>记录员 / 作者</label>
            <input v-model="currentEvent.author" type="text" placeholder="谁记录的？" />
          </div>

          <div class="form-group flex-1">
            <label>时间标签</label>
            <input v-model="currentEvent.time_label" type="text" placeholder="例如：初元 102 年" />
          </div>
          
          <div class="form-group flex-1">
            <label>排序权重</label>
            <input v-model.number="currentEvent.sort_order" type="number" placeholder="数字越小越靠前" />
          </div>
        </div>

        <div class="form-group">
          <label>🔗 关联的设定 (人物/地点/物品)</label>
          <div class="node-picker-box">
            <div class="tag-cloud">
              <div v-for="(node, index) in currentEvent.selectedNodes" :key="node.id" class="node-tag-complex">
                <div class="tag-top">
                  <span class="tag-type">[{{ node.type }}]</span>
                  <span class="tag-name">{{ node.name }}</span>
                  <i class="remove-icon" @click="removeNode(node.id)">×</i>
                </div>
                <input 
                  v-model="node.role" 
                  class="role-input" 
                  placeholder="输入角色(如:发起者)" 
                  @click.stop
                />
              </div>

              <button class="add-tag-btn" @click="showNodeSelector = true">+ 关联新设定</button>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label>简述 (Summary)</label>
          <textarea v-model="currentEvent.summary" class="summary-input" rows="2" placeholder="用于列表卡片展示的简短描述..."></textarea>
        </div>

        <div class="form-group">
          <label>详细内容 (Markdown)</label>
          <textarea v-model="currentEvent.content" class="content-editor" rows="10" placeholder="详细的历史记录、起因经过结果..."></textarea>
        </div>

        <div class="form-group">
          <label>🖼️ 事件配图</label>
          <div class="image-area">
            <div v-for="(url, idx) in parsedImages" :key="idx" class="img-preview">
              <img :src="url" />
              <div class="img-del" @click="removeImage(idx)">删除</div>
            </div>
            <div class="upload-btn-wrapper">
              <button class="upload-btn" :disabled="isUploading">
                {{ isUploading ? '上传中...' : '+ 上传图片' }}
              </button>
              <input type="file" @change="handleImageUpload" accept="image/*" />
            </div>
          </div>
        </div>

        <div class="action-bar">
          <button class="btn-delete" v-if="currentEvent.id !== 0" @click="handleDelete">🗑️ 删除事件</button>
          <div class="right-actions">
            <button class="btn-save" @click="saveEvent" :disabled="isSaving">
              {{ isSaving ? '保存中...' : '💾 保存当前事件' }}
            </button>
          </div>
        </div>

      </div>

      <div v-else class="empty-state">
        <p>👈 请在左侧选择一个事件，或点击“新事件”</p>
      </div>
    </div>

    <div v-if="showNodeSelector" class="modal-overlay" @click.self="showNodeSelector = false">
      <div class="modal-card">
        <div class="modal-header">
          <h3>关联设定节点</h3>
          <input v-model="searchKeyword" placeholder="🔍 搜索角色、地点..." class="search-input" />
        </div>
        <div class="node-select-list">
          <div 
            v-for="node in filteredNodes" 
            :key="node.id" 
            class="select-item"
            :class="{ selected: isNodeSelected(node.id) }"
            @click="toggleNodeSelect(node)"
          >
            <span class="item-check">{{ isNodeSelected(node.id) ? '✅' : '⬜' }}</span>
            <span class="item-type">[{{ node.type }}]</span>
            <span class="item-name">{{ node.name }}</span>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-primary" @click="showNodeSelector = false">完成选择</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import apiClient from '@/utils/api';

export default {
  name: 'ChronicleEditor',
  props: {
    ipId: { type: [Number, String], required: true }
  },
  data() {
    return {
      loading: false,
      isSaving: false,
      isUploading: false,
      events: [],
      allNodes: [],
      currentEvent: null,
      showNodeSelector: false,
      searchKeyword: ''
    };
  },
  computed: {
    sortedEvents() {
      return [...this.events].sort((a, b) => a.sort_order - b.sort_order);
    },
    filteredNodes() {
      if (!this.searchKeyword) return this.allNodes;
      const k = this.searchKeyword.toLowerCase();
      return this.allNodes.filter(n => n.name.toLowerCase().includes(k) || n.type.includes(k));
    },
    parsedImages() {
      if (!this.currentEvent || !this.currentEvent.image_urls) return [];
      try {
        const json = JSON.parse(this.currentEvent.image_urls);
        return Array.isArray(json) ? json : [];
      } catch {
        return [];
      }
    }
  },
  async mounted() {
    await Promise.all([this.fetchEvents(), this.fetchNodes()]);
  },
  methods: {
    async fetchEvents() {
  this.loading = true;
  try {
    const res = await apiClient.get(`/Events?ipId=${this.ipId}`);
    
    this.events = res.data.map(ev => ({
      ...ev,
      // 🔴 关键修复：后端返回的是 ev.Author (大写)，前端用 ev.author (小写)
      // 我们做一个兼容写法，优先读大写
      author: ev.Author || ev.author, 
      
      // 同样，确保 EventNodes 也是读取大写
      selectedNodes: (ev.EventNodes || ev.eventNodes) ? (ev.EventNodes || ev.eventNodes).map(n => ({
        id: n.id || n.Id, // 兼容大小写
        name: n.name || n.Name,
        type: n.type || n.Type,
        role: n.role || n.Role || n.role_name // 兼容后端可能返回的不同字段名
      })) : []
    }));
  } catch (e) {
    console.error(e);
  } finally {
    this.loading = false;
  }
},
    async fetchNodes() {
      try {
        const res = await apiClient.get(`/Setting/ip/${this.ipId}`);
        this.allNodes = res.data;
      } catch (e) { console.error("加载节点失败", e); }
    },

    createNewEvent() {
      this.currentEvent = {
        id: 0,
        ip_id: this.ipId,
        title: '',
        author: '', // ✅ 初始化作者字段
        time_label: '',
        sort_order: 0,
        summary: '',
        content: '',
        image_urls: '[]',
        selectedNodes: [] 
      };
    },
    selectEvent(ev) {
      this.currentEvent = JSON.parse(JSON.stringify(ev));
    },

    isNodeSelected(nodeId) {
      return this.currentEvent.selectedNodes.some(n => n.id === nodeId);
    },
    toggleNodeSelect(node) {
      const idx = this.currentEvent.selectedNodes.findIndex(n => n.id === node.id);
      if (idx > -1) {
        this.currentEvent.selectedNodes.splice(idx, 1);
      } else {
        this.currentEvent.selectedNodes.push({
          id: node.id,
          name: node.name,
          type: node.type,
          role: '参与者'
        });
      }
    },
    removeNode(nodeId) {
      this.currentEvent.selectedNodes = this.currentEvent.selectedNodes.filter(n => n.id !== nodeId);
    },

    async saveEvent() {
      if (!this.currentEvent.title) return alert("标题不能为空");
      this.isSaving = true;
      
      try {
        const relatedNodesPayload = this.currentEvent.selectedNodes.map(n => ({
          NodeId: n.id,
          Role: n.role
        }));

        const payload = {
            Id: this.currentEvent.id, // 确保 ID 也传过去
            ip_id: this.ipId,         // 你的后端 DTO 里这个是小写的，保持原样
            Title: this.currentEvent.title,
            Author: this.currentEvent.author, // ✅ 发送 Author (大写)
            Time_label: this.currentEvent.time_label, // 检查后端 DTO 是 Time_label 还是 time_label
            Sort_order: this.currentEvent.sort_order,
            Summary: this.currentEvent.summary,
            Content: this.currentEvent.content,
            Image_urls: this.currentEvent.image_urls,
            RelatedNodes: relatedNodesPayload
            };

        delete payload.selectedNodes; 
        delete payload.EventNodes;

        if (this.currentEvent.id === 0) {
          await apiClient.post('/Events', payload);
        } else {
          await apiClient.put(`/Events/${this.currentEvent.id}`, payload);
        }

        await this.fetchEvents(); 
        alert("保存成功");
        
        if (this.currentEvent.id !== 0) {
           const fresh = this.events.find(e => e.id === this.currentEvent.id);
           if(fresh) this.selectEvent(fresh);
        } else {
           this.currentEvent = null;
        }

      } catch (e) {
        alert("保存失败: " + e.message);
      } finally {
        this.isSaving = false;
      }
    },

    async handleDelete() {
      if (!confirm(`确定要删除事件“${this.currentEvent.title}”吗？`)) return;
      try {
        await apiClient.delete(`/Events/${this.currentEvent.id}`);
        this.currentEvent = null;
        await this.fetchEvents();
      } catch (e) {
        alert("删除失败");
      }
    },

    async handleImageUpload(e) {
      const file = e.target.files[0];
      if (!file) return;
      this.isUploading = true;
      const formData = new FormData();
      formData.append('file', file);
      try {
        const res = await apiClient.post(`/Upload/image`, formData);
        const imgs = this.parsedImages;
        imgs.push(res.data.url);
        this.currentEvent.image_urls = JSON.stringify(imgs);
      } catch (e) {
        alert("图片上传失败");
      } finally {
        this.isUploading = false;
      }
    },
    removeImage(index) {
      const imgs = this.parsedImages;
      imgs.splice(index, 1);
      this.currentEvent.image_urls = JSON.stringify(imgs);
    }
  }
};
</script>

<style scoped>
.chronicle-editor { display: flex; height: 100%; background: #f4f6f9; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; }

/* 侧边栏 */
.sidebar { width: 280px; background: white; border-right: 1px solid #ddd; display: flex; flex-direction: column; }
.sidebar-header { padding: 15px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
.sidebar-header h3 { margin: 0; font-size: 1rem; color: #333; }
.add-btn { background: #e6f7ff; color: #1890ff; border: 1px solid #91d5ff; border-radius: 4px; padding: 4px 8px; cursor: pointer; font-size: 0.8rem; }
.event-list { flex: 1; overflow-y: auto; padding: 10px 0; }
.event-item { padding: 12px 15px; border-bottom: 1px solid #f9f9f9; cursor: pointer; transition: 0.2s; }
.event-item:hover { background: #f0f7ff; }
.event-item.active { background: #e6f7ff; border-left: 3px solid #1890ff; }
.time-badge { font-size: 0.75rem; color: #999; margin-bottom: 4px; font-weight: bold; }
.event-title { font-size: 0.95rem; color: #333; }
.empty-tip { text-align: center; color: #999; padding: 20px; font-size: 0.9rem; }

/* 编辑区 */
.editor-main { flex: 1; display: flex; flex-direction: column; overflow: hidden; background: #f9fafc; }
.edit-form { flex: 1; overflow-y: auto; padding: 30px; display: flex; flex-direction: column; gap: 20px; max-width: 900px; margin: 0 auto; width: 100%; box-sizing: border-box; }
.form-row { display: flex; gap: 20px; }
.form-group { display: flex; flex-direction: column; gap: 8px; }
.flex-1 { flex: 1; }
.flex-2 { flex: 2; }
label { font-weight: 600; font-size: 0.9rem; color: #555; }
input, textarea { padding: 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 0.95rem; transition: 0.3s; }
input:focus, textarea:focus { border-color: #1890ff; outline: none; box-shadow: 0 0 0 2px rgba(24,144,255,0.1); }

/* 标签样式 */
.node-picker-box { background: white; border: 1px solid #ddd; padding: 15px; border-radius: 6px; min-height: 60px; }
.tag-cloud { display: flex; flex-wrap: wrap; gap: 10px; }

.node-tag-complex {
  display: flex; flex-direction: column;
  background: #fff; border: 1px solid #d9d9d9; 
  border-radius: 4px; padding: 6px 8px; 
  min-width: 120px; box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}
.tag-top { display: flex; align-items: center; justify-content: space-between; margin-bottom: 4px; font-size: 0.85rem; color: #333; font-weight: bold; }
.tag-type { font-size: 0.7rem; color: #999; font-weight: normal; margin-right: 4px; }
.remove-icon { cursor: pointer; color: #999; font-style: normal; margin-left: auto; }
.remove-icon:hover { color: #ff4d4f; }

.role-input {
  border: none; border-bottom: 1px dashed #ccc; 
  border-radius: 0; padding: 2px 0; 
  font-size: 0.8rem; width: 100%; color: #666; background: transparent;
}
.role-input:focus { border-bottom-color: #1890ff; box-shadow: none; }

.add-tag-btn { background: white; border: 1px dashed #aaa; color: #666; padding: 4px 12px; border-radius: 20px; cursor: pointer; font-size: 0.85rem; align-self: center; }
.add-tag-btn:hover { border-color: #1890ff; color: #1890ff; }

/* 其他样式 */
.image-area { display: flex; gap: 10px; flex-wrap: wrap; }
.img-preview { width: 100px; height: 100px; border-radius: 6px; overflow: hidden; position: relative; border: 1px solid #ddd; }
.img-preview img { width: 100%; height: 100%; object-fit: cover; }
.img-del { position: absolute; bottom: 0; width: 100%; background: rgba(0,0,0,0.6); color: white; text-align: center; font-size: 0.7rem; padding: 2px 0; cursor: pointer; }
.upload-btn-wrapper { position: relative; width: 100px; height: 100px; border: 1px dashed #ddd; border-radius: 6px; display: flex; align-items: center; justify-content: center; background: white; }
.upload-btn-wrapper input { position: absolute; top: 0; left: 0; width: 100%; height: 100%; opacity: 0; cursor: pointer; }
.upload-btn { border: none; background: transparent; color: #999; font-size: 0.8rem; }

.action-bar { margin-top: 20px; padding-top: 20px; border-top: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; }
.right-actions { display: flex; align-items: center; gap: 15px; }
.btn-save { background: #1890ff; color: white; padding: 10px 25px; border-radius: 6px; border: none; font-weight: bold; cursor: pointer; box-shadow: 0 4px 10px rgba(24,144,255,0.2); }
.btn-save:hover { background: #40a9ff; }
.btn-save:disabled { background: #ccc; cursor: not-allowed; }
.btn-delete { background: white; color: #ff4d4f; border: 1px solid #ff4d4f; padding: 8px 16px; border-radius: 6px; cursor: pointer; }
.btn-delete:hover { background: #fff1f0; }

.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1000; display: flex; justify-content: center; align-items: center; }
.modal-card { background: white; width: 500px; max-height: 80vh; border-radius: 8px; display: flex; flex-direction: column; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }
.modal-header { padding: 15px 20px; border-bottom: 1px solid #eee; }
.search-input { width: 100%; margin-top: 10px; box-sizing: border-box; }
.node-select-list { flex: 1; overflow-y: auto; padding: 10px 0; }
.select-item { padding: 10px 20px; display: flex; align-items: center; cursor: pointer; border-bottom: 1px solid #f9f9f9; }
.select-item:hover { background: #f5f7fa; }
.select-item.selected { background: #e6f7ff; }
.item-check { margin-right: 10px; width: 20px; }
.item-type { color: #999; font-size: 0.8rem; margin-right: 8px; min-width: 40px; }
.modal-footer { padding: 15px; border-top: 1px solid #eee; text-align: right; }
.btn-primary { background: #1890ff; color: white; border: none; padding: 8px 20px; border-radius: 4px; cursor: pointer; }
</style>