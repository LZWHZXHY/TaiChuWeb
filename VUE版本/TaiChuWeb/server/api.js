import axios from 'axios';

// 创建axios实例
const createApi = () => {
  // 确保 baseURL 正确包含 /api
  const baseURL = import.meta.env.VITE_API_BASE_URL;
  console.log('API Base URL:', baseURL); // 添加日志确认URL
  
  const api = axios.create({
    baseURL,
    timeout: 30000,
    headers: {
      'Content-Type': 'application/json'
    }
  });

  // 请求拦截器
  api.interceptors.request.use(config => {
    // 打印完整URL
    const fullUrl = config.baseURL + config.url;
    console.log('发送请求到:', fullUrl);
    
    // 添加认证令牌
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  }, error => {
    return Promise.reject(error);
  });

  // 响应拦截器
  api.interceptors.response.use(response => {
    console.log('收到响应:', response.config.url, response.status);
    return response.data;
  }, error => {
    console.error('API错误:', error);
    
    if (error.response) {
      console.error('错误详情:', error.response.data);
      
      if (error.response.status === 401) {
        localStorage.removeItem('authToken');
        window.location.href = '/login';
      }
      
      return Promise.reject(error.response.data);
    } else if (error.request) {
      return Promise.reject(new Error('服务器无响应'));
    } else {
      return Promise.reject(error);
    }
  });

  return api;
};

// 创建API实例
const api = createApi();

// 用户认证API - 使用绝对路径确保正确性
export const authApi = {
  login: (credentials) => api.post('/api/Login/login', credentials), // 使用绝对路径
  register: (data) => api.post('/api/auth/register-with-code', data),
  sendVerificationCode: (email) => api.get(`/api/mailkittest/send-code/${email}`),
  getProfile: () => api.get('/api/UserInfo'), // 修改为实际后端路径
  logout: () => api.post('/api/auth/logout')
};

// 小说API
export const novelApi = {
  // 获取小说列表
  getNovels: () => api.get('/api/novels'),
  // 获取单本小说详情
  getNovel: (id) => api.get(`/api/novels/${id}`),
  // 获取某本小说的章节目录（和内容）
  getChapters: (novelId) => api.get(`/api/novels/${novelId}/chapters`),
  // 获取单个章节内容
  getChapterContent: (chapterId) => api.get(`/api/novels/chapter/${chapterId}`),
  // 点赞章节
  likeChapter: (chapterId) => api.post(`/api/novels/chapter/${chapterId}/like`),
  // 举报章节
  reportChapter: (chapterId) => api.post(`/api/novels/chapter/${chapterId}/report`),
  getChapterActionStatus: (chapterId) => api.get(`/api/novels/chapter/${chapterId}/actionstatus`)
};



export default api
