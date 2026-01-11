<template>
  <transition name="fade">
    <div v-if="chapter"
         class="chapter-modal-full"
         @click.self="handleClose"
    >
      <div class="chapter-modal-full-content">
        <div class="chapter-modal-header">
          <h2 class="chapter-title">{{ chapter.title }}</h2>
          <p v-if="showAuthor" class="chapter-modal-author">
            作者：{{ chapter.author || defaultAuthor }}
          </p>
          <!-- 字体大小控制区 -->
          <div class="font-controls">
            <label>阅读字号：</label>
            <button
              class="fo-btn"
              :disabled="fontSize<=18"
              @click="updateFont(fontSize-2)">
              A-
            </button>
            <span class="font-size-label">{{ fontSize }}px</span>
            <button
              class="fo-btn"
              :disabled="fontSize>=36"
              @click="updateFont(fontSize+2)">
              A+
            </button>
          </div>
        </div>
        <div class="chapter-content-box-full">
          <div
            class="chapter-content"
            :style="{fontSize: fontSize + 'px'}"
            v-html="displayContent(chapter.content)">
          </div>
        </div>
        <button class="btn-ghost close-btn" @click="handleClose">关闭</button>
      </div>
    </div>
  </transition>
</template>

<script>
export default {
  name: "ChapterReader",
  props: {
    chapter: {
      type: Object,
      default: null
    },
    showAuthor: {
      type: Boolean,
      default: false
    },
    defaultAuthor: {
      type: String,
      default: ""
    }
  },
  data() {
    return {
      fontSize: 20 // 默认字体大小
    }
  },
  methods: {
    displayContent(content) {
      if (!content) return "";
      return content.replace(/\n/g, "<br>");
    },
    handleClose() {
      this.$emit("close");
    },
    updateFont(size) {
      if (size < 18) size = 18;
      if (size > 36) size = 36;
      this.fontSize = size;
    }
  }
}
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity .23s;
}
.fade-enter, .fade-leave-to {
  opacity: 0;
}
.chapter-modal-full {
  position: fixed;
  z-index: 999;
  left: 0; right: 0; top: 0; bottom: 0;
  background: #e6e7dcde; /* 柔和淡黄色调，减缓视觉疲劳 */
  display: flex;
  align-items: flex-start;
  justify-content: center;
  overflow-y: auto;
  padding-top: 54px;
}
.chapter-modal-full-content {
  background: #f4f1e9; /* 暖白色纸感 */
  border-radius: 20px;
  box-shadow: 0 10px 70px rgba(32,40,60,0.14);
  width: 85vw;
  min-height: 480px;
  max-height: 92vh;
  margin-bottom: 40px;
  padding: 52px 54px 36px 54px;
  position: relative;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  top:50px;
  border: 1.5px solid #ede7d8;
}
.chapter-modal-header {
  margin-bottom: 18px;
  position: relative;
}
.chapter-title {
  font-size: 29px;
  font-weight: 700;
  margin-bottom: 10px;
  color: #3b4222;
  letter-spacing: 0.03em;
}
.chapter-modal-author {
  font-size: 16px;
  color: #5e6b42;
  margin-bottom: 7px;
}
.font-controls {
  margin-top: 13px;
  margin-bottom: 2px;
  display: flex;
  align-items: center;
  gap: 0.6em;
  user-select: none;
}
.fo-btn {
  padding:3px 11px;
  border-radius:5px;
  font-size:18px;
  background: #f8f4e8;
  color: #595d32;
  border: 1px solid #d6d3c0;
  cursor: pointer;
  transition: background .12s;
}
.fo-btn:disabled {
  color: #cac6bb;
  border-color: #f2f1eb;
  background: #f4f2ee;
  cursor: not-allowed;
}
.font-size-label {
  font-size:15px;
  color:#789544;
  font-family: "Consolas", "Menlo", "Monaco", "monospace";
  margin: 0 8px;
  min-width: 38px;
  display: inline-block;
  text-align: center;
}
.chapter-content-box-full {
  background: #f7f5ec;
  border-radius: 15px;
  padding: 26px;
  max-height: 56vh;
  font-size: 20px;
  overflow-y: auto;
  margin-bottom: 35px;
  line-height: 2.09;
  box-shadow: 0 3px 18px rgba(144,139,110, 0.09);
  border: 1px solid #f3eede;
}
.chapter-content {
  font-family: "Songti SC","Serif","Georgia","PingFang SC",Arial,sans-serif;
  white-space: pre-line;
  word-break: break-word;
  background: transparent;
  margin: 0;
  color: #3c442b;
  letter-spacing: 0.01em;
  transition: font-size .13s;
}
.close-btn {
  position: absolute;
  right: 38px;
  top: 24px;
  font-size: 19px;
  z-index: 2;
}
.btn-ghost {
  background: transparent;
  border: 1px solid #e8e3cf;
  padding: 10px 15px;
  border-radius: 10px;
  cursor: pointer;
  color: #6a7c52;
  font-weight: 600;
  font-size: 18px;
  transition:background .12s;
}
.btn-ghost:hover {
  background: #ede8d8;
  border-color:#d1c48c;
}
@media (max-width: 1100px) {
  .chapter-modal-full-content {
    min-width: 96vw;
    max-width: 99vw;
    padding: 22px 4vw 26px 4vw;
  }
  .chapter-content-box-full {
    font-size: 17px;
    padding: 14px;
    max-height: 44vh;
  }
  .chapter-title { font-size: 22px; }
}
</style>