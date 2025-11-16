<template>
  <div class="messages-panel">
    <h3>消息中心</h3>
    <div class="tabs-bar">
      <button v-for="type in msgTypes" :key="type.id"
              :class="['message-tab-btn', {active: activeType === type.id}]"
              @click="activeType = type.id">{{ type.name }}</button>
    </div>
    <div class="msgs-content">
      <template v-if="activeType==='system'">
        <div v-if="systemNotices.length">
          <div v-for="(msg,i) in systemNotices" :key="i" class="msg-item system">
            <span class="msg-date">{{ formatTime(msg.time) }}</span>
            <span class="msg-content">{{ msg.content }}</span>
          </div>
        </div>
        <div v-else class="msg-empty">暂无系统通知</div>
      </template>
      <template v-if="activeType==='friend'">
        <div v-if="friendMsgs.length">
          <div v-for="(msg,i) in friendMsgs" :key="i" class="msg-item friend">
            <span class="msg-date">{{ formatTime(msg.time) }}</span>
            <span class="msg-from">[{{ msg.from }}]</span>
            <span class="msg-content">{{ msg.content }}</span>
          </div>
        </div>
        <div v-else class="msg-empty">暂无好友消息</div>
      </template>
      <!-- 预留更多类型... -->
    </div>
  </div>
</template>
<script setup>
import { ref } from 'vue'

defineProps({
  user: { type: Object, required: true },
  isMe: { type: Boolean, default: false }
})

// 消息类型Tab（可扩展）
const msgTypes = [
  { id: 'system', name: '系统通知' },
  { id: 'friend', name: '好友消息' }
  // { id: 'group', name: '群组消息'}, ...
]
const activeType = ref('system')

// 例子数据（后续可替换为API获取）
const systemNotices = ref([
  { content: "系统维护将于11/20凌晨2点进行。", time: "2025-11-16T11:00:00" },
  { content: "安全功能升级已上线。", time: "2025-11-15T10:30:00" }
])
const friendMsgs = ref([
  { from: "小明", content: "在吗？一起画画呀", time: "2025-11-16T09:21:00" }
])
function formatTime(dt) {
  try { return new Date(dt).toLocaleString('zh-CN', { hour12: false }) } catch { return dt }
}
</script>
<style scoped>
.messages-panel { padding: 8px; }
.tabs-bar { margin: 18px 0 10px 0; display:flex; gap:15px;}
.message-tab-btn { border:none; background:none; padding:6px 20px; border-radius:6px; cursor:pointer; color:#888;}
.message-tab-btn.active { background: #eef3ff; color: #217dbb; font-weight:700;}
.msgs-content { min-height: 70px;}
.msg-item { padding: 9px 0; border-bottom:1px solid #eee; font-size: 15px;}
.msg-item:last-child { border-bottom: none;}
.msg-item .msg-date { color: #aaa; font-size: 12px; margin-right: 12px;}
.msg-item.system .msg-content { color: #ce8500;}
.msg-item.friend .msg-from { color: #0b887d; margin-right:6px;}
.msg-empty { text-align: center; color: #bbb; padding: 38px 0 }
</style>