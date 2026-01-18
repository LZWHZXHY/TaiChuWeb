<template>
  <div class="cyber-chat-container" :class="{ 'dark-mode-active': activeRoom.type === 'dark' }">
    <div class="grid-bg moving-grid" v-if="activeRoom.type !== 'dark'"></div>
    <div class="dark-bg-pattern" v-else></div>

    <header class="chat-header">
      <div class="header-left">
        <h1 class="glitch-title">
          <span class="text-row" v-if="activeRoom.type === 'dark'" style="color: #0f0">DARK_NET</span>
          <span class="text-row" v-else-if="activeRoom.type === 'private'" style="color: #FFD700">SECURE_Line</span>
          <span class="text-row" v-else>COMM_LINK</span>
          
          <span class="text-row red-fill" v-if="activeRoom.type === 'public'">聊天室</span>
          <span class="text-row" style="color: #0f0; margin-left: 20px;" v-else-if="activeRoom.type === 'dark'">加密暗间</span>
          <span class="text-row" style="color: #FFD700; margin-left: 20px;" v-else>机密频道</span>
        </h1>
      </div>
      <div class="header-right">
        <div class="status-block">
          <div class="live-indicator">
            <span class="dot" :class="activeRoom.type"></span> 
            {{ getStatusText(activeRoom.type) }}
          </div>
          <div class="channel-info">// NODE: {{ activeRoom.name }}</div>
        </div>
      </div>
    </header>

    <div class="tech-strip" :class="{ 'dark-strip': activeRoom.type === 'dark', 'gold-strip': activeRoom.type === 'private' }">
      <div class="strip-content" v-if="activeRoom.type === 'public'">
        // NEURAL_LINK_ESTABLISHED // PACKET_LOSS: 0% // PUBLIC_ARCHIVE_ON // 
      </div>
      <div class="strip-content" v-else-if="activeRoom.type === 'private'" style="color: #000">
        // RESTRICTED_ACCESS // AUTHORIZATION_REQUIRED // LOGS_ENCRYPTED //
      </div>
      <div class="strip-content" v-else style="color: #0f0">
        // WARNING: NO_LOGS // EPHEMERAL_MODE // IDENTITY_MASKED // TRACE_FAILED //
      </div>
    </div>

    <div class="chat-bridge">
      
      <aside class="combined-sidebar">
        <div class="sidebar-section channels-section" :class="themeClass">
          <div class="section-header" :class="headerClass">
            <span>// CHANNELS</span>
          </div>
          <div class="channel-list custom-scroll">
            <div 
              v-for="room in rooms" 
              :key="room.id" 
              class="channel-item" 
              :class="{ 
                active: activeRoom.id === room.id,
                'dark-item': room.type === 'dark',
                'gold-item': room.type === 'private'
              }"
              @click="handleRoomClick(room)"
            >
              <span class="prefix">
                {{ room.type === 'dark' ? 'ø' : (room.type === 'private' ? '🔒' : '#') }}
              </span>
              <span class="name">{{ room.name }}</span>
              <span class="tag" v-if="room.type === 'dark'">[ANON]</span>
              <span class="tag" v-if="room.type === 'private'">[PWD]</span>
            </div>
          </div>
        </div>

        <div class="sidebar-section users-section" :class="themeClass">
          <div class="section-header" :class="headerClass">
            <span>> {{ activeRoom.type === 'dark' ? 'GHOST_PROTOCOL' : 'ACTIVE_USERS.exe' }}</span>
            <span class="count" v-if="activeRoom.type !== 'dark'">[{{ onlineCount }}]</span>
          </div>
          
          <div class="user-list custom-scroll" v-if="activeRoom.type !== 'dark'">
            <div v-for="user in onlineUsers" :key="user.id" class="user-row">
              <span class="status-dot" :class="{ 'gold-dot': activeRoom.type === 'private' }"></span>
              <span class="user-name">{{ user.username }}</span>
            </div>
          </div>
          
          <div class="anon-placeholder" v-else>
             <div class="matrix-rain">
               01010100101<br>ANONYMOUS<br>SECURE<br>10101010101
             </div>
          </div>
        </div>
      </aside>

      <main class="chat-main">
        <div class="messages-wrapper" :class="themeClass">
          
          <div class="flow-header">
            <span class="header-title">
              <span class="icon" :style="{ color: themeColor }">■</span> 
              {{ activeRoom.type === 'dark' ? 'ENCRYPTED_STREAM' : 'MESSAGE_STREAM' }}
            </span>
            <div class="user-count" :style="{ color: themeColor }">
              {{ activeRoom.type === 'dark' ? 'ENCRYPTION: AES-256' : 'SIGNAL: STRONG' }}
            </div>
          </div>

          <div class="messages-display custom-scroll" ref="scrollBox" :class="scrollThemeClass">
            <div v-if="loading" class="status-text">[ SYNCING_DATA... ]</div>
            
            <div v-for="(msg, index) in messages" :key="index" 
                 class="msg-entry" 
                 :class="{ 
                   'is-me': msg.isMe, 
                   'is-system': msg.type === 'system',
                   'is-dark': activeRoom.type === 'dark',
                   'is-private': activeRoom.type === 'private'
                 }">
              
              <template v-if="msg.type !== 'system'">
                <div class="msg-avatar" v-if="activeRoom.type !== 'dark'">
                  <img :src="fixAvatarUrl(msg.user.avatar)" @error="handleImgError" />
                </div>
                <div class="msg-avatar anon-avatar" v-else>
                  <span>?</span>
                </div>

                <div class="msg-content">
                  <div class="msg-meta">
                    <span class="msg-user">
                      {{ msg.displayName }}
                    </span>
                    <span class="msg-time">{{ formatTime(msg.time) }}</span>
                  </div>
                  
                  <div class="msg-bubble" :class="bubbleClass(msg.isMe)">
                    <div class="bubble-scanline" v-if="activeRoom.type !== 'dark'"></div>
                    {{ msg.text }}
                  </div>
                </div>
              </template>

              <template v-else>
                <div class="system-msg" :class="systemMsgClass">> {{ msg.text }}</div>
              </template>
            </div>
          </div>

          <div class="chat-input-area" :class="inputAreaClass">
            <div class="input-container">
              <div class="input-prefix" :style="{ color: themeColor }">>></div>
              <textarea 
                v-model="newMessage" 
                @keyup.enter.prevent="sendMessage"
                :placeholder="activeRoom.type === 'dark' ? 'ENCRYPTED_INPUT... (NO LOGS)' : 'INPUT_MESSAGE...'"
                class="cyber-textarea custom-scroll"
                :class="textareaClass"
              ></textarea>
              <button class="send-btn" @click="sendMessage" :disabled="!newMessage.trim()" :class="btnClass">
                <div class="btn-inner">
                  <span class="main">TRANSMIT</span>
                  <span class="sub" :style="{ opacity: 0.7 }">SEND_DATA</span>
                </div>
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>

    <Teleport to="body">
      <div v-if="showAuthModal" class="auth-overlay">
        <div class="auth-box">
          <div class="auth-header">
            <span>SECURE_ACCESS_REQUIRED</span>
            <button class="close-btn" @click="closeAuthModal">X</button>
          </div>
          <div class="auth-body">
            <div class="lock-icon">🔒</div>
            <p>该频道已加密，请输入访问密钥。</p>
            <input 
              ref="pwdInput"
              v-model="inputPassword" 
              type="password" 
              class="auth-input" 
              placeholder="ENTER_PASSWORD"
              @keyup.enter="unlockRoom"
            />
            <div v-if="authError" class="auth-error">> 密钥错误: ACCESS_DENIED</div>
            <button class="auth-confirm-btn" @click="unlockRoom">UNLOCK_CHANNEL</button>
          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue';
