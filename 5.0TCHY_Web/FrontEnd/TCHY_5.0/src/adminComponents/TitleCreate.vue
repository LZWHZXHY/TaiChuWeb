<template>
  <div class="admin-title-dashboard">
    <div class="list-section">
      <div class="section-header">
        <h2>头衔图鉴 // TITLE_ROSTER</h2>
        <span class="count-badge">共 {{ titles.length }} 个头衔</span>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>正在加载系统数据...
      </div>

      <div v-else-if="titles.length === 0" class="empty-state">
        暂无头衔数据，请在右侧创建。
      </div>

      <div v-else class="title-grid">
        <div 
          v-for="title in titles" 
          :key="title.id" 
          class="title-card"
          :class="{ 'is-disabled': title.isDisabled }"
          :style="{ borderLeftColor: getRarityColor(title.rarity) }"
        >
          <div class="card-header">
            <h3 class="title-name" :style="{ color: getRarityColor(title.rarity) }">
              {{ title.name }}
            </h3>
            <span class="rarity-badge" :style="{ backgroundColor: getRarityColor(title.rarity) }">
              {{ getRarityLabel(title.rarity) }}
            </span>
          </div>

          <p class="title-desc">{{ title.description }}</p>

          <div class="card-meta">
            <span class="meta-item">Lv.{{ title.titleLevel }} 阶</span>
            <span class="meta-item">要求: {{ title.requiredLevel }}级</span>
            <span class="meta-item">需: {{ title.requiredCoins }}金币</span>
          </div>

          <div class="card-actions">
            <button class="action-btn grant" @click="grantToSpecificUsers(title.id, title.name)" title="根据ID定向发放给指定用户">
              🎯 发放
            </button>
            <button class="action-btn edit" @click="editTitle(title)">编辑</button>
            <button 
              class="action-btn toggle" 
              :class="title.isDisabled ? 'enable' : 'disable'"
              @click="toggleStatus(title.id)"
            >
              {{ title.isDisabled ? '启用' : '禁用' }}
            </button>
            <button class="action-btn delete" @click="deleteTitle(title.id, title.name)">删除</button>
          </div>
        </div>
      </div>
    </div>

    <div class="form-section">
      <div class="form-panel">
        <div class="form-header">
          <h2>{{ isEditing ? '编辑头衔 // EDIT' : '发放新头衔 // CREATE' }}</h2>
          <button v-if="isEditing" class="cancel-btn" @click="resetForm">取消编辑 ✖</button>
        </div>

        <div class="form-content">
          <div class="form-group">
            <label>头衔名称 <span class="req">*</span></label>
            <input v-model="formData.name" type="text" class="custom-input" placeholder="输入头衔名称..." />
          </div>

          <div class="form-group">
            <label>获取条件描述 <span class="req">*</span></label>
            <textarea v-model="formData.description" class="custom-textarea" placeholder="描述获取方式..."></textarea>
          </div>

          <div class="form-row">
            <div class="form-group half">
              <label>稀有度 (Rarity)</label>
              <select v-model.number="formData.rarity" class="custom-input">
                <option value="1">1 - 普通 (Common)</option>
                <option value="2">2 - 优秀 (Uncommon)</option>
                <option value="3">3 - 稀有 (Rare)</option>
                <option value="4">4 - 史诗 (Epic)</option>
                <option value="5">5 - 传说 (Legendary)</option>
                <option value="6">6 - 神话 (Mythic)</option>
                <option value="7">7 - 不朽 (Immortal)</option>
              </select>
            </div>
            <div class="form-group half">
              <label>头衔等阶 (Level)</label>
              <input v-model.number="formData.titleLevel" type="number" class="custom-input font-mono" min="1" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group half">
              <label>要求角色等级</label>
              <input v-model.number="formData.requiredLevel" type="number" class="custom-input font-mono" min="0" />
            </div>
            <div class="form-group half">
              <label>要求消耗金币</label>
              <input v-model.number="formData.requiredCoins" type="number" class="custom-input font-mono" min="0" />
            </div>
          </div>

          <div class="form-group">
            <label>样式配置 (JSON) <span class="hint">- 选填</span></label>
            <textarea 
              v-model="formData.styleConfig" 
              class="json-textarea" 
              placeholder='{"color": "#ff0000"}'
              @blur="validateJson"
            ></textarea>
            <div v-if="jsonError" class="error-msg">{{ jsonError }}</div>
          </div>

          <button class="submit-btn" @click="handleSubmit" :disabled="isSaving">
            {{ isSaving ? '处理中...' : (isEditing ? '保存修改' : '确认录入系统') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import apiClient from '@/utils/api'; 

const loading = ref(true);
const isSaving = ref(false);
const jsonError = ref('');
const titles = ref([]);

// 稀有度字典 (7个等级)
const rarityDict = {
  1: { label: '普通', color: '#9e9e9e' }, // 灰色
  2: { label: '优秀', color: '#4caf50' }, // 绿色
  3: { label: '稀有', color: '#2196f3' }, // 蓝色
  4: { label: '史诗', color: '#9c27b0' }, // 紫色
  5: { label: '传说', color: '#ff9800' }, // 橙色
  6: { label: '神话', color: '#ffd700' }, // 红色
  7: { label: '不朽', color: '#f44336' }  // 金色
};

const getRarityLabel = (rarity) => rarityDict[rarity]?.label || '未知';
const getRarityColor = (rarity) => rarityDict[rarity]?.color || '#333';

// 表单数据
const formData = reactive({
  id: null,
  name: '',
  description: '',
  requiredLevel: 0,
  requiredCoins: 0,
  titleLevel: 1,
  rarity: 1,
  styleConfig: ''
});

const isEditing = computed(() => formData.id !== null);

// --- API 操作 ---

// 1. 获取列表 (包含大小写映射修复)
const fetchTitles = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/admin/titles/list');
    if (res.data.success) {
      // 🚀 核心修复：把后端的大写字段映射为前端需要的小写字段
      titles.value = res.data.data.map(t => ({
        id: t.Id,
        name: t.Name,
        description: t.Description,
        requiredLevel: t.RequiredLevel,
        requiredCoins: t.RequiredCoins,
        titleLevel: t.TitleLevel,
        rarity: t.Rarity,
        isDisabled: t.IsDisabled,
        styleConfig: t.StyleConfig || ''
      }));
    }
  } catch (error) {
    console.error("获取头衔列表失败", error);
  } finally {
    loading.value = false;
  }
};

