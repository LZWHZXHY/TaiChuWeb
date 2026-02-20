<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import apiClient from '@/utils/api';

// 组件导入
import HeroCarousel from '@/MainPushComponent/HeroCarousel.vue';
import CyberTicker from '@/MainPushComponent/CyberTicker.vue';  
import PostCard from '@/MainPushComponent/cards/PostCard.vue';
import BlogCard from '@/MainPushComponent/cards/BlogCard.vue';
import ArtCard from '@/MainPushComponent/cards/ArtCard.vue';

// --- 分页与数据逻辑 ---
const rawFeedData = ref<any[]>([]);
const isLoading = ref(false); 
const currentPage = ref(1);
const pageSize = 10;
const hasMore = ref(true); 
const loadingTriggerRef = ref<HTMLElement | null>(null); 
let observer: IntersectionObserver | null = null;

const fetchFeedData = async (isLoadMore = false) => {
  if (isLoading.value || !hasMore.value) return;
  try {
    isLoading.value = true;
    const response = await apiClient.get(`/Feed/latest?page=${currentPage.value}&limit=${pageSize}`); 
    if (response.data && response.data.code === 200) {
      const newItems = response.data.data.map((item: any) => ({
        id: item.Id,
        type: item.Type.toLowerCase(),
        author: item.Author,
        authorId: item.AuthorId,
        timestamp: item.Timestamp,
        title: item.Title,
        content: item.Content,
        imgUrl: item.ImgUrl
      })); 
      if (newItems.length < pageSize) hasMore.value = false;
      if (isLoadMore) rawFeedData.value.push(...newItems);
      else rawFeedData.value = newItems;
      currentPage.value++;
    }
  } catch (error) {
    console.error('获取动态数据失败:', error);
  } finally {
    isLoading.value = false;
  }
};

const sortedFeed = computed(() => {
  return [...rawFeedData.value].sort((a: any, b: any) => {
    return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime();
  });
});

const getComponentType = (type: string): any => {
  const map: Record<string, any> = { post: PostCard, blog: BlogCard, art: ArtCard };
  return map[type] || PostCard;
};

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  const d = new Date(dateString);
  return `${d.getFullYear()}.${String(d.getMonth() + 1).padStart(2, '0')}.${String(d.getDate()).padStart(2, '0')}`;
};

onMounted(() => {
  fetchFeedData();
  observer = new IntersectionObserver((entries) => {
    if (entries[0].isIntersecting && hasMore.value && !isLoading.value) {
      fetchFeedData(true);
    }
  }, { threshold: 0.1 });
  if (loadingTriggerRef.value) observer.observe(loadingTriggerRef.value);
});

onUnmounted(() => {
  if (observer) observer.disconnect();
});
</script>

<template>
  <div class="doc-wrapper">
    

    <main class="main-content">
      <section class="hero-section">
        <div class="hero-inner">
          <HeroCarousel />
          <div class="ticker-box">
            <CyberTicker />
          </div>
        </div>
        <div class="section-divider">
          <span class="divider-label">FILE_STREAM // 实时动态数据流</span>
        </div>
      </section>

      <div class="feed-container">
        <transition-group name="fade-list" tag="div" class="feed-list">
          <div 
            v-for="item in sortedFeed" 
            :key="item.type + item.id" 
            class="feed-item-wrapper"
          >
            <div class="item-header">
              <span class="type-badge">{{ item.type }}</span>
              <span class="timestamp">{{ formatDate(item.timestamp) }}</span>
            </div>
            
            <div class="item-content">
              <component :is="getComponentType(item.type)" :data="item" />
            </div>
          </div>
        </transition-group>

        <div ref="loadingTriggerRef" class="loading-trigger">
          <div v-if="isLoading" class="loader">
            <span class="dot"></span>
            <span>SYNCHRONIZING...</span>
          </div>
          <p v-else-if="!hasMore" class="end-msg">--- END OF LOCAL DATA ---</p>
        </div>
      </div>
    </main>

    <footer class="doc-footer">
      <div class="footer-inner">
        <p class="footer-main">支持太初寰宇！</p>
        <p class="footer-sub">© 2026 TAI CHU HUAN YU. ALL RIGHTS RESERVED.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
/* 核心容器：灰白色温增加纸张感 */
.doc-wrapper {
  background-color: #f8f9fa;
  color: #1a1a1a;
  min-height: 100vh;
  font-family: 'Inter', -apple-system, sans-serif;
}

/* 导航：利用边框线条增加设计感 */
.doc-header {
  position: sticky;
  top: 0;
  background: rgba(248, 249, 250, 0.9);
  backdrop-filter: blur(10px);
  border-bottom: 2px solid #1a1a1a;
  z-index: 1000;
}

.header-content {
  max-width: 1000px;
  margin: 0 auto;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-text { font-size: 20px; font-weight: 800; letter-spacing: -1px; }
.status-tag { 
  font-family: 'JetBrains Mono', monospace; 
  font-size: 10px; 
  background: #1a1a1a; 
  color: #fff; 
  padding: 2px 6px; 
  margin-left: 12px;
}

.nav-item { 
  text-decoration: none; 
  color: #666; 
  font-weight: 600; 
  font-size: 13px; 
  margin-left: 24px;
}
.nav-item.active { color: #1a1a1a; text-decoration: underline; text-underline-offset: 4px; }

.main-content {
  width: 80%;
  margin: 0 auto;
  padding: 40px 20px;
}

/* 推广展示区 */
.hero-section {
  margin-bottom: 60px;
}

.hero-inner {
  background: #fff;
  border: 1px solid #ddd;
  padding: 8px;
  box-shadow: 4px 4px 0px #eee; /* 这里的阴影是硬阴影，更显简约大方 */
}

.ticker-box {
  margin-top: 8px;
  border-top: 1px solid #eee;
  padding: 8px 4px;
}

/* 分割线：模拟文档章节 */
.section-divider {
  display: flex;
  align-items: center;
  margin: 40px 0;
}

.divider-label {
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: #999;
  padding-right: 15px;
  white-space: nowrap;
}

.section-divider::after {
  content: "";
  flex-grow: 1;
  height: 1px;
  background: #ddd;
}

/* 动态流列表 */
.feed-container {
  max-width: 700px;
  margin: 0 auto;
}

.feed-item-wrapper {
  margin-bottom: 40px;
  background: #fff;
  border-left: 3px solid #1a1a1a; /* 增强文档感 */
  padding: 24px;
  transition: transform 0.2s ease;
}

.feed-item-wrapper:hover {
  transform: translateX(5px);
}

.item-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 16px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 11px;
}

.type-badge { color: #fff; background: #1a1a1a; padding: 2px 8px; }
.timestamp { color: #999; }

/* 页脚 */
.doc-footer {
  border-top: 1px solid #ddd;
  padding: 80px 0;
  text-align: center;
  background: #fff;
}

.footer-inner .footer-main { font-weight: 700; margin-bottom: 8px; }
.footer-inner .footer-sub { font-size: 12px; color: #999; font-family: 'JetBrains Mono'; }

/* 过渡动画 */
.fade-list-enter-active, .fade-list-leave-active { transition: all 0.4s ease; }
.fade-list-enter-from { opacity: 0; transform: translateY(20px); }
</style>