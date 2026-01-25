<template>
  <section class="module-intro">
    <div class="briefing-terminal">
      <div class="term-header">
        <span class="blink">>></span> 企划简报
      </div>
      
      <div class="term-content">
        <div class="hero-section">
          <h3 class="hero-title">太初战场：大混乱协议 </h3>
          <p class="hero-desc">
            [ STATUS: ACTIVE ] &nbsp;&nbsp; [ VER: 2.0.4 ] 
          </p>
        </div>

        <div class="divider-line"></div>

        <div class="lore-matrix">
          
          <div class="lore-block">
            <h4 class="block-title">01 // 世界观</h4>
            <p class="text-body">
              太初战场是一片没有边界的领域，在这里什么都有可能出现，什么都有可能消失，充斥着混沌的一个地方，每时每刻里面存在的都是都在出现与消失.
              <br><br>
              这里的法则会随着出现人物而发生变化，你可以在这里做任何事情，只要你能做到
            </p>
          </div>

          <div class="lore-block">
            <h4 class="block-title">02 // 企划玩法</h4>
            <ul class="cyber-list">
              <li><span class="bullet">></span> <strong>所有参与者都可以参与世界观的编写：</strong> 需要申请，但注意，你不得编写别人已经写过的内容</li>
              <li><span class="bullet">></span> <strong>内容限制:</strong> 角色，世界观设定，剧情等内容若经审查后被判断不适宜会暂时下架处理</li>
              <li><span class="bullet">></span> <strong>企划设定限制在该企划内部:</strong> 所有设定应该被限制在 该宇宙内部，只是人物和能力可以来自作者个人企划</li>
              <li><span class="bullet">></span> <strong>战斗结果:</strong> 战斗结果由画师本身或者通过网站投票等方式决定。</li>
              <li><span class="bullet">></span> <strong>LOGS提交视频网址:</strong> 在Logs页面提交OC之间的决斗视频链接，等管理员审核完，会正确的更新数据</li>
            </ul>
          </div>

          



          <div class="lore-block full-width">
            <h4 class="block-title">03 // 目标与奖励</h4>
            <div class="reward-grid">

              <div class="reward-item">
                <span class="r-icon">🏆</span>
                <span class="r-text">季度冠军将获得专属定制插画一张</span>
                <span class="r-text">剩下的没想好</span>
              </div>


            </div>
          </div>

        </div>

        <div class="alert-strip">
          <span class="icon">!</span>
          <div class="alert-content">
            <strong>SYSTEM_NOTICE:</strong>
            这个地方的企划人设归作者本人所有。在这里填写内容仅表示愿意参加这个公开企划，任何第三方不得私自使用他人的 OC 设定进行商业活动或二创。
          </div>
        </div>

      </div>
    </div>

    <div class="action-deck">
      <button class="cyber-btn primary large" @click="$emit('switch-tab', 'upload')">
        >>> 登记角色
      </button>
      <button class="cyber-btn ghost large" @click="$emit('switch-tab', 'roster')">
        查看名册
      </button>
    </div>
  </section>
</template>

<script setup>
defineEmits(['switch-tab'])
</script>

<style scoped>
.module-intro { width: 100%; display: flex; flex-direction: column; align-items: center; }

/* 终端主容器 */
.briefing-terminal { 
  background: var(--black); 
  color: var(--white); 
  border: 2px solid var(--black); 
  margin-bottom: 30px; 
  width: 100%; 
  max-width: 900px; /* 加宽了一点以便容纳更多文字 */
  box-sizing: border-box; 
  position: relative;
  box-shadow: 10px 10px 0 rgba(0,0,0,0.15);
}

/* 顶部状态栏 */
.term-header { 
  background: #222;
  padding: 10px 20px; 
  border-bottom: 2px solid var(--red); 
  color: var(--white); 
  font-family: var(--mono);
  font-weight: bold;
  letter-spacing: 1px;
}

.term-content { padding: 30px; }

/* 英雄标题区 */
.hero-section { text-align: center; margin-bottom: 30px; }
.hero-title { 
  font-family: var(--heading); 
  font-size: 3.5rem; 
  margin: 0 0 10px 0; 
  line-height: 1; 
  text-transform: uppercase;
  letter-spacing: 2px;
}
.hero-desc { 
  font-family: var(--mono); 
  font-size: 0.8rem; 
  color: var(--red); 
  background: rgba(217, 35, 35, 0.1);
  display: inline-block;
  padding: 4px 12px;
  border: 1px solid var(--red);
}

.divider-line {
  height: 2px;
  background: repeating-linear-gradient(90deg, #444 0, #444 10px, transparent 10px, transparent 20px);
  margin-bottom: 30px;
}

/* --- 核心：内容矩阵布局 --- */
.lore-matrix {
  display: grid;
  grid-template-columns: 1fr 1fr; /* 两列布局 */
  gap: 20px;
  margin-bottom: 30px;
}

.lore-block {
  background: rgba(255,255,255,0.05);
  border: 1px solid #444;
  padding: 20px;
  transition: 0.3s;
}
.lore-block:hover {
  border-color: var(--white);
  background: rgba(255,255,255,0.08);
}

.lore-block.full-width {
  grid-column: span 2; /* 跨两列 */
}

.block-title {
  color: var(--red);
  font-family: var(--mono);
  font-size: 0.9rem;
  font-weight: bold;
  border-bottom: 1px solid #444;
  padding-bottom: 8px;
  margin: 0 0 15px 0;
}

.text-body {
  font-size: 0.9rem;
  line-height: 1.6;
  color: #ccc;
  margin: 0;
  text-align: justify;
}

/* 列表样式 */
.cyber-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
.cyber-list li {
  margin-bottom: 10px;
  font-size: 0.9rem;
  color: #ccc;
  line-height: 1.4;
}
.cyber-list strong { color: var(--white); }
.bullet { color: var(--red); margin-right: 8px; font-weight: bold; }

/* 奖励网格 */
.reward-grid {
  display: flex;
  justify-content: space-between;
  gap: 20px;
}
.reward-item {
  flex: 1;
  background: #000;
  border: 1px solid #333;
  padding: 15px;
  display: flex;
  align-items: center;
  gap: 10px;
}
.r-icon { font-size: 1.5rem; }
.r-text { font-size: 0.85rem; font-weight: bold; color: #eee; }

/* 警告条 */
.alert-strip { 
  background: rgba(217, 35, 35, 0.1); 
  border: 1px solid var(--red);
  padding: 15px; 
  display: flex; 
  gap: 15px; 
  align-items: flex-start; 
}
.alert-strip .icon { 
  background: var(--red); 
  color: #fff; 
  min-width: 24px; 
  height: 24px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-weight: bold; 
  font-family: var(--mono);
}
.alert-content {
  font-size: 0.85rem;
  color: #ddd;
  line-height: 1.4;
}
.alert-content strong { color: var(--red); }

/* 按钮组 */
.action-deck { display: flex; justify-content: center; gap: 20px; margin-top: 10px; }
.blink { animation: blink 1s infinite; }
@keyframes blink { 50% { opacity: 0; } }

/* 响应式适配 */
@media (max-width: 768px) {
  .lore-matrix { grid-template-columns: 1fr; }
  .lore-block.full-width { grid-column: span 1; }
  .reward-grid { flex-direction: column; gap: 10px; }
  .hero-title { font-size: 2.5rem; }
}
</style>