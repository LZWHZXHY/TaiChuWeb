<template>
  <div class="cyber-dashboard">
    <div class="grid-bg moving-grid"></div>

    <header class="main-header">
      <div class="header-split left">
        <h1 class="giant-text glitch-hover">
          <div class="text-row">TAICHU!!</div>
          <div class="text-row outline">NETWORK</div>
          <div class="text-row red-fill">社区</div>
        </h1>
      </div>
      <div class="info-block">
        <h2 class="cn-title">太初中枢 // NEURAL_HUB</h2>
        <div class="live-indicator"><span class="dot"></span> SYSTEM ONLINE</div>
      </div>
      <div class="tech-lines">
        <span>// NODE_SYNC: {{ stats.posts }} POSTS</span>
        <span>// DB_ARCHIVE: {{ stats.blogs }} BLOGS</span>
        <div class="sys-time-display">{{ currentTime }}</div>
      </div>
    </header>

    <div class="tech-strip">
      <div class="strip-content">
        // 官网开发中 // 关注太初寰宇 // Steam 愿望单开启 // 欢迎加入太初网络 // SYSTEM_READY //
      </div>
    </div>

    <div class="main-bridge">
      
      <aside class="rank-column">
        <div class="cyber-card full-height-card">
          <div class="card-label-strip"><span>// RANKING</span></div>
          <div class="card-inner-pad"><ActivityRanking /></div>
        </div>
      </aside>

      <aside class="left-column">
        <section class="cyber-card full-height-card">
          <div class="card-label-strip"><span>// BLOG_ARCHIVES</span></div>
          <div class="blog-min-list custom-scroll">
            <div v-if="blogLoading" class="status-text">[ LOADING... ]</div>
            <BlogItem v-for="b in blogs" :key="b.Id" :blog="b" :base-url="BASE_URL" @click="openBlogDetail(b.Id)" />
          </div>
        </section>
      </aside>

      <main class="mid-column">
        
        <div class="module-switcher-bar">
          <button class="module-tab" :class="{ active: currentModule === 'post' }" @click="currentModule = 'post'">
            <span class="tab-icon">■</span> POST_STREAM // 社区流
          </button>
          <button class="module-tab" :class="{ active: currentModule === 'chat' }" @click="currentModule = 'chat'">
            <span class="tab-icon">●</span> SECURE_CHAT // 加密通讯
            <span class="unread-dot"></span> 
          </button>
        </div>

        <Transition name="fade-slide" mode="out-in">
          
          <div v-if="currentModule === 'post'" class="mid-content-split" key="post">
            <section class="posts-container cyber-card">
              <div class="flow-header">
                <span class="header-title"><span class="icon">■</span> COMMUNITY_POSTS</span>
                <div class="mini-banner">公告: 太初宇宙 5.0 开发中...</div>
              </div>
              
              <div class="posts-scroll-area custom-scroll" @scroll="handleScroll">
                <PostItem v-for="p in posts" :key="p.id" :post="p" :base-url="BASE_URL" @click="openPostDetail(p.id)" />
                <div v-if="loading" class="loading-status">SYNCING_STREAM...</div>
                <div class="bottom-spacer"></div> </div>
            </section>

            <aside class="action-sidebar">
              <div class="section-label">// ACTIONS</div>
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
            </aside>
          </div>

          <div v-else class="chat-module-wrapper" key="chat">
            <ChatRoom class="embedded-chat" />
          </div>

        </Transition>
      </main>

      <aside class="right-column">
        <div class="cyber-terminal">
           <div class="terminal-header"><span>> METRICS.exe</span><span class="blink">_</span></div>
           <div class="metric-row"><span class="label">POST_INDEX:</span><span class="val">{{ stats.posts }}</span></div>
           <div class="metric-row"><span class="label">BLOG_INDEX:</span><span class="val">{{ stats.blogs }}</span></div>
        </div>

        <div class="cyber-card news-panel">
          <div class="card-label-strip">
            <div class="label-left"><i class="fas fa-terminal"></i> <span>// SYSTEM_LOGS</span></div>
            <div class="live-status"><span class="blink-dot"></span> LIVE</div>
          </div>
          <div class="news-list custom-scroll">
            <div v-for="n in news" :key="n.id" class="news-item">
              <div class="item-header">
                <span class="news-tag" :class="n.type">{{ n.type }}</span>
                <span class="news-date">{{ n.date }}</span>
              </div>
              <div class="item-content"><span class="arrow">>></span> {{ n.text }}</div>
            </div>
          </div>
        </div>

        <div class="cyber-card online-panel">
          <div class="card-header-simple">
            <span class="header-label">ONLINE</span>
            <div class="online-badge"><span class="pulse-green"></span> {{ onlineStore.onlineCount }}</div>
          </div>
          <div class="online-list custom-scroll">
            <div class="user-grid">
               <div v-for="u in onlineStore.onlineUsersList" :key="u.userId" class="online-avatar" :title="u.userName">
                 <img :src="fixAvatarUrl(u.avatar)" />
               </div>
            </div>
          </div>
        </div>
      </aside>
    </div>

    <CreatePost v-model="showPostForm" @success="handlePostSuccess" />

    <Teleport to="body">
      <Transition name="modal-scale">
        <div v-if="showDetail" class="cyber-modal-overlay" @click.self="closeDetail">
          <div class="cyber-modal-window">
            <div class="cyber-modal-header">
              <div class="modal-title-block">
                <span class="deco-box">■</span>
                <span class="title-text">DATA_VIEWER // {{ currentType.toUpperCase() }}</span>
              </div>
              <button class="cyber-close-btn" @click="closeDetail">CLOSE [X]</button>
            </div>

            <div v-if="detailLoading" class="status-text">DOWNLOADING_STREAM...</div>
            <div v-else class="detail-wrapper custom-scroll">
              <PostDetailContent 
                v-if="currentType === 'post' && currentData" 
                :data="currentData" 
                :comments="comments" 
                :replyTarget="replyTarget"
                :fixAvatar="fixAvatarUrl"
                @reply="handleReplyTo"
                @cancelReply="cancelReply"
                @submitComment="submitComment" 
              />
              <BlogDetailContent v-else-if="currentType === 'blog' && currentData" :data="currentData" :fixAvatar="fixAvatarUrl" />
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';
import { useOnlineStore } from '@/stores/online';

