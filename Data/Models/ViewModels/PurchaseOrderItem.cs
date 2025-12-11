namespace Medicine_ERP.Data.Models.ViewModels.Dashboard;

public class PurchaseOrderItem
{
    public string PONumber { get; set; } = "";
    public string Supplier { get; set; } = "";
    public decimal Amount { get; set; } = 0m;
    public string Status { get; set; } = "pending"; 
}