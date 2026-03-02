# Документ проектирования: Лендинг платформы онлайн-обучения

## Обзор

Лендинг платформы онлайн-обучения представляет собой простое веб-приложение на базе Piranha CMS (ASP.NET Core 8.0), которое служит точкой входа для новых пользователей. Лендинг предоставляет информацию о платформе и тарифах, а также обеспечивает переход к фронтенд-микросервису для авторизации и работы с личным кабинетом.

Архитектура приложения следует принципам микросервисной архитектуры, где Landing CMS является отдельным сервисом, отвечающим исключительно за отображение контента. Вся логика аутентификации, авторизации и работы с личным кабинетом вынесена в отдельный фронтенд-микросервис (User Portal).

## Архитектура

### Общая архитектура системы

```
┌─────────────────────────────────────────────────────────────┐
│                        Клиент (Браузер)                      │
└────────────┬────────────────────────────────────────────┬───┘
             │                                             │
             │ HTTP/HTTPS                                  │
             │                                             │
┌────────────▼─────────────────────────────────────────────────┐
│                    Landing CMS Service                        │
│  ┌──────────────────────────────────────────────────────┐    │
│  │           ASP.NET Core 8.0 + Piranha CMS             │    │
│  │  ┌────────────────┐                                  │    │
│  │  │  Razor Pages   │  Серверный рендеринг            │    │
│  │  │  (Landing)     │  Статический контент             │    │
│  │  └────────────────┘                                  │    │
│  │                                                       │    │
│  │  ┌────────────────────────────────────────────────┐ │    │
│  │  │        Piranha CMS Manager (/manager)          │ │    │
│  │  └────────────────────────────────────────────────┘ │    │
│  └──────────────────────────────────────────────────────┘    │
│                           │                                   │
│                           │ Entity Framework Core             │
│                           ▼                                   │
│  ┌──────────────────────────────────────────────────────┐    │
│  │         PostgreSQL (Piranha Schema)                  │    │
│  └──────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
                                │
                                │ Редирект на /app
                                │
┌───────────────────────────────▼───────────────────────────────┐
│              User Portal (React + Blueprint)                   │
│                    Отдельный микросервис                       │
│  ┌──────────────────────────────────────────────────────┐     │
│  │  /app/login - авторизация                           │     │
│  │  /app/register - регистрация                        │     │
│  │  /app/dashboard - личный кабинет                    │     │
│  │  /app/payment - оплата                              │     │
│  └──────────────────────────────────────────────────────┘     │
└────────────────────────────────────────────────────────────────┘
```

### Архитектура Landing CMS

Landing CMS использует простую двухуровневую архитектуру:

1. **Presentation Layer (Слой представления)**
   - Razor Pages для рендеринга лендинга
   - Piranha Manager для административной панели

2. **Infrastructure Layer (Слой инфраструктуры)**
   - Entity Framework Core для работы с БД
   - Piranha CMS для управления контентом

## Компоненты и интерфейсы

### 1. Серверные компоненты (ASP.NET Core)

#### 1.1 LandingController

Контроллер для обработки запросов главной страницы лендинга.

```csharp
/// <summary>
/// Контроллер главной страницы лендинга
/// </summary>
public class LandingController : Controller
{
    private readonly IApi _api; // Piranha CMS API
    private readonly ILogger<LandingController> _logger;
    
    public LandingController(IApi api, ILogger<LandingController> logger)
    {
        _api = api;
        _logger = logger;
    }
    
    /// <summary>
    /// Отображение главной страницы
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Получение данных страницы из Piranha CMS
            var page = await _api.Pages.GetBySlugAsync<LandingPage>("/");
            
            if (page == null)
            {
                _logger.LogWarning("Главная страница не найдена в CMS");
                return NotFound();
            }
            
            return View(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке страницы из CMS");
            return StatusCode(500, "Ошибка загрузки страницы");
        }
    }
}
```

#### 1.2 TariffController

Контроллер для обработки действий с тарифами.

```csharp
/// <summary>
/// Контроллер тарифов
/// </summary>
public class TariffController : Controller
{
    private readonly ILogger<TariffController> _logger;
    
    public TariffController(ILogger<TariffController> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Обработка покупки тарифа
    /// Перенаправляет на фронтенд-микросервис для обработки авторизации и оплаты
    /// </summary>
    [HttpGet]
    public IActionResult Purchase(string tariffId)
    {
        _logger.LogInformation("Редирект на страницу оплаты для тарифа {TariffId}", tariffId);
        
        // Простой редирект на фронтенд-микросервис
        // Фронтенд сам решит: показать авторизацию или страницу оплаты
        return Redirect($"/app/payment?tariffId={tariffId}");
    }
}
```

