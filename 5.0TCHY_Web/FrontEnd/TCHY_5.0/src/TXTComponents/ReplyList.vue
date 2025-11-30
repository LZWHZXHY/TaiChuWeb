<template>
  <section class="replies-section">
    <h3>回复（{{ total }}）</h3>
    <div v-if="loading" class="loading-state">加载回复...</div>
    <div v-else>
      <div v-if="replies.length === 0" class="empty-state">还没有回复</div>
      <div v-else class="replies-list">
        <div v-for="r in replies" :key="r.Id" class="reply-item">
          <div class="reply-header">
            <span class="reply-author-avatar">{{ getAuthorInitial(r) }}</span>
            <div class="reply-meta">
              <div class="reply-author">用户#{{ r.AuthorId }}</div>
              <div class="reply-time">{{ formatTime(r.CreateTime) }}</div>
            </div>
          </div>
          <div class="reply-content">{{ r.Content }}</div>
          <div v-if="r.images && r.images.length" class="reply-images">
            <img v-for="(img, i) in r.images" :key="i" :src="img" class="reply-image" />
          </div>
        </div>
      </div>
      <div v-if="hasMore" class="load-more-replies">
        <button class="load-more" @click="loadMore">加载更多回复</button>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  postId: { type: [String, Number], required: true }
})
const emit = defineEmits(['replies-loaded'])

const replies = ref([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const hasMore = ref(false)
const total = ref(0)

const load = async (p=1) => {
  if (!props.postId) return
  loading.value = true
  try {
    const resp = await apiClient.get(`/posts/${props.postId}/replies`, { params: { page: p, pageSize: pageSize.value } })
    if (resp.data && resp.data.success) {
      if (p===1) replies.value = resp.data.data
      else replies.value.push(...resp.data.data)
      total.value = resp.data.pagination?.total || replies.value.length
      hasMore.value = (p * pageSize.value) < (resp.data.pagination?.total || 0)
      emit('replies-loaded', total.value)
    }
  } catch (e) {
    console.error('load replies error', e)
  } finally {
    loading.value = false
  }
}

watch(() => props.postId, (id) => {
  page.value = 1
  load(1)
})

onMounted(() => {
  if (props.postId) load(1)
})

const loadMore = () => {
  if (hasMore.value && !loading.value) {
    page.value += 1
    load(page.value)
  }
}

const getAuthorInitial = (r) => String(r.AuthorId).charAt(0).toUpperCase()
const formatTime = (t) => { if (!t) return ''; const d=new Date(t); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}` }

</script>

<style scoped>
.replies-section { margin-top:18px; border-top:1px dashed #eee; padding-top:12px }
.reply-item { padding:10px 0; border-bottom:1px solid #f5f5f5 }
.reply-header { display:flex; gap:8px; align-items:center; margin-bottom:8px }
.reply-author-avatar { width:36px; height:36px; border-radius:50%; background:linear-gradient(135deg,#667eea,#764ba2); color:#fff; display:inline-flex; align-items:center; justify-content:center }
.reply-content { color:#333 }
.reply-images { display:flex; gap:8px; margin-top:8px; flex-wrap:wrap }
.reply-image { width:120px; height:80px; object-fit:cover; border-radius:6px; border:1px solid #e0e0e0 }
.load-more-replies { text-align:center; margin-top:8px }
</style>