<template>
  <section class="panel">
    <header class="panel-head">
      <div class="panel-title">
        <h2>社团审核</h2>
        <span class="tag">柴圈</span>
      </div>
      <div class="tools">
        <input
          v-model.trim="search"
          class="input"
          type="search"
          placeholder="搜索：社团名称 / 团长…"
          @keydown.enter.prevent
        />
        <select v-model="filterStatus" class="select">
          <option value="pending">待审核</option>
          <option value="approved">已通过</option>
          <option value="rejected">已拒绝</option>
        </select>
        <div class="bulk">
          <button class="btn primary small" type="button" disabled>批量通过</button>
          <button class="btn danger small" type="button" disabled>批量拒绝</button>
        </div>
      </div>
    </header>

    <!-- 表格区域：不加载任何数据，只保留空态 -->
    <div class="table-wrap" role="region" aria-labelledby="society-review-table">
      <table class="table" id="society-review-table">
        <thead>
          <tr>
            <th class="th checkbox"><input type="checkbox" disabled aria-label="全选" /></th>
            <th class="th">名称</th>
            <th class="th">团长</th>
            <th class="th">类型</th>
            <th class="th">规模</th>
            <th class="th">考核</th>
            <th class="th">提交时间</th>
            <th class="th actions">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="td empty" colspan="8">
              <div class="empty">
                <div class="emoji">🗒️</div>
                <div class="text">
                  <strong>暂无数据</strong>
                  <span>接入 API 后将展示社团申请。此处将支持单条审核（通过/拒绝）。</span>
                </div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer class="panel-foot">
      <div class="pager">
        <button class="btn ghost small" disabled type="button">上一页</button>
        <span class="page-text">第 1 / 1 页</span>
        <button class="btn ghost small" disabled type="button">下一页</button>
      </div>
    </footer>
  </section>
</template>

<script setup>
import { ref } from 'vue'

const search = ref('')
const filterStatus = ref('pending')

// 未来对外暴露的刷新方法（供父组件或事件总线调用）
function refresh() {
  // TODO: 接入时写：拉取审核列表
}
// 未来单条操作占位
function approveOne(id) { /* TODO */ }
function rejectOne(id) { /* TODO */ }
</script>

<style scoped>
.panel {
  background: #fff;
  border: 1px solid #e6ebf3;
  border-radius: 16px;
  box-shadow: 0 2px 10px rgba(2,6,23,.06);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.panel-head {
  display: grid; grid-template-columns: 1fr auto; gap: 10px; align-items: center;
  padding-bottom: 10px; border-bottom: 1px solid #e6ebf3;
}
.panel-title { display: flex; align-items: center; gap: 10px; }
.panel-title h2 { margin: 0; font-size: 18px; font-weight: 900; }
.tag {
  display: inline-block; padding: 3px 8px; font-size: 12px; font-weight: 800;
  color: #1e293b; background: #eef2ff; border: 1px solid #dbe5ff; border-radius: 999px;
}
.tools { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }
.input, .select {
  height: 36px; background: #fff; border: 1px solid #e6ebf3; border-radius: 10px;
  padding: 0 12px; font-size: 14px; outline: none;
  transition: border-color .15s ease, box-shadow .15s ease;
}
.input:focus, .select:focus { border-color: #d6deea; box-shadow: 0 0 0 3px rgba(37,99,235,.12); }
.bulk { display: flex; gap: 8px; }

.btn {
  appearance: none; border-radius: 10px; padding: 8px 12px;
  font-weight: 800; border: 1px solid #e6ebf3; background: #fff; color: #0f172a;
  cursor: pointer; transition: background .15s, transform .12s;
}
.btn.small { padding: 6px 10px; font-weight: 700; }
.btn.primary { background: #2563eb; border-color: #2563eb; color: #fff; }
.btn.danger { background: #ef4444; border-color: #ef4444; color: #fff; }
.btn:disabled { opacity: .55; cursor: default; }

.table-wrap { border: 1px solid #e6ebf3; border-radius: 12px; overflow: hidden; }
.table { width: 100%; border-collapse: collapse; font-size: 14px; }
.th, .td { text-align: left; padding: 10px 12px; border-bottom: 1px solid #e6ebf3; }
.th.checkbox { width: 44px; }
.th.actions { width: 180px; }

.empty { display: grid; grid-auto-flow: column; gap: 12px; align-items: center; justify-content: center; padding: 18px; color: #475569; }
.empty .emoji { font-size: 18px; }
.empty .text { display: grid; gap: 4px; text-align: center; }
.empty .text strong { font-weight: 900; }

.panel-foot { display: flex; justify-content: flex-end; }
.pager { display: flex; align-items: center; gap: 10px; }
.page-text { font-size: 13px; color: #475569; }
</style>