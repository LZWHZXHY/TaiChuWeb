<template>
  <div class="cyber-rack-container custom-scroll">
    <div class="system-status-bar">
      <div class="status-light blink"></div>
      <span class="status-text">SYSTEM_CONFIG // 终端配置</span>
      <div class="deco-lines"></div>
      <span class="battery-level">PWR: 98%</span>
    </div>

    <div class="rack-module heavy-metal">
      <div class="module-header">
        <div class="header-left">
          <span class="mod-idx">01</span>
          <span class="mod-title">DATA_CORE // 核心数据</span>
        </div>
        <div class="hazard-stripes"></div>
      </div>
      
      <div class="module-body">
        <div class="control-group">
          <div class="group-label">
            <span>FEATURED_WORKS</span>
            <span class="count">[4/4]</span>
          </div>
          <div class="slot-grid-2x2">
            <div
              v-for="(item, index) in form.works"
              :key="'w'+index"
              class="cyber-slot work-slot"
              :class="{ 'is-empty': !item, 'is-filled': item }"
              @click="openSelector('works', index)"
            >
              <div class="slot-frame">
                <template v-if="item">
                  <div class="holo-bg"></div>
                  <span class="chip-type">{{ item.type }}</span>
                  <div class="work-info">
                    <span class="w-title">{{ item.title }}</span>
                  </div>
                  <div class="scan-line"></div>
                </template>
                <template v-else>
                  <span class="empty-cross">+</span>
                  <span class="empty-label">NULL</span>
                </template>
              </div>
              <div class="corner-screw top-left"></div>
              <div class="corner-screw bottom-right"></div>
            </div>
          </div>
        </div>

        <div class="control-group mt-20">
          <div class="group-label">
            <span>DOC_ARCHIVE</span>
            <span class="count">[READ_ONLY]</span>
          </div>
          <div class="list-stack">
            <div
              v-for="(item, index) in form.articles"
              :key="'a'+index"
              class="data-strip"
              :class="{ 'has-data': item }"
              @click="openSelector('articles', index)"
            >
              <div class="strip-indicator"></div>
              <div class="strip-content">
                <template v-if="item">
                  <span class="file-icon">📄</span>
                  <span class="file-name">{{ item.title }}</span>
                  <span class="file-ext">.{{ item.type }}</span>
                </template>
                <template v-else>
                  <span class="empty-text">/// SLOT_EMPTY ///</span>
                </template>
              </div>
              <div class="strip-deco"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="rack-module glass-panel">
      <div class="module-header">
        <div class="header-left">
          <span class="mod-idx">02</span>
          <span class="mod-title">MEDALS // 荣誉矩阵</span>
        </div>
        <div class="led-row">
          <span></span><span></span><span></span>
        </div>
      </div>
      <div class="module-body honeycomb-bg">
        <div class="hex-grid">
          <div
            v-for="(badge, index) in form.achievements"
            :key="'ach'+index"
            class="hex-wrapper"
            @click="openSelector('achievements', index)"
          >
            <div class="hex-cell" :class="{ 'unlocked': badge }">
              <div class="hex-inner">
                <span v-if="badge" class="badge-text">{{ badge }}</span>
                <span v-else class="lock-icon">🔒</span>
              </div>
              <div class="hex-border"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="rack-module weapon-case">
      <div class="module-header">
        <div class="header-left">
          <span class="mod-idx">03</span>
          <span class="mod-title">INVENTORY // 装备库</span>
        </div>
        <div class="capacity-bar">
          <div class="fill" style="width: 45%"></div>
        </div>
      </div>
      <div class="module-body">
        <div class="inv-grid">
          <div
            v-for="(item, index) in form.inventory"
            :key="'inv'+index"
            class="inv-cell"
            :class="[item ? item.rarity : 'empty']"
            @click="openSelector('inventory', index)"
          >
            <div class="rarity-glow"></div>
            
            <div class="cell-content">
              <template v-if="item">
                <span class="item-name">{{ item.name }}</span>
                <div class="item-tech-deco"></div>
              </template>
              <template v-else>
                <div class="crosshair"></div>
              </template>
            </div>
            
            <div class="bracket tl"></div>
            <div class="bracket br"></div>
          </div>
        </div>
        <div class="case-footer">
          <span>SECURE_STORAGE</span>
          <span>UNIT_ID: 88-X</span>
        </div>
      </div>
    </div>

    <div class="rack-footer">
      <div class="vent-grill">
        <span></span><span></span><span></span><span></span><span></span>
      </div>
      <div class="warning-label">WARNING: AUTHORIZED PERSONNEL ONLY</div>
    </div>

    <div v-if="selectorState.visible" class="modal-backdrop" @click.self="selectorState.visible = false">
      <div class="cyber-modal">
        <div class="modal-header">
          <div class="modal-title-box">
            <span class="blink">></span> {{ selectorState.title }}
          </div>
          <button class="close-btn" @click="selectorState.visible = false">
            [ CLOSE ]
          </button>
        </div>
        
        <div class="modal-body custom-scroll">
          <div class="scan-grid-bg"></div>
          
          <div
            v-for="(opt, idx) in currentOptions"
            :key="idx"
            class="modal-option"
            @click="selectItem(opt)"
          >
            <div class="opt-left">
              <span class="opt-idx">{{ idx < 9 ? '0'+(idx+1) : idx+1 }}</span>
              <span class="opt-text">{{ typeof opt === 'object' ? (opt.title || opt.name) : opt }}</span>
            </div>
            <span v-if="typeof opt === 'object'" class="opt-tag">{{ opt.type || opt.rarity }}</span>
          </div>

          <div class="modal-option destructive" @click="clearSlot">
            <div class="opt-left">
              <span class="opt-idx">XX</span>
              <span class="opt-text">UNMOUNT_DEVICE // 卸载设备</span>
            </div>
          </div>
        </div>
        
        <div class="modal-footer">
          STATUS: WAITING_FOR_INPUT...
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, defineProps, defineEmits } from 'vue'

