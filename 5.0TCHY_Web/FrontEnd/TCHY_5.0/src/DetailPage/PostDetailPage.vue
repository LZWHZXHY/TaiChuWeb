<template>
  <div class="modern-page-wrapper custom-scroll">
    
    <nav class="modern-nav-bar">
      <div class="nav-container">
        <button class="back-btn" @click="handleBack">
          <svg viewBox="0 0 24 24" width="20" height="20" stroke="currentColor" stroke-width="2" fill="none"><path d="M19 12H5M12 19l-7-7 7-7"/></svg>
          返回探索
        </button>
        <span class="nav-title" v-if="postData">DATA NODE // {{ route.params.id }}</span>
      </div>
    </nav>

    <div v-if="loading" class="status-screen">
      <div class="loading-spinner"></div>
      <div class="loading-text">正在同步节点数据...</div>
    </div>

    <div v-else-if="postData" class="page-layout-grid">
      
      <main class="main-content-column">
        <article class="modern-card post-article">
          <header class="post-header">
            <h1 class="post-title">{{ postData.post_title }}</h1>
            <div class="author-bar">
              <div class="author-avatar-small">
                <UserAvatar :user-id="postData.author_id" :passed-avatar="postData.author?.avatar" :show-level="false" />
              </div>
              <div class="author-info-inline">
                <span class="name-text">@{{ postData.author?.username || '匿名创造者' }}</span>
                <span class="dot">·</span>
                <span class="time">{{ formatTime(postData.createTime) }}</span>
              </div>
            </div>
          </header>

          <div v-if="postData.images?.length" class="media-gallery">
            <div class="media-grid" :class="`grid-${Math.min(postData.images.length, 4)}`">
              <div 
                v-for="(img, i) in postData.images" 
                :key="i" 
                class="image-wrapper zoomable"
                @click="openLightbox(img)"
              >
                <img :src="img" class="grid-image" @error="handleImgError" alt="Post media" />
              </div>
            </div>
          </div>

          <div class="post-body">
            <div class="modern-markdown" v-html="renderMarkdown(postData.content)"></div>
          </div>

          <div class="post-footer">
            <hr class="divider" />
            <span class="footer-text">END OF POST</span>
          </div>
        </article>

        <section class="comments-section">
          <h3 class="section-title">参与讨论</h3>
          <div class="modern-card comments-card">
            <UniversalComments 
              v-if="postData && postData.id" 
              targetType="Post" 
              :targetId="postData.id" 
            />
          </div>
        </section>
      </main>

      <aside class="sidebar-column">
        
        <section class="modern-card sidebar-widget author-identity">
          <div class="widget-header">
            <span class="widget-label">ORIGIN AUTHOR</span>
            <span class="widget-id">UID: #{{ postData.author_id?.toString().padStart(4, '0') }}</span>
          </div>
          <div class="identity-main">
            <div class="avatar-large">
              <UserAvatar :user-id="postData.author_id" :passed-avatar="postData.author?.avatar" :show-level="true" />
            </div>
            <div class="identity-info">
              <div class="username hover-link" @click="router.push(`/profile/${postData.author_id}`)">
                @{{ postData.author?.username }}
              </div>
              <div class="user-status"><span class="status-dot"></span> 节点在线</div>
            </div>
          </div>
          <div class="action-grid">
            <button class="modern-btn primary">关注创造者</button>
            <button class="modern-btn secondary" @click="handleShare" :class="{ 'success-state': shareStatus === 'COPIED!' }">
              {{ shareStatus === 'READY' ? '分享档案' : '已复制链接!' }}
            </button>
          </div>
        </section>

        <section class="modern-card sidebar-widget related-posts">
          <div class="widget-header">
            <span class="widget-label">RELATED DATA</span>
          </div>
          <div class="linked-list">
            <template v-if="relatedPosts.length > 0">
              <div 
                v-for="item in relatedPosts" 
                :key="item.id" 
                class="linked-item"
                @click="router.push(`/post/${item.id}`)" 
              >
                <div class="item-title">{{ item.post_title }}</div>
                <div class="item-meta">👁️ {{ item.view_count || 0 }} views</div>
              </div>
            </template>
            <div v-else class="empty-hint">暂无其他关联数据</div>
          </div>
        </section>

        <section class="modern-card sidebar-widget metrics-board">
          <div class="widget-header">
            <span class="widget-label">METRICS LOG</span>
          </div>
          <div class="metrics-grid">
            <div class="metric-box">
              <span class="m-val">{{ postData?.view_count || 0 }}</span>
              <span class="m-lab">阅读量</span>
            </div>
            <div class="metric-box">
              <span class="m-val">{{ postData?.comment_count || 0 }}</span>
              <span class="m-lab">讨论数</span>
            </div>
          </div>
        </section>
      </aside>

    </div>

    <Transition name="fade">
      <div v-if="showLightbox" class="sleek-lightbox" @click="closeLightbox">
        <button class="close-btn" @click.stop="closeLightbox">
          <svg viewBox="0 0 24 24" width="32" height="32" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
        </button>
        <img :src="currentZoomImage" class="lightbox-image" @click.stop />
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, onMounted, watch, onBeforeUnmount } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { marked } from 'marked';
import apiClient from '@/utils/api';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';
import UniversalComments from '@/GeneralComponents/UniversalComments.vue';

