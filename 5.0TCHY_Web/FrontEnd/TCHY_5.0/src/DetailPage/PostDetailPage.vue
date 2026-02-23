<template>
  <div class="cyber-post-page">
    <div class="grid-bg moving-grid"></div>

    <header class="page-header">
      <div class="nav-back" @click="handleBack">
        <span class="arrow"> < </span> RETURN_TO_HUB
      </div>
      <div class="header-title">DATA_NODE_VIEWER // ID: {{ route.params.id }}</div>
    </header>

    <div class="page-main-bridge">
      <main class="post-primary">
        <div v-if="loading" class="loading-state">
          <div class="glitch-text" data-text="DOWNLOADING_STREAM...">DOWNLOADING_STREAM...</div>
        </div>
        
        <div v-else-if="postData" class="content-scroll-box custom-scroll">
          <PostDetailContent 
            :key="postData.id"
            :data="postData" 
            :fixAvatar="fixAvatarUrl" 
          />
          </div>
      </main>

      <aside class="post-sidebar custom-scroll">
        <section class="author-identity-card" v-if="postData">
          <div class="id-header">
            <span class="id-label">ORIGIN_AUTHOR</span>
            <span class="id-serial">#{{ postData.author_id?.toString().padStart(4, '0') }}</span>
          </div>
          
          <div class="id-main">
            <div class="avatar-container">
              <UserAvatar :user-id="postData.author_id" />
            </div>

            <div class="name-zone">
              <div class="username" @click="router.push(`/profile/${postData.author_id}`)">
                @{{ postData.author?.username }}
              </div>
              <div class="user-status"><span class="dot"></span> NODE_ACTIVE</div>
            </div>
          </div>

          <div class="id-stats">
            <div class="stat-item"><span class="l">LV.</span><span class="v">DATA</span></div>
            <div class="stat-item"><span class="l">RANK</span><span class="v">PRO</span></div>
          </div>

          <div class="id-actions">
            <button class="id-btn follow">FOLLOW_NODE</button>
            <button class="id-btn share" @click="handleShare">
               {{ shareStatus === 'READY' ? 'SHARE_DATA' : 'COPIED!' }}
            </button>
          </div>
        </section>

        <section class="cyber-card linked-posts">
          <div class="card-label-strip"><span>// AUTHOR_OTHER_NODES</span></div>
          <div class="linked-list">
            <template v-if="relatedPosts.length > 0">
              <div 
                v-for="item in relatedPosts" 
                :key="item.id" 
                class="mini-post-link"
                @click="router.push(`/post/${item.id}`)" 
              >
                <div class="link-tag">[ REL_DATA // {{ item.view_count }} ]</div>
                <div class="link-title">{{ item.post_title }}</div>
              </div>
            </template>
            <div v-else class="null-signal-mini">[ NO_OTHER_DATA ]</div>
          </div>
        </section>

        <div class="cyber-terminal-mini">
          <div class="terminal-header">> METRICS.log</div>
          <div class="metric"><span class="lab">VIEWS:</span> {{ postData?.view_count || 0 }}</div>
          <div class="metric"><span class="lab">REPLIES:</span> {{ postData?.comment_count || 0 }}</div>
          <div class="metric"><span class="lab">STATUS:</span> <span class="green">STABLE</span></div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/utils/api';
import PostDetailContent from '@/comminicateCenter/PostDetailContent.vue';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'; // 引入组件

const route = useRoute();
const router = useRouter();

const postData = ref(null);
const relatedPosts = ref([]);
const loading = ref(true);
const shareStatus = ref('READY');

// 这个 BASE_URL 如果 PostDetailContent 里还在用，就保留；否则可以删除
const BASE_URL = window.location.hostname === 'localhost' ? 'https://localhost:44359' : 'https://bianyuzhou.com';

// 同上，如果 PostDetailContent 需要，则保留
const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

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
  } catch (e) { console.error(e); } finally { loading.value = false; }
};

watch(() => route.params.id, (newId) => { fetchFullData(newId); });
onMounted(() => { fetchFullData(route.params.id); });
</script>

<style scoped>
.cyber-post-page {
  --red: #D92323; --black: #111111; --off-white: #F4F1EA;
  width: 100%; height: 100vh; background: var(--off-white);
  display: flex; flex-direction: column; overflow: hidden;
}

