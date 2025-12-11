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
                    FROM [PurchaseOrderItem] poi
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
	}
}