<template>
  <div class="profile-setting-wrapper">
    
    <div v-if="loading" class="loading-mask">
      <div class="spinner"></div>
      <span>加载中...</span>
    </div>

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
              maxlength="1000"
            ></textarea>
            <div class="char-counter">
              <span>{{ formData.bio ? formData.bio.length : 0 }}</span> / 1000
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
                    <input v-model="link.title" class="link-input" placeholder="如: B站" />
                  </div>
                  <div class="input-cell url-cell">
                    <span class="cell-label">URL</span>
                    <input v-model="link.url" class="link-input font-mono" placeholder="https://" />
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
      <div class="scroll-container">
        <div class="form-header">
          <h2>基本信息 // BASIC_INFO</h2>
          <div class="line"></div>
        </div>

        <div class="form-grid">
          <div class="form-group full">
            <label>
              昵称 // NICKNAME 
              <span class="tag-badge">需要改名卡</span>
            </label>
            <input v-model="formData.name" type="text" class="standard-input disabled" disabled placeholder="用户昵称" />
          </div>

          <div class="form-group full">
            <label>个性签名 // SIGNATURE</label>
            <input v-model="formData.personalSignature" type="text" class="standard-input" placeholder="一句话介绍自己..." maxlength="200" />
          </div>

          <div class="form-row">
            <div class="form-group half">
              <label>性别 // GENDER</label>
              <input v-model="formData.gender" type="text" class="standard-input" placeholder="自定义性别 (如: 武装直升机)" maxlength="50" />
            </div>
            
            <div class="form-group half">
              <label>生日 // BIRTHDAY</label>
              <input v-model="formData.birthday" type="date" class="standard-input" />
            </div>
          </div>

          <div class="form-group full">
            <label>地区 // LOCATION</label>
            <input v-model="formData.location" type="text" class="standard-input" placeholder="例如：夜之城" />
          </div>
          
          <div class="form-group full">
              <label>详细地址 // ADDRESS</label>
              <input v-model="formData.address" type="text" class="standard-input" placeholder="街道/门牌号 (选填)" />
          </div>

          <div class="form-group full">
            <label>联系方式 // CONTACT</label>
            <input v-model="formData.contact" type="text" class="standard-input" placeholder="VX / Email / QQ" />
          </div>
          
          <div class="form-group full">
              <label>兴趣爱好 // INTERESTS</label>
              <input v-model="formData.interests" type="text" class="standard-input" placeholder="编程, 绘画, 游戏..." />
          </div>

          <div class="form-group full extra-section">
            <div class="extra-header" @click="showExtra = !showExtra">
              <label style="margin:0; cursor:pointer">
                高级配置 (JSON) // EXTRA_CONTENT
                <span class="arrow" :class="{ rotated: showExtra }">▼</span>
              </label>
            </div>
            
            <div v-show="showExtra" class="json-editor-wrapper">
              <p class="hint-text">请填写合法的 JSON 格式，用于配置背景图、主题色等。</p>
              <textarea 
                v-model="formData.extraContent" 
                class="json-textarea" 
                placeholder='例如: {"theme": "dark", "bgImage": "url..."}'
                @blur="validateJson"
              ></textarea>
              <div v-if="jsonError" class="error-msg">{{ jsonError }}</div>
            </div>
          </div>

        </div>

        <div class="footer-action">
          <button class="save-button" @click="handleSave" :disabled="isSaving">
            <span class="btn-text">{{ isSaving ? '保存中...' : '保存修改 // SAVE' }}</span>
            <span class="btn-deco" v-if="!isSaving">➜</span>
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { reactive, onMounted, ref } from 'vue';
import apiClient from '@/utils/api'; 

const loading = ref(true);
const isSaving = ref(false);
const showExtra = ref(false); 
const jsonError = ref('');

// 表单数据
const formData = reactive({
  name: '', 
  gender: '', 
  location: '',
  address: '',
  birthday: '',
  contact: '',
  bio: '',
  personalSignature: '',
  interests: '',
  extraContent: '', 
  links: [] 
});

const validateJson = () => {
  if (!formData.extraContent) {
    jsonError.value = '';
    return true;
  }
  try {
    JSON.parse(formData.extraContent);
    jsonError.value = '';
    return true;
  } catch (e) {
    jsonError.value = 'JSON 格式错误，请检查引号和括号';
    return false;
  }
};

