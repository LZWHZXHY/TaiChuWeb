<template>
  <div class="battlefield-container">
    <header class="bf-header">
      <div class="header-left">
        <h2 class="title">TAICHU_BATTLEFIELD // 约战场</h2>
        <span class="subtitle">OC 混合成长型企划平台</span>
      </div>
      
      <nav class="bf-tabs">
        <button 
          v-for="t in tabs" 
          :key="t.key"
          class="tab-btn"
          :class="{ active: activeTab === t.key }"
          @click="activeTab = t.key"
        >
          <i :class="t.icon"></i> {{ t.label }}
        </button>
      </nav>
    </header>

    <div class="bf-content custom-scroll">
      
      <section v-if="activeTab === 'intro'" class="module-intro fade-in">
        <div class="hero-box">
          <h3>欢迎来到太初约战场</h3>
          <p>这是一个由社区驱动的长期运营企划，旨在连接每一位火柴人创作者。</p>
          <div class="alert-box">
            <i class="fas fa-info-circle"></i>
            <span>当前版本支持人设上传、展示与作者本人修订。</span>
          </div>
        </div>
        
        <div class="info-grid">
          <div class="info-card">
            <div class="icon">📜</div>
            <h4>企划概念</h4>
            <p>不同于单一IP创作，这是一个混合成长型舞台。上传你的OC，在规则框架下与其他创作者互动。</p>
          </div>
          <div class="info-card">
            <div class="icon">🎯</div>
            <h4>核心目标</h4>
            <p>打造"上传-匹配-约战-结算-归档"的完整闭环，让创作有迹可循。</p>
          </div>
          <div class="info-card">
            <div class="icon">⚖️</div>
            <h4>基本规则</h4>
            <p>尊重原创，公平竞技。所有战斗结果需经双方确认及官方审核后方可计入正史。</p>
          </div>
        </div>

        <div class="action-area">
          <button class="bf-btn primary large" @click="activeTab = 'upload'">立即上传人设</button>
          <button class="bf-btn ghost large" @click="activeTab = 'roster'">浏览人设列表</button>
        </div>
      </section>

      <section v-else-if="activeTab === 'upload'" class="module-upload fade-in">
        <div class="form-container">
          <h3 class="section-title">登记新的人设档案</h3>
          <form @submit.prevent="submitOC">
            <div class="form-row">
              <div class="form-group">
                <label>角色名称 <span class="req">*</span></label>
                <input v-model="upForm.OCName" class="bf-input" placeholder="例如：炎刃·赤霄" required />
              </div>
              <div class="form-group">
                <label>作者名称 <span class="req">*</span></label>
                <input v-model="upForm.authorName" class="bf-input" placeholder="你的署名" required />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>性别 <span class="req">*</span></label>
                <select v-model="upForm.gender" class="bf-select" required>
                  <option value="">选择性别</option>
                  <option value="0">男</option>
                  <option value="1">女</option>
                  <option value="2">未知</option>
                </select>
              </div>
              <div class="form-group">
                <label>种族 <span class="req">*</span></label>
                <input v-model="upForm.species" class="bf-input" placeholder="如：人类、机械" required />
              </div>
              <div class="form-group">
                <label>年龄 <span class="req">*</span></label>
                <input v-model.number="upForm.age" type="number" class="bf-input" required />
              </div>
            </div>

            <div class="form-group">
              <label>能力设定 <span class="req">*</span></label>
              <textarea v-model="upForm.ability" class="bf-textarea" rows="5" placeholder="详细描述能力、招式与限制..." required></textarea>
            </div>

            <div class="form-group">
              <label>POO (Power of Origin) <span class="req">*</span></label>
              <input v-model="upForm.poo" class="bf-input" placeholder="核心能量/起源设定" required />
            </div>

            <div class="form-group upload-zone">
              <label>立绘上传 <span class="req">*</span></label>
              <div class="upload-box" @click="$refs.charInput.click()">
                <div v-if="upForm.charFile" class="file-preview">
                  <span class="file-name">{{ upForm.charFile.name }}</span>
                  <button type="button" @click.stop="upForm.charFile = null" class="remove-btn">×</button>
                </div>
                <div v-else class="placeholder">
                  <i class="fas fa-cloud-upload-alt"></i>
                  <span>点击上传立绘 (Max 5MB)</span>
                </div>
              </div>
              <input ref="charInput" type="file" accept="image/*" style="display:none" @change="handleCharFile" />
            </div>

            <div class="form-group">
              <label class="checkbox-label">
                <input type="checkbox" v-model="upForm.agreed" required />
                我已阅读并遵守社区约战规则
              </label>
            </div>

            <div class="form-actions">
              <button class="bf-btn primary" type="submit" :disabled="upSubmitting">
                {{ upSubmitting ? '上传中...' : '提交档案' }}
              </button>
            </div>
          </form>
        </div>
      </section>

      <section v-else-if="activeTab === 'roster'" class="module-roster fade-in">
        <div class="roster-tools">
          <div class="search-bar">
            <i class="fas fa-search"></i>
            <input v-model="rosterQuery" placeholder="搜索角色、作者或能力..." @input="rosterPage = 1" />
          </div>
          <button class="bf-btn ghost" @click="loadRoster">刷新列表</button>
        </div>

        <div v-if="rosterLoading" class="state-box">
          <i class="fas fa-circle-notch fa-spin"></i> 正在读取档案库...
        </div>
        
        <div v-else-if="rosterList.length === 0" class="state-box">
          <i class="fas fa-box-open"></i> 暂无角色数据
        </div>

        <div v-else class="roster-grid">
          <div 
            v-for="oc in paginatedRoster" 
            :key="oc.id" 
            class="oc-card"
            @click="openDetail(oc)"
          >
            <div class="oc-cover">
              <img :src="getImageUrl(oc.imageUrl || oc.OC_image_url)" loading="lazy" @error="handleImgError" />
              <span class="status-tag" :class="getStatusClass(oc.OC_status)">
                {{ getStatusText(oc.OC_status) }}
              </span>
              <span v-if="isOwner(oc)" class="owner-tag" title="我的角色"><i class="fas fa-user-edit"></i></span>
            </div>
            <div class="oc-info">
              <h4 class="oc-name">{{ oc.name }}</h4>
              <div class="oc-meta">
                <span>{{ oc.authorName }}</span>
                <span class="dot">•</span>
                <span>{{ oc.species }}</span>
              </div>
              <p class="oc-desc">{{ truncate(oc.ability, 40) }}</p>
            </div>
          </div>
        </div>

        <div class="pagination" v-if="totalPages > 1">
          <button :disabled="rosterPage <= 1" @click="rosterPage--">上一页</button>
          <span>{{ rosterPage }} / {{ totalPages }}</span>
          <button :disabled="rosterPage >= totalPages" @click="rosterPage++">下一页</button>
        </div>
      </section>

      <section v-else-if="activeTab === 'records'" class="module-records fade-in">
        <div class="records-table-wrap">
          <table class="records-table">
            <thead>
              <tr>
                <th>日期</th>
                <th>对局标题</th>
                <th>发起方</th>
                <th>应战方</th>
                <th>结果</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td colspan="5" class="empty-row">
                  <i class="fas fa-history"></i> 暂无战斗记录，等待系统接入...
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

    </div>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="detailLoading || selectedOC" class="modal-overlay" @click.self="closeModal">
          
          <div v-if="detailLoading" class="modal-card loading-card">
            <div class="modal-body center-content">
              <i class="fas fa-circle-notch fa-spin fa-2x"></i>
              <p>正在读取神经元数据...</p>
            </div>
          </div>

          <div v-else class="modal-card">
            <div class="modal-header">
              <h3>
                <span v-if="isEditing">正在编辑 // {{ editForm.name }}</span>
                <span v-else>档案详情 // {{ selectedOC.name }}</span>
              </h3>
              <button class="close-icon" @click="closeModal"><i class="fas fa-times"></i></button>
            </div>
            
            <div class="modal-body custom-scroll">
              
              <div v-if="!isEditing" class="detail-container">
                <div class="detail-split">
                  <div class="detail-left">
                    <div class="char-image-box">
                      <img :src="getImageUrl(selectedOC.imageUrl || selectedOC.OC_image_url)" @error="handleImgError" />
                    </div>
                    
                    <div class="battle-stats">
                      <div class="stat-item win">
                        <span class="label">WIN</span>
                        <span class="val">{{ selectedOC.winCount || 0 }}</span>
                      </div>
                      <div class="stat-item lose">
                        <span class="label">LOSE</span>
                        <span class="val">{{ selectedOC.loseCount || 0 }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="detail-right">
                    <div class="info-row">
                      <label>作者</label> <span>{{ selectedOC.authorName }}</span>
                    </div>
                    <div class="info-row">
                      <label>种族</label> <span>{{ selectedOC.species }}</span>
                    </div>
                    <div class="info-row">
                      <label>性别</label> <span>{{ getGenderText(selectedOC.gender) }} / {{ selectedOC.age }}岁</span>
                    </div>
                    <div class="info-row">
                      <label>POO</label> <span class="poo-text">{{ selectedOC.POO || selectedOC.poo }}</span>
                    </div>
                    
                    <div class="info-section">
                      <label>能力设定</label>
                      <p>{{ selectedOC.ability }}</p>
                    </div>

                    <div class="info-section" v-if="selectedOC.background">
                      <label>背景故事</label>
                      <p>{{ selectedOC.background }}</p>
                    </div>

                    <div class="info-section" v-if="selectedOC.weaponImages && selectedOC.weaponImages.length">
                      <label>武器/装备库</label>
                      <div class="weapon-grid">
                        <div v-for="(wp, idx) in selectedOC.weaponImages" :key="idx" class="weapon-item">
                          <img :src="getImageUrl(wp)" @click="openImage(wp)" />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="edit-mode-form">
                <div class="form-row">
                  <div class="form-group">
                    <label>角色名称</label>
                    <input v-model="editForm.name" class="bf-input" />
                  </div>
                  <div class="form-group">
                    <label>种族</label>
                    <input v-model="editForm.species" class="bf-input" />
                  </div>
                </div>
                <div class="form-row">
                  <div class="form-group">
                    <label>性别 (0男 1女 2未知)</label>
                    <select v-model="editForm.gender" class="bf-select">
                      <option :value="0">男</option>
                      <option :value="1">女</option>
                      <option :value="2">未知</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>年龄</label>
                    <input v-model.number="editForm.age" type="number" class="bf-input" />
                  </div>
                </div>
                <div class="form-group">
                  <label>POO (核心能量)</label>
                  <input v-model="editForm.POO" class="bf-input" />
                </div>
                <div class="form-group">
                  <label>能力设定</label>
                  <textarea v-model="editForm.ability" class="bf-textarea" rows="4"></textarea>
                </div>
                <div class="form-group">
                  <label>背景故事</label>
                  <textarea v-model="editForm.background" class="bf-textarea" rows="3"></textarea>
                </div>
                <div class="form-group">
                  <label>更新立绘 (可选)</label>
                  <input type="file" @change="handleEditFileChange" accept="image/*" class="bf-input" />
                  <span class="tip-text">不上传则保留原图</span>
                </div>
                <div class="form-group">
                  <label>版本更新说明</label>
                  <input v-model="editForm.updateDescription" class="bf-input" placeholder="简述本次修改内容" />
                </div>
              </div>

            </div>

            <div class="modal-footer">
              <template v-if="!isEditing">
                
                <div style="font-size: 10px; color: #999; margin-right: auto;">
                  [Debug] Current: {{ currentUserId }} / Author: {{ selectedOC?.authorId }} [检测用途]
                </div>

                <button v-if="isOwner(selectedOC)" class="bf-btn secondary" @click="startEdit">
                  <i class="fas fa-edit"></i> 编辑档案
                </button>
                <button class="bf-btn ghost" @click="closeModal">关闭</button>
                <button class="bf-btn primary" @click="challengeOC(selectedOC)">发起约战</button>
              </template>
              <template v-else>
                <button class="bf-btn ghost" @click="cancelEdit" :disabled="upSubmitting">取消</button>
                <button class="bf-btn primary" @click="submitUpdate" :disabled="upSubmitting">
                  {{ upSubmitting ? '保存中...' : '保存修改' }}
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

// --- Tabs Configuration ---
const tabs = [
  { key: 'intro', label: '企划介绍', icon: 'fas fa-info-circle' },
  { key: 'upload', label: '人设上传', icon: 'fas fa-upload' },
  { key: 'roster', label: '人设列表', icon: 'fas fa-users' },
  { key: 'records', label: '战斗记录', icon: 'fas fa-trophy' }
]
const activeTab = ref('intro')

// --- User Identity & Token Logic ---
const currentUserId = ref(null)

// 深度解析 JWT 获取当前用户ID (增强版)
const getUserFromToken = () => {
  const token = localStorage.getItem('auth_token')
  if (!token) return null
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(window.atob(base64))
    
    // 兼容 Asp.Net Core Identity 默认生成的复杂 Key 和常见 Key
    const nameId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] 
                || payload['nameid'] 
                || payload['sub']
                || payload['id']
                || payload['UserId']
                
    if (nameId) {
      console.log('>> Debug: Token 解析成功, UserID:', parseInt(nameId))
      return parseInt(nameId)
    }
    console.warn('>> Debug: Token 解析成功但未找到 UserID 字段')
    return null
  } catch (e) {
    console.error("Token parse error", e)
    return null
  }
}

