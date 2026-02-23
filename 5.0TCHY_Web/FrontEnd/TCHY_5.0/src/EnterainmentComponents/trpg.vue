<template>
  <div class="trpg-lobby-terminal">
    <header class="lobby-header">
      <div class="brand-group">
        <span class="dept">TRPG_NETWORK // 跑团枢纽</span>
        <h1 class="title">MISSION_HUB_中央大厅</h1>
      </div>
      <div class="server-stats">
        <div class="stat-item">
          <span class="k">ACTIVE_SESSIONS:</span>
          <span class="v">{{ rooms.length }}</span>
        </div>
        <div class="stat-item">
          <span class="k">UPLINK_STATUS:</span>
          <span class="v pulse">ENCRYPTED</span>
        </div>
      </div>
    </header>

    <nav class="lobby-controls">
      <div class="search-field">
        <span class="prefix">>> SEARCH_REF:</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="输入模组代号、GM或关键词..." 
        />
      </div>

      <div class="filter-tabs">
        <button 
          v-for="sys in systems" 
          :key="sys" 
          :class="{ active: filterSystem === sys }"
          @click="filterSystem = sys"
        >
          {{ sys }}
        </button>
      </div>

      <button class="initiate-btn" @click="$emit('create')">
        + 发起新招募 // NEW_RECRUIT
      </button>
    </nav>

    <main class="mission-scroll custom-scroll">
      <div v-if="filteredRooms.length === 0" class="empty-state">
        [ ❌ 警告：未检索到对应频率的招募信号 ]
      </div>

      <div class="mission-grid" v-else>
        <article 
          v-for="room in filteredRooms" 
          :key="room.id" 
          class="mission-card"
          @click="handleEnter(room.id)"
        >
          <div class="card-side">
            <span class="sys-vertical">{{ room.system }}</span>
          </div>

          <div class="card-content">
            <div class="c-header">
              <span class="c-id">REF.TR-{{ room.id }}</span>
              <div class="c-status-group">
                <span class="c-ver">VER_2.0</span>
                <span class="c-badge">RECRUITING</span>
              </div>
            </div>

            <h3 class="c-title">{{ room.title }}</h3>

            <div class="c-meta">
              <div class="m-item"><span class="k">GM:</span> <span class="v">{{ room.gm }}</span></div>
              <div class="m-item"><span class="k">SLOTS:</span> <span class="v">{{ room.players }}/{{ room.maxPlayers }}</span></div>
            </div>

            <p class="c-desc">{{ room.desc }}</p>

            <footer class="c-footer">
              <div class="tags">
                <span v-for="tag in room.tags" :key="tag" class="tag">#{{ tag }}</span>
              </div>
              <div class="action-hint">介入请求 [LINK] →</div>
            </footer>
          </div>

          <div class="card-stamp">FILE_OP</div>
        </article>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 定义事件
defineEmits(['create', 'enter'])

// 状态变量
const searchQuery = ref('')
const filterSystem = ref('ALL')
const systems = ['ALL', 'COC', 'DND', 'CYBERPUNK', 'OTHER']

// 模拟数据 (后续通过后端填充)
const rooms = ref([
  {
    id: 1024,
    title: "马赛克工厂的怪谈",
    gm: "落子无悔",
    system: "COC",
    players: 1,
    maxPlayers: 4,
    desc: "在废弃的像素处理厂，员工们一个个消失在代码碎裂的声音中。你们作为调查员被派往现场...",
    tags: ["硬核", "封闭空间", "解谜"],
  },
  {
    id: 1088,
    title: "零号协议：意识上传",
    gm: "卡利加里",
    system: "CYBERPUNK",
    players: 3,
    maxPlayers: 3,
    desc: "这是一场关于灵魂的买卖。夜之城的霓虹灯下，没有人是安全的。你们需要潜入荒坂塔...",
    tags: ["高科技", "暴力", "扮演"],
  }
])

// 过滤逻辑
const filteredRooms = computed(() => {
  return rooms.value.filter(r => {
    const matchSearch = r.title.includes(searchQuery.value) || r.gm.includes(searchQuery.value)
    const matchSystem = filterSystem.value === 'ALL' || r.system === filterSystem.value
    return matchSearch && matchSystem
  })
})

const handleEnter = (id) => {
  console.log(`正在接入房间: ${id}`)
}
</script>

<style scoped>
.trpg-lobby-terminal {
  --ink: #111; --paper: #fff; --accent: #D92323; --neon: #00f2ff;
  width: 100%; height: 100vh; background: #eee;
  display: flex; flex-direction: column; overflow: hidden;
  font-family: 'JetBrains Mono', 'PingFang SC', monospace;
}

