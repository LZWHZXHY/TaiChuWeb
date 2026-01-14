<template>
  <div class="cyber-dashboard">
    <div class="grid-bg moving-grid"></div>

    <div class="panel-scroll-container custom-scroll">
      
      <header class="main-header">
        <div class="header-split left">
          <h1 class="giant-text glitch-hover">
            <div class="text-row">TAICHU</div>
            <div class="text-row outline">NETWORK</div>
            <div class="text-row red-fill">社区</div>
          </h1>
        </div>
        <div class="header-split right">
          <div class="info-block">
            <h2 class="cn-title">太初中枢 // NEURAL_HUB</h2>
            <div class="live-indicator"><span class="dot"></span> SYSTEM ONLINE</div>
            <div class="sys-time-display">{{ currentTime }}</div>
            <div class="tech-lines">
               <span>// NODE_SYNC: {{ stats.posts }} POSTS</span>
               <span>// DB_ARCHIVE: {{ stats.blogs }} BLOGS</span>
            </div>
          </div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // COMMUNITY_Protocol_v5.0 // DATA_STREAM_ACTIVE // WELCOME_USER // SHARE_YOUR_THOUGHTS // 
          // COMMUNITY_Protocol_v5.0 // DATA_STREAM_ACTIVE // WELCOME_USER // SHARE_YOUR_THOUGHTS //
        </div>
      </div>

      <div class="main-bridge">
        
        <aside class="rank-column">
          <div class="cyber-card">
            <div class="card-label-strip"><span>// RANKING_SYSTEM</span></div>
            <div class="card-inner-pad">
               <ActivityRanking />
            </div>
          </div>
        </aside>

        <aside class="left-column">
          <section class="cyber-card full-height">
            <div class="card-label-strip"><span>// RECOMMENDED_BLOGS</span></div>
            <div class="blog-min-list custom-scroll">
              <div v-if="blogLoading" class="status-text">[ LOADING_ARCHIVES... ]</div>
              <div v-else-if="blogs.length === 0" class="status-text">// NO_DATA //</div>
              
              <div v-for="b in blogs" :key="b.Id" class="min-blog-item" @click="openBlogDetail(b.Id)">
                <div class="blog-indicator"></div>
                <div v-if="b.CoverImage" class="min-blog-thumb">
                  <img :src="fixAvatarUrl(b.CoverImage)" @error="handleImgError" loading="lazy" />
                </div>
                
                <div class="blog-content">
                  <div class="blog-top-row">
                    <span class="blog-tag">#{{ (b.Tags && b.Tags[0]) || 'CORE' }}</span>
                    <div class="blog-accent" v-if="b.isTop">TOP</div>
                  </div>
                  <p class="blog-title">{{ b.Title }}</p>
                </div>
              </div>
            </div>
          </section>
        </aside>

        <main class="mid-column">
          <section class="activity-banner">
            <div class="banner-content">
              <div class="banner-badge">ANNOUNCEMENT</div>
              <h2 class="banner-title">太初宇宙 5.0 官网开发中...</h2>
              <p class="banner-sub">NODE_STATUS: SYNCING // 2026.01.09</p>
            </div>
            <div class="banner-pattern"></div>
          </section>

          <div class="module-switcher-bar">
            <button 
              class="module-tab" 
              :class="{ active: currentModule === 'post' }" 
              @click="switchModule('post')"
            >
              <span class="tab-icon">■</span>
              <span class="tab-text">POST_STREAM // 社区流</span>
            </button>
            
            <button 
              class="module-tab" 
              :class="{ active: currentModule === 'chat' }" 
              @click="switchModule('chat')"
            >
              <span class="tab-icon">●</span>
              <span class="tab-text">SECURE_CHAT // 加密通讯</span>
              <span class="unread-dot"></span> 
            </button>
          </div>

          <Transition name="fade-slide" mode="out-in">
            
            <div v-if="currentModule === 'post'" class="mid-content-split" key="post">
              <section class="posts-container cyber-card">
                <div class="flow-header">
                  <span class="header-title">
                    <span class="icon">■</span> COMMUNITY_POSTS
                  </span>
                  <div class="header-tabs">
                    <span class="tab active">LATEST / 最新</span>
                    <span class="tab">HOT / 热门</span>
                  </div>
                </div>
                
                <div class="posts-scroll-area custom-scroll" @scroll="handleScroll">
                  <div v-if="loading && posts.length === 0" class="status-text">[ SIGNAL_CONNECTING... ]</div>
                  
                  <div v-for="p in posts" :key="p.id" class="post-entry" @click="openPostDetail(p.id)">
                    <div class="entry-scanline"></div>
                    <div class="entry-main">
                      <div class="entry-user-row">
                        <div class="avatar-box">
                           <img :src="fixAvatarUrl(p.author?.avatar)" @error="handleImgError" />
                        </div>
                        <div class="user-info">
                          <span class="username">{{ p.author?.username || 'UNKNOWN_USER' }}</span>
                          <span class="post-time">{{ formatTime(p.createTime) }}</span>
                        </div>
                      </div>
                      <h3 class="entry-title">{{ p.post_title }}</h3>
                      <div class="entry-actions">
                        <span class="action-item"><i class="far fa-comment-alt"></i> REPLIES: {{ p.comment_count }}</span>
                        <span class="action-item"><i class="far fa-eye"></i> VISITS: {{ p.view_count }}</span>
                      </div>
                    </div>
                    <div v-if="p.images?.length" class="entry-thumb-wrapper">
                      <img :src="p.images[0]" @error="handleImgError" class="entry-thumb" />
                      <span v-if="p.images.length > 1" class="img-count-badge">+{{ p.images.length }}</span>
                    </div>
                  </div>
                  
                  <div v-if="loading" class="loading-status">
                    <i class="fas fa-circle-notch fa-spin"></i> SYNCING_STREAM...
                  </div>
                </div>
              </section>

              <aside class="action-sidebar">
                <div class="section-label">// QUICK_ACTIONS</div>
                <div class="action-grid">
                  <button class="cyber-btn" @click="router.push('/blogCreater')">
                    <span class="btn-icon"><i class="fas fa-edit"></i></span>
                    <div class="btn-text"><span class="main">发布博客</span><span class="sub">POST_BLOG</span></div>
                  </button>
                  <button class="cyber-btn red-mode" @click="showPostForm = true">
                    <span class="btn-icon"><i class="fas fa-paper-plane"></i></span>
                    <div class="btn-text"><span class="main">发布帖子</span><span class="sub">POST_THREAD</span></div>
                  </button>
                  <button class="cyber-btn" @click="router.push('/trade')">
                    <span class="btn-icon"><i class="fas fa-exchange-alt"></i></span>
                    <div class="btn-text"><span class="main">交易站</span><span class="sub">MARKET_PLACE</span></div>
                  </button>
                </div>
              </aside>
            </div>

            <div v-else class="chat-module-wrapper" key="chat">
              <ChatRoom class="embedded-chat" />
            </div>

          </Transition>
        </main>

        <aside class="right-column">
          <div class="cyber-terminal">
             <div class="terminal-header">
               <span>> WEB_METRICS.exe</span>
               <span class="blink">_</span>
             </div>
             <div class="metrics-content">
                <div class="metric-row">
                  <span class="label">BLOG_INDEX:</span>
                  <span class="val">{{ stats.blogs }}</span>
                </div>
                <div class="metric-row">
                  <span class="label">POST_INDEX:</span>
                  <span class="val">{{ stats.posts }}</span>
                </div>
             </div>
          </div>

          <div class="cyber-card news-panel">
            <div class="card-label-strip"><span>// LATEST_NEWS</span></div>
            <div class="news-list">
              <div v-for="n in news" :key="n.id" class="news-item">
                <span class="news-marker">>></span>
                <p>{{ n.text }}</p>
              </div>
            </div>
          </div>

          <div class="cyber-card online-panel">
            <div class="card-header-simple">
              <span class="header-label">ONLINE_USERS</span>
              <div class="online-badge">
                <span class="pulse-green"></span> {{ onlineStore.onlineCount }}
              </div>
            </div>
            <div class="online-list custom-scroll">
              <div v-if="onlineStore.onlineUsersList.length === 0" class="empty-online">
                <span>[ SCANNING_SIGNALS... ]</span>
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
          <div v-if="showDetail" class="cyber-modal-overlay" @click.self="closeDetail">
            <div class="cyber-modal-window" :class="{ 'blog-mode': currentType === 'blog' }">
              
              <div class="cyber-modal-header">
                <div class="modal-title-block">
                  <span class="deco-box">■</span>
                  <span class="title-text">DATA_VIEWER // {{ currentType.toUpperCase() }}</span>
                </div>
                <button class="cyber-close-btn" @click="closeDetail">
                   <span class="btn-text">CLOSE [X]</span>
                </button>
              </div>

              <div v-if="detailLoading" class="detail-loading-state">
                <div class="loading-spinner"></div>
                <span>DOWNLOADING_STREAM...</span>
              </div>

              <div v-else-if="currentData" class="detail-wrapper custom-scroll">
                
                <div v-if="currentData.coverImage || (currentData.images && currentData.images.length > 0)" 
                     class="detail-hero" 
                     :style="{ backgroundImage: `url(${fixAvatarUrl(currentData.coverImage || currentData.images[0].url)})` }">
                  <div class="hero-overlay-grid"></div>
                  <div class="hero-content">
                    <div class="hero-meta-tag">AUTHOR: {{ currentData.author?.username || currentData.author?.name }}</div>
                    <h1 class="hero-title">{{ currentData.post_title || currentData.title }}</h1>
                    <span class="hero-time">{{ formatTime(currentData.createTime || currentData.updateTime, true) }}</span>
                  </div>
                </div>

                <div v-else class="detail-header-simple">
                  <h1 class="simple-title">{{ currentData.post_title || currentData.title }}</h1>
                  <div class="simple-meta">
                    <span class="meta-tag">@{{ currentData.author?.username || currentData.author?.name }}</span>
                    <span class="meta-date">{{ formatTime(currentData.createTime || currentData.updateTime, true) }}</span>
                  </div>
                </div>
                
                <article class="detail-body">
                  <div v-if="currentData.images?.length > 1" class="detail-gallery">
                    <img v-for="(img, idx) in currentData.images.slice(1)" :key="idx" :src="img.url" @error="handleImgError" loading="lazy" class="gallery-img" />
                  </div>
                  <div class="detail-text markdown-body" v-html="renderMarkdown(currentData.content)"></div>
                </article>

                <div class="divider-strip">// END_OF_FILE // COMMENTS_SECTION_BELOW //</div>

                <section v-if="currentType === 'post'" class="comment-section">
                  <div class="section-label">COMMUNITY_REPLIES ({{ currentData.comment_count }})</div>
                  <div v-if="commentLinearList.length" class="comment-list">
                    <div v-for="c in commentLinearList" :key="c.id" class="comment-tree-item" :style="{ paddingLeft: (c.level * 20) + 'px' }">
                      <div class="comment-block" :class="{ 'is-reply': c.level > 0 }">
                        <div class="comment-top">
                          <span class="c-name">> {{ c.author?.username }}</span>
                          <span v-if="c.replyToUser && c.level > 0" class="reply-tag">
                            @{{ c.replyToUser }}
                          </span>
                          <span class="c-time">{{ formatTime(c.createTime) }}</span>
                        </div>
                        <div class="comment-text">{{ c.content }}</div>
                        <button class="reply-text-btn" @click="handleReplyTo(c)">[ REPLY ]</button>
                      </div>
                    </div>
                  </div>
                  <div v-else class="empty-comment">
                    <p>>> NO_DATA. BE THE FIRST TO TRANSMIT.</p>
                  </div>
                </section>
              </div>

              <div v-if="currentType === 'post'" class="detail-footer-wrapper">
                <div v-if="replyTarget" class="reply-status-bar">
                  <span>TARGET: <strong style="color: var(--red)">@{{ replyTarget.author?.username }}</strong></span>
                  <button class="cancel-reply" @click="cancelReply">CANCEL</button>
                </div>
                <div class="detail-footer-input">
                  <input ref="commentInputRef" type="text" v-model="newComment" 
                    :placeholder="replyTarget ? 'INPUT_RESPONSE...' : 'TRANSMIT_DATA...'" 
                    @keyup.enter="submitComment" :disabled="isSubmitting" 
                    class="cyber-input" />
                  <button class="send-btn-rect" @click="submitComment" :disabled="isSubmitting || !newComment">
                    <span v-if="isSubmitting">SENDING...</span>
                    <span v-else>SEND</span>
                  </button>
                </div>
              </div>

            </div>
          </div>
        </Transition>
      </Teleport>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive, nextTick, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';
