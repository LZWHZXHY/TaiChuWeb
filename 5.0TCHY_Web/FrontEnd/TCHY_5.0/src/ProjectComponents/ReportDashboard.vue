<template>
  <div class="reports-canvas">
    <div class="reports-layout">
      
      <aside class="status-sidebar">
        <h3 class="section-title" style="margin-bottom: 20px;">
          提交情况 <span class="stats-count">({{ stats.submitted }}/{{ stats.total }})</span>
        </h3>
        <div class="member-status-list">
          <div v-for="m in memberStatusList" :key="m.userId" class="member-status-row">
            <div class="m-avatar-wrapper">
              <div class="strict-avatar-28">
                <UniversalAvatar :user-id="m.userId" :show-level="false" />
              </div>
              <div :class="['status-indicator', m.hasSubmitted ? 'done' : 'missing']"></div>
            </div>
            <span class="m-name">{{ m.username }}</span>
            <span :class="['status-badge-small', m.hasSubmitted ? 'done' : 'missing']">
              {{ m.hasSubmitted ? '已交' : '未交' }}
            </span>
          </div>
        </div>
      </aside>

      <aside class="cycle-sidebar">
        <div class="sidebar-header">
          <h3 class="section-title">汇报周期</h3>
          <button class="btn-icon-small" @click="showCycleManager = true" title="管理周期">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"></circle><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path></svg>
          </button>
        </div>
        
        <div class="cycle-list">
          <div 
            v-for="cycle in reportCycles" 
            :key="cycle.id" 
            class="cycle-item"
            :class="{ active: currentReportCycle?.id === cycle.id }"
            @click="currentReportCycle = cycle"
          >
            <div class="cycle-info">
              <div class="cycle-title">
                {{ cycle.name }}
                <span v-if="cycle.isLocked === 1" class="lock-icon" title="已锁定">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                </span>
              </div>
              <div class="cycle-date">{{ formatCycleDate(cycle.startDate, cycle.endDate) }}</div>
            </div>
            <div v-if="currentReportCycle?.id === cycle.id" class="active-indicator"></div>
          </div>
        </div>
      </aside>

      <main class="report-feed" v-if="currentReportCycle">
        <div class="feed-header">
          <div class="header-content">
            <h2 class="feed-title">{{ currentReportCycle.name }} 工作汇报</h2>
            <div class="feed-meta">
              <span class="meta-item">🗓 {{ formatCycleDate(currentReportCycle.startDate, currentReportCycle.endDate) }}</span>
              <span class="meta-dot">·</span>
              <span class="meta-item">📄 {{ currentCycleReports.length }} 份提交</span>
            </div>
            
            <div class="progress-section">
              <div class="progress-info">
                <span class="progress-label">团队完成度</span>
                <span class="progress-percent">{{ stats.percent }}%</span>
              </div>
              <div class="progress-track">
                <div class="progress-fill" :style="{ width: stats.percent + '%' }"></div>
              </div>
            </div>
          </div>
          
          <div class="header-actions">
            <template v-if="!isViewer">
              <button 
                v-if="!userHasSubmitted" 
                class="btn-primary" 
                :disabled="currentReportCycle.isLocked === 1"
                @click="openWriteReportModal"
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;"><path d="M12 20h9"></path><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path></svg>
                {{ currentReportCycle.isLocked === 1 ? '提交已截止' : '写工作汇报' }}
              </button>
              <button v-else class="btn-success" disabled>
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;"><polyline points="20 6 9 17 4 12"></polyline></svg>
                本期已提交
              </button>
            </template>
            
            <button v-else class="btn-ghost" disabled style="opacity: 0.6; cursor: default;">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 6px;"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>
              观察者无需汇报
            </button>
          </div>






        </div>

        <div v-if="currentCycleReports.length === 0" class="empty-state">
          <div class="empty-icon-wrapper">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"></path></svg>
          </div>
          <p class="empty-title">暂无数据流入</p>
          <p class="empty-desc">本周期还没有任何干员提交汇报记录。</p>
          <button v-if="!userHasSubmitted && currentReportCycle.isLocked !== 1" class="btn-ghost" @click="openWriteReportModal">
            成为第一个汇报者
          </button>
        </div>

        <div class="report-grid">
          <div v-for="rep in currentCycleReports" :key="rep.id" class="report-card" @click="viewReport(rep)">
            <div class="rep-header">
              <div class="rep-user">
                <div class="strict-avatar-32">
                  <UniversalAvatar :user-id="rep.userId" :show-level="false" />
                </div>
                <div class="rep-meta-info">
                  <span class="rep-name" :title="rep.userName">{{ rep.userName }}</span>
                  <span class="rep-time">
                    {{ formatTimeAgo(rep.createdAt) }}
                    <span v-if="isReportEdited(rep)" class="edited-badge" :title="'修改于: ' + formatExactTime(rep.updatedAt)">(已编辑)</span>
                  </span>
                </div>
              </div>
              <div class="rep-type-badge" :class="rep.type?.toLowerCase() || 'weekly'">{{ rep.type || 'Weekly' }}</div>
            </div>
            
            <div class="rep-preview">{{ rep.content.slice(0, 150) }}{{ rep.content.length > 150 ? '...' : '' }}</div>
            
            <div class="rep-footer">
              <div v-if="rep.completedTasks?.length" class="task-count-tag">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"></path><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"></path></svg>
                <span>{{ rep.completedTasks.length }} 个关联任务</span>
              </div>
              <span class="read-more">阅读全文 →</span>
            </div>
          </div>
        </div>
      </main>
    </div>

    <transition name="modal-fade">
      <div v-if="showWriteReport" class="modal-backdrop" @click.self="showWriteReport = false">
        <div class="modal-panel large">
          <div class="modal-header">
            <h3>撰写汇报 <span class="header-cycle-name">/ {{ currentReportCycle?.name }}</span></h3>
            <button class="close-icon" @click="showWriteReport = false"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg></button>
          </div>
          <div class="modal-body">
            <div class="form-row">
              <div class="form-group">
                <label>汇报类型</label>
                <div class="select-wrapper">
                  <select v-model="newReport.type" class="modern-input">
                    <option value="Weekly">Weekly Report (周报)</option>
                    <option value="Monthly">Monthly Report (月报)</option>
                  </select>
                </div>
              </div>
              <div class="form-group">
                <label>关联周期</label>
                <input type="text" :value="currentReportCycle?.name" class="modern-input disabled" disabled />
              </div>
            </div>
            <div class="form-group flex-fill">
              <label>总结内容 (支持 Markdown)</label>
              <textarea v-model="newReport.content" class="modern-textarea" placeholder="## 本周重点工作&#10;- 完成了架构设计&#10;- 修复了核心 Bug..."></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-ghost" @click="showWriteReport = false">取消</button>
            <button class="btn-primary" @click="submitReport" :disabled="isSaving || !newReport.content.trim()">
              {{ isSaving ? '提交传输中...' : '提交汇报' }}
            </button>
          </div>
        </div>
      </div>
    </transition>

    <ReportDetailModal 
      v-if="viewingReport" 
      :report="viewingReport" 
      :current-user-id="currentUserId"
      :is-cycle-locked="currentReportCycle?.isLocked === 1"
      @close="viewingReport = null"
      @refresh="loadCycleData"
    />

    <transition name="modal-fade">
      <div v-if="showCycleManager" class="modal-backdrop" @click.self="showCycleManager = false">
        <div class="modal-panel medium">
          <div class="modal-header">
            <h3>管理汇报周期</h3>
            <button class="close-icon" @click="showCycleManager = false"><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg></button>
          </div>
          <div class="modal-body">
            <div class="add-cycle-box">
              <div class="input-group" style="flex: 2;">
                <label>周期名称</label>
                <input v-model="editingCycle.name" placeholder="如: 2026年 第9周" class="modern-input" />
              </div>
              <div class="input-group" style="flex: 1.5;">
                <label>开始日期</label>
                <input type="date" v-model="editingCycle.startDate" class="modern-input" />
              </div>
              <div class="input-group" style="flex: 1.5;">
                <label>结束日期</label>
                <input type="date" v-model="editingCycle.endDate" class="modern-input" />
              </div>
              <div class="input-group" style="justify-content: flex-end; padding-top: 22px;">
                <button class="btn-primary" @click="saveCycle">添加</button>
              </div>
            </div>

            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>周期名称</th>
                    <th>时间范围</th>
                    <th>状态</th>
                    <th style="text-align: right;">操作</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="c in reportCycles" :key="c.id">
                    <td class="fw-500">{{ c.name }}</td>
                    <td class="text-muted">{{ formatCycleDate(c.startDate, c.endDate) }}</td>
                    <td>
                      <span :class="['status-badge', c.isLocked ? 'locked' : 'open']">
                        {{ c.isLocked ? '🔒 已锁定' : '🟢 开放中' }}
                      </span>
                    </td>
                    <td style="text-align: right;">
                      <button class="btn-text" @click="toggleLock(c)">{{ c.isLocked ? '解锁' : '锁定' }}</button>
                      <button class="btn-text danger" @click="deleteCycle(c.id)">删除</button>
                    </td>
                  </tr>
                  <tr v-if="reportCycles.length === 0">
                    <td colspan="4" class="text-center text-muted" style="padding: 32px 0;">暂无汇报周期数据</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import ReportDetailModal from './DetailComponents/ReportDetailModal.vue'

