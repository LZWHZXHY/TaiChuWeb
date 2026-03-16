<template>
  <div class="oc-industrial-system">
    <header class="sys-toolbar">
      <div class="toolbar-left">
        <div class="query-unit">
          <span class="prefix">QUERY_>></span>
          <input v-model="rosterQuery" placeholder="搜索档案代号..." @input="rosterPage = 1" />
        </div>
        
        <div class="category-tabs">
          <button :class="{ active: viewMode === 'all' }" @click="viewMode = 'all'">[ ALL_ARCHIVES ]</button>
          <button :class="{ active: viewMode === 'mine' }" @click="viewMode = 'mine'">[ MY_RECORDS ]</button>
        </div>
      </div>

      <div class="sys-meta">
        <div class="status-led">
          <span class="pulse"></span> 
          <span class="status-text">DB_CONNECTED</span>
        </div>
        <button class="action-btn-industrial" @click="startCreate">CREATE_ARCHIVE</button>
        <button class="action-btn-industrial red" @click="loadRoster">RELOAD_DATABASE</button>
      </div>
    </header>

    <div v-if="rosterLoading" class="sys-loader">
      <div class="glitch-container">
        <div class="glitch-text" data-text="SYNCING_DATA...">SYNCING_DATA...</div>
      </div>
    </div>

    <div v-else class="roster-viewport custom-scroll">
      <div v-if="paginatedRoster.length === 0" class="empty-notice">
        >> NO_DATA_DETECTED: 数据库中未检索到匹配的记录
      </div>

      <div class="module-grid">
        <div 
          v-for="oc in paginatedRoster" 
          :key="oc.id" 
          class="oc-module-card"
          :class="{ 'is-owner': isOwner(oc) }"
          @click="openDetail(oc)"
        >
          <div class="warning-tape"></div>
          
          <div class="card-inner">
            <div class="portrait-box">
              <img :src="formatAvatar(oc.avatar || oc.OC_image_url)" @error="handleImgError" />
              <div class="scan-line"></div>
              <div class="id-tag">REF_{{ oc.id }}</div>
            </div>

            <div class="info-box">
              <h4 class="oc-name">{{ oc.name }}</h4>
              <div class="auth-id">AUTHOR: {{ oc.authorName || 'UID: ' + oc.authorID }}</div>
              
              <div class="battle-stats">
                <div class="stat-labels">
                  <span class="label">SYNC_RATE</span>
                  <span class="val">{{ oc.winCount || 0 }}W / {{ oc.loseCount || 0 }}L</span>
                </div>
                <div class="energy-track">
                  <div class="energy-fill" :style="{ width: calculateWinRate(oc) + '%' }"></div>
                </div>
              </div>
            </div>
          </div>
          
          <div v-if="isOwner(oc)" class="owner-flag">OWNER_UNIT</div>
        </div>
      </div>
    </div>

    <footer class="pagination-footer" v-if="totalPages > 1">
      <button :disabled="rosterPage <= 1" @click="rosterPage--" class="nav-btn">PREV_BLOCK</button>
      <div class="page-info">
        <span class="current">{{ rosterPage }}</span> / <span class="total">{{ totalPages }}</span>
      </div>
      <button :disabled="rosterPage >= totalPages" @click="rosterPage++" class="nav-btn">NEXT_BLOCK</button>
    </footer>

    <Teleport to="body">
      <Transition name="term-zoom">
        <div v-if="selectedOC || isCreating" class="terminal-overlay" @click.self="closeModal">
          <div class="terminal-frame">
            
            <header class="term-header">
              <div class="term-title">
                <span class="icon">■</span> >> {{ isCreating ? 'INIT_ARCHIVE' : (isEditing ? 'EDIT_PROTOCOL' : 'IDENTITY_ARCHIVE') }}: {{ isCreating ? 'NEW_SUBJECT' : selectedOC.name }}
              </div>
              <button class="term-close" @click="closeModal">TERMINATE_PROCESS [X]</button>
            </header>

            <div class="term-body" v-if="currentVersion || isCreating">
              <div class="term-layout">
                
                <aside v-if="!isEditing" class="term-aside">
                  <div class="focus-portrait">
                    <img :src="formatAvatar(currentVersion.OC_image_url)" @error="handleImgError" />
                    <div class="laser-pointer"></div>
                  </div>
                  
                  <div class="combat-matrix">
                    <div class="matrix-box">
                      <label>VICTORIES</label>
                      <span class="val green">{{ currentVersion.winCount || 0 }}</span>
                    </div>
                    <div class="matrix-box">
                      <label>DEFEATS</label>
                      <span class="val red">{{ currentVersion.loseCount || 0 }}</span>
                    </div>
                  </div>

                  <div class="aside-btns">
                    <button class="btn-block red" @click="challengeOC(selectedOC)">约战协议_INIT</button>
                    <button v-if="isOwner(selectedOC)" class="btn-block" @click="startEdit">修改档案_MODIFY</button>
                    <button v-if="isOwner(selectedOC)" class="btn-block hollow" @click="deleteOC(selectedOC)">抹除档案_ERASE</button>
                  </div>
                </aside>

                <main class="term-main custom-scroll">
                  
                  <template v-if="!isEditing">
                    <nav class="term-tabs">
                      <button :class="{ active: detailTab === 'info' }" @click="detailTab = 'info'">[ INTEL_INFO ]</button>
                      <button :class="{ active: detailTab === 'history' }" @click="detailTab = 'history'">[ VERSION_LOG ]</button>
                    </nav>

                    <div v-if="detailTab === 'info'" class="tab-pane">
                      <div class="data-group">
                        <div class="group-label">// IDENTITY_METRICS</div>
                        <div class="metric-grid">
                          <div class="m-item"><span class="k">REALM:</span><span class="v">{{ selectedOC.realm }}</span></div>
                          <div class="m-item"><span class="k">SPECIES:</span><span class="v">{{ currentVersion.species }}</span></div>
                          <div class="m-item"><span class="k">GENDER:</span><span class="v">{{ getGenderText(currentVersion.gender) }}</span></div>
                          <div class="m-item"><span class="k">AGE:</span><span class="v">{{ currentVersion.age }}</span></div>
                          <div class="m-item full"><span class="k">P.O.O:</span><span class="v red-txt">{{ currentVersion.POO }}</span></div>
                        </div>
                      </div>

                      <div class="data-group">
                        <div class="group-label">// TEMPORAL_TIMESTAMP</div>
                        <div class="metric-grid">
                          <div class="m-item"><span class="k">创建日期:</span><span class="v">{{ formatDate(selectedOC.createTime) }}</span></div>
                          <div class="m-item"><span class="k">最后同步:</span><span class="v">{{ formatDate(selectedOC.updateTime) }}</span></div>
                        </div>
                      </div>

                      <div class="data-group">
                        <div class="group-label">// ABILITY_CORE</div>
                        <div class="text-block">{{ currentVersion.ability }}</div>
                      </div>

                      <div class="data-group" v-if="currentVersion.character">
                        <div class="group-label">// PSYCHOLOGY</div>
                        <div class="text-block">{{ currentVersion.character }}</div>
                      </div>
                      
                      <div class="data-group" v-if="currentVersion.OC_WeapenDesc">
                        <div class="group-label">// ARMAMENT</div>
                        <div class="text-block">{{ currentVersion.OC_WeapenDesc }}</div>
                      </div>
                    </div>

                    <div v-else class="tab-pane history-log">
                      <div v-for="(ver, index) in sortedVersions" :key="ver.id" class="log-card" :class="{ current: ver.isCurrent }">
                        <div class="log-meta">
                          <span class="v-num">V_{{ ver.versionNumber }}</span>
                          <span class="v-date">{{ formatDate(ver.createTime) }}</span>
                          <span v-if="ver.isCurrent" class="v-tag">CURRENT</span>
                        </div>
                        <p class="v-note"><strong>备注:</strong> {{ ver.versionDescription }}</p>
                        
                        <div class="v-diff-box" v-if="getVersionDiff(ver, index).length">
                          <div v-for="diff in getVersionDiff(ver, index)" :key="diff.field" class="diff-line">
                            <span class="diff-field">[{{ diff.field }}]</span>
                            <span class="diff-old">{{ diff.old }}</span>
                            <span class="diff-arrow">>>></span>
                            <span class="diff-new">{{ diff.new }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>

                  <template v-else>
                    <div class="editor-view">
                      <div class="group-label">{{ isCreating ? '新建档案授权录入' : '修改授权记录: ' + selectedOC.name }}</div>
                      <div class="form-grid">
                        <div class="f-item"><label>识别名 NAME</label><input v-model="editForm.name" /></div>
                        
                        <div class="f-item"><label>归属领域 REALM</label>
                          <select v-model="editForm.realm">
                            <option value="火柴人">火柴人 (Stick Figure)</option>
                            <option value="跑团">跑团 (TTRPG)</option>
                            <option value="综合">综合设定 (General)</option>
                            <option value="企划">企划 (Project)</option>
                          </select>
                        </div>

                        <div class="f-item"><label>种族 SPECIES</label><input v-model="editForm.species" /></div>
                        <div class="f-item"><label>性别</label>
                          <select v-model="editForm.gender">
                            <option :value="0">MALE</option><option :value="1">FEMALE</option><option :value="2">UNKNOWN</option>
                          </select>
                        </div>
                        <div class="f-item"><label>年龄 AGE</label><input type="number" v-model.number="editForm.age" /></div>
                        <div class="f-item"><label>出生地 ORIGIN</label><input v-model="editForm.POO" /></div>
                        <div class="f-item full"><label>特殊能力 ABILITY</label><textarea v-model="editForm.ability" rows="4"></textarea></div>
                        <div class="f-item full"><label>背景故事 BACKGROUND</label><textarea v-model="editForm.background" rows="3"></textarea></div>
                        <div class="f-item full"><label>性格设定 CHARACTER</label><textarea v-model="editForm.character" rows="2"></textarea></div>
                        
                        <div class="f-item full">
                          <label>直传新立绘 (Direct Link to COS)</label>
                          <input type="file" @change="e => editForm.newPortrait = e.target.files[0]" accept="image/*" :disabled="upSubmitting" />
                        </div>

                        <div class="f-item full highlight-box">
                          <label>更新摘要 VERSION_LOG (Required)</label>
                          <input v-model="editForm.updateDescription" placeholder="描述本次同步的细节..." :disabled="upSubmitting" />
                        </div>
                      </div>

                      <div class="upload-feedback" v-if="upSubmitting">
                        <div class="feedback-status">{{ uploadProgress < 100 ? 'UPLOADING TO CLOUD_>>' : 'SYNCING TO DATABASE_>>' }}</div>
                        <div class="progress-track">
                          <div class="progress-fill" :style="{ width: uploadProgress + '%' }"></div>
                        </div>
                        <div class="progress-val">{{ uploadProgress }}%</div>
                      </div>

                      <div class="editor-btns">
                        <button class="action-btn-industrial" @click="closeModal" :disabled="upSubmitting">中止同步 ABORT</button>
                        <button class="action-btn-industrial red" @click="submitUpdate" :disabled="upSubmitting">
                          {{ upSubmitting ? 'PROCESSING...' : '提交同步 COMMIT' }}
                        </button>
                      </div>
                    </div>
                  </template>

                </main>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import axios from 'axios' // ✨ 引入原生的 axios 用于向 COS 直传，避免携带后端拦截器的 Token
import apiClient from '@/utils/api'

// --- 基础状态 ---
const rosterLoading = ref(false)
const rosterList = ref([])
const rosterQuery = ref('')
const viewMode = ref('all')
const rosterPage = ref(1)
const rosterSize = 12
const selectedOC = ref(null)
const isEditing = ref(false)
const isCreating = ref(false)
const upSubmitting = ref(false)
const uploadProgress = ref(0) // ✨ 新增：进度条状态
const detailTab = ref('info')

const editForm = reactive({
  name: '', realm: '火柴人', gender: 0, age: 0, species: '', ability: '', 
  POO: '', background: '', character: '', updateDescription: '',
  newPortrait: null
})

// --- 计算属性：分页与过滤 ---
const filteredRoster = computed(() => {
  const q = rosterQuery.value.toLowerCase()
  if (!q) return rosterList.value
  return rosterList.value.filter(oc => oc.name?.toLowerCase().includes(q))
})

const paginatedRoster = computed(() => {
  const start = (rosterPage.value - 1) * rosterSize
  return filteredRoster.value.slice(start, start + rosterSize)
})

const totalPages = computed(() => Math.ceil(filteredRoster.value.length / rosterSize) || 1)

// --- 计算属性：版本逻辑 ---
const sortedVersions = computed(() => {
  if (!selectedOC.value?.Versions) return []
  return [...selectedOC.value.Versions].sort((a, b) => b.versionNumber - a.versionNumber)
})

const currentVersion = computed(() => {
  if (!selectedOC.value?.Versions) return null
  const versions = selectedOC.value.Versions
  return versions.find(v => v.isCurrent) || versions[versions.length - 1]
})

// --- 版本差异算法 ---
const getVersionDiff = (ver, index) => {
  const allVers = sortedVersions.value
  const prevVer = allVers[index + 1]
  if (!prevVer) return []

  const diffs = []
  
  // 👇 1. 在比对字典里加上立绘字段
  const fields = [
    { key: 'name', label: '姓名' }, { key: 'age', label: '年龄' },
    { key: 'species', label: '种族' }, { key: 'ability', label: '能力' },
    { key: 'background', label: '背景' }, { key: 'character', label: '性格' },
    { key: 'POO', label: '出生地' },
    { key: 'OC_image_url', label: '立绘' } // ✨ 新增立绘监听
  ]

  fields.forEach(f => {
    const newVal = ver[f.key] || ''
    const oldVal = prevVer[f.key] || ''
    
    if (newVal !== oldVal) {
      // 👇 2. 如果是立绘更新，做特殊的优雅展示（避免显示一长串URL）
      if (f.key === 'OC_image_url') {
        diffs.push({
          field: f.label,
          old: oldVal ? '旧版影像数据' : '未收录',
          new: newVal ? '新版影像数据' : '未收录'
        })
      } 
      // 其他普通文本字段的展示逻辑
      else {
        // 安全转换为字符串再判断长度
        const oldStr = String(oldVal);
        const newStr = String(newVal);
        diffs.push({
          field: f.label,
          old: oldStr.length > 15 ? oldStr.substring(0, 15) + '...' : oldStr || '空',
          new: newStr.length > 15 ? newStr.substring(0, 15) + '...' : newStr || '空'
        })
      }
    }
  })
  return diffs
}

// --- 业务方法 ---
const loadRoster = async () => {
  rosterLoading.value = true
  const endpoint = viewMode.value === 'all' ? '/OcMaster/all-ocs' : '/OcMaster/my-ocs'
  try {
    const res = await apiClient.get(endpoint)
    if (res.data.success) rosterList.value = res.data.data || []
  } catch (e) { console.error("API_FAILED") }
  finally { rosterLoading.value = false }
}

watch(viewMode, () => { rosterPage.value = 1; loadRoster() })

const openDetail = async (ocSummary) => {
  selectedOC.value = null; isEditing.value = false; isCreating.value = false; detailTab.value = 'info'
  try {
    const res = await apiClient.get(`/OcMaster/${ocSummary.id}`)
    if (res.data.success) selectedOC.value = res.data.data
  } catch (e) { alert("DATA_UNREADABLE") }
}

const startCreate = () => {
  selectedOC.value = null
  isCreating.value = true
  isEditing.value = true 
  Object.assign(editForm, {
    name: '', realm: '火柴人', gender: 0, age: 0, species: '', ability: '', 
    POO: '', background: '', character: '', updateDescription: 'INIT_CREATION: 初始化档案',
    newPortrait: null
  })
}

const startEdit = () => {
  const v = currentVersion.value
  if (!v) return
  Object.assign(editForm, {
    name: v.name, realm: selectedOC.value.realm || '火柴人', gender: v.gender, age: v.age, species: v.species,
    ability: v.ability, POO: v.POO, background: v.background || '',
    character: v.character || '', updateDescription: '', newPortrait: null
  })
  isEditing.value = true
  isCreating.value = false
}

// ✨ 核心重写：引入预签名与进度条直传逻辑
const submitUpdate = async () => {
  if (!editForm.name) return alert("必须填写识别名 (NAME)")
  if (!editForm.updateDescription) return alert("必须填写版本摘要 (VERSION_LOG)")
  
  upSubmitting.value = true
  uploadProgress.value = 0
  let finalCosUrl = null

  try {
    // 步骤1：如果选了新图片，先请求预签名并直传腾讯云
    if (editForm.newPortrait) {
      const file = editForm.newPortrait;
      
      // 1-A. 获取授权签名
      const signRes = await apiClient.get(`/OcMaster/generate-upload-url?fileName=${encodeURIComponent(file.name)}`);
      const { uploadUrl, finalUrl } = signRes.data;

      // 1-B. 直传腾讯云，绑定进度条
      await axios.put(uploadUrl, file, {
        headers: { 'Content-Type': file.type },
        onUploadProgress: (progressEvent) => {
          uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        }
      });
      
      // 记录云端最终地址
      finalCosUrl = finalUrl;
    } else {
      uploadProgress.value = 100; // 没传图片直接满进度进入保存环节
    }

    // 步骤2：向你自己的服务器提交数据 (不再传文件，只传最终 URL)
    const fd = new FormData()
    Object.keys(editForm).forEach(k => { 
      if(k !== 'newPortrait') fd.append(k, editForm[k]) 
    })
    
    if (finalCosUrl) {
      fd.append('OC_image_url', finalCosUrl) // 将链接存入
    }
    
    let res;
    if (isCreating.value) {
      res = await apiClient.post(`/OcMaster/create`, fd)
    } else {
      res = await apiClient.post(`/OcMaster/${selectedOC.value.id}/update`, fd)
    }

    if (res.data.success) {
      alert(isCreating.value ? "ARCHIVE_CREATED" : "SYNC_SUCCESS")
      closeModal()
      loadRoster()
    }
  } catch (e) { 
    console.error(e)
    alert("SYNC_DENIED: 上传或保存失败，请检查 CORS 跨域配置或网络") 
  } finally { 
    upSubmitting.value = false 
    uploadProgress.value = 0
  }
}

const isOwner = (oc) => {
  if (viewMode.value === 'mine') return true
  const myId = Number(localStorage.getItem('userId'))
  return Number(oc.authorID || oc.authorId) === myId
}

const deleteOC = async (oc) => {
  if (confirm(`ERASE ${oc.name}?`)) {
    try {
      const res = await apiClient.delete(`/OcMaster/${oc.id}`)
      if (res.data.success) { closeModal(); loadRoster() }
    } catch (e) { alert("ERASE_FAILED") }
  }
}

// --- 工具函数 ---
const formatAvatar = (url) => {
  if (!url) return '/土豆.jpg'
  return url.startsWith('http') ? url : `https://img.bianyuzhou.com/${url.replace(/^\//, '')}`
}
const calculateWinRate = (oc) => {
  const t = (oc.winCount || 0) + (oc.loseCount || 0)
  return t === 0 ? 0 : Math.round((oc.winCount / t) * 100)
}
const handleImgError = (e) => { e.target.src = '/土豆.jpg' }
const getGenderText = (g) => g === 0 ? 'MALE' : g === 1 ? 'FEMALE' : 'UNKNOWN'
const formatDate = (str) => str ? new Date(str).toLocaleString() : 'N/A'
const closeModal = () => { selectedOC.value = null; isEditing.value = false; isCreating.value = false; upSubmitting.value = false }
const challengeOC = (oc) => alert(`已向 ${oc.name} 发起约战`)

onMounted(() => loadRoster())
</script>

<style scoped>
/* --- 完整战术工业视觉 CSS --- */
.oc-industrial-system {
  --red: #D92323; --black: #111111; --white: #F4F1EA; --gray: #E0DDD5; --neon: #00ff00;
  width: 100%; height: 100%; display: flex; flex-direction: column; gap: 20px;
  font-family: 'JetBrains Mono', monospace; color: var(--black);
}

/* 顶部栏 */
.sys-toolbar { display: flex; justify-content: space-between; align-items: center; border-bottom: 4px solid var(--black); padding-bottom: 15px; }
.toolbar-left { display: flex; gap: 30px; }
.query-unit { background: var(--black); padding: 8px 15px; display: flex; align-items: center; gap: 10px; width: 350px; }
.query-unit input { background: transparent; border: none; color: #fff; outline: none; width: 100%; }
.prefix { color: var(--red); font-weight: bold; font-size: 0.8rem; }
.category-tabs { display: flex; gap: 10px; }
.category-tabs button { background: none; border: 2px solid var(--black); padding: 5px 15px; cursor: pointer; font-weight: bold; }
.category-tabs button.active { background: var(--black); color: #fff; }

/* 状态显示 */
.status-led { display: flex; align-items: center; gap: 8px; font-size: 0.7rem; font-weight: bold; }
.pulse { width: 8px; height: 8px; background: var(--neon); border-radius: 50%; box-shadow: 0 0 8px var(--neon); animation: blink 1.5s infinite; }

/* 网格系统 */
.roster-viewport { flex: 1; overflow-y: auto; padding: 10px; }
.module-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 25px; }
.oc-module-card { background: var(--white); border: 3px solid var(--black); position: relative; cursor: pointer; transition: 0.2s; }
.oc-module-card:hover { transform: translate(-5px, -5px); box-shadow: 8px 8px 0 rgba(0,0,0,0.1); }
.oc-module-card.is-owner { border-color: var(--red); }
.warning-tape { height: 8px; background: repeating-linear-gradient(45deg, #fbbf24, #fbbf24 10px, #000 10px, #000 20px); border-bottom: 2px solid var(--black); }
.card-inner { padding: 15px; }
.portrait-box { aspect-ratio: 1; border: 2px solid var(--black); position: relative; overflow: hidden; background: #eee; }
.portrait-box img { width: 100%; height: 100%; object-fit: cover; }
.scan-line { position: absolute; top: 0; width: 100%; height: 2px; background: var(--red); opacity: 0.5; animation: scan 3s infinite linear; }
.id-tag { position: absolute; top: 0; right: 0; background: var(--black); color: #fff; font-size: 0.6rem; padding: 2px 8px; }

/* 角色信息文字 */
.oc-name { font-size: 1.2rem; font-weight: 900; margin: 10px 0 2px 0; }
.auth-id { font-size: 0.65rem; color: #666; margin-bottom: 10px; }
.energy-track { height: 8px; background: #ddd; border: 2px solid var(--black); margin-top: 5px; }
.energy-fill { height: 100%; background: var(--black); }
.is-owner .energy-fill { background: var(--red); }
.owner-flag { position: absolute; bottom: 0; right: 0; background: var(--red); color: #fff; font-size: 0.6rem; padding: 2px 8px; font-weight: bold; }

/* 分页 */
.pagination-footer { display: flex; justify-content: center; align-items: center; gap: 20px; padding: 20px; border-top: 3px solid var(--black); }
.nav-btn { background: var(--black); color: #fff; border: none; padding: 8px 20px; cursor: pointer; font-weight: bold; }
.nav-btn:disabled { opacity: 0.3; }

/* 弹窗核心布局 */
.terminal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); display: flex; justify-content: center; align-items: center; z-index: 9999; }
.terminal-frame { width: 1100px; max-width: 95vw; height: 85vh; background: var(--white); border: 5px solid var(--black); display: flex; flex-direction: column; }
.term-header { background: var(--black); color: #fff; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; }
.term-close { background: var(--red); color: #fff; border: none; padding: 5px 15px; cursor: pointer; font-weight: bold; }
.term-layout { display: flex; height: 100%; overflow: hidden; }
.term-aside { width: 350px; background: var(--gray); border-right: 5px solid var(--black); padding: 25px; display: flex; flex-direction: column; gap: 20px; }
.focus-portrait { aspect-ratio: 1; border: 3px solid var(--black); background: #000; overflow: hidden; }
.focus-portrait img { width: 100%; height: 100%; object-fit: contain; }
.combat-matrix { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
.matrix-box { background: #fff; border: 2px solid var(--black); padding: 10px; text-align: center; }
.matrix-box label { font-size: 0.6rem; font-weight: bold; color: #888; display: block; }
.matrix-box .val { font-size: 1.8rem; font-weight: 900; }
.val.green { color: #2ecc71; } .val.red { color: var(--red); }

.term-main { flex: 1; padding: 35px; background: #fff; overflow-y: auto; }
.term-tabs { display: flex; gap: 20px; border-bottom: 4px solid #eee; margin-bottom: 25px; }
.term-tabs button { background: none; border: none; font-weight: 900; color: #aaa; cursor: pointer; padding-bottom: 10px; font-size: 1rem; }
.term-tabs button.active { color: var(--red); border-bottom: 4px solid var(--red); margin-bottom: -4px; }

/* 数据展示 */
.group-label { background: var(--black); color: #fff; padding: 3px 12px; font-size: 0.8rem; font-weight: bold; margin-bottom: 15px; display: inline-block; }
.metric-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 30px; }
.m-item { border-bottom: 1px solid #eee; padding-bottom: 5px; }
.m-item .k { font-size: 0.65rem; color: #999; margin-right: 10px; }
.m-item .v { font-weight: bold; }
.text-block { background: #f9f9f9; padding: 15px; border-left: 5px solid var(--black); white-space: pre-wrap; line-height: 1.6; margin-bottom: 20px; }

/* 日志与差异 */
.log-card { border: 2px solid #eee; padding: 15px; margin-bottom: 15px; }
.log-card.current { border-left: 5px solid var(--red); background: #fff9f9; }
.v-diff-box { margin-top: 10px; background: #111; padding: 12px; border-left: 4px solid var(--red); }
.diff-line { font-size: 0.75rem; color: #ccc; font-family: monospace; margin-bottom: 5px; display: flex; gap: 10px; }
.diff-field { color: var(--red); font-weight: bold; min-width: 60px; }
.diff-old { color: #777; text-decoration: line-through; }
.diff-new { color: #2ecc71; font-weight: bold; }

/* 编辑器样式 */
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
.f-item { display: flex; flex-direction: column; gap: 5px; }
.f-item.full { grid-column: span 2; }
.f-item label { font-size: 0.7rem; font-weight: bold; color: #666; }
.f-item input, .f-item textarea, .f-item select { border: 2px solid var(--black); padding: 10px; font-family: inherit; }
.editor-btns { display: flex; gap: 15px; margin-top: 20px; justify-content: flex-end; }

/* 按钮通用 */
.btn-block { width: 100%; padding: 12px; border: none; font-weight: bold; cursor: pointer; margin-bottom: 5px; }
.btn-block.red { background: var(--red); color: #fff; }
.btn-block.hollow { background: transparent; border: 2px solid var(--red); color: var(--red); }
.action-btn-industrial { background: var(--black); color: #fff; border: none; padding: 8px 20px; cursor: pointer; font-weight: bold; }
.action-btn-industrial.red { background: var(--red); }
.action-btn-industrial:disabled { opacity: 0.5; cursor: not-allowed; }

/* ✨ 新增：工业风上传进度条 CSS */
.upload-feedback {
  margin-top: 25px;
  background: var(--black);
  padding: 15px;
  border: 2px solid var(--red);
  color: #fff;
  position: relative;
}
.feedback-status {
  font-size: 0.75rem;
  font-weight: bold;
  margin-bottom: 8px;
  color: var(--neon);
  animation: blink 1.5s infinite;
}
.progress-track {
  width: 100%;
  height: 10px;
  background: #333;
  border: 1px solid #555;
  overflow: hidden;
}
.progress-fill {
  height: 100%;
  background: var(--neon);
  transition: width 0.1s linear;
  box-shadow: 0 0 10px var(--neon);
}
.progress-val {
  position: absolute;
  right: 15px;
  top: 15px;
  font-size: 1.2rem;
  font-weight: 900;
  color: #fff;
}

@keyframes scan { 0% { top: 0; } 100% { top: 100%; } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>