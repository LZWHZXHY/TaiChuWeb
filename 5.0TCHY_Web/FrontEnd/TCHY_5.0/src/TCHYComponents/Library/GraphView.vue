<template>
  <div class="graph-view-container">
    <div class="toolbar">
      <div class="left-tools">
        <span class="tip">操作提示：滚轮缩放 / 拖拽节点 / 点击查看详情</span>
      </div>
      <div class="right-tools">
        <button class="refresh-btn" @click="refreshGraph">🔄 重新布局</button>
      </div>
    </div>
    
    <div id="mountNode" ref="graphContainer"></div>

    <transition name="slide-fade">
      <div v-if="currentNode" class="info-drawer">
        <button class="close-btn" @click="closeDrawer">×</button>
        
        <div class="drawer-header">
          <div class="header-top-tags">
            <div class="node-type-tag">{{ currentNode.category || currentNode.type }}</div>
            <div class="node-author-tag" v-if="currentNode.author">
              <span class="pen-icon">✍️</span> {{ currentNode.author }}
            </div>
          </div>
          
          <h2>{{ currentNode.label || currentNode.name }}</h2>
          <div class="node-id">ID: {{ currentNode.id.replace('n-', '') }}</div>
        </div>

        <div class="drawer-content">
          <div class="info-item">
            <label>📝 详细描述</label>
            <div class="text-content">
              {{ currentNode.description || '暂无详细描述...' }}
            </div>
          </div>
          
          <div class="info-item" v-if="parsedProperties">
            <label>⚙️ 属性规格</label>
            <div class="attr-group">
              <div v-for="(val, key) in parsedProperties" :key="key" class="attr-row">
                <span class="attr-key">{{ key }}:</span>
                
                <div v-if="isObject(val)" class="attr-nested">
                  <div v-for="(subVal, subKey) in val" :key="subKey" class="sub-attr">
                    <span class="sub-key">{{ subKey }}:</span>
                    <span class="sub-val">{{ subVal }}</span>
                  </div>
                </div>
                
                <span v-else class="attr-val">{{ val }}</span>
              </div>
            </div>
          </div>

          <div class="info-item" v-if="currentNodeImages.length > 0">
            <label>🖼️ 设定图集 ({{ currentNodeImages.length }})</label>
            <div class="viewer-gallery">
              <div 
                v-for="(imgUrl, index) in currentNodeImages" 
                :key="index" 
                class="viewer-img-wrap"
                @click="openImagePreview(imgUrl)"
                title="点击查看大图"
              >
                <img :src="imgUrl" alt="设定图" loading="lazy">
              </div>
            </div>
          </div>

          <div class="info-item">
            <label>🔗 图谱连接</label>
            <p>该节点在图谱中关联了 <strong>{{ getConnectedEdgesCount(currentNode.id) }}</strong> 个对象。</p>
          </div>

          <div class="info-item">
            <label>🕒 更新时间</label>
            <p>{{ currentNode.updated_at || '未知' }}</p>
          </div>
        </div>
      </div>
    </transition>

    <transition name="fade">
      <div v-if="previewImageUrl" class="lightbox-overlay" @click="closeImagePreview">
        <div class="lightbox-content">
          <img :src="previewImageUrl" alt="全屏预览" @click.stop />
          <button class="lightbox-close-btn" @click="closeImagePreview">×</button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
import { Graph } from '@antv/g6';
import apiClient from '@/utils/api';
import { markRaw } from 'vue';

