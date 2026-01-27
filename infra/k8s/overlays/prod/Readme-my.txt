Ключевые особенности продакшена:
1. Managed база данных:
Используется Yandex Managed PostgreSQL вместо локального StatefulSet

SSL/TLS соединения с проверкой сертификатов

Чтение реплик для распределения нагрузки

Автоматические бэкапы и обновления от Yandex Cloud

2. Профессиональный SSL/TLS:
Let's Encrypt production сертификаты

Автоматическое обновление через cert-manager

Современные cipher suites (TLS 1.2/1.3)

HSTS с preload

Безопасные заголовки (CSP, X-Frame-Options и т.д.)

3. Высокая доступность:
3+ реплики приложения

Horizontal Pod Autoscaler

Pod Anti-Affinity для распределения по нодам

Readiness и Liveness пробы

4. Безопасность:
ReadOnlyRootFilesystem

Non-root пользователь

Dropped capabilities

RuntimeDefault seccomp профиль

Network policies

WAF и DDoS защита через Yandex Cloud

5. Мониторинг и алерты:
Детализированные метрики

Production алерты

Интеграция с Yandex Cloud Monitoring

Audit logging с 365 дней хранения

6. Инфраструктура как код:
Переменные окружения для чувствительных данных

Sealed Secrets для шифрования

ExternalDNS для управления DNS

Все настройки через GitOps

Использование:
bash
# Применение продакшен конфигурации
kustomize build infra/k8s/overlays/prod/ \
  | kubectl apply -f -

# С переменными окружения
export YC_MANAGED_POSTGRES_HOST="your-host"
export YC_MANAGED_POSTGRES_PASSWORD="your-password"
export IMAGE_TAG="v1.0.0"

kustomize build infra/k8s/overlays/prod/ \
  --load_restrictor none \
  | kubectl apply -f -

# Просмотр сгенерированных манифестов
kustomize build infra/k8s/overlays/prod/ --load_restrictor none

# Удаление
kubectl delete -k infra/k8s/overlays/prod/
Требования перед применением:
Настроен кластер Yandex Managed Kubernetes

Создан Yandex Managed PostgreSQL

Настроен Container Registry в Yandex Cloud

Установлен cert-manager и внешний DNS

Созданы необходимые секреты в Kubernetes

Настроены домены и DNS записи

Этот overlay обеспечивает профессиональное, безопасное и масштабируемое продакшен окружение для .NET микросервиса с использованием лучших практик Kubernetes и облачных сервисов Yandex Cloud.