// 2. 提交（创建/更新）
const handleSubmit = async () => {
  if (!formData.name.trim() || !formData.description.trim()) {
    alert('头衔名称和描述为必填项！');
    return;
  }

  if (!validateJson()) return;

  isSaving.value = true;
  try {
    const payload = {
      name: formData.name,
      description: formData.description,
      requiredLevel: formData.requiredLevel,
      requiredCoins: formData.requiredCoins,
      titleLevel: formData.titleLevel,
      rarity: formData.rarity,
      styleConfig: formData.styleConfig.trim() || null
    };

    let res;
    if (isEditing.value) {
      res = await apiClient.put(`/admin/titles/update/${formData.id}`, payload);
    } else {
      res = await apiClient.post('/admin/titles/create', payload);
    }

    if (res.data.success) {
      alert(res.data.message);
      resetForm();
      fetchTitles(); // 刷新列表
    } else {
      alert(res.data.message || '操作失败');
    }
  } catch (error) {
    console.error(error);
    alert('请求错误');
  } finally {
    isSaving.value = false;
  }
};

// 3. 启用/禁用状态切换
const toggleStatus = async (id) => {
  try {
    const res = await apiClient.put(`/admin/titles/toggle-status/${id}`);
    if (res.data.success) {
      fetchTitles(); // 刷新列表查看最新状态
    }
  } catch (error) {
    alert("切换状态失败");
  }
};

