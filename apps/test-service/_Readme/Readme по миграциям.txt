# Перейдите в проект с DbContext
cd "D:\SITES\My\_for_Qdrant\Test\Infrastructure\Infrastructure.EntityFramework"

# Создайте миграцию из этой папки
dotnet ef migrations add InitialPostgreSQL --context DatabaseContext --output-dir Migrations --startup-project ..\..\WebApi

# Примените миграцию к PostgreSQL (Находясь в той же папке (Infrastructure.EntityFramework))
dotnet ef database update --context DatabaseContext --startup-project ..\..\WebApi