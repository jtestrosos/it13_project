// PharmacyManagementSystem/Services/ReportModels.cs
using System;
using System.Collections.Generic;

namespace PharmacyManagementSystem.Services
{
	public class TopStats
	{
		public decimal TotalRevenue { get; set; }
		public decimal TotalProfit { get; set; }
		public int ItemsSold { get; set; }
		public int TotalTransactions { get; set; }
        public int TotalMedicines { get; internal set; }
        public int LowStockItems { get; internal set; }
        public int ExpiringSoon { get; internal set; }
        public decimal DailySales { get; internal set; }
    }

	public class SaleReportEntry
	{
		public DateTime SaleDate { get; set; }
		public string CustomerName { get; set; } = "";
		public string PaymentMethod { get; set; } = "";
		public int Items { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal Profit { get; set; }
	}

	public class CategoryData
	{
		public string Category { get; set; } = "";
		public decimal Value { get; set; }
	}

	public class InventoryStats
	{
		public int TotalItems { get; set; }
		public decimal TotalValue { get; set; }
		public int LowStockCount { get; set; }
		public int ExpiredCount { get; set; }
	}

	public class PurchaseStats
	{
		public int TotalPurchases { get; set; }
		public decimal TotalSpent { get; set; }
		public int PendingOrders { get; set; }
		public int CompletedOrders { get; set; }
	}
}