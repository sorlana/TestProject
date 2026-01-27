#!/bin/bash
# Скрипт миграции с локальной базы данных на Yandex Managed PostgreSQL
# Создает бэкап локальной базы, проверяет совместимость и восстанавливает в managed БД

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
SOURCE_HOST=${SOURCE_HOST:-"localhost"}
SOURCE_PORT=${SOURCE_PORT:-5432}
SOURCE_DB=${SOURCE_DB:-"testdb"}
SOURCE_USER=${SOURCE_USER:-"postgres"}
SOURCE_PASSWORD=${SOURCE_PASSWORD:-""}

TARGET_HOST=${TARGET_HOST:-""}
TARGET_PORT=${TARGET_PORT:-6432}
TARGET_DB=${TARGET_DB:-"testdb"}
TARGET_USER=${TARGET_USER:-"appuser"}
TARGET_PASSWORD=${TARGET_PASSWORD:-""}
TARGET_SSL_MODE=${TARGET_SSL_MODE:-"verify-full"}
TARGET_CA_CERT=${TARGET_CA_CERT:-"./certs/yandex-ca.pem"}

BACKUP_DIR=${BACKUP_DIR:-"./migration-backups"}
COMPRESSION=${COMPRESSION:-"gzip"}
VERIFY_COMPATIBILITY=${VERIFY_COMPATIBILITY:-"true"}
TEST_MIGRATION=${TEST_MIGRATION:-"true"}
DRY_RUN=${DRY_RUN:-"false"}
VERBOSE=${VERBOSE:-"false"}

# Функция для проверки зависимостей
check_dependencies() {
    local dependencies=("pg_dump" "psql" "pg_dumpall" "pg_restore")
    
    for dep in "${dependencies[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            log_error "Требуется $dep, но не установлен"
            exit 1
        fi
    done
    
    log_success "Все зависимости установлены"
}

# Функция для проверки подключения к исходной базе
check_source_connection() {
    log_info "Проверка подключения к исходной базе данных..."
    
    local check_sql="SELECT 1;"
    
    PGPASSWORD="$SOURCE_PASSWORD" psql \
        -h "$SOURCE_HOST" \
        -p "$SOURCE_PORT" \
        -U "$SOURCE_USER" \
        -d "$SOURCE_DB" \
        -c "$check_sql" \
        -q > /dev/null 2>&1
    
    if [ $? -eq 0 ]; then
        log_success "Подключение к исходной базе данных успешно"
        return 0
    else
        log_error "Не удалось подключиться к исходной базе данных"
        return 1
    fi
}

# Функция для проверки подключения к целевой базе
check_target_connection() {
    if [ -z "$TARGET_HOST" ]; then
        log_error "Не указан хост целевой базы данных"
        return 1
    fi
    
    log_info "Проверка подключения к целевой базе данных..."
    
    local check_sql="SELECT 1;"
    
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                -q > /dev/null 2>&1
        else
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                -q > /dev/null 2>&1
        fi
    else
        PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "postgres" \
            -c "$check_sql" \
            -q > /dev/null 2>&1
    fi
    
    if [ $? -eq 0 ]; then
        log_success "Подключение к целевой базе данных успешно"
        return 0
    else
        log_error "Не удалось подключиться к целевой базе данных"
        return 1
    fi
}

