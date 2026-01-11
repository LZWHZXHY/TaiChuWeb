<template>
  <div class="creator-info-panel">
    <div class="panel-header">
      <h3>🎨 创作者中心</h3>
      <span class="subtitle">太初寰宇 / 资源与协作</span>
    </div>

    <div class="panel-content">
      <div class="info-section">
        <h4>📂 共享资源 (网盘)</h4>
        <div class="link-grid">
          <a href="https://taichuhuanyu.us5.ug.link/desktop/" target="_blank" class="resource-card">
            <span class="icon">☁️</span>
            <span class="text">太初寰宇网盘地址</span>
            <span class="tip"></span>
          </a>
        </div>
      </div>

      <hr class="divider" />

      <div class="info-section">
        <h4>🛠️ 创作工具箱</h4>
        <ul class="tool-list">
          <li>
            <span class="label">📅 项目进度表:</span>
            <a href="#" @click.prevent="$emit('open-project-manager')">
                项目进度表
            </a>
          </li>
          <li>
            <span class="label">制作成员联系方式:</span>
            <a href="#" @click.prevent="showContacts">
                联系方式列表
            </a>
          </li>
        </ul>
      </div>

      <hr class="divider" />

      <div class="info-section row-layout">
        <div class="notice-box">
          <h4>📢 近期公告</h4>
          <p>该页面还在升级和调试，若需要使用需要在群内申请使用</p>
          <p>另外现任何人不得使用世界观设定提交功能，谢谢配合</p>
        </div>
      </div>
    </div>

    <ContactListModal ref="contactModal" />

  </div>
</template>

<script>
// 补全点 3：引入我们刚才创建的 ContactListModal 组件
// 请确保文件名和路径正确，建议统一使用英文文件名 ContactListModal.vue
import ContactListModal from './联系人列表.vue'; 

export default {
  name: 'CreatorInfo',
  components: {
    ContactListModal // 注册组件
  },
  data() {
    return {
      // 父组件不需要存联系人数据了，数据全交给子组件管理
    };
  },
  methods: {
    // 补全点 4：定义打开弹窗的方法
    showContacts() {
      // 通过 ref 找到子组件，并调用它的 open 方法
      this.$refs.contactModal.open();
    }
  }
};
</script>

<style scoped>
.creator-info-panel {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem; 
  box-shadow: inset 0 0 10px rgba(0,0,0,0.02);
}

.panel-header {
  margin-bottom: 1.5rem;
  border-bottom: 2px solid #e0e0e0;
  padding-bottom: 0.5rem;
  display: flex;
  align-items: baseline;
  gap: 10px;
}

.panel-header h3 {
  margin: 0;
  color: #2c3e50;
}

.subtitle {
  font-size: 0.85rem;
  color: #7f8c8d;
}

.info-section h4 {
  margin-top: 0;
  margin-bottom: 1rem;
  font-size: 1rem;
  color: #34495e;
  border-left: 4px solid #3498db;
  padding-left: 8px;
}

/* 资源链接卡片样式 */
.link-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 12px;
}

.resource-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: white;
  padding: 12px;
  border-radius: 8px;
  text-decoration: none;
  color: #333;
  border: 1px solid #ddd;
  transition: all 0.2s;
}

.resource-card:hover {
  border-color: #3498db;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(52, 152, 219, 0.15);
}

.resource-card .icon {
  font-size: 1.5rem;
  margin-bottom: 4px;
}

.resource-card .text {
  font-weight: bold;
  font-size: 0.9rem;
}

.resource-card .tip {
  font-size: 0.75rem;
  color: #999;
  margin-top: 2px;
}

/* 工具列表样式 */
.tool-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.tool-list li {
  background: white;
  padding: 6px 12px;
  border-radius: 20px;
  border: 1px solid #eee;
  font-size: 0.9rem;
}

.tool-list a {
  color: #3498db;
  font-weight: bold;
  text-decoration: none;
}

/* 底部公告与联系人样式 */
.row-layout {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.notice-box, .contact-box {
  flex: 1;
  background: #fff3cd; 
  padding: 12px;
  border-radius: 8px;
  min-width: 250px;
}

.contact-box {
  background: #e8f4fd; 
}

.notice-box p, .contact-box p {
  margin: 0;
  font-size: 0.9rem;
  line-height: 1.5;
  color: #555;
}

.divider {
  border: 0;
  border-top: 1px dashed #ccc;
  margin: 1.5rem 0;
}
</style>