const route = useRoute();
const router = useRouter();

// 状态管理
const postData = ref(null);
const relatedPosts = ref([]);
const loading = ref(true);
const shareStatus = ref('READY');

// Lightbox 状态
const showLightbox = ref(false);
const currentZoomImage = ref('');

// ==========================================
// 逻辑方法
// ==========================================

const handleBack = () => {
  if (window.history.length > 1) { router.back(); } 
  else { router.push('/comCenter'); }
};

const handleShare = async () => {
  try {
    await navigator.clipboard.writeText(window.location.href);
    shareStatus.value = 'COPIED!';
    setTimeout(() => { shareStatus.value = 'READY'; }, 2000);
  } catch (err) { shareStatus.value = 'ERROR'; }
};

// 格式化时间
const formatTime = (t) => {
  if (!t) return 'N/A';
  const date = new Date(t);
  return `${date.getFullYear()}-${String(date.getMonth()+1).padStart(2,'0')}-${String(date.getDate()).padStart(2,'0')} ${String(date.getHours()).padStart(2,'0')}:${String(date.getMinutes()).padStart(2,'0')}`;
};

// Markdown 与图片处理
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

// Lightbox 方法
const openLightbox = (imgUrl) => {
  currentZoomImage.value = imgUrl;
  showLightbox.value = true;
  document.body.style.overflow = 'hidden';
};
const closeLightbox = () => {
  showLightbox.value = false;
  currentZoomImage.value = '';
  document.body.style.overflow = '';
};

// ==========================================
// 数据获取
// ==========================================
const fetchRelatedPosts = async (userId, currentPostId) => {
  try {
    const res = await apiClient.get(`/ThePost/user-related/${userId}`, {
      params: { excludePostId: currentPostId, limit: 5 }
    });
    if (res.data.success) { relatedPosts.value = res.data.data; }
  } catch (e) { console.error(e); }
};

const fetchFullData = async (id) => {
  if (!id) return;
  loading.value = true;
  try {
    const res = await apiClient.get(`/ThePost/${id}`);
    if (res.data.success) {
      postData.value = res.data.data;
      document.title = `${postData.value.post_title} // TAICHU`;
      if (postData.value.author_id) { fetchRelatedPosts(postData.value.author_id, id); }
    }
  } catch (e) { console.error(e); } 
  finally { loading.value = false; }
};

// 监听与生命周期
watch(() => route.params.id, (newId) => { fetchFullData(newId); });
onMounted(() => { fetchFullData(route.params.id); });
onBeforeUnmount(() => { document.body.style.overflow = ''; });
</script>

<style scoped>
/* --- 全局与色彩变量 --- */
.modern-page-wrapper {
  --bg-color: #f3f4f6; /* 清新浅灰底色 */
  --card-bg: #ffffff;
  --text-main: #111827;
  --text-muted: #6b7280;
  --border-color: #e5e7eb;
  --accent-color: #3b82f6; /* 现代科技蓝 */
  
  background-color: var(--bg-color);
  min-height: 100vh;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: var(--text-main);
}

/* --- 1. 顶部导航栏 --- */
.modern-nav-bar {
  position: sticky; top: 0; z-index: 100;
  height: 60px; background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(12px); border-bottom: 1px solid var(--border-color);
}
.nav-container {
  max-width: 1200px; margin: 0 auto; height: 100%;
  display: flex; align-items: center; padding: 0 20px; gap: 20px;
}
.back-btn {
  background: transparent; border: none; color: var(--text-muted);
  font-size: 0.95rem; font-weight: 600; cursor: pointer; 
  display: flex; align-items: center; gap: 6px; padding: 0; transition: color 0.2s;
}
.back-btn:hover { color: var(--text-main); }
.nav-title { color: var(--text-muted); font-size: 0.85rem; font-family: 'JetBrains Mono', monospace; border-left: 1px solid var(--border-color); padding-left: 20px; }

