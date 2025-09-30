<template>
  <div class="big-container">
    <!-- 顶部导航栏 -->
    <header class="header">
      <div class="logo">     太初寰宇 Beta</div>
      <div class="user-info">
        <span class="username"><RouterLink to = '/user'>{{ userName }}</RouterLink></span>
        <span class="RegisterNumber"></span>
      </div>
    </header>

    <!-- 欢迎区域 -->
    <section class="welcome-section">
      <h1>欢迎来到太初寰宇社区</h1>
      <div class="beta-notice">
        <p>当前是太初寰宇社区网站beta删档内测版</p>
        <p>您所注册的账户将在beta测试后删除，等二测或者正式版发布时会需要重新注册</p>
      </div>
    </section>

    <!-- 主要内容和路由视图布局 -->
    <div class="main-layout">
      <!-- 左侧内容区域 -->
      <div class="sidebar">
        <!-- 新闻板块 -->
        <div class="content-block news-block">
          <h2 class="block-title">最新消息！</h2>
          <div class="block-content">
            <RouterLink to = "/news">查看最新消息！</RouterLink>
          </div>
        </div>
        
        <!-- 活动板块 -->
        <div class="content-block activity-block">
          <h2 class="block-title">最新活动</h2>
          <div class="block-content"></div>
        </div>
        
        <!-- 项目面板 -->
        <div class="content-block project-block">
          <h2 class="block-title">彼岸宇宙项目</h2>
          <div class="project-panel">
            <div class="project-item">
              <router-link to="/BYDseries" class="project-link">彼岸宇宙企划</router-link>
            </div>
            <div class="project-item">
              <router-link to="/BYDanimation" class="project-link">彼岸宇宙动画</router-link>
            </div>
            <div class="project-item">
              <router-link to="/BYDgame" class="project-link">彼岸宇宙游戏</router-link>
            </div>
            <div class="project-item">
              <router-link to="/BYDstory#" class="project-link">彼岸宇宙故事</router-link>
            </div>
          </div>
        </div>
      </div>

       <!-- 中央路由视图 - 添加一个高度过渡容器 -->
      <div class="router-wrapper">
        <Transition name="fade-height">
        <main class="router-container" v-if="shouldShowRouter">
            <router-view v-slot="{ Component }">
            <transition name="fade-slide-up" mode="out-in">
                <component :is="Component" />
            </transition>
            </router-view>
        </main>
        </Transition>
      </div>








      <!-- 右侧内容区域 -->
      <div class="sidebar">
        <!-- 指南板块 -->
        <div class="content-block intro-block">
          <h2 class="block-title">教你如何玩转太初寰宇社区！</h2>
          <div class="block-content">
            <ul>
              <li><RouterLink to = "/intro">太初寰宇社区介绍</RouterLink></li>
              <li><RouterLink to = "/links">【跳转导航】</RouterLink></li>
              <li><RouterLink to = "/updates">更新记录</RouterLink></li>
              <li><RouterLink to = "/rules">社区规则</RouterLink></li>
            </ul>
            
            
          </div>
        </div>
        
        <!-- 关于我们 -->
        <div class="content-block about-block">
          <h2 class="block-title">关于我们</h2>
          <div class="about-section">
            <h3 class="section-subtitle">关注我们！</h3>
            <div class="section-content">
              <a href="https://space.bilibili.com/3546736970696727?spm_id_from=333.337.0.0" target="_blank">B站：太初寰宇</a> <!-- 只需这样绑定即可 -->
              <span id = "follower">粉丝数：</span>
              <span class="follower-count">{{ followerCount }}</span>
            </div>
          </div>
          <div class="about-section">
            <h3 class="section-subtitle">成员</h3>
            <div class="section-content"></div>
          </div>
        </div>
        
        <!-- 财政板块 -->
        <div class="content-block finance-block">
          <h2 class="block-title">财政</h2>
          <div class="block-content">
            <RouterLink to = "/financial">流水记录</RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
    @import url('/static/css/taichu.css');
</style>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useUserStore } from '../../store/user'
import axios from 'axios'

const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321'   // 你的 .NET 本地后端地址
  : 'https://bianyuzhou.com'    // 线上后端地址（请根据实际填）

const route = useRoute()
const shouldShowRouter = ref(true)

const userStore = useUserStore()
const userName = computed(() => userStore.user.username)

// 粉丝数响应式变量
const followerCount = ref('加载中...')




// 获取粉丝数的函数
async function fetchBiliFans() {
  try {
    // 本地开发时走本地后端，生产环境走线上后端
    const res = await axios.get(`${baseApiUrl}/api/BilibiliProxy/fans`)
    followerCount.value = res.data?.data?.follower ?? '未知'
  } catch (e) {
    followerCount.value = '获取失败'
  }
}

// 组件挂载时请求粉丝数
onMounted(() => {
  fetchBiliFans()
})

// 路由过渡
watch(
  () => route.path,
  () => {
    shouldShowRouter.value = false
    setTimeout(() => {
      shouldShowRouter.value = true
    }, 80)
  }
)
</script>

<script>
export default {



  data() {
    return {
      hasRouterContent: false,
      currentUser: 'LZWHZXHY',
      currentTime: '2025-08-16 04:56:14'
    }
  },
  methods: {
    updateContentState(hasContent) {
      this.hasRouterContent = hasContent;
    }
    
  },
  mounted() {

      

    // 初始检查路由内容
    this.$nextTick(() => {
      const routerView = document.querySelector('.router-container > *');
      this.hasRouterContent = routerView && routerView.innerHTML.trim() !== '';
    });
  },
  watch: {
    // 监听路由变化
    '$route'() {
      this.$nextTick(() => {
        const routerView = document.querySelector('.router-container > *');
        this.hasRouterContent = routerView && routerView.innerHTML.trim() !== '';
      });
    }
  }
}
</script>