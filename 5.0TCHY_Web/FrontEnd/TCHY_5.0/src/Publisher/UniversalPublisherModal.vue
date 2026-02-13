<template>
  <div class="modal-mask" @click.self="handleClose">
    <div :class="['tactical-console', { 'full-screen': currentMode === 'Blog' }]">
      
      <div class="console-header">
        <div class="system-id">
          <span class="blink">●</span> 
          NEURAL_LINK // {{ currentMode.toUpperCase() }}_PROTOCOL
        </div>
        <div class="console-actions">
          <button class="action-btn" @click="handleClose">MINIMIZE [-]</button>
          <button class="action-btn close" @click="handleClose">TERMINATE [X]</button>
        </div>
      </div>

      <div class="console-main">
        <aside class="mode-sidebar">
          <div 
            v-for="mode in modes" :key="mode.value"
            :class="['mode-node', { active: currentMode === mode.value }]"
            @click="currentMode = mode.value"
          >
            <div class="node-icon">{{ mode.icon }}</div>
            <div class="node-label">{{ mode.label }}</div>
          </div>
        </aside>

        <section class="editor-view custom-scroll">
          <Transition name="fade-slide" mode="out-in">
            <component 
              :is="activeComponent" 
              ref="activeSubForm"
              :shared-meta="sharedMeta"
              @success="handleSuccess"
            />
          </Transition>
        </section>

        <aside class="meta-sidebar">
          <div class="meta-block">
            <label class="section-label">// TAGS / 标签系统</label>
            <CyberTagInput v-model="sharedMeta.tags" :maxTags="5" />
            <p class="hint">合理的标签能提高中枢检索率</p>
          </div>

          <div class="system-status">
            <div class="status-row"><span>AUTH:</span> <span class="val">CONNECTED</span></div>
            <div class="status-row"><span>SYNC:</span> <span class="val">READY</span></div>
          </div>
          
          <button class="publish-trigger" @click="triggerPublish" :disabled="isPublishing">
            <div class="btn-inner">
              <span class="btn-deco">>>></span>
              <span class="btn-text">{{ isPublishing ? 'EXECUTING...' : 'EXECUTE_SYNC // 确认发布' }}</span>
            </div>
            <div class="btn-scanline"></div>
          </button>
        </aside>
      </div>

      <div class="console-footer">
        <span class="deco-text">TAICHU_CORE // NEURAL_LINK_STABLE // {{ new Date().getFullYear() }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, markRaw } from 'vue';
import { usePublisherStore } from '@/stores/publisher';
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue';

// 引入同级组件
import PublishPost from './PublishPost.vue'; 
import PublishBlog from './PublishBlog.vue';
import PublishDrawing from './PublishDrawing.vue';

const publisherStore = usePublisherStore();
const emit = defineEmits(['success']);

const currentMode = ref('Post');
const isPublishing = ref(false);
const activeSubForm = ref(null);

const modes = [
  { label: '动态', value: 'Post', icon: '⚡', component: markRaw(PublishPost) },
  { label: '专栏', value: 'Blog', icon: '📝', component: markRaw(PublishBlog) },
  { label: '视界', value: 'Drawing', icon: '🎨', component: markRaw(PublishDrawing) }
];

// ✅ 虽然 UI 移除了，但在数据层保留 visibility，默认设为 public
// 这样子组件的 submit 函数依然能读到该值发送给后端
const sharedMeta = reactive({
  tags: '',
  visibility: 'public' 
});

const activeComponent = computed(() => {
  const mode = modes.find(m => m.value === currentMode.value);
  return mode ? mode.component : null;
});

const triggerPublish = async () => {
  if (!activeSubForm.value) return;
  if (activeSubForm.value.validate()) {
    isPublishing.value = true;
    try {
      await activeSubForm.value.submit();
    } catch (err) {
      console.error('Submission Interrupted:', err);
    } finally {
      isPublishing.value = false;
    }
  }
};

const handleSuccess = (data) => {
  emit('success', data);
  publisherStore.close();
};

const handleClose = () => publisherStore.close();
</script>
<style scoped>
/* --- 全局控制台变量 --- */
.modal-mask {
  --red: #D92323;
  --black: #111111;
  --bg: #F4F1EA;
  --panel-white: #FFFFFF;
  --font-mono: 'JetBrains Mono', monospace;
  --font-title: 'Anton', sans-serif;
  
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.85);
  backdrop-filter: blur(12px);
  z-index: 9999;
  display: flex; align-items: center; justify-content: center;
}

/* --- 强制输入框字体颜色 --- */
:deep(input), :deep(textarea), :deep(select) {
  color: #111111 !important; /* 👈 强制黑色，解决白字问题 */
  font-family: var(--font-mono);
}

/* 战术控制台容器 */
.tactical-console {
  width: 1000px; height: 720px;
  background: var(--bg);
  border: 4px solid var(--black);
  display: flex; flex-direction: column;
  transition: all 0.4s cubic-bezier(0.19, 1, 0.22, 1);
  box-shadow: 15px 15px 0 rgba(0,0,0,0.4);
  position: relative;
}

