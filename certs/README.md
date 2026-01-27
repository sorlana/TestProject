# SSL/TLS Сертификаты для TestProject

Эта папка содержит SSL/TLS сертификаты для безопасного подключения к внешним сервисам.

## 📁 Содержимое

- `yandex-ca.pem` - Корневой сертификат Yandex Cloud для подключения к Managed PostgreSQL
- `generate-dev-cert.sh` - Скрипт для генерации самоподписанных сертификатов для разработки
- `dev/` - Директория для сертификатов разработки
- `production/` - Директория для продакшен сертификатов (не хранить в репозитории!)

## 🔐 Получение сертификатов

### Yandex Cloud CA Certificate

**Для продакшена:**
1. Скачайте официальный сертификат Yandex Cloud:
   ```bash
   # Скачать корневой сертификат
   curl -o yandex-ca.pem https://storage.yandexcloud.net/cloud-certs/CA.pem
   
   # Проверить сертификат
   openssl x509 -in yandex-ca.pem -text -noout