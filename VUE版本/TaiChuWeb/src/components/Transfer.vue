<template>
  <div class="container">
    <div class="header">
      <h1>这里是太初寰宇中转站</h1>
      <p>请随意探索各种不同的社区板块~</p>
    </div>
    
    <div class="communities">
      <div class="community-card ChaiquanCommunity">
        <div class="card-content">
          <div class="icon">
            <i class="fas fa-users"></i>
          </div>
          <h2>柴圈社区</h2>
          <p>这里是火柴人圣地，你可以在这里交流和分享关于火柴人的内容，包括联合，借力，企划等</p>
          <div class="links">
            <a href="#" class="link-btn">柴柴柴！</a>
            <a href="/功能性页面/柴圈规则.html" class="link-btn">柴圈规则</a>
          </div>
        </div>
      </div>


      
      <div class="community-card GameCommunity">
        <div class="card-content">
          <div class="icon">
            <i class="fas fa-gamepad"></i>
          </div>
          <h2>游戏社区</h2>
          <p>太初寰宇的游戏社区, 这里会根据用户的数量和喜欢适当的开辟不同的游戏板块</p>
          <div class="links">
            <a href="#" class="link-btn">Let's Play!</a>
            
          </div>
        </div>
      </div>
      


       <div class="community-card IpCommunity">
        <div class="card-content">
          <div class="icon">
            <i class="fas fa-lightbulb"></i>
          </div>
          <h2>IP社区</h2>
          <p>原创IP孵化基地,创作者交流平台,内容合作</p>
          <div class="links">
            <a href="#" class="link-btn">查阅世界观</a>

          </div>
        </div>
      </div>

      <div class="community-card CodeCommunity">
        <div class="card-content">
          <div class="icon">
            <i class="fas fa-lightbulb"></i>
          </div>
          <h2>编程社区</h2>
          <p>喜欢敲代码？进来吧你！</p>
          <div class="links">
            <a href="#" class="link-btn">代码时间！</a>

          </div>
        </div>
      </div>

      <div class="community-card UnKnownCommunity">
        <div class="card-content">
          <div class="icon">
            <i class="fas fa-lightbulb"></i>
          </div>
          <h2>未知社区</h2>
          <p>还不知道开放什么新社区，有想法可以和我们说</p>
          <div class="links">
            <a href="#" class="link-btn">Error！</a>

          </div>
        </div>
      </div>


    </div>
    
    <div class="footer">
      <p>© 2025 太初寰宇 - 让世界因连接而更美好</p>
    </div>
  </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';

// 社区数据
const communities = ref([
  { id: 1, name: '柴圈社区', icon: 'users', color: '#4fc3f7' },
  { id: 2, name: '游戏社区', icon: 'gamepad', color: '#ab47bc' },
  { id: 3, name: 'IP社区', icon: 'lightbulb', color: '#66bb6a' }
]);

// 组件挂载后的逻辑
onMounted(() => {
  console.log('社区导航页面已加载');
  
  // 添加卡片动画延迟
  const cards = document.querySelectorAll('.community-card');
  cards.forEach((card, index) => {
    card.style.animationDelay = `${index * 0.2}s`;
  });
  
  // 确保滚动条被隐藏
  hideScrollbars();
});

// 隐藏滚动条的函数
function hideScrollbars() {
  // 隐藏所有滚动条
  const style = document.createElement('style');
  style.innerHTML = `
    body::-webkit-scrollbar { display: none; }
    .container::-webkit-scrollbar { display: none; }
    .communities::-webkit-scrollbar { display: none; }
  `;
  document.head.appendChild(style);
  
  // 添加其他浏览器的支持
  document.body.style.scrollbarWidth = 'none'; /* Firefox */
  document.body.style.msOverflowStyle = 'none'; /* IE 10+ */
}
</script>

<style scoped>

    @import url('/static/css/general.css');
    @import url('/static/css/Transfer.css');
    



    

</style>


