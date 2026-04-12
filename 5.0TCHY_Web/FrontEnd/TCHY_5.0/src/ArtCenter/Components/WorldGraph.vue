<template>
  <div class="graph-wrapper" ref="wrapperRef">
    <div id="graph-canvas" ref="graphRef"></div>

    <div class="graph-toolbar">
      <div class="search-group">
        <span class="icon">///</span>
        <input 
          v-model="searchQuery" 
          class="cyber-input" 
          placeholder="SEARCH_DATA..." 
          @input="handleSearch"
        />
      </div>
      
      <div class="separator"></div>

      <div class="speed-control">
        <span class="label">EVOLUTION:</span>
        <input 
          type="range" 
          min="0.1" 
          max="10" 
          step="0.1"
          v-model.number="evolutionSpeed" 
          class="cyber-range" 
          title="Adjust growth speed"
        />
      </div>

      <div class="separator"></div>

      <div class="speed-control">
        <span class="label">REPULSION:</span>
        <input 
          type="range" 
          min="-2000" 
          max="-100" 
          step="50"
          v-model.number="repulsionStrength" 
          class="cyber-range" 
          title="Adjust node repulsion"
        />
      </div>

      <div class="separator"></div>

      <div class="stats">
        MASS: {{ totalMass }} <span class="sep">|</span> ENTITIES: {{ currentRenderCount }} / {{ totalNodeCount }}
      </div>
      
      <button class="cyber-btn" @click="resetCamera">RESET</button>
      <button class="cyber-btn" @click="replayEvolution">REPLAY</button>
    </div>

    <transition name="slide-right">
      <div v-if="selectedNode" class="node-detail-panel custom-scroll">
        <div class="panel-header">
          <span class="node-type">{{ selectedNode.type || 'ENTITY' }}</span>
          <button class="close-btn" @click="selectedNode = null">×</button>
        </div>

        <div class="panel-body">
          <div v-if="selectedNode.image" class="node-cover">
            <img :src="getCleanImageUrl(selectedNode.image)" alt="cover" @error="imgError" />
          </div>

          <h2 class="node-title">{{ selectedNode.label }}</h2>
          <div class="data-quality">
            DATA_WEIGHT: <span :style="{ width: Math.min(selectedNode.val * 5, 200) + 'px' }"></span>
          </div>

          <div class="node-meta">
            <span>AUTHOR: <b style="color:#fff">{{ selectedNode.author || 'SYSTEM' }}</b></span>
            <span>UPDATED: {{ selectedNode.updated_at }}</span>
          </div>

          <div class="divider"></div>

          <div class="node-section">
            <h3 class="sec-title">/// SUMMARY</h3>
            <p class="desc-text">{{ selectedNode.description }}</p>
          </div>

          <div v-if="selectedNode.properties" class="node-section">
            <h3 class="sec-title">/// PROPERTIES</h3>
            <div class="props-grid">
              <div v-for="(val, key) in selectedNode.properties" :key="key" class="prop-item">
                <span class="prop-key">{{ key }}</span>
                <span class="prop-val" :title="formatProp(val)">{{ formatProp(val) }}</span>
              </div>
            </div>
          </div>
          
          <div class="action-area">
             <button class="cyber-btn full" @click="$emit('select-node', selectedNode.id)">
               OPEN DOCUMENT >>
             </button>
          </div>
        </div>
      </div>
    </transition>

    <div v-if="loading" class="loading-mask">
      <div class="loader">SYSTEM_INITIALIZING...</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import ForceGraph from 'force-graph'
import apiClient from '@/utils/api'

const props = defineProps({ ipId: { type: [String, Number], required: true } })
const emit = defineEmits(['select-node'])

const wrapperRef = ref(null)
const graphRef = ref(null)
const loading = ref(true)

const graphInstance = ref(null)
const rawNodes = ref([]) 
const rawLinks = ref([]) 
const displayedNodes = ref([]) 
const displayedLinks = ref([]) 

