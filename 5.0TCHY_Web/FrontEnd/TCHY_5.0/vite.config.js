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
          // 将 utils 下的文件打包到单独的 chunk
          if (id.includes('/src/utils/')) {
            return 'utils'
          }
          // 将 vue 相关依赖打包到 vendor chunk
          if (id.includes('node_modules/vue') || id.includes('node_modules/@vue/')) {
            return 'vendor'
          }
        }
      }
    },
    chunkSizeWarningLimit: 1000 // 提高 chunk 大小警告限制
  },
  optimizeDeps: {
    include: ['@/utils/api.js', '@/utils/auth.js'] // 预构建这些模块
  }
})