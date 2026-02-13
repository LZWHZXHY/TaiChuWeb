<template>
  <div class="container"></div>

  <div class="background"><backgroundcard /></div>

  <div class="New-Profile container">
    <div class="main-box container">
      
      <div class="left-box container">
        <component :is="currentComponent" />
      </div>

      <div class="right-box container">
        <div class="setting-header">
          设置
        </div>

        <div class="setting-menu">
          <button 
            v-for="(item, index) in menuItems" 
            :key="index" 
            class="menu-btn"
            :class="{ active: currentLabel === item }" 
            @click="switchMenu(item)"
          >
            {{ item }}
          </button>
        </div>
      </div>

      <div class="xuanxiang"></div>
    </div>
  </div>
</template>

<script setup>
// 你的 Script 保持不变
import { ref, shallowRef } from 'vue'
import CenterSetting from './Settings/CenterSetting.vue'
import ProfileSetting from './Settings/ProfileSetting.vue'
import AvatarSetting from './Settings/AvatarSetting.vue'
import PersonalSetting from './Settings/PersonalSetting.vue'
import PrivacySetting from './Settings/PrivacySetting.vue'
import SecuritySetting from './Settings/SecuritySetting.vue'
import FeatureSetting from './Settings/FeatureSetting.vue'
import OtherSetting from './Settings/OtherSetting.vue'
import TestSetting from './Settings/TestSetting.vue'
import ContentSetting from './Settings/ContentSetting.vue'
const menuItems = ref([
  '中心首页', '资料设置', '内容管理', '头像设置', '个性设置', 
  '隐私设置', '安全设置', '功能设置', '其它设置', '测试功能'
])

const componentMap = {
  '中心首页': CenterSetting,
  '资料设置': ProfileSetting,
  '头像设置': AvatarSetting,
  '内容管理': ContentSetting, // 新增映射
  '个性设置': PersonalSetting,
  '隐私设置': PrivacySetting,
  '安全设置': SecuritySetting,
  '功能设置': FeatureSetting,
  '其它设置': OtherSetting,
  '测试功能': TestSetting
}

const currentLabel = ref('中心首页')
const currentComponent = shallowRef(CenterSetting)

const switchMenu = (itemName) => {
  currentLabel.value = itemName
  currentComponent.value = componentMap[itemName]
}
</script>

<style scoped>
/* ================= 重置与基础 ================= */
* {
  box-sizing: border-box;
}

/* 移除原来 container 的黑边框，保持布局属性 */
.container {
  background: transparent;
  box-sizing: border-box; 
}

/* 全局背景色 - 暖米色 */
.background {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 0;
  background-color: #F4F1EA; /* 用户指定主色调 */
}

/* ================= 主布局容器 ================= */
.New-Profile.container {
  display: flex;
  justify-content: center; 
  align-items: flex-start;
  width: 100vw;
  min-height: 100vh;      
  position: relative;
  padding-top: 60px; 
  z-index: 2;
}

.main-box.container {
  width: 1200px; 
  max-width: 95%;
  height: 80vh; /* 视口高度的80% */
  display: flex;
  /* 菜单(right-box) 在左，Content(left-box) 在右 */
  flex-direction: row-reverse; 
  gap: 40px; 
}

/* ================= 侧边栏 (原 Right Box) ================= */
.right-box.container {
  width: 240px; 
  height: auto;
  display: flex;
  flex-direction: column; 
  background: transparent; 
  padding: 0;             
  border: none; 
  flex-shrink: 0;
}

.setting-header {
  width: 100%;
  height: auto;           
  padding: 20px;
  display: flex;
  justify-content: flex-start; 
  align-items: center;     
  font-size: 24px;         
  font-weight: 800;       
  color: #1a1a1a;
}

.setting-menu {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;              
}

/* ================= 菜单按钮样式 ================= */
.menu-btn {
  width: 100%;
  height: 48px;
  display: flex;
  align-items: center;
  padding-left: 20px; 
  font-size: 15px;
  font-weight: 500;
  color: #666; 
  background-color: transparent;
  border: none;
  border-radius: 24px; 
  cursor: pointer;
  transition: all 0.2s ease-in-out;
}

.menu-btn:hover {
  background-color: rgba(0, 0, 0, 0.05); 
  color: #000;
}

.menu-btn.active {
  background-color: #000000; 
  color: #ffffff; 
  font-weight: bold;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15); 
}

/* ================= 内容区域 (核心修改点) ================= */
.left-box.container {
  flex: 1; 
  height: 100%; /* 继承父容器 80vh 的高度 */
  background: #FFFDF8; 
  border-radius: 24px; 
  box-shadow: 0 4px 20px rgba(0,0,0,0.02); 
  
  /* --- 关键修复：开启内部滚动 --- */
  overflow-y: auto;  
  overflow-x: hidden; 
  /* ---------------------------- */
  
  padding: 0; 
  border: none;
  scroll-behavior: smooth;
}

/* ================= 自定义精致滚动条 (工业美学) ================= */
/* 滚动条整体宽度 */
.left-box.container::-webkit-scrollbar {
  width: 6px;
}

/* 滚动条轨道 */
.left-box.container::-webkit-scrollbar-track {
  background: transparent;
}

/* 滚动条滑块 (默认浅色，呼应背景) */
.left-box.container::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.05);
  border-radius: 10px;
}

/* 鼠标悬停在卡片上时滑块变深，起到提示作用 */
.left-box.container:hover::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.1);
}

/* 手动抓取滚动条时变深 */
.left-box.container::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.3);
}

</style>