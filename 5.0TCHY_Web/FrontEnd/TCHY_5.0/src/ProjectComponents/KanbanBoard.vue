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
              <button class="op-btn check" @click="toggleColumnDone(column)" :class="{ active: column.isCompleted }" title="标记此列完成">✓</button>
              <button v-if="column.tasks.length === 0" class="op-btn del" @click="deleteColumn(column.id)" title="删除此列">×</button>
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
                <span class="badge-tag" :style="{ backgroundColor: getCategoryColor(task.categoryId) + '15', color: getCategoryColor(task.categoryId) }">
                  {{ getCategoryName(task.categoryId) }}
                </span>
              </div>
              <div class="card-title">{{ task.title }}</div>
              
              <div class="card-meta">
                <div class="assignee-row">
                  <div class="mini-av-wrapper" v-for="(id, index) in task.assigneeIds" :key="id" :style="{ zIndex: 10 - Number(index) }">
                    <UniversalAvatar :user-id="id" :show-level="false" :allow-link="false" class="mini-av" />
                  </div>
                  <span v-if="!task.assigneeIds?.length" class="no-assignee">待认领</span>
                </div>
                <div v-if="task.dueDate" class="date-badge" :class="{ 'overdue': isOverdue(task.dueDate) }">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 4px;"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
                  {{ formatDateShort(task.dueDate) }}
                </div>
              </div>
            </div>
            <button class="ghost-btn" @click="quickAddTask(column.name)">
              <span class="plus-icon">+</span> 新增任务
            </button>
          </div>
        </div>

        <div class="add-col-placeholder">
          <button v-if="!isAddingColumn" class="btn-add-col" @click="isAddingColumn = true">
            <span class="plus-icon">+</span> 新建阶段
          </button>
          <div v-else class="new-col-input-box">
            <input v-model="newColumnName" class="modern-input" placeholder="输入阶段名称..." v-focus @keyup.enter="submitNewColumn" />
            <div class="row-actions">
              <button class="btn-sm primary" @click="submitNewColumn">保存</button>
              <button class="btn-sm" @click="isAddingColumn = false">取消</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <transition name="modal-fade">
      <div v-if="showCreateTask" class="modal-backdrop" @click.self="showCreateTask = false">
        <div class="modal-panel small">
          <div class="modal-header">
            <h3>发布新指令</h3>
            <button class="close-icon" @click="showCreateTask = false">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>任务标题</label>
              <input v-model="newTask.title" class="modern-input" placeholder="输入核心任务目标..." autofocus />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>分类</label>
                <div class="select-wrapper">
                  <select v-model="newTask.categoryId" class="modern-input">
                    <option v-for="c in categories" :key="c.id" :value="c.id">{{c.name}}</option>
                  </select>
                </div>
              </div>
              <div class="form-group">
                <label>优先级</label>
                <div class="select-wrapper">
                  <select v-model="newTask.priority" class="modern-input">
                    <option value="P0">P0 - 紧急 (Urgent)</option>
                    <option value="P1">P1 - 高 (High)</option>
                    <option value="P2">P2 - 普通 (Normal)</option>
                  </select>
                </div>
              </div>
            </div>
            <div class="form-group">
              <label>截止日期</label>
              <input type="date" v-model="newTask.dueDate" class="modern-input" />
            </div>
            <div class="form-group">
              <label>指派给</label>
              <div class="user-grid">
                <div 
                  v-for="m in members" 
                  :key="m.userId" 
                  class="user-chip" 
                  :class="{active: newTask.assigneeIds.includes(m.userId)}" 
                  @click="toggleAssigneeInNewTask(m.userId)"
                >
                  {{m.username}}
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-ghost" @click="showCreateTask = false">取消</button>
            <button class="btn-primary" @click="submitCreateTask" :disabled="isSaving || !newTask.title">
              {{ isSaving ? '发布中...' : '发布任务' }}
            </button>
          </div>
        </div>
      </div>
    </transition>

    <transition name="modal-fade">
      <div v-if="isDetailOpen" class="modal-backdrop" @click.self="closeDetail">
        <div class="modal-panel large">
          <div class="modal-header no-border">
            <span class="task-id-badge">TASK-{{ editingTask.id }}</span>
            <button class="close-icon" @click="closeDetail">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
            </button>
          </div>
          <div class="detail-layout">
            <div class="detail-main">
              <input v-model="editingTask.title" class="detail-title-input" placeholder="输入任务标题..." />
              <div class="detail-block">
                <label class="block-label">描述说明</label>
                <textarea v-model="editingTask.description" class="modern-textarea" rows="10" placeholder="添加更详细的任务描述、验收标准..."></textarea>
              </div>
              <div class="detail-block">
                <label class="block-label">执行干员</label>
                <div class="user-grid">
                  <div 
                    v-for="m in members" 
                    :key="m.userId" 
                    class="user-chip with-av" 
                    :class="{active: editingTask.assigneeIds?.includes(m.userId)}" 
                    @click="toggleAssignee(m.userId)"
                  >
                    <div class="chip-av-wrapper">
                      <UniversalAvatar :user-id="m.userId" :show-level="false" class="chip-av" />
                    </div>
                    <span>{{m.username}}</span>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="detail-side">
              <div class="prop-item">
                <label>所属阶段 (Status)</label>
                <div class="select-wrapper">
                  <select v-model="editingTask.status" class="modern-input">
                    <option v-for="c in columns" :key="c.id" :value="c.name">{{c.name}}</option>
                  </select>
                </div>
              </div>
              <div class="prop-item">
                <label>分类 (Category)</label>
                <div class="select-wrapper">
                  <select v-model="editingTask.categoryId" class="modern-input">
                    <option v-for="c in categories" :key="c.id" :value="c.id">{{c.name}}</option>
                  </select>
                </div>
              </div>
              <div class="prop-item">
                <label>优先级 (Priority)</label>
                <div class="priority-tabs">
                  <button v-for="p in ['P0','P1','P2']" :key="p" :class="{active: editingTask.priority === p}" @click="editingTask.priority = p">{{p}}</button>
                </div>
              </div>
              <div class="prop-item">
                <label>截止日期 (Due Date)</label>
                <input type="date" v-model="editingTask.dueDateFormatted" class="modern-input" />
              </div>
              <div class="side-footer">
                <button class="btn-primary full" @click="saveTaskChanges" :disabled="isSaving">
                  {{ isSaving ? '保存中...' : '保存更改' }}
                </button>
                <button class="btn-danger full" @click="deleteTask">删除此任务</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'

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

