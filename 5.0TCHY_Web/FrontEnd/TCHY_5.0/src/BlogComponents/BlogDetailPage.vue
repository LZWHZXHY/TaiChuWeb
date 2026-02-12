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

        <section class="comment-section">
          <div class="section-title">
            <span class="icon">■</span> 评论 ({{ commentList.length }})
          </div>

          <div class="comment-input-box">
            <div v-if="replyTarget" class="reply-tip">
              REPLYING_TO: @{{ replyTarget.author?.name }}
              <button @click="replyTarget = null">[CANCEL]</button>
            </div>
            <textarea 
              v-model="commentContent" 
              class="cyber-textarea" 
              placeholder="输入你的见解... (Markdown Supported)"
            ></textarea>
            <div class="action-row">
              <button class="submit-btn" @click="handleSubmitComment" :disabled="!commentContent.trim()">
                PUBLISH_COMMENT >>
              </button>
            </div>
          </div>

          <div class="comment-list">
            <div v-if="loadingComments" class="status-text">LOADING_DATA...</div>
            <div v-else-if="commentList.length === 0" class="status-text">[ NO_COMMENTS_YET ]</div>
            
            <div 
              v-for="c in commentList" 
              :key="c.Id || c.id" 
              class="comment-item"
              :style="{ marginLeft: (c.level * 30) + 'px' }"
            >
              <div class="c-avatar">
                <img :src="formatUrl(c.author?.avatar)" @error="handleImgError" />
              </div>
              <div class="c-content-block">
                <div class="c-header">
                  <span class="c-name">{{ c.author?.name || '匿名用户' }}</span>
                  <span class="c-time">{{ formatTime(c.createTime || c.CreateTime) }}</span>
                  <button class="reply-btn" @click="handleReply(c)">[REPLY]</button>
                </div>
                <div class="c-text">{{ c.Content || c.content }}</div>
              </div>
            </div>
          </div>
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

const route = useRoute();
const router = useRouter();
const BASE_URL = 'https://bianyuzhou.com';

// --- 状态数据 ---
const blogData = ref(null);
const loading = ref(true);
const commentContent = ref('');
const commentList = ref([]);
const loadingComments = ref(false);
const replyTarget = ref(null);

// --- 1. 获取文章详情 ---
const fetchDetail = async () => {
  const id = route.params.id;
  if (!id) return;

  loading.value = true;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    blogData.value = res.data;
    // 获取到文章后加载评论
    fetchComments(id);
  } catch (e) {
    console.error("文章获取失败", e);
  } finally {
    loading.value = false;
  }
};

// --- 2. 获取评论列表 ---
const fetchComments = async (articleId) => {
  loadingComments.value = true;
  try {
    const res = await apiClient.get(`/Blog/comments/${articleId}`);
    if (res.status === 200) {
      // 在这里处理树状结构，并兼容大小写
      commentList.value = processCommentTree(res.data);
    }
  } catch (e) {
    console.error("加载评论失败", e);
  } finally {
    loadingComments.value = false;
  }
};

// --- 3. 提交评论 ---
const handleSubmitComment = async () => {
  if (!commentContent.value.trim() || !blogData.value) return;
  try {
    const payload = {
      ArticleId: blogData.value.id || blogData.value.Id,
      Content: commentContent.value,
      ParentId: replyTarget.value ? (replyTarget.value.Id || replyTarget.value.id) : 0
    };

    const res = await apiClient.post('/Blog/comments', payload);
    if (res.status === 200) {
      commentContent.value = '';
      replyTarget.value = null;
      fetchComments(blogData.value.id || blogData.value.Id);
    }
  } catch (e) {
    alert("评论失败: " + (e.response?.data || e.message));
  }
};

// --- 工具函数 ---

// 统一处理图片 URL
const formatUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

// 动态封面样式
const heroStyle = computed(() => {
  if (!blogData.value) return {};
  // 🔴 关键修复：兼容后端返回的 CoverImage (大写) 或 coverImage (小写)
  const cover = blogData.value.coverImage || blogData.value.CoverImage;
  const url = formatUrl(cover);
  return cover 
    ? { backgroundImage: `url(${url})` } 
    : { backgroundColor: '#111' };
});

const formatTime = (t) => t ? t.replace('T', ' ').substring(0, 16) : 'UNKNOWN';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const handleReply = (c) => {
  replyTarget.value = c;
  document.querySelector('.comment-input-box')?.scrollIntoView({ behavior: 'smooth' });
};