import CreatePost from '@/comminicateCenter/CreatePost.vue';
import ActivityRanking from '@/comminicateCenter/ActivityRanking.vue';
import { marked } from 'marked';
import { useOnlineStore } from '@/stores/online';
// [NEW] 引入聊天室组件
import ChatRoom from '@/ChatRoom/ChatRoom.vue';

const router = useRouter(); 
const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

const onlineStore = useOnlineStore();

const isDev = window.location.hostname === 'localhost';
const BASE_URL = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com';

// --- 模块切换状态 ---
const currentModule = ref('post'); // 'post' | 'chat'

const switchModule = (module) => {
  if (currentModule.value === module) return;
  // 这里可以加一些切换时的预加载逻辑
  currentModule.value = module;
};

// --- 原有逻辑 ---
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
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量：工业赛博风格 --- */
.cyber-dashboard { 
  --red: #D92323; 
  --black: #111111; 
  --off-white: #F4F1EA; 
  --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  --body: 'Inter', sans-serif;
  --gap: 20px;
  
  --border-thick: 2px solid var(--black);
  --border-thin: 1px solid var(--black);
  --shadow-hard: 4px 4px 0 rgba(0,0,0,0.15);
  
  width: 100%; height: 100vh; 
  background-color: var(--off-white); 
  display: flex; flex-direction: column; overflow: hidden; 
  font-family: var(--body);
  color: var(--black);
}

