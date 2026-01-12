<template>
  <div class="tc-dashboard">
    <header class="sys-info-bar md-elevation-1">
      <div class="sys-status">
        <span class="pulse"></span>
        <span class="status-text">TAICHU_NETWORK // STATUS: OPTIMAL</span>
      </div>
      <div class="sys-time">{{ currentTime }}</div>
    </header>

    <div class="main-bridge">
      
      <aside class="rank-column">
        <ActivityRanking />
      </aside>

      <aside class="left-column">
        <section class="panel-card md-elevation-1 full-height">
          <div class="card-header">
            <span class="header-label">RECOMMENDED_BLOGS</span>
          </div>
          <div class="blog-min-list custom-scroll">
            <div v-if="blogLoading" class="empty-hint">同步档案中...</div>
            <div v-else-if="blogs.length === 0" class="empty-hint">暂无博客文章...</div>
            
            <div v-for="b in blogs" :key="b.Id" class="min-blog-item md-ripple" @click="openBlogDetail(b.Id)">
              <div v-if="b.CoverImage" class="min-blog-thumb">
                <img :src="fixAvatarUrl(b.CoverImage)" @error="handleImgError" loading="lazy" />
              </div>
              
              <div class="blog-content">
                <div class="blog-top-row">
                  <span class="blog-tag"># {{ (b.Tags && b.Tags[0]) || '太初' }}</span>
                  <div class="blog-accent" v-if="b.isTop">TOP</div>
                </div>
                <p class="blog-title">{{ b.Title }}</p>
              </div>
            </div>
          </div>
        </section>
      </aside>

      <main class="mid-column">
        <section class="activity-slider md-elevation-2">
          <div class="slider-content">
            <div class="slider-badge">ANNOUNCEMENT</div>
            <h2 class="slider-title">太初宇宙 5.0 官网开发进行中...</h2>
            <p class="slider-sub">NODE_STATUS: SYNCING // 2026.01.09</p>
          </div>
          <div class="slider-bg-effect"></div>
        </section>

        <div class="mid-content-split">
          <section class="posts-flow-container md-elevation-1">
            <div class="flow-header">
              <span class="header-title">COMMUNITY_POSTS</span>
              <div class="header-tabs">
                <span class="tab">最新</span>
                <span class="tab">热门</span>
              </div>
            </div>
            
            <div class="posts-scroll-area custom-scroll" @scroll="handleScroll">
              <div v-if="loading && posts.length === 0" class="empty-hint">接入信号中...</div>
              
              <div v-for="p in posts" :key="p.id" class="post-entry md-ripple" @click="openPostDetail(p.id)">
                <div class="entry-main">
                  <div class="entry-user-row">
                    <img :src="fixAvatarUrl(p.author?.avatar)" @error="handleImgError" class="user-avatar-sm" />
                    <div class="user-info">
                      <span class="username">@{{ p.author?.username || 'USER' }}</span>
                      <span class="post-time">{{ formatTime(p.createTime) }}</span>
                    </div>
                  </div>
                  <h3 class="entry-title">{{ p.post_title }}</h3>
                  <div class="entry-actions">
                    <span class="action-item"><i class="far fa-comment-alt"></i> {{ p.comment_count }}</span>
                    <span class="action-item"><i class="far fa-eye"></i> {{ p.view_count }}</span>
                  </div>
                </div>
                <div v-if="p.images?.length" class="entry-thumb-wrapper">
                  <img :src="p.images[0]" @error="handleImgError" class="entry-thumb" />
                  <span v-if="p.images.length > 1" class="img-count">+{{ p.images.length }}</span>
                </div>
              </div>
              
              <div v-if="loading" class="loading-status">
                <i class="fas fa-circle-notch fa-spin"></i> SYNCING_STREAM...
              </div>
            </div>
          </section>

          <aside class="action-sidebar">
            <div class="section-label">QUICK_ACTIONS</div>
            <div class="action-grid">
              <button class="btn-action primary md-elevation-1 md-ripple" @click="router.push('/blogCreater')">
                <div class="icon-box">●<i class="fas fa-edit"></i></div>
                <div class="btn-text"><span class="main">发布博客</span><span class="sub">Post Blog</span></div>
              </button>
              <button class="btn-action secondary md-elevation-1 md-ripple" @click="showPostForm = true">
                <div class="icon-box">●<i class="fas fa-paper-plane"></i></div>
                <div class="btn-text"><span class="main">发布帖子</span><span class="sub">Post Thread</span></div>
              </button>
              <button class="btn-action info md-elevation-1 md-ripple" @click="router.push('/suggest')">
                <div class="icon-box">●<i class="fas fa-comment-dots"></i></div>
                <div class="btn-text"><span class="main">意见反馈</span><span class="sub">Feed Back</span></div>
              </button>
              <button class="btn-action trade md-elevation-1 md-ripple" @click="router.push('/trade')">
                <div class="icon-box">●<i class="fas fa-exchange-alt"></i></div>
                <div class="btn-text"><span class="main">交易站</span><span class="sub">Market</span></div>
              </button>
            </div>
          </aside>
        </div>
      </main>

      <aside class="right-column">
        <div class="panel-card md-elevation-1 web-data-panel">
          <div class="card-header">
            <span class="header-label">WEB_METRICS</span>
          </div>
          <div class="metrics-grid">
            <div class="metric-item">
              <span class="metric-val">{{stats.blogs}}</span>
              <span class="metric-label">博客收录</span>
            </div>
            <div class="metric-divider"></div>
            <div class="metric-item">
              <span class="metric-val">{{stats.posts}}</span>
              <span class="metric-label">社区帖子</span>
            </div>
          </div>
        </div>

        <div class="panel-card md-elevation-1 news-panel">
          <div class="card-header">
            <span class="header-label">LATEST_NEWS</span>
          </div>
          <div class="news-list">
            <div v-for="n in news" :key="n.id" class="news-item">
              <span class="news-dot"></span>
              <p>{{ n.text }}</p>
            </div>
          </div>
        </div>

        <div class="panel-card md-elevation-1 online-panel">
          <div class="card-header online-header">
            <span class="header-label">ONLINE_USERS</span>
            <div class="online-badge">
              <span class="pulse-green"></span> {{ onlineStore.onlineCount }}
            </div>
          </div>
          <div class="online-list custom-scroll">
            <div v-if="onlineStore.onlineUsersList.length === 0" class="empty-online">
              <i class="fas fa-satellite-dish"></i>
              <span>正在扫描信号...</span>
            </div>
            <div v-else class="user-grid">
               <div v-for="(u, index) in onlineStore.onlineUsersList" :key="u.userId || index" class="online-avatar" :title="u.userName">
                 <img :src="fixAvatarUrl(u.avatar)" @error="handleImgError" />
               </div>
            </div>
          </div>
        </div>
      </aside>
    </div>

    <CreatePost 
      v-model="showPostForm" 
      @success="handlePostSuccess" 
    />

    <Teleport to="body">
      <Transition name="modal-scale">
        <div v-if="showDetail" class="detail-overlay" @click.self="closeDetail">
          <div class="detail-container md-elevation-5 wide-modal" :class="{ 'blog-mode': currentType === 'blog' }">
            <button class="close-btn floating-close" @click="closeDetail"><i class="fas fa-times"></i></button>
            <div v-if="detailLoading" class="detail-loading">
              <div class="loading-spinner"></div>
              <span>正在提取数据流...</span>
            </div>
            <div v-else-if="currentData" class="detail-wrapper">
              <div v-if="currentData.coverImage || (currentData.images && currentData.images.length > 0)" 
                   class="detail-hero" 
                   :style="{ backgroundImage: `url(${fixAvatarUrl(currentData.coverImage || currentData.images[0].url)})` }">
                <div class="hero-overlay"></div>
                <div class="hero-content">
                  <h1 class="hero-title">{{ currentData.post_title || currentData.title }}</h1>
                  <div class="hero-meta">
                    <div class="hero-author">
                      <img :src="fixAvatarUrl(currentData.author?.avatar)" @error="handleImgError" />
                      <span>{{ currentData.author?.username || currentData.author?.name }}</span>
                    </div>
                    <span class="hero-time">{{ formatTime(currentData.createTime || currentData.updateTime, true) }}</span>
                  </div>
                </div>
              </div>

              <div v-else class="detail-header-simple">
                <h1 class="simple-title">{{ currentData.post_title || currentData.title }}</h1>
                <div class="simple-meta">
                  <div class="meta-author">
                    <img :src="fixAvatarUrl(currentData.author?.avatar)" @error="handleImgError" />
                    <span>@{{ currentData.author?.username || currentData.author?.name }}</span>
                  </div>
                  <span class="meta-date">{{ formatTime(currentData.createTime || currentData.updateTime, true) }}</span>
                </div>
              </div>
              
              <div class="detail-scroll-area custom-scroll">
                <article class="detail-body">
                  <div v-if="currentData.images?.length > 1" class="detail-gallery">
                    <img v-for="(img, idx) in currentData.images.slice(1)" :key="idx" :src="img.url" @error="handleImgError" loading="lazy" class="gallery-img md-elevation-1" />
                  </div>
                  <div class="detail-text markdown-body" v-html="renderMarkdown(currentData.content)"></div>
                </article>

                <div class="divider-line"></div>

                <section v-if="currentType === 'post'" class="comment-section">
                  <div class="section-label">COMMUNITY_REPLIES ({{ currentData.comment_count }})</div>
                  <div v-if="commentLinearList.length" class="comment-list">
                    <div v-for="c in commentLinearList" :key="c.id" class="comment-tree-item" :style="{ paddingLeft: (c.level * 32) + 'px' }">
                      <div class="comment-bubble md-elevation-1" :class="{ 'is-reply': c.level > 0 }">
                        <div class="comment-top">
                          <div class="comment-user">
                            <img :src="fixAvatarUrl(c.author?.avatar)" @error="handleImgError" class="c-avatar" />
                            <span class="c-name">{{ c.author?.username }}</span>
                            <span v-if="c.replyToUser && c.level > 0" class="reply-tag">
                              <i class="fas fa-share"></i> {{ c.replyToUser }}
                            </span>
                          </div>
                          <span class="c-time">{{ formatTime(c.createTime) }}</span>
                        </div>
                        <div class="comment-text">{{ c.content }}</div>
                        <div class="comment-actions">
                          <button class="action-btn" @click="handleReplyTo(c)"><i class="fas fa-reply"></i> 回复</button>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div v-else class="empty-comment">
                    <div class="empty-icon"><i class="far fa-comments"></i></div>
                    <p>暂无回响，等待首个信号...</p>
                  </div>
                </section>
              </div>

              <div v-if="currentType === 'post'" class="detail-footer-wrapper md-elevation-3">
                <div v-if="replyTarget" class="reply-status-bar">
                  <span>回复 <strong style="color: var(--accent-blue)">@{{ replyTarget.author?.username }}</strong></span>
                  <button class="cancel-reply" @click="cancelReply"><i class="fas fa-times"></i></button>
                </div>
                <div class="detail-footer-input">
                  <input ref="commentInputRef" type="text" v-model="newComment" 
                    :placeholder="replyTarget ? '回复评论...' : '发布你的看法...'" 
                    @keyup.enter="submitComment" :disabled="isSubmitting" />
                  <button class="send-btn md-ripple" @click="submitComment" :disabled="isSubmitting || !newComment">
                    <span v-if="isSubmitting"><i class="fas fa-spinner fa-spin"></i></span>
                    <span v-else><i class="fas fa-paper-plane"></i></span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, reactive, nextTick, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';
