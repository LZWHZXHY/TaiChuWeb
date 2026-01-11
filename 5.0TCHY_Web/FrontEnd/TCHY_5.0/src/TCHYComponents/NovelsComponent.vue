<template>
  <div class="app-shell">
    <!-- 左侧世界观 IP -->
    <aside class="aside-ip">
      <h2 class="aside-title">世界观 IP</h2>
      <ul class="ip-list">
        <li v-for="ip in ips" :key="ip.key"
            :class="{ active: activeIP && activeIP.key === ip.key }"
            @click="selectIP(ip.key)">
          <div class="ip-name">{{ ip.name }}</div>
          <div class="ip-meta">{{ ip.tagline }}</div>
        </li>
      </ul>
    </aside>
    <!-- 中间作品展示 -->
    <main class="main-content">
      <div v-if="!activeIP" class="placeholder">
        <h3>请选择一个世界观 IP（系列）</h3>
        <p>例如 “彼岸宇宙”。</p>
      </div>
      <div v-else>
        <section class="works-grid">
          <article class="work-card"
                   v-for="work in activeIP.works"
                   :key="work.id"
                   @click="selectWork(work.id)"
                   :class="{active: activeWork && activeWork.id === work.id}">
            <div class="cover" :style="{ backgroundImage: `url(${work.cover_url || defaultCover})` }"></div>
            <div class="work-body">
              <h3 class="work-title">{{ work.title }}</h3>
              <div class="work-type">
                <span class="pill" :class="work.type === 2 ? 'serial' : 'single'">
                  {{ work.type === 2 ? '连载' : '单篇' }}
                </span>
                <span class="tagline">{{ work.desc }}</span>
              </div>
              <div class="work-meta">
                <span>作者：{{ work.author }}</span>
              </div>
            </div>
          </article>
        </section>
      </div>
      <!-- 章节内容弹窗组件（ChapterReader） -->
      <ChapterReader
        v-if="selectedChapter"
        :chapter="selectedChapter"
        :showAuthor="activeWork && activeWork.type !== 2"
        :defaultAuthor="activeWork ? activeWork.author : ''"
        @close="selectedChapter = null"
      />
    </main>
    <!-- 右侧章节列表 -->
    <aside class="aside-detail" v-if="activeWork">
      <div>
        <h3 class="detail-title">{{ activeWork.title }}</h3>
        <div v-if="activeChapters.length">
          <h4 style="margin-top:16px;">章节列表</h4>
          <ul class="chapter-list">
            <li v-for="ch in activeChapters" :key="ch.id">
              <button class="chapter-link" @click="openChapter(ch)">
                第{{ ch.order_num }}章：{{ ch.title }}
              </button>
            </li>
          </ul>
        </div>
        <div v-else>
          <span style="color:#888;">本书暂无章节。</span>
        </div>
      </div>
    </aside>
  </div>
</template>

<script>
import apiClient from '@/utils/api'
import ChapterReader from './Novels/ChapterReader.vue'

function novelsArrayToIps(rawArr) {
  const seriesMap = {};
  rawArr.forEach(novel => {
    const seriesKey = novel.series || 'default';
    if (!seriesMap[seriesKey]) {
      seriesMap[seriesKey] = {
        key: seriesKey,
        name: `${seriesKey}`,
        tagline: '',
        works: [],
      };
    }
    seriesMap[seriesKey].works.push(novel);
  });
  return Object.values(seriesMap);
}

export default {
  name: "IpDrivenNovels",
  components: { ChapterReader },
  data() {
    return {
      defaultCover: "https://picsum.photos/seed/book/400/240",
      ips: [],
      activeIP: null,
      activeWork: null,
      activeChapters: [],
      selectedChapter: null,
    }
  },
  async created() {
    try {
      const res = await apiClient.get("/Novels/all");
      this.ips = novelsArrayToIps(res.data || []);
      if (this.ips.length) {
        this.selectIP(this.ips[0].key);
      }
    } catch (e) {
      console.error("获取小说内容失败", e);
    }
  },
  methods: {
    selectIP(key) {
      const ip = this.ips.find(i => i.key === key);
      this.activeIP = ip || null;
      this.activeWork = null;
      this.activeChapters = [];
      this.selectedChapter = null;
    },
    async selectWork(workId) {
      if (!this.activeIP) return;
      const w = this.activeIP.works.find(x => x.id === workId);
      if (!w) return;
      this.activeWork = w;
      this.selectedChapter = null;
      try {
        const res = await apiClient.get(`/Novels/${w.id}/chapters`);
        this.activeChapters = Array.isArray(res.data) ? res.data : [];
      } catch (e) {
        this.activeChapters = [];
      }
    },
    openChapter(ch) {
      this.selectedChapter = ch;
    },
  }
}
</script>

