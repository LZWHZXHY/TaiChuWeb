    <template>
    <div class="comments-container">
        <div v-if="loading" class="loading">评论加载中...</div>
        <div v-else>
        <div v-if="comments.length === 0" class="no-comments">暂无评论</div>
        <div v-else>
            <CommentItem
            v-for="comment in comments"
            :key="comment.id"
            :comment="comment"
            :post-id="postId"
            @refresh="fetchComments"
            />
        </div>
        </div>
        <div class="comment-input" v-if="true">
        <textarea v-model="newComment" placeholder="写下你的评论..." rows="3"></textarea>
        <button class="btn btn-primary" :disabled="submitting" @click="submitComment">
            {{ submitting ? '发布中...' : '发表评论' }}
        </button>
        </div>
        <div v-else class="tip">请登录后才能发表评论</div>
    </div>
    </template>

    <script setup>
import { ref, watch, defineProps } from "vue";
import axios from "axios";
import { useUserStore } from "../../store/user";
import CommentItem from "./CommentItem.vue";

// 根据环境设置基础URL
const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321' 
  : 'https://bianyuzhou.com';

const props = defineProps({
  postId: { type: Number, required: true }
});

const userStore = useUserStore();
const userToken = ref(userStore.token);

const comments = ref([]);
const loading = ref(false);
const newComment = ref("");
const submitting = ref(false);

userStore.$subscribe(() => {
  userToken.value = userStore.token;
});

const fetchComments = async () => {
  loading.value = true;
  try {
    const res = await axios.get(`${baseApiUrl}/api/Comment?postId=${props.postId}`);
    comments.value = Array.isArray(res.data) ? res.data : [];
  } catch {
    comments.value = [];
  }
  loading.value = false;
};

const submitComment = async () => {
  if (!newComment.value.trim()) {
    alert("评论不能为空！");
    return;
  }
  submitting.value = true;
  try {
    await axios.post(
      `${baseApiUrl}/api/Comment`,
      {
        postId: props.postId,
        content: newComment.value,
        parentCommentId: null
      },
      {
        headers: { Authorization: `Bearer ${userToken.value}` }
      }
    );
    newComment.value = "";
    await fetchComments();
  } catch (err) {
    alert("评论失败：" + (err.response?.data || err.message));
  } finally {
    submitting.value = false;
  }
};

watch(
  () => props.postId,
  () => {
    fetchComments();
  },
  { immediate: true }
);
</script>

    <style scoped>
    .comments-container {
    margin-top: 20px;
    background: rgba(245, 248, 255, 0.7);
    border-radius: 12px;
    padding: 18px 16px 10px 16px;
    box-shadow: 0 2px 8px rgba(52, 152, 219, 0.08);
    }

    .no-comments {
    color: #aaa;
    text-align: center;
    padding: 20px 0 10px;
    font-size: 15px;
    }

    .loading {
    color: #3498db;
    text-align: center;
    padding: 20px 0 10px;
    font-size: 15px;
    }

    .comment-list {
    display: flex;
    flex-direction: column;
    gap: 14px;
    margin-bottom: 10px;
    }

    .comment-card {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 1px 4px rgba(52, 152, 219, 0.06);
    padding: 12px 14px;
    transition: box-shadow 0.2s, background 0.2s;
    border-left: 4px solid #8fd3f4;
    }

    .comment-card:hover {
    box-shadow: 0 4px 16px rgba(52, 152, 219, 0.15);
    background: #f5fbff;
    }

    .comment-avatar {
    width: 38px;
    height: 38px;
    border-radius: 50%;
    object-fit: cover;
    background: linear-gradient(135deg, #8fd3f4, #3498db);
    display: flex;
    align-items: center;
    justify-content: center;
    color: #fff;
    font-weight: bold;
    font-size: 20px;
    }

    .comment-main {
    flex: 1;
    display: flex;
    flex-direction: column;
    }

    .comment-header {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 4px;
    }

    .comment-author {
    font-weight: bold;
    color: #3498db;
    font-size: 15px;
    }

    .comment-time {
    color: #aaa;
    font-size: 12px;
    }

    .comment-content {
    line-height: 1.7;
    color: #333;
    font-size: 15px;
    word-break: break-word;
    margin-bottom: 4px;
    }

    .comment-footer {
    margin-top: 2px;
    font-size: 12px;
    color: #81b3d2;
    display: flex;
    gap: 16px;
    }

    .comment-input {
    margin-top: 16px;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(52,152,219,0.07);
    padding: 14px 12px 10px 12px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    }

    .comment-input textarea {
    resize: vertical;
    min-height: 60px;
    font-size: 15px;
    padding: 10px;
    border-radius: 6px;
    border: 1.5px solid #e0eaff;
    transition: border 0.2s;
    background: #f7fbfe;
    }

    .comment-input textarea:focus {
    outline: none;
    border: 1.5px solid #3498db;
    background: #fff;
    }

    .comment-input .btn {
    width: 115px;
    align-self: flex-end;
    padding: 7px 0;
    border-radius: 6px;
    background: linear-gradient(90deg, #3498db 60%, #8fd3f4 100%);
    color: #fff;
    font-weight: bold;
    border: none;
    cursor: pointer;
    transition: background 0.2s, transform 0.2s;
    font-size: 15px;
    box-shadow: 0 2px 6px rgba(52,152,219,0.12);
    }

    .comment-input .btn:disabled {
    background: #b1cff7;
    cursor: not-allowed;
    }

    .comment-input .btn:hover:enabled {
    background: linear-gradient(90deg, #217dbb 60%, #3498db 100%);
    transform: translateY(-2px) scale(1.03);
    }

    .tip {
    color: #aaa;
    text-align: center;
    margin-top: 12px;
    font-size: 15px;
    }

    @media (max-width: 700px) {
    .comments-container {
        padding: 10px 2vw 6px 2vw;
    }
    .comment-card {
        flex-direction: column;
        align-items: stretch;
        gap: 8px;
        padding: 10px 8px;
    }
    .comment-avatar {
        width: 30px;
        height: 30px;
        font-size: 16px;
    }
    .comment-input {
        padding: 9px 5px;
    }
    .comment-input .btn {
        width: 95px;
        font-size: 13px;
    }
    }
    </style>