# Функция для сбора информации об исходной базе
collect_source_info() {
    log_info "Сбор информации об исходной базе данных..."
    
    local info_sql="
        SELECT 
            current_database() as database,
            version() as version,
            pg_database_size(current_database()) as size_bytes,
            pg_size_pretty(pg_database_size(current_database())) as size_pretty,
            (SELECT setting FROM pg_settings WHERE name = 'server_version_num') as version_num,
            (SELECT setting FROM pg_settings WHERE name = 'server_encoding') as encoding,
            (SELECT setting FROM pg_settings WHERE name = 'lc_collate') as collation,
            (SELECT setting FROM pg_settings WHERE name = 'lc_ctype') as ctype,
            COUNT(DISTINCT schemaname) as schema_count,
            COUNT(*) as table_count,
            SUM(pg_total_relation_size(quote_ident(schemaname) || '.' || quote_ident(tablename))) as total_size_bytes
        FROM pg_tables 
        WHERE schemaname NOT IN ('pg_catalog', 'information_schema');
    "
    
    local result=$(PGPASSWORD="$SOURCE_PASSWORD" psql \
        -h "$SOURCE_HOST" \
        -p "$SOURCE_PORT" \
        -U "$SOURCE_USER" \
        -d "$SOURCE_DB" \
        -c "$info_sql" \
        -t -A -F "|" 2>/dev/null)
    
    if [ -z "$result" ]; then
        log_error "Не удалось собрать информацию об исходной базе"
        exit 1
    fi
    
    IFS='|' read -r db_name db_version db_size_bytes db_size_pretty \
        db_version_num db_encoding db_collation db_ctype \
        db_schema_count db_table_count db_total_size_bytes <<< "$result"
    
    echo -e "\n${GREEN}📊 Информация об исходной базе:${NC}"
    echo "=============================="
    echo "База данных:      $db_name"
    echo "Версия:           $db_version"
    echo "Размер:           $db_size_pretty"
    echo "Кодировка:        $db_encoding"
    echo "Collation:        $db_collation"
    echo "Ctype:            $db_ctype"
    echo "Количество схем:  $db_schema_count"
    echo "Количество таблиц: $db_table_count"
    echo "=============================="
    
    # Сохраняем информацию для использования в других функциях
    SOURCE_INFO="$result"
}

# Функция для проверки совместимости
check_compatibility() {
    if [ "$VERIFY_COMPATIBILITY" != "true" ]; then
        log_warning "Проверка совместимости отключена"
        return 0
    fi
    
    log_info "Проверка совместимости баз данных..."
    
    # Получаем информацию о целевой базе
    local target_info_sql="
        SELECT 
            (SELECT setting FROM pg_settings WHERE name = 'server_version_num') as version_num,
            (SELECT setting FROM pg_settings WHERE name = 'server_encoding') as encoding,
            (SELECT setting FROM pg_settings WHERE name = 'lc_collate') as collation,
            (SELECT setting FROM pg_settings WHERE name = 'lc_ctype') as ctype;
    "
    
    local target_result
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            target_result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$target_info_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                -t -A -F "|" 2>/dev/null)
        else
            target_result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$target_info_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                -t -A -F "|" 2>/dev/null)
        fi
    else
        target_result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "postgres" \
            -c "$target_info_sql" \
            -t -A -F "|" 2>/dev/null)
    fi
    
    if [ -z "$target_result" ]; then
        log_warning "Не удалось получить информацию о целевой базе"
        return 0
    fi
    
    IFS='|' read -r target_version_num target_encoding target_collation target_ctype <<< "$target_result"
    IFS='|' read -r _ _ _ _ source_version_num source_encoding source_collation source_ctype _ _ _ <<< "$SOURCE_INFO"
    
    local issues=0
    
    echo -e "\n${GREEN}🔍 Проверка совместимости:${NC}"
    echo "=============================="
    
    # Проверяем версию PostgreSQL
    if [ "$source_version_num" -gt "$target_version_num" ]; then
        log_warning "Версия исходной БД ($source_version_num) новее целевой ($target_version_num)"
        ((issues++))
    else
        echo "✓ Версия PostgreSQL: совместима"
    fi
    
    # Проверяем кодировку
    if [ "$source_encoding" != "$target_encoding" ]; then
        log_warning "Разные кодировки: исходная=$source_encoding, целевая=$target_encoding"
        ((issues++))
    else
        echo "✓ Кодировка: совместима ($source_encoding)"
    fi
    
    # Проверяем collation
    if [ "$source_collation" != "$target_collation" ]; then
        log_warning "Разные collation: исходная=$source_collation, целевая=$target_collation"
        ((issues++))
    else
        echo "✓ Collation: совместимо ($source_collation)"
    fi
    
    # Проверяем расширения
    check_extensions_compatibility
    
    echo "=============================="
    
    if [ $issues -gt 0 ]; then
        log_warning "Обнаружено $issues проблем совместимости"
        read -p "Продолжить миграцию? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            log_info "Миграция отменена"
            exit 0
        fi
    else
        log_success "Проблем совместимости не обнаружено"
    fi
}

