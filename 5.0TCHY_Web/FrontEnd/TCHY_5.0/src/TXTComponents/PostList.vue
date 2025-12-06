<template>
  <div class="posts-list-root">
    <div class="controls">
      <div class="left">
        <span class="count">共 {{ posts?.length ?? 0 }} 帖</span>
      </div>
      <div class="right">
        <button class="btn-compact" @click="$emit('load-more')" v-if="hasMore">加载更多</button>
      </div>
    </div>

    <div v-if="!posts || posts.length === 0" class="empty-state">当前没有帖子</div>

    <div v-else class="posts-column">
      <post-item
        v-for="post in posts"
        :key="post.id ?? post.post_id ?? post._id"
        :post="post"
        @view="onView"
      />
    </div>

    <div v-if="loading" class="loading-state">加载中...</div>

    <div class="footer-actions" v-if="!loading && hasMore">
      <button class="btn-load" @click="$emit('load-more')">加载更多帖子</button>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'
import PostItem from './PostItem.vue'

const props = defineProps({
  posts: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  hasMore: { type: Boolean, default: false }
})
const emit = defineEmits(['view-post','load-more'])

const onView = (id) => { emit('view-post', id) }
</script>

<style scoped>
.posts-list-root { display:flex; flex-direction:column; gap:14px; max-width:980px; margin:0 auto; padding:8px 12px; }

.controls { display:flex; justify-content:space-between; align-items:center; padding:8px 4px; }
.count { font-weight:700; color:#0f172a }
.btn-compact { padding:8px 12px; border-radius:8px; border:1px solid #eef6fb; background:#fff; cursor:pointer }

.posts-column { display:flex; flex-direction:column; gap:18px; }

/* single-column posts take full width of container */
.posts-column > * { width:100%; }

.empty-state { text-align:center; padding:28px; color:#6b7280; background:#fbfdff; border:1px solid #eef6fb; border-radius:12px }
.loading-state { text-align:center; padding:18px; color:#6b7280 }

.footer-actions { text-align:center; padding:12px 0 }
.btn-load { padding:10px 16px; border-radius:8px; border:none; background:linear-gradient(90deg,#667eea,#764ba2); color:#fff; cursor:pointer }
</style>