export default {
  name: 'GraphView',
  props: {
    ipId: { type: Number, required: true }
  },
  data() {
    return {
      currentNode: null,
      previewImageUrl: null,
    };
  },
  computed: {
    currentNodeImages() {
      if (!this.currentNode) return [];
      const raw = this.currentNode.image_url || this.currentNode.image;
      if (!raw) return [];
      try {
        const parsed = (typeof raw === 'string') ? JSON.parse(raw) : raw;
        return Array.isArray(parsed) ? parsed : [raw];
      } catch (e) {
        return [raw];
      }
    },
    parsedProperties() {
      if (!this.currentNode) return null;
      const rawProps = this.currentNode.properties || this.currentNode.meta_data_json;
      if (!rawProps) return null;
      try {
        const parsed = (typeof rawProps === 'string') ? JSON.parse(rawProps) : rawProps;
        return Object.keys(parsed).length > 0 ? parsed : null;
      } catch (e) {
        return null;
      }
    }
  },
  created() {
    this.graphInstance = null; 
  },
  mounted() {
    this.initGraph();
    this.fetchGraphData();
  },
  beforeUnmount() {
    this.destroyGraph();
  },
  methods: {
    isObject(val) {
      return val !== null && typeof val === 'object' && !Array.isArray(val);
    },

    initGraph() {
      const container = this.$refs.graphContainer;
      if (!container) return;
      
      container.innerHTML = '';
      const width = container.offsetWidth;
      const height = container.offsetHeight;

      const graph = new Graph({
        container,
        width,
        height,
        animation: false, 
        behaviors: ['drag-canvas', 'zoom-canvas', 'drag-element'],
        layout: {
          type: 'force',
          preventOverlap: true,
          nodeSize: 120,
          linkDistance: 250,
          nodeStrength: 3000, 
          edgeStrength: 0.5,
          gravity: 0.5,       
          animated: false,
        },
        node: {
          style: {
            labelText: (d) => d.label,
            labelPlacement: 'bottom',
            labelFill: '#fff',
            labelFontSize: 14,
            labelStroke: '#000',
            labelLineWidth: 2,
            fill: (d) => this.getNodeColor(d.category).fill,
            stroke: (d) => this.getNodeColor(d.category).stroke,
            shadowColor: (d) => this.getNodeColor(d.category).stroke,
            shadowBlur: 20, 
            size: (d) => this.getNodeSize(d.category),
            cursor: 'pointer'
          }
        },
        edge: {
          style: {
            stroke: 'rgba(255, 255, 255, 0.4)',
            lineWidth: 2,
            endArrow: true,
            labelText: (d) => d.label || '',
            labelFill: '#ccc',
            labelFontSize: 11,
            labelBackground: true,
            labelBackgroundFill: 'rgba(0,0,0,0.6)'
          }
        }
      });

      this.graphInstance = markRaw(graph);
      this.bindEvents();
      window.addEventListener('resize', this.handleResize);
    },

    bindEvents() {
      if (!this.graphInstance) return;
      this.graphInstance.on('node:click', (e) => {
        const id = e.target.id;
        const data = this.graphInstance.getNodeData(id);
        this.currentNode = data;
      });
      this.graphInstance.on('canvas:click', () => { 
        this.closeDrawer(); 
      });
    },

    async fetchGraphData() {
      try {
        const res = await apiClient.get(`/Graph/ip/${this.ipId}`);
        const rawData = res.data;
        const validNodeSet = new Set();
        
        const nodes = rawData.nodes.map(node => {
          const newId = `n-${node.id}`; 
          validNodeSet.add(newId);
          // 确保 spread 了所有原始属性，包括 author
          return { ...node, id: newId, category: node.type, type: 'circle' };
        });

        const edges = rawData.edges.map((edge, index) => ({
             ...edge,
             id: edge.id ? `e-${edge.id}` : `edge-${index}`,
             source: `n-${edge.source}`,
             target: `n-${edge.target}`,
             type: 'line'
        })).filter(e => validNodeSet.has(e.source) && validNodeSet.has(e.target));

        if (this.graphInstance) {
            this.graphInstance.setData({ nodes, edges });
            await this.graphInstance.render();
        }
      } catch (error) {
        console.error("图谱加载失败", error);
      }
    },

    handleResize() {
       if (this.graphInstance && !this.graphInstance.destroyed && this.$refs.graphContainer) {
         this.graphInstance.resize(this.$refs.graphContainer.offsetWidth, this.$refs.graphContainer.offsetHeight);
       }
    },
    refreshGraph() {
       if (this.graphInstance) this.graphInstance.layout();
    },
    destroyGraph() {
        window.removeEventListener('resize', this.handleResize);
        if (this.graphInstance) {
            this.graphInstance.destroy();
            this.graphInstance = null;
        }
    },
    closeDrawer() { this.currentNode = null; },
    openImagePreview(url) { this.previewImageUrl = url; },
    closeImagePreview() { this.previewImageUrl = null; },
    
    getConnectedEdgesCount(nodeId) {
      if (!this.graphInstance) return 0;
      const edges = this.graphInstance.getEdgeData();
      return edges.filter(e => e.source === nodeId || e.target === nodeId).length;
    },
    
    getNodeColor(t) {
       if (t === '星系') return { fill: '#FFF1B8', stroke: '#FFD700' }; 
       if (t === '星球') return { fill: '#bae7ff', stroke: '#00BFFF' }; 
       if (['角色', '主要角色', '人物'].includes(t)) return { fill: '#ffd6e7', stroke: '#FF69B4' }; 
       return { fill: '#d9f7be', stroke: '#32CD32' }; 
    },
    getNodeSize(t) {
       if (t === '星系') return 90;
       if (t === '星球') return 70;
       return 50;
    }
  }
};
</script>

