# Манифесты для TestService (.NET 10)

Эта папка содержит Kubernetes манифесты для .NET 10 приложения TestService.

## Файлы

### Основные манифесты

1. **configmap.yaml** - Конфигурационные данные приложения
   - Настройки среды выполнения
   - Параметры логирования
   - Настройки CORS, Rate Limiting и т.д.
   - Инициализационные скрипты БД

2. **secret.yaml** - Шаблон секретов приложения
   - Пароли БД
   - JWT ключи
   - API ключи внешних сервисов
   - TLS сертификаты

3. **generate-secrets.sh** - Скрипт генерации секретов

### Использование

#### 1. Генерация секретов для локальной разработки

```bash
# Дать права на выполнение
chmod +x manifests/generate-secrets.sh

# Запустить генерацию
./manifests/generate-secrets.sh testproject-namespace

# Показать сгенерированные секреты (только для разработки!)
./manifests/generate-secrets.sh testproject-namespace --show-secrets