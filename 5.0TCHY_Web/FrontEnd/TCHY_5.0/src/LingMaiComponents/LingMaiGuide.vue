<template>
  <div class="guide-container">
    <div class="guide-paper">
      
      <div class="doc-meta">
        <span class="badge">v1.2 Core</span>
        <span class="last-updated">Last updated: {{ new Date().toLocaleDateString() }}</span>
      </div>

      <header class="doc-header">
        <div class="header-main">
          <img src="/favicon.ico" class="logo-mini" alt="Logo">
          <h1>用户手册 User Guide</h1>
        </div>
        <p class="doc-intro">
          欢迎使用灵脉空间。本文档将帮助您快速掌握<strong>双向链接</strong>、<strong>快捷指令</strong>及<strong>知识图谱</strong>的使用方法，构建您的第二大脑。
        </p>
      </header>

      <hr class="divider" />

      <div class="doc-body">
        
        <section class="doc-section">
          <h2>1. 核心概念 Core Concepts</h2>
          <div class="callout info">
            <span class="icon">💡</span>
            <div class="text">
              <strong>双向链接 (Bi-directional Linking)</strong> 是灵脉空间的灵魂。不同于传统的文件夹分类，我们通过引用让知识互联。
            </div>
          </div>
          <p>
            在任意文档中输入 <kbd>[[</kbd> 即可唤起引用菜单。您可以选择现有文档，或直接输入新标题创建新节点。这种连接会在<strong>全局图谱</strong>中可视化呈现。
          </p>
        </section>

        <section class="doc-section">
          <h2>2. 编辑器与指令 Editor & Commands</h2>
          <p>我们采用“所见即所得”的富文本编辑模式，支持 Markdown 语法快捷键。</p>
          
          <div class="table-wrapper">
            <table class="cmd-table">
              <thead>
                <tr>
                  <th>操作</th>
                  <th>快捷键 / 指令</th>
                  <th>说明</th>
                </tr>
                </thead>
              <tbody>
                <tr>
                  <td>唤起菜单</td>
                  <td><kbd>/</kbd></td>
                  <td>插入标题、列表、代码块、图片等 (需要用空格隔开)</td>
                </tr>
                <tr>
                  <td>建立链接</td>
                  <td><kbd>[[</kbd></td>
                  <td>引用或创建新页面 (需要用空格隔开)</td>
                </tr>
                <tr>
                  <td>上传图片</td>
                  <td><kbd>Ctrl</kbd> + <kbd>V</kbd></td>
                  <td>直接粘贴剪贴板中的图片</td>
                </tr>
                <tr>
                  <td>粗体</td>
                  <td><kbd>Ctrl</kbd> + <kbd>B</kbd></td>
                  <td>加粗选中的文字 [也可以选中后使用菜单]</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <section class="doc-section">
          <h2>3. 视图与组织 Views & Organization</h2>
          <h3>文档层级</h3>
          <p>虽然我们强调网状连接，但树状层级依然有效。您可以直接在左侧侧边栏<strong>拖拽文档</strong>来调整父子关系。</p>
          <h3>多维视图</h3>
          <ul class="feature-list">
            <li><strong>📄 文档模式：</strong> 沉浸式写作体验，适合记录笔记、撰写文章。</li>
            <li><strong>📊 看板模式：</strong> 适合项目管理。由于我们还在持续优化中，目前建议用于简单的任务追踪。 这个被挪动到了 / 菜单中</li>
            <li><strong>🕸️ 全局图谱：</strong> 点击侧边栏入口，查看所有知识点的连接网络。</li>
          </ul>
        </section>

        <section class="doc-section">
          <h2>4. 数据与安全 Data Safety</h2>
          <div class="callout warning">
            <span class="icon">💾</span>
            <div class="text">
              <strong>自动保存机制：</strong> 系统会实时监听您的输入并自动保存。请务必留意编辑器右下角的同步状态，当显示 <code>✅ 已保存</code> 时，表示数据已安全写入数据库。
            </div>
          </div>
        </section>

      </div>

      <div class="doc-footer">
        <button class="primary-btn" @click="$emit('start')">
          <span class="icon">✨</span> 开始创建笔记
        </button>
        <p class="footer-note">遇到问题？请通过 GitHub Issues 反馈。</p>
      </div>

    </div>
  </div>
</template>

<style lang="scss" scoped>
$font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
$font-family-mono: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, Courier, monospace;

.guide-container {
  height: 100%;
  width: 100%;
  overflow-y: auto; 
  box-sizing: border-box; 
  background-color: #f7f7f5;
  
  /* 🔥 核心改动 1：不再使用 display: flex 居中 */
  /* display: flex; <--- 删除 */
  /* justify-content: center; <--- 删除 */
  
  /* 改为 padding 挤压 */
  padding: 40px 20px; 
  
  font-family: $font-family-base;
  color: #37352f;
}

.guide-paper {
  /* 🔥 核心改动 2：使用 margin auto 实现居中 */
  margin: 0 auto; 
  
  max-width: 800px;
  width: 100%;
  background: #fff;
  padding: 60px 80px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border-radius: 2px;
  min-height: 800px;
  box-sizing: border-box; 

  /* 🔥 核心改动 3：强制断词，防止文字刺穿背景框 */
  overflow-wrap: break-word; 
  word-wrap: break-word;
  word-break: break-word; 

  @media (max-width: 768px) {
    padding: 40px 24px;
  }
}

/* ... 以下样式保持不变 ... */
.doc-meta { display: flex; gap: 12px; font-size: 12px; color: #999; margin-bottom: 24px; align-items: center; .badge { background: #e6f7ff; color: #0078d4; padding: 2px 6px; border-radius: 4px; font-weight: 500; } }
.doc-header { margin-bottom: 30px; .header-main { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; .logo-mini { width: 32px; height: 32px; border-radius: 4px; } h1 { font-size: 32px; font-weight: 700; margin: 0; color: #333; letter-spacing: -0.5px; } } .doc-intro { font-size: 16px; line-height: 1.6; color: #555; } }
.divider { border: none; border-bottom: 1px solid #eaeaea; margin: 30px 0; }
.doc-body { h2 { font-size: 20px; font-weight: 600; margin-top: 40px; margin-bottom: 16px; color: #333; padding-bottom: 8px; border-bottom: 1px solid #f0f0f0; } h3 { font-size: 16px; font-weight: 600; margin-top: 24px; margin-bottom: 8px; color: #444; } p { font-size: 15px; line-height: 1.75; color: #37352f; margin-bottom: 16px; } }
.table-wrapper { border: 1px solid #e0e0e0; border-radius: 6px; overflow: hidden; margin: 20px 0; }
.cmd-table { width: 100%; border-collapse: collapse; font-size: 14px; th, td { padding: 10px 16px; text-align: left; border-bottom: 1px solid #f0f0f0; } th { background: #fafafa; font-weight: 600; color: #666; } tr:last-child td { border-bottom: none; } }
.callout { display: flex; gap: 12px; padding: 16px; border-radius: 4px; margin: 20px 0; font-size: 14px; line-height: 1.6; .icon { font-size: 18px; flex-shrink: 0; } &.info { background-color: #f1f7ff; border: 1px solid #dce9fa; color: #2c5282; } &.warning { background-color: #fff9db; border: 1px solid #faeccc; color: #745e15; } code { background: rgba(0,0,0,0.06); padding: 2px 4px; border-radius: 3px; font-family: $font-family-mono; } }
.feature-list { padding-left: 20px; li { margin-bottom: 8px; font-size: 15px; line-height: 1.6; color: #444; strong { color: #222; } } }
kbd { display: inline-block; padding: 3px 6px; font-family: $font-family-mono; font-size: 12px; line-height: 1; color: #444; vertical-align: middle; background-color: #fafafa; border: 1px solid #d1d5db; border-radius: 3px; box-shadow: 0 1px 0 rgba(0,0,0,0.1); margin: 0 2px; }
.doc-footer { margin-top: 60px; text-align: center; padding-top: 40px; border-top: 1px solid #eaeaea; .primary-btn { background: #2383e2; color: #fff; border: none; padding: 10px 24px; font-size: 15px; font-weight: 500; border-radius: 6px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; transition: background 0.2s; &:hover { background: #1a6fb0; } } .footer-note { margin-top: 16px; font-size: 12px; color: #999; } }
</style>