const authStore = useAuthStore()
const currentUserId = computed(() => authStore.user?.id || 0)

const props = defineProps<{ 
  projectId: string | number, 
  members: any[] 
}>()

// --- 状态定义 ---
const reportCycles = ref<any[]>([])
const currentReportCycle = ref<any>(null)
const currentCycleReports = ref<any[]>([])
const memberStatusList = ref<any[]>([]) 

const showWriteReport = ref(false)
const viewingReport = ref<any>(null)
const isSaving = ref(false)

const newReport = reactive({ 
  type: 'Weekly', 
  cycleId: 0, 
  content: '',
  taskIds: [] 
})

// 管理周期状态
const showCycleManager = ref(false);
const editingCycle = reactive({ name: '', startDate: '', endDate: '' });

// --- 计算属性 ---
const stats = computed(() => {
  const total = memberStatusList.value.length;
  const submitted = memberStatusList.value.filter(m => m.hasSubmitted).length;
  return {
    total,
    submitted,
    percent: total > 0 ? Math.round((submitted / total) * 100) : 0
  }
})

const isViewer = computed(() => {
  const me = props.members.find(m => (m.userId || m.UserId) === currentUserId.value);
  // 注意兼容后端大小写
  const roleName = me?.role || me?.Role || ''; 
  return roleName === 'Viewer';
});




