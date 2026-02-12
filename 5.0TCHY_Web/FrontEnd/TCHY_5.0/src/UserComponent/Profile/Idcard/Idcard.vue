<template>
  <div class="id-card-wrapper">
    <div class="user-card-container">
      <UserCard :user-id="userId" />
    </div>

    <div class="data-card-container">
      <DataCard :user-id="userId" />
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'
import UserCard from './UserCard.vue'
import DataCard from './DataCard.vue' 

const props = defineProps({
  // 接收来自 NewProfile 的 userId
  userId: {
    type: [String, Number],
    default: null
  }
})
</script>

<style scoped>
/* 样式保持不变 */
.id-card-wrapper { width: 100%; height: 100%; display: flex; flex-direction: row; background-color: #F4F1EA; border-radius: 8px; overflow: hidden; }
.user-card-container { flex: 0 0 60%; width: 60%; height: 100%; }
.data-card-container { flex: 0 0 40%; width: 40%; height: 100%; }
</style>