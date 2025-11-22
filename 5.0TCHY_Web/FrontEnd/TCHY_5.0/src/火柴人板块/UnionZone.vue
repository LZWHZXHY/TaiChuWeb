<template>
  <section class="panel preview">
    <header class="panel-head">
      <div class="panel-title">
        <h2>柴圈联合板块</h2>
        <span class="tag">展示与发起（实时数据）</span>
      </div>

      <div class="tools">
        <div class="tabs" role="tablist" aria-label="活动筛选">
          <button type="button" class="tab" :class="{ active: tab === 'ongoing' }" @click="switchTab('ongoing')">进行中</button>
          <button type="button" class="tab" :class="{ active: tab === 'finished' }" @click="switchTab('finished')">已结束</button>
        </div>

        <input v-model="q" class="input" type="search" placeholder="搜索：名称 / 主办方 / 规则…" @input="onSearchInput" />

        <select v-model="typeFilter" class="select" @change="onFilterChange">
          <option value="">全部类型</option>
          <option :value="1">联合</option>
          <option :value="2">接力</option>
          <option :value="3">竞标赛</option>
        </select>

        <div class="actions">
          <button type="button" class="btn primary" @click="openCreate">申请发起联合</button>
        </div>
      </div>
    </header>

    <div class="table-wrap" role="region" aria-labelledby="events-table">
      <table class="table" id="events-table" aria-live="polite">
        <tbody>
          <tr v-if="loading">
            <td class="td empty" colspan="7">
              <div class="loading-spinner">
                <div class="spinner"></div>
                <span>加载中…</span>
              </div>
            </td>
          </tr>

          <tr v-else-if="!items.length">
            <td class="td empty" colspan="7">
              <div class="empty-state">
                <div class="empty-icon">🗓️</div>
                <div class="empty-text">
                  <strong>暂无活动</strong>
                  <span>试试切换标签或发起新的联合。</span>
                </div>
              </div>
            </td>
          </tr>

          <tr v-else v-for="it in items" :key="it.id" class="card-row">
            <td class="card-cell" colspan="7">
              <article class="event-card" :class="`status-${it.verify}`">
                <div class="card-left">
                  <div class="card-header">
                    <div class="label-row">
                      <span class="type-pill" :class="`type-${it.type}`">{{ typeLabel(it.type) }}</span>
                      <h3 class="event-name" :title="it.name">{{ it.name }}</h3>
                    </div>
                    <div class="verify-badge" :class="`v${it.verify}`">{{ verifyLabel(it.verify) }}</div>
                  </div>

                  <div class="meta">
                    <span class="meta-item">
                      <span class="meta-icon">👤</span>
                      <b>发起者</b> {{ it.host }}
                    </span>
                    <span class="meta-item">
                      <span class="meta-icon">📅</span>
                      <b>发起时间</b> {{ shortDate(it.startdate) }}
                    </span>
                    <span class="meta-item">
                      <span class="meta-icon">⏱️</span>
                      <b>预计结束</b> {{ it.enddate ? shortDate(it.enddate) : '—' }}
                    </span>
                  </div>

                  <div class="desc-container">
                    <p class="desc" :class="{ 'clamp-2': !isDescOpen(it.id) }">{{ it.desc }}</p>
                    <button class="toggle-desc-btn" @click="toggleDesc(it.id)">
                      {{ isDescOpen(it.id) ? '收起' : '展开' }}
                    </button>
                  </div>

                  <div v-if="isDescOpen(it.id)" class="rules-block">
                    <div class="rules-header">
                      <span class="rules-icon">📋</span>
                      <strong>规则 & 要求</strong>
                    </div>
                    <p class="rules">{{ it.rules || '无额外规则' }}</p>
                  </div>

                  <div class="card-footer">
                    <div class="time-range">
                      <span class="time-label">活动时间：</span>
                      <span class="time-value">{{ formatDate(it.startdate) }} — {{ it.enddate ? formatDate(it.enddate) : '—' }}</span>
                    </div>

                    <div class="card-actions">
                      <button class="btn small outline" @click="viewDetail(it)">
                        <span class="btn-icon">🔍</span>
                        详情
                      </button>
                      <button class="btn small primary" @click="applyJoin(it)" :disabled="!it.qQgroup">
                        <span class="btn-icon">👥</span>
                        申请加入
                      </button>
                      <button class="btn small success" @click="applyForEvent(it)">
                        <span class="btn-icon">📝</span>
                        申请联合
                      </button>
                      <button v-if="isMine(it)" class="btn small secondary" @click="editItem(it)">
                        <span class="btn-icon">✏️</span>
                        编辑
                      </button>
                      <button v-if="isAdmin && it.verify !== 1" class="btn small success" @click="approveItem(it)">
                        <span class="btn-icon">✓</span>
                        通过
                      </button>
                      <button v-if="isAdmin && it.verify !== 2" class="btn small danger" @click="rejectItem(it)">
                        <span class="btn-icon">✕</span>
                        驳回
                      </button>
                    </div>
                  </div>
                </div>

                <div class="card-right">
                  <div class="qqgroup" v-if="it.qQgroup">
                    <div class="qqgroup-label">
                      <span class="qq-icon">💬</span>
                      <b>QQ群号</b>
                    </div>
                    <div class="qq-number">{{ it.qQgroup }}</div>
                  </div>
                </div>
              </article>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer class="panel-foot">
      <div class="pager">
        <button class="btn ghost small" @click="prevPage" :disabled="page<=1">
          <span class="pager-icon">←</span>
          上一页
        </button>
        <span class="page-text">第 {{ page }} / {{ totalPages }} 页</span>
        <button class="btn ghost small" @click="nextPage" :disabled="page>= totalPages">
          下一页
          <span class="pager-icon">→</span>
        </button>
      </div>
    </footer>

    <!-- detail modal -->
    <div v-if="detail" class="modal-backdrop" @click.self="detail = null">
      <div class="modal">
        <div class="modal-header">
          <h3>{{ detail.name }}</h3>
          <button class="modal-close" @click="detail = null">×</button>
        </div>
        <div class="modal-body">
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">发起者：</span>
              <span class="detail-value">{{ detail.host }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">类型：</span>
              <span class="detail-value">{{ typeLabel(detail.type) }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">时间：</span>
              <span class="detail-value">{{ formatDate(detail.startdate) }} — {{ detail.enddate ? formatDate(detail.enddate) : '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">群号：</span>
              <span class="detail-value">{{ detail.qQgroup || '-' }}</span>
            </div>
          </div>
          <hr />
          <div class="detail-section">
            <h4>简介</h4>
            <div class="detail-content">{{ detail.desc }}</div>
          </div>
          <div class="detail-section">
            <h4>规则与要求</h4>
            <div class="detail-content">{{ detail.rules || '无' }}</div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn outline" @click="detail = null">关闭</button>
          <button class="btn primary" @click="applyForEvent(detail)">
            <span class="btn-icon">📝</span>
            申请联合
          </button>
        </div>
      </div>
    </div>

    <!-- create/edit modal -->
    <div v-if="showCreate" class="modal-backdrop" @click.self="closeCreate">
      <div class="modal form-modal">
        <div class="modal-header">
          <h3>{{ editMode ? '编辑联合' : '申请发起联合' }}</h3>
          <button class="modal-close" @click="closeCreate">×</button>
        </div>

        <form @submit.prevent="submitForm">
          <div class="form-body">
            <div class="form-row">
              <label>类型</label>
              <select v-model.number="form.type" class="select">
                <option :value="1">联合</option>
                <option :value="2">接力</option>
                <option :value="3">竞标赛</option>
              </select>
            </div>

            <div class="form-row">
              <label>标题（名称）</label>
              <input v-model="form.name" class="input" />
            </div>

            <div class="form-row grid-2">
              <div>
                <label>发起者（主办）</label>
                <input v-model="form.host" class="input" />
              </div>
              <div>
                <label>群号 (可选)</label>
                <input v-model="form.qQgroup" class="input" />
              </div>
            </div>

            <div class="form-row grid-2">
              <div>
                <label>发起时间</label>
                <input v-model="form.startdate" type="datetime-local" class="input" />
              </div>
              <div>
                <label>预计结束时间</label>
                <input v-model="form.enddate" type="datetime-local" class="input" />
              </div>
            </div>

            <div class="form-row">
              <label>内容介绍</label>
              <div class="textarea-container">
                <textarea v-model="form.desc" class="textarea" rows="6" placeholder="请详细描述活动内容、目的、参与方式等..."></textarea>
              </div>
            </div>

            <div class="form-row">
              <label>规则与要求</label>
              <div class="textarea-container">
                <textarea v-model="form.rules" class="textarea" rows="6" placeholder="例如：作品尺寸、格式要求、截止时间、投稿方式、评分规则等"></textarea>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button class="btn outline" type="button" @click="closeCreate">取消</button>
            <button class="btn primary" type="submit" :disabled="submitting">
              {{ submitting ? '处理中…' : (editMode ? '保存' : '提交申请') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- 申请联合模态框 -->
    <div v-if="showApplyModal" class="modal-backdrop" @click.self="closeApplyModal">
      <div class="modal form-modal">
        <div class="modal-header">
          <h3>申请联合 - {{ selectedEvent?.name }}</h3>
          <button class="modal-close" @click="closeApplyModal">×</button>
        </div>

        <form @submit.prevent="submitApplication">
          <div class="form-body">
            <div class="form-row">
              <label>申请人姓名</label>
              <input v-model="applicationForm.applicantName" class="input" placeholder="请输入您的姓名" required />
            </div>

            <div class="form-row">
              <label>联系方式</label>
              <input v-model="applicationForm.contact" class="input" placeholder="请输入QQ号或手机号" required />
            </div>

            <div class="form-row">
              <label>申请说明</label>
              <div class="textarea-container">
                <textarea v-model="applicationForm.description" class="textarea" rows="4" placeholder="请简要说明您的申请理由和相关经验..." required></textarea>
              </div>
            </div>

            <div class="form-row">
              <label>作品链接 (可选)</label>
              <input v-model="applicationForm.portfolioUrl" class="input" placeholder="如有相关作品，请提供链接" />
            </div>

            <div class="form-row" v-if="selectedEvent">
              <div class="application-info">
                <h4>申请活动信息</h4>
                <p><strong>活动名称：</strong>{{ selectedEvent.name }}</p>
                <p><strong>活动类型：</strong>{{ typeLabel(selectedEvent.type) }}</p>
                <p><strong>主办方：</strong>{{ selectedEvent.host }}</p>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button class="btn outline" type="button" @click="closeApplyModal">取消</button>
            <button class="btn primary" type="submit" :disabled="submittingApplication">
              {{ submittingApplication ? '提交中…' : '提交申请' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const auth = useAuthStore()

// state & ui
const tab = ref('ongoing')
const q = ref('')
const typeFilter = ref('')
const page = ref(1)
const pageSize = ref(8)

const items = ref([])
const total = ref(0)
const loading = ref(false)

const expandedDesc = ref(new Set())
const detail = ref(null)
const showCreate = ref(false)
const editMode = ref(false)
const submitting = ref(false)

// 添加申请相关的状态
const showApplyModal = ref(false)
const selectedEvent = ref(null)
const submittingApplication = ref(false)

const applicationForm = reactive({
  applicantName: '',
  contact: '',
  description: '',
  portfolioUrl: ''
})

const form = reactive({
  id: null,
  name: '',
  host: '',
  type: 1, // 默认改为联合
  startdate: '',
  enddate: '',
  desc: '',
  rules: '',
  qQgroup: '', // 修正为小写q大写Q
  verify: 0,
  creatorId: null
})

// convenience - 修正类型映射
const TYPE_MAP = { 1: '联合', 2: '接力', 3: '竞标赛' }
function typeLabel(t) { return TYPE_MAP[t] ?? `类型 ${t}` }
function formatDate(d) { if (!d) return '-'; try { return new Date(d).toLocaleString() } catch { return d } }
function shortDate(d) { if (!d) return '-'; try { return new Date(d).toLocaleDateString() } catch { return d } }
function verifyLabel(v) { return v === 0 ? '待审' : v === 1 ? '通过' : '拒绝' }
function isDescOpen(id) { return expandedDesc.value.has(id) }
function toggleDesc(id) { const set = new Set(expandedDesc.value); set.has(id) ? set.delete(id) : set.add(id); expandedDesc.value = set }
function isMine(it) { return auth.user?.id && it.creatorId === auth.user.id }
const isAdmin = computed(() => !!auth.user?.roles?.includes?.('Admin'))

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

// ---- Inline API methods ----
const basePath = 'ChaiLianHe' // controller route

async function listApi(params) {
  return apiClient.get(`${basePath}/list`, { params })
}
async function getApi(id) {
  return apiClient.get(`${basePath}/${id}`)
}
async function createApi(payload) {
  return apiClient.post(`${basePath}/create`, payload)
}
async function updateApi(id, payload) {
  return apiClient.put(`${basePath}/${id}`, payload)
}
async function deleteApi(id) {
  return apiClient.delete(`${basePath}/${id}`)
}
async function approveApi(id) {
  return apiClient.post(`${basePath}/moderation/approve`, { Id: id })
}
async function rejectApi(id, note) {
  return apiClient.post(`${basePath}/moderation/reject`, { Id: id, Note: note })
}

// 添加申请联合的API
async function applyForEventApi(payload) {
  return apiClient.post(`${basePath}/apply`, payload)
}

// ---- behaviors ----
async function ensureAuth() {
  if (!auth.checkAuth()) {
    auth.clearAuthState()
    router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } })
    return false
  }
  return true
}

async function fetchList() {
  if (!(await ensureAuth())) return
  loading.value = true
  try {
    const params = {
      q: q.value || undefined,
      type: typeFilter.value !== '' ? typeFilter.value : undefined,
      verify: 1, // 只显示已通过的活动
      page: page.value,
      pageSize: pageSize.value
    }
    const resp = await listApi(params)
    const payload = resp.data
    if (payload && Array.isArray(payload.data)) {
      items.value = payload.data
      total.value = payload.total ?? payload.data.length
    } else if (Array.isArray(payload)) {
      items.value = payload
      total.value = items.value.length
    } else {
      items.value = []
      total.value = 0
    }
  } catch (err) {
    const status = err?.response?.status
    if (status === 401) {
      auth.clearAuthState()
      alert('请先登录')
      router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } })
    } else if (status === 403) {
      alert('权限不足')
    } else {
      console.error('fetchList error', err)
      alert('获取列表失败')
    }
    items.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

async function viewDetail(it) {
  try {
    const resp = await getApi(it.id)
    detail.value = resp.data
    // 确保群号正确显示
    if (detail.value && detail.value.qQgroup === undefined) {
      detail.value.qQgroup = it.qQgroup
    }
  } catch {
    detail.value = it
  }
}

function applyJoin(it) {
  if (!it.qQgroup) { alert('该活动未公开群号'); return }
  window.open(`https://jq.qq.com/?_=${encodeURIComponent(it.qQgroup)}`, '_blank', 'noopener')
}

// 添加申请联合功能
function applyForEvent(event) {
  if (!auth.user) {
    alert('请先登录')
    router.push({ name: 'Login' })
    return
  }
  
  selectedEvent.value = event
  // 初始化表单数据
  Object.assign(applicationForm, {
    applicantName: auth.user.username || '',
    contact: '',
    description: '',
    portfolioUrl: ''
  })
  showApplyModal.value = true
}

function closeApplyModal() {
  showApplyModal.value = false
  selectedEvent.value = null
  submittingApplication.value = false
}

async function submitApplication() {
  if (!(await ensureAuth())) return
  
  if (!applicationForm.applicantName.trim() || !applicationForm.contact.trim() || !applicationForm.description.trim()) {
    alert('请填写必填项')
    return
  }
  
  submittingApplication.value = true
  try {
    const payload = {
      eventId: selectedEvent.value.id,
      eventName: selectedEvent.value.name,
      applicantName: applicationForm.applicantName.trim(),
      contact: applicationForm.contact.trim(),
      description: applicationForm.description.trim(),
      portfolioUrl: applicationForm.portfolioUrl?.trim() || '',
      applyTime: new Date().toISOString()
    }
    
    await applyForEventApi(payload)
    alert('申请提交成功！主办方将会尽快与您联系。')
    closeApplyModal()
  } catch (err) {
    const status = err?.response?.status
    if (status === 401) {
      auth.clearAuthState()
      alert('请先登录')
      router.push({ name: 'Login' })
    } else if (status === 403) {
      alert('权限不足')
    } else {
      console.error('submitApplication error', err)
      alert('提交申请失败，请稍后重试')
    }
  } finally {
    submittingApplication.value = false
  }
}

function openCreate() {
  editMode.value = false
  Object.assign(form, { 
    id: null, 
    name: '', 
    host: auth.user?.username ?? '', 
    type: 1, 
    startdate: '', 
    enddate: '', 
    desc: '', 
    rules: '', 
    qQgroup: '', // 修正为小写q大写Q
    verify: 0, 
    creatorId: auth.user?.id ?? null 
  })
  showCreate.value = true
}

function closeCreate() {
  showCreate.value = false
  submitting.value = false
}

async function editItem(it) {
  try {
    const resp = await getApi(it.id)
    const data = resp.data
    editMode.value = true
    Object.assign(form, {
      id: data.id,
      name: data.name,
      host: data.host,
      type: data.type,
      startdate: data.startdate ? data.startdate.substring(0,16) : '',
      enddate: data.enddate ? data.enddate.substring(0,16) : '',
      desc: data.desc || '',
      rules: data.rules || '',
      qQgroup: data.qQgroup || '', // 修正为小写q大写Q
      verify: data.verify,
      creatorId: auth.user?.id ?? null
    })
    showCreate.value = true
  } catch (err) {
    alert('获取详情失败，无法编辑')
  }
}

async function submitForm() {
  if (!(await ensureAuth())) return
  if (!form.name.trim() || !form.host.trim() || !form.startdate) {
    alert('请填写必填项')
    return
  }
  submitting.value = true
  try {
    const payload = {
      name: form.name.trim(),
      host: form.host.trim(),
      type: form.type,
      startdate: new Date(form.startdate).toISOString(),
      enddate: form.enddate ? new Date(form.enddate).toISOString() : null,
      desc: form.desc?.trim() ?? '',
      qQgroup: form.qQgroup?.trim() ?? '', // 修正为小写q大写Q
      rules: form.rules?.trim() ?? ''
    }

    if (editMode.value && form.id) {
      await updateApi(form.id, payload)
      alert('保存成功')
    } else {
      await createApi(payload)
      alert('提交成功，已进入审核流程')
    }

    await fetchList()
    showCreate.value = false
  } catch (err) {
    const status = err?.response?.status
    if (status === 401) { auth.clearAuthState(); router.push({ name: 'Login' }) }
    else if (status === 403) alert('权限不足')
    else alert(err.message || '提交失败')
  } finally {
    submitting.value = false
  }
}

async function deleteItem(id) {
  if (!confirm('确认删除？')) return
  try {
    await deleteApi(id)
    await fetchList()
    alert('删除成功')
  } catch (err) {
    alert('删除失败')
  }
}

async function approveItem(it) {
  if (!confirm(`确认通过：${it.name}？`)) return
  try {
    await approveApi(it.id)
    await fetchList()
    alert('已通过')
  } catch (err) {
    alert('操作失败')
  }
}

async function rejectItem(it) {
  const note = prompt('请输入驳回原因（可选）')
  if (note === null) return
  try {
    await rejectApi(it.id, note)
    await fetchList()
    alert('已驳回')
  } catch {
    alert('操作失败')
  }
}

// search debounce / pagination
let debounceTimer = null
function onSearchInput() { page.value = 1; clearTimeout(debounceTimer); debounceTimer = setTimeout(fetchList, 300) }
function onFilterChange() { page.value = 1; fetchList() }
function switchTab(t) { tab.value = t; page.value = 1; fetchList() }
function prevPage() { if (page.value > 1) { page.value--; fetchList() } }
function nextPage() { if (page.value < totalPages.value) { page.value++; fetchList() } }

// init
onMounted(() => {
  auth.checkAuth()
  fetchList()
})
</script>

<style scoped>
/* 基础变量 - 优化配色方案 */
:root {
  --primary-color: #4f46e5;
  --primary-light: #e0e7ff;
  --primary-dark: #3730a3;
  --secondary-color: #6b7280;
  --success-color: #10b981;
  --warning-color: #f59e0b;
  --danger-color: #ef4444;
  --light-bg: #f8fafc;
  --card-bg: #ffffff;
  --border-color: #e5e7eb;
  --text-primary: #111827;
  --text-secondary: #6b7280;
  --text-light: #9ca3af;
  --shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  --shadow-card: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  --radius: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background-color: #f1f5f9;
  color: var(--text-primary);
  line-height: 1.5;
}

/* 主面板样式 - 优化视觉效果 */
.panel.preview {
  max-width: 1200px;
  margin: 24px auto;
  background: var(--card-bg);
  border-radius: var(--radius-xl);
  padding: 24px;
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--border-color);
}

.panel-head {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 0 0 20px;
  border-bottom: 1px solid var(--border-color);
  margin-bottom: 24px;
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 12px;
}

.panel-title h2 {
  margin: 0;
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  background: linear-gradient(135deg, var(--primary-color), #7c3aed);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.tag {
  background: linear-gradient(135deg, var(--primary-light), #e0e7ff);
  color: var(--primary-dark);
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  border: 1px solid rgba(79, 70, 229, 0.1);
}

.tools {
  display: flex;
  gap: 16px;
  align-items: center;
  flex-wrap: wrap;
}

.tabs {
  display: flex;
  background: var(--light-bg);
  border-radius: var(--radius);
  padding: 4px;
  border: 1px solid var(--border-color);
}

.tab {
  padding: 10px 20px;
  border: none;
  background: transparent;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: var(--transition);
  color: var(--text-secondary);
}

.tab.active {
  background: var(--card-bg);
  box-shadow: var(--shadow);
  color: var(--primary-color);
  font-weight: 600;
}

.tab:hover:not(.active) {
  background: rgba(255, 255, 255, 0.8);
  color: var(--primary-color);
}

.input, .select {
  padding: 10px 14px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius);
  font-size: 14px;
  transition: var(--transition);
  background: transparent; /* 透明背景 */
  min-width: 180px;
}

.input:focus, .select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  transform: translateY(-1px);
}

.input::placeholder {
  color: var(--text-light);
}

.actions {
  margin-left: auto;
}

/* 按钮样式 - 优化视觉效果 */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: var(--radius);
  border: 1px solid transparent;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition);
  text-decoration: none;
  white-space: nowrap;
  position: relative;
  overflow: hidden;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none !important;
}

.btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: left 0.5s;
}