// 1. 初始化数据
const initData = async () => {
  try {
    loading.value = true;
    
    // 并行请求
    const [detailRes, statusRes] = await Promise.all([
      apiClient.get('/profile/detail'),
      apiClient.get('/profile/me') 
    ]);

    // A. 这里的 statusRes 主要是获取等级状态，不需要用来覆盖名字
    // (之前的错误逻辑已移除)

    // B. 填充 Profile 资料 (包含 Name, Bio, Links 等)
    if (detailRes.data && detailRes.data.success) {
      const data = detailRes.data.data;
      
      // ✅ 修正点：从 detail 接口获取真实的 Name (用户名)
      formData.name = data.Name || '未知用户'; 

      // 其他字段映射 (注意 PascalCase)
      formData.bio = data.Bio || '';
      formData.gender = data.Gender || ''; 
      formData.location = data.Region || ''; 
      formData.address = data.Address || '';
      formData.contact = data.ContactInfo || ''; 
      formData.personalSignature = data.PersonalSignature || '';
      formData.interests = data.Interests || '';
      formData.extraContent = data.ExtraContent || ''; 

      // 处理日期
      if (data.BirthDate) {
        formData.birthday = data.BirthDate.split('T')[0];
      } else {
        formData.birthday = '';
      }

      // 处理链接
      if (data.SocialLinks && Array.isArray(data.SocialLinks)) {
        formData.links = data.SocialLinks.map(link => ({
          title: link.Name, 
          url: link.Url     
        }));
      } else {
        formData.links = [];
      }
    } else {
        // 如果 detail 获取失败，尝试从本地缓存兜底显示名字
        const userStr = localStorage.getItem('user_info');
        if(userStr) {
            const user = JSON.parse(userStr);
            formData.name = user.name || user.nickname || '未命名用户';
        }
    }

  } catch (error) {
    console.error('获取资料失败:', error);
  } finally {
    loading.value = false;
  }
};

// 2. 添加/删除链接
const addLink = () => { formData.links.push({ title: '', url: '' }); };
const removeLink = (index) => { formData.links.splice(index, 1); };

// 3. 保存逻辑
const handleSave = async () => {
  if (isSaving.value) return;
  if (!validateJson()) {
    showExtra.value = true; 
    return;
  }
  isSaving.value = true;

  try {
    const payload = {
      bio: formData.bio,
      gender: formData.gender,
      region: formData.location, 
      address: formData.address,
      contactInfo: formData.contact,
      personalSignature: formData.personalSignature,
      interests: formData.interests,
      extraContent: formData.extraContent, 
      birthDate: formData.birthday ? formData.birthday : null,
      
      socialLinks: formData.links
        .filter(l => l.title && l.url)
        .map(l => ({
          name: l.title,
          url: l.url
        }))
    };

    const res = await apiClient.put('/profile/update', payload);

    if (res.data && res.data.success) {
      alert('资料已更新 // DATA_SAVED');
    } else {
      alert(res.data.message || '保存失败');
    }
  } catch (error) {
    console.error(error);
    alert('网络错误，保存失败');
  } finally {
    isSaving.value = false;
  }
};

