<template>
  <div class="workbench-container">
    
    <aside class="sidebar glass-effect">
      <div class="sidebar-header">
        <a-button type="primary" block @click="createNewDraft">
          <PlusOutlined /> 新建草稿
        </a-button>
      </div>

      <a-tabs v-model:activeKey="activeTab" class="sidebar-tabs" :centered="true">
        <a-tab-pane key="drafts" tab="草稿箱" />
        <a-tab-pane key="published" tab="已发布" />
      </a-tabs>

      <div class="article-list-container">
        <a-spin :spinning="listLoading">
          <div 
            v-for="item in filteredList" 
            :key="item.id" 
            :class="['article-item', { active: currentArticleId === item.id }]"
            @click="loadArticle(item.id)"
          >
            <div class="item-main">
              <span class="item-title">{{ item.title || '无标题草稿' }}</span>
              <p class="item-summary">{{ item.summary || '暂无摘要...' }}</p>
              <span class="item-date">{{ formatTime(item.updateTime) }}</span>
            </div>
            <div class="item-actions">
              <a-popconfirm title="确定删除这篇稿件吗？" @confirm.stop="deleteArticle(item.id)">
                <DeleteOutlined class="delete-icon" />
              </a-popconfirm>
            </div>
          </div>
          <a-empty v-if="filteredList.length === 0" description="暂无内容" style="margin-top: 40px" />
        </a-spin>
      </div>
      
      <div class="sidebar-footer">
        <a-button type="text" @click="router.push('/')">
          <HomeOutlined /> 返回首页
        </a-button>
      </div>
    </aside>

    <main class="editor-area">
      <div v-if="uploadProgress > 0 && uploadProgress < 100" class="global-progress-bar">
        <a-progress 
          :percent="uploadProgress" 
          :show-info="false" 
          stroke-color="#1890ff" 
          size="small" 
          status="active"
        />
      </div>
      <header class="editor-header">
        <div class="header-left">
          <span class="status-badge" :class="formState.status === 1 ? 'published' : 'draft'">
            {{ formState.status === 1 ? '已发布' : '草稿状态' }}
          </span>
          <span v-if="lastSaved" class="save-time">上次同步: {{ lastSaved }}</span>
        </div>
        <div class="header-right">
          <a-button type="dashed" @click="togglePreview" style="margin-right: 10px;">
            <EyeOutlined /> {{ showPreview ? '关闭预览' : '开启预览' }}
          </a-button>
          
          <a-button @click="submitArticle(true)" :loading="draftLoading">保存草稿</a-button>
          <a-button type="primary" @click="submitArticle(false)" :loading="publishLoading" style="margin-left: 10px;">
            <SendOutlined /> 发布文章
          </a-button>
        </div>
      </header>

      <div class="editor-content-wrapper">
        <a-row :gutter="24" style="height: 100%">
          
          <a-col :xs="24" :lg="showPreview ? 12 : 17" style="height: 100%; display: flex; flex-direction: column;">
            <div class="writing-paper">
              <input v-model="formState.title" class="title-input" placeholder="输入文章标题..." maxlength="100" />
              
              <div class="editor-toolbar">
                <a-tooltip title="插入图片到正文">
                  <a-button size="small" @click="handleInsertImage">
                    <PictureOutlined /> 插入图片
                  </a-button>
                </a-tooltip>
                <a-divider type="vertical" />
                <span class="toolbar-hint">支持 Markdown 语法</span>
              </div>

              <textarea 
                ref="contentEditor"
                v-model="formState.content" 
                placeholder="开始书写你的故事... (左侧输入 Markdown，右侧实时预览)" 
                class="content-input" 
              ></textarea>
            </div>
          </a-col>

          <a-col v-if="showPreview" :xs="0" :lg="12" style="height: 100%; overflow-y: auto;">
            <div class="preview-paper markdown-body" v-html="renderedContent"></div>
          </a-col>

          <a-col v-if="!showPreview" :xs="0" :lg="7">
            <div class="settings-panel">
              <h3 class="panel-title">发布设置</h3>
              <a-form layout="vertical" :model="formState">
                
                <a-form-item label="文章封面 (点击上传)">
                  <div class="cover-uploader" @click="handleUploadCover">
                    <div v-if="isCoverUploading" class="uploading-overlay">
                      <a-spin tip="上传中..." />
                    </div>
                    
                    <img v-if="formState.coverImage" :src="formState.coverImage" class="cover-img"/>
                    <div v-else class="upload-placeholder">
                      <PlusOutlined style="font-size: 24px" />
                      <span>上传封面图</span>
                    </div>
                    <div v-if="formState.coverImage && !isCoverUploading" class="cover-mask">修改封面</div>
                  </div>
                </a-form-item>

                <a-form-item label="所属分类" required>
                  <div style="display: flex; gap: 8px;">
                    <a-select 
                      v-model:value="formState.categoryId" 
                      :options="categoryOptions" 
                      placeholder="请选择分类" 
                      style="flex: 1"
                      show-search
                      :filter-option="filterOption"
                    />
                    <a-tooltip title="新建分类">
                      <a-button @click="showCreateCategoryModal">
                        <PlusOutlined />
                      </a-button>
                    </a-tooltip>
                  </div>
                </a-form-item>

                <a-form-item label="标签列表">
                  <a-select v-model:value="formState.tags" mode="tags" placeholder="输入标签按回车" />
                </a-form-item>

                <a-form-item label="文章摘要">
                  <a-textarea v-model:value="formState.summary" :rows="4" placeholder="如果不填，系统将自动生成摘要..." />
                </a-form-item>

              </a-form>
            </div>
          </a-col>
        </a-row>
      </div>

      <a-modal
        v-model:visible="modalVisible"
        title="创建新分类"
        @ok="handleCreateCategory"
        :confirmLoading="createCatLoading"
      >
        <a-form layout="vertical">
          <a-form-item label="分类名称" required>
            <a-input 
              v-model:value="newCategoryName" 
              placeholder="请输入分类名称" 
              @pressEnter="handleCreateCategory"
            />
          </a-form-item>
        </a-form>
      </a-modal>

    </main>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted, computed } from 'vue';
