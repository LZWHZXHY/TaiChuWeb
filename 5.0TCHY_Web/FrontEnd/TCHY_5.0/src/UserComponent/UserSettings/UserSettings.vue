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

const menuItems = ref([
  '中心首页', '资料设置', '头像设置', '个性设置', 
  '隐私设置', '安全设置', '功能设置', '其它设置', '测试功能'
])

const componentMap = {
  '中心首页': CenterSetting,
  '资料设置': ProfileSetting,
  '头像设置': AvatarSetting,
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
  justify-content: center; /* 居中 */
  align-items: flex-start;
  width: 100vw;
  min-height: 100vh;      
  position: relative;
  padding-top: 60px; /* 顶部留白 */
  z-index: 2;
}

.main-box.container {
  width: 1200px; /* 固定最大宽度，大屏更舒服 */
  max-width: 95%;
  height: 80vh; /* 视口高度的80% */
  display: flex;
  /* 关键改动：反转方向，使 Menu(right-box) 在左，Content(left-box) 在右，符合参考图 */
  flex-direction: row-reverse; 
  gap: 40px; /* 左右板块间距 */
}

/* ================= 侧边栏 (原 Right Box) ================= */
/* 视觉上现在位于左侧 */
.right-box.container {
  width: 240px; /* 菜单栏宽度固定 */
  height: auto;
  display: flex;
  flex-direction: column; 
  background: transparent; /* 透明背景融入大背景 */
  padding: 0;             
  border: none; /* 移除边框 */
  flex-shrink: 0;
}

/* 标题 "设置" */
.setting-header {
  width: 100%;
  height: auto;           
  padding: 20px 20px 20px 20px;
  display: flex;
  justify-content: flex-start; /* 左对齐 */
  align-items: center;     
  font-size: 24px;         
  font-weight: 800;       
  background-color: transparent;  
  border: none;
  color: #1a1a1a;
}

/* 菜单列表容器 */
.setting-menu {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;              
  padding: 0;          
}

/* ================= 菜单按钮样式 ================= */
.menu-btn {
  width: 100%;
  height: 48px;
  display: flex;
  align-items: center;
  padding-left: 20px; /* 文字左对齐 */
  font-size: 15px;
  font-weight: 500;
  color: #666; /* 默认灰色文字 */
  
  /* 去除默认按钮样式 */
  background-color: transparent;
  border: none;
  border-radius: 24px; /* 胶囊形状 */
  cursor: pointer;
  transition: all 0.2s ease-in-out;
}

/* 悬浮效果 */
.menu-btn:hover {
  background-color: rgba(0, 0, 0, 0.05); /* 极浅的黑色背景 */
  color: #000;
}

/* 选中状态 - 黑色背景 */
.menu-btn.active {
  background-color: #000000; /* 纯黑 */
  color: #ffffff; /* 纯白文字 */
  font-weight: bold;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15); /* 选中时的投影 */
}

/* ================= 内容区域 (原 Left Box) ================= */
/* 视觉上现在位于右侧，作为主卡片 */
.left-box.container {
  flex: 1; /* 占据剩余宽度 */
  height: 100%;
  background: #FFFDF8; /* 纯白卡片 */
  border-radius: 24px; /* 大圆角 */
  box-shadow: 0 4px 20px rgba(0,0,0,0.02); /* 极轻微的阴影，增加层次 */
  overflow: hidden; 
  padding: 0; /* 内部 padding 由子组件控制，或者在这里加 padding */
  border: none;
}

/* 如果你的子组件没有 padding，可以在这里给 left-box 加一个 padding */
/* .left-box.container { padding: 40px; } */

</style>