const userHasSubmitted = computed(() => {
  return memberStatusList.value.some(m => m.userId === currentUserId.value && m.hasSubmitted);
})

const isReportEdited = (report: any) => {
  if (!report.updatedAt) return false;
  const created = new Date(report.createdAt).getTime();
  const updated = new Date(report.updatedAt).getTime();
  return (updated - created) > 2000;
}

// --- 格式化工具 ---
const formatCycleDate = (start: string, end: string) => {
  if (!start || !end) return '';
  const s = new Date(start).toLocaleDateString('zh-CN', { month: 'short', day: 'numeric' })
  const e = new Date(end).toLocaleDateString('zh-CN', { month: 'short', day: 'numeric' })
  return `${s} - ${e}`
}

const formatTimeAgo = (dateString: string) => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);
  
  if (diffInSeconds < 60) return '刚刚';
  if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} 分钟前`;
  if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} 小时前`;
  if (diffInSeconds < 2592000) return `${Math.floor(diffInSeconds / 86400)} 天前`;
  return date.toLocaleDateString('zh-CN', { year: 'numeric', month: 'short', day: 'numeric' });
}

const formatExactTime = (dateString: string) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleString('zh-CN');
}

// --- 核心 API 请求逻辑 ---

/**
 * 获取周期列表：确保属性全部转为小写
 */