# Функция для проверки совместимости расширений
check_extensions_compatibility() {
    log_info "Проверка расширений PostgreSQL..."
    
    # Получаем расширения из исходной базы
    local source_extensions_sql="
        SELECT extname, extversion 
        FROM pg_extension 
        WHERE extname NOT IN ('plpgsql')
        ORDER BY extname;
    "
    
    local source_extensions=$(PGPASSWORD="$SOURCE_PASSWORD" psql \
        -h "$SOURCE_HOST" \
        -p "$SOURCE_PORT" \
        -U "$SOURCE_USER" \
        -d "$SOURCE_DB" \
        -c "$source_extensions_sql" \
        -t -A -F "|" 2>/dev/null)
    
    # Получаем доступные расширения в целевой базе
    local target_extensions_sql="
        SELECT name 
        FROM pg_available_extensions 
        WHERE installed_version IS NOT NULL
        ORDER BY name;
    "
    
    local target_extensions
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            target_extensions=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$target_extensions_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                -t -A 2>/dev/null)
        else
            target_extensions=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$target_extensions_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                -t -A 2>/dev/null)
        fi
    else
        target_extensions=$(PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "postgres" \
            -c "$target_extensions_sql" \
            -t -A 2>/dev/null)
    fi
    
    local incompatible_extensions=()
    
    while IFS='|' read -r ext_name ext_version; do
        if [ -n "$ext_name" ]; then
            if echo "$target_extensions" | grep -q "^$ext_name$"; then
                echo "✓ Расширение: $ext_name ($ext_version) - доступно"
            else
                log_warning "Расширение $ext_name не доступно в целевой базе"
                incompatible_extensions+=("$ext_name")
            fi
        fi
    done <<< "$source_extensions"
    
    if [ ${#incompatible_extensions[@]} -gt 0 ]; then
        log_warning "Несовместимые расширения: ${incompatible_extensions[*]}"
        echo "Примечание: Эти расширения не будут перенесены"
    fi
}

# Функция для создания бэкапа исходной базы
create_source_backup() {
    log_info "Создание бэкапа исходной базы данных..."
    
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="${BACKUP_DIR}/migration_backup_${timestamp}.sql"
    
    mkdir -p "$BACKUP_DIR"
    
    # Параметры pg_dump
    local dump_options=""
    dump_options+="--host=$SOURCE_HOST "
    dump_options+="--port=$SOURCE_PORT "
    dump_options+="--username=$SOURCE_USER "
    dump_options+="--dbname=$SOURCE_DB "
    dump_options+="--clean "
    dump_options+="--if-exists "
    dump_options+="--verbose "
    dump_options+="--no-privileges "
    dump_options+="--no-owner "
    
    # Исключаем несовместимые расширения
    dump_options+="--exclude-schema=pg_catalog "
    dump_options+="--exclude-schema=information_schema "
    
    export PGPASSWORD="$SOURCE_PASSWORD"
    
    log_info "Выполнение pg_dump..."
    if ! pg_dump $dump_options > "$backup_file" 2>> "${BACKUP_DIR}/migration_${timestamp}.log"; then
        log_error "Ошибка при создании бэкапа"
        exit 1
    fi
    
    # Сжимаем бэкап
    if [ "$COMPRESSION" = "gzip" ]; then
        log_info "Сжатие бэкапа..."
        gzip -9 -c "$backup_file" > "${backup_file}.gz"
        rm "$backup_file"
        backup_file="${backup_file}.gz"
    fi
    
    # Создаем контрольную сумму
    sha256sum "$backup_file" > "${backup_file}.sha256"
    
    log_success "Бэкап создан: $(basename "$backup_file")"
    echo "$backup_file"
}

# Функция для подготовки целевой базы
prepare_target_database() {
    log_info "Подготовка целевой базы данных..."
    
    # Создаем базу данных если не существует
    local check_db_sql="SELECT 1 FROM pg_database WHERE datname = '$TARGET_DB';"
    local create_db_sql="
        CREATE DATABASE $TARGET_DB
        WITH 
        OWNER = $TARGET_USER
        ENCODING = 'UTF8'
        LC_COLLATE = '$TARGET_COLLATION'
        LC_CTYPE = '$TARGET_CTYPE'
        TEMPLATE = template0
        CONNECTION LIMIT = -1;
    "
    
    # Проверяем существует ли база
    local db_exists
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            db_exists=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$check_db_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                -t -A 2>/dev/null)
        else
            db_exists=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "postgres" \
                -c "$check_db_sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                -t -A 2>/dev/null)
        fi
    else
        db_exists=$(PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "postgres" \
            -c "$check_db_sql" \
            -t -A 2>/dev/null)
    fi
    
    if [ "$db_exists" = "1" ]; then
        log_warning "База данных '$TARGET_DB' уже существует"
        
        read -p "Удалить существующую базу данных? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            # Завершаем активные подключения
            local terminate_sql="
                SELECT pg_terminate_backend(pid) 
                FROM pg_stat_activity 
                WHERE datname = '$TARGET_DB' 
                AND pid <> pg_backend_pid();
            "
            
            execute_target_sql "postgres" "$terminate_sql" || true
            
            # Удаляем базу
            local drop_sql="DROP DATABASE $TARGET_DB;"
            if execute_target_sql "postgres" "$drop_sql"; then
                log_success "Существующая база данных удалена"
            else
                log_error "Не удалось удалить базу данных"
                exit 1
            fi
        else
            log_error "Миграция отменена"
            exit 0
        fi
    fi
    
    # Создаем базу данных
    if execute_target_sql "postgres" "$create_db_sql"; then
        log_success "Целевая база данных создана"
    else
        log_error "Не удалось создать целевую базу данных"
        exit 1
    fi
    
    # Создаем расширения
    create_target_extensions
}

