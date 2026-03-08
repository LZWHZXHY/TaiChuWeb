import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'

export const useMusicStore = defineStore('music', () => {
  // --- 状态定义 ---
  const isPlaying = ref(false)
  const isGlobalMode = ref(true) // 默认开启 SYNC (全局模式)
  const volume = ref(0.5)
  const playlist = ref([])
  const currentTrack = ref({ title: 'AWAITING_SIGNAL...', artist: 'SYSTEM', url: '' })
  
  // --- 内部变量 ---
  let hubConnection = null
  let audioInstance = null // 保存 Vue 组件传过来的 <audio> DOM 节点引用

  // --- 核心 1：接入深空电台网络 ---
  const initRadio = async (audioElement) => {
    if (hubConnection) return // 防止重复连接
    audioInstance = audioElement
    audioInstance.volume = volume.value

    // 1. 建立 SignalR 连接 (⚠️ 把地址换成你本地 .NET 运行的地址，比如 https://localhost:7123)
    const backendUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001'
    const hubUrl = backendUrl.endsWith('/') ? `${backendUrl}hub/music` : `${backendUrl}/hub/music`
    
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect() // 自动重连装甲
      .build()

    // 2. 监听后端的同步指令：收到精准时间戳
    hubConnection.on("SyncRadioState", (snapshot) => {
      console.log(">> [AUDIO_SYS] 接收到深空电台同步信号:", snapshot)
      if (!snapshot) return

      playlist.value = snapshot.playlist
      currentTrack.value = snapshot.track
      
      // 核心算法：计算歌曲已经播放了多少秒
      const offsetSeconds = (snapshot.serverTime - snapshot.startTime) / 1000

      if (isGlobalMode.value && audioInstance) {
        audioInstance.src = currentTrack.value.url
        audioInstance.currentTime = offsetSeconds
        
        // 尝试自动播放
        audioInstance.play().then(() => {
          isPlaying.value = true
        }).catch(e => {
          console.warn(">> [AUDIO_SYS] 浏览器防噪音策略已拦截。请用户手动点击 [ PLAY ] 解除静音锁定。", e)
          isPlaying.value = false 
        })
      }
    })

    // 3. 监听别人加歌：实时刷新本地歌单
    hubConnection.on("PlaylistUpdated", (newPlaylist) => {
      console.log(">> [AUDIO_SYS] 战术歌单已热更新！", newPlaylist)
      playlist.value = newPlaylist
    })

    // 4. 启动连接
    try {
      await hubConnection.start()
      console.log(">> [AUDIO_SYS] WebSocket 长连接已建立。")
      if (isGlobalMode.value) {
        await hubConnection.invoke("JoinGlobalRadio")
      }
    } catch (err) {
      console.error(">> [AUDIO_SYS] 电台连接失败，请检查 .NET 后端是否启动:", err)
    }
  }

  // --- 核心 2：模式切换逻辑 ---
  const toggleMode = async () => {
    isGlobalMode.value = !isGlobalMode.value
    
    if (isGlobalMode.value) {
      console.log(">> 切换至 [SYNC_ON] 全局同步模式")
      if (hubConnection?.state === signalR.HubConnectionState.Connected) {
        await hubConnection.invoke("JoinGlobalRadio") // 重新要最新进度
      }
    } else {
      console.log(">> 切换至 [PVT_ONLY] 个人战术模式")
      if (hubConnection?.state === signalR.HubConnectionState.Connected) {
        await hubConnection.invoke("LeaveGlobalRadio") // 退出电台组，不再受指挥
      }
    }
  }

  // --- 核心 3：播放/暂停控制 ---
  const togglePlay = () => {
    if (!audioInstance) return

    if (isPlaying.value) {
      audioInstance.pause()
      isPlaying.value = false
    } else {
      // 如果是全局模式，点播放时最好重新向服务器同步一次最精准的时间
      if (isGlobalMode.value && hubConnection?.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke("JoinGlobalRadio")
      } else {
        audioInstance.play().then(() => isPlaying.value = true).catch(e => console.error(e))
      }
    }
  }

  return {
    isPlaying,
    isGlobalMode,
    volume,
    currentTrack,
    playlist,
    initRadio,
    toggleMode,
    togglePlay
  }
})