const totalNodeCount = ref(0)
const currentRenderCount = ref(0)
const totalMass = ref(0) 
const searchQuery = ref('')
const selectedNode = ref(null)

const evolutionSpeed = ref(2) 
const repulsionStrength = ref(-160) // 🚀 增强初始排斥力，让整体更扩散
let evolutionTimer = null
let hoverNode = null
let hoverLink = null
let lastClickTime = 0

// --- 1. 动态色彩生成 ---
const getAutoColor = (text) => {
  if (!text) return '#888888';
  let hash = 0;
  for (let i = 0; i < text.length; i++) {
    hash = text.charCodeAt(i) + ((hash << 5) - hash);
  }
  const h = Math.abs(hash % 360);
  return `hsl(${h}, 70%, 65%)`;
};

// --- 2. 物理引擎安全重启 ---
const reheatSimulation = () => {
  if (!graphInstance.value) return;
  if (typeof graphInstance.value.d3ReheatSimulation === 'function') {
    graphInstance.value.d3ReheatSimulation();
  } else {
    graphInstance.value.d3AlphaTarget(0.1);
    graphInstance.value.d3Restart();
    setTimeout(() => graphInstance.value?.d3AlphaTarget(0), 300);
  }
}

// 🚀 优化：监听排斥力滑块
watch(repulsionStrength, (newVal) => {
  if (graphInstance.value) {
    const chargeForce = graphInstance.value.d3Force('charge');
    if (chargeForce) {
      chargeForce
        .strength(node => (node.val > 15 ? newVal * 2.2 : newVal))
        .distanceMax(300); // 🚀 扩大排斥有效半径
    }
    reheatSimulation();
  }
});

// --- 3. 辅助计算 ---
const calculateNodeMass = (node) => {
  let mass = 4; 
  if (node.description) mass += Math.min(node.description.length / 50, 30);
  if (node.properties) mass += Math.min(Object.keys(node.properties).length * 1.5, 15);
  if (node.image) mass += 6;
  return Math.round(mass);
}

// --- 4. 演化逻辑 ---
const startEvolution = () => {
  if (evolutionTimer) clearInterval(evolutionTimer);
  displayedNodes.value = [];
  displayedLinks.value = [];
  currentRenderCount.value = 0;

  const pendingNodes = JSON.parse(JSON.stringify(rawNodes.value));
  pendingNodes.sort((a, b) => {
    const timeA = new Date(a.updatedAt || a.updateTime || 0).getTime();
    const timeB = new Date(b.updatedAt || b.updateTime || 0).getTime();
    return timeA - timeB || a.id - b.id;
  });

  const visibleNodeIds = new Set();
  const addedLinkKeys = new Set();

  evolutionTimer = setInterval(() => {
    if (pendingNodes.length === 0) {
      clearInterval(evolutionTimer);
      return;
    }

    const chunk = pendingNodes.splice(0, evolutionSpeed.value);
    chunk.forEach(node => {
      node.x = (Math.random() - 0.5) * 10;
      node.y = (Math.random() - 0.5) * 10;
      visibleNodeIds.add(String(node.id));
    });

    displayedNodes.value.push(...chunk);
    currentRenderCount.value = displayedNodes.value.length;

    rawLinks.value.forEach(link => {
      const sId = String(link.source);
      const tId = String(link.target);
      const linkKey = `${sId}->${tId}`;
      if (visibleNodeIds.has(sId) && visibleNodeIds.has(tId) && !addedLinkKeys.has(linkKey)) {
        displayedLinks.value.push({ ...link, source: sId, target: tId });
        addedLinkKeys.add(linkKey);
      }
    });

    if (graphInstance.value) {
      graphInstance.value.graphData({
        nodes: [...displayedNodes.value],
        links: [...displayedLinks.value]
      });
      reheatSimulation();
    }
  }, 100);
}