import * as signalR from '@microsoft/signalr';
import apiClient from '@/utils/api'; 
import { useAuthStore } from '@/utils/auth';

const authStore = useAuthStore();

// --- 状态定义 ---
const currentUser = computed(() => {
  if (authStore.user) {
    return {
      username: authStore.user.username || '未知用户',
      avatar: authStore.user.avatar || '', 
      id: authStore.user.id
    };
  }
  return { username: '访客', avatar: '', id: null };
});

const rooms = ref([
  { id: 'general', name: '公共大厅', type: 'public', unread: 0 },
  { id: 'dark_mode', name: '暗网禁区', type: 'dark', unread: 0 },
  { id: 'vip_lounge', name: '机密会议室', type: 'private', unread: 0, password: '123' }, 
  { id: 'admin_core', name: '核心层', type: 'private', unread: 0, password: '888' }, 
]);

const activeRoom = ref(rooms.value[0]);
const unlockedRooms = ref(new Set()); 

// 匿名代号缓存 Map <Username, Alias>
const anonAliases = ref(new Map());

// 模态框状态
const showAuthModal = ref(false);
const inputPassword = ref('');
const pendingRoom = ref(null);
const authError = ref(false);
const pwdInput = ref(null);

const newMessage = ref('');
const messages = ref([]);
const loading = ref(false);
const scrollBox = ref(null);
const connection = ref(null);
const isConnected = ref(false);
const onlineUsers = ref([]);

