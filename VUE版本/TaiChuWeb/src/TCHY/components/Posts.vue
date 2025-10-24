<template>
  <div class="posts-container">
    <header class="navbar">
      <div class="navbar-logo">论坛社区</div>
      <div class="navbar-userinfo">
        <span>[{{ userName }}]</span>
        <span v-if="userLevel">Lv.{{ userLevel }}</span>
        <router-link to="/user"><div class="navbar-avatar"></div></router-link>
      </div>
    </header>
    <div class="layout">
      <aside class="sidebar left">
        <section class="panel">
          <h3>社区数据</h3>
          <div class="stats">
            <div class="stat-item"><span>注册人数</span><span>{{ stats.registered }}</span></div>
            <div class="stat-item"><span>今日活跃</span><span>{{ stats.activeToday }}</span></div>
            <div class="stat-item"><span>帖子总数</span><span>{{ stats.totalPosts }}</span></div>
          </div>
        </section>
        <section class="panel">
          <h3>在线用户</h3>
          <div class="user-list">
            <div class="user-item" v-for="user in onlineUsers" :key="user.id">
              <img class="user-avatar-sm"
                :src="baseApiUrl + user.logo"
                alt="头像"
                @error="e => e.target.src = '/static/img/default-avatar.png'" />
              <div class="user-details">
                <div class="user-name">{{ user.username }}</div>
                <div class="user-level">Lv.{{ user.level }}</div>
              </div>
            </div>
          </div>
        </section>
      </aside>
      <main class="main-content">
        <div class="filter-bar">
          <input v-model="keyword" placeholder="搜索帖子..." class="search-input" @keyup.enter="fetchPosts"/>
          <select v-model="selectedType" class="type-select" @change="fetchPosts">
            <option value="">全部类型</option>
            <option v-for="(cat, idx) in categories.slice(1)" :key="idx" :value="idx">{{ cat }}</option>
          </select>
          <select v-model="sortBy" class="sort-select" @change="fetchPosts">
            <option value="latest">最新</option>
            <option value="likes">点赞最多</option>
          </select>
          <button @click="fetchPosts" class="btn-search">搜索</button>
        </div>
        <section class="panel send-posts">
          <div class="panel-header" style="display:flex;align-items:center;justify-content:space-between;">
            <h3 style="margin:0;">发帖</h3>
            <button @click="togglePostEditor" class="collapse-btn">
              {{ showPostEditor ? '收起' : '展开' }}
            </button>
          </div>
          <div v-show="showPostEditor">
            <div class="editor-body">
              <input type="text" placeholder="输入标题..." class="title-input" v-model="newPost.title" />
              <div class="post-type-selector">
                <label>帖子类型：</label>
                <select v-model="newPost.postType">
                  <option v-for="(cat, idx) in categories.slice(1)" :key="idx" :value="idx">{{ cat }}</option>
                </select>
              </div>
              <textarea placeholder="分享你的想法..." class="content-input" v-model="newPost.content"></textarea>
            </div>
            <div class="media-upload">
              <label for="image-upload" class="upload-label">添加图片</label>
              <input type="file" id="image-upload" multiple accept="image/*" @change="handleImageUpload" style="display: none;" />
              <div class="preview-container" v-if="newPost.images && newPost.images.length">
                <div v-for="(img, index) in newPost.images" :key="index" class="preview-item">
                  <img :src="img.preview" alt="预览" class="preview-img" v-if="img.preview" />
                  <button @click="removeImage(index)" class="remove-btn">×</button>
                </div>
              </div>
            </div>
            <div class="media-upload">
              <label for="video-upload" class="upload-label">添加视频</label>
              <input type="file" id="video-upload" accept="video/*" @change="handleVideoUpload" style="display: none;" />
              <div v-if="newPost.video" class="preview-container">
                <div class="preview-item">
                  <video :src="newPost.video.preview" class="preview-video" controls></video>
                  <button @click="removeVideo" class="remove-btn">×</button>
                </div>
              </div>
            </div>
            <div class="editor-actions">
              <button class="btn">草稿</button>
              <button class="btn btn-primary" @click="createPost" :disabled="isPosting">
                {{ isPosting ? '发布中...' : '发布' }}
              </button>
            </div>
          </div>
        </section>
        <div class="panel posts-window">
          <div v-if="loadingPosts" class="loading">加载中...</div>
          <div v-else-if="posts.length === 0" class="no-posts">暂无帖子</div>
          <div v-else class="post-list">
            <div v-for="post in posts" :key="post.id" class="post-item" :id="'post-' + post.id">
              <div class="post-header">
                <div class="post-author">
                  <img :src="`${baseApiUrl}/api/UserInfo/imageproxy?path=${post.authorAvatar}`"
                    alt="avatar" class="post-author-avatar"
                    @error="() => post._avatarSrc = '/static/img/default-avatar.png'"
                  />
                  <span>{{ post.author }}</span>
                </div>
                <div class="post-time">{{ formatDate(post.createTime) }}</div>
              </div>
              <h3 class="post-title">{{ post.title }}</h3>
              <div class="post-content">{{ post.content }}</div>
              <div v-if="post.images && post.images.length" class="post-images">
                <img v-for="(img, idx) in post.images"
                  :src="`${baseApiUrl}/api/UserInfo/imageproxy?path=${img}`"
                  :key="idx" class="post-thumb"
                  @click="showImageModal(`${baseApiUrl}/api/UserInfo/imageproxy?path=${img}`)"
                />
              </div>
              <div v-if="post.videos && post.videos.length">
                <video v-for="(vid, idx) in post.videos"
                       :src="`${baseApiUrl}/api/UserInfo/imageproxy?path=${vid}`"
                       controls :key="idx" class="post-thumb" style="width:120px;height:120px;" />
              </div>
              <div v-if="post.hotScore !== undefined" class="post-hot-score">
                热度：{{ post.hotScore ?? 0 }}
              </div>
              <div class="post-footer">
                <button class="post-action" :class="{ liked: post.likedByMe }"
                  @click="toggleLike(post)" :disabled="post.liking">
                  {{ post.likedByMe ? '已点赞' : '点赞' }} {{ post.likeCount }}
                </button>
                <span class="post-action">评论 {{ getCommentCount(post) }}</span>
                <span class="post-action">浏览 {{ post.view_count ?? post.viewCount }}</span>
                <span class="post-action" @click="reportPost(post)">举报</span>
              </div>
              <button
                class="toggle-comments-btn"
                @click="toggleComments(post)"
                style="margin: 8px 0 4px 0;"
              >
                {{ post._showComments ? `收起评论（${getCommentCount(post)}）` : `展开评论（${getCommentCount(post)}）` }}
              </button>
              <Comments v-if="post._showComments" :postId="post.id" />
            </div>
          </div>
        </div>
      </main>
      <aside class="sidebar right">
        <section class="panel">
          <h3>热帖排行榜</h3>
          <div class="rank-list hot-rank-list">
            <div
              class="rank-item hot-rank-item"
              v-for="(post, index) in sortedHotPosts"
              :key="post.id"
            >
              <span :class="['rank-number', { 'top-rank': index < 3 }]">{{ index + 1 }}</span>
              <a
                @click.prevent="scrollToPost(post.id)"
                class="hot-post-title-ellipsis"
                :title="post.title"
              >{{ post.title }}</a>
              <span class="stat-value">热度：{{ post.hotScore ?? 0 }}</span>
            </div>
          </div>
        </section>
      </aside>
    </div>
    <div v-if="imageModal.visible" class="image-modal" @click.self="closeImageModal">
      <img :src="imageModal.imgSrc" class="big-image" />
    </div>
  </div>
