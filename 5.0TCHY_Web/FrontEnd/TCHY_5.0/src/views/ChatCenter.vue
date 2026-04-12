<template>
  <div class="chat-wrapper">
    <div class="chat-container">
      
      <main class="chat-main">
        <header class="chat-header">
          <div class="header-info">
            <h2 class="room-title">
              <span class="status-dot animate-pulse"></span>
              太虚坛 - 内院 [限时公测开发]
            </h2>
            <p class="room-desc">安全通讯链路已连接</p>
          </div>
          <div class="header-actions">
            <button class="icon-btn" title="聊天室设置">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"></path></svg>
            </button>
          </div>
        </header>

        <div class="message-area" ref="messageAreaRef">
          <div class="message-list">
            <template v-for="msg in messages" :key="msg.id">
              
              <div v-if="msg.type === 'system'" class="system-message">
                <span class="system-text">{{ msg.content }}</span>
              </div>

              <div 
                v-else 
                :class="['message-wrapper', msg.isMine ? 'is-mine' : 'is-others']"
              >
                <img v-if="!msg.isMine" :src="msg.avatar || defaultAvatar" class="msg-avatar" alt="avatar">
                
                <div class="msg-content-box">
                  <div class="msg-meta" v-if="!msg.isMine">
                    <span class="msg-username">{{ msg.username }}</span>
                    <span class="msg-time">{{ msg.time }}</span>
                  </div>
                  <div class="msg-meta justify-end" v-else>
                    <span class="msg-time">{{ msg.time }}</span>
                    <span class="msg-username">{{ msg.username }}</span>
                  </div>

                  <div class="msg-bubble">
                    {{ msg.content }}
                  </div>
                </div>

                <img v-if="msg.isMine" :src="msg.avatar || defaultAvatar" class="msg-avatar" alt="avatar">
              </div>

            </template>
          </div>
        </div>

        <div class="input-area">
          <div class="input-box">
            <button class="attach-btn" title="发送图片或文件">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"></path></svg>
            </button>
            <input 
              type="text" 
              class="chat-input" 
              placeholder="发送消息到超级聊天室..." 
              v-model="inputText"
              @keyup.enter="sendMessage"
            >
            <button 
              class="send-btn" 
              :class="{ 'is-active': inputText.trim().length > 0 }"
              @click="sendMessage"
            >
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"></path></svg>
            </button>
          </div>
        </div>
      </main>

      <aside class="chat-sidebar">
        <div class="sidebar-header">
          <h3 class="sidebar-title">房间节点状态</h3>
        </div>
        <div class="online-list">
          <div class="role-group">当前会话身份</div>
          <div class="online-user">
            <div class="avatar-wrapper">
              <img :src="currentUserAvatar" class="user-avatar" alt="avatar">
              <span class="status-indicator online"></span>
            </div>
            <span class="user-name admin-name">{{ currentUsername }}</span>
          </div>

          <div class="role-group mt-4">网络拓扑说明</div>
          <div class="text-xs text-gray-500 leading-relaxed pr-2">
            当前处于 <strong class="text-indigo-500">{{ roomId }}</strong> 频道。<br><br>
            SignalR 双向通讯流已建立，历史记录模块运转正常。<br><br>
            所有交互数据均已通过量子加密网关处理。
          </div>
        </div>
      </aside>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue'
import apiClient from '@/utils/api.js' 
import { useAuthStore } from '@/utils/auth.js' 
import { useOnlineStore } from '@/stores/online.js'

const authStore = useAuthStore()
const onlineStore = useOnlineStore()

const roomId = 'taixu_inner' 
const defaultAvatar = 'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png'

const messageAreaRef = ref(null)
const inputText = ref('')
const messages = ref([])

// ⚡ 修正：authStore 中的用户信息字段 (通常 store 中已经是小写)
const currentUsername = computed(() => authStore.user?.username || '未知探索者')
const currentUserAvatar = computed(() => {
  return authStore.user?.avatar || authStore.user?.profile?.avatarUrl || defaultAvatar
})

