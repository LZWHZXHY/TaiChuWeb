<template>
  <div class="chailianhe-platform">
    <!-- 头部区域 -->
    <div class="header">
      <h1 class="main-title">
        <i class="fas fa-palette"></i>
        这里是太虚绘院 · 柴圈联合平台！
      </h1>
      <p class="welcome-desc">
        欢迎在此平台发起或申请各类联合、接力、企划等社区活动！在这里，艺术家们可以自由交流创意，共同创作精彩作品。
      </p>
    </div>

    <div class="content-container">
      <!-- 侧边栏区域 -->
      <div class="sidebar">
        <!-- 搜索面板 -->
        <div class="searchPanel">
          <h3 class="panel-title"><i class="fas fa-search"></i> 活动搜索</h3>
          <label>
            名字：
            <input v-model="searchName" type="text" placeholder="请输入名称" />
          </label>
          <label>
            类型：
            <select v-model="searchType" class="glass-select">
              <option value="">全部</option>
              <option value="1">联合</option>
              <option value="2">接力</option>
              <option value="3">锦标赛</option>
              <option value="4">企划</option>
            </select>
          </label>
          <label>
            发起人：
            <input v-model="searchHost" type="text" placeholder="请输入主办方" />
          </label>
          <button class="search-btn" @click="clearSearch">
            <i class="fas fa-undo"></i> 重置搜索
          </button>
        </div>

        <!-- 功能面板 -->
        <div class="functionPanel">
          <h3 class="panel-title"><i class="fas fa-tools"></i> 平台功能</h3>
          <button class="func-btn" @click="showApply = true">
            <i class="fas fa-plus-circle"></i> 申请活动
          </button>
          <button class="func-btn">
            <i class="fas fa-comment-alt"></i> 反馈建议
          </button>
          <button class="func-btn">
            <i class="fas fa-flag"></i> 举报问题
          </button>
        </div>
      </div>

      <!-- 主内容区域 -->
      <div class="main-content">
        <div class="content-header">
          <h2 class="content-title">当前活动列表</h2>
          <div class="status-info">
            <span v-if="loading" class="status-text">
              <i class="fas fa-spinner fa-spin"></i> 正在加载数据...
            </span>
            <span v-else-if="error" class="error-text">
              <i class="fas fa-exclamation-triangle"></i> {{ error }}
            </span>
          </div>
        </div>

        <div>
          <div v-if="filteredChailianhes.length === 0 && !loading" class="status-text">
            <i class="fas fa-inbox"></i> 暂无活动数据
          </div>
          <ul class="chailianhe-list">
            <li
              v-for="item in filteredChailianhes"
              :key="item.id"
              class="chailianhe-item"
            >
              <div class="item-header">
                <span class="item-title">{{ item.name }}</span>
                <span :class="'type-badge type-'+item.type">{{ typeText(item.type) }}</span>
              </div>
              <ul class="item-details">
                <li><strong>主办：</strong>{{ item.host }}</li>
                <li><strong>开始：</strong>{{ formatDate(item.startdate) }}</li>
                <li><strong>结束：</strong>{{ item.enddate ? formatDate(item.enddate) : '进行中' }}</li>
                <li><strong>QQ群: </strong>{{ item.QQgroup }}</li>
                <li><strong>要求：</strong>{{ item.require }}</li>
                <li><strong>描述：</strong>{{ item.desc }}</li>
              </ul>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <!-- 申请弹窗 -->
    <div v-if="showApply" class="modal-overlay" @click.self="showApply = false">
      <div class="modal">
        <h2>申请联合活动</h2>
        <form @submit.prevent="submitApply">
          <div class="form-row">
            <label>活动名称：</label>
            <input v-model="applyForm.name" required maxlength="30" placeholder="请输入活动名称" />
          </div>
          <div class="form-row">
            <label>主办方：</label>
            <input v-model="applyForm.host" required maxlength="30" placeholder="请输入主办方" />
          </div>
          <div class="form-row">
            <label>类型：</label>
            <select v-model="applyForm.type" required class="glass-select">
              <option value="">请选择</option>
              <option value="1">联合</option>
              <option value="2">接力</option>
              <option value="3">锦标赛</option>
              <option value="4">企划</option>
            </select>
          </div>
          <div class="form-row">
            <label>开始时间：</label>
            <input v-model="applyForm.startdate" type="date" required />
          </div>
          <div class="form-row">
            <label>结束时间：</label>
            <input v-model="applyForm.enddate" type="date" />
          </div>
          <div class="form-row">
            <label>QQ群：</label>
            <input v-model="applyForm.QQgroup" maxlength="20" placeholder="请输入QQ群" />
          </div>
          <div class="form-row">
            <label>要求：</label>
            <input v-model="applyForm.require" maxlength="50" placeholder="请输入要求" />
          </div>
          <div class="form-row">
            <label>描述：</label>
            <textarea v-model="applyForm.desc" maxlength="300" placeholder="请简要描述活动"></textarea>
          </div>
          <div class="modal-actions">
            <button type="submit" class="modal-btn modal-btn-primary">提交申请</button>
            <button type="button" class="modal-btn" @click="showApply = false">取消</button>
          </div>
          <div v-if="applyMsg" class="modal-msg">{{ applyMsg }}</div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ChailianhePlatform',
  data() {
    return {
      chailianhes: [
        {
          id: 1,
          name: "秋季联合绘画活动",
          host: "太虚绘院",
          type: 1,
          startdate: "2023-09-01",
          enddate: "2023-10-15",
          QQgroup: "123456789",
          require: "主题：秋日幻想",
          desc: "描绘你心中的秋日景色，风格不限",
          verify: 1
        },
        {
          id: 2,
          name: "角色设计接力赛",
          host: "柴圈艺术社",
          type: 2,
          startdate: "2023-09-10",
          enddate: "",
          QQgroup: "987654321",
          require: "每人设计一个角色部位",
          desc: "多人接力完成角色设计，每周一个主题",
          verify: 1
        },
        {
          id: 3,
          name: "场景绘制锦标赛",
          host: "幻想艺术联盟",
          type: 3,
          startdate: "2023-10-01",
          enddate: "2023-10-31",
          QQgroup: "456789123",
          require: "限时3小时完成",
          desc: "每月一次的快速场景绘制比赛",
          verify: 1
        }
      ],
      loading: false,
      error: '',
      searchName: '',
      searchType: '',
      searchHost: '',
      showApply: false,
      applyMsg: '',
      applyForm: {
        name: '',
        host: '',
        type: '',
        startdate: '',
        enddate: '',
        QQgroup: '',
        require: '',
        desc: ''
      }
    };
  },
  mounted() {
    this.fetchChailianhes();
  },
  computed: {
    filteredChailianhes() {
      return this.chailianhes.filter(item => {
        if (item.verify !== 1) return false;
        if (this.searchName && !item.name.includes(this.searchName)) return false;
        if (this.searchType && String(item.type) !== this.searchType) return false;
        if (this.searchHost && !item.host.includes(this.searchHost)) return false;
        return true;
      });
    }
  },
  methods: {
    async fetchChailianhes() {
      this.loading = true;
      this.error = '';
      try {
        // 使用环境变量动态设置API地址
        const apiBase = window.location.hostname === 'localhost' 
          ? 'https://localhost:44321/api/Chailianhe' 
          : 'https://bianyuzhou.com/api/Chailianhe';
        
        const response = await axios.get(apiBase);
        this.chailianhes = response.data;
      } catch (err) {
        console.error('获取数据错误:', err);
        this.error = '加载失败，请检查后端接口是否启动。';
      } finally {
        this.loading = false;
      }
    },
    formatDate(date) {
      if (!date) return '';
      const d = new Date(date);
      return d.toLocaleDateString();
    },
    typeText(type) {
      switch (Number(type)) {
        case 1: return '联合';
        case 2: return '接力';
        case 3: return '锦标赛';
        case 4: return '企划';
        default: return '未知';
      }
    },
    clearSearch() {
      this.searchName = '';
      this.searchType = '';
      this.searchHost = '';
    },
    async submitApply() {
      this.applyMsg = '';
      // 简单校验
      if (!this.applyForm.name || !this.applyForm.host || !this.applyForm.type || !this.applyForm.startdate) {
        this.applyMsg = '请完整填写必填项';
        return;
      }
      
      try {
        // 使用环境变量动态设置API地址
        const apiBase = window.location.hostname === 'localhost' 
          ? 'https://localhost:44321/api/Chailianhe/ApplyLianHe' 
          : 'https://bianyuzhou.com/api/Chailianhe/ApplyLianHe';
        
        // 准备提交数据
        const payload = {
          name: this.applyForm.name,
          host: this.applyForm.host,
          type: Number(this.applyForm.type),
          startdate: this.formatDateForAPI(this.applyForm.startdate),
          enddate: this.applyForm.enddate ? this.formatDateForAPI(this.applyForm.enddate) : null,
          QQgroup: this.applyForm.QQgroup || '',
          require: this.applyForm.require || '',
          desc: this.applyForm.desc || '',
          verify: 0,
          report: 0 // 后端要求的必填字段
        };
        
        // 发送请求
        const response = await axios.post(apiBase, payload, {
          headers: {
            'Content-Type': 'application/json'
          }
        });
        
        this.applyMsg = '申请提交成功，等待审核！';
        setTimeout(() => {
          this.showApply = false;
          // 清空表单
          this.applyForm = {
            name: '',
            host: '',
            type: '',
            startdate: '',
            enddate: '',
            QQgroup: '',
            require: '',
            desc: ''
          };
          this.applyMsg = '';
          this.fetchChailianhes();
        }, 1200);
      } catch (e) {
        console.error('提交错误:', e);
        
        if (e.response) {
          // 服务器返回了错误响应
          if (e.response.status === 400) {
            // 400错误通常是数据验证失败
            this.applyMsg = `提交失败: ${e.response.data?.error || '数据格式不正确'}`;
          } else {
            this.applyMsg = `服务器错误: ${e.response.status}`;
          }
        } else if (e.request) {
          // 请求已发送但无响应
          this.applyMsg = '服务器无响应，请检查网络连接';
        } else {
          // 其他错误
          this.applyMsg = `请求错误: ${e.message}`;
        }
      }
    },
    // 日期格式化方法
    formatDateForAPI(dateString) {
      if (!dateString) return null;
      
      // 处理各种可能的日期格式
      const date = new Date(dateString);
      
      // 返回ISO格式（带时区信息）
      return date.toISOString();
    }
  }
};
</script>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  -webkit-tap-highlight-color: transparent;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.chailianhe-platform {
  min-height: 100vh;
  padding: 20px;
  background: linear-gradient(135deg, #1a2a6c3b, #b21f1f31, #fdbb2d23);
  background-size: 400% 400%;
  animation: gradientBG 15s ease infinite;
  border-radius: 12px;
}

@keyframes gradientBG {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

.content-container {
  max-width: 1400px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 25px;
}

/* 头部样式 - 增强毛玻璃效果 */
.header {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 25px 35px;
  margin-bottom: 25px;
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.15);
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.main-title {
  font-size: 2.5rem;
  font-weight: 800;
  color: #fff;
  letter-spacing: 1.5px;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
  display: flex;
  align-items: center;
  gap: 15px;
}

.main-title i {
  font-size: 2.2rem;
}

.welcome-desc {
  font-size: 1.15rem;
  color: rgba(255, 255, 255, 0.95);
  letter-spacing: 0.5px;
  line-height: 1.6;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
}

/* 侧边栏样式 - 增强毛玻璃效果 */
.sidebar {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 25px;
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.15);
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.panel-title {
  font-size: 1.4rem;
  font-weight: 600;
  color: #fff;
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  gap: 10px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
}

.panel-title i {
  font-size: 1.2rem;
}

.searchPanel {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.searchPanel label {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.95);
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.searchPanel input,
.searchPanel select {
  font-size: 1rem;
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 10px;
  padding: 10px 15px;
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  outline: none;
  transition: all 0.3s ease;
}

/* 修复下拉菜单显示问题 */
.glass-select {
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
  background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='white'%3e%3cpath d='M7 10l5 5 5-5z'/%3e%3c/svg%3e");
  background-repeat: no-repeat;
  background-position: right 15px center;
  background-size: 16px;
}

/* 下拉菜单选项样式 */
.glass-select option {
  background: rgba(30, 30, 40, 0.95);
  color: white;
}

.searchPanel input::placeholder {
  color: rgba(255, 255, 255, 0.7);
}

.searchPanel input:focus,
.searchPanel select:focus {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.8);
  box-shadow: 0 0 0 3px rgba(255, 255, 255, 0.2);
}

.search-btn {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border: none;
  border-radius: 10px;
  padding: 12px 0;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-top: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.search-btn:hover {
  background: rgba(255, 255, 255, 0.25);
  transform: translateY(-2px);
}

.functionPanel {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.func-btn {
  width: 100%;
  padding: 14px 0;
  background: rgba(255, 255, 255, 0.1);
  border: none;
  border-radius: 10px;
  color: #fff;
  font-size: 1.1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.func-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-3px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

/* 主内容区样式 - 增强毛玻璃效果 */
.main-content {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 25px;
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.15);
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.content-title {
  font-size: 1.6rem;
  font-weight: 600;
  color: #fff;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
}

.status-text {
  text-align: center;
  color: rgba(255, 255, 255, 0.95);
  font-size: 1.1rem;
  padding: 20px;
  font-weight: 500;
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.error-text {
  text-align: center;
  color: #ffcccc;
  font-size: 1.1rem;
  padding: 20px;
  font-weight: 600;
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.3);
}

.chailianhe-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 25px;
}

/* 活动卡片样式 - 增强毛玻璃效果 */
.chailianhe-item {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 20px;
  backdrop-filter: blur(15px);
  -webkit-backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.chailianhe-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15);
  background: rgba(255, 255, 255, 0.15);
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 15px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.item-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #fff;
  letter-spacing: 0.5px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
}

.type-badge {
  display: inline-block;
  padding: 5px 15px;
  border-radius: 20px;
  font-size: 0.9rem;
  color: #fff;
  font-weight: 500;
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.type-1 { background: linear-gradient(45deg, #4c9adb, #30567b); }
.type-2 { background: linear-gradient(45deg, #7cb518, #5a8e1a); }
.type-3 { background: linear-gradient(45deg, #e07a5f, #c45d42); }
.type-4 { background: linear-gradient(45deg, #7f5af0, #6a4bc9); }

.item-details {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.item-details li {
  display: flex;
  gap: 8px;
  font-size: 0.98rem;
  color: rgba(255, 255, 255, 0.95);
  text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
}

.item-details strong {
  font-weight: 600;
  color: #fff;
  min-width: 60px;
}

/* 申请弹窗样式 - 增强毛玻璃效果 */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(8px);
}

.modal {
  background: rgba(255, 255, 255, 0.92);
  border-radius: 20px;
  box-shadow: 0 15px 50px rgba(0, 0, 0, 0.25);
  padding: 30px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
}

.modal h2 {
  font-size: 1.6rem;
  color: #30567b;
  font-weight: 700;
  text-align: center;
  margin-bottom: 25px;
}

.form-row {
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
  gap: 8px;
}

.form-row label {
  font-weight: 500;
  color: #30567b;
}

.form-row input,
.form-row select,
.form-row textarea {
  padding: 12px 15px;
  border: 1px solid #b6ccf2;
  border-radius: 10px;
  font-size: 1rem;
  background: rgba(248, 250, 252, 0.8);
  transition: border 0.2s;
}

.form-row textarea {
  min-height: 100px;
  resize: vertical;
}

.modal-actions {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-top: 20px;
}

.modal-btn {
  padding: 12px 30px;
  border: none;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.modal-btn-primary {
  background: linear-gradient(45deg, #4c9adb, #30567b);
  color: #fff;
}

.modal-btn {
  background: rgba(224, 231, 255, 0.8);
  color: #30567b;
}

.modal-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.modal-msg {
  margin-top: 15px;
  color: #e07a5f;
  text-align: center;
  font-size: 1.05rem;
  font-weight: 500;
}

/* 响应式设计 */
@media (max-width: 992px) {
  .content-container {
    grid-template-columns: 1fr;
  }
  
  .chailianhe-list {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
}

@media (max-width: 768px) {
  .header {
    padding: 20px;
  }
  
  .main-title {
    font-size: 2rem;
  }
  
  .sidebar, .main-content {
    padding: 20px;
  }
  
  .chailianhe-list {
    grid-template-columns: 1fr;
  }
  
  .modal {
    padding: 20px;
  }
}
</style>