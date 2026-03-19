<template>
  <div class="minimal-uploader">
    <div class="status-bar">
      <div class="status-left">
        <span class="dot active"></span>
        <span class="text">影像档案上传系统中...</span>
      </div>
      <div class="status-right">SESSION: {{ sessionId }}</div>
    </div>

    <div class="uploader-body">
      <div class="preview-section">
        <div class="section-title">1. 上传视频文件</div>
        
        <div class="upload-card" :class="{ 'has-file': finalVideoUrl }">
          <div v-if="!isUploading && !finalVideoUrl" class="upload-trigger" @click="triggerFileSelect">
            <div class="icon-circle">↑</div>
            <p>点击或拖拽视频文件到这里</p>
            <span class="sub-text">支持 MP4, MOV 格式</span>
          </div>

          <div v-else-if="isUploading" class="uploading-state">
            <div class="progress-info">正在上传... {{ uploadPercent }}%</div>
            <div class="progress-track">
              <div class="progress-bar" :style="{ width: uploadPercent + '%' }"></div>
            </div>
          </div>

          <div v-else class="file-info-state">
            <div class="file-icon">Video</div>
            <div class="file-details">
              <p class="duration">解析时长: {{ form.duration }}</p>
              <button class="btn-text" @click="resetUpload">重新上传</button>
            </div>
          </div>
          <input type="file" ref="fileInput" hidden accept="video/*" @change="handleFileChange" />
        </div>

        <div class="section-title mt-30">2. 设置视频封面</div>
        <div class="cover-card" @click="triggerCoverSelect" :style="{ backgroundImage: `url(${form.coverUrl})` }">
          <div v-if="!form.coverUrl" class="cover-placeholder">
            <span>+ 选择封面图</span>
          </div>
          <div class="cover-overlay" v-else>更换封面</div>
        </div>
        <input type="file" ref="coverInput" hidden accept="image/*" @change="handleCoverChange" />
      </div>

      <div class="config-section">
        <div class="section-title">3. 基本信息</div>
        <div class="form-group">
          <label>视频标题</label>
          <input v-model="form.title" class="min-input" placeholder="请输入标题" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>内容形态</label>
            <select v-model="form.videoType" class="min-select">
              <option value="UGC">个人记录</option>
              <option value="SHORT">短视频</option>
              <option value="ANIME">番剧</option>
              <option value="MOVIE">电影</option>
              <option value="TV">电视剧</option>
            </select>
          </div>
          <div class="form-group" v-if="['ANIME', 'TV', 'MOVIE'].includes(form.videoType)">
            <label>系列 ID / 集数</label>
            <div class="flex-inputs">
              <input v-model.number="form.seriesId" type="number" class="min-input" placeholder="ID" />
              <input v-model.number="form.episodeNumber" type="number" class="min-input" placeholder="EP" />
            </div>
          </div>
        </div>

        <div class="form-group">
          <label>视频简介</label>
          <textarea v-model="form.description" class="min-textarea" placeholder="描述一下你的视频..." rows="3"></textarea>
        </div>

        <div class="section-title mt-20">4. 发布设置</div>
        <div class="setting-list">
          <div class="setting-item">
            <div class="setting-label">
              <strong>公开状态</strong>
              <span>所有人可见 / 仅自己可见</span>
            </div>
            <select v-model="form.visibility" class="min-select small">
              <option :value="0">公开</option>
              <option :value="1">私密</option>
            </select>
          </div>

          <div class="setting-item">
            <div class="setting-label">
              <strong>定时发布</strong>
            </div>
            <div class="switch-group">
              <input type="checkbox" v-model="isScheduled">
              <input v-if="isScheduled" type="datetime-local" v-model="form.scheduledAt" class="min-input small" />
            </div>
          </div>

          <div class="setting-item">
            <div class="setting-label"><strong>开启评论</strong></div>
            <input type="checkbox" v-model="form.allowComment" class="min-checkbox">
          </div>

          <div class="setting-item">
            <div class="setting-label"><strong>开启弹幕</strong></div>
            <input type="checkbox" v-model="form.allowDanmaku" class="min-checkbox">
          </div>
        </div>

        <button class="submit-button" :disabled="!isFormValid || isSubmitting" @click="submitArchive">
          {{ isSubmitting ? '提交中...' : '提交视频' }}
        </button>
      </div>
    </div>

    <video ref="tempVideo" hidden></video>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import axios from 'axios'
