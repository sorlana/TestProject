# TestProject - Инструкция по запуску

## Быстрый старт

### Запуск всего проекта

```powershell
.\start-project.ps1
```

Или вручную:

```powershell
docker-compose up -d
```

### Остановка проекта

```powershell
.\stop-project.ps1
```

Или вручную:

```powershell
docker-compose stop
```

### Полное удаление контейнеров

```powershell
docker-compose down
```

## Доступные сервисы

После запуска проекта доступны следующие сервисы:

| Сервис | URL | Описание |
|--------|-----|----------|
| User Portal | http://localhost:3001 | Пользовательский портал (React SPA) |
| User Auth Service | http://localhost:8081 | Сервис аутентификации |
| User Auth Swagger | http://localhost:8081/swagger | API документация аутентификации |
| Test Service | http://localhost:5000 | Основной API сервис |
| Test Service Swagger | http://localhost:5000/swagger | API документация основного сервиса |
| Landing CMS | http://localhost:3000 | CMS для лендинга |
| PostgreSQL | localhost:5432 | База данных |
| Redis | localhost:6379 | Кеш и сессии |

## Структура проекта

Все контейнеры запускаются в единой сети `testproject_default` и управляются как один проект.

### Контейнеры

- `testproject-postgres` - PostgreSQL база данных
- `testproject-redis` - Redis кеш
- `testproject-test-service` - Основной .NET API сервис
- `testproject-user-auth-service` - Сервис аутентификации .NET
- `testproject-landing-cms` - Landing CMS (.NET + Piranha CMS)
- `testproject-user-portal` - Пользовательский портал (React)

## Автоматический перезапуск

Все контейнеры настроены с политикой `restart: unless-stopped`, что означает:

- ✅ Контейнеры автоматически запустятся после перезагрузки компьютера
- ✅ Контейнеры автоматически перезапустятся при падении
- ✅ Контейнеры останутся остановленными только если вы явно их остановили

## Полезные команды

### Просмотр статуса всех контейнеров

```powershell
docker-compose ps
```

### Просмотр логов

Все сервисы:
```powershell
docker-compose logs -f
```

Конкретный сервис:
```powershell
docker-compose logs -f user-authentication-service
docker-compose logs -f user-portal
docker-compose logs -f test-service
```

### Перезапуск конкретного сервиса

```powershell
docker-compose restart user-authentication-service
```

### Пересборка и перезапуск сервиса

```powershell
docker-compose up -d --build user-authentication-service
```

### Проверка сети

```powershell
docker network inspect testproject_default
```

## Переменные окружения

Все настройки находятся в файле `.env`. Основные параметры:

- `COMPOSE_PROJECT_NAME=testproject` - имя проекта
- `APP_PORT=5000` - порт основного API
- `AUTH_SERVICE_PORT=8081` - порт сервиса аутентификации
- `USER_PORTAL_PORT=3001` - порт пользовательского портала
- `LANDING_CMS_PORT=3000` - порт CMS
- `DB_PORT=5432` - порт PostgreSQL
- `REDIS_PORT=6379` - порт Redis

## Troubleshooting

### Контейнеры не запускаются

1. Проверьте, что Docker Desktop запущен
2. Проверьте логи: `docker-compose logs`
3. Попробуйте пересобрать: `docker-compose up -d --build`

### Порты заняты

Если порты уже используются другими приложениями, измените их в файле `.env`

### Контейнеры не в одной сети

Выполните полную пересборку:

```powershell
docker-compose down
docker-compose up -d
```

### База данных не инициализируется

Удалите volume и пересоздайте:

```powershell
docker-compose down -v
docker-compose up -d
```

⚠️ **Внимание**: Это удалит все данные из базы!

## Разработка

### Горячая перезагрузка

Для разработки с горячей перезагрузкой используйте volume mapping в docker-compose.yaml

### Отладка

Для отладки .NET сервисов можно подключиться к контейнеру:

```powershell
docker exec -it testproject-user-auth-service /bin/bash
```

## Продакшн

Для продакшн окружения:

1. Измените `.env` на `.env.production`
2. Установите `NODE_ENV=production`
3. Настройте SSL сертификаты
4. Измените пароли и секретные ключи
5. Настройте внешние базы данных и Redis
