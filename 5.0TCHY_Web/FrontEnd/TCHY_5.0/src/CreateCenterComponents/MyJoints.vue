<template>
  <section class="view-section">
    <div class="section-header">
      <div class="title-group">
        <h2 class="cyber-title">OPERATION_MANAGEMENT // 联合行动管理</h2>
        <span class="sub-line">COMMANDER: {{ authStore.user?.username }}</span>
      </div>
      <div class="stats-row">
        <span class="stat">档案总数: {{ myJoints.length }}</span>
        <span class="stat highlight">运行中: {{ myJoints.filter(i => i.verify === 1).length }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-txt">DATA_SYNCING... [ 正在建立指挥链路 ]</div>
    
    <div class="cards-grid" v-else>
      <div 
        v-for="item in myJoints" 
        :key="item.id" 
        class="manage-card" 
        :class="[
          { 'is-pending': item.verify === 0 },
          { 'is-active': item.verify === 1 },
          { 'is-rejected': item.verify === 2 },
          { 'is-closed': item.verify === 3 }
        ]"
      >
        <div class="mc-status" :class="getStatusClass(item.verify)"></div>
        
        <div class="mc-content">
          <div class="mc-id">
            REF.OP-{{ padZero(item.id) }} 
            <span class="status-tag">[{{ getStatusText(item.verify) }}]</span>
          </div>
          <h3 class="mc-title">{{ item.name }}</h3>
          
          <UniversalTags :tags="item.tags" />

          <div class="mc-meta">
            <span>{{ typeLabelCn(item.type) }}</span>
            <span>发起于: {{ shortDate(item.startdate) }}</span>
          </div>
        </div>

        <div class="mc-actions-wrapper">
          <div class="utility-row">
            <button class="icon-btn" title="查看预览" @click="$router.push(`/joint/${item.id}`)">👁️</button>
            <button class="icon-btn" title="编辑方案" @click="handleEdit(item)">📝</button>
            <button class="icon-btn danger" title="撤回提案" @click="handleDelete(item)">🗑️</button>
          </div>
          
          <div class="command-row">
            <button class="cyber-btn sm primary" @click="openManageModal(item)" :disabled="item.verify !== 1">
              人员管理 <span v-if="item.pendingCount > 0" class="badge">{{ item.pendingCount }}</span>
            </button>

            <button v-if="item.verify === 1" class="cyber-btn sm outline" @click="handleStatusChange(item, 'close')">
              结案 // CLOSE
            </button>

            <button v-if="item.verify === 3" class="cyber-btn sm outline warn" @click="handleStatusChange(item, 'approve')">
              重启 // REOPEN
            </button>

            <span v-if="item.verify === 2" class="status-hint danger">方案被驳回</span>
          </div>
        </div>
      </div>
      
      <div class="manage-card add-new" @click="openCreateModal"> 
        <span class="plus">+</span>
        <span>INITIATE_NEW // 发起新行动提案</span>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showManageModal" class="cyber-modal-overlay" @click.self="closeModal">
          <div class="cyber-terminal console-mode">
            <div class="term-header">
              <div class="term-info">
                <span class="term-title">>> PERSONNEL_CONTROL // {{ selectedOp?.name }}</span>
                <span class="term-id">REF.OP-{{ padZero(selectedOp?.id) }}</span>
              </div>
              <button class="term-close" @click="closeModal">DISCONNECT_X</button>
            </div>

            <div class="terminal-stats-bar">
              <div class="ts-item">
                <span class="label">接入请求</span>
                <span class="val">{{ waitingApplicants.length }}</span>
              </div>
              <div class="ts-item">
                <span class="label">已部署单位</span>
                <span class="val">{{ authorizedMembers.length }}</span>
              </div>
              <div class="ts-item">
                <span class="label">数据交付率</span>
                <span class="val">{{ submissionRate }}%</span>
              </div>
            </div>

            <div class="terminal-nav">
              <button :class="{ active: currentTab === 'waitlist' }" @click="currentTab = 'waitlist'">
                WAITLIST [ 待核准信号 ]
              </button>
              <button :class="{ active: currentTab === 'authorized' }" @click="currentTab = 'authorized'">
                ROSTER [ 参战名册 ]
              </button>
            </div>
            
            <div class="console-body custom-scroll">
              <div v-if="currentTab === 'waitlist'" class="tab-pane">
                <div v-if="applicantsLoading" class="loading-txt">SIGNAL_SEARCHING...</div>
                <div v-else-if="waitingApplicants.length === 0" class="empty-txt">[ 暂无待处理的接入信号 ]</div>
                
                <div v-else v-for="app in waitingApplicants" :key="app.AppId" class="t-row applicant-row">
                  <div class="t-user">
                    <img :src="app.Avatar || '/default-avatar.png'" class="t-avatar">
                    <div class="t-info">
                      <div class="t-name">{{ app.Username }}</div>
                      <div class="t-msg">"{{ app.Message || '正在请求链路接入...' }}"</div>
                    </div>
                  </div>
                  <div class="t-meta">
                    <div class="t-time">{{ formatTime(app.ApplyTime) }}</div>
                    <div class="t-actions">
                      <button class="act-btn btn-reject" @click="audit(app, false)">REJECT</button>
                      <button class="act-btn btn-approve" @click="audit(app, true)">AUTHORIZE</button>
                    </div>
                  </div>
                </div>
              </div>

              <div v-if="currentTab === 'authorized'" class="tab-pane">
                <div v-if="authorizedMembers.length === 0" class="empty-txt">[ 尚未部署任何战术单位 ]</div>
                
                <div v-else v-for="app in authorizedMembers" :key="app.AppId" class="authorized-container">
                  <div class="t-row authorized-row">
                    <div class="t-user">
                      <img :src="app.Avatar || '/default-avatar.png'" class="t-avatar">
                      <div class="t-info">
                        <div class="t-name">{{ app.Username }} <span class="member-tag">DEPLOYED</span></div>
                        <div class="t-sub">UID: {{ padZero(app.UserId) }}</div>
                      </div>
                    </div>
                    
                    <div class="submission-status">
                      <span v-if="app.HasSubmission" class="status-chip ready">DATA_READY</span>
                      <span v-else class="status-chip waiting">WAITING_DATA</span>
                    </div>

                    <div class="t-actions">
                      <button v-if="app.HasSubmission" class="act-btn btn-files" @click="toggleUserFiles(app)">
                        {{ app.showFiles ? 'HIDE_PACKET' : 'VIEW_PACKET' }}
                      </button>
                    </div>
                  </div>

                  <div v-if="app.showFiles" class="submission-grid-wrapper">
                    <div v-if="userSubmissions[app.UserId]" class="submission-grid">
                      <div v-for="file in userSubmissions[app.UserId]" :key="file.Id" class="file-card">
                        <div class="file-icon">📦</div>
                        <div class="file-main">
                          <div class="f-name">{{ file.FileName }}</div>
                          <div class="f-meta">{{ (file.FileSize / 1024 / 1024).toFixed(2) }} MB // {{ shortDate(file.CreatedAt) }}</div>
                        </div>
                        <button class="file-dl" @click="handleDownload(file)">DOWNLOAD</button>
                      </div>
                    </div>
                    <div v-else class="loading-txt">PULLING_DATA... [ 正在抓取数据包 ]</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <JointProposalModal 
      v-model="showCreateModal" 
      :editingData="editingItem"
      @success="onProposalSuccess" 
    />

  </section>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import JointProposalModal from '@/CreateCenterComponents/components/JointProposalModal.vue'