import CreatePost from '@/comminicateCenter/CreatePost.vue';
import ActivityRanking from '@/comminicateCenter/ActivityRanking.vue'; // ✅ 引入排行榜
import { marked } from 'marked';
import { useOnlineStore } from '@/stores/online';

const router = useRouter(); 
const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

const onlineStore = useOnlineStore();

const isDev = window.location.hostname === 'localhost';
const BASE_URL = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const posts = ref([]);
const blogs = ref([]);
const stats = reactive({ blogs: 0, posts: 0 });
const loading = ref(false);
const blogLoading = ref(false);
const hasMore = ref(true);
const lastId = ref(null);
const news = ref([{ id: 1, text: '喝水游戏持续开发中ing.....' }, { id: 2, text: '暂无，随便放点' }]);

const showDetail = ref(false);
const showPostForm = ref(false);
const detailLoading = ref(false);
const currentType = ref('post'); 
const currentData = ref(null);   
const comments = ref([]);
const newComment = ref('');
const isSubmitting = ref(false);
const replyTarget = ref(null);
const commentInputRef = ref(null);

const handleImgError = (e) => { 
  if (e.target.src.includes('土豆.jpg')) return;
  e.target.src = '/土豆.jpg'; 
};

const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  if (!path.startsWith('uploads/')) path = `uploads/${path}`;
  return `${BASE_URL}/${path}`;
};

