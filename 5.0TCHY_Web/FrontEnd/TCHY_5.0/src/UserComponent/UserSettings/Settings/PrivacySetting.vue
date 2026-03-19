<template>
  <div class="privacy-setting-wrapper">
    <SuccessToast 
      v-model:isVisible="showSuccessToast"
      :title="toastTitle"
      :message="toastMessage"
    />
    
    <div class="scroll-container">
      <div class="main-content">
        
        <div class="layout-column">
          <div class="privacy-card" :class="{ 'is-active': expanded.profile }">
            <div class="card-header" @click="toggleCard('profile')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>个人资料</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.profile }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.profile }">
              <div class="options-inner">
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">个人资料可见性</span>
                  </label>
                  <select v-model="profileVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">联系方式可见性</span>
                  </label>
                  <select v-model="contactVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">个人简介可见性</span>
                  </label>
                  <select v-model="bioVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <div class="privacy-card" :class="{ 'is-active': expanded.content }">
            <div class="card-header" @click="toggleCard('content')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>内容隐私</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.content }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.content }">
              <div class="options-inner">
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">动态可见性</span>
                  </label>
                  <select v-model="dynamicVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">博客可见性</span>
                  </label>
                  <select v-model="blogVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">图片投稿可见性</span>
                  </label>
                  <select v-model="imageVisibility" class="standard-input">
                    <option value="public">公开</option>
                    <option value="friends">仅好友</option>
                    <option value="private">私密</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="layout-column">
          <div class="privacy-card" :class="{ 'is-active': expanded.friends }">
            <div class="card-header" @click="toggleCard('friends')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>好友系统</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.friends }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.friends }">
              <div class="options-inner">
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">好友请求</span>
                  </label>
                  <select v-model="friendRequests" class="standard-input">
                    <option value="anyone">任何人</option>
                    <option value="mutual">互相关注</option>
                    <option value="none">关闭</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">在线状态显示</span>
                  </label>
                  <select v-model="onlineStatus" class="standard-input">
                    <option value="everyone">所有人</option>
                    <option value="friends">仅好友</option>
                    <option value="nobody">隐藏</option>
                  </select>
                </div>
                
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">最后在线时间</span>
                  </label>
                  <select v-model="lastSeen" class="standard-input">
                    <option value="everyone">所有人</option>
                    <option value="friends">仅好友</option>
                    <option value="nobody">隐藏</option>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <div class="privacy-card" :class="{ 'is-active': expanded.security }">
            <div class="card-header" @click="toggleCard('security')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>黑名单与安全</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.security }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.security }">
              <div class="options-inner">
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">黑名单自动拦截</span>
                  </label>
                  <select class="standard-input">
                    <option value="strict">严格</option>
                    <option value="standard">标准</option>
                    <option value="off">关闭</option>
                  </select>
                </div>
                <div class="option-item">
                  <label class="option-label">
                    <span class="option-text">陌生人私信</span>
                  </label>
                  <select class="standard-input">
                    <option value="allow">允许</option>
                    <option value="block">拦截</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';
import SuccessToast from './SuccessToast.vue';

// 展开状态管理 (全部默认为 true 展开)
const expanded = reactive({
  profile: true,
  friends: true,
  content: true,
  security: true
});

const toggleCard = (key) => {
  expanded[key] = !expanded[key];
};

// 隐私设置状态
const profileVisibility = ref('public');
const contactVisibility = ref('friends');
const bioVisibility = ref('public');
const friendRequests = ref('anyone');
const onlineStatus = ref('everyone');
const lastSeen = ref('everyone');
const dynamicVisibility = ref('public');
const blogVisibility = ref('public');
const imageVisibility = ref('public');

// 成功弹窗状态
const showSuccessToast = ref(false);
const toastTitle = ref('');
const toastMessage = ref('');

// 监听设置变化，自动触发纯前端弹窗保存
const settingRefs = [
  profileVisibility, contactVisibility, bioVisibility,
  friendRequests, onlineStatus, lastSeen,
  dynamicVisibility, blogVisibility, imageVisibility
];

let timeoutId = null;
settingRefs.forEach(ref => {
  watch(ref, () => {
    showSuccessToast.value = true;
    toastTitle.value = '隐私设置';
    toastMessage.value = '设置已自动保存';
    
    if (timeoutId) clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
      showSuccessToast.value = false;
    }, 2000);
  });
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;500;700;900&display=swap');

.privacy-setting-wrapper {
  width: 100%;
  height: 100%;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Noto Sans SC', sans-serif;
  position: relative;
  overflow: hidden;
}

.scroll-container {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-gutter: stable; /* 核心修复：永远为滚动条预留空间，防止宽度突然变化挤压布局 */
  padding: 30px;
  padding-right: 24px; /* 补偿滚动条预留的宽度，视觉上保持居中对齐 */
}

.scroll-container::-webkit-scrollbar {
  width: 6px;
  background: transparent;
}

.scroll-container::-webkit-scrollbar-thumb {
  background: #ddd; 
  border-radius: 3px; 
}

/* ======== 核心布局修改：双列 Flex ======== */
.main-content {
  display: flex;
  align-items: flex-start;
  gap: 24px;
  padding-bottom: 30px;
}

.layout-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 24px;
  min-width: 0;
}

/* ======== 卡片样式 ======== */
.privacy-card {
  background: #F4F1EA;
  border-radius: 20px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.privacy-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
}

.privacy-card.is-active {
  background: #f7f5ef;
}

.card-header { 
  display: flex; 
  justify-content: space-between;
  align-items: center; 
  color: #333; 
  cursor: pointer; 
  user-select: none; 
}

.header-title {
  display: flex; 
  align-items: center; 
  gap: 10px;
}

.card-header h3 { 
  margin: 0; 
  font-size: 15px; 
  font-weight: 700; 
  letter-spacing: 0.5px; 
}

.deco-icon { 
  font-size: 16px; 
  color: #d35400; 
  font-weight: bold;
}

.expand-icon {
  font-size: 18px;
  color: #888;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: inline-block;
}

.expand-icon.rotated {
  transform: rotate(180deg);
  color: #d35400;
}

/* ======== 折叠面板顺滑推开动画 ======== */
.options-wrapper {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open {
  grid-template-rows: 1fr;
}

.options-inner {
  min-height: 0; /* 关键修复：防止高度为0时被内部元素撑开突变 */
  overflow: hidden;
  display: flex;
  flex-direction: column;
  padding-top: 8px; /* 间距下放给父级，杜绝塌陷卡顿 */
  opacity: 0;
  transform: translateY(-8px);
  transition: opacity 0.35s ease, transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open .options-inner {
  opacity: 1;
  transform: translateY(0);
}

.option-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0; /* 将外边距改为内边距，配合 border 使用 */
  border-top: 1px solid rgba(0, 0, 0, 0.04);
}

.option-item:first-child {
  border-top: none; /* 第一个元素不显示顶边框 */
}

.option-label {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.option-text {
  font-size: 14px;
  font-weight: 500;
  color: #444;
}

.standard-input {
  width: auto;
  min-width: 130px;
  padding: 10px 14px;
  background: #FFFDF8;
  border: 1px solid #e5e5e5;
  border-radius: 12px;
  font-size: 13px;
  font-family: inherit;
  font-weight: 500;
  color: #333;
  transition: all 0.2s;
  box-sizing: border-box;
  text-align: right;
  cursor: pointer;
}

.standard-input:focus {
  border-color: #d35400;
  background: #fff;
  box-shadow: 0 4px 12px rgba(211, 84, 0, 0.05);
  outline: none;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .main-content {
    flex-direction: column;
  }
}
</style>