<template>
  <section class="replies-root">
    <div v-if="!postId" class="no-post">请选择帖子以查看回复</div>

    <div v-else>
      <div v-if="loading" class="loading">加载中…</div>
      <div v-else>
        <div class="replies-header">
          <h3 class="replies-title">回复列表</h3>
          <button class="refresh-btn" @click="refresh" :disabled="loading">
            {{ loading ? '刷新中...' : '🔄 刷新' }}
          </button>
        </div>

        <div v-if="roots.length === 0" class="empty">还没有回复</div>

        <div v-else class="tree">
          <ReplyNode
            v-for="node in roots"
            :key="node.id"
            :comment="node"
            :depth="0"
            :post-id="postId"
            @reply-open="forwardReplyOpen"
          />
        </div>

        <div v-if="hasMore" class="more">
          <button class="more-btn" @click="loadMore" :disabled="loading">
            {{ loading ? '加载中...' : '加载更多' }}
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import apiClient from '@/utils/api'
import ReplyNode from './ReplyNode.vue'

const props = defineProps({
  postId: { type: [String, Number], default: null },
  pageSize: { type: Number, default: 100 }
})
const emit = defineEmits(['reply-open','replies-loaded'])

const raw = ref([])
const loading = ref(false)
const page = ref(1)
const hasMore = ref(false)

// 加载回复列表
const load = async (p = 1) => {
  if (!props.postId) return
  loading.value = true
  try {
    const r = await apiClient.get(`/posts/${props.postId}/replies`, { 
      params: { page: p, pageSize: props.pageSize } 
    })
    if (r.data && r.data.success) {
      if (p === 1) {
        raw.value = r.data.data || []
      } else {
        raw.value.push(...(r.data.data || []))
      }
      hasMore.value = !!r.data.pagination?.hasMore
      emit('replies-loaded', r.data.pagination?.total || raw.value.length)
    }
  } catch (e) {
    console.error('load replies error', e)
    // 可以添加错误处理，比如显示错误信息
  } finally {
    loading.value = false
  }
}

// 刷新回复列表
const refresh = async () => {
  page.value = 1
  await load(1)
}

// 加载更多
const loadMore = async () => { 
  if (!props.postId || !hasMore.value || loading.value) return
  page.value += 1
  await load(page.value)
}

// 监听帖子ID变化
watch(() => props.postId, (newId) => {
  if (newId) {
    page.value = 1
    raw.value = []
    load(1)
  } else {
    raw.value = []
  }
})

// 构建树形结构
const tree = computed(() => {
  const map = new Map()
  
  // 克隆节点避免直接修改原始数据
  for (const c of raw.value || []) {
    map.set(c.id, { ...c, children: [] })
  }
  
  const roots = []
  
  // 构建树结构
  for (const node of map.values()) {
    const pid = node.parentCommentId ?? null
    if (pid == null || !map.has(pid)) {
      roots.push(node)
    } else {
      map.get(pid).children.push(node)
    }
  }
  
  // 排序：根回复按时间倒序，子回复按时间正序
  const sortRoots = (a, b) => new Date(b.createTime) - new Date(a.createTime)
  const sortChildren = (a, b) => new Date(a.createTime) - new Date(b.createTime)
  
  roots.sort(sortRoots)
  
  for (const n of map.values()) {
    if (n.children && n.children.length) {
      n.children.sort(sortChildren)
    }
  }
  
  return { roots, map }
})

const roots = computed(() => tree.value.roots || [])

// 转发回复事件
const forwardReplyOpen = (parentId, parentName) => {
  emit('reply-open', parentId, parentName)
}

// 暴露刷新方法给父组件
defineExpose({
  refresh
})

onMounted(() => { 
  if (props.postId) load(1) 
})
</script>

<style scoped>
.replies-root {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.no-post {
  color: #6b7280;
  padding: 20px;
  text-align: center;
  background: #f9fafb;
  border-radius: 8px;
}

.loading {
  color: #6b7280;
  padding: 20px;
  text-align: center;
}

.replies-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.replies-title {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 600;
  color: #1f2937;
}

.refresh-btn {
  padding: 6px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  background: white;
  color: #374151;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s ease;
}

.refresh-btn:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.empty {
  color: #6b7280;
  padding: 40px 20px;
  text-align: center;
  background: #f9fafb;
  border-radius: 8px;
}

.tree {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.more {
  text-align: center;
  margin-top: 20px;
}

.more-btn {
  padding: 10px 20px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: white;
  color: #374151;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s ease;
}

.more-btn:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.more-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>