<template>
  <section class="reply-form-section">
    <h4>发表评论</h4>
    <textarea v-model="content" class="form-textarea" rows="3" placeholder="写下你的回复..."></textarea>

    <div class="reply-image-uploader">
      <image-uploader ref="uploader" :max-files="10" :max-size="10*1024*1024" accept="image/*,.gif" @change="onFilesChanged" />
    </div>

    <div class="form-actions" style="margin-top:12px;">
      <button class="btn btn-primary" @click="submit" :disabled="posting || !content.trim()">{{ posting ? '发布中...' : '发布回复' }}</button>
      <button class="btn btn-secondary" @click="reset" :disabled="posting">重置</button>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/utils/api'
import ImageUploader from './ImageUploader.vue'

const props = defineProps({
  postId: { type: [String, Number], required: true }
})
const emit = defineEmits(['posted'])

const content = ref('')
const files = ref([])
const posting = ref(false)

const onFilesChanged = (f) => files.value = f

const submit = async () => {
  if (!content.value.trim()) { alert('请输入回复内容'); return }
  posting.value = true
  try {
    const fd = new FormData()
    fd.append('Content', content.value.trim())
    files.value.forEach(f => fd.append('Images', f))
    const resp = await apiClient.post(`/posts/${props.postId}/reply`, fd)
    if (resp.data && resp.data.success) {
      alert('回复已发布')
      content.value = ''
      files.value = []
      const uploader = $refs?.uploader
      if (uploader?.reset) uploader.reset()
      emit('posted')
    } else {
      alert('发布失败: ' + (resp.data?.message || '未知错误'))
    }
  } catch (e) {
    console.error('submit reply error', e)
    alert('发布回复失败')
  } finally {
    posting.value = false
  }
}

const reset = () => {
  content.value = ''
  files.value = []
  const uploader = $refs?.uploader
  if (uploader?.reset) uploader.reset()
}
</script>

<style scoped>
.reply-form-section { margin-top:16px; border-top:1px dashed #eee; padding-top:12px }
.form-textarea { width:100%; min-height:80px; padding:8px; border-radius:6px; border:1px solid #e9ecef }
.form-actions { display:flex; gap:8px; }
</style>