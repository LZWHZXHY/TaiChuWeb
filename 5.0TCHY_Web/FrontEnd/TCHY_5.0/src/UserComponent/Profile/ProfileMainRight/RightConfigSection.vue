<template>
  <div class="content-col right-col custom-scroll">
    <div class="config-module"></div>

    <div class="config-module">
      <div class="module-header">
        <span class="mod-id">MOD_A1</span>
        <span class="mod-name">PINNED_CONTENT // 置顶管理</span>
        <div class="mod-decor"></div>
      </div>
      <div class="module-body">
        <div class="sub-group">
          <label>FEATURED_WORK // 代表作 [4_SLOTS]</label>
          <div class="grid-row-4">
            <div
              v-for="(item, index) in form.works"
              :key="'w'+index"
              class="square-slot"
              :class="{ empty: !item }"
              @click="openSelector('works', index)"
            >
              <template v-if="item">
                <span class="slot-type">{{ item.type }}</span>
                <span class="slot-name">{{ item.title }}</span>
              </template>
              <span v-else class="add-sign">+</span>
            </div>
          </div>
        </div>

        <div class="sub-group">
          <label>TOP_ARTICLES // 置顶文章 [4_SLOTS]</label>
          <div class="grid-row-4">
            <div
              v-for="(item, index) in form.articles"
              :key="'a'+index"
              class="square-slot article-slot"
              :class="{ empty: !item }"
              @click="openSelector('articles', index)"
            >
              <template v-if="item">
                <div class="doc-icon">📄</div>
                <span class="slot-name tiny">{{ item.title }}</span>
              </template>
              <span v-else class="add-sign">+</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="config-module">
      <div class="module-header">
        <span class="mod-id">MOD_A2</span>
        <span class="mod-name">ACHIEVEMENTS // 成就矩阵</span>
        <div class="mod-decor"></div>
      </div>
      <div class="module-body">
        <div class="grid-row-4 wrap-grid">
          <div
            v-for="(badge, index) in form.achievements"
            :key="'ach'+index"
            class="hex-slot"
            :class="{ active: badge, empty: !badge }"
            @click="openSelector('achievements', index)"
          >
            <span v-if="badge">{{ badge }}</span>
            <span v-else class="tiny-idx">{{ index + 1 }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="config-module">
      <div class="module-header">
        <span class="mod-id">MOD_A3</span>
        <span class="mod-name">INVENTORY // 展示柜</span>
        <div class="mod-decor"></div>
      </div>
      <div class="module-body inventory-body">
        <div class="inventory-grid">
          <div
            v-for="(item, index) in form.inventory"
            :key="'inv'+index"
            class="inv-slot"
            :class="[item ? item.rarity : 'empty']"
            @click="openSelector('inventory', index)"
          >
            <div class="slot-inner">
              <template v-if="item">
                <span class="rarity-dot"></span>
                <span class="inv-name">{{ item.name }}</span>
              </template>
              <span v-else class="pattern-bg"></span>
            </div>
            <div class="slot-corner"></div>
          </div>
        </div>
        <div class="inventory-footer">
          <span>CAPACITY: 8/64</span>
          <span>SORT: DATE</span>
        </div>
      </div>
    </div>

    <div class="end-marker">/// END OF CUSTOM_CONFIG ///</div>

    <!-- 通用选择器弹窗 -->
    <div v-if="selectorState.visible" class="modal-overlay" @click.self="selectorState.visible = false">
      <div class="selector-window">
        <div class="sel-header">
          <span>{{ selectorState.title }}</span>
          <button class="close-x" @click="selectorState.visible = false">×</button>
        </div>
        <div class="sel-body custom-scroll">
          <div
            v-for="(opt, idx) in currentOptions"
            :key="idx"
            class="sel-option"
            @click="selectItem(opt)"
          >
            <span class="opt-bullet">>></span>
            <span class="opt-text">{{ typeof opt === 'object' ? (opt.title || opt.name) : opt }}</span>
            <span v-if="typeof opt === 'object'" class="opt-tag">{{ opt.type || opt.rarity }}</span>
          </div>
          <div class="sel-option destructive" @click="clearSlot">
            <span class="opt-bullet">X</span>
            <span class="opt-text">CLEAR_SLOT // 清空该槽位</span>
          </div>
        </div>
        <div class="sel-footer">
          DATABASE_CONNECTED // RESULTS: {{ currentOptions.length }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, defineProps, defineEmits } from 'vue'

// 接收父组件传递的form数据
const props = defineProps({
  form: {
    type: Object,
    required: true
  }
})

// 定义更新form的事件
const emit = defineEmits(['update:form'])

// --- 通用选择器逻辑 ---
const selectorState = reactive({
  visible: false,
  type: '',
  targetIndex: -1,
  title: ''
})

// 模拟数据库中的备选数据
const database = {
  works: [
    { title: 'SaaS Dashboard', type: 'PRO' },
    { title: 'E-Commerce UI', type: 'UI' },
    { title: 'Three.js Earth', type: '3D' },
    { title: 'Cyber Blog', type: 'WEB' }
  ],
  articles: [
    { title: 'React vs Vue', type: 'DOC' },
    { title: 'JS Async/Await', type: 'JS' },
    { title: 'Grid Layout', type: 'CSS' },
    { title: 'Web Assembly', type: 'WASM' }
  ],
  achievements: [
    'Top Writer', 'Mentor', 'Early Bird', 'Night Owl', 'Full Stack', 'Designer'
  ],
  inventory: [
    { name: 'RTX 4090', rarity: 'legendary' },
    { name: 'Switch', rarity: 'epic' },
    { name: 'Book', rarity: 'common' },
    { name: 'Headset', rarity: 'rare' }
  ]
}

// 计算当前弹窗显示的选项
const currentOptions = computed(() => {
  return database[selectorState.type] || []
})

// 打开选择器
const openSelector = (type, index) => {
  selectorState.type = type
  selectorState.targetIndex = index
  selectorState.title = `SELECT_${type.toUpperCase()} // SLOT_${index < 9 ? '0'+(index+1) : index+1}`
  selectorState.visible = true
}

// 选中数据
const selectItem = (item) => {
  props.form[selectorState.type][selectorState.targetIndex] = item
  selectorState.visible = false
  emit('update:form', props.form)
}

// 清空槽位
const clearSlot = () => {
  props.form[selectorState.type][selectorState.targetIndex] = null
  selectorState.visible = false
  emit('update:form', props.form)
}
</script>

<style scoped>
/* 变量复用 */
:root {
  --red: #D92323;
  --black: #111111;
  --dark-grey: #1a1a1a;
  --light-grey: #f4f4f4;
  --text-main: #111;
  --text-inv: #eee;
  --mono: 'JetBrains Mono', monospace;
  --sans: 'Noto Sans SC', sans-serif;
}

/* 右侧列基础样式 */
.content-col.right-col { 
  flex: 1; 
  background: #F4F1EA; 
  padding: 40px 30px; 
  overflow-y: auto;
}

/* 模块通用样式 */
.config-module { background: #fff; border: 1px solid var(--black); margin-bottom: 30px; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.module-header { background: black; color: #fff; padding: 8px 15px; display: flex; align-items: center; gap: 10px; font-family: var(--mono); font-size: 0.8rem; border-bottom: 1px solid var(--black); }
.mod-id { color: var(--red); font-weight: bold; }
.mod-decor { flex: 1; height: 2px; background: repeating-linear-gradient(90deg, #444, #444 2px, transparent 2px, transparent 4px); }
.module-body { padding: 20px; }
.sub-group { padding-top: 10px; padding-bottom: 10px; }
.end-marker { text-align: center; font-family: var(--mono); color: #bbb; margin-top: 40px; font-size: 0.7rem; letter-spacing: 2px; }

/* Grid布局 */
.grid-row-4 {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
  width: 100%;
}
.wrap-grid { grid-template-rows: repeat(2, 1fr); }

/* 作品/文章槽位 */
.square-slot {
  aspect-ratio: 1;
  background: #fff;
  border: 1px solid #333;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
  padding: 5px;
  text-align: center;
}
.square-slot:hover {
  background: #eee;
  border-color: var(--red);
  transform: translateY(-2px);
  box-shadow: 2px 2px 0 rgba(0,0,0,0.2);
}
.square-slot.empty {
  border: 1px dashed #999;
  background: #f9f9f9;
  color: #ccc;
}
.square-slot.empty:hover { border-color: #666; color: #666; }
.add-sign { font-size: 1.5rem; font-weight: bold; }
.slot-type { 
  font-size: 0.6rem; background: var(--black); color: #fff; 
  padding: 1px 4px; position: absolute; top: 5px; right: 5px; 
}
.slot-name { 
  font-size: 0.75rem; font-weight: bold; line-height: 1.2; word-break: break-all;
}
.slot-name.tiny { font-size: 0.65rem; color: #555; }
.doc-icon { font-size: 1.2rem; margin-bottom: 5px; }

/* 成就六边形槽位 */
.hex-slot {
  aspect-ratio: 1;
  border: 1px solid #444;
  background: #2a2a2a;
  color: #666;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.65rem;
  cursor: pointer;
  text-align: center;
  clip-path: polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%);
  transition: 0.2s;
}
.hex-slot:hover { border-color: #fff; color: #fff; }
.hex-slot.active {
  background: var(--black);
  border: 1px solid var(--red);
  color: var(--red);
  font-weight: bold;
  box-shadow: inset 0 0 10px rgba(217, 35, 35, 0.2);
}
.tiny-idx { font-family: var(--mono); font-size: 0.6rem; opacity: 0.3; }

/* 展示柜样式 */
.inventory-body {
  background: #1a1a1a;
  padding: 15px;
}
.inventory-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-auto-rows: 1fr;
  gap: 8px;
}
.inv-slot {
  aspect-ratio: 1;
  background: #252525;
  border: 1px solid #444;
  position: relative;
  cursor: pointer;
  transition: 0.2s;
}
.inv-slot:hover { z-index: 2; transform: scale(1.05); border-color: #888; background: #333; }
.slot-inner {
  width: 100%; height: 100%;
  display: flex; flex-direction: column;
  justify-content: center; align-items: center;
  padding: 4px;
}
/* 稀有度侧边条 */
.inv-slot::before {
  content: ''; position: absolute; left: 0; bottom: 0; 
  width: 3px; height: 100%; background: #333;
}
.inv-slot.legendary::before { background: #ffaa00; box-shadow: 0 0 5px #ffaa00; }
.inv-slot.epic::before { background: #aa00ff; }
.inv-slot.rare::before { background: #0088ff; }
.inv-slot.common::before { background: #888; }
.inv-slot.empty::before { display: none; }
/* 空槽位纹理 */
.pattern-bg {
  width: 100%; height: 100%; opacity: 0.1;
  background-image: repeating-linear-gradient(45deg, #ccc 0, #ccc 1px, transparent 0, transparent 50%);
  background-size: 10px 10px;
}
.inv-name { color: #eee; font-size: 0.6rem; text-align: center; font-family: var(--mono); }
.rarity-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; margin-bottom: 5px; opacity: 0.8; }
.slot-corner {
  position: absolute; top: 0; right: 0; 
  width: 0; height: 0; 
  border-style: solid; border-width: 0 10px 10px 0; 
  border-color: transparent #333 transparent transparent;
}
.inventory-footer {
  margin-top: 10px;
  border-top: 1px dashed #444;
  padding-top: 5px;
  display: flex; justify-content: space-between;
  color: #666; font-family: var(--mono); font-size: 0.6rem;
}

/* 选择器弹窗 */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.6); backdrop-filter: blur(2px);
  z-index: 200; display: flex; align-items: center; justify-content: center;
}
.selector-window {
  width: 300px; max-height: 400px;
  background: #f4f4f4; border: 2px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.5);
  display: flex; flex-direction: column;
}
.sel-header {
  background: var(--black); color: #fff; padding: 8px 10px;
  font-family: var(--mono); font-size: 0.75rem;
  display: flex; justify-content: space-between; align-items: center;
}
.close-x { background: none; border: none; color: #fff; font-size: 1.2rem; cursor: pointer; }
.sel-body { padding: 10px; overflow-y: auto; flex: 1; }
.sel-option {
  padding: 8px; border-bottom: 1px solid #ddd; cursor: pointer;
  display: flex; align-items: center; font-family: var(--mono); font-size: 0.8rem;
  transition: 0.2s;
}
.sel-option:hover { background: #ddd; padding-left: 12px; }
.opt-bullet { color: var(--red); margin-right: 8px; font-weight: bold; }
.opt-text { flex: 1; color: #333; }
.opt-tag { font-size: 0.6rem; background: #333; color: #fff; padding: 2px 4px; border-radius: 2px; }
.destructive .opt-text { color: var(--red); }
.sel-footer {
  background: #e5e5e5; padding: 5px 10px; font-size: 0.6rem; color: #666;
  border-top: 1px solid #ccc; font-family: var(--mono);
}

/* 滚动条样式 */
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #413e3a; }
</style>