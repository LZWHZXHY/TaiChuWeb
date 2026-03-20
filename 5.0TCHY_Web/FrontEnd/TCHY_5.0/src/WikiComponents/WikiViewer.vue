<template>
  <main class="main-content">
    <div class="blog-drawer" :class="{ 'drawer-open': isDrawerOpen }">
      <div class="drawer-header">
        <div class="drawer-title">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path></svg>
          博客正文预览
        </div>
        <button class="close-drawer-btn" @click="isDrawerOpen = false">✕</button>
      </div>
      <div class="drawer-body markdown-body" v-if="drawerLoading">
        <div class="loading-placeholder">正在加载宇宙中的文字...</div>
      </div>
      <div class="drawer-body markdown-body" v-else v-html="drawerContentHtml"></div>
    </div>

    <div class="content-wrapper" v-if="article && article.id && Array.isArray(article.blocks)">
      
      <nav class="breadcrumbs" v-if="article.breadcrumbs">
        <span v-for="(crumb, index) in article.breadcrumbs" :key="index">
          <span class="crumb-text">{{ crumb }}</span>
          <span v-if="index < article.breadcrumbs.length - 1" class="separator">/</span>
        </span>
      </nav>
      
      <header class="article-header">
        <h1 class="title">{{ article.title || '无标题' }}</h1>
        <div class="metadata">
          <div class="meta-left">
            <router-link 
              v-if="article.creator" 
              :to="`/profile/${article.creator.id}`" 
              class="meta-item creator-link"
              title="词条初创者"
            >
              👑 {{ article.creator.name }} 建立
            </router-link>

            <span class="meta-item">📦 共 {{ article.blocks.length }} 段</span>
            <span class="meta-item" v-if="article.lastUpdated">📅 {{ article.lastUpdated }}</span>

            <button class="meta-item btn-contributors" @click="showContributorsModal = true">
              👥 {{ allContributors.length }} 位贡献者
            </button>
          </div>
          <div class="meta-actions">
            <button class="edit-link" @click="$emit('edit')">编辑全文</button>
            <button v-if="isAdmin" class="btn-delete-article" @click="$emit('delete-article', article.id)">删除</button>
          </div>
        </div>
      </header>

      <div class="article-body" @click="handleBodyClick">
        <div 
          v-for="block in article.blocks" 
          :key="block.id || block.Id" 
          class="content-block-wrapper"
        >
          <div class="block-text markdown-body" v-html="renderMarkdown(block.content || block.Content)"></div>
          
          <div class="block-signature">
            <div class="sig-content">
              <template v-if="getContributors(block).length > 0">
                <div class="contributors-group">
                  <router-link 
                    :to="`/profile/${getLatestEditor(block).id}`" 
                    class="user-link primary-editor"
                  >
                    👤 {{ getLatestEditor(block).name }}
                  </router-link>
                  
                  <span v-if="getContributors(block).length > 1" class="co-editors-badge">
                    等 {{ getContributors(block).length }} 人贡献
                    <div class="contributors-tooltip">
                      <div class="tooltip-title">区块贡献者：</div>
                      <router-link 
                        v-for="(user, idx) in getContributors(block)" 
                        :key="user.id + '-' + idx"
                        :to="`/profile/${user.id}`"
                        class="tooltip-user-link"
                      >
                        {{ user.name }}
                      </router-link>
                    </div>
                  </span>
                </div>
              </template>

              <template v-else>
                <router-link 
                  :to="`/profile/${block.lastEditorId || block.LastEditorId}`" 
                  class="user-link"
                >
                  👤 {{ block.lastEditor || block.LastEditor || '系统' }}
                </router-link>
              </template>

              <span class="time">{{ block.updatedAt || block.UpdatedAt || '刚刚' }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="loading-state">
      <div class="spinner"></div>
      <p>正在同步百科数据...</p>
    </div>

    <div class="modal-overlay" v-if="showContributorsModal" @click.self="showContributorsModal = false">
      <div class="contributors-modal">
        <div class="modal-header">
          <h3>👥 词条贡献者 (共 {{ allContributors.length }} 人)</h3>
          <button class="close-btn" @click="showContributorsModal = false">✕</button>
        </div>
        
        <div class="modal-body">
          <div class="contributor-list">
            <router-link 
              v-for="user in allContributors" 
              :key="user.id" 
              :to="`/profile/${user.id}`"
              class="contributor-card"
            >
              <div class="avatar-container">
                <UserAvatar 
                  :userId="user.id" 
                  :allowLink="false" 
                  :showLevel="false" 
                />
              </div>
              
              <div class="user-info">
                <span class="name">{{ user.name }}</span>
                <span class="id">UID: {{ user.id }}</span>
              </div>
            </router-link>
          </div>
        </div>
      </div>
    </div>

  </main>
</template>

<script setup>
import { ref, computed } from 'vue';
import { marked } from 'marked';
import UserAvatar from '@/GeneralComponents/UserAvatar.vue';
import apiClient from '@/utils/api'; // 🚀 需要引入 api 才能拉取博客详情

const props = defineProps({ article: Object, isAdmin: Boolean });
const emit = defineEmits(['edit', 'delete-article', 'wiki-link-click']);

const showContributorsModal = ref(false);

// 🚀 3. 新增抽屉组件需要的状态变量
const isDrawerOpen = ref(false);
const drawerLoading = ref(false);
const drawerContentHtml = ref('');

const allContributors = computed(() => {
  if (!props.article || !props.article.blocks) return [];
  const uniqueUsers = new Map();
  props.article.blocks.forEach(block => {
    const list = block.contributors || block.Contributors || [];
    list.forEach(user => {
      if (!uniqueUsers.has(user.id)) uniqueUsers.set(user.id, user);
    });
    const lastEditorId = block.lastEditorId || block.LastEditorId;
    const lastEditorName = block.lastEditor || block.LastEditor;
    if (lastEditorId && lastEditorName && !uniqueUsers.has(lastEditorId)) {
      uniqueUsers.set(lastEditorId, { id: lastEditorId, name: lastEditorName });
    }
  });
  return Array.from(uniqueUsers.values());
});

const getContributors = (block) => {
  const list = block.Contributors || block.contributors;
  if (Array.isArray(list)) return list;
  return [];
};

const getLatestEditor = (block) => {
  const list = getContributors(block);
  if (list.length > 0) return list[list.length - 1]; 
  return null;
};

// 🚀 4. 在 renderMarkdown 中补齐对博客引用语法的正则替换
const renderMarkdown = (rawText) => {
  if (!rawText || typeof rawText !== 'string') return '';

  try {
    // 替换百科内部链接
    let processedText = rawText.replace(/\[\[([^\]\s][^\]]*?)\]\]/g, (match, title) => {
      if (!title) return '';
      return `<a class="wiki-link" href="javascript:void(0)" data-wiki-title="${title}">${title}</a>`;
    });

    // 🚀 核心修复：把 HTML 写成一行，或者去掉前面的空格缩进，防止被当成 Markdown 代码块
    processedText = processedText.replace(/:::blog (\d+)\n([\s\S]*?):::/g, (match, id, title) => {
      return `<div class="blog-embed-card" data-blog-id="${id}"><div class="card-left">📖</div><div class="card-center"><div class="card-title">${title.trim()}</div><div class="card-subtitle">点击展开关联博客</div></div><div class="card-right">查看全文</div></div>`;
    });

    const renderer = new marked.Renderer();
    
    renderer.link = ({ href, title, text }) => {
      const safeHref = href || '#';
      const isExternal = /^https?:\/\//.test(safeHref);
      
      if (isExternal) {
        return `<a href="${safeHref}" 
                   class="external-link" 
                   target="_blank" 
                   rel="noopener noreferrer" 
                   title="${title || text || '外部链接'}">
                  ${text}<svg class="icon-external" viewBox="0 0 24 24" width="13" height="13" style="margin-left:3px; vertical-align: middle; display: inline-block;"><path fill="currentColor" d="M14,3V5H17.59L7.76,14.83L9.17,16.24L19,6.41V10H21V3M19,19H5V5H12V3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V12H19V19Z"/></svg>
                </a>`;
      }
      return `<a href="${safeHref}" title="${title || ''}">${text}</a>`;
    };

    return marked.parse(processedText, { renderer });
  } catch (e) {
    console.error("Markdown 渲染异常:", e);
    return rawText;
  }
};

