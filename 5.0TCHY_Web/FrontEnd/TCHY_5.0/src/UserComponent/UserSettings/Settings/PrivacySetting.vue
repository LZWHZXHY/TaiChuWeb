<template>
  <div class="privacy-setting-wrapper">
    <SuccessToast 
      v-model:isVisible="showSuccessToast"
      :title="toastTitle"
      :message="toastMessage"
    />
    
    <div class="scroll-container">
      <div class="main-content">
        <!-- 个人资料隐私设置 -->
        <div class="privacy-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>个人资料</h3>
          </div>
          
          <div class="privacy-options">
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

        <!-- 好友系统隐私设置 -->
        <div class="privacy-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>好友系统</h3>
          </div>
          
          <div class="privacy-options">
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

        <!-- 内容隐私设置 -->
        <div class="privacy-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>内容隐私</h3>
          </div>
          
          <div class="privacy-options">
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
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import SuccessToast from './SuccessToast.vue';

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

// 监听设置变化，自动保存
const settingRefs = [
  profileVisibility, contactVisibility, bioVisibility,
  friendRequests, onlineStatus, lastSeen,
  dynamicVisibility, blogVisibility, imageVisibility
];

settingRefs.forEach(ref => {
  watch(ref, () => {
    showSuccessToast.value = true;
    toastTitle.value = '隐私设置';
    toastMessage.value = '设置已自动保存';
    setTimeout(() => {
      showSuccessToast.value = false;
    }, 2000);
  });
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

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
  padding: 30px;
  padding-right: 10px;
}

.scroll-container::-webkit-scrollbar { width: 6px; }
.scroll-container::-webkit-scrollbar-thumb { background: #ddd; border-radius: 3px; }

.main-content {
  display: flex;
  flex-direction: column;
  gap: 30px;
  padding-right: 20px;
}

.privacy-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.2s ease;
}

.privacy-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.card-header { 
  display: flex; 
  align-items: center; 
  gap: 8px; 
  color: #333; 
  margin-bottom: 20px; 
}

.card-header h3 { 
  margin: 0; 
  font-size: 14px; 
  font-weight: 700; 
  letter-spacing: 0.5px; 
}

.deco-icon { 
  font-size: 16px; 
  color: #d35400; 
}

.privacy-options {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.option-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.option-item:last-child {
  border-bottom: none;
}

.option-label {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.option-text {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.standard-input {
  width: 100%;
  padding: 12px 16px;
  background: #FFFDF8;
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  font-size: 14px;
  color: #333;
  transition: all 0.2s;
  box-sizing: border-box;
  min-width: 120px;
  text-align: right;
}

.standard-input:focus {
  border-color: #000;
  background: #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  outline: none;
}
</style>