</template>

<script>
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import Comments from '../Post_components/Comments.vue';
import axios from "axios";
import { useRouter } from 'vue-router';
import { useUserStore } from "../../store/user";
import { ref, reactive, onMounted, computed } from "vue";

dayjs.extend(utc);
dayjs.extend(timezone);

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";

export default {
  name: "Posts",
  components: { Comments },
  setup(_, { emit }) {
    const userStore = useUserStore();
    const userName = ref(userStore.user ? userStore.user.username : "未登录");
    const userLevel = ref(userStore.user ? userStore.user.level : 0);
    const userExp = ref(userStore.user ? userStore.user.experience : 0);
    const userToken = ref(userStore.token);

    userStore.$subscribe(() => {
      userName.value = userStore.user ? userStore.user.username : "未登录";
      userLevel.value = userStore.user ? userStore.user.level : 0;
      userExp.value = userStore.user ? userStore.user.experience : 0;
      userToken.value = userStore.token;
    });

    const stats = reactive({
      registered: 0,
      activeToday: 0,
      totalPosts: 0,
    });

    const onlineUsers = ref([]);
    const rankedUsers = ref([]);

    const newPost = reactive({
      title: "",
      content: "",
      postType: 0,
      images: [],
      videos: [],
      video: null,
    });

    const isPosting = ref(false);
    const categories = ref(["全部", "柴圈", "游戏", "其他"]);
    const currentCategory = ref("全部");
    const loadingPosts = ref(false);
    const posts = ref([]);
    const keyword = ref('');
    const selectedType = ref('');
    const sortBy = ref('latest');

    const imageModal = reactive({
      visible: false,
      imgSrc: ""
    });
    function showImageModal(src) {
      imageModal.visible = true;
      imageModal.imgSrc = src;
    }
    function closeImageModal() {
      imageModal.visible = false;
      imageModal.imgSrc = "";
    }

    const showPostEditor = ref(false);
    function togglePostEditor() {
      showPostEditor.value = !showPostEditor.value;
    }

    const hotPosts = ref([]);

    const fetchHotPosts = async () => {
      try {
        const res = await axios.get(`${baseApiUrl}/api/Posts/hot?count=10`);
        hotPosts.value = Array.isArray(res.data) ? res.data : [];
      } catch {
        hotPosts.value = [];
      }
    };

    const sortedHotPosts = computed(() => {
      return [...hotPosts.value].sort((a, b) => ((b.hotScore ?? 0) - (a.hotScore ?? 0)));
    });

    function scrollToPost(postId) {
      const el = document.getElementById('post-' + postId);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });

        
      }
    }

    const router = useRouter();
    function goToPost(postId) {
      router.push(`/post/${postId}`);
    }

    const fetchStats = async () => {
      try {
        const res = await axios.get(`${baseApiUrl}/api/Auth/total-users`);
        stats.registered = typeof res.data === "number" ? res.data : 0;
      } catch {
        stats.registered = 0;
      }
      try {
        const res = await axios.get(`${baseApiUrl}/api/UserInfo/active-today`);
        stats.activeToday = typeof res.data === "number" ? res.data : 0;
      } catch {
        stats.activeToday = 0;
      }
      try {
        const res = await axios.get(`${baseApiUrl}/api/Posts`);
        stats.totalPosts = res.data && typeof res.data.total === "number"
          ? res.data.total
          : 0;
      } catch {
        stats.totalPosts = 0;
      }
    };

    const fetchOnlineUsers = async () => {
      try {
        const res = await axios.get(`${baseApiUrl}/api/UserInfo/online`);
        onlineUsers.value = Array.isArray(res.data) ? res.data : [];
      } catch {
        onlineUsers.value = [];
      }
    };

    const fetchRankedUsers = async () => {
      try {
        const res = await axios.get(`${baseApiUrl}/api/UserInfo/rankings`);
        rankedUsers.value = Array.isArray(res.data) ? res.data : [];
      } catch {
        rankedUsers.value = [];
      }
    };

    const fetchPosts = async () => {
        loadingPosts.value = true;
        try {
          const headers = userToken.value ? { Authorization: `Bearer ${userToken.value}` } : {};
          const params = {
            keyword: keyword.value || undefined,
            postType: selectedType.value !== '' ? selectedType.value : undefined,
            sort: sortBy.value || undefined,
          };
          const res = await axios.get(`${baseApiUrl}/api/Posts/list`, { headers, params });
          console.log('后端返回的帖子数据', res.data);
          posts.value = Array.isArray(res.data)
            ? res.data.map(post => {
                return {
                  ...post,
                  _avatarSrc: post.authorAvatar
                }
              })
            : [];
        } catch {
          posts.value = [];
        }
        loadingPosts.value = false;
    };

    const reportPost = async (post) => {
      if (!userToken.value) {
        alert("请先登录再举报");
        return;
      }
      let reason = prompt("请输入举报理由（可选）：");
      try {
        await axios.post(
          `${baseApiUrl}/api/Posts/report`,
          {
            PostId: post.id,
            Reason: reason
          },
          {
            headers: { Authorization: `Bearer ${userToken.value}` }
          }
        );
        alert("举报成功，感谢你对太初寰宇社区做出的贡献！");
      } catch (e) {
        alert(e.response?.data || "举报失败");
      }
    };

    const toggleLike = async (post) => {
      if (!userToken.value) {
        alert("请先登录再点赞");
        return;
      }
      post.liking = true;
      try {
        if (post.likedByMe) {
          const res = await axios.post(
            `${baseApiUrl}/api/Posts/unlike`,
            post.id,
            {
              headers: {
                Authorization: `Bearer ${userToken.value}`,
                "Content-Type": "application/json"
              }
            }
          );
          post.likeCount = res.data.likeCount;
          post.likedByMe = false;
        } else {
          const res = await axios.post(
            `${baseApiUrl}/api/Posts/like`,
            post.id,
            {
              headers: {
                Authorization: `Bearer ${userToken.value}`,
                "Content-Type": "application/json"
              }
            }
          );
          post.likeCount = res.data.likeCount;
          post.likedByMe = true;
        }
      } catch (e) {
        alert(e.response?.data || "操作失败");
      } finally {
        post.liking = false;
      }
    };

    const formatDate = (timestamp) => {
        if (!timestamp) return "";
        return dayjs.utc(timestamp).tz('Asia/Shanghai').format('YYYY-MM-DD HH:mm:ss');
    };

    const uploadMedia = async () => {
      const formData = new FormData();
      newPost.images.forEach(img => formData.append("images", img.file));
      if (newPost.video && newPost.video.file) {
        formData.append("videos", newPost.video.file);
      }
      newPost.videos.forEach(vid => formData.append("videos", vid.file));
      if (!formData.has("images") && !formData.has("videos")) {
        return { images: [], videos: [] };
      }
      const uploadRes = await axios.post(
        `${baseApiUrl}/api/Posts/UploadMedia?userId=${userStore.user.id}`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${userToken.value}`,
          }
        }
      );
      return {
        images: uploadRes.data.images || [],
        videos: uploadRes.data.videos || [],
      };
    };

    const createPost = async () => {
      if (isPosting.value) return;
      isPosting.value = true;
      try {
        if (!userToken.value) {
          alert("请先登录");
          return;
        }
        let imagePaths = [];
        let videoPaths = [];
        if (
          (newPost.images && newPost.images.length > 0) ||
          (newPost.videos && newPost.videos.length > 0) ||
          (newPost.video && newPost.video.file)
        ) {
          const mediaRes = await uploadMedia();
          imagePaths = mediaRes.images;
          videoPaths = mediaRes.videos;
        }
        const postPayload = {
          Title: newPost.title,
          Content: newPost.content,
          PostType: newPost.postType,
          Images: imagePaths,
          Videos: videoPaths,
        };
        const res = await axios.post(`${baseApiUrl}/api/Posts/create`, postPayload, {
          headers: {
            Authorization: `Bearer ${userToken.value}`,
          },
        });
        if (res.status === 200) {
          alert("发布成功 ✅");
          newPost.title = "";
          newPost.content = "";
          newPost.postType = 0;
          newPost.images = [];
          newPost.videos = [];
          newPost.video = null;
          await refreshUserInfo();
          emit("post-created");
          await fetchPosts();
        }
      } catch (err) {
        const msg =
          err.response?.data && typeof err.response.data === "string"
            ? err.response.data
            : JSON.stringify(err.response?.data ?? {});
        alert(`发布失败: ${msg}`);
      } finally {
        isPosting.value = false;
      }
    };

    const refreshUserInfo = async () => {
      if (!userStore.user?.id) return;
      try {
        const res = await axios.get(`${baseApiUrl}/api/UserInfo?userId=${userStore.user.id}`);
        if (res.data) {
          userStore.updateUser(res.data);
        }
      } catch (e) {
      }
    };

    const handleImageUpload = (event) => {
      const files = event.target.files;
      if (!files || files.length === 0) return;
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        if (file.type.startsWith("image/")) {
          const reader = new FileReader();
          reader.onload = (e) => {
            newPost.images.push({
              file: file,
              preview: e.target.result,
            });
          };
          reader.readAsDataURL(file);
        }
      }
    };

    const handleVideoUpload = (event) => {
      const file = event.target.files[0];
      if (file && file.type.startsWith("video/")) {
        const reader = new FileReader();
        reader.onload = (e) => {
          newPost.video = {
            file: file,
            preview: e.target.result,
          };
        };
        reader.readAsDataURL(file);
      }
    };

    const removeImage = (index) => {
      if (Array.isArray(newPost.images)) {
        newPost.images.splice(index, 1);
      }
    };

    const removeVideo = () => {
      newPost.video = null;
    };

    function toggleComments(post) {
      post._showComments = !post._showComments;
    }

    function getCommentCount(post) {
      return post.comment_count ?? post.commentCount ?? 0;
    }

    onMounted(() => {
      fetchStats();
      fetchOnlineUsers();
      fetchRankedUsers();
      fetchPosts();
      fetchHotPosts();
    });

    return {
      userName,
      userLevel,
      userExp,
      userToken,
      stats,
      onlineUsers,
      rankedUsers,
      newPost,
      isPosting,
      createPost,
      toggleLike,
      reportPost,
      handleImageUpload,
      handleVideoUpload,
      removeImage,
      removeVideo,
      categories,
      currentCategory,
      loadingPosts,
      posts,
      baseApiUrl,
      imageModal,
      showImageModal,
      closeImageModal,
      formatDate,
      keyword,
      selectedType,
      sortBy,
      fetchPosts,
      scrollToPost,
      hotPosts,
      fetchHotPosts,
      showPostEditor,
      togglePostEditor,
      sortedHotPosts,
      goToPost,
      toggleComments,
      getCommentCount,
    };
  },
};
</script>

<style scoped>
@import url('/static/css/Posts.css');
.collapse-btn {
  background: none;
  border: none;
  color: #409eff;
  cursor: pointer;
  font-size: 14px;
  padding: 2px 8px;
}
.post-hot-score {
  margin-top: 4px;
  color: #ff9800;
  font-size: 13px;
}
.toggle-comments-btn {
  background: none;
  border: none;
  color: #409eff;
  cursor: pointer;
  font-size: 14px;
  margin-bottom: 4px;
}
.hot-rank-list {
  width: 100%;
  min-width: 0;
}
.hot-rank-item {
  display: flex;
  align-items: center;
  padding: 4px 0;
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
}
.rank-number {
  min-width: 15px;
  text-align: center;
  margin-right: 8px;
  font-weight: bold;
  flex-shrink: 0;
}
.hot-post-title-ellipsis {
  flex: 1 1 0%;
  min-width: 0;
  margin-right: 10px;
  color: #1976d2;
  text-decoration: underline;
  cursor: pointer;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 140px;
  margin-left: 2px;
}
.stat-value {
  flex-shrink: 0;
  font-size: 12px;
  color: #ff9800;
  margin-left: 6px;
}
.sidebar.right {
  width: 300px;
  min-width: 0;
}
@media (max-width: 800px) {
  .hot-rank-list {
    width: 100%;
  }
  .hot-post-title-ellipsis {
    max-width: 80px;
  }
}
</style>