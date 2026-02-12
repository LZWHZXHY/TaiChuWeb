<template>
  <div class="user-card">
    <UserInfo :user-id="userId" />

    <div class="uc-center">
      <div class="avatar-box">
        <UserAvatar :user-id="userId" />
      </div>
    </div>
  </div>
</template>

<script setup>
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';
import UserInfo from './UserInfo.vue' 

const props = defineProps({
  userId: { type: [String, Number], default: null }
})
</script>

<style scoped>
.user-card {
  width: 100%; height: 100%; max-height: 178px;
  display: flex; background: transparent;
}
.uc-center {
  flex: 1; display: flex; align-items: center; justify-content: center;
}
.avatar-box {
  /* 在 UserCard 里，我们希望头像稍微大一点，看起来有视觉重心 */
  width: 120px;
  height: 120px;
}
</style>