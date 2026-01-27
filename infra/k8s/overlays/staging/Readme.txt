Особенности staging окружения:
1. Изолированный namespace:
Используется отдельный namespace: testproject-namespace-staging

Отдельные ресурсы от production и local

2. Меньшие ресурсы:
PostgreSQL: 1 реплика, 10Gi хранилища

Приложение: 2 реплики, умеренные лимиты ресурсов

3. Отдельные домены:
staging.testproject.com

api.staging.testproject.com

Используется Let's Encrypt staging issuer для сертификатов

4. Настройки для тестирования:
Включен experimental API

Подробное логирование

Ослабленные rate limits

Включены health checks и метрики

5. Тестовые интеграции:
Используются тестовые внешние сервисы (Mailtrap, Stripe test mode)

Application Insights staging

Тестовые данные

6. Мониторинг и алерты:
Специфичные правила для staging

Информационные алерты вместо критических

Отдельные дашборды в Grafana

Использование:
bash
# Применить staging конфигурацию
kustomize build infra/k8s/overlays/staging/ | kubectl apply -f -

# Или использовать kubectl с kustomize
kubectl apply -k infra/k8s/overlays/staging/

# Просмотреть сгенерированные манифесты
kustomize build infra/k8s/overlays/staging/

# Удалить staging окружение
kubectl delete -k infra/k8s/overlays/staging/
Отличия от production:
Ресурсы: Меньше CPU/memory/storage

Реплики: Меньше реплик приложения (2 вместо 3-5)

База данных: Одна реплика, меньший размер

Сертификаты: Let's Encrypt staging вместо production

Домены: staging.* вместо *.com

Интеграции: Тестовые сервисы вместо реальных

Алерты: Более мягкие пороги, информационные алерты

Feature flags: Включены экспериментальные функции

Этот overlay обеспечивает стабильную среду для тестирования, максимально приближенную к production, но с меньшими затратами и безопасностью для экспериментов.