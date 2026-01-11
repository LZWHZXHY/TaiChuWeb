<template>
  <div class="settings-panel">
    <!-- 面板内容 -->
    <div class="panel-content">
      <!-- 侧边导航 -->
      <nav class="settings-nav">
        <div class="nav-section">
          <h3 class="section-title">信息分类</h3>
          <ul class="nav-list">
            <li
              v-for="item in navItems"
              :key="item.id"
              class="nav-item"
              :class="{ 'nav-item--active': activeTab === item.id }"
              @click="setActiveTab(item.id)"
            >
              <span class="nav-text">{{ item.name }}</span>
            </li>
          </ul>
        </div>
      </nav>

      <!-- 内容区域 -->
      <main class="settings-main">
        <!-- 社区介绍 -->
        <section v-if="activeTab === 'intro'" class="content-section">
          <div class="content-body">
            <CommunityIntro />
          </div>
        </section>

        <!-- 社区规则 -->
        <section v-if="activeTab === 'rules'" class="content-section">
          <div class="content-header">
            <h3>社区规则</h3>
            <p>请遵守以下规则，共同维护良好社区环境</p>
          </div>
          <div class="content-body">
            <CommunityRules />
          </div>
        </section>

        <!-- 日历 -->
        <section v-if="activeTab === 'calendar'" class="content-section">
          <div class="content-header">
            <h3>社区日历</h3>
            <p>查看社区活动和重要日期安排</p>
          </div>




          <div class="content-body">
            <CommunityCalendar />
            
          </div>
        </section>

        <!-- 关于我们 -->
        <section v-if="activeTab === 'update'" class="content-section">
          
          <div class="content-body">
            <CommnuityUpdate />
          </div>
        </section>




        <!-- 关于我们 -->
        <section v-if="activeTab === 'about'" class="content-section">
          <div class="content-header">
            <h3>关于我们</h3>
            <p>了解太初寰宇团队的使命和愿景</p>
          </div>
          <div class="content-body">
            <div class="content-placeholder">
              <p>关于我们内容将在这里显示</p>
            </div>
          </div>
        </section>

        <!-- 财政记录 -->
        <section v-if="activeTab === 'finance'" class="content-section">
          <div class="content-header">
            <h3>财政记录</h3>
            <p>平台运营收支透明公开</p>
          </div>
          <div class="content-body">
            <CommunityFinancial />
          </div>
        </section>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import CommunityIntro from '@/homeComponents/CommunityIntro.vue'
import CommunityRules from '@/homeComponents/CommunityRules.vue'
import CommunityCalendar from '@/homeComponents/CommunityCalendar.vue'
import CommunityFinancial from '@/homeComponents/CommunityFinancial.vue'
import CommnuityUpdate from '@/homeComponents/CommnuityUpdate.vue'


// 导航项配置
const navItems = [
  { id: 'intro', name: '社区介绍' },
  { id: 'rules', name: '社区规则' },
  { id: 'calendar', name:'社区日历'},
  { id: 'update', name: '更新日志' },
  { id: 'about', name: '关于我们' },
  { id: 'finance', name: '财政记录' }
]

const activeTab = ref('intro')

const setActiveTab = (tabId) => {
  activeTab.value = tabId
}
</script>

<style scoped>
/* 日历容器样式 */
.calendar-container {
  max-width: 100%;
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* 日历头部 */
.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e0e0e0;
}

.calendar-nav {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.nav-btn {
  background: #f8f9fa;
  border: 1px solid #ddd;
  width: 40px;
  height: 40px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.nav-btn:hover {
  background: #e9ecef;
  border-color: #adb5bd;
}

.current-month {
  font-size: 1.4rem;
  font-weight: 600;
  color: #333;
  min-width: 150px;
  text-align: center;
}

.calendar-actions {
  display: flex;
  gap: 1rem;
}

.action-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: background 0.2s ease;
}

.action-btn:hover {
  background: #0056b3;
}

/* 星期标题 */
.calendar-week-header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 1rem;
  text-align: center;
  font-weight: 600;
  color: #666;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f0f0f0;
}

.week-day {
  padding: 0.5rem;
}

/* 日历网格 */
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 0.5rem;
  margin-bottom: 2rem;
}

.calendar-cell {
  aspect-ratio: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  padding: 0.75rem;
  position: relative;
  border: 1px solid #f0f0f0;
  transition: all 0.2s ease;
}

.calendar-cell.current-month {
  background: white;
  color: #333;
}

.calendar-cell.other-month {
  background: #f8f9fa;
  color: #999;
}

.calendar-cell.today {
  background: #e3f2fd;
  border-color: #2196f3;
  font-weight: 600;
}

