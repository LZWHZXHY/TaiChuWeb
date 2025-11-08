<template>
  <div class="update-log-container">
    <h2>更新记录</h2>
    <div v-if="logs.length">
      <div
        v-for="log in logsSorted"
        :key="log.date + log.version"
        class="log-block"
      >
        <div class="log-header">
          <span class="log-version">{{ log.version }}</span>
          <span class="log-date">{{ log.date }}</span>
        </div>
        <ul class="log-items">
          <li
            v-for="(item, idx) in log.items"
            :key="item.title + idx"
            class="log-item"
          >
            <span class="item-title">{{ item.title }}</span>
            <span class="item-content">{{ item.content }}</span>
          </li>
        </ul>
      </div>
    </div>
    <div v-else>暂无更新记录。</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
const logs = ref([]);

// 最新的在最上面
const logsSorted = computed(() => {
  return [...logs.value].sort((a, b) => b.date.localeCompare(a.date));
});

onMounted(async () => {
  try {
    const res = await fetch("/update-log.json");
    logs.value = await res.json();
  } catch (e) {
    logs.value = [];
  }
});
</script>

<style scoped>
.update-log-container {
  max-width: 600px;
  margin: 0 auto;
  padding: 32px 0;
  font-family: 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif;
}
h2 {
  text-align: center;
  margin-bottom: 28px;
  font-weight: bold;
  color: #3a4667;
  letter-spacing: 2px;
}
.log-block {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(60,90,150,0.05);
  padding: 18px 20px 12px 20px;
  margin-bottom: 20px;
  transition: box-shadow 0.2s;
}
.log-block:hover {
  box-shadow: 0 6px 24px rgba(60,90,150,0.10);
}
.log-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}
.log-version {
  background: #36a2f5;
  color: #fff;
  border-radius: 6px;
  padding: 3px 10px;
  font-size: 13px;
  font-weight: bold;
  letter-spacing: 1px;
}
.log-date {
  color: #888;
  font-size: 13px;
}
.log-items {
  margin: 0;
  padding: 0 0 0 8px;
  list-style: none;
}
.log-item {
  margin-bottom: 6px;
  line-height: 1.65;
}
.item-title {
  font-weight: bold;
  color: #293d58;
}
.item-content {
  color: #5a5a5a;
  margin-left: 0.5em;
  word-break: break-all;
}
</style>