// --- 5. 初始化图表 ---
const initGraph = async () => {
  loading.value = true
  try {
    const res = await apiClient.get(`/Graph/ip/${props.ipId}`)
    const { nodes, edges } = res.data

    const validIds = new Set(nodes.map(n => String(n.id)));
    rawNodes.value = nodes.map(n => ({ ...n, id: String(n.id), val: calculateNodeMass(n) }));
    rawLinks.value = edges.filter(l => {
      const s = String(typeof l.source === 'object' ? l.source.id : l.source);
      const t = String(typeof l.target === 'object' ? l.target.id : l.target);
      return validIds.has(s) && validIds.has(t);
    }).map(l => ({
      ...l,
      source: String(typeof l.source === 'object' ? l.source.id : l.source),
      target: String(typeof l.target === 'object' ? l.target.id : l.target)
    }));

    totalNodeCount.value = nodes.length;
    totalMass.value = rawNodes.value.reduce((s, n) => s + n.val, 0);

    const width = wrapperRef.value.offsetWidth
    const height = wrapperRef.value.offsetHeight

    graphInstance.value = ForceGraph()(graphRef.value)
      .width(width).height(height).backgroundColor('#080808')
      .nodeId('id')
      .linkSource('source')
      .linkTarget('target')
      .nodeCanvasObject((node, ctx, globalScale) => {
        const isSelected = selectedNode.value?.id === node.id;
        const isHover = node === hoverNode;
        const query = (searchQuery.value || '').trim().toLowerCase();
        const isMatch = query && node.label.toLowerCase().includes(query);
        const isDimmed = query && !isMatch && !isSelected;

        ctx.globalAlpha = isDimmed ? 0.15 : 1;
        const r = node.val || 5;

        let baseColor = getAutoColor(node.type || 'Entity');
        if (isSelected || isMatch) baseColor = '#FFFFFF';
        if (isHover && !isSelected) { ctx.shadowBlur = 15; ctx.shadowColor = baseColor; }

        ctx.beginPath();
        ctx.arc(node.x, node.y, r, 0, 2 * Math.PI, false);
        ctx.fillStyle = baseColor;
        ctx.fill();
        ctx.shadowBlur = 0;

        if (isSelected || isMatch) {
          ctx.beginPath();
          ctx.arc(node.x, node.y, r + 4, 0, 2 * Math.PI, false);
          ctx.strokeStyle = '#D92323';
          ctx.lineWidth = 2.5 / globalScale;
          ctx.stroke();
        }

        const showText = globalScale > 0.8 || isHover || isSelected;
        if (showText) {
          const fontSize = isSelected ? 14 : 10;
          ctx.font = `${fontSize}px 'JetBrains Mono'`;
          ctx.textAlign = 'center';
          ctx.fillStyle = isSelected ? '#D92323' : '#cccccc';
          ctx.fillText(node.label, node.x, node.y + r + fontSize + 2);
        }
        ctx.globalAlpha = 1;
      })
      .linkCanvasObject((link, ctx, globalScale) => {
        const isStructure = link.label === '包含';
        const isHoveredLink = link === hoverLink;
        const sId = typeof link.source === 'object' ? link.source.id : link.source;
        const tId = typeof link.target === 'object' ? link.target.id : link.target;
        const isRelated = selectedNode.value?.id === sId || selectedNode.value?.id === tId || 
                          hoverNode?.id === sId || hoverNode?.id === tId;

        let lineColor = isStructure ? 'rgba(100, 100, 100, 0.3)' : getAutoColor(link.label);
        if (isRelated || isHoveredLink) lineColor = '#FFFFFF';

        ctx.lineWidth = (isHoveredLink || isRelated ? 2.5 : 1) / globalScale;
        ctx.strokeStyle = lineColor;

        if (!isStructure) {
          ctx.setLineDash([4 / globalScale, 2 / globalScale]);
          if (isRelated || isHoveredLink) { ctx.shadowColor = lineColor; ctx.shadowBlur = 8; }
        } else { ctx.setLineDash([]); }

        ctx.beginPath();
        ctx.moveTo(link.source.x, link.source.y);
        ctx.lineTo(link.target.x, link.target.y);
        ctx.stroke();
        ctx.setLineDash([]); ctx.shadowBlur = 0;

        if (!isStructure && (globalScale > 1.5 || isHoveredLink || isRelated)) {
          const midX = link.source.x + (link.target.x - link.source.x) * 0.5;
          const midY = link.source.y + (link.target.y - link.source.y) * 0.5;
          ctx.font = `${9/globalScale}px 'JetBrains Mono'`;
          const txt = link.label || '';
          const tw = ctx.measureText(txt).width;
          ctx.fillStyle = '#080808';
          ctx.fillRect(midX - tw/2 - 2, midY - 6, tw + 4, 12);
          ctx.fillStyle = lineColor;
          ctx.fillText(txt, midX - tw/2, midY + 3);
        }
      })
      .onNodeHover(node => {
        wrapperRef.value.style.cursor = node ? 'pointer' : null;
        hoverNode = node;
      })
      .onLinkHover(link => { hoverLink = link; })
      .onNodeClick(node => {
        const now = Date.now();
        if (now - lastClickTime < 300) emit('select-node', node.id);
        else {
          selectedNode.value = node;
          graphInstance.value.centerAt(node.x, node.y, 600);
          graphInstance.value.zoom(2.5, 600);
        }
        lastClickTime = now;
      })
      .onBackgroundClick(() => { selectedNode.value = null; });

    // 🚀 6. 扩散性物理调优
    const chargeForce = graphInstance.value.d3Force('charge');
    if (chargeForce) {
      chargeForce
        .strength(repulsionStrength.value)
        .distanceMax(300); // 🚀 允许推力传递得更远
    }

    const linkForce = graphInstance.value.d3Force('link');
    if (linkForce) {
      // 🚀 显著拉长连线，给簇群留出空间
      linkForce.distance(link => link.label === '包含' ? 100 : 220); 
    }

    import('d3-force').then(d3 => {
      if (graphInstance.value) {
        // 🚀 减弱引力场强度，让点不再死磕圆心
        graphInstance.value.d3Force('x', d3.forceX(0).strength(0.025));
        graphInstance.value.d3Force('y', d3.forceY(0).strength(0.025));
        graphInstance.value.d3Force('center', d3.forceCenter(0, 0));
        graphInstance.value.d3Force('collide', d3.forceCollide(node => node.val + 15).iterations(2));
        reheatSimulation();
      }
    });

    graphInstance.value.d3VelocityDecay(0.35); // 🚀 降低阻尼，让扩散更丝滑
    
    startEvolution();

  } catch (e) { console.error("Graph Error:", e); } finally { loading.value = false }
}

