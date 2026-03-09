import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'

export const useMusicStore = defineStore('music', () => {
  const isPlaying = ref(false)
  const isGlobalMode = ref(true) 
  const volume = ref(0.5)
  const playlist = ref([])
  const currentTrack = ref({ title: 'AWAITING_SIGNAL...', artist: 'SYSTEM', url: '' })
  
  let hubConnection = null

  const initRadio = async (audioElement) => {
    if (hubConnection) return 

    // 1. 修正 URL 拼接 (确保不出现双斜杠)
    let baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44359'
    const cleanBaseUrl = baseUrl.replace(/\/api\/?$/, '').replace(/\/+$/, '')
    const hubUrl = `${cleanBaseUrl}/hub/music`
    
    console.log(">> [AUDIO_SYS] 正在握手电台枢纽:", hubUrl)

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    // music.js 中的 hubConnection.on("SyncRadioState", ...) 修正部分
    hubConnection.on("SyncRadioState", (snapshot) => {
      if (!snapshot || !snapshot.track) return
      
      const el = document.querySelector('audio');
      if (!el) return;

      const isUrlChanged = currentTrack.value.url !== snapshot.track.url;
      playlist.value = snapshot.playlist || []
      currentTrack.value = snapshot.track
      
      const offsetSeconds = (snapshot.serverTime - snapshot.startTime) / 1000

      if (isGlobalMode.value) {
        if (isUrlChanged || el.src !== snapshot.track.url) {
          console.log(">> [AUDIO_SYS] 检测到新航线，执行物理重载...");
          el.src = snapshot.track.url;
          el.load(); 
        }

        const syncExecution = () => {
          const targetTime = Math.max(0, offsetSeconds);
          if (Math.abs(el.currentTime - targetTime) > 2) {
            el.currentTime = targetTime;
          }
          
          // ⚡ 核心修复：坚决执行播放指令
          // 只要 Store 的状态是 true 或者后端快照显示正在播放，就强制 play()
          if (isPlaying.value || snapshot.isPlaying) {
            isPlaying.value = true; // 确保 UI 状态同步
            el.play().then(() => {
              console.log(">> [AUDIO_SYS] 自动续播成功，电台流保持中");
            }).catch(e => {
              console.warn(">> [AUDIO_SYS] 续播被拦截，可能需要用户点击一次页面", e);
            });
          }
        }

        // ⚡ 使用 readyState 3 (HAVE_FUTURE_DATA) 或 4 (HAVE_ENOUGH_DATA) 保证稳定
        if (el.readyState >= 3) {
          syncExecution();
        } else {
          // ⚡ 改用 oncanplay，确保缓冲足够播放
          el.oncanplay = () => {
            syncExecution();
            el.oncanplay = null;
          }
        }
      }
    })














    

    hubConnection.on("PlaylistUpdated", (newPlaylist) => {
      playlist.value = newPlaylist
    })

    try {
      await hubConnection.start()
      console.log(">> [AUDIO_SYS] WebSocket 长连接已建立。")
      // ⚡ 连接成功后，主动向服务器索要一次进度快照
      if (isGlobalMode.value) {
        await hubConnection.invoke("JoinGlobalRadio")
      }
    } catch (err) {
      console.error(">> [AUDIO_SYS] 连接失败:", err)
    }
  }

  const togglePlay = () => {
    const el = document.querySelector('audio');
    if (!el) return;

    // ⚡ 防御检查：如果当前 URL 是占位符，说明还没收到电台信号
    if (!currentTrack.value.url || currentTrack.value.url.includes('AWAITING')) {
        alert(">> [SYS_ERR] 尚未接收到电台发射信号，请稍后...");
        return;
    }

    if (isPlaying.value) {
      el.pause();
      isPlaying.value = false;
    } else {
      // 在全局模式下，点击 PLAY 应该是向服务器同步最新进度，而不是本地盲目播放
      if (isGlobalMode.value && hubConnection?.state === 'Connected') {
        isPlaying.value = true; // 预设为 true，等待 Sync 信号回来触发 play()
        hubConnection.invoke("JoinGlobalRadio");
      } else {
        el.play().then(() => isPlaying.value = true).catch(e => console.error(e));
      }
    }
  }

  return { isPlaying, isGlobalMode, volume, currentTrack, playlist, initRadio, togglePlay }
})