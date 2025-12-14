// PharmacyManagementSystem/Services/ISaleService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Medicine_ERP_Desktop.Components.Pages;

namespace PharmacyManagementSystem.Services
{
	public interface ISaleService
	{
		Task RecordSaleAsync(
			List<Medicine_ERP_Desktop.Components.Pages.Sales.CartItem> cartItems,
			string paymentMethod,
			decimal totalAmount,
			decimal discountAmount,
			decimal amountPaid);

		Task UpdateSaleAsync(
			int saleId,
			List<Medicine_ERP_Desktop.Components.Pages.Sales.CartItem> cartItems,
			string paymentMethod,
			decimal totalAmount,
			decimal discountAmount,
			decimal amountPaid);

		Task<List<SaleRecord>> GetRecentSalesAsync(int limit = 20);

		Task<TopStats> GetAggregateStatsAsync(string period = "Monthly");
		Task<List<SaleReportEntry>> GetSalesReportsAsync(string period = "Monthly");
		Task<List<CategoryData>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);
		Task<List<CategoryData>> GetSalesByPaymentMethodAsync(string period = "Monthly");
		Task<List<CategoryData>> GetTopSellingMedicinesAsync(string period = "Monthly");
		Task<bool> DeleteSaleAsync(int saleId);
		
		// Archive / Restore
		Task<List<SaleRecord>> GetArchivedSalesAsync();
		Task RestoreSaleAsync(int saleId);
	}
}