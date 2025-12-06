<template>
  <aside class="left-sidebar">
    <div class="search-box">
      <i class="fas fa-search"></i>
      <input type="text" placeholder="搜索文章、作者...">
    </div>
    
    <div class="trending-tags">
      <h3 class="sidebar-title"><i class="fas fa-tags"></i> 热门标签</h3>
      <div class="tag-cloud">
        <a v-for="tag in tags" :key="tag" href="#" class="tag">{{ tag }}</a>
      </div>
    </div>
    
    <div class="newsletter">
      <h3><i class="fas fa-envelope"></i> 订阅更新</h3>
      <p>订阅我们的邮件，获取最新文章和活动通知</p>
      <form class="newsletter-form">
        <input type="email" placeholder="输入您的邮箱">
        <button type="submit">订阅</button>
      </form>
    </div>
  </aside>
</template>

<script>
export default {
  name: 'LeftSidebar',
  data() {
    return {
      tags: ['前端', 'JavaScript', 'CSS', 'React', 'Vue', '生活', '旅行', '摄影']
    }
  }
}
</script>

<style scoped>
.left-sidebar {
  width: 280px;
  background: white;
  padding: 25px;
  box-shadow: 2px 0 10px rgba(0, 0, 0, 0.05);
}

.search-box {
  margin-bottom: 30px;
  position: relative;
}

.search-box input {
  width: 100%;
  padding: 12px 15px 12px 45px;
  border: 1px solid var(--light-gray);
  border-radius: var(--border-radius);
  font-size: 0.95rem;
}

.search-box i {
  position: absolute;
  left: 15px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--gray-color);
}

.trending-tags {
  margin-bottom: 30px;
}

.sidebar-title {
  font-size: 1.1rem;
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tag-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.tag {
  background-color: white;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  text-decoration: none;
  border: 1px solid var(--light-gray);
}

.newsletter {
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  padding: 20px;
  border-radius: var(--border-radius);
  color: white;
}

.newsletter-form input {
  width: 100%;
  padding: 10px 15px;
  border-radius: 20px;
  margin-bottom: 10px;
}

.newsletter-form button {
  width: 100%;
  background-color: white;
  color: var(--secondary-color);
  padding: 10px;
  border-radius: 20px;
}
</style>