const formatTime = (timeStr) => {
  if (!timeStr) return ''
  const date = new Date(timeStr)
  return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

const scrollToBottom = async () => {
  await nextTick()
  if (messageAreaRef.value) {
    messageAreaRef.value.scrollTop = messageAreaRef.value.scrollHeight
  }
}

// 1. 拉取历史消息 (全量驼峰修正)
const fetchHistory = async () => {
  try {
    const response = await apiClient.get('/ChatMessage/history', {
      params: { roomId }
    })
    const payload = response.data

    if (payload.success && payload.data) {
      messages.value = payload.data.map((m, index) => ({
        id: m.id || `hist_${index}`, // ⚡ 修正：m.id
        type: 'user', 
        isMine: m.user?.username === currentUsername.value, // ⚡ 修正：m.user.username
        username: m.user?.username,
        avatar: m.user?.avatar, // ⚡ 修正：m.user.avatar
        content: m.text, // ⚡ 修正：m.text
        time: formatTime(m.time) // ⚡ 修正：m.time
      }))
      scrollToBottom()
    }
  } catch (error) { console.error('历史档案读取失败:', error) }
}

// 2. SignalR 实时监听 (全量驼峰修正)
const setupSignalR = async () => {
  // 等待连接建立
  let retry = 0;
  while (!onlineStore.chatConnection && retry < 6) {
    await new Promise(r => setTimeout(r, 500));
    retry++;
  }

  const connection = onlineStore.chatConnection
  if (!connection) return

  // 系统广播
  connection.on('ReceiveSystemMessage', (sysMsg) => {
    messages.value.push({ id: `sys_${Date.now()}`, type: 'system', content: sysMsg })
    scrollToBottom()
  })

  // 实时消息
  connection.on('ReceiveMessage', (msgObj) => {
    // ⚡ 核心修正：msgObj 现在的属性都是小写开头
    messages.value.push({
      id: msgObj.id || `msg_${Date.now()}`,
      type: 'user',
      isMine: msgObj.user?.username === currentUsername.value,
      username: msgObj.user?.username,
      avatar: msgObj.user?.avatar,
      content: msgObj.text, 
      time: formatTime(msgObj.time)
    })
    scrollToBottom()
  })

  if (connection.state === 'Connected') {
    await connection.invoke('JoinRoom', roomId, currentUsername.value)
  }
}

const sendMessage = async () => {
  const text = inputText.value.trim();
  if (!text) return;

  const connection = onlineStore.chatConnection;
  if (!connection || connection.state !== 'Connected') {
    console.error('❌ 通讯链路中断');
    return;
  }

  try {
    // 这里的参数名是传给后端的，通常不受返回驼峰影响，但建议保持一致
    await connection.invoke(
      'SendMessage', 
      roomId, 
      currentUsername.value, 
      text, 
      currentUserAvatar.value
    );
    inputText.value = '';
  } catch (error) {
    console.error('消息发射失败:', error);
  }
}

onMounted(async () => {
  const BASE_URL = window.location.hostname === 'localhost' ? 'https://localhost:44359' : 'https://bianyuzhou.com'
  await onlineStore.startChatSignalR(BASE_URL)
  await fetchHistory()
  setupSignalR()
})

onUnmounted(() => {
  const connection = onlineStore.chatConnection
  if (connection) {
    connection.invoke('LeaveRoom', roomId).catch(e => console.error(e))
    connection.off('ReceiveSystemMessage')
    connection.off('ReceiveMessage')
  }
})
</script>

<style scoped>
/* 保持之前那段完美的 CSS 不变！ */
/* --- 布局骨架 --- */
.chat-wrapper {
  background-color: #f1f5f9; 
  min-height: calc(100vh - 60px); 
  padding: 2rem 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: ui-sans-serif, system-ui, -apple-system, sans-serif;
}

.chat-container {
  width: 100%;
  max-width: 72rem;
  height: 80vh; 
  min-height: 600px;
  background-color: #fff;
  border-radius: 1.5rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
  display: flex;
  overflow: hidden;
  border: 1px solid #e2e8f0;
}

/* --- 左侧主区域 --- */
.chat-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  background-color: #fafafa;
}

