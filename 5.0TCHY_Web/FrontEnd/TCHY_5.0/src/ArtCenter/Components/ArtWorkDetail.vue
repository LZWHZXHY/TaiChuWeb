<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="visible" class="lightbox-overlay" @click="handleClose">
        
        <div class="lightbox-container" @click.stop>
          
          <div class="image-area" @click="handleClose">
            <img 
              :src="upgradeUrlToHttps(artwork.imageUrlFull || artwork.url)" 
              class="lightbox-img" 
              @click.stop 
            />
            
            <button 
              class="floating-like-btn"
              :class="{ 'is-liked': artwork.isLiked }"
              @click.stop="onLike"
            >
              <span :class="{ 'heart-bounce': artwork.isAnimating }">
                <i class="fas fa-heart"></i>
              </span>
            </button>
          </div>

          <div class="sidebar-area">
            
            <div class="artwork-header">
              <div class="author-info">
                <div class="avatar-placeholder">{{ (artwork.authorName || 'A')[0] }}</div>
                <div class="author-text">
                  <span class="name">@{{ artwork.authorName || artwork.author }}</span>
                  <span class="time">{{ formatTime(artwork.uploadAt) }}</span>
                </div>
              </div>
              <button class="close-btn-mobile" @click="handleClose"><i class="fas fa-times"></i></button>
            </div>

            <div class="artwork-scroll-content custom-scroll">
              
              <div class="artwork-details">
                <h3 class="title">{{ artwork.title }}</h3>
                <p class="desc" v-if="artwork.desc">{{ artwork.desc }}</p>
                
                <div class="action-bar-left">
                  <button 
                    class="sidebar-like-btn" 
                    :class="{ 'is-liked': artwork.isLiked }" 
                    @click="onLike"
                  >
                    <span :class="{ 'heart-bounce': artwork.isAnimating }" class="icon-wrap">
                      <i class="fas fa-heart"></i>
                    </span>
                    <span class="label">{{ artwork.likes || 0 }} 喜欢</span>
                  </button>
                </div>
              </div>

              <div class="comment-wrapper">
                <CommentSection :artworkId="artwork.id" />
              </div>
            </div>

          </div>
        </div>

        <button class="lightbox-close-desktop" @click="handleClose">
          <i class="fas fa-times"></i>
        </button>

      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
// 请根据你实际存放 CommentSection 的路径修改引用
// 如果是在 src/CommentSection 下，则是 '@/CommentSection/CommentSection.vue'
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

const formatTime = (t) => t ? new Date(t).toLocaleDateString() : ''
</script>

<style scoped>
/* === 基础布局 (保持不变) === */
.lightbox-overlay { 
  position: fixed; inset: 0; background: rgba(0,0,0,0.9); z-index: 1000; 
  display: flex; justify-content: center; align-items: center; 
}
.lightbox-container { 
  display: flex; width: 90vw; height: 90vh; max-width: 1400px; 
  background: #fff; border-radius: 12px; overflow: hidden; 
  box-shadow: 0 20px 50px rgba(0,0,0,0.5); 
}
.image-area { 
  flex: 1; background: #080808; display: flex; 
  justify-content: center; align-items: center; 
  overflow: hidden; position: relative; cursor: pointer; 
}
.lightbox-img { max-width: 100%; max-height: 100%; object-fit: contain; }

