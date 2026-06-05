import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0', // Listen on all local IPs
    allowedHosts: 'all', // Allow ngrok domain
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5262',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
