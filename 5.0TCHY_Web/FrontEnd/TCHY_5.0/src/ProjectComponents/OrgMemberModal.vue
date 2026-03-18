<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content cyber-modal ultra-wide">
      
      <div class="modal-header-row">
        <div class="header-left">
          <h3>// 组织神经中枢：人员与权限配置</h3>
          <span class="member-count">在编干员：{{ memberList.length }} 人</span>
        </div>
        
        <div class="header-right">
          <div class="role-legend">
            <span class="legend-item"><i class="dot gold"></i>Owner</span>
            <span class="legend-item"><i class="dot red"></i>Admin</span>
            <span class="legend-item"><i class="dot blue"></i>Member</span>
          </div>
          <button class="modal-btn close-btn" @click="emit('close')">✖ 关闭终端</button>
        </div>
      </div>

      <div v-if="loading" class="global-loading">
        扫描组织名册中，请稍候...
      </div>

      <div v-else class="split-workspace">
        
        <aside class="member-sidebar">
          <div class="sidebar-search-bar">
            <span class="sidebar-title">干员花名册</span>
          </div>
          
          <div class="sidebar-list">
            <div 
              v-for="member in memberList" 
              :key="member.userId" 
              class="sidebar-item" 
              :class="{ 'is-active': selectedMemberId === member.userId }"
              @click="selectMember(member)"
            >
              <div class="s-avatar">
                <img v-if="member.avatar" :src="member.avatar" />
                <span v-else>{{ member.username.charAt(0).toUpperCase() }}</span>
              </div>
              <div class="s-info">
                <div class="s-name">
                  {{ member.username }}
                  <span class="s-you" v-if="member.userId === myUserId">(我)</span>
                </div>
                <div class="s-role" :class="member.role">{{ member.role }}</div>
              </div>
              <div class="s-arrow">›</div>
            </div>
          </div>
        </aside>

        <main class="member-detail-panel">
          
          <div v-if="!activeMember" class="empty-state">
            <div class="empty-icon">⎔</div>
            <p>请在左侧选择干员以进行调配</p>
          </div>

          <div v-else class="detail-container">
            <div class="detail-header">
              <div class="d-avatar">
                <img v-if="activeMember.avatar" :src="activeMember.avatar" />
                <span v-else>{{ activeMember.username.charAt(0).toUpperCase() }}</span>
              </div>
              <div class="d-info">
                <h2>{{ activeMember.username }}</h2>
                <div class="d-meta">
                  <span class="d-level">Lv.{{ activeMember.level || 0 }}</span>
                  <span class="d-divider">|</span>
                  <span class="d-date">入职时间: {{ formatDate(activeMember.joinedAt) }}</span>
                </div>
              </div>
            </div>

            <div class="detail-scroll-area">
              <section class="control-card">
                <div class="card-title">
                  <span class="bracket">[</span> 组织级别调度 <span class="bracket">]</span>
                </div>
                <div class="card-content org-control-row">
                  <div class="control-group">
                    <label>组织角色分配</label>
                    <select 
                      v-if="canManageRole(activeMember.role)" 
                      v-model="activeMember.role" 
                      @change="updateMemberRole(activeMember)"
                      class="cyber-select large"
                      :class="activeMember.role"
                    >
                      <option value="Admin">Admin (管理员)</option>
                      <option value="Member">Member (标准执行者)</option>
                      <option value="Guest">Guest (访客)</option>
                    </select>
                    <div v-else class="role-badge large" :class="activeMember.role">{{ activeMember.role }}</div>
                  </div>

                  <div class="control-group action-right" v-if="canManageRole(activeMember.role) && activeMember.userId !== myUserId">
                    <label class="danger-text">危险操作</label>
                    <button class="btn-danger" @click="removeMemberFromOrg(activeMember)">
                      ⚠️ 从组织彻底除名
                    </button>
                  </div>
                </div>
              </section>

              <section class="control-card">
                <div class="card-title">
                  <span class="bracket">[</span> 参与的项目/扇区配置 <span class="bracket">]</span>
                </div>
                
                <div class="card-content project-control-area">
                  <div v-if="activeMember.projectsLoading" class="loading-text">
                    正在同步干员扇区轨迹...
                  </div>
                  
                  <div v-else-if="activeMember.projects && activeMember.projects.length > 0" class="project-grid">
                    <div class="project-item" v-for="proj in activeMember.projects" :key="proj.projectId">
                      <div class="p-header">
                        <span class="p-icon">◈</span>
                        <span class="p-name">{{ proj.name }}</span>
                      </div>
                      <div class="p-controls">
                        <select 
                          v-model="proj.role" 
                          @change="updateProjectRole(activeMember.userId, proj)"
                          class="cyber-select"
                        >
                          <option value="Admin">Admin</option>
                          <option value="Member">Member</option>
                          <option value="Viewer">Viewer</option>
                        </select>
                        <button class="btn-icon-danger" @click="removeMemberFromProject(activeMember.userId, proj.projectId)" title="撤出该扇区">
                          ✖
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <div v-else class="empty-projects">
                    该干员当前处于待命状态，未被分配到任何具体项目。
                  </div>
                </div>
              </section>
            </div>
          </div>

        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps<{
  orgId: number;
  myUserId: number;
  myRole: string;
}>()

