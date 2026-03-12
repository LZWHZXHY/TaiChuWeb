<template>
  <form @submit.prevent="handleSubmit" class="cyber-form post-theme">
    <div class="form-decorative-bar"></div>

    <div class="form-group">
      <label><span class="label-decor"></span>数据代号 <span class="dim-text">// TITLE</span></label>
      <input type="text" v-model="formData.title" placeholder="输入帖子标题..." required />
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>核心数据 <span class="dim-text">// CONTENT</span></label>
      <div class="textarea-wrapper">
        <textarea 
          v-model="formData.content" 
          rows="5" 
          maxlength="500"
          placeholder="分享你的最新动态或想法..."
        ></textarea>
        <span class="char-counter" :class="{ limit: formData.content.length >= 500 }">
          {{ formData.content.length }} / 500
        </span>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>影像载入 <span class="dim-text">// UPLOAD IMAGES</span></label>
      <div class="file-drop-zone">
        <input type="file" @change="handleFileUpload" multiple accept="image/*" title="" />
        <div class="drop-text">
          <span class="icon">📸</span> 
          <span>点击或拖拽图像文件至此区域 <br><small class="limit-hint">(最多 5 张，单图限 5MB)</small></span>
        </div>
      </div>
      
      <div class="image-preview-matrix" v-if="previewUrls.length > 0">
        <div class="preview-node" v-for="(url, index) in previewUrls" :key="index">
          <img :src="url" alt="preview" />
          <div class="delete-btn" @click="removeFile(index)">✖</div>
        </div>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>特征标签 <span class="dim-text">// TAGS</span></label>
      <CyberTagInput v-model="formData.tags" :max-tags="5" />
    </div>

    <div class="progress-section" v-if="isSubmitting && uploadProgress > 0">
      <div class="progress-meta">
        <span class="glitch-text">TRANSMITTING_DATA...</span>
        <span class="percent-text">{{ uploadProgress }}%</span>
      </div>
      <div class="cyber-progress-bar">
        <div class="progress-fill" :style="{ width: uploadProgress + '%' }">
          <div class="scanline"></div>
        </div>
      </div>
    </div>

    <div class="form-actions">
      <button type="submit" class="cyber-submit-btn" :disabled="isSubmitting">
        <span class="btn-decor"></span>
        <span v-if="!isSubmitting">BROADCAST // 广播发射</span>
        <span v-else class="glitch-text">DATA_FLOWING...</span>
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount } from 'vue'
import apiClient from '@/utils/api' 
import axios from 'axios' // 🔥 必须引入原生 axios，用于绕过 apiClient 的 Token 拦截器直传 COS
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'

const isSubmitting = ref(false)
const selectedFiles = ref([])
const previewUrls = ref([])
const uploadProgress = ref(0) // 0 到 100 的数值
const formData = reactive({ title: '', content: '', tags: '' })

// 1. 处理文件选择与本地预览
const handleFileUpload = (event) => {
  const files = Array.from(event.target.files)
  
  // 限制 5 张
  if (selectedFiles.value.length + files.length > 5) {
    alert('系统拦截：单个帖子最多只能承载 5 张影像数据')
    return
  }

  // 限制单图 5MB
  const maxSize = 5 * 1024 * 1024;
  for (let file of files) {
    if (file.size > maxSize) {
      alert(`协议拒绝：影像 [${file.name}] 体积过大，上限为 5MB`);
      return;
    }
  }

  files.forEach(file => {
    selectedFiles.value.push(file)
    previewUrls.value.push(URL.createObjectURL(file))
  })
  
  event.target.value = '' // 清空 input 以便重复选择同名文件
}

// 2. 移除单张预览
const removeFile = (index) => {
  URL.revokeObjectURL(previewUrls.value[index]) 
  selectedFiles.value.splice(index, 1)
  previewUrls.value.splice(index, 1)
}

// 3. 提交逻辑
const handleSubmit = async () => {
  if (!formData.title) return alert('核心缺失：必须提供数据代号 (Title)')
  
  isSubmitting.value = true
  uploadProgress.value = 0
  
  try {
    let finalImageUrls = []

    // --- 第一阶段：多图并发直传腾讯云 ---
    if (selectedFiles.value.length > 0) {
      // (1) 向后端申请 URL 通行证
      const fileNames = selectedFiles.value.map(f => f.name)
      const authResponse = await apiClient.post('/ThePost/get-upload-urls', { fileNames })
      
      if (!authResponse.data.success) throw new Error('无法获取云端传输凭证')
      const credentials = authResponse.data.data

      // (2) 准备进度计算：计算所有文件的总字节数
      const totalSize = selectedFiles.value.reduce((acc, file) => acc + file.size, 0)
      const loadedBytesArr = new Array(selectedFiles.value.length).fill(0)

      // (3) 并发上传
      const uploadTasks = selectedFiles.value.map(async (file, index) => {
        const { uploadUrl, finalUrl } = credentials[index]
        
        await axios.put(uploadUrl, file, {
          headers: { 'Content-Type': file.type },
          onUploadProgress: (p) => {
            loadedBytesArr[index] = p.loaded // 更新当前文件的已传字节
            const currentTotalLoaded = loadedBytesArr.reduce((a, b) => a + b, 0)
            // 计算总百分比
            uploadProgress.value = Math.round((currentTotalLoaded / totalSize) * 100)
          }
        })
        return finalUrl
      })

      finalImageUrls = await Promise.all(uploadTasks)
    }

    // --- 第二阶段：向后端发送最终入库请求 (JSON 格式) ---
    const payload = {
      Title: formData.title.trim(),
      Content: formData.content.trim(),
      Tags: formData.tags,
      PostType: 0,
      ImageUrls: finalImageUrls // 传递干净的 CDN 地址列表
    }

    const response = await apiClient.post('/ThePost/create', payload)

    if (response.data && response.data.success) {
      alert(`POST 广播发射成功！`)
      resetForm()
    } else {
      throw new Error(response.data?.message || '后端入库失败')
    }

  } catch (error) {
    console.error('传输异常:', error)
    alert(`发射失败: ${error.message || '中枢响应异常'}`)
  } finally {
    isSubmitting.value = false
    uploadProgress.value = 0
  }
}

