.PHONY: help setup build up down logs clean backup restore list-volumes

help:
	@echo "Доступные команды для TestProject:"
	@echo "  make setup          - Настройка окружения проекта"
	@echo "  make build          - Сборка образа test-service"
	@echo "  make up             - Запуск всех сервисов (с томами)"
	@echo "  make down           - Остановка сервисов (томы сохраняются)"
	@echo "  make down-clean     - Остановка с удалением томов (ПОТЕРЯ ДАННЫХ!)"
	@echo "  make logs           - Просмотр логов"
	@echo "  make psql           - Подключение к БД"
	@echo "  make backup         - Создание бэкапа БД"
	@echo "  make restore        - Восстановление БД из бэкапа"
	@echo "  make list-volumes   - Показать тома проекта"
	@echo "  make clean          - Полная очистка проекта"
	@echo "  make setup-k8s      - Настройка Docker Desktop Kubernetes"
	@echo "  make deploy-local   - Деплой в локальный Kubernetes (Docker Desktop)"
	@echo "  make k8s-status     - Показать статус Kubernetes"

setup:
	@echo "Настройка TestProject..."
	@cp .env.example .env || echo "Файл .env уже существует"
	@echo "? Отредактируйте файл .env, изменив пароли!"
	@mkdir -p backups
	@echo "? Папка для бэкапов создана"

build:
	@echo "Сборка образа test-service..."
	@cd apps/test-service && docker build -t test-service:latest .
	@echo "? Образ собран"

up:
	@echo "Запуск TestProject..."
	@docker-compose up -d
	@echo "? TestProject запущен!"
	@echo "?? Test Service: http://localhost:4000"
	@echo "???  PostgreSQL: localhost:5432 (пользователь: test_user, БД: testdb)"
	@echo "?? PGAdmin: http://localhost:8080 (логин: admin@testproject.com / admin123)"
	@echo "?? Redis: localhost:6379"

down:
	@echo "Остановка сервисов (томы сохраняются)..."
	@docker-compose down
	@echo "? Сервисы остановлены, данные сохранены в томах"

down-clean:
	@echo "??  ВНИМАНИЕ: Будут удалены все тома с данными!"
	@read -p "Вы уверены? (y/N): " confirm && [ $$confirm = y ] || exit 1
	@docker-compose down -v
	@echo "? Сервисы остановлены, тома удалены"

logs:
	@echo "Логи test-service:"
	@docker-compose logs -f test-service

logs-db:
	@echo "Логи PostgreSQL:"
	@docker-compose logs -f postgres-test

psql:
	@echo "Подключение к PostgreSQL..."
	@docker-compose exec postgres-test psql -U test_user -d testdb

backup:
	@echo "Создание бэкапа базы данных testdb..."
	@mkdir -p backups
	@docker-compose exec postgres-test pg_dump -U test_user testdb > backups/testdb_backup_$(date +%Y%m%d_%H%M%S).sql
	@echo "? Бэкап создан: backups/testdb_backup_*.sql"

restore:
	@echo "Восстановление базы данных из последнего бэкапа..."
	@latest_backup=$$(ls -t backups/testdb_backup_*.sql 2>/dev/null | head -1) && \
	if [ -z "$$latest_backup" ]; then \
		echo "? Бэкапы не найдены"; \
	else \
		echo "Восстанавливаем из: $$latest_backup"; \
		docker-compose exec -T postgres-test psql -U test_user testdb < $$latest_backup; \
		echo "? База данных восстановлена"; \
	fi

list-volumes:
	@echo "Тома TestProject:"
	@docker volume ls --filter "name=testproject" --format "table {{.Name}}\t{{.Driver}}\t{{.Mountpoint}}"
	@echo ""
	@echo "Использование томов:"
	@docker system df -v

clean:
	@echo "Полная очистка TestProject..."
	@docker-compose down -v
	@docker system prune -f
	@docker volume prune -f
	@echo "? Очистка завершена"

# Команды для управления данными
inspect-volumes:
	@echo "Информация о томах:"
	@docker volume inspect testproject-postgres-data testproject-app-logs testproject-redis-data 2>/dev/null || echo "Тома не найдены"

volume-size:
	@echo "Размеры томов:"
	@docker run --rm -v testproject-postgres-data:/data alpine sh -c "du -sh /data 2>/dev/null || echo 'Том пуст'"
	@docker run --rm -v testproject-app-logs:/data alpine sh -c "du -sh /data 2>/dev/null || echo 'Том пуст'"

