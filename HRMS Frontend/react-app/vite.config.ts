import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
    '/api': {
      target: 'https://ec2-35-172-146-76.compute-1.amazonaws.com:5001',
      changeOrigin: true,
      secure: false
    }
  }
  },
  build: {
    outDir: 'build', 
  },
})
