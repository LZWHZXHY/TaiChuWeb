<template>
  <div class="container">
    <header class="forum-header">
      <h1 class="forum-title">太虚坛</h1>
      <p class="forum-subtitle">太初寰宇 - 自由交流地</p>
    </header>

    <main>
      <div class="posts-header">
        <h2 class="posts-title"><i class="fas fa-stream"></i> 最新帖子</h2>
        <div class="filter-controls">
          <button 
            v-for="filter in filters" 
            :key="filter.id"
            :class="['filter-btn', { active: activeFilter === filter.id }]"
            @click="changeFilter(filter.id)"
          >
            {{ filter.name }}
          </button>
        </div>
      </div>

      <post-list
        :posts="posts"
        :loading="loading"
        :has-more="hasMore"
        @view-post="onViewPost"
        @load-more="loadMore"
      />
    </main>

    <aside class="sidebar">
      <post-form @posted="onPostPosted" />
    </aside>

    <post-detail-modal
      v-if="showPostDetail"
      :post-id="currentPostId"
      :visible="showPostDetail"
      @close="closePostDetail"
      @reply-posted="onReplyPosted"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'
import PostList from '@/TXTComponents/PostList.vue'
import PostForm from '@/TXTComponents/PostForm.vue'
import PostDetailModal from '@/TXTComponents/PostDetailModal.vue'

const posts = ref([])
const loading = ref(false)
const posting = ref(false)
const hasMore = ref(true)
const lastId = ref(null)
const activeFilter = ref('all')
const showPostDetail = ref(false)
const currentPostId = ref(null)

const filters = ref([
  { id: 'all', name: '全部' },
  { id: '0', name: '柴圈帖子' },
  { id: '1', name: '游戏帖子' },
  { id: '2', name: '其他帖子' }
])

const fetchPosts = async () => {
  if (loading.value) return
  loading.value = true
  try {
    const params = { pageSize: 10 }
    if (lastId.value) params.lastId = lastId.value
    if (activeFilter.value !== 'all') params.postType = parseInt(activeFilter.value)
    const resp = await apiClient.get('/posts', { params })
    const data = resp.data
    if (data && data.success) {
      if (params.lastId) posts.value.push(...data.data)
      else posts.value = data.data
      hasMore.value = data.pagination?.hasMore || false
      lastId.value = data.pagination?.lastId || null
    } else {
      console.error('加载帖子失败', data)
    }
  } catch (err) {
    console.error('fetchPosts error', err)
  } finally {
    loading.value = false
  }
}

const loadMore = () => {
  if (!hasMore.value || loading.value) return
  fetchPosts()
}

const changeFilter = async (filterId) => {
  activeFilter.value = filterId
  lastId.value = null
  posts.value = []
  await fetchPosts()
}

const onPostPosted = async () => {
  lastId.value = null
  await fetchPosts()
}

const onViewPost = (postId) => {
  currentPostId.value = postId
  showPostDetail.value = true
}

const closePostDetail = () => {
  showPostDetail.value = false
  currentPostId.value = null
}

const onReplyPosted = async () => {
  // optionally refresh the list or the post count
  await fetchPosts()
}

onMounted(() => {
  fetchPosts()
})
</script>

<style scoped>
/* 你原先 TaiXuTan.vue 的样式可放在这里（或保留在单独 css 文件） */
/* 仅保留布局相关，组件各自维护部分细化样式 */
.container { max-width:1200px; margin:0 auto; padding:20px; display:grid; grid-template-columns:1fr 350px; gap:25px; min-height:100vh; }
.forum-header { grid-column: 1 / -1; text-align:center; margin-bottom:30px; padding:25px; background:linear-gradient(135deg,#667eea 0%,#764ba2 100%); border-radius:12px; color:#fff; }
.posts-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:20px; padding-bottom:15px; border-bottom:1px solid #eaeaea; }
.filter-controls { display:flex; gap:8px; }
.filter-btn { padding:8px 16px; border-radius:20px; background:#fff; border:1px solid #e0e0e0; cursor:pointer; }
.filter-btn.active { background:#667eea; color:#fff; border-color:#667eea; }
.sidebar { display:flex; flex-direction:column; gap:20px; }
@media (max-width:900px) {
  .container { grid-template-columns: 1fr; }
}
</style>