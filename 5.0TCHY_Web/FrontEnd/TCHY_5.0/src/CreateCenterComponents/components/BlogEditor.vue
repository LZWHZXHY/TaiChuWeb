<template>
  <div class="workbench-container">
    
    <aside class="sidebar">
      <div class="sidebar-header">
        <button class="btn-primary-pro" @click="createNewDraft">
          <PlusOutlined /> 新建文章
        </button>
      </div>

      <div class="sidebar-nav">
        <div class="nav-item" :class="{ active: activeTab === 'drafts' }" @click="activeTab = 'drafts'">草稿箱</div>
        <div class="nav-item" :class="{ active: activeTab === 'published' }" @click="activeTab = 'published'">已发布</div>
      </div>

      <div class="article-list-container">
        <a-spin :spinning="listLoading">
          <div 
            v-for="item in filteredList" 
            :key="item.id" 
            :class="['article-card', { active: currentArticleId === item.id }]"
            @click="loadArticle(item.id)"
          >
            <div class="card-content">
              <span class="card-title">{{ item.title || '无标题文章' }}</span>
              <p class="card-summary">{{ item.summary || '暂无摘要描述...' }}</p>
              <span class="card-meta">{{ formatTime(item.updateTime) }}</span>
            </div>
            <div class="card-actions">
              <a-popconfirm title="永久删除？" @confirm.stop="deleteArticle(item.id)">
                <DeleteOutlined class="icon-delete" />
              </a-popconfirm>
            </div>
          </div>
          <a-empty v-if="filteredList.length === 0" description="空" />
        </a-spin>
      </div>
    </aside>

    <main class="editor-area">
      <header class="editor-header">
        <div class="header-left">
          <span class="dot-status" :class="formState.status === 1 ? 'is-published' : 'is-draft'"></span>
          <span class="status-text">{{ formState.status === 1 ? '已发布' : '草稿' }}</span>
          <span class="last-saved" v-if="lastSaved">同步于 {{ lastSaved }}</span>
        </div>
        <div class="header-right">
          <button class="btn-secondary-pro" @click="showPreview = !showPreview">
            <EyeOutlined /> {{ showPreview ? '退出预览' : '实时预览' }}
          </button>
          <button class="btn-secondary-pro" @click="submitArticle(true)" :disabled="draftLoading">保存草稿</button>
          <button class="btn-primary-pro" @click="submitArticle(false)" :disabled="publishLoading">
            <SendOutlined /> 发布
          </button>
        </div>
      </header>

      <div class="editor-body">
        <a-row style="height: 100%">
          <a-col :xs="24" :lg="showPreview ? 12 : 16" class="col-full-height">
            <div class="writing-container">
              <input v-model="formState.title" class="input-title-pro" placeholder="请输入标题..." />
              
              <div class="editor-toolbar-pro">
  <button class="toolbar-tool" title="加粗" @click="applyFormat('**', '**', '粗体文本')">
    <BoldOutlined />
  </button>
  <button class="toolbar-tool" title="斜体" @click="applyFormat('*', '*', '斜体文本')">
    <ItalicOutlined />
  </button>
  <button class="toolbar-tool" title="删除线" @click="applyFormat('~~', '~~', '删除线')">
    <StrikethroughOutlined />
  </button>
  
  <div class="divider-v"></div>

  <button class="toolbar-tool" title="标题" @click="applyFormat('\n## ', '\n', '输入标题')">
    <FontSizeOutlined />
  </button>
  <button class="toolbar-tool" title="引用" @click="applyFormat('\n> ', '\n', '引用内容')">
    <AlignLeftOutlined />
  </button>
  <button class="toolbar-tool" title="代码块" @click="applyFormat('\n```\n', '\n```\n', '在此输入代码')">
    <CodeOutlined />
  </button>
  <button class="toolbar-tool" title="超链接" @click="applyFormat('[', '](https://)', '链接描述')">
    <LinkOutlined />
  </button>

  <div class="divider-v"></div>

  <button class="toolbar-tool" @click="handleInsertImage">
    <PictureOutlined /> 插图
  </button>
  <button class="toolbar-tool" @click="openWikiSelector">
    <BookOutlined /> 引用百科
  </button>

  <span class="md-hint">Markdown Engine Active</span>
