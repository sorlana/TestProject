# Landing CMS - Лендинг платформы онлайн-обучения

Простой лендинг на базе Piranha CMS для платформы онлайн-обучения. Предоставляет информацию о платформе, тарифах и обеспечивает переход к фронтенд-микросервису для авторизации и работы с личным кабинетом.

## Оглавление

- [Архитектура](#архитектура)
- [Технологический стек](#технологический-стек)
- [Структура проекта](#структура-проекта)
- [Локальный запуск](#локальный-запуск)
- [Конфигурация](#конфигурация)
- [Работа с Piranha Manager](#работа-с-piranha-manager)
- [Развертывание](#развертывание)
- [Зависимости](#зависимости)

## Архитектура

Landing CMS является простым контентным сайтом без логики аутентификации. Вся логика авторизации, регистрации и личного кабинета вынесена в отдельный фронтенд-микросервис (User Portal).

```
┌─────────────────────────────────────────────────────────────┐
│                        Клиент (Браузер)                      │
└────────────┬────────────────────────────────────────────┬───┘
             │                                             │
             │ HTTP/HTTPS                                  │
             │                                             │
┌────────────▼─────────────────────────────────────────────────┐
│                    Landing CMS Service                        │
│  ┌──────────────────────────────────────────────────────┐    │
│  │           ASP.NET Core 8.0 + Piranha CMS             │    │
│  │  ┌────────────────┐                                  │    │
│  │  │  Razor Pages   │  Серверный рендеринг            │    │
│  │  │  (Landing)     │  Статический контент             │    │
│  │  └────────────────┘                                  │    │
│  │                                                       │    │
│  │  ┌────────────────────────────────────────────────┐ │    │
│  │  │        Piranha CMS Manager (/manager)          │ │    │
│  │  └────────────────────────────────────────────────┘ │    │
│  └──────────────────────────────────────────────────────┘    │
│                           │                                   │
│                           │ Entity Framework Core             │
│                           ▼                                   │
│  ┌──────────────────────────────────────────────────────┐    │
│  │         PostgreSQL (Piranha Schema)                  │    │
│  └──────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
                                │
                                │ Редирект на /app
                                │
┌───────────────────────────────▼───────────────────────────────┐
│              User Portal (React + Blueprint)                   │
│                    Отдельный микросервис                       │
│  ┌──────────────────────────────────────────────────────┐     │
│  │  /app/login - авторизация                           │     │
│  │  /app/register - регистрация                        │     │
│  │  /app/dashboard - личный кабинет                    │     │
│  │  /app/payment - оплата                              │     │
│  └──────────────────────────────────────────────────────┘     │
└────────────────────────────────────────────────────────────────┘
```

### Компоненты

1. **LandingController** - обработка запросов главной страницы
2. **TariffController** - обработка покупки тарифов (редирект на /app/payment)
3. **Razor Views** - серверный рендеринг страниц
4. **Piranha CMS Models** - модели данных для контента
5. **Piranha Manager** - административная панель для управления контентом

### Маршрутизация

```
/                    → Главная страница лендинга
/manager             → Piranha Manager (админка)
/tariff/purchase/:id → Редирект на /app/payment?tariffId=:id

/app                 → User Portal (фронтенд-микросервис)
/app/login           → Авторизация
/app/register        → Регистрация
/app/dashboard       → Личный кабинет
/app/payment         → Оплата
```

## Технологический стек

- **ASP.NET Core 8.0** - серверная платформа
- **Piranha CMS 11.x** - система управления контентом
- **PostgreSQL 15+** - база данных
- **Entity Framework Core 8.0** - ORM
- **Bootstrap 5** - CSS фреймворк
- **Serilog** - логирование
- **Docker** - контейнеризация
- **Kubernetes** - оркестрация

## Структура проекта

```
apps/landing-cms/
├── Controllers/              # Контроллеры ASP.NET Core
│   ├── LandingController.cs  # Главная страница
│   └── TariffController.cs   # Обработка покупки тарифов
├── Models/                   # Модели данных Piranha CMS
│   ├── LandingPage.cs        # Модель главной страницы
│   ├── HeroRegion.cs         # Hero секция
│   ├── AboutRegion.cs        # Секция "О программе"
│   ├── TariffsRegion.cs      # Секция "Тарифы"
│   ├── TariffItem.cs         # Элемент тарифа
│   └── SeoRegion.cs          # SEO данные
├── Views/                    # Razor Views
│   ├── Landing/
│   │   └── Index.cshtml      # Главная страница
│   └── Shared/
│       ├── _Layout.cshtml    # Общий layout
│       ├── _TariffCard.cshtml # Карточка тарифа
│       └── _StructuredData.cshtml # Structured data
├── wwwroot/                  # Статические файлы
│   ├── css/
│   │   └── site.css          # Стили
│   └── js/
│       └── site.js           # JavaScript
├── Data/                     # Seed данные
│   └── SeedData.cs           # Начальные данные
├── Helpers/                  # Вспомогательные классы
│   └── ImageHelper.cs        # Работа с изображениями
├── k8s/                      # Kubernetes манифесты
│   ├── deployment.yaml       # Deployment
│   ├── service.yaml          # Service
│   ├── hpa.yaml              # HorizontalPodAutoscaler
│   ├── configmap.yaml        # ConfigMap
│   └── secret.yaml           # Secret
├── logs/                     # Логи приложения
├── Program.cs                # Точка входа
├── appsettings.json          # Конфигурация
├── appsettings.Development.json # Конфигурация для разработки
├── appsettings.Production.json  # Конфигурация для продакшена
├── Dockerfile                # Docker образ
└── LandingCms.csproj         # Файл проекта
```

## Локальный запуск

### Предварительные требования

- .NET SDK 8.0+
- Docker и Docker Compose
- PostgreSQL 15+ (или через Docker)

### Запуск через Docker Compose

1. Клонируйте репозиторий и перейдите в корневую директорию проекта

2. Запустите все сервисы:

```bash
docker-compose up -d
```

3. Приложение будет доступно по адресу:
   - Лендинг: http://localhost:3000
   - Piranha Manager: http://localhost:3000/manager

4. Для просмотра логов:

```bash
docker-compose logs -f landing-cms
```

5. Для остановки:

```bash
docker-compose down
```

### Локальный запуск без Docker

1. Убедитесь, что PostgreSQL запущен и доступен

2. Обновите строку подключения в `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "PiranhaDb": "Host=localhost;Database=learning_platform;Username=postgres;Password=your_password;SearchPath=piranha"
  }
}
```

3. Перейдите в директорию проекта:

```bash
cd apps/landing-cms
```

4. Восстановите зависимости:

```bash
dotnet restore
```

5. Примените миграции базы данных:

```bash
dotnet ef database update
```

6. Запустите приложение:

```bash
dotnet run
```

7. Приложение будет доступно по адресу:
   - Лендинг: http://localhost:5000
   - Piranha Manager: http://localhost:5000/manager

## Конфигурация

### Переменные окружения

Приложение использует следующие переменные окружения:

| Переменная | Описание | Значение по умолчанию |
|-----------|----------|----------------------|
| `ConnectionStrings__PiranhaDb` | Строка подключения к PostgreSQL | `Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha` |
| `ASPNETCORE_ENVIRONMENT` | Окружение (Development/Production) | `Production` |
| `Logging__LogLevel__Default` | Уровень логирования | `Information` |
| `Piranha__MediaCDN` | CDN для медиа файлов | `` (пусто) |

### appsettings.json

Основная конфигурация приложения:

```json
{
  "ConnectionStrings": {
    "PiranhaDb": "Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Piranha": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/landing-cms-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "AllowedHosts": "*",
  "Piranha": {
    "MediaCDN": ""
  }
}
```

### Конфигурация базы данных

Приложение использует PostgreSQL с отдельной схемой `piranha` для изоляции данных Piranha CMS.

**Строка подключения:**
```
Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha
```

**Параметры:**
- `Host` - хост PostgreSQL
- `Database` - имя базы данных
- `Username` - имя пользователя
- `Password` - пароль
- `SearchPath` - схема для Piranha CMS (обязательно `piranha`)

### Логирование

Приложение использует Serilog для логирования:

- **Console** - вывод в консоль
- **File** - запись в файлы в директории `logs/`
- **Rolling Interval** - новый файл каждый день

Логи сохраняются в формате: `logs/landing-cms-YYYYMMDD.txt`

## Работа с Piranha Manager

### Доступ к административной панели

1. Откройте браузер и перейдите по адресу: http://localhost:3000/manager

2. Войдите с учетными данными администратора:
   - **Email:** admin@example.com
   - **Password:** Admin123!

### Управление контентом

#### Редактирование главной страницы

1. В Piranha Manager перейдите в раздел **Pages**
2. Выберите страницу **Home** (/)
3. Отредактируйте регионы:

**Hero Section:**
- Title - заголовок Hero секции
- Description - описание под заголовком
- Background Image - фоновое изображение
- Button Text - текст кнопки призыва к действию

**About Section:**
- Title - заголовок секции "О программе"
- Description - HTML контент с описанием
- Image - изображение для секции

**Tariffs Section:**
- Title - заголовок секции "Тарифы"
- Tariffs - список тарифов (можно добавлять/удалять)

**SEO:**
- Meta Title - заголовок для поисковых систем
- Meta Description - описание для поисковых систем
- Meta Keywords - ключевые слова
- OG Title - заголовок для социальных сетей
- OG Description - описание для социальных сетей
- OG Image - изображение для социальных сетей

4. Нажмите **Save** для сохранения изменений

#### Управление тарифами

1. В редакторе страницы перейдите к секции **Tariffs**
2. Нажмите **Add Item** для добавления нового тарифа
3. Заполните поля:
   - **Tariff ID** - уникальный идентификатор (например, "general")
   - **Name** - название тарифа
   - **Description** - краткое описание
   - **Price** - стоимость в рублях
   - **Features** - список особенностей (по одной на строку)
4. Нажмите **Save**

#### Загрузка изображений

1. В Piranha Manager перейдите в раздел **Media**
2. Нажмите **Upload** и выберите изображение
3. После загрузки изображение будет доступно для использования в регионах

**Рекомендации:**
- Используйте изображения в формате JPG или PNG
- Оптимальный размер для Hero: 1920x1080px
- Оптимальный размер для About: 800x600px
- Оптимальный размер для OG Image: 1200x630px

### Начальные данные (Seed Data)

При первом запуске приложение автоматически создает:

1. **Администратора:**
   - Email: admin@example.com
   - Password: Admin123!

2. **Главную страницу** с тестовым контентом:
   - Hero секция с заголовком и описанием
   - Секция "О программе"
   - Тариф "Общий" стоимостью 1 рубль (ID: "general")
   - SEO данные

## Развертывание

### Docker

#### Сборка образа

```bash
cd apps/landing-cms
docker build -t landing-cms:latest .
```

#### Запуск контейнера

```bash
docker run -d \
  --name landing-cms \
  -p 3000:80 \
  -e ConnectionStrings__PiranhaDb="Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha" \
  landing-cms:latest
```

### Kubernetes

#### Применение манифестов

1. Создайте Secret с строкой подключения к БД:

```bash
kubectl create secret generic landing-cms-secrets \
  --from-literal=db-connection="Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha"
```

2. Примените манифесты:

```bash
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml
```

3. Проверьте статус:

```bash
kubectl get pods -l app=landing-cms
kubectl get svc landing-cms
```

#### Health Checks

Приложение предоставляет следующие endpoints для health checks:

- `/health` - общий health check с проверкой PostgreSQL
- `/health/ready` - readiness probe для Kubernetes
- `/health/live` - liveness probe для Kubernetes

#### Автомасштабирование

HorizontalPodAutoscaler настроен на:
- Минимум: 2 реплики
- Максимум: 10 реплик
- Целевая загрузка CPU: 70%

#### Resource Limits

```yaml
resources:
  requests:
    cpu: 200m
    memory: 256Mi
  limits:
    cpu: 500m
    memory: 512Mi
```

## Зависимости

### Внешние зависимости

1. **PostgreSQL 15+**
   - База данных для хранения контента Piranha CMS
   - Требуется схема `piranha`
   - Порт: 5432

2. **User Portal (фронтенд-микросервис)**
   - Обрабатывает авторизацию и личный кабинет
   - Доступен на `/app`
   - Не является обязательной зависимостью для работы лендинга

### NuGet пакеты

Основные зависимости проекта:

```xml
<PackageReference Include="Piranha" Version="11.x" />
<PackageReference Include="Piranha.AspNetCore" Version="11.x" />
<PackageReference Include="Piranha.AspNetCore.Identity.PostgreSQL" Version="11.x" />
<PackageReference Include="Piranha.Data.EF.PostgreSQL" Version="11.x" />
<PackageReference Include="Piranha.Manager" Version="11.x" />
<PackageReference Include="Piranha.Manager.TinyMCE" Version="11.x" />
<PackageReference Include="Piranha.ImageSharp" Version="11.x" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.x" />
<PackageReference Include="Serilog.AspNetCore" Version="8.x" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.x" />
<PackageReference Include="Serilog.Sinks.File" Version="5.x" />
```

### Схема базы данных

Приложение использует отдельную схему `piranha` в PostgreSQL для изоляции данных:

```sql
-- Создание схемы
CREATE SCHEMA IF NOT EXISTS piranha;

-- Установка search_path
SET search_path TO piranha;
```

Все таблицы Piranha CMS создаются автоматически при первом запуске через Entity Framework Core Migrations.

## Устранение неполадок

### Проблемы с подключением к БД

**Ошибка:** `Npgsql.NpgsqlException: Connection refused`

**Решение:**
1. Убедитесь, что PostgreSQL запущен
2. Проверьте строку подключения в `appsettings.json`
3. Проверьте, что схема `piranha` существует

### Проблемы с Piranha Manager

**Ошибка:** `Unable to access Piranha Manager`

**Решение:**
1. Убедитесь, что приложение запущено
2. Перейдите по адресу `/manager`
3. Проверьте, что администратор создан (см. Seed Data)

### Проблемы с изображениями

**Ошибка:** `Images not loading`

**Решение:**
1. Проверьте, что директория `wwwroot` существует
2. Убедитесь, что ImageSharp настроен корректно
3. Проверьте права доступа к файлам

## Лицензия

Проект разработан для внутреннего использования платформы онлайн-обучения.

## Контакты

Для вопросов и поддержки обращайтесь к команде разработки.