// --- Upload Logic ---
const upSubmitting = ref(false)
const upForm = reactive({
  OCName: '', authorName: '', gender: '', age: '', species: '', ability: '', poo: '', agreed: false, charFile: null
})
const handleCharFile = (e) => {
  const file = e.target.files[0]
  if(file && file.size <= 5*1024*1024) upForm.charFile = file
  else alert('文件过大或格式错误')
}
const submitOC = async () => {
  if(!upForm.agreed) return alert('请先同意规则')
  upSubmitting.value = true
  try {
    const fd = new FormData()
    fd.append('OCName', upForm.OCName)
    fd.append('authorName', upForm.authorName)
    fd.append('gender', upForm.gender)
    fd.append('age', upForm.age)
    fd.append('species', upForm.species)
    fd.append('ability', upForm.ability)
    fd.append('POO', upForm.poo)
    fd.append('currentTime', Math.floor(Date.now()/1000).toString())
    if(upForm.charFile) fd.append('CharacterImage', upForm.charFile)
    
    await apiClient.post('/OCBattleField/upload', fd, { headers: {'Content-Type': 'multipart/form-data'} })
    alert('上传成功')
    Object.assign(upForm, { OCName:'', authorName:'', gender:'', age:'', species:'', ability:'', poo:'', agreed:false, charFile:null })
    activeTab.value = 'roster'
    loadRoster()
  } catch(e) { alert('上传失败: ' + (e.response?.data?.message || e.message)) }
  finally { upSubmitting.value = false }
}

