import { loadEnv } from 'vite'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Dev-only proxy target. Deployed builds call VITE_API_BASE_URL directly (see src/config.ts).
  const devApiTarget = env.DEV_API_PROXY_TARGET || 'http://localhost:5000'

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    build: {
      rollupOptions: {
        output: {
          // Keep the large UI libraries in their own cacheable chunks.
          manualChunks: {
            mui: ['@mui/material', '@mui/icons-material'],
            datagrid: ['@mui/x-data-grid'],
          },
        },
      },
    },
    test: {
      environment: 'jsdom',
      globals: false,
      setupFiles: ['./src/test/setup.ts'],
    },
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: devApiTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
