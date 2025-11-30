<template>
  <div class="posts-list">
    <post-item
      v-for="post in posts"
      :key="post.id"
      :post="post"
      @view="onView"
    />
    <div v-if="loading" class="loading-state">加载中...</div>
    <button v-if="!loading && hasMore" class="load-more" @click="$emit('load-more')">加载更多</button>
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

const onView = (id) => {
  emit('view-post', id)
}
</script>

<style scoped>
.posts-list { display:flex; flex-direction:column; gap:16px; }
.loading-state { text-align:center; padding:20px; color:#666; }
.load-more { margin:20px auto; padding:10px 20px; border-radius:8px; }
</style>