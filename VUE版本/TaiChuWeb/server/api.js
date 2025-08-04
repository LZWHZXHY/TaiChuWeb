import axios from 'axios';

// 创建axios实例
const api = axios.create({
  baseURL: 'https://localhost:7029/api', // 您的.NET后端地址
  timeout: 10000, // 10秒超时
  headers: {
    'Content-Type': 'application/json'
  }
});

// 财务数据API
export const financialApi = {
  // 获取所有财务记录
  getAll: () => api.get('/financial'),
  
  // 获取单个记录
  getById: (id) => api.get(`/financial/${id}`),
  
  // 创建新记录
  create: (data) => api.post('/financial', data),
  
  // 更新记录
  update: (id, data) => api.put(`/financial/${id}`, data),
  
  // 删除记录
  delete: (id) => api.delete(`/financial/${id}`),
  
  // 获取财务摘要
  getSummary: () => api.get('/financial/summary')
};

// 其他API可以继续添加...