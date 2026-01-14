<template>
  <div class="battlefield-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="bf-container">
      
      <header class="tactical-header">
        <div class="header-left">
          <div class="logo-group">
            <span class="icon-box">BATTLE</span>
            <div class="text-info">
              <h2 class="title">BATTLEFIELD_OS</h2>
              <span class="sub">混合成长型企划平台 // OC_DATABASE</span>
            </div>
          </div>
        </div>
        
        <nav class="tactical-tabs">
          <button 
            v-for="t in tabs" 
            :key="t.key"
            class="cyber-tab-btn"
            :class="{ active: activeTab === t.key }"
            @click="activeTab = t.key"
          >
            <span class="tab-deco"></span>
            <span class="tab-text">{{ t.label }}</span>
          </button>
        </nav>
      </header>

      <div class="bf-main-viewport custom-scroll">
        
        <Transition name="glitch-fade" mode="out-in">
          <section v-if="activeTab === 'intro'" class="module-intro">
            <div class="briefing-terminal">
              <div class="term-header">>> MISSION_BRIEFING // 企划简报</div>
              <div class="term-content">
                <h3 class="hero-title">WELCOME_TO_THE_ARENA</h3>
                <p class="hero-desc">这是一个由社区驱动的长期运营企划，旨在连接每一位火柴人创作者。</p>
                <div class="alert-strip">
                  <span class="icon">!</span>
                  <span>SYSTEM_NOTICE: 当前版本支持人设上传、展示与作者本人修订。</span>
                </div>
              </div>
            </div>
            <div class="action-deck">
              <button class="cyber-btn primary large" @click="activeTab = 'upload'">
                >>> REGISTER_NEW_OC
              </button>
              <button class="cyber-btn ghost large" @click="activeTab = 'roster'">
                VIEW_ROSTER
              </button>
            </div>
          </section>

          <section v-else-if="activeTab === 'upload'" class="module-upload">
            <div class="form-panel">
              <div class="panel-header">
                <span class="deco">▼</span> NEW_RECRUIT_REGISTRY // 档案登记
              </div>
              <form @submit.prevent="submitOC" class="cyber-form">
                <div class="form-row">
                  <div class="form-group half">
                    <label>CODENAME / 角色名称 <span class="req">*</span></label>
                    <input v-model="upForm.OCName" class="cyber-input" placeholder="ENTER_NAME..." required />
                  </div>
                  <div class="form-group half">
                    <label>CREATOR / 作者署名 <span class="req">*</span></label>
                    <input v-model="upForm.authorName" class="cyber-input" placeholder="YOUR_ID..." required />
                  </div>
                </div>
                <div class="form-row three-col">
                  <div class="form-group">
                    <label>GENDER</label>
                    <select v-model="upForm.gender" class="cyber-select" required>
                      <option value="">SELECT...</option>
                      <option value="0">MALE</option>
                      <option value="1">FEMALE</option>
                      <option value="2">UNKNOWN</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>SPECIES</label>
                    <input v-model="upForm.species" class="cyber-input" required />
                  </div>
                  <div class="form-group">
                    <label>AGE</label>
                    <input v-model.number="upForm.age" type="number" class="cyber-input" required />
                  </div>
                </div>
                <div class="form-group">
                  <label>ABILITY_DATA / 能力设定</label>
                  <textarea v-model="upForm.ability" class="cyber-textarea" rows="5" required></textarea>
                </div>
                <div class="form-group">
                  <label>P.O.O (Place of Origin)</label>
                  <input v-model="upForm.poo" class="cyber-input" required />
                </div>
                <div class="form-group">
                  <label>VISUAL_ASSET / 立绘</label>
                  <div class="upload-zone" @click="$refs.charInput.click()">
                    <div v-if="upForm.charFile" class="file-preview">
                      <span class="file-name">FILE: {{ upForm.charFile.name }}</span>
                      <button type="button" @click.stop="upForm.charFile = null" class="remove-btn">[REMOVE]</button>
                    </div>
                    <div v-else class="placeholder">
                      <div class="icon">+</div>
                      <span>UPLOAD_IMAGE (MAX 5MB)</span>
                    </div>
                  </div>
                  <input ref="charInput" type="file" accept="image/*" style="display:none" @change="handleCharFile" />
                </div>
                <div class="form-group checkbox-row">
                  <label class="cyber-checkbox">
                    <input type="checkbox" v-model="upForm.agreed" required />
                    <span class="checkmark"></span>
                    <span class="text">I_ACCEPT_THE_RULES</span>
                  </label>
                </div>
                <div class="form-actions">
                  <button class="cyber-btn primary full" type="submit" :disabled="upSubmitting">
                    {{ upSubmitting ? 'UPLOADING...' : 'SUBMIT_DOSSIER' }}
                  </button>
                </div>
              </form>
            </div>
          </section>

          <section v-else-if="activeTab === 'roster'" class="module-roster">
            <div class="roster-toolbar">
              <div class="search-unit">
                <span class="prompt">> QUERY:</span>
                <input v-model="rosterQuery" class="search-input" placeholder="NAME / AUTHOR..." @input="rosterPage = 1" />
              </div>
              <button class="cyber-btn ghost" @click="loadRoster">REFRESH_DB</button>
            </div>

            <div v-if="rosterLoading" class="loading-state">
              <div class="spinner"></div><span>ACCESSING_DATABASE...</span>
            </div>
            
            <div v-else class="roster-grid">
              <div 
                v-for="oc in paginatedRoster" 
                :key="oc.id" 
                class="dossier-card"
                @click="openDetail(oc)"
              >
                <div class="card-status-bar">
                  <span class="status-indicator" :class="getStatusClass(oc.OC_status)">
                    {{ getStatusText(oc.OC_status) }}
                  </span>
                  <span v-if="isOwner(oc)" class="owner-badge">[ MY_OC ]</span>
                </div>
                <div class="card-portrait">
                  <img :src="getImageUrl(oc.imageUrl || oc.OC_image_url)" loading="lazy" @error="handleImgError" />
                  <div class="scan-overlay"></div>
                </div>
                <div class="card-info">
                  <h4 class="oc-name">{{ oc.name }}</h4>
                  <div class="oc-meta">
                    <span>AUTH: {{ oc.authorName }}</span>
                    <span>RACE: {{ oc.species }}</span>
                  </div>
                  <div class="oc-stats">
                    <div class="stat-bar win" :style="{width: (oc.winCount/(oc.winCount+oc.loseCount+1)*100)+'%' }"></div>
                    <span class="stat-text">W:{{ oc.winCount || 0 }} / L:{{ oc.loseCount || 0 }}</span>
                  </div>
                </div>
                <div class="corner-br"></div>
              </div>
            </div>

            <div class="pagination-bar" v-if="totalPages > 1">
              <button class="nav-btn" :disabled="rosterPage <= 1" @click="rosterPage--">&lt;</button>
              <span class="page-num">{{ rosterPage }} / {{ totalPages }}</span>
              <button class="nav-btn" :disabled="rosterPage >= totalPages" @click="rosterPage++">&gt;</button>
            </div>
          </section>

          <section v-else-if="activeTab === 'records'" class="module-records">
            <div class="cyber-table-wrapper">
              <table class="cyber-table">
                <thead>
                  <tr><th>DATE</th><th>OPERATION</th><th>INITIATOR</th><th>DEFENDER</th><th>OUTCOME</th></tr>
                </thead>
                <tbody>
                  <tr><td colspan="5" class="empty-row">[ NO_COMBAT_LOGS_RECORDED ]</td></tr>
                </tbody>
              </table>
            </div>
          </section>
        </Transition>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="glitch-fade">
        <div v-if="detailLoading || selectedOC" class="cyber-modal-overlay" @click.self="closeModal">
          
          <div v-if="detailLoading" class="loading-modal">
            <div class="spinner"></div>
            <span>DECRYPTING_FILE...</span>
          </div>

          <div v-else class="cyber-terminal-window wide">
            
            <div class="term-header">
              <div class="header-title">
                <span class="deco-dot"></span>
                <span class="title-text">
                  {{ isEditing ? '>> EDIT_PROTOCOL' : '>> DOSSIER_VIEWER' }} // {{ isEditing ? editForm.name : selectedOC.name }}
                </span>
              </div>
              <button class="term-close" @click="closeModal">[ CLOSE / ESC ]</button>
            </div>

            <div class="term-body custom-scroll">
              
              <div v-if="!isEditing" class="detail-layout">
                
                <div class="detail-col-left">
                  <div class="portrait-display">
                    <img :src="getImageUrl(selectedOC.imageUrl || selectedOC.OC_image_url)" @error="handleImgError" />
                    <div class="frame-decor"></div>
                  </div>
                  
                  <div class="combat-matrix">
                    <div class="matrix-item">
                      <span class="m-label">WINS</span>
                      <span class="m-val win-color">{{ selectedOC.winCount || 0 }}</span>
                    </div>
                    <div class="matrix-divider"></div>
                    <div class="matrix-item">
                      <span class="m-label">LOSSES</span>
                      <span class="m-val lose-color">{{ selectedOC.loseCount || 0 }}</span>
                    </div>
                  </div>
                </div>

                <div class="detail-col-right custom-scroll">
                  
                  <div class="info-block basic">
                    <h3 class="block-head"># BASIC_INTEL // 基础档案</h3>
                    <div class="info-grid">
                      <div class="grid-item">
                        <span class="label">CREATOR</span>
                        <span class="value">{{ selectedOC.authorName }}</span>
                      </div>
                      <div class="grid-item">
                        <span class="label">SPECIES</span>
                        <span class="value">{{ selectedOC.species }}</span>
                      </div>
                      <div class="grid-item">
                        <span class="label">GENDER</span>
                        <span class="value">{{ getGenderText(selectedOC.gender) }}</span>
                      </div>
                      <div class="grid-item">
                        <span class="label">AGE</span>
                        <span class="value">{{ selectedOC.age }}</span>
                      </div>
                      <div class="grid-item full">
                        <span class="label">P.O.O (ORIGIN)</span>
                        <span class="value highlight">{{ selectedOC.POO || selectedOC.poo }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="info-block ability">
                    <h3 class="block-head"># ABILITY_SPECS // 能力设定</h3>
                    <div class="text-body">
                      {{ selectedOC.ability }}
                    </div>
                  </div>

                  <div class="info-block background" v-if="selectedOC.background">
                    <h3 class="block-head"># BACKGROUND_LOG // 背景故事</h3>
                    <div class="text-body">
                      {{ selectedOC.background }}
                    </div>
                  </div>

                  <div class="info-block armory" v-if="selectedOC.weaponImages && selectedOC.weaponImages.length">
                    <h3 class="block-head"># VISUAL_ARMORY // 武器库</h3>
                    <div class="weapon-grid">
                      <div v-for="(wp, idx) in selectedOC.weaponImages" :key="idx" class="weapon-slot">
                        <img :src="getImageUrl(wp)" @click="openImage(wp)" />
                      </div>
                    </div>
                  </div>

                </div>
              </div>

              <div v-else class="edit-layout">
                <div class="form-row">
                  <div class="form-group half"><label>NAME:</label><input v-model="editForm.name" class="cyber-input" /></div>
                  <div class="form-group half"><label>SPECIES:</label><input v-model="editForm.species" class="cyber-input" /></div>
                </div>
                <div class="form-row">
                  <div class="form-group"><label>GENDER (0:M, 1:F, 2:?):</label><select v-model="editForm.gender" class="cyber-select"><option :value="0">MALE</option><option :value="1">FEMALE</option><option :value="2">UNKNOWN</option></select></div>
                  <div class="form-group"><label>AGE:</label><input v-model.number="editForm.age" type="number" class="cyber-input" /></div>
                </div>
                <div class="form-group"><label>P.O.O:</label><input v-model="editForm.POO" class="cyber-input" /></div>
                <div class="form-group"><label>ABILITY:</label><textarea v-model="editForm.ability" class="cyber-textarea" rows="4"></textarea></div>
                <div class="form-group"><label>BACKGROUND:</label><textarea v-model="editForm.background" class="cyber-textarea" rows="3"></textarea></div>
                <div class="form-group"><label>UPDATE_IMAGE:</label><input type="file" @change="handleEditFileChange" accept="image/*" class="cyber-input file" /></div>
                <div class="form-group"><label>UPDATE_NOTE:</label><input v-model="editForm.updateDescription" class="cyber-input" /></div>
              </div>

            </div>

            <div class="term-footer">
              <template v-if="!isEditing">
                <div class="debug-info">[ID:{{ selectedOC?.id }}]</div>
                <div class="btn-group">
                  <button v-if="isOwner(selectedOC)" class="cyber-btn secondary" @click="startEdit">
                    EDIT_FILE
                  </button>
                  <button class="cyber-btn primary" @click="challengeOC(selectedOC)">
                    INITIATE_BATTLE
                  </button>
                </div>
              </template>
              <template v-else>
                <button class="cyber-btn ghost" @click="cancelEdit" :disabled="upSubmitting">CANCEL</button>
                <button class="cyber-btn primary" @click="submitUpdate" :disabled="upSubmitting">
                  {{ upSubmitting ? 'SAVING...' : 'CONFIRM_UPDATE' }}
                </button>
              </template>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

// --- 逻辑代码完全保持一致 ---
const tabs = [{ key: 'intro', label: 'BRIEFING' },{ key: 'upload', label: 'UPLOAD' },{ key: 'roster', label: 'ROSTER' },{ key: 'records', label: 'LOGS' }]
const activeTab = ref('intro')
const currentUserId = ref(null)

const getUserFromToken = () => {
  const token = localStorage.getItem('auth_token')
  if (!token) return null
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(window.atob(base64))
    const nameId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'] || payload['id'] || payload['UserId']
    return nameId ? parseInt(nameId) : null
  } catch (e) { return null }
}