.btn:hover:not(:disabled)::before {
  left: 100%;
}

.btn.primary {
  background: linear-gradient(135deg, var(--primary-color), #7c3aed);
  color: rgb(0, 0, 0);
  border-color: var(--primary-color);
  box-shadow: 0 2px 4px rgba(79, 70, 229, 0.2);
}

.btn.primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(79, 70, 229, 0.3);
}

.btn.secondary {
  background: linear-gradient(135deg, #f3f4f6, #e5e7eb);
  color: var(--text-secondary);
  border-color: #d1d5db;
}

.btn.secondary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.btn.success {
  background: linear-gradient(135deg, var(--success-color), #34d399);
  color: white;
  border-color: var(--success-color);
}

.btn.success:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(16, 185, 129, 0.2);
}

.btn.danger {
  background: linear-gradient(135deg, var(--danger-color), #f87171);
  color: white;
  border-color: var(--danger-color);
}

.btn.danger:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(239, 68, 68, 0.2);
}

.btn.outline {
  background: transparent;
  color: var(--primary-color);
  border-color: var(--primary-color);
}

.btn.outline:hover:not(:disabled) {
  background: var(--primary-color);
  color: white;
  transform: translateY(-1px);
}

.btn.ghost {
  background: transparent;
  color: var(--text-secondary);
  border-color: transparent;
}

.btn.ghost:hover:not(:disabled) {
  background: var(--light-bg);
  color: var(--primary-color);
}

.btn.small {
  padding: 8px 16px;
  font-size: 13px;
  gap: 6px;
}

.btn-icon {
  font-size: 14px;
}

/* 表格和卡片样式 - 优化视觉效果 */
.table-wrap {
  margin-top: 20px;
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.table {
  width: 100%;
  border-collapse: collapse;
  min-width: 800px;
}

.card-cell {
  padding: 0;
  border-bottom: 1px solid var(--border-color);
}

.card-row:last-child .card-cell {
  border-bottom: none;
}

.event-card {
  display: flex;
  gap: 24px;
  padding: 24px;
  transition: var(--transition);
  position: relative;
  background: var(--card-bg);
}

.event-card:hover {
  background: #fafbfc;
  transform: translateY(-2px);
  box-shadow: var(--shadow-card);
}

.event-card.status-0 {
  border-left: 4px solid var(--warning-color);
}

.event-card.status-1 {
  border-left: 4px solid var(--success-color);
}

.event-card.status-2 {
  border-left: 4px solid var(--danger-color);
}

.card-left {
  flex: 1;
  min-width: 0;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
  gap: 12px;
}

.label-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

/* 类型标签样式 - 为不同类型设置不同颜色 */
.type-pill {
  font-size: 12px;
  padding: 6px 12px;
  border-radius: 20px;
  font-weight: 700;
  white-space: nowrap;
  flex-shrink: 0;
  border: 1px solid;
}

.type-pill.type-1 { /* 联合 */
  background: linear-gradient(135deg, #e0e7ff, #c7d2fe);
  color: #3730a3;
  border-color: #a5b4fc;
}

.type-pill.type-2 { /* 接力 */
  background: linear-gradient(135deg, #f0fdf4, #dcfce7);
  color: #166534;
  border-color: #86efac;
}

.type-pill.type-3 { /* 竞标赛 */
  background: linear-gradient(135deg, #fef7f1, #fed7aa);
  color: #9a3412;
  border-color: #fdba74;
}

.event-name {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text-primary);
}

.verify-badge {
  font-size: 12px;
  padding: 6px 12px;
  border-radius: 20px;
  font-weight: 600;
  flex-shrink: 0;
  border: 1px solid;
}

.verify-badge.v0 {
  background: #fffbeb;
  color: #d97706;
  border-color: #fcd34d;
}

.verify-badge.v1 {
  background: #f0fdf4;
  color: #166534;
  border-color: #86efac;
}

.verify-badge.v2 {
  background: #fef2f2;
  color: #dc2626;
  border-color: #fca5a5;
}

.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  margin-bottom: 16px;
  color: var(--text-secondary);
  font-size: 14px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  background: var(--light-bg);
  padding: 6px 12px;
  border-radius: 20px;
}

.meta-icon {
  font-size: 14px;
}

.desc-container {
  margin-bottom: 16px;
  position: relative;
}

.desc {
  margin: 0;
  color: var(--text-primary);
  font-size: 14px;
  line-height: 1.6;
}

.clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.toggle-desc-btn {
  background: none;
  border: none;
  color: var(--primary-color);
  font-size: 13px;
  cursor: pointer;
  margin-top: 8px;
  padding: 4px 8px;
  font-weight: 600;
  border-radius: 4px;
  transition: var(--transition);
}

.toggle-desc-btn:hover {
  background: var(--primary-light);
}

.rules-block {
  margin: 16px 0;
  padding: 16px;
  border-radius: var(--radius);
  background: var(--light-bg);
  border-left: 4px solid var(--primary-color);
}

.rules-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.rules-icon {
  font-size: 16px;
}

.rules {
  margin: 0;
  font-size: 14px;
  line-height: 1.6;
  color: var(--text-secondary);
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  flex-wrap: wrap;
  gap: 16px;
}

.time-range {
  font-size: 13px;
  color: var(--text-secondary);
  background: var(--light-bg);
  padding: 8px 12px;
  border-radius: var(--radius);
}

.time-label {
  font-weight: 600;
  color: var(--text-primary);
}

.card-actions {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.card-right {
  width: 160px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 16px;
  flex-shrink: 0;
}

.qqgroup {
  text-align: center;
  padding: 16px;
  border-radius: var(--radius);
  background: linear-gradient(135deg, var(--light-bg), #f1f5f9);
  width: 100%;
  border: 1px solid var(--border-color);
}

.qqgroup-label {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-bottom: 8px;
  font-size: 13px;
  color: var(--text-secondary);
}

.qq-icon {
  font-size: 16px;
}

.qq-number {
  font-size: 18px;
  font-weight: 700;
  color: var(--primary-color);
  background: white;
  padding: 8px;
  border-radius: var(--radius);
  border: 1px solid var(--border-color);
}

/* 加载和空状态 - 优化视觉效果 */
.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 40px;
  gap: 16px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--border-color);
  border-top: 3px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 40px;
  gap: 16px;
  text-align: center;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 16px;
  opacity: 0.7;
}

.empty-text {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.empty-text strong {
  font-size: 18px;
  color: var(--text-primary);
  font-weight: 600;
}

.empty-text span {
  font-size: 14px;
  color: var(--text-secondary);
}

/* 分页器 - 优化视觉效果 */
.panel-foot {
  display: flex;
  justify-content: center;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid var(--border-color);
}

.pager {
  display: flex;
  align-items: center;
  gap: 20px;
}

.page-text {
  font-size: 14px;
  color: var(--text-secondary);
  font-weight: 600;
  background: var(--light-bg);
  padding: 8px 16px;
  border-radius: var(--radius);
}

.pager-icon {
  font-size: 14px;
}

/* 模态框样式 - 优化视觉效果 */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  z-index: 1000;
  padding: 20px;
  animation: backdropFadeIn 0.3s ease-out;
}

@keyframes backdropFadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal {
  width: 100%;
  max-width: 600px;
  background: var(--card-bg);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-lg);
  max-height: 90vh;
  overflow: auto;
  animation: modalSlideIn 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid var(--border-color);
  transform-origin: center;
}

@keyframes modalSlideIn {
  from { opacity: 0; transform: translateY(-30px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 24px 0;
  margin-bottom: 24px;
}

.modal-header h3 {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary);
}

.modal-close {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: var(--text-light);
  padding: 0;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: var(--transition);
}

.modal-close:hover {
  background: var(--light-bg);
  color: var(--text-primary);
  transform: rotate(90deg);
}

.modal-body {
  padding: 0 24px;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  background: var(--light-bg);
  padding: 12px;
  border-radius: var(--radius);
}

.detail-label {
  font-size: 13px;
  color: var(--text-secondary);
  font-weight: 600;
}

.detail-value {
  font-size: 14px;
  color: var(--text-primary);
  font-weight: 500;
}

.detail-section {
  margin-bottom: 24px;
}

.detail-section h4 {
  margin: 0 0 12px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  padding-bottom: 8px;
  border-bottom: 2px solid var(--primary-light);
}

.detail-content {
  font-size: 14px;
  line-height: 1.6;
  color: var(--text-secondary);
  white-space: pre-wrap;
  background: var(--light-bg);
  padding: 16px;
  border-radius: var(--radius);
  border-left: 3px solid var(--primary-color);
}

.modal-footer {
  padding: 24px;
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid var(--border-color);
  margin-top: 24px;
}

/* 表单模态框 */
.form-modal {
  max-width: 700px;
  color: white;
}

.form-body {
  padding: 0 24px;
}

.form-row {
  margin-bottom: 24px;
}

.form-row label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: var(--text-primary);
  font-size: 14px;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

/* 文本区域容器 - 修改为透明背景 */
.textarea-container {
  position: relative;
  background: transparent; /* 改为透明背景 */
  border-radius: var(--radius);
  border: 1px solid var(--border-color);
  transition: var(--transition);
}

.textarea-container:focus-within {
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  transform: translateY(-1px);
}

/* 文本区域样式 - 修改为透明背景 */
.textarea {
  width: 100%;
  min-height: 120px;
  border: none;
  background: transparent; /* 改为透明背景 */
  resize: vertical;
  line-height: 1.6;
  padding: 12px 14px; /* 与输入框保持一致的内边距 */
  font-size: 14px;
  border-radius: var(--radius);
  font-family: inherit;
  color: var(--text-primary);
}

.textarea:focus {
  outline: none;
  box-shadow: none;
}

.textarea::placeholder {
  color: var(--text-light);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 16px;
  padding: 24px;
  border-top: 1px solid var(--border-color);
  margin-top: 24px;
}

/* 申请信息样式 */
.application-info {
  background: var(--light-bg);
  padding: 16px;
  border-radius: var(--radius);
  border-left: 4px solid var(--primary-color);
}

.application-info h4 {
  margin: 0 0 12px;
  color: var(--text-primary);
  font-size: 16px;
  font-weight: 600;
}

.application-info p {
  margin: 8px 0;
  color: var(--text-secondary);
  font-size: 14px;
}

.application-info strong {
  color: var(--text-primary);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .panel.preview {
    margin: 16px;
    padding: 20px;
    border-radius: var(--radius-lg);
  }
  
  .tools {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .tabs {
    align-self: stretch;
    justify-content: center;
  }
  
  .actions {
    margin-left: 0;
    align-self: stretch;
  }
  
  .actions .btn {
    width: 100%;
    justify-content: center;
  }
  
  .event-card {
    flex-direction: column;
    gap: 20px;
    padding: 20px;
  }
  
  .card-right {
    width: 100%;
    align-items: stretch;
  }
  
  .qqgroup {
    width: auto;
  }
  
  .card-footer {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }
  
  .card-actions {
    justify-content: center;
  }
  
  .grid-2 {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .detail-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }
  
  .pager {
    flex-direction: column;
    gap: 16px;
  }
  
  .modal {
    margin: 16px;
    max-height: calc(100vh - 32px);
  }
  
  .modal-header {
    padding: 20px 20px 0;
  }
  
  .modal-body {
    padding: 0 20px;
  }
  
  .textarea {
    min-height: 100px;
    padding: 10px 12px;
  }
}

@media (max-width: 480px) {
  .panel-title {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .panel-title h2 {
    font-size: 24px;
  }
  
  .tabs {
    width: 100%;
  }
  
  .tab {
    flex: 1;
    text-align: center;
    padding: 12px 16px;
  }
  
  .card-actions {
    flex-direction: column;
    align-items: stretch;
  }
  
  .card-actions .btn {
    justify-content: center;
  }
  
  .meta {
    flex-direction: column;
    gap: 12px;
  }
  
  .meta-item {
    justify-content: center;
  }
}

/* 响应式设计调整 */
@media (max-width: 768px) {
  .card-actions {
    flex-direction: column;
    align-items: stretch;
  }
  
  .card-actions .btn {
    justify-content: center;
    margin-bottom: 8px;
  }
}

@media (max-width: 480px) {
  .card-actions {
    flex-direction: column;
  }
  
  .card-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
</style>