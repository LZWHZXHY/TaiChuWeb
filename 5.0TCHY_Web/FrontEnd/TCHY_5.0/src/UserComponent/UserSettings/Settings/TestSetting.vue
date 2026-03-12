<template>
  <div class="test-setting-wrapper">
    <SuccessToast 
      v-model:isVisible="showSuccessToast"
      :title="toastTitle"
      :message="toastMessage"
    />
    
    <div class="scroll-container">
      <div class="main-content">
        <div class="test-header">
          <h2>测试功能</h2>
          <div class="header-line"></div>
          <p class="header-description">这里是开发中的测试功能预览</p>
        </div>

        <!-- 功能卡片网格 -->
        <div class="feature-grid">
          <!-- 卡片 1: 新的动态系统 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">增强动态系统</h3>
              <p class="card-description">支持富文本、图片、视频混合发布</p>
              <div class="card-status">
                <span class="status-badge testing">测试中</span>
              </div>
            </div>
            <button 
              class="card-button" 
              @click="enableFeature('enhancedDynamic')"
            >
              启用
            </button>
          </div>

          <!-- 卡片 2: AI辅助创作 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">AI辅助创作</h3>
              <p class="card-description">智能内容生成和编辑建议</p>
              <div class="card-status">
                <span class="status-badge coming-soon">即将推出</span>
              </div>
            </div>
            <button 
              class="card-button disabled" 
              disabled
            >
              暂不可用
            </button>
          </div>

          <!-- 卡片 3: 虚拟形象 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">虚拟形象系统</h3>
              <p class="card-description">自定义3D虚拟形象和表情</p>
              <div class="card-status">
                <span class="status-badge alpha">Alpha版本</span>
              </div>
            </div>
            <button 
              class="card-button" 
              @click="enableFeature('virtualAvatar')"
            >
              试用
            </button>
          </div>

          <!-- 卡片 4: 高级隐私控制 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">高级隐私控制</h3>
              <p class="card-description">精细化的内容可见性管理</p>
              <div class="card-status">
                <span class="status-badge beta">Beta版本</span>
              </div>
            </div>
            <button 
              class="card-button" 
              @click="enableFeature('advancedPrivacy')"
            >
              加入测试
            </button>
          </div>

          <!-- 卡片 5: 跨平台同步 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">跨平台同步</h3>
              <p class="card-description">多设备间无缝数据同步</p>
              <div class="card-status">
                <span class="status-badge planning">规划中</span>
              </div>
            </div>
            <button 
              class="card-button disabled" 
              disabled
            >
              敬请期待
            </button>
          </div>

          <!-- 卡片 6: 社区活动 -->
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">社区活动中心</h3>
              <p class="card-description">参与线上活动和挑战</p>
              <div class="card-status">
                <span class="status-badge testing">测试中</span>
              </div>
            </div>
            <button 
              class="card-button" 
              @click="enableFeature('communityEvents')"
            >
              参与
            </button>
          </div>
        </div>

        <!-- 反馈区域 -->
        <div class="feedback-section">
          <div class="feedback-card">
            <div class="card-header">
              <span class="deco-icon">⌬</span>
              <h3>测试反馈</h3>
            </div>
            <div class="feedback-content">
              <p>使用测试功能时遇到问题？欢迎提交反馈！</p>
              <button 
                class="feedback-button"
                @click="submitFeedback"
              >
                提交反馈
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import SuccessToast from './SuccessToast.vue';

// 成功弹窗状态
const showSuccessToast = ref(false);
const toastTitle = ref('');
const toastMessage = ref('');

// 启用功能（前端模拟）
const enableFeature = (featureName) => {
  console.log(`启用测试功能: ${featureName}`);
  
  // 显示成功弹窗
  toastTitle.value = '测试功能';
  toastMessage.value = `已成功启用功能！`;
  showSuccessToast.value = true;
  
  // 3秒后自动关闭弹窗
  setTimeout(() => {
    showSuccessToast.value = false;
  }, 3000);
};

