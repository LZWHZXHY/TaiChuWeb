<template>
  <div class="novel-container">
    <h2 class="novel-title-main">彼岸宇宙小说</h2>
    <el-tabs v-model="tab" class="novel-tabs">
      <el-tab-pane label="全部" name="all" />
      <el-tab-pane label="单篇" name="single" />
      <el-tab-pane label="连载" name="serial" />
      <el-tab-pane label="长篇" name="long" />
    </el-tabs>
    <div class="novel-list">
      <el-card
        v-for="novel in filteredNovels"
        :key="novel.id"
        class="novel-card"
        shadow="hover"
      >
        <div class="novel-card-header">
          <h3 class="novel-title-ellipsis">{{ novel.title }}</h3>
        </div>
        <div class="novel-type-line">
          <el-tag size="small" type="info">
            {{
              novel.type === 1
                ? "单篇故事合集"
                : novel.type === 2
                ? "长篇连载"
                : novel.type === 3
                ? "长篇"
                : novel.type
            }}
          </el-tag>
          <el-tag v-if="novel.series" size="small" class="ml10">{{ novel.series }}</el-tag>
          <el-tag v-if="novel.part" size="small" class="ml10">{{ novel.part }}</el-tag>
          <el-tag
            v-if="novel.status"
            size="small"
            :type="novel.status === '连载中' ? 'success' : 'warning'"
            class="ml10"
            >{{ novel.status }}</el-tag
          >
        </div>
        <div class="novel-author" v-if="novel.author">
          作者：{{ novel.author }}
        </div>
        <div class="novel-desc">
          {{ novel.description || novel.desc }}
        </div>
        <div class="chapter-list">
          <div
            v-if="!novel.chapters || novel.chapters.length === 0"
            class="chapter-empty"
          >
            暂无章节
          </div>
          <div
            v-for="chapter in novel.chapters"
            :key="chapter.id"
            class="chapter-link-wrap"
          >
            <el-link
              type="primary"
              class="chapter-link"
              @click="openChapter(chapter)"
            >
              {{ chapter.title }}
            </el-link>
            <span v-if="chapter.author" class="chapter-author">作者：{{ chapter.author }}</span>
          </div>
        </div>
      </el-card>
    </div>
    <!-- 章节内容弹窗 -->
    <el-dialog
      v-model="showDialog"
      width="80vw"
      :title="selectedChapter?.title"
      class="novel-dialog"
      :modal="true"
      :close-on-click-modal="true"
      :close-on-press-escape="true"
      top="5vh"
    >
      <!-- 信息栏 -->
      <div v-if="selectedChapter" class="chapter-info-bar">
        <span v-if="selectedChapter.author"><i class="el-icon-user"></i> 作者：{{ selectedChapter.author }}</span>
        <span v-if="selectedChapter.createdAt || selectedChapter.createTime" class="chapter-date">
          <i class="el-icon-time"></i> 创建：{{ formatDate(selectedChapter.createdAt || selectedChapter.createTime) }}
        </span>
        <span v-if="selectedChapter.updatedAt || selectedChapter.updateTime" class="chapter-date">
          <i class="el-icon-refresh"></i> 更新：{{ formatDate(selectedChapter.updatedAt || selectedChapter.updateTime) }}
        </span>
        <span v-if="typeof selectedChapter.likes === 'number'" class="chapter-like">
          <i class="el-icon-like"></i> {{ selectedChapter.likes }}
        </span>
        <span v-if="typeof selectedChapter.reports === 'number'" class="chapter-report">
          <i class="el-icon-warning-outline"></i> {{ selectedChapter.reports }}
        </span>
        <span v-if="typeof selectedChapter.views === 'number'" class="chapter-views">
          <i class="el-icon-view"></i> {{ selectedChapter.views }}
        </span>
      </div>
      <div style="margin-bottom: 16px;">
        <el-button size="small" :disabled="hasLiked" @click="likeChapter" type="primary">
          👍 点赞 {{ selectedChapter.likes }}
        </el-button>
        <el-button size="small" :disabled="hasReported" @click="reportChapter" type="danger">
          🚩 举报 {{ selectedChapter.reports }}
        </el-button>
      </div>
      <div
        v-if="chapterContent"
        class="chapter-text"
        v-html="formatContent(chapterContent)"
      ></div>
      <div v-else>正在加载章节内容...</div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { novelApi } from "../../server/api";

