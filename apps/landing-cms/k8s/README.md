# Kubernetes манифесты для Landing CMS

Этот каталог содержит манифесты Kubernetes для развертывания Landing CMS в кластере.

## Файлы

- **deployment.yaml** - Deployment для Landing CMS с настройками ресурсов и health checks
- **service.yaml** - Service для доступа к приложению внутри кластера
- **hpa.yaml** - HorizontalPodAutoscaler для автоматического масштабирования
- **configmap.yaml** - ConfigMap с конфигурацией приложения
- **secret.yaml** - Secret с чувствительными данными (строка подключения к БД)

## Предварительные требования

1. Kubernetes кластер (версия 1.24+)
2. kubectl настроен для работы с кластером
3. PostgreSQL развернут в кластере или доступен извне
4. Docker образ landing-cms собран и доступен в registry

## Развертывание

### 1. Обновите секреты

Отредактируйте `secret.yaml` и замените `CHANGE_ME` на реальный пароль PostgreSQL:

```bash
# Или создайте секрет напрямую из командной строки
kubectl create secret generic landing-cms-secrets \
  --from-literal=db-connection-string="Host=postgres;Database=learning_platform;Username=postgres;Password=YOUR_PASSWORD;SearchPath=piranha"
```

### 2. Примените манифесты

```bash
# Применить все манифесты
kubectl apply -f k8s/

# Или по отдельности
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml
```

### 3. Проверьте статус развертывания

```bash
# Проверить поды
kubectl get pods -l app=landing-cms

# Проверить deployment
kubectl get deployment landing-cms

# Проверить service
kubectl get service landing-cms

# Проверить HPA
kubectl get hpa landing-cms-hpa

# Посмотреть логи
kubectl logs -l app=landing-cms --tail=100 -f
```

## Health Checks

Приложение предоставляет следующие endpoints для проверки здоровья:

- **/health/live** - Liveness probe (проверяет, что приложение запущено)
- **/health/ready** - Readiness probe (проверяет подключение к PostgreSQL)
- **/health** - Общий health check

## Ресурсы

Настройки ресурсов для каждого пода:

- **Requests**: CPU 200m, Memory 256Mi
- **Limits**: CPU 500m, Memory 512Mi

## Автомасштабирование

HPA настроен на:

- Минимум 2 реплики
- Максимум 10 реплик
- Масштабирование при CPU > 70%
- Масштабирование при Memory > 80%

## Переменные окружения

Приложение использует следующие переменные окружения:

- `ASPNETCORE_ENVIRONMENT` - Окружение (Production)
- `ASPNETCORE_URLS` - URL для прослушивания (http://+:80)
- `ConnectionStrings__PiranhaDb` - Строка подключения к PostgreSQL (из Secret)
- `Piranha__MediaCDN` - URL CDN для медиафайлов (из ConfigMap)

## Безопасность

⚠️ **ВАЖНО**: В продакшене:

1. Используйте внешние системы управления секретами (HashiCorp Vault, Sealed Secrets)
2. Не храните пароли в plain text в Git
3. Настройте RBAC для ограничения доступа к секретам
4. Используйте Network Policies для изоляции трафика
5. Включите TLS/SSL для всех соединений

## Мониторинг

Рекомендуется настроить:

- Prometheus для сбора метрик
- Grafana для визуализации
- Alertmanager для уведомлений
- Loki для агрегации логов

## Обновление

Для обновления приложения:

```bash
# Обновить образ
kubectl set image deployment/landing-cms landing-cms=landing-cms:new-version

# Или применить обновленный манифест
kubectl apply -f k8s/deployment.yaml

# Проверить статус обновления
kubectl rollout status deployment/landing-cms

# Откатить при необходимости
kubectl rollout undo deployment/landing-cms
```

## Удаление

```bash
# Удалить все ресурсы
kubectl delete -f k8s/

# Или по отдельности
kubectl delete deployment landing-cms
kubectl delete service landing-cms
kubectl delete hpa landing-cms-hpa
kubectl delete configmap landing-cms-config
kubectl delete secret landing-cms-secrets
```