</div>

              <textarea 
                ref="contentEditor"
                v-model="formState.content" 
                placeholder="在此开始你的创作..." 
                class="textarea-content-pro" 
              ></textarea>
            </div>
          </a-col>

          <a-col v-if="showPreview" :xs="0" :lg="12" class="col-full-height">
            <div class="preview-container markdown-body" v-html="renderedContent"></div>
          </a-col>

          <a-col v-if="!showPreview" :xs="24" :lg="8" class="col-full-height">
            <div class="settings-sidebar">
              <h3 class="settings-title">发布设置</h3>
              
              <div class="setting-block">
                <label class="setting-label">封面图 // COVER</label>
                <div class="image-uploader-pro" @click="handleUploadCover">
                  <div v-if="isCoverUploading" class="upload-mask-pro"><a-spin /></div>
                  <img v-if="formState.coverImage" :src="formState.coverImage" class="preview-img-pro"/>
                  <div v-else class="upload-empty-pro">
                    <PlusOutlined /> <span>点击上传封面</span>
                  </div>
                </div>
              </div>

              <div class="setting-block">
                <label class="setting-label">标签属性 // TAGS</label>
                <CyberTagInput v-model="tagString" :maxTags="5" />
              </div>

              <div class="setting-block">
                <label class="setting-label">内容摘要 // SUMMARY</label>
                <textarea 
                  v-model="formState.summary" 
                  rows="5" 
                  placeholder="如果不填写，系统将自动生成摘要..." 
                  class="textarea-summary-pro"
                ></textarea>
              </div>
            </div>
          </a-col>
        </a-row>
      </div>
    </main>

    <a-modal
      v-model:open="showWikiSelector"
      title="引用寰宇百科词条"
      @ok="confirmWikiSelection"
      :confirmLoading="wikiTreeLoading"
      cancelText="取消"
      okText="插入引用"
      width="600px"
    >
      <a-spin :spinning="wikiTreeLoading">
        <div style="min-height: 200px; max-height: 400px; overflow-y: auto;">
          <a-tree
            v-if="wikiTreeData.length > 0"
            :tree-data="wikiTreeData"
            v-model:selectedKeys="selectedWikiKeys"
            defaultExpandAll
            blockNode
          />
          <a-empty v-else-if="!wikiTreeLoading" description="暂无百科数据" />
        </div>
      </a-spin>
    </a-modal>

  </div>
</template>

<script setup>
import { reactive, ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { message } from 'ant-design-vue';
import { 
  PlusOutlined, DeleteOutlined, HomeOutlined, 
  SendOutlined, PictureOutlined, EyeOutlined, BookOutlined,
  BoldOutlined, ItalicOutlined, StrikethroughOutlined, 
  LinkOutlined, CodeOutlined, FontSizeOutlined, AlignLeftOutlined
} from '@ant-design/icons-vue';
import apiClient from '@/utils/api';
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'; 

import dayjs from 'dayjs';
import { marked } from 'marked';


//import { markedHighlight } from 'marked-highlight';
import hljs from 'highlight.js';
import 'highlight.js/styles/atom-one-dark.css';

marked.use({
  renderer: {
    // 这里的参数可能是 token 对象（新版），也可能是纯代码字符串（老版）
    code(tokenOrCode, maybeLang) {
      // 智能提取代码内容和语言（完美兼容新老版本 marked）
      const codeText = typeof tokenOrCode === 'string' ? tokenOrCode : tokenOrCode.text;
      const langName = typeof tokenOrCode === 'string' ? maybeLang : tokenOrCode.lang;

      // 判断语言，如果没有指定则默认使用纯文本 plaintext
      const validLang = (langName && hljs.getLanguage(langName)) ? langName : 'plaintext';
      
      // highlight.js 内部已经做了一次完美的安全转义
      const highlighted = hljs.highlight(codeText, { language: validLang }).value;
      
      // 直接拼接成最终的 HTML，拒绝二次瞎转义！
      return `<pre><code class="hljs language-${validLang}">${highlighted}</code></pre>`;
    }
  }
});

const router = useRouter();
const contentEditor = ref(null); 
const allArticles = ref([]);
const listLoading = ref(false);
const activeTab = ref('drafts');
const currentArticleId = ref(null);
const publishLoading = ref(false);
const draftLoading = ref(false);
const lastSaved = ref('');
const showPreview = ref(false);
const uploadProgress = ref(0);
const isCoverUploading = ref(false);

const formState = reactive({
  title: '',
  content: '',
  coverImage: '',
  tags: [], 
  summary: '',
  status: 0
});

// --- 百科引用弹窗相关状态 ---
const showWikiSelector = ref(false);
const wikiTreeLoading = ref(false);
const wikiTreeData = ref([]);
const selectedWikiKeys = ref([]);

const tagString = computed({
  get: () => formState.tags.join(','),
  set: (val) => { formState.tags = val ? val.split(',') : []; }
});

const filteredList = computed(() => {
  const targetStatus = activeTab.value === 'drafts' ? 0 : 1;
  return allArticles.value.filter(item => item.status === targetStatus);
});

// 为了让预览模式能正常显示 Wiki 链接，在 marked 解析前替换一下语法
const renderedContent = computed(() => {
  if (!formState.content) return '<p style="color:#9ca3af">等待输入内容预览...</p>';
  let raw = formState.content;
  
  // 简单正则替换 [[词条名]] 为蓝色高亮文本供预览
  raw = raw.replace(/\[\[([^\]\s][^\]]*?)\]\]/g, (match, title) => {
    return `<span style="color: #2563eb; font-weight: 600; cursor: pointer; border-bottom: 1px dashed #2563eb;">${title}</span>`;
  });

  return marked.parse(raw);
});

