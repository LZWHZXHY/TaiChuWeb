<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content cyber-modal member-manager-modal">
      <div class="modal-header">
        <h3>// 扇区干员调度中心</h3>
        <button class="close-btn" @click="emit('close')">✖</button>
      </div>

      <div class="member-list-container">
        <div v-if="loading" class="loading-state">正在扫描干员名册...</div>
        
        <table v-else class="cyber-table">
          <thead>
            <tr>
              <th>干员档案</th>
              <th>入职时间</th>
              <th>当前授予权限 (角色)</th>
              <th class="text-right">战术操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="member in members" :key="member.userId">
              <td>
                <div class="user-info">
                  <div class="avatar-wrap">
                    <UniversalAvatar :user-id="member.userId" :show-level="false" />
                  </div>
                  <span class="username">{{ member.username }}</span>
                  <span v-if="member.userId === currentUserId" class="you-badge">YOU</span>
                </div>
              </td>
              <td class="text-muted">{{ formatDate(member.joinedAt) }}</td>
              <td>
                <template v-if="canManage">
                  <select 
                    class="cyber-select-small" 
                    v-model="member.roleId"
                    @change="updateMemberRole(member)"
                    :disabled="member.userId === currentUserId || isUpdating === member.userId"
                  >
                    <option v-for="role in availableRoles" :key="role.id" :value="role.id">
                      {{ role.name }}
                    </option>
                  </select>
                  <span v-if="isUpdating === member.userId" class="updating-text">同步中...</span>
                </template>
                <template v-else>
                  <span class="readonly-role">{{ getRoleName(member.roleId) }}</span>
                </template>
              </td>
              <td class="text-right">
                <button 
                  v-if="canManage"
                  class="btn-danger-ghost" 
                  :disabled="member.userId === currentUserId"
                  @click="removeMember(member)"
                >
                  除名
                </button>
                <span v-else class="text-muted">-</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import { useAuthStore } from '@/utils/auth'

const props = defineProps<{ projectId: string | number }>()
const emit = defineEmits(['close', 'members-updated'])

const authStore = useAuthStore()
const currentUserId = computed(() => authStore.user?.id || 0)

const members = ref<any[]>([])
const availableRoles = ref<any[]>([])
const loading = ref(true)
const isUpdating = ref<number | null>(null)

// 🌟 新增：控制当前登录者是否有权限操作面板
const canManage = ref(false) 

