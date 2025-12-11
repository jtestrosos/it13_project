// File: SaleService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Medicine_ERP_Desktop.Components.Pages;

namespace PharmacyManagementSystem.Services
{
	// DTOs definition 
	public class SaleItemRecord
	{
		public string MedicineName { get; set; } = string.Empty;
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal DiscountPct { get; set; }
		public decimal TotalAmount { get; set; }
	}

	public class SaleRecord
	{
		public int SaleId { get; set; }
		public DateTime SaleDate { get; set; }
		public decimal TotalSaleAmount { get; set; }
		public List<SaleItemRecord> Items { get; set; } = new();
	}

	public class SaleService : ISaleService
	{
		private readonly string _connectionString;

		public SaleService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// -----------------------------------------------------------------
		// Record Sale - FIX: Using fully qualified type for CartItem
		// -----------------------------------------------------------------
		public async Task RecordSaleAsync(
			List<Medicine_ERP_Desktop.Components.Pages.Sales.CartItem> cartItems,
			string paymentMethod,
			decimal totalAmount,
			decimal discountAmount,
			decimal amountPaid)
		{
			await Task.CompletedTask;
		}

		// -----------------------------------------------------------------
		// Get Recent Sales – TEMPORARILY REMOVED TOP AND ORDER BY
		// -----------------------------------------------------------------
		public async Task<List<SaleRecord>> GetRecentSalesAsync(int limit = 20)
		{
			// TEMPORARY QUERY: Removed TOP and ORDER BY clauses for troubleshooting.
			// This query will attempt to pull all linked sales data.
			string sql = @"
				SELECT
					s.SaleId,
					s.SaleDate,
					s.TotalAmount AS TotalSaleAmount,
					si.Quantity,
					si.UnitPrice,
					si.DiscountPercent AS DiscountPct,
					(si.Quantity * si.UnitPrice) * (1 - si.DiscountPercent / 100.0) AS LineTotalAmount,
					m.GenericName AS MedicineName
				FROM dbo.Sales s
				JOIN dbo.SaleItems si ON s.SaleId = si.SaleId
				JOIN dbo.Medicines m ON si.MedicineId = m.MedicineId";

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

				// Removed: command.Parameters.AddWithValue("@Limit", limit);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					// Using explicit NULL and type checks for maximum safety
					var saleId = reader.GetInt32("SaleId");

					var saleDate = reader.IsDBNull("SaleDate") ? DateTime.MinValue : reader.GetDateTime("SaleDate");
					var totalSaleAmount = reader.IsDBNull("TotalSaleAmount") ? 0M : reader.GetFieldValue<decimal>("TotalSaleAmount");
					var quantity = reader.IsDBNull("Quantity") ? 0 : reader.GetInt32("Quantity");
					var unitPrice = reader.IsDBNull("UnitPrice") ? 0M : reader.GetFieldValue<decimal>("UnitPrice");
					var discountPct = reader.IsDBNull("DiscountPct") ? 0M : reader.GetFieldValue<decimal>("DiscountPct");
					var lineTotalAmount = reader.IsDBNull("LineTotalAmount") ? 0M : reader.GetFieldValue<decimal>("LineTotalAmount");
					var medicineName = reader.IsDBNull("MedicineName") ? "N/A" : reader.GetString("MedicineName");

					allSaleData.Add((
						saleId,
						saleDate,
						totalSaleAmount,
						quantity,
						unitPrice,
						discountPct,
						lineTotalAmount,
						medicineName
					));
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine($"--- SQL ERROR in GetRecentSalesAsync ---");
				Console.WriteLine($"DB Error ({ex.Number}): {ex.Message}");
				return new List<SaleRecord>();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"--- GENERAL C# READER ERROR in GetRecentSalesAsync ---");
				Console.WriteLine($"Error Message: {ex.Message}");
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
				.ToList();

			return recentSales;
		}

		public async Task<TopStats> GetAggregateStatsAsync(string period = "Monthly") { throw new NotImplementedException(); }
		public async Task<List<SaleReportEntry>> GetSalesReportsAsync(string period = "Monthly") { throw new NotImplementedException(); }
		public async Task<List<CategoryData>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate) { throw new NotImplementedException(); }

		private (DateTime startDate, DateTime endDate) GetDateRange(string period) { throw new NotImplementedException(); }
	}
}