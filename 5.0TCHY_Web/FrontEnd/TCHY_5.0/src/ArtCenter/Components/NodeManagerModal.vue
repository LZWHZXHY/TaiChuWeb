<template>
  <Teleport to="body">
    <Transition name="modal-scale">
      <div v-if="show" class="modal-overlay" @click="$emit('close')">
        <div class="modal-card" @click.stop>
          <div class="modal-header">
            <div class="header-title-group">
              <span class="deco-box">■</span>
              <h3>>> {{ mode === 'create' ? 'REGISTER_NEW_NODE' : 'REASSIGN_PARENT' }}</h3>
            </div>
            <button class="close-btn" @click="$emit('close')">CLOSE [X]</button>
          </div>

          <div class="modal-body custom-scroll">
            <div v-if="mode === 'create'" class="form-group-heavy">
              <label class="input-label-tech">> IDENTIFIER_NAME / 节点名称</label>
              <input v-model="formData.name" class="cyber-input-heavy" placeholder="INPUT_NAME..." />
            </div>

            <div v-if="mode === 'create'" class="form-group-heavy">
              <label class="input-label-tech">> NODE_CLASSIFICATION / 节点类型</label>
              <input 
                v-model="formData.type" 
                class="cyber-input-heavy" 
                placeholder="例如：星系、主要角色、武器..." 
                list="existing-types" 
              />
              <datalist id="existing-types">
                <option v-for="t in existingTypes" :key="t" :value="t" />
              </datalist>
            </div>

            <div class="form-group-heavy">
              <label class="input-label-tech">> PARENT_SECTOR_ASSIGNMENT / 所属层级</label>
              <select v-model="formData.parentId" class="cyber-select-heavy">
                <option :value="null">-- ROOT_SECTOR (根目录) --</option>
                <option 
                  v-for="node in nodeList" 
                  :key="node.id" 
                  :value="node.id" 
                  :disabled="node.id === initialData?.id"
                >
                  {{ getLevelDots(node.level) }} {{ node.name }}
                </option>
              </select>
              <div class="tech-hint">// WARNING: 修改层级将导致神经连接重定向。</div>
            </div>

            <div class="form-actions-heavy">
              <button class="cyber-btn-rect ghost" @click="$emit('close')">退出</button>
              <button class="cyber-btn-rect primary-red" @click="handleSubmit" :disabled="submitting">
                {{ submitting ? 'SYNCING...' : 'CONFIRM_EXECUTION' }}
              </button>
            </div>
          </div>
          
          <div class="modal-footer-strip"></div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, reactive, watch, computed } from 'vue'

const props = defineProps({
  show: Boolean,
  mode: String,
  nodeList: Array,
  initialData: Object
})

const emit = defineEmits(['close', 'submit'])
const submitting = ref(false)

const formData = reactive({
  id: null,
  name: '',
  type: '角色',
  parentId: null
})

const existingTypes = computed(() => {
  const types = props.nodeList.map(n => n.type)
  return [...new Set(types)].filter(t => t && t !== '待定')
})

const getLevelDots = (level) => "│ ".repeat(level || 0) + (level > 0 ? "├ " : "")

watch(() => props.show, (val) => {
  if (val) {
    formData.id = props.initialData?.id || null
    formData.name = props.initialData?.name || ''
    formData.type = props.initialData?.type || '角色'
    formData.parentId = props.initialData?.parentId || null
  }
})

const handleSubmit = () => {
  submitting.value = true
  emit('submit', { ...formData }, () => { submitting.value = false })
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.modal-card {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
}

/* 遮罩层 */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0, 0, 0, 0.85);
  display: flex; justify-content: center; align-items: center;
  z-index: 9999;
  backdrop-filter: blur(4px);
}

/* 模态框主体 */
.modal-card {
  width: 500px;
  max-width: 95vw;
  background: var(--off-white);
  border: 3px solid var(--black);
  box-shadow: 15px 15px 0 rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  position: relative;
}

/* 头部 */
.modal-header {
  background: var(--black);
  color: #fff;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 4px solid var(--red);
}
.header-title-group { display: flex; align-items: center; gap: 10px; }
.deco-box { color: var(--red); }
.modal-header h3 {
  margin: 0;
  font-family: var(--heading);
  font-size: 1.2rem;
  letter-spacing: 1px;
}
.close-btn {
  background: transparent;
  border: 1px solid #444;
  color: #888;
  font-family: var(--mono);
  font-size: 0.7rem;
  padding: 4px 8px;
  cursor: pointer;
}
.close-btn:hover { color: #fff; border-color: #fff; }

/* 表单主体 */
.modal-body {
  padding: 30px;
}

.form-group-heavy {
  margin-bottom: 25px;
}

.input-label-tech {
  display: block;
  font-family: var(--mono);
  font-weight: 800;
  font-size: 0.75rem;
  margin-bottom: 10px;
  color: var(--black);
}

/* 输入框与下拉框 */
.cyber-input-heavy, .cyber-select-heavy {
  width: 100%;
  background: #fff;
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--mono);
  font-weight: 700;
  outline: none;
  box-sizing: border-box;
  transition: 0.2s;
  color:#111111;
}

.cyber-input-heavy:focus, .cyber-select-heavy:focus {
  background: #fffef0;
  border-color: var(--red);
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
}

.tech-hint {
  font-family: var(--mono);
  font-size: 0.65rem;
  color: var(--red);
  margin-top: 8px;
  opacity: 0.8;
}

/* 按钮区域 */
.form-actions-heavy {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 10px;
}

.cyber-btn-rect {
  border: none;
  padding: 10px 24px;
  font-family: var(--heading);
  font-size: 1rem;
  letter-spacing: 1px;
  cursor: pointer;
  transition: 0.2s;
}

.primary-red {
  background: var(--red);
  color: #fff;
}
.primary-red:hover {
  transform: translate(-3px, -3px);
  box-shadow: 6px 6px 0 var(--black);
}

.ghost {
  background: transparent;
  border: 2px solid var(--black);
  color: var(--black);
}
.ghost:hover {
  background: var(--black);
  color: #fff;
}

.modal-footer-strip {
  height: 8px;
  background: var(--black);
  margin-top: auto;
}

/* 动画效果 */
.modal-scale-enter-active, .modal-scale-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-scale-enter-from, .modal-scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>