const props = defineProps({ form: { type: Object, required: true } })
const emit = defineEmits(['update:form'])

const selectorState = reactive({ visible: false, type: '', targetIndex: -1, title: '' })

// Mock Database (保持原有数据结构)
const database = {
  works: [ { title: 'SaaS Dashboard', type: 'PRO' }, { title: 'E-Commerce UI', type: 'UI' }, { title: 'Three.js Earth', type: '3D' }, { title: 'Cyber Blog', type: 'WEB' } ],
  articles: [ { title: 'React vs Vue', type: 'DOC' }, { title: 'JS Async/Await', type: 'JS' }, { title: 'Grid Layout', type: 'CSS' }, { title: 'Web Assembly', type: 'WASM' } ],
  achievements: [ 'Top Writer', 'Mentor', 'Early Bird', 'Night Owl', 'Full Stack', 'Designer' ],
  inventory: [ { name: 'RTX 4090', rarity: 'legendary' }, { name: 'Switch', rarity: 'epic' }, { name: 'Book', rarity: 'common' }, { name: 'Headset', rarity: 'rare' } ]
}

const currentOptions = computed(() => database[selectorState.type] || [])

const openSelector = (type, index) => {
  selectorState.type = type
  selectorState.targetIndex = index
  selectorState.title = `SELECT :: ${type.toUpperCase()} [SLOT_${index+1}]`
  selectorState.visible = true
}

const selectItem = (item) => {
  props.form[selectorState.type][selectorState.targetIndex] = item
  selectorState.visible = false
  emit('update:form', props.form)
}

