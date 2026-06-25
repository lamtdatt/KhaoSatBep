import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          // Treat ion-* tags as native custom elements (Ionicons web components)
          isCustomElement: tag => tag.startsWith('ion-'),
        },
      },
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'https://khaosatbep-api.onrender.com',
        changeOrigin: true,
        secure: true,
      },
    },
  },
})