/* --- 2. 页面布局 (左右分栏) --- */
.page-layout-grid {
  max-width: 1200px; margin: 40px auto; padding: 0 20px;
  display: grid; grid-template-columns: 1fr 320px; gap: 30px; align-items: start;
}
.main-content-column { display: flex; flex-direction: column; gap: 30px; min-width: 0; }
.sidebar-column { display: flex; flex-direction: column; gap: 20px; position: sticky; top: 80px; }

/* 核心卡片通用样式 */
.modern-card {
  background: var(--card-bg); border-radius: 16px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
  border: 1px solid var(--border-color); overflow: hidden;
}

/* --- 3. 文章主体区 --- */
.post-header { padding: 32px 32px 20px 32px; }
.post-title { font-size: 2.2rem; font-weight: 800; line-height: 1.3; margin: 0 0 20px 0; letter-spacing: -0.02em; word-break: break-word; }

.author-bar { display: flex; align-items: center; gap: 12px; }
.author-avatar-small { width: 40px; height: 40px; border-radius: 50%; }
.author-info-inline { display: flex; align-items: center; gap: 8px; font-size: 0.9rem; }
.name-text { font-weight: 700; color: var(--text-main); }
.time, .dot { color: var(--text-muted); }

/* 相册网格 */
.media-gallery { padding: 0 32px 20px 32px; }
.media-grid { display: grid; gap: 12px; border-radius: 12px; overflow: hidden; }
.media-grid.grid-1 { grid-template-columns: 1fr; }
.media-grid.grid-1 .image-wrapper { max-height: 500px; }
.media-grid.grid-2 { grid-template-columns: repeat(2, 1fr); }
.media-grid.grid-2 .image-wrapper { aspect-ratio: 1; }
.media-grid.grid-3, .media-grid.grid-4 { grid-template-columns: repeat(2, 1fr); }
.media-grid.grid-3 .image-wrapper:first-child { grid-column: span 2; aspect-ratio: 21/9; }
.media-grid.grid-3 .image-wrapper:not(:first-child) { aspect-ratio: 16/9; }
.media-grid.grid-4 .image-wrapper { aspect-ratio: 16/9; }

