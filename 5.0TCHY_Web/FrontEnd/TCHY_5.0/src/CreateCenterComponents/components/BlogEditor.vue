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
                <button class="toolbar-tool" @click="handleInsertImage">
                  <PictureOutlined /> 插入图片
                </button>
                <div class="divider-v"></div>
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
  </div>
</template>

<script setup>
import { reactive, ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { message } from 'ant-design-vue';
import apiClient from '@/utils/api';
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'; // 确保路径正确
import { 
  PlusOutlined, DeleteOutlined, HomeOutlined, 
  SendOutlined, PictureOutlined, EyeOutlined 
} from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import { marked } from 'marked';

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
  tags: [], // 后端需要 List<string>
  summary: '',
  status: 0
});

// 计算属性适配：CyberTagInput 内部使用逗号分隔的 String，formState 使用 Array
const tagString = computed({
  get: () => formState.tags.join(','),
  set: (val) => { formState.tags = val ? val.split(',') : []; }
});

const filteredList = computed(() => {
  const targetStatus = activeTab.value === 'drafts' ? 0 : 1;
  return allArticles.value.filter(item => item.status === targetStatus);
});

const renderedContent = computed(() => {
  return formState.content ? marked.parse(formState.content) : '<p style="color:#9ca3af">等待输入内容预览...</p>';
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
      const el = contentEditor.value;
      const start = el.selectionStart;
      formState.content = formState.content.substring(0, start) + insertText + formState.content.substring(el.selectionEnd);
    } catch (err) { message.error('插图失败'); }
  };
  input.click();
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
.workbench-container { display: flex; height: 100vh; background: #f8f9fa; color: #1f2937; overflow: hidden; font-family: -apple-system, sans-serif; }

/* 侧边栏列表 */
.sidebar { width: 280px; background: #fff; border-right: 1px solid #e5e7eb; display: flex; flex-direction: column; }
.sidebar-header { padding: 20px; }
.sidebar-nav { display: flex; padding: 0 20px 15px; gap: 20px; border-bottom: 1px solid #f3f4f6; }
.nav-item { font-size: 14px; color: #6b7280; cursor: pointer; padding-bottom: 5px; border-bottom: 2px solid transparent; transition: 0.2s; }
.nav-item.active { color: #2563eb; border-bottom-color: #2563eb; font-weight: 600; }

.article-list-container { flex: 1; overflow-y: auto; padding: 10px; }
.article-card { padding: 12px 15px; border-radius: 8px; cursor: pointer; margin-bottom: 4px; transition: 0.2s; display: flex; justify-content: space-between; align-items: flex-start; }
.article-card:hover { background: #f3f4f6; }
.article-card.active { background: #eff6ff; }
.card-title { display: block; font-weight: 600; color: #111827; margin-bottom: 4px; }
.card-summary { font-size: 12px; color: #9ca3af; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.card-meta { font-size: 11px; color: #d1d5db; margin-top: 5px; display: block; }
.card-actions { opacity: 0; }
.article-card:hover .card-actions { opacity: 1; }

/* 编辑区域 */
.editor-area { flex: 1; display: flex; flex-direction: column; background: #fff; }
.editor-header { height: 56px; border-bottom: 1px solid #e5e7eb; display: flex; justify-content: space-between; align-items: center; padding: 0 24px; }
.dot-status { width: 8px; height: 8px; border-radius: 50%; display: inline-block; margin-right: 6px; }
.dot-status.is-draft { background: #d1d5db; }
.dot-status.is-published { background: #10b981; }
.status-text { font-size: 13px; color: #4b5563; font-weight: 500; }

.writing-container { height: 100%; padding: 40px 10%; display: flex; flex-direction: column; }
.input-title-pro { border: none; font-size: 32px; font-weight: 800; outline: none; margin-bottom: 20px; width: 100%; color: #111827; }
.textarea-content-pro { flex: 1; border: none; resize: none; outline: none; font-size: 16px; line-height: 1.8; color: #374151; }

.editor-toolbar-pro { display: flex; align-items: center; gap: 12px; margin-bottom: 15px; border-bottom: 1px solid #f3f4f6; padding-bottom: 10px; }
.toolbar-tool { background: none; border: none; color: #6b7280; font-size: 13px; cursor: pointer; }
.toolbar-tool:hover { color: #2563eb; }
.divider-v { width: 1px; height: 14px; background: #e5e7eb; }

/* 设置侧面板 */
.settings-sidebar { height: 100%; background: #f9fafb; border-left: 1px solid #e5e7eb; padding: 24px; overflow-y: auto; }
.settings-title { font-size: 15px; font-weight: 600; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 10px; }
.setting-block { margin-bottom: 24px; }
.setting-label { font-size: 12px; color: #9ca3af; margin-bottom: 8px; display: block; font-weight: bold; text-transform: uppercase; letter-spacing: 0.5px; }

.image-uploader-pro { width: 100%; height: 160px; border-radius: 8px; border: 1px dashed #d1d5db; background: #fff; display: flex; align-items: center; justify-content: center; cursor: pointer; position: relative; overflow: hidden; }
.preview-img-pro { width: 100%; height: 100%; object-fit: cover; }
.upload-mask-pro { position: absolute; inset: 0; background: rgba(255,255,255,0.7); display: flex; align-items: center; justify-content: center; }

.textarea-summary-pro { width: 100%; padding: 12px; border: 1px solid #e5e7eb; border-radius: 6px; font-size: 13px; outline: none; transition: 0.2s; resize: none; }
.textarea-summary-pro:focus { border-color: #2563eb; }

/* 按钮通用 */
.btn-primary-pro { background: #2563eb; color: #fff; border: none; padding: 8px 16px; border-radius: 6px; font-weight: 500; cursor: pointer; transition: 0.2s; }
.btn-primary-pro:hover { background: #1d4ed8; }
.btn-secondary-pro { background: #fff; border: 1px solid #d1d5db; padding: 7px 16px; border-radius: 6px; font-weight: 500; cursor: pointer; margin-right: 10px; }

.preview-container { height: 100%; padding: 40px; overflow-y: auto; background: #fdfdfd; border-left: 1px solid #f3f4f6; }
</style>