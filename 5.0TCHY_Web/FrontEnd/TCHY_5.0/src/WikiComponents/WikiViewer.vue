<template>
  <main class="main-content">
    <div class="content-wrapper" v-if="article">
      <nav class="breadcrumbs" v-if="article?.breadcrumbs?.length">
        <span v-for="(crumb, i) in article.breadcrumbs" :key="i">
          {{ crumb }} <span v-if="i < article.breadcrumbs.length - 1" class="sep">/</span>
        </span>
      </nav>
      
      <header class="article-header">
        <h1 class="title">{{ article?.title || '未命名词条' }}</h1>
        <div class="metadata" v-if="article?.metadata">
          <span>更新于 {{ article.metadata?.updatedAt }}</span> · 
          <span>作者: {{ article.metadata?.author }}</span> · 
          <button class="edit-link" @click="$emit('edit')">编辑此页</button>
        </div>
      </header>

      <article class="article-body" v-html="article?.contentHtml"></article>
    </div>
  </main>
</template>

<script setup>
defineProps({ article: Object })
defineEmits(['edit'])
</script>

<style scoped>
/* 样式保持完全不变 */
.main-content { flex: 1; overflow-y: auto; display: flex; justify-content: center; padding: 40px 0; background: #fff; }
.content-wrapper { width: 100%; max-width: 800px; padding: 0 32px; }
.breadcrumbs { font-size: 14px; color: #64748b; margin-bottom: 24px; }
.sep { margin: 0 8px; color: #cbd5e1; }
.title { font-size: 2.5rem; margin-bottom: 16px; color: #0f172a; }
.metadata { font-size: 14px; color: #64748b; margin-bottom: 32px; }
.edit-link { background: none; border: none; color: #2563eb; cursor: pointer; }
.article-body { line-height: 1.7; color: #334155; }
</style>