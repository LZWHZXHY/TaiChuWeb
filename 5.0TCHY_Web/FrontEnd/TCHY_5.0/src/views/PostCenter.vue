<template>
  <div class="forum-container">
    <div class="inner-container">
      
      <header class="forum-header">
        <div class="header-content">
          <h1 class="main-title">社区讨论版</h1>
          <p class="sub-title">在这里提出问题、分享经验，与数十万同行碰撞思想。</p>
        </div>
      </header>

      <div class="main-layout">
        
        <main class="content-area">
          <div class="filter-bar">
            <div class="tabs">
              <button class="tab-btn active">最新发布</button>
              <button class="tab-btn">热门讨论</button>
              <button class="tab-btn">精华区</button>
            </div>
            <div class="sort-dropdown">
              <span class="sort-label">排序：</span>
              <select class="sort-select">
                <option>按时间倒序</option>
                <option>按回复量倒序</option>
              </select>
            </div>
          </div>

          <div v-if="isLoading && posts.length === 0" class="loading-state">
            数据接入中，请稍候...
          </div>

          <div class="post-list" v-else>
            <article v-for="post in posts" :key="post.id" class="post-item">
              <div class="post-avatar-col">
                <img :src="post.author?.avatar || 'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png'" :alt="post.author?.username" class="avatar">
              </div>
              
              <div class="post-main-col">
                <h3 class="post-title">
                  <span v-if="post.view_count > 50" class="badge-hot">热帖</span>
                  <a :href="`/post/${post.id}`">{{ post.post_title }}</a>
                </h3>
                <p class="post-excerpt line-clamp-2">{{ post.content }}</p>
                <div class="post-meta">
                  <span class="author">{{ post.author?.username || '未知用户' }}</span>
                  <span class="dot"></span>
                  <time class="time">{{ formatDate(post.createTime) }}</time>
                  <span class="dot" v-if="post.tags && post.tags.length"></span>
                  <div class="tags-row" v-if="post.tags && post.tags.length">
                    <span class="tag-mini" v-for="tag in post.tags.slice(0, 3)" :key="tag">{{ tag }}</span>
                  </div>
                </div>
              </div>

              <div class="post-stats-col">
                <div class="stat-item" title="回复数">
                  <svg class="stat-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path></svg>
                  <span>{{ post.comment_count }}</span>
                </div>
                <div class="stat-item light" title="浏览量">
                  <svg class="stat-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path></svg>
                  <span>{{ post.view_count }}</span>
                </div>
              </div>
            </article>
          </div>

          <div class="load-more-wrapper" v-if="hasMore">
            <button class="btn-outline" @click="handleLoadMore" :disabled="isLoading">
              {{ isLoading ? '链路同步中...' : '载入更早的帖子' }}
            </button>
          </div>
          <div v-else-if="posts.length > 0" class="end-state">
            — 探测器已抵达数据边界 —
          </div>
        </main>

        <aside class="sidebar">
          <div class="widget stats-widget">
            <h3 class="widget-title">矩阵概况</h3>
            <div class="stats-grid">
              <div class="stat-box">
                <div class="stat-value">{{ total }}</div>
                <div class="stat-label">总帖文数</div>
              </div>
              <div class="stat-box">
                <div class="stat-value text-indigo">活跃</div>
                <div class="stat-label">服务器状态</div>
              </div>
            </div>
          </div>

          <div class="widget" v-if="activeUsers.length > 0">
            <h3 class="widget-title">
              <svg class="icon-sparkle" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M11.3 1.046A1 1 0 0112 2v5h4a1 1 0 01.82 1.573l-7 10A1 1 0 018 18v-5H4a1 1 0 01-.82-1.573l7-10a1 1 0 011.12-.381z" clip-rule="evenodd"></path></svg>
              活跃节点先锋
            </h3>
            <ul class="active-users-list">
              <li v-for="(user, index) in activeUsers" :key="index" class="user-item">
                <img :src="user.avatar" class="user-avatar-sm" alt="user">
                <div class="user-info">
                  <h4 class="user-name">{{ user.username }}</h4>
                  <p class="user-desc">获得 {{ user.totalViews }} 次阅览</p>
                </div>
                <div class="rank-badge" :class="'rank-' + (index + 1)">#{{ index + 1 }}</div>
              </li>
            </ul>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api.js' 

const posts = ref([])
const activeUsers = ref([])
const page = ref(1)
const pageSize = ref(15) 
const total = ref(0)
const isLoading = ref(false)

const hasMore = computed(() => {
  return posts.value.length < total.value
})

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

