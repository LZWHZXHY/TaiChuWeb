<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content cyber-modal role-matrix-modal">
      <div class="modal-header">
        <h3>// 动态权限矩阵枢纽 (RBAC Core)</h3>
        <button class="close-btn" @click="emit('close')">✖</button>
      </div>

      <div class="matrix-layout">
        <aside class="role-sidebar">
          <div class="sidebar-top">
            <h4>自定义角色</h4>
            <button class="btn-ghost-small" @click="startCreateRole">+ 新增</button>
          </div>
          <ul class="role-list">
            <li 
              v-for="role in roles" 
              :key="role.id"
              :class="{ active: selectedRole?.id === role.id }"
              @click="selectRole(role)"
            >
              <div class="role-name-row">
                <span class="role-name">{{ role.name }}</span>
                <span v-if="role.isSystem" class="sys-badge">SYS</span>
              </div>
            </li>
          </ul>
        </aside>

        <main class="permission-panel" v-if="selectedRole">
          <div class="panel-header">
            <h4>配置 [ {{ selectedRole.name }} ] 的权限能力</h4>
            <p class="desc">{{ selectedRole.description || '暂无描述' }}</p>
          </div>

          <div class="permission-groups">
            <div class="perm-group" v-for="group in permissionDictionary" :key="group.title">
              <div class="group-title">{{ group.title }}</div>
              <div class="perm-grid">
                <label 
                  v-for="perm in group.items" 
                  :key="perm.key" 
                  class="cyber-checkbox"
                >
                  <input 
                    type="checkbox" 
                    :value="perm.key"
                    v-model="currentRolePermissions"
                    :disabled="selectedRole.name === 'Admin'" 
                  />
                  <span class="checkmark"></span>
                  <span class="perm-label">{{ perm.label }}</span>
                </label>
              </div>
            </div>
          </div>

          <div class="panel-footer">
            <span class="warning-text" v-if="selectedRole.name === 'Admin'">
              ⚠️ Admin 是最高系统角色，默认拥有所有权限，不可修改。
            </span>
            <div class="actions">
              <button class="btn-danger" v-if="!selectedRole.isSystem" @click="deleteRole">删除该角色</button>
              <button class="btn-primary" :disabled="selectedRole.name === 'Admin' || isSaving" @click="savePermissions">
                {{ isSaving ? '同步中...' : '保存权限配置' }}
              </button>
            </div>
          </div>
        </main>

        <main class="permission-panel empty-state" v-else>
          <p>请在左侧选择一个角色进行配置</p>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps<{ projectId: string | number }>()
const emit = defineEmits(['close', 'roles-updated'])

const roles = ref<any[]>([])
const selectedRole = ref<any>(null)
const currentRolePermissions = ref<string[]>([])
const isSaving = ref(false)

// 🌟 前端的权限字典 (和我们 C# 里约定的一模一样)
const permissionDictionary = [
  {
    title: '🛠️ 项目级设置',
    items: [
      { key: 'project:settings', label: '修改项目基础信息' },
      { key: 'project:manage_roles', label: '管理人员与权限' }
    ]
  },
  {
    title: '⊞ 任务看板',
    items: [
      { key: 'board:manage_columns', label: '管理看板阶段 (增/删/改列)' },
      { key: 'task:create', label: '下发新任务' },
      { key: 'task:edit_own', label: '编辑自己负责的任务' },
      { key: 'task:edit_any', label: '越权编辑任意任务' },
      { key: 'task:delete', label: '销毁任务' }
    ]
  },
  {
    title: '📄 资源文档',
    items: [
      { key: 'doc:create', label: '起草新文档' },
      { key: 'doc:edit_own', label: '修改自己起草的文档' },
      { key: 'doc:edit_any', label: '越权修改任意文档' },
      { key: 'doc:delete', label: '删除文档' }
    ]
  },
  {
    title: '📊 工作汇报',
    items: [
      { key: 'report:manage_cycles', label: '管理汇报周期 (创建/锁定)' },
      { key: 'report:submit', label: '提交工作汇报' }
    ]
  }
]

