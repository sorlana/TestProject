Особенности конфигурации
Проброс портов
80/443 → Ingress Controller

30001 → Приложение (NodePort)

30002 → PostgreSQL

30003 → Grafana

30004 → Prometheus

30005 → Kubernetes Dashboard

Персистентное хранилище
Данные сохраняются в локальные директории:

./data/postgres - Данные PostgreSQL

./data/redis - Данные Redis

./data/prometheus - Метрики Prometheus

./data/grafana - Дашборды Grafana

Сетевые политики
По умолчанию используется CNI от Kind (kindnet).
Для тестирования Network Policies можно установить Calico:

bash
kubectl apply -f https://docs.projectcalico.org/manifests/calico.yaml
Решение проблем
Порты заняты на хосте

Измените порты в kind-cluster.yaml

Или освободите порты: sudo lsof -i :<PORT>

Недостаточно памяти

Увеличьте лимиты Docker (8GB+ RAM recommended)

Уменьшите количество worker нод

Ошибки загрузки образов

Используйте make kind-load-image для локальных образов

Или настройте локальный registry

text

Эта конфигурация обеспечит полноценный локальный Kubernetes кластер для разработки и тестирования вашего микросервиса.