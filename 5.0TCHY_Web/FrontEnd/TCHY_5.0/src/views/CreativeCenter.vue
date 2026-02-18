<template>
  <div class="creative-terminal">
    <div class="bg-grid"></div>
    
    <aside class="nav-tower">
      <div class="tower-header">
        <span class="logo-box">CREATOR</span>
        <span class="sub">创造中心 // V.1.0</span>
      </div>
      
      <nav class="nav-menu">
        <div 
          class="nav-item" 
          :class="{ active: currentView === 'my-joints' }"
          @click="currentView = 'my-joints'"
        >
          <span class="icon">▤</span> 我的联合 // MY_OPS
        </div>
        <div class="nav-item disabled">
          <span class="icon">◈</span> 稿件管理 (W.I.P)
        </div>
        <div class="nav-item disabled">
          <span class="icon">📊</span> 数据分析 (W.I.P)
        </div>
      </nav>

      <div class="tower-footer">
        <button class="exit-btn" @click="$router.push('/')"><< EXIT_SYSTEM</button>
      </div>
    </aside>

    <main class="main-deck custom-scroll">
      
      <section v-if="currentView === 'my-joints'" class="view-section">
        <div class="section-header">
          <h2>OPERATION_MANAGEMENT // 联合行动管理</h2>
          <div class="stats-row">
            <span class="stat">TOTAL: {{ myJoints.length }}</span>
            <span class="stat highlight">ACTIVE: {{ myJoints.filter(i=>i.verify===1).length }}</span>
          </div>
        </div>

        <div v-if="loading" class="loading-txt">DATA_SYNCING...</div>
        
        <div class="cards-grid" v-else>
          <div v-for="item in myJoints" :key="item.id" class="manage-card">
            <div class="mc-status" :class="getStatusClass(item.verify)"></div>
            <div class="mc-content">
              <div class="mc-id">OP-{{ padZero(item.id) }}</div>
              <h3 class="mc-title">{{ item.name }}</h3>
              <div class="mc-meta">
                <span>{{ typeLabelCn(item.type) }}</span>
                <span>{{ shortDate(item.startdate) }}</span>
              </div>
            </div>
            <div class="mc-actions">
              <button class="cyber-btn sm" @click="$router.push(`/joint/${item.id}`)">预览</button>
              <button class="cyber-btn sm primary" @click="openManageModal(item)">
                人员管理 <span v-if="item.pendingCount > 0" class="badge">{{ item.pendingCount }}</span>
              </button>
            </div>
          </div>
          
          <div class="manage-card add-new" @click="$router.push('/create-joint')"> 
            <span class="plus">+</span>
            <span>INITIATE_NEW</span>
          </div>
        </div>
      </section>

    </main>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showManageModal" class="cyber-modal-overlay" @click.self="closeModal">
          <div class="cyber-terminal console-mode">
            <div class="term-header">
              <span class="term-title">>> COMMAND_CONSOLE // 人员管理: {{ selectedOp?.name }}</span>
              <button class="term-close" @click="closeModal">CLOSE</button>
            </div>
            
            <div class="console-body custom-scroll">
              <div class="applicant-list">
                <div v-if="applicantsLoading" class="loading-txt">LOADING_SIGNALS...</div>
                <div v-else-if="applicants.length === 0" class="empty-txt">[ 暂无申请记录 ]</div>
                
                <div v-else v-for="app in applicants" :key="app.AppId" class="app-row-container">
                  <div class="app-row">
                    <div class="app-user">
                      <img :src="app.Avatar || '/default-avatar.png'" class="app-avatar">
                      <div class="app-info">
                        <div class="u-name">{{ app.Username }}</div>
                        <div class="u-msg">"{{ app.Message || '无留言' }}"</div>
                        <div class="u-time">{{ formatTime(app.ApplyTime) }}</div>
                      </div>
                    </div>
                    
                    <div class="app-actions">
                      <template v-if="app.Status === 1">
                        <div class="status-group">
                          <span class="st-tag pass">已授权 // AUTH_OK</span>
                          
                          <button 
                            v-if="app.HasSubmission" 
                            class="act-btn files" 
                            @click="toggleUserFiles(app)"
                          >
                            📂 稿件库 ({{ userSubmissions[app.UserId]?.length || '...' }})
                          </button>
                          <span v-else class="st-hint">未提交数据</span>
                        </div>
                      </template>

                      <span v-else-if="app.Status === 2" class="st-tag reject">已拦截 // DENIED</span>
                      
                      <template v-else-if="app.Status === 0">
                        <button class="act-btn reject" @click="audit(app, false)">拒绝</button>
                        <button class="act-btn pass" @click="audit(app, true)">批准接入</button>
                      </template>
                    </div>
                  </div>

                  <div v-if="app.showFiles" class="file-list-subpanel">
                    <div v-if="!userSubmissions[app.UserId]" class="sync-text">数据包检索中...</div>
                    <div v-for="file in userSubmissions[app.UserId]" :key="file.Id" class="file-entry">
                      <div class="file-meta">
                        <span class="f-name">{{ file.FileName }}</span>
                        <span class="f-info">
                          {{ (file.FileSize / 1024 / 1024).toFixed(2) }} MB // 
                          {{ formatTime(file.CreatedAt) }}
                        </span>
                      </div>
                      <button class="download-link" @click="handleDownload(file.Id)">
                        [ 下载 // DOWNLOAD ]
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const authStore = useAuthStore()
const currentView = ref('my-joints')
const myJoints = ref([])
const loading = ref(false)

// 弹窗管理
const showManageModal = ref(false)
const selectedOp = ref(null)
const applicants = ref([])
const applicantsLoading = ref(false)

// ✅ 新增：用于存储每个用户的多份投稿数据
const userSubmissions = ref({}) // 格式: { [userId]: Array }

// --- 1. 获取我发起的行动 ---
const fetchMyJoints = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('ChaiLianHe/my-hosted')
    myJoints.value = res.data.data || []
  } catch (e) {
    console.error("同步失败", e)
  } finally {
    loading.value = false
  }
}

