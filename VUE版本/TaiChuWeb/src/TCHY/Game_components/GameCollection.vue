<template>
  <div class="dashboard-root">
    <nav class="sidebar">
      <div class="logo">🎮 Game Center</div>
      <div class="divider"></div>
      <ul class="menu">
        <li :class="{ active: selected === 'web' }" @click="select('web')">
          <span class="icon">🌐</span> 网页游戏
        </li>
        <li :class="{ active: selected === 'single' }" @click="select('single')">
          <span class="icon">💻</span> 单机游戏
        </li>
        <li :class="{ active: selected === 'conway' }" @click="select('conway')">
          <span class="icon">🧬</span> 细胞游戏
        </li>
      </ul>
    </nav>
    <section class="main-content">
      <div class="content-card">
        <div class="card-header">
          <div class="card-title">
            {{
              selected === "web"
                ? "网页游戏"
                : selected === "single"
                ? "单机游戏"
                : "康威生命游戏"
            }}
          </div>
          <div class="card-toolbar">
            <button class="toolbar-btn" title="刷新" @click="reload">
              <span class="toolbar-icon">⟳</span>
            </button>
          </div>
        </div>
        <div class="card-body">
          <template v-if="selected === 'conway'">
            <ConwayGame />
          </template>
          <template v-else-if="selected === 'single'">
            <div class="single-game-list">
              <div v-for="game in singleGames" :key="game.name" class="game-card">
                <div class="game-title">{{ game.name }}</div>
                <div class="game-desc">{{ game.description }}</div>
                <div class="game-creater">作者：{{ game.creater }}</div>
                <a :href="game.download" download class="game-download">下载</a>
              </div>
            </div>
          </template>
          <template v-else>
            <slot :category="selected"></slot>
          </template>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref } from "vue";
import ConwayGame from "./ConwayGame.vue";
const selected = ref("web");
const select = (type) => { selected.value = type; };
const reload = () => {
  if (selected.value === "conway") {
    location.reload();
  }
  // 其它游戏刷新逻辑
};


const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";



const getFileName = (downloadUrl) => {
  const match = downloadUrl.match(/path=([^&]+)/);
  if (match) {
    return decodeURIComponent(match[1].split('/').pop());
  }
  return "downloaded_file";
};
// 单机游戏数据
const singleGames = [
  {
    name: "实验性游戏0.152",
    description: "早期对技术进行研究和实验性质的游戏。",
    creater:"太初寰宇-游戏制作部门",
    download: `${baseApiUrl}/api/UserInfo/fileproxy?path=games/UnrealSkillTestGame.7z`
  },
  {
    name: "充电小球",
    description: "一个充电小球游戏，是学校的作业",
    creater:"腾蛇",
    download: `${baseApiUrl}/api/UserInfo/fileproxy?path=games/Maze1.1.7z`
  },
  {
    name: "未完成的屠杀室Rouglike",
    description: "学校作业+1",
    creater:"腾蛇",
    download: `${baseApiUrl}/api/UserInfo/fileproxy?path=games/屠杀室测试版v1.41.7z`
  }
  
];
</script>

