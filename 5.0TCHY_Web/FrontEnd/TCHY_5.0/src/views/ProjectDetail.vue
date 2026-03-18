<template>
  <div class="linear-layout">
    <header class="linear-header">
      <div class="header-left">
        <button class="icon-btn" @click="router.push('/workspace')">
          <span class="symbol">←</span>
        </button>
        <div class="breadcrumb">
          <h1 class="project-name">{{ projectInfo.name || 'Loading...' }}</h1>
          <span class="separator">/</span>
          <div class="view-switcher">
            <button class="view-tab" :class="{ active: currentView === 'board' }" @click="currentView = 'board'">
              <span class="icon">⊞</span> Board
            </button>
            <button class="view-tab" :class="{ active: currentView === 'timeline' }" @click="currentView = 'timeline'">
              <span class="icon">◤</span> Timeline
            </button>
            <button class="view-tab" :class="{ active: currentView === 'reports' }" @click="currentView = 'reports'">
              <span class="icon">📄</span> Reports
            </button>
          </div>
        </div>
      </div>

      <div class="header-right">
        <div v-if="currentView === 'board' && stats.total > 0" class="status-capsule">
          <div class="status-dot" :class="{ 'done': stats.percentage === 100 }"></div>
          <span>{{ stats.percentage }}% ({{ stats.done }}/{{ stats.total }})</span>
        </div>

        <div class="avatar-group">
          <div class="avatar-ring" v-for="m in projectMembers.slice(0,5)" :key="m.userId">
            <UniversalAvatar :user-id="m.userId" :passed-avatar="m.avatar" :show-level="false" />
          </div>
        </div>

        <button class="icon-btn" @click="showAddMemberModal = true" title="招募项目干员" style="margin-right: 12px;">
          <span class="symbol">+👥</span>
        </button>

        <button v-if="currentView === 'board'" class="btn-primary" @click="triggerCreateTask">
          <span>+ 新建任务</span>
        </button>
        <button v-else-if="currentView === 'reports'" class="btn-primary" @click="triggerWriteReport">
          <span>+ 撰写汇报</span>
        </button>
        </div>
    </header>

    <div class="main-content">
      <KanbanBoard 
        v-if="currentView === 'board'" 
        ref="kanbanRef"
        :project-id="projectId" 
        :members="projectMembers"
        @update-progress="updateStats"
      />
      
      <ProjectTimeline 
        v-if="currentView === 'timeline'" 
        :project-id="projectId" 
      />
      
      <ReportDashboard 
        v-if="currentView === 'reports'" 
        ref="reportRef"
        :project-id="projectId" 
        :members="projectMembers"
      />
    </div>

    <div v-if="showAddMemberModal" class="modal-overlay" @click.self="showAddMemberModal = false">
      <div class="modal-content cyber-modal">
        <h3>// 调配干员至本扇区 (项目)</h3>
        <form @submit.prevent="submitAddProjectMember">
          
          <div class="form-group">
            <label>选择待分配的组织干员</label>
            <select v-model="selectedUserId" class="cyber-select" required>
              <option :value="null" disabled>-- 请选择干员 --</option>
              <option 
                v-for="member in availableOrgMembers" 
                :key="member.userId" 
                :value="member.userId"
              >
                {{ member.username }}
              </option>
            </select>
            <span class="help-text" v-if="availableOrgMembers.length === 0">
              当前组织内没有可分配的闲置干员。请先在工作区邀请更多人加入组织。
            </span>
          </div>

          <div class="form-group" v-if="availableOrgMembers.length > 0">
            <label>授予项目权限</label>
            <select v-model="selectedRole" class="cyber-select" required>
              <option value="Admin">Admin (项目管理员)</option>
              <option value="Member">Member (普通执行者)</option>
              <option value="Viewer">Viewer (仅查看/观察者)</option>
            </select>
          </div>

          <div class="form-actions">
            <button type="button" class="modal-btn cancel" @click="showAddMemberModal = false">取消</button>
            <button type="submit" class="modal-btn submit" :disabled="isAdding || !selectedUserId || availableOrgMembers.length === 0">
              {{ isAdding ? '传输中...' : '确认授权' }}
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import KanbanBoard from '@/ProjectComponents/KanbanBoard.vue'
import ReportDashboard from '@/ProjectComponents/ReportDashboard.vue'
import ProjectTimeline from '@/ProjectComponents/ProjectTimeline.vue'

const route = useRoute()
const router = useRouter()
const projectId = route.params.id as string

const currentView = ref('board')
const projectInfo = ref<any>({})
const projectMembers = ref<any[]>([]) // 🌟 真正进入本项目的成员
const orgMembers = ref<any[]>([])     // 🌟 整个组织的成员
const stats = ref({ percentage: 0, done: 0, total: 0 })

// 子组件引用
const kanbanRef = ref<InstanceType<typeof KanbanBoard> | null>(null)
const reportRef = ref<InstanceType<typeof ReportDashboard> | null>(null)

// 🌟 UI 状态 (招募弹窗)
const showAddMemberModal = ref(false)
const selectedUserId = ref<number | null>(null)
const selectedRole = ref('Member')
const isAdding = ref(false)

// 🌟 计算属性：筛选出还没进这个项目的组织成员
const availableOrgMembers = computed(() => {
  return orgMembers.value.filter(orgM => 
    !projectMembers.value.some(projM => projM.userId === orgM.userId)
  )
})

// API
const fetchProjectData = async () => {
  try {
    const res = await apiClient.get(`/projects/${projectId}`)
    projectInfo.value = { id: res.data.Id, name: res.data.Name, creatorName: res.data.CreatorName, orgId: res.data.OrganizationId }
    
    // 🌟 分别拉取项目成员和组织成员
    fetchProjectMembers()
    fetchOrgMembers(res.data.OrganizationId)
  } catch (e) { console.error(e) }
}