const fetchData = async () => {
  loading.value = true
  try {
    // 1. 拉取所有可用角色
    const rolesRes = await apiClient.get(`/projects/${props.projectId}/roles`)
    availableRoles.value = rolesRes.data.map((r: any) => ({
      id: r.Id || r.id,
      name: r.Name || r.name
    }))

    // 2. 拉取项目当前成员
    const membersRes = await apiClient.get(`/projects/${props.projectId}/members`)
    members.value = membersRes.data.map((m: any) => ({
      userId: m.UserId || m.userId,
      username: m.Username || m.username,
      roleId: m.RoleId || m.roleId,
      joinedAt: m.JoinedAt || m.joinedAt
    }))

    // 🌟 3. 动态判断当前用户的权限！
    const myRecord = members.value.find(m => m.userId === currentUserId.value)
    
    if (myRecord && myRecord.roleId) {
      // 去后端查一下我这个角色到底有没有 project:manage_roles 的权限点
      try {
        const permRes = await apiClient.get(`/projects/${props.projectId}/roles/${myRecord.roleId}/permissions`)
        canManage.value = permRes.data.includes('project:manage_roles')
      } catch (e) {
        canManage.value = false // 查不到就默认没权限，最安全
      }
    } else {
      // 隐秘逻辑：如果当前用户不在项目成员列表里，说明他是以“组织Owner/Admin”的上帝视角进来的
      // 根据我们后端的逻辑，上帝视角天然拥有所有权限，所以这里放行
      canManage.value = true 
    }

  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const getRoleName = (roleId: number) => {
  const role = availableRoles.value.find(r => r.id === roleId)
  return role ? role.name : '未知角色'
}

const updateMemberRole = async (member: any) => {
  isUpdating.value = member.userId
  try {
    await apiClient.put(`/projects/${props.projectId}/members/${member.userId}/role`, {
      roleId: member.roleId
    })
    emit('members-updated') 
  } catch (e: any) {
    alert(e.response?.data?.message || '修改权限失败')
    await fetchData() 
  } finally {
    isUpdating.value = null
  }
}

const removeMember = async (member: any) => {
  if (confirm(`警告：确定要将干员 [${member.username}] 从本扇区彻底除名吗？`)) {
    try {
      await apiClient.delete(`/projects/${props.projectId}/members/${member.userId}`)
      members.value = members.value.filter(m => m.userId !== member.userId)
      emit('members-updated')
    } catch (e: any) {
      alert(e.response?.data?.message || '踢人失败')
    }
  }
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleDateString()
}

onMounted(() => fetchData())
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(9, 30, 66, 0.6); backdrop-filter: blur(3px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.member-manager-modal { width: 800px; max-width: 95vw; background: #fff; border-radius: 8px; box-shadow: 0 12px 24px rgba(0,0,0,0.2); overflow: hidden;}
.modal-header { padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; background: #FAFBFC; }
.modal-header h3 { margin: 0; font-size: 16px; color: #172B4D; font-weight: 700; }
.close-btn { background: transparent; border: none; font-size: 16px; cursor: pointer; color: #6B778C; }

.member-list-container { padding: 24px; max-height: 60vh; overflow-y: auto; }
.loading-state { text-align: center; padding: 40px; color: #0052CC; font-weight: 600; }

.cyber-table { width: 100%; border-collapse: collapse; }
.cyber-table th { text-align: left; padding: 12px; font-size: 12px; color: #5E6C84; border-bottom: 2px solid #DFE1E6; text-transform: uppercase; }
.cyber-table td { padding: 16px 12px; border-bottom: 1px solid #EBECF0; vertical-align: middle; }
.cyber-table tr:hover td { background: #FAFBFC; }

.text-right { text-align: right; }
.text-muted { color: #6B778C; font-size: 13px; }

.user-info { display: flex; align-items: center; gap: 12px; }
.avatar-wrap { width: 32px; height: 32px; border-radius: 50%; overflow: hidden; }
.username { font-weight: 600; color: #172B4D; font-size: 14px; }
.you-badge { background: #E3FCEF; color: #006644; font-size: 10px; padding: 2px 6px; border-radius: 12px; font-weight: bold; }

/* 🌟 新增：只读模式下的文本样式 */
.readonly-role { background: #EBECF0; padding: 4px 10px; border-radius: 4px; font-size: 13px; font-weight: 600; color: #5E6C84; }

.cyber-select-small { padding: 6px 10px; border: 1px solid #DFE1E6; border-radius: 4px; background: #FAFBFC; outline: none; font-weight: 600; color: #172B4D; cursor: pointer; transition: 0.2s;}
.cyber-select-small:hover:not(:disabled) { background: #EBECF0; }
.cyber-select-small:disabled { opacity: 0.6; cursor: not-allowed; }
.updating-text { font-size: 12px; color: #0052CC; margin-left: 8px; font-weight: bold; animation: pulse 1s infinite; }

.btn-danger-ghost { background: transparent; border: 1px solid #DE350B; color: #DE350B; border-radius: 4px; padding: 6px 12px; font-size: 12px; font-weight: 600; cursor: pointer; transition: 0.2s;}
.btn-danger-ghost:hover:not(:disabled) { background: #DE350B; color: #fff; }
.btn-danger-ghost:disabled { border-color: #DFE1E6; color: #DFE1E6; cursor: not-allowed; }

@keyframes pulse { 0% { opacity: 0.5; } 50% { opacity: 1; } 100% { opacity: 0.5; } }
</style>