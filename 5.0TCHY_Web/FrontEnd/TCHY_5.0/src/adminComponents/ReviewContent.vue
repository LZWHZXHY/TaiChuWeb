<template>
  <section class="cyber-review-container">
    <div class="panel-scanline"></div>

    <nav class="cyber-tab-deck">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        :class="['cyber-tab-btn', { active: activeTab === tab.key }]"
        @click="switchTab(tab.key)"
      >
        <span class="tab-status-led"></span>
        <span class="tab-icon">{{ tab.icon }}</span>
        <span class="tab-label">{{ tab.label.toUpperCase() }}</span>
        <span v-if="tab.count > 0" class="tab-count-tag">{{ tab.count }}</span>
        <div class="tab-glitch-effect"></div>
      </button>
    </nav>

    <div class="cyber-search-terminal">
      <div class="search-prefix">> FILTER_NODE:</div>
      <input
        v-model="searchTerm"
        class="terminal-input"
        type="search"
        placeholder="INPUT_ID_OR_NAME_TO_QUERY..."
        @input="onSearchInput"
      />
      <div class="search-deco">SIGNAL_STRENGTH: 100%</div>
    </div>

    <div class="review-data-viewport cyber-card">
      <div class="card-label-strip">
        <span>// DATA_LOG_ACCESS // SECTION: {{ activeTab.toUpperCase() }}</span>
        <span class="blink-cursor">_</span>
      </div>

      <div class="panel-content-wrapper custom-scroll">
        <Transition name="fade-slide" mode="out-in">
          <div :key="activeTab" class="panel-container">
            <SocietyReviewPanel
              v-if="activeTab === 'society'"
              ref="societyPanel"
              :search="search"
              @update-count="updateTabCount('society', $event)"
            />

            <UnionReviewPanel
              v-if="activeTab === 'union'"
              ref="unionPanel"
              :search="search"
              @update-count="updateTabCount('union', $event)"
            />

            <PostsPanel
              v-if="activeTab === 'posts'"
              ref="postsPanel"
              :search="search"
              @update-count="updateTabCount('posts', $event)"
            />
            <ArtGalleryPanel
              v-if="activeTab === 'art'"
              ref="artPanel"
              :search="search"
              @update-count="updateTabCount('art', $event)"
            />
            <ArtistReviewPanel
              v-if="activeTab === 'artist'"
              ref="artistPanel"
              :search="search"
              @update-count="updateTabCount('artist', $event)"
            />
          </div>
        </Transition>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed } from 'vue'
import SocietyReviewPanel from './ReviewComponents/SocietyReviewPanel.vue'
import UnionReviewPanel from './ReviewComponents/UnionReviewPanel.vue'
import PostsPanel from './ReviewComponents/PostsPanel.vue'
import ArtistReviewPanel from './ReviewComponents/ArtistReviewPanel.vue'
import ArtGalleryPanel from './ReviewComponents/ArtGalleryPanel.vue'

const activeTab = ref('society')
const searchTerm = ref('')
const artPanel = ref(null);
const societyPanel = ref(null)
const unionPanel = ref(null)
const postsPanel = ref(null)
const artistPanel = ref(null)

const tabs = ref([
  { key: 'society', label: '社团审核', icon: '🏢', count: 0 },
  { key: 'union', label: '联合内容', icon: '🤝', count: 0 },
  { key: 'posts', label: '帖子审核', icon: '📝', count: 0 },
  { key: 'art', label: '画廊审核', icon: '🖼️', count: 0 },
  { key: 'artist', label: '画师认证', icon: '🎨', count: 0 }
])

let searchDebounce = null
const search = computed(() => searchTerm.value)

function onSearchInput() {
  clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    const panels = {
  society: societyPanel,
  union: unionPanel,
  posts: postsPanel,
  artist: artistPanel,
  art: artPanel // 添加此行
};
    const current = panels[activeTab.value].value
    if (current && current.onSearch) {
      current.onSearch(searchTerm.value)
    }
  }, 300)
}

function switchTab(tabKey) {
  activeTab.value = tabKey
}

