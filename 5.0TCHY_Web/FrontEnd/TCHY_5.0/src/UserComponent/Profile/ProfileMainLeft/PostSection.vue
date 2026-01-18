<template>
  <div class="view-section view-posts custom-scroll">
    <div class="broadcast-station">
      <div class="station-header">
        <span class="blink">●</span> BROADCAST_EMITTER // 发送广播
      </div>
      <div class="input-matrix">
        <textarea 
          v-model="newPostContent" 
          placeholder="正在等待指令输入..." 
          class="cyber-textarea"
        ></textarea>
        <div class="matrix-footer">
          <div class="tools">
            <button class="tool-btn">[IMG]</button>
            <button class="tool-btn">[LOC]</button>
          </div>
          <button class="emit-btn" @click="publishPost">
            <span class="btn-text">EMIT // 发布</span>
            <div class="btn-glitch"></div>
          </button>
        </div>
      </div>
    </div>

    <div class="feed-stream">
      <TransitionGroup name="list-decode">
        <div v-for="post in posts" :key="post.id" class="cyber-post-card">
          <div class="post-sidebar">
            <div class="avatar-box">
              <img :src="post.avatar" alt="avatar" />
            </div>
            <div class="connect-line"></div>
          </div>
          
          <div class="post-main">
            <div class="post-header">
              <span class="u-name">{{ post.user }}</span>
              <span class="u-tag">LV.{{ post.level }}</span>
              <span class="time-stamp">{{ post.time }}</span>
            </div>
            
            <div class="post-content">
              {{ post.content }}
            </div>
            
            <div v-if="post.images && post.images.length" class="post-imgs">
              <div 
                v-for="(img, idx) in post.images" 
                :key="idx" 
                class="img-slot"
              >
                <img :src="img" loading="lazy" />
                <div class="scan-line"></div>
              </div>
            </div>

            <div class="post-actions">
              <button class="act-btn" :class="{ active: post.liked }" @click="toggleLike(post)">
                <span class="icon">♥</span> {{ post.likes }}
              </button>
              <button class="act-btn">
                <span class="icon">💬</span> REPLY
              </button>
              <button class="act-btn share">
                <span class="icon">↱</span> SHARE
              </button>
            </div>
          </div>
          
          <div class="corner-tr"></div>
          <div class="corner-bl"></div>
        </div>
      </TransitionGroup>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'

// --- Mock Data ---
const newPostContent = ref('')

const posts = reactive([
  {
    id: 101,
    user: 'Cyber_Punk',
    level: 42,
    avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
    time: 'JUST NOW',
    content: '刚刚完成了新的神经接口调试，感觉延迟降低了 15ms。这也是一种艺术吗？#Cybernetics',
    images: ['https://picsum.photos/300/200?random=10'],
    likes: 24,
    liked: false
  },
  {
    id: 102,
    user: 'Glitch_Artist',
    level: 15,
    avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Aneka',
    time: '2 HOURS AGO',
    content: '不要温和地走进那个良夜，除非你带了夜视仪。正在重构我的渲染引擎。',
    images: [],
    likes: 108,
    liked: true
  },
  {
    id: 103,
    user: 'System_Root',
    level: 99,
    avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Root',
    time: 'YESTERDAY',
    content: '检测到未授权的访问请求。安全协议已更新至 v4.5。',
    images: ['https://picsum.photos/300/200?random=11', 'https://picsum.photos/300/200?random=12'],
    likes: 550,
    liked: false
  }
])

// --- Methods ---
const publishPost = () => {
  if (!newPostContent.value.trim()) return
  
  posts.unshift({
    id: Date.now(),
    user: 'USER_CURRENT',
    level: 42,
    avatar: 'https://api.dicebear.com/7.x/notionists/svg?seed=Felix',
    time: 'PROCESSING...',
    content: newPostContent.value,
    images: [],
    likes: 0,
    liked: false
  })
  newPostContent.value = ''
}

