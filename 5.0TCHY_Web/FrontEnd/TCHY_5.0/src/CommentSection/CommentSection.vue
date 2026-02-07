<template>
  <div class="comment-section">
    <div class="section-title">评论 ({{ total }})</div>
    
    <div class="comment-list custom-scroll">
      <div v-for="item in comments" :key="item.Id" class="comment-group">
        
        <div class="comment-item root-item">
          <div class="avatar-box">
            <img :src="fixAvatarUrl(item.UserAvatar)" @error="handleImgError" alt="avatar" />
          </div>
          <div class="comment-content">
            <div class="user-info">
              <span class="user-name main-name">{{ item.UserName }}</span>
              <span class="time">{{ formatTime(item.CreateTime) }}</span>
            </div>
            <p class="text root-text">{{ item.Content }}</p>
            <div class="op-bar">
              <span class="reply-btn" @click="handleSetReply(item)">回复</span>
            </div>
          </div>
        </div>

        <div v-if="item.FlatReplies && item.FlatReplies.length > 0" class="replies-wrapper">
          <div v-for="sub in item.FlatReplies" :key="sub.Id" class="sub-comment-item">
            
            <div class="sub-avatar">
               <img :src="fixAvatarUrl(sub.UserAvatar)" @error="handleImgError" alt="avatar" />
            </div>

            <div class="sub-content-box">
              <div class="sub-header">
                <span class="sub-user">{{ sub.UserName }}</span>

                <span v-if="sub.ReplyToUserName && sub.ReplyToUserName !== item.UserName" class="reply-relation">
                  <span class="arrow">▶</span> 
                  <span class="target-user">{{ sub.ReplyToUserName }}</span>
                </span>
                
                <span class="sub-time">{{ formatTime(sub.CreateTime) }}</span>
              </div>

              <div class="sub-text">
                {{ sub.Content }}
              </div>

              <div class="sub-op">
                <span class="reply-btn-mini" @click="handleSetReply(sub)">回复</span>
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
        <span>回复: <strong>{{ replyTarget.UserName }}</strong></span>
        <span class="cancel-btn" @click="replyTarget = null">×</span>
      </div>

      <div class="input-row">
        <input 
          v-model="inputContent" 
          type="text" 
          ref="commentInputRef"
          :placeholder="replyTarget ? `回复 @${replyTarget.UserName}...` : '说点什么...'" 
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

// 🔥 修改 1: 属性名改为 drawingId，与父组件传参保持一致
const props = defineProps({
  drawingId: { type: Number, required: true }
})

// --- 常量 ---
const BASE_URL = 'https://bianyuzhou.com'; 

// --- 状态 ---
const comments = ref([])
const total = ref(0)
const inputContent = ref('')
const isSubmitting = ref(false)
const replyTarget = ref(null)
const commentInputRef = ref(null)

const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';

  // 清洗掉可能的空格或反斜杠
  let path = url.trim().replace(/\\/g, '/');

  // 🔥 核心逻辑：如果地址已经包含了 img.bianyuzhou.com，说明后端已经拼好了，直接返回
  if (path.includes('img.bianyuzhou.com')) {
    return path;
  }

  // 如果是那种旧的、不带域名的相对路径，才执行下面的拼接逻辑
  const TARGET_HOST = 'https://img.bianyuzhou.com'; // 统一使用图片服务器域名
  
  if (!path.startsWith('/')) path = '/' + path;

  // 确保 /uploads 不重复
  if (!path.startsWith('/uploads') && path !== '/土豆.jpg') {
    path = '/uploads' + path;
  }

  return `${TARGET_HOST}${path}`;
};

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const formatTime = (t) => {
  if (!t) return '';
  const date = new Date(t);
  if (isNaN(date.getTime())) return '';
  const M = (date.getMonth() + 1).toString().padStart(2, '0');
  const D = date.getDate().toString().padStart(2, '0');
  const h = date.getHours().toString().padStart(2, '0');
  const m = date.getMinutes().toString().padStart(2, '0');
  return `${M}-${D} ${h}:${m}`;
};

// --- API ---

const loadComments = async () => {
  // 🔥 防御性检查：如果没有 ID，不发请求
  if (!props.drawingId) return;

  try {
    const res = await apiClient.get(`/Drawing/comment_list`, {
      // 🔥 修改 2: 使用 props.drawingId
      params: { drawingId: props.drawingId }
    })
    
    if (res.data.success) {
      const rawData = res.data.data || [];

      // 递归拍平函数
      const flattenReplies = (nodes) => {
        let result = [];
        if (!nodes || nodes.length === 0) return result;
        
        nodes.forEach(node => {
          const safeNode = { ...node, Id: node.Id || node.id };
          result.push(safeNode);
          
          if (node.Replies && node.Replies.length > 0) {
            result = result.concat(flattenReplies(node.Replies));
          }
        });
        return result.sort((a, b) => new Date(a.CreateTime) - new Date(b.CreateTime));
      };

      let count = 0;
      const processedData = rawData.map(item => {
        count++;
        const flatList = flattenReplies(item.Replies || []);
        count += flatList.length;

        return {
          ...item,
          Id: item.Id || item.id,
          FlatReplies: flatList 
        };
      });

      comments.value = processedData;
      total.value = count;
    }
  } catch (err) {
    console.error('加载失败', err)
  }
}

