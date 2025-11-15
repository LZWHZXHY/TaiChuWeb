<template>
  <div class="community-intro">
    <!-- 平台概述 -->
    <div class="hero-section">
      <h1>太初寰宇</h1>
      <p class="subtitle">一个值得信任的社区</p>
      <p class="description">来一起建设社区</p>
    </div>

    <!-- 网站介绍 -->
    <div class="about-section">
      <h2>🎉 社区简介</h2>
      <div class="about-content">
        <p><strong>太初寰宇</strong>成立于2025年5月20日（是的，这是一个巧合，我们成立完才发现），是一个多元化的创意交流平台。我们致力于打造一个包容、友善的社区环境，让每个成员都能在这里找到归属感。</p>
        <p>从技术讨论到艺术创作，从学习分享到项目协作，我们为不同兴趣的创作者提供专属空间。社区定期举办线上活动、创作挑战和知识分享会，促进成员间的深度交流与合作。</p>
        <p>我们相信，每一个创意都值得被看见，每一次交流都能带来新的灵感。加入我们，共同书写属于这个社区的独特故事！</p>
      </div>
    </div>

    <!-- 快速指南 -->
    <div class="guide-section">
      <h2>🚀 快速入门</h2>
      <div class="guide-steps">
        <div class="step">
          <span class="step-number">1</span>
          <div class="step-content">
            <h4>注册账号</h4>
            <p>简单注册即可解锁全部基础功能，开始你的社区之旅</p>
          </div>
        </div>
        <div class="step">
          <span class="step-number">2</span>
          <div class="step-content">
            <h4>完善个人资料</h4>
            <p>上传头像、设置个性签名，让更多人认识独特的你</p>
          </div>
        </div>
        <div class="step">
          <span class="step-number">3</span>
          <div class="step-content">
            <h4>探索与参与</h4>
            <p>浏览感兴趣的内容，发表你的第一个作品或评论</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 群聊入口 -->
    <div class="group-section">
      <h2>💬 加入官方群聊</h2>
      <div class="group-cards">
        <div class="group-card" @click="joinQQGroup('784251645')">
          <div class="group-icon">👥</div>
          <div class="group-info">
            <h4>太初寰宇 - 太虚坛</h4>
            <p>群号：784251645</p>
            <span class="group-desc">太初寰宇社区的最大外部无门槛交流群</span>
          </div>
          <div class="join-button">立即加入</div>
        </div>
        <div class="group-card" @click="joinQQGroup('718834306')">
          <div class="group-icon">🎨</div>
          <div class="group-info">
            <h4>太初寰宇 - 八卦乾听司</h4>
            <p>群号：718834306</p>
            <span class="group-desc">这里是太初寰宇社区的反馈群，如果你对太初寰宇有任何的意见和想法可以在这里提供</span>
          </div>
          <div class="join-button">立即加入</div>
        </div>
      </div>
    </div>

    <!-- Q&A部分 -->
    <div class="qa-section">
      <h2>❓ 常见问题</h2>
      <div class="qa-list">
        <div class="qa-item">
          <h4>如何快速提升等级？</h4>
          <p>• 每日签到获得基础经验<br>• 发布优质内容获得额外奖励<br>• 参与社区活动获取大量经验<br>• 积极互动帮助其他成员</p>
        </div>
       <div class="qa-item">
          <h4>等级有什么作用？</h4>
          <p>等级越高，解锁的权限越多，包括更多的每日发布限额、专属标识等特权，能够查看更多区域，专属的头衔特效</p>
        </div>
        <div class="qa-item">
          <h4>如何反馈建议或联系管理员？</h4>
          <p>• 使用网站内的意见反馈功能<br>• 加入官方QQ群直接交流<br></p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// 加入QQ群函数
const joinQQGroup = (groupNumber) => {
  // 根据群号选择正确的链接
  let qqGroupUrl;
  if (groupNumber === '784251645') {
    qqGroupUrl = 'https://qm.qq.com/q/AlnCgd4EXA'; // 太虚坛群链接
  } else if (groupNumber === '718834306') {
    qqGroupUrl = 'https://qm.qq.com/q/2t9O9DQlZ0'; // 八卦乾听司群链接
  } else {
    // 默认链接（可选）
    qqGroupUrl = `https://qm.qq.com/q/${groupNumber}`;
  }
  
  window.open(qqGroupUrl, '_blank');
}
</script>

<style scoped>
/* 原有的CSS样式保持不变 */
.community-intro {
  max-width: 1000px;
  margin: 0 auto;
  padding: 1.5rem;
  font-family: 'Segoe UI', system-ui, sans-serif;
  color: #333;
  line-height: 1.6;
}

/* 英雄区域 */
.hero-section {
  text-align: center;
  padding: 3rem 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  color: white;
  margin-bottom: 3rem;
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
}

