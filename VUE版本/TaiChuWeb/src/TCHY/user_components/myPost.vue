<template>
  <div class="mypost-root">
    <div class="mypost-title">我的帖子</div>
    <div v-if="loading" class="mypost-loading">加载中...</div>
    <div v-else>
      <div v-if="posts.length === 0" class="mypost-empty">你还没有发布过任何帖子。</div>
      <div v-else class="mypost-list">
        <div v-for="post in posts" :key="post.id" class="mypost-card">
          <div class="mypost-card-header">
            <div class="mypost-card-title-row">
              <span class="mypost-post-title">{{ post.title }}</span>
              <span v-if="postTypeName(post.post_type) !== '未知'" class="mypost-post-type">
                {{ postTypeName(post.post_type) }}
              </span>
            </div>
            <div class="mypost-card-meta-row">
              <span class="mypost-meta-item"><i class="fa fa-calendar"></i> {{ formatDate(post.createTime) }}</span>
              <span class="mypost-meta-item"><i class="fa fa-heart"></i> {{ post.like_count }}</span>
              <span class="mypost-meta-item"><i class="fa fa-comment"></i> {{ post.comment_count }}</span>
            </div>
          </div>
          <div class="mypost-card-content-row">
            <div class="mypost-content">{{ post.content }}</div>
            <div v-if="post.images && post.images.length > 0" class="mypost-images">
              <img v-for="(img, i) in post.images" :key="i" :src="getImgUrl(img)" class="mypost-img" />
            </div>
            <div v-if="post.videos && post.videos.length > 0" class="mypost-videos">
              <video v-for="(vid, i) in post.videos" :key="i" :src="getVideoUrl(vid)" controls />
            </div>
          </div>
          <div class="mypost-card-footer">
            <button @click="deletePost(post.id)" class="mypost-delete-btn">
              <i class="fa fa-trash"></i> 删除
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";

const posts = ref([]);
const loading = ref(true);

const postTypeName = (type) => {
  if (type === 0) return "柴圈";
  if (type === 1) return "游戏";
  if (type === 2) return "其他";
  return "未知";
};

const formatDate = (isoStr) =>
  new Date(isoStr).toLocaleString("zh-CN", { hour12: false });

const getImgUrl = (imgPath) =>
  imgPath.startsWith("http") ? imgPath : `${baseApiUrl}/api/UserInfo/fileproxy?path=${encodeURIComponent(imgPath)}`;
const getVideoUrl = (vidPath) =>
  vidPath.startsWith("http") ? vidPath : `${baseApiUrl}/api/UserInfo/fileproxy?path=${encodeURIComponent(vidPath)}`;

async function fetchMyPosts() {
  loading.value = true;
  try {
    const res = await axios.get(`${baseApiUrl}/api/Posts/my`, {
      headers: {
        Authorization: "Bearer " + localStorage.authToken,
      },
    });
    posts.value = res.data.map(post => ({
      ...post,
      images: post.images ?? [],
      videos: post.videos ?? [],
    }));
  } catch (e) {
    alert("获取我的帖子失败：" + (e.response?.data?.message || e.message));
  }
  loading.value = false;
}

async function deletePost(id) {
  if (!confirm("确定要删除这个帖子吗？")) return;
  try {
    await axios.post(`${baseApiUrl}/api/Posts/delete`, { id }, {
      headers: {
        Authorization: "Bearer " + localStorage.authToken,
      },
    });
    posts.value = posts.value.filter(p => p.id !== id);
    alert("删除成功");
  } catch (e) {
    alert("删除失败：" + (e.response?.data?.message || e.message));
  }
}

onMounted(fetchMyPosts);
</script>

<style scoped>
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css");

/* 清新蓝紫渐变风格 + 卡片阴影 + 现代排版 */
.mypost-root {
  height: 100%;
  max-height: 80vh;
  overflow-y: auto;
  padding-bottom: 18px;
  background: linear-gradient(105deg, #f3f7fd 0%, #e0e7fa 100%);
}

.mypost-title {
  font-size: 2.1rem;
  font-weight: 700;
  margin-bottom: 26px;
  color: #5a6aff;
  letter-spacing: 1px;
  text-align: center;
  text-shadow: 0 2px 14px #b0b8ff33;
}

.mypost-loading,
.mypost-empty {
  color: #6a7ea7;
  padding: 30px 0 0 0;
  text-align: center;
  font-size: 1.18rem;
  font-weight: 500;
}

.mypost-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 32px;
  justify-items: center;
  align-items: stretch;
  padding-bottom: 32px;
}

