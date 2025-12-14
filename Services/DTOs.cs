using System;

namespace PharmacyManagementSystem.Services
{


	public class InventoryItem
	{
		public int MedicineId { get; set; }
		public string Name { get; set; } = "";
		public string Category { get; set; } = "";
		public string Manufacturer { get; set; } = "";
		public int Quantity { get; set; }
		public int MinQuantity { get; set; }
		public decimal Price { get; set; } 
		public DateTime ExpiryDate { get; set; }
	}

	public class LowStockItem
	{
		public string Name { get; set; } = "";
		public int Quantity { get; set; }
		public int StockDifference { get; set; } 
	}

	public class StockInTransaction
	{
		public int MedicineId { get; set; }
		public int Quantity { get; set; }
		public DateTime DeliveryDate { get; set; } = DateTime.Today;
		public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(1);
	}

	public class StockOutTransaction
	{
		public int MedicineId { get; set; }
		public int Quantity { get; set; }
		public string Reason { get; set; } = "Damaged";
	}

	
}