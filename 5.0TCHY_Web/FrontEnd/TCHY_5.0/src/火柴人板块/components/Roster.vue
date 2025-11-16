<template>
  <section class="card" aria-labelledby="roster-title">
    <header class="card-head">
      <h3 id="roster-title">3. 人设列表</h3>
      <div class="tools">
        <input class="input" type="search" placeholder="搜索：角色名称 / 阵营…（接入后生效）" />
        <button class="btn ghost" type="button" disabled>刷新</button>
      </div>
    </header>

    <!-- 网格卡片（无数据时显示空态） -->
    <div class="cards">
      <div class="empty">
        <div class="emoji">📄</div>
        <div class="text">
          <strong>暂无人设</strong>
          <span>接入 API 后将展示角色卡片（名称、头像、能力标签、提交者等）。</span>
        </div>
      </div>
    </div>

    <footer class="panel-foot">
      <div class="pager">
        <button class="btn ghost small" disabled>上一页</button>
        <span class="page-text">第 1 / 1 页</span>
        <button class="btn ghost small" disabled>下一页</button>
      </div>
    </footer>
  </section>
</template>

<script setup>
</script>

<style scoped>
:root { --card:#fff; --ring:#e6ebf3; --r-lg:16px; --shadow-sm:0 2px 10px rgba(2,6,23,.06); --muted:#5a6478; }
.card { background:var(--card); border:1px solid var(--ring); border-radius:var(--r-lg); box-shadow:var(--shadow-sm); padding:12px; }
.card-head { display:grid; grid-template-columns:1fr auto; gap:10px; align-items:center; border-bottom:1px solid var(--ring); padding-bottom:8px; margin-bottom:10px; }
.card-head h3 { margin:0; font-size:18px; font-weight:900; }

.tools { display:flex; gap:8px; align-items:center; flex-wrap:wrap; }
.input { height:36px; background:#fff; border:1px solid var(--ring); border-radius:10px; padding:0 12px; outline:none; }
.btn { appearance:none; border:1px solid var(--ring); border-radius:10px; padding:8px 12px; font-weight:800; background:#fff; color:#0f172a; }
.btn.ghost { background:#fff; }

.cards {
  border:1px solid var(--ring); border-radius:12px; padding:12px; background:#fff;
  display:grid; gap:10px; min-height:120px; place-items:center;
}

.empty { display:grid; grid-auto-flow:column; gap:10px; align-items:center; justify-content:center; color:#475569; text-align:center; }
.empty .emoji { font-size:18px; }
.empty .text strong { display:block; font-weight:900; color:#0f172a; }

.panel-foot { display:grid; justify-content:end; padding-top:10px; }
.pager { display:flex; gap:10px; align-items:center; }
.page-text { color:#475569; font-size:13px; }
.btn.small { padding:6px 10px; font-weight:700; }
</style>