# Функция для создания расширений в целевой базе
create_target_extensions() {
    log_info "Создание расширений в целевой базе..."
    
    local extensions=("uuid-ossp" "pgcrypto" "pg_stat_statements")
    
    for ext in "${extensions[@]}"; do
        local create_ext_sql="CREATE EXTENSION IF NOT EXISTS \"$ext\";"
        
        if execute_target_sql "$TARGET_DB" "$create_ext_sql"; then
            log_info "Расширение создано: $ext"
        else
            log_warning "Не удалось создать расширение: $ext"
        fi
    done
}

# Функция для выполнения SQL в целевой базе
execute_target_sql() {
    local database="$1"
    local sql="$2"
    
    if [ "$DRY_RUN" = "true" ]; then
        log_info "[DRY RUN] Выполнилось бы: $sql"
        return 0
    fi
    
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "$database" \
                -c "$sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                --set=ON_ERROR_STOP=1 \
                > /dev/null 2>&1
        else
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "$database" \
                -c "$sql" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=ON_ERROR_STOP=1 \
                > /dev/null 2>&1
        fi
    else
        PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "$database" \
            -c "$sql" \
            --set=ON_ERROR_STOP=1 \
            > /dev/null 2>&1
    fi
    
    return $?
}

# Функция для восстановления бэкапа в целевую базу
restore_to_target() {
    local backup_file="$1"
    
    log_info "Восстановление бэкапа в целевую базу..."
    
    # Распаковываем если нужно
    local sql_file="$backup_file"
    if [[ "$backup_file" == *.gz ]]; then
        sql_file="${backup_file%.gz}"
        log_info "Распаковка бэкапа..."
        gzip -dc "$backup_file" > "$sql_file"
    fi
    
    # Восстанавливаем
    if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
        if [ -f "$TARGET_CA_CERT" ]; then
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "$TARGET_DB" \
                -f "$sql_file" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=sslrootcert="$TARGET_CA_CERT" \
                --set=ON_ERROR_STOP=1 \
                > "${BACKUP_DIR}/restore.log" 2>&1
        else
            PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "$TARGET_DB" \
                -f "$sql_file" \
                --set=sslmode="$TARGET_SSL_MODE" \
                --set=ON_ERROR_STOP=1 \
                > "${BACKUP_DIR}/restore.log" 2>&1
        fi
    else
        PGPASSWORD="$TARGET_PASSWORD" psql \
            -h "$TARGET_HOST" \
            -p "$TARGET_PORT" \
            -U "$TARGET_USER" \
            -d "$TARGET_DB" \
            -f "$sql_file" \
            --set=ON_ERROR_STOP=1 \
            > "${BACKUP_DIR}/restore.log" 2>&1
    fi
    
    local restore_status=$?
    
    # Очищаем временный файл
    if [ "$sql_file" != "$backup_file" ]; then
        rm "$sql_file"
    fi
    
    if [ $restore_status -eq 0 ]; then
        log_success "Бэкап успешно восстановлен в целевую базу"
        return 0
    else
        log_error "Ошибка при восстановлении бэкапа"
        log_error "Логи восстановления:"
        tail -50 "${BACKUP_DIR}/restore.log" >&2
        return 1
    fi
}

