<template>
  <div class="BigContainer">
    <main class="MainContent">
      <section v-if="selectedTab === 'activity'">
        <!-- <GameActivity /> -->
        <div class="frosted-card">
          <div class="card-title"><span class="icon">🎮</span>当前最新游戏活动</div>
          <div class="card-content">这里将展示最新的游戏活动信息。</div>
        </div>
      </section>
      <section v-else-if="selectedTab === 'tournaments'">
        <GameTournaments />
      </section>
      <section v-else-if="selectedTab === 'news'">
        <!-- <GameNews /> -->
        <div class="frosted-card">
          <div class="card-title"><span class="icon">📰</span>游戏新闻</div>
          <div class="card-content">这里将展示最新的游戏新闻与开发资讯。</div>
        </div>
      </section>
      <section v-else-if="selectedTab === 'mc'">
        <!-- <GameMCServer /> -->
        <div class="frosted-card">
          <div class="card-title"><span class="icon">🌐</span>我的世界服务器</div>
          <div class="card-content">这里将展示MC服务器的详细信息和加入方式。</div>
        </div>
      </section>

      <section v-else-if="selectedTab === 'games'">
        
        <GameCollection />

      </section>

      <section v-else>
        <slot />
      </section>
    </main>
    <nav class="RightNav">
      <div class="logo-area">
        <span class="logo-icon">🕹️</span>
        <span class="logo-title">Game Center</span>
      </div>
      <div class="nav-group">
        <div
          :class="['tab', { active: selectedTab === 'activity' }]"
          @click="selectedTab = 'activity'"
        >
          <span class="icon">🎮</span>
          活动
        </div>
        <div
          :class="['tab', { active: selectedTab === 'tournaments' }]"
          @click="selectedTab = 'tournaments'"
        >
          <span class="icon">🏆</span>
          赛制
        </div>
        <div
          :class="['tab', { active: selectedTab === 'news' }]"
          @click="selectedTab = 'news'"
        >
          <span class="icon">📰</span>
          新闻
        </div>
        <div
          :class="['tab', { active: selectedTab === 'mc' }]"
          @click="selectedTab = 'mc'"
        >
          <span class="icon">🌐</span>
          我的世界服务器
        </div>

        <div
          :class="['tab', { active: selectedTab === 'games' }]"
          @click="selectedTab = 'games'"
        >
          <span class="icon">📰</span>
          游戏板块
        </div>

      </div>
      <div class="nav-more">
        <div class="tab disabled">
          <span class="icon">➕</span>
          更多功能
        </div>
      </div>
    </nav>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import GameTournaments from '../Game_components/GameTournaments.vue'
import GameCollection from '../Game_components/GameCollection.vue'

const selectedTab = ref('activity')
</script>

<style scoped>
/* 整体渐变背景&极简布局 */
.BigContainer {
  width: 100vw;
  height: 100vh;
  background: linear-gradient(120deg, rgba(40,58,90,0.99) 0%, rgba(70,120,200,0.22) 100%);
  position: absolute;
  display: flex;
  flex-direction: row;
  overflow: hidden;
}

/* 内容区: 毛玻璃卡片+卡片动效+留白 */
.MainContent {
  flex: 1;
  padding: 56px 80px 56px 56px;
  overflow-y: auto;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  gap: 48px;
}