// 4. 彻底删除
const deleteTitle = async (id, name) => {
  if (!confirm(`⚠️ 危险操作：确认要彻底删除头衔【${name}】吗？\n所有拥有此头衔的玩家也会失去它！`)) {
    return;
  }
  try {
    const res = await apiClient.delete(`/admin/titles/delete/${id}`);
    if (res.data.success) {
      alert(res.data.message);
      if (formData.id === id) resetForm(); // 如果正在编辑这个被删的头衔，清空表单
      fetchTitles();
    }
  } catch (error) {
    alert("删除失败");
  }
};

// 5. 定向批量发放头衔
const grantToSpecificUsers = async (id, name) => {
  const inputStr = prompt(
    `🎯 正在为【${name}】进行定向发放\n\n请在下方输入目标用户的 ID，多个 ID 之间请用英文逗号 (,) 隔开：\n例如: 154, 265, 888`
  );

  // 如果管理员点了取消或输入为空
  if (!inputStr) return;

  // 解析字符串，转为数字数组，并过滤掉非数字的输入
  const userIds = inputStr
    .split(',')
    .map(s => parseInt(s.trim()))
    .filter(n => !isNaN(n));

  if (userIds.length === 0) {
    alert("❌ 未识别到有效的用户ID，请检查输入格式！");
    return;
  }

  // 二次确认
  if (!confirm(`即将尝试为 ${userIds.length} 个用户发放头衔，\n解析出的目标 ID: ${userIds.join(', ')}\n\n确认执行吗？`)) {
    return;
  }
  
  try {
    const res = await apiClient.post(`/admin/titles/grant-batch/${id}`, { userIds });
    if (res.data.success) {
      alert(`🎉 发放结果：\n\n${res.data.message}`);
    } else {
      alert(res.data.message || '操作失败');
    }
  } catch (error) {
    console.error(error);
    alert("请求发送失败，请检查网络！");
  }
};

// --- UI 逻辑 ---

const validateJson = () => {
  if (!formData.styleConfig.trim()) {
    jsonError.value = '';
    return true;
  }
  try {
    JSON.parse(formData.styleConfig);
    jsonError.value = '';
    return true;
  } catch (e) {
    jsonError.value = 'JSON 格式错误';
    return false;
  }
};