<style scoped>
.graph-view-container { 
  width: 100%; height: 100%; position: relative; overflow: hidden; background: #0b0c15; 
}

#mountNode {
  width: 100%; height: 100%;
  background: radial-gradient(ellipse at bottom, #1b2735 0%, #090a0f 100%);
}

/* 工具栏 */
.toolbar {
  position: absolute; top: 0; left: 0; right: 0; height: 50px;
  display: flex; align-items: center; justify-content: space-between;
  padding: 0 20px; z-index: 5; pointer-events: none; 
  background: linear-gradient(to bottom, rgba(0,0,0,0.6), transparent);
}
.toolbar button { pointer-events: auto; }
.tip { color: rgba(255,255,255,0.5); font-size: 0.8rem; }
.refresh-btn { 
  background: rgba(24, 144, 255, 0.8); color: white; border: none; 
  padding: 6px 12px; border-radius: 4px; cursor: pointer;
}

/* 抽屉面板 */
.info-drawer {
  position: absolute; top: 0; right: 0; width: 350px; height: 100%;
  background: rgba(16, 20, 35, 0.85); backdrop-filter: blur(12px);
  border-left: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: -5px 0 30px rgba(0, 0, 0, 0.5);
  padding: 25px; box-sizing: border-box; color: #fff; z-index: 10;
  display: flex; flex-direction: column;
}
.close-btn {
  position: absolute; top: 15px; right: 15px; background: none; border: none;
  color: rgba(255, 255, 255, 0.5); font-size: 1.5rem; cursor: pointer;
}

/* 🔥 Header 区域布局 */
.header-top-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 10px;
  align-items: center;
}

.drawer-header h2 {
  margin: 10px 0; font-size: 1.8rem;
  background: linear-gradient(120deg, #fff, #a5c2ff); -webkit-background-clip: text; color: transparent;
}

.node-type-tag {
  display: inline-block; padding: 4px 8px; background: rgba(24, 144, 255, 0.2); 
  color: #40a9ff; border-radius: 4px; font-size: 0.75rem; border: 1px solid rgba(24, 144, 255, 0.3);
}

/* 🔥 作者标签样式 */
.node-author-tag {
  display: inline-block; padding: 4px 8px; background: rgba(255, 255, 255, 0.05); 
  color: rgba(255, 255, 255, 0.6); border-radius: 4px; font-size: 0.75rem;
  font-style: italic; border: 1px solid rgba(255, 255, 255, 0.1);
}
.pen-icon { margin-right: 4px; font-style: normal; }

.node-id { margin-top: 5px; font-size: 0.8rem; color: rgba(255, 255, 255, 0.3); font-family: monospace; }

.drawer-content { flex: 1; overflow-y: auto; padding-right: 5px; } 
.info-item { margin-bottom: 25px; }
.info-item label { display: block; font-size: 0.9rem; color: #8c8c8c; margin-bottom: 10px; font-weight: bold; }
.text-content { font-size: 0.95rem; line-height: 1.6; color: rgba(255, 255, 255, 0.8); white-space: pre-wrap; }

/* 属性规格：嵌套样式 */
.attr-group { display: flex; flex-direction: column; gap: 10px; }
.attr-row { font-size: 0.9rem; }
.attr-key { color: #1890ff; font-weight: bold; display: block; margin-bottom: 4px; }
.attr-val { color: rgba(255, 255, 255, 0.9); }

.attr-nested {
  margin-left: 10px; margin-top: 6px; padding: 8px 12px;
  background: rgba(255, 255, 255, 0.05);
  border-left: 2px solid #1890ff; border-radius: 0 4px 4px 0;
}
.sub-attr { margin-bottom: 4px; font-size: 0.85rem; display: flex; }
.sub-key { color: rgba(255, 255, 255, 0.45); margin-right: 8px; }
.sub-val { color: #a5c2ff; font-weight: 500; }

/* 图集网格 */
.viewer-gallery { display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; }
.viewer-img-wrap {
  width: 100%; aspect-ratio: 16 / 9; border-radius: 6px; overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.2); cursor: zoom-in;
}
.viewer-img-wrap img { width: 100%; height: 100%; object-fit: cover; }

/* 图片预览层 */
.lightbox-overlay {
  position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(0, 0, 0, 0.85); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
}
.lightbox-content img { max-width: 90vw; max-height: 90vh; border-radius: 4px; }

/* 动画 */
.slide-fade-enter-active, .slide-fade-leave-active { transition: all 0.3s ease-out; }
.slide-fade-enter-from, .slide-fade-leave-to { transform: translateX(100%); opacity: 0; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>