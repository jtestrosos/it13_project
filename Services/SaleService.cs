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
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			using var transaction = connection.BeginTransaction();

			try
			{
				// 1. Insert into Sales Output inserted.SaleId
				string sqlSale = @"
					INSERT INTO Sales (SaleDate, PaymentMethod, SubTotal, DiscountAmount, TotalAmount, AmountPaid)
					OUTPUT INSERTED.SaleId
					VALUES (@SaleDate, @PaymentMethod, @SubTotal, @DiscountAmount, @TotalAmount, @AmountPaid)";

				int saleId;
				decimal subTotal = totalAmount + discountAmount;
				// decimal change = amountPaid - totalAmount; // Change is computed by DB

				using (var command = new SqlCommand(sqlSale, connection, transaction))
				{
					command.Parameters.AddWithValue("@SaleDate", DateTime.Now);
					command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
					command.Parameters.AddWithValue("@SubTotal", subTotal);
					command.Parameters.AddWithValue("@DiscountAmount", discountAmount);
					command.Parameters.AddWithValue("@TotalAmount", totalAmount);
					command.Parameters.AddWithValue("@AmountPaid", amountPaid);
					// command.Parameters.AddWithValue("@ChangeGiven", change); // Computed

					saleId = (int)await command.ExecuteScalarAsync();
				}

				// 2. Insert Sale Items and Update Inventory
				string sqlItem = @"
					INSERT INTO SaleItems (SaleId, MedicineId, Quantity, UnitPrice, DiscountPct)
					VALUES (@SaleId, @MedicineId, @Quantity, @UnitPrice, @DiscountPct)";

				string sqlUpdateStock = @"
					UPDATE Medicines 
					SET Quantity = Quantity - @Quantity 
					WHERE MedicineId = @MedicineId";

				foreach (var item in cartItems)
				{
					// Add Line Item
					using (var cmdItem = new SqlCommand(sqlItem, connection, transaction))
					{
						cmdItem.Parameters.AddWithValue("@SaleId", saleId);
						cmdItem.Parameters.AddWithValue("@MedicineId", item.Medicine.MedicineId);
						cmdItem.Parameters.AddWithValue("@Quantity", item.Quantity);
						cmdItem.Parameters.AddWithValue("@UnitPrice", item.Medicine.Price);
						cmdItem.Parameters.AddWithValue("@DiscountPct", item.DiscountPercent);
						// cmdItem.Parameters.AddWithValue("@LineTotal", item.Total); // Computed
						await cmdItem.ExecuteNonQueryAsync();
					}

					// Update Stock
					using (var cmdStock = new SqlCommand(sqlUpdateStock, connection, transaction))
					{
						cmdStock.Parameters.AddWithValue("@Quantity", item.Quantity);
						cmdStock.Parameters.AddWithValue("@MedicineId", item.Medicine.MedicineId);
						await cmdStock.ExecuteNonQueryAsync();
					}
				}

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
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
				LEFT JOIN dbo.Medicines m ON si.MedicineId = m.MedicineId
				WHERE s.IsDeleted = 0
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
					ISNULL(SUM(si.Quantity * si.UnitPrice * (1 - si.DiscountPct/100.0)), 0) as TotalValue
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

		// -----------------------------------------------------------------
		// Update Sale (Edit)
		// -----------------------------------------------------------------
		public async Task UpdateSaleAsync(
			int saleId,
			List<Medicine_ERP_Desktop.Components.Pages.Sales.CartItem> cartItems,
			string paymentMethod,
			decimal totalAmount,
			decimal discountAmount,
			decimal amountPaid)
		{
			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();

			using var transaction = connection.BeginTransaction();

			try
			{
				// 1. RESTORE STOCK for existing items
				// We need to fetch existing items first to know what to restore
				string sqlGetOldItems = "SELECT MedicineId, Quantity FROM SaleItems WHERE SaleId = @SaleId";
				var oldItems = new List<(int MedicineId, int Quantity)>();

				using (var cmdGet = new SqlCommand(sqlGetOldItems, connection, transaction))
				{
					cmdGet.Parameters.AddWithValue("@SaleId", saleId);
					using (var reader = await cmdGet.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							oldItems.Add((reader.GetInt32(0), reader.GetInt32(1)));
						}
					}
				}

				string sqlRestoreStock = "UPDATE Medicines SET Quantity = Quantity + @Qty WHERE MedicineId = @Mid";
				foreach (var old in oldItems)
				{
					using (var cmdRestore = new SqlCommand(sqlRestoreStock, connection, transaction))
					{
						cmdRestore.Parameters.AddWithValue("@Qty", old.Quantity);
						cmdRestore.Parameters.AddWithValue("@Mid", old.MedicineId);
						await cmdRestore.ExecuteNonQueryAsync();
					}
				}

				// 2. DELETE OLD ITEMS
				string sqlDeleteItems = "DELETE FROM SaleItems WHERE SaleId = @SaleId";
				using (var cmdDel = new SqlCommand(sqlDeleteItems, connection, transaction))
				{
					cmdDel.Parameters.AddWithValue("@SaleId", saleId);
					await cmdDel.ExecuteNonQueryAsync();
				}

				// 3. UPDATE SALE RECORD
				string sqlUpdateSale = @"
					UPDATE Sales 
					SET SaleDate = @SaleDate, 
						PaymentMethod = @PaymentMethod, 
						SubTotal = @SubTotal, 
						DiscountAmount = @DiscountAmount, 
						TotalAmount = @TotalAmount, 
						AmountPaid = @AmountPaid
					WHERE SaleId = @SaleId";
				
				decimal subTotal = totalAmount + discountAmount;
				
				using (var cmdUpdate = new SqlCommand(sqlUpdateSale, connection, transaction))
				{
					cmdUpdate.Parameters.AddWithValue("@SaleDate", DateTime.Now); // Update time to now? Or keep original? Usually update time.
					cmdUpdate.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
					cmdUpdate.Parameters.AddWithValue("@SubTotal", subTotal);
					cmdUpdate.Parameters.AddWithValue("@DiscountAmount", discountAmount);
					cmdUpdate.Parameters.AddWithValue("@TotalAmount", totalAmount);
					cmdUpdate.Parameters.AddWithValue("@AmountPaid", amountPaid);
					cmdUpdate.Parameters.AddWithValue("@SaleId", saleId);
					await cmdUpdate.ExecuteNonQueryAsync();
				}

				// 4. INSERT NEW ITEMS & DEDUCT STOCK
				string sqlItem = @"
					INSERT INTO SaleItems (SaleId, MedicineId, Quantity, UnitPrice, DiscountPct)
					VALUES (@SaleId, @MedicineId, @Quantity, @UnitPrice, @DiscountPct)";

				string sqlDeductStock = @"
					UPDATE Medicines 
					SET Quantity = Quantity - @Quantity 
					WHERE MedicineId = @MedicineId";

				foreach (var item in cartItems)
				{
					// Insert Item
					using (var cmdItem = new SqlCommand(sqlItem, connection, transaction))
					{
						cmdItem.Parameters.AddWithValue("@SaleId", saleId);
						cmdItem.Parameters.AddWithValue("@MedicineId", item.Medicine.MedicineId);
						cmdItem.Parameters.AddWithValue("@Quantity", item.Quantity);
						cmdItem.Parameters.AddWithValue("@UnitPrice", item.Medicine.Price);
						cmdItem.Parameters.AddWithValue("@DiscountPct", item.DiscountPercent);
						await cmdItem.ExecuteNonQueryAsync();
					}

					// Deduct Stock
					using (var cmdStock = new SqlCommand(sqlDeductStock, connection, transaction))
					{
						cmdStock.Parameters.AddWithValue("@Quantity", item.Quantity);
						cmdStock.Parameters.AddWithValue("@MedicineId", item.Medicine.MedicineId);
						await cmdStock.ExecuteNonQueryAsync();
					}
				}

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public async Task<bool> DeleteSaleAsync(int saleId)
		{
			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var transaction = connection.BeginTransaction();

				try
				{
					// 1. RESTORE STOCK BEFORE DELETING
					string sqlGetOldItems = "SELECT MedicineId, Quantity FROM SaleItems WHERE SaleId = @SaleId";
					var oldItems = new List<(int MedicineId, int Quantity)>();

					using (var cmdGet = new SqlCommand(sqlGetOldItems, connection, transaction))
					{
						cmdGet.Parameters.AddWithValue("@SaleId", saleId);
						using (var reader = await cmdGet.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								oldItems.Add((reader.GetInt32(0), reader.GetInt32(1)));
							}
						}
					}

					string sqlRestoreStock = "UPDATE Medicines SET Quantity = Quantity + @Qty WHERE MedicineId = @Mid";
					foreach (var old in oldItems)
					{
						using (var cmdRestore = new SqlCommand(sqlRestoreStock, connection, transaction))
						{
							cmdRestore.Parameters.AddWithValue("@Qty", old.Quantity);
							cmdRestore.Parameters.AddWithValue("@Mid", old.MedicineId);
							await cmdRestore.ExecuteNonQueryAsync();
						}
					}

					// 2. DELETE SALE ITEMS - SKIPPED FOR SOFT DELETE
					// Items remain in DB but filtered out by parent IsDeleted flag

					// 3. SOFT DELETE SALE (Mark as Archived)
					string deleteSaleSql = "UPDATE dbo.Sales SET IsDeleted = 1 WHERE SaleId = @SaleId";
					using (var cmd = new SqlCommand(deleteSaleSql, connection, transaction))
					{
						cmd.Parameters.AddWithValue("@SaleId", saleId);
						int rowsAffected = await cmd.ExecuteNonQueryAsync();

						if (rowsAffected > 0)
						{
							transaction.Commit();
							return true;
						}
						else
						{
							transaction.Rollback();
							return false;
						}
					}
				}
				catch
				{
					transaction.Rollback();
					throw;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting sale: {ex.Message}");
				return false;
			}
		}
	
	
		// -----------------------------------------------------------------
		// ARCHIVE / RESTORE
		// -----------------------------------------------------------------
		public async Task<List<SaleRecord>> GetArchivedSalesAsync()
		{
			string sql = @"
				SELECT 
					s.SaleId, s.SaleDate, s.TotalAmount AS TotalSaleAmount,
					si.Quantity, si.UnitPrice, si.DiscountPct, si.LineTotal AS LineTotalAmount,
					m.Name AS MedicineName
				FROM dbo.Sales s
				JOIN dbo.SaleItems si ON s.SaleId = si.SaleId
				LEFT JOIN dbo.Medicines m ON si.MedicineId = m.MedicineId
				WHERE s.IsDeleted = 1
				ORDER BY s.SaleDate DESC";

			var allData = new List<(int SaleId, DateTime Date, decimal Total, int Qty, decimal Price, decimal Disc, decimal LineTotal, string MedName)>();

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							allData.Add((
								reader.GetInt32(0),
								reader.GetDateTime(1),
								reader.GetDecimal(2),
								reader.GetInt32(3),
								reader.GetDecimal(4),
								reader.GetDecimal(5),
								reader.GetDecimal(6),
								reader.IsDBNull(7) ? "N/A" : reader.GetString(7)
							));
						}
					}
				}
			}

			// Group by SaleId
			var result = allData.GroupBy(x => x.SaleId).Select(g => new SaleRecord
			{
				SaleId = g.Key,
				SaleDate = g.First().Date,
				TotalSaleAmount = g.First().Total,
				Items = g.Select(i => new SaleItemRecord
				{
					MedicineName = i.MedName,
					Quantity = i.Qty,
					UnitPrice = i.Price,
					DiscountPct = i.Disc,
					TotalAmount = i.LineTotal
				}).ToList()
			}).ToList();

			return result;
		}

		public async Task RestoreSaleAsync(int saleId)
		{
            using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try {
                // 1. Get Items to deduct stock
                var items = new List<(int MedId, int Qty)>();
                string getItems = "SELECT MedicineId, Quantity FROM SaleItems WHERE SaleId = @Id";
                using(var cmd = new SqlCommand(getItems, connection, transaction)){
                    cmd.Parameters.AddWithValue("@Id", saleId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while(await reader.ReadAsync()) items.Add((reader.GetInt32(0), reader.GetInt32(1)));
                }

                // 2. Deduct Stock
                string updateStock = "UPDATE Medicines SET Quantity = Quantity - @Qty WHERE MedicineId = @Mid";
                foreach(var item in items){
                    using var cmd = new SqlCommand(updateStock, connection, transaction);
                    cmd.Parameters.AddWithValue("@Qty", item.Qty);
                    cmd.Parameters.AddWithValue("@Mid", item.MedId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Un-Archive Sale
                string sql = "UPDATE Sales SET IsDeleted = 0 WHERE SaleId = @Id";
                using(var cmd = new SqlCommand(sql, connection, transaction)){
                     cmd.Parameters.AddWithValue("@Id", saleId);
                     await cmd.ExecuteNonQueryAsync();
                }
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }
		}
	}
}