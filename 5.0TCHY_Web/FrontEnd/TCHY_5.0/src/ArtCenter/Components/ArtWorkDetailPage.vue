<template>
  <div class="cyber-artwork-page">
    <div class="grid-bg moving-grid"></div>

    <header class="page-header">
      <div class="nav-back" @click="handleBack">
        <span class="arrow"> &lt; </span> RETURN_TO_GALLERY
      </div>
      <div class="header-title">ASSET_VIEWER // NODE_ID: {{ route.params.id }}</div>
    </header>

    <div class="page-main-bridge">
      <main class="artwork-primary">
        <div v-if="loading" class="loading-state">
          <div class="glitch-text">SYNCING_VISUAL_DATA...</div>
        </div>
        
        <div v-else-if="artwork" class="artwork-display-container custom-scroll">
          <div class="image-viewport" @click="openLightbox">
            <div class="viewport-deco-tl"></div>
            <div class="viewport-deco-br"></div>
            
            <img :src="fixAvatarUrl(artwork.urlFull || artwork.url)" class="main-img" />
            
            <div class="hover-overlay">
              <span class="scan-icon">✛</span>
              <span class="scan-text">CLICK_TO_ANALYZE // 点击全屏解析</span>
            </div>

            <div class="viewport-hud">
              <span class="hud-text">RESOLUTION: HIGH_RES</span>
              <span class="hud-text">RENDER_STABLE: 100%</span>
            </div>
          </div>
          
          <div class="content-body">
            <div class="artwork-info-block">
              <h1 class="asset-title">{{ artwork.title || 'UNTITLED_ASSET' }}</h1>
              <p class="asset-desc">{{ artwork.desc || artwork.description || '暂无描述数据...' }}</p>
            </div>

            <div class="interaction-console">
              <div class="console-header">>> USER_INTERACTION_PROTOCOL</div>
              
              <div class="like-btn-wrapper">
                <UniversalLikeBtn 
                  v-if="artwork && artwork.id"
                  targetType="Drawing" 
                  :targetId="artwork.id" 
                  :initialCount="artwork.likes || 0" 
                />
              </div>
            </div>

            <div class="comment-zone">
              <div class="zone-deco-line"></div>
              
              <UniversalComments 
                v-if="artwork && artwork.id" 
                targetType="Drawing" 
                :targetId="artwork.id" 
              />
            </div>
          </div>
        </div>
      </main>

      <aside class="post-sidebar">
        <section class="author-identity-card" v-if="artwork">
          <div class="id-header">
            <span class="id-label">CREATOR_ID</span>
            <span class="id-serial">#{{ artwork.uploaderId?.toString().padStart(4, '0') || '0000' }}</span>
          </div>
          <div class="id-main">
            <div class="avatar-container">
              <UserAvatar 
                :user-id="artwork.uploaderId" 
                :passed-avatar="artwork.userAvatar"
                :show-level="true"
              />
            </div>
            <div class="name-zone">
              <div class="username">@{{ artwork.userName || 'UNKNOWN' }}</div>
              <div class="user-status"><span class="dot"></span> CREATOR_ONLINE</div>
            </div>
          </div>
          <div class="id-actions">
            <button class="id-btn follow">FOLLOW_CREATOR</button>
            <button class="id-btn share" @click="handleShare">
                {{ shareStatus === 'READY' ? 'SHARE_ASSET' : 'LINK_COPIED!' }}
            </button>
          </div>
        </section>

        <div class="cyber-terminal-mini" v-if="artwork">
          <div class="terminal-header">> ASSET_METRICS.log</div>
          <div class="metric"><span class="lab">VIEWS:</span> {{ artwork?.views || artwork?.viewCount || 0 }}</div>
          <div class="metric"><span class="lab">LIKES:</span> {{ artwork?.likes || artwork?.likeCount || 0 }}</div>
          <div class="metric"><span class="lab">LINK:</span> <span class="green">ENCRYPTED</span></div>
        </div>
      </aside>
    </div>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="isLightboxOpen" class="lightbox-overlay" @click="closeLightbox">
          <div class="lightbox-hud-top">
            <span class="hud-tag">:: VISUAL_ANALYSIS_MODE ::</span>
            <span class="hud-tag">[ ESC ] TO EXIT</span>
          </div>

          <div 
            class="lightbox-img-wrapper" 
            :class="{ 'is-zoomed': isZoomed }"
            @click.stop="toggleZoom"
            @mousemove="handleMouseMove"
          >
            <img 
              :src="fixAvatarUrl(artwork.urlFull || artwork.url)" 
              class="lightbox-img" 
              :style="zoomStyle"
            />
          </div>

          <div class="lightbox-hud-bottom">
            <span>ZOOM_LEVEL: {{ isZoomed ? '250%' : 'FIT_SCREEN' }}</span>
            <span v-if="!isZoomed" class="blink">>> CLICK TO ZOOM</span>
            <span v-else class="blink">>> MOVE MOUSE TO PAN</span>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, onMounted, computed, reactive, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/utils/api';