// --- 样式逻辑 ---
const themeClass = computed(() => ({ 'dark-border': activeRoom.value.type === 'dark', 'gold-border': activeRoom.value.type === 'private' }));
const headerClass = computed(() => ({ 'dark-header': activeRoom.value.type === 'dark', 'gold-header': activeRoom.value.type === 'private' }));
const themeColor = computed(() => {
  if (activeRoom.value.type === 'dark') return '#0f0';
  if (activeRoom.value.type === 'private') return '#FFD700';
  return '';
});
const scrollThemeClass = computed(() => ({ 'dark-mode-scroll': activeRoom.value.type === 'dark', 'gold-mode-scroll': activeRoom.value.type === 'private' }));
const inputAreaClass = computed(() => ({ 'dark-input-area': activeRoom.value.type === 'dark', 'gold-input-area': activeRoom.value.type === 'private' }));
const textareaClass = computed(() => ({ 'dark-textarea': activeRoom.value.type === 'dark', 'gold-textarea': activeRoom.value.type === 'private' }));
const btnClass = computed(() => ({ 'dark-btn': activeRoom.value.type === 'dark', 'gold-btn': activeRoom.value.type === 'private' }));
const systemMsgClass = computed(() => ({ 'dark-sys': activeRoom.value.type === 'dark', 'gold-sys': activeRoom.value.type === 'private' }));

const bubbleClass = (isMe) => {
  if (activeRoom.value.type === 'dark') return isMe ? 'dark-bubble is-me-dark' : 'dark-bubble';
  if (activeRoom.value.type === 'private') return isMe ? 'gold-bubble is-me-gold' : 'gold-bubble';
  return '';
};

// --- 辅助逻辑 ---

const getStatusText = (type) => {
  if (type === 'public') return 'SYSTEM_ONLINE';
  if (type === 'dark') return 'UNTRACEABLE';
  if (type === 'private') return 'ENCRYPTED';
  return 'UNKNOWN';
};

// ✨ 核心：生成或获取用户的匿名代号
const getAnonID = (username) => {
  if (!anonAliases.value.has(username)) {
    // 生成 1000-9999 的随机数
    const randomId = Math.floor(1000 + Math.random() * 9000);
    anonAliases.value.set(username, `Anonymous_${randomId}`);
  }
  return anonAliases.value.get(username);
};

