using PharmacyManagementSystem.Services;
using static Medicine_ERP_Desktop.Components.Pages.Reports;

public interface IPurchaseService
{
	// Fetches purchase order data within a specified time period
	// Joins PurchaseOrders, PurchaseOrderItems, and Supplier (if you have a Suppliers table)
	Task<List<PurchaseReportEntry>> GetPurchaseReportsAsync(string period);
}