const fetchPosts = async (isLoadMore = false) => {
  if (isLoading.value) return
  isLoading.value = true

  try {
    const response = await apiClient.get('/ThePost', {
      params: { 
        page: page.value, 
        pageSize: pageSize.value 
      }
    })

    const resPayload = response.data || response
    if (resPayload.success) {
      const list = resPayload.data || []
      
      if (resPayload.pagination) {
        total.value = resPayload.pagination.totalCount
      }

      if (isLoadMore) {
        posts.value.push(...list)
      } else {
        posts.value = list
      }

      calculateActiveUsers(posts.value)
    }
  } catch (error) {
    console.error('获取帖子失败:', error)
  } finally {
    isLoading.value = false
  }
}

const calculateActiveUsers = (postList) => {
  const userMap = {}
  postList.forEach(p => {
    if (p.author && p.author.Id) {
      if (!userMap[p.author.Id]) {
        userMap[p.author.Id] = {
          username: p.author.username,
          avatar: p.author.avatar || 'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png',
          totalViews: 0
        }
      }
      userMap[p.author.Id].totalViews += p.view_count
    }
  })
  
  activeUsers.value = Object.values(userMap)
    .sort((a, b) => b.totalViews - a.totalViews)
    .slice(0, 4)
}

const handleLoadMore = () => {
  if (!hasMore.value) return
  page.value++
  fetchPosts(true)
}

onMounted(() => {
  fetchPosts()
})
</script>

<style scoped>
/* 全局变量与基础设定 */
.forum-container {
  background-color: #f8fafc; /* slate-50 */
  min-height: 100vh;
  padding: 2.5rem 0;
  font-family: ui-sans-serif, system-ui, -apple-system, "Segoe UI", Roboto, Arial, sans-serif;
  color: #0f172a; /* slate-900 */
}

.forum-container * {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

.inner-container {
  max-width: 72rem; 
  margin: 0 auto;
  padding: 0 1.5rem;
}

a { text-decoration: none; color: inherit; }
button { cursor: pointer; border: none; background: none; font-family: inherit; }

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* --- 头部区域 (精简版) --- */
.forum-header {
  margin-bottom: 2rem;
  background-color: #fff;
  padding: 2.5rem 3rem;
  border-radius: 1.25rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.02), 0 2px 4px -1px rgba(0, 0, 0, 0.02);
  border: 1px solid #f1f5f9;
  text-align: center; /* 居中排版显得更纯粹 */
}

.main-title {
  font-size: 2rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.025em;
}

.sub-title {
  margin-top: 0.75rem;
  font-size: 0.9375rem;
  color: #64748b;
  max-width: 36rem;
  margin-left: auto;
  margin-right: auto;
}

/* --- 核心网格布局 --- */
.main-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}
@media (min-width: 1024px) {
  .main-layout {
    grid-template-columns: 3fr 1fr;
  }
}

/* --- 内容区与过滤器 --- */
.filter-bar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 0.25rem;
}
@media (min-width: 640px) {
  .filter-bar {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }
}

.tabs { 
  display: flex; 
  gap: 0.5rem; 
}

.tab-btn {
  padding: 0.625rem 1rem;
  font-weight: 500;
  color: #64748b;
  position: relative;
  transition: color 0.2s ease;
  border-radius: 0.5rem 0.5rem 0 0;
}
.tab-btn:hover { 
  color: #0f172a; 
}
.tab-btn.active { 
  color: #4f46e5; 
  font-weight: 600; 
}
.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -0.25rem; /* 对齐底部边线 */
  left: 0.5rem;
  right: 0.5rem;
  height: 3px;
  background-color: #4f46e5;
  border-radius: 3px 3px 0 0;
}

