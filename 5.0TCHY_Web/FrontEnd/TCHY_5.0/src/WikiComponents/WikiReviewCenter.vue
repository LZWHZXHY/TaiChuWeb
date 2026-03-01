<template>
  <div class="review-center">
    <header class="review-header">
      <div class="header-title">
        <h2>🛡️ 寰宇审核控制台</h2>
        <span class="review-count" v-if="reviews && reviews.length">{{ reviews.length }}</span>
      </div>
      <button class="btn-exit" @click="$emit('exit')">
        <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
        退出审核
      </button>
    </header>
    
    <div v-if="!reviews || reviews.length === 0" class="empty-state">
      <div class="empty-icon">✨</div>
      <p>暂无待审核请求，喝杯咖啡休息一下吧</p>
    </div>

    <div v-else class="review-list">
      <div v-for="review in reviews" :key="review.Id || review.id" class="review-card">
        
        <div class="card-header">
          <div class="header-info">
            <span class="user-badge">👤 {{ review.AuthorName || review.authorName }}</span>
            <span class="action-text">提交了对</span>
            <span class="article-link">《{{ review.ArticleTitle || review.articleTitle }}》</span>
            <span class="action-text">的修改</span>
          </div>
          <span class="time">{{ review.CreatedAt || review.createdAt }}</span>
        </div>

        <div class="card-body">
          
          <div class="meta-panel">
           <div class="info-group">
  <div class="info-item">
    <label>词条标题</label>
    <div class="val title-val">{{ review.Title || review.title }}</div>
  </div>

  <div class="info-item">
    <label>所属分类变动</label>
    <div class="category-diff-box">
      <template v-if="review.oldCategoryName === review.newCategoryName">
        <span class="cat-tag">{{ review.newCategoryName }}</span>
      </template>
      <template v-else>
        <span class="cat-tag old">{{ review.oldCategoryName }}</span>
        <span class="cat-arrow">➜</span>
        <span class="cat-tag new">{{ review.newCategoryName }}</span>
      </template>
    </div>
  </div>

  <div class="info-item">
    <label>编辑备注</label>
    <div class="val summary">{{ review.EditSummary || review.editSummary || '该用户未留下任何备注...' }}</div>
  </div>
</div>
            
            <div class="btn-group">
              <button class="btn-pass" @click="$emit('action', review.Id || review.id, true)">
                <svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" stroke-width="2.5" fill="none" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                准予发布
              </button>
              <button class="btn-reject" @click="$emit('action', review.Id || review.id, false)">
                <svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" stroke-width="2.5" fill="none" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                打回修改
              </button>
            </div>
          </div>

          <div class="diff-panel">
            <div class="diff-tabs">
              <button :class="{active: !showPreview}" @click="showPreview = false">
                <svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"><polyline points="16 18 22 12 16 6"></polyline><polyline points="8 6 2 12 8 18"></polyline></svg>
                差异对比 (Diff)
              </button>
              <button :class="{active: showPreview}" @click="showPreview = true">
                <svg viewBox="0 0 24 24" width="14" height="14" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                最终预览 (Preview)
              </button>
            </div>
            
            <div class="diff-content-area">
              <div v-if="!showPreview" class="diff-engine">
                <div v-for="(line, idx) in getNativeDiff(review)" 
                     :key="idx"
                     :class="['diff-row', line.type]">
                  <div class="diff-line-num">{{ idx + 1 }}</div>
                  <div class="diff-sign-gutter">{{ line.sign }}</div>
                  <div class="diff-text-content">{{ line.text }}</div>
                </div>
              </div>
              <div v-else class="markdown-body" v-html="renderMarkdown(review)"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { marked } from 'marked'

const props = defineProps({ reviews: Array })
defineEmits(['exit', 'action'])

const showPreview = ref(false)