import imageCompression from 'browser-image-compression';
import { useRouter } from 'vue-router';
import { message } from 'ant-design-vue';
import apiClient from '@/utils/api';
import { 
  PlusOutlined, DeleteOutlined, HomeOutlined, 
  SendOutlined, PictureOutlined, EyeOutlined 
} from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import { marked } from 'marked';

const router = useRouter();
const contentEditor = ref(null); 

// --- 状态变量 ---
const allArticles = ref([]);
const listLoading = ref(false);
const activeTab = ref('drafts');
const currentArticleId = ref(null);
const publishLoading = ref(false);
const draftLoading = ref(false);
const lastSaved = ref('');
const showPreview = ref(false);

// ▼▼▼ 新增：上传相关状态 ▼▼▼
const uploadProgress = ref(0); // 0-100
const isCoverUploading = ref(false);
// ▲▲▲

// 分类相关
const categories = ref([]);
const categoryOptions = ref([]);

// 弹窗相关
const modalVisible = ref(false);
const newCategoryName = ref('');
const createCatLoading = ref(false);

const formState = reactive({
  title: '',
  content: '',
  coverImage: '',
  categoryId: undefined,
  tags: [],
  summary: '',
  status: 0
});

// --- 计算属性 ---
const filteredList = computed(() => {
  const targetStatus = activeTab.value === 'drafts' ? 0 : 1;
  return allArticles.value.filter(item => item.status === targetStatus);
});

const renderedContent = computed(() => {
  return formState.content ? marked.parse(formState.content) : '<p style="color:#ccc">预览区域...</p>';
});

const togglePreview = () => {
  showPreview.value = !showPreview.value;
};

