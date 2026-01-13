<template>
  <div class="rules-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="rules-container">
      <header class="page-header">
        <div class="header-main">
          <h1 class="giant-text">
            <span class="prefix">///</span> RULES_PROTOCOL
          </h1>
          <div class="sub-text">社区律法 // 行为准则数据库</div>
        </div>
        <div class="header-meta">
          <div class="status-badge">SYSTEM: ACTIVE</div>
          <div class="version-tag">VER: 5.0</div>
        </div>
      </header>

      <div class="search-section">
        <div class="search-box">
          <span class="search-icon">>></span>
          <input 
            v-model="search" 
            placeholder="INPUT_QUERY..." 
            class="cyber-input"
            @input="filterTree"
          />
          <button class="search-btn" @click="filterTree">SEARCH</button>
        </div>
      </div>

      <div class="tree-wrapper custom-scroll">
        <el-tree
          ref="treeRef"
          class="industrial-tree"
          node-key="Id"
          :data="rules"
          :props="treeProps"
          :expand-on-click-node="false"
          :default-expand-all="true"
          :filter-node-method="filterNode"
          :indent="0"
        >
          <template #default="{ data, node }">
            <div v-if="node.level === 1" class="rule-root-block">
              <div class="root-label">SECTION_{{ padZero(data.OrderNum) }}</div>
              <h2 class="root-title">{{ data.Title }}</h2>
              <div class="root-deco-line"></div>
            </div>

            <div v-else class="rule-leaf-card" :style="{ marginLeft: (node.level - 1) * 20 + 'px' }">
              <div class="connection-line"></div>

              <div class="card-inner">
                <div class="card-header">
                  <span class="rule-id-tag" v-if="data.RuleNumber">NO.{{ data.RuleNumber }}</span>
                  <span class="rule-title">{{ data.Title }}</span>
                  <span class="version-badge" v-if="data.Version">v{{ data.Version }}</span>
                </div>

                <div class="card-content-box" v-if="data.Content">
                  {{ data.Content }}
                </div>

                <div class="penalty-strip" v-if="data.Penalty">
                  <span class="icon">⚠</span>
                  <span class="label">VIOLATION_PENALTY:</span>
                  <span class="value">{{ data.Penalty }}</span>
                </div>

                <div class="card-footer">
                  <span>// UPDATE: {{ formatDate(data.UpdateAt || data.CreateAt) }}</span>
                </div>
              </div>
            </div>
          </template>
        </el-tree>
        
        <div v-if="rules.length === 0" class="empty-state">
          [ DATA_NOT_FOUND ]
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'

const rules = ref([])
const search = ref('')
const treeRef = ref(null)

const treeProps = { children: 'Children', label: 'Title' }

function padZero(num) {
  return num < 10 ? `0${num}` : num
}

function formatDate(val) {
  if (!val) return 'N/A'
  try { return new Date(val).toLocaleDateString() } catch { return val }
}

function filterNode(value, data) {
  if (!value) return true
  const v = value.trim()
  return (
    (data.Title && data.Title.includes(v)) ||
    (data.Content && data.Content.includes(v)) ||
    (data.RuleNumber && data.RuleNumber.includes(v)) ||
    (data.Penalty && data.Penalty.includes(v))
  )
}

function filterTree() {
  treeRef.value && treeRef.value.filter(search.value)
}

function filterEnabled(arr) {
  if (!Array.isArray(arr)) return []
  return arr
    .filter(item => item.Enabled !== false)
    .map(item => ({
      ...item,
      Children: filterEnabled(item.Children || [])
    }))
}

function fixChildren(arr) {
  if (!Array.isArray(arr)) return []
  arr.forEach(item => {
    if (!Array.isArray(item.Children)) item.Children = []
    fixChildren(item.Children)
  })
  return arr
}

