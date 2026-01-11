<template>
  <div class="rules-page">
    <div class="title">社区规则</div>
    <el-input
      v-model="search"
      placeholder="搜索规则内容/编号/惩罚..."
      clearable
      prefix-icon="el-icon-search"
      class="rule-search"
      @input="filterTree"
    />
    <el-tree
      ref="treeRef"
      class="rules-tree"
      node-key="Id"
      :data="rules"
      :props="treeProps"
      :expand-on-click-node="false"
      :default-expand-all="true"
      :filter-node-method="filterNode"
    >
      <template #default="{ data }">
        <div class="rule-item">
          <div class="rule-header">
            <span class="rule-title">{{ data.Title }}</span>
            <span v-if="data.RuleNumber" class="rule-number">〔{{ data.RuleNumber }}〕</span>
            <span v-if="data.Version" class="rule-version">v{{ data.Version }}</span>
          </div>
          <div class="rule-meta">
            <span v-if="data.Penalty" class="rule-penalty">处罚：{{ data.Penalty }}</span>
            <span>排序：{{ data.OrderNum }}</span>
            <span>创建时间：{{ formatDate(data.CreateAt) }}</span>
          </div>
          <div class="rule-content">{{ data.Content }}</div>
        </div>
      </template>
    </el-tree>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'

const rules = ref([])
const search = ref('')
const treeRef = ref(null)

// 修改点 1：配置树形结构对应的字段名（必须大写）
const treeProps = { children: 'Children', label: 'Title' }

function formatDate(val) {
  if (!val) return ''
  try { return new Date(val).toLocaleString() } catch { return val }
}

