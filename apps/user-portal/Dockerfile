# Multi-stage build для User Portal
# Stage 1: Builder - установка зависимостей и сборка проекта с Vite
FROM node:18-alpine AS builder

# Установка рабочей директории
WORKDIR /app

# Принимаем build аргументы для переменных окружения
ARG VITE_GOOGLE_CLIENT_ID
ARG VITE_API_BASE_URL

# Устанавливаем переменные окружения для сборки
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Копирование package.json и package-lock.json для кэширования зависимостей
COPY package*.json ./

# Установка зависимостей (включая dev для сборки)
RUN npm ci --legacy-peer-deps

# Копирование исходного кода
COPY . .

# Сборка проекта с Vite
RUN npm run build

# Stage 2: Production - копирование собранных файлов в nginx:alpine
FROM nginx:alpine

# Копирование собранных файлов из builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Копирование кастомной конфигурации nginx
COPY nginx/nginx.conf /etc/nginx/nginx.conf

# Открытие порта 80
EXPOSE 80

# Запуск nginx
CMD ["nginx", "-g", "daemon off;"]