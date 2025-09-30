<template>
  <div class="BigContainer">
    <nav class="TopBar">
      <ul>
        <li><span>协议！必看</span></li>
        <li><span>周报</span></li>
        <li><span>参与成员</span></li>
        <li><span>项目管理</span></li>
        <li><span>链接合集</span></li>
      </ul>
    </nav>

    <div class="MainContent">
      <aside class="SidePanel leftPanel">
        <div class="PanelTitle">功能选项</div>
        <ul class="OptionList">
          <li
            v-for="opt in options"
            :key="opt.key"
            @click="activeOption = opt.key"
            :class="{active: activeOption === opt.key}"
          >
            <span>{{ opt.label }}</span>
          </li>
        </ul>
      </aside>
      <section class="viewWindow">
        <!-- 这里显示主内容区域 -->
        <component :is="viewWindowMap[activeOption] || DefaultContent" />
      </section>
      <aside class="SidePanel rightPanel">
        <!-- 右侧面板内容 -->
      </aside>
    </div>
  </div>
</template>

<script setup>
import { ref, defineComponent } from "vue";
import UploadChannelInfo from './BYD_production/UploadInfo.vue';

// 可扩展选项列表
const options = [
  { key: "upload", label: "上传通道" },
  { key: "another", label: "其它功能" },
  // 继续添加更多选项
];

const activeOption = ref("upload");

// 中间窗口内容映射
const viewWindowMap = {
  upload: UploadChannelInfo, // 直接赋值组件
  another: defineComponent({
    template: `<div class="placeholder" style="color: #fff;">这里是其它功能内容区域（可扩展）</div>`
  }),
  // 更多内容可以继续添加
};

// 默认内容组件
const DefaultContent = defineComponent({
  template: `<div class="placeholder" style="color: #fff;">请选择左侧功能选项</div>`
});
</script>

<style scoped>
.BigContainer {
  background-color: aliceblue;
  width: 100vw;
  min-height: 100vh;
  position: absolute;
}

.TopBar {
  width: 100%;
  background: linear-gradient(90deg, #616161 0%, #505050 100%);
  box-shadow: 0 2px 8px #cfd8dc;
  padding: 0;
  position: sticky;
  top: 0;
  z-index: 100;
}

.TopBar ul {
  display: flex;
  list-style: none;
  margin: 0;
  padding: 0 4vw;
  height: 56px;
  align-items: center;
  gap: 36px;
}

.TopBar li {
  height: 100%;
  display: flex;
  align-items: center;
  transition: background 0.18s, color 0.18s;
  cursor: pointer;
  border-radius: 8px;
  padding: 0 16px;
  user-select: none;
}

.TopBar li:active,
.TopBar li:focus,
.TopBar li:hover {
  background: #ffe082;
}

.TopBar span {
  font-size: 17px;
  font-weight: 600;
  color: #f5f5f5;
  letter-spacing: 1px;
  transition: color 0.18s;
}

.TopBar li:hover span,
.TopBar li:active span,
.TopBar li:focus span {
  color: #424242;
}

.MainContent {
  display: flex;
  width: 100vw;
  height: 80vh;
  margin-top: 36px;
  justify-content: center;
  align-items: stretch;
}

.SidePanel {
  width: 10vw;
  min-width: 120px;
  background: #f6f7fb;
  border-radius: 16px;
  margin: 0 1vw;
  box-shadow: 0 1px 6px #e0e7ef;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  padding: 18px 0;
}

.PanelTitle {
  text-align: center;
  font-size: 16px;
  font-weight: 700;
  color: #2563eb;
  margin-bottom: 12px;
  letter-spacing: 1px;
}

.OptionList {
  list-style: none;
  padding: 0;
  margin: 0 0 12px 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.OptionList li {
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 15px;
  color: #424242;
  cursor: pointer;
  transition: background 0.18s, color 0.18s;
  display: flex;
  align-items: center;
}

.OptionList li.active,
.OptionList li:hover,
.OptionList li:focus {
  background: #dbeafe;
  color: #2563eb;
}

.OptionList li span {
  font-weight: 600;
}

.SidePanelContent {
  margin: 10px 16px 0 16px;
  padding: 12px;
  border-radius: 10px;
  background: #fff;
  box-shadow: 0 2px 8px #e0e7ef;
  font-size: 15px;
  min-height: 80px;
}

.placeholder {
  color: #888;
  text-align: center;
  font-size: 15px;
  padding: 8px 0;
}

.viewWindow {
  width: 80vw;
  background-color: #424242;
  border-radius: 18px;
  box-shadow: 0 2px 12px #cfd8dc;
  padding: 26px;
  min-height: 360px;
  margin: 0 1vw;
  display: flex;
  flex-direction: column;
  align-items: center;
}
</style>