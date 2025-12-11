// File: SaleService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Medicine_ERP_Desktop.Components.Pages;
using System.Globalization;
using System.Data; // Ensure this is present for GetOrdinal/ExecuteReader

namespace PharmacyManagementSystem.Services
{
	// NOTE: SaleRecord, SaleItemRecord, TopStats, SaleReportEntry, and CategoryData
	// DTOs are assumed to be defined and accessible via using statements.

	public class SaleService : ISaleService
	{
		private readonly string _connectionString;

		public SaleService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// --- RecordSaleAsync (Placeholder) ---
		public async Task RecordSaleAsync(List<Sales.CartItem> cartItems, string paymentMethod, decimal totalAmount, decimal discountAmount, decimal amountPaid)
		{
			// Your actual database insertion logic must be fully implemented here.
			await Task.CompletedTask;
		}

		// -----------------------------------------------------------------
		// IMPLEMENTED: Get Recent Sales (Fetches data for the table)
		// -----------------------------------------------------------------
		public async Task<List<SaleRecord>> GetRecentSalesAsync(int limit = 20)
		{
			string sql = @$"
                SELECT TOP (@Limit)
                    s.SaleId,
                    s.SaleDate,
                    s.TotalAmount AS TotalSaleAmount,
                    si.Quantity,
                    si.UnitPrice,
                    si.DiscountPercent AS DiscountPct,
                    (si.Quantity * si.UnitPrice * (1 - si.DiscountPercent / 100.0)) AS LineTotalAmount,
                    m.Name AS MedicineName
                FROM dbo.Sales s
                INNER JOIN dbo.SaleItems si ON s.SaleId = si.SaleId
                INNER JOIN dbo.Medicines m ON si.MedicineId = m.MedicineId
                ORDER BY s.SaleDate DESC";

			var allSaleData = new List<(
				int SaleId,
				DateTime SaleDate,
				decimal TotalSaleAmount,
				int Quantity,
				decimal UnitPrice,
				decimal DiscountPct,
				decimal LineTotalAmount,
				string MedicineName)>();

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@Limit", limit);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					allSaleData.Add((
						SaleId: reader.GetInt32(reader.GetOrdinal("SaleId")),
						SaleDate: reader.GetDateTime(reader.GetOrdinal("SaleDate")),
						TotalSaleAmount: reader.GetDecimal(reader.GetOrdinal("TotalSaleAmount")),
						Quantity: reader.GetInt32(reader.GetOrdinal("Quantity")),
						UnitPrice: reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
						DiscountPct: reader.GetDecimal(reader.GetOrdinal("DiscountPct")),
						LineTotalAmount: reader.GetDecimal(reader.GetOrdinal("LineTotalAmount")),
						MedicineName: reader.GetString(reader.GetOrdinal("MedicineName"))
					));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"DB Error loading recent sales: {ex.Message}");
				return new List<SaleRecord>();
			}

			// Group the flat result set by SaleId in C#
			var recentSales = allSaleData
				.GroupBy(data => data.SaleId)
				.Select(group => new SaleRecord
				{
					SaleId = group.Key,
					SaleDate = group.First().SaleDate,
					TotalSaleAmount = group.First().TotalSaleAmount,
					Items = group.Select(item => new SaleItemRecord
					{
						MedicineName = item.MedicineName,
						UnitPrice = item.UnitPrice,
						Quantity = item.Quantity,
						DiscountPct = item.DiscountPct,
						TotalAmount = item.LineTotalAmount
					}).ToList()
				})
				.OrderByDescending(s => s.SaleDate)
				.ToList();

			return recentSales;
		}

		// -----------------------------------------------------------------
		// Get Aggregate Stats (Placeholder/NotImplemented)
		// -----------------------------------------------------------------
		public async Task<TopStats> GetAggregateStatsAsync(string period = "Monthly")
		{
			// Note: Your aggregation query in this method needs the appropriate cost columns (e.g., s.CostAmount)
			// to calculate profit accurately if it is fully implemented.
			throw new NotImplementedException();
		}

		// -----------------------------------------------------------------
		// Get Detailed Sales Reports (Placeholder/NotImplemented)
		// -----------------------------------------------------------------
		public async Task<List<SaleReportEntry>> GetSalesReportsAsync(string period = "Monthly")
		{
			throw new NotImplementedException();
		}

		// -----------------------------------------------------------------
		// Get Sales by Category (Placeholder/NotImplemented)
		// -----------------------------------------------------------------
		public async Task<List<CategoryData>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate)
		{
			throw new NotImplementedException();
		}

		// -----------------------------------------------------------------
		// Date Range Helper (Placeholder/NotImplemented)
		// -----------------------------------------------------------------
		private (DateTime start, DateTime end) GetDateRange(string period)
		{
			throw new NotImplementedException();
		}
	}
}