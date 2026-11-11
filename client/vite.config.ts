// client/vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  
  // 🔹 Ваши существующие настройки сервера (сохранены!)
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5051'
    }
  },
  
  // 🔹 Настройки для псевдонимов (опционально, но удобно для импортов)
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  
  // 🔹 НОВАЯ СЕКЦИЯ: Конфигурация тестов Vitest
  test: {
    globals: true,                    // Позволяет использовать describe, it, expect без импорта
    environment: 'jsdom',             // Эмулирует браузерное окружение для React
    setupFiles: './src/test/setupTests.ts',  // Файл с глобальными настройками тестов
    css: true,                        // Поддержка CSS-модулей в тестах
    // Отключаем лишние логи в тестах
    silent: false,
  },
})