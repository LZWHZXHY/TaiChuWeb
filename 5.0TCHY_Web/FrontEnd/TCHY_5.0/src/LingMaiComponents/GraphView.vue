<template>
  <div class="graph-container" ref="containerRef">
    <div id="graph-dom" ref="graphRef"></div>
    
    <div class="graph-controls">
      <div class="title">🌌 灵脉 · 知识图谱</div>
      
      <div class="search-box">
        <input 
          v-model="searchQuery" 
          placeholder="搜索节点..." 
          class="search-input"
          @input="handleSearch"
        />
        <div class="search-modes">
          <label title="保留所有节点，高亮匹配项">
            <input type="radio" value="highlight" v-model="searchMode" @change="handleSearch"> 高亮
          </label>
          <label title="仅显示匹配项">
            <input type="radio" value="filter" v-model="searchMode" @change="handleSearch"> 过滤
          </label>
        </div>
      </div>

      <div class="speed-control">
        <div class="label-row">
          <span class="label">节点跳出间隔</span>
          <span class="val">{{ spawnInterval }}ms</span>
        </div>
        <input type="range" min="0" max="150" step="5" v-model="spawnInterval" class="speed-slider">
      </div>

      <div class="info">节点: {{ displayNodeCount }} | 连线: {{ displayLinkCount }}</div>
      <button class="reset-btn" @click="rebootAnimation">💥 宇宙大爆炸启动</button>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, onBeforeUnmount, nextTick } from 'vue'
import ForceGraph from 'force-graph'
import apiClient from '@/utils/api'

const emit = defineEmits(['node-click'])
const containerRef = ref(null)
const graphRef = ref(null)

// 数据状态
const rawData = ref({ nodes: [], links: [] }) // 原始完整数据
const displayNodeCount = ref(0)
const displayLinkCount = ref(0)

// 搜索与动画状态
const searchQuery = ref('')
const searchMode = ref('highlight') // 'highlight' | 'filter'
const spawnInterval = ref(25) 
const growDuration = 500;

let myGraph = null
let resizeObserver = null
let hoverNode = null;

const OBSIDIAN_COLORS = ['#c77dff', '#9bf6ff', '#ffadad', '#ffd6a5', '#bdb2ff', '#a0c4ff', '#fdffb6'];
const getColor = (group) => {
  if (!group) return '#999';
  let hash = 0;
  for (let i = 0; i < group.length; i++) hash = group.charCodeAt(i) + ((hash << 5) - hash);
  return OBSIDIAN_COLORS[Math.abs(hash) % OBSIDIAN_COLORS.length];
}

// 🔥 核心逻辑：大爆炸生长动画
const rebootAnimation = () => {
  if (!myGraph) return;
  const data = myGraph.graphData();
  const now = Date.now();
  
  data.nodes.forEach((n, idx) => {
    // 1. 中心随机喷发
    n.x = (Math.random() - 0.5) * 40;
    n.y = (Math.random() - 0.5) * 40;
    n.vx = (Math.random() - 0.5) * 10;
    n.vy = (Math.random() - 0.5) * 10;
    
    // 2. 序列计时
    n.growStarted = now + (idx * spawnInterval.value);
  });
  
  myGraph.graphData(data);
  myGraph.d3Force('charge').strength(-300);
  myGraph.d3ReheatSimulation(); 
  
  setTimeout(() => {
    myGraph.zoomToFit(1200, 100);
    myGraph.d3Force('charge').strength(-120);
  }, 600);
}

