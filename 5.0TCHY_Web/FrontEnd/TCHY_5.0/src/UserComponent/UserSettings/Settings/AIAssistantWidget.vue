<template>
  <input 
    type="file" 
    accept="image/*" 
    ref="fileInputRef" 
    @change="handleFileUpload" 
    style="display: none;" 
  />

  <div 
    v-show="selectionMenu.show" 
    class="ai-selection-tooltip"
    :style="{ top: selectionMenu.y + 'px', left: selectionMenu.x + 'px' }"
    @mousedown.stop
  >
    <button class="tooltip-btn" @click="handleQuickAction('提问')">
      <span class="icon">?</span> 提问
    </button>
    <div class="tooltip-divider"></div>
    <button class="tooltip-btn" @click="handleQuickAction('翻译')">
      <span class="icon">⚏</span> 翻译
    </button>
    <div class="tooltip-divider"></div>
    <button class="tooltip-btn" @click="handleQuickAction('续写')">
      <span class="icon">✎</span> 续写
    </button>
  </div>

  <div class="ai-floating-widget" :class="{ 'is-active': isChatExpanded || showSettings }">
    
    <div class="settings-popover" :class="{ 'is-open': showSettings }">
      <div class="popover-header">
        <h4>模型与 API 配置</h4>
        <button class="close-btn" @click="toggleSettings">×</button>
      </div>
      
      <div class="popover-content scroll-container">
        <div class="config-section">
          <div class="section-header">
            <h5>💬 对话模型 (支持多模态)</h5>
            <button class="add-btn" @click="addChatModel">+ 添加</button>
          </div>
          <div class="model-list">
            <div 
              v-for="(model, index) in chatModels" 
              :key="model.id" 
              class="model-card"
              :class="{ 'is-active': activeChatModelId === model.id }"
            >
              <div class="model-card-header">
                <label class="radio-label">
                  <input type="radio" :value="model.id" v-model="activeChatModelId" />
                  <span class="radio-custom"></span>
                  配置 {{ index + 1 }}
                </label>
                <button 
                  v-if="chatModels.length > 1" 
                  class="delete-btn" 
                  @click="removeChatModel(model.id)"
                  title="删除此配置"
                >×</button>
              </div>
              
              <input v-model="model.apiUrl" class="standard-input mini" placeholder="API URL (需带 /v1/chat/completions)" />
              <input type="password" v-model="model.apiKey" class="standard-input mini" placeholder="API Key (sk-...)" />
              <input v-model="model.name" class="standard-input mini" placeholder="输入模型名称 (如: gpt-4o)" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="chat-panel" :class="{ 'is-expanded': isChatExpanded }">
      
      <div class="floating-notice">
        内容由用户自配置AI模型生成
      </div>

      <div class="disclaimer-watermark">
        <div class="watermark-content">
          <p>本对话框内所有内容由用户自配置模型生成，与本平台无任何关联。</p>
          <p>内容涉及的敏感信息、立场观点均由用户自配置模型决定，平台不参与、不干预、不承担任何相关责任。</p>
          <p>用户需自行把控内容的合规性、敏感性及立场导向，自行承担因内容引发的全部风险与责任，与本平台无关。</p>
        </div>
      </div>

      <div class="chat-header" @click="toggleChat">
        <div class="header-left">
          <span class="deco-icon">⌬</span>
          <h3>AI 助手 ({{ currentActiveModelName }})</h3>
        </div>
        <button class="close-btn">▾ 收起</button>
      </div>
      
      <div class="chat-body scroll-container" ref="chatBodyRef">
        <div v-if="chatHistory.length === 0" class="empty-state">
          <span class="empty-icon">✧</span>
          <p>随时准备协助您的创作与阅读。</p>
        </div>
        
        <div 
          v-for="(msg, index) in chatHistory" 
          :key="index"
          class="chat-bubble-wrapper"
          :class="msg.role === 'user' ? 'is-user' : 'is-ai'"
        >
          <div class="chat-bubble">
            <img v-if="msg.image" :src="msg.image" class="bubble-image" alt="User uploaded image" />
            <div v-html="formatMessage(msg.content)"></div>
          </div>
        </div>

        <div v-if="isTyping" class="chat-bubble-wrapper is-ai">
          <div class="chat-bubble typing-indicator">
            <span></span><span></span><span></span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="attachedImage" class="attachment-preview-wrapper">
      <div class="attachment-preview">
        <img :src="attachedImage" alt="attachment" />
        <button class="remove-img-btn" @click="removeAttachedImage" title="移除图片">×</button>
      </div>
    </div>

    <div class="control-bar" @dragover.prevent @dragenter.prevent @drop.prevent="handleDrop">
      <button 
        class="icon-btn" 
        :class="{ 'is-active': showSettings }"
        @click="toggleSettings" 
        title="配置模型与 API"
      >
        ⚙
      </button>

      <button 
        class="icon-btn" 
        :class="{ 'is-active': isChatExpanded }"
        @click="toggleChat" 
        title="展开对话"
      >
        💬
      </button>
      
      <div class="input-wrapper">
        <input 
          v-model="inputText" 
          type="text" 
          class="chat-input"
          placeholder="输入问题，或粘贴/拖拽图片..."
          @keyup.enter="sendMessage()"
          @focus="handleInputFocus"
          @paste="handlePaste"
        />
        <button 
          class="send-btn" 
          :class="{ 'has-text': inputText.trim().length > 0 || attachedImage }"
          @click="sendMessage()"
          :disabled="isTyping"
          title="发送"
        >
          ↑
        </button>
      </div>

      <button 
        class="upload-btn" 
        @click="triggerUpload"
        :disabled="isTyping"
        title="上传图片进行视觉分析"
      >
        📷
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted, nextTick } from 'vue';

