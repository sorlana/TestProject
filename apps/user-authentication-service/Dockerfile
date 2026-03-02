# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов и восстанавливаем зависимости
COPY ["Domain/Domain.Entities/Domain.Entities.csproj", "Domain/Domain.Entities/"]
COPY ["Services/Services.Abstractions/Services.Abstractions.csproj", "Services/Services.Abstractions/"]
COPY ["Services/Services.Contracts/Services.Contracts.csproj", "Services/Services.Contracts/"]
COPY ["Services/Services.Implementations/Services.Implementations.csproj", "Services/Services.Implementations/"]
COPY ["Services/Services.Repositories.Abstractions/Services.Repositories.Abstractions.csproj", "Services/Services.Repositories.Abstractions/"]
COPY ["Infrastructure/Infrastructure.EntityFramework/Infrastructure.EntityFramework.csproj", "Infrastructure/Infrastructure.EntityFramework/"]
COPY ["Infrastructure/Infrastructure.Repositories.Implementations/Infrastructure.Repositories.Implementations.csproj", "Infrastructure/Infrastructure.Repositories.Implementations/"]
COPY ["Infrastructure/Infrastructure.Identity/Infrastructure.Identity.csproj", "Infrastructure/Infrastructure.Identity/"]
COPY ["Infrastructure/Infrastructure.ExternalServices/Infrastructure.ExternalServices.csproj", "Infrastructure/Infrastructure.ExternalServices/"]
COPY ["WebApi/WebApi/WebApi.csproj", "WebApi/WebApi/"]

RUN dotnet restore "WebApi/WebApi/WebApi.csproj"

# Копируем весь исходный код
COPY . .

# Собираем приложение
WORKDIR "/src/WebApi/WebApi"
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап выполнения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