onMounted(async () => {
  try {
    const res = await apiClient.get('/rules/tree')
    const origin = fixChildren(Array.isArray(res.data?.data) ? res.data.data : [])
    rules.value = filterEnabled(origin)
  } catch (error) {
    console.error('Failed to load rules:', error)
  }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 (与 ComCenter 一致) --- */
.rules-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  min-height: 100vh; /* 确保填满 */
  position: relative;
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* --- 背景网格 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* --- 主容器 --- */
.rules-container {
  position: relative;
  z-index: 1;
  max-width: 1000px;
  margin: 0 auto;
  width: 100%;
  padding: 40px 20px;
  display: flex;
  flex-direction: column;
  height: 100vh;
}

/* --- 头部 --- */
.page-header {
  border-bottom: 4px solid var(--black);
  padding-bottom: 20px;
  margin-bottom: 30px;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}
.giant-text {
  font-family: var(--heading);
  font-size: 3.5rem;
  margin: 0;
  line-height: 0.9;
  letter-spacing: -1px;
}
.prefix { color: var(--red); margin-right: 10px; }
.sub-text {
  font-family: var(--mono);
  font-weight: bold;
  margin-top: 5px;
  color: #666;
}
.header-meta { text-align: right; font-size: 0.8rem; font-weight: bold; }
.status-badge { background: var(--black); color: var(--white); padding: 2px 6px; display: inline-block; margin-bottom: 4px; }
.version-tag { color: var(--red); }

/* --- 搜索框 --- */
.search-section { margin-bottom: 30px; }
.search-box {
  display: flex;
  border: 2px solid var(--black);
  background: #fff;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
  height: 50px;
}
.search-icon {
  background: var(--black);
  color: var(--white);
  width: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}
.cyber-input {
  flex: 1;
  border: none;
  outline: none;
  padding: 0 15px;
  font-family: var(--mono);
  font-size: 1rem;
  background: transparent;
}
.search-btn {
  background: var(--white);
  border: none;
  border-left: 2px solid var(--black);
  font-family: var(--heading);
  font-size: 1.2rem;
  padding: 0 20px;
  cursor: pointer;
  transition: all 0.2s;
}
.search-btn:hover { background: var(--red); color: var(--white); }

/* --- 树形列表容器 --- */
.tree-wrapper {
  flex: 1;
  overflow-y: auto;
  padding-bottom: 50px;
}

/* 覆盖 Element UI Tree 的默认样式 */
.industrial-tree {
  background: transparent;
}
:deep(.el-tree-node__content) {
  height: auto !important;
  background: transparent !important;
  align-items: flex-start !important;
  padding: 0 !important;
  margin-bottom: 15px;
  cursor: default;
}
:deep(.el-tree-node__expand-icon) { display: none; } /* 隐藏展开箭头，默认全展开 */

/* --- 根节点 (章节) 样式 --- */
.rule-root-block {
  width: 100%;
  margin-top: 30px;
  margin-bottom: 20px;
}
.root-label {
  background: var(--black);
  color: var(--white);
  display: inline-block;
  padding: 4px 12px;
  font-size: 0.8rem;
  font-weight: bold;
}
.root-title {
  font-family: var(--heading);
  font-size: 2rem;
  margin: 10px 0;
  text-transform: uppercase;
}
.root-deco-line {
  height: 4px;
  background: repeating-linear-gradient(45deg, var(--black), var(--black) 10px, transparent 10px, transparent 20px);
}

/* --- 子节点 (规则卡片) 样式 --- */
.rule-leaf-card {
  width: 100%;
  position: relative;
  padding-left: 20px; /* 给连接线留位置 */
  margin-bottom: 10px;
}

/* 左侧连接线 */
.connection-line {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #ccc;
}
.connection-line::before {
  content: '';
  position: absolute;
  left: -4px;
  top: 15px;
  width: 10px;
  height: 10px;
  background: var(--white);
  border: 2px solid var(--black);
}

.card-inner {
  border: 2px solid var(--black);
  background: #fff;
  padding: 15px;
  transition: transform 0.2s;
  /* 硬阴影 */
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}
.card-inner:hover {
  transform: translate(-2px, -2px);
  box-shadow: 6px 6px 0 var(--red);
  border-color: var(--black);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
  border-bottom: 1px dashed #ccc;
  padding-bottom: 8px;
}
.rule-id-tag {
  background: var(--black);
  color: var(--white);
  font-size: 0.7rem;
  padding: 2px 6px;
  font-weight: bold;
}
.rule-title {
  font-weight: 700;
  font-size: 1.1rem;
}
.version-badge {
  font-size: 0.7rem;
  border: 1px solid #ccc;
  padding: 0 4px;
  color: #888;
}

.card-content-box {
  font-size: 0.95rem;
  line-height: 1.6;
  color: #333;
  white-space: pre-wrap;
  margin-bottom: 15px;
}

/* 惩罚条 */
.penalty-strip {
  background: rgba(217, 35, 35, 0.05);
  border: 1px solid var(--red);
  color: var(--red);
  padding: 8px 12px;
  font-size: 0.85rem;
  display: flex;
  align-items: flex-start;
  gap: 8px;
}
.penalty-strip .icon { font-weight: bold; }
.penalty-strip .label { font-weight: 800; }
.penalty-strip .value { font-weight: 500; }

.card-footer {
  margin-top: 10px;
  text-align: right;
  font-size: 0.7rem;
  color: #999;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 40px;
  color: #999;
  border: 2px dashed #ccc;
  margin-top: 20px;
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }

/* 响应式 */
@media (max-width: 768px) {
  .giant-text { font-size: 2.5rem; }
  .rules-container { padding: 20px 10px; }
  .card-inner { padding: 10px; }
  .rule-leaf-card { padding-left: 10px; margin-left: 0 !important; }
  .connection-line { display: none; }
}
</style>