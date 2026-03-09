<template>
  <div class="cyber-uploader-panel">
    <div class="panel-header">
      <span class="title">>> DEEP_SPACE_RADIO // 弹药自动装填终端</span>
    </div>

    <div class="form-group">
      <label>1. 选择音频文件 (MP3 / WAV)</label>
      <input type="file" accept="audio/mp3, audio/wav, audio/mpeg" @change="handleFileSelect" class="cyber-file-input">
      <div v-if="duration > 0" class="duration-tag">
        [SYS_OK] 音频解析成功: {{ duration }} 秒 | 准备就绪
      </div>
    </div>

    <div class="form-group">
      <label>2. 歌曲元数据</label>
      <input v-model="formData.title" type="text" placeholder="TRACK_TITLE // 歌名 (默认使用文件名)" class="cyber-input">
      <input v-model="formData.artist" type="text" placeholder="ARTIST // 创作者 (选填)" class="cyber-input">
    </div>

    <div class="form-group">
      <label>3. 物理存储链路 (系统自动分配)</label>
      <div class="cyber-status-bar" :class="{ 'has-url': formData.url }">
        {{ formData.url ? formData.url : '[ WAITING_FOR_UPLOAD // 等待上传分配... ]' }}
      </div>
    </div>

    <button 
      class="cyber-btn-deploy" 
      :disabled="isDeploying || !isValid" 
      @click="deployToRadio"
    >
      {{ deployStatusText }}
    </button>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import apiClient from '@/utils/api'

// 表单响应式数据
const formData = ref({
  title: '',
  artist: '',
  url: ''
})

const duration = ref(0)
const selectedFile = ref(null)
const isDeploying = ref(false)
const deployStatusText = ref('[ DEPLOY_TRACK // 自动上传并全网发射 ]')

// 验证逻辑：只需要选了文件且解析出时长即可
const isValid = computed(() => {
  return selectedFile.value && duration.value > 0 && formData.value.title
})

// --- 核心 1：选择文件并解析时长 ---
const handleFileSelect = (event) => {
  const file = event.target.files[0]
  if (!file) return

  selectedFile.value = file
  formData.value.url = '' // 切换文件时清空之前的链接
  
  if (!formData.value.title) {
    formData.value.title = file.name.replace(/\.[^/.]+$/, "")
  }

  const objectUrl = URL.createObjectURL(file)
  const audio = new Audio(objectUrl)
  
  audio.onloadedmetadata = () => {
    duration.value = Math.round(audio.duration)
    URL.revokeObjectURL(objectUrl)
    console.log(`>> [AUDIO_SYS] 弹药解析成功: ${duration.value}s`)
  }
}

// --- 核心 2：先传物理文件，成功后再注册元数据 ---
const deployToRadio = async () => {
  if (!selectedFile.value) return
  isDeploying.value = true

  try {
    // 【第一阶段：上传实体文件至腾讯云】
    deployStatusText.value = 'UPLOADING_AUDIO_DATA... // 音频跃迁中'
    
    const uploadData = new FormData()
    uploadData.append('file', selectedFile.value)

    // ⚡ 指向我们在 SongController 中新加的接口
    const uploadRes = await apiClient.post('/Song/upload', uploadData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })

    if (!uploadRes.data || !uploadRes.data.url) {
      throw new Error("文件上传失败，未能获取到云端链接")
    }

    // 拿到后端从 MusicCosHelper 返回的真实 URL
    formData.value.url = uploadRes.data.url

    // 【第二阶段：将元数据与 URL 绑定并注册到电台】
    deployStatusText.value = 'SYNCING_WITH_RADIO_NETWORK... // 并入电台网络'
    
    const payload = {
      title: formData.value.title,
      artist: formData.value.artist || 'SYSTEM',
      url: formData.value.url,
      durationSeconds: duration.value // ⚡ 关键：把前端解析的秒数传给后端打碟机
    }

    const radioRes = await apiClient.post('/Song/add', payload)

    if (radioRes.data && radioRes.data.success) {
      alert('🚀 弹药装填成功！已全自动并入深空电台网络！')
      
      // 重置所有状态
      formData.value = { title: '', artist: '', url: '' }
      duration.value = 0
      selectedFile.value = null
      // 清空 file input
      const fileInput = document.querySelector('.cyber-file-input')
      if (fileInput) fileInput.value = ''
    } else {
      alert(`发射失败: ${radioRes.data?.message || '未知错误'}`)
    }

  } catch (error) {
    console.error("部署失败:", error)
    alert(`系统故障: ${error.message || '上传接口响应异常'}`)
  } finally {
    isDeploying.value = false
    deployStatusText.value = '[ DEPLOY_TRACK // 自动上传并全网发射 ]'
  }
}
</script>

<style scoped>
.cyber-uploader-panel { 
  background: var(--bg-color, #F4F1EA); 
  border: 2px solid var(--ink-black, #111); 
  padding: 25px; 
  max-width: 600px; 
  font-family: 'JetBrains Mono', monospace; 
  box-shadow: 8px 8px 0 rgba(0,0,0,0.15); 
  color: var(--ink-black, #111); 
}
.panel-header { border-bottom: 2px solid var(--cyber-red, #D92323); padding-bottom: 10px; margin-bottom: 25px; }
.title { font-weight: bold; font-size: 16px; color: var(--cyber-red, #D92323); }
.form-group { margin-bottom: 20px; display: flex; flex-direction: column; gap: 8px; }
label { font-size: 12px; font-weight: bold; color: #555; border-left: 3px solid var(--ink-black, #111); padding-left: 8px; }
.cyber-input, .cyber-file-input { background: transparent; border: 1px solid #ccc; padding: 10px; font-family: inherit; font-size: 12px; transition: all 0.2s ease; }
.cyber-input:focus { outline: none; border-color: var(--ink-black, #111); background: rgba(0,0,0,0.03); }
.duration-tag { font-size: 11px; color: #00AA00; font-weight: bold; margin-top: 4px; }

.cyber-status-bar {
  background: #eee;
  border: 1px dashed #999;
  padding: 10px;
  font-size: 10px;
  color: #888;
  word-break: break-all;
  min-height: 32px;
  display: flex;
  align-items: center;
}
.cyber-status-bar.has-url {
  background: #eaffea;
  border-color: #00AA00;
  color: #00AA00;
  font-weight: bold;
}

.cyber-btn-deploy { 
  width: 100%; 
  padding: 15px; 
  margin-top: 10px; 
  background: var(--ink-black, #111); 
  color: #fff; 
  border: none; 
  font-family: inherit; 
  font-size: 14px; 
  font-weight: bold; 
  cursor: pointer; 
  transition: all 0.2s ease; 
  letter-spacing: 1px; 
}
.cyber-btn-deploy:hover:not(:disabled) { background: var(--cyber-red, #D92323); transform: translateY(-2px); box-shadow: 4px 4px 0 rgba(0,0,0,0.2); }
.cyber-btn-deploy:disabled { background: #ccc; color: #888; cursor: not-allowed; }
</style>