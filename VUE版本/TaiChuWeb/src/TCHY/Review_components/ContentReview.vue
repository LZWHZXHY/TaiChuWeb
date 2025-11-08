<template>
  <div class="review">
    <!-- 工具栏 -->
    <div class="toolbar">
      <div class="left">
        <h2 class="h2">内容审核</h2>
        <div class="sub">管理社区帖子/评论/举报等内容</div>
      </div>
      <div class="right">
        <div class="seg">
          <button class="seg-btn" :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'">表格</button>
          <button class="seg-btn" :class="{ active: viewMode === 'card' }" @click="viewMode = 'card'">卡片</button>
        </div>
        <button class="btn ghost" @click="onRefresh">刷新</button>
        <button class="btn" @click="onOpenSettings">页面设置</button>
      </div>
    </div>

    <!-- 筛选 -->
    <section class="section filter">
      <div class="search">
        <span class="ico">🔍</span>
        <input class="input search-input" type="search" v-model="filters.query" placeholder="搜索标题/作者/关键词" @keyup.enter="onSearch(filters.query)" />
        <button class="btn ghost" @click="onClearFilters">清空</button>
        <button class="btn primary" @click="onSearch(filters.query)">搜索</button>
      </div>

      <div class="chips">
        <div class="group">
          <span class="label">状态</span>
          <button class="chip" :class="{ active: filters.status === 'pending' }" @click="setStatus('pending')">待审</button>
          <button class="chip" :class="{ active: filters.status === 'approved' }" @click="setStatus('approved')">已通过</button>
          <button class="chip" :class="{ active: filters.status === 'rejected' }" @click="setStatus('rejected')">已拒绝</button>
        </div>

        <div class="group">
          <span class="label">类型</span>
          <select class="select" v-model="filters.type" @change="onFilterChange">
            <option value="">全部</option>
            <option value="post">帖子</option>
            <option value="comment">评论</option>
            <option value="report">举报</option>
          </select>
        </div>

        <div class="group">
          <span class="label">时间</span>
          <input class="select" type="date" v-model="filters.dateRange.start" @change="onFilterChange" />
          <span class="dash">—</span>
          <input class="select" type="date" v-model="filters.dateRange.end" @change="onFilterChange" />
        </div>

        <div class="spacer"></div>
        <button class="btn ghost" @click="onExport">导出</button>
      </div>
    </section>

    <!-- 批量操作 -->
    <div v-if="selectedIds.size > 0" class="bulk">
      <div>已选 {{ selectedIds.size }} 项</div>
      <div class="spacer"></div>
      <button class="btn ghost" @click="onClearSelection">清空</button>
      <button class="btn danger" @click="onRejectSelected">批量拒绝</button>
      <button class="btn primary" @click="onApproveSelected">批量通过</button>
    </div>

    <!-- 列表 -->
    <section class="section surface">
      <div class="list-head">
        <label class="check">
          <input type="checkbox" :checked="false" @change="onToggleSelectAll($event.target && $event.target.checked)" />
          全选
        </label>
        <div class="spacer"></div>
        <span class="muted">{{ viewMode === 'table' ? '表格模式' : '卡片模式' }}</span>
      </div>

      <div v-if="loading" class="loading">
        <div class="skeleton" v-for="i in 6" :key="i"></div>
      </div>

      <template v-else>
        <div v-if="items.length === 0" class="empty">
          <div class="ico">📭</div>
          <div class="t">暂无内容</div>
          <div class="d">调整筛选条件或稍后再试</div>
        </div>

        <table v-else-if="viewMode === 'table'" class="table">
          <thead>
            <tr>
              <th style="width:48px;"></th>
              <th>ID</th>
              <th>标题</th>
              <th>作者</th>
              <th>提交时间</th>
              <th>状态</th>
              <th style="width:220px;">操作</th>
            </tr>
          </thead>
          <tbody>
            <!--
            <tr v-for="item in items" :key="item.id">
              <td>
                <input type="checkbox" :checked="selectedIds.has(item.id)" @change="onToggleSelect(item.id, $event.target && $event.target.checked)" />
              </td>
              <td>{{ item.id }}</td>
              <td class="ellips">{{ item.title }}</td>
              <td>{{ item.author }}</td>
              <td>{{ item.submittedAt }}</td>
              <td><span class="badge">{{ item.status }}</span></td>
              <td class="row-actions">
                <button class="btn ghost" @click="onOpenDetail(item.id)">详情</button>
                <button class="btn" @click="onApproveOne(item.id)">通过</button>
                <button class="btn danger" @click="onRejectOne(item.id)">拒绝</button>
              </td>
            </tr>
            -->
          </tbody>
        </table>

        <div v-else class="cards">
          <!--
          <article class="card" v-for="item in items" :key="item.id">
            <header class="card-head">
              <label><input type="checkbox" :checked="selectedIds.has(item.id)" @change="onToggleSelect(item.id, $event.target && $event.target.checked)" /></label>
              <div class="meta"><span class="id">#{{ item.id }}</span><span class="dot"></span><span class="time">{{ item.submittedAt }}</span></div>
            </header>
            <div class="card-body">
              <h3 class="title ellips">{{ item.title }}</h3>
              <div class="sub">作者：{{ item.author }}</div>
              <div class="preview">预览占位…</div>
            </div>
            <footer class="card-foot">
              <button class="btn ghost" @click="onOpenDetail(item.id)">详情</button>
              <div class="spacer"></div>
              <button class="btn" @click="onApproveOne(item.id)">通过</button>
              <button class="btn danger" @click="onRejectOne(item.id)">拒绝</button>
            </footer>
          </article>
          -->
        </div>
      </template>
    </section>

    <!-- 分页 -->
    <section class="section pager">
      <div class="muted">共 {{ pagination.total }} 条 · 第 {{ pagination.page }} / {{ totalPages }} 页</div>
      <div class="spacer"></div>
      <button class="btn ghost" :disabled="pagination.page <= 1" @click="setPage(pagination.page - 1)">上一页</button>
      <button class="btn ghost" :disabled="pagination.page >= totalPages" @click="setPage(pagination.page + 1)">下一页</button>
      <div class="sep"></div>
      <label class="psize">每页
        <select :value="pagination.pageSize" @change="setPageSize($event.target && $event.target.value)">
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </label>
    </section>

    <!-- 详情抽屉 -->
    <teleport to="body">
      <div v-if="detail.open" class="drawer-overlay" @click.self="detail.open = false">
        <aside class="drawer">
          <header class="drawer-head">
            <div class="t">审核详情</div>
            <button class="btn ghost" @click="detail.open = false">关闭</button>
          </header>
          <section class="drawer-body">
            <div class="grid-2">
              <div class="card">
                <div class="card-title">内容信息</div>
                <div class="meta">
                  <div>内容ID：{{ detail.itemId != null ? detail.itemId : '-' }}</div>
                  <div>作者：—</div>
                  <div>提交时间：—</div>
                  <div>状态：—</div>
                </div>
                <div class="box">内容预览占位</div>
              </div>
              <div class="card">
                <div class="card-title">审核记录</div>
                <div class="box">记录占位</div>
              </div>
            </div>
          </section>
          <footer class="drawer-foot">
            <button class="btn danger" :disabled="!detail.itemId" @click="onRejectOne(detail.itemId)">拒绝</button>
            <button class="btn" :disabled="!detail.itemId" @click="onApproveOne(detail.itemId)">通过</button>
          </footer>
        </aside>
      </div>
    </teleport>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'