/* 布局结构 */
.page-main-bridge {
  flex: 1; display: flex; gap: 30px; padding: 30px;
  max-width: 1600px; margin: 0 auto; width: 100%; box-sizing: border-box; overflow: hidden;
}
.post-primary { flex: 1; min-width: 0; background: #fff; border: 2px solid var(--black); box-shadow: 10px 10px 0 rgba(0,0,0,0.1); overflow: hidden; }
.content-scroll-box { height: 100%; overflow-y: auto; }
.post-sidebar { width: 350px; display: flex; flex-direction: column; gap: 20px; overflow-y: auto; }

/* ✨ 重写的作者卡片：身份证风格 */
.author-identity-card {
  background: var(--black); color: #fff; padding: 20px;
  border: 1px solid #444; position: relative;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 20px), calc(100% - 20px) 100%, 0 100%);
}
.id-header { display: flex; justify-content: space-between; font-family: 'JetBrains Mono'; font-size: 0.7rem; border-bottom: 1px solid #333; padding-bottom: 10px; margin-bottom: 15px; }
.id-label { color: var(--red); font-weight: bold; }
.id-main { display: flex; gap: 15px; align-items: center; }

/* ✨ 新增：头像容器样式 */
.avatar-container {
  width: 70px; 
  height: 70px;
  /* UserAvatar 组件会自动填充这个容器 */
}

/* 移除不再需要的 .avatar-frame, .author-img, .scan-line 样式 */

.username { 
  font-size: 1.1rem; 
  font-weight: 900; 
  margin-bottom: 4px; 
  color: var(--off-white); 
  cursor: pointer; /* 增加手型 */
}
.username:hover { color: var(--red); }

.user-status { font-family: 'JetBrains Mono'; font-size: 0.6rem; color: #0f0; display: flex; align-items: center; gap: 5px; }
.user-status .dot { width: 6px; height: 6px; background: #0f0; border-radius: 50%; animation: pulse 1.5s infinite; }

.id-stats { display: flex; gap: 20px; margin-top: 15px; background: #222; padding: 8px 12px; }
.stat-item { font-family: 'JetBrains Mono'; font-size: 0.7rem; }
.stat-item .l { color: #666; margin-right: 5px; }
.stat-item .v { color: var(--red); font-weight: bold; }

.id-actions { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-top: 15px; }
.id-btn { 
  border: 1px solid #444; background: transparent; color: #fff; padding: 8px; 
  font-family: 'JetBrains Mono'; font-size: 0.7rem; cursor: pointer; transition: 0.2s;
}
.id-btn.follow { background: var(--red); border-color: var(--red); font-weight: bold; }
.id-btn:hover { background: #fff; color: #000; }

/* 其它卡片通用样式 */
.cyber-card { background: #fff; border: 2px solid var(--black); }
.card-label-strip { background: var(--black); color: #fff; padding: 8px 12px; font-family: 'JetBrains Mono'; font-size: 0.7rem; }
.mini-post-link { padding: 12px; border-bottom: 1px solid #eee; cursor: pointer; transition: 0.2s; }
.mini-post-link:hover { background: #fff9f9; transform: translateX(5px); }
.link-tag { font-family: 'JetBrains Mono'; font-size: 0.6rem; color: var(--red); }
.link-title { font-weight: bold; font-size: 0.85rem; margin-top: 4px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.cyber-terminal-mini { background: var(--black); color: #0f0; padding: 15px; font-family: 'JetBrains Mono'; font-size: 0.8rem; border-left: 4px solid var(--red); }
.terminal-header { border-bottom: 1px dashed #333; margin-bottom: 8px; color: #fff; }
.metric .lab { color: #666; }
.metric .green { color: #0f0; }

/* 动画与修饰 */
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.3; } 100% { opacity: 1; } }
.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(0,0,0,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(0,0,0,0.05) 1px, transparent 1px); background-size: 40px 40px; pointer-events: none; }
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

.page-header { height: 60px; background: var(--black); color: #fff; display: flex; align-items: center; padding: 0 40px; gap: 30px; border-bottom: 4px solid var(--red); }
.nav-back { cursor: pointer; font-family: 'JetBrains Mono'; font-weight: bold; }
.nav-back:hover { color: var(--red); }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>