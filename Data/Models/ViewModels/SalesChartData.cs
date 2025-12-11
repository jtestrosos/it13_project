namespace Medicine_ERP.Data.Models.ViewModels.Dashboard;

public class SalesChartData
{
    public string Month { get; set; } = "";   // e.g. "Jan", "Feb", "Mar"
    public decimal Sales { get; set; } = 0m;
    public decimal Purchases { get; set; } = 0m;
}