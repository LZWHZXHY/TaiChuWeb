<template>
  <Teleport to="body">
    <Transition name="glitch-fade">
      <div v-if="visible" class="cyber-lightbox-overlay" @click="handleClose">
        
        <div class="cyber-terminal-window" @click.stop>
          
          <div class="terminal-header">
            <div class="header-left">
              <span class="status-dot blink"></span>
              <span class="term-title">ASSET_VIEWER // {{ artwork.id || 'UNKNOWN_ID' }}</span>
            </div>
            <button class="term-close-btn" @click="handleClose">
              [ ABORT / ESC ]
            </button>
          </div>

          <div class="terminal-body">
            
            <div class="image-viewport" @click="handleClose">
              <div class="viewport-deco-tl"></div>
              <div class="viewport-deco-br"></div>
              
              <img 
                :src="upgradeUrlToHttps(artwork.imageUrlFull || artwork.url)" 
                class="main-img" 
                @click.stop 
              />
              
              <div class="viewport-hud">
                <span class="hud-text top-left">IMG_RENDER_MODE: HIGH_RES</span>
                <span class="hud-text bottom-right">SCALE: 100%</span>
              </div>

              <div class="hud-actions" @click.stop>
                <button 
                  class="hud-like-btn"
                  :class="{ 'is-active': artwork.isLiked }"
                  @click.stop="onLike"
                >
                  <span class="icon" :class="{ 'heart-beat': artwork.isAnimating }">
                    {{ artwork.isLiked ? '♥' : '♡' }}
                  </span>
                  <span class="label">ACKNOWLEDGE</span>
                </button>
              </div>
            </div>

            <div class="data-sidebar custom-scroll">
              
              <div class="author-card">
                <div class="avatar-frame">
                  <div class="avatar-box">
                    {{ (artwork.authorName || 'A')[0] }}
                  </div>
                </div>
                <div class="author-meta">
                  <div class="meta-label">UPLOADER_ID:</div>
                  <div class="meta-name">@{{ artwork.authorName || artwork.author }}</div>
                  <div class="meta-time">{{ formatTime(artwork.uploadAt) }}</div>
                </div>
              </div>

              <div class="divider-line"></div>

              <div class="artwork-meta-block">
                <h3 class="meta-title">{{ artwork.title }}</h3>
                <div class="meta-desc custom-scroll">
                  <span class="prefix">>></span> {{ artwork.desc || 'NO_DESCRIPTION_DATA...' }}
                </div>
                
                <button 
                  class="cyber-action-btn" 
                  :class="{ 'is-liked': artwork.isLiked }" 
                  @click="onLike"
                >
                  <div class="btn-inner">
                    <span class="btn-icon">{{ artwork.isLiked ? '★' : '☆' }}</span>
                    <span class="btn-text">
                      {{ artwork.isLiked ? 'APPROVED' : 'APPROVE_WORK' }}
                    </span>
                    <span class="btn-count">[{{ artwork.likes || 0 }}]</span>
                  </div>
                </button>
              </div>

              <div class="divider-line"></div>

              <div class="comment-terminal-area">
                <div class="area-label">// USER_FEEDBACK_LOGS</div>
                <CommentSection :artworkId="artwork.id" />
              </div>

            </div>
          </div>
        </div>

      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import CommentSection from '@/CommentSection/CommentSection.vue'

