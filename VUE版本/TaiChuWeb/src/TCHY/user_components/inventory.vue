<template>
  <div class="delta-inventory-root">
    <h2 class="delta-inventory-title">仓库箱</h2>
    <div v-if="loading" class="delta-inventory-loading">加载中...</div>
    <div v-else>
      <div v-if="inventory.length === 0" class="delta-inventory-empty">
        暂无已拥有的道具
      </div>
      <div v-else class="delta-inventory-list">
        <div
          v-for="item in inventory"
          :key="item.id"
          class="delta-inventory-item"
        >
          <img :src="item.icon" class="delta-inventory-icon" />
          <div class="delta-inventory-info">
            <div class="delta-inventory-name">{{ item.name }}</div>
            <div class="delta-inventory-desc">{{ item.desc }}</div>
            <div class="delta-inventory-count">数量: {{ item.count }}</div>
            <button
              v-if="isRenameCard(item)"
              class="use-btn"
              @click="openRenameModal(item)"
            >使用</button>
          </div>
        </div>
      </div>
    </div>
    <!-- 改名弹窗 -->
    <div v-if="showRenameModal" class="rename-modal-overlay">
      <div class="rename-modal">
        <div class="rename-modal-title">使用改名卡</div>
        <div class="rename-modal-body">
          <input
            v-model="newUsername"
            maxlength="25"
            placeholder="请输入新的用户名（1-25个字符）"
            class="rename-input"
            @keyup.enter="submitRename"
          />
          <div v-if="renameError" class="rename-error">{{ renameError }}</div>
        </div>
        <div class="rename-modal-actions">
          <button @click="submitRename" class="rename-confirm">确认改名</button>
          <button @click="closeRenameModal" class="rename-cancel">取消</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '../../../server/api'

const inventory = ref([])
const loading = ref(true)
const showRenameModal = ref(false)
const renameCardItem = ref(null)
const newUsername = ref('')
const renameError = ref('')

async function fetchInventory() {
  loading.value = true
  try {
    const res = await api.get('/api/Trade/inventory')
    inventory.value = res || []
  } catch (e) {
    inventory.value = []
  }
  loading.value = false
}

onMounted(fetchInventory)

function isRenameCard(item) {
  // 根据你的实际数据结构调整判断条件
  return item.type === 'rename_card' || item.name.includes('改名卡')
}

function openRenameModal(item) {
  showRenameModal.value = true
  renameCardItem.value = item
  newUsername.value = ''
  renameError.value = ''
}

function closeRenameModal() {
  showRenameModal.value = false
  renameCardItem.value = null
  newUsername.value = ''
  renameError.value = ''
}

async function submitRename() {
  if (!newUsername.value.trim()) {
    renameError.value = '用户名不能为空'
    return
  }
  if (newUsername.value.length < 1 || newUsername.value.length > 25) {
    renameError.value = '用户名长度需为1-25个字符'
    return
  }
  try {
    const res = await api.post('/api/Inventory/use-rename-card', {
      newUsername: newUsername.value,
      itemId: renameCardItem.value.itemId   // 关键在这里
    })
    if (res.success) {
      alert('改名成功，请重新登录！')
      closeRenameModal()
      fetchInventory()
    } else {
      renameError.value = res.message || '改名失败'
    }
  } catch (e) {
    renameError.value = e?.response?.data?.message || '服务器错误'
  }
}
</script>

<style scoped>
.delta-inventory-root {
  max-width: 95%;
  margin: 32px auto;
  padding: 0 12px 20px 12px;
  background: linear-gradient(120deg, #181c23 90%, #232e47 100%);
  border-radius: 16px;
  box-shadow: 0 4px 24px #14233a55;
}

.delta-inventory-title {
  font-size: 1.35rem;
  color: #c6e3ff;
  margin-bottom: 16px;
  letter-spacing: 2px;
  font-weight: bold;
  text-shadow: 0 3px 16px #1e47a099, 0 1px 2px #223;
  padding-top: 20px;
  padding-left: 4px;
}

.delta-inventory-loading,
.delta-inventory-empty {
  text-align: center;
  color: #85b7e8;
  margin-top: 40px;
  font-size: 1.1rem;
  letter-spacing: 1.5px;
}

.delta-inventory-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(198px, 1fr));
  gap: 14px;
  padding-bottom: 4px;
}