// --- 加载列表 ---
const fetchMyArticles = async () => {
  listLoading.value = true;
  try {
    const res = await apiClient.get('/blog/my-articles');
    allArticles.value = res.data.map(item => ({
      id: item.Id,
      title: item.Title,
      summary: item.Summary, 
      status: item.Status,
      updateTime: item.UpdateTime
    }));
  } catch (error) {
    message.error('列表加载失败');
  } finally {
    listLoading.value = false;
  }
};

const fetchCategories = async () => {
  try {
    const res = await apiClient.get('/blog/categories');
    categories.value = res.data.map(c => ({ value: c.Id, label: c.Name }));
    categoryOptions.value = [...categories.value];
  } catch (error) {
    console.error('分类获取失败');
  }
};

// --- 新建分类逻辑 ---
const showCreateCategoryModal = () => {
  newCategoryName.value = '';
  modalVisible.value = true;
};

const handleCreateCategory = async () => {
  if (!newCategoryName.value) { message.warning('请输入分类名称'); return; }
  const exists = categoryOptions.value.some(c => c.label === newCategoryName.value);
  if (exists) { message.warning('该分类已存在'); return; }

  createCatLoading.value = true;
  try {
    const res = await apiClient.post('/blog/categories', { name: newCategoryName.value });
    const newId = res.data.Id || res.data.id;
    const newName = res.data.Name || res.data.name;
    const newOption = { value: newId, label: newName };
    categories.value.push(newOption);
    categoryOptions.value.push(newOption);
    formState.categoryId = newId;
    message.success('创建成功');
    modalVisible.value = false;
  } catch (error) {
    message.error('创建失败');
  } finally {
    createCatLoading.value = false;
  }
};

const filterOption = (input, option) => {
  return option.label.toLowerCase().indexOf(input.toLowerCase()) >= 0;
};

// --- 文章管理 ---
const createNewDraft = () => {
  currentArticleId.value = null;
  Object.assign(formState, { title: '', content: '', coverImage: '', categoryId: undefined, tags: [], summary: '', status: 0 });
  message.info('切换到新文档');
};

const loadArticle = async (id) => {
  if (currentArticleId.value === id) return;
  try {
    const res = await apiClient.get(`/blog/articles/${id}`);
    const data = res.data;
    formState.title = data.Title || data.title || '';
    formState.content = data.Content || data.content || ''; 
    formState.summary = data.Summary || data.summary || '';
    formState.coverImage = data.CoverImage || data.coverImage || '';
    formState.status = data.Status;
    
    if (data.CategoryId || data.categoryId) formState.categoryId = Number(data.CategoryId || data.categoryId);
    else if (data.Category?.Id || data.category?.id) formState.categoryId = Number(data.Category?.Id || data.category?.id);
    else formState.categoryId = undefined;

    if (data.Tags || data.tags) formState.tags = data.Tags || data.tags;

    currentArticleId.value = id;
    lastSaved.value = dayjs(data.UpdateTime || data.updateTime).format('HH:mm:ss');
  } catch (error) {
    message.error('文章加载失败');
  }
};

const deleteArticle = async (id) => {
  try {
    await apiClient.delete(`/blog/articles/${id}`);
    message.success('删除成功');
    allArticles.value = allArticles.value.filter(a => a.id !== id);
    if (currentArticleId.value === id) createNewDraft();
  } catch (error) {
    message.error('删除失败');
  }
};

// --- 图片上传 (带进度条) ---
const MAX_NO_COMPRESS_SIZE = 5 * 1024 * 1024; // 5MB

