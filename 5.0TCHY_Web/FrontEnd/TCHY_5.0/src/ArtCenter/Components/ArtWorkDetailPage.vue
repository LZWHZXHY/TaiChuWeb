<template>
  <div class="modern-artwork-page custom-scroll">
    
    <nav class="modern-nav-bar">
      <div class="nav-container">
        <button class="back-btn" @click="handleBack">
          <svg viewBox="0 0 24 24" width="20" height="20" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"><line x1="19" y1="12" x2="5" y2="12"></line><polyline points="12 19 5 12 12 5"></polyline></svg>
          返回画廊
        </button>
        <span class="nav-title" v-if="artwork">VISUAL ASSET // {{ route.params.id }}</span>
      </div>
    </nav>

    <div v-if="loading" class="status-screen">
      <div class="loading-spinner"></div>
      <div class="loading-text">正在同步高质量视觉数据...</div>
    </div>

    <div v-else-if="artwork" class="page-layout-grid">
      
      <main class="main-content-column">
        <article class="modern-card artwork-showcase">
          
          <div class="image-viewport zoomable" @click="openLightbox">
            <img :src="fixAvatarUrl(artwork.urlFull || artwork.url)" class="main-img" @error="handleImgError" alt="Artwork" />
            <div class="hover-overlay">
              <svg viewBox="0 0 24 24" width="48" height="48" stroke="currentColor" stroke-width="1.5" fill="none"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line><line x1="11" y1="8" x2="11" y2="14"></line><line x1="8" y1="11" x2="14" y2="11"></line></svg>
              <span class="overlay-text">点击全屏解析细节</span>
            </div>
          </div>
          
          <div class="content-body">
            
            <div class="action-bar">
              <div class="action-group-left">
                <div class="action-item">
                  <UniversalLikeBtn 
                    v-if="artwork && artwork.id"
                    targetType="Drawing" 
                    :targetId="artwork.id" 
                    :initialCount="artwork.likes || 0" 
                  />
                </div>
                <button 
                  v-if="artwork && artwork.id"
                  class="modern-action-btn star-btn" 
                  @click="showArchiveModal = true"
                >
                  <span class="btn-icon">★</span>
                  <span class="btn-text">收藏 [{{ artwork.favoriteCount || artwork.starCount || 0 }}]</span>
                </button>
              </div>
              <div class="action-group-right">
                <button class="modern-action-btn share-btn" @click="handleShare" :class="{ 'success-state': shareStatus === 'COPIED!' }">
                  <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2" fill="none"><circle cx="18" cy="5" r="3"></circle><circle cx="6" cy="12" r="3"></circle><circle cx="18" cy="19" r="3"></circle><line x1="8.59" y1="13.51" x2="15.42" y2="17.49"></line><line x1="15.41" y1="6.51" x2="8.59" y2="10.49"></line></svg>
                  {{ shareStatus === 'READY' ? '分享' : '已复制' }}
                </button>
              </div>
            </div>

            <div class="artwork-info-block">
              <h1 class="asset-title">{{ artwork.title || '无题作品' }}</h1>
              <p class="asset-desc">{{ artwork.desc || artwork.description || '创造者暂未留下相关描述...' }}</p>
            </div>

          </div>
        </article>

        <section class="comments-section">
          <h3 class="section-title">参与鉴赏</h3>
          <div class="modern-card comments-card">
            <UniversalComments 
              v-if="artwork && artwork.id" 
              targetType="Drawing" 
              :targetId="artwork.id" 
            />
          </div>
        </section>
      </main>

      <aside class="sidebar-column">
        
        <section class="modern-card sidebar-widget author-identity">
          <div class="widget-header">
            <span class="widget-label">CREATOR PROFILE</span>
            <span class="widget-id">UID: #{{ artwork.uploaderId?.toString().padStart(4, '0') || '0000' }}</span>
          </div>
          <div class="identity-main">
            <div class="avatar-large">
              <UserAvatar 
                :user-id="artwork.uploaderId" 
                :passed-avatar="artwork.userAvatar"
                :show-level="true"
              />
            </div>
            <div class="identity-info">
              <div class="username hover-link" @click="router.push(`/profile/${artwork.uploaderId}`)">
                @{{ artwork.userName || '未知艺术家' }}
              </div>
              <div class="user-status"><span class="status-dot"></span> 艺术家在线</div>
            </div>
          </div>
          <div class="action-grid">
            <button class="modern-btn primary full-width">关注创造者</button>
          </div>
        </section>

        <section class="modern-card sidebar-widget metrics-board">
          <div class="widget-header">
            <span class="widget-label">ASSET METRICS</span>
          </div>
          <div class="metrics-grid">
            <div class="metric-box">
              <span class="m-val">{{ artwork?.views || artwork?.viewCount || 0 }}</span>
              <span class="m-lab">浏览量</span>
            </div>
            <div class="metric-box">
              <span class="m-val">{{ artwork?.likes || artwork?.likeCount || 0 }}</span>
              <span class="m-lab">获赞数</span>
            </div>
            <div class="metric-box full-span">
              <span class="m-val">{{ artwork?.favoriteCount || artwork?.starCount || 0 }}</span>
              <span class="m-lab">总收藏</span>
            </div>
          </div>
        </section>
      </aside>

    </div>

    <Teleport to="body">
      
      <ArchiveModal 
        v-if="artwork && artwork.id"
        v-model="showArchiveModal" 
        :target-id="artwork.id" 
        :category="0" 
        @success="handleArchiveSuccess"
      />

      <Transition name="fade">
        <div v-if="isLightboxOpen" class="sleek-lightbox-overlay" @click="closeLightbox">
          
          <div class="lightbox-top-bar">
            <span class="hud-tag">HIGH RESOLUTION VIEWER</span>
            <button class="icon-close-btn" @click.stop="closeLightbox">
              <svg viewBox="0 0 24 24" width="28" height="28" stroke="currentColor" stroke-width="2" fill="none"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
            </button>
          </div>

          <div class="lightbox-img-wrapper">
            <img 
              :src="fixAvatarUrl(artwork.urlFull || artwork.url)" 
              class="lightbox-img" 
              alt="High Res Artwork"
              @click.stop
            />
          </div>

          <div class="lightbox-bottom-bar">
            <span class="zoom-status">RESOLUTION: ORIGINAL</span>
            <span class="hint-text">>> 单击空白处关闭</span>
          </div>
          
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/utils/api';