.mypost-card {
  background: linear-gradient(120deg, #fafdff 0%, #ece5ff 100%);
  border-radius: 20px;
  box-shadow: 0 6px 32px #8e9cff20, 0 1.5px 6px #8e9cff16;
  width: 100%;
  max-width: 410px;
  min-width: 230px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1.5px solid #e7eaff;
  transition: box-shadow 0.22s, transform 0.15s;
  position: relative;
}

.mypost-card:hover {
  box-shadow: 0 14px 32px #a7bfff66, 0 2.5px 12px #a7bfff22;
  transform: translateY(-6px) scale(1.03);
}

.mypost-card-header {
  background: linear-gradient(90deg, #ecf3ff 0%, #f0f7ff 80%);
  padding: 18px 22px 9px 22px;
  border-bottom: 1px solid #e2f1fc;
}

.mypost-card-title-row {
  display: flex;
  align-items: baseline;
  gap: 10px;
  margin-bottom: 8px;
}

.mypost-post-title {
  font-size: 1.28rem;
  font-weight: 700;
  color: #6b5bfa;
  flex: 1;
  white-space: pre-wrap;
  word-break: break-word;
  letter-spacing: 0.5px;
}

.mypost-post-type {
  background: linear-gradient(90deg, #b48cff 0%, #6b90ff 100%);
  color: #fff;
  border-radius: 8px;
  padding: 3px 14px;
  font-size: 0.98rem;
  font-weight: 500;
  letter-spacing: 1px;
  box-shadow: 0 1px 8px #a38cff22;
}

.mypost-card-meta-row {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
  font-size: 1rem;
  color: #8b9bc1;
}

.mypost-meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.98rem;
}

.mypost-meta-item i {
  color: #b48cff;
  margin-right: 2px;
}

.mypost-card-content-row {
  padding: 18px 20px 12px 20px;
  border-bottom: 1px solid #e2f1fc;
  background: linear-gradient(110deg, #fafdff 0%, #f2f7ff 100%);
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.mypost-content {
  margin-bottom: 9px;
  color: #5746af;
  font-size: 1.11rem;
  line-height: 1.75;
  word-break: break-word;
  min-height: 28px;
  font-family: 'Segoe UI', 'PingFang SC', 'Helvetica Neue', Arial, 'Microsoft YaHei', sans-serif;
}

.mypost-images {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 2px;
}
.mypost-img {
  width: 80px;
  height: 80px;
  border-radius: 12px;
  box-shadow: 0 2px 8px #b48cff22;
  object-fit: cover;
  border: 1.5px solid #e2f1fc;
  background: #fff;
  transition: transform 0.15s, box-shadow 0.15s;
}
.mypost-img:hover {
  transform: scale(1.10) rotate(-2deg);
  box-shadow: 0 6px 14px #b48cff33;
}

.mypost-videos {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 2px;
}
.mypost-videos video {
  width: 160px;
  border-radius: 12px;
  background: #eee;
  box-shadow: 0 2px 8px #6b90ff22;
}

.mypost-card-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding: 16px 22px;
  background: linear-gradient(90deg, #f3f7fd 0%, #efe5ff 100%);
  border-top: 1px solid #e2f1fc;
}

.mypost-delete-btn {
  background: linear-gradient(90deg, #ff97b4 0%, #b48cff 100%);
  color: #fff;
  font-weight: 600;
  padding: 9px 26px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  box-shadow: 0 2px 10px #ff97b422;
  font-size: 1.13rem;
  transition: background 0.13s, box-shadow 0.18s, transform 0.12s;
  display: flex;
  align-items: center;
  gap: 10px;
}

.mypost-delete-btn i {
  font-size: 1.18em;
}

.mypost-delete-btn:hover {
  background: linear-gradient(90deg, #f46f8d 0%, #6b90ff 100%);
  box-shadow: 0 4px 18px #ff97b433;
  transform: scale(1.08);
}

@media (max-width: 900px) {
  .mypost-root {
    max-height: calc(100vh - 120px);
  }
  .mypost-list {
    gap: 16px;
    grid-template-columns: repeat(auto-fill, minmax(210px, 1fr));
  }
  .mypost-card {
    max-width: 98vw;
    min-width: 120px;
  }
}
@media (max-width: 600px) {
  .mypost-title {
    font-size: 1.13rem;
    margin-bottom: 7px;
  }
  .mypost-card-header,
  .mypost-card-content-row,
  .mypost-card-footer {
    padding: 7px 7px;
  }
  .mypost-card {
    font-size: 0.96rem;
  }
  .mypost-post-title {
    font-size: 0.93rem;
  }
  .mypost-post-type {
    font-size: 0.83rem;
  }
  .mypost-img {
    width: 58px;
    height: 58px;
  }
  .mypost-videos video {
    width: 90px;
  }
}
</style>