.tactical-console.full-screen { width: 98vw; height: 95vh; }

/* 头部操作栏 */
.console-header {
  background: var(--black); color: var(--bg);
  padding: 10px 18px; display: flex; justify-content: space-between; align-items: center;
  font-family: var(--font-mono); font-size: 11px;
}

/* 按钮优化：通用操作按钮 */
.action-btn {
  background: transparent; border: 1px solid #444; color: #888;
  font-family: var(--font-mono); font-size: 10px; padding: 4px 10px;
  cursor: pointer; transition: 0.2s; margin-left: 8px;
}
.action-btn:hover { border-color: var(--bg); color: var(--bg); }
.action-btn.close:hover { background: var(--red); border-color: var(--red); }

.console-main { flex: 1; display: flex; overflow: hidden; }

/* 侧边栏 */
.mode-sidebar { width: 85px; border-right: 2px solid var(--black); background: #E0DDD5; display: flex; flex-direction: column; }
.mode-node {
  flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center;
  cursor: pointer; opacity: 0.5; transition: 0.3s; border-bottom: 1px solid #CCC;
  color: var(--black);
}
.mode-node.active { 
  opacity: 1; background: var(--panel-white); 
  border-left: 8px solid var(--red);
  color: var(--red);
  box-shadow: inset 0 0 10px rgba(0,0,0,0.05);
}
.node-icon { font-size: 26px; margin-bottom: 4px; }
.node-label { font-size: 10px; font-weight: 800; font-family: var(--font-mono); }

/* 主编辑区 */
.editor-view { flex: 1; padding: 0; background: var(--panel-white); display: flex; flex-direction: column; overflow-y: auto; }

/* 配置侧边栏 */
.meta-sidebar { width: 280px; background: #EBE8E0; border-left: 2px solid var(--black); padding: 25px; display: flex; flex-direction: column; }
.meta-block { margin-bottom: 35px; }
.section-label { font-family: var(--font-mono); font-size: 10px; font-weight: 800; color: #777; display: block; margin-bottom: 12px; }

/* 下拉框视觉优化 */
.select-wrapper { position: relative; }
.select-wrapper::after {
  content: "▼"; position: absolute; right: 12px; top: 12px; font-size: 10px; pointer-events: none; color: var(--black);
}
.tactical-select { 
  width: 100%; border: 2px solid var(--black); padding: 10px; 
  background: var(--panel-white); appearance: none; cursor: pointer;
  font-weight: 700;
}

/* ✅ 重磅：发布按钮视觉升级 */
.publish-trigger {
  margin-top: auto; width: 100%; height: 64px;
  background: var(--black); color: var(--bg);
  border: none; position: relative; overflow: hidden; cursor: pointer;
  /* 战术切角造型 */
  clip-path: polygon(0 0, 100% 0, 100% 75%, 90% 100%, 0 100%);
  transition: 0.2s;
  box-shadow: 0 4px 0 #555;
}

.publish-trigger:hover:not(:disabled) {
  background: var(--red);
  transform: translateY(-2px);
  box-shadow: 0 6px 0 #8B1A1A;
}

.publish-trigger:active:not(:disabled) {
  transform: translateY(2px);
  box-shadow: 0 2px 0 #555;
}

.publish-trigger:disabled { 
  background: #666; cursor: not-allowed; clip-path: none; 
}

.btn-inner {
  display: flex; align-items: center; justify-content: center; gap: 12px;
  font-family: var(--font-title); font-size: 1.3rem; letter-spacing: 1px;
}
.btn-deco { color: var(--red); transition: 0.2s; }
.publish-trigger:hover .btn-deco { color: var(--white); }

.btn-scanline { 
  position: absolute; top: 0; left: 0; width: 100%; height: 4px; 
  background: rgba(255,255,255,0.15); animation: scan 2s linear infinite; 
}

/* 装饰性细节 */
@keyframes scan { 0% { top: -10%; } 100% { top: 110%; } }
.blink { color: var(--red); animation: blink 1.2s infinite; }
@keyframes blink { 50% { opacity: 0; } }

.system-status { margin-bottom: 25px; border-top: 2px dashed #CCC; padding-top: 20px; }
.status-row { display: flex; justify-content: space-between; font-family: var(--font-mono); font-size: 10px; margin-bottom: 6px; color: #555; }
.status-row .val { color: var(--red); font-weight: 800; }

.console-footer { 
  background: #D1CEC7; padding: 6px 18px; border-top: 2px solid var(--black); 
  font-family: var(--font-mono); font-size: 9px; color: #666; text-align: right; 
}

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }

/* 切换动画 */
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.fade-slide-enter-from { opacity: 0; transform: translateX(30px); }
.fade-slide-leave-to { opacity: 0; transform: translateX(-30px); }
</style>