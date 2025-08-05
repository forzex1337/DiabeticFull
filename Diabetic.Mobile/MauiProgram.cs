using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace Diabetic.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Add HTTP client for API calls
		builder.Services.AddHttpClient();
		builder.Services.AddHttpClient("DiabeticAPI", client =>
		{
			client.BaseAddress = new Uri("https://localhost:7001/"); // Adjust to your API URL
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
