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
                <label>行动代号 // MISSION_NAME</label>
                <input v-model="form.name" type="text" placeholder="正式行动名称..." />
              </div>
              <div class="f-row">
                <div class="md-input-box">
                  <label>发起人 // HOST</label>
                  <input :value="form.host" type="text" readonly class="readonly-input" />
                </div>
                <div class="md-input-box">
                  <label>协议类别 // TYPE</label>
                  <select v-model="form.type">
                    <option :value="1">协作 (COLLAB)</option>
                    <option :value="2">接力 (RELAY)</option>
                    <option :value="3">竞技 (MATCH)</option>
                    <option :value="4">同屏同步 (SYNC)</option>
                    <option :value="5">测试项 (TEST)</option>
                  </select>
                </div>
              </div>

              <div class="md-input-box">
                <label>行动标签 // TAGS</label>
                <CyberTagInput 
                  v-model="form.tags" 
                  placeholder="输入标签并按回车..." 
                />
              </div>
            </div>

            <div class="f-section">
              <div class="f-section-title">02 时间尺度 / TEMPORAL</div>
              <div class="f-row">
                <div class="md-input-box">
                  <label>启动日期 // START</label>
                  <input v-model="form.startdate" type="date" />
                </div>
                <div class="md-input-box">
                  <label>截止日期 // LIMIT</label>
                  <input v-model="form.enddate" type="date" />
                </div>
              </div>
            </div>

            <div class="f-section">
              <div class="f-section-title">03 详细情报 / INTEL</div>
              <div class="md-input-box">
                <label>任务目标简报 // BRIEFING</label>
                <textarea v-model="form.desc" placeholder="详细说明行动内容..."></textarea>
              </div>
              <div class="md-input-box">
                <label>指令频道 // COMMS (QQ群号)</label>
                <input v-model="form.QQgroup" type="text" placeholder="群号..." />
              </div>
              <div class="md-input-box">
                <label>执行协议 // PROTOCOLS</label>
                <textarea v-model="form.rules" placeholder="24fps, 1080P等具体执行条件..."></textarea>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="md-btn ghost" @click="closeModal">放弃更改 // ABORT</button>
            <button class="md-btn primary-ink" @click="submitProposal" :disabled="submitting">
              {{ submitting ? '数据同步中...' : (isEdit ? '保存档案修改 // UPDATE' : '提交正式提案 // EXECUTE') }}
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

const props = defineProps({
  modelValue: Boolean,    // 控制显隐
  editingData: Object     // 修改时传入的现有数据（包含 tags 数组和 require 字段）
})

const emit = defineEmits(['update:modelValue', 'success'])

const auth = useAuthStore()
const submitting = ref(false)
const isDraftSaved = ref(false)
const STORAGE_KEY = 'THCY_JOINT_PROPOSAL_DRAFT'

// 初始表单状态
const form = reactive({
  name: '',
  host: '',
  type: 1,
  startdate: '',
  enddate: '',
  desc: '',
  QQgroup: '',
  rules: '',
  tags: '' // 绑定 CyberTagInput
})

// 判断是否为编辑模式
const isEdit = computed(() => !!props.editingData?.id)

// 初始化表单核心逻辑
const initForm = () => {
  if (isEdit.value) {
    const d = props.editingData
    
    form.name = d.name || ''
    form.host = d.host || auth.user?.username
    form.type = d.type || 1
    
    // 🔴 修复 1：日期处理，去除 ISO 字符串中的时间部分
    form.startdate = d.startdate ? d.startdate.split('T')[0] : ''
    form.enddate = d.enddate ? d.enddate.split('T')[0] : ''
    
    // 🔴 修复 2：内容填充
    form.desc = d.desc || ''
    form.QQgroup = d.QQgroup || ''
    
    // 🔴 修复 3：字段对齐（后端 require 对应前端 rules）
    form.rules = d.require || '' 
    
    // 🔴 修复 4：标签回显（数组转为逗号字符串）
    if (Array.isArray(d.tags)) {
      form.tags = d.tags.join(',')
    } else {
      form.tags = d.tags || ''
    }
    
  } else {
    // 新建模式：尝试加载本地草稿
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      try {
        const parsed = JSON.parse(saved)
        Object.assign(form, parsed)
      } catch (e) {
        resetForm()
      }
    } else {
      resetForm()
    }
    form.host = auth.user?.username || ''
  }
}