const handleRoomClick = (room) => {
  if (room.id === activeRoom.value.id) return;
  if (room.type === 'private' && !unlockedRooms.value.has(room.id)) {
    pendingRoom.value = room;
    inputPassword.value = '';
    authError.value = false;
    showAuthModal.value = true;
    nextTick(() => pwdInput.value?.focus());
  } else {
    switchRoom(room);
  }
};

const unlockRoom = () => {
  if (inputPassword.value === (pendingRoom.value.password || '123')) {
    unlockedRooms.value.add(pendingRoom.value.id);
    switchRoom(pendingRoom.value);
    closeAuthModal();
  } else {
    authError.value = true;
    inputPassword.value = '';
  }
};

const closeAuthModal = () => {
  showAuthModal.value = false;
  pendingRoom.value = null;
};

const switchRoom = async (room) => {
  if (connection.value?.state === signalR.HubConnectionState.Connected && activeRoom.value.id) {
    await connection.value.invoke("LeaveRoom", activeRoom.value.id);
  }
  
  activeRoom.value = room;
  messages.value = []; 
  anonAliases.value.clear(); // 切换房间时清空匿名缓存，重置随机数

  if (room.type !== 'dark') {
    await fetchHistory(room.id);
  } else {
    messages.value.push({ type: 'system', text: '已进入加密频道。数据流单向加密... 历史记录已焚毁...' });
  }

  if (isConnected.value) {
    await connection.value.invoke("JoinRoom", room.id, currentUser.value.username); 
  }
};

const initSignalR = async () => {
  if (!authStore.isAuthenticated) authStore.checkAuth();
  const hubUrl = "https://bianyuzhou.com/chatHub"; 

  connection.value = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { accessTokenFactory: () => authStore.token })
    .withAutomaticReconnect()
    .build();

  connection.value.on("ReceiveMessage", (msg) => {
    let isMe = false;
    let displayName = msg.user.username;

    // ✨ 核心逻辑：区分房间类型处理名字
    if (activeRoom.value.type === 'dark') {
        // 暗网：如果是自己，显示"ME"，否则显示随机匿名代号
        isMe = msg.user.username === currentUser.value.username;
        if (isMe) {
            // 如果是自己，可以显示特殊标记，或者也显示匿名代号
            // 这里选择显示匿名代号 + (ME) 标记
            displayName = `${getAnonID(msg.user.username)} (YOU)`;
        } else {
            displayName = getAnonID(msg.user.username);
        }
    } else {
        // 公开/私密：正常显示
        isMe = msg.user.username === currentUser.value.username;
        displayName = msg.user.username;
    }

    messages.value.push({
      type: 'user', 
      isMe: isMe, 
      user: msg.user, 
      displayName: displayName, // 使用计算出的名字
      text: msg.text, 
      time: msg.time, 
      isAnonymous: msg.isAnonymous
    });
    scrollToBottom();
  });

  connection.value.on("ReceiveSystemMessage", (text) => {
    messages.value.push({ type: 'system', text, time: new Date() });
    scrollToBottom();
  });

  try {
    await connection.value.start();
    isConnected.value = true;
    await switchRoom(activeRoom.value);
  } catch (err) {
    messages.value.push({ type: 'system', text: `ERROR: 连接失败 - ${err.message}` });
  }
};

const fetchHistory = async (roomId) => {
  loading.value = true;
  try {
    const res = await apiClient.get('/ChatMessage/history', { params: { roomId } });
    if (res.data.success) {
      const history = res.data.data.map(m => ({
        ...m,
        isMe: m.user.username === currentUser.value.username,
        displayName: m.user.username // 历史记录永远是公开房/私密房的，所以直接用真名
      }));
      messages.value = history;
      messages.value.push({ type: 'system', text: '--- 历史记录加载完毕 ---', time: new Date() });
    }
  } catch (e) {
    messages.value.push({ type: 'system', text: '--- 无法读取神经档案 ---' });
  } finally {
    loading.value = false;
    scrollToBottom();
  }
};

