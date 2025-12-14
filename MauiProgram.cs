using System.Data;
using ApexCharts;
// using Medicine_ERP.Data;
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

			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["ConnectionStrings:DefaultConnection"] =
						"Data Source=MSI\\SQLEXPRESS;Database=db34512;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30;",

					["AppSettings:ApiBaseUrl"] = "https://localhost:7001",
					["Logging:LogLevel:Default"] = "Information"
				})
				.Build();

			builder.Services.AddSingleton<IConfiguration>(config);

			var connectionString = config.GetConnectionString("DefaultConnection")
								 ?? throw new InvalidOperationException("Missing 'DefaultConnection' in configuration.");

			// builder.Services.AddDbContext<Medicine_ERP_Data.AppDbContext>(options =>
			// options.UseSqlServer(connectionString));

			builder.Services.AddMauiBlazorWebView();
			builder.Services.AddApexCharts();

			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<ISupplierService, SupplierService>();
			builder.Services.AddScoped<IMedicineService, MedicineService>();
			builder.Services.AddScoped<ISaleService, SaleService>();
			builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
			builder.Services.AddScoped<IStaffService, StaffService>();
			builder.Services.AddScoped<IInventoryService, InventoryService>();			

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
