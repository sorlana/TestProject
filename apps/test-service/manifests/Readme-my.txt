2. Настройка переменных окружения для продакшена
Для продакшена рекомендуется использовать:

Sealed Secrets (Kubernetes)

External Secret Operators (AWS Secrets Manager, Azure Key Vault, HashiCorp Vault)

GitHub Secrets (для CI/CD)

3. Обновление ConfigMap
bash
# Применить ConfigMap
kubectl apply -f manifests/configmap.yaml -n testproject-namespace

# Просмотр ConfigMap
kubectl get configmap test-service-config -n testproject-namespace -o yaml

# Обновление значения
kubectl create configmap test-service-config \
  --from-literal=AppSettings__Name="New Name" \
  --dry-run=client -o yaml | kubectl apply -f - -n testproject-namespace
Структура секретов для .NET 10
Формат именования
Секреты используют двойное подчеркивание (__) для разделения секций в соответствии с конфигурацией .NET:

text
Jwt__SecretKey → Jwt:SecretKey в .NET
ConnectionStrings__DefaultConnection → ConnectionStrings:DefaultConnection
Обязательные секреты для .NET
JWT аутентификация:

Jwt__SecretKey - минимум 32 символа

Jwt__Issuer, Jwt__Audience

База данных:

ConnectionStrings__DefaultConnection - строка подключения

Data Protection (для распределённых систем):

DataProtection__ApplicationName

DataProtection__KeyIdentifier

Безопасность
⚠️ ВАЖНО: Никогда не коммитьте реальные секреты в Git!

Файл secret.yaml - это только шаблон

Используйте .gitignore для реальных файлов с секретами

Для локальной разработки используйте generate-secrets.sh

Для продакшена используйте безопасные хранилища секретов

Регулярно ротируйте секретные ключи

Мониторинг
Манифесты включают настройки для:

Prometheus метрик

Health checks

Horizontal Pod Autoscaling на основе метрик .NET приложения

text

## Ключевые особенности для .NET 10:

1. **HPA настроен для .NET метрик**:
   - CPU и memory utilization
   - HTTP requests per second
   - HTTP request duration
   - Поведение масштабирования с задержками

2. **Секреты структурированы для .NET Configuration**:
   - Используют `__` для разделения секций
   - Все основные настройки .NET приложения
   - Поддержка JWT, Data Protection, внешних API

3. **ConfigMap адаптирован для .NET**:
   - ASP.NET Core специфичные переменные
   - OpenTelemetry для метрик
   - Health checks endpoints
   - CORS и Rate Limiting настройки

4. **Добавлены утилиты**:
   - Скрипт генерации секретов
   - Документация для .NET разработчиков
   - Поддержка EF Core миграций

Теперь ваш микросервис на .NET 10 полностью готов к работе с Kubernetes!