/* --- 背景与容器 --- */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(var(--gray) 1px, transparent 1px), linear-gradient(90deg, var(--gray) 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 30s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-50px); } }

.panel-scroll-container { 
  flex: 1; overflow-y: auto; position: relative; z-index: 1; display: flex; flex-direction: column; 
}

/* --- 头部重构 --- */
.main-header { 
  display: flex; flex-wrap: wrap; 
  border-bottom: 4px solid var(--black); 
  background: var(--off-white); position: relative; z-index: 10;
}
.header-split { padding: 30px; }
.header-split.left { 
  background: var(--black); color: var(--off-white); 
  display: flex; justify-content: center; align-items: center; 
  flex: 0 0 350px;
}
.giant-text { 
  font-family: var(--heading); font-size: 3.5rem; line-height: 0.9; 
  text-transform: uppercase; transform: rotate(-2deg); 
}
.text-row.outline { color: var(--black); -webkit-text-stroke: 1px var(--off-white); }
.text-row.red-fill { color: var(--red); margin-left: 20px; }

.header-split.right { flex: 1; display: flex; align-items: center; justify-content: space-between; }
.cn-title { font-weight: 900; margin: 0 0 5px 0; font-size: 1.5rem; letter-spacing: -1px; }
.live-indicator { display: inline-flex; align-items: center; gap: 8px; font-family: var(--mono); font-size: 0.8rem; color: var(--red); border: 1px solid var(--red); padding: 4px 8px; margin-bottom: 5px; background: rgba(217, 35, 35, 0.05); }
.dot { width: 8px; height: 8px; background: var(--red); border-radius: 50%; animation: pulse 1s infinite; }
.sys-time-display { font-family: var(--mono); font-weight: 700; font-size: 1.2rem; }
.tech-lines { font-family: var(--mono); font-size: 0.75rem; color: #666; margin-top: 5px; }
.tech-lines span { margin-right: 15px; }

/* --- 跑马灯 --- */
.tech-strip { 
  background: var(--black); color: var(--off-white); 
  padding: 6px 0; border-bottom: 4px solid var(--black); 
  overflow: hidden; white-space: nowrap; font-family: var(--mono); font-size: 0.8rem; 
}
.strip-content { display: inline-block; animation: marquee 20s linear infinite; }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }

/* --- 主布局网格 --- */
.main-bridge { 
  display: flex; padding: var(--gap); gap: var(--gap); 
  max-width: 2560px; margin: 0 auto; width: 100%; box-sizing: border-box; 
  align-items: flex-start;
}
.rank-column { flex: 0 0 240px; }
.left-column { flex: 0 0 260px; }
.mid-column { flex: 1; display: flex; flex-direction: column; gap: var(--gap); min-width: 0; }
.right-column { flex: 0 0 300px; display: flex; flex-direction: column; gap: var(--gap); }
.mid-content-split { display: flex; gap: var(--gap); flex: 1; }
.action-sidebar { width: 220px; display: flex; flex-direction: column; gap: 10px; flex-shrink: 0; }
.posts-container { flex: 1; min-width: 0; }

/* --- [NEW] 模块切换与聊天室样式 --- */
.module-switcher-bar {
  display: flex; gap: 10px; margin-bottom: 5px;
  border-bottom: 2px solid var(--black); padding-bottom: 15px;
}
.module-tab {
  flex: 1; height: 50px; background: transparent;
  border: 1px solid var(--black);
  display: flex; align-items: center; justify-content: center; gap: 10px;
  cursor: pointer; font-family: var(--mono); font-size: 0.9rem;
  transition: all 0.2s; position: relative; color: #666;
  background: rgba(255,255,255,0.5);
}
.module-tab:hover { background: #fff; color: var(--black); }
.module-tab.active {
  background: var(--black); color: var(--off-white);
  border: 2px solid var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.2);
  transform: translate(-2px, -2px); font-weight: 700;
}
.unread-dot {
  width: 8px; height: 8px; background: var(--red);
  border-radius: 50%; position: absolute; top: 6px; right: 6px;
  animation: pulse 1s infinite;
}