const formatTime = (t, showTime = false) => {
  if (!t) return 'N/A';
  const date = new Date(t);
  return showTime ? date.toLocaleString() : date.toLocaleDateString();
};
const renderMarkdown = (content) => content ? marked.parse(content) : '';

const commentLinearList = computed(() => {
  if (!comments.value?.length) return [];
  const map = {}, roots = [], result = [];
  comments.value.forEach(c => map[c.id] = { ...c, children: [] });
  comments.value.forEach(c => {
    const pid = c.ParentCommentId || c.parentId;
    (pid && map[pid]) ? map[pid].children.push(map[c.id]) : roots.push(map[c.id]);
  });
  const traverse = (node, level) => {
    node.level = level;
    const pid = node.ParentCommentId || node.parentId;
    if (level > 0 && map[pid]) node.replyToUser = map[pid].author?.username;
    result.push(node);
    node.children?.sort((a, b) => new Date(a.createTime) - new Date(b.createTime)).forEach(child => traverse(child, level + 1));
  };
  roots.sort((a, b) => new Date(b.createTime) - new Date(a.createTime)).forEach(root => traverse(root, 0));
  return result;
});

const fetchPosts = async (isFirstLoad = true) => {
  if (loading.value || (!hasMore.value && !isFirstLoad)) return;
  loading.value = true;
  try {
    const res = await apiClient.get('/Posts', { params: { lastId: isFirstLoad ? null : lastId.value, pageSize: 15, _t: new Date().getTime() } });
    if (res.data.success) {
      posts.value = isFirstLoad ? res.data.data : [...posts.value, ...res.data.data];
      lastId.value = res.data.pagination.lastId;
      hasMore.value = res.data.pagination.hasMore;
    }
  } catch (e) { console.error(e); } finally { loading.value = false; }
};

