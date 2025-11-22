<template>
  <section class="review-content">
    <!-- 顶部导航标签 -->
    <nav class="review-tabs">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        :class="['tab-button', { active: activeTab === tab.key }]"
        @click="switchTab(tab.key)"
      >
        <span class="tab-icon">{{ tab.icon }}</span>
        <span class="tab-label">{{ tab.label }}</span>
        <span v-if="tab.count > 0" class="tab-badge">{{ tab.count }}</span>
      </button>
    </nav>

    <!-- 内容区域 -->
    <div class="review-panels">
      <!-- 社团审核面板 -->
      <div v-show="activeTab === 'society'" class="panel-container">
        <SocietyReviewPanel
          ref="societyPanel"
          :search="search"
          @update-count="updateTabCount('society', $event)"
        />
      </div>

      <!-- 联合内容审核面板 -->
      <div v-show="activeTab === 'union'" class="panel-container">
        <UnionReviewPanel
          ref="unionPanel"
          :search="search"
          @update-count="updateTabCount('union', $event)"
        />
      </div>
    </div>

    <!-- 全局搜索框 -->
    <div class="global-search">
      <input
        v-model="searchTerm"
        class="search-input"
        type="search"
        placeholder="全局搜索：名称 / 团长 / 作者…"
        @input="onSearchInput"
      />
    </div>
  </section>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import SocietyReviewPanel from './ReviewComponents/SocietyReviewPanel.vue'
import UnionReviewPanel from './ReviewComponents/UnionReviewPanel.vue'

const activeTab = ref('society') // 默认显示社团审核
const searchTerm = ref('')
const societyPanel = ref(null)
const unionPanel = ref(null)

// 标签页配置
const tabs = ref([
  {
    key: 'society',
    label: '社团审核',
    icon: '🏢',
    count: 0
  },
  {
    key: 'union',
    label: '联合内容审核',
    icon: '🤝',
    count: 0
  }
])

// 防抖搜索
let searchDebounce = null
const search = computed(() => searchTerm.value)

function onSearchInput() {
  clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    // 通知当前激活的面板进行搜索
    if (activeTab.value === 'society' && societyPanel.value) {
      societyPanel.value.onSearch(searchTerm.value)
    } else if (activeTab.value === 'union' && unionPanel.value) {
      unionPanel.value.onSearch(searchTerm.value)
    }
  }, 300)
}

function switchTab(tabKey) {
  activeTab.value = tabKey
  // 切换标签时重置搜索，或者保持搜索状态取决于需求
  // searchTerm.value = ''
}

function updateTabCount(tabKey, count) {
  const tab = tabs.value.find(t => t.key === tabKey)
  if (tab) {
    tab.count = count
  }
}

// 暴露方法供父组件调用
defineExpose({
  refresh: () => {
    if (societyPanel.value) societyPanel.value.refresh()
    if (unionPanel.value) unionPanel.value.refresh()
  },
  getActivePanel: () => {
    return activeTab.value === 'society' ? societyPanel.value : unionPanel.value
  }
})
</script>

<style scoped>
.review-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
  height: 100%;
  position: relative;
}

/* 标签导航样式 */
.review-tabs {
  display: flex;
  background: var(--light-bg);
  border-radius: var(--radius-lg);
  padding: 4px;
  border: 1px solid var(--border-color);
  margin-bottom: 16px;
}

.tab-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  border: none;
  background: transparent;
  border-radius: var(--radius);
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: var(--transition);
  color: var(--text-secondary);
  flex: 1;
  justify-content: center;
  position: relative;
}

.tab-button.active {
  background: var(--card-bg);
  box-shadow: var(--shadow);
  color: var(--primary-color);
  font-weight: 600;
}

.tab-button:hover:not(.active) {
  background: rgba(255, 255, 255, 0.8);
  color: var(--primary-color);
}

.tab-icon {
  font-size: 16px;
}

.tab-label {
  white-space: nowrap;
}

.tab-badge {
  background: var(--primary-color);
  color: white;
  border-radius: 10px;
  padding: 2px 6px;
  font-size: 11px;
  font-weight: 600;
  min-width: 18px;
  text-align: center;
}

/* 内容区域 */
.review-panels {
  flex: 1;
  min-height: 0;
}

.panel-container {
  height: 100%;
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 全局搜索 */
.global-search {
  position: sticky;
  bottom: 20px;
  z-index: 10;
  display: flex;
  justify-content: center;
}

.search-input {
  width: 300px;
  padding: 12px 16px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  background: var(--card-bg);
  box-shadow: var(--shadow-lg);
  font-size: 14px;
  transition: var(--transition);
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  transform: translateY(-2px);
}

.search-input::placeholder {
  color: var(--text-light);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .review-tabs {
    flex-direction: column;
    gap: 4px;
  }
  
  .tab-button {
    padding: 10px 16px;
  }
  
  .global-search {
    position: static;
    margin-top: 16px;
  }
  
  .search-input {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .tab-button {
    flex-direction: column;
    gap: 4px;
    padding: 8px 12px;
  }
  
  .tab-label {
    font-size: 13px;
  }
  
  .tab-icon {
    font-size: 14px;
  }
}
</style>