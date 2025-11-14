import { createApp } from 'vue'
import App from './App.vue'
import PageNotFound from './componements/PageNotFound.vue';
import { createRouter, createWebHistory } from 'vue-router';



const routes = [
    {
        path:'/',
        component:PageNotFound
    },
    {
        path: '/taichu',
        name: '太初板块', 
        component: () => import('./componements/太初板块.vue') // 需要创建这个组件
    },
    {
        path:'/admin',
        name:'管理员页面',
        component: () => import('./componements/PageNotFound.vue')
    }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

const app = createApp(App);

// 关键：安装路由器
app.use(router);

app.mount('#app');