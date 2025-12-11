using System.Data;
using ApexCharts;
using Medicine_ERP.Data;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmacyManagementSystem.Services;


namespace Medicine_ERP_Desktop
{
	public static class MauiProgram
	{
		[System.Runtime.Versioning.SupportedOSPlatform("windows10.0.17763.0")]
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			// ==============================
			// 1. MAUI APP + FONTS
			// ==============================
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			// ==============================
			// 2. IN-MEMORY CONFIGURATION
			// ==============================
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					// ✅ NEW LOCAL SQL EXPRESS CONNECTION STRING
					["ConnectionStrings:DefaultConnection"] =
						"Server=db34512.databaseasp.net; Database=db34512; User Id=db34512; Password=\t8c%YB5r+nQ?3; Encrypt=False; MultipleActiveResultSets=True;",

					// Optional: API Base URL and Logging
					["AppSettings:ApiBaseUrl"] = "https://localhost:7001",
					["Logging:LogLevel:Default"] = "Information"
				})
				.Build();

			// Register configuration for DI
			builder.Services.AddSingleton<IConfiguration>(config);

			// ==============================
			// 3. GET CONNECTION STRING
			// ==============================
			var connectionString = config.GetConnectionString("DefaultConnection")
								 ?? throw new InvalidOperationException("Missing 'DefaultConnection' in configuration.");

			// ==============================
			// 4. REGISTER DATABASE CONTEXT (If you need Entity Framework)
			// ==============================
			// NOTE: You may need to replace Medicine_ERP_Data.AppDbContext with your actual DbContext name.
			builder.Services.AddDbContext<Medicine_ERP_Data.AppDbContext>(options =>
			options.UseSqlServer(connectionString));

			// ==============================
			// 5. BLAZOR + SERVICES
			// ==============================
			builder.Services.AddMauiBlazorWebView();
			builder.Services.AddApexCharts();

			// FIX: Removed the extraneous '[' that caused the error.
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<ISupplierService, SupplierService>();
			builder.Services.AddScoped<IMedicineService, MedicineService>();
			builder.Services.AddScoped<ISaleService, SaleService>();
			builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
			builder.Services.AddScoped<IStaffService, StaffService>();
			


#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}