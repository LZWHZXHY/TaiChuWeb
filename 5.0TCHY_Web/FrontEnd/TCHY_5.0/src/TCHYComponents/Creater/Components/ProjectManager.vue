<template>
  <div class="app-layout">
    
    <aside class="sidebar">
      <div class="brand">
        <div class="logo-box">🪐</div>
        <div class="brand-text">
          <span class="name">太初工坊</span>
          <span class="ver">Pro v2.3</span>
        </div>
      </div>
      <div class="nav-group">
        <div class="nav-title">职能部门</div>
        <ul class="nav-list">
          <li v-for="dept in departments" :key="dept.key" :class="{ active: currentDept === dept.key }" @click="selectDept(dept.key)">
            <span class="nav-icon">{{ dept.icon }}</span><span class="nav-name">{{ dept.name }}</span>
          </li>
        </ul>
      </div>
    </aside>

    <main class="workspace">
      <header class="toolbar">
        <div class="breadcrumb">
          <span class="curr-dept">{{ getDeptName(currentDept) }}</span>
          <span class="divider">/</span>
          <span class="view-name">看板视图</span>
        </div>
        <div class="actions">
          <div class="search-box">
            <span class="icon">🔍</span>
            <input type="text" placeholder="搜索任务..." />
          </div>
          <button class="primary-btn" @click="openAddTaskModal">+ 新建任务</button>
        </div>
      </header>

      <div class="board-scroller">
        <div class="board-container">
          <div v-for="col in columns" :key="col.Key" class="board-column">
            <div class="column-header">
              <span class="col-name" :class="col.TitleClass">{{ col.Name }}</span>
              <span class="col-count">{{ columnTasks[col.Key] ? columnTasks[col.Key].length : 0 }}</span>
            </div>

            <draggable
              class="column-body"
              v-model="columnTasks[col.Key]"
              group="tasks"
              animation="300"
              item-key="Id"
              ghost-class="ghost-card"
              drag-class="dragging-card"
              @change="(event) => onTaskMove(event, col.Key)"
            >
              <template #item="{ element: t }">
                <div 
                  class="kanban-card"
                  :class="{ 
                    'active-border': col.Key === 'doing', 
                    'review-style': col.Key === 'review', 
                    'submit-style': col.Key === 'submit',
                    'done-style': col.Key === 'done' 
                  }"
                  @click="openEditModal(t)"
                >
                  <div class="card-check" v-if="col.Key === 'done'"><span class="check-icon">✓ 已归档</span></div>
                  <div class="card-check" v-if="col.Key === 'submit'"><span class="pass-icon">🎉 已通过</span></div>

                  <div class="card-badges">
                    <span class="badge-proj">{{ t.ProjectName }}</span>
                    <span class="badge-prio high" v-if="t.Id % 2 === 0">P0</span>
                  </div>

                  <h4 class="card-title" :class="{ 'del': col.Key === 'done' }">{{ t.Title }}</h4>
                  
                  <div class="card-meta">
                    <div class="assignee-avatar" :title="t.Assignee">{{ t.Assignee ? t.Assignee[0] : 'U' }}</div>
                    <span class="date">{{ formatDate(t.CreateTime) }}</span>
                  </div>
                </div>
              </template>
            </draggable>
          </div>
          <div class="board-column add-column">
            <button class="add-col-btn" @click="openAddColumnModal">+ 添加列表</button>
          </div>
        </div>
      </div>
    </main>

    <div class="modal-overlay" v-if="showTaskModal" @click.self="closeTaskModal">
      <div class="modal-card">
        <div class="modal-header"><h3>发布新任务</h3><button class="close-btn" @click="closeTaskModal">✕</button></div>
        <div class="modal-body">
          <div class="form-group"><label>任务标题</label><input type="text" v-model="newTaskForm.title" ref="titleInput"></div>
          <div class="form-group"><label>所属项目</label><input type="text" v-model="newTaskForm.projectName"></div>
          <div class="form-group"><label>负责人</label><input type="text" v-model="newTaskForm.assignee" placeholder="默认: User"></div>
          <div class="form-group"><label>状态</label>
            <select v-model="newTaskForm.status">
              <option v-for="col in columns" :key="col.Key" :value="col.Key">{{ col.Name }}</option>
            </select>
          </div>
          <div class="form-group"><label>任务描述</label><textarea v-model="newTaskForm.desc" rows="3" placeholder="输入任务详情..."></textarea></div>
        </div>
        <div class="modal-footer">
          <button class="btn-cancel" @click="closeTaskModal">取消</button>
          <button class="btn-confirm" @click="submitTask">🚀 发布</button>
        </div>
      </div>
    </div>

    <div class="modal-overlay" v-if="showColumnModal" @click.self="closeColumnModal">
      <div class="modal-card">
        <div class="modal-header"><h3>添加列表</h3><button class="close-btn" @click="closeColumnModal">✕</button></div>
        <div class="modal-body">
          <div class="form-group"><label>名称</label><input type="text" v-model="newColumnForm.name" ref="colNameInput"></div>
          <div class="form-group"><label>Key</label><input type="text" v-model="newColumnForm.key"></div>
        </div>
        <div class="modal-footer"><button class="btn-cancel" @click="closeColumnModal">取消</button><button class="btn-confirm" @click="submitColumn">创建</button></div>
      </div>
    </div>

    <div class="modal-overlay" v-if="showEditModal" @click.self="closeEditModal">
      <div class="modal-card">
        <div class="modal-header"><h3>编辑任务详情</h3><button class="close-btn" @click="closeEditModal">✕</button></div>
        <div class="modal-body">
          <div class="form-group"><label>任务标题</label><input type="text" v-model="editTaskForm.Title"></div>
          <div class="form-row">
            <div class="form-group half">
                <label>归属部门</label>
                <select v-model="editTaskForm.Department"><option v-for="dept in departments" :key="dept.key" :value="dept.key">{{ dept.name }}</option></select>
            </div>
            <div class="form-group half"><label>所属项目</label><input type="text" v-model="editTaskForm.ProjectName"></div>
          </div>
          <div class="form-group"><label>负责人</label><input type="text" v-model="editTaskForm.Assignee"></div>
          <div class="form-group"><label>任务描述</label><textarea v-model="editTaskForm.desc" rows="5" placeholder="暂无描述..."></textarea></div>
          <div class="form-group"><label>创建时间</label><input type="text" :value="formatDate(editTaskForm.CreateTime)" disabled style="background:#f5f5f5; color:#999;"></div>
        </div>
        <div class="modal-footer">
          <button class="btn-cancel" @click="closeEditModal">取消</button>
          <button class="btn-confirm" @click="submitEditTask">💾 保存修改</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import apiClient from '@/utils/api';
