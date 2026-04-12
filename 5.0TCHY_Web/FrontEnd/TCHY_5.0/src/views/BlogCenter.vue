<template>
  <div class="blog-container">
    <div class="inner-container">
      
      <header class="blog-header">
        <div class="header-titles">
          <h1 class="main-title">探索与洞见</h1>
          <p class="sub-title">探索来自社区专家的深度文章、最新动态与行业趋势。</p>
        </div>
        <div class="search-box">
          <input type="text" placeholder="搜索文章..." class="search-input">
          <svg class="search-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
          </svg>
        </div>
      </header>

      <nav class="category-nav">
        <ul class="category-list">
          <li v-for="category in categories" :key="category.name">
            <button :class="['category-btn', { 'active': category.active }]">
              {{ category.name }}
            </button>
          </li>
        </ul>
      </nav>

      <section class="hero-section" v-if="heroPost">
        <div class="hero-card">
          <div class="hero-img-wrapper">
            <img :src="heroPost.coverImage || 'https://images.unsplash.com/photo-1499750310107-5fef28a66643?ixlib=rb-4.0.3&auto=format&fit=crop&w=1200&q=80'" :alt="heroPost.title" class="hero-img">
            <div class="hero-badge">编辑精选</div>
          </div>
          <div class="hero-content">
            <div class="meta-info">
              <span>{{ heroPost.publishTime }}</span>
              <span class="dot"></span>
              <span><svg class="icon-fire inline-block w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M12.395 2.553a1 1 0 00-1.45-.385c-.345.23-.614.558-.822.88-.214.33-.403.713-.57 1.116-.334.804-.614 1.768-.84 2.734a31.365 31.365 0 00-.613 3.58 2.64 2.64 0 01-.945-1.067c-.328-.68-.398-1.534-.398-2.654A1 1 0 005.05 6.05 6.981 6.981 0 003 11a7 7 0 1011.95-4.95c-.592-.591-.98-.985-1.348-1.467-.363-.476-.724-1.063-1.207-2.03zM12.12 15.12A3 3 0 017 13s.879.5 2.5.5c0-1 .5-4 1.25-4.5.5 1 .786 1.293 1.371 1.879A2.99 2.99 0 0113 13a2.99 2.99 0 01-.879 2.121z"></path></svg>{{ heroPost.viewCount }} 次阅读</span>
            </div>
            <h2 class="hero-title">
              <a :href="`/blog/${heroPost.id}`">{{ heroPost.title }}</a>
            </h2>
            <p class="hero-excerpt line-clamp-3">
              {{ heroPost.summary }}
            </p>
            <div class="author-info">
              <img :src="heroPost.authorAvatar || 'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png'" alt="Author" class="author-avatar">
              <div class="author-details">
                <p class="author-name">{{ heroPost.authorName }}</p>
                <p class="author-role">社区创作者</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <div class="main-layout">
        <main class="content-area">
          <div class="section-header">
            <h3 class="section-title">最新发布</h3>
          </div>

          <div v-if="isLoading && posts.length === 0" class="text-center text-gray-500 py-10">
            加载文章中...
          </div>

          <div class="post-grid" v-else>
            <article v-for="post in posts" :key="post.id" class="post-card">
              <div class="post-img-wrapper">
                <img :src="post.coverImage || 'https://images.unsplash.com/photo-1555066931-4365d14bab8c?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80'" :alt="post.title" class="post-img">
                <div class="post-category-badge" v-if="post.tags && post.tags.length > 0">{{ post.tags[0] }}</div>
              </div>
              <div class="post-content">
                <div class="meta-info">
                  <time :datetime="post.publishTime">{{ post.publishTime }}</time>
                  <span class="dot"></span>
                  <span>{{ post.viewCount }} 阅</span>
                </div>
                <h4 class="post-title line-clamp-2">
                  <a :href="`/blog/${post.id}`">{{ post.title }}</a>
                </h4>
                <p class="post-excerpt line-clamp-3">{{ post.summary }}</p>
                <div class="post-footer">
                  <img :src="post.authorAvatar || 'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png'" :alt="post.authorName" class="author-avatar-small">
                  <span class="author-name-small">{{ post.authorName }}</span>
                </div>
              </div>
            </article>
          </div>

          <div class="load-more-wrapper" v-if="hasMore">
            <button class="load-more-btn" @click="handleLoadMore" :disabled="isLoading">
              {{ isLoading ? '加载中...' : '加载更多文章' }}
            </button>
          </div>
          <div v-else-if="posts.length > 0" class="text-center text-gray-400 mt-8 text-sm">
            没有更多文章了
          </div>
        </main>

        <aside class="sidebar">
          <div class="widget">
            <h3 class="widget-title">
              <svg class="icon-fire" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M12.395 2.553a1 1 0 00-1.45-.385c-.345.23-.614.558-.822.88-.214.33-.403.713-.57 1.116-.334.804-.614 1.768-.84 2.734a31.365 31.365 0 00-.613 3.58 2.64 2.64 0 01-.945-1.067c-.328-.68-.398-1.534-.398-2.654A1 1 0 005.05 6.05 6.981 6.981 0 003 11a7 7 0 1011.95-4.95c-.592-.591-.98-.985-1.348-1.467-.363-.476-.724-1.063-1.207-2.03zM12.12 15.12A3 3 0 017 13s.879.5 2.5.5c0-1 .5-4 1.25-4.5.5 1 .786 1.293 1.371 1.879A2.99 2.99 0 0113 13a2.99 2.99 0 01-.879 2.121z"></path></svg>
              高能热文
            </h3>
            <ul class="trending-list">
              <li v-for="(trending, index) in trendingPosts" :key="index" class="trending-item">
                <a :href="`/blog/${trending.id}`" class="trending-link">
                  <span class="trending-number">0{{ index + 1 }}</span>
                  <div class="trending-info">
                    <h4 class="trending-title line-clamp-2">{{ trending.title }}</h4>
                    <span class="trending-views">{{ trending.viewsDisplay }} 次阅读</span>
                  </div>
                </a>
              </li>
            </ul>
          </div>

          <div class="widget" v-if="allTags.length > 0">
            <h3 class="widget-title">热门标签</h3>
            <div class="tag-cloud">
              <a v-for="tag in allTags" :key="tag" href="#" class="tag-link">#{{ tag }}</a>
            </div>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api.js' 

