<template>
  <transition name="modal-fade">
    <div class="modal-overlay" v-if="isVisible" @click.self="close">
      <div class="modal-card wide-modal">
        
        <div class="modal-header">
          <div class="header-left">
            <h3>{{ formTitle }}</h3>
            <span class="subtitle" v-if="!isFormVisible">共 {{ groupedContacts.length }} 位成员</span>
          </div>
          <div class="header-right">
            <button v-if="!isFormVisible" class="btn-primary" @click="startAddEmpty">+ 添加成员</button>
            <button v-else class="btn-text" @click="cancelForm">← 返回列表</button>
            <button class="btn-close" @click="close">✕</button>
          </div>
        </div>
        
        <div class="modal-body-wrapper">
          <transition name="fade-slide" mode="out-in">
            
            <div v-if="isFormVisible" class="form-container" key="form">
              <div class="form-grid">
                <div class="form-col">
                  <h4 class="col-title">👤 基础资料</h4>
                  <div class="form-group"><label>姓名</label><input type="text" v-model="addForm.name" placeholder="成员姓名" ref="nameInput"></div>
                  <div class="form-group"><label>职位头衔</label><input type="text" v-model="addForm.role" placeholder="如: 主策划"></div>
                  <div class="form-group">
                    <label>职能组别</label>
                    <select v-model="addForm.roleType">
                      <option value="admin">🟠 策划/管理组</option>
                      <option value="art">🟣 美术/设计组</option>
                      <option value="dev">⚫ 程序/技术组</option>
                      <option value="other">⚪ 综合/其他组</option>
                    </select>
                  </div>
                </div>
                <div class="form-col">
                  <h4 class="col-title">📞 状态与联系</h4>
                  <div class="row-2">
                    <div class="form-group"><label>当前状态</label><select v-model="addForm.status"><option :value="0">🟢 在线 / 工作中</option><option :value="1">🔴 忙碌 / 暂离</option></select></div>
                    <div class="form-group"><label>在线时段</label><input type="text" v-model="addForm.contactTime" placeholder="如: 10:00-19:00"></div>
                  </div>
                  <div class="divider"></div>
                  <div class="row-2">
                    <div class="form-group" style="flex: 0.4"><label>方式</label><select v-model="addForm.method"><option value="QQ">QQ</option><option value="微信">微信</option><option value="电话">电话</option><option value="Email">邮箱</option><option value="其他">其他</option></select></div>
                    <div class="form-group" style="flex: 0.6"><label>号码 / 账号</label><input type="text" v-model="addForm.number" placeholder="输入号码..." ref="numberInput"></div>
                  </div>
                </div>
              </div>
              <div class="form-footer">
                <button v-if="addForm.id" class="btn-danger" @click="deleteContact(addForm.id)">删除此条</button>
                <div class="spacer"></div>
                <button class="btn-save" @click="submitContact">{{ addForm.id ? '💾 保存修改' : '✨ 确认添加' }}</button>
              </div>
            </div>

            <div v-else class="list-container" key="list">
              <div v-if="loading" class="state-tip">数据加载中...</div>
              <div v-else-if="groupedContacts.length === 0" class="state-tip">暂无数据，点击右上角添加</div>
              <div v-else class="card-grid">
                <div v-for="(user, index) in groupedContacts" :key="index" class="user-card">
                  <div class="card-left">
                    <div class="avatar-big" :class="user.RoleType">
                      {{ user.Name ? user.Name[0] : 'U' }}
                      <div class="status-badge" :class="user.Status === 0 ? 'on' : 'off'">{{ user.Status === 0 ? '在线' : '忙碌' }}</div>
                    </div>
                  </div>
                  <div class="card-right">
                    <div class="card-header-row">
                      <div><div class="user-name">{{ user.Name }}</div><div class="user-role">{{ user.Role }}</div></div>
                      <button class="btn-icon-add" @click="startAddForUser(user)" title="添加联系方式">+</button>
                    </div>
                    <div class="time-tag" v-if="user.ContactTime">🕐 {{ user.ContactTime }}</div>
                    <div class="contact-scroll">
                      <div v-for="(contact, cIndex) in user.Contacts" :key="cIndex" class="contact-pill" @click="copyText(contact.Number)">
                        <span class="c-icon">{{ getMethodIcon(contact.Method) }}</span>
                        <span class="c-num">{{ contact.Number }}</span>
                        <span class="c-edit" @click.stop="startEdit(user, contact)">✎</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </transition>
        </div>

        <div class="modal-footer-bar">
          <span class="info-icon">ℹ️</span>
          <span>本通讯录仅用于团队内部协作，填写纯属自愿，并非强制要求。</span>
        </div>

      </div>
    </div>
  </transition>
</template>

<script>
import apiClient from '@/utils/api';

