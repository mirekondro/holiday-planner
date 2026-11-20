import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Dev only: forward API calls to the local backend (BE.Api "http" profile),
    // so the browser sees one origin and no CORS setup is needed locally.
    proxy: {
      '/api': 'http://localhost:5102',
    },
  },
})
