<template>
  <div class="view-section view-guestbook" ref="guestbookRef">
    <div class="guestbook-input-area">
      <div class="gb-input-header">
        <span class="blink">PULSE_INPUT //</span> 请输入加密留言
      </div>
      <div class="gb-form">
        <textarea 
          v-model="newGuestMsg" 
          placeholder="输入内容..." 
          rows="3"
          class="gb-textarea"
        ></textarea>
        <div class="gb-actions">
          <span class="deco-code">CODE: 0x88</span>
          <button class="sign-btn" @click="submitMessage">
            [ 签署并归档 / SIGN ]
          </button>
        </div>
      </div>
    </div>

    <div class="guestbook-list">
      <div class="list-label">历史归档 // ARCHIVE_HISTORY ({{ guestMessages.length }})</div>
      
      <div v-for="msg in guestMessages" :key="msg.id" class="archive-note">
        <div class="pin"></div>
        <div class="note-header">
          <span class="note-id">NO.{{ msg.id.toString().padStart(4, '0') }}</span>
          <span class="note-date">{{ msg.date }}</span>
        </div>
        <div class="note-body">{{ msg.content }}</div>
        <div class="note-footer">
          <div class="signature-block">
            <span class="sig-label">Signed by:</span>
            <span class="sig-name">{{ msg.user }}</span>
          </div>
          <div class="stamp-seal" :class="msg.stampType">{{ msg.stampText }}</div>
        </div>
      </div>
      <div class="end-of-file">--- END OF FILE ---</div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

// 1. 恢复原交互所需的响应式状态
const newGuestMsg = ref('') // 留言输入框绑定值
const guestbookRef = ref(null) // 组件容器ref，用于滚动操作

// 2. 恢复原模拟数据（与原文件一致）
const guestMessages = ref([
  { id: 1024, user: 'NetRunner_01', date: '2023-10-14 22:00', content: '非常喜欢你的配色方案，特别是红色警戒色的运用。期待更多作品！', stampType: 'approved', stampText: 'APPROVED' },
  { id: 1023, user: 'Unknown_V', date: '2023-10-12 10:15', content: 'Ping... 这里的交互逻辑很流畅。使用的是哪个版本的框架？', stampType: 'reviewed', stampText: 'REVIEWED' },
  { id: 1022, user: 'Design_Bot', date: '2023-10-10 09:30', content: '[自动留言] 系统检测到高能级设计作品。', stampType: 'system', stampText: 'LOGGED' },
])

// 3. 恢复原提交留言的交互方法（完整还原逻辑）
const submitMessage = () => {
  // 验证输入非空
  if (!newGuestMsg.value.trim()) return
  
  // 生成当前时间戳（与原格式一致）
  const now = new Date()
  const timeStr = `${now.getFullYear()}-${(now.getMonth()+1).toString().padStart(2,'0')}-${now.getDate().toString().padStart(2,'0')} ${now.getHours()}:${now.getMinutes()}`
  
  // 新增留言到列表头部
  guestMessages.value.unshift({
    id: Math.floor(Math.random() * 10000), // 随机生成ID
    user: 'Visitor_Guest', // 固定访客用户名（与原逻辑一致）
    date: timeStr,
    content: newGuestMsg.value,
    stampType: 'pending', 
    stampText: 'RECEIVED' 
  })
  
  // 清空输入框
  newGuestMsg.value = ''
  
  // 模拟原滚动反馈：提交后滚动到输入区附近
  setTimeout(() => {
    if (guestbookRef.value) {
      guestbookRef.value.scrollTop = 150 
    }
  }, 100)
}
</script>

<style scoped>
/* 引入必要字体（与原文件一致） */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Caveat:wght@700&display=swap');

/* 基础变量定义（与原文件一致） */
.view-guestbook {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-dark: #333;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --sans: 'Noto Sans SC', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  --hand: 'Caveat', cursive;
}

/* 访客留言核心样式（完整还原原文件） */
.view-guestbook { 
  display: flex; 
  flex-direction: column; 
  gap: 30px; 
  max-width: 800px;
  font-family: var(--mono), var(--sans);
  max-height: calc(100vh - 200px); /* 适配容器高度，可根据父容器调整 */
}

.guestbook-input-area { 
  border: 2px solid var(--black); 
  background: #eee; 
  padding: 15px; 
}

.gb-input-header { 
  font-family: var(--mono); 
  font-weight: bold; 
  margin-bottom: 10px; 
  font-size: 0.8rem; 
  color: #555; 
}

.gb-form { 
  display: flex; 
  flex-direction: column; 
  gap: 10px; 
}

.gb-textarea { 
  width: 100%; 
  border: 1px solid var(--black); 
  padding: 10px; 
  font-family: var(--sans); 
  background: var(--white); 
  resize: vertical; 
  min-height: 80px; 
  box-sizing: border-box; 
}