.chat-header {
  padding: 1.25rem 1.5rem;
  background-color: #fff;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
  z-index: 10;
  box-shadow: 0 1px 2px 0 rgba(0,0,0,0.02);
}
.header-info { display: flex; flex-direction: column; gap: 0.25rem; }
.room-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #0f172a;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.status-dot {
  width: 0.5rem;
  height: 0.5rem;
  background-color: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.2);
}
@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); }
  70% { box-shadow: 0 0 0 6px rgba(16, 185, 129, 0); }
  100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
}
.animate-pulse { animation: pulse 2s infinite; }
.room-desc { font-size: 0.8125rem; color: #64748b; }
.icon-btn {
  width: 2rem;
  height: 2rem;
  color: #94a3b8;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.5rem;
  transition: all 0.2s;
}
.icon-btn:hover { background-color: #f1f5f9; color: #0f172a; }

.message-area {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
  scroll-behavior: smooth;
}
.message-area::-webkit-scrollbar { width: 6px; }
.message-area::-webkit-scrollbar-track { background: transparent; }
.message-area::-webkit-scrollbar-thumb { background-color: #cbd5e1; border-radius: 10px; }
.message-area:hover::-webkit-scrollbar-thumb { background-color: #94a3b8; }

.message-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.system-message {
  text-align: center;
  margin: 1rem 0;
}
.system-text {
  font-size: 0.75rem;
  color: #64748b;
  background-color: #f1f5f9;
  padding: 0.375rem 1rem;
  border-radius: 999px;
}

.message-wrapper {
  display: flex;
  gap: 0.75rem;
  align-items: flex-end;
  animation: slideIn 0.3s ease-out;
}
@keyframes slideIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.is-mine { justify-content: flex-end; }

.msg-avatar {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 0.75rem;
  object-fit: cover;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

.msg-content-box {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  max-width: 70%;
}
.is-mine .msg-content-box { align-items: flex-end; }

.msg-meta {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
}
.justify-end { justify-content: flex-end; }

.msg-username { font-size: 0.8125rem; font-weight: 600; color: #475569; }
.msg-time { font-size: 0.6875rem; color: #94a3b8; }

.msg-bubble {
  padding: 0.75rem 1rem;
  font-size: 0.9375rem;
  line-height: 1.5;
  color: #1e293b;
  background-color: #fff;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.02);
  border-radius: 1rem 1rem 1rem 0.25rem; 
}

.is-mine .msg-bubble {
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  color: #fff;
  border: none;
  box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2);
  border-radius: 1rem 1rem 0.25rem 1rem; 
}

/* 底部输入框 */
.input-area {
  padding: 1.5rem;
  background-color: #fafafa;
  border-top: 1px solid transparent;
}
.input-box {
  display: flex;
  align-items: center;
  background-color: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 1rem;
  padding: 0.5rem 0.5rem 0.5rem 1rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  transition: all 0.2s;
}
.input-box:focus-within {
  border-color: #818cf8;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}

.attach-btn {
  color: #94a3b8;
  width: 2rem;
  height: 2rem;
  display: flex;
  justify-content: center;
  align-items: center;
  border-radius: 0.5rem;
}
.attach-btn:hover { background-color: #f1f5f9; color: #475569; }
.attach-btn svg { width: 1.25rem; height: 1.25rem; }

.chat-input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  padding: 0.5rem 0.75rem;
  font-size: 0.9375rem;
  color: #0f172a;
}
.chat-input::placeholder { color: #94a3b8; }

.send-btn {
  width: 2.5rem;
  height: 2.5rem;
  background-color: #f1f5f9;
  color: #94a3b8;
  border-radius: 0.75rem;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: all 0.3s;
}
.send-btn svg { width: 1.25rem; height: 1.25rem; transform: translateX(-1px); }
.send-btn.is-active {
  background-color: #4f46e5;
  color: #fff;
  box-shadow: 0 2px 4px rgba(79, 70, 229, 0.3);
}
.send-btn.is-active:hover { background-color: #4338ca; transform: translateY(-1px); }
.send-btn.is-active:active { transform: translateY(0); }

/* --- 右侧边栏 --- */
.chat-sidebar {
  width: 16rem;
  background-color: #fff;
  border-left: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
}
@media (max-width: 860px) {
  .chat-sidebar { display: none; }
}

.sidebar-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #f1f5f9;
}
.sidebar-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: #0f172a;
}

.online-list {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
}
.online-list::-webkit-scrollbar { width: 4px; }
.online-list::-webkit-scrollbar-thumb { background-color: #e2e8f0; border-radius: 10px; }

.role-group {
  font-size: 0.6875rem;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.75rem;
}
.mt-4 { margin-top: 1.5rem; }

.online-user {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.375rem 0;
  cursor: pointer;
  border-radius: 0.5rem;
  transition: background-color 0.2s;
}
.online-user:hover { background-color: #f8fafc; }

.avatar-wrapper {
  position: relative;
}
.user-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 0.5rem;
  object-fit: cover;
}
.status-indicator {
  position: absolute;
  bottom: -2px;
  right: -2px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid #fff;
}
.status-indicator.online { background-color: #10b981; }

.user-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: #334155;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.admin-name { color: #eab308; }

.text-xs { font-size: 0.75rem; }
.text-gray-500 { color: #6b7280; }
.text-indigo-500 { color: #6366f1; }
.leading-relaxed { line-height: 1.625; }
.pr-2 { padding-right: 0.5rem; }
</style>