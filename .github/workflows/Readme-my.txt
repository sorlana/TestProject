🚀 Как настроить GitHub Secrets для workflow:
bash
# Необходимые секреты для deploy-prod.yaml:

# Yandex Cloud
YC_FOLDER_ID="ваш-folder-id"
YC_CLOUD_ID="ваш-cloud-id"
YC_SERVICE_ACCOUNT_KEY='{"id":"...","service_account_id":"...","private_key":"..."}'
YC_REGISTRY_ID="ваш-registry-id"
YC_K8S_CLUSTER_ID="ваш-cluster-id"
YC_DB_CLUSTER_ID="ваш-db-cluster-id"

# База данных
YC_DB_PASSWORD="ваш-пароль-бд"
PRODUCTION_JWT_SECRET="ваш-секрет-jwt"

# Мониторинг и уведомления
SLACK_WEBHOOK_URL="https://hooks.slack.com/..."
GRAFANA_URL="https://grafana.example.com"
GRAFANA_API_KEY="ваш-api-key"

# Как добавить секреты в GitHub:
# 1. Перейдите в Settings → Secrets and variables → Actions
# 2. Нажмите "New repository secret"
# 3. Добавьте каждый секрет с именем и значением
📁 Структура после создания:
text
.github/
├── workflows/
│   ├── deploy-local.yaml      # CI для локального k8s
│   └── deploy-prod.yaml       # CD для Yandex Cloud
├── ISSUE_TEMPLATE/
│   ├── bug_report.md          # Шаблон для багов
│   └── feature_request.md     # Шаблон для фич
├── PULL_REQUEST_TEMPLATE.md   # Шаблон для PR
├── dependabot.yml             # Автообновление зависимостей
└── CODEOWNERS                 # Владельцы кода (опционально)
🚀 Как начать использовать:
1. Создайте папку и файлы:
bash
mkdir -p .github/{workflows,ISSUE_TEMPLATE}
touch .github/{PULL_REQUEST_TEMPLATE.md,dependabot.yml}
touch .github/workflows/{deploy-local.yaml,deploy-prod.yaml}
touch .github/ISSUE_TEMPLATE/{bug_report.md,feature_request.md}
2. Настройте секреты в GitHub:
Перейдите в ваш репозиторий на GitHub

Settings → Secrets and variables → Actions

Добавьте необходимые секреты (см. выше)

3. Запустите workflow:
При пуше в develop запустится deploy-local.yaml

При пуше в main запустится deploy-prod.yaml

Вручную можно запустить через Actions → Workflow

Теперь у вас есть полная CI/CD система для TestProject! 🚀