<template>
  <div class="modal-overlay" v-if="visible" @click.self="closeModal">
    <div class="modal-content">
      <header class="modal-header">
        <h3>{{ isEdit ? '✏️ 修改分类目录' : '📁 新建分类目录' }}</h3>
        <button class="close-btn" @click="closeModal">×</button>
      </header>

      <div class="modal-body">
        <div class="form-group">
          <label>目录名称 <span class="required">*</span></label>
          <input 
            type="text" 
            v-model="form.name" 
            placeholder="例如：后端技术、日常规范..." 
            @keyup.enter="submitCategory"
          />
        </div>

        <div class="form-group">
          <label>父级目录 (挂在谁下面？)</label>
          <select v-model="form.parentId">
            <option :value="0">🏆 作为顶级目录 (不挂在任何目录下)</option>
            <option 
              v-for="cat in folderList" 
              :key="cat.id" 
              :value="cat.id"
              :disabled="cat.isDisabled"
            >
              {{ cat.name }} {{ cat.isDisabled ? '(不可选为自己的父级)' : '' }}
            </option>
          </select>
        </div>
      </div>

      <footer class="modal-footer">
        <div class="footer-left">
          <button v-if="isEdit" class="btn-delete" @click="confirmDelete">
            🗑️ 删除分类
          </button>
        </div>
        
        <div class="footer-right">
          <button class="btn-cancel" @click="closeModal">取消</button>
          <button class="btn-submit" @click="submitCategory">
            {{ isEdit ? '确认修改' : '确认创建' }}
          </button>
        </div>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  visible: Boolean,
  treeData: {
    type: Array,
    default: () => []
  },
  isEdit: {
    type: Boolean,
    default: false
  },
  editData: {
    type: Object,
    default: () => ({})
  }
})

// 🚀 增加了 delete 事件抛出
const emit = defineEmits(['close', 'submit', 'delete'])

const form = ref({
  name: '',
  parentId: 0 
})

watch(() => props.visible, (newVal) => {
  if (newVal) {
    if (props.isEdit && props.editData) {
      form.value.name = props.editData.name || ''
      form.value.parentId = props.editData.parentId || 0
    } else {
      form.value.name = ''
      form.value.parentId = 0
    }
  }
})

const folderList = computed(() => {
  const flatten = (nodes, level = 0) => {
    let list = []
    if (!nodes || !Array.isArray(nodes)) return list;

    nodes.forEach(node => {
      if (node.Type === 'folder' || node.type === 'folder') {
        const prefix = '　'.repeat(level) + (level > 0 ? '├─ ' : '')
        const catName = node.Name || node.name || node.Title || node.title || node.CategoryName || node.categoryName || '未命名分类'
        const catId = node.Id || node.id || node.CategoryId || node.categoryId

        list.push({ 
          id: catId, 
          name: prefix + catName,
          isDisabled: props.isEdit && catId === props.editData?.id
        })
      }
      
      const children = node.Children || node.children;
      if (children && children.length > 0) {
        list = list.concat(flatten(children, level + 1))
      }
    })
    return list
  }
  return flatten(props.treeData)
})

const closeModal = () => {
  emit('close')
}

// 🚀 新增：删除确认逻辑
const confirmDelete = () => {
  if (confirm(`确定要删除分类“${props.editData.name}”吗？\n注意：如果该分类下仍有子目录或文章，删除可能会失败。`)) {
    emit('delete', props.editData.id);
  }
}

const submitCategory = () => {
  if (!form.value.name.trim()) {
    alert("目录名称不能为空！")
    return
  }
  
  const payload = {
    Name: form.value.name.trim(),
    ParentId: form.value.parentId
  };

  if (props.isEdit) {
    payload.CategoryId = props.editData.id;
  }

  emit('submit', payload)
}
</script>

<style scoped>
.modal-overlay { 
  position: fixed; 
  top: 0; left: 0; right: 0; bottom: 0; 
  background: rgba(15, 23, 42, 0.4); 
  backdrop-filter: blur(4px); 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  z-index: 9999; 
}

.modal-content { 
  background: #ffffff; 
  width: 100%;
  max-width: 440px; 
  border-radius: 12px; 
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04); 
  overflow: hidden; 
  transform: translateY(-20px);
  animation: modal-fade-in 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes modal-fade-in {
  to { transform: translateY(0); opacity: 1; }
  from { transform: translateY(15px); opacity: 0; }
}

.modal-header { 
  padding: 18px 24px; 
  border-bottom: 1px solid #e2e8f0; 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  background: #f8fafc;
}

.modal-header h3 { 
  margin: 0; 
  font-size: 16px; 
  color: #0f172a; 
  font-weight: 700;
}

.close-btn { 
  background: none; 
  border: none; 
  font-size: 22px; 
  cursor: pointer; 
  color: #94a3b8; 
  line-height: 1;
  transition: color 0.2s;
}
.close-btn:hover { color: #0f172a; }

.modal-body { 
  padding: 24px; 
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
}

.form-group { 
  display: flex; 
  flex-direction: column; 
  gap: 8px; 
}

.form-group label { 
  font-size: 13px; 
  font-weight: 600; 
  color: #475569; 
}
.required { color: #ef4444; margin-left: 2px; }

.form-group input, 
.form-group select { 
  padding: 10px 12px; 
  border: 1px solid #cbd5e1; 
  border-radius: 8px; 
  font-size: 14px; 
  color: #334155;
  outline: none; 
  transition: all 0.2s;
  font-family: inherit;
}
.form-group input:focus, 
.form-group select:focus { 
  border-color: #3b82f6; 
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); 
}

.form-group select {
  font-family: "SFMono-Regular", Consolas, monospace;
}
.form-group select option:disabled {
  color: #cbd5e1;
  font-style: italic;
}

/* 🚀 底部布局调整 */
.modal-footer { 
  padding: 16px 24px; 
  background: #f8fafc; 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  border-top: 1px solid #e2e8f0; 
}

.footer-right {
  display: flex;
  gap: 12px;
}

.btn-cancel { 
  padding: 9px 18px; 
  border: 1px solid #cbd5e1; 
  background: white; 
  color: #475569;
  border-radius: 8px; 
  font-weight: 600;
  font-size: 14px;
  cursor: pointer; 
}
.btn-cancel:hover { background: #f1f5f9; color: #0f172a; }

.btn-submit { 
  padding: 9px 18px; 
  border: none; 
  background: #2563eb; 
  color: white; 
  border-radius: 8px; 
  font-weight: 600;
  font-size: 14px;
  cursor: pointer; 
}
.btn-submit:hover { background: #1d4ed8; }

/* 🚀 新增删除按钮样式 */
.btn-delete {
  background: transparent;
  border: none;
  color: #ef4444;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  padding: 8px 0;
  transition: opacity 0.2s;
}
.btn-delete:hover {
  opacity: 0.8;
  text-decoration: underline;
}
</style>