const upSubmitting = ref(false)
const upForm = reactive({ OCName: '', authorName: '', gender: '', age: '', species: '', ability: '', poo: '', agreed: false, charFile: null })
const handleCharFile = (e) => { const file = e.target.files[0]; if(file && file.size <= 5*1024*1024) upForm.charFile = file; else alert('FILE_ERROR') }
const submitOC = async () => {
  if(!upForm.agreed) return alert('ACCEPT_RULES_REQUIRED')
  upSubmitting.value = true
  try {
    const fd = new FormData()
    fd.append('OCName', upForm.OCName); fd.append('authorName', upForm.authorName); fd.append('gender', upForm.gender); fd.append('age', upForm.age); fd.append('species', upForm.species); fd.append('ability', upForm.ability); fd.append('POO', upForm.poo); fd.append('currentTime', Math.floor(Date.now()/1000).toString());
    if(upForm.charFile) fd.append('CharacterImage', upForm.charFile)
    await apiClient.post('/OCBattleField/upload', fd, { headers: {'Content-Type': 'multipart/form-data'} })
    alert('UPLOAD_COMPLETE')
    Object.assign(upForm, { OCName:'', authorName:'', gender:'', age:'', species:'', ability:'', poo:'', agreed:false, charFile:null })
    activeTab.value = 'roster'; loadRoster()
  } catch(e) { alert('ERROR') } finally { upSubmitting.value = false }
}

