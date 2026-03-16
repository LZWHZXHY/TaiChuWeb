<template>
  <Teleport to="body">
    <Transition name="oc-fade">
      <div v-if="modelValue" class="oc-overlay" @click.self="close">
        <div class="oc-terminal">
          <div class="term-head">
            <div class="term-title">
              <span class="blink">_</span> SELECT_OPERATIVE // 派遣指派
            </div>
            <button class="term-close" @click="close">ABORT_X</button>
          </div>
          
          <div class="term-body custom-scroll">
            <div v-if="loading" class="status-box">同步个人名册中...</div>
            <div v-else-if="ocList.length === 0" class="status-box empty">
              未检测到可用 OC 档案。<br/>
              请先前往 [个人档案库] 建立角色。
            </div>
            
            <div v-else class="oc-grid">
              <div 
                v-for="oc in ocList" 
                :key="oc.id" 
                class="oc-item" 
                :class="{ 'is-selected': selectedId === oc.id }"
                @click="selectedId = oc.id"
              >
                <div class="oc-frame">
                  <img :src="oc.avatar || '/default-avatar.png'" />
                  <div class="sel-tag">LOCKED</div>
                </div>
                <div class="oc-meta">
                  <div class="n">{{ oc.name }}</div>
                  <div class="i">REF.{{ padZero(oc.id) }}</div>
                </div>
              </div>
            </div>
          </div>

          <div class="term-foot">
            <div class="sel-info">
              <template v-if="selectedId">
                目标确认: <span class="h">#{{ selectedId }}</span>
              </template>
              <template v-else>等待选择指令...</template>
            </div>
            <button class="deploy-btn" :disabled="!selectedId" @click="confirm">
              DEPLOY // 确认指派
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({ modelValue: Boolean })
const emit = defineEmits(['update:modelValue', 'selected'])

const ocList = ref([])
const loading = ref(false)
const selectedId = ref(null)

const padZero = (n) => n < 10 ? `0${n}` : n

const fetchMyOcs = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/OcMaster/my-ocs')
    ocList.value = res.data.data || []
  } catch (e) {
    console.error("名册同步中断")
  } finally {
    loading.value = false
  }
}

watch(() => props.modelValue, (val) => {
  if (val) {
    selectedId.value = null
    fetchMyOcs()
  }
})

const close = () => emit('update:modelValue', false)
const confirm = () => {
  if (selectedId.value) {
    emit('selected', selectedId.value)
    close()
  }
}
</script>

<style scoped>
.oc-overlay { 
  position: fixed; inset: 0; background: rgba(0,0,0,0.9); 
  z-index: 4000; display: flex; justify-content: center; align-items: center; backdrop-filter: blur(15px); 
}
.oc-terminal { 
  width: 720px; background: #fff; border: 4px solid #111; 
  display: flex; flex-direction: column; box-shadow: 25px 25px 0 rgba(217, 35, 35, 0.2); 
}

/* 页眉 */
.term-head { background: #111; color: #fff; padding: 15px 25px; display: flex; justify-content: space-between; align-items: center; }
.term-title { font-family: monospace; font-weight: 900; font-size: 14px; letter-spacing: 1px; }
.blink { animation: blinker 1s linear infinite; color: #D92323; }
@keyframes blinker { 50% { opacity: 0; } }
.term-close { background: #D92323; color: #fff; border: none; padding: 6px 12px; font-weight: 900; cursor: pointer; font-size: 10px; }

/* 内容区 */
.term-body { padding: 40px; max-height: 480px; overflow-y: auto; background: #fdfdfd; }
.status-box { text-align: center; padding: 60px 0; font-weight: 900; color: #bbb; line-height: 2; font-size: 13px; }

.oc-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; }
.oc-item { background: #fff; border: 2px solid #eee; padding: 12px; cursor: pointer; transition: 0.15s; position: relative; }
.oc-item:hover { border-color: #111; transform: translateY(-4px); }
.oc-item.is-selected { border-color: #D92323; background: #fff8f8; box-shadow: 6px 6px 0 #D92323; }

.oc-frame { width: 100%; aspect-ratio: 1; border: 1px solid #eee; margin-bottom: 10px; overflow: hidden; position: relative; }
.oc-frame img { width: 100%; height: 100%; object-fit: cover; }
.sel-tag { 
  position: absolute; inset: 0; background: rgba(217, 35, 35, 0.8); color: #fff; 
  display: flex; justify-content: center; align-items: center; font-weight: 900; 
  font-size: 12px; opacity: 0; transition: 0.2s; 
}
.oc-item.is-selected .sel-tag { opacity: 1; }

.oc-meta { text-align: center; }
.oc-meta .n { font-weight: 900; font-size: 13px; margin-bottom: 2px; color: #111; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.oc-meta .i { font-size: 10px; color: #999; font-family: monospace; }

/* 页脚 */
.term-foot { padding: 25px 40px; border-top: 4px solid #111; display: flex; justify-content: space-between; align-items: center; background: #fff; }
.sel-info { font-family: monospace; font-weight: 900; font-size: 13px; color: #666; }
.sel-info .h { color: #D92323; text-decoration: underline; }

.deploy-btn { 
  background: #111; color: #fff; border: none; padding: 12px 30px; 
  font-weight: 900; cursor: pointer; transition: 0.2s; font-size: 13px;
}
.deploy-btn:hover:not(:disabled) { background: #D92323; }
.deploy-btn:disabled { background: #eee; color: #aaa; cursor: not-allowed; }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }

/* 动画 */
.oc-fade-enter-active, .oc-fade-leave-active { transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1); }
.oc-fade-enter-from, .oc-fade-leave-to { opacity: 0; transform: scale(0.95); }
</style>