/* 顶部栏 */
.lobby-header {
  background: #111; color: #fff; padding: 20px 40px;
  display: flex; justify-content: space-between; align-items: center;
  border-bottom: 5px solid var(--accent);
}
.brand-group .dept { font-size: 0.7rem; opacity: 0.5; letter-spacing: 2px; }
.brand-group .title { font-size: 1.8rem; font-weight: 900; margin-top: 5px; letter-spacing: -1px; }

.server-stats { display: flex; gap: 40px; }
.stat-item .k { font-size: 0.6rem; color: #666; margin-right: 8px; }
.stat-item .v { font-weight: 900; font-size: 1.1rem; color: var(--neon); }
.pulse { animation: blink 2s infinite; }

/* 控制区 */
.lobby-controls {
  padding: 20px 40px; background: #fff; border-bottom: 2px solid #111;
  display: flex; gap: 30px; align-items: center;
}
.search-field { flex: 1; display: flex; align-items: center; background: #f4f4f4; border: 2px solid #111; padding: 0 15px; }
.search-field .prefix { font-size: 0.7rem; font-weight: 900; color: var(--accent); }
.search-field input { border: none; background: none; padding: 12px; width: 100%; outline: none; font-weight: bold; }

.filter-tabs { display: flex; gap: 2px; background: #111; padding: 2px; }
.filter-tabs button { border: none; background: #fff; padding: 8px 18px; font-size: 0.7rem; font-weight: 900; cursor: pointer; }
.filter-tabs button.active { background: var(--accent); color: #fff; }

.initiate-btn {
  background: #111; color: #fff; border: none; padding: 12px 25px;
  font-weight: 900; font-size: 0.8rem; cursor: pointer; transition: 0.2s;
}
.initiate-btn:hover { background: var(--accent); box-shadow: 5px 5px 0 rgba(217,35,35,0.2); }

/* 任务列表 */
.mission-scroll { flex: 1; padding: 40px; overflow-y: auto; }
.mission-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(450px, 1fr)); gap: 30px; }

.mission-card {
  background: #fff; border: 2px solid #111; display: flex; height: 220px;
  position: relative; overflow: hidden; cursor: pointer; transition: 0.3s;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}
.mission-card:hover { transform: translate(-4px, -4px); box-shadow: 12px 12px 0 rgba(217, 35, 35, 0.2); border-color: var(--accent); }

/* 卡片细节 */
.card-side { width: 40px; background: #111; color: #fff; display: flex; align-items: center; justify-content: center; }
.sys-vertical { writing-mode: vertical-rl; transform: rotate(180deg); font-weight: 900; font-size: 0.75rem; letter-spacing: 5px; }

.card-content { flex: 1; padding: 20px; display: flex; flex-direction: column; }
.c-header { display: flex; justify-content: space-between; align-items: flex-start; }
.c-id { font-size: 0.7rem; font-weight: 900; color: var(--accent); font-family: monospace; }
.c-badge { background: #111; color: #fff; font-size: 0.6rem; padding: 2px 6px; font-weight: 900; }
.c-ver { font-size: 0.6rem; color: #ccc; margin-right: 10px; font-family: monospace; }

.c-title { font-size: 1.5rem; font-weight: 900; margin: 10px 0; line-height: 1.1; }
.c-meta { display: flex; gap: 20px; border-top: 1px solid #eee; padding-top: 10px; margin-bottom: 10px; }
.m-item { font-size: 0.8rem; font-weight: bold; }
.m-item .k { color: #999; margin-right: 5px; }

.c-desc { font-size: 0.85rem; color: #666; line-height: 1.5; height: 3em; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }

.c-footer { margin-top: auto; display: flex; justify-content: space-between; align-items: flex-end; }
.tag { font-size: 0.7rem; font-weight: 900; color: #bbb; margin-right: 8px; }
.action-hint { font-size: 0.8rem; font-weight: 900; text-decoration: underline; color: #111; }

/* 装饰印章 */
.card-stamp {
  position: absolute; top: 50%; right: -10px; transform: rotate(-15deg);
  font-size: 4rem; font-weight: 900; color: #000; opacity: 0.03; pointer-events: none;
}

.empty-state { text-align: center; padding: 100px; color: #999; font-weight: 900; letter-spacing: 2px; }

@keyframes blink { 50% { opacity: 0.2; } }
.custom-scroll::-webkit-scrollbar { width: 5px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>