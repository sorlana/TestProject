# RBAC Манифесты для TestProject

Эта папка содержит конфигурации RBAC (Role-Based Access Control) для проекта TestProject.

## 📁 Структура файлов

### Основные файлы:
- `service-account.yaml` - Сервисные аккаунты для различных задач
- `cluster-role.yaml` - Роли с правами доступа
- `cluster-role-binding.yaml` - Привязки ролей к сервисным аккаунтам
- `namespace.yaml` - Пространство имен и политики сети
- `kustomization.yaml` - Конфигурация Kustomize для управления манифестами

## 🔐 Сервисные аккаунты

### 1. `testproject-service-account`
Основной сервисный аккаунт для приложения `test-service`. Используется в Deployment для доступа к Kubernetes API.

### 2. `testproject-backup-service-account`
Для задач бэкапа и восстановления данных. Используется в CronJob для автоматических бэкапов.

### 3. `testproject-migration-service-account`
Для выполнения миграций базы данных. Используется в Job для применения миграций.

### 4. `testproject-monitoring-service-account`
Для сбора метрик и мониторинга. Используется Prometheus, Grafana и другими инструментами мониторинга.

## 🛡️ Роли (ClusterRole)

### 1. `testproject-app-role`
Минимальные права для основного приложения:
- Чтение ConfigMaps и Secrets
- Просмотр Pods, Services, Endpoints
- Просмотр Events для дебаггинга
- Чтение информации о нодах
- Доступ к метрикам для автоскейлинга

### 2. `testproject-backup-role`
Права для задач бэкапа:
- Управление PersistentVolumes и PersistentVolumeClaims
- Создание и удаление временных подов
- Управление Jobs и CronJobs

### 3. `testproject-migration-role`
Права для миграций БД:
- Создание подов для выполнения миграций
- Чтение конфигураций подключения к БД

### 4. `testproject-monitoring-role`
Права для мониторинга:
- Чтение метрик
- Доступ к событиям для алертинга

## 🔗 Привязки (ClusterRoleBinding)

Каждая роль привязана к соответствующему сервисному аккаунту в namespace `testproject-namespace`.

## 🚀 Использование

### Применение всех манифестов:
```bash
kubectl apply -k manifests/