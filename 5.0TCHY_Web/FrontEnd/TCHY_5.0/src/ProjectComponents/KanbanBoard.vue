<template>
  <div class="board-wrapper">
    <div class="board-canvas">
      <div class="board-track">
        <div 
          v-for="column in boardData" 
          :key="column.id" 
          class="board-col"
          :class="{ 'col-done': column.isCompleted }"
          @dragover.prevent
          @drop="onDrop($event, column.name)"
        >
          <div class="col-header">
            <div class="col-title-area" @dblclick="editColumn(column)">
              <input 
                v-if="editingColId === column.id"
                v-model="tempColName"
                class="col-rename-input"
                @blur="saveColumnName(column)"
                @keyup.enter="saveColumnName(column)"
                v-focus
              />
              <div v-else class="col-label">
                <span class="name">{{ column.name }}</span>
                <span class="count">{{ column.tasks.length }}</span>
              </div>
            </div>
            <div class="col-ops">
              <button class="op-btn check" @click="toggleColumnDone(column)" :class="{ active: column.isCompleted }">✓</button>
              <button v-if="column.tasks.length === 0" class="op-btn del" @click="deleteColumn(column.id)">×</button>
            </div>
          </div>

          <div class="task-container">
            <div 
              v-for="task in column.tasks" 
              :key="task.id" 
              class="task-card"
              :class="{ 'card-dimmed': column.isCompleted }"
              draggable="true"
              @dragstart="onDragStart($event, task)"
              @click="openTaskDetail(task)"
            >
              <div class="card-badges">
                <span class="badge-priority" :class="task.priority">{{ task.priority }}</span>
                <span class="badge-tag" :style="{ color: getCategoryColor(task.categoryId) }">
                  {{ getCategoryName(task.categoryId) }}
                </span>
              </div>
              <div class="card-title">{{ task.title }}</div>
              <div class="card-meta">
                <div class="assignee-row">
                  <div class="mini-av" v-for="id in task.assigneeIds" :key="id">
                    <UniversalAvatar :user-id="id" :show-level="false" :allow-link="false" />
                  </div>
                  <span v-if="!task.assigneeIds?.length" class="no-assignee">待认领</span>
                </div>
                <div v-if="task.dueDate" class="date-badge" :class="{ 'overdue': isOverdue(task.dueDate) }">
                  <span class="clock-icon">🕒</span> {{ formatDateShort(task.dueDate) }}
                </div>
              </div>
            </div>
            <button class="ghost-btn" @click="quickAddTask(column.name)">+ 添加</button>
          </div>
        </div>

        <div class="add-col-placeholder">
          <button v-if="!isAddingColumn" class="btn-add-col" @click="isAddingColumn = true">+ 新增阶段</button>
          <div v-else class="new-col-input-box">
            <input v-model="newColumnName" placeholder="输入阶段名..." v-focus @keyup.enter="submitNewColumn" />
            <div class="row">
              <button class="btn-xs primary" @click="submitNewColumn">确认</button>
              <button class="btn-xs" @click="isAddingColumn = false">取消</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showCreateTask" class="modal-backdrop" @click.self="showCreateTask = false">
      <div class="modal-panel small">
        <div class="modal-header"><h3>发布新指令</h3><button class="close-icon" @click="showCreateTask = false">×</button></div>
        <div class="modal-body">
          <div class="form-group"><label>任务标题</label><input v-model="newTask.title" class="linear-input" autofocus /></div>
          <div class="form-row">
            <div class="form-group"><label>分类</label><select v-model="newTask.categoryId" class="linear-select"><option v-for="c in categories" :key="c.id" :value="c.id">{{c.name}}</option></select></div>
            <div class="form-group"><label>优先级</label><select v-model="newTask.priority" class="linear-select"><option value="P0">P0 - Urgent</option><option value="P1">P1 - High</option><option value="P2">P2 - Normal</option></select></div>
          </div>
          <div class="form-group"><label>截止日期</label><input type="date" v-model="newTask.dueDate" class="linear-input" /></div>
          <div class="form-group"><label>指派给</label><div class="user-grid"><div v-for="m in members" :key="m.userId" class="user-chip" :class="{active: newTask.assigneeIds.includes(m.userId)}" @click="toggleAssigneeInNewTask(m.userId)">{{m.username}}</div></div></div>
        </div>
        <div class="modal-footer"><button class="btn-ghost" @click="showCreateTask = false">取消</button><button class="btn-primary" @click="submitCreateTask" :disabled="isSaving">发布</button></div>
      </div>
    </div>

    <div v-if="isDetailOpen" class="modal-backdrop" @click.self="closeDetail">
      <div class="modal-panel large">
        <div class="modal-header no-border"><span class="task-id-badge">TASK-{{ editingTask.id }}</span><button class="close-icon" @click="closeDetail">×</button></div>
        <div class="detail-layout">
          <div class="detail-main">
            <input v-model="editingTask.title" class="detail-title-input" />
            <div class="detail-block"><label class="block-label">描述说明</label><textarea v-model="editingTask.description" class="detail-textarea" rows="10"></textarea></div>
            <div class="detail-block"><label class="block-label">执行干员</label><div class="user-grid"><div v-for="m in members" :key="m.userId" class="user-chip with-av" :class="{active: editingTask.assigneeIds?.includes(m.userId)}" @click="toggleAssignee(m.userId)"><div class="chip-av"><UniversalAvatar :user-id="m.userId" :show-level="false" /></div><span>{{m.username}}</span></div></div></div>
          </div>
          <div class="detail-side">
            <div class="prop-item"><label>状态</label><select v-model="editingTask.status" class="linear-select"><option v-for="c in columns" :key="c.id" :value="c.name">{{c.name}}</option></select></div>
            <div class="prop-item"><label>分类</label><select v-model="editingTask.categoryId" class="linear-select"><option v-for="c in categories" :key="c.id" :value="c.id">{{c.name}}</option></select></div>
            <div class="prop-item"><label>优先级</label><div class="priority-tabs"><button v-for="p in ['P0','P1','P2']" :key="p" :class="{active: editingTask.priority === p}" @click="editingTask.priority = p">{{p}}</button></div></div>
            <div class="prop-item"><label>截止日期</label><input type="date" v-model="editingTask.dueDateFormatted" class="linear-input" /></div>
            <div class="side-footer"><button class="btn-primary full" @click="saveTaskChanges" :disabled="isSaving">保存更改</button><button class="btn-danger full" @click="deleteTask">删除任务</button></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted} from 'vue'