import apiClient from '@/utils/api'

// --- 状态定义 ---
const sessionId = ref(Math.random().toString(36).substring(7).toUpperCase())
const fileInput = ref(null)
const coverInput = ref(null)
const tempVideo = ref(null)
const isUploading = ref(false)
const isSubmitting = ref(false)
const isScheduled = ref(false)
const uploadPercent = ref(0)
const finalVideoUrl = ref('')

// 1. 定义初始状态模板，方便一键重置
const initialForm = {
  title: '',
  description: '',
  coverUrl: '',
  videoType: 'UGC',
  duration: '00:00',
  visibility: 0,
  allowComment: true,
  allowDanmaku: true,
  scheduledAt: '',
  seriesId: null,
  episodeNumber: null,
  channel: 'CREATION' // 默认频道
}

// 2. 使用 reactive 包装表单
const form = reactive({ ...initialForm })

// --- 核心逻辑：彻底重置系统 ---
const resetForm = () => {
  // ⚡ 物理清理：恢复表单所有字段
  Object.assign(form, initialForm)
  
  // ⚡ 状态清理：清空视频 URL 和上传进度
  finalVideoUrl.value = ''
  uploadPercent.value = 0
  isScheduled.value = false
  
  // ⚡ 关键：清空 HTML 原生文件输入框的值，否则无法连续选择同一个文件
  if (fileInput.value) fileInput.value.value = ''
  if (coverInput.value) coverInput.value.value = ''
  
  // ⚡ 更新会话 ID，开启新上传任务
  sessionId.value = Math.random().toString(36).substring(7).toUpperCase()
}

// --- 计算属性 ---
const isFormValid = computed(() => {
  return form.title.trim() !== '' && 
         finalVideoUrl.value !== '' && 
         form.coverUrl !== ''
})

// --- 交互辅助 ---
const triggerFileSelect = () => fileInput.value.click()
const triggerCoverSelect = () => coverInput.value.click()

const formatSeconds = (s) => {
  const min = Math.floor(s / 60)
  const sec = Math.floor(s % 60)
  return `${min.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`
}

// --- 视频文件处理 ---
const handleFileChange = async (e) => {
  const file = e.target.files[0]
  if (!file) return

  // 限制 100MB
  const maxSize = 100 * 1024 * 1024
  if (file.size > maxSize) {
    alert('档案过大！由于太初节点带宽限制，上传视频请控制在 100MB 以内。')
    fileInput.value.value = ''
    return
  }

  // 提取时长
  const url = URL.createObjectURL(file)
  tempVideo.value.src = url
  tempVideo.value.onloadedmetadata = () => {
    form.duration = formatSeconds(tempVideo.value.duration)
    URL.revokeObjectURL(url)
  }

  isUploading.value = true
  uploadPercent.value = 0

  try {
    // 1. 向后端申请上传预签名链接
    const { data } = await apiClient.get('/Video/apply-upload', { params: { fileName: file.name } })
    
    // 2. 直接推送到 COS 存储
    await axios.put(data.uploadUrl, file, {
      onUploadProgress: (p) => {
        uploadPercent.value = Math.round((p.loaded * 100) / p.total)
      }
    })
    
    finalVideoUrl.value = data.finalUrl
  } catch (err) {
    alert('视频上传失败，请检查网络链路')
    resetUpload()
  } finally {
    isUploading.value = false
  }
}

// --- 封面图片处理 ---
const handleCoverChange = async (e) => {
  const file = e.target.files[0]
  if (!file) return
  
  try {
    const { data } = await apiClient.get('/Video/apply-upload', { params: { fileName: file.name } })
    await axios.put(data.uploadUrl, file, { headers: { 'Content-Type': file.type } })
    form.coverUrl = data.finalUrl
  } catch (err) {
    alert('封面归档失败')
  }
}

const resetUpload = () => {
  finalVideoUrl.value = ''
  uploadPercent.value = 0
  if (fileInput.value) fileInput.value.value = ''
}