// ======= 状态控制 =======
const isChatExpanded = ref(false);
const showSettings = ref(false);
const inputText = ref('');
const isTyping = ref(false);

const chatBodyRef = ref(null);
const chatHistory = ref([]);

// ======= 图片上传处理 (点击/拖拽/粘贴) =======
const fileInputRef = ref(null);
const attachedImage = ref(null);

const triggerUpload = () => {
  if (isTyping.value) return;
  fileInputRef.value.click();
};

const processImageFile = (file) => {
  if (!file || !file.type.startsWith('image/')) return;
  
  const reader = new FileReader();
  reader.onload = (e) => {
    attachedImage.value = e.target.result; // 获取 Base64
    isChatExpanded.value = true; // 附加图片时自动展开聊天框以便预览
    showSettings.value = false;
  };
  reader.readAsDataURL(file);
};

const handleFileUpload = (e) => {
  const file = e.target.files[0];
  processImageFile(file);
  e.target.value = ''; // 重置 input，允许重复上传同名文件
};

const handleDrop = (e) => {
  const file = e.dataTransfer?.files[0];
  processImageFile(file);
};

const handlePaste = (e) => {
  const items = e.clipboardData?.items;
  if (!items) return;
  for (let item of items) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile();
      processImageFile(file);
      break;
    }
  }
};

const removeAttachedImage = () => {
  attachedImage.value = null;
};

// ======= 模型与 API 配置数据 =======
let modelIdCounter = 3;
const chatModels = ref([
  { id: 1, name: 'gpt-4o', apiUrl: 'https://api.openai.com/v1/chat/completions', apiKey: '' },
  { id: 2, name: 'deepseek-chat', apiUrl: 'https://api.deepseek.com/chat/completions', apiKey: '' },
  { id: 3, name: 'moonshot-v1-8k', apiUrl: 'https://api.moonshot.cn/v1/chat/completions', apiKey: '' }
]);
const activeChatModelId = ref(1);

const currentActiveModelName = computed(() => {
  const model = chatModels.value.find(m => m.id === activeChatModelId.value);
  return model ? (model.name || '未配置模型') : '未知配置';
});

const addChatModel = () => {
  modelIdCounter++;
  chatModels.value.push({ id: modelIdCounter, name: '', apiUrl: 'https://api.openai.com/v1/chat/completions', apiKey: '' });
};