const categories = ref([
  { name: '全部动态', active: true },
  { name: '技术前沿', active: false },
  { name: '设计趋势', active: false },
  { name: '产品思考', active: false }
])

const heroPost = ref(null)      
const posts = ref([])           
const trendingPosts = ref([])   
const allTags = ref([])         

const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const isLoading = ref(false)

const hasMore = computed(() => {
  const currentLoaded = heroPost.value ? posts.value.length + 1 : posts.value.length
  return currentLoaded < total.value
})

const fetchArticles = async (isLoadMore = false) => {
  if (isLoading.value) return
  isLoading.value = true

  try {
    const response = await apiClient.get('/Blog/articles', {
      params: { 
        page: page.value, 
        pageSize: pageSize.value 
      }
    })

    // ⚡ 修正数据提取逻辑
    const data = response.data || response
    const list = data.list || []
    total.value = data.total || 0

    if (isLoadMore) {
      posts.value.push(...list)
    } else {
      if (list.length > 0) {
        heroPost.value = list[0]
        posts.value = list.slice(1)
      } else {
        posts.value = []
        heroPost.value = null
      }
    }

    const allLoadedPosts = [heroPost.value, ...posts.value].filter(Boolean)

    // ⚡ 修正: 使用驼峰命名的 viewCount 进行排序
    trendingPosts.value = [...allLoadedPosts]
      .sort((a, b) => (b.viewCount || 0) - (a.viewCount || 0))
      .slice(0, 5)
      .map(p => ({
        id: p.id,
        title: p.title,
        viewsDisplay: p.viewCount > 1000 ? (p.viewCount / 1000).toFixed(1) + 'k' : p.viewCount
      }))

    const tagSet = new Set(allTags.value)
    allLoadedPosts.forEach(p => {
      if (p.tags && Array.isArray(p.tags)) {
        p.tags.forEach(t => tagSet.add(t))
      }
    })
    allTags.value = Array.from(tagSet).slice(0, 15)

  } catch (error) {
    console.error('获取博客文章失败:', error)
  } finally {
    isLoading.value = false
  }
}

const handleLoadMore = () => {
  if (!hasMore.value) return
  page.value++
  fetchArticles(true)
}

onMounted(() => {
  fetchArticles()
})
</script>

