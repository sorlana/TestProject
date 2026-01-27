# Используем SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем ВСЕ файлы (самый простой способ)
COPY . .

# Публикуем WebApi проект
RUN dotnet publish "apps/test-service/WebApi/WebApi.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]