<style scoped>
.app-shell {
  display: grid;
  grid-template-columns: 240px 1fr 380px;
  gap: 22px;
  padding: 28px;
  min-height: 100vh;
  background: linear-gradient(180deg, #f7fbff 0%, #f2f6fb 100%);
  font-family: "PingFang SC", "Segoe UI", Arial, sans-serif;
  color: #2b2b2b;
}
.aside-ip {
  background: #fff;
  border-radius: 12px;
  padding: 18px;
  box-shadow: 0 8px 22px rgba(30, 40, 60, 0.06);
  border: 1px solid #eef2f6;
}
.aside-title {
  margin: 0 0 12px;
  font-size: 16px;
  font-weight: 700;
}
.ip-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.ip-list li {
  padding: 10px 10px;
  border-radius: 8px;
  cursor: pointer;
  transition: all .14s ease;
  margin-bottom: 8px;
  border: 1px solid transparent;
}
.ip-list li:hover {
  background: #f2f8ff;
  border-color: #e6f0ff;
}
.ip-list li.active {
  background: linear-gradient(90deg, #eef5ff, #e9f2ff);
  border-color: #dbeafe;
  box-shadow: inset 0 0 0 2px rgba(67,118,255,0.06);
}
.ip-name {
  font-weight: 600;
}
.ip-meta {
  font-size: 12px;
  color: #7a8aa3;
  margin-top: 4px;
}
.main-content {
background-color: #ffffff00;
}

.placeholder {
  background: #fff;
  padding: 36px;
  border-radius: 12px;
  box-shadow: 0 8px 22px rgba(30, 40, 60, 0.04);
  text-align: center;
}
.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 18px;
}
.work-card {
  display: flex;
  flex-direction: column;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: transform .18s ease, box-shadow .18s ease;
  border: 1px solid #f0f4f9;
  box-shadow: 0 8px 20px rgba(24, 38, 74, 0.04);
}
.work-card.active {
  box-shadow: 0 18px 40px rgba(24,38,74,0.14);
  border-color: #4170ff;
}
.work-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 18px 40px rgba(24, 38, 74, 0.08);
}
.cover {
  height: 140px;
  background-size: cover;
  background-position: center;
  background-color: #eef7ff;
}
.work-body {
  padding: 12px 14px 16px;
}
.work-title {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #1f2b3a;
}
.work-type {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-top: 6px;
}
.pill {
  padding: 4px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
}
.pill.serial { background: #f0f7ff; color: #2d6cff; border: 1px solid #dbeafe; }
.pill.single { background: #fff7f0; color: #e36b2f; border: 1px solid #fde8da; }
.tagline {
  color: #6b7280;
  font-size: 12px;
}
.work-meta {
  display:flex;
  justify-content: space-between;
  font-size: 12px;
  color: #7b8796;
}
.aside-detail {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 8px 22px rgba(30, 40, 60, 0.06);
  border: 1px solid #eef2f6;
  overflow: auto;
  max-height: 96vh;
  min-height: 300px;
}
.detail-title {
  margin: 4px 0 6px;
  font-size: 18px;
  font-weight: 700;
}
.chapter-list {
  list-style: none;
  margin: 12px 0 8px;
  padding: 0;
}
.chapter-list li {
  margin-bottom: 6px;
}
.chapter-link {
  display: inline-block;
  width: 100%;
  background: transparent;
  color: #4170ff;
  border: none;
  font-size: 16px;
  padding: 9px 4px;
  cursor: pointer;
  text-align: left;
  border-radius: 7px;
  transition: background 0.14s;
}
.chapter-link:hover {
  background: #e5efff;
}
</style>