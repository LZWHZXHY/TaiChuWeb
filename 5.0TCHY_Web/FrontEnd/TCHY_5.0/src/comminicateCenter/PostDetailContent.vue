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

      <section class="universal-comments-container">
        <UniversalComments 
          v-if="data && data.id" 
          targetType="Post" 
          :targetId="data.id" 
        />
      </section>

    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue';
import { marked } from 'marked';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';
// ✅ 引入通用评论组件
import UniversalComments from '@/GeneralComponents/UniversalComments.vue';

const props = defineProps({
  data: { type: Object, required: true },
  fixAvatar: { type: Function, required: true }
});

const formatTime = (t) => t ? new Date(t).toLocaleString() : 'N/A';
const renderMarkdown = (c) => c ? marked.parse(c) : '';
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

// ❌ 删除了所有 fetchComments, submitLocalComment 等旧逻辑
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

/* 头像框 */
.cyber-avatar-frame {
  position: relative;
  border: 2px solid #D92323;
  background: #000;
  padding: 2px;
  flex-shrink: 0;
  z-index: 10;
}
.main-size { width: 55px; height: 55px; }
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

.end-bar { display: flex; align-items: center; gap: 20px; margin-top: 60px; opacity: 0.5; font-family: 'JetBrains Mono'; font-size: 0.8rem; }
.line { flex: 1; height: 1px; background: #000; }

/* --- 4. 评论区容器 --- */
.universal-comments-container {
  margin: 60px 40px 0 40px;
}

@keyframes scan { 0% { top: 0; } 100% { top: 100%; } }
@keyframes blink { 50% { opacity: 0.3; } }
</style>