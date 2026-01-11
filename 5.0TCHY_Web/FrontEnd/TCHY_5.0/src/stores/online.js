import { defineStore } from 'pinia'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { ref } from 'vue'

export const useOnlineStore = defineStore('online', () => {
  const onlineCount = ref(0)
  const onlineUsersList = ref([])
  const connection = ref(null)

  const startSignalR = async (baseUrl) => {
    if (connection.value) return // 防止重复连接

    const token = localStorage.getItem('auth_token') // 根据你的 token 键名修改
    connection.value = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/online`, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build()

    // 接收全站在线统计
    connection.value.on("ReceiveOnlineStats", (data) => {
      onlineCount.value = data.count || 0
      // 统一处理用户对象字段名
      onlineUsersList.value = (data.users || []).map(u => ({
        userId: u.userId || u.UserId,
        userName: u.userName || u.UserName,
        avatar: u.avatar || u.Avatar
      }))
    })

    try {
      await connection.value.start()
      console.log("[SignalR] ✅ 全站在线统计连接已激活")
    } catch (err) {
      console.error("[SignalR] ❌ 连接失败:", err)
    }
  }

  return { onlineCount, onlineUsersList, startSignalR }
})