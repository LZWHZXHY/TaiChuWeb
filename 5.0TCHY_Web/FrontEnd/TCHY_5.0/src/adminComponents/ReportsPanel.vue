<template>
  <section class="panel">
    <header class="panel-head">
      <div class="panel-title">
        <h2>举报处理</h2>
        <span class="tag">占位</span>
      </div>
    </header>
    <div class="placeholder">
      未来功能：举报单列表、筛选状态（未处理/已处理）、查看举报详情、操作记录。
    </div>
  </section>
</template>

<script setup>
function refresh() {
  // TODO: 接入后用于刷新举报列表
}
</script>

<style scoped>
.panel { background:#fff; border:1px solid #e6ebf3; border-radius:16px; padding:14px; box-shadow:0 2px 10px rgba(2,6,23,.06); }
.panel-head { display:grid; grid-template-columns:1fr auto; align-items:center; padding-bottom:10px; border-bottom:1px solid #e6ebf3; margin-bottom:10px; }
.panel-title { display:flex; gap:10px; align-items:center; }
.panel-title h2 { margin:0; font-size:18px; font-weight:900; }
.tag { padding:3px 8px; font-size:12px; font-weight:800; background:#eef2ff; border:1px solid #dbe5ff; border-radius:999px; }
.placeholder { padding:8px 4px; font-size:14px; color:#475569; line-height:1.7; }
</style>