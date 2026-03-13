<template>
  <div class="feature-setting-wrapper">
    
    <div class="scroll-container">
      <div class="main-content">
        
        <div class="layout-column">
          
          <div class="feature-card" :class="{ 'is-active': expanded.friends }">
            <div class="card-header" @click="toggleCard('friends')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>好友系统</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.friends }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.friends }">
              <div class="options-inner">
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
            </div>
          </div>

          <div class="feature-card" :class="{ 'is-active': expanded.blog }">
            <div class="card-header" @click="toggleCard('blog')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>博客系统</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.blog }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.blog }">
              <div class="options-inner">
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
            </div>
          </div>

          <div class="feature-card" :class="{ 'is-active': expanded.notifications }">
            <div class="card-header" @click="toggleCard('notifications')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>通知设置</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.notifications }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.notifications }">
              <div class="options-inner">
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

        <div class="layout-column">
          
          <div class="feature-card" :class="{ 'is-active': expanded.dynamic }">
            <div class="card-header" @click="toggleCard('dynamic')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>动态系统</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.dynamic }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.dynamic }">
              <div class="options-inner">
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
            </div>
          </div>

          <div class="feature-card" :class="{ 'is-active': expanded.image }">
            <div class="card-header" @click="toggleCard('image')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>图片投稿</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.image }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.image }">
              <div class="options-inner">
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
            </div>
          </div>

        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';

// 展开状态管理 (默认全部展开)
const expanded = reactive({
  friends: true,
  dynamic: true,
  blog: true,
  image: true,
  notifications: true
});

const toggleCard = (key) => {
  expanded[key] = !expanded[key];
};

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
  overflow-x: hidden;
  scrollbar-gutter: stable; /* 永远为滚动条预留空间 */
  padding: 30px;
  padding-right: 24px; /* 补偿滚动条预留的宽度 */
}

.scroll-container::-webkit-scrollbar { 
  width: 6px; 
  background: transparent;
}

.scroll-container::-webkit-scrollbar-thumb { 
  background: #ddd; 
  border-radius: 3px; 
}

/* ======== 核心布局：双列 Flex ======== */
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
.feature-card {
  background: #F4F1EA;
  border-radius: 20px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.feature-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
}

.feature-card.is-active {
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

/* ======== 折叠面板顺滑动画 ======== */
.options-wrapper {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open {
  grid-template-rows: 1fr;
}

.options-inner {
  min-height: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  padding-top: 8px; /* 间距下放给父级 */
  opacity: 0;
  transform: translateY(-8px);
  transition: opacity 0.35s ease, transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open .options-inner {
  opacity: 1;
  transform: translateY(0);
}

.feature-options {
  display: flex;
  flex-direction: column;
}

.option-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-top: 1px solid rgba(0, 0, 0, 0.04); /* 边框上移以解决高度塌陷问题 */
}

.option-item:first-child {
  border-top: none;
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

/* ======== 开关 Switch 样式优化 ======== */
.switch-container {
  position: relative;
  display: inline-block;
  width: 44px; /* 稍微精简宽度以匹配更精致的 UI */
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
  background-color: #dcdcdc; /* 默认关闭状态颜色变浅些 */
  transition: .4s cubic-bezier(0.4, 0, 0.2, 1); /* 加入更平滑的曲线 */
  border-radius: 24px;
}

.switch-label:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .4s cubic-bezier(0.4, 0, 0.2, 1);
  border-radius: 50%;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); /* 圆形按钮加上微微质感 */
}

.switch-input:checked + .switch-label {
  background-color: #000;
}

.switch-input:checked + .switch-label:before {
  transform: translateX(20px);
}

/* 响应式调整 */
@media (max-width: 768px) {
  .main-content {
    flex-direction: column;
  }
}
</style>