/* === 侧边栏 === */
.sidebar-area {
  width: 340px; /* 稍微窄一点，更精致 */
  background: #fff; display: flex; 
  flex-direction: column; border-left: 1px solid #f0f0f0; 
  flex-shrink: 0; overflow: hidden;
}
.artwork-header {
  flex-shrink: 0; display: flex; justify-content: space-between; 
  align-items: center; padding: 20px 24px; border-bottom: 1px solid #f8f9fa;
}
.artwork-scroll-content {
  flex: 1; overflow-y: auto; padding: 24px; 
  display: flex; flex-direction: column;
}
.artwork-details { margin-bottom: 24px; }
.artwork-details .title { margin: 0 0 8px 0; font-size: 18px; color: #1a1a1a; font-weight: 800; letter-spacing: -0.5px; }
.artwork-details .desc { font-size: 13px; color: #666; line-height: 1.7; margin-bottom: 20px; }
.comment-wrapper { flex: 1; min-height: 200px; display: flex; flex-direction: column; }

/* === 动画定义：高级贝塞尔曲线 === */
@keyframes heartElastic {
  0% { transform: scale(1); }
  30% { transform: scale(1.35); } /* 蓄力 */
  50% { transform: scale(0.9); }  /* 回弹 */
  70% { transform: scale(1.1); }  /* 余震 */
  100% { transform: scale(1); }
}

@keyframes gradientFlow {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

@keyframes rippleEffect {
  0% { box-shadow: 0 0 0 0 rgba(255, 65, 108, 0.4); }
  100% { box-shadow: 0 0 0 15px rgba(255, 65, 108, 0); }
}

.heart-bounce {
  display: inline-block;
  animation: heartElastic 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

/* ========================================= */
/* === [核心修改] 按钮样式优化 === */
/* ========================================= */

/* 1. 侧边栏按钮：流体渐变 + 精致尺寸 */
.sidebar-like-btn {
  position: relative;
  width: 100%; 
  height: 40px; /* 减小高度，更精致 */
  border-radius: 20px;
  border: 1px solid #e2e8f0; 
  background: #fff; 
  color: #64748b;
  font-size: 13px; font-weight: 600; 
  cursor: pointer;
  display: flex; align-items: center; justify-content: center; gap: 8px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden; /* 必须有，为了流光 */
  z-index: 1;
}

.sidebar-like-btn:hover {
  border-color: #cbd5e1;
  background: #f8fafc;
  transform: translateY(-1px);
}

.sidebar-like-btn:active {
  transform: scale(0.98);
}

/* 激活态：流光渐变 */
.sidebar-like-btn.is-liked {
  border: none;
  color: #fff;
  /* 300% 宽度的背景，实现流动感 */
  background: linear-gradient(90deg, #ff416c, #ff4b2b, #ff416c);
  background-size: 300% 100%;
  animation: gradientFlow 3s ease infinite; /* 永久流动的背景 */
}

/* 激活瞬间的涟漪效果 (伪元素) */
.sidebar-like-btn.is-liked::after {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: 20px;
  animation: rippleEffect 0.6s ease-out;
  z-index: -1;
}

/* 2. 左侧悬浮按钮：极简玻璃态 */
.floating-like-btn {
  position: absolute; bottom: 25px; right: 25px;
  width: 48px; height: 48px; /* 缩小尺寸 */
  border-radius: 50%;
  
  /* 高级磨砂玻璃 */
  background: rgba(255, 255, 255, 0.1); 
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.25);
  
  color: #fff;
  font-size: 20px; 
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); /* 弹性过渡 */
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
}

.floating-like-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: scale(1.1) rotate(5deg); /* 悬浮时轻微旋转放大 */
}

.floating-like-btn:active {
  transform: scale(0.9);
}

.floating-like-btn.is-liked {
  background: #ff4757; /* 鲜艳的西瓜红 */
  border-color: #ff4757;
  box-shadow: 0 10px 25px rgba(255, 71, 87, 0.5); /* 红色辉光 */
}

/* ========================================= */

/* 其他辅助样式 */
.author-info { display: flex; align-items: center; gap: 12px; }
.avatar-placeholder { width: 40px; height: 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #fff; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 16px; box-shadow: 0 4px 10px rgba(118, 75, 162, 0.3); }
.author-text { display: flex; flex-direction: column; justify-content: center; }
.author-text .name { font-weight: 700; font-size: 14px; color: #1a1a1a; margin-bottom: 2px; }
.author-text .time { font-size: 11px; color: #94a3b8; }
.lightbox-close-desktop { position: absolute; top: 20px; right: 20px; background: none; border: none; color: rgba(255,255,255,0.8); font-size: 30px; cursor: pointer; transition: 0.2s; }
.lightbox-close-desktop:hover { color: #fff; transform: rotate(90deg); }
.close-btn-mobile { display: none; }
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 4px; }
.custom-scroll::-webkit-scrollbar-track { background: transparent; }

@media screen and (max-width: 900px) {
  .lightbox-container { flex-direction: column; width: 100%; height: 100%; border-radius: 0; }
  .image-area { flex: 0 0 45%; }
  .sidebar-area { width: 100%; flex: 1; border-left: none; }
  .lightbox-close-desktop { display: none; }
  .close-btn-mobile { display: block; background: none; border: none; font-size: 22px; color: #666; }
  .floating-like-btn { bottom: 20px; right: 20px; width: 44px; height: 44px; font-size: 18px; }
}
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>