// 提交反馈（前端模拟）
const submitFeedback = () => {
  alert('反馈提交功能将在未来版本中实现');
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* 布局调整：防止内容溢出 */
.test-setting-wrapper {
  width: 100%; height: 100%;
  padding: 30px;
  box-sizing: border-box;
  font-family: 'Noto Sans SC', sans-serif;
  position: relative;
  overflow: hidden; 
}

/* 隐藏滚动条但保留功能 */
.scroll-container {
  height: 100%;
  overflow-y: auto;
  padding-right: 10px; 
  display: flex;
  flex-direction: column;
}

.scroll-container::-webkit-scrollbar { width: 6px; }
.scroll-container::-webkit-scrollbar-thumb { background: #ddd; border-radius: 3px; }

.main-content {
  display: flex;
  flex-direction: column;
  gap: 30px;
  padding-right: 20px;
}

.test-header {
  text-align: center;
  margin-bottom: 30px;
}

.test-header h2 {
  margin: 0 0 16px 0;
  font-size: 24px;
  font-weight: 900;
  letter-spacing: -1px;
}

.header-line {
  width: 40px;
  height: 4px;
  background: #000;
  margin: 0 auto 16px auto;
}

.header-description {
  margin: 0;
  font-size: 14px;
  color: #666;
  font-family: 'Noto Sans SC', sans-serif;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
  margin-bottom: 30px;
}

.feature-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.card-icon {
  font-size: 32px;
  text-align: center;
}

.card-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.card-title {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #333;
}

.card-description {
  margin: 0;
  font-size: 14px;
  color: #666;
  line-height: 1.4;
}

.card-status {
  display: flex;
  justify-content: flex-end;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
  font-family: 'Noto Sans SC', sans-serif;
}

.status-badge.testing {
  background: #e8f5e9;
  color: #2e7d32;
}

.status-badge.coming-soon {
  background: #fff3e0;
  color: #f57c00;
}

.status-badge.alpha {
  background: #e3f2fd;
  color: #1976d2;
}

.status-badge.beta {
  background: #f3e5f5;
  color: #7b1fa2;
}

.status-badge.planning {
  background: #eeeeee;
  color: #616161;
}

.card-button {
  background: #000;
  color: #fff;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  font-family: 'Noto Sans SC', sans-serif;
  align-self: flex-start;
}

.card-button:hover:not(:disabled) {
  background: #333;
  transform: translateY(-2px);
}

.card-button.disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
}

.feedback-section {
  margin-top: auto;
}

.feedback-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
}

.card-header { 
  display: flex; 
  align-items: center; 
  gap: 8px; 
  color: #333; 
  margin-bottom: 16px; 
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

.feedback-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.feedback-content p {
  margin: 0;
  font-size: 14px;
  color: #666;
  font-family: 'Noto Sans SC', sans-serif;
}

.feedback-button {
  background: #1976d2;
  color: #fff;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  font-family: 'Noto Sans SC', sans-serif;
  align-self: flex-start;
}

.feedback-button:hover {
  background: #1565c0;
  transform: translateY(-2px);
}

/* 底部按钮 */
.footer-action { 
  margin-top: 30px; 
  display: flex; 
  justify-content: flex-end; 
  padding-bottom: 20px; 
}

.save-button {
  background: #000; 
  color: #fff; 
  padding: 12px 40px; 
  border-radius: 30px; 
  border: none;
  font-weight: 700; 
  font-size: 14px; 
  cursor: pointer; 
  display: flex; 
  align-items: center; 
  gap: 10px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.save-button:disabled { 
  opacity: 0.7; 
  cursor: not-allowed; 
}

.save-button:hover:not(:disabled) { 
  transform: translateY(-2px); 
  box-shadow: 0 6px 20px rgba(0,0,0,0.2); 
}

.btn-deco { 
  font-size: 16px; 
  color: #d35400; 
}
</style>