const fetchCycles = async () => {
  try {
    const res = await apiClient.get(`/Reports/project/${props.projectId}/cycles`);
    reportCycles.value = res.data.map((c: any) => ({
      id: c.id || c.Id,
      name: c.name || c.Name,
      startDate: c.startDate || c.StartDate,
      endDate: c.endDate || c.EndDate,
      type: c.type || c.Type,
      isLocked: c.isLocked || c.IsLocked
    }));

    if (reportCycles.value.length > 0) {
      // 默认选中第一个（最新）周期
      if (!currentReportCycle.value) {
        currentReportCycle.value = reportCycles.value[0];
      } else {
        // 如果已经有选中的，在列表中找回它以更新状态（如锁定状态）
        const updated = reportCycles.value.find(c => c.id === currentReportCycle.value.id);
        if (updated) currentReportCycle.value = updated;
      }
    }
  } catch (error) {
    console.error("获取周期失败", error);
  }
}

/**
 * 加载当前周期的汇报流和成员提交状态
 */
const loadCycleData = async () => {
  if (!currentReportCycle.value) return;
  
  const pid = props.projectId;
  const cid = currentReportCycle.value.id;

  try {
    const [feedRes, statusRes] = await Promise.all([
      apiClient.get(`/Reports/project/${pid}/cycle/${cid}/feed`),
      apiClient.get(`/Reports/project/${pid}/cycle/${cid}/status`)
    ]);

    // 映射 feed 数据 (保持不变)
    currentCycleReports.value = feedRes.data.map((r: any) => ({
      // ... 原有映射代码
      id: r.id || r.Id,
      title: r.title || r.Title,
      content: r.content || r.Content,
      type: r.type || r.Type,
      createdAt: r.createdAt || r.CreatedAt,
      updatedAt: r.updatedAt || r.UpdatedAt,
      userId: r.userId || r.UserId,
      userName: r.userName || r.UserName,
      completedTasks: (r.completedTasks || r.CompletedTasks || []).map((t: any) => ({
        taskId: t.taskId || t.TaskId,
        title: t.title || t.Title
      }))
    }));

    // 🔥 核心修改：筛选出不是 Viewer 的成员 ID 集合
    const requiredMemberIds = props.members
      .filter(m => (m.role || m.Role) !== 'Viewer')
      .map(m => m.userId || m.UserId);

    // 映射状态列表 (并只保留需要提交汇报的成员)
    memberStatusList.value = statusRes.data
      .filter((m: any) => requiredMemberIds.includes(m.userId || m.UserId))
      .map((m: any) => ({
        userId: m.userId || m.UserId,
        username: m.username || m.Username,
        hasSubmitted: m.hasSubmitted ?? m.HasSubmitted,
        reportId: m.reportId || m.ReportId,
        submittedAt: m.submittedAt || m.SubmittedAt
      }));

  } catch (error) {
    console.error("加载周期数据失败", error);
  }
}

// 监听周期切换
watch(() => currentReportCycle.value?.id, (newVal) => {
  if (newVal) {
    newReport.cycleId = newVal;
    loadCycleData();
  }
}, { immediate: true })

/**
 * 提交汇报
 */
const submitReport = async () => {
  if (!newReport.content.trim() || !currentReportCycle.value) return;
  isSaving.value = true;

  try {
    const payload = {
      projectId: Number(props.projectId),
      cycleId: currentReportCycle.value.id,
      title: `${currentReportCycle.value.name} 工作汇报`,
      content: newReport.content,
      type: newReport.type,
      taskIds: newReport.taskIds 
    };

    await apiClient.post('/Reports/submit', payload);
    
    showWriteReport.value = false;
    newReport.content = '';
    newReport.taskIds = [];
    await loadCycleData();
  } catch (error: any) {
    alert(error.response?.data || "提交失败");
  } finally {
    isSaving.value = false;
  }
}