.hero-section h1 {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 1rem;
  text-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.subtitle {
  font-size: 1.4rem;
  opacity: 0.95;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.description {
  font-size: 1.1rem;
  max-width: 500px;
  margin: 0 auto;
  opacity: 0.9;
}

/* 关于我们部分 */
.about-section {
  margin-bottom: 3rem;
  padding: 2rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  border-left: 4px solid #667eea;
}

.about-section h2 {
  font-size: 1.8rem;
  margin-bottom: 1.5rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.about-content p {
  margin-bottom: 1.2rem;
  color: #555;
  text-align: justify;
  font-size: 1.05rem;
  line-height: 1.7;
}

.about-content p:last-child {
  margin-bottom: 0;
}

.about-content strong {
  color: #667eea;
  font-weight: 600;
}

/* 快速指南 */
.guide-section {
  margin-bottom: 3rem;
}

.guide-section h2 {
  font-size: 1.8rem;
  margin-bottom: 2rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.guide-steps {
  display: grid;
  gap: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
}

.step {
  display: flex;
  align-items: flex-start;
  gap: 1.5rem;
  padding: 1.8rem;
  background: white;
  border-radius: 12px;
  border-left: 4px solid #007bff;
  box-shadow: 0 4px 15px rgba(0,0,0,0.06);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.step:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0,0,0,0.1);
}

.step-number {
  background: linear-gradient(135deg, #007bff, #0056b3);
  color: white;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  font-weight: 600;
  flex-shrink: 0;
  margin-top: 0.2rem;
}

.step-content h4 {
  font-size: 1.2rem;
  margin-bottom: 0.8rem;
  color: #2c3e50;
  font-weight: 600;
}

.step-content p {
  color: #666;
  margin: 0;
  font-size: 1rem;
  line-height: 1.6;
}

/* 群聊入口 */
.group-section {
  margin-bottom: 3rem;
}

.group-section h2 {
  font-size: 1.8rem;
  margin-bottom: 2rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.group-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
}

.group-card {
  display: flex;
  align-items: center;
  padding: 1.5rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.06);
  cursor: pointer;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.group-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.1);
  border-color: #667eea;
}

.group-icon {
  font-size: 2.5rem;
  margin-right: 1.2rem;
  flex-shrink: 0;
}

.group-info {
  flex: 1;
}

.group-info h4 {
  font-size: 1.2rem;
  margin-bottom: 0.5rem;
  color: #2c3e50;
  font-weight: 600;
}

.group-info p {
  color: #667eea;
  font-weight: 500;
  margin-bottom: 0.3rem;
}

.group-desc {
  color: #666;
  font-size: 0.9rem;
}

.join-button {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  font-weight: 600;
  font-size: 0.9rem;
  transition: transform 0.2s ease;
}

.group-card:hover .join-button {
  transform: scale(1.05);
}

/* Q&A部分 */
.qa-section {
  margin-bottom: 2rem;
}

.qa-section h2 {
  font-size: 1.8rem;
  margin-bottom: 2rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.qa-list {
  display: grid;
  gap: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
}

.qa-item {
  padding: 1.8rem;
  background: white;
  border-radius: 12px;
  border-left: 4px solid #28a745;
  box-shadow: 0 4px 15px rgba(0,0,0,0.06);
  transition: transform 0.3s ease;
}

.qa-item:hover {
  transform: translateY(-2px);
}

.qa-item h4 {
  font-size: 1.2rem;
  margin-bottom: 1rem;
  color: #2c3e50;
  font-weight: 600;
}

.qa-item p {
  color: #555;
  margin: 0;
  font-size: 1rem;
  line-height: 1.7;
}

.qa-item strong {
  color: #28a745;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .community-intro {
    padding: 1rem;
  }
  
  .hero-section {
    padding: 2rem 1rem;
    border-radius: 12px;
  }
  
  .hero-section h1 {
    font-size: 2.2rem;
  }
  
  .subtitle {
    font-size: 1.2rem;
  }
  
  .about-section,
  .step,
  .qa-item,
  .group-card {
    padding: 1.5rem;
  }
  
  .guide-steps,
  .qa-list {
    gap: 1.2rem;
  }
  
  .group-cards {
    grid-template-columns: 1fr;
  }
  
  .step {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
  
  .step-number {
    align-self: center;
    margin-top: 0;
  }
  
  .group-card {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
  
  .group-icon {
    margin-right: 0;
  }
}

@media (max-width: 480px) {
  .hero-section h1 {
    font-size: 1.8rem;
  }
  
  .guide-section h2,
  .about-section h2,
  .qa-section h2,
  .group-section h2 {
    font-size: 1.5rem;
  }
  
  .step-content h4,
  .qa-item h4,
  .group-info h4 {
    font-size: 1.1rem;
  }
  
  .step-content p,
  .qa-item p,
  .about-content p,
  .group-desc {
    font-size: 0.95rem;
  }
}
</style>