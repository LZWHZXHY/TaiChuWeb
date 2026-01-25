<template>
  <section class="module-roster">
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

    <Teleport to="body">
      <Transition name="glitch-fade">
        <div v-if="detailLoading || selectedOC" class="cyber-modal-overlay" @click.self="closeModal">
          
          <div v-if="detailLoading" class="loading-modal">
            <div class="spinner"></div><span>DECRYPTING_FILE...</span>
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
                      <span class="m-label">WINS</span><span class="m-val win-color">{{ selectedOC.winCount || 0 }}</span>
                    </div>
                    <div class="matrix-divider"></div>
                    <div class="matrix-item">
                      <span class="m-label">LOSSES</span><span class="m-val lose-color">{{ selectedOC.loseCount || 0 }}</span>
                    </div>
                  </div>
                </div>

                <div class="detail-col-right custom-scroll">
                  
                  <div class="inner-tabs">
                    <button class="tab-btn" :class="{active: detailTab==='info'}" @click="detailTab='info'">INFO</button>
                    <button class="tab-btn" :class="{active: detailTab==='history'}" @click="detailTab='history'">LOGS ({{ selectedOC.versionHistory?.length || 0 }})</button>
                  </div>

                  <div v-if="detailTab==='info'">
                    <div class="info-block basic">
                      <h3 class="block-head"># BASIC_INTEL</h3>
                      <div class="info-grid">
                        <div class="grid-item"><span class="label">CREATOR</span><span class="value">{{ selectedOC.authorName }}</span></div>
                        <div class="grid-item"><span class="label">SPECIES</span><span class="value">{{ selectedOC.species }}</span></div>
                        <div class="grid-item"><span class="label">GENDER</span><span class="value">{{ getGenderText(selectedOC.gender) }}</span></div>
                        <div class="grid-item"><span class="label">AGE</span><span class="value">{{ selectedOC.age }}</span></div>
                        <div class="grid-item full"><span class="label">P.O.O</span><span class="value highlight">{{ selectedOC.POO || selectedOC.poo }}</span></div>
                        <div class="grid-item full" v-if="selectedOC.colorScheme || selectedOC.colors">
                          <span class="label">COLOR SCHEME</span>
                          <span class="value">{{ selectedOC.colorScheme || selectedOC.colors }}</span>
                        </div>
                      </div>
                    </div>
                    
                    <div class="info-block">
                      <h3 class="block-head"># ABILITY</h3>
                      <div class="text-body">{{ selectedOC.ability }}</div>
                    </div>

                    <div class="info-block" v-if="selectedOC.characterDesc || selectedOC.character">
                      <h3 class="block-head"># CHARACTER / PERSONALITY</h3>
                      <div class="text-body">{{ selectedOC.characterDesc || selectedOC.character }}</div>
                    </div>

                    <div class="info-block" v-if="selectedOC.background">
                      <h3 class="block-head"># BACKGROUND</h3>
                      <div class="text-body">{{ selectedOC.background }}</div>
                    </div>

                    <div class="info-block armory" v-if="(selectedOC.weaponImages && selectedOC.weaponImages.length) || selectedOC.weaponDesc || selectedOC.OC_WeapenDesc">
                      <h3 class="block-head"># VISUAL_ARMORY</h3>
                      <div class="text-body mb-3" v-if="selectedOC.weaponDesc || selectedOC.OC_WeapenDesc">
                        {{ selectedOC.weaponDesc || selectedOC.OC_WeapenDesc }}
                      </div>
                      <div class="weapon-grid" v-if="selectedOC.weaponImages && selectedOC.weaponImages.length">
                        <div v-for="(wp, idx) in selectedOC.weaponImages" :key="idx" class="weapon-slot">
                          <img :src="getImageUrl(wp)" @click="openImage(wp)" />
                        </div>
                      </div>
                    </div>

                    <div class="info-block" v-if="selectedOC.extraDesc || selectedOC.ExtraDesc">
                      <h3 class="block-head"># EXTRA INTEL</h3>
                      <div class="text-body">{{ selectedOC.extraDesc || selectedOC.ExtraDesc }}</div>
                    </div>
                  </div>

                  <div v-else class="history-list">
                    <div v-for="(ver, idx) in selectedOC.versionHistory" :key="idx" class="history-item">
                      <div class="h-meta">
                        <span class="h-ver">VER {{ ver.versionNumber }}</span>
                        <span class="h-date">{{ formatDate(ver.createTime) }}</span>
                        <span v-if="ver.isCurrent" class="h-tag current">CURRENT</span>
                      </div>
                      
                      <div class="h-desc" v-if="ver.versionDescription">
                        Note: {{ ver.versionDescription }}
                      </div>
                      
                      <div class="diff-container" v-if="ver.changes && ver.changes.length">
                        <div v-for="(diff, dIdx) in ver.changes" :key="dIdx" class="diff-row">
                          <div v-if="diff.Field === 'init'" class="diff-init">★ {{ diff.Label }}</div>
                          <div v-else-if="diff.Field === 'image'" class="diff-img-row">
                            <span class="diff-label">{{ diff.Label }}:</span>
                            <div class="img-compare">
                              <div class="img-box old" v-if="diff.OldValue">
                                <span class="tag">OLD</span><img :src="getImageUrl(diff.OldValue)" />
                              </div>
                              <span class="arrow" v-if="diff.OldValue">➜</span>
                              <div class="img-box new">
                                <span class="tag">NEW</span><img :src="getImageUrl(diff.NewValue)" />
                              </div>
                            </div>
                          </div>
                          <div v-else class="diff-text-row">
                            <span class="diff-label">{{ diff.Label }}:</span>
                            <span class="val old" v-if="diff.OldValue">{{ diff.OldValue }}</span>
                            <span class="arrow" v-if="diff.OldValue">➜</span>
                            <span class="val new">{{ diff.NewValue }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                </div>
              </div>

              <div v-else class="edit-layout">
                <div class="edit-title">UPDATING FILE: {{ selectedOC.name }}</div>
                
                <div class="form-row">
                  <div class="form-group half"><label>NAME:</label><input v-model="editForm.name" class="cyber-input" /></div>
                  <div class="form-group half"><label>SPECIES:</label><input v-model="editForm.species" class="cyber-input" /></div>
                </div>
                <div class="form-row">
                  <div class="form-group"><label>GENDER:</label><select v-model="editForm.gender" class="cyber-select"><option :value="0">MALE</option><option :value="1">FEMALE</option><option :value="2">UNKNOWN</option></select></div>
                  <div class="form-group"><label>AGE:</label><input v-model.number="editForm.age" type="number" class="cyber-input" /></div>
                </div>
                <div class="form-group"><label>P.O.O:</label><input v-model="editForm.POO" class="cyber-input" /></div>
                <div class="form-group"><label>ABILITY:</label><textarea v-model="editForm.ability" class="cyber-textarea" rows="4"></textarea></div>
                
                <div class="form-group"><label>CHARACTER / PERSONALITY:</label><textarea v-model="editForm.character" class="cyber-textarea" rows="3"></textarea></div>
                <div class="form-group"><label>BACKGROUND:</label><textarea v-model="editForm.background" class="cyber-textarea" rows="4"></textarea></div>
                <div class="form-group"><label>COLOR SCHEME:</label><input v-model="editForm.colors" class="cyber-input" /></div>
                <div class="form-group"><label>WEAPON DETAILS:</label><textarea v-model="editForm.weaponDesc" class="cyber-textarea" rows="3"></textarea></div>
                <div class="form-group"><label>EXTRA NOTES:</label><textarea v-model="editForm.extraDesc" class="cyber-textarea" rows="2"></textarea></div>
                
                <div class="form-row">
                  <div class="form-group half">
                    <label>UPDATE PORTRAIT (Optional):</label>
                    <input type="file" @change="handleEditFileChange" accept="image/*" class="cyber-input file" />
                  </div>
                  <div class="form-group half">
                    <label>ADD WEAPONS (Optional):</label>
                    <input type="file" @change="handleEditWeaponFiles" accept="image/*" multiple class="cyber-input file" />
                    <div v-if="editForm.newWeapons.length" class="file-list-tiny">
                      <span v-for="(f, i) in editForm.newWeapons" :key="i">{{ f.name }} <b @click="removeEditWeapon(i)" class="x-btn">x</b></span>
                    </div>
                  </div>
                </div>

                <div class="form-group highlight-box">
                  <label style="color: var(--red);">VERSION UPDATE NOTE (Required):</label>
                  <input v-model="editForm.updateDescription" class="cyber-input" placeholder="e.g. Added background story, updated stats..." />
                </div>
              </div>

            </div>

            <div class="term-footer">
              <template v-if="!isEditing">
                <div class="debug-info">[ID:{{ selectedOC?.ocId || selectedOC?.id }}]</div>
                <div class="btn-group">
                  <button v-if="isOwner(selectedOC)" class="cyber-btn secondary" @click="startEdit">EDIT_FILE</button>
                  <button class="cyber-btn primary" @click="challengeOC(selectedOC)">INITIATE_BATTLE</button>
                  <button v-if="isOwner(selectedOC)" class="cyber-btn danger" @click="deleteOC(selectedOC)">DELETE_FILE</button>
                </div>
              </template>
              <template v-else>
                <button class="cyber-btn ghost" @click="cancelEdit" :disabled="upSubmitting">CANCEL</button>
                <button class="cyber-btn primary" @click="submitUpdate" :disabled="upSubmitting">
                  {{ upSubmitting ? 'UPLOADING...' : 'CONFIRM_UPDATE' }}
                </button>
              </template>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </section>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps(['currentUserId'])

// Data
const rosterLoading = ref(false)
const rosterList = ref([])
const rosterQuery = ref('')
const rosterPage = ref(1)
const rosterSize = 12

const selectedOC = ref(null)
const detailLoading = ref(false)
const isEditing = ref(false)
const upSubmitting = ref(false)
const detailTab = ref('info')

const editForm = reactive({ 
  name: '', gender: 0, age: 0, species: '', ability: '', 
  POO: '', background: '', 
  character: '', colors: '', weaponDesc: '', extraDesc: '',
  updateDescription: '', newImage: null, newWeapons: [] 
})

// Actions
const loadRoster = async () => {
  rosterLoading.value = true
  try {
    const res = await apiClient.get('/ocbattlefield/list')
    if(res.data.success) rosterList.value = res.data.data.items || res.data.data || []
  } catch(e) {} finally { rosterLoading.value = false }
}

const openDetail = async (ocSummary) => {
  selectedOC.value = null; detailLoading.value = true; isEditing.value = false; detailTab.value = 'info'
  try {
    const res = await apiClient.get(`/OCBattleField/${ocSummary.id}`)
    if (res.data.success) selectedOC.value = res.data.data
    else alert(res.data.message)
  } catch (e) { alert("DATA_CORRUPT") } finally { detailLoading.value = false }
}

const closeModal = () => { selectedOC.value = null; isEditing.value = false }

// Delete Logic
const deleteOC = async (oc) => {
  const confirmText = prompt(`警告：确认要删除角色 [ ${oc.name} ] 吗？\n此操作不可逆！\n\n请输入 "DELETE" 确认删除：`)
  if (confirmText === 'DELETE') {
    try {
      const targetId = oc.ocId || oc.id
      const res = await apiClient.delete(`/OCBattleField/${targetId}`)
      if (res.data.success) {
        alert("角色档案已销毁 (TERMINATED)")
        closeModal()
        loadRoster()
      } else {
        alert(res.data.message)
      }
    } catch (e) {
      alert("DELETION_FAILED: " + (e.response?.data?.message || 'Server Error'))
    }
  }
}

// Edit Logic
const startEdit = () => {
  if (!selectedOC.value) return
  const oc = selectedOC.value
  Object.assign(editForm, { 
    name: oc.name, gender: oc.gender, age: oc.age, species: oc.species, 
    ability: oc.ability, POO: oc.POO || oc.poo, 
    background: oc.background || '', 
    character: oc.characterDesc || oc.character || '',
    colors: oc.colorScheme || oc.colors || '',
    weaponDesc: oc.weaponDesc || oc.OC_WeapenDesc || '',
    extraDesc: oc.extraDesc || oc.ExtraDesc || '',
    updateDescription: '', newImage: null, newWeapons: [] 
  })
  isEditing.value = true
}

const cancelEdit = () => isEditing.value = false
const handleEditFileChange = (e) => { const file = e.target.files[0]; if(file) editForm.newImage = file }
const handleEditWeaponFiles = (e) => {
  const files = Array.from(e.target.files).filter(f => f.size <= 5*1024*1024)
  editForm.newWeapons = [...editForm.newWeapons, ...files]
  e.target.value = ''
}
const removeEditWeapon = (idx) => editForm.newWeapons.splice(idx, 1)

const submitUpdate = async () => {
  if (!editForm.name || !editForm.updateDescription) return alert("NAME & UPDATE NOTE REQUIRED")
  upSubmitting.value = true
  try {
    const fd = new FormData()
    fd.append('name', editForm.name)
    fd.append('gender', editForm.gender)
    fd.append('age', editForm.age)
    fd.append('species', editForm.species)
    fd.append('ability', editForm.ability)
    fd.append('POO', editForm.POO)
    fd.append('background', editForm.background)
    if(editForm.character) fd.append('character', editForm.character)
    if(editForm.colors) fd.append('colors', editForm.colors)
    if(editForm.weaponDesc) fd.append('weaponDesc', editForm.weaponDesc)
    if(editForm.extraDesc) fd.append('extraDesc', editForm.extraDesc)
    fd.append('updateDescription', editForm.updateDescription)
    if (editForm.newImage) fd.append('CharacterImage', editForm.newImage)
    editForm.newWeapons.forEach(f => fd.append('WeaponImages', f))

    const targetId = selectedOC.value.ocId || selectedOC.value.id
    const res = await apiClient.post(`/OCBattleField/${targetId}/update`, fd, { headers: {'Content-Type': 'multipart/form-data'} })
    if (res.data.success) { 
      alert("UPDATE_SUCCESS")
      isEditing.value = false
      loadRoster()
      openDetail({ id: targetId }) 
    }
  } catch(e) { alert('UPDATE_FAILED') } finally { upSubmitting.value = false }
}

// Utils
const challengeOC = (oc) => alert(`INITIATING_BATTLE_PROTOCOL... TARGET: ${oc.name}`)
const getImageUrl = (url) => (!url ? '/土豆.jpg' : (url.startsWith('http') ? url : `https://bianyuzhou.com/${url.replace(/^\//, '')}`))
const handleImgError = (e) => e.target.src = '/土豆.jpg'
const getStatusText = (s) => ['IDLE', 'COMBAT', 'CLOSED', 'BANNED'][s] || 'UNKNOWN'
const getStatusClass = (s) => ['s-idle', 's-busy', 's-closed', 's-banned'][s] || ''
const getGenderText = (g) => g === 0 ? 'MALE' : g === 1 ? 'FEMALE' : 'UNKNOWN'
const openImage = (url) => window.open(getImageUrl(url), '_blank')
const formatDate = (str) => str ? new Date(str).toLocaleDateString() : 'N/A'
const isOwner = (oc) => {
  if (!oc || !props.currentUserId) return false
  const ocAuthorId = oc.authorId || oc.authorID || oc.AuthorId
  return parseInt(ocAuthorId) === parseInt(props.currentUserId)
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

onMounted(() => { loadRoster() })
</script>

<style scoped>
/* Roster Styles */
.module-roster { width: 100%; }
.roster-toolbar { display: flex; justify-content: space-between; margin-bottom: 20px; align-items: center; }
.search-unit { display: flex; align-items: center; gap: 10px; background: var(--white); border: 2px solid var(--black); padding: 5px 10px; width: 400px; }
.search-unit .prompt { color: var(--red); font-weight: bold; }
.search-input { border: none; outline: none; width: 100%; font-weight: bold; background: transparent; }
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

/* Modal Styles */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.9); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
.cyber-terminal-window { width: 1000px; max-width: 95vw; height: 90vh; background: #111; border: 2px solid #666; display: flex; flex-direction: column; box-shadow: 0 0 50px rgba(0,0,0,0.8); color: #eee; }
.term-header { background: #222; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid var(--red); }
.header-title { display: flex; align-items: center; gap: 10px; }
.title-text { text-shadow: 0 0 10px rgba(255, 255, 255, 0.2); }
.deco-dot { width: 10px; height: 10px; background: var(--red); border-radius: 50%; box-shadow: 0 0 5px var(--red); }
.term-close { background: transparent; border: 1px solid #777; color: #ccc; cursor: pointer; font-family: var(--mono); font-weight: bold; padding: 6px 12px; }
.term-close:hover { border-color: var(--red); color: var(--red); }
.term-body { flex: 1; overflow-y: auto; padding: 0; display: flex; flex-direction: column; min-height: 0; }
.detail-layout { display: flex; width: 100%; height: 100%; }
.detail-col-left { width: 350px; flex-shrink: 0; display: flex; flex-direction: column; border-right: 2px solid #333; background: #000; }
.portrait-display { flex: 1; position: relative; display: flex; justify-content: center; align-items: center; overflow: hidden; background-image: linear-gradient(#222 1px, transparent 1px), linear-gradient(90deg, #222 1px, transparent 1px); background-size: 20px 20px; }
.portrait-display img { max-width: 95%; max-height: 95%; object-fit: contain; z-index: 1; }
.frame-decor { position: absolute; inset: 10px; border: 1px dashed #444; pointer-events: none; }
.combat-matrix { padding: 20px; background: #1a1a1a; border-top: 2px solid #333; display: flex; justify-content: space-around; align-items: center; }
.matrix-item { text-align: center; }
.m-label { display: block; font-size: 0.7rem; color: #888; font-weight: bold; margin-bottom: 5px; }
.m-val { font-family: var(--heading); font-size: 2.2rem; line-height: 1; }
.win-color { color: var(--green); text-shadow: 0 0 10px rgba(46, 204, 113, 0.4); }
.lose-color { color: var(--red); text-shadow: 0 0 10px rgba(217, 35, 35, 0.4); }
.matrix-divider { width: 1px; height: 40px; background: #444; }
.detail-col-right { flex: 1; padding: 30px; overflow-y: auto; background: #141414; }
.info-block { margin-bottom: 35px; }
.block-head { font-size: 1rem; color: #fff; font-weight: bold; border-bottom: 1px solid #444; padding-bottom: 8px; margin-bottom: 15px; letter-spacing: 1px; }
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.grid-item { display: flex; flex-direction: column; }
.grid-item .label { font-size: 0.75rem; color: #888; font-weight: bold; margin-bottom: 5px; }
.grid-item .value { font-size: 1.1rem; color: #fff; font-weight: bold; border-bottom: 1px solid #333; padding-bottom: 5px; }
.grid-item.full { grid-column: span 2; }
.highlight { color: #f1c40f !important; }
.text-body { font-size: 1rem; line-height: 1.7; color: #ddd; white-space: pre-wrap; background: #1a1a1a; padding: 15px; border-left: 4px solid #333; border-radius: 4px; }
.weapon-grid { display: flex; flex-wrap: wrap; gap: 10px; }
.weapon-slot { width: 70px; height: 70px; background: #000; border: 1px solid #555; display: flex; justify-content: center; align-items: center; cursor: zoom-in; }
.weapon-slot:hover { border-color: #fff; }
.weapon-slot img { max-width: 100%; max-height: 100%; }
.term-footer { background: #1a1a1a; padding: 15px 25px; border-top: 2px solid #333; display: flex; justify-content: space-between; align-items: center; }
.debug-info { color: #555; font-size: 0.7rem; }
.btn-group { display: flex; gap: 15px; }
.edit-layout { padding: 30px; width: 100%; }
.edit-layout .form-row { display: flex; gap: 20px; margin-bottom: 20px; }
.edit-layout .form-group { flex: 1; display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px; }
.edit-layout label { color: #888; font-size: 0.8rem; font-weight: bold; }
.edit-layout input, .edit-layout textarea, .edit-layout select { background: #222; border: 1px solid #444; color: #fff; padding: 10px; font-family: var(--mono); outline: none; }
.edit-layout input:focus { border-color: var(--red); background: #2a2a2a; }

/* 内置 Tabs */
.inner-tabs { display: flex; gap: 2px; margin-bottom: 25px; border-bottom: 1px solid #333; }
.tab-btn { background: #222; border: 1px solid #333; border-bottom: none; color: #888; padding: 8px 20px; cursor: pointer; font-weight: bold; font-family: var(--mono); }
.tab-btn.active { background: #141414; color: var(--red); border-color: var(--red); border-bottom: 1px solid #141414; margin-bottom: -1px; }

/* History & Diff */
.history-list { display: flex; flex-direction: column; gap: 15px; }
.history-item { background: #1a1a1a; border: 1px solid #333; padding: 15px; border-left: 3px solid #555; }
.h-meta { display: flex; align-items: center; gap: 15px; margin-bottom: 8px; font-size: 0.8rem; }
.h-ver { color: var(--red); font-weight: bold; }
.h-date { color: #666; }
.h-tag.current { background: var(--green); color: #000; padding: 2px 6px; font-size: 0.6rem; font-weight: bold; border-radius: 2px; }
.h-desc { color: #ccc; font-size: 0.9rem; line-height: 1.4; }
.diff-container { margin-top: 10px; background: rgba(0, 0, 0, 0.2); padding: 12px; border-radius: 4px; border: 1px dashed #444; }
.diff-row { margin-bottom: 8px; font-size: 0.85rem; font-family: var(--mono); line-height: 1.5; }
.diff-row:last-child { margin-bottom: 0; }
.diff-label { color: #888; font-weight: bold; margin-right: 8px; display: inline-block; }
.diff-text-row { display: flex; flex-wrap: wrap; align-items: center; }
.val.old { color: #999; text-decoration: line-through; background: rgba(255, 255, 255, 0.05); padding: 0 4px; border-radius: 2px; }
.val.new { color: var(--green); font-weight: bold; background: rgba(46, 204, 113, 0.1); padding: 0 4px; border-radius: 2px; border-bottom: 1px solid var(--green); }
.arrow { margin: 0 6px; color: var(--red); font-weight: bold; font-size: 0.8rem; }
.diff-img-row { display: flex; flex-direction: column; margin-top: 5px; }
.img-compare { display: flex; align-items: center; margin-top: 5px; gap: 10px; }
.img-box { width: 60px; height: 60px; border: 1px solid #555; position: relative; background: #000; border-radius: 4px; overflow: hidden; }
.img-box img { width: 100%; height: 100%; object-fit: cover; }
.img-box .tag { position: absolute; bottom: 0; left: 0; right: 0; background: rgba(0,0,0,0.7); color: #fff; font-size: 0.6rem; text-align: center; padding: 1px 0; }
.img-box.new { border-color: var(--green); }
.diff-init { color: var(--green); font-weight: bold; letter-spacing: 1px; text-transform: uppercase; }

/* Edit & Utils */
.edit-title { font-size: 1.2rem; color: var(--red); margin-bottom: 20px; font-weight: bold; border-bottom: 2px solid #333; padding-bottom: 10px; }
.file-list-tiny { margin-top: 5px; display: flex; flex-wrap: wrap; gap: 5px; font-size: 0.7rem; color: #888; }
.file-list-tiny span { background: #333; padding: 2px 6px; border-radius: 2px; display: flex; align-items: center; gap: 5px; }
.x-btn { cursor: pointer; color: var(--red); font-weight: bold; }
.highlight-box { border: 1px dashed var(--red); padding: 15px; background: rgba(217, 35, 35, 0.05); }
.mb-3 { margin-bottom: 15px; }

/* 危险按钮样式 */
.cyber-btn.danger { background: transparent; border: 2px solid var(--red); color: var(--red); margin-left: 15px; }
.cyber-btn.danger:hover { background: var(--red); color: #fff; box-shadow: 0 0 10px var(--red); }

@media (max-width: 1680px) {
  .detail-layout { flex-direction: column; height: auto; overflow: visible; }
  .detail-col-left { width: 100%; height: auto; aspect-ratio: 16/9; border-right: none; border-bottom: 2px solid #333; flex-shrink: 0; }
  .detail-col-right { height: auto; overflow: visible; flex: none; }
  .cyber-terminal-window { height: 95vh; max-height: 95vh; }
}
</style>