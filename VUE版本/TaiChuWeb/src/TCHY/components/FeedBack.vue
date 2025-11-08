<template>
  <div class="BigContainer">
    <div class="topBar">
      太初寰宇建议收集箱
    </div>
    <div class="main-layout">
      <!-- 全部意见展示区域：左侧可滚动 -->
      <div class="all-feedback-list">
        <h3>全部意见展示</h3>
        <div v-if="feedbackList.length === 0" class="no-feedback">暂无意见</div>
        <div class="feedback-card" v-for="item in feedbackList" :key="item.id">
          <div class="status-circle" :style="{background: statusMeta[item.status]?.color}">
            <span class="status-icon">{{ statusMeta[item.status]?.icon }}</span>
          </div>
          <div class="feedback-content">
            <div class="feedback-type">{{ categories[item.type] || '未知' }}</div>
            <div class="feedback-main">{{ item.content }}</div>
            <div class="feedback-meta">
              <span>{{ item.createTime }}</span>
              <span class="feedback-status">{{ statusMeta[item.status]?.label }}</span>
            </div>
          </div>
        </div>
      </div>
      <!-- 中间表单 -->
      <div class="content">
        <h2>建议收集</h2>
        <label for="category">请选择建议板块：</label>
        <select v-model="selectedCategory" id="category">
          <option disabled value="">请选择</option>
          <option v-for="cat in categories" :key="cat" :value="cat">
            {{ cat }}
          </option>
        </select>

        <label for="feedback" class="feedback-label">请填写您的意见：</label>
        <textarea
          id="feedback"
          v-model="feedback"
          placeholder="请输入您的宝贵意见"
          rows="6"
        ></textarea>

        <button
          @click="submitFeedback"
          :disabled="userLevel < 2 || cooldownLeft > 0"
          :title="userLevel < 2 ? '等级达2级后才可提交意见' : (cooldownLeft > 0 ? '请等待冷却时间' : '')"
        >
          提交意见
        </button>
        <div v-if="userLevel < 2" class="level-tip">
          您当前等级不足2级，无法提交意见。
        </div>
        <div v-else-if="cooldownLeft > 0" class="level-tip">
          请等待 {{ Math.floor(cooldownLeft / 60) }}分{{ cooldownLeft % 60 }}秒后再提交。
        </div>
        <div v-if="submitted" class="success">
          感谢您的反馈！
        </div>
      </div>
      <!-- 右侧解释 -->
      <div class="explanation">
        <h3>为什么需要2级才能使用建议箱？</h3>
        <p>
          为了保障建议箱的有效性和管理效率，避免大量无效、恶意或垃圾信息的产生，我们要求用户在等级达到2级后才可以提交建议。这样可以确保您已经有一定的活跃度和了解，您的建议更具参考价值。感谢您的理解和支持！
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import { useUserStore } from '../../store/user'

// 分类与type的映射
const categoryTypeMap = {
  "其他": 0,
  "BUG反馈": 1,
  "社区发展建议": 2,
  "功能建议": 3,
  "动画项目建议": 4,
  "游戏项目建议": 5
};
const categories = Object.keys(categoryTypeMap);

const selectedCategory = ref("");
const feedback = ref("");
const submitted = ref(false);

const userStore = useUserStore();
const userLevel = computed(() => userStore.user?.level ?? 0);

// 发送冷却相关
const COOLDOWN_MINUTES = 5;
const lastSubmitKey = "feedbackLastSubmitTime";
const cooldownLeft = ref(0);

function checkCooldown() {
  const last = localStorage.getItem(lastSubmitKey);
  if (last) {
    const lastTime = parseInt(last, 10);
    const now = Date.now();
    const diff = now - lastTime;
    if (diff < COOLDOWN_MINUTES * 60 * 1000) {
      cooldownLeft.value = Math.ceil((COOLDOWN_MINUTES * 60 * 1000 - diff) / 1000);
      return true;
    }
  }
  cooldownLeft.value = 0;
  return false;
}

function startCooldownTimer() {
  cooldownLeft.value = COOLDOWN_MINUTES * 60;
  const timer = setInterval(() => {
    if (cooldownLeft.value > 0) {
      cooldownLeft.value--;
    } else {
      clearInterval(timer);
    }
  }, 1000);
}

// ===== 全部意见相关 =====
const feedbackList = ref([]);
const statusMeta = [
  { color: "#eee", icon: "▢", label: "已提交" },      // 0
  { color: "#ffb347", icon: "≡", label: "已知晓" },   // 1
  { color: "#43b244", icon: "✓", label: "已采纳/已修复" }, // 2
  { color: "#ff5757", icon: "×", label: "驳回/不予采纳" } // 3
];
async function fetchFeedbackList() {
  const res = await axios.get((import.meta.env.DEV ? "https://localhost:44321" : "https://bianyuzhou.com") + "/api/FeedBack/all");
  feedbackList.value = Array.isArray(res.data) ? res.data : [];
}

