<template>
  <div class="wrap">
    <h2 class="h2">站点设置</h2>
    <div class="grid-2">
      <div class="card">
        <div class="card-title">基础设置</div>
        <div class="form">
          <label class="field"><span class="lab">站点标题</span><input class="input" placeholder="社区官网" /></label>
          <label class="field"><span class="lab">联系邮箱</span><input class="input" type="email" placeholder="ops@example.com" /></label>
          <label class="field"><span class="lab">全站公告</span><textarea class="input" rows="3" placeholder="维护/活动通知…"></textarea></label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">品牌外观</div>
        <div class="form">
          <label class="field"><span class="lab">主色</span><input class="input" type="color" value="#4f46e5" /></label>
          <label class="field"><span class="lab">LOGO</span><div class="upload">上传占位</div></label>
          <label class="field"><span class="lab">Favicon</span><div class="upload">上传占位</div></label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">安全与风控</div>
        <div class="form">
          <label class="switch"><input type="checkbox" /> <span>启用敏感词过滤</span></label>
          <label class="switch"><input type="checkbox" /> <span>开启注册审核</span></label>
          <label class="switch"><input type="checkbox" /> <span>限制外链跳转</span></label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">数据与备份</div>
        <div class="form">
          <div class="actions">
            <button class="btn ghost" @click="onExport">导出数据</button>
            <button class="btn" @click="onBackup">立即备份</button>
            <button class="btn danger" @click="onClearCache">清理缓存</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
function onExport() { /* TODO */ }
function onBackup() { /* TODO */ }
function onClearCache() { /* TODO */ }
</script>

<style scoped>
.wrap { display: grid; gap: 12px; }
.h2 { margin: 0; font-size: 18px; }
.grid-2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
.card { background: #fff; border: 1px solid #e5e7eb; border-radius: 12px; overflow: hidden; }
.card-title { font-weight: 700; padding: 10px 12px; border-bottom: 1px solid #e5e7eb; }
.form { display: grid; gap: 10px; padding: 12px; }
.field { display: grid; gap: 6px; }
.lab { color: #64748b; font-size: 12px; }
.input, textarea.input { width: 100%; border: 1px solid #e5e7eb; border-radius: 10px; padding: 8px 10px; outline: none; background: #fff; }
.upload { border: 1px dashed #e5e7eb; border-radius: 12px; padding: 20px; text-align: center; color: #64748b; background: #f8fafc; }
.switch { display: flex; align-items: center; gap: 8px; }
.actions { display: flex; gap: 8px; }
.btn { padding: 8px 12px; border-radius: 10px; border: 1px solid #e5e7eb; background: #fff; cursor: pointer; }
.btn.ghost { background: transparent; }
.btn.danger { background: #ef4444; color: #fff; border-color: #ef4444; }
@media (max-width: 960px) { .grid-2 { grid-template-columns: 1fr; } }
</style>