<template>
  <div class="universal-comments">
    <div class="comments-header">
      <h3 class="title">评论区 // COMMENTS ({{ comments.length }})</h3>
      <div class="header-line"></div>
    </div>

    <div class="input-area" :class="{ 'disabled': !authStore.isAuthenticated }">
      <div class="avatar-box">
        <UserAvatar 
          userId="MEE" 
          :passedAvatar="authStore.user?.avatar"
          :showLevel="false" 
          :allowLink="false"
        />
      </div>
      
      <div class="input-wrapper">
        <div v-if="replyToUser" class="reply-status-bar">
          <span>正在回复: <b>@{{ replyToUser.Name }}</b></span>
          <button class="cancel-reply-btn" @click="cancelReply">取消回复 ×</button>
        </div>

        <textarea 
          ref="textareaRef"
          v-model="content" 
          :placeholder="inputPlaceholder"
          @focus="checkLogin"
          rows="3"
        ></textarea>
        
        <div class="action-bar">
          <span class="hint">理性发言，构建和谐赛博空间</span>
          <button class="send-btn" @click="submitComment" :disabled="isSubmitting || !content.trim()">
            <span class="btn-text">{{ isSubmitting ? '发送中...' : '发射 // SEND' }}</span>
          </button>
        </div>
      </div>
    </div>

    <div class="comment-list">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div> 正在加载神经信号...
      </div>
      
      <div v-else-if="comments.length === 0" class="empty-state">
        <span class="empty-icon">∅</span>
        <p>暂无数据信号，抢占首层扇区...</p>
      </div>

      <div v-else v-for="item in comments" :key="item.Id" class="comment-item">
        
        <div class="c-avatar">
  <UserAvatar 
    :userId="item.User?.Id" 
    :passedAvatar="item.User?.Avatar" 
    
    :passedLevel="item.User?.Level"  :showLevel="true"                :allowLink="true"
  />
</div>

        <div class="c-content">
          <div class="c-header">
            <span class="c-name">{{ item.User?.Name || '未知用户' }}</span>
            
            <span v-if="item.ReplyToUser" class="reply-indicator">
              <span class="arrow">▸</span> 
              <span class="at-name">@{{ item.ReplyToUser.Name }}</span>
            </span>

            <span class="c-date">{{ formatDate(item.CreatedAt) }}</span>
          </div>
          
          <div class="c-text">{{ item.Content }}</div>
          
          <div class="c-actions">
            <span class="action-link reply-btn" @click="handleReply(item)">
              <span class="icon">↩</span> 回复
            </span>
            <span class="action-link delete-btn" v-if="isMyComment(item.User?.Id)" @click="handleDelete(item.Id)">
              <span class="icon">×</span> 删除
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { useAuthStore } from '@/utils/auth'
import apiClient from '@/utils/api'

// ✅ 引入 UserAvatar 组件
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'

const props = defineProps({
  targetType: { type: String, required: true },
  targetId: { type: [String, Number], required: true }
})

const authStore = useAuthStore()
const textareaRef = ref(null)
const content = ref('')
const comments = ref([])
const loading = ref(false)
const isSubmitting = ref(false)

// 回复状态
const replyToUser = ref(null) 
const replyToId = ref(null)   

const inputPlaceholder = computed(() => {
  if (!authStore.isAuthenticated) return '请先登录后发表评论...'
  return '输入你的想法...'
})