// --- 2. 审核与人员控制 ---
const openManageModal = async (op) => {
  selectedOp.value = op
  showManageModal.value = true
  userSubmissions.value = {} // 清空之前的临时缓存
  fetchApplicants(op.id)
}

const fetchApplicants = async (opId) => {
  applicantsLoading.value = true
  try {
    const res = await apiClient.get(`ChaiLianHe/${opId}/applications`)
    applicants.value = res.data.data || []
  } catch (e) {
    applicants.value = []
  } finally {
    applicantsLoading.value = false
  }
}

// --- 3. 核心：获取并展开某个用户的稿件库 ---
const toggleUserFiles = async (app) => {
  // 如果当前已经显示，则折叠
  if (app.showFiles) {
    app.showFiles = false
    return
  }

  // 如果没有缓存，则请求后端
  if (!userSubmissions.value[app.UserId]) {
    try {
      const res = await apiClient.get(`ChaiLianHe/${selectedOp.value.id}/submissions/${app.UserId}`)
      userSubmissions.value[app.UserId] = res.data.data || []
    } catch (e) {
      alert('无法获取该单位的稿件历史')
      return
    }
  }
  
  app.showFiles = true
}

// --- 4. 核心：基于 ID 下载特定稿件 ---
const handleDownload = async (submissionId) => {
  try {
    // 调用根据 ID 生成下载链接的新接口
    const res = await apiClient.get(`ChaiLianHe/submission/download/${submissionId}`)
    
    if (res.data?.url) {
      window.open(res.data.url, '_blank')
    }
  } catch (e) {
    alert(e.response?.data?.message || '下载指令失效')
  }
}

// --- 5. 审核逻辑 ---
const audit = async (app, isPass) => {
  if (!confirm(isPass ? '批准该单位接入行动序列？' : '拒绝接入？')) return
  try {
    await apiClient.post('ChaiLianHe/application/audit', {
      appId: app.AppId,
      pass: isPass
    })
    await fetchApplicants(selectedOp.value.id)
    fetchMyJoints()
  } catch (e) {
    alert('同步失败')
  }
}

const closeModal = () => { showManageModal.value = false }

// --- 辅助 ---
const padZero = (n) => n < 10 ? `0${n}` : n
const typeLabelCn = (t) => ({ 1: '联合', 2: '接力', 3: '比赛' }[t] || '未知')
const shortDate = (d) => d ? d.substring(0, 10) : ''
const formatTime = (t) => new Date(t).toLocaleString()
const getStatusClass = (v) => v === 1 ? 'bg-green' : (v === 2 ? 'bg-red' : 'bg-yellow')

onMounted(fetchMyJoints)
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.creative-terminal {
  display: flex; height: 100vh; background: #f4f4f4; color: #111;
  font-family: 'JetBrains Mono', monospace;
}

