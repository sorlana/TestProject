#!/bin/bash
# Скрипт восстановления PostgreSQL из резервной копии
# Поддерживает: локальные базы, Yandex Managed PostgreSQL, зашифрованные бэкапы

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
BACKUP_FILE=${BACKUP_FILE:-""}
ENVIRONMENT=${ENVIRONMENT:-"production"}
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-"testdb"}
DB_USER=${DB_USER:-"postgres"}
DB_PASSWORD=${DB_PASSWORD:-""}
SSL_MODE=${SSL_MODE:-"disable"}
CA_CERT_PATH=${CA_CERT_PATH:-""}
RESTORE_METHOD=${RESTORE_METHOD:-"replace"}  # replace, merge, schema-only
DROP_DATABASE=${DROP_DATABASE:-"false"}
CREATE_DATABASE=${CREATE_DATABASE:-"true"}
VERIFY_BACKUP=${VERIFY_BACKUP:-"true"}
ENCRYPTION_PASSWORD=${ENCRYPTION_PASSWORD:-""}
PRE_RESTORE_SCRIPT=${PRE_RESTORE_SCRIPT:-""}
POST_RESTORE_SCRIPT=${POST_RESTORE_SCRIPT:-""}
VERBOSE=${VERBOSE:-"false"}
CONFIRM_PROMPT=${CONFIRM_PROMPT:-"true"}

# Функция для проверки зависимостей
check_dependencies() {
    local dependencies=("psql")
    
    if [[ "$BACKUP_FILE" == *.gz ]]; then
        dependencies+=("gzip")
    elif [[ "$BACKUP_FILE" == *.bz2 ]]; then
        dependencies+=("bzip2")
    elif [[ "$BACKUP_FILE" == *.enc ]]; then
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

# Функция для проверки файла бэкапа
check_backup_file() {
    if [ -z "$BACKUP_FILE" ]; then
        log_error "Не указан файл бэкапа"
        exit 1
    fi
    
    if [ ! -f "$BACKUP_FILE" ]; then
        log_error "Файл бэкапа не найден: $BACKUP_FILE"
        exit 1
    fi
    
    if [ ! -s "$BACKUP_FILE" ]; then
        log_error "Файл бэкапа пуст"
        exit 1
    fi
    
    log_success "Файл бэкапа найден: $(basename "$BACKUP_FILE")"
    log_info "Размер: $(du -h "$BACKUP_FILE" | cut -f1)"
}

# Функция для верификации бэкапа
verify_backup() {
    if [ "$VERIFY_BACKUP" != "true" ]; then
        log_warning "Верификация бэкапа отключена"
        return 0
    fi
    
    log_info "Верификация бэкапа..."
    
    local backup_file="$1"
    
    # Проверяем контрольную сумму
    if [ -f "${backup_file}.sha256" ]; then
        log_info "Проверка контрольной суммы..."
        if sha256sum -c "${backup_file}.sha256" > /dev/null 2>&1; then
            log_success "Контрольная сумма верна"
        else
            log_error "Контрольная сумма не совпадает"
            return 1
        fi
    else
        log_warning "Файл контрольной суммы не найден"
    fi
    
    # Проверяем, что файл является валидным архивом
    if [[ "$backup_file" == *.gz ]]; then
        log_info "Проверка gzip архива..."
        if ! gzip -t "$backup_file" 2>/dev/null; then
            log_error "Файл бэкапа поврежден (gzip)"
            return 1
        fi
    elif [[ "$backup_file" == *.bz2 ]]; then
        log_info "Проверка bzip2 архива..."
        if ! bzip2 -t "$backup_file" 2>/dev/null; then
            log_error "Файл бэкапа поврежден (bzip2)"
            return 1
        fi
    elif [[ "$backup_file" == *.enc ]]; then
        log_info "Проверка зашифрованного файла..."
        if [ -z "$ENCRYPTION_PASSWORD" ]; then
            log_error "Требуется пароль для расшифровки"
            return 1
        fi
    fi
    
    log_success "Бэкап прошел базовую верификацию"
    return 0
}

# Функция для расшифровки бэкапа
decrypt_backup() {
    local encrypted_file="$1"
    local decrypted_file="${encrypted_file%.enc}"
    
    if [[ "$encrypted_file" != *.enc ]]; then
        echo "$encrypted_file"
        return 0
    fi
    
    if [ -z "$ENCRYPTION_PASSWORD" ]; then
        log_error "Не указан пароль для расшифровки"
        exit 1
    fi
    
    log_info "Расшифровка бэкапа..."
    
    openssl enc -d -aes-256-cbc -pbkdf2 \
        -in "$encrypted_file" \
        -out "$decrypted_file" \
        -pass pass:"$ENCRYPTION_PASSWORD" 2>/dev/null
    
    if [ $? -ne 0 ]; then
        log_error "Не удалось расшифровать бэкап. Проверьте пароль."
        rm -f "$decrypted_file"
        exit 1
    fi
    
    log_success "Бэкап расшифрован"
    echo "$decrypted_file"
}

# Функция для распаковки бэкапа
extract_backup() {
    local backup_file="$1"
    local extracted_file="${backup_file%.*}"
    
    if [[ "$backup_file" == *.gz ]]; then
        log_info "Распаковка gzip архива..."
        gzip -dc "$backup_file" > "$extracted_file"
    elif [[ "$backup_file" == *.bz2 ]]; then
        log_info "Распаковка bzip2 архива..."
        bzip2 -dc "$backup_file" > "$extracted_file"
    else
        echo "$backup_file"
        return 0
    fi
    
    if [ $? -ne 0 ]; then
        log_error "Ошибка при распаковке бэкапа"
        rm -f "$extracted_file"
        exit 1
    fi
    
    log_success "Бэкап распакован"
    echo "$extracted_file"
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

# Функция для проверки существования базы данных
check_database_exists() {
    log_info "Проверка существования базы данных '$DB_NAME'..."
    
    local check_sql="SELECT 1 FROM pg_database WHERE datname = '$DB_NAME';"
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            local result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                -t -A 2>/dev/null)
        else
            local result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "postgres" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                -t -A 2>/dev/null)
        fi
    else
        local result=$(PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "postgres" \
            -c "$check_sql" \
            -t -A 2>/dev/null)
    fi
    
    if [ "$result" = "1" ]; then
        log_warning "База данных '$DB_NAME' уже существует"
        return 0
    else
        log_info "База данных '$DB_NAME' не существует"
        return 1
    fi
}