const handleSubmit = async () => {
  if (!inputContent.value.trim() || isSubmitting.value) return
  
  // 🔥 防御性检查
  if (!props.drawingId) {
    alert("数据尚未加载完成，请稍后再试");
    return;
  }

  isSubmitting.value = true
  try {
    const target = replyTarget.value;
    const parentId = target ? (target.Id || target.id) : null;

    const payload = {
      // 🔥 修改 3: 使用 props.drawingId
      DrawingId: props.drawingId,
      Content: inputContent.value,
      ParentId: parentId
    }

    // 打印一下，方便调试
    console.log("提交评论:", payload);

    const res = await apiClient.post(`/Drawing/add_comment`, payload)
    if (res.data.success) {
      inputContent.value = ''
      replyTarget.value = null
      await loadComments() 
    }
  } catch (err) {
    console.error(err)
    // 如果后端返回了具体的错误信息，可以 alert 出来
    if (err.response && err.response.data && err.response.data.message) {
      alert(err.response.data.message);
    }
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
// 🔥 修改 4: 监听 drawingId 变化
watch(() => props.drawingId, (newVal) => {
  if(newVal) loadComments();
})
</script>

<style scoped>
/* 容器调整 */
.comment-section {
  display: flex; flex-direction: column; height: 100%; 
  border-top: 1px solid #f1f5f9; margin-top: 15px; padding-top: 15px;
}
.section-title { font-size: 15px; font-weight: 600; margin-bottom: 15px; color: #1e293b; }
.comment-list { flex: 1; overflow-y: auto; padding-right: 6px; }

/* 滚动条美化 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }

/* 单个评论组 (包含一级和它的子回复) */
.comment-group { margin-bottom: 20px; border-bottom: 1px solid #f8fafc; padding-bottom: 15px; }

/* === 一级评论样式 === */
.root-item { display: flex; gap: 12px; margin-bottom: 10px; }
.avatar-box img { width: 36px; height: 36px; border-radius: 50%; object-fit: cover; border: 1px solid #f1f5f9; }
.comment-content { flex: 1; }
.main-name { font-size: 13px; font-weight: 600; color: #334155; }
.time { font-size: 11px; color: #94a3b8; margin-left: 8px; font-weight: normal; }
.root-text { font-size: 14px; color: #334155; line-height: 1.5; margin: 4px 0; }
.op-bar { margin-top: 4px; }
.reply-btn { font-size: 12px; color: #64748b; cursor: pointer; transition: color 0.2s; }
.reply-btn:hover { color: #8b5cf6; }

/* === 子回复区域 (关键样式优化) === */
.replies-wrapper {
  margin-left: 48px; /* 跟一级头像对齐后的偏移 */
  background-color: #f8fafc; /* 浅灰底色 */
  border-radius: 8px;
  padding: 10px 12px;
}

.sub-comment-item {
  display: flex;
  gap: 8px;
  margin-bottom: 10px;
}
.sub-comment-item:last-child { margin-bottom: 0; }

/* 子评论头像更小 */
.sub-avatar img { width: 24px; height: 24px; border-radius: 50%; object-fit: cover; }

.sub-content-box { flex: 1; }

.sub-header {
  display: flex; align-items: center; flex-wrap: wrap; gap: 4px;
  font-size: 12px; line-height: 1.4;
}

/* 名字高亮 */
.sub-user { color: #475569; font-weight: 600; }
.target-user { color: #475569; font-weight: 600; }

/* 关系箭头 */
.reply-relation { color: #94a3b8; display: flex; align-items: center; gap: 4px; margin: 0 2px; }
.arrow { font-size: 10px; transform: scale(0.8); }

.sub-time { color: #cbd5e1; font-size: 11px; margin-left: auto; }

/* 子评论内容 */
.sub-text {
  font-size: 13px; color: #1e293b; margin-top: 2px; line-height: 1.4;
}

/* 子回复按钮 */
.sub-op { margin-top: 2px; text-align: right; } /* 按钮放右边，或者放左边看你喜好 */
.reply-btn-mini { font-size: 11px; color: #94a3b8; cursor: pointer; }
.reply-btn-mini:hover { color: #8b5cf6; text-decoration: underline; }

/* === 底部输入框 === */
.comment-input-area { padding: 12px 0; border-radius:12px;  background: #00000000; }
.reply-hint {
  font-size: 12px; color: #64748b; background: #f1f5f9;
  padding: 4px 10px; border-radius: 4px; margin-bottom: 8px;
  display: inline-flex; align-items: center; gap: 8px;
}
.cancel-btn { 
  cursor: pointer; width: 16px; height: 16px; background: #cbd5e1; color: #fff; 
  border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px;
}
.input-row { display: flex; gap: 10px; }
.input-row input { 
  flex: 1; border: 1px solid #3e3e3e; border-radius: 20px; 
  padding: 8px 16px; font-size: 13px; outline: none; transition: border 0.2s; color:#000000
}
.input-row input:focus { border-color: #000000; }
.send-btn { 
  background: #4d849c; color: #010101; border: none; border-radius: 20px; 
  padding: 0 20px; font-size: 13px; cursor: pointer; font-weight: 500;
  transition: opacity 0.2s;
}
.send-btn:disabled { background: #e2e8f0; color: #94a3b8; cursor: not-allowed; }

.empty-comment{color: #334155;}
</style>