# Проверка сохранности данных
check-data:
	@echo "Проверка сохранности данных..."
	@echo "1. Проверяем PostgreSQL:"
	@docker-compose exec postgres-test psql -U test_user -d testdb -c "SELECT COUNT(*) FROM courses;" || echo "Таблица courses не существует"
	@echo "2. Проверяем Redis:"
	@docker-compose exec redis-test redis-cli -a redis123 ping || echo "Redis недоступен"

# Настройка Docker Desktop Kubernetes
setup-k8s:
	@echo "?? Настройка Docker Desktop Kubernetes для TestProject..."
	@echo "1. Проверяем Docker Desktop..."
	@if ! docker info > /dev/null 2>&1; then \
		echo "❌ Docker Desktop не запущен"; \
		echo "   Запустите Docker Desktop и включите Kubernetes:"; \
		echo "   Settings > Kubernetes > Enable Kubernetes"; \
		exit 1; \
	fi
	@echo "✅ Docker Desktop запущен"
	
	@echo "2. Проверяем Kubernetes..."
	@if ! kubectl cluster-info > /dev/null 2>&1; then \
		echo "❌ Kubernetes не запущен в Docker Desktop"; \
		echo "   Включите Kubernetes в Docker Desktop:"; \
		echo "   Settings > Kubernetes > Enable Kubernetes"; \
		exit 1; \
	fi
	@echo "✅ Kubernetes запущен"
	
	@echo "3. Устанавливаем nginx ingress controller..."
	@kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml 2>/dev/null || echo "Ingress controller уже установлен"
	
	@echo "4. Ожидаем запуск ingress controller..."
	@kubectl wait --namespace ingress-nginx \
		--for=condition=ready pod \
		--selector=app.kubernetes.io/component=controller \
		--timeout=120s 2>/dev/null || echo "Пропускаем ожидание"
	
	@echo "?? Настройка завершена!"
	@echo "?? Для деплоя выполните: make deploy-local"

# Показать статус Kubernetes
k8s-status:
	@echo "📊 Статус Docker Desktop Kubernetes:"
	@echo "Kubernetes контекст:"
	@kubectl config current-context
	@echo ""
	@echo "Ноды:"
	@kubectl get nodes
	@echo ""
	@echo "Пространства имен:"
	@kubectl get namespaces
	@echo ""
	@echo "Ingress controller:"
	@kubectl get pods -n ingress-nginx

# Деплой в локальный Kubernetes (Docker Desktop)
deploy-local:
	@echo "🚀 Деплой TestProject в Docker Desktop Kubernetes..."
	@echo "1. Применяем манифесты..."
	@kubectl apply -k infra/k8s/overlays/local
	@echo ""
	@echo "2. Ожидаем запуск подов..."
	@kubectl wait --for=condition=ready pod -l app=test-service -n testproject-namespace --timeout=300s
	@echo ""
	@echo "✅ Деплой завершен!"
	@echo ""
	@echo "🌐 Точки доступа:"
	@echo "   Приложение:    http://localhost:8080"
	@echo "   Health check:  http://localhost:8080/health"
	@echo "   Swagger UI:    http://localhost:8080/swagger"
	@echo "   Метрики:       http://localhost:8080/metrics"
	@echo ""
	@echo "📊 Проверить состояние:"
	@echo "   kubectl get all -n testproject-namespace"
	@echo "   kubectl get ingress -n testproject-namespace"

# Очистка Kubernetes
k8s-clean:
	@echo "🧹 Очистка Kubernetes развертывания..."
	@kubectl delete -k infra/k8s/overlays/local --ignore-not-found
	@echo "✅ Очистка завершена"

# Порт-форвардинг для доступа к сервисам
port-forward:
	@echo "🔗 Настройка порт-форвардинга..."
	@echo "1. Test Service (порт 8080)..."
	@kubectl port-forward svc/test-service 8080:8080 -n testproject-namespace &
	@echo "2. PostgreSQL (порт 5432)..."
	@kubectl port-forward svc/postgres 5432:5432 -n testproject-namespace &
	@echo ""
	@echo "🌐 Точки доступа:"
	@echo "   Приложение: http://localhost:8080"
	@echo "   PostgreSQL: localhost:5432"
	@echo ""
	@echo "Для остановки: pkill -f 'kubectl port-forward'"