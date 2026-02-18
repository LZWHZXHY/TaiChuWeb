<template>
  <div class="reports-canvas">
    <div class="reports-layout">
      <div class="report-sidebar">
        <h3>汇报周期</h3>
        <div class="cycle-list">
          <div 
            v-for="cycle in reportCycles" 
            :key="cycle.id" 
            class="cycle-item"
            :class="{ active: currentReportCycle.id === cycle.id }"
            @click="currentReportCycle = cycle"
          >
            <div class="cycle-title">{{ cycle.name }}</div>
            <div class="cycle-date">{{ cycle.dateRange }}</div>
          </div>
        </div>
      </div>

      <div class="report-feed">
        <div class="feed-header">
          <h2>{{ currentReportCycle.name }} 工作汇报</h2>
          <span class="feed-subtitle">{{ currentReportCycle.dateRange }} · {{ currentCycleReports.length }} 份提交</span>
        </div>

        <div v-if="currentCycleReports.length === 0" class="empty-state">
          <div class="empty-icon">📭</div>
          <p>本周期暂无成员提交汇报</p>
          <button class="btn-text" @click="openWriteReportModal">写第一篇汇报</button>
        </div>

        <div class="report-grid">
          <div v-for="rep in currentCycleReports" :key="rep.id" class="report-card" @click="viewReport(rep)">
            <div class="rep-header">
              <div class="rep-user">
                <UniversalAvatar :user-id="rep.userId" :show-level="false" class="rep-av" />
                <div class="rep-meta">
                  <span class="rep-name">{{ rep.userName }}</span>
                  <span class="rep-time">{{ formatTimeAgo(rep.createdAt) }}</span>
                </div>
              </div>
              <div class="rep-type-badge" :class="rep.type.toLowerCase()">{{ rep.type }}</div>
            </div>
            <div class="rep-preview">
              {{ rep.content.slice(0, 120) }}...
            </div>
            <div class="rep-footer"><span>阅读全文 →</span></div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showWriteReport" class="modal-backdrop" @click.self="showWriteReport = false">
      <div class="modal-panel large">
        <div class="modal-header"><h3>撰写工作汇报</h3><button class="close-icon" @click="showWriteReport = false">×</button></div>
        <div class="modal-body">
          <div class="form-row">
            <div class="form-group"><label>汇报类型</label><select v-model="newReport.type" class="linear-select"><option value="Weekly">Weekly Report (周报)</option><option value="Monthly">Monthly Report (月报)</option></select></div>
            <div class="form-group"><label>关联周期</label><select v-model="newReport.cycleId" class="linear-select"><option v-for="c in reportCycles" :key="c.id" :value="c.id">{{ c.name }}</option></select></div>
          </div>
          <div class="form-group" style="flex: 1; display: flex; flex-direction: column;">
            <label>内容 (Markdown)</label>
            <textarea v-model="newReport.content" class="detail-textarea" style="flex: 1; min-height: 300px;" placeholder="# 本周工作..."></textarea>
          </div>
        </div>
        <div class="modal-footer"><button class="btn-ghost" @click="showWriteReport = false">取消</button><button class="btn-primary" @click="submitReport" :disabled="isSaving">提交</button></div>
      </div>
    </div>

    <div v-if="viewingReport" class="modal-backdrop" @click.self="viewingReport = null">
      <div class="modal-panel medium">
        <div class="modal-header">
          <div class="rep-view-header">
            <UniversalAvatar :user-id="viewingReport.userId" :show-level="false" />
            <div class="rep-view-info"><h3>{{ viewingReport.userName }}</h3><span>{{ viewingReport.type }} · {{ formatTimeAgo(viewingReport.createdAt) }}</span></div>
          </div>
          <button class="close-icon" @click="viewingReport = null">×</button>
        </div>
        <div class="modal-body report-reader"><pre>{{ viewingReport.content }}</pre></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted} from 'vue'
import UniversalAvatar from '@/GeneralComponents/UserAvatar.vue'
import apiClient from '@/utils/api'

const props = defineProps<{ projectId: string | string[], members: any[] }>()

const reportCycles = ref([
  { id: 1, name: '2026年 第8周', dateRange: 'Feb 16 - Feb 22' },
  { id: 2, name: '2026年 第7周', dateRange: 'Feb 09 - Feb 15' }
])
const currentReportCycle = ref(reportCycles.value[0])
const allReports = ref<any[]>([])
const showWriteReport = ref(false)
const viewingReport = ref<any>(null)
const isSaving = ref(false)
const newReport = reactive({ type: 'Weekly', cycleId: 1, content: '' })

const currentCycleReports = computed(() => allReports.value.filter(r => r.cycleId === currentReportCycle.value.id))

const fetchReports = async () => {
  // 模拟数据 (可替换为 apiClient.get)
  setTimeout(() => {
    if (props.members.length > 0) {
      allReports.value = [
        { id: 101, cycleId: 1, userId: props.members[0].userId, userName: props.members[0].username, type: 'Weekly', content: '# 本周工作\n- 重构了前端看板组件\n- 修复了拖拽 Bug', createdAt: new Date().toISOString() }
      ]
    }
  }, 500)
}