### 2. Модели данных Piranha CMS

#### 2.1 LandingPage Model

Модель главной страницы лендинга.

```csharp
/// <summary>
/// Модель главной страницы лендинга
/// </summary>
[PageType(Title = "Landing Page", UseBlocks = false)]
public class LandingPage : Page<LandingPage>
{
    /// <summary>
    /// Hero секция
    /// </summary>
    [Region(Title = "Hero Section", Description = "Главная секция с заголовком и описанием")]
    public HeroRegion Hero { get; set; }
    
    /// <summary>
    /// Секция "О программе"
    /// </summary>
    [Region(Title = "About Section", Description = "Описание возможностей платформы")]
    public AboutRegion About { get; set; }
    
    /// <summary>
    /// Секция "Тарифы"
    /// </summary>
    [Region(Title = "Tariffs Section", Description = "Доступные тарифы")]
    public TariffsRegion Tariffs { get; set; }
    
    /// <summary>
    /// SEO данные
    /// </summary>
    [Region(Title = "SEO", Description = "Мета-теги и Open Graph")]
    public SeoRegion Seo { get; set; }
}
```

#### 2.2 HeroRegion

Регион Hero секции.

```csharp
/// <summary>
/// Регион Hero секции
/// </summary>
public class HeroRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Главный заголовок Hero секции")]
    public StringField Title { get; set; }
    
    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "Текст описания под заголовком")]
    public TextField Description { get; set; }
    
    /// <summary>
    /// Фоновое изображение
    /// </summary>
    [Field(Title = "Фоновое изображение", Description = "Изображение для фона Hero секции")]
    public ImageField BackgroundImage { get; set; }
    
    /// <summary>
    /// Текст кнопки
    /// </summary>
    [Field(Title = "Текст кнопки", Description = "Текст на кнопке призыва к действию")]
    public StringField ButtonText { get; set; }
}
```

#### 2.3 AboutRegion

Регион секции "О программе".

```csharp
/// <summary>
/// Регион секции "О программе"
/// </summary>
public class AboutRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Заголовок секции")]
    public StringField Title { get; set; }
    
    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "HTML контент с описанием программы")]
    public HtmlField Description { get; set; }
    
    /// <summary>
    /// Изображение
    /// </summary>
    [Field(Title = "Изображение", Description = "Изображение для секции")]
    public ImageField Image { get; set; }
}
```

#### 2.4 TariffsRegion

Регион секции "Тарифы".

```csharp
/// <summary>
/// Регион секции "Тарифы"
/// </summary>
public class TariffsRegion
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Field(Title = "Заголовок", Description = "Заголовок секции тарифов")]
    public StringField Title { get; set; }
    
    /// <summary>
    /// Список тарифов
    /// </summary>
    [Field(Title = "Список тарифов", Description = "Карточки доступных тарифов")]
    public IList<TariffItem> Tariffs { get; set; }
}
```

#### 2.5 TariffItem

Элемент тарифа.

```csharp
/// <summary>
/// Элемент тарифа
/// </summary>
public class TariffItem
{
    /// <summary>
    /// ID тарифа
    /// </summary>
    [Field(Title = "ID тарифа", Description = "Уникальный идентификатор тарифа")]
    public StringField TariffId { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    [Field(Title = "Название", Description = "Название тарифа")]
    public StringField Name { get; set; }
    
    /// <summary>
    /// Описание
    /// </summary>
    [Field(Title = "Описание", Description = "Краткое описание тарифа")]
    public TextField Description { get; set; }
    
    /// <summary>
    /// Цена
    /// </summary>
    [Field(Title = "Цена", Description = "Стоимость тарифа в рублях")]
    public NumberField Price { get; set; }
    
    /// <summary>
    /// Особенности
    /// </summary>
    [Field(Title = "Особенности", Description = "Список особенностей тарифа (по одной на строку)")]
    public TextField Features { get; set; }
}
```

#### 2.6 SeoRegion

Регион для SEO данных.

