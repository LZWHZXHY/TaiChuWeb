<template>
  <div class="id-card-wrapper">
    <div class="user-card-container">
      <UserCard />
    </div>

    <div class="data-card-container">
      <DataCard />
    </div>
  </div>
</template>

<script setup>
import UserCard from './UserCard.vue'
import DataCard from './DataCard.vue' // 引入新组件
</script>

<style scoped>
.id-card-wrapper {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: row;
  background-color: #F4F1EA; /* 保持背景色 */
  border-radius: 8px;
  overflow: hidden;
}

/* 左边容器样式 */
.user-card-container {
  flex: 0 0 60%; /* 固定宽度 60% */
  width: 60%;
  height: 100%;
}

/* 右边容器样式 
   (注意：这里将原先的 .right-section 改名为 .data-card-container 以匹配模板)
*/
.data-card-container {
  flex: 0 0 40%; /* 固定宽度 40% */
  width: 40%;
  height: 100%;
}
</style>