.delta-inventory-item {
  background: linear-gradient(100deg, rgba(36, 54, 88, 0.92) 80%, rgba(25, 37, 54, 0.92) 100%);
  border-radius: 10px;
  box-shadow: 0 1.5px 7px 0 #1c253644;
  border: 1px solid #314e7d77;
  display: flex;
  align-items: center;
  padding: 10px 12px;
  min-width: 0;
  transition: box-shadow 0.14s, transform 0.14s, border 0.10s;
  cursor: pointer;
}

.delta-inventory-item:hover {
  box-shadow: 0 6px 20px #29bcff33, 0 1.5px 8px #38e6e060;
  border: 1.5px solid #38e6e0cc;
  transform: translateY(-2.5px) scale(1.018);
}

.delta-inventory-icon {
  width: 48px;
  height: 48px;
  margin-right: 10px;
  background: #1c2536;
  border-radius: 8px;
  object-fit: contain;
  border: 1.5px solid #5db5ff33;
  box-shadow: 0 1px 4px #2f9dff1a;
}

.delta-inventory-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 2px;
}

.delta-inventory-name {
  font-weight: bold;
  font-size: 1.06rem;
  color: #e4eaff;
  margin-bottom: 0px;
  letter-spacing: 1px;
  text-shadow: 0 1px 4px #1e47a033;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.delta-inventory-desc {
  color: #86bbff;
  font-size: 0.91rem;
  margin-bottom: 0px;
  letter-spacing: 0.2px;
  text-shadow: 0 1px 4px #4a9aff22;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.delta-inventory-count {
  color: #ffe66d;
  font-weight: bold;
  font-size: 0.95rem;
  letter-spacing: 1.1px;
  text-shadow: 0 1px 8px #ffe66d13;
}

/* 改名卡按钮和弹窗样式 */
.use-btn {
  background: linear-gradient(90deg,#3dbcff,#5ad6e6 95%);
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 2px 10px;
  font-size: 0.92rem;
  margin-top: 6px;
  cursor: pointer;
  box-shadow: 0 1px 6px #38e6e033;
  transition: background 0.2s;
}
.use-btn:hover {
  background: linear-gradient(90deg,#22a7ff,#3dc5c4 95%);
}
.rename-modal-overlay {
  position: fixed; left: 0; top: 0; right: 0; bottom: 0;
  background: rgba(18,28,43,0.42);
  z-index: 9999; display: flex; align-items: center; justify-content: center;
}
.rename-modal {
  min-width: 320px;
  background: #222d44;
  border-radius: 12px;
  padding: 24px 30px 18px 30px;
  box-shadow: 0 8px 32px 0 #1e47a077;
  color: #eaf4ff;
}
.rename-modal-title {
  font-size: 1.13rem;
  font-weight: bold;
  margin-bottom: 16px;
  color: #65d5ff;
  letter-spacing: 1.2px;
}
.rename-modal-body {
  margin-bottom: 16px;
}
.rename-input {
  width: 100%;
  padding: 7px 10px;
  border-radius: 5px;
  border: 1px solid #377fd5cc;
  font-size: 1.05rem;
  margin-bottom: 4px;
  background: #1a2336;
  color: #eaf4ff;
}
.rename-error {
  color: #ff6f6f;
  font-size: 0.95rem;
  margin-top: 3px;
}
.rename-modal-actions {
  display: flex;
  gap: 18px;
  justify-content: flex-end;
}
.rename-confirm, .rename-cancel {
  border: none;
  border-radius: 6px;
  padding: 5px 16px;
  font-size: 1.01rem;
  cursor: pointer;
}
.rename-confirm {
  background: linear-gradient(90deg,#3dbcff,#5ad6e6 95%);
  color: #fff;
}
.rename-cancel {
  background: #bbb;
  color: #222;
}
.rename-confirm:hover {
  background: linear-gradient(90deg,#22a7ff,#3dc5c4 95%);
}
.rename-cancel:hover {
  background: #e1e1e1;
}
</style>