const fetchBlogs = async () => {
  blogLoading.value = true;
  try {
    const res = await apiClient.get('/Blog/articles', { params: { page: 1, pageSize: 10 } });
    blogs.value = res.data.list;
  } catch (e) { console.error(e); } finally { blogLoading.value = false; }
};

const fetchStats = async () => {
  try {
    const [blogRes, postRes] = await Promise.all([
      apiClient.get('/Blog/blogAmount'),
      apiClient.get('/Posts/postAmount')
    ]);
    stats.blogs = blogRes.data;
    stats.posts = postRes.data;
  } catch (e) { console.error("Stats Error", e); }
};

const openPostDetail = async (id) => {
  currentType.value = 'post'; showDetail.value = true; detailLoading.value = true;
  currentData.value = null; comments.value = []; cancelReply();
  try {
    const [postRes, commentRes] = await Promise.all([
      apiClient.get(`/Posts/${id}`),
      apiClient.get(`/Posts/${id}/comments`, { params: { pageSize: 500 } })
    ]);
    if (postRes.data.success) currentData.value = postRes.data.data;
    if (commentRes.data.success) comments.value = commentRes.data.data;
  } catch (e) { console.error(e); } finally { detailLoading.value = false; }
};

const openBlogDetail = async (id) => {
  currentType.value = 'blog'; showDetail.value = true; detailLoading.value = true; currentData.value = null;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    currentData.value = res.data;
  } catch (e) { console.error(e); } finally { detailLoading.value = false; }
};

const submitComment = async () => {
  if (!newComment.value.trim() || !currentData.value) return;
  isSubmitting.value = true;
  try {
    const res = await apiClient.post(`/Posts/${currentData.value.id}/comments/json`, {
      Content: newComment.value,
      ParentCommentId: replyTarget.value?.id || 0
    });
    if (res.data.success) {
      comments.value.push(res.data.data);
      newComment.value = '';
      cancelReply();
      if(currentData.value.comment_count !== undefined) currentData.value.comment_count++;
    }
  } catch (e) { alert("发送失败"); } finally { isSubmitting.value = false; }
};

const handlePostSuccess = async () => {
  showPostForm.value = false; 
  const scrollContainer = document.querySelector('.posts-scroll-area');
  if (scrollContainer) {
    scrollContainer.scrollTop = 0;
  }
  lastId.value = null;
  hasMore.value = true;
  loading.value = false;
  await fetchPosts(true);
};

const handleScroll = (e) => {
  if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight - 50) fetchPosts(false);
};
const closeDetail = () => { showDetail.value = false; currentData.value = null; comments.value = []; };
const handleReplyTo = (c) => { replyTarget.value = c; nextTick(() => commentInputRef.value?.focus()); };
const cancelReply = () => { replyTarget.value = null; };

onMounted(() => {
  fetchPosts(true); fetchBlogs(); fetchStats();
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});

onUnmounted(() => {
  clearInterval(clockTimer);
});




</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Noto+Serif+SC:wght@500;700&display=swap');

