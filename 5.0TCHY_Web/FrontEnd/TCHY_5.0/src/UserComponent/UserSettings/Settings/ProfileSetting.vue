<template>
  <div class="profile-setting-wrapper">
    
    <div class="left-panel">
      <div class="panel-card">
        <div class="panel-header">
          <span class="deco-icon">⌬</span>
          <h3>简介 // BIO_DATA</h3>
        </div>
        
        <div class="bio-wrapper">
          <div class="textarea-container">
            <textarea 
              v-model="formData.bio" 
              placeholder="请输入个人简介..."
              maxlength="200"
            ></textarea>
            <div class="char-counter">
              <span>{{ formData.bio.length }}</span> / 200
            </div>
            <div class="corner-deco br"></div>
          </div>
        </div>

        <div class="links-wrapper">
          <div class="panel-header small">
            <h3>链接 // LINKS</h3>
            <button class="add-btn-mini" @click="addLink" title="添加新链接">+</button>
          </div>
          <div class="links-scroll">
            <transition-group name="list">
              <div v-for="(link, index) in formData.links" :key="index" class="link-row">
                <div class="link-body">
                  <div class="input-cell title-cell">
                    <span class="cell-label">标题</span>
                    <input 
                      v-model="link.title" 
                      class="link-input" 
                      placeholder="如: 博客" 
                    />
                  </div>
                  <div class="input-cell url-cell">
                    <span class="cell-label">URL</span>
                    <input 
                      v-model="link.url" 
                      class="link-input font-mono" 
                      placeholder="https://" 
                    />
                  </div>
                </div>
                <button class="del-btn" @click="removeLink(index)">×</button>
              </div>
            </transition-group>
            
            <div v-if="formData.links.length === 0" class="empty-hint">
              未检测到链接 // NO_LINKS
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="right-panel">
      <div class="form-header">
        <h2>基本信息 // BASIC_INFO</h2>
        <div class="line"></div>
      </div>

      <div class="form-grid">
        <div class="form-group full">
          <label>
            昵称 // NICKNAME 
            <span class="tag-badge">改名卡: 1</span>
          </label>
          <input v-model="formData.name" type="text" class="standard-input" placeholder="请输入你的昵称" />
        </div>

        <div class="form-row">
          <div class="form-group half">
            <label>性别 // GENDER</label>
            <div class="select-box">
              <select v-model="formData.gender">
                <option value="secret">保密 // SECRET</option>
                <option value="male">男 // MALE</option>
                <option value="female">女 // FEMALE</option>
                <option value="other">其他 // OTHER</option>
              </select>
            </div>
          </div>
          <div class="form-group half">
            <label>生日 // BIRTHDAY</label>
            <input v-model="formData.birthday" type="date" class="standard-input" />
          </div>
        </div>

        <div class="form-group full">
          <label>地区 // LOCATION</label>
          <input v-model="formData.location" type="text" class="standard-input" placeholder="例如：夜之城 · 第七区" />
        </div>

        <div class="form-group full">
          <label>联系方式 // CONTACT</label>
          <input v-model="formData.contact" type="text" class="standard-input" placeholder="email@example.com" />
        </div>
      </div>

      <div class="footer-action">
        <button class="save-button" @click="handleSave">
          <span class="btn-text">保存修改 // SAVE</span>
          <span class="btn-deco">➜</span>
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { reactive } from 'vue';

const formData = reactive({
  name: 'K_Runner',
  gender: 'male',
  location: '夜之城 · 第七区',
  birthday: '2077-12-10',
  contact: 'k_dev@net.connect',
  bio: '义体维修专家，专注于老式神经网络接口的调试。寻找电子幽灵的踪迹。',
  links: [
    { title: 'Blog', url: 'https://k-runner.dev' },
    { title: 'Github', url: 'https://github.com' }
  ]
});