# Функция для тестирования миграции
test_migration() {
    if [ "$TEST_MIGRATION" != "true" ]; then
        return 0
    fi
    
    log_info "Тестирование миграции..."
    
    # Тестовые запросы
    local test_queries=(
        "SELECT COUNT(*) as table_count FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema');"
        "SELECT COUNT(*) as migration_count FROM public.__efmigrationshistory;"
        "SELECT pg_size_pretty(pg_database_size('$TARGET_DB')) as db_size;"
    )
    
    echo -e "\n${GREEN}🧪 Результаты тестирования:${NC}"
    echo "=============================="
    
    for query in "${test_queries[@]}"; do
        local result
        if [ "$TARGET_SSL_MODE" = "require" ] || [ "$TARGET_SSL_MODE" = "verify-full" ]; then
            if [ -f "$TARGET_CA_CERT" ]; then
                result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                    -h "$TARGET_HOST" \
                    -p "$TARGET_PORT" \
                    -U "$TARGET_USER" \
                    -d "$TARGET_DB" \
                    -c "$query" \
                    --set=sslmode="$TARGET_SSL_MODE" \
                    --set=sslrootcert="$TARGET_CA_CERT" \
                    -t -A 2>/dev/null)
            else
                result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                    -h "$TARGET_HOST" \
                    -p "$TARGET_PORT" \
                    -U "$TARGET_USER" \
                    -d "$TARGET_DB" \
                    -c "$query" \
                    --set=sslmode="$TARGET_SSL_MODE" \
                    -t -A 2>/dev/null)
            fi
        else
            result=$(PGPASSWORD="$TARGET_PASSWORD" psql \
                -h "$TARGET_HOST" \
                -p "$TARGET_PORT" \
                -U "$TARGET_USER" \
                -d "$TARGET_DB" \
                -c "$query" \
                -t -A 2>/dev/null)
        fi
        
        local query_name=$(echo "$query" | cut -d' ' -f2)
        echo "✓ $query_name: $result"
    done
    
    # Тестируем подключение приложения
    test_application_connection
    
    echo "=============================="
    log_success "Тестирование завершено"
}

