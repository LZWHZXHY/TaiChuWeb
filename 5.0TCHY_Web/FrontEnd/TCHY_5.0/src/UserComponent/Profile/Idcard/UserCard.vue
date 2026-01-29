<template>
  <div class="user-card">
    <UserInfo 
      :name="name"
      :gender="gender"
      :bio="bio"
      :tags="tags"
      :location="location"
      :contact="contact"
    />

    <div class="uc-center">
      <UserAvatar :avatar="avatar" :level="level" />
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'
import UserAvatar from './UserAvatar.vue'
import UserInfo from './UserInfo.vue' // 引入新拆分的组件

const props = defineProps({
  name: { type: String, default: 'K_Runner' },
  gender: { type: String, default: 'Male' }, 
  bio: { type: String, default: '义体维修专家，专注于老式神经网络接口的调试。寻找电子幽灵的踪迹。' },
  tags: { type: Array, default: () => ['赛博义体', '全栈开发', '黑客'] },
  location: { type: String, default: '夜之城·第七区' },
  contact: { type: String, default: 'k_dev@net.connect' },
  avatar: { type: String, default: 'https://image2url.com/r2/default/images/1769279786393-674d59cf-5620-4bde-9d31-c66661aa72f2.jpg' },
  level: { type: Number, default: 42 }
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

.user-card {
  width: 100%;
  height: 100%;
  max-height: 178px;
  display: flex;
  background: transparent;
  font-family: 'Noto Sans SC', sans-serif;
  color: #2c3e50;
  overflow: hidden;
  position: relative;
}

/* === 右侧 uc-center (Flex 1) === */
.uc-center {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  border-left: 0px solid rgba(0,0,0,0.05);
  border-right: 0px solid rgba(0,0,0,0.05);
}
</style>