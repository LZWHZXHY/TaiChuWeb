<template>
  <div class="my-posts-container">
    <div class="header-section glass-effect">
      <div class="title-group">
        <h2 class="page-title">我的发布</h2>
        <span class="sub-count">共 {{ pagination.total }} 条回响</span>
      </div>
      <div class="filter-actions">
        <a-radio-group v-model:value="statusFilter" size="small" button-style="solid" @change="handleFilterChange">
          <a-radio-button :value="null">全部</a-radio-button>
          <a-radio-button :value="0">已发布</a-radio-button>
          <a-radio-button :value="1">隐藏</a-radio-button>
          <a-radio-button :value="2">封禁</a-radio-button>
        </a-radio-group>
      </div>
    </div>

    <div class="list-section">
      <a-spin :spinning="loading">
        <div v-if="posts.length > 0" class="posts-grid">
          <div v-for="post in posts" :key="post.id" class="post-card glass-effect" @click="goToDetail(post.id)">
            <div v-if="post.images && post.images.length > 0" class="post-thumb" 
                 :style="{ backgroundImage: `url(${post.images[0]})` }">
              <div class="type-badge">{{ getPostTypeName(post.post_type) }}</div>
            </div>
            <div v-else class="post-thumb empty">
              <span>NO IMAGE</span>
              <div class="type-badge">{{ getPostTypeName(post.post_type) }}</div>
            </div>

            <div class="post-body">
              <div class="status-row">
                <a-tag :color="getStatusColor(post.status)" class="mini-tag">{{ getStatusName(post.status) }}</a-tag>
                <span class="mini-time">{{ formatDate(post.createTime) }}</span>
              </div>

              <h3 class="post-title">{{ post.post_title }}</h3>
              
              <div class="stats-row">
                <div class="stats-group">
                  <span><EyeOutlined /> {{ post.view_count }}</span>
                  <span><LikeOutlined /> {{ post.like_count }}</span>
                  <span><MessageOutlined /> {{ post.comment_count }}</span>
                </div>
                <div class="action-icons" @click.stop>
                   <a-popconfirm title="确定删除？" @confirm="handleDelete(post.id)">
                      <DeleteOutlined class="del-btn" />
                   </a-popconfirm>
                </div>
              </div>
            </div>
          </div>
        </div>

        <a-empty v-else-if="!loading" description="空空如也" />
      </a-spin>
    </div>

    <div class="pagination-section" v-if="pagination.total > pagination.pageSize">
      <a-pagination
        size="small"
        v-model:current="pagination.page"
        :total="pagination.total"
        :page-size="pagination.pageSize"
        @change="fetchMyPosts"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { message } from 'ant-design-vue';
import apiClient from '@/utils/api';
import dayjs from 'dayjs';
import { 
  EyeOutlined, LikeOutlined, MessageOutlined, 
  SearchOutlined, DeleteOutlined 
} from '@ant-design/icons-vue';

const router = useRouter();

// --- 状态变量 ---
const loading = ref(false);
const posts = ref([]);
const statusFilter = ref(null); // 默认看全部

const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
});

// --- 获取数据 ---
const fetchMyPosts = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/posts/my-posts', {
      params: {
        page: pagination.page,
        pageSize: pagination.pageSize,
        status: statusFilter.value
      }
    });

    if (res.data.success) {
      posts.value = res.data.data;
      pagination.total = res.data.pagination.totalCount;
    }
  } catch (error) {
    message.error('获取个人作品列表失败');
    console.error(error);
  } finally {
    loading.value = false;
  }
};

// --- 动作处理 ---
const handleFilterChange = () => {
  pagination.page = 1;
  fetchMyPosts();
};

const goToDetail = (id) => {
  router.push(`/post/${id}`);
};

const handleDelete = async (id) => {
  try {
    // 假设你后台有 DELETE api/posts/{id} 的接口
    await apiClient.delete(`/posts/${id}`);
    message.success('回响已消散');
    fetchMyPosts(); // 刷新
  } catch (error) {
    message.error('删除失败');
  }
};

// --- 格式化辅助函数 ---
const formatDate = (date) => dayjs(date).format('YYYY-MM-DD HH:mm');

const getPostTypeName = (type) => {
  const names = { 0: '柴圈', 1: '游戏', 2: '创作' };
  return names[type] || '其他';
};

const getPostTypeColor = (type) => {
  const colors = { 0: 'orange', 1: 'blue', 2: 'purple' };
  return colors[type] || 'default';
};

const getStatusName = (status) => {
  const names = { 0: '已发布', 1: '待审核/隐藏', 2: '已封禁' };
  return names[status] || '未知';
};

const getStatusColor = (status) => {
  const colors = { 0: 'success', 1: 'warning', 2: 'error' };
  return colors[status] || 'default';
};

onMounted(() => {
  fetchMyPosts();
});
</script>

<style scoped>
.my-posts-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  font-family: "Inter", system-ui, sans-serif;
}

/* 1. 更加紧凑的头部 */
.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 20px;
  margin-bottom: 20px;
  border-radius: 12px;
}

.page-title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
}

.sub-count {
  font-size: 12px;
  color: #94a3b8;
  margin-left: 8px;
}

/* 2. 网格布局：每行 3-4 个卡片 */
.posts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

/* 3. 紧凑型卡片设计 */
.post-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  cursor: pointer;
  transition: all 0.3s ease;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.4);
}

.post-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.05);
  background: white;
}

/* 4. 封面图区域（固定高度） */
.post-thumb {
  height: 140px;
  background-size: cover;
  background-position: center;
  position: relative;
  border-radius: 12px 12px 0 0;
}

.post-thumb.empty {
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #cbd5e1;
  font-size: 12px;
  font-weight: 700;
}

.type-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  backdrop-filter: blur(4px);
}

/* 5. 卡片主体内容 */
.post-body {
  padding: 12px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.status-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.mini-tag {
  font-size: 10px;
  line-height: 1.6;
  padding: 0 4px;
}

.mini-time {
  font-size: 11px;
  color: #94a3b8;
}

.post-title {
  font-size: 15px;
  font-weight: 700;
  color: #334155;
  margin-bottom: 12px;
  line-height: 1.4;
  height: 42px; /* 限制标题高度，保持整齐 */
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 6. 统计数据与操作 */
.stats-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto; /* 始终位于底部 */
  padding-top: 8px;
  border-top: 1px solid #f1f5f9;
}

.stats-group {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #64748b;
}

.stats-group span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.action-icons {
  font-size: 14px;
}

.del-btn {
  color: #94a3b8;
  transition: color 0.3s;
}

.del-btn:hover {
  color: #ef4444;
}

/* 分页器精简 */
.pagination-section {
  margin-top: 30px;
  display: flex;
  justify-content: center;
}

.glass-effect {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
}
</style>