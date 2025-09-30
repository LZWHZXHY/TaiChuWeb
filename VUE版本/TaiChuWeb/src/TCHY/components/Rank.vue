<template>
  <div class="ranking-container">
    <h2>排行榜</h2>
    <div class="tab-bar">
      <button
        v-for="type in rankingTypes"
        :key="type.value"
        @click="selectType(type.value)"
        :class="{ active: currentType === type.value }"
      >
        {{ type.label }}
      </button>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else>
      <div class="table-scroll" v-if="rankings.length">
        <table class="ranking-table">
          <thead>
            <tr>
              <th>排名</th>
              <th>头像</th>
              <th>用户名</th>
              <th v-if="currentType === 'level'">等级</th>
              <th v-if="currentType === 'level'">经验</th>
              <th v-if="currentType === 'active'">活跃度</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(user, idx) in rankings" :key="user.id">
              <td>{{ idx + 1 }}</td>
              <td>
                <img :src="logoUrl(user.logo)" alt="头像" class="avatar" />
              </td>
              <td>{{ user.username }}</td>
              <td v-if="currentType === 'level'">{{ user.level }}</td>
              <td v-if="currentType === 'level'">{{ user.exp }}</td>
              <td v-if="currentType === 'active'">{{ user.active }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="empty">暂无数据</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321' 
  : 'https://bianyuzhou.com';

const rankingTypes = [
  { label: "等级排行榜", value: "level" },
  { label: "周活跃排行榜", value: "active" },
  { label: "未知排行榜", value: "unknown" }

];

const currentType = ref(rankingTypes[0].value);
const rankings = ref([]);
const loading = ref(false);

function logoUrl(logo) {
  // 如果 logo 已经是完整 http(s) 路径就直接返回
  if (logo.startsWith('http://') || logo.startsWith('https://')) return logo;
  // 否则拼接 baseApiUrl
  return `${baseApiUrl}${logo}`;
}

async function fetchRankings(type) {
  loading.value = true;
  try {
    const res = await axios.get(`${baseApiUrl}/api/UserInfo/LevelRanking?type=${type}`);
    rankings.value = res.data || [];
  } catch (e) {
    rankings.value = [];
  }
  loading.value = false;
}

function selectType(type) {
  if (type !== currentType.value) {
    currentType.value = type;
    fetchRankings(type);
  }
}

onMounted(() => {
  fetchRankings(currentType.value);
});
</script>

<style scoped>
.ranking-container {
  max-width: 650px;
  margin: 40px auto 0;
  background: #fff;
  border-radius: 15px;
  box-shadow: 0 4px 24px rgba(60,90,150,0.06);
  padding: 32px 20px 24px 20px;
  font-family: 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif;
}

.tab-bar {
  display: flex;
  gap: 16px;
  margin-bottom: 18px;
  justify-content: center;
}

.tab-bar button {
  background: #f0f4fa;
  border: none;
  border-radius: 6px 6px 0 0;
  padding: 8px 32px;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  color: #667;
  transition: background 0.2s, color 0.2s;
}
.tab-bar button.active {
  background: #36a2f5;
  color: #fff;
}

.table-scroll {
  max-height: 480px;
  overflow-y: auto;
  border-radius: 8px;
  border: 1px solid #f2f3f5;
}
.ranking-table {
  width: 100%;
  border-collapse: collapse;
  text-align: center;
}
.ranking-table th, .ranking-table td {
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}
.ranking-table th {
  background: #f8fafd;
  color: #222;
  font-weight: bold;
}
.ranking-table tr:nth-child(odd) {
  background: #f8fbfd;
}
.avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  object-fit: cover;
  background: #eef3fa;
  border: 1.5px solid #e2e6ec;
}
.loading, .empty {
  text-align: center;
  color: #aaa;
  margin: 32px 0;
  font-size: 18px;
}
</style>