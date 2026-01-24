<template>
  <div class="legal-terminal">
    <div class="header-block">
      <h3 class="title">{{ $t('legal.system_title') }}</h3>
      <div class="meta">
        <span>VER: 2.1.0</span>
        <span class="divider">//</span>
        <span>LANG: {{ $i18n.locale.toUpperCase() }}</span>
      </div>
    </div>

    <div class="protocol-content custom-scroll">
      
      <div class="clause-block">
        <h4 class="clause-title">01. {{ $t('legal.terms_title') }}</h4>
        
        <div class="raw-text-display">
          {{ $t('legal.terms_content') }}
        </div>
      </div>

      <div class="section-divider">
        ----------------------------------------
      </div>

      <div class="clause-block">
        <h4 class="clause-title">02. {{ $t('legal.privacy_title') }}</h4>
        
        <div class="raw-text-display">
          {{ $t('legal.privacy_content') }}
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* 基础样式复用 */
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
  overflow-y: auto; /* 让协议内容可以滚动 */
  padding-right: 10px; /* 防止滚动条遮挡文字 */
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

/* --- 核心修改：处理长文本字符串 --- */
.raw-text-display {
  white-space: pre-wrap; /* 关键：保留换行符和空格 */
  line-height: 1.8;      /* 增加行高，提升可读性 */
  color: #555;
  font-size: 0.95rem;
  background: rgba(0,0,0,0.02); /* 给个淡淡的背景，像打印纸 */
  padding: 20px;
  border-left: 3px solid #ccc;  /* 左侧装饰线 */
}

.section-divider {
  color: #ccc;
  margin: 30px 0;
  user-select: none;
  font-weight: bold;
}

/* 自定义滚动条样式 (配合你的主题) */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #D92323; }
</style>