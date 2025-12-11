namespace Medicine_ERP.Data.Models.ViewModels.Dashboard;

public class AlertItem
{
    public string MedicineName { get; set; } = "";
    public string Details { get; set; } = "";    // e.g. "Stock: 8 (Min: 30)" or "Expires: 2025-12-15"
    public string Type { get; set; } = "Low Stock"; // "Low Stock" or "Expiring"
}