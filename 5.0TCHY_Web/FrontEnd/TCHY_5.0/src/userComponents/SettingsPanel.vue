<template>
  <div>
    <h3>账户设置</h3>
    <div v-if="isMe">这里展示个人信息管理功能等...</div>
    <div v-else>仅本人可配置账户设置</div>
    <div>用户ID：{{ user.id }}</div>
  </div>
</template>

<script setup>
defineProps({
  user: Object,
  isMe: Boolean
})
</script>