// ✅ 引入两大通用组件
import UniversalComments from '@/GeneralComponents/UniversalComments.vue'; 
import UniversalLikeBtn from '@/GeneralComponents/UniversalLikeBtn.vue';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'; 

const route = useRoute();
const router = useRouter();
const artwork = ref(null);
const loading = ref(true);
const shareStatus = ref('READY');

// --- Lightbox 状态 ---
const isLightboxOpen = ref(false);
const isZoomed = ref(false);
const mousePos = reactive({ x: 0, y: 0 }); 

const BASE_URL = window.location.hostname === 'localhost' ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const fixAvatarUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

const handleBack = () => {
  if (window.history.length > 1) { router.back(); } else { router.push('/gallery'); }
};

const fetchArtwork = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get(`/Drawing/${route.params.id}`);
    if (res.data.success) {
      artwork.value = res.data.data;
    } else if (res.data.id) {
       artwork.value = res.data;
    }
  } catch (e) { console.error("Fetch failed", e); } 
  finally { loading.value = false; }
};

// --- Lightbox 逻辑 ---
const openLightbox = () => {
  isLightboxOpen.value = true;
  document.body.style.overflow = 'hidden'; 
  window.addEventListener('keydown', handleKeydown);
};

const closeLightbox = () => {
  isLightboxOpen.value = false;
  isZoomed.value = false;
  document.body.style.overflow = ''; 
  window.removeEventListener('keydown', handleKeydown);
};

const handleKeydown = (e) => {
  if (e.key === 'Escape') closeLightbox();
};

const toggleZoom = () => {
  isZoomed.value = !isZoomed.value;
};

const handleMouseMove = (e) => {
  if (!isZoomed.value) return;
  mousePos.x = (e.clientX / window.innerWidth) * 100;
  mousePos.y = (e.clientY / window.innerHeight) * 100;
};

const zoomStyle = computed(() => {
  if (!isZoomed.value) return {};
  return {
    transformOrigin: `${mousePos.x}% ${mousePos.y}%`
  };
});

// --- 分享逻辑 ---
const handleShare = async () => {
  try {
    await navigator.clipboard.writeText(window.location.href);
    shareStatus.value = 'COPIED!';
    setTimeout(() => shareStatus.value = 'READY', 2000);
  } catch(e) { shareStatus.value = 'ERR'; }
};

onMounted(fetchArtwork);
onUnmounted(() => {
  document.body.style.overflow = '';
  window.removeEventListener('keydown', handleKeydown);
});
</script>

<style scoped>
/* 保持原有赛博风格样式 */
.cyber-artwork-page {
  --red: #D92323; --black: #111111; --off-white: #F4F1EA;
  width: 100%; height: 100vh; background: var(--off-white);
  display: flex; flex-direction: column; overflow: hidden; position: relative;
}

.page-main-bridge {
  flex: 1; display: flex; gap: 30px; padding: 30px;
  max-width: 1600px; margin: 0 auto; width: 100%; box-sizing: border-box; overflow: hidden;
}

.artwork-primary {
  flex: 1; background: var(--black); 
  border: 2px solid var(--black); position: relative; overflow: hidden;
  display: flex; flex-direction: column; min-width: 0;
}

.artwork-display-container {
  flex: 1; overflow-y: auto; display: flex; flex-direction: column;
}

