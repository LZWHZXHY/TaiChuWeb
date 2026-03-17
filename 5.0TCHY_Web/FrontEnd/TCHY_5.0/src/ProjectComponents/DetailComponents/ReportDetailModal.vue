<template>
  <transition name="modal-fade" appear>
    <div class="modal-backdrop" @click.self="closeModal">
      <div class="modal-panel expanded-mode">
        
        <div class="modal-header rep-view-header">
          <div class="rep-view-title-group">
            <div class="strict-avatar-40 shadow-sm">
              <UniversalAvatar :user-id="report.userId" :show-level="false" />
            </div>
            <div class="rep-view-info">
              <h3>{{ report.userName }} 的工作汇报</h3>
              <div class="meta-info-container">
                <span class="meta-info">{{ report.type || 'Weekly' }} · 发布于 {{ formatTimeAgo(report.createdAt) }}</span>
                <span 
                  v-if="isEdited" 
                  class="edited-tag" 
                  :title="'最后修改于: ' + formatExactTime(report.updatedAt)"
                >
                  (已编辑)
                </span>
              </div>
            </div>
          </div>
          
          <div class="header-actions">
            <button 
              v-if="!isEditing && canEdit" 
              class="btn-ghost edit-btn" 
              @click="startEdit"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 4px;"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
              修改汇报
            </button>
            
            <template v-if="isEditing">
              <button class="btn-ghost" @click="cancelEdit" :disabled="isSaving">取消</button>
              <button class="btn-primary" @click="saveEdit" :disabled="isSaving || !editForm.content.trim()">
                {{ isSaving ? '保存中...' : '保存更改' }}
              </button>
            </template>
            
            <button v-if="!isEditing" class="close-icon" @click="closeModal" title="关闭 (Esc)">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
            </button>
          </div>
        </div>
        
        <div class="modal-body report-reader">
          
          <template v-if="!isEditing">
            <div class="markdown-body">
              <pre>{{ report.content }}</pre>
            </div>
            
            <div v-if="report.completedTasks?.length" class="linked-tasks-box">
              <h4 class="tasks-box-title">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color:#36B37E"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                本期完成关联任务
              </h4>
              <ul class="clean-list">
                <li v-for="task in report.completedTasks" :key="task.taskId" class="task-list-item">
                  <span class="task-bullet"></span>
                  {{ task.title }}
                </li>
              </ul>
            </div>
          </template>

          <template v-else>
            <div class="edit-form">
              <div class="form-group" style="width: 200px;">
                <label>汇报类型</label>
                <select v-model="editForm.type" class="modern-input">
                  <option value="Weekly">Weekly Report (周报)</option>
                  <option value="Monthly">Monthly Report (月报)</option>
                </select>
              </div>
              <div class="form-group flex-fill">
                <label>修改正文内容</label>
                <textarea 
                  v-model="editForm.content" 
                  class="modern-textarea" 
                  placeholder="请输入您的工作汇报内容..."
                ></textarea>
              </div>
            </div>
          </template>

        </div>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import apiClient from '@/utils/api'

const props = defineProps<{
  report: any
  currentUserId: string | number
  isCycleLocked: boolean
}>()

const emit = defineEmits(['close', 'refresh'])

const isEditing = ref(false)
const isSaving = ref(false)
const editForm = reactive({ type: 'Weekly', content: '' })

// 🌟 核心判断重写：抛弃毫秒级计算，直接用更稳健的逻辑判断
const isEdited = computed(() => {
  if (!props.report.updatedAt) return false;
  
  // 第一重判断：如果字符串完全不一致（哪怕差一秒），就肯定是修改过的
  if (props.report.updatedAt !== props.report.createdAt) {
      // 为了防止数据库插入时有毫秒级误差，我们容忍 5 秒以内的差异
      const created = new Date(props.report.createdAt).getTime();
      const updated = new Date(props.report.updatedAt).getTime();
      return (updated - created) > 5000;
  }
  return false;
})

const canEdit = computed(() => {
  return props.report.userId === Number(props.currentUserId) && !props.isCycleLocked
})

const closeModal = () => {
  if (isEditing.value) {
    if(!confirm('有未保存的修改，确认要关闭吗？')) return;
  }
  emit('close')
}

const startEdit = () => {
  editForm.type = props.report.type || 'Weekly'
  editForm.content = props.report.content || ''
  isEditing.value = true
}

const cancelEdit = () => {
  isEditing.value = false
}

const saveEdit = async () => {
  if (!editForm.content.trim()) return
  isSaving.value = true
  
  try {
    await apiClient.put(`/Reports/${props.report.id}`, {
      type: editForm.type,
      content: editForm.content
    })
    
    isEditing.value = false
    emit('refresh')
  } catch (error: any) {
    console.error(error)
    alert(error.response?.data || '修改失败，请重试')
  } finally {
    isSaving.value = false
  }
}

// 时间格式化：相对时间
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

// 时间格式化：绝对精确时间（鼠标悬停查看）
const formatExactTime = (dateString: string) => {
  return new Date(dateString).toLocaleString('zh-CN');
}
</script>

