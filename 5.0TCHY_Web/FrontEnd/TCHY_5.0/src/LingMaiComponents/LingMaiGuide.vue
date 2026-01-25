<template>
  <div class="guide-container">
    <div class="guide-content">
      <header class="hero">
        <div class="icon-wrapper">
          <img src="/favicon.ico" class="logo-img" alt="LOGO">
        </div>
        <h1>欢迎来到灵脉空间</h1>
        <p class="subtitle">不仅仅是笔记，更是你的第二大脑。通过双向链接构建知识网络。</p>
      </header>

      <div class="features-grid">
        <div class="feature-card">
          <div class="card-icon">⚡</div>
          <h3>快捷指令</h3>
          <p>
            在编辑器中输入 <kbd>/</kbd> 唤起菜单，快速插入标题、列表、代码块或<strong>上传图片</strong>。<br>
            支持 <kbd>Ctrl</kbd> + <kbd>V</kbd> 直接粘贴图片。
          </p>
        </div>

        <div class="feature-card highlight">
          <div class="card-icon">🔗</div>
          <h3>双向链接 (核心)</h3>
          <p>
            输入 <kbd>[[</kbd> 即可引用或创建其他笔记。<br>
            这会在文档间建立<strong>实体连接</strong>，并在知识图谱中显示引用关系。
          </p>
        </div>

        <div class="feature-card">
          <div class="card-icon">🕸️</div>
          <h3>全局图谱</h3>
          <p>点击侧边栏的“全局关系图谱”，以上帝视角俯瞰你的知识星空。支持缩放查看细节，点击节点直接跳转。</p>
        </div>

        <div class="feature-card">
          <div class="card-icon">📊</div>
          <h3>看板与文档</h3>
          <p>
            在编辑器顶部切换视图。<br>
            <strong>文档模式</strong>适合深度写作，<strong>看板模式</strong>适合项目管理。(看板功能持续优化中)
          </p>
        </div>

        <div class="feature-card warning">
          <div class="card-icon">💾</div>
          <h3>保存机制</h3>
          <p>
            系统会自动保存内容。请留意右下角的同步状态，当显示<strong>“✅ 已保存”</strong>时，数据才算安全落盘。
          </p>
        </div>

        <div class="feature-card">
          <div class="card-icon">🚀</div>
          <h3>未来计划</h3>
          <p>
            我们将支持视频链接、社区帖子嵌入等功能。<br>
            欢迎在意见区反馈，让我们一起打造解决实际生活问题的工具。
          </p>
        </div>
      </div>

      <div class="action-area">
        <button class="start-btn" @click="$emit('start')">开始创建第一篇笔记</button>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.guide-container {
  height: 100%;
  overflow-y: auto;
  background-color: #fff;
  display: flex;
  justify-content: center;
  padding: 60px 20px;
}

.guide-content {
  max-width: 900px;
  width: 100%;
}

.hero {
  text-align: center;
  margin-bottom: 60px;

  .icon-wrapper {
    margin-bottom: 24px;
    
    .logo-img {
      height: 80px;   /* 固定高度 */
      width: auto;    /* 自适应宽度 */
      object-fit: contain;
      display: block;
      margin: 0 auto; /* 水平居中 */
      border-radius: 12px; /* 稍微圆角一点更好看 */
    }
  }

  h1 { 
    font-size: 32px; 
    color: #1f2937; 
    margin-bottom: 12px; 
    font-weight: 800; 
    letter-spacing: -0.5px;
  }
  
  .subtitle { 
    font-size: 18px; 
    color: #6b7280; 
    font-weight: 400; 
    line-height: 1.6;
  }
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); /* 响应式布局 */
  gap: 24px;
  margin-bottom: 60px;
}

.feature-card {
  padding: 24px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  transition: all 0.2s ease;
  background: #ffffff;
  display: flex;
  flex-direction: column;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 24px rgba(0,0,0,0.06);
    border-color: #d1d5db;
  }

  /* 高亮卡片样式 */
  &.highlight {
    background: #eff6ff;
    border-color: #bfdbfe;
    
    .card-icon { background: #dbeafe; border-radius: 8px; width: fit-content; padding: 4px; }
  }

  /* 警告/注意卡片样式 */
  &.warning {
    background: #ffffff;
    border-left: 4px solid #f59e0b; /* 左侧橙色条 */
  }

  .card-icon { 
    font-size: 28px; 
    margin-bottom: 16px; 
    line-height: 1;
  }

  h3 { 
    font-size: 16px; 
    font-weight: 600; 
    color: #111827; 
    margin-bottom: 8px; 
  }

  p { 
    font-size: 14px; 
    color: #4b5563; 
    line-height: 1.6; 
    flex: 1; /* 让文字撑满 */
  }

  /* 键盘按键样式优化 */
  kbd {
    background: #f3f4f6;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    padding: 2px 6px;
    font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
    font-size: 0.85em;
    color: #374151;
    box-shadow: 0 1px 0 rgba(0,0,0,0.1);
  }
}

.action-area {
  text-align: center;
  padding-bottom: 40px;
}

.start-btn {
  background: #2383e2;
  color: white;
  border: none;
  padding: 14px 40px;
  font-size: 16px;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 6px -1px rgba(35, 131, 226, 0.2), 0 2px 4px -1px rgba(35, 131, 226, 0.1);

  &:hover { 
    background: #1a6fb0; 
    transform: translateY(-1px);
    box-shadow: 0 10px 15px -3px rgba(35, 131, 226, 0.3);
  }
  
  &:active {
    transform: translateY(0);
  }
}
</style>