const sendMessage = async () => {
  const text = newMessage.value.trim();
  if (!text || !isConnected.value) return;
  try {
    await connection.value.invoke("SendMessage", activeRoom.value.id, currentUser.value.username, text, currentUser.value.avatar);
    newMessage.value = '';
  } catch (err) {
    messages.value.push({ type: 'system', text: '❌ 发送失败' });
  }
};

const scrollToBottom = async () => {
  await nextTick();
  if (scrollBox.value) scrollBox.value.scrollTop = scrollBox.value.scrollHeight;
};

watch(currentUser, (newVal) => {
  if (newVal.username !== '访客') onlineUsers.value = [{ id: newVal.id || 1, username: newVal.username }];
}, { immediate: true });
const onlineCount = computed(() => onlineUsers.value.length);
const fixAvatarUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:')) return url;
  const BASE_URL = 'https://bianyuzhou.com/'; 
  return `${BASE_URL}${url.startsWith('/') ? url.substring(1) : url}`;
};
const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };
const formatTime = (t) => t ? new Date(t).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '';

onMounted(() => { initSignalR(); });
onUnmounted(() => { if (connection.value) connection.value.stop(); });
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* =========================================
   0. 全局布局 & 变量
   ========================================= */
.cyber-chat-container {
  --red: #D92323; 
  --black: #111111; 
  --off-white: #F4F1EA; 
  --dark-text: #0f0; 
  --gold: #FFD700;
  
  width: 100%; 
  height: 100%; /* 继承父组件高度 */
  background-color: var(--off-white);
  display: flex; 
  flex-direction: column;
  overflow: hidden; /* 绝对禁止外层滚动 */
  position: relative; 
  font-family: 'Inter', sans-serif;
}

