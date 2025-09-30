<template>
  <div class="conway-root">
    <div class="controls">
      <button @click="startGame" :disabled="running">开始</button>
      <button @click="stopGame" :disabled="!running">暂停</button>
      <button @click="clearGrid">清空</button>
      <button @click="randomFill">随机</button>
      <label class="slider-label">
        速度
        <input type="range" min="50" max="600" step="10" v-model="interval" :disabled="running" />
        <span>{{ interval }}ms</span>
      </label>
      <span class="control-tip">
        滚轮缩放（缩小时能看到更多世界，放大时细胞变大），窗口和缩放都能自动适应
      </span>
    </div>
    <div
      class="grid-outer"
      ref="gridOuter"
      @wheel="handleWheel"
      @mousedown.middle="startPan"
      @mousemove="onPan"
      @mouseup.middle="stopPan"
      @mousedown.left="dragging = true"
      @mouseup.left="dragging = false"
      @mouseleave="handleMouseLeave"
    >
      <div class="grid">
        <div v-for="vy in viewportH" :key="vy" class="row">
          <div
            v-for="vx in viewportW"
            :key="vx"
            class="cell"
            :style="cellStyle"
            :class="{ alive: isAlive(vx + viewportX, vy + viewportY) }"
            @mousedown.left="toggleCell(vx + viewportX, vy + viewportY)"
            @mousemove.left="dragPaint(vx + viewportX, vy + viewportY)"
          ></div>
        </div>
      </div>
    </div>
    <div class="stats">
      代数: <b>{{ gen }}</b> | 活细胞: <b>{{ liveCount }}</b> | 视口左上角: <b>({{ viewportX }}, {{ viewportY }})</b> | 缩放: <b>{{ (scale*100).toFixed(0) }}%</b>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onUnmounted, watch, onMounted } from "vue";

const cellSizeBase = 16;
const scale = ref(1);
const cellSize = computed(() => cellSizeBase * scale.value);
const cellStyle = computed(() => ({
  width: cellSize.value + "px",
  height: cellSize.value + "px"
}));

const viewportX = ref(0);
const viewportY = ref(0);
// 动态计算视窗格子数
const viewportW = ref(80);
const viewportH = ref(40);

const gridOuter = ref(null);

function updateViewportSize() {
  // 用 grid-outer 尺寸动态算显示多少格子
  const el = gridOuter.value;
  if (el) {
    viewportW.value = Math.floor(el.clientWidth / cellSize.value);
    viewportH.value = Math.floor(el.clientHeight / cellSize.value);
  }
}

onMounted(() => {
  updateViewportSize();
  window.addEventListener("resize", updateViewportSize);
});
onUnmounted(() => {
  window.removeEventListener("resize", updateViewportSize);
});

watch(cellSize, () => {
  updateViewportSize();
});

const isPanning = ref(false);
const panStart = ref({ x: 0, y: 0, vpx: 0, vpy: 0 });
const dragging = ref(false);

const running = ref(false);
const interval = ref(150);
const timer = ref(null);
const gen = ref(0);
const aliveSet = ref(new Set());

function key(x, y) { return `${x},${y}`; }
function isAlive(x, y) { return aliveSet.value.has(key(x, y)); }
function toggleCell(x, y) {
  if (running.value) return;
  const k = key(x, y);
  if (aliveSet.value.has(k)) aliveSet.value.delete(k);
  else aliveSet.value.add(k);
}
function dragPaint(x, y) {
  if (dragging.value && !running.value) aliveSet.value.add(key(x, y));
}
const liveCount = computed(() => aliveSet.value.size);

function nextGen() {
  const newSet = new Set();
  const countMap = new Map();
  for (const k of aliveSet.value) {
    const [x, y] = k.split(',').map(Number);
    for (let dx=-1; dx<=1; dx++) {
      for (let dy=-1; dy<=1; dy++) {
        const nx = x+dx, ny = y+dy;
        const nk = key(nx, ny);
        countMap.set(nk, (countMap.get(nk)||0) + (dx||dy ? 1 : 0));
      }
    }
  }
  for (const [k, n] of countMap.entries()) {
    if (aliveSet.value.has(k)) {
      if (n===2 || n===3) newSet.add(k);
    } else {
      if (n===3) newSet.add(k);
    }
  }
  aliveSet.value = newSet;
  gen.value++;
}
function startGame() {
  running.value = true;
  timer.value = setInterval(nextGen, interval.value);
}
function stopGame() {
  running.value = false;
  if (timer.value) clearInterval(timer.value);
  timer.value = null;
}
function clearGrid() {
  stopGame();
  aliveSet.value = new Set();
  gen.value = 0;
}
function randomFill() {
  stopGame();
  for (let x=viewportX.value; x<viewportX.value+viewportW.value; x++)
    for (let y=viewportY.value; y<viewportY.value+viewportH.value; y++)
      if (Math.random()<0.17) aliveSet.value.add(key(x, y));
  gen.value = 0;
}
onUnmounted(() => { if (timer.value) clearInterval(timer.value); });
watch(interval, (val) => { if (running.value) { stopGame(); startGame(); } });