function submitFeedback() {
  if (userLevel.value < 2) {
    alert("您的等级未达到2级，无法提交意见。");
    return;
  }
  if (!selectedCategory.value || !feedback.value.trim()) {
    alert("请选择板块并填写意见！");
    return;
  }
  if (checkCooldown()) {
    const mins = Math.floor(cooldownLeft.value / 60);
    const secs = cooldownLeft.value % 60;
    alert(`请等待${mins > 0 ? mins + '分' : ''}${secs}秒后再提交。`);
    return;
  }
  const typeValue = categoryTypeMap[selectedCategory.value];
  axios.post(
    (import.meta.env.DEV ? "https://localhost:44321" : "https://bianyuzhou.com") + "/api/FeedBack",
    {
      type: typeValue,
      content: feedback.value
    },
    {
      headers: {
        Authorization: 'Bearer ' + userStore.token
      }
    }
  ).then(() => {
    localStorage.setItem(lastSubmitKey, Date.now().toString());
    submitted.value = true;
    startCooldownTimer();
    fetchFeedbackList(); // 提交后刷新全部意见
    setTimeout(() => {
      submitted.value = false;
      selectedCategory.value = "";
      feedback.value = "";
    }, 1500);
  }).catch(err => {
    if (err.response && err.response.status === 403) {
      alert(
        (typeof err.response.data === "string" && err.response.data) ||
        "您的等级未达到2级，无法提交意见。"
      );
    } else if (err.response && err.response.status === 401) {
      alert("未登录或登录已失效，请重新登录。");
    } else {
      alert("提交失败，请稍后再试。");
    }
  });
}

// 页面加载时检查一次冷却与拉取全部意见
checkCooldown();
if (cooldownLeft.value > 0) {
  startCooldownTimer();
}
onMounted(fetchFeedbackList);
</script>

<style scoped>
.BigContainer {
  min-height: 100vh;
  background: #f5f7fa;
  font-family: 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif;
}

.topBar {
  width: 100%;
  height: 60px;
  line-height: 60px;
  position: sticky;
  top: 0;
  background-color: #ffffff;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
  font-size: 20px;
  text-align: center;
  font-weight: bold;
  z-index: 10;
}

.main-layout {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  max-width: 1200px;
  margin: 80px auto 0;
  gap: 32px;
}

.all-feedback-list {
  flex: 0 0 340px;
  max-height: 650px;
  overflow-y: auto;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
  padding: 20px 18px;
  margin-top: 0;
  display: flex;
  flex-direction: column;
}
.all-feedback-list h3 {
  margin-bottom: 18px;
  font-size: 18px;
  color: #298ddc;
  position: sticky;
  top: 0;
  background: #fff;
  z-index: 2;
  padding-bottom: 8px;
}
.feedback-card {
  display: flex;
  align-items: flex-start;
  margin-bottom: 14px;
  background: #f8fafc;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04);
  padding: 10px 14px;
}
.status-circle {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  margin-right: 13px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.status-icon {
  font-size: 20px;
  font-weight: bold;
  color: #fff;
  display: block;
}
.feedback-content {
  flex: 1;
  min-width: 0;
}
.feedback-type {
  font-size: 13px;
  color: #999;
  margin-bottom: 3px;
}
.feedback-main {
  font-size: 15px;
  color: #222;
  margin-bottom: 2px;
  word-break: break-word;
}
.feedback-meta {
  font-size: 12px;
  color: #888;
  margin-bottom: 2px;
  display: flex;
  gap: 16px;
  align-items: center;
}
.feedback-status {
  font-weight: bold;
}
.no-feedback {
  color: #bbb;
  text-align: center;
}

.content {
  flex: 1 1 0;
  min-width: 350px;
  max-width: 480px;
  padding: 32px 24px 24px 24px;
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 6px 24px rgba(0,0,0,0.08);
}

.explanation {
  flex: 0 0 320px;
  padding: 32px 20px 24px 20px;
  background: #fffdf5;
  border-radius: 14px;
  box-shadow: 0 6px 24px rgba(0,0,0,0.06);
  margin-top: 0;
  font-size: 15px;
  color: #7a6e55;
}
.explanation p{
    padding-top: 5px;
}

.level-tip {
  color: #d99e2b;
  margin-top: 10px;
  text-align: center;
  font-size: 14px;
}

label {
  display: block;
  margin: 16px 0 6px 0;
  font-weight: 500;
}

select, textarea {
  width: 100%;
  margin-bottom: 16px;
  padding: 8px;
  border: 1px solid #d6dee4;
  border-radius: 6px;
  font-size: 15px;
  resize: none;
}

textarea {
  min-height: 96px;
}

button {
  display: block;
  width: 100%;
  padding: 10px 0;
  background: #36a2f5;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  margin-top: 8px;
  transition: background 0.2s;
}
button:disabled {
  background: #b8c6d8;
  cursor: not-allowed;
}
button:hover:enabled {
  background: #298ddc;
}

.success {
  margin-top: 18px;
  color: #43b244;
  font-weight: bold;
  text-align: center;
}
</style>