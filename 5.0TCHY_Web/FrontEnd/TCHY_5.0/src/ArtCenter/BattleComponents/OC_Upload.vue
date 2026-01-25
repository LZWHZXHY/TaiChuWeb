<template>
  <section class="module-upload">
    <div class="form-panel">
      <div class="panel-header">
        <span class="deco">▼</span> 档案登记
      </div>
      <form @submit.prevent="submitOC" class="cyber-form">
        <div class="form-row">
          <div class="form-group half">
            <label>角色名称 <span class="req">*</span></label>
            <input v-model="upForm.OCName" class="cyber-input" placeholder="输入角色名称" required />
          </div>
          <div class="form-group half">
            <label>作者署名 <span class="req">*</span></label>
            <input v-model="upForm.authorName" class="cyber-input" placeholder="建议圈名" required />
          </div>
        </div>
        
        <div class="form-row three-col">
          <div class="form-group">
            <label>性别</label>
            <select v-model="upForm.gender" class="cyber-select" required>
              <option value="">选择性别...</option>
              <option value="0">男</option>
              <option value="1">女</option>
              <option value="2">其他</option>
            </select>
          </div>
          <div class="form-group">
            <label>物种</label>
            <input v-model="upForm.species" class="cyber-input" required />
          </div>
          <div class="form-group">
            <label>年龄</label>
            <input v-model.number="upForm.age" type="number" class="cyber-input" required />
          </div>
        </div>

        <div class="form-group">
          <label>所属世界观 <span class="req">*</span></label>
          <input v-model="upForm.poo" class="cyber-input" required placeholder="如无特定世界观请填 '太初战场'"/>
        </div>

        <div class="form-group">
          <label>能力设定 (ABILITY) <span class="req">*</span></label>
          <textarea v-model="upForm.ability" class="cyber-textarea" rows="4" required></textarea>
        </div>

        <div class="form-group">
          <label>角色性格 (PERSONALITY)</label>
          <textarea v-model="upForm.character" class="cyber-textarea" rows="3" placeholder="性格特点、行为习惯、喜好等..."></textarea>
        </div>

        <div class="form-group">
          <label>背景故事 (BACKGROUND)</label>
          <textarea v-model="upForm.background" class="cyber-textarea" rows="4" placeholder="角色的身世、经历或重要过往..."></textarea>
        </div>

        <div class="form-group">
          <label>代表色/配色方案 (COLOR SCHEME)</label>
          <input v-model="upForm.colors" class="cyber-input" placeholder="例如：黑/金/红，或者具体的色号设定" />
        </div>

        <div class="form-group">
          <label>武器/装备详细描述 (WEAPON DETAILS)</label>
          <textarea v-model="upForm.weaponDesc" class="cyber-textarea" rows="3" placeholder="武器的名称、功能、参数等文字描述..."></textarea>
        </div>

        <div class="form-group">
          <label>武器立绘 (可选/多张)</label>
          <div class="upload-zone weapon-zone" @click="$refs.weaponInput.click()">
            <div v-if="upForm.weaponFiles.length" class="weapon-grid">
              <div v-for="(item, idx) in upForm.weaponFiles" :key="idx" class="weapon-item preview-mode" @click.stop>
                <img :src="item.url" class="w-img" />
                <button type="button" @click="removeWeapon(idx)" class="w-remove">×</button>
              </div>
              <div class="add-more" @click.stop="$refs.weaponInput.click()">+</div>
            </div>
            <div v-else class="placeholder">
              <div class="icon-small">⚔️</div>
              <span>上传武器图 (多选)</span>
            </div>
          </div>
          <input ref="weaponInput" type="file" accept="image/*" multiple style="display:none" @change="handleWeaponFiles" />
        </div>

        <div class="form-group">
          <label>其他备注 (EXTRA NOTES)</label>
          <textarea v-model="upForm.extraDesc" class="cyber-textarea" rows="2" placeholder="补充设定、注意事项等..."></textarea>
        </div>

        <div class="form-group">
          <label>角色立绘 (MAIN PORTRAIT) <span class="req">*</span></label>
          <div class="upload-zone" :class="{ 'has-file': upForm.charFile }" @click="$refs.charInput.click()">
            <div v-if="upForm.charFile" class="file-preview">
              <span class="file-name">FILE: {{ upForm.charFile.name }}</span>
              <button type="button" @click.stop="upForm.charFile = null" class="remove-btn">[REMOVE]</button>
            </div>
            <div v-else class="placeholder">
              <div class="icon">+</div>
              <span>上传主立绘 (MAX 5MB)</span>
            </div>
          </div>
          <input ref="charInput" type="file" accept="image/*" style="display:none" @change="handleCharFile" />
        </div>

        <div class="form-group checkbox-row">
          <label class="cyber-checkbox">
            <input type="checkbox" v-model="upForm.agreed" required />
            <span class="checkmark"></span>
            <span class="text">我已阅读并接受企划规则</span>
          </label>
        </div>

        <div class="form-actions">
          <button class="cyber-btn primary full" type="submit" :disabled="upSubmitting">
            {{ upSubmitting ? 'UPLOADING_DATA...' : 'SUBMIT_DOSSIER // 提交档案' }}
          </button>
        </div>
      </form>
    </div>
  </section>
