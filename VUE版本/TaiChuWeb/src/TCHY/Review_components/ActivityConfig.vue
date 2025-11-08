<template>
  <div class="wrap">
    <h2 class="h2">活动配置</h2>
    <div class="grid-2">
      <div class="card">
        <div class="card-title">基础信息</div>
        <div class="form">
          <label class="field"><span class="lab">活动标题</span><input class="input" placeholder="双十一创作挑战赛" /></label>
          <label class="field"><span class="lab">活动简介</span><textarea class="input" rows="4" placeholder="一句话介绍活动目标与参与方式"></textarea></label>
          <label class="field"><span class="lab">活动封面</span><div class="upload">上传占位</div></label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">时间与规则</div>
        <div class="form">
          <div class="row-2">
            <label class="field"><span class="lab">开始时间</span><input class="input" type="datetime-local" /></label>
            <label class="field"><span class="lab">结束时间</span><input class="input" type="datetime-local" /></label>
          </div>
          <label class="field"><span class="lab">参与门槛</span><input class="input" placeholder="注册满7天、实名认证" /></label>
          <label class="field"><span class="lab">评审规则</span><textarea class="input" rows="4" placeholder="质量×60% + 热度×40%"></textarea></label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">配额与奖励</div>
        <div class="form row-3">
          <label class="field"><span class="lab">名额</span><input class="input" type="number" min="0" placeholder="100" /></label>
          <label class="field"><span class="lab">人均奖励</span><input class="input" type="number" min="0" placeholder="100" /></label>
          <label class="field"><span class="lab">货币单位</span>
            <select class="input"><option>人民币</option><option>积分</option></select>
          </label>
        </div>
      </div>

      <div class="card">
        <div class="card-title">可见性与发布</div>
        <div class="form">
          <label class="switch"><input type="checkbox" /> <span>在官网首页展示</span></label>
          <label class="switch"><input type="checkbox" /> <span>允许匿名参与</span></label>
          <div class="actions">
            <button class="btn ghost" @click="onReset">重置</button>
            <button class="btn primary" @click="onSave">保存配置</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
function onReset() { /* TODO: 重置表单（保持空） */ }
function onSave() { /* TODO: 保存配置（保持空） */ }
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
.input, textarea.input, select.input { width: 100%; border: 1px solid #e5e7eb; border-radius: 10px; padding: 8px 10px; outline: none; background: #fff; }
.upload { border: 1px dashed #e5e7eb; border-radius: 12px; padding: 20px; text-align: center; color: #64748b; background: #f8fafc; }
.row-2 { display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; }
.row-3 { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
.switch { display: flex; align-items: center; gap: 8px; }
.actions { display: flex; gap: 8px; }
.btn { padding: 8px 12px; border-radius: 10px; border: 1px solid #e5e7eb; background: #fff; cursor: pointer; }
.btn.primary { background: #111827; color: #fff; }
.btn.ghost { background: transparent; }
@media (max-width: 960px) { .grid-2, .row-2, .row-3 { grid-template-columns: 1fr; } }
</style>