<template>
  <article class="post-item" @click="viewPost" role="article" :aria-label="titleText" tabindex="0">
    <div class="media" aria-hidden="true">
      <template v-if="displayImages.length">
        <div class="media-img" :style="{ backgroundImage: `url(${getImageUrl(displayImages[0])})` }"></div>
        <div v-if="displayImages.length > 1" class="badge-images">+{{ (post.images?.length || 0) - displayImages.length }}</div>
      </template>
      <template v-else>
        <div class="media-fallback" aria-hidden="true">
          <span class="fallback-initial" aria-hidden="true">{{ getAuthorInitial(post.author) }}</span>
        </div>
      </template>
    </div>

    <div class="body">
      <h3 class="title">{{ titleText }}</h3>
      <p class="excerpt" v-if="excerptText">{{ excerptText }}</p>

      <div class="meta-row" aria-hidden="true">
        <div class="author">
          <span class="author-avatar" aria-hidden="true">{{ getAuthorInitial(post.author) }}</span>
          <span class="author-name">{{ post.author?.username ?? '匿名' }}</span>
        </div>

        <div class="right-meta">
          <span class="tag" v-if="post.post_type !== undefined" :title="mapPostType(post.post_type)">{{ mapPostType(post.post_type) }}</span>
          <span class="counts" title="评论数">💬 {{ post.comment_count ?? 0 }}</span>
          <span class="counts" title="浏览量">👁 {{ post.view_count ?? 0 }}</span>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  post: { type: Object, required: true }
})
const emit = defineEmits(['view'])

const displayImages = computed(() => {
  const imgs = props.post.images || []
  return Array.isArray(imgs) ? imgs.slice(0, 3) : (imgs ? [imgs] : [])
})

const titleText = computed(() => props.post.post_title ?? props.post.title ?? '(无标题)')
const excerptText = computed(() => props.post.excerpt ?? (props.post.content ? (props.post.content.length > 140 ? props.post.content.slice(0, 140) + '…' : props.post.content) : ''))

const getImageUrl = (img) => {
  if (!img) return ''
  if (typeof img === 'string' && img.startsWith('http')) return img
  if (typeof img === 'object' && img.url) return img.url
  const base = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/api.*$/,'') || (import.meta.env.DEV ? 'http://localhost:44359' : '')
  return base ? `${base}/${String(img).replace(/^\/+/,'')}` : String(img)
}

const getAuthorInitial = (author) => (author && author.username) ? author.username.charAt(0).toUpperCase() : (props.post.authorId ? String(props.post.authorId).charAt(0) : '?')

const mapPostType = (n) => n === 0 ? '柴圈' : n === 1 ? '游戏' : '其它'

const viewPost = () => emit('view', props.post.id ?? props.post.post_id ?? null)
</script>

<style scoped>
:root{
  --card-bg: #ffffff;
  --card-border: #e9f0fb;
  --muted: #64748b;
  --text: #0f172a;
  --accent-from: #667eea;
  --accent-to: #764ba2;
  --radius: 8px;       /* 更扁的圆角 */
  --gap: 8px;          /* 更紧凑的间距 */
  --transition: 150ms cubic-bezier(.2,.9,.25,1);
}

/* Card — 更扁、扁平化、信息集中（横向布局） */
.post-item {
  display: flex;
  flex-direction: row;
  gap: var(--gap);
  align-items: flex-start;
  background: var(--card-bg);
  border-radius: var(--radius);
  overflow: hidden;
  border: 1px solid var(--card-border);
  box-shadow: 0 4px 10px rgba(15,23,42,0.03); /* 更轻的阴影 */
  cursor: pointer;
  transition: transform var(--transition), box-shadow var(--transition), border-color var(--transition);
  outline: none;
  min-height: 0;
}

/* Hover & keyboard focus — 更微妙 */
.post-item:hover,
.post-item:focus-visible {
  transform: translateY(-2px);
  box-shadow: 0 10px 22px rgba(15,23,42,0.04);
  border-color: rgba(102,126,234,0.08);
}
.post-item:active { transform: translateY(0); }