// 组件引入
import PostItem from '@/comminicateCenter/PostItem.vue';
import BlogItem from '@/comminicateCenter/BlogItem.vue';
import PostDetailContent from '@/comminicateCenter/PostDetailContent.vue'; // 确保这是最新的高对比度版
import BlogDetailContent from '@/comminicateCenter/BlogDetailContent.vue';
import CreatePost from '@/comminicateCenter/CreatePost.vue';
import ActivityRanking from '@/comminicateCenter/ActivityRanking.vue';
import ChatRoom from '@/ChatRoom/ChatRoom.vue';

const router = useRouter(); 
const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;
const onlineStore = useOnlineStore();

const BASE_URL = window.location.hostname === 'localhost' ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const currentModule = ref('post');
const posts = ref([]);
const blogs = ref([]);
const stats = reactive({ blogs: 0, posts: 0 });
const loading = ref(false);
const blogLoading = ref(false);
const hasMore = ref(true);
const currentPage = ref(1);

// 新闻数据
const news = ref([
  { id: 1, type: 'SYS', date: '01.18', text: '太初宇宙 5.0 核心架构重构完成。' },
  { id: 2, type: 'UPD', date: '01.15', text: '新增 "ThePost" 社区高频交互协议。' },
  { id: 3, type: 'WAR', date: '01.12', text: '检测到外部未知信号接入...' },
  { id: 4, type: 'ACT', date: '01.10', text: 'Steam 愿望单添加活动开启。' }
]);