.calendar-cell.has-event {
  background: #fff3e0;
  border-color: #ff9800;
}

.date-number {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.event-indicator {
  position: absolute;
  bottom: 0.25rem;
}

.event-dot {
  display: block;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #ff9800;
}

/* 事件列表 */
.events-section {
  border-top: 2px solid #f0f0f0;
  padding-top: 1.5rem;
}

.events-section h4 {
  margin: 0 0 1rem 0;
  color: #333;
  font-size: 1.2rem;
  font-weight: 600;
}

.events-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.event-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  border-left: 4px solid #007bff;
}

.event-date {
  font-weight: 600;
  color: #007bff;
  min-width: 80px;
}

.event-title {
  flex: 1;
  color: #333;
  font-weight: 500;
}

.event-type {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.event-type-tech {
  background: #e3f2fd;
  color: #1976d2;
}

.event-type-social {
  background: #f3e5f5;
  color: #7b1fa2;
}

.event-type-update {
  background: #e8f5e8;
  color: #388e3c;
}

.no-events {
  text-align: center;
  padding: 2rem;
  color: #999;
  background: #f8f9fa;
  border-radius: 8px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .calendar-container {
    padding: 1rem;
  }
  
  .calendar-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .calendar-nav {
    justify-content: center;
  }
  
  .calendar-actions {
    justify-content: center;
  }
  
  .calendar-grid {
    gap: 0.25rem;
  }
  
  .calendar-cell {
    padding: 0.5rem;
  }
  
  .date-number {
    font-size: 0.9rem;
  }
  
  .event-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .event-date {
    min-width: auto;
  }
}

/* 保持原有的样式不变 */
.settings-panel {
  width: 100%;
  margin: 0;
  background: white;
  border-radius: 0;
  box-shadow: none;
  overflow: hidden;
  min-height: 100vh;
}

.panel-content {
  display: grid;
  grid-template-columns: minmax(250px, 300px) 1fr;
  min-height: calc(100vh - 200px);
  width: 100%; 
  margin-left: -50vw;
  left: 50%;
  position: relative;
}

.settings-nav {
  background: #f8f9fa;
  border-right: 1px solid #e0e0e0;
  padding: 2rem;
  height: 100%;
  width: 100%;
  box-sizing: border-box;
}

.section-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 1.5rem;
  padding: 0;
}

.nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 1.5rem 2rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  color: #555;
  margin-bottom: 0.75rem;
  font-size: 1.2rem;
  font-weight: 500;
  border: 1px solid transparent;
  width: 100%;
  box-sizing: border-box;
}

.nav-item:hover {
  background: #e9ecef;
  color: #333;
  border-color: #d0d7de;
}

.nav-item--active {
  background: #007bff;
  color: white;
  border-color: #007bff;
  box-shadow: 0 2px 12px rgba(0, 123, 255, 0.3);
}

.nav-text {
  flex: 1;
  font-weight: 500;
}

.settings-main {
  padding: 3rem;
  background: rgb(255, 255, 255);
  min-height: 100%;
  width: 100%;
  overflow-x: hidden;
  box-sizing: border-box;
}

.content-section {
  animation: fadeIn 0.3s ease;
  height: 100%;
  display: flex;
  flex-direction: column;
  width: 100%;
  max-width: 100%;
}

.content-header {
  margin-bottom: 3rem;
  padding-bottom: 2rem;
  border-bottom: 2px solid #f0f0f0;
  width: 100%;
  max-width: 100%;
}

.content-header h3 {
  font-size: 2.2rem;
  font-weight: 600;
  color: #333;
  margin: 0 0 1rem 0;
  word-wrap: break-word;
}

.content-header p {
  color: #666;
  margin: 0;
  font-size: 1.3rem;
  font-weight: 400;
  word-wrap: break-word;
}

.content-body {
  flex: 1;
  min-height: 500px;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow: hidden;
}

.content-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 500px;
  background: #f8f9fa;
  border-radius: 8px;
  color: #999;
  font-size: 1.5rem;
  border: 2px dashed #ddd;
  font-weight: 500;
  width: 100%;
  box-sizing: border-box;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 1200px) {
  .panel-content {
    grid-template-columns: minmax(220px, 250px) 1fr;
    width: 100vw;
  }
  
  .settings-nav {
    padding: 1.5rem;
  }
  
  .nav-item {
    padding: 1.25rem 1.5rem;
    font-size: 1.1rem;
  }
  
  .settings-main {
    padding: 2rem;
    max-width: calc(100vw - 250px);
  }
  
  .content-header h3 {
    font-size: 2rem;
  }
}
</style>