/* Media area — 固定较窄宽度，信息更集中 */
.media {
  position: relative;
  width: 120px;             /* 更小、更集中 */
  min-width: 120px;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  background: linear-gradient(180deg,#fbfdff,#fff);
  display:flex;
  align-items:center;
  justify-content:center;
  flex-shrink: 0;
}
.media-img { width:100%; height:100%; background-size:cover; background-position:center; transition: transform 260ms cubic-bezier(.2,.9,.25,1); }
.post-item:hover .media-img { transform: scale(1.02); }

/* Fallback */
.media-fallback { width:100%; height:100%; display:flex; align-items:center; justify-content:center; background: linear-gradient(135deg,#f6f9ff,#fbfdff); }
.fallback-initial {
  width:48px;
  height:48px;
  border-radius:10px;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  font-weight:800;
  color:#fff;
  background: linear-gradient(135deg,var(--accent-from),var(--accent-to));
  font-size:1.05rem;
  box-shadow: 0 6px 14px rgba(102,126,234,0.06);
}

/* badge — 更小、更不占空间 */
.badge-images {
  position:absolute;
  right:8px;
  bottom:8px;
  background: rgba(2,6,23,0.72);
  color:#fff;
  padding:4px 6px;
  border-radius:8px;
  font-size:0.75rem;
  box-shadow:0 6px 12px rgba(2,6,23,0.08);
}

/* Body — 更紧凑 */
.body {
  padding: 10px 12px;
  display:flex;
  flex-direction:column;
  gap:6px;
  width: 100%;
}

/* Title */
.title {
  margin:0;
  font-size:0.98rem;
  color: var(--text);
  line-height:1.22;
  font-weight:700;
  display:-webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  overflow: hidden;
  line-clamp: 2;
  word-break: break-word;
}

/* Excerpt：只显示两行以节省空间 */
.excerpt {
  margin:0;
  color: #475569;
  font-size:0.86rem;
  line-height:1.4;
  display:-webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  overflow: hidden;
  line-clamp: 2;
  max-height: calc(1.4em * 2);
  text-overflow: ellipsis;
}

/* Meta row — 横向排列，紧凑 */
.meta-row {
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap:8px;
  margin-top:auto;
  width:100%;
}

/* Author group 更紧凑 */
.author {
  display:flex;
  align-items:center;
  gap:8px;
  color:#334155;
  font-size:0.84rem;
}
.author-avatar {
  width:24px;
  height:24px;
  border-radius:6px;
  background: linear-gradient(135deg,var(--accent-from),var(--accent-to));
  display:inline-flex;
  align-items:center;
  justify-content:center;
  color:#fff;
  font-weight:700;
  font-size:0.78rem;
  box-shadow:0 6px 14px rgba(102,126,234,0.05);
}
.author-name { font-weight:600; color:var(--text); font-size:0.86rem; }

/* Right meta — 计数与标签并列 */
.right-meta {
  display:flex;
  align-items:center;
  gap:8px;
  color:var(--muted);
  font-size:0.82rem;
  white-space:nowrap;
}
.tag {
  background: linear-gradient(180deg, rgba(102,126,234,0.04), rgba(118,75,162,0.02));
  color:#2b3a67;
  padding:4px 7px;
  border-radius:999px;
  font-weight:600;
  font-size:0.78rem;
}
.counts { display:inline-flex; align-items:center; gap:6px; font-size:0.82rem; color:var(--muted); }

/* Responsive — 在小屏幕回到纵向，但仍保持紧凑 */
@media (max-width: 640px) {
  .post-item { flex-direction: column; }
  .media { width:100%; aspect-ratio: 16/9; min-width: auto; }
  .body { padding:12px; }
  .title { font-size:1rem; }
  .fallback-initial { width:56px; height:56px; font-size:1.15rem; border-radius:10px; }
}

/* focus-visible 更柔和 */
.post-item:focus-visible { box-shadow: 0 0 0 3px rgba(102,126,234,0.08), 0 10px 20px rgba(15,23,42,0.03); transform: translateY(-2px); }
</style>