const viewMode = ref('table')

const filters = reactive({
  query: '',
  status: 'pending',
  type: '',
  dateRange: { start: '', end: '' },
})

const items = ref([]) // 数据占位
const loading = ref(false)
const selectedIds = reactive(new Set())

const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0,
})
const totalPages = computed(() => {
  const size = pagination.pageSize || 10
  return Math.max(1, Math.ceil((pagination.total || 0) / size))
})

const detail = reactive({
  open: false,
  itemId: null,
})

function setStatus(v) { filters.status = v; onFilterChange() }
function onFilterChange() { /* TODO */ }
function onSearch(q) { filters.query = q /* TODO */ }
function onClearFilters() {
  filters.query = ''; filters.status = 'pending'; filters.type = ''; filters.dateRange = { start: '', end: '' }
}

function onToggleSelectAll(checked) { if (!checked) return selectedIds.clear(); /* items.value.forEach(it => selectedIds.add(it.id)) */ }
function onToggleSelect(id, checked) { if (checked) selectedIds.add(id); else selectedIds.delete(id) }
function onClearSelection() { selectedIds.clear() }

function onOpenDetail(id) { detail.itemId = id; detail.open = true }
function onApproveSelected() { /* TODO */ }
function onRejectSelected() { /* TODO */ }
function onApproveOne(id) { /* TODO */ }
function onRejectOne(id) { /* TODO */ }

function setPage(p) { pagination.page = Math.min(Math.max(1, p), totalPages.value) }
function setPageSize(val) { const size = parseInt(val, 10) || 10; pagination.pageSize = size; pagination.page = 1 }

function onRefresh() { /* TODO */ }
function onOpenSettings() { /* TODO */ }
function onExport() { /* TODO */ }
</script>

<style scoped>
.review { display: grid; gap: 12px; }

