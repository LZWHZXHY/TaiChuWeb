<template>
  <div class="test-wrapper">
    
    <div v-if="currentView === 'list'" class="list-view">
      <div class="header-section">
        <h2>实验室</h2>
        <p>探索正在孵化中的新功能，可能会不稳定，但一定很有趣。</p>
      </div>

      <div class="card-grid">
        <div v-for="item in features" :key="item.id" class="feature-card">
          <div class="card-cover">
            <div class="icon-container" v-html="item.icon"></div>
            <div class="status-tag" :class="item.statusColor">
              {{ item.statusText }}
            </div>
          </div>
          <div class="card-content">
            <h3>{{ item.title }}</h3>
            <p class="desc">{{ item.desc }}</p>
            <div class="card-footer">
              <span class="version">Ver {{ item.version }}</span>
              <button class="jump-btn" @click="handleJump(item)">
                立即体验
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line><polyline points="12 5 19 12 12 19"></polyline></svg>
              </button>
            </div>
          </div>
        </div>

        <div class="feature-card placeholder">
          <div class="placeholder-content">
            <span class="plus-icon">+</span>
            <p>更多黑科技<br>正在编译中...</p>
          </div>
        </div>
      </div>
    </div>

    <AiChat v-else-if="currentView === 'chat'" @goBack="currentView = 'list'" />

  </div>
</template>

<script setup>
import { ref } from 'vue'
import AiChat from './Test/AiChat.vue'

const currentView = ref('list')

// 新设计的 SVG：外环细，内环粗且空心
const iconSvg = `
<svg class="tch-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
  <circle cx="12" cy="12" r="10" stroke-width="1.5"></circle>
  <circle cx="12" cy="12" r="4.5" stroke-width="3.5"></circle>
</svg>
`

const features = ref([
  {
    id: 1,
    title: '太初 Ai Chat',
    desc: '基于 DeepSeek V3 模型的智能对话助手。支持上下文理解、代码生成与创意写作。',
    icon: iconSvg,
    statusText: 'Alpha 测试',
    statusColor: 'tag-red',
    version: '0.9.1',
    type: 'chat' 
  },
])

const handleJump = (item) => {
  if (item.type === 'chat') {
    currentView.value = 'chat'
  } else {
    alert('该功能暂未开放')
  }
}
</script>

<style scoped>
/* 容器 */
.test-wrapper { width: 100%; height: 100%; overflow: hidden; }

/* 列表视图 */
.list-view { width: 100%; height: 100%; padding: 40px; overflow-y: auto; display: flex; flex-direction: column; }
.header-section { margin-bottom: 30px; }
.header-section h2 { font-size: 28px; font-weight: 800; color: #1a1a1a; margin-bottom: 8px; }
.header-section p { color: #666; font-size: 14px; }

/* Grid 布局 */
.card-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 24px; padding-bottom: 40px; }

/* 卡片样式 */
.feature-card { background: #fff; border: 1px solid #eee; border-radius: 16px; overflow: hidden; min-height: 360px; display: flex; flex-direction: column; transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1); box-shadow: 0 4px 6px rgba(0,0,0,0.02); position: relative; }
.feature-card:hover { transform: translateY(-5px); box-shadow: 0 12px 24px rgba(0,0,0,0.08); border-color: #000; }

/* 封面区 */
.card-cover { height: 160px; background: #f4f4f5; display: flex; justify-content: center; align-items: center; position: relative; }

/* 图标容器 - 控制大小 */
.icon-container { width: 80px; height: 80px; color: #1a1a1a; display: flex; align-items: center; justify-content: center; }
/* 深度选择器确保 v-html 内部 SVG 响应大小 */
:deep(.tch-icon) { width: 100%; height: 100%; }

.status-tag { position: absolute; top: 12px; right: 12px; padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 700; }
.tag-red { background: #ffe5e5; color: #d93025; }

/* 内容区 */
.card-content { flex: 1; padding: 24px; display: flex; flex-direction: column; }
.card-content h3 { font-size: 20px; font-weight: 700; margin: 0 0 12px 0; color: #1a1a1a; }
.card-content .desc { font-size: 14px; color: #666; line-height: 1.6; margin-bottom: 24px; flex: 1; }

.card-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #f0f0f0; padding-top: 16px; }
.version { font-size: 12px; color: #999; font-family: monospace; }
.jump-btn { display: flex; align-items: center; gap: 6px; background: #1a1a1a; color: #fff; border: none; padding: 8px 16px; border-radius: 8px; font-size: 14px; font-weight: 600; cursor: pointer; transition: background 0.2s; }
.jump-btn:hover { background: #000; }

/* 占位卡片 */
.feature-card.placeholder { border: 2px dashed #e0e0e0; background: transparent; box-shadow: none; cursor: default; }
.placeholder-content { width: 100%; height: 100%; display: flex; flex-direction: column; justify-content: center; align-items: center; color: #aaa; text-align: center; }
.plus-icon { font-size: 40px; font-weight: 300; margin-bottom: 10px; }
</style>