const resetForm = () => {
  Object.assign(form, {
    name: '',
    host: auth.user?.username || '',
    type: 1,
    startdate: new Date().toISOString().substr(0, 10),
    enddate: '',
    desc: '',
    QQgroup: '',
    rules: '',
    tags: ''
  })
}

// 监听弹窗打开时初始化数据
watch(() => props.modelValue, (val) => {
  if (val) initForm()
})

// 自动保存草稿（仅在非编辑模式下生效）
watch(form, (newVal) => {
  if (!isEdit.value && props.modelValue) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(newVal))
    isDraftSaved.value = true
    setTimeout(() => { isDraftSaved.value = false }, 1000)
  }
}, { deep: true })

const closeModal = () => {
  emit('update:modelValue', false)
}

const submitProposal = async () => {
  if (!form.name || !form.desc) return alert('代号和简报是核心情报，必须填写。')
  
  submitting.value = true
  try {
    if (isEdit.value) {
      // 执行 PUT 档案修正
      await apiClient.put(`ChaiLianHe/${props.editingData.id}`, form)
      alert('档案库数据已更新。')
    } else {
      // 执行 POST 正式提案
      await apiClient.post('ChaiLianHe/create', form)
      localStorage.removeItem(STORAGE_KEY)
      alert('提案已提交，进入审核序列。')
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
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 2100; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(8px); }
.md-modal { background: #fff; width: 720px; max-height: 85vh; border: 4px solid #121212; display: flex; flex-direction: column; box-shadow: 20px 20px 0 rgba(0,0,0,0.2); }

.modal-header { padding: 25px 40px; border-bottom: 4px solid #121212; display: flex; justify-content: space-between; align-items: center; background: #fff; }
.m-title { font-size: 14px; font-weight: 900; letter-spacing: 1px; }
.draft-tag { font-size: 10px; color: #2ecc71; margin-left: 15px; font-family: monospace; }
.m-close { background: none; border: none; font-size: 28px; cursor: pointer; }

.modal-body { padding: 40px; flex: 1; overflow-y: auto; background: #fafafa; }
.f-section { margin-bottom: 35px; border-left: 3px solid #121212; padding-left: 20px; }
.f-section-title { font-size: 11px; font-weight: 900; background: #121212; color: #fff; padding: 4px 12px; display: inline-block; margin-bottom: 20px; margin-left: -23px;}

.md-input-box { margin-bottom: 20px; }
.md-input-box label { font-size: 10px; font-weight: 900; color: #888; margin-bottom: 8px; display: block; text-transform: uppercase; }
.md-input-box input, .md-input-box select, .md-input-box textarea { width: 100%; border: 2px solid #121212; background: #fff; padding: 12px; font-family: inherit; font-size: 14px; transition: 0.2s; }
.md-input-box textarea { height: 120px; resize: vertical; }

.readonly-input { background: #eee !important; color: #888; cursor: not-allowed; }
.f-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }

.modal-footer { padding: 25px 40px; border-top: 2px solid #121212; display: flex; justify-content: flex-end; gap: 20px; background: #fff; }
.md-btn { padding: 12px 25px; font-weight: 900; font-size: 12px; cursor: pointer; border: 2px solid #121212; transition: 0.2s; }
.md-btn.primary-ink { background: #121212; color: #fff; }
.md-btn.primary-ink:hover { background: #D92323; border-color: #D92323; }
.md-btn.ghost { border: none; background: none; color: #999; text-decoration: underline; }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #121212; }
</style>