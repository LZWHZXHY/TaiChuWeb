<template>
  <main class="main-content">
    <section class="posts-section">
      <div class="section-header">
        <h2 class="section-title">热门文章</h2>
        <a href="#" class="view-all">查看全部</a>
      </div>
      
      <div class="posts-grid">
        <PostCard 
          v-for="post in posts" 
          :key="post.id"
          :post="post"
        />
      </div>
    </section>
  </main>
</template>

<script>
// 导入 PostCard 组件
import PostCard from './PostCard.vue'

export default {
  name: 'MainContent',
  components: {
    // 注册 PostCard 组件
    PostCard
  },
  data() {
    return {
      posts: [
        {
          id: 1,
          title: '深入理解CSS Grid布局',
          excerpt: 'CSS Grid布局是CSS中功能最强大的布局系统...',
          image: 'https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1170&h=780&fit=crop',
          category: '技术',
          author: {
            name: '张伟',
            avatar: 'https://example.com/32.jpg'
          },
          date: '2023-10-15',
          comments: 12,
          likes: 45
        },
        {
          id: 2,
          title: '周末咖啡馆时光',
          excerpt: '周末的早晨，阳光透过咖啡馆的玻璃窗洒进来...',
          image: 'https://images.unsplash.com/photo-1499750310107-5fef28a66643?w=1170&h=780&fit=crop',
          category: '生活',
          author: {
            name: '李娜',
            avatar: 'https://example.com/44.jpg'
          },
          date: '2023-10-12',
          comments: 8,
          likes: 32
        }
      ]
    }
  }
}
</script>

<style scoped>
.main-content {
  flex: 1;
  padding: 40px 30px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.section-title {
  font-size: 1.8rem;
  color: var(--primary-color);
  margin: 0;
}

.view-all {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 500;
}

.view-all:hover {
  text-decoration: underline;
}

.posts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 30px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .main-content {
    padding: 20px 15px;
  }
  
  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .posts-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }
}
</style>