const saveCycle = async () => {
  if (!editingCycle.name) return alert("请输入周期名称");
  try {
    const payload = {
      name: editingCycle.name,
      start_date: editingCycle.startDate, // 必须和后端 JSON 字段完全一致
      end_date: editingCycle.endDate,     // 必须和后端 JSON 字段完全一致
      projectId: Number(props.projectId)
      // 注意：如果你的后端也要求 project_id，这里也需要改成 project_id: ...
    };
    
    await apiClient.post(`/Reports/project/${props.projectId}/cycles`, payload);
    
    editingCycle.name = ''; 
    editingCycle.startDate = ''; 
    editingCycle.endDate = '';
    await fetchCycles(); 
  } catch (error: any) {
    alert(error.response?.data || "添加周期失败");
  }
}

const deleteCycle = async (id: number) => {
  if (!confirm("确定要删除这个周期吗？该操作不可恢复！")) return;
  try {
    await apiClient.delete(`/Reports/cycles/${id}`);
    if (currentReportCycle.value?.id === id) currentReportCycle.value = null;
    await fetchCycles();
  } catch (error: any) {
    alert("删除失败");
  }
}

const toggleLock = async (cycle: any) => {
  try {
    await apiClient.patch(`/Reports/cycles/${cycle.id}/toggle-lock`);
    await fetchCycles(); 
  } catch (error: any) {
    alert("操作失败");
  }
}

const openWriteReportModal = () => { 
  if (currentReportCycle.value?.isLocked) return;
  showWriteReport.value = true; 
}

const viewReport = (r: any) => { viewingReport.value = r }

defineExpose({ openWriteReportModal })

onMounted(() => {
  fetchCycles();
})
</script>

<style scoped>
/* ================= 核心变量 (Design Tokens) ================= */
.reports-canvas {
  --primary: #0052CC;
  --primary-hover: #0043A6;
  --success: #36B37E;
  --success-bg: #E3FCEF;
  --success-text: #006644;
  --danger: #DE350B;
  --danger-bg: #FFEBE6;
  --danger-text: #BF2600;
  --warning-bg: #FFFAE6;
  --warning-text: #FF8B00;
  
  --bg-main: #F7F8FA;
  --bg-surface: #FFFFFF;
  --bg-hover: #F4F5F7;
  --bg-active: #E9F2FF;
  
  --text-main: #172B4D;
  --text-muted: #6B778C;
  --text-inverse: #FFFFFF;
  
  --border-light: #EBECF0;
  --border-main: #DFE1E6;
  --border-focus: #4C9AFF;
  
  --shadow-sm: 0 1px 2px rgba(9, 30, 66, 0.05);
  --shadow-md: 0 4px 8px -2px rgba(9, 30, 66, 0.08), 0 0 1px rgba(9, 30, 66, 0.31);
  --shadow-lg: 0 8px 16px -4px rgba(9, 30, 66, 0.08), 0 0 1px rgba(9, 30, 66, 0.31);
  --shadow-modal: 0 16px 32px -8px rgba(9, 30, 66, 0.12), 0 0 1px rgba(9, 30, 66, 0.31);
  
  flex: 1;
  overflow: hidden;
  display: flex;
  background: var(--bg-main);
  height: 100%;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: var(--text-main);
}