function handleWheel(e) {
  e.preventDefault();
  const zoom = e.deltaY < 0 ? 1.09 : 0.91;
  scale.value = Math.max(0.3, Math.min(2.5, scale.value * zoom));
}

function startPan(e) {
  e.preventDefault();
  isPanning.value = true;
  panStart.value = {
    x: e.clientX,
    y: e.clientY,
    vpx: viewportX.value,
    vpy: viewportY.value
  };
}
function onPan(e) {
  if (!isPanning.value) return;
  const cellSizeVal = cellSize.value;
  const dx = Math.floor((e.clientX - panStart.value.x) / cellSizeVal);
  const dy = Math.floor((e.clientY - panStart.value.y) / cellSizeVal);
  if (dx !== 0 || dy !== 0) {
    viewportX.value = panStart.value.vpx - dx;
    viewportY.value = panStart.value.vpy - dy;
  }
}
function stopPan() { isPanning.value = false; }
function handleMouseLeave() {
  stopPan();
  dragging.value = false;
}
</script>

<style scoped>
.conway-root {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: #f5faff;
  border-radius: 18px;
  box-shadow: 0 6px 32px rgba(90, 186, 255, 0.12);
  padding: 22px 18px 16px 18px;
  overflow: hidden;
}
.controls {
  margin-bottom: 12px;
  display: flex;
  gap: 16px;
  align-items: center;
  flex-wrap: wrap;
  font-size: 15px;
}
button {
  padding: 7px 14px;
  border: none;
  border-radius: 6px;
  background: linear-gradient(90deg, #5abaff 0%, #53e3f7 90%);
  color: #fff;
  font-weight: 500;
  cursor: pointer;
  transition: box-shadow 0.2s, background 0.17s;
  box-shadow: 0 2px 8px rgba(90,186,255,0.07);
}
button:disabled {
  background: #b4dffa;
  color: #eee;
  cursor: not-allowed;
  box-shadow: none;
}
.slider-label {
  display: flex;
  align-items: center;
  gap: 6px;
}
input[type="range"] {
  accent-color: #5abaff;
  margin: 0 6px;
}
.control-tip {
  color: #5abaff;
  font-size: 14px;
  font-style: italic;
  margin-left: 8px;
}
.grid-outer {
  position: relative;
  background: #e9f4ff;
  border-radius: 10px;
  border: 2.5px solid #5abaff;
  box-shadow: 0 3px 20px rgba(90,186,255,0.07);
  overflow: auto;
  width: 90vw;
  height: 72vh;
  margin-bottom: 10px;
  user-select: none;
}
.grid {
  /* grid自动撑开，内部滚动 */
}
.row {
  display: flex;
}
.cell {
  border: 1px solid #d7e4fa;
  background: #f3f9fd;
  cursor: pointer;
  transition: background 0.18s, box-shadow 0.18s;
  box-sizing: border-box;
  box-shadow: 0 1px 0.5px rgba(90,186,255,0.05);
  min-width: 2px;
  min-height: 2px;
}
.cell:hover {
  background: #bfe7fb;
  box-shadow: 0 2px 7px #5abaff22;
}
.cell.alive {
  background: linear-gradient(120deg, #5abaff 30%, #53e3f7 100%);
  border-color: #53e3f7;
  box-shadow: 0 2px 10px #5abaff44;
  animation: popcell 0.18s;
}
@keyframes popcell {
  from { transform: scale(0.8);}
  to { transform: scale(1);}
}
.stats {
  font-size: 15px;
  margin-top: 7px;
  color: #253043;
  background: #e8f9ff;
  border-radius: 8px;
  padding: 6px 17px;
  box-shadow: 0 2px 7px #5abaff11;
}
</style>