function filterNode(value, data) {
  if (!value) return true
  const v = value.trim()
  // 修改点 2：过滤逻辑中的字段名改为大写
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

// 递归过滤掉 Enabled=false 的规则及其所有后代
function filterEnabled(arr) {
  if (!Array.isArray(arr)) return []
  return arr
    // 修改点 3：属性名改为大写 Enabled, Children
    .filter(item => item.Enabled !== false)
    .map(item => ({
      ...item,
      Children: filterEnabled(item.Children || [])
    }))
}

// 保证 Children 字段始终是数组
function fixChildren(arr) {
  if (!Array.isArray(arr)) return []
  arr.forEach(item => {
    // 修改点 4：属性名改为大写 Children
    if (!Array.isArray(item.Children)) item.Children = []
    fixChildren(item.Children)
  })
  return arr
}

onMounted(async () => {
  try {
    const res = await apiClient.get('/rules/tree')
    // 注意：res.data.data 是你的 API 响应结构，通常 axios 这一层还是小写的 data
    const origin = fixChildren(Array.isArray(res.data?.data) ? res.data.data : [])
    rules.value = filterEnabled(origin)
  } catch (error) {
    console.error('Failed to load rules:', error)
  }
})
</script>

<style scoped>
.rules-page {
  --primary-color: #7a87d7;
  --primary-light: #b6c8f2;
  --primary-lighter: #d5e4fa;
  --primary-glass: rgba(169, 183, 228, 0.12);
  --danger-color: #f7898c;
  --danger-light: #e9636c;
  --danger-glass: rgba(251, 207, 210, 0.12);
  --warning-color: #fcd38c;
  --success-color: #b9f2ca;
  --info-color: #99cdfa;
  --text-primary: #505a7f;
  --text-secondary: #6678a4;
  --text-tertiary: #90a4c2;
  --text-muted: #bfcde5;
  --text-light: #dee6f5;
  --bg-blur: blur(18px);
  --glass-bg: rgba(240, 244, 250, 0.88);
  --glass-border: rgba(210, 220, 255, 0.19);
  --glass-shadow: 0 8px 32px rgba(150, 180, 255, 0.08);
  --glass-hover: rgba(247, 250, 255, 0.96);
  --content-bg: rgba(242, 246, 255, 0.6);
  --hover-bg: rgba(169, 183, 228, 0.22);
  --bg-gradient-start: #e7ecf9;
  --bg-gradient-mid: #f3f6fc;
  --bg-gradient-end: #fbfcff;
  --spacing-xs: 4px;
  --spacing-sm: 8px;
  --spacing-md: 12px;
  --spacing-lg: 16px;
  --spacing-xl: 20px;
  --spacing-xxl: 24px;
  --spacing-xxxl: 32px;
  --spacing-xxxxl: 40px;
  --spacing-page: 60px;
  --font-size-xs: 0.875rem;
  --font-size-sm: 0.9375rem;
  --font-size-md: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.5rem;
  --font-size-xxl: 2rem;
  --border-radius-sm: 8px;
  --border-radius-md: 12px;
  --border-radius-lg: 20px;
  --border-radius-pill: 999px;
  --border-radius-blur: 16px;
  --max-width-xs: 480px;
  --max-width-md: 860px;
  --max-width-mobile: 92vw;
  --tree-indent: 28px;
  --transition-fast: 0.2s ease;
  --transition-normal: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  --transition-slow: 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  padding: var(--spacing-xxxxl) 0 var(--spacing-page) 0;
  min-height: 100vh;
  position: relative;
  overflow-x: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', sans-serif;
  backdrop-filter: var(--bg-blur);
  -webkit-backdrop-filter: var(--bg-blur);
  background: transparent !important;
  border-radius: 15px;
}
.rules-page::before {
  content: '';
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: 
    radial-gradient(circle at 22% 78%, rgba(169, 183, 228, 0.08) 0%, transparent 60%),
    radial-gradient(circle at 85% 8%, rgba(230, 235, 255, 0.18) 0%, transparent 60%),
    linear-gradient(135deg, var(--bg-gradient-start) 0%, var(--bg-gradient-mid) 55%, var(--bg-gradient-end) 100%);
  z-index: -1;
  pointer-events: none;
}
.rules-page::after {
  content: '';
  position: fixed;
  width: 410px;
  height: 410px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(170,200,240,0.07) 0%, rgba(190,218,255,0.04) 60%, rgba(255,255,255,0.03) 100%);
  top: -110px;
  right: -110px;
  z-index: -1;
  pointer-events: none;
}
.title {
  font-size: var(--font-size-xxl);
  color: var(--primary-color);
  font-weight: 700;
  margin: 0 auto var(--spacing-xxxxl) auto;
  text-align: center;
  letter-spacing: 1px;
  position: relative;
  display: inline-block;
  left: 50%;
  transform: translateX(-50%);
  padding: var(--spacing-lg) var(--spacing-xxxl);
  backdrop-filter: var(--bg-blur);
  -webkit-backdrop-filter: var(--bg-blur);
  background: var(--glass-bg);
  border: 1px solid var(--glass-border);
  border-radius: var(--border-radius-lg);
  box-shadow: 
    0 4px 30px rgba(170,200,240,0.11),
    inset 0 1px 0 rgba(255, 255, 255, 0.6);
  z-index: 1;
  transition: all var(--transition-normal);
}
.title:hover {
  background: var(--glass-hover);
  box-shadow: 
    0 8px 40px rgba(170,200,240,0.15),
    inset 0 1px 0 rgba(255,255,255,0.8);
  transform: translateX(-50%) translateY(-2px);
}
.title::after {
  content: '';
  position: absolute;
  bottom: -4px;
  left: 50%;
  transform: translateX(-50%);
  width: 52px;
  height: 3px;
  background: linear-gradient(90deg, transparent, var(--primary-light), transparent);
  border-radius: var(--border-radius-pill);
  opacity: 0.7;
}
.rule-search {
  max-width: var(--max-width-xs);
  margin: 0 auto var(--spacing-xxxl) auto;
  display: block;
  border-radius: var(--border-radius-pill);
  overflow: hidden;
  transition: all var(--transition-normal);
  backdrop-filter: var(--bg-blur);
  -webkit-backdrop-filter: var(--bg-blur);
  background: var(--glass-bg) !important;
  border: 1px solid var(--glass-border) !important;
  box-shadow: 
    0 4px 20px rgba(170,200,240,0.08),
    inset 0 1px 0 rgba(255,255,255,0.6) !important;
}
.rule-search:hover,
.rule-search:focus-within {
  background: var(--glass-hover) !important;
  box-shadow: 
    0 8px 30px rgba(170,200,240,0.12),
    inset 0 1px 0 rgba(255,255,255,0.8) !important;
  transform: translateY(-1px);
  border-color: rgba(169, 183, 228, 0.23) !important;
}
.rules-tree {
  max-width: var(--max-width-md);
  margin: 0 auto;
  backdrop-filter: var(--bg-blur);
  -webkit-backdrop-filter: var(--bg-blur);
  background: var(--glass-bg);
  border: 1px solid var(--glass-border);
  border-radius: var(--border-radius-blur);
  box-shadow: var(--glass-shadow);
  padding: var(--spacing-lg) 0;
  transition: all var(--transition-normal);
  position: relative;
  overflow: hidden;
}
.rules-tree::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, 
    transparent, 
    rgba(255,255,255,0.7), 
    transparent);
  z-index: 1;
}
.rules-tree:hover {
  box-shadow: 
    0 12px 40px rgba(170,200,240,0.13),
    inset 0 1px 0 rgba(255,255,255,0.88);
  background: var(--glass-hover);
  transform: translateY(-2px);
}
.rule-item {
  padding: var(--spacing-lg) var(--spacing-xxl) var(--spacing-lg) var(--spacing-xl);
  border-radius: var(--border-radius-md);
  margin: var(--spacing-xs) var(--spacing-lg);
  transition: all var(--transition-normal);
  border-left: 3px solid transparent;
  position: relative;
  backdrop-filter: blur(7px);
  -webkit-backdrop-filter: blur(7px);
  background: rgba(242, 246, 255, 0.6);
  border: 1px solid rgba(210, 220, 255, 0.12);
}
.rule-item:hover {
  background: var(--glass-hover);
  transform: translateX(2px);
  border-left-color: var(--primary-light);
  box-shadow: 
    0 4px 20px rgba(170,200,240,0.10),
    inset 0 1px 0 rgba(255,255,255,0.88);
  border-color: rgba(210,220,255,0.17);
}
.rule-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, 
    rgba(255,255,255,0.85) 0%, 
    rgba(255,255,255,0.67) 100%);
  border-radius: var(--border-radius-md);
  z-index: -1;
  opacity: 0.47;
}
.rule-header {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--spacing-sm) var(--spacing-lg);
  margin-bottom: var(--spacing-xs);
  position: relative;
}
.rule-title {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--primary-color);
  line-height: 1.4;
  position: relative;
  padding-left: var(--spacing-lg);
  text-shadow: 0 1px 1px rgba(240,245,255,0.7);
}
.rule-title::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 8px;
  height: 8px;
  background: linear-gradient(135deg, var(--primary-light), var(--primary-color));
  border-radius: 50%;
  box-shadow: 0 2px 4px rgba(169, 183, 228, 0.18);
}
.rule-meta {
  margin-bottom: var(--spacing-sm);
  color: var(--text-tertiary);
  font-size: var(--font-size-sm);
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
  padding-left: var(--spacing-lg);
  text-shadow: 0 1px 1px rgba(255,255,255,0.6);
}
.rule-content {
  color: var(--text-secondary);
  font-size: var(--font-size-md);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  background: var(--content-bg);
  border-radius: var(--border-radius-sm);
  line-height: 1.7;
  margin: var(--spacing-md) 0 var(--spacing-sm) 0;
  padding: var(--spacing-lg) var(--spacing-xl);
  word-break: break-word;
  white-space: pre-line;
  min-height: 20px;
  overflow-x: auto;
  transition: all var(--transition-normal);
  border: 1px solid rgba(210, 220, 255, 0.17);
  box-shadow: 
    inset 0 1px 2px rgba(240,245,255,0.5),
    0 2px 8px rgba(170,200,240,0.03);
  position: relative;
  overflow: hidden;
}
.rule-content::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: linear-gradient(to bottom, 
    transparent, 
    var(--primary-glass), 
    transparent);
  border-radius: var(--border-radius-pill);
  z-index: 1;
}
.rule-content::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, 
    rgba(255,255,255,0.97) 0%, 
    rgba(255,255,255,0.75) 100%);
  z-index: -1;
  border-radius: var(--border-radius-sm);
}
.rule-number {
  color: var(--primary-color);
  font-size: var(--font-size-sm);
  font-weight: 500;
  background: var(--primary-glass);
  padding: 4px var(--spacing-sm);
  border-radius: var(--border-radius-sm);
  border: 1px solid rgba(169,183,228,0.14);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}
