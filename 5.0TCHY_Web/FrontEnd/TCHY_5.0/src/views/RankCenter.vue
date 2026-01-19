<template>
  <div class="dashboard-container">
    <header class="main-header">
      <h2>🏆 全站排行榜概览</h2>
    </header>

    <div v-if="loading" class="loading-state">
      正在加载数据...
    </div>

    <div v-else class="rank-grid">
      
      <div 
        v-for="(list, key) in rankingsMap" 
        :key="key" 
        class="rank-card"
      >
        <div class="card-header">
          <h3>{{ getMeta(key).icon }} {{ getMeta(key).title }}</h3>
          <span class="unit-label">{{ getMeta(key).unit }}</span>
        </div>

        <div class="card-body">
          <ul class="rank-list">
            <li v-for="(item, index) in list" :key="item.id" class="rank-item">
              
              <div class="rank-num">
                <span :class="['badge', `top-${index + 1}`]">{{ index + 1 }}</span>
              </div>

              <div class="user-info">
                <img :src="item.avatar || 'https://via.placeholder.com/32'" class="avatar" alt="avatar"/>
                <span class="name text-ellipsis">{{ item.name }}</span>
              </div>

              <div class="score">
                {{ item.value }}
              </div>
            </li>
          </ul>

          <div v-if="list.length === 0" class="empty-tip">
            暂无上榜数据
          </div>
        </div>

        <div class="card-footer" @click="handleViewMore(key)">
          查看全部 >
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import apiClient from '@/utils/api';

const loading = ref(true);

// 定义各个榜单的元数据 (标题、图标、单位)
const metaConfig = {
  active: { title: '活跃大佬榜', icon: '🔥', unit: '活跃度' },
  level:  { title: '等级排行榜', icon: '⭐', unit: 'Lv.' },
  points: { title: '富豪点数榜', icon: '💰', unit: '点数' },
  // 你可以在这里无限添加新的榜单类型
};

// 存储数据的对象
const rankingsMap = ref({
  active: [],
  level: [],
  points: []
});

// 辅助函数：获取元数据
const getMeta = (key) => metaConfig[key] || { title: '榜单', icon: '🏆', unit: '' };

const handleViewMore = (type) => {
  console.log(`点击了查看更多: ${type}`);
  // 这里可以跳转到只有该榜单的详情页
  // router.push(`/rank/${type}`);
};

const fetchAllRankings = async () => {
  loading.value = true;
  try {
    // 真实环境：你可以并行请求多个接口，或者请求一个聚合接口
    // const [resActive, resLevel] = await Promise.all([
    //   apiClient.get('/ranks/active'),
    //   apiClient.get('/ranks/level')
    // ]);

    // --- 模拟数据 (开发测试用) ---
    await new Promise(r => setTimeout(r, 600));
    
    rankingsMap.value = {
      active: [
        { id: 1, name: '龙傲天', avatar: '', value: 9850 },
        { id: 2, name: '赵日天', avatar: '', value: 8900 },
        { id: 3, name: '叶良辰', avatar: '', value: 7600 },
        { id: 4, name: '路人甲', avatar: '', value: 5400 },
        { id: 5, name: '路人乙', avatar: '', value: 3200 },
      ],
      level: [
        { id: 10, name: '肝帝', avatar: '', value: 99 },
        { id: 11, name: '练级狂', avatar: '', value: 88 },
        { id: 12, name: '新手导师', avatar: '', value: 75 },
        { id: 1, name: '龙傲天', avatar: '', value: 60 },
        { id: 13, name: '萌新', avatar: '', value: 12 },
      ],
      points: [
        { id: 99, name: '马斯克', avatar: '', value: '999k' },
        { id: 98, name: '盖茨', avatar: '', value: '888k' },
        { id: 10, name: '肝帝', avatar: '', value: '500k' },
      ]
    };
    // -------------------------

  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchAllRankings();
});
</script>

<style scoped>
/* 容器 */
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.main-header {
  text-align: center;
  margin-bottom: 30px;
  color: #333;
}

/* --- 核心网格布局 --- */
.rank-grid {
  display: grid;
  /* 自动适应列宽：最小300px，空间足够则平铺 */
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); 
  gap: 20px;
  align-items: start; /* 让卡片高度根据内容自适应，不强行拉伸 */
}

/* 单个卡片样式 */
.rank-card {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  border: 1px solid #f0f0f0;
  transition: transform 0.2s;
}

.rank-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

/* 卡片头部 */
.card-header {
  background: linear-gradient(to right, #f8f9fa, #fff);
  padding: 15px 20px;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  margin: 0;
  font-size: 18px;
  color: #333;
}

.unit-label {
  font-size: 12px;
  color: #999;
  background: #eee;
  padding: 2px 6px;
  border-radius: 4px;
}

/* 列表区域 */
.card-body {
  padding: 0;
}

.rank-item {
  display: flex;
  align-items: center;
  padding: 12px 20px;
  border-bottom: 1px solid #f9f9f9;
}

.rank-item:last-child {
  border-bottom: none;
}

/* 排名数字 */
.rank-num {
  width: 30px;
  margin-right: 10px;
}

.badge {
  display: inline-block;
  width: 20px;
  height: 20px;
  line-height: 20px;
  text-align: center;
  border-radius: 4px;
  font-size: 12px;
  color: #999;
  background: #f5f5f5;
  font-weight: bold;
}

/* 前三名高亮 */
.top-1 { background: #FFD700; color: #fff; } /* 金 */
.top-2 { background: #C0C0C0; color: #fff; } /* 银 */
.top-3 { background: #CD7F32; color: #fff; } /* 铜 */

/* 用户信息 */
.user-info {
  flex: 1;
  display: flex;
  align-items: center;
  overflow: hidden; /* 防止名字太长撑坏布局 */
}

.avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  margin-right: 10px;
  background: #ddd;
}

.name {
  font-size: 14px;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 分数 */
.score {
  font-weight: bold;
  color: #409eff;
  font-size: 14px;
  margin-left: 10px;
}

/* 底部 查看更多 */
.card-footer {
  text-align: center;
  padding: 10px;
  background: #fafafa;
  color: #666;
  font-size: 13px;
  cursor: pointer;
  border-top: 1px solid #eee;
}

.card-footer:hover {
  color: #409eff;
  background: #f0f7ff;
}

.loading-state, .empty-tip {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>