import draggable from 'vuedraggable';

export default {
  name: 'ProjectManager',
  components: { draggable },
  data() {
    return {
      departments: [
        { key: 'setting', name: '世界观设定部', icon: '📜' },
        { key: 'game',    name: '游戏开发部',   icon: '🎮' },
        { key: 'anime',   name: '动画制作部',   icon: '🎬' },
        { key: 'novel',   name: '小说/剧本部',  icon: '🖋️' },
        { key: 'infra',   name: '平台架构部',   icon: '💻' },
      ],
      currentDept: 'setting',
      columns: [],
      columnTasks: {}, 
      showTaskModal: false,
      newTaskForm: { title: '', projectName: '', status: 'todo', assignee: 'User', desc: '' },
      showColumnModal: false,
      newColumnForm: { name: '', key: '' },
      showEditModal: false,
      editTaskForm: {} 
    };
  },
  async mounted() {
    await this.fetchColumns();
    await this.fetchTasksByDept(this.currentDept);
  },
  methods: {
    getDeptName(key) { const d = this.departments.find(i => i.key === key); return d ? d.name : ''; },
    selectDept(key) { this.currentDept = key; this.fetchTasksByDept(key); },
    async fetchColumns() {
      try {
        const res = await apiClient.get('/Column');
        this.columns = res.data;
        this.columns.forEach(col => { if(!this.columnTasks[col.Key]) this.columnTasks[col.Key] = []; });
      } catch (e) { console.error(e); }
    },
    async fetchTasksByDept(key) {
      this.columnTasks = {};
      this.columns.forEach(col => { this.columnTasks[col.Key] = []; });
      try {
        const res = await apiClient.get(`/Task?department=${key}`);
        res.data.forEach(task => {
            const statusKey = task.Status;
            if (this.columnTasks[statusKey]) this.columnTasks[statusKey].push(task);
            else { if(!this.columnTasks['unknown']) this.columnTasks['unknown'] = []; this.columnTasks['unknown'].push(task); }
        });
      } catch (e) { console.error(e); }
    },
    async onTaskMove(evt, statusKey) {
        if (evt.added) {
            const task = evt.added.element;
            const newStatus = statusKey;
            task.Status = newStatus; 
            try { await apiClient.put(`/Task/${task.Id}`, { ...task, Status: newStatus }); } 
            catch (e) { console.error("更新失败", e); this.fetchTasksByDept(this.currentDept); alert("移动失败: " + e.message); }
        }
    },
    openAddTaskModal() {
      this.newTaskForm = { title: '', projectName: '', status: 'todo', assignee: 'User', desc: '' };
      if(this.columns.length > 0) this.newTaskForm.status = this.columns[0].Key;
      this.showTaskModal = true;
      this.$nextTick(() => { if(this.$refs.titleInput) this.$refs.titleInput.focus(); });
    },
    closeTaskModal() { this.showTaskModal = false; },
    async submitTask() {
        if (!this.newTaskForm.title || !this.newTaskForm.projectName) { alert("请填写完整信息"); return; }
        const newTask = {
            title: this.newTaskForm.title,
            department: this.currentDept,
            projectName: this.newTaskForm.projectName,
            status: this.newTaskForm.status,
            assignee: this.newTaskForm.assignee || 'User',
            desc: this.newTaskForm.desc,
            createTime: new Date()
        };
        try {
            const res = await apiClient.post('/Task', newTask);
            const createdTask = res.data;
            if (this.columnTasks[createdTask.Status]) this.columnTasks[createdTask.Status].unshift(createdTask);
            this.closeTaskModal();
        } catch (e) { alert('发布失败: ' + e.message); }
    },
    openAddColumnModal() { this.newColumnForm = { name: '', key: '' }; this.showColumnModal = true; this.$nextTick(() => { if(this.$refs.colNameInput) this.$refs.colNameInput.focus(); }); },
    closeColumnModal() { this.showColumnModal = false; },
    async submitColumn() {
        if (!this.newColumnForm.name || !this.newColumnForm.key) return;
        try {
            const res = await apiClient.post('/Column', { name: this.newColumnForm.name, key: this.newColumnForm.key, titleClass: '' });
            this.columns.push(res.data);
            this.columnTasks[res.data.Key] = [];
            this.closeColumnModal();
        } catch (e) { alert("添加失败"); }
    },
    openEditModal(task) { this.editTaskForm = JSON.parse(JSON.stringify(task)); this.showEditModal = true; },
    closeEditModal() { this.showEditModal = false; this.editTaskForm = {}; },
    async submitEditTask() {
      try {
        const res = await apiClient.put(`/Task/${this.editTaskForm.Id}`, this.editTaskForm);
        const updatedTask = res.data;
        if (updatedTask.Department !== this.currentDept) {
            const list = this.columnTasks[updatedTask.Status];
            const index = list.findIndex(t => t.Id === updatedTask.Id);
            if (index !== -1) list.splice(index, 1);
            alert(`任务已转移至【${this.getDeptName(updatedTask.Department)}】`);
        } else {
            const list = this.columnTasks[updatedTask.Status];
            if (list) {
                const index = list.findIndex(t => t.Id === updatedTask.Id);
                if (index !== -1) list.splice(index, 1, updatedTask);
            }
        }
        this.closeEditModal();
      } catch (e) { alert("保存失败: " + e.message); }
    },
    formatDate(d) { if(!d) return ''; const date = new Date(d); return `${date.getMonth()+1}/${date.getDate()}`; }
  }
};
</script>