<style scoped>
/* 全局变量与基础重置 */
.blog-container {
  background-color: #f9fafb; /* gray-50 */
  min-height: 100vh;
  padding: 2.5rem 0;
  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  color: #111827; /* gray-900 */
  box-sizing: border-box;
}

.blog-container * {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

.inner-container {
  max-width: 80rem; /* max-w-7xl */
  margin: 0 auto;
  padding: 0 1rem;
}

@media (min-width: 640px) {
  .inner-container { padding: 0 1.5rem; }
}
@media (min-width: 1024px) {
  .inner-container { padding: 0 2rem; }
}

a {
  text-decoration: none;
  color: inherit;
}

button {
  cursor: pointer;
  border: none;
  background: none;
  font-family: inherit;
}

/* 文本截断工具类 */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* --- 头部与搜索 --- */
.blog-header {
  margin-bottom: 3rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
@media (min-width: 768px) {
  .blog-header {
    flex-direction: row;
    align-items: flex-end;
    justify-content: space-between;
  }
}

.main-title {
  font-size: 2.25rem;
  font-weight: 800;
  letter-spacing: -0.025em;
  color: #111827;
}
@media (min-width: 640px) {
  .main-title { font-size: 3rem; }
}

.sub-title {
  margin-top: 1rem;
  font-size: 1.125rem;
  color: #6b7280; /* gray-500 */
  max-width: 42rem;
}

.search-box {
  position: relative;
  width: 100%;
}
@media (min-width: 768px) {
  .search-box { width: 18rem; }
}

.search-input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border-radius: 9999px;
  border: 1px solid #e5e7eb;
  background-color: #fff;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  outline: none;
}
.search-input:focus {
  border-color: transparent;
  box-shadow: 0 0 0 2px #3b82f6; /* blue-500 ring */
}

.search-icon {
  width: 1.25rem;
  height: 1.25rem;
  color: #9ca3af;
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
}

/* --- 分类导航 --- */
.category-nav {
  margin-bottom: 2.5rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.category-nav::-webkit-scrollbar {
  display: none;
}

.category-list {
  display: flex;
  list-style: none;
  gap: 0.5rem;
}

.category-btn {
  padding: 0.5rem 1.25rem;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
  transition: all 0.2s ease;
  border: 1px solid #e5e7eb;
  background-color: #fff;
  color: #4b5563;
}
.category-btn:hover {
  background-color: #f3f4f6;
}
.category-btn.active {
  background-color: #111827;
  color: #fff;
  border-color: #111827;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

/* --- 精选文章 (Hero) --- */
.hero-section {
  margin-bottom: 4rem;
}

.hero-card {
  background-color: #fff;
  border-radius: 1.5rem;
  overflow: hidden;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.3s ease;
}
.hero-card:hover {
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}
@media (min-width: 1024px) {
  .hero-card {
    flex-direction: row;
    height: 400px;
  }
}

.hero-img-wrapper {
  position: relative;
  height: 16rem;
  overflow: hidden;
}
@media (min-width: 1024px) {
  .hero-img-wrapper {
    width: 60%;
    height: 100%;
  }
}

.hero-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.7s ease;
}
.hero-card:hover .hero-img {
  transform: scale(1.05);
}

.hero-badge {
  position: absolute;
  top: 1rem;
  left: 1rem;
  background-color: #2563eb; /* blue-600 */
  color: #fff;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.hero-content {
  padding: 2rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
}
@media (min-width: 1024px) {
  .hero-content {
    width: 40%;
    padding: 3rem;
  }
}

.meta-info {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 1rem;
  gap: 1rem;
}
.dot {
  width: 0.25rem;
  height: 0.25rem;
  background-color: #d1d5db;
  border-radius: 9999px;
}

.hero-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1rem;
  line-height: 1.25;
  transition: color 0.3s ease;
}
.hero-card:hover .hero-title { color: #2563eb; }

.hero-excerpt {
  color: #4b5563;
  margin-bottom: 1.5rem;
}

.author-info {
  display: flex;
  align-items: center;
  margin-top: auto;
}
.author-avatar {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 9999px;
  object-fit: cover;
  margin-right: 0.75rem;
}
.author-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: #111827;
}
.author-role {
  font-size: 0.75rem;
  color: #6b7280;
}

/* --- 主体网格布局 --- */
.main-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: 3rem;
}
@media (min-width: 1024px) {
  .main-layout {
    grid-template-columns: 2fr 1fr; /* 左边占2份，右边占1份 */
  }
}

