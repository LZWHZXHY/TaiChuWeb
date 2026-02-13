<template>
  <div class="post-entry" @click="handlePostClick">
    <div class="entry-scanline"></div>
    
    <div class="entry-main">
      <div class="entry-user-row">
        <div class="avatar-box" @click.stop="navigateToProfile">
          <UserAvatar 
            v-if="validUserId"
            :user-id="validUserId" 
            :passed-avatar="post.author?.avatar"
            :passed-level="post.author?.Level || post.author?.level"
            :show-level="true" 
          />
          <div v-else class="fallback-placeholder">
            <span class="err-txt">NO_ID</span>
          </div>
        </div>
        
        <div class="user-info">
          <span class="username" @click.stop="navigateToProfile">
            {{ post.author?.username || post.Author?.Username || 'UNKNOWN' }}
          </span>
          <span class="post-time">{{ formatTime(post.createTime || post.CreateTime) }}</span>
        </div>
      </div>

      <h3 class="entry-title">{{ post.post_title || post.Post_title }}</h3>

      <div class="entry-tags" v-if="postTags && postTags.length > 0">
        <span 
          v-for="(tag, index) in postTags" 
          :key="index" 
          class="mini-tag"
          @click.stop="handleTagClick(tag)"
        >
          #{{ tag }}
        </span>
      </div>

      <div class="entry-actions">
        <span class="action-item">
          <i class="far fa-comment-alt"></i> REPLIES: {{ post.comment_count ?? 0 }}
        </span>
        <span class="action-item">
          <i class="far fa-eye"></i> VISITS: {{ post.view_count ?? 0 }}
        </span>
      </div>
    </div>

    <div v-if="hasImages" class="entry-thumb-wrapper">
      <img :src="cleanUrl(firstImage)" @error="handleImgError" class="entry-thumb" />
      <span v-if="imageCount > 1" class="img-count-badge">+{{ imageCount }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';

const props = defineProps({
  post: { type: Object, required: true },
  baseUrl: { type: String, default: '' }
});

const emit = defineEmits(['click']);
const router = useRouter();

const validUserId = computed(() => {
  const p = props.post;
  if (!p) return null;
  return p.author?.Id || p.author?.id || p.Author?.Id || p.author_id || null;
});

// ✅ 新增：处理标签数据
const postTags = computed(() => {
  // 兼容后端返回的 tags (List<string>) 或 Tags (大小写不敏感)
  const t = props.post.tags || props.post.Tags;
  
  if (Array.isArray(t)) return t;
  if (typeof t === 'string' && t.length > 0) return t.split(',');
  return [];
});

const handlePostClick = () => emit('click');

const navigateToProfile = () => {
  if (validUserId.value) {
    router.push(`/profile/${validUserId.value}`);
  }
};

// ✅ 新增：点击标签跳转
const handleTagClick = (tagName) => {
  // 假设你的搜索页路由是 /search
  router.push({ path: '/search', query: { q: tagName, type: 'tag' } });
};

// 辅助计算属性
const hasImages = computed(() => {
  const imgs = props.post.images || props.post.Images;
  return imgs && imgs.length > 0;
});
const firstImage = computed(() => (props.post.images || props.post.Images)?.[0]);
const imageCount = computed(() => (props.post.images || props.post.Images)?.length || 0);

const cleanUrl = (url) => {
  if (!url) return '/土豆.jpg';
  return String(url).replace(/https?:\/\/localhost(:\d+)?/i, 'https://img.bianyuzhou.com');
};
const formatTime = (t) => t ? new Date(t).toLocaleDateString() : 'N/A';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };
</script>

<style scoped>
.post-entry { background: #fff; border: 1px solid #111; margin-bottom: 12px; padding: 20px; position: relative; display: flex; gap: 15px; cursor: pointer; transition: 0.2s; overflow: hidden; }
.post-entry:hover { transform: translateX(5px); box-shadow: -4px 4px 0 rgba(0,0,0,0.1); }
.entry-scanline { position: absolute; left: 0; top: 0; width: 4px; height: 100%; background: #D92323; transform: scaleY(0); transition: 0.2s; }
.post-entry:hover .entry-scanline { transform: scaleY(1); }
.entry-main { flex: 1; min-width: 0; display: flex; flex-direction: column; }
.entry-user-row { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; }

/* 头像框 */
.avatar-box { width: 36px; height: 36px; flex-shrink: 0; position: relative; z-index: 10; cursor: pointer; }
.fallback-placeholder { width: 100%; height: 100%; background: #333; display: flex; align-items: center; justify-content: center; }
.err-txt { color: #fff; font-size: 8px; font-family: monospace; }

.username { font-family: 'JetBrains Mono'; font-weight: 700; font-size: 0.85rem; cursor: pointer; position: relative; z-index: 10; }
.username:hover { color: #D92323; text-decoration: underline; }
.post-time { font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #666; }
.entry-title { font-weight: 800; font-size: 1.1rem; margin: 0 0 8px 0; color: #111; } /* margin改小点，给标签留位置 */

/* ✅ 新增：标签样式 */
.entry-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 12px;
}

.mini-tag {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  color: #555;
  background: #f0f0f0;
  padding: 2px 8px;
  border: 1px solid #ccc;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 600;
}

.mini-tag:hover {
  color: #fff;
  background: #D92323;
  border-color: #D92323;
  transform: translateY(-1px);
  box-shadow: 2px 2px 0 rgba(217, 35, 35, 0.2);
}

.entry-actions { display: flex; gap: 15px; font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #555; }
.entry-thumb-wrapper { width: 90px; height: 90px; border: 1px solid #111; position: relative; flex-shrink: 0; }
.entry-thumb { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); transition: 0.3s; }
.post-entry:hover .entry-thumb { filter: grayscale(0); }
.img-count-badge { position: absolute; bottom: 0; right: 0; background: #D92323; color: #fff; padding: 2px 4px; font-size: 0.7rem; }
</style>