const showDetail = ref(false);
const showPostForm = ref(false);
const detailLoading = ref(false);
const currentType = ref('post'); 
const currentData = ref(null);
const comments = ref([]);
const replyTarget = ref(null);

const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

// --- API 方法 ---
const fetchPosts = async (isFirstLoad = true) => {
  if (loading.value || (!hasMore.value && !isFirstLoad)) return;
  loading.value = true;
  const targetPage = isFirstLoad ? 1 : currentPage.value + 1;
  try {
    const res = await apiClient.get('/ThePost', { params: { page: targetPage, pageSize: 10 } });
    if (res.data.success) {
      posts.value = isFirstLoad ? res.data.data : [...posts.value, ...res.data.data];
      currentPage.value = targetPage;
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
      apiClient.get('/ThePost/postAmount')
    ]);
    stats.blogs = blogRes.data;
    stats.posts = postRes.data;
  } catch (e) { console.error(e); }
};

// --- 详情与交互 ---
const openPostDetail = async (id) => {
  currentType.value = 'post'; showDetail.value = true; detailLoading.value = true;
  comments.value = []; replyTarget.value = null; // 重置
  try {
    const postRes = await apiClient.get(`/ThePost/${id}`);
    if (postRes.data.success) currentData.value = postRes.data.data;
    // 获取评论 (由 Detail 组件内部也可再次获取，这里预加载)
    const commentRes = await apiClient.get(`/ThePost/${id}/comments`);
    if(commentRes.data.success) comments.value = processCommentTree(commentRes.data.data);
  } finally { detailLoading.value = false; }
};

const openBlogDetail = async (id) => {
  currentType.value = 'blog'; showDetail.value = true; detailLoading.value = true;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    currentData.value = res.data;
  } finally { detailLoading.value = false; }
};

const submitComment = async (content) => {
  if(!currentData.value) return;
  try {
    const res = await apiClient.post(`/ThePost/${currentData.value.id}/comments/json`, {
      Content: content,
      ParentCommentId: replyTarget.value?.id || 0
    });
    if(res.data.success) {
      // 重新拉取评论刷新列表
      const refresh = await apiClient.get(`/ThePost/${currentData.value.id}/comments`);
      comments.value = processCommentTree(refresh.data.data);
      replyTarget.value = null; // 提交后取消回复状态
      currentData.value.comment_count++; // 手动加1显示
    }
  } catch(e) { alert("发送失败: " + e.message); }
};

// 辅助：评论树处理
const processCommentTree = (list) => {
  const map = {}, result = [];
  list.forEach(item => { item.level = 0; map[item.id] = item; });
  list.forEach(item => {
    if (item.ParentCommentId && map[item.ParentCommentId]) {
      item.level = map[item.ParentCommentId].level + 1;
      item.replyToUser = map[item.ParentCommentId].author.username;
    }
    result.push(item);
  });
  return result.sort((a, b) => new Date(b.createTime) - new Date(a.createTime));
};

const handlePostSuccess = async () => {
  hasMore.value = true;
  await fetchPosts(true);
};

const handleScroll = (e) => {
  if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight - 50) fetchPosts(false);
};

const handleReplyTo = (c) => { replyTarget.value = c; };
const cancelReply = () => { replyTarget.value = null; };
const closeDetail = () => { showDetail.value = false; currentData.value = null; };

onMounted(() => {
  fetchPosts(true); fetchBlogs(); fetchStats();
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});
onUnmounted(() => { clearInterval(clockTimer); });
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 0. 全局变量 --- */
.cyber-dashboard { 
  --red: #D92323; --black: #111111; --off-white: #F4F1EA; --gray: #E0DDD5; 
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif; --body: 'Inter', sans-serif;
  --gap: 24px; --shadow-hover: 6px 6px 0 var(--black);
  
  width: 100%; height: 100vh; /* 强制占满视口 */
  background-color: var(--off-white); 
  display: flex; flex-direction: column; overflow: hidden; /* 禁止整页滚动 */
  font-family: var(--body); color: var(--black);
}

