<template>
  <form class="post-form" @submit.prevent="submit">
    <div class="form-header">
      <h3 class="form-title">发表新帖子</h3>
    </div>

    <div class="form-row">
      <input 
        v-model="title" 
        class="title-input" 
        type="text" 
        placeholder="帖子标题（必填）" 
        maxlength="100"
      />
    </div>

    <div class="form-row">
      <select v-model="postType" class="type-select">
        <option value="0">🐕 柴圈帖子</option>
        <option value="1">🎮 游戏帖子</option>
        <option value="2">💬 其他帖子</option>
      </select>
    </div>

    <div class="form-row">
      <textarea
        v-model="content"
        placeholder="写下你想分享的内容…（支持Markdown格式）"
        class="content-textarea"
        rows="4"
      ></textarea>
    </div>

    <!-- 图片上传区域 -->
    <div class="uploader-section">
      <div class="uploader-header">
        <span class="uploader-label">添加图片（最多6张）</span>
      </div>
      <ImageUploader 
        ref="uploader" 
        :max-files="6" 
        :max-size="6 * 1024 * 1024" 
        @change="onImagesChange" 
      />
    </div>

    <div class="form-actions">
      <button 
        type="submit" 
        class="btn btn-submit" 
        :disabled="posting || !canSubmit"
      >
        {{ posting ? '发布中…' : '发布帖子' }}
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, computed } from 'vue'
import apiClient from '@/utils/api'
import ImageUploader from './ImageUploader.vue'

const emit = defineEmits(['posted'])

// 响应式数据
const title = ref('')
const content = ref('')
const postType = ref('0')
const posting = ref(false)
const files = ref([])
const uploader = ref(null)

// 计算属性
const canSubmit = computed(() => {
  if (posting.value) return false
  const hasTitle = title.value.trim().length > 0
  const hasContent = content.value.trim().length > 0
  const hasFiles = files.value.length > 0
  
  return hasTitle && (hasContent || hasFiles)
})

// 方法
const onImagesChange = (fileArray) => { 
  files.value = Array.isArray(fileArray) ? fileArray : [] 
}

const submit = async () => {
  if (!canSubmit.value) {
    alert('请输入标题和内容或添加图片')
    return
  }

  posting.value = true

  try {
    const formData = new FormData()
    formData.append('Title', title.value.trim())
    formData.append('Content', content.value.trim())
    formData.append('PostType', postType.value)
    
    files.value.forEach(file => {
      formData.append('Images', file)
    })

    const response = await apiClient.post('/posts/create', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    if (response.status === 201 || response.data?.success) {
      const newPost = response.data?.data || response.data || {}
      
      // 重置表单
      title.value = ''
      content.value = ''
      postType.value = '0'
      files.value = []
      
      // 重置上传器
      try { 
        uploader.value?.reset?.() 
      } catch {}
      
      emit('posted', newPost)
      alert('帖子发布成功！')
    } else {
      throw new Error(response.data?.message || '发布失败')
    }
  } catch (error) {
    console.error('发帖失败:', error)
    const errorMessage = error.response?.data?.message || error.message || '网络错误'
    alert(`发布失败：${errorMessage}`)
  } finally {
    posting.value = false
  }
}
</script>

<style scoped>
.post-form {
  width: 100%;
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.form-header {
  margin-bottom: 16px;
}

.form-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1a202c;
}

.form-row {
  margin-bottom: 16px;
}

.title-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 1rem;
  background: #ffffff;
  transition: border-color 0.2s ease;
}

.title-input:focus {
  outline: none;
  border-color: #0fa3a3;
  box-shadow: 0 0 0 3px rgba(15, 163, 163, 0.1);
}

.type-select {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #ffffff;
  font-size: 0.9rem;
  cursor: pointer;
}

.content-textarea {
  width: 100%;
  min-height: 120px;
  padding: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  line-height: 1.5;
  background: #ffffff;
  resize: vertical;
  transition: border-color 0.2s ease;
  font-family: inherit;
}

.content-textarea:focus {
  outline: none;
  border-color: #0fa3a3;
  box-shadow: 0 0 0 3px rgba(15, 163, 163, 0.1);
}

.uploader-section {
  margin: 16px 0;
}

.uploader-header {
  margin-bottom: 8px;
}

.uploader-label {
  font-size: 0.9rem;
  color: #64748b;
  font-weight: 500;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.btn {
  padding: 12px 24px;
  border: 1px solid;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-submit {
  background: #0fa3a3;
  border-color: #0fa3a3;
  color: #ffffff;
  min-width: 120px;
  justify-content: center;
}

.btn-submit:hover:not(:disabled) {
  background: #0d9191;
  border-color: #0d9191;
  transform: translateY(-1px);
}

@media (max-width: 768px) {
  .post-form {
    padding: 16px;
  }
}
</style>