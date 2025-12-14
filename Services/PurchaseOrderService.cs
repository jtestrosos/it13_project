using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace PharmacyManagementSystem.Services
{
	// The PurchaseOrder DTO/Model definitions are correctly placed in IPurchaseOrderService.cs 
	// and duplicated here, but should ideally be in a shared Models/DTOs folder.

	public class PurchaseOrderService : IPurchaseOrderService
	{
		private readonly string _connectionString;

		public PurchaseOrderService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
								?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// ------------------------------------------
		// 1. FETCH ALL ORDERS (Summary List)
		// ------------------------------------------
		public async Task<List<PurchaseOrders>> GetAllOrdersAsync()
		{
			var orders = new List<PurchaseOrders>();

			// CRITICAL: JOIN with Suppliers table to get the Supplier Name
			string sql = @"
                SELECT 
                    po.PurchaseOrderId, po.SupplierId, po.OrderDate, 
                    po.ExpectedDate, po.TotalAmount, po.Status,
                    s.Name AS SupplierName 
                FROM [PurchaseOrders] po
                JOIN [Suppliers] s ON po.SupplierId = s.SupplierId
                ORDER BY po.OrderDate DESC";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							orders.Add(new PurchaseOrders
							{
								PurchaseOrderId = reader.GetInt32(reader.GetOrdinal("PurchaseOrderId")),
								SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
								OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
								ExpectedDate = reader.IsDBNull(reader.GetOrdinal("ExpectedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpectedDate")),
								TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
								Status = reader.GetString(reader.GetOrdinal("Status")),
								SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")) // Read the joined Name
							});
						}
					}
				}
			}
			return orders;
		}


		// ------------------------------------------
		// 2. FETCH ORDER DETAILS (Header + Items)
		// ------------------------------------------
		public async Task<PurchaseOrders> GetOrderDetailsAsync(int purchaseOrderId)
		{
			PurchaseOrders order = null;

			// 1. Get Header Data
			string headerSql = @"
                SELECT 
                    po.PurchaseOrderId, po.SupplierId, po.OrderDate, 
                    po.ExpectedDate, po.TotalAmount, po.Status,
                    s.Name AS SupplierName 
                FROM [PurchaseOrders] po
                JOIN [Suppliers] s ON po.SupplierId = s.SupplierId
                WHERE po.PurchaseOrderId = @PurchaseOrderId";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				using (var command = new SqlCommand(headerSql, connection))
				{
					command.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							order = new PurchaseOrders
							{
								PurchaseOrderId = reader.GetInt32(reader.GetOrdinal("PurchaseOrderId")),
								SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
								OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
								ExpectedDate = reader.IsDBNull(reader.GetOrdinal("ExpectedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpectedDate")),
								TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
								Status = reader.GetString(reader.GetOrdinal("Status")),
								SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
								Items = new List<PurchaseOrderItem>()
							};
						}
					}
				}

				if (order == null) return null;

				// 2. Get Item Data
				string itemsSql = @"
                    SELECT 
                        poi.PurchaseOrderItemId, poi.MedicineId, poi.QuantityOrdered, 
                        poi.UnitPrice, poi.LineTotal, m.Name AS MedicineName
                    FROM [PurchaseOrderItems] poi
                    JOIN [Medicines] m ON poi.MedicineId = m.MedicineId
                    WHERE poi.PurchaseOrderId = @PurchaseOrderId";

				using (var itemCommand = new SqlCommand(itemsSql, connection))
				{
					itemCommand.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);

					using (var itemReader = await itemCommand.ExecuteReaderAsync())
					{
						while (await itemReader.ReadAsync())
						{
							order.Items.Add(new PurchaseOrderItem
							{
								PurchaseOrderItemId = itemReader.GetInt32(itemReader.GetOrdinal("PurchaseOrderItemId")),
								MedicineId = itemReader.GetInt32(itemReader.GetOrdinal("MedicineId")),
								QuantityOrdered = itemReader.GetInt32(itemReader.GetOrdinal("QuantityOrdered")),
								UnitPrice = itemReader.GetDecimal(itemReader.GetOrdinal("UnitPrice")),
								LineTotal = itemReader.GetDecimal(itemReader.GetOrdinal("LineTotal")),
								MedicineName = itemReader.GetString(itemReader.GetOrdinal("MedicineName"))
							});
						}
					}
				}
			}
			return order;
		}

		// -----------------------------------------------------------------
		// 3. Get Purchase Stats
		// -----------------------------------------------------------------
		public async Task<PurchaseStats> GetPurchaseStatsAsync(string period = "Monthly")
		{
			var stats = new PurchaseStats();
			var (startDate, endDate) = GetDateRange(period);

			string sql = @"
				SELECT 
					COUNT(*) as TotalPurchases,
					ISNULL(SUM(TotalAmount), 0) as TotalSpent,
					SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingOrders,
					SUM(CASE WHEN Status = 'Completed' OR Status = 'Received' THEN 1 ELSE 0 END) as CompletedOrders
				FROM PurchaseOrders
				WHERE OrderDate >= @StartDate AND OrderDate < @EndDate";

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
					stats.TotalPurchases = reader.GetInt32(reader.GetOrdinal("TotalPurchases"));
					stats.TotalSpent = reader.GetDecimal(reader.GetOrdinal("TotalSpent"));
					stats.PendingOrders = reader.GetInt32(reader.GetOrdinal("PendingOrders"));
					stats.CompletedOrders = reader.GetInt32(reader.GetOrdinal("CompletedOrders"));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetPurchaseStatsAsync: {ex.Message}");
			}

			return stats;
		}

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

		public async Task CreateOrderAsync(PurchaseOrders order)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// 1. Calculate Totals based on Items
						decimal totalOrderAmount = 0;
						foreach (var item in order.Items)
						{
							// Get Price from Medicines table
							string priceSql = "SELECT Price FROM Medicines WHERE MedicineId = @Mid";
							using (var pCmd = new SqlCommand(priceSql, connection, transaction))
							{
								pCmd.Parameters.AddWithValue("@Mid", item.MedicineId);
								var result = await pCmd.ExecuteScalarAsync();
								if (result != null && result != DBNull.Value)
								{
									item.UnitPrice = Convert.ToDecimal(result);
								}
								else 
								{
									item.UnitPrice = 0; 
								}
							}
							item.LineTotal = item.QuantityOrdered * item.UnitPrice;
							totalOrderAmount += item.LineTotal;
						}
						
						order.TotalAmount = totalOrderAmount;
						order.Status = "Pending";
						order.OrderDate = DateTime.Now;

						// 2. Insert Order Header
						string insertOrderSql = @"
							INSERT INTO PurchaseOrders (SupplierId, OrderDate, ExpectedDate, TotalAmount, Status)
							VALUES (@SupplierId, @OrderDate, @ExpectedDate, @TotalAmount, @Status);
							SELECT SCOPE_IDENTITY();";

						int newOrderId;
						using (var cmd = new SqlCommand(insertOrderSql, connection, transaction))
						{
							cmd.Parameters.AddWithValue("@SupplierId", order.SupplierId);
							cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
							cmd.Parameters.AddWithValue("@ExpectedDate", (object)order.ExpectedDate ?? DBNull.Value);
							cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
							cmd.Parameters.AddWithValue("@Status", order.Status);
							
							newOrderId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
						}

						// 3. Insert Items
						string insertItemSql = @"
							INSERT INTO PurchaseOrderItems (PurchaseOrderId, MedicineId, QuantityOrdered, UnitPrice)
							VALUES (@PurchaseOrderId, @MedicineId, @QuantityOrdered, @UnitPrice)";

						foreach (var item in order.Items)
						{
							using (var cmd = new SqlCommand(insertItemSql, connection, transaction))
							{
								cmd.Parameters.AddWithValue("@PurchaseOrderId", newOrderId);
								cmd.Parameters.AddWithValue("@MedicineId", item.MedicineId);
								cmd.Parameters.AddWithValue("@QuantityOrdered", item.QuantityOrdered);
								cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
								await cmd.ExecuteNonQueryAsync();
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
			}
		}

		public async Task UpdateOrderStatusAsync(int purchaseOrderId, string newStatus)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						// Update Status
						string updateSql = "UPDATE PurchaseOrders SET Status = @Status WHERE PurchaseOrderId = @Id";
						using (var cmd = new SqlCommand(updateSql, connection, transaction))
						{
							cmd.Parameters.AddWithValue("@Status", newStatus);
							cmd.Parameters.AddWithValue("@Id", purchaseOrderId);
							await cmd.ExecuteNonQueryAsync();
						}

						// If Received/Completed, update Stock
						if (newStatus == "Received" || newStatus == "Completed")
						{
							// Get Items
							string getItemsSql = "SELECT MedicineId, QuantityOrdered FROM PurchaseOrderItems WHERE PurchaseOrderId = @Id";
							var items = new List<(int MedId, int Qty)>();
							using (var cmd = new SqlCommand(getItemsSql, connection, transaction))
							{
								cmd.Parameters.AddWithValue("@Id", purchaseOrderId);
								using (var reader = await cmd.ExecuteReaderAsync())
								{
									while (await reader.ReadAsync())
									{
										items.Add((reader.GetInt32(0), reader.GetInt32(1)));
									}
								}
							}

							// Update Stock
							string updateStockSql = "UPDATE Medicines SET Quantity = Quantity + @Qty WHERE MedicineId = @Mid";
							foreach (var item in items)
							{
								using (var cmd = new SqlCommand(updateStockSql, connection, transaction))
								{
									cmd.Parameters.AddWithValue("@Qty", item.Qty);
									cmd.Parameters.AddWithValue("@Mid", item.MedId);
									await cmd.ExecuteNonQueryAsync();
								}
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
			}
		}

        public async Task<List<PurchaseItemReportDto>> GetPurchaseItemsReportAsync(DateTime startDate, DateTime endDate)
        {
            var list = new List<PurchaseItemReportDto>();
            string sql = @"
                SELECT 
                    s.Name AS SupplierName,
                    m.Name AS MedicineName,
                    SUM(poi.LineTotal) as TotalCost,
                    SUM(poi.QuantityOrdered) as TotalQty
                FROM PurchaseOrders po
                JOIN PurchaseOrderItems poi ON po.PurchaseOrderId = poi.PurchaseOrderId
                JOIN Suppliers s ON po.SupplierId = s.SupplierId
                JOIN Medicines m ON poi.MedicineId = m.MedicineId
                WHERE po.OrderDate >= @StartDate AND po.OrderDate < @EndDate
                GROUP BY s.Name, m.Name";

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
                    list.Add(new PurchaseItemReportDto
                    {
                        SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                        MedicineName = reader.GetString(reader.GetOrdinal("MedicineName")),
                        TotalCost = reader.GetDecimal(reader.GetOrdinal("TotalCost")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("TotalQty"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching purchase item report: {ex.Message}");
            }
            return list;
        }
	}
}