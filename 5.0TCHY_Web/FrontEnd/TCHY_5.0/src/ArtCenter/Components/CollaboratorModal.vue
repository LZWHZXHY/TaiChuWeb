<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="show" class="modal-overlay" @click="$emit('close')">
        <div class="modal-card" @click.stop>
          
          <div class="modal-header">
            <h3>>> TEAM_MANAGEMENT // 成员管理</h3>
            <button class="close-btn" @click="$emit('close')">×</button>
          </div>

          <div class="modal-body custom-scroll">
            
            <div class="invite-box">
              <label>INVITE_USER_ID:</label>
              <div class="input-row">
                <input 
                  v-model="targetUserId" 
                  type="number" 
                  placeholder="INPUT_UID..." 
                  class="cyber-input" 
                  @keyup.enter="inviteUser"
                />
                <button class="cyber-btn primary" @click="inviteUser" :disabled="inviting">
                  {{ inviting ? 'SENDING...' : 'INVITE' }}
                </button>
              </div>
            </div>

            <div class="divider"></div>

            <div class="member-list">
              <div class="list-title">CURRENT_MEMBERS ({{ members.length }})</div>
              
              <div v-if="loading" class="loading-txt">LOADING...</div>
              <div v-else-if="members.length === 0" class="empty-txt">NO_COLLABORATORS</div>
              
              <div v-else v-for="m in members" :key="m.id" class="member-row">
                <div class="member-info">
                  <span class="uid-tag">UID: {{ m.user_id }}</span>
                  <span class="role-tag">EDITOR</span>
                  <span class="join-time">{{ formatDate(m.createTime) }}</span>
                </div>
                <button class="kick-btn" @click="removeUser(m.user_id)">REMOVE</button>
              </div>
            </div>

          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'

const props = defineProps({
  show: Boolean,
  ipId: { type: [Number, String], required: true }
})

const emit = defineEmits(['close'])

const members = ref([])
const loading = ref(false)
const inviting = ref(false)
const targetUserId = ref('')

const formatDate = (t) => new Date(t).toLocaleDateString()

// 获取列表
const fetchMembers = async () => {
  if (!props.ipId) return
  loading.value = true
  try {
    const res = await apiClient.get(`/Collaborator/${props.ipId}`)
    members.value = res.data
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

// 邀请
const inviteUser = async () => {
  if (!targetUserId.value) return alert("请输入用户ID")
  
  inviting.value = true
  try {
    await apiClient.post('/Collaborator', {
      IpId: parseInt(props.ipId),
      TargetUserId: parseInt(targetUserId.value)
    })
    alert("邀请成功！")
    targetUserId.value = ''
    fetchMembers()
  } catch (e) {
    alert("邀请失败: " + (e.response?.data || e.message))
  } finally {
    inviting.value = false
  }
}

// 移除
const removeUser = async (userId) => {
  if(!confirm(`确定移除用户 ${userId} 的权限吗？`)) return
  try {
    await apiClient.delete(`/Collaborator/${props.ipId}/${userId}`)
    fetchMembers()
  } catch (e) {
    alert("移除失败: " + (e.response?.data || e.message))
  }
}

// 监听打开时刷新
watch(() => props.show, (val) => {
  if (val) fetchMembers()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.7); 
  display: flex; justify-content: center; align-items: center; z-index: 3000;
  backdrop-filter: blur(2px);
}

.modal-card {
  width: 500px; max-width: 90vw; background: #F4F1EA; 
  border: 3px solid #111; box-shadow: 10px 10px 0 rgba(0,0,0,0.5);
  font-family: 'JetBrains Mono', monospace; color: #111;
  display: flex; flex-direction: column;
}

.modal-header {
  background: #111; color: #fff; padding: 12px 20px;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-header h3 { margin: 0; font-family: 'Anton', sans-serif; font-size: 1.2rem; letter-spacing: 1px; }
.close-btn { background: none; border: none; color: #fff; font-size: 1.5rem; cursor: pointer; line-height: 1; }
.close-btn:hover { color: #D92323; }

.modal-body { padding: 25px; }

/* 邀请区 */
.invite-box label { font-weight: bold; font-size: 0.8rem; display: block; margin-bottom: 8px; }
.input-row { display: flex; gap: 10px; }
.cyber-input { flex: 1; padding: 8px; border: 2px solid #ccc; font-family: inherit; outline: none; transition: 0.2s; }
.cyber-input:focus { border-color: #111; background: #fffef0; }

.cyber-btn { border: 2px solid #111; padding: 6px 15px; font-weight: bold; cursor: pointer; transition: 0.2s; font-family: 'Anton'; letter-spacing: 1px; }
.cyber-btn.primary { background: #111; color: #fff; }
.cyber-btn.primary:hover { background: #D92323; border-color: #D92323; }
.cyber-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.divider { border-bottom: 2px dashed #ccc; margin: 25px 0; }

/* 列表区 */
.list-title { font-weight: bold; margin-bottom: 15px; font-size: 0.9rem; color: #666; }
.member-row { display: flex; justify-content: space-between; align-items: center; background: #fff; border: 1px solid #ddd; padding: 10px; margin-bottom: 8px; }
.member-info { display: flex; gap: 10px; align-items: center; }
.uid-tag { font-weight: bold; }
.role-tag { background: #e6f7ff; color: #1890ff; padding: 2px 6px; font-size: 0.7rem; border-radius: 2px; }
.join-time { color: #999; font-size: 0.75rem; }

.kick-btn { background: transparent; border: 1px solid #ccc; color: #666; cursor: pointer; padding: 2px 8px; font-size: 0.7rem; font-weight: bold; }
.kick-btn:hover { border-color: #D92323; color: #D92323; background: #fff0f0; }

.loading-txt, .empty-txt { text-align: center; color: #999; padding: 20px; font-style: italic; }

.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>