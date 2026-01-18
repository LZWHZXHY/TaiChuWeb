<template>
  <div class="blog-detail-wrapper">
    <div class="hero-section" :style="heroStyle">
      <div class="hero-overlay"></div>
      <div class="hero-content">
        <div class="meta-badges">
          <span class="badge type">BLOG_ARCHIVE</span>
          <span class="badge id">ID: #{{ data.id }}</span>
          <span class="badge date">{{ formatTime(data.updateTime || data.createTime) }}</span>
        </div>
        
        <h1 class="hero-title">{{ data.title }}</h1>
        
        <div class="author-row">
          <div class="avatar-box">
            <img :src="fixAvatar(data.author?.avatar)" @error="handleImgError" />
          </div>
          <div class="author-info">
            <span class="name">@{{ data.author?.name || 'UNKNOWN' }}</span>
            <span class="role">AUTHOR_LEVEL: CONTRIBUTOR</span>
          </div>
        </div>
      </div>
    </div>

    <div class="main-content-area">
      <article class="detail-body">
        <div class="markdown-body cyber-markdown" v-html="renderMarkdown(data.content)"></div>
        
        <div class="tags-row" v-if="data.tags && data.tags.length">
          <span class="tag-icon">TAGS:</span>
          <span v-for="tag in data.tags" :key="tag" class="cyber-tag">#{{ tag }}</span>
        </div>

        <div class="end-divider">
          <div class="line"></div>
          <span>END_OF_DOCUMENT</span>
          <div class="line"></div>
        </div>
      </article>

      <section class="comment-section">
        <div class="section-title">
          <span class="icon">■</span> NEURAL_COMMENTS ({{ commentList.length }})
        </div>

        <div class="comment-input-box">
          <div v-if="replyTarget" class="reply-tip">
            REPLYING_TO: @{{ replyTarget.author.name }}
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
            :key="c.id" 
            class="comment-item"
            :style="{ marginLeft: (c.level * 30) + 'px' }"
          >
            <div class="c-avatar">
              <img :src="fixAvatar(c.author.avatar)" @error="handleImgError" />
            </div>
            <div class="c-content-block">
              <div class="c-header">
                <span class="c-name">{{ c.author.name }}</span>
                <span class="c-time">{{ c.createTime }}</span>
                <button class="reply-btn" @click="replyTarget = c">[REPLY]</button>
              </div>
              <div class="c-text">{{ c.content }}</div>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { marked } from 'marked';
import apiClient from '@/utils/api'; // 确保引入了你的 axios 实例

const props = defineProps(['data', 'fixAvatar']);

// 状态
const commentContent = ref('');
const commentList = ref([]);
const loadingComments = ref(false);
const replyTarget = ref(null);

// 背景图处理
const heroStyle = computed(() => {
  const url = props.fixAvatar(props.data.coverImage);
  return url && url !== '/土豆.jpg' 
    ? { backgroundImage: `url(${url})` } 
    : { backgroundColor: '#111' }; // 无图时显示纯黑
});

// 工具函数
const formatTime = (t) => t ? new Date(t).toLocaleString() : 'UNKNOWN_TIME';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

// --- 核心：获取评论列表 ---
const fetchComments = async () => {
  if (!props.data.id) return;
  loadingComments.value = true;
  try {
    // 调用后端: GET /api/Blog/comments/{articleId}
    const res = await apiClient.get(`/Blog/comments/${props.data.id}`);
    if (res.status === 200) {
      // 后端返回的是扁平数组，我们需要在前端把它转成树形结构以便缩进显示
      commentList.value = processCommentTree(res.data);
    }
  } catch (e) {
    console.error("加载评论失败", e);
  } finally {
    loadingComments.value = false;
  }
};

// --- 核心：提交评论 ---
const handleSubmitComment = async () => {
  if (!commentContent.value.trim()) return;
  try {
    const payload = {
      ArticleId: props.data.id,
      Content: commentContent.value,
      ParentId: replyTarget.value ? replyTarget.value.id : 0
    };

    // 调用后端: POST /api/Blog/comments
    const res = await apiClient.post('/Blog/comments', payload);
    
    if (res.status === 200) {
      alert("评论发布成功");
      commentContent.value = '';
      replyTarget.value = null;
      fetchComments(); // 刷新列表
    }
  } catch (e) {
    alert("评论失败: " + (e.response?.data || e.message));
  }
};

// --- 辅助：将扁平评论转为带层级的显示列表 ---
const processCommentTree = (list) => {
  if (!list) return [];
  const map = {};
  const roots = [];
  
  // 1. 初始化 Map
  list.forEach(item => {
    item.children = [];
    item.level = 0;
    map[item.id] = item;
  });

  // 2. 构建树
  list.forEach(item => {
    if (item.parentId && map[item.parentId]) {
      map[item.parentId].children.push(item);
    } else {
      roots.push(item);
    }
  });

  // 3. 拍平树为列表 (以便 v-for 渲染并用 padding 做缩进)
  const result = [];
  const traverse = (nodes, level) => {
    // 按时间倒序
    nodes.sort((a, b) => new Date(b.createTime) - new Date(a.createTime));
    nodes.forEach(node => {
      node.level = level;
      result.push(node);
      if (node.children.length > 0) {
        traverse(node.children, level + 1);
      }
    });
  };
  traverse(roots, 0);
  return result;
};

// 生命周期
onMounted(() => {
  fetchComments();
});

