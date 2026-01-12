<template>
  <div class="comment-section">
    <div class="section-title">评论 ({{ total }})</div>
    
    <div class="comment-list custom-scroll">
      <div v-for="item in comments" :key="item.Id" class="comment-group">
        <div class="comment-item">
          <div class="avatar-box">
            <img :src="fixAvatarUrl(item.UserAvatar)" @error="handleImgError" alt="avatar" />
          </div>
          <div class="comment-content">
            <div class="user-name">
              {{ item.UserName }}
              <span class="time">{{ formatTime(item.CreateTime) }}</span>
            </div>
            <p class="text">{{ item.Content }}</p>
            <div class="op-bar">
              <span class="reply-btn" @click="handleSetReply(item)">回复</span>
            </div>
          </div>
        </div>

        <div v-if="item.Replies && item.Replies.length > 0" class="replies-container">
          <div v-for="sub in item.Replies" :key="sub.Id" class="comment-item sub-item">
            <div class="avatar-box mini">
              <img :src="fixAvatarUrl(sub.UserAvatar)" @error="handleImgError" alt="avatar" />
            </div>
            <div class="comment-content">
              <div class="user-name">
                {{ sub.UserName }} 
                <span class="reply-text">回复</span> 
                @{{ sub.ReplyToUserName || item.UserName }}
                <span class="time">{{ formatTime(sub.CreateTime) }}</span>
              </div>
              <p class="text">{{ sub.Content }}</p>
              <div class="op-bar">
                <span class="reply-btn" @click="handleSetReply(sub)">回复</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div v-if="comments.length === 0" class="empty-comment">
        还没有人评论，快来抢沙发~
      </div>
    </div>

    <div class="comment-input-area">
      <div v-if="replyTarget" class="reply-hint">
        回复 @{{ replyTarget.UserName }}
        <span class="cancel" @click="replyTarget = null">取消</span>
      </div>

      <div class="input-row">
        <input 
          v-model="inputContent" 
          type="text" 
          ref="commentInputRef"
          :placeholder="replyTarget ? '输入回复内容...' : '说点什么...'" 
          @keyup.enter="handleSubmit"
        />
        <button 
          class="send-btn" 
          :disabled="!inputContent.trim() || isSubmitting"
          @click="handleSubmit"
        >
          {{ isSubmitting ? '...' : '发送' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  artworkId: { type: Number, required: true }
})

// --- 配置常量 (参考你提供的代码) ---
const isDev = window.location.hostname === 'localhost';
const BASE_URL = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com';

// --- 响应式数据 ---
const comments = ref([])
const total = ref(0)
const inputContent = ref('')
const isSubmitting = ref(false)
const replyTarget = ref(null)
const commentInputRef = ref(null)

// --- 工具函数 (同步你提供的逻辑) ---

// 1. 头像处理：确保 localhost 路径正确或显示兜底图
const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';
  // 如果已经是完整路径则直接返回
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  return `${BASE_URL}/${path}`;
};

// 2. 图片错误兜底
const handleImgError = (e) => { 
  e.target.src = '/土豆.jpg'; 
};

// 3. 时间格式化：解决 NaN 问题
const formatTime = (t) => {
  if (!t) return 'N/A';
  const date = new Date(t);
  if (isNaN(date.getTime())) return '时间格式错误';
  
  // 简易格式化：MM-DD HH:mm
  const M = (date.getMonth() + 1).toString().padStart(2, '0');
  const D = date.getDate().toString().padStart(2, '0');
  const h = date.getHours().toString().padStart(2, '0');
  const m = date.getMinutes().toString().padStart(2, '0');
  return `${M}-${D} ${h}:${m}`;
};

// --- API 交互 ---

const loadComments = async () => {
  try {
    const res = await apiClient.get(`/Drawing/comment_list`, {
      params: { drawingId: props.artworkId }
    })
    if (res.data.success) {
      comments.value = res.data.data
      // 计算包含回复的总数
      let count = 0
      const countFn = (list) => {
        list.forEach(c => { 
          count++; 
          if(c.Replies) countFn(c.Replies); 
        });
      }
      countFn(res.data.data)
      total.value = count
    }
  } catch (err) {
    console.error('获取评论失败', err)
  }
}

const handleSubmit = async () => {
  if (!inputContent.value.trim() || isSubmitting.value) return
  
  isSubmitting.value = true
  try {
    const payload = {
      DrawingId: props.artworkId,
      Content: inputContent.value,
      // 注意：后端可能区分 ParentId 或 ParentCommentId，根据你的 DTO 选择
      ParentId: replyTarget.value ? replyTarget.value.Id : null
    }

    const res = await apiClient.post(`/Drawing/add_comment`, payload)
    
    if (res.data.success) {
      inputContent.value = ''
      replyTarget.value = null
      await loadComments() 
    }
  } catch (err) {
    console.error('发送评论错误', err)
  } finally {
    isSubmitting.value = false
  }
}

const handleSetReply = (item) => {
  replyTarget.value = item
  nextTick(() => {
    commentInputRef.value?.focus()
  })
}

onMounted(loadComments)
watch(() => props.artworkId, loadComments)
</script>

<style scoped>
.comment-section {
  display: flex;
  flex-direction: column;
  height: 100%; 
  border-top: 1px solid #f1f5f9;
  margin-top: 15px;
  padding-top: 15px;
}

.section-title { font-size: 14px; font-weight: bold; margin-bottom: 12px; color: #333; }

.comment-list { flex: 1; overflow-y: auto; padding-right: 5px; min-height: 0; }

.comment-group { margin-bottom: 15px; }

.comment-item { display: flex; gap: 10px; margin-bottom: 8px; }

.avatar-box img { 
  width: 32px; height: 32px; border-radius: 50%; 
  background: #eee; object-fit: cover; 
}

.mini img { width: 24px; height: 24px; }

.comment-content { flex: 1; }

.user-name { font-size: 12px; font-weight: bold; color: #555; margin-bottom: 2px; }
.time { font-weight: normal; color: #999; font-size: 10px; margin-left: 8px; }

.text { font-size: 13px; color: #333; margin: 4px 0; line-height: 1.4; word-break: break-all; }

.replies-container {
  margin-left: 42px;
  background-color: #f8fafc;
  border-radius: 6px;
  padding: 8px 10px;
}

.op-bar { margin-top: 2px; }
.reply-btn { font-size: 11px; color: #8b5cf6; cursor: pointer; }
.reply-btn:hover { text-decoration: underline; }

.reply-hint {
  font-size: 12px; color: #8b5cf6; background: #f5f3ff;
  padding: 6px 12px; border-radius: 20px; margin-bottom: 8px;
  display: flex; justify-content: space-between; align-items: center;
}
.cancel { cursor: pointer; font-size: 14px; font-weight: bold; opacity: 0.7; }

.comment-input-area { padding: 12px 0; border-top: 1px solid #f1f5f9; }
.input-row { display: flex; gap: 10px; }
.input-row input { 
  flex: 1; border: 1px solid #e2e8f0; border-radius: 20px; 
  padding: 8px 15px; font-size: 13px; outline: none; background: #f8fafc; 
}
.send-btn { 
  background: #8b5cf6; color: #fff; border: none; border-radius: 20px; 
  padding: 0 18px; font-size: 12px; cursor: pointer; font-weight: bold;
}
.send-btn:disabled { background: #cbd5e1; cursor: not-allowed; }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 4px; }
</style>