<template>
  <div class="editor-container">
    <div class="editor-header">
      <div class="left">
        <button class="back-btn" @click="$parent.currentMode = 'list'">⬅️ 返回</button>
        <span class="world-title">当前世界 ID: {{ ipId }}</span>
        
        <div class="mode-tabs">
          <button 
            :class="['tab-btn', { active: currentMode === 'node' }]" 
            @click="currentMode = 'node'"
          >📖 设定管理</button>
          <button 
            :class="['tab-btn', { active: currentMode === 'chronicle' }]" 
            @click="currentMode = 'chronicle'"
          >⏳ 编年史</button>
        </div>
      </div>

      <div class="right">
        <button v-if="currentMode === 'node'" class="add-btn" @click="openCreateModal">
          <span v-if="currentNode">+ 在【{{ currentNode.name }}】下新建</span>
          <span v-else>+ 新建根目录</span>
        </button>
      </div>
    </div>

    <div class="editor-body">
      
      <div v-if="currentMode === 'node'" class="node-editor-layout">
        <div class="sidebar">
          <div v-if="loading" class="loading">加载目录中...</div>
          <div v-else class="tree-root">
            <div 
              v-for="node in flatTree" 
              :key="node.id" 
              class="tree-item" 
              :style="{ paddingLeft: (node.level * 20 + 10) + 'px' }"
              :class="{ active: currentNode && currentNode.id === node.id }"
              @click="selectNode(node)"
            >
              <span class="arrow" v-if="node.children && node.children.length > 0">▼</span>
              <span class="arrow placeholder" v-else></span>
              <span class="icon">{{ getIcon(node.type) }}</span>
              <span class="node-name">{{ node.name }}</span>
            </div>
            <div v-if="nodes.length === 0" class="empty-tree">暂无设定，点击右上角新建</div>
          </div>
        </div>

        <div class="content-area">
          <div v-if="currentNode" class="node-detail">
            
            <NodeRelationPanel 
              :currentNode="currentNode" 
              :allNodes="nodes" 
              @select-node="handleSelectRelationTarget" 
            />
            
            <div class="detail-header">
              <div class="header-left">
                <h2>{{ currentNode.name }}</h2>
                <span class="tag">{{ currentNode.type }}</span>
                
                <div class="author-input-wrap">
                  <span class="author-label">✍️ 作者:</span>
                  <input 
                    v-model="editingAuthor" 
                    class="author-field" 
                    placeholder="匿名"
                  />
                </div>
              </div>
              
              <div class="header-actions">
                <button class="move-btn" @click="openMoveModal">➡️ 移动</button>
                <button class="delete-node-btn" @click="handleDeleteNode">🗑️ 删除</button>
                <button class="save-btn" @click="saveContent" :disabled="isSaving">
                  {{ isSaving ? '💾 保存中...' : '💾 保存更改' }}
                </button>
              </div>
            </div>
            
            <hr class="divider">

            <div class="properties-panel">
              <div class="panel-title">
                <h4>⚙️ 详细设定 (参数)</h4>
                <button class="small-btn" @click="addProperty">+ 添加属性</button>
              </div>
              <div class="props-list">
                <PropertyItem 
                  v-for="(item, index) in editingProps" 
                  :key="index"
                  v-model="editingProps[index]"
                  @delete="removeRootProperty(index)"
                />
                <div v-if="editingProps.length === 0" class="empty-props">暂无详细设定，点击右上角添加</div>
              </div>
            </div>

            <hr class="divider">

            <div class="images-panel">
              <div class="panel-title">
                <h4>🖼️ 设定图集</h4>
                <div class="upload-box">
                  <button class="small-btn" @click="triggerUpload" :disabled="isUploading">
                    {{ isUploading ? '正在压缩上传...' : '+ 上传新图' }}
                  </button>
                  <input 
                    type="file" 
                    ref="fileInput" 
                    style="display: none" 
                    accept="image/*" 
                    @change="handleImageUpload" 
                  />
                </div>
              </div>
              <div class="editor-gallery" v-if="parsedImages.length > 0">
                <div v-for="(url, index) in parsedImages" :key="index" class="editor-img-wrap">
                  <img :src="url" alt="设定图" />
                </div>
              </div>
              <div v-else class="empty-props">暂无设定图，点击右上角上传</div>
            </div>

            <hr class="divider">

            <div class="description-panel">
              <div class="panel-header"><h4>📝 详细描述 / 背景故事</h4></div>
              <textarea 
                v-model="editingDescription" 
                class="simple-editor" 
                placeholder="在这里开始书写关于这个设定的详细故事、历史、外观描述..."
              ></textarea>
            </div>
          </div>

          <div v-else class="empty-state">
            <h3>👈 请在左侧选择一个设定</h3>
            <p>或者点击右上角新建一个设定</p>
          </div>
        </div>
      </div>

      <ChronicleEditor 
        v-if="currentMode === 'chronicle'" 
        :ipId="ipId"
        class="chronicle-editor-instance"
      />

    </div>

    <div v-if="showCreateModal" class="modal-overlay">
      <div class="modal-card">
        <h3>{{ createForm.parentId ? '新建子设定' : '新建根设定' }}</h3>
        <form @submit.prevent="handleCreateNode">
          <div class="form-group">
            <label>名称</label>
            <input v-model="createForm.name" type="text" required autoFocus />
          </div>
          <div class="form-group">
            <label>类型</label>
            <input v-model="createForm.type" type="text" list="type-suggestions" required />
            <datalist id="type-suggestions">
              <option v-for="t in typeOptions" :key="t" :value="t"></option>
            </datalist>
          </div>
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="showCreateModal = false">取消</button>
            <button type="submit" class="confirm-btn" :disabled="isSubmitting">确定创建</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showMoveModal" class="modal-overlay">
      <div class="modal-card">
        <h3>移动节点：{{ currentNode.name }}</h3>
        <p class="modal-tip">请选择新的父节点：</p>
        <div class="form-group">
          <select v-model="moveTargetId" class="full-width-select">
            <option :value="null">-- 设为根节点 (无父级) --</option>
            <option v-for="node in availableParents" :key="node.id" :value="node.id">
              {{ '　'.repeat(node.level) }} {{ node.name }} ({{ node.type }})
            </option>
          </select>
        </div>
        <div class="modal-actions">
          <button class="cancel-btn" @click="showMoveModal = false">取消</button>
          <button class="confirm-btn" @click="handleMoveNode">确定移动</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import apiClient from '@/utils/api';