// 🔥 核心逻辑：双模式搜索处理
const handleSearch = () => {
  if (!myGraph) return;
  const query = searchQuery.value.trim().toLowerCase();
  
  // 1. 如果搜索为空，恢复完整数据
  if (!query) {
    myGraph.graphData(rawData.value);
    displayNodeCount.value = rawData.value.nodes.length;
    displayLinkCount.value = rawData.value.links.length;
    return;
  }

  // 2. 过滤模式：重新计算子集数据
  if (searchMode.value === 'filter') {
    const matchedNodes = rawData.value.nodes.filter(node => node.name.toLowerCase().includes(query));
    const matchedNodeIds = new Set(matchedNodes.map(n => n.id));
    const matchedLinks = rawData.value.links.filter(link => {
      const s = typeof link.source === 'object' ? link.source.id : link.source;
      const t = typeof link.target === 'object' ? link.target.id : link.target;
      return matchedNodeIds.has(s) && matchedNodeIds.has(t);
    });
    myGraph.graphData({ nodes: matchedNodes, links: matchedLinks });
    displayNodeCount.value = matchedNodes.length;
    displayLinkCount.value = matchedLinks.length;
  } 
  // 3. 高亮模式：结构不变，依赖 nodeCanvasObject 重绘
  else {
    myGraph.graphData(rawData.value);
    displayNodeCount.value = rawData.value.nodes.length;
    displayLinkCount.value = rawData.value.links.length;
  }
}

const initGraph = async () => {
  await nextTick() 
  if (!graphRef.value || !containerRef.value) return;

  try {
    const res = await apiClient.get('/Graph/full')
    const data = res.data;
    data.nodes.forEach(n => {
      if (!n.color) n.color = getColor(n.group || 'default');
      // 初始时让所有节点处于“未来”出生状态
      n.growStarted = Date.now() + 1000000; 
    });

    rawData.value = JSON.parse(JSON.stringify(data));
    displayNodeCount.value = data.nodes.length;
    displayLinkCount.value = data.links.length;

    myGraph = ForceGraph()(graphRef.value)
      .graphData(rawData.value)
      .backgroundColor('#1e1e1e') 
      .width(containerRef.value.offsetWidth)
      .height(containerRef.value.offsetHeight)
      .nodeRelSize(4)
      .onNodeHover(node => {
        hoverNode = node || null;
        graphRef.value.style.cursor = node ? 'pointer' : null;
      })
      // 绘制逻辑：融合生长动画与高亮模式
      .nodeCanvasObject((node, ctx, globalScale) => {
        const now = Date.now();
        if (!node.growStarted) return;
        const elapsed = now - node.growStarted;
        if (elapsed < 0) return;

        const progress = Math.min(elapsed / growDuration, 1);
        const easeProgress = 1 - Math.pow(1 - progress, 3);
        const currentR = 3.5 * easeProgress;

        const label = node.name;
        const query = searchQuery.value.trim().toLowerCase();
        const isMatch = query && label.toLowerCase().includes(query);
        const isHover = hoverNode === node;
        
        // ✨ 高亮模式逻辑：未匹配的节点变极其透明
        if (searchMode.value === 'highlight' && query && !isMatch) {
          ctx.globalAlpha = 0.05;
        } else {
          ctx.globalAlpha = 1;
        }

        ctx.beginPath();
        ctx.arc(node.x, node.y, currentR, 0, 2 * Math.PI, false);
        ctx.fillStyle = isMatch ? '#ff4d4f' : node.color; 
        ctx.fill();

        // 文字 Halo (生长到一定程度才显示)
        if ((globalScale > 1.2 || isHover || isMatch) && progress > 0.6) {
           const fontSize = 12 / globalScale;
           ctx.font = `${fontSize < 3 ? 3 : fontSize}px "Sans-Serif"`;
           ctx.textAlign = 'left'; ctx.textBaseline = 'middle';
           ctx.strokeStyle = '#1e1e1e'; ctx.lineWidth = 3 / globalScale;
           ctx.strokeText(label, node.x + currentR + 3, node.y);
           ctx.fillStyle = isMatch ? '#ff4d4f' : '#e0e0e0'; 
           ctx.fillText(label, node.x + currentR + 3, node.y);
        }
        ctx.globalAlpha = 1;
      })
      .linkColor(link => {
        const now = Date.now();
        const s = link.source.growStarted || 0;
        const t = link.target.growStarted || 0;
        
        // 动画未出生时不显示线
        if (now < s || now < t) return 'transparent';

        // ✨ 搜索高亮逻辑
        const query = searchQuery.value.trim().toLowerCase();
        if (searchMode.value === 'highlight' && query) {
          const sMatch = link.source.name.toLowerCase().includes(query);
          const tMatch = link.target.name.toLowerCase().includes(query);
          return (sMatch || tMatch) ? 'rgba(255,255,255,0.5)' : 'rgba(255,255,255,0.02)';
        }
        return '#4a4a4a';
      })
      .linkWidth(link => {
        const query = searchQuery.value.trim().toLowerCase();
        return (searchMode.value === 'highlight' && query && (link.source.name.toLowerCase().includes(query) || link.target.name.toLowerCase().includes(query))) ? 1 : 0.5;
      })
      .onNodeClick(node => emit('node-click', node.id));

    // 物理：球形凝聚
    myGraph.d3Force('charge').strength(-120);
    myGraph.d3Force('link').distance(60);
    
    import('d3-force').then(d3 => {
      myGraph.d3Force('radial', d3.forceRadial(10).strength(0.15));
    });

    // 自动触发初始爆炸
    setTimeout(() => { rebootAnimation(); }, 500);

  } catch (e) { console.error(e); }
}