<style scoped>
.game-creater {
  color: #2897c8;
  font-size: 0.93rem;
  font-weight: 500;
  margin-bottom: 12px;
  background: linear-gradient(90deg, #eaf6ff 0%, #f9fcff 100%);
  padding: 2px 6px;
  border-radius: 4px;
  letter-spacing: 0.5px;
}
.dashboard-root {
  display: flex;
  min-height: 100vh;
  background: linear-gradient(120deg, #eaf6ff 0%, #f7fcff 100%);
  font-family: 'Segoe UI', 'PingFang SC', 'Helvetica Neue', Arial, 'Microsoft YaHei', sans-serif;
}

/* Sidebar */
.sidebar {
  width: 220px;
  min-width: 180px;
  background: #2e415a;
  color: #f7fcff;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 32px 0 0 0;
  box-shadow: 2px 0 18px 0 #2e415a14;
  border-top-right-radius: 30px;
  border-bottom-right-radius: 30px;
}

.logo {
  font-size: 2rem;
  font-weight: 700;
  letter-spacing: 1px;
  margin-bottom: 22px;
  background: linear-gradient(90deg, #5abaff 0%, #53e3f7 80%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  text-align: center;
  text-shadow: 0 3px 12px #5abaff32;
}

.divider {
  width: 70%;
  height: 2px;
  background: #5abaff33;
  margin: 0 auto 18px auto;
  border-radius: 1px;
}

.menu {
  list-style: none;
  padding: 0;
  width: 100%;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.menu li {
  cursor: pointer;
  padding: 10px 30px;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  border-radius: 9px;
  transition: background 0.16s, color 0.16s, box-shadow 0.2s;
  margin: 2px 0;
  position: relative;
  user-select: none;
}

.menu li:hover, .menu li.active {
  background: linear-gradient(90deg, #5abaff55 0%, #53e3f755 100%);
  color: #fff;
  box-shadow: 0 2px 14px #5abaff17;
  font-weight: 500;
}

.menu .icon {
  margin-right: 13px;
  font-size: 1.25em;
  vertical-align: middle;
}

/* Main Content */
.main-content {
  flex: 1;
  padding: 44px 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 0;
  background: transparent;
}

.content-card {
  width: 96%;
  max-width: 1600px;
  min-height: 700px;
  background: #f9fcff;
  border-radius: 22px;
  box-shadow: 0 6px 32px #5abaff15;
  display: flex;
  flex-direction: column;
  margin: 0 auto;
  overflow: hidden;
  border: 1.5px solid #e2f1fc;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 22px 28px 12px 28px;
  background: #e9f6ff;
  border-bottom: 1px solid #e2f1fc;
}

.card-title {
  font-size: 1.8rem;
  font-weight: 600;
  color: #2897c8;
  letter-spacing: 1px;
  text-shadow: 0 2px 10px #5abaff22;
}

.card-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
}

.toolbar-btn {
  background: #5abaff;
  color: #fff;
  border: none;
  border-radius: 7px;
  padding: 7px 14px;
  font-size: 1.1rem;
  box-shadow: 0 2px 10px #5abaff33;
  cursor: pointer;
  transition: background 0.15s, box-shadow 0.18s;
  outline: none;
}

.toolbar-btn:hover {
  background: #2897c8;
  box-shadow: 0 2px 20px #5abaff44;
}

.toolbar-icon {
  font-size: 1.35em;
  vertical-align: middle;
}

.card-body {
  flex: 1;
  padding: 28px 26px 28px 26px;
  background: #f9fcff;
  overflow: auto;
  min-height: 500px;
  /* For embedded game styles, can be overridden by slot */
}

/* 单机游戏卡片样式 */
.single-game-list {
  display: flex;
  gap: 28px;
  flex-wrap: wrap;
  padding: 10px 0;
}

.game-card {
  background: #eaf6ff;
  border-radius: 12px;
  box-shadow: 0 2px 12px #5abaff22;
  padding: 22px 18px;
  width: 250px;
  margin-bottom: 18px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.game-title {
  font-size: 1.2rem;
  font-weight: 600;
  color: #2897c8;
  margin-bottom: 8px;
}

.game-desc {
  color: #3a5e7c;
  font-size: 0.98rem;
  margin-bottom: 14px;
}

.game-download {
  background: #5abaff;
  color: #fff;
  padding: 6px 16px;
  border-radius: 6px;
  text-decoration: none;
  font-weight: 500;
  transition: background 0.15s;
}

.game-download:hover {
  background: #2897c8;
}

@media (max-width: 900px) {
  .sidebar {
    width: 60px;
    min-width: 0;
    padding: 20px 0;
    border-radius: 0 22px 22px 0;
  }
  .logo {
    font-size: 1.4rem;
    margin-bottom: 12px;
  }
  .menu li {
    font-size: 1rem;
    text-align: center;
    padding: 9px 8px;
    margin: 2px 0;
    justify-content: center;
  }
  .menu .icon {
    margin-right: 0;
    font-size: 1.1em;
  }
}

@media (max-width: 600px) {
  .main-content, .content-card {
    width: 100%;
    min-height: 350px;
    padding: 0;
  }
  .card-header {
    padding: 12px 10px 8px 10px;
  }
  .card-title {
    font-size: 1.2rem;
  }
  .card-body {
    padding: 10px 5px 10px 5px;
    min-height: 200px;
  }
}
</style>