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
            <div class="cyber-avatar-frame main-size">
              <UserAvatar 
                :user-id="data.author_id" 
                :passed-avatar="data.author?.avatar"
                :show-level="true"
              />
              <div class="frame-scanline"></div>
            </div>
            <div class="author-meta">
              <span class="author-name">@{{ data.author?.username }}</span>
              <span class="author-id">CREATOR_ID: #{{ data.author_id }}</span>
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
                <div class="cyber-avatar-frame comment-size">
                  <UserAvatar 
                    :user-id="c.user_id || c.author_id" 
                    :passed-avatar="c.author?.avatar"
                    :show-level="false" 
                  />
                  <div class="frame-scanline"></div>
                </div>

                <div class="header-info-col">
                  <div class="info-top-row">
                    <span class="reply-user">> {{ c.author?.username }}</span>
                    <span v-if="c.replyToUser" class="reply-target-tag">@{{ c.replyToUser }}</span>
                  </div>
                  <span class="reply-time">{{ formatTime(c.createTime) }}</span>
                </div>
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
import apiClient from '@/utils/api';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';

const props = defineProps({
  data: { type: Object, required: true },
  fixAvatar: { type: Function, required: true }
});

const localComments = ref([]);
const commentText = ref('');
const commentInput = ref(null);
const replyTarget = ref(null);
const isSubmitting = ref(false);
const loadingComments = ref(false);

const formatTime = (t) => t ? new Date(t).toLocaleString() : 'N/A';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const fetchComments = async () => {
  loadingComments.value = true;
  try {
    const res = await apiClient.get(`/ThePost/${props.data.id}/comments`);
    if (res.data.success) {
      localComments.value = processCommentTree(res.data.data);
    }
  } catch (e) { console.error(e); } finally { loadingComments.value = false; }
};

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
      await fetchComments();
      commentText.value = '';
      cancelLocalReply();
    }
  } catch (e) { alert("传输中断"); } finally { isSubmitting.value = false; }
};

const prepareReply = (comment) => {
  replyTarget.value = comment;
  document.getElementById('comment-terminal')?.scrollIntoView({ behavior: 'smooth' });
  commentInput.value?.focus();
};
const cancelLocalReply = () => { replyTarget.value = null; };

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
  return result.sort((a, b) => new Date(b.createTime) - new Date(a.createTime));
};

onMounted(fetchComments);
watch(() => props.data.id, fetchComments);
</script>

