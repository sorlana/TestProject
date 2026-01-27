#!/bin/bash

echo "🚀 Настройка Docker Desktop Kubernetes для TestProject..."

# Проверка Docker Desktop
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker Desktop не запущен"
    echo "   Запустите Docker Desktop и включите Kubernetes:"
    echo "   1. Откройте Docker Desktop"
    echo "   2. Перейдите в Settings > Kubernetes"
    echo "   3. Включите 'Enable Kubernetes'"
    echo "   4. Нажмите 'Apply & Restart'"
    exit 1
fi
echo "✅ Docker Desktop запущен"

# Проверка Kubernetes
if ! kubectl cluster-info > /dev/null 2>&1; then
    echo "❌ Kubernetes не запущен в Docker Desktop"
    echo "   Включите Kubernetes в Docker Desktop:"
    echo "   Settings > Kubernetes > Enable Kubernetes"
    exit 1
fi
echo "✅ Kubernetes запущен"

echo "🔍 Проверяем текущий контекст..."
CURRENT_CONTEXT=$(kubectl config current-context)
echo "   Текущий контекст: $CURRENT_CONTEXT"

# Установка nginx ingress controller для Docker Desktop
echo "📦 Установка nginx ingress controller для Docker Desktop..."
echo "   Применяем манифест ingress-nginx..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

echo "⏳ Ожидаем запуск ingress controller..."
echo "   Это может занять 1-2 минуты..."

# Ждем доступности ingress controller
for i in $(seq 1 30); do
    if kubectl get pods -n ingress-nginx 2>/dev/null | grep -q "Running"; then
        echo "✅ Ingress controller запущен"
        break
    fi
    echo "   Попытка $i/30: ingress controller еще не готов..."
    sleep 5
done

# Проверяем запущенные поды в ingress-nginx
echo "📊 Статус ingress controller:"
kubectl get pods -n ingress-nginx

echo ""
echo "🎉 Настройка Docker Desktop Kubernetes завершена!"
echo ""
echo "📋 Следующие шаги:"
echo "   1. Соберите образ приложения:"
echo "      docker build -t test-service-local:latest ./apps/test-service"
echo ""
echo "   2. Задеплойте приложение:"
echo "      kubectl apply -k infra/k8s/overlays/local"
echo "      или используйте make: make deploy-local"
echo ""
echo "   3. Проверьте статус:"
echo "      kubectl get all -n testproject-namespace"
echo ""
echo "🌐 После деплоя приложение будет доступно:"
echo "   - http://localhost:8080 (через порт-форвардинг)"
echo "   - или через ingress: http://localhost"
echo ""
echo "🛠️  Полезные команды:"
echo "   make k8s-status          - Показать статус Kubernetes"
echo "   make port-forward        - Настроить порт-форвардинг"
echo "   kubectl get ingress -n testproject-namespace"
echo "   kubectl logs -f deployment/test-service -n testproject-namespace"