// --- 数据接口 ---
const fetchMyArticles = async () => {
  listLoading.value = true;
  try {
    const res = await apiClient.get('/Blog/my-articles');
    allArticles.value = res.data.map(item => ({
      id: item.Id || item.id,
      title: item.Title || item.title,
      summary: item.Summary || item.summary,
      status: item.Status ?? item.status,
      updateTime: item.UpdateTime || item.updateTime
    }));
  } finally { listLoading.value = false; }
};

const createNewDraft = () => {
  currentArticleId.value = null;
  Object.assign(formState, { title: '', content: '', coverImage: '', tags: [], summary: '', status: 0 });
};

const loadArticle = async (id) => {
  if (currentArticleId.value === id) return;
  try {
    const res = await apiClient.get(`/Blog/articles/${id}`);
    const data = res.data;
    formState.title = data.title || '';
    formState.content = data.content || '';
    formState.summary = data.summary || '';
    formState.coverImage = data.coverImage || '';
    formState.status = data.status;
    formState.tags = data.tags || [];
    currentArticleId.value = id;
    lastSaved.value = dayjs().format('HH:mm:ss');
  } catch (err) { message.error('文章加载失败'); }
};

const submitArticle = async (isDraft) => {
  if (!formState.title) return message.warning('请输入标题');
  const loader = isDraft ? draftLoading : publishLoading;
  loader.value = true;
  try {
    const payload = { ...formState, status: isDraft ? 0 : 1 };
    if (!payload.summary) payload.summary = formState.content.substring(0, 120);

    if (currentArticleId.value) {
      await apiClient.put(`/Blog/articles/${currentArticleId.value}`, payload);
    } else {
      const res = await apiClient.post('/Blog/articles', payload);
      currentArticleId.value = res.data.articleId || res.data.id;
    }
    message.success(isDraft ? '已存入草稿' : '发布成功');
    lastSaved.value = dayjs().format('HH:mm:ss');
    fetchMyArticles();
  } finally { loader.value = false; }
};

const uploadFile = async (file) => {
  if (!currentArticleId.value) await submitArticle(true);
  const formData = new FormData();
  formData.append('file', file);
  const res = await apiClient.post(`/Blog/upload-image/${currentArticleId.value}`, formData);
  return res.data.url;
};

const handleUploadCover = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.onchange = async (e) => {
    if (!e.target.files[0]) return;
    isCoverUploading.value = true;
    try { formState.coverImage = await uploadFile(e.target.files[0]); }
    finally { isCoverUploading.value = false; }
  };
  input.click();
};

const handleInsertImage = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.onchange = async (e) => {
    if (!e.target.files[0]) return;
    try {
      const url = await uploadFile(e.target.files[0]);
      const insertText = `\n![图片描述](${url})\n`;
      insertTextAtCursor(insertText);
    } catch (err) { message.error('插图失败'); }
  };
  input.click();
};

// --- 百科引用核心逻辑 ---

