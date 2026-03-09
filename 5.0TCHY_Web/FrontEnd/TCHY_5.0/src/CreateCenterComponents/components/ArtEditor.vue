<template>
  <form @submit.prevent="handleSubmit" class="cyber-form art-theme">
    <div class="form-decorative-bar"></div>

    <div class="form-group">
      <label><span class="label-decor"></span>作品代号 <span class="dim-text">// ARTWORK TITLE</span></label>
      <input type="text" v-model="formData.title" placeholder="输入作品名称 (留空则默认使用图片名)..." />
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>创作者 <span class="dim-text">// AUTHOR</span></label>
      <input type="text" v-model="formData.author" placeholder="输入作者名 (留空则默认为当前用户)..." />
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>补充描述 <span class="dim-text">// DESCRIPTION</span></label>
      <textarea v-model="formData.desc" rows="3" placeholder="作品背后的故事、创作灵感..."></textarea>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>视觉载入 <span class="dim-text">// UPLOAD ARTWORK</span></label>
      
      <div class="file-drop-zone" :class="{ 'has-file': previewUrl }">
        <input v-if="!previewUrl" type="file" @change="handleFileUpload" accept="image/*" title="" />
        
        <div class="drop-text" v-if="!previewUrl">
          <span class="icon">🎨</span> 
          <span>点击或拖拽原创图像至此<br><small class="limit-hint">(必需，支持 JPG/PNG/GIF，最高 10MB)</small></span>
        </div>

        <div class="artwork-preview-container" v-else>
          <img :src="previewUrl" alt="Artwork Preview" class="artwork-image" />
          <div class="overlay-actions">
            <span class="file-name">{{ selectedFile?.name }}</span>
            <button type="button" class="remove-art-btn" @click="removeFile">
              REPLACE // 重新载入
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>归档画册 <span class="dim-text">// TARGET_ALBUM</span></label>
      <div class="album-selector-group">
        <select v-model="formData.folderId" class="cyber-select">
          <option :value="null">-- 独立发布 (STANDALONE) --</option>
          <option v-for="folder in myAlbums" :key="folder.id || folder.Id" :value="folder.id || folder.Id">
            📁 {{ folder.title || folder.Title }}
          </option>
        </select>
        <button type="button" class="cyber-btn-mini outline" @click="handleCreateAlbum">
          + NEW_ALBUM
        </button>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>特征标签 <span class="dim-text">// TAGS</span></label>
      <CyberTagInput v-model="formData.tags" :max-tags="5" />
    </div>

    <div class="progress-container" v-if="isSubmitting && uploadProgress > 0">
      <div class="progress-meta">
        <span class="glitch-text">TRANSMITTING_DATA...</span>
        <span class="percent">{{ uploadProgress }}%</span>
      </div>
      <div class="progress-bar-bg">
        <div class="progress-bar-fill" :style="{ width: uploadProgress + '%' }"></div>
      </div>
    </div>

    <div class="form-actions">
      <button type="submit" class="cyber-submit-btn" :disabled="isSubmitting">
        <span class="btn-decor"></span>
        <span v-if="!isSubmitting">EXECUTE // 发布画作</span>
        <span v-else class="glitch-text">DATA_FLOWING...</span>
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount, onMounted } from 'vue'
import axios from 'axios' // 🚀 引入原生 axios 处理 COS 直传，避免被 apiClient 拦截器干扰
import apiClient from '@/utils/api'
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue' 

const isSubmitting = ref(false)
const uploadProgress = ref(0) // 进度百分比 (0-100)
const selectedFile = ref(null)
const previewUrl = ref(null)

const formData = reactive({ title: '', author: '', desc: '', tags: '', folderId: null })
const myAlbums = ref([])

// 加载用户的画册列表
const fetchMyAlbums = async () => {
  try {
    const res = await apiClient.get('/Folder/my-list?category=0&type=0')
    if (res.data.success) {
      myAlbums.value = res.data.data
    }
  } catch (e) {
    console.error("无法加载画册列表", e)
  }
}

// 创建新画册弹窗
const handleCreateAlbum = async () => {
  const newAlbumName = prompt('>> SYSTEM: 请输入新画册名称 (ENTER_ALBUM_NAME):')
  if (!newAlbumName || !newAlbumName.trim()) return

  try {
    const res = await apiClient.post('/Folder/create', {
      title: newAlbumName.trim(),
      category: 0, 
      folderType: 0 
    })
    
    if (res.data.success) {
      await fetchMyAlbums()
      formData.folderId = res.data.data.id || res.data.data.Id
    }
  } catch (e) {
    alert('>> ERROR: 画册创建失败')
  }
}

