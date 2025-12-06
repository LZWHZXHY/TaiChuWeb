<template>
  <article class="post-card">
    <div class="post-image">
      
      <span class="category-badge">{{ post.category }}</span>
    </div>
    
    <div class="post-content">
      <h3 class="post-title">{{ post.title }}</h3>
      <p class="post-excerpt">{{ post.excerpt }}</p>
      
      <div class="post-meta">
        <div class="author-info">
          
          <span class="author-name">{{ post.author.name }}</span>
        </div>
        
        <div class="post-stats">
          <span class="post-date">{{ post.date }}</span>
          <div class="interactions">
            <span class="comments">💬 {{ post.comments }}</span>
            <span class="likes">❤️ {{ post.likes }}</span>
          </div>
        </div>
      </div>
    </div>
  </article>
</template>

<script>
export default {
  name: 'PostCard',
  props: {
    post: {
      type: Object,
      required: true
    }
  }
}
</script>

<style scoped>
.post-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.post-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
}

.post-image {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.post-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.category-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  background: var(--primary-color);
  color: white;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
}

.post-content {
  padding: 20px;
}

.post-title {
  font-size: 1.2rem;
  margin-bottom: 10px;
  color: #333;
  line-height: 1.4;
}

.post-excerpt {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.5;
  margin-bottom: 15px;
}

.post-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 15px;
  border-top: 1px solid #f0f0f0;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.author-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
}

.author-name {
  font-size: 0.9rem;
  color: #666;
}

.post-stats {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
}

.post-date {
  font-size: 0.8rem;
  color: #999;
}

.interactions {
  display: flex;
  gap: 12px;
  font-size: 0.8rem;
  color: #666;
}
</style>