# Функция для удаления базы данных
drop_database() {
    if [ "$DROP_DATABASE" != "true" ]; then
        return 0
    fi
    
    log_warning "Удаление базы данных '$DB_NAME'..."
    
    # Завершаем все активные подключения
    local terminate_sql="
        SELECT pg_terminate_backend(pid) 
        FROM pg_stat_activity 
        WHERE datname = '$DB_NAME' 
        AND pid <> pg_backend_pid();
    "
    
    execute_sql "postgres" "$terminate_sql" || true
    
    # Удаляем базу данных
    local drop_sql="DROP DATABASE IF EXISTS $DB_NAME;"
    
    if execute_sql "postgres" "$drop_sql"; then
        log_success "База данных удалена"
    else
        log_error "Не удалось удалить базу данных"
        exit 1
    fi
}

# Функция для создания базы данных
create_database() {
    if [ "$CREATE_DATABASE" != "true" ]; then
        return 0
    fi
    
    log_info "Создание базы данных '$DB_NAME'..."
    
    local create_sql="
        CREATE DATABASE $DB_NAME
        WITH 
        OWNER = $DB_USER
        ENCODING = 'UTF8'
        LC_COLLATE = 'C'
        LC_CTYPE = 'C'
        TEMPLATE = template0
        CONNECTION LIMIT = -1;
    "
    
    if execute_sql "postgres" "$create_sql"; then
        log_success "База данных создана"
    else
        log_error "Не удалось создать базу данных"
        exit 1
    fi
}

