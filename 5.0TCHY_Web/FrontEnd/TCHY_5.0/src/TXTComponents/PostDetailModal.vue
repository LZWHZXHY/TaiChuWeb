<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="post-detail-modal" @click.stop>
      <button class="close-modal" @click="$emit('close')"><i class="fas fa-times"></i></button>

      <div v-if="loading" class="modal-loading">加载中...</div>

      <div v-else-if="post" class="post-detail-content">
        <h2>{{ post.post_title }}</h2>
        <div class="post-author-info">
          <span class="author-avatar">{{ getAuthorInitial(post.author) }}</span>
          <div>
            <span class="author-name">{{ post.author.username }}</span>
            <span class="post-time">{{ formatTime(post.createTime) }}</span>
          </div>
        </div>

        <div v-if="post.images && post.images.length" class="post-detail-images">
          <img v-for="(img, idx) in post.images" :key="idx" :src="getImageUrl(img)" class="detail-image" />
        </div>

        <div class="post-body"><p>{{ post.content }}</p></div>

        <div class="post-stats">
          <span><i class="far fa-eye"></i> {{ post.view_count || 0 }}</span>
          <span><i class="far fa-comment"></i> {{ post.comment_count || 0 }}</span>
        </div>

        <reply-list :post-id="postId" @replies-loaded="onRepliesLoaded" />

        <reply-form :post-id="postId" @posted="onReplyPosted" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'
import ReplyList from './ReplyList.vue'
import ReplyForm from './ReplyForm.vue'

const props = defineProps({
  postId: { type: [String, Number], required: true },
  visible: { type: Boolean, default: false }
})
const emit = defineEmits(['close','reply-posted'])

const post = ref(null)
const loading = ref(false)

const fetchPost = async (id) => {
  loading.value = true
  try {
    const resp = await apiClient.get(`/posts/${id}`)
    if (resp.data && resp.data.success) post.value = resp.data.data
  } catch (e) {
    console.error('fetch post error', e)
  } finally {
    loading.value = false
  }
}

watch(() => props.postId, (newId) => {
  if (newId) fetchPost(newId)
})

onMounted(() => {
  if (props.postId) fetchPost(props.postId)
})

const getAuthorInitial = (author) => (author && author.username) ? author.username.charAt(0).toUpperCase() : '?'
const getImageUrl = (img) => {
  if (!img) return '/土豆.jpg'
  if (typeof img === 'string' && img.startsWith('http')) return img
  if (typeof img === 'object' && img.url) return img.url
  const base = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/api.*$/,'') || (import.meta.env.DEV ? 'http://localhost:44359' : 'https://bianyuzhou.com')
  return `${base}/${String(img).replace(/^\/+/,'')}`
}
const formatTime = (t) => { if (!t) return ''; const d=new Date(t); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}` }

const onReplyPosted = () => {
  // bubble up so parent can refresh lists if needed
  emit('reply-posted')
  // also refresh replies list by emitting an event or using ref
  // we'll re-emit to parent; ReplyList watches postId and will reload when postId unchanged; so we can use an internal event bus if necessary; for simplicity, force re-render by toggling postId (not ideal) — here we emit event that ReplyList listens to via provide/inject or use a simple pub; for brevity, we emit 'reply-posted' to parent
}

const onRepliesLoaded = (total) => {
  // update local post comment_count if needed
  if (post.value) post.value.comment_count = total
}
</script>

<style scoped>
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.5); display:flex; align-items:center; justify-content:center; z-index:1000; padding:20px }
.post-detail-modal { background:#fff; width:90%; max-width:700px; max-height:80vh; overflow:auto; border-radius:8px; padding:18px; position:relative }
.close-modal { position:absolute; right:12px; top:12px; background:none; border:none; font-size:18px }
.post-detail-images img { width:100%; margin-bottom:8px; object-fit:contain; border-radius:6px }
</style>