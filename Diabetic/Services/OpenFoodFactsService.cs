using System.Text.Json;
using System.Text.Json.Serialization;
using Diabetic.Shared.Models;

namespace Diabetic.Services;

public class OpenFoodFactsService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenFoodFactsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<FoodProduct?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://world.openfoodfacts.org/api/v0/product/{barcode}.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var openFoodFactsResponse = JsonSerializer.Deserialize<OpenFoodFactsResponse>(json, _jsonOptions);

            if (openFoodFactsResponse?.Product == null || openFoodFactsResponse.Status != 1)
                return null;

            return MapToFoodProduct(openFoodFactsResponse.Product, barcode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching product by barcode {barcode}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<FoodProduct>> SearchProductsAsync(string query, int pageSize = 20)
    {
        try
        {
            var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={Uri.EscapeDataString(query)}&search_simple=1&action=process&json=1&page_size={pageSize}";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<FoodProduct>();

            var json = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<OpenFoodFactsSearchResponse>(json, _jsonOptions);

            if (searchResponse?.Products == null)
                return new List<FoodProduct>();

            var results = new List<FoodProduct>();
            foreach (var product in searchResponse.Products.Where(p => p != null))
            {
                if (product == null) continue;
                
                var foodProduct = MapToFoodProduct(product, product.Code ?? "");
                if (foodProduct != null)
                {
                    results.Add(foodProduct);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching products with query '{query}': {ex.Message}");
            return new List<FoodProduct>();
        }
    }

    private static FoodProduct? MapToFoodProduct(OpenFoodFactsProduct product, string barcode)
    {
        try
        {
            // Skip products without essential data
            if (string.IsNullOrWhiteSpace(product.ProductName))
                return null;

            return new FoodProduct
            {
                Name = product.ProductName.Trim(),
                Brand = product.Brands?.Split(',').FirstOrDefault()?.Trim(),
                Barcode = barcode,
                Description = product.GenericName?.Trim(),
                ImageUrl = GetBestImageUrl(product),
                CaloriesPer100g = ParseNutriment(product.Nutriments?.EnergyKcal100g),
                CarbsPer100g = ParseNutriment(product.Nutriments?.Carbohydrates100g),
                SugarsPer100g = ParseNutriment(product.Nutriments?.Sugars100g),
                FiberPer100g = ParseNutriment(product.Nutriments?.Fiber100g),
                ProteinPer100g = ParseNutriment(product.Nutriments?.Proteins100g),
                FatPer100g = ParseNutriment(product.Nutriments?.Fat100g),
                SodiumPer100g = ParseNutriment(product.Nutriments?.Sodium100g) * 1000, // Convert from g to mg
                GlycemicIndex = null, // OpenFoodFacts doesn't provide GI
                Source = "OpenFoodFacts",
                OpenFoodFactsId = barcode,
                IsVerified = !string.IsNullOrEmpty(product.ProductName),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error mapping product {barcode}: {ex.Message}");
            return null;
        }
    }

    private static double ParseNutriment(object? value)
    {
        if (value == null) return 0;
        
        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue)) return 0;
        
        // Handle different number formats
        if (double.TryParse(stringValue.Replace(',', '.'), 
            System.Globalization.NumberStyles.Float, 
            System.Globalization.CultureInfo.InvariantCulture, 
            out double result))
        {
            return Math.Max(0, result); // Ensure non-negative values
        }
        
        return 0;
    }

    private static string? GetBestImageUrl(OpenFoodFactsProduct product)
    {
        // Try different image URLs in order of preference
        if (!string.IsNullOrWhiteSpace(product.ImageFrontUrl))
            return product.ImageFrontUrl;
        
        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            return product.ImageUrl;
        
        if (!string.IsNullOrWhiteSpace(product.ImageSmallUrl))
            return product.ImageSmallUrl;
        
        return null;
    }
}

// DTOs for OpenFoodFacts API Response
public class OpenFoodFactsResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("product")]
    public OpenFoodFactsProduct? Product { get; set; }
}

public class OpenFoodFactsSearchResponse
{
    [JsonPropertyName("products")]
    public List<OpenFoodFactsProduct?>? Products { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}

public class OpenFoodFactsProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }
    
    [JsonPropertyName("brands")]
    public string? Brands { get; set; }
    
    [JsonPropertyName("generic_name")]
    public string? GenericName { get; set; }
    
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
    
    [JsonPropertyName("image_front_url")]
    public string? ImageFrontUrl { get; set; }
    
    [JsonPropertyName("image_small_url")]
    public string? ImageSmallUrl { get; set; }
    
    [JsonPropertyName("categories")]
    public string? Categories { get; set; }
    
    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }
    
    [JsonPropertyName("serving_size")]
    public string? ServingSize { get; set; }
    
    [JsonPropertyName("nutriments")]
    public OpenFoodFactsNutriments? Nutriments { get; set; }
    
    [JsonPropertyName("nutrition_grades")]
    public string? NutritionGrades { get; set; }
    
    [JsonPropertyName("nova_group")]
    public int? NovaGroup { get; set; }
}

public class OpenFoodFactsNutriments
{
    [JsonPropertyName("energy-kcal_100g")]
    public object? EnergyKcal100g { get; set; }
    
    [JsonPropertyName("energy_100g")]
    public object? Energy100g { get; set; }
    
    [JsonPropertyName("carbohydrates_100g")]
    public object? Carbohydrates100g { get; set; }
    
    [JsonPropertyName("sugars_100g")]
    public object? Sugars100g { get; set; }
    
    [JsonPropertyName("fiber_100g")]
    public object? Fiber100g { get; set; }
    
    [JsonPropertyName("proteins_100g")]
    public object? Proteins100g { get; set; }
    
    [JsonPropertyName("fat_100g")]
    public object? Fat100g { get; set; }
    
    [JsonPropertyName("saturated-fat_100g")]
    public object? SaturatedFat100g { get; set; }
    
    [JsonPropertyName("sodium_100g")]
    public object? Sodium100g { get; set; }
    
    [JsonPropertyName("salt_100g")]
    public object? Salt100g { get; set; }
}