const emit = defineEmits(['close', 'role-changed'])

interface MemberProject { projectId: number; name: string; role: string; }
interface Member {
  userId: number; username: string; avatar?: string; level?: number; 
  role: string; joinedAt: string;
  projects?: MemberProject[]; projectsLoading?: boolean;
}

const memberList = ref<Member[]>([])
const loading = ref(true)

// 🔥 记录选中的干员ID
const selectedMemberId = ref<number | null>(null)

// 🔥 计算属性：获取当前选中的干员对象
const activeMember = computed(() => {
  return memberList.value.find(m => m.userId === selectedMemberId.value)
})

const fetchMembers = async () => {
  loading.value = true
  try {
    const res = await apiClient.get<any[]>(`/organizations/${props.orgId}/members`)
    memberList.value = res.data.map(m => ({
      userId: m.UserId || m.userId,
      username: m.Username || m.username,
      avatar: m.Avatar || m.avatar,
      level: m.Level || m.level,
      role: m.Role || m.role,
      joinedAt: m.JoinedAt || m.joinedAt
    }))

    // 如果有干员且还没选中，默认选中列表第一个人
    if (memberList.value.length > 0 && !selectedMemberId.value) {
      selectMember(memberList.value[0])
    }
  } catch (err) {
    console.error('Fetch Members Error', err)
  } finally {
    loading.value = false
  }
}

// 🔥 选中干员，并自动拉取TA的项目数据
const selectMember = async (member: Member) => {
  selectedMemberId.value = member.userId
  
  if (!member.projects) {
    member.projectsLoading = true
    try {
      const res = await apiClient.get<any[]>(`/organizations/${props.orgId}/members/${member.userId}/projects`)
      member.projects = res.data.map(p => ({
        projectId: p.projectId || p.ProjectId,
        name: p.projectName || p.ProjectName,
        role: p.role || p.Role 
      }))
    } catch (err) {
      console.error('获取干员项目轨迹失败', err)
      member.projects = []
    } finally {
      member.projectsLoading = false
    }
  }
}

const updateMemberRole = async (member: Member) => {
  try {
    await apiClient.put(`/organizations/${props.orgId}/members/${member.userId}/role`, { Role: member.role })
    if (member.userId === props.myUserId) {
      emit('role-changed', member.role)
    }
  } catch (err) {
    alert('权限更新失败')
    await fetchMembers() 
  }
}

const updateProjectRole = async (userId: number, proj: MemberProject) => {
  try {
    await apiClient.put(`/projects/${proj.projectId}/members/${userId}/role`, { Role: proj.role })
  } catch (err) {
    alert('扇区权限配置失败，请检查您的操作权限。')
  }
}

const removeMemberFromProject = async (userId: number, projectId: number) => {
  if (!confirm('确定将该干员从该扇区(项目)撤出吗？')) return
  try {
    await apiClient.delete(`/projects/${projectId}/members/${userId}`)
    const targetMember = memberList.value.find(m => m.userId === userId)
    if (targetMember && targetMember.projects) {
      targetMember.projects = targetMember.projects.filter(p => p.projectId !== projectId)
    }
  } catch (err) {
    alert('撤出失败。')
  }
}

const removeMemberFromOrg = async (member: Member) => {
  if (!confirm(`警告：确定要将 [${member.username}] 彻底移出当前组织吗？他们将失去所有项目的访问权限。`)) return
  try {
    await apiClient.delete(`/organizations/${props.orgId}/members/${member.userId}`)
    memberList.value = memberList.value.filter(m => m.userId !== member.userId)
    if(selectedMemberId.value === member.userId) {
      selectedMemberId.value = null
    }
  } catch (err) {
    alert('除名操作失败，可能权限不足。')
  }
}