// 响应式对象统一字段
const editingTask = reactive<any>({ 
  id: null,
  title: '',
  description: '',
  status: '',
  priority: 'P2',
  categoryId: 1,
  assigneeIds: [],
  dueDateFormatted: ''
})

const newTask = reactive({ 
  title: '', 
  categoryId: 1, 
  priority: 'P2', 
  status: '', 
  assigneeIds: [] as number[], 
  dueDate: '' 
})

const draggedTask = ref<any>(null)

// --- 计算属性 (修复字段读取) ---
const boardData = computed(() => {
  const existingStatusList = columns.value.map(c => c.name);
  return columns.value.map((col, index) => {
    // 统一读取 t.status
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

watch(projectProgress, (val) => {
  emit('update-progress', { percentage: val, count: tasksCount.value })
}, { immediate: true })

// --- API (映射全部改为小写) ---
const fetchColumns = async () => {
  try {
    const res = await apiClient.get(`/columns/project/${props.projectId}`)
    // 兼容 Id/id, Name/name
    columns.value = res.data.map((c: any) => ({ 
      id: c.id || c.Id, 
      name: c.name || c.Name, 
      isCompleted: c.isCompleted || c.IsCompleted 
    }))
  } catch (e) { console.error(e) }
}

const fetchTasks = async () => {
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/tasks`)
    rawTasks.value = res.data.map((t: any) => ({
      id: t.id || t.Id, 
      title: t.title || t.Title, 
      description: t.description || t.Description, 
      status: t.status || t.Status,
      priority: t.priority || t.Priority || 'P2', 
      categoryId: t.categoryId || t.CategoryId || 1, 
      assigneeIds: t.assigneeIds || t.AssigneeIds || [], 
      dueDate: t.dueDate || t.DueDate
    }))
  } catch (e) { console.error(e) }
}

// --- 动作 ---
const openCreateModal = () => { 
  newTask.status = columns.value[0]?.name || 'Todo'
  showCreateTask.value = true 
}
const quickAddTask = (s: string) => { 
  newTask.status = s
  showCreateTask.value = true 
}

const submitCreateTask = async () => {
  if (!newTask.title.trim()) return
  isSaving.value = true
  try {
    await apiClient.post('/tasks', {
      projectId: props.projectId, 
      title: newTask.title,
      categoryId: newTask.categoryId, 
      priority: newTask.priority,
      status: newTask.status, 
      assigneeIds: newTask.assigneeIds,
      dueDate: newTask.dueDate ? new Date(newTask.dueDate) : null
    })
    showCreateTask.value = false; 
    newTask.title = ''; 
    newTask.assigneeIds = []; 
    newTask.dueDate = ''
    await fetchTasks()
  } catch (e) { alert('发布失败') } finally { isSaving.value = false }
}

const saveTaskChanges = async () => {
  if (!editingTask.id) return
  isSaving.value = true
  try {
    await apiClient.put(`/tasks/${editingTask.id}`, {
      title: editingTask.title, 
      description: editingTask.description,
      status: editingTask.status, 
      priority: editingTask.priority,
      assigneeIds: editingTask.assigneeIds, 
      categoryId: editingTask.categoryId,
      dueDate: editingTask.dueDateFormatted ? new Date(editingTask.dueDateFormatted) : null
    })
    await fetchTasks(); 
    isDetailOpen.value = false
  } catch(e) { alert('同步失败') } finally { isSaving.value = false }
}

const toggleAssignee = (uid: number) => { 
  if(!editingTask.assigneeIds) editingTask.assigneeIds = []
  const idx = editingTask.assigneeIds.indexOf(uid); 
  idx > -1 ? editingTask.assigneeIds.splice(idx,1) : editingTask.assigneeIds.push(uid) 
}

const toggleAssigneeInNewTask = (uid: number) => { 
  const idx = newTask.assigneeIds.indexOf(uid); 
  idx > -1 ? newTask.assigneeIds.splice(idx,1) : newTask.assigneeIds.push(uid) 
}

const onDrop = async (e: any, status: string) => {
  if (!draggedTask.value || draggedTask.value.status === status) return
  const oldStatus = draggedTask.value.status; 
  const taskId = draggedTask.value.id;
  
  // 乐观更新 UI
  draggedTask.value.status = status
  
  try { 
    await apiClient.put(`/tasks/${taskId}`, { status: status }) 
  } catch { 
    draggedTask.value.status = oldStatus 
  }
}

const onDragStart = (e: any, t: any) => { draggedTask.value = t }

const editColumn = (col: any) => { 
  editingColId.value = col.id; 
  tempColName.value = col.name 
}

const saveColumnName = async (col: any) => { 
  await apiClient.put(`/columns/${col.id}`, { 
    name: tempColName.value, 
    isCompleted: col.isCompleted 
  }); 
  await fetchColumns(); 
  editingColId.value = null 
}
const toggleColumnDone = async (col: any) => { 
  // 1. 🌟 核心修复：去源数据 columns.value 里找到真正的那个列，而不是修改拷贝！
  const targetCol = columns.value.find(c => c.id === col.id);
  if (!targetCol) return;

  // 2. 修改源数据，触发 Vue 响应式重新渲染页面
  targetCol.isCompleted = !targetCol.isCompleted; 

  try {
    // 3. 发送给后端
    await apiClient.put(`/columns/${targetCol.id}`, { 
      isCompleted: targetCol.isCompleted, 
      name: targetCol.name 
    });
  } catch (error) {
    // 如果后端报错，把状态改回来
    targetCol.isCompleted = !targetCol.isCompleted;
    console.error("状态同步失败", error);
  }
}

const deleteColumn = async (id: number) => { 
  if(confirm('确定要删除这个阶段吗？')) { 
    await apiClient.delete(`/columns/${id}`); 
    fetchColumns() 
  } 
}

const submitNewColumn = async () => { 
  if(!newColumnName.value) return; 
  await apiClient.post('/columns', { 
    projectId: props.projectId, 
    name: newColumnName.value 
  }); 
  newColumnName.value = ''; 
  isAddingColumn.value = false; 
  await fetchColumns() 
}

const openTaskDetail = (t: any) => { 
  // 深度克隆，防止修改时影响列表 UI
  const clone = JSON.parse(JSON.stringify(t))
  Object.assign(editingTask, clone); 
  // 格式化日期给 <input type="date">
  editingTask.dueDateFormatted = clone.dueDate ? clone.dueDate.split('T')[0] : ''
  isDetailOpen.value = true 
}

const closeDetail = () => isDetailOpen.value = false

const deleteTask = async () => { 
  if (confirm('确认删除此任务？该操作无法恢复。')) { 
    await apiClient.delete(`/tasks/${editingTask.id}`); 
    await fetchTasks(); 
    isDetailOpen.value = false 
  } 
}

// 格式化工具
const getCategoryColor = (id: number) => categories.value.find(c => c.id === id)?.color || '#999'
const getCategoryName = (id: number) => categories.value.find(c => c.id === id)?.name || 'General'
const formatDateShort = (d: string) => { 
  if(!d) return ''; 
  const da = new Date(d); 
  return `${da.getMonth()+1}月${da.getDate()}日` 
}
const isOverdue = (d: string) => { 
  if(!d) return false; 
  const target = new Date(d);
  const now = new Date();
  return target < now && target.toDateString() !== now.toDateString() 
}
const vFocus = { mounted: (el: any) => el.focus() }

defineExpose({ openCreateModal })

onMounted(() => { 
  fetchColumns(); 
  fetchTasks() 
})
</script>

<style scoped>
/* ================= 核心变量 (Design Tokens) ================= */
.board-wrapper {
  --primary: #0052CC;
  --primary-hover: #0043A6;
  --success: #36B37E;
  --danger: #DE350B;
  --danger-bg: #FFEBE6;
  
  --bg-main: #FFFFFF;
  --bg-track: #F4F5F7;
  --bg-col: #EBECF0;
  --bg-card: #FFFFFF;
  --bg-card-hover: #FAFBFC;
  
  --text-main: #172B4D;
  --text-muted: #5E6C84;
  --text-inverse: #FFFFFF;
  
  --border-light: #DFE1E6;
  --border-focus: #4C9AFF;
  
  --shadow-sm: 0 1px 2px rgba(9, 30, 66, 0.05);
  --shadow-md: 0 4px 8px -2px rgba(9, 30, 66, 0.08), 0 0 1px rgba(9, 30, 66, 0.31);
  --shadow-lg: 0 8px 16px -4px rgba(9, 30, 66, 0.08), 0 0 1px rgba(9, 30, 66, 0.31);
  --shadow-modal: 0 16px 32px -8px rgba(9, 30, 66, 0.12), 0 0 1px rgba(9, 30, 66, 0.31);

  flex: 1; 
  overflow: hidden; 
  display: flex; 
  height: 100%;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: var(--text-main);
  background-color: var(--bg-main);
}

/* ================= 滚动条美化 ================= */
::-webkit-scrollbar { width: 8px; height: 8px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: #C1C7D0; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #A5ADBA; }

/* ================= 画布与轨道 ================= */
.board-canvas { flex: 1; overflow-x: auto; overflow-y: hidden; padding: 24px; display: flex; }
.board-track { display: inline-flex; height: 100%; align-items: flex-start; gap: 16px; padding-bottom: 12px; }

/* ================= 列 (Column) ================= */
.board-col { width: 280px; min-width: 280px; max-height: 100%; background: var(--bg-track); border-radius: 8px; display: flex; flex-direction: column; transition: background 0.3s; }
.board-col.col-done { background: #E3FCEF; }

.col-header { padding: 14px 16px 10px 16px; display: flex; justify-content: space-between; align-items: center; font-size: 14px; font-weight: 600; }
.col-title-area { flex: 1; cursor: pointer; border-radius: 4px; padding: 2px 4px; margin-left: -4px; transition: background 0.2s; }
.col-title-area:hover { background: rgba(9, 30, 66, 0.08); }
.col-label { display: flex; align-items: center; }
.col-label .name { color: var(--text-main); font-size: 14px; }
.col-label .count { font-weight: 500; color: var(--text-muted); margin-left: 8px; background: var(--border-light); padding: 2px 8px; border-radius: 12px; font-size: 12px; }

.col-rename-input { width: 100%; padding: 6px; border: 2px solid var(--border-focus); border-radius: 4px; font-family: inherit; font-size: 14px; font-weight: 600; outline: none; background: #fff; }

.col-ops { display: flex; gap: 4px; margin-left: 8px; }
.col-ops .op-btn { width: 24px; height: 24px; border: none; background: transparent; color: var(--text-muted); cursor: pointer; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 14px; transition: 0.2s; }
.col-ops .op-btn:hover { background: rgba(9, 30, 66, 0.08); color: var(--text-main); }
.col-ops .check.active { color: var(--success); }
.col-ops .del:hover { color: var(--danger); background: var(--danger-bg); }

/* ================= 任务卡片 ================= */
.task-container { padding: 0 12px 12px 12px; overflow-y: auto; flex: 1; display: flex; flex-direction: column; gap: 10px; }
.task-card { background: var(--bg-card); border-radius: 8px; box-shadow: var(--shadow-sm); padding: 14px; cursor: pointer; border: 1px solid var(--border-light); transition: all 0.2s ease; position: relative; }
.task-card:hover { background: var(--bg-card-hover); transform: translateY(-2px); box-shadow: var(--shadow-md); border-color: #B3D4FF; }
/* ================= 完成状态卡片视觉效果 ================= */
.card-dimmed { 
  opacity: 0.5; 
  filter: grayscale(100%); 
  background-color: #F8F9FA; /* 让背景稍微灰一点 */
  border-color: transparent;
}

/* 核心：给完成状态的任务标题加上中划线 */
.card-dimmed .card-title {
  text-decoration: line-through;
  color: var(--text-muted);
}

/* 可选：让完成的任务卡片里的标签和头像也进一步变淡，降低视觉干扰 */
.card-dimmed .card-badges,
.card-dimmed .card-meta {
  opacity: 0.7;
}

/* 可选：鼠标悬浮时不再有明显的上浮动画 */
.card-dimmed:hover {
  transform: none;
  box-shadow: var(--shadow-sm);
  border-color: transparent;
}
.card-badges { display: flex; gap: 8px; margin-bottom: 10px; flex-wrap: wrap; }
.badge-priority { font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 12px; }
.badge-priority.P0 { background: var(--danger-bg); color: var(--danger); } 
.badge-priority.P1 { background: #FFFAE6; color: #FF8B00; } 
.badge-priority.P2 { background: #DEEBFF; color: #0052CC; }
.badge-tag { font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 12px; }

.card-title { font-size: 14px; font-weight: 500; color: var(--text-main); margin-bottom: 14px; line-height: 1.5; word-wrap: break-word; }

.card-meta { display: flex; justify-content: space-between; align-items: center; }
.assignee-row { display: flex; align-items: center; }
.mini-av-wrapper { width: 24px; height: 24px; border-radius: 50%; border: 2px solid #fff; margin-right: -8px; background: #fff; position: relative; }
.mini-av { width: 100%; height: 100%; }
.mini-av :deep(*) { width: 100% !important; height: 100% !important; border-radius: 50% !important; }
.no-assignee { font-size: 12px; color: var(--text-muted); font-style: italic; }

.date-badge { font-size: 11px; font-weight: 500; color: var(--text-muted); display: flex; align-items: center; background: var(--bg-track); padding: 4px 8px; border-radius: 4px; }
.date-badge.overdue { color: var(--danger); background: var(--danger-bg); font-weight: 600; }

.ghost-btn { width: 100%; text-align: left; padding: 10px 12px; color: var(--text-muted); font-weight: 500; background: transparent; border: none; cursor: pointer; border-radius: 6px; transition: background 0.2s; font-size: 13px; display: flex; align-items: center; gap: 6px; }
.ghost-btn:hover { background: rgba(9,30,66,0.08); color: var(--text-main); }
.plus-icon { font-size: 16px; font-weight: 300; }

/* ================= 添加列占位 ================= */
.add-col-placeholder { min-width: 280px; width: 280px; }
.btn-add-col { width: 100%; height: 48px; border: none; background: rgba(9, 30, 66, 0.04); color: var(--text-muted); cursor: pointer; border-radius: 8px; font-weight: 500; font-size: 14px; transition: 0.2s; display: flex; align-items: center; justify-content: center; gap: 6px;}
.btn-add-col:hover { background: rgba(9, 30, 66, 0.08); color: var(--text-main); }
.new-col-input-box { background: var(--bg-card); padding: 12px; border-radius: 8px; box-shadow: var(--shadow-md); border: 1px solid var(--border-light); }
.row-actions { display: flex; gap: 8px; margin-top: 10px; }

/* ================= 通用按钮与输入框 ================= */
.modern-input, .modern-textarea, .linear-select { width: 100%; padding: 10px 12px; border: 2px solid var(--border-light); border-radius: 6px; font-size: 14px; color: var(--text-main); transition: 0.2s; background: #FAFBFC; font-family: inherit; box-sizing: border-box; }
.modern-input:focus, .modern-textarea:focus, .linear-select:focus { background: #fff; border-color: var(--border-focus); outline: none; box-shadow: 0 0 0 3px rgba(76, 154, 255, 0.2); }
.modern-textarea { resize: vertical; line-height: 1.6; }

.btn-sm { padding: 6px 12px; font-size: 13px; font-weight: 500; border: none; border-radius: 4px; cursor: pointer; transition: 0.2s; background: transparent; color: var(--text-main); }
.btn-sm:hover { background: rgba(9, 30, 66, 0.08); }
.btn-sm.primary { background: var(--primary); color: #fff; }
.btn-sm.primary:hover { background: var(--primary-hover); }

.btn-primary { background: var(--primary); color: var(--text-inverse); border: none; border-radius: 6px; padding: 10px 20px; font-weight: 500; font-size: 14px; cursor: pointer; transition: 0.2s; display: inline-flex; align-items: center; justify-content: center; }
.btn-primary:not(:disabled):hover { background: var(--primary-hover); box-shadow: var(--shadow-sm); }
.btn-ghost { background: transparent; border: none; color: var(--text-muted); font-weight: 500; cursor: pointer; padding: 10px 20px; font-size: 14px; border-radius: 6px; transition: 0.2s; }
.btn-ghost:hover { background: var(--bg-track); color: var(--text-main); }
.btn-danger { border: none; background: var(--danger-bg); color: var(--danger); padding: 10px; border-radius: 6px; font-weight: 500; cursor: pointer; transition: 0.2s; }
.btn-danger:hover { background: #FFBDAD; }

button:disabled { cursor: not-allowed; opacity: 0.6; }

/* ================= 模态框通用体系 ================= */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-panel { background: var(--bg-card); border-radius: 8px; display: flex; flex-direction: column; box-shadow: var(--shadow-modal); max-width: 95vw; }
.modal-panel.small { width: 480px; }
.modal-panel.large { width: 960px; height: 85vh; }

.modal-header { padding: 20px 28px; border-bottom: 1px solid var(--border-light); display: flex; justify-content: space-between; align-items: center; }
.modal-header.no-border { border-bottom: none; padding-bottom: 10px; }
.modal-header h3 { margin: 0; font-size: 18px; font-weight: 600; color: var(--text-main); }
.task-id-badge { font-size: 13px; font-weight: 600; color: var(--text-muted); background: var(--bg-track); padding: 4px 10px; border-radius: 4px; letter-spacing: 0.5px; }

.close-icon { background: none; border: none; cursor: pointer; color: var(--text-muted); border-radius: 4px; padding: 4px; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.close-icon:hover { background: var(--bg-track); color: var(--text-main); }

.modal-body { padding: 28px; flex: 1; overflow-y: auto; display: flex; flex-direction: column; }
.modal-footer { padding: 16px 28px; border-top: 1px solid var(--border-light); display: flex; justify-content: flex-end; gap: 12px; background: #FAFBFC; border-radius: 0 0 8px 8px; }

/* 表单布局 */
.form-group { margin-bottom: 20px; }
.form-row { display: flex; gap: 20px; }
.form-row .form-group { flex: 1; }
.form-group label { display: block; font-size: 13px; font-weight: 600; color: var(--text-muted); margin-bottom: 8px; }

.select-wrapper { position: relative; }
.select-wrapper::after { content: "▼"; position: absolute; right: 14px; top: 50%; transform: translateY(-50%); font-size: 10px; color: var(--text-muted); pointer-events: none; }
.select-wrapper select { appearance: none; cursor: pointer; }

/* 详情编辑专区 */
.detail-layout { display: flex; flex: 1; overflow: hidden; }
.detail-main { flex: 2.5; padding: 10px 32px 32px 32px; overflow-y: auto; display: flex; flex-direction: column; gap: 24px;}
.detail-side { flex: 1; padding: 24px; background: #FAFBFC; border-left: 1px solid var(--border-light); display: flex; flex-direction: column; gap: 20px; overflow-y: auto;}

.detail-title-input { font-size: 26px; font-weight: 600; color: var(--text-main); border: none; width: 100%; outline: none; padding: 0; background: transparent; }
.detail-title-input::placeholder { color: #A5ADBA; }

.detail-block { display: flex; flex-direction: column; gap: 12px; flex: 1; }
.block-label { font-size: 15px; font-weight: 600; color: var(--text-main); }

.prop-item { display: flex; flex-direction: column; gap: 8px; }
.prop-item label { font-size: 12px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; }

/* 用户选择芯片 */
.user-grid { display: flex; flex-wrap: wrap; gap: 10px; }
.user-chip { padding: 6px 14px; background: var(--bg-track); color: var(--text-main); border-radius: 20px; font-size: 13px; font-weight: 500; cursor: pointer; border: 1px solid transparent; transition: 0.2s; }
.user-chip:hover { background: #EBECF0; }
.user-chip.active { background: #DEEBFF; color: var(--primary); border-color: var(--border-focus); }
.user-chip.with-av { display: flex; gap: 8px; align-items: center; padding: 4px 14px 4px 4px; }
.chip-av-wrapper { width: 24px; height: 24px; border-radius: 50%; overflow: hidden; }
.chip-av :deep(*) { width: 100% !important; height: 100% !important; border-radius: 50% !important; }

/* 优先级切换器 */
.priority-tabs { display: flex; background: var(--border-light); padding: 3px; border-radius: 6px; }
.priority-tabs button { flex: 1; border: none; background: transparent; padding: 8px; cursor: pointer; font-size: 13px; font-weight: 600; color: var(--text-muted); border-radius: 4px; transition: 0.2s; }
.priority-tabs button:hover { color: var(--text-main); }
.priority-tabs button.active { background: var(--bg-card); color: var(--text-main); box-shadow: var(--shadow-sm); }

.side-footer { margin-top: auto; display: flex; flex-direction: column; gap: 12px; padding-top: 24px; }
.full { width: 100%; margin: 0; }

/* Vue 动画过渡 */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s cubic-bezier(0.2, 0.8, 0.2, 1); }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: translateY(10px) scale(0.98); }
</style>