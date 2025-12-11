using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;


namespace Medicine_ERP.Data.Models.ViewModels
{
	// === MEDICINE MODEL ===
	public class Medicine
	{
		public int MedicineId { get; set; } = 0;
		public string Name { get; set; } = "";
		public string GenericName { get; set; } = "";
		public string Category { get; set; } = "Antibiotics";
		public int Quantity { get; set; }
		public int MinQuantity { get; set; } = 100;
		public decimal Price { get; set; }
		public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(2);
		public string Batch { get; set; } = "";
		public string Manufacturer { get; set; } = "";
		public string StorageLocation { get; set; } = "";
		public int SupplierId { get; set; } = 0; 
	}

	// === SUPPLIER MODEL ===
	public class Supplier
	{
		public int SupplierId { get; set; }
		public string Name { get; set; } = "";
		public string ContactPerson { get; set; } = "";
		public string Email { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";
		public string City { get; set; } = "";
		public string Country { get; set; } = "";
	}
}

// --- NAMESPACE FOR SERVICES ---
namespace PharmERP.Data
{
	// === IMedicineService INTERFACE ===
	public interface IMedicineService
	{
		Task<IEnumerable<Medicine_ERP.Data.Models.ViewModels.Medicine>> GetAllMedicinesAsync();
		Task AddMedicineAsync(Medicine_ERP.Data.Models.ViewModels.Medicine medicine);
		Task UpdateMedicineAsync(Medicine_ERP.Data.Models.ViewModels.Medicine medicine);
		Task DeleteMedicineAsync(int medicineId);
		Task<IEnumerable<Medicine_ERP.Data.Models.ViewModels.Supplier>> GetAllSuppliersAsync();
	}

	// === MedicineService IMPLEMENTATION ===
	public class MedicineService : IMedicineService
	{
		private readonly string _connectionString;

		public MedicineService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		}

		private async Task<SqlConnection> GetConnection() => new SqlConnection(_connectionString);

		public async Task<IEnumerable<Medicine_ERP.Data.Models.ViewModels.Medicine>> GetAllMedicinesAsync()
		{
			using var db = await GetConnection();
			var sql = @"SELECT 
                            M.MedicineId, M.Name, M.GenericName, M.Category, M.Quantity, M.MinQuantity, M.Price, 
                            M.Manufacturer, M.Batch, M.ExpiryDate, M.StorageLocation,
                            ISNULL(S.SupplierId, 0) AS SupplierId 
                        FROM 
                            dbo.Medicines M
                        LEFT JOIN 
                            dbo.Suppliers S ON M.Manufacturer = S.Name";
			return await db.QueryAsync<Medicine_ERP.Data.Models.ViewModels.Medicine>(sql);
		}

		public async Task<IEnumerable<Medicine_ERP.Data.Models.ViewModels.Supplier>> GetAllSuppliersAsync()
		{
			using var db = await GetConnection();
			var sql = @"SELECT SupplierId, Name, ContactPerson, Email, Phone, Address, City, Country 
                         FROM dbo.Suppliers WHERE IsActive = 1";
			return await db.QueryAsync<Medicine_ERP.Data.Models.ViewModels.Supplier>(sql);
		}

		public async Task AddMedicineAsync(Medicine_ERP.Data.Models.ViewModels.Medicine medicine)
		{
			using var db = await GetConnection();
			var sql = @"INSERT INTO dbo.Medicines (Name, GenericName, Category, Quantity, MinQuantity, Price, 
                        Manufacturer, Batch, ExpiryDate, StorageLocation)
                        VALUES (@Name, @GenericName, @Category, @Quantity, @MinQuantity, @Price, 
                        @Manufacturer, @Batch, @ExpiryDate, @StorageLocation)";
			await db.ExecuteAsync(sql, medicine);
		}

		public async Task UpdateMedicineAsync(Medicine_ERP.Data.Models.ViewModels.Medicine medicine)
		{
			using var db = await GetConnection();
			var sql = @"UPDATE dbo.Medicines SET 
                        Name = @Name, GenericName = @GenericName, Category = @Category, 
                        Quantity = @Quantity, MinQuantity = @MinQuantity, Price = @Price, 
                        Manufacturer = @Manufacturer, Batch = @Batch, ExpiryDate = @ExpiryDate, 
                        StorageLocation = @StorageLocation
                        WHERE MedicineId = @MedicineId";
			await db.ExecuteAsync(sql, medicine);
		}

		public async Task DeleteMedicineAsync(int medicineId)
		{
			using var db = await GetConnection();
			await db.ExecuteAsync("DELETE FROM dbo.Medicines WHERE MedicineId = @MedicineId", new { MedicineId = medicineId });
		}
	}
}