/* --- 主视口 --- */
.image-viewport {
  position: relative; padding: 40px; display: flex; justify-content: center; align-items: center;
  min-height: 60vh; background: radial-gradient(circle, #222 0%, #000 100%);
  border-bottom: 4px solid var(--red);
  flex-shrink: 0;
  cursor: zoom-in; overflow: hidden;
}

.hover-overlay {
  position: absolute; inset: 0; background: rgba(217, 35, 35, 0.1);
  display: flex; flex-direction: column; justify-content: center; align-items: center;
  opacity: 0; transition: opacity 0.3s; pointer-events: none; z-index: 10;
}
.image-viewport:hover .hover-overlay { opacity: 1; }
.scan-icon { font-size: 3rem; color: var(--red); text-shadow: 0 0 10px var(--red); margin-bottom: 10px; }
.scan-text { font-family: 'JetBrains Mono'; color: #fff; background: var(--black); padding: 5px 10px; font-size: 0.8rem; }

.main-img { 
  max-width: 100%; max-height: 80vh; object-fit: contain; 
  box-shadow: 0 0 30px rgba(0,0,0,0.8); border: 1px solid #333; 
  transition: transform 0.3s;
}
.image-viewport:hover .main-img { transform: scale(1.02); }

.viewport-hud {
  position: absolute; bottom: 10px; right: 10px; display: flex; gap: 15px; z-index: 11;
}
.hud-text { font-family: 'JetBrains Mono'; font-size: 0.7rem; color: #666; }

/* --- 交互与内容 --- */
.content-body { background: #fff; flex: 1; display: flex; flex-direction: column; }
.artwork-info-block { padding: 30px 40px; color: #000; }
.asset-title { font-family: 'Anton'; font-size: 2.5rem; margin: 0 0 10px 0; letter-spacing: 1px; line-height: 1.1; }
.asset-desc { font-family: 'JetBrains Mono'; color: #555; line-height: 1.6; font-size: 1rem; max-width: 800px; }

.interaction-console { padding: 0 40px 30px; }
.console-header { font-family: 'JetBrains Mono'; font-size: 0.75rem; color: #999; margin-bottom: 10px; border-bottom: 1px dashed #ccc; padding-bottom: 5px; width: fit-content; }

/* 评论区样式 */
.comment-zone { padding: 0 40px 60px; background: #fafafa; border-top: 2px solid #eee; flex: 1; }
.zone-deco-line { width: 40px; height: 4px; background: var(--black); margin-bottom: 20px; }

/* 侧边栏 */
.post-sidebar { width: 350px; display: flex; flex-direction: column; gap: 20px; }
.author-identity-card { background: var(--black); color: #fff; padding: 20px; border: 1px solid #444; clip-path: polygon(0 0, 100% 0, 100% calc(100% - 20px), calc(100% - 20px) 100%, 0 100%); }
.id-header { display: flex; justify-content: space-between; border-bottom: 1px solid #333; padding-bottom: 10px; margin-bottom: 15px; font-family: 'JetBrains Mono'; font-size: 0.7rem; }
.id-label { color: var(--red); font-weight: bold; }
.id-main { display: flex; gap: 15px; align-items: center; }
.avatar-container { width: 65px; height: 65px; flex-shrink: 0; position: relative; }
.name-zone { flex: 1; }
.username { font-weight: 900; font-size: 1.1rem; color: #fff; }
.user-status { font-family: 'JetBrains Mono'; font-size: 0.6rem; color: #0f0; display: flex; align-items: center; gap: 5px; margin-top: 4px; }
.dot { width: 6px; height: 6px; background: #0f0; border-radius: 50%; animation: pulse 1s infinite; }
.id-actions { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-top: 20px; }
.id-btn { border: 1px solid #444; background: transparent; color: #fff; padding: 8px; font-family: 'JetBrains Mono'; font-size: 0.7rem; cursor: pointer; transition: 0.2s; }
.id-btn.follow { background: var(--red); border-color: var(--red); font-weight: bold; }
.id-btn:hover { background: #fff; color: #000; }
.cyber-terminal-mini { background: var(--black); color: #0f0; padding: 20px; font-family: 'JetBrains Mono'; border-radius: 4px; font-size: 0.8rem; border-left: 4px solid var(--red); }
.terminal-header { border-bottom: 1px dashed #333; margin-bottom: 15px; color: #fff; opacity: 0.7; }
.metric { margin-bottom: 8px; display: flex; justify-content: space-between; }
.metric .lab { color: #666; }
.green { color: #0f0; }

/* --- Lightbox Styles --- */
.lightbox-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.95); z-index: 9999; display: flex; flex-direction: column; justify-content: center; align-items: center; cursor: zoom-out; }
.lightbox-img-wrapper { width: 100%; height: 100%; display: flex; justify-content: center; align-items: center; transition: transform 0.2s; cursor: zoom-in; }
.lightbox-img { max-width: 95vw; max-height: 90vh; object-fit: contain; transition: transform 0.1s linear; box-shadow: 0 0 50px rgba(217, 35, 35, 0.2); }
.lightbox-img-wrapper.is-zoomed { cursor: move; }
.lightbox-img-wrapper.is-zoomed .lightbox-img { max-width: none; max-height: none; width: 100vw; transform: scale(2.5); }
.lightbox-hud-top, .lightbox-hud-bottom { position: absolute; left: 0; right: 0; padding: 20px; display: flex; justify-content: space-between; pointer-events: none; font-family: 'JetBrains Mono'; color: #fff; text-shadow: 0 0 5px #fff; font-size: 0.8rem; z-index: 10000; }
.lightbox-hud-top { top: 0; border-bottom: 1px solid rgba(255,255,255,0.1); background: linear-gradient(to bottom, rgba(0,0,0,0.8), transparent); }
.lightbox-hud-bottom { bottom: 0; border-top: 1px solid rgba(255,255,255,0.1); background: linear-gradient(to top, rgba(0,0,0,0.8), transparent); }
.blink { animation: pulse 1s infinite; }

.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(0,0,0,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(0,0,0,0.05) 1px, transparent 1px); background-size: 40px 40px; pointer-events: none; }
.page-header { height: 60px; background: var(--black); color: #fff; display: flex; align-items: center; padding: 0 40px; gap: 30px; border-bottom: 4px solid var(--red); flex-shrink: 0; }
.nav-back { cursor: pointer; font-family: 'JetBrains Mono'; font-weight: bold; }
.nav-back:hover { color: var(--red); }
.header-title { font-family: 'JetBrains Mono'; opacity: 0.7; font-size: 0.9rem; }
.loading-state { flex: 1; display: flex; align-items: center; justify-content: center; background: #000; color: #fff; }
.glitch-text { font-family: 'JetBrains Mono'; animation: pulse 0.2s infinite; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

@keyframes pulse { 50% { opacity: 0.5; } }
</style>