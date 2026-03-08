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
          <span>点击或拖拽原创图像至此<br><small class="limit-hint">(必需，单图最高支持 10MB)</small></span>
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

    <div class="form-actions">
      <button type="submit" class="cyber-submit-btn" :disabled="isSubmitting">
        <span class="btn-decor"></span>
        <span v-if="!isSubmitting">EXECUTE // 发布画作</span>
        <span v-else class="glitch-text">TRANSMITTING...</span>
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount, onMounted } from 'vue'
import apiClient from '@/utils/api'
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue' 

const isSubmitting = ref(false)
const selectedFile = ref(null)
const previewUrl = ref(null)

// ✅ 添加了 folderId 字段
const formData = reactive({ title: '', author: '', desc: '', tags: '', folderId: null })

// ✅ 新增：画册数据与相关方法
const myAlbums = ref([])

const fetchMyAlbums = async () => {
  try {
    // category=0(Drawing), type=0(Album - 作者自己的合集)
    const res = await apiClient.get('/Folder/my-list?category=0&type=0')
    if (res.data.success) {
      myAlbums.value = res.data.data
    }
  } catch (e) {
    console.error("无法加载画册列表", e)
  }
}

const handleCreateAlbum = async () => {
  const newAlbumName = prompt('>> SYSTEM: 请输入新画册名称 (ENTER_ALBUM_NAME):')
  if (!newAlbumName || !newAlbumName.trim()) return

  try {
    const res = await apiClient.post('/Folder/create', {
      title: newAlbumName.trim(),
      category: 0, // 0: Drawing
      folderType: 0 // 0: Album
    })
    
    if (res.data.success) {
      await fetchMyAlbums() // 刷新列表
      // 兼容后端返回字段的大小写问题
      formData.folderId = res.data.data.id || res.data.data.Id
    }
  } catch (e) {
    alert('>> ERROR: 画册创建失败')
  }
}

// 页面加载时自动拉取现有画册
onMounted(() => {
  fetchMyAlbums()
})

const handleFileUpload = (event) => {
  const file = event.target.files[0]
  if (!file) return

  const maxSize = 10 * 1024 * 1024;
  if (file.size > maxSize) {
    alert(`协议拒绝：画作 [${file.name}] 体积过大，上限为 10MB`);
    event.target.value = ''
    return;
  }

  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
  }

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
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
  }
})