import NodeRelationPanel from '../../ArtCenter/Components/NodeRelationPanel.vue';
import PropertyItem from './PropertyItem.vue';
// ✅ 引入组件
import ChronicleEditor from './ChronicleEditor.vue';

export default {
  name: 'WorldEditor',
  props: {
    ipId: { type: Number, required: true }
  },
  components: {
    PropertyItem,
    NodeRelationPanel,
    // ✅ 注册组件
    ChronicleEditor
  },
  data() {
    return {
      // ✅ 新增：控制当前模式 'node' | 'chronicle'
      currentMode: 'node',

      nodes: [],        
      loading: false,   
      currentNode: null,
      isUploading: false,
      isSaving: false,
      
      // 编辑缓存
      editingDescription: '',
      editingAuthor: '',
      editingProps: [], 

      // 弹窗状态
      showCreateModal: false,
      isSubmitting: false,
      typeOptions: [],
      createForm: { name: '', type: '', parentId: null },
      showMoveModal: false,
      moveTargetId: null,
    };
  },
  computed: {
    // 扁平化树结构用于左侧目录
    flatTree() {
      if (!this.nodes || this.nodes.length === 0) return [];
      const tree = this.buildTree(this.nodes);
      return this.flatten(tree);
    },
    // 解析当前节点的图片 JSON
    parsedImages() {
      if (!this.currentNode || !this.currentNode.image_url) return [];
      try {
        const res = JSON.parse(this.currentNode.image_url);
        return Array.isArray(res) ? res : [this.currentNode.image_url];
      } catch (e) {
        return [this.currentNode.image_url];
      }
    },
    // 计算移动时可选的父节点（排除自身）
    availableParents() {
      if (!this.currentNode) return [];
      return this.flatTree.filter(n => n.id !== this.currentNode.id);
    }
  },
  mounted() {
    this.fetchNodes();
  },
  methods: {
    // ======================================================
    // 🔥 核心逻辑：递归转换函数 (JSON <-> Tree)
    // ======================================================
    jsonToTree(jsonObj) {
      if (!jsonObj || typeof jsonObj !== 'object') return [];
      return Object.keys(jsonObj).map(key => {
        const val = jsonObj[key];
        if (val && typeof val === 'object' && !Array.isArray(val)) {
          return { key: key, children: this.jsonToTree(val) };
        } else {
          return { key: key, value: val };
        }
      });
    },

    treeToJson(treeArr) {
      const result = {};
      treeArr.forEach(item => {
        if (!item.key || item.key.trim() === '') return;
        if (item.children && Array.isArray(item.children)) {
          result[item.key] = this.treeToJson(item.children);
        } else {
          result[item.key] = item.value;
        }
      });
      return result;
    },

    // ======================================================
    // 业务逻辑：读取、保存、删除
    // ======================================================
    selectNode(node) {
      this.currentNode = node;
      this.editingDescription = node.description || '';
      this.editingAuthor = node.author || '';
      this.editingProps = [];
      
      const jsonStr = node.meta_data_json || node.metaDataJson;
      if (jsonStr) { 
        try {
          const meta = (typeof jsonStr === 'string') ? JSON.parse(jsonStr) : jsonStr;
          this.editingProps = this.jsonToTree(meta);
        } catch (e) {
          console.error("元数据解析失败", e);
        }
      }
    },

    async saveContent() {
      if (!this.currentNode) return;
      this.isSaving = true;

      try {
        const metaObj = this.treeToJson(this.editingProps);
        const payload = {
          name: this.currentNode.name,
          author: this.editingAuthor,
          description: this.editingDescription,
          metaData: metaObj
        };

        await apiClient.put(`/Setting/${this.currentNode.id}`, payload);
        
        // 同步状态到本地
        this.currentNode.author = this.editingAuthor;
        this.currentNode.description = this.editingDescription;
        this.currentNode.meta_data_json = JSON.stringify(metaObj);
        
        alert("保存成功！");
      } catch (error) {
        console.error("保存失败", error);
        alert("保存失败，请检查后端 API");
      } finally {
        this.isSaving = false;
      }
    },

    async handleDeleteNode() {
      if (!confirm(`警告：确定要删除【${this.currentNode.name}】吗？`)) return;
      try {
        await apiClient.delete(`/Setting/${this.currentNode.id}`);
        await this.fetchNodes();
        this.currentNode = null;
        alert("已删除");
      } catch (error) {
        alert("删除失败");
      }
    },

    // ======================================================
    // 辅助逻辑：节点操作、图片压缩
    // ======================================================
    addProperty() {
      this.editingProps.push({ key: '', value: '' });
    },
    removeRootProperty(index) {
      this.editingProps.splice(index, 1);
    },
    async fetchNodes() {
      this.loading = true;
      try {
        const res = await apiClient.get(`/Setting/ip/${this.ipId}`);
        this.nodes = res.data;
      } catch (error) {
        alert("获取列表失败");
      } finally {
        this.loading = false;
      }
    },
    triggerUpload() { this.$refs.fileInput.click(); },
    
    // 图片压缩逻辑
    compressImage(file, quality = 0.7) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = (e) => {
          const img = new Image();
          img.src = e.target.result;
          img.onload = () => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            let { width, height } = img;
            const maxSize = 1920;
            if (width > maxSize || height > maxSize) {
              const ratio = Math.min(maxSize / width, maxSize / height);
              width *= ratio; height *= ratio;
            }
            canvas.width = width; canvas.height = height;
            ctx.drawImage(img, 0, 0, width, height);
            canvas.toBlob((blob) => resolve(new File([blob], file.name, { type: 'image/jpeg' })), 'image/jpeg', quality);
          };
        };
      });
    },

    async handleImageUpload(event) {
      const file = event.target.files[0];
      if (!file) return;
      this.isUploading = true;
      try {
        let uploadFile = file;
        if (file.size > 1024 * 1024) uploadFile = await this.compressImage(file);
        const formData = new FormData();
        formData.append('file', uploadFile);
        const res = await apiClient.post(`/Setting/${this.currentNode.id}/image`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
        if (res.data.success) {
          this.currentNode.image_url = JSON.stringify(res.data.allImages);
        }
      } catch (error) {
        alert("图片上传失败");
      } finally {
        this.isUploading = false;
        event.target.value = '';
      }
    },

    // 目录树算法
    buildTree(items) {
      const data = JSON.parse(JSON.stringify(items));
      const result = []; const map = {};
      data.forEach(it => { it.children = []; map[it.id] = it; });
      data.forEach(it => {
        if (it.parentId && map[it.parentId]) map[it.parentId].children.push(it);
        else result.push(it);
      });
      return result;
    },
    flatten(tree, level = 0) {
      let res = [];
      tree.forEach(n => {
        n.level = level; res.push(n);
        if (n.children?.length) res = res.concat(this.flatten(n.children, level + 1));
      });
      return res;
    },
    getIcon(type) {
      const icons = { '星系': '🌌', '星球': '🪐', '角色': '👤', '能力': '✨', '物品': '📦' };
      return icons[type] || '📄';
    },

    // 弹窗逻辑
    openCreateModal() {
      this.createForm = { name: '', type: '', parentId: this.currentNode?.id || null };
      apiClient.get('/Setting/types').then(res => this.typeOptions = res.data);
      this.showCreateModal = true;
    },
    async handleCreateNode() {
      this.isSubmitting = true;
      try {
        await apiClient.post('/Setting', { ...this.createForm, ipId: this.ipId });
        this.showCreateModal = false;
        await this.fetchNodes();
      } catch (e) { alert("创建失败"); } finally { this.isSubmitting = false; }
    },
    openMoveModal() { this.showMoveModal = true; },
    async handleMoveNode() {
      try {
        await apiClient.post('/Setting/move', { nodeId: this.currentNode.id, targetParentId: this.moveTargetId });
        this.showMoveModal = false;
        await this.fetchNodes();
      } catch (e) { alert("移动失败"); }
    },
    handleSelectRelationTarget(id) {
      const n = this.nodes.find(x => x.id === id);
      if (n) this.selectNode(n);
    }
  }
};
</script>

