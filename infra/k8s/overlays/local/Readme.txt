Ключевые особенности локального overlay:
1. Упрощенные ресурсы:
PostgreSQL: уменьшенные лимиты памяти, отключена репликация

Приложение: 1 реплика, уменьшенные ресурсы

Отключены бэкапы и мониторинг высокой детализации

2. Настройки для разработки:
Включен режим Development для .NET

Подробное логирование

Включен Swagger и детальные ошибки

Упрощенные CORS настройки

3. Hot reload:
Использование dotnet watch для автоматической перекомпиляции

Volume mounts для исходного кода

Локальный сбор Docker образа

4. Локальный доступ:
NodePort для прямого доступа

Port forwarding через kubectl

Локальный домен test.local

5. Безопасность:
Упрощенные секреты для разработки

Отключен SSL/TLS

Разрешенный доступ из любых источников

Использование:
bash
# Инициализация локального кластера (если используете Minikube)
minikube start
minikube addons enable ingress

# Применение локальной конфигурации
kustomize build infra/k8s/overlays/local/ | kubectl apply -f -

# Или используя make
make deploy
make port-forward

# Доступ к приложению
# - Через NodePort: http://localhost:30080
# - Через Ingress: http://test.local
# - Swagger UI: http://localhost:30080/swagger
# - Health checks: http://localhost:30080/health
Этот overlay оптимизирован для локальной разработки, обеспечивая быстрый запуск и удобную отладку .NET приложения с использованием hot reload и упрощенной конфигурации.