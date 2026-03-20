<template>
  <div class="post-detail-wrapper">
    <div class="detail-grid-bg"></div>

    <div class="post-detail-content">
      <header class="header-dashboard">
        <div class="header-top-bar">
          <span class="protocol-label">STATUS // DATA_SYNC_ACTIVE</span>
          <span class="status-led"><span class="led-dot"></span> ENCRYPTED_LINK</span>
        </div>
        
        <h1 class="post-title-heavy">{{ data.post_title }}</h1>

        <div class="metadata-row">
          <div class="author-tag-md">
            <div class="avatar-wrapper">
              <img :src="fixAvatar(data.author?.avatar)" @error="handleImgError" />
            </div>
            <div class="author-meta">
              <span class="author-name">@{{ data.author?.username }}</span>
              <span class="author-id">AUTH_ID: {{ data.id }}</span>
            </div>
          </div>
          <div class="time-block-cyber">
            <span class="label">TIMESTAMP:</span> {{ formatTime(data.createTime) }}
          </div>
        </div>
      </header>

      <article class="article-surface">
        <div v-if="data.images?.length" class="industrial-gallery">
          <div v-for="(img, i) in data.images" :key="i" class="gallery-packet">
            <img :src="img" class="content-image" @error="handleImgError" />
            <div class="img-barcode">PACKET_ID_{{ i + 1 }} // TYPE: VISUAL_DATA</div>
          </div>
        </div>

        <div class="cyber-markdown-body markdown-body" v-html="renderMarkdown(data.content)"></div>

        <div class="end-bar">
          <div class="line"></div>
          <span>::: TRANSMISSION_COMPLETE :::</span>
          <div class="line"></div>
        </div>
      </article>

      <section class="transmission-zone" id="comment-terminal">
        <div class="zone-header-row">
          <div class="zone-label">COMMAND_CONSOLE</div>
          <Transition name="fade">
            <div v-if="replyTarget" class="reply-status-tag">
              REPLYING_TO: @{{ replyTarget.author?.username }}
              <button class="abort-btn" @click="cancelLocalReply">ABORT [X]</button>
            </div>
          </Transition>
        </div>

        <div class="terminal-input-box">
          <textarea 
            ref="commentInput"
            v-model="commentText" 
            :placeholder="replyTarget ? '输入你的响应指令...' : '在此处广播你的信号...'" 
            class="cyber-textarea"
          ></textarea>
          
          <div class="terminal-footer">
            <div class="input-stats">BUFFER: {{ commentText.length }}/500</div>
            <button 
              class="action-btn-heavy" 
              :disabled="!commentText.trim() || isSubmitting" 
              @click="submitLocalComment"
            >
              <span class="btn-text">{{ isSubmitting ? 'TRANSMITTING...' : 'EXECUTE_SEND' }}</span>
              <span class="btn-icon">>></span>
            </button>
          </div>
        </div>
      </section>

      <section class="replies-zone">
        <div class="zone-header">
          <span class="label">NEURAL_FEEDBACK</span>
          <span class="count">{{ localComments.length }} RESPONSES</span>
        </div>

        <div v-if="localComments.length" class="replies-stack">
          <div 
            v-for="c in localComments" 
            :key="c.id" 
            class="reply-node"
            :style="{ marginLeft: (c.level * 32) + 'px' }"
          >
            <div v-if="c.level > 0" class="thread-line"></div>
            
            <div class="reply-card-md" :class="{ 'is-nested': c.level > 0 }">
              <div class="reply-header">
                <span class="reply-user">> {{ c.author?.username }}</span>
                <span v-if="c.replyToUser" class="reply-target-tag">@{{ c.replyToUser }}</span>
                <span class="reply-time">{{ formatTime(c.createTime) }}</span>
              </div>
              <div class="reply-body">{{ c.content }}</div>
              
              <div class="reply-actions">
                <button class="btn-reply-trigger" @click="prepareReply(c)">
                  [ REPLY_SIGNAL ]
                </button>
              </div>
            </div>
          </div>
        </div>
        <div v-else-if="loadingComments" class="null-signal">SYNCING_COMMENTS...</div>
        <div v-else class="null-signal">[ NO_FEEDBACK_DETECTED ]</div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { marked } from 'marked';
import apiClient from '@/utils/api'; // 引入你的 API 客户端

const props = defineProps({
  data: { type: Object, required: true },
  fixAvatar: { type: Function, required: true }
});