.rule-version {
  color: var(--text-tertiary);
  font-size: var(--font-size-xs);
  font-style: italic;
  opacity: 0.7;
}
.rule-disabled {
  display: none;
}
.rule-penalty {
  color: var(--danger-light);
  font-weight: 600;
  background: var(--danger-glass);
  padding: 4px var(--spacing-sm);
  border-radius: var(--border-radius-sm);
  border: 1px solid rgba(251,207,210,0.17);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}
:deep(.el-tree) {
  --el-tree-node-content-height: auto;
  --el-tree-node-hover-bg-color: transparent;
  --el-tree-text-color: var(--text-primary);
  background: transparent;
}
:deep(.el-tree-node__content) {
  height: auto !important;
  min-height: 48px;
  align-items: stretch;
  padding: 0;
  margin: 4px 0;
  border-radius: var(--border-radius-sm);
  transition: all var(--transition-fast);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  background: rgba(240, 244, 250, 0.42);
  border: 1px solid rgba(210,220,255,0.12);
}
:deep(.el-tree-node__content:hover) {
  background: var(--hover-bg);
  border-color: rgba(169,183,228,0.22);
  box-shadow: 0 2px 8px rgba(170,200,240,0.06);
}
:deep(.el-tree-node.is-current > .el-tree-node__content) {
  background: var(--hover-bg);
  border-left: 3px solid var(--primary-light);
  border-color: rgba(169,183,228,0.33);
}
:deep(.el-tree-node__children) {
  margin-left: var(--tree-indent);
  border-left: 2px dashed rgba(210,220,255,0.17);
  padding-left: var(--spacing-lg);
  margin-top: var(--spacing-xs);
  position: relative;
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}
:deep(.el-tree-node__children::before) {
  content: '';
  position: absolute;
  left: -1px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: linear-gradient(to bottom, 
    transparent, 
    rgba(228,235,255,0.5), 
    transparent);
  z-index: 1;
}
:deep(.el-tree-node__expand-icon) {
  color: var(--primary-light);
  font-size: 1.1rem;
  transition: all var(--transition-normal);
  padding: 6px;
  border-radius: 6px;
  margin-right: var(--spacing-sm);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
  background: rgba(247,250,255,0.53);
  border: 1px solid rgba(210,220,255,0.12);
}
:deep(.el-tree-node__expand-icon:hover) {
  background: var(--primary-light);
  color: white;
  transform: scale(1.1);
  box-shadow: 0 2px 8px rgba(169,183,228,0.22);
  border-color: var(--primary-light);
}
:deep(.el-tree-node__expand-icon.is-leaf) {
  color: var(--text-light);
  opacity: 0.33;
  background: transparent;
  border: none;
}
:deep(.el-tree-node__expand-icon.expanded) {
  transform: rotate(90deg);
  background: rgba(169,183,228,0.13);
}
@media (max-width: 768px) {
  .rules-page {
    --max-width-md: 96vw;
    --tree-indent: 20px;
    padding: var(--spacing-xxl) 0 var(--spacing-xxxl) 0;
  }
  .title {
    font-size: 1.75rem;
    margin-bottom: var(--spacing-xxxl);
    padding: var(--spacing-md) var(--spacing-xxl);
  }
  .rules-tree {
    max-width: var(--max-width-md);
    padding: var(--spacing-md) 0;
    border-radius: var(--border-radius-md);
  }
  .rule-search {
    max-width: var(--max-width-mobile);
    margin: 0 var(--spacing-md) var(--spacing-xxl) var(--spacing-md);
  }
  .rule-header {
    gap: var(--spacing-xs) var(--spacing-md);
  }
  .rule-meta {
    gap: var(--spacing-md);
    font-size: var(--font-size-xs);
  }
  .rule-item {
    padding: var(--spacing-md) var(--spacing-lg) var(--spacing-md) var(--spacing-md);
    margin: var(--spacing-xs) var(--spacing-sm);
    backdrop-filter: blur(5px);
    -webkit-backdrop-filter: blur(5px);
  }
  .rule-content {
    padding: var(--spacing-md) var(--spacing-lg);
    font-size: var(--font-size-sm);
    backdrop-filter: blur(3px);
    -webkit-backdrop-filter: blur(3px);
  }
  :deep(.el-tree-node__children) {
    margin-left: var(--tree-indent);
    padding-left: var(--spacing-md);
  }
  :deep(.el-tree-node__content) {
    backdrop-filter: blur(5px);
    -webkit-backdrop-filter: blur(5px);
  }
}
@media (max-width: 480px) {
  .title {
    font-size: 1.5rem;
    margin-bottom: var(--spacing-xxl);
    padding: var(--spacing-sm) var(--spacing-xl);
  }
  .title::after { width: 36px; height: 2px; }
  .rule-header { flex-direction: column; align-items: flex-start; gap: var(--spacing-xs); }
  .rule-meta { flex-direction: column; align-items: flex-start; gap: var(--spacing-xs); }
  .rule-content { line-height: 1.6; padding: var(--spacing-sm) var(--spacing-md); }
  .rule-item { border-radius: var(--border-radius-sm); }
  :deep(.el-tree-node__content) { min-height: 38px; }
}
</style>