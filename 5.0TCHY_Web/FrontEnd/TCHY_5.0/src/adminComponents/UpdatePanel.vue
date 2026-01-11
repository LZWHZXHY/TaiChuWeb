<template>
  <div class="update-log-container">
    <h2>添加更新日志</h2>
    <form @submit.prevent="submitUpdate" class="update-form">
      <div class="form-group">
        <label for="version">版本号 *</label>
        <input
          v-model="formData.version"
          type="text"
          id="version"
          placeholder="例如：1.0.0"
          required
        />
      </div>

      <div class="form-group">
        <label for="title">更新标题 *</label>
        <input
          v-model="formData.title"
          type="text"
          id="title"
          placeholder="输入更新标题"
          maxlength="200"
          required
        />
      </div>

      <div class="form-group">
        <label for="type">更新类型 *</label>
        <select v-model="formData.type" id="type" required>
          <option value="1">Bug修复</option>
          <option value="2">小内容更新</option>
          <option value="3">网站优化</option>
          <option value="4">重大更新</option>
          <option value="5">其他</option>
        </select>
      </div>

      <div class="form-group">
        <label for="content">更新内容 *</label>
        <textarea
          v-model="formData.content"
          id="content"
          rows="6"
          placeholder="详细描述更新内容..."
          required
        ></textarea>
      </div>

      <button type="submit" :disabled="isSubmitting" class="submit-btn">
        {{ isSubmitting ? '提交中...' : '添加更新日志' }}
      </button>

      <div v-if="message" :class="['message', messageType]">
        {{ message }}
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import apiClient from '@/utils/api';

const isSubmitting = ref(false);
const message = ref('');
const messageType = ref('');

const formData = reactive({
  version: '',
  title: '',
  content: '',
  type: '1' // 默认是1: bug修复
});

