using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PharmacyManagementSystem.Services
{
	// === MEDICINE MODEL (Full Definition for Inventory) ===
	public class Medicines
	{
		public int MedicineId { get; set; }
		public string Name { get; set; } = "";
		public string GenericName { get; set; } = "";
		public string Category { get; set; } = "";
		public decimal Price { get; set; }

		public int Quantity { get; set; }
		public int MinQuantity { get; set; }
		public DateTime ExpiryDate { get; set; }
		public string Batch { get; set; } = "";
		public string StorageLocation { get; set; } = "";

		public string Manufacturer { get; set; } = "";
		public int SupplierId { get; set; }
	}

	public interface IMedicineService
	{
		// Data Fetching
		Task<List<Medicines>> GetAllMedicinesAsync();

		// CRUD Operations (Required by Inventory.razor)
		Task AddMedicineAsync(Medicines medicine);
		Task UpdateMedicineAsync(Medicines medicine);
		Task DeleteMedicineAsync(int id);


		// Helper method (Required by Inventory.razor for the modal)
		Task<List<Supplier>> GetAllSuppliersAsync();
		
		// Stock Management
		Task AddStockInAsync(StockInTransaction transaction);
		Task AddStockOutAsync(StockOutTransaction transaction);
		
		// Archive / Restore
		Task<List<Medicines>> GetArchivedMedicinesAsync();
		Task RestoreMedicineAsync(int id);
	}
}