// --- Roster Logic ---
const rosterLoading = ref(false)
const rosterList = ref([])
const rosterQuery = ref('')
const rosterPage = ref(1)
const rosterSize = 10

// --- Detail & Edit Logic ---
const selectedOC = ref(null)
const detailLoading = ref(false)
const isEditing = ref(false)
const editForm = reactive({
  name: '', gender: 0, age: 0, species: '', ability: '', POO: '', background: '', updateDescription: '', newImage: null
})

const loadRoster = async () => {
  rosterLoading.value = true
  try {
    const res = await apiClient.get('/ocbattlefield/list')
    if(res.data.success) {
      rosterList.value = res.data.data.items || res.data.data || []
    }
  } catch(e) { console.error(e) }
  finally { rosterLoading.value = false }
}

const filteredRoster = computed(() => {
  if(!rosterQuery.value) return rosterList.value
  const q = rosterQuery.value.toLowerCase()
  return rosterList.value.filter(oc => 
    oc.name?.toLowerCase().includes(q) || oc.authorName?.toLowerCase().includes(q)
  )
})

const paginatedRoster = computed(() => {
  const start = (rosterPage.value - 1) * rosterSize
  return filteredRoster.value.slice(start, start + rosterSize)
})
const totalPages = computed(() => Math.ceil(filteredRoster.value.length / rosterSize) || 1)

