<template>
  <div class="post-form">
    <h3 class="form-title"><i class="fas fa-edit"></i> 发布新帖</h3>

    <div class="form-group">
      <label>帖子标题 *</label>
      <input v-model="title" class="form-input" maxlength="50" />
      <div class="char-counter">{{ title.length }}/50</div>
    </div>

    <div class="form-group">
      <label>帖子内容 *</label>
      <textarea v-model="content" class="form-textarea" maxlength="2000" rows="4"></textarea>
      <div class="char-counter">{{ content.length }}/2000</div>
    </div>

    <div class="form-group">
      <label>上传图片（最多10张）</label>
      <image-uploader ref="uploader" :max-files="10" :max-size="10*1024*1024" @change="onImagesChange" accept="image/*,.gif" />
    </div>

    <div class="form-group">
      <label>帖子类型</label>
      <select v-model="postType" class="form-select">
        <option :value="0">柴圈帖子</option>
        <option :value="1">游戏帖子</option>
        <option :value="2">其他帖子</option>
      </select>
    </div>

    <div class="form-actions">
      <button class="btn btn-primary" @click="submit" :disabled="submitting || !title.trim() || !content.trim()">
        {{ submitting ? '发布中...' : '发布' }}
      </button>
      <button class="btn btn-secondary" @click="reset" :disabled="submitting">重置</button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/utils/api'
import ImageUploader from './ImageUploader.vue'


const emit = defineEmits(['posted'])
const title = ref('')
const content = ref('')
const postType = ref(0)
const selectedFiles = ref([])
const submitting = ref(false)

const onImagesChange = (files) => selectedFiles.value = files

const submit = async () => {
  if (!title.value.trim() || !content.value.trim()) {
    alert('标题和内容为必填')
    return
  }
  submitting.value = true
  try {
    const fd = new FormData()
    fd.append('Title', title.value)
    fd.append('Content', content.value)
    fd.append('PostType', String(postType.value))
    selectedFiles.value.forEach(f => fd.append('Images', f))
    // do not set Content-Type header
    const resp = await apiClient.post('/posts/create', fd)
    if (resp.data && resp.data.success) {
      alert('发布成功')
      reset()
      emit('posted')
    } else {
      alert('发布失败: ' + (resp.data?.message || '未知错误'))
    }
  } catch (err) {
    console.error('submit post error', err)
    alert('发布失败，请重试')
  } finally {
    submitting.value = false
  }
}

const reset = () => {
  title.value = ''
  content.value = ''
  postType.value = 0
  selectedFiles.value = []
  // reset uploader
  const uploader = $refs?.uploader
  if (uploader?.reset) uploader.reset()
}
</script>

<style scoped>
/* 变量（作用域内） */
.post-form {
  --bg: #ffffff;
  --border: #eef2f6;
  --muted: #6b7280;
  --accent: 104, 109, 255; /* 用于半透明 focus/hover：rgb(... ) */
  --primary-grad-from: #667eea;
  --primary-grad-to: #764ba2;
  --radius: 12px;

  background: var(--bg);
  padding: 20px;
  border-radius: var(--radius);
  border: 1px solid var(--border);
  box-shadow: 0 6px 20px rgba(16, 24, 40, 0.04);
  max-width: 900px;
  margin: 0 auto;
  transition: box-shadow 160ms ease, transform 160ms ease;
}

/* 标题 */
.form-title {
  margin: 0 0 16px;
  font-weight: 700;
  font-size: 1.05rem;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #0f172a;
}
.form-title i {
  color: rgba(var(--accent), 1);
  font-size: 1.05rem;
  width: 20px;
  text-align: center;
}

/* 表单行 */
.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 14px;
}

/* 标签 */
.form-group > label {
  font-size: 0.95rem;
  color: #0f172a;
  font-weight: 600;
}

/* 输入控件通用样式 */
.form-input,
.form-textarea,
.form-select {
  width: 100%;
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid #e6edf3;
  background: #fbfdff;
  font-size: 0.96rem;
  color: #0b1220;
  transition: box-shadow 140ms ease, border-color 140ms ease, transform 120ms ease;
  outline: none;
  -webkit-appearance: none;
  -moz-appearance: none;
  appearance: none;
  box-sizing: border-box;
}

/* textarea 微调 */
.form-textarea {
  min-height: 110px;
  resize: vertical;
  line-height: 1.6;
}

/* 焦点态：清晰可见且不破坏布局 */
.form-input:focus,
.form-textarea:focus,
.form-select:focus {
  border-color: rgba(var(--accent), 0.85);
  box-shadow: 0 6px 18px rgba(99, 102, 241, 0.08), 0 0 0 4px rgba(var(--accent), 0.08);
}

/* 占位/提示计数器 */
.char-counter {
  align-self: flex-end;
  font-size: 0.82rem;
  color: var(--muted);
  margin-top: 4px;
}

/* 图片上传（假设 image-uploader 渲染自身 UI）——提供占位样式以提高一致性 */
.form-group image-uploader,
.form-group .image-uploader {
  display: block;
  width: 100%;
  min-height: 86px;
  padding: 10px;
  border: 1px dashed #e6edf3;
  border-radius: 10px;
  background: linear-gradient(180deg, rgba(250,252,255,0.6), rgba(248,250,255,0.4));
  box-sizing: border-box;
}

/* 操作按钮容器 */
.form-actions {
  display: flex;
  gap: 10px;
  margin-top: 16px;
  justify-content: flex-end;
  align-items: center;
  flex-wrap: wrap;
}

/* 按钮基类 */
.btn {
  padding: 10px 16px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.95rem;
  transition: transform 120ms ease, box-shadow 120ms ease, opacity 120ms ease;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #fff;
}

/* 主按钮（渐变）*/
.btn-primary {
  background: linear-gradient(135deg, var(--primary-grad-from), var(--primary-grad-to));
  box-shadow: 0 8px 22px rgba(118, 75, 162, 0.12);
}
.btn-primary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 12px 30px rgba(118, 75, 162, 0.14);
}
.btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

/* 次要按钮 */
.btn-secondary {
  background: #f8f9fb;
  color: #0b1220;
  border: 1px solid #e6edf3;
  box-shadow: none;
}
.btn-secondary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 8px 18px rgba(12, 18, 28, 0.04);
}

/* 禁用态 */
.btn:disabled,
.btn[disabled] {
  opacity: 0.56;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
  pointer-events: none;
}

/* 更紧凑的移动端布局 */
@media (max-width: 640px) {
  .post-form {
    padding: 14px;
    border-radius: 10px;
  }
  .form-title { font-size: 1rem; gap: 6px; }
  .form-actions { justify-content: stretch; }
  .btn { width: 100%; justify-content: center; }
  .btn + .btn { margin-top: 8px; }
  .char-counter { font-size: 0.78rem; }
}

/* 可选：悬停聚焦辅助（增强可访问性）*/
.form-input:focus-visible,
.form-textarea:focus-visible,
.form-select:focus-visible {
  outline: 3px solid rgba(var(--accent), 0.06);
  outline-offset: 2px;
}
</style>