export default {
  name: 'ContactListModal',
  data() {
    return {
      isVisible: false,
      loading: false,
      isFormVisible: false,
      contactList: [],
      addForm: { id: null, name: '', role: '', roleType: 'admin', status: 0, contactTime: '', method: 'QQ', number: '' }
    };
  },
  computed: {
    formTitle() {
      if (!this.isFormVisible) return '📖 团队通讯录';
      return this.addForm.id ? '🛠️ 编辑信息' : '👤 添加成员';
    },
    groupedContacts() {
      const groups = {};
      this.contactList.forEach(item => {
        const key = item.Name;
        if (!groups[key]) {
          groups[key] = {
            Name: item.Name, Role: item.Role, RoleType: item.RoleType,
            Status: item.Status || 0, ContactTime: item.ContactTime,
            Contacts: []
          };
        }
        groups[key].Contacts.push({ Method: item.Method, Number: item.Number, Id: item.Id });
      });
      return Object.values(groups);
    }
  },
  methods: {
    open() { this.isVisible = true; this.isFormVisible = false; this.fetchContacts(); },
    close() { this.isVisible = false; },
    cancelForm() { this.isFormVisible = false; },
    startAddEmpty() { this.resetForm(); this.isFormVisible = true; this.$nextTick(() => { if(this.$refs.nameInput) this.$refs.nameInput.focus(); }); },
    startAddForUser(user) {
      this.resetForm();
      this.addForm.name = user.Name; this.addForm.role = user.Role; this.addForm.roleType = user.RoleType;
      this.addForm.status = user.Status; this.addForm.contactTime = user.ContactTime;
      this.isFormVisible = true; this.$nextTick(() => { if(this.$refs.numberInput) this.$refs.numberInput.focus(); });
    },
    startEdit(user, contact) {
      this.addForm = {
        id: contact.Id, name: user.Name, role: user.Role, roleType: user.RoleType,
        status: user.Status, contactTime: user.ContactTime, method: contact.Method, number: contact.Number
      };
      this.isFormVisible = true;
    },
    resetForm() { this.addForm = { id: null, name: '', role: '', roleType: 'admin', status: 0, contactTime: '', method: 'QQ', number: '' }; },
    async fetchContacts() {
      this.loading = true;
      try { const res = await apiClient.get('/Contact'); this.contactList = res.data; } catch (e) { console.error(e); } finally { this.loading = false; }
    },
    async submitContact() {
      if (!this.addForm.name || !this.addForm.number) { alert("请填写姓名和号码"); return; }
      const payload = {
        Name: this.addForm.name, Role: this.addForm.role, RoleType: this.addForm.roleType,
        Method: this.addForm.method, Number: this.addForm.number,
        Status: parseInt(this.addForm.status), ContactTime: this.addForm.contactTime
      };
      try {
        if (this.addForm.id) await apiClient.put(`/Contact/${this.addForm.id}`, payload);
        else await apiClient.post('/Contact', payload);
        this.isFormVisible = false; this.fetchContacts(); 
      } catch (e) { alert("操作失败: " + e.message); }
    },
    async deleteContact(id) {
      if(!confirm("确定要删除吗？")) return;
      try { await apiClient.delete(`/Contact/${id}`); this.isFormVisible = false; this.fetchContacts(); } catch (e) { alert("删除失败"); }
    },
    async copyText(text) { try { await navigator.clipboard.writeText(text); alert(`已复制: ${text}`); } catch (err) { console.error(err); } },
    getMethodIcon(method) {
      if (method.includes('QQ')) return '🐧';
      if (method.includes('微信')) return '💬';
      if (method.includes('电话')) return '📞';
      if (method.includes('Email') || method.includes('邮件')) return '✉️';
      if (method.includes('Github')) return '🐱';
      return '🔗';
    }
  }
};
</script>

<style scoped>
/* 动画 */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.3s ease; }
.fade-slide-enter-from { opacity: 0; transform: translateX(10px); }
.fade-slide-leave-to { opacity: 0; transform: translateX(-10px); }

/* 基础遮罩 */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.4); backdrop-filter: blur(5px);
  z-index: 9999; display: flex; justify-content: center; align-items: center;
}

/* === 核心：宽屏卡片 === */
.modal-card.wide-modal {
  width: 760px; max-width: 90vw; height: 580px; /* 稍微加高一点给Footer */
  background: #ffffff; border-radius: 16px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.15);
  display: flex; flex-direction: column; overflow: hidden;
}

