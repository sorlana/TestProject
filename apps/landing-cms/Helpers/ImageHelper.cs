using Microsoft.AspNetCore.Mvc;

namespace LandingCms.Helpers;

/// <summary>
/// Вспомогательный класс для работы с изображениями
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Генерирует URL для изображения с указанными размерами
    /// </summary>
    /// <param name="urlHelper">URL Helper</param>
    /// <param name="imageId">ID изображения в Piranha CMS</param>
    /// <param name="width">Ширина изображения</param>
    /// <param name="height">Высота изображения (опционально)</param>
    /// <returns>URL изображения с параметрами размера</returns>
    public static string GetImageUrl(IUrlHelper urlHelper, Guid? imageId, int? width = null, int? height = null)
    {
        if (!imageId.HasValue)
            return string.Empty;
            
        var baseUrl = urlHelper.Content($"~/uploads/{imageId.Value}");
        
        if (width.HasValue || height.HasValue)
        {
            var queryParams = new List<string>();
            if (width.HasValue) queryParams.Add($"width={width.Value}");
            if (height.HasValue) queryParams.Add($"height={height.Value}");
            
            return $"{baseUrl}?{string.Join("&", queryParams)}";
        }
        
        return baseUrl;
    }
    
    /// <summary>
    /// Генерирует атрибут srcset для responsive изображений
    /// </summary>
    /// <param name="urlHelper">URL Helper</param>
    /// <param name="imageId">ID изображения в Piranha CMS</param>
    /// <param name="widths">Массив ширин для генерации srcset</param>
    /// <returns>Строка srcset</returns>
    public static string GenerateSrcSet(IUrlHelper urlHelper, Guid? imageId, int[] widths)
    {
        if (!imageId.HasValue)
            return string.Empty;
            
        var srcSetParts = widths.Select(w => $"{GetImageUrl(urlHelper, imageId, w)} {w}w");
        return string.Join(", ", srcSetParts);
    }
    
    /// <summary>
    /// Генерирует атрибут sizes для responsive изображений
    /// </summary>
    /// <param name="breakpoints">Словарь с брейкпоинтами и размерами</param>
    /// <returns>Строка sizes</returns>
    public static string GenerateSizes(Dictionary<string, string> breakpoints)
    {
        var sizesParts = breakpoints.Select(bp => $"({bp.Key}) {bp.Value}");
        return string.Join(", ", sizesParts);
    }
}
