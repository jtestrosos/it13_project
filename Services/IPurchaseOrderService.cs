using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PharmacyManagementSystem.Services
{
	// Fixes all 'PurchaseOrder' type not found errors (if this file exists)
	public class PurchaseOrders
	{
		public int PurchaseOrderId { get; set; }
		public int SupplierId { get; set; }
		public string SupplierName { get; set; } = "";
		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; } = "";

		public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
		public string OrderDateFormatted => OrderDate.ToString("yyyy-MM-dd");
	}

	public class PurchaseOrderItem
	{
		public int PurchaseOrderItemId { get; set; }
		public int MedicineId { get; set; }
		public string MedicineName { get; set; } = "";
		public int QuantityOrdered { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal LineTotal { get; set; }
	}

	// Fixes all 'IPurchaseOrderService' type not found errors
	public interface IPurchaseOrderService
	{
		Task<List<PurchaseOrders>> GetAllOrdersAsync();
		Task<PurchaseOrders> GetOrderDetailsAsync(int purchaseOrderId);
		Task<PurchaseStats> GetPurchaseStatsAsync(string period = "Monthly");
		Task CreateOrderAsync(PurchaseOrders order);
		Task UpdateOrderStatusAsync(int purchaseOrderId, string newStatus);
        Task<List<PurchaseItemReportDto>> GetPurchaseItemsReportAsync(DateTime startDate, DateTime endDate);
	}

    public class PurchaseItemReportDto
    {
        public string SupplierName { get; set; } = "";
        public string MedicineName { get; set; } = "";
        public decimal TotalCost { get; set; }
        public int Quantity { get; set; }
    }
}