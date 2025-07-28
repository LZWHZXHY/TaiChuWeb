import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

import{resolve} from "path"


// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve:{
    extensions:['.vue','.js','.ts','.json'],
  },
  build:{
    rollupOptions:{
      input:{
        index:resolve(__dirname, "index.html"),
        LoginMain:resolve(__dirname, "LoginMain.html"),
        彼岸宇宙:resolve(__dirname,"彼岸宇宙.html"),
        柴圈社区:resolve(__dirname,"ChaiQuan.html")
      }
    }
  }
})
