<template>
  <div class="other-setting-wrapper">
    
    <div class="scroll-container">
      <div class="main-content">
        
        <div class="layout-column">
          <div class="other-card" :class="{ 'is-active': expanded.account }">
            <div class="card-header" @click="toggleCard('account')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>账户设置</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.account }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.account }">
              <div class="options-inner">
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
                      <option value="Asia/Shanghai">中国标准时间</option>
                      <option value="Asia/Tokyo">日本标准时间</option>
                      <option value="America/New_York">美国东部时间</option>
                      <option value="Europe/London">格林威治标准</option>
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
            </div>
          </div>

          <div class="other-card" :class="{ 'is-active': expanded.data }">
            <div class="card-header" @click="toggleCard('data')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>数据与隐私</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.data }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.data }">
              <div class="options-inner">
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
            </div>
          </div>
        </div>

        <div class="layout-column">
          <div class="other-card" :class="{ 'is-active': expanded.display }">
            <div class="card-header" @click="toggleCard('display')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>显示设置</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.display }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.display }">
              <div class="options-inner">
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
            </div>
          </div>

          <div class="other-card" :class="{ 'is-active': expanded.management }">
            <div class="card-header" @click="toggleCard('management')">
              <div class="header-title">
                <span class="deco-icon">⌬</span>
                <h3>账户管理</h3>
              </div>
              <span class="expand-icon" :class="{ 'rotated': expanded.management }">▾</span>
            </div>
            
            <div class="options-wrapper" :class="{ 'is-open': expanded.management }">
              <div class="options-inner">
                <div class="account-actions">
                  <div class="action-item">
                    <div class="action-row">
                      <div class="action-info">
                        <p class="action-title">下载我的数据</p>
                        <p class="action-description">导出您的个人数据和内容</p>
                      </div>
                      <button class="action-button download-btn" @click="downloadData">
                        下载
                      </button>
                    </div>
                  </div>
                  
                  <div class="action-item">
                    <div class="action-row">
                      <div class="action-info">
                        <p class="action-title">删除账户</p>
                        <p class="action-description">永久删除账户和所有数据</p>
                      </div>
                      <button class="action-button delete-btn" @click="deleteAccount">
                        删除
                      </button>
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
  account: true,
  display: true,
  data: true,
  management: true
});

const toggleCard = (key) => {
  expanded[key] = !expanded[key];
};

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

// 下载数据（前端模拟）
const downloadData = () => {
  alert('数据下载功能将在未来版本中实现');
};

// 删除账户（前端模拟）
const deleteAccount = () => {
  if (confirm('确定要删除账户吗？此操作不可撤销！')) {
    alert('账户已永久删除 (模拟)');
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;500;700;900&display=swap');

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
  overflow-x: hidden;
  scrollbar-gutter: stable; /* 永远为滚动条预留空间，防挤压抖动 */
  padding: 30px;
  padding-right: 24px;
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
.other-card {
  background: #F4F1EA;
  border-radius: 20px;
  padding: 24px;
  box-sizing: border-box;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.other-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
}

.other-card.is-active {
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
  padding-top: 8px;
  opacity: 0;
  transform: translateY(-8px);
  transition: opacity 0.35s ease, transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
}

.options-wrapper.is-open .options-inner {
  opacity: 1;
  transform: translateY(0);
}

.other-options, .account-actions {
  display: flex;
  flex-direction: column;
}

.option-item, .action-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  border-top: 1px solid rgba(0, 0, 0, 0.04);
}

.option-item:first-child, .action-item:first-child {
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

/* Select 输入框同步 */
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

/* ======== 开关 Switch 样式 ======== */
.switch-container {
  position: relative;
  display: inline-block;
  width: 44px;
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
  background-color: #dcdcdc;
  transition: .4s cubic-bezier(0.4, 0, 0.2, 1);
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
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.switch-input:checked + .switch-label {
  background-color: #000;
}

.switch-input:checked + .switch-label:before {
  transform: translateX(20px);
}

/* ======== 账户管理按钮区域 ======== */
.action-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  gap: 16px;
}

.action-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.action-title {
  margin: 0;
  font-size: 14px;
  font-weight: 500;
  color: #444;
}

.action-description {
  margin: 0;
  font-size: 12px;
  color: #888;
}

.action-button {
  background: #000;
  color: #fff;
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
  white-space: nowrap;
  flex-shrink: 0;
}

.action-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.download-btn {
  background: #333; /* 配合整体黑白金配色调整了更素雅的颜色 */
}

.download-btn:hover {
  background: #000;
}

.delete-btn {
  background: transparent;
  color: #d32f2f;
  border: 1px solid #ffcdd2;
}

.delete-btn:hover {
  background: #ffebee;
  color: #c62828;
  box-shadow: none;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .main-content {
    flex-direction: column;
  }
}
</style>