/* 工具栏 */
.toolbar { display: flex; align-items: center; justify-content: space-between; background: #fff; border: 1px solid rgba(17,24,39,.08); border-radius: 12px; padding: 10px 12px; }
.h2 { margin: 0; font-size: 18px; }
.sub { font-size: 12px; color: #64748b; margin-top: 2px; }
.right { display: flex; align-items: center; gap: 8px; }
.seg { display: inline-flex; gap: 4px; padding: 3px; border: 1px solid rgba(17,24,39,.12); border-radius: 10px; background: #f8fafc; }
.seg-btn { padding: 6px 10px; border-radius: 8px; border: 1px solid transparent; background: transparent; cursor: pointer; }
.seg-btn.active { background: #111827; color: #fff; border-color: #111827; }

/* 通用按钮 */
.btn { padding: 8px 12px; border-radius: 10px; border: 1px solid rgba(17,24,39,.12); background: #fff; cursor: pointer; }
.btn:hover { filter: brightness(0.98); }
.btn.ghost { background: transparent; }
.btn.primary { background: #111827; color: #fff; }
.btn.danger { background: #ef4444; color: #fff; border-color: #ef4444; }

/* 筛选 */
.section { background: #fff; border: 1px solid rgba(17,24,39,.08); border-radius: 12px; padding: 10px; }
.filter { display: grid; gap: 10px; }
.search { display: flex; align-items: center; gap: 8px; border: 1px solid rgba(17,24,39,.12); border-radius: 10px; padding: 6px 8px; background: #fff; }
.search-input { flex: 1; border: none; outline: none; background: transparent; }
.chips { display: flex; align-items: center; gap: 10px; }
.group { display: inline-flex; align-items: center; gap: 8px; padding: 4px 8px; border-radius: 999px; background: #f8fafc; border: 1px solid rgba(17,24,39,.12); }
.label { font-size: 12px; color: #64748b; }
.chip { padding: 4px 10px; border-radius: 999px; border: 1px solid transparent; background: transparent; cursor: pointer; }
.chip.active { background: #111827; color: #fff; }
.select { height: 32px; border-radius: 8px; border: 1px solid rgba(17,24,39,.12); background: #fff; padding: 4px 8px; }

/* 批量条 */
.bulk { display: flex; align-items: center; gap: 8px; padding: 10px 12px; border: 1px dashed rgba(17,24,39,.12); border-radius: 12px; background: #fff; }

/* 列表 */
.surface { padding: 0; }
.list-head { display: flex; align-items: center; gap: 10px; padding: 10px; border-bottom: 1px dashed rgba(17,24,39,.12); }
.check { display: inline-flex; align-items: center; gap: 8px; }
.muted { color: #64748b; font-size: 12px; }
.spacer { flex: 1; }
.empty { display: grid; justify-items: center; gap: 6px; padding: 28px 8px; color: #64748b; }
.empty .t { color: #0f172a; font-weight: 700; }
.loading .skeleton { height: 48px; margin: 8px 10px; border-radius: 10px; background: linear-gradient(90deg, #f1f5f9, #e2e8f0, #f1f5f9); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
@keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

.table { width: 100%; border-collapse: collapse; }
.table th, .table td { padding: 10px 8px; border-bottom: 1px solid #e5e7eb; text-align: left; vertical-align: top; }
.row-actions { display: flex; gap: 8px; }
.ellips { max-width: 520px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.badge { display: inline-block; padding: 2px 8px; border-radius: 999px; background: #eef2ff; color: #3730a3; border: 1px solid #c7d2fe; }

.cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 12px; padding: 10px; }
.card { border: 1px solid #e5e7eb; border-radius: 12px; background: #fff; overflow: hidden; display: flex; flex-direction: column; }
.card-head, .card-foot { display: flex; align-items: center; gap: 10px; padding: 10px; border-bottom: 1px solid #e5e7eb; }
.card-foot { border-bottom: none; border-top: 1px solid #e5e7eb; }
.card-body { padding: 10px; display: grid; gap: 8px; }
.meta { display: flex; align-items: center; gap: 8px; color: #64748b; font-size: 12px; }
.meta .dot { width: 6px; height: 6px; border-radius: 50%; background: #e5e7eb; }

/* 分页 */
.pager { display: flex; align-items: center; gap: 8px; border: 1px dashed #e5e7eb; border-radius: 12px; padding: 10px 12px; background: #fff; }
.sep { width: 1px; height: 18px; background: #e5e7eb; margin: 0 8px; }

/* 抽屉 */
.drawer-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; justify-content: flex-end; z-index: 50; }
.drawer { width: min(760px, 100%); height: 100%; background: #fff; display: grid; grid-template-rows: auto 1fr auto; border-left: 1px solid #e5e7eb; }
.drawer-head, .drawer-foot { display: flex; align-items: center; justify-content: space-between; padding: 12px; border-bottom: 1px solid #e5e7eb; }
.drawer-foot { border-bottom: none; border-top: 1px solid #e5e7eb; }
.drawer-body { padding: 12px; overflow: auto; display: grid; gap: 12px; }
.grid-2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
.card-title { font-weight: 700; padding: 10px 12px; border-bottom: 1px solid #e5e7eb; }
.box, .meta { background: #fafafa; border: 1px solid #e5e7eb; border-radius: 10px; padding: 12px; }
@media (max-width: 960px) { .grid-2 { grid-template-columns: 1fr; } }
</style>