# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["LandingCms.csproj", "./"]
RUN dotnet restore "LandingCms.csproj"

# Копируем остальные файлы и собираем приложение
COPY . .
RUN dotnet build "LandingCms.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "LandingCms.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Создаем директорию для логов
RUN mkdir -p /app/logs

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# Открываем порт 80
EXPOSE 80

# Запускаем приложение
ENTRYPOINT ["dotnet", "LandingCms.dll"]