const removeChatModel = (id) => {
  chatModels.value = chatModels.value.filter(m => m.id !== id);
  if (activeChatModelId.value === id && chatModels.value.length > 0) {
    activeChatModelId.value = chatModels.value[0].id;
  }
};

// ======= UI 展开逻辑 =======
const toggleChat = () => {
  isChatExpanded.value = !isChatExpanded.value;
  if (isChatExpanded.value) showSettings.value = false;
};

const toggleSettings = () => {
  showSettings.value = !showSettings.value;
  if (showSettings.value) isChatExpanded.value = false;
};

const handleInputFocus = () => {
  isChatExpanded.value = true;
  showSettings.value = false;
};

const scrollToBottom = async () => {
  await nextTick();
  if (chatBodyRef.value) chatBodyRef.value.scrollTop = chatBodyRef.value.scrollHeight;
};

// ======= 真实对话 API 请求 =======
const sendMessage = async (customText = '') => {
  const text = customText || inputText.value.trim();
  const hasImage = !!attachedImage.value;
  
  if (!text && !hasImage) return;
  if (isTyping.value) return;

  const currentImgBase64 = attachedImage.value;

  showSettings.value = false;
  isChatExpanded.value = true;
  inputText.value = '';
  attachedImage.value = null; 

  chatHistory.value.push({ 
    role: 'user', 
    content: text || '请分析这张图片', 
    image: currentImgBase64 
  });
  scrollToBottom();
  isTyping.value = true;

  const activeConfig = chatModels.value.find(m => m.id === activeChatModelId.value);

  if (!activeConfig || !activeConfig.apiUrl || !activeConfig.apiKey || !activeConfig.name) {
    chatHistory.value.push({ 
      role: 'ai', 
      content: `⚠️ 请先在 ⚙ 设置面板中完善 <b>对话模型</b> 的 API URL、API Key 和 模型名称。` 
    });
    isTyping.value = false;
    scrollToBottom();
    return;
  }

  const messages = chatHistory.value.filter(m => m.role !== 'system').map(m => {
    if (m.image) {
      return {
        role: m.role === 'ai' ? 'assistant' : 'user',
        content: [
          { type: "text", text: m.content || "请分析这张图片" },
          { type: "image_url", image_url: { url: m.image } }
        ]
      };
    } else {
      return {
        role: m.role === 'ai' ? 'assistant' : 'user',
        content: m.content
      };
    }
  });

  try {
    const response = await fetch(activeConfig.apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${activeConfig.apiKey}`
      },
      body: JSON.stringify({
        model: activeConfig.name,
        messages: messages
      })
    });

    if (!response.ok) {
      const errData = await response.json().catch(() => ({}));
      if (response.status === 404) {
        throw new Error("HTTP 404 - 找不到接口。请确保 API URL 包含后缀 /v1/chat/completions");
      }
      throw new Error(errData.error?.message || `HTTP 错误代码 ${response.status}`);
    }

    const data = await response.json();
    const replyText = data.choices?.[0]?.message?.content || "API 返回数据异常。";
    
    chatHistory.value.push({ role: 'ai', content: replyText });
  } catch (err) {
    chatHistory.value.push({ role: 'ai', content: `❌ 请求失败：${err.message}` });
  } finally {
    isTyping.value = false;
    scrollToBottom();
  }
};

const formatMessage = (text) => {
  if (!text) return '';
  return text.replace(/\n/g, '<br>');
};

// ======= 全局文本划选逻辑 =======
const selectionMenu = reactive({ show: false, x: 0, y: 0, text: '' });

const handleMouseUp = (e) => {
  if (e.target.closest('.ai-floating-widget') || e.target.closest('.ai-selection-tooltip')) return;

  setTimeout(() => {
    const selection = window.getSelection();
    const text = selection.toString().trim();
    if (text) {
      selectionMenu.text = text;
      selectionMenu.show = true;
      selectionMenu.x = e.clientX + 10;
      selectionMenu.y = e.clientY + 15;
    } else {
      selectionMenu.show = false;
    }
  }, 10);
};

const handleMouseDown = (e) => {
  if (!e.target.closest('.ai-selection-tooltip')) selectionMenu.show = false;
};

const handleQuickAction = (action) => {
  const text = selectionMenu.text;
  if (!text) return;
  selectionMenu.show = false;
  window.getSelection().removeAllRanges();

  if (action === '提问') sendMessage(`关于这段文本我有个问题：\n"${text}"\n`);
  else if (action === '翻译') sendMessage(`【翻译】请翻译以下文本：\n"${text}"`);
  else if (action === '续写') sendMessage(`【续写】请根据以下文本继续写：\n"${text}"`);
};

onMounted(() => {
  document.addEventListener('mouseup', handleMouseUp);
  document.addEventListener('mousedown', handleMouseDown);
});

onUnmounted(() => {
  document.removeEventListener('mouseup', handleMouseUp);
  document.removeEventListener('mousedown', handleMouseDown);
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;500;700;900&display=swap');

/* ======= 1. 划选快捷菜单 (深色) ======= */
.ai-selection-tooltip {
  position: fixed; z-index: 10000;
  background: #1e1e1e; border: 1px solid #333; border-radius: 12px;
  display: flex; align-items: center; padding: 4px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.5);
  transform-origin: top left;
  animation: tooltipFadeIn 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
@keyframes tooltipFadeIn {
  from { opacity: 0; transform: scale(0.9) translateY(-10px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}
.tooltip-btn {
  background: transparent; color: #eee; border: none;
  padding: 8px 12px; font-size: 13px; font-weight: 500;
  font-family: 'Noto Sans SC', sans-serif; cursor: pointer;
  border-radius: 8px; display: flex; align-items: center; gap: 6px;
  transition: all 0.2s; white-space: nowrap;
}
.tooltip-btn:hover { background: rgba(255, 255, 255, 0.1); color: #fff; }
.tooltip-btn .icon { font-size: 14px; color: #d35400; }
.tooltip-divider { width: 1px; height: 16px; background: rgba(255, 255, 255, 0.15); margin: 0 4px; }

/* ======= 2. 悬浮主件 ======= */
.ai-floating-widget {
  position: fixed; bottom: 40px; right: 40px; z-index: 9999;
  width: 440px; 
  font-family: 'Noto Sans SC', sans-serif;
  display: flex; flex-direction: column; align-items: flex-end; gap: 16px;
  pointer-events: none;
}
.ai-floating-widget > * { pointer-events: auto; }

/* ======= 3. 设置面板 (深色、可滚动) ======= */
.settings-popover {
  width: 360px; max-height: 500px;
  display: flex; flex-direction: column;
  background: #1a1a1a; border-radius: 16px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.6);
  border: 1px solid #333;
  transform: translateY(20px); opacity: 0; pointer-events: none;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: absolute; bottom: 80px; right: 0;
}
.settings-popover.is-open { transform: translateY(0); opacity: 1; pointer-events: auto; }

.popover-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 20px; border-bottom: 1px solid #2a2a2a; flex-shrink: 0;
}
.popover-header h4 { margin: 0; font-size: 15px; font-weight: 700; color: #eee; }

.popover-content {
  padding: 20px; overflow-y: auto; scrollbar-gutter: stable;
  display: flex; flex-direction: column; gap: 24px;
}
.section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.section-header h5 { margin: 0; font-size: 13px; color: #aaa; font-weight: 700; }
.add-btn { background: transparent; border: none; color: #1976d2; font-size: 12px; font-weight: 700; cursor: pointer; }
.add-btn:hover { color: #42a5f5; text-decoration: underline; }

.model-list { display: flex; flex-direction: column; gap: 12px; }
.model-card {
  background: #121212; border: 1px solid #333; border-radius: 12px;
  padding: 12px; display: flex; flex-direction: column; gap: 8px; transition: all 0.2s;
}
.model-card.is-active { border-color: #555; background: #1e1e1e; box-shadow: 0 2px 8px rgba(0,0,0,0.2); }
.model-card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px; }
.radio-label { display: flex; align-items: center; gap: 8px; font-size: 12px; color: #eee; cursor: pointer; font-weight: 700; }
.radio-label input[type="radio"] { display: none; }
.radio-custom {
  width: 14px; height: 14px; border-radius: 50%; border: 2px solid #555;
  display: inline-block; position: relative; transition: all 0.2s;
}
.radio-label input[type="radio"]:checked + .radio-custom { border-color: #d35400; }
.radio-label input[type="radio"]:checked + .radio-custom::after {
  content: ''; width: 6px; height: 6px; background: #d35400;
  border-radius: 50%; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
}
.delete-btn { background: transparent; border: none; color: #666; font-size: 16px; cursor: pointer; }
.delete-btn:hover { color: #d32f2f; }

.standard-input.mini {
  padding: 8px 10px; font-size: 12px; border-radius: 6px;
  background: #121212; border: 1px solid #333; color: #eee; outline: none;
}
.standard-input.mini:focus { border-color: #555; background: #1a1a1a; }

/* ======= 4. 聊天面板 ======= */
.chat-panel {
  width: 100%; height: 0; background: #141414; border-radius: 24px;
  border: 1px solid transparent; display: flex; flex-direction: column;
  overflow: hidden; opacity: 0; transform: translateY(20px);
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.6); position: relative; 
}
.chat-panel.is-expanded { height: 480px; opacity: 1; transform: translateY(0); border-color: #2a2a2a; }

/* 最上层悬浮提醒 (穿透点击) */
.floating-notice {
  position: absolute;
  top: 64px; 
  left: 50%;
  transform: translateX(-50%);
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.05);
  color: #D35400;
  font-size: 11px;
  padding: 4px 12px;
  border-radius: 20px;
  pointer-events: none; 
  z-index: 100;         
  white-space: nowrap;
  backdrop-filter: blur(4px); 
}

/* 底层免责声明水印 */
.disclaimer-watermark {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  display: flex; align-items: center; justify-content: center;
  pointer-events: none; z-index: 0; padding: 0 30px;
}
.watermark-content { padding-top: 40px; }
.disclaimer-watermark p {
  color: rgba(255, 255, 255, 0.06); font-size: 13px; font-weight: 700;
  text-align: center; line-height: 1.6; letter-spacing: 1px; margin: 0 0 8px 0;
}

.chat-header {
  position: relative; z-index: 2; display: flex; justify-content: space-between;
  align-items: center; padding: 16px 20px; background: #1a1a1a; border-bottom: 1px solid #2a2a2a; cursor: pointer;
}
.header-left { display: flex; align-items: center; gap: 8px; }
.deco-icon { color: #d35400; font-size: 16px; font-weight: bold; }
.chat-header h3 { margin: 0; font-size: 14px; font-weight: 700; color: #eee; }
.close-btn { background: transparent; border: none; font-size: 12px; color: #888; font-weight: 700; cursor: pointer; transition: color 0.2s; }
.close-btn:hover { color: #fff; }

.chat-body {
  position: relative; z-index: 1; flex: 1; padding: 20px;
  display: flex; flex-direction: column; gap: 16px; overflow-y: auto; scrollbar-gutter: stable;
}
.scroll-container::-webkit-scrollbar { width: 4px; background: transparent; }
.scroll-container::-webkit-scrollbar-thumb { background: #444; border-radius: 2px; }

.empty-state { margin: auto; display: flex; flex-direction: column; align-items: center; gap: 10px; color: #666; }
.empty-icon { font-size: 24px; color: #444; }
.empty-state p { margin: 0; font-size: 13px; }

.chat-bubble-wrapper { display: flex; width: 100%; }
.chat-bubble-wrapper.is-user { justify-content: flex-end; }
.chat-bubble-wrapper.is-ai { justify-content: flex-start; }
.chat-bubble { max-width: 85%; padding: 12px 16px; font-size: 14px; line-height: 1.5; word-wrap: break-word; }
.is-user .chat-bubble { background: #eeeeee; color: #000; border-radius: 20px 20px 4px 20px; font-weight: 500; }
.is-ai .chat-bubble { background: #222; color: #eee; border: 1px solid #333; border-radius: 20px 20px 20px 4px; }

.bubble-image {
  max-width: 100%; border-radius: 8px; margin-bottom: 8px; display: block;
}

.typing-indicator { display: flex; align-items: center; gap: 4px; height: 20px; padding: 12px 16px !important; }
.typing-indicator span { width: 6px; height: 6px; background-color: #888; border-radius: 50%; animation: bounce 1.4s infinite ease-in-out both; }
.typing-indicator span:nth-child(1) { animation-delay: -0.32s; }
.typing-indicator span:nth-child(2) { animation-delay: -0.16s; }
@keyframes bounce { 0%, 80%, 100% { transform: scale(0); } 40% { transform: scale(1); } }

/* ======= 图片悬浮预览 ======= */
.attachment-preview-wrapper {
  width: 100%; display: flex; justify-content: flex-start;
  padding-left: 110px; margin-bottom: -4px;
}
.attachment-preview {
  position: relative; width: 60px; height: 60px; background: #1a1a1a;
  border: 1px solid #333; border-radius: 8px; padding: 4px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.5); animation: popIn 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
@keyframes popIn {
  from { opacity: 0; transform: scale(0.8) translateY(10px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}
.attachment-preview img { width: 100%; height: 100%; object-fit: cover; border-radius: 4px; }
.remove-img-btn {
  position: absolute; top: -6px; right: -6px; background: #d32f2f; color: #fff; border: none;
  width: 18px; height: 18px; border-radius: 50%; font-size: 12px; display: flex; align-items: center; justify-content: center;
  cursor: pointer; box-shadow: 0 2px 4px rgba(0,0,0,0.3);
}
.remove-img-btn:hover { background: #f44336; }

/* ======= 5. 底部控制栏 ======= */
.control-bar { width: 100%; display: flex; align-items: center; gap: 12px; }
.icon-btn {
  width: 44px; height: 44px; border-radius: 50%; background: #1a1a1a;
  border: 1px solid #333; font-size: 16px; color: #eee; display: flex; justify-content: center;
  align-items: center; cursor: pointer; transition: all 0.2s; box-shadow: 0 4px 12px rgba(0,0,0,0.3); flex-shrink: 0;
}
.icon-btn:hover { background: #2a2a2a; border-color: #555; transform: translateY(-2px); }
.icon-btn.is-active { background: #2c2c2c; border-color: #d35400; color: #fff; }

.input-wrapper { flex: 1; position: relative; display: flex; align-items: center; }
.chat-input {
  width: 100%; height: 48px; padding: 0 48px 0 20px; background: #1a1a1a;
  border: 1px solid #333; border-radius: 24px; font-size: 14px; font-family: inherit;
  color: #eee; transition: all 0.3s; box-shadow: 0 4px 16px rgba(0,0,0,0.3); box-sizing: border-box;
}
.chat-input:focus { outline: none; background: #1e1e1e; border-color: #555; box-shadow: 0 6px 20px rgba(0,0,0,0.5); }
.chat-input::placeholder { color: #777; }

.send-btn {
  position: absolute; right: 6px; width: 36px; height: 36px; border-radius: 50%;
  background: #333; color: #666; border: none; font-size: 16px; font-weight: 700;
  display: flex; justify-content: center; align-items: center; cursor: not-allowed; transition: all 0.3s;
}
.send-btn.has-text { background: #eee; color: #000; cursor: pointer; }
.send-btn.has-text:hover { background: #fff; transform: translateY(-2px); box-shadow: 0 2px 8px rgba(255,255,255,0.2); }
.send-btn:disabled { background: #333 !important; color: #666 !important; cursor: not-allowed !important; transform: none !important; }

/* 独立上传图片按钮 */
.upload-btn {
  width: 48px; height: 48px; border-radius: 24px; background: #1a1a1a;
  border: 1px solid #333; color: #eee; font-size: 18px; display: flex;
  justify-content: center; align-items: center; cursor: pointer; transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(0,0,0,0.3); flex-shrink: 0;
}
.upload-btn:hover:not(:disabled) { background: #2a2a2a; border-color: #555; transform: translateY(-2px); color: #fff; }
.upload-btn:disabled { opacity: 0.5; cursor: not-allowed; }

@media (max-width: 768px) {
  .ai-floating-widget { bottom: 20px; right: 20px; left: 20px; width: auto; }
}
</style>