/* 侧边导航 */
.nav-tower {
  width: 260px; background: #111; color: #fff; display: flex; flex-direction: column;
  padding: 30px 0; border-right: 4px solid #D92323; z-index: 2;
}
.tower-header { padding: 0 25px 30px; border-bottom: 1px solid #333; }
.logo-box { font-family: 'Anton'; font-size: 2rem; display: block; letter-spacing: 2px; }
.sub { color: #666; font-size: 0.7rem; font-weight: bold; }

.nav-menu { flex: 1; padding-top: 20px; display: flex; flex-direction: column; gap: 5px; }
.nav-item { padding: 15px 25px; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 10px; font-weight: bold; }
.nav-item:hover { background: #222; color: #D92323; }
.nav-item.active { background: #D92323; color: #fff; }
.nav-item.disabled { opacity: 0.3; cursor: not-allowed; }

.tower-footer { padding: 20px; border-top: 1px solid #333; }
.exit-btn { background: transparent; border: 1px solid #444; color: #666; padding: 5px 15px; cursor: pointer; width: 100%; text-align: left; }
.exit-btn:hover { border-color: #D92323; color: #D92323; }

/* 主工作台 */
.main-deck { flex: 1; padding: 40px; position: relative; overflow-y: auto; }
.bg-grid { position: absolute; inset: 0; background-image: linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px); background-size: 40px 40px; z-index: 0; pointer-events: none; }

.view-section { position: relative; z-index: 1; max-width: 1200px; margin: 0 auto; }
.section-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 30px; border-bottom: 3px solid #111; padding-bottom: 10px; }
.section-header h2 { font-family: 'Anton'; font-size: 2rem; margin: 0; }
.stats-row { font-weight: bold; font-size: 0.9rem; }
.stats-row .highlight { color: #D92323; margin-left: 15px; }

/* 任务卡片 */
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
.manage-card {
  background: #fff; border: 2px solid #111; padding: 20px; position: relative;
  display: flex; flex-direction: column; height: 180px; transition: 0.2s;
  box-shadow: 5px 5px 0 rgba(0,0,0,0.1);
}
.manage-card:hover { transform: translateY(-3px); box-shadow: 8px 8px 0 rgba(217, 35, 35, 0.1); }

.mc-status { width: 10px; height: 10px; border-radius: 50%; position: absolute; top: 15px; right: 15px; }
.bg-green { background: #2ecc71; box-shadow: 0 0 5px #2ecc71; }
.bg-yellow { background: #f1c40f; }
.bg-red { background: #D92323; }

.mc-content { flex: 1; }
.mc-id { font-size: 0.7rem; color: #D92323; font-weight: bold; }
.mc-title { font-family: 'Anton'; font-size: 1.4rem; margin: 5px 0 10px; line-height: 1.1; }
.mc-meta { font-size: 0.75rem; color: #666; display: flex; gap: 10px; font-weight: bold; }

.mc-actions { display: flex; gap: 10px; margin-top: auto; }
.cyber-btn { border: none; font-weight: bold; cursor: pointer; padding: 8px 15px; background: #eee; color: #111; transition: 0.2s; font-family: 'Anton'; letter-spacing: 1px; }
.cyber-btn.primary { background: #111; color: #fff; position: relative; }
.cyber-btn:hover { background: #D92323; color: #fff; }
.badge { position: absolute; top: -5px; right: -5px; background: #D92323; color: #fff; font-size: 0.6rem; padding: 2px 6px; border-radius: 4px; }

.add-new { align-items: center; justify-content: center; border: 2px dashed #999; cursor: pointer; color: #999; font-weight: bold; background: transparent; }
.add-new:hover { border-color: #D92323; color: #D92323; }
.add-new .plus { font-size: 3rem; line-height: 1; margin-bottom: 10px; }

/* 审核与管理弹窗 */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 100; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
.cyber-terminal.console-mode { width: 750px; height: 85vh; background: #eee; border: 4px solid #111; display: flex; flex-direction: column; }
.term-header { background: #111; color: #fff; padding: 15px; display: flex; justify-content: space-between; align-items: center; }
.console-body { flex: 1; padding: 20px; overflow-y: auto; background: #fff; }

.app-row-container { border-bottom: 1px solid #ddd; }
.app-row { display: flex; justify-content: space-between; align-items: center; padding: 15px 0; }
.app-user { display: flex; gap: 15px; align-items: center; }
.app-avatar { width: 45px; height: 45px; border: 1px solid #111; }
.u-name { font-weight: bold; font-size: 0.95rem; }
.u-msg { font-size: 0.8rem; color: #555; margin-top: 2px; font-style: italic; }

.app-actions { display: flex; align-items: center; gap: 10px; }
.act-btn { border: none; padding: 6px 12px; cursor: pointer; font-weight: bold; font-family: 'JetBrains Mono'; transition: 0.2s; }
.act-btn.pass { background: #111; color: #fff; }
.act-btn.reject { background: transparent; border: 1px solid #111; }
.act-btn.reject:hover { background: #D92323; color: #fff; border-color: #D92323; }

/* 稿件库按钮样式 */
.act-btn.files { background: #2ecc71; color: #fff; font-size: 0.75rem; box-shadow: 3px 3px 0 rgba(46, 204, 113, 0.2); }
.act-btn.files:hover { background: #27ae60; }

/* 子面板样式：展开的文件列表 */
.file-list-subpanel { background: #f9f9f9; padding: 15px 20px 20px 65px; border-top: 1px dashed #ddd; margin-bottom: 10px; }
.file-entry { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px solid #eee; }
.file-entry:last-child { border: none; }
.file-meta { display: flex; flex-direction: column; }
.f-name { font-weight: bold; font-size: 0.85rem; color: #D92323; }
.f-info { font-size: 0.7rem; color: #999; margin-top: 2px; }
.download-link { background: #111; color: #fff; border: none; padding: 4px 10px; font-size: 0.7rem; cursor: pointer; font-family: 'Anton'; }
.download-link:hover { background: #D92323; }

.st-tag { font-size: 0.75rem; font-weight: bold; padding: 4px 8px; border: 1px solid #ccc; }
.st-tag.pass { color: #2ecc71; border-color: #2ecc71; }
.st-tag.reject { color: #D92323; border-color: #D92323; text-decoration: line-through; }
.st-hint { font-size: 0.7rem; color: #999; font-style: italic; }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>