// 🚀 5. 统一的点击事件分发，处理 Wiki 链接 和 博客卡片点击
const handleBodyClick = async (event) => {
  const target = event.target;
  
  // 处理百科内部链接
  if (target && target.classList.contains('wiki-link')) {
    const title = target.getAttribute('data-wiki-title');
    if (title) emit('wiki-link-click', title);
    return;
  }

  // 处理博客卡片点击，弹出抽屉
  const card = target.closest('.blog-embed-card');
  if (card) {
    const blogId = card.dataset.blogId;
    isDrawerOpen.value = true;
    drawerLoading.value = true;
    try {
      const res = await apiClient.get(`/Blog/articles/${blogId}`);
      drawerContentHtml.value = marked.parse(res.data.content || '该文章暂无内容');
    } catch (err) {
      drawerContentHtml.value = '<p class="error-msg">内容获取失败，请确认文章是否已发布。</p>';
    } finally {
      drawerLoading.value = false;
    }
  }
};
</script>

<style scoped>
/* ==========================================================
   1. 基础布局与滚动条 (百科专业版)
========================================================== */
.main-content { 
  flex: 1; height: 100%; min-height: 0;
  overflow-y: auto; overflow-x: hidden;
  background-color: #ffffff; 
  display: flex; justify-content: center;
  scroll-behavior: smooth;
  position: relative;
}