</template>

<script setup>
import { ref, reactive } from 'vue'
import apiClient from '@/utils/api'

const emit = defineEmits(['upload-success'])
const upSubmitting = ref(false)

// 表单状态：已补齐所有字段
const upForm = reactive({
  OCName: '',
  authorName: '',
  gender: '',
  age: '',
  species: '',
  ability: '',
  background: '', 
  poo: '',
  // ✨ 新增字段初始化
  character: '', // 性格
  colors: '',    // 配色
  weaponDesc: '',// 武器描述
  extraDesc: '', // 额外描述
  
  agreed: false,
  charFile: null,
  weaponFiles: [] // 存储结构: { file: File, url: string, name: string }
})

// 处理角色主图
const handleCharFile = (e) => { 
  const file = e.target.files[0]
  if(file && file.size <= 5*1024*1024) upForm.charFile = file; 
  else alert('FILE_TOO_LARGE_OR_INVALID') 
}

// 处理武器多图 (生成预览URL)
const handleWeaponFiles = (e) => {
  const files = Array.from(e.target.files)
  const validFiles = files.filter(f => f.size <= 5*1024*1024)
  if (validFiles.length < files.length) alert('SOME_FILES_SKIPPED (MAX 5MB)')
  
  const newFiles = validFiles.map(file => ({
    file: file,
    url: URL.createObjectURL(file), // 生成缩略图链接
    name: file.name
  }))

  upForm.weaponFiles = [...upForm.weaponFiles, ...newFiles]
  e.target.value = ''
}

// 移除单个武器图
const removeWeapon = (index) => {
  URL.revokeObjectURL(upForm.weaponFiles[index].url) // 释放内存
  upForm.weaponFiles.splice(index, 1)
}

// 提交逻辑
const submitOC = async () => {
  if(!upForm.agreed) return alert('PLEASE_ACCEPT_RULES')
  if(!upForm.charFile) return alert('MAIN_IMAGE_REQUIRED')

  upSubmitting.value = true
  try {
    const fd = new FormData()
    // 基础字段
    fd.append('OCName', upForm.OCName)
    fd.append('authorName', upForm.authorName)
    fd.append('gender', upForm.gender)
    fd.append('age', upForm.age)
    fd.append('species', upForm.species)
    fd.append('ability', upForm.ability)
    fd.append('POO', upForm.poo)
    fd.append('currentTime', Math.floor(Date.now()/1000).toString())
    
    // 可选字段 (有值才传)
    if(upForm.background) fd.append('Background', upForm.background)
    // ✨ 提交新增字段
    if(upForm.character) fd.append('character', upForm.character)
    if(upForm.colors) fd.append('colors', upForm.colors)
    if(upForm.weaponDesc) fd.append('weaponDesc', upForm.weaponDesc)
    if(upForm.extraDesc) fd.append('extraDesc', upForm.extraDesc)
    
    // 文件字段
    fd.append('CharacterImage', upForm.charFile)
    
    // 武器多文件
    upForm.weaponFiles.forEach(item => {
      fd.append('WeaponImages', item.file) // 注意只传 file 对象
    })

    await apiClient.post('/OCBattleField/upload', fd, { 
      headers: {'Content-Type': 'multipart/form-data'} 
    })
    
    alert('UPLOAD_COMPLETE')
    
    // 清理预览图内存
    upForm.weaponFiles.forEach(item => URL.revokeObjectURL(item.url))

    // 重置表单
    Object.assign(upForm, { 
      OCName:'', authorName:'', gender:'', age:'', species:'', ability:'', 
      background: '', poo:'', 
      character: '', colors: '', weaponDesc: '', extraDesc: '', // 重置新字段
      agreed:false, charFile:null, weaponFiles: [] 
    })
    
    emit('upload-success')
  } catch(e) { 
    console.error(e)
    alert('UPLOAD_FAILED: ' + (e.response?.data?.message || 'UNKNOWN_ERROR')) 
  } finally { 
    upSubmitting.value = false 
  }
}
</script>

