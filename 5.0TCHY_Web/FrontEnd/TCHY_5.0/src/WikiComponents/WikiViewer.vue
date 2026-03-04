<template>
  <main class="main-content">
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
            <span class="meta-item">📦 共 {{ article.blocks.length }} 段</span>
            <span class="meta-item" v-if="article.lastUpdated">📅 {{ article.lastUpdated }}</span>
          </div>
          <div class="meta-actions">
            <button class="edit-link" @click="$emit('edit')">编辑全文</button>
            <button v-if="isAdmin" class="btn-delete-article" @click="$emit('delete-article', article.id)">删除</button>
          </div>
        </div>
      </header>

      <div class="article-body" @click="handleWikiLinkClick">
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
  </main>
</template>

<script setup>
import { marked } from 'marked';

const props = defineProps({ article: Object, isAdmin: Boolean });
const emit = defineEmits(['edit', 'delete-article', 'wiki-link-click']);


// 更加暴力的获取方法，防止解析异常
const getContributors = (block) => {
  const list = block.Contributors || block.contributors;
  // 强制确保它是一个数组
  if (Array.isArray(list)) return list;
  return [];
};
const getLatestEditor = (block) => {
  const list = getContributors(block);
  if (list.length > 0) {
    return list[list.length - 1]; 
  }
  return null;
};






/**
 * 🚀 极致安全的渲染逻辑
 */
const renderMarkdown = (rawText) => {
  if (typeof rawText !== 'string' || !rawText) return '';

  try {
    // 1. 预处理 Wiki 内部链接 [[title]]
    let processedText = rawText.replace(/\[\[([^\]\s][^\]]*?)\]\]/g, (match, title) => {
      return `<a class="wiki-link" href="javascript:void(0)" data-wiki-title="${title}">${title}</a>`;
    });

    // 2. 配置 Marked 自定义链接渲染
    const renderer = new marked.Renderer();
    
    renderer.link = (href, title, text) => {
      // 判断是否为外部链接 (http 或 https 开头)
      const isExternal = /^https?:\/\//.test(href);
      
      if (isExternal) {
        // 外部链接：新窗口打开 + 外部图标
        return `<a href="${href}" 
                   class="external-link" 
                   target="_blank" 
                   rel="noopener noreferrer" 
                   title="${title || href}">
                  ${text}<svg class="icon-external" viewBox="0 0 24 24" width="13" height="13" style="margin-left:3px; vertical-align: middle; display: inline-block;"><path fill="currentColor" d="M14,3V5H17.59L7.76,14.83L9.17,16.24L19,6.41V10H21V3M19,19H5V5H12V3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V12H19V19Z"/></svg>
                </a>`;
      }
      
      // 内部链接（相对路径等）：原样渲染
      return `<a href="${href}" title="${title || ''}">${text}</a>`;
    };

    return marked.parse(processedText, { renderer });
  } catch (e) {
    console.error("Markdown 渲染异常:", e);
    return rawText;
  }
};

