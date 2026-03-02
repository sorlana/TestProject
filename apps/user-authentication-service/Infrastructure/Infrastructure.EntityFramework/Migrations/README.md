# Миграции базы данных

## Описание

Этот каталог содержит миграции Entity Framework Core для микросервиса аутентификации пользователей.

## Применение миграций

### Локальная разработка

Для применения миграций к локальной базе данных PostgreSQL:

```bash
# Перейти в каталог проекта Infrastructure.EntityFramework
cd apps/user-authentication-service/Infrastructure/Infrastructure.EntityFramework

# Применить миграции
dotnet ef database update
```

### Использование SQL скрипта

Альтернативно, можно применить миграции вручную, используя SQL скрипт:

```bash
# Подключиться к PostgreSQL
psql -h localhost -U postgres -d user_authentication_db

# Выполнить SQL скрипт
\i Migrations/InitialCreate.sql
```

## Создание новых миграций

Для создания новой миграции после изменения моделей:

```bash
# Создать миграцию
dotnet ef migrations add <МиграцияИмя> --output-dir Migrations

# Сгенерировать SQL скрипт
dotnet ef migrations script --output Migrations/<МиграцияИмя>.sql
```

## Откат миграций

Для отката последней миграции:

```bash
# Откатить миграцию
dotnet ef migrations remove

# Откатить базу данных к предыдущей миграции
dotnet ef database update <ПредыдущаяМиграция>
```

## Требования

- .NET 8.0 SDK
- PostgreSQL 12 или выше
- Entity Framework Core Tools (`dotnet tool install --global dotnet-ef`)

## Строка подключения

Строка подключения настраивается в файле `appsettings.json` проекта WebApi:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=user_authentication_db;Username=postgres;Password=postgres"
  }
}
```

Для production окружения используйте переменные окружения для безопасного хранения учетных данных.