const handleSearch = () => reheatSimulation();
const resetCamera = () => graphInstance.value?.zoomToFit(800, 50);
const replayEvolution = () => startEvolution();

let resizeObserver = null;
const setupResize = () => {
  resizeObserver = new ResizeObserver(entries => {
    if (!entries[0]) return;
    const { width, height } = entries[0].contentRect
    if(graphInstance.value) graphInstance.value.width(width).height(height)
  })
  if(wrapperRef.value) resizeObserver.observe(wrapperRef.value)
}

onMounted(() => { initGraph(); setupResize(); })
onBeforeUnmount(() => { 
  if(resizeObserver) resizeObserver.disconnect(); 
  if(evolutionTimer) clearInterval(evolutionTimer);
  if(graphInstance.value) graphInstance.value._destructor?.();
})
</script>





<style scoped>
/* 样式保持原样，仅补充滑块可能需要的微调 */
.graph-wrapper { position: relative; width: 100%; height: 100%; background: #080808; overflow: hidden; font-family: 'JetBrains Mono', monospace; color: #eee; }
.graph-toolbar { position: absolute; top: 20px; left: 20px; display: flex; gap: 15px; align-items: center; background: rgba(0,0,0,0.9); padding: 8px 12px; border: 1px solid #333; z-index: 10; flex-wrap: wrap; }
.search-group { display: flex; align-items: center; border-bottom: 1px solid #666; }
.icon { font-size: 12px; color: #666; margin-right: 5px; font-weight: bold; }
.cyber-input { background: transparent; border: none; color: #fff; font-family: inherit; font-size: 12px; width: 140px; outline: none; }
.stats { font-size: 10px; color: #444; font-weight: bold; letter-spacing: 1px; }
.sep { color: #222; margin: 0 5px; }
.separator { width: 1px; height: 16px; background: #333; }
.speed-control { display: flex; align-items: center; gap: 8px; font-size: 10px; color: #666; font-weight: bold; }
.cyber-range { -webkit-appearance: none; width: 80px; height: 4px; background: #333; outline: none; cursor: pointer; }
.cyber-range::-webkit-slider-thumb { -webkit-appearance: none; width: 8px; height: 8px; background: #D92323; cursor: pointer; border-radius: 0; }
.cyber-btn { background: transparent; color: #888; border: 1px solid #444; font-family: inherit; font-size: 10px; padding: 4px 10px; cursor: pointer; transition: 0.1s; }
.cyber-btn:hover { border-color: #D92323; color: #D92323; }
.node-detail-panel { position: absolute; top: 0; right: 0; bottom: 0; width: 450px; background: #111; border-left: 2px solid #D92323; z-index: 20; display: flex; flex-direction: column; box-shadow: -10px 0 50px rgba(0,0,0,0.9); }
.panel-header { height: 55px; display: flex; justify-content: space-between; align-items: center; padding: 0 25px; border-bottom: 1px solid #333; background: #080808; }
.node-type { font-size: 12px; color: #D92323; border: 1px solid #D92323; padding: 3px 8px; font-weight: bold; }
.close-btn { background: transparent; border: none; color: #666; font-size: 24px; cursor: pointer; }
.close-btn:hover { color: #fff; }
.panel-body { flex: 1; overflow-y: auto; padding: 30px; }
.node-cover { width: 100%; height: 200px; background: #050505; margin-bottom: 25px; border: 1px solid #333; }
.node-cover img { width: 100%; height: 100%; object-fit: cover; }
.node-title { margin: 0 0 10px 0; font-size: 28px; color: #fff; font-weight: bold; letter-spacing: 1px; }
.data-quality { font-size: 10px; color: #666; margin-bottom: 20px; display: flex; align-items: center; gap: 8px; }
.data-quality span { height: 6px; background: #333; display: block; }
.node-meta { font-size: 12px; color: #888; display: flex; flex-direction: column; gap: 6px; }
.divider { height: 1px; background: #222; margin: 30px 0; }
.sec-title { font-size: 12px; color: #D92323; margin-bottom: 12px; font-weight: bold; border-left: 3px solid #D92323; padding-left: 8px; }
.desc-text { font-size: 14px; line-height: 1.8; color: #e0e0e0; white-space: pre-wrap; }
.props-grid { display: grid; grid-template-columns: 1fr; gap: 10px; }
.prop-item { display: flex; justify-content: space-between; font-size: 13px; border-bottom: 1px solid #222; padding-bottom: 8px; }
.prop-key { color: #888; font-weight: bold; min-width: 80px; }
.prop-val { color: #fff; text-align: right; }
.action-area { margin-top: 30px; }
.cyber-btn.full { width: 100%; padding: 14px; background: #fff; color: #000; border: none; font-weight: bold; font-size: 14px; letter-spacing: 1px; transition: 0.2s; }
.cyber-btn.full:hover { background: #D92323; color: #fff; }
.slide-right-enter-active, .slide-right-leave-active { transition: transform 0.2s ease-out; }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }
.loading-mask { position: absolute; inset: 0; background: #000; display: flex; align-items: center; justify-content: center; z-index: 50; }
.loader { color: #444; font-size: 10px; letter-spacing: 2px; animation: blink 1s infinite; }
@keyframes blink { 50% { opacity: 0; } }
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
</style>