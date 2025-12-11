namespace PharmERP.Models
{
	public class DashboardDataModel
	{
		public int TotalMedicines { get; set; }
		public int LowStockItems { get; set; }
		public int ExpiringSoonItems { get; set; }
		public decimal DailySales { get; set; }
		public List<LowStockAlert> LowStockAlerts { get; set; } = new List<LowStockAlert>();
		public List<PurchaseOrder> RecentOrders { get; set; } = new List<PurchaseOrder>();
	}

	public class LowStockAlert
	{
		public string Name { get; set; }
		public int CurrentStock { get; set; }
		public int MinStock { get; set; }
	}

	public class PurchaseOrder
	{
		public string Id { get; set; }
		public string Supplier { get; set; }
		public DateTime OrderDate { get; set; }
		public string Status { get; set	; }
	}
}