/* 自定义滚动条 */
::-webkit-scrollbar { width: 6px; height: 6px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: #C1C7D0; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #A5ADBA; }

/* ================= 整体布局 ================= */
.reports-layout { display: flex; width: 100%;  margin: 0 auto; height: 100%; }

/* ================= 💥 暴力锁定全宇宙头像尺寸 💥 ================= */
.strict-avatar-28 { width: 28px; height: 28px; min-width: 28px; max-width: 28px; border-radius: 50%; overflow: hidden; display: block; flex-shrink: 0; }
.strict-avatar-32 { width: 32px; height: 32px; min-width: 32px; max-width: 32px; border-radius: 50%; overflow: hidden; display: block; flex-shrink: 0; }

.strict-avatar-28 :deep(*),
.strict-avatar-32 :deep(*) {
  width: 100% !important;
  height: 100% !important;
  min-width: 100% !important;
  min-height: 100% !important;
  max-width: 100% !important;
  max-height: 100% !important;
  object-fit: cover !important;
  border-radius: 50% !important;
  display: block !important;
  margin: 0 !important;
  padding: 0 !important;
}

/* ================= 三栏分离布局设计 ================= */
.status-sidebar { width: 240px; padding: 24px 20px; border-right: 1px solid var(--border-main); background: var(--bg-surface); overflow-y: auto; display: flex; flex-direction: column; z-index: 10;}
.cycle-sidebar { width: 260px; padding: 24px 20px; border-right: 1px solid var(--border-main); background: var(--bg-surface); overflow-y: auto; display: flex; flex-direction: column; z-index: 9;}

.sidebar-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.section-title { font-size: 13px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; margin: 0; }
.stats-count { font-weight: normal; font-size: 12px; }

.btn-icon-small { background: transparent; border: none; cursor: pointer; color: var(--text-muted); border-radius: 4px; transition: all 0.2s; padding: 6px; display: flex; align-items: center; justify-content: center; }
.btn-icon-small:hover { background: var(--bg-hover); color: var(--text-main); }

.cycle-list { display: flex; flex-direction: column; gap: 4px; }
.cycle-item { position: relative; padding: 12px 16px; border-radius: 6px; cursor: pointer; transition: all 0.2s ease; border: 1px solid transparent; }
.cycle-item:hover { background: var(--bg-hover); }
.cycle-item.active { background: var(--bg-active); border-color: rgba(0, 82, 204, 0.2); }
.cycle-info { display: flex; flex-direction: column; gap: 4px; }
.cycle-title { font-weight: 600; font-size: 14px; color: var(--text-main); display: flex; align-items: center; gap: 6px; }
.cycle-item.active .cycle-title { color: var(--primary); }
.cycle-date { font-size: 12px; color: var(--text-muted); }
.lock-icon { color: var(--text-muted); display: flex; align-items: center; }
.active-indicator { position: absolute; left: 0; top: 50%; transform: translateY(-50%); width: 3px; height: 20px; background: var(--primary); border-radius: 0 4px 4px 0; }

.member-status-list { display: flex; flex-direction: column; gap: 8px; }
.member-status-row { display: flex; align-items: center; gap: 12px; padding: 8px 12px; border-radius: 6px; transition: background 0.2s; }
.member-status-row:hover { background: var(--bg-hover); }
.m-avatar-wrapper { position: relative; width: 28px; height: 28px; flex-shrink: 0; border-radius: 50%; }
.status-indicator { position: absolute; bottom: -2px; right: -2px; width: 10px; height: 10px; border-radius: 50%; border: 2px solid var(--bg-surface); z-index: 2; }
.status-indicator.done { background: var(--success); }
.status-indicator.missing { background: var(--danger); }
.m-name { flex: 1; font-size: 14px; font-weight: 500; color: var(--text-main); white-space: nowrap; overflow: hidden; text-overflow: ellipsis;}
.status-badge-small { font-size: 11px; padding: 2px 6px; border-radius: 12px; font-weight: 600; }
.status-badge-small.done { background: var(--success-bg); color: var(--success-text); }
.status-badge-small.missing { background: var(--danger-bg); color: var(--danger-text); }

/* ================= 右侧主信息流 ================= */
.report-feed { flex: 1; padding: 40px 60px; overflow-y: auto; position: relative; }
.feed-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 32px; padding-bottom: 24px; border-bottom: 1px solid var(--border-main); }
.header-content { flex: 1; }
.feed-title { margin: 0 0 8px 0; font-size: 28px; font-weight: 700; color: var(--text-main); letter-spacing: -0.5px; }
.feed-meta { display: flex; align-items: center; gap: 12px; color: var(--text-muted); font-size: 14px; margin-bottom: 20px; }
.meta-dot { font-weight: bold; }

.progress-section { max-width: 400px; }
.progress-info { display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 13px; font-weight: 600; color: var(--text-main); }
.progress-track { height: 6px; background: var(--border-light); border-radius: 3px; overflow: hidden; }
.progress-fill { height: 100%; background: var(--success); border-radius: 3px; transition: width 0.8s cubic-bezier(0.2, 0.8, 0.2, 1); }

.header-actions { margin-left: 24px; }