import apiClient from '@/utils/api'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'

// 接收父组件传入的属性
const props = defineProps<{
  projectId: string | string[],
  members: any[]
}>()

const emit = defineEmits(['update-progress'])

// --- 内部状态 ---
const rawTasks = ref<any[]>([])
const columns = ref<any[]>([])
const categories = ref<any[]>([
  { id: 1, name: 'Code', color: '#5E6AD2' },
  { id: 2, name: 'Art', color: '#F2994A' },
  { id: 3, name: 'Design', color: '#EB5757' },
  { id: 4, name: 'Other', color: '#27AE60' }
])

const showCreateTask = ref(false)
const isDetailOpen = ref(false)
const isSaving = ref(false)
const isAddingColumn = ref(false)
const newColumnName = ref('')
const editingColId = ref<number | null>(null)
const tempColName = ref('')

const editingTask = reactive<any>({ assigneeIds: [] })
const newTask = reactive({ title: '', categoryId: 1, priority: 'P2', status: '', assigneeIds: [] as number[], dueDate: '' })
const draggedTask = ref<any>(null)

// --- 计算属性 ---
const boardData = computed(() => {
  const existingStatusList = columns.value.map(c => c.name);
  return columns.value.map((col, index) => {
    const matchingTasks = rawTasks.value.filter(t => t.status === col.name);
    if (index === 0) {
      const orphanTasks = rawTasks.value.filter(t => !existingStatusList.includes(t.status));
      return { ...col, tasks: [...matchingTasks, ...orphanTasks] };
    }
    return { ...col, tasks: matchingTasks };
  })
})