.main-content::-webkit-scrollbar { width: 6px; }
.main-content::-webkit-scrollbar-track { background: transparent; }
.main-content::-webkit-scrollbar-thumb {
  background-color: #e2e8f0;
  border-radius: 10px;
}
.main-content::-webkit-scrollbar-thumb:hover { background-color: #cbd5e1; }

.content-wrapper { 
  width: 88%; 
  max-width: 960px;
  padding: 30px 24px 180px 24px; 
  box-sizing: border-box;
}

/* ==========================================================
   2. 抽屉式侧边栏样式 (博客预览专用)
========================================================== */
.blog-drawer {
  position: fixed; right: -600px; top: 0; width: 550px; height: 100vh;
  background: white; box-shadow: -10px 0 30px rgba(0,0,0,0.1);
  z-index: 2000; transition: right 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex; flex-direction: column;
}
.blog-drawer.drawer-open { right: 0; }
.drawer-header {
  padding: 20px 24px; border-bottom: 1px solid #f1f5f9;
  display: flex; justify-content: space-between; align-items: center; background: #fff;
}
.drawer-title { display: flex; align-items: center; gap: 8px; font-weight: 700; color: #1e293b; font-size: 16px; }
.close-drawer-btn { border: none; background: none; font-size: 22px; cursor: pointer; color: #94a3b8; transition: color 0.2s; }
.close-drawer-btn:hover { color: #ef4444; }
.drawer-body { flex: 1; overflow-y: auto; padding: 32px; line-height: 1.8; }
.loading-placeholder { text-align: center; color: #94a3b8; margin-top: 100px; }

/* 博客嵌入卡片 (文章正文中的卡片) */
:deep(.blog-embed-card) {
  display: flex; align-items: center; gap: 16px; padding: 18px 24px;
  background: #ffffff; border: 1px solid #e2e8f0; border-left: 4px solid #10b981;
  border-radius: 12px; margin: 24px 0; cursor: pointer; transition: all 0.2s;
}
:deep(.blog-embed-card:hover) { 
  transform: translateX(5px); box-shadow: 0 4px 20px rgba(16, 185, 129, 0.1); border-color: #10b981; 
}
:deep(.card-left) { font-size: 24px; }
:deep(.card-center) { flex: 1; }
:deep(.card-title) { font-weight: 700; color: #0f172a; font-size: 16px; margin-bottom: 4px; }
:deep(.card-subtitle) { font-size: 12px; color: #64748b; }
:deep(.card-right) { font-size: 12px; font-weight: 600; color: #059669; background: #f0fdf4; padding: 4px 12px; border-radius: 20px; }

/* 以下为你原有的页头、内容块、弹窗等全部保留样式 */
.breadcrumbs { font-size: 13px; color: #94a3b8; margin-bottom: 16px; display: flex; align-items: center; }
.crumb-text { cursor: pointer; transition: color 0.2s; }
.crumb-text:hover { color: #2563eb; }
.separator { margin: 0 8px; color: #e2e8f0; }
.article-header { margin-bottom: 30px; padding-bottom: 16px; border-bottom: 1px solid #f1f5f9; }
.title { font-size: 38px; font-weight: 850; color: #0f172a; margin: 0 0 16px 0; letter-spacing: -0.02em; }
.metadata { display: flex; align-items: center; justify-content: space-between; font-size: 13.5px; color: #64748b; }
.meta-left { display: flex; gap: 16px; }
.meta-item { display: flex; align-items: center; gap: 4px; }
.meta-actions { display: flex; gap: 10px; }
.edit-link, .btn-delete-article { display: flex; align-items: center; gap: 4px; padding: 5px 12px; border-radius: 6px; font-weight: 600; font-size: 13px; cursor: pointer; transition: all 0.2s; }
.edit-link { background: #f8faff; border: 1px solid #e0e7ff; color: #4f46e5; }
.edit-link:hover { background: #e0e7ff; }
.btn-delete-article { background: #fffafa; border: 1px solid #fee2e2; color: #ef4444; }
.btn-delete-article:hover { background: #fee2e2; }
.article-body { display: flex; flex-direction: column; gap: 0; padding-bottom: 50px; }
.content-block-wrapper { position: relative; padding: 4px 20px; border-left: 3px solid transparent; transition: all 0.2s ease; }
.content-block-wrapper:hover { background-color: #f8fafc; border-left-color: #3b82f6; }
.markdown-body { color: #1e293b; font-size: 15.8px; line-height: 1.7; word-wrap: break-word; }
.markdown-body :deep(p) { margin-top: 0; margin-bottom: 0.85em; }
.markdown-body :deep(> *:last-child) { margin-bottom: 0 !important; }
.markdown-body :deep(h1), .markdown-body :deep(h2), .markdown-body :deep(h3) { color: #0f172a; font-weight: 700; margin: 1.5em 0 0.6em; line-height: 1.3; }
.markdown-body :deep(h1) { font-size: 2em; border-bottom: 1px solid #f1f5f9; padding-bottom: 0.3em; }
.markdown-body :deep(h2) { font-size: 1.5em; border-bottom: 1px solid #f1f5f9; padding-bottom: 0.3em; }
.markdown-body :deep(h3) { font-size: 1.25em; }
.markdown-body :deep(blockquote) { margin: 1.2em 0; padding: 10px 20px; border-left: 4px solid #cbd5e1; background: #f8fafc; border-radius: 0 6px 6px 0; color: #475569; }
.markdown-body :deep(ul), .markdown-body :deep(ol) { padding-left: 1.5em; margin-bottom: 1em; }
.markdown-body :deep(li) { margin-bottom: 0.3em; }
.markdown-body :deep(code) { background: #f1f5f9; color: #db2777; padding: 0.1em 0.3em; border-radius: 4px; font-size: 0.9em; }
.markdown-body :deep(pre) { background: #0f172a; padding: 16px; border-radius: 8px; overflow-x: auto; margin: 1.2em 0; }
.markdown-body :deep(pre code) { background: transparent; color: #f8fafc; }
.markdown-body :deep(img) { max-width: 100%; height: auto; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); margin: 1em 0; }
:deep(.wiki-link) { color: #2563eb; font-weight: 600; text-decoration: none; border-bottom: 1.5px dashed #bfdbfe; transition: all 0.2s; cursor: pointer; padding: 0 2px; }
:deep(.wiki-link:hover) { color: #1d4ed8; background-color: #eff6ff; border-bottom-style: solid; }
:deep(.external-link) { color: #2563eb; text-decoration: none; border-bottom: 1px solid rgba(37, 99, 235, 0.2); transition: all 0.2s ease; padding: 0 2px; }
:deep(.external-link:hover) { color: #1d4ed8; background-color: rgba(37, 99, 235, 0.05); border-bottom-color: #1d4ed8; }
:deep(.icon-external) { color: #94a3b8; transition: color 0.2s; }
:deep(.external-link:hover .icon-external) { color: #1d4ed8; }
.block-signature { position: absolute; right: 10px; top: 4px; opacity: 0; transform: scale(0.9); transition: all 0.25s cubic-bezier(0.175, 0.885, 0.32, 1.275); z-index: 10; pointer-events: none; }
.content-block-wrapper:hover .block-signature { opacity: 1; transform: scale(1); pointer-events: auto; }
.sig-content { background: rgba(255, 255, 255, 0.98); border: 1px solid #e2e8f0; padding: 4px 12px; border-radius: 16px; box-shadow: 0 4px 10px rgba(0,0,0,0.06); display: flex; align-items: center; gap: 8px; font-size: 11.5px; }
.user-link { font-weight: 700; color: #64748b; text-decoration: none; transition: color 0.2s; }
.user-link:hover { color: #2563eb; }
.sig-content .time { color: #94a3b8; font-weight: normal; }
.loading-state { height: 60vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #94a3b8; }
.spinner { width: 32px; height: 32px; border: 3px solid #f1f5f9; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
@keyframes spin { to { transform: rotate(360deg); } }
.contributors-group { position: relative; display: flex; align-items: center; gap: 4px; }
.co-editors-badge { color: #64748b; font-size: 11px; background: #f1f5f9; padding: 2px 6px; border-radius: 10px; cursor: help; position: relative; transition: all 0.2s; display: inline-flex; align-items: center; }
.co-editors-badge:hover { background: #e2e8f0; color: #0f172a; }
.contributors-tooltip { position: absolute; bottom: 120%; left: 50%; transform: translateX(-50%) translateY(5px); background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1); padding: 8px; border-radius: 8px; display: flex; flex-direction: column; gap: 4px; min-width: 120px; opacity: 0; visibility: hidden; transition: all 0.2s ease; z-index: 20; }
.co-editors-badge:hover .contributors-tooltip { opacity: 1; visibility: visible; transform: translateX(-50%) translateY(0); }
.tooltip-title { font-size: 10px; color: #94a3b8; margin-bottom: 4px; border-bottom: 1px dashed #e2e8f0; padding-bottom: 4px; }
.tooltip-user-link { color: #3b82f6; text-decoration: none; font-size: 12px; font-weight: 600; padding: 4px 6px; border-radius: 4px; transition: background 0.2s; white-space: nowrap; }
.tooltip-user-link:hover { background: #eff6ff; color: #1d4ed8; }
.btn-contributors { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 4px 10px; cursor: pointer; transition: all 0.2s; font-family: inherit; font-size: 13px; color: #475569; }
.btn-contributors:hover { background: #eff6ff; border-color: #bfdbfe; color: #2563eb; }
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(15, 23, 42, 0.5); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 999; }
.contributors-modal { background: #ffffff; width: 540px; max-width: 90vw; border-radius: 12px; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04); overflow: hidden; animation: modal-pop 0.3s cubic-bezier(0.16, 1, 0.3, 1); }
@keyframes modal-pop { 0% { opacity: 0; transform: scale(0.95) translateY(10px); } 100% { opacity: 1; transform: scale(1) translateY(0); } }
.modal-header { padding: 18px 24px; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; background: #f8fafc; }
.modal-header h3 { margin: 0; font-size: 16px; color: #0f172a; font-weight: 700; }
.close-btn { background: transparent; border: none; font-size: 20px; color: #94a3b8; cursor: pointer; transition: color 0.2s; padding: 0; }
.close-btn:hover { color: #ef4444; }
.modal-body { padding: 24px; max-height: 55vh; overflow-y: auto; }
.modal-body::-webkit-scrollbar { width: 6px; }
.modal-body::-webkit-scrollbar-thumb { background-color: #cbd5e1; border-radius: 10px; }
.contributor-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: 16px; }
.contributor-card { display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; padding: 16px 12px; border: 1px solid #e2e8f0; border-radius: 12px; text-decoration: none; transition: all 0.2s ease; background: #ffffff; text-align: center; }
.contributor-card:hover { border-color: #3b82f6; background: #eff6ff; transform: translateY(-4px); box-shadow: 0 8px 16px rgba(37, 99, 235, 0.08); }
.avatar-container { width: 56px; height: 56px; flex-shrink: 0; }
.user-info { display: flex; flex-direction: column; align-items: center; gap: 6px; width: 100%; }
.user-info .name { font-weight: 700; color: #1e293b; font-size: 13.5px; width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.user-info .id { font-size: 11px; color: #64748b; font-family: monospace; background: #f1f5f9; padding: 2px 8px; border-radius: 10px; }
.creator-link { color: #d97706; font-weight: 600; text-decoration: none; background: #fef3c7; padding: 2px 8px; border-radius: 6px; transition: all 0.2s; }
.creator-link:hover { background: #fde68a; color: #b45309; transform: translateY(-1px); }
</style>