// --- 最终提交 ---
const submitArchive = async () => {
  if (isSubmitting.value) return
  isSubmitting.value = true

  try {
    const payload = { 
      ...form, 
      videoUrl: finalVideoUrl.value, 
      scheduledAt: isScheduled.value ? form.scheduledAt : null 
    }
    
    const res = await apiClient.post('/Video/record', payload)
    
    if (res.data.success) {
      alert('档案已成功归档至太初寰宇！系统即将重置以备下次任务。')
      // ⚡ 提交成功，立刻重置所有状态
      resetForm()
    }
  } catch (err) {
    alert('归档失败：与主控中心通讯中断')
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.minimal-uploader {
  background: #fff;
  color: #333;
  padding: 40px;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
}

/* 状态栏 */
.status-bar {
  display: flex; justify-content: space-between; border-bottom: 1px solid #eee; padding-bottom: 20px; margin-bottom: 30px; font-size: 13px; color: #888;
}
.status-left { display: flex; align-items: center; gap: 8px; }
.dot { width: 8px; height: 8px; background: #ccc; border-radius: 50%; }
.dot.active { background: #007AFF; box-shadow: 0 0 5px rgba(0, 122, 255, 0.5); }

/* 布局 */
.uploader-body { display: grid; grid-template-columns: 1fr 400px; gap: 60px; }
.section-title { font-size: 14px; font-weight: 600; color: #111; margin-bottom: 15px; }
.mt-30 { margin-top: 30px; }
.mt-20 { margin-top: 20px; }

/* 上传卡片 */
.upload-card {
  height: 200px; border: 1px dashed #ddd; border-radius: 12px; display: flex; align-items: center; justify-content: center; transition: 0.2s; cursor: pointer;
}
.upload-card:hover { border-color: #007AFF; background: #f9f9f9; }
.upload-trigger { text-align: center; }
.icon-circle { width: 40px; height: 40px; background: #eee; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 10px; font-weight: bold; }
.sub-text { font-size: 12px; color: #aaa; }

.progress-track { width: 200px; height: 4px; background: #eee; border-radius: 2px; margin-top: 10px; overflow: hidden; }
.progress-bar { height: 100%; background: #007AFF; transition: width 0.3s; }

/* 封面卡片 */
.cover-card {
  width: 100%; aspect-ratio: 16/9; background: #f5f5f5; border-radius: 8px; background-size: cover; background-position: center; cursor: pointer; position: relative; overflow: hidden;
}
.cover-placeholder { height: 100%; display: flex; align-items: center; justify-content: center; color: #888; font-size: 14px; }
.cover-overlay { position: absolute; inset: 0; background: rgba(0,0,0,0.3); color: #fff; display: flex; align-items: center; justify-content: center; opacity: 0; transition: 0.2s; }
.cover-card:hover .cover-overlay { opacity: 1; }

/* 表单控件 */
.form-group { margin-bottom: 20px; }
.form-group label { display: block; font-size: 12px; color: #666; margin-bottom: 8px; }
.min-input, .min-select, .min-textarea {
  width: 100%; border: 1px solid #eee; border-radius: 6px; padding: 10px; outline: none; transition: 0.2s; font-size: 14px; box-sizing: border-box;
}
.min-input:focus { border-color: #007AFF; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
.flex-inputs { display: flex; gap: 10px; }

/* 发布设置列表 */
.setting-list { background: #f9f9f9; border-radius: 10px; padding: 15px; }
.setting-item { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid #f0f0f0; }
.setting-item:last-child { border-bottom: none; }
.setting-label { display: flex; flex-direction: column; gap: 2px; }
.setting-label strong { font-size: 13px; }
.setting-label span { font-size: 11px; color: #999; }

.submit-button {
  width: 100%; background: #007AFF; color: #fff; border: none; border-radius: 8px; padding: 15px; font-weight: 600; margin-top: 30px; cursor: pointer; transition: 0.2s;
}
.submit-button:hover:not(:disabled) { background: #0056b3; }
.submit-button:disabled { opacity: 0.4; cursor: not-allowed; }
</style>