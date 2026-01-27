1. StatefulSet:
Использует StatefulSet для гарантии стабильных сетевых идентификаторов

Настроены security context для запуска от непривилегированного пользователя

Добавлены readiness и liveness пробы

Настроены resource limits

2. Service:
Headless service для StatefulSet

Read-only service для реплик (если добавим)

publishNotReadyAddresses: true для корректной работы StatefulSet

3. PVC:
Основной PVC для данных

PVC для бэкапов

PVC для WAL файлов

4. ConfigMap:
Детальные настройки PostgreSQL

Init скрипты для инициализации базы

Конфигурационный файл PostgreSQL

Отдельные скрипты для мониторинга

5. Secret (шаблон):
Все пароли в одном месте

Connection strings для .NET приложения

Подготовлено для SSL/TLS

6. Backup Job:
Ежедневные бэкапы

Проверка контрольных сумм

Очистка старых бэкапов

Скрипты для восстановления

Логирование операций бэкапа

Особенности для .NET:
Оптимизированные настройки для работы с Entity Framework Core

Поддержка UUID через расширение uuid-ossp

Connection strings в формате .NET

Настройки для Npgsql (поставщик данных для PostgreSQL в .NET)

Использование:
bash
# Применить все конфигурации PostgreSQL
kubectl apply -f infra/k8s/base/postgres/

# Проверить состояние
kubectl get statefulset,svc,pvc,pod -n testproject-namespace -l app=postgres

# Посмотреть логи PostgreSQL
kubectl logs -f statefulset/postgres -n testproject-namespace

# Ручной запуск бэкапа
kubectl create job --from=cronjob/postgres-backup manual-backup-$(date +%s) -n testproject-namespace

# Подключиться к базе
kubectl exec -it postgres-0 -n testproject-namespace -- psql -U postgres -d testdb