# Функция для выполнения SQL
execute_sql() {
    local database="$1"
    local sql="$2"
    
    if [ "$VERBOSE" = "true" ]; then
        log_info "Выполнение SQL в базе $database: ${sql:0:100}..."
    fi
    
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$database" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                --set=ON_ERROR_STOP=1 \
                > /dev/null 2>&1
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$database" \
                -c "$sql" \
                --set=sslmode="$SSL_MODE" \
                --set=ON_ERROR_STOP=1 \
                > /dev/null 2>&1
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$database" \
            -c "$sql" \
            --set=ON_ERROR_STOP=1 \
            > /dev/null 2>&1
    fi
    
    return $?
}

# Функция для выполнения предварительного скрипта
run_pre_restore_script() {
    if [ -n "$PRE_RESTORE_SCRIPT" ] && [ -f "$PRE_RESTORE_SCRIPT" ]; then
        log_info "Выполнение предварительного скрипта..."
        
        # Устанавливаем переменные окружения для скрипта
        export DB_HOST DB_PORT DB_NAME DB_USER DB_PASSWORD
        export SSL_MODE CA_CERT_PATH ENVIRONMENT
        
        if bash "$PRE_RESTORE_SCRIPT"; then
            log_success "Предварительный скрипт выполнен"
        else
            log_error "Ошибка в предварительном скрипте"
            exit 1
        fi
    fi
}

# Функция для выполнения скрипта после восстановления
run_post_restore_script() {
    if [ -n "$POST_RESTORE_SCRIPT" ] && [ -f "$POST_RESTORE_SCRIPT" ]; then
        log_info "Выполнение скрипта после восстановления..."
        
        # Устанавливаем переменные окружения для скрипта
        export DB_HOST DB_PORT DB_NAME DB_USER DB_PASSWORD
        export SSL_MODE CA_CERT_PATH ENVIRONMENT
        
        if bash "$POST_RESTORE_SCRIPT"; then
            log_success "Скрипт после восстановления выполнен"
        else
            log_error "Ошибка в скрипте после восстановления"
            exit 1
        fi
    fi
}

# Функция для восстановления базы данных
restore_database() {
    local sql_file="$1"
    
    log_info "Восстановление базы данных из файла: $(basename "$sql_file")"
    
    # Проверяем, что файл является SQL дампом
    local first_line=$(head -1 "$sql_file")
    if ! echo "$first_line" | grep -q "PostgreSQL database dump"; then
        log_error "Файл не является PostgreSQL дампом"
        return 1
    fi
    
    # Восстанавливаем базу данных
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
                --set=ON_ERROR_STOP=1 \
                > "${sql_file}.restore.log" 2>&1
        else
            PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -f "$sql_file" \
                --set=sslmode="$SSL_MODE" \
                --set=ON_ERROR_STOP=1 \
                > "${sql_file}.restore.log" 2>&1
        fi
    else
        PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -f "$sql_file" \
            --set=ON_ERROR_STOP=1 \
            > "${sql_file}.restore.log" 2>&1
    fi
    
    local restore_status=$?
    
    if [ $restore_status -eq 0 ]; then
        log_success "База данных восстановлена"
        return 0
    else
        log_error "Ошибка при восстановлении базы данных"
        log_error "Логи восстановления:"
        tail -50 "${sql_file}.restore.log" >&2
        return 1
    fi
}