// --- Helper Functions ---
const getImageUrl = (url) => {
  if(!url) return '/土豆.jpg'
  if (url.startsWith('http') || url.startsWith('blob:')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  return `https://bianyuzhou.com/${path}`; // 生产环境
}

const handleImgError = (e) => e.target.src = '/土豆.jpg'
const truncate = (t, l) => t?.length > l ? t.substring(0,l)+'...' : t
const getStatusText = (s) => ['空闲', '战斗中', '拒战', '封禁'][s] || '未知'
const getStatusClass = (s) => ['s-idle', 's-busy', 's-closed', 's-banned'][s] || ''
const getGenderText = (g) => g === 0 ? '男' : g === 1 ? '女' : '未知'
const openImage = (url) => window.open(getImageUrl(url), '_blank')

// 核心：所有权判断 (增强兼容性)
const isOwner = (oc) => {
  if (!oc || !currentUserId.value) return false
  // 详情接口字段: authorId, 列表接口字段: authorID / authorId
  const ocAuthorId = oc.authorId || oc.authorID || oc.AuthorId
  // 强制数字对比，防止 '82' !== 82
  return parseInt(ocAuthorId) === parseInt(currentUserId.value)
}

// --- Modal Actions ---

// 打开详情：点击列表后调用
const openDetail = async (ocSummary) => {
  selectedOC.value = null
  detailLoading.value = true
  isEditing.value = false
  
  try {
    // 根据 summary.id 去拿详情
    const res = await apiClient.get(`/OCBattleField/${ocSummary.id}`)
    
    if (res.data.success) {
      selectedOC.value = res.data.data
    } else {
      alert(res.data.message)
    }
  } catch (e) {
    console.error("获取详情失败", e)
    alert("无法获取角色详情")
  } finally {
    detailLoading.value = false
  }
}

const closeModal = () => {
  selectedOC.value = null
  isEditing.value = false
}

// 填充编辑表单
const startEdit = () => {
  if (!selectedOC.value) return
  const oc = selectedOC.value
  editForm.name = oc.name
  editForm.gender = oc.gender
  editForm.age = oc.age
  editForm.species = oc.species
  editForm.ability = oc.ability
  editForm.POO = oc.POO || oc.poo
  editForm.background = oc.background || ''
  editForm.updateDescription = ''
  editForm.newImage = null
  isEditing.value = true
}

const cancelEdit = () => {
  isEditing.value = false
}

const handleEditFileChange = (e) => {
  const file = e.target.files[0]
  if(file) editForm.newImage = file
}

const submitUpdate = async () => {
  if (!editForm.name) return alert("名称不能为空")
  
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
    fd.append('updateDescription', editForm.updateDescription || '作者编辑更新')
    
    if (editForm.newImage) {
      fd.append('CharacterImage', editForm.newImage)
    }

    // 从详情对象中获取ID (字段名为 ocId)
    const targetId = selectedOC.value.ocId || selectedOC.value.id

    const res = await apiClient.post(`/OCBattleField/${targetId}/update`, fd, { 
      headers: {'Content-Type': 'multipart/form-data'} 
    })

    if (res.data.success) {
      alert("更新成功！")
      isEditing.value = false
      loadRoster() // 刷新列表数据
      openDetail({ id: targetId }) // 重新获取最新详情
    }
  } catch(e) {
    alert('更新失败: ' + (e.response?.data?.message || e.message))
  } finally {
    upSubmitting.value = false
  }
}

