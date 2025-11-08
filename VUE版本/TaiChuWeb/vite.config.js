import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';

export default defineConfig(({ command, mode }) => {
  // 加载环境变量
  const env = loadEnv(mode, process.cwd(), '');
  
  // 获取当前环境变量
  const apiBaseUrl = env.VITE_API_BASE_URL;
  const proxySecure = env.VITE_PROXY_SECURE === 'true';
  const debugMode = env.VITE_DEBUG_MODE === 'true';
  
  return {
    plugins: [vue()],
    resolve: {
      extensions: ['.vue', '.js', '.ts', '.json'],
      alias: {
        '@': resolve(__dirname, 'src')
      }
    },
  build: {
    rollupOptions: {
      input: {
        index: resolve(__dirname, "index.html"),
        beyondUniverse: resolve(__dirname, "彼岸宇宙.html"), 
        tchy:resolve(__dirname, "TCHY.html")

      },
      output: {
        assetFileNames: 'assets/[name]-[hash][extname]',
        chunkFileNames: 'chunks/[name]-[hash].js',
        entryFileNames: 'entries/[name]-[hash].js'
      }
    },
    assetsDir: 'assets',
    cssCodeSplit: true
  },
  server: {
      port: 5173,
      open: '/index.html',
      proxy: {
        '/api': {
          target: apiBaseUrl,
          changeOrigin: true,
          secure: proxySecure, // 使用环境变量控制
          rewrite: (path) => path.replace(/^\/api/, ''),
          configure: debugMode ? (proxy) => {
            proxy.on('error', (err) => {
              console.error('Proxy error:', err);
            });
            proxy.on('proxyReq', (proxyReq, req) => {
              console.log('Sending Request to the Target:', req.url);
            });
            proxy.on('proxyRes', (proxyRes, req) => {
              console.log('Received Response from the Target:', req.url, proxyRes.statusCode);
            });
          } : undefined // 生产环境关闭调试日志
        },
        '/UserInfo': {
          target: apiBaseUrl,
          changeOrigin: true,
          secure: proxySecure // 使用环境变量控制
        }
      }
    },
    define: {
      'process.env': {}
    }
  };
});