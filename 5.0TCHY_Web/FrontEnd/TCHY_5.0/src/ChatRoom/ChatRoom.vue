<template>
  <div class="cyber-chat-container" :class="{ 'dark-mode-active': activeRoom.type === 'dark' }">
    <div class="grid-bg moving-grid" v-if="activeRoom.type === 'public'"></div>
    <div class="dark-bg-pattern" v-else></div>

    <header class="chat-header">
      <div class="header-left">
        <h1 class="glitch-title">
          <span class="text-row" v-if="activeRoom.type === 'public'">COMM_LINK</span>
          <span class="text-row" v-else style="color: #0f0">DARK_NET</span>
          
          <span class="text-row red-fill" v-if="activeRoom.type === 'public'">聊天室</span>
          <span class="text-row" style="color: #0f0; margin-left: 20px;" v-else>加密暗间</span>
        </h1>
      </div>
      <div class="header-right">
        <div class="status-block">
          <div class="live-indicator">
            <span class="dot" :class="activeRoom.type"></span> 
            {{ activeRoom.type === 'public' ? 'SYSTEM_ONLINE' : 'UNTRACEABLE' }}
          </div>
          <div class="channel-info">// NODE: {{ activeRoom.name }}</div>
        </div>
      </div>
    </header>

    <div class="tech-strip" :class="{ 'dark-strip': activeRoom.type === 'dark' }">
      <div class="strip-content" v-if="activeRoom.type === 'public'">
        // NEURAL_LINK_ESTABLISHED // PACKET_LOSS: 0% // PUBLIC_ARCHIVE_ON // 
      </div>
      <div class="strip-content" v-else style="color: #0f0">
        // WARNING: NO_LOGS // EPHEMERAL_MODE // IDENTITY_MASKED // TRACE_FAILED //
      </div>
    </div>

    <div class="chat-bridge">
      
      <aside class="channel-sidebar">
        <div class="cyber-card full-height" :class="{ 'dark-border': activeRoom.type === 'dark' }">
          <div class="card-label-strip"><span>// CHANNELS</span></div>
          <div class="channel-list custom-scroll">
            <div 
              v-for="room in rooms" 
              :key="room.id" 
              class="channel-item" 
              :class="{ 
                active: activeRoom.id === room.id,
                'dark-item': room.type === 'dark' 
              }"
              @click="switchRoom(room)"
            >
              <span class="prefix">{{ room.type === 'dark' ? 'ø' : '#' }}</span>
              <span class="name">{{ room.name }}</span>
              <span class="tag" v-if="room.type === 'dark'">[ANON]</span>
            </div>
          </div>
        </div>
      </aside>

      <main class="chat-main">
        <div class="messages-wrapper cyber-card" :class="{ 'dark-border': activeRoom.type === 'dark' }">
          
          <div class="flow-header">
            <span class="header-title">
              <span class="icon">{{ activeRoom.type === 'dark' ? 'ø' : '■' }}</span> 
              {{ activeRoom.type === 'public' ? 'MESSAGE_STREAM' : 'ENCRYPTED_STREAM' }}
            </span>
            <div class="user-count">
              {{ activeRoom.type === 'public' ? `ONLINE: ${onlineCount}` : 'USERS: HIDDEN' }}
            </div>
          </div>

          <div class="messages-display custom-scroll" ref="scrollBox" :class="{ 'dark-mode-scroll': activeRoom.type === 'dark' }">
            <div v-if="loading" class="status-text">[ SYNCING_DATA... ]</div>
            
            <div v-for="(msg, index) in messages" :key="index" 
                 class="msg-entry" 
                 :class="{ 
                   'is-me': msg.isMe, 
                   'is-system': msg.type === 'system',
                   'is-dark': activeRoom.type === 'dark'
                 }">
              
              <template v-if="msg.type !== 'system'">
                <div class="msg-avatar" v-if="activeRoom.type === 'public'">
                  <img :src="fixAvatarUrl(msg.user.avatar)" @error="handleImgError" />
                </div>
                <div class="msg-avatar anon-avatar" v-else>
                  <span>?</span>
                </div>

                <div class="msg-content">
                  <div class="msg-meta">
                    <span class="msg-user">
                      {{ activeRoom.type === 'public' ? msg.user.username : '######' }}
                    </span>
                    <span class="msg-time">{{ formatTime(msg.time) }}</span>
                  </div>
                  
                  <div class="msg-bubble" :class="{ 'dark-bubble': activeRoom.type === 'dark' }">
                    <div class="bubble-scanline" v-if="activeRoom.type === 'public'"></div>
                    {{ msg.text }}
                  </div>
                </div>
              </template>

              <template v-else>
                <div class="system-msg" :class="{ 'dark-sys': activeRoom.type === 'dark' }">> {{ msg.text }}</div>
              </template>
            </div>
          </div>

          <div class="chat-input-area" :class="{ 'dark-input-area': activeRoom.type === 'dark' }">
            <div class="input-container">
              <div class="input-prefix" :style="{ color: activeRoom.type === 'dark' ? '#0f0' : '' }">>></div>
              <textarea 
                v-model="newMessage" 
                @keyup.enter.prevent="sendMessage"
                :placeholder="activeRoom.type === 'public' ? 'INPUT_MESSAGE...' : 'ENCRYPTED_INPUT... (NO LOGS)'"
                class="cyber-textarea custom-scroll"
                :class="{ 'dark-textarea': activeRoom.type === 'dark' }"
              ></textarea>
              <button class="send-btn" @click="sendMessage" :disabled="!newMessage.trim()" :class="{ 'dark-btn': activeRoom.type === 'dark' }">
                <div class="btn-inner">
                  <span class="main">TRANSMIT</span>
                  <span class="sub" v-if="activeRoom.type === 'public'">SEND_DATA</span>
                  <span class="sub" v-else>NO_TRACE</span>
                </div>
              </button>
            </div>
          </div>
        </div>
      </main>

      <aside class="users-sidebar">
        <div class="cyber-terminal" :class="{ 'dark-border': activeRoom.type === 'dark' }">
          <div class="terminal-header" :style="{ color: activeRoom.type === 'dark' ? '#0f0' : '' }">
            > {{ activeRoom.type === 'public' ? 'ACTIVE_USERS.exe' : 'GHOST_PROTOCOL' }}
          </div>
          
          <div class="user-list custom-scroll" v-if="activeRoom.type === 'public'">
            <div v-for="user in onlineUsers" :key="user.id" class="user-row">
              <span class="status-dot"></span>
              <span class="user-name">{{ user.username }}</span>
            </div>
          </div>
          
          <div class="anon-placeholder" v-else>
             <div class="matrix-rain">
               01010100101<br>
               ANONYMOUS<br>
               CONNECTION<br>
               SECURE<br>
               10101010101
             </div>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue';
