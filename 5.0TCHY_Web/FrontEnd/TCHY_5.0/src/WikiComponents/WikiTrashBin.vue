<template>
  <div class="modal-overlay" v-if="visible" @click.self="$emit('close')">
    <div class="modal-content">
      <header class="modal-header">
        <div class="header-left">
          <h3>🗑️ 百科回收站</h3>
          <span class="count-badge" v-if="trashList.length">{{ trashList.length }}</span>
        </div>
        <button class="close-btn" @click="$emit('close')">×</button>
      </header>

      <div class="modal-body">
        <div v-if="loading" class="loading-box">
          <div class="spinner"></div>
          <p>正在检索时空碎片...</p>
        </div>

        <div v-else-if="trashList.length === 0" class="empty-box">
          <div class="empty-icon">✨</div>
          <p>回收站空空如也，环境保持得很好！</p>
        </div>

        <div v-else class="trash-list-container">
          <table class="trash-table">
            <thead>
              <tr>
                <th>词条名称</th>
                <th>原所属分类</th>
                <th>删除时间</th>
                <th class="actions-head">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in trashList" :key="item.id">
                <td class="col-title"><strong>{{ item.title }}</strong></td>
                <td><span class="cat-tag">{{ item.categoryName }}</span></td>
                <td class="col-time">{{ item.deletedAt }}</td>
                <td class="col-actions">
                  <button class="btn-restore" @click="handleRestore(item)" title="恢复到原目录">
                    ↺ 恢复
                  </button>
                  <button v-if="isSuperAdmin" class="btn-hard-delete" @click="handleHardDelete(item)" title="物理粉碎，不可找回">
                    🧨 物理粉碎
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <footer class="modal-footer" v-if="trashList.length > 0">
        <div class="footer-tip">💡 软删除的词条在此保留，物理粉碎后将永久消失。</div>
        <button v-if="isSuperAdmin" class="btn-clear-all" @click="handleClearAll">
          💥 彻底清空回收站
        </button>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

const props = defineProps({
  visible: Boolean,
  isSuperAdmin: Boolean
})
const emit = defineEmits(['close', 'refresh-tree'])
const authStore = useAuthStore()

const trashList = ref([])
const loading = ref(false)

// 监听弹窗打开，自动刷新列表
watch(() => props.visible, (newVal) => {
  if (newVal) fetchTrashList()
})

const fetchTrashList = async () => {
  loading.value = true
  try {
    const res = await apiClient.get(`/Wiki/trash-bin?userId=${authStore.userID}`)
    const data = res.data || res

    // 🚀 核心修复：将后端传来的大写字段映射为前端需要的小写字段
    trashList.value = data.map(item => ({
      id: item.Id ?? item.id,
      title: item.Title ?? item.title,
      categoryName: item.CategoryName ?? item.categoryName,
      deletedAt: item.DeletedAt ?? item.deletedAt
    }))
    
  } catch (error) {
    console.error("加载回收站失败:", error)
  } finally {
    loading.value = false
  }
}

// 恢复词条
const handleRestore = async (item) => {
  if (!confirm(`确定要恢复《${item.title}》吗？`)) return
  try {
    await apiClient.post('/Wiki/restore-article', {
      UserId: authStore.userID,
      ArticleId: item.id
    })
    alert("词条已复活！")
    fetchTrashList()
    emit('refresh-tree') // 通知父组件刷新左侧目录树
  } catch (error) {
    alert("恢复失败：" + (error.response?.data || "未知错误"))
  }
}

// 单个硬删除
const handleHardDelete = async (item) => {
  if (!confirm(`警告：确定要物理粉碎《${item.title}》吗？此操作无法撤销！`)) return
  try {
    await apiClient.post('/Wiki/delete-article', {
      UserId: authStore.userID,
      ArticleId: item.id
    })
    fetchTrashList()
  } catch (error) {
    alert("操作失败")
  }
}

// 清空所有
const handleClearAll = async () => {
  if (!confirm("⚠️ 终极警告：你正在彻底清空整个回收站！\n所有被删词条及其历史版本都将永久消失。确定执行物理粉碎吗？")) return
  try {
    await apiClient.post('/Wiki/clear-trash', authStore.userID)
    alert("回收站已彻底归零。")
    fetchTrashList()
  } catch (error) {
    alert("清空失败")
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.6); backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center; z-index: 10000;
}
.modal-content {
  background: white; width: 90%; max-width: 800px; border-radius: 16px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25); overflow: hidden;
  display: flex; flex-direction: column; max-height: 80vh;
}

.modal-header {
  padding: 20px 24px; border-bottom: 1px solid #f1f5f9;
  display: flex; justify-content: space-between; align-items: center;
}
.header-left { display: flex; align-items: center; gap: 12px; }
.header-left h3 { margin: 0; font-size: 18px; color: #0f172a; }
.count-badge { background: #64748b; color: white; padding: 2px 8px; border-radius: 10px; font-size: 12px; font-weight: bold; }
.close-btn { background: none; border: none; font-size: 24px; cursor: pointer; color: #94a3b8; }

.modal-body { padding: 0; overflow-y: auto; flex: 1; min-height: 300px; }

/* 表格样式 */
.trash-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.trash-table th { background: #f8fafc; text-align: left; padding: 12px 24px; color: #64748b; font-weight: 600; }
.trash-table td { padding: 16px 24px; border-bottom: 1px solid #f1f5f9; color: #475569; }
.col-title { color: #0f172a; }
.cat-tag { background: #f1f5f9; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
.col-time { font-family: monospace; font-size: 12px; color: #94a3b8; }

.col-actions { display: flex; gap: 8px; justify-content: flex-end; }
.btn-restore { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; padding: 4px 10px; border-radius: 6px; cursor: pointer; }
.btn-restore:hover { background: #dcfce7; }
.btn-hard-delete { background: white; color: #ef4444; border: 1px solid #fecaca; padding: 4px 10px; border-radius: 6px; cursor: pointer; }
.btn-hard-delete:hover { background: #fef2f2; border-color: #ef4444; }

.modal-footer { padding: 16px 24px; background: #f8fafc; border-top: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; }
.footer-tip { font-size: 12px; color: #94a3b8; }
.btn-clear-all { background: #ef4444; color: white; border: none; padding: 8px 16px; border-radius: 8px; font-weight: bold; cursor: pointer; transition: 0.2s; }
.btn-clear-all:hover { background: #dc2626; transform: scale(1.02); }

/* 状态样式 */
.loading-box, .empty-box { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 0; color: #94a3b8; }
.empty-icon { font-size: 40px; margin-bottom: 12px; }
.spinner { width: 30px; height: 30px; border: 3px solid #f1f5f9; border-top-color: #3b82f6; border-radius: 50%; animation: spin 0.8s linear infinite; margin-bottom: 12px; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>