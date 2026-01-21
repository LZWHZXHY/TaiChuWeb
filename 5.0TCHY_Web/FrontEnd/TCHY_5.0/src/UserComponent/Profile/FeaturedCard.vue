<template>
  <section v-if="list.length > 0" class="pinned-wrapper">
    <div class="pinned-header">
      <h3>📌 置顶推荐</h3>
    </div>

    <div class="pinned-container">
      <div 
        v-for="item in list" 
        :key="item.id" 
        class="pinned-card"
        :class="item.type"
        @click="goToDetail(item)"
      >
        <span class="tag">{{ formatType(item.type) }}</span>

        <div v-if="item.type === 'work'" class="media-area">
          <img :src="item.cover" alt="" />
        </div>

        <div class="content-area">
          <h4>{{ item.title }}</h4>
          <p v-if="item.type !== 'work'" class="desc">{{ item.desc }}</p>
          <span class="date">{{ item.date }}</span>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
// 接收父组件处理好的数据
const props = defineProps({
  list: {
    type: Array,
    default: () => []
  }
});

const formatType = (type) => {
  const map = { work: '精选作品', blog: '技术博客', post: '生活动态' };
  return map[type] || '置顶';
};

const goToDetail = (item) => {
  // 根据 item.type 和 item.id 跳转到你现有的详情页
  console.log('跳转逻辑', item);
};
</script>

<style scoped>
.pinned-wrapper {
  margin-bottom: 30px; /* 和下方内容的间距 */
  padding: 20px;
  background: #f9f9fa; /* 给一个极淡的背景区分 */
  border-radius: 12px;
}

.pinned-header h3 {
  margin: 0 0 15px 0;
  font-size: 1.1rem;
}

.pinned-container {
  display: flex;
  gap: 15px;
  overflow-x: auto; /* 手机端支持横向滑动 */
  flex-wrap: wrap;  /* 桌面端换行 */
}

.pinned-card {
  position: relative;
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
  width: 100%;
  flex: 1 1 300px; /* 自适应宽度 */
  cursor: pointer;
  transition: transform 0.2s;
  overflow: hidden;
}

.pinned-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.tag {
  position: absolute;
  top: 8px; 
  right: 8px;
  background: rgba(0,0,0,0.7);
  color: #fff;
  font-size: 12px;
  padding: 2px 6px;
  border-radius: 4px;
  z-index: 2;
}

.media-area img {
  width: 100%;
  height: 140px;
  object-fit: cover;
  display: block;
}

.content-area {
  padding: 12px;
}

.content-area h4 {
  margin: 0 0 6px 0;
  font-size: 1rem;
}

.desc {
  font-size: 0.85rem;
  color: #666;
  margin-bottom: 8px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.date {
  font-size: 0.75rem;
  color: #999;
}
</style>