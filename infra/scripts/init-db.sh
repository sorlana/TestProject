#!/bin/bash
# Скрипт инициализации базы данных для TestProject
# Поддерживает: локальный PostgreSQL, Yandex Managed PostgreSQL, Docker

set -euo pipefail

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Логирование
log_info() { echo -e "${BLUE}[INFO]${NC} $*"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $*"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $*"; }
log_error() { echo -e "${RED}[ERROR]${NC} $*" >&2; }

# Параметры по умолчанию
ENVIRONMENT=${ENVIRONMENT:-"development"}
DB_TYPE=${DB_TYPE:-"local"}  # local, managed, docker
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-"testdb"}
DB_USER=${DB_USER:-"postgres"}
DB_PASSWORD=${DB_PASSWORD:-""}
SSL_MODE=${SSL_MODE:-"disable"}
CA_CERT_PATH=${CA_CERT_PATH:-""}
MIGRATIONS_DIR=${MIGRATIONS_DIR:-"./migrations"}
SCHEMAS_DIR=${SCHEMAS_DIR:-"./schemas"}
SEED_DATA_DIR=${SEED_DATA_DIR:-"./seed-data"}
BACKUP_ON_INIT=${BACKUP_ON_INIT:-"false"}
VERBOSE=${VERBOSE:-"false"}

# Функция для проверки зависимостей
check_dependencies() {
    local dependencies=("psql")
    
    for dep in "${dependencies[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            log_error "Требуется $dep, но не установлен"
            exit 1
        fi
    done
    
    log_success "Все зависимости установлены"
}

# Функция для проверки подключения к БД
check_db_connection() {
    log_info "Проверка подключения к базе данных..."
    
    local check_sql="SELECT 1;"
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                -q > /dev/null 2>&1
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                -q > /dev/null 2>&1
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "postgres" \
            -c "$check_sql" \
            -q > /dev/null 2>&1
    fi
    
    if [ $? -eq 0 ]; then
        log_success "Подключение к базе данных успешно"
        return 0
    else
        log_error "Не удалось подключиться к базе данных"
        return 1
    fi
}

# Функция для создания базы данных
create_database() {
    log_info "Создание базы данных '$DB_NAME'..."
    
    local create_db_sql="CREATE DATABASE $DB_NAME 
        WITH 
        OWNER = $DB_USER
        ENCODING = 'UTF8'
        LC_COLLATE = 'C'
        LC_CTYPE = 'C'
        TEMPLATE = template0
        CONNECTION LIMIT = -1;"
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$create_db_sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH"
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$create_db_sql" \
                --set=sslmode="$SSL_MODE"
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "postgres" \
            -c "$create_db_sql"
    fi
    
    if [ $? -eq 0 ]; then
        log_success "База данных создана"
    else
        log_warning "База данных уже существует или произошла ошибка"
    fi
}

# Функция для создания расширений
create_extensions() {
    log_info "Создание расширений PostgreSQL..."
    
    local extensions_sql="
        CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";
        CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";
        CREATE EXTENSION IF NOT EXISTS \"pg_stat_statements\";
        CREATE EXTENSION IF NOT EXISTS \"btree_gin\";
        CREATE EXTENSION IF NOT EXISTS \"unaccent\";
    "
    
    execute_sql "$extensions_sql"
    log_success "Расширения созданы"
}