// --- 方法 ---

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`
}

const checkLogin = () => {
  if (!authStore.isAuthenticated) alert('请先登录')
}

const isMyComment = (userId) => {
  return authStore.isAuthenticated && authStore.user?.id === userId
}

// 获取评论
const fetchComments = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/comment/list', {
      params: { type: props.targetType, id: props.targetId }
    })
    if (res.data.success) {
      comments.value = res.data.data
    }
  } finally {
    loading.value = false
  }
}

// 触发回复
const handleReply = (item) => {
  if (!authStore.isAuthenticated) return alert('请先登录')
  
  replyToId.value = item.Id
  replyToUser.value = item.User 
  
  nextTick(() => {
    textareaRef.value?.focus()
    textareaRef.value?.scrollIntoView({ behavior: 'smooth', block: 'center' })
  })
}

// 取消回复
const cancelReply = () => {
  replyToId.value = null
  replyToUser.value = null
}

// 提交
const submitComment = async () => {
  if (!authStore.isAuthenticated) return alert('请先登录')
  if (!content.value.trim()) return

  isSubmitting.value = true
  try {
    const payload = {
      targetType: props.targetType,
      targetId: props.targetId.toString(),
      content: content.value,
      parentId: replyToId.value 
    }
    
    const res = await apiClient.post('/comment/add', payload)
    if (res.data.success) {
      content.value = ''
      cancelReply()
      await fetchComments() 
    }
  } catch (err) {
    alert('发送失败')
  } finally {
    isSubmitting.value = false
  }
}

const handleDelete = async (id) => {
  if (!confirm('确定删除这条评论吗？')) return
  try {
    await apiClient.delete(`/comment/delete/${id}`)
    await fetchComments()
  } catch(e) { console.error(e) }
}

onMounted(() => {
  fetchComments()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500&family=Noto+Sans+SC:wght@400;700&display=swap');

.universal-comments {
  margin-top: 40px; padding: 24px;
  background: #fff; border-radius: 16px;
  border: 1px solid rgba(0,0,0,0.05);
  box-shadow: 0 4px 20px rgba(0,0,0,0.02);
  font-family: 'Noto Sans SC', sans-serif;
}

.comments-header { margin-bottom: 24px; }
.title { font-size: 14px; font-weight: 900; letter-spacing: 1px; margin: 0 0 8px 0; font-family: 'Roboto Mono'; color: #333; }
.header-line { width: 40px; height: 4px; background: #000; }

/* 输入区 */
.input-area { display: flex; gap: 16px; margin-bottom: 40px; }

/* ✅ 调整头像容器样式，让 UserAvatar 充满 */
.avatar-box { 
  width: 48px; 
  height: 48px; 
  flex-shrink: 0; 
  /* 移除原本的 border-radius 和 border，因为 UserAvatar 自带样式 */
}

.input-wrapper { flex: 1; display: flex; flex-direction: column; gap: 12px; }

.reply-status-bar {
  background: #f0f9ff; border: 1px solid #bae7ff; color: #0050b3;
  padding: 8px 12px; border-radius: 8px; font-size: 12px;
  display: flex; justify-content: space-between; align-items: center;
}
.cancel-reply-btn { background: transparent; border: none; color: #0050b3; cursor: pointer; font-weight: bold; }

textarea {
  width: 100%; padding: 12px; border: 2px solid #eee; border-radius: 12px;
  background: #f9f9f9; resize: vertical; outline: none; transition: all 0.2s;
  font-family: inherit; font-size: 14px; box-sizing: border-box;
}
textarea:focus { border-color: #000; background: #fff; }

.action-bar { display: flex; justify-content: space-between; align-items: center; }
.hint { font-size: 12px; color: #999; }
.send-btn {
  background: #000; color: #fff; border: none; padding: 8px 24px;
  border-radius: 20px; cursor: pointer; font-weight: bold; font-size: 12px;
  transition: transform 0.1s;
}
.send-btn:hover:not(:disabled) { transform: scale(1.05); }
.send-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* 列表区 */
.loading-state, .empty-state { text-align: center; padding: 40px; color: #999; font-size: 12px; }
.empty-icon { font-size: 32px; display: block; margin-bottom: 10px; }
.spinner { width: 20px; height: 20px; border: 2px solid #eee; border-top-color: #000; border-radius: 50%; animation: spin 1s linear infinite; display: inline-block; vertical-align: middle; margin-right: 8px; }
@keyframes spin { 100% { transform: rotate(360deg); } }

.comment-item { display: flex; gap: 16px; margin-bottom: 24px; border-bottom: 1px dashed #eee; padding-bottom: 24px; }
.comment-item:last-child { border-bottom: none; margin-bottom: 0; }

/* ✅ 调整列表头像容器 */
.c-avatar { 
  width: 40px; 
  height: 40px; 
  flex-shrink: 0;
}

.c-content { flex: 1; }
.c-header { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; }
.c-name { font-weight: 800; font-size: 14px; color: #000; }
.c-date { font-size: 11px; color: #bbb; font-family: 'Roboto Mono'; margin-left: auto; }

/* 回复指示器样式 */
.reply-indicator { background: #f5f5f5; padding: 2px 6px; border-radius: 4px; font-size: 12px; color: #666; display: flex; align-items: center; }
.reply-indicator .arrow { margin-right: 4px; color: #999; font-size: 10px; }
.reply-indicator .at-name { color: #d35400; font-weight: 700; }

.c-text { font-size: 14px; color: #444; line-height: 1.6; margin-bottom: 8px; white-space: pre-wrap; }

.c-actions { font-size: 12px; display: flex; gap: 16px; }
.action-link { cursor: pointer; font-weight: bold; color: #888; display: flex; align-items: center; gap: 4px; transition: color 0.2s; }
.action-link .icon { font-size: 14px; }
.reply-btn:hover { color: #000; }
.delete-btn:hover { color: #ff4d4f; }
</style>