.tc-dashboard { 
  --bg-main: #f3f4f6; 
  --card-bg: #ffffff;
  --accent-blue: #3b82f6; 
  --accent-blue-hover: #2563eb;
  --accent-teal: #14b8a6; 
  --accent-orange: #f59e0b; 
  --accent-dark: #1e293b;
  --text-primary: #111827; 
  --text-secondary: #4b5563; 
  --text-sub: #9ca3af;
  --border-light: #e5e7eb;
  --header-height: 56px;
  --gap: 20px;
  width: 100%; height: 100vh; background-color: var(--bg-main); 
  display: flex; flex-direction: column; overflow: hidden; 
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

/* --- 核心布局修改 --- */
.main-bridge { 
  flex: 1; 
  display: flex; 
  padding: var(--gap); 
  gap: var(--gap); 
  min-height: 0; 
  width: 100%; 
  /* ✅ 修改点 1：最大宽度从 1920 改为 2560，解决 2K 屏两边空的问题 */
  max-width: 2560px; 
  margin: 0 auto;
  box-sizing: border-box;
}

/* 排行榜栏 */
.rank-column {
  flex: 0 0 240px;
  display: flex;
  flex-direction: column;
  gap: var(--gap);
}

.left-column { 
  flex: 0 0 260px; 
  display: flex; flex-direction: column; gap: var(--gap); 
  transition: all 0.3s ease; 
}

.right-column { 
  flex: 0 0 320px; 
  display: flex; flex-direction: column; gap: var(--gap); 
  transition: all 0.3s ease;
}

.mid-column { 
  flex: 1; 
  display: flex; flex-direction: column; gap: var(--gap); 
  min-width: 0; 
}

.action-sidebar { 
  width: 240px; 
  display: flex; flex-direction: column; gap: 16px; flex-shrink: 0; 
  transition: width 0.3s ease;
}

/* 通用组件基础样式 (保持不变) */
.sys-info-bar { height: var(--header-height); background: var(--card-bg); border-bottom: 1px solid var(--border-light); display: flex; justify-content: space-between; align-items: center; padding: 0 32px; font-size: 12px; font-weight: 600; color: var(--text-secondary); z-index: 10; }
.panel-card { background: var(--card-bg); border-radius: 16px; overflow: hidden; display: flex; flex-direction: column; border: 1px solid var(--border-light); }
.full-height { height: 100%; }

/* --- ⚡️ 响应式适配关键修改 ⚡️ --- */

/* ✅ 修改点 2：将隐藏排行榜的断点从 1600px 降到 1200px。
   这样即使是 1080P 屏幕开启了 125% 缩放 (等效 1536px)，排行榜也依然会显示。
*/
@media screen and (max-width: 1200px) {
  .rank-column {
    display: none;
  }
  .left-column { flex: 0 0 280px; }
  .right-column { flex: 0 0 340px; }
}

/* 1920px (1080P) 屏幕缩放逻辑 - 保持您喜欢的 zoom 设置 */
@media screen and (max-width: 1920px) {
  .tc-dashboard {
    font-size: 13px; 
    zoom: 0.7; /* 保持缩放 */
    --gap: 16px; 
  }

  /* 适配 4 列的宽度调整 */
  .rank-column { 
    flex: 0 0 220px; 
    display: flex !important; /* ✅ 双重保险：强制显示 */
  }
  .left-column { flex: 0 0 240px; }
  .right-column { flex: 0 0 300px; }
  .action-sidebar { width: 190px; gap: 12px; }

  .sys-info-bar { height: 48px; padding: 0 24px; }
  .activity-slider { height: 150px !important; }
  .slider-title { font-size: 24px !important; }
  .post-entry { padding: 16px !important; gap: 12px !important; }
  .entry-title { font-size: 16px !important; margin: 6px 0 !important; }
  .entry-thumb-wrapper { width: 80px !important; height: 80px !important; }







  .btn-action { /*帖子右侧导航栏*/
    padding: 12px !important;
     border-radius: 12px !important;
     margin: 0 auto;
     transition: all 0.5s ease-in-out;
    }
    
.icon-box { /*导航栏前小点*/
      font-size: 8px !important;
      width: 36px !important;
      height: 36px !important;
    }

  .metrics-grid { padding: 16px !important; }
  .metric-val { font-size: 22px !important; }
  .online-panel { min-height: 160px !important; }
}






/* --- 下面是保持不变的视觉样式 --- */
.detail-overlay { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.75); backdrop-filter: blur(8px); z-index: 9999; display: flex; justify-content: center; align-items: center; }
.detail-container.wide-modal { width: 90vw; max-width: 1200px; height: 90vh; border-radius: 24px; }
.detail-wrapper { display: flex; flex-direction: column; height: 100%; background: #fff; }
.floating-close { position: absolute; top: 20px; right: 20px; z-index: 20; width: 40px; height: 40px; border-radius: 50%; background: rgba(255,255,255,0.9); border: none; box-shadow: 0 4px 12px rgba(0,0,0,0.15); cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 20px; color: #333; transition: all 0.2s; }
.floating-close:hover { background: #fff; transform: scale(1.1); color: var(--accent-red); }
.detail-hero { height: 350px; position: relative; background-size: cover; background-position: center; flex-shrink: 0; display: flex; flex-direction: column; justify-content: flex-end; transition: height 0.3s ease; }
.hero-overlay { position: absolute; inset: 0; background: linear-gradient(to bottom, rgba(0,0,0,0.1), rgba(0,0,0,0.8)); }
.hero-content { position: relative; z-index: 2; padding: 40px; color: #fff; text-shadow: 0 2px 4px rgba(0,0,0,0.5); }
.hero-title { font-size: clamp(2rem, 3.5vw, 3rem); font-weight: 800; margin: 0 0 16px 0; line-height: 1.2; font-family: 'Noto Serif SC', serif; }
.hero-meta { display: flex; align-items: center; gap: 20px; font-size: 14px; font-weight: 500; }
.hero-author { display: flex; align-items: center; gap: 10px; }
.hero-author img { width: 32px; height: 32px; border-radius: 50%; border: 2px solid #fff; }
.detail-header-simple { padding: 40px 60px 20px 60px; border-bottom: 1px solid var(--border-light); }
.simple-title { font-size: 2.2rem; font-weight: 800; color: var(--text-primary); margin-bottom: 16px; line-height: 1.2; }
.simple-meta { display: flex; align-items: center; justify-content: space-between; color: var(--text-secondary); }
.meta-author { display: flex; align-items: center; gap: 10px; font-weight: 600; }
.meta-author img { width: 32px; height: 32px; border-radius: 50%; }
.detail-scroll-area { flex: 1; overflow-y: auto; }
.detail-body { padding: 40px 15% 20px 15%; font-size: 1.1rem; }
.detail-gallery { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 30px; }
.gallery-img { width: 100%; height: 200px; object-fit: cover; border-radius: 8px; cursor: zoom-in; }
.divider-line { height: 1px; background: var(--border-light); margin: 40px 10%; }
.comment-section { padding: 0 15% 40px 15%; }
.comment-bubble { background: #f8fafc; border-radius: 12px; padding: 16px; margin-bottom: 16px; border: 1px solid var(--border-light); position: relative; }
.comment-bubble.is-reply { background: #fff; border-left: 4px solid var(--accent-blue); margin-left: 10px; }
.comment-top { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 12px; }
.comment-user { display: flex; align-items: center; gap: 8px; font-weight: 700; color: var(--text-primary); }
.c-avatar { width: 24px; height: 24px; border-radius: 50%; }
.reply-tag { background: #e0f2fe; color: #0284c7; padding: 2px 6px; border-radius: 4px; font-size: 10px; }
.comment-text { font-size: 14px; color: var(--text-secondary); line-height: 1.6; }
.action-btn { background: none; border: none; font-size: 12px; color: var(--accent-blue); cursor: pointer; font-weight: 600; margin-top: 8px; }
.detail-footer-wrapper { padding: 16px 15%; border-top: 1px solid var(--border-light); background: #fff; }
.detail-footer-input { display: flex; gap: 12px; }
.detail-footer-input input { flex: 1; padding: 14px 20px; border-radius: 24px; border: 1px solid var(--border-light); outline: none; background: #f9fafb; font-size: 14px; transition: 0.2s; }
.detail-footer-input input:focus { background: #fff; border-color: var(--accent-blue); box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }
.send-btn { width: 48px; height: 48px; border-radius: 50%; border: none; background: var(--accent-blue); color: #fff; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 18px; transition: 0.2s; }
.send-btn:hover { background: var(--accent-blue-hover); transform: scale(1.05); }
.card-header { padding: 16px 20px; background: transparent; border-bottom: 1px solid var(--border-light); }
.header-label { font-size: 12px; font-weight: 800; color: var(--text-sub); letter-spacing: 1px; }
.blog-min-list { padding: 12px; max-height: 500px; overflow-y: auto; }
.min-blog-item { display: flex; gap: 12px; padding: 12px; margin-bottom: 10px; border-radius: 12px; background: #fff; border: 1px solid transparent; transition: all 0.2s; align-items: flex-start; }
.min-blog-item:hover { border-color: var(--accent-blue); background: #f8fafc; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.06); }
.min-blog-thumb { width: 64px; height: 64px; flex-shrink: 0; border-radius: 8px; overflow: hidden; border: 1px solid var(--border-light); background-color: #f1f5f9; }
.min-blog-thumb img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
.min-blog-item:hover .min-blog-thumb img { transform: scale(1.1); }
.blog-content { flex: 1; min-width: 0; display: flex; flex-direction: column; justify-content: center; height: 100%; }
.blog-top-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px; }
.blog-accent { font-size: 9px; font-weight: 800; color: #ef4444; background: #fee2e2; padding: 2px 6px; border-radius: 4px; }
.blog-tag { font-size: 10px; color: var(--accent-teal); font-weight: 600; background: #f0fdfa; padding: 2px 6px; border-radius: 4px; }
.blog-title { font-size: 13px; font-weight: 600; color: var(--text-primary); margin: 0; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.markdown-body { font-size: 1.05rem; line-height: 1.8; color: #374151; }
.markdown-body :deep(h1), .markdown-body :deep(h2) { border-bottom: none; margin-top: 1.5em; margin-bottom: 0.5em; }
.markdown-body :deep(p) { margin-bottom: 1.2em; }
.markdown-body :deep(blockquote) { border-left: 4px solid var(--accent-blue); background: #f8fafc; padding: 16px; border-radius: 0 8px 8px 0; color: #555; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #d1d5db; border-radius: 4px; }
.modal-scale-enter-active, .modal-scale-leave-active { transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1); }
.modal-scale-enter-from, .modal-scale-leave-to { opacity: 0; transform: scale(0.96) translateY(20px); }
.sys-status { display: flex; align-items: center; gap: 8px; }
.pulse { width: 8px; height: 8px; background-color: #10b981; border-radius: 50%; box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.2); animation: pulse 2s infinite; }
.activity-slider { height: 180px; border-radius: 16px; background: linear-gradient(135deg, #0f172a 0%, #334155 100%); position: relative; overflow: hidden; flex-shrink: 0; display: flex; align-items: center; padding: 0 48px; color: #fff; transition: height 0.3s ease; }
.slider-bg-effect { position: absolute; top: -50%; right: -20%; width: 400px; height: 400px; background: radial-gradient(circle, rgba(59,130,246,0.3) 0%, transparent 70%); filter: blur(40px); z-index: 0; }
.slider-content { position: relative; z-index: 1; }
.slider-badge { font-size: 10px; font-weight: 800; background: rgba(255,255,255,0.2); padding: 4px 8px; border-radius: 4px; display: inline-block; margin-bottom: 12px; }
.slider-title { font-size: 28px; font-weight: 800; margin: 0 0 8px 0; letter-spacing: -0.5px; }
.slider-sub { font-family: monospace; font-size: 12px; opacity: 0.7; }
.mid-content-split { flex: 1; display: flex; gap: var(--gap); min-height: 0; }
.posts-flow-container { flex: 1; background: var(--card-bg); border-radius: 16px; border: 1px solid var(--border-light); display: flex; flex-direction: column; overflow: hidden; }
.flow-header { padding: 16px 24px; border-bottom: 1px solid var(--border-light); display: flex; justify-content: space-between; align-items: center; background: #fff; }
.header-title { font-size: 14px; font-weight: 700; color: var(--text-primary); letter-spacing: 0.5px; }




/* 容器：灰色的背景轨道 */
.header-tabs { 
  display: inline-flex; /* 改为 inline-flex 让容器紧贴内容，或保持 flex */
  align-items: center;
  background-color: #f1f3f5; /* 浅灰色轨道背景 */
  padding: 0px; /* 轨道内边距，给滑块留出呼吸空间 */
  border-radius: 10px; /* 无论多高都保持圆角 */
  position: relative;
  /* 移除 gap，因为我们用 padding 和布局来控制间距 */
  gap: 4px;
}

/* 单个 Tab 按钮：默认状态 */
.tab {
  position: relative;
  padding: 8px 20px; /* 调整内边距，控制按钮大小 */
  font-size: 16px; /* 稍微调小一点字体使其更精致，原 20px 可能过大 */
  font-weight: 500;
  color: var(--text-sub, #888); /* 默认文字颜色（灰色） */
  cursor: pointer;
  border-radius: 10px; /* 按钮自身也需要圆角 */
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); /* 添加平滑过渡动画 */
  z-index: 1;
  user-select: none; /* 防止双击选中文本 */
}

/* 激活状态：变成白色的药丸 */
.tab.active {
  background-color: #ffffff; /* 激活时的背景色（白色） */
  color: #ffffff; /* 激活时的文字颜色（深色） */
  font-weight: 600; /* 激活时加粗 */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08); /* 添加轻微的投影，增加立体感 */
}

/* 悬停状态：轻微反馈 */
.tab:hover:not(.active) {
  color: #ffffff; /* 没被选中时，鼠标放上去稍微变深一点 */
  background-color: #212c3f; /* 轻微的灰色背景 */
}










.btn-text {
  /* 核心：开启Flex布局 */
  display: flex;
  /* 核心：设置竖向排列（主轴为垂直方向），实现自动换行堆叠 */
  flex-direction: column;
  
  /* 可选补充：优化布局（根据需求调整） */
  align-items: flex-start; /* 子元素水平左对齐（默认居中） */
  gap: 8px; /* 两个span之间的间距（替代margin，更便捷） */
  /* 其他自定义样式 */
  width: 100%; /* 让按钮宽度撑满容器 */
  padding: 1px;
  background-color: var(--header-height);
}





.posts-scroll-area { flex: 1; overflow-y: auto; background: #f3f4f6; padding: 16px; }
.post-entry { background: #fff; border-radius: 16px; padding: 24px; margin-bottom: 16px; border: 1px solid transparent; cursor: pointer; transition: all 0.2s; display: flex; justify-content: space-between; gap: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
.post-entry:hover { border-color: var(--accent-blue); box-shadow: 0 8px 20px rgba(59, 130, 246, 0.1); transform: translateY(-2px); }
.entry-main { flex: 1; min-width: 0; display: flex; flex-direction: column; justify-content: space-between; }
.entry-user-row { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
.user-avatar-sm { width: 28px; height: 28px; border-radius: 50%; object-fit: cover; }
.username { font-size: 13px; font-weight: 600; color: var(--text-primary); }
.post-time { font-size: 11px; color: var(--text-sub); }
.entry-title { font-size: 18px; font-weight: 700; color: var(--text-primary); margin: 0 0 12px 0; line-height: 1.4; }
.entry-actions { display: flex; gap: 16px; font-size: 12px; color: var(--text-secondary); }
.entry-thumb-wrapper { width: 100px; height: 100px; border-radius: 12px; overflow: hidden; position: relative; flex-shrink: 0; }
.entry-thumb { width: 100%; height: 100%; object-fit: cover; }
.action-grid { display: flex; flex-direction: column; gap: 12px; transition: 0.4s ease-in-out; }
.btn-action { padding: 16px; border-radius: 16px; border: none; cursor: pointer; display: flex; align-items: center; gap: 12px; width: 100%; transition: all 0.2s; text-align: left; background: #fff; box-shadow: 0 2px 4px rgba(0,0,0,0.02); border: 1px solid transparent; }
.btn-action:hover { transform: translateY(-2px); box-shadow: 0 8px 16px rgba(0,0,0,0.05); border-color: var(--border-light); }
.icon-box { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 18px; flex-shrink: 0; }
.metrics-grid { padding: 24px; display: flex; justify-content: space-around; align-items: center; }
.metric-val { font-size: 28px; font-weight: 800; color: var(--text-primary); }
.metric-label { font-size: 12px; color: var(--text-secondary); }
.metric-divider { width: 1px; height: 30px; background: var(--border-light); }
.news-list { padding: 16px; }
.news-item { display: flex; gap: 10px; margin-bottom: 16px; align-items: flex-start; }
.news-dot { width: 8px; height: 8px; background: var(--accent-blue); border-radius: 50%; margin-top: 6px; flex-shrink: 0; }
.news-item p { margin: 0; font-size: 13px; color: var(--text-secondary); line-height: 1.6; }
.md-elevation-1 { box-shadow: 0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px 0 rgba(0,0,0,0.06); }
.md-elevation-2 { box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06); }
.md-elevation-5 { box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04); }
.md-ripple { position: relative; overflow: hidden; transform: translate3d(0,0,0); transition: background-color 0.2s, box-shadow 0.2s; }
.md-ripple:active { background-color: rgba(0,0,0,0.05); }
.online-panel { flex: 1; min-height: 200px; display: flex; flex-direction: column; margin-top: 20px; }
.online-header { display: flex; justify-content: space-between; align-items: center; }
.online-badge { background: #ecfdf5; color: #059669; padding: 2px 8px; border-radius: 12px; font-size: 12px; font-weight: 700; display: flex; align-items: center; gap: 6px; }
.pulse-green { width: 6px; height: 6px; background: #10b981; border-radius: 50%; animation: pulse 2s infinite; }
.online-list { flex: 1; padding: 16px; overflow-y: auto; }
.empty-online { color: #9ca3af; text-align: center; margin-top: 20px; font-size: 12px; display: flex; flex-direction: column; align-items: center; gap: 8px; }
.empty-online i { font-size: 24px; opacity: 0.5; }
.user-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(40px, 1fr)); gap: 12px; }
.online-avatar { width: 40px; height: 40px; border-radius: 50%; overflow: hidden; border: 2px solid #fff; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: transform 0.2s; }
.online-avatar:hover { transform: scale(1.1); border-color: var(--accent-blue); z-index: 2; }
.online-avatar img { width: 100%; height: 100%; object-fit: cover; }
@keyframes pulse {
0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7); }
70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(16, 185, 129, 0); }
100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
}
</style>