# Функция для создания схем
create_schemas() {
    log_info "Создание схем базы данных..."
    
    if [ -d "$SCHEMAS_DIR" ]; then
        for schema_file in "$SCHEMAS_DIR"/*.sql; do
            if [ -f "$schema_file" ]; then
                log_info "Применение схемы: $(basename "$schema_file")"
                execute_sql_file "$schema_file"
            fi
        done
        log_success "Схемы созданы"
    else
        log_warning "Директория схем не найдена: $SCHEMAS_DIR"
    fi
}

# Функция для применения миграций
apply_migrations() {
    log_info "Применение миграций..."
    
    if [ -d "$MIGRATIONS_DIR" ]; then
        # Проверяем наличие таблицы миграций
        local check_migrations_table="
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = '__efmigrationshistory'
            );
        "
        
        local migrations_exist=$(execute_sql_query "$check_migrations_table")
        
        # Применяем миграции в порядке
        for migration_file in $(ls "$MIGRATIONS_DIR"/*.sql | sort -V); do
            if [ -f "$migration_file" ]; then
                local migration_name=$(basename "$migration_file" .sql)
                
                # Проверяем, применена ли уже миграция
                if [[ "$migrations_exist" =~ t|true ]]; then
                    local check_migration="
                        SELECT EXISTS (
                            SELECT 1 FROM public.__efmigrationshistory 
                            WHERE migrationid = '$migration_name'
                        );
                    "
                    local is_applied=$(execute_sql_query "$check_migration")
                    
                    if [[ "$is_applied" =~ t|true ]]; then
                        log_info "Миграция уже применена: $migration_name"
                        continue
                    fi
                fi
                
                log_info "Применение миграции: $migration_name"
                execute_sql_file "$migration_file"
                
                # Записываем в историю миграций
                local record_migration="
                    INSERT INTO public.__efmigrationshistory (migrationid, productversion)
                    VALUES ('$migration_name', '$(dotnet --version 2>/dev/null || echo "8.0.0")');
                "
                execute_sql "$record_migration"
            fi
        done
        
        log_success "Миграции применены"
    else
        log_warning "Директория миграций не найдена: $MIGRATIONS_DIR"
    fi
}

# Функция для загрузки тестовых данных
load_seed_data() {
    log_info "Загрузка тестовых данных..."
    
    if [ -d "$SEED_DATA_DIR" ]; then
        for seed_file in "$SEED_DATA_DIR"/*.sql; do
            if [ -f "$seed_file" ]; then
                log_info "Загрузка данных: $(basename "$seed_file")"
                execute_sql_file "$seed_file"
            fi
        done
        log_success "Тестовые данные загружены"
    else
        log_warning "Директория тестовых данных не найдена: $SEED_DATA_DIR"
    fi
}

# Функция для создания бэкапа перед инициализацией
create_backup() {
    if [ "$BACKUP_ON_INIT" = "true" ]; then
        log_info "Создание резервной копии перед инициализацией..."
        
        local timestamp=$(date +%Y%m%d_%H%M%S)
        local backup_file="./backups/backup_before_init_${timestamp}.sql.gz"
        
        mkdir -p ./backups
        
        PGPASSWORD="$DB_PASSWORD" pg_dump \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            --clean \
            --if-exists \
            | gzip -9 > "$backup_file"
        
        if [ $? -eq 0 ]; then
            log_success "Резервная копия создана: $backup_file"
        else
            log_warning "Не удалось создать резервную копию"
        fi
    fi
}

# Функция для выполнения SQL запроса
execute_sql() {
    local sql="$1"
    
    if [ "$VERBOSE" = "true" ]; then
        log_info "Выполнение SQL: ${sql:0:100}..."
    fi
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                --set=ON_ERROR_STOP=1
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                --set=ON_ERROR_STOP=1
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -c "$sql" \
            --set=ON_ERROR_STOP=1
    fi
}

# Функция для выполнения SQL из файла
execute_sql_file() {
    local sql_file="$1"
    
    if [ "$VERBOSE" = "true" ]; then
        log_info "Выполнение SQL из файла: $sql_file"
    fi
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -f "$sql_file" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                --set=ON_ERROR_STOP=1
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -f "$sql_file" \
                --set=sslmode="$SSL_MODE" \
                --set=ON_ERROR_STOP=1
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -f "$sql_file" \
            --set=ON_ERROR_STOP=1
    fi
}

# Функция для выполнения SQL запроса с возвратом результата
execute_sql_query() {
    local sql="$1"
    local result
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                -t -A 2>/dev/null)
        else
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                -t -A 2>/dev/null)
        fi
    else
        result=$(PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -c "$sql" \
            -t -A 2>/dev/null)
    fi
    
    echo "$result"
}

# Функция для вывода статистики
show_statistics() {
    log_info "Сбор статистики базы данных..."
    
    local stats_sql="
        SELECT 
            'База данных' as metric, 
            '$DB_NAME' as value
        UNION ALL
        SELECT 
            'Размер', 
            pg_size_pretty(pg_database_size('$DB_NAME'))
        UNION ALL
        SELECT 
            'Количество таблиц', 
            COUNT(*)::text 
        FROM information_schema.tables 
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
        UNION ALL
        SELECT 
            'Миграции', 
            COUNT(*)::text 
        FROM public.__efmigrationshistory;
    "
    
    local result=$(execute_sql_query "$stats_sql")
    
    echo -e "\n${GREEN}📊 Статистика базы данных:${NC}"
    echo "=============================="
    echo "$result" | column -t -s '|'
    echo "=============================="
}

# Основная функция
main() {
    log_info "🚀 Начало инициализации базы данных"
    log_info "Окружение: $ENVIRONMENT"
    log_info "Тип БД: $DB_TYPE"
    log_info "Хост: $DB_HOST"
    log_info "База данных: $DB_NAME"
    
    # Проверяем зависимости
    check_dependencies
    
    # Проверяем подключение
    if ! check_db_connection; then
        log_error "Не удалось подключиться к базе данных. Проверьте параметры подключения."
        exit 1
    fi
    
    # Создаем бэкап перед изменениями
    create_backup
    
    # Создаем базу данных (если не существует)
    create_database
    
    # Создаем расширения
    create_extensions
    
    # Создаем схемы
    create_schemas
    
    # Применяем миграции
    apply_migrations
    
    # Загружаем тестовые данные (для development/staging)
    if [ "$ENVIRONMENT" != "production" ]; then
        load_seed_data
    fi
    
    # Выводим статистику
    show_statistics
    
    log_success "✅ Инициализация базы данных завершена успешно!"
}

# Обработка аргументов командной строки
while [[ $# -gt 0 ]]; do
    case $1 in
        --environment|-e)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --db-type|-t)
            DB_TYPE="$2"
            shift 2
            ;;
        --host|-h)
            DB_HOST="$2"
            shift 2
            ;;
        --port|-p)
            DB_PORT="$2"
            shift 2
            ;;
        --name|-n)
            DB_NAME="$2"
            shift 2
            ;;
        --user|-u)
            DB_USER="$2"
            shift 2
            ;;
        --password|-P)
            DB_PASSWORD="$2"
            shift 2
            ;;
        --ssl-mode)
            SSL_MODE="$2"
            shift 2
            ;;
        --ca-cert)
            CA_CERT_PATH="$2"
            shift 2
            ;;
        --migrations-dir)
            MIGRATIONS_DIR="$2"
            shift 2
            ;;
        --schemas-dir)
            SCHEMAS_DIR="$2"
            shift 2
            ;;
        --seed-data-dir)
            SEED_DATA_DIR="$2"
            shift 2
            ;;
        --backup-on-init)
            BACKUP_ON_INIT="true"
            shift
            ;;
        --verbose|-v)
            VERBOSE="true"
            shift
            ;;
        --help)
            echo "Использование: $0 [опции]"
            echo ""
            echo "Опции:"
            echo "  -e, --environment      Окружение (development, staging, production)"
            echo "  -t, --db-type          Тип БД (local, managed, docker)"
            echo "  -h, --host             Хост БД"
            echo "  -p, --port             Порт БД"
            echo "  -n, --name             Имя базы данных"
            echo "  -u, --user             Пользователь БД"
            echo "  -P, --password         Пароль БД"
            echo "  --ssl-mode             Режим SSL (disable, require, verify-full)"
            echo "  --ca-cert              Путь к CA сертификату"
            echo "  --migrations-dir       Директория с миграциями"
            echo "  --schemas-dir          Директория со схемами"
            echo "  --seed-data-dir        Директория с тестовыми данными"
            echo "  --backup-on-init       Создать бэкап перед инициализацией"
            echo "  -v, --verbose          Подробный вывод"
            echo "  --help                 Показать эту справку"
            exit 0
            ;;
        *)
            log_error "Неизвестный аргумент: $1"
            exit 1
            ;;
    esac
done

# Запуск основной функции
main

# Примеры использования:
# 1. Локальная разработка:
#    ./scripts/init-db.sh --environment development --db-type local \
#         --host localhost --user postgres --password postgres
#
# 2. Yandex Managed PostgreSQL:
#    ./scripts/init-db.sh --environment staging --db-type managed \
#         --host your-postgres-host.yandexcloud.net --port 6432 \
#         --user appuser --password your-password --ssl-mode verify-full \
#         --ca-cert ./certs/yandex-ca.pem
#
# 3. Kubernetes PostgreSQL:
#    ./scripts/init-db.sh --environment production --db-type kubernetes \
#         --host postgres.testproject-namespace.svc.cluster.local \
#         --user postgres --password $(kubectl get secret postgres-secrets -o jsonpath='{.data.POSTGRES_PASSWORD}' | base64 -d)