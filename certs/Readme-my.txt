Для разработки:

bash
# Используйте скрипт генерации самоподписанных сертификатов
./certs/generate-dev-cert.sh
Самоподписанные сертификаты (только для разработки)
Для локальной разработки можно использовать самоподписанные сертификаты:

bash
# Генерация самоподписанного CA
openssl req -x509 -newkey rsa:4096 -keyout ca-key.pem -out ca-cert.pem -days 365 -nodes -subj "/CN=TestProject Dev CA"

# Генерация сертификата для PostgreSQL
openssl req -newkey rsa:4096 -nodes -keyout postgres-key.pem -out postgres-req.pem -subj "/CN=localhost"
openssl x509 -req -in postgres-req.pem -CA ca-cert.pem -CAkey ca-key.pem -CAcreateserial -out postgres-cert.pem -days 365

# Проверка
openssl verify -CAfile ca-cert.pem postgres-cert.pem
🚀 Использование
В Docker Compose
yaml
services:
  test-service:
    volumes:
      - ./certs:/certs:ro  # Монтируем папку с сертификатами
    environment:
      SSL_CA_PATH: /certs/yandex-ca.pem
      SSL_MODE: verify-full
В Kubernetes
yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: yandex-ca-cert
data:
  yandex-ca.pem: |
    -----BEGIN CERTIFICATE-----
    # Содержимое сертификата
    -----END CERTIFICATE-----
В коде приложения
javascript
// config/database.js
const fs = require('fs');

const dbConfig = {
  ssl: process.env.NODE_ENV === 'production' ? {
    rejectUnauthorized: true,
    ca: fs.readFileSync(process.env.SSL_CA_PATH || '/certs/yandex-ca.pem')
  } : false
};
🔧 Конфигурация для разных окружений
Разработка (Development)
bash
# Используйте самоподписанные сертификаты или отключите SSL
export SSL_MODE=disable
# или
export SSL_MODE=require
export SSL_CA_PATH=./certs/dev/ca-cert.pem
Тестирование (Staging)
bash
# Используйте тестовые сертификаты
export SSL_MODE=verify-ca
export SSL_CA_PATH=./certs/staging/ca-cert.pem
Продакшен (Production)
bash
# Используйте официальные сертификаты Yandex Cloud
export SSL_MODE=verify-full
export SSL_CA_PATH=./certs/yandex-ca.pem
📋 Проверка сертификатов
Проверка валидности
bash
# Проверить даты действия
openssl x509 -in yandex-ca.pem -dates -noout

# Проверить отпечаток (fingerprint)
openssl x509 -in yandex-ca.pem -fingerprint -noout

# Проверить цепочку доверия
openssl verify -CAfile yandex-ca.pem your-certificate.pem
Проверка подключения
bash
# Проверить подключение к PostgreSQL с SSL
psql "host=your-db.mdb.yandexcloud.net port=6432 \
      dbname=testdb user=test_user \
      sslmode=verify-full sslrootcert=./certs/yandex-ca.pem"

# Проверить с помощью OpenSSL
openssl s_client -connect your-db.mdb.yandexcloud.net:6432 \
  -CAfile ./certs/yandex-ca.pem \
  -verify_return_error
🛡️ Безопасность
Хранение сертификатов
В репозитории: Только публичные CA сертификаты (как yandex-ca.pem)

НЕ в репозитории: Приватные ключи, самоподписанные сертификаты для продакшена

В Docker образах: Сертификаты добавляются на этапе сборки

В Kubernetes: Используйте Secrets для приватных ключей, ConfigMaps для публичных

Ротация сертификатов
Yandex Cloud CA: Обновляется автоматически, следите за уведомлениями

Самоподписанные: Обновляйте каждые 90 дней

Процесс ротации:

bash
# 1. Генерация новых сертификатов
# 2. Тестирование
# 3. Замена в staging
# 4. Замена в production (zero-downtime)
🔄 Обновление сертификатов Yandex Cloud
Yandex Cloud обновляет свои корневые сертификаты. Следите за обновлениями:

Подпишитесь на уведомления в Yandex Cloud Console

Проверяйте даты окончания действия:

bash
openssl x509 -in yandex-ca.pem -enddate -noout
Актуальная версия всегда доступна по URL:

text
https://storage.yandexcloud.net/cloud-certs/CA.pem
🚨 Устранение неполадок
Ошибка: "SSL connection is required"
bash
# Проверьте наличие сертификата
ls -la ./certs/yandex-ca.pem

# Проверьте права доступа
chmod 644 ./certs/yandex-ca.pem

# Проверьте содержимое
head -5 ./certs/yandex-ca.pem
Ошибка: "certificate verify failed"
bash
# Проверьте цепочку доверия
openssl verify -CAfile ./certs/yandex-ca.pem ./certs/yandex-ca.pem

