<template>
  <div class="BigContainer">
    <div class="Header">
      <h2>管理员审核页面</h2>
    </div>
    <div class="FilterBar">
      <label>
        举报类型：
        <select v-model="selectedType">
          <option value="">全部</option>
          <option value="post">帖子</option>
          <option value="comment">评论</option>
          <option value="user">用户</option>
        </select>
      </label>
      <label>
        状态筛选：
        <select v-model="selectedStatus">
          <option value="">全部</option>
          <option value="pending">待审核</option>
          <option value="resolved">已处理</option>
        </select>
      </label>
      <button @click="onFilter">筛选</button>
    </div>
    <div class="ReportList">
      <!-- 这里后续由API返回举报数据后循环渲染 -->
      <!-- 示例结构，实际内容由父级传递/接口返回 -->
      <div
        v-for="report in reports"
        :key="report.id"
        class="ReportItem"
      >
        <div class="ReportSummary">
          <div class="ReportTypeTag" :class="report.type">{{ getTypeText(report.type) }}</div>
          <div class="ReportTime">{{ report.time }}</div>
          <div class="ReportStatus" :class="report.status">{{ getStatusText(report.status) }}</div>
        </div>
        <div class="ReportContent">
          <div class="ReportedContent">
            <span>被举报内容：</span>
            <!-- 可插入帖子预览、评论预览、用户卡片等 -->
            <slot name="reported-content" :report="report"></slot>
          </div>
          <div class="ReportReason">
            <span>举报理由：</span>
            <span>{{ report.reason }}</span>
          </div>
        </div>
        <div class="AuditActions">
          <button @click="onApprove(report.id)">通过</button>
          <button @click="onReject(report.id)">驳回</button>
          <button @click="onViewDetail(report.id)">详情</button>
        </div>
      </div>
      <!-- 没有数据的提示 -->
      <div v-if="reports.length === 0" class="NoDataTip">暂无举报信息</div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";

// 这里的 reports 只是结构参考，后续你可以用 API 数据替换
const reports = ref([]); // 数组内容由API更新

const selectedType = ref("");
const selectedStatus = ref("");

function onFilter() {
  // 这里可以触发 API 查询
}

function onApprove(id) {
  // 审核通过逻辑，API操作
}
function onReject(id) {
  // 驳回逻辑，API操作
}
function onViewDetail(id) {
  // 跳转详情页或弹窗
}

function getTypeText(type) {
  switch (type) {
    case "post": return "帖子";
    case "comment": return "评论";
    case "user": return "用户";
    default: return "未知类型";
  }
}
function getStatusText(status) {
  switch (status) {
    case "pending": return "待审核";
    case "resolved": return "已处理";
    default: return "未知状态";
  }
}
</script>

<style scoped>
.BigContainer {
  background-color: aliceblue;
  width: 100vw;
  min-height: 100vh;
  position: absolute;
  padding: 32px 0;
}

.Header {
  text-align: center;
  margin-bottom: 24px;
}

.FilterBar {
  display: flex;
  gap: 16px;
  align-items: center;
  margin-bottom: 24px;
  justify-content: center;
}

.ReportList {
  max-width: 900px;
  margin: 0 auto;
}

.ReportItem {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px #e0e7ef;
  padding: 18px 24px;
  margin-bottom: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ReportSummary {
  display: flex;
  gap: 24px;
  align-items: center;
}
.ReportTypeTag {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
}
.ReportTypeTag.post { background: #dbeafe; color: #2563eb; }
.ReportTypeTag.comment { background: #fef3c7; color: #92400e; }
.ReportTypeTag.user { background: #f3e8ff; color: #7c3aed; }
.ReportTime {
  color: #888;
  font-size: 13px;
}
.ReportStatus {
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
}
.ReportStatus.pending { background: #fde68a; color: #92400e; }
.ReportStatus.resolved { background: #bbf7d0; color: #065f46; }

.ReportContent {
  border-top: 1px solid #e5e7eb;
  padding-top: 10px;
  font-size: 15px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.ReportedContent span {
  font-weight: 600;
}
.ReportReason span:first-child {
  font-weight: 600;
}
.AuditActions {
  display: flex;
  gap: 16px;
  margin-top: 8px;
}
.AuditActions button {
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 4px 16px;
  cursor: pointer;
  font-size: 15px;
  transition: background 0.2s;
}
.AuditActions button:nth-child(2) {
  background: #ef4444;
}
.AuditActions button:nth-child(3) {
  background: #6b7280;
}
.NoDataTip {
  text-align: center;
  color: #888;
  margin-top: 32px;
  font-size: 16px;
}
</style>