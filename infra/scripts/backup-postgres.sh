#!/bin/bash
# Скрипт для создания резервных копий PostgreSQL
# Поддерживает: локальные базы, Yandex Managed PostgreSQL, S3 бэкапы

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
BACKUP_TYPE=${BACKUP_TYPE:-"full"}  # full, schema-only, data-only
ENVIRONMENT=${ENVIRONMENT:-"production"}
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-"testdb"}
DB_USER=${DB_USER:-"postgres"}
DB_PASSWORD=${DB_PASSWORD:-""}
SSL_MODE=${SSL_MODE:-"disable"}
CA_CERT_PATH=${CA_CERT_PATH:-""}
BACKUP_DIR=${BACKUP_DIR:-"./backups"}
RETENTION_DAYS=${RETENTION_DAYS:-7}
COMPRESSION=${COMPRESSION:-"gzip"}  # gzip, bzip2, none
ENCRYPTION=${ENCRYPTION:-"false"}
ENCRYPTION_PASSWORD=${ENCRYPTION_PASSWORD:-""}
S3_ENABLED=${S3_ENABLED:-"false"}
S3_BUCKET=${S3_BUCKET:-""}
S3_PREFIX=${S3_PREFIX:-"postgres-backups"}
VERBOSE=${VERBOSE:-"false"}
NOTIFICATION_ENABLED=${NOTIFICATION_ENABLED:-"false"}
SLACK_WEBHOOK=${SLACK_WEBHOOK:-""}
HEALTH_CHECK_URL=${HEALTH_CHECK_URL:-""}

# Функция для отправки уведомлений
send_notification() {
    local status="$1"
    local message="$2"
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    
    if [ "$NOTIFICATION_ENABLED" = "true" ]; then
        # Slack уведомление
        if [ -n "$SLACK_WEBHOOK" ]; then
            local color="good"
            local emoji="✅"
            
            if [ "$status" = "failed" ]; then
                color="danger"
                emoji="❌"
            elif [ "$status" = "warning" ]; then
                color="warning"
                emoji="⚠️"
            fi
            
            curl -s -X POST -H 'Content-type: application/json' \
                --data "{
                    \"attachments\": [{
                        \"color\": \"$color\",
                        \"title\": \"PostgreSQL Backup $status $emoji\",
                        \"text\": \"$message\",
                        \"fields\": [
                            {
                                \"title\": \"Environment\",
                                \"value\": \"$ENVIRONMENT\",
                                \"short\": true
                            },
                            {
                                \"title\": \"Database\",
                                \"value\": \"$DB_NAME\",
                                \"short\": true
                            },
                            {
                                \"title\": \"Backup Type\",
                                \"value\": \"$BACKUP_TYPE\",
                                \"short\": true
                            }
                        ],
                        \"ts\": \"$(date +%s)\"
                    }]
                }" "$SLACK_WEBHOOK" > /dev/null 2>&1 || true
        fi
        
        # Health check ping
        if [ -n "$HEALTH_CHECK_URL" ]; then
            if [ "$status" = "success" ]; then
                curl -fsS --retry 3 "$HEALTH_CHECK_URL" > /dev/null 2>&1 || true
            fi
        fi
    fi
}

# Функция для проверки зависимостей
check_dependencies() {
    local dependencies=("pg_dump" "psql")
    
    if [ "$S3_ENABLED" = "true" ]; then
        dependencies+=("aws")
    fi
    
    if [ "$COMPRESSION" = "gzip" ]; then
        dependencies+=("gzip")
    elif [ "$COMPRESSION" = "bzip2" ]; then
        dependencies+=("bzip2")
    fi
    
    if [ "$ENCRYPTION" = "true" ]; then
        dependencies+=("openssl")
    fi
    
    for dep in "${dependencies[@]}"; do
        if ! command -v "$dep" &> /dev/null; then
            log_error "Требуется $dep, но не установлен"
            exit 1
        fi
    done
    
    log_success "Все зависимости установлены"
}