# Функция для проверки восстановленной базы
verify_restored_database() {
    log_info "Проверка восстановленной базы данных..."
    
    local check_sql="
        SELECT 
            'Таблицы' as check,
            COUNT(*)::text as result
        FROM information_schema.tables 
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
        UNION ALL
        SELECT 
            'Миграции',
            COUNT(*)::text 
        FROM public.__efmigrationshistory
        UNION ALL
        SELECT 
            'Размер',
            pg_size_pretty(pg_database_size('$DB_NAME'));
    "
    
    local result
    if [ "$SSL_MODE" = "require" ] || [ "$SSL_MODE" = "verify-full" ]; then
        if [ -n "$CA_CERT_PATH" ] && [ -f "$CA_CERT_PATH" ]; then
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                --set=sslrootcert="$CA_CERT_PATH" \
                -t -A -F "|" 2>/dev/null)
        else
            result=$(PGPASSWORD="$DB_PASSWORD" psql \
                -h "$DB_HOST" \
                -p "$DB_PORT" \
                -U "$DB_USER" \
                -d "$DB_NAME" \
                -c "$check_sql" \
                --set=sslmode="$SSL_MODE" \
                -t -A -F "|" 2>/dev/null)
        fi
    else
        result=$(PGPASSWORD="$DB_PASSWORD" psql \
            -h "$DB_HOST" \
            -p "$DB_PORT" \
            -U "$DB_USER" \
            -d "$DB_NAME" \
            -c "$check_sql" \
            -t -A -F "|" 2>/dev/null)
    fi
    
    if [ -n "$result" ]; then
        echo -e "\n${GREEN}📊 Проверка восстановленной базы:${NC}"
        echo "=============================="
        echo "$result" | column -t -s '|'
        echo "=============================="
        return 0
    else
        log_error "Не удалось проверить восстановленную базу"
        return 1
    fi
}

# Функция для очистки временных файлов
cleanup_temp_files() {
    log_info "Очистка временных файлов..."
    
    # Удаляем распакованные файлы (если они были созданы)
    if [ -f "${BACKUP_FILE%.*}" ] && [ "${BACKUP_FILE%.*}" != "$BACKUP_FILE" ]; then
        rm -f "${BACKUP_FILE%.*}"
    fi
    
    # Удаляем расшифрованные файлы
    if [ -f "${BACKUP_FILE%.enc}" ] && [ "${BACKUP_FILE%.enc}" != "$BACKUP_FILE" ]; then
        rm -f "${BACKUP_FILE%.enc}"
    fi
    
    # Удаляем логи восстановления
    rm -f "${BACKUP_FILE}.restore.log" 2>/dev/null || true
    
    log_success "Временные файлы очищены"
}

# Функция для запроса подтверждения
confirm_action() {
    if [ "$CONFIRM_PROMPT" != "true" ]; then
        return 0
    fi
    
    echo -e "${YELLOW}⚠️  ВНИМАНИЕ:${NC}"
    echo "Вы собираетесь восстановить базу данных '$DB_NAME'"
    echo "Хост: $DB_HOST:$DB_PORT"
    echo "Пользователь: $DB_USER"
    echo "Файл бэкапа: $(basename "$BACKUP_FILE")"
    echo ""
    
    if [ "$DROP_DATABASE" = "true" ]; then
        echo -e "${RED}Существующая база данных будет УДАЛЕНА!${NC}"
    fi
    
    read -p "Продолжить? (y/N): " -n 1 -r
    echo
    
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Восстановление отменено"
        exit 0
    fi
}

