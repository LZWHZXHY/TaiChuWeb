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
          设置中心
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
import { ref, shallowRef } from 'vue'

// 1. 引入刚才创建的所有子组件
// 注意：请确保路径正确，如果组件在 components 文件夹下，路径可能是 './components/CenterSetting.vue'
import CenterSetting from './Settings/CenterSetting.vue'
import ProfileSetting from './Settings/ProfileSetting.vue'
import AvatarSetting from './Settings/AvatarSetting.vue'
import PersonalSetting from './Settings/PersonalSetting.vue'
import PrivacySetting from './Settings/PrivacySetting.vue'
import SecuritySetting from './Settings/SecuritySetting.vue'
import FeatureSetting from './Settings/FeatureSetting.vue'
import OtherSetting from './Settings/OtherSetting.vue'
import TestSetting from './Settings/TestSetting.vue'

// 2. 定义菜单项名称列表
const menuItems = ref([
  '中心首页', 
  '资料设置', 
  '头像设置', 
  '个性设置', 
  '隐私设置', 
  '安全设置', 
  '功能设置', 
  '其它设置', 
  '测试功能'
])

// 3. 建立 "中文名称" 到 "组件对象" 的映射关系
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

// 4. 定义当前选中的状态
// currentLabel 用于控制按钮的高亮样式
const currentLabel = ref('中心首页')
// currentComponent 用于控制左侧显示哪个组件 (使用 shallowRef 优化性能)
const currentComponent = shallowRef(CenterSetting)

// 5. 切换菜单的方法
const switchMenu = (itemName) => {
  currentLabel.value = itemName
  currentComponent.value = componentMap[itemName]
}
</script>

<style scoped>
/* 全局容器设置 */
.container {
  border: 1px solid #000000;
  background: transparent;
  box-sizing: border-box; 
}

.background {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 0;
  background-color: #f5f5f5;
}

.New-Profile.container {
  display: flex;
  flex-direction: column;
  justify-content: start;
  align-items: center;
  width: 100vw;
  height: 89vh;      
  position: relative;
  padding: 1% 0px 2% 0px;
  z-index: 2;
}

.main-box.container {
  width: 60%;
  height: 100%;
  margin-top: 1%;
  display: flex;
}

/* 左侧盒子 */
.left-box.container {
  width: 80%;
  height: 100%;
  background: #ffffff;
  overflow: hidden; /* 防止子组件内容溢出 */
}

/* 右侧盒子 */
.right-box.container {
  width: 20%;
  height: 100%;
  display: flex;
  flex-direction: column; 
  background: #f9f9f9;
  padding: 0;             
  border-left: 1px solid #ddd; 
}

/* 标题区域 */
.setting-header {
  width: 100%;
  height: 60px;           
  display: flex;
  justify-content: center; 
  align-items: center;     
  font-size: 16px;         
  font-weight: bold;       
  background-color: #fff;  
  border-bottom: 1px solid #e0e0e0; 
}

/* 按钮区域 */
.setting-menu {
  flex: 1;                
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 10px;              
  padding: 10px;          
  align-items: center;
  overflow-y: auto;       
  box-sizing: border-box;
}

.menu-btn {
  width: 100%;
  height: 50px;
  max-height: 50px;
  min-height: 50px; /* 防止被压缩 */
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 14px;
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 5px;
  cursor: pointer;
  transition: all 0.2s;
}

.menu-btn:hover {
  background-color: #e6f7ff;
  border-color: #1890ff;
  color: #1890ff;
}

/* 新增：选中状态的样式 */
.menu-btn.active {
  background-color: #1890ff;
  color: #ffffff;
  border-color: #1890ff;
}
</style>