<template>
  <div class="post-entry" @click="$emit('click')">
    <div class="entry-scanline"></div>
    
    <div class="entry-main">
      <div class="entry-user-row">
        <div class="avatar-box">
          <img :src="avatarUrl" @error="handleImgError" />
        </div>
        <div class="user-info">
          <span class="username">{{ post.author?.username || 'UNKNOWN_USER' }}</span>
          <span class="post-time">{{ formatTime(post.createTime) }}</span>
        </div>
      </div>

      <h3 class="entry-title">{{ post.post_title }}</h3>

      <div class="entry-actions">
        <span class="action-item">
          <i class="far fa-comment-alt"></i> REPLIES: {{ post.comment_count }}
        </span>
        <span class="action-item">
          <i class="far fa-eye"></i> VISITS: {{ post.view_count }}
        </span>
      </div>
    </div>

    <div v-if="post.images?.length" class="entry-thumb-wrapper">
      <img :src="post.images[0]" @error="handleImgError" class="entry-thumb" />
      <span v-if="post.images.length > 1" class="img-count-badge">+{{ post.images.length }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  post: { type: Object, required: true },
  baseUrl: { type: String, default: '' }
});

defineEmits(['click']);

const avatarUrl = computed(() => {
  const url = props.post.author?.avatar;
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  return `${props.baseUrl}/${url.replace(/\\/g, '/')}`;
});

const formatTime = (t) => t ? new Date(t).toLocaleDateString() : 'N/A';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };
</script>

<style scoped>
.post-entry {
  background: #fff; border: 1px solid #111; margin-bottom: 12px;
  padding: 20px; position: relative; display: flex; gap: 15px; cursor: pointer;
  transition: transform 0.2s; overflow: hidden;
}
.post-entry:hover { transform: translateX(5px); box-shadow: -4px 4px 0 rgba(0,0,0,0.1); }

.entry-scanline {
  position: absolute; left: 0; top: 0; width: 4px; height: 100%;
  background: #D92323; transform: scaleY(0); transition: transform 0.2s;
}
.post-entry:hover .entry-scanline { transform: scaleY(1); }

.avatar-box { width: 24px; height: 24px; border: 1px solid #111; }
.avatar-box img { width: 100%; height: 100%; object-fit: cover; }
.username { font-family: 'JetBrains Mono'; font-weight: 700; font-size: 0.8rem; }
.post-time { font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #666; }
.entry-title { font-weight: 800; font-size: 1.1rem; margin: 5px 0 10px 0; }
.entry-actions { display: flex; gap: 15px; font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #555; }

.entry-thumb-wrapper { width: 90px; height: 90px; border: 1px solid #111; position: relative; flex-shrink: 0; }
.entry-thumb { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); }
.post-entry:hover .entry-thumb { filter: grayscale(0); }
.img-count-badge { position: absolute; bottom: 0; right: 0; background: #D92323; color: #fff; padding: 2px 4px; font-size: 0.7rem; }
</style>