const clearSlot = () => {
  props.form[selectorState.type][selectorState.targetIndex] = null
  selectorState.visible = false
  emit('update:form', props.form)
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&family=Rajdhani:wght@500;700&display=swap');

:root {
  --cy-black: #050505;
  --cy-dark-grey: #111111;
  --cy-panel: #1a1a1a;
  --cy-red: #ff2a2a;
  --cy-yellow: #ffae00;
  --cy-blue: #00f0ff;
  --cy-text: #e0e0e0;
  --cy-border: #444;
  --font-tech: 'Share Tech Mono', monospace;
  --font-ui: 'Rajdhani', sans-serif;
}

/* --- 容器整体 --- */
.cyber-rack-container {
  flex: 1;
  background-color: #080808;
  /* 碳纤维纹理背景 */
  background-image: 
    linear-gradient(rgba(0,0,0,0.8), rgba(0,0,0,0.8)),
    repeating-linear-gradient(45deg, #111 0, #111 1px, transparent 0, transparent 50%);
  background-size: auto, 10px 10px;
  padding: 20px;
  overflow-y: auto;
  color: var(--cy-text);
  font-family: var(--font-tech);
  border-left: 4px solid var(--cy-dark-grey);
}

/* --- 状态栏 --- */
.system-status-bar {
  display: flex; align-items: center; gap: 10px;
  margin-bottom: 25px; padding: 5px;
  border-bottom: 2px solid #00F200;
  color: white; font-size: 0.7rem;
}
.status-light { width: 8px; height: 8px; background: var(--cy-red); border-radius: 50%; box-shadow: 0 0 8px var(--cy-red); }
.status-text { font-weight: bold; letter-spacing: 1px; }
.deco-lines { flex: 1; height: 4px; background: repeating-linear-gradient(90deg, var(--cy-red), var(--cy-red) 2px, transparent 2px, transparent 6px); opacity: 0.5; }
.blink { animation: blink 2s infinite; }

/* --- 通用模块样式 --- */
.rack-module {
  background: var(--cy-panel);
  border: 1px solid var(--cy-border);
  margin-bottom: 30px;
  position: relative;
  box-shadow: 5px 5px 0 #000;
}
.rack-module::before {
  content: ''; position: absolute; top: -1px; left: -1px;
  width: 10px; height: 10px; border-top: 2px solid var(--cy-yellow); border-left: 2px solid var(--cy-yellow);
}
.rack-module::after {
  content: ''; position: absolute; bottom: -1px; right: -1px;
  width: 10px; height: 10px; border-bottom: 2px solid var(--cy-yellow); border-right: 2px solid var(--cy-yellow);
}

.module-header {
  background: #0f0f0f;
  border-bottom: 1px solid #333;
  padding: 10px 15px;
  display: flex; justify-content: space-between; align-items: center;
}
.header-left { display: flex; align-items: center; gap: 10px; }
.mod-idx { color: var(--cy-yellow); font-size: 1.2rem; font-weight: bold; font-family: var(--font-ui); opacity: 0.5; }
.mod-title { font-weight: bold; letter-spacing: 1px; color: #fff; font-size: 0.8rem; }

.module-body { padding: 20px; position: relative; }
.control-group { position: relative; }
.group-label { 
  display: flex; justify-content: space-between; 
  color: #666; font-size: 0.6rem; margin-bottom: 8px; 
  border-bottom: 1px dashed #333; padding-bottom: 2px;
}
.mt-20 { margin-top: 20px; }

/* --- 模块 1: 核心数据 (Works) --- */
.hazard-stripes {
  width: 40px; height: 10px;
  background: repeating-linear-gradient(45deg, #333, #333 5px, var(--cy-yellow) 5px, var(--cy-yellow) 10px);
}
.slot-grid-2x2 {
  display: grid; grid-template-columns: 1fr 1fr; gap: 10px;
}
.cyber-slot {
  aspect-ratio: 1;
  background: #111; border: 1px solid #333;
  position: relative; cursor: pointer; transition: 0.2s;
  overflow: hidden;
}
.cyber-slot:hover { border-color: var(--cy-yellow); box-shadow: 0 0 10px rgba(255, 174, 0, 0.1); }
.slot-frame {
  width: 100%; height: 100%;
  display: flex; flex-direction: column; justify-content: center; align-items: center;
  position: relative; z-index: 1;
}
.corner-screw {
  width: 4px; height: 4px; background: #555; border-radius: 50%;
  position: absolute; z-index: 2; box-shadow: inset 1px 1px 1px #000;
}
.top-left { top: 4px; left: 4px; }
.bottom-right { bottom: 4px; right: 4px; }

.empty-cross { font-size: 2rem; color: #333; font-weight: 100; transition: 0.2s; }
.empty-label { font-size: 0.6rem; color: #333; margin-top: 5px; }
.cyber-slot:hover .empty-cross { color: var(--cy-yellow); transform: rotate(90deg); }

.chip-type {
  position: absolute; top: 0; right: 0;
  background: var(--cy-red); color: #000;
  font-size: 0.55rem; padding: 2px 4px; font-weight: bold;
}
.work-info { text-align: center; padding: 5px; z-index: 2; }
.w-title { font-size: 0.7rem; color: #fff; line-height: 1.2; display: block; }
.holo-bg {
  position: absolute; inset: 0;
  background: radial-gradient(circle, rgba(255,42,42,0.1) 0%, transparent 70%);
  opacity: 0.5;
}
.scan-line {
  position: absolute; top: 0; left: 0; width: 100%; height: 2px;
  background: rgba(255, 255, 255, 0.3);
  animation: scan 2s linear infinite; pointer-events: none;
}
@keyframes scan { 0% {top:0; opacity:0;} 50% {opacity:1;} 100% {top:100%; opacity:0;} }

/* --- 模块 1: 文章列表 (Stack) --- */
.list-stack { display: flex; flex-direction: column; gap: 6px; }
.data-strip {
  background: #0d0d0d; border-left: 3px solid #333;
  padding: 8px; display: flex; align-items: center; justify-content: space-between;
  cursor: pointer; transition: 0.2s;
}
.data-strip:hover { background: #1a1a1a; border-left-color: var(--cy-blue); padding-left: 12px; }
.has-data { border-left-color: var(--cy-text); }
.strip-indicator { width: 4px; height: 4px; background: #333; margin-right: 8px; }
.has-data .strip-indicator { background: var(--cy-blue); box-shadow: 0 0 5px var(--cy-blue); }
.strip-content { display: flex; align-items: center; gap: 8px; flex: 1; overflow: hidden; }
.file-name { font-size: 0.7rem; color: #eee; white-space: nowrap; }
.file-ext { font-size: 0.6rem; color: #666; }
.empty-text { font-size: 0.6rem; color: #333; letter-spacing: 2px; }

/* --- 模块 2: 荣誉矩阵 (Hexagons) --- */
.honeycomb-bg {
  background-image: radial-gradient(#222 15%, transparent 16%), radial-gradient(#222 15%, transparent 16%);
  background-size: 20px 20px; background-position: 0 0, 10px 10px;
}
.hex-grid {
  display: flex; flex-wrap: wrap; gap: 8px; justify-content: center;
}
.hex-wrapper {
  width: 50px; height: 55px; position: relative; cursor: pointer;
  filter: drop-shadow(0 0 2px #000);
}
.hex-cell {
  width: 100%; height: 100%;
  background: #111;
  clip-path: polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%);
  display: flex; align-items: center; justify-content: center;
  transition: 0.2s;
}
.hex-border {
  position: absolute; inset: 0; 
  background: #333; z-index: -1;
  clip-path: polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%);
  transform: scale(1.05);
}
.hex-cell:hover .hex-border { background: var(--cy-yellow); }
.hex-cell.unlocked { background: #222; }
.hex-cell.unlocked .hex-border { background: var(--cy-red); }
.hex-cell.unlocked .badge-text { color: var(--cy-red); font-weight: bold; font-size: 0.6rem; text-shadow: 0 0 5px var(--cy-red); }
.lock-icon { font-size: 0.8rem; opacity: 0.2; }

/* --- 模块 3: 装备库 (Inventory) --- */
.weapon-case { border-color: #444; }
.capacity-bar { width: 60px; height: 6px; background: #000; border: 1px solid #444; padding: 1px; }
.capacity-bar .fill { height: 100%; background: var(--cy-blue); }

.inv-grid {
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 6px;
}
.inv-cell {
  aspect-ratio: 1;
  background: #0d0d0d;
  border: 1px solid #222;
  position: relative; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
}
.inv-cell:hover { border-color: #666; z-index: 10; transform: scale(1.1); background: #151515; }

/* 稀有度边框光效 */
.inv-cell.legendary { border-color: #ffaa00; box-shadow: inset 0 0 10px rgba(255,170,0,0.2); }
.inv-cell.epic { border-color: #aa00ff; box-shadow: inset 0 0 10px rgba(170,0,255,0.2); }
.inv-cell.rare { border-color: #0088ff; box-shadow: inset 0 0 10px rgba(0,136,255,0.2); }
.inv-cell.common { border-color: #888; }

.cell-content { position: relative; z-index: 2; width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; padding: 2px; }
.item-name { font-size: 0.5rem; text-align: center; line-height: 1; color: #ddd; word-break: break-all; }
.crosshair {
  width: 10px; height: 10px; border: 1px solid #333;
  position: relative;
}
.crosshair::after, .crosshair::before {
  content: ''; position: absolute; background: #333;
}
.crosshair::after { top: 4px; left: -2px; width: 14px; height: 1px; }
.crosshair::before { left: 4px; top: -2px; height: 14px; width: 1px; }

/* 装饰 Bracket */
.bracket { position: absolute; width: 4px; height: 4px; border-color: #555; border-style: solid; transition: 0.2s; }
.bracket.tl { top: 1px; left: 1px; border-width: 1px 0 0 1px; }
.bracket.br { bottom: 1px; right: 1px; border-width: 0 1px 1px 0; }
.inv-cell:hover .bracket { border-color: #fff; width: 8px; height: 8px; }

.case-footer { 
  margin-top: 10px; font-size: 0.55rem; color: #444; 
  display: flex; justify-content: space-between; border-top: 1px solid #222; padding-top: 5px; 
}

/* --- 底部装饰 --- */
.rack-footer { margin-top: 20px; text-align: center; }
.vent-grill { display: flex; justify-content: center; gap: 4px; margin-bottom: 5px; }
.vent-grill span { width: 30px; height: 4px; background: #222; border-radius: 2px; }
.warning-label { color: #555; font-size: 0.6rem; border: 1px solid #333; display: inline-block; padding: 2px 10px; background: repeating-linear-gradient(45deg, #111, #111 5px, #222 5px, #222 10px); }

/* --- 弹窗样式 --- */
.modal-backdrop {
  position: fixed; inset: 0; background: rgba(0,0,0,0.85);
  backdrop-filter: blur(5px); z-index: 999;
  display: flex; align-items: center; justify-content: center;
}
.cyber-modal {
  width: 350px; background: #000; border: 2px solid var(--cy-red);
  box-shadow: 0 0 20px rgba(255, 42, 42, 0.2);
  display: flex; flex-direction: column;
}
.modal-header {
  background: var(--cy-red); color: #000; padding: 10px;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-title-box { font-weight: bold; font-family: var(--font-ui); }
.close-btn {
  background: #000; color: var(--cy-red); border: none;
  font-family: var(--font-tech); cursor: pointer; font-weight: bold;
}
.close-btn:hover { background: #fff; }

.modal-body {
  height: 300px; padding: 15px; position: relative; background: #0a0a0a;
}
.scan-grid-bg {
  position: absolute; inset: 0; opacity: 0.1; pointer-events: none;
  background-image: linear-gradient(#333 1px, transparent 1px), linear-gradient(90deg, #333 1px, transparent 1px);
  background-size: 20px 20px;
}

.modal-option {
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px; border-bottom: 1px solid #222; cursor: pointer;
  position: relative; z-index: 2; transition: 0.2s;
}
.modal-option:hover { background: #151515; border-left: 4px solid var(--cy-red); }
.opt-idx { color: #444; margin-right: 10px; font-size: 0.8rem; }
.opt-text { color: #ccc; font-size: 0.9rem; }
.opt-tag { font-size: 0.6rem; border: 1px solid #444; padding: 2px 6px; color: #888; }
.modal-option.destructive .opt-text { color: var(--cy-red); }

.modal-footer {
  background: #111; color: #555; padding: 5px 10px; font-size: 0.6rem;
  border-top: 1px solid #333; text-align: right;
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-track { background: #000; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--cy-red); }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }
</style>