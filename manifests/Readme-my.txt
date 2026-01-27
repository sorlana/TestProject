Применение отдельных компонентов:
bash
# Только namespace
kubectl apply -f manifests/namespace.yaml

# Только RBAC
kubectl apply -f manifests/service-account.yaml
kubectl apply -f manifests/cluster-role.yaml
kubectl apply -f manifests/cluster-role-binding.yaml
Проверка прав:
bash
# Проверить права сервисного аккаунта
kubectl auth can-i --as=system:serviceaccount:testproject-namespace:testproject-service-account \
  get pods -n testproject-namespace

# Проверить все права
kubectl auth can-i --list --as=system:serviceaccount:testproject-namespace:testproject-service-account
🛠️ Настройка для окружений
Разработка (development):
Все права включены для удобства отладки

Возможность создавать и удалять ресурсы

Продакшен (production):
Минимальные необходимые права (principle of least privilege)

Запрет на удаление критичных ресурсов

Аудит доступа через аннотации

🔒 Безопасность
Рекомендации:
Принцип минимальных прав - давать только необходимые права

Регулярный аудит - проверять используемые права

Разделение обязанностей - разные сервисные аккаунты для разных задач

Мониторинг доступа - логировать все операции с критичными ресурсами

Безопасные настройки:
yaml
automountServiceAccountToken: false  # Отключать автомонтирование токена где не нужно
📊 Мониторинг RBAC
Просмотр привязок:
bash
# Все ClusterRoleBinding
kubectl get clusterrolebindings -l app.kubernetes.io/name=testproject

# Все ClusterRole
kubectl get clusterroles -l app.kubernetes.io/name=testproject

# Все ServiceAccount
kubectl get serviceaccounts -n testproject-namespace -l app.kubernetes.io/name=testproject
Логирование доступа:
Включить аудит в Kubernetes кластере для отслеживания всех операций с RBAC.

🔄 Обновление
При изменении требований к безопасности:

Обновите соответствующие роли в cluster-role.yaml

Протестируйте изменения в development среде

Примените изменения в production через процесс Code Review

🆘 Устранение неполадок
Проблема: "Unauthorized" ошибки
bash
# Проверить токен сервисного аккаунта
kubectl describe serviceaccount testproject-service-account -n testproject-namespace

# Проверить привязки
kubectl describe clusterrolebinding testproject-app-binding

# Проверить логи API сервера
kubectl logs -n kube-system -l component=kube-apiserver --tail=100
Проблема: Недостаточно прав
bash
# Проверить текущие права
kubectl auth can-i --list --as=system:serviceaccount:testproject-namespace:testproject-service-account

# Увеличить права (временно для отладки)
# Обновите cluster-role.yaml и перепримените
📞 Поддержка
Для вопросов по безопасности и правам доступа обращайтесь к команде DevOps.

text

## 🚀 **Как использовать эти манифесты:**

### **1. Создать папку и файлы:**
```bash
mkdir -p manifests
cd manifests

# Создать основные файлы
touch service-account.yaml cluster-role.yaml cluster-role-binding.yaml namespace.yaml kustomization.yaml README.md
2. Применить манифесты:
bash
# Способ 1: Через Kustomize
kubectl apply -k manifests/

# Способ 2: По отдельности
kubectl apply -f manifests/namespace.yaml
kubectl apply -f manifests/service-account.yaml
kubectl apply -f manifests/cluster-role.yaml
kubectl apply -f manifests/cluster-role-binding.yaml
3. Проверить созданные ресурсы:
bash
# Проверить namespace
kubectl get namespace testproject-namespace

# Проверить сервисные аккаунты
kubectl get serviceaccounts -n testproject-namespace

# Проверить роли
kubectl get clusterroles -l app.kubernetes.io/name=testproject

# Проверить привязки
kubectl get clusterrolebindings -l app.kubernetes.io/name=testproject
4. Использовать сервисный аккаунт в Deployment:
yaml
# В deployment.yaml для test-service
apiVersion: apps/v1
kind: Deployment
metadata:
  name: test-service
  namespace: testproject-namespace
spec:
  template:
    spec:
      serviceAccountName: testproject-service-account  # Используем наш SA
      containers:
      - name: test-service
        image: test-service:latest
📊 Схема RBAC для TestProject:
text
┌─────────────────────────────────────────┐
│         ClusterRole (Права)             │
│  • testproject-app-role                 │
│  • testproject-backup-role              │
│  • testproject-migration-role           │
│  • testproject-monitoring-role          │
└───────────────┬─────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────┐
│     ClusterRoleBinding (Привязки)       │
│  • testproject-app-binding              │
│  • testproject-backup-binding           │
│  • testproject-migration-binding        │
│  • testproject-monitoring-binding       │
└───────────────┬─────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────┐
│     ServiceAccount (Аккаунты)           │
│  • testproject-service-account          │
│  • testproject-backup-service-account   │
│  • testproject-migration-service-account│
│  • testproject-monitoring-service-account│
└───────────────┬─────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────┐
│        Pods (Приложения)                │
│  • test-service (использует SA)         │
│  • backup-job (использует SA)           │
│  • migration-job (использует SA)        │
│  • prometheus (использует SA)           │
└─────────────────────────────────────────┘
Теперь у вас есть полная система RBAC для TestProject с безопасным разделением прав! 🔐🚀