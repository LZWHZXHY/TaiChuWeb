<template>
  <section class="view-section">
    <div class="section-header">
      <h2>INITIATE_CREATION // 创造矩阵</h2>
      <p class="sub-text">选择要接入的数据类型以开始构建</p>
    </div>

    <div class="type-selector">
      <div class="type-card" :class="{ active: currentType === 'post' }" @click="currentType = 'post'">
        <span class="icon">💬</span>
        <span class="title">动态帖子 // POST</span>
        <span class="desc">发布轻量级的短讯或交流讨论</span>
      </div>
      <div class="type-card" :class="{ active: currentType === 'art' }" @click="currentType = 'art'">
        <span class="icon">🎨</span>
        <span class="title">视觉绘画 // ART</span>
        <span class="desc">上传单张或多张原创图像作品</span>
      </div>
      <div class="type-card" :class="{ active: currentType === 'blog' }" @click="currentType = 'blog'">
        <span class="icon">📜</span>
        <span class="title">深度博客 // BLOG</span>
        <span class="desc">撰写富文本长篇文章与教程</span>
      </div>
    </div>

    <div class="form-workspace">
      <div class="workspace-header">
        <span class="indicator"></span>
        CURRENT_MODULE: {{ currentType.toUpperCase() }}_EDITOR
      </div>

      <PostEditor v-if="currentType === 'post'" />
      <ArtEditor v-else-if="currentType === 'art'" />
      <BlogEditor v-else-if="currentType === 'blog'" />

    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import PostEditor from './components/PostEditor.vue'
import ArtEditor from './components/ArtEditor.vue'
import BlogEditor from './components/BlogEditor.vue'

const currentType = ref('post')
</script>

<style scoped>
/* 这里只保留外层布局和类型选择器的样式 */
.view-section { position: relative; z-index: 1; max-width: 100%; margin: 0 auto; }
.section-header { margin-bottom: 30px; border-bottom: 3px solid #111; padding-bottom: 10px; }
.section-header h2 { font-family: 'Anton'; font-size: 2rem; margin: 0; }
.sub-text { font-weight: bold; color: #666; margin-top: 5px; font-size: 0.9rem; }

.type-selector { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 30px; }
.type-card { background: #fff; border: 2px solid #111; padding: 20px; cursor: pointer; transition: 0.2s; display: flex; flex-direction: column; align-items: flex-start; }
.type-card:hover { transform: translateY(-3px); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.type-card.active { background: #111; color: #fff; border-color: #111; box-shadow: 6px 6px 0 #D92323; transform: translateY(-3px); }

.type-card .icon { font-size: 2rem; margin-bottom: 10px; }
.type-card .title { font-family: 'Anton'; font-size: 1.2rem; margin-bottom: 5px; }
.type-card .desc { font-size: 0.75rem; color: #888; line-height: 1.4; font-weight: bold; }
.type-card.active .desc { color: #ccc; }

.form-workspace { background: #fff; border: 2px solid #111; padding: 30px; position: relative; }
.workspace-header { font-family: 'Anton'; font-size: 0.9rem; color: #111; margin-bottom: 25px; display: flex; align-items: center; gap: 10px; border-bottom: 1px dashed #ccc; padding-bottom: 10px; }
.indicator { width: 10px; height: 10px; background: #2ecc71; border-radius: 50%; display: inline-block; box-shadow: 0 0 5px #2ecc71; }
</style>