# Основная функция
main() {
    local start_time=$(date +%s)
    
    log_info "🚀 Начало восстановления базы данных"
    log_info "Окружение: $ENVIRONMENT"
    log_info "База данных: $DB_NAME@$DB_HOST:$DB_PORT"
    log_info "Файл бэкапа: $(basename "$BACKUP_FILE")"
    
    # Запрашиваем подтверждение
    confirm_action
    
    # Проверяем зависимости
    check_dependencies
    
    # Проверяем файл бэкапа
    check_backup_file
    
    # Верифицируем бэкап
    if ! verify_backup "$BACKUP_FILE"; then
        log_error "Бэкап не прошел верификацию"
        exit 1
    fi
    
    # Проверяем подключение к БД
    if ! check_db_connection; then
        log_error "Не удалось подключиться к базе данных"
        exit 1
    fi
    
    # Выполняем предварительный скрипт
    run_pre_restore_script
    
    # Проверяем существование базы данных
    local db_exists=false
    if check_database_exists; then
        db_exists=true
    fi
    
    # Расшифровываем бэкап (если необходимо)
    local decrypted_file=$(decrypt_backup "$BACKUP_FILE")
    
    # Распаковываем бэкап (если необходимо)
    local sql_file=$(extract_backup "$decrypted_file")
    
    # Удаляем базу данных (если требуется)
    drop_database
    
    # Создаем базу данных (если требуется)
    create_database
    
    # Восстанавливаем базу данных
    if ! restore_database "$sql_file"; then
        log_error "Восстановление не удалось"
        cleanup_temp_files
        exit 1
    fi
    
    # Выполняем скрипт после восстановления
    run_post_restore_script
    
    # Проверяем восстановленную базу
    verify_restored_database
    
    # Очищаем временные файлы
    cleanup_temp_files
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    log_success "✅ Восстановление успешно завершено за $duration секунд"
}

# Обработка аргументов командной строки
while [[ $# -gt 0 ]]; do
    case $1 in
        --backup-file|-f)
            BACKUP_FILE="$2"
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
        --restore-method)
            RESTORE_METHOD="$2"
            shift 2
            ;;
        --drop-database)
            DROP_DATABASE="true"
            shift
            ;;
        --no-create-database)
            CREATE_DATABASE="false"
            shift
            ;;
        --no-verify)
            VERIFY_BACKUP="false"
            shift
            ;;
        --encryption-password)
            ENCRYPTION_PASSWORD="$2"
            shift 2
            ;;
        --pre-restore-script)
            PRE_RESTORE_SCRIPT="$2"
            shift 2
            ;;
        --post-restore-script)
            POST_RESTORE_SCRIPT="$2"
            shift 2
            ;;
        --no-confirm)
            CONFIRM_PROMPT="false"
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
            echo "  -f, --backup-file        Путь к файлу бэкапа (обязательно)"
            echo "  -e, --environment        Окружение (development, staging, production)"
            echo "  -h, --host               Хост БД"
            echo "  -p, --port               Порт БД"
            echo "  -n, --name               Имя базы данных"
            echo "  -u, --user               Пользователь БД"
            echo "  -P, --password           Пароль БД"
            echo "  --ssl-mode               Режим SSL (disable, require, verify-full)"
            echo "  --ca-cert                Путь к CA сертификату"
            echo "  --restore-method         Метод восстановления (replace, merge, schema-only)"
            echo "  --drop-database          Удалить существующую базу данных"
            echo "  --no-create-database     Не создавать базу данных"
            echo "  --no-verify              Не проверять бэкап перед восстановлением"
            echo "  --encryption-password    Пароль для расшифровки бэкапа"
            echo "  --pre-restore-script     Скрипт для выполнения перед восстановлением"
            echo "  --post-restore-script    Скрипт для выполнения после восстановления"
            echo "  --no-confirm             Не запрашивать подтверждение"
            echo "  -v, --verbose            Подробный вывод"
            echo "  --help                   Показать эту справку"
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
# 1. Восстановление локальной базы:
#    ./scripts/restore-postgres.sh --backup-file ./backups/full_backup_20240101_020000.sql.gz \
#         --host localhost --user postgres --password postgres --drop-database
#
# 2. Восстановление в Yandex Managed PostgreSQL:
#    ./scripts/restore-postgres.sh --backup-file ./backups/full_backup_20240101_020000.sql.gz \
#         --host your-postgres-host.yandexcloud.net --port 6432 \
#         --user appuser --password your-password --ssl-mode verify-full \
#         --ca-cert ./certs/yandex-ca.pem --drop-database
#
# 3. Восстановление зашифрованного бэкапа:
#    ./scripts/restore-postgres.sh --backup-file ./backups/full_backup_20240101_020000.sql.gz.enc \
#         --encryption-password your-secret-password --drop-database