// 打开选择器时，拉取目录树
const openWikiSelector = async () => {
  showWikiSelector.value = true;
  if (wikiTreeData.value.length === 0) {
    wikiTreeLoading.value = true;
    try {
      const res = await apiClient.get('/Wiki/category-tree');
      
      // 递归格式化后端数据以适配 Ant Design Vue Tree
      const formatTree = (nodes) => {
        if (!nodes) return [];
        return nodes.map(node => {
          // 🚀 核心修复：兼容后端返回的 PascalCase (大写) 或 camelCase (小写)
          const nodeType = node.Type || node.type;
          const nodeTitle = node.Title || node.title;
          const nodeId = node.Id || node.id;
          const nodeChildren = node.Children || node.children;

          const isArticle = nodeType === 'article';
          
          return {
            title: isArticle ? `📄 ${nodeTitle}` : `📁 ${nodeTitle}`,
            key: `${nodeType}_${nodeId}`, // 加前缀防止目录和文章ID冲突
            rawTitle: nodeTitle,
            type: nodeType,
            selectable: isArticle, // 关键：只能选中文章，不能选中目录
            disabled: !isArticle,  // 让目录显示为不可选中状态
            children: formatTree(nodeChildren)
          };
        });
      };
      
      wikiTreeData.value = formatTree(res.data);
    } catch (err) {
      message.error('获取百科目录失败，请检查网络');
    } finally {
      wikiTreeLoading.value = false;
    }
  }
};

// 确认插入百科链接
const confirmWikiSelection = () => {
  if (selectedWikiKeys.value.length === 0) {
    message.warning('请先点击选择一个百科词条');
    return;
  }
  
  const key = selectedWikiKeys.value[0];
  if (!key.startsWith('article_')) {
    message.warning('只能引用词条，不能引用目录哦');
    return;
  }

  // 递归在树中找到原始标题
  const findTitle = (nodes, targetKey) => {
    for (const node of nodes) {
      if (node.key === targetKey) return node.rawTitle;
      if (node.children) {
        const found = findTitle(node.children, targetKey);
        if (found) return found;
      }
    }
    return null;
  };

  const title = findTitle(wikiTreeData.value, key);
  if (title) {
    const insertText = `[[${title}]]`;
    insertTextAtCursor(insertText);
    
    // 关闭并清理状态
    showWikiSelector.value = false;
    selectedWikiKeys.value = [];
  }
};

// --- 通用方法：在光标处插入文字 ---
const insertTextAtCursor = (insertText) => {
  const el = contentEditor.value;
  if (!el) return;
  const start = el.selectionStart;
  const end = el.selectionEnd;
  formState.content = formState.content.substring(0, start) + insertText + formState.content.substring(end);
  
  // DOM 更新后恢复焦点和光标位置
  setTimeout(() => {
    el.focus();
    el.setSelectionRange(start + insertText.length, start + insertText.length);
  }, 0);
};

// --- 新增：智能文本格式化 ---
const applyFormat = (prefix, suffix, defaultText = '') => {
  const el = contentEditor.value;
  if (!el) return;

  const start = el.selectionStart;
  const end = el.selectionEnd;
  const selectedText = formState.content.substring(start, end);
  
  // 如果用户选中了文字，就包裹它；如果没有选中，就插入默认文字
  const textToInsert = selectedText || defaultText;

  formState.content = 
    formState.content.substring(0, start) + 
    prefix + textToInsert + suffix + 
    formState.content.substring(end);

  // 恢复焦点并设置光标位置
  setTimeout(() => {
    el.focus();
    if (selectedText) {
      // 如果之前有选中文字，处理完后将光标移到末尾
      const cursorTarget = start + prefix.length + textToInsert.length + suffix.length;
      el.setSelectionRange(cursorTarget, cursorTarget);
    } else {
      // 如果没有选中文字，高亮我们插入的“默认文字”，方便用户直接输入替换
      el.setSelectionRange(start + prefix.length, start + prefix.length + textToInsert.length);
    }
  }, 0);
};



const deleteArticle = async (id) => {
  await apiClient.delete(`/Blog/articles/${id}`);
  fetchMyArticles();
  if (currentArticleId.value === id) createNewDraft();
};

const formatTime = (t) => dayjs(t).format('YY/MM/DD HH:mm');
onMounted(fetchMyArticles);
</script>

