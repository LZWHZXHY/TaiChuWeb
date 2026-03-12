<template>
  <div class="other-setting-wrapper">
    <SuccessToast 
      v-model:isVisible="showSuccessToast"
      :title="toastTitle"
      :message="toastMessage"
    />
    
    <div class="scroll-container">
      <div class="main-content">
        <!-- 账户设置 -->
        <div class="other-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>账户设置</h3>
          </div>
          
          <div class="other-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">语言偏好</span>
              </label>
              <select v-model="language" class="standard-input">
                <option value="zh-CN">简体中文</option>
                <option value="en-US">English</option>
                <option value="ja-JP">日本語</option>
              </select>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">时区设置</span>
              </label>
              <select v-model="timezone" class="standard-input">
                <option value="Asia/Shanghai">中国标准时间 (UTC+8)</option>
                <option value="Asia/Tokyo">日本标准时间 (UTC+9)</option>
                <option value="America/New_York">美国东部时间 (UTC-5)</option>
                <option value="Europe/London">格林威治标准时间 (UTC+0)</option>
              </select>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">日期格式</span>
              </label>
              <select v-model="dateFormat" class="standard-input">
                <option value="YYYY-MM-DD">2026-03-12</option>
                <option value="MM/DD/YYYY">03/12/2026</option>
                <option value="DD/MM/YYYY">12/03/2026</option>
              </select>
            </div>
          </div>
        </div>

        <!-- 显示设置 -->
        <div class="other-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>显示设置</h3>
          </div>
          
          <div class="other-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">主题模式</span>
              </label>
              <select v-model="themeMode" class="standard-input">
                <option value="light">浅色主题</option>
                <option value="dark">深色主题</option>
                <option value="auto">自动</option>
              </select>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">字体大小</span>
              </label>
              <select v-model="fontSize" class="standard-input">
                <option value="small">小</option>
                <option value="medium">中</option>
                <option value="large">大</option>
              </select>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">动画效果</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="animationsEnabled" 
                  v-model="animationsEnabled"
                  class="switch-input"
                />
                <label for="animationsEnabled" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 数据与隐私 -->
        <div class="other-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>数据与隐私</h3>
          </div>
          
          <div class="other-options">
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">数据使用统计</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="usageStatistics" 
                  v-model="usageStatistics"
                  class="switch-input"
                />
                <label for="usageStatistics" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">个性化推荐</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="personalizedRecommendations" 
                  v-model="personalizedRecommendations"
                  class="switch-input"
                />
                <label for="personalizedRecommendations" class="switch-label"></label>
              </div>
            </div>
            
            <div class="option-item">
              <label class="option-label">
                <span class="option-text">Cookie 同意</span>
              </label>
              <div class="switch-container">
                <input 
                  type="checkbox" 
                  id="cookieConsent" 
                  v-model="cookieConsent"
                  class="switch-input"
                />
                <label for="cookieConsent" class="switch-label"></label>
              </div>
            </div>
          </div>
        </div>

        <!-- 账户管理 -->
        <div class="other-card">
          <div class="card-header">
            <span class="deco-icon">⌬</span>
            <h3>账户管理</h3>
          </div>
          
          <div class="account-actions">
            <div class="action-item">
              <div class="action-row">
                <button 
                  class="action-button download-btn"
                  @click="downloadData"
                >
                  下载我的数据
                </button>
                <p class="action-description">导出您的个人数据和内容</p>
              </div>
            </div>
            
            <div class="action-item">
              <div class="action-row">
                <button 
                  class="action-button delete-btn"
                  @click="deleteAccount"
                >
                  删除账户
                </button>
                <p class="action-description">永久删除您的账户和所有数据</p>
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

// 其它设置状态
const language = ref('zh-CN');
const timezone = ref('Asia/Shanghai');
const dateFormat = ref('YYYY-MM-DD');
const themeMode = ref('light');
const fontSize = ref('medium');
const animationsEnabled = ref(true);
const usageStatistics = ref(true);
const personalizedRecommendations = ref(true);
const cookieConsent = ref(true);

// 成功弹窗状态
const showSuccessToast = ref(false);
const toastTitle = ref('');
const toastMessage = ref('');

// 监听设置变化，自动保存
const settingRefs = [
  language, timezone, dateFormat, themeMode, fontSize,
  animationsEnabled, usageStatistics, personalizedRecommendations, cookieConsent
];

settingRefs.forEach(ref => {
  watch(ref, () => {
    showSuccessToast.value = true;
    toastTitle.value = '其它设置';
    toastMessage.value = '设置已自动保存';
    setTimeout(() => {
      showSuccessToast.value = false;
    }, 2000);
  });
});

// 下载数据（前端模拟）
const downloadData = () => {
  alert('数据下载功能将在未来版本中实现');
};

// 删除账户（前端模拟）
const deleteAccount = () => {
  if (confirm('确定要删除账户吗？此操作不可撤销！')) {
    alert('账户删除功能将在未来版本中实现');
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.other-setting-wrapper {
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

.other-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.2s ease;
}

.other-card:hover {
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

.other-options {
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
  min-width: 180px;
  text-align: right;
}

.standard-input:focus {
  border-color: #000;
  background: #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  outline: none;
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

.account-actions {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-item {
  display: flex;
  flex-direction: column;
}

.action-row {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: nowrap;
}

.action-button {
  background: #000;
  color: #fff;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
  font-family: 'Noto Sans SC', sans-serif;
  white-space: nowrap;
  flex-shrink: 0;
}

.action-button:hover {
  background: #333;
  transform: translateY(-2px);
}

.download-btn {
  background: #1976d2;
}

.download-btn:hover {
  background: #1565c0;
}

.delete-btn {
  background: #d32f2f;
}

.delete-btn:hover {
  background: #c62828;
}

.action-description {
  margin: 0;
  font-size: 12px;
  color: #666;
  font-family: 'Noto Sans SC', sans-serif;
  white-space: nowrap;
}
</style>