const resetForm = () => {
  formData.title = ''
  formData.content = ''
  formData.tags = ''
  selectedFiles.value = []
  previewUrls.value.forEach(url => URL.revokeObjectURL(url))
  previewUrls.value = []
}

onBeforeUnmount(() => {
  previewUrls.value.forEach(url => URL.revokeObjectURL(url))
})
</script>

<style scoped>
/* POST 模块专属变量：赛博蓝 */
.post-theme {
  --theme-color: #00f3ff;
  --theme-color-rgb: 0, 243, 255;
  --bg-dark: #111;
  --border-dim: #ddd;
}

.cyber-form { display: flex; flex-direction: column; gap: 24px; padding-top: 10px; position: relative; }

/* 顶部装饰条 */
.form-decorative-bar {
  position: absolute; top: -10px; left: 0; width: 60px; height: 4px;
  background: var(--theme-color); box-shadow: 0 0 10px rgba(var(--theme-color-rgb), 0.5);
}

.form-group { display: flex; flex-direction: column; gap: 10px; }
.form-group label { display: flex; align-items: center; gap: 8px; font-weight: 800; font-size: 0.85rem; color: #111; letter-spacing: 1px; }
.label-decor { display: inline-block; width: 6px; height: 6px; background: #111; transform: rotate(45deg); }
.dim-text { color: #888; font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; font-weight: normal; }

/* 输入框与文本域 */
.cyber-form input[type="text"], .cyber-form textarea {
  width: 100%; padding: 14px 16px; border: 1px solid var(--border-dim); background: #fdfdfd; font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: all 0.3s ease; outline: none; border-left: 4px solid var(--border-dim);
}
.cyber-form input[type="text"]:focus, .cyber-form textarea:focus { border-left-color: var(--theme-color); border-color: var(--bg-dark); box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.2); }

.textarea-wrapper { position: relative; }
.char-counter { position: absolute; bottom: 10px; right: 10px; font-size: 0.75rem; color: #aaa; background: rgba(255,255,255,0.9); padding: 2px 6px; pointer-events: none; }
.char-counter.limit { color: #ff2a6d; font-weight: bold; }

/* 文件拖拽区 */
.file-drop-zone { 
  position: relative; border: 1px dashed #bbb; background-color: #f9f9f9; padding: 35px 20px; text-align: center; cursor: pointer; transition: 0.3s; background-image: radial-gradient(#e0e0e0 1px, transparent 1px); background-size: 15px 15px; 
}
.file-drop-zone:hover { border-color: var(--theme-color); background-color: #f0fcff; }
.file-drop-zone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; width: 100%; }
.drop-text { font-weight: bold; color: #555; display: flex; flex-direction: column; align-items: center; gap: 8px; }
.limit-hint { font-size: 0.75rem; color: #999; }

/* 预览矩阵 */
.image-preview-matrix { display: grid; grid-template-columns: repeat(auto-fill, minmax(90px, 1fr)); gap: 12px; margin-top: 15px; }
.preview-node { position: relative; aspect-ratio: 1; background: #eee; clip-path: polygon(0 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%); border-bottom: 2px solid var(--theme-color); }
.preview-node img { width: 100%; height: 100%; object-fit: cover; }
.delete-btn { position: absolute; top: 0; right: 0; background: rgba(17,17,17,0.85); color: #fff; width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; cursor: pointer; }

/* 🚀 进度条样式 */
.progress-section { margin-top: 5px; width: 100%; }
.progress-meta { display: flex; justify-content: space-between; margin-bottom: 6px; font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; color: var(--theme-color); text-shadow: 0 0 8px rgba(var(--theme-color-rgb), 0.6); }
.cyber-progress-bar { height: 6px; background: rgba(0, 0, 0, 0.2); border: 1px solid rgba(var(--theme-color-rgb), 0.3); position: relative; overflow: hidden; }
.progress-fill { height: 100%; background: var(--theme-color); box-shadow: 0 0 15px var(--theme-color); transition: width 0.3s ease; position: relative; }
.scanline { position: absolute; top: 0; left: 0; bottom: 0; right: 0; background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent); animation: scan 1.5s infinite; }

@keyframes scan { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

/* 提交按钮 */
.form-actions { margin-top: 20px; display: flex; justify-content: flex-end; }
.cyber-submit-btn {
  position: relative; background: var(--bg-dark); color: #fff; border: none; padding: 16px 45px; font-family: 'Anton'; font-size: 1.2rem; cursor: pointer; transition: 0.2s; letter-spacing: 2px; clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%);
}
.cyber-submit-btn:hover:not(:disabled) { background: var(--theme-color); color: #000; box-shadow: 0 0 20px rgba(var(--theme-color-rgb), 0.4); }
.btn-decor { position: absolute; bottom: 5px; right: 5px; width: 6px; height: 6px; background: rgba(255,255,255,0.2); }

@keyframes glitch { 0% { opacity: 1; transform: translateX(0); } 50% { opacity: 0.9; transform: translateX(1px); } 100% { opacity: 1; } }
.glitch-text { display: inline-block; animation: glitch 0.3s infinite; }
</style>