const goBack = () => { router.back(); };

// --- 🔴 关键修复：防御性评论树处理 ---
const processCommentTree = (list) => {
  if (!list || !Array.isArray(list)) return [];
  
  const map = {};
  const roots = [];

  // 第一次遍历：初始化并建立映射
  list.forEach(item => {
    const id = item.Id ?? item.id;
    item.children = [];
    item.level = 0;
    map[id] = item;
  });

  // 第二次遍历：挂载父子关系
  list.forEach(item => {
    const parentId = item.ParentId ?? item.parentId;
    if (parentId && map[parentId]) {
      map[parentId].children.push(item);
    } else {
      roots.push(item);
    }
  });

  const result = [];
  const traverse = (nodes, level) => {
    // 排序：按创建时间倒序
    nodes.sort((a, b) => {
      const timeA = new Date(a.createTime || a.CreateTime);
      const timeB = new Date(b.createTime || b.CreateTime);
      return timeB - timeA;
    });

    nodes.forEach(node => {
      node.level = level;
      result.push(node);
      if (node.children && node.children.length > 0) {
        traverse(node.children, level + 1);
      }
    });
  };

  traverse(roots, 0);
  return result;
};

onMounted(() => {
  fetchDetail();
});

watch(() => route.params.id, (newId) => {
  if(newId) fetchDetail();
});
</script>

<style scoped>
/* 样式部分保持你酷炫的赛博风格，未做变动 */
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

.section-title { font-family: 'Anton'; font-size: 1.8rem; margin-bottom: 25px; border-bottom: 3px solid var(--black); padding-bottom: 10px; color:#333}
.section-title .icon { color: var(--red); }

.comment-input-box { background: #fff; border: 2px solid var(--black); padding: 20px; box-shadow: 6px 6px 0 rgba(0,0,0,0.1); margin-bottom: 40px; }
.reply-tip { background: var(--red); color: #fff; padding: 5px 10px; font-size: 0.8rem; margin-bottom: 10px; display: flex; justify-content: space-between; }
.reply-tip button { background: none; border: none; color: #fff; cursor: pointer; font-weight: bold; }

.cyber-textarea { width: 100%; height: 100px; border: 1px solid #ccc; padding: 10px; font-family: inherit; font-size: 1rem; margin-bottom: 15px; outline: none; background: #fafafa; transition: 0.2s; color:#000}
.cyber-textarea:focus { border-color: var(--black); background: #fff; color:#000}

.submit-btn { background: var(--black); color: #fff; border: none; padding: 12px 25px; font-family: 'JetBrains Mono'; font-weight: bold; cursor: pointer; transition: 0.2s; float: right; }
.submit-btn:hover:not(:disabled) { background: var(--red); transform: translate(-2px, -2px); box-shadow: 3px 3px 0 var(--black); }
.submit-btn:disabled { background: #999; cursor: not-allowed; }

.comment-item { display: flex; gap: 15px; margin-bottom: 20px; padding-left: 10px; border-left: 2px solid #ddd; }
.c-avatar { width: 40px; height: 40px; flex-shrink: 0; border: 1px solid var(--black); }
.c-avatar img { width: 100%; height: 100%; object-fit: cover; }
.c-content-block { flex: 1; background: #fff; border: 1px solid #eee; padding: 10px 15px; }
.c-header { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; font-size: 0.85rem; }
.c-name { font-weight: bold; color: var(--black); }
.c-time { font-family: 'JetBrains Mono'; color: #999; font-size: 0.75rem; }
.reply-btn { margin-left: auto; border: none; background: none; color: var(--red); font-weight: bold; font-size: 0.75rem; cursor: pointer; font-family: 'JetBrains Mono'; }
.reply-btn:hover { text-decoration: underline; }
.c-text { font-size: 0.95rem; line-height: 1.5; color: #333; white-space: pre-wrap; }

.status-screen { height: 80vh; display: flex; flex-direction: column; align-items: center; justify-content: center; font-family: 'JetBrains Mono'; font-size: 1.2rem; }
.loading-spinner { width: 50px; height: 50px; border: 5px solid #ddd; border-top-color: var(--red); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 20px; }
@keyframes spin { to { transform: rotate(360deg); } }
.status-text{color:#333 }
</style>