const props = defineProps({
  visible: { type: Boolean, default: false },
  artwork: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['close', 'like'])

const handleClose = () => { emit('close') }

const onLike = () => {
  emit('like', props.artwork)
}

const upgradeUrlToHttps = (url) => {
  if (!url) return ''
  if (url.startsWith('/')) return url
  return url.replace('http://', 'https://')
}

// 格式化为工业风格时间戳
const formatTime = (t) => {
  if (!t) return 'UNKNOWN_DATE'
  const d = new Date(t)
  return `${d.getFullYear()}.${(d.getMonth()+1).toString().padStart(2,'0')}.${d.getDate().toString().padStart(2,'0')}`
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.cyber-lightbox-overlay { 
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;

  position: fixed; inset: 0; 
  background: rgba(10, 10, 10, 0.95); /* 深色背景，高对比 */
  z-index: 9999; 
  display: flex; justify-content: center; align-items: center; 
  backdrop-filter: blur(5px);
  padding: 20px;
}

/* --- 终端窗口 --- */
.cyber-terminal-window {
  width: 100%; max-width: 1400px; height: 90vh;
  background: var(--white);
  border: 4px solid var(--black);
  box-shadow: 0 0 0 2px #333, 20px 20px 0 rgba(0,0,0,0.5);
  display: flex; flex-direction: column;
  overflow: hidden;
  font-family: var(--mono);
}

/* 1. 顶部栏 */
.terminal-header {
  height: 50px; flex-shrink: 0;
  background: var(--black); color: var(--white);
  display: flex; justify-content: space-between; align-items: center;
  padding: 0 15px; border-bottom: 2px solid var(--red);
}
.header-left { display: flex; align-items: center; gap: 10px; }
.status-dot { width: 10px; height: 10px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.term-title { font-family: var(--heading); font-size: 1.2rem; letter-spacing: 1px; }

.term-close-btn {
  background: transparent; border: 1px solid #555; color: #aaa;
  padding: 5px 15px; cursor: pointer; font-family: var(--mono);
  font-weight: bold; transition: 0.2s;
}
.term-close-btn:hover { background: var(--red); color: var(--white); border-color: var(--red); }

/* 2. 主体布局 */
.terminal-body { flex: 1; display: flex; overflow: hidden; }

/* 左侧：图片视口 */
.image-viewport {
  flex: 1; background: #050505; 
  position: relative; overflow: hidden;
  display: flex; justify-content: center; align-items: center;
  /* 网格背景装饰 */
  background-image: linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px),
                    linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px);
  background-size: 40px 40px;
}

.main-img {
  max-width: 95%; max-height: 95%; object-fit: contain;
  border: 1px solid #333;
  box-shadow: 0 0 30px rgba(0,0,0,0.8);
}

/* 视口 HUD 装饰 */
.viewport-deco-tl { position: absolute; top: 10px; left: 10px; width: 30px; height: 30px; border-top: 2px solid var(--red); border-left: 2px solid var(--red); }
.viewport-deco-br { position: absolute; bottom: 10px; right: 10px; width: 30px; height: 30px; border-bottom: 2px solid var(--red); border-right: 2px solid var(--red); }
.hud-text { position: absolute; color: rgba(255,255,255,0.4); font-size: 0.7rem; pointer-events: none; }
.top-left { top: 15px; left: 50px; }
.bottom-right { bottom: 15px; right: 50px; }

/* 视口悬浮按钮 (HUD Style) */
.hud-actions {
  position: absolute; bottom: 30px; left: 50%; transform: translateX(-50%);
  display: flex; gap: 20px;
}
.hud-like-btn {
  background: rgba(0,0,0,0.6); border: 1px solid #fff; color: #fff;
  padding: 10px 20px; cursor: pointer; font-family: var(--mono);
  display: flex; align-items: center; gap: 10px;
  backdrop-filter: blur(4px); transition: 0.2s;
}
.hud-like-btn:hover { background: #fff; color: #000; }
.hud-like-btn.is-active { background: var(--red); border-color: var(--red); color: #fff; box-shadow: 0 0 15px var(--red); }
.hud-like-btn .icon { font-size: 1.2rem; }

/* 右侧：数据侧边栏 */
.data-sidebar {
  width: 400px; background: var(--white);
  border-left: 4px solid var(--black);
  display: flex; flex-direction: column;
  padding: 25px; gap: 20px;
  overflow-y: auto; flex-shrink: 0;
}

/* 作者信息 */
.author-card { display: flex; align-items: center; gap: 15px; }
.avatar-frame {
  width: 60px; height: 60px; border: 2px solid var(--black);
  padding: 2px;
}
.avatar-box {
  width: 100%; height: 100%; background: var(--black); color: var(--white);
  display: flex; justify-content: center; align-items: center;
  font-family: var(--heading); font-size: 1.5rem;
}
.author-meta { flex: 1; }
.meta-label { font-size: 0.6rem; color: #888; font-weight: bold; }
.meta-name { font-weight: bold; font-size: 1.1rem; color: var(--black); }
.meta-time { font-size: 0.8rem; color: var(--red); font-weight: bold; margin-top: 2px; }

.divider-line { height: 2px; background: #ccc; width: 100%; background-image: repeating-linear-gradient(90deg, #ccc 0, #ccc 5px, transparent 5px, transparent 10px); }

/* 作品信息 */
.artwork-meta-block { display: flex; flex-direction: column; gap: 15px; }
.meta-title { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1.1; text-transform: uppercase; }
.meta-desc {
  font-size: 0.9rem; color: #444; line-height: 1.6;
  max-height: 150px; overflow-y: auto;
  border-left: 2px solid #ccc; padding-left: 10px;
}
.prefix { color: var(--red); font-weight: bold; }

/* 侧边栏大按钮 */
.cyber-action-btn {
  background: var(--white); border: 2px solid var(--black);
  padding: 5px; cursor: pointer; transition: 0.2s;
  width: 100%;
}
.btn-inner {
  background: var(--black); color: var(--white);
  padding: 12px; display: flex; align-items: center; justify-content: center; gap: 10px;
  font-family: var(--heading); font-size: 1.2rem;
  transition: 0.2s;
}
.cyber-action-btn:hover .btn-inner { background: var(--white); color: var(--black); }
.cyber-action-btn.is-liked .btn-inner { background: var(--red); color: var(--white); }
.cyber-action-btn.is-liked:hover { border-color: var(--red); }

/* 评论区 */
.comment-terminal-area { flex: 1; display: flex; flex-direction: column; }
.area-label { font-size: 0.7rem; font-weight: bold; color: #888; margin-bottom: 10px; }

/* 动画与响应式 */
.blink { animation: blink 1s infinite; }
.heart-beat { animation: heartBeat 0.3s ease-in-out; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@keyframes heartBeat { 0% { transform: scale(1); } 50% { transform: scale(1.4); } 100% { transform: scale(1); } }

.glitch-fade-enter-active, .glitch-fade-leave-active { transition: all 0.3s ease; }
.glitch-fade-enter-from, .glitch-fade-leave-to { opacity: 0; transform: scale(0.95); }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }

@media (max-width: 900px) {
  .cyber-terminal-window { height: 100%; border: none; max-width: 100%; }
  .terminal-body { flex-direction: column; overflow-y: auto; }
  .image-viewport { min-height: 40vh; flex: none; border-bottom: 4px solid var(--black); }
  .data-sidebar { width: 100%; height: auto; border-left: none; overflow: visible; }
  .viewport-hud { display: none; }
}
</style>