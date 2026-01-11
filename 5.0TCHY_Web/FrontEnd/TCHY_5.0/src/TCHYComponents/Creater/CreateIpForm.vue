<template>

  <div class="ip-create-card">

    <div class="card-header">

      <h2>🌍 创建新的世界观</h2>

      <p>开启一个新的宇宙纪元</p>

    </div>



    <form @submit.prevent="handleCreate" class="ip-form">

      <div class="form-group">

        <label for="name">世界观名称 <span class="required">*</span></label>

        <input

          type="text"

          id="name"

          v-model="formData.name"

          placeholder="例如：彼岸宇宙"

          required

        />

      </div>



      <div class="form-group">

        <label for="tagline">标语 / Slogan</label>

        <input

          type="text"

          id="tagline"

          v-model="formData.tagline"

          placeholder="例如：探索未知的边界"

        />

      </div>



      <div class="form-group">

        <label for="summary">世界观简介</label>

        <textarea

          id="summary"

          v-model="formData.summary"

          rows="4"

          placeholder="简要描述这个世界的主题、风格..."

        ></textarea>

      </div>



      <div class="form-group">

        <label for="coverUrl">封面图片链接</label>

        <input

          type="text"

          id="coverUrl"

          v-model="formData.coverUrl"

          placeholder="http://..."

        />

      </div>



      <div v-if="errorMessage" class="error-msg">

        ⚠️ {{ errorMessage }}

      </div>



      <div class="form-actions">

        <button type="submit" class="submit-btn" :disabled="isLoading">

          <span v-if="isLoading">创建中...</span>

          <span v-else>立即创建</span>

        </button>

      </div>

    </form>

  </div>

</template>



<script>

import apiClient from '@/utils/api'; // 假设你已经封装了 Axios 实例



export default {

  name: 'CreateIpForm',

  data() {

    return {

      // 表单数据模型

      formData: {

        name: '',

        tagline: '',

        summary: '',

        coverUrl: ''

      },

      isLoading: false,   // 加载状态

      errorMessage: ''    // 错误信息

    };

  },

  methods: {

    async handleCreate() {

      // 1. 重置状态

      this.isLoading = true;

      this.errorMessage = '';



      try {

        // 2. 发送请求到后端

        // 对应后端 POST /api/IP

        // 注意：后端 DTO 是 PascalCase (Name)，但 JSON 默认会自动处理 camelCase (name)，

        // 如果后端没有特殊配置，通常传小写首字母即可。

        const response = await apiClient.post('/IP', this.formData);



        // 3. 成功处理

        console.log('创建成功:', response.data);

        alert(`成功创建世界观：${response.data.name}`);

       

        // 可选：清空表单

        this.formData = { name: '', tagline: '', summary: '', coverUrl: '' };

       

        // 可选：跳转到详情页或列表页

        // this.$router.push('/dashboard');



      } catch (error) {

        // 4. 错误处理

        console.error('创建失败:', error);

       

        // 获取后端返回的具体错误信息 (如果有的话)

        if (error.response && error.response.data) {

           // 有时候后端直接返回字符串，有时候是对象

           this.errorMessage = typeof error.response.data === 'string'

              ? error.response.data

              : '创建失败，请检查输入';

        } else {

           this.errorMessage = '网络连接错误，请稍后重试';

        }

      } finally {

        // 5. 无论成功失败，取消加载状态

        this.isLoading = false;

      }

    }

  }

};

</script>



<style scoped>

/* 简单的卡片样式，你可以根据你的项目 UI 库（如 ElementUI / AntDesign）替换这些 CSS */



.ip-create-card {

  max-width: 600px;

  margin: 2rem auto;

  padding: 2rem;

  background: #fff;

  border-radius: 12px;

  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);

}



.card-header h2 {

  margin-bottom: 0.5rem;

  color: #333;

}

.card-header p {

  color: #666;

  margin-bottom: 2rem;

}



.form-group {

  margin-bottom: 1.5rem;

  display: flex;

  flex-direction: column;

  text-align: left;

}



label {

  font-weight: 600;

  margin-bottom: 0.5rem;

  color: #444;

}



.required {

  color: #e74c3c;

}



input, textarea {

  padding: 10px;

  border: 1px solid #ddd;

  border-radius: 6px;

  font-size: 1rem;

  transition: border-color 0.3s;

}



input:focus, textarea:focus {

  outline: none;

  border-color: #3498db;

}



.error-msg {

  color: #e74c3c;

  background: #fdeaea;

  padding: 10px;

  border-radius: 6px;

  margin-bottom: 1rem;

}



.submit-btn {

  width: 100%;

  padding: 12px;

  background-color: #3498db;

  color: white;

  border: none;

  border-radius: 6px;

  font-size: 1rem;

  cursor: pointer;

  transition: background 0.3s;

}



.submit-btn:hover {

  background-color: #2980b9;

}



.submit-btn:disabled {

  background-color: #95a5a6;

  cursor: not-allowed;

}

</style>