/* 定制化排序下拉框 */
.sort-dropdown { 
  display: flex; 
  align-items: center; 
}
.sort-label {
  font-size: 0.875rem;
  color: #94a3b8;
  margin-right: 0.5rem;
}
.sort-select {
  appearance: none;
  background-color: #f1f5f9;
  border: 1px solid transparent;
  padding: 0.375rem 2.25rem 0.375rem 1rem;
  border-radius: 999px;
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
  outline: none;
  cursor: pointer;
  transition: all 0.2s ease;
  /* 精致的自定义下拉箭头 */
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%2364748b'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'%3E%3C/path%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 0.75rem center;
  background-size: 1rem;
}
.sort-select:hover {
  background-color: #e2e8f0;
}
.sort-select:focus {
  background-color: #fff;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

/* --- 帖子列表 --- */
.post-list { display: flex; flex-direction: column; gap: 1rem; }

.post-item {
  display: flex;
  gap: 1.25rem;
  padding: 1.5rem;
  background-color: #fff;
  border-radius: 1rem;
  border: 1px solid #f1f5f9;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.post-item:hover {
  border-color: #e2e8f0;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05), 0 4px 6px -2px rgba(0, 0, 0, 0.025);
  transform: translateY(-2px); /* 悬停轻微浮起 */
}

.post-avatar-col { flex-shrink: 0; }
.avatar {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 0.625rem;
  object-fit: cover;
  border: 1px solid #f1f5f9;
}

.post-main-col { flex: 1; min-width: 0; }
.post-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 0.375rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.post-title a { transition: color 0.2s; }
.post-title a:hover { color: #4f46e5; }

.badge-hot {
  font-size: 0.625rem;
  padding: 0.125rem 0.375rem;
  background-color: #fef2f2;
  color: #ef4444;
  border: 1px solid #fecaca;
  border-radius: 0.375rem;
  white-space: nowrap;
}

.post-excerpt {
  font-size: 0.875rem;
  color: #64748b;
  line-height: 1.6;
  margin-bottom: 0.75rem;
}

.post-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: #94a3b8;
}
.author { font-weight: 500; color: #475569; }
.dot { width: 3px; height: 3px; border-radius: 50%; background-color: #cbd5e1; }

.tags-row { display: flex; gap: 0.375rem; }
.tag-mini {
  background-color: #f8fafc;
  color: #475569;
  padding: 0.125rem 0.625rem;
  border-radius: 999px;
  border: 1px solid #e2e8f0;
  transition: all 0.2s ease;
  cursor: pointer;
}
.tag-mini:hover {
  background-color: #f1f5f9;
  border-color: #cbd5e1;
  color: #0f172a;
}

.post-stats-col {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: flex-end;
  gap: 0.5rem;
  flex-shrink: 0;
  min-width: 4rem;
}
.stat-item {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
}
.stat-item.light { color: #94a3b8; font-weight: 400; }
.stat-icon { width: 1.125rem; height: 1.125rem; }

/* 底部状态区 */
.load-more-wrapper { margin-top: 2rem; display: flex; justify-content: center; }
.btn-outline {
  padding: 0.75rem 2.5rem;
  border: 1px solid #cbd5e1;
  background-color: #fff;
  color: #475569;
  border-radius: 999px;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
}
.btn-outline:hover { 
  background-color: #f8fafc; 
  color: #0f172a; 
  border-color: #94a3b8; 
  transform: translateY(-1px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}
.btn-outline:active {
  transform: translateY(0);
}
.end-state { margin-top: 2.5rem; text-align: center; font-size: 0.875rem; color: #94a3b8; }
.loading-state { text-align: center; padding: 4rem 0; color: #64748b; }

/* --- 右侧边栏 --- */
.sidebar { display: flex; flex-direction: column; gap: 1.5rem; }

.widget {
  background-color: #fff;
  border-radius: 1rem;
  padding: 1.5rem;
  border: 1px solid #f1f5f9;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.02);
}
.widget-title {
  font-size: 1rem;
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 1.25rem;
  display: flex;
  align-items: center;
}
.icon-sparkle { width: 1.25rem; height: 1.25rem; color: #eab308; margin-right: 0.5rem; }

.stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.stat-box {
  background-color: #f8fafc;
  padding: 1rem;
  border-radius: 0.75rem;
  text-align: center;
  border: 1px solid #f1f5f9;
}
.stat-value { font-size: 1.5rem; font-weight: 800; color: #0f172a; }
.stat-value.text-indigo { color: #4f46e5; }
.stat-label { font-size: 0.75rem; color: #64748b; margin-top: 0.25rem; }

.active-users-list { list-style: none; display: flex; flex-direction: column; gap: 0.75rem; }
.user-item { 
  display: flex; 
  align-items: center; 
  gap: 0.75rem; 
  position: relative; 
  padding: 0.5rem;
  border-radius: 0.5rem;
  transition: background-color 0.2s ease;
}
.user-item:hover {
  background-color: #f8fafc;
}
.user-avatar-sm { width: 2.25rem; height: 2.25rem; border-radius: 0.5rem; object-fit: cover; }
.user-info { flex: 1; min-width: 0; }
.user-name { font-size: 0.875rem; font-weight: 600; color: #0f172a; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.user-desc { font-size: 0.75rem; color: #64748b; }

.rank-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.125rem 0.5rem;
  border-radius: 0.375rem;
  background-color: #f1f5f9;
  color: #64748b;
}
.rank-1 { background-color: #fef2f2; color: #ef4444; } 
.rank-2 { background-color: #fff7ed; color: #f97316; } 
.rank-3 { background-color: #fefce8; color: #eab308; } 
</style>