// 内部状态
const localComments = ref([]);
const commentText = ref('');
const commentInput = ref(null);
const replyTarget = ref(null);
const isSubmitting = ref(false);
const loadingComments = ref(false);

const formatTime = (t) => t ? new Date(t).toLocaleString() : 'N/A';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

// --- API 交互 1：获取全部评论 ---
const fetchComments = async () => {
  loadingComments.value = true;
  try {
    const res = await apiClient.get(`/ThePost/${props.data.id}/comments`);
    if (res.data.success) {
      // 这里的逻辑是将平铺数据处理成带 level 的树形展示逻辑
      // 简单起见，这里直接赋值，实际渲染可根据后端 ParentCommentId 计算 level
      localComments.value = processCommentTree(res.data.data);
    }
  } catch (e) {
    console.error("评论获取失败", e);
  } finally {
    loadingComments.value = false;
  }
};

// --- API 交互 2：提交评论 (对接 /json 接口) ---
const submitLocalComment = async () => {
  if (!commentText.value.trim()) return;
  isSubmitting.value = true;
  
  try {
    const payload = {
      Content: commentText.value,
      ParentCommentId: replyTarget.value?.id || 0
    };
    
    const res = await apiClient.post(`/ThePost/${props.data.id}/comments/json`, payload);
    
    if (res.data.success) {
      // 提交成功，重新获取或手动推入
      await fetchComments();
      commentText.value = '';
      cancelLocalReply();
    }
  } catch (e) {
    alert("数据传输中断: 身份验证失效或服务器超时");
  } finally {
    isSubmitting.value = false;
  }
};

// 交互逻辑
const prepareReply = (comment) => {
  replyTarget.value = comment;
  document.getElementById('comment-terminal')?.scrollIntoView({ behavior: 'smooth' });
  commentInput.value?.focus();
};

const cancelLocalReply = () => {
  replyTarget.value = null;
};

// 工具：简单的树形层级处理逻辑
const processCommentTree = (list) => {
  const map = {}, result = [];
  list.forEach(item => { item.level = 0; map[item.id] = item; });
  list.forEach(item => {
    if (item.ParentCommentId && map[item.ParentCommentId]) {
      item.level = map[item.ParentCommentId].level + 1;
      item.replyToUser = map[item.ParentCommentId].author.username;
    }
    result.push(item);
  });
  // 排序：按时间倒序
  return result.sort((a, b) => new Date(b.createTime) - new Date(a.createTime));
};

onMounted(() => {
  fetchComments();
});

// 监听数据变化（当点击另一个帖子时）
watch(() => props.data.id, () => {
  fetchComments();
});
</script>