const setupResizeObserver = () => {
  if (!containerRef.value) return;
  resizeObserver = new ResizeObserver(entries => {
    for (const entry of entries) {
      const { width, height } = entry.contentRect;
      if (myGraph && width > 0 && height > 0) myGraph.width(width).height(height);
    }
  });
  resizeObserver.observe(containerRef.value);
}

onMounted(() => { initGraph(); setupResizeObserver(); })
onBeforeUnmount(() => { resizeObserver?.disconnect(); myGraph?._destructor(); })
</script>

<style scoped>
.graph-container { width: 100%; height: 100%; position: relative; overflow: hidden; background: #1e1e1e; display: flex; }
#graph-dom { width: 100%; height: 100%; }

.graph-controls {
  position: absolute; bottom: 20px; left: 20px;
  background: rgba(30, 30, 30, 0.85); backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 16px; border-radius: 12px; box-shadow: 0 8px 32px rgba(0,0,0,0.3);
  z-index: 10; display: flex; flex-direction: column; gap: 12px; min-width: 240px; color: #e0e0e0;
}

.search-box { display: flex; flex-direction: column; gap: 8px; }
.search-input { width: 100%; padding: 8px 10px; background: #2a2a2a; border: 1px solid #444; border-radius: 6px; font-size: 12px; color: #fff; outline: none; }
.search-modes { display: flex; gap: 12px; font-size: 11px; color: #aaa; }
.search-modes label { display: flex; align-items: center; gap: 4px; cursor: pointer; }

.speed-control {
  display: flex; flex-direction: column; gap: 6px;
  .label-row { display: flex; justify-content: space-between; align-items: center; }
  .label { font-size: 11px; color: #888; }
  .val { font-size: 11px; color: #c77dff; font-weight: bold; }
  .speed-slider {
    width: 100%; height: 4px; background: #444; border-radius: 2px;
    appearance: none; outline: none;
    &::-webkit-slider-thumb { appearance: none; width: 14px; height: 14px; background: #c77dff; border-radius: 50%; cursor: pointer; border: 2px solid #fff; }
  }
}

.info { font-size: 11px; color: #666; }
.reset-btn { width: 100%; padding: 8px 0; font-size: 12px; font-weight: 600; background: #333; color: #ddd; border: 1px solid #444; border-radius: 6px; cursor: pointer; transition: all 0.2s; }
.reset-btn:hover { background: #c77dff; color: #fff; border-color: #c77dff; }
</style>