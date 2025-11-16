<template>
  <div class="admin-root">
    <AdminLayout
      :active="activeTab"
      @change="activeTab = $event"
      @refresh="handleGlobalRefresh"
    >
      <!-- 根据当前 tab 动态渲染 -->
      <component
        :is="currentComponent"
        class="admin-panel"
      />
    </AdminLayout>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

import AdminLayout from '@/adminComponents/AdminLayout.vue'
import ReviewSocieties from '@/adminComponents/ReviewSocieties.vue'
import UsersManagement from '@/adminComponents/UserManagement.vue'
import ReportsPanel from '@/adminComponents/ReportsPanel.vue'
import SettingsPanel from '@/adminComponents/SettingsPanel.vue'

const activeTab = ref('review') // review | users | reports | settings

const map = {
  review: ReviewSocieties,
  users: UsersManagement,
  reports: ReportsPanel,
  settings: SettingsPanel
}

const currentComponent = computed(() => map[activeTab.value] || ReviewSocieties)

// 全局刷新按钮（顶栏触发）—后续可广播事件或调用各组件的公开方法
function handleGlobalRefresh() {
  // TODO: 后续统一触发各面板的刷新逻辑（可用事件总线/Pinia）
}
</script>

<style scoped>
.admin-root {
  min-height: 100vh;
  background: #f6f8fb;
  display: flex;
  flex-direction: column;
}
.admin-panel {
  width: 100%;
}
</style>