/* 头部 */
.modal-header { padding: 16px 24px; border-bottom: 1px solid #f0f0f0; display: flex; justify-content: space-between; align-items: center; background: #fff; }
.header-left h3 { margin: 0; font-size: 1.2rem; color: #1e293b; display: inline-block; }
.subtitle { font-size: 0.85rem; color: #94a3b8; margin-left: 10px; font-weight: normal; }
.btn-primary { background: #2563eb; color: white; border: none; padding: 6px 16px; border-radius: 6px; font-weight: 500; cursor: pointer; transition: background 0.2s; }
.btn-primary:hover { background: #1d4ed8; }
.btn-text { background: none; border: none; color: #64748b; cursor: pointer; font-weight: 500; }
.btn-text:hover { color: #334155; }
.btn-close { background: none; border: none; font-size: 1.2rem; color: #94a3b8; cursor: pointer; margin-left: 10px; }
.btn-close:hover { color: #ef4444; }

/* 内容区 */
.modal-body-wrapper { flex: 1; overflow-y: auto; background: #f8fafc; padding: 24px; padding-bottom: 10px; }

/* === 页脚样式 === */
.modal-footer-bar {
  background: #f1f5f9;
  padding: 10px 24px;
  border-top: 1px solid #e2e8f0;
  font-size: 0.8rem;
  color: #64748b;
  display: flex;
  align-items: center;
  gap: 8px;
}
.info-icon { font-size: 1rem; }

/* 列表视图 */
.card-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
@media (max-width: 700px) { .card-grid { grid-template-columns: 1fr; } }
.user-card { background: white; border-radius: 12px; padding: 16px; border: 1px solid #e2e8f0; display: flex; gap: 16px; transition: all 0.2s ease; position: relative; }
.user-card:hover { transform: translateY(-3px); box-shadow: 0 10px 20px rgba(0,0,0,0.05); border-color: #cbd5e1; }
.card-left { flex-shrink: 0; display: flex; flex-direction: column; align-items: center; }
.avatar-big { width: 64px; height: 64px; border-radius: 50%; color: white; display: flex; align-items: center; justify-content: center; font-size: 1.5rem; font-weight: bold; position: relative; }
.avatar-big.admin { background: linear-gradient(135deg, #f6d365, #fda085); }
.avatar-big.art { background: linear-gradient(135deg, #a18cd1, #fbc2eb); }
.avatar-big.dev { background: linear-gradient(135deg, #434343, #000000); }
.avatar-big.other { background: #cbd5e1; }
.status-badge { position: absolute; bottom: -6px; padding: 2px 8px; border-radius: 10px; font-size: 0.6rem; color: white; border: 2px solid white; font-weight: bold; }
.status-badge.on { background: #10b981; }
.status-badge.off { background: #ef4444; }
.card-right { flex: 1; min-width: 0; }
.card-header-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 6px; }
.user-name { font-size: 1.1rem; font-weight: 700; color: #334155; }
.user-role { font-size: 0.8rem; color: #64748b; margin-top: 2px; }
.btn-icon-add { background: #f1f5f9; border: none; color: #2563eb; width: 28px; height: 28px; border-radius: 50%; font-size: 1.2rem; line-height: 1; cursor: pointer; }
.btn-icon-add:hover { background: #dbeafe; }
.time-tag { font-size: 0.75rem; color: #94a3b8; margin-bottom: 10px; background: #f8fafc; display: inline-block; padding: 2px 6px; border-radius: 4px; }
.contact-scroll { display: flex; flex-direction: column; gap: 6px; }
.contact-pill { display: flex; align-items: center; justify-content: space-between; background: #f1f5f9; padding: 6px 10px; border-radius: 6px; cursor: pointer; transition: background 0.2s; font-size: 0.85rem; }
.contact-pill:hover { background: #e2e8f0; }
.c-icon { margin-right: 6px; }
.c-num { font-family: monospace; color: #334155; flex: 1; }
.c-edit { color: #cbd5e1; font-size: 0.8rem; padding: 0 4px; }
.contact-pill:hover .c-edit { color: #64748b; }

/* 表单样式 */
.form-container { background: white; border-radius: 12px; padding: 24px; border: 1px solid #e2e8f0; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; }
.col-title { margin: 0 0 16px 0; color: #94a3b8; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 1px; }
.form-group { margin-bottom: 16px; }
.form-group label { display: block; margin-bottom: 6px; font-size: 0.9rem; color: #475569; font-weight: 500; }
.form-group input, .form-group select { width: 100%; padding: 10px; border: 1px solid #cbd5e1; border-radius: 6px; outline: none; box-sizing: border-box; transition: border 0.2s; }
.form-group input:focus, .form-group select:focus { border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37,99,235,0.1); }
.row-2 { display: flex; gap: 16px; }
.divider { height: 1px; background: #e2e8f0; margin: 20px 0; }
.form-footer { margin-top: 24px; display: flex; align-items: center; }
.spacer { flex: 1; }
.btn-save { background: #10b981; color: white; border: none; padding: 10px 24px; border-radius: 6px; font-weight: 600; cursor: pointer; box-shadow: 0 4px 6px rgba(16,185,129,0.2); }
.btn-save:hover { background: #059669; }
.btn-danger { background: #fee2e2; color: #ef4444; border: none; padding: 10px 16px; border-radius: 6px; font-weight: 600; cursor: pointer; }
.btn-danger:hover { background: #fecaca; }
.state-tip { text-align: center; color: #94a3b8; padding: 40px; font-size: 1rem; }
</style>