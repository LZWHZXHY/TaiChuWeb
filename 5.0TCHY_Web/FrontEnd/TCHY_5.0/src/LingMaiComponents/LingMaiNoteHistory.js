//LingMaiNoteHistory.js
import { ref } from 'vue'
import apiClient from '@/utils/api'

/**
 * 封装笔记历史记录相关的逻辑
 * @param {Object} noteRef - 响应式的笔记对象 (包含 id, title 等)
 * @param {Object} editorRef - Tiptap 编辑器实例的 ref
 * @param {Function} debouncedSaveFn - 触发编辑器保存的方法
 */
export function useNoteHistory(noteRef, editorRef, debouncedSaveFn) {
  // 状态变量
  const showHistoryPanel = ref(false)
  const historyList = ref([])
  const isLoadingHistory = ref(false)

  // 展开/收起历史面板
  const toggleHistoryPanel = async () => {
    showHistoryPanel.value = !showHistoryPanel.value
    // 每次打开面板时，自动拉取最新的历史列表
    if (showHistoryPanel.value) {
      await loadHistoryList()
    }
  }

  // 获取历史列表
  const loadHistoryList = async () => {
    if (!noteRef.id) return
    isLoadingHistory.value = true
    try {
      const res = await apiClient.get(`/NoteHistory/list/${noteRef.id}`)
      historyList.value = res.data
    } catch (e) {
      console.error("加载历史记录失败", e)
    } finally {
      isLoadingHistory.value = false
    }
  }

  // 恢复到指定的历史版本
  const applyHistory = async (historyId) => {
    if (!confirm("⚠️ 确定要用该历史版本覆盖当前内容吗？\n当前未保存的内容将被丢失，此操作不可逆！")) {
      return
    }

    try {
      const res = await apiClient.get(`/NoteHistory/detail/${historyId}`)
      const historicalData = res.data
      
      if (editorRef.value) {
        // 1. 替换编辑器的内容
        const jsonContent = JSON.parse(historicalData.contentJson)
        editorRef.value.commands.setContent(jsonContent)
        
        // 2. 恢复当时的标题
        noteRef.title = historicalData.title

        // 3. 强制触发一次保存，将历史数据覆盖到真实的 Note 表中
        if (debouncedSaveFn) {
          debouncedSaveFn(editorRef.value.getJSON())
        }

        // 4. 关闭面板并提示
        showHistoryPanel.value = false
        alert("✅ 已成功恢复至该历史版本！")
      }
    } catch (e) {
      console.error("恢复历史版本失败", e)
      alert("恢复失败，请检查网络或稍后重试。")
    }
  }

  // 格式化时间辅助函数 (展示给用户看)
  const formatHistoryTime = (dateStr) => {
    if (!dateStr) return ''
    const d = new Date(dateStr)
    // 格式如：04-14 14:30:00
    return `${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}:${d.getSeconds().toString().padStart(2, '0')}`
  }

  return {
    showHistoryPanel,
    historyList,
    isLoadingHistory,
    toggleHistoryPanel,
    loadHistoryList,
    applyHistory,
    formatHistoryTime
  }
}