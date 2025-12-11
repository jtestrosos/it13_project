using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Medicine_ERP_Desktop.Components.Pages.Reports;

namespace PharmacyManagementSystem.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly string _connectionString;

		public InventoryService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// -----------------------------------------------------------------
		// 1. Get All Inventory Items
		// -----------------------------------------------------------------
		public async Task<List<InventoryItem>> GetAllInventoryAsync()
		{
			var inventory = new List<InventoryItem>();
			string sql = @"
				SELECT
					MedicineId,
					Name,
					Category,
					Manufacturer,
					Quantity,
					MinQuantity,
					Price,
					ExpiryDate
				FROM dbo.Medicines
				ORDER BY Name";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				inventory.Add(new InventoryItem
				{
					MedicineId = reader.GetInt32("MedicineId"),
					Name = reader.GetString("Name"),
					Category = reader.GetString("Category"),
					Manufacturer = reader.GetString("Manufacturer"),
					Quantity = reader.GetInt32("Quantity"),
					MinQuantity = reader.GetInt32("MinQuantity"),
					Price = reader.GetDecimal("Price"),
					ExpiryDate = reader.GetDateTime("ExpiryDate")
				});
			}
			return inventory;
		}

		// -----------------------------------------------------------------
		// 2. Get Low Stock Items (For Charts/Alerts)
		// -----------------------------------------------------------------
		public async Task<List<LowStockItem>> GetLowStockItemsAsync()
		{
			var lowStock = new List<LowStockItem>();
			string sql = @"
				SELECT
					Name,
					Quantity,
					MinQuantity
				FROM dbo.Medicines
				WHERE Quantity <= MinQuantity
				ORDER BY Quantity ASC, Name ASC";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				int quantity = reader.GetInt32("Quantity");
				int minQuantity = reader.GetInt32("MinQuantity");

				lowStock.Add(new LowStockItem
				{
					Name = reader.GetString("Name"),
					Quantity = quantity,
					StockDifference = minQuantity - quantity
				});
			}
			return lowStock;
		}

		// -----------------------------------------------------------------
		// 3. Get Inventory Item By Id
		// -----------------------------------------------------------------
		public async Task<InventoryItem?> GetInventoryItemByIdAsync(int medicineId)
		{
			string sql = @"
				SELECT
					MedicineId, Name, Category, Manufacturer, Quantity, MinQuantity, Price, ExpiryDate
				FROM dbo.Medicines
				WHERE MedicineId = @MedicineId";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);
			command.Parameters.AddWithValue("@MedicineId", medicineId);

			using var reader = await command.ExecuteReaderAsync();
			if (await reader.ReadAsync())
			{
				return new InventoryItem
				{
					MedicineId = reader.GetInt32("MedicineId"),
					Name = reader.GetString("Name"),
					Category = reader.GetString("Category"),
					Manufacturer = reader.GetString("Manufacturer"),
					Quantity = reader.GetInt32("Quantity"),
					MinQuantity = reader.GetInt32("MinQuantity"),
					Price = reader.GetDecimal("Price"),
					ExpiryDate = reader.GetDateTime("ExpiryDate")
				};
			}
			return null;
		}

		// -----------------------------------------------------------------
		// 4. Update Stock Quantity (Crucial for Sales/Restock)
		// -----------------------------------------------------------------
		public async Task<bool> UpdateStockQuantityAsync(int medicineId, int newQuantity)
		{
			string sql = @"
				UPDATE dbo.Medicines
				SET Quantity = @NewQuantity
				WHERE MedicineId = @MedicineId";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);
			command.Parameters.AddWithValue("@NewQuantity", newQuantity);
			command.Parameters.AddWithValue("@MedicineId", medicineId);

			return await command.ExecuteNonQueryAsync() > 0;
		}

		// -----------------------------------------------------------------
		// 5. Add New Inventory Item (CRUD)
		// -----------------------------------------------------------------
		public async Task<bool> AddInventoryItemAsync(InventoryItem item)
		{
			string sql = @"
				INSERT INTO dbo.Medicines
				(Name, Category, Manufacturer, Quantity, MinQuantity, Price, ExpiryDate)
				VALUES
				(@Name, @Category, @Manufacturer, @Quantity, @MinQuantity, @Price, @ExpiryDate)";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);

			command.Parameters.AddWithValue("@Name", item.Name);
			command.Parameters.AddWithValue("@Category", item.Category);
			command.Parameters.AddWithValue("@Manufacturer", item.Manufacturer);
			command.Parameters.AddWithValue("@Quantity", item.Quantity);
			command.Parameters.AddWithValue("@MinQuantity", item.MinQuantity);
			command.Parameters.AddWithValue("@Price", item.Price);
			command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate);

			return await command.ExecuteNonQueryAsync() > 0;
		}

		// -----------------------------------------------------------------
		// 6. Update Existing Inventory Item (CRUD)
		// -----------------------------------------------------------------
		public async Task<bool> UpdateInventoryItemAsync(InventoryItem item)
		{
			string sql = @"
				UPDATE dbo.Medicines
				SET
					Name = @Name,
					Category = @Category,
					Manufacturer = @Manufacturer,
					Quantity = @Quantity,
					MinQuantity = @MinQuantity,
					Price = @Price,
					ExpiryDate = @ExpiryDate
				WHERE MedicineId = @MedicineId";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);

			command.Parameters.AddWithValue("@MedicineId", item.MedicineId);
			command.Parameters.AddWithValue("@Name", item.Name);
			command.Parameters.AddWithValue("@Category", item.Category);
			command.Parameters.AddWithValue("@Manufacturer", item.Manufacturer);
			command.Parameters.AddWithValue("@Quantity", item.Quantity);
			command.Parameters.AddWithValue("@MinQuantity", item.MinQuantity);
			command.Parameters.AddWithValue("@Price", item.Price);
			command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate);

			return await command.ExecuteNonQueryAsync() > 0;
		}

		// -----------------------------------------------------------------
		// 7. Delete Inventory Item (CRUD)
		// -----------------------------------------------------------------
		public async Task<bool> DeleteInventoryItemAsync(int medicineId)
		{
			string sql = "DELETE FROM dbo.Medicines WHERE MedicineId = @MedicineId";

			using var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			using var command = new SqlCommand(sql, connection);
			command.Parameters.AddWithValue("@MedicineId", medicineId);

			return await command.ExecuteNonQueryAsync() > 0;
		}

		// -----------------------------------------------------------------
		// 8. Get Inventory Stats
		// -----------------------------------------------------------------
		public async Task<InventoryStats> GetInventoryStatsAsync()
		{
			var stats = new InventoryStats();
			string sql = @"
				SELECT
					COUNT(MedicineId) as TotalItems,
					ISNULL(SUM(Quantity * Price), 0) as TotalValue,
					(SELECT COUNT(*) FROM dbo.Medicines WHERE Quantity <= MinQuantity) as LowStockCount,
					(SELECT COUNT(*) FROM dbo.Medicines WHERE ExpiryDate <= GETDATE()) as ExpiredCount
				FROM dbo.Medicines";

			try
			{
				using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();
				using var command = new SqlCommand(sql, connection);

				using var reader = await command.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					stats.TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems"));
					stats.TotalValue = reader.GetDecimal(reader.GetOrdinal("TotalValue"));
					stats.LowStockCount = reader.GetInt32(reader.GetOrdinal("LowStockCount"));
					stats.ExpiredCount = reader.GetInt32(reader.GetOrdinal("ExpiredCount"));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetInventoryStatsAsync: {ex.Message}");
			}
			return stats;
		}
	}
}