<template>
  <div class="test-setting-wrapper">
    <div class="scroll-container">
      <div class="main-content">
        <div class="test-header">
          <h2>测试功能</h2>
          <div class="header-line"></div>
          <p class="header-description">这里是开发中的测试功能预览</p>
        </div>

        <div class="feature-grid">
          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">增强动态系统</h3>
              <p class="card-description">支持富文本、图片、视频混合发布</p>
              <div class="card-status">
                <span class="status-badge coming-soon">即将推出</span>
              </div>
            </div>
            <button class="card-button disabled" disabled>敬请期待</button>
          </div>

          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">AI辅助创作</h3>
              <p class="card-description">智能内容生成和全局划选编辑建议</p>
              <div class="card-status">
                <span class="status-badge testing">测试中</span>
              </div>
            </div>
            <button 
              class="card-button" 
              :class="{ 'is-active': isAIEnabled }"
              @click="toggleAIAssist"
            >
              {{ isAIEnabled ? '关闭功能' : '试用' }}
            </button>
          </div>

          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">虚拟形象系统</h3>
              <p class="card-description">自定义3D虚拟形象和表情</p>
              <div class="card-status">
                <span class="status-badge coming-soon">即将推出</span>
              </div>
            </div>
            <button class="card-button disabled" disabled>敬请期待</button>
          </div>

          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">高级隐私控制</h3>
              <p class="card-description">精细化的内容可见性管理</p>
              <div class="card-status">
                <span class="status-badge coming-soon">即将推出</span>
              </div>
            </div>
            <button class="card-button disabled" disabled>敬请期待</button>
          </div>

          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">跨平台同步</h3>
              <p class="card-description">多设备间无缝数据同步</p>
              <div class="card-status">
                <span class="status-badge planning">规划中</span>
              </div>
            </div>
            <button class="card-button disabled" disabled>敬请期待</button>
          </div>

          <div class="feature-card">
            <div class="card-icon">⌬</div>
            <div class="card-content">
              <h3 class="card-title">社区活动中心</h3>
              <p class="card-description">参与线上活动和挑战</p>
              <div class="card-status">
                <span class="status-badge coming-soon">即将推出</span>
              </div>
            </div>
            <button class="card-button disabled" disabled>敬请期待</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// 引入刚刚创建的全局状态
import { isAIEnabled } from '@/stores/aiState'; 

// 切换 AI 功能开关
const toggleAIAssist = () => {
  isAIEnabled.value = !isAIEnabled.value;
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* 保留你原有的样式... */
.test-setting-wrapper { width: 100%; height: 100%; padding: 30px; box-sizing: border-box; font-family: 'Noto Sans SC', sans-serif; position: relative; overflow: hidden; }
.scroll-container { height: 100%; overflow-y: auto; padding-right: 10px; display: flex; flex-direction: column; }
.scroll-container::-webkit-scrollbar { width: 6px; }
.scroll-container::-webkit-scrollbar-thumb { background: #ddd; border-radius: 3px; }
.main-content { display: flex; flex-direction: column; gap: 30px; padding-right: 20px; }
.test-header { text-align: center; margin-bottom: 30px; }
.test-header h2 { margin: 0 0 16px 0; font-size: 24px; font-weight: 900; letter-spacing: -1px; }
.header-line { width: 40px; height: 4px; background: #000; margin: 0 auto 16px auto; }
.header-description { margin: 0; font-size: 14px; color: #666; font-family: 'Noto Sans SC', sans-serif; }
.feature-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 24px; margin-bottom: 30px; }
.feature-card { background: #F4F1EA; border-radius: 24px; padding: 24px; box-sizing: border-box; display: flex; flex-direction: column; gap: 16px; }
.card-icon { font-size: 32px; text-align: center; }
.card-content { flex: 1; display: flex; flex-direction: column; gap: 12px; }
.card-title { margin: 0; font-size: 16px; font-weight: 700; color: #333; }
.card-description { margin: 0; font-size: 14px; color: #666; line-height: 1.4; }
.card-status { display: flex; justify-content: flex-end; }
.status-badge { padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 700; font-family: 'Noto Sans SC', sans-serif; }
.status-badge.testing { background: #e8f5e9; color: #2e7d32; }
.status-badge.coming-soon { background: #fff3e0; color: #f57c00; }
.status-badge.planning { background: #eeeeee; color: #616161; }

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

/* 新增：激活状态（也就是想要关闭时）的按钮样式 */
.card-button.is-active {
  background: transparent;
  color: #d32f2f;
  border: 1px solid #d32f2f;
}

.card-button.is-active:hover {
  background: #ffebee;
  transform: translateY(-2px);
}
</style>