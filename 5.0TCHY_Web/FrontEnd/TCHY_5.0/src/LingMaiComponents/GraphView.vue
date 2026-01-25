<template>
  <div class="graph-container" ref="containerRef">
    <div id="graph-dom" ref="graphRef"></div>
    
    <div class="graph-controls">
      <div class="title">🌌 灵脉 · 知识图谱</div>
      <div class="info">节点: {{ nodeCount }} | 连线: {{ linkCount }}</div>
      <div class="tip">滚轮缩放 / 拖拽移动</div>
      <button class="reset-btn" @click="resetCamera">🎯 复位视角</button>
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
const nodeCount = ref(0)
const linkCount = ref(0)
let myGraph = null
let resizeObserver = null

// 🔥 核心修复：用这个变量来存当前悬停的节点
let hoverNode = null;

const resetCamera = () => {
  if (myGraph) {
    myGraph.zoomToFit(400, 20); 
  }
}

const initGraph = async () => {
  await nextTick() 
  
  if (!graphRef.value || !containerRef.value) return;

  const width = containerRef.value.offsetWidth
  const height = containerRef.value.offsetHeight

  if (width === 0 || height === 0) {
    setTimeout(initGraph, 100);
    return;
  }

  try {
    const res = await apiClient.get('/Graph/full')
    const gData = JSON.parse(JSON.stringify(res.data))
    
    nodeCount.value = gData.nodes.length
    linkCount.value = gData.links.length

    const elem = graphRef.value
    
    myGraph = ForceGraph()(elem)
      .graphData(gData)
      .backgroundColor('#ffffff') 
      .width(width)
      .height(height)
      .nodeRelSize(6)
      .nodeAutoColorBy('group')
      
      // 🔥 修复点 1：通过事件手动追踪悬停节点
      .onNodeHover(node => {
        hoverNode = node || null; // 更新悬停状态
        elem.style.cursor = node ? 'pointer' : null; // 鼠标变手型
      })

      // 自定义绘制
      .nodeCanvasObject((node, ctx, globalScale) => {
        const label = node.name;
        const r = 5; 
        
        ctx.beginPath();
        ctx.arc(node.x, node.y, r, 0, 2 * Math.PI, false);
        ctx.fillStyle = node.color || '#333';
        ctx.fill();
        
        // 🔥 修复点 2：使用变量判断，而不是调用不存在的方法
        if (hoverNode === node) {
           ctx.beginPath();
           ctx.arc(node.x, node.y, r + 2, 0, 2 * Math.PI, false);
           ctx.strokeStyle = node.color; // 用节点自己的颜色画圈
           ctx.lineWidth = 2;
           ctx.stroke();
        }

        // 文字绘制 (LOD)
        if (globalScale >= 1.2) { 
           // 保持文字大小适中
           const fontSize = 12 / globalScale; 
           ctx.font = `${4}px Sans-Serif`; 
           ctx.textAlign = 'center';
           ctx.textBaseline = 'top'; 
           ctx.fillStyle = '#333'; 
           ctx.fillText(label, node.x, node.y + r + 2); 
        }
      })
      .nodePointerAreaPaint((node, color, ctx) => {
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(node.x, node.y, 8, 0, 2 * Math.PI, false);
        ctx.fill();
      })

      .linkWidth(link => link.type === 'hierarchy' ? 2 : 1)
      .linkColor(link => link.type === 'hierarchy' ? '#ccc' : '#ffcfcf')
      .linkDirectionalParticles(2)
      .linkDirectionalParticleSpeed(0.005)
      .linkDirectionalParticleWidth(2)

      .onNodeClick(node => {
        emit('node-click', node.id)
      })
      .onNodeDragEnd(node => {
        node.fx = node.x;
        node.fy = node.y;
      });

    myGraph.d3Force('charge').strength(-300); 
    myGraph.d3Force('link').distance(100);    

    setTimeout(() => {
        myGraph.zoomToFit(400, 50);
    }, 500);
    
  } catch (e) {
    console.error("图谱加载失败", e)
  }
}

const setupResizeObserver = () => {
  if (!containerRef.value) return;
  
  resizeObserver = new ResizeObserver(entries => {
    for (const entry of entries) {
      const { width, height } = entry.contentRect;
      if (myGraph && width > 0 && height > 0) {
        myGraph.width(width);
        myGraph.height(height);
      }
    }
  });
  
  resizeObserver.observe(containerRef.value);
}

onMounted(() => {
  initGraph()
  setupResizeObserver()
})

onBeforeUnmount(() => {
  if (resizeObserver) resizeObserver.disconnect()
  if (myGraph) myGraph._destructor(); 
})
</script>

<style scoped>
.graph-container {
  width: 100%;
  height: 100%; 
  position: relative;
  overflow: hidden;
  background: #fff;
  display: flex; 
}

#graph-dom {
  width: 100%;
  height: 100%;
}

.graph-controls {
  position: absolute;
  bottom: 20px;
  left: 20px;
  background: rgba(255, 255, 255, 0.95);
  padding: 12px 16px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  border: 1px solid #eee;
  z-index: 10;
  
  .title { font-size: 15px; font-weight: 700; margin-bottom: 4px; color: #333; }
  .info { font-size: 12px; color: #666; margin-bottom: 4px;}
  .tip { font-size: 11px; color: #999; font-style: italic; margin-bottom: 8px; }
  
  .reset-btn {
    width: 100%;
    padding: 4px 0;
    font-size: 12px;
    background: #f0f0f0;
    border: 1px solid #ddd;
    border-radius: 4px;
    cursor: pointer;
    &:hover { background: #e0e0e0; }
  }
}
</style>