const handleWikiLinkClick = (event) => {
  const target = event.target;
  if (target && target.classList.contains('wiki-link')) {
    const title = target.getAttribute('data-wiki-title');
    if (title) emit('wiki-link-click', title);
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
  padding: 30px 24px 180px 24px; /* 🚀 底部增加 180px 留白，防止内容贴地 */
  box-sizing: border-box;
}

/* ==========================================================
   2. 面包屑与页头美化
========================================================== */
.breadcrumbs { 
  font-size: 13px; color: #94a3b8; 
  margin-bottom: 16px; display: flex; align-items: center;
}
.crumb-text { cursor: pointer; transition: color 0.2s; }
.crumb-text:hover { color: #2563eb; }
.separator { margin: 0 8px; color: #e2e8f0; }

.article-header { 
  margin-bottom: 30px; padding-bottom: 16px; border-bottom: 1px solid #f1f5f9; 
}
.title { 
  font-size: 38px; font-weight: 850; color: #0f172a; 
  margin: 0 0 16px 0; letter-spacing: -0.02em;
}

.metadata { 
  display: flex; align-items: center; justify-content: space-between;
  font-size: 13.5px; color: #64748b; 
}
.meta-left { display: flex; gap: 16px; }
.meta-item { display: flex; align-items: center; gap: 4px; }
.meta-actions { display: flex; gap: 10px; }

/* 功能按钮样式恢复 */
.edit-link, .btn-delete-article {
  display: flex; align-items: center; gap: 4px; 
  padding: 5px 12px; border-radius: 6px; 
  font-weight: 600; font-size: 13px;
  cursor: pointer; transition: all 0.2s;
}
.edit-link { 
  background: #f8faff; border: 1px solid #e0e7ff; color: #4f46e5; 
}
.edit-link:hover { background: #e0e7ff; }

.btn-delete-article { 
  background: #fffafa; border: 1px solid #fee2e2; color: #ef4444; 
}
.btn-delete-article:hover { background: #fee2e2; }

/* ==========================================================
   3. 内容块流式布局 (核心修改)
========================================================== */
.article-body { 
  display: flex; flex-direction: column; 
  gap: 0; /* 🚀 消除物理间隔 */
  padding-bottom: 50px;
}

.content-block-wrapper { 
  position: relative; 
  padding: 4px 20px; /* 🚀 紧凑内边距 */
  border-left: 3px solid transparent; 
  transition: all 0.2s ease;
}
.content-block-wrapper:hover { 
  background-color: #f8fafc; 
  border-left-color: #3b82f6;
}

/* ==========================================================
   4. Markdown 元素细节美化 (重写渲染样式)
========================================================== */
.markdown-body {
  color: #1e293b; font-size: 15.8px; line-height: 1.7; word-wrap: break-word;
}

/* 🚀 段落间距：紧凑化 */
.markdown-body :deep(p) { margin-top: 0; margin-bottom: 0.85em; }

/* 🚀 关键：消除 Block 结尾的空隙 */
.markdown-body :deep(> *:last-child) { margin-bottom: 0 !important; }

/* 标题样式 */
.markdown-body :deep(h1), 
.markdown-body :deep(h2), 
.markdown-body :deep(h3) { 
  color: #0f172a; font-weight: 700; margin: 1.5em 0 0.6em; line-height: 1.3;
}
.markdown-body :deep(h1) { font-size: 2em; border-bottom: 1px solid #f1f5f9; padding-bottom: 0.3em; }
.markdown-body :deep(h2) { font-size: 1.5em; border-bottom: 1px solid #f1f5f9; padding-bottom: 0.3em; }
.markdown-body :deep(h3) { font-size: 1.25em; }

/* 引用样式 */
.markdown-body :deep(blockquote) {
  margin: 1.2em 0; padding: 10px 20px;
  border-left: 4px solid #cbd5e1; background: #f8fafc; border-radius: 0 6px 6px 0;
  color: #475569;
}

/* 列表样式 */
.markdown-body :deep(ul), .markdown-body :deep(ol) { padding-left: 1.5em; margin-bottom: 1em; }
.markdown-body :deep(li) { margin-bottom: 0.3em; }

/* 代码与预格式化 */
.markdown-body :deep(code) {
  background: #f1f5f9; color: #db2777; padding: 0.1em 0.3em; border-radius: 4px; font-size: 0.9em;
}
.markdown-body :deep(pre) {
  background: #0f172a; padding: 16px; border-radius: 8px; overflow-x: auto; margin: 1.2em 0;
}
.markdown-body :deep(pre code) { background: transparent; color: #f8fafc; }

/* 图片 */
.markdown-body :deep(img) {
  max-width: 100%; height: auto; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); margin: 1em 0;
}

/* Wiki 链接 */
:deep(.wiki-link) {
  color: #2563eb; font-weight: 600; text-decoration: none;
  border-bottom: 1.5px dashed #bfdbfe; transition: all 0.2s;
  cursor: pointer; padding: 0 2px;
}
:deep(.wiki-link:hover) {
  color: #1d4ed8; background-color: #eff6ff; border-bottom-style: solid;
}

/* 外部链接基础样式 */
:deep(.external-link) {
  color: #2563eb; /* 品牌蓝 */
  text-decoration: none;
  border-bottom: 1px solid rgba(37, 99, 235, 0.2); /* 浅色的下划线 */
  transition: all 0.2s ease;
  padding: 0 2px;
}

/* 鼠标悬停：下划线加深，颜色变亮 */
:deep(.external-link:hover) {
  color: #1d4ed8;
  background-color: rgba(37, 99, 235, 0.05); /* 淡淡的背景色 */
  border-bottom-color: #1d4ed8;
}

/* 图标颜色跟随文字 */
:deep(.icon-external) {
  color: #94a3b8; /* 默认灰蓝色，不喧宾夺主 */
  transition: color 0.2s;
}

:deep(.external-link:hover .icon-external) {
  color: #1d4ed8;
}

/* ==========================================================
   5. 签名气泡微调
========================================================== */
/* ==========================================================
   5. 签名气泡微调
========================================================== */
.block-signature { 
  position: absolute; right: 10px; top: 4px; 
  opacity: 0; transform: scale(0.9); 
  transition: all 0.25s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  z-index: 10; 
  pointer-events: none; /* 默认隐藏时不可点击，防止遮挡文字 */
}

.content-block-wrapper:hover .block-signature { 
  opacity: 1; 
  transform: scale(1); 
  pointer-events: auto; /* 🚀 核心修复：悬停显示时恢复鼠标点击事件！ */
}

.sig-content { 
  background: rgba(255, 255, 255, 0.98); border: 1px solid #e2e8f0;
  padding: 4px 12px; border-radius: 16px; box-shadow: 0 4px 10px rgba(0,0,0,0.06);
  display: flex; align-items: center; gap: 8px; font-size: 11.5px;
}

/* 🚀 新增：用户链接的样式，让它有交互反馈 */
.user-link { 
  font-weight: 700; 
  color: #64748b; 
  text-decoration: none;
  transition: color 0.2s;
}
.user-link:hover { 
  color: #2563eb; /* 悬停时变成品牌蓝 */
}

.sig-content .time { color: #94a3b8; font-weight: normal; }

/* 加载动画 */
.loading-state { height: 60vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #94a3b8; }
.spinner { width: 32px; height: 32px; border: 3px solid #f1f5f9; border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 12px; }
@keyframes spin { to { transform: rotate(360deg); } }


/* ==========================================================
   6. 多贡献者样式与 Tooltip (气泡菜单)
========================================================== */
.contributors-group {
  position: relative;
  display: flex;
  align-items: center;
  gap: 4px;
}

.co-editors-badge {
  color: #64748b;
  font-size: 11px;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 10px;
  cursor: help;
  position: relative;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
}

.co-editors-badge:hover {
  background: #e2e8f0;
  color: #0f172a;
}

/* 隐藏在徽章里的完整名单（默认透明度为0且不可点击） */
.contributors-tooltip {
  position: absolute;
  bottom: 120%; /* 显示在徽章上方 */
  left: 50%;
  transform: translateX(-50%) translateY(5px);
  background: #ffffff;
  border: 1px solid #e2e8f0;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  padding: 8px;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 120px;
  
  opacity: 0;
  visibility: hidden;
  transition: all 0.2s ease;
  z-index: 20;
}

/* 鼠标悬停时显示名单 */
.co-editors-badge:hover .contributors-tooltip {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(0);
}

.tooltip-title {
  font-size: 10px;
  color: #94a3b8;
  margin-bottom: 4px;
  border-bottom: 1px dashed #e2e8f0;
  padding-bottom: 4px;
}

.tooltip-user-link {
  color: #3b82f6;
  text-decoration: none;
  font-size: 12px;
  font-weight: 600;
  padding: 4px 6px;
  border-radius: 4px;
  transition: background 0.2s;
  white-space: nowrap;
}

.tooltip-user-link:hover {
  background: #eff6ff;
  color: #1d4ed8;
}

</style>