<template>
  <div class="publish-blog-module">
    <div class="blog-workbench">
      
      <aside class="drafts-sidebar custom-scroll">
        <div class="sidebar-action">
          <button class="new-draft-btn" @click="createNewDraft">+ NEW_DRAFT</button>
        </div>
        <div class="draft-list">
          <div 
            v-for="item in allArticles" :key="item.id" 
            :class="['draft-item', { active: currentArticleId === item.id }]"
            @click="loadArticle(item.id)"
          >
            <div class="draft-title">{{ item.title || 'UNTITLED_DRAFT' }}</div>
            <div class="draft-meta">{{ formatTime(item.updateTime) }}</div>
          </div>
        </div>
      </aside>

      <main class="editor-main">
        <div class="blog-meta-header">
          <div class="cover-uploader" @click="handleUploadCover">
            <img v-if="form.coverImage" :src="form.coverImage" class="cover-img"/>
            <div v-else class="upload-placeholder">
              <span class="plus">+</span>
              <span>ADD_COVER</span>
            </div>
            <div v-if="isCoverUploading" class="upload-mask">SYNCING...</div>
          </div>
          
          <div class="title-input-wrap">
            <span class="prefix">>></span>
            <input v-model="form.title" class="blog-title-input" placeholder="INPUT_ARTICLE_TITLE..." />
          </div>
        </div>

        <div class="editor-toolbar">
          <div class="category-selector">
            <span class="label">CATEGORY:</span>
            <select v-model="form.categoryId" class="cyber-select-mini">
              <option :value="undefined">SELECT_CATEGORY</option>
              <option v-for="cat in categories" :key="cat.value" :value="cat.value">
                {{ cat.label.toUpperCase() }}
              </option>
            </select>
          </div>
          <div class="tool-group">
            <button class="tool-btn" @click="handleInsertImage">ATTACH_IMAGE</button>
            <button class="tool-btn" @click="showPreview = !showPreview">
              {{ showPreview ? 'CLOSE_PREVIEW' : 'OPEN_PREVIEW' }}
            </button>
          </div>
        </div>

        <div class="writing-area" :class="{ 'preview-active': showPreview }">
          <textarea 
            ref="contentEditor"
            v-model="form.content" 
            class="markdown-input custom-scroll" 
            placeholder="开始构建你的深度档案... (支持 Markdown 协议)"
          ></textarea>
          
          <div v-if="showPreview" class="markdown-preview custom-scroll markdown-body" v-html="renderedContent"></div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { marked } from 'marked';
import dayjs from 'dayjs';
import imageCompression from 'browser-image-compression';
import apiClient from '@/utils/api';

const props = defineProps(['sharedMeta']);
const emit = defineEmits(['success']);

// --- 状态管理 ---
const allArticles = ref([]);
const categories = ref([]);
const currentArticleId = ref(null);
const isCoverUploading = ref(false);
const showPreview = ref(true);
const contentEditor = ref(null);

const form = reactive({
  title: '',
  content: '',
  coverImage: '',
  categoryId: undefined,
  summary: ''
});

// --- 核心逻辑 ---
const renderedContent = computed(() => {
  return form.content ? marked.parse(form.content) : '<p style="color:#666">READY_FOR_INPUT...</p>';
});

const fetchCategories = async () => {
  const res = await apiClient.get('/blog/categories');
  categories.value = res.data.map(c => ({ value: c.Id, label: c.Name }));
};

const fetchMyArticles = async () => {
  const res = await apiClient.get('/blog/my-articles');
  allArticles.value = res.data.map(item => ({
    id: item.Id, title: item.Title, status: item.Status, updateTime: item.UpdateTime
  }));
};

const loadArticle = async (id) => {
  if (currentArticleId.value === id) return;
  
  try {
    const res = await apiClient.get(`/blog/articles/${id}`);
    const data = res.data;

    // ✅ 1. 兼容性映射：同时检查大小写，防止后端字段变动导致内容为空
    form.title = data.title || data.Title || '';
    form.content = data.content || data.Content || '';
    form.coverImage = data.coverImage || data.CoverImage || '';
    form.summary = data.summary || data.Summary || '';
    
    // ✅ 2. 分类 ID 处理
    if (data.categoryId || data.CategoryId) {
      form.categoryId = Number(data.categoryId || data.CategoryId);
    } else if (data.category?.id || data.category?.Id) {
      form.categoryId = Number(data.category?.id || data.category?.Id);
    }

    // ✅ 3. 标签同步：将数组转为字符串同步给父组件的 sharedMeta
    const rawTags = data.tags || data.Tags || [];
    if (Array.isArray(rawTags)) {
      // 因为父组件传进来的是 reactive 对象，这里修改会直接触发父组件 UI 更新
      props.sharedMeta.tags = rawTags.join(',');
    }

    currentArticleId.value = id;
    console.log('📡 档案已载入中枢:', id);
  } catch (error) {
    console.error('CRITICAL_ERROR // 档案读取失败:', error);
  }
};

const createNewDraft = () => {
  currentArticleId.value = null;
  Object.assign(form, { title: '', content: '', coverImage: '', categoryId: undefined, summary: '' });
};