# Функция для получения информации о базе данных
get_database_info() {
    log_info "Получение информации о базе данных..."
    
    local info_sql="
        SELECT 
            current_database() as database,
            version() as version,
            pg_database_size(current_database()) as size_bytes,
            pg_size_pretty(pg_database_size(current_database())) as size_pretty,
            pg_stat_get_db_numbackends(datid) as connections,
            count(*) as table_count
        FROM information_schema.tables 
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema');
    "
    
    local result
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$info_sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                -t -A -F "|" 2>/dev/null)
        else
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$info_sql" \
                --set=sslmode="$SSL_MODE" \
                -t -A -F "|" 2>/dev/null)
        fi
    else
        result=$(PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -c "$info_sql" \
            -t -A -F "|" 2>/dev/null)
    fi
    
    echo "$result"
}

# Функция для создания полного бэкапа
create_full_backup() {
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="${BACKUP_DIR}/full_backup_${timestamp}.sql"
    local compressed_file="${backup_file}.${COMPRESSION}"
    
    log_info "Создание полного бэкапа..."
    
    # Параметры pg_dump
    local dump_options=""
    dump_options+="--host=$DB_HOST "
    dump_options+="--port=$DB_PORT "
    dump_options+="--username=$DB_USER "
    dump_options+="--dbname=$DB_NAME "
    dump_options+="--clean "
    dump_options+="--if-exists "
    dump_options+="--verbose "
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        dump_options+="--ssl-mode=$SSL_MODE "
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            dump_options+="--ssl-root-cert=$CA_CERT_PATH "
        fi
    fi
    
    # Создаем директорию для бэкапов
    mkdir -p "$BACKUP_DIR"
    
    # Выполняем бэкап
    export PGPASSWORD="$DB_PASSWORD"
    
    if [ "$VERBOSE" = "true" ]; then
        log_info "Выполнение pg_dump с параметрами: $dump_options"
    fi
    
    if ! pg_dump $dump_options > "$backup_file" 2>> "${BACKUP_DIR}/backup_${timestamp}.log"; then
        log_error "Ошибка при создании бэкапа"
        send_notification "failed" "Ошибка при создании полного бэкапа базы данных $DB_NAME"
        exit 1
    fi
    
    # Сжимаем бэкап
    if [ "$COMPRESSION" = "gzip" ]; then
        log_info "Сжатие бэкапа с помощью gzip..."
        gzip -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    elif [ "$COMPRESSION" = "bzip2" ]; then
        log_info "Сжатие бэкапа с помощью bzip2..."
        bzip2 -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    else
        compressed_file="$backup_file"
    fi
    
    # Шифруем бэкап (если требуется)
    if [ "$ENCRYPTION" = "true" ] && [ -n "$ENCRYPTION_PASSWORD" ]; then
        log_info "Шифрование бэкапа..."
        local encrypted_file="${compressed_file}.enc"
        openssl enc -aes-256-cbc -salt -pbkdf2 \
            -in "$compressed_file" \
            -out "$encrypted_file" \
            -pass pass:"$ENCRYPTION_PASSWORD"
        
        if [ $? -eq 0 ]; then
            rm "$compressed_file"
            compressed_file="$encrypted_file"
            log_success "Бэкап зашифрован"
        else
            log_warning "Не удалось зашифровать бэкап"
        fi
    fi
    
    # Создаем контрольную сумму
    log_info "Создание контрольной суммы..."
    sha256sum "$compressed_file" > "${compressed_file}.sha256"
    
    echo "$compressed_file"
}

# Функция для создания бэкапа только схемы
create_schema_backup() {
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="${BACKUP_DIR}/schema_backup_${timestamp}.sql"
    local compressed_file="${backup_file}.${COMPRESSION}"
    
    log_info "Создание бэкапа схемы..."
    
    # Параметры pg_dump
    local dump_options=""
    dump_options+="--host=$DB_HOST "
    dump_options+="--port=$DB_PORT "
    dump_options+="--username=$DB_USER "
    dump_options+="--dbname=$DB_NAME "
    dump_options+="--schema-only "
    dump_options+="--verbose "
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        dump_options+="--ssl-mode=$SSL_MODE "
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            dump_options+="--ssl-root-cert=$CA_CERT_PATH "
        fi
    fi
    
    # Выполняем бэкап
    export PGPASSWORD="$DB_PASSWORD"
    
    if ! pg_dump $dump_options > "$backup_file" 2>> "${BACKUP_DIR}/backup_${timestamp}.log"; then
        log_error "Ошибка при создании бэкапа схемы"
        send_notification "failed" "Ошибка при создании бэкапа схемы базы данных $DB_NAME"
        exit 1
    fi
    
    # Сжимаем бэкап
    if [ "$COMPRESSION" = "gzip" ]; then
        gzip -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    elif [ "$COMPRESSION" = "bzip2" ]; then
        bzip2 -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    else
        compressed_file="$backup_file"
    fi
    
    echo "$compressed_file"
}