const submitReport = async () => {
  if (!newReport.content.trim()) return
  isSaving.value = true
  setTimeout(() => {
    allReports.value.unshift({
      id: Date.now(), cycleId: newReport.cycleId, userId: 888, userName: '我', type: newReport.type, content: newReport.content, createdAt: new Date().toISOString()
    })
    showWriteReport.value = false; newReport.content = ''; isSaving.value = false
  }, 500)
}

const openWriteReportModal = () => { showWriteReport.value = true }
const viewReport = (r: any) => { viewingReport.value = r }
const formatTimeAgo = (d: string) => new Date(d).toLocaleString()

defineExpose({ openWriteReportModal })
onMounted(() => fetchReports())
</script>

<style scoped>
/* 样式与 ProjectDetail 中的 Report 部分一致 */
.reports-canvas { flex: 1; overflow: hidden; display: flex; background: #FAFBFC; height: 100%; }
.reports-layout { display: flex; width: 100%; max-width: 1400px; margin: 0 auto; height: 100%; }
.report-sidebar { width: 260px; padding: 24px; border-right: 1px solid #EBECF0; background: #F4F5F7; overflow-y: auto; }
.report-sidebar h3 { font-size: 12px; font-weight: 700; color: #6B778C; text-transform: uppercase; margin-bottom: 16px; }
.cycle-item { padding: 10px 12px; border-radius: 6px; cursor: pointer; margin-bottom: 4px; transition: 0.2s; }
.cycle-item:hover { background: #EBECF0; }
.cycle-item.active { background: #DEEBFF; color: #0052CC; }
.cycle-title { font-weight: 600; font-size: 14px; }
.cycle-date { font-size: 11px; opacity: 0.8; margin-top: 2px; }
.report-feed { flex: 1; padding: 32px 48px; overflow-y: auto; }
.feed-header { margin-bottom: 32px; border-bottom: 1px solid #EBECF0; padding-bottom: 16px; }
.feed-header h2 { margin: 0; font-size: 24px; font-weight: 700; color: #172B4D; }
.feed-subtitle { color: #6B778C; font-size: 14px; }
.empty-state { text-align: center; padding: 60px; color: #6B778C; }
.empty-icon { font-size: 48px; margin-bottom: 16px; }
.btn-text { background: none; border: none; color: #0052CC; cursor: pointer; font-weight: 600; font-size: 14px; }
.report-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
.report-card { background: #fff; border: 1px solid #DFE1E6; border-radius: 8px; padding: 20px; cursor: pointer; transition: 0.2s; display: flex; flex-direction: column; }
.report-card:hover { border-color: #0052CC; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.rep-header { display: flex; justify-content: space-between; margin-bottom: 16px; }
.rep-user { display: flex; gap: 10px; align-items: center; }
.rep-av { width: 32px; height: 32px; }
.rep-name { font-weight: 600; font-size: 14px; display: block; }
.rep-time { font-size: 11px; color: #6B778C; }
.rep-type-badge { font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px; text-transform: uppercase; height: fit-content; }
.rep-type-badge.weekly { background: #E3FCEF; color: #006644; }
.rep-type-badge.monthly { background: #DEEBFF; color: #0747A6; }
.rep-preview { font-size: 13px; color: #42526E; line-height: 1.6; overflow: hidden; margin-bottom: 16px; flex: 1; }
.rep-footer { font-size: 12px; color: #0052CC; font-weight: 500; margin-top: auto; }
/* 模态框复用样式... (为了简洁省略部分通用样式，实际需保留 modal-backdrop, panel 等) */
.modal-backdrop { position: fixed; inset: 0; background: rgba(9,30,66,0.54); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal-panel { background: #fff; border-radius: 3px; display: flex; flex-direction: column; box-shadow: 0 8px 30px rgba(0,0,0,0.3); }
.modal-panel.medium { width: 600px; max-height: 80vh; }
.modal-panel.large { width: 900px; height: 80vh; }
.modal-header { padding: 16px 24px; border-bottom: 1px solid #DFE1E6; display: flex; justify-content: space-between; align-items: center; }
.modal-body { padding: 24px; flex: 1; overflow-y: auto; }
.modal-footer { padding: 16px 24px; border-top: 1px solid #DFE1E6; display: flex; justify-content: flex-end; gap: 8px; }
.form-group { margin-bottom: 16px; }
.form-row { display: flex; gap: 16px; }
.form-row .form-group { flex: 1; }
label { display: block; font-size: 12px; font-weight: 600; color: #6B778C; margin-bottom: 4px; }
.linear-select, .detail-textarea { width: 100%; padding: 8px; border: 2px solid #DFE1E6; border-radius: 3px; font-size: 14px; background: #FAFBFC; }
.btn-primary { background: #0052CC; color: #fff; border: none; border-radius: 3px; padding: 6px 12px; font-weight: 500; font-size: 13px; cursor: pointer; }
.btn-ghost { background: transparent; border: none; color: #6B778C; cursor: pointer; }
.close-icon { background: none; border: none; font-size: 20px; cursor: pointer; color: #6B778C; }
.rep-view-header { display: flex; gap: 16px; align-items: center; }
.rep-view-info h3 { margin: 0; font-size: 16px; }
.report-reader pre { white-space: pre-wrap; font-family: sans-serif; line-height: 1.6; color: #172B4D; }
</style>