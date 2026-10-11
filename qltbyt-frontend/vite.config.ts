import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, 'src') },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5149', changeOrigin: true },
      '/files': { target: 'http://localhost:5149', changeOrigin: true },
      '/captures': { target: 'http://localhost:5149', changeOrigin: true },
      '/hdbaotri': { target: 'http://localhost:5149', changeOrigin: true },
    },
  },
  preview: {
    host: '0.0.0.0',
    port: 8017,
  },
});