const submitUpdate = async () => {
  if (isSubmitting.value) return;

  // 验证必填字段
  if (!formData.version.trim() || !formData.title.trim() || !formData.content.trim()) {
    message.value = '请填写所有必填字段';
    messageType.value = 'error';
    return;
  }

  isSubmitting.value = true;
  message.value = '';
  messageType.value = '';

  try {
    const response = await apiClient.post('/Update/create', {
      Version: formData.version,
      Title: formData.title,
      Content: formData.content,
      Type: parseInt(formData.type)
    });

    if (response.data) {
      message.value = '更新日志添加成功！';
      messageType.value = 'success';
      // 清空表单
      formData.version = '';
      formData.title = '';
      formData.content = '';
      formData.type = '1';
    }
  } catch (error) {
    console.error('添加更新日志失败:', error);
    message.value = error.response?.data?.message || '添加失败，请稍后重试';
    messageType.value = 'error';
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.update-log-container {
  max-width: 560px;
  margin: 48px auto 0;
  padding: 0 18px 36px 18px;
  border-radius: 18px;
  background:
    linear-gradient(#fff, #f6faff 93%, #f6fbff),
    linear-gradient(92deg, #2376ee 12%, #5bc5fa 87%);
  background-origin: border-box;
  background-clip: padding-box, border-box;
  box-shadow: 0 6px 32px rgba(33,111,237,.14);
  position: relative;
}
.update-log-container::before {
  content: "";
  position: absolute;
  inset: -2px 0 0 0;
  height: 100%;
  width: 100%;
  border-radius: 22px;
  border: 2px solid transparent;
  background: linear-gradient(92deg,#2376ee 0%,#5bc5fa 100%);
  z-index: -1;
  opacity: 0.18;
  pointer-events: none;
}
.update-log-container h2 {
  text-align: center;
  font-size: 2rem;
  font-weight: 700;
  color: #1e374a;
  margin-bottom: 28px;
  letter-spacing: 1.5px;
  position: relative;
}
.update-log-container h2::after {
  content: "";
  display: block;
  width: 66px;
  height: 3px;
  background: linear-gradient(90deg,#2376ee 0,#5bc5fa 90%);
  border-radius: 2px;
  margin: 13px auto 0 auto;
  opacity: 0.55;
}
.update-form {
  background: rgba(255,255,255,.97);
  padding: 28px 22px 22px 22px;
  border-radius: 10px;
  box-shadow: 0 2px 12px rgba(33, 111, 237, 0.10);
  border: none;
  transition: box-shadow 0.2s;
  backdrop-filter: blur(2px);
  position: relative;
  z-index: 1;
}
.update-form:hover {
  box-shadow: 0 6px 32px rgba(33,111,237,0.19);
}
.form-group {
  margin-bottom: 25px;
  position: relative;
  display: flex;
  flex-direction: column;
}
.form-group label {
  margin-bottom: 8px;
  font-weight: 550;
  color: #2762bb;
  font-size: 1.10rem;
  letter-spacing: 0.4px;
  position: relative;
}
.form-group label::before {
  content: "✦";
  color: #50afe7;
  margin-right: 7px;
  font-size: 1rem;
  vertical-align: middle;
  opacity: .62;
}
.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px 15px;
  border: 1.7px solid #e6eaf1;
  border-radius: 6px;
  font-size: 16px;
  background: linear-gradient(95deg,#f8faff 90%,#ecf8ff 100%);
  transition: border-color 0.3s, box-shadow 0.3s;
  box-sizing: border-box;
  color: #222;
  outline: none;
  font-family: inherit;
}
.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  border-color: #4db8fa;
  box-shadow: 0 0 0 2px rgba(81,199,242,0.16);
  background: #fff;
}
.form-group select {
  min-height: 39px;
  font-weight: 600;
  background: linear-gradient(90deg, #f8faff 70%, #e7f8fc);
}
.form-group textarea {
  min-height: 100px;
  resize: vertical;
}
.submit-btn {
  width: 100%;
  padding: 14px 0;
  background: linear-gradient(90deg, #45a4ed 0%, #5bc5fa 80%, #56e8ae 100%);
  color: #fff;
  border: none;
  border-radius: 6px;
  font-weight: 700;
  font-size: 1.13rem;
  cursor: pointer;
  letter-spacing: 1px;
  box-shadow: 0 2px 10px rgba(81,199,242,0.11);
  transition: background 0.3s, box-shadow 0.3s, transform 0.18s;
  outline: none;
  position: relative;
  z-index: 2;
}
.submit-btn:hover:not(:disabled) {
  background: linear-gradient(92deg, #2376ee 0%, #30e0a1 100%);
  box-shadow: 0 6px 26px rgba(33,110,237,.17);
  transform: translateY(-2px) scale(1.03);
}
.submit-btn:disabled {
  background: #dfe6ee;
  color: #aaa;
  cursor: not-allowed;
  box-shadow: none;
  transform: none;
}
.message {
  margin-top: 18px;
  padding: 12px 16px;
  border-radius: 6px;
  text-align: center;
  font-weight: 600;
  font-size: 1.06rem;
  transition: background 0.25s, border 0.23s, color 0.22s;
  box-shadow: 0 1px 5px rgba(80,174,231,0.06);
  letter-spacing: 0.7px;
  animation: fadeInMessage 0.27s;
}
@keyframes fadeInMessage {
  from { opacity: 0; transform: translateY(15px);}
  to { opacity: 1; transform: none;}
}
.message.success {
  background-color: #e7f9f2;
  color: #208b55;
  border: 1.4px solid #b9efc3;
}
.message.error {
  background-color: #fff0ef;
  color: #c0392b;
  border: 1.4px solid #fed6d3;
}
@media (max-width: 700px) {
  .update-log-container {
    max-width: 98vw;
    padding: 0 5vw 18px 5vw;
  }
  .update-form {
    padding: 16px 8px 12px 8px;
  }
  .form-group input,
  .form-group select,
  .form-group textarea {
    font-size: 15px;
    padding: 9px 8px;
  }
  .submit-btn {
    font-size: 1rem;
    padding: 12px 0;
  }
  .message {
    font-size: 0.96rem;
    padding: 8px 5px;
  }
  .update-log-container h2 {
    font-size: 1.26rem;
    margin-bottom: 12px;
  }
}
@media (hover: hover) {
  .form-group textarea::-webkit-scrollbar {
    height: 8px;
    width: 6px;
    background: #eef6fc;
    border-radius: 6px;
  }
  .form-group textarea::-webkit-scrollbar-thumb {
    background: #85c6fa;
    border-radius: 6px;
  }
  .form-group textarea::-webkit-scrollbar-thumb:hover {
    background: #56bff9;
  }
}
</style>