import UniversalComments from '@/GeneralComponents/UniversalComments.vue'; 
import UniversalLikeBtn from '@/GeneralComponents/UniversalLikeBtn.vue';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue'; 
import ArchiveModal from '@/GeneralComponents/ArchiveModal.vue'; 

const route = useRoute();
const router = useRouter();
const artwork = ref(null);
const loading = ref(true);
const shareStatus = ref('READY');

const showArchiveModal = ref(false);
const handleArchiveSuccess = () => {
  if (artwork.value) {
    artwork.value.favoriteCount = (artwork.value.favoriteCount || 0) + 1;
  }
};

// --- 精简后的 Lightbox 状态 ---
const isLightboxOpen = ref(false);

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

const openLightbox = () => {
  isLightboxOpen.value = true;
  document.body.style.overflow = 'hidden'; 
  window.addEventListener('keydown', handleKeydown);
};

const closeLightbox = () => {
  isLightboxOpen.value = false;
  document.body.style.overflow = ''; 
  window.removeEventListener('keydown', handleKeydown);
};

const handleKeydown = (e) => {
  if (e.key === 'Escape') closeLightbox();
};

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
.modern-artwork-page {
  --bg-color: #f3f4f6; 
  --card-bg: #ffffff;
  --text-main: #111827;
  --text-muted: #6b7280;
  --border-color: #e5e7eb;
  --accent-color: #3b82f6; 
  --star-color: #f59e0b; 
  
  background-color: var(--bg-color);
  min-height: 100vh;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: var(--text-main);
}

.modern-nav-bar {
  position: sticky; top: 0; z-index: 100;
  height: 60px; background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(12px); border-bottom: 1px solid var(--border-color);
}
.nav-container {
  max-width: 1400px; margin: 0 auto; height: 100%;
  display: flex; align-items: center; padding: 0 20px; gap: 20px;
}
.back-btn {
  background: transparent; border: none; color: var(--text-muted);
  font-size: 0.95rem; font-weight: 600; cursor: pointer; 
  display: flex; align-items: center; gap: 6px; padding: 0; transition: color 0.2s;
}
.back-btn:hover { color: var(--text-main); }
.nav-title { color: var(--text-muted); font-size: 0.85rem; font-family: 'JetBrains Mono', monospace; border-left: 1px solid var(--border-color); padding-left: 20px; }

