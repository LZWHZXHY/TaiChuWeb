<template>
  <div class="comment-item">
    <div class="avatar">
      <img :src="userAvatarUrl(comment)" alt="头像" class="comment-avatar" />
    </div>
    <div class="body">
      <div class="meta">
        <span class="name">{{ comment.userName }}</span>
        <span class="time">{{ formatDate(comment.createTime) }}</span>
      </div>
      <div class="content">{{ comment.content }}</div>
      <div class="actions">
        <span class="reply-btn" @click="showReply = !showReply">回复</span>
        <span class="report-btn" @click="reportComment">举报</span>
      </div>
      <div v-if="showReply" class="reply-box">
        <textarea v-model="replyContent" rows="2" placeholder="回复内容"></textarea>
        <button class="btn" :disabled="replying" @click="submitReply">提交</button>
        <button class="btn" @click="cancelReply">取消</button>
      </div>
      <div class="replies" v-if="comment.replies && comment.replies.length">
        <CommentItem
          v-for="child in comment.replies"
          :key="child.id"
          :comment="child"
          :post-id="postId"
          @refresh="$emit('refresh')"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits } from "vue";
import axios from "axios";
import { useUserStore } from "../../store/user";

// 自动根据环境选择后端api地址
const baseApiUrl = import.meta.env.DEV 
  ? "https://localhost:44321"   // 本地开发环境
  : "https://bianyuzhou.com";   // 生产环境

const props = defineProps({
  comment: { type: Object, required: true },
  postId: { type: Number, required: true }
});
const emit = defineEmits(["refresh"]);
const userStore = useUserStore();
const userToken = userStore.token;
const showReply = ref(false);
const replyContent = ref("");
const replying = ref(false);

const formatDate = (val) => {
  if (!val) return "";
  return new Date(val).toLocaleString();
};
const reportComment = async () => {
  if (!userToken) {
    alert("请先登录再举报评论");
    return;
  }
  let reason = prompt("请输入举报理由（可选）：");
  try {
    await axios.post(
      `${baseApiUrl}/api/Comment/report`,
      {
        CommentId: props.comment.id,
        Reason: reason
      },
      {
        headers: { Authorization: `Bearer ${userToken}` }
      }
    );
    alert("举报成功，感谢你对太初寰宇社区做出的贡献！");
  } catch (e) {
    alert(e.response?.data || "举报失败");
  }
};
// 关键：头像路径处理，兼容本地服务器与正式服务器
const userAvatarUrl = (comment) => {
  // comment.userAvatar 形如 "avatars/110.png"
  if (!comment.userAvatar) {
    // 没头像，走默认
    return `${baseApiUrl}/api/UserInfo/imageproxy?path=default-avatar&t=${Date.now()}`;
  }
  return `${baseApiUrl}/api/UserInfo/imageproxy?path=${encodeURIComponent(comment.userAvatar)}&t=${Date.now()}`;
};

const submitReply = async () => {
  if (!replyContent.value.trim()) {
    alert("回复不能为空！");
    return;
  }
  replying.value = true;
  try {
    await axios.post(
      `${baseApiUrl}/api/Comment`,
      {
        postId: props.postId,
        content: replyContent.value,
        parentCommentId: props.comment.id
      },
      { headers: { Authorization: `Bearer ${userToken}` } }
    );
    replyContent.value = "";
    showReply.value = false;
    emit("refresh");
  } catch (err) {
    alert("回复失败：" + (err.response?.data || err.message));
  } finally {
    replying.value = false;
  }
};
const cancelReply = () => {
  showReply.value = false;
  replyContent.value = "";
};
</script>

<style scoped>
.report-btn {
  color: #e74c3c;
  cursor: pointer;
  font-size: 13px;
  margin-left: 12px;
}
.report-btn:hover {
  text-decoration: underline;
}
.comment-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  background: #eee;
}
.avatar {
  margin-right: 12px;
  display: flex;
  align-items: center;
}
.comment-item {
  display: flex;
  margin-bottom: 16px;
  align-items: flex-start;
}
.body {
  flex: 1;
}
.meta {
  color: #888;
  font-size: 12px;
  margin-bottom: 2px;
}
.name {
  font-weight: bold;
  margin-right: 8px;
  color: #333;
}
.time {
  color: #aaa;
}
.content {
  margin-bottom: 6px;
  font-size: 15px;
}
.reply-btn {
  color: #409eff;
  cursor: pointer;
  font-size: 13px;
}
.reply-box {
  margin-top: 6px;
}
.btn {
  margin-right: 8px;
}
.replies {
  margin-left: 32px;
  border-left: 2px solid #f0f0f0;
  padding-left: 8px;
}
</style>