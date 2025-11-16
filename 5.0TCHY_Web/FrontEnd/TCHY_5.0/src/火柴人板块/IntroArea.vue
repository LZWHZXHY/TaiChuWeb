<template>
  <div class="intro">
    <!-- Hero -->
    <header class="hero" role="banner">
      <h2 class="hero-title">欢迎来到国产新柴圈平台</h2>
      <p class="hero-sub">
        在这里，你可以发起接力、联合、锦标赛；参与官方活动；完成动画师等级考核；创建或加入柴圈社团；更可探索太初约战场。
      </p>
      <div class="hero-cta">
        <button class="btn primary" type="button" aria-label="查看板块介绍">了解平台</button>
        <button class="btn ghost" type="button" aria-label="开始探索功能">开始探索</button>
      </div>
    </header>

    <!-- 快速统计（空态） -->
    <section class="stats" aria-labelledby="stats-title">
      <h3 id="stats-title" class="sr-only">平台关键指标</h3>
      <template v-if="loading">
        <ul class="stats-list">
          <li v-for="n in 4" :key="n" class="stat-item skeleton">
            <div class="stat-value skeleton-line"></div>
            <div class="stat-label skeleton-line sm"></div>
          </li>
        </ul>
      </template>
      <template v-else-if="stats.length">
        <ul class="stats-list">
          <li v-for="s in stats" :key="s.id" class="stat-item">
            <div class="stat-value">{{ s.value }}</div>
            <div class="stat-label">{{ s.label }}</div>
          </li>
        </ul>
      </template>
      <div v-else class="empty">统计数据待接入</div>
    </section>

    <!-- 核心功能（空态） -->
    <section class="highlights" aria-labelledby="highlights-title">
      <h3 id="highlights-title" class="section-title">核心功能</h3>
      <template v-if="loading">
        <div class="cards">
          <article v-for="n in 6" :key="n" class="card skeleton">
            <div class="card-icon skeleton-circle"></div>
            <h4 class="card-title skeleton-line"></h4>
            <p class="card-desc skeleton-line lg"></p>
          </article>
        </div>
      </template>
      <div v-else-if="features.length" class="cards">
        <article v-for="f in features" :key="f.id" class="card">
          <div class="card-icon">{{ f.icon }}</div>
          <h4 class="card-title">{{ f.title }}</h4>
          <p class="card-desc">{{ f.description }}</p>
        </article>
      </div>
      <div v-else class="empty">功能列表待接入</div>
    </section>

    <!-- 快速行动（空态） -->
    <section class="quick-actions" aria-labelledby="actions-title">
      <h3 id="actions-title" class="section-title">快速开始</h3>
      <template v-if="loading">
        <div class="actions">
          <div v-for="n in 5" :key="n" class="action-btn skeleton">
            <span class="action-icon skeleton-circle"></span>
            <span class="action-text">
              <span class="skeleton-line"></span>
              <span class="skeleton-line sm"></span>
            </span>
          </div>
        </div>
      </template>
      <div v-else-if="actions.length" class="actions">
        <button
          v-for="a in actions"
          :key="a.id"
          type="button"
          class="action-btn"
          :title="a.tip"
        >
          <span class="action-icon">{{ a.icon }}</span>
          <span class="action-text">
            <strong>{{ a.title }}</strong>
            <small>{{ a.subtitle }}</small>
          </span>
        </button>
      </div>
      <div v-else class="empty">快速入口待接入</div>
      <p class="hint">提示：使用左侧导航可切换至对应板块查看详情。</p>
    </section>

    <!-- 活动与约战（空态） -->
    <section class="events" aria-labelledby="events-title">
      <h3 id="events-title" class="section-title">活动与约战</h3>
      <div class="event-columns">
        <div class="event-col">
          <h4 class="event-col-title">当前最火热 🔥</h4>
          <template v-if="loading">
            <ul class="event-list">
              <li v-for="n in 2" :key="n" class="event-item skeleton">
                <div class="event-badge skeleton-line sm"></div>
                <div class="event-body">
                  <div class="event-title skeleton-line"></div>
                  <div class="event-meta skeleton-line sm"></div>
                  <p class="event-desc skeleton-line lg"></p>
                </div>
              </li>
            </ul>
          </template>
          <ul v-else-if="hotEvents.length" class="event-list">
            <li v-for="e in hotEvents" :key="e.id" class="event-item">
              <div class="event-badge">热门</div>
              <div class="event-body">
                <div class="event-title">{{ e.title }}</div>
                <div class="event-meta">{{ e.time }} · {{ e.category }}</div>
                <p class="event-desc">{{ e.desc }}</p>
              </div>
            </li>
          </ul>
          <div v-else class="empty">热门活动待接入</div>
        </div>
        <div class="event-col">
          <h4 class="event-col-title">历史佳作精选 📜</h4>
          <template v-if="loading">
            <ul class="event-list">
              <li v-for="n in 2" :key="n" class="event-item skeleton">
                <div class="event-badge skeleton-line sm"></div>
                <div class="event-body">
                  <div class="event-title skeleton-line"></div>
                  <div class="event-meta skeleton-line sm"></div>
                  <p class="event-desc skeleton-line lg"></p>
                </div>
              </li>
            </ul>
          </template>
          <ul v-else-if="historyPicks.length" class="event-list">
            <li v-for="e in historyPicks" :key="e.id" class="event-item">
              <div class="event-badge ghost">经典</div>
              <div class="event-body">
                <div class="event-title">{{ e.title }}</div>
                <div class="event-meta">{{ e.year }} · {{ e.author }}</div>
                <p class="event-desc">{{ e.desc }}</p>
              </div>
            </li>
          </ul>
          <div v-else class="empty">历史精选待接入</div>
        </div>
      </div>
    </section>

    <!-- FAQ（空态，可选） -->
    <section class="faq" aria-labelledby="faq-title">
      <h3 id="faq-title" class="section-title">常见问题</h3>
      <template v-if="loading">
        <div class="faq-item skeleton">
          <div class="faq-q skeleton-line"></div>
          <div class="faq-a skeleton-line lg"></div>
        </div>
      </template>
      <template v-else-if="faqs.length">
        <details v-for="q in faqs" :key="q.id" class="faq-item">
          <summary class="faq-q">{{ q.q }}</summary>
          <p class="faq-a">{{ q.a }}</p>
        </details>
      </template>
      <div v-else class="empty">FAQ 待接入</div>
    </section>

    <!-- 结束语（保持文案，非数据） -->
    <section class="closing">
      <div class="closing-card">
        <h3 class="closing-title">总之，请随意探索这个区域</h3>
        <p class="closing-desc">从创作到竞技，从社群到企划，这里都是你的舞台。现在就出发吧！</p>
        <div class="closing-cta">
          <button class="btn primary" type="button">去创建社团</button>
          <button class="btn outline" type="button">看看约战场</button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { toRefs } from 'vue'