function updateTabCount(tabKey, count) {
  const tab = tabs.value.find(t => t.key === tabKey)
  if (tab) tab.count = count
}

defineExpose({
  refresh: () => {
    [societyPanel, unionPanel, postsPanel, artistPanel].forEach(p => {
      if (p.value?.refresh) p.value.refresh()
    })
  },
  getActivePanel: () => {
    const map = { society: societyPanel, union: unionPanel, posts: postsPanel, artist: artistPanel }
    return map[activeTab.value]?.value || null
  }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.cyber-review-container {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;

  display: flex;
  flex-direction: column;
  gap: 20px;
  height: 100%;
  position: relative;
  font-family: 'Inter', sans-serif;
}

/* --- 顶部标签栏：工业控制面板风格 --- */
.cyber-tab-deck {
  display: flex;
  gap: 10px;
  padding: 5px;
  background: var(--gray);
  border: 2px solid var(--black);
}

.cyber-tab-btn {
  flex: 1;
  height: 45px;
  background: #fff;
  border: 1px solid var(--black);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  cursor: pointer;
  position: relative;
  transition: all 0.2s cubic-bezier(0.18, 0.89, 0.32, 1.28);
  overflow: hidden;
}

.tab-status-led {
  width: 6px;
  height: 6px;
  background: #ccc;
  border: 1px solid var(--black);
}

.tab-label {
  font-family: var(--mono);
  font-weight: 800;
  font-size: 0.8rem;
  color: #666;
}

.cyber-tab-btn.active {
  background: var(--black);
  color: var(--white);
  transform: translate(-2px, -2px);
  box-shadow: 4px 4px 0 var(--red);
}

.cyber-tab-btn.active .tab-label { color: var(--white); }
.cyber-tab-btn.active .tab-status-led {
  background: var(--red);
  box-shadow: 0 0 8px var(--red);
}

.tab-count-tag {
  background: var(--red);
  color: #fff;
  font-family: var(--mono);
  font-size: 0.65rem;
  padding: 0 5px;
  font-weight: bold;
}

/* --- 搜索框：终端风格 --- */
.cyber-search-terminal {
  background: var(--black);
  border: 2px solid var(--black);
  display: flex;
  align-items: center;
  padding: 0 15px;
  height: 40px;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.search-prefix {
  color: var(--red);
  font-family: var(--mono);
  font-weight: bold;
  font-size: 0.8rem;
  margin-right: 15px;
  white-space: nowrap;
}

.terminal-input {
  flex: 1;
  background: transparent;
  border: none;
  color: #fff;
  font-family: var(--mono);
  font-size: 0.9rem;
  outline: none;
}

.terminal-input::placeholder { color: #555; }

.search-deco {
  font-family: var(--mono);
  color: #333;
  font-size: 0.6rem;
  font-weight: bold;
}

/* --- 数据视口容器 --- */
.review-data-viewport {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

.cyber-card {
  background: #fff;
  border: 3px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.15);
}

.card-label-strip {
  background: var(--black);
  color: var(--white);
  padding: 5px 15px;
  font-family: var(--mono);
  font-size: 0.7rem;
  font-weight: bold;
  display: flex;
  justify-content: space-between;
}

.panel-content-wrapper {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: var(--white); /* 略带复古的纸张感 */
}

.blink-cursor {
  animation: blink 1s infinite;
  color: var(--red);
}

@keyframes blink { 
  0%, 100% { opacity: 1; } 
  50% { opacity: 0; } 
}

/* --- 切换动画 --- */
.fade-slide-enter-active, .fade-slide-leave-active {
  transition: all 0.3s ease;
}
.fade-slide-enter-from { opacity: 0; transform: translateX(20px); }
.fade-slide-leave-to { opacity: 0; transform: translateX(-20px); }

/* --- 滚动条 (同步 ComCenter) --- */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: var(--gray); }

/* 响应式 */
@media (max-width: 768px) {
  .cyber-tab-deck { flex-wrap: wrap; }
  .cyber-tab-btn { flex: 1 1 45%; }
  .cyber-search-terminal { height: auto; padding: 10px; flex-direction: column; align-items: flex-start; }
}
</style>