<template>
  <div class="update-log-list-container">
    <h2 class="update-log-title">更新日志</h2>
    <div v-if="loading && updateTree.length === 0" class="log-loading">加载中…</div>
    <div v-if="error" class="log-error">{{ error }}</div>
    <div class="update-tree">
      <LogNode v-for="node in updateTree" :key="node.version" :node="node" />
    </div>
    <div v-if="loading && updateTree.length > 0" class="log-loading">加载更多…</div>
    <div v-else-if="!hasMore && updateTree.length > 0" class="log-end">没有更多内容了</div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import apiClient from '@/utils/api'
import LogNode from './LogNode.vue'

const updateTree = ref([])
const loading = ref(false)
const error = ref('')
const page = ref(1)
const hasMore = ref(true)
const pageSize = 20

async function loadUpdates() {
  if (!hasMore.value || loading.value) return
  loading.value = true
  error.value = ''
  try {
    const res = await apiClient.get('/Update/all', { params: { page: page.value, pageSize } })
    const arr = res.data?.data || []
    if (arr.length < pageSize) hasMore.value = false
    updateTree.value = [...updateTree.value, ...arr]
    page.value += 1
  } catch (e) {
    error.value = '加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

function handleScroll() {
  if (!hasMore.value || loading.value) return
  if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 120) {
    loadUpdates()
  }
}

onMounted(() => {
  loadUpdates()
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
.update-log-list-container {
  max-width: 100%;
  margin: 38px auto;
  padding: 0 8vw 38px 8vw;
  background: linear-gradient(120deg,#f8fcfe 70%, #e8f3fa 100%);
  border-radius: 16px;
  box-shadow: 0 8px 48px 0 rgba(40,130,220,0.13);
}

.update-log-title {
  text-align: center;
  font-size: 2rem;
  color: #256cc1;
  letter-spacing: 1.2px;
  font-weight: 650;
  margin-bottom: 34px;
  position: relative;
}
.update-log-title:after {
  content: '';
  display: block;
  width: 70px;
  height: 3px;
  background: linear-gradient(90deg,#2376ee,#5bc5fa);
  border-radius: 3px;
  margin: 13px auto 0;
  opacity: .6;
}

.log-loading {
  text-align: center;
  color: #2376ee;
  font-weight: 550;
  padding: 14px 0 8px 0;
  font-size: 1rem;
}
.log-error {
  color: #f56c6c;
  text-align: center;
  margin-top: 20px;
}
.log-end {
  color: #aaa;
  text-align: center;
  margin-top: 10px;
  font-size: .98rem;
}
.update-tree {
  margin-bottom: 8px;
}
</style>