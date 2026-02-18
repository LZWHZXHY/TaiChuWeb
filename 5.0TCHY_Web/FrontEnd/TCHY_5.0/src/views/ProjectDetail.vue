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

        <button v-if="currentView === 'board'" class="btn-primary" @click="triggerCreateTask">
          <span>+ 新建任务</span>
        </button>
        <button v-else class="btn-primary" @click="triggerWriteReport">
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
      
      <ReportDashboard 
        v-if="currentView === 'reports'" 
        ref="reportRef"
        :project-id="projectId" 
        :members="projectMembers"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import KanbanBoard from '@/ProjectComponents/KanbanBoard.vue'
import ReportDashboard from '@/ProjectComponents/ReportDashboard.vue'

const route = useRoute()
const router = useRouter()
const projectId = route.params.id as string

const currentView = ref('board')
const projectInfo = ref<any>({})
const projectMembers = ref<any[]>([])
const stats = ref({ percentage: 0, done: 0, total: 0 })

// 子组件引用
const kanbanRef = ref<InstanceType<typeof KanbanBoard> | null>(null)
const reportRef = ref<InstanceType<typeof ReportDashboard> | null>(null)

// API
const fetchProjectData = async () => {
  try {
    const res = await apiClient.get(`/projects/${projectId}`)
    projectInfo.value = { id: res.data.Id, name: res.data.Name, creatorName: res.data.CreatorName, orgId: res.data.OrganizationId }
    fetchMembers(res.data.OrganizationId)
  } catch (e) { console.error(e) }
}

const fetchMembers = async (orgId: number) => {
  try {
    const res = await apiClient.get(`/organizations/${orgId}/members`)
    projectMembers.value = res.data.map((m: any) => ({ userId: m.UserId, username: m.Username, avatar: m.Avatar }))
  } catch (e) { console.error(e) }
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

/* 通用 Header 元素 (复制自原代码) */
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
</style>