// 🌟 获取本项目的成员 (替代原来的 fetchMembers)
const fetchProjectMembers = async () => {
  try {
    const res = await apiClient.get(`/projects/${projectId}/members`)
    projectMembers.value = res.data.map((m: any) => ({ 
      userId: m.UserId || m.userId, 
      username: m.Username || m.username,
      avatar: m.Avatar || m.avatar,
      role: m.Role || m.role 
    }))
  } catch (e) { console.error(e) }
}

// 🌟 获取整个组织的后备干员
const fetchOrgMembers = async (orgId: number) => {
  try {
    const res = await apiClient.get(`/organizations/${orgId}/members`)
    orgMembers.value = res.data.map((m: any) => ({ 
      userId: m.UserId || m.userId, 
      username: m.Username || m.username,
      avatar: m.Avatar || m.avatar
    }))
  } catch (e) { console.error(e) }
}

// 🌟 提交拉人请求
const submitAddProjectMember = async () => {
  if (!selectedUserId.value) return;
  isAdding.value = true;
  try {
    await apiClient.post(`/projects/${projectId}/members`, {
      UserId: selectedUserId.value,
      RoleInProject: selectedRole.value
    });
    
    showAddMemberModal.value = false;
    selectedUserId.value = null;
    selectedRole.value = 'Member';
    
    // 拉人成功后，重新刷新项目成员列表
    await fetchProjectMembers();
  } catch (error: any) {
    console.error('分配失败:', error);
    alert(error.response?.data?.message || '分配干员失败');
  } finally {
    isAdding.value = false;
  }
}

// 交互
const triggerCreateTask = () => {
  kanbanRef.value?.openCreateModal()
}

const triggerWriteReport = () => {
  reportRef.value?.openWriteReportModal()
}

const updateStats = (data: any) => {
  stats.value = { 
    percentage: data.percentage, 
    done: data.count.done, 
    total: data.count.total 
  }
}

onMounted(() => fetchProjectData())
</script>

<style scoped>
/* 基础布局 */
.linear-layout {
  position: absolute; top: 72px; left: 0; right: 0; bottom: 0;
  background-color: #F4F5F7; display: flex; flex-direction: column;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; color: #172B4D;
}
.linear-header {
  height: 56px; padding: 0 24px; background: #fff; border-bottom: 1px solid #EBECF0;
  display: flex; justify-content: space-between; align-items: center; flex-shrink: 0;
}
.header-left { display: flex; align-items: center; gap: 16px; }
.header-right { display: flex; align-items: center; gap: 16px; }
.main-content { flex: 1; overflow: hidden; display: flex; flex-direction: column; }

/* 通用 Header 元素 */
.icon-btn { border: 1px solid #DFE1E6; background: #fff; width: 28px; height: 28px; border-radius: 4px; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.project-name { font-size: 16px; font-weight: 600; margin: 0; }
.separator { color: #6B778C; margin: 0 8px; }
.view-switcher { display: flex; background: #EBECF0; padding: 2px; border-radius: 6px; margin-left: 12px; }
.view-tab { display: flex; align-items: center; gap: 6px; border: none; background: transparent; padding: 4px 12px; font-size: 13px; font-weight: 600; color: #6B778C; cursor: pointer; border-radius: 4px; transition: 0.2s; }
.view-tab.active { background: #fff; color: #0052CC; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
.status-capsule { font-size: 12px; font-weight: 500; display: flex; align-items: center; gap: 6px; color: #5E6C84; }
.status-dot { width: 8px; height: 8px; border-radius: 50%; background: #0052CC; }
.status-dot.done { background: #36B37E; }
.avatar-ring { width: 24px; height: 24px; border-radius: 50%; border: 2px solid #fff; margin-left: -8px; overflow: hidden; }
.avatar-group { display: flex; padding-left: 8px; }
.btn-primary { background: #0052CC; color: #fff; border: none; border-radius: 3px; padding: 6px 12px; font-weight: 500; font-size: 13px; cursor: pointer; }

/* 🌟 新增：招募弹窗相关样式 */
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(9, 30, 66, 0.54); backdrop-filter: blur(2px);
  display: flex; justify-content: center; align-items: center; z-index: 2000;
}
.modal-content.cyber-modal {
  background: #fff; padding: 32px; border-radius: 4px;
  width: 100%; max-width: 420px;
  box-shadow: 0 8px 16px -4px rgba(9,30,66,0.25);
}
.modal-content h3 { margin-top: 0; font-size: 18px; margin-bottom: 24px; color: #172B4D; border-left: 4px solid #0052CC; padding-left: 12px;}
.form-group { margin-bottom: 20px; }
.form-group label { display: block; font-size: 12px; font-weight: 600; margin-bottom: 6px; color: #5E6C84; }
.cyber-select {
  width: 100%; padding: 8px 12px; border: 2px solid #DFE1E6; border-radius: 4px;
  font-family: inherit; font-size: 14px; outline: none; background: #FAFBFC;
}
.cyber-select:focus { border-color: #4C9AFF; background: #fff; }
.help-text { font-size: 11px; color: #DE350B; margin-top: 6px; display: block; }
.form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 32px; }
.modal-btn { padding: 8px 16px; font-weight: 600; cursor: pointer; border-radius: 4px; border: none; font-size: 14px; }
.modal-btn.cancel { background: transparent; color: #5E6C84; }
.modal-btn.cancel:hover { background: #091E420F; }
.modal-btn.submit { background: #0052CC; color: #fff; }
.modal-btn.submit:hover:not(:disabled) { background: #0047B3; }
.modal-btn:disabled { opacity: 0.5; cursor: not-allowed; }
</style>