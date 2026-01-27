# Основная функция
main() {
    log_info "Запуск проверки здоровья инфраструктуры"
    log_info "Окружение: $ENVIRONMENT"
    
    # Массив для хранения последовательности проверок
    local checks=()
    
    # Добавляем проверки в зависимости от настроек
    if [ "$CHECK_DATABASE" = "true" ]; then
        checks+=("database")
    fi
    
    if [ "$CHECK_KUBERNETES" = "true" ]; then
        checks+=("kubernetes")
    fi
    
    if [ "$CHECK_APPLICATION" = "true" ]; then
        checks+=("application")
    fi
    
    if [ "$CHECK_NETWORK" = "true" ]; then
        checks+=("network")
    fi
    
    if [ "$CHECK_DISK" = "true" ]; then
        checks+=("disk")
    fi
    
    if [ "$CHECK_MEMORY" = "true" ]; then
        checks+=("memory")
    fi
    
    if [ "$CHECK_CPU" = "true" ]; then
        checks+=("cpu")
    fi
    
    if [ "$CHECK_CERTIFICATES" = "true" ]; then
        checks+=("certificates")
    fi
    
    # Запускаем все проверки
    for check in "${checks[@]}"; do
        case $check in
            database)
                run_check "База данных" check_database
                ;;
            kubernetes)
                run_check "Kubernetes кластер" check_kubernetes
                ;;
            application)
                run_check "Приложение" check_application
                ;;
            network)
                run_check "Сетевое подключение" check_network
                ;;
            disk)
                run_check "Дисковое пространство" check_disk
                ;;
            memory)
                run_check "Оперативная память" check_memory
                ;;
            cpu)
                run_check "Загрузка CPU" check_cpu
                ;;
            certificates)
                run_check "SSL сертификаты" check_certificates
                ;;
        esac
    done
    
    # Выводим результаты в выбранном формате
    case $OUTPUT_FORMAT in
        json)
            output_json
            ;;
        prometheus)
            output_prometheus
            ;;
        *)
            output_text
            ;;
    esac
    
    # Определяем код возврата
    local failed_count=0
    for result in "${CHECK_RESULTS[@]}"; do
        if [ "$result" = "failed" ]; then
            ((failed_count++))
        fi
    done
    
    if [ "$failed_count" -eq 0 ]; then
        log_success "Все проверки завершены успешно"
        exit 0
    else
        log_error "Найдены проблемы: $failed_count проверок не пройдено"
        exit 1
    fi
}

# Точка входа
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    parse_arguments "$@"
    main
fi