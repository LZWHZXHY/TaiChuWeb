<template>
  <section class="card" aria-labelledby="records-title">
    <header class="card-head">
      <h3 id="records-title">4. 战斗记录</h3>
      <div class="tools">
        <input class="input" type="search" placeholder="搜索：对局标题 / 参与者…（接入后生效）" />
        <button class="btn ghost" type="button" disabled>刷新</button>
      </div>
    </header>

    <div class="table-wrap" role="region" aria-labelledby="records-table">
      <table class="table" id="records-table">
        <thead>
          <tr>
            <th class="th">日期</th>
            <th class="th">对局标题</th>
            <th class="th">参与者</th>
            <th class="th">结果</th>
            <th class="th">记录链接</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="td empty" colspan="5">
              <div class="empty">
                <div class="emoji">🕒</div>
                <div class="text">
                  <strong>暂无战斗记录</strong>
                  <span>接入 API 后将展示对战的基本信息与跳转链接。</span>
                </div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
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

.table-wrap { border:1px solid var(--ring); border-radius:12px; overflow:hidden; }
.table { width:100%; border-collapse:collapse; background:#fff; }
.th, .td { text-align:left; font-size:14px; color:#0f172a; padding:10px 12px; border-bottom:1px solid var(--ring); }

.empty { display:grid; grid-auto-flow:column; gap:10px; align-items:center; justify-content:center; color:#475569; text-align:center; }
.empty .emoji { font-size:18px; }
.empty .text strong { display:block; font-weight:900; color:#0f172a; }

.panel-foot { display:grid; justify-content:end; padding-top:10px; }
.pager { display:flex; gap:10px; align-items:center; }
.page-text { color:#475569; font-size:13px; }
.btn.small { padding:6px 10px; font-weight:700; }
</style>