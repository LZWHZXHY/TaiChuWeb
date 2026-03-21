<template>
  <div class="blog-detail-wrapper custom-scroll">
    
    <div class="wiki-drawer" :class="{ 'drawer-open': isDrawerOpen }">
      <div class="drawer-header">
        <div class="drawer-title">
          <span class="icon">🌌</span> 寰宇百科：{{ drawerTitle }}
        </div>
        
        <div class="drawer-actions">
          <button v-if="drawerArticleId" class="go-wiki-btn" @click="goToWikiPage">
            前往完整词条 ↗
          </button>
          <button class="close-drawer-btn" @click="isDrawerOpen = false">✕</button>
        </div>
      </div>
      <div class="drawer-body markdown-body" v-if="drawerLoading">
        <div class="loading-placeholder">正在连接灵脉空间读取节点...</div>
      </div>
      <div class="drawer-body markdown-body tech-markdown" v-else v-html="drawerContentHtml"></div>
    </div>

    <nav class="nav-bar">
      <div class="nav-container">
        <button class="back-btn" @click="goBack">
          <span class="icon">←</span> 返回列表
        </button>
        <span class="nav-title" v-if="blogData">{{ truncateText(blogData.title || blogData.Title, 20) }}</span>
      </div>
    </nav>

    <div v-if="loading" class="status-screen">
      <div class="loading-spinner"></div>
      <div class="loading-text">正在读取节点数据...</div>
    </div>

    <div v-else-if="!blogData" class="status-screen error">
      <div class="error-icon">404</div>
      <h2>档案丢失或已被销毁</h2>
      <p>您寻找的目标数据节点不存在，请检查索引标识。</p>
      <button class="primary-btn" @click="goBack">返回安全区域</button>
    </div>

    <main v-else class="content-container">
      
      <aside class="floating-actions">
        <button class="action-btn" title="点赞" @click="handleLike">
          <span class="icon">👍</span>
          <span class="count">{{ blogData.likeCount || blogData.LikeCount || 0 }}</span>
        </button>
        <button class="action-btn" title="前往评论区" @click="scrollToComments">
          <span class="icon">💬</span>
          <span class="count">{{ blogData.commentCount || blogData.CommentCount || 0 }}</span>
        </button>
        <button class="action-btn share" title="分享文章" @click="handleShare">
          <span class="icon">🔗</span>
        </button>
      </aside>

      <header class="article-header">
        <div class="meta-row">
          <span class="meta-tag">INTELLIGENCE_NODE</span>
          <span class="meta-date">{{ formatTime(blogData.updateTime || blogData.createTime || blogData.CreateTime) }}</span>
          <span class="meta-id">#{{ blogData.id || blogData.Id }}</span>
          <span class="meta-read-time">· 约 {{ readTime }} 分钟阅读 ({{ wordCount }} 字)</span>
        </div>

        <h1 class="article-title">{{ blogData.title || blogData.Title }}</h1>
        
        <div class="article-excerpt" v-if="blogData.summary || blogData.Summary || blogData.excerpt">
          {{ blogData.summary || blogData.Summary || blogData.excerpt }}
        </div>

        <div class="author-block">
          <div class="author-avatar-wrapper">
            <CommonAvatar 
              :userId="authorId" 
              :passedAvatar="formatUrl(blogData.author?.avatar)" 
              :showLevel="true" 
            />
          </div>
          <div class="author-info">
            <router-link :to="`/user/${authorId}`" class="author-name hover-link">
              {{ blogData.author?.name || '匿名节点' }}
            </router-link>
            <span class="author-desc">太初寰宇内容创作者</span>
          </div>
          <div class="article-stats">
            <span class="stat-pill">👁️ {{ blogData.viewCount || blogData.views || 0 }} 阅</span>
          </div>
        </div>
      </header>

      <div class="article-cover" v-if="blogData.coverImage || blogData.CoverImage">
        <img :src="formatUrl(blogData.coverImage || blogData.CoverImage)" alt="Blog Cover" />
      </div>

      <article class="article-body">
        <div class="markdown-body tech-markdown" @click="handleBodyClick" v-html="renderMarkdown(blogData.content || blogData.Content)"></div>
        
        <div class="tags-container" v-if="blogData.tags && blogData.tags.length">
          <span class="tag-label">TAGS:</span>
          <span v-for="tag in blogData.tags" :key="tag" class="tech-tag"># {{ tag }}</span>
        </div>

        <div class="end-mark">
          <hr class="line">
          <span class="text">END OF DOCUMENT</span>
          <hr class="line">
        </div>
      </article>

      <section class="comment-section" id="comments-section">
        <h3 class="section-title">高维讨论节点</h3>
        <UniversalComments 
          v-if="blogData && (blogData.id || blogData.Id)" 
          targetType="Blog" 
          :targetId="blogData.id || blogData.Id" 
        />
      </section>

    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { marked } from 'marked';
