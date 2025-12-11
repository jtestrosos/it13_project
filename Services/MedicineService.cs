using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PharmacyManagementSystem.Services
{
	public class MedicineService : IMedicineService
	{
		private readonly string _connectionString;

		public MedicineService(IConfiguration configuration)
		{
			// Retrieves the local connection string configured in MauiProgram.cs
			_connectionString = configuration.GetConnectionString("DefaultConnection")
								?? throw new InvalidOperationException("DefaultConnection string not found.");
		}

		// ------------------------------------------
		// 1. FETCH ALL MEDICINES (READ)
		// Resolves: Error loading data: SupplierId
		// ------------------------------------------
		public async Task<List<Medicines>> GetAllMedicinesAsync()
		{
			var medicines = new List<Medicines>();

			// CRITICAL FIX: Explicitly select all required columns. 
			// We select the SupplierId from the Suppliers table (s.SupplierId) via the JOIN.
			string sql = @"
                SELECT 
                    m.MedicineId, m.Name, m.GenericName, m.Category, m.Quantity, 
                    m.MinQuantity, m.Price, m.ExpiryDate, m.Batch, m.StorageLocation, 
                    m.Manufacturer,  
                    s.SupplierId 
                FROM [Medicines] m 
                LEFT JOIN [Suppliers] s ON m.Manufacturer = s.Name";
			// Assumes m.Manufacturer column stores the Supplier Name, matching your data model and UI logic.

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							// Safely get ordinal for SupplierId (it may be null if the join fails)
							int supplierIdOrdinal = reader.GetOrdinal("SupplierId");

							medicines.Add(new Medicines
							{
								MedicineId = reader.GetInt32(reader.GetOrdinal("MedicineId")),
								Name = reader.GetString(reader.GetOrdinal("Name")),
								GenericName = reader.IsDBNull(reader.GetOrdinal("GenericName")) ? "" : reader.GetString(reader.GetOrdinal("GenericName")),
								Category = reader.GetString(reader.GetOrdinal("Category")),
								Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
								MinQuantity = reader.GetInt32(reader.GetOrdinal("MinQuantity")),
								Price = reader.GetDecimal(reader.GetOrdinal("Price")),
								ExpiryDate = reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
								Batch = reader.IsDBNull(reader.GetOrdinal("Batch")) ? "" : reader.GetString(reader.GetOrdinal("Batch")),
								StorageLocation = reader.IsDBNull(reader.GetOrdinal("StorageLocation")) ? "" : reader.GetString(reader.GetOrdinal("StorageLocation")),
								Manufacturer = reader.IsDBNull(reader.GetOrdinal("Manufacturer")) ? "" : reader.GetString(reader.GetOrdinal("Manufacturer")),

								// FIX: Safely read the joined SupplierId column
								SupplierId = reader.IsDBNull(supplierIdOrdinal) ? 0 : reader.GetInt32(supplierIdOrdinal),
							});
						}
					}
				}
			}
			return medicines;
		}

		// ------------------------------------------
		// 2. FETCH ALL SUPPLIERS (Helper/Dropdown)
		// ------------------------------------------
		public async Task<List<Supplier>> GetAllSuppliersAsync()
		{
			var suppliers = new List<Supplier>();
			string sql = "SELECT SupplierId, Name FROM [Suppliers]";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							suppliers.Add(new Supplier
							{
								SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
								Name = reader.GetString(reader.GetOrdinal("Name")),
							});
						}
					}
				}
			}
			return suppliers;
		}


		// ------------------------------------------
		// 3. ADD MEDICINE (CREATE)
		// ------------------------------------------
		public async Task AddMedicineAsync(Medicines m)
		{
			string sql = @"
                INSERT INTO [Medicines] (Name, GenericName, Category, Quantity, MinQuantity, Price, Manufacturer, Batch, ExpiryDate, StorageLocation) 
                VALUES (@Name, @GenericName, @Category, @Quantity, @MinQuantity, @Price, @Manufacturer, @Batch, @ExpiryDate, @StorageLocation)";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@Name", m.Name);
					command.Parameters.AddWithValue("@GenericName", m.GenericName);
					command.Parameters.AddWithValue("@Category", m.Category);
					command.Parameters.AddWithValue("@Quantity", m.Quantity);
					command.Parameters.AddWithValue("@MinQuantity", m.MinQuantity);
					command.Parameters.AddWithValue("@Price", m.Price);
					command.Parameters.AddWithValue("@Manufacturer", m.Manufacturer);
					command.Parameters.AddWithValue("@Batch", m.Batch);
					command.Parameters.AddWithValue("@ExpiryDate", m.ExpiryDate);
					command.Parameters.AddWithValue("@StorageLocation", m.StorageLocation);
					await command.ExecuteNonQueryAsync();
				}
			}
		}

		// ------------------------------------------
		// 4. UPDATE MEDICINE (UPDATE)
		// ------------------------------------------
		public async Task UpdateMedicineAsync(Medicines m)
		{
			string sql = @"
                UPDATE [Medicines] SET 
                    Name = @Name, GenericName = @GenericName, Category = @Category, 
                    Quantity = @Quantity, MinQuantity = @MinQuantity, Price = @Price, 
                    Manufacturer = @Manufacturer, Batch = @Batch, ExpiryDate = @ExpiryDate, 
                    StorageLocation = @StorageLocation
                WHERE MedicineId = @MedicineId";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@MedicineId", m.MedicineId);
					command.Parameters.AddWithValue("@Name", m.Name);
					command.Parameters.AddWithValue("@GenericName", m.GenericName);
					command.Parameters.AddWithValue("@Category", m.Category);
					command.Parameters.AddWithValue("@Quantity", m.Quantity);
					command.Parameters.AddWithValue("@MinQuantity", m.MinQuantity);
					command.Parameters.AddWithValue("@Price", m.Price);
					command.Parameters.AddWithValue("@Manufacturer", m.Manufacturer);
					command.Parameters.AddWithValue("@Batch", m.Batch);
					command.Parameters.AddWithValue("@ExpiryDate", m.ExpiryDate);
					command.Parameters.AddWithValue("@StorageLocation", m.StorageLocation);
					await command.ExecuteNonQueryAsync();
				}
			}
		}

		// ------------------------------------------
		// 5. DELETE MEDICINE (DELETE)
		// ------------------------------------------
		public async Task DeleteMedicineAsync(int id)
		{
			string sql = "DELETE FROM [Medicines] WHERE MedicineId = @Id";

			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				using (var command = new SqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@Id", id);
					await command.ExecuteNonQueryAsync();
				}
			}
		}
	}
}