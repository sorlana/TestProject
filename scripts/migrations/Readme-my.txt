🚀 Использование
Основные команды после настройки алиасов:
powershell
# Показать статус миграций
mig-status

# Создать новую миграцию
mig-add "AddDescriptionToCourse"

# Применить все миграции
mig-up

# Сгенерировать SQL скрипт
mig-script -FromMigration "InitialCreate" -ToMigration "AddDescriptionToCourse"

# Показать список всех миграций
mig-list
Или используйте интерактивное меню:
powershell
.\scripts\dev-migrate.ps1
🎯 Workflow работы с миграциями
1. При изменении модели:
powershell
# 1. Измените модель в Domain.Entities
# 2. Создайте миграцию
mig-add "AddNewPropertyToModel"

# 3. Проверьте созданный файл
# 4. Примените миграцию
mig-up

# 5. Проверьте статус
mig-status
2. Перед коммитом:
powershell
# Убедитесь, что все миграции применены
mig-status

# Если есть pending миграции
mig-up
3. При работе в команде:
powershell
# Получите миграции из Git
git pull

# Примените новые миграции
mig-up

# Проверьте статус
mig-status