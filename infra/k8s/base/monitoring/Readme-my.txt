Основные особенности:
1. ServiceMonitor:
Для сбора метрик через сервисы (все поды за сервисом)

Настроены endpoint'ы для .NET метрик, health checks

Добавлены relabelings для улучшения меток

Поддержка метрик из разных источников: .NET приложение, PostgreSQL, Ingress, Kubernetes API

2. PodMonitor:
Для сбора метрик напрямую с подов

Более детальные настройки для .NET приложения

Группировка метрик по категориям: GC, ThreadPool, Memory, JIT, исключения

Поддержка бизнес-метрик и трассировки

3. PrometheusRules:
Recording rules для агрегации метрик

Alerting rules для .NET приложения, PostgreSQL и инфраструктуры

Настроены пороги для предупреждений

4. Grafana Dashboard:
Готовый дашборд для .NET приложения

Панели для мониторинга: доступность, HTTP запросы, задержка, память, исключения, GC

Настраиваемые переменные (service, pod)

Использование:
bash
# Применить все конфигурации мониторинга
kubectl apply -f infra/k8s/base/monitoring/

# Проверить ServiceMonitors
kubectl get servicemonitors -n testproject-namespace

# Проверить PodMonitors
kubectl get podmonitors -n testproject-namespace

# Проверить PrometheusRules
kubectl get prometheusrules -n testproject-namespace

# Проверить Grafana Dashboard ConfigMap
kubectl get configmap test-service-grafana-dashboard -n testproject-namespace -o yaml
Требования для работы:
Prometheus Operator должен быть установлен в кластере

Grafana должна быть установлена и настроена для использования ConfigMap с дашбордами

.NET приложение должно экспортировать метрики в формате Prometheus (используя библиотеки типа prometheus-net или OpenTelemetry)

Для PostgreSQL метрик требуется установить postgres-exporter как sidecar контейнер

Эти конфигурации обеспечат полный мониторинг вашего .NET микросервиса, включая метрики приложения, базы данных и инфраструктуры.