const addLink = () => { formData.links.push({ title: '', url: '' }); };
const removeLink = (index) => { formData.links.splice(index, 1); };
const handleSave = () => {
  // 模拟保存动画
  alert('数据已同步 // DATA_SYNCED');
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.profile-setting-wrapper {
  width: 100%; height: 100%;
  display: flex;
  gap: 30px;
  padding: 30px;
  box-sizing: border-box;
  font-family: 'Noto Sans SC', sans-serif;
}

/* === 左侧面板：米色卡片风格 === */
.left-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.panel-card {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  height: 100%;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.panel-header {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #333;
}
.panel-header h3 { margin: 0; font-size: 14px; font-family: 'Noto Sans SC'; font-weight: 700; letter-spacing: 0.5px; }
.deco-icon { font-size: 16px; color: #d35400; }
.panel-header.small { justify-content: space-between; border-bottom: 1px dashed rgba(0,0,0,0.1); padding-bottom: 8px; }

/* Bio 输入区 */
.bio-wrapper { flex: 1; display: flex; flex-direction: column; }
.textarea-container {
  flex: 1; position: relative;
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: inset 0 2px 6px rgba(0,0,0,0.02);
  transition: box-shadow 0.2s;
}
.textarea-container:focus-within { box-shadow: inset 0 2px 6px rgba(0,0,0,0.05), 0 0 0 2px #000; }

textarea {
  width: 100%; height: 90%;
  border: none; background: transparent; resize: none; outline: none;
  font-size: 14px; line-height: 1.6; color: #333;
  font-family: 'Noto Sans SC';
}
.char-counter {
  position: absolute; bottom: 12px; right: 16px;
  font-size: 11px; font-family: 'Roboto Mono'; color: #999;
}
.corner-deco {
  position: absolute; width: 10px; height: 10px;
  border-right: 2px solid #d35400; border-bottom: 2px solid #d35400;
  bottom: 6px; right: 6px; opacity: 0.5;
}

/* Links 区域 - 优化版 */
.links-wrapper { flex: 1.2; display: flex; flex-direction: column; overflow: hidden; }
.add-btn-mini {
  background: #000; color: #fff; border: none; width: 24px; height: 24px;
  border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center;
  font-size: 16px; transition: transform 0.2s;
}
.add-btn-mini:hover { transform: scale(1.1); }

.links-scroll { flex: 1; overflow-y: auto; padding-top: 10px; padding-right: 4px; }
.links-scroll::-webkit-scrollbar { width: 3px; }
.links-scroll::-webkit-scrollbar-thumb { background: #ccc; }

.link-row {
  display: flex; align-items: center; gap: 8px; margin-bottom: 12px;
  background: #fff; padding: 12px; border-radius: 12px;
  border: 1px solid rgba(0,0,0,0.05);
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
  transition: all 0.2s;
}
.link-row:hover { border-color: rgba(0,0,0,0.15); box-shadow: 0 4px 12px rgba(0,0,0,0.05); }

.link-body { flex: 1; display: flex; flex-direction: column; gap: 8px; }

/* 输入框单元 */
.input-cell {
  display: flex; align-items: center;
  background: #F7F7F7;
  border-radius: 8px;
  padding: 4px 10px;
  border: 1px solid transparent;
}
.input-cell:focus-within { background: #fff; border-color: #000; }

.cell-label {
  font-size: 10px; color: #999; margin-right: 8px; font-weight: bold; flex-shrink: 0; width: 28px;
}

.link-input {
  border: none; outline: none; background: transparent; width: 100%;
  font-size: 13px; color: #333; padding: 4px 0;
  font-family: 'Noto Sans SC';
}
.font-mono { font-family: 'Roboto Mono', monospace; font-size: 12px; }

.del-btn { 
  border: none; background: transparent; color: #ccc; 
  cursor: pointer; font-size: 18px; width: 24px; height: 24px; 
  display: flex; align-items: center; justify-content: center;
}
.del-btn:hover { color: #ff4d4f; }

.empty-hint {
  text-align: center; color: #ccc; font-size: 12px; padding: 20px;
  font-family: 'Noto Sans SC';
}

/* === 右侧面板：表单 === */
.right-panel {
  flex: 1.5;
  padding: 10px 20px;
  display: flex;
  flex-direction: column;
}

.form-header { margin-bottom: 30px; }
.form-header h2 { font-size: 24px; font-weight: 900; margin: 0; letter-spacing: -1px; font-family: 'Noto Sans SC'; }
.form-header .line { width: 40px; height: 4px; background: #000; margin-top: 8px; }

.form-grid { flex: 1; display: flex; flex-direction: column; gap: 24px; }
.form-row { display: flex; gap: 20px; }
.form-group.full { width: 100%; }
.form-group.half { flex: 1; }

label {
  display: flex; justify-content: space-between; align-items: center;
  font-family: 'Noto Sans SC'; font-size: 12px; font-weight: 700; color: #666; margin-bottom: 8px;
}
.tag-badge { background: #000; color: #fff; padding: 2px 6px; border-radius: 4px; font-size: 10px; font-family: 'Noto Sans SC'; }

.standard-input, select {
  width: 100%;
  padding: 14px 16px;
  background: #FFFDF8; /* 淡米色背景 */
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  font-size: 14px;
  font-family: 'Noto Sans SC';
  color: #333;
  transition: all 0.2s;
  box-sizing: border-box;
}
.standard-input:focus, select:focus {
  border-color: #000;
  background: #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  outline: none;
}
.select-box { position: relative; }
.select-box::after {
  content: '▼'; font-size: 10px; position: absolute; right: 16px; top: 18px; color: #999; pointer-events: none;
}
select { appearance: none; cursor: pointer; }

/* 底部按钮 */
.footer-action { margin-top: 30px; display: flex; justify-content: flex-end; }
.save-button {
  background: #000; color: #fff;
  padding: 14px 40px; border-radius: 30px; border: none;
  font-family: 'Noto Sans SC'; font-weight: 700; font-size: 14px;
  cursor: pointer; display: flex; align-items: center; gap: 10px;
  transition: transform 0.2s, box-shadow 0.2s;
}
.save-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0,0,0,0.2);
}
.btn-deco { font-size: 16px; color: #d35400; }

/* 列表动画 */
.list-enter-active, .list-leave-active { transition: all 0.3s ease; }
.list-enter-from, .list-leave-to { opacity: 0; transform: translateX(10px); }
</style>