// 标签过滤
const tab = ref("all");
const novels = ref([]);
const showDialog = ref(false);
const selectedChapter = ref(null);
const chapterContent = ref("");
const hasLiked = ref(false);
const hasReported = ref(false);

const filteredNovels = computed(() => {
  if (tab.value === "all") return novels.value;
  if (tab.value === "single") return novels.value.filter((n) => n.type === 1);
  if (tab.value === "serial") return novels.value.filter((n) => n.type === 2);
  if (tab.value === "long") return novels.value.filter((n) => n.type === 3);
  return novels.value;
});

// 获取小说及章节
onMounted(async () => {
  try {
    const novelList = await novelApi.getNovels();
    const chaptersArr = await Promise.all(
      novelList.map((novel) => novelApi.getChapters(novel.id))
    );
    novels.value = novelList.map((novel, idx) => ({
      ...novel,
      chapters: chaptersArr[idx],
    }));
  } catch (e) {
    novels.value = [];
  }
});

// 打开章节弹窗并拉取内容和操作状态
async function openChapter(chapter) {
  showDialog.value = true;
  selectedChapter.value = chapter;
  chapterContent.value = "";
  hasLiked.value = false;
  hasReported.value = false;

  try {
    console.log("请求章节内容...");
    const data = await novelApi.getChapterContent(chapter.id);
    console.log("章节内容接口返回:", data);
    Object.assign(selectedChapter.value, data);
    chapterContent.value = data.content || data;

    console.log("请求章节操作状态...");
    const status = await novelApi.getChapterActionStatus(chapter.id);
    console.log("章节操作状态接口返回:", status);

    // 判空健壮
    if (!status || typeof status.hasLiked === "undefined") {
      chapterContent.value = status?.message || "加载失败";
      return;
    }
    hasLiked.value = !!status.hasLiked;
    hasReported.value = !!status.hasReported;
  } catch (e) {
    console.error("加载章节内容失败", e);
    chapterContent.value = "加载失败";
  }
}

// 点赞
async function likeChapter() {
  try {
    await novelApi.likeChapter(selectedChapter.value.id);
    selectedChapter.value.likes++;
    hasLiked.value = true;
    ElMessage.success("点赞成功！");
  } catch (e) {
    hasLiked.value = true;
    ElMessage.warning(e?.message || "不能重复点赞");
  }
}

// 举报
async function reportChapter() {
  try {
    await novelApi.reportChapter(selectedChapter.value.id);
    selectedChapter.value.reports++;
    hasReported.value = true;
    ElMessage.success("举报成功！");
  } catch (e) {
    hasReported.value = true;
    ElMessage.warning(e?.message || "不能重复举报");
  }
}

// 支持段落/换行排版
function formatContent(content) {
  if (!content) return "";
  return content
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0)
    .map(
      (line) =>
        `<p style="text-indent:2em;margin:0 0 1em 0;">${line}</p>`
    )
    .join("");
}

// 格式化时间
function formatDate(date) {
  if (!date) return '';
  const d = new Date(date);
  return d.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  }).replace(/\//g, '-');
}
</script>

