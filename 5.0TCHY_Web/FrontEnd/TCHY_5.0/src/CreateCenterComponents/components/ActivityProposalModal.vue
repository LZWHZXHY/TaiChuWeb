<template>
  <Teleport to="body">
    <Transition name="md-fade">
      <div v-if="modelValue" class="modal-overlay" @click.self="closeModal">
        <div class="md-modal proposal-form">
          <div class="modal-header">
            <div class="m-header-left">
              <h2 class="m-title">
                {{ isEdit ? '档案修正申请 // EDIT_ARCHIVE' : '行动提案申请表 // FORM_1024_A' }}
              </h2>
              <span class="draft-tag" v-if="isDraftSaved">● {{ isEdit ? '修改同步中' : '草稿已保存' }}</span>
            </div>
            <button class="m-close" @click="closeModal">×</button>
          </div>

          <div class="modal-body custom-scroll">
            <div class="f-section">
              <div class="f-section-title">01 核心定义 / DEFINITION</div>
              
              <div class="md-input-box">
                <label>企划/模组名称 // MISSION_NAME</label>
                <input v-model="form.name" type="text" placeholder="例：《东京白夜：东京奥运会》..." />
              </div>
              
              <div class="f-row">
                <div class="md-input-box">
                  <label>企划领域 // DOMAIN</label>
                  <select v-model="form.category" @change="handleCategoryChange">
                    <option value="ANIMATION">[ANIM] 动画/视频联合</option>
                    <option value="ART">[ART] 视觉/绘圈企划</option>
                    <option value="TRPG">[TRPG] 跑团/桌面角色扮演</option>
                    <option value="WORLD">[WORLD] 世界观/文字共创</option>
                  </select>
                </div>
                <div class="md-input-box">
                  <label>运行模式 // MODE (TYPE)</label>
                  <select v-model="form.type">
                    <option v-for="opt in currentModeOptions" :key="opt.value" :value="opt.value">
                      {{ opt.label }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="f-row">
                <div class="md-input-box">
                  <label>发起人/KP/PL // HOST</label>
                  <input :value="form.host" type="text" readonly class="readonly-input" />
                </div>
                <div class="md-input-box">
                  <label>行动标签 // TAGS</label>
                  <CyberTagInput v-model="form.tags" placeholder="输入标签并按回车 (如: COC7th, 密室)..." />
                </div>
              </div>
            </div>

            <div class="f-section dynamic-meta">
              <div class="f-section-title">01-B 领域专业参数 / DOMAIN_PARAMS</div>
              <MetaAnimation v-if="form.category === 'ANIMATION'" :meta="metaConfig" />
              <MetaTRPG v-if="form.category === 'TRPG'" :meta="metaConfig" />
              <MetaArt v-if="form.category === 'ART'" :meta="metaConfig" />
              <MetaWorld v-if="form.category === 'WORLD'" :meta="metaConfig" />
            </div>

            <div class="f-section">
              <div class="f-section-title">02 时间尺度 / TEMPORAL</div>
              <div class="f-row">
                <div class="md-input-box">
                  <label>报名开始 // START</label>
                  <input v-model="form.startdate" type="date" />
                </div>
                <div class="md-input-box">
                  <label>预计开团/截止 // LIMIT</label>
                  <input v-model="form.enddate" type="date" />
                </div>
              </div>
            </div>

            <div class="f-section">
              <div class="f-section-title">03 详细情报 / INTEL</div>
              <div class="md-input-box">
                <label>企划背景/模组导入简报 // BRIEFING</label>
                <textarea v-model="form.desc" placeholder="“你们都是来自X国的志愿者，乘上了前往日本的飞机……”"></textarea>
              </div>
              <div class="f-row">
                <div class="md-input-box">
                  <label>跑团群/招募点 // COMMS</label>
                  <input v-model="form.QQgroup" type="text" placeholder="QQ群号/KOOK频道链接..." />
                </div>
                <div class="md-input-box">
                  <label>跑团须知/企划村规 // PROTOCOLS</label>
                  <input v-model="form.rules" placeholder="例: 禁止踢门，非游戏发言加括号，快乐至上..." />
                </div>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="md-btn ghost" @click="closeModal">放弃 // ABORT</button>
            <button class="md-btn primary-ink" @click="submitProposal" :disabled="submitting">
              {{ submitting ? '数据同步中...' : (isEdit ? '保存档案修改 // UPDATE' : '发布正式招募 // EXECUTE') }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { reactive, ref, watch, computed } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'

// 🚀 引入我们刚才拆分出来的子组件！
// ⚠️ 注意：根据你实际保存的路径调整 import 路径
import MetaAnimation from '@/CreateCenterComponents/ActivityMeta/MetaAnimation.vue'
import MetaTRPG from '@/CreateCenterComponents/ActivityMeta/MetaTRPG.vue'
import MetaArt from '@/CreateCenterComponents/ActivityMeta/MetaArt.vue'
import MetaWorld from '@/CreateCenterComponents/ActivityMeta/MetaWorld.vue'

const props = defineProps({
  modelValue: Boolean,    
  editingData: Object     
})

const emit = defineEmits(['update:modelValue', 'success'])

const auth = useAuthStore()
const submitting = ref(false)
const isDraftSaved = ref(false)
const STORAGE_KEY = 'THCY_ACTIVITY_PROPOSAL_DRAFT' 

const form = reactive({
  name: '', host: '', category: 'ANIMATION', type: 1, 
  startdate: '', enddate: '', desc: '', QQgroup: '', rules: '', tags: '' 
})

// ✨ 统管所有领域的元数据，传给子组件
const metaConfig = reactive({
  fps: '', canvas: '', subject: '',
  system: '', playerLimit: '', requireOC: true, difficulty: '', duration: '', buildRules: '', warnings: '',
  color: '', era: '', wordCount: ''
})

const isEdit = computed(() => !!props.editingData?.id)

const categoryModeMap = {
  ANIMATION: [{ value: 1, label: '并行协作 (MEP / 分镜切片)' }, { value: 2, label: '串行接力 (MAP / 动画传递)' }],
  ART: [{ value: 3, label: '同题共绘 (独立作画)' }, { value: 4, label: '合绘画卷 (多人同版)' }, { value: 5, label: '画技接力' }],
  TRPG: [{ value: 6, label: '中长团 (Campaign)' }, { value: 7, label: '短团/单次 (One-shot)' }, { value: 8, label: '文字团 (异步)' }, { value: 9, label: '规则试跑 (Test)' }],
  WORLD: [{ value: 10, label: '沙盒共创' }, { value: 11, label: '剧情接龙' }, { value: 12, label: '阵营对抗' }]
}

const currentModeOptions = computed(() => categoryModeMap[form.category] || categoryModeMap['ANIMATION'])

const handleCategoryChange = () => {
  Object.keys(metaConfig).forEach(k => {
    if (k === 'requireOC') metaConfig[k] = true; else metaConfig[k] = ''
  })
  if (currentModeOptions.value.length > 0) form.type = currentModeOptions.value[0].value
}

const initForm = () => {
  if (isEdit.value) {
    const d = props.editingData
    form.name = d.name || ''; form.host = d.host || auth.user?.username; form.category = d.category || 'ANIMATION';
    form.type = d.type || currentModeOptions.value[0].value; form.startdate = d.startdate ? d.startdate.split('T')[0] : '';
    form.enddate = d.enddate ? d.enddate.split('T')[0] : ''; form.desc = d.desc || ''; form.QQgroup = d.QQgroup || '';
    form.rules = d.require || ''; form.tags = Array.isArray(d.tags) ? d.tags.join(',') : (d.tags || '');
    if (d.metaJson) {
      try { Object.assign(metaConfig, JSON.parse(d.metaJson)) } catch(e) { console.warn("Meta 解析失败") }
    }
  } else {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      try {
        const parsed = JSON.parse(saved); Object.assign(form, parsed.form); Object.assign(metaConfig, parsed.meta)
      } catch (e) { resetForm() }
    } else { resetForm() }
    form.host = auth.user?.username || ''
  }
}

const resetForm = () => {
  Object.assign(form, { name: '', host: auth.user?.username || '', category: 'ANIMATION', startdate: new Date().toISOString().substr(0, 10), enddate: '', desc: '', QQgroup: '', rules: '', tags: '' })
  handleCategoryChange()
}

watch(() => props.modelValue, (val) => { if (val) initForm() })

watch([form, metaConfig], ([newForm, newMeta]) => {
  if (!isEdit.value && props.modelValue) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ form: newForm, meta: newMeta }))
    isDraftSaved.value = true
    setTimeout(() => { isDraftSaved.value = false }, 1000)
  }
}, { deep: true })

