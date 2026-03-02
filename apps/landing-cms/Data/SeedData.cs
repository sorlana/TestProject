using Piranha;
using Piranha.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using LandingCms.Models;
using Piranha.Extend.Fields;

namespace LandingCms.Data;

/// <summary>
/// Класс для инициализации начальных данных
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Инициализация начального администратора
    /// </summary>
    public static async Task InitializeAdminAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            
            // Проверяем, существует ли администратор
            var adminEmail = "admin@landing.local";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            
            if (existingAdmin == null)
            {
                logger.LogInformation("Создание начального администратора");
                
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(admin, "Admin123!");
                
                if (result.Succeeded)
                {
                    logger.LogInformation("Администратор успешно создан: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Ошибка создания администратора: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Администратор уже существует");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации администратора");
        }
    }
    
    /// <summary>
    /// Инициализация начального контента
    /// </summary>
    public static async Task InitializeContentAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var api = services.GetRequiredService<IApi>();
            
            // Проверяем, существует ли главная страница
            var existingPage = await api.Pages.GetBySlugAsync<LandingPage>("/");
            
            if (existingPage == null)
            {
                logger.LogInformation("Создание главной страницы с тестовым контентом");
                
                var landingPage = await LandingPage.CreateAsync(api);
                landingPage.SiteId = (await api.Sites.GetDefaultAsync()).Id;
                landingPage.Title = "Платформа онлайн-обучения";
                landingPage.Slug = "/"; // Главная страница должна иметь slug "/"
                landingPage.Published = DateTime.UtcNow; // Используем UTC время для PostgreSQL
                
                // Hero секция
                landingPage.Hero.Title.Value = "Добро пожаловать на платформу онлайн-обучения";
                landingPage.Hero.Description.Value = "Изучайте новые навыки в удобном темпе с нашими интерактивными курсами";
                landingPage.Hero.ButtonText.Value = "Начать обучение";
                
                // Секция "О программе"
                landingPage.About.Title.Value = "О нашей платформе";
                landingPage.About.Description.Value = "<p>Наша платформа предоставляет доступ к качественным образовательным материалам.</p>" +
                    "<ul>" +
                    "<li>Интерактивные курсы от экспертов</li>" +
                    "<li>Гибкий график обучения</li>" +
                    "<li>Сертификаты по окончании курсов</li>" +
                    "<li>Поддержка преподавателей</li>" +
                    "</ul>";
                
                // Секция "Тарифы"
                landingPage.Tariffs.Title.Value = "Выберите свой тариф";
                landingPage.Tariffs.GeneralTariffId.Value = "general";
                landingPage.Tariffs.GeneralTariffName.Value = "Общий";
                landingPage.Tariffs.GeneralTariffDescription.Value = "Базовый доступ к платформе";
                landingPage.Tariffs.GeneralTariffPrice.Value = 1;
                landingPage.Tariffs.GeneralTariffFeatures.Value = "Доступ к базовым курсам\nОбщий чат поддержки\nСертификаты об окончании";
                
                // SEO данные
                landingPage.Seo.MetaTitle.Value = "Платформа онлайн-обучения - Учитесь в удобном темпе";
                landingPage.Seo.MetaDescription.Value = "Присоединяйтесь к нашей платформе онлайн-обучения. Интерактивные курсы, гибкий график, сертификаты.";
                landingPage.Seo.MetaKeywords.Value = "онлайн обучение, курсы, образование, сертификаты";
                landingPage.Seo.OgTitle.Value = "Платформа онлайн-обучения";
                landingPage.Seo.OgDescription.Value = "Изучайте новые навыки с нашими интерактивными курсами";
                
                await api.Pages.SaveAsync(landingPage);
                
                logger.LogInformation("Главная страница успешно создана");
            }
            else
            {
                logger.LogInformation("Главная страница уже существует");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации контента");
        }
    }
}
