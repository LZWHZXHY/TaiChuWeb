<template>
  <div class="cyber-card archive-card-content">
    <div class="archive-header">
      <span class="blink">> PERSONAL_DATA</span>
      <button class="close-btn" @click="handleClose">×</button>
    </div>
    
    <div class="archive-body custom-scroll">
      <div class="data-group">
        <div class="group-title">01. 基础识别 // BASIC_ID</div>
        <div class="info-grid">
          <div class="info-cell">
            <span class="label">AGE</span>
            <span class="value">{{ user.age }}</span>
          </div>
          <div class="info-cell">
            <span class="label">SEX</span>
            <span class="value">{{ user.gender }}</span>
          </div>
          <div class="info-cell full-width">
            <span class="label">LOC</span>
            <span class="value">{{ user.location }}</span>
          </div>
          <div class="info-cell full-width">
            <span class="label">EXP</span>
            <span class="value">{{ user.creationAge }} (since 2019)</span>
          </div>
        </div>
      </div>

      <div class="data-group">
        <div class="group-title">02. 通讯终端 // CONNECT</div>
        <div class="contact-list">
          <a :href="`mailto:${user.email}`" class="contact-item">
            <span class="c-icon">✉</span>
            <div class="c-detail">
              <span class="c-label">EMAIL_LINK</span>
              <span class="c-val">{{ user.email }}</span>
            </div>
            <span class="c-arrow">>></span>
          </a>
          <a :href="`tencent://message/?uin=${user.qq}&Site=Sambow&Menu=yes`" class="contact-item">
            <span class="c-icon">🐧</span>
            <div class="c-detail">
              <span class="c-label">TENCENT_QQ</span>
              <span class="c-val">{{ user.qq }}</span>
            </div>
            <span class="c-arrow">>></span>
          </a>
        </div>
      </div>

      <div class="data-group">
        <div class="group-title">03. 外部链路 // LINKS</div>
        <div class="link-buttons">
          <a v-for="link in user.externalLinks" :key="link.name" :href="link.url" target="_blank" class="cyber-link-btn">
            [{{ link.name }}]
          </a>
        </div>
      </div>

      <div class="barcode-area">
        <div class="barcode"></div>
        <span class="code-num">UID: {{ user.uid }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>

// 定义Props接收用户数据
const props = defineProps({
  user: {
    type: Object,
    required: true,
    default: () => ({})
  }
})

// 定义关闭事件，传递给父组件
const emit = defineEmits(['close'])
const handleClose = () => emit('close')
</script>

<style scoped>
/* 复用原全局变量，保证样式一致性 */
:root {
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

/* 原archive-card-content相关样式 */
.archive-card-content {
  width: 100%; height: 100%;
  background: #222; color: #ddd;
  display: flex; flex-direction: column;
  text-align: left;
  border: 2px solid var(--black);
}
.archive-header {
  background: #000; padding: 10px 15px;
  display: flex; justify-content: space-between; align-items: center;
  border-bottom: 1px dashed #444; font-family: var(--mono); font-size: 0.8rem; font-weight: bold; color: #0f0;
}
.close-btn { background: none; border: none; color: #fff; font-size: 2rem; cursor: pointer; line-height: 1; }
.close-btn:hover { color: var(--red); }

.archive-body { padding: 15px; display: flex; flex-direction: column; gap: 20px; flex: 1; overflow-y: auto; background: #000000;}

/* 数据组样式 */
.data-group { display: flex; flex-direction: column; gap: 8px; }
.group-title { font-size: 0.7rem; color: #666; font-weight: bold; border-left: 3px solid var(--red); padding-left: 8px; font-family: var(--mono); text-transform: uppercase; }

/* 基础信息网格 */
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.info-cell { background: #111; border: 1px solid #333; padding: 8px; display: flex; flex-direction: column; }
.info-cell.full-width { grid-column: span 2; }
.info-cell .label { font-size: 0.6rem; color: #888; font-family: var(--mono); margin-bottom: 2px; }
.info-cell .value { font-size: 0.85rem; font-weight: bold; color: #fff; font-family: var(--sans); }

/* 联系方式列表 */
.contact-list { display: flex; flex-direction: column; gap: 8px; }
.contact-item { 
  display: flex; align-items: center; gap: 10px; 
  background: #111; border: 1px solid #333; padding: 10px; 
  text-decoration: none; color: #ccc; transition: all 0.2s; 
}
.contact-item:hover { border-color: #0f0; background: #000; color: #fff; box-shadow: 0 0 8px rgba(0, 255, 0, 0.2); }
.c-icon { font-size: 1.2rem; width: 30px; text-align: center; }
.c-detail { flex: 1; display: flex; flex-direction: column; }
.c-label { font-size: 0.6rem; color: #666; font-family: var(--mono); }
.c-val { font-size: 0.8rem; font-weight: bold; }
.c-arrow { font-family: var(--mono); font-size: 0.8rem; color: #0f0; opacity: 0; transform: translateX(-5px); transition: 0.2s; }
.contact-item:hover .c-arrow { opacity: 1; transform: translateX(0); }

/* 外部链接按钮 */
.link-buttons { display: flex; flex-wrap: wrap; gap: 8px; }
.cyber-link-btn {
  background: transparent; border: 1px solid #555; color: #aaa;
  padding: 6px 12px; font-family: var(--mono); font-size: 0.75rem;
  text-decoration: none; transition: 0.2s;
}
.cyber-link-btn:hover { border-color: var(--red); color: var(--red); background: rgba(217, 35, 35, 0.1); }

/* 条形码区域 */
.barcode-area { margin-top: auto; padding-top: 15px; text-align: center; opacity: 0.5; }
.barcode { height: 25px; background: repeating-linear-gradient(90deg, #fff, #fff 2px, transparent 2px, transparent 4px); margin-bottom: 5px; }
.code-num { font-size: 0.6rem; letter-spacing: 2px; font-family: var(--mono); }

/* 滚动条样式 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #111; }
.custom-scroll::-webkit-scrollbar-thumb { background: #555; border-radius: 3px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--red); }
</style>