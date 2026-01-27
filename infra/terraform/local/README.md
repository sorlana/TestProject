# Локальная разработка с Kind

## Требования
- Docker 20.10+
- Kind 0.20+
- kubectl 1.27+
- Helm 3.12+ (опционально)

## Быстрый старт

```bash
# 1. Создать кластер
make kind-create

# 2. Развернуть приложение
make deploy-local

# 3. Проверить статус
kubectl get all -n testproject

# 4. Удалить кластер
make kind-delete