const challengeOC = (oc) => alert(`向 ${oc.name} 发起约战功能开发中...`)

onMounted(() => {
  currentUserId.value = getUserFromToken()
  loadRoster()
})
</script>

<style scoped>
/* Scoped Variables for Purple Theme */
.battlefield-container {
  --bf-purple: #8b5cf6;
  --bf-purple-light: #ddd6fe;
  --bf-bg: #f8fafc;
  --bf-text: #1e293b;
  --bf-text-sub: #64748b;
  --bf-border: #e2e8f0;
  
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--bf-bg);
  padding: 20px;
  box-sizing: border-box;
  font-family: 'Segoe UI', sans-serif;
  color: var(--bf-text);
}

/* Header */
.bf-header {
  display: flex; justify-content: space-between; align-items: flex-end;
  padding-bottom: 15px; border-bottom: 1px solid var(--bf-border); margin-bottom: 20px;
}
.title { font-size: 22px; font-weight: 800; margin: 0; letter-spacing: 1px; }
.subtitle { font-size: 12px; color: var(--bf-text-sub); margin-top: 4px; display: block; }

.bf-tabs { display: flex; gap: 8px; }
.tab-btn {
  background: transparent; border: 1px solid transparent; padding: 8px 16px;
  border-radius: 8px; cursor: pointer; color: var(--bf-text-sub); font-weight: 600;
  font-size: 13px; transition: 0.2s; display: flex; align-items: center; gap: 6px;
}
.tab-btn:hover { background: #f1f5f9; color: var(--bf-purple); }
.tab-btn.active { background: var(--bf-purple-light); color: #5b21b6; }

/* Content Area */
.bf-content { flex: 1; overflow-y: auto; padding-right: 5px; position: relative; }

/* Intro Module */
.module-intro { max-width: 900px; margin: 0 auto; }
.hero-box { background: white; padding: 30px; border-radius: 12px; text-align: center; border: 1px solid var(--bf-border); margin-bottom: 20px; }
.hero-box h3 { margin: 0 0 10px 0; font-size: 24px; }
.alert-box { 
  display: inline-flex; align-items: center; gap: 8px; margin-top: 15px; 
  background: #fffbeb; color: #b45309; padding: 8px 16px; border-radius: 20px; font-size: 12px; font-weight: bold;
}
.info-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; margin-bottom: 30px; }
.info-card { background: white; padding: 20px; border-radius: 10px; border: 1px solid var(--bf-border); }
.info-card .icon { font-size: 24px; margin-bottom: 10px; }
.info-card h4 { margin: 0 0 8px 0; color: var(--bf-purple); }
.info-card p { font-size: 13px; color: var(--bf-text-sub); line-height: 1.5; margin: 0; }
.action-area { display: flex; justify-content: center; gap: 15px; }

/* Upload Module & Edit Form */
.module-upload, .edit-mode-form { max-width: 700px; margin: 0 auto; background: white; padding: 30px; border-radius: 12px; border: 1px solid var(--bf-border); }
.edit-mode-form { border: none; padding: 0; box-shadow: none; } 
.section-title { margin: 0 0 20px 0; font-size: 18px; border-left: 4px solid var(--bf-purple); padding-left: 10px; }
.form-row { display: flex; gap: 15px; }
.form-group { margin-bottom: 15px; flex: 1; }
.form-group label { display: block; font-size: 12px; font-weight: bold; margin-bottom: 6px; color: var(--bf-text-sub); }
.tip-text { font-size: 11px; color: #94a3b8; margin-top: 4px; display: block; }
.req { color: #ef4444; }
.bf-input, .bf-select, .bf-textarea {
  width: 100%; padding: 10px; border: 1px solid var(--bf-border); border-radius: 6px;
  font-size: 14px; outline: none; transition: 0.2s; box-sizing: border-box;
}
.bf-input:focus, .bf-select:focus, .bf-textarea:focus { border-color: var(--bf-purple); box-shadow: 0 0 0 3px rgba(139,92,246,0.1); }
.upload-box {
  border: 2px dashed var(--bf-border); padding: 20px; text-align: center; border-radius: 8px; cursor: pointer; transition: 0.2s;
}
.upload-box:hover { border-color: var(--bf-purple); background: #fdfbff; }
.file-preview { display: flex; justify-content: space-between; align-items: center; background: #f1f5f9; padding: 5px 10px; border-radius: 4px; }
.remove-btn { background: none; border: none; color: #ef4444; font-weight: bold; cursor: pointer; }

/* Roster Module */
.roster-tools { display: flex; justify-content: space-between; margin-bottom: 20px; }
.search-bar { 
  position: relative; flex: 1; max-width: 400px; 
}
.search-bar i { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: var(--bf-text-sub); }
.search-bar input { width: 100%; padding: 10px 10px 10px 35px; border: 1px solid var(--bf-border); border-radius: 8px; outline: none; }
.roster-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 20px; }
.oc-card { 
  background: white; border-radius: 10px; overflow: hidden; border: 1px solid var(--bf-border); 
  cursor: pointer; transition: 0.2s; position: relative;
}
.oc-card:hover { transform: translateY(-5px); box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1); border-color: var(--bf-purple-light); }
.oc-cover { height: 180px; position: relative; background: #f1f5f9; }
.oc-cover img { width: 100%; height: 100%; object-fit: cover; }
.status-tag { 
  position: absolute; top: 10px; right: 10px; padding: 2px 8px; border-radius: 12px; 
  font-size: 10px; font-weight: bold; color: white;
}
.owner-tag {
  position: absolute; top: 10px; left: 10px; padding: 4px 8px; border-radius: 50%;
  font-size: 12px; background: rgba(0,0,0,0.6); color: #fff; backdrop-filter: blur(4px);
}
.s-idle { background: #10b981; } .s-busy { background: #ef4444; } .s-closed { background: #64748b; }
.oc-info { padding: 12px; }
.oc-name { margin: 0 0 5px 0; font-size: 15px; font-weight: bold; }
.oc-meta { font-size: 11px; color: var(--bf-text-sub); display: flex; align-items: center; gap: 5px; margin-bottom: 8px; }
.oc-desc { font-size: 12px; color: #475569; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

/* Records Module */
.records-table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; border: 1px solid var(--bf-border); }
.records-table th, .records-table td { padding: 12px; text-align: left; border-bottom: 1px solid var(--bf-border); font-size: 13px; }
.records-table th { background: #f8fafc; font-weight: bold; color: var(--bf-text-sub); }
.empty-row { text-align: center; padding: 40px; color: var(--bf-text-sub); }

/* Buttons & Utils */
.bf-btn { padding: 10px 20px; border-radius: 6px; border: none; cursor: pointer; font-weight: bold; transition: 0.2s; font-size: 13px; }
.bf-btn.primary { background: var(--bf-purple); color: white; }
.bf-btn.primary:hover { background: #7c3aed; }
.bf-btn.secondary { background: #f59e0b; color: white; }
.bf-btn.secondary:hover { background: #d97706; }
.bf-btn.ghost { background: white; border: 1px solid var(--bf-border); color: var(--bf-text-sub); }
.bf-btn.ghost:hover { border-color: var(--bf-purple); color: var(--bf-purple); }
.bf-btn.large { padding: 12px 30px; font-size: 15px; }
.bf-btn:disabled { background: #cbd5e1; cursor: not-allowed; }
.state-box { text-align: center; padding: 50px; color: var(--bf-text-sub); font-size: 14px; }
.pagination { display: flex; justify-content: center; gap: 10px; margin-top: 20px; align-items: center; font-size: 13px; }

/* Modal */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.7); z-index: 1000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(2px); }
.modal-card { background: white; width: 700px; max-width: 90vw; max-height: 85vh; border-radius: 12px; display: flex; flex-direction: column; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.2); }
.loading-card { min-height: 300px; align-items: center; justify-content: center; }
.center-content { text-align: center; color: var(--bf-purple); }
.modal-header { padding: 15px 20px; background: #f8fafc; border-bottom: 1px solid var(--bf-border); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 16px; color: var(--bf-purple); letter-spacing: 1px; }
.close-icon { border: none; background: transparent; cursor: pointer; font-size: 16px; color: var(--bf-text-sub); }
.modal-body { padding: 25px; overflow-y: auto; }

/* Detail Layout */
.detail-split { display: flex; gap: 25px; }
.detail-left { width: 240px; flex-shrink: 0; display: flex; flex-direction: column; gap: 15px; }
.detail-right { flex: 1; }

.char-image-box { width: 100%; border-radius: 8px; overflow: hidden; border: 1px solid var(--bf-border); background: #000; }
.char-image-box img { width: 100%; height: auto; display: block; }

.battle-stats { display: flex; gap: 10px; }
.stat-item { flex: 1; text-align: center; padding: 8px; border-radius: 6px; color: #fff; font-weight: bold; }
.stat-item .label { display: block; font-size: 10px; opacity: 0.8; }
.stat-item .val { font-size: 18px; }
.stat-item.win { background: #10b981; }
.stat-item.lose { background: #ef4444; }

.info-row { display: flex; margin-bottom: 10px; font-size: 14px; border-bottom: 1px dashed var(--bf-border); padding-bottom: 6px; }
.info-row label { width: 60px; color: var(--bf-text-sub); font-weight: bold; }
.poo-text { color: var(--bf-purple); font-weight: bold; font-family: monospace; }

.info-section { margin-top: 20px; }
.info-section label { display: block; font-size: 13px; font-weight: bold; color: var(--bf-purple); margin-bottom: 8px; }
.info-section p { font-size: 14px; line-height: 1.6; margin: 0; background: #f8fafc; padding: 12px; border-radius: 8px; white-space: pre-wrap; }

/* Weapon Grid */
.weapon-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(60px, 1fr)); gap: 8px; }
.weapon-item { aspect-ratio: 1; border-radius: 4px; overflow: hidden; border: 1px solid var(--bf-border); cursor: zoom-in; }
.weapon-item img { width: 100%; height: 100%; object-fit: cover; transition: 0.2s; }
.weapon-item:hover img { transform: scale(1.1); }

.modal-footer { padding: 15px 20px; border-top: 1px solid var(--bf-border); display: flex; justify-content: flex-end; gap: 10px; background: #f8fafc; }

/* Animations */
.fade-in { animation: fadeIn 0.3s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }

/* Responsive */
@media (max-width: 768px) {
  .detail-split { flex-direction: column; }
  .detail-left { width: 100%; }
}
</style>