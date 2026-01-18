<template>
  <div class="min-blog-item" @click="$emit('click')">
    <div class="blog-indicator" :class="{ 'is-top': blog.isTop }"></div>
    
    <div v-if="blog.CoverImage" class="min-blog-thumb">
      <img :src="coverUrl" @error="handleImgError" loading="lazy" />
    </div>
    
    <div class="blog-content">
      <div class="blog-top-row">
        <span class="blog-tag">#{{ (blog.Tags && blog.Tags[0]) || 'CORE_DATA' }}</span>
        <div class="blog-accent" v-if="blog.isTop">ARCHIVE_TOP</div>
      </div>
      <p class="blog-title">{{ blog.Title }}</p>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  blog: { type: Object, required: true },
  baseUrl: { type: String, default: '' }
});

defineEmits(['click']);

const coverUrl = computed(() => {
  const url = props.blog.CoverImage;
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  return `${props.baseUrl}/${url.replace(/\\/g, '/')}`;
});

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };
</script>

<style scoped>
.min-blog-item {
  display: flex; gap: 10px; padding: 10px; margin-bottom: 10px;
  border: 1px solid #ddd; transition: all 0.2s; position: relative;
  background: #fdfdfd; cursor: pointer;
}
.min-blog-item:hover { 
  transform: translate(-2px, -2px); 
  border-color: #111; 
  box-shadow: 2px 2px 0 #111; 
}

.blog-indicator { width: 4px; background: #ddd; position: absolute; left: 0; top: 0; bottom: 0; transition: 0.2s; }
.blog-indicator.is-top { background: #D92323; }
.min-blog-item:hover .blog-indicator { width: 6px; }

.min-blog-thumb { width: 60px; height: 60px; border: 1px solid #111; flex-shrink: 0; }
.min-blog-thumb img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); transition: 0.3s; }
.min-blog-item:hover img { filter: grayscale(0); }

.blog-top-row { 
  display: flex; justify-content: space-between; 
  font-size: 0.7rem; font-family: 'JetBrains Mono'; margin-bottom: 4px; 
}
.blog-tag { color: #D92323; font-weight: 700; }
.blog-accent { background: #111; color: #fff; padding: 0 4px; font-size: 0.6rem; }
.blog-title { font-weight: 700; font-size: 0.85rem; margin: 0; line-height: 1.3; color: #111; }
</style>