<style scoped>
/* --- 1. 基础布局 --- */
.post-detail-wrapper { position: relative; background: #f4f1ea; min-height: 100%; color: #111; }
.detail-grid-bg { position: absolute; inset: 0; background-image: linear-gradient(#e0ddd5 1px, transparent 1px), linear-gradient(90deg, #e0ddd5 1px, transparent 1px); background-size: 40px 40px; pointer-events: none; opacity: 0.5; }
.post-detail-content { position: relative; z-index: 2; max-width: 960px; margin: 0 auto; padding-bottom: 100px; }

/* --- 2. 头部：黑底红字 (高压对比度) --- */
.header-dashboard { background: #111; color: #f4f1ea; padding: 50px 40px; border-bottom: 8px solid #D92323; }
.header-top-bar { display: flex; justify-content: space-between; font-family: 'JetBrains Mono'; font-size: 0.8rem; margin-bottom: 20px; }
.status-led { color: #D92323; font-weight: bold; display: flex; align-items: center; gap: 8px; }
.led-dot { width: 10px; height: 10px; background: #D92323; border-radius: 50%; box-shadow: 0 0 10px #D92323; animation: blink 1s infinite; }

.post-title-heavy { font-family: 'Anton'; font-size: 3.5rem; text-transform: uppercase; line-height: 1; margin: 0 0 30px 0; letter-spacing: -2px; }

.metadata-row { display: flex; justify-content: space-between; align-items: flex-end; }
.author-tag-md { display: flex; align-items: center; gap: 15px; background: rgba(255,255,255,0.1); padding: 12px 20px; border-radius: 2px; }
.avatar-wrapper { width: 50px; height: 50px; border: 3px solid #D92323; }
.avatar-wrapper img { width: 100%; height: 100%; object-fit: cover; }
.author-name { font-weight: 900; font-size: 1.2rem; display: block; }
.time-block-cyber { font-family: 'JetBrains Mono'; font-size: 0.9rem; }

/* --- 3. 内容层 --- */
.article-surface { background: #fff; margin-top: -30px; padding: 60px; border: 3px solid #111; box-shadow: 12px 12px 0 rgba(0,0,0,0.2); }
.industrial-gallery { display: flex; flex-direction: column; gap: 30px; margin-bottom: 40px; }
.gallery-packet { border: 3px solid #111; background: #000; }
.content-image { width: 100%; display: block; }
.img-barcode { background: #111; color: #fff; padding: 6px 15px; text-align: right; font-family: 'JetBrains Mono'; font-size: 0.7rem; }

.cyber-markdown-body { font-size: 1.2rem; line-height: 1.8; }
.cyber-markdown-body :deep(h2) { font-family: 'Anton'; border-left: 10px solid #D92323; padding-left: 20px; margin: 40px 0 20px 0; }

/* --- 4. ✨ 评论终端：警告黄 (绝对可见) --- */
.transmission-zone { margin: 60px 40px; }
.zone-header-row { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 12px; }
.zone-label { background: #111; color: #fff; padding: 6px 15px; font-family: 'JetBrains Mono'; font-weight: bold; }
.reply-status-tag { background: #D92323; color: #fff; padding: 6px 15px; font-family: 'JetBrains Mono'; display: flex; gap: 10px; }
.abort-btn { background: #000; color: #fff; border: none; cursor: pointer; padding: 0 5px; }

.terminal-input-box { background: #ffcc00; border: 4px solid #111; padding: 25px; box-shadow: 10px 10px 0 #111; }
.cyber-textarea { width: 100%; height: 140px; border: 2px solid #111; padding: 20px; font-size: 1.2rem; font-family: inherit; outline: none; background: #fff; }
.cyber-textarea:focus { box-shadow: inset 4px 4px 0 rgba(0,0,0,0.1); }

.terminal-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 20px; }
.input-stats { font-family: 'JetBrains Mono'; font-weight: bold; color: #111; }

/* 发送按钮：红底、黑框、硬投影 */
.action-btn-heavy { background: #D92323; color: #fff; border: 3px solid #111; padding: 15px 40px; cursor: pointer; transition: 0.2s; display: flex; gap: 15px; box-shadow: 4px 4px 0 #111; }
.action-btn-heavy:hover:not(:disabled) { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 #111; background: #ff0000; }
.action-btn-heavy:disabled { background: #999; box-shadow: none; cursor: not-allowed; }
.btn-text { font-weight: 900; font-family: 'Anton'; font-size: 1.2rem; }

/* --- 5. 评论列表区 --- */
.replies-zone { padding: 0 40px; }
.zone-header { display: flex; justify-content: space-between; border-bottom: 5px solid #111; padding-bottom: 10px; margin-bottom: 40px; }
.zone-header .label { font-family: 'Anton'; font-size: 2.2rem; }

.reply-node { position: relative; margin-bottom: 30px; }
.thread-line { position: absolute; left: -16px; top: 0; bottom: -30px; width: 3px; background: #111; border-left: 2px dashed #D92323; }

.reply-card-md { background: #fff; border: 2px solid #111; padding: 25px; box-shadow: 8px 8px 0 rgba(0,0,0,0.05); transition: 0.3s; }
.reply-card-md:hover { border-color: #D92323; transform: translateX(5px); box-shadow: 8px 8px 0 rgba(217,35,35,0.2); }
.is-nested { border-left: 10px solid #111; }

.reply-header { display: flex; gap: 15px; margin-bottom: 15px; font-family: 'JetBrains Mono'; font-size: 0.9rem; }
.reply-user { font-weight: 900; color: #D92323; }
.reply-target-tag { background: #111; color: #fff; padding: 2px 8px; font-size: 0.8rem; }
.reply-time { margin-left: auto; color: #888; }
.reply-body { font-size: 1.15rem; line-height: 1.6; }

.reply-actions { margin-top: 20px; }
.btn-reply-trigger { background: #111; color: #fff; border: none; padding: 8px 15px; font-family: 'JetBrains Mono'; font-weight: bold; cursor: pointer; transition: 0.2s; }
.btn-reply-trigger:hover { background: #ffcc00; color: #111; }

.null-signal { text-align: center; padding: 60px; font-family: 'JetBrains Mono'; font-size: 1rem; color: #888; border: 2px dashed #ccc; }

@keyframes blink { 50% { opacity: 0.3; } }
@keyframes marquee { 0% { transform: translateX(0); } 100% { transform: translateX(-50%); } }
</style>