# 🚀 Микросервис Test Service

Простой и масштабируемый микросервис для управления тестовыми данными, готовый к работе с Docker Compose, Docker Desktop Kubernetes и облачным развертыванием.

## 🎯 Особенности

- ✅ **REST API** для управления тестовыми сущностями
- ✅ **PostgreSQL 15** с автоматическими миграциями
- ✅ **Docker Compose** для локальной разработки
- ✅ **Docker Desktop Kubernetes** для локального K8s
- ✅ **Kubernetes-ready** архитектура для продакшена
- ✅ **Health checks** для мониторинга
- ✅ **Готовая миграция** в Yandex Cloud
- ✅ **Автоматические бэкапы** базы данных
- ✅ **Полная CI/CD** система

## 🛠️ Быстрый старт

### Предварительные требования

- [Docker Desktop](https://docs.docker.com/get-docker/) (версия 4.15+)
- [Docker Compose](https://docs.docker.com/compose/install/)
- Node.js 18+ или .NET 10+ (в зависимости от стека)
- kubectl (устанавливается с Docker Desktop)

### Запуск с Docker Compose (самый быстрый способ)

1. **Клонируйте проект:**
   ```bash
   git clone <your-repo-url>
   cd testproject