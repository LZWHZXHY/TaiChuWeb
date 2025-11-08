import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import App from './彼岸宇宙.vue'

import SettingsSidebar from './components/SettingsSidebar.vue'
import OCFileList from './components/OCFileList.vue'

const routes = [
    {path:"/Settings", component:SettingsSidebar},
    {path:"/OClist", component:OCFileList}
]

const router = createRouter({
    history:createWebHashHistory(),
    routes
})





const app = createApp(App)

app.use(router)

app.mount('#app')