```csharp
/// <summary>
/// Регион для SEO данных
/// </summary>
public class SeoRegion
{
    /// <summary>
    /// Meta Title
    /// </summary>
    [Field(Title = "Meta Title", Description = "Заголовок страницы для поисковых систем")]
    public StringField MetaTitle { get; set; }
    
    /// <summary>
    /// Meta Description
    /// </summary>
    [Field(Title = "Meta Description", Description = "Описание страницы для поисковых систем")]
    public TextField MetaDescription { get; set; }
    
    /// <summary>
    /// Meta Keywords
    /// </summary>
    [Field(Title = "Meta Keywords", Description = "Ключевые слова через запятую")]
    public StringField MetaKeywords { get; set; }
    
    /// <summary>
    /// OG Title
    /// </summary>
    [Field(Title = "OG Title", Description = "Заголовок для социальных сетей")]
    public StringField OgTitle { get; set; }
    
    /// <summary>
    /// OG Description
    /// </summary>
    [Field(Title = "OG Description", Description = "Описание для социальных сетей")]
    public TextField OgDescription { get; set; }
    
    /// <summary>
    /// OG Image
    /// </summary>
    [Field(Title = "OG Image", Description = "Изображение для социальных сетей")]
    public ImageField OgImage { get; set; }
}
```

### 3. Конфигурация приложения

#### appsettings.json

```json
{
  "ConnectionStrings": {
    "PiranhaDb": "Host=postgres;Database=learning_platform;Username=postgres;Password=password;SearchPath=piranha"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Piranha": "Information"
    }
  },
  "AllowedHosts": "*",
  "Piranha": {
    "MediaCDN": ""
  }
}
```

## Свойства корректности

*Свойство - это характеристика или поведение, которое должно выполняться во всех допустимых выполнениях системы.*

### Свойство 1: Отображение всех элементов карточки тарифа

*Для любой* карточки тарифа, отображаемой на странице, она должна содержать название, описание, цену и кнопку "Купить".

**Валидирует: Требования 3.2**

### Свойство 2: Редирект при покупке тарифа

*Для любого* пользователя, нажимающего кнопку "Купить" на карточке тарифа, система должна перенаправлять на /app/payment с параметром tariffId.

**Валидирует: Требования 3.3**

### Свойство 3: Отображение обновленного контента после сохранения в CMS

*Для любого* контента, сохраненного в Piranha Manager, система должна отображать обновленный контент на лендинге при следующей загрузке страницы.

**Валидирует: Требования 6.6**

### Свойство 4: Генерация SEO мета-тегов

*Для любой* страницы с заполненными SEO данными, система должна генерировать соответствующие мета-теги в HTML.

**Валидирует: Требования 7.1, 7.2**

### Свойство 5: Адаптивность элементов

*Для любого* размера экрана (мобильный, планшет, десктоп), все элементы страницы должны корректно отображаться без потери функциональности.

**Валидирует: Требования 8.1, 8.2, 8.3, 8.4**

## Обработка ошибок

### Стратегия обработки ошибок

Система использует простую стратегию обработки ошибок:

1. **Ошибки загрузки страницы из CMS**
   - Логирование ошибки
   - Возврат HTTP 404 если страница не найдена
   - Возврат HTTP 500 при других ошибках

2. **Ошибки работы с базой данных**
   - Try-catch для DbContext операций
   - Логирование через ILogger
   - Возврат понятных сообщений пользователю

### Примеры обработки ошибок

```csharp
// Обработка ошибок при загрузке страницы
public async Task<IActionResult> Index()
{
    try
    {
        var page = await _api.Pages.GetBySlugAsync<LandingPage>("/");
        
        if (page == null)
        {
            _logger.LogWarning("Главная страница не найдена в CMS");
            return NotFound();
        }
        
        return View(page);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ошибка при загрузке страницы из CMS");
        return StatusCode(500, "Ошибка загрузки страницы");
    }
}
```

### Логирование

Система использует встроенный ILogger для логирования:

- **Information**: Успешные операции, редиректы
- **Warning**: Неожиданные ситуации (страница не найдена)
- **Error**: Ошибки, требующие внимания
- **Critical**: Критические ошибки (недоступность БД)

```csharp
// Примеры логирования
_logger.LogInformation("Редирект на страницу оплаты для тарифа {TariffId}", tariffId);
_logger.LogWarning("Главная страница не найдена в CMS");
_logger.LogError(ex, "Ошибка при загрузке страницы из CMS");
```

## Стратегия тестирования

### Unit-тестирование

Unit-тесты фокусируются на:
- Корректности работы контроллеров
- Обработке ошибок
- Логировании

**Технологии**: xUnit, Moq

**Примеры unit-тестов**:

