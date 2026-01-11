<template>
  <div class="chronicle-wrapper">
    <div class="filter-sidebar">
      <h4>筛选主体</h4>
      </div>

    <div class="timeline-stream">
      <div v-for="event in eventList" :key="event.id" class="event-item">
        <div class="time-point">{{ event.timeLabel }}</div>
        <div class="event-card">
          <div class="card-header">
            <h3>{{ event.title }}</h3>
            <span class="author-tag">
              ✍️ {{ event.author || '佚名' }}
            </span>
          </div>
          
          <p class="summary-text">{{ event.summary }}</p>
          
          <div class="tags" v-if="event.relatedEntities.length > 0">
            <span v-for="tag in event.relatedEntities" :key="tag">{{ tag }}</span>
          </div>
        </div>
      </div>
      
      <div v-if="eventList.length === 0 && !loading" class="empty-tip">
        暂无编年史记录
      </div>
    </div>
  </div>
</template>

<script>
import apiClient from '@/utils/api';

export default {
  name: 'ChronicleView',
  props: {
    ipId: {
      type: [String, Number],
      required: true
    }
  },
  data() {
    return {
      loading: false,
      eventList: [], 
      filterEntity: '' 
    };
  },
  mounted() {
    this.fetchEvents();
  },
  methods: {
    async fetchEvents() {
      this.loading = true;
      try {
        const res = await apiClient.get(`/Events?ipId=${this.ipId}`);
        
        // 数据映射 (Mapping)
        this.eventList = res.data.map(ev => ({
          id: ev.id,
          title: ev.title,
          summary: ev.summary,
          
          // ✅ 新增：映射 author 字段
          author: ev.author, 
          
          // 后端叫 time_label -> 前端 timeLabel
          timeLabel: ev.time_label, 
          
          // 处理关联实体显示格式
          relatedEntities: ev.EventNodes 
            ? ev.EventNodes.map(n => `${n.name} [${n.role}]`) 
            : []
        }));

      } catch (error) {
        console.error("加载纪事失败:", error);
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>

<style scoped>
.chronicle-wrapper { display: flex; height: 100%; overflow: hidden; background: #f9f9f9; }

.filter-sidebar {
  width: 200px; padding: 20px; background: white; 
  border-right: 1px solid #eee; display: none; /* 暂时隐藏，等做筛选功能再开 */
}

.timeline-stream { 
  flex: 1; overflow-y: auto; padding: 40px 40px 40px 80px; 
  /* 左边距加大，给时间标签留位置 */
  border-left: 2px solid #e0e0e0; margin-left: 100px; 
}

.event-item { position: relative; margin-bottom: 40px; }

.time-point { 
  position: absolute; left: -140px; top: 0; width: 100px; 
  text-align: right; font-weight: bold; color: #1890ff; font-size: 1.1rem;
}

/* 时间轴圆点 */
.event-item::before {
  content: ''; position: absolute; left: -89px; top: 6px;
  width: 14px; height: 14px; background: #fff; 
  border: 3px solid #1890ff; border-radius: 50%; z-index: 1;
}

.event-card {
  background: white; padding: 20px; border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05); border: 1px solid #eaeaea;
  transition: transform 0.2s;
}
.event-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.1); }

.card-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 10px; border-bottom: 1px solid #f0f0f0; padding-bottom: 10px;
}

.card-header h3 { margin: 0; font-size: 1.2rem; color: #333; }

/* ✅ 作者标签样式 */
.author-tag { font-size: 0.85rem; color: #999; background: #f5f5f5; padding: 2px 8px; border-radius: 4px; }

.summary-text { color: #555; line-height: 1.6; margin-bottom: 15px; white-space: pre-wrap; }

.tags { display: flex; flex-wrap: wrap; gap: 8px; }
.tags span {
  display: inline-block; background-color: #e6f7ff; color: #1890ff;
  padding: 2px 10px; border-radius: 12px; font-size: 0.8rem;
  border: 1px solid #91d5ff;
}

.empty-tip { text-align: center; color: #999; margin-top: 50px; }
</style>