.chat-module-wrapper {
  height: 100%; /* 与帖子列表高度对齐 */
  width: 100%; border: 2px solid var(--black);
  background: #fff; box-shadow: var(--shadow-hard);
  overflow: hidden; position: relative;
}

/* 强制覆盖 ChatRoom 内部样式以适应嵌入 */
:deep(.embedded-chat) {
  height: 100% !important;
  width: 100% !important;
  min-height: 0 !important;
}
:deep(.embedded-chat .cyber-chat-container) {
  height: 100%;
}

/* 切换动画 */
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.3s ease; }
.fade-slide-enter-from { opacity: 0; transform: translateY(10px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }


/* --- 通用卡片样式 (Cyber Card) --- */
.cyber-card { 
  background: #fff; border: 2px solid var(--black); 
  box-shadow: var(--shadow-hard); display: flex; flex-direction: column; 
  overflow: hidden;
}
.card-label-strip { 
  background: var(--black); color: var(--off-white); 
  padding: 4px 10px; font-family: var(--mono); font-size: 0.7rem; 
}
.card-inner-pad { padding: 10px; }
.full-height { height: 600px; max-height: 80vh; }

/* --- 博客列表 --- */
.blog-min-list { padding: 10px; overflow-y: auto; flex: 1; }
.status-text { font-family: var(--mono); color: #888; text-align: center; padding: 20px; font-size: 0.8rem; }
.min-blog-item { 
  display: flex; gap: 10px; padding: 10px; margin-bottom: 10px; 
  border: 1px solid #ddd; transition: all 0.2s; position: relative; 
  background: #fdfdfd; cursor: pointer;
}
.min-blog-item:hover { transform: translate(-2px, -2px); border-color: var(--black); box-shadow: 2px 2px 0 var(--black); }
.blog-indicator { width: 4px; background: #ddd; position: absolute; left: 0; top: 0; bottom: 0; transition: 0.2s; }
.min-blog-item:hover .blog-indicator { background: var(--red); }
.min-blog-thumb { width: 60px; height: 60px; border: 1px solid var(--black); flex-shrink: 0; }
.min-blog-thumb img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); transition: 0.3s; }
.min-blog-item:hover img { filter: grayscale(0); }
.blog-content { flex: 1; display: flex; flex-direction: column; justify-content: center; }
.blog-top-row { display: flex; justify-content: space-between; font-size: 0.7rem; font-family: var(--mono); margin-bottom: 4px; }
.blog-accent { background: var(--red); color: #fff; padding: 0 4px; }
.blog-title { font-weight: 700; font-size: 0.9rem; margin: 0; line-height: 1.3; }

/* --- Banner --- */
.activity-banner { 
  height: 160px; background: var(--black); color: var(--off-white); 
  border: 2px solid var(--black); position: relative; display: flex; align-items: center; 
  padding: 0 40px; overflow: hidden; box-shadow: var(--shadow-hard); margin-bottom: 20px;
}
.banner-content { position: relative; z-index: 2; }
.banner-badge { background: var(--red); color: #fff; font-family: var(--mono); font-size: 0.75rem; padding: 2px 6px; display: inline-block; margin-bottom: 10px; }
.banner-title { font-family: var(--heading); font-size: 2.5rem; margin: 0; text-transform: uppercase; }
.banner-sub { font-family: var(--mono); color: #888; font-size: 0.8rem; margin-top: 5px; }
.banner-pattern { 
  position: absolute; right: -50px; top: -50px; width: 300px; height: 300px; 
  background: repeating-linear-gradient(45deg, #222, #222 10px, #111 10px, #111 20px); 
  z-index: 1; opacity: 0.5; border-radius: 50%;
}

/* --- 帖子列表 --- */
.flow-header { 
  padding: 15px; border-bottom: 2px solid var(--black); 
  display: flex; justify-content: space-between; align-items: center; background: #fff;
}
.header-title { font-family: var(--heading); font-size: 1.2rem; display: flex; align-items: center; gap: 8px; }
.header-tabs { display: flex; gap: -1px; }
.tab { 
  padding: 6px 12px; font-family: var(--mono); font-size: 0.8rem; cursor: pointer; 
  border: 1px solid transparent; font-weight: 700; color: #888;
}
.tab.active { background: var(--black); color: var(--off-white); border: 1px solid var(--black); }

.posts-scroll-area { height: 600px; overflow-y: auto; background: #f0f0f0; padding: 15px; }
.post-entry { 
  background: #fff; border: 1px solid var(--black); margin-bottom: 12px; 
  padding: 20px; position: relative; display: flex; gap: 15px; cursor: pointer; 
  transition: transform 0.2s; overflow: hidden;
}
.post-entry:hover { transform: translateX(5px); box-shadow: -4px 4px 0 rgba(0,0,0,0.1); }
.entry-scanline { 
  position: absolute; left: 0; top: 0; width: 4px; height: 100%; 
  background: var(--black); transform: scaleY(0); transition: transform 0.2s; 
}
.post-entry:hover .entry-scanline { transform: scaleY(1); }
.entry-main { flex: 1; display: flex; flex-direction: column; justify-content: space-between; }
.entry-user-row { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; }
.avatar-box { width: 24px; height: 24px; border: 1px solid var(--black); }
.avatar-box img { width: 100%; height: 100%; object-fit: cover; }
.username { font-family: var(--mono); font-weight: 700; font-size: 0.8rem; }
.post-time { font-family: var(--mono); font-size: 0.7rem; color: #666; }
.entry-title { font-family: var(--body); font-weight: 800; font-size: 1.1rem; margin: 5px 0 10px 0; }
.entry-actions { display: flex; gap: 15px; font-family: var(--mono); font-size: 0.7rem; color: #555; }
.entry-thumb-wrapper { width: 90px; height: 90px; border: 1px solid var(--black); position: relative; }
.entry-thumb { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); }
.post-entry:hover .entry-thumb { filter: grayscale(0); }
.img-count-badge { position: absolute; bottom: 0; right: 0; background: var(--red); color: #fff; font-size: 0.7rem; padding: 2px 4px; font-family: var(--mono); }

/* --- 按钮 --- */
.cyber-btn { 
  background: #fff; border: 1px solid var(--black); padding: 0; 
  display: flex; height: 50px; cursor: pointer; transition: 0.2s; 
  box-shadow: 2px 2px 0 var(--black);
}
.cyber-btn:hover { transform: translate(-2px, -2px); box-shadow: 4px 4px 0 var(--black); }
.btn-icon { width: 40px; background: var(--black); color: #fff; display: flex; align-items: center; justify-content: center; }
.btn-text { flex: 1; display: flex; flex-direction: column; justify-content: center; padding-left: 10px; text-align: left; }
.btn-text .main { font-weight: 800; font-size: 0.9rem; }
.btn-text .sub { font-family: var(--mono); font-size: 0.6rem; color: #666; }
.red-mode .btn-icon { background: var(--red); }
.red-mode:hover .btn-text .sub { color: var(--red); }

/* --- 右侧组件 --- */
.cyber-terminal { 
  background: var(--black); color: var(--off-white); font-family: var(--mono); 
  padding: 15px; border: 2px solid var(--black); box-shadow: var(--shadow-hard); 
}
.terminal-header { border-bottom: 1px dashed #555; padding-bottom: 8px; margin-bottom: 10px; font-size: 0.8rem; }
.blink { animation: pulse 0.8s infinite; }
.metric-row { display: flex; justify-content: space-between; margin-bottom: 6px; font-size: 0.9rem; }
.metric-row .val { color: var(--red); font-weight: 700; }

.news-list { padding: 15px; }
.news-item { display: flex; gap: 8px; margin-bottom: 10px; align-items: flex-start; font-size: 0.85rem; }
.news-marker { font-family: var(--mono); color: var(--red); font-weight: 700; }

.online-panel { min-height: 200px; }
.card-header-simple { padding: 10px 15px; border-bottom: 1px solid #ddd; display: flex; justify-content: space-between; align-items: center; }
.header-label { font-family: var(--heading); font-size: 1.1rem; }
.online-badge { font-family: var(--mono); font-size: 0.8rem; display: flex; align-items: center; gap: 6px; color: #009966; }
.pulse-green { width: 6px; height: 6px; background: #009966; border-radius: 50%; animation: pulse 1s infinite; }
.user-grid { padding: 10px; display: grid; grid-template-columns: repeat(auto-fill, minmax(36px, 1fr)); gap: 8px; }
.online-avatar { width: 36px; height: 36px; border: 1px solid var(--black); transition: 0.2s; }
.online-avatar:hover { transform: scale(1.2); z-index: 5; border-color: var(--red); }
.online-avatar img { width: 100%; height: 100%; object-fit: cover; }
.empty-online { padding: 20px; text-align: center; font-family: var(--mono); color: #888; font-size: 0.7rem; }

/* --- Modal (重制版) --- */
.cyber-modal-overlay { 
  position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 9999; 
  display: flex; justify-content: center; align-items: center; backdrop-filter: blur(2px);
}
.cyber-modal-window { 
  width: 90vw; max-width: 1000px; height: 90vh; 
  background: #fff; border: 4px solid var(--black); 
  display: flex; flex-direction: column; 
  box-shadow: 10px 10px 0 rgba(0,0,0,0.5);
}
.cyber-modal-header { 
  height: 50px; background: var(--black); color: var(--off-white); 
  display: flex; justify-content: space-between; align-items: center; padding: 0 15px; 
}
.modal-title-block { display: flex; align-items: center; gap: 10px; font-family: var(--mono); }
.deco-box { color: var(--red); }
.cyber-close-btn { 
  background: transparent; border: 1px solid #555; color: var(--off-white); 
  font-family: var(--mono); padding: 5px 10px; cursor: pointer; transition: 0.2s; 
}
.cyber-close-btn:hover { background: var(--red); border-color: var(--red); }

.detail-wrapper { flex: 1; overflow-y: auto; background: var(--off-white); display: flex; flex-direction: column; }
.detail-hero { 
  height: 300px; background-size: cover; background-position: center; position: relative; 
  display: flex; flex-direction: column; justify-content: flex-end; 
  border-bottom: 2px solid var(--black);
}
.hero-overlay-grid { 
  position: absolute; inset: 0; background: rgba(0,0,0,0.4); 
  background-image: linear-gradient(rgba(0,0,0,0.5) 1px, transparent 1px); 
  background-size: 100% 4px;
}
.hero-content { position: relative; z-index: 2; padding: 30px; color: #fff; text-shadow: 2px 2px 0 #000; }
.hero-title { font-family: var(--heading); font-size: 3rem; margin: 10px 0; text-transform: uppercase; line-height: 1; }
.hero-meta-tag { background: var(--red); display: inline-block; padding: 2px 8px; font-family: var(--mono); font-size: 0.8rem; }
.hero-time { font-family: var(--mono); font-size: 0.9rem; opacity: 0.8; }

.detail-header-simple { padding: 40px; background: #fff; border-bottom: 2px solid var(--black); }
.simple-title { font-family: var(--heading); font-size: 2.5rem; margin-bottom: 10px; }
.simple-meta { font-family: var(--mono); color: #666; display: flex; justify-content: space-between; border-top: 1px dashed #ccc; padding-top: 10px; }
.meta-tag { color: var(--black); font-weight: 700; }

.detail-body { padding: 40px; font-family: var(--body); font-size: 1.1rem; line-height: 1.8; color: #222; max-width: 800px; margin: 0 auto; }
.detail-gallery { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 10px; margin-bottom: 20px; }
.gallery-img { width: 100%; border: 1px solid var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.divider-strip { background: var(--black); color: #fff; text-align: center; font-family: var(--mono); font-size: 0.8rem; padding: 5px; margin-top: 40px; }

.comment-section { padding: 30px; background: #eee; flex: 1; }
.section-label { font-family: var(--heading); font-size: 1.2rem; margin-bottom: 20px; border-left: 5px solid var(--red); padding-left: 10px; }
.comment-block { 
  background: #fff; border: 1px solid var(--black); padding: 15px; margin-bottom: 15px; 
  box-shadow: 2px 2px 0 rgba(0,0,0,0.1); 
}
.comment-block.is-reply { border-left: 4px solid var(--black); margin-left: 10px; background: #f9f9f9; }
.comment-top { font-family: var(--mono); font-size: 0.8rem; margin-bottom: 8px; color: #555; }
.c-name { color: var(--black); font-weight: 700; margin-right: 10px; }
.reply-tag { background: #ddd; padding: 0 4px; margin-right: 10px; }
.reply-text-btn { background: none; border: none; font-family: var(--mono); color: var(--red); cursor: pointer; font-size: 0.7rem; margin-top: 5px; }

.detail-footer-wrapper { padding: 15px; border-top: 2px solid var(--black); background: #fff; }
.reply-status-bar { font-family: var(--mono); font-size: 0.8rem; margin-bottom: 10px; display: flex; justify-content: space-between; background: #f0f0f0; padding: 5px 10px; }
.cancel-reply { background: none; border: none; text-decoration: underline; cursor: pointer; }
.detail-footer-input { display: flex; gap: 10px; }
.cyber-input { flex: 1; border: 2px solid var(--black); padding: 10px; font-family: var(--mono); outline: none; }
.cyber-input:focus { background: #fdfdfd; border-color: var(--red); }
.send-btn-rect { 
  background: var(--black); color: #fff; border: none; padding: 0 20px; 
  font-family: var(--heading); cursor: pointer; transition: 0.2s; 
}
.send-btn-rect:hover { background: var(--red); }
.send-btn-rect:disabled { background: #ccc; cursor: not-allowed; }

/* --- 响应式 --- */
@media screen and (max-width: 1200px) {
  .rank-column { display: none; }
  .left-column { flex: 0 0 240px; }
  .right-column { flex: 0 0 280px; }
}

@media screen and (max-width: 1920px) {
  .cyber-dashboard { zoom: 0.8; } /* 保持原来的缩放习惯 */
  .rank-column { display: flex !important; }
}

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }
</style>