import { ElMessage } from 'element-plus';
import apiClient from '@/utils/api';

import UniversalComments from '@/GeneralComponents/UniversalComments.vue';
import CommonAvatar from '@/GeneralComponents/UserAvatar.vue'; 

const route = useRoute();
const router = useRouter();
const BASE_URL = 'https://bianyuzhou.com';

const blogData = ref(null);
const loading = ref(true);

// --- 抽屉状态 ---
const isDrawerOpen = ref(false);
const drawerLoading = ref(false);
const drawerContentHtml = ref('');
const drawerTitle = ref('');
const drawerArticleId = ref(null); // 🚀 新增：保存当前词条的 ID，用于跳转

const authorId = computed(() => {
  if (!blogData.value) return 0;
  return blogData.value.author?.id || blogData.value.AuthorId || blogData.value.authorId;
});

const wordCount = computed(() => {
  if (!blogData.value) return 0;
  const content = blogData.value.content || blogData.value.Content || '';
  return content.replace(/[#*`~_]/g, '').trim().length;
});

const readTime = computed(() => {
  return Math.max(1, Math.ceil(wordCount.value / 400));
});

const fetchDetail = async () => {
  const id = route.params.id;
  if (!id) return;
  loading.value = true;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    blogData.value = res.data.data || res.data;
  } catch (e) {
    console.error("文章获取失败", e);
  } finally {
    loading.value = false;
  }
};

const scrollToComments = () => {
  const el = document.getElementById('comments-section');
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
};

const handleLike = () => {
  ElMessage.success('信号已发送：赞！');
};

const handleShare = async () => {
  const url = window.location.href;
  try {
    await navigator.clipboard.writeText(url);
    ElMessage.success('档案链接已复制到剪贴板');
  } catch (err) {
    ElMessage.error('复制失败，请手动复制链接');
  }
};

const formatUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

const formatTime = (t) => {
  if (!t) return 'UNKNOWN TIME';
  const date = new Date(t);
  return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`;
};

// --- 核心：解析 Markdown 中的 [[词条名]] ---
const renderMarkdown = (rawText) => {
  if (!rawText || typeof rawText !== 'string') return '';
  
  try {
    let processedText = rawText.replace(/\[\[([^\]\s][^\]]*?)\]\]/g, (match, title) => {
      return `<div class="wiki-embed-card" data-wiki-title="${title.trim()}"><div class="card-left">🌌</div><div class="card-center"><div class="card-title">${title.trim()}</div><div class="card-subtitle">点击检索寰宇百科</div></div><div class="card-right">查看词条</div></div>`;
    });

    return marked.parse(processedText);
  } catch (e) {
    return rawText;
  }
};

// --- 核心：监听正文内的点击事件，呼出百科抽屉 ---
const handleBodyClick = async (event) => {
  const card = event.target.closest('.wiki-embed-card');
  if (card) {
    const title = card.dataset.wikiTitle;
    drawerTitle.value = title;
    drawerArticleId.value = null; // 🚀 每次打开前先重置 ID
    isDrawerOpen.value = true;
    drawerLoading.value = true;
    
    try {
      const res = await apiClient.get(`/Wiki/article/by-title/${encodeURIComponent(title)}`);
      
      console.log("百科词条接口返回:", res);

      const finalData = res.data || res;
      
      // 🚀 获取词条 ID，供跳转使用
      drawerArticleId.value = finalData.id || finalData.Id;
      
      let mdContent = finalData.rawMarkdown || finalData.RawMarkdown || finalData.content || finalData.Content;

      if (!mdContent && finalData.blocks && finalData.blocks.length > 0) {
          mdContent = finalData.blocks.map(b => b.Content || b.content).join('\n\n');
      }

      if (!mdContent) {
          mdContent = '未能成功提取词条内容，请检查控制台数据格式。';
      }
      
      drawerContentHtml.value = marked.parse(mdContent);
      
    } catch (err) {
      console.error("百科词条加载异常:", err);
      drawerContentHtml.value = '<p style="color:#ef4444; padding: 20px;">未在灵脉空间中找到该词条，节点可能已被销毁或尚未创建。</p>';
    } finally {
      drawerLoading.value = false;
    }
  }
};

// 🚀 新增：跳转到百科详情页的方法
const goToWikiPage = () => {
  if (drawerArticleId.value) {
    router.push(`/wiki/${drawerArticleId.value}`);
  }
};

const goBack = () => { router.back(); };
const truncateText = (text, len) => (text && text.length > len) ? text.substring(0, len) + '...' : text;

onMounted(() => { fetchDetail(); });
watch(() => route.params.id, (newId) => { if(newId) fetchDetail(); });
</script>

<style scoped>
/* --- 全局与变量 --- */
.blog-detail-wrapper {
  --bg-main: #ffffff;
  --bg-offset: #f8f9fa;
  --text-main: #1a1a1a;
  --text-muted: #6b7280;
  --border-color: #eaeaea;
  --accent: #0047ff; 
  
  background: var(--bg-offset);
  min-height: 100vh;
  font-family: 'Inter', -apple-system, sans-serif;
  color: var(--text-main);
  padding-bottom: 80px;
  position: relative;
  overflow-x: hidden;
}

/* --- 百科预览抽屉样式 --- */
.wiki-drawer {
  position: fixed; top: 0; right: -600px; width: 550px; height: 100vh;
  background: #ffffff; box-shadow: -10px 0 30px rgba(0,0,0,0.15);
  z-index: 2000; transition: right 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex; flex-direction: column;
}
.wiki-drawer.drawer-open { right: 0; }
.drawer-header {
  padding: 16px 24px; border-bottom: 1px solid var(--border-color);
  display: flex; justify-content: space-between; align-items: center; background: #fafafa;
}
.drawer-title { display: flex; align-items: center; gap: 8px; font-weight: 700; color: var(--text-main); font-size: 16px; }

/* 🚀 新增跳转按钮及操作区样式 */
.drawer-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}
.go-wiki-btn {
  background: rgba(0, 71, 255, 0.08);
  color: var(--accent);
  border: 1px solid rgba(0, 71, 255, 0.15);
  padding: 6px 14px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
}
.go-wiki-btn:hover {
  background: rgba(0, 71, 255, 0.15);
  transform: translateY(-1px);
}

.close-drawer-btn { border: none; background: transparent; font-size: 22px; cursor: pointer; color: var(--text-muted); transition: color 0.2s; }
.close-drawer-btn:hover { color: #ef4444; }
.drawer-body { flex: 1; overflow-y: auto; padding: 32px; line-height: 1.8; }
.loading-placeholder { text-align: center; color: var(--text-muted); margin-top: 100px; font-family: 'JetBrains Mono', monospace;}

/* --- 嵌入的百科卡片样式 --- */
:deep(.wiki-embed-card) {
  display: flex; align-items: center; gap: 16px; padding: 18px 24px;
  background: #ffffff; border: 1px solid var(--border-color); border-left: 4px solid var(--accent);
  border-radius: 12px; margin: 24px 0; cursor: pointer; transition: all 0.2s;
  box-shadow: 0 2px 10px rgba(0,0,0,0.02);
}
:deep(.wiki-embed-card:hover) { 
  transform: translateX(5px); box-shadow: 0 4px 20px rgba(0, 71, 255, 0.1); border-color: var(--accent); 
}
:deep(.card-left) { font-size: 24px; }
:deep(.card-center) { flex: 1; }
:deep(.card-title) { font-weight: 700; color: var(--text-main); font-size: 16px; margin-bottom: 4px; }
:deep(.card-subtitle) { font-size: 12px; color: var(--text-muted); }
:deep(.card-right) { font-size: 12px; font-weight: 600; color: var(--accent); background: rgba(0, 71, 255, 0.08); padding: 6px 14px; border-radius: 20px; }

/* --- 基础与框架保留样式 --- */
.nav-bar { position: sticky; top: 0; z-index: 100; height: 60px; background: rgba(255, 255, 255, 0.9); backdrop-filter: blur(12px); border-bottom: 1px solid var(--border-color); }
.nav-container { max-width: 800px; margin: 0 auto; height: 100%; display: flex; align-items: center; padding: 0 20px; gap: 20px; }
.back-btn { background: transparent; border: none; color: var(--text-muted); font-size: 0.95rem; font-weight: 500; cursor: pointer; display: flex; align-items: center; gap: 6px; padding: 0; transition: color 0.2s; }
.back-btn:hover { color: var(--text-main); }
.nav-title { color: var(--text-muted); font-size: 0.9rem; border-left: 1px solid var(--border-color); padding-left: 20px; }
.content-container { position: relative; max-width: 800px; margin: 40px auto 0; background: var(--bg-main); border-radius: 12px; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.04); border: 1px solid var(--border-color); }
.floating-actions { position: absolute; left: -80px; top: 100px; display: flex; flex-direction: column; gap: 15px; }
.action-btn { width: 48px; height: 48px; border-radius: 50%; border: 1px solid var(--border-color); background: var(--bg-main); display: flex; flex-direction: column; align-items: center; justify-content: center; cursor: pointer; transition: all 0.2s; color: var(--text-muted); box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
.action-btn:hover { border-color: var(--text-main); color: var(--text-main); transform: translateY(-2px); }
.action-btn .icon { font-size: 1.1rem; }
.action-btn .count { font-size: 0.7rem; font-family: 'JetBrains Mono', monospace; font-weight: bold; margin-top: -2px; }
.action-btn.share .icon { font-size: 1.3rem; margin-top: 2px; }
.article-header { padding: 50px 50px 30px; }
.meta-row { display: flex; align-items: center; gap: 12px; font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; color: var(--text-muted); margin-bottom: 20px; flex-wrap: wrap; }
.meta-tag { color: var(--accent); font-weight: 700; background: rgba(0, 71, 255, 0.08); padding: 4px 8px; border-radius: 4px; }
.article-title { font-size: 2.6rem; font-weight: 800; line-height: 1.3; margin: 0 0 20px 0; color: var(--text-main); letter-spacing: -0.5px; }
.article-excerpt { font-size: 1.05rem; line-height: 1.7; color: #555; background: var(--bg-offset); border-left: 4px solid var(--accent); padding: 16px 20px; margin-bottom: 30px; border-radius: 0 8px 8px 0; }
.author-block { display: flex; align-items: center; gap: 16px; padding-top: 20px; border-top: 1px solid var(--border-color); }
.author-avatar-wrapper { width: 45px; height: 45px; }
.author-info { display: flex; flex-direction: column; flex-grow: 1; }
.author-name { font-weight: 600; font-size: 1rem; color: var(--text-main); text-decoration: none; }
.author-name.hover-link:hover { color: var(--accent); text-decoration: underline; }
.author-desc { font-size: 0.85rem; color: var(--text-muted); margin-top: 2px; }
.stat-pill { background: var(--bg-offset); padding: 4px 10px; border-radius: 20px; font-weight: 600; }
.article-cover { width: 100%; max-height: 450px; overflow: hidden; background: #f0f0f0; }
.article-cover img { width: 100%; height: 100%; object-fit: cover; display: block; }
.article-body { padding: 40px 50px 50px; }
.tech-markdown { font-size: 1.125rem; line-height: 1.8; color: #333; }
.tech-markdown :deep(h1), .tech-markdown :deep(h2) { font-weight: 700; margin-top: 2.5em; margin-bottom: 1em; line-height: 1.3; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3em; }
.tech-markdown :deep(h3) { font-weight: 600; margin-top: 2em; margin-bottom: 0.8em; }
.tech-markdown :deep(p) { margin-bottom: 1.5em; }
.tech-markdown :deep(img) { max-width: 100%; border-radius: 8px; margin: 2em auto; display: block; box-shadow: 0 4px 12px rgba(0,0,0,0.08); }
.tech-markdown :deep(blockquote) { margin: 2em 0; padding: 1em 1.5em; background: var(--bg-offset); border-left: 4px solid var(--text-muted); color: #555; border-radius: 0 8px 8px 0; font-style: italic; }
.tech-markdown :deep(code) { background: var(--bg-offset); padding: 0.2em 0.4em; border-radius: 4px; font-family: 'JetBrains Mono', monospace; font-size: 0.85em; color: #d10000; }
.tech-markdown :deep(pre) { background: #1e1e1e; padding: 1.5em; border-radius: 8px; overflow-x: auto; margin: 2em 0; }
.tech-markdown :deep(pre code) { background: transparent; color: #e5e5e5; padding: 0; }
.tags-container { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin-top: 50px; }
.tag-label { font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; font-weight: 700; color: var(--text-muted); }
.tech-tag { background: var(--bg-offset); border: 1px solid var(--border-color); padding: 6px 12px; border-radius: 20px; font-size: 0.85rem; color: var(--text-main); transition: all 0.2s; }
.tech-tag:hover { background: var(--text-main); color: #fff; }
.end-mark { display: flex; align-items: center; gap: 20px; margin-top: 60px; opacity: 0.5; }
.end-mark .line { flex-grow: 1; height: 1px; background: var(--border-color); border: none; }
.end-mark .text { font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; letter-spacing: 1px; }
.comment-section { padding: 0 50px 50px; background: var(--bg-main); border-top: 1px solid var(--border-color); }
.section-title { font-size: 1.25rem; font-weight: 700; margin: 40px 0 20px; }
.status-screen { height: 70vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-muted); }
.loading-spinner { width: 40px; height: 40px; border: 3px solid var(--border-color); border-top-color: var(--accent); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 20px; }
.loading-text { font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; }
@keyframes spin { to { transform: rotate(360deg); } }
.status-screen.error { color: var(--text-main); }
.error-icon { font-size: 5rem; font-weight: 900; font-family: 'JetBrains Mono', monospace; color: var(--border-color); line-height: 1; margin-bottom: 10px;}
.primary-btn { margin-top: 20px; background: var(--text-main); color: #fff; border: none; padding: 10px 24px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: 0.2s; }
.primary-btn:hover { background: var(--accent); }

@media (max-width: 1000px) {
  .floating-actions { position: fixed; left: 0; bottom: 0; top: auto; flex-direction: row; width: 100%; justify-content: space-around; background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px); padding: 10px 0; border-top: 1px solid var(--border-color); z-index: 99; }
  .action-btn { border: none; background: transparent; box-shadow: none; }
  .content-container { margin-bottom: 60px; }
}
@media (max-width: 768px) {
  .content-container { margin-top: 0; border-radius: 0; border: none; box-shadow: none; }
  .article-header { padding: 30px 20px 20px; }
  .article-title { font-size: 2rem; }
  .article-body { padding: 30px 20px; }
  .comment-section { padding: 0 20px 30px; }
  .nav-title { display: none; }
}
</style>