const closeModal = () => { emit('update:modelValue', false) }

const submitProposal = async () => {
  if (!form.name || !form.desc) return alert('代号和简报是核心情报，必须填写。')
  
  const payload = {
    ...form,
    require: form.rules,
    metaJson: JSON.stringify(metaConfig) 
  }

  submitting.value = true
  try {
    if (isEdit.value) {
      await apiClient.put(`Activity/${props.editingData.id}`, payload)
      alert('档案库数据已更新。')
    } else {
      await apiClient.post('Activity/create', payload)
      localStorage.removeItem(STORAGE_KEY)
      alert('招募指令已发布，进入审核序列。')
    }
    emit('success')
    closeModal()
    resetForm()
  } catch (err) {
    alert(err.response?.data?.message || '链路同步失败')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* 基本样式保留，移除了 meta-grid，因为这些 CSS 已经下放到了子组件里 */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 2100; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(8px); }
.md-modal { background: #fff; width: 720px; max-height: 85vh; border: 4px solid #121212; display: flex; flex-direction: column; box-shadow: 20px 20px 0 rgba(0,0,0,0.2); }
.modal-header { padding: 25px 40px; border-bottom: 4px solid #121212; display: flex; justify-content: space-between; align-items: center; background: #fff; }
.m-title { font-size: 14px; font-weight: 900; letter-spacing: 1px; }
.draft-tag { font-size: 10px; color: #2ecc71; margin-left: 15px; font-family: monospace; }
.m-close { background: none; border: none; font-size: 28px; cursor: pointer; }
.modal-body { padding: 40px; flex: 1; overflow-y: auto; background: #fafafa; }
.f-section { margin-bottom: 35px; border-left: 3px solid #121212; padding-left: 20px; }
.dynamic-meta { border-left-color: #D92323; background: #fff; padding: 20px; border: 1px solid #ddd; }
.f-section-title { font-size: 11px; font-weight: 900; background: #121212; color: #fff; padding: 4px 12px; display: inline-block; margin-bottom: 20px; margin-left: -23px;}
.dynamic-meta .f-section-title { background: #D92323; margin-left: -24px; }
.md-input-box { margin-bottom: 20px; flex: 1; }
.md-input-box label { font-size: 10px; font-weight: 900; color: #888; margin-bottom: 8px; display: block; text-transform: uppercase; }
.md-input-box input, .md-input-box select, .md-input-box textarea { width: 100%; box-sizing: border-box; border: 2px solid #121212; background: #fff; padding: 12px; font-family: inherit; font-size: 14px; transition: 0.2s; }
.md-input-box textarea { height: 120px; resize: vertical; }
.readonly-input { background: #eee !important; color: #888; cursor: not-allowed; }
.f-row { display: flex; gap: 20px; }
.modal-footer { padding: 25px 40px; border-top: 2px solid #121212; display: flex; justify-content: flex-end; gap: 20px; background: #fff; }
.md-btn { padding: 12px 25px; font-weight: 900; font-size: 12px; cursor: pointer; border: 2px solid #121212; transition: 0.2s; }
.md-btn.primary-ink { background: #121212; color: #fff; }
.md-btn.primary-ink:hover { background: #D92323; border-color: #D92323; }
.md-btn.ghost { border: none; background: none; color: #999; text-decoration: underline; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #121212; }
</style>