const uploadFile = async (file) => {
  if (!currentArticleId.value) {
    await submitArticle(true); // 先保存草稿获取ID
  }

  // 重置进度条
  uploadProgress.value = 0;

  try {
    let fileToUpload = file;

    // 压缩逻辑
    if (file.size > MAX_NO_COMPRESS_SIZE) {
      message.loading({ content: '正在压缩图片...', key: 'up' });
      try {
        const options = { maxSizeMB: 5, maxWidthOrHeight: 1920, useWebWorker: true };
        fileToUpload = await imageCompression(file, options);
      } catch (err) {
        console.warn('压缩失败，尝试原图上传', err);
      }
    }

    message.loading({ content: '正在上传...', key: 'up' });
    
    const formData = new FormData();
    formData.append('file', fileToUpload, file.name);

    const res = await apiClient.post(`/blog/upload-image/${currentArticleId.value}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 120000,
      // ▼▼▼ Axios 上传进度回调 ▼▼▼
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        uploadProgress.value = percentCompleted;
      }
    });

    // 延迟一点清除进度条，让用户看到 100%
    setTimeout(() => { uploadProgress.value = 0; }, 500);

    if (res.data.success) {
      return res.data.url;
    } else {
      throw new Error(res.data.message);
    }
  } catch (error) {
    uploadProgress.value = 0;
    console.error("上传出错", error);
    throw error;
  }
};

const handleUploadCover = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  input.onchange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    
    isCoverUploading.value = true; // 开启封面遮罩
    try {
      const url = await uploadFile(file);
      formState.coverImage = url;
      message.success({ content: '封面已同步', key: 'up' });
    } catch (err) {
      message.error({ content: '上传失败', key: 'up' });
    } finally {
      isCoverUploading.value = false; // 关闭封面遮罩
    }
  };
  input.click();
};

const handleInsertImage = () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  input.onchange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    try {
      const url = await uploadFile(file);
      
      // 原生 textarea 插入逻辑
      const textarea = contentEditor.value;
      const startPos = textarea.selectionStart;
      const endPos = textarea.selectionEnd;
      const originalText = formState.content;
      
      const insertText = `\n![图片描述](${url})\n`;
      formState.content = originalText.substring(0, startPos) + insertText + originalText.substring(endPos);
      
      message.success({ content: '已插入正文', key: 'up' });
    } catch (err) {
      message.error({ content: '插图失败', key: 'up' });
    }
  };
  input.click();
};

// --- 保存与发布 ---
const submitArticle = async (isDraft) => {
  if (!formState.title) return message.warning('请输入标题');
  if (!isDraft && !formState.categoryId) {
    return message.warning('请选择文章分类');
  }

  const loadingRef = isDraft ? draftLoading : publishLoading;
  loadingRef.value = true;

  try {
    const payload = {
      ...formState,
      status: isDraft ? 0 : 1,
      summary: formState.summary || formState.content.substring(0, 100).replace(/[#*`]/g, '') + '...'
    };

    let res;
    if (currentArticleId.value) {
      res = await apiClient.put(`/blog/articles/${currentArticleId.value}`, payload);
    } else {
      res = await apiClient.post('/blog/articles', payload);
      // 如果是新建，后端会返回新ID
      currentArticleId.value = res.data.articleId || res.data.data?.articleId; 
    }

    message.success(isDraft ? '草稿已保存' : '文章已正式发布');
    lastSaved.value = dayjs().format('HH:mm:ss');
    await fetchMyArticles(); 
    
    if (!isDraft) activeTab.value = 'published';
  } catch (error) {
    console.error(error);
    message.error('操作失败，请检查网络');
  } finally {
    loadingRef.value = false;
  }
};

const formatTime = (time) => dayjs(time).format('MM-DD HH:mm');

onMounted(async () => {
  await fetchCategories();
  await fetchMyArticles();
  const lastDraft = allArticles.value.find(a => a.status === 0);
  if (lastDraft) loadArticle(lastDraft.id);
});
</script>