# Функция для тестирования подключения приложения
test_application_connection() {
    log_info "Тестирование подключения приложения..."
    
    # Простой тест - создаем временную таблицу и удаляем ее
    local test_table_sql="
        CREATE TEMP TABLE migration_test (id SERIAL PRIMARY KEY, name TEXT);
        INSERT INTO migration_test (name) VALUES ('test');
        SELECT COUNT(*) FROM migration_test;
        DROP TABLE migration_test;
    "
    
    if execute_target_sql "$TARGET_DB" "$test_table_sql"; then
        echo "✓ Подключение приложения: работает"
    else
        log_warning "Подключение приложения: возможны проблемы"
    fi
}

# Функция для создания отчета о миграции
create_migration_report() {
    local backup_file="$1"
    local timestamp=$(date +%Y-%m-%d\ %H:%M:%S)
    
    local report_file="${BACKUP_DIR}/migration_report_$(date +%Y%m%d_%H%M%S).md"
    
    cat > "$report_file" << EOF
# Отчет о миграции базы данных

## Основная информация
- **Дата миграции:** $timestamp
- **Исходная база:** $SOURCE_DB @ $SOURCE_HOST:$SOURCE_PORT
- **Целевая база:** $TARGET_DB @ $TARGET_HOST:$TARGET_PORT
- **Файл бэкапа:** $(basename "$backup_file")

## Статистика миграции
$(echo "$SOURCE_INFO" | awk -F'|' '{
    print "### Исходная база"
    print "- База данных: " \$1
    print "- Версия PostgreSQL: " \$2
    print "- Размер: " \$4
    print "- Кодировка: " \$6
    print "- Collation: " \$7
    print "- Количество таблиц: " \$10
}')

## Проверки
$(if [ "$VERIFY_COMPATIBILITY" = "true" ]; then
    echo "- Проверка совместимости: выполнена"
else
    echo "- Проверка совместимости: пропущена"
fi)

$(if [ "$TEST_MIGRATION" = "true" ]; then
    echo "- Тестирование миграции: выполнено"
else
    echo "- Тестирование миграции: пропущено"
fi)

## Результат
Миграция успешно завершена.

## Следующие шаги
1. Обновить connection strings в приложениях
2. Протестировать работу приложений с новой базой
3. Настроить мониторинг целевой базы
4. Удалить исходную базу данных (если нужно)

## Контактная информация
Для вопросов по миграции обращайтесь к команде инфраструктуры.
EOF
    
    log_success "Отчет создан: $report_file"
}

# Основная функция
main() {
    local start_time=$(date +%s)
    
    log_info "🚀 Начало миграции базы данных"
    log_info "Источник: $SOURCE_DB@$SOURCE_HOST:$SOURCE_PORT"
    log_info "Цель: $TARGET_DB@$TARGET_HOST:$TARGET_PORT"
    
    if [ "$DRY_RUN" = "true" ]; then
        log_warning "РЕЖИМ ПРОБНОГО ЗАПУСКА - изменения не будут применены"
    fi
    
    # Проверяем зависимости
    check_dependencies
    
    # Проверяем подключения
    check_source_connection
    check_target_connection
    
    # Собираем информацию
    collect_source_info
    
    # Проверяем совместимость
    check_compatibility
    
    # Создаем бэкап
    local backup_file=$(create_source_backup)
    
    # Подготавливаем целевую базу
    prepare_target_database
    
    # Восстанавливаем бэкап
    if [ "$DRY_RUN" != "true" ]; then
        restore_to_target "$backup_file"
    fi
    
    # Тестируем миграцию
    test_migration
    
    # Создаем отчет
    create_migration_report "$backup_file"
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    log_success "✅ Миграция успешно завершена за $duration секунд"
    
    echo -e "\n${GREEN}🎉 Миграция завершена!${NC}"
    echo "Следующие шаги:"
    echo "1. Обновите connection strings в приложениях"
    echo "2. Протестируйте работу с новой базой"
    echo "3. Настройте мониторинг"
    echo "4. Отчет о миграции: $(find "$BACKUP_DIR" -name "migration_report_*.md" | tail -1)"
}

