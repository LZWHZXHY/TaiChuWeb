<template>
  <Transition name="glitch-fade">
    <div v-if="modelValue" class="cyber-modal-overlay" @click.self="close">
      <div class="cyber-modal-window archive-mode">
        
        <div class="modal-header">
          <div class="header-left">
            <span class="status-light blink"></span>
            <span class="title">STAR_ARCHIVES // 星标数据库</span>
          </div>
          <button class="close-btn" @click="close">[ESC]</button>
        </div>

        <div class="modal-body custom-scroll">
          <div class="context-tag">
            <span>TARGET_NODE: #{{ targetId }}</span>
            <span class="green-text">READY_FOR_INJECTION</span>
          </div>

          <div v-if="!showCreateForm" class="create-trigger" @click="showCreateForm = true">
            <span class="plus">+</span> INITIALIZE_NEW_ARCHIVE // 新建收藏夹
          </div>

          <div v-else class="new-folder-form">
            <input 
              v-model="newTitle" 
              placeholder="输入新收藏夹名称..." 
              class="cyber-input mini"
              @keyup.enter="handleCreateFolder"
            />
            <div class="form-actions">
              <button class="action-btn confirm" @click="handleCreateFolder" :disabled="!newTitle || creating">确认</button>
              <button class="action-btn cancel" @click="showCreateForm = false">取消</button>
            </div>
          </div>

          <hr class="cyber-divider" />

          <div v-if="loading" class="loading-state">
            <span class="blink">>> SCANNING_FOLDERS...</span>
          </div>
          
          <div v-else-if="folders.length === 0" class="empty-state">
            [ NO_ARCHIVES_FOUND ]
          </div>

          <div class="folder-grid">
            <div 
              v-for="folder in folders" 
              :key="folder.id || folder.Id" 
              class="folder-item"
              :class="{ 'is-selected': selectedId === (folder.id || folder.Id) }"
              @click="selectedId = (folder.id || folder.Id)"
            >
              <div class="folder-icon">★</div>
              <div class="folder-info">
                <div class="f-name">{{ folder.title || folder.Title || 'UNNAMED_ARCHIVE' }}</div>
                <div class="f-meta">{{ folder.itemCount || folder.ItemCount || 0 }} ITEMS_STORED</div>
              </div>
              <div class="select-indicator"></div>
            </div>
          </div>







        </div>

        <div class="modal-footer">
          <div class="status">
            STATUS: <span :class="selectedId ? 'green-text' : 'red-text'">
              {{ selectedId ? 'TARGET_LOCKED' : 'AWAITING_SELECTION' }}
            </span>
          </div>
          <button 
            class="execute-btn" 
            :disabled="!selectedId || processing"
            @click="handleConfirmArchive"
          >
            <span class="btn-text">{{ processing ? 'TRANSMITTING...' : 'INITIATE_INJECTION' }}</span>
            <span class="btn-deco">>>></span>
          </button>
        </div>

      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch } from 'vue';
import apiClient from '@/utils/api';

const props = defineProps({
  modelValue: Boolean,
  targetId: Number,
  category: { type: Number, default: 0 } // 0: Drawing, 1: Post, 2: Blog
});

const emit = defineEmits(['update:modelValue', 'success']);

// 状态变量
const folders = ref([]);
const loading = ref(false);
const processing = ref(false);
const selectedId = ref(null);

// 创建文件夹相关
const showCreateForm = ref(false);
const newTitle = ref('');
const creating = ref(false);

// 获取我的收藏夹
const fetchFolders = async () => {
  loading.value = true;
  try {
    // 调用后端接口：获取类别为当前category，且FolderType=1(收藏夹)的列表
    const res = await apiClient.get(`/Folder/my-list?category=${props.category}&type=1`);
    if (res.data.success) {
      folders.value = res.data.data;
    }
  } catch (e) {
    console.error("Failed to fetch folders");
  } finally {
    loading.value = false;
  }
};

// 提交新建文件夹
const handleCreateFolder = async () => {
  if (!newTitle.value.trim()) return;
  creating.value = true;
  try {
    const res = await apiClient.post('/Folder/create', {
      title: newTitle.value.trim(),
      category: props.category,
      folderType: 1 // 1代表观众个人的收藏夹
    });
    if (res.data.success) {
      newTitle.value = '';
      showCreateForm.value = false;
      await fetchFolders(); // 刷新列表
    }
  } catch (e) {
    alert("创建失败");
  } finally {
    creating.value = false;
  }
};

// 确认收藏注入
const handleConfirmArchive = async () => {
  if (!selectedId.value) return;
  processing.value = true;
  try {
    const res = await apiClient.post('/Folder/add-item', {
      folderId: selectedId.value,
      targetId: props.targetId
    });
    
    if (res.data.success) {
      close();
      emit('success');
    } else {
      alert(res.data.message || "注入失败或已存在");
    }
  } catch (e) {
    alert("INJECTION_ERROR");
  } finally {
    processing.value = false;
  }
};

const close = () => {
  emit('update:modelValue', false);
};