<style scoped>
.app-layout { display: flex; width: 100%; height: 100vh; background: #f4f5f7; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Arial, sans-serif; }
.sidebar { width: 240px; background: #1e293b; color: #94a3b8; display: flex; flex-direction: column; border-right: 1px solid #334155; flex-shrink: 0; }
.brand { padding: 20px; display: flex; align-items: center; gap: 10px; border-bottom: 1px solid #334155; color: white; }
.logo-box { width: 32px; height: 32px; background: #3b82f6; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; }
.brand-text { display: flex; flex-direction: column; }
.brand-text .name { font-weight: bold; font-size: 0.95rem; }
.brand-text .ver { font-size: 0.7rem; opacity: 0.6; }
.nav-group { flex: 1; padding: 20px 10px; overflow-y: auto; }
.nav-title { font-size: 0.75rem; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 10px; padding-left: 10px; font-weight: bold; }
.nav-list { list-style: none; padding: 0; margin: 0; }
.nav-list li { padding: 10px 12px; margin-bottom: 4px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; gap: 10px; transition: all 0.2s; }
.nav-list li:hover { background: rgba(255,255,255,0.05); color: #f1f5f9; }
.nav-list li.active { background: #2563eb; color: white; }
.nav-icon { font-size: 1.1rem; }
.nav-name { font-size: 0.9rem; font-weight: 500; }
.workspace { flex: 1; display: flex; flex-direction: column; overflow: hidden; }
.toolbar { height: 64px; background: white; border-bottom: 1px solid #e2e8f0; display: flex; justify-content: space-between; align-items: center; padding: 0 24px; }
.breadcrumb { font-size: 1.1rem; font-weight: 600; color: #1e293b; display: flex; align-items: center; gap: 8px;}
.breadcrumb .divider { color: #cbd5e1; font-weight: normal;}
.breadcrumb .view-name { font-size: 0.9rem; color: #64748b; font-weight: normal; background: #f1f5f9; padding: 4px 8px; border-radius: 4px;}
.actions { display: flex; gap: 16px; align-items: center; }
.search-box { background: #f1f5f9; padding: 6px 12px; border-radius: 6px; display: flex; align-items: center; gap: 8px; color: #94a3b8; }
.search-box input { border: none; background: transparent; outline: none; font-size: 0.9rem; color: #334155; width: 150px; }
.primary-btn { background: #2563eb; color: white; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 600; cursor: pointer; font-size: 0.9rem; box-shadow: 0 2px 4px rgba(37,99,235,0.2); transition: background 0.2s; }
.primary-btn:hover { background: #1d4ed8; }
.board-scroller { flex: 1; overflow-x: auto; overflow-y: hidden; padding: 24px; }
.board-container { display: flex; gap: 20px; height: 100%; align-items: flex-start; }
.board-column { width: 300px; flex-shrink: 0; background: #ebecf0; border-radius: 8px; padding: 10px 10px 4px 10px; max-height: 100%; display: flex; flex-direction: column; }
.column-header { padding: 8px 4px 12px 4px; display: flex; justify-content: space-between; font-size: 0.9rem; font-weight: 600; color: #5e6c84; text-transform: uppercase; }
.column-body { overflow-y: auto; flex: 1; padding-right: 4px; padding-bottom: 8px; min-height: 50px; }
.kanban-card { background: white; border-radius: 4px; padding: 10px; margin-bottom: 8px; box-shadow: 0 1px 2px rgba(9,30,66,0.25); cursor: pointer; border-bottom: 2px solid transparent; transition: all 0.2s; }
.kanban-card:hover { background: #fafbfc; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.08); }

/* === 状态特殊样式 === */
.active-border { border-left: 3px solid #0052cc; }
.review-style { border-left: 3px solid #f59e0b; background: #fffbf0; } /* 审核中 - 橙色 */
.submit-style { border-left: 3px solid #10b981; background: #f0fdf4; } /* 已通过 - 绿色 */
.done-style { opacity: 0.7; background: #f4f5f7; box-shadow: none; border-left: 3px solid #ccc;} /* 已归档 - 灰色 */

.card-badges { display: flex; gap: 6px; margin-bottom: 8px; }
.badge-proj { font-size: 11px; background: #e0e7ff; color: #4338ca; padding: 2px 6px; border-radius: 3px; font-weight: 600; }
.review-style .badge-proj { background: #fef3c7; color: #d97706; } /* 审核中 Badge 变色 */
.submit-style .badge-proj { background: #d1fae5; color: #059669; } /* 通过 Badge 变色 */

.badge-prio { font-size: 11px; padding: 2px 6px; border-radius: 3px; font-weight: bold; }
.badge-prio.high { background: #fee2e2; color: #ef4444; }
.card-title { margin: 0 0 10px 0; font-size: 0.95rem; color: #172b4d; line-height: 1.4; font-weight: 500; }
.card-title.del { text-decoration: line-through; color: #6b778c; }
.card-meta { display: flex; align-items: center; justify-content: space-between; margin-top: 8px; }
.assignee-avatar { width: 24px; height: 24px; background: #dfe1e6; border-radius: 50%; font-size: 10px; display: flex; align-items: center; justify-content: center; color: #42526e; font-weight: bold; }
.date { font-size: 11px; color: #6b778c; }

/* 状态图标文字 */
.card-check { margin-bottom: 5px; }
.check-icon { color: #6b778c; font-weight: bold; font-size: 0.75rem; }
.pass-icon { font-size: 0.75rem; font-weight: bold; color: #059669; background: #d1fae5; padding: 2px 6px; border-radius: 4px; display: inline-block; }

.card-badges-bottom { margin-top: 5px; }
.badge-proj.gray { background: #e2e8f0; color: #64748b; }
.add-column { background: transparent; border: 2px dashed #dfe1e6; align-items: center; justify-content: center; cursor: pointer; min-height: 100px; transition: border-color 0.2s; }
.add-column:hover { border-color: #94a3b8; }
.add-col-btn { background: none; border: none; color: #64748b; font-weight: 600; cursor: pointer; font-size: 0.95rem; width: 100%; height: 100%; }
.add-col-btn:hover { color: #475569; }
.ghost-card { opacity: 0.5; background: #e2e4e7; border: 2px dashed #999; }
.dragging-card { transform: rotate(2deg); opacity: 1; }
.modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.6); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); animation: fadeIn 0.2s; }
.modal-card { background: white; width: 450px; border-radius: 12px; box-shadow: 0 20px 50px rgba(0,0,0,0.3); overflow: hidden; animation: slideUp 0.3s; }
.modal-header { padding: 20px 24px; border-bottom: 1px solid #f0f0f0; display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1.1rem; color: #333; }
.close-btn { background: none; border: none; font-size: 1.2rem; cursor: pointer; color: #999; }
.close-btn:hover { color: #333; }
.modal-body { padding: 24px; }
.form-row { display: flex; gap: 15px; }
.form-group { margin-bottom: 20px; }
.form-group.half { flex: 1; }
.form-group label { display: block; margin-bottom: 8px; font-size: 0.9rem; font-weight: 600; color: #34495e; }
.form-group input, .form-group select, textarea { width: 100%; padding: 10px 12px; border: 1px solid #dfe1e6; border-radius: 6px; font-size: 0.95rem; outline: none; transition: border-color 0.2s; box-sizing: border-box; }
.form-group input:focus, .form-group select:focus, textarea:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
textarea { resize: vertical; font-family: inherit; }
.modal-footer { padding: 16px 24px; background: #f8f9fa; border-top: 1px solid #f0f0f0; display: flex; justify-content: flex-end; gap: 12px; }
.btn-cancel { background: white; border: 1px solid #dcdfe6; color: #606266; padding: 8px 16px; border-radius: 6px; cursor: pointer; font-weight: 500; }
.btn-cancel:hover { border-color: #c6e2ff; color: #409eff; }
.btn-confirm { background: #3b82f6; border: none; color: white; padding: 8px 20px; border-radius: 6px; cursor: pointer; font-weight: 600; }
.btn-confirm:hover { background: #2563eb; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
</style>