onMounted(() => {
  initData();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* 布局调整：防止内容溢出 */
.profile-setting-wrapper {
  width: 100%; height: 100%;
  display: flex;
  gap: 30px;
  padding: 30px;
  box-sizing: border-box;
  font-family: 'Noto Sans SC', sans-serif;
  position: relative;
  overflow: hidden; /* 防止最外层出现滚动条 */
}

.loading-mask {
    position: absolute; inset: 0; background: rgba(255,255,255,0.9);
    display: flex; flex-direction: column; gap: 10px;
    justify-content: center; align-items: center; z-index: 100;
}
.spinner {
    width: 30px; height: 30px; border: 3px solid #eee; border-top-color: #000;
    border-radius: 50%; animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* 禁用输入框 */
.standard-input.disabled {
    background: #f4f4f4; color: #aaa; cursor: not-allowed; border-color: #eee;
}

/* === 左侧面板 === */
.left-panel { flex: 1; display: flex; flex-direction: column; min-width: 300px; }

.panel-card {
  background: #F4F1EA; border-radius: 24px; padding: 24px;
  height: 100%; display: flex; flex-direction: column; gap: 20px;
  box-sizing: border-box;
}

.panel-header { display: flex; align-items: center; gap: 8px; color: #333; }
.panel-header h3 { margin: 0; font-size: 14px; font-weight: 700; letter-spacing: 0.5px; }
.deco-icon { font-size: 16px; color: #d35400; }
.panel-header.small { justify-content: space-between; border-bottom: 1px dashed rgba(0,0,0,0.1); padding-bottom: 8px; }

/* Bio */
.bio-wrapper { flex: 1; display: flex; flex-direction: column; min-height: 150px; }
.textarea-container {
  flex: 1; position: relative; background: #fff; border-radius: 12px;
  padding: 16px; box-shadow: inset 0 2px 6px rgba(0,0,0,0.02);
  transition: box-shadow 0.2s; display: flex; flex-direction: column;
}
.textarea-container:focus-within { box-shadow: inset 0 2px 6px rgba(0,0,0,0.05), 0 0 0 2px #000; }

textarea {
  flex: 1; width: 100%; border: none; background: transparent; resize: none; outline: none;
  font-size: 14px; line-height: 1.6; color: #333; font-family: 'Noto Sans SC';
}
.char-counter { position: absolute; bottom: 12px; right: 16px; font-size: 11px; font-family: 'Roboto Mono'; color: #999; }
.corner-deco { position: absolute; width: 10px; height: 10px; border-right: 2px solid #d35400; border-bottom: 2px solid #d35400; bottom: 6px; right: 6px; opacity: 0.5; }

/* Links */
.links-wrapper { flex: 1.5; display: flex; flex-direction: column; overflow: hidden; }
.add-btn-mini { background: #000; color: #fff; border: none; width: 24px; height: 24px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 16px; transition: transform 0.2s; }
.add-btn-mini:hover { transform: scale(1.1); }
.links-scroll { flex: 1; overflow-y: auto; padding-top: 10px; padding-right: 4px; }

.link-row {
  display: flex; align-items: center; gap: 8px; margin-bottom: 12px;
  background: #fff; padding: 12px; border-radius: 12px;
  border: 1px solid rgba(0,0,0,0.05); box-shadow: 0 2px 8px rgba(0,0,0,0.02);
}
.link-body { flex: 1; display: flex; flex-direction: column; gap: 8px; }
.input-cell { display: flex; align-items: center; background: #F7F7F7; border-radius: 8px; padding: 4px 10px; border: 1px solid transparent; }
.input-cell:focus-within { background: #fff; border-color: #000; }
.cell-label { font-size: 10px; color: #999; margin-right: 8px; font-weight: bold; flex-shrink: 0; width: 28px; }
.link-input { border: none; outline: none; background: transparent; width: 100%; font-size: 13px; color: #333; padding: 4px 0; font-family: 'Noto Sans SC'; }
.font-mono { font-family: 'Roboto Mono', monospace; font-size: 12px; }
.del-btn { border: none; background: transparent; color: #ccc; cursor: pointer; font-size: 18px; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; }
.del-btn:hover { color: #ff4d4f; }
.empty-hint { text-align: center; color: #ccc; font-size: 12px; padding: 20px; }

/* === 右侧面板 === */
.right-panel {
  flex: 1.5; padding: 0 10px;
  display: flex; flex-direction: column; overflow: hidden; /* 防止超出 */
}
.scroll-container {
    height: 100%; overflow-y: auto; padding-right: 10px; /* 内部滚动 */
    display: flex; flex-direction: column;
}
/* 隐藏滚动条但保留功能 */
.scroll-container::-webkit-scrollbar { width: 6px; }
.scroll-container::-webkit-scrollbar-thumb { background: #ddd; border-radius: 3px; }

.form-header { margin-bottom: 20px; }
.form-header h2 { font-size: 24px; font-weight: 900; margin: 0; letter-spacing: -1px; }
.form-header .line { width: 40px; height: 4px; background: #000; margin-top: 8px; }

.form-grid { display: flex; flex-direction: column; gap: 20px; flex: 1; }
.form-row { display: flex; gap: 20px; }
.form-group.full { width: 100%; }
.form-group.half { flex: 1; }

label { display: flex; justify-content: space-between; align-items: center; font-size: 12px; font-weight: 700; color: #666; margin-bottom: 8px; }
.tag-badge { background: #000; color: #fff; padding: 2px 6px; border-radius: 4px; font-size: 10px; }

.standard-input {
  width: 100%; padding: 12px 16px; background: #FFFDF8;
  border: 1px solid #e0e0e0; border-radius: 12px;
  font-size: 14px; color: #333; transition: all 0.2s; box-sizing: border-box;
}
.standard-input:focus { border-color: #000; background: #fff; box-shadow: 0 4px 12px rgba(0,0,0,0.05); outline: none; }

/* Extra Content 样式 */
.extra-section { background: #f9f9f9; border-radius: 12px; padding: 10px; border: 1px solid #eee; }
.extra-header { cursor: pointer; user-select: none; }
.arrow { transition: transform 0.3s; display: inline-block; font-size: 10px; margin-left: 5px; }
.arrow.rotated { transform: rotate(180deg); }
.json-editor-wrapper { margin-top: 10px; }
.hint-text { font-size: 12px; color: #999; margin: 0 0 5px 0; }
.json-textarea {
    width: 100%; height: 100px; padding: 10px; font-family: 'Roboto Mono', monospace; font-size: 12px;
    background: #222; color: #0f0; border-radius: 8px; border: none; resize: vertical; box-sizing: border-box;
}
.error-msg { color: red; font-size: 12px; margin-top: 4px; }

/* 底部按钮 */
.footer-action { margin-top: 30px; display: flex; justify-content: flex-end; padding-bottom: 20px; }
.save-button {
  background: #000; color: #fff; padding: 12px 40px; border-radius: 30px; border: none;
  font-weight: 700; font-size: 14px; cursor: pointer; display: flex; align-items: center; gap: 10px;
  transition: transform 0.2s, box-shadow 0.2s;
}
.save-button:disabled { opacity: 0.7; cursor: not-allowed; }
.save-button:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(0,0,0,0.2); }
.btn-deco { font-size: 16px; color: #d35400; }

.list-enter-active, .list-leave-active { transition: all 0.3s ease; }
.list-enter-from, .list-leave-to { opacity: 0; transform: translateX(10px); }
</style>