const toggleLike = (post) => {
  post.liked = !post.liked
  post.liked ? post.likes++ : post.likes--
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

.view-posts {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 30px;
  --post-border: #333;
  --post-bg: #fff;
  --accent: #D92323;
}

/* --- 发布器 --- */
.broadcast-station {
  border: 2px solid var(--post-border);
  background: #f4f4f4;
  padding: 2px;
}
.station-header {
  background: var(--post-border);
  color: #fff;
  font-family: 'JetBrains Mono';
  font-size: 0.75rem;
  padding: 5px 10px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.blink { animation: blink 1s infinite; color: var(--accent); }

.cyber-textarea {
  width: 100%;
  border: none;
  background: #fff;
  padding: 15px;
  font-family: 'Noto Sans SC';
  resize: none;
  height: 80px;
  outline: none;
  display: block;
}
.matrix-footer {
  border-top: 1px dashed #ccc;
  padding: 8px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fff;
}
.tools { display: flex; gap: 10px; }
.tool-btn {
  background: none; border: 1px solid transparent;
  font-family: 'JetBrains Mono'; font-size: 0.7rem; cursor: pointer; color: #666;
}
.tool-btn:hover { color: var(--accent); border-color: #eee; }

.emit-btn {
  background: var(--post-border);
  color: #fff;
  border: none;
  padding: 6px 20px;
  font-family: 'JetBrains Mono';
  font-weight: bold;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: 0.2s;
}
.emit-btn:hover { background: var(--accent); }
.emit-btn:active { transform: scale(0.95); }

/* --- 帖子卡片 --- */
.feed-stream { display: flex; flex-direction: column; gap: 20px; }

.cyber-post-card {
  display: flex;
  gap: 15px;
  border-bottom: 1px solid #eee;
  padding-bottom: 20px;
  position: relative;
  background: #fff;
  transition: transform 0.2s;
  padding: 15px;
  border: 1px solid transparent;
}
.cyber-post-card:hover {
  background: #fafafa;
  border-color: #eee;
  box-shadow: 5px 5px 0 rgba(0,0,0,0.05);
}

.post-sidebar { display: flex; flex-direction: column; align-items: center; gap: 10px; width: 40px; }
.avatar-box { width: 40px; height: 40px; border: 2px solid var(--post-border); overflow: hidden; }
.avatar-box img { width: 100%; height: 100%; object-fit: cover; }
.connect-line { flex: 1; width: 1px; background: #ddd; }

.post-main { flex: 1; }
.post-header { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; }
.u-name { font-weight: 800; font-family: 'Noto Sans SC'; font-size: 0.95rem; }
.u-tag { background: #000; color: #fff; font-size: 0.6rem; padding: 1px 4px; font-family: 'JetBrains Mono'; }
.time-stamp { color: #999; font-size: 0.7rem; font-family: 'JetBrains Mono'; margin-left: auto; }

.post-content { font-size: 0.9rem; line-height: 1.6; color: #333; margin-bottom: 15px; font-family: 'Noto Sans SC'; }

.post-imgs { display: flex; gap: 10px; margin-bottom: 15px; flex-wrap: wrap; }
.img-slot { 
  width: 120px; height: 80px; 
  border: 1px solid #000; 
  position: relative; overflow: hidden; cursor: pointer;
}
.img-slot img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; }
.img-slot:hover img { transform: scale(1.1); filter: grayscale(100%) contrast(1.2); }
/* 扫描线特效 */
.scan-line {
  position: absolute; top:0; left:0; width:100%; height:100%;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,0,0,0.2) 51%);
  background-size: 100% 4px;
  pointer-events: none;
}

.post-actions { display: flex; gap: 20px; }
.act-btn { 
  background: none; border: none; cursor: pointer; 
  font-family: 'JetBrains Mono'; font-size: 0.75rem; 
  color: #888; display: flex; align-items: center; gap: 5px; 
  transition: 0.2s;
}
.act-btn:hover { color: #000; }
.act-btn.active { color: var(--accent); }

/* 角标装饰 */
.corner-tr { position: absolute; top: 0; right: 0; width: 8px; height: 8px; border-top: 2px solid var(--accent); border-right: 2px solid var(--accent); opacity: 0; transition: 0.2s; }
.corner-bl { position: absolute; bottom: 0; left: 0; width: 8px; height: 8px; border-bottom: 2px solid var(--accent); border-left: 2px solid var(--accent); opacity: 0; transition: 0.2s; }
.cyber-post-card:hover .corner-tr, .cyber-post-card:hover .corner-bl { opacity: 1; }

/* 列表动画 */
.list-decode-enter-active,
.list-decode-leave-active {
  transition: all 0.4s ease;
}
.list-decode-enter-from {
  opacity: 0;
  transform: translateX(-20px);
  clip-path: inset(0 100% 0 0);
}
.list-decode-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
</style>