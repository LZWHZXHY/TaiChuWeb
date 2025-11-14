<!-- src/components/TopNav.vue -->
<template>
  <div class="top-nav">
    <div class="search-box">
      <i class="fas fa-search"></i>
      <input
        type="text"
        placeholder="搜索内容、用户或板块..."
        v-model="searchQuery"
        @input="handleSearchInput"
      />
    </div>
    
    <div class="user-actions">
      <div class="notification">
        <i class="fas fa-bell"></i>
        <span class="notification-badge">{{ notificationCount }}</span>
      </div>
      <div class="user-profile" @click="toggleAuthModal">
        <div class="user-avatar">访</div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'TopNav',
  data() {
    return {
      searchQuery: '',
      notificationCount: 4, // 示例数据
    };
  },
  methods: {
    handleSearchInput() {
      // 处理搜索输入逻辑
      console.log('搜索内容:', this.searchQuery);
      // 这里可以添加实际的搜索逻辑
    },
    toggleAuthModal() {
      // 通过自定义事件通知父组件打开 AuthModal
      this.$emit('toggle-auth-modal');
    },
  },
};
</script>

<style scoped>
/* 你的样式 */
.top-nav {
  background: var(--dark);
  padding: 15px 30px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 99;
}

.search-box {
  display: flex;
  align-items: center;
  background: var(--darker);
  border-radius: 20px;
  padding: 8px 15px;
  width: 300px;
}

.search-box input {
  background: none;
  border: none;
  color: var(--light);
  width: 100%;
  margin-left: 10px;
  outline: none;
}

.user-actions {
  display: flex;
  align-items: center;
  gap: 20px;
}

.notification, .user-profile {
  position: relative;
  cursor: pointer;
}

.notification-badge {
  position: absolute;
  top: -5px;
  right: -5px;
  background: var(--danger);
  color: white;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  font-size: 0.7rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  margin-left: 10px;
}
</style>