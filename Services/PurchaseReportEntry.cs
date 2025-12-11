namespace PharmacyManagementSystem.Services
{
	// --- Purchase Model ---

	// Maps to the PurchaseOrders table, possibly joined with Supplier name
	public class PurchaseReportEntry
	{
		public int PurchaseOrderId { get; set; }
		public string SupplierName { get; set; } = "N/A";
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; } = "";
	}
}