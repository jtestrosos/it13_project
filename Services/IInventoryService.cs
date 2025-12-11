using PharmacyManagementSystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IInventoryService
{
	Task<List<InventoryItem>> GetAllInventoryAsync();

	Task<InventoryItem?> GetInventoryItemByIdAsync(int medicineId);

	Task<List<LowStockItem>> GetLowStockItemsAsync();

	Task<bool> UpdateStockQuantityAsync(int medicineId, int newQuantity);

	Task<bool> AddInventoryItemAsync(InventoryItem item);

	Task<bool> UpdateInventoryItemAsync(InventoryItem item);

	Task<bool> DeleteInventoryItemAsync(int medicineId);

	Task<InventoryStats> GetInventoryStatsAsync();
}