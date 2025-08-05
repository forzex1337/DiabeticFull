using System;
using System.Threading.Tasks;
using System.Net.Http;
using Diabetic.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        var service = new OpenFoodFactsService(httpClient);
        
        Console.WriteLine("Testing OpenFoodFacts API integration...");
        
        // Test search
        Console.WriteLine("\n--- Testing Search ---");
        var searchResults = await service.SearchProductsAsync("masło", 5);
        Console.WriteLine($"Found {searchResults.Count} products for 'masło':");
        
        foreach (var product in searchResults)
        {
            Console.WriteLine($"- {product.Name} ({product.Brand}) - {product.CaloriesPer100g} kcal/100g");
        }
        
        // Test barcode lookup
        Console.WriteLine("\n--- Testing Barcode Lookup ---");
        var barcodeProduct = await service.GetProductByBarcodeAsync("3017620422003");
        if (barcodeProduct != null)
        {
            Console.WriteLine($"Found product: {barcodeProduct.Name} - {barcodeProduct.CaloriesPer100g} kcal/100g");
        }
        else
        {
            Console.WriteLine("Product not found by barcode");
        }
        
        Console.WriteLine("\nTest completed!");
    }
}