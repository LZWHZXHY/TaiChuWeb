<template>
  <div class="blog-detail-wrapper custom-scroll">
    <div class="nav-bar">
      <button class="back-btn" @click="goBack">
        &lt;&lt; BACK_TO_NEXUS // 返回
      </button>
    </div>

    <div v-if="loading" class="status-screen">
      <div class="loading-spinner"></div>
      <div>DOWNLOADING_DATA_STREAM...</div>
    </div>

    <div v-else-if="!blogData" class="status-screen error">
      <h1>404 // DATA_NOT_FOUND</h1>
      <p>目标档案不存在或已被删除</p>
      <button class="back-btn" @click="goBack">RETURN</button>
    </div>

    <div v-else class="content-container">
      
      <div class="hero-section" :style="heroStyle">
        <div class="hero-overlay"></div>
        <div class="hero-content">
          <div class="meta-badges">
            <span class="badge type">BLOG_ARCHIVE</span>
            <span class="badge id">ID: #{{ blogData.id || blogData.Id }}</span>
            <span class="badge date">{{ formatTime(blogData.updateTime || blogData.createTime || blogData.CreateTime) }}</span>
          </div>
          
          <h1 class="hero-title">{{ blogData.title || blogData.Title }}</h1>
          
          <div class="author-row">
            <div class="avatar-box">
              <img :src="formatUrl(blogData.author?.avatar)" @error="handleImgError" />
            </div>
            <div class="author-info">
              <span class="name">@{{ blogData.author?.name || 'UNKNOWN' }}</span>
              <span class="role">AUTHOR_LEVEL: CONTRIBUTOR</span>
            </div>
          </div>
        </div>
      </div>

      <div class="main-content-area">
        <article class="detail-body">
          <div class="markdown-body cyber-markdown" v-html="renderMarkdown(blogData.content || blogData.Content)"></div>
          
          <div class="tags-row" v-if="blogData.tags && blogData.tags.length">
            <span class="tag-icon">TAGS:</span>
            <span v-for="tag in blogData.tags" :key="tag" class="cyber-tag">#{{ tag }}</span>
          </div>

          <div class="end-divider">
            <div class="line"></div>
            <span>END_OF_DOCUMENT</span>
            <div class="line"></div>
          </div>
        </article>

        <section class="comment-zone">
          <UniversalComments 
            v-if="blogData && (blogData.id || blogData.Id)" 
            targetType="Blog" 
            :targetId="blogData.id || blogData.Id" 
          />
        </section>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { marked } from 'marked';
import apiClient from '@/utils/api';

// ✅ 引入通用组件
import UniversalComments from '@/GeneralComponents/UniversalComments.vue';

const route = useRoute();
const router = useRouter();
const BASE_URL = 'https://bianyuzhou.com';

// --- 状态数据 ---
const blogData = ref(null);
const loading = ref(true);

// --- 1. 获取文章详情 ---
const fetchDetail = async () => {
  const id = route.params.id;
  if (!id) return;

  loading.value = true;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    blogData.value = res.data;
  } catch (e) {
    console.error("文章获取失败", e);
  } finally {
    loading.value = false;
  }
};

// --- 工具函数 ---

const formatUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

const heroStyle = computed(() => {
  if (!blogData.value) return {};
  const cover = blogData.value.coverImage || blogData.value.CoverImage;
  const url = formatUrl(cover);
  return cover 
    ? { backgroundImage: `url(${url})` } 
    : { backgroundColor: '#111' };
});

const formatTime = (t) => t ? t.replace('T', ' ').substring(0, 16) : 'UNKNOWN';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const goBack = () => { router.back(); };

onMounted(() => {
  fetchDetail();
});

watch(() => route.params.id, (newId) => {
  if(newId) fetchDetail();
});
</script>

<style scoped>
/* 赛博风格样式 */
.blog-detail-wrapper {
  --red: #D92323;
  --black: #111111;
  --bg: #F4F1EA;
  background: var(--bg);
  min-height: 100vh;
  position: relative;
  font-family: 'Inter', sans-serif;
}

.nav-bar {
  position: sticky; top: 0; z-index: 100;
  height: 60px;
  background: rgba(17, 17, 17, 0.95);
  backdrop-filter: blur(10px);
  display: flex; align-items: center; padding: 0 20px;
  border-bottom: 2px solid var(--red);
}
.back-btn {
  background: transparent; border: 1px solid #fff; color: #fff;
  padding: 6px 16px; font-family: 'JetBrains Mono', monospace;
  cursor: pointer; transition: 0.2s; font-size: 0.9rem;
}
.back-btn:hover { background: var(--red); border-color: var(--red); }

.hero-section {
  height: 45vh;
  background-size: cover; background-position: center;
  position: relative;
  border-bottom: 4px solid var(--red);
  display: flex; align-items: flex-end;
}
.hero-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(to bottom, rgba(0,0,0,0.3), rgba(0,0,0,0.9));
}
.hero-content {
  position: relative; z-index: 2;
  width: 100%; max-width: 1000px; margin: 0 auto;
  padding: 40px; color: #fff;
}
.meta-badges { display: flex; gap: 10px; margin-bottom: 15px; }
.badge { background: var(--red); color: #fff; padding: 2px 8px; font-family: 'JetBrains Mono'; font-size: 0.75rem; font-weight: bold; }
.badge.id { background: transparent; border: 1px solid #fff; }
.badge.date { background: #000; color: #fff; }

.hero-title {
  font-family: 'Anton', sans-serif; font-size: 3.5rem;
  text-transform: uppercase; line-height: 1.1; margin: 0 0 25px 0;
  text-shadow: 2px 2px 0 var(--red);
}

.author-row { display: flex; align-items: center; gap: 15px; }
.avatar-box { width: 50px; height: 50px; border: 2px solid #fff; overflow: hidden; }
.avatar-box img { width: 100%; height: 100%; object-fit: cover; }
.author-info { display: flex; flex-direction: column; }
.author-info .name { font-weight: bold; font-size: 1.1rem; }
.author-info .role { font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #ccc; }

.main-content-area { max-width: 900px; margin: 0 auto; padding: 60px 20px; }
.cyber-markdown { font-size: 1.15rem; line-height: 1.8; color: #222; }
.cyber-markdown :deep(h2) { border-left: 6px solid var(--red); padding-left: 15px; margin-top: 40px; font-family: 'Anton'; text-transform: uppercase; }
.cyber-markdown :deep(p) { margin-bottom: 20px; }
.cyber-markdown :deep(img) { max-width: 100%; border: 2px solid var(--black); box-shadow: 5px 5px 0 rgba(0,0,0,0.1); }

.tags-row { margin-top: 40px; display: flex; gap: 10px; align-items: center; }
.tag-icon { font-weight: bold; font-family: 'JetBrains Mono'; color:black}
.cyber-tag { background: #eee; border: 1px solid var(--black); padding: 4px 10px; font-family: 'JetBrains Mono'; font-size: 0.8rem; color:#111111}

.end-divider { 
  display: flex; align-items: center; gap: 20px; margin: 60px 0; 
  color: #999; font-family: 'JetBrains Mono'; font-size: 0.8rem;
}
.end-divider .line { flex: 1; height: 1px; background: #ccc; }

/* 评论区容器 */
.comment-zone {
  margin-top: 60px;
}

.status-screen { height: 80vh; display: flex; flex-direction: column; align-items: center; justify-content: center; font-family: 'JetBrains Mono'; font-size: 1.2rem; }
.loading-spinner { width: 50px; height: 50px; border: 5px solid #ddd; border-top-color: var(--red); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 20px; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>