<template>
  <div class="reports-canvas">
    <div class="reports-layout">
      <div class="report-sidebar">
        <div class="sidebar-header">
          <h3 class="section-title" style="margin: 0;">汇报周期</h3>
          <button class="btn-icon-small" @click="showCycleManager = true" title="管理周期">⚙️</button>
        </div>
        
        <div class="cycle-list">
          <div 
            v-for="cycle in reportCycles" 
            :key="cycle.id" 
            class="cycle-item"
            :class="{ active: currentReportCycle?.id === cycle.id }"
            @click="currentReportCycle = cycle"
          >
            <div class="cycle-title">
              {{ cycle.name }}
              <span v-if="cycle.isLocked === 1" class="lock-icon" title="已锁定">🔒</span>
            </div>
            <div class="cycle-date">{{ formatCycleDate(cycle.startDate, cycle.endDate) }}</div>
          </div>
        </div>

        <h3 class="section-title mt-6">提交情况 ({{ stats.submitted }}/{{ stats.total }})</h3>
        <div class="member-status-list">
          <div v-for="m in memberStatusList" :key="m.userId" class="member-status-row">
            <UniversalAvatar :user-id="m.userId" :size="24" :show-level="false" />
            <span class="m-name">{{ m.username }}</span>
            <span :class="['status-dot', m.hasSubmitted ? 'done' : 'missing']">
              {{ m.hasSubmitted ? '已交' : '未交' }}
            </span>
          </div>
        </div>
      </div>

      <div class="report-feed" v-if="currentReportCycle">
        <div class="feed-header">
          <div class="header-main">
            <div>
              <h2>{{ currentReportCycle.name }} 工作汇报</h2>
              <span class="feed-subtitle">
                {{ formatCycleDate(currentReportCycle.startDate, currentReportCycle.endDate) }} · 
                {{ currentCycleReports.length }} 份提交
              </span>
            </div>
            
            <button 
              v-if="!userHasSubmitted" 
              class="btn-primary" 
              :disabled="currentReportCycle.isLocked === 1"
              @click="openWriteReportModal"
            >
              {{ currentReportCycle.isLocked === 1 ? '提交已截止' : '写工作汇报' }}
            </button>
            <button v-else class="btn-ghost" disabled>✅ 本期已提交</button>
          </div>

          <div class="progress-container">
            <div class="progress-bar"><div class="fill" :style="{ width: stats.percent + '%' }"></div></div>
            <span class="progress-text">进度 {{ stats.percent }}%</span>
          </div>
        </div>

        <div v-if="currentCycleReports.length === 0" class="empty-state">
          <div class="empty-icon">📭</div>
          <p>本周期暂无成员提交汇报</p>
          <button v-if="!userHasSubmitted && currentReportCycle.isLocked !== 1" class="btn-text" @click="openWriteReportModal">
            写第一篇汇报
          </button>
        </div>

        <div class="report-grid">
          <div v-for="rep in currentCycleReports" :key="rep.id" class="report-card" @click="viewReport(rep)">
            <div class="rep-header">
              <div class="rep-user">
                <UniversalAvatar :user-id="rep.userId" :show-level="false" class="rep-av" />
                <div class="rep-meta">
                  <span class="rep-name">{{ rep.userName }}</span>
                  <span class="rep-time">{{ formatTimeAgo(rep.createdAt) }}</span>
                </div>
              </div>
              <div class="rep-type-badge" :class="rep.type?.toLowerCase() || 'weekly'">{{ rep.type || 'Weekly' }}</div>
            </div>
            
            <div class="rep-preview">{{ rep.content.slice(0, 120) }}...</div>
            
            <div v-if="rep.completedTasks?.length" class="task-count-tag">
              🔗 关联了 {{ rep.completedTasks.length }} 个任务
            </div>

            <div class="rep-footer"><span>阅读全文 →</span></div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showWriteReport" class="modal-backdrop" @click.self="showWriteReport = false">
      <div class="modal-panel large">
        <div class="modal-header">
          <h3>撰写 {{ currentReportCycle?.name }} 汇报</h3>
          <button class="close-icon" @click="showWriteReport = false">×</button>
        </div>
        <div class="modal-body">
          <div class="form-row">
            <div class="form-group">
              <label>汇报类型</label>
              <select v-model="newReport.type" class="linear-select">
                <option value="Weekly">Weekly Report (周报)</option>
                <option value="Monthly">Monthly Report (月报)</option>
              </select>
            </div>
            <div class="form-group">
              <label>关联周期</label>
              <select v-model="newReport.cycleId" class="linear-select" disabled>
                <option :value="currentReportCycle?.id">{{ currentReportCycle?.name }}</option>
              </select>
            </div>
          </div>
          <div class="form-group" style="flex: 1; display: flex; flex-direction: column;">
            <label>总结内容 (支持 Markdown)</label>
            <textarea v-model="newReport.content" class="detail-textarea" style="flex: 1; min-height: 300px;" placeholder="# 本周重点工作..."></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-ghost" @click="showWriteReport = false">取消</button>
          <button class="btn-primary" @click="submitReport" :disabled="isSaving || !newReport.content.trim()">
            {{ isSaving ? '提交中...' : '提交汇报' }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="viewingReport" class="modal-backdrop" @click.self="viewingReport = null">
      <div class="modal-panel medium">
        <div class="modal-header">
          <div class="rep-view-header">
            <UniversalAvatar :user-id="viewingReport.userId" :show-level="false" />
            <div class="rep-view-info">
              <h3>{{ viewingReport.userName }} 的工作汇报</h3>
              <span>{{ viewingReport.type || 'Weekly' }} · {{ formatTimeAgo(viewingReport.createdAt) }}</span>
            </div>
          </div>
          <button class="close-icon" @click="viewingReport = null">×</button>
        </div>
        <div class="modal-body report-reader">
          <pre>{{ viewingReport.content }}</pre>
          
          <div v-if="viewingReport.completedTasks?.length" class="linked-tasks-box">
            <h4>✅ 本期完成任务</h4>
            <ul>
              <li v-for="task in viewingReport.completedTasks" :key="task.taskId">
                {{ task.title }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showCycleManager" class="modal-backdrop" @click.self="showCycleManager = false">
      <div class="modal-panel medium">
        <div class="modal-header">
          <h3>管理汇报周期</h3>
          <button class="close-icon" @click="showCycleManager = false">×</button>
        </div>
        <div class="modal-body">
          <div class="add-cycle-box">
            <input v-model="editingCycle.name" placeholder="周期名称 (如: 2026年 第9周)" class="linear-select" style="width: 40%;" />
            <input type="date" v-model="editingCycle.startDate" class="linear-select" />
            <span style="color: #6B778C;">至</span>
            <input type="date" v-model="editingCycle.endDate" class="linear-select" />
            <button class="btn-primary" @click="saveCycle">添加</button>
          </div>

          <table class="admin-table">
            <thead>
              <tr><th>周期名称</th><th>时间范围</th><th>状态</th><th>操作</th></tr>
            </thead>
            <tbody>
              <tr v-for="c in reportCycles" :key="c.id">
                <td>{{ c.name }}</td>
                <td style="font-size: 12px;">{{ formatCycleDate(c.startDate, c.endDate) }}</td>
                <td>
                  <span :class="['status-badge', c.isLocked ? 'locked' : 'open']">
                    {{ c.isLocked ? '🔒 已锁定' : '🟢 开放中' }}
                  </span>
                </td>
                <td>
                  <button class="btn-text" @click="toggleLock(c)">{{ c.isLocked ? '解锁' : '锁定' }}</button>
                  <button class="btn-text danger" @click="deleteCycle(c.id)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import apiClient from '@/utils/api'

// TODO: 替换为真实的当前用户 ID，通常从全局状态管理中获取
const currentUserId = 888; 

const props = defineProps<{ 
  projectId: string | number, 
  members: any[] 
}>()

// --- 状态定义 ---
const reportCycles = ref<any[]>([])
const currentReportCycle = ref<any>(null)
const currentCycleReports = ref<any[]>([])
const memberStatusList = ref<any[]>([]) // 左侧的"谁交了/谁没交"列表

const showWriteReport = ref(false)
const viewingReport = ref<any>(null)
const isSaving = ref(false)

const newReport = reactive({ 
  type: 'Weekly', 
  cycleId: 1, 
  content: '',
  taskIds: [] // 预留的任务 ID 数组
})

// 管理周期状态
const showCycleManager = ref(false);
const editingCycle = reactive({ name: '', startDate: '', endDate: '' });

// --- 计算属性 ---
// 1. 计算项目成员整体提交进度
const stats = computed(() => {
  const total = memberStatusList.value.length;
  const submitted = memberStatusList.value.filter(m => m.hasSubmitted).length;
  return {
    total,
    submitted,
    percent: total > 0 ? Math.round((submitted / total) * 100) : 0
  }
})

// 2. 检查当前登录用户是否已经在当前周期交过报告
const userHasSubmitted = computed(() => {
  return memberStatusList.value.some(m => m.userId === currentUserId && m.hasSubmitted);
})

// --- 格式化工具 ---
const formatCycleDate = (start: string, end: string) => {
  if (!start || !end) return '';
  const s = new Date(start).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  const e = new Date(end).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  return `${s} - ${e}`
}
const formatTimeAgo = (d: string) => new Date(d).toLocaleString()

// --- 核心 API 请求逻辑 ---

// 1. 获取左侧的所有汇报周期
const fetchCycles = async () => {
  try {
    const res = await apiClient.get('/Reports/cycles');
    
    // 🔥 关键修复：将后端返回的大写首字母字段，映射为前端预期的小写字段
    reportCycles.value = res.data.map((c: any) => ({
      id: c.Id,
      name: c.Name,
      startDate: c.StartDate,
      endDate: c.EndDate,
      type: c.Type,
      isLocked: c.IsLocked
    }));

    if (reportCycles.value.length > 0) {
      if (!currentReportCycle.value || !reportCycles.value.find(c => c.id === currentReportCycle.value.id)) {
        currentReportCycle.value = reportCycles.value[0];
        newReport.cycleId = currentReportCycle.value.id;
      }
    } else {
        currentReportCycle.value = null;
        currentCycleReports.value = [];
    }
  } catch (error) {
    console.error("获取汇报周期失败", error);
  }
}

const loadCycleData = async () => {
  if (!currentReportCycle.value) return;
  
  const pid = Number(props.projectId);
  const cid = currentReportCycle.value.id;

  try {
    const [feedRes, statusRes] = await Promise.all([
      apiClient.get(`/Reports/project/${pid}/cycle/${cid}/feed`),
      apiClient.get(`/Reports/project/${pid}/cycle/${cid}/status`)
    ]);

    // 🔥 关键修复：同样对右侧的报告列表进行大写到小写的映射
    currentCycleReports.value = feedRes.data.map((r: any) => ({
      id: r.Id,
      title: r.Title,
      content: r.Content,
      type: r.Type,
      createdAt: r.CreatedAt,
      userId: r.UserId,
      userName: r.UserName,
      completedTasks: r.CompletedTasks?.map((t: any) => ({
        taskId: t.TaskId,
        title: t.Title
      })) || []
    }));

    // 🔥 关键修复：对侧边栏的成员状态列表进行映射
    memberStatusList.value = statusRes.data.map((m: any) => ({
      userId: m.UserId,
      username: m.Username,
      hasSubmitted: m.HasSubmitted,
      reportId: m.ReportId,
      submittedAt: m.SubmittedAt
    }));

  } catch (error) {
    console.error("加载周期数据失败", error);
  }
}

// 监听左侧周期的点击切换，一旦切换，自动重新拉取右侧数据
watch(() => currentReportCycle.value?.id, (newVal, oldVal) => {
  if (newVal && newVal !== oldVal) {
    newReport.cycleId = newVal;
    loadCycleData();
  }
})

// 3. 提交新汇报
const submitReport = async () => {
  if (!newReport.content.trim()) return;
  isSaving.value = true;

  try {
    const payload = {
      organizationId: 1, // TODO: 替换为真实的当前组织 ID
      projectId: Number(props.projectId),
      cycleId: currentReportCycle.value.id,
      title: `${currentReportCycle.value.name} 工作汇报`,
      content: newReport.content,
      type: newReport.type,
      taskIds: newReport.taskIds 
    };

    await apiClient.post('/Reports/submit', payload);
    
    // 提交成功后：关闭弹窗、清空内容、刷新本周期数据列表
    showWriteReport.value = false;
    newReport.content = '';
    newReport.taskIds = [];
    await loadCycleData();

  } catch (error: any) {
    alert(error.response?.data || "提交失败，请重试");
  } finally {
    isSaving.value = false;
  }
}

// --- 周期管理 API 逻辑 ---

// 4. 添加新周期
const saveCycle = async () => {
  if (!editingCycle.name) return alert("请输入周期名称");
  try {
    const payload = {
      name: editingCycle.name,
      start_date: editingCycle.startDate || new Date().toISOString(),
      end_date: editingCycle.endDate || new Date().toISOString(),
    };
    await apiClient.post('/Reports/cycles', payload);
    editingCycle.name = ''; 
    editingCycle.startDate = ''; 
    editingCycle.endDate = '';
    await fetchCycles(); // 刷新侧边栏周期列表
  } catch (error: any) {
    alert(error.response?.data || "添加周期失败");
  }
}

// 5. 删除周期
const deleteCycle = async (id: number) => {
  if (!confirm("确定要删除这个周期吗？该周期内的所有汇报也将被一并删除，且不可恢复！")) return;
  try {
    await apiClient.delete(`/Reports/cycles/${id}`);
    await fetchCycles();
  } catch (error: any) {
    alert("删除失败");
  }
}

// 6. 锁定/解锁周期
const toggleLock = async (cycle: any) => {
  try {
    await apiClient.patch(`/Reports/cycles/${cycle.id}/toggle-lock`);
    await fetchCycles(); // 刷新状态以显示锁图标
  } catch (error: any) {
    alert("操作失败");
  }
}


// --- 弹窗交互逻辑 ---
const openWriteReportModal = () => { showWriteReport.value = true }
const viewReport = (r: any) => { viewingReport.value = r }

defineExpose({ openWriteReportModal })

// 组件挂载时拉取周期列表
onMounted(() => {
  fetchCycles();
})
</script>

<style scoped>
/* ================= 基础布局 ================= */
.reports-canvas { flex: 1; overflow: hidden; display: flex; background: #FAFBFC; height: 100%; }
.reports-layout { display: flex; width: 100%; max-width: 1400px; margin: 0 auto; height: 100%; }
.report-sidebar { width: 280px; padding: 24px; border-right: 1px solid #EBECF0; background: #F4F5F7; overflow-y: auto; }

/* ================= 左侧周期列表 ================= */
.sidebar-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.btn-icon-small { background: transparent; border: none; cursor: pointer; font-size: 14px; opacity: 0.7; transition: 0.2s; padding: 4px; }
.btn-icon-small:hover { opacity: 1; transform: scale(1.1); }
.cycle-item { padding: 10px 12px; border-radius: 6px; cursor: pointer; margin-bottom: 4px; transition: 0.2s; }
.cycle-item:hover { background: #EBECF0; }
.cycle-item.active { background: #DEEBFF; color: #0052CC; }
.cycle-title { font-weight: 600; font-size: 14px; display: flex; align-items: center; }
.cycle-date { font-size: 11px; opacity: 0.8; margin-top: 2px; }

/* ================= 报告流主区域 ================= */
.report-feed { flex: 1; padding: 32px 48px; overflow-y: auto; }
.feed-header { margin-bottom: 32px; border-bottom: 1px solid #EBECF0; padding-bottom: 16px; }
.feed-header h2 { margin: 0; font-size: 24px; font-weight: 700; color: #172B4D; }
.feed-subtitle { color: #6B778C; font-size: 14px; margin-top: 4px; display: inline-block; }
.empty-state { text-align: center; padding: 60px; color: #6B778C; }
.empty-icon { font-size: 48px; margin-bottom: 16px; }

/* ================= 报告卡片 ================= */
.report-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
.report-card { background: #fff; border: 1px solid #DFE1E6; border-radius: 8px; padding: 20px; cursor: pointer; transition: 0.2s; display: flex; flex-direction: column; }
.report-card:hover { border-color: #0052CC; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.rep-header { display: flex; justify-content: space-between; margin-bottom: 16px; }
.rep-user { display: flex; gap: 10px; align-items: center; }
.rep-av { width: 32px; height: 32px; }
.rep-name { font-weight: 600; font-size: 14px; display: block; }
.rep-time { font-size: 11px; color: #6B778C; }
.rep-type-badge { font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; height: fit-content; }
.rep-type-badge.weekly { background: #E3FCEF; color: #006644; }
.rep-type-badge.monthly { background: #DEEBFF; color: #0747A6; }
.rep-preview { font-size: 13px; color: #42526E; line-height: 1.6; overflow: hidden; margin-bottom: 16px; flex: 1; }
.rep-footer { font-size: 12px; color: #0052CC; font-weight: 500; margin-top: auto; }

/* ================= 增强UI样式 ================= */
.section-title { font-size: 12px; font-weight: 700; color: #6B778C; text-transform: uppercase; margin-bottom: 12px; }
.mt-6 { margin-top: 32px; }
.lock-icon { font-size: 14px; margin-left: 6px; cursor: help; }

/* ================= 左侧名单状态 ================= */
.member-status-list { display: flex; flex-direction: column; gap: 8px; }
.member-status-row { display: flex; align-items: center; gap: 10px; padding: 6px 8px; background: #fff; border-radius: 6px; border: 1px solid #DFE1E6; }
.member-status-row .m-name { flex: 1; font-size: 13px; font-weight: 500; color: #172B4D; }
.status-dot { font-size: 11px; padding: 2px 8px; border-radius: 12px; font-weight: 600; }
.status-dot.done { background: #E3FCEF; color: #006644; }
.status-dot.missing { background: #FFEBE6; color: #BF2600; }

/* ================= 头部进度条区 ================= */
.header-main { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
.progress-container { display: flex; align-items: center; gap: 12px; background: #F4F5F7; padding: 10px 16px; border-radius: 6px; }
.progress-bar { flex: 1; height: 8px; background: #DFE1E6; border-radius: 4px; overflow: hidden; }
.progress-bar .fill { height: 100%; background: #36B37E; transition: width 0.5s ease; }
.progress-text { font-size: 13px; font-weight: 600; color: #42526E; }

/* ================= 关联任务UI ================= */
.task-count-tag { margin-bottom: 16px; font-size: 11px; color: #5E6C84; background: #EBECF0; padding: 4px 8px; border-radius: 4px; width: fit-content; }
.linked-tasks-box { margin-top: 24px; padding-top: 24px; border-top: 1px solid #EBECF0; }
.linked-tasks-box h4 { font-size: 14px; margin-bottom: 12px; color: #172B4D; }
.linked-tasks-box ul { padding-left: 20px; color: #42526E; font-size: 13px; line-height: 1.8; margin: 0; }

/* ================= 按钮与表单状态 ================= */
button:disabled { cursor: not-allowed; opacity: 0.6; }
.btn-primary { background: #0052CC; color: #fff; border: none; border-radius: 3px; padding: 6px 12px; font-weight: 500; font-size: 13px; cursor: pointer; transition: background 0.2s; }
.btn-primary:not(:disabled):hover { background: #0043A6; }
.btn-ghost { background: transparent; border: none; color: #6B778C; cursor: pointer; padding: 6px 12px; font-size: 13px; }
.btn-ghost:not(:disabled):hover { background: #EBECF0; border-radius: 3px; }
.btn-text { background: none; border: none; color: #0052CC; cursor: pointer; font-weight: 600; font-size: 13px; margin-top: 12px; }

/* ================= 周期管理弹窗特定样式 ================= */
.add-cycle-box { display: flex; gap: 10px; align-items: center; margin-bottom: 24px; background: #F4F5F7; padding: 16px; border-radius: 6px; }
.admin-table { width: 100%; border-collapse: collapse; text-align: left; }
.admin-table th { padding: 12px 8px; border-bottom: 2px solid #DFE1E6; color: #6B778C; font-size: 12px; font-weight: 600;}
.admin-table td { padding: 12px 8px; border-bottom: 1px solid #DFE1E6; color: #172B4D; font-size: 13px; }
.btn-text.danger { color: #DE350B; margin-top: 0; margin-left: 8px;}
.status-badge { padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; }
.status-badge.open { background: #E3FCEF; color: #006644; }
.status-badge.locked { background: #DFE1E6; color: #42526E; }

/* ================= 模态框通用 ================= */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal-panel { background: #fff; border-radius: 3px; display: flex; flex-direction: column; box-shadow: 0 8px 30px rgba(0,0,0,0.3); }
.modal-panel.medium { width: 650px; max-height: 85vh; }
.modal-panel.large { width: 900px; height: 85vh; }
.modal-header { padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; }
.modal-body { padding: 24px; flex: 1; overflow-y: auto; }
.modal-footer { padding: 16px 24px; border-top: 1px solid #DFE1E6; display: flex; justify-content: flex-end; gap: 8px; }
.form-group { margin-bottom: 16px; }
.form-row { display: flex; gap: 16px; }
.form-row .form-group { flex: 1; }
label { display: block; font-size: 12px; font-weight: 600; color: #6B778C; margin-bottom: 4px; }
.linear-select, .detail-textarea { width: 100%; padding: 8px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; background: #FAFBFC; box-sizing: border-box;}
.close-icon { background: none; border: none; font-size: 20px; cursor: pointer; color: #6B778C; }
.rep-view-header { display: flex; gap: 16px; align-items: center; }
.rep-view-info h3 { margin: 0; font-size: 16px; color: #172B4D; }
.report-reader pre { white-space: pre-wrap; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; line-height: 1.6; color: #172B4D; font-size: 14px; margin: 0; }
</style>