const editTitle = (title) => {
  formData.id = title.id;
  formData.name = title.name;
  formData.description = title.description;
  formData.requiredLevel = title.requiredLevel;
  formData.requiredCoins = title.requiredCoins;
  formData.titleLevel = title.titleLevel;
  formData.rarity = title.rarity;
  formData.styleConfig = title.styleConfig || '';
  
  // 滚动到顶部方便编辑
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const resetForm = () => {
  formData.id = null;
  formData.name = '';
  formData.description = '';
  formData.requiredLevel = 0;
  formData.requiredCoins = 0;
  formData.titleLevel = 1;
  formData.rarity = 1;
  formData.styleConfig = '';
  jsonError.value = '';
};

onMounted(() => {
  fetchTitles();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* 全局布局 */
.admin-title-dashboard {
  display: flex;
  gap: 24px;
  padding: 24px;
  min-height: 80vh;
  font-family: 'Noto Sans SC', sans-serif;
  background-color: #f8f9fa;
  box-sizing: border-box;
}

/* --- 左侧：列表区 --- */
.list-section {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-header h2 { margin: 0; font-size: 20px; font-weight: 900; color: #2c3e50; }
.count-badge { background: #e0e0e0; padding: 4px 10px; border-radius: 12px; font-size: 12px; font-family: 'Roboto Mono'; color: #555; }

.loading-state, .empty-state {
  text-align: center; padding: 40px; color: #888; font-size: 14px;
  background: #fff; border-radius: 12px; border: 1px dashed #ccc;
}
.spinner { width: 20px; height: 20px; border: 2px solid #ccc; border-top-color: #333; border-radius: 50%; animation: spin 1s linear infinite; display: inline-block; vertical-align: middle; margin-right: 8px; }
@keyframes spin { to { transform: rotate(360deg); } }

.title-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
  overflow-y: auto;
  padding-right: 8px;
}

/* 卡片样式 */
.title-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  border-left: 4px solid #333; /* 通过内联样式覆盖颜色 */
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
  transition: transform 0.2s, box-shadow 0.2s;
  display: flex;
  flex-direction: column;
}
.title-card:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(0,0,0,0.08); }
.title-card.is-disabled { opacity: 0.6; filter: grayscale(0.8); }

.card-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px; }
.title-name { margin: 0; font-size: 16px; font-weight: 900; }
.rarity-badge { color: #fff; padding: 2px 8px; border-radius: 4px; font-size: 10px; font-weight: bold; }

.title-desc { font-size: 12px; color: #666; margin: 0 0 12px 0; line-height: 1.5; flex: 1; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

.card-meta { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 16px; }
.meta-item { background: #f0f2f5; font-size: 11px; padding: 4px 8px; border-radius: 4px; color: #555; font-family: 'Roboto Mono'; }

.card-actions { display: flex; gap: 8px; border-top: 1px solid #eee; padding-top: 12px; }
.action-btn { flex: 1; border: none; padding: 6px 0; border-radius: 6px; font-size: 12px; font-weight: bold; cursor: pointer; transition: background 0.2s; }

/* 按钮颜色配置 */
.action-btn.grant { background: #fff8e1; color: #f57f17; flex: 1.2; }
.action-btn.grant:hover { background: #ffecb3; }
.action-btn.edit { background: #e3f2fd; color: #1976d2; }
.action-btn.edit:hover { background: #bbdefb; }
.action-btn.toggle.disable { background: #fff3e0; color: #f57c00; }
.action-btn.toggle.enable { background: #e8f5e9; color: #388e3c; }
.action-btn.delete { background: #ffebee; color: #d32f2f; }
.action-btn.delete:hover { background: #ffcdd2; }

/* --- 右侧：表单区 --- */
.form-section {
  width: 380px;
  flex-shrink: 0;
}

.form-panel {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 8px 30px rgba(0,0,0,0.04);
  padding: 24px;
  position: sticky;
  top: 24px;
}

.form-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; border-bottom: 2px solid #f0f0f0; padding-bottom: 12px; }
.form-header h2 { margin: 0; font-size: 18px; font-weight: 900; color: #111; }
.cancel-btn { background: none; border: none; color: #999; cursor: pointer; font-size: 12px; }
.cancel-btn:hover { color: #d32f2f; }

.form-content { display: flex; flex-direction: column; gap: 16px; }

.form-group { display: flex; flex-direction: column; }
.form-row { display: flex; gap: 12px; }
.half { flex: 1; }

label { font-size: 12px; font-weight: 700; color: #444; margin-bottom: 6px; }
.req { color: #e91e63; }
.hint { color: #999; font-weight: normal; font-size: 11px; }

.custom-input, .custom-textarea, .json-textarea {
  width: 100%; padding: 10px 12px; background: #f8f9fa;
  border: 1px solid #e0e0e0; border-radius: 8px;
  font-size: 13px; color: #333; box-sizing: border-box; transition: border-color 0.2s;
}
.custom-input:focus, .custom-textarea:focus, .json-textarea:focus { border-color: #2c3e50; outline: none; background: #fff; }
.custom-textarea { resize: vertical; min-height: 80px; }
.font-mono { font-family: 'Roboto Mono', monospace; }

.json-textarea { font-family: 'Roboto Mono', monospace; background: #1e1e1e; color: #a5d6ff; border: none; }
.json-textarea:focus { background: #111; }
.error-msg { color: #e91e63; font-size: 11px; margin-top: 4px; }

.submit-btn {
  background: #2c3e50; color: #fff; padding: 12px; border-radius: 8px; border: none;
  font-weight: 700; font-size: 14px; cursor: pointer; margin-top: 10px; transition: background 0.2s;
}
.submit-btn:hover:not(:disabled) { background: #1a252f; }
.submit-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* 响应式调整 */
@media (max-width: 1024px) {
  .admin-title-dashboard { flex-direction: column; }
  .form-section { width: 100%; }
  .form-panel { position: relative; top: 0; }
}
</style>