<style scoped>
.chapter-info-bar .chapter-views {
  margin-left: 8px;
  color: #4fae6e;
  font-size: 13px;
}
.chapter-info-bar .el-icon-view {
  font-size: 16px;
  vertical-align: middle;
}
.novel-container {
  max-width: 1400px;
  width: 95vw;
  margin: 40px auto;
  background: #fafcff;
  padding: 32px 40px;
  border-radius: 16px;
  box-shadow: 0 8px 32px 0 rgba(80, 120, 200, 0.10);
}
.novel-title-main {
  text-align: center;
  margin-bottom: 24px;
  font-size: 2.2rem;
  color: #3a4276;
  letter-spacing: 2px;
  font-weight: bold;
}
.novel-tabs {
  margin-bottom: 20px;
}
.novel-list {
  display: flex;
  flex-wrap: wrap;
  gap: 22px;
  justify-content: flex-start;
}
.novel-card {
  width: 400px;
  min-height: 300px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 14px 0 rgba(110, 140, 210, 0.09);
  transition: transform 0.16s, box-shadow 0.16s;
  padding: 13px 17px 17px 17px;
}
.novel-card:hover {
  transform: translateY(-7px) scale(1.04);
  box-shadow: 0 8px 32px 0 rgba(80, 120, 200, 0.16);
}
.novel-card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
  min-width: 0;
}
.novel-title-ellipsis {
  flex: 1 1 0;
  font-size: 1.08rem;
  font-weight: bold;
  color: #3267ab;
  margin: 0;
  min-width: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.novel-type-line {
  display: flex;
  align-items: center;
  gap: 7px;
  margin: 2px 0 4px 0;
}
.novel-author {
  font-size: 13px;
  color: #888;
  margin: 4px 0 0 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.novel-desc {
  margin: 10px 0 16px 0;
  color: #333;
  font-size: 14px;
  min-height: 36px;
  line-height: 1.55;
  word-break: break-word;
}
.chapter-list {
  margin-top: 10px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.chapter-link-wrap {
  display: flex;
  align-items: center;
  gap: 6px;
}
.chapter-link {
  cursor: pointer;
  font-size: 14px;
  background: #e5f0fe;
  color: #3178d3 !important;
  border-radius: 6px;
  padding: 2px 10px;
  margin-right: 0;
  transition: background 0.15s, color 0.15s;
  border: none;
  text-decoration: none;
  display: inline-block;
  line-height: 1.8;
}
.chapter-link:hover {
  background: #afd6fa;
  color: #19498a !important;
  text-decoration: underline;
}
.chapter-author {
  font-size: 12px;
  color: #888;
  margin-left: 6px;
}
.chapter-empty {
  color: #b8b8b8;
  font-size: 13px;
  padding: 6px 0;
}
.ml10 {
  margin-left: 10px;
}
.chapter-info-bar {
  font-size: 14px;
  color: #6a7fad;
  margin-bottom: 12px;
  display: flex;
  flex-wrap: wrap;
  gap: 18px;
  align-items: center;
}
.chapter-info-bar .chapter-date,
.chapter-info-bar .chapter-like,
.chapter-info-bar .chapter-report {
  margin-left: 8px;
  color: #a3a3a3;
  font-size: 13px;
}
.chapter-info-bar i {
  margin-right: 2px;
}
.chapter-info-bar .chapter-like {
  color: #f59e42;
}
.chapter-info-bar .chapter-report {
  color: #e55;
}
.chapter-info-bar .el-icon-like,
.chapter-info-bar .el-icon-warning-outline {
  font-size: 16px;
  vertical-align: middle;
}

/* 弹窗优化 */
:deep(.el-dialog__wrapper) {
  z-index: 2100 !important;
}
:deep(.el-dialog) {
  border-radius: 18px;
  width: 80%;
  height: 85%;
  padding: 0;
  background: #cfd0d2;
  box-shadow: 0 8px 36px 0 rgba(80, 120, 200, 0.543);
}
:deep(.el-dialog__header) {
  padding: 24px 32px 12px 32px;
  font-size: 1.25rem;
  color: #2a4a7c;
  font-weight: bold;
  border-bottom: 1px solid #e6eaf2;
  letter-spacing: 1px;
}
:deep(.el-dialog__body) {
  padding: 18px 32px 32px 32px;
  height: 100%;
  overflow-y: auto;
  background: #ffffff;
}
.chapter-text {
  font-size: 0.85rem;
  line-height: 2.2;
  color: #232323;
  margin-top: 4px;
  background: #ffffffed;
  border-radius: 12px;
  padding: 18px 26px;
  box-shadow: 0 2px 12px #bfcbe64a;
  height: 90%;
  overflow-y: auto;
  font-family: "Source Han Serif SC", "serif", "宋体", serif;
  letter-spacing: 0.02em;
}
@media (max-width: 1100px) {
  .novel-container {
    max-width: 99vw;
    padding: 8vw 1vw;
  }
  .novel-card {
    width: 94vw;
    max-width: 99vw;
  }
  .chapter-text {
    padding: 8px 4vw;
  }
  :deep(.el-dialog) {
    max-width: 99vw;
  }
  :deep(.el-dialog__body) {
    padding: 6vw 2vw;
  }
}
</style>