.dark-mode-active { background-color: #050505; }

/* 背景纹理 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; 
}
.dark-bg-pattern { 
  position: absolute; inset: 0; 
  background: radial-gradient(circle, transparent 20%, #000 20%, #000 80%, transparent 80%, transparent) 25px 25px, linear-gradient(#0f0 2px, transparent 2px) 0 -1px, linear-gradient(90deg, #0f0 2px, #000 2px) -1px 0; 
  background-size: 100px 100px, 100px 100px, 50px 50px, 50px 50px; 
  opacity: 0.05; pointer-events: none; 
}

/* =========================================
   1. 头部区域 (Fixed Height)
   ========================================= */
.chat-header { 
  flex-shrink: 0; 
  display: flex; justify-content: space-between; align-items: center; 
  padding: 20px 40px; 
  background: var(--black); color: var(--off-white); 
  border-bottom: 4px solid var(--black); 
  z-index: 10; height: 80px; box-sizing: border-box; 
}

.glitch-title { font-family: 'Anton', sans-serif; font-size: 2.2rem; line-height: 1; }
.red-fill { color: var(--red); }

.status-block { font-family: 'JetBrains Mono'; text-align: right; }
.channel-info { font-size: 0.8rem; opacity: 0.8; margin-top: 4px; }

.live-indicator { font-weight: bold; font-size: 0.9rem; }
.dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 6px; }
.dot.public { background: var(--red); animation: pulse 1s infinite; }
.dot.dark { background: #0f0; box-shadow: 0 0 5px #0f0; }
.dot.private { background: var(--gold); box-shadow: 0 0 5px var(--gold); }

/* 装饰条 */
.tech-strip { 
  flex-shrink: 0; 
  background: var(--red); color: #fff; 
  padding: 4px 0; height: 24px; 
  overflow: hidden; white-space: nowrap; 
  font-family: 'JetBrains Mono'; font-size: 0.7rem; 
  display: flex; align-items: center; 
}
.tech-strip.dark-strip { background: #000; border-bottom: 1px solid #0f0; }
.tech-strip.gold-strip { background: #000; border-bottom: 1px solid var(--gold); color: var(--gold); }

/* =========================================
   2. 主布局桥接 (Flex Core)
   ========================================= */
.chat-bridge { 
  flex: 1; /* 占据剩余空间 */
  min-height: 0; /* 关键：允许内部收缩，防止溢出 */
  display: flex; 
  padding: 20px; gap: 20px; 
  max-width: 1800px; margin: 0 auto; width: 100%; 
  box-sizing: border-box; 
}

/* =========================================
   3. 左侧边栏 (Fixed Width)
   ========================================= */
.combined-sidebar { 
  width: 260px; flex-shrink: 0; 
  display: flex; flex-direction: column; gap: 20px; 
  height: 100%; 
}

.sidebar-section { 
  background: #fff; border: 2px solid var(--black); 
  display: flex; flex-direction: column; 
  transition: 0.3s; overflow: hidden; 
}
.sidebar-section.channels-section { flex: 0 0 auto; /* 高度自适应 */ }
.sidebar-section.users-section { flex: 1; min-height: 0; /* 撑满剩余 */ }

/* 侧边栏 Header */
.section-header { 
  padding: 10px 15px; 
  background: var(--black); color: var(--off-white); 
  font-family: 'JetBrains Mono'; font-size: 0.75rem; font-weight: bold; 
  display: flex; justify-content: space-between; flex-shrink: 0; 
}

/* 侧边栏主题变体 */
.sidebar-section.dark-border { background: #000; border-color: #0f0; }
.section-header.dark-header { background: #000; color: #0f0; border-bottom: 1px solid #0f0; }

.sidebar-section.gold-border { border-color: var(--gold); }
.section-header.gold-header { background: #222; color: var(--gold); border-bottom: 1px solid var(--gold); }

/* 频道列表 */
.channel-list { padding: 10px; overflow-y: auto; max-height: 300px; }
.channel-item { 
  display: flex; align-items: center; 
  padding: 12px; border: 1px solid transparent; 
  cursor: pointer; font-family: 'JetBrains Mono'; transition: 0.2s; 
}
.channel-item:hover { background: rgba(0,0,0,0.05); }
.channel-item.active { background: var(--black); color: var(--off-white); }

/* 频道主题 */
.channel-item.dark-item { color: #888; }
.channel-item.dark-item:hover { color: #0f0; background: #000; }
.channel-item.active.dark-item { background: #000; color: #0f0; border: 1px solid #0f0; }

.channel-item.gold-item { color: #888; }
.channel-item.gold-item:hover { color: var(--gold); background: #222; }
.channel-item.active.gold-item { background: #222; color: var(--gold); border: 1px solid var(--gold); }

.prefix { width: 20px; font-weight: bold; }
.tag { font-size: 0.6rem; margin-left: auto; border: 1px solid #888; padding: 0 4px; }

/* 用户列表 */
.user-list { flex: 1; overflow-y: auto; padding: 10px; }
.user-row { display: flex; align-items: center; gap: 10px; padding: 8px 0; font-size: 0.9rem; font-family: 'JetBrains Mono'; border-bottom: 1px dashed #eee; }
.status-dot { width: 6px; height: 6px; background: #00FF99; border-radius: 50%; box-shadow: 0 0 5px #00FF99; }
.status-dot.gold-dot { background: var(--gold); box-shadow: 0 0 5px var(--gold); }

.anon-placeholder { 
  flex: 1; display: flex; align-items: center; justify-content: center; 
  color: #0f0; text-align: center; opacity: 0.5; 
  font-size: 0.8rem; font-family: 'JetBrains Mono'; 
}

/* =========================================
   4. 右侧聊天主窗口 (Flexible)
   ========================================= */
.chat-main { 
  flex: 1; 
  display: flex; flex-direction: column; 
  min-width: 0; height: 100%; 
}

.messages-wrapper { 
  flex: 1; 
  display: flex; flex-direction: column; 
  min-height: 0; /* 核心：允许子元素收缩 */
  background: #fff; border: 2px solid var(--black); 
  overflow: hidden; transition: 0.3s; 
}
.messages-wrapper.dark-border { border-color: #0f0; background: #000; }
.messages-wrapper.gold-border { border-color: var(--gold); }

/* A. 消息头部 */
.flow-header { 
  flex-shrink: 0; 
  padding: 15px; border-bottom: 2px solid var(--black); 
  display: flex; justify-content: space-between; font-family: 'Anton'; 
}
.dark-border .flow-header { border-bottom-color: #0f0; color: #0f0; }
.gold-border .flow-header { border-bottom-color: var(--gold); color: var(--gold); background: #111; }

/* B. 消息滚动区 (Scrollable) */
.messages-display { 
  flex: 1; /* 撑满剩余空间 */
  overflow-y: auto; /* 允许滚动 */
  padding: 20px; 
  background: #F9F8F5; 
  min-height: 0; 
}
.messages-display.dark-mode-scroll { background: #000; }
.messages-display.gold-mode-scroll { background: #111; }

.status-text { text-align: center; color: #999; font-family: 'JetBrains Mono'; margin-top: 20px; }

/* C. 底部输入区 (Pinned) */
.chat-input-area { 
  flex-shrink: 0; 
  padding: 20px; 
  border-top: 2px solid var(--black); background: #fff; 
}
.chat-input-area.dark-input-area { background: #000; border-top-color: #0f0; }
.chat-input-area.gold-input-area { background: #111; border-top-color: var(--gold); }

.input-container { display: flex; gap: 15px; align-items: flex-start; }
.input-prefix { padding-top: 10px; font-weight: bold; font-family: 'JetBrains Mono'; }

.cyber-textarea { 
  flex: 1; height: 60px; 
  border: 2px solid var(--black); padding: 10px; 
  font-family: 'JetBrains Mono'; outline: none; resize: none; 
  background: #fafafa;
}
.cyber-textarea.dark-textarea { background: #000; border-color: #0f0; color: #0f0; }
.cyber-textarea.gold-textarea { background: #222; border-color: var(--gold); color: var(--gold); }

.send-btn { 
  background: var(--black); color: #fff; 
  border: none; padding: 0 25px; height: 60px;
  cursor: pointer; font-family: 'JetBrains Mono';
}
.send-btn.dark-btn { background: #000; border: 1px solid #0f0; color: #0f0; }
.send-btn.gold-btn { background: #000; border: 1px solid var(--gold); color: var(--gold); }

.btn-inner { display: flex; flex-direction: column; align-items: center; justify-content: center; }
.btn-inner .main { font-weight: bold; font-size: 1rem; }
.btn-inner .sub { font-size: 0.6rem; opacity: 0.7; }

/* =========================================
   5. 消息气泡样式
   ========================================= */
.msg-entry { display: flex; gap: 12px; margin-bottom: 20px; }
.msg-entry.is-me { flex-direction: row-reverse; }

.msg-avatar { width: 40px; height: 40px; border: 2px solid var(--black); flex-shrink: 0; }
.msg-avatar img { width: 100%; height: 100%; object-fit: cover; }
.anon-avatar { border-color: #0f0; background: #000; color: #0f0; display: flex; justify-content: center; align-items: center; font-weight: bold; }

.msg-content { max-width: 70%; display: flex; flex-direction: column; }
.msg-meta { font-family: 'JetBrains Mono'; font-size: 0.75rem; margin-bottom: 4px; display: flex; gap: 10px; color: #666; }
.is-me .msg-meta { flex-direction: row-reverse; }
.is-dark .msg-meta { color: #0f0; }
.is-private .msg-meta { color: var(--gold); }

.msg-bubble { 
  background: #fff; border: 2px solid var(--black); 
  padding: 12px; position: relative; 
  font-size: 0.95rem; line-height: 1.5; 
  box-shadow: 3px 3px 0 var(--black); 
  word-break: break-word;
}
.is-me .msg-bubble { background: var(--black); color: var(--off-white); box-shadow: 3px 3px 0 var(--red); }

/* 暗网气泡 */
.dark-bubble { background: #000 !important; color: #0f0 !important; border-color: #0f0 !important; box-shadow: none !important; }
.is-me-dark { border-style: dashed !important; }

/* 密码房气泡 */
.gold-bubble { background: #222 !important; color: var(--gold) !important; border-color: var(--gold) !important; box-shadow: 3px 3px 0 rgba(255, 215, 0, 0.2) !important; }
.is-me-gold { background: var(--gold) !important; color: #000 !important; }

.system-msg { width: 100%; text-align: center; font-family: 'JetBrains Mono'; font-size: 0.8rem; color: #888; margin: 10px 0; }
.system-msg.dark-sys { color: #0f0; opacity: 0.7; }
.system-msg.gold-sys { color: var(--gold); opacity: 0.7; }

/* =========================================
   6. 密码弹窗 (修复可见性问题)
   ========================================= */
.auth-overlay { 
  position: fixed; inset: 0; 
  background: rgba(0,0,0,0.95); 
  z-index: 9999; 
  display: flex; justify-content: center; align-items: center; 
  backdrop-filter: blur(5px); 
}

.auth-box { 
  width: 420px; 
  background-color: #6767677f !important; /* 强制背景 */
  border: 2px solid var(--gold); 
  box-shadow: 0 0 30px rgba(255, 215, 0, 0.15); 
  color: var(--gold) !important; /* 强制字体颜色 */
  font-family: 'JetBrains Mono', monospace; 
  display: flex; flex-direction: column;
}

.auth-header { 
  background: rgba(255, 215, 0, 0.1); 
  padding: 15px 20px; 
  border-bottom: 1px solid var(--gold); 
  display: flex; justify-content: space-between; align-items: center;
  font-weight: bold; 
}

.close-btn { 
  background: transparent; border: 1px solid var(--gold); color: var(--gold); 
  width: 24px; height: 24px; cursor: pointer; display: flex; align-items: center; justify-content: center;
}
.close-btn:hover { background: var(--gold); color: #000; }

.auth-body { padding: 40px 30px; text-align: center; }
.lock-icon { font-size: 3rem; margin-bottom: 20px; }
.auth-body p { color: var(--gold) !important; margin-bottom: 20px; }

.auth-input { 
  width: 100%; padding: 12px; margin-bottom: 20px; 
  background-color: #00000052 !important; 
  border: 1px solid var(--gold); 
  color: var(--gold) !important; 
  outline: none; text-align: center; font-size: 1.2rem; letter-spacing: 2px;
}
.auth-input::placeholder { color: rgba(160, 143, 42, 0.692); }

.auth-error { color: #ff3333; font-size: 0.8rem; margin-bottom: 15px; animation: blink 0.2s 3; }

.auth-confirm-btn { 
  width: 100%; padding: 12px; 
  background: var(--gold); color: #000; 
  border: none; font-weight: 900; 
  cursor: pointer; transition: 0.2s; 
  font-family: 'Anton', sans-serif; letter-spacing: 1px;
}
.auth-confirm-btn:hover { background: #4b4949; box-shadow: 0 0 15px var(--gold); }

/* =========================================
   7. 滚动条与动画
   ========================================= */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }

.dark-mode-scroll::-webkit-scrollbar-track { background: #111; }
.dark-mode-scroll::-webkit-scrollbar-thumb { background: #0f0; }

.gold-mode-scroll::-webkit-scrollbar-track { background: #111; }
.gold-mode-scroll::-webkit-scrollbar-thumb { background: var(--gold); }

@keyframes blink { 50% { opacity: 0; } }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.3; } 100% { opacity: 1; } }
</style>