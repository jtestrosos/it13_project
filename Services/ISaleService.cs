// PharmacyManagementSystem/Services/ISaleService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Medicine_ERP_Desktop.Components.Pages; // Assuming this contains required context

namespace PharmacyManagementSystem.Services
{
	public interface ISaleService
	{
		Task RecordSaleAsync(
			List<Sales.CartItem> cartItems,
			string paymentMethod,
			decimal totalAmount,
			decimal discountAmount,
			decimal amountPaid);

		Task<List<SaleRecord>> GetRecentSalesAsync(int limit = 20);

		// Assuming these DTOs are defined or accessible in the Services namespace for other reports
		Task<TopStats> GetAggregateStatsAsync(string period = "Monthly");
		Task<List<SaleReportEntry>> GetSalesReportsAsync(string period = "Monthly");
		Task<List<CategoryData>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);
	}

	// === SALE ITEM DETAIL DTO ===
	public class SaleItemRecord
	{
		public string MedicineName { get; set; } = string.Empty;
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal DiscountPct { get; set; }
		public decimal TotalAmount { get; set; } // Line Total
	}

	// === SALE HEADER DTO ===
	public class SaleRecord
	{
		public int SaleId { get; set; }
		public DateTime SaleDate { get; set; }
		public decimal TotalSaleAmount { get; set; } // Total for the entire transaction
		public List<SaleItemRecord> Items { get; set; } = new(); // Nested items
	}

	// NOTE: TopStats, SaleReportEntry, and CategoryData DTOs are assumed to be defined elsewhere in the service layer.
}