.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17,17,17,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17,17,17,0.05) 1px, transparent 1px); 
  background-size: 40px 40px; pointer-events: none; z-index: 0; 
}
.moving-grid { animation: gridScroll 60s linear infinite; }

/* --- 1. 顶部 Header (固定高度) --- */
.main-header { 
  display: flex; height: 80px; flex-shrink: 0; z-index: 100;
  background: rgba(244, 241, 234, 0.95); backdrop-filter: blur(10px);
  border-bottom: 3px solid var(--black); box-shadow: 0 4px 20px rgba(0,0,0,0.05);
}
.header-split.left { background: var(--black); color: var(--off-white); padding: 0 40px; display: flex; align-items: center; width: 320px; clip-path: polygon(0 0, 100% 0, 95% 100%, 0 100%); }
.giant-text { font-family: var(--heading); font-size: 3.2rem; line-height: 0.85; transform: skewX(-10deg); text-shadow: 2px 2px 0 var(--red); }
.text-row.outline { -webkit-text-stroke: 1px var(--off-white); color: transparent; }
.text-row.red-fill { color: var(--off-white); margin-left: 20px; }

.info-block { flex: 1; padding: 0 30px; display: flex; flex-direction: column; justify-content: center; }
.cn-title { font-weight: 900; font-size: 1.4rem; margin: 0; }
.live-indicator { font-family: var(--mono); color: var(--red); font-size: 0.75rem; border: 1px solid var(--red); padding: 4px 8px; margin-top: 5px; width: fit-content; background: rgba(217,35,35,0.05); }
.dot { width: 6px; height: 6px; background: var(--red); border-radius: 50%; display: inline-block; animation: pulse 2s infinite; margin-right: 6px; }

