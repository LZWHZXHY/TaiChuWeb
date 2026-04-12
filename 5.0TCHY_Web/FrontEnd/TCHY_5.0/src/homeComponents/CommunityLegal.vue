<template>
  <div class="legal-terminal">
    <div class="header-block">
      <h3 class="title">LEGAL_TERMINAL // 法律信息终端</h3>
      <div class="meta">
        <span>VER: 2.1.0</span>
        <span class="divider">//</span>
        <span>LANG: ZH_CN</span>
      </div>
    </div>

    <div class="protocol-content custom-scroll">
      
      <div class="clause-block">
        <h4 class="clause-title">01. USER_AGREEMENT // 用户协议</h4>
        <div class="raw-text-display">
          {{ termsContent }}
        </div>
      </div>

      <div class="section-divider">
        ----------------------------------------
      </div>

      <div class="clause-block">
        <h4 class="clause-title">02. PRIVACY_POLICY // 隐私政策</h4>
        <div class="raw-text-display">
          {{ privacyContent }}
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
// 🚀 直接从指定路径引入原始文本字符串
import privacyContent from '@/locales/legal/privacy.zh.js'
import termsContent from '@/locales/legal/terms.zh.js'
</script>

<style scoped>
/* 保持原有样式，white-space: pre-wrap 会完美保留 .js 文件里的换行和空格 */
.legal-terminal {
  font-family: 'JetBrains Mono', monospace;
  color: #333;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.header-block {
  border-bottom: 2px solid #111;
  margin-bottom: 20px;
  padding-bottom: 15px;
  flex-shrink: 0;
}

.title {
  font-family: 'Anton', sans-serif;
  font-size: 2.5rem;
  margin: 0;
  letter-spacing: 1px;
}

.meta {
  color: #D92323;
  font-weight: bold;
  font-size: 0.8rem;
  margin-top: 5px;
}

.divider { margin: 0 10px; color: #ccc; }

.protocol-content {
  flex: 1;
  overflow-y: auto;
  padding-right: 10px;
}

.clause-block { margin-bottom: 40px; }

.clause-title {
  font-size: 1.2rem;
  background: #111;
  color: #fff;
  display: inline-block;
  padding: 5px 10px;
  margin-bottom: 20px;
}

.raw-text-display {
  white-space: pre-wrap; /* 核心：确保文本文件里的 \n 换行符生效 */
  line-height: 1.8;      
  color: #555;
  font-size: 0.95rem;
  background: rgba(0,0,0,0.02); 
  padding: 20px;
  border-left: 3px solid #ccc;  
}

.section-divider {
  color: #ccc;
  margin: 30px 0;
  user-select: none;
  font-weight: bold;
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #D92323; }
</style>