# Функция для создания бэкапа только данных
create_data_backup() {
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="${BACKUP_DIR}/data_backup_${timestamp}.sql"
    local compressed_file="${backup_file}.${COMPRESSION}"
    
    log_info "Создание бэкапа данных..."
    
    # Параметры pg_dump
    local dump_options=""
    dump_options+="--host=$DB_HOST "
    dump_options+="--port=$DB_PORT "
    dump_options+="--username=$DB_USER "
    dump_options+="--dbname=$DB_NAME "
    dump_options+="--data-only "
    dump_options+="--verbose "
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        dump_options+="--ssl-mode=$SSL_MODE "
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            dump_options+="--ssl-root-cert=$CA_CERT_PATH "
        fi
    fi
    
    # Выполняем бэкап
    export PGPASSWORD="$DB_PASSWORD"
    
    if ! pg_dump $dump_options > "$backup_file" 2>> "${BACKUP_DIR}/backup_${timestamp}.log"; then
        log_error "Ошибка при создании бэкапа данных"
        send_notification "failed" "Ошибка при создании бэкапа данных базы данных $DB_NAME"
        exit 1
    fi
    
    # Сжимаем бэкап
    if [ "$COMPRESSION" = "gzip" ]; then
        gzip -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    elif [ "$COMPRESSION" = "bzip2" ]; then
        bzip2 -9 -c "$backup_file" > "$compressed_file"
        rm "$backup_file"
    else
        compressed_file="$backup_file"
    fi
    
    echo "$compressed_file"
}

# Функция для загрузки в S3
upload_to_s3() {
    local file_path="$1"
    local file_name=$(basename "$file_path")
    
    if [ "$S3_ENABLED" = "true" ] && [ -n "$S3_BUCKET" ]; then
        log_info "Загрузка бэкапа в S3..."
        
        local s3_path="s3://${S3_BUCKET}/${S3_PREFIX}/${ENVIRONMENT}/${file_name}"
        
        if aws s3 cp "$file_path" "$s3_path" 2>> "${BACKUP_DIR}/s3_upload.log"; then
            log_success "Бэкап загружен в S3: $s3_path"
            
            # Загружаем контрольную сумму
            if [ -f "${file_path}.sha256" ]; then
                aws s3 cp "${file_path}.sha256" "s3://${S3_BUCKET}/${S3_PREFIX}/${ENVIRONMENT}/${file_name}.sha256"
            fi
            
            # Удаляем старые бэкапы в S3
            if [ "$RETENTION_DAYS" -gt 0 ]; then
                log_info "Очистка старых бэкапов в S3..."
                aws s3 ls "s3://${S3_BUCKET}/${S3_PREFIX}/${ENVIRONMENT}/" \
                    | awk '{print $4}' \
                    | while read -r old_file; do
                        local file_date=$(echo "$old_file" | grep -oE '[0-9]{8}_[0-9]{6}')
                        if [ -n "$file_date" ]; then
                            local file_timestamp=$(date -d "${file_date:0:8} ${file_date:9:2}:${file_date:11:2}:${file_date:13:2}" +%s)
                            local cutoff_timestamp=$(date -d "$RETENTION_DAYS days ago" +%s)
                            
                            if [ "$file_timestamp" -lt "$cutoff_timestamp" ]; then
                                log_info "Удаление старого бэкапа из S3: $old_file"
                                aws s3 rm "s3://${S3_BUCKET}/${S3_PREFIX}/${ENVIRONMENT}/$old_file"
                                aws s3 rm "s3://${S3_BUCKET}/${S3_PREFIX}/${ENVIRONMENT}/$old_file.sha256" 2>/dev/null || true
                            fi
                        fi
                    done
            fi
        else
            log_warning "Не удалось загрузить бэкап в S3"
        fi
    fi
}

# Функция для очистки старых бэкапов
cleanup_old_backups() {
    if [ "$RETENTION_DAYS" -gt 0 ]; then
        log_info "Очистка старых бэкапов (старше $RETENTION_DAYS дней)..."
        
        find "$BACKUP_DIR" -name "*_backup_*" -type f -mtime +"$RETENTION_DAYS" \
            -exec rm -f {} \;
        find "$BACKUP_DIR" -name "*_backup_*.sha256" -type f -mtime +"$RETENTION_DAYS" \
            -exec rm -f {} \;
        
        log_success "Старые бэкапы удалены"
    fi
}