.tech-lines { padding: 0 40px; display: flex; flex-direction: column; justify-content: center; align-items: flex-end; font-family: var(--mono); font-size: 0.75rem; color: #666; border-left: 1px solid #ddd; }
.sys-time-display { font-size: 1.5rem; font-weight: 700; margin-top: 4px; }

/* --- 2. 跑马灯 (固定高度) --- */
.tech-strip { background: var(--black); color: var(--off-white); height: 36px; display: flex; align-items: center; border-bottom: 2px solid #333; overflow: hidden; flex-shrink: 0; }
.strip-content { font-family: var(--mono); font-size: 0.85rem; white-space: nowrap; animation: marquee 40s linear infinite; }

/* --- 3. 主布局桥接 (Flex 布局核心修复) --- */
.main-bridge { 
  flex: 1; /* 占据剩余所有空间 */
  min-height: 0; /* 允许子元素滚动 */
  display: flex; gap: var(--gap); padding: var(--gap); 
  max-width: 1920px; margin: 0 auto; width: 100%; box-sizing: border-box;
  overflow: hidden; /* 防止 Bridge 本身滚动 */
}

/* 列布局通用设置 */
.rank-column { flex: 0 0 260px; height: 100%; display: flex; flex-direction: column; }
.left-column { flex: 0 0 280px; height: 100%; display: flex; flex-direction: column; }
.right-column { flex: 0 0 320px; height: 100%; display: flex; flex-direction: column; gap: 20px; }

/* 中间列：必须自适应并允许内部滚动 */
.mid-column { 
  flex: 1; min-width: 0; 
  height: 100%; display: flex; flex-direction: column; 
}

.mid-content-split { 
  flex: 1; /* 占据剩余高度 */
  min-height: 0; /* 关键：允许内部滚动 */
  display: flex; gap: var(--gap); 
}

/* 帖子容器 */
.posts-container { 
  flex: 1; /* 占满中间列左侧 */
  display: flex; flex-direction: column; 
  overflow: hidden; /* 裁剪溢出 */
  background: #fff; border: 2px solid var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.flow-header { padding: 15px 20px; background: #fff; border-bottom: 2px solid var(--black); display: flex; justify-content: space-between; align-items: center; }
.header-title { font-family: var(--heading); font-size: 1.5rem; display: flex; align-items: center; gap: 10px; }
.mini-banner { font-family: var(--mono); font-size: 0.7rem; background: var(--black); color: #fff; padding: 2px 8px; }

/* ✨ 修复点：滚动区域设置 */
.posts-scroll-area { 
  flex: 1; /* 自动填充剩余空间 */
  overflow-y: auto; /* 允许垂直滚动 */
  padding: 20px; 
  background-color: #e8e8e8; 
  background-image: radial-gradient(#ccc 1px, transparent 1px); background-size: 20px 20px;
}
.bottom-spacer { height: 40px; } /* 底部留白 */

/* 聊天室容器修复 */
.chat-module-wrapper { 
  flex: 1; height: 100%; /* 占满高度 */
  border: 2px solid var(--black); background: #fff; box-shadow: 4px 4px 0 rgba(0,0,0,0.15); 
  overflow: hidden; 
}

/* 侧边操作栏 */
.action-sidebar { width: 200px; flex-shrink: 0; display: flex; flex-direction: column; gap: 15px; overflow-y: auto; }

/* --- 4. 组件样式 (Card, Btn, News) --- */
.cyber-card { background: #fff; border: 2px solid var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); display: flex; flex-direction: column; overflow: hidden; transition: transform 0.2s, box-shadow 0.2s; border-radius: 2px; }
.cyber-card:hover { transform: translateY(-2px); box-shadow: var(--shadow-hover); }
.full-height-card { height: 100%; flex: 1; }
.card-label-strip { background: var(--black); color: var(--off-white); padding: 8px 12px; font-family: var(--mono); font-size: 0.75rem; font-weight: 700; display: flex; justify-content: space-between; align-items: center; flex-shrink: 0; }
.blog-min-list { flex: 1; overflow-y: auto; }

.module-switcher-bar { display: flex; gap: 12px; padding-bottom: 10px; border-bottom: 3px solid var(--black); margin-bottom: 15px; flex-shrink: 0; }
.module-tab { flex: 1; height: 55px; border: 2px solid var(--black); background: #fff; cursor: pointer; transition: 0.2s; display: flex; align-items: center; justify-content: center; gap: 10px; font-family: var(--mono); font-weight: 700; color: #666; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.module-tab:hover { transform: translateY(-2px); box-shadow: 4px 6px 0 var(--black); color: var(--black); }
.module-tab.active { background: var(--black); color: var(--off-white); transform: translate(2px, 2px); box-shadow: none; }

.cyber-btn { position: relative; height: 60px; width: 100%; background: #fff; border: 2px solid var(--black); display: flex; cursor: pointer; transition: 0.15s; box-shadow: 4px 4px 0 var(--black); padding: 0; }
.cyber-btn:active { transform: translate(2px, 2px); box-shadow: 2px 2px 0 var(--black); }
.cyber-btn:hover { background: #fffdf5; transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.btn-icon { width: 50px; background: var(--black); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; }
.red-mode .btn-icon { background: var(--red); }
.btn-text { flex: 1; display: flex; flex-direction: column; justify-content: center; padding-left: 15px; text-align: left; }
.btn-text .main { font-weight: 800; font-size: 1rem; }
.btn-text .sub { font-family: var(--mono); font-size: 0.6rem; color: #888; }

/* News Panel */
.news-panel { height: 320px; flex-shrink: 0; display: flex; flex-direction: column; }
.news-list { flex: 1; overflow-y: auto; padding: 15px; background: #f4f4f4; box-shadow: inset 0 0 10px rgba(0,0,0,0.05); }
.news-item { margin-bottom: 12px; background: #fff; border-left: 3px solid #ccc; padding: 10px; font-family: var(--mono); box-shadow: 2px 2px 0 rgba(0,0,0,0.05); transition: 0.2s; }
.news-item:hover { transform: translateX(4px); border-left-color: var(--black); background: #fffdf0; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.item-header { display: flex; justify-content: space-between; margin-bottom: 6px; font-size: 0.7rem; }
.news-tag { padding: 1px 6px; color: #fff; font-weight: bold; border-radius: 2px; }
.news-tag.SYS { background: var(--black); }
.news-tag.UPD { background: #009966; }
.news-tag.WAR { background: var(--red); }
.news-tag.ACT { background: #dba11c; color: #000; }
.item-content { font-size: 0.85rem; font-weight: 600; display: flex; gap: 6px; }
.item-content .arrow { color: var(--red); font-weight: 900; }

/* Terminal & Online */
.cyber-terminal { background: var(--black); color: var(--off-white); padding: 20px; font-family: var(--mono); border-radius: 4px; box-shadow: 4px 4px 0 rgba(0,0,0,0.2); flex-shrink: 0; }
.terminal-header { border-bottom: 1px dashed #555; padding-bottom: 10px; margin-bottom: 15px; display: flex; justify-content: space-between; }
.metric-row { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 0.9rem; }
.metric-row .val { color: var(--red); font-weight: bold; }

.online-panel { flex: 1; min-height: 0; display: flex; flex-direction: column; }
.card-header-simple { padding: 12px 15px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; flex-shrink: 0; }
.online-list { flex: 1; overflow-y: auto; padding: 15px; }
.user-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(40px, 1fr)); gap: 12px; }
.online-avatar { width: 40px; height: 40px; border: 2px solid var(--black); border-radius: 4px; overflow: hidden; transition: 0.2s; }
.online-avatar:hover { transform: scale(1.1) rotate(3deg); border-color: var(--red); z-index: 10; box-shadow: 0 4px 8px rgba(0,0,0,0.2); }
.online-avatar img { width: 100%; height: 100%; object-fit: cover; }

/* 模态框 */
.cyber-modal-overlay { position: fixed; inset: 0; z-index: 9999; background: rgba(17,17,17,0.85); backdrop-filter: blur(8px); display: flex; justify-content: center; align-items: center; }
.cyber-modal-window { width: 90vw; max-width: 1200px; height: 90vh; background: #f4f4f4; border: 1px solid #444; display: flex; flex-direction: column; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.5); border-radius: 4px; overflow: hidden; }
.cyber-modal-header { height: 60px; background: var(--black); color: var(--off-white); display: flex; justify-content: space-between; align-items: center; padding: 0 25px; box-shadow: 0 4px 10px rgba(0,0,0,0.3); z-index: 10; flex-shrink: 0; }
.cyber-close-btn { background: rgba(255,255,255,0.1); border: none; color: #fff; padding: 6px 12px; font-family: var(--mono); cursor: pointer; transition: 0.2s; }
.cyber-close-btn:hover { background: var(--red); }
.detail-wrapper { flex: 1; overflow-y: auto; background: var(--off-white); }

/* 动画 & 滚动条 */
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.4; } 100% { opacity: 1; } }
.fade-slide-enter-active, .fade-slide-leave-active { transition: 0.3s; }
.fade-slide-enter-from { opacity: 0; transform: translateY(20px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-10px); }
.modal-scale-enter-active, .modal-scale-leave-active { transition: 0.2s; }
.modal-scale-enter-from, .modal-scale-leave-to { opacity: 0; transform: scale(0.95); }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }

@media screen and (max-width: 1600px) { .left-column { flex: 0 0 240px; } .rank-column { display: none; } }
@media screen and (max-width: 1200px) { .right-column { display: none; } }
</style>