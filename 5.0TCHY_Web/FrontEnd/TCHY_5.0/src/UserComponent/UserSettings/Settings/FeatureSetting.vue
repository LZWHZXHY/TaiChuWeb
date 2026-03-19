<template>
  <div class="feature-setting-wrapper">
    <SuccessToast 
      v-model:isVisible="showSuccessToast"
      :title="toastTitle"
      :message="toastMessage"
    />
    
    <div class="scroll-container">
      <div class="main-content">
        <!-- 好友系统功能 -->
        <div class="feature-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>好友系统</h3>
          </div>
          
          <div class="feature-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">启用好友系统</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="friendsEnabled" 
                  v-model="friendsEnabled"
                  class="switch-input"
                />
                <label for="friendsEnabled" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">好友动态推送</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="friendFeedNotifications" 
                  v-model="friendFeedNotifications"
                  class="switch-input"
                />
                <label for="friendFeedNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">好友在线提醒</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="friendOnlineAlerts" 
                  v-model="friendOnlineAlerts"
                  class="switch-input"
                />
                <label for="friendOnlineAlerts" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 动态功能 -->
        <div class="feature-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>动态系统</h3>
          </div>
          
          <div class="feature-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">启用动态发布</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="dynamicPostsEnabled" 
                  v-model="dynamicPostsEnabled"
                  class="switch-input"
                />
                <label for="dynamicPostsEnabled" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">动态评论通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="dynamicCommentNotifications" 
                  v-model="dynamicCommentNotifications"
                  class="switch-input"
                />
                <label for="dynamicCommentNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">动态点赞通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="dynamicLikeNotifications" 
                  v-model="dynamicLikeNotifications"
                  class="switch-input"
                />
                <label for="dynamicLikeNotifications" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 博客功能 -->
        <div class="feature-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>博客系统</h3>
          </div>
          
          <div class="feature-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">启用博客功能</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="blogEnabled" 
                  v-model="blogEnabled"
                  class="switch-input"
                />
                <label for="blogEnabled" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">博客评论通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="blogCommentNotifications" 
                  v-model="blogCommentNotifications"
                  class="switch-input"
                />
                <label for="blogCommentNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">博客订阅功能</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="blogSubscription" 
                  v-model="blogSubscription"
                  class="switch-input"
                />
                <label for="blogSubscription" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 图片投稿功能 -->
        <div class="feature-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>图片投稿</h3>
          </div>
          
          <div class="feature-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">启用图片投稿</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="imageSubmissionEnabled" 
                  v-model="imageSubmissionEnabled"
                  class="switch-input"
                />
                <label for="imageSubmissionEnabled" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">图片评论通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="imageCommentNotifications" 
                  v-model="imageCommentNotifications"
                  class="switch-input"
                />
                <label for="imageCommentNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">图片点赞通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="imageLikeNotifications" 
                  v-model="imageLikeNotifications"
                  class="switch-input"
                />
                <label for="imageLikeNotifications" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 通知设置 -->
        <div class="feature-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>通知设置</h3>
          </div>
          
          <div class="feature-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">桌面通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="desktopNotifications" 
                  v-model="desktopNotifications"
                  class="switch-input"
                />
                <label for="desktopNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">邮件通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="emailNotifications" 
                  v-model="emailNotifications"
                  class="switch-input"
                />
                <label for="emailNotifications" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">推送通知</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="pushNotifications" 
                  v-model="pushNotifications"
                  class="switch-input"
                />
                <label for="pushNotifications" class="switch-label"></label>
              </div>
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

// 功能设置状态
const friendsEnabled = ref(true);
const friendFeedNotifications = ref(true);
const friendOnlineAlerts = ref(false);
const dynamicPostsEnabled = ref(true);
const dynamicCommentNotifications = ref(true);
const dynamicLikeNotifications = ref(true);
const blogEnabled = ref(true);
const blogCommentNotifications = ref(true);
const blogSubscription = ref(true);
const imageSubmissionEnabled = ref(true);
const imageCommentNotifications = ref(true);
const imageLikeNotifications = ref(true);
const desktopNotifications = ref(true);
const emailNotifications = ref(false);
const pushNotifications = ref(true);

// 成功弹窗状态
const showSuccessToast = ref(false);
const toastTitle = ref('');
const toastMessage = ref('');

// 监听设置变化，自动保存
const settingRefs = [
  friendsEnabled, friendFeedNotifications, friendOnlineAlerts,
  dynamicPostsEnabled, dynamicCommentNotifications, dynamicLikeNotifications,
  blogEnabled, blogCommentNotifications, blogSubscription,
  imageSubmissionEnabled, imageCommentNotifications, imageLikeNotifications,
  desktopNotifications, emailNotifications, pushNotifications
];

settingRefs.forEach(ref => {
  watch(ref, () => {
    showSuccessToast.value = true;
    toastTitle.value = '功能设置';
    toastMessage.value = '设置已自动保存';
    setTimeout(() => {
      showSuccessToast.value = false;
    }, 2000);
  });
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.feature-setting-wrapper {
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

.feature-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.2s ease;
}

.feature-card:hover {
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

.feature-options {
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

.switch-container {
  position: relative;
  display: inline-block;
  width: 48px;
  height: 24px;
}

.switch-input {
  opacity: 0;
  width: 0;
  height: 0;
}

.switch-label {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
  border-radius: 24px;
}

.switch-label:before {
  position: absolute;
  content: "";
  height: 20px;
  width: 20px;
  left: 2px;
  bottom: 2px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

.switch-input:checked + .switch-label {
  background-color: #000;
}

.switch-input:checked + .switch-label:before {
  transform: translateX(24px);
}
</style>