.gb-textarea:focus { 
  outline: 2px solid var(--red); 
}

.gb-actions { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
}

.deco-code { 
  font-family: var(--mono); 
  font-size: 0.7rem; 
  color: #888; 
}

.sign-btn { 
  background: var(--black); 
  color: var(--white); 
  border: none; 
  padding: 8px 20px; 
  font-family: var(--mono); 
  font-weight: bold; 
  cursor: pointer; 
  transition: 0.2s; /* 恢复hover过渡动画 */
}

/* 恢复按钮hover交互样式 */
.sign-btn:hover { 
  background: var(--red); 
}

.guestbook-list { 
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
  margin-top: 1%; 
}

.list-label { 
  font-family: var(--heading); 
  font-size: 1.2rem; 
  border-bottom: 2px solid var(--black); 
  padding-bottom: 5px; 
  margin-bottom: 10px; 
}

.end-of-file { 
  text-align: center; 
  font-family: var(--mono); 
  color: #ccc; 
  margin-top: 20px; 
  font-size: 0.8rem; 
}

/* 留言归档卡片样式（完整还原） */
.archive-note { 
  background: var(--white); 
  border: 1px solid #999; 
  padding: 20px; 
  position: relative; 
  max-height: 300px;
  overflow-y: auto;
  box-shadow: 3px 3px 0 rgba(0,0,0,0.1); 
}
/* .archive-note 专属自定义滚动条样式 */
.archive-note::-webkit-scrollbar {
  /* 滚动条整体宽度 */
  width: 6px;
}

.archive-note::-webkit-scrollbar-track {
  /* 滚动条轨道（背景），设置为 #F4F1EA */
  background: #F4F1EA;
  /* 可选：添加轻微圆角，优化视觉 */
  border-radius: 3px;
}

.archive-note::-webkit-scrollbar-thumb {
  /* 滚动条滑块（可拖动部分），与组件风格保持一致 */
  background: #888888;
  /* 可选：添加轻微圆角，优化视觉 */
  border-radius: 3px;
  /* 可选：添加hover效果，增强交互感 */
  transition: all 0.2s;
}

/* 可选：滚动条滑块hover变色，与组件红色主题呼应 */
.archive-note::-webkit-scrollbar-thumb:hover {
  background: var(--red);
}

.pin { 
  width: 12px; 
  height: 12px; 
  background: #aaa; 
  border-radius: 50%; 
  position: absolute; 
  top: 10px; 
  left: 50%; 
  transform: translateX(-50%); 
  box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3); 
}

.note-header { 
  display: flex; 
  justify-content: space-between; 
  font-family: var(--mono); 
  font-size: 0.75rem; 
  color: #666; 
  border-bottom: 1px solid #ddd; 
  padding-bottom: 8px; 
  margin-bottom: 10px; 
}

.note-body { 
  font-family: var(--sans); 
  line-height: 1.6; 
  font-size: 0.95rem; 
  margin-bottom: 20px; 
  min-height: 40px; 
}

.note-footer { 
  display: flex; 
  justify-content: space-between; 
  align-items: flex-end; 
  position: relative; 
}

.signature-block { 
  display: flex; 
  flex-direction: column; 
}

.sig-label { 
  font-size: 0.6rem; 
  color: #888; 
  font-family: var(--sans); 
}

.sig-name { 
  font-family: var(--hand), cursive; 
  font-size: 1.5rem; 
  color: var(--black); 
  transform: rotate(-2deg); 
  margin-top: -5px; 
}

.stamp-seal { 
  position: absolute; 
  right: 0; 
  bottom: -5px; 
  border: 3px double; 
  padding: 5px 10px; 
  font-family: var(--heading); 
  font-size: 1rem; 
  letter-spacing: 2px; 
  transform: rotate(-15deg); 
  opacity: 0.7; 
  mix-blend-mode: multiply; 
  text-transform: uppercase; 
}

.stamp-seal.approved { 
  color: var(--red); 
  border-color: var(--red); 
}

.stamp-seal.reviewed { 
  color: #0099ff; 
  border-color: #0099ff; 
}

.stamp-seal.system { 
  color: #333; 
  border-color: #333; 
  transform: rotate(0deg); 
  border-style: solid; 
  background: #eee; 
}

.stamp-seal.pending { 
  color: #ffaa00; 
  border-color: #ffaa00; 
  border-style: dashed; 
}

/* 恢复闪烁动画（原文件交互动效） */
@keyframes blink { 
  0%, 100% { opacity: 1; } 
  50% { opacity: 0; } 
}

.blink { 
  animation: blink 1s infinite; 
}

/* 恢复自定义滚动条（与原文件一致） */
.view-guestbook::-webkit-scrollbar { width: 8px; }
.view-guestbook::-webkit-scrollbar-thumb { background: var(--black); }
.view-guestbook::-webkit-scrollbar-track { background: #eee; }
</style>