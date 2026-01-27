<template>
  <div class="link-preview-card">
    <div v-if="loading" class="loading-state">
      <div class="skeleton title-sk"></div>
      <div class="skeleton body-sk"></div>
      <div class="skeleton body-sk short"></div>
    </div>
    <div v-else-if="error" class="error-state">
      ⚠️ 无法加载内容
    </div>
    <div v-else class="preview-content">
      <div class="preview-header">
        <span class="icon">📄</span>
        <span class="title">{{ noteData.title }}</span>
      </div>
      <div class="preview-body">
        {{ previewText || '暂无内容...' }}
      </div>
      <div class="preview-footer">
        最后更新: {{ formatDate(noteData.updatedAt) }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps(['noteId'])
const loading = ref(true)
const error = ref(false)
const noteData = ref({})
const previewText = ref('')

const formatDate = (d) => new Date(d).toLocaleDateString()

// 简单的 JSON 转纯文本辅助函数
const extractTextFromJson = (contentJson) => {
  try {
    const json = JSON.parse(contentJson)
    if (!json || !json.content) return ''
    
    let text = ''
    // 简单遍历前几个 block 获取文本
    for (const block of json.content) {
      if (text.length > 100) break; // 最多取 100 字
      if (block.content) {
        block.content.forEach(span => {
          if (span.text) text += span.text
        })
        text += ' '
      }
    }
    return text.slice(0, 100) + (text.length > 100 ? '...' : '')
  } catch (e) {
    return ''
  }
}

onMounted(async () => {
  try {
    const res = await apiClient.get(`/Notes/detail/${props.noteId}`)
    noteData.value = res.data
    previewText.value = extractTextFromJson(res.data.ContentJson)
  } catch (e) {
    error.value = true
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.link-preview-card {
  width: 280px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  padding: 12px;
  border: 1px solid #eee;
  font-family: sans-serif;
  text-align: left;
}

.loading-state .skeleton {
  background: #f0f0f0;
  border-radius: 4px;
  margin-bottom: 8px;
  animation: pulse 1.5s infinite;
}
.title-sk { height: 20px; width: 60%; }
.body-sk { height: 14px; width: 100%; }
.body-sk.short { width: 80%; }

.preview-header {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 8px;
  border-bottom: 1px solid #f5f5f5;
  padding-bottom: 6px;
}
.preview-header .title {
  font-weight: 600;
  font-size: 14px;
  color: #333;
}

.preview-body {
  font-size: 12px;
  color: #666;
  line-height: 1.5;
  margin-bottom: 8px;
  min-height: 40px;
}

.preview-footer {
  font-size: 10px;
  color: #999;
  text-align: right;
}

@keyframes pulse {
  0% { opacity: 0.6; }
  50% { opacity: 1; }
  100% { opacity: 0.6; }
}
</style>