```csharp
public class LandingControllerTests
{
    [Fact]
    public async Task Index_WithExistingPage_ReturnsViewResult()
    {
        // Arrange
        var mockApi = new Mock<IApi>();
        var mockPages = new Mock<IPageApi>();
        var testPage = new LandingPage { Title = "Test" };
        
        mockPages.Setup(p => p.GetBySlugAsync<LandingPage>("/", null))
            .ReturnsAsync(testPage);
        mockApi.Setup(a => a.Pages).Returns(mockPages.Object);
        
        var controller = new LandingController(mockApi.Object, Mock.Of<ILogger<LandingController>>());
        
        // Act
        var result = await controller.Index();
        
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(testPage, viewResult.Model);
    }
    
    [Fact]
    public async Task Index_WithNonExistingPage_ReturnsNotFound()
    {
        // Arrange
        var mockApi = new Mock<IApi>();
        var mockPages = new Mock<IPageApi>();
        
        mockPages.Setup(p => p.GetBySlugAsync<LandingPage>("/", null))
            .ReturnsAsync((LandingPage)null);
        mockApi.Setup(a => a.Pages).Returns(mockPages.Object);
        
        var controller = new LandingController(mockApi.Object, Mock.Of<ILogger<LandingController>>());
        
        // Act
        var result = await controller.Index();
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

public class TariffControllerTests
{
    [Fact]
    public void Purchase_WithTariffId_RedirectsToPaymentPage()
    {
        // Arrange
        var controller = new TariffController(Mock.Of<ILogger<TariffController>>());
        var tariffId = "tariff-1";
        
        // Act
        var result = controller.Purchase(tariffId);
        
        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"/app/payment?tariffId={tariffId}", redirectResult.Url);
    }
}
```

### Property-Based тестирование

Property-тесты фокусируются на универсальных свойствах.

**Технологии**: FsCheck

**Примеры property-тестов**:

```csharp
[Property]
public Property TariffCard_AlwaysContains_AllRequiredElements()
{
    // Feature: landing-cms, Property 1: Отображение всех элементов карточки тарифа
    
    var tariffGen = from name in Arb.Generate<NonEmptyString>()
                    from desc in Arb.Generate<string>()
                    from price in Arb.Generate<PositiveInt>()
                    from id in Arb.Generate<NonEmptyString>()
                    select new TariffItem
                    {
                        TariffId = new StringField { Value = id.Get },
                        Name = new StringField { Value = name.Get },
                        Description = new TextField { Value = desc },
                        Price = new NumberField { Value = price.Get }
                    };
    
    return Prop.ForAll(
        tariffGen.ToArbitrary(),
        tariff =>
        {
            // Render tariff card
            var html = RenderTariffCard(tariff);
            
            // Assert all elements present
            return (html.Contains(tariff.Name.Value) &&
                    html.Contains(tariff.Description.Value) &&
                    html.Contains(tariff.Price.Value.ToString()) &&
                    html.Contains("Купить"))
                .ToProperty();
        }
    );
}

[Property]
public Property Purchase_AlwaysRedirects_ToPaymentWithTariffId()
{
    // Feature: landing-cms, Property 2: Редирект при покупке тарифа
    
    return Prop.ForAll<NonEmptyString>(
        tariffId =>
        {
            var controller = new TariffController(Mock.Of<ILogger<TariffController>>());
            
            var result = controller.Purchase(tariffId.Get);
            
            var redirectResult = result as RedirectResult;
            return (redirectResult != null &&
                    redirectResult.Url.Contains("/app/payment") &&
                    redirectResult.Url.Contains($"tariffId={tariffId.Get}"))
                .ToProperty();
        }
    );
}
```

### Интеграционное тестирование

Интеграционные тесты проверяют взаимодействие с PostgreSQL и Piranha CMS.

```csharp
public class PiranhaCmsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public PiranhaCmsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Index_LoadsPageFromDatabase()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<!DOCTYPE html>", content);
    }
}
```

### Покрытие тестами

Целевые показатели покрытия:
- **Unit-тесты**: 80%+ покрытие кода
- **Property-тесты**: Все свойства корректности
- **Интеграционные тесты**: Критичные интеграционные точки

## Развертывание

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LandingCms.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "LandingCms.dll"]
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: landing-cms
spec:
  replicas: 2
  selector:
    matchLabels:
      app: landing-cms
  template:
    metadata:
      labels:
        app: landing-cms
    spec:
      containers:
      - name: landing-cms
        image: landing-cms:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__PiranhaDb
          valueFrom:
            secretKeyRef:
              name: landing-cms-secrets
              key: db-connection
```