.page-layout-grid {
  max-width: 1400px; margin: 40px auto; padding: 0 20px;
  display: grid; grid-template-columns: 1fr 340px; gap: 30px; align-items: start;
}
.main-content-column { display: flex; flex-direction: column; gap: 30px; min-width: 0; }
.sidebar-column { display: flex; flex-direction: column; gap: 20px; position: sticky; top: 80px; }

.modern-card {
  background: var(--card-bg); border-radius: 16px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
  border: 1px solid var(--border-color); overflow: hidden;
}

.artwork-showcase { display: flex; flex-direction: column; }

.image-viewport {
  position: relative; 
  background: #e5e7eb; 
  background-image: repeating-linear-gradient(45deg, #f3f4f6 25%, transparent 25%, transparent 75%, #f3f4f6 75%, #f3f4f6), repeating-linear-gradient(45deg, #f3f4f6 25%, #e5e7eb 25%, #e5e7eb 75%, #f3f4f6 75%, #f3f4f6);
  background-position: 0 0, 10px 10px; background-size: 20px 20px;
  display: flex; justify-content: center; align-items: center;
  min-height: 50vh; max-height: 75vh;
  cursor: zoom-in; overflow: hidden; border-bottom: 1px solid var(--border-color);
}
.main-img { 
  max-width: 100%; max-height: 75vh; object-fit: contain; 
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

.hover-overlay {
  position: absolute; inset: 0; background: rgba(0, 0, 0, 0.3);
  display: flex; flex-direction: column; justify-content: center; align-items: center;
  opacity: 0; transition: opacity 0.3s; pointer-events: none; color: #fff;
}
.image-viewport:hover .hover-overlay { opacity: 1; }
.image-viewport:hover .main-img { transform: scale(1.02); }
.overlay-text { margin-top: 12px; font-weight: 600; letter-spacing: 1px; font-size: 0.95rem; }

.content-body { display: flex; flex-direction: column; }

.action-bar {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 32px; border-bottom: 1px solid var(--border-color);
  background: #f9fafb;
}
.action-group-left, .action-group-right { display: flex; align-items: center; gap: 12px; }

.modern-action-btn {
  background: var(--card-bg); border: 1px solid var(--border-color);
  color: var(--text-main); padding: 8px 16px; border-radius: 8px;
  font-weight: 600; font-size: 0.9rem; cursor: pointer;
  display: flex; align-items: center; gap: 8px;
  transition: all 0.2s; box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
.modern-action-btn:hover { background: #f3f4f6; border-color: #d1d5db; transform: translateY(-1px); }

.star-btn .btn-icon { font-size: 1.1rem; color: #9ca3af; transition: color 0.2s; }
.star-btn:hover .btn-icon { color: var(--star-color); }
.star-btn:hover { border-color: var(--star-color); color: var(--star-color); }
.share-btn.success-state { color: #10b981; border-color: #10b981; background: #ecfdf5; }

.artwork-info-block { padding: 32px; }
.asset-title { font-size: 2rem; font-weight: 800; margin: 0 0 16px 0; color: var(--text-main); line-height: 1.3; }
.asset-desc { font-size: 1.05rem; line-height: 1.7; color: var(--text-muted); white-space: pre-wrap; }

.comments-section { display: flex; flex-direction: column; gap: 16px; }
.section-title { font-size: 1.2rem; font-weight: 700; color: var(--text-main); padding-left: 8px; }
.comments-card { padding: 24px; }

.sidebar-widget { padding: 24px; }
.widget-header { border-bottom: 1px solid var(--border-color); padding-bottom: 12px; margin-bottom: 20px; display: flex; justify-content: space-between; align-items: center; }
.widget-label { font-size: 0.85rem; font-weight: 700; color: var(--text-muted); letter-spacing: 0.5px; }
.widget-id { font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; color: #9ca3af; }

.identity-main { display: flex; gap: 16px; align-items: center; margin-bottom: 24px; }
.avatar-large { width: 64px; height: 64px; border-radius: 50%; }
.username { font-weight: 800; font-size: 1.15rem; color: var(--text-main); cursor: pointer; transition: color 0.2s; }
.username.hover-link:hover { color: var(--accent-color); }
.user-status { font-size: 0.8rem; color: #10b981; display: flex; align-items: center; gap: 6px; margin-top: 4px; font-weight: 500; }
.status-dot { width: 8px; height: 8px; background: #10b981; border-radius: 50%; box-shadow: 0 0 8px rgba(16, 185, 129, 0.4); animation: pulse 2s infinite; }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

.modern-btn { padding: 10px 16px; border-radius: 8px; font-size: 0.95rem; font-weight: 600; cursor: pointer; transition: all 0.2s; text-align: center; border: 1px solid transparent; }
.modern-btn.primary { background: var(--text-main); color: #fff; }
.modern-btn.primary:hover { background: #374151; }
.full-width { width: 100%; }

.metrics-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.metric-box { background: #f9fafb; border: 1px solid var(--border-color); border-radius: 8px; padding: 16px 12px; text-align: center; display: flex; flex-direction: column; gap: 6px; }
.metric-box.full-span { grid-column: span 2; }
.m-val { font-size: 1.5rem; font-weight: 800; color: var(--text-main); font-family: 'JetBrains Mono', monospace; }
.m-lab { font-size: 0.8rem; color: var(--text-muted); font-weight: 600; }

.sleek-lightbox-overlay {
  position: fixed; inset: 0; z-index: 9999;
  background: rgba(0, 0, 0, 0.9); backdrop-filter: blur(15px);
  display: flex; flex-direction: column; justify-content: center; align-items: center;
  cursor: zoom-out;
}

.lightbox-top-bar {
  position: absolute; top: 0; left: 0; right: 0; padding: 20px 30px;
  display: flex; justify-content: space-between; align-items: center; z-index: 10000;
  background: linear-gradient(to bottom, rgba(0,0,0,0.6), transparent); pointer-events: none;
}
.lightbox-bottom-bar {
  position: absolute; bottom: 0; left: 0; right: 0; padding: 20px 30px;
  display: flex; justify-content: space-between; align-items: center; z-index: 10000;
  background: linear-gradient(to top, rgba(0,0,0,0.6), transparent); pointer-events: none;
}

.hud-tag, .zoom-status, .hint-text {
  color: #fff; font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; letter-spacing: 1px; opacity: 0.8;
}
.hint-text { opacity: 0.6; }

.icon-close-btn {
  background: rgba(255,255,255,0.1); border: none; color: #fff; border-radius: 50%;
  width: 44px; height: 44px; display: flex; align-items: center; justify-content: center;
  cursor: pointer; pointer-events: auto; transition: all 0.2s;
}
.icon-close-btn:hover { background: rgba(255,255,255,0.25); transform: scale(1.1); }

.lightbox-img-wrapper {
  width: 100%; height: 100%; display: flex; justify-content: center; align-items: center;
}
.lightbox-img {
  max-width: 95vw; max-height: 85vh; object-fit: contain;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.8); border-radius: 4px;
}

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

.status-screen { height: 70vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: var(--text-muted); }
.loading-spinner { width: 40px; height: 40px; border: 3px solid var(--border-color); border-top-color: var(--accent-color); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 20px; }
.loading-text { font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; }
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1024px) {
  .page-layout-grid { grid-template-columns: 1fr; margin-top: 20px; }
  .sidebar-column { position: relative; top: 0; }
}
@media (max-width: 768px) {
  .modern-artwork-page { background: var(--card-bg); }
  .page-layout-grid { padding: 0; margin: 0; gap: 0; }
  .modern-card { border-radius: 0; border-left: none; border-right: none; box-shadow: none; border-top: none; }
  .artwork-info-block, .action-bar { padding-left: 20px; padding-right: 20px; }
  .sidebar-widget { padding: 20px; border-bottom: 8px solid var(--bg-color); }
  .image-viewport { min-height: 40vh; }
  .action-bar { flex-direction: column; align-items: stretch; gap: 16px; }
  .action-group-right { justify-content: flex-end; }
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #d1d5db; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #9ca3af; }




</style>