<template>
  <div class="version-group">
    <div class="version-tag">{{ node.version }}</div>
    <div v-if="node.logs && node.logs.length > 0">
      <div v-for="log in node.logs" :key="log.id" class="single-log-card">
        <div class="log-header">
          <span class="log-version">{{ log.version }}</span>
          <span class="log-type" :class="'type-' + log.type">{{ typeMap[log.type] || '其他' }}</span>
          <span class="log-date">{{ formatDate(log.created_at) }}</span>
        </div>
        <div class="log-title">{{ log.title }}</div>
        <div class="log-content">{{ log.content }}</div>
      </div>
    </div>
    <div v-if="node.children && node.children.length > 0" class="log-children">
      <LogNode v-for="child in node.children" :key="child.version" :node="child" />
    </div>
  </div>
</template>

<script setup>
const props = defineProps({ node: Object })
const typeMap = {
  1: 'Bug修复',
  2: '小内容更新',
  3: '网站优化',
  4: '重大更新',
  5: '其他'
}
function formatDate(ts) {
  if (!ts) return '-'
  const d = new Date(ts)
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.version-group {
  margin-bottom: 26px;
  padding-left: 0;
}
.version-tag {
  font-size: 1.18rem;
  font-weight: 700;
  color: #44a3eb;
  text-shadow: 0 1px 7px rgba(80,180,255,0.10);
  margin-bottom: 12px;
  margin-left: 2px;
  position: relative;
  letter-spacing: .4px;
}
.log-children {
  margin-left: 32px;
}
.single-log-card {
  background: #fff;
  box-shadow: 0 2px 14px rgba(40,130,220,0.09);
  border-radius: 10px;
  padding: 16px 18px 8px 15px;
  margin-bottom: 14px;
  border-left: 5px solid #46acef;
  border-bottom: 1.6px solid #e5edf6;
  transition: box-shadow .16s;
}
.single-log-card:hover {
  box-shadow: 0 7px 32px rgba(40,130,220,0.18);
  border-left: 5px solid #2376ee;
}
.log-header {
  display: flex;
  align-items: center;
  gap: 14px;
  font-size: 0.97rem;
  margin-bottom: 2px;
}
.log-version {
  background: linear-gradient(90deg,#2376ee, #5bc5fa 90%);
  color: #fff;
  font-size: 0.98rem;
  font-weight: 600;
  border-radius: 5px;
  padding: 1px 8px;
  margin-right: 3px;
  letter-spacing: .3px;
}
.log-type {
  padding: 2px 12px;
  border-radius: 4px;
  font-size: 0.97rem;
  font-weight: 600;
  margin-right: 6px;
  letter-spacing: .28px;
}
.type-1 { background: #d5eafe; color: #2376ee; }
.type-2 { background: #e3ffef; color: #17b968; }
.type-3 { background: #fef9db; color: #ea9907; }
.type-4 { background: #ffe1e1; color: #d63e3e; }
.type-5 { background: #f3f3f3; color: #666; }
.log-date {
  color: #979797;
  font-size: 0.92rem;
}
.log-title {
  font-size: 1.08rem;
  font-weight: 600;
  color: #256cc1;
  margin-bottom: 6px;
  margin-top: 3px;
  letter-spacing: 0.2px;
}
.log-content {
  color: #244e6c;
  font-size: 1rem;
  margin-bottom: 3px;
  word-break: break-word;
}
</style>