<style scoped>
/* --- 1. 基础布局 --- */
.post-detail-wrapper { position: relative; background: #f4f1ea; min-height: 100vh; color: #111; }
.detail-grid-bg { position: absolute; inset: 0; background-image: linear-gradient(#e0ddd5 1px, transparent 1px), linear-gradient(90deg, #e0ddd5 1px, transparent 1px); background-size: 40px 40px; pointer-events: none; opacity: 0.5; }
.post-detail-content { position: relative; z-index: 2; max-width: 960px; margin: 0 auto; padding-bottom: 100px; }

/* --- 2. 赛博头部 --- */
.header-dashboard { background: #111; color: #f4f1ea; padding: 50px 40px; border-bottom: 8px solid #D92323; }
.header-top-bar { display: flex; justify-content: space-between; font-family: 'JetBrains Mono'; font-size: 0.8rem; margin-bottom: 20px; }
.status-led { color: #D92323; font-weight: bold; display: flex; align-items: center; gap: 8px; }
.led-dot { width: 10px; height: 10px; background: #D92323; border-radius: 50%; box-shadow: 0 0 10px #D92323; animation: blink 1s infinite; }
.post-title-heavy { font-family: 'Anton'; font-size: 3.5rem; text-transform: uppercase; line-height: 1; margin: 0 0 30px 0; letter-spacing: -2px; }

/* --- ✨ 关键：赛博头像框回归 ✨ --- */
.cyber-avatar-frame {
  position: relative;
  border: 2px solid #D92323;
  background: #000;
  padding: 2px;
  flex-shrink: 0;
  z-index: 10;
}
.main-size { width: 55px; height: 55px; }
.comment-size { width: 44px; height: 44px; }
.frame-scanline { position: absolute; top: 0; left: 0; width: 100%; height: 2px; background: rgba(217, 35, 35, 0.4); animation: scan 3s infinite linear; pointer-events: none; }

.metadata-row { display: flex; justify-content: space-between; align-items: flex-end; }
.author-tag-md { display: flex; align-items: center; gap: 15px; background: rgba(255,255,255,0.05); padding: 12px 20px; border-radius: 2px; }
.author-name { font-weight: 900; font-size: 1.2rem; display: block; }
.author-id { opacity: 0.6; font-size: 0.7rem; font-family: 'JetBrains Mono'; }

/* --- 3. 正文层 --- */
.article-surface { background: #fff; margin-top: -30px; padding: 60px; border: 3px solid #111; box-shadow: 12px 12px 0 rgba(0,0,0,0.2); }
.industrial-gallery { display: flex; flex-direction: column; gap: 30px; margin-bottom: 40px; }
.gallery-packet { border: 3px solid #111; background: #000; }
.content-image { width: 100%; display: block; }
.img-barcode { background: #111; color: #fff; padding: 6px 15px; text-align: right; font-family: 'JetBrains Mono'; font-size: 0.7rem; }
.cyber-markdown-body { font-size: 1.15rem; line-height: 1.8; }

/* --- 4. 评论终端 (黄金终端) --- */
.transmission-zone { margin: 60px 40px; }
.zone-header-row { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 12px; }
.zone-label { background: #111; color: #fff; padding: 6px 15px; font-family: 'JetBrains Mono'; font-weight: bold; }
.reply-status-tag { background: #D92323; color: #fff; padding: 6px 15px; font-family: 'JetBrains Mono'; display: flex; gap: 10px; }
.abort-btn { background: #000; color: #fff; border: none; cursor: pointer; padding: 0 5px; }

.terminal-input-box { background: #ffcc00; border: 4px solid #111; padding: 25px; box-shadow: 10px 10px 0 #111; }
.cyber-textarea { width: 100%; height: 140px; border: 2px solid #111; padding: 20px; font-size: 1.2rem; outline: none; background: #fff; }
.terminal-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 20px; }
.action-btn-heavy { background: #D92323; color: #fff; border: 3px solid #111; padding: 15px 40px; cursor: pointer; transition: 0.2s; display: flex; gap: 15px; box-shadow: 4px 4px 0 #111; }
.action-btn-heavy:hover:not(:disabled) { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 #111; }

/* --- 5. 评论列表 --- */
.replies-zone { padding: 0 40px; }
.zone-header { display: flex; justify-content: space-between; border-bottom: 5px solid #111; padding-bottom: 10px; margin-bottom: 40px; }
.zone-header .label { font-family: 'Anton'; font-size: 2.2rem; }
.reply-node { position: relative; margin-bottom: 30px; }
.thread-line { position: absolute; left: -16px; top: 0; bottom: -30px; width: 3px; background: #111; border-left: 2px dashed #D92323; }
.reply-card-md { background: #fff; border: 2px solid #111; padding: 25px; box-shadow: 8px 8px 0 rgba(0,0,0,0.05); transition: 0.3s; }
.reply-card-md:hover { transform: translateX(5px); border-color: #D92323; }

.reply-header { display: flex; align-items: flex-start; gap: 15px; margin-bottom: 15px; border-bottom: 1px dashed #e0e0e0; padding-bottom: 10px; }
.reply-user { font-weight: 900; color: #D92323; font-family: 'JetBrains Mono'; font-size: 1rem; }
.reply-target-tag { background: #111; color: #fff; padding: 2px 6px; font-size: 0.75rem; margin-left: 10px; }
.reply-time { color: #888; font-weight: bold; font-size: 0.8rem; }
.btn-reply-trigger { background: #111; color: #fff; border: none; padding: 8px 15px; font-family: 'JetBrains Mono'; cursor: pointer; margin-top: 15px; }

@keyframes scan { 0% { top: 0; } 100% { top: 100%; } }
@keyframes blink { 50% { opacity: 0.3; } }
</style>