const rosterLoading = ref(false)
const rosterList = ref([])
const rosterQuery = ref('')
const rosterPage = ref(1)
const rosterSize = 12

const selectedOC = ref(null)
const detailLoading = ref(false)
const isEditing = ref(false)
const editForm = reactive({ name: '', gender: 0, age: 0, species: '', ability: '', POO: '', background: '', updateDescription: '', newImage: null })

const loadRoster = async () => {
  rosterLoading.value = true
  try {
    const res = await apiClient.get('/ocbattlefield/list')
    if(res.data.success) rosterList.value = res.data.data.items || res.data.data || []
  } catch(e) {} finally { rosterLoading.value = false }
}

const filteredRoster = computed(() => {
  if(!rosterQuery.value) return rosterList.value
  const q = rosterQuery.value.toLowerCase()
  return rosterList.value.filter(oc => oc.name?.toLowerCase().includes(q) || oc.authorName?.toLowerCase().includes(q))
})
const paginatedRoster = computed(() => {
  const start = (rosterPage.value - 1) * rosterSize
  return filteredRoster.value.slice(start, start + rosterSize)
})
const totalPages = computed(() => Math.ceil(filteredRoster.value.length / rosterSize) || 1)

const getImageUrl = (url) => {
  if(!url) return '/土豆.jpg'
  if (url.startsWith('http') || url.startsWith('blob:')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  return `https://bianyuzhou.com/${path}`;
}
const handleImgError = (e) => e.target.src = '/土豆.jpg'
const getStatusText = (s) => ['IDLE', 'COMBAT', 'CLOSED', 'BANNED'][s] || 'UNKNOWN'
const getStatusClass = (s) => ['s-idle', 's-busy', 's-closed', 's-banned'][s] || ''
const getGenderText = (g) => g === 0 ? 'MALE' : g === 1 ? 'FEMALE' : 'UNKNOWN'
const openImage = (url) => window.open(getImageUrl(url), '_blank')
const isOwner = (oc) => {
  if (!oc || !currentUserId.value) return false
  const ocAuthorId = oc.authorId || oc.authorID || oc.AuthorId
  return parseInt(ocAuthorId) === parseInt(currentUserId.value)
}

const openDetail = async (ocSummary) => {
  selectedOC.value = null; detailLoading.value = true; isEditing.value = false
  try {
    const res = await apiClient.get(`/OCBattleField/${ocSummary.id}`)
    if (res.data.success) selectedOC.value = res.data.data
    else alert(res.data.message)
  } catch (e) { alert("DATA_CORRUPT") } finally { detailLoading.value = false }
}
const closeModal = () => { selectedOC.value = null; isEditing.value = false }

const startEdit = () => {
  if (!selectedOC.value) return
  const oc = selectedOC.value
  Object.assign(editForm, { name: oc.name, gender: oc.gender, age: oc.age, species: oc.species, ability: oc.ability, POO: oc.POO || oc.poo, background: oc.background || '', updateDescription: '', newImage: null })
  isEditing.value = true
}
const cancelEdit = () => isEditing.value = false
const handleEditFileChange = (e) => { const file = e.target.files[0]; if(file) editForm.newImage = file }
const submitUpdate = async () => {
  if (!editForm.name) return alert("NAME_REQUIRED")
  upSubmitting.value = true
  try {
    const fd = new FormData()
    fd.append('name', editForm.name); fd.append('gender', editForm.gender); fd.append('age', editForm.age); fd.append('species', editForm.species); fd.append('ability', editForm.ability); fd.append('POO', editForm.POO); fd.append('background', editForm.background); fd.append('updateDescription', editForm.updateDescription || 'AUTO_UPDATE')
    if (editForm.newImage) fd.append('CharacterImage', editForm.newImage)
    const targetId = selectedOC.value.ocId || selectedOC.value.id
    const res = await apiClient.post(`/OCBattleField/${targetId}/update`, fd, { headers: {'Content-Type': 'multipart/form-data'} })
    if (res.data.success) { alert("UPDATE_SUCCESS"); isEditing.value = false; loadRoster(); openDetail({ id: targetId }) }
  } catch(e) { alert('UPDATE_FAILED') } finally { upSubmitting.value = false }
}
const challengeOC = (oc) => alert(`INITIATING_BATTLE_PROTOCOL... TARGET: ${oc.name}`)

onMounted(() => {
  currentUserId.value = getUserFromToken()
  loadRoster()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.battlefield-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --gray: #E0DDD5; 
  --green: #2ecc71;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%; height: 100%;
  font-family: var(--mono); color: var(--black);
  position: relative; overflow: hidden;
  display: flex; flex-direction: column;
}

/* ... (背景、Container、Header、Tabs 样式保持不变) ... */
.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); background-size: 40px 40px; z-index: 0; pointer-events: none; }
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }
.bf-container { position: relative; z-index: 1; flex: 1; display: flex; flex-direction: column; padding: 20px; gap: 20px; overflow-y: scroll;}
.tactical-header { background: var(--white); border: 4px solid var(--black); padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; box-shadow: 8px 8px 0 rgba(0,0,0,0.1); }
.logo-group { display: flex; align-items: center; gap: 15px; }
.icon-box { background: var(--black); color: var(--white); font-family: var(--heading); font-size: 1.5rem; padding: 5px 10px; }
.text-info h2 { font-family: var(--heading); font-size: 2rem; margin: 0; line-height: 1; }
.text-info .sub { font-size: 0.7rem; font-weight: bold; color: #666; }
.tactical-tabs { display: flex; gap: 10px; }
.cyber-tab-btn { background: transparent; border: 2px solid var(--black); padding: 8px 16px; cursor: pointer; display: flex; align-items: center; gap: 5px; font-family: var(--mono); font-weight: bold; transition: 0.2s; }
.cyber-tab-btn:hover { background: #eee; }
.cyber-tab-btn.active { background: var(--black); color: var(--white); box-shadow: 4px 4px 0 var(--red); }
.tab-deco { width: 6px; height: 6px; background: var(--red); display: none; }
.cyber-tab-btn.active .tab-deco { display: block; }
.bf-main-viewport { flex: 1; overflow-y: auto; padding-right: 5px; }

/* ... (Intro, Upload, Roster 样式保持不变) ... */
.briefing-terminal { background: var(--black); color: var(--white); padding: 30px; border: 2px solid var(--black); margin-bottom: 20px; }
.term-header { border-bottom: 1px dashed #555; padding-bottom: 10px; margin-bottom: 15px; color: var(--red); font-weight: bold; }
.hero-title { font-family: var(--heading); font-size: 3rem; margin: 0; line-height: 1.1; }
.hero-desc { font-size: 1.1rem; color: #ccc; margin: 10px 0 20px; }
.alert-strip { background: rgba(255,255,255,0.1); padding: 10px; display: flex; gap: 10px; align-items: center; font-size: 0.9rem; }
.alert-strip .icon { background: var(--red); color: #fff; width: 20px; height: 20px; display: flex; align-items: center; justify-content: center; font-weight: bold; }
.info-matrix { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 30px; }
.matrix-cell { background: var(--white); border: 2px solid var(--black); padding: 20px; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.cell-icon { font-size: 2rem; margin-bottom: 10px; }
.matrix-cell h4 { font-weight: bold; color: var(--red); margin: 0 0 5px 0; border-bottom: 2px solid var(--black); display: inline-block; }
.matrix-cell p { font-size: 0.9rem; color: #555; margin: 5px 0 0 0; }
.action-deck { display: flex; justify-content: center; gap: 20px; }
.module-upload { display: flex; justify-content: center; }
.form-panel { width: 100%; max-width: 800px; background: var(--white); border: 4px solid var(--black); padding: 30px; box-shadow: 10px 10px 0 rgba(0,0,0,0.2); }
.panel-header { font-family: var(--heading); font-size: 1.5rem; border-bottom: 4px solid var(--black); padding-bottom: 15px; margin-bottom: 25px; }
.deco { color: var(--red); margin-right: 10px; }
.cyber-form { display: flex; flex-direction: column; gap: 20px; }
.form-row { display: flex; gap: 20px; }
.form-row.three-col { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px; }
.form-group { flex: 1; }
.form-group label { display: block; font-weight: bold; margin-bottom: 8px; font-size: 0.8rem; }
.req { color: var(--red); }
.cyber-input, .cyber-select, .cyber-textarea { width: 100%; border: 2px solid var(--black); background: #fff; padding: 10px; font-family: var(--mono); outline: none; transition: 0.2s; box-sizing: border-box; }
.cyber-input:focus, .cyber-select:focus, .cyber-textarea:focus { background: #000000; border-color: var(--red); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.upload-zone { border: 2px dashed var(--black); padding: 20px; text-align: center; cursor: pointer; background: #eee; transition: 0.2s; }
.upload-zone:hover { background: #fff; border-color: var(--red); }
.file-preview { display: flex; justify-content: space-between; background: var(--black); color: var(--white); padding: 5px 10px; }
.remove-btn { background: none; border: none; color: var(--red); cursor: pointer; font-weight: bold; }
.placeholder .icon { font-size: 2rem; font-weight: bold; }
.cyber-checkbox { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.cyber-checkbox input { display: none; }
.checkmark { width: 20px; height: 20px; border: 2px solid var(--black); display: inline-block; position: relative; }
.cyber-checkbox input:checked + .checkmark { background: var(--red); }
.cyber-checkbox input:checked + .checkmark::after { content: '✓'; color: white; position: absolute; left: 3px; top: -2px; font-weight: bold; }
.roster-toolbar { display: flex; justify-content: space-between; margin-bottom: 20px; align-items: center; }
.search-unit { display: flex; align-items: center; gap: 10px; background: var(--white); border: 2px solid var(--black); padding: 5px 10px; width: 400px; }
.search-unit .prompt { color: var(--red); font-weight: bold; }
.search-input { border: none; outline: none; width: 100%; font-family: var(--mono); font-weight: bold; }
.roster-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; padding-bottom: 30px; }
.dossier-card { background: var(--white); border: 2px solid var(--black); position: relative; overflow: hidden; cursor: pointer; box-shadow: 6px 6px 0 rgba(0,0,0,0.1); transition: all 0.2s; }
.dossier-card:hover { transform: translateY(-4px); box-shadow: 8px 8px 0 var(--red); border-color: var(--black); }
.card-status-bar { display: flex; justify-content: space-between; padding: 5px 10px; background: var(--black); color: var(--white); font-size: 0.7rem; }
.status-indicator { font-weight: bold; }
.s-idle { color: var(--green); } .s-busy { color: var(--red); }
.owner-badge { color: #f1c40f; }
.card-portrait { height: 200px; background: #333; position: relative; border-bottom: 2px solid var(--black); display: flex; justify-content: center; align-items: center; overflow: hidden; }
.card-portrait img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; }
.dossier-card:hover img { transform: scale(1.1); }
.scan-overlay { position: absolute; inset: 0; pointer-events: none; background-image: linear-gradient(rgba(0,255,0,0.1) 1px, transparent 1px); background-size: 100% 4px; opacity: 0.3; }
.card-info { padding: 15px; }
.oc-name { font-family: var(--heading); font-size: 1.5rem; margin: 0 0 5px 0; line-height: 1; }
.oc-meta { font-size: 0.7rem; color: #666; font-weight: bold; margin-bottom: 10px; display: flex; justify-content: space-between; }
.oc-stats { display: flex; align-items: center; gap: 10px; font-size: 0.7rem; font-weight: bold; }
.stat-bar { height: 6px; background: #eee; flex: 1; border: 1px solid var(--black); position: relative; }
.stat-bar.win::after { content:''; position: absolute; left: 0; top: 0; height: 100%; background: var(--green); width: var(--width, 50%); }
.corner-br { position: absolute; bottom: 0; right: 0; width: 0; height: 0; border-style: solid; border-width: 0 0 15px 15px; border-color: transparent transparent var(--black) transparent; }
.pagination-bar { display: flex; justify-content: center; align-items: center; gap: 15px; }
.nav-btn { background: var(--white); border: 2px solid var(--black); font-weight: bold; cursor: pointer; width: 30px; height: 30px; }
.nav-btn:hover:not(:disabled) { background: var(--black); color: var(--white); }
.cyber-table-wrapper { border: 2px solid var(--black); background: var(--white); }
.cyber-table { width: 100%; border-collapse: collapse; text-align: left; }
.cyber-table th { background: #eee; border-bottom: 2px solid var(--black); padding: 10px; font-weight: bold; }
.cyber-table td { padding: 10px; border-bottom: 1px solid #ccc; font-size: 0.9rem; }
.empty-row { text-align: center; padding: 40px; color: #888; }
.cyber-btn { border: 2px solid var(--black); padding: 10px 20px; font-family: var(--heading); font-size: 1.1rem; cursor: pointer; background: var(--white); transition: 0.2s; }
.cyber-btn:hover { box-shadow: 4px 4px 0 var(--black); transform: translate(-2px, -2px); }
.cyber-btn.primary { background: var(--black); color: var(--white); }
.cyber-btn.primary:hover { background: var(--red); }
.cyber-btn.ghost { background: transparent; }
.cyber-btn.large { font-size: 1.5rem; padding: 15px 30px; }
.cyber-btn.full { width: 100%; }
.loading-state, .loading-modal { text-align: center; padding: 50px; font-weight: bold; color: #888; display: flex; flex-direction: column; align-items: center; }
.spinner { width: 30px; height: 30px; border: 4px solid #ccc; border-top-color: var(--black); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 10px; }
.empty-state { text-align: center; padding: 50px; color: #aaa; border: 2px dashed #ccc; font-weight: bold; }

/* =========================================
   🔥 [重点修改] 详情弹窗样式 (High Contrast Dark Mode)
   ========================================= */

.cyber-modal-overlay { 
  position: fixed; inset: 0; background: rgba(0,0,0,0.9); z-index: 2000; 
  display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); 
}

/* 弹窗容器：黑底，白字，高对比 */
.cyber-terminal-window { 
  width: 1000px; max-width: 95vw; height: 90vh; 
  background: #111; /* 纯黑背景 */
  border: 2px solid #666; /* 明显的边框 */
  display: flex; flex-direction: column; 
  box-shadow: 0 0 50px rgba(0,0,0,0.8);
  color: #eee;
}

/* 顶部栏 */
.term-header { 
  background: #222; padding: 15px 20px; 
  display: flex; justify-content: space-between; align-items: center; 
  border-bottom: 2px solid var(--red); 
}
.term-title { font-family: var(--heading); letter-spacing: 1px; font-size: 1.4rem; color: #fff; }
.title-text { text-shadow: 0 0 10px rgba(255, 255, 255, 0.2); }
.term-close { 
  background: transparent; border: 1px solid #777; color: #ccc; 
  cursor: pointer; font-family: var(--mono); font-weight: bold; padding: 6px 12px;
}
.term-close:hover { border-color: var(--red); color: var(--red); }

.term-body { 
  flex: 1; 
  overflow-y: auto; /* 确保这里允许滚动 */
  padding: 0; 
  display: flex; 
  flex-direction: column; /* 确保内部布局方向明确 */
  min-height: 0; /* 关键：防止Flex子元素撑破容器导致无法滚动 */
}

/* 布局：左右分栏 */
/* 布局：左右分栏 */
.detail-layout { 
  display: flex; 
  width: 100%; 
  height: 100%; /* 桌面端保持全高，让右侧独立滚动 */
}

/* 左侧：立绘与战绩 */
.detail-col-left { 
  width: 350px; flex-shrink: 0; display: flex; flex-direction: column; 
  border-right: 2px solid #333; background: #000;
}
.portrait-display { 
  flex: 1; position: relative; display: flex; justify-content: center; align-items: center; overflow: hidden;
  background-image: linear-gradient(#222 1px, transparent 1px), linear-gradient(90deg, #222 1px, transparent 1px);
  background-size: 20px 20px;
}
.portrait-display img { max-width: 95%; max-height: 95%; object-fit: contain; z-index: 1; }
/* 战绩 */
.combat-matrix { 
  padding: 20px; background: #1a1a1a; border-top: 2px solid #333;
  display: flex; justify-content: space-around; align-items: center;
}
.matrix-item { text-align: center; }
.m-label { display: block; font-size: 0.7rem; color: #888; font-weight: bold; margin-bottom: 5px; }
.m-val { font-family: var(--heading); font-size: 2.2rem; line-height: 1; }
.win-color { color: var(--green); text-shadow: 0 0 10px rgba(46, 204, 113, 0.4); }
.lose-color { color: var(--red); text-shadow: 0 0 10px rgba(217, 35, 35, 0.4); }
.matrix-divider { width: 1px; height: 40px; background: #444; }

/* 右侧：数据情报 (清晰阅读区) */
.detail-col-right { 
  flex: 1; padding: 30px; overflow-y: auto; background: #141414;
}

.info-block { margin-bottom: 35px; }
.block-head { 
  font-size: 1rem; color: #fff; font-weight: bold; border-bottom: 1px solid #444; 
  padding-bottom: 8px; margin-bottom: 15px; letter-spacing: 1px;
}

/* 键值对网格 */
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.grid-item { display: flex; flex-direction: column; }
.grid-item .label { font-size: 0.75rem; color: #888; font-weight: bold; margin-bottom: 5px; }
.grid-item .value { font-size: 1.1rem; color: #fff; font-weight: bold; font-family: var(--mono); border-bottom: 1px solid #333; padding-bottom: 5px; }
.grid-item.full { grid-column: span 2; }
.highlight { color: #f1c40f !important; }

/* 文本正文 (高清晰度) */
.text-body { 
  font-size: 1rem; line-height: 1.7; color: #ddd; 
  white-space: pre-wrap; background: #1a1a1a; padding: 15px; 
  border-left: 4px solid #333; border-radius: 4px;
}

/* 武器库 */
.weapon-grid { display: flex; flex-wrap: wrap; gap: 10px; }
.weapon-slot { 
  width: 70px; height: 70px; background: #000; border: 1px solid #555; 
  display: flex; justify-content: center; align-items: center; cursor: zoom-in;
}
.weapon-slot:hover { border-color: #fff; }
.weapon-slot img { max-width: 100%; max-height: 100%; }

/* 底部按钮栏 */
.term-footer { 
  background: #1a1a1a; padding: 15px 25px; border-top: 2px solid #333;
  display: flex; justify-content: space-between; align-items: center;
}
.debug-info { color: #555; font-size: 0.7rem; }
.btn-group { display: flex; gap: 15px; }

/* 编辑模式表单 (深色适配) */
.edit-layout { padding: 30px; width: 100%; }
.edit-layout .form-row { display: flex; gap: 20px; margin-bottom: 20px; }
.edit-layout .form-group { flex: 1; display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px; }
.edit-layout label { color: #888; font-size: 0.8rem; font-weight: bold; }
.edit-layout input, .edit-layout textarea, .edit-layout select {
  background: #222; border: 1px solid #444; color: #fff; padding: 10px;
  font-family: var(--mono); outline: none;
}
.edit-layout input:focus { border-color: var(--red); background: #2a2a2a; }

/* 通用动画 */
@keyframes spin { to { transform: rotate(360deg); } }
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from { opacity: 0; transform: scale(0.98); }

/* 响应式 */
/* 响应式 */
@media (max-width: 1680px) {
  /* 1. 让布局变成垂直排列，并且高度自动撑开 */
  .detail-layout { 
    flex-direction: column; 
    height: auto; /* 关键：取消固定高度，允许内容撑开 */
    overflow: visible; /* 取消内部滚动，交给外层 .term-body 滚动 */
  }

  /* 2. 左侧图片区域自适应 */
  .detail-col-left { 
    width: 100%; 
    height: auto; 
    aspect-ratio: 16/9; /* 保持一个长宽比，防止图片太高占满屏幕 */
    border-right: none; 
    border-bottom: 2px solid #333; 
    flex-shrink: 0;
  }

  /* 3. 右侧内容区域让它自然生长 */
  .detail-col-right { 
    height: auto; /* 取消高度限制 */
    overflow: visible; /* 取消独立滚动条 */
    flex: none; /* 防止被压缩 */
  }
  
  /* 4. 优化小屏幕下的内边距 */
  .cyber-terminal-window {
    height: 95vh; /* 稍微增加高度占比 */
    max-height: 95vh;
  }
}

@media (max-height: 1080px) {
  .bf-container{
    margin: 2% 0 10%;
  }
}
</style>