// 图片上传处理 (带压缩)
const uploadFile = async (file) => {
  const options = { maxSizeMB: 5, maxWidthOrHeight: 1920, useWebWorker: true };
  const compressedFile = file.size > 5 * 1024 * 1024 ? await imageCompression(file, options) : file;
  
  const formData = new FormData();
  formData.append('file', compressedFile);
  
  const res = await apiClient.post(`/blog/upload-image/${currentArticleId.value || 0}`, formData);
  return res.data.url;
};

const handleUploadCover = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.onchange = async (e) => {
    isCoverUploading.value = true;
    try {
      form.coverImage = await uploadFile(e.target.files[0]);
    } finally {
      isCoverUploading.value = false;
    }
  };
  input.click();
};

const handleInsertImage = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.onchange = async (e) => {
    const url = await uploadFile(e.target.files[0]);
    const textarea = contentEditor.value;
    const start = textarea.selectionStart;
    form.content = form.content.substring(0, start) + `\n![IMAGE](${url})\n` + form.content.substring(textarea.selectionEnd);
  };
  input.click();
};

// --- ✅ 统一暴露给控制台的接口 ---
const validate = () => {
  if (!form.title) { alert('ERROR: TITLE_REQUIRED'); return false; }
  if (!form.categoryId) { alert('ERROR: CATEGORY_REQUIRED'); return false; }
  return true;
};

const submit = async () => {
  const payload = {
    ...form,
    tags: props.sharedMeta.tags,
    status: 1, // 正式发布
    visibility: props.sharedMeta.visibility,
    summary: form.summary || form.content.substring(0, 100).replace(/[#*`]/g, '')
  };

  const res = currentArticleId.value 
    ? await apiClient.put(`/blog/articles/${currentArticleId.value}`, payload)
    : await apiClient.post('/blog/articles', payload);

  if (res.data.success) {
    emit('success', res.data);
  }
};

defineExpose({ validate, submit });

onMounted(() => {
  fetchCategories();
  fetchMyArticles();
});

const formatTime = (time) => dayjs(time).format('MM/DD HH:mm');
</script>

<style scoped>
.publish-blog-module { height: 100%; display: flex; flex-direction: column; background: #fff; }
.blog-workbench { display: flex; flex: 1; overflow: hidden; }

/* 草稿侧边栏 */
.drafts-sidebar { width: 200px; background: #f4f4f4; border-right: 2px solid #111; display: flex; flex-direction: column; }
.sidebar-action { padding: 15px; border-bottom: 1px solid #ddd; }
.new-draft-btn { width: 100%; padding: 8px; background: #111; color: #fff; border: none; font-family: 'JetBrains Mono'; font-size: 10px; cursor: pointer; }
.draft-item { padding: 12px 15px; border-bottom: 1px solid #eee; cursor: pointer; transition: 0.2s; }
.draft-item:hover { background: #e8e8e8; }
.draft-item.active { background: #fff; border-left: 4px solid #D92323; }
.draft-title { font-size: 12px; font-weight: bold; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: #333; }
.draft-meta { font-size: 9px; color: #999; margin-top: 4px; font-family: 'JetBrains Mono'; }

/* 主编辑器区 */
.editor-main { flex: 1; display: flex; flex-direction: column; padding: 0; overflow: hidden; }

.blog-meta-header { display: flex; gap: 20px; padding: 20px; border-bottom: 1px solid #eee; }
.cover-uploader { width: 120px; height: 80px; border: 2px dashed #ccc; position: relative; cursor: pointer; overflow: hidden; flex-shrink: 0; }
.cover-img { width: 100%; height: 100%; object-fit: cover; }
.upload-placeholder { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #999; font-size: 10px; font-weight: bold; }
.upload-mask { position: absolute; inset: 0; background: rgba(0,0,0,0.5); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 10px; }

.title-input-wrap { flex: 1; display: flex; align-items: center; gap: 10px; }
.prefix { color: #D92323; font-weight: bold; font-family: 'JetBrains Mono'; }
.blog-title-input { border: none; outline: none; font-size: 28px; font-weight: 900; width: 100%; font-family: 'Anton', sans-serif; text-transform: uppercase; }

.editor-toolbar { padding: 10px 20px; background: #fafafa; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
.category-selector { display: flex; align-items: center; gap: 10px; }
.category-selector .label { font-size: 10px; font-weight: bold; color: #888; font-family: 'JetBrains Mono'; }
.cyber-select-mini { border: 1px solid #111; font-size: 11px; padding: 4px; font-family: 'JetBrains Mono'; outline: none; }

.tool-btn { background: none; border: 1px solid #ddd; padding: 4px 12px; font-size: 11px; font-weight: bold; cursor: pointer; margin-left: 8px; font-family: 'JetBrains Mono'; }
.tool-btn:hover { border-color: #111; background: #eee; }

.writing-area { flex: 1; display: flex; overflow: hidden; }
.markdown-input { flex: 1; border: none; outline: none; padding: 30px; font-size: 16px; line-height: 1.8; resize: none; background: #fff; color: #333; }
.markdown-preview { flex: 0; width: 0; transition: all 0.3s; background: #fcfcfc; border-left: 1px solid #eee; padding: 0; overflow-y: auto; }
.preview-active .markdown-input { border-right: 1px solid #eee; }
.preview-active .markdown-preview { flex: 1; width: auto; padding: 30px; }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>