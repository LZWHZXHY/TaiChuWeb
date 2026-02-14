<template>
  <div class="calendar-admin-panel">
    
    <div class="panel-toolbar">
      <div class="toolbar-left">
        <h2 class="panel-title">
          <span class="icon">📅</span> CHRONO_DB_MANAGER
        </h2>
        <div class="stats-badge">ENTRIES: {{ events.length }}</div>
      </div>
      <div class="toolbar-right">
        <div class="filter-box">
          <label>FILTER_YEAR:</label>
          <select v-model="filterYear" @change="fetchEvents" class="cyber-select">
            <option :value="2025">2025</option>
            <option :value="2026">2026</option>
          </select>
        </div>
        <button class="btn-create" @click="openModal(false)">
          <span>+ NEW_SIGNAL</span>
        </button>
      </div>
    </div>

    <div class="data-grid-wrapper custom-scroll">
      <table class="cyber-table">
        <thead>
          <tr>
            <th width="60">ID</th>
            <th width="110">DATE</th>
            <th width="80">TIME</th>
            <th>EVENT_TITLE</th>
            <th width="120">TYPE</th>
            <th width="140">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="events.length === 0">
            <td colspan="6" class="empty-row">NO_DATA_FOUND // 暂无数据</td>
          </tr>
          <tr v-for="ev in events" :key="ev.Id">
            <td class="col-id">#{{ padZero(ev.Id) }}</td>
            <td class="col-mono">{{ ev.Date }}</td>
            <td class="col-mono">{{ ev.Time }}</td>
            <td class="col-main">
              <div class="title-text">{{ ev.Title }}</div>
              <div class="desc-text" v-if="ev.Description" :title="ev.Description">
                {{ ev.Description }}
              </div>
            </td>
            <td>
              <span class="type-tag" :style="{ color: ev.Color, borderColor: ev.Color }">
                {{ ev.Type }}
              </span>
            </td>
            <td class="col-actions">
              <button class="action-btn edit" @click="openModal(true, ev)">EDIT</button>
              <button class="action-btn delete" @click="deleteEvent(ev.Id)">DEL</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
      <div class="cyber-form-window">
        <div class="window-header">
          <span>{{ isEdit ? `EDITING PROTOCOL #${currentId}` : 'INIT_NEW_EVENT' }}</span>
          <button class="close-btn" @click="closeModal">×</button>
        </div>
        
        <div class="window-body">
          <div class="form-group">
            <label>EVENT_TITLE <span class="req">*</span></label>
            <input type="text" v-model="form.title" class="cyber-input" placeholder="输入活动标题..." />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>DATE <span class="req">*</span></label>
              <input type="date" v-model="form.date" class="cyber-input" />
            </div>
            <div class="form-group">
              <label>TIME <span class="req">*</span></label>
              <input type="time" v-model="form.time" class="cyber-input" />
            </div>
          </div>

          <div class="form-group type-group">
            <div class="label-row">
              <label>SIGNAL_TYPE <span class="req">*</span></label>
              <button class="mini-btn" @click="showTypeAdder = !showTypeAdder">
                {{ showTypeAdder ? '- CANCEL ADDING' : '+ ADD NEW TYPE' }}
              </button>
            </div>

            <transition name="slide-fade">
              <div v-if="showTypeAdder" class="type-adder-box">
                <div class="adder-row">
                  <input v-model="newTypeForm.name" placeholder="Name (e.g. GAME)" class="cyber-input mini" />
                  <input v-model="newTypeForm.slug" placeholder="ID (e.g. game)" class="cyber-input mini" />
                  <div class="color-wrapper">
                    <input type="color" v-model="newTypeForm.color" class="color-picker" />
                  </div>
                  <button class="btn-save-type" @click="addNewType">SAVE</button>
                </div>
              </div>
            </transition>

            <div class="type-selector custom-scroll">
              <div 
                v-for="type in eventTypes" 
                :key="type.slug" 
                class="type-option"
                :class="{ active: form.typeSlug === type.slug }"
                :style="{ '--type-color': type.color }"
                @click="form.typeSlug = type.slug"
              >
                <span class="dot" :style="{ background: type.color }"></span>
                {{ type.name }}
              </div>
            </div>
          </div>

          <div class="form-group">
            <label>DESCRIPTION</label>
            <textarea v-model="form.description" class="cyber-input textarea" rows="3" placeholder="活动详情描述..."></textarea>
          </div>
        </div>

        <div class="window-footer">
          <button class="btn-cancel" @click="closeModal">CANCEL</button>
          <button class="btn-confirm" @click="submitForm" :disabled="submitting">
            {{ submitting ? 'PROCESSING...' : (isEdit ? 'UPDATE_DATA' : 'EXECUTE_SAVE') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import apiClient from '@/utils/api'

// --- 状态 ---
const events = ref([])
const eventTypes = ref([])
const filterYear = ref(new Date().getFullYear())
const filterMonth = ref(new Date().getMonth() + 1)
const submitting = ref(false)

const isModalOpen = ref(false)
const isEdit = ref(false)
const currentId = ref(null)
const showTypeAdder = ref(false)

const form = reactive({
  title: '',
  date: '',
  time: '10:00',
  typeSlug: '',
  description: ''
})

const newTypeForm = reactive({
  name: '',
  slug: '',
  color: '#2196F3'
})

// --- API ---

const fetchEvents = async () => {
  try {
    const res = await apiClient.get('/events', {
      params: { year: filterYear.value, month: filterMonth.value }
    })
    events.value = res.data
  } catch (error) { console.error("Fetch Error:", error) }
}

const fetchTypes = async () => {
  try {
    const res = await apiClient.get('/events/types')
    // 关键修正：后端可能返回 PascalCase (Slug, Name), 前端统一转为 camelCase (slug, name)
    eventTypes.value = res.data.map(t => ({
      slug: (t.slug || t.Slug || '').toLowerCase(),
      name: (t.name || t.Name || '').toUpperCase(),
      color: t.color || t.Color
    }))
  } catch (error) { console.error("Type Fetch Error:", error) }
}

const submitForm = async () => {
  if (!form.title || !form.date || !form.typeSlug) return alert("Please fill required fields")
  
  submitting.value = true
  const payload = {
    Title: form.title,
    Date: form.date,
    Time: form.time,
    TypeSlug: form.typeSlug,
    Description: form.description
  }

  try {
    if (isEdit.value) {
      await apiClient.put(`/events/${currentId.value}`, payload)
    } else {
      await apiClient.post('/events', payload)
    }
    closeModal()
    fetchEvents()
  } catch (error) {
    alert("Save failed")
    console.error(error)
  } finally {
    submitting.value = false
  }
}

const deleteEvent = async (id) => {
  if (!confirm(`Delete event #${id}?`)) return
  try {
    await apiClient.delete(`/events/${id}`)
    fetchEvents()
  } catch (error) { alert("Delete failed") }
}

const addNewType = async () => {
  if(!newTypeForm.name || !newTypeForm.slug) return alert("Type info missing")
  try {
    await apiClient.post('/events/types', {
      Name: newTypeForm.name,
      Slug: newTypeForm.slug,
      Color: newTypeForm.color
    })
    await fetchTypes() // 刷新列表
    form.typeSlug = newTypeForm.slug.toLowerCase() // 选中新类型
    showTypeAdder.value = false
    newTypeForm.name = ''; newTypeForm.slug = '';
  } catch (e) { alert("Add type failed (duplicate ID?)") }
}

// --- 交互 ---

const openModal = (editMode, eventData = null) => {
  showTypeAdder.value = false
  isEdit.value = editMode
  if (editMode && eventData) {
    currentId.value = eventData.Id
    form.title = eventData.Title
    form.date = eventData.Date
    form.time = eventData.Time
    form.typeSlug = eventData.Type
    form.description = eventData.Description
  } else {
    currentId.value = null
    form.title = ''
    form.date = new Date().toISOString().split('T')[0]
    form.time = '10:00'
    // 默认选中第一个类型
    form.typeSlug = eventTypes.value.length > 0 ? eventTypes.value[0].slug : ''
    form.description = ''
  }
  isModalOpen.value = true
}

const closeModal = () => isModalOpen.value = false
const padZero = (n) => n < 10 ? `0${n}` : n

onMounted(async () => {
  await fetchTypes()
  fetchEvents()
})
</script>

<style scoped>
.calendar-admin-panel {
  --red: #D92323;
  --black: #111111;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  height: 100%; display: flex; flex-direction: column; gap: 20px;
}

/* 工具栏 */
.panel-toolbar { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 2px solid var(--black); padding-bottom: 15px; }
.panel-title { font-family: 'Anton', sans-serif; font-size: 1.8rem; margin: 0; display: flex; align-items: center; gap: 10px; }
.stats-badge { font-family: var(--mono); font-size: 0.7rem; background: var(--black); color: #fff; padding: 2px 8px; margin-top: 5px; }
.toolbar-right { display: flex; gap: 15px; align-items: center; }
.cyber-select { border: 1px solid var(--black); background: transparent; padding: 5px; font-family: var(--mono); font-weight: bold; cursor: pointer; }
.btn-create { background: var(--red); color: #fff; border: 2px solid var(--black); padding: 8px 20px; font-family: var(--mono); font-weight: bold; cursor: pointer; box-shadow: 4px 4px 0 var(--black); transition: transform 0.1s; }
.btn-create:active { transform: translate(2px, 2px); box-shadow: 2px 2px 0 var(--black); }

/* 表格 */
.data-grid-wrapper { flex: 1; border: 2px solid var(--black); background: #fff; overflow: auto; }
.cyber-table { width: 100%; border-collapse: collapse; text-align: left; table-layout: fixed; /* 固定列宽 */ }
.cyber-table th { background: var(--black); color: #fff; padding: 12px; font-family: var(--mono); font-size: 0.8rem; position: sticky; top: 0; z-index: 10; }
.cyber-table td { padding: 12px; border-bottom: 1px solid #eee; vertical-align: middle; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.col-id { font-family: var(--mono); font-weight: bold; color: #888; }
.col-mono { font-family: var(--mono); font-weight: bold; }
.title-text { font-weight: 800; font-size: 1rem; overflow: hidden; text-overflow: ellipsis; }
.desc-text { font-size: 0.8rem; color: #666; margin-top: 4px; overflow: hidden; text-overflow: ellipsis; max-width: 100%; }
.type-tag { display: inline-block; font-size: 0.7rem; font-weight: 800; border: 1px solid; padding: 2px 6px; text-transform: uppercase; border-radius: 2px; }

.col-actions { display: flex; gap: 10px; }
.action-btn { border: none; background: none; font-family: var(--mono); font-weight: bold; cursor: pointer; text-decoration: underline; font-size: 0.8rem; padding: 0; }
.action-btn.edit { color: #2196F3; }
.action-btn.delete { color: var(--red); }

/* 模态框 */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); z-index: 999; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(3px); }
.cyber-form-window { width: 520px; background: #fff; border: 3px solid var(--black); box-shadow: 12px 12px 0 rgba(0,0,0,0.3); display: flex; flex-direction: column; max-height: 90vh; }
.window-header { background: var(--black); color: #fff; padding: 12px 20px; font-family: var(--mono); font-weight: bold; display: flex; justify-content: space-between; }
.close-btn { background: none; border: none; color: #fff; font-size: 1.2rem; cursor: pointer; }

.window-body { padding: 25px; display: flex; flex-direction: column; gap: 18px; overflow-y: auto; }
.cyber-input { width: 100%; padding: 10px; border: 2px solid #ddd; background: #fcfcfc; font-family: 'Inter', sans-serif; box-sizing: border-box; font-size: 0.9rem; color:#111111}
.cyber-input:focus { border-color: var(--black); outline: none; }
.form-row { display: flex; gap: 20px; }
.form-group { flex: 1; }
.form-group label { display: block; font-family: var(--mono); font-size: 0.7rem; font-weight: bold; margin-bottom: 6px; }
.form-group .req { color: var(--red); }

/* 类型添加器 (修复样式) */
.label-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.mini-btn { border: none; background: none; color: #2196F3; font-size: 0.7rem; cursor: pointer; font-weight: bold; text-transform: uppercase; }
.mini-btn:hover { text-decoration: underline; }

.type-adder-box { background: #f5f5f5; padding: 12px; border: 1px dashed #bbb; margin-bottom: 10px; }
.adder-row { display: flex; gap: 8px; align-items: center; }
.cyber-input.mini { flex: 1; padding: 6px; font-size: 0.8rem; }
.color-wrapper { width: 34px; height: 34px; border: 2px solid #ddd; padding: 2px; background: #fff; display: flex; align-items: center; justify-content: center; }
.color-picker { width: 100%; height: 100%; border: none; cursor: pointer; padding: 0; background: none; }
.btn-save-type { background: var(--black); color: #fff; border: none; padding: 0 15px; height: 34px; font-weight: bold; cursor: pointer; font-family: var(--mono); font-size: 0.8rem; }
.btn-save-type:hover { background: #333; }

/* 类型选择列表 (grid布局防止乱) */
.type-selector { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; max-height: 120px; overflow-y: auto; }
.type-option { border: 1px solid #ddd; padding: 8px; cursor: pointer; font-size: 0.75rem; font-weight: bold; display: flex; align-items: center; gap: 6px; transition: all 0.2s; background: #fff; }
.type-option .dot { width: 10px; height: 10px; border-radius: 50%; border: 1px solid rgba(0,0,0,0.1); }
.type-option:hover { border-color: #bbb; background: #fafafa; }
.type-option.active { border-color: var(--type-color); background: rgba(0,0,0,0.03); box-shadow: 2px 2px 0 var(--type-color); }

.window-footer { border-top: 1px solid #eee; padding: 15px 25px; display: flex; justify-content: flex-end; gap: 10px; background: #fafafa; }
.btn-cancel { background: #fff; border: 2px solid #ddd; padding: 8px 15px; cursor: pointer; font-weight: bold; font-family: var(--mono); }
.btn-confirm { background: var(--black); color: #fff; border: 2px solid var(--black); padding: 8px 20px; cursor: pointer; font-family: var(--mono); font-weight: bold; }
.btn-confirm:disabled { opacity: 0.6; cursor: not-allowed; }

/* 动画 */
.slide-fade-enter-active, .slide-fade-leave-active { transition: all 0.3s ease; }
.slide-fade-enter-from, .slide-fade-leave-to { transform: translateY(-10px); opacity: 0; }
</style>