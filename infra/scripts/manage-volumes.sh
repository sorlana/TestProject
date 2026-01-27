#!/bin/bash
# Скрипт управления томами TestProject

set -e

VOLUME_PREFIX="testproject"

show_help() {
    echo "Управление томами TestProject"
    echo "Использование: $0 [команда]"
    echo ""
    echo "Команды:"
    echo "  list        - Показать все тома проекта"
    echo "  create      - Создать все тома проекта"
    echo "  backup      - Создать бэкап всех томов"
    echo "  restore     - Восстановить тома из бэкапа"
    echo "  clean       - Удалить все тома проекта (ОПАСНО!)"
    echo "  inspect     - Показать детальную информацию о томах"
    echo "  size        - Показать размеры томов"
}

list_volumes() {
    echo "📦 Тома TestProject:"
    docker volume ls --filter "name=${VOLUME_PREFIX}" --format "table {{.Name}}\t{{.Driver}}\t{{.Mountpoint}}"
    
    echo ""
    echo "Всего томов: $(docker volume ls --filter "name=${VOLUME_PREFIX}" --format "{{.Name}}" | wc -l)"
}

create_volumes() {
    echo "🔄 Создание томов для TestProject..."
    
    # Создаем основные тома
    docker volume create ${VOLUME_PREFIX}-postgres-data
    docker volume create ${VOLUME_PREFIX}-app-logs
    docker volume create ${VOLUME_PREFIX}-redis-data
    docker volume create ${VOLUME_PREFIX}-pgadmin-data
    
    echo "✅ Тома созданы:"
    list_volumes
}

backup_volumes() {
    echo "💾 Создание бэкапа томов..."
    
    BACKUP_DIR="./backups/volumes/$(date +%Y%m%d_%H%M%S)"
    mkdir -p ${BACKUP_DIR}
    
    for volume in $(docker volume ls --filter "name=${VOLUME_PREFIX}" --format "{{.Name}}"); do
        echo "Бэкапируем том: ${volume}"
        docker run --rm -v ${volume}:/source -v ${BACKUP_DIR}:/backup alpine \
            sh -c "cd /source && tar -czf /backup/${volume}.tar.gz ."
    done
    
    echo "✅ Бэкапы созданы в: ${BACKUP_DIR}"
    ls -la ${BACKUP_DIR}/
}

restore_volume() {
    VOLUME_NAME=$1
    BACKUP_FILE=$2
    
    if [ -z "${VOLUME_NAME}" ] || [ -z "${BACKUP_FILE}" ]; then
        echo "❌ Использование: $0 restore [имя_тома] [файл_бэкапа]"
        exit 1
    fi
    
    if [ ! -f "${BACKUP_FILE}" ]; then
        echo "❌ Файл бэкапа не найден: ${BACKUP_FILE}"
        exit 1
    fi
    
    echo "🔄 Восстановление тома ${VOLUME_NAME} из ${BACKUP_FILE}"
    
    # Удаляем старый том если существует
    docker volume rm ${VOLUME_NAME} 2>/dev/null || true
    
    # Создаем новый том
    docker volume create ${VOLUME_NAME}
    
    # Восстанавливаем данные
    docker run --rm -v ${VOLUME_NAME}:/target -v $(pwd)/${BACKUP_FILE}:/backup.tar.gz alpine \
        sh -c "cd /target && tar -xzf /backup.tar.gz"
    
    echo "✅ Том ${VOLUME_NAME} восстановлен"
}

clean_volumes() {
    echo "⚠️  ВНИМАНИЕ: Вы собираетесь удалить ВСЕ тома TestProject!"
    echo "Это приведёт к ПОТЕРЕ ВСЕХ ДАННЫХ!"
    read -p "Продолжить? (y/N): " confirm
    
    if [ "${confirm}" != "y" ]; then
        echo "❌ Отменено"
        exit 0
    fi
    
    for volume in $(docker volume ls --filter "name=${VOLUME_PREFIX}" --format "{{.Name}}"); do
        echo "Удаляем том: ${volume}"
        docker volume rm ${volume}
    done
    
    echo "✅ Все тома удалены"
}

inspect_volumes() {
    echo "🔍 Детальная информация о томах:"
    
    for volume in $(docker volume ls --filter "name=${VOLUME_PREFIX}" --format "{{.Name}}"); do
        echo ""
        echo "=== Том: ${volume} ==="
        docker volume inspect ${volume} | jq '.[0] | {Name: .Name, Driver: .Driver, Mountpoint: .Mountpoint, Labels: .Labels}'
        
        # Показываем содержимое (если не пусто)
        SIZE=$(docker run --rm -v ${volume}:/data alpine sh -c "du -sh /data 2>/dev/null | cut -f1")
        echo "Размер: ${SIZE}"
    done
}

check_volume_sizes() {
    echo "📊 Размеры томов:"
    
    for volume in $(docker volume ls --filter "name=${VOLUME_PREFIX}" --format "{{.Name}}"); do
        SIZE=$(docker run --rm -v ${volume}:/data alpine sh -c "du -sh /data 2>/dev/null | cut -f1" || echo "0B")
        echo "  ${volume}: ${SIZE}"
    done
}

# Обработка команд
case "$1" in
    list)
        list_volumes
        ;;
    create)
        create_volumes
        ;;
    backup)
        backup_volumes
        ;;
    restore)
        restore_volume "$2" "$3"
        ;;
    clean)
        clean_volumes
        ;;
    inspect)
        inspect_volumes
        ;;
    size)
        check_volume_sizes
        ;;
    *)
        show_help
        ;;
esac