// 🌟 1. 获取角色列表并做数据映射
const fetchRoles = async () => {
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/roles`)
    // 🔥 在这里统一处理 C# 的大写字段，这样 template 里就不用改了！
    roles.value = res.data.map((r: any) => ({
      id: r.Id || r.id,
      name: r.Name || r.name,
      description: r.Description || r.description,
      isSystem: r.IsSystem !== undefined ? r.IsSystem : r.isSystem
    }))
    
    if (roles.value.length > 0) {
      selectRole(roles.value[0])
    }
  } catch (e) { console.error(e) }
}

// 🌟 2. 选中角色并获取它的权限点
const selectRole = async (role: any) => {
  selectedRole.value = role
  if (!role.id) return

  try {
    const res = await apiClient.get(`/projects/${props.projectId}/roles/${role.id}/permissions`)
    currentRolePermissions.value = res.data // 直接赋值后端的字符串数组
  } catch (e) {
    currentRolePermissions.value = []
  }
}

// 🌟 3. 保存更新的权限
const savePermissions = async () => {
  if (!selectedRole.value) return
  isSaving.value = true
  try {
    await apiClient.put(`/projects/${props.projectId}/roles/${selectedRole.value.id}/permissions`, {
      permissions: currentRolePermissions.value
    })
    alert('✅ 权限同步完成')
  } catch (e: any) {
    console.error(e)
    alert(e.response?.data?.message || '同步失败，可能权限不足')
  } finally {
    isSaving.value = false
  }
}

// 🌟 4. 新建自定义角色
const startCreateRole = async () => {
  const name = prompt('输入新角色名称 (如: 外包开发, 文档审核):')
  if (!name) return
  
  try {
    await apiClient.post(`/projects/${props.projectId}/roles`, {
      name: name,
      description: '自定义角色'
    })
    emit('roles-updated') // 通知父组件更新外面的下拉框
    fetchRoles() // 刷新当前弹窗左侧的列表
  } catch (e: any) {
    alert(e.response?.data?.message || '新建角色失败')
  }
}

// 🌟 5. 删除角色
const deleteRole = async () => {
  if (confirm(`确定要销毁 [${selectedRole.value.name}] 角色吗？\n注意：如果有干员正在使用该角色，将无法删除。`)) {
    try {
      await apiClient.delete(`/projects/${props.projectId}/roles/${selectedRole.value.id}`)
      emit('roles-updated')
      fetchRoles()
    } catch (e: any) {
      alert(e.response?.data?.message || '删除角色失败')
    }
  }
}

onMounted(() => fetchRoles())
</script>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(9, 30, 66, 0.6); backdrop-filter: blur(3px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.role-matrix-modal { width: 900px; max-width: 95vw; height: 700px; max-height: 90vh; display: flex; flex-direction: column; padding: 0; overflow: hidden; background: #fff; border-radius: 8px; box-shadow: 0 12px 24px rgba(0,0,0,0.2); }
.modal-header { padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; background: #FAFBFC; }
.modal-header h3 { margin: 0; font-size: 16px; color: #172B4D; font-weight: 700; }
.close-btn { background: transparent; border: none; font-size: 16px; cursor: pointer; color: #6B778C; }

.matrix-layout { display: flex; flex: 1; overflow: hidden; }
.role-sidebar { width: 240px; border-right: 1px solid #DFE1E6; background: #FAFBFC; display: flex; flex-direction: column; }
.sidebar-top { padding: 16px; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #DFE1E6; }
.sidebar-top h4 { margin: 0; font-size: 13px; color: #5E6C84; }
.btn-ghost-small { background: transparent; border: none; color: #0052CC; cursor: pointer; font-size: 12px; font-weight: 600; }
.role-list { list-style: none; padding: 0; margin: 0; overflow-y: auto; }
.role-list li { padding: 12px 16px; cursor: pointer; border-bottom: 1px solid #EBECF0; transition: 0.2s; }
.role-list li:hover { background: #EBECF0; }
.role-list li.active { background: #DEEBFF; border-left: 3px solid #0052CC; }
.role-name-row { display: flex; justify-content: space-between; align-items: center; }
.role-name { font-size: 14px; font-weight: 600; color: #172B4D; }
.sys-badge { font-size: 10px; background: #FFAB00; color: #fff; padding: 2px 6px; border-radius: 10px; font-weight: bold; }

.permission-panel { flex: 1; display: flex; flex-direction: column; background: #fff; }
.panel-header { padding: 24px; border-bottom: 1px solid #EBECF0; }
.panel-header h4 { margin: 0 0 8px 0; font-size: 18px; color: #172B4D; }
.panel-header .desc { margin: 0; font-size: 13px; color: #6B778C; }

.permission-groups { flex: 1; overflow-y: auto; padding: 24px; }
.perm-group { margin-bottom: 32px; }
.group-title { font-size: 14px; font-weight: 700; color: #172B4D; margin-bottom: 16px; border-bottom: 2px solid #EBECF0; padding-bottom: 8px; }
.perm-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }

/* Cyber Checkbox */
.cyber-checkbox { display: flex; align-items: center; position: relative; cursor: pointer; font-size: 13px; color: #42526E; user-select: none; }
.cyber-checkbox input { position: absolute; opacity: 0; cursor: pointer; height: 0; width: 0; }
.checkmark { height: 18px; width: 18px; background-color: #FAFBFC; border: 2px solid #DFE1E6; border-radius: 4px; margin-right: 10px; transition: 0.2s; }
.cyber-checkbox:hover input ~ .checkmark { background-color: #EBECF0; }
.cyber-checkbox input:checked ~ .checkmark { background-color: #0052CC; border-color: #0052CC; }
.cyber-checkbox input:disabled ~ .checkmark { background-color: #EBECF0; cursor: not-allowed; opacity: 0.5; }
.checkmark:after { content: ""; position: absolute; display: none; }
.cyber-checkbox input:checked ~ .checkmark:after { display: block; }
.cyber-checkbox .checkmark:after { left: 6px; top: 2px; width: 4px; height: 9px; border: solid white; border-width: 0 2px 2px 0; transform: rotate(45deg); }

.panel-footer { padding: 16px 24px; border-top: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; background: #FAFBFC; }
.warning-text { font-size: 12px; color: #FF5630; font-weight: 600; }
.actions { display: flex; gap: 12px; margin-left: auto; }
.btn-primary { background: #0052CC; color: #fff; border: none; padding: 8px 16px; border-radius: 4px; font-weight: 600; cursor: pointer; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-danger { background: transparent; color: #DE350B; border: 1px solid #DE350B; padding: 8px 16px; border-radius: 4px; font-weight: 600; cursor: pointer; }
.empty-state { align-items: center; justify-content: center; color: #6B778C; }
</style>