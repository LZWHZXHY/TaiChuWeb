<template>
  <form class="composer" @submit.prevent="submit">
    <div class="composer-body">
      <textarea
        v-model="text"
        :placeholder="placeholderText"
        class="reply-textarea"
        @focus="expanded = true"
        rows="4"
      ></textarea>

      <div v-if="expanded" class="composer-controls">
        <div class="controls-left">
          <input ref="fileInput" type="file" multiple accept="image/*,.gif" @change="onFileSelect" style="display:none" />
          <button type="button" class="btn btn-icon" @click="triggerFileSelect">
            <span class="icon">📎</span> 添加图片
          </button>
        </div>
        
        <div class="controls-right">
          <button type="button" class="btn btn-cancel" v-if="parentCommentId" @click="onCancel">取消</button>
          <button type="submit" class="btn btn-submit" :disabled="posting || !canSubmit">
            {{ posting ? '发布中…' : submitLabel }}
          </button>
        </div>
      </div>

      <div v-if="expanded && filePreviews.length" class="file-previews">
        <div v-for="(file, index) in filePreviews" :key="index" class="file-preview">
          
          <button type="button" class="btn-remove" @click="removeFile(index)">✕</button>
        </div>
      </div>
    </div>
  </form>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  postId: { type: [String, Number], required: true },
  parentCommentId: { type: [String, Number], default: null },
  placeholder: { type: String, default: '' }
})

const emit = defineEmits(['posted', 'cancel'])

const text = ref('')
const expanded = ref(false)
const posting = ref(false)
const selectedFiles = ref([])
const filePreviews = ref([])
const fileInput = ref(null)

// 自动展开回复表单
watch(() => props.parentCommentId, (newVal) => {
  expanded.value = newVal !== null && newVal !== undefined
}, { immediate: true })

const placeholderText = computed(() => {
  return props.placeholder || (props.parentCommentId ? '回复这条评论…' : '写下你的回复…')
})

const submitLabel = computed(() => {
  return props.parentCommentId ? '发布回复' : '发表评论'
})

const canSubmit = computed(() => {
  return text.value.trim().length > 0 || selectedFiles.value.length > 0
})

const triggerFileSelect = () => {
  fileInput.value?.click()
}

const onFileSelect = (event) => {
  const files = Array.from(event.target.files)
  selectedFiles.value = files
  
  // 生成预览
  filePreviews.value = files.map(file => ({
    file,
    preview: URL.createObjectURL(file)
  }))
  
  event.target.value = ''
}

const removeFile = (index) => {
  if (filePreviews.value[index]) {
    URL.revokeObjectURL(filePreviews.value[index].preview)
  }
  filePreviews.value.splice(index, 1)
  selectedFiles.value.splice(index, 1)
}

const onCancel = () => {
  text.value = ''
  selectedFiles.value = []
  filePreviews.value.forEach(file => URL.revokeObjectURL(file.preview))
  filePreviews.value = []
  expanded.value = false
  emit('cancel')
}

const submit = async () => {
  if (!props.postId) {
    alert('无法回复：帖子ID无效')
    return
  }
  
  if (!canSubmit.value) {
    alert('请输入内容或选择图片')
    return
  }

  posting.value = true
  
  try {
    const formData = new FormData()
    formData.append('Content', text.value.trim())
    
    if (props.parentCommentId) {
      formData.append('ParentCommentId', props.parentCommentId.toString())
    }
    
    selectedFiles.value.forEach(file => {
      formData.append('Images', file)
    })

    const response = await apiClient.post(`/posts/${props.postId}/reply`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    if (response.data?.success) {
      const newComment = response.data.data || {
        id: Date.now(),
        content: text.value.trim(),
        createTime: new Date().toISOString(),
        author: { username: '我' },
        images: selectedFiles.value.length > 0 ? selectedFiles.value : []
      }
      
      // 重置表单
      text.value = ''
      selectedFiles.value = []
      filePreviews.value.forEach(file => URL.revokeObjectURL(file.preview))
      filePreviews.value = []
      expanded.value = false
      
      // 发出posted事件，包含新回复的数据
      emit('posted', newComment)
    } else {
      throw new Error(response.data?.message || '发布失败')
    }
  } catch (error) {
    console.error('回复发布失败:', error)
    alert(`发布失败: ${error.response?.data?.message || error.message || '网络错误'}`)
  } finally {
    posting.value = false
  }
}
</script>

<style scoped>
.composer {
  background: var(--bg-card);
  border-radius: 12px;
  border: 1px solid var(--border-color);
  overflow: hidden;
}

.composer-body {
  padding: 16px;
}

.reply-textarea {
  width: 100%;
  min-height: 100px;
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  resize: vertical;
  font-family: inherit;
  font-size: 0.95rem;
  line-height: 1.5;
  transition: all 0.2s ease;
}

.reply-textarea:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(15, 163, 163, 0.1);
}

.reply-textarea:disabled {
  background-color: #f8fafc;
  cursor: not-allowed;
}

.composer-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
  gap: 12px;
}

.controls-left, .controls-right {
  display: flex;
  gap: 8px;
  align-items: center;
}

.btn {
  padding: 8px 16px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background: white;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn:hover:not(:disabled) {
  background: var(--bg-hover);
  transform: translateY(-1px);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-icon {
  background: transparent;
  border-color: transparent;
  padding: 6px 12px;
}

.btn-icon:hover {
  background: #f8fafc;
}

.btn-submit {
  background: var(--accent);
  color: rgb(0, 0, 0);
  border-color: var(--accent);
  font-weight: 600;
}

.btn-submit:hover:not(:disabled) {
  background: #0d9191;
  border-color: #0d9191;
  transform: translateY(-1px);
}

.btn-cancel {
  background: #f8fafc;
  color: #64748b;
  border-color: #e2e8f0;
}

.btn-cancel:hover {
  background: #f1f5f9;
  border-color: #cbd5e0;
}

.file-previews {
  display: flex;
  gap: 8px;
  margin-top: 12px;
  flex-wrap: wrap;
}

.file-preview {
  position: relative;
  width: 80px;
  height: 80px;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid var(--border-color);
  transition: transform 0.2s ease;
}

.file-preview:hover {
  transform: scale(1.05);
}

.preview-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.btn-remove {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: none;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.file-preview:hover .btn-remove {
  opacity: 1;
}

.btn-remove:hover {
  background: rgba(0, 0, 0, 0.9);
}

@media (max-width: 768px) {
  .composer-controls {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }
  
  .controls-left, .controls-right {
    justify-content: space-between;
  }
  
  .btn {
    padding: 10px 16px;
    flex: 1;
    justify-content: center;
  }
}
</style>