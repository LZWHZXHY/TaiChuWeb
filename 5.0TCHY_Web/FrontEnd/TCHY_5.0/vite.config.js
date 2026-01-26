import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (id.includes('/src/utils/')) {
            return 'utils'
          }
          if (id.includes('node_modules/vue') || id.includes('node_modules/@vue/')) {
            return 'vendor'
          }
        }
      }
    },
    chunkSizeWarningLimit: 1000
  },
  optimizeDeps: {
    // 🔥 核心修复：只保留 include，删除之前的 exclude 块！
    // 让 Vite 重新接管这些包的优化，这能解决 500 错误。
    include: ['@/utils/api.js', '@/utils/auth.js'] 
  }
})