<style scoped>
/* 容器布局 */
.workbench-container { display: flex; height: 100vh; background-color: #f0f2f5; overflow: hidden; }

/* 侧边栏样式 */
.sidebar { width: 280px; background: white; border-right: 1px solid #e8e8e8; display: flex; flex-direction: column; flex-shrink: 0; }
.sidebar-header { padding: 16px; border-bottom: 1px solid #f0f0f0; }
.article-list-container { flex: 1; overflow-y: auto; padding: 8px; }
.article-item { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; border-radius: 8px; cursor: pointer; margin-bottom: 4px; transition: all 0.2s; border-left: 3px solid transparent; }
.article-item:hover { background: #f5f5f5; }
.article-item.active { background: #e6f7ff; border-left-color: #1890ff; }
.item-title { font-weight: 500; color: #333; font-size: 14px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; display: block; margin-bottom: 4px; }
.item-summary { font-size: 12px; color: #888; margin: 0 0 6px 0; line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.item-date { font-size: 12px; color: #999; }
.item-actions { opacity: 0; }
.article-item:hover .item-actions { opacity: 1; }
.delete-icon { color: #999; }
.delete-icon:hover { color: #ff4d4f; }
.sidebar-footer { padding: 12px; border-top: 1px solid #f0f0f0; text-align: center; }

/* 编辑器区域 */
.editor-area { flex: 1; display: flex; flex-direction: column; background: #f5f7fa; overflow: hidden; position: relative; }

/* 进度条样式 */
.global-progress-bar {
  position: absolute; top: 0; left: 0; width: 100%; z-index: 999;
}

.editor-header { height: 56px; background: white; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; padding: 0 24px; }
.status-badge { font-size: 12px; padding: 2px 8px; border-radius: 4px; margin-right: 12px; }
.status-badge.draft { background: #e6f7ff; color: #1890ff; border: 1px solid #91d5ff; }
.status-badge.published { background: #f6ffed; color: #52c41a; border: 1px solid #b7eb8f; }
.save-time { color: #bbb; font-size: 12px; }

.editor-content-wrapper { flex: 1; padding: 24px; overflow-y: hidden; }
.writing-paper { background: white; padding: 40px; border-radius: 8px; height: 100%; box-shadow: 0 1px 4px rgba(0,0,0,0.05); display: flex; flex-direction: column; }

.preview-paper { 
  background: white; padding: 40px; border-radius: 8px; 
  height: 100%; box-shadow: 0 1px 4px rgba(0,0,0,0.05); 
  overflow-y: auto; 
  border-left: 1px solid #eee;
}
.markdown-body :deep(img) { max-width: 100%; border-radius: 8px; margin: 10px 0; }

.editor-toolbar { margin: 10px 0; display: flex; align-items: center; gap: 8px; flex-shrink: 0; }
.toolbar-hint { font-size: 12px; color: #bbb; }

.title-input { width: 100%; border: none; font-size: 28px; font-weight: bold; outline: none; margin-bottom: 20px; color: #1f1f1f; flex-shrink: 0; }
.content-input { 
  flex: 1; font-size: 16px; line-height: 1.8; color: #333; 
  border: none; outline: none; resize: none; width: 100%; 
  background: transparent;
}

.settings-panel { background: white; padding: 24px; border-radius: 8px; height: 100%; overflow-y: auto; }
.panel-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; border-left: 3px solid #1890ff; padding-left: 10px; }

/* 封面图样式 */
.cover-uploader { width: 100%; height: 160px; border: 1px dashed #d9d9d9; border-radius: 8px; cursor: pointer; overflow: hidden; position: relative; background: #fafafa; display: flex; flex-direction: column; align-items: center; justify-content: center; }
.cover-uploader:hover { border-color: #1890ff; }
.cover-img { width: 100%; height: 100%; object-fit: cover; }
.upload-placeholder { color: #999; display: flex; flex-direction: column; align-items: center; gap: 4px; }
.cover-mask { position: absolute; bottom: 0; width: 100%; background: rgba(0,0,0,0.5); color: white; text-align: center; padding: 4px 0; font-size: 12px; opacity: 0; transition: opacity 0.3s; }
.cover-uploader:hover .cover-mask { opacity: 1; }

.uploading-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(255, 255, 255, 0.8);
  display: flex; align-items: center; justify-content: center; z-index: 10;
}

.glass-effect { backdrop-filter: blur(10px); }
</style>