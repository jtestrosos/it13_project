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
		public int Items { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal Profit { get; set; }
	}

	public class CategoryData
	{
		public string Category { get; set; } = "";
		public decimal Value { get; set; }
	}
}