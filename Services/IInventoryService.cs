using PharmacyManagementSystem.Services;

public interface IInventoryService
{
	
	Task<List<InventoryService>> GetAllInventoryAsync();

	Task<InventoryService?> GetInventoryItemByIdAsync(int medicineId);


	
	Task<List<LowStockItem>> GetLowStockItemsAsync();

	
	Task<bool> UpdateStockQuantityAsync(int medicineId, int newQuantity);

	
	Task<bool> AddInventoryItemAsync(InventoryService item);


	Task<bool> UpdateInventoryItemAsync(InventoryService item);

	Task<bool> DeleteInventoryItemAsync(int medicineId);
}