/* Frosted glass card风格 */
.frosted-card {
  background: rgba(255,255,255,0.21);
  border-radius: 22px;
  box-shadow: 0 8px 36px 0 rgba(60,90,160,0.13), 0 2px 16px 0 rgba(80,120,200,0.12);
  padding: 38px 54px;
  min-height: 210px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  backdrop-filter: blur(22px) saturate(1.3);
  border: 1.5px solid rgba(255,255,255,0.23);
  transition: box-shadow 0.22s, border-color 0.16s;
  animation: fade-in-card 0.66s cubic-bezier(.48,1.32,.52,.97);
}
@keyframes fade-in-card {
  0% { opacity: 0; transform: translateY(50px) scale(0.97);}
  100% { opacity: 1; transform: none;}
}
.frosted-card:hover {
  box-shadow: 0 16px 48px 0 rgba(42,82,180,0.18), 0 4px 26px 0 rgba(80,120,200,0.15);
  border-color: #a2b9ef99;
}
.card-title {
  font-size: 2rem;
  font-weight: 600;
  color: #263a67;
  display: flex;
  align-items: center;
  margin-bottom: 22px;
  letter-spacing: 1.5px;
  /* text-shadow: 0 1px 0 #fff, 0 2px 8px #bddcff33; */
}
.card-title .icon {
  font-size: 2.2rem;
  margin-right: 16px;
  filter: drop-shadow(0 2px 8px #bddcff44);
}
.card-content {
  font-size: 1.12rem;
  color: #1d2742;
  opacity: 0.96;
  font-weight: 400;
  line-height: 1.7;
}

/* 右侧导航栏: 毛玻璃+浮光渐变+hover+高亮指示器 */
.RightNav {
  width: 300px;
  min-width: 220px;
  max-width: 360px;
  background: linear-gradient(160deg, rgba(255,255,255,0.14) 70%, rgba(120,160,220,0.09) 100%);
  border-left: 2.5px solid rgba(100,120,200,0.12);
  box-shadow: -8px 0 24px 2px rgba(60,100,180,0.10);
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  padding: 0;
  position: relative;
  backdrop-filter: blur(22px) saturate(1.19);
}

/* 顶部LOGO区 */
.logo-area {
  width: 100%;
  padding: 50px 0 36px 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: bold;
  color: #e3e6ef;
  letter-spacing: 3.5px;
  gap: 12px;
  user-select: none;
  text-shadow: 0 2px 16px #4862b880, 0 1px 0 #fff;
}
.logo-icon {
  font-size: 2.3rem;
  margin-right: 8px;
  filter: drop-shadow(0 2px 8px #bddcff66);
}
.logo-title {
  font-size: 1.25rem;
  background: linear-gradient(90deg, #aabfff 20%, #fff 90%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  font-weight: 700;
  letter-spacing: 3.5px;
}

/* 导航组 */
.nav-group {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 2px;
  margin-top: 10px;
}

/* 高端Tab按钮：高亮条、渐变、微动画 */
.tab {
  width: 100%;
  padding: 22px 44px 22px 38px;
  font-size: 1.10rem;
  color: #ced9f4;
  font-family: 'Montserrat', 'HarmonyOS_Sans', 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  cursor: pointer;
  display: flex;
  align-items: center;
  border-radius: 14px 0 0 14px;
  margin-bottom: 2.5px;
  transition: 
    background 0.21s, 
    color 0.19s, 
    font-weight 0.18s,
    box-shadow 0.16s;
  user-select: none;
  position: relative;
  z-index: 1;
  letter-spacing: 1.2px;
}
.tab .icon {
  margin-right: 18px;
  font-size: 1.28rem;
  filter: drop-shadow(0 2px 8px #bddcff33);
}
.tab.active {
  background: linear-gradient(to left, #d6e5fa 98%, transparent 100%);
  color: #2e4b94;
  font-weight: bold;
  box-shadow: -7px 0 25px -12px #90caf9bb;
  position: relative;
}
.tab.active::before {
  content: "";
  display: block;
  position: absolute;
  right: 0;
  top: 14px;
  bottom: 14px;
  width: 6px;
  border-radius: 4px;
  background: linear-gradient(180deg,#6ec6ff 0%,#aabfff 100%);
  box-shadow: 0 1px 8px #7bb1ff44;
  z-index: 2;
  transition: opacity 0.2s;
}
.tab:not(.active):hover:not(.disabled) {
  background: linear-gradient(to left, #eaf3ff 60%, transparent 100%);
  color: #7db7ff;
  font-weight: 500;
  box-shadow: -2px 0 12px -10px #8bb6ff33;
}

.tab.disabled {
  color: #bdbdbd;
  cursor: not-allowed;
  opacity: 0.72;
  background: none;
  font-weight: normal;
  box-shadow: none;
}

/* 更多功能按钮固定底部 */
.nav-more {
  width: 100%;
  position: absolute;
  bottom: 46px;
  left: 0;
  padding-top: 18px;
  border-top: 1.7px solid #bbdefb33;
  background: linear-gradient(90deg, #e3f2fd22 80%, transparent 100%);
}

::-webkit-scrollbar {
  width: 8px;
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: rgba(120,150,200,0.11);
  border-radius: 8px;
}
</style>