const getNativeDiff = (review) => {
  const oldText = review.OldContent ?? review.oldContent ?? '';
  const newText = review.NewContent ?? review.newContent ?? '';

  const oldLines = oldText.split('\n');
  const newLines = newText.split('\n');
  const rows = [];
  
  const maxLength = Math.max(oldLines.length, newLines.length);

  for (let i = 0; i < maxLength; i++) {
    const oldLine = oldLines[i];
    const newLine = newLines[i];

    if (oldLine === newLine) {
      if (oldLine !== undefined) {
        rows.push({ type: 'normal', sign: ' ', text: oldLine || ' ' });
      }
    } else {
      if (oldLine !== undefined) {
        rows.push({ type: 'removed', sign: '-', text: oldLine || ' ' });
      }
      if (newLine !== undefined) {
        rows.push({ type: 'added', sign: '+', text: newLine || ' ' });
      }
    }
  }
  return rows;
}

const renderMarkdown = (review) => {
  return marked.parse(review.NewContent || review.newContent || '');
}
</script>

<style scoped>
/* 🎯 基础布局与背景 */
.review-center { flex: 1; overflow-y: auto; padding: 40px; background: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }

/* 🛡️ 头部导航 */
.review-header { display: flex; justify-content: space-between; align-items: center; max-width: 1200px; margin: 0 auto 32px auto; }
.header-title { display: flex; align-items: center; gap: 12px; }
.header-title h2 { margin: 0; font-size: 26px; font-weight: 800; color: #0f172a; letter-spacing: -0.5px; }
.review-count { background: #ef4444; color: white; padding: 2px 10px; border-radius: 20px; font-size: 13px; font-weight: bold; box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3); }

/* 🔙 退出按钮 */
.btn-exit { display: flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #cbd5e1; background: white; border-radius: 8px; cursor: pointer; color: #475569; font-weight: 600; transition: all 0.2s ease; box-shadow: 0 1px 2px rgba(0,0,0,0.05); }
.btn-exit:hover { background: #f8fafc; color: #0f172a; border-color: #94a3b8; transform: translateY(-1px); }

/* 📭 空状态 */
.empty-state { text-align: center; padding: 100px 20px; color: #64748b; background: white; border-radius: 16px; border: 1px dashed #cbd5e1; max-width: 600px; margin: 0 auto; }
.empty-icon { font-size: 48px; margin-bottom: 16px; }
.empty-state p { font-size: 16px; font-weight: 500; }

/* 📝 审核列表与卡片 */
.review-list { display: flex; flex-direction: column; gap: 32px; max-width: 1200px; margin: 0 auto; }
.review-card { background: white; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; display: flex; flex-direction: column; box-shadow: 0 10px 25px -5px rgba(0,0,0,0.05), 0 8px 10px -6px rgba(0,0,0,0.01); transition: box-shadow 0.3s ease; }
.review-card:hover { box-shadow: 0 20px 25px -5px rgba(0,0,0,0.05), 0 8px 10px -6px rgba(0,0,0,0.01); }

/* 💳 卡片头部 */
.card-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 24px; background: #ffffff; border-bottom: 1px solid #e2e8f0; }
.header-info { display: flex; align-items: center; gap: 8px; font-size: 15px; color: #475569; }
.user-badge { font-weight: 700; color: #0f172a; background: #f1f5f9; padding: 4px 10px; border-radius: 6px; font-size: 13px; }
.action-text { font-size: 14px; }
.article-link { color: #2563eb; font-weight: 700; cursor: pointer; transition: color 0.2s; }
.article-link:hover { color: #1d4ed8; text-decoration: underline; }
.time { color: #94a3b8; font-size: 13px; font-weight: 500; }

/* 📦 卡片主体网格 */
.card-body { display: grid; grid-template-columns: 280px 1fr; min-height: 500px; }

/* 👈 左侧操作面板 */
.meta-panel { padding: 24px; border-right: 1px solid #e2e8f0; background: #f8fafc; display: flex; flex-direction: column; justify-content: space-between; }
.info-group { display: flex; flex-direction: column; gap: 20px; }
.info-item label { color: #64748b; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; display: block; margin-bottom: 8px; }
.title-val { font-size: 16px; color: #0f172a; font-weight: 700; line-height: 1.4; }
.val.summary { background: #ffffff; padding: 12px 14px; border-radius: 8px; font-size: 13.5px; color: #334155; border: 1px solid #e2e8f0; line-height: 1.6; box-shadow: inset 0 2px 4px 0 rgba(0, 0, 0, 0.02); }

/* 🎯 核心按钮组 */
.btn-group { display: flex; flex-direction: column; gap: 12px; margin-top: 32px; }
.btn-pass, .btn-reject { display: flex; justify-content: center; align-items: center; gap: 8px; width: 100%; padding: 12px; border-radius: 10px; font-size: 15px; font-weight: 700; cursor: pointer; transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1); }

/* 🟢 通过按钮 */
.btn-pass { background: #10b981; color: white; border: none; box-shadow: 0 4px 6px -1px rgba(16, 185, 129, 0.3); }
.btn-pass:hover { background: #059669; box-shadow: 0 6px 12px -2px rgba(16, 185, 129, 0.4); transform: translateY(-2px); }
.btn-pass:active { transform: translateY(0); }

/* 🔴 拒绝按钮 */
.btn-reject { background: white; color: #ef4444; border: 1px solid #fca5a5; }
.btn-reject:hover { background: #fef2f2; border-color: #ef4444; transform: translateY(-2px); box-shadow: 0 4px 6px -1px rgba(239, 68, 68, 0.1); }
.btn-reject:active { transform: translateY(0); }

/* 👉 右侧 Diff 面板 */
.diff-panel { display: flex; flex-direction: column; background: #ffffff; }

/* 📑 视窗切换 Tabs */
.diff-tabs { display: flex; background: #ffffff; border-bottom: 1px solid #e2e8f0; padding: 0 16px; }
.diff-tabs button { display: flex; align-items: center; gap: 6px; padding: 14px 20px; border: none; cursor: pointer; background: none; color: #64748b; font-weight: 600; font-size: 14px; border-bottom: 3px solid transparent; transition: all 0.2s ease; margin-bottom: -1px; }
.diff-tabs button:hover { color: #0f172a; }
.diff-tabs button.active { color: #2563eb; border-bottom-color: #2563eb; }

/* 💻 Diff 渲染区 */
.diff-content-area { flex: 1; overflow-y: auto; height: 500px; background: #ffffff; }
.diff-engine { font-family: 'JetBrains Mono', 'Fira Code', Consolas, monospace; font-size: 13.5px; line-height: 1.6; padding: 16px 0; }

.diff-row { display: flex; width: 100%; white-space: pre-wrap; word-break: break-all; }
.diff-row:hover { background-color: rgba(0,0,0,0.02); }

/* 左侧行号与符号槽 */
.diff-line-num { width: 45px; text-align: right; padding-right: 12px; color: #cbd5e1; font-size: 11.5px; background: #ffffff; user-select: none; flex-shrink: 0; padding-top: 2px; }
.diff-sign-gutter { width: 30px; text-align: center; flex-shrink: 0; font-weight: 800; font-size: 15px; margin-right: 8px; color: #94a3b8; }

/* 红绿高亮状态 */
.added { background-color: #e6ffec !important; color: #166534; }
.added .diff-sign-gutter { color: #22863a; }
.added .diff-line-num { background-color: #f0fdf4; color: #86efac; }

.removed { background-color: #ffeef0 !important; color: #b31d28; }
.removed .diff-sign-gutter { color: #cb2431; }
.removed .diff-line-num { background-color: #fef2f2; color: #fca5a5; }
.removed .diff-text-content { text-decoration: line-through; opacity: 0.85; }

.diff-text-content { flex: 1; padding: 0 8px 0 0; }
.markdown-body { padding: 32px; }

/* 🚀 分类变动盒子 */
.category-diff-box {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  background: #ffffff;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.cat-tag {
  font-size: 12px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 4px;
  background: #f1f5f9;
  color: #475569;
}

/* 旧分类：红色/灰色调 */
.cat-tag.old {
  background: #fff1f2;
  color: #e11d48;
  text-decoration: line-through;
  opacity: 0.8;
}

/* 新分类：绿色调 */
.cat-tag.new {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
}

.cat-arrow {
  color: #94a3b8;
  font-size: 14px;
  font-weight: bold;
}
</style>