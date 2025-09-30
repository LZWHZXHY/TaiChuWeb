<template>
  <div>
    <h2>世界观基本规则</h2>
    <p>这里是世界规则，其本质也可以理解为世间的一种固定现象</p>
    <div v-if="loading">加载中...</div>
    <div v-else-if="error">{{ error }}</div>
    <div v-else>
      <div v-for="rule in rules" :key="rule.id" class="rule-block">
        <h3 class = "rule-title">{{ rule.title }}</h3>
        <div class="rule-content" style="white-space: pre-wrap;">{{ rule.content }}</div>
        <div class="rule-update">更新时间：{{ formatDate(rule.updateTime) }}</div>
        <hr />
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

const rules = ref([]);
const loading = ref(true);
const error = ref("");

function formatDate(str) {
  // 兼容字符串或Date类型
  if (!str) return "";
  const d = typeof str === "string" ? new Date(str) : str;
  return d.toLocaleString();
}

onMounted(async () => {
  loading.value = true;
  try {
    const res = await axios.get(`${baseApiUrl}/api/WorldBasicRules`);
    // 假设后端字段和C#模型一致，可能需要调整字段名
    rules.value = res.data.map(item => ({
      id: item.id,
      title: item.title,
      content: item.content ?? item.Content, // 兼容content/Content
      updateTime: item.updateTime ?? item.UpdateTime // 兼容updateTime/UpdateTime
    }));
  } catch (e) {
    error.value = "加载失败：" + (e.response?.data?.message || e.message);
  }
  loading.value = false;
});
</script>

<style scoped>
.rule-block {
  margin: 24px 0;
  background: #fafbfc;
  border-radius: 8px;
  box-shadow: 0 2px 8px #eee;
  padding: 24px;
}
.rule-content {
  color: #222;
  font-size: 16px;
  line-height: 1.8;
}
.rule-update {
  color: #888;
  font-size: 13px;
  margin-top: 6px;
}
.rule-title{
    color: #222;
}
</style>