/* 全部数据通过 props 注入；默认空数组 + 可选 loading 骨架态 */
const props = defineProps({
  loading: { type: Boolean, default: false },
  stats: { type: Array, default: () => [] },
  features: { type: Array, default: () => [] },
  actions: { type: Array, default: () => [] },
  hotEvents: { type: Array, default: () => [] },
  historyPicks: { type: Array, default: () => [] },
  faqs: { type: Array, default: () => [] }
})

const { loading, stats, features, actions, hotEvents, historyPicks, faqs } = toRefs(props)
</script>

<style scoped>
/* 提升整体对比度与可读性 */
:root {
  --bg: #edf2f7;                /* 稍深的背景，增强与白卡片的对比 */
  --ink: #0b1220;               /* 主文字更深 */
  --muted: #3f4a5f;             /* 次级文字加深，避免发灰 */
  --brand: #2563eb;
  --brand-weak: #dfeaff;        /* 浅品牌背景提升可见度 */
  --card: #ffffff;
  --ring: #d6dbe6;              /* 边框更清晰 */
  --shadow-sm: 0 1px 2px rgba(2,6,23,.05), 0 6px 18px rgba(2,6,23,.06);
  --shadow-md: 0 10px 30px rgba(2,6,23,.10);
  --r-lg: 16px; --r-md: 12px; --r-sm: 10px;
}

.intro {
  display: grid;
  gap: 28px;
  padding: clamp(16px, 4vw, 28px);
  background: var(--bg);
  color: var(--ink);
}

/* Hero */
.hero {
  background: linear-gradient(180deg, #ffffff 0%, #f2f6ff 100%);
  border: 1px solid var(--ring);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm);
  padding: clamp(18px, 4vw, 28px);
  text-align: center;
}
.hero-title {
  font-size: clamp(22px, 3.2vw, 30px);
  font-weight: 900;
  letter-spacing: .3px;
  margin: 0;
  color: #0e1a32;
}
.hero-sub {
  margin: 10px auto 0;
  max-width: 760px;
  color: var(--muted);
  line-height: 1.75;
}
.hero-cta {
  margin-top: 16px;
  display: flex; gap: 12px; justify-content: center; flex-wrap: wrap;
}

