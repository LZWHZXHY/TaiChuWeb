<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="show" class="modal-overlay" @click="$emit('close')">
        <div class="modal-card" @click.stop>
          
          <div class="modal-header">
            <h3>>> COMMAND_CENTER // 变更审核</h3>
            <button class="close-btn" @click="$emit('close')">×</button>
          </div>

          <div class="modal-body custom-scroll">
            <div v-if="loading" class="loading-box">LOADING_DATA_STREAM...</div>
            <div v-else-if="auditList.length === 0" class="empty-box">NO_PENDING_REQUESTS</div>

            <div v-else class="audit-list">
              <div 
                v-for="item in auditList" 
                :key="item.id + item.type" 
                class="audit-item"
                :class="item.type.toLowerCase()"
              >
                <div class="item-header">
                  <span class="type-tag">{{ formatType(item.type) }}</span>
                  <span class="time-tag">{{ formatDate(item.createTime) }}</span>
                </div>

                <div class="item-content">
                  <div class="item-title">{{ item.title }}</div>
                  
                  <div class="diff-container">
                    <template v-if="item.type === 'UpdateNode'">
                      <div v-for="(val, key) in getParsedObject(item.data)" :key="key" class="diff-row">
                        <span class="diff-label">{{ key }}:</span>
                        <div class="diff-value">
                          <template v-if="key === 'MetaData'">
                            <pre class="json-inner">{{ JSON.stringify(val, null, 2) }}</pre>
                          </template>
                          <template v-else-if="key === 'Description'">
                            <div class="text-inner">{{ val || '(EMPTY)' }}</div>
                          </template>
                          <template v-else>
                            <span>{{ val }}</span>
                          </template>
                        </div>
                      </div>
                    </template>

                    <template v-else-if="item.type === 'CreateNode'">
                      <div class="diff-row">
                        <span class="diff-label">INITIAL_DATA:</span>
                        <pre class="json-inner">{{ JSON.stringify(item.data, null, 2) }}</pre>
                      </div>
                    </template>

                    <template v-else-if="item.type === 'DeleteNode'">
                      <div class="alert-text">ATTENTION: 该操作将软删除此条目及其关联关系。</div>
                    </template>
                  </div>
                </div>

                <div class="item-actions">
                  <button class="cyber-btn reject" @click="handleAudit(item, false)">REJECT // 拒绝</button>
                  <button class="cyber-btn approve" @click="handleAudit(item, true)">APPROVE // 通过</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  show: Boolean,
  ipId: { type: [Number, String], required: true }
})
const emit = defineEmits(['close'])

const auditList = ref([])
const loading = ref(false)

const fetchAudits = async () => {
  if (!props.ipId) return
  loading.value = true
  try {
    const res = await apiClient.get(`/Audit/${props.ipId}`)
    auditList.value = res.data
  } catch (e) { console.error(e) } finally { loading.value = false }
}

const handleAudit = async (item, isApproved) => {
  const actionText = isApproved ? "通过" : "拒绝"
  if (!confirm(`确定要 [${actionText}] 这条请求吗？`)) return
  try {
    await apiClient.post('/Audit/approve', {
      TargetId: item.id, 
      Type: item.type,
      IsApproved: isApproved
    })
    alert(`已${actionText}`)
    fetchAudits()
  } catch (e) { alert("操作失败") }
}

const formatDate = (t) => new Date(t).toLocaleString()
const formatType = (t) => ({ 'CreateNode': 'NEW', 'UpdateNode': 'EDIT', 'DeleteNode': 'DEL' }[t] || t)

// 解析数据字符串为对象
const getParsedObject = (data) => {
  if (typeof data === 'object') return data
  try { return JSON.parse(data) } catch { return {} }
}

watch(() => props.show, (val) => { if (val) fetchAudits() })
</script>

<style scoped>
/* 延续之前的工业风样式，新增 diff 相关样式 */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 3000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); }
.modal-card { width: 700px; max-width: 95vw; height: 85vh; background: #F4F1EA; border: 3px solid #111; display: flex; flex-direction: column; box-shadow: 15px 15px 0 rgba(0,0,0,0.5); }
.modal-header { background: #111; color: #fff; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-family: 'Anton', sans-serif; font-size: 1.2rem; }
.close-btn { background: none; border: none; color: #fff; font-size: 1.5rem; cursor: pointer; }
.modal-body { padding: 20px; flex: 1; overflow-y: auto; background: #e5e5e5; }

.audit-item { background: #fff; border: 2px solid #111; margin-bottom: 20px; padding: 15px; border-left-width: 8px; }
.audit-item.createnode { border-left-color: #2ecc71; }
.audit-item.updatenode { border-left-color: #3498db; }
.audit-item.deletenode { border-left-color: #D92323; }

.item-header { display: flex; justify-content: space-between; margin-bottom: 10px; font-size: 0.8rem; font-weight: bold; border-bottom: 1px solid #eee; padding-bottom: 5px; }

/* 🔥 详情展示区样式 */
.diff-container { background: #f9f9f9; border: 1px solid #ddd; padding: 10px; font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; }
.diff-row { margin-bottom: 10px; border-bottom: 1px dashed #eee; padding-bottom: 5px; }
.diff-row:last-child { border-bottom: none; }
.diff-label { color: #888; font-weight: bold; margin-right: 10px; display: block; margin-bottom: 4px; }
.diff-value { color: #111; }
.json-inner { background: #222; color: #7fdbff; padding: 10px; border-radius: 4px; overflow-x: auto; margin: 5px 0; }
.text-inner { white-space: pre-wrap; background: #fff; border: 1px solid #eee; padding: 8px; color: #444; }
.alert-text { color: #D92323; font-weight: bold; }

.item-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 15px; }
.cyber-btn { border: 2px solid #111; padding: 8px 16px; font-weight: bold; cursor: pointer; font-family: 'Anton'; }
.cyber-btn.approve { background: #111; color: #fff; }
.cyber-btn.approve:hover { background: #2ecc71; border-color: #2ecc71; }
.cyber-btn.reject { background: transparent; color: #111; }
.cyber-btn.reject:hover { background: #fff0f0; border-color: #D92323; color: #D92323; }

.loading-box, .empty-box { text-align: center; padding: 50px; color: #999; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #bbb; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>