// 如果 props.data 变化（比如切换文章），重新加载评论
watch(() => props.data.id, () => {
  fetchComments();
});
</script>

<style scoped>
/* 全局变量 */
.blog-detail-wrapper {
  --red: #D92323;
  --black: #111111;
  --bg: #F4F1EA;
  background: var(--bg);
  min-height: 100%;
}

/* 1. Hero 头部 */
.hero-section {
  height: 45vh;
  background-size: cover;
  background-position: center;
  position: relative;
  border-bottom: 4px solid var(--red);
  display: flex;
  align-items: flex-end;
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
  font-family: 'Anton', sans-serif;
  font-size: 3.5rem;
  text-transform: uppercase;
  line-height: 1.1;
  margin: 0 0 25px 0;
  text-shadow: 2px 2px 0 var(--red);
}

.author-row { display: flex; align-items: center; gap: 15px; }
.avatar-box { width: 50px; height: 50px; border: 2px solid #fff; overflow: hidden; }
.avatar-box img { width: 100%; height: 100%; object-fit: cover; }
.author-info { display: flex; flex-direction: column; }
.author-info .name { font-weight: bold; font-size: 1.1rem; }
.author-info .role { font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #ccc; }

/* 2. 内容区 */
.main-content-area {
  max-width: 900px; margin: 0 auto;
  padding: 60px 20px;
}

.cyber-markdown {
  font-size: 1.15rem; line-height: 1.8; color: #222;
}
/* Markdown 样式微调 */
.cyber-markdown :deep(h2) { border-left: 6px solid var(--red); padding-left: 15px; margin-top: 40px; font-family: 'Anton'; text-transform: uppercase; }
.cyber-markdown :deep(p) { margin-bottom: 20px; }
.cyber-markdown :deep(img) { max-width: 100%; border: 2px solid var(--black); box-shadow: 5px 5px 0 rgba(0,0,0,0.1); }
.cyber-markdown :deep(blockquote) { border-left: 4px solid var(--black); background: #e5e5e5; padding: 10px 20px; font-style: italic; }

.tags-row { margin-top: 40px; display: flex; gap: 10px; align-items: center; }
.tag-icon { font-weight: bold; font-family: 'JetBrains Mono'; }
.cyber-tag { background: #eee; border: 1px solid var(--black); padding: 4px 10px; font-family: 'JetBrains Mono'; font-size: 0.8rem; }

.end-divider { 
  display: flex; align-items: center; gap: 20px; margin: 60px 0; 
  color: #999; font-family: 'JetBrains Mono'; font-size: 0.8rem;
}
.end-divider .line { flex: 1; height: 1px; background: #ccc; }

/* 3. 评论区 */
.comment-section { margin-top: 40px; }
.section-title { 
  font-family: 'Anton'; font-size: 1.8rem; margin-bottom: 25px; 
  border-bottom: 3px solid var(--black); padding-bottom: 10px;
}
.section-title .icon { color: var(--red); }

/* 输入框 */
.comment-input-box { background: #fff; border: 2px solid var(--black); padding: 20px; box-shadow: 6px 6px 0 rgba(0,0,0,0.1); margin-bottom: 40px; }
.reply-tip { background: var(--red); color: #fff; padding: 5px 10px; font-size: 0.8rem; margin-bottom: 10px; display: flex; justify-content: space-between; }
.reply-tip button { background: none; border: none; color: #fff; cursor: pointer; font-weight: bold; }

.cyber-textarea { 
  width: 100%; height: 100px; border: 1px solid #ccc; padding: 10px; 
  font-family: inherit; font-size: 1rem; margin-bottom: 15px; outline: none; 
  background: #fafafa; transition: 0.2s;
}
.cyber-textarea:focus { border-color: var(--black); background: #fff; }

.submit-btn { 
  background: var(--black); color: #fff; border: none; 
  padding: 12px 25px; font-family: 'JetBrains Mono'; font-weight: bold; 
  cursor: pointer; transition: 0.2s; float: right;
}
.submit-btn:hover:not(:disabled) { background: var(--red); transform: translate(-2px, -2px); box-shadow: 3px 3px 0 var(--black); }
.submit-btn:disabled { background: #999; cursor: not-allowed; }
.action-row::after { content: ""; display: block; clear: both; }

/* 评论列表 */
.status-text { text-align: center; color: #888; font-family: 'JetBrains Mono'; margin: 40px 0; }

.comment-item { 
  display: flex; gap: 15px; margin-bottom: 20px; 
  padding-left: 10px; border-left: 2px solid #ddd;
}
.c-avatar { width: 40px; height: 40px; flex-shrink: 0; border: 1px solid var(--black); }
.c-avatar img { width: 100%; height: 100%; object-fit: cover; }

.c-content-block { flex: 1; background: #fff; border: 1px solid #eee; padding: 10px 15px; }
.c-header { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; font-size: 0.85rem; }
.c-name { font-weight: bold; color: var(--black); }
.c-time { font-family: 'JetBrains Mono'; color: #999; font-size: 0.75rem; }
.reply-btn { margin-left: auto; border: none; background: none; color: var(--red); font-weight: bold; font-size: 0.75rem; cursor: pointer; font-family: 'JetBrains Mono'; }
.reply-btn:hover { text-decoration: underline; }

.c-text { font-size: 0.95rem; line-height: 1.5; color: #333; }
</style>