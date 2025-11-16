<template>
  <div class="recommendations-area">
    <header class="recommendations-header">
      <h2>🔥 作品推荐</h2>
      <p>探索历史佳作与当前最火的优秀作品</p>
    </header>
    <div class="recommendations-content">
      <!-- 当前最火热 -->
      <section class="hot-recommendations">
        <h3>当前最火热 🔥</h3>
        <div class="recommendation-cards">
          <div
            v-for="hot in hotRecommendations"
            :key="hot.id"
            class="recommendation-card"
          >
            <img :src="hot.image" alt="作品图" class="recommendation-image" />
            <h4>{{ hot.title }}</h4>
            <p>{{ hot.description }}</p>
          </div>
        </div>
      </section>
      <!-- 历史作品 -->
      <section class="history-recommendations">
        <h3>历史佳作 📜</h3>
        <div class="recommendation-cards">
          <div
            v-for="history in historyRecommendations"
            :key="history.id"
            class="recommendation-card"
          >
            <img :src="history.image" alt="作品图" class="recommendation-image" />
            <h4>{{ history.title }}</h4>
            <p>{{ history.description }}</p>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
const hotRecommendations = [
  {
    id: 1,
    image: "https://via.placeholder.com/150",
    title: "热血战斗场景",
    description: "展现极致的激情与角色魅力。"
  },
  {
    id: 2,
    image: "https://via.placeholder.com/150",
    title: "火柴人的冒险",
    description: "探索未知领域，勇敢无畏的火柴人！"
  },
]

const historyRecommendations = [
  {
    id: 1,
    image: "https://via.placeholder.com/150",
    title: "时代经典作品",
    description: "火柴人动画的永久记忆。"
  },
  {
    id: 2,
    image: "https://via.placeholder.com/150",
    title: "史诗级战斗",
    description: "每一帧都历历在目，扣人心弦。"
  },
]
</script>

<style scoped>
.recommendations-area {
  padding: 20px;
}
.recommendations-header {
  text-align: center;
  margin-bottom: 24px;
}
.recommendations-header h2 {
  font-size: 24px;
  font-weight: bold;
  color: #222;
}
.recommendations-header p {
  font-size: 16px;
  color: #555;
}

/* 区块内容 */
.recommendations-content {
  display: flex;
  flex-direction: column;
  gap: 30px;
}
.hot-recommendations,
.history-recommendations {
  margin-top: 10px;
}
.recommendation-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 15px;
}
.recommendation-card {
  background: #fff;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
  padding: 12px;
  text-align: center;
}
.recommendation-card img {
  max-width: 100%;
  border-radius: 8px;
  margin-bottom: 12px;
}
.recommendation-card h4 {
  font-size: 18px;
  font-weight: bold;
  color: #1a202c;
}
.recommendation-card p {
  color: #4a5568;
  font-size: 14px;
}
</style>