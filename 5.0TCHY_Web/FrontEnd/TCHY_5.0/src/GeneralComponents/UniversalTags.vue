<template>
  <div class="universal-tags-wrapper" v-if="tags && tags.length > 0">
    <div class="tags-icon">🏷️</div>
    <div class="tags-list">
      <div 
        v-for="(tag, index) in tags" 
        :key="index" 
        class="cyber-tag-chip"
        @click="handleTagClick(tag)"
      >
        <span class="hash">#</span>
        <span class="text">{{ tag }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const props = defineProps({
  // 接收字符串数组 ["赛博", "机甲"]
  tags: { type: Array, default: () => [] } 
});

const router = useRouter();

const handleTagClick = (tagName) => {
  // 跳转到搜索页或标签聚合页
  // 假设你的搜索路由是 /search?q=xxx
  router.push({ path: '/search', query: { q: tagName, type: 'tag' } });
};
</script>

<style scoped>
.universal-tags-wrapper {
  display: flex;
  align-items: flex-start; /* 对齐顶部 */
  gap: 12px;
  margin-top: 15px;
  padding: 10px 0;
  border-top: 1px dashed #eee;
}

.tags-icon {
  font-size: 1.1rem;
  margin-top: 2px;
  opacity: 0.7;
}

.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.cyber-tag-chip {
  background: #fff;
  border: 1px solid #333;
  padding: 4px 10px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.85rem;
  color: #333;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  position: relative;
  overflow: hidden;
}

.cyber-tag-chip .hash {
  color: #D92323; /* 赛博红 */
  margin-right: 4px;
  font-weight: bold;
}

.cyber-tag-chip .text {
  font-weight: 600;
}

/* 悬停特效：变成黑底白字 */
.cyber-tag-chip:hover {
  background: #111;
  border-color: #111;
  transform: translateY(-2px);
  box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.2);
}

.cyber-tag-chip:hover .text {
  color: #fff;
}
</style>