<template>
  <aside class="sidebar-nav">
    <a href="#" class="logo"><i class="fas fa-pen-nib"></i> 博客<span>平台</span></a>
    
    <nav class="nav-menu">
      <ul>
        <li v-for="item in menuItems" :key="item.text">
          <a :href="item.link" :class="{ active: item.active }">
            <i :class="item.icon"></i> {{ item.text }}
          </a>
        </li>
      </ul>
    </nav>
    
    <div class="user-profile">
      <div class="user-info" @click="toggleDropdown">
        <img src="https://randomuser.me/api/portraits/women/44.jpg" alt="用户头像" class="user-avatar">
        <span class="user-name">用户名</span>
      </div>
      
      <div class="user-dropdown" v-show="showDropdown">
        <a href="#"><i class="fas fa-user"></i> 个人中心</a>
        <a href="#"><i class="fas fa-pen"></i> 写博客</a>
        <a href="#"><i class="fas fa-cog"></i> 设置</a>
      </div>
    </div>
  </aside>
</template>

<script>
export default {
  name: 'RightSidebar',
  data() {
    return {
      showDropdown: false,
      menuItems: [
        { text: '首页', icon: 'fas fa-home', link: '#', active: true },
        { text: '热门', icon: 'fas fa-fire', link: '#' },
        { text: '分类', icon: 'fas fa-bookmark', link: '#' },
        { text: '作者', icon: 'fas fa-users', link: '#' }
      ]
    }
  },
  methods: {
    toggleDropdown() {
      this.showDropdown = !this.showDropdown
    }
  }
}
</script>

<style scoped>
.sidebar-nav {
  width: 280px;
  background: white;
  padding: 25px 0;
  box-shadow: -2px 0 10px rgba(0, 0, 0, 0.05);
}

.logo {
  font-size: 1.8rem;
  font-weight: 700;
  color: var(--primary-color);
  text-decoration: none;
  padding: 0 25px 25px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.nav-menu ul {
  list-style: none;
}

.nav-menu ul li a {
  display: flex;
  align-items: center;
  text-decoration: none;
  color: var(--dark-color);
  padding: 12px 25px;
}

.user-profile {
  padding: 20px;
  margin-top: auto;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}

.user-dropdown {
  background: white;
  border-radius: var(--border-radius);
  padding: 15px;
  width: 200px;
}

.user-dropdown a {
  display: flex;
  align-items: center;
  padding: 10px 15px;
  text-decoration: none;
}
</style>