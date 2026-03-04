# Kubernetes манифесты для User Portal

Этот каталог содержит Kubernetes манифесты для развертывания User Portal в кластере.

## Файлы

- `deployment.yaml` - Deployment с 2 репликами, ограничениями ресурсов и проверками здоровья
- `service.yaml` - ClusterIP сервис на порту 80
- `ingress.yaml` - Ingress с путем /app/* и TLS конфигурацией

## Развертывание

### Предварительные требования

1. Kubernetes кластер
2. nginx-ingress контроллер
3. cert-manager для автоматического управления TLS сертификатами (опционально)

### Шаги развертывания

1. **Создание TLS секрета** (если используете собственный сертификат):
   ```bash
   kubectl create secret tls user-portal-tls \
     --cert=path/to/tls.crt \
     --key=path/to/tls.key
   ```

2. **Применение манифестов**:
   ```bash
   # Применить все манифесты
   kubectl apply -f k8s/
   
   # Или по отдельности
   kubectl apply -f k8s/deployment.yaml
   kubectl apply -f k8s/service.yaml
   kubectl apply -f k8s/ingress.yaml
   ```

3. **Проверка развертывания**:
   ```bash
   # Проверить статус подов
   kubectl get pods -l app=user-portal
   
   # Проверить сервис
   kubectl get service user-portal-service
   
   # Проверить ingress
   kubectl get ingress user-portal-ingress
   ```

### Настройка

#### Изменение домена

В файле `ingress.yaml` замените `your-domain.com` на ваш реальный домен:

```yaml
tls:
- hosts:
  - your-actual-domain.com
  secretName: user-portal-tls
rules:
- host: your-actual-domain.com
```

#### Настройка ресурсов

В файле `deployment.yaml` можно изменить ограничения ресурсов в зависимости от нагрузки:

```yaml
resources:
  requests:
    memory: "64Mi"
    cpu: "50m"
  limits:
    memory: "128Mi"
    cpu: "100m"
```

#### Масштабирование

Для изменения количества реплик:

```bash
kubectl scale deployment user-portal --replicas=3
```

### Мониторинг

Проверка логов:
```bash
# Логи всех подов
kubectl logs -l app=user-portal

# Логи конкретного пода
kubectl logs <pod-name>
```

Проверка событий:
```bash
kubectl get events --sort-by=.metadata.creationTimestamp
```

### Обновление

Для обновления приложения:

1. Соберите новый Docker образ с новым тегом
2. Обновите тег образа в `deployment.yaml`
3. Примените изменения:
   ```bash
   kubectl apply -f k8s/deployment.yaml
   ```

Или используйте команду set image:
```bash
kubectl set image deployment/user-portal user-portal=user-portal:new-tag
```