/* --- 文章列表 (左侧) --- */
.content-area {
  display: flex;
  flex-direction: column;
  gap: 2.5rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
.section-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #111827;
}
.view-all-btn {
  color: #2563eb;
  font-weight: 500;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
}
.view-all-btn:hover { color: #1e40af; }
.icon-arrow {
  width: 1rem;
  height: 1rem;
  margin-left: 0.25rem;
}

.post-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}
@media (min-width: 768px) {
  .post-grid { grid-template-columns: repeat(2, 1fr); }
}

.post-card {
  display: flex;
  flex-direction: column;
  background-color: #fff;
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
  transition: all 0.3s ease;
}
.post-card:hover {
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  transform: translateY(-4px);
}

.post-img-wrapper {
  position: relative;
  height: 12rem;
  overflow: hidden;
}
.post-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}
.post-card:hover .post-img { transform: scale(1.05); }

.post-category-badge {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  background-color: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(4px);
  color: #111827;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.25rem 0.625rem;
  border-radius: 0.375rem;
}

.post-content {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.post-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 0.75rem;
  transition: color 0.3s ease;
}
.post-card:hover .post-title { color: #2563eb; }

.post-excerpt {
  font-size: 0.875rem;
  color: #4b5563;
  margin-bottom: 1.25rem;
  flex: 1;
}

.post-footer {
  display: flex;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #f9fafb;
  margin-top: auto;
}
.author-avatar-small {
  width: 2rem;
  height: 2rem;
  border-radius: 9999px;
  margin-right: 0.5rem;
}
.author-name-small {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.load-more-wrapper {
  padding-top: 2rem;
  display: flex;
  justify-content: center;
}
.load-more-btn {
  padding: 0.75rem 2rem;
  background-color: #fff;
  border: 1px solid #e5e7eb;
  color: #374151;
  font-weight: 500;
  border-radius: 9999px;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  transition: all 0.2s ease;
}
.load-more-btn:hover {
  background-color: #f9fafb;
  color: #111827;
}

/* --- 侧边栏 --- */
.sidebar {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.widget {
  background-color: #fff;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
}

.widget-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
}
.icon-fire {
  width: 1.25rem;
  height: 1.25rem;
  margin-right: 0.5rem;
  color: #ef4444; /* red-500 */
}

/* 热门文章列表 */
.trending-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}
.trending-link {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}
.trending-number {
  font-size: 1.5rem;
  font-weight: 900;
  color: #e5e7eb;
  transition: color 0.3s ease;
}
.trending-item:hover .trending-number { color: #bfdbfe; }

.trending-title {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.25rem;
  transition: color 0.3s ease;
}
.trending-item:hover .trending-title { color: #2563eb; }

.trending-views {
  font-size: 0.75rem;
  color: #6b7280;
  display: block;
}

/* 订阅组件 */
.newsletter-widget {
  background: linear-gradient(to bottom right, #111827, #1f2937);
  color: #fff;
  border: none;
  position: relative;
  overflow: hidden;
  padding: 2rem;
}
.glow-effect {
  position: absolute;
  right: -2.5rem;
  top: -2.5rem;
  width: 8rem;
  height: 8rem;
  background-color: #fff;
  opacity: 0.05;
  border-radius: 9999px;
  filter: blur(24px);
}
.newsletter-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  position: relative;
  z-index: 10;
}
.newsletter-desc {
  color: #9ca3af;
  font-size: 0.875rem;
  margin-bottom: 1.5rem;
  position: relative;
  z-index: 10;
}
.newsletter-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  position: relative;
  z-index: 10;
}
.newsletter-input {
  width: 100%;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  background-color: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: #fff;
  outline: none;
}
.newsletter-input::placeholder { color: #9ca3af; }
.newsletter-input:focus { border-color: #3b82f6; }

.newsletter-btn {
  width: 100%;
  background-color: #2563eb;
  color: #fff;
  font-weight: 500;
  padding: 0.75rem;
  border-radius: 0.5rem;
  transition: background-color 0.2s;
}
.newsletter-btn:hover { background-color: #3b82f6; }

/* 标签云 */
.tag-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.tag-link {
  padding: 0.25rem 0.75rem;
  background-color: #f3f4f6;
  color: #4b5563;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s ease;
}
.tag-link:hover {
  background-color: #e5e7eb;
  color: #111827;
}
</style>