const handleSubmit = async () => {
  if (!selectedFile.value) return alert('核心缺失：视觉绘画作品必须包含图像数据')
  
  let finalTitle = formData.title.trim()
  if (!finalTitle) {
    finalTitle = selectedFile.value.name.replace(/\.[^/.]+$/, "")
  }

  let finalAuthor = formData.author.trim()

  isSubmitting.value = true
  try {
    // 第一步：上传画作本体
    const payload = new FormData()
    payload.append('Title', finalTitle)
    
    if (finalAuthor) payload.append('AuthorName', finalAuthor)
    if (formData.tags) payload.append('Tags', formData.tags)
    if (formData.desc) payload.append('Desc', formData.desc)
    payload.append('Image', selectedFile.value) 

    const response = await apiClient.post('/Drawing/upload', payload, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    if (response.data && response.data.success) {
      // 获取新画作的 ID (兼容大小写)
      const newDrawingId = response.data.data?.id || response.data.data?.Id;

      // ✅ 第二步：如果用户选择了画册，则调用注入画册接口 (链式调用)
      if (formData.folderId && newDrawingId) {
        try {
          await apiClient.post('/Folder/add-item', {
            folderId: formData.folderId,
            targetId: newDrawingId
          });
        } catch (folderErr) {
          console.error("画作已上传，但归档至画册失败", folderErr);
          // 这里不打断整体成功提示，仅在控制台记录
        }
      }

      alert(`ART 序列发布成功！\n系统提示: 画作已成功上传${formData.folderId ? '并归档至画册' : ''}`)
      
      // 清空所有状态
      formData.title = ''
      formData.author = ''
      formData.desc = ''
      formData.tags = '' 
      formData.folderId = null // ✅ 重置画册选择
      removeFile()
    } else {
      alert(`发射失败: ${response.data?.message || '未知错误'}`)
    }

  } catch (error) {
    alert(`网络传输崩溃: ${error.response?.data?.message || error.message}`)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
/* 保持原有的霓虹粉 CSS 不变 */
.art-theme {
  --theme-color: #ff2a6d;
  --theme-color-rgb: 255, 42, 109;
  --theme-color-dark: #cc1d53;
  --bg-dark: #111;
  --border-dim: #ddd;
}

.cyber-form { display: flex; flex-direction: column; gap: 24px; padding-top: 10px; position: relative; }
.form-decorative-bar { position: absolute; top: -10px; left: 0; width: 60px; height: 4px; background: var(--theme-color); box-shadow: 0 0 10px rgba(var(--theme-color-rgb), 0.5); }
.form-group { display: flex; flex-direction: column; gap: 10px; }
.form-group label { display: flex; align-items: center; gap: 8px; font-weight: 800; font-size: 0.85rem; color: #111; letter-spacing: 1px; }
.label-decor { display: inline-block; width: 6px; height: 6px; background: #111; transform: rotate(45deg); }
.dim-text { color: #888; font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; font-weight: normal; }

.cyber-form input[type="text"], .cyber-form textarea { width: 100%; padding: 14px 16px; border: 1px solid var(--border-dim); background: #fdfdfd; font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: all 0.3s ease; outline: none; resize: vertical; box-sizing: border-box; border-left: 4px solid var(--border-dim); }
.cyber-form input[type="text"]:hover, .cyber-form textarea:hover { background: #f4f5f7; border-color: #ccc; }
.cyber-form input[type="text"]:focus, .cyber-form textarea:focus { border-color: var(--bg-dark); background: #fff; border-left-color: var(--theme-color); box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.15); }

.file-drop-zone { position: relative; border: 1px dashed #bbb; background-color: #f9f9f9; background-image: radial-gradient(#e0e0e0 1px, transparent 1px); background-size: 15px 15px; padding: 40px 20px; text-align: center; cursor: pointer; transition: all 0.3s; }
.file-drop-zone:hover:not(.has-file) { border-color: var(--theme-color); background-color: #fff5f8; background-image: radial-gradient(rgba(var(--theme-color-rgb), 0.15) 1px, transparent 1px); }
.file-drop-zone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; width: 100%; z-index: 10; }

.drop-text { font-weight: bold; color: #555; font-size: 0.95rem; pointer-events: none; display: flex; flex-direction: column; align-items: center; gap: 8px; }
.drop-text .icon { font-size: 2.2rem; margin-bottom: 5px; opacity: 0.9; }
.limit-hint { font-size: 0.75rem; color: #999; font-family: 'JetBrains Mono', monospace; }

.file-drop-zone.has-file { padding: 10px; border-style: solid; border-color: var(--theme-color); background: #111; cursor: default; }
.artwork-preview-container { position: relative; width: 100%; max-height: 500px; display: flex; justify-content: center; align-items: center; overflow: hidden; background: #000; }
.artwork-image { max-width: 100%; max-height: 500px; object-fit: contain; display: block; box-shadow: 0 0 20px rgba(0,0,0,0.5); }

.overlay-actions { position: absolute; bottom: 0; left: 0; width: 100%; padding: 15px; background: linear-gradient(to top, rgba(0,0,0,0.9), transparent); display: flex; justify-content: space-between; align-items: flex-end; opacity: 0; transition: opacity 0.3s ease; }
.artwork-preview-container:hover .overlay-actions { opacity: 1; }
.file-name { color: #fff; font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; text-shadow: 1px 1px 2px #000; max-width: 70%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.remove-art-btn { background: var(--theme-color); color: #fff; border: none; padding: 6px 15px; font-family: 'Anton'; font-size: 0.9rem; cursor: pointer; transition: 0.2s; z-index: 20; }
.remove-art-btn:hover { background: #fff; color: var(--theme-color); }

.form-actions { margin-top: 20px; display: flex; justify-content: flex-end; }
.cyber-submit-btn { position: relative; background: var(--bg-dark); color: #fff; border: none; padding: 16px 45px; font-family: 'Anton'; font-size: 1.2rem; cursor: pointer; transition: 0.2s; letter-spacing: 2px; clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%); }
.btn-decor { position: absolute; bottom: 5px; right: 5px; width: 6px; height: 6px; background: rgba(255,255,255,0.2); }
.cyber-submit-btn:hover:not(:disabled) { background: var(--theme-color); color: #fff; transform: translateY(-2px); box-shadow: 0 10px 20px rgba(var(--theme-color-rgb), 0.3); }
.cyber-submit-btn:hover:not(:disabled) .btn-decor { background: #fff; }
.cyber-submit-btn:disabled { background: #555; color: #999; cursor: not-allowed; }

@keyframes glitch { 0% { opacity: 1; transform: translateX(0); } 25% { opacity: 0.8; transform: translateX(-1px); } 50% { opacity: 0.9; transform: translateX(1px); } 100% { opacity: 1; transform: translateX(0); } }
.glitch-text { display: inline-block; animation: glitch 0.3s infinite; }

/* ✅ 新增：归档画册选择器样式 */
.album-selector-group {
  display: flex;
  gap: 10px;
  align-items: center;
}

.cyber-select {
  flex: 1;
  padding: 14px 16px;
  border: 1px solid var(--border-dim);
  background: #fdfdfd;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.9rem;
  color: #111;
  outline: none;
  cursor: pointer;
  border-left: 4px solid var(--border-dim);
  transition: all 0.3s ease;
  appearance: none;
  background-image: linear-gradient(45deg, transparent 50%, var(--bg-dark) 50%), linear-gradient(135deg, var(--bg-dark) 50%, transparent 50%);
  background-position: calc(100% - 20px) calc(1em + 2px), calc(100% - 15px) calc(1em + 2px);
  background-size: 5px 5px, 5px 5px;
  background-repeat: no-repeat;
}

.cyber-select:focus, .cyber-select:hover {
  border-color: var(--bg-dark);
  background-color: #fff;
  border-left-color: var(--theme-color);
  box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.15);
}

.cyber-btn-mini.outline {
  background: transparent;
  border: 2px solid var(--bg-dark);
  color: var(--bg-dark);
  padding: 0 15px;
  height: 48px;
  font-family: 'Anton', sans-serif;
  font-size: 0.9rem;
  font-weight: bold;
  cursor: pointer;
  transition: 0.2s;
  flex-shrink: 0;
}

.cyber-btn-mini.outline:hover {
  background: var(--bg-dark);
  color: #fff;
  box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.3);
}
</style>