<style scoped>
/* 模态框底层与尺寸 */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); backdrop-filter: blur(4px); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-panel { background: #FFFFFF; border-radius: 8px; display: flex; flex-direction: column; box-shadow: 0 16px 32px -8px rgba(9, 30, 66, 0.12), 0 0 1px rgba(9, 30, 66, 0.31); max-width: 95vw; }
.modal-panel.expanded-mode { width: 850px; height: 85vh; }

.modal-header { padding: 24px 32px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; background: #FAFBFC; border-radius: 8px 8px 0 0; }
.rep-view-title-group { display: flex; align-items: center; gap: 16px; }
.rep-view-info { display: flex; flex-direction: column; gap: 4px; }
.rep-view-info h3 { font-size: 20px; margin: 0; color: #172B4D; }

/* 元数据与已编辑标签 */
.meta-info-container { display: flex; align-items: center; gap: 6px; }
.meta-info { font-size: 13px; color: #6B778C; font-weight: 500; }
.edited-tag { font-size: 12px; color: #8993A4; font-style: italic; cursor: help; }
.edited-tag:hover { color: #0052CC; text-decoration: underline; }

/* 头像外壳保护 */
.strict-avatar-40 { width: 40px; height: 40px; min-width: 40px; max-width: 40px; border-radius: 50%; overflow: hidden; display: block; flex-shrink: 0; box-shadow: 0 1px 2px rgba(9, 30, 66, 0.05);}
.strict-avatar-40 :deep(*) { width: 100% !important; height: 100% !important; min-width: 100% !important; min-height: 100% !important; max-width: 100% !important; max-height: 100% !important; object-fit: cover !important; border-radius: 50% !important; display: block !important; margin: 0 !important; padding: 0 !important; }

/* 按钮与操作区 */
.header-actions { display: flex; align-items: center; gap: 12px; }
.edit-btn { color: #0052CC; background: #DEEBFF; }
.edit-btn:hover { background: #B3D4FF; }

.close-icon { background: none; border: none; cursor: pointer; color: #6B778C; border-radius: 4px; padding: 4px; display: flex; align-items: center; justify-content: center; transition: background 0.2s; }
.close-icon:hover { background: #EBECF0; color: #172B4D; }

button { font-family: inherit; }
button:disabled { cursor: not-allowed; opacity: 0.6; }
.btn-primary { display: inline-flex; align-items: center; justify-content: center; background: #0052CC; color: #FFFFFF; border: none; border-radius: 4px; padding: 8px 16px; font-weight: 500; font-size: 14px; cursor: pointer; transition: 0.2s; }
.btn-primary:not(:disabled):hover { background: #0043A6; }
.btn-ghost { background: transparent; border: 1px solid transparent; color: #172B4D; cursor: pointer; padding: 8px 16px; font-size: 14px; font-weight: 500; border-radius: 4px; transition: 0.2s; }
.btn-ghost:not(:disabled):hover { background: #F4F5F7; }

/* 主体阅读区 */
.modal-body { padding: 32px 40px; flex: 1; overflow-y: auto; display: flex; flex-direction: column; }
.report-reader pre { white-space: pre-wrap; word-wrap: break-word; font-family: inherit; line-height: 1.8; color: #172B4D; font-size: 15px; margin: 0; }

.linked-tasks-box { margin-top: 40px; padding: 24px; background: #F7F8FA; border-radius: 8px; border: 1px solid #EBECF0; }
.tasks-box-title { font-size: 15px; font-weight: 600; color: #172B4D; margin: 0 0 16px 0; display: flex; align-items: center; gap: 8px; }
.clean-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 10px; }
.task-list-item { font-size: 14px; color: #42526E; display: flex; align-items: flex-start; gap: 10px; line-height: 1.5; }
.task-bullet { width: 6px; height: 6px; background: #0052CC; border-radius: 50%; margin-top: 7px; flex-shrink: 0; }

/* 编辑模式表单 */
.edit-form { display: flex; flex-direction: column; height: 100%; }
.form-group { display: flex; flex-direction: column; margin-bottom: 20px; }
.form-group label { font-size: 13px; font-weight: 600; color: #6B778C; margin-bottom: 8px; }
.flex-fill { flex: 1; }
.modern-input, .modern-textarea { width: 100%; padding: 10px 12px; border: 2px solid #DFE1E6; border-radius: 6px; font-size: 14px; color: #172B4D; background: #FFFFFF; font-family: inherit; transition: 0.2s; box-sizing: border-box; }
.modern-input:focus, .modern-textarea:focus { outline: none; border-color: #4C9AFF; box-shadow: 0 0 0 3px rgba(76, 154, 255, 0.2); }
.modern-textarea { flex: 1; min-height: 300px; resize: vertical; line-height: 1.6; }

/* 动画效果 */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s cubic-bezier(0.2, 0.8, 0.2, 1); }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: translateY(10px) scale(0.98); }
</style>