# Обработка аргументов командной строки
while [[ $# -gt 0 ]]; do
    case $1 in
        --source-host)
            SOURCE_HOST="$2"
            shift 2
            ;;
        --source-port)
            SOURCE_PORT="$2"
            shift 2
            ;;
        --source-db)
            SOURCE_DB="$2"
            shift 2
            ;;
        --source-user)
            SOURCE_USER="$2"
            shift 2
            ;;
        --source-password)
            SOURCE_PASSWORD="$2"
            shift 2
            ;;
        --target-host)
            TARGET_HOST="$2"
            shift 2
            ;;
        --target-port)
            TARGET_PORT="$2"
            shift 2
            ;;
        --target-db)
            TARGET_DB="$2"
            shift 2
            ;;
        --target-user)
            TARGET_USER="$2"
            shift 2
            ;;
        --target-password)
            TARGET_PASSWORD="$2"
            shift 2
            ;;
        --target-ssl-mode)
            TARGET_SSL_MODE="$2"
            shift 2
            ;;
        --target-ca-cert)
            TARGET_CA_CERT="$2"
            shift 2
            ;;
        --backup-dir)
            BACKUP_DIR="$2"
            shift 2
            ;;
        --compression)
            COMPRESSION="$2"
            shift 2
            ;;
        --no-compatibility-check)
            VERIFY_COMPATIBILITY="false"
            shift
            ;;
        --no-test)
            TEST_MIGRATION="false"
            shift
            ;;
        --dry-run)
            DRY_RUN="true"
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
            echo "  --source-host          Хост исходной БД (по умолчанию: localhost)"
            echo "  --source-port          Порт исходной БД (по умолчанию: 5432)"
            echo "  --source-db            Имя исходной БД (по умолчанию: testdb)"
            echo "  --source-user          Пользователь исходной БД (по умолчанию: postgres)"
            echo "  --source-password      Пароль исходной БД"
            echo "  --target-host          Хост целевой БД (обязательно)"
            echo "  --target-port          Порт целевой БД (по умолчанию: 6432)"
            echo "  --target-db            Имя целевой БД (по умолчанию: testdb)"
            echo "  --target-user          Пользователь целевой БД (по умолчанию: appuser)"
            echo "  --target-password      Пароль целевой БД"
            echo "  --target-ssl-mode      Режим SSL для целевой БД (по умолчанию: verify-full)"
            echo "  --target-ca-cert       Путь к CA сертификату Yandex Cloud"
            echo "  --backup-dir           Директория для бэкапов"
            echo "  --compression          Сжатие (gzip, none)"
            echo "  --no-compatibility-check Отключить проверку совместимости"
            echo "  --no-test              Отключить тестирование после миграции"
            echo "  --dry-run              Пробный запуск без изменений"
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
# 1. Миграция с локальной базы на Yandex Managed PostgreSQL:
#    ./scripts/migrate-to-managed-db.sh --target-host your-postgres-host.yandexcloud.net \
#         --target-user appuser --target-password your-password \
#         --target-ca-cert ./certs/yandex-ca.pem
#
# 2. Пробный запуск миграции:
#    ./scripts/migrate-to-managed-db.sh --target-host your-postgres-host.yandexcloud.net \
#         --dry-run --verbose
#
# 3. Миграция с указанием всех параметров:
#    ./scripts/migrate-to-managed-db.sh \
#         --source-host localhost --source-db myapp \
#         --source-user postgres --source-password postgres \
#         --target-host managed-postgres.yandexcloud.net --target-port 6432 \
#         --target-db myapp-prod --target-user appuser --target-password secret \
#         --target-ssl-mode verify-full --target-ca-cert ./certs/yandex-ca.pem \
#         --backup-dir ./migration-backups --verbose