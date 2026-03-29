import { defineStore } from 'pinia'
import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr'
import { ref } from 'vue'

export const useOnlineStore = defineStore('online', () => {
  // --- 状态定义 ---
  const onlineCount = ref(0)
  const onlineUsersList = ref([])
  
  // 🔗 链路 A: 全局在线监测连接 (/hubs/online)
  const connection = ref(null)
  
  // 🔗 链路 B: 聊天室专用连接 (/chatHub)
  const chatConnection = ref(null)

  /**
   * 初始化：全站在线统计链路
   * 指向：/hubs/online
   */
  const startSignalR = async (baseUrl) => {
    if (connection.value) return // 防止重复初始化

    const token = localStorage.getItem('auth_token')
    
    connection.value = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/online`, {
        accessTokenFactory: () => token || '',
        // 强制使用 WebSocket 提高稳定性
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true 
      })
      .withAutomaticReconnect()
      .build()

    // 监听全站在线统计广播
    connection.value.on("ReceiveOnlineStats", (data) => {
      onlineCount.value = data.count || 0
      onlineUsersList.value = (data.users || []).map(u => ({
        userId: u.userId || u.UserId,
        userName: u.userName || u.UserName,
        avatar: u.avatar || u.Avatar
      }))
    })

    try {
      await connection.value.start()
      console.log("[SignalR] ✅ 全站在线统计链路 (A) 已激活")
    } catch (err) {
      console.error("[SignalR] ❌ 全站统计链路连接失败:", err)
      connection.value = null
    }
  }

  /**
   * 初始化：专用聊天通讯链路
   * 指向：/chatHub (对应后端的 ChatHub.cs)
   */
  const startChatSignalR = async (baseUrl) => {
    // 如果已经存在且处于连接状态，则不再初始化
    if (chatConnection.value) return 

    const token = localStorage.getItem('auth_token')
    
    chatConnection.value = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/chatHub`, {
        accessTokenFactory: () => token || '',
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true
      })
      .withAutomaticReconnect()
      .build()

    try {
      await chatConnection.value.start()
      console.log("[SignalR] ✅ 聊天通讯业务链路 (B) 已激活")
    } catch (err) {
      console.error("[SignalR] ❌ 聊天链路连接失败:", err)
      chatConnection.value = null
    }
  }

  // --- 暴露给外部使用的接口 ---
  return { 
    onlineCount, 
    onlineUsersList, 
    connection,      // 暴露链路 A 实例
    chatConnection,  // 暴露链路 B 实例
    startSignalR,    // 启动链路 A
    startChatSignalR // 启动链路 B
  }
})