// 监听弹窗打开时刷新列表
watch(() => props.modelValue, (val) => {
  if (val) {
    selectedId.value = null;
    showCreateForm.value = false;
    fetchFolders();
  }
});
</script>

<style scoped>
/* 继承全局赛博风格 */
.cyber-modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 2000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(5px); }
/* ✅ 给弹窗根节点重新补上局部变量，防止 Teleport 丢失 */
.cyber-modal-window.archive-mode { 
  --black: #111111;
  --red: #D92323;
  width: 450px; 
  max-width: 95vw; 
  background: #f4f4f4; 
  border: 4px solid var(--black); 
  box-shadow: 15px 15px 0 rgba(0,0,0,0.5); 
  display: flex; 
  flex-direction: column; 
  max-height: 85vh; 
  font-family: 'JetBrains Mono', monospace; 
}

/* ✅ 强制设定文字颜色为深黑色 */
.f-name { 
  font-weight: bold; 
  font-family: 'Anton', sans-serif; 
  font-size: 1.1rem; 
  color: #111111; /* 直接写死颜色防丢 */
  letter-spacing: 0.5px; 
}
.modal-header { background: #000; color: #fff; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #FFD700; flex-shrink: 0; }
.header-left { display: flex; align-items: center; gap: 10px; }
.status-light { width: 10px; height: 10px; background: #FFD700; border-radius: 50%; box-shadow: 0 0 5px #FFD700; }
.title { font-family: 'Anton', sans-serif; font-size: 1.2rem; letter-spacing: 1px; color: #FFD700; }
.close-btn { background: transparent; border: 1px solid #666; color: #aaa; padding: 4px 10px; cursor: pointer; font-size: 0.75rem; transition: 0.2s; }
.close-btn:hover { border-color: var(--red); color: var(--red); }

.modal-body { padding: 20px; overflow-y: auto; flex: 1; }
.context-tag { background: var(--black); color: #fff; padding: 8px 10px; font-size: 0.75rem; margin-bottom: 15px; display: flex; justify-content: space-between; }
.green-text { color: #0f0; }
.red-text { color: var(--red); }

.create-trigger { text-align: center; border: 1px dashed #999; padding: 12px; cursor: pointer; font-size: 0.85rem; font-weight: bold; transition: 0.2s; color: #555; }
.create-trigger .plus { color: var(--red); font-weight: 900; margin-right: 5px; }
.create-trigger:hover { background: #fff; border-color: var(--black); color: var(--black); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }

.new-folder-form { display: flex; gap: 10px; background: #ddd; padding: 10px; border: 1px solid #999; }
.cyber-input.mini { flex: 1; border: 2px solid #999; padding: 5px 10px; outline: none; font-family: 'JetBrains Mono'; }
.cyber-input.mini:focus { border-color: var(--black); }
.form-actions { display: flex; gap: 5px; }
.action-btn { border: 2px solid var(--black); background: transparent; cursor: pointer; font-weight: bold; padding: 0 10px; }
.action-btn.confirm { background: var(--black); color: #fff; }
.action-btn.confirm:disabled { background: #999; border-color: #999; cursor: not-allowed; }

.cyber-divider { border: none; border-top: 2px dashed #ccc; margin: 20px 0; }

.folder-grid { display: flex; flex-direction: column; gap: 10px; }
.folder-item { display: flex; align-items: center; padding: 12px; border: 2px solid #ccc; background: #fff; cursor: pointer; transition: 0.2s; position: relative; }
.folder-item:hover { border-color: var(--black); transform: translateX(2px); }
.folder-item.is-selected { border-color: #FFD700; background: #fffdf0; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.folder-icon { font-size: 1.5rem; margin-right: 15px; color: #999; transition: 0.2s; }
.folder-item.is-selected .folder-icon { color: #FFD700; text-shadow: 0 0 5px rgba(255, 215, 0, 0.5); }
.f-name { font-weight: bold; font-family: 'Anton', sans-serif; font-size: 1.1rem; color: var(--black); letter-spacing: 0.5px; }
.f-meta { font-size: 0.7rem; color: #888; margin-top: 2px; }
.select-indicator { position: absolute; right: 15px; width: 12px; height: 12px; border: 2px solid #ccc; border-radius: 50%; }
.folder-item.is-selected .select-indicator { border-color: #FFD700; background: #FFD700; box-shadow: 0 0 5px #FFD700; }

.modal-footer { padding: 15px 20px; background: #e5e5e5; display: flex; justify-content: space-between; align-items: center; border-top: 2px solid #ccc; }
.status { font-size: 0.8rem; font-weight: bold; }
.execute-btn { background: var(--black); color: #fff; border: none; padding: 10px 20px; font-family: 'Anton', sans-serif; font-size: 1.1rem; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 8px; box-shadow: 4px 4px 0 #999; }
.execute-btn:hover:not(:disabled) { background: #FFD700; color: var(--black); transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.execute-btn:disabled { background: #ccc; color: #888; cursor: not-allowed; box-shadow: none; }

.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
.glitch-fade-enter-from, .glitch-fade-leave-to { opacity: 0; transform: scale(0.95); }
.blink { animation: blink 1s infinite; }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>