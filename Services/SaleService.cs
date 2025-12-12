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
		public int MedicineId { get; set; }
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
				SELECT TOP 50
					s.SaleId,
					s.SaleDate,
					s.TotalAmount AS TotalSaleAmount,
					si.Quantity,
					si.UnitPrice,
					si.DiscountPct,
					si.LineTotal AS LineTotalAmount,
					m.Name AS MedicineName
				FROM dbo.Sales s
				JOIN dbo.SaleItems si ON s.SaleId = si.SaleId
				JOIN dbo.Medicines m ON si.MedicineId = m.MedicineId
				ORDER BY s.SaleDate DESC, s.SaleId DESC";

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

		// -----------------------------------------------------------------
		// Get Aggregate Stats
		// -----------------------------------------------------------------
		public async Task<TopStats> GetAggregateStatsAsync(string period = "Monthly")
		{
			var (startDate, endDate) = GetDateRange(period);
			var stats = new TopStats();

			string sql = @"
				SELECT 
					COUNT(DISTINCT s.SaleId) as TotalTransactions,
					ISNULL(SUM(s.TotalAmount), 0) as TotalRevenue,
					ISNULL(SUM(si.Quantity), 0) as ItemsSold
				FROM Sales s
				LEFT JOIN SaleItems si ON s.SaleId = si.SaleId
				WHERE s.SaleDate >= @StartDate AND s.SaleDate < @EndDate";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);

				using var reader = await command.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					stats.TotalTransactions = reader.GetInt32(reader.GetOrdinal("TotalTransactions"));
					stats.TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
					stats.ItemsSold = reader.GetInt32(reader.GetOrdinal("ItemsSold"));
					// Assuming 20% profit margin as cost is not tracked
					stats.TotalProfit = stats.TotalRevenue * 0.20m; 
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetAggregateStatsAsync: {ex.Message}");
			}

			return stats;
		}

		// -----------------------------------------------------------------
		// Get Sales Reports
		// -----------------------------------------------------------------
		public async Task<List<SaleReportEntry>> GetSalesReportsAsync(string period = "Monthly")
		{
			var (startDate, endDate) = GetDateRange(period);
			var list = new List<SaleReportEntry>();

			string sql = @"
				SELECT 
					s.SaleDate,
					s.CustomerName,
					s.PaymentMethod,
					ISNULL(SUM(si.Quantity), 0) as ItemsCount,
					s.TotalAmount
				FROM Sales s
				LEFT JOIN SaleItems si ON s.SaleId = si.SaleId
				WHERE s.SaleDate >= @StartDate AND s.SaleDate < @EndDate
				GROUP BY s.SaleId, s.SaleDate, s.CustomerName, s.PaymentMethod, s.TotalAmount
				ORDER BY s.SaleDate DESC";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var totalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
					list.Add(new SaleReportEntry
					{
						SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")),
						CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? "Walk-in" : reader.GetString(reader.GetOrdinal("CustomerName")),
						PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? "Cash" : reader.GetString(reader.GetOrdinal("PaymentMethod")),
						Items = reader.GetInt32(reader.GetOrdinal("ItemsCount")),
						TotalAmount = totalAmount,
						Profit = totalAmount * 0.20m // Assumed 20% margin
					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetSalesReportsAsync: {ex.Message}");
			}

			return list;
		}

		// -----------------------------------------------------------------
		// Get Sales By Category
		// -----------------------------------------------------------------
		public async Task<List<CategoryData>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate)
		{
			var list = new List<CategoryData>();

			string sql = @"
				SELECT 
					ISNULL(m.Category, 'Uncategorized') as Category,
					ISNULL(SUM(si.Quantity * si.UnitPrice * (1 - si.DiscountPercent/100.0)), 0) as TotalValue
				FROM SaleItems si
				JOIN Medicines m ON si.MedicineId = m.MedicineId
				JOIN Sales s ON si.SaleId = s.SaleId
				WHERE s.SaleDate >= @StartDate AND s.SaleDate < @EndDate
				GROUP BY m.Category";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var category = reader.IsDBNull(reader.GetOrdinal("Category")) 
						? "Uncategorized" 
						: reader.GetString(reader.GetOrdinal("Category"));
					
					list.Add(new CategoryData
					{
						Category = category,
						Value = reader.GetDecimal(reader.GetOrdinal("TotalValue"))
					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetSalesByCategoryAsync: {ex.Message}");
			}

			return list;
		}

		// -----------------------------------------------------------------
		// Get Sales By Payment Method (Pie Chart)
		// -----------------------------------------------------------------
		public async Task<List<CategoryData>> GetSalesByPaymentMethodAsync(string period = "Monthly")
		{
			var (startDate, endDate) = GetDateRange(period);
			var list = new List<CategoryData>();

			string sql = @"
				SELECT 
					ISNULL(PaymentMethod, 'Cash') as PaymentMethod,
					COUNT(*) as TransactionCount,
					ISNULL(SUM(TotalAmount), 0) as TotalValue
				FROM Sales
				WHERE SaleDate >= @StartDate AND SaleDate < @EndDate
				GROUP BY PaymentMethod";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					list.Add(new CategoryData
					{
						Category = reader.GetString(reader.GetOrdinal("PaymentMethod")),
						Value = reader.GetDecimal(reader.GetOrdinal("TotalValue")) // Or use TransactionCount if preferred
					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetSalesByPaymentMethodAsync: {ex.Message}");
			}
			return list;
		}

		// -----------------------------------------------------------------
		// Get Top Selling Medicines (Bar Chart)
		// -----------------------------------------------------------------
		public async Task<List<CategoryData>> GetTopSellingMedicinesAsync(string period = "Monthly")
		{
			var (startDate, endDate) = GetDateRange(period);
			var list = new List<CategoryData>();

			string sql = @"
				SELECT TOP 5
					m.Name,
					ISNULL(SUM(si.Quantity), 0) as TotalSold
				FROM SaleItems si
				JOIN Sales s ON si.SaleId = s.SaleId
				JOIN Medicines m ON si.MedicineId = m.MedicineId
				WHERE s.SaleDate >= @StartDate AND s.SaleDate < @EndDate
				GROUP BY m.Name
				ORDER BY TotalSold DESC";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);
				command.Parameters.AddWithValue("@StartDate", startDate);
				command.Parameters.AddWithValue("@EndDate", endDate);

				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					list.Add(new CategoryData
					{
						Category = reader.GetString(reader.GetOrdinal("Name")),
						Value = reader.GetInt32(reader.GetOrdinal("TotalSold"))
					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetTopSellingMedicinesAsync: {ex.Message}");
			}
			return list;
		}

		// -----------------------------------------------------------------
		// Helper: Get Date Range
		// -----------------------------------------------------------------
		private (DateTime startDate, DateTime endDate) GetDateRange(string period)
		{
			var now = DateTime.Today;
			return period switch
			{
				"Daily" => (now, now.AddDays(1)),
				"Weekly" => (now.AddDays(-(int)now.DayOfWeek), now.AddDays(7 - (int)now.DayOfWeek)),
				"Monthly" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
				"Yearly" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1)),
				_ => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1))
			};
		}
	}
}