const canManageRole = (targetRole: string) => {
  if (props.myRole === 'Owner') return targetRole !== 'Owner' 
  if (props.myRole === 'Admin') return targetRole !== 'Owner' && targetRole !== 'Admin' 
  return false
}

// 格式化日期工具
const formatDate = (dateString: string) => {
  if (!dateString) return '未知';
  const d = new Date(dateString);
  return `${d.getFullYear()}.${(d.getMonth()+1).toString().padStart(2, '0')}.${d.getDate().toString().padStart(2, '0')}`;
}

onMounted(() => {
  fetchMembers()
})
</script>

<style scoped>
/* ================= 基础外框 ================= */
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.7); backdrop-filter: blur(4px);
  display: flex; justify-content: center; align-items: center; z-index: 2000;
}
.modal-content.cyber-modal {
  background: #fff; border: 2px solid #111111;
  box-shadow: 16px 16px 0 #00A3FF;
  display: flex; flex-direction: column; overflow: hidden;
}
.modal-content.ultra-wide {
  width: 100%; max-width: 1000px; /* 🔥 加宽到 1000px */
  height: 80vh; /* 🔥 固定高度 */
}

/* ================= 头部控制条 ================= */
.modal-header-row {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 24px; border-bottom: 2px solid #111111; background: #fff;
  flex-shrink: 0;
}
.header-left { display: flex; align-items: center; gap: 16px; }
.header-left h3 { margin: 0; font-size: 18px; font-weight: 800; border-left: 4px solid #111111; padding-left: 12px; }
.member-count { font-size: 12px; font-weight: 700; background: #eee; padding: 4px 10px; border-radius: 4px; }

.header-right { display: flex; align-items: center; gap: 24px; }
.role-legend { font-size: 11px; display: flex; gap: 12px; color: #666; font-weight: 700; }
.legend-item { display: flex; align-items: center; gap: 4px; }
.dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; }
.dot.gold { background: #FFD700; }
.dot.red { background: #E91E63; }
.dot.blue { background: #00A3FF; }

.modal-btn.close-btn { 
  background: transparent; border: 2px solid #111; padding: 6px 16px; 
  font-size: 12px; font-weight: bold; cursor: pointer; transition: 0.2s;
}
.modal-btn.close-btn:hover { background: #111; color: #fff; }

/* ================= 分栏布局 ================= */
.split-workspace {
  display: flex; flex: 1; overflow: hidden; background: #F4F5F7;
}

/* ---------------- 左侧：花名册 ---------------- */
.member-sidebar {
  width: 300px; background: #fff; border-right: 2px solid #111;
  display: flex; flex-direction: column; flex-shrink: 0;
}
.sidebar-search-bar { padding: 16px; border-bottom: 1px solid #eee; background: #fafafa; }
.sidebar-title { font-size: 13px; font-weight: 800; color: #111; }

.sidebar-list { flex: 1; overflow-y: auto; }
.sidebar-list::-webkit-scrollbar { width: 6px; }
.sidebar-list::-webkit-scrollbar-thumb { background: #ccc; border-radius: 3px; }

.sidebar-item {
  display: flex; align-items: center; padding: 12px 16px;
  border-bottom: 1px solid #f0f0f0; cursor: pointer; transition: 0.2s;
  border-left: 4px solid transparent;
}
.sidebar-item:hover { background: #f9f9f9; }
.sidebar-item.is-active { background: #E6F5FF; border-left-color: #00A3FF; }

.s-avatar {
  width: 36px; height: 36px; background: #111; color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-weight: 800; margin-right: 12px; border-radius: 4px; overflow: hidden;
}
.s-avatar img { width: 100%; height: 100%; object-fit: cover; }

.s-info { flex: 1; overflow: hidden; }
.s-name { font-weight: 700; font-size: 13px; color: #111; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.s-you { font-size: 10px; color: #00A3FF; margin-left: 4px; }
.s-role { font-size: 10px; font-weight: 800; margin-top: 4px; }
.s-role.Owner { color: #D4AF37; }
.s-role.Admin { color: #E91E63; }
.s-role.Member { color: #00A3FF; }
.s-arrow { color: #ccc; font-size: 20px; font-weight: bold; }
.sidebar-item.is-active .s-arrow { color: #00A3FF; }

/* ---------------- 右侧：操作面板 ---------------- */
.member-detail-panel {
  flex: 1; display: flex; flex-direction: column; overflow: hidden;
}
.empty-state { height: 100%; display: flex; flex-direction: column; justify-content: center; align-items: center; color: #999; }
.empty-icon { font-size: 48px; margin-bottom: 16px; opacity: 0.3; }

.detail-container { display: flex; flex-direction: column; height: 100%; }

/* 右侧头部信息 */
.detail-header {
  display: flex; align-items: center; gap: 20px; padding: 32px;
  background: #fff; border-bottom: 1px solid #ddd; flex-shrink: 0;
}
.d-avatar {
  width: 64px; height: 64px; background: #111; border-radius: 8px; overflow: hidden;
  display: flex; justify-content: center; align-items: center; color: #fff; font-size: 24px; font-weight: bold;
}
.d-avatar img { width: 100%; height: 100%; object-fit: cover; }
.d-info h2 { margin: 0 0 8px 0; font-size: 24px; font-weight: 900; }
.d-meta { font-size: 12px; color: #666; font-weight: bold; }
.d-divider { margin: 0 8px; color: #ccc; }

/* 右侧滚动控制区 */
.detail-scroll-area {
  flex: 1; overflow-y: auto; padding: 32px; display: flex; flex-direction: column; gap: 24px;
}

/* 控制卡片 */
.control-card { background: #fff; border: 2px solid #111; position: relative; }
.card-title {
  position: absolute; top: -10px; left: 16px; background: #F4F5F7; 
  padding: 0 8px; font-size: 12px; font-weight: 900; color: #111;
}
.bracket { color: #00A3FF; }
.card-content { padding: 24px; }

/* 组织控制行 */
.org-control-row { display: flex; justify-content: space-between; align-items: flex-end; }
.control-group label { display: block; font-size: 12px; font-weight: 700; color: #555; margin-bottom: 8px; }
.danger-text { color: #E91E63 !important; }

.cyber-select { border: 2px solid #111; padding: 6px 12px; font-weight: 700; font-size: 12px; outline: none; cursor: pointer; background: #fff; }
.cyber-select.large { font-size: 14px; width: 220px; }
.cyber-select:hover { border-color: #00A3FF; }

.role-badge.large { display: inline-block; padding: 8px 16px; background: #eee; font-weight: bold; font-size: 14px; border: 1px solid #ccc; color: #555; }

.btn-danger {
  background: #FFEBE6; color: #DE350B; border: 2px solid #DE350B; 
  padding: 8px 16px; font-size: 12px; font-weight: 700; cursor: pointer; transition: 0.2s;
}
.btn-danger:hover { background: #DE350B; color: #fff; }

/* 项目展示网格 */
.loading-text, .empty-projects { text-align: center; padding: 30px; font-size: 12px; color: #999; font-weight: bold; border: 1px dashed #ccc;}
.loading-text { color: #00A3FF; animation: pulse 1.5s infinite; }

.project-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
.project-item {
  border: 1px solid #ddd; background: #fafafa; padding: 16px; 
  border-left: 4px solid #00A3FF; display: flex; flex-direction: column; gap: 16px;
  transition: 0.2s;
}
.project-item:hover { background: #fff; border-color: #00A3FF; box-shadow: 0 4px 8px rgba(0,0,0,0.05); }

.p-header { display: flex; align-items: center; gap: 8px; }
.p-icon { color: #00A3FF; font-size: 16px; }
.p-name { font-weight: 800; font-size: 14px; color: #111; }

.p-controls { display: flex; justify-content: space-between; align-items: center; }
.btn-icon-danger {
  background: transparent; border: 1px solid #E91E63; color: #E91E63; width: 26px; height: 26px;
  display: flex; justify-content: center; align-items: center; cursor: pointer; border-radius: 2px; transition: 0.2s;
}
.btn-icon-danger:hover { background: #E91E63; color: #fff; }

.global-loading { padding: 60px; text-align: center; color: #00A3FF; font-weight: bold; font-size: 14px; animation: pulse 1.5s infinite; }
@keyframes pulse { 0% { opacity: 0.5; } 50% { opacity: 1; } 100% { opacity: 0.5; } }
</style>