const tasksCount = computed(() => {
  const total = rawTasks.value.length
  const doneCols = columns.value.filter(c => c.isCompleted).map(c => c.name)
  const done = rawTasks.value.filter(t => doneCols.includes(t.status)).length
  return { total, done }
})

const projectProgress = computed(() => {
  if (tasksCount.value.total === 0) return 0
  return Math.round((tasksCount.value.done / tasksCount.value.total) * 100)
})

// 监听进度变化，通知父组件 Header
import { watch } from 'vue'
watch(projectProgress, (val) => {
  emit('update-progress', { percentage: val, count: tasksCount.value })
}, { immediate: true })

// --- API ---
const fetchColumns = async () => {
  try {
    const res = await apiClient.get(`/columns/project/${props.projectId}`)
    columns.value = res.data.map((c: any) => ({ id: c.Id, name: c.Name, isCompleted: c.IsCompleted }))
  } catch (e) { console.error(e) }
}
const fetchTasks = async () => {
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/tasks`)
    rawTasks.value = res.data.map((t: any) => ({
      id: t.Id, title: t.Title, description: t.Description, status: t.Status,
      priority: t.Priority || 'P2', categoryId: t.CategoryId || 1, 
      assigneeIds: t.AssigneeIds || [], dueDate: t.DueDate
    }))
  } catch (e) { console.error(e) }
}

// --- 动作 ---
const openCreateModal = () => { newTask.status = columns.value[0]?.name || 'Todo'; showCreateTask.value = true }
const quickAddTask = (s: string) => { newTask.status = s; showCreateTask.value = true }

const submitCreateTask = async () => {
  if (!newTask.title.trim()) return
  isSaving.value = true
  try {
    await apiClient.post('/tasks', {
      ProjectId: props.projectId, Title: newTask.title,
      CategoryId: newTask.categoryId, Priority: newTask.priority,
      Status: newTask.status, AssigneeIds: newTask.assigneeIds,
      DueDate: newTask.dueDate ? new Date(newTask.dueDate) : null
    })
    showCreateTask.value = false; newTask.title = ''; newTask.assigneeIds = []; newTask.dueDate = ''
    await fetchTasks()
  } catch (e) { alert('发布失败') } finally { isSaving.value = false }
}

const saveTaskChanges = async () => {
  isSaving.value = true
  try {
    await apiClient.put(`/tasks/${editingTask.id}`, {
      Title: editingTask.title, Description: editingTask.description,
      Status: editingTask.status, Priority: editingTask.priority,
      AssigneeIds: editingTask.assigneeIds, CategoryId: editingTask.categoryId,
      DueDate: editingTask.dueDateFormatted ? new Date(editingTask.dueDateFormatted) : null
    })
    await fetchTasks(); isDetailOpen.value = false
  } catch(e) { alert('同步失败') } finally { isSaving.value = false }
}

// 通用逻辑 (保持不变，只是引用改成 props.members)
const toggleAssignee = (uid: number) => { 
  if(!editingTask.assigneeIds) editingTask.assigneeIds = []
  const idx = editingTask.assigneeIds.indexOf(uid); idx > -1 ? editingTask.assigneeIds.splice(idx,1) : editingTask.assigneeIds.push(uid) 
}
const toggleAssigneeInNewTask = (uid: number) => { 
  const idx = newTask.assigneeIds.indexOf(uid); idx > -1 ? newTask.assigneeIds.splice(idx,1) : newTask.assigneeIds.push(uid) 
}
const onDrop = async (e: any, status: string) => {
  if (!draggedTask.value || draggedTask.value.status === status) return
  const old = draggedTask.value.status; draggedTask.value.status = status
  try { await apiClient.put(`/tasks/${draggedTask.value.id}`, { Status: status }) } catch { draggedTask.value.status = old }
}
const onDragStart = (e: any, t: any) => { draggedTask.value = t }
const editColumn = (col: any) => { editingColId.value = col.id; tempColName.value = col.name }
const saveColumnName = async (col: any) => { await apiClient.put(`/columns/${col.id}`, { Name: tempColName.value, IsCompleted: col.isCompleted }); await fetchColumns(); editingColId.value = null }
const toggleColumnDone = async (col: any) => { col.isCompleted = !col.isCompleted; await apiClient.put(`/columns/${col.id}`, { IsCompleted: col.isCompleted, Name: col.name }) }
const deleteColumn = async (id: number) => { if(confirm('删除列？')) { await apiClient.delete(`/columns/${id}`); fetchColumns() } }
const submitNewColumn = async () => { await apiClient.post('/columns', { ProjectId: props.projectId, Name: newColumnName.value }); newColumnName.value = ''; isAddingColumn.value = false; await fetchColumns() }
const openTaskDetail = (t: any) => { 
  Object.assign(editingTask, JSON.parse(JSON.stringify(t))); 
  editingTask.dueDateFormatted = t.dueDate ? t.dueDate.split('T')[0] : ''
  isDetailOpen.value = true 
}
const closeDetail = () => isDetailOpen.value = false
const deleteTask = async () => { if (confirm('删除任务？')) { await apiClient.delete(`/tasks/${editingTask.id}`); await fetchTasks(); isDetailOpen.value = false } }

// 格式化工具
const getCategoryColor = (id: number) => categories.value.find(c => c.id === id)?.color || '#999'
const getCategoryName = (id: number) => categories.value.find(c => c.id === id)?.name || 'General'
const formatDateShort = (d: string) => { if(!d)return''; const da=new Date(d); return `${da.getMonth()+1}/${da.getDate()}` }
const isOverdue = (d: string) => { if(!d)return false; return new Date(d) < new Date() && new Date(d).toDateString() !== new Date().toDateString() }
const vFocus = { mounted: (el: any) => el.focus() }

// 🔥 暴露给父组件调用
defineExpose({ openCreateModal })

onMounted(() => { fetchColumns(); fetchTasks() })
</script>

<style scoped>
/* 复制之前 ProjectDetail.vue 中关于 .board-canvas 及以下的所有样式 */
/* 注意：这里需要把 .board-canvas 的 flex: 1 属性保留，但它现在是组件的根元素之一 */
.board-wrapper { flex: 1; overflow-x: hidden; display: flex; height: 100%; }
.board-canvas { flex: 1; overflow-x: auto; padding: 24px; }
.board-track { display: flex; height: 100%; align-items: flex-start; gap: 12px; }
.board-col { width: 272px; min-width: 272px; max-height: 100%; background: #EBECF0; border-radius: 3px; display: flex; flex-direction: column; }
.board-col.col-done { background: #E3FCEF; }
.col-header { padding: 10px 8px; display: flex; justify-content: space-between; align-items: center; font-size: 14px; font-weight: 600; }
.col-label .count { font-weight: 400; color: #5E6C84; margin-left: 6px; }
.col-rename-input { width: 100%; padding: 4px; border: 2px solid #0052CC; border-radius: 3px; }
.col-ops .op-btn { border: none; background: transparent; color: #6B778C; cursor: pointer; padding: 4px; }
.col-ops .check.active { color: #36B37E; }
.task-container { padding: 0 8px 8px 8px; overflow-y: auto; flex: 1; display: flex; flex-direction: column; gap: 8px; }
.task-card { background: #fff; border-radius: 3px; box-shadow: 0 1px 2px rgba(9,30,66,0.25); padding: 8px; cursor: pointer; transition: 0.1s; }
.task-card:hover { background: #FAFBFC; }
.card-dimmed { opacity: 0.6; }
.card-badges { display: flex; gap: 4px; margin-bottom: 6px; }
.badge-priority { font-size: 10px; font-weight: 700; padding: 2px 4px; border-radius: 2px; color: #fff; }
.badge-priority.P0 { background: #BF2600; } .badge-priority.P1 { background: #FF991F; } .badge-priority.P2 { background: #0052CC; }
.badge-tag { font-size: 10px; font-weight: 600; }
.card-title { font-size: 14px; color: #172B4D; margin-bottom: 8px; line-height: 1.4; }
.card-meta { display: flex; justify-content: space-between; align-items: center; }
.assignee-row { display: flex; }
.mini-av { width: 20px; height: 20px; border-radius: 50%; overflow: hidden; margin-right: 4px; }
.no-assignee { font-size: 11px; color: #97A0AF; }
.date-badge { font-size: 11px; color: #6B778C; display: flex; align-items: center; gap: 4px; padding: 2px 4px; border-radius: 3px; }
.date-badge.overdue { color: #DE350B; background: #FFEBE6; font-weight: 600; }
.ghost-btn { width: 100%; text-align: left; padding: 8px; color: #5E6C84; background: transparent; border: none; cursor: pointer; border-radius: 3px; }
.ghost-btn:hover { background: rgba(9,30,66,0.08); }
.add-col-placeholder { min-width: 272px; }
.btn-add-col { width: 100%; height: 48px; border: 2px dashed #DFE1E6; background: transparent; color: #6B778C; cursor: pointer; }
.new-col-input-box { background: #fff; padding: 8px; box-shadow: 0 1px 2px rgba(9,30,66,0.25); }
.new-col-input-box input { width: 100%; margin-bottom: 8px; padding: 4px; border: 2px solid #0052CC; }
.btn-xs { padding: 4px 8px; font-size: 12px; border: none; border-radius: 2px; margin-right: 4px; cursor: pointer; }
.btn-xs.primary { background: #0052CC; color: #fff; }

/* 模态框通用样式 */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal-panel { background: #fff; border-radius: 3px; display: flex; flex-direction: column; box-shadow: 0 8px 30px rgba(0,0,0,0.3); }
.modal-panel.small { width: 400px; }
.modal-panel.large { width: 900px; height: 80vh; }
.modal-header { padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; }
.modal-header.no-border { border-bottom: none; }
.modal-body { padding: 24px; flex: 1; overflow-y: auto; display: flex; flex-direction: column; }
.modal-footer { padding: 16px 24px; border-top: 1px solid #DFE1E6; display: flex; justify-content: flex-end; gap: 8px; }
.form-group { margin-bottom: 16px; }
.form-row { display: flex; gap: 16px; }
.form-row .form-group { flex: 1; }
label { display: block; font-size: 12px; font-weight: 600; color: #6B778C; margin-bottom: 4px; }
.linear-input, .linear-select, .detail-textarea { width: 100%; padding: 8px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; transition: 0.2s; background: #FAFBFC; }
.linear-input:focus, .linear-select:focus, .detail-textarea:focus { background: #fff; border-color: #0052CC; outline: none; }
.detail-textarea { font-family: monospace; line-height: 1.5; resize: none; }
.detail-layout { display: flex; flex: 1; overflow: hidden; }
.detail-main { flex: 3; padding: 24px 32px; overflow-y: auto; }
.detail-side { flex: 1; padding: 24px; background: #FAFBFC; border-left: 1px solid #EBECF0; display: flex; flex-direction: column; gap: 20px; }
.detail-title-input { font-size: 24px; font-weight: 600; border: none; width: 100%; outline: none; margin-bottom: 24px; }
.user-grid { display: flex; flex-wrap: wrap; gap: 8px; }
.user-chip { padding: 4px 10px; background: #F4F5F7; border-radius: 100px; font-size: 12px; cursor: pointer; border: 1px solid transparent; }
.user-chip.active { background: #DEEBFF; color: #0052CC; border-color: #0052CC; }
.user-chip.with-av { display: flex; gap: 6px; align-items: center; padding-left: 4px; }
.chip-av { width: 16px; height: 16px; }
.priority-tabs { display: flex; background: #EBECF0; padding: 2px; border-radius: 3px; }
.priority-tabs button { flex: 1; border: none; background: transparent; padding: 6px; cursor: pointer; font-size: 12px; font-weight: 600; color: #6B778C; border-radius: 2px; }
.priority-tabs button.active { background: #fff; color: #0052CC; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
.btn-primary.full { width: 100%; margin-top: 20px; }
.btn-danger.full { width: 100%; border: 1px solid #FFEBE6; background: #FFEBE6; color: #DE350B; padding: 8px; border-radius: 3px; cursor: pointer; margin-top: auto; }
</style>