<template>
  <div class="post-item" @click="viewPost">
    <div v-if="post.images && post.images.length" class="post-images-preview">
      <img v-for="(img, idx) in displayImages" :key="idx" :src="getImageUrl(img)" class="post-preview-image" />
      <div v-if="post.images.length > 3" class="more-images">+{{ post.images.length - 3 }}</div>
    </div>
    <h3 class="post-title">{{ post.post_title }}</h3>
    <p class="post-excerpt">{{ post.excerpt || truncate(post.content,100) }}</p>
    <div class="post-meta">
      <div class="post-author">
        <span class="author-avatar">{{ getAuthorInitial(post.author) }}</span>
        <span>{{ post.author.username }}</span>
      </div>
      <div class="post-tags"><span class="post-tag">{{ post.post_type }}</span></div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits, computed } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  post: { type: Object, required: true }
})
const emit = defineEmits(['view'])

const displayImages = computed(() => {
  const imgs = props.post.images || []
  return Array.isArray(imgs) ? imgs.slice(0,3) : []
})

const getImageUrl = (img) => {
  if (!img) return '/土豆.jpg'
  if (typeof img === 'string' && img.startsWith('http')) return img
  if (typeof img === 'object' && img.url) return img.url
  // fallback: treat as relative path
  const base = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/api.*$/,'') || (import.meta.env.DEV ? 'http://localhost:44359' : 'https://bianyuzhou.com')
  return `${base}/${String(img).replace(/^\/+/,'')}`
}

const getAuthorInitial = (author) => (author && author.username) ? author.username.charAt(0).toUpperCase() : '?'
const truncate = (s, n) => !s ? '' : (s.length>n ? s.slice(0,n)+'...' : s)

const viewPost = () => emit('view', props.post.id)
</script>

<style scoped>
.post-item { background:white; padding:16px; border-radius:8px; cursor:pointer; border:1px solid #f0f0f0; }
.post-preview-image { width:80px; height:80px; object-fit:cover; border-radius:6px; margin-right:8px; }
.post-images-preview { display:flex; gap:8px; margin-bottom:8px; align-items:center; }
.more-images { width:80px; height:80px; display:flex; align-items:center; justify-content:center; background:#f8f9fa; border-radius:6px; }
.post-title { font-weight:600; margin:8px 0; }
.post-excerpt { color:#666; }
.post-meta { display:flex; justify-content:space-between; align-items:center; margin-top:8px; font-size:0.85rem; color:#888; }
.author-avatar { width:28px;height:28px;border-radius:50%;background:linear-gradient(135deg,#667eea,#764ba2);display:inline-flex;align-items:center;justify-content:center;color:#fff; }
</style>