import * as signalR from '@microsoft/signalr';
import apiClient from '@/utils/api'; 
// 1. 引入 Pinia Store
import { useAuthStore } from '@/utils/auth';

// ==========================================
// 1. 用户状态管理 (基于 Pinia)
// ==========================================
const authStore = useAuthStore();

// 计算当前用户 (直接从 Store 获取，不再手动解析 Token)
const currentUser = computed(() => {
  if (authStore.user) {
    return {
      username: authStore.user.username || '未知用户',
      // 如果 Store 里没有存头像，这里留空，后端会负责查库补充
      avatar: authStore.user.avatar || '', 
      id: authStore.user.id
    };
  }
  return { username: '访客', avatar: '', id: null };
});

// ==========================================
// 2. 房间配置
// ==========================================
const rooms = ref([
  { id: 'general', name: '公共大厅', type: 'public', unread: 0 },
  { id: 'dark_mode', name: '暗网禁区', type: 'dark', unread: 0 },
]);
const activeRoom = ref(rooms.value[0]);

// ==========================================
// 3. 聊天核心状态
// ==========================================
const newMessage = ref('');
const messages = ref([]);
const loading = ref(false);
const scrollBox = ref(null);
const connection = ref(null);
const isConnected = ref(false);

// 在线列表 (这里简单实现：只显示自己，完整功能需要后端推送在线名单)
const onlineUsers = ref([]);

// 监听用户变化，更新在线列表显示
watch(currentUser, (newVal) => {
  if (newVal.username !== '访客') {
    onlineUsers.value = [{ id: newVal.id || 1, username: newVal.username }];
  }
}, { immediate: true });

const onlineCount = computed(() => onlineUsers.value.length);