# Функция для верификации бэкапа
verify_backup() {
    local backup_file="$1"
    
    log_info "Верификация бэкапа..."
    
    # Проверяем контрольную сумму
    if [ -f "${backup_file}.sha256" ]; then
        if cd "$BACKUP_DIR" && sha256sum -c "$(basename "${backup_file}.sha256")" > /dev/null 2>&1; then
            log_success "Контрольная сумма верна"
        else
            log_error "Контрольная сумма не совпадает"
            return 1
        fi
    fi
    
    # Проверяем, что файл не пустой
    if [ ! -s "$backup_file" ]; then
        log_error "Файл бэкапа пуст"
        return 1
    fi
    
    # Проверяем заголовок SQL (для незашифрованных файлов)
    if [[ "$backup_file" != *.enc ]]; then
        if [ "$COMPRESSION" = "gzip" ]; then
            if ! gzip -t "$backup_file" 2>/dev/null; then
                log_error "Файл бэкапа поврежден (gzip)"
                return 1
            fi
            
            # Проверяем SQL заголовок
            local header=$(gzip -dc "$backup_file" | head -1)
            if ! echo "$header" | grep -q "PostgreSQL database dump"; then
                log_error "Неверный формат SQL дампа"
                return 1
            fi
        elif [ "$COMPRESSION" = "bzip2" ]; then
            if ! bzip2 -t "$backup_file" 2>/dev/null; then
                log_error "Файл бэкапа поврежден (bzip2)"
                return 1
            fi
        else
            local header=$(head -1 "$backup_file")
            if ! echo "$header" | grep -q "PostgreSQL database dump"; then
                log_error "Неверный формат SQL дампа"
                return 1
            fi
        fi
    fi
    
    log_success "Бэкап прошел верификацию"
    return 0
}

# Функция для логирования статистики
log_statistics() {
    local backup_file="$1"
    local db_info="$2"
    
    local timestamp=$(date +%Y-%m-%d\ %H:%M:%S)
    local size_bytes=$(stat -c%s "$backup_file" 2>/dev/null || stat -f%z "$backup_file")
    local size_human=$(numfmt --to=iec-i --suffix=B "$size_bytes")
    
    # Парсим информацию о БД
    IFS='|' read -r db_name db_version db_size_bytes db_size_pretty db_connections db_table_count <<< "$db_info"
    
    local log_entry="
================================================================================
Backup Completed: $timestamp
Environment: $ENVIRONMENT
Database: $db_name ($DB_HOST:$DB_PORT)
Backup Type: $BACKUP_TYPE
Backup File: $(basename "$backup_file")
Backup Size: $size_human
Database Size: $db_size_pretty
Database Version: $db_version
Table Count: $db_table_count
Active Connections: $db_connections
================================================================================
"
    
    echo "$log_entry" >> "${BACKUP_DIR}/backup_history.log"
    
    if [ "$VERBOSE" = "true" ]; then
        echo "$log_entry"
    fi
}

# Основная функция
main() {
    local start_time=$(date +%s)
    
    log_info "🚀 Начало создания бэкапа базы данных"
    log_info "Тип бэкапа: $BACKUP_TYPE"
    log_info "Окружение: $ENVIRONMENT"
    log_info "База данных: $DB_NAME@$DB_HOST:$DB_PORT"
    
    # Проверяем зависимости
    check_dependencies
    
    # Получаем информацию о базе данных
    local db_info=$(get_database_info)
    if [ -z "$db_info" ]; then
        log_error "Не удалось получить информацию о базе данных"
        send_notification "failed" "Не удалось получить информацию о базе данных $DB_NAME"
        exit 1
    fi
    
    # Создаем бэкап в зависимости от типа
    local backup_file=""
    
    case "$BACKUP_TYPE" in
        "full")
            backup_file=$(create_full_backup)
            ;;
        "schema-only")
            backup_file=$(create_schema_backup)
            ;;
        "data-only")
            backup_file=$(create_data_backup)
            ;;
        *)
            log_error "Неизвестный тип бэкапа: $BACKUP_TYPE"
            exit 1
            ;;
    esac
    
    # Верифицируем бэкап
    if ! verify_backup "$backup_file"; then
        log_error "Бэкап не прошел верификацию"
        send_notification "failed" "Бэкап базы данных $DB_NAME не прошел верификацию"
        exit 1
    fi
    
    # Загружаем в S3 (если включено)
    upload_to_s3 "$backup_file"
    
    # Очищаем старые бэкапы
    cleanup_old_backups
    
    # Логируем статистику
    log_statistics "$backup_file" "$db_info"
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    log_success "✅ Бэкап успешно создан за $duration секунд"
    log_success "Файл: $(basename "$backup_file")"
    log_success "Размер: $(du -h "$backup_file" | cut -f1)"
    
    # Отправляем уведомление об успехе
    send_notification "success" "Бэкап базы данных $DB_NAME успешно создан\nРазмер: $(du -h "$backup_file" | cut -f1)\nВремя: ${duration}с"
}