<style scoped>
/* 0. 全局与重置 */
* { box-sizing: border-box; }
.workbench-container { 
  display: flex; 
  height: 100vh; 
  background: #f8f9fa; 
  color: #1f2937; 
  overflow: hidden; 
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; 
}

/* 1. 左侧侧边栏 */
.sidebar { 
  width: 280px; 
  background: #fff; 
  border-right: 1px solid #e5e7eb; 
  display: flex; 
  flex-direction: column; 
  flex-shrink: 0;
}
.sidebar-header { padding: 20px; }
.sidebar-nav { 
  display: flex; 
  padding: 0 20px 12px; 
  gap: 20px; 
  border-bottom: 1px solid #f3f4f6; 
}
.nav-item { 
  font-size: 14px; color: #6b7280; cursor: pointer; padding-bottom: 6px; 
  border-bottom: 2px solid transparent; transition: all 0.2s; 
}
.nav-item.active { color: #2563eb; border-bottom-color: #2563eb; font-weight: 600; }

/* 文章列表 */
.article-list-container { flex: 1; overflow-y: auto; padding: 12px; }
.article-list-container::-webkit-scrollbar { width: 4px; }
.article-list-container::-webkit-scrollbar-thumb { background: #e5e7eb; border-radius: 4px; }
.article-card { 
  padding: 12px 14px; border-radius: 8px; cursor: pointer; margin-bottom: 6px; 
  transition: all 0.2s; display: flex; justify-content: space-between; align-items: flex-start; 
}
.article-card:hover { background: #f3f4f6; }
.article-card.active { background: #eff6ff; }
.card-content { flex: 1; min-width: 0; }
.card-title { display: block; font-weight: 600; color: #111827; margin-bottom: 4px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.card-summary { font-size: 12px; color: #9ca3af; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; margin: 0; line-height: 1.5; }
.card-meta { font-size: 11px; color: #d1d5db; margin-top: 6px; display: block; }
.card-actions { opacity: 0; padding-left: 8px; color: #ef4444; font-size: 14px; }
.article-card:hover .card-actions { opacity: 1; }

/* 2. 右侧编辑主区域 */
.editor-area { 
  flex: 1; 
  display: flex; 
  flex-direction: column; 
  background: #fff; 
  min-width: 0; /* 防止子元素撑破flex容器 */
}

/* 顶部 Header (修复按钮错位) */
.editor-header { 
  height: 60px; 
  border-bottom: 1px solid #e5e7eb; 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  padding: 0 24px; 
  background: #ffffff;
  flex-shrink: 0;
}
.header-left { display: flex; align-items: center; gap: 8px; }
.header-right { display: flex; align-items: center; gap: 12px; } /* 核心：解决预览、发布按钮排版错乱 */

.dot-status { width: 8px; height: 8px; border-radius: 50%; display: inline-block; }
.dot-status.is-draft { background: #d1d5db; }
.dot-status.is-published { background: #10b981; }
.status-text { font-size: 13px; color: #4b5563; font-weight: 600; }
.last-saved { font-size: 12px; color: #9ca3af; margin-left: 8px; }

/* 编辑与预览布局 (修复高度塌陷) */
.editor-body { flex: 1; overflow: hidden; }
.col-full-height { height: 100%; overflow-y: auto; } /* 核心：让左右分栏独立滚动 */

/* 书写区 */
.writing-container { height: 100%; padding: 30px 40px; display: flex; flex-direction: column; }
.input-title-pro { border: none; font-size: 32px; font-weight: 800; outline: none; margin-bottom: 20px; width: 100%; color: #111827; background: transparent; }
.input-title-pro::placeholder { color: #d1d5db; }

.editor-toolbar-pro { 
  display: flex; align-items: center; gap: 12px; 
  margin-bottom: 16px; border-bottom: 1px solid #f3f4f6; padding-bottom: 12px; flex-wrap: wrap;
}
.toolbar-tool { background: none; border: none; color: #6b7280; font-size: 13px; cursor: pointer; display: flex; align-items: center; gap: 6px; padding: 4px 8px; border-radius: 4px; transition: 0.2s; }
.toolbar-tool:hover { color: #2563eb; background: #eff6ff; }
.divider-v { width: 1px; height: 14px; background: #e5e7eb; }
.md-hint { font-size: 12px; color: #d1d5db; margin-left: auto; font-family: monospace; }

.textarea-content-pro { 
  flex: 1; border: none; resize: none; outline: none; 
  font-size: 15px; line-height: 1.8; color: #374151; font-family: 'Consolas', monospace; background: transparent;
}

/* 预览区 */
.preview-container { 
  height: 100%; padding: 30px 40px; overflow-y: auto; 
  background: #fafafa; border-left: 1px solid #f3f4f6; 
  word-break: break-word;
}

/* 3. 设置侧面板 */
.settings-sidebar { height: 100%; background: #f9fafb; border-left: 1px solid #e5e7eb; padding: 24px; overflow-y: auto; }
.settings-title { font-size: 15px; font-weight: 600; margin-bottom: 20px; border-bottom: 1px solid #e5e7eb; padding-bottom: 12px; color: #111827; }
.setting-block { margin-bottom: 28px; }
.setting-label { font-size: 12px; color: #6b7280; margin-bottom: 10px; display: block; font-weight: 700; letter-spacing: 0.5px; }

/* 图片上传器 */
.image-uploader-pro { 
  width: 100%; height: 160px; border-radius: 8px; border: 1px dashed #d1d5db; 
  background: #fff; display: flex; align-items: center; justify-content: center; 
  cursor: pointer; position: relative; overflow: hidden; transition: 0.2s;
}
.image-uploader-pro:hover { border-color: #3b82f6; background: #eff6ff; }
.preview-img-pro { width: 100%; height: 100%; object-fit: cover; }
.upload-mask-pro { position: absolute; inset: 0; background: rgba(255,255,255,0.8); display: flex; align-items: center; justify-content: center; z-index: 10; }
.upload-empty-pro { display: flex; flex-direction: column; align-items: center; gap: 8px; color: #9ca3af; font-size: 13px; }

.textarea-summary-pro { 
  width: 100%; padding: 12px; border: 1px solid #e5e7eb; border-radius: 8px; 
  font-size: 13px; outline: none; transition: 0.2s; resize: vertical; min-height: 100px;
}
.textarea-summary-pro:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }

/* 4. 按钮统一排版 */
.btn-primary-pro { 
  background: #2563eb; color: #fff; border: none; padding: 8px 16px; 
  border-radius: 6px; font-weight: 500; font-size: 14px; cursor: pointer; 
  transition: 0.2s; display: flex; align-items: center; justify-content: center; gap: 6px; width: 100%;
}
.btn-primary-pro:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary-pro:disabled { opacity: 0.6; cursor: not-allowed; }

.header-right .btn-primary-pro { width: auto; } /* 顶部的发布按钮不需要100%宽 */

.btn-secondary-pro { 
  background: #fff; color: #4b5563; border: 1px solid #d1d5db; padding: 7px 16px; 
  border-radius: 6px; font-weight: 500; font-size: 14px; cursor: pointer; 
  transition: 0.2s; display: flex; align-items: center; justify-content: center; gap: 6px;
}
.btn-secondary-pro:hover:not(:disabled) { background: #f9fafb; border-color: #9ca3af; color: #111827; }
.btn-secondary-pro:disabled { opacity: 0.6; cursor: not-allowed; }


/* --- Markdown 预览区排版美化 (对应 .markdown-body) --- */
.preview-container :deep(h1),
.preview-container :deep(h2),
.preview-container :deep(h3) {
  color: #111827;
  border-bottom: 1px solid #e5e7eb;
  padding-bottom: 8px;
  margin-top: 24px;
  margin-bottom: 16px;
}

/* 文字引用 (Blockquote) 的样式 */
.preview-container :deep(blockquote) {
  margin: 16px 0;
  padding: 12px 20px;
  border-left: 4px solid #3b82f6; /* 蓝色的左侧提示线 */
  background-color: #eff6ff;     /* 浅蓝色的背景 */
  color: #4b5563;
  border-radius: 0 8px 8px 0;
}

/* 行内代码的样式 */
.preview-container :deep(code:not(.hljs)) {
  background-color: #f3f4f6;
  color: #ef4444;
  padding: 2px 6px;
  border-radius: 4px;
  font-family: 'Consolas', monospace;
  font-size: 0.9em;
}

/* 多行代码块的外层圆角和阴影 */
.preview-container :deep(pre) {
  background-color: #282c34; /* 配合 atom-one-dark 主题的背景色 */
  border-radius: 8px;
  padding: 16px;
  overflow-x: auto;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

</style>