/* ================= 报告卡片 ================= */
.report-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
.report-card { background: var(--bg-surface); border: 1px solid var(--border-main); border-radius: 8px; padding: 20px; cursor: pointer; transition: all 0.2s ease; display: flex; flex-direction: column; box-shadow: var(--shadow-sm); min-height: 160px; }
.report-card:hover { transform: translateY(-4px); border-color: var(--border-focus); box-shadow: var(--shadow-lg); }

/* 头部排版优化：解决名字过长导致的竖排问题 */
.rep-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; gap: 12px; }
.rep-user { display: flex; gap: 8px; align-items: center; min-width: 0; flex: 1; }

.rep-meta-info { display: flex; align-items: center; gap: 6px; white-space: nowrap; overflow: hidden; }
.rep-name { font-weight: 600; font-size: 13px; color: var(--text-main); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 120px; }
.rep-time { font-size: 12px; color: var(--text-muted); display: flex; align-items: center; flex-shrink: 0; }
.rep-time::before { content: "·"; margin-right: 6px; font-weight: bold; opacity: 0.5; }

/* 🌟 已编辑标签样式 */
.edited-badge { margin-left: 4px; font-size: 11px; color: #8993A4; font-style: italic; font-weight: normal; }

.rep-type-badge { font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; letter-spacing: 0.5px; flex-shrink: 0; }
.rep-type-badge.weekly { background: var(--success-bg); color: var(--success-text); }
.rep-type-badge.monthly { background: var(--bg-active); color: var(--primary); }

.rep-preview { font-size: 13px; color: #42526E; line-height: 1.6; flex: 1; margin-bottom: 16px; white-space: pre-wrap; word-break: break-word; }

.rep-footer { display: flex; justify-content: space-between; align-items: center; margin-top: auto; padding-top: 12px; border-top: 1px solid var(--border-light); }
.task-count-tag { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 500; color: var(--text-muted); background: var(--bg-main); padding: 4px 8px; border-radius: 12px; border: 1px solid var(--border-main); }
.read-more { font-size: 12px; font-weight: 600; color: var(--primary); opacity: 0; transition: opacity 0.2s; }
.report-card:hover .read-more { opacity: 1; }

/* ================= 空状态 ================= */
.empty-state { text-align: center; padding: 80px 20px; background: transparent; border: 2px dashed var(--border-main); border-radius: 12px; margin-top: 24px; }
.empty-icon-wrapper { color: var(--text-muted); margin-bottom: 16px; opacity: 0.5; }
.empty-title { font-size: 18px; font-weight: 600; color: var(--text-main); margin: 0 0 8px 0; }
.empty-desc { font-size: 14px; color: var(--text-muted); margin: 0 0 24px 0; }

/* ================= 按钮样式 ================= */
button { font-family: inherit; }
button:disabled { cursor: not-allowed; opacity: 0.6; }
.btn-primary { display: inline-flex; align-items: center; justify-content: center; background: var(--primary); color: var(--text-inverse); border: none; border-radius: 4px; padding: 8px 16px; font-weight: 500; font-size: 14px; cursor: pointer; transition: background 0.2s, box-shadow 0.2s; }
.btn-primary:not(:disabled):hover { background: var(--primary-hover); box-shadow: var(--shadow-sm); }
.btn-success { display: inline-flex; align-items: center; justify-content: center; background: var(--success-bg); color: var(--success-text); border: 1px solid var(--success); border-radius: 4px; padding: 8px 16px; font-weight: 600; font-size: 14px; }
.btn-ghost { background: transparent; border: 1px solid transparent; color: var(--text-main); cursor: pointer; padding: 8px 16px; font-size: 14px; font-weight: 500; border-radius: 4px; transition: background 0.2s; }
.btn-ghost:not(:disabled):hover { background: var(--bg-hover); }
.btn-text { background: none; border: none; color: var(--primary); cursor: pointer; font-weight: 500; font-size: 13px; padding: 4px 8px; border-radius: 4px; transition: background 0.2s; }
.btn-text:hover { background: var(--bg-active); }
.btn-text.danger { color: var(--danger); }
.btn-text.danger:hover { background: var(--danger-bg); }

/* ================= 模态框通用体系 ================= */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-panel { background: var(--bg-surface); border-radius: 8px; display: flex; flex-direction: column; box-shadow: var(--shadow-modal); max-width: 90vw; }
.modal-panel.medium { width: 680px; max-height: 85vh; }
.modal-panel.large { width: 900px; height: 85vh; }
.modal-header { padding: 20px 24px; border-bottom: 1px solid var(--border-main); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 18px; font-weight: 600; color: var(--text-main); display: flex; align-items: center; gap: 8px;}
.header-cycle-name { color: var(--text-muted); font-weight: normal; font-size: 16px; }
.modal-body { padding: 24px; flex: 1; overflow-y: auto; display: flex; flex-direction: column; }
.modal-footer { padding: 16px 24px; border-top: 1px solid var(--border-main); display: flex; justify-content: flex-end; gap: 12px; background: #FAFBFC; border-radius: 0 0 8px 8px; }

.close-icon { background: none; border: none; cursor: pointer; color: var(--text-muted); border-radius: 4px; padding: 4px; display: flex; align-items: center; justify-content: center; transition: background 0.2s; }
.close-icon:hover { background: var(--bg-hover); color: var(--text-main); }

/* ================= 表单与输入框 ================= */
.form-group { margin-bottom: 20px; display: flex; flex-direction: column; }
.form-row { display: flex; gap: 20px; }
.form-row .form-group { flex: 1; }
.form-group label { font-size: 13px; font-weight: 600; color: var(--text-main); margin-bottom: 8px; }
.flex-fill { flex: 1; }

.modern-input, .modern-textarea { width: 100%; padding: 10px 12px; border: 2px solid var(--border-main); border-radius: 6px; font-size: 14px; background: var(--bg-surface); color: var(--text-main); transition: border-color 0.2s, box-shadow 0.2s; box-sizing: border-box; font-family: inherit; }
.modern-input:focus, .modern-textarea:focus { outline: none; border-color: var(--border-focus); box-shadow: 0 0 0 3px rgba(76, 154, 255, 0.2); }
.modern-input.disabled { background: var(--bg-main); color: var(--text-muted); cursor: not-allowed; }
.modern-textarea { flex: 1; min-height: 200px; resize: vertical; line-height: 1.6; }

.select-wrapper { position: relative; }
.select-wrapper::after { content: "▼"; position: absolute; right: 12px; top: 50%; transform: translateY(-50%); font-size: 10px; color: var(--text-muted); pointer-events: none; }
.select-wrapper select { appearance: none; cursor: pointer; }

/* ================= 周期管理特定样式 ================= */
.add-cycle-box { display: flex; gap: 16px; align-items: flex-start; margin-bottom: 24px; background: var(--bg-main); padding: 20px; border-radius: 8px; border: 1px solid var(--border-main); }
.input-group { display: flex; flex-direction: column; gap: 8px; }
.input-group label { font-size: 12px; font-weight: 600; color: var(--text-muted); margin: 0; }

.table-container { border: 1px solid var(--border-main); border-radius: 8px; overflow: hidden; }
.admin-table { width: 100%; border-collapse: collapse; text-align: left; background: var(--bg-surface); }
.admin-table th { padding: 14px 16px; background: var(--bg-main); border-bottom: 1px solid var(--border-main); color: var(--text-muted); font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; }
.admin-table td { padding: 14px 16px; border-bottom: 1px solid var(--border-light); font-size: 14px; color: var(--text-main); vertical-align: middle; }
.admin-table tr:last-child td { border-bottom: none; }
.admin-table tr:hover td { background: var(--bg-hover); }
.fw-500 { font-weight: 500; }
.text-muted { color: var(--text-muted); }
.text-center { text-align: center; }

.status-badge { padding: 4px 10px; border-radius: 12px; font-size: 12px; font-weight: 600; display: inline-flex; align-items: center; gap: 4px; }
.status-badge.open { background: var(--success-bg); color: var(--success-text); }
.status-badge.locked { background: var(--border-light); color: var(--text-main); }

/* Vue 动画过渡 */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s cubic-bezier(0.2, 0.8, 0.2, 1); }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: translateY(10px) scale(0.98); }
</style>