# Обработка аргументов командной строки
while [[ $# -gt 0 ]]; do
    case $1 in
        --type|-t)
            BACKUP_TYPE="$2"
            shift 2
            ;;
        --environment|-e)
            ENVIRONMENT="$2"
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
        --backup-dir)
            BACKUP_DIR="$2"
            shift 2
            ;;
        --retention-days)
            RETENTION_DAYS="$2"
            shift 2
            ;;
        --compression)
            COMPRESSION="$2"
            shift 2
            ;;
        --encryption)
            ENCRYPTION="true"
            shift
            ;;
        --encryption-password)
            ENCRYPTION_PASSWORD="$2"
            shift 2
            ;;
        --s3-enabled)
            S3_ENABLED="true"
            shift
            ;;
        --s3-bucket)
            S3_BUCKET="$2"
            shift 2
            ;;
        --s3-prefix)
            S3_PREFIX="$2"
            shift 2
            ;;
        --slack-webhook)
            SLACK_WEBHOOK="$2"
            NOTIFICATION_ENABLED="true"
            shift 2
            ;;
        --health-check-url)
            HEALTH_CHECK_URL="$2"
            NOTIFICATION_ENABLED="true"
            shift 2
            ;;
        --verbose|-v)
            VERBOSE="true"
            shift
            ;;
        --help)
            echo "Использование: $0 [опции]"
            echo ""
            echo "Опции:"
            echo "  -t, --type              Тип бэкапа (full, schema-only, data-only)"
            echo "  -e, --environment       Окружение (development, staging, production)"
            echo "  -h, --host              Хост БД"
            echo "  -p, --port              Порт БД"
            echo "  -n, --name              Имя базы данных"
            echo "  -u, --user              Пользователь БД"
            echo "  -P, --password          Пароль БД"
            echo "  --ssl-mode              Режим SSL (disable, require, verify-full)"
            echo "  --ca-cert               Путь к CA сертификату"
            echo "  --backup-dir            Директория для бэкапов"
            echo "  --retention-days        Дни хранения бэкапов (по умолчанию: 7)"
            echo "  --compression           Сжатие (gzip, bzip2, none)"
            echo "  --encryption            Включить шифрование"
            echo "  --encryption-password   Пароль для шифрования"
            echo "  --s3-enabled            Включить загрузку в S3"
            echo "  --s3-bucket             Имя S3 бакета"
            echo "  --s3-prefix             Префикс в S3"
            echo "  --slack-webhook         Webhook URL для Slack уведомлений"
            echo "  --health-check-url      URL для health check ping"
            echo "  -v, --verbose           Подробный вывод"
            echo "  --help                  Показать эту справку"
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
# 1. Простой бэкап локальной базы:
#    ./scripts/backup-postgres.sh --type full --environment development \
#         --host localhost --user postgres --password postgres
#
# 2. Бэкап Yandex Managed PostgreSQL с загрузкой в S3:
#    ./scripts/backup-postgres.sh --type full --environment production \
#         --host your-postgres-host.yandexcloud.net --port 6432 \
#         --user appuser --password your-password --ssl-mode verify-full \
#         --ca-cert ./certs/yandex-ca.pem --s3-enabled --s3-bucket my-backups \
#         --slack-webhook https://hooks.slack.com/...
#
# 3. Бэкап только схемы:
#    ./scripts/backup-postgres.sh --type schema-only --environment staging \
#         --host postgres.staging.svc.cluster.local --user postgres