import UniversalTags from '@/GeneralComponents/UniversalTags.vue'

const authStore = useAuthStore()
const myJoints = ref([])
const loading = ref(false)
const showManageModal = ref(false)
const showCreateModal = ref(false)
const editingItem = ref(null)
const selectedOp = ref(null)

const applicants = ref([])
const applicantsLoading = ref(false)
const userSubmissions = ref({})
const currentTab = ref('waitlist')

// --- 🎯 计算属性：战术分流 ---
const waitingApplicants = computed(() => applicants.value.filter(a => a.Status === 0))
const authorizedMembers = computed(() => applicants.value.filter(a => a.Status === 1))
const submissionRate = computed(() => {
  if (authorizedMembers.value.length === 0) return 0
  const submitted = authorizedMembers.value.filter(a => a.HasSubmission).length
  return Math.round((submitted / authorizedMembers.value.length) * 100)
})

// --- 1. 数据调取 ---
const fetchMyJoints = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('ChaiLianHe/my-hosted')
    myJoints.value = res.data.data || []
  } catch (e) {
    console.error("同步失败")
  } finally {
    loading.value = false
  }
}

// --- 2. 方案编辑逻辑 ---
const handleEdit = async (item) => {
  loading.value = true
  try {
    const res = await apiClient.get(`ChaiLianHe/${item.id}`)
    // 聚合详情与标签传给弹窗
    editingItem.value = { ...res.data.data, tags: res.data.tags }
    showCreateModal.value = true
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  editingItem.value = null
  showCreateModal.value = true
}

const onProposalSuccess = () => {
  showCreateModal.value = false
  fetchMyJoints()
}

// --- 3. 指令下达 (结案/重启/删除) ---
const handleStatusChange = async (item, actionType) => {
  const msg = actionType === 'close' ? '确定要封存此行动吗？' : '确定要重启该行动吗？'
  if (!confirm(msg)) return
  try {
    await apiClient.post(`ChaiLianHe/moderation/${actionType}`, { id: item.id })
    fetchMyJoints()
  } catch (e) { alert('指令同步失败') }
}

const handleDelete = async (item) => {
  if (!confirm(`致命警告：确定要销毁档案 [${item.name}] 吗？`)) return
  try {
    await apiClient.delete(`ChaiLianHe/${item.id}`)
    fetchMyJoints()
  } catch (e) { alert('删除受阻') }
}

// --- 4. 人员管理控制台逻辑 ---
const openManageModal = async (op) => {
  selectedOp.value = op
  showManageModal.value = true
  currentTab.value = 'waitlist'
  userSubmissions.value = {}
  fetchApplicants(op.id)
}

const fetchApplicants = async (opId) => {
  applicantsLoading.value = true
  try {
    const res = await apiClient.get(`ChaiLianHe/${opId}/applications`)
    applicants.value = res.data.data || []
  } finally { applicantsLoading.value = false }
}

const audit = async (app, isPass) => {
  try {
    await apiClient.post('ChaiLianHe/application/audit', { appId: app.AppId, pass: isPass })
    fetchApplicants(selectedOp.value.id)
    fetchMyJoints()
  } catch (e) { alert('审核同步失败') }
}

const toggleUserFiles = async (app) => {
  if (app.showFiles) { app.showFiles = false; return }
  if (!userSubmissions.value[app.UserId]) {
    try {
      // 调取特定用户的提交记录
      const res = await apiClient.get(`ChaiLianHe/${selectedOp.value.id}/submissions/${app.UserId}`)
      userSubmissions.value[app.UserId] = res.data.data || []
    } catch (e) { alert('获取数据包失败') }
  }
  app.showFiles = true
}

const handleDownload = async (file) => {
  try {
    const res = await apiClient.get(`ChaiLianHe/submission/download/${file.Id}`)
    if (res.data?.url) window.open(res.data.url, '_blank')
  } catch (e) { alert('调取下载链路失败') }
}

const closeModal = () => { showManageModal.value = false }
const padZero = (n) => n < 10 ? `0${n}` : n
const typeLabelCn = (t) => ({ 1: '协作', 2: '接力', 3: '比赛', 4: '同步', 5: '测试' }[t] || '常规')
const shortDate = (d) => d ? d.substring(0, 10) : ''
const formatTime = (t) => new Date(t).toLocaleString()
const getStatusText = (v) => ({ 0: '待审批', 1: '运行中', 2: '驳回', 3: '已结案' }[v] || '未知')
const getStatusClass = (v) => v === 1 ? 'bg-green' : (v === 3 ? 'bg-grey' : 'bg-yellow')

onMounted(fetchMyJoints)
</script>

<style scoped>
/* =========================================
   📊 赛博战术终端 CSS 系统
   ========================================= */

.view-section { max-width: 1200px; margin: 0 auto; padding: 20px; }
.section-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 30px; border-bottom: 3px solid #111; padding-bottom: 15px; }
.cyber-title { font-family: 'Anton', sans-serif; font-size: 2.2rem; margin: 0; line-height: 1; }
.sub-line { font-size: 0.7rem; color: #999; font-weight: bold; font-family: monospace; }
.stats-row { font-weight: bold; font-size: 0.9rem; }
.stats-row .highlight { color: #D92323; margin-left: 15px; }

/* 1. 档案卡片样式 */
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 25px; }
.manage-card {
  background: #fff; border: 2px solid #111; padding: 25px; position: relative;
  display: flex; flex-direction: column; min-height: 240px; transition: 0.3s;
  box-shadow: 6px 6px 0 #111;
}
.manage-card:hover { transform: translate(-2px, -2px); box-shadow: 10px 10px 0 rgba(217, 35, 35, 0.1); }
.is-pending { background: #fffdf5; border-color: #f1c40f; }
.is-rejected { border-color: #D92323; opacity: 0.8; }
.is-closed { background: #f8f8f8; border-color: #999; border-style: dashed; }

/* 状态灯 */
.mc-status { width: 12px; height: 12px; border-radius: 50%; position: absolute; top: 20px; right: 20px; }
.bg-green { background: #2ecc71; box-shadow: 0 0 8px #2ecc71; }
.bg-yellow { background: #f1c40f; animation: blink 1.5s infinite; }
.bg-grey { background: #999; }
@keyframes blink { 50% { opacity: 0.3; } }

.mc-id { font-size: 0.65rem; color: #D92323; font-weight: 900; font-family: monospace; }
.mc-title { font-size: 1.5rem; font-weight: 900; margin: 10px 0; line-height: 1.2; }
.mc-meta { font-size: 0.75rem; color: #666; display: flex; gap: 15px; font-weight: bold; margin-top: 10px; }

/* 按钮区 */
.mc-actions-wrapper { display: flex; flex-direction: column; gap: 12px; margin-top: auto; border-top: 1px solid #eee; padding-top: 15px; }
.utility-row { display: flex; justify-content: flex-end; gap: 15px; }
.command-row { display: flex; gap: 10px; align-items: center; }

.icon-btn { background: none; border: none; cursor: pointer; font-size: 1.1rem; opacity: 0.5; transition: 0.2s; }
.icon-btn:hover { opacity: 1; transform: scale(1.2); }
.icon-btn.danger:hover { color: #D92323; }

.cyber-btn { border: 2px solid #111; font-weight: bold; cursor: pointer; padding: 7px 15px; font-size: 0.8rem; transition: 0.2s; }
.cyber-btn.primary { background: #111; color: #fff; position: relative; flex: 1.5; }
.cyber-btn.outline { background: transparent; color: #111; flex: 1; }
.cyber-btn:hover:not(:disabled) { background: #D92323; border-color: #D92323; color: #fff; }
.cyber-btn:disabled { opacity: 0.3; cursor: not-allowed; border-color: #ccc; }
.badge { position: absolute; top: -8px; right: -5px; background: #D92323; color: #fff; font-size: 0.6rem; padding: 2px 7px; border-radius: 10px; border: 1px solid #fff; }

.add-new { align-items: center; justify-content: center; border: 2px dashed #999; cursor: pointer; color: #999; background: transparent; box-shadow: none; }
.add-new .plus { font-size: 3rem; margin-bottom: 5px; }

/* 2. 🕹️ 终端管理弹窗系统 */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(8px); }
.cyber-terminal { width: 950px; height: 85vh; background: #fff; border: 5px solid #111; display: flex; flex-direction: column; box-shadow: 25px 25px 0 rgba(0,0,0,0.8); }

.term-header { background: #111; color: #fff; padding: 15px 25px; display: flex; justify-content: space-between; align-items: center; }
.term-title { font-family: monospace; font-weight: 900; font-size: 1.1rem; }
.term-id { font-family: monospace; font-size: 0.7rem; color: #666; margin-left: 15px; }
.term-close { background: #D92323; color: #fff; border: none; padding: 6px 15px; cursor: pointer; font-weight: 900; font-size: 0.7rem; }

/* 统计条 */
.terminal-stats-bar { display: flex; background: #f9f9f9; border-bottom: 2px solid #111; padding: 12px 25px; gap: 50px; }
.ts-item .label { font-size: 0.65rem; font-weight: 900; color: #999; margin-right: 12px; text-transform: uppercase; }
.ts-item .val { font-family: monospace; font-weight: 900; font-size: 1.2rem; }

/* 终端导航 */
.terminal-nav { display: flex; background: #111; gap: 2px; padding: 0 2px; }
.terminal-nav button { flex: 1; border: none; padding: 14px; font-weight: 900; font-size: 0.75rem; background: #eee; cursor: pointer; transition: 0.2s; color: #888; }
.terminal-nav button.active { background: #fff; color: #111; clip-path: polygon(0 0, 100% 0, 95% 100%, 5% 100%); }

.console-body { flex: 1; padding: 25px; background: #fff; overflow-y: auto; }

/* 列表行样式 */
.t-row { display: flex; align-items: center; justify-content: space-between; padding: 15px; border: 1px solid #eee; margin-bottom: 12px; background: #fff; transition: 0.2s; }
.t-row:hover { border-color: #111; box-shadow: 4px 4px 0 rgba(0,0,0,0.05); }

.t-user { display: flex; align-items: center; gap: 15px; }
.t-avatar { width: 45px; height: 45px; border: 2px solid #111; }
.t-info .t-name { font-weight: 900; font-size: 1rem; }
.t-info .t-msg { font-size: 0.75rem; color: #666; margin-top: 4px; font-style: italic; }
.t-meta { text-align: right; }
.t-time { font-family: monospace; font-size: 0.7rem; color: #bbb; margin-bottom: 8px; }

/* 控制按钮 */
.act-btn { border: 2px solid #111; padding: 6px 15px; font-weight: 900; font-size: 0.7rem; cursor: pointer; transition: 0.2s; }
.btn-approve { background: #111; color: #fff; margin-left: 10px; }
.btn-approve:hover { background: #2ecc71; border-color: #2ecc71; }
.btn-reject:hover { background: #D92323; color: #fff; border-color: #D92323; }
.btn-files { background: #2ecc71; color: #fff; border-color: #2ecc71; }

/* 已授权名单特有 */
.authorized-row { border-left: 6px solid #2ecc71; }
.member-tag { font-size: 0.6rem; background: #2ecc71; color: #fff; padding: 2px 5px; margin-left: 10px; border-radius: 2px; }
.status-chip { font-family: monospace; font-size: 0.65rem; font-weight: 900; padding: 4px 10px; }
.status-chip.ready { color: #2ecc71; border: 1px solid #2ecc71; }
.status-chip.waiting { color: #f1c40f; border: 1px solid #f1c40f; }

/* 📁 稿件网格系统 */
.submission-grid-wrapper { padding: 15px; background: #f6f6f6; border: 1px solid #eee; margin: -12px 0 15px; }
.submission-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 15px; }
.file-card { background: #fff; border: 1px solid #ddd; padding: 12px; display: flex; gap: 12px; align-items: center; transition: 0.2s; }
.file-card:hover { border-color: #111; }
.file-icon { font-size: 1.5rem; }
.file-main { flex: 1; overflow: hidden; }
.f-name { font-weight: 900; font-size: 0.8rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.f-meta { font-size: 0.6rem; color: #999; font-family: monospace; }
.file-dl { border: none; background: #111; color: #fff; font-size: 0.6rem; padding: 5px 8px; cursor: pointer; font-weight: 900; }

/* 通用文本样式 */
.loading-txt { padding: 40px; text-align: center; color: #999; font-family: monospace; }
.empty-txt { padding: 60px; text-align: center; color: #bbb; font-weight: 900; font-size: 0.8rem; letter-spacing: 2px; }
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>