<template>
  <router-link :to="`/blog/${data.id}`" class="card-link">
    <div class="cyber-card blog-card">
      <div class="card-header">
        <span class="author">{{ data.author }}</span>
        <span class="timestamp">{{ formatDate(data.timestamp) }}</span>
      </div>
      
      <div class="card-body">
        <h2 class="blog-title">{{ data.title }}</h2>
        <p class="blog-excerpt">{{ data.excerpt }}</p>
      </div>
      
      <div class="card-footer">
        <span class="read-more">READ MORE >></span>
        <span class="tag type-blog">ARTICLE</span>
      </div>
    </div>
  </router-link>
</template>

<script setup>
defineProps({ data: Object });
const formatDate = (dateString) => {
  const d = new Date(dateString);
  return `${d.getMonth() + 1}-${d.getDate()} ${d.getHours()}:${String(d.getMinutes()).padStart(2, '0')}`;
};
</script>

<style scoped>
.card-link { text-decoration: none; display: block; }
.cyber-card {
  padding: 24px;
  background: linear-gradient(135deg, rgba(20,20,20,0.8) 0%, rgba(5,5,5,0.9) 100%);
  border: 1px solid rgba(255, 0, 85, 0.2); /* 博客用霓虹粉/红色调区分 */
  border-radius: 8px;
  transition: all 0.3s ease;
}
.cyber-card:hover {
  border-color: rgba(255, 0, 85, 0.6);
  box-shadow: 0 4px 20px rgba(255, 0, 85, 0.15);
}

.card-header { display: flex; gap: 12px; color: #888; font-size: 12px; margin-bottom: 16px; font-family: 'JetBrains Mono', monospace; }
.author { color: #FF0055; }

.blog-title { margin: 0 0 12px 0; color: #fff; font-size: 20px; letter-spacing: 1px; }
.blog-excerpt { color: #aaa; font-size: 14px; line-height: 1.6; margin: 0; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

.card-footer { margin-top: 20px; display: flex; justify-content: space-between; align-items: center; }
.read-more { color: #FF0055; font-size: 12px; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.tag { font-size: 10px; padding: 2px 6px; border-radius: 2px; font-family: 'JetBrains Mono', monospace; }
.type-blog { color: #FF0055; border: 1px solid #FF0055; }
</style>