onMounted(() => {
  fetchMyAlbums()
})

// 处理文件选择
const handleFileUpload = (event) => {
  const file = event.target.files[0]
  if (!file) return

  const maxSize = 10 * 1024 * 1024;
  if (file.size > maxSize) {
    alert(`协议拒绝：画作 [${file.name}] 体积过大，上限为 10MB`);
    event.target.value = ''
    return;
  }

  if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)

  selectedFile.value = file
  previewUrl.value = URL.createObjectURL(file)
  event.target.value = '' 
}

const removeFile = () => {
  selectedFile.value = null
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = null
  }
}

onBeforeUnmount(() => {
  if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
})

// 🚀 核心逻辑：三步走直传
const handleSubmit = async () => {
  if (!selectedFile.value) return alert('核心缺失：视觉绘画作品必须包含图像数据')
  
  // 准备最终标题
  let finalTitle = formData.title.trim()
  if (!finalTitle) {
    finalTitle = selectedFile.value.name.replace(/\.[^/.]+$/, "")
  }

  isSubmitting.value = true
  uploadProgress.value = 1; // 开始时设为 1% 唤醒进度条

  try {
    // 🚀 第一步：使用 apiClient (带 Token) 向后端申请凭证
    const authResponse = await apiClient.get(`/Drawing/get-upload-url?fileName=${encodeURIComponent(selectedFile.value.name)}`)
    
    if (!authResponse.data || !authResponse.data.success) {
        throw new Error(authResponse.data?.message || '无法获取上传凭证')
    }

    const { uploadUrl, finalUrl } = authResponse.data.data

    // 🚀 第二步：使用原生 axios 直传腾讯云 (避开所有全局配置)
    const cosResponse = await axios.put(uploadUrl, selectedFile.value, {
        headers: {
            'Content-Type': selectedFile.value.type // 确保云端正确识别文件格式
        },
        // 清除默认的 Authorization，防止腾讯云报错 403
        transformRequest: [(data) => data],
        onUploadProgress: (progressEvent) => {
            if (progressEvent.total) {
                // 计算进度并更新 UI
                uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            }
        }
    })

    if (cosResponse.status !== 200) {
        throw new Error(`云端传输异常: ${cosResponse.status}`)
    }

    // 🚀 第三步：拿着最终干净地址回到后端登记入库
    // 注意：我们传给后端 finalUrl，后端会自动掐掉签名部分存库
    const payload = {
        ImageUrl: finalUrl,
        Title: finalTitle,
        AuthorName: formData.author.trim(),
        Desc: formData.desc,
        Tags: formData.tags
    }

    const submitResponse = await apiClient.post('/Drawing/submit', payload)

    if (submitResponse.data && submitResponse.data.success) {
      const newDrawingId = submitResponse.data.data?.id || submitResponse.data.data?.Id;

      // 归档画册逻辑
      if (formData.folderId && newDrawingId) {
        try {
          await apiClient.post('/Folder/add-item', {
            folderId: formData.folderId,
            targetId: newDrawingId
          });
        } catch (folderErr) {
          console.error("画册归档异步失败", folderErr);
        }
      }

      alert(`ART 序列发布成功！\n奖励已通过神经链接发放。`)
      
      // 清空表单
      formData.title = ''
      formData.author = ''
      formData.desc = ''
      formData.tags = '' 
      formData.folderId = null
      uploadProgress.value = 0;
      removeFile()
    } else {
      throw new Error(submitResponse.data?.message || '数据库同步失败')
    }

  } catch (error) {
    uploadProgress.value = 0;
    // 如果 error 是 axios 错误，尝试提取更友好的信息
    const errorMsg = error.response?.data?.message || error.message || '系统传输崩溃'
    alert(`发射失败: ${errorMsg}`)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
/* 核心主题变量 */
.art-theme {
  --theme-color: #ff2a6d;
  --theme-color-rgb: 255, 42, 109;
  --theme-color-dark: #cc1d53;
  --bg-dark: #111;
  --border-dim: #ddd;
}

/* 🚀 赛博进度条专属样式 */
.progress-container {
  margin-top: 5px;
  width: 100%;
}

.progress-meta {
  display: flex;
  justify-content: space-between;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  color: var(--theme-color);
  margin-bottom: 5px;
  text-shadow: 0 0 5px rgba(var(--theme-color-rgb), 0.5);
}

.progress-bar-bg {
  height: 4px;
  background: rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(var(--theme-color-rgb), 0.2);
  position: relative;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background: var(--theme-color);
  box-shadow: 0 0 10px var(--theme-color);
  transition: width 0.3s ease;
  position: relative;
}

/* 进度条扫描动画效果 */
.progress-bar-fill::after {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
  animation: scan 1.5s infinite;
}

@keyframes scan {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

/* 基础表单样式 */
.cyber-form { display: flex; flex-direction: column; gap: 24px; padding-top: 10px; position: relative; }
.form-decorative-bar { position: absolute; top: -10px; left: 0; width: 60px; height: 4px; background: var(--theme-color); box-shadow: 0 0 10px rgba(var(--theme-color-rgb), 0.5); }
.form-group { display: flex; flex-direction: column; gap: 10px; }
.form-group label { display: flex; align-items: center; gap: 8px; font-weight: 800; font-size: 0.85rem; color: #111; letter-spacing: 1px; }
.label-decor { display: inline-block; width: 6px; height: 6px; background: #111; transform: rotate(45deg); }
.dim-text { color: #888; font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; font-weight: normal; }

.cyber-form input[type="text"], .cyber-form textarea { width: 100%; padding: 14px 16px; border: 1px solid var(--border-dim); background: #fdfdfd; font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: all 0.3s ease; outline: none; resize: vertical; box-sizing: border-box; border-left: 4px solid var(--border-dim); }
.cyber-form input[type="text"]:focus, .cyber-form textarea:focus { border-color: var(--bg-dark); background: #fff; border-left-color: var(--theme-color); box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.15); }

/* 上传预览区样式 */
.file-drop-zone { position: relative; border: 1px dashed #bbb; background-color: #f9f9f9; padding: 40px 20px; text-align: center; cursor: pointer; transition: 0.3s; }
.file-drop-zone:hover:not(.has-file) { border-color: var(--theme-color); background-color: #fff5f8; }
.file-drop-zone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; width: 100%; z-index: 10; }
.file-drop-zone.has-file { padding: 10px; border-style: solid; border-color: var(--theme-color); background: #111; cursor: default; }

.artwork-preview-container { position: relative; width: 100%; max-height: 500px; display: flex; justify-content: center; align-items: center; overflow: hidden; background: #000; }
.artwork-image { max-width: 100%; max-height: 500px; object-fit: contain; display: block; }
.overlay-actions { position: absolute; bottom: 0; left: 0; width: 100%; padding: 15px; background: linear-gradient(to top, rgba(0,0,0,0.9), transparent); display: flex; justify-content: space-between; align-items: flex-end; opacity: 0; transition: opacity 0.3s; }
.artwork-preview-container:hover .overlay-actions { opacity: 1; }

.file-name { color: #fff; font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; text-shadow: 1px 1px 2px #000; max-width: 70%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.remove-art-btn { background: var(--theme-color); color: #fff; border: none; padding: 6px 15px; font-family: 'Anton'; font-size: 0.9rem; cursor: pointer; transition: 0.2s; z-index: 20; }
.remove-art-btn:hover { background: #fff; color: var(--theme-color); }

/* 下拉选择与画册样式 */
.album-selector-group { display: flex; gap: 10px; align-items: center; }
.cyber-select { flex: 1; padding: 14px 16px; border: 1px solid var(--border-dim); background: #fdfdfd; font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; outline: none; border-left: 4px solid var(--border-dim); appearance: none; }
.cyber-select:focus { border-left-color: var(--theme-color); }

.cyber-btn-mini.outline { background: transparent; border: 2px solid var(--bg-dark); color: var(--bg-dark); padding: 0 15px; height: 48px; font-family: 'Anton'; font-size: 0.9rem; cursor: pointer; transition: 0.2s; }
.cyber-btn-mini.outline:hover { background: var(--bg-dark); color: #fff; }

/* 提交按钮 */
.form-actions { margin-top: 10px; display: flex; justify-content: flex-end; }
.cyber-submit-btn { position: relative; background: var(--bg-dark); color: #fff; border: none; padding: 16px 45px; font-family: 'Anton'; font-size: 1.2rem; cursor: pointer; transition: 0.2s; letter-spacing: 2px; clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%); }
.cyber-submit-btn:hover:not(:disabled) { background: var(--theme-color); transform: translateY(-2px); box-shadow: 0 10px 20px rgba(var(--theme-color-rgb), 0.3); }
.cyber-submit-btn:disabled { background: #555; color: #999; cursor: not-allowed; }

.glitch-text { display: inline-block; animation: glitch 0.3s infinite; }
@keyframes glitch { 0% { opacity: 1; transform: translateX(0); } 25% { opacity: 0.8; transform: translateX(-1px); } 50% { opacity: 0.9; transform: translateX(1px); } 100% { opacity: 1; transform: translateX(0); } }
</style>