<template>
  <div class="wall">
    <!-- 顶部：标题 + 搜索（可留空） -->
    <header class="container head">
      <div class="head__title">
        <h1 class="h1">发现作品</h1>
        <p class="muted">最新与热门作品流 · 瀑布式排布</p>
      </div>
      <div class="head__actions">
        <input class="input" type="search" placeholder="搜索标题 / 作者 / 标签（占位）" />
      </div>
    </header>

    <!-- 主体：瀑布流 -->
    <main class="container">
      <section class="masonry">
        <article v-for="it in items" :key="it.id" class="card" @click="onCardClick(it)">
          <img class="img" :src="it.src" :alt="it.title" loading="lazy" />
          <div class="overlay">
            <div class="meta">
              <h3 class="title" :title="it.title">{{ it.title }}</h3>
              <div class="sub">
                <span class="author">{{ it.author }}</span>
                <span class="dot">·</span>
                <span class="muted">{{ it.likes }} 赞</span>
              </div>
            </div>
          </div>
        </article>
      </section>

      <!-- 分页/加载更多（占位） -->
      <div class="more">
        <button class="btn">加载更多</button>
      </div>
    </main>

    <!-- 页脚（可选） -->
    <footer class="container foot">
      <small class="muted">© 2025 ArtHub</small>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

/**
 * 这里只提供示例数据用于展示版式。
 * 接后端时，直接替换 items 为你的数据源即可。
 */
const items = ref(
  Array.from({ length: 24 }).map((_, i) => {
    const w = 600 + (i % 4) * 80
    const h = 400 + ((i * 7) % 5) * 90
    const id = `seed-${i + 1}`
    return {
      id,
      title: `作品 #${i + 1}`,
      author: ['Mika', 'Yuki', 'Hana', 'Sora', 'Aoi'][i % 5],
      likes: Math.floor(Math.random() * 300),
      src: `https://picsum.photos/seed/${encodeURIComponent(id)}/${w}/${h}`,
    }
  }),
)

function onCardClick(it: any) {
  // 这里只做占位。后续你可在此触发弹窗/跳转详情。
  console.log('open item:', it.id)
}
</script>

<style scoped>
/* 局部主题与版式变量（仅当前页面生效） */
.wall {
  --container-max: 1180px;
  --gutter: clamp(16px, 4vw, 48px);

  --bg: #f9fafb;
  --panel: #ffffff;
  --fg: #111827;
  --fg-soft: #374151;
  --muted: #6b7280;
  --border: #e5e7eb;
  --accent: #2563eb;

  --radius: 12px;

  --step-2: clamp(24px, 2.2vw, 32px);
  --step-0: clamp(14px, 1.1vw, 16px);
  --step--1: clamp(12px, .95vw, 13px);

  color: var(--fg);
  background: var(--bg);
  min-height: 100vh;
  font-family: system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif;
}

.container {
  max-width: var(--container-max);
  margin-inline: auto;
  padding-inline: var(--gutter);
}

.h1 {
  margin: 0 0 4px;
  font-size: var(--step-2);
  font-weight: 700;
  letter-spacing: .2px;
}
.muted { color: var(--muted); font-size: var(--step--1); }

/* 顶部 */
.head {
  padding: 28px 0 12px;
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: end;
  gap: 12px;
}
.head__title { display: grid; gap: 4px; }
.head__actions { display: flex; align-items: center; gap: 8px; }
.input {
  width: min(360px, 62vw);
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 10px;
  background: #fff;
  color: var(--fg);
  font: inherit;
}
.input::placeholder { color: #9ca3af; }

/* Masonry：使用 CSS columns 实现，兼容性好 */
.masonry {
  column-gap: 14px;
  /* 响应式列数 */
  column-count: 5;
}
@media (max-width: 1400px) { .masonry { column-count: 4; } }
@media (max-width: 1100px) { .masonry { column-count: 3; } }
@media (max-width: 800px)  { .masonry { column-count: 2; } }
@media (max-width: 520px)  { .masonry { column-count: 1; } }

/* 瀑布卡片 */
.card {
  position: relative;
  display: inline-block;          /* 让元素参与列布局 */
  width: 100%;
  margin: 0 0 14px;               /* 与 column-gap 对齐 */
  border-radius: var(--radius);
  overflow: hidden;
  background: var(--panel);
  border: 1px solid var(--border);
  break-inside: avoid;            /* 核心：防止被拆分 */
  cursor: pointer;
}
.img {
  display: block;
  width: 100%;
  height: auto;
}

/* 悬浮信息层（极简） */
.overlay {
  position: absolute;
  inset: auto 0 0 0;              /* 贴底部 */
  padding: 10px 12px;
  background: linear-gradient(180deg, rgba(0,0,0,0) 0%, rgba(0,0,0,.45) 100%);
  color: #fff;
  opacity: 0;
  transform: translateY(8px);
  transition: .18s ease;
}
.card:hover .overlay {
  opacity: 1;
  transform: translateY(0);
}
.meta { display: grid; gap: 4px; }
.title {
  margin: 0;
  font-size: var(--step-0);
  font-weight: 600;
  line-height: 1.3;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.sub {
  display: inline-flex; gap: 6px; align-items: center;
  font-size: var(--step--1);
  opacity: .95;
}
.dot { opacity: .6; }

/* 底部区域 */
.more { display: flex; justify-content: center; padding: 16px 0 36px; }
.btn {
  padding: 10px 16px;
  border: 1px solid var(--border);
  border-radius: 999px;
  background: #fff;
  color: var(--fg-soft);
  cursor: pointer;
  font: inherit;
}
.btn:hover { border-color: var(--accent); color: var(--accent); }

.foot { padding: 0 0 26px; }

/* 深色模式 */
@media (prefers-color-scheme: dark) {
  .wall {
    --bg: #0d1117;
    --panel: #0f172a;
    --fg: #e5e7eb;
    --fg-soft: #e5e7eb;
    --muted: #9ca3af;
    --border: #334155;
    --accent: #60a5fa;
  }
  .input { background: #0b1222; }
}
</style>