<style scoped>
/* 保持原有布局样式 */
.module-upload { display: flex; justify-content: center; width: 100%; padding-bottom: 50px; }
.form-panel { width: 100%; max-width: 800px; background: var(--white); border: 4px solid var(--black); padding: 30px; box-shadow: 10px 10px 0 rgba(0,0,0,0.2); }
.panel-header { font-family: var(--heading); font-size: 1.5rem; border-bottom: 4px solid var(--black); padding-bottom: 15px; margin-bottom: 25px; }
.deco { color: var(--red); margin-right: 10px; }
.cyber-form { display: flex; flex-direction: column; gap: 20px; }
.form-row { display: flex; gap: 20px; }
.form-row.three-col { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px; }
.form-group { flex: 1; }
.form-group label { display: block; font-weight: bold; margin-bottom: 8px; font-size: 0.8rem; }
.req { color: var(--red); }

/* 上传区域通用样式 */
.upload-zone { border: 2px dashed var(--black); padding: 20px; text-align: center; cursor: pointer; background: #eee; transition: 0.2s; min-height: 80px; display: flex; align-items: center; justify-content: center; flex-direction: column; }
.upload-zone:hover { background: #fff; border-color: var(--red); }
.upload-zone.has-file { border-style: solid; background: #000; }

.file-preview { display: flex; justify-content: space-between; align-items: center; width: 100%; color: var(--white); }
.file-name { font-family: var(--mono); font-size: 0.8rem; text-overflow: ellipsis; overflow: hidden; white-space: nowrap; max-width: 80%; }
.remove-btn { background: var(--red); border: none; color: #fff; cursor: pointer; font-weight: bold; padding: 2px 6px; font-size: 0.7rem; }

/* 武器立绘预览网格 */
.weapon-zone { padding: 10px; align-items: flex-start; }
.weapon-grid { display: flex; flex-wrap: wrap; gap: 10px; width: 100%; }

/* 缩略图模式样式 */
.weapon-item.preview-mode {
  padding: 0;
  width: 60px;
  height: 60px;
  position: relative;
  border: 1px solid #555;
  overflow: hidden;
  background: #000;
}
.w-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  opacity: 0.8;
  transition: 0.2s;
}
.weapon-item:hover .w-img { opacity: 1; }
.weapon-item:hover { border-color: var(--red); }

/* 删除按钮 */
.w-remove {
  position: absolute;
  top: 0;
  right: 0;
  background: rgba(0, 0, 0, 0.7);
  color: var(--red);
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  z-index: 2;
  border: none; 
  cursor: pointer;
}

/* 添加按钮 */
.add-more { width: 30px; height: 30px; border: 2px dashed #999; display: flex; align-items: center; justify-content: center; font-weight: bold; color: #666; cursor: pointer; }
.add-more:hover { border-color: var(--red); color: var(--red); }

.placeholder .icon { font-size: 2rem; font-weight: bold; margin-bottom: 5px; }
.placeholder .icon-small { font-size: 1.5rem; margin-bottom: 5px; }
</style>