<style scoped>
/* 核心布局 */
.editor-container { display: flex; flex-direction: column; height: 90vh; background: #f5f7fa; border-radius: 8px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }
.editor-header { height: 60px; background: white; border-bottom: 1px solid #ddd; display: flex; justify-content: space-between; align-items: center; padding: 0 20px; }
.editor-body { flex: 1; display: flex; overflow: hidden; }

/* ✅ 新增：Mode Tabs 样式 */
.mode-tabs { display: flex; background: #f0f2f5; padding: 4px; border-radius: 6px; margin-left: 20px; gap: 4px; }
.tab-btn { border: none; background: transparent; padding: 6px 16px; border-radius: 4px; cursor: pointer; font-size: 0.9rem; color: #666; font-weight: 500; transition: 0.2s; }
.tab-btn:hover { background: rgba(255,255,255,0.5); }
.tab-btn.active { background: white; color: #1890ff; box-shadow: 0 2px 4px rgba(0,0,0,0.1); font-weight: bold; }

/* ✅ 新增：布局隔离样式 */
.node-editor-layout { flex: 1; display: flex; width: 100%; height: 100%; overflow: hidden; }
.chronicle-editor-instance { flex: 1; width: 100%; height: 100%; }

/* 侧边栏 */
.sidebar { width: 280px; background: #fff; border-right: 1px solid #ddd; overflow-y: auto; padding: 10px 0; }
.tree-item { padding: 10px; cursor: pointer; display: flex; align-items: center; font-size: 0.95rem; border-bottom: 1px solid #f9f9f9; }
.tree-item:hover { background: #f0f7ff; }
.tree-item.active { background: #e6f7ff; color: #1890ff; font-weight: bold; border-right: 3px solid #1890ff; }
.arrow { width: 16px; font-size: 0.7rem; color: #999; }
.icon { margin: 0 6px; }

/* 编辑内容区 */
.content-area { flex: 1; padding: 30px; background: #f5f7fa; overflow-y: auto; }
.detail-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.header-left { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
.tag { background: #eee; padding: 2px 8px; border-radius: 4px; font-size: 0.8rem; color: #666; }

/* 🔥 作者输入框样式 */
.author-input-wrap {
display: flex; align-items: center; margin-left: 15px; background: #fff;
padding: 4px 12px; border-radius: 20px; border: 1px solid #e0e0e0;
}
.author-label { font-size: 0.8rem; color: #999; margin-right: 6px; }
.author-field { border: none; background: transparent; font-size: 0.9rem; width: 90px; outline: none; }
.author-field:focus { color: #1890ff; }

/* 操作按钮 */
.header-actions { display: flex; gap: 10px; }
.move-btn, .delete-node-btn { background: #fff; border: 1px solid #ddd; padding: 8px 16px; border-radius: 6px; cursor: pointer; transition: 0.3s; }
.move-btn:hover { border-color: #1890ff; color: #1890ff; }
.delete-node-btn:hover { background: #ff4d4f; color: white; border-color: #ff4d4f; }
.save-btn { background: #52c41a; color: white; border: none; padding: 8px 20px; border-radius: 6px; font-weight: bold; cursor: pointer; }
.save-btn:hover { background: #45b915; }
.save-btn:disabled { background: #ccc; cursor: not-allowed; }

/* 通用面板 */
.divider { border: 0; border-top: 1px solid #e8e8e8; margin: 20px 0; }
.properties-panel, .images-panel, .description-panel { background: #fff; padding: 20px; border-radius: 8px; border: 1px solid #eee; margin-bottom: 20px; }
.panel-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.small-btn { font-size: 0.8rem; padding: 5px 12px; background: #fff; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; }
.small-btn:hover { color: #1890ff; border-color: #1890ff; }

/* 文本编辑器 */
.simple-editor { width: 100%; min-height: 400px; padding: 15px; border: 1px solid #ddd; border-radius: 6px; font-size: 1rem; line-height: 1.6; resize: vertical; box-sizing: border-box; }

/* 图集网格 */
.editor-gallery { display: grid; grid-template-columns: repeat(auto-fill, minmax(120px, 1fr)); gap: 15px; margin-top: 15px; }
.editor-img-wrap { width: 100%; aspect-ratio: 1; border-radius: 6px; overflow: hidden; border: 1px solid #ddd; background: #f9f9f9; }
.editor-img-wrap img { width: 100%; height: 100%; object-fit: cover; }

/* 弹窗样式 */
.modal-overlay { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 100; }
.modal-card { background: white; width: 400px; padding: 25px; border-radius: 8px; }
.form-group { margin-bottom: 15px; }
.form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
.form-group input, .full-width-select { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
.confirm-btn { background: #1890ff; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; }
.cancel-btn { background: #f5f5f5; border: 1px solid #ddd; padding: 8px 16px; border-radius: 4px; cursor: pointer; }
</style>