.image-wrapper { position: relative; overflow: hidden; background: #e5e7eb; border: 1px solid rgba(0,0,0,0.05); }
.grid-image { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s ease, filter 0.3s ease; }
.image-wrapper.zoomable { cursor: zoom-in; }
.image-wrapper.zoomable:hover .grid-image { transform: scale(1.03); filter: brightness(0.95); }

/* Markdown 渲染 */
.post-body { padding: 0 32px 32px 32px; }
.modern-markdown { font-size: 1.1rem; line-height: 1.7; color: #374151; word-wrap: break-word; }
.modern-markdown :deep(p) { margin-bottom: 1.2em; }
.modern-markdown :deep(h1), .modern-markdown :deep(h2), .modern-markdown :deep(h3) { color: var(--text-main); font-weight: 700; margin-top: 1.5em; margin-bottom: 0.8em; }
.modern-markdown :deep(h2) { border-bottom: 1px solid var(--border-color); padding-bottom: 0.3em; }
.modern-markdown :deep(strong) { font-weight: 700; color: #111; }
.modern-markdown :deep(u) { text-decoration: underline; text-decoration-color: var(--accent-color); text-underline-offset: 3px; }
.modern-markdown :deep(blockquote) { border-left: 4px solid var(--border-color); padding-left: 16px; color: var(--text-muted); background: #f9fafb; margin: 1.5em 0; padding: 12px 16px; border-radius: 0 8px 8px 0; font-style: italic; }
.modern-markdown :deep(ul), .modern-markdown :deep(ol) { padding-left: 24px; margin-bottom: 1.2em; }
.modern-markdown :deep(code) { background: #f3f4f6; color: #ef4444; padding: 2px 6px; border-radius: 4px; font-family: 'JetBrains Mono', monospace; font-size: 0.9em; }
.modern-markdown :deep(pre) { background: #1f2937; color: #e5e7eb; padding: 16px; border-radius: 8px; overflow-x: auto; margin: 1.5em 0; }
.modern-markdown :deep(pre code) { background: transparent; color: inherit; padding: 0; }

.post-footer { padding: 0 32px 24px 32px; text-align: center; }
.divider { border: none; border-top: 1px solid var(--border-color); margin-bottom: 16px; }
.footer-text { font-size: 0.75rem; color: #9ca3af; letter-spacing: 1px; }

/* 评论区 */
.section-title { font-size: 1.2rem; font-weight: 700; color: var(--text-main); margin-bottom: 16px; padding-left: 8px; }
.comments-card { padding: 24px; }

/* --- 4. 侧边栏区 --- */
.sidebar-widget { padding: 20px; }
.widget-header { border-bottom: 1px solid var(--border-color); padding-bottom: 12px; margin-bottom: 16px; display: flex; justify-content: space-between; align-items: center; }
.widget-label { font-size: 0.8rem; font-weight: 700; color: var(--text-muted); letter-spacing: 0.5px; }
.widget-id { font-family: 'JetBrains Mono', monospace; font-size: 0.7rem; color: #9ca3af; }

/* 作者卡片 */
.identity-main { display: flex; gap: 16px; align-items: center; margin-bottom: 20px; }
.avatar-large { width: 64px; height: 64px; border-radius: 50%; }
.username { font-weight: 800; font-size: 1.1rem; color: var(--text-main); cursor: pointer; transition: color 0.2s; }
.username.hover-link:hover { color: var(--accent-color); }
.user-status { font-size: 0.75rem; color: #10b981; display: flex; align-items: center; gap: 6px; margin-top: 4px; font-weight: 500; }
.status-dot { width: 8px; height: 8px; background: #10b981; border-radius: 50%; box-shadow: 0 0 8px rgba(16, 185, 129, 0.4); animation: pulse 2s infinite; }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

.action-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
.modern-btn { padding: 8px 12px; border-radius: 8px; font-size: 0.85rem; font-weight: 600; cursor: pointer; transition: all 0.2s; text-align: center; border: 1px solid transparent; }
.modern-btn.primary { background: var(--text-main); color: #fff; }
.modern-btn.primary:hover { background: #374151; }
.modern-btn.secondary { background: #fff; border-color: var(--border-color); color: var(--text-main); }
.modern-btn.secondary:hover { background: #f9fafb; border-color: #d1d5db; }
.modern-btn.secondary.success-state { background: #10b981; color: #fff; border-color: #10b981; }

/* 关联帖子 */
.linked-list { display: flex; flex-direction: column; }
.linked-item { padding: 12px 0; border-bottom: 1px solid var(--border-color); cursor: pointer; transition: padding-left 0.2s; }
.linked-item:last-child { border-bottom: none; padding-bottom: 0; }
.linked-item:hover { padding-left: 8px; }
.item-title { font-weight: 600; font-size: 0.9rem; color: var(--text-main); margin-bottom: 6px; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
.item-meta { font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; color: var(--text-muted); }
.empty-hint { color: #9ca3af; font-size: 0.85rem; text-align: center; padding: 20px 0; }

/* 数据看板 */
.metrics-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.metric-box { background: #f9fafb; border: 1px solid var(--border-color); border-radius: 8px; padding: 12px; text-align: center; display: flex; flex-direction: column; gap: 4px; }
.m-val { font-size: 1.4rem; font-weight: 800; color: var(--accent-color); font-family: 'JetBrains Mono', monospace; }
.m-lab { font-size: 0.75rem; color: var(--text-muted); font-weight: 600; }

/* --- 5. 全息检视器 (Lightbox) --- */
.sleek-lightbox { position: fixed; inset: 0; z-index: 9999; background: rgba(0, 0, 0, 0.85); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; cursor: zoom-out; }
.lightbox-image { max-width: 90vw; max-height: 90vh; border-radius: 8px; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5); object-fit: contain; cursor: default; transform: scale(1); transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1); }
.close-btn { position: absolute; top: 24px; right: 24px; background: rgba(255, 255, 255, 0.1); color: #fff; border: none; border-radius: 50%; width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all 0.2s; }
.close-btn:hover { background: rgba(255, 255, 255, 0.2); transform: scale(1.1); }
.fade-enter-active, .fade-leave-active { transition: opacity 0.25s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

/* 加载状态 */
.status-screen { height: 70vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-muted); }
.loading-spinner { width: 40px; height: 40px; border: 3px solid var(--border-color); border-top-color: var(--accent-color); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 20px; }
.loading-text { font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; }
@keyframes spin { to { transform: rotate(360deg); } }

/* --- 响应式 --- */
@media (max-width: 1024px) {
  .page-layout-grid { grid-template-columns: 1fr; margin-top: 20px; }
  .sidebar-column { position: relative; top: 0; }
  .action-grid { grid-template-columns: 1fr; }
}
@media (max-width: 768px) {
  .modern-page-wrapper { background: var(--card-bg); }
  .page-layout-grid { padding: 0; margin: 0; gap: 0; }
  .modern-card { border-radius: 0; border-left: none; border-right: none; box-shadow: none; border-top: none; }
  .post-header, .media-gallery, .post-body, .post-footer { padding-left: 20px; padding-right: 20px; }
  .media-grid.grid-3 .image-wrapper:not(:first-child) { aspect-ratio: 1; }
  .sidebar-widget { padding: 20px; border-bottom: 8px solid var(--bg-color); }
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #d1d5db; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #9ca3af; }
</style>