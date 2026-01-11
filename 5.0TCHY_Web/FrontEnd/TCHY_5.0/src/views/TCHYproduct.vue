<template>
  <div>
    <!-- 导航栏 -->
    <nav class="navbar">
      <a 
        v-for="(item, index) in navItems" 
        :key="index" 
        :href="item.href" 
        :class="{ active: activeNav === item.name }" 
        @click.prevent="setActive(item.name)"
      >
        {{ item.name }}
      </a>
    </nav>

    <!-- 动态切换内容 -->
    <component :is="currentComponent" />
  </div>
</template>

<script>
import apiClient from '@/utils/api';

import NovelsComponent from '@/TCHYComponents/NovelsComponent.vue';
import SettingsComponent from '@/TCHYComponents/SettingsComponent.vue';
import CreaterComponent from '@/TCHYComponents/CreaterComponent.vue';


export default {
  name: "TCHYproduct",
  components: {
    NovelsComponent,
    SettingsComponent,
    CreaterComponent
  },
  data() {
    return {
      navItems: [
        { name: "小说", href: "#novel" },
        { name: "设定库", href: "#settings" },
        { name: "制作参与", href: "#creater" }
      ],
      activeNav: "小说",
    };
  },
  computed: {
    currentComponent() {
      switch (this.activeNav) {
        case "小说":
          return "NovelsComponent";
        case "设定库":
          return "SettingsComponent";
        case "制作参与":
          return "CreaterComponent";
        default:
          return null;
      }
    },
  },
  methods: {
    // 👇 必须变成 async 方法
    async setActive(navName) {
      // 身份校验只针对制作参与
      if (navName === "制作参与") {
        let userInfo = null;
        try {
          userInfo = JSON.parse(localStorage.getItem('user'));
        } catch (err) {
          userInfo = null;
        }

        // 前端本地校验
        if (!userInfo || userInfo.creater !== 1) {
          alert("⚠️ 仅创作者身份用户可进入制作参与页面！");
          return;
        }

        // 后端接口实时校验
        try {
          const resp = await apiClient.get('/userinfo/is-creater');
          if (!resp.data.isCreater) {
            alert("⚠️ 后端校验：您当前不是创作者，无法进入制作参与页面！");
            return;
          }
        } catch (error) {
          alert("⚠️ 获取身份信息失败，请稍后重试！");
          return;
        }
      }
      this.activeNav = navName;
    },
  },
};
</script>

<style scoped>
.navbar {
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #333;
  padding: 10px 15px;
}

.navbar a {
  color: white;
  text-decoration: none;
  padding: 10px 20px;
  margin: 0 10px;
  border-radius: 4px;
  font-size: 16px;
  transition: background-color 0.3s ease;
}

.navbar a:hover {
  background-color: #555;
}

.navbar a.active {
  background-color: #007BFF;
}
</style>