// ==========================================
// 4. SignalR 初始化
// ==========================================
const initSignalR = async () => {
  // 确保 Store 状态是最新的
  if (!authStore.isAuthenticated) {
    authStore.checkAuth();
  }

  // 从 Store 获取 Token
  const token = authStore.token;
  if (!token) {
    console.warn("⚠️ 未检测到 Token，将以匿名身份连接");
  }

  // ⚠️ 这里的 URL 必须和你后端 launchSettings.json 里的 https 端口一致
  const hubUrl = "https://bianyuzhou.com/chatHub"; 

  connection.value = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      // 动态获取 Token，确保重连时使用最新 Token
      accessTokenFactory: () => authStore.token 
    })
    .withAutomaticReconnect()
    .build();

  // --- 监听：接收消息 ---
  connection.value.on("ReceiveMessage", (msg) => {
    // 判断是否是自己发的
    let isMe = false;
    
    // 公开房：通过用户名对比
    if (activeRoom.value.type === 'public') {
        isMe = msg.user.username === currentUser.value.username;
    }
    // 暗网：后端返回的是 "######"，无法通过名字判断
    // 如果你想在暗网也显示 "isMe" 样式，可以在 sendMessage 时在本地暂存一个 ID 进行比对，
    // 但目前的逻辑是暗网谁都不知道是谁，包括自己，所以 isMe 为 false 也没问题。

    messages.value.push({
      type: 'user',
      isMe: isMe, 
      user: msg.user,
      text: msg.text,
      time: msg.time,
      isAnonymous: msg.isAnonymous // 后端传回的标记
    });
    scrollToBottom();
  });

  // --- 监听：系统消息 ---
  connection.value.on("ReceiveSystemMessage", (text) => {
    messages.value.push({ type: 'system', text, time: new Date() });
    scrollToBottom();
  });

  // --- 开始连接 ---
  try {
    await connection.value.start();
    console.log("✅ SignalR 连接成功!");
    isConnected.value = true;
    
    // 连接成功后，进入当前选中的房间
    await switchRoom(activeRoom.value);
  } catch (err) {
    console.error("❌ SignalR 连接失败:", err);
    messages.value.push({ type: 'system', text: `ERROR: 连接中枢失败 - ${err.message}` });
  }
};

// ==========================================
// 5. 获取历史记录 (仅公开房)
// ==========================================
const fetchHistory = async (roomId) => {
  loading.value = true;
  try {
    // 调用 ChatMessageController 接口
    const res = await apiClient.get('/ChatMessage/history', { params: { roomId } });
    
    if (res.data.success) {
      // 映射历史消息，标记 isMe
      const history = res.data.data.map(m => ({
        ...m,
        isMe: m.user.username === currentUser.value.username
      }));
      
      messages.value = history;
      messages.value.push({ type: 'system', text: '--- 历史记录加载完毕 ---', time: new Date() });
    }
  } catch (e) {
    console.error("获取历史记录失败", e);
    messages.value.push({ type: 'system', text: '--- 无法读取神经档案 ---' });
  } finally {
    loading.value = false;
    scrollToBottom();
  }
};

// ==========================================
// 6. 切换房间逻辑
// ==========================================
const switchRoom = async (room) => {
  // 如果已连接，先离开旧房间
  if (connection.value?.state === signalR.HubConnectionState.Connected && activeRoom.value.id) {
    await connection.value.invoke("LeaveRoom", activeRoom.value.id);
  }

  activeRoom.value = room;
  messages.value = []; // 清屏

  // 分支逻辑
  if (room.type === 'public') {
    // --> 公开房：加载历史 -> 加入 SignalR
    await fetchHistory(room.id);
    if (isConnected.value) {
       await connection.value.invoke("JoinRoom", room.id, currentUser.value.username);
    }
  } else {
    // --> 暗网：不加载历史 -> 提示 -> 加入 SignalR
    messages.value.push({ type: 'system', text: '已进入加密频道。数据流单向加密... 历史记录已焚毁...' });
    if (isConnected.value) {
       // 暗网模式下，传什么名字都行，后端会强制改成匿名
       await connection.value.invoke("JoinRoom", room.id, currentUser.value.username); 
    }
  }
};

// ==========================================
// 7. 发送消息
// ==========================================
const sendMessage = async () => {
  const text = newMessage.value.trim();
  if (!text) return;
  
  if (!isConnected.value) {
    messages.value.push({ type: 'system', text: '⚠️ 连接已断开，无法发送信号' });
    return;
  }

  try {
    await connection.value.invoke(
      "SendMessage", 
      activeRoom.value.id, 
      currentUser.value.username, 
      text, 
      currentUser.value.avatar // 后端公开房会忽略此值去查库，暗网会忽略此值置空
    );
    newMessage.value = '';
  } catch (err) {
    console.error("发送失败:", err);
    messages.value.push({ type: 'system', text: '❌ 信号传输失败' });
  }
};

// ==========================================
// 8. 辅助函数
// ==========================================
const scrollToBottom = async () => {
  await nextTick();
  if (scrollBox.value) {
    scrollBox.value.scrollTop = scrollBox.value.scrollHeight;
  }
};

const fixAvatarUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:')) return url;
  
  // ⚠️ 替换为你的后端实际地址，确保图片能访问
  const BASE_URL = 'https://bianyuzhou.com/'; 
  
  // 处理路径前面的斜杠，防止双斜杠
  let cleanUrl = url.startsWith('/') ? url.substring(1) : url;
  return `${BASE_URL}${cleanUrl}`;
};

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

const formatTime = (t) => {
  if (!t) return '';
  return new Date(t).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

// ==========================================
// 9. 生命周期
// ==========================================
onMounted(() => {
  initSignalR();
});

onUnmounted(() => {
  if (connection.value) {
    connection.value.stop();
  }
});
</script>

<style scoped>
/* 核心变量 */
.cyber-chat-container {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --dark-text: #0f0;
  --dark-bg: #000;
  
  width: 100%; 
  height: 100vh; /* 强制占满视口高度 */
  background-color: var(--off-white);
  display: flex; 
  flex-direction: column;
  overflow: hidden; /* 防止最外层出现滚动条 */
  position: relative;
  transition: background 0.3s;
}

/* --- Dark Mode 全局覆盖 --- */
.dark-mode-active {
  background-color: #050505;
}

/* 背景 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none;
}
.dark-bg-pattern {
  position: absolute; inset: 0;
  background: radial-gradient(circle, transparent 20%, #000 20%, #000 80%, transparent 80%, transparent), radial-gradient(circle, transparent 20%, #000 20%, #000 80%, transparent 80%, transparent) 25px 25px, linear-gradient(#0f0 2px, transparent 2px) 0 -1px, linear-gradient(90deg, #0f0 2px, #000 2px) -1px 0;
  background-size: 100px 100px, 100px 100px, 50px 50px, 50px 50px;
  opacity: 0.05; pointer-events: none;
}

/* 头部 (固定高度) */
.chat-header {
  flex-shrink: 0; /* 防止被压缩 */
  display: flex; justify-content: space-between; align-items: center;
  padding: 20px 40px; background: var(--black); color: var(--off-white);
  border-bottom: 4px solid var(--black); z-index: 10;
  height: ;
}
.glitch-title { font-family: 'Anton', sans-serif; font-size: 2.5rem; line-height: 1; }
.red-fill { color: var(--red); }
.status-block { font-family: 'JetBrains Mono'; text-align: right; }
.dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; }
.dot.public { background: var(--red); animation: pulse 1s infinite; }
.dot.dark { background: #0f0; box-shadow: 0 0 5px #0f0; }

.tech-strip { 
  flex-shrink: 0; /* 防止被压缩 */
  background: var(--red); color: #fff; padding: 4px 0; 
  overflow: hidden; white-space: nowrap; font-family: 'JetBrains Mono'; font-size: 0.7rem; 
}
.tech-strip.dark-strip { background: #000; border-bottom: 1px solid #0f0; }

/* 主布局桥接 (占据剩余高度) */
.chat-bridge { 
  flex: 1; /* 占据 header 和 strip 剩下的所有空间 */
  min-height: 0; /* 关键：允许 flex 子元素小于内容高度，实现内部滚动 */
  display: flex; 
  padding: 20px; gap: 20px; 
  max-width: 1800px; margin: 0 auto; width: 100%; box-sizing: border-box; 
}

/* 侧边栏 */
.channel-sidebar { width: 240px; display: flex; flex-direction: column; }
.users-sidebar { width: 260px; display: flex; flex-direction: column; }

.cyber-card.full-height {
  height: 100%; /* 撑满父容器 */
  display: flex; flex-direction: column;
}

/* 中间聊天区 */
.chat-main { 
  flex: 1; 
  display: flex; 
  flex-direction: column; 
  min-width: 0; 
  min-height: 0; /* 关键 */
}

/* 消息包装器 */
.messages-wrapper {
  flex: 1; /* 撑满 chat-main */
  display: flex;
  flex-direction: column;
  min-height: 0; /* 关键：限制高度不被内容撑开 */
  background: #fff; 
  border: 2px solid var(--black); 
  overflow: hidden;
  transition: 0.3s;
}
.messages-wrapper.dark-border { border-color: #0f0; background: #000; box-shadow: 0 0 10px rgba(0,255,0,0.1); }

/* 消息区头部 (固定) */
.flow-header { 
  flex-shrink: 0;
  padding: 15px; border-bottom: 2px solid var(--black); 
  display: flex; justify-content: space-between; font-family: 'Anton'; 
}
.dark-border .flow-header { border-bottom-color: #0f0; color: #0f0; }

/* 消息滚动列表 (核心滚动区) */
.messages-display { 
  flex: 1; /* 占据剩余空间 */
  overflow-y: auto; /* 开启滚动 */
  padding: 20px; 
  background: #F9F8F5; 
  height: 0; /* 配合 flex:1 强制使用 flex 计算高度 */
}
.messages-display.dark-mode-scroll { background: #000; }

/* 底部输入区 (固定) */
.chat-input-area { 
  flex-shrink: 0;
  padding: 20px; border-top: 2px solid var(--black); background: #fff; 
}
.chat-input-area.dark-input-area { background: #000; border-top-color: #0f0; }

/* 其他样式保持不变 */
.channel-list { padding: 10px; flex: 1; overflow-y: auto; }
.channel-item { display: flex; align-items: center; padding: 12px; border: 1px solid transparent; cursor: pointer; font-family: 'JetBrains Mono'; transition: 0.2s; }
.channel-item:hover { background: rgba(0,0,0,0.05); }
.channel-item.active { background: var(--black); color: var(--off-white); }
.channel-item.dark-item { color: #888; }
.channel-item.dark-item:hover { color: #0f0; text-shadow: 0 0 5px #0f0; background: #000; }
.channel-item.active.dark-item { background: #000; color: #0f0; border: 1px solid #0f0; }
.tag { font-size: 0.6rem; margin-left: auto; border: 1px solid #888; padding: 0 4px; }

.msg-entry { display: flex; gap: 12px; margin-bottom: 20px; }
.msg-entry.is-me { flex-direction: row-reverse; }
.msg-avatar { width: 40px; height: 40px; border: 2px solid var(--black); flex-shrink: 0; }
.msg-avatar img { width: 100%; height: 100%; object-fit: cover; }
.anon-avatar { border-color: #0f0; background: #000; color: #0f0; display: flex; justify-content: center; align-items: center; font-weight: bold; }
.msg-content { max-width: 70%; }
.msg-meta { font-family: 'JetBrains Mono'; font-size: 0.75rem; margin-bottom: 4px; display: flex; gap: 10px; }
.is-me .msg-meta { flex-direction: row-reverse; }
.is-dark .msg-meta { color: #0f0; }
.msg-bubble { background: #fff; border: 2px solid var(--black); padding: 12px; position: relative; font-size: 0.95rem; line-height: 1.5; box-shadow: 3px 3px 0 var(--black); }
.is-me .msg-bubble { background: var(--black); color: var(--off-white); box-shadow: 3px 3px 0 var(--red); }
.dark-bubble { background: #000 !important; color: #0f0 !important; border-color: #0f0 !important; box-shadow: none !important; text-shadow: 0 0 2px #0f0; }
.is-me .dark-bubble { border-style: dashed !important; }

.input-container { display: flex; gap: 15px; }
.cyber-textarea { flex: 1; height: 60px; border: 2px solid var(--black); padding: 10px; font-family: 'JetBrains Mono'; outline: none; resize: none; }
.cyber-textarea.dark-textarea { background: #000; border-color: #0f0; color: #0f0; }
.send-btn { background: var(--black); color: #fff; border: none; padding: 0 25px; cursor: pointer; }
.send-btn.dark-btn { background: #000; border: 1px solid #0f0; color: #0f0; }
.send-btn.dark-btn:hover { background: #0f0; color: #000; }

.cyber-terminal { background: var(--black); color: var(--off-white); font-family: 'JetBrains Mono'; padding: 15px; border: 2px solid var(--black); height: 100%; display: flex; flex-direction: column; }
.cyber-terminal.dark-border { border-color: #0f0; background: #000; box-shadow: 0 0 10px rgba(0,255,0,0.1); }
.user-list { flex: 1; overflow-y: auto; }
.user-row { display: flex; align-items: center; gap: 10px; padding: 8px 0; font-size: 0.9rem; }
.status-dot { width: 6px; height: 6px; background: #00FF99; border-radius: 50%; box-shadow: 0 0 5px #00FF99; }
.anon-placeholder { flex: 1; display: flex; align-items: center; justify-content: center; color: #0f0; text-align: center; opacity: 0.5; font-size: 0.8rem; }

.card-label-strip { background: var(--black); color: var(--off-white); padding: 4px 10px; font-family: 'JetBrains Mono'; font-size: 0.7rem; flex-shrink: 0; }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.dark-mode-scroll::-webkit-scrollbar-track { background: #111; }
.dark-mode-scroll::-webkit-scrollbar-thumb { background: #0f0; }

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.3; } 100% { opacity: 1; } }
</style>