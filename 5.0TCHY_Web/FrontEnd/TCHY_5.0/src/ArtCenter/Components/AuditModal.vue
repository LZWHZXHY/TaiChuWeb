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
                      <div class="diff-banner">💡 检测到属性变更请求：</div>
                      <div v-for="(val, key) in getParsedObject(item.data)" :key="key" class="diff-row">
                        <span class="diff-label">{{ formatKeyName(key) }}:</span>
                        
                        <div class="diff-value">
                          <template v-if="key === 'MetaData' || key === 'meta_data_json'">
                            <pre class="json-inner">{{ formatJson(val) }}</pre>
                          </template>
                          
                          <template v-else-if="key === 'author_id' || key === 'Author_id'">
                            <span class="id-highlight">USER_ID: {{ val }}</span>
                          </template>

                          <template v-else-if="key === 'Description' || key === 'description'">
                            <div class="text-inner">{{ val || '(EMPTY_DATA)' }}</div>
                          </template>

                          <template v-else>
                            <span :class="{ 'val-important': key === 'Name' }">{{ val }}</span>
                          </template>
                        </div>
                      </div>
                    </template>

                    <template v-else-if="item.type === 'CreateNode'">
                      <div class="diff-banner">🆕 初始节点数据存档：</div>
                      <div class="diff-row">
                        <pre class="json-inner">{{ formatJson(item.data) }}</pre>
                      </div>
                    </template>

                    <template v-else-if="item.type === 'DeleteNode'">
                      <div class="alert-text">⚠️ 警告: 该操作将执行数据抹除，请谨慎核实。</div>
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
    // 后端现在返回的是合并后的列表
    auditList.value = res.data
  } catch (e) { 
    console.error("无法调取审核流:", e) 
  } finally { 
    loading.value = false 
  }
}

const handleAudit = async (item, isApproved) => {
  const actionText = isApproved ? "核准通过" : "驳回申请"
  if (!confirm(`确认要 [${actionText}] 这条指令吗？`)) return
  
  try {
    // 这里的 item.id 在 UpdateNode 类型下是 IpChangeLog 的 ID
    // 在 CreateNode 类型下是 Settings 表里的临时 ID
    await apiClient.post('/Audit/approve', {
      TargetId: item.id, 
      Type: item.type,
      IsApproved: isApproved
    })
    alert(`指令已下达: ${actionText}`)
    fetchAudits()
  } catch (e) { 
    alert("系统指令执行失败，请检查网络连接") 
  }
}

const formatDate = (t) => t ? new Date(t).toLocaleString() : 'N/A'
const formatType = (t) => ({ 'CreateNode': 'NEW_NODE', 'UpdateNode': 'UPDATE_REQ', 'DeleteNode': 'PURGE' }[t] || t)

// 转换键名为更易读的形式
const formatKeyName = (key) => {
  const map = {
    'Name': '节点名称',
    'name': '节点名称',
    'Description': '详细描述',
    'description': '详细描述',
    'Type': '分类标签',
    'type': '分类标签',
    'Author': '作者署名',
    'author': '作者署名',
    'author_id': '作者用户ID',
    'Author_id': '作者用户ID',
    'ParentId': '父节点ID',
    'MetaData': '扩展元数据'
  }
  return map[key] || key.toUpperCase()
}

// 格式化 JSON 显示
const formatJson = (val) => {
  if (!val) return '{}'
  if (typeof val === 'string') {
    try {
      return JSON.stringify(JSON.parse(val), null, 2)
    } catch {
      return val
    }
  }
  return JSON.stringify(val, null, 2)
}

// 解析数据字符串为对象
const getParsedObject = (data) => {
  if (!data) return {}
  if (typeof data === 'object') return data
  try { return JSON.parse(data) } catch { return {} }
}

watch(() => props.show, (val) => { if (val) fetchAudits() })
</script>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 3000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); }
.modal-card { width: 750px; max-width: 95vw; height: 85vh; background: #F4F1EA; border: 3px solid #111; display: flex; flex-direction: column; box-shadow: 15px 15px 0 rgba(0,0,0,0.5); }
.modal-header { background: #111; color: #fff; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-family: 'Anton', sans-serif; font-size: 1.2rem; letter-spacing: 2px; }
.close-btn { background: none; border: none; color: #fff; font-size: 1.5rem; cursor: pointer; }
.modal-body { padding: 25px; flex: 1; overflow-y: auto; background: #e5e5e5; }

.audit-item { background: #fff; border: 2px solid #111; margin-bottom: 25px; padding: 18px; border-left-width: 10px; position: relative; }
.audit-item.createnode { border-left-color: #2ecc71; } /* 绿色-创建 */
.audit-item.updatenode { border-left-color: #3498db; } /* 蓝色-更新 */
.audit-item.deletenode { border-left-color: #D92323; } /* 红色-删除 */

.item-header { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.75rem; font-family: 'JetBrains Mono'; font-weight: bold; border-bottom: 1px solid #eee; padding-bottom: 8px; }
.type-tag { color: #111; }
.time-tag { color: #999; }

.item-title { font-size: 1.1rem; font-weight: 900; color: #111; margin-bottom: 15px; }

/* 详情展示区 */
.diff-container { background: #fdfdfd; border: 1px solid #ccc; padding: 15px; font-family: 'JetBrains Mono', monospace; font-size: 0.85rem; }
.diff-banner { color: #111; font-weight: bold; border-bottom: 2px solid #111; padding-bottom: 5px; margin-bottom: 12px; font-size: 0.75rem; }
.diff-row { margin-bottom: 12px; }
.diff-label { color: #666; font-weight: bold; margin-bottom: 4px; display: block; font-size: 0.7rem; text-transform: uppercase; }
.diff-value { color: #111; padding-left: 10px; border-left: 2px solid #eee; }

.json-inner { background: #1a1a1a; color: #a6e22e; padding: 12px; border-radius: 4px; overflow-x: auto; margin: 8px 0; font-size: 0.8rem; line-height: 1.4; }
.text-inner { white-space: pre-wrap; background: #fff; border: 1px dashed #ccc; padding: 10px; color: #333; line-height: 1.6; }

.id-highlight { background: #111; color: #fff; padding: 2px 8px; font-weight: bold; }
.val-important { font-weight: bold; color: #D92323; font-size: 1rem; }
.alert-text { color: #D92323; font-weight: bold; padding: 10px; background: #fff0f0; border: 1px solid #D92323; }

.item-actions { display: flex; justify-content: flex-end; gap: 15px; margin-top: 20px; }
.cyber-btn { border: 2px solid #111; padding: 10px 22px; font-weight: bold; cursor: pointer; font-family: 'Anton'; font-size: 0.9rem; transition: 0.2s; }
.cyber-btn.approve { background: #111; color: #fff; }
.cyber-btn.approve:hover { background: #2ecc71; border-color: #2ecc71; transform: translate(-2px, -2px); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.cyber-btn.reject { background: transparent; color: #111; }
.cyber-btn.reject:hover { background: #fff0f0; border-color: #D92323; color: #D92323; }

.loading-box, .empty-box { text-align: center; padding: 60px; color: #999; font-family: 'JetBrains Mono'; font-weight: bold; letter-spacing: 2px; }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>