# Проверьте срок действия
openssl x509 -in ./certs/yandex-ca.pem -checkend 86400

# Скачайте свежую версию
curl -f https://storage.yandexcloud.net/cloud-certs/CA.pem -o ./certs/yandex-ca.pem
Ошибка в Kubernetes
bash
# Проверьте ConfigMap
kubectl get configmap yandex-ca-cert -o yaml

# Проверьте монтирование
kubectl describe pod test-service-xxx | grep -A5 -B5 cert

# Проверьте файл внутри пода
kubectl exec test-service-xxx -- ls -la /certs/
kubectl exec test-service-xxx -- cat /certs/yandex-ca.pem | head -3
📁 Структура папки certs (рекомендуемая)
text
certs/
├── README.md                    # Эта документация
├── yandex-ca.pem               # Корневой сертификат Yandex Cloud
├── generate-dev-cert.sh        # Скрипт генерации сертификатов для разработки
├── dev/                        # Сертификаты для разработки
│   ├── ca-cert.pem            # Самоподписанный CA
│   ├── ca-key.pem             # Приватный ключ CA (НЕ в репозитории!)
│   ├── postgres-cert.pem      # Сертификат для PostgreSQL
│   └── postgres-key.pem       # Приватный ключ PostgreSQL (НЕ в репозитории!)
├── staging/                    # Сертификаты для staging (опционально)
└── .gitignore                 # Игнорирование приватных ключей
⚠️ Важные замечания
Никогда не коммитьте приватные ключи в репозиторий

Используйте разные сертификаты для разных окружений

Регулярно обновляйте сертификаты

Мониторьте сроки действия сертификатов

Тестируйте подключение перед деплоем в продакшен

🚀 Как создать папку certs и файлы:
bash
# Создаём папку certs
mkdir -p certs

# Создаём файлы
cd certs
touch README.md .gitignore generate-dev-cert.sh yandex-ca.pem

# Делаем скрипт исполняемым
chmod +x generate-dev-cert.sh

# Создаём структуру папок
mkdir -p dev staging production

# Добавляем .gitignore для приватных ключей
echo "*.key" > .gitignore
echo "*-key.pem" >> .gitignore
echo "production/" >> .gitignore
echo "dev/*.key" >> .gitignore
echo "dev/*-key.pem" >> .gitignore
echo "staging/*.key" >> .gitignore
echo "staging/*-key.pem" >> .gitignore

# Копируем содержимое файлов из кода выше
🔧 Быстрый старт с SSL для TestProject:
1. Генерация сертификатов для разработки:
bash
cd certs
./generate-dev-cert.sh
2. Использование в Docker Compose:
yaml
# В docker-compose.yaml
services:
  postgres-test:
    volumes:
      - ./certs/dev/postgres-combined.pem:/var/lib/postgresql/data/server.crt:ro
      - ./certs/dev/postgres-key.pem:/var/lib/postgresql/data/server.key:ro
    command: postgres -c ssl=on -c ssl_cert_file=/var/lib/postgresql/data/server.crt -c ssl_key_file=/var/lib/postgresql/data/server.key
  
  test-service:
    volumes:
      - ./certs/dev/ca-cert.pem:/certs/ca.pem:ro
    environment:
      DB_SSL: "true"
      SSL_CA_PATH: /certs/ca.pem
3. Использование в Kubernetes:
bash
# Создаём ConfigMap с сертификатом
kubectl create configmap yandex-ca-cert \
  --from-file=yandex-ca.pem=./certs/yandex-ca.pem \
  -n testproject-namespace

# Или через манифест
kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: yandex-ca-cert
  namespace: testproject-namespace
data:
  yandex-ca.pem: |
    $(cat ./certs/yandex-ca.pem | sed 's/^/    /')
EOF
📋 Проверка сертификатов:
bash
# Проверить Yandex Cloud сертификат
openssl x509 -in certs/yandex-ca.pem -text -noout | grep -E "Subject:|Not Before:|Not After:|CA:"

# Проверить самоподписанные сертификаты
openssl verify -CAfile certs/dev/ca-cert.pem certs/dev/postgres-cert.pem

# Проверить подключение (если есть доступ к БД)
psql "host=your-db.mdb.yandexcloud.net port=6432 \
      sslmode=verify-full sslrootcert=./certs/yandex-ca.pem" \
      -c "SELECT 1;"
🛡️ Важные моменты:
yandex-ca.pem — публичный сертификат, безопасно хранить в репозитории

Приватные ключи (*.key, *-key.pem) — НИКОГДА не коммитить!

Скрипт генерации создаёт сертификаты только для разработки

Для продакшена всегда используйте официальные сертификаты Yandex Cloud

Теперь у вас есть полная система управления SSL/TLS сертификатами для TestProject! 🔐🚀