namespace Medicine_ERP.Data.Models.ViewModels.Dashboard;

public class CategoryData
{
    public string Category { get; set; } = "";   // e.g. "Antibiotics", "Painkillers"
    public int Percentage { get; set; } = 0;     // 0 to 100
}