/* Buttons */
.btn {
  appearance: none;
  border: 1px solid transparent;
  border-radius: 999px;
  padding: 10px 16px;
  font-weight: 700;
  cursor: pointer;
  transition: transform .12s, box-shadow .18s, background .18s, color .18s, border-color .18s;
}
.btn.primary { background: var(--brand); color: #000000; box-shadow: 0 6px 18px rgba(37,99,235,.25); }
.btn.primary:hover { transform: translateY(-1px); box-shadow: 0 10px 24px rgba(37,99,235,.35); }
.btn.ghost { background: #fff; color: var(--ink); border-color: var(--ring); }
.btn.ghost:hover { background: #f8fbff; border-color: #cfdaf3; }
.btn.outline { background: transparent; color: var(--brand); border-color: #c8d8ff; }
.btn.outline:hover { background: var(--brand-weak); }

/* Stats */
.stats-list {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
  list-style: none;
  padding: 0; margin: 0;
}
.stat-item {
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--r-md);
  padding: 14px 16px;
  text-align: center;
  box-shadow: var(--shadow-sm);
}
.stat-value { font-size: clamp(18px, 3vw, 24px); font-weight: 900; color: var(--ink); }
.stat-label { margin-top: 6px; color: #44506a; }

/* Sections */
.section-title {
  font-size: clamp(18px, 2.2vw, 22px);
  font-weight: 800;
  margin: 0 0 12px;
  color: #0f1e3a;   /* 标题更醒目 */
}

/* Highlights */
.cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 14px;
}
.card {
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--r-md);
  padding: 16px;
  text-align: left;
  box-shadow: var(--shadow-sm);
  transition: transform .12s, box-shadow .16s, border-color .16s;
}
.card:hover { transform: translateY(-2px); box-shadow: var(--shadow-md); border-color: #cbd6f0; }
.card-icon { font-size: 22px; color: #2050d6; }
.card-title { margin: 8px 0 6px; font-weight: 800; color: #13203a; }
.card-desc { color: #3f4a5f; line-height: 1.75; }

/* Quick actions */
.quick-actions .actions {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
  gap: 12px;
}
.action-btn {
  display: flex; gap: 12px; align-items: center;
  padding: 12px 14px;
  background: #fff;
  border: 1px solid var(--ring);
  border-radius: var(--r-md);
  box-shadow: var(--shadow-sm);
  cursor: pointer;
  text-align: left;
  transition: background .16s, box-shadow .16s, transform .12s, border-color .16s;
}
.action-btn:hover { background: #f6f9ff; transform: translateY(-2px); box-shadow: var(--shadow-md); border-color: #cbd6f0; }
.action-icon { font-size: 18px; color: #1d4ed8; }
.action-text strong { color: #0e1a32; }
.action-text small { display: block; color: #54607a; margin-top: 2px; }
.hint { margin-top: 8px; color: #4b566d; }

/* Events */
.event-columns {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 14px;
}
.event-col-title { margin: 0 0 8px; font-weight: 800; color: #0f1e3a; }
.event-list { list-style: none; padding: 0; margin: 0; display: grid; gap: 10px; }
.event-item {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 12px;
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--r-md);
  padding: 12px;
  box-shadow: var(--shadow-sm);
}
.event-badge {
  align-self: start;
  padding: 6px 10px;
  background: var(--brand);
  color: #fff;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 800;
}
.event-badge.ghost {
  background: #e9efff;   /* 更亮的底色 */
  color: #0e1a32;        /* 更深文字 */
}
.event-title { font-weight: 800; color: #13203a; }
.event-meta { color: #586380; font-size: 13px; margin-top: 2px; }
.event-desc { margin-top: 6px; color: #0b1220; line-height: 1.7; }

/* FAQ */
.faq-item {
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--r-md);
  padding: 12px 14px;
  box-shadow: var(--shadow-sm);
}
.faq-item + .faq-item { margin-top: 10px; }
.faq-q { font-weight: 800; cursor: pointer; color: #0e1a32; }
.faq-a { margin-top: 8px; color: #3f4a5f; line-height: 1.75; }

/* Closing */
.closing-card {
  background: linear-gradient(180deg, #ffffff, #f5f9ff);
  border: 1px solid var(--ring);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm);
  padding: clamp(16px, 3vw, 24px);
  text-align: center;
}
.closing-title { margin: 0; font-weight: 900; font-size: clamp(18px, 2.4vw, 24px); color: #0f1e3a; }
.closing-desc { color: #46536b; margin: 8px auto 14px; max-width: 720px; }
.closing-cta { display: flex; gap: 10px; justify-content: center; flex-wrap: wrap; }

/* 空态与骨架（提升可视性） */
.empty {
  color: #3f4a5f;
  background: #fbfdff;           /* 与页面背景区分 */
  border: 1px dashed #cbd6f0;    /* 更清晰的虚线 */
  border-radius: var(--r-md);
  padding: 14px;
  text-align: center;
  box-shadow: var(--shadow-sm);
}
.skeleton { position: relative; overflow: hidden; }
.skeleton::after {
  content: "";
  position: absolute; inset: 0;
  background: linear-gradient(90deg, transparent, rgba(0,0,0,.05), transparent);
  animation: shimmer 1.2s infinite;
}
.skeleton-line { height: 14px; background: #e3e9f2; border-radius: 6px; }
.skeleton-line.sm { height: 10px; width: 60%; margin-top: 8px; }
.skeleton-line.lg { height: 14px; width: 90%; margin-top: 10px; }
.skeleton-circle { width: 24px; height: 24px; background: #e3e9f2; border-radius: 50%; }
@keyframes shimmer { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

/* accessibility */
.sr-only {
  position: absolute !important;
  height: 1px; width: 1px;
  overflow: hidden; clip: rect(1px, 1px, 1px, 1px);
  white-space: nowrap; border: 0; padding: 0; margin: -1px;
}

/* Responsive tweaks */
@media (min-width: 900px) {
  .steps { grid-template-columns: repeat(2, 1fr); }
}
</style>