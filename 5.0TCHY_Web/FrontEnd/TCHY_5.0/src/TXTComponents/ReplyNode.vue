<template>
  <div class="reply-node" :style="{ marginLeft: depth ? depth * 18 + 'px' : '0' }">
    <div class="card">
      <div class="left">
        <img
          v-if="comment.author?.avatar"
          :src="resolveUrl(comment.author.avatar)"
          class="avatar"
          @error="onAvatarError"
        />
        <div v-else class="avatar-fallback">{{ initial }}</div>
      </div>

      <div class="right">
        <div class="head">
          <div class="author">{{ comment.author?.username || ('用户#' + comment.authorId) }}</div>
          <div class="meta">
            <span class="time">{{ formatTime(comment.createTime) }}</span>
            <button
              v-if="postId"
              type="button"
              class="reply-btn"
              @click="onReplyClick"
              :aria-label="`回复 ${comment.author?.username||'用户'}`"
            >
              回复
            </button>
          </div>
        </div>

        <div class="content" v-text="comment.content"></div>

        <div v-if="children && children.length" class="children">
          <ReplyNode
            v-for="c in children"
            :key="c.id"
            :comment="c"
            :depth="depth + 1"
            :post-id="postId"
            :children-map="childrenMap"
            :resolve-url="resolveUrl"
            @reply-open="onChildReplyOpen"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  comment: { type: Object, required: true }, // comment may have children[]
  depth: { type: Number, default: 0 },
  postId: { type: [String, Number], default: null },
  // optional childrenMap for backward compatibility
  childrenMap: { type: Object, default: null },
  resolveUrl: { type: Function, default: (p) => p }
})
const emit = defineEmits(['reply-open'])

// children: prefer comment.children (tree-built), fallback to childrenMap[comment.id]
const children = computed(() => {
  if (Array.isArray(props.comment.children) && props.comment.children.length > 0) {
    return props.comment.children
  }
  if (props.childrenMap && Object.prototype.hasOwnProperty.call(props.childrenMap, props.comment.id)) {
    return props.childrenMap[props.comment.id] || []
  }
  return []
})

const initial = computed(() => (props.comment.author?.username || String(props.comment.authorId || '')).charAt(0).toUpperCase())

const formatTime = (t) => {
  if (!t) return ''
  const d = (typeof t === 'string' || typeof t === 'number') ? new Date(t) : t
  if (Number.isNaN(d.getTime())) return ''
  return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}`
}

const onReplyClick = () => {
  emit('reply-open', props.comment.id, props.comment.author?.username || '')
}

const onAvatarError = (ev) => { ev.target.style.display = 'none' }

// forward child's reply-open events (may have multiple args)
const onChildReplyOpen = (...args) => {
  emit('reply-open', ...args)
}
</script>

<style scoped>
:root {
  --card-bg: #fff;
  --card-border: #eef6fb;
  --muted: #8b95a1;
  --accent: #0fa3a3;
  --text: #24303a;
  --radius: 8px;
  --gap: 12px;
}

/* Card wrapper */
.card {
  display: flex;
  gap: var(--gap);
  padding: 8px;
  border-radius: var(--radius);
  background: var(--card-bg);
  border: 1px solid var(--card-border);
  align-items: flex-start;
}

/* left column (avatar) */
.left {
  width: 44px;
  flex-shrink: 0;
  display: flex;
  align-items: flex-start;
  justify-content: center;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 6px;
  object-fit: cover;
  display: block;
}
.avatar-fallback {
  width: 40px;
  height: 40px;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--accent);
  color: #fff;
  font-weight: 700;
}

/* right column */
.right {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* header row: author + meta */
.head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.author {
  font-weight: 700;
  color: var(--text);
}
.meta {
  display: flex;
  gap: 8px;
  align-items: center;
}
.time {
  color: var(--muted);
  font-size: 0.88rem;
}

/* reply button */
.reply-btn {
  background: transparent;
  border: none;
  color: var(--accent);
  cursor: pointer;